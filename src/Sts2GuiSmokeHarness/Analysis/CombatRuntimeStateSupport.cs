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

sealed record CombatRuntimeState(
    bool? CardPlayPending,
    string? PlayMode,
    PendingCombatSelection? PendingSelection,
    bool? TargetingInProgress,
    string? ValidTargetsType,
    int? TargetableEnemyCount,
    IReadOnlyList<string> TargetableEnemyIds,
    int? HittableEnemyCount,
    IReadOnlyList<string> HittableEnemyIds,
    string? HoveredTargetKind,
    string? HoveredTargetId,
    string? HoveredTargetLabel,
    bool? HoveredTargetIsHittable,
    string? LastCardPlayStartedCardId,
    string? LastCardPlayFinishedCardId,
    string? LastCardPlayFinishedCardName)
{
    public int? RoundNumber { get; init; }

    public bool? PlayerActionsDisabled { get; init; }

    public bool? EndingPlayerTurnPhaseOne { get; init; }

    public bool? EndingPlayerTurnPhaseTwo { get; init; }

    public int? HistoryStartedCount { get; init; }

    public int? HistoryFinishedCount { get; init; }

    public string? InteractionRevision { get; init; }

    public string? ScreenEpisodeId { get; init; }

    public int HandSelectionSelectedCount { get; init; }

    public bool? HandSelectionConfirmEnabled { get; init; }

    public IReadOnlyList<string> HandSelectionSelectedCardIds { get; init; } = Array.Empty<string>();

    public bool RequiresHandCardSelection =>
        string.Equals(PlayMode, "SimpleSelect", StringComparison.OrdinalIgnoreCase);

    public bool HasSelectedHandCardForConfirmation =>
        RequiresHandCardSelection
        && HandSelectionSelectedCount > 0
        && HandSelectionConfirmEnabled == true;

    public bool KeepsCardPlayOpen => CardPlayPending == true || TargetingInProgress == true || RequiresHandCardSelection;

    public bool ExplicitlyClearedSelection => !RequiresHandCardSelection && CardPlayPending == false && TargetingInProgress != true;

    public bool HasCardSelectionEvidence => PendingSelection is not null || KeepsCardPlayOpen;

    public bool HasExplicitEnemyTargetingEvidence =>
        TargetingInProgress == true
        || string.Equals(HoveredTargetKind, "enemy", StringComparison.OrdinalIgnoreCase);

    public bool HasExplicitTargetableEnemyAuthority => TargetableEnemyCount is not null;

    public bool HasExplicitTargetableEnemy => TargetableEnemyCount > 0 || TargetableEnemyIds.Count > 0;

    public bool HasExplicitHittableEnemyAuthority => HittableEnemyCount is not null;

    public bool HasExplicitHittableEnemy => HittableEnemyCount > 0 || HittableEnemyIds.Count > 0;

    public bool HasInFlightPlayerDrivenAction =>
        HistoryStartedCount is not null
        && HistoryFinishedCount is not null
        && HistoryStartedCount > HistoryFinishedCount;

    public bool HasAttackSelectionWithoutExplicitTargeting =>
        PendingSelection?.Kind == AutoCombatCardKind.AttackLike
        && !HasExplicitEnemyTargetingEvidence;
}

