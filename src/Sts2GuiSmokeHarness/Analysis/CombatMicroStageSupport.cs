using System.Globalization;

enum CombatMicroStageKind
{
    PlayerActionOpen,
    HandSelectRequired,
    ResolvingNonEnemy,
    ResolvingAttackTarget,
    AwaitingCardPlayConfirm,
    ResolvingCardPlay,
    TurnClosing,
    EnemyTurnClosed,
}

sealed record CombatMicroStageSnapshot(
    CombatMicroStageKind Kind,
    PendingCombatSelection? PendingSelection,
    CombatBarrierKind BarrierKind,
    string? LaneLabel,
    bool AllowsNewActionStart,
    bool AllowsEndTurn,
    bool AllowsCancel,
    string SettledFingerprint);

static class CombatMicroStageSupport
{
    public static CombatMicroStageSnapshot Resolve(GuiSmokeStepAnalysisContext context)
    {
        var runtime = context.RuntimeCombatState;
        var pendingSelection = context.PendingCombatSelection;
        var barrier = context.CombatBarrierEvaluation;
        var effectiveBarrierKind = barrier.IsActive ? barrier.Kind : CombatBarrierKind.None;
        var kind = ResolveKind(context, runtime, pendingSelection, barrier, effectiveBarrierKind);
        var laneLabel = ResolveLaneLabel(context.History, pendingSelection, barrier, effectiveBarrierKind);
        return new CombatMicroStageSnapshot(
            kind,
            pendingSelection,
            effectiveBarrierKind,
            laneLabel,
            AllowsNewActionStart: kind == CombatMicroStageKind.PlayerActionOpen,
            AllowsEndTurn: kind == CombatMicroStageKind.PlayerActionOpen,
            AllowsCancel: kind is CombatMicroStageKind.ResolvingNonEnemy or CombatMicroStageKind.ResolvingAttackTarget or CombatMicroStageKind.AwaitingCardPlayConfirm,
            SettledFingerprint: BuildSettledFingerprint(
                kind,
                laneLabel,
                pendingSelection,
                effectiveBarrierKind,
                runtime,
                context.HasSelectedNonEnemyConfirmEvidence,
                context.CanResolveCombatEnemyTarget));
    }

    private static CombatMicroStageKind ResolveKind(
        GuiSmokeStepAnalysisContext context,
        CombatRuntimeState runtime,
        PendingCombatSelection? pendingSelection,
        CombatBarrierEvaluation barrier,
        CombatBarrierKind effectiveBarrierKind)
    {
        var attackLaneOpen = pendingSelection?.Kind == AutoCombatCardKind.AttackLike
                             || runtime.PendingSelection?.Kind == AutoCombatCardKind.AttackLike
                             || effectiveBarrierKind is CombatBarrierKind.AttackSelect or CombatBarrierKind.EnemyClick;
        var attackRequiresExplicitEnemyTarget = attackLaneOpen
                                                && CombatRuntimeStateSupport.RequiresExplicitTargetingBeforeEnemyClick(
                                                    context.Observer.Summary,
                                                    context.CombatCardKnowledge,
                                                    pendingSelection);

        if (context.CombatPlayerActionWindowClosed)
        {
            return CombatMicroStageKind.EnemyTurnClosed;
        }

        if (runtime.RequiresHandCardSelection)
        {
            return CombatMicroStageKind.HandSelectRequired;
        }

        if (effectiveBarrierKind == CombatBarrierKind.EndTurn
            || runtime.PlayerActionsDisabled == true
            || runtime.EndingPlayerTurnPhaseOne == true
            || runtime.EndingPlayerTurnPhaseTwo == true)
        {
            return CombatMicroStageKind.TurnClosing;
        }

        if (attackRequiresExplicitEnemyTarget
            && (attackLaneOpen
                || (runtime.PendingSelection?.Kind == AutoCombatCardKind.AttackLike
                    && (runtime.HasExplicitEnemyTargetingEvidence
                        || context.CanResolveCombatEnemyTarget))))
        {
            return CombatMicroStageKind.ResolvingAttackTarget;
        }

        if (attackLaneOpen
            && !attackRequiresExplicitEnemyTarget)
        {
            return runtime.HasBlockingCardPlayResolution
                ? CombatMicroStageKind.ResolvingCardPlay
                : CombatMicroStageKind.AwaitingCardPlayConfirm;
        }

        if (runtime.HasBlockingCardPlayResolution)
        {
            return CombatMicroStageKind.ResolvingCardPlay;
        }

        if (pendingSelection?.Kind == AutoCombatCardKind.DefendLike
            || effectiveBarrierKind == CombatBarrierKind.NonEnemySelect
            || context.HasSelectedNonEnemyConfirmEvidence)
        {
            return CombatMicroStageKind.ResolvingNonEnemy;
        }

        return CombatMicroStageKind.PlayerActionOpen;
    }

