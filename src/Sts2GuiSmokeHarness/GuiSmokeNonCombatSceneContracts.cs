sealed record RewardMapLayerState(
    bool RewardPanelVisible,
    bool MapContextVisible,
    bool RewardBackNavigationAvailable,
    bool StaleRewardChoicePresent,
    bool StaleRewardBoundsPresent,
    bool OffWindowBoundsReused,
    bool RewardForegroundOwned,
    bool RewardTeardownInProgress,
    bool RewardIsCurrentActiveScreen,
    bool MapCurrentActiveScreen,
    bool TerminalRunBoundary);

enum RewardReleaseStage
{
    None,
    Active,
    ReleasePending,
    Released,
}

enum RewardExplicitActionKind
{
    None,
    Claim,
    CardChoice,
    ColorlessChoice,
    SkipProceed,
    Back,
}

enum NonCombatCanonicalForegroundOwner
{
    Unknown,
    Combat,
    Reward,
    Event,
    Shop,
    RestSite,
    Treasure,
    Map,
}

enum NonCombatReleaseStage
{
    None,
    Active,
    ReleasePending,
    Released,
}

enum NonCombatHandoffTarget
{
    None,
    HandleCombat,
    HandleRewards,
    HandleEvent,
    HandleShop,
    ChooseFirstNode,
    WaitEventRelease,
    WaitMap,
    WaitPostMapNodeRoom,
}

enum PostNodeHandoffSurfaceKind
{
    None,
    CombatTakeover,
    CombatForeground,
    Reward,
    Event,
    Shop,
    RestSiteChoice,
    RestSiteSmithUpgrade,
    RestSiteProceed,
    RestSiteSelectionSettling,
    RestSiteReleasePending,
    Treasure,
    MapSurfacePending,
    MapOverlay,
    MapNode,
    ContractMismatch,
}

sealed record PostNodeHandoffState(
    NonCombatCanonicalForegroundOwner Owner,
    NonCombatHandoffTarget HandoffTarget,
    NonCombatReleaseStage ReleaseStage,
    bool HasExplicitSurface,
    PostNodeHandoffSurfaceKind SurfaceKind,
    bool ContractMismatch,
    bool MapOverlayVisible,
    bool StaleBackgroundPresent)
{
    public bool IsMapOwner => Owner == NonCombatCanonicalForegroundOwner.Map;
    public bool IsEventOwner => Owner == NonCombatCanonicalForegroundOwner.Event;
    public bool IsCombatOwner => Owner == NonCombatCanonicalForegroundOwner.Combat;
}

enum CombatAuthorityState
{
    Active,
    ResidueOnly,
    Released,
}

enum CombatReleaseSubtype
{
    None,
    EnemyClickResidue,
    EndTurnReopenLatency,
}

enum NextRoomTransitStage
{
    None,
    MapClickAccepted,
    RoomEntryPending,
    Settled,
}

sealed record NextRoomEntryState(
    NonCombatCanonicalForegroundOwner Owner,
    NonCombatHandoffTarget HandoffTarget,
    NonCombatReleaseStage ReleaseStage,
    NextRoomTransitStage TransitStage,
    bool ExplicitSurfacePresent,
    PostNodeHandoffSurfaceKind SurfaceKind,
    bool RecentMapClickAccepted,
    bool MapTransitPending,
    bool CombatResiduePresent,
    bool RewardResiduePresent,
    bool MapOverlayResiduePresent)
{
    public bool HasExplicitWinner => Owner != NonCombatCanonicalForegroundOwner.Unknown
                                     && ExplicitSurfacePresent;
}

sealed record CombatReleaseState(
    CombatBarrierKind BarrierKind,
    CombatAuthorityState CombatAuthorityState,
    NonCombatCanonicalForegroundOwner ForegroundOwner,
    NonCombatHandoffTarget ReleaseTarget,
    NonCombatReleaseStage ReleaseStage,
    bool HasExplicitForegroundSurface,
    bool ReleaseMismatch,
    CombatReleaseSubtype ReleaseSubtype,
    PostNodeHandoffState HandoffState)
{
    public bool HasReleasedOwnership => CombatAuthorityState != CombatAuthorityState.Active
                                        && ReleaseTarget is not NonCombatHandoffTarget.None and not NonCombatHandoffTarget.HandleCombat;
}