static class CombatAuthoritySupport
{
    public static string? TryGetMetaValue(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value) ? value : null;
    }

    public static bool? TryGetBoolOrCrossCheck(ObserverSummary observer, string key, string crossCheckKey)
    {
        if (observer.Meta.TryGetValue(key, out var value)
            && bool.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return TryGetCrossCheckValue(observer, crossCheckKey, out value) && bool.TryParse(value, out parsed)
            ? parsed
            : null;
    }

    public static int? TryGetIntOrCrossCheck(ObserverSummary observer, string key, string crossCheckKey)
    {
        if (observer.Meta.TryGetValue(key, out var value)
            && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return TryGetCrossCheckValue(observer, crossCheckKey, out value)
               && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed)
            ? parsed
            : null;
    }

    private static bool TryGetCrossCheckValue(ObserverSummary observer, string key, out string value)
    {
        value = string.Empty;
        if (!observer.Meta.TryGetValue("combatCrossCheck", out var combatCrossCheck)
            || string.IsNullOrWhiteSpace(combatCrossCheck))
        {
            return false;
        }

        foreach (var segment in combatCrossCheck.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separatorIndex = segment.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var candidateKey = segment[..separatorIndex].Trim();
            if (!string.Equals(candidateKey, key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            value = segment[(separatorIndex + 1)..].Trim();
            return true;
        }

        return false;
    }
}

static class CombatRuntimeStateSupport
{
    public static CombatRuntimeState Read(ObserverSummary observer, IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var selectedCardSlot = CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatSelectedCardSlot", "combatSelectedCardSlot");
        var awaitingPlaySlots = ParseSlotList(CombatAuthoritySupport.TryGetMetaValue(observer, "combatAwaitingPlaySlots"));
        if (selectedCardSlot is null && awaitingPlaySlots.Count > 0)
        {
            selectedCardSlot = awaitingPlaySlots[0];
        }

        var selectedCardTargetType = CombatAuthoritySupport.TryGetMetaValue(observer, "combatSelectedCardTargetType");
        var selectedCardType = CombatAuthoritySupport.TryGetMetaValue(observer, "combatSelectedCardType");
        var pendingSelection = TryResolvePendingSelection(selectedCardSlot, selectedCardTargetType, selectedCardType, observer, combatCardKnowledge);

        return new CombatRuntimeState(
            CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatCardPlayPending", "combatCardPlayPending"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatPlayMode"),
            pendingSelection,
            CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatTargetingInProgress", "combatTargetingInProgress"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatValidTargetsType"),
            CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatTargetableEnemyCount", "combatTargetableEnemyCount"),
            ParseIdList(CombatAuthoritySupport.TryGetMetaValue(observer, "combatTargetableEnemyIds")),
            CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatHittableEnemyCount", "combatHittableEnemyCount"),
            ParseIdList(CombatAuthoritySupport.TryGetMetaValue(observer, "combatHittableEnemyIds")),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatHoveredTargetKind"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatHoveredTargetId"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatHoveredTargetLabel"),
            CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatHoveredTargetIsHittable", "combatHoveredTargetIsHittable"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatLastCardPlayStartedCardId"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatLastCardPlayFinishedCardId"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatLastCardPlayFinishedCardName"))
        {
            RoundNumber = CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatRoundNumber", "CombatState.RoundNumber"),
            PlayerActionsDisabled = CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatPlayerActionsDisabled", "CombatManager.PlayerActionsDisabled"),
            EndingPlayerTurnPhaseOne = CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatEndingPlayerTurnPhaseOne", "CombatManager.EndingPlayerTurnPhaseOne"),
            EndingPlayerTurnPhaseTwo = CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatEndingPlayerTurnPhaseTwo", "CombatManager.EndingPlayerTurnPhaseTwo"),
            HistoryStartedCount = CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatHistoryStartedCount", "combatHistoryStartedCount"),
            HistoryFinishedCount = CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatHistoryFinishedCount", "combatHistoryFinishedCount"),
            InteractionRevision = CombatAuthoritySupport.TryGetMetaValue(observer, "combatInteractionRevision"),
            ScreenEpisodeId = observer.SceneEpisodeId,
            HandSelectionSelectedCount = CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatHandSelectionSelectedCount", "combatHandSelectionSelectedCount") ?? 0,
            HandSelectionConfirmEnabled = CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatHandSelectionConfirmEnabled", "combatHandSelectionConfirmEnabled"),
            HandSelectionSelectedCardIds = ParseIdList(CombatAuthoritySupport.TryGetMetaValue(observer, "combatHandSelectionSelectedCardIds")),
        };
    }

    public static PendingCombatSelection? ResolvePendingSelection(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? historyPendingSelection)
    {
        var runtime = Read(observer, combatCardKnowledge);
        if (runtime.PendingSelection is not null)
        {
            return runtime.PendingSelection;
        }

        if (runtime.ExplicitlyClearedSelection)
        {
            if (ShouldPreserveHistoryAttackSelection(observer, runtime, historyPendingSelection))
            {
                return historyPendingSelection;
            }

            return null;
        }

        return historyPendingSelection;
    }

    public static bool HasNonEnemyConfirmEvidence(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? pendingSelection,
        AutoCombatAnalysis analysis)
    {
        if (HasRuntimeSelectedNonEnemyConfirmEvidence(observer, combatCardKnowledge, pendingSelection))
        {
            return true;
        }

        var runtime = Read(observer, combatCardKnowledge);
        if (runtime.RequiresHandCardSelection)
        {
            return false;
        }

        return pendingSelection?.Kind == AutoCombatCardKind.DefendLike
               && pendingSelection.SlotIndex is >= 1 and <= 5
               && ((analysis.HasSelectedCard && analysis.SelectedCardKind == AutoCombatCardKind.DefendLike)
                   || analysis.HasSelfTargetBrackets);
    }

    public static bool HasRuntimeSelectedNonEnemyConfirmEvidence(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? pendingSelection)
    {
        var runtime = Read(observer, combatCardKnowledge);
        if (runtime.RequiresHandCardSelection)
        {
            return false;
        }

        if (runtime.PendingSelection?.Kind == AutoCombatCardKind.DefendLike
            && runtime.PendingSelection.SlotIndex is >= 1 and <= 5
            && runtime.TargetingInProgress != true)
        {
            return true;
        }

        return false;
    }

    public static bool CanResolveEnemyTarget(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? pendingSelection,
        AutoCombatAnalysis analysis)
    {
        if (CanResolveEnemyTargetWithoutScreenshot(observer, combatCardKnowledge, pendingSelection))
        {
            return true;
        }

        var runtime = Read(observer, combatCardKnowledge);
        if (runtime.HasExplicitHittableEnemyAuthority)
        {
            return false;
        }

        return analysis.HasTargetArrow
               || (analysis.HasSelectedCard
                   && analysis.SelectedCardKind == AutoCombatCardKind.AttackLike
                   && (GetPlayableAttackSlots(observer, combatCardKnowledge).Any()
                       || (observer.CombatHand.Count == 0 && combatCardKnowledge.Count == 0)));
    }

    public static bool CanResolveEnemyTargetWithoutScreenshot(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? pendingSelection)
    {
        var runtime = Read(observer, combatCardKnowledge);
        if (CombatTargetabilitySupport.GetCombatEnemyTargetNodes(observer).Count > 0)
        {
            return true;
        }

        if (runtime.PendingSelection?.Kind == AutoCombatCardKind.AttackLike
            && runtime.HasExplicitEnemyTargetingEvidence
            && (!runtime.HasExplicitHittableEnemyAuthority || runtime.HasExplicitHittableEnemy))
        {
            return true;
        }

        if (runtime.HasExplicitHittableEnemyAuthority)
        {
            return false;
        }

        if (RequiresExplicitTargetingBeforeEnemyClick(observer, combatCardKnowledge))
        {
            return false;
        }

        if (pendingSelection?.Kind == AutoCombatCardKind.AttackLike)
        {
            var pendingCard = observer.CombatHand.FirstOrDefault(card => card.SlotIndex == pendingSelection.SlotIndex);
            if (pendingCard is not null)
            {
                return IsObservedAttackCard(pendingCard)
                       && IsObservedPlayableAtEnergy(pendingCard, observer.PlayerEnergy, combatCardKnowledge);
            }

            var pendingKnowledge = combatCardKnowledge.FirstOrDefault(card => card.SlotIndex == pendingSelection.SlotIndex);
            if (pendingKnowledge is not null)
            {
                return IsKnowledgeEnemyTargetCard(pendingKnowledge)
                       && IsKnowledgePlayableAtEnergy(pendingKnowledge, observer.PlayerEnergy);
            }
        }

        return false;
    }

    public static bool RequiresExplicitTargetingBeforeEnemyClick(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        return Read(observer, combatCardKnowledge).HasAttackSelectionWithoutExplicitTargeting;
    }

    public static bool HasSelectionToKeep(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var runtime = Read(observer, combatCardKnowledge);
        return runtime.KeepsCardPlayOpen || runtime.PendingSelection is not null;
    }

    public static bool ShouldDeferLoopEndTurn(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        return Read(observer, combatCardKnowledge).KeepsCardPlayOpen;
    }

    private static PendingCombatSelection? TryResolvePendingSelection(
        int? selectedCardSlot,
        string? selectedCardTargetType,
        string? selectedCardType,
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        if (!selectedCardSlot.HasValue || selectedCardSlot.Value is < 1 or > 5)
        {
            return null;
        }

        var slotIndex = selectedCardSlot.Value;

        if (IsEnemyTargetType(selectedCardTargetType)
            || string.Equals(selectedCardType, "Attack", StringComparison.OrdinalIgnoreCase))
        {
            return new PendingCombatSelection(slotIndex, AutoCombatCardKind.AttackLike);
        }

        if (IsNonEnemyTargetType(selectedCardTargetType)
            || string.Equals(selectedCardType, "Skill", StringComparison.OrdinalIgnoreCase)
            || string.Equals(selectedCardType, "Power", StringComparison.OrdinalIgnoreCase))
        {
            return new PendingCombatSelection(slotIndex, AutoCombatCardKind.DefendLike);
        }

        var observerCard = observer.CombatHand.FirstOrDefault(card => card.SlotIndex == slotIndex);
        if (observerCard is not null)
        {
            return IsObservedAttackCard(observerCard)
                ? new PendingCombatSelection(slotIndex, AutoCombatCardKind.AttackLike)
                : CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatHandCard(observerCard, observer.PlayerEnergy, combatCardKnowledge)
                    ? new PendingCombatSelection(slotIndex, AutoCombatCardKind.DefendLike)
                    : null;
        }

        var knowledgeCard = combatCardKnowledge.FirstOrDefault(card => card.SlotIndex == slotIndex);
        if (knowledgeCard is not null)
        {
            return IsKnowledgeEnemyTargetCard(knowledgeCard)
                ? new PendingCombatSelection(slotIndex, AutoCombatCardKind.AttackLike)
                : CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatCard(knowledgeCard, observer.PlayerEnergy)
                    ? new PendingCombatSelection(slotIndex, AutoCombatCardKind.DefendLike)
                    : null;
        }

        return null;
    }

    private static bool ShouldPreserveHistoryAttackSelection(
        ObserverSummary observer,
        CombatRuntimeState runtime,
        PendingCombatSelection? historyPendingSelection)
    {
        return historyPendingSelection?.Kind == AutoCombatCardKind.AttackLike
               && historyPendingSelection.SlotIndex is >= 1 and <= 5
               && observer.Meta.TryGetValue("combatTargetSummary", out var rawTargetSummary)
               && !string.IsNullOrWhiteSpace(rawTargetSummary)
               && string.IsNullOrWhiteSpace(runtime.LastCardPlayFinishedCardId)
               && runtime.PlayerActionsDisabled != true
               && runtime.EndingPlayerTurnPhaseOne != true
               && runtime.EndingPlayerTurnPhaseTwo != true;
    }

    private static bool IsEnemyTargetType(string? targetType)
    {
        return string.Equals(targetType, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "RandomEnemy", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyTargetType(string? targetType)
    {
        return string.Equals(targetType, "Self", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "None", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "AllEnemies", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "AllAllies", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsObservedAttackCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("STRIKE", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("BASH", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsKnowledgeEnemyTargetCard(CombatCardKnowledgeHint card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<int> ParseSlotList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<int>();
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(segment => int.TryParse(segment, NumberStyles.Integer, CultureInfo.InvariantCulture, out var slotIndex) ? slotIndex : -1)
            .Where(slotIndex => slotIndex is >= 1 and <= 5)
            .Distinct()
            .OrderBy(static slotIndex => slotIndex)
            .ToArray();
    }

    private static IReadOnlyList<string> ParseIdList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static segment => !string.IsNullOrWhiteSpace(segment))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IEnumerable<int> GetPlayableAttackSlots(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var knowledgeSlots = combatCardKnowledge
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .Where(card => IsKnowledgeEnemyTargetCard(card) && IsKnowledgePlayableAtEnergy(card, observer.PlayerEnergy))
            .Select(static card => card.SlotIndex);
        var observerSlots = observer.CombatHand
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .Where(card => IsObservedAttackCard(card) && IsObservedPlayableAtEnergy(card, observer.PlayerEnergy, combatCardKnowledge))
            .Select(static card => card.SlotIndex);
        return knowledgeSlots.Concat(observerSlots).Distinct().OrderBy(static slotIndex => slotIndex);
    }

    private static bool IsKnowledgePlayableAtEnergy(CombatCardKnowledgeHint card, int? energy)
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

    private static bool IsObservedPlayableAtEnergy(
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
