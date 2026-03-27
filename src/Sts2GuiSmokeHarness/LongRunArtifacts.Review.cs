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

static partial class LongRunArtifacts
{
    public static GuiSmokeSelfMetaReview WriteAttemptMetaReview(
        string sessionRoot,
        string attemptId,
        int attemptOrdinal,
        string runId,
        string status,
        string? resultMessage,
        string? explicitFailureClass = null)
    {
        var currentProgress = ReadNdjsonRecords<GuiSmokeStepProgress>(Path.Combine(sessionRoot, "attempts", attemptId, "progress.ndjson"));
        var previousReviews = ReadNdjsonRecords<GuiSmokeSelfMetaReview>(Path.Combine(sessionRoot, "meta-reviews.ndjson"));
        var observerCoverageRatio = currentProgress.Count == 0
            ? 0d
            : currentProgress.Count(static entry => entry.ObserverProgress) / (double)currentProgress.Count;
        var actuatorSteps = currentProgress.Count(static entry => entry.ActuatorSignals.Count > 0);
        var actuatorSuccessRatio = actuatorSteps == 0
            ? 0d
            : currentProgress.Count(static entry => entry.ActuatorProgress) / (double)actuatorSteps;
        var novelSceneCount = currentProgress.Count(static entry => entry.FirstSeenScene);
        var sameActionStallCount = currentProgress.Count(entry => entry.ObserverSignals.Contains("same-action-stall", StringComparer.OrdinalIgnoreCase));
        var plateauDetected = DetectProgressPlateau(previousReviews, observerCoverageRatio, actuatorSuccessRatio, novelSceneCount, sameActionStallCount);
        var dominantFailureClass = DetermineDominantFailureClass(currentProgress, status, resultMessage, explicitFailureClass);
        var evidence = BuildReviewEvidence(currentProgress, observerCoverageRatio, actuatorSuccessRatio, novelSceneCount, sameActionStallCount, status, resultMessage);
        var directionRisk = plateauDetected
            ? $"direction-risk:{dominantFailureClass}"
            : "direction-stable";
        var nextAttemptAdjustments = BuildNextAttemptAdjustments(dominantFailureClass, plateauDetected);
        var review = new GuiSmokeSelfMetaReview(
            DateTimeOffset.UtcNow,
            attemptId,
            attemptOrdinal,
            runId,
            status,
            resultMessage,
            plateauDetected,
            dominantFailureClass,
            directionRisk,
            evidence,
            nextAttemptAdjustments,
            observerCoverageRatio,
            actuatorSuccessRatio,
            novelSceneCount,
            sameActionStallCount,
            currentProgress.Count);
        AppendNdjson(Path.Combine(sessionRoot, "meta-reviews.ndjson"), review);
        return review;
    }

    public static void WriteSessionArtifacts(
        string sessionRoot,
        ArtifactRecorder logger,
        string runId,
        string scenarioId,
        string providerKind,
        string attemptId,
        int attemptOrdinal,
        int stepCount,
        string status,
        string resultMessage,
        string? terminalCause,
        bool launchFailed,
        string? failureClass,
        string trustStateAtStart)
    {
        var runManifestPath = Path.Combine(logger.RunRoot, "run.json");
        var runManifest = File.Exists(runManifestPath)
            ? JsonSerializer.Deserialize<GuiSmokeRunManifest>(File.ReadAllText(runManifestPath), GuiSmokeShared.JsonOptions)
            : null;
        var attemptEntry = new GuiSmokeAttemptIndexEntry(
            attemptId,
            attemptOrdinal,
            runId,
            status,
            resultMessage,
            runManifest?.StartedAt ?? DateTimeOffset.UtcNow,
            runManifest?.CompletedAt ?? DateTimeOffset.UtcNow,
            stepCount,
            terminalCause,
            launchFailed,
            failureClass,
            trustStateAtStart);

        var attemptIndexPath = Path.Combine(sessionRoot, "attempt-index.ndjson");
        AppendUniqueNdjson(attemptIndexPath, attemptEntry, static existing => existing.RunId, attemptEntry.RunId);
        RefreshSessionSummary(sessionRoot);
        RefreshStallSentinel(sessionRoot);
        RefreshSupervisorState(sessionRoot);
    }

