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
using static GuiSmokeSceneReasoningSupport;
using static GuiSmokeStepRequestFactory;
using static ObserverScreenProvenance;

internal static partial class Program
{
    sealed record GuiSmokeAttemptPreflightResult(
        GuiSmokeAttemptResult? CompletedAttempt,
        bool ShouldContinueLoop,
        GuiSmokePhase Phase,
        ObserverState Observer);

    sealed record GuiSmokeStepRequestBuildResult(
        GuiSmokeStepRequest Request,
        string SceneReasoningMode,
        long RequestReadyElapsedMs);

    static (
        IReadOnlyList<GuiSmokeHistoryEntry> SerializedHistory,
        IReadOnlyList<CombatCardKnowledgeHint> CombatCardKnowledge,
        GuiSmokeStepAnalysisContext AnalysisContext,
        GuiSmokeSceneRequestContext SceneContext)
        BuildIterationContext(
            string workspaceRoot,
            GuiSmokePhase phase,
            IReadOnlyList<GuiSmokeHistoryEntry> history,
            ObserverState currentObserver,
            WindowCaptureTarget currentWindow,
            string? analysisScreenshotPath,
            bool isLongRun,
            GuiSmokeSessionSceneHistoryIndex sceneHistoryIndex)
    {
        var serializedHistory = BuildSerializedStepHistory(phase, history);
        var combatCardKnowledge = LoadCombatCardKnowledge(workspaceRoot, currentObserver);
        var windowBounds = new WindowBounds(currentWindow.Bounds.X, currentWindow.Bounds.Y, currentWindow.Bounds.Width, currentWindow.Bounds.Height);
        var stepAnalysisContext = string.IsNullOrWhiteSpace(analysisScreenshotPath)
            ? CreateObserverOnlyAnalysisContext(phase, currentObserver, serializedHistory, combatCardKnowledge, windowBounds)
            : CreateStepAnalysisContext(phase, currentObserver, analysisScreenshotPath, serializedHistory, combatCardKnowledge, windowBounds);
        var iterationSceneSignature = ComputeSceneSignatureCore(analysisScreenshotPath, currentObserver, phase, stepAnalysisContext);
        var iterationFirstSeenScene = isLongRun && !sceneHistoryIndex.HasSeen(iterationSceneSignature);
        var iterationReasoningMode = DetermineReasoningMode(phase, currentObserver, iterationFirstSeenScene);
        var iterationKnownRecipes = stepAnalysisContext.UseAuthorityFastPath
            ? Array.Empty<KnownRecipeHint>()
            : sceneHistoryIndex.GetKnownRecipes(iterationSceneSignature, phase.ToString());
        return (
            serializedHistory,
            combatCardKnowledge,
            stepAnalysisContext,
            new GuiSmokeSceneRequestContext(
                iterationSceneSignature,
                iterationFirstSeenScene,
                iterationReasoningMode,
                iterationKnownRecipes));
    }

    static bool HasWaitWakeDelta(ObserverState baselineObserver, ObserverState latestObserver)
    {
        return HasMeaningfulObserverDelta(baselineObserver, latestObserver)
               || !string.Equals(PublishedCurrentScreen(baselineObserver), PublishedCurrentScreen(latestObserver), StringComparison.OrdinalIgnoreCase)
               || !string.Equals(PublishedVisibleScreen(baselineObserver), PublishedVisibleScreen(latestObserver), StringComparison.OrdinalIgnoreCase)
               || PublishedSceneReady(baselineObserver) != PublishedSceneReady(latestObserver)
               || !string.Equals(PublishedSceneAuthority(baselineObserver), PublishedSceneAuthority(latestObserver), StringComparison.OrdinalIgnoreCase)
               || !string.Equals(PublishedSceneStability(baselineObserver), PublishedSceneStability(latestObserver), StringComparison.OrdinalIgnoreCase)
               || !string.Equals(baselineObserver.ChoiceExtractorPath, latestObserver.ChoiceExtractorPath, StringComparison.OrdinalIgnoreCase)
               || baselineObserver.SnapshotVersion != latestObserver.SnapshotVersion;
    }

    static async Task<(ObserverState Observer, string? WakeReason)> WaitWithObserverPollingAsync(
        ObserverSnapshotReader observerReader,
        int totalBudgetMs,
        int sliceMs,
        ObserverState baselineObserver,
        Func<ObserverState, string?> wakeEvaluator,
        bool includeEventTail = false)
    {
        if (totalBudgetMs <= 0)
        {
            return (baselineObserver, null);
        }

        var remainingBudgetMs = totalBudgetMs;
        var latestObserver = baselineObserver;
        while (remainingBudgetMs > 0)
        {
            var delayMs = Math.Min(sliceMs, remainingBudgetMs);
            await Task.Delay(delayMs).ConfigureAwait(false);
            remainingBudgetMs -= delayMs;
            latestObserver = observerReader.Read(includeEventTail);
            var wakeReason = wakeEvaluator(latestObserver);
            if (!string.IsNullOrWhiteSpace(wakeReason))
            {
                return (latestObserver, wakeReason);
            }
        }

        return (latestObserver, null);
    }

