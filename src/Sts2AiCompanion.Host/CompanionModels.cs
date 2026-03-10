using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Host;

public sealed record CompanionArtifactPaths(
    string CompanionRoot,
    string CurrentRunStatePath,
    string? RunRoot,
    string? LiveMirrorRoot,
    string? PromptPacksRoot,
    string? AdviceLogPath,
    string? AdviceLatestJsonPath,
    string? AdviceLatestMarkdownPath,
    string? CodexSessionPath,
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
    LiveExportEventEnvelope? SourceEvent);

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
    string? RunId,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastAdviceAt,
    string Message);

public sealed record CompanionHostSnapshot(
    CompanionHostStatus Status,
    CompanionRunState? RunState,
    AdviceResponse? LatestAdvice,
    KnowledgeSlice? LatestKnowledgeSlice,
    CompanionArtifactPaths Paths);

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
        CancellationToken cancellationToken);
}
