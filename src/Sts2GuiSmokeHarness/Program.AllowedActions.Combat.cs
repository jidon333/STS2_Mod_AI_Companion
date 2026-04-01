using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModAiCompanion.Mod;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;
using static GuiSmokeChoicePrimitiveSupport;
using static GuiSmokeSceneReasoningSupport;
using static GuiSmokeStepRequestFactory;
using static ObserverScreenProvenance;

internal static partial class Program
{
    static string[] GetCombatAllowedActions(
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        GuiSmokeStepAnalysisContext? analysisContext = null)
    {
        var context = analysisContext ?? CreateStepAnalysisContext(GuiSmokePhase.HandleCombat, observer, screenshotPath, history, combatCardKnowledge);
        if (AutoDecisionProvider.HasReleasedCombatOwnership(context.CombatReleaseState))
        {
            return new[] { "wait" };
        }

        if (context.CombatPlayerActionWindowClosed)
        {
            return new[] { "wait" };
        }

        var combatBarrier = context.CombatBarrierEvaluation;
        if (combatBarrier.IsActive && combatBarrier.IsHardWaitBarrier)
        {
            return new[] { "wait" };
        }

        var cardSelectionState = CardSelectionObserverSignals.TryGetState(observer.Summary);
        if (cardSelectionState is not null)
        {
            return GuiSmokeNonCombatAllowedActionSupport.BuildCardSelectionAllowedActions(observer.Summary, cardSelectionState);
        }

        var actions = new List<string>();
        var combatContext = context.CombatContext;
        var historyPendingSelection = CombatHistorySupport.TryGetPendingCombatSelection(combatContext.CombatHistory);
        var blockedCombatNoOpCounts = combatContext.CombatNoOpCountsBySlot;
        var pendingSelection = context.PendingCombatSelection;
        var runtimeCombatState = context.RuntimeCombatState;
        var hasSelectedNonEnemyConfirmEvidence = context.HasSelectedNonEnemyConfirmEvidence;
        var combatMicroStage = context.CombatMicroStage;
        if (runtimeCombatState.RequiresHandCardSelection)
        {
            if (runtimeCombatState.HasSelectedHandCardForConfirmation)
            {
                actions.Add("confirm selected hand card");
            }
            else if (observer.CombatHand.Any(card => card.SlotIndex is >= 1 and <= 5))
            {
                actions.Add("select card from hand");
            }

            actions.Add("wait");
            return actions
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        if (combatMicroStage.Kind is CombatMicroStageKind.ResolvingCardPlay or CombatMicroStageKind.TurnClosing or CombatMicroStageKind.EnemyTurnClosed)
        {
            return new[] { "wait" };
        }

        var keepNonEnemySelectionClosed = pendingSelection?.Kind == AutoCombatCardKind.DefendLike && hasSelectedNonEnemyConfirmEvidence;
        bool ShouldSuppressNonEnemyReselect(int slotIndex)
        {
            if (keepNonEnemySelectionClosed)
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

        var pendingAttackBlocked = pendingSelection?.Kind == AutoCombatCardKind.AttackLike
                                   && blockedCombatNoOpCounts.TryGetValue(pendingSelection.SlotIndex, out var pendingNoOpCount)
                                   && pendingNoOpCount >= 2;
        var unresolvedAttackSelectionChurn = pendingSelection is null
                                             && !runtimeCombatState.HasCardSelectionEvidence
                                             && !context.CanResolveCombatEnemyTarget
                                             && CombatHistorySupport.HasRecentAttackSelectionChurnWithoutResolution(combatContext.CombatHistory);
        var staleAttackSelectionTail = pendingSelection is null
                                       && CombatRuntimeStateSupport.HasResidualAttackSelectionTail(
                                           observer.Summary,
                                           combatCardKnowledge,
                                           historyPendingSelection,
                                           combatContext.CombatHistory);
        if (combatMicroStage.Kind == CombatMicroStageKind.ResolvingNonEnemy)
        {
            if (hasSelectedNonEnemyConfirmEvidence)
            {
                actions.Add("confirm selected non-enemy card");
            }

            if (combatMicroStage.AllowsCancel)
            {
                actions.Add("right-click cancel selected card");
            }

            actions.Add("wait");
            return actions
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        if (combatMicroStage.Kind == CombatMicroStageKind.ResolvingAttackTarget)
        {
            if (!pendingAttackBlocked && context.CanResolveCombatEnemyTarget)
            {
                actions.Add("click enemy");
            }

            if (combatMicroStage.AllowsCancel)
            {
                actions.Add("right-click cancel selected card");
            }

            actions.Add("wait");
            return actions
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        if (combatMicroStage.Kind == CombatMicroStageKind.AwaitingCardPlayConfirm
            && CombatRuntimeStateSupport.HasPositiveAttackConfirmEvidence(
                observer.Summary,
                combatCardKnowledge,
                pendingSelection))
        {
            actions.Add("confirm selected attack card");
            if (combatMicroStage.AllowsCancel)
            {
                actions.Add("right-click cancel selected card");
            }

            actions.Add("wait");
            return actions
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        var allowsEndTurnFromStaleAttackTail = observer.PlayerEnergy is <= 0
                                               && (combatMicroStage.AllowsEndTurn
                                                   || CombatRuntimeStateSupport.HasExplicitEndTurnAffordance(observer.Summary));
        if (unresolvedAttackSelectionChurn)
        {
            return allowsEndTurnFromStaleAttackTail
                ? new[] { "click end turn", "wait" }
                : new[] { "wait" };
        }

        if (staleAttackSelectionTail)
        {
            return allowsEndTurnFromStaleAttackTail
                ? new[] { "click end turn", "wait" }
                : new[] { "wait" };
        }

        foreach (var slotIndex in GetPlayableCombatAttackSlots(observer, combatCardKnowledge))
        {
            if (CombatBarrierSupport.SuppressesAttackSlot(combatBarrier, slotIndex))
            {
                continue;
            }

            if (!blockedCombatNoOpCounts.TryGetValue(slotIndex, out var noOpCount) || noOpCount < 2)
            {
                actions.Add($"select attack slot {slotIndex}");
            }
        }

        foreach (var slotIndex in GetPlayableCombatNonEnemySlots(observer, combatCardKnowledge))
        {
            if (CombatBarrierSupport.SuppressesNonEnemySlot(combatBarrier, slotIndex))
            {
                continue;
            }

            if (ShouldSuppressNonEnemyReselect(slotIndex))
            {
                continue;
            }

            actions.Add($"select non-enemy slot {slotIndex}");
        }

        if (hasSelectedNonEnemyConfirmEvidence)
        {
            actions.Add("confirm selected non-enemy card");
        }

        if (combatMicroStage.AllowsEndTurn)
        {
            actions.Add("click end turn");
        }

        var analysis = context.HasScreenshotEvidence
            ? context.CombatAnalysis
            : new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown);
        if (HasCombatSelectionToCancelFromAnalysis(observer, combatCardKnowledge, analysis, pendingSelection, combatContext))
        {
            actions.Add("right-click cancel selected card");
        }

        if (actions.Count == 1)
        {
            actions.Insert(0, "select card from hand");
        }

        actions.Add("wait");
        return actions
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    static IEnumerable<int> GetPlayableCombatAttackSlots(
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var knowledgeSlots = combatCardKnowledge
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .Where(card => IsEnemyTargetCombatCard(card) && IsPlayableAtCurrentEnergy(card, observer.PlayerEnergy))
            .Select(static card => card.SlotIndex);
        var observerSlots = observer.CombatHand
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .Where(card => IsAttackCombatHandCard(card) && IsObservedCombatCardPlayableAtCurrentEnergy(card, observer.PlayerEnergy, combatCardKnowledge))
            .Select(static card => card.SlotIndex);
        return knowledgeSlots
            .Concat(observerSlots)
            .Distinct()
            .OrderBy(static slotIndex => slotIndex);
    }

    static IEnumerable<int> GetPlayableCombatNonEnemySlots(
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var knowledgeSlots = combatCardKnowledge
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .Where(card => CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatCard(card, observer.PlayerEnergy))
            .Select(static card => card.SlotIndex);
        var observerSlots = observer.CombatHand
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .Where(card => CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatHandCard(card, observer.PlayerEnergy, combatCardKnowledge))
            .Select(static card => card.SlotIndex);
        return knowledgeSlots
            .Concat(observerSlots)
            .Distinct()
            .OrderBy(static slotIndex => slotIndex);
    }

    internal static bool CanResolveEnemyTargetFromStateAnalysis(
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        AutoCombatAnalysis analysis,
        PendingCombatSelection? pendingSelection)
    {
        return CombatRuntimeStateSupport.CanResolveEnemyTarget(observer.Summary, combatCardKnowledge, pendingSelection, analysis);
    }

    static bool HasCombatSelectionToCancelFromAnalysis(
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        AutoCombatAnalysis analysis,
        PendingCombatSelection? pendingSelection,
        ReconstructedHandleCombatContext? combatContext = null)
    {
        if (HasBlockedOpenAttackSelectionToCancel(observer.Summary, combatCardKnowledge, pendingSelection, combatContext))
        {
            return true;
        }

        if (CombatRuntimeStateSupport.HasSelectionToKeep(observer.Summary, combatCardKnowledge))
        {
            return false;
        }

        return false;
    }

    static bool HasBlockedOpenAttackSelectionToCancel(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? pendingSelection,
        ReconstructedHandleCombatContext? combatContext)
    {
        if (combatContext is null
            || pendingSelection?.Kind != AutoCombatCardKind.AttackLike
            || pendingSelection.SlotIndex is < 1 or > 5
            || !CombatRuntimeStateSupport.HasSelectionToKeep(observer, combatCardKnowledge))
        {
            return false;
        }

        return HandleCombatContextSupport.GetCombatNoOpCountForSlot(combatContext, pendingSelection.SlotIndex) >= 2;
    }

    internal static string BuildCombatFailureModeHint(
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var attackSlots = GetPlayableCombatAttackSlots(observer, combatCardKnowledge).ToArray();
        if (attackSlots.Length > 0)
        {
            return $"Only click the enemy after selecting an attack. Current playable attack slots: {string.Join(", ", attackSlots)}. Ignore stale map or reward contamination when the observer still shows combat.";
        }

        var nonEnemySlots = GetPlayableCombatNonEnemySlots(observer, combatCardKnowledge).ToArray();
        if (nonEnemySlots.Length > 0)
        {
            return $"No playable enemy-target attack is currently confirmed. Use a non-enemy slot ({string.Join(", ", nonEnemySlots)}) or end turn, and do not click the enemy without a selected attack.";
        }

        return "No playable enemy-target attack is currently confirmed. Do not click the enemy without a visible target arrow or a matching selected attack lane; prefer end turn instead.";
    }

    static bool IsAttackCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("STRIKE", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("BASH", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsEnemyTargetCombatCard(CombatCardKnowledgeHint card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsPlayableAtCurrentEnergy(CombatCardKnowledgeHint card, int? energy)
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

    static bool IsObservedCombatCardPlayableAtCurrentEnergy(
        ObservedCombatHandCard card,
        int? energy,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var resolvedCost = ResolveObservedCombatCardCost(card, combatCardKnowledge);
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

    static bool IsProceedNode(ObserverActionNode node)
    {
        return EventProceedObserverSignals.HasExplicitEventProceedSemantic(node.SemanticHints)
               || node.Label.Contains("Proceed", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Continue", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("진행", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("계속", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("proceed", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsCurrentRewardProgressionChoiceForObserver(ObserverChoice choice, WindowBounds? windowBounds)
    {
        return IsProgressionLikeRewardChoice(choice)
               && HasActiveRewardBoundsForObserver(choice.ScreenBounds, windowBounds);
    }

    static bool IsCurrentRewardProgressionNodeForObserver(ObserverActionNode node, WindowBounds? windowBounds)
    {
        return node.Actionable
               && IsProgressionLikeRewardNode(node)
               && HasActiveRewardBoundsForObserver(node.ScreenBounds, windowBounds);
    }

    static bool IsProgressionLikeRewardChoice(ObserverChoice choice)
    {
        if (!TryParseScreenBounds(choice.ScreenBounds, out _))
        {
            return false;
        }

        return choice.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || choice.Kind.Contains("card", StringComparison.OrdinalIgnoreCase)
               || choice.Kind.Contains("potion", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => hint.StartsWith("reward", StringComparison.OrdinalIgnoreCase))
               || choice.Label.Contains("골드", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("gold", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("포션", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("potion", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("넘기", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("skip", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("proceed", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("continue", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || choice.Value?.StartsWith("CARD.", StringComparison.OrdinalIgnoreCase) == true
               || choice.Value?.StartsWith("POTION.", StringComparison.OrdinalIgnoreCase) == true
               || ContainsAny(choice.Description, "포션", "potion")
               || HasLargeChoiceBounds(choice.ScreenBounds);
    }

    static bool IsProgressionLikeRewardNode(ObserverActionNode node)
    {
        if (!TryParseScreenBounds(node.ScreenBounds, out _))
        {
            return false;
        }

        return node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("proceed", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("골드", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("gold", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("포션", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("potion", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("넘기", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("skip", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("proceed", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("continue", StringComparison.OrdinalIgnoreCase)
               || HasLargeChoiceBounds(node.ScreenBounds);
    }

    static bool HasActiveRewardBoundsForObserver(string? screenBounds, WindowBounds? windowBounds)
    {
        if (HasUsableLogicalBoundsForObserver(screenBounds))
        {
            return true;
        }

        return windowBounds is not null && IsBoundsInsideWindowForObserver(screenBounds, windowBounds);
    }

    static bool IsBoundsInsideWindowForObserver(string? screenBounds, WindowBounds windowBounds)
    {
        if (!TryParseScreenBounds(screenBounds, out var bounds))
        {
            return false;
        }

        return bounds.Right > windowBounds.X
               && bounds.Bottom > windowBounds.Y
               && bounds.X < windowBounds.X + windowBounds.Width
               && bounds.Y < windowBounds.Y + windowBounds.Height;
    }

    static bool HasUsableLogicalBoundsForObserver(string? raw)
    {
        if (!TryParseScreenBounds(raw, out var bounds))
        {
            return false;
        }

        var centerX = bounds.X + bounds.Width / 2f;
        var centerY = bounds.Y + bounds.Height / 2f;
        return centerX >= 0f
               && centerY >= 0f
               && centerX <= 1920f
               && centerY <= 1080f;
    }

    static int? ResolveObservedCombatCardCost(ObservedCombatHandCard card, IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
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

        var cardKeys = BuildCombatKnowledgeLookupKeys(card.Name);
        if (cardKeys.Count == 0)
        {
            return null;
        }

        return combatCardKnowledge
            .Where(candidate => candidate.Cost is not null)
            .Where(candidate => BuildCombatKnowledgeLookupKeys(candidate.Name).Any(cardKeys.Contains))
            .Select(static candidate => candidate.Cost)
            .FirstOrDefault();
    }

    internal static bool IsOverlayCleanupTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay back", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay backdrop close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "inspect overlay escape", StringComparison.OrdinalIgnoreCase);
    }

}
