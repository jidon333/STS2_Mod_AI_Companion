using static GuiSmokeNonCombatAllowedActionSupport;

static class GuiSmokeStepScreenshotPolicy
{
    public static (bool NeedsScreenshot, string SkipReason) Evaluate(
        int stepIndex,
        bool isAuthoritativeFirstAttempt,
        GuiSmokeStepAnalysisContext analysisContext)
    {
        var observer = analysisContext.Observer;
        var cardSelectionState = CardSelectionObserverSignals.TryGetState(observer.Summary);

        if (stepIndex == 1 && isAuthoritativeFirstAttempt)
        {
            return (true, "authoritative-first-attempt");
        }

        if (analysisContext.Phase is GuiSmokePhase.WaitMainMenu
            or GuiSmokePhase.WaitRunLoad
            or GuiSmokePhase.WaitCharacterSelect
            or GuiSmokePhase.WaitMap
            or GuiSmokePhase.WaitPostMapNodeRoom
            or GuiSmokePhase.WaitCombat
            or GuiSmokePhase.WaitEventRelease
            or GuiSmokePhase.EnterRun
            or GuiSmokePhase.ChooseCharacter
            or GuiSmokePhase.Embark)
        {
            return (false, "observer-phase");
        }

        if (analysisContext.Phase == GuiSmokePhase.HandleRewards && analysisContext.UseRewardFastPath)
        {
            return (false, "reward-fast-path");
        }

        if (analysisContext.Phase == GuiSmokePhase.HandleShop
            && cardSelectionState is null
            && ShopObserverSignals.IsShopAuthorityActive(observer.Summary))
        {
            return (false, "shop-explicit-authority");
        }

        if (analysisContext.Phase == GuiSmokePhase.HandleEvent
            && cardSelectionState is null
            && !LooksLikeInspectOverlayState(observer))
        {
            var eventScene = analysisContext.EventScene;
            if (eventScene.RewardSubstateActive
                || (eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.ReleasePending)
                || (eventScene.EventForegroundOwned && eventScene.HasExplicitProgression)
                || AncientEventObserverSignals.IsDialogueActive(observer.Summary)
                || AncientEventObserverSignals.HasExplicitCompletionAction(observer.Summary)
                || AncientEventObserverSignals.HasExplicitOptionSelection(observer.Summary)
                || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary)
                || EventProceedObserverSignals.HasExplicitEventProceedAuthority(observer.Summary, analysisContext.WindowBounds))
            {
                return (false, "event-explicit-authority");
            }
        }

        if (analysisContext.Phase == GuiSmokePhase.ChooseFirstNode)
        {
            if (GuiSmokeNonCombatContractSupport.HasExplicitRestSiteChoiceAuthority(observer, analysisContext.ScreenshotPath)
                || GuiSmokeNonCombatContractSupport.LooksLikeRestSiteProceedState(observer.Summary)
                || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary)
                || ShopObserverSignals.IsShopAuthorityActive(observer.Summary))
            {
                return (false, "room-explicit-authority");
            }

            var mapOverlayState = analysisContext.MapOverlayState;
            if (mapOverlayState.ExportedReachableNodeCandidatePresent)
            {
                return (false, "map-exported-node");
            }

            if (!mapOverlayState.ReachableNodeCandidatePresent && mapOverlayState.MapBackNavigationAvailable)
            {
                return (false, "map-overlay-back-only");
            }
        }

        if (analysisContext.Phase == GuiSmokePhase.HandleCombat && analysisContext.UseCombatFastPath)
        {
            if (analysisContext.CombatPlayerActionWindowClosed)
            {
                return (false, "combat-player-window-closed");
            }

            var combatBarrier = analysisContext.CombatBarrierEvaluation;
            if (combatBarrier.IsActive && combatBarrier.IsHardWaitBarrier)
            {
                return (false, "combat-hard-wait-barrier");
            }
        }

        return (true, "screenshot-required");
    }
}
