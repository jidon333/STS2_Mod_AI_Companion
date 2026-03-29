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
        var loopTracking = new GuiSmokeAttemptLoopTrackingState();
        var attemptStartedRecorded = false;
        var consecutiveFallbackCapturesWithoutProcess = 0;
        var useDecisionAgeGuard = !string.Equals(providerKind, "auto", StringComparison.OrdinalIgnoreCase);
        var decisionStaleBudget = useDecisionAgeGuard
            ? TimeSpan.FromSeconds(2)
            : TimeSpan.FromSeconds(30);

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
                    loopTracking.SameActionStallCount,
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
                loopTracking.ResetDecisionWaitTracking();
                continue;
            }

            var capturePreparation = await TryPrepareStepCaptureContextAsync(
                    stepIndex,
                    stepPrefix,
                    screenshotPath,
                    phase,
                    window,
                    observer,
                    stepAnalysisContext,
                    iterationSceneContext,
                    workspaceRoot,
                    sessionRoot,
                    attemptId,
                    attemptOrdinal,
                    trustStateAtStart,
                    runId,
                    isLongRun,
                    isAuthoritativeFirstAttempt,
                    attemptStartedRecorded,
                    loopTracking.SameActionStallCount,
                    consecutiveBlackFrames,
                    consecutiveFallbackCapturesWithoutProcess,
                    history,
                    logger,
                    sceneHistoryIndex,
                    observerReader,
                    captureService,
                    inputDriver,
                    evaluator,
                    (exitCode, status, message, terminalCause, failureClass) => CompleteAttempt(
                        exitCode,
                        status,
                        message,
                        terminalCause: terminalCause,
                        failureClass: failureClass),
                    RecordAttemptStartupStage,
                    TransitionSettleMs)
                .ConfigureAwait(false);
            observer = capturePreparation.Observer;
            stepAnalysisContext = capturePreparation.StepAnalysisContext;
            iterationSceneContext = capturePreparation.SceneContext;
            iterationSceneSignature = iterationSceneContext.SceneSignature;
            iterationFirstSeenScene = iterationSceneContext.FirstSeenScene;
            iterationReasoningMode = iterationSceneContext.ReasoningMode;
            attemptStartedRecorded = capturePreparation.AttemptStartedRecorded;
            consecutiveBlackFrames = capturePreparation.ConsecutiveBlackFrames;
            consecutiveFallbackCapturesWithoutProcess = capturePreparation.ConsecutiveFallbackCapturesWithoutProcess;
            var captureMode = capturePreparation.CaptureMode;
            if (capturePreparation.CompletedAttempt is not null)
            {
                loopTracking.ResetDecisionWaitTracking();
                if (string.Equals(capturePreparation.CompletedAttempt.Message, "process-lost", StringComparison.OrdinalIgnoreCase))
                {
                    RecordAttemptStartupFailure("process-lost");
                }

                return capturePreparation.CompletedAttempt;
            }

            if (capturePreparation.ShouldContinueLoop)
            {
                loopTracking.ResetDecisionWaitTracking();
                continue;
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

            var decisionStatus = await TryHandleDecisionStatusAsync(
                    stepIndex,
                    screenshotPath,
                    phase,
                    observer,
                    request,
                    decision,
                    stepAnalysisContext,
                    isLongRun,
                    logger,
                    observerReader,
                    evaluator,
                    history,
                    stepStopwatch,
                    decisionReadyElapsedMs,
                    loopTracking,
                    (exitCode, status, message, terminalCause, failureClass) => CompleteAttempt(
                        exitCode,
                        status,
                        message,
                        terminalCause: terminalCause,
                        failureClass: failureClass))
                .ConfigureAwait(false);
            if (decisionStatus.CompletedAttempt is not null)
            {
                return decisionStatus.CompletedAttempt;
            }

            if (decisionStatus.ShouldContinueLoop)
            {
                continue;
            }

            var actuationPreparation = await TryPrepareDecisionActuationAsync(
                    stepIndex,
                    screenshotPath,
                    phase,
                    window,
                    observer,
                    request,
                    decision,
                    isLongRun,
                    logger,
                    observerReader,
                    evaluator,
                    history,
                    stepStopwatch,
                    loopTracking,
                    (exitCode, status, message, terminalCause, failureClass) => CompleteAttempt(
                        exitCode,
                        status,
                        message,
                        terminalCause: terminalCause,
                        failureClass: failureClass),
                    TransitionSettleMs,
                    decisionStaleBudget,
                    useDecisionAgeGuard)
                .ConfigureAwait(false);
            if (actuationPreparation.CompletedAttempt is not null)
            {
                return actuationPreparation.CompletedAttempt;
            }

            if (actuationPreparation.ShouldContinueLoop)
            {
                continue;
            }

            var latestObserver = actuationPreparation.LatestObserver;
            var clickWindow = actuationPreparation.ClickWindow ?? window;
            var rewardMapRecoveryAttempt = actuationPreparation.RewardMapRecoveryAttempt;

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
                    loopTracking.SameActionStallCount,
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
