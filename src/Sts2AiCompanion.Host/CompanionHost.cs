using System.Text;
using System.Text.Json;
using Sts2AiCompanion.AdvisorSceneModel;
using Sts2AiCompanion.SceneProvenance;
using Sts2AiCompanion.Foundation.State;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.LiveExport;
using FoundationCodexCliClient = Sts2AiCompanion.Foundation.Reasoning.CodexCliClient;
using FoundationKnowledgeCatalogService = Sts2AiCompanion.Foundation.Knowledge.KnowledgeCatalogService;
using FoundationAdvicePromptBuilder = Sts2AiCompanion.Foundation.Reasoning.AdvicePromptBuilder;
using FoundationAdviceResponseFinalizer = Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.AdviceResponseFinalizer;
using FoundationCompactAdvisorFallbackFactory = Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.CompactAdvisorFallbackFactory;
using FoundationCompactAdvisorScenePolicy = Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.CompactAdvisorScenePolicy;
using FoundationRewardEventCompactAdvisorInputBuilder = Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.RewardEventCompactAdvisorInputBuilder;
using FoundationStrategyPrinciplesService = Sts2AiCompanion.Foundation.Knowledge.StrategyPrinciplesService;

namespace Sts2AiCompanion.Host;

public sealed partial class CompanionHost : IAsyncDisposable
{
    private const int InitialEventsTailBytes = 4 * 1024 * 1024;

