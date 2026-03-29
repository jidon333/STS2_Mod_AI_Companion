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
using static GuiSmokeNonCombatAllowedActionSupport;
using static GuiSmokePostActionPhaseSupport;
using static GuiSmokeSceneReasoningSupport;
using static GuiSmokeStepRequestFactory;
using static GuiSmokeStepScreenshotPolicy;
using static ObserverScreenProvenance;

internal static partial class Program
{
    static async Task<GuiSmokeAttemptResult> RunAttemptLoopAsync(
        string workspaceRoot,
        IReadOnlyDictionary<string, string> options,
        string scenarioId,
        string providerKind,
        LiveExportLayout liveLayout,
        HarnessQueueLayout harnessLayout,
        string sessionRoot,
        bool isLongRun,
        string attemptId,
        int attemptOrdinal,
        DateTimeOffset sessionDeadline,
        string trustStateAtStart,
        int? maxSteps,
        string runId,
        string stepsRoot,
        GuiSmokeSessionSceneHistoryIndex sceneHistoryIndex,
        ArtifactRecorder logger,
        bool isAuthoritativeFirstAttempt,
        Action<string, string, string?>? recordAttemptStartupStage,
        Action<string>? recordAttemptStartupFailure,
        Func<int, int, string, string, bool, string?, string?, GuiSmokeAttemptResult> completeAttempt)
    {
        const int PassiveWaitMs = 450;
        const int ActionSettleMinimumMs = 180;
        const int CombatActionSettleMinimumMs = 90;
        const int CombatNoOpProbeGraceMs = 80;
        const int TransitionSettleMs = 1000;
        const int ManualCleanBootObserverBootstrapPollMs = 500;
        const int ManualCleanBootObserverBootstrapPollCount = 12;

        var stepIndex = 0;

        void RecordAttemptStartupFailure(string reason)
        {
            recordAttemptStartupFailure?.Invoke(reason);
        }

        void RecordAttemptStartupStage(string stage, string status, string? detail = null)
        {
            recordAttemptStartupStage?.Invoke(stage, status, detail);
        }

        GuiSmokeAttemptResult CompleteAttempt(
            int exitCode,
            string status,
            string message,
            bool launchFailed = false,
            string? terminalCause = null,
            string? failureClass = null)
        {
            return completeAttempt(stepIndex, exitCode, status, message, launchFailed, terminalCause, failureClass);
        }

        IGuiDecisionProvider provider = string.Equals(providerKind, "headless", StringComparison.OrdinalIgnoreCase)
            ? new HeadlessCodexDecisionProvider(options)
            : string.Equals(providerKind, "auto", StringComparison.OrdinalIgnoreCase)
                ? new AutoDecisionProvider()
                : new SessionDecisionProvider();

        var observerReader = new ObserverSnapshotReader(liveLayout, harnessLayout);
        var captureService = new ScreenCaptureService();
        var inputDriver = new MouseInputDriver();
        var evaluator = new ObserverAcceptanceEvaluator();

        var phase = GuiSmokePhase.WaitMainMenu;
        var attemptsByPhase = new Dictionary<GuiSmokePhase, int>();
        var history = new List<GuiSmokeHistoryEntry>();
        var waitDeadline = isLongRun
            ? sessionDeadline
            : DateTimeOffset.UtcNow.AddMinutes(10);
        var freshnessFloor = DateTimeOffset.UtcNow.AddSeconds(-5);
        var consecutiveBlackFrames = 0;
        string? lastActionFingerprint = null;
        var sameActionStallCount = 0;
        var attemptStartedRecorded = false;
        string? lastDecisionWaitFingerprint = null;
        var consecutiveDecisionWaitCount = 0;
        string? lastOverlayLoopFingerprint = null;
        var consecutiveOverlayLoopCount = 0;
        var consecutiveFallbackCapturesWithoutProcess = 0;
        var useDecisionAgeGuard = !string.Equals(providerKind, "auto", StringComparison.OrdinalIgnoreCase);
        var decisionStaleBudget = useDecisionAgeGuard
            ? TimeSpan.FromSeconds(2)
            : TimeSpan.FromSeconds(30);

        void ResetDecisionWaitTracking()
        {
            lastDecisionWaitFingerprint = null;
            consecutiveDecisionWaitCount = 0;
        }

        void ResetOverlayLoopTracking()
        {
            lastOverlayLoopFingerprint = null;
            consecutiveOverlayLoopCount = 0;
        }

        while (DateTimeOffset.UtcNow < waitDeadline)
        {
            if (maxSteps is int stepLimit && stepIndex >= stepLimit)
            {
                return CompleteAttempt(
                    0,
                    "completed",
                    $"max-steps-reached:{stepLimit}",
                    terminalCause: "max-steps-reached");
            }

            stepIndex += 1;
            var window = WindowLocator.TryFindSts2Window()
                         ?? WindowLocator.GetPrimaryMonitorFallback();
            if (window.IsMinimized)
            {
                LogHarness($"step={stepIndex} window minimized; restoring title={window.Title}");
                window = WindowLocator.EnsureRestored(window);
                LogHarness($"step={stepIndex} window restored={DescribeWindow(window)}");
            }
            if (!window.IsFallback)
            {
                window = WindowLocator.EnsureInteractive(window);
            }
            LogHarness($"step={stepIndex} phase={phase} window={DescribeWindow(window)}");
            var stepPrefix = Path.Combine(stepsRoot, stepIndex.ToString("0000"));
            var screenshotPath = stepPrefix + ".screen.png";
            var stepStopwatch = Stopwatch.StartNew();

            var observer = observerReader.Read();
            if (isLongRun
                && history.Count == 0
                && phase == GuiSmokePhase.WaitMainMenu
                && !IsManualCleanBootObserverReady(observer, freshnessFloor))
            {
                observer = await BootstrapManualCleanBootObserverAsync(
                        observer,
                        () => observerReader.Read(includeEventTail: false),
                        freshnessFloor,
                        ManualCleanBootObserverBootstrapPollCount,
                        ManualCleanBootObserverBootstrapPollMs)
                    .ConfigureAwait(false);
            }

            var (_, _, stepAnalysisContext, iterationSceneContext) = BuildIterationContext(
                workspaceRoot,
                phase,
                history,
                observer,
                window,
                null,
                isLongRun,
                sceneHistoryIndex);
            var iterationSceneSignature = iterationSceneContext.SceneSignature;
            var iterationFirstSeenScene = iterationSceneContext.FirstSeenScene;
            var iterationReasoningMode = iterationSceneContext.ReasoningMode;
            var preflight = await TryHandleStepObserverPreflightAsync(
                    stepIndex,
                    stepPrefix,
                    screenshotPath,
                    phase,
                    observer,
                    iterationSceneSignature,
                    iterationFirstSeenScene,
                    iterationReasoningMode,
                    isLongRun,
                    sameActionStallCount,
                    freshnessFloor,
                    attemptsByPhase,
                    history,
                    logger,
                    observerReader,
                    evaluator,
                    (exitCode, status, message, terminalCause, failureClass) => CompleteAttempt(
                        exitCode,
                        status,
                        message,
                        terminalCause: terminalCause,
                        failureClass: failureClass),
                    PassiveWaitMs,
                    TransitionSettleMs)
                .ConfigureAwait(false);
            observer = preflight.Observer;
            phase = preflight.Phase;
            if (preflight.CompletedAttempt is not null)
            {
                return preflight.CompletedAttempt;
            }

            if (preflight.ShouldContinueLoop)
            {
                ResetDecisionWaitTracking();
                continue;
            }

            var captureMode = "skipped";
            var capturePolicy = Evaluate(stepIndex, isAuthoritativeFirstAttempt, stepAnalysisContext);
            var captureSkipReason = capturePolicy.SkipReason;
            if (capturePolicy.NeedsScreenshot)
            {
                var captureResult = captureService.TryCaptureDetailed(window, screenshotPath, ScreenCaptureService.CaptureTimeout);
                if (!captureResult.Succeeded)
                {
                    var captureFailureObserver = observerReader.Read(includeEventTail: false);
                    if (captureResult.FailureKind is CaptureBoundaryFailureKind.TimedOut or CaptureBoundaryFailureKind.Exception)
                    {
                        ResetDecisionWaitTracking();
                        var captureTerminalSignal = captureResult.FailureKind == CaptureBoundaryFailureKind.TimedOut
                            ? "capture-timeout"
                            : "capture-exception";
                        var captureFailureMessage = captureResult.FailureKind == CaptureBoundaryFailureKind.TimedOut
                            ? "capture-timeout"
                            : $"capture-exception: {captureResult.Detail ?? captureResult.Exception?.GetType().Name ?? "unknown"}";
                        LogHarness($"step={stepIndex} {captureTerminalSignal} detail={captureResult.Detail ?? "none"}");
                        logger.AppendTrace(new GuiSmokeTraceEntry(
                            DateTimeOffset.UtcNow,
                            stepIndex,
                            phase.ToString(),
                            captureTerminalSignal,
                            captureFailureObserver.CurrentScreen,
                            captureFailureObserver.InCombat,
                            null));
                        AppendProgressIfLongRun(
                            isLongRun,
                            logger,
                            EvaluateStepProgress(
                                stepIndex,
                                phase,
                                "capture-boundary-failure",
                                captureFailureObserver,
                                null,
                                null,
                                false,
                                "capture-boundary",
                                false,
                                sameActionStallCount,
                                captureTerminalSignal));
                        logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                            phase.ToString(),
                            captureFailureMessage,
                            captureFailureObserver.CurrentScreen,
                            captureFailureObserver.InCombat,
                            screenshotPath));
                        return CompleteAttempt(
                            1,
                            "failed",
                            captureFailureMessage,
                            terminalCause: captureTerminalSignal,
                            failureClass: ClassifyFailureForAttempt(phase, captureFailureObserver, captureTerminalSignal, launchFailed: false));
                    }

                    if (WindowLocator.IsHungWindow(window))
                    {
                        ResetDecisionWaitTracking();
                        LogHarness($"step={stepIndex} window not responding; aborting capture-driven wait");
                        logger.AppendTrace(new GuiSmokeTraceEntry(
                            DateTimeOffset.UtcNow,
                            stepIndex,
                            phase.ToString(),
                            "window-not-responding",
                            captureFailureObserver.CurrentScreen,
                            captureFailureObserver.InCombat,
                            null));
                        logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                            phase.ToString(),
                            "game-window-not-responding",
                            captureFailureObserver.CurrentScreen,
                            captureFailureObserver.InCombat,
                            screenshotPath));
                        return CompleteAttempt(
                            1,
                            "failed",
                            "game-window-not-responding",
                            terminalCause: "game-window-not-responding",
                            failureClass: "launch-runtime-noise");
                    }

