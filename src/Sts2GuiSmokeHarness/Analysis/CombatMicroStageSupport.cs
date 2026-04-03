using System.Globalization;

enum CombatMicroStageKind
{
    PlayerActionOpen,
    ResolvingOverlayCardSelection,
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
        var cardSelectionState = CardSelectionObserverSignals.TryGetState(context.Observer.Summary);
        var kind = ResolveKind(context, runtime, pendingSelection, barrier, effectiveBarrierKind, cardSelectionState);
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
                cardSelectionState,
                context.HasSelectedNonEnemyConfirmEvidence,
                context.CanResolveCombatEnemyTarget));
    }

    private static CombatMicroStageKind ResolveKind(
        GuiSmokeStepAnalysisContext context,
        CombatRuntimeState runtime,
        PendingCombatSelection? pendingSelection,
        CombatBarrierEvaluation barrier,
        CombatBarrierKind effectiveBarrierKind,
        CardSelectionSubtypeState? cardSelectionState)
    {
        var carriedAttackLaneOpen = pendingSelection?.Kind == AutoCombatCardKind.AttackLike;
        var carriedAttackLaneBlocked = carriedAttackLaneOpen
                                       && pendingSelection!.SlotIndex is >= 1 and <= 5
                                       && HandleCombatContextSupport.GetCombatNoOpCountForSlot(context.CombatContext, pendingSelection.SlotIndex) >= 2;
        var retireResolvedAttackSelectionTail = pendingSelection is null
                                                && CombatRuntimeStateSupport.ShouldRetireResolvedAttackSelectionTail(
                                                    context.Observer.Summary,
                                                    context.CombatCardKnowledge,
                                                    context.CombatContext.PendingSelection,
                                                    context.History);
        var barrierAttackLaneOpen = effectiveBarrierKind is CombatBarrierKind.AttackSelect or CombatBarrierKind.EnemyClick
                                    && (barrier.SourceSlotIndex is not int barrierSourceSlotIndex
                                        || barrierSourceSlotIndex is < 1 or > 5
                                        || HandleCombatContextSupport.GetCombatNoOpCountForSlot(context.CombatContext, barrierSourceSlotIndex) < 2);
        var runtimeAttackLaneOpen = runtime.PendingSelection?.Kind == AutoCombatCardKind.AttackLike
                                    || barrierAttackLaneOpen;
        var attackLaneOpen = runtimeAttackLaneOpen || (carriedAttackLaneOpen && !carriedAttackLaneBlocked);
        var attackRequiresExplicitEnemyTarget = attackLaneOpen
                                                && CombatRuntimeStateSupport.RequiresExplicitTargetingBeforeEnemyClick(
                                                    context.Observer.Summary,
                                                    context.CombatCardKnowledge,
                                                    pendingSelection);
        var hasExplicitAttackLaneEvidence = runtimeAttackLaneOpen
                                            || (!retireResolvedAttackSelectionTail
                                                && CombatRuntimeStateSupport.HasSelectedAttackMetadata(runtime.SelectedCardType, runtime.SelectedCardTargetType))
                                            || (carriedAttackLaneOpen
                                                && !carriedAttackLaneBlocked
                                                && (runtime.HasExplicitEnemyTargetingEvidence
                                                    || context.CanResolveCombatEnemyTarget));
        var attackConfirmReady = attackLaneOpen
                                 && !attackRequiresExplicitEnemyTarget
                                 && CombatRuntimeStateSupport.HasPositiveAttackConfirmEvidence(
                                     context.Observer.Summary,
                                     context.CombatCardKnowledge,
                                     pendingSelection);

        if (context.CombatPlayerActionWindowClosed)
        {
            return CombatMicroStageKind.EnemyTurnClosed;
        }

        if (cardSelectionState is not null)
        {
            return CombatMicroStageKind.ResolvingOverlayCardSelection;
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
            && hasExplicitAttackLaneEvidence)
        {
            return CombatMicroStageKind.ResolvingAttackTarget;
        }

        if (attackConfirmReady)
        {
            return CombatMicroStageKind.AwaitingCardPlayConfirm;
        }

        if (runtime.HasInFlightPlayerDrivenAction
            && HasRecentPlayCommitAction(context.History))
        {
            return CombatMicroStageKind.ResolvingCardPlay;
        }

        if (pendingSelection?.Kind == AutoCombatCardKind.DefendLike
            || effectiveBarrierKind == CombatBarrierKind.NonEnemySelect
            || context.HasSelectedNonEnemyConfirmEvidence)
        {
            return CombatMicroStageKind.ResolvingNonEnemy;
        }

        if (runtime.HasBlockingCardPlayResolution)
        {
            return CombatMicroStageKind.ResolvingCardPlay;
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

    private static bool HasRecentPlayCommitAction(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        for (var index = history.Count - 1; index >= 0 && index >= history.Count - 3; index -= 1)
        {
            var entry = history[index];
            if (string.Equals(entry.Action, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Action, "confirm-attack-card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.TargetLabel, "confirm selected hand card", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(entry.Action, "observer-accepted", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Action, "recapture-required", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return false;
        }

        return false;
    }

    private static string BuildSettledFingerprint(
        CombatMicroStageKind kind,
        string? laneLabel,
        PendingCombatSelection? pendingSelection,
        CombatBarrierKind barrierKind,
        CombatRuntimeState runtime,
        CardSelectionSubtypeState? cardSelectionState,
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
            $"card-selection:{cardSelectionState?.ScreenType ?? "none"}",
            $"card-selection-selected:{cardSelectionState?.SelectedCount.ToString(CultureInfo.InvariantCulture) ?? "none"}",
            $"card-selection-confirm:{(cardSelectionState is null ? "false" : CardSelectionObserverSignals.IsConfirmReady(cardSelectionState).ToString().ToLowerInvariant())}",
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
