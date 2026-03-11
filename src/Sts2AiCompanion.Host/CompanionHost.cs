using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Host;

public sealed class CompanionHost : IAsyncDisposable
{
    private readonly ScaffoldConfiguration _configuration;
    private readonly string _workspaceRoot;
    private readonly LiveExportLayout _layout;
    private readonly KnowledgeCatalogService _knowledgeCatalogService;
    private readonly AdvicePromptBuilder _promptBuilder;
    private readonly ICodexSessionClient _codexSessionClient;
    private readonly SemaphoreSlim _adviceLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;
    private long _lastObservedSeq;
    private DateTimeOffset? _lastAdviceAt;
    private string? _currentRunId;
    private AdviceResponse? _latestAdvice;
    private KnowledgeSlice? _latestKnowledgeSlice;
    private CompanionRunState? _currentRunState;
    private CodexSessionState? _sessionState;
    private bool _autoAdviceEnabled;

    public CompanionHost(ScaffoldConfiguration configuration, string workspaceRoot, ICodexSessionClient? codexSessionClient = null)
    {
        _configuration = configuration;
        _workspaceRoot = workspaceRoot;
        _layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        _knowledgeCatalogService = new KnowledgeCatalogService(configuration, workspaceRoot);
        _promptBuilder = new AdvicePromptBuilder(configuration);
        _codexSessionClient = codexSessionClient ?? new CodexCliClient(configuration, workspaceRoot);
        _autoAdviceEnabled = configuration.Assistant.AutoAdviceEnabled;
        CurrentSnapshot = CreateSnapshot("idle", "Waiting for live export.");
    }

    public event EventHandler<CompanionHostSnapshot>? SnapshotChanged;

    public CompanionHostSnapshot CurrentSnapshot { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_loopTask is not null)
        {
            return;
        }

