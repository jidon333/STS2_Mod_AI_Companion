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
            var captureResult = captureService.TryCaptureDetailed(window, screenshotPath, ScreenCaptureService.CaptureTimeout);
            if (!captureResult.Succeeded)
            {
                var captureFailureObserver = observerReader.Read();
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
                await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
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
            LogHarness($"step={stepIndex} captured={screenshotPath}");
            var stepStopwatch = Stopwatch.StartNew();

            var observer = observerReader.Read();
            if (isLongRun
                && history.Count == 0
                && phase == GuiSmokePhase.WaitMainMenu
                && !IsManualCleanBootObserverReady(observer, freshnessFloor))
            {
                observer = await BootstrapManualCleanBootObserverAsync(
                        observer,
                        observerReader.Read,
                        freshnessFloor,
                        ManualCleanBootObserverBootstrapPollCount,
                        ManualCleanBootObserverBootstrapPollMs)
                    .ConfigureAwait(false);
            }

            logger.WriteObserverCopies(stepPrefix, observer);
            LogHarness($"step={stepIndex} observer {DescribeObserverHuman(observer)} capturedAt={observer.CapturedAt?.ToString("O") ?? "null"}");
            var serializedHistory = BuildSerializedStepHistory(phase, history);
            var combatCardKnowledge = LoadCombatCardKnowledge(workspaceRoot, observer);
            var stepAnalysisContext = CreateStepAnalysisContext(
                phase,
                observer,
                screenshotPath,
                serializedHistory,
                combatCardKnowledge,
                new WindowBounds(window.Bounds.X, window.Bounds.Y, window.Bounds.Width, window.Bounds.Height));
            var iterationSceneSignature = ComputeSceneSignatureCore(screenshotPath, observer, phase, stepAnalysisContext);
            var iterationFirstSeenScene = isLongRun && !sceneHistoryIndex.HasSeen(iterationSceneSignature);
            var iterationReasoningMode = DetermineReasoningMode(phase, observer, iterationFirstSeenScene);
            var iterationKnownRecipes = stepAnalysisContext.UseAuthorityFastPath
                ? Array.Empty<KnownRecipeHint>()
                : sceneHistoryIndex.GetKnownRecipes(iterationSceneSignature, phase.ToString());
            var iterationSceneContext = new GuiSmokeSceneRequestContext(
                iterationSceneSignature,
                iterationFirstSeenScene,
                iterationReasoningMode,
                iterationKnownRecipes);

            if (observer.SceneReady == false)
            {
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "scene-not-ready", observer.CurrentScreen, observer.InCombat, null));
            }

            if (isLongRun
                && history.Count > 0
                && phase is not GuiSmokePhase.WaitMainMenu and not GuiSmokePhase.EnterRun and not GuiSmokePhase.WaitRunLoad and not GuiSmokePhase.WaitCharacterSelect
                && string.Equals(observer.CurrentScreen, "main-menu", StringComparison.OrdinalIgnoreCase))
            {
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "terminal-detected", observer.CurrentScreen, observer.InCombat, "returned-main-menu"));
                logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                    phase.ToString(),
                    "returned-main-menu",
                    observer.CurrentScreen,
                    observer.InCombat,
                    screenshotPath));
                AppendProgressIfLongRun(
                    isLongRun,
                    logger,
                    EvaluateStepProgress(
                        stepIndex,
                        phase,
                        iterationSceneSignature,
                        observer,
                        null,
                        null,
                        iterationFirstSeenScene,
                        iterationReasoningMode,
                        false,
                        sameActionStallCount,
                        "terminal-returned-main-menu"));
                return CompleteAttempt(
                    0,
                    "completed",
                    "returned-main-menu",
                    terminalCause: "returned-main-menu");
            }

            if (isLongRun
                && history.Count > 0
                && phase is not GuiSmokePhase.WaitMainMenu and not GuiSmokePhase.EnterRun and not GuiSmokePhase.WaitRunLoad and not GuiSmokePhase.WaitCharacterSelect
                && observer.Summary.PlayerCurrentHp is <= 0)
            {
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "terminal-detected", observer.CurrentScreen, observer.InCombat, "player-defeated"));
                logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                    phase.ToString(),
                    "player-defeated",
                    observer.CurrentScreen,
                    observer.InCombat,
                    screenshotPath));
                AppendProgressIfLongRun(
                    isLongRun,
                    logger,
                    EvaluateStepProgress(
                        stepIndex,
                        phase,
                        iterationSceneSignature,
                        observer,
                        null,
                        null,
                        iterationFirstSeenScene,
                        iterationReasoningMode,
                        false,
                        sameActionStallCount,
                        "terminal-player-defeated"));
                return CompleteAttempt(
                    0,
                    "completed",
                    "player-defeated",
                    terminalCause: "player-defeated");
            }

            if (!observer.IsFreshSince(freshnessFloor))
            {
                ResetDecisionWaitTracking();
                LogHarness($"step={stepIndex} stale observer snapshot ignored freshnessFloor={freshnessFloor:O}");
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "stale-observer", observer.CurrentScreen, observer.InCombat, null));
                AppendProgressIfLongRun(
                    isLongRun,
                    logger,
                    EvaluateStepProgress(
                        stepIndex,
                        phase,
                        iterationSceneSignature,
                        observer,
                        null,
                        null,
                        iterationFirstSeenScene,
                        iterationReasoningMode,
                        false,
                        sameActionStallCount,
                        "observer-stale"));
                await Task.Delay(PassiveWaitMs).ConfigureAwait(false);
                continue;
            }

            if (evaluator.IsPhaseSatisfied(phase, observer, history))
            {
                ResetDecisionWaitTracking();
                LogHarness($"step={stepIndex} observer accepted phase={phase}");
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "observer-accepted", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "observer-accepted", observer.CurrentScreen, observer.InCombat, null));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "observer-confirmed", observer.CurrentScreen, observer.InCombat, null));
                AppendProgressIfLongRun(
                    isLongRun,
                    logger,
                    EvaluateStepProgress(
                        stepIndex,
                        phase,
                        iterationSceneSignature,
                        observer,
                        null,
                        null,
                        iterationFirstSeenScene,
                        iterationReasoningMode,
                        false,
                        sameActionStallCount,
                        "observer-confirmed"));
                phase = phase switch
                {
                    GuiSmokePhase.WaitMainMenu => GuiSmokePhase.EnterRun,
                    GuiSmokePhase.WaitRunLoad => GuiSmokePhase.WaitRunLoad,
                    GuiSmokePhase.WaitCharacterSelect => GuiSmokePhase.ChooseCharacter,
                    GuiSmokePhase.WaitMap => GuiSmokePhase.ChooseFirstNode,
                    GuiSmokePhase.WaitPostMapNodeRoom => GuiSmokePhase.ChooseFirstNode,
                    GuiSmokePhase.WaitCombat => GuiSmokePhase.HandleCombat,
                    _ => phase,
                };

                if (phase == GuiSmokePhase.Completed)
                {
                    return CompleteAttempt(
                        0,
                        "passed",
                        "combat flow accepted by observer",
                        terminalCause: "combat-flow-accepted");
                }

                await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
                continue;
            }

            if (TryAdvanceAlternateBranch(phase, observer, history, logger, stepIndex, isLongRun, out var alternatePhase))
            {
                ResetDecisionWaitTracking();
                LogHarness($"step={stepIndex} alternate branch {phase} -> {alternatePhase} from screen={observer.CurrentScreen ?? "null"}");
                AppendProgressIfLongRun(
                    isLongRun,
                    logger,
                    EvaluateStepProgress(
                        stepIndex,
                        phase,
                        iterationSceneSignature,
                        observer,
                        null,
                        null,
                        iterationFirstSeenScene,
                        iterationReasoningMode,
                        false,
                        sameActionStallCount,
                        $"alternate-branch:{alternatePhase}"));
                phase = alternatePhase;

                if (phase == GuiSmokePhase.Completed)
                {
                    return CompleteAttempt(
                        0,
                        "passed",
                        "combat flow accepted by observer",
                        terminalCause: "combat-flow-accepted");
                }

                await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
                continue;
            }

            if (IsPassiveWaitPhase(phase))
            {
                ResetDecisionWaitTracking();
                var waitAttempt = IncrementAttempt(attemptsByPhase, phase);
                if (phase == GuiSmokePhase.WaitCharacterSelect
                    && string.Equals(observer.CurrentScreen, "main-menu", StringComparison.OrdinalIgnoreCase)
                    && waitAttempt >= 2
                    && waitAttempt % 2 == 0)
                {
                    LogHarness($"step={stepIndex} still on main-menu after continue; retrying enter-run attempt={waitAttempt}");
                    history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "retry-enter-run", observer.CurrentScreen, DateTimeOffset.UtcNow));
                    logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "retry-enter-run", observer.CurrentScreen, observer.InCombat, null));
                    phase = GuiSmokePhase.EnterRun;
                    await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
                    continue;
                }

                if (waitAttempt >= 30)
                {
                    LogHarness($"step={stepIndex} timeout waiting for phase={phase} screen={observer.CurrentScreen ?? "null"}");
                    logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                        phase.ToString(),
                        $"Timed out waiting for observer phase: {phase}",
                        observer.CurrentScreen,
                        observer.InCombat,
                        screenshotPath));
                    return CompleteAttempt(
                        1,
                        "failed",
                        $"timeout at {phase}",
                        terminalCause: "phase-timeout",
                        failureClass: ClassifyFailureForAttempt(phase, observer, "phase-timeout", launchFailed: false));
                }

                LogHarness($"step={stepIndex} waiting phase={phase} attempt={waitAttempt} screen={observer.CurrentScreen ?? "null"}");
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "waiting", observer.CurrentScreen, observer.InCombat, null));
                AppendProgressIfLongRun(
                    isLongRun,
                    logger,
                    EvaluateStepProgress(
                        stepIndex,
                        phase,
                        iterationSceneSignature,
                        observer,
                        null,
                        null,
                        iterationFirstSeenScene,
                        iterationReasoningMode,
                        false,
                        sameActionStallCount,
                        "passive-wait"));
                await Task.Delay(PassiveWaitMs).ConfigureAwait(false);
                continue;
            }

            var request = CreateStepRequest(
                runId,
                scenarioId,
                stepIndex,
                phase,
                screenshotPath,
                window,
                observer,
                history,
                workspaceRoot,
                sessionRoot,
                attemptId,
                attemptOrdinal,
                stepAnalysisContext,
                iterationSceneContext);
            var requestPath = stepPrefix + ".request.json";
            var decisionPath = stepPrefix + ".decision.json";
            logger.WriteRequest(requestPath, request);
            LogHarness($"step={stepIndex} request={requestPath}");
            var requestReadyElapsedMs = stepStopwatch.ElapsedMilliseconds;
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
            LogHarness($"step={stepIndex} timing captured->request={requestReadyElapsedMs}ms request->decision={Math.Max(0, decisionReadyElapsedMs - requestReadyElapsedMs)}ms");
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
                LogHarness($"step={stepIndex} wait requested ms={Math.Max(250, decision.WaitMs ?? waitMinimumMs)}");
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
                await Task.Delay(Math.Max(waitMinimumMs, decision.WaitMs ?? waitMinimumMs)).ConfigureAwait(false);
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
                await Task.Delay(GetDecisionWaitMinimumMs(phase)).ConfigureAwait(false);
                continue;
            }

            var latestObserver = observerReader.Read();
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
                await Task.Delay(GetDecisionWaitMinimumMs(phase)).ConfigureAwait(false);
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
                await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
                continue;
            }

            if (string.Equals(decision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase))
            {
                if (IsNonEnemyCombatSelectionLabel(decision.TargetLabel))
                {
                    inputDriver.MoveCursor(clickWindow, GuiSmokeCombatConstants.NonEnemyPrimeNormalizedX, GuiSmokeCombatConstants.NonEnemyPrimeNormalizedY);
                    LogHarness($"step={stepIndex} cursor primed for non-enemy staging point normalized=({GuiSmokeCombatConstants.NonEnemyPrimeNormalizedX:0.000},{GuiSmokeCombatConstants.NonEnemyPrimeNormalizedY:0.000})");
                }

                LogHarness($"step={stepIndex} key target={decision.TargetLabel ?? decision.KeyText ?? "null"} key={decision.KeyText ?? "null"}");
                inputDriver.PressKey(clickWindow, decision.KeyText!);
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "press-key", decision.TargetLabel ?? decision.KeyText, DateTimeOffset.UtcNow)
                {
                    Metadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(request, decision),
                });
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "press-key", observer.CurrentScreen, observer.InCombat, decision.TargetLabel ?? decision.KeyText));
                LogHarness($"step={stepIndex} key sent key={decision.KeyText ?? "null"}");
            }
            else if (string.Equals(decision.ActionKind, "right-click", StringComparison.OrdinalIgnoreCase))
            {
                var rightClickX = decision.NormalizedX ?? 0.5;
                var rightClickY = decision.NormalizedY ?? 0.5;
                var clickPoint = MouseInputDriver.TransformNormalizedPoint(clickWindow, rightClickX, rightClickY);
                LogHarness($"step={stepIndex} right-click target={decision.TargetLabel ?? "null"} normalized=({rightClickX:0.000},{rightClickY:0.000}) absolute=({clickPoint.X},{clickPoint.Y}) bounds={DescribeBounds(clickWindow.Bounds)}");
                inputDriver.RightClick(clickWindow, rightClickX, rightClickY);
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "right-click", decision.TargetLabel, DateTimeOffset.UtcNow)
                {
                    Metadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(request, decision),
                });
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "right-click", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
                LogHarness($"step={stepIndex} right-click sent target={decision.TargetLabel ?? "null"}");
            }
            else if (string.Equals(decision.ActionKind, "click-current", StringComparison.OrdinalIgnoreCase))
            {
                LogHarness($"step={stepIndex} click-current target={decision.TargetLabel ?? "null"}");
                inputDriver.ClickCurrent(clickWindow);
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "click-current", decision.TargetLabel, DateTimeOffset.UtcNow)
                {
                    Metadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(request, decision),
                });
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "click-current", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
                LogHarness($"step={stepIndex} click-current sent target={decision.TargetLabel ?? "null"}");
            }
            else if (string.Equals(decision.ActionKind, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase))
            {
                LogHarness($"step={stepIndex} confirm-non-enemy target={decision.TargetLabel ?? "null"} normalized=({GuiSmokeCombatConstants.NonEnemyConfirmNormalizedX:0.000},{GuiSmokeCombatConstants.NonEnemyConfirmNormalizedY:0.000}) holdMs={GuiSmokeCombatConstants.NonEnemyConfirmHoldMs}");
                inputDriver.ConfirmNonEnemy(clickWindow, GuiSmokeCombatConstants.NonEnemyConfirmNormalizedX, GuiSmokeCombatConstants.NonEnemyConfirmNormalizedY, GuiSmokeCombatConstants.NonEnemyConfirmHoldMs);
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "confirm-non-enemy", decision.TargetLabel, DateTimeOffset.UtcNow)
                {
                    Metadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(request, decision),
                });
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "confirm-non-enemy", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
                LogHarness($"step={stepIndex} confirm-non-enemy sent target={decision.TargetLabel ?? "null"}");
            }
            else
            {
                var clickPoint = MouseInputDriver.TransformNormalizedPoint(clickWindow, decision.NormalizedX!.Value, decision.NormalizedY!.Value);
                if (ShouldUseHoverPrimedClick(decision))
                {
                    LogHarness($"step={stepIndex} click target={decision.TargetLabel ?? "null"} hoverPrime=true normalized=({decision.NormalizedX:0.000},{decision.NormalizedY:0.000}) absolute=({clickPoint.X},{clickPoint.Y}) bounds={DescribeBounds(clickWindow.Bounds)}");
                    inputDriver.HoverPrimedClick(clickWindow, decision.NormalizedX!.Value, decision.NormalizedY!.Value);
                }
                else
                {
                    LogHarness($"step={stepIndex} click target={decision.TargetLabel ?? "null"} normalized=({decision.NormalizedX:0.000},{decision.NormalizedY:0.000}) absolute=({clickPoint.X},{clickPoint.Y}) bounds={DescribeBounds(clickWindow.Bounds)}");
                    inputDriver.Click(clickWindow, decision.NormalizedX!.Value, decision.NormalizedY!.Value);
                }
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "click", decision.TargetLabel, DateTimeOffset.UtcNow)
                {
                    Metadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(request, decision),
                });
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "click", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
                LogHarness($"step={stepIndex} click sent target={decision.TargetLabel ?? "null"}");
            }

            var completedPhase = phase;
            var recipeRecorded = false;
            if (isLongRun)
            {
                LongRunArtifacts.AppendSceneRecipe(sessionRoot, request, decision);
                recipeRecorded = !string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                                 && !string.Equals(decision.Status, "abort", StringComparison.OrdinalIgnoreCase)
                                 && !string.IsNullOrWhiteSpace(decision.ActionKind);
                if (recipeRecorded)
                {
                    sceneHistoryIndex.NoteRecipe(request, decision);
                }
            }

            var settleDelayMs = GetActionSettleDelayMs(completedPhase, decision, ActionSettleMinimumMs, CombatActionSettleMinimumMs);
            var progressProbeDelayMs = Math.Min(settleDelayMs, completedPhase == GuiSmokePhase.HandleCombat ? 80 : 180);
            if (progressProbeDelayMs > 0)
            {
                await Task.Delay(progressProbeDelayMs).ConfigureAwait(false);
            }

            var extraProgressSignals = new List<string>();
            if (rewardMapRecoveryAttempt)
            {
                extraProgressSignals.Add("reward-map-recovery-attempt");
            }

            var postActionObserver = observerReader.Read();
            var additionalProbeDelayMs = 0;
            if (ShouldGrantCombatNoOpProbeGrace(completedPhase, observer, postActionObserver, decision))
            {
                additionalProbeDelayMs = CombatNoOpProbeGraceMs;
                extraProgressSignals.Add("combat-noop-probe-grace");
                LogHarness($"step={stepIndex} combat noop probe grace target={decision.TargetLabel ?? "null"} delayMs={additionalProbeDelayMs}");
                await Task.Delay(additionalProbeDelayMs).ConfigureAwait(false);
                postActionObserver = observerReader.Read();
                if (!ShouldGrantCombatNoOpProbeGrace(completedPhase, observer, postActionObserver, decision))
                {
                    extraProgressSignals.Add("combat-noop-probe-recovered");
                    LogHarness($"step={stepIndex} combat noop probe recovered target={decision.TargetLabel ?? "null"} screen={postActionObserver.CurrentScreen ?? postActionObserver.VisibleScreen ?? "null"}");
                }
            }

            if (TryRecordCombatNoOpObservation(
                    completedPhase,
                    observer,
                    postActionObserver,
                    decision,
                    history,
                    logger,
                    stepIndex,
                    out var combatNoOpSignal))
            {
                extraProgressSignals.Add(combatNoOpSignal);
            }

            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    completedPhase,
                    request.SceneSignature,
                    observer,
                    postActionObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    recipeRecorded,
                    sameActionStallCount,
                    extraProgressSignals.ToArray()));
            LogHarness($"step={stepIndex} timing decision->after={Math.Max(0, stepStopwatch.ElapsedMilliseconds - decisionReadyElapsedMs)}ms total={stepStopwatch.ElapsedMilliseconds}ms");

            phase = phase switch
            {
                GuiSmokePhase.EnterRun => GetPostEnterRunPhase(decision),
                GuiSmokePhase.ChooseCharacter => GuiSmokePhase.Embark,
                GuiSmokePhase.Embark => GuiSmokePhase.WaitRunLoad,
                GuiSmokePhase.HandleRewards => GetPostRewardPhase(decision),
                GuiSmokePhase.ChooseFirstNode => GetPostChooseFirstNodePhase(decision),
                GuiSmokePhase.HandleEvent => GetPostHandleEventPhase(decision),
                GuiSmokePhase.HandleCombat => GuiSmokePhase.HandleCombat,
                _ => phase,
            };

            if (phase == GuiSmokePhase.WaitRunLoad
                && GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(postActionObserver, out var postRunLoadPhase))
            {
                LogHarness($"step={stepIndex} post-action phase reconciliation WaitRunLoad -> {postRunLoadPhase} from screen={postActionObserver.CurrentScreen ?? "null"}");
                phase = postRunLoadPhase;
            }

            if (phase == GuiSmokePhase.WaitCharacterSelect
                && GuiSmokeObserverPhaseHeuristics.TryGetPostCharacterSelectPhase(postActionObserver, out var postCharacterSelectPhase))
            {
                LogHarness($"step={stepIndex} post-action phase reconciliation WaitCharacterSelect -> {postCharacterSelectPhase} from screen={postActionObserver.CurrentScreen ?? "null"}");
                phase = postCharacterSelectPhase;
            }

            if (phase == GuiSmokePhase.Embark && GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(postActionObserver, out var postEmbarkPhase))
            {
                LogHarness($"step={stepIndex} post-action phase reconciliation Embark -> {postEmbarkPhase} from screen={postActionObserver.CurrentScreen ?? "null"}");
                phase = postEmbarkPhase;
            }

            if (phase == GuiSmokePhase.WaitPostMapNodeRoom
                && GuiSmokeObserverPhaseHeuristics.TryGetPostMapNodePhase(postActionObserver, out var postMapNodePhase))
            {
                LogHarness($"step={stepIndex} post-action phase reconciliation WaitPostMapNodeRoom -> {postMapNodePhase} from screen={postActionObserver.CurrentScreen ?? "null"}");
                phase = postMapNodePhase;
            }

            if (phase == GuiSmokePhase.WaitEventRelease
                && GuiSmokeObserverPhaseHeuristics.TryGetPostEventReleasePhase(postActionObserver, out var postEventReleasePhase))
            {
                LogHarness($"step={stepIndex} post-action phase reconciliation WaitEventRelease -> {postEventReleasePhase} from screen={postActionObserver.CurrentScreen ?? "null"}");
                phase = postEventReleasePhase;
            }

            attemptsByPhase.Clear();
            LogHarness($"step={stepIndex} next phase={phase}");
            var remainingSettleDelayMs = Math.Max(0, settleDelayMs - progressProbeDelayMs - additionalProbeDelayMs);
            if (remainingSettleDelayMs > 0)
            {
                await Task.Delay(remainingSettleDelayMs).ConfigureAwait(false);
            }
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
