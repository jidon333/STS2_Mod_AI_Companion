static class CombatPostActionObservationSupport
{
    public const int QuietConvergencePollingSliceMs = 75;
    public const int QuietConvergenceStablePollCount = 4;
    private const int NonEnemySelectMinimumSettleMs = 1200;
    private const int CombatLaneMinimumSettleMs = 900;

    public static bool ShouldUseStageAwareObservation(GuiSmokePhase phase, GuiSmokeStepDecision decision)
    {
        return phase == GuiSmokePhase.HandleCombat
               && string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase);
    }

    public static int GetMinimumSettleDelayMs(GuiSmokeStepDecision decision)
    {
        if (IsNonEnemySelectionDecision(decision))
        {
            return NonEnemySelectMinimumSettleMs;
        }

        if (IsNonEnemyConfirmDecision(decision)
            || IsAttackConfirmDecision(decision)
            || IsAttackSelectionDecision(decision)
            || IsEnemyTargetDecision(decision)
            || IsCancelSelectionDecision(decision))
        {
            return CombatLaneMinimumSettleMs;
        }

        return 0;
    }

    public static Func<ObserverState, string?> CreateWakeEvaluator(
        ObserverState baselineObserver,
        GuiSmokeStepDecision decision,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        WindowBounds? windowBounds)
    {
        var tracker = new CombatPostActionObservationTracker(baselineObserver, decision, history, combatCardKnowledge, windowBounds);
        return tracker.Evaluate;
    }

    private sealed class CombatPostActionObservationTracker
    {
        private readonly ObserverState _baselineObserver;
        private readonly GuiSmokeStepDecision _decision;
        private readonly IReadOnlyList<GuiSmokeHistoryEntry> _history;
        private readonly IReadOnlyList<CombatCardKnowledgeHint> _combatCardKnowledge;
        private readonly WindowBounds? _windowBounds;
        private readonly GuiSmokeStepAnalysisContext _baselineContext;
        private readonly CombatMicroStageSnapshot _baselineStage;
        private string? _stableFingerprint;
        private int _stablePollCount;

        public CombatPostActionObservationTracker(
            ObserverState baselineObserver,
            GuiSmokeStepDecision decision,
            IReadOnlyList<GuiSmokeHistoryEntry> history,
            IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
            WindowBounds? windowBounds)
        {
            _baselineObserver = baselineObserver;
            _decision = decision;
            _history = history;
            _combatCardKnowledge = combatCardKnowledge;
            _windowBounds = windowBounds;
            _baselineContext = GuiSmokeStepRequestFactory.CreateObserverOnlyAnalysisContext(
                GuiSmokePhase.HandleCombat,
                baselineObserver,
                history,
                combatCardKnowledge,
                windowBounds);
            _baselineStage = _baselineContext.CombatMicroStage;
        }

        public string? Evaluate(ObserverState latestObserver)
        {
            var context = GuiSmokeStepRequestFactory.CreateObserverOnlyAnalysisContext(
                GuiSmokePhase.HandleCombat,
                latestObserver,
                _history,
                _combatCardKnowledge,
                _windowBounds);
            var stage = context.CombatMicroStage;
            var hasFreshObservationProgress = HasFreshObservationProgress(context, stage);
            if (TryGetTerminalSignal(context, stage, hasFreshObservationProgress, out var terminalReason))
            {
                ResetStableFingerprint();
                return terminalReason;
            }

            if (!IsAcceptableSettledStage(stage) || !hasFreshObservationProgress)
            {
                ResetStableFingerprint();
                return null;
            }

            if (!string.Equals(_stableFingerprint, stage.SettledFingerprint, StringComparison.Ordinal))
            {
                _stableFingerprint = stage.SettledFingerprint;
                _stablePollCount = 1;
                return null;
            }

            _stablePollCount += 1;
            return _stablePollCount >= QuietConvergenceStablePollCount
                ? $"combat-quiet-convergence:{stage.Kind.ToString().ToLowerInvariant()}"
                : null;
        }

        private bool TryGetTerminalSignal(
            GuiSmokeStepAnalysisContext context,
            CombatMicroStageSnapshot stage,
            bool hasFreshObservationProgress,
            out string reason)
        {
            if (IsEndTurnDecision(_decision))
            {
                var barrier = context.CombatBarrierEvaluation;
                if (barrier.Kind == CombatBarrierKind.EndTurn && barrier.IsActive && barrier.ReleaseSatisfied)
                {
                    reason = "combat-end-turn-ack";
                    return true;
                }

                if (stage.Kind == CombatMicroStageKind.EnemyTurnClosed)
                {
                    reason = "combat-enemy-turn-closed";
                    return true;
                }

                reason = string.Empty;
                return false;
            }

            if (IsNonEnemySelectionDecision(_decision))
            {
                if (stage.Kind == CombatMicroStageKind.ResolvingNonEnemy
                    && context.HasSelectedNonEnemyConfirmEvidence
                    && hasFreshObservationProgress)
                {
                    reason = "combat-non-enemy-confirm-ready";
                    return true;
                }

                if (stage.Kind == CombatMicroStageKind.ResolvingAttackTarget
                    && context.CanResolveCombatEnemyTarget
                    && hasFreshObservationProgress)
                {
                    reason = "combat-non-enemy-selection-superseded-by-attack-target";
                    return true;
                }

                if (stage.Kind == CombatMicroStageKind.PlayerActionOpen
                    && context.RuntimeCombatState.ExplicitlyClearedSelection
                    && hasFreshObservationProgress)
                {
                    reason = "combat-non-enemy-selection-cleared";
                    return true;
                }

                if (stage.Kind is CombatMicroStageKind.TurnClosing or CombatMicroStageKind.EnemyTurnClosed)
                {
                    reason = $"combat-{stage.Kind.ToString().ToLowerInvariant()}";
                    return true;
                }

                reason = string.Empty;
                return false;
            }

            if (IsNonEnemyConfirmDecision(_decision))
            {
                if (stage.Kind == CombatMicroStageKind.PlayerActionOpen
                    && !context.RuntimeCombatState.HasCardSelectionEvidence
                    && !context.RuntimeCombatState.HasInFlightPlayerDrivenAction
                    && !context.HasSelectedNonEnemyConfirmEvidence
                    && hasFreshObservationProgress)
                {
                    reason = "combat-non-enemy-confirm-resolved";
                    return true;
                }

                if (stage.Kind is CombatMicroStageKind.TurnClosing or CombatMicroStageKind.EnemyTurnClosed)
                {
                    reason = $"combat-{stage.Kind.ToString().ToLowerInvariant()}";
                    return true;
                }

                reason = string.Empty;
                return false;
            }

            if (IsAttackSelectionDecision(_decision))
            {
                if (stage.Kind == CombatMicroStageKind.AwaitingCardPlayConfirm
                    && !CombatRuntimeStateSupport.RequiresExplicitTargetingBeforeEnemyClick(context.Observer.Summary, _combatCardKnowledge, context.PendingCombatSelection)
                    && hasFreshObservationProgress)
                {
                    reason = "combat-selected-attack-confirm-ready";
                    return true;
                }

                if (stage.Kind == CombatMicroStageKind.ResolvingCardPlay
                    && !CombatRuntimeStateSupport.RequiresExplicitTargetingBeforeEnemyClick(context.Observer.Summary, _combatCardKnowledge, context.PendingCombatSelection)
                    && hasFreshObservationProgress)
                {
                    reason = "combat-attack-autoplay-started";
                    return true;
                }

                if (stage.Kind == CombatMicroStageKind.ResolvingAttackTarget
                    && context.CanResolveCombatEnemyTarget
                    && hasFreshObservationProgress)
                {
                    reason = "combat-enemy-target-ready";
                    return true;
                }

                if (stage.Kind == CombatMicroStageKind.PlayerActionOpen
                    && !context.RuntimeCombatState.HasInFlightPlayerDrivenAction
                    && !context.RuntimeCombatState.HasCardSelectionEvidence
                    && hasFreshObservationProgress)
                {
                    reason = "combat-attack-selection-cleared";
                    return true;
                }

                if (stage.Kind is CombatMicroStageKind.TurnClosing or CombatMicroStageKind.EnemyTurnClosed)
                {
                    reason = $"combat-{stage.Kind.ToString().ToLowerInvariant()}";
                    return true;
                }

                reason = string.Empty;
                return false;
            }

            if (IsAttackConfirmDecision(_decision))
            {
                if (stage.Kind == CombatMicroStageKind.ResolvingCardPlay
                    && context.RuntimeCombatState.HasInFlightPlayerDrivenAction
                    && hasFreshObservationProgress)
                {
                    reason = "combat-selected-attack-enqueued";
                    return true;
                }

                if (stage.Kind == CombatMicroStageKind.PlayerActionOpen
                    && !context.RuntimeCombatState.HasInFlightPlayerDrivenAction
                    && !context.RuntimeCombatState.HasCardSelectionEvidence
                    && hasFreshObservationProgress)
                {
                    reason = "combat-selected-attack-cleared";
                    return true;
                }

                if (stage.Kind is CombatMicroStageKind.TurnClosing or CombatMicroStageKind.EnemyTurnClosed)
                {
                    reason = $"combat-{stage.Kind.ToString().ToLowerInvariant()}";
                    return true;
                }

                reason = string.Empty;
                return false;
            }

            if (IsEnemyTargetDecision(_decision))
            {
                if (stage.Kind == CombatMicroStageKind.PlayerActionOpen
                    && !context.RuntimeCombatState.HasInFlightPlayerDrivenAction
                    && !context.RuntimeCombatState.HasCardSelectionEvidence
                    && hasFreshObservationProgress)
                {
                    reason = "combat-enemy-click-resolved";
                    return true;
                }

                if (stage.Kind is CombatMicroStageKind.TurnClosing or CombatMicroStageKind.EnemyTurnClosed)
                {
                    reason = $"combat-{stage.Kind.ToString().ToLowerInvariant()}";
                    return true;
                }

                reason = string.Empty;
                return false;
            }

            if (IsCancelSelectionDecision(_decision))
            {
                if (stage.Kind == CombatMicroStageKind.PlayerActionOpen
                    && !context.RuntimeCombatState.HasInFlightPlayerDrivenAction
                    && !context.RuntimeCombatState.HasCardSelectionEvidence
                    && hasFreshObservationProgress)
                {
                    reason = "combat-selection-cleared";
                    return true;
                }

                if (stage.Kind is CombatMicroStageKind.TurnClosing or CombatMicroStageKind.EnemyTurnClosed)
                {
                    reason = $"combat-{stage.Kind.ToString().ToLowerInvariant()}";
                    return true;
                }
            }

            reason = string.Empty;
            return false;
        }

        private bool HasFreshObservationProgress(
            GuiSmokeStepAnalysisContext context,
            CombatMicroStageSnapshot stage)
        {
            if (context.Observer.Summary.SnapshotVersion != _baselineObserver.Summary.SnapshotVersion)
            {
                return true;
            }

            if (!string.Equals(stage.SettledFingerprint, _baselineStage.SettledFingerprint, StringComparison.Ordinal))
            {
                return true;
            }

            var runtime = context.RuntimeCombatState;
            var baselineRuntime = _baselineContext.RuntimeCombatState;
            return !string.Equals(runtime.InteractionRevision, baselineRuntime.InteractionRevision, StringComparison.OrdinalIgnoreCase)
                   || runtime.HistoryStartedCount != baselineRuntime.HistoryStartedCount
                   || runtime.HistoryFinishedCount != baselineRuntime.HistoryFinishedCount
                   || !string.Equals(runtime.LastCardPlayStartedCardId, baselineRuntime.LastCardPlayStartedCardId, StringComparison.OrdinalIgnoreCase)
                   || !string.Equals(runtime.LastCardPlayFinishedCardId, baselineRuntime.LastCardPlayFinishedCardId, StringComparison.OrdinalIgnoreCase)
                   || context.HasSelectedNonEnemyConfirmEvidence != _baselineContext.HasSelectedNonEnemyConfirmEvidence
                   || context.CanResolveCombatEnemyTarget != _baselineContext.CanResolveCombatEnemyTarget;
        }

        private bool IsAcceptableSettledStage(CombatMicroStageSnapshot stage)
        {
            if (IsEndTurnDecision(_decision))
            {
                return false;
            }

            if (IsNonEnemySelectionDecision(_decision))
            {
                return stage.Kind is CombatMicroStageKind.ResolvingNonEnemy
                    or CombatMicroStageKind.ResolvingAttackTarget
                    or CombatMicroStageKind.PlayerActionOpen
                    or CombatMicroStageKind.TurnClosing
                    or CombatMicroStageKind.EnemyTurnClosed;
            }

            if (IsNonEnemyConfirmDecision(_decision))
            {
                return stage.Kind is CombatMicroStageKind.PlayerActionOpen
                    or CombatMicroStageKind.TurnClosing
                    or CombatMicroStageKind.EnemyTurnClosed;
            }

            if (IsAttackSelectionDecision(_decision))
            {
                return stage.Kind is CombatMicroStageKind.ResolvingAttackTarget
                    or CombatMicroStageKind.AwaitingCardPlayConfirm
                    or CombatMicroStageKind.ResolvingCardPlay
                    or CombatMicroStageKind.PlayerActionOpen
                    or CombatMicroStageKind.TurnClosing
                    or CombatMicroStageKind.EnemyTurnClosed;
            }

            if (IsAttackConfirmDecision(_decision))
            {
                return stage.Kind is CombatMicroStageKind.ResolvingCardPlay
                    or CombatMicroStageKind.PlayerActionOpen
                    or CombatMicroStageKind.TurnClosing
                    or CombatMicroStageKind.EnemyTurnClosed;
            }

            if (IsEnemyTargetDecision(_decision))
            {
                return stage.Kind is CombatMicroStageKind.PlayerActionOpen
                    or CombatMicroStageKind.TurnClosing
                    or CombatMicroStageKind.EnemyTurnClosed;
            }

            if (IsCancelSelectionDecision(_decision))
            {
                return stage.Kind is CombatMicroStageKind.PlayerActionOpen
                    or CombatMicroStageKind.TurnClosing
                    or CombatMicroStageKind.EnemyTurnClosed;
            }

            return false;
        }

        private void ResetStableFingerprint()
        {
            _stableFingerprint = null;
            _stablePollCount = 0;
        }
    }

    private static bool IsNonEnemySelectionDecision(GuiSmokeStepDecision decision)
    {
        var targetLabel = decision.TargetLabel;
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return false;
        }

        return targetLabel.StartsWith("combat select non-enemy slot ", StringComparison.OrdinalIgnoreCase)
               || targetLabel.StartsWith("combat select defend slot ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyConfirmDecision(GuiSmokeStepDecision decision)
    {
        return string.Equals(decision.TargetLabel, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decision.ActionKind, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAttackSelectionDecision(GuiSmokeStepDecision decision)
    {
        return decision.TargetLabel?.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool IsAttackConfirmDecision(GuiSmokeStepDecision decision)
    {
        return string.Equals(decision.TargetLabel, "confirm selected attack card", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decision.ActionKind, "confirm-attack-card", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEnemyTargetDecision(GuiSmokeStepDecision decision)
    {
        return CombatHistorySupport.IsCombatEnemyTargetLabel(decision.TargetLabel);
    }

    private static bool IsEndTurnDecision(GuiSmokeStepDecision decision)
    {
        return CombatHistorySupport.IsCombatEndTurnLabel(decision.TargetLabel)
               || (string.Equals(decision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(decision.KeyText, "E", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsCancelSelectionDecision(GuiSmokeStepDecision decision)
    {
        return CombatHistorySupport.IsCombatCancelSelectionLabel(decision.TargetLabel)
               || string.Equals(decision.ActionKind, "right-click", StringComparison.OrdinalIgnoreCase);
    }
}