interface ICanonicalNonCombatSceneState
{
    NonCombatCanonicalForegroundOwner CanonicalForegroundOwner { get; }

    NonCombatReleaseStage ReleaseStage { get; }

    NonCombatHandoffTarget HandoffTarget { get; }

    bool SuppressSameActionReissue { get; }

    bool AllowsFastForegroundWait { get; }

    string? ForegroundDebugKind { get; }

    string? BackgroundDebugKind { get; }
}

sealed record RewardSceneState(
    RewardMapLayerState LayerState,
    RewardScreenState? ScreenState,
    NonCombatForegroundOwner CanonicalForegroundOwner,
    RewardReleaseStage ReleaseStage,
    RewardExplicitActionKind ExplicitAction,
    bool RewardChoiceVisible,
    bool ColorlessChoiceVisible,
    bool ClaimableRewardPresent,
    bool ExplicitProceedVisible,
    bool SuppressSameSkipReissue,
    bool CardProgressionSurfacePresent,
    bool ClaimSurfacePresent,
    bool AftermathResiduePresent) : ICanonicalNonCombatSceneState
{
    public bool RewardForegroundOwned => CanonicalForegroundOwner == NonCombatForegroundOwner.Reward;
    public bool MapForegroundOwned => CanonicalForegroundOwner == NonCombatForegroundOwner.Map;
    public bool ReleaseToMapPending => ReleaseStage is RewardReleaseStage.ReleasePending or RewardReleaseStage.Released;

    NonCombatCanonicalForegroundOwner ICanonicalNonCombatSceneState.CanonicalForegroundOwner => CanonicalForegroundOwner switch
    {
        NonCombatForegroundOwner.Reward => NonCombatCanonicalForegroundOwner.Reward,
        NonCombatForegroundOwner.Map => NonCombatCanonicalForegroundOwner.Map,
        _ => NonCombatCanonicalForegroundOwner.Unknown,
    };

    NonCombatReleaseStage ICanonicalNonCombatSceneState.ReleaseStage => ReleaseStage switch
    {
        RewardReleaseStage.Active => NonCombatReleaseStage.Active,
        RewardReleaseStage.ReleasePending => NonCombatReleaseStage.ReleasePending,
        RewardReleaseStage.Released => NonCombatReleaseStage.Released,
        _ => NonCombatReleaseStage.None,
    };

    NonCombatHandoffTarget ICanonicalNonCombatSceneState.HandoffTarget => ReleaseToMapPending
        ? NonCombatHandoffTarget.WaitMap
        : RewardForegroundOwned
            ? NonCombatHandoffTarget.HandleRewards
            : NonCombatHandoffTarget.None;

    bool ICanonicalNonCombatSceneState.SuppressSameActionReissue => SuppressSameSkipReissue;

    bool ICanonicalNonCombatSceneState.AllowsFastForegroundWait => RewardForegroundOwned
                                                                   && ReleaseStage == RewardReleaseStage.Active;

    string? ICanonicalNonCombatSceneState.ForegroundDebugKind => RewardForegroundOwned
        ? ReleaseStage == RewardReleaseStage.ReleasePending ? "reward-release-pending" : "reward"
        : MapForegroundOwned
            ? "map"
            : null;

    string? ICanonicalNonCombatSceneState.BackgroundDebugKind => RewardForegroundOwned
        ? LayerState.MapContextVisible ? "map" : null
        : MapForegroundOwned && LayerState.RewardPanelVisible
            ? "reward"
            : null;
}

enum EventReleaseStage
{
    None,
    Active,
    ReleasePending,
    Released,
}

enum EventExplicitActionKind
{
    None,
    RewardSubstate,
    AncientDialogue,
    AncientCompletion,
    AncientOption,
    AncientOptionContractMismatch,
    EventChoice,
    Proceed,
}

