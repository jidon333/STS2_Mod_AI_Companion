using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Configuration;
using LiveEventEnvelope = Sts2ModKit.Core.LiveExport.LiveExportEventEnvelope;
using LiveChoiceSummary = Sts2ModKit.Core.LiveExport.LiveExportChoiceSummary;
using LiveExportSession = Sts2ModKit.Core.LiveExport.LiveExportSession;
using LiveExportSnapshot = Sts2ModKit.Core.LiveExport.LiveExportSnapshot;
using LiveKnowledgeEntry = Sts2ModKit.Core.Knowledge.StaticKnowledgeEntry;
using LegacyCollectorStatus = Sts2AiCompanion.Host.CompanionCollectorStatus;
using LegacyHost = Sts2AiCompanion.Host.CompanionHost;
using LegacyHostSnapshot = Sts2AiCompanion.Host.CompanionHostSnapshot;
using LegacyRunState = Sts2AiCompanion.Host.CompanionRunState;
using LegacyAdviceResponse = Sts2AiCompanion.Host.AdviceResponse;
using LegacyKnowledgeSlice = Sts2AiCompanion.Host.KnowledgeSlice;
using LegacyArtifactPaths = Sts2AiCompanion.Host.CompanionArtifactPaths;
using LegacyHostStatus = Sts2AiCompanion.Host.CompanionHostStatus;

namespace Sts2AiCompanion.Advisor;

public sealed class AdvisorCoordinator : IAsyncDisposable
{
    private readonly LegacyHost _legacyHost;

    public AdvisorCoordinator(ScaffoldConfiguration configuration, string workspaceRoot)
    {
        _legacyHost = new LegacyHost(configuration, workspaceRoot);
        CurrentSnapshot = MapSnapshot(_legacyHost.CurrentSnapshot);
        _legacyHost.SnapshotChanged += OnLegacySnapshotChanged;
    }

    public event EventHandler<CompanionHostSnapshot>? SnapshotChanged;

    public CompanionHostSnapshot CurrentSnapshot { get; private set; }

    public Task StartAsync(CancellationToken cancellationToken = default) => _legacyHost.StartAsync(cancellationToken);

    public Task RefreshAsync(CancellationToken cancellationToken = default) => _legacyHost.RefreshAsync(cancellationToken);

    public Task<bool> RequestManualAdviceAsync(CancellationToken cancellationToken = default) => _legacyHost.RequestManualAdviceAsync(cancellationToken);

    public Task<bool> RequestRetryLastAdviceAsync(CancellationToken cancellationToken = default) => _legacyHost.RequestRetryLastAdviceAsync(cancellationToken);

    public void SetAutoAdviceEnabled(bool enabled) => _legacyHost.SetAutoAdviceEnabled(enabled);

    public void SetSelectedModel(string? model) => _legacyHost.SetSelectedModel(model);

    public void SetSelectedReasoningEffort(string? value) => _legacyHost.SetSelectedReasoningEffort(value);

    public async ValueTask DisposeAsync()
    {
        _legacyHost.SnapshotChanged -= OnLegacySnapshotChanged;
        await _legacyHost.DisposeAsync().ConfigureAwait(false);
    }

    private void OnLegacySnapshotChanged(object? sender, LegacyHostSnapshot snapshot)
    {
        CurrentSnapshot = MapSnapshot(snapshot);
        SnapshotChanged?.Invoke(this, CurrentSnapshot);
    }

    private static CompanionHostSnapshot MapSnapshot(LegacyHostSnapshot snapshot)
    {
        return new CompanionHostSnapshot(
            MapStatus(snapshot.Status),
            snapshot.RunState is null ? null : MapRunState(snapshot.RunState),
            snapshot.LatestAdvice is null ? null : MapAdvice(snapshot.LatestAdvice),
            snapshot.LatestKnowledgeSlice is null ? null : MapKnowledgeSlice(snapshot.LatestKnowledgeSlice),
            snapshot.CollectorStatus is null ? null : MapCollector(snapshot.CollectorStatus),
            MapPaths(snapshot.Paths));
    }

    private static CompanionHostStatus MapStatus(LegacyHostStatus status)
    {
        return new CompanionHostStatus(
            status.State,
            status.LiveConnected,
            status.CodexAvailable,
            status.AutoAdviceEnabled,
            status.AnalysisInProgress,
            status.RunId,
            status.SelectedModel,
            status.SelectedReasoningEffort,
            status.AnalysisTriggerKind,
            status.AnalysisStartedAt,
            status.UpdatedAt,
            status.LastAdviceAt,
            status.Message);
    }

    private static CompanionArtifactPaths MapPaths(LegacyArtifactPaths paths)
    {
        return new CompanionArtifactPaths(
            paths.CompanionRoot,
            paths.CurrentRunStatePath,
            paths.RunRoot,
            paths.LiveMirrorRoot,
            paths.PromptPacksRoot,
            paths.AdviceRoot,
            paths.AdviceLogPath,
            paths.AdviceLatestJsonPath,
            paths.AdviceLatestMarkdownPath,
            paths.CodexSessionPath,
            paths.CodexTracePath,
            paths.CollectorSummaryPath,
            paths.HostStatusPath);
    }

    private static CompanionRunState MapRunState(LegacyRunState runState)
    {
        return new CompanionRunState(
            MapSnapshot(runState.Snapshot),
            MapSession(runState.Session),
            runState.SummaryText,
            runState.RecentEvents.Select(MapEvent).ToArray(),
            runState.IsStale)
        {
            NormalizedState = runState.NormalizedState
        };
    }

    private static LiveExportSnapshot MapSnapshot(LiveExportSnapshot snapshot) => snapshot;

    private static LiveExportSession? MapSession(LiveExportSession? session) => session;

    private static LiveEventEnvelope MapEvent(LiveEventEnvelope envelope) => envelope;

    private static KnowledgeSlice MapKnowledgeSlice(LegacyKnowledgeSlice slice)
    {
        return new KnowledgeSlice(slice.Entries.Select(MapKnowledgeEntry).ToArray(), slice.ApproximateBytes, slice.Reasons.ToArray());
    }

    private static LiveKnowledgeEntry MapKnowledgeEntry(LiveKnowledgeEntry entry) => entry;

    private static AdviceResponse MapAdvice(LegacyAdviceResponse response)
    {
        return new AdviceResponse(
            response.Status,
            response.Headline,
            response.Summary,
            response.RecommendedAction,
            response.RecommendedChoiceLabel,
            response.ReasoningBullets.ToArray(),
            response.RiskNotes.ToArray(),
            response.MissingInformation.ToArray(),
            response.DecisionBlockers.ToArray(),
            response.Confidence,
            response.KnowledgeRefs.ToArray(),
            response.GeneratedAt,
            response.RunId,
            response.TriggerKind,
            response.SessionId,
            response.RawResponse,
            response.RewardRecommendationTrace);
    }

    private static CompanionCollectorStatus MapCollector(LegacyCollectorStatus collector)
    {
        return new CompanionCollectorStatus(
            collector.Enabled,
            collector.ActiveScreenEpisode,
            collector.LastSemanticScreen,
            collector.ChoiceExtractionStatus,
            collector.LastAcceptedExtractorPath,
            collector.LastDegradedReason,
            collector.KnowledgeEntriesUsedCount,
            collector.KnowledgeReasons.ToArray(),
            collector.TopKnowledgeRefs.ToArray(),
            collector.MissingInformation.ToArray(),
            collector.DecisionBlockers.ToArray(),
            collector.SessionId,
            collector.Notes);
    }
}
