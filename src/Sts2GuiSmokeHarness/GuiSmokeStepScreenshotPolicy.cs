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
            && !LooksLikeInspectOverlayState(observer))
        {
            if (cardSelectionState is not null)
            {
                return (false, "event-card-selection-explicit-authority");
            }

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
            if (AutoDecisionProvider.HasObserverOnlyRestSiteUpgradeAuthority(observer.Summary, analysisContext.WindowBounds))
            {
                return (false, "rest-site-upgrade-runtime");
            }

            if (GuiSmokeNonCombatContractSupport.HasExplicitRestSiteChoiceAuthority(observer, analysisContext.ScreenshotPath)
                || RestSiteObserverSignals.IsRestSiteSelectionSettlingState(observer.Summary)
                || GuiSmokeNonCombatContractSupport.LooksLikeRestSiteProceedState(observer.Summary)
                || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary)
                || ShopObserverSignals.IsShopAuthorityActive(observer.Summary))
            {
                return (false, "room-explicit-authority");
            }

            if (!LooksLikeInspectOverlayState(observer)
                && AutoDecisionProvider.HasExplicitEventRecoveryAuthority(
                    observer,
                    analysisContext.WindowBounds,
                    analysisContext.History,
                    analysisContext.EventScene))
            {
                return (false, "event-explicit-authority");
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

            var runtimeCombatState = analysisContext.RuntimeCombatState;
            if (runtimeCombatState.RequiresHandCardSelection)
            {
                return (false, "combat-hand-selection-runtime");
            }

            if (analysisContext.HasSelectedNonEnemyConfirmEvidence)
            {
                return (false, "combat-non-enemy-confirm-runtime");
            }

            if (observer.PlayerEnergy is <= 0)
            {
                return (false, "combat-no-energy");
            }

            if (analysisContext.CanResolveCombatEnemyTarget)
            {
                return (false, "combat-explicit-target-runtime");
            }

            if (HasPlayableObservedAttackSlot(observer.Summary, analysisContext.CombatCardKnowledge))
            {
                return (false, "combat-observer-attack-slot");
            }

            if (HasPlayableObservedNonEnemySlot(observer.Summary, analysisContext.CombatCardKnowledge))
            {
                return (false, "combat-observer-non-enemy-slot");
            }
        }

        return (true, "screenshot-required");
    }

    private static bool HasPlayableObservedAttackSlot(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var hasKnowledgeAttack = combatCardKnowledge.Any(card =>
            card.SlotIndex is >= 1 and <= 5
            && (string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
                || string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
                || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase)
                || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase))
            && IsPlayableKnowledgeCardAtCurrentEnergy(card, observer.PlayerEnergy));
        if (hasKnowledgeAttack)
        {
            return true;
        }

        return observer.CombatHand.Any(card =>
            card.SlotIndex is >= 1 and <= 5
            && (string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
                || card.Name.Contains("STRIKE", StringComparison.OrdinalIgnoreCase)
                || card.Name.Contains("BASH", StringComparison.OrdinalIgnoreCase))
            && IsPlayableObservedCardAtCurrentEnergy(card, observer.PlayerEnergy, combatCardKnowledge));
    }

    private static bool HasPlayableObservedNonEnemySlot(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        return combatCardKnowledge.Any(card =>
                   card.SlotIndex is >= 1 and <= 5
                   && CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatCard(card, observer.PlayerEnergy))
               || observer.CombatHand.Any(card =>
                   card.SlotIndex is >= 1 and <= 5
                   && CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatHandCard(card, observer.PlayerEnergy, combatCardKnowledge));
    }

    private static bool IsPlayableKnowledgeCardAtCurrentEnergy(CombatCardKnowledgeHint card, int? energy)
    {
        if (energy is null || card.Cost is null)
        {
            return true;
        }

        if (card.Cost < 0)
        {
            return energy > 0;
        }

        return card.Cost <= energy;
    }

    private static bool IsPlayableObservedCardAtCurrentEnergy(
        ObservedCombatHandCard card,
        int? energy,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var resolvedCost = card.Cost
                           ?? combatCardKnowledge.FirstOrDefault(candidate => candidate.SlotIndex == card.SlotIndex)?.Cost;
        if (energy is null || resolvedCost is null)
        {
            return true;
        }

        if (resolvedCost < 0)
        {
            return energy > 0;
        }

        return resolvedCost <= energy;
    }
}
