using System.Text;
using System.Text.Json;
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
    private readonly object _automaticAdviceSync = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    private readonly JsonSerializerOptions _ndjsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false };
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
    private string? _lastPromptPackPath;
    private AdviceTrigger? _pendingAutomaticTrigger;
    private CompanionRunState? _pendingAutomaticRunState;
    private DateTimeOffset? _analysisStartedAt;
    private string? _analysisTriggerKind;
    private string? _analysisMessage;
    private long _adviceRequestSequence;

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
        CurrentSnapshot = CreateSnapshot("idle", "Waiting for live export.");
    }

    public event EventHandler<CompanionHostSnapshot>? SnapshotChanged;
    public CompanionHostSnapshot CurrentSnapshot { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_loopTask is not null) return;
        _loopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loopTask = Task.Run(() => RunLoopAsync(_loopCts.Token), _loopCts.Token);
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public Task RefreshAsync(CancellationToken cancellationToken = default) => PollOnceAsync(cancellationToken);

    public async Task StopAsync()
    {
        if (_loopCts is null || _loopTask is null) return;
        _loopCts.Cancel();
        try { await _loopTask.ConfigureAwait(false); } catch (OperationCanceledException) { }
        finally { _loopTask = null; _loopCts.Dispose(); _loopCts = null; }
    }

    public async Task<bool> RequestManualAdviceAsync(CancellationToken cancellationToken = default)
    {
        if (_currentRunState is null) return false;
        var trigger = new AdviceTrigger("manual", DateTimeOffset.UtcNow, true, true, "manual-trigger", null);
        await ExecuteRequestedAdviceAsync(_currentRunState, trigger, null, null, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> RequestRetryLastAdviceAsync(CancellationToken cancellationToken = default)
    {
        var promptPackPath = ResolveLastPromptPackPath();
        if (string.IsNullOrWhiteSpace(promptPackPath) || !File.Exists(promptPackPath)) return false;
        var promptPack = TryReadJson<AdviceInputPack>(promptPackPath);
        if (promptPack is null) return false;
        var runState = _currentRunState is not null && string.Equals(_currentRunState.Snapshot.RunId, promptPack.RunId, StringComparison.Ordinal)
            ? _currentRunState
            : new CompanionRunState(promptPack.Snapshot, null, promptPack.SummaryText, promptPack.RecentEvents, false);
        var trigger = new AdviceTrigger("retry-last", DateTimeOffset.UtcNow, true, true, "retry-last-prompt-pack", null);
        await ExecuteRequestedAdviceAsync(runState, trigger, promptPack, promptPackPath, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public void SetAutoAdviceEnabled(bool enabled) { _autoAdviceEnabled = enabled; PublishSnapshot(CreateSnapshot("running", enabled ? "Automatic advice is enabled." : "Automatic advice is paused.")); }
    public void SetSelectedModel(string? model) { _selectedModel = string.IsNullOrWhiteSpace(model) ? null : model.Trim(); PublishSnapshot(CreateSnapshot("running", _selectedModel is null ? "Using the default Codex model." : $"Using Codex model: {_selectedModel}")); }
    public void SetSelectedReasoningEffort(string? value) { _selectedReasoningEffort = string.IsNullOrWhiteSpace(value) ? null : value.Trim(); PublishSnapshot(CreateSnapshot("running", _selectedReasoningEffort is null ? "Using the default reasoning effort." : $"Using reasoning effort: {_selectedReasoningEffort}")); }
    public async ValueTask DisposeAsync() { await StopAsync().ConfigureAwait(false); _adviceLock.Dispose(); }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Math.Max(_configuration.Assistant.LivePollIntervalMs, 250)));
        do { await PollOnceAsync(cancellationToken).ConfigureAwait(false); }
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
            if (snapshot is null) { PublishSnapshot(CreateSnapshot("waiting-live-export", "state.latest.json is not available yet.")); return; }
            var recentEvents = (_currentRunState?.RecentEvents ?? Array.Empty<LiveExportEventEnvelope>()).Concat(events).TakeLast(_configuration.Assistant.RecentEventsCount).ToArray();
            var runState = new CompanionRunState(snapshot, session, summary ?? string.Empty, recentEvents, DateTimeOffset.UtcNow - snapshot.CapturedAt > TimeSpan.FromSeconds(10));
            if (!string.Equals(_currentRunId, snapshot.RunId, StringComparison.Ordinal))
            {
                _currentRunId = snapshot.RunId;
                _sessionState = TryReadExistingSessionState(snapshot.RunId);
                _latestAdvice = null;
                _lastPromptPackPath = null;
                ClearPendingAutomaticAdvice();
            }
            _currentRunState = runState;
            _latestKnowledgeSlice = _knowledgeCatalogService.BuildSlice(runState, _configuration.Assistant.MaxKnowledgeEntries, _configuration.Assistant.MaxKnowledgeBytes);
            if (_autoAdviceEnabled)
            {
                var autoTrigger = DetermineAutomaticTrigger(events);
                if (autoTrigger is not null) await EnqueueAutomaticAdviceAsync(runState, autoTrigger, cancellationToken).ConfigureAwait(false);
            }
            UpdateCollectorArtifacts(runState);
            PublishSnapshot(CreateSnapshot("running", "Monitoring live export updates."));
        }
        catch (Exception exception) { PublishSnapshot(CreateSnapshot("error", exception.Message)); }
    }

    private AdviceTrigger? DetermineAutomaticTrigger(IReadOnlyList<LiveExportEventEnvelope> events)
    {
        AdviceTrigger? selected = null;
        foreach (var envelope in events)
        {
            if (!IsAutomaticAdviceTriggerKind(envelope.Kind)) continue;
            var decision = LiveExportSummaryFormatter.EvaluateCodexTrigger(new LiveExportObservation(envelope.Kind, envelope.Ts, envelope.RunId, null, envelope.Screen, envelope.Act, envelope.Floor, null, null, null, null, null, null, null, envelope.Payload, new Dictionary<string, string?>()), new LiveExportTriggerWindow(_lastAdviceAt, TimeSpan.FromMilliseconds(_configuration.Assistant.MinAdviceIntervalMs)));
            if (decision.ShouldTriggerCodex) selected = new AdviceTrigger(envelope.Kind, envelope.Ts, false, decision.BypassMinInterval, decision.Reason, envelope);
        }
        return selected;
    }

    private async Task EnqueueAutomaticAdviceAsync(CompanionRunState runState, AdviceTrigger trigger, CancellationToken cancellationToken)
    {
        var acquired = await _adviceLock.WaitAsync(TimeSpan.Zero, cancellationToken).ConfigureAwait(false);
        if (!acquired) { QueuePendingAutomaticAdvice(runState, trigger); return; }
        try { await ProcessAdviceChainWhileLockedAsync(runState, trigger, null, null, cancellationToken).ConfigureAwait(false); }
        finally { _adviceLock.Release(); }
    }

    private async Task ExecuteRequestedAdviceAsync(CompanionRunState runState, AdviceTrigger trigger, AdviceInputPack? inputPackOverride, string? retrySourcePromptPack, CancellationToken cancellationToken)
    {
        if (string.Equals(trigger.Kind, "retry-last", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(retrySourcePromptPack))
            WriteCodexTrace(EnsureRunArtifacts(runState.Snapshot.RunId), new { kind = "request-retried", trigger = trigger.Kind, retrySourcePromptPack, requestedAt = trigger.RequestedAt });
        await _adviceLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try { await ProcessAdviceChainWhileLockedAsync(runState, trigger, inputPackOverride, retrySourcePromptPack, cancellationToken).ConfigureAwait(false); }
        finally { _adviceLock.Release(); }
    }

    private async Task ProcessAdviceChainWhileLockedAsync(CompanionRunState runState, AdviceTrigger trigger, AdviceInputPack? inputPackOverride, string? retrySourcePromptPack, CancellationToken cancellationToken)
    {
        var currentRunState = runState; var currentTrigger = trigger; var currentInputPack = inputPackOverride; var currentRetrySource = retrySourcePromptPack;
        while (true)
        {
            await GenerateAdviceCoreAsync(currentRunState, currentTrigger, currentInputPack, currentRetrySource, cancellationToken).ConfigureAwait(false);
            currentInputPack = null; currentRetrySource = null;
            if (!TryTakePendingAutomaticAdvice(out currentRunState, out currentTrigger)) break;
        }
    }

    private async Task GenerateAdviceCoreAsync(CompanionRunState runState, AdviceTrigger trigger, AdviceInputPack? inputPackOverride, string? retrySourcePromptPack, CancellationToken cancellationToken)
    {
        BeginAnalysis(trigger, retrySourcePromptPack is null ? "analyzing" : "retrying");
        _latestKnowledgeSlice = inputPackOverride is not null
            ? new KnowledgeSlice(inputPackOverride.KnowledgeEntries, inputPackOverride.KnowledgeEntries.Sum(e => e.Name.Length + (e.RawText?.Length ?? 0)), inputPackOverride.KnowledgeReasons)
            : _knowledgeCatalogService.BuildSlice(runState, _configuration.Assistant.MaxKnowledgeEntries, _configuration.Assistant.MaxKnowledgeBytes);
        var inputPack = inputPackOverride ?? _promptBuilder.BuildInputPack(runState, trigger, _latestKnowledgeSlice);
        var prompt = _promptBuilder.FormatPrompt(inputPack);
        var paths = EnsureRunArtifacts(runState.Snapshot.RunId);
        var promptPath = retrySourcePromptPack ?? Path.Combine(paths.PromptPacksRoot!, $"{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}-{trigger.Kind}.json");
        if (inputPackOverride is null) WriteJson(promptPath, inputPack);
        _lastPromptPackPath = promptPath;
        var requestId = Guid.NewGuid().ToString("N");
        var startedAt = DateTimeOffset.UtcNow;
        WriteCodexTrace(paths, new { kind = "request-started", requestId, trigger = trigger.Kind, manual = trigger.Manual, requestedAt = trigger.RequestedAt, startedAt, existingSessionId = _sessionState?.SessionId, retrySourcePromptPack, knowledgeEntriesUsedCount = inputPack.KnowledgeEntries.Count, knowledgeReasons = inputPack.KnowledgeReasons });
        try
        {
            var (response, sessionId) = await _codexSessionClient.ExecuteAsync(inputPack, prompt, _sessionState?.SessionId, _selectedModel, _selectedReasoningEffort, cancellationToken).ConfigureAwait(false);
            _latestAdvice = response; _lastAdviceAt = response.GeneratedAt;
            if (!string.IsNullOrWhiteSpace(sessionId)) _sessionState = new CodexSessionState(runState.Snapshot.RunId, sessionId!, _sessionState?.CreatedAt ?? DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
            WriteJson(paths.AdviceLatestJsonPath!, response);
            File.WriteAllText(paths.AdviceLatestMarkdownPath!, _promptBuilder.FormatAdviceMarkdown(response));
            AppendNdjson(paths.AdviceLogPath!, response);
            if (_sessionState is not null) WriteJson(paths.CodexSessionPath!, _sessionState);
            WriteCodexTrace(paths, new { kind = "request-finished", requestId, trigger = trigger.Kind, manual = trigger.Manual, status = response.Status, generatedAt = response.GeneratedAt, durationMs = (int)Math.Max(0, (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds), sessionId = _sessionState?.SessionId ?? sessionId, retrySourcePromptPack, missingInformation = response.MissingInformation, decisionBlockers = response.DecisionBlockers, knowledgeEntriesUsedCount = inputPack.KnowledgeEntries.Count, knowledgeReasons = inputPack.KnowledgeReasons, knowledgeRefs = response.KnowledgeRefs });
            UpdateCollectorArtifacts(runState);
            EndAnalysis();
            PublishSnapshot(CreateSnapshot(MapAdviceCompletionState(response), BuildAdviceCompletionMessage(trigger, response)));
        }
        catch (OperationCanceledException)
        {
            WriteCodexTrace(paths, new { kind = "request-canceled", requestId, trigger = trigger.Kind, manual = trigger.Manual, retrySourcePromptPack });
            EndAnalysis("canceled", $"AI request canceled: {trigger.Kind}");
            PublishSnapshot(CreateSnapshot("canceled", $"AI request canceled: {trigger.Kind}"));
            throw;
        }
        catch (Exception exception)
        {
            WriteCodexTrace(paths, new { kind = "request-failed", requestId, trigger = trigger.Kind, manual = trigger.Manual, retrySourcePromptPack, error = exception.Message });
            EndAnalysis("failed", exception.Message);
            PublishSnapshot(CreateSnapshot("failed", $"AI request failed: {trigger.Kind} - {exception.Message}"));
            throw;
        }
    }

    private void QueuePendingAutomaticAdvice(CompanionRunState runState, AdviceTrigger trigger)
    {
        if (trigger.Manual) return;
        lock (_automaticAdviceSync)
        {
            var paths = EnsureRunArtifacts(runState.Snapshot.RunId);
            if (_pendingAutomaticTrigger is not null)
                WriteCodexTrace(paths, new { kind = "request-superseded", previousTrigger = _pendingAutomaticTrigger.Kind, nextTrigger = trigger.Kind, requestedAt = trigger.RequestedAt });
            else
                WriteCodexTrace(paths, new { kind = "request-coalesced", trigger = trigger.Kind, requestedAt = trigger.RequestedAt });
            _pendingAutomaticRunState = runState;
            _pendingAutomaticTrigger = trigger;
        }
    }

    private bool TryTakePendingAutomaticAdvice(out CompanionRunState runState, out AdviceTrigger trigger)
    {
        lock (_automaticAdviceSync)
        {
            if (_pendingAutomaticRunState is null || _pendingAutomaticTrigger is null) { runState = default!; trigger = default!; return false; }
            runState = _pendingAutomaticRunState; trigger = _pendingAutomaticTrigger;
            _pendingAutomaticRunState = null; _pendingAutomaticTrigger = null; return true;
        }
    }

    private void ClearPendingAutomaticAdvice()
    {
        lock (_automaticAdviceSync) { _pendingAutomaticRunState = null; _pendingAutomaticTrigger = null; }
    }

    private string? ResolveLastPromptPackPath()
    {
        if (!string.IsNullOrWhiteSpace(_lastPromptPackPath) && File.Exists(_lastPromptPackPath)) return _lastPromptPackPath;
        if (string.IsNullOrWhiteSpace(_currentRunId)) return null;
        var paths = CompanionPathResolver.Resolve(_configuration, _workspaceRoot, _currentRunId);
        if (string.IsNullOrWhiteSpace(paths.PromptPacksRoot) || !Directory.Exists(paths.PromptPacksRoot)) return null;
        return Directory.GetFiles(paths.PromptPacksRoot, "*.json", SearchOption.TopDirectoryOnly).OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
    }

    private static bool IsAutomaticAdviceTriggerKind(string kind)
    {
        return kind is "choice-list-presented" or "reward-opened" or "reward-screen-opened" or "event-opened" or "event-screen-opened" or "shop-opened" or "rest-opened";
    }

    private static string MapAdviceCompletionState(AdviceResponse response)
    {
        return string.Equals(response.Status, "ok", StringComparison.OrdinalIgnoreCase) ? "running"
            : response.Summary.Contains("canceled", StringComparison.OrdinalIgnoreCase) ? "canceled"
            : string.Equals(response.Status, "degraded", StringComparison.OrdinalIgnoreCase) ? "degraded"
            : "failed";
    }

    private static string BuildAdviceCompletionMessage(AdviceTrigger trigger, AdviceResponse response)
    {
        return string.Equals(response.Status, "ok", StringComparison.OrdinalIgnoreCase)
            ? $"AI advice ready: {trigger.Kind}"
            : $"AI advice degraded: {trigger.Kind} - {response.Summary}";
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
        if (paths.LiveMirrorRoot is null) return;
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
        WriteJson(paths.CurrentRunStatePath, new { runId = runState.Snapshot.RunId, screen = runState.Snapshot.CurrentScreen, capturedAt = runState.Snapshot.CapturedAt, liveRoot = _layout.LiveRoot, sessionId = _sessionState?.SessionId, collectorModeEnabled = _configuration.LiveExport.CollectorModeEnabled });
    }

    private CompanionArtifactPaths EnsureRunArtifacts(string runId)
    {
        var paths = CompanionPathResolver.Resolve(_configuration, _workspaceRoot, runId);
        Directory.CreateDirectory(paths.CompanionRoot);
        if (paths.RunRoot is not null) Directory.CreateDirectory(paths.RunRoot);
        if (paths.LiveMirrorRoot is not null) Directory.CreateDirectory(paths.LiveMirrorRoot);
        if (paths.PromptPacksRoot is not null) Directory.CreateDirectory(paths.PromptPacksRoot);
        if (paths.AdviceRoot is not null) Directory.CreateDirectory(paths.AdviceRoot);
        return paths;
    }

    private CompanionHostSnapshot CreateSnapshot(string state, string message)
    {
        var paths = CompanionPathResolver.Resolve(_configuration, _workspaceRoot, _currentRunId);
        var status = new CompanionHostStatus(state, _currentRunState is not null, true, _autoAdviceEnabled, _analysisStartedAt is not null, _currentRunId, _selectedModel, _selectedReasoningEffort, _analysisTriggerKind, _analysisStartedAt, DateTimeOffset.UtcNow, _lastAdviceAt, _analysisMessage ?? message);
        WriteJson(paths.HostStatusPath, status);
        return new CompanionHostSnapshot(status, _currentRunState, _latestAdvice, _latestKnowledgeSlice, _latestCollectorStatus, paths);
    }

    private void BeginAnalysis(AdviceTrigger trigger, string state = "analyzing")
    {
        _analysisStartedAt = DateTimeOffset.UtcNow;
        _analysisTriggerKind = trigger.Kind;
        _analysisMessage = state == "retrying" ? $"Retrying AI advice: {trigger.Kind}" : $"AI analyzing: {trigger.Kind}";
        PublishSnapshot(CreateSnapshot(state, _analysisMessage));
    }

    private void EndAnalysis(string? state = null, string? message = null)
    {
        _analysisStartedAt = null;
        _analysisTriggerKind = null;
        _analysisMessage = message;
    }

    private void PublishSnapshot(CompanionHostSnapshot snapshot) { CurrentSnapshot = snapshot; SnapshotChanged?.Invoke(this, snapshot); }

    private CodexSessionState? TryReadExistingSessionState(string runId)
    {
        var paths = CompanionPathResolver.Resolve(_configuration, _workspaceRoot, runId);
        if (string.IsNullOrWhiteSpace(paths.CodexSessionPath) || !File.Exists(paths.CodexSessionPath)) return null;
        try { return TryReadJson<CodexSessionState>(paths.CodexSessionPath); } catch { return null; }
    }

    private static T? TryReadJson<T>(string path)
    {
        if (!File.Exists(path)) return default;
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        return JsonSerializer.Deserialize<T>(stream, ConfigurationLoader.JsonOptions);
    }

    private static string? TryReadAllText(string path)
    {
        if (!File.Exists(path)) return null;
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static IReadOnlyList<LiveExportEventEnvelope> ReadNewEvents(string path, ref long lastObservedSeq, int tailCount)
    {
        if (!File.Exists(path)) return Array.Empty<LiveExportEventEnvelope>();
        var results = new List<LiveExportEventEnvelope>();
        foreach (var line in ReadAllLinesShared(path))
        {
            var envelope = JsonSerializer.Deserialize<LiveExportEventEnvelope>(line, ConfigurationLoader.JsonOptions);
            if (envelope is null || envelope.Seq <= lastObservedSeq) continue;
            results.Add(envelope);
            lastObservedSeq = Math.Max(lastObservedSeq, envelope.Seq);
        }
        return results.TakeLast(tailCount).ToArray();
    }

    private CompanionCollectorStatus BuildCollectorStatus(CompanionRunState runState, CompanionArtifactPaths paths)
    {
        if (!_configuration.LiveExport.CollectorModeEnabled)
            return new CompanionCollectorStatus(false, null, null, null, null, null, 0, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), _sessionState?.SessionId, "collector mode disabled");
        var latestDecision = ReadLastJsonObject(paths.LiveMirrorRoot, Path.GetFileName(_layout.ChoiceDecisionsPath));
        var lastSemanticScreen = runState.RecentEvents.LastOrDefault(IsSemanticEvent)?.Screen;
        var activeEpisode = IsHighValueScreen(runState.Snapshot.CurrentScreen) ? runState.Snapshot.CurrentScreen : lastSemanticScreen;
        var acceptedExtractorPath = TryReadNestedString(latestDecision, "decision", "extractorPath");
        var acceptedCount = TryReadNestedInt(latestDecision, "decision", "acceptedCount");
        var failureReason = TryReadNestedString(latestDecision, "decision", "failureReason");
        var choiceStatus = acceptedCount > 0 ? $"resolved ({acceptedCount})" : runState.Snapshot.CurrentChoices.Count > 0 ? $"resolved ({runState.Snapshot.CurrentChoices.Count})" : latestDecision is not null ? "missing" : "not-seen";
        var degradedReason = failureReason ?? _latestAdvice?.DecisionBlockers.FirstOrDefault() ?? _latestAdvice?.MissingInformation.FirstOrDefault() ?? (_latestAdvice is { Status: not "ok" } ? _latestAdvice.Summary : null);
        var notes = string.Join(Environment.NewLine, new[]
        {
            $"collector mode: on",
            $"latest semantic screen: {lastSemanticScreen ?? "none"}",
            $"active screen episode: {activeEpisode ?? "none"}",
            $"choice extraction: {choiceStatus}",
            $"extractor path: {acceptedExtractorPath ?? "none"}",
            $"last degraded reason: {degradedReason ?? "none"}",
            $"knowledge entries used: {_latestKnowledgeSlice?.Entries.Count ?? 0}",
            $"knowledge reasons: {(_latestKnowledgeSlice?.Reasons.Count > 0 ? string.Join(", ", _latestKnowledgeSlice.Reasons.Take(4)) : "none")}",
            $"knowledge refs: {(_latestAdvice?.KnowledgeRefs.Count > 0 ? string.Join(", ", _latestAdvice.KnowledgeRefs.Take(4)) : "none")}",
            $"missing information: {(_latestAdvice?.MissingInformation.Count > 0 ? string.Join(", ", _latestAdvice.MissingInformation.Take(4)) : "none")}",
            $"decision blockers: {(_latestAdvice?.DecisionBlockers.Count > 0 ? string.Join(", ", _latestAdvice.DecisionBlockers.Take(4)) : "none")}",
            $"session id: {_sessionState?.SessionId ?? "none"}",
        });
        return new CompanionCollectorStatus(true, activeEpisode, lastSemanticScreen, choiceStatus, acceptedExtractorPath, degradedReason, _latestKnowledgeSlice?.Entries.Count ?? 0, _latestKnowledgeSlice?.Reasons ?? Array.Empty<string>(), _latestAdvice?.KnowledgeRefs ?? Array.Empty<string>(), _latestAdvice?.MissingInformation ?? Array.Empty<string>(), _latestAdvice?.DecisionBlockers ?? Array.Empty<string>(), _sessionState?.SessionId, notes);
    }

    private CompanionCollectorSummary BuildCollectorSummary(CompanionRunState runState, CompanionArtifactPaths paths, CompanionCollectorStatus? collectorStatus)
    {
        var transitions = ReadJsonLines(paths.LiveMirrorRoot, Path.GetFileName(_layout.ScreenTransitionsPath));
        var decisions = ReadJsonLines(paths.LiveMirrorRoot, Path.GetFileName(_layout.ChoiceDecisionsPath));
        var candidates = ReadJsonLines(paths.LiveMirrorRoot, Path.GetFileName(_layout.ChoiceCandidatesPath));
        var adviceLog = paths.AdviceLogPath is not null && File.Exists(paths.AdviceLogPath) ? ReadJsonLines(paths.AdviceLogPath) : Array.Empty<JsonDocument>();
        var traceLog = paths.CodexTracePath is not null && File.Exists(paths.CodexTracePath) ? ReadJsonLines(paths.CodexTracePath) : Array.Empty<JsonDocument>();
        var runtimeFatalErrors = ReadRuntimeFatalErrors();
        var timeline = transitions.Select(d => { var r = d.RootElement; return $"{TryReadString(r, "triggerKind") ?? "unknown"}: {TryReadString(r, "before") ?? "unknown"} -> {TryReadString(r, "after") ?? "unknown"} (incoming={TryReadString(r, "incoming") ?? "unknown"}, reason={TryReadString(r, "reason") ?? "none"})"; }).TakeLast(24).ToArray();
        var semanticCounts = runState.RecentEvents.Where(IsSemanticEvent).GroupBy(e => e.Kind, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        var missingChoices = decisions.Where(d => (TryReadNestedInt(d, "decision", "acceptedCount") ?? 0) == 0).Select(d => $"{TryReadString(d.RootElement, "screen") ?? "unknown"} via {TryReadNestedString(d, "decision", "extractorPath") ?? "unknown"} ({TryReadNestedString(d, "decision", "failureReason") ?? "unknown"})").Distinct(StringComparer.Ordinal).ToArray();
        var placeholderLabels = candidates.Where(d => string.Equals(TryReadNestedString(d, "candidate", "rejectReason"), "placeholder-label", StringComparison.Ordinal)).Select(d => TryReadNestedString(d, "candidate", "label")).Where(v => !string.IsNullOrWhiteSpace(v)).Cast<string>().Distinct(StringComparer.Ordinal).OrderBy(v => v, StringComparer.Ordinal).Take(64).ToArray();
        var autoAdviceFailures = adviceLog.Where(d => { var r = d.RootElement; var s = TryReadString(r, "status"); var t = TryReadString(r, "triggerKind"); return !string.Equals(s, "ok", StringComparison.OrdinalIgnoreCase) && !string.Equals(t, "manual", StringComparison.OrdinalIgnoreCase) && !string.Equals(t, "retry-last", StringComparison.OrdinalIgnoreCase); }).Select(d => { var r = d.RootElement; return $"{TryReadString(r, "triggerKind") ?? "unknown"}: {TryReadString(r, "summary") ?? "degraded"}"; }).TakeLast(12).ToArray();
        var missingInformationObserved = SummarizeFrequency(adviceLog.SelectMany(d => ReadStringArray(d.RootElement, "missingInformation")), 64);
        var decisionBlockersObserved = SummarizeFrequency(adviceLog.SelectMany(d => ReadStringArray(d.RootElement, "decisionBlockers")), 64);
        return new CompanionCollectorSummary(runState.Snapshot.RunId, DateTimeOffset.UtcNow, timeline, semanticCounts, missingChoices, placeholderLabels, autoAdviceFailures, missingInformationObserved, decisionBlockersObserved, collectorStatus?.SessionId is null ? "missing-session-id" : "session-tracked", ReadObservedMergeCounts(), BuildRequestLatencySummary(traceLog), BuildDuplicateTriggerSummary(traceLog), BuildScreenOverwriteSummary(transitions), BuildStateRegressionSummary(runState), BuildKnowledgeUsageSummary(adviceLog, traceLog), runtimeFatalErrors, BuildAppHangSuspicionIndicators(paths, runtimeFatalErrors, traceLog), BuildRecommendedFixes(missingChoices, placeholderLabels, autoAdviceFailures, missingInformationObserved, decisionBlockersObserved, collectorStatus));
    }

    private static void CopyIfExists(string sourcePath, string destinationPath) { if (File.Exists(sourcePath)) File.Copy(sourcePath, destinationPath, overwrite: true); }
    private static void CopyDirectoryIfExists(string sourcePath, string destinationPath) { if (!Directory.Exists(sourcePath)) return; Directory.CreateDirectory(destinationPath); foreach (var file in Directory.GetFiles(sourcePath)) File.Copy(file, Path.Combine(destinationPath, Path.GetFileName(file)), overwrite: true); }
    private static IReadOnlyList<string> ReadAllLinesShared(string path) { using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete); using var reader = new StreamReader(stream, Encoding.UTF8); return reader.ReadToEnd().Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n', StringSplitOptions.RemoveEmptyEntries).ToArray(); }
    private void WriteJson<T>(string path, T value) { Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path, JsonSerializer.Serialize(value, _jsonOptions)); }
    private void AppendNdjson<T>(string path, T value) { Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.AppendAllText(path, JsonSerializer.Serialize(value, _ndjsonOptions) + Environment.NewLine); }
    private void WriteCodexTrace(CompanionArtifactPaths paths, object trace) { if (!_configuration.LiveExport.CollectorModeEnabled || paths.CodexTracePath is null) return; AppendNdjson(paths.CodexTracePath, trace); }
    private IReadOnlyDictionary<string, int> ReadObservedMergeCounts() { var path = Path.Combine(CompanionPathResolver.ResolveKnowledgeRoot(_configuration, _workspaceRoot), "observed-merge.json"); if (!File.Exists(path)) return new Dictionary<string, int>(StringComparer.Ordinal); using var document = JsonDocument.Parse(File.ReadAllText(path)); return document.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.ValueKind switch { JsonValueKind.Array => p.Value.GetArrayLength(), JsonValueKind.Object => p.Value.EnumerateObject().Count(), _ => 0 }, StringComparer.OrdinalIgnoreCase); }
    private static IReadOnlyList<string> BuildRequestLatencySummary(IReadOnlyList<JsonDocument> traceLog)
    {
        var durations = traceLog
            .Where(d => string.Equals(TryReadString(d.RootElement, "kind"), "request-finished", StringComparison.Ordinal))
            .Select(d => TryReadInt(d.RootElement, "durationMs"))
            .Where(v => v is not null)
            .Select(v => v!.Value)
            .OrderBy(v => v)
            .ToArray();
        if (durations.Length == 0)
        {
            return Array.Empty<string>();
        }

        static int Percentile(int[] sorted, double percentile)
        {
            var index = (int)Math.Ceiling(sorted.Length * percentile) - 1;
            return sorted[Math.Clamp(index, 0, sorted.Length - 1)];
        }

        return new[]
        {
            $"count={durations.Length}",
            $"min_ms={durations[0]}",
            $"p50_ms={Percentile(durations, 0.50)}",
            $"p90_ms={Percentile(durations, 0.90)}",
            $"avg_ms={(int)durations.Average()}",
            $"max_ms={durations[^1]}",
        };
    }
    private static IReadOnlyList<string> BuildDuplicateTriggerSummary(IReadOnlyList<JsonDocument> traceLog) => traceLog.Select(d => TryReadString(d.RootElement, "kind")).Where(k => k is "request-coalesced" or "request-superseded" or "request-canceled" or "request-retried").GroupBy(k => k!, StringComparer.Ordinal).OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => $"{g.Key}={g.Count()}").ToArray();
    private static IReadOnlyList<string> BuildScreenOverwriteSummary(IReadOnlyList<JsonDocument> transitions) => transitions.Where(d => d.RootElement.TryGetProperty("keptPreviousScreen", out var kept) && kept.ValueKind == JsonValueKind.True).Select(d => { var r = d.RootElement; return $"{TryReadString(r, "triggerKind") ?? "unknown"} kept {TryReadString(r, "before") ?? "unknown"} over {TryReadString(r, "incoming") ?? "unknown"}"; }).TakeLast(24).ToArray();
    private static IReadOnlyList<string> BuildStateRegressionSummary(CompanionRunState runState) => runState.Snapshot.Warnings.Where(w => w.StartsWith("state-regression:", StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.Ordinal).Take(64).ToArray();
    private static IReadOnlyList<string> BuildKnowledgeUsageSummary(IReadOnlyList<JsonDocument> adviceLog, IReadOnlyList<JsonDocument> traceLog)
    {
        var knowledgeRefCounts = adviceLog.Select(d => ReadStringArray(d.RootElement, "knowledgeRefs").Count).ToArray();
        var latestTrace = traceLog.LastOrDefault(d => string.Equals(TryReadString(d.RootElement, "kind"), "request-finished", StringComparison.Ordinal));
        var latestTraceKnowledgeCount = latestTrace is null ? null : TryReadInt(latestTrace.RootElement, "knowledgeEntriesUsedCount");
        var latestReasons = latestTrace is null ? Array.Empty<string>() : ReadStringArray(latestTrace.RootElement, "knowledgeReasons");
        var blockedByState = adviceLog.Count(d => ReadStringArray(d.RootElement, "missingInformation").Count > 0 && ReadStringArray(d.RootElement, "knowledgeRefs").Count > 0);
        var knowledgeEmpty = adviceLog.Count(d => ReadStringArray(d.RootElement, "knowledgeRefs").Count == 0 && ReadStringArray(d.RootElement, "missingInformation").Count > 0);
        var topRefs = SummarizeFrequency(adviceLog.SelectMany(d => ReadStringArray(d.RootElement, "knowledgeRefs")), 6);
        return new[]
        {
            $"responses_with_knowledge_refs={knowledgeRefCounts.Count(c => c > 0)}",
            $"max_knowledge_refs={knowledgeRefCounts.DefaultIfEmpty(0).Max()}",
            $"latest_prompt_knowledge_entries={latestTraceKnowledgeCount?.ToString() ?? "0"}",
            $"latest_prompt_knowledge_reasons={(latestReasons.Count > 0 ? string.Join(", ", latestReasons.Take(4)) : "none")}",
            $"knowledge_present_but_blocked_by_runtime={blockedByState}",
            $"knowledge_slice_empty={knowledgeEmpty}",
        }.Concat(topRefs.Select(item => $"top_ref={item}")).ToArray();
    }
    private IReadOnlyList<string> ReadRuntimeFatalErrors()
    {
        var path = Path.Combine(_configuration.GamePaths.GameDirectory, "mods", _configuration.AiCompanionMod.RuntimeLogFileName);
        if (!File.Exists(path))
        {
            return Array.Empty<string>();
        }

        return ReadAllLinesShared(path)
            .Where(line => line.Contains("Method not found", StringComparison.OrdinalIgnoreCase)
                           || line.Contains("runtime exporter worker failure", StringComparison.OrdinalIgnoreCase)
                           || line.Contains("runtime exporter poll failure", StringComparison.OrdinalIgnoreCase)
                           || line.Contains("hook failure", StringComparison.OrdinalIgnoreCase)
                           || line.Contains("disposed", StringComparison.OrdinalIgnoreCase)
                           || line.Contains("format", StringComparison.OrdinalIgnoreCase) && line.Contains("LocString", StringComparison.OrdinalIgnoreCase))
            .TakeLast(48)
            .ToArray();
    }
    private static IReadOnlyList<string> BuildAppHangSuspicionIndicators(CompanionArtifactPaths paths, IReadOnlyList<string> runtimeFatalErrors, IReadOnlyList<JsonDocument> traceLog)
    {
        var failedRequests = traceLog.Count(d => { var kind = TryReadString(d.RootElement, "kind"); return kind is "request-failed" or "request-canceled"; });
        var disposedFailures = runtimeFatalErrors.Count(line => line.Contains("disposed", StringComparison.OrdinalIgnoreCase));
        var exporterFailures = runtimeFatalErrors.Count(line => line.Contains("runtime exporter worker failure", StringComparison.OrdinalIgnoreCase));
        var lastFinishedAt = traceLog.Where(d => string.Equals(TryReadString(d.RootElement, "kind"), "request-finished", StringComparison.Ordinal)).Select(d => TryReadString(d.RootElement, "generatedAt")).LastOrDefault();
        var semanticRoot = paths.LiveMirrorRoot is null ? null : Path.Combine(paths.LiveMirrorRoot, "semantic-snapshots");
        var semanticFiles = semanticRoot is not null && Directory.Exists(semanticRoot)
            ? Directory.GetFiles(semanticRoot, "*.json", SearchOption.TopDirectoryOnly)
            : Array.Empty<string>();
        var lastSemanticSnapshotAt = semanticFiles
            .Select(File.GetLastWriteTimeUtc)
            .DefaultIfEmpty()
            .Max();
        return new[]
        {
            $"runtime_fatal_error_count={runtimeFatalErrors.Count}",
            $"exporter_worker_failure_count={exporterFailures}",
            $"disposed_object_failure_count={disposedFailures}",
            $"failed_or_canceled_requests={failedRequests}",
            $"semantic_snapshot_count={semanticFiles.Length}",
            $"last_semantic_snapshot_at={(lastSemanticSnapshotAt == default ? "none" : new DateTimeOffset(lastSemanticSnapshotAt, TimeSpan.Zero).ToString("O"))}",
            $"last_successful_advice_at={lastFinishedAt ?? "none"}",
        };
    }
    private static IReadOnlyList<string> BuildRecommendedFixes(IReadOnlyList<string> missingChoices, IReadOnlyList<string> placeholderLabels, IReadOnlyList<string> autoAdviceFailures, IReadOnlyList<string> missingInformationObserved, IReadOnlyList<string> decisionBlockersObserved, CompanionCollectorStatus? collectorStatus)
    {
        var fixes = new List<string>();
        if (missingChoices.Count > 0) fixes.Add("Strengthen strict choice extractors for the affected screens before trusting generic fallback.");
        if (placeholderLabels.Count > 0) fixes.Add("Expand placeholder filtering for UI/internal node names in generic choice extraction.");
        if (autoAdviceFailures.Count > 0) fixes.Add("Inspect prompt packs and Codex trace for degraded auto advice failures.");
        if (missingInformationObserved.Any(i => i.Contains("price", StringComparison.OrdinalIgnoreCase))) fixes.Add("Shop extraction still misses concrete price or item identity fields.");
        if (missingInformationObserved.Any(i => i.Contains("item identity", StringComparison.OrdinalIgnoreCase) || i.Contains("상품", StringComparison.OrdinalIgnoreCase))) fixes.Add("Shop and reward extraction still miss concrete item identity fields.");
        if (collectorStatus?.SessionId is null) fixes.Add("Gameplay triggers are not reusing or persisting a Codex session yet.");
        if (decisionBlockersObserved.Any(i => i.Contains("deck", StringComparison.OrdinalIgnoreCase))) fixes.Add("Deck/state merge is still dropping authoritative deck information on overlay screens.");
        if (decisionBlockersObserved.Any(i => i.Contains("price", StringComparison.OrdinalIgnoreCase) || i.Contains("context", StringComparison.OrdinalIgnoreCase))) fixes.Add("Advice is blocked by missing shop price or deck context; treat those as first-class collector blockers.");
        if (fixes.Count == 0) fixes.Add("No high-priority collector blocker detected. Expand gameplay screen coverage.");
        return fixes;
    }
    private static IReadOnlyList<string> SummarizeFrequency(IEnumerable<string> values, int maxItems)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value.Trim(), StringComparer.Ordinal)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Take(maxItems)
            .Select(group => $"{group.Count()}x {group.Key}")
            .ToArray();
    }
    private static bool IsSemanticEvent(LiveExportEventEnvelope envelope) => envelope.Kind is "choice-list-presented" or "reward-opened" or "reward-screen-opened" or "event-opened" or "event-screen-opened" or "shop-opened" or "rest-opened";
    private static bool IsHighValueScreen(string? screen) => screen is "rewards" or "event" or "shop" or "rest-site" or "card-choice" or "upgrade" or "transform";
    private static IReadOnlyList<JsonDocument> ReadJsonLines(string? directory, string fileName) => string.IsNullOrWhiteSpace(directory) ? Array.Empty<JsonDocument>() : ReadJsonLines(Path.Combine(directory, fileName));
    private static IReadOnlyList<JsonDocument> ReadJsonLines(string path) { if (!File.Exists(path)) return Array.Empty<JsonDocument>(); var content = TryReadAllText(path); if (string.IsNullOrWhiteSpace(content)) return Array.Empty<JsonDocument>(); var docs = new List<JsonDocument>(); foreach (var line in SplitJsonObjects(content)) { try { docs.Add(JsonDocument.Parse(line)); } catch (JsonException) { } } return docs; }
    private static JsonDocument? ReadLastJsonObject(string? directory, string fileName) { var lines = ReadJsonLines(directory, fileName); return lines.Count == 0 ? null : lines[^1]; }
    private static string? TryReadString(JsonElement element, string propertyName) => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    private static int? TryReadInt(JsonElement element, string propertyName) => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
    private static string? TryReadNestedString(JsonDocument? document, string parentProperty, string propertyName) => document is not null && document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty(parentProperty, out var parent) && parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    private static int? TryReadNestedInt(JsonDocument? document, string parentProperty, string propertyName) => document is not null && document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty(parentProperty, out var parent) && parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
}
