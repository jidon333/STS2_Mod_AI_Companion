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
using static ObserverScreenProvenance;

enum CombatBarrierKind
{
    None,
    AttackSelect,
    EnemyClick,
    NonEnemySelect,
    CancelSelection,
    EndTurn,
}

sealed record CombatBarrierEvaluation(
    bool IsActive,
    CombatBarrierKind Kind,
    string Reason,
    string? SourceAction,
    int? SourceSlotIndex,
    long? ArmedSnapshotVersion,
    DateTimeOffset? ArmedCapturedAt,
    bool ReleaseSatisfied,
    bool OverWaitRisk)
{
    public bool IsHardWaitBarrier => Kind is CombatBarrierKind.EnemyClick or CombatBarrierKind.CancelSelection or CombatBarrierKind.EndTurn;
}

sealed record CombatBarrierHistoryMetadata(
    long? SnapshotVersion,
    DateTimeOffset? CapturedAt,
    string? ScreenEpisodeId,
    string? InteractionRevision,
    int? HistoryStartedCount,
    int? HistoryFinishedCount,
    string? LastFinishedCardId)
{
    public int? RoundNumber { get; init; }

    public bool? PlayerActionsDisabled { get; init; }

    public bool? EndingPlayerTurnPhaseOne { get; init; }

    public bool? EndingPlayerTurnPhaseTwo { get; init; }
}

static class CombatBarrierSupport
{
    private sealed record BarrierSource(
        CombatBarrierKind Kind,
        GuiSmokeHistoryEntry Entry,
        int? SlotIndex,
        CombatBarrierHistoryMetadata? Metadata);

    public static CombatBarrierEvaluation Evaluate(
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        ObserverState observer,
        PostNodeHandoffState combatResolutionHandoffState,
        ReconstructedHandleCombatContext combatContext,
        CombatRuntimeState runtime,
        AutoCombatAnalysis analysis,
        bool hasSelectedNonEnemyConfirmEvidence,
        bool canResolveCombatEnemyTarget,
        bool combatPlayerActionWindowClosed)
    {
        var source = TryFindSource(combatContext.CombatHistory);
        if (source is null)
        {
            return Inactive;
        }

        var freshSnapshotSeen = HasFreshSnapshotSinceSource(observer, source.Metadata, source.Entry);
        return source.Kind switch
        {
            CombatBarrierKind.AttackSelect => EvaluateAttackSelectBarrier(
                source,
                freshSnapshotSeen,
                runtime,
                analysis,
                canResolveCombatEnemyTarget),
            CombatBarrierKind.EnemyClick => EvaluateEnemyClickBarrier(
                source,
                observer,
                combatResolutionHandoffState,
                freshSnapshotSeen,
                runtime,
                canResolveCombatEnemyTarget),
            CombatBarrierKind.NonEnemySelect => EvaluateNonEnemySelectBarrier(
                source,
                freshSnapshotSeen,
                runtime,
                hasSelectedNonEnemyConfirmEvidence),
            CombatBarrierKind.CancelSelection => EvaluateCancelSelectionBarrier(
                source,
                freshSnapshotSeen,
                runtime,
                hasSelectedNonEnemyConfirmEvidence),
            CombatBarrierKind.EndTurn => EvaluateEndTurnBarrier(
                source,
                observer,
                combatResolutionHandoffState,
                freshSnapshotSeen,
                runtime,
                combatPlayerActionWindowClosed),
            _ => Inactive,
        };
    }

    public static bool SuppressesAttackSlot(CombatBarrierEvaluation barrier, int slotIndex)
    {
        return barrier.IsActive
               && barrier.Kind == CombatBarrierKind.AttackSelect
               && barrier.SourceSlotIndex == slotIndex;
    }

    public static bool SuppressesNonEnemySlot(CombatBarrierEvaluation barrier, int slotIndex)
    {
        return barrier.IsActive
               && barrier.Kind == CombatBarrierKind.NonEnemySelect
               && barrier.SourceSlotIndex == slotIndex;
    }

