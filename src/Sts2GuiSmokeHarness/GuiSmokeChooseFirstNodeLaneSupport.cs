internal enum GuiSmokeChooseFirstNodeLane
{
    Unknown,
    CombatTakeover,
    RewardForeground,
    EventForeground,
    RestSiteExplicitChoice,
    RestSiteSmithUpgrade,
    RestSiteProceed,
    RestSiteSelectionSettling,
    TreasureRoom,
    ShopRoom,
    MapSurfacePending,
    MapOverlay,
    MapForeground,
    ContractMismatch,
}

internal static class GuiSmokeChooseFirstNodeLaneSupport
{
    public static GuiSmokeChooseFirstNodeLane Resolve(
        ObserverState observer,
        WindowBounds? windowBounds,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        GuiSmokeStepAnalysisContext? analysisContext = null)
    {
        var handoffState = analysisContext?.PostNodeHandoffState
                           ?? AutoDecisionProvider.BuildPostNodeHandoffState(observer, windowBounds, history, screenshotPath);

        return handoffState.Owner switch
        {
            NonCombatCanonicalForegroundOwner.Combat => GuiSmokeChooseFirstNodeLane.CombatTakeover,
            NonCombatCanonicalForegroundOwner.Reward => GuiSmokeChooseFirstNodeLane.RewardForeground,
            NonCombatCanonicalForegroundOwner.Event => GuiSmokeChooseFirstNodeLane.EventForeground,
            NonCombatCanonicalForegroundOwner.Shop => GuiSmokeChooseFirstNodeLane.ShopRoom,
            NonCombatCanonicalForegroundOwner.RestSite => handoffState.SurfaceKind switch
            {
                PostNodeHandoffSurfaceKind.RestSiteSmithUpgrade => GuiSmokeChooseFirstNodeLane.RestSiteSmithUpgrade,
                PostNodeHandoffSurfaceKind.RestSiteProceed => GuiSmokeChooseFirstNodeLane.RestSiteProceed,
                PostNodeHandoffSurfaceKind.RestSiteSelectionSettling => GuiSmokeChooseFirstNodeLane.RestSiteSelectionSettling,
                _ => GuiSmokeChooseFirstNodeLane.RestSiteExplicitChoice,
            },
            NonCombatCanonicalForegroundOwner.Treasure => GuiSmokeChooseFirstNodeLane.TreasureRoom,
            NonCombatCanonicalForegroundOwner.Map when handoffState.SurfaceKind == PostNodeHandoffSurfaceKind.MapSurfacePending
                => GuiSmokeChooseFirstNodeLane.MapSurfacePending,
            NonCombatCanonicalForegroundOwner.Map when !handoffState.HasExplicitSurface
                => GuiSmokeChooseFirstNodeLane.MapSurfacePending,
            NonCombatCanonicalForegroundOwner.Map when handoffState.SurfaceKind == PostNodeHandoffSurfaceKind.MapOverlay => GuiSmokeChooseFirstNodeLane.MapOverlay,
            NonCombatCanonicalForegroundOwner.Map => GuiSmokeChooseFirstNodeLane.MapForeground,
            _ when handoffState.ContractMismatch => GuiSmokeChooseFirstNodeLane.ContractMismatch,
            _ => GuiSmokeChooseFirstNodeLane.Unknown,
        };
    }
}