        _loopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loopTask = Task.Run(() => RunLoopAsync(_loopCts.Token), _loopCts.Token);
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        return PollOnceAsync(cancellationToken);
    }

    public async Task StopAsync()
    {
        if (_loopCts is null || _loopTask is null)
        {
            return;
        }

        _loopCts.Cancel();
        try
        {
            await _loopTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _loopTask = null;
            _loopCts.Dispose();
            _loopCts = null;
        }
    }

    public async Task<bool> RequestManualAdviceAsync(CancellationToken cancellationToken = default)
    {
        if (_currentRunState is null)
        {
            return false;
        }

        var trigger = new AdviceTrigger("manual", DateTimeOffset.UtcNow, true, true, "manual-trigger", null);
        await GenerateAdviceAsync(_currentRunState, trigger, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public void SetAutoAdviceEnabled(bool enabled)
    {
        _autoAdviceEnabled = enabled;
        PublishSnapshot(CreateSnapshot("running", enabled ? "Automatic advice is enabled." : "Automatic advice is paused."));
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _adviceLock.Dispose();
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Math.Max(_configuration.Assistant.LivePollIntervalMs, 250)));
        do
        {
            await PollOnceAsync(cancellationToken).ConfigureAwait(false);
        }
        while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false));
    }

    private async Task PollOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = TryReadJson<LiveExportSnapshot>(_layout.SnapshotPath);
            var session = TryReadJson<LiveExportSession>(_layout.SessionPath);
            var summary = TryReadAllText(_layout.SummaryPath);
            var events = ReadNewEvents(_layout.EventsPath, ref _lastObservedSeq, _configuration.Assistant.RecentEventsCount);

            if (snapshot is null)
            {
                PublishSnapshot(CreateSnapshot("waiting-live-export", "state.latest.json is not available yet."));
                return;
            }

            var recentEvents = (_currentRunState?.RecentEvents ?? Array.Empty<LiveExportEventEnvelope>())
                .Concat(events)
                .TakeLast(_configuration.Assistant.RecentEventsCount)
                .ToArray();
            var isStale = DateTimeOffset.UtcNow - snapshot.CapturedAt > TimeSpan.FromSeconds(10);
            var runState = new CompanionRunState(snapshot, session, summary ?? string.Empty, recentEvents, isStale);

            if (!string.Equals(_currentRunId, snapshot.RunId, StringComparison.Ordinal))
            {
                _currentRunId = snapshot.RunId;
                _sessionState = null;
                _latestAdvice = null;
            }

            _currentRunState = runState;
            _latestKnowledgeSlice = _knowledgeCatalogService.BuildSlice(
                runState,
                _configuration.Assistant.MaxKnowledgeEntries,
                _configuration.Assistant.MaxKnowledgeBytes);

            var autoTrigger = _autoAdviceEnabled
                ? DetermineAutomaticTrigger(events)
                : null;
            if (autoTrigger is not null)
            {
                await GenerateAdviceAsync(runState, autoTrigger, cancellationToken).ConfigureAwait(false);
            }

            MirrorLiveArtifacts(runState);
            PublishSnapshot(CreateSnapshot("running", "Monitoring live export updates."));
        }
        catch (Exception exception)
        {
            PublishSnapshot(CreateSnapshot("error", exception.Message));
        }
    }

    private AdviceTrigger? DetermineAutomaticTrigger(IReadOnlyList<LiveExportEventEnvelope> events)
    {
        AdviceTrigger? selected = null;
        foreach (var envelope in events)
        {
            var decision = LiveExportSummaryFormatter.EvaluateCodexTrigger(
                new LiveExportObservation(
                    envelope.Kind,
                    envelope.Ts,
                    envelope.RunId,
                    null,
                    envelope.Screen,
                    envelope.Act,
                    envelope.Floor,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    envelope.Payload,
                    new Dictionary<string, string?>()),
                new LiveExportTriggerWindow(_lastAdviceAt, TimeSpan.FromMilliseconds(_configuration.Assistant.MinAdviceIntervalMs)));

            if (decision.ShouldTriggerCodex)
            {
                selected = new AdviceTrigger(envelope.Kind, envelope.Ts, false, decision.BypassMinInterval, decision.Reason, envelope);
            }
        }

        return selected;
    }

    private async Task GenerateAdviceAsync(CompanionRunState runState, AdviceTrigger trigger, CancellationToken cancellationToken)
    {
        var acquired = trigger.Manual
            ? await _adviceLock.WaitAsync(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false)
            : await _adviceLock.WaitAsync(TimeSpan.Zero, cancellationToken).ConfigureAwait(false);
        if (!acquired)
        {
            return;
        }

        try
        {
            _latestKnowledgeSlice ??= _knowledgeCatalogService.BuildSlice(
                runState,
                _configuration.Assistant.MaxKnowledgeEntries,
                _configuration.Assistant.MaxKnowledgeBytes);

            var inputPack = _promptBuilder.BuildInputPack(runState, trigger, _latestKnowledgeSlice);
            var prompt = _promptBuilder.FormatPrompt(inputPack);
            var paths = EnsureRunArtifacts(runState.Snapshot.RunId);
            var promptPath = Path.Combine(paths.PromptPacksRoot!, $"{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}-{trigger.Kind}.json");
            WriteJson(promptPath, inputPack);

            var (response, sessionId) = await _codexSessionClient.ExecuteAsync(inputPack, prompt, _sessionState?.SessionId, cancellationToken).ConfigureAwait(false);
            _latestAdvice = response;
            _lastAdviceAt = response.GeneratedAt;
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                _sessionState = new CodexSessionState(
                    runState.Snapshot.RunId,
                    sessionId!,
                    _sessionState?.CreatedAt ?? DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow);
            }

            WriteJson(paths.AdviceLatestJsonPath!, response);
            File.WriteAllText(paths.AdviceLatestMarkdownPath!, _promptBuilder.FormatAdviceMarkdown(response));
            AppendNdjson(paths.AdviceLogPath!, response);
            if (_sessionState is not null)
            {
                WriteJson(paths.CodexSessionPath!, _sessionState);
            }

            MirrorLiveArtifacts(runState);

            PublishSnapshot(CreateSnapshot("running", $"Advice generated for {trigger.Kind}."));
        }
        finally
        {
            _adviceLock.Release();
        }
    }

    private void MirrorLiveArtifacts(CompanionRunState runState)
    {
        var paths = EnsureRunArtifacts(runState.Snapshot.RunId);
        if (paths.LiveMirrorRoot is null)
        {
            return;
        }

        Directory.CreateDirectory(paths.LiveMirrorRoot);
        CopyIfExists(_layout.SnapshotPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.SnapshotPath)));
        CopyIfExists(_layout.SummaryPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.SummaryPath)));
        CopyIfExists(_layout.SessionPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.SessionPath)));
        CopyIfExists(_layout.EventsPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.EventsPath)));
        WriteJson(paths.CurrentRunStatePath, new
        {
            runId = runState.Snapshot.RunId,
            screen = runState.Snapshot.CurrentScreen,
            capturedAt = runState.Snapshot.CapturedAt,
            liveRoot = _layout.LiveRoot,
            sessionId = _sessionState?.SessionId,
        });
    }

    private CompanionArtifactPaths EnsureRunArtifacts(string runId)
    {
        var paths = CompanionPathResolver.Resolve(_configuration, _workspaceRoot, runId);
        Directory.CreateDirectory(paths.CompanionRoot);
        if (paths.RunRoot is not null)
        {
            Directory.CreateDirectory(paths.RunRoot);
        }

        if (paths.LiveMirrorRoot is not null)
        {
            Directory.CreateDirectory(paths.LiveMirrorRoot);
        }

        if (paths.PromptPacksRoot is not null)
        {
            Directory.CreateDirectory(paths.PromptPacksRoot);
        }

        return paths;
    }

    private CompanionHostSnapshot CreateSnapshot(string state, string message)
    {
        var paths = CompanionPathResolver.Resolve(_configuration, _workspaceRoot, _currentRunId);
        var status = new CompanionHostStatus(
            state,
            _currentRunState is not null,
            true,
            _autoAdviceEnabled,
            _currentRunId,
            DateTimeOffset.UtcNow,
            _lastAdviceAt,
            message);

        WriteJson(paths.HostStatusPath, status);
        return new CompanionHostSnapshot(status, _currentRunState, _latestAdvice, _latestKnowledgeSlice, paths);
    }

    private void PublishSnapshot(CompanionHostSnapshot snapshot)
    {
        CurrentSnapshot = snapshot;
        SnapshotChanged?.Invoke(this, snapshot);
    }

    private static T? TryReadJson<T>(string path)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        return JsonSerializer.Deserialize<T>(stream, ConfigurationLoader.JsonOptions);
    }

    private static string? TryReadAllText(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static IReadOnlyList<LiveExportEventEnvelope> ReadNewEvents(string path, ref long lastObservedSeq, int tailCount)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<LiveExportEventEnvelope>();
        }

        var results = new List<LiveExportEventEnvelope>();
        foreach (var line in ReadAllLinesShared(path))
        {
            var envelope = JsonSerializer.Deserialize<LiveExportEventEnvelope>(line, ConfigurationLoader.JsonOptions);
            if (envelope is null || envelope.Seq <= lastObservedSeq)
            {
                continue;
            }

            results.Add(envelope);
            lastObservedSeq = Math.Max(lastObservedSeq, envelope.Seq);
        }

        return results.TakeLast(tailCount).ToArray();
    }

    private static void CopyIfExists(string sourcePath, string destinationPath)
    {
        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, destinationPath, overwrite: true);
        }
    }

    private static IReadOnlyList<string> ReadAllLinesShared(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd()
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .ToArray();
    }

    private void WriteJson<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(value, _jsonOptions));
    }

    private void AppendNdjson<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.AppendAllText(path, JsonSerializer.Serialize(value, _jsonOptions) + Environment.NewLine);
    }
}