    static async Task<GuiSmokeAttemptPreflightResult> TryHandleStepObserverPreflightAsync(
        int stepIndex,
        string stepPrefix,
        string screenshotPath,
        GuiSmokePhase phase,
        ObserverState observer,
        string iterationSceneSignature,
        bool iterationFirstSeenScene,
        string iterationReasoningMode,
        bool isLongRun,
        int sameActionStallCount,
        DateTimeOffset freshnessFloor,
        Dictionary<GuiSmokePhase, int> attemptsByPhase,
        List<GuiSmokeHistoryEntry> history,
        ArtifactRecorder logger,
        ObserverSnapshotReader observerReader,
        ObserverAcceptanceEvaluator evaluator,
        Func<int, string, string, string?, string?, GuiSmokeAttemptResult> completeAttempt,
        int passiveWaitMs,
        int transitionSettleMs)
    {
        if (isLongRun
            && history.Count > 0
            && phase is not GuiSmokePhase.WaitMainMenu and not GuiSmokePhase.EnterRun and not GuiSmokePhase.WaitRunLoad and not GuiSmokePhase.WaitCharacterSelect
            && MatchesControlFlowScreen(observer, "main-menu"))
        {
            logger.WriteObserverCopies(stepPrefix, observer);
            LogHarness($"step={stepIndex} capture skipped reason=terminal-current-screen");
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "terminal-detected", observer.CurrentScreen, observer.InCombat, "returned-main-menu"));
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
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                "returned-main-menu",
                observer.CurrentScreen,
                observer.InCombat,
                screenshotPath));
            return new GuiSmokeAttemptPreflightResult(
                completeAttempt(0, "completed", "returned-main-menu", "returned-main-menu", null),
                ShouldContinueLoop: false,
                phase,
                observer);
        }

        if (isLongRun
            && history.Count > 0
            && phase is not GuiSmokePhase.WaitMainMenu and not GuiSmokePhase.EnterRun and not GuiSmokePhase.WaitRunLoad and not GuiSmokePhase.WaitCharacterSelect
            && observer.Summary.PlayerCurrentHp is <= 0)
        {
            logger.WriteObserverCopies(stepPrefix, observer);
            LogHarness($"step={stepIndex} capture skipped reason=terminal-player-defeated");
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "terminal-detected", observer.CurrentScreen, observer.InCombat, "player-defeated"));
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
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                "player-defeated",
                observer.CurrentScreen,
                observer.InCombat,
                screenshotPath));
            return new GuiSmokeAttemptPreflightResult(
                completeAttempt(0, "completed", "player-defeated", "player-defeated", null),
                ShouldContinueLoop: false,
                phase,
                observer);
        }

        if (!observer.IsFreshSince(freshnessFloor))
        {
            logger.WriteObserverCopies(stepPrefix, observer);
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
            var staleObserverWait = await WaitWithObserverPollingAsync(
                    observerReader,
                    passiveWaitMs,
                    75,
                    observer,
                    latestObserver => latestObserver.IsFreshSince(freshnessFloor)
                        ? "observer-fresh"
                        : HasWaitWakeDelta(observer, latestObserver)
                            ? "observer-delta"
                            : null)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(staleObserverWait.WakeReason))
            {
                LogHarness($"step={stepIndex} stale observer wait woke early reason={staleObserverWait.WakeReason}");
            }

            return new GuiSmokeAttemptPreflightResult(null, ShouldContinueLoop: true, phase, staleObserverWait.Observer);
        }

        if (evaluator.IsPhaseSatisfied(phase, observer, history))
        {
            logger.WriteObserverCopies(stepPrefix, observer);
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
                return new GuiSmokeAttemptPreflightResult(
                    completeAttempt(0, "passed", "combat flow accepted by observer", "combat-flow-accepted", null),
                    ShouldContinueLoop: false,
                    phase,
                    observer);
            }

            var acceptedSettleWait = await WaitWithObserverPollingAsync(
                    observerReader,
                    transitionSettleMs,
                    75,
                    observer,
                    latestObserver => HasWaitWakeDelta(observer, latestObserver)
                        ? "observer-delta"
                        : evaluator.IsPhaseSatisfied(phase, latestObserver, history)
                            ? "phase-satisfied"
                            : null)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(acceptedSettleWait.WakeReason))
            {
                LogHarness($"step={stepIndex} observer accepted settle woke early reason={acceptedSettleWait.WakeReason}");
            }

            return new GuiSmokeAttemptPreflightResult(null, ShouldContinueLoop: true, phase, acceptedSettleWait.Observer);
        }

        if (TryAdvanceAlternateBranch(phase, observer, history, logger, stepIndex, isLongRun, out var alternatePhase))
        {
            logger.WriteObserverCopies(stepPrefix, observer);
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
                return new GuiSmokeAttemptPreflightResult(
                    completeAttempt(0, "passed", "combat flow accepted by observer", "combat-flow-accepted", null),
                    ShouldContinueLoop: false,
                    phase,
                    observer);
            }

            var alternateBranchWait = await WaitWithObserverPollingAsync(
                    observerReader,
                    transitionSettleMs,
                    75,
                    observer,
                    latestObserver => HasWaitWakeDelta(observer, latestObserver)
                        ? "observer-delta"
                        : evaluator.IsPhaseSatisfied(phase, latestObserver, history)
                            ? "phase-satisfied"
                            : null)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(alternateBranchWait.WakeReason))
            {
                LogHarness($"step={stepIndex} alternate branch settle woke early reason={alternateBranchWait.WakeReason}");
            }

            return new GuiSmokeAttemptPreflightResult(null, ShouldContinueLoop: true, phase, alternateBranchWait.Observer);
        }

        if (!IsPassiveWaitPhase(phase))
        {
            return new GuiSmokeAttemptPreflightResult(null, ShouldContinueLoop: false, phase, observer);
        }

        logger.WriteObserverCopies(stepPrefix, observer);
        var waitAttempt = IncrementAttempt(attemptsByPhase, phase);
        if (phase == GuiSmokePhase.WaitCharacterSelect
            && MatchesControlFlowScreen(observer, "main-menu")
            && waitAttempt >= 2
            && waitAttempt % 2 == 0)
        {
            LogHarness($"step={stepIndex} still on main-menu after continue; retrying enter-run attempt={waitAttempt}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "retry-enter-run", observer.CurrentScreen, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "retry-enter-run", observer.CurrentScreen, observer.InCombat, null));
            phase = GuiSmokePhase.EnterRun;
            var retryEnterRunWait = await WaitWithObserverPollingAsync(
                    observerReader,
                    transitionSettleMs,
                    75,
                    observer,
                    latestObserver => HasWaitWakeDelta(observer, latestObserver)
                        ? "observer-delta"
                        : evaluator.IsPhaseSatisfied(phase, latestObserver, history)
                            ? "phase-satisfied"
                            : null)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(retryEnterRunWait.WakeReason))
            {
                LogHarness($"step={stepIndex} retry enter-run settle woke early reason={retryEnterRunWait.WakeReason}");
            }

            return new GuiSmokeAttemptPreflightResult(null, ShouldContinueLoop: true, phase, retryEnterRunWait.Observer);
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
            return new GuiSmokeAttemptPreflightResult(
                completeAttempt(
                    1,
                    "failed",
                    $"timeout at {phase}",
                    "phase-timeout",
                    ClassifyFailureForAttempt(phase, observer, "phase-timeout", launchFailed: false)),
                ShouldContinueLoop: false,
                phase,
                observer);
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
        var passiveWait = await WaitWithObserverPollingAsync(
                observerReader,
                passiveWaitMs,
                75,
                observer,
                latestObserver => HasWaitWakeDelta(observer, latestObserver)
                    ? "observer-delta"
                    : evaluator.IsPhaseSatisfied(phase, latestObserver, history)
                        ? "phase-satisfied"
                        : null)
            .ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(passiveWait.WakeReason))
        {
            LogHarness($"step={stepIndex} passive wait woke early reason={passiveWait.WakeReason}");
        }

        return new GuiSmokeAttemptPreflightResult(null, ShouldContinueLoop: true, phase, passiveWait.Observer);
    }

    static GuiSmokeStepRequestBuildResult BuildAndLogStepRequest(
        int stepIndex,
        string stepPrefix,
        string runId,
        string scenarioId,
        GuiSmokePhase phase,
        string screenshotPath,
        WindowCaptureTarget window,
        ObserverState observer,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        string workspaceRoot,
        string sessionRoot,
        string attemptId,
        int attemptOrdinal,
        string captureMode,
        GuiSmokeStepAnalysisContext stepAnalysisContext,
        GuiSmokeSceneRequestContext sceneContext,
        ArtifactRecorder logger,
        Stopwatch stepStopwatch)
    {
        if (ControlFlowSceneReady(observer) == false)
        {
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "scene-not-ready", observer.CurrentScreen, observer.InCombat, null));
        }

        var sceneReasoningMode = stepAnalysisContext.HasScreenshotEvidence ? "enriched" : "observer-only";
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
            sceneContext);
        var requestPath = stepPrefix + ".request.json";
        logger.WriteRequest(requestPath, request);
        LogHarness($"step={stepIndex} request={requestPath} captureMode={captureMode} sceneReasoningMode={sceneReasoningMode}");
        return new GuiSmokeStepRequestBuildResult(request, sceneReasoningMode, stepStopwatch.ElapsedMilliseconds);
    }
}