    private readonly ScaffoldConfiguration _configuration;
    private readonly string _workspaceRoot;
    private readonly LiveExportLayout _layout;
    private readonly KnowledgeCatalogService _knowledgeCatalogService;
    private readonly FoundationStrategyPrinciplesService _strategyPrinciplesService;
    private readonly AdvicePromptBuilder _promptBuilder;
    private readonly ICodexSessionClient _codexSessionClient;
    private readonly CompanionHostDiagnosticsService _diagnosticsService;
    private readonly FoundationRewardEventCompactAdvisorInputBuilder _compactInputBuilder = new();
    private readonly SemaphoreSlim _adviceLock = new(1, 1);
    private readonly object _automaticAdviceSync = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    private readonly JsonSerializerOptions _ndjsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false };
    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;
    private long _lastObservedSeq;
    private long _lastEventsReadPosition;
    private string _pendingEventLine = string.Empty;
    private bool _eventsCursorInitialized;
    private DateTimeOffset? _lastAdviceAt;
    private string? _currentRunId;
    private AdviceResponse? _latestAdvice;
    private AdvisorSceneArtifact? _latestSceneModel;
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
    private string? _lastLoggedSceneModelPayload;

    public CompanionHost(ScaffoldConfiguration configuration, string workspaceRoot, ICodexSessionClient? codexSessionClient = null)
    {
        _configuration = configuration;
        _workspaceRoot = workspaceRoot;
        _layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        var foundationKnowledgeCatalogService = new FoundationKnowledgeCatalogService(configuration, workspaceRoot);
        var foundationPromptBuilder = new FoundationAdvicePromptBuilder(configuration);
        _knowledgeCatalogService = new KnowledgeCatalogService(foundationKnowledgeCatalogService);
        _strategyPrinciplesService = new FoundationStrategyPrinciplesService(configuration, workspaceRoot);
        _promptBuilder = new AdvicePromptBuilder(foundationPromptBuilder);
        _codexSessionClient = codexSessionClient ?? new CodexCliClient(new FoundationCodexCliClient(configuration, workspaceRoot));
        _diagnosticsService = new CompanionHostDiagnosticsService(configuration, workspaceRoot, _layout, _jsonOptions, _ndjsonOptions);
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
            : CreateRunState(promptPack.Snapshot, null, promptPack.SummaryText, promptPack.RecentEvents, false);
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
            if (snapshot is null) { PublishSnapshot(CreateSnapshot("waiting-live-export", "state.latest.json is not available yet.")); return; }
            var runChanged = !string.Equals(_currentRunId, snapshot.RunId, StringComparison.Ordinal);
            if (runChanged)
            {
                _currentRunId = snapshot.RunId;
                _sessionState = TryReadExistingSessionState(snapshot.RunId);
                _latestAdvice = null;
                _latestSceneModel = null;
                _lastPromptPackPath = null;
                _lastLoggedSceneModelPayload = null;
                _lastObservedSeq = 0;
                ClearPendingAutomaticAdvice();
            }

            var events = ReadNewEvents(
                _layout.EventsPath,
                snapshot.RunId,
                ref _lastObservedSeq,
                ref _lastEventsReadPosition,
                ref _pendingEventLine,
                ref _eventsCursorInitialized,
                _configuration.Assistant.RecentEventsCount);
            var baseRecentEvents = runChanged || _currentRunState is null
                ? Array.Empty<LiveExportEventEnvelope>()
                : _currentRunState.RecentEvents;
            var recentEvents = baseRecentEvents
                .Concat(events)
                .Where(envelope => string.IsNullOrWhiteSpace(snapshot.RunId)
                                   || string.Equals(envelope.RunId, snapshot.RunId, StringComparison.Ordinal))
                .TakeLast(_configuration.Assistant.RecentEventsCount)
                .ToArray();
            var runState = CreateRunState(snapshot, session, summary ?? string.Empty, recentEvents, DateTimeOffset.UtcNow - snapshot.CapturedAt > TimeSpan.FromSeconds(10));
            _currentRunState = runState;
            _latestSceneModel = UpdateSceneArtifacts(runState);
            PublishSnapshot(CreateSnapshot("running", "Scene model updated from live export."));
            _latestKnowledgeSlice = _knowledgeCatalogService.BuildSlice(runState, _configuration.Assistant.MaxKnowledgeEntries, _configuration.Assistant.MaxKnowledgeBytes);
            if (_autoAdviceEnabled)
            {
                var autoTrigger = DetermineAutomaticTrigger(events);
                if (autoTrigger is not null)
                {
                    if (IsCompactAdvisorManagedScene(runState.NormalizedState.Scene.SceneType))
                    {
                        WriteCodexTrace(
                            EnsureRunArtifacts(runState.Snapshot.RunId),
                            new
                            {
                                kind = "request-skipped",
                                trigger = autoTrigger.Kind,
                                manual = autoTrigger.Manual,
                                requestedAt = autoTrigger.RequestedAt,
                                reason = "compact-advisor-manual-only",
                                scene = NormalizeCompactAdvisorSceneType(runState.NormalizedState.Scene.SceneType),
                            });
                    }
                    else
                    {
                        StartAutomaticAdvice(runState, autoTrigger, cancellationToken);
                    }
                }
            }
            _latestCollectorStatus = _diagnosticsService.UpdateArtifacts(runState, _latestKnowledgeSlice, _latestAdvice, _sessionState);
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

    private void StartAutomaticAdvice(CompanionRunState runState, AdviceTrigger trigger, CancellationToken cancellationToken)
    {
        if (!_adviceLock.Wait(0))
        {
            QueuePendingAutomaticAdvice(runState, trigger);
            return;
        }

        _ = RunAutomaticAdviceAsync(runState, trigger, cancellationToken);
    }

    private async Task RunAutomaticAdviceAsync(CompanionRunState runState, AdviceTrigger trigger, CancellationToken cancellationToken)
    {
        try
        {
            await ProcessAdviceChainWhileLockedAsync(runState, trigger, null, null, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Best effort only on shutdown/cancellation.
        }
        catch (Exception)
        {
            // GenerateAdviceCoreAsync already published failure state and trace.
        }
        finally
        {
            _adviceLock.Release();
        }
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
        AdviceInputPack inputPack;
        if (inputPackOverride is not null)
        {
            inputPack = inputPackOverride;
            if (TryResolveCompactNoCallReason(runState, inputPack, out var noCallReason))
            {
                CompleteNoCallAdviceResult(runState, trigger, inputPack, retrySourcePromptPack, noCallReason);
                return;
            }
        }
        else
        {
            var compactResult = trigger.Manual
                ? _compactInputBuilder.Build(runState.ToFoundation(), _latestKnowledgeSlice.ToFoundation())
                : null;
            var strategyPrinciples = compactResult?.CompactInput is null
                ? null
                : _strategyPrinciplesService.GetRelevantPrinciples(compactResult.CompactInput);
            if (trigger.Manual && compactResult is { Supported: false })
            {
                var fallbackInputPack = _promptBuilder.BuildInputPack(runState, trigger, _latestKnowledgeSlice, compactResult.CompactInput, strategyPrinciples);
                CompleteNoCallAdviceResult(runState, trigger, fallbackInputPack, retrySourcePromptPack, compactResult.ReasonCode);
                return;
            }

            inputPack = _promptBuilder.BuildInputPack(runState, trigger, _latestKnowledgeSlice, compactResult?.CompactInput, strategyPrinciples);
        }

        var paths = EnsureRunArtifacts(runState.Snapshot.RunId);
        var promptPath = retrySourcePromptPack ?? Path.Combine(paths.PromptPacksRoot!, $"{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}-{trigger.Kind}.json");
        if (inputPackOverride is null) WriteJson(promptPath, inputPack);
        _lastPromptPackPath = promptPath;
        var requestId = Guid.NewGuid().ToString("N");
        var startedAt = DateTimeOffset.UtcNow;
        WriteCodexTrace(paths, new { kind = "request-started", requestId, trigger = trigger.Kind, manual = trigger.Manual, requestedAt = trigger.RequestedAt, startedAt, existingSessionId = _sessionState?.SessionId, retrySourcePromptPack, compactInput = inputPack.CompactInput is not null, knowledgeEntriesUsedCount = inputPack.KnowledgeEntries.Count, knowledgeReasons = inputPack.KnowledgeReasons });
        var prompt = _promptBuilder.FormatPrompt(inputPack);
        try
        {
            var (response, sessionId) = await _codexSessionClient.ExecuteAsync(inputPack, prompt, _sessionState?.SessionId, _selectedModel, _selectedReasoningEffort, cancellationToken).ConfigureAwait(false);
            CompleteAdviceResult(runState, trigger, inputPack, retrySourcePromptPack, requestId, startedAt, paths, response, sessionId, modelCall: true);
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

    private void CompleteNoCallAdviceResult(
        CompanionRunState runState,
        AdviceTrigger trigger,
        AdviceInputPack inputPack,
        string? retrySourcePromptPack,
        string reasonCode)
    {
        var paths = EnsureRunArtifacts(runState.Snapshot.RunId);
        var promptPath = retrySourcePromptPack ?? Path.Combine(paths.PromptPacksRoot!, $"{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}-{trigger.Kind}.json");
        if (retrySourcePromptPack is null)
        {
            WriteJson(promptPath, inputPack);
        }

        _lastPromptPackPath = promptPath;

        var requestId = Guid.NewGuid().ToString("N");
        var startedAt = DateTimeOffset.UtcNow;
        WriteCodexTrace(paths, new
        {
            kind = "request-started",
            requestId,
            trigger = trigger.Kind,
            manual = trigger.Manual,
            requestedAt = trigger.RequestedAt,
            startedAt,
            existingSessionId = _sessionState?.SessionId,
            retrySourcePromptPack,
            compactInput = inputPack.CompactInput is not null,
            compactUnsupportedReason = reasonCode,
            promptPackPath = promptPath,
            knowledgeEntriesUsedCount = inputPack.KnowledgeEntries.Count,
            knowledgeReasons = inputPack.KnowledgeReasons,
        });

        var skippedResponse = CreateCompactNoCallResponse(runState, inputPack, reasonCode);
        CompleteAdviceResult(runState, trigger, inputPack, retrySourcePromptPack, requestId, startedAt, paths, skippedResponse, null, modelCall: false);
    }

    private static AdviceResponse CreateCompactNoCallResponse(
        CompanionRunState runState,
        AdviceInputPack inputPack,
        string reasonCode)
    {
        var sceneType = NormalizeCompactAdvisorSceneType(inputPack.CompactInput?.SceneType ?? runState.NormalizedState.Scene.SceneType ?? inputPack.CurrentScreen);
        return reasonCode.StartsWith("unsupported-scene-for-compact-advisor:", StringComparison.Ordinal)
            ? FoundationCompactAdvisorFallbackFactory.CreateUnsupportedScene(inputPack.ToFoundation(), runState.NormalizedState.Scene.SceneType).ToHost()
            : sceneType is "combat" || reasonCode.StartsWith("combat-", StringComparison.OrdinalIgnoreCase)
                ? FoundationCompactAdvisorFallbackFactory.CreateCombatPreview(
                    inputPack.ToFoundation(),
                    reasonCode,
                    inputPack.CompactInput?.MissingInformation ?? Array.Empty<string>(),
                    inputPack.CompactInput?.DecisionBlockers ?? Array.Empty<string>()).ToHost()
            : FoundationCompactAdvisorFallbackFactory.CreateInsufficientCompactInput(
                inputPack.ToFoundation(),
                reasonCode,
                inputPack.CompactInput?.MissingInformation ?? Array.Empty<string>(),
                inputPack.CompactInput?.DecisionBlockers ?? Array.Empty<string>()).ToHost();
    }

    private static bool TryResolveCompactNoCallReason(
        CompanionRunState runState,
        AdviceInputPack inputPack,
        out string reasonCode)
    {
        var sceneType = NormalizeCompactAdvisorSceneType(inputPack.CompactInput?.SceneType ?? runState.NormalizedState.Scene.SceneType ?? inputPack.CurrentScreen);
        if (!IsCompactAdvisorScene(sceneType))
        {
            reasonCode = $"unsupported-scene-for-compact-advisor:{sceneType}";
            return true;
        }

        var blockers = inputPack.CompactInput?.DecisionBlockers ?? Array.Empty<string>();
        var blockingReason = blockers.FirstOrDefault(IsCompactNoCallBlocker);
        if (!string.IsNullOrWhiteSpace(blockingReason))
        {
            reasonCode = blockingReason;
            return true;
        }

        reasonCode = string.Empty;
        return false;
    }

    private static bool IsCompactNoCallBlocker(string blocker)
    {
        return FoundationCompactAdvisorScenePolicy.IsNoCallReason(blocker);
    }

    private static bool IsCompactAdvisorScene(string? sceneType)
    {
        return FoundationCompactAdvisorScenePolicy.IsCompactAdvisorScene(sceneType);
    }

    private static bool IsCompactAdvisorManagedScene(string? sceneType)
    {
        return IsCompactAdvisorScene(sceneType);
    }

    private static string NormalizeCompactAdvisorSceneType(string? sceneType)
    {
        return FoundationCompactAdvisorScenePolicy.NormalizeSceneType(sceneType);
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

    private void CompleteAdviceResult(
        CompanionRunState runState,
        AdviceTrigger trigger,
        AdviceInputPack inputPack,
        string? retrySourcePromptPack,
        string requestId,
        DateTimeOffset startedAt,
        CompanionArtifactPaths paths,
        AdviceResponse response,
        string? sessionId,
        bool modelCall)
    {
        var finalResponse = FoundationAdviceResponseFinalizer.Apply(inputPack.ToFoundation(), response.ToFoundation()).ToHost();
        _latestAdvice = finalResponse;
        _lastAdviceAt = finalResponse.GeneratedAt;
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            _sessionState = new CodexSessionState(runState.Snapshot.RunId, sessionId!, _sessionState?.CreatedAt ?? DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        }

        WriteJson(paths.AdviceLatestJsonPath!, finalResponse);
        File.WriteAllText(paths.AdviceLatestMarkdownPath!, _promptBuilder.FormatAdviceMarkdown(finalResponse));
        AppendNdjson(paths.AdviceLogPath!, finalResponse);
        if (_sessionState is not null)
        {
            WriteJson(paths.CodexSessionPath!, _sessionState);
        }

        WriteCodexTrace(paths, new
        {
            kind = "request-finished",
            requestId,
            trigger = trigger.Kind,
            manual = trigger.Manual,
            status = finalResponse.Status,
            generatedAt = finalResponse.GeneratedAt,
            durationMs = (int)Math.Max(0, (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds),
            sessionId = _sessionState?.SessionId ?? sessionId,
            retrySourcePromptPack,
            modelCall,
            missingInformation = finalResponse.MissingInformation,
            decisionBlockers = finalResponse.DecisionBlockers,
            knowledgeEntriesUsedCount = inputPack.KnowledgeEntries.Count,
            knowledgeReasons = inputPack.KnowledgeReasons,
            knowledgeRefs = finalResponse.KnowledgeRefs,
        });
        _latestCollectorStatus = _diagnosticsService.UpdateArtifacts(runState, _latestKnowledgeSlice, _latestAdvice, _sessionState);
        EndAnalysis();
        PublishSnapshot(CreateSnapshot(MapAdviceCompletionState(finalResponse), BuildAdviceCompletionMessage(trigger, finalResponse)));
    }

    private CompanionArtifactPaths EnsureRunArtifacts(string runId)
    {
        var paths = CompanionPathResolver.Resolve(_configuration, _workspaceRoot, runId);
        Directory.CreateDirectory(paths.CompanionRoot);
        if (paths.RunRoot is not null) Directory.CreateDirectory(paths.RunRoot);
        if (paths.LiveMirrorRoot is not null) Directory.CreateDirectory(paths.LiveMirrorRoot);
        if (paths.PromptPacksRoot is not null) Directory.CreateDirectory(paths.PromptPacksRoot);
        if (paths.AdviceRoot is not null) Directory.CreateDirectory(paths.AdviceRoot);
        if (paths.AdvisorSceneRoot is not null) Directory.CreateDirectory(paths.AdvisorSceneRoot);
        return paths;
    }

    private CompanionHostSnapshot CreateSnapshot(string state, string message)
    {
        var paths = CompanionPathResolver.Resolve(_configuration, _workspaceRoot, _currentRunId);
        var status = new CompanionHostStatus(state, _currentRunState is not null, true, _autoAdviceEnabled, _analysisStartedAt is not null, _currentRunId, _selectedModel, _selectedReasoningEffort, _analysisTriggerKind, _analysisStartedAt, DateTimeOffset.UtcNow, _lastAdviceAt, _analysisMessage ?? message);
        WriteJson(paths.HostStatusPath, status);
        return new CompanionHostSnapshot(status, _currentRunState, _latestSceneModel, _latestAdvice, _latestKnowledgeSlice, _latestCollectorStatus, paths);
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

    private static IReadOnlyList<LiveExportEventEnvelope> ReadNewEvents(
        string path,
        string? currentRunId,
        ref long lastObservedSeq,
        ref long lastReadPosition,
        ref string pendingLine,
        ref bool cursorInitialized,
        int tailCount)
    {
        if (!File.Exists(path)) return Array.Empty<LiveExportEventEnvelope>();

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        if (!cursorInitialized || stream.Length < lastReadPosition)
        {
            lastReadPosition = 0;
            pendingLine = string.Empty;
            cursorInitialized = false;
            lastObservedSeq = 0;
        }

        var startPosition = lastReadPosition;
        var skipLeadingPartialLine = false;
        if (!cursorInitialized)
        {
            startPosition = Math.Max(0, stream.Length - InitialEventsTailBytes);
            skipLeadingPartialLine = startPosition > 0;
        }

        stream.Seek(startPosition, SeekOrigin.Begin);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var content = reader.ReadToEnd();
        lastReadPosition = stream.Position;
        cursorInitialized = true;

        if (skipLeadingPartialLine)
        {
            var newlineIndex = content.IndexOf('\n');
            if (newlineIndex < 0)
            {
                pendingLine = string.Empty;
                return Array.Empty<LiveExportEventEnvelope>();
            }

            content = content[(newlineIndex + 1)..];
        }

        if (!string.IsNullOrEmpty(pendingLine))
        {
            content = pendingLine + content;
            pendingLine = string.Empty;
        }

        if (string.IsNullOrEmpty(content))
        {
            return Array.Empty<LiveExportEventEnvelope>();
        }

        content = content.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (!content.EndsWith("\n", StringComparison.Ordinal))
        {
            var lastNewline = content.LastIndexOf('\n');
            if (lastNewline < 0)
            {
                pendingLine = content;
                return Array.Empty<LiveExportEventEnvelope>();
            }

            pendingLine = content[(lastNewline + 1)..];
            content = content[..(lastNewline + 1)];
        }

        var results = new List<LiveExportEventEnvelope>();
        foreach (var line in content.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            LiveExportEventEnvelope? envelope;
            try
            {
                envelope = JsonSerializer.Deserialize<LiveExportEventEnvelope>(line, ConfigurationLoader.JsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (envelope is null || envelope.Seq <= lastObservedSeq) continue;
            if (!string.IsNullOrWhiteSpace(currentRunId)
                && !string.Equals(envelope.RunId, currentRunId, StringComparison.Ordinal))
            {
                continue;
            }

            results.Add(envelope);
            lastObservedSeq = Math.Max(lastObservedSeq, envelope.Seq);
        }

        return results.TakeLast(tailCount).ToArray();
    }

    private static IReadOnlyList<string> ReadAllLinesShared(string path) { using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete); using var reader = new StreamReader(stream, Encoding.UTF8); return reader.ReadToEnd().Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n', StringSplitOptions.RemoveEmptyEntries).ToArray(); }
    private void WriteJson<T>(string path, T value) { Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path, JsonSerializer.Serialize(value, _jsonOptions)); }
    private void AppendNdjson<T>(string path, T value) { Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.AppendAllText(path, JsonSerializer.Serialize(value, _ndjsonOptions) + Environment.NewLine); }
    private void WriteCodexTrace(CompanionArtifactPaths paths, object trace) { if (!_configuration.LiveExport.CollectorModeEnabled || paths.CodexTracePath is null) return; AppendNdjson(paths.CodexTracePath, trace); }

    private static CompanionRunState CreateRunState(
        LiveExportSnapshot snapshot,
        LiveExportSession? session,
        string summaryText,
        IReadOnlyList<LiveExportEventEnvelope> recentEvents,
        bool isStale)
    {
        var provenance = ScreenProvenanceResolver.Resolve(ScreenProvenanceResolver.CreateFromLiveSnapshot(snapshot));
        return new CompanionRunState(snapshot, session, summaryText, recentEvents, isStale)
        {
            NormalizedState = CompanionStateMapper.FromLiveExport(
                snapshot,
                session,
                recentEvents,
                provenance.ResolvedCurrentScreen,
                provenance.ResolvedVisibleScreen,
                provenance.ProvenanceSource),
            ScreenProvenance = provenance,
        };
    }
}
