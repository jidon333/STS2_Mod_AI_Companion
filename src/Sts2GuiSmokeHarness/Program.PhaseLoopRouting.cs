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
    static bool IsPassiveWaitPhase(GuiSmokePhase phase)
    {
        return phase is GuiSmokePhase.WaitMainMenu
            or GuiSmokePhase.WaitRunLoad
            or GuiSmokePhase.WaitCharacterSelect
            or GuiSmokePhase.WaitMap
            or GuiSmokePhase.WaitPostMapNodeRoom
            or GuiSmokePhase.WaitEventRelease
            or GuiSmokePhase.WaitCombat;
    }

    static string BuildDecisionWaitFingerprint(
        GuiSmokePhase phase,
        string sceneSignature,
        ObserverState observer,
        GuiSmokeStepAnalysisContext? analysisContext = null)
    {
        var safeTransitFingerprint = phase == GuiSmokePhase.HandleCombat && analysisContext is not null
            ? CombatBarrierSupport.TryBuildSafeTransitProgressFingerprint(analysisContext.CombatBarrierEvaluation, observer.Summary)
            : null;
        return string.Join("|",
            phase.ToString(),
            NormalizeSceneSignatureForPlateau(sceneSignature),
            observer.CurrentScreen ?? "unknown",
            observer.VisibleScreen ?? "unknown",
            observer.ChoiceExtractorPath ?? "unknown",
            GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "declaringType") ?? "unknown",
            observer.InventoryId ?? "unknown",
            BuildActionableStateSignature(observer.ActionNodes),
            safeTransitFingerprint ?? "transit:none");
    }

    static string BuildOverlayLoopFingerprint(string sceneSignature, ObserverState observer)
    {
        var overlayLabels = observer.CurrentChoices
            .Where(label => IsOverlayChoiceLabel(label) || IsSkipOrProceedLabel(label))
            .Take(6);
        return string.Join("|",
            NormalizeSceneSignatureForPlateau(sceneSignature),
            observer.CurrentScreen ?? "unknown",
            observer.VisibleScreen ?? "unknown",
            observer.InventoryId ?? "unknown",
            string.Join(";", overlayLabels));
    }

    static string NormalizeSceneSignatureForPlateau(string sceneSignature)
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

    static string BuildActionableStateSignature(IReadOnlyList<ObserverActionNode> actionNodes)
    {
        return string.Join(";",
            actionNodes
                .Where(static node => node.Actionable)
                .OrderBy(static node => node.NodeId, StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .Select(node => $"{node.NodeId}:{node.Kind}:{node.Label}"));
    }

    static bool TryClassifyDecisionWaitPlateau(
        GuiSmokePhase phase,
        ObserverState observer,
        int consecutiveDecisionWaitCount,
        out string terminalCause,
        out string message)
    {
        var postEmbarkPhase = GuiSmokePhase.Embark;
        var phaseMismatchObserved = phase == GuiSmokePhase.Embark && GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(observer, out postEmbarkPhase);
        var plateauLimit = phaseMismatchObserved ? 2 : GetDecisionWaitPlateauLimit(phase);
        if (consecutiveDecisionWaitCount < plateauLimit)
        {
            terminalCause = string.Empty;
            message = string.Empty;
            return false;
        }

        if (phaseMismatchObserved)
        {
            terminalCause = "phase-mismatch-stall";
            message = $"phase-mismatch-stall phase={phase} observer={observer.CurrentScreen ?? observer.VisibleScreen ?? "null"} reconciledPhase={postEmbarkPhase} waits={consecutiveDecisionWaitCount}";
            return true;
        }

        terminalCause = "decision-wait-plateau";
        message = $"decision-wait-plateau phase={phase} screen={observer.CurrentScreen ?? "null"} waits={consecutiveDecisionWaitCount} inventory={observer.InventoryId ?? "null"}";
        return true;
    }

    static int GetDecisionWaitPlateauLimit(GuiSmokePhase phase)
    {
        return phase == GuiSmokePhase.HandleCombat ? 4 : 5;
    }

    static int GetSameActionStallLimit(GuiSmokePhase phase, GuiSmokeStepDecision decision)
    {
        if (phase == GuiSmokePhase.HandleCombat)
        {
            return 2;
        }

        return decision.TargetLabel switch
        {
            "continue" => 4,
            "singleplayer" => 4,
            "ironclad" => 4,
            "embark" => 4,
            "ancient event completion" => 4,
            "event progression choice" => 4,
            "first event option" => 4,
            "reward choice" => 3,
            "reward card choice" => 3,
            "colorless card choice" => 3,
            "reward skip" => 3,
            "visible proceed" => 4,
            "visible map advance" => 4,
            "visible reachable node" => 4,
            "first reachable node" => 4,
            "exported reachable map node" => 4,
            "map back" => 4,
            "treasure chest center" => 4,
            "treasure chest" => 4,
            "treasure relic holder" => 4,
            "treasure proceed" => 4,
            "claim reward item" => 3,
            "rest site: smith card" => 4,
            "rest site: smith confirm" => 4,
            "hidden overlay close" => 4,
            "overlay back" => 4,
            "overlay close" => 4,
            "overlay backdrop close" => 4,
            "treasure overlay back" => 4,
            "inspect overlay escape" => 3,
            _ => 2,
        };
    }

    static bool TryClassifyRewardMapLoop(
        GuiSmokePhase phase,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string message)
    {
        message = string.Empty;
        if (phase is not GuiSmokePhase.HandleRewards and not GuiSmokePhase.WaitMap)
        {
            return false;
        }

        var rewardScene = AutoDecisionProvider.BuildRewardSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath);
        var rewardMapLayer = rewardScene.LayerState;
        var mapContextVisible = rewardMapLayer.MapContextVisible
                                || request.SceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase)
                                || request.SceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)
                                || request.SceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase);
        if ((!rewardScene.RewardForegroundOwned && !rewardMapLayer.StaleRewardChoicePresent)
            || !mapContextVisible)
        {
            return false;
        }

        if (!IsRewardMapLoopTarget(decision.TargetLabel) && !IsStaleRewardLoopTarget(decision.TargetLabel))
        {
            return false;
        }

        var repeatedLoopCount = 1;
        for (var index = request.History.Count - 1; index >= 0; index -= 1)
        {
            var entry = request.History[index];
            if (entry.Phase is not nameof(GuiSmokePhase.HandleRewards) and not nameof(GuiSmokePhase.WaitMap))
            {
                break;
            }

            if (IsRewardMapLoopTarget(entry.TargetLabel) || IsStaleRewardLoopTarget(entry.TargetLabel))
            {
                repeatedLoopCount += 1;
                continue;
            }

            if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            break;
        }

        if (repeatedLoopCount < 3)
        {
            return false;
        }

        message = $"reward-map-loop phase={phase} target={decision.TargetLabel ?? "null"} observer={request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "null"} repeats={repeatedLoopCount} staleChoice={rewardMapLayer.StaleRewardChoicePresent} staleBounds={rewardMapLayer.StaleRewardBoundsPresent} backNav={rewardMapLayer.RewardBackNavigationAvailable} mapVisible={mapContextVisible} offWindow={rewardMapLayer.OffWindowBoundsReused}";
        return true;
    }

    static bool TryClassifyMapTransitionStall(
        GuiSmokePhase phase,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string message)
    {
        message = string.Empty;
        if (phase is not GuiSmokePhase.HandleEvent and not GuiSmokePhase.WaitMap and not GuiSmokePhase.ChooseFirstNode)
        {
            return false;
        }

        if (!HasStrongMapTransitionEvidenceFromScene(request.Observer, request.SceneSignature))
        {
            return false;
        }

        if (!IsMapTransitionLoopTarget(decision.TargetLabel))
        {
            return false;
        }

        var repeatedLoopCount = 1;
        for (var index = request.History.Count - 1; index >= 0; index -= 1)
        {
            var entry = request.History[index];
            if (entry.Phase is not nameof(GuiSmokePhase.HandleEvent) and not nameof(GuiSmokePhase.WaitMap) and not nameof(GuiSmokePhase.ChooseFirstNode))
            {
                break;
            }

            if (IsMapTransitionLoopTarget(entry.TargetLabel))
            {
                repeatedLoopCount += 1;
                continue;
            }

            if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            break;
        }

        if (repeatedLoopCount < 3)
        {
            return false;
        }

        message = $"map-transition-stall phase={phase} target={decision.TargetLabel ?? "null"} observer={request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "null"} repeats={repeatedLoopCount}";
        return true;
    }

    static bool TryClassifyMapOverlayNoOpLoop(
        GuiSmokePhase phase,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string message)
    {
        message = string.Empty;
        if (phase is not GuiSmokePhase.HandleEvent and not GuiSmokePhase.WaitMap and not GuiSmokePhase.ChooseFirstNode)
        {
            return false;
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (!mapOverlayState.ForegroundVisible
            || !mapOverlayState.StaleEventChoicePresent
            || (!mapOverlayState.CurrentNodeArrowVisible && !mapOverlayState.MapBackNavigationAvailable))
        {
            return false;
        }

        if (!IsMapOverlayLoopTarget(decision.TargetLabel))
        {
            return false;
        }

        var repeatedLoopCount = 1;
        var repeatedCurrentNodeArrowClick = string.Equals(decision.TargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        for (var index = request.History.Count - 1; index >= 0; index -= 1)
        {
            var entry = request.History[index];
            if (entry.Phase is not nameof(GuiSmokePhase.HandleEvent) and not nameof(GuiSmokePhase.WaitMap) and not nameof(GuiSmokePhase.ChooseFirstNode))
            {
                break;
            }

            if (IsMapOverlayLoopTarget(entry.TargetLabel))
            {
                repeatedLoopCount += 1;
                if (string.Equals(entry.TargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase))
                {
                    repeatedCurrentNodeArrowClick += 1;
                }

                continue;
            }

            if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            break;
        }

        if (repeatedLoopCount < 4 && repeatedCurrentNodeArrowClick < 2)
        {
            return false;
        }

        message = $"map-overlay-noop-loop phase={phase} target={decision.TargetLabel ?? "null"} observer={request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "null"} repeats={repeatedLoopCount} staleEventChoice={mapOverlayState.StaleEventChoicePresent} backNav={mapOverlayState.MapBackNavigationAvailable} currentArrow={mapOverlayState.CurrentNodeArrowVisible} exportedReachable={mapOverlayState.ExportedReachableNodeCandidatePresent}";
        return true;
    }

    static bool TryClassifyCombatNoOpLoop(
        GuiSmokePhase phase,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string message)
    {
        message = string.Empty;
        if (phase != GuiSmokePhase.HandleCombat)
        {
            return false;
        }

        var analysis = AutoDecisionProvider.PeekCombatNoOpLoop(request.History);
        if (!analysis.LoopDetected || !analysis.BlockedSlotIndex.HasValue)
        {
            return false;
        }

        var blockedSlot = analysis.BlockedSlotIndex.Value;
        var loopTarget = $"combat select attack slot {blockedSlot}";
        var pendingSelection = AutoDecisionProvider.TryPeekPendingCombatSelection(request.History);
        var decisionRepeatsLoop = string.Equals(decision.TargetLabel, loopTarget, StringComparison.OrdinalIgnoreCase)
                                  || ((string.Equals(decision.TargetLabel, "auto-target enemy", StringComparison.OrdinalIgnoreCase)
                                       || string.Equals(decision.TargetLabel, "auto-target enemy recenter", StringComparison.OrdinalIgnoreCase)
                                       || string.Equals(decision.TargetLabel, "auto-target enemy alternate", StringComparison.OrdinalIgnoreCase)
                                       || (decision.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) ?? false))
                                      && pendingSelection?.Kind == AutoCombatCardKind.AttackLike
                                      && pendingSelection.SlotIndex == blockedSlot);
        if (!decisionRepeatsLoop)
        {
            return false;
        }

        message = $"combat-noop-loop phase={phase} blockedSlot={blockedSlot} energy={request.Observer.PlayerEnergy?.ToString(CultureInfo.InvariantCulture) ?? "null"} repeats={analysis.RepeatedSelectionCount}";
        return true;
    }

    static bool TryClassifyRestSitePostClickNoOp(
        GuiSmokePhase phase,
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string message)
    {
        message = string.Empty;
        if (phase is not GuiSmokePhase.ChooseFirstNode and not GuiSmokePhase.WaitMap)
        {
            return false;
        }

        if (!string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase)
            || !IsRestSitePostClickDecisionRisk(decision.DecisionRisk)
            || !AutoDecisionProvider.IsExplicitRestSiteOptionTargetLabel(decision.TargetLabel)
            || (!AutoDecisionProvider.HasExplicitRestSiteChoiceAuthorityForRequest(request)
                && !AutoDecisionProvider.HasRecentRestSiteExplicitClickForRequest(request, decision.TargetLabel)))
        {
            return false;
        }

        var evidence = RestSiteObserverSignals.BuildPostClickEvidence(request.Observer, decision.TargetLabel);
        message = $"{decision.DecisionRisk ?? "rest-site-post-click-noop"} phase={phase} target={decision.TargetLabel ?? "null"} observer={request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "null"} fingerprint={RestSiteChoiceSupport.BuildExplicitChoiceFingerprint(request.Observer)} outcome={evidence.Outcome ?? "null"} outcomeEvidence={evidence.OutcomeEvidence ?? "null"} currentStatus={evidence.CurrentStatus ?? "null"} currentOption={evidence.CurrentOptionId ?? "null"} lastSignal={evidence.LastSignal ?? "null"} lastOption={evidence.LastOptionId ?? "null"} upgradeScreenVisible={evidence.UpgradeScreenVisible} smithGridVisible={evidence.SmithGridVisible} smithConfirmVisible={evidence.SmithConfirmVisible} explicitChoices={evidence.ExplicitChoiceVisible} observerMiss={evidence.UpgradeChoiceObserverMiss}";
        return true;
    }

    static bool IsRestSitePostClickDecisionRisk(string? decisionRisk)
    {
        return string.Equals(decisionRisk, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionRisk, "rest-site-selection-failed", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionRisk, "rest-site-grid-not-visible-after-selection", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionRisk, "rest-site-grid-observer-miss", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsMapTransitionLoopTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "event progression choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "proceed after resolving rewards", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsMapOverlayLoopTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "map back", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsRewardMapLoopTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsStaleRewardLoopTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "reward skip", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "reward choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "claim reward item", StringComparison.OrdinalIgnoreCase);
    }

    static bool ShouldAllowRewardMapRecovery(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        if (!IsRewardMapRecoveryTarget(decision.TargetLabel))
        {
            return false;
        }

        return CountRecentRewardMapRecoveryAttempts(request.History) < 2;
    }

    static bool IsRewardMapRecoveryTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "reward back", StringComparison.OrdinalIgnoreCase);
    }

    static int CountRecentRewardMapRecoveryAttempts(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var count = 0;
        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (string.Equals(entry.Action, "reward-map-recovery", StringComparison.OrdinalIgnoreCase))
            {
                count += 1;
                continue;
            }

            if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            break;
        }

        return count;
    }

    static bool TryParseScreenBounds(string? raw, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
        {
            return false;
        }

        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
        {
            return false;
        }

        if (width <= 0 || height <= 0)
        {
            return false;
        }

        bounds = new RectangleF(x, y, width, height);
        return true;
    }

    static bool ShouldOpenCombatAlternateBranch(ObserverState observer)
    {
        return CombatBarrierPolicy.IsStableCombatEntryObserver(observer)
               && GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary);
    }

    static bool TryAdvanceAlternateBranch(
        GuiSmokePhase phase,
        ObserverState observer,
        List<GuiSmokeHistoryEntry> history,
        ArtifactRecorder logger,
        int stepIndex,
        bool isLongRun,
        out GuiSmokePhase nextPhase)
    {
        nextPhase = phase;

        if (phase == GuiSmokePhase.WaitRunLoad)
        {
            if (GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(observer, out var postRunLoadPhase))
            {
                var branchKind = postRunLoadPhase switch
                {
                    GuiSmokePhase.ChooseCharacter => "branch-character-select",
                    GuiSmokePhase.HandleRewards => "branch-rewards",
                    GuiSmokePhase.HandleCombat => "branch-combat",
                    GuiSmokePhase.HandleEvent => "branch-event",
                    GuiSmokePhase.HandleShop => "branch-shop",
                    GuiSmokePhase.ChooseFirstNode when TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary) => "branch-treasure",
                    GuiSmokePhase.ChooseFirstNode when LooksLikeRestSiteState(observer.Summary) => "branch-rest-site",
                    GuiSmokePhase.ChooseFirstNode => "branch-map",
                    _ => "branch-room",
                };
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), branchKind, null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), branchKind, observer.CurrentScreen, observer.InCombat, null));
                nextPhase = postRunLoadPhase;
                return true;
            }

            if (WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(observer.Summary))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "retry-enter-run", observer.CurrentScreen, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "retry-enter-run", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.EnterRun;
                return true;
            }
        }

        if (phase == GuiSmokePhase.WaitCharacterSelect)
        {
            if (LooksLikeSingleplayerSubmenuState(observer.Summary))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-singleplayer-submenu", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-singleplayer-submenu", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.EnterRun;
                return true;
            }

            if (GuiSmokeObserverPhaseHeuristics.TryGetPostCharacterSelectPhase(observer, out var postCharacterSelectPhase))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-character-select", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-character-select", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = postCharacterSelectPhase;
                return true;
            }
        }

        if (phase == GuiSmokePhase.Embark && GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(observer, out var postEmbarkPhase))
        {
            var branchKind = postEmbarkPhase switch
            {
                GuiSmokePhase.HandleRewards => "branch-rewards",
                GuiSmokePhase.HandleCombat => "branch-combat",
                GuiSmokePhase.HandleEvent => "branch-event",
                GuiSmokePhase.HandleShop => "branch-shop",
                GuiSmokePhase.ChooseFirstNode when LooksLikeRestSiteState(observer.Summary) => "branch-rest-site",
                GuiSmokePhase.ChooseFirstNode => "branch-map",
                _ => "branch-room",
            };
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), branchKind, null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), branchKind, observer.CurrentScreen, observer.InCombat, null));
            nextPhase = postEmbarkPhase;
            return true;
        }

        if (phase == GuiSmokePhase.WaitMainMenu)
        {
            if (ShopObserverSignals.IsShopAuthorityActive(observer.Summary))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-shop", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-shop", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleShop;
                return true;
            }

            if (string.Equals(observer.CurrentScreen, "character-select", StringComparison.OrdinalIgnoreCase))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-character-select", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-character-select", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseCharacter;
                return true;
            }

            if (ShouldRouteToHandleRewards(observer, history))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rewards", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rewards", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleRewards;
                return true;
            }

            if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer.Summary))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rest-site-upgrade", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rest-site-upgrade", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (LooksLikeRestSiteProceedState(observer.Summary))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rest-site-proceed", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rest-site-proceed", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (MapForegroundReconciliation.HasMapForegroundOwnership(observer, history))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-map", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-map", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (ShouldOpenCombatAlternateBranch(observer))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-combat", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-combat", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleCombat;
                return true;
            }

            if (ShouldRouteToHandleEvent(observer, history))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-event", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-event", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleEvent;
                return true;
            }
        }

        if (phase == GuiSmokePhase.WaitMap)
        {
            if (TryReopenMixedStateModalBranchFromWaitMap(observer, history, logger, stepIndex, out nextPhase))
            {
                return true;
            }

            if (ShopObserverSignals.IsShopAuthorityActive(observer.Summary))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-shop", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-shop", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleShop;
                return true;
            }

            if (ShouldRouteToHandleRewards(observer, history))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rewards", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rewards", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleRewards;
                return true;
            }

            if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer.Summary))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rest-site-upgrade", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rest-site-upgrade", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (LooksLikeRestSiteProceedState(observer.Summary))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rest-site-proceed", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rest-site-proceed", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (MapForegroundReconciliation.HasMapForegroundOwnership(observer, history))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-map", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-map", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (ShouldOpenCombatAlternateBranch(observer))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-combat", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-combat", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleCombat;
                return true;
            }

            if (ShouldRouteToHandleEvent(observer, history))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-event", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-event", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleEvent;
                return true;
            }
        }

        if (phase is GuiSmokePhase.ChooseFirstNode or GuiSmokePhase.WaitPostMapNodeRoom or GuiSmokePhase.WaitCombat)
        {
            if (ShouldOpenCombatAlternateBranch(observer))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-combat", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-combat", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleCombat;
                return true;
            }

            if ((phase == GuiSmokePhase.ChooseFirstNode || phase == GuiSmokePhase.WaitPostMapNodeRoom)
                && AncientEventObserverSignals.HasMapReleaseAuthority(
                    observer.Summary,
                    GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "declaringType"),
                    GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "instanceType")))
            {
                if (phase == GuiSmokePhase.WaitPostMapNodeRoom)
                {
                    history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-map", null, DateTimeOffset.UtcNow));
                    logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-map", observer.CurrentScreen, observer.InCombat, null));
                    nextPhase = GuiSmokePhase.ChooseFirstNode;
                    return true;
                }

                return false;
            }

            if (AncientEventObserverSignals.HasForegroundAuthority(observer.Summary))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-event", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-event", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleEvent;
                return true;
            }

            if (phase == GuiSmokePhase.WaitPostMapNodeRoom
                && GuiSmokeObserverPhaseHeuristics.TryGetPostMapNodePhase(observer, out var postMapNodePhase))
            {
                var branchKind = postMapNodePhase switch
                {
                    GuiSmokePhase.HandleRewards => "branch-rewards",
                    GuiSmokePhase.HandleCombat => "branch-combat",
                    GuiSmokePhase.HandleEvent => "branch-event",
                    GuiSmokePhase.HandleShop => "branch-shop",
                    GuiSmokePhase.ChooseFirstNode when TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary) => "branch-treasure",
                    GuiSmokePhase.ChooseFirstNode when LooksLikeRestSiteState(observer.Summary) => "branch-rest-site",
                    GuiSmokePhase.ChooseFirstNode => "branch-map",
                    _ => "branch-room",
                };
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), branchKind, null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), branchKind, observer.CurrentScreen, observer.InCombat, null));
                nextPhase = postMapNodePhase;
                return true;
            }

            if (ShopObserverSignals.IsShopAuthorityActive(observer.Summary))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-shop", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-shop", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleShop;
                return true;
            }

            if (ShouldRouteToHandleRewards(observer, history))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rewards", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rewards", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleRewards;
                return true;
            }

            if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer.Summary))
            {
                if (phase == GuiSmokePhase.ChooseFirstNode)
                {
                    return false;
                }

                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rest-site-upgrade", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rest-site-upgrade", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (LooksLikeRestSiteState(observer.Summary))
            {
                if (phase == GuiSmokePhase.ChooseFirstNode)
                {
                    return false;
                }

                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rest-site", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rest-site", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary))
            {
                if (phase == GuiSmokePhase.ChooseFirstNode)
                {
                    return false;
                }

                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-treasure", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-treasure", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer) && phase == GuiSmokePhase.WaitCombat)
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-map", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-map", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (ShouldRouteToHandleEvent(observer, history))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-event", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-event", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleEvent;
                return true;
            }
        }

        if (phase == GuiSmokePhase.HandleShop)
        {
            var shopState = ShopObserverSignals.TryGetState(observer.Summary);
            if (shopState is { ForegroundOwned: true })
            {
                return false;
            }

            if (CardSelectionObserverSignals.IsCardSelectionState(observer.Summary))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "shop-released-card-selection", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "shop-released-card-selection", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "shop-released-treasure", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "shop-released-treasure", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (ShouldRouteToHandleRewards(observer, history))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "shop-resolved-rewards", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "shop-resolved-rewards", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleRewards;
                return true;
            }

            if (LooksLikeRestSiteState(observer.Summary))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "shop-resolved-rest-site", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "shop-resolved-rest-site", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (ShouldRouteToHandleEvent(observer, history))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "shop-resolved-event", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "shop-resolved-event", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleEvent;
                return true;
            }

            if (ShouldOpenCombatAlternateBranch(observer))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "shop-resolved-combat", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "shop-resolved-combat", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleCombat;
                return true;
            }

            if (shopState is { TeardownInProgress: true }
                || shopState is { MapIsCurrentActiveScreen: true }
                || GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "shop-released-map", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "shop-released-map", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.WaitMap;
                return true;
            }
        }

        if (phase == GuiSmokePhase.HandleEvent)
        {
            if (AncientEventObserverSignals.HasForegroundAuthority(observer.Summary))
            {
                return false;
            }

            if (GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "event-resolved-map", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "event-resolved-map", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.ChooseFirstNode;
                return true;
            }

            if (ShouldOpenCombatAlternateBranch(observer))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "event-resolved-combat", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "event-resolved-combat", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleCombat;
                return true;
            }

            if (ShouldRouteToHandleRewards(observer, history))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "event-resolved-rewards", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "event-resolved-rewards", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = GuiSmokePhase.HandleRewards;
                return true;
            }
        }

        if (phase == GuiSmokePhase.WaitEventRelease)
        {
            if (GuiSmokeObserverPhaseHeuristics.TryGetPostEventReleasePhase(observer, out var postEventReleasePhase))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "event-release-reconciled", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "event-release-reconciled", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = postEventReleasePhase;
                return true;
            }
        }

        if (phase == GuiSmokePhase.HandleCombat)
        {
            if (ShouldRouteToHandleRewards(observer, history))
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "combat-resolved-rewards", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "combat-resolved-rewards", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = isLongRun
                    ? GuiSmokePhase.HandleRewards
                    : GuiSmokePhase.Completed;
                return true;
            }

            if (string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase) && observer.InCombat != true)
            {
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "combat-resolved-map", null, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "combat-resolved-map", observer.CurrentScreen, observer.InCombat, null));
                nextPhase = isLongRun
                    ? GuiSmokePhase.ChooseFirstNode
                    : GuiSmokePhase.Completed;
                return true;
            }
        }

        return false;
    }

    static bool TryReopenMixedStateModalBranchFromWaitMap(
        ObserverState observer,
        List<GuiSmokeHistoryEntry> history,
        ArtifactRecorder logger,
        int stepIndex,
        out GuiSmokePhase nextPhase)
    {
        nextPhase = GuiSmokePhase.WaitMap;
        string? branchKind = null;

        if (CardSelectionObserverSignals.IsCardSelectionState(observer.Summary))
        {
            branchKind = "branch-card-selection";
            nextPhase = GuiSmokePhase.ChooseFirstNode;
        }
        else if (TryGetCanonicalWaitMapReopenBranch(observer, history, out branchKind, out nextPhase))
        {
        }

        if (branchKind is null)
        {
            return false;
        }

        history.Add(new GuiSmokeHistoryEntry(GuiSmokePhase.WaitMap.ToString(), branchKind, null, DateTimeOffset.UtcNow));
        logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, GuiSmokePhase.WaitMap.ToString(), branchKind, observer.CurrentScreen, observer.InCombat, null));
        return true;
    }

    static bool ShouldRouteToHandleRewards(
        ObserverState observer,
        IReadOnlyList<GuiSmokeHistoryEntry>? history,
        string? screenshotPath = null)
    {
        if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer.Summary)
            || LooksLikeRestSiteProceedState(observer.Summary)
            || LooksLikeRestSiteState(observer.Summary)
            || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary)
            || ShopObserverSignals.IsShopAuthorityActive(observer.Summary)
            || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary))
        {
            return false;
        }

        var rewardScene = AutoDecisionProvider.BuildRewardSceneState(observer, null, history, screenshotPath);
        return rewardScene.RewardForegroundOwned
               && rewardScene.ReleaseStage == RewardReleaseStage.Active;
    }

    static bool ShouldRouteToHandleEvent(
        ObserverState observer,
        IReadOnlyList<GuiSmokeHistoryEntry>? history,
        string? screenshotPath = null)
    {
        if (ShopObserverSignals.IsShopAuthorityActive(observer.Summary)
            || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary)
            || LooksLikeRestSiteState(observer.Summary)
            || LooksLikeRestSiteProceedState(observer.Summary)
            || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer.Summary)
            || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary))
        {
            return false;
        }

        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, null, history, screenshotPath);
        return eventScene.EventForegroundOwned
               && eventScene.ReleaseStage == EventReleaseStage.Active;
    }

    static bool TryGetCanonicalWaitMapReopenBranch(
        ObserverState observer,
        IReadOnlyList<GuiSmokeHistoryEntry>? history,
        out string? branchKind,
        out GuiSmokePhase nextPhase)
    {
        branchKind = null;
        nextPhase = GuiSmokePhase.WaitMap;

        if (AutoDecisionProvider.BuildTreasureSceneState(observer) is not null)
        {
            branchKind = "branch-treasure";
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        var rewardScene = AutoDecisionProvider.BuildRewardSceneState(observer, null, history);
        if (rewardScene.RewardForegroundOwned && rewardScene.ReleaseStage == RewardReleaseStage.Active)
        {
            branchKind = "branch-rewards";
            nextPhase = GuiSmokePhase.HandleRewards;
            return true;
        }

        if (AutoDecisionProvider.BuildShopSceneState(observer, history) is { ReleaseStage: NonCombatReleaseStage.Active })
        {
            branchKind = "branch-shop";
            nextPhase = GuiSmokePhase.HandleShop;
            return true;
        }

        if (AutoDecisionProvider.BuildRestSiteSceneState(observer) is { ReleaseStage: NonCombatReleaseStage.Active })
        {
            branchKind = "branch-rest-site";
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, null, history);
        if (eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.Active)
        {
            branchKind = "branch-event";
            nextPhase = GuiSmokePhase.HandleEvent;
            return true;
        }

        return false;
    }

}
