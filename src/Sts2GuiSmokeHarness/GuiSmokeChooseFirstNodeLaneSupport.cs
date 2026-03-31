internal enum GuiSmokeChooseFirstNodeLane
{
    Unknown,
    RewardForeground,
    EventForeground,
    RestSiteExplicitChoice,
    RestSiteSmithUpgrade,
    RestSiteProceed,
    RestSiteSelectionSettling,
    TreasureRoom,
    ShopRoom,
    MapPending,
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
            NonCombatCanonicalForegroundOwner.Map when !handoffState.HasExplicitSurface => GuiSmokeChooseFirstNodeLane.MapPending,
            NonCombatCanonicalForegroundOwner.Map when handoffState.SurfaceKind == PostNodeHandoffSurfaceKind.MapOverlay => GuiSmokeChooseFirstNodeLane.MapOverlay,
            NonCombatCanonicalForegroundOwner.Map => GuiSmokeChooseFirstNodeLane.MapForeground,
            _ when handoffState.ContractMismatch => GuiSmokeChooseFirstNodeLane.ContractMismatch,
            _ => GuiSmokeChooseFirstNodeLane.Unknown,
        };
    }
}
