namespace Sts2AiCompanion.AdvisorSceneModel;

public static class AdvisorSceneSchema
{
    public const string Version = "advisor-scene/v1alpha2";
}

public sealed record AdvisorSceneArtifact(
    string SchemaVersion,
    string SourceKind,
    string RunId,
    DateTimeOffset? CapturedAtUtc,
    DateTimeOffset? PublishedAtUtc,
    string? AttemptId,
    int? StepIndex,
    string? Phase,
    string SceneType,
    string SceneStage,
    string CanonicalOwner,
    string SummaryText,
    AdvisorScenePlayerContext PlayerContext,
    IReadOnlyList<AdvisorSceneUiSurface> UiSurfaceInventory,
    IReadOnlyList<AdvisorSceneOption> Options,
    IReadOnlyList<string> MissingFacts,
    IReadOnlyList<string> ObserverGaps,
    IReadOnlyDictionary<string, double> Confidence,
    IReadOnlyList<string> SourceRefs)
{
    public string? RequestPath { get; init; }

    public string? ScreenshotPath { get; init; }

    public AdvisorSceneCombatDetails? Combat { get; init; }

    public AdvisorSceneRewardDetails? Reward { get; init; }

    public AdvisorSceneEventDetails? Event { get; init; }

    public AdvisorSceneRestSiteDetails? RestSite { get; init; }

    public AdvisorSceneShopDetails? Shop { get; init; }

    public AdvisorSceneMapDetails? Map { get; init; }
}

public sealed record AdvisorScenePlayerContext(
    int? CurrentHp,
    int? MaxHp,
    int? Energy,
    int? Gold,
    int DeckCount,
    IReadOnlyList<string> Relics,
    IReadOnlyList<string> Potions);

public sealed record AdvisorSceneUiSurface(
    string SurfaceId,
    bool Present,
    bool? Enabled,
    string? Summary);

public sealed record AdvisorSceneOption(
    string Label,
    string Kind,
    string? Value,
    string? Description,
    bool Enabled,
    IReadOnlyList<string> Tags);

public sealed record AdvisorSceneCombatDetails(
    bool InCombat,
    string EncounterKind,
    string LifecycleStage,
    int? Turn,
    int HandCount,
    string? HandSummary,
    int? DrawPileCount,
    int? DiscardPileCount,
    int? ExhaustPileCount,
    int? PlayPileCount,
    int? HittableEnemyCount,
    int? TargetableEnemyCount,
    bool TargetingInProgress,
    string? EnemyIntentSummary);

public sealed record AdvisorSceneRewardDetails(
    string ExplicitAction,
    string ReleaseStage,
    bool ClaimableRewardPresent,
    bool ExplicitProceedVisible,
    bool RewardChoiceVisible,
    bool ColorlessChoiceVisible,
    int RewardEntryCount,
    bool CardProgressionSurfacePresent);

public sealed record AdvisorSceneEventDetails(
    string ExplicitAction,
    string ReleaseStage,
    bool RewardSubstateActive,
    bool ExplicitProceedVisible,
    bool HasExplicitProgression,
    string AncientContractLane,
    string? EventIdentity,
    int OptionCount);

public sealed record AdvisorSceneRestSiteDetails(
    string ReleaseStage,
    bool ExplicitChoiceVisible,
    bool ExplicitChoiceReady,
    bool SmithUpgradeActive,
    bool SmithConfirmVisible,
    bool ProceedVisible,
    bool SelectionSettling,
    int ActionCount,
    bool UpgradeCandidateSurfaceVisible);

public sealed record AdvisorSceneShopDetails(
    bool InventoryOpen,
    bool ProceedEnabled,
    bool BackEnabled,
    bool CardRemovalVisible,
    bool CardRemovalEnabled,
    bool CardRemovalUsed,
    int OptionCount,
    int AffordableOptionCount,
    int ServiceCount,
    int ItemCount);

public sealed record AdvisorSceneMapDetails(
    bool ForegroundVisible,
    bool CurrentNodeArrowVisible,
    bool ReachableNodePresent,
    int ReachableNodeCount,
    string? CurrentNode,
    IReadOnlyList<string> ReachableNodeLabels);
