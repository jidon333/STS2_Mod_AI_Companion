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
using static GuiSmokePostActionPhaseSupport;

internal static partial class Program
{
    sealed record GuiSmokePostActionResult(GuiSmokePhase Phase, ObserverState PostActionObserver);

    static void ExecuteDecisionActuation(
        int stepIndex,
        GuiSmokePhase phase,
        ObserverState observer,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        WindowCaptureTarget clickWindow,
        MouseInputDriver inputDriver,
        List<GuiSmokeHistoryEntry> history,
        ArtifactRecorder logger)
    {
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
            return;
        }

        if (string.Equals(decision.ActionKind, "right-click", StringComparison.OrdinalIgnoreCase))
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
            return;
        }

        if (string.Equals(decision.ActionKind, "click-current", StringComparison.OrdinalIgnoreCase))
        {
            LogHarness($"step={stepIndex} click-current target={decision.TargetLabel ?? "null"}");
            inputDriver.ClickCurrent(clickWindow);
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "click-current", decision.TargetLabel, DateTimeOffset.UtcNow)
            {
                Metadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(request, decision),
            });
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "click-current", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
            LogHarness($"step={stepIndex} click-current sent target={decision.TargetLabel ?? "null"}");
            return;
        }

        if (string.Equals(decision.ActionKind, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase))
        {
            LogHarness($"step={stepIndex} confirm-non-enemy target={decision.TargetLabel ?? "null"} normalized=({GuiSmokeCombatConstants.NonEnemyConfirmNormalizedX:0.000},{GuiSmokeCombatConstants.NonEnemyConfirmNormalizedY:0.000}) holdMs={GuiSmokeCombatConstants.NonEnemyConfirmHoldMs}");
            inputDriver.ConfirmNonEnemy(clickWindow, GuiSmokeCombatConstants.NonEnemyConfirmNormalizedX, GuiSmokeCombatConstants.NonEnemyConfirmNormalizedY, GuiSmokeCombatConstants.NonEnemyConfirmHoldMs);
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "confirm-non-enemy", decision.TargetLabel, DateTimeOffset.UtcNow)
            {
                Metadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(request, decision),
            });
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "confirm-non-enemy", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
            LogHarness($"step={stepIndex} confirm-non-enemy sent target={decision.TargetLabel ?? "null"}");
            return;
        }

        var genericClickPoint = MouseInputDriver.TransformNormalizedPoint(clickWindow, decision.NormalizedX!.Value, decision.NormalizedY!.Value);
        if (ShouldUseHoverPrimedClick(decision))
        {
            LogHarness($"step={stepIndex} click target={decision.TargetLabel ?? "null"} hoverPrime=true normalized=({decision.NormalizedX:0.000},{decision.NormalizedY:0.000}) absolute=({genericClickPoint.X},{genericClickPoint.Y}) bounds={DescribeBounds(clickWindow.Bounds)}");
            inputDriver.HoverPrimedClick(clickWindow, decision.NormalizedX!.Value, decision.NormalizedY!.Value);
        }
        else
        {
            LogHarness($"step={stepIndex} click target={decision.TargetLabel ?? "null"} normalized=({decision.NormalizedX:0.000},{decision.NormalizedY:0.000}) absolute=({genericClickPoint.X},{genericClickPoint.Y}) bounds={DescribeBounds(clickWindow.Bounds)}");
            inputDriver.Click(clickWindow, decision.NormalizedX!.Value, decision.NormalizedY!.Value);
        }

        history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "click", decision.TargetLabel, DateTimeOffset.UtcNow)
        {
            Metadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(request, decision),
        });
        logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "click", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
        LogHarness($"step={stepIndex} click sent target={decision.TargetLabel ?? "null"}");
    }

    static async Task<GuiSmokePostActionResult> FinalizePostActionAndAdvancePhaseAsync(
        int stepIndex,
        GuiSmokePhase phase,
        ObserverState observer,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        bool isLongRun,
        string sessionRoot,
        GuiSmokeSessionSceneHistoryIndex sceneHistoryIndex,
        ArtifactRecorder logger,
        ObserverSnapshotReader observerReader,
        ObserverAcceptanceEvaluator evaluator,
        List<GuiSmokeHistoryEntry> history,
        Stopwatch stepStopwatch,
        long decisionReadyElapsedMs,
        int actionSettleMinimumMs,
        int combatActionSettleMinimumMs,
        int combatNoOpProbeGraceMs,
        int sameActionStallCount,
        bool rewardMapRecoveryAttempt)
    {
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

        var settleDelayMs = GetActionSettleDelayMs(completedPhase, decision, actionSettleMinimumMs, combatActionSettleMinimumMs);
        var progressProbeDelayMs = Math.Min(settleDelayMs, completedPhase == GuiSmokePhase.HandleCombat ? 80 : 180);
        ObserverState? probedPostActionObserver = null;
        if (progressProbeDelayMs > 0)
        {
            var progressProbeWait = await WaitWithObserverPollingAsync(
                    observerReader,
                    progressProbeDelayMs,
                    75,
                    observer,
                    latestObserver => HasWaitWakeDelta(observer, latestObserver) ? "observer-delta" : null)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(progressProbeWait.WakeReason))
            {
                probedPostActionObserver = progressProbeWait.Observer;
                LogHarness($"step={stepIndex} progress probe woke early reason={progressProbeWait.WakeReason}");
            }
        }

        var extraProgressSignals = new List<string>();
        if (rewardMapRecoveryAttempt)
        {
            extraProgressSignals.Add("reward-map-recovery-attempt");
        }

        var postActionObserver = probedPostActionObserver ?? observerReader.Read();
        var additionalProbeDelayMs = 0;
        if (ShouldGrantCombatNoOpProbeGrace(completedPhase, observer, postActionObserver, decision))
        {
            additionalProbeDelayMs = combatNoOpProbeGraceMs;
            extraProgressSignals.Add("combat-noop-probe-grace");
            LogHarness($"step={stepIndex} combat noop probe grace target={decision.TargetLabel ?? "null"} delayMs={additionalProbeDelayMs}");
            var noOpGraceWait = await WaitWithObserverPollingAsync(
                    observerReader,
                    additionalProbeDelayMs,
                    75,
                    postActionObserver,
                    latestObserver => !ShouldGrantCombatNoOpProbeGrace(completedPhase, observer, latestObserver, decision)
                        ? "observer-delta"
                        : null)
                .ConfigureAwait(false);
            postActionObserver = noOpGraceWait.Observer;
            if (!string.IsNullOrWhiteSpace(noOpGraceWait.WakeReason))
            {
                LogHarness($"step={stepIndex} combat noop probe grace woke early reason={noOpGraceWait.WakeReason}");
            }
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

        LogHarness($"step={stepIndex} next phase={phase}");
        var remainingSettleDelayMs = Math.Max(0, settleDelayMs - progressProbeDelayMs - additionalProbeDelayMs);
        if (remainingSettleDelayMs > 0)
        {
            var trailingSettleWait = await WaitWithObserverPollingAsync(
                    observerReader,
                    remainingSettleDelayMs,
                    75,
                    postActionObserver,
                    latestObserver => HasWaitWakeDelta(postActionObserver, latestObserver)
                        ? "observer-delta"
                        : evaluator.IsPhaseSatisfied(phase, latestObserver, history)
                            ? "phase-satisfied"
                            : null)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(trailingSettleWait.WakeReason))
            {
                LogHarness($"step={stepIndex} trailing settle woke early reason={trailingSettleWait.WakeReason}");
                postActionObserver = trailingSettleWait.Observer;
            }
        }

        return new GuiSmokePostActionResult(phase, postActionObserver);
    }
}
