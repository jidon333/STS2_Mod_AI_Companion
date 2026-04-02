using System.Collections.Generic;
using System.Diagnostics;
using static GuiSmokeNonCombatAllowedActionSupport;
using static ObserverScreenProvenance;

internal static partial class Program
{
    sealed class GuiSmokeAttemptLoopTrackingState
    {
        public string? LastActionFingerprint { get; set; }

        public int SameActionStallCount { get; set; }

        public string? LastDecisionWaitFingerprint { get; set; }

        public int ConsecutiveDecisionWaitCount { get; set; }

        public string? LastOverlayLoopFingerprint { get; set; }

        public int ConsecutiveOverlayLoopCount { get; set; }

        public void ResetDecisionWaitTracking()
        {
            LastDecisionWaitFingerprint = null;
            ConsecutiveDecisionWaitCount = 0;
        }

        public void ResetOverlayLoopTracking()
        {
            LastOverlayLoopFingerprint = null;
            ConsecutiveOverlayLoopCount = 0;
        }
    }

    sealed record GuiSmokeDecisionStatusResult(
        GuiSmokeAttemptResult? CompletedAttempt,
        bool ShouldContinueLoop);

    sealed record GuiSmokeActuationPreparationResult(
        GuiSmokeAttemptResult? CompletedAttempt,
        bool ShouldContinueLoop,
        ObserverState LatestObserver,
        WindowCaptureTarget? ClickWindow,
        bool RewardMapRecoveryAttempt);