sealed record EventSceneState(
    NonCombatForegroundOwner CanonicalForegroundOwner,
    EventReleaseStage ReleaseStage,
    EventExplicitActionKind ExplicitAction,
    RewardSceneState RewardScene,
    MapOverlayState MapOverlayState,
    AncientEventObserverSignals.AncientEventOptionContractState AncientContract,
    bool MapContextVisible,
    bool RewardSubstateActive,
    bool HasExplicitProgression,
    bool ExplicitRoomEntrySurfacePresent,
    bool StrongForegroundChoice,
    bool ForceProgressionAfterCardSelection,
    bool ExplicitProceedVisible,
    bool SuppressSameProceedReissue) : ICanonicalNonCombatSceneState
{
    public bool EventForegroundOwned => CanonicalForegroundOwner == NonCombatForegroundOwner.Event;
    public bool RewardForegroundOwned => CanonicalForegroundOwner == NonCombatForegroundOwner.Reward;
    public bool MapForegroundOwned => CanonicalForegroundOwner == NonCombatForegroundOwner.Map;

    NonCombatCanonicalForegroundOwner ICanonicalNonCombatSceneState.CanonicalForegroundOwner => CanonicalForegroundOwner switch
    {
        NonCombatForegroundOwner.Reward => NonCombatCanonicalForegroundOwner.Reward,
        NonCombatForegroundOwner.Event => NonCombatCanonicalForegroundOwner.Event,
        NonCombatForegroundOwner.Map => NonCombatCanonicalForegroundOwner.Map,
        _ => NonCombatCanonicalForegroundOwner.Unknown,
    };

    NonCombatReleaseStage ICanonicalNonCombatSceneState.ReleaseStage => ReleaseStage switch
    {
        EventReleaseStage.Active => NonCombatReleaseStage.Active,
        EventReleaseStage.ReleasePending => NonCombatReleaseStage.ReleasePending,
        EventReleaseStage.Released => NonCombatReleaseStage.Released,
        _ => NonCombatReleaseStage.None,
    };

    NonCombatHandoffTarget ICanonicalNonCombatSceneState.HandoffTarget => RewardSubstateActive
        ? NonCombatHandoffTarget.HandleRewards
        : EventForegroundOwned
            ? ReleaseStage == EventReleaseStage.ReleasePending
                ? NonCombatHandoffTarget.WaitEventRelease
                : NonCombatHandoffTarget.HandleEvent
            : MapForegroundOwned
                ? NonCombatHandoffTarget.WaitMap
                : NonCombatHandoffTarget.None;

    bool ICanonicalNonCombatSceneState.SuppressSameActionReissue => SuppressSameProceedReissue;

    bool ICanonicalNonCombatSceneState.AllowsFastForegroundWait => EventForegroundOwned
        && ReleaseStage == EventReleaseStage.Active
        && (ExplicitAction is EventExplicitActionKind.Proceed
            or EventExplicitActionKind.AncientDialogue
            or EventExplicitActionKind.AncientCompletion
            or EventExplicitActionKind.AncientOption);

    string? ICanonicalNonCombatSceneState.ForegroundDebugKind => RewardSubstateActive
        ? "reward"
        : ExplicitAction switch
        {
            EventExplicitActionKind.AncientDialogue => "ancient-event-dialogue",
            EventExplicitActionKind.AncientCompletion => "ancient-event-completion",
            EventExplicitActionKind.AncientOption => "ancient-event-options",
            EventExplicitActionKind.AncientOptionContractMismatch => "event-option-contract-mismatch",
            _ when EventForegroundOwned && ReleaseStage == EventReleaseStage.ReleasePending => "event-release-pending",
            _ when EventForegroundOwned => "event",
            _ when MapForegroundOwned => "map",
            _ => null,
        };

    string? ICanonicalNonCombatSceneState.BackgroundDebugKind => RewardSubstateActive
        ? (RewardScene.LayerState.MapContextVisible ? "map" : null)
        : MapContextVisible
            ? "map"
            : null;
}