    private static string? ResolveLaneLabel(
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        PendingCombatSelection? pendingSelection,
        CombatBarrierEvaluation barrier,
        CombatBarrierKind effectiveBarrierKind)
    {
        if (pendingSelection?.SlotIndex is >= 1 and <= 10)
        {
            return $"combat lane slot {pendingSelection.SlotIndex.ToString(CultureInfo.InvariantCulture)}";
        }

        if (effectiveBarrierKind != CombatBarrierKind.None
            && barrier.SourceSlotIndex is >= 1 and <= 10)
        {
            return $"combat lane slot {barrier.SourceSlotIndex.Value.ToString(CultureInfo.InvariantCulture)}";
        }

        if (effectiveBarrierKind != CombatBarrierKind.None
            && !string.IsNullOrWhiteSpace(barrier.SourceAction))
        {
            return CombatHistorySupport.ResolveCombatLaneLabel(barrier.SourceAction, history) ?? barrier.SourceAction;
        }

        return null;
    }

    private static string BuildSettledFingerprint(
        CombatMicroStageKind kind,
        string? laneLabel,
        PendingCombatSelection? pendingSelection,
        CombatBarrierKind barrierKind,
        CombatRuntimeState runtime,
        bool hasSelectedNonEnemyConfirmEvidence,
        bool canResolveCombatEnemyTarget)
    {
        return string.Join("|",
            $"stage:{kind}",
            $"lane:{laneLabel ?? "none"}",
            $"pending-slot:{pendingSelection?.SlotIndex.ToString(CultureInfo.InvariantCulture) ?? "none"}",
            $"pending-kind:{pendingSelection?.Kind.ToString() ?? "none"}",
            $"barrier:{barrierKind}",
            $"card-play-pending:{FormatNullableBool(runtime.CardPlayPending)}",
            $"targeting:{FormatNullableBool(runtime.TargetingInProgress)}",
            $"live-card-play-ownership:{runtime.HasLiveCardPlayOwnership.ToString()}",
            $"blocking-card-play-resolution:{runtime.HasBlockingCardPlayResolution.ToString()}",
            $"hand-selected:{runtime.HandSelectionSelectedCount.ToString(CultureInfo.InvariantCulture)}",
            $"hand-confirm:{FormatNullableBool(runtime.HandSelectionConfirmEnabled)}",
            $"confirm-evidence:{hasSelectedNonEnemyConfirmEvidence.ToString()}",
            $"can-resolve-enemy:{canResolveCombatEnemyTarget.ToString()}",
            $"interaction:{runtime.InteractionRevision ?? "none"}",
            $"history-started:{runtime.HistoryStartedCount?.ToString(CultureInfo.InvariantCulture) ?? "none"}",
            $"history-finished:{runtime.HistoryFinishedCount?.ToString(CultureInfo.InvariantCulture) ?? "none"}",
            $"last-started:{runtime.LastCardPlayStartedCardId ?? "none"}",
            $"last-finished:{runtime.LastCardPlayFinishedCardId ?? "none"}",
            $"round:{runtime.RoundNumber?.ToString(CultureInfo.InvariantCulture) ?? "none"}",
            $"actions-disabled:{FormatNullableBool(runtime.PlayerActionsDisabled)}",
            $"ending-p1:{FormatNullableBool(runtime.EndingPlayerTurnPhaseOne)}",
            $"ending-p2:{FormatNullableBool(runtime.EndingPlayerTurnPhaseTwo)}");
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
}
