using System.Text.Json.Serialization;

namespace Sts2ModKit.Core.LiveExport;

public sealed record LiveExportLayout(
    string ModdedProfileRoot,
    string LiveRoot,
    string EventsPath,
    string SnapshotPath,
    string SummaryPath,
    string SessionPath)
{
    public string RawObservationsPath { get; init; } = string.Empty;

    public string ScreenTransitionsPath { get; init; } = string.Empty;

    public string ChoiceCandidatesPath { get; init; } = string.Empty;

    public string ChoiceDecisionsPath { get; init; } = string.Empty;

    public string SemanticSnapshotsRoot { get; init; } = string.Empty;
}

public sealed record LiveExportPlayerSummary(
    string? Name,
    int? CurrentHp,
    int? MaxHp,
    int? Gold,
    int? Energy,
    IReadOnlyDictionary<string, string?> Resources)
{
    public static LiveExportPlayerSummary Empty { get; } = new(
        null,
        null,
        null,
        null,
        null,
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase));
}

public sealed record LiveExportCardSummary(
    string Name,
    string? Id,
    int? Cost,
    string? Type,
    bool? Upgraded);

public sealed record LiveExportChoiceSummary(
    string Kind,
    string Label,
    string? Value,
    string? Description)
{
    public string? NodeId { get; init; }

    public string? ScreenBounds { get; init; }

    public string? BindingKind { get; init; }

    public string? BindingId { get; init; }

    public bool? Enabled { get; init; }

    public string? IconAssetPath { get; init; }

    public IReadOnlyList<string> SemanticHints { get; init; } = Array.Empty<string>();
}

public sealed record LiveExportChoiceCandidate(
    string ExtractorPath,
    string TypeName,
    string? Label,
    string? Value,
    string? Description,
    int Score,
    bool Accepted,
    string? RejectReason);

public sealed record LiveExportChoiceDecision(
    string? ExtractorPath,
    bool UsedStrictExtractor,
    int CandidateCount,
    int AcceptedCount,
    string Outcome,
    string? FailureReason,
    IReadOnlyList<string> PlaceholderLabels);

public sealed record LiveExportEncounterSummary(
    string? Name,
    string? Kind,
    bool? InCombat,
    int? Turn);

public sealed record LiveExportSnapshot(
    string RunId,
    string RunStatus,
    long Version,
    DateTimeOffset CapturedAt,
    string CurrentScreen,
    int? Act,
    int? Floor,
    LiveExportPlayerSummary Player,
    IReadOnlyList<LiveExportCardSummary> Deck,
    IReadOnlyList<string> Relics,
    IReadOnlyList<string> Potions,
    IReadOnlyList<LiveExportChoiceSummary> CurrentChoices,
    IReadOnlyList<string> RecentChanges,
    IReadOnlyList<string> Warnings,
    LiveExportEncounterSummary? Encounter,
    IReadOnlyDictionary<string, string?> Meta)
{
    public string? RawCurrentScreen { get; init; }

    public string? RawObservedScreen { get; init; }

    public string? PublishedCurrentScreen { get; init; }

    public string? PublishedVisibleScreen { get; init; }

    public bool? PublishedSceneReady { get; init; }

    public string? PublishedSceneAuthority { get; init; }

    public string? PublishedSceneStability { get; init; }

    public string? CompatibilityCurrentScreen { get; init; }

    public string? CompatibilityLogicalScreen { get; init; }

    public string? CompatibilityVisibleScreen { get; init; }

    public bool? CompatibilitySceneReady { get; init; }

    public string? CompatibilitySceneAuthority { get; init; }

    public string? CompatibilitySceneStability { get; init; }

    public static LiveExportSnapshot CreateEmpty(string runId)
    {
        return new LiveExportSnapshot(
            runId,
            "idle",
            0,
            DateTimeOffset.UtcNow,
            "unknown",
            null,
            null,
            LiveExportPlayerSummary.Empty,
            Array.Empty<LiveExportCardSummary>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<LiveExportChoiceSummary>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            null,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase));
    }
}