    public static string? BuildHistoryMetadataForDecision(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        if (!string.Equals(request.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
            || !string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!TryResolveBarrierKind(decision.TargetLabel, decision.ActionKind, out _))
        {
            return null;
        }

        var runtime = CombatRuntimeStateSupport.Read(request.Observer, request.CombatCardKnowledge);
        return JsonSerializer.Serialize(
            new CombatBarrierHistoryMetadata(
                request.Observer.SnapshotVersion,
                request.Observer.CapturedAt,
                request.Observer.SceneEpisodeId,
                runtime.InteractionRevision,
                runtime.HistoryStartedCount,
                runtime.HistoryFinishedCount,
                runtime.LastCardPlayFinishedCardId)
            {
                RoundNumber = runtime.RoundNumber,
                PlayerActionsDisabled = runtime.PlayerActionsDisabled,
                EndingPlayerTurnPhaseOne = runtime.EndingPlayerTurnPhaseOne,
                EndingPlayerTurnPhaseTwo = runtime.EndingPlayerTurnPhaseTwo,
            },
            GuiSmokeShared.JsonOptions);
    }

    public static bool TryClassifyWaitPlateau(
        GuiSmokeStepRequest request,
        GuiSmokeStepAnalysisContext context,
        int consecutiveDecisionWaitCount,
        out string terminalCause,
        out string message)
    {
        var barrier = context.CombatBarrierEvaluation;
        var releaseState = context.CombatReleaseState;
        if (consecutiveDecisionWaitCount < CombatBarrierPolicy.HandleCombatWaitPlateauLimit)
        {
            terminalCause = string.Empty;
            message = string.Empty;
            return false;
        }

        if (releaseState.HasReleasedOwnership
            && releaseState.ReleaseTarget is not NonCombatHandoffTarget.None and not NonCombatHandoffTarget.HandleCombat
            && releaseState.ReleaseSubtype != CombatReleaseSubtype.None)
        {
            terminalCause = "combat-release-failure-under-noncombat-foreground";
            message = $"combat-release-failure-under-noncombat-foreground phase=HandleCombat barrier={barrier.Kind} subtype={releaseState.ReleaseSubtype} owner={releaseState.ForegroundOwner} target={releaseState.ReleaseTarget} screen={request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "null"} waits={consecutiveDecisionWaitCount}";
            return true;
        }

        if (releaseState.LifecycleStage == CombatLifecycleStage.CombatEntryPending)
        {
            terminalCause = "combat-entry-surface-pending-wait-plateau";
            message = $"combat-entry-surface-pending-wait-plateau phase=HandleCombat lifecycle={releaseState.LifecycleStage} barrier={barrier.Kind} waits={consecutiveDecisionWaitCount} round={context.RuntimeCombatState.RoundNumber?.ToString(CultureInfo.InvariantCulture) ?? "none"} interactionRevision={context.RuntimeCombatState.InteractionRevision ?? "none"} inventory={request.Observer.InventoryId ?? "null"}";
            return true;
        }

        if (releaseState.LifecycleStage is CombatLifecycleStage.EndTurnTransit
            or CombatLifecycleStage.EnemyTurn
            or CombatLifecycleStage.PlayerReopenPending)
        {
            terminalCause = "combat-lifecycle-transit-wait-plateau";
            message = $"combat-lifecycle-transit-wait-plateau phase=HandleCombat lifecycle={releaseState.LifecycleStage} barrier={barrier.Kind} waits={consecutiveDecisionWaitCount} round={context.RuntimeCombatState.RoundNumber?.ToString(CultureInfo.InvariantCulture) ?? "none"} interactionRevision={context.RuntimeCombatState.InteractionRevision ?? "none"} inventory={request.Observer.InventoryId ?? "null"}";
            return true;
        }

        if (!barrier.IsActive
            || !barrier.IsHardWaitBarrier
            || !barrier.OverWaitRisk)
        {
            terminalCause = string.Empty;
            message = string.Empty;
            return false;
        }

        terminalCause = "combat-barrier-wait-plateau";
        message = $"combat-barrier-wait-plateau phase=HandleCombat barrier={barrier.Kind} source={barrier.SourceAction ?? "null"} waits={consecutiveDecisionWaitCount} armedSnapshot={barrier.ArmedSnapshotVersion?.ToString(CultureInfo.InvariantCulture) ?? "none"} currentSnapshot={request.Observer.SnapshotVersion?.ToString(CultureInfo.InvariantCulture) ?? "none"} inventory={request.Observer.InventoryId ?? "null"}";
        return true;
    }

    private static CombatBarrierEvaluation EvaluateAttackSelectBarrier(
        BarrierSource source,
        bool freshSnapshotSeen,
        CombatRuntimeState runtime,
        AutoCombatAnalysis analysis,
        bool canResolveCombatEnemyTarget)
    {
        var attackSelectionProgressSeen = freshSnapshotSeen
                                         || HasAttackSelectionProgressSinceSource(source.Metadata, runtime);
        if (attackSelectionProgressSeen
            && runtime.HasInFlightPlayerDrivenAction)
        {
            return Released(source, "selected combat card entered the play queue");
        }

        if (attackSelectionProgressSeen
            && canResolveCombatEnemyTarget)
        {
            return Released(source, "enemy-target authority surfaced after attack selection");
        }

        if (attackSelectionProgressSeen
            && runtime.ExplicitlyClearedSelection
            && runtime.PendingSelection is null
            && !runtime.HasCardSelectionEvidence)
        {
            return Released(source, "attack selection cleared without pending target authority");
        }

        return Active(
            source,
            attackSelectionProgressSeen
                ? "attack selection is still awaiting targetable enemy authority or explicit clear"
                : "waiting for fresh post-attack-select progress",
            false,
            attackSelectionProgressSeen);
    }

    private static CombatBarrierEvaluation EvaluateEnemyClickBarrier(
        BarrierSource source,
        ObserverState observer,
        PostNodeHandoffState combatResolutionHandoffState,
        bool freshSnapshotSeen,
        CombatRuntimeState runtime,
        bool canResolveCombatEnemyTarget)
    {
        var finishedCountAdvanced = source.Metadata?.HistoryFinishedCount is not null
                                    && runtime.HistoryFinishedCount is not null
                                    && runtime.HistoryFinishedCount > source.Metadata.HistoryFinishedCount;
        var finishedCardChanged = !string.IsNullOrWhiteSpace(runtime.LastCardPlayFinishedCardId)
                                  && !string.Equals(runtime.LastCardPlayFinishedCardId, source.Metadata?.LastFinishedCardId, StringComparison.OrdinalIgnoreCase);
        var explicitClear = runtime.ExplicitlyClearedSelection
                            && !runtime.HasExplicitTargetableEnemy
                            && !runtime.HasExplicitHittableEnemy
                            && runtime.TargetingInProgress != true;
        var finishedOrClearedWithoutLiveOwnership = !canResolveCombatEnemyTarget
                                                    && !runtime.HasLiveCardPlayOwnership
                                                    && (finishedCountAdvanced || finishedCardChanged || explicitClear);
        if (finishedOrClearedWithoutLiveOwnership)
        {
            return Released(source, "enemy click lane lost live ownership and now shows explicit finish or clear evidence");
        }

        if (CanReleaseIntoResolvedNonCombatHandoff(combatResolutionHandoffState, observer, runtime, canResolveCombatEnemyTarget))
        {
            return Released(source, BuildResolvedCombatHandoffReason(combatResolutionHandoffState));
        }

        if (!freshSnapshotSeen)
        {
            return Active(source, "waiting for a fresh post-enemy-click snapshot", false, true);
        }

        if (canResolveCombatEnemyTarget)
        {
            return Released(source, "targeting authority is still alive after the click");
        }

        if (finishedCountAdvanced || finishedCardChanged || explicitClear)
        {
            return Released(source, "enemy click resolved into finish or clear evidence");
        }

        return Active(
            source,
            $"enemy click is still awaiting finish/clear evidence screen={observer.CurrentScreen ?? observer.VisibleScreen ?? "null"}",
            false,
            true);
    }

    private static CombatBarrierEvaluation EvaluateNonEnemySelectBarrier(
        BarrierSource source,
        bool freshSnapshotSeen,
        CombatRuntimeState runtime,
        bool hasSelectedNonEnemyConfirmEvidence)
    {
        if (freshSnapshotSeen && hasSelectedNonEnemyConfirmEvidence)
        {
            return Released(source, "non-enemy confirm evidence surfaced");
        }

        var attackTargetingSuperseded = freshSnapshotSeen
                                        && runtime.PendingSelection?.Kind == AutoCombatCardKind.AttackLike
                                        && (runtime.HasExplicitEnemyTargetingEvidence
                                            || runtime.HasExplicitTargetableEnemy
                                            || runtime.HasExplicitHittableEnemy);
        if (attackTargetingSuperseded)
        {
            return Released(source, "non-enemy selection superseded by explicit attack-target authority");
        }

        if (freshSnapshotSeen
            && runtime.ExplicitlyClearedSelection
            && runtime.PendingSelection is null
            && !runtime.HasCardSelectionEvidence
            && !hasSelectedNonEnemyConfirmEvidence)
        {
            return Released(source, "non-enemy selection cleared without confirm evidence");
        }

        return Active(
            source,
            freshSnapshotSeen
                ? "non-enemy selection is still awaiting confirm evidence or explicit clear"
                : "waiting for a fresh post-non-enemy-select snapshot",
            false,
            freshSnapshotSeen);
    }

    private static CombatBarrierEvaluation EvaluateCancelSelectionBarrier(
        BarrierSource source,
        bool freshSnapshotSeen,
        CombatRuntimeState runtime,
        bool hasSelectedNonEnemyConfirmEvidence)
    {
        if (freshSnapshotSeen
            && !runtime.HasCardSelectionEvidence
            && runtime.PendingSelection is null
            && !hasSelectedNonEnemyConfirmEvidence)
        {
            return Released(source, "selection cancel completed");
        }

        return Active(
            source,
            freshSnapshotSeen
                ? "cancel selection is still awaiting a cleared combat snapshot"
                : "waiting for a fresh post-cancel snapshot",
            false,
            freshSnapshotSeen);
    }

    private static CombatBarrierEvaluation EvaluateEndTurnBarrier(
        BarrierSource source,
        ObserverState observer,
        PostNodeHandoffState combatResolutionHandoffState,
        bool freshSnapshotSeen,
        CombatRuntimeState runtime,
        bool combatPlayerActionWindowClosed)
    {
        if (CanReleaseIntoResolvedNonCombatHandoff(combatResolutionHandoffState, observer, runtime, canResolveCombatEnemyTarget: false))
        {
            return Released(source, BuildResolvedCombatHandoffReason(combatResolutionHandoffState));
        }

        if (observer.InCombat != true
            || !MatchesControlFlowScreen(observer, "combat"))
        {
            return Released(source, "combat exited after end turn");
        }

        var reopenedPlayerWindow = IsReopenedPlayerActionWindow(observer.Summary, runtime);
        var roundAdvanced = runtime.RoundNumber is not null
                            && source.Metadata?.RoundNumber is not null
                            && runtime.RoundNumber > source.Metadata.RoundNumber;
        if (roundAdvanced && reopenedPlayerWindow)
        {
            return Released(
                source,
                $"next player turn reopened after round advanced from {source.Metadata!.RoundNumber!.Value.ToString(CultureInfo.InvariantCulture)} to {runtime.RoundNumber!.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (freshSnapshotSeen
            && reopenedPlayerWindow
            && !HasEndTurnTransitionAcknowledgement(observer.Summary, runtime, combatPlayerActionWindowClosed))
        {
            return Released(source, "player action window remained open after end-turn submission");
        }

        if (!freshSnapshotSeen)
        {
            return Active(source, "waiting for a fresh post-end-turn snapshot", false, true);
        }

        if (HasEndTurnTransitionAcknowledgement(observer.Summary, runtime, combatPlayerActionWindowClosed))
        {
            return Active(source, "end turn acknowledged; waiting for the next round reopen", false, false);
        }

        return Active(
            source,
            "end turn has not yet been acknowledged by combat turn transition",
            false,
            true);
    }

    private static bool HasEndTurnTransitionAcknowledgement(
        ObserverSummary observer,
        CombatRuntimeState runtime,
        bool combatPlayerActionWindowClosed)
    {
        return combatPlayerActionWindowClosed
               || runtime.PlayerActionsDisabled == true
               || runtime.EndingPlayerTurnPhaseOne == true
               || runtime.EndingPlayerTurnPhaseTwo == true
               || CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsEnemyTurnStarted", "CombatManager.IsEnemyTurnStarted") == true
               || CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsPlayPhase", "CombatManager.IsPlayPhase") == false;
    }

    private static bool IsReopenedPlayerActionWindow(ObserverSummary observer, CombatRuntimeState runtime)
    {
        var isPlayPhase = CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsPlayPhase", "CombatManager.IsPlayPhase");
        var isEnemyTurnStarted = CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsEnemyTurnStarted", "CombatManager.IsEnemyTurnStarted");
        return isPlayPhase == true
               && isEnemyTurnStarted == false
               && runtime.PlayerActionsDisabled == false
               && runtime.EndingPlayerTurnPhaseOne == false
               && runtime.EndingPlayerTurnPhaseTwo == false;
    }

    public static string? TryBuildSafeTransitProgressFingerprint(CombatBarrierEvaluation barrier, ObserverSummary observer)
    {
        if (!barrier.IsActive
            || !barrier.IsHardWaitBarrier
            || barrier.Kind != CombatBarrierKind.EndTurn
            || barrier.OverWaitRisk)
        {
            return null;
        }

        return string.Join("|",
            observer.SnapshotVersion?.ToString(CultureInfo.InvariantCulture) ?? "snapshot:none",
            observer.CapturedAt?.ToString("O", CultureInfo.InvariantCulture) ?? "captured:none",
            CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatRoundNumber", "CombatState.RoundNumber")?.ToString(CultureInfo.InvariantCulture) ?? "round:none",
            FormatNullableBool(CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsPlayPhase", "CombatManager.IsPlayPhase")),
            FormatNullableBool(CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsEnemyTurnStarted", "CombatManager.IsEnemyTurnStarted")),
            FormatNullableBool(CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatPlayerActionsDisabled", "CombatManager.PlayerActionsDisabled")),
            FormatNullableBool(CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatEndingPlayerTurnPhaseOne", "CombatManager.EndingPlayerTurnPhaseOne")),
            FormatNullableBool(CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatEndingPlayerTurnPhaseTwo", "CombatManager.EndingPlayerTurnPhaseTwo")));
    }

    private static string FormatNullableBool(bool? value)
    {
        return value switch
        {
            true => "true",
            false => "false",
            _ => "null",
        };
    }

    private static bool CanReleaseIntoResolvedNonCombatHandoff(
        PostNodeHandoffState combatResolutionHandoffState,
        ObserverState observer,
        CombatRuntimeState runtime,
        bool canResolveCombatEnemyTarget)
    {
        return combatResolutionHandoffState.Owner != NonCombatCanonicalForegroundOwner.Combat
               && combatResolutionHandoffState.HandoffTarget is not NonCombatHandoffTarget.None and not NonCombatHandoffTarget.HandleCombat
               && (combatResolutionHandoffState.HasExplicitSurface
                   || combatResolutionHandoffState.ReleaseStage != NonCombatReleaseStage.None)
               && !runtime.HasLiveCardPlayOwnership
               && !canResolveCombatEnemyTarget;
    }

    private static string BuildResolvedCombatHandoffReason(PostNodeHandoffState combatResolutionHandoffState)
    {
        var owner = combatResolutionHandoffState.Owner.ToString().ToLowerInvariant();
        var target = combatResolutionHandoffState.HandoffTarget.ToString().ToLowerInvariant();
        var surface = combatResolutionHandoffState.SurfaceKind.ToString().ToLowerInvariant();
        return $"combat authority released into {owner} foreground handoff target={target} surface={surface}";
    }

    private static CombatBarrierEvaluation Released(BarrierSource source, string reason)
    {
        return new CombatBarrierEvaluation(
            false,
            source.Kind,
            reason,
            source.Entry.TargetLabel,
            source.SlotIndex,
            source.Metadata?.SnapshotVersion,
            source.Metadata?.CapturedAt ?? source.Entry.RecordedAt,
            true,
            false);
    }

    private static CombatBarrierEvaluation Active(BarrierSource source, string reason, bool releaseSatisfied, bool overWaitRisk)
    {
        return new CombatBarrierEvaluation(
            true,
            source.Kind,
            reason,
            source.Entry.TargetLabel,
            source.SlotIndex,
            source.Metadata?.SnapshotVersion,
            source.Metadata?.CapturedAt ?? source.Entry.RecordedAt,
            releaseSatisfied,
            overWaitRisk);
    }

    private static BarrierSource? TryFindSource(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (IsMeaningfulCombatHistoryAction(entry.Action)
                && TryResolveBarrierKind(entry.TargetLabel, entry.Action, out var kind))
            {
                var slotIndex = CombatHistorySupport.TryParsePendingCombatSelection(entry.TargetLabel, out var selection)
                    ? selection?.SlotIndex
                    : null;
                return new BarrierSource(kind, entry, slotIndex, TryParseHistoryMetadata(entry.Metadata));
            }

            if (IsMeaningfulCombatHistoryAction(entry.Action))
            {
                return null;
            }
        }

        return null;
    }

    private static bool HasFreshSnapshotSinceSource(
        ObserverState observer,
        CombatBarrierHistoryMetadata? metadata,
        GuiSmokeHistoryEntry entry)
    {
        if (metadata?.SnapshotVersion is not null
            && observer.SnapshotVersion is not null
            && observer.SnapshotVersion > metadata.SnapshotVersion)
        {
            return true;
        }

        if (metadata?.CapturedAt is not null
            && observer.CapturedAt is not null
            && observer.CapturedAt > metadata.CapturedAt)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(metadata?.ScreenEpisodeId)
            && !string.Equals(observer.Summary.SceneEpisodeId, metadata.ScreenEpisodeId, StringComparison.Ordinal))
        {
            return true;
        }

        return observer.CapturedAt is not null && observer.CapturedAt > entry.RecordedAt;
    }

    private static bool HasAttackSelectionProgressSinceSource(
        CombatBarrierHistoryMetadata? metadata,
        CombatRuntimeState runtime)
    {
        if (metadata is null)
        {
            return false;
        }

        if (metadata.RoundNumber is not null
            && runtime.RoundNumber is not null
            && runtime.RoundNumber != metadata.RoundNumber)
        {
            return true;
        }

        if (metadata.HistoryStartedCount is not null
            && runtime.HistoryStartedCount is not null
            && runtime.HistoryStartedCount != metadata.HistoryStartedCount)
        {
            return true;
        }

        if (metadata.HistoryFinishedCount is not null
            && runtime.HistoryFinishedCount is not null
            && runtime.HistoryFinishedCount != metadata.HistoryFinishedCount)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(metadata.InteractionRevision)
            && !string.Equals(runtime.InteractionRevision, metadata.InteractionRevision, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return !string.Equals(runtime.LastCardPlayFinishedCardId, metadata.LastFinishedCardId, StringComparison.OrdinalIgnoreCase);
    }

    internal static CombatBarrierHistoryMetadata? TryParseHistoryMetadata(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
        {
            return null;
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<CombatBarrierHistoryMetadata>(metadata, GuiSmokeShared.JsonOptions);
            return parsed is not null
                   && (parsed.SnapshotVersion is not null
                       || parsed.CapturedAt is not null
                       || !string.IsNullOrWhiteSpace(parsed.ScreenEpisodeId)
                       || !string.IsNullOrWhiteSpace(parsed.InteractionRevision))
                ? parsed
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool TryResolveBarrierKind(string? targetLabel, string? action, out CombatBarrierKind kind)
    {
        if (CombatHistorySupport.TryParsePendingCombatSelection(targetLabel, out var selection))
        {
            kind = selection?.Kind == AutoCombatCardKind.AttackLike
                ? CombatBarrierKind.AttackSelect
                : CombatBarrierKind.NonEnemySelect;
            return kind != CombatBarrierKind.None;
        }

        if (IsCombatEnemyTargetLabel(targetLabel))
        {
            kind = CombatBarrierKind.EnemyClick;
            return true;
        }

        if (string.Equals(action, "right-click", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(targetLabel))
        {
            kind = CombatBarrierKind.CancelSelection;
            return true;
        }

        if (IsCombatEndTurnActionLabel(targetLabel))
        {
            kind = CombatBarrierKind.EndTurn;
            return true;
        }

        kind = CombatBarrierKind.None;
        return false;
    }

    internal static bool IsMeaningfulCombatHistoryAction(string action)
    {
        return string.Equals(action, "click", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "click-current", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "confirm-attack-card", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "right-click", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "press-key", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCombatEndTurnActionLabel(string? targetLabel)
    {
        return targetLabel is not null
               && (string.Equals(targetLabel, "click end turn", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.Contains("end turn", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.Contains("auto-end turn", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsCombatEnemyTargetLabel(string? targetLabel)
    {
        return targetLabel is not null
               && (targetLabel.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase));
    }

    public static CombatBarrierEvaluation Inactive { get; } = new(
        false,
        CombatBarrierKind.None,
        string.Empty,
        null,
        null,
        null,
        null,
        false,
        false);
}
