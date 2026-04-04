using Sts2AiCompanion.AdvisorSceneModel;
using Sts2AiCompanion.SceneProvenance;
using FoundationCompanionState = Sts2AiCompanion.Foundation.Contracts.CompanionState;
using FoundationRewardAssessmentFacts = Sts2AiCompanion.Foundation.Contracts.RewardAssessmentFacts;
using FoundationRewardOptionSet = Sts2AiCompanion.Foundation.Contracts.RewardOptionSet;
using FoundationRewardRecommendationTrace = Sts2AiCompanion.Foundation.Contracts.RewardRecommendationTrace;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Host;

public sealed record CompanionArtifactPaths(
    string CompanionRoot,
    string CurrentRunStatePath,
    string? RunRoot,
    string? LiveMirrorRoot,
    string? PromptPacksRoot,
    string? AdviceRoot,
    string? AdviceLogPath,
    string? AdviceLatestJsonPath,
    string? AdviceLatestMarkdownPath,
    string? CodexSessionPath,
    string? CodexTracePath,
    string? CollectorSummaryPath,
    string? AdvisorSceneRoot,
    string? AdvisorSceneLatestJsonPath,
    string? AdvisorSceneLogPath,
    string HostStatusPath);

public sealed record CompanionRunState(
    LiveExportSnapshot Snapshot,
    LiveExportSession? Session,
    string SummaryText,
    IReadOnlyList<LiveExportEventEnvelope> RecentEvents,
    bool IsStale)
{
    public FoundationCompanionState NormalizedState { get; init; } = FoundationCompanionState.CreateUnknown(runId: Snapshot.RunId);

    internal ScreenProvenanceResult ScreenProvenance { get; init; } = new(
        null,
        null,
        null,
        null,
        null,
        "none",
        false);
}

public sealed record AdviceTrigger(
    string Kind,
    DateTimeOffset RequestedAt,
    bool Manual,
    bool BypassMinInterval,
    string Reason,
    LiveExportEventEnvelope? SourceEvent,
    string? RetrySourcePromptPackPath = null);

public sealed record KnowledgeSlice(
    IReadOnlyList<StaticKnowledgeEntry> Entries,
    int ApproximateBytes,
    IReadOnlyList<string> Reasons);

public sealed record AdviceInputPack(
    string RunId,
    string TriggerKind,
    DateTimeOffset CreatedAt,
    bool Manual,
    string CurrentScreen,
    string SummaryText,
    LiveExportSnapshot Snapshot,
    IReadOnlyList<LiveExportEventEnvelope> RecentEvents,
    IReadOnlyList<StaticKnowledgeEntry> KnowledgeEntries,
    IReadOnlyList<string> KnowledgeReasons,
    string ConstraintsText,
    FoundationCompanionState? NormalizedState = null,
    FoundationRewardOptionSet? RewardOptionSet = null,
    FoundationRewardAssessmentFacts? RewardAssessmentFacts = null,
    FoundationRewardRecommendationTrace? RewardRecommendationTraceSeed = null);

public sealed record AdviceResponse(
    string Status,
    string Headline,
    string Summary,
    string RecommendedAction,
    string? RecommendedChoiceLabel,
    IReadOnlyList<string> ReasoningBullets,
    IReadOnlyList<string> RiskNotes,
    IReadOnlyList<string> MissingInformation,
    IReadOnlyList<string> DecisionBlockers,
    double? Confidence,
    IReadOnlyList<string> KnowledgeRefs,
    DateTimeOffset GeneratedAt,
    string RunId,
    string TriggerKind,
    string? SessionId,
    string? RawResponse,
    FoundationRewardRecommendationTrace? RewardRecommendationTrace = null);

public sealed record CodexSessionState(
    string RunId,
    string SessionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CompanionHostStatus(
    string State,
    bool LiveConnected,
    bool CodexAvailable,
    bool AutoAdviceEnabled,
    bool AnalysisInProgress,
    string? RunId,
    string? SelectedModel,
    string? SelectedReasoningEffort,
    string? AnalysisTriggerKind,
    DateTimeOffset? AnalysisStartedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastAdviceAt,
    string Message);

public sealed record CompanionHostSnapshot(
    CompanionHostStatus Status,
    CompanionRunState? RunState,
    AdvisorSceneArtifact? LatestSceneModel,
    AdviceResponse? LatestAdvice,
    KnowledgeSlice? LatestKnowledgeSlice,
    CompanionCollectorStatus? CollectorStatus,
    CompanionArtifactPaths Paths);

public sealed record CompanionCollectorStatus(
    bool Enabled,
    string? ActiveScreenEpisode,
    string? LastSemanticScreen,
    string? ChoiceExtractionStatus,
    string? LastAcceptedExtractorPath,
    string? LastDegradedReason,
    int KnowledgeEntriesUsedCount,
    IReadOnlyList<string> KnowledgeReasons,
    IReadOnlyList<string> TopKnowledgeRefs,
    IReadOnlyList<string> MissingInformation,
    IReadOnlyList<string> DecisionBlockers,
    string? SessionId,
    string Notes);

public sealed record CompanionCollectorSummary(
    string RunId,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<string> SeenScreensTimeline,
    IReadOnlyDictionary<string, int> SemanticEventCounts,
    IReadOnlyList<string> ScreensWithMissingChoices,
    IReadOnlyList<string> PlaceholderLabelsObserved,
    IReadOnlyList<string> AutoAdviceFailures,
    IReadOnlyList<string> MissingInformationObserved,
    IReadOnlyList<string> DecisionBlockersObserved,
    string SessionTrackingStatus,
    IReadOnlyDictionary<string, int> ObservedMergeCounts,
    IReadOnlyList<string> RequestLatencySummary,
    IReadOnlyList<string> DuplicateTriggerSummary,
    IReadOnlyList<string> ScreenOverwriteSummary,
    IReadOnlyList<string> StateRegressionSummary,
    IReadOnlyList<string> KnowledgeUsageSummary,
    IReadOnlyList<string> RuntimeFatalErrors,
    IReadOnlyList<string> AppHangSuspicionIndicators,
    IReadOnlyList<string> RecommendedNextFixes);

internal sealed record SessionIndexEntry(
    string Id,
    string? ThreadName,
    DateTimeOffset UpdatedAt);

public interface ICodexSessionClient
{
    Task<(AdviceResponse Response, string? SessionId)> ExecuteAsync(
        AdviceInputPack inputPack,
        string prompt,
        string? sessionId,
        string? modelOverride,
        string? reasoningEffortOverride,
        CancellationToken cancellationToken);
}
