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
    public string? SelectedCardType { get; init; }

    public string? SelectedCardTargetType { get; init; }

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

    public bool ExplicitlyClearedSelection =>
        !RequiresHandCardSelection
        && PendingSelection is null
        && CardPlayPending == false
        && TargetingInProgress != true;

    public bool HasCardSelectionEvidence => PendingSelection is not null || KeepsCardPlayOpen;

    public bool HasLiveCardPlayOwnership => PendingSelection is not null || KeepsCardPlayOpen;

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

    public bool HasBlockingCardPlayResolution =>
        HasInFlightPlayerDrivenAction
        && HasLiveCardPlayOwnership;

    public bool HasAttackSelectionWithoutExplicitTargeting =>
        PendingSelection?.Kind == AutoCombatCardKind.AttackLike
        && CombatRuntimeStateSupport.RequiresExplicitEnemyTargetType(SelectedCardTargetType)
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
    public static bool HasExplicitEndTurnAffordance(ObserverSummary observer, WindowBounds? windowBounds = null)
    {
        if (observer.ActionNodes.Any(node =>
                node.Actionable
                && CombatHistorySupport.IsCombatEndTurnLabel(node.Label)
                && HasExplicitActionSurfaceBounds(node.ScreenBounds, windowBounds)))
        {
            return true;
        }

        if (observer.Choices.Any(choice =>
                CombatHistorySupport.IsCombatEndTurnLabel(choice.Label)
                && HasExplicitActionSurfaceBounds(choice.ScreenBounds, windowBounds)))
        {
            return true;
        }

        return observer.CurrentChoices.Any(CombatHistorySupport.IsCombatEndTurnLabel);
    }

    public const string DefaultInteractionRevision = "none:none:false:false:none";

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
            SelectedCardType = selectedCardType,
            SelectedCardTargetType = selectedCardTargetType,
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
        return ResolvePendingSelection(observer, combatCardKnowledge, historyPendingSelection, null);
    }

    public static PendingCombatSelection? ResolvePendingSelection(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? historyPendingSelection,
        IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        var runtime = Read(observer, combatCardKnowledge);
        if (runtime.PendingSelection is not null)
        {
            return runtime.PendingSelection;
        }

        if (historyPendingSelection?.Kind == AutoCombatCardKind.AttackLike
            && historyPendingSelection.SlotIndex is >= 1 and <= 5
            && HasDownstreamAttackResolutionAttempt(history, historyPendingSelection.SlotIndex))
        {
            return HasCurrentFrameAttackLaneOwnership(observer, runtime)
                   && HasLiveEnemyTargetSurface(observer, runtime)
                ? historyPendingSelection
                : null;
        }

        if (runtime.ExplicitlyClearedSelection)
        {
            return null;
        }

        return ShouldPreserveHistoryAttackSelection(observer, runtime, historyPendingSelection, history)
            ? historyPendingSelection
            : null;
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
        return CanResolveEnemyTargetWithoutScreenshot(observer, combatCardKnowledge, pendingSelection);
    }

    public static bool CanResolveEnemyTargetWithoutScreenshot(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? pendingSelection)
    {
        var runtime = Read(observer, combatCardKnowledge);
        if (!HasCurrentFrameAttackLaneOwnership(observer, runtime))
        {
            return false;
        }

        if (CombatTargetabilitySupport.GetCombatEnemyTargetNodes(observer).Count > 0)
        {
            return true;
        }

        if (runtime.HasExplicitHittableEnemyAuthority)
        {
            return false;
        }

        return false;
    }

    public static bool RequiresExplicitTargetingBeforeEnemyClick(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? pendingSelection = null)
    {
        var runtime = Read(observer, combatCardKnowledge);
        var effectivePendingSelection = runtime.PendingSelection?.Kind == AutoCombatCardKind.AttackLike
            ? runtime.PendingSelection
            : pendingSelection?.Kind == AutoCombatCardKind.AttackLike
                ? pendingSelection
                : null;
        if (effectivePendingSelection?.Kind != AutoCombatCardKind.AttackLike)
        {
            return false;
        }

        if (RequiresExplicitEnemyTargetType(runtime.SelectedCardTargetType))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(runtime.SelectedCardTargetType))
        {
            return false;
        }

        var selectedSlot = effectivePendingSelection.SlotIndex;
        var knowledgeCard = combatCardKnowledge.FirstOrDefault(card => card.SlotIndex == selectedSlot);
        if (knowledgeCard is not null)
        {
            return RequiresExplicitEnemyTargetCard(knowledgeCard);
        }

        return true;
    }

    public static bool HasSelectedAttackMetadata(string? selectedCardType, string? selectedCardTargetType)
    {
        return string.Equals(selectedCardType, "Attack", StringComparison.OrdinalIgnoreCase)
               || IsEnemyTargetType(selectedCardTargetType);
    }

    public static bool HasPositiveAttackConfirmEvidence(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? pendingSelection = null)
    {
        var runtime = Read(observer, combatCardKnowledge);
        if (runtime.RequiresHandCardSelection)
        {
            return false;
        }

        var effectivePendingSelection = runtime.PendingSelection?.Kind == AutoCombatCardKind.AttackLike
            ? runtime.PendingSelection
            : pendingSelection?.Kind == AutoCombatCardKind.AttackLike
                ? pendingSelection
                : null;
        if (effectivePendingSelection?.Kind != AutoCombatCardKind.AttackLike)
        {
            return false;
        }

        if (RequiresExplicitTargetingBeforeEnemyClick(observer, combatCardKnowledge, effectivePendingSelection))
        {
            return false;
        }

        return runtime.CardPlayPending == true
               && runtime.TargetingInProgress != true
               && (runtime.PendingSelection?.Kind == AutoCombatCardKind.AttackLike
                   || HasSelectedAttackMetadata(runtime.SelectedCardType, runtime.SelectedCardTargetType));
    }

    public static bool HasSelectionToKeep(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var runtime = Read(observer, combatCardKnowledge);
        return runtime.KeepsCardPlayOpen || runtime.PendingSelection is not null;
    }

    public static bool HasCurrentFrameAttackLaneOwnership(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        return HasCurrentFrameAttackLaneOwnership(observer, Read(observer, combatCardKnowledge));
    }

    public static bool HasResidualAttackSelectionTail(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? historyPendingSelection,
        IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        return HasResidualAttackSelectionTail(observer, Read(observer, combatCardKnowledge), historyPendingSelection, history);
    }

    public static bool ShouldRetireResolvedAttackSelectionTail(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? historyPendingSelection,
        IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        return historyPendingSelection?.Kind == AutoCombatCardKind.AttackLike
               && historyPendingSelection.SlotIndex is >= 1 and <= 5
               && ShouldRetireResolvedAttackSelectionTail(
                   observer,
                   Read(observer, combatCardKnowledge),
                   historyPendingSelection.SlotIndex,
                   history);
    }

    public static bool LooksLikeFreshCombatEncounterStart(CombatRuntimeState runtime)
    {
        return runtime.RoundNumber == 1
               && runtime.PendingSelection is null
               && runtime.CardPlayPending != true
               && runtime.TargetingInProgress != true
               && !runtime.RequiresHandCardSelection
               && !runtime.HasInFlightPlayerDrivenAction
               && runtime.PlayerActionsDisabled != true
               && runtime.EndingPlayerTurnPhaseOne != true
               && runtime.EndingPlayerTurnPhaseTwo != true
               && string.IsNullOrWhiteSpace(runtime.LastCardPlayFinishedCardId)
               && IsDefaultInteractionRevision(runtime.InteractionRevision)
               && runtime.HistoryStartedCount is null
               && runtime.HistoryFinishedCount is null;
    }

    public static bool IsDefaultInteractionRevision(string? interactionRevision)
    {
        if (string.IsNullOrWhiteSpace(interactionRevision)
            || string.Equals(interactionRevision, DefaultInteractionRevision, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var segments = interactionRevision.Split(':', StringSplitOptions.TrimEntries);
        if (segments.Length != 5)
        {
            return false;
        }

        return string.Equals(segments[0], "none", StringComparison.OrdinalIgnoreCase)
               && string.Equals(segments[1], "none", StringComparison.OrdinalIgnoreCase)
               && (string.Equals(segments[2], "none", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(segments[2], "false", StringComparison.OrdinalIgnoreCase))
               && (string.Equals(segments[3], "none", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(segments[3], "false", StringComparison.OrdinalIgnoreCase))
               && string.Equals(segments[4], "none", StringComparison.OrdinalIgnoreCase);
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
        PendingCombatSelection? historyPendingSelection,
        IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        if (historyPendingSelection?.Kind != AutoCombatCardKind.AttackLike
            || historyPendingSelection.SlotIndex is < 1 or > 5
            || !HasCurrentFrameAttackLaneOwnership(observer, runtime)
            || runtime.HasInFlightPlayerDrivenAction
            || runtime.PlayerActionsDisabled == true
            || runtime.EndingPlayerTurnPhaseOne == true
            || runtime.EndingPlayerTurnPhaseTwo == true)
        {
            return false;
        }

        if (HasDownstreamAttackResolutionAttempt(history, historyPendingSelection.SlotIndex))
        {
            return HasLiveEnemyTargetSurface(observer, runtime);
        }

        var sourceMetadata = TryGetRecentAttackSelectionMetadata(history, historyPendingSelection.SlotIndex);
        if (sourceMetadata is null)
        {
            return true;
        }

        return !HasPostSelectionProgress(runtime, sourceMetadata);
    }

    private static bool HasCurrentFrameAttackLaneOwnership(ObserverSummary observer, CombatRuntimeState runtime)
    {
        if (runtime.PendingSelection?.Kind == AutoCombatCardKind.AttackLike)
        {
            return true;
        }

        if (runtime.CardPlayPending != true
            && runtime.TargetingInProgress != true)
        {
            return false;
        }

        return HasSelectedAttackMetadata(runtime.SelectedCardType, runtime.SelectedCardTargetType)
               || HasLiveEnemyTargetSurface(observer, runtime);
    }

    private static bool HasResidualAttackSelectionTail(
        ObserverSummary observer,
        CombatRuntimeState runtime,
        PendingCombatSelection? historyPendingSelection,
        IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        if (historyPendingSelection?.Kind != AutoCombatCardKind.AttackLike
            || historyPendingSelection.SlotIndex is < 1 or > 5
            || HasCurrentFrameAttackLaneOwnership(observer, runtime)
            || runtime.HasInFlightPlayerDrivenAction
            || runtime.PlayerActionsDisabled == true
            || runtime.EndingPlayerTurnPhaseOne == true
            || runtime.EndingPlayerTurnPhaseTwo == true
            || LooksLikeFreshCombatEncounterStart(runtime)
            || HasDownstreamAttackResolutionAttempt(history, historyPendingSelection.SlotIndex))
        {
            return false;
        }

        if (ShouldRetireResolvedAttackSelectionTail(observer, runtime, historyPendingSelection.SlotIndex, history))
        {
            return false;
        }

        return HasSelectedAttackMetadata(runtime.SelectedCardType, runtime.SelectedCardTargetType)
               || observer.Meta.TryGetValue("combatTargetSummary", out var rawTargetSummary)
               && !string.IsNullOrWhiteSpace(rawTargetSummary);
    }

    private static bool ShouldRetireResolvedAttackSelectionTail(
        ObserverSummary observer,
        CombatRuntimeState runtime,
        int slotIndex,
        IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        if (!runtime.ExplicitlyClearedSelection
            || runtime.HasInFlightPlayerDrivenAction
            || runtime.PlayerActionsDisabled == true
            || runtime.EndingPlayerTurnPhaseOne == true
            || runtime.EndingPlayerTurnPhaseTwo == true
            || HasLiveEnemyTargetSurface(observer, runtime))
        {
            return false;
        }

        if (string.Equals(runtime.SelectedCardTargetType, "RandomEnemy", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (observer.Meta.TryGetValue("combatTargetSummary", out var rawTargetSummary)
            && !string.IsNullOrWhiteSpace(rawTargetSummary)
            && !HasStaleRuntimeTargetSummaryWithoutLiveTargetAuthority(observer, runtime))
        {
            return false;
        }

        var sourceMetadata = TryGetRecentAttackSelectionMetadata(history, slotIndex);
        if (sourceMetadata is null || !HasPostSelectionProgress(runtime, sourceMetadata))
        {
            return false;
        }

        return HasSelectedAttackMetadata(runtime.SelectedCardType, runtime.SelectedCardTargetType);
    }

    private static bool HasStaleRuntimeTargetSummaryWithoutLiveTargetAuthority(
        ObserverSummary observer,
        CombatRuntimeState runtime)
    {
        if (!observer.Meta.TryGetValue("combatTargetSummary", out var rawTargetSummary)
            || string.IsNullOrWhiteSpace(rawTargetSummary))
        {
            return false;
        }

        if (HasLiveEnemyTargetSurface(observer, runtime)
            || runtime.TargetingInProgress == true
            || runtime.HasExplicitEnemyTargetingEvidence
            || runtime.HasExplicitTargetableEnemy
            || runtime.HasExplicitHittableEnemy)
        {
            return false;
        }

        return true;
    }

    private static bool HasDownstreamAttackResolutionAttempt(
        IReadOnlyList<GuiSmokeHistoryEntry>? history,
        int slotIndex)
    {
        if (history is null || history.Count == 0)
        {
            return false;
        }

        var attackSelectionIndex = TryFindRecentAttackSelectionIndex(history, slotIndex);
        if (attackSelectionIndex < 0)
        {
            return false;
        }

        for (var index = attackSelectionIndex + 1; index < history.Count; index += 1)
        {
            var entry = history[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (CombatHistorySupport.IsCombatEnemyTargetLabel(entry.TargetLabel)
                || string.Equals(entry.TargetLabel, "confirm selected attack card", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static int TryFindRecentAttackSelectionIndex(
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        int slotIndex)
    {
        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                || !CombatHistorySupport.TryParsePendingCombatSelection(entry.TargetLabel, out var selection)
                || selection?.Kind != AutoCombatCardKind.AttackLike
                || selection.SlotIndex != slotIndex)
            {
                continue;
            }

            return index;
        }

        return -1;
    }

    private static bool HasLiveEnemyTargetSurface(ObserverSummary observer, CombatRuntimeState runtime)
    {
        if (runtime.HasExplicitEnemyTargetingEvidence)
        {
            return true;
        }

        var targetNodes = CombatTargetabilitySupport.GetCombatEnemyTargetNodes(observer);
        if (targetNodes.Count == 0)
        {
            return false;
        }

        if (!runtime.ExplicitlyClearedSelection)
        {
            return true;
        }

        return runtime.PendingSelection?.Kind == AutoCombatCardKind.AttackLike
               || runtime.CardPlayPending == true
               || runtime.TargetingInProgress == true
               || runtime.HasExplicitTargetableEnemy
               || runtime.HasExplicitHittableEnemy;
    }

    private static bool HasExplicitActionSurfaceBounds(string? screenBounds, WindowBounds? windowBounds)
    {
        if (HasUsableLogicalBounds(screenBounds))
        {
            return true;
        }

        return windowBounds is not null && IsBoundsInsideWindow(screenBounds, windowBounds);
    }

    private static bool HasUsableLogicalBounds(string? raw)
    {
        if (!TryParseCombatNodeBounds(raw, out var bounds))
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

    private static bool IsBoundsInsideWindow(string? screenBounds, WindowBounds windowBounds)
    {
        if (!TryParseCombatNodeBounds(screenBounds, out var bounds))
        {
            return false;
        }

        return bounds.Right > windowBounds.X
               && bounds.Bottom > windowBounds.Y
               && bounds.X < windowBounds.X + windowBounds.Width
               && bounds.Y < windowBounds.Y + windowBounds.Height;
    }

    private static bool TryParseCombatNodeBounds(string? raw, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
        {
            return false;
        }

        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
        {
            return false;
        }

        if (width <= 0f || height <= 0f)
        {
            return false;
        }

        bounds = new RectangleF(x, y, width, height);
        return true;
    }

    private static CombatBarrierHistoryMetadata? TryGetRecentAttackSelectionMetadata(
        IReadOnlyList<GuiSmokeHistoryEntry>? history,
        int slotIndex)
    {
        if (history is null || history.Count == 0)
        {
            return null;
        }

        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                || !string.Equals(entry.Action, "press-key", StringComparison.OrdinalIgnoreCase)
                || !CombatHistorySupport.TryParsePendingCombatSelection(entry.TargetLabel, out var selection)
                || selection?.Kind != AutoCombatCardKind.AttackLike
                || selection.SlotIndex != slotIndex)
            {
                continue;
            }

            return CombatBarrierSupport.TryParseHistoryMetadata(entry.Metadata);
        }

        return null;
    }

    private static bool HasPostSelectionProgress(
        CombatRuntimeState runtime,
        CombatBarrierHistoryMetadata sourceMetadata)
    {
        if (sourceMetadata.RoundNumber is not null
            && runtime.RoundNumber is not null
            && runtime.RoundNumber != sourceMetadata.RoundNumber)
        {
            return true;
        }

        if (sourceMetadata.HistoryStartedCount is not null
            && runtime.HistoryStartedCount is not null
            && runtime.HistoryStartedCount != sourceMetadata.HistoryStartedCount)
        {
            return true;
        }

        if (sourceMetadata.HistoryFinishedCount is not null
            && runtime.HistoryFinishedCount is not null
            && runtime.HistoryFinishedCount != sourceMetadata.HistoryFinishedCount)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(sourceMetadata.InteractionRevision)
            && !string.Equals(runtime.InteractionRevision, sourceMetadata.InteractionRevision, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.Equals(runtime.LastCardPlayFinishedCardId, sourceMetadata.LastFinishedCardId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static bool IsEnemyTargetType(string? targetType)
    {
        return RequiresExplicitEnemyTargetType(targetType)
               || string.Equals(targetType, "RandomEnemy", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyTargetType(string? targetType)
    {
        return string.Equals(targetType, "Self", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "None", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "AllAllies", StringComparison.OrdinalIgnoreCase);
    }

    public static bool RequiresExplicitEnemyTargetType(string? targetType)
    {
        return string.Equals(targetType, "AnyEnemy", StringComparison.OrdinalIgnoreCase);
    }

    public static bool RequiresExplicitEnemyTargetCard(CombatCardKnowledgeHint card)
    {
        return RequiresExplicitEnemyTargetType(card.Target);
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
