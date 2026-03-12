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
    string HostStatusPath);

public sealed record CompanionRunState(
    LiveExportSnapshot Snapshot,
    LiveExportSession? Session,
    string SummaryText,
    IReadOnlyList<LiveExportEventEnvelope> RecentEvents,
    bool IsStale);

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
    string ConstraintsText);

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
    string? RawResponse);

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
