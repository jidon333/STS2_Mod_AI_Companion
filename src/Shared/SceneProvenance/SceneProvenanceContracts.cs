namespace Sts2AiCompanion.SceneProvenance;

internal sealed record ScreenProvenanceInput(
    string? RawCurrentScreen,
    string? RawObservedScreen,
    string? TrackerCurrentScreen,
    string? PublishedCurrentScreen,
    string? PublishedVisibleScreen,
    string? CompatibilityCurrentScreen,
    string? CompatibilityLogicalScreen,
    string? CompatibilityVisibleScreen,
    bool? PublishedSceneReady,
    string? PublishedSceneAuthority,
    string? PublishedSceneStability,
    bool? CompatibilitySceneReady,
    string? CompatibilitySceneAuthority,
    string? CompatibilitySceneStability,
    bool? EncounterInCombat)
{
    public string? InventorySceneType { get; init; }

    public string? InventoryRawCurrentScreen { get; init; }

    public string? InventoryPublishedCurrentScreen { get; init; }

    public string? InventoryPublishedVisibleScreen { get; init; }

    public string? InventoryCompatibilityCurrentScreen { get; init; }

    public string? InventoryCompatibilityLogicalScreen { get; init; }

    public string? InventoryCompatibilityVisibleScreen { get; init; }

    public bool? InventoryPublishedSceneReady { get; init; }

    public string? InventoryPublishedSceneAuthority { get; init; }

    public string? InventoryPublishedSceneStability { get; init; }

    public bool? InventoryCompatibilitySceneReady { get; init; }

    public string? InventoryCompatibilitySceneAuthority { get; init; }

    public string? InventoryCompatibilitySceneStability { get; init; }
}

internal sealed record ScreenProvenanceResult(
    string? ResolvedCurrentScreen,
    string? ResolvedVisibleScreen,
    bool? ResolvedSceneReady,
    string? ResolvedSceneAuthority,
    string? ResolvedSceneStability,
    string ProvenanceSource,
    bool CombatPromotionApplied)
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
}
