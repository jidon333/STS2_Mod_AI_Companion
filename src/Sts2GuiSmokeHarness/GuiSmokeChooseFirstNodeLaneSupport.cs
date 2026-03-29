internal enum GuiSmokeChooseFirstNodeLane
{
    AncientMapPending,
    AncientMapForeground,
    RestSiteExplicitChoice,
    RestSiteSmithUpgrade,
    RestSiteProceed,
    TreasureRoom,
    ShopRoom,
    EventRecovery,
    MapOverlay,
    MapForeground,
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
        var eventScene = analysisContext?.EventScene
                         ?? AutoDecisionProvider.BuildEventSceneState(observer, windowBounds, history, screenshotPath);
        var mapOverlayState = analysisContext?.MapOverlayState
                              ?? GuiSmokeMapOverlayHeuristics.BuildState(observer, windowBounds, screenshotPath);
        var restSiteScene = AutoDecisionProvider.BuildRestSiteSceneState(observer);
        var explicitRestSiteChoiceAuthority = GuiSmokeNonCombatContractSupport.HasExplicitRestSiteChoiceAuthority(observer, screenshotPath);
        var restSiteSmithUpgradeActive = (restSiteScene?.SmithUpgradeActive ?? false)
                                         || (CardSelectionObserverSignals.IsUpgradeState(observer.Summary)
                                             && string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase));
        var restSiteProceedVisible = restSiteScene is
            {
                ProceedVisible: true,
                ExplicitChoiceVisible: false,
                SmithUpgradeActive: false,
            }
            || GuiSmokeNonCombatContractSupport.LooksLikeRestSiteProceedState(observer.Summary);
        var ancientMapOwner = AncientEventObserverSignals.IsMapForegroundOwner(observer.Summary);
        var ancientMapSurfacePending = AncientEventObserverSignals.IsMapSurfacePending(observer.Summary);
        var explicitEventRecoveryAuthority = !GuiSmokeNonCombatAllowedActionSupport.LooksLikeInspectOverlayState(observer)
                                             && AutoDecisionProvider.HasExplicitEventRecoveryAuthority(observer, windowBounds, history, eventScene);

        if (ancientMapOwner && ancientMapSurfacePending)
        {
            return GuiSmokeChooseFirstNodeLane.AncientMapPending;
        }

        if (ancientMapOwner)
        {
            return GuiSmokeChooseFirstNodeLane.AncientMapForeground;
        }

        if (explicitRestSiteChoiceAuthority)
        {
            return GuiSmokeChooseFirstNodeLane.RestSiteExplicitChoice;
        }

        if (restSiteSmithUpgradeActive)
        {
            return GuiSmokeChooseFirstNodeLane.RestSiteSmithUpgrade;
        }

        if (restSiteProceedVisible)
        {
            return GuiSmokeChooseFirstNodeLane.RestSiteProceed;
        }

        if (TreasureRoomObserverSignals.TryGetState(observer.Summary) is { RoomDetected: true })
        {
            return GuiSmokeChooseFirstNodeLane.TreasureRoom;
        }

        if (ShopObserverSignals.IsShopAuthorityActive(observer.Summary))
        {
            return GuiSmokeChooseFirstNodeLane.ShopRoom;
        }

        if (explicitEventRecoveryAuthority)
        {
            return GuiSmokeChooseFirstNodeLane.EventRecovery;
        }

        if (mapOverlayState.ForegroundVisible)
        {
            return GuiSmokeChooseFirstNodeLane.MapOverlay;
        }

        return GuiSmokeChooseFirstNodeLane.MapForeground;
    }
}