    public static void AppendSceneRecipe(string sessionRoot, GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        if (string.IsNullOrWhiteSpace(sessionRoot)
            || string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase)
            || string.Equals(decision.Status, "abort", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(decision.ActionKind))
        {
            return;
        }

        var recipe = new SceneRecipeEntry(
            DateTimeOffset.UtcNow,
            request.SceneSignature,
            request.Phase,
            decision.ActionKind,
            decision.TargetLabel,
            decision.ExpectedScreen,
            decision.Reason,
            request.ScreenshotPath);
        AppendNdjson(Path.Combine(sessionRoot, "scene-recipes.ndjson"), recipe);
    }

    public static void MaybeRecordUnknownScene(string sessionRoot, GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        var observerScreenUnknown = string.Equals(request.Observer.CurrentScreen, "unknown", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(request.Observer.VisibleScreen, "unknown", StringComparison.OrdinalIgnoreCase);
        if (!observerScreenUnknown)
        {
            return;
        }

        var entry = new UnknownSceneEntry(
            DateTimeOffset.UtcNow,
            request.SceneSignature,
            request.Phase,
            request.ScreenshotPath,
            request.Observer.CurrentScreen,
            request.Observer.VisibleScreen,
            decision.AbortReason ?? decision.Reason);
        AppendUniqueNdjson(Path.Combine(sessionRoot, "unknown-scenes.ndjson"), entry, static existing => existing.SceneSignature, entry.SceneSignature);
    }

    private static void AppendNdjson<T>(string path, T entry)
    {
        LiveExportAtomicFileWriter.AppendAllTextShared(
            path,
            JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions) + Environment.NewLine);
    }

    private static void AppendUniqueNdjson<T>(string path, T entry, Func<T, string> keySelector, string key)
    {
        if (File.Exists(path))
        {
            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                T? existing;
                try
                {
                    existing = JsonSerializer.Deserialize<T>(line, GuiSmokeShared.JsonOptions);
                }
                catch (JsonException)
                {
                    continue;
                }

                if (existing is not null && string.Equals(keySelector(existing), key, StringComparison.Ordinal))
                {
                    return;
                }
            }
        }