public sealed record LiveExportEventEnvelope(
    DateTimeOffset Ts,
    long Seq,
    string RunId,
    string Kind,
    string Screen,
    int? Act,
    int? Floor,
    IReadOnlyDictionary<string, object?> Payload);

public sealed record LiveExportObservation(
    string TriggerKind,
    DateTimeOffset ObservedAt,
    string? RunId,
    string? RunStatus,
    string? Screen,
    int? Act,
    int? Floor,
    LiveExportPlayerSummary? Player,
    IReadOnlyList<LiveExportCardSummary>? Deck,
    IReadOnlyList<string>? Relics,
    IReadOnlyList<string>? Potions,
    IReadOnlyList<LiveExportChoiceSummary>? Choices,
    IReadOnlyList<string>? Warnings,
    LiveExportEncounterSummary? Encounter,
    IReadOnlyDictionary<string, object?> Payload,
    IReadOnlyDictionary<string, string?> Meta)
{
    public IReadOnlyList<LiveExportChoiceCandidate> ChoiceCandidates { get; init; } = Array.Empty<LiveExportChoiceCandidate>();

    public LiveExportChoiceDecision? ChoiceDecision { get; init; }

    public string? SemanticScreen { get; init; }

    public static LiveExportObservation Create(
        string triggerKind,
        string? screen = null,
        IReadOnlyDictionary<string, object?>? payload = null)
    {
        return new LiveExportObservation(
            triggerKind,
            DateTimeOffset.UtcNow,
            null,
            null,
            screen,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            payload ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase));
    }
}

public sealed record LiveExportSession(
    string SessionId,
    string RunId,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset UpdatedAt,
    long LastSeq,
    string LiveRoot,
    string? LastEventKind,
    string? LastScreen);

public sealed record LiveExportBatch(
    LiveExportSnapshot Snapshot,
    LiveExportSession Session,
    IReadOnlyList<LiveExportEventEnvelope> Events)
{
    public LiveExportObservation? SourceObservation { get; init; }

    public IReadOnlyList<LiveExportScreenTransition> ScreenTransitions { get; init; } = Array.Empty<LiveExportScreenTransition>();

    public LiveExportCollectorStatus? CollectorStatus { get; init; }
}

public sealed record LiveExportScreenTransition(
    DateTimeOffset ObservedAt,
    string TriggerKind,
    string Before,
    string Incoming,
    string After,
    string Reason,
    bool KeptPreviousScreen);

public sealed record LiveExportCollectorStatus(
    bool CollectorModeEnabled,
    string? ActiveScreenEpisode,
    string? LastSemanticScreen,
    string? LastAcceptedExtractorPath,
    string ChoiceExtractionStatus,
    string? LastDegradedReason);

public sealed record LiveExportTriggerDecision(
    bool ShouldTriggerCodex,
    string Reason,
    bool BypassMinInterval);

public sealed record LiveExportTriggerWindow(
    DateTimeOffset? LastTriggerAt,
    TimeSpan MinInterval);

public sealed record LiveExportStateTrackerOptions(
    int MaxRecentChanges,
    int MaxDeckEntries,
    int MaxChoiceEntries,
    bool CollectorModeEnabled)
{
    public static LiveExportStateTrackerOptions CreateDefault() => new(16, 40, 10, false);
}

public sealed record LiveExportReplayResult(
    LiveExportSnapshot Snapshot,
    IReadOnlyList<LiveExportEventEnvelope> Events,
    IReadOnlyList<string> Warnings);

public sealed record LiveExportDedupKey(
    string Kind,
    string Screen,
    string PayloadSignature);

public sealed record LiveExportReplayDocument(
    [property: JsonPropertyName("events")]
    IReadOnlyList<LiveExportEventEnvelope> Events,
    [property: JsonPropertyName("snapshot")]
    LiveExportSnapshot Snapshot);
