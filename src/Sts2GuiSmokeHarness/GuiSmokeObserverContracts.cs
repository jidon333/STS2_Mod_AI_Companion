using System.Text.Json;

sealed record ObserverSummary(
    string? CurrentScreen,
    string? VisibleScreen,
    bool? InCombat,
    DateTimeOffset? CapturedAt,
    string? InventoryId,
    bool? SceneReady,
    string? SceneAuthority,
    string? SceneStability,
    string? SceneEpisodeId,
    string? EncounterKind,
    string? ChoiceExtractorPath,
    int? PlayerCurrentHp,
    int? PlayerMaxHp,
    int? PlayerEnergy,
    IReadOnlyList<string> CurrentChoices,
    IReadOnlyList<string> LastEventsTail,
    IReadOnlyList<ObserverActionNode> ActionNodes,
    IReadOnlyList<ObserverChoice> Choices,
    IReadOnlyList<ObservedCombatHandCard> CombatHand)
{
    public long? SnapshotVersion { get; init; }

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

    public string? CompatibilityVisibleObservedScreen { get; init; }

    public bool? CompatibilitySceneReady { get; init; }

    public string? CompatibilitySceneAuthority { get; init; }

    public string? CompatibilitySceneStability { get; init; }

    public IReadOnlyDictionary<string, string?> Meta { get; init; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
}

sealed record ObserverActionNode(
    string NodeId,
    string Kind,
    string Label,
    string? ScreenBounds,
    bool Actionable)
{
    public string? TypeName { get; init; }

    public IReadOnlyList<string> SemanticHints { get; init; } = Array.Empty<string>();
}

sealed record ObserverChoice(
    string Kind,
    string Label,
    string? ScreenBounds,
    string? Value = null,
    string? Description = null)
{
    public string? NodeId { get; init; }

    public string? BindingKind { get; init; }

    public string? BindingId { get; init; }

    public bool? Enabled { get; init; }

    public string? IconAssetPath { get; init; }

    public IReadOnlyList<string> SemanticHints { get; init; } = Array.Empty<string>();
}

sealed record ObservedCombatHandCard(
    int SlotIndex,
    string Name,
    string? Type,
    int? Cost);

sealed record RestSiteObservedChoice(
    string OptionId,
    string TargetLabel,
    string CandidateLabel,
    string Label,
    string? Description,
    bool HasMetadata,
    bool IsEnabled,
    bool IsDefaultAutoPick,
    ObserverChoice? Choice,
    ObserverActionNode? ActionNode);

sealed record TreasureRoomSubtypeState(
    bool RoomDetected,
    bool ChestClickable,
    bool ChestOpened,
    int RelicHolderCount,
    int VisibleRelicHolderCount,
    int EnabledRelicHolderCount,
    bool ProceedEnabled,
    bool InspectOverlayVisible,
    IReadOnlyList<string> RelicHolderIds,
    string? RootType);

sealed record ShopRoomState(
    bool RoomDetected,
    bool RoomVisible,
    bool ForegroundOwned,
    bool TeardownInProgress,
    bool ShopIsCurrentActiveScreen,
    bool MapIsCurrentActiveScreen,
    string? ActiveScreenType,
    bool InventoryOpen,
    bool MerchantButtonVisible,
    bool MerchantButtonEnabled,
    bool ProceedEnabled,
    bool BackVisible,
    bool BackEnabled,
    int OptionCount,
    int AffordableOptionCount,
    IReadOnlyList<string> AffordableOptionIds,
    IReadOnlyList<string> AffordableRelicIds,
    IReadOnlyList<string> AffordableCardIds,
    IReadOnlyList<string> AffordablePotionIds,
    bool CardRemovalVisible,
    bool CardRemovalEnabled,
    bool CardRemovalEnoughGold,
    bool CardRemovalUsed,
    string? RootType);

sealed record RewardScreenState(
    bool ScreenDetected,
    bool ScreenVisible,
    bool ForegroundOwned,
    bool TeardownInProgress,
    bool RewardIsCurrentActiveScreen,
    bool RewardIsTopOverlay,
    bool MapIsCurrentActiveScreen,
    string? ActiveScreenType,
    bool ProceedVisible,
    bool ProceedEnabled,
    int VisibleButtonCount,
    int EnabledButtonCount,
    bool TerminalRunBoundary,
    bool GameOverScreenDetected,
    bool UnlockScreenDetected,
    bool TimelineUnlockDetected,
    bool MainMenuReturnDetected,
    string? RootType);

sealed record RestSiteDecisionCandidate(
    string Label,
    string Source,
    double Score,
    string? RejectReason,
    string? RawBounds,
    string? BoundsSource,
    GuiSmokeStepDecision? Decision);

sealed record RestSiteActionMetadata(
    string Kind,
    string TargetLabel,
    string Fingerprint);

enum RestSiteExplicitChoiceRepeatState
{
    None,
    GraceNeeded,
    NoOpDetected,
}

sealed record ObserverState(
    ObserverSummary Summary,
    JsonDocument? StateDocument,
    JsonDocument? InventoryDocument,
    string[]? EventLines)
{
    public string? CurrentScreen => Summary.CurrentScreen;
    public string? VisibleScreen => Summary.VisibleScreen;
    public string? RawCurrentScreen => Summary.RawCurrentScreen;
    public string? RawObservedScreen => Summary.RawObservedScreen;
    public string? PublishedCurrentScreen => Summary.PublishedCurrentScreen;
    public string? PublishedVisibleScreen => Summary.PublishedVisibleScreen;
    public bool? PublishedSceneReady => Summary.PublishedSceneReady;
    public string? PublishedSceneAuthority => Summary.PublishedSceneAuthority;
    public string? PublishedSceneStability => Summary.PublishedSceneStability;
    public string? CompatibilityCurrentScreen => Summary.CompatibilityCurrentScreen;
    public string? CompatibilityLogicalScreen => Summary.CompatibilityLogicalScreen;
    public string? CompatibilityVisibleScreen => Summary.CompatibilityVisibleScreen;
    public string? CompatibilityVisibleObservedScreen => Summary.CompatibilityVisibleObservedScreen;
    public bool? InCombat => Summary.InCombat;
    public string? InventoryId => Summary.InventoryId;
    public bool? SceneReady => Summary.SceneReady;
    public string? SceneAuthority => Summary.SceneAuthority;
    public string? SceneStability => Summary.SceneStability;
    public bool? CompatibilitySceneReady => Summary.CompatibilitySceneReady;
    public string? CompatibilitySceneAuthority => Summary.CompatibilitySceneAuthority;
    public string? CompatibilitySceneStability => Summary.CompatibilitySceneStability;
    public int? PlayerEnergy => Summary.PlayerEnergy;
    public IReadOnlyList<string> CurrentChoices => Summary.CurrentChoices;
    public IReadOnlyList<string> LastEventsTail => Summary.LastEventsTail;
    public IReadOnlyList<ObserverActionNode> ActionNodes => Summary.ActionNodes;
    public IReadOnlyList<ObserverChoice> Choices => Summary.Choices;
    public IReadOnlyList<ObservedCombatHandCard> CombatHand => Summary.CombatHand;
    public IReadOnlyDictionary<string, string?> Meta => Summary.Meta;
    public DateTimeOffset? CapturedAt => Summary.CapturedAt;
    public long? SnapshotVersion => Summary.SnapshotVersion;

    public bool IsFreshSince(DateTimeOffset threshold)
    {
        return CapturedAt is not null && CapturedAt >= threshold;
    }

    public string? EncounterKind => Summary.EncounterKind;
    public string? ChoiceExtractorPath => Summary.ChoiceExtractorPath;
}

enum NonCombatForegroundOwner
{
    Unknown,
    Combat,
    Reward,
    Shop,
    RestSite,
    Map,
    Event,
}

sealed record RootSceneTransitionState(
    bool TransitionInProgress,
    string? RootSceneCurrentType,
    bool RootSceneIsMainMenu,
    bool RootSceneIsRun,
    bool CurrentRunNodePresent,
    string? CurrentRunRoomType,
    string? CurrentRunRoomSceneType);

sealed record RestSitePostClickEvidence(
    string Classification,
    string? Outcome,
    string? OutcomeEvidence,
    string? CurrentStatus,
    string? CurrentOptionId,
    string? LastSignal,
    string? LastOptionId,
    bool UpgradeScreenVisible,
    bool ExplicitChoiceVisible,
    bool SmithGridVisible,
    bool SmithConfirmVisible,
    bool UpgradeChoiceObserverMiss);

sealed record CardSelectionSubtypeState(
    string ScreenType,
    string? Prompt,
    int? MinSelect,
    int? MaxSelect,
    int SelectedCount,
    bool? RequireManualConfirmation,
    bool? Cancelable,
    bool PreviewVisible,
    bool MainConfirmEnabled,
    bool PreviewConfirmEnabled,
    string? PreviewMode,
    IReadOnlyList<string> SelectedCardIds,
    string? RootType);