sealed record ShopSceneState(
    ShopRoomState ShopState,
    bool AlreadyPurchased) : ICanonicalNonCombatSceneState
{
    public NonCombatCanonicalForegroundOwner CanonicalForegroundOwner => ShopState.ForegroundOwned
        ? NonCombatCanonicalForegroundOwner.Shop
        : ShopState.TeardownInProgress || ShopState.MapIsCurrentActiveScreen
            ? NonCombatCanonicalForegroundOwner.Map
            : NonCombatCanonicalForegroundOwner.Unknown;

    public NonCombatReleaseStage ReleaseStage => ShopState.ForegroundOwned
        ? NonCombatReleaseStage.Active
        : ShopState.TeardownInProgress || ShopState.MapIsCurrentActiveScreen
            ? NonCombatReleaseStage.Released
            : NonCombatReleaseStage.None;

    public NonCombatHandoffTarget HandoffTarget => ShopState.ForegroundOwned
        ? NonCombatHandoffTarget.HandleShop
        : ShopState.TeardownInProgress || ShopState.MapIsCurrentActiveScreen
            ? NonCombatHandoffTarget.WaitMap
            : NonCombatHandoffTarget.None;

    public bool SuppressSameActionReissue => false;

    public bool AllowsFastForegroundWait => ShopState.ForegroundOwned;

    public string? ForegroundDebugKind => CanonicalForegroundOwner == NonCombatCanonicalForegroundOwner.Map
        ? "map"
        : ShopState.InventoryOpen ? "shop-inventory" : "shop";

    public string? BackgroundDebugKind => CanonicalForegroundOwner == NonCombatCanonicalForegroundOwner.Map
        ? ShopState.RoomVisible ? "shop" : null
        : ShopState.MapIsCurrentActiveScreen ? "map" : null;
}

sealed record RestSiteSceneState(
    bool ExplicitChoiceVisible,
    bool SmithUpgradeActive,
    bool SmithConfirmVisible,
    bool ProceedVisible,
    bool SelectionSettling,
    bool SelectionAcceptedRecently,
    bool AftermathResiduePresent,
    bool MapOverlayResiduePresent,
    bool MapContextVisible) : ICanonicalNonCombatSceneState
{
    public NonCombatCanonicalForegroundOwner CanonicalForegroundOwner => NonCombatCanonicalForegroundOwner.RestSite;

    public NonCombatReleaseStage ReleaseStage => SelectionSettling
                                                 || ExplicitChoiceVisible
                                                 || SmithUpgradeActive
                                                 || ProceedVisible
        ? NonCombatReleaseStage.Active
        : SelectionAcceptedRecently
            ? NonCombatReleaseStage.ReleasePending
            : NonCombatReleaseStage.None;

    public NonCombatHandoffTarget HandoffTarget => NonCombatHandoffTarget.ChooseFirstNode;

    public bool SuppressSameActionReissue => false;

    public bool AllowsFastForegroundWait => true;

    public string? ForegroundDebugKind => SmithUpgradeActive
        ? SmithConfirmVisible ? "rest-site-smith-confirm" : "rest-site-smith-grid"
        : SelectionSettling
            ? "rest-site-selection-settling"
        : ReleaseStage == NonCombatReleaseStage.ReleasePending
            ? "rest-site-release-pending"
            : "rest-site";

    public string? BackgroundDebugKind => MapOverlayResiduePresent
        ? "map-overlay"
        : MapContextVisible ? "map" : null;
}

sealed record TreasureSceneState(
    TreasureRoomSubtypeState TreasureState) : ICanonicalNonCombatSceneState
{
    public NonCombatCanonicalForegroundOwner CanonicalForegroundOwner => NonCombatCanonicalForegroundOwner.Treasure;

    public NonCombatReleaseStage ReleaseStage => NonCombatReleaseStage.Active;

    public NonCombatHandoffTarget HandoffTarget => NonCombatHandoffTarget.ChooseFirstNode;

    public bool SuppressSameActionReissue => false;

    public bool AllowsFastForegroundWait => false;

    public string? ForegroundDebugKind => TreasureState.InspectOverlayVisible ? "treasure-overlay" : "treasure";

    public string? BackgroundDebugKind => null;
}

sealed record MapOverlayState(
    bool ForegroundVisible,
    bool EventBackgroundPresent,
    bool MapBackNavigationAvailable,
    bool StaleEventChoicePresent,
    bool CurrentNodeArrowVisible,
    bool ReachableNodeCandidatePresent,
    bool ExportedReachableNodeCandidatePresent);
