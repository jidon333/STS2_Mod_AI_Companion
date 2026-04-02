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
using static GuiSmokeReplayArtifactSupport;

internal static partial class Program
{
    sealed record GuiSmokeDecisionResolutionResult(
        GuiSmokeStepDecision Decision,
        GuiSmokeReplayEvaluation ReplayEvaluation,
        long DecisionReadyElapsedMs);

    sealed record GuiSmokeStepDecisionCycleResult(
        GuiSmokePhase Phase,
        GuiSmokeAttemptResult? CompletedAttempt,
        bool ShouldContinueLoop);

    static async Task<GuiSmokeDecisionResolutionResult> ResolveStepDecisionAsync(
        int stepIndex,
        GuiSmokePhase phase,
        IGuiDecisionProvider provider,
        GuiSmokeStepRequest request,
        string requestPath,
        string decisionPath,
        string candidateDumpPath,
        string captureMode,
        string sceneReasoningMode,
        GuiSmokeStepAnalysisContext stepAnalysisContext,
        ArtifactRecorder logger,
        Stopwatch stepStopwatch,
        long requestReadyElapsedMs)
    {
        GuiSmokeReplayEvaluation replayEvaluation;
        GuiSmokeStepDecision decision;
        var activeCombatBarrier = phase == GuiSmokePhase.HandleCombat
            ? stepAnalysisContext.CombatBarrierEvaluation
            : CombatBarrierSupport.Inactive;
        if (phase == GuiSmokePhase.HandleCombat
            && activeCombatBarrier.IsActive
            && activeCombatBarrier.IsHardWaitBarrier
            && stepAnalysisContext.CombatReleaseState.LifecycleStage is not CombatLifecycleStage.CombatEntryPending
                and not CombatLifecycleStage.EndTurnTransit
                and not CombatLifecycleStage.EnemyTurn
                and not CombatLifecycleStage.PlayerReopenPending
                and not CombatLifecycleStage.Inactive)
        {
            decision = AutoDecisionProvider.CreateCombatBarrierWaitDecision(activeCombatBarrier, stepAnalysisContext.CombatReleaseState, request.Observer.CurrentScreen);
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
            WriteCandidateDumpArtifact(candidateDumpPath, replayEvaluation.CandidateDump);
        }

        logger.WriteDecision(decisionPath, decision);
        LogHarness($"step={stepIndex} decision status={decision.Status} action={decision.ActionKind ?? "null"} target={decision.TargetLabel ?? "null"} confidence={decision.Confidence?.ToString("0.00") ?? "null"} reason={decision.Reason ?? "null"}");
        LogHarness($"step={stepIndex} timing preflight->request={requestReadyElapsedMs}ms request->decision={Math.Max(0, decisionReadyElapsedMs - requestReadyElapsedMs)}ms captureMode={captureMode} sceneReasoningMode={sceneReasoningMode}");
        return new GuiSmokeDecisionResolutionResult(decision, replayEvaluation, decisionReadyElapsedMs);
    }

    static async Task<GuiSmokeStepDecisionCycleResult> ExecuteStepDecisionCycleAsync(
        int stepIndex,
        GuiSmokePhase phase,
        string screenshotPath,
        WindowCaptureTarget window,
        ObserverState observer,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        GuiSmokeStepAnalysisContext stepAnalysisContext,
        bool isLongRun,
        string sessionRoot,
        GuiSmokeSessionSceneHistoryIndex sceneHistoryIndex,
        ArtifactRecorder logger,
        ObserverSnapshotReader observerReader,
        ObserverAcceptanceEvaluator evaluator,
        List<GuiSmokeHistoryEntry> history,
        Stopwatch stepStopwatch,
        long decisionReadyElapsedMs,
        GuiSmokeAttemptLoopTrackingState loopTracking,
        MouseInputDriver inputDriver,
        TimeSpan decisionStaleBudget,
        bool useDecisionAgeGuard,
        int transitionSettleMs,
        int actionSettleMinimumMs,
        int combatActionSettleMinimumMs,
        Func<int, string, string, string?, string?, GuiSmokeAttemptResult> completeAttempt)
    {
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
                completeAttempt)
            .ConfigureAwait(false);
        if (decisionStatus.CompletedAttempt is not null)
        {
            return new GuiSmokeStepDecisionCycleResult(phase, decisionStatus.CompletedAttempt, false);
        }

        if (decisionStatus.ShouldContinueLoop)
        {
            return new GuiSmokeStepDecisionCycleResult(phase, null, true);
        }

        var actuationPreparation = await TryPrepareDecisionActuationAsync(
                stepIndex,
                screenshotPath,
                phase,
                window,
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
                loopTracking,
                completeAttempt,
                transitionSettleMs,
                decisionStaleBudget,
                useDecisionAgeGuard)
            .ConfigureAwait(false);
        if (actuationPreparation.CompletedAttempt is not null)
        {
            return new GuiSmokeStepDecisionCycleResult(phase, actuationPreparation.CompletedAttempt, false);
        }

        if (actuationPreparation.ShouldContinueLoop)
        {
            return new GuiSmokeStepDecisionCycleResult(phase, null, true);
        }

        var clickWindow = actuationPreparation.ClickWindow ?? window;
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
                actionSettleMinimumMs,
                combatActionSettleMinimumMs,
                loopTracking.SameActionStallCount,
                actuationPreparation.RewardMapRecoveryAttempt)
            .ConfigureAwait(false);

        return new GuiSmokeStepDecisionCycleResult(postAction.Phase, null, false);
    }
}
