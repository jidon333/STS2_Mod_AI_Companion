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

public sealed partial class CompanionHost : IAsyncDisposable
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
    private readonly JsonSerializerOptions _ndjsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
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
    private CompanionCollectorStatus? _latestCollectorStatus;
    private bool _autoAdviceEnabled;
    private string? _selectedModel;
    private string? _selectedReasoningEffort;
    private DateTimeOffset? _analysisStartedAt;
    private string? _analysisTriggerKind;
    private string? _analysisMessage;

    public CompanionHost(ScaffoldConfiguration configuration, string workspaceRoot, ICodexSessionClient? codexSessionClient = null)
    {
        _configuration = configuration;
        _workspaceRoot = workspaceRoot;
        _layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        _knowledgeCatalogService = new KnowledgeCatalogService(configuration, workspaceRoot);
        _promptBuilder = new AdvicePromptBuilder(configuration);
        _codexSessionClient = codexSessionClient ?? new CodexCliClient(configuration, workspaceRoot);
        _autoAdviceEnabled = configuration.Assistant.AutoAdviceEnabled;
        _selectedModel = configuration.Assistant.CodexModel;
        _selectedReasoningEffort = configuration.Assistant.CodexReasoningEffort;
        CurrentSnapshot = CreateSnapshot("idle", "실시간 추출을 기다리는 중입니다.");
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
        PublishSnapshot(CreateSnapshot("running", enabled ? "자동 조언이 켜져 있습니다." : "자동 조언이 일시중지되었습니다."));
    }

    public void SetSelectedModel(string? model)
    {
        _selectedModel = string.IsNullOrWhiteSpace(model) ? null : model.Trim();
        PublishSnapshot(CreateSnapshot("running", _selectedModel is null
            ? "기본 Codex 모델을 사용합니다."
            : $"Codex 모델을 {_selectedModel}로 설정했습니다."));
    }

    public void SetSelectedReasoningEffort(string? reasoningEffort)
    {
        _selectedReasoningEffort = string.IsNullOrWhiteSpace(reasoningEffort) ? null : reasoningEffort.Trim();
        PublishSnapshot(CreateSnapshot("running", _selectedReasoningEffort is null
            ? "기본 추론 강도를 사용합니다."
            : $"Codex 추론 강도를 {_selectedReasoningEffort}로 설정했습니다."));
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
                PublishSnapshot(CreateSnapshot("waiting-live-export", "아직 state.latest.json이 생성되지 않았습니다."));
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
                _sessionState = TryReadExistingSessionState(snapshot.RunId);
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

            UpdateCollectorArtifacts(runState);
            PublishSnapshot(CreateSnapshot("running", "실시간 추출 갱신을 감시 중입니다."));
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
            BeginAnalysis(trigger);
            _latestKnowledgeSlice ??= _knowledgeCatalogService.BuildSlice(
                runState,
                _configuration.Assistant.MaxKnowledgeEntries,
                _configuration.Assistant.MaxKnowledgeBytes);

            var inputPack = _promptBuilder.BuildInputPack(runState, trigger, _latestKnowledgeSlice);
            var prompt = _promptBuilder.FormatPrompt(inputPack);
            var paths = EnsureRunArtifacts(runState.Snapshot.RunId);
            var promptPath = Path.Combine(paths.PromptPacksRoot!, $"{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}-{trigger.Kind}.json");
            WriteJson(promptPath, inputPack);
            WriteCodexTrace(paths, new
            {
                kind = "request-started",
                trigger = trigger.Kind,
                manual = trigger.Manual,
                requestedAt = trigger.RequestedAt,
                existingSessionId = _sessionState?.SessionId,
            });

            var (response, sessionId) = await _codexSessionClient.ExecuteAsync(
                inputPack,
                prompt,
                _sessionState?.SessionId,
                _selectedModel,
                _selectedReasoningEffort,
                cancellationToken).ConfigureAwait(false);
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
            WriteCodexTrace(paths, new
            {
                kind = "request-finished",
                trigger = trigger.Kind,
                manual = trigger.Manual,
                status = response.Status,
                generatedAt = response.GeneratedAt,
                sessionId = _sessionState?.SessionId ?? sessionId,
                missingInformation = response.MissingInformation,
                decisionBlockers = response.DecisionBlockers,
            });

            UpdateCollectorArtifacts(runState);
            EndAnalysis();
            PublishSnapshot(CreateSnapshot("running", $"조언 생성 완료: {trigger.Kind}"));
        }
        catch
        {
            EndAnalysis();
            throw;
        }
        finally
        {
            _adviceLock.Release();
        }
    }

    private void UpdateCollectorArtifacts(CompanionRunState runState)
    {
        var paths = EnsureRunArtifacts(runState.Snapshot.RunId);
        MirrorLiveArtifacts(runState, paths);
        _latestCollectorStatus = BuildCollectorStatus(runState, paths);
        if (_configuration.LiveExport.CollectorModeEnabled && paths.CollectorSummaryPath is not null)
        {
            WriteJson(paths.CollectorSummaryPath, BuildCollectorSummary(runState, paths, _latestCollectorStatus));
        }
    }

    private void MirrorLiveArtifacts(CompanionRunState runState, CompanionArtifactPaths paths)
    {
        if (paths.LiveMirrorRoot is null)
        {
            return;
        }

        Directory.CreateDirectory(paths.LiveMirrorRoot);
        CopyIfExists(_layout.SnapshotPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.SnapshotPath)));
        CopyIfExists(_layout.SummaryPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.SummaryPath)));
        CopyIfExists(_layout.SessionPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.SessionPath)));
        CopyIfExists(_layout.EventsPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.EventsPath)));
        CopyIfExists(_layout.RawObservationsPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.RawObservationsPath)));
        CopyIfExists(_layout.ScreenTransitionsPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.ScreenTransitionsPath)));
        CopyIfExists(_layout.ChoiceCandidatesPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.ChoiceCandidatesPath)));
        CopyIfExists(_layout.ChoiceDecisionsPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.ChoiceDecisionsPath)));
        CopyDirectoryIfExists(_layout.SemanticSnapshotsRoot, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.SemanticSnapshotsRoot)));
        WriteJson(paths.CurrentRunStatePath, new
        {
            runId = runState.Snapshot.RunId,
            screen = runState.Snapshot.CurrentScreen,
            capturedAt = runState.Snapshot.CapturedAt,
            liveRoot = _layout.LiveRoot,
            sessionId = _sessionState?.SessionId,
            collectorModeEnabled = _configuration.LiveExport.CollectorModeEnabled,
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

        if (paths.AdviceRoot is not null)
        {
            Directory.CreateDirectory(paths.AdviceRoot);
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
            _analysisStartedAt is not null,
            _currentRunId,
            _selectedModel,
            _selectedReasoningEffort,
            _analysisTriggerKind,
            _analysisStartedAt,
            DateTimeOffset.UtcNow,
            _lastAdviceAt,
            _analysisMessage ?? message);

        WriteJson(paths.HostStatusPath, status);
        return new CompanionHostSnapshot(status, _currentRunState, _latestAdvice, _latestKnowledgeSlice, _latestCollectorStatus, paths);
    }

    private void BeginAnalysis(AdviceTrigger trigger)
    {
        _analysisStartedAt = DateTimeOffset.UtcNow;
        _analysisTriggerKind = trigger.Kind;
        _analysisMessage = $"AI 분석 중: {trigger.Kind}";
        PublishSnapshot(CreateSnapshot("analyzing", _analysisMessage));
    }

    private void EndAnalysis()
    {
        _analysisStartedAt = null;
        _analysisTriggerKind = null;
        _analysisMessage = null;
    }

    private void PublishSnapshot(CompanionHostSnapshot snapshot)
    {
        CurrentSnapshot = snapshot;
        SnapshotChanged?.Invoke(this, snapshot);
    }

    private CodexSessionState? TryReadExistingSessionState(string runId)
    {
        var paths = CompanionPathResolver.Resolve(_configuration, _workspaceRoot, runId);
        if (string.IsNullOrWhiteSpace(paths.CodexSessionPath) || !File.Exists(paths.CodexSessionPath))
        {
            return null;
        }

        try
        {
            return TryReadJson<CodexSessionState>(paths.CodexSessionPath);
        }
        catch
        {
            return null;
        }
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

    private CompanionCollectorStatus BuildCollectorStatus(CompanionRunState runState, CompanionArtifactPaths paths)
    {
        if (!_configuration.LiveExport.CollectorModeEnabled)
        {
            return new CompanionCollectorStatus(false, null, null, null, null, null, _sessionState?.SessionId, "collector mode disabled");
        }

        var latestDecision = ReadLastJsonObject(paths.LiveMirrorRoot, Path.GetFileName(_layout.ChoiceDecisionsPath));
        var lastSemanticScreen = runState.RecentEvents
            .LastOrDefault(IsSemanticEvent)?.Screen;
        var activeEpisode = IsHighValueScreen(runState.Snapshot.CurrentScreen)
            ? runState.Snapshot.CurrentScreen
            : lastSemanticScreen;
        var acceptedExtractorPath = TryReadNestedString(latestDecision, "decision", "extractorPath");
        var acceptedCount = TryReadNestedInt(latestDecision, "decision", "acceptedCount");
        var failureReason = TryReadNestedString(latestDecision, "decision", "failureReason");
        var choiceStatus = acceptedCount > 0
            ? $"resolved ({acceptedCount})"
            : runState.Snapshot.CurrentChoices.Count > 0
                ? $"resolved ({runState.Snapshot.CurrentChoices.Count})"
                : latestDecision is not null
                    ? "missing"
                    : "not-seen";
        var degradedReason = failureReason
                             ?? _latestAdvice?.DecisionBlockers.FirstOrDefault()
                             ?? _latestAdvice?.MissingInformation.FirstOrDefault()
                             ?? (_latestAdvice is { Status: not "ok" } ? _latestAdvice.Summary : null);
        var notes = string.Join(
            Environment.NewLine,
            new[]
            {
                $"collector mode: on",
                $"latest semantic screen: {lastSemanticScreen ?? "none"}",
                $"active screen episode: {activeEpisode ?? "none"}",
                $"choice extraction: {choiceStatus}",
                $"extractor path: {acceptedExtractorPath ?? "none"}",
                $"last degraded reason: {degradedReason ?? "none"}",
                $"missing information: {(_latestAdvice?.MissingInformation.Count > 0 ? string.Join(", ", _latestAdvice.MissingInformation.Take(4)) : "none")}",
                $"decision blockers: {(_latestAdvice?.DecisionBlockers.Count > 0 ? string.Join(", ", _latestAdvice.DecisionBlockers.Take(4)) : "none")}",
                $"session id: {_sessionState?.SessionId ?? "none"}",
            });

        return new CompanionCollectorStatus(
            true,
            activeEpisode,
            lastSemanticScreen,
            choiceStatus,
            acceptedExtractorPath,
            degradedReason,
            _sessionState?.SessionId,
            notes);
    }

    private CompanionCollectorSummary BuildCollectorSummary(
        CompanionRunState runState,
        CompanionArtifactPaths paths,
        CompanionCollectorStatus? collectorStatus)
    {
        var liveMirrorRoot = paths.LiveMirrorRoot;
        var transitions = ReadJsonLines(liveMirrorRoot, Path.GetFileName(_layout.ScreenTransitionsPath));
        var decisions = ReadJsonLines(liveMirrorRoot, Path.GetFileName(_layout.ChoiceDecisionsPath));
        var candidates = ReadJsonLines(liveMirrorRoot, Path.GetFileName(_layout.ChoiceCandidatesPath));
        var adviceLog = paths.AdviceLogPath is not null && File.Exists(paths.AdviceLogPath)
            ? ReadJsonLines(paths.AdviceLogPath)
            : Array.Empty<JsonDocument>();

        var timeline = transitions
            .Select(document =>
            {
                var root = document.RootElement;
                var before = TryReadString(root, "before") ?? "unknown";
                var incoming = TryReadString(root, "incoming") ?? "unknown";
                var after = TryReadString(root, "after") ?? "unknown";
                var trigger = TryReadString(root, "triggerKind") ?? "unknown";
                return $"{trigger}: {before} -> {after} (incoming={incoming})";
            })
            .TakeLast(24)
            .ToArray();

        var semanticCounts = runState.RecentEvents
            .Where(IsSemanticEvent)
            .GroupBy(evt => evt.Kind, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

        var missingChoices = decisions
            .Where(document => (TryReadNestedInt(document, "decision", "acceptedCount") ?? 0) == 0)
            .Select(document =>
            {
                var screen = TryReadString(document.RootElement, "screen") ?? "unknown";
                var extractor = TryReadNestedString(document, "decision", "extractorPath") ?? "unknown";
                return $"{screen} via {extractor}";
            })
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var placeholderLabels = candidates
            .Where(document => string.Equals(TryReadNestedString(document, "candidate", "rejectReason"), "placeholder-label", StringComparison.Ordinal))
            .Select(document => TryReadNestedString(document, "candidate", "label"))
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(label => label, StringComparer.Ordinal)
            .Take(64)
            .ToArray();

        var autoAdviceFailures = adviceLog
            .Where(document =>
            {
                var root = document.RootElement;
                var status = TryReadString(root, "status");
                var trigger = TryReadString(root, "triggerKind");
                return !string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase)
                       && !string.Equals(trigger, "manual", StringComparison.OrdinalIgnoreCase);
            })
            .Select(document =>
            {
                var root = document.RootElement;
                return $"{TryReadString(root, "triggerKind") ?? "unknown"}: {TryReadString(root, "summary") ?? "degraded"}";
            })
            .TakeLast(12)
            .ToArray();

        var missingInformationObserved = adviceLog
            .SelectMany(document => ReadStringArray(document.RootElement, "missingInformation"))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .Take(64)
            .ToArray();

        var decisionBlockersObserved = adviceLog
            .SelectMany(document => ReadStringArray(document.RootElement, "decisionBlockers"))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .Take(64)
            .ToArray();

        var observedMergeCounts = ReadObservedMergeCounts();
        var recommendedFixes = BuildRecommendedFixes(
            missingChoices,
            placeholderLabels,
            autoAdviceFailures,
            missingInformationObserved,
            decisionBlockersObserved,
            collectorStatus);

        return new CompanionCollectorSummary(
            runState.Snapshot.RunId,
            DateTimeOffset.UtcNow,
            timeline,
            semanticCounts,
            missingChoices,
            placeholderLabels,
            autoAdviceFailures,
            missingInformationObserved,
            decisionBlockersObserved,
            collectorStatus?.SessionId is null ? "missing-session-id" : "session-tracked",
            observedMergeCounts,
            recommendedFixes);
    }

    private static void CopyIfExists(string sourcePath, string destinationPath)
    {
        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, destinationPath, overwrite: true);
        }
    }

    private static void CopyDirectoryIfExists(string sourcePath, string destinationPath)
    {
        if (!Directory.Exists(sourcePath))
        {
            return;
        }

        Directory.CreateDirectory(destinationPath);
        foreach (var file in Directory.GetFiles(sourcePath))
        {
            File.Copy(file, Path.Combine(destinationPath, Path.GetFileName(file)), overwrite: true);
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
        File.AppendAllText(path, JsonSerializer.Serialize(value, _ndjsonOptions) + Environment.NewLine);
    }

    private void WriteCodexTrace(CompanionArtifactPaths paths, object trace)
    {
        if (!_configuration.LiveExport.CollectorModeEnabled || paths.CodexTracePath is null)
        {
            return;
        }

        AppendNdjson(paths.CodexTracePath, trace);
    }

    private IReadOnlyDictionary<string, int> ReadObservedMergeCounts()
    {
        var observedMergePath = Path.Combine(CompanionPathResolver.ResolveKnowledgeRoot(_configuration, _workspaceRoot), "observed-merge.json");
        if (!File.Exists(observedMergePath))
        {
            return new Dictionary<string, int>(StringComparer.Ordinal);
        }

        using var document = JsonDocument.Parse(File.ReadAllText(observedMergePath));
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            result[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.Array => property.Value.GetArrayLength(),
                JsonValueKind.Object => property.Value.EnumerateObject().Count(),
                _ => 0,
            };
        }

        return result;
    }

    private static IReadOnlyList<string> BuildRecommendedFixes(
        IReadOnlyList<string> missingChoices,
        IReadOnlyList<string> placeholderLabels,
        IReadOnlyList<string> autoAdviceFailures,
        IReadOnlyList<string> missingInformationObserved,
        IReadOnlyList<string> decisionBlockersObserved,
        CompanionCollectorStatus? collectorStatus)
    {
        var fixes = new List<string>();
        if (missingChoices.Count > 0)
        {
            fixes.Add("choice extractor가 비어 있는 화면부터 strict extractor 경로를 우선 보강하세요.");
        }

        if (placeholderLabels.Count > 0)
        {
            fixes.Add("placeholder UI 라벨이 여전히 남아 있으니 generic fallback 필터링을 더 강화하세요.");
        }

        if (autoAdviceFailures.Count > 0)
        {
            fixes.Add("auto advice degraded 원인을 prompt pack과 Codex trace에서 먼저 좁히세요.");
        }

        if (collectorStatus?.SessionId is null)
        {
            fixes.Add("gameplay trigger에서도 Codex session 재사용이 유지되는지 다시 확인하세요.");
        }

        if (fixes.Count == 0)
        {
            fixes.Add("collector summary 기준 blocker가 보이지 않으니 다음 high-value screen coverage를 늘리세요.");
        }

        return fixes;
    }

    private static bool IsSemanticEvent(LiveExportEventEnvelope envelope)
    {
        return envelope.Kind is "choice-list-presented"
            or "reward-opened"
            or "reward-screen-opened"
            or "event-opened"
            or "event-screen-opened"
            or "shop-opened"
            or "rest-opened";
    }

    private static bool IsHighValueScreen(string? screen)
    {
        return screen is "rewards" or "event" or "shop" or "rest-site" or "card-choice" or "upgrade" or "transform";
    }

    private static IReadOnlyList<JsonDocument> ReadJsonLines(string? directory, string fileName)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return Array.Empty<JsonDocument>();
        }

        return ReadJsonLines(Path.Combine(directory, fileName));
    }

    private static IReadOnlyList<JsonDocument> ReadJsonLines(string path)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<JsonDocument>();
        }

        var content = TryReadAllText(path);
        if (string.IsNullOrWhiteSpace(content))
        {
            return Array.Empty<JsonDocument>();
        }

        var documents = new List<JsonDocument>();
        foreach (var line in SplitJsonObjects(content))
        {
            try
            {
                documents.Add(JsonDocument.Parse(line));
            }
            catch (JsonException)
            {
            }
        }

        return documents;
    }

    private static JsonDocument? ReadLastJsonObject(string? directory, string fileName)
    {
        var lines = ReadJsonLines(directory, fileName);
        return lines.Count == 0 ? null : lines[^1];
    }

    private static string? TryReadString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static string? TryReadNestedString(JsonDocument? document, string parentProperty, string propertyName)
    {
        if (document is null)
        {
            return null;
        }

        return document.RootElement.ValueKind == JsonValueKind.Object
               && document.RootElement.TryGetProperty(parentProperty, out var parent)
               && parent.ValueKind == JsonValueKind.Object
               && parent.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static int? TryReadNestedInt(JsonDocument? document, string parentProperty, string propertyName)
    {
        if (document is null)
        {
            return null;
        }

        return document.RootElement.ValueKind == JsonValueKind.Object
               && document.RootElement.TryGetProperty(parentProperty, out var parent)
               && parent.ValueKind == JsonValueKind.Object
               && parent.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.Number
               && property.TryGetInt32(out var value)
            ? value
            : null;
    }
}
