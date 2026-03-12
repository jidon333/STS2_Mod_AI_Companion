namespace Sts2AiCompanion.Foundation.Contracts;

public sealed record CompanionState(
    string StateId,
    DateTimeOffset CapturedAt,
    CompanionRunIdentity Run,
    CompanionSceneState Scene,
    CompanionPlayerState Player,
    CompanionCombatState Combat,
    CompanionChoiceState Choices,
    CompanionRewardState Reward,
    CompanionEventState Event,
    CompanionShopState Shop,
    CompanionRestState Rest,
    CompanionMapState Map,
    CompanionModalState Modal,
    CompanionTransitionState Transition,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Unknowns,
    IReadOnlyDictionary<string, double> Confidence)
{
    public static CompanionState CreateUnknown(string? runId = null)
    {
        return new CompanionState(
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow,
            new CompanionRunIdentity(runId, null, "unknown", null, null, null),
            new CompanionSceneState("unknown", "unknown", 0.0, "none", null),
            new CompanionPlayerState(null, null, null, null, null, new Dictionary<string, string?>(), "unknown", Array.Empty<string>(), Array.Empty<string>()),
            new CompanionCombatState(false, null, null, null, null),
            new CompanionChoiceState(Array.Empty<CompanionChoiceItem>(), "none", null, 0.0),
            new CompanionRewardState(null, Array.Empty<CompanionChoiceItem>(), false),
            new CompanionEventState(null, null, Array.Empty<CompanionChoiceItem>()),
            new CompanionShopState(Array.Empty<CompanionChoiceItem>(), Array.Empty<CompanionChoiceItem>(), false),
            new CompanionRestState(Array.Empty<CompanionChoiceItem>()),
            new CompanionMapState(null, Array.Empty<string>()),
            new CompanionModalState(null, false),
            new CompanionTransitionState(null, "unknown", null),
            Array.Empty<string>(),
            new[] { "state-unavailable" },
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase));
    }
}

public sealed record CompanionRunIdentity(
    string? RunId,
    string? SessionId,
    string Status,
    int? Act,
    int? Floor,
    string? Seed);

public sealed record CompanionSceneState(
    string SceneType,
    string SemanticSceneType,
    double Confidence,
    string Source,
    string? EpisodeId);

public sealed record CompanionPlayerState(
    int? CurrentHp,
    int? MaxHp,
    int? Block,
    int? Energy,
    int? Gold,
    IReadOnlyDictionary<string, string?> Resources,
    string DeckSummary,
    IReadOnlyList<string> Relics,
    IReadOnlyList<string> Potions);

public sealed record CompanionCombatState(
    bool InCombat,
    string? EncounterKind,
    int? Turn,
    string? HandSummary,
    string? EnemyIntentSummary);

public sealed record CompanionChoiceState(
    IReadOnlyList<CompanionChoiceItem> List,
    string ChoiceSource,
    string? ExtractorPath,
    double Confidence);

public sealed record CompanionChoiceItem(
    string Kind,
    string Label,
    string? Value,
    string? Description);

public sealed record CompanionRewardState(
    string? RewardType,
    IReadOnlyList<CompanionChoiceItem> Entries,
    bool SkippedAllowed);

public sealed record CompanionEventState(
    string? EventId,
    string? PageId,
    IReadOnlyList<CompanionChoiceItem> Options);

public sealed record CompanionShopState(
    IReadOnlyList<CompanionChoiceItem> Services,
    IReadOnlyList<CompanionChoiceItem> Entries,
    bool PricesKnown);

public sealed record CompanionRestState(
    IReadOnlyList<CompanionChoiceItem> Actions);

public sealed record CompanionMapState(
    string? CurrentNode,
    IReadOnlyList<string> ReachableNodes);

public sealed record CompanionModalState(
    string? ModalType,
    bool IsBlocking);

public sealed record CompanionTransitionState(
    string? PreviousScene,
    string CurrentScene,
    string? Marker);