    static async Task<GuiSmokeDecisionStatusResult> TryHandleDecisionStatusAsync(
        int stepIndex,
        string screenshotPath,
        GuiSmokePhase phase,
        ObserverState observer,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        GuiSmokeStepAnalysisContext stepAnalysisContext,
        bool isLongRun,
        ArtifactRecorder logger,
        ObserverSnapshotReader observerReader,
        ObserverAcceptanceEvaluator evaluator,
        List<GuiSmokeHistoryEntry> history,
        Stopwatch stepStopwatch,
        long decisionReadyElapsedMs,
        GuiSmokeAttemptLoopTrackingState loopTracking,
        Func<int, string, string, string?, string?, GuiSmokeAttemptResult> completeAttempt)
    {
        GuiSmokeAttemptResult CompleteFailure(string message, string terminalCause)
        {
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                message,
                observer.CurrentScreen,
                observer.InCombat,
                screenshotPath));
            return completeAttempt(
                1,
                "failed",
                message,
                terminalCause,
                ClassifyFailureForAttempt(phase, observer, terminalCause, launchFailed: false));
        }

        if (string.Equals(decision.Status, "abort", StringComparison.OrdinalIgnoreCase))
        {
            var abortTerminalCause = IsExplicitDecisionAbortRisk(decision.DecisionRisk)
                ? decision.DecisionRisk!
                : "decision-abort";
            var abortMessage = decision.AbortReason
                               ?? decision.DecisionRisk
                               ?? "decision aborted";
            loopTracking.ResetDecisionWaitTracking();
            LogHarness($"step={stepIndex} aborted reason={abortMessage}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    null,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    loopTracking.SameActionStallCount,
                    abortTerminalCause));
            return new GuiSmokeDecisionStatusResult(
                CompleteFailure(abortMessage, abortTerminalCause),
                ShouldContinueLoop: false);
        }

        if (!string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase))
        {
            return new GuiSmokeDecisionStatusResult(null, ShouldContinueLoop: false);
        }

        var waitFingerprint = BuildDecisionWaitFingerprint(phase, request.SceneSignature, observer, stepAnalysisContext);
        if (string.Equals(loopTracking.LastDecisionWaitFingerprint, waitFingerprint, StringComparison.Ordinal))
        {
            loopTracking.ConsecutiveDecisionWaitCount += 1;
        }
        else
        {
            loopTracking.LastDecisionWaitFingerprint = waitFingerprint;
            loopTracking.ConsecutiveDecisionWaitCount = 1;
        }

        if (phase == GuiSmokePhase.HandleCombat
            && CombatBarrierSupport.TryClassifyWaitPlateau(request, stepAnalysisContext, loopTracking.ConsecutiveDecisionWaitCount, out var barrierPlateauCause, out var barrierPlateauMessage))
        {
            LogHarness($"step={stepIndex} abort {barrierPlateauMessage}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    null,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    loopTracking.SameActionStallCount,
                    "decision-wait",
                    barrierPlateauCause));
            return new GuiSmokeDecisionStatusResult(
                CompleteFailure(barrierPlateauMessage, barrierPlateauCause),
                ShouldContinueLoop: false);
        }

        if (TryClassifyRewardAftermathCardProgressionWait(
                phase,
                request,
                decision,
                stepAnalysisContext,
                loopTracking.ConsecutiveDecisionWaitCount,
                out var rewardAftermathWaitCause,
                out var rewardAftermathWaitMessage))
        {
            LogHarness($"step={stepIndex} abort {rewardAftermathWaitMessage}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    null,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    loopTracking.SameActionStallCount,
                    "reward-aftermath-card-progression-stall"));
            return new GuiSmokeDecisionStatusResult(
                CompleteFailure(rewardAftermathWaitMessage, rewardAftermathWaitCause),
                ShouldContinueLoop: false);
        }

        if (TryClassifyRestSiteReleaseMapHandoffWait(
                phase,
                request,
                decision,
                stepAnalysisContext,
                loopTracking.ConsecutiveDecisionWaitCount,
                out var restSiteReleaseWaitCause,
                out var restSiteReleaseWaitMessage))
        {
            LogHarness($"step={stepIndex} abort {restSiteReleaseWaitMessage}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    null,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    loopTracking.SameActionStallCount,
                    "rest-site-release-map-handoff-stall"));
            return new GuiSmokeDecisionStatusResult(
                CompleteFailure(restSiteReleaseWaitMessage, restSiteReleaseWaitCause),
                ShouldContinueLoop: false);
        }

        if (TryClassifyDecisionWaitPlateau(phase, observer, loopTracking.ConsecutiveDecisionWaitCount, out var plateauTerminalCause, out var plateauMessage))
        {
            LogHarness($"step={stepIndex} abort {plateauMessage}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    null,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    loopTracking.SameActionStallCount,
                    "decision-wait",
                    "wait-plateau"));
            return new GuiSmokeDecisionStatusResult(
                CompleteFailure(plateauMessage, plateauTerminalCause),
                ShouldContinueLoop: false);
        }

        if (TryClassifyRestSitePostClickNoOp(phase, request, decision, out var restSitePostClickNoOpMessage))
        {
            var restSiteTerminalCause = IsRestSitePostClickDecisionRisk(decision.DecisionRisk)
                ? decision.DecisionRisk!
                : "rest-site-post-click-noop";
            LogHarness($"step={stepIndex} abort {restSitePostClickNoOpMessage}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    null,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    loopTracking.SameActionStallCount,
                    "rest-site-post-click-noop"));
            return new GuiSmokeDecisionStatusResult(
                CompleteFailure(restSitePostClickNoOpMessage, restSiteTerminalCause),
                ShouldContinueLoop: false);
        }

        var waitMinimumMs = GetDecisionWaitMinimumMs(phase);
        var waitBudgetMs = Math.Max(waitMinimumMs, decision.WaitMs ?? waitMinimumMs);
        LogHarness($"step={stepIndex} wait requested ms={waitBudgetMs}");
        history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "wait", decision.TargetLabel, DateTimeOffset.UtcNow)
        {
            Metadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(request, decision),
        });
        logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "wait", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
        AppendProgressIfLongRun(
            isLongRun,
            logger,
            EvaluateStepProgress(
                stepIndex,
                phase,
                request.SceneSignature,
                observer,
                null,
                decision,
                request.FirstSeenScene,
                request.ReasoningMode,
                false,
                loopTracking.SameActionStallCount,
                "decision-wait"));
        var decisionWait = await WaitWithObserverPollingAsync(
                observerReader,
                waitBudgetMs,
                75,
                observer,
                latestObserver => HasGenericObserverWakeDelta(observer, latestObserver)
                    ? "observer-delta"
                    : evaluator.IsPhaseSatisfied(phase, latestObserver, history)
                        ? "phase-satisfied"
                        : null)
            .ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(decisionWait.WakeReason))
        {
            LogHarness($"step={stepIndex} decision wait woke early reason={decisionWait.WakeReason}");
        }
        LogHarness($"step={stepIndex} timing decision->after={Math.Max(0, stepStopwatch.ElapsedMilliseconds - decisionReadyElapsedMs)}ms total={stepStopwatch.ElapsedMilliseconds}ms");
        return new GuiSmokeDecisionStatusResult(null, ShouldContinueLoop: true);
    }

    static async Task<GuiSmokeActuationPreparationResult> TryPrepareDecisionActuationAsync(
        int stepIndex,
        string screenshotPath,
        GuiSmokePhase phase,
        WindowCaptureTarget window,
        ObserverState observer,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        GuiSmokeStepAnalysisContext stepAnalysisContext,
        bool isLongRun,
        ArtifactRecorder logger,
        ObserverSnapshotReader observerReader,
        ObserverAcceptanceEvaluator evaluator,
        List<GuiSmokeHistoryEntry> history,
        Stopwatch stepStopwatch,
        GuiSmokeAttemptLoopTrackingState loopTracking,
        Func<int, string, string, string?, string?, GuiSmokeAttemptResult> completeAttempt,
        int transitionSettleMs,
        TimeSpan decisionStaleBudget,
        bool useDecisionAgeGuard)
    {
        GuiSmokeAttemptResult CompleteFailure(string message, string terminalCause, ObserverState? failureObserver = null)
        {
            var resolvedObserver = failureObserver ?? observer;
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                message,
                resolvedObserver.CurrentScreen,
                resolvedObserver.InCombat,
                screenshotPath));
            return completeAttempt(
                1,
                "failed",
                message,
                terminalCause,
                ClassifyFailureForAttempt(phase, resolvedObserver, terminalCause, launchFailed: false));
        }

        if (LooksLikeInspectOverlayState(observer) && IsOverlayCleanupTarget(decision.TargetLabel))
        {
            var overlayFingerprint = BuildOverlayLoopFingerprint(request.SceneSignature, observer);
            if (string.Equals(loopTracking.LastOverlayLoopFingerprint, overlayFingerprint, StringComparison.Ordinal))
            {
                loopTracking.ConsecutiveOverlayLoopCount += 1;
            }
            else
            {
                loopTracking.LastOverlayLoopFingerprint = overlayFingerprint;
                loopTracking.ConsecutiveOverlayLoopCount = 1;
            }

            if (loopTracking.ConsecutiveOverlayLoopCount >= 4)
            {
                var overlayLoopMessage = $"inspect-overlay-loop phase={phase} screen={observer.CurrentScreen ?? "null"} overlayAttempts={loopTracking.ConsecutiveOverlayLoopCount}";
                LogHarness($"step={stepIndex} abort {overlayLoopMessage}");
                AppendProgressIfLongRun(
                    isLongRun,
                    logger,
                    EvaluateStepProgress(
                        stepIndex,
                        phase,
                        request.SceneSignature,
                        observer,
                        null,
                        decision,
                        request.FirstSeenScene,
                        request.ReasoningMode,
                        false,
                        loopTracking.SameActionStallCount,
                        "inspect-overlay-loop"));
                return new GuiSmokeActuationPreparationResult(
                    CompleteFailure(overlayLoopMessage, "inspect-overlay-loop"),
                    ShouldContinueLoop: false,
                    LatestObserver: observer,
                    ClickWindow: null,
                    RewardMapRecoveryAttempt: false);
            }
        }
        else if (!LooksLikeInspectOverlayState(observer) || !string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase))
        {
            loopTracking.ResetOverlayLoopTracking();
        }
        else if (!IsOverlayCleanupTarget(decision.TargetLabel))
        {
            loopTracking.ResetOverlayLoopTracking();
        }

        loopTracking.ResetDecisionWaitTracking();

        var decisionAge = DateTimeOffset.UtcNow - request.IssuedAt;
        if (useDecisionAgeGuard && decisionAge > decisionStaleBudget)
        {
            LogHarness($"step={stepIndex} recapture required stale-decision ageMs={(int)decisionAge.TotalMilliseconds}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "stale-decision", decision.TargetLabel, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "stale-decision", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    null,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    loopTracking.SameActionStallCount,
                    "decision-stale"));
            var staleDecisionWait = await WaitWithObserverPollingAsync(
                    observerReader,
                    GetDecisionWaitMinimumMs(phase),
                    75,
                    observer,
                    latestObserver => HasGenericObserverWakeDelta(observer, latestObserver)
                        ? "observer-delta"
                        : evaluator.IsPhaseSatisfied(phase, latestObserver, history)
                            ? "phase-satisfied"
                            : null)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(staleDecisionWait.WakeReason))
            {
                LogHarness($"step={stepIndex} stale decision backoff woke early reason={staleDecisionWait.WakeReason}");
            }

            return new GuiSmokeActuationPreparationResult(
                null,
                ShouldContinueLoop: true,
                LatestObserver: staleDecisionWait.Observer,
                ClickWindow: null,
                RewardMapRecoveryAttempt: false);
        }

        var latestObserver = observerReader.Read(includeEventTail: false);
        if (ShouldRecaptureForObserverDrift(request.Observer, latestObserver, decision))
        {
            LogHarness($"step={stepIndex} recapture required observer-drift requestScreen={request.Observer.CurrentScreen ?? "null"} latestScreen={latestObserver.CurrentScreen ?? "null"} requestVisible={request.Observer.VisibleScreen ?? "null"} latestVisible={latestObserver.VisibleScreen ?? "null"}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "observer-drift", decision.TargetLabel, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "observer-drift", latestObserver.CurrentScreen, latestObserver.InCombat, decision.TargetLabel));
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    latestObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    loopTracking.SameActionStallCount,
                    "observer-drift"));
            var observerDriftWait = await WaitWithObserverPollingAsync(
                    observerReader,
                    GetDecisionWaitMinimumMs(phase),
                    75,
                    latestObserver,
                    currentObserver => HasGenericObserverWakeDelta(latestObserver, currentObserver)
                        ? "observer-delta"
                        : evaluator.IsPhaseSatisfied(phase, currentObserver, history)
                            ? "phase-satisfied"
                            : null)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(observerDriftWait.WakeReason))
            {
                LogHarness($"step={stepIndex} observer drift backoff woke early reason={observerDriftWait.WakeReason}");
            }

            return new GuiSmokeActuationPreparationResult(
                null,
                ShouldContinueLoop: true,
                LatestObserver: observerDriftWait.Observer,
                ClickWindow: null,
                RewardMapRecoveryAttempt: false);
        }

        ValidateDecision(phase, request, decision);

        var rewardMapRecoveryAttempt = false;
        if (TryClassifyRewardMapLoop(phase, request, decision, out var rewardMapLoopMessage))
        {
            if (ShouldAllowRewardMapRecovery(request, decision))
            {
                rewardMapRecoveryAttempt = true;
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "reward-map-recovery", decision.TargetLabel, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "reward-map-recovery", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
                LogHarness($"step={stepIndex} recovery reward-map-loop delayed target={decision.TargetLabel ?? "null"}");
            }
            else
            {
                LogHarness($"step={stepIndex} abort {rewardMapLoopMessage}");
                AppendProgressIfLongRun(
                    isLongRun,
                    logger,
                    EvaluateStepProgress(
                        stepIndex,
                        phase,
                        request.SceneSignature,
                        observer,
                        latestObserver,
                        decision,
                        request.FirstSeenScene,
                        request.ReasoningMode,
                        false,
                        loopTracking.SameActionStallCount,
                        "reward-map-loop"));
                return new GuiSmokeActuationPreparationResult(
                    CompleteFailure(rewardMapLoopMessage, "reward-map-loop"),
                    ShouldContinueLoop: false,
                    LatestObserver: latestObserver,
                    ClickWindow: null,
                    RewardMapRecoveryAttempt: false);
            }
        }

        if (TryClassifyMapOverlayNoOpLoop(phase, request, decision, out var mapOverlayNoOpLoopMessage))
        {
            LogHarness($"step={stepIndex} abort {mapOverlayNoOpLoopMessage}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    latestObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    loopTracking.SameActionStallCount,
                    "map-overlay-noop-loop"));
            return new GuiSmokeActuationPreparationResult(
                CompleteFailure(mapOverlayNoOpLoopMessage, "map-overlay-noop-loop"),
                ShouldContinueLoop: false,
                LatestObserver: latestObserver,
                ClickWindow: null,
                RewardMapRecoveryAttempt: false);
        }

        if (TryClassifyMapTransitionStall(phase, request, decision, out var mapTransitionStallMessage))
        {
            LogHarness($"step={stepIndex} abort {mapTransitionStallMessage}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    latestObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    loopTracking.SameActionStallCount,
                    "map-transition-stall"));
            return new GuiSmokeActuationPreparationResult(
                CompleteFailure(mapTransitionStallMessage, "map-transition-stall"),
                ShouldContinueLoop: false,
                LatestObserver: latestObserver,
                ClickWindow: null,
                RewardMapRecoveryAttempt: false);
        }

        if (TryClassifyCombatNoOpLoop(phase, request, decision, out var combatNoOpLoopMessage))
        {
            LogHarness($"step={stepIndex} abort {combatNoOpLoopMessage}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    latestObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    loopTracking.SameActionStallCount,
                    "combat-noop-loop"));
            return new GuiSmokeActuationPreparationResult(
                CompleteFailure(combatNoOpLoopMessage, "combat-noop-loop"),
                ShouldContinueLoop: false,
                LatestObserver: latestObserver,
                ClickWindow: null,
                RewardMapRecoveryAttempt: false);
        }

        var actionFingerprint = string.Join("|",
            phase.ToString(),
            request.SceneSignature,
            decision.ActionKind ?? "null",
            decision.TargetLabel ?? "null");
        if (string.Equals(loopTracking.LastActionFingerprint, actionFingerprint, StringComparison.Ordinal))
        {
            loopTracking.SameActionStallCount += 1;
        }
        else
        {
            loopTracking.LastActionFingerprint = actionFingerprint;
            loopTracking.SameActionStallCount = 0;
        }

        if (loopTracking.SameActionStallCount >= GetSameActionStallLimit(phase, decision))
        {
            if (TryClassifyRewardAftermathCardProgressionActionStall(
                    phase,
                    request,
                    decision,
                    stepAnalysisContext,
                    loopTracking.SameActionStallCount,
                    out var rewardAftermathActionCause,
                    out var rewardAftermathActionMessage))
            {
                LogHarness($"step={stepIndex} abort {rewardAftermathActionMessage}");
                AppendProgressIfLongRun(
                    isLongRun,
                    logger,
                    EvaluateStepProgress(
                        stepIndex,
                        phase,
                        request.SceneSignature,
                        observer,
                        latestObserver,
                        decision,
                        request.FirstSeenScene,
                        request.ReasoningMode,
                        false,
                        loopTracking.SameActionStallCount,
                        "reward-aftermath-card-progression-stall"));
                return new GuiSmokeActuationPreparationResult(
                    CompleteFailure(rewardAftermathActionMessage, rewardAftermathActionCause),
                    ShouldContinueLoop: false,
                    LatestObserver: latestObserver,
                    ClickWindow: null,
                    RewardMapRecoveryAttempt: false);
            }

            if (TryClassifyRestSiteReleaseMapHandoffActionStall(
                    phase,
                    request,
                    decision,
                    stepAnalysisContext,
                    loopTracking.SameActionStallCount,
                    out var restSiteReleaseActionCause,
                    out var restSiteReleaseActionMessage))
            {
                LogHarness($"step={stepIndex} abort {restSiteReleaseActionMessage}");
                AppendProgressIfLongRun(
                    isLongRun,
                    logger,
                    EvaluateStepProgress(
                        stepIndex,
                        phase,
                        request.SceneSignature,
                        observer,
                        latestObserver,
                        decision,
                        request.FirstSeenScene,
                        request.ReasoningMode,
                        false,
                        loopTracking.SameActionStallCount,
                        "rest-site-release-map-handoff-stall"));
                return new GuiSmokeActuationPreparationResult(
                    CompleteFailure(restSiteReleaseActionMessage, restSiteReleaseActionCause),
                    ShouldContinueLoop: false,
                    LatestObserver: latestObserver,
                    ClickWindow: null,
                    RewardMapRecoveryAttempt: false);
            }

            var abortReason = $"same-action-stall phase={phase} target={decision.TargetLabel ?? "null"} screen={observer.CurrentScreen ?? "null"} inventory={observer.InventoryId ?? "null"}";
            LogHarness($"step={stepIndex} abort {abortReason}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    latestObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    loopTracking.SameActionStallCount,
                    "same-action-stall"));
            return new GuiSmokeActuationPreparationResult(
                CompleteFailure(abortReason, "same-action-stall"),
                ShouldContinueLoop: false,
                LatestObserver: latestObserver,
                ClickWindow: null,
                RewardMapRecoveryAttempt: false);
        }

        var clickWindow = WindowLocator.Refresh(window);
        if (clickWindow.IsMinimized)
        {
            LogHarness($"step={stepIndex} click blocked: window minimized before action; restoring title={clickWindow.Title}");
            clickWindow = WindowLocator.EnsureRestored(clickWindow);
            LogHarness($"step={stepIndex} click window restored={DescribeWindow(clickWindow)}");
        }

        if (WindowLocator.HasMeaningfulDrift(window, clickWindow))
        {
            LogHarness($"step={stepIndex} recapture required captureBounds={DescribeBounds(window.Bounds)} clickBounds={DescribeBounds(clickWindow.Bounds)}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "recapture-required", decision.TargetLabel, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "recapture-required", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    latestObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    loopTracking.SameActionStallCount,
                    "window-drift"));
            var windowDriftWait = await WaitWithObserverPollingAsync(
                    observerReader,
                    transitionSettleMs,
                    75,
                    latestObserver,
                    currentObserver => HasGenericObserverWakeDelta(latestObserver, currentObserver)
                        ? "observer-delta"
                        : evaluator.IsPhaseSatisfied(phase, currentObserver, history)
                            ? "phase-satisfied"
                            : null)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(windowDriftWait.WakeReason))
            {
                LogHarness($"step={stepIndex} window drift backoff woke early reason={windowDriftWait.WakeReason}");
            }

            return new GuiSmokeActuationPreparationResult(
                null,
                ShouldContinueLoop: true,
                LatestObserver: windowDriftWait.Observer,
                ClickWindow: null,
                RewardMapRecoveryAttempt: false);
        }

        return new GuiSmokeActuationPreparationResult(
            null,
            ShouldContinueLoop: false,
            LatestObserver: latestObserver,
            ClickWindow: clickWindow,
            RewardMapRecoveryAttempt: rewardMapRecoveryAttempt);
    }

    static bool TryClassifyRewardAftermathCardProgressionWait(
        GuiSmokePhase phase,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        GuiSmokeStepAnalysisContext analysisContext,
        int consecutiveDecisionWaitCount,
        out string terminalCause,
        out string message)
    {
        terminalCause = string.Empty;
        message = string.Empty;
        if (phase != GuiSmokePhase.HandleRewards
            || consecutiveDecisionWaitCount < 4
            || !string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var rewardScene = analysisContext.RewardScene;
        if (!rewardScene.RewardForegroundOwned
            || rewardScene.ReleaseStage != RewardReleaseStage.Active
            || rewardScene.ExplicitAction is not RewardExplicitActionKind.CardChoice and not RewardExplicitActionKind.ColorlessChoice)
        {
            return false;
        }

        if (!(decision.Reason?.Contains("reward card progression", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            return false;
        }

        terminalCause = "reward-aftermath-card-progression-stall";
        message = $"reward-aftermath-card-progression-stall phase=HandleRewards target={decision.TargetLabel ?? "wait"} screen={request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "null"} waits={consecutiveDecisionWaitCount} extractor={request.Observer.ChoiceExtractorPath ?? "null"} aftermath={rewardScene.AftermathResiduePresent}";
        return true;
    }

    static bool TryClassifyRewardAftermathCardProgressionActionStall(
        GuiSmokePhase phase,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        GuiSmokeStepAnalysisContext analysisContext,
        int sameActionStallCount,
        out string terminalCause,
        out string message)
    {
        terminalCause = string.Empty;
        message = string.Empty;
        if (phase != GuiSmokePhase.HandleRewards
            || sameActionStallCount < GetSameActionStallLimit(phase, decision)
            || (!string.Equals(decision.TargetLabel, "reward card choice", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(decision.TargetLabel, "colorless card choice", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        var rewardScene = analysisContext.RewardScene;
        if (!rewardScene.RewardForegroundOwned
            || rewardScene.ReleaseStage != RewardReleaseStage.Active
            || rewardScene.ExplicitAction is not RewardExplicitActionKind.CardChoice and not RewardExplicitActionKind.ColorlessChoice)
        {
            return false;
        }

        terminalCause = "reward-aftermath-card-progression-stall";
        message = $"reward-aftermath-card-progression-stall phase=HandleRewards target={decision.TargetLabel ?? "null"} screen={request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "null"} repeats={sameActionStallCount + 1} extractor={request.Observer.ChoiceExtractorPath ?? "null"} aftermath={rewardScene.AftermathResiduePresent}";
        return true;
    }

    static bool TryClassifyRestSiteReleaseMapHandoffWait(
        GuiSmokePhase phase,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        GuiSmokeStepAnalysisContext analysisContext,
        int consecutiveDecisionWaitCount,
        out string terminalCause,
        out string message)
    {
        terminalCause = string.Empty;
        message = string.Empty;
        if (phase is not (GuiSmokePhase.ChooseFirstNode or GuiSmokePhase.WaitMap or GuiSmokePhase.WaitPostMapNodeRoom)
            || consecutiveDecisionWaitCount < 4
            || !string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase)
            || !(decision.Reason?.Contains("rest-site release", StringComparison.OrdinalIgnoreCase) ?? false)
            || !IsRestSiteReleasePendingHandoff(request.Observer, analysisContext))
        {
            return false;
        }

        var restSiteScene = analysisContext.CanonicalNonCombatScene as RestSiteSceneState;
        terminalCause = "rest-site-release-map-handoff-stall";
        message = $"rest-site-release-map-handoff-stall phase={phase} screen={request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "null"} waits={consecutiveDecisionWaitCount} extractor={request.Observer.ChoiceExtractorPath ?? "null"} selectionAccepted={restSiteScene?.SelectionAcceptedRecently ?? false} proceedVisible={restSiteScene?.ProceedVisible ?? false} mapCurrentActive={NonCombatForegroundOwnership.HasAuthoritativeMapForegroundScreen(request.Observer)} exportedReachable={analysisContext.MapOverlayState.ExportedReachableNodeCandidatePresent}";
        return true;
    }

    static bool TryClassifyRestSiteReleaseMapHandoffActionStall(
        GuiSmokePhase phase,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        GuiSmokeStepAnalysisContext analysisContext,
        int sameActionStallCount,
        out string terminalCause,
        out string message)
    {
        terminalCause = string.Empty;
        message = string.Empty;
        if (phase is not (GuiSmokePhase.ChooseFirstNode or GuiSmokePhase.WaitMap or GuiSmokePhase.WaitPostMapNodeRoom)
            || sameActionStallCount < GetSameActionStallLimit(phase, decision)
            || !string.Equals(decision.TargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase)
            || !IsRestSiteReleasePendingHandoff(request.Observer, analysisContext))
        {
            return false;
        }

        var restSiteScene = analysisContext.CanonicalNonCombatScene as RestSiteSceneState;
        terminalCause = "rest-site-release-map-handoff-stall";
        message = $"rest-site-release-map-handoff-stall phase={phase} target={decision.TargetLabel ?? "null"} screen={request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "null"} repeats={sameActionStallCount + 1} extractor={request.Observer.ChoiceExtractorPath ?? "null"} selectionAccepted={restSiteScene?.SelectionAcceptedRecently ?? false} proceedVisible={restSiteScene?.ProceedVisible ?? false} mapCurrentActive={NonCombatForegroundOwnership.HasAuthoritativeMapForegroundScreen(request.Observer)} exportedReachable={analysisContext.MapOverlayState.ExportedReachableNodeCandidatePresent}";
        return true;
    }

    static bool IsRestSiteReleasePendingHandoff(ObserverSummary observer, GuiSmokeStepAnalysisContext analysisContext)
    {
        var handoffState = analysisContext.PostNodeHandoffState;
        return string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
               && string.Equals(observer.CurrentScreen ?? observer.VisibleScreen, "rest-site", StringComparison.OrdinalIgnoreCase)
               && handoffState.Owner == NonCombatCanonicalForegroundOwner.RestSite
               && handoffState.ReleaseStage == NonCombatReleaseStage.ReleasePending
               && handoffState.SurfaceKind == PostNodeHandoffSurfaceKind.RestSiteReleasePending
               && !handoffState.HasExplicitSurface
               && analysisContext.CanonicalNonCombatScene is RestSiteSceneState { SelectionAcceptedRecently: true };
    }
}