        AppendNdjson(path, entry);
    }

    private static bool DetectProgressPlateau(
        IReadOnlyList<GuiSmokeSelfMetaReview> previousReviews,
        double observerCoverageRatio,
        double actuatorSuccessRatio,
        int novelSceneCount,
        int sameActionStallCount)
    {
        var window = previousReviews.TakeLast(2).ToArray();
        if (window.Length < 2)
        {
            return false;
        }

        var bestObserverCoverage = window.Max(static review => review.ObserverCoverageRatio);
        var bestActuatorSuccess = window.Max(static review => review.ActuatorSuccessRatio);
        var bestNovelSceneCount = window.Max(static review => review.NovelSceneCount);
        var lowestSameActionStallCount = window.Min(static review => review.SameActionStallCount);
        return observerCoverageRatio <= bestObserverCoverage + 0.01
               && actuatorSuccessRatio <= bestActuatorSuccess + 0.01
               && novelSceneCount <= bestNovelSceneCount
               && sameActionStallCount >= lowestSameActionStallCount;
    }

    private static string DetermineDominantFailureClass(
        IReadOnlyList<GuiSmokeStepProgress> progressEntries,
        string status,
        string? resultMessage,
        string? explicitFailureClass = null)
    {
        if (!string.IsNullOrWhiteSpace(explicitFailureClass))
        {
            return explicitFailureClass;
        }

        if (resultMessage?.Contains("launch", StringComparison.OrdinalIgnoreCase) == true
            || progressEntries.Any(entry => entry.ObserverSignals.Contains("black-frame-nudge", StringComparer.OrdinalIgnoreCase)))
        {
            return "launch-runtime-noise";
        }

        var observerBlindspots = progressEntries.Count(entry =>
            entry.Phase == GuiSmokePhase.HandleCombat.ToString()
            && (!entry.ObserverSignals.Contains("combat-energy", StringComparer.OrdinalIgnoreCase)
                || !entry.ObserverSignals.Contains("combat-hand", StringComparer.OrdinalIgnoreCase)));
        var actuatorConfirmFailures = progressEntries.Count(entry =>
            entry.DecisionTargetLabel is not null
            && entry.DecisionTargetLabel.Contains("confirm", StringComparison.OrdinalIgnoreCase)
            && !entry.ActuatorSignals.Contains("non-enemy-confirmed", StringComparer.OrdinalIgnoreCase)
            && !entry.ActuatorSignals.Contains("post-action-delta", StringComparer.OrdinalIgnoreCase));
        var semanticAmbiguity = progressEntries.Count(static entry => entry.SemanticReasoningActive && entry.FirstSeenScene && !entry.ActuatorProgress);
        var screenshotDrift = progressEntries.Count(entry => entry.ObserverSignals.Contains("same-action-stall", StringComparer.OrdinalIgnoreCase));
        if (observerBlindspots >= actuatorConfirmFailures && observerBlindspots >= semanticAmbiguity && observerBlindspots >= screenshotDrift)
        {
            return "observer-blindspot";
        }

        if (actuatorConfirmFailures >= semanticAmbiguity && actuatorConfirmFailures >= screenshotDrift)
        {
            return "actuator-confirm-failure";
        }

        if (semanticAmbiguity >= screenshotDrift)
        {
            return "semantic-scene-ambiguity";
        }

        if (string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase))
        {
            return "screenshot-heuristic-drift";
        }

        return "observer-blindspot";
    }

    private static IReadOnlyList<string> BuildReviewEvidence(
        IReadOnlyList<GuiSmokeStepProgress> progressEntries,
        double observerCoverageRatio,
        double actuatorSuccessRatio,
        int novelSceneCount,
        int sameActionStallCount,
        string status,
        string? resultMessage)
    {
        return new[]
        {
            $"observer-coverage:{observerCoverageRatio:0.000}",
            $"actuator-success:{actuatorSuccessRatio:0.000}",
            $"novel-scenes:{novelSceneCount}",
            $"same-action-stalls:{sameActionStallCount}",
            $"status:{status}",
            $"result:{resultMessage ?? "null"}",
            $"semantic-steps:{progressEntries.Count(static entry => entry.SemanticReasoningActive)}",
        };
    }

    private static IReadOnlyList<string> BuildNextAttemptAdjustments(string dominantFailureClass, bool plateauDetected)
    {
        var adjustments = new List<string>();
        if (plateauDetected)
        {
            adjustments.Add("run self-meta review before trusting the next direction change");
        }

        switch (dominantFailureClass)
        {
            case "observer-blindspot":
                adjustments.Add("prioritize decompiled-source observer candidates for missing combat or room state");
                adjustments.Add("treat screenshot as authority while expanding observer coverage");
                break;
            case "actuator-confirm-failure":
                adjustments.Add("tighten non-enemy confirm success checks using hand or energy delta");
                adjustments.Add("prefer low-risk repeatable confirmations over aggressive extra clicks");
                break;
            case "semantic-scene-ambiguity":
                adjustments.Add("expand semantic event candidates from assistant/events.json and observed choices");
                adjustments.Add("prefer safe progress options over speculative rare outcomes");
                break;
            case "launch-runtime-noise":
                adjustments.Add("re-verify focus and deployment identity before debugging gameplay behavior");
                break;
            default:
                adjustments.Add("review screenshot-first heuristics for drift and repeated stalls");
                break;
        }

        return adjustments;
    }
}
