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
    static int IncrementAttempt(Dictionary<GuiSmokePhase, int> attemptsByPhase, GuiSmokePhase phase)
    {
        if (!attemptsByPhase.TryGetValue(phase, out var current))
        {
            current = 0;
        }

        current += 1;
        attemptsByPhase[phase] = current;
        return current;
    }

    static string ComputeFileFingerprint(string path)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("file-fingerprint", path, () =>
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var sha = SHA256.Create();
                var hash = sha.ComputeHash(stream);
                return Convert.ToHexString(hash.AsSpan(0, 8));
            }
            catch
            {
                return "no-image";
            }
        });
    }

    static void LogHarness(string message)
    {
        var line = $"[gui-smoke {DateTimeOffset.Now:HH:mm:ss}] {message}";
        Console.WriteLine(line);
        GuiSmokeShared.HarnessLogSink?.Invoke(line);
    }

    static string DescribeObserverHuman(ObserverState observer)
    {
        var logical = observer.CurrentScreen ?? "null";
        var visible = observer.VisibleScreen ?? "null";
        var inCombat = observer.InCombat?.ToString() ?? "null";
        var sceneReady = observer.SceneReady?.ToString() ?? "null";
        var sceneStability = observer.SceneStability ?? "null";
        var hp = observer.Summary.PlayerCurrentHp is not null && observer.Summary.PlayerMaxHp is not null
            ? $"{observer.Summary.PlayerCurrentHp}/{observer.Summary.PlayerMaxHp}"
            : "null";
        var energy = observer.PlayerEnergy?.ToString(CultureInfo.InvariantCulture) ?? "null";
        var hand = observer.CombatHand.Count == 0
            ? "-"
            : string.Join(", ", observer.CombatHand.Take(4).Select(card => $"{card.SlotIndex}:{card.Type ?? "?"}/{card.Cost?.ToString(CultureInfo.InvariantCulture) ?? "?"}"));
        var encounter = observer.EncounterKind ?? "null";
        var extractor = observer.ChoiceExtractorPath ?? "null";
        var choices = observer.Summary.CurrentChoices.Take(4).ToArray();
        var choiceText = choices.Length == 0 ? "-" : string.Join(", ", choices);
        return $"logical={logical} visible={visible} ready={sceneReady} stability={sceneStability} inCombat={inCombat} hp={hp} energy={energy} hand=[{hand}] encounter={encounter} extractor={extractor} choices=[{choiceText}]";
    }

    static void AppendProgressIfLongRun(bool isLongRun, ArtifactRecorder logger, GuiSmokeStepProgress progress)
    {
        if (!isLongRun)
        {
            return;
        }

        logger.AppendProgress(progress);
    }

    static GuiSmokeStepProgress EvaluateStepProgress(
        int stepIndex,
        GuiSmokePhase phase,
        string sceneSignature,
        ObserverState observer,
        ObserverState? postActionObserver,
        GuiSmokeStepDecision? decision,
        bool firstSeenScene,
        string reasoningMode,
        bool recipeRecorded,
        int sameActionStallCount,
        params string[] extraSignals)
    {
        var observerSignals = new List<string>();
        var suppressRoomSubstateHeuristics = ShouldSuppressRoomSubstateHeuristics(phase, observer);
        var canonicalScene = AutoDecisionProvider.TryBuildCanonicalNonCombatSceneState(observer, null);
        var suppressMapTransitionByForegroundAuthority = canonicalScene is
            {
                CanonicalForegroundOwner: not NonCombatCanonicalForegroundOwner.Unknown
                    and not NonCombatCanonicalForegroundOwner.Map,
            };
        if (observer.SceneReady is not null)
        {
            observerSignals.Add(observer.SceneReady == true ? "scene-ready-true" : "scene-ready-false");
        }

        if (!string.IsNullOrWhiteSpace(observer.SceneAuthority))
        {
            observerSignals.Add($"scene-authority:{observer.SceneAuthority}");
        }

        if (phase == GuiSmokePhase.HandleCombat)
        {
            if (observer.PlayerEnergy is not null)
            {
                observerSignals.Add("combat-energy");
            }

            if (observer.CombatHand.Count > 0)
            {
                observerSignals.Add("combat-hand");
            }
        }

        if (IsSpecificExtractorPath(observer.ChoiceExtractorPath))
        {
            observerSignals.Add($"choice-extractor:{observer.ChoiceExtractorPath}");
        }

        if (!suppressRoomSubstateHeuristics)
        {
            if (sceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase))
            {
                observerSignals.Add("map-background-visible");
            }

            if (sceneSignature.Contains("layer:map-overlay-foreground", StringComparison.OrdinalIgnoreCase))
            {
                observerSignals.Add("map-overlay-visible");
            }

            if (sceneSignature.Contains("map-back-navigation-available", StringComparison.OrdinalIgnoreCase))
            {
                observerSignals.Add("map-back-navigation-available");
            }

            if (sceneSignature.Contains("stale:event-choice", StringComparison.OrdinalIgnoreCase))
            {
                observerSignals.Add("stale-event-choice");
            }

            if (sceneSignature.Contains("current-node-arrow-visible", StringComparison.OrdinalIgnoreCase))
            {
                observerSignals.Add("current-node-arrow-visible");
            }

            if (sceneSignature.Contains("reachable-node-candidate-present", StringComparison.OrdinalIgnoreCase))
            {
                observerSignals.Add("reachable-node-candidate-present");
            }

            if (sceneSignature.Contains("exported-reachable-node-present", StringComparison.OrdinalIgnoreCase))
            {
                observerSignals.Add("exported-reachable-node-present");
            }

            if (sceneSignature.Contains("layer:reward-back-nav", StringComparison.OrdinalIgnoreCase))
            {
                observerSignals.Add("reward-back-navigation-available");
            }

            if (sceneSignature.Contains("stale:reward-choice", StringComparison.OrdinalIgnoreCase))
            {
                observerSignals.Add("stale-reward-choice");
            }

            if (sceneSignature.Contains("stale:reward-bounds", StringComparison.OrdinalIgnoreCase))
            {
                observerSignals.Add("stale-reward-bounds");
            }

            if (sceneSignature.Contains("reward:claimable", StringComparison.OrdinalIgnoreCase))
            {
                observerSignals.Add("claimable-reward-present");
            }

            if (LooksLikeInspectOverlayState(observer))
            {
                observerSignals.Add("inspect-overlay");
            }

            if (GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer))
            {
                observerSignals.Add("reward-choice");
            }

            if (GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer))
            {
                observerSignals.Add("colorless-card-choice");
            }

            if (GuiSmokeRewardSceneSignals.HasStrongRewardScreenAuthority(observer))
            {
                observerSignals.Add("reward-screen-authority");
            }

            if (GuiSmokeNonCombatContractSupport.HasExplicitRewardProgressionAffordance(observer.Summary))
            {
                observerSignals.Add("reward-explicit-progression");
            }

            if (GuiSmokeNonCombatContractSupport.HasStrongMapTransitionEvidence(observer)
                && !string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
                && !suppressMapTransitionByForegroundAuthority)
            {
                observerSignals.Add("map-transition-evidence");
            }
        }

        if (postActionObserver is not null && HasMeaningfulObserverDelta(observer, postActionObserver))
        {
            observerSignals.Add("post-action-delta");
        }

        foreach (var signal in extraSignals)
        {
            if (!string.IsNullOrWhiteSpace(signal))
            {
                observerSignals.Add(signal);
            }
        }

        var actuatorSignals = new List<string>();
        if (decision is not null && string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase))
        {
            actuatorSignals.Add($"action:{decision.ActionKind ?? "unknown"}");
            if (!string.IsNullOrWhiteSpace(decision.TargetLabel))
            {
                actuatorSignals.Add($"target:{decision.TargetLabel}");
            }

            if (GuiSmokeNonCombatContractSupport.IsRewardMapRecoveryTarget(decision.TargetLabel))
            {
                actuatorSignals.Add("map-node-candidate-chosen");
            }

            if (sameActionStallCount == 0)
            {
                actuatorSignals.Add("no-repeat-stall");
            }

            if (observer.SceneReady != false)
            {
                actuatorSignals.Add("scene-safe");
            }

            if (postActionObserver is not null && HasMeaningfulObserverDelta(observer, postActionObserver))
            {
                actuatorSignals.Add("post-action-delta");
                if (GuiSmokeNonCombatContractSupport.IsRewardMapRecoveryTarget(decision.TargetLabel))
                {
                    actuatorSignals.Add("post-click-recapture-observed");
                }
            }

            if (string.Equals(decision.TargetLabel, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase)
                && postActionObserver is not null
                && LooksLikeNonEnemyConfirmSuccess(observer, postActionObserver))
            {
                actuatorSignals.Add("non-enemy-confirmed");
            }
        }

        if (recipeRecorded)
        {
            actuatorSignals.Add("recipe-recorded");
        }

        var observerProgress = observerSignals.Any(static signal =>
            signal == "scene-ready-true"
            || signal == "combat-energy"
            || signal == "combat-hand"
            || signal == "post-action-delta"
            || signal.StartsWith("choice-extractor:", StringComparison.Ordinal));
        var actuatorProgress = actuatorSignals.Any(static signal =>
            signal == "post-action-delta"
            || signal == "non-enemy-confirmed"
            || signal == "recipe-recorded");
        return new GuiSmokeStepProgress(
            DateTimeOffset.UtcNow,
            stepIndex,
            phase.ToString(),
            sceneSignature,
            observer.CurrentScreen,
            postActionObserver?.CurrentScreen,
            decision?.TargetLabel,
            observerProgress,
            actuatorProgress,
            firstSeenScene,
            string.Equals(reasoningMode, "semantic", StringComparison.OrdinalIgnoreCase),
            recipeRecorded,
            observerSignals,
            actuatorSignals);
    }

    static bool ShouldGrantCombatNoOpProbeGrace(
        GuiSmokePhase phase,
        ObserverState before,
        ObserverState after,
        GuiSmokeStepDecision decision)
    {
        if (phase != GuiSmokePhase.HandleCombat
            || !string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase)
            || !AutoDecisionProvider.IsCombatNoOpSensitiveTarget(decision.TargetLabel))
        {
            return false;
        }

        return before.InCombat == true
               && after.InCombat == true
               && !HasMeaningfulObserverDelta(before, after)
               && string.Equals(after.CurrentScreen ?? after.VisibleScreen, "combat", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsSpecificExtractorPath(string? choiceExtractorPath)
    {
        if (string.IsNullOrWhiteSpace(choiceExtractorPath))
        {
            return false;
        }

        return !string.Equals(choiceExtractorPath, "generic", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(choiceExtractorPath, "unknown", StringComparison.OrdinalIgnoreCase);
    }

    static bool HasMeaningfulObserverDelta(ObserverState before, ObserverState after)
    {
        var beforeRuntime = CombatRuntimeStateSupport.Read(before.Summary, Array.Empty<CombatCardKnowledgeHint>());
        var afterRuntime = CombatRuntimeStateSupport.Read(after.Summary, Array.Empty<CombatCardKnowledgeHint>());
        return !string.Equals(before.CurrentScreen, after.CurrentScreen, StringComparison.OrdinalIgnoreCase)
               || !string.Equals(before.VisibleScreen, after.VisibleScreen, StringComparison.OrdinalIgnoreCase)
               || before.InCombat != after.InCombat
               || !string.Equals(before.Summary.SceneEpisodeId, after.Summary.SceneEpisodeId, StringComparison.Ordinal)
               || before.PlayerEnergy != after.PlayerEnergy
               || before.CombatHand.Count != after.CombatHand.Count
               || !string.Equals(before.InventoryId, after.InventoryId, StringComparison.Ordinal)
               || !string.Equals(beforeRuntime.InteractionRevision, afterRuntime.InteractionRevision, StringComparison.Ordinal)
               || beforeRuntime.HistoryStartedCount != afterRuntime.HistoryStartedCount
               || beforeRuntime.HistoryFinishedCount != afterRuntime.HistoryFinishedCount;
    }

    static bool TryRecordCombatNoOpObservation(
        GuiSmokePhase phase,
        ObserverState before,
        ObserverState after,
        GuiSmokeStepDecision decision,
        List<GuiSmokeHistoryEntry> history,
        ArtifactRecorder logger,
        int stepIndex,
        out string signal)
    {
        signal = string.Empty;
        if (phase != GuiSmokePhase.HandleCombat
            || !string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase)
            || !AutoDecisionProvider.IsCombatNoOpSensitiveTarget(decision.TargetLabel))
        {
            return false;
        }

        if (before.InCombat != true
            || after.InCombat != true
            || HasMeaningfulObserverDelta(before, after)
            || !string.Equals(after.CurrentScreen ?? after.VisibleScreen, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var laneLabel = AutoDecisionProvider.ResolveCombatLaneLabel(decision.TargetLabel, history) ?? decision.TargetLabel ?? "combat";
        history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "combat-noop", laneLabel, DateTimeOffset.UtcNow));
        logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "combat-noop", after.CurrentScreen, after.InCombat, laneLabel));
        signal = laneLabel.StartsWith("combat lane slot ", StringComparison.OrdinalIgnoreCase)
            ? $"combat-noop-observed:{laneLabel}"
            : "combat-noop-observed";
        LogHarness($"step={stepIndex} observed combat-noop target={decision.TargetLabel ?? "null"} lane={laneLabel} screen={after.CurrentScreen ?? after.VisibleScreen ?? "null"}");
        return true;
    }

    static bool LooksLikeNonEnemyConfirmSuccess(ObserverState before, ObserverState after)
    {
        var energySpent = before.PlayerEnergy is not null
                          && after.PlayerEnergy is not null
                          && after.PlayerEnergy < before.PlayerEnergy;
        var handCountDropped = after.CombatHand.Count < before.CombatHand.Count;
        var beforeRuntime = CombatRuntimeStateSupport.Read(before.Summary, Array.Empty<CombatCardKnowledgeHint>());
        var afterRuntime = CombatRuntimeStateSupport.Read(after.Summary, Array.Empty<CombatCardKnowledgeHint>());
        var finishedCardChanged = !string.IsNullOrWhiteSpace(afterRuntime.LastCardPlayFinishedCardId)
                                  && !string.Equals(beforeRuntime.LastCardPlayFinishedCardId, afterRuntime.LastCardPlayFinishedCardId, StringComparison.OrdinalIgnoreCase);
        var pendingClearedWithMeaningfulDelta = beforeRuntime.PendingSelection?.Kind == AutoCombatCardKind.DefendLike
                                                && afterRuntime.PendingSelection is null
                                                && beforeRuntime.CardPlayPending == true
                                                && afterRuntime.CardPlayPending == false
                                                && (energySpent || handCountDropped || finishedCardChanged);
        return energySpent
               || handCountDropped
               || finishedCardChanged
               || pendingClearedWithMeaningfulDelta;
    }

    static string DescribeWindow(WindowCaptureTarget target)
    {
        return $"{target.Title} fallback={target.IsFallback} minimized={target.IsMinimized} bounds={DescribeBounds(target.Bounds)}";
    }

    static string DescribeBounds(Rectangle bounds)
    {
        return $"{bounds.X},{bounds.Y},{bounds.Width},{bounds.Height}";
    }

    static void ValidateDecision(GuiSmokePhase phase, GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        if (string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase))
        {
            if (!GuiSmokeNonCombatContractSupport.AllowsAction(request, "wait"))
            {
                throw new InvalidOperationException($"Phase {phase} does not allow wait decisions.");
            }

            if (decision.ActionKind is not null
                || decision.KeyText is not null
                || decision.NormalizedX is not null
                || decision.NormalizedY is not null)
            {
                throw new InvalidOperationException("Wait decision must not provide an action kind, key, or coordinates.");
            }

            return;
        }

        if (!string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only act or wait decisions are valid here.");
        }

        if (string.Equals(decision.ActionKind, "click", StringComparison.OrdinalIgnoreCase))
        {
            if (decision.NormalizedX is null || decision.NormalizedY is null)
            {
                throw new InvalidOperationException("Click decision requires normalized coordinates.");
            }

            if (decision.NormalizedX < 0 || decision.NormalizedX > 1 || decision.NormalizedY < 0 || decision.NormalizedY > 1)
            {
                throw new InvalidOperationException("Normalized coordinates must be within [0,1].");
            }
        }
        else if (string.Equals(decision.ActionKind, "click-current", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(decision.ActionKind, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase))
        {
            if (decision.NormalizedX is not null || decision.NormalizedY is not null)
            {
                throw new InvalidOperationException($"{decision.ActionKind} decision must not provide normalized coordinates.");
            }
        }
        else if (string.Equals(decision.ActionKind, "right-click", StringComparison.OrdinalIgnoreCase))
        {
            if ((decision.NormalizedX is null) != (decision.NormalizedY is null))
            {
                throw new InvalidOperationException("right-click decision must provide both normalized coordinates or neither.");
            }

            if (decision.NormalizedX is not null
                && (decision.NormalizedX < 0 || decision.NormalizedX > 1 || decision.NormalizedY < 0 || decision.NormalizedY > 1))
            {
                throw new InvalidOperationException("Normalized coordinates must be within [0,1].");
            }
        }
        else if (string.Equals(decision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(decision.KeyText))
            {
                throw new InvalidOperationException("press-key decision requires keyText.");
            }
        }
        else
        {
            throw new InvalidOperationException("Only click, click-current, confirm-non-enemy, right-click, and press-key actionKind are supported.");
        }

        if (request.AllowedActions.Length == 1 && string.Equals(request.AllowedActions[0], "wait", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Phase {phase} does not allow actions.");
        }

        if (phase == GuiSmokePhase.HandleCombat)
        {
            if (!CombatDecisionContract.IsAllowed(request, decision, out _))
            {
                throw new InvalidOperationException($"Combat decision '{decision.TargetLabel ?? decision.ActionKind ?? "unknown"}' is not allowed by request contract.");
            }

            return;
        }

        if (GuiSmokeNonCombatContractSupport.TryMapNonCombatAllowedAction(decision, out var allowedAction)
            && !GuiSmokeNonCombatContractSupport.AllowsAction(request, allowedAction))
        {
            throw new InvalidOperationException($"Decision '{decision.TargetLabel ?? decision.ActionKind ?? "unknown"}' maps to disallowed action '{allowedAction}'.");
        }
    }

    static void EnsureGameNotRunning()
    {
        if (WindowLocator.TryFindSts2Window() is not null)
        {
            throw new InvalidOperationException("Slay the Spire 2 appears to be running. Close the game before deploy/launch.");
        }
    }
}
