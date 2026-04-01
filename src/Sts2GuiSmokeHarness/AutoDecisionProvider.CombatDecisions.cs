using System.Drawing;
using System.Globalization;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

sealed partial class AutoDecisionProvider
{
    private static GuiSmokeStepDecision DecideHandleCombat(GuiSmokeStepRequest request, GuiSmokeStepAnalysisContext? analysisContext = null)
    {
        var context = analysisContext ?? GuiSmokeStepAnalysisContext.CreateForHandleCombatRequest(request);
        var combatReleaseState = context.CombatReleaseState;
        var combatContext = context.CombatContext;
        var historyPendingSelection = CombatHistorySupport.TryGetPendingCombatSelection(combatContext.CombatHistory);
        var pendingSelection = context.PendingCombatSelection;
        var runtimeCombatState = context.RuntimeCombatState;
        var combatBarrier = context.CombatBarrierEvaluation;
        var combatMicroStage = context.CombatMicroStage;
        if (HasReleasedCombatOwnership(combatReleaseState))
        {
            if (ShouldAbortReleasedCombatPhase(combatReleaseState))
            {
                return CreateCombatReleaseAbortDecision(
                    request,
                    BuildCombatReleaseMismatchReason(combatReleaseState));
            }

            return CreatePhaseWaitDecision(
                GuiSmokePhase.HandleCombat,
                BuildCombatReleaseWaitReason(combatReleaseState),
                DisplayControlFlowScreen(request.Observer));
        }

        if (context.CombatPlayerActionWindowClosed)
        {
            return CreatePhaseWaitDecision(GuiSmokePhase.HandleCombat, "observer reports enemy turn or a closed combat play phase", DisplayControlFlowScreen(request.Observer));
        }

        if (combatBarrier.IsActive && combatBarrier.IsHardWaitBarrier)
        {
            return CreateCombatBarrierWaitDecision(combatBarrier, DisplayControlFlowScreen(request.Observer));
        }

        if (CardSelectionObserverSignals.TryGetState(request.Observer) is not null)
        {
            if (TryUseCombatDecision(TryCreateCardSelectionDecision(request), out var allowedCardSelectionDecision))
            {
                return allowedCardSelectionDecision;
            }

            return CreatePhaseWaitDecision(
                GuiSmokePhase.HandleCombat,
                "waiting for explicit combat card-selection overlay action",
                DisplayControlFlowScreen(request.Observer));
        }

        var hasSelectedNonEnemyConfirmEvidence = context.HasSelectedNonEnemyConfirmEvidence;
        var enemyTargetOpportunity = context.CanResolveCombatEnemyTarget;
        var combatNoOpCountsBySlot = combatContext.CombatNoOpCountsBySlot;
        var observerHasAttackCard = request.Observer.CombatHand.Any(card =>
            card.SlotIndex >= 1
            && card.SlotIndex <= 5
            && IsAttackCombatHandCard(card)
            && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge));
        var observerHandHasOnlyNonEnemyOrInertCards = request.Observer.CombatHand.Count > 0
            && request.Observer.CombatHand.All(card =>
                IsNonEnemyCombatHandCard(card)
                || IsInertCombatHandCard(card)
                || !IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge));
        var hasExplicitCombatActionabilityTruth = request.Observer.CombatHand.Count > 0
                                                || request.CombatCardKnowledge.Count > 0
                                                || runtimeCombatState.PendingSelection is not null
                                                || runtimeCombatState.RequiresHandCardSelection
                                                || runtimeCombatState.CardPlayPending == true
                                                || runtimeCombatState.TargetingInProgress == true
                                                || runtimeCombatState.HasExplicitEnemyTargetingEvidence;
        var hardBlockedAttackSlots = combatNoOpCountsBySlot
            .Where(static pair => pair.Value >= 2)
            .Select(static pair => pair.Key)
            .ToHashSet();
        var softBlockedAttackSlots = combatNoOpCountsBySlot
            .Where(static pair => pair.Value >= 1)
            .Select(static pair => pair.Key)
            .ToHashSet();
        var alternatePlayableAttackSlots = request.CombatCardKnowledge
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .Where(card => IsEnemyTargetCombatCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy))
            .Select(static card => card.SlotIndex)
            .Concat(request.Observer.CombatHand
                .Where(card => card.SlotIndex is >= 1 and <= 5)
                .Where(card => IsAttackCombatHandCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge))
                .Select(static card => card.SlotIndex))
            .Distinct()
            .Where(slotIndex => pendingSelection?.SlotIndex is null || slotIndex != pendingSelection.SlotIndex)
            .Where(slotIndex => !hardBlockedAttackSlots.Contains(slotIndex))
            .OrderBy(static slotIndex => slotIndex)
            .ToArray();
        var pendingSelectionNoOpCount = pendingSelection?.Kind == AutoCombatCardKind.AttackLike && pendingSelection.SlotIndex is >= 1 and <= 5
            ? HandleCombatContextSupport.GetCombatNoOpCountForSlot(combatContext, pendingSelection.SlotIndex)
            : 0;
        bool ShouldSuppressPendingNonEnemyReselect(int slotIndex)
        {
            if (pendingSelection?.Kind == AutoCombatCardKind.DefendLike
                && hasSelectedNonEnemyConfirmEvidence)
            {
                return true;
            }

            if (pendingSelection?.Kind == AutoCombatCardKind.DefendLike
                && pendingSelection.SlotIndex == slotIndex
                && !hasSelectedNonEnemyConfirmEvidence)
            {
                return true;
            }

            return !hasSelectedNonEnemyConfirmEvidence
                   && HandleCombatContextSupport.HasRecentNonEnemySelection(combatContext, slotIndex);
        }

        bool TryUseCombatDecision(GuiSmokeStepDecision? candidate, out GuiSmokeStepDecision allowedDecision)
        {
            allowedDecision = default!;
            if (candidate is null || !CombatDecisionContract.IsAllowed(request, candidate, out _))
            {
                return false;
            }

            allowedDecision = candidate;
            return true;
        }

        GuiSmokeStepDecision CloseWithLegalCombatFallback()
        {
            if (!hasExplicitCombatActionabilityTruth)
            {
                return CreatePhaseWaitDecision(
                    GuiSmokePhase.HandleCombat,
                    "waiting for explicit combat actionability truth before ending the turn",
                    DisplayControlFlowScreen(request.Observer));
            }

            if (!combatMicroStage.AllowsEndTurn)
            {
                return CreatePhaseWaitDecision(GuiSmokePhase.HandleCombat, "waiting for legal combat action", DisplayControlFlowScreen(request.Observer));
            }

            var fallbackDecision = CreateCombatPressKeyDecision(
                "E",
                "auto-end turn",
                "No clear playable combat action remains in observer/runtime truth. End the turn.",
                0.88,
                200);
            if (TryUseCombatDecision(fallbackDecision, out var allowedFallback))
            {
                return allowedFallback;
            }

            return CreatePhaseWaitDecision(GuiSmokePhase.HandleCombat, "waiting for legal combat action", DisplayControlFlowScreen(request.Observer));
        }

        if (combatMicroStage.Kind == CombatMicroStageKind.AwaitingCardPlayConfirm
            && CombatRuntimeStateSupport.HasPositiveAttackConfirmEvidence(
                request.Observer,
                request.CombatCardKnowledge,
                pendingSelection))
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                    "act",
                    "confirm-attack-card",
                    null,
                    null,
                    null,
                    "confirm selected attack card",
                    "A targetless or multi-target combat card is selected. Confirm that play before starting another lane or ending the turn.",
                    0.84,
                    "combat",
                    150,
                    true,
                    null), out var allowedAttackConfirmDecision))
            {
                return allowedAttackConfirmDecision;
            }

            return CreatePhaseWaitDecision(GuiSmokePhase.HandleCombat, "waiting for selected combat card confirm lane to resolve", DisplayControlFlowScreen(request.Observer));
        }

        if (combatMicroStage.Kind == CombatMicroStageKind.ResolvingCardPlay)
        {
            return CreatePhaseWaitDecision(GuiSmokePhase.HandleCombat, "waiting for played combat action to resolve", DisplayControlFlowScreen(request.Observer));
        }

        if (request.Observer.PlayerEnergy is <= 0
            && combatMicroStage.AllowsEndTurn)
        {
            if (TryUseCombatDecision(CreateCombatPressKeyDecision(
                "E",
                "auto-end turn",
                "Observer reports no remaining energy. End the turn instead of retrying non-playable cards.",
                0.92,
                200), out var allowedNoEnergyDecision))
            {
                return allowedNoEnergyDecision;
            }
        }

        if (runtimeCombatState.RequiresHandCardSelection)
        {
            if (runtimeCombatState.HasSelectedHandCardForConfirmation
                && TryUseCombatDecision(CreateCombatHandSelectionConfirmDecision(), out var allowedHandSelectionConfirmDecision))
            {
                return allowedHandSelectionConfirmDecision;
            }

            if (TryUseCombatDecision(TryCreateCombatHandSelectionDecision(request), out var allowedHandSelectionDecision))
            {
                return allowedHandSelectionDecision;
            }

            return CreatePhaseWaitDecision(GuiSmokePhase.HandleCombat, "waiting for selectable combat hand card", DisplayControlFlowScreen(request.Observer));
        }

        if (combatMicroStage.Kind is CombatMicroStageKind.TurnClosing or CombatMicroStageKind.EnemyTurnClosed)
        {
            return CreatePhaseWaitDecision(GuiSmokePhase.HandleCombat, "waiting for combat turn transition to settle", DisplayControlFlowScreen(request.Observer));
        }

        var blockedOpenAttackSelection = pendingSelection?.Kind == AutoCombatCardKind.AttackLike
                                         && pendingSelection.SlotIndex is >= 1 and <= 5
                                         && CombatRuntimeStateSupport.HasSelectionToKeep(request.Observer, request.CombatCardKnowledge)
                                         && HandleCombatContextSupport.GetCombatNoOpCountForSlot(combatContext, pendingSelection.SlotIndex) >= 2;
        var unresolvedAttackSelectionChurn = pendingSelection is null
                                             && !runtimeCombatState.HasCardSelectionEvidence
                                             && !enemyTargetOpportunity
                                             && CombatHistorySupport.HasRecentAttackSelectionChurnWithoutResolution(combatContext.CombatHistory);
        var staleAttackSelectionTail = pendingSelection is null
                                       && CombatRuntimeStateSupport.HasResidualAttackSelectionTail(
                                           request.Observer,
                                           request.CombatCardKnowledge,
                                           historyPendingSelection,
                                           combatContext.CombatHistory);
        var allowsEndTurnFromStaleAttackTail = request.Observer.PlayerEnergy is <= 0
                                               && (combatMicroStage.AllowsEndTurn
                                                   || CombatRuntimeStateSupport.HasExplicitEndTurnAffordance(request.Observer, request.WindowBounds));
        if (combatMicroStage.Kind == CombatMicroStageKind.ResolvingNonEnemy)
        {
            if (hasSelectedNonEnemyConfirmEvidence
                && TryUseCombatDecision(new GuiSmokeStepDecision(
                    "act",
                    "confirm-non-enemy",
                    null,
                    null,
                    null,
                    "confirm selected non-enemy card",
                    "A non-enemy card is still selected. Confirm or clear that lane before starting a new combat action or ending the turn.",
                    0.82,
                    "combat",
                    150,
                    true,
                    null), out var allowedNonEnemyConfirmDecision))
            {
                return allowedNonEnemyConfirmDecision;
            }

            return CreatePhaseWaitDecision(GuiSmokePhase.HandleCombat, "waiting for selected non-enemy combat lane to resolve", DisplayControlFlowScreen(request.Observer));
        }

        if (combatMicroStage.Kind == CombatMicroStageKind.ResolvingAttackTarget)
        {
            if (pendingSelection?.Kind == AutoCombatCardKind.AttackLike
                && enemyTargetOpportunity)
            {
                if (TryCreateCombatEnemyTargetDecision(request, pendingSelection, pendingSelectionNoOpCount, alternatePlayableAttackSlots, out var targetDecision)
                    && TryUseCombatDecision(targetDecision, out var allowedTargetDecision))
                {
                    return allowedTargetDecision;
                }
            }

            if (blockedOpenAttackSelection
                && TryUseCombatDecision(new GuiSmokeStepDecision(
                    "act",
                    "right-click",
                    null,
                    null,
                    null,
                    "cancel unresolved selected card",
                    $"The selected attack lane {pendingSelection!.SlotIndex} is still open after repeated no-op outcomes. Cancel it before ending the turn or choosing another lane.",
                    0.84,
                    "combat",
                    250,
                    true,
                    null), out var allowedBlockedSelectionCancelDecision))
            {
                return allowedBlockedSelectionCancelDecision;
            }

            return CreatePhaseWaitDecision(GuiSmokePhase.HandleCombat, "waiting for selected combat lane to resolve", DisplayControlFlowScreen(request.Observer));
        }

        if (unresolvedAttackSelectionChurn)
        {
            if (allowsEndTurnFromStaleAttackTail
                && TryUseCombatDecision(CreateCombatPressKeyDecision(
                    "E",
                    "auto-end turn",
                    "Stale combat target diagnostics still suppress attack reentry, but observer/runtime truth reports no remaining energy and an explicit end-turn affordance. Close the turn instead of plateauing behind stale attack residue.",
                    0.91,
                    200), out var allowedStaleChurnEndTurnDecision))
            {
                return allowedStaleChurnEndTurnDecision;
            }

            return CreatePhaseWaitDecision(
                GuiSmokePhase.HandleCombat,
                "waiting for explicit combat selection truth before reopening another attack lane",
                DisplayControlFlowScreen(request.Observer));
        }

        if (staleAttackSelectionTail)
        {
            if (allowsEndTurnFromStaleAttackTail
                && TryUseCombatDecision(CreateCombatPressKeyDecision(
                    "E",
                    "auto-end turn",
                    "Stale attack-target residue still suppresses attack reentry, but observer/runtime truth reports no remaining energy and an explicit end-turn affordance. Close the turn instead of waiting for stale target diagnostics to disappear.",
                    0.92,
                    200), out var allowedStaleTailEndTurnDecision))
            {
                return allowedStaleTailEndTurnDecision;
            }

            return CreatePhaseWaitDecision(
                GuiSmokePhase.HandleCombat,
                "waiting for fresh combat selection truth before reopening an attack lane from stale target diagnostics",
                DisplayControlFlowScreen(request.Observer));
        }

        var knowledgeAttackSlot = request.CombatCardKnowledge
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => IsEnemyTargetCombatCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy))
            .Where(card => !hardBlockedAttackSlots.Contains(card.SlotIndex))
            .Where(card => !CombatBarrierSupport.SuppressesAttackSlot(combatBarrier, card.SlotIndex))
            .Where(card => pendingSelectionNoOpCount <= 0 || !softBlockedAttackSlots.Contains(card.SlotIndex) || card.SlotIndex != pendingSelection?.SlotIndex)
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var observerAttackSlot = request.Observer.CombatHand
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => IsAttackCombatHandCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge))
            .Where(card => !hardBlockedAttackSlots.Contains(card.SlotIndex))
            .Where(card => !CombatBarrierSupport.SuppressesAttackSlot(combatBarrier, card.SlotIndex))
            .Where(card => pendingSelectionNoOpCount <= 0 || !softBlockedAttackSlots.Contains(card.SlotIndex) || card.SlotIndex != pendingSelection?.SlotIndex)
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var attackFallbackBlockedByObserver = request.Observer.CombatHand.Count > 0
            && (observerHandHasOnlyNonEnemyOrInertCards || !observerHasAttackCard);
        var attackSlot = !attackFallbackBlockedByObserver && knowledgeAttackSlot is not null
            ? new AutoCombatHandSlotAnalysis(
                knowledgeAttackSlot.SlotIndex,
                true,
                AutoCombatCardKind.AttackLike,
                double.MaxValue,
                double.MaxValue,
                0,
                0)
            : !attackFallbackBlockedByObserver && observerAttackSlot is not null
                ? new AutoCombatHandSlotAnalysis(
                observerAttackSlot.SlotIndex,
                true,
                AutoCombatCardKind.AttackLike,
                double.MaxValue,
                double.MaxValue,
                0,
                0)
                : null;
        if (attackSlot is not null)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "press-key",
                GetCombatSlotHotkey(attackSlot.SlotIndex),
                null,
                null,
                $"combat select attack slot {attackSlot.SlotIndex}",
                hardBlockedAttackSlots.Contains(attackSlot.SlotIndex) || pendingSelectionNoOpCount > 0
                    ? "Recent combat history shows no board delta on another lane. Switch to a different explicit playable attack slot before trying to target the enemy again."
                    : "Observer/runtime truth still shows a playable explicit attack slot. Use the corresponding hotkey first, then target the enemy.",
                hardBlockedAttackSlots.Count == 0 && pendingSelectionNoOpCount == 0 ? 0.80 : 0.88,
                "combat",
                120,
                true,
                null), out var allowedAttackSlotDecision))
            {
                return allowedAttackSlotDecision;
            }
        }

        var knowledgeNonEnemySlot = request.CombatCardKnowledge
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatCard(card, request.Observer.PlayerEnergy))
            .Where(card => !CombatBarrierSupport.SuppressesNonEnemySlot(combatBarrier, card.SlotIndex))
            .Where(card => !ShouldSuppressPendingNonEnemyReselect(card.SlotIndex))
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var observerNonEnemySlot = request.Observer.CombatHand
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatHandCard(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge))
            .Where(card => !CombatBarrierSupport.SuppressesNonEnemySlot(combatBarrier, card.SlotIndex))
            .Where(card => !ShouldSuppressPendingNonEnemyReselect(card.SlotIndex))
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var nonEnemySlot = knowledgeNonEnemySlot is not null
            ? new AutoCombatHandSlotAnalysis(
                knowledgeNonEnemySlot.SlotIndex,
                true,
                AutoCombatCardKind.DefendLike,
                double.MinValue,
                double.MaxValue,
                0,
                0)
            : observerNonEnemySlot is not null
                ? new AutoCombatHandSlotAnalysis(
                observerNonEnemySlot.SlotIndex,
                true,
                AutoCombatCardKind.DefendLike,
                double.MinValue,
                double.MaxValue,
                0,
                0)
                : null;
        if (nonEnemySlot is not null)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "press-key",
                GetCombatSlotHotkey(nonEnemySlot.SlotIndex),
                null,
                null,
                $"combat select non-enemy slot {nonEnemySlot.SlotIndex}",
                "Only non-enemy cards remain in hand. Use the corresponding hotkey, then resolve the self or non-enemy confirmation.",
                0.74,
                "combat",
                120,
                true,
                null), out var allowedNonEnemySlotDecision))
            {
                return allowedNonEnemySlotDecision;
            }
        }

        if (blockedOpenAttackSelection)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "right-click",
                null,
                null,
                null,
                "cancel unresolved selected card",
                $"The selected attack lane {pendingSelection!.SlotIndex} is still open after repeated no-op outcomes. Cancel it before ending the turn or choosing another lane.",
                0.84,
                "combat",
                250,
                true,
                null), out var allowedBlockedSelectionCancelDecision))
            {
                return allowedBlockedSelectionCancelDecision;
            }
        }

        return CloseWithLegalCombatFallback();
    }

    private static GuiSmokeStepDecision? TryCreateCombatHandSelectionDecision(GuiSmokeStepRequest request)
    {
        var handSelectionCard = request.Observer.CombatHand
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .OrderBy(card => GetCombatHandSelectionPriority(card))
            .ThenBy(card => card.SlotIndex)
            .FirstOrDefault();
        if (handSelectionCard is null)
        {
            return null;
        }

        return new GuiSmokeStepDecision(
            "act",
            "press-key",
            GetCombatSlotHotkey(handSelectionCard.SlotIndex),
            null,
            null,
            $"combat select hand slot {handSelectionCard.SlotIndex}",
            "Combat runtime is waiting for a follow-up hand-card selection. Choose a card from the current hand instead of confirming or ending the turn.",
            0.84,
            "combat",
            120,
            true,
            null);
    }

    private static GuiSmokeStepDecision CreateCombatHandSelectionConfirmDecision()
    {
        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            GuiSmokeCombatConstants.HandSelectionConfirmNormalizedX,
            GuiSmokeCombatConstants.HandSelectionConfirmNormalizedY,
            "confirm selected hand card",
            "Combat runtime is waiting for the selected follow-up hand card to be confirmed. Click the explicit confirm button instead of reselecting the hand slot or ending the turn.",
            0.88,
            "combat",
            180,
            true,
            null);
    }

    private static int GetCombatHandSelectionPriority(ObservedCombatHandCard card)
    {
        if (string.Equals(card.Type, "Curse", StringComparison.OrdinalIgnoreCase)
            || string.Equals(card.Type, "Status", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (string.Equals(card.Type, "Skill", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (string.Equals(card.Type, "Power", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase))
        {
            return 3;
        }

        return 4;
    }

    private static bool TryCreateCombatEnemyTargetDecision(
        GuiSmokeStepRequest request,
        PendingCombatSelection? pendingSelection,
        int pendingSelectionNoOpCount,
        IReadOnlyList<int> alternatePlayableAttackSlots,
        out GuiSmokeStepDecision decision)
    {
        if (pendingSelection?.Kind != AutoCombatCardKind.AttackLike)
        {
            decision = default!;
            return false;
        }

        if (pendingSelectionNoOpCount >= 2)
        {
            if (alternatePlayableAttackSlots.Count > 0)
            {
                decision = new GuiSmokeStepDecision(
                    "act",
                    "press-key",
                    GetCombatSlotHotkey(alternatePlayableAttackSlots[0]),
                    null,
                    null,
                    $"combat select attack slot {alternatePlayableAttackSlots[0]}",
                    $"Recent combat history shows no board delta after targeting from slot {pendingSelection.SlotIndex}. Switch to another playable attack lane before trying to target the enemy again.",
                    0.91,
                    "combat",
                    120,
                    true,
                    null);
                return true;
            }

            decision = default!;
            return false;
        }

        if (TryCreateCombatEnemyTargetDecisionFromObservedNodes(request, pendingSelection, pendingSelectionNoOpCount, out decision))
        {
            return true;
        }

        decision = CreatePhaseWaitDecision(
            GuiSmokePhase.HandleCombat,
            CombatTargetabilitySupport.DescribeMissingCombatEnemyTargetDecisionSource(request.Observer, request.WindowBounds),
            "combat");
        return true;
    }

    private static bool TryCreateCombatEnemyTargetDecisionFromObservedNodes(
        GuiSmokeStepRequest request,
        PendingCombatSelection pendingSelection,
        int pendingSelectionNoOpCount,
        out GuiSmokeStepDecision decision)
    {
        var targetNodes = GetCombatEnemyTargetNodes(request.Observer, request.WindowBounds);
        if (targetNodes.Count == 0)
        {
            decision = default!;
            return false;
        }

        var targetNode = pendingSelectionNoOpCount == 0
            ? targetNodes[0]
            : targetNodes[Math.Clamp(pendingSelectionNoOpCount, 0, targetNodes.Count - 1)];
        var targetLabel = BuildCombatEnemyTargetLabel(targetNode, pendingSelectionNoOpCount);
        decision = CreateCombatEnemyTargetDecisionFromNode(request, targetNode, targetLabel, pendingSelectionNoOpCount);
        return true;
    }

    private static bool CanResolveEnemyTargetFromCurrentState(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        AutoCombatAnalysis analysis,
        PendingCombatSelection? pendingSelection)
    {
        return CombatRuntimeStateSupport.CanResolveEnemyTarget(observer, combatCardKnowledge, pendingSelection, analysis);
    }

    public static CombatNoOpLoopAnalysis PeekCombatNoOpLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return AnalyzeCombatNoOpLoop(history);
    }

    public static PendingCombatSelection? TryPeekPendingCombatSelection(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return CombatHistorySupport.TryGetPendingCombatSelection(history);
    }

    public static bool IsCombatNoOpSensitiveTarget(string? targetLabel)
    {
        return targetLabel is not null
               && (targetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(targetLabel, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase));
    }

    public static string? ResolveCombatLaneLabel(string? targetLabel, IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return CombatHistorySupport.ResolveCombatLaneLabel(targetLabel, history);
    }

    private static int? ExtractCombatLaneSlotIndex(string? targetLabel)
    {
        return CombatHistorySupport.ExtractCombatLaneSlotIndex(targetLabel);
    }

    public static IReadOnlyDictionary<int, int> GetCombatNoOpCountsBySlot(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return CombatHistorySupport.GetCombatNoOpCountsBySlot(history);
    }

    private static int GetCombatNoOpCountForSlot(IReadOnlyList<GuiSmokeHistoryEntry> history, int slotIndex)
    {
        return HandleCombatContextSupport.GetCombatNoOpCountForSlot(HandleCombatContextSupport.Reconstruct(history), slotIndex);
    }

    private static CombatNoOpLoopAnalysis AnalyzeCombatNoOpLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return HandleCombatContextSupport.Reconstruct(history).CombatNoOpLoop;
    }

    private static bool IsNonEnemySelectionLabel(string? targetLabel)
    {
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return false;
        }

        return targetLabel.StartsWith("combat select non-enemy slot ", StringComparison.OrdinalIgnoreCase)
               || targetLabel.StartsWith("combat select defend slot ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParsePendingCombatSelection(string targetLabel, out PendingCombatSelection? selection)
    {
        return CombatHistorySupport.TryParsePendingCombatSelection(targetLabel, out selection);
    }

    private static int? ExtractFirstDigit(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        foreach (var character in value)
        {
            if (char.IsDigit(character))
            {
                return character - '0';
            }
        }

        return null;
    }

    private static string GetCombatSlotHotkey(int slotIndex)
    {
        return slotIndex == 10
            ? "0"
            : slotIndex.ToString(CultureInfo.InvariantCulture);
    }

    internal static bool HasReleasedCombatOwnership(CombatReleaseState releaseState)
    {
        return releaseState.HasReleasedOwnership;
    }

    internal static bool HasReleasedCombatOwnership(PostNodeHandoffState handoffState)
    {
        return handoffState.Owner != NonCombatCanonicalForegroundOwner.Combat
               && handoffState.HandoffTarget is not NonCombatHandoffTarget.None and not NonCombatHandoffTarget.HandleCombat;
    }

    internal static bool ShouldAbortReleasedCombatPhase(CombatReleaseState releaseState)
    {
        return releaseState.HasExplicitForegroundSurface
               || releaseState.ReleaseMismatch
               || releaseState.ReleaseTarget is NonCombatHandoffTarget.HandleRewards
                   or NonCombatHandoffTarget.HandleEvent
                   or NonCombatHandoffTarget.HandleShop
                   or NonCombatHandoffTarget.ChooseFirstNode;
    }

    internal static string BuildCombatReleaseWaitReason(CombatReleaseState releaseState)
    {
        var subtypePrefix = releaseState.ReleaseSubtype switch
        {
            CombatReleaseSubtype.EnemyClickResidue => "waiting for combat release after EnemyClick residue",
            CombatReleaseSubtype.EndTurnReopenLatency => "waiting for combat release after EndTurn reopen latency",
            _ => "waiting for combat release",
        };

        return releaseState.ReleaseTarget switch
        {
            NonCombatHandoffTarget.WaitEventRelease
                => $"{subtypePrefix} into event release handoff",
            NonCombatHandoffTarget.WaitMap when releaseState.HandoffState.SurfaceKind == PostNodeHandoffSurfaceKind.MapSurfacePending
                => $"{subtypePrefix} into post-room map surface republish",
            NonCombatHandoffTarget.WaitMap
                => $"{subtypePrefix} into map handoff",
            _ => $"{subtypePrefix} into {FormatCombatResolutionOwner(releaseState.ForegroundOwner)} foreground handoff",
        };
    }

    internal static string BuildCombatReleaseMismatchReason(CombatReleaseState releaseState)
    {
        return $"combat release failure under noncombat foreground: barrier={releaseState.BarrierKind} subtype={releaseState.ReleaseSubtype} owner={FormatCombatResolutionOwner(releaseState.ForegroundOwner)} target={releaseState.ReleaseTarget} surface={releaseState.HandoffState.SurfaceKind}";
    }

    private static GuiSmokeStepDecision CreateCombatReleaseAbortDecision(
        GuiSmokeStepRequest request,
        string reason)
    {
        return new GuiSmokeStepDecision(
            "abort",
            null,
            null,
            null,
            null,
            null,
            reason,
            0.0,
            ResolveObserverScreen(request.Observer),
            null,
            true,
            reason,
            null,
            null,
            "combat-release-failure-under-noncombat-foreground");
    }

    private static string FormatCombatResolutionOwner(NonCombatCanonicalForegroundOwner owner)
    {
        return owner switch
        {
            NonCombatCanonicalForegroundOwner.RestSite => "rest-site",
            _ => owner.ToString().ToLowerInvariant(),
        };
    }

    private static ObserverActionNode? FindEndTurnActionNode(GuiSmokeStepRequest request)
    {
        return FindEndTurnActionNode(request.Observer, request.WindowBounds);
    }

    private static ObserverActionNode? FindEndTurnActionNode(ObserverSummary observer, WindowBounds? windowBounds = null)
    {
        return observer.ActionNodes.FirstOrDefault(node =>
            node.Actionable
            && (string.Equals(node.Label, "1턴 종료", StringComparison.OrdinalIgnoreCase)
                || node.Label.Contains("턴 종료", StringComparison.OrdinalIgnoreCase)
                || node.Label.Contains("End Turn", StringComparison.OrdinalIgnoreCase))
            && (windowBounds is null
                ? TryParseNodeBounds(node.ScreenBounds, out _)
                : HasActiveNodeBounds(node.ScreenBounds, windowBounds)));
    }

    private static IReadOnlyList<ObserverActionNode> GetCombatEnemyTargetNodes(ObserverSummary observer, WindowBounds? windowBounds = null)
    {
        return CombatTargetabilitySupport.GetCombatEnemyTargetNodes(observer, windowBounds)
            .Where(node => windowBounds is null
                ? TryParseNodeBounds(node.ScreenBounds, out _)
                : HasActiveNodeBounds(node.ScreenBounds, windowBounds))
            .OrderBy(static node => GetNodeSortX(node))
            .ThenBy(static node => GetNodeSortY(node))
            .ToArray();
    }

    private static bool IsCombatEnemyTargetNode(ObserverActionNode node)
    {
        return string.Equals(node.Kind, "enemy-target", StringComparison.OrdinalIgnoreCase)
               || node.NodeId.StartsWith("enemy-target:", StringComparison.OrdinalIgnoreCase)
               || string.Equals(node.TypeName, "enemy-target", StringComparison.OrdinalIgnoreCase)
               || node.SemanticHints.Any(static hint =>
                   string.Equals(hint, "combat-targetable", StringComparison.OrdinalIgnoreCase)
                   || hint.StartsWith("target-id:", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsAttackCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("STRIKE", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("BASH", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEnemyTargetCombatCard(CombatCardKnowledgeHint card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Skill", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Type, "Power", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("DEFEND", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInertCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Status", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Type, "Curse", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyCombatCard(CombatCardKnowledgeHint card)
    {
        if (string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
            || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Equals(card.Type, "Power", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "Self", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "None", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllAllies", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyAlly", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlayableAtCurrentEnergy(ObservedCombatHandCard card, int? energy)
    {
        if (IsInertCombatHandCard(card) && card.Cost is null)
        {
            return false;
        }

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

    private static bool IsPlayableAtCurrentEnergy(
        ObservedCombatHandCard card,
        int? energy,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var resolvedCost = ResolveCombatCardCost(card, combatCardKnowledge);
        return IsPlayableAtCurrentEnergy(card with { Cost = resolvedCost }, energy);
    }

    private static bool IsPlayableAtCurrentEnergy(CombatCardKnowledgeHint card, int? energy)
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

    private static int? ResolveCombatCardCost(ObservedCombatHandCard card, IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        if (card.Cost is not null)
        {
            return card.Cost;
        }

        var slotMatch = combatCardKnowledge.FirstOrDefault(candidate => candidate.SlotIndex == card.SlotIndex);
        if (slotMatch?.Cost is not null)
        {
            return slotMatch.Cost;
        }

        var cardKeys = BuildCombatKnowledgeLookupKeysForCombat(card.Name);
        if (cardKeys.Count == 0)
        {
            return null;
        }

        return combatCardKnowledge
            .Where(candidate => candidate.Cost is not null)
            .Where(candidate =>
            {
                var candidateKeys = BuildCombatKnowledgeLookupKeysForCombat(candidate.Name);
                return candidateKeys.Any(cardKeys.Contains);
            })
            .Select(static candidate => candidate.Cost)
            .FirstOrDefault();
    }

    private static IReadOnlyList<string> BuildCombatKnowledgeLookupKeysForCombat(string? cardName)
    {
        if (string.IsNullOrWhiteSpace(cardName))
        {
            return Array.Empty<string>();
        }

        var keys = new List<string>();
        var normalizedName = NormalizeCombatLookupKey(cardName);
        AddCombatLookupKey(keys, normalizedName);

        var parts = cardName
            .Split(new[] { '.', '_', '-', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeCombatLookupKey)
            .Where(static part => part.Length > 0)
            .ToArray();
        if (parts.Length == 0)
        {
            return keys;
        }

        var trimmedParts = parts[0] == "card"
            ? parts[1..]
            : parts;
        if (trimmedParts.Length == 0)
        {
            return keys;
        }

        AddCombatLookupKey(keys, string.Concat(trimmedParts));
        AddCombatLookupKey(keys, trimmedParts[0]);
        if (trimmedParts.Length > 1 && IsCombatLookupClassSuffix(trimmedParts[^1]))
        {
            AddCombatLookupKey(keys, string.Concat(trimmedParts[..^1]));
        }

        foreach (var part in trimmedParts)
        {
            AddCombatLookupKey(keys, part);
        }

        return keys;
    }

    private static void AddCombatLookupKey(List<string> keys, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate)
            || keys.Contains(candidate, StringComparer.Ordinal))
        {
            return;
        }

        keys.Add(candidate);
    }

    private static bool IsCombatLookupClassSuffix(string value)
    {
        return value is "ironclad" or "silent" or "defect" or "watcher" or "colorless" or "status" or "curse";
    }

    private static string NormalizeCombatLookupKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var buffer = new char[value.Length];
        var length = 0;
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                buffer[length] = char.ToLowerInvariant(character);
                length += 1;
            }
        }

        return length == 0
            ? string.Empty
            : new string(buffer, 0, length);
    }
}
