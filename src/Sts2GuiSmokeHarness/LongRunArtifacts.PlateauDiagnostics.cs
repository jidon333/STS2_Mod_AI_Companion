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
    private static void WriteStallDiagnosis(string sessionRoot, string runRoot, GuiSmokeAttemptIndexEntry attemptEntry)
    {
        var failureSummary = TryReadJson<GuiSmokeFailureSummary>(Path.Combine(runRoot, "failure-summary.json"));
        var selfMetaReview = TryReadJson<GuiSmokeSelfMetaReview>(Path.Combine(runRoot, "self-meta-review.json"));
        var progress = ReadNdjsonRecords<GuiSmokeStepProgress>(Path.Combine(runRoot, "progress.ndjson"));
        var latestStepContext = LoadLatestStepContext(runRoot);
        var latestProgress = progress.LastOrDefault();
        var sameActionStallCount = progress.Count(entry => entry.ObserverSignals.Contains("same-action-stall", StringComparer.OrdinalIgnoreCase));
        var decisionWaitPlateau = AnalyzeDecisionWaitPlateau(progress);
        var inspectOverlayLoop = AnalyzeInspectOverlayLoop(progress);
        var rewardMapLoop = AnalyzeRewardMapLoop(progress);
        var mapOverlayNoOpLoop = AnalyzeMapOverlayNoOpLoop(progress, latestStepContext);
        var mapTransitionStall = AnalyzeMapTransitionStall(progress);
        var combatNoOpLoop = AnalyzeCombatNoOpLoop(progress);
        var latestPhase = failureSummary?.Phase ?? latestStepContext?.Phase ?? latestProgress?.Phase;
        var latestObserverScreen = failureSummary?.ObserverScreen ?? latestStepContext?.ObserverScreen ?? latestProgress?.PostActionScreen ?? latestProgress?.ObserverScreen;
        var diagnosisKind = DetermineDiagnosisKind(attemptEntry, failureSummary, sameActionStallCount, decisionWaitPlateau, inspectOverlayLoop, rewardMapLoop, mapOverlayNoOpLoop, mapTransitionStall, combatNoOpLoop, latestPhase, latestObserverScreen);
        var useCombatAnalysis = string.Equals(diagnosisKind, "combat-noop-loop", StringComparison.OrdinalIgnoreCase);
        var useRewardAnalysis = string.Equals(diagnosisKind, "reward-map-loop", StringComparison.OrdinalIgnoreCase);
        var useMapOverlayAnalysis = string.Equals(diagnosisKind, "map-overlay-noop-loop", StringComparison.OrdinalIgnoreCase);
        var useMapTransitionAnalysis = string.Equals(diagnosisKind, "map-transition-stall", StringComparison.OrdinalIgnoreCase);
        var useOverlayAnalysis = string.Equals(diagnosisKind, "inspect-overlay-loop", StringComparison.OrdinalIgnoreCase);
        var useWaitAnalysis = string.Equals(diagnosisKind, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(diagnosisKind, "combat-entry-surface-pending-wait-plateau", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(diagnosisKind, "combat-enemy-click-resolution-pending-wait-plateau", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(diagnosisKind, "combat-lifecycle-transit-wait-plateau", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(diagnosisKind, "combat-barrier-wait-plateau", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(diagnosisKind, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase);
        var phase = failureSummary?.Phase
                    ?? (useCombatAnalysis ? combatNoOpLoop.Phase : null)
                    ?? (useRewardAnalysis ? rewardMapLoop.Phase : null)
                    ?? (useMapOverlayAnalysis ? mapOverlayNoOpLoop.Phase : null)
                    ?? (useMapTransitionAnalysis ? mapTransitionStall.Phase : null)
                    ?? (useOverlayAnalysis ? inspectOverlayLoop.Phase : null)
                    ?? (useWaitAnalysis ? decisionWaitPlateau.Phase : null)
                    ?? latestProgress?.Phase;
        var observerScreen = failureSummary?.ObserverScreen
                             ?? (useCombatAnalysis ? combatNoOpLoop.ObserverScreen : null)
                             ?? (useRewardAnalysis ? rewardMapLoop.ObserverScreen : null)
                             ?? (useMapOverlayAnalysis ? mapOverlayNoOpLoop.ObserverScreen : null)
                             ?? (useMapTransitionAnalysis ? mapTransitionStall.ObserverScreen : null)
                             ?? (useOverlayAnalysis ? inspectOverlayLoop.ObserverScreen : null)
                             ?? (useWaitAnalysis ? decisionWaitPlateau.ObserverScreen : null)
                             ?? latestProgress?.PostActionScreen
                             ?? latestProgress?.ObserverScreen;
        var screenshotPath = failureSummary?.ScreenshotPath ?? latestStepContext?.ScreenshotPath ?? FindLatestScreenshotPath(runRoot);
        var backlogRoute = ShouldRouteToDecompilerBacklog(diagnosisKind, phase, observerScreen)
            ? "decompiled-source-first-observer"
            : null;
        var evidence = new List<string>
        {
            $"status:{attemptEntry.Status}",
            $"terminalCause:{attemptEntry.TerminalCause ?? "null"}",
            $"failureClass:{attemptEntry.FailureClass ?? "null"}",
            $"sameActionStalls:{sameActionStallCount}",
            $"repeatedDecisionWaits:{decisionWaitPlateau.RepeatedWaitCount}",
            $"overlayLoopCount:{inspectOverlayLoop.OverlayCloseCount}",
            $"rewardMapLoopCount:{rewardMapLoop.RepeatedLoopCount}",
            $"mapOverlayLoopCount:{mapOverlayNoOpLoop.RepeatedLoopCount}",
            $"mapTransitionLoopCount:{mapTransitionStall.RepeatedLoopCount}",
            $"combatNoOpLoopCount:{combatNoOpLoop.RepeatedLoopCount}",
            $"trustStateAtStart:{attemptEntry.TrustStateAtStart}",
        };
        if (!string.IsNullOrWhiteSpace(phase))
        {
            evidence.Add($"phase:{phase}");
        }

        if (!string.IsNullOrWhiteSpace(decisionWaitPlateau.SceneSignature))
        {
            evidence.Add($"waitSignature:{decisionWaitPlateau.SceneSignature}");
        }

        if (!string.IsNullOrWhiteSpace(inspectOverlayLoop.SceneSignature))
        {
            evidence.Add($"overlayLoopSignature:{inspectOverlayLoop.SceneSignature}");
        }

        if (!string.IsNullOrWhiteSpace(inspectOverlayLoop.LastMisdirectedTarget))
        {
            evidence.Add($"overlayLoopMisdirectedTarget:{inspectOverlayLoop.LastMisdirectedTarget}");
        }

        if (!string.IsNullOrWhiteSpace(mapOverlayNoOpLoop.LastLoopTarget))
        {
            evidence.Add($"mapOverlayLoopTarget:{mapOverlayNoOpLoop.LastLoopTarget}");
        }

        if (mapOverlayNoOpLoop.MapOverlayVisible)
        {
            evidence.Add("mapOverlayVisible:true");
        }

        if (mapOverlayNoOpLoop.MapBackNavigationAvailable)
        {
            evidence.Add("mapBackNavigationAvailable:true");
        }

        if (mapOverlayNoOpLoop.StaleEventChoicePresent)
        {
            evidence.Add("staleEventChoicePresent:true");
        }

        if (mapOverlayNoOpLoop.CurrentNodeArrowVisible)
        {
            evidence.Add("currentNodeArrowVisible:true");
        }

        if (mapOverlayNoOpLoop.ReachableNodeCandidatePresent)
        {
            evidence.Add("reachableNodeCandidatePresent:true");
        }

        if (mapOverlayNoOpLoop.RepeatedCurrentNodeArrowClick)
        {
            evidence.Add("repeatedCurrentNodeArrowClick:true");
        }

        if (!string.IsNullOrWhiteSpace(rewardMapLoop.LastLoopTarget))
        {
            evidence.Add($"rewardMapLoopTarget:{rewardMapLoop.LastLoopTarget}");
        }

        if (rewardMapLoop.ExplicitRewardChoicesPresent)
        {
            evidence.Add("rewardExplicitChoicesPresent:true");
        }

        if (rewardMapLoop.StaleRewardChoicePresent)
        {
            evidence.Add("staleRewardChoicePresent:true");
        }

        if (rewardMapLoop.StaleRewardBoundsPresent)
        {
            evidence.Add("staleRewardBoundsPresent:true");
        }

        if (rewardMapLoop.RewardBackNavigationAvailable)
        {
            evidence.Add("rewardBackNavigationAvailable:true");
        }

        if (rewardMapLoop.ClaimableRewardPresent)
        {
            evidence.Add("claimableRewardPresent:true");
        }

        if (rewardMapLoop.MapArrowContaminationPresent)
        {
            evidence.Add("mapContextVisible:true");
            evidence.Add("rewardMapArrowContamination:true");
        }

        if (rewardMapLoop.OffWindowBoundsReused)
        {
            evidence.Add("offWindowBoundsReused:true");
        }

        if (rewardMapLoop.MapNodeCandidateChosen)
        {
            evidence.Add("mapNodeCandidateChosen:true");
        }

        if (rewardMapLoop.RecoveryAttemptObserved)
        {
            evidence.Add("rewardMapRecoveryAttemptObserved:true");
        }

        if (rewardMapLoop.PostClickRecaptureObserved)
        {
            evidence.Add("postClickRecaptureObserved:true");
        }

        if (!string.IsNullOrWhiteSpace(mapTransitionStall.LastLoopTarget))
        {
            evidence.Add($"mapTransitionLoopTarget:{mapTransitionStall.LastLoopTarget}");
        }

        if (!string.IsNullOrWhiteSpace(combatNoOpLoop.BlockedTargetLabel))
        {
            evidence.Add($"combatNoOpTarget:{combatNoOpLoop.BlockedTargetLabel}");
        }

        if (!string.IsNullOrWhiteSpace(backlogRoute))
        {
            evidence.Add($"backlogRoute:{backlogRoute}");
        }

        var diagnosis = new GuiSmokeStallDiagnosisEntry(
            DateTimeOffset.UtcNow,
            Path.GetFileName(sessionRoot),
            attemptEntry.AttemptId,
            attemptEntry.AttemptOrdinal,
            diagnosisKind,
            diagnosisKind is "same-action-stall" or "scene-authority-invalid" or "phase-timeout" or "decision-abort" or "phase-mismatch-stall" or "decision-wait-plateau" or "combat-entry-surface-pending-wait-plateau" or "combat-enemy-click-resolution-pending-wait-plateau" or "combat-lifecycle-transit-wait-plateau" or "combat-barrier-wait-plateau" or "combat-release-failure-under-noncombat-foreground" or "reward-aftermath-card-progression-stall" or "rest-site-release-map-handoff-stall" or "rest-site-choice-not-click-ready" or "inspect-overlay-loop" or "reward-map-loop" or "map-overlay-noop-loop" or "map-transition-stall" or "combat-noop-loop" or "combat-entry-surface-pending-step-budget-exhausted" or "combat-enemy-click-resolution-pending-step-budget-exhausted" or "combat-lifecycle-transit-step-budget-exhausted" or "combat-barrier-step-budget-exhausted" or "combat-barrier-handoff-mismatch" or "rest-site-post-click-noop" or "rest-site-selection-failed" or "rest-site-grid-not-visible-after-selection" or "rest-site-grid-observer-miss" or "ancient-event-option-contract-mismatch" or "post-node-handoff-contract-mismatch",
            attemptEntry.FailureClass,
            attemptEntry.TerminalCause,
            phase,
            observerScreen,
            screenshotPath,
            sameActionStallCount,
            selfMetaReview?.PlateauDetected == true
            || decisionWaitPlateau.PlateauDetected
            || inspectOverlayLoop.LoopDetected
            || rewardMapLoop.LoopDetected
            || mapOverlayNoOpLoop.LoopDetected
            || mapTransitionStall.StallDetected
            || combatNoOpLoop.LoopDetected
            || string.Equals(diagnosisKind, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "rest-site-selection-failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "rest-site-grid-not-visible-after-selection", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "rest-site-grid-observer-miss", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "combat-entry-surface-pending-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "combat-enemy-click-resolution-pending-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "combat-lifecycle-transit-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "combat-barrier-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "combat-entry-surface-pending-wait-plateau", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "combat-enemy-click-resolution-pending-wait-plateau", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "combat-lifecycle-transit-wait-plateau", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "combat-barrier-handoff-mismatch", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "combat-release-failure-under-noncombat-foreground", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "reward-aftermath-card-progression-stall", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "rest-site-release-map-handoff-stall", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "rest-site-choice-not-click-ready", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "ancient-event-option-contract-mismatch", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "post-node-handoff-contract-mismatch", StringComparison.OrdinalIgnoreCase),
            backlogRoute,
            evidence);
        UpsertNdjson(GetStallDiagnosisPath(sessionRoot), diagnosis, static existing => existing.AttemptId, diagnosis.AttemptId);
    }
    private static string DetermineDiagnosisKind(
        GuiSmokeAttemptIndexEntry attemptEntry,
        GuiSmokeFailureSummary? failureSummary,
        int sameActionStallCount,
        DecisionWaitPlateauAnalysis decisionWaitPlateau,
        InspectOverlayLoopAnalysis inspectOverlayLoop,
        RewardMapLoopAnalysis rewardMapLoop,
        MapOverlayNoOpLoopAnalysis mapOverlayNoOpLoop,
        MapTransitionStallAnalysis mapTransitionStall,
        CombatNoOpLoopAnalysis combatNoOpLoop,
        string? latestPhase,
        string? latestObserverScreen)
    {
        var latestStateLooksCombat = string.Equals(latestPhase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(latestObserverScreen, "combat", StringComparison.OrdinalIgnoreCase);
        var latestStateLooksEvent = string.Equals(latestPhase, GuiSmokePhase.HandleEvent.ToString(), StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(latestObserverScreen, "event", StringComparison.OrdinalIgnoreCase);
        var latestStateLooksReward = string.Equals(latestPhase, GuiSmokePhase.HandleRewards.ToString(), StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(latestObserverScreen, "rewards", StringComparison.OrdinalIgnoreCase);
        if (string.Equals(attemptEntry.FailureClass, "scene-authority-invalid", StringComparison.OrdinalIgnoreCase))
        {
            return "scene-authority-invalid";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-entry-surface-pending-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-entry-surface-pending-step-budget-exhausted", StringComparison.OrdinalIgnoreCase))
        {
            return "combat-entry-surface-pending-step-budget-exhausted";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-enemy-click-resolution-pending-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-enemy-click-resolution-pending-step-budget-exhausted", StringComparison.OrdinalIgnoreCase))
        {
            return "combat-enemy-click-resolution-pending-step-budget-exhausted";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-lifecycle-transit-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-lifecycle-transit-step-budget-exhausted", StringComparison.OrdinalIgnoreCase))
        {
            return "combat-lifecycle-transit-step-budget-exhausted";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-barrier-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-barrier-step-budget-exhausted", StringComparison.OrdinalIgnoreCase))
        {
            return "combat-barrier-step-budget-exhausted";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-barrier-handoff-mismatch", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-barrier-handoff-mismatch", StringComparison.OrdinalIgnoreCase))
        {
            return "combat-barrier-handoff-mismatch";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-release-failure-under-noncombat-foreground", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-release-failure-under-noncombat-foreground", StringComparison.OrdinalIgnoreCase))
        {
            return "combat-release-failure-under-noncombat-foreground";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-entry-surface-pending-wait-plateau", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-entry-surface-pending-wait-plateau", StringComparison.OrdinalIgnoreCase))
        {
            return "combat-entry-surface-pending-wait-plateau";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-enemy-click-resolution-pending-wait-plateau", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-enemy-click-resolution-pending-wait-plateau", StringComparison.OrdinalIgnoreCase))
        {
            return "combat-enemy-click-resolution-pending-wait-plateau";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-lifecycle-transit-wait-plateau", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-lifecycle-transit-wait-plateau", StringComparison.OrdinalIgnoreCase))
        {
            return "combat-lifecycle-transit-wait-plateau";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-barrier-wait-plateau", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-barrier-wait-plateau", StringComparison.OrdinalIgnoreCase))
        {
            return "combat-barrier-wait-plateau";
        }

        if (string.Equals(attemptEntry.TerminalCause, "reward-aftermath-card-progression-stall", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "reward-aftermath-card-progression-stall", StringComparison.OrdinalIgnoreCase))
        {
            return "reward-aftermath-card-progression-stall";
        }

        if (string.Equals(attemptEntry.TerminalCause, "rest-site-release-map-handoff-stall", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "rest-site-release-map-handoff-stall", StringComparison.OrdinalIgnoreCase))
        {
            return "rest-site-release-map-handoff-stall";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-noop-loop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-noop-loop", StringComparison.OrdinalIgnoreCase)
            || (latestStateLooksCombat && combatNoOpLoop.LoopDetected))
        {
            return "combat-noop-loop";
        }

        if (string.Equals(attemptEntry.TerminalCause, "reward-map-loop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "reward-map-loop", StringComparison.OrdinalIgnoreCase)
            || (!latestStateLooksCombat && !latestStateLooksEvent && latestStateLooksReward && rewardMapLoop.LoopDetected))
        {
            return "reward-map-loop";
        }

        if (string.Equals(attemptEntry.TerminalCause, "map-overlay-noop-loop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "map-overlay-noop-loop", StringComparison.OrdinalIgnoreCase)
            || (latestStateLooksEvent && mapOverlayNoOpLoop.LoopDetected))
        {
            return "map-overlay-noop-loop";
        }

        if (string.Equals(attemptEntry.TerminalCause, "map-transition-stall", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "map-transition-stall", StringComparison.OrdinalIgnoreCase)
            || mapTransitionStall.StallDetected)
        {
            return "map-transition-stall";
        }

        if (string.Equals(attemptEntry.TerminalCause, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase))
        {
            return "rest-site-post-click-noop";
        }

        if (string.Equals(attemptEntry.TerminalCause, "rest-site-selection-failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "rest-site-selection-failed", StringComparison.OrdinalIgnoreCase))
        {
            return "rest-site-selection-failed";
        }

        if (string.Equals(attemptEntry.TerminalCause, "rest-site-grid-not-visible-after-selection", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "rest-site-grid-not-visible-after-selection", StringComparison.OrdinalIgnoreCase))
        {
            return "rest-site-grid-not-visible-after-selection";
        }

        if (string.Equals(attemptEntry.TerminalCause, "rest-site-grid-observer-miss", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "rest-site-grid-observer-miss", StringComparison.OrdinalIgnoreCase))
        {
            return "rest-site-grid-observer-miss";
        }

        if (string.Equals(attemptEntry.TerminalCause, "rest-site-choice-not-click-ready", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "rest-site-choice-not-click-ready", StringComparison.OrdinalIgnoreCase))
        {
            return "rest-site-choice-not-click-ready";
        }

        if (string.Equals(attemptEntry.TerminalCause, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase)
            || (decisionWaitPlateau.PlateauDetected && string.Equals(decisionWaitPlateau.DiagnosisKind, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase)))
        {
            return "phase-mismatch-stall";
        }

        if (string.Equals(attemptEntry.TerminalCause, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase)
            || (decisionWaitPlateau.PlateauDetected && string.Equals(decisionWaitPlateau.DiagnosisKind, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase)))
        {
            return "decision-wait-plateau";
        }

        if (string.Equals(attemptEntry.TerminalCause, "inspect-overlay-loop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "inspect-overlay-loop", StringComparison.OrdinalIgnoreCase)
            || inspectOverlayLoop.LoopDetected)
        {
            return "inspect-overlay-loop";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-noop-loop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-noop-loop", StringComparison.OrdinalIgnoreCase)
            || combatNoOpLoop.LoopDetected)
        {
            return "combat-noop-loop";
        }

        if (string.Equals(attemptEntry.TerminalCause, "same-action-stall", StringComparison.OrdinalIgnoreCase)
            || sameActionStallCount > 0)
        {
            return "same-action-stall";
        }

        if (string.Equals(attemptEntry.TerminalCause, "phase-timeout", StringComparison.OrdinalIgnoreCase))
        {
            return "phase-timeout";
        }

        if (string.Equals(attemptEntry.TerminalCause, "decision-abort", StringComparison.OrdinalIgnoreCase))
        {
            return "decision-abort";
        }

        if (string.Equals(attemptEntry.TerminalCause, "ancient-event-option-contract-mismatch", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "ancient-event-option-contract-mismatch", StringComparison.OrdinalIgnoreCase))
        {
            return "ancient-event-option-contract-mismatch";
        }

        if (string.Equals(attemptEntry.TerminalCause, "post-node-handoff-contract-mismatch", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "post-node-handoff-contract-mismatch", StringComparison.OrdinalIgnoreCase))
        {
            return "post-node-handoff-contract-mismatch";
        }

        if (string.Equals(attemptEntry.FailureClass, "launch-runtime-noise", StringComparison.OrdinalIgnoreCase))
        {
            return "launch-runtime-noise";
        }

        if (string.Equals(attemptEntry.Status, "failed", StringComparison.OrdinalIgnoreCase))
        {
            return failureSummary?.Phase is not null ? $"failed:{failureSummary.Phase}" : "failed";
        }

        return "no-stall";
    }

    private static DecisionWaitPlateauAnalysis AnalyzeDecisionWaitPlateau(IReadOnlyList<GuiSmokeStepProgress> progress)
    {
        if (progress.Count == 0)
        {
            return new DecisionWaitPlateauAnalysis("no-stall", false, 0, null, null, null);
        }

        var lastWait = progress.LastOrDefault(entry => entry.ObserverSignals.Contains("decision-wait", StringComparer.OrdinalIgnoreCase));
        if (lastWait is null)
        {
            return new DecisionWaitPlateauAnalysis("no-stall", false, 0, null, null, null);
        }

        var normalizedSignature = NormalizeSceneSignatureForPlateau(lastWait.SceneSignature);
        var repeatedWaitCount = 0;
        for (var index = progress.Count - 1; index >= 0; index -= 1)
        {
            var entry = progress[index];
            if (!entry.ObserverSignals.Contains("decision-wait", StringComparer.OrdinalIgnoreCase))
            {
                break;
            }

            if (!string.Equals(entry.Phase, lastWait.Phase, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(entry.ObserverScreen, lastWait.ObserverScreen, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(NormalizeSceneSignatureForPlateau(entry.SceneSignature), normalizedSignature, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            repeatedWaitCount += 1;
        }

        var phaseMismatchObserved = string.Equals(lastWait.Phase, GuiSmokePhase.Embark.ToString(), StringComparison.OrdinalIgnoreCase)
                                    && SignatureIndicatesRoomScreen(normalizedSignature, lastWait.ObserverScreen);
        var plateauLimit = phaseMismatchObserved ? 2 : 5;
        if (repeatedWaitCount < plateauLimit)
        {
            return new DecisionWaitPlateauAnalysis("no-stall", false, repeatedWaitCount, lastWait.Phase, lastWait.ObserverScreen, normalizedSignature);
        }

        return new DecisionWaitPlateauAnalysis(
            phaseMismatchObserved ? "phase-mismatch-stall" : "decision-wait-plateau",
            true,
            repeatedWaitCount,
            lastWait.Phase,
            lastWait.ObserverScreen,
            normalizedSignature);
    }

    private static InspectOverlayLoopAnalysis AnalyzeInspectOverlayLoop(IReadOnlyList<GuiSmokeStepProgress> progress)
    {
        if (progress.Count == 0)
        {
            return new InspectOverlayLoopAnalysis("no-stall", false, 0, null, null, null, null);
        }

        var lastOverlayAction = progress.LastOrDefault(entry => IsOverlayCleanupDecisionTarget(entry.DecisionTargetLabel));
        if (lastOverlayAction is null)
        {
            return new InspectOverlayLoopAnalysis("no-stall", false, 0, null, null, null, null);
        }

        var normalizedSignature = NormalizeSceneSignatureForPlateau(lastOverlayAction.SceneSignature);
        if (!SignatureIndicatesRoomScreen(normalizedSignature, lastOverlayAction.ObserverScreen))
        {
            return new InspectOverlayLoopAnalysis("no-stall", false, 0, lastOverlayAction.Phase, lastOverlayAction.ObserverScreen, normalizedSignature, null);
        }

        var overlayCloseCount = 0;
        string? lastMisdirectedTarget = null;
        for (var index = progress.Count - 1; index >= 0; index -= 1)
        {
            var entry = progress[index];
            if (!string.Equals(entry.ObserverScreen, lastOverlayAction.ObserverScreen, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(NormalizeSceneSignatureForPlateau(entry.SceneSignature), normalizedSignature, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (IsOverlayCleanupDecisionTarget(entry.DecisionTargetLabel))
            {
                overlayCloseCount += 1;
                continue;
            }

            if (entry.ObserverSignals.Contains("alternate-branch:HandleEvent", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("alternate-branch:HandleRewards", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("inspect-overlay", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("reward-choice", StringComparer.OrdinalIgnoreCase)
                || entry.DecisionTargetLabel is null)
            {
                continue;
            }

            if (string.Equals(entry.DecisionTargetLabel, "first event option", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.DecisionTargetLabel, "event progression choice", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.DecisionTargetLabel, "reward choice", StringComparison.OrdinalIgnoreCase)
                || GuiSmokeRewardActionTargetSupport.IsRewardCardChoiceTarget(entry.DecisionTargetLabel)
                || GuiSmokeRewardActionTargetSupport.IsRewardClaimTarget(entry.DecisionTargetLabel))
            {
                lastMisdirectedTarget = entry.DecisionTargetLabel;
            }

            break;
        }

        if (overlayCloseCount < 3)
        {
            return new InspectOverlayLoopAnalysis("no-stall", false, overlayCloseCount, lastOverlayAction.Phase, lastOverlayAction.ObserverScreen, normalizedSignature, lastMisdirectedTarget);
        }

        return new InspectOverlayLoopAnalysis(
            "inspect-overlay-loop",
            true,
            overlayCloseCount,
            lastOverlayAction.Phase,
            lastOverlayAction.ObserverScreen,
            normalizedSignature,
            lastMisdirectedTarget);
    }

    private static RewardMapLoopAnalysis AnalyzeRewardMapLoop(IReadOnlyList<GuiSmokeStepProgress> progress)
    {
        if (progress.Count == 0)
        {
            return new RewardMapLoopAnalysis("no-stall", false, 0, null, null, null, null, false, false, false, false, false, false, false, false, false, false);
        }

        var latestProgress = progress[^1];
        if (string.Equals(latestProgress.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestProgress.ObserverScreen, "combat", StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestProgress.PostActionScreen, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return new RewardMapLoopAnalysis("no-stall", false, 0, latestProgress.Phase, latestProgress.PostActionScreen ?? latestProgress.ObserverScreen, NormalizeSceneSignatureForPlateau(latestProgress.SceneSignature), null, false, false, false, false, false, false, false, false, false, false);
        }

        var lastLoopEntry = progress.LastOrDefault(IsRewardMapLoopProgressEntry);
        if (lastLoopEntry is null)
        {
            return new RewardMapLoopAnalysis("no-stall", false, 0, null, null, null, null, false, false, false, false, false, false, false, false, false, false);
        }

        var normalizedSignature = NormalizeSceneSignatureForPlateau(lastLoopEntry.SceneSignature);
        var repeatedLoopCount = 0;
        string? lastLoopTarget = null;
        var explicitRewardChoicesPresent = false;
        var staleRewardChoicePresent = normalizedSignature.Contains("stale:reward-choice", StringComparison.OrdinalIgnoreCase);
        var staleRewardBoundsPresent = normalizedSignature.Contains("stale:reward-bounds", StringComparison.OrdinalIgnoreCase);
        var rewardBackNavigationAvailable = normalizedSignature.Contains("layer:reward-back-nav", StringComparison.OrdinalIgnoreCase);
        var claimableRewardPresent = normalizedSignature.Contains("reward:claimable", StringComparison.OrdinalIgnoreCase);
        var mapArrowContaminationPresent = normalizedSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase)
                                           || normalizedSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)
                                           || normalizedSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase);
        var offWindowBoundsReused = staleRewardBoundsPresent;
        var mapNodeCandidateChosen = false;
        var recoveryAttemptObserved = false;
        var postClickRecaptureObserved = false;
        for (var index = progress.Count - 1; index >= 0; index -= 1)
        {
            var entry = progress[index];
            if (!IsRewardMapLoopProgressEntry(entry)
                || !string.Equals(entry.ObserverScreen, lastLoopEntry.ObserverScreen, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(NormalizeSceneSignatureForPlateau(entry.SceneSignature), normalizedSignature, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            explicitRewardChoicesPresent |= HasExplicitRewardProgressionEvidence(entry);
            staleRewardChoicePresent |= entry.SceneSignature.Contains("stale:reward-choice", StringComparison.OrdinalIgnoreCase);
            staleRewardBoundsPresent |= entry.SceneSignature.Contains("stale:reward-bounds", StringComparison.OrdinalIgnoreCase);
            rewardBackNavigationAvailable |= entry.SceneSignature.Contains("layer:reward-back-nav", StringComparison.OrdinalIgnoreCase);
            claimableRewardPresent |= entry.SceneSignature.Contains("reward:claimable", StringComparison.OrdinalIgnoreCase)
                                      || entry.ObserverSignals.Contains("claimable-reward-present", StringComparer.OrdinalIgnoreCase);
            mapArrowContaminationPresent |= entry.SceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase)
                                            || entry.SceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)
                                            || entry.SceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase);
            offWindowBoundsReused |= entry.SceneSignature.Contains("stale:reward-bounds", StringComparison.OrdinalIgnoreCase);
            mapNodeCandidateChosen |= entry.ActuatorSignals.Contains("map-node-candidate-chosen", StringComparer.OrdinalIgnoreCase);
            recoveryAttemptObserved |= entry.ObserverSignals.Contains("reward-map-recovery-attempt", StringComparer.OrdinalIgnoreCase);
            postClickRecaptureObserved |= entry.ActuatorSignals.Contains("post-click-recapture-observed", StringComparer.OrdinalIgnoreCase);

            if (IsRewardMapLoopTarget(entry.DecisionTargetLabel) || IsStaleRewardLoopTarget(entry.DecisionTargetLabel))
            {
                repeatedLoopCount += 1;
                lastLoopTarget ??= entry.DecisionTargetLabel;
                continue;
            }

            if (entry.DecisionTargetLabel is null
                || entry.ObserverSignals.Contains("alternate-branch:HandleRewards", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("alternate-branch:WaitMap", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("reward-screen-authority", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("reward-explicit-progression", StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            break;
        }

        var loopDetected = repeatedLoopCount >= 2
                           && mapArrowContaminationPresent
                           && (explicitRewardChoicesPresent || staleRewardChoicePresent || staleRewardBoundsPresent);
        if (!loopDetected)
        {
            return new RewardMapLoopAnalysis(
                "no-stall",
                false,
                repeatedLoopCount,
                lastLoopEntry.Phase,
                lastLoopEntry.ObserverScreen,
                normalizedSignature,
                lastLoopTarget,
                explicitRewardChoicesPresent,
                mapArrowContaminationPresent,
                staleRewardChoicePresent,
                staleRewardBoundsPresent,
                rewardBackNavigationAvailable,
                offWindowBoundsReused,
                claimableRewardPresent,
                mapNodeCandidateChosen,
                recoveryAttemptObserved,
                postClickRecaptureObserved);
        }

        return new RewardMapLoopAnalysis(
            "reward-map-loop",
            true,
            repeatedLoopCount,
            lastLoopEntry.Phase,
            lastLoopEntry.ObserverScreen,
            normalizedSignature,
            lastLoopTarget,
            explicitRewardChoicesPresent,
            mapArrowContaminationPresent,
            staleRewardChoicePresent,
            staleRewardBoundsPresent,
            rewardBackNavigationAvailable,
            offWindowBoundsReused,
            claimableRewardPresent,
            mapNodeCandidateChosen,
            recoveryAttemptObserved,
            postClickRecaptureObserved);
    }

    private static MapTransitionStallAnalysis AnalyzeMapTransitionStall(IReadOnlyList<GuiSmokeStepProgress> progress)
    {
        if (progress.Count == 0)
        {
            return new MapTransitionStallAnalysis("no-stall", false, 0, null, null, null, null);
        }

        var lastLoopEntry = progress.LastOrDefault(IsMapTransitionProgressEntry);
        if (lastLoopEntry is null)
        {
            return new MapTransitionStallAnalysis("no-stall", false, 0, null, null, null, null);
        }

        var normalizedSignature = NormalizeSceneSignatureForPlateau(lastLoopEntry.SceneSignature);
        var repeatedLoopCount = 0;
        string? lastLoopTarget = null;
        for (var index = progress.Count - 1; index >= 0; index -= 1)
        {
            var entry = progress[index];
            if (!IsMapTransitionProgressEntry(entry)
                || !string.Equals(entry.ObserverScreen, lastLoopEntry.ObserverScreen, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(NormalizeSceneSignatureForPlateau(entry.SceneSignature), normalizedSignature, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (IsMapTransitionLoopTarget(entry.DecisionTargetLabel))
            {
                repeatedLoopCount += 1;
                lastLoopTarget ??= entry.DecisionTargetLabel;
                continue;
            }

            if (entry.DecisionTargetLabel is null
                || entry.ObserverSignals.Contains("alternate-branch:HandleEvent", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("alternate-branch:ChooseFirstNode", StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            break;
        }

        if (repeatedLoopCount < 2)
        {
            return new MapTransitionStallAnalysis("no-stall", false, repeatedLoopCount, lastLoopEntry.Phase, lastLoopEntry.ObserverScreen, normalizedSignature, lastLoopTarget);
        }

        return new MapTransitionStallAnalysis(
            "map-transition-stall",
            true,
            repeatedLoopCount,
            lastLoopEntry.Phase,
            lastLoopEntry.ObserverScreen,
            normalizedSignature,
            lastLoopTarget);
    }

    private static MapOverlayNoOpLoopAnalysis AnalyzeMapOverlayNoOpLoop(IReadOnlyList<GuiSmokeStepProgress> progress, LatestStepContext? latestStepContext)
    {
        if (progress.Count == 0)
        {
            return new MapOverlayNoOpLoopAnalysis("no-stall", false, 0, null, null, null, null, false, false, false, false, false, false);
        }

        var latestOverlayState = latestStepContext?.MapOverlayState;
        var shouldUseLatestOverlayFallback = latestOverlayState?.ForegroundVisible == true;
        var lastLoopEntry = progress.LastOrDefault(entry =>
            IsMapOverlayProgressEntry(entry)
            || (shouldUseLatestOverlayFallback && IsMapTransitionProgressEntry(entry)));
        if (lastLoopEntry is null)
        {
            return new MapOverlayNoOpLoopAnalysis("no-stall", false, 0, null, null, null, null, false, false, false, false, false, false);
        }

        var normalizedSignature = NormalizeSceneSignatureForPlateau(lastLoopEntry.SceneSignature);
        var repeatedLoopCount = 0;
        var repeatedCurrentNodeArrowClick = false;
        string? lastLoopTarget = null;
        for (var index = progress.Count - 1; index >= 0; index -= 1)
        {
            var entry = progress[index];
            if (!(IsMapOverlayProgressEntry(entry)
                  || (shouldUseLatestOverlayFallback && IsMapTransitionProgressEntry(entry)))
                || !string.Equals(entry.ObserverScreen, lastLoopEntry.ObserverScreen, StringComparison.OrdinalIgnoreCase)
                || (!shouldUseLatestOverlayFallback
                    && !string.Equals(NormalizeSceneSignatureForPlateau(entry.SceneSignature), normalizedSignature, StringComparison.OrdinalIgnoreCase)))
            {
                break;
            }

            if (IsMapOverlayLoopTarget(entry.DecisionTargetLabel))
            {
                repeatedLoopCount += 1;
                lastLoopTarget ??= entry.DecisionTargetLabel;
                if (string.Equals(entry.DecisionTargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase))
                {
                    repeatedCurrentNodeArrowClick = true;
                }

                continue;
            }

            if (entry.DecisionTargetLabel is null
                || entry.ObserverSignals.Contains("alternate-branch:WaitMap", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("alternate-branch:ChooseFirstNode", StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            break;
        }

        var mapOverlayVisible = normalizedSignature.Contains("layer:map-overlay-foreground", StringComparison.OrdinalIgnoreCase)
                                || lastLoopEntry.ObserverSignals.Contains("map-overlay-visible", StringComparer.OrdinalIgnoreCase)
                                || latestOverlayState?.ForegroundVisible == true;
        var mapBackNavigationAvailable = normalizedSignature.Contains("map-back-navigation-available", StringComparison.OrdinalIgnoreCase)
                                         || lastLoopEntry.ObserverSignals.Contains("map-back-navigation-available", StringComparer.OrdinalIgnoreCase)
                                         || latestOverlayState?.MapBackNavigationAvailable == true;
        var staleEventChoicePresent = normalizedSignature.Contains("stale:event-choice", StringComparison.OrdinalIgnoreCase)
                                      || lastLoopEntry.ObserverSignals.Contains("stale-event-choice", StringComparer.OrdinalIgnoreCase)
                                      || latestOverlayState?.StaleEventChoicePresent == true;
        var currentNodeArrowVisible = normalizedSignature.Contains("current-node-arrow-visible", StringComparison.OrdinalIgnoreCase)
                                      || lastLoopEntry.ObserverSignals.Contains("current-node-arrow-visible", StringComparer.OrdinalIgnoreCase)
                                      || latestOverlayState?.CurrentNodeArrowVisible == true;
        var reachableNodeCandidatePresent = normalizedSignature.Contains("reachable-node-candidate-present", StringComparison.OrdinalIgnoreCase)
                                            || lastLoopEntry.ObserverSignals.Contains("reachable-node-candidate-present", StringComparer.OrdinalIgnoreCase)
                                            || normalizedSignature.Contains("exported-reachable-node-present", StringComparison.OrdinalIgnoreCase)
                                            || lastLoopEntry.ObserverSignals.Contains("exported-reachable-node-present", StringComparer.OrdinalIgnoreCase)
                                            || latestOverlayState?.ReachableNodeCandidatePresent == true
                                            || latestOverlayState?.ExportedReachableNodeCandidatePresent == true;
        var loopDetected = repeatedLoopCount >= 3
                           && mapOverlayVisible
                           && staleEventChoicePresent
                           && currentNodeArrowVisible;
        return new MapOverlayNoOpLoopAnalysis(
            loopDetected ? "map-overlay-noop-loop" : "no-stall",
            loopDetected,
            repeatedLoopCount,
            lastLoopEntry.Phase,
            lastLoopEntry.ObserverScreen,
            normalizedSignature,
            lastLoopTarget,
            mapOverlayVisible,
            mapBackNavigationAvailable,
            staleEventChoicePresent,
            currentNodeArrowVisible,
            reachableNodeCandidatePresent,
            repeatedCurrentNodeArrowClick);
    }

    private static CombatNoOpLoopAnalysis AnalyzeCombatNoOpLoop(IReadOnlyList<GuiSmokeStepProgress> progress)
    {
        if (progress.Count == 0)
        {
            return new CombatNoOpLoopAnalysis("no-stall", false, 0, null, null, null, null);
        }

        var lastCombatEntry = progress.LastOrDefault(entry =>
            string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(entry.DecisionTargetLabel));
        if (lastCombatEntry is null)
        {
            return new CombatNoOpLoopAnalysis("no-stall", false, 0, null, null, null, null);
        }

        var normalizedSignature = NormalizeSceneSignatureForPlateau(lastCombatEntry.SceneSignature);
        var blockedTargetLabel = progress
            .Where(entry =>
                string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                && entry.ObserverSignals.Any(signal => signal.StartsWith("combat-noop-observed:combat lane slot ", StringComparison.OrdinalIgnoreCase)))
            .Select(entry => entry.ObserverSignals.Last(signal => signal.StartsWith("combat-noop-observed:combat lane slot ", StringComparison.OrdinalIgnoreCase)).Split(':', 2)[1])
            .LastOrDefault()
            ?? progress
                .Where(entry =>
                    string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                    && entry.DecisionTargetLabel is not null
                    && entry.DecisionTargetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase))
                .Select(static entry => entry.DecisionTargetLabel)
                .LastOrDefault();
        var repeatedSelectionCount = 0;
        var enemyTargetCount = 0;
        var combatLoopActionCount = 0;
        for (var index = progress.Count - 1; index >= 0; index -= 1)
        {
            var entry = progress[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                || !string.Equals(entry.ObserverScreen, lastCombatEntry.ObserverScreen, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(NormalizeSceneSignatureForPlateau(entry.SceneSignature), normalizedSignature, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (entry.ObserverSignals.Any(signal => string.Equals(signal, $"combat-noop-observed:{blockedTargetLabel}", StringComparison.OrdinalIgnoreCase)))
            {
                repeatedSelectionCount += 1;
                if (string.Equals(entry.DecisionTargetLabel, "auto-target enemy", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(entry.DecisionTargetLabel, "auto-target enemy recenter", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(entry.DecisionTargetLabel, "auto-target enemy alternate", StringComparison.OrdinalIgnoreCase)
                    || (entry.DecisionTargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    enemyTargetCount += 1;
                }

                combatLoopActionCount += 1;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(blockedTargetLabel)
                && string.Equals(entry.DecisionTargetLabel, blockedTargetLabel.Replace("combat lane ", "combat select attack ", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase))
            {
                repeatedSelectionCount += 1;
                combatLoopActionCount += 1;
                continue;
            }

            if (string.Equals(entry.DecisionTargetLabel, "auto-target enemy", StringComparison.OrdinalIgnoreCase)
                || (entry.DecisionTargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                enemyTargetCount += 1;
                combatLoopActionCount += 1;
                continue;
            }

            if (string.Equals(entry.DecisionTargetLabel, "auto-target enemy recenter", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.DecisionTargetLabel, "auto-target enemy alternate", StringComparison.OrdinalIgnoreCase))
            {
                enemyTargetCount += 1;
                combatLoopActionCount += 1;
                continue;
            }

            if (entry.DecisionTargetLabel is null)
            {
                continue;
            }

            if (entry.DecisionTargetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase))
            {
                combatLoopActionCount += 1;
                continue;
            }

            break;
        }

        if (repeatedSelectionCount == 0 && string.IsNullOrWhiteSpace(blockedTargetLabel))
        {
            blockedTargetLabel = progress
                .Where(entry =>
                    string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                    && entry.DecisionTargetLabel is not null
                    && entry.DecisionTargetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase))
                .GroupBy(static entry => entry.DecisionTargetLabel, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(static group => group.Count())
                .Select(static group => group.Key)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(blockedTargetLabel))
            {
                repeatedSelectionCount = progress.Count(entry =>
                    string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(entry.DecisionTargetLabel, blockedTargetLabel, StringComparison.OrdinalIgnoreCase));
            }
        }

        var explicitNoOpSignalsObserved = progress.Any(entry =>
            string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
            && entry.ObserverSignals.Any(signal => signal.StartsWith("combat-noop-observed:", StringComparison.OrdinalIgnoreCase)));
        var loopDetected = repeatedSelectionCount >= 2
                           && enemyTargetCount >= (explicitNoOpSignalsObserved ? 2 : 3)
                           && combatLoopActionCount >= 5
                           && !string.IsNullOrWhiteSpace(blockedTargetLabel);
        if (!loopDetected)
        {
            return new CombatNoOpLoopAnalysis("no-stall", false, repeatedSelectionCount, lastCombatEntry.Phase, lastCombatEntry.ObserverScreen, normalizedSignature, blockedTargetLabel);
        }

        return new CombatNoOpLoopAnalysis(
            "combat-noop-loop",
            true,
            repeatedSelectionCount,
            lastCombatEntry.Phase,
            lastCombatEntry.ObserverScreen,
            normalizedSignature,
            blockedTargetLabel);
    }

    private static bool IsRewardMapLoopProgressEntry(GuiSmokeStepProgress entry)
    {
        return (string.Equals(entry.Phase, GuiSmokePhase.HandleRewards.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Phase, GuiSmokePhase.WaitMap.ToString(), StringComparison.OrdinalIgnoreCase))
               && (SignatureIndicatesRewardScreen(entry.SceneSignature, entry.ObserverScreen)
                   || HasExplicitRewardProgressionEvidence(entry));
    }

    private static bool HasExplicitRewardProgressionEvidence(GuiSmokeStepProgress entry)
    {
        return entry.ObserverSignals.Contains("reward-explicit-progression", StringComparer.OrdinalIgnoreCase)
               || entry.ObserverSignals.Contains("reward-choice", StringComparer.OrdinalIgnoreCase)
               || entry.ObserverSignals.Contains("choice-extractor:reward", StringComparer.OrdinalIgnoreCase)
               || entry.ObserverSignals.Contains("choice-extractor:rewards", StringComparer.OrdinalIgnoreCase)
               || SignatureIndicatesRewardScreen(entry.SceneSignature, entry.ObserverScreen);
    }

    private static bool SignatureIndicatesRewardScreen(string? sceneSignature, string? observerScreen)
    {
        return string.Equals(observerScreen, "rewards", StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(sceneSignature)
                   && (sceneSignature.Contains("screen:rewards", StringComparison.OrdinalIgnoreCase)
                       || sceneSignature.Contains("visible:rewards", StringComparison.OrdinalIgnoreCase)));
    }

    private static bool IsMapTransitionProgressEntry(GuiSmokeStepProgress entry)
    {
        return (string.Equals(entry.Phase, GuiSmokePhase.HandleEvent.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Phase, GuiSmokePhase.WaitMap.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Phase, GuiSmokePhase.ChooseFirstNode.ToString(), StringComparison.OrdinalIgnoreCase)
                || LooksLikeEventMapFallbackWait(entry))
               && (entry.ObserverSignals.Contains("map-transition-evidence", StringComparer.OrdinalIgnoreCase)
                   || entry.SceneSignature.Contains("substate:map-transition", StringComparison.OrdinalIgnoreCase)
                   || entry.SceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)
                   || LooksLikeEventMapFallbackWait(entry));
    }

    private static bool IsMapOverlayProgressEntry(GuiSmokeStepProgress entry)
    {
        return (string.Equals(entry.Phase, GuiSmokePhase.HandleEvent.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Phase, GuiSmokePhase.WaitMap.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Phase, GuiSmokePhase.ChooseFirstNode.ToString(), StringComparison.OrdinalIgnoreCase))
               && (entry.SceneSignature.Contains("layer:map-overlay-foreground", StringComparison.OrdinalIgnoreCase)
                   || entry.ObserverSignals.Contains("map-overlay-visible", StringComparer.OrdinalIgnoreCase)
                   || entry.ObserverSignals.Contains("stale-event-choice", StringComparer.OrdinalIgnoreCase));
    }

    private static LatestStepContext? LoadLatestStepContext(string runRoot)
    {
        var stepsRoot = Path.Combine(runRoot, "steps");
        if (!Directory.Exists(stepsRoot))
        {
            return null;
        }

        var latestRequestPath = Directory.GetFiles(stepsRoot, "*.request.json", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .LastOrDefault();
        if (latestRequestPath is null)
        {
            return null;
        }

        var request = TryReadJson<GuiSmokeStepRequest>(latestRequestPath);
        if (request is null)
        {
            return null;
        }

        var observerStatePath = latestRequestPath.Replace(".request.json", ".observer.state.json", StringComparison.OrdinalIgnoreCase);
        using var observerStateDocument = TryReadJsonDocument(observerStatePath);
        var overlayObserver = new ObserverState(request.Observer, observerStateDocument, null, null);
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(overlayObserver, request.WindowBounds, request.ScreenshotPath);
        return new LatestStepContext(
            request.Phase,
            request.Observer.CurrentScreen ?? request.Observer.VisibleScreen,
            request.ScreenshotPath,
            mapOverlayState);
    }

    private static bool IsMapTransitionLoopTarget(string? decisionTargetLabel)
    {
        return string.Equals(decisionTargetLabel, "event progression choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "proceed after resolving rewards", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMapOverlayLoopTarget(string? decisionTargetLabel)
    {
        return string.Equals(decisionTargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "map back", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRewardMapLoopTarget(string? decisionTargetLabel)
    {
        return string.Equals(decisionTargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsStaleRewardLoopTarget(string? decisionTargetLabel)
    {
        return GuiSmokeRewardActionTargetSupport.IsRewardStaleLoopTarget(decisionTargetLabel);
    }

    private static bool IsOverlayCleanupDecisionTarget(string? decisionTargetLabel)
    {
        return string.Equals(decisionTargetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "overlay back", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "overlay backdrop close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "inspect overlay escape", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeEventMapFallbackWait(GuiSmokeStepProgress entry)
    {
        return string.Equals(entry.Phase, GuiSmokePhase.WaitCombat.ToString(), StringComparison.OrdinalIgnoreCase)
               && string.Equals(entry.ObserverScreen, "event", StringComparison.OrdinalIgnoreCase)
               && entry.ObserverSignals.Contains("alternate-branch:HandleEvent", StringComparer.OrdinalIgnoreCase)
               && (entry.ObserverSignals.Contains("reward-explicit-progression", StringComparer.OrdinalIgnoreCase)
                   || entry.ObserverSignals.Contains("choice-extractor:event", StringComparer.OrdinalIgnoreCase));
    }

    private static string NormalizeSceneSignatureForPlateau(string? sceneSignature)
    {
        if (string.IsNullOrWhiteSpace(sceneSignature))
        {
            return "scene:none";
        }

        var parts = sceneSignature
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static part =>
                !part.StartsWith("shot:", StringComparison.OrdinalIgnoreCase)
                && !part.StartsWith("phase:", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        return parts.Length == 0 ? sceneSignature : string.Join("|", parts);
    }

    private static bool SignatureIndicatesRoomScreen(string normalizedSignature, string? observerScreen)
    {
        if (string.Equals(observerScreen, "event", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observerScreen, "rewards", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observerScreen, "shop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observerScreen, "map", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observerScreen, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return normalizedSignature.Contains("|screen:event|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|screen:rewards|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|screen:shop|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|screen:map|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|screen:combat|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|room:rest-site|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|room:shop|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|room:treasure|", StringComparison.OrdinalIgnoreCase);
    }

    private static string? FindLatestScreenshotPath(string runRoot)
    {
        var stepsRoot = Path.Combine(runRoot, "steps");
        if (!Directory.Exists(stepsRoot))
        {
            return null;
        }

        return Directory.GetFiles(stepsRoot, "*.screen.png", SearchOption.TopDirectoryOnly)
            .OrderByDescending(static path => path, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    private static bool ShouldRouteToDecompilerBacklog(string diagnosisKind, string? phase, string? observerScreen)
    {
        return string.Equals(diagnosisKind, "scene-authority-invalid", StringComparison.OrdinalIgnoreCase)
               && (IsEarlyMenuPhase(phase)
                   || string.Equals(observerScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(observerScreen, "character-select", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsEarlyMenuPhase(string? phase)
    {
        return string.Equals(phase, GuiSmokePhase.EnterRun.ToString(), StringComparison.OrdinalIgnoreCase)
               || string.Equals(phase, GuiSmokePhase.WaitCharacterSelect.ToString(), StringComparison.OrdinalIgnoreCase)
               || string.Equals(phase, GuiSmokePhase.ChooseCharacter.ToString(), StringComparison.OrdinalIgnoreCase)
               || string.Equals(phase, GuiSmokePhase.Embark.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