                    ResetDecisionWaitTracking();
                    consecutiveBlackFrames += 1;
                    var transitionBlackFrame = RootSceneTransitionObserverSignals.ShouldTreatCaptureAsTransitionWait(phase, captureFailureObserver.Summary);
                    var captureFailureSignal = transitionBlackFrame ? "transition-black-frame" : "capture-unusable";
                    LogHarness(transitionBlackFrame
                        ? $"step={stepIndex} capture unusable during explicit transition/loading boundary; waiting for scene readiness"
                        : $"step={stepIndex} capture unusable; waiting for a valid process frame");
                    logger.AppendTrace(new GuiSmokeTraceEntry(
                        DateTimeOffset.UtcNow,
                        stepIndex,
                        phase.ToString(),
                        captureFailureSignal,
                        captureFailureObserver.CurrentScreen,
                        captureFailureObserver.InCombat,
                        null));
                    if (window.IsFallback && !HasLiveGameProcess())
                    {
                        consecutiveFallbackCapturesWithoutProcess += 1;
                        if (consecutiveFallbackCapturesWithoutProcess >= 3)
                        {
                            RecordAttemptStartupFailure("process-lost");
                            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                                phase.ToString(),
                                "process-lost",
                                null,
                                null,
                                screenshotPath));
                            return CompleteAttempt(
                                1,
                                "failed",
                                "process-lost",
                                terminalCause: "process-lost",
                                failureClass: "launch-runtime-noise");
                        }
                    }
                    else
                    {
                        consecutiveFallbackCapturesWithoutProcess = 0;
                    }

                    if (!transitionBlackFrame
                        && !window.IsFallback
                        && consecutiveBlackFrames >= 3)
                    {
                        var focusWindow = WindowLocator.EnsureInteractive(window);
                        LogHarness($"step={stepIndex} black-frame streak={consecutiveBlackFrames}; sending center nudge");
                        inputDriver.Click(focusWindow, 0.5, 0.5);
                        history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "black-frame-nudge", "center", DateTimeOffset.UtcNow));
                        logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "black-frame-nudge", null, null, "center"));
                        consecutiveBlackFrames = 0;
                    }

                    var captureFailureWait = await WaitWithObserverPollingAsync(
                            observerReader,
                            TransitionSettleMs,
                            75,
                            captureFailureObserver,
                            latestObserver => HasWaitWakeDelta(captureFailureObserver, latestObserver)
                                ? "observer-delta"
                                : evaluator.IsPhaseSatisfied(phase, latestObserver, history)
                                    ? "phase-satisfied"
                                    : null)
                        .ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(captureFailureWait.WakeReason))
                    {
                        LogHarness($"step={stepIndex} capture backoff woke early reason={captureFailureWait.WakeReason}");
                    }

                    continue;
                }

                consecutiveBlackFrames = 0;
                consecutiveFallbackCapturesWithoutProcess = 0;
                if (isAuthoritativeFirstAttempt && stepIndex == 1)
                {
                    RecordAttemptStartupStage("authoritative-first-screenshot-captured", "finished", screenshotPath);
                }

                if (isLongRun && !attemptStartedRecorded && stepIndex == 1)
                {
                    LongRunArtifacts.RecordAttemptStarted(sessionRoot, attemptId, attemptOrdinal, runId, trustStateAtStart, screenshotPath);
                    attemptStartedRecorded = true;
                }

                captureMode = "captured";
                captureSkipReason = "screenshot-required";
                observer = observerReader.Read();
                logger.WriteObserverCopies(stepPrefix, observer);
                LogHarness($"step={stepIndex} captured={screenshotPath}");
                LogHarness($"step={stepIndex} observer {DescribeObserverHuman(observer)} capturedAt={observer.CapturedAt?.ToString("O") ?? "null"}");
                (_, _, stepAnalysisContext, iterationSceneContext) = BuildIterationContext(
                    workspaceRoot,
                    phase,
                    history,
                    observer,
                    window,
                    screenshotPath,
                    isLongRun,
                    sceneHistoryIndex);
                iterationSceneSignature = iterationSceneContext.SceneSignature;
                iterationFirstSeenScene = iterationSceneContext.FirstSeenScene;
                iterationReasoningMode = iterationSceneContext.ReasoningMode;
            }
            else
            {
                if (isLongRun && !attemptStartedRecorded && stepIndex == 1)
                {
                    LongRunArtifacts.RecordAttemptStarted(sessionRoot, attemptId, attemptOrdinal, runId, trustStateAtStart, screenshotPath);
                    attemptStartedRecorded = true;
                }

                logger.WriteObserverCopies(stepPrefix, observer);
                LogHarness($"step={stepIndex} capture skipped reason={captureSkipReason}");
                LogHarness($"step={stepIndex} observer {DescribeObserverHuman(observer)} capturedAt={observer.CapturedAt?.ToString("O") ?? "null"}");
            }

            var requestBuild = BuildAndLogStepRequest(
                stepIndex,
                stepPrefix,
                runId,
                scenarioId,
                phase,
                screenshotPath,
                window,
                observer,
                history,
                workspaceRoot,
                sessionRoot,
                attemptId,
                attemptOrdinal,
                captureMode,
                stepAnalysisContext,
                iterationSceneContext,
                logger,
                stepStopwatch);
            var request = requestBuild.Request;
            var sceneReasoningMode = requestBuild.SceneReasoningMode;
            var requestReadyElapsedMs = requestBuild.RequestReadyElapsedMs;
            var requestPath = stepPrefix + ".request.json";
            var decisionPath = stepPrefix + ".decision.json";
            GuiSmokeReplayEvaluation replayEvaluation;
            GuiSmokeStepDecision decision;
            var activeCombatBarrier = phase == GuiSmokePhase.HandleCombat
                ? stepAnalysisContext.CombatBarrierEvaluation
                : CombatBarrierSupport.Inactive;
            if (phase == GuiSmokePhase.HandleCombat
                && activeCombatBarrier.IsActive
                && activeCombatBarrier.IsHardWaitBarrier)
            {
                decision = AutoDecisionProvider.CreateCombatBarrierWaitDecision(activeCombatBarrier, request.Observer.CurrentScreen);
                replayEvaluation = EvaluateAutoDecisionWithDiagnostics(requestPath, request, decision, stepAnalysisContext);
            }
            else if (provider is AutoDecisionProvider)
            {
                replayEvaluation = EvaluateAutoDecisionWithDiagnostics(requestPath, request, null, stepAnalysisContext);
                decision = replayEvaluation.Decision;
            }
            else
            {
                decision = await provider.GetDecisionAsync(requestPath, decisionPath, TimeSpan.FromMinutes(3), CancellationToken.None).ConfigureAwait(false);
                replayEvaluation = EvaluateAutoDecisionWithDiagnostics(requestPath, request, decision, stepAnalysisContext);
            }
            var decisionReadyElapsedMs = stepStopwatch.ElapsedMilliseconds;
            if (ShouldPersistCandidateDumpArtifact(request, decision))
            {
                WriteCandidateDumpArtifact(stepPrefix + ".candidates.json", replayEvaluation.CandidateDump);
            }
            logger.WriteDecision(decisionPath, decision);
            LogHarness($"step={stepIndex} decision status={decision.Status} action={decision.ActionKind ?? "null"} target={decision.TargetLabel ?? "null"} confidence={decision.Confidence?.ToString("0.00") ?? "null"} reason={decision.Reason ?? "null"}");
            LogHarness($"step={stepIndex} timing preflight->request={requestReadyElapsedMs}ms request->decision={Math.Max(0, decisionReadyElapsedMs - requestReadyElapsedMs)}ms captureMode={captureMode} sceneReasoningMode={sceneReasoningMode}");
            if (isLongRun)
            {
                LongRunArtifacts.MaybeRecordUnknownScene(sessionRoot, request, decision);
                if (ShouldRecordUnknownScene(request))
                {
                    sceneHistoryIndex.NoteUnknownScene(request);
                }
            }

            if (string.Equals(decision.Status, "abort", StringComparison.OrdinalIgnoreCase))
            {
                ResetDecisionWaitTracking();
                LogHarness($"step={stepIndex} aborted reason={decision.AbortReason ?? "null"}");
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
                        sameActionStallCount,
                        "decision-abort"));
                logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                    phase.ToString(),
                    decision.AbortReason ?? "decision aborted",
                    observer.CurrentScreen,
                    observer.InCombat,
                    screenshotPath));
                return CompleteAttempt(
                    1,
                    "failed",
                    decision.AbortReason ?? "decision aborted",
                    terminalCause: "decision-abort",
                    failureClass: ClassifyFailureForAttempt(phase, observer, "decision-abort", launchFailed: false));
            }

            if (string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase))
            {
                var waitFingerprint = BuildDecisionWaitFingerprint(phase, request.SceneSignature, observer, stepAnalysisContext);
                if (string.Equals(lastDecisionWaitFingerprint, waitFingerprint, StringComparison.Ordinal))
                {
                    consecutiveDecisionWaitCount += 1;
                }
                else
                {
                    lastDecisionWaitFingerprint = waitFingerprint;
                    consecutiveDecisionWaitCount = 1;
                }

                if (phase == GuiSmokePhase.HandleCombat
                    && CombatBarrierSupport.TryClassifyWaitPlateau(request, stepAnalysisContext, consecutiveDecisionWaitCount, out var barrierPlateauCause, out var barrierPlateauMessage))
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
                            sameActionStallCount,
                            "decision-wait",
                            "combat-barrier-wait-plateau"));
                    logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                        phase.ToString(),
                        barrierPlateauMessage,
                        observer.CurrentScreen,
                        observer.InCombat,
                        screenshotPath));
                    return CompleteAttempt(
                        1,
                        "failed",
                        barrierPlateauMessage,
                        terminalCause: barrierPlateauCause,
                        failureClass: ClassifyFailureForAttempt(phase, observer, barrierPlateauCause, launchFailed: false));
                }

                if (TryClassifyDecisionWaitPlateau(phase, observer, consecutiveDecisionWaitCount, out var plateauTerminalCause, out var plateauMessage))
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
                            sameActionStallCount,
                            "decision-wait",
                            "wait-plateau"));
                    logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                        phase.ToString(),
                        plateauMessage,
                        observer.CurrentScreen,
                        observer.InCombat,
                        screenshotPath));
                    return CompleteAttempt(
                        1,
                        "failed",
                        plateauMessage,
                        terminalCause: plateauTerminalCause,
                        failureClass: ClassifyFailureForAttempt(phase, observer, plateauTerminalCause, launchFailed: false));
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
                            sameActionStallCount,
                            "rest-site-post-click-noop"));
                    logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                        phase.ToString(),
                        restSitePostClickNoOpMessage,
                        observer.CurrentScreen,
                        observer.InCombat,
                        screenshotPath));
                    return CompleteAttempt(
                        1,
                        "failed",
                        restSitePostClickNoOpMessage,
                        terminalCause: restSiteTerminalCause,
                        failureClass: ClassifyFailureForAttempt(phase, observer, restSiteTerminalCause, launchFailed: false));
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
                        sameActionStallCount,
                        "decision-wait"));
                var decisionWait = await WaitWithObserverPollingAsync(
                        observerReader,
                        waitBudgetMs,
                        75,
                        observer,
                        latestObserver => HasWaitWakeDelta(observer, latestObserver)
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
                continue;
            }

            if (LooksLikeInspectOverlayState(observer) && IsOverlayCleanupTarget(decision.TargetLabel))
            {
                var overlayFingerprint = BuildOverlayLoopFingerprint(request.SceneSignature, observer);
                if (string.Equals(lastOverlayLoopFingerprint, overlayFingerprint, StringComparison.Ordinal))
                {
                    consecutiveOverlayLoopCount += 1;
                }
                else
                {
                    lastOverlayLoopFingerprint = overlayFingerprint;
                    consecutiveOverlayLoopCount = 1;
                }

                if (consecutiveOverlayLoopCount >= 4)
                {
                    var overlayLoopMessage = $"inspect-overlay-loop phase={phase} screen={observer.CurrentScreen ?? "null"} overlayAttempts={consecutiveOverlayLoopCount}";
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
                            sameActionStallCount,
                            "inspect-overlay-loop"));
                    logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                        phase.ToString(),
                        overlayLoopMessage,
                        observer.CurrentScreen,
                        observer.InCombat,
                        screenshotPath));
                    return CompleteAttempt(
                        1,
                        "failed",
                        overlayLoopMessage,
                        terminalCause: "inspect-overlay-loop",
                        failureClass: ClassifyFailureForAttempt(phase, observer, "inspect-overlay-loop", launchFailed: false));
                }
            }
            else if (!LooksLikeInspectOverlayState(observer) || !string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase))
            {
                ResetOverlayLoopTracking();
            }
            else if (!IsOverlayCleanupTarget(decision.TargetLabel))
            {
                ResetOverlayLoopTracking();
            }

            ResetDecisionWaitTracking();

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
                        sameActionStallCount,
                        "decision-stale"));
                var staleDecisionWait = await WaitWithObserverPollingAsync(
                        observerReader,
                        GetDecisionWaitMinimumMs(phase),
                        75,
                        observer,
                        latestObserver => HasWaitWakeDelta(observer, latestObserver)
                            ? "observer-delta"
                            : evaluator.IsPhaseSatisfied(phase, latestObserver, history)
                                ? "phase-satisfied"
                                : null)
                    .ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(staleDecisionWait.WakeReason))
                {
                    LogHarness($"step={stepIndex} stale decision backoff woke early reason={staleDecisionWait.WakeReason}");
                }
                continue;
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
                        sameActionStallCount,
                        "observer-drift"));
                var observerDriftWait = await WaitWithObserverPollingAsync(
                        observerReader,
                        GetDecisionWaitMinimumMs(phase),
                        75,
                        latestObserver,
                        currentObserver => HasWaitWakeDelta(latestObserver, currentObserver)
                            ? "observer-delta"
                            : evaluator.IsPhaseSatisfied(phase, currentObserver, history)
                                ? "phase-satisfied"
                                : null)
                    .ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(observerDriftWait.WakeReason))
                {
                    LogHarness($"step={stepIndex} observer drift backoff woke early reason={observerDriftWait.WakeReason}");
                }
                continue;
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
                        sameActionStallCount,
                        "reward-map-loop"));
                logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                    phase.ToString(),
                    rewardMapLoopMessage,
                    observer.CurrentScreen,
                    observer.InCombat,
                    screenshotPath));
                return CompleteAttempt(
                    1,
                    "failed",
                    rewardMapLoopMessage,
                    terminalCause: "reward-map-loop",
                    failureClass: ClassifyFailureForAttempt(phase, observer, "reward-map-loop", launchFailed: false));
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
                        sameActionStallCount,
                        "map-overlay-noop-loop"));
                logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                    phase.ToString(),
                    mapOverlayNoOpLoopMessage,
                    observer.CurrentScreen,
                    observer.InCombat,
                    screenshotPath));
                return CompleteAttempt(
                    1,
                    "failed",
                    mapOverlayNoOpLoopMessage,
                    terminalCause: "map-overlay-noop-loop",
                    failureClass: ClassifyFailureForAttempt(phase, observer, "map-overlay-noop-loop", launchFailed: false));
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
                        sameActionStallCount,
                        "map-transition-stall"));
                logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                    phase.ToString(),
                    mapTransitionStallMessage,
                    observer.CurrentScreen,
                    observer.InCombat,
                    screenshotPath));
                return CompleteAttempt(
                    1,
                    "failed",
                    mapTransitionStallMessage,
                    terminalCause: "map-transition-stall",
                    failureClass: ClassifyFailureForAttempt(phase, observer, "map-transition-stall", launchFailed: false));
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
                        sameActionStallCount,
                        "combat-noop-loop"));
                logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                    phase.ToString(),
                    combatNoOpLoopMessage,
                    observer.CurrentScreen,
                    observer.InCombat,
                    screenshotPath));
                return CompleteAttempt(
                    1,
                    "failed",
                    combatNoOpLoopMessage,
                    terminalCause: "combat-noop-loop",
                    failureClass: ClassifyFailureForAttempt(phase, observer, "combat-noop-loop", launchFailed: false));
            }

            var actionFingerprint = string.Join("|",
                phase.ToString(),
                request.SceneSignature,
                decision.ActionKind ?? "null",
                decision.TargetLabel ?? "null");
            if (string.Equals(lastActionFingerprint, actionFingerprint, StringComparison.Ordinal))
            {
                sameActionStallCount += 1;
            }
            else
            {
                lastActionFingerprint = actionFingerprint;
                sameActionStallCount = 0;
            }

            if (sameActionStallCount >= GetSameActionStallLimit(phase, decision))
            {
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
                        sameActionStallCount,
                        "same-action-stall"));
                logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                    phase.ToString(),
                    abortReason,
                    observer.CurrentScreen,
                    observer.InCombat,
                    screenshotPath));
                return CompleteAttempt(
                    1,
                    "failed",
                    abortReason,
                    terminalCause: "same-action-stall",
                    failureClass: ClassifyFailureForAttempt(phase, observer, "same-action-stall", launchFailed: false));
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
                        sameActionStallCount,
                        "window-drift"));
                var windowDriftWait = await WaitWithObserverPollingAsync(
                        observerReader,
                        TransitionSettleMs,
                        75,
                        latestObserver,
                        currentObserver => HasWaitWakeDelta(latestObserver, currentObserver)
                            ? "observer-delta"
                            : evaluator.IsPhaseSatisfied(phase, currentObserver, history)
                                ? "phase-satisfied"
                                : null)
                    .ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(windowDriftWait.WakeReason))
                {
                    LogHarness($"step={stepIndex} window drift backoff woke early reason={windowDriftWait.WakeReason}");
                }
                continue;
            }

            ExecuteDecisionActuation(
                stepIndex,
                phase,
                observer,
                request,
                decision,
                clickWindow,
                inputDriver,
                history,
                logger);

            var postAction = await FinalizePostActionAndAdvancePhaseAsync(
                    stepIndex,
                    phase,
                    observer,
                    request,
                    decision,
                    isLongRun,
                    sessionRoot,
                    sceneHistoryIndex,
                    logger,
                    observerReader,
                    evaluator,
                    history,
                    stepStopwatch,
                    decisionReadyElapsedMs,
                    ActionSettleMinimumMs,
                    CombatActionSettleMinimumMs,
                    CombatNoOpProbeGraceMs,
                    sameActionStallCount,
                    rewardMapRecoveryAttempt)
                .ConfigureAwait(false);

            phase = postAction.Phase;
            attemptsByPhase.Clear();
        }

        var finalObserver = observerReader.Read();
        logger.WriteFailureSummary(new GuiSmokeFailureSummary(
            phase.ToString(),
            "Global timeout reached.",
            finalObserver.CurrentScreen,
            finalObserver.InCombat,
            null));
        return CompleteAttempt(
            1,
            "failed",
            "global timeout",
            terminalCause: "global-timeout",
            failureClass: ClassifyFailureForAttempt(phase, finalObserver, "global-timeout", launchFailed: false));
    }
}
