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
using static ObserverScreenProvenance;

static class GuiSmokeStepRequestFactory
{
    internal static IReadOnlyList<GuiSmokeHistoryEntry> BuildSerializedStepHistory(
        GuiSmokePhase phase,
        IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return phase == GuiSmokePhase.HandleCombat
            ? HandleCombatContextSupport.BuildSerializedHistoryWindow(history)
            : history.TakeLast(5).ToArray();
    }

    internal static GuiSmokeStepAnalysisContext CreateStepAnalysisContext(
        GuiSmokePhase phase,
        ObserverState observer,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        WindowBounds? windowBounds = null)
    {
        static AutoCombatAnalysis EmptyCombatAnalysis() => new(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown);
        static AutoCombatHandAnalysis EmptyCombatHandAnalysis() => new(Array.Empty<AutoCombatHandSlotAnalysis>());

        ReconstructedHandleCombatContext? combatContext = null;
        PendingCombatSelection? pendingSelection = null;
        var pendingSelectionComputed = false;
        CombatRuntimeState? runtimeCombatState = null;
        AutoCombatAnalysis? combatAnalysis = null;
        AutoCombatHandAnalysis? combatHandAnalysis = null;
        CombatBarrierEvaluation? combatBarrierEvaluation = null;
        RewardSceneState? rewardScene = null;
        EventSceneState? eventScene = null;
        PostNodeHandoffState? postNodeHandoffState = null;
        ICanonicalNonCombatSceneState? canonicalNonCombatScene = null;
        var canonicalNonCombatSceneComputed = false;

        ReconstructedHandleCombatContext GetCombatContext()
            => combatContext ??= HandleCombatContextSupport.Reconstruct(history, observer.Summary, combatCardKnowledge);

        PendingCombatSelection? GetPendingSelection()
        {
            if (!pendingSelectionComputed)
            {
                pendingSelection = CombatRuntimeStateSupport.ResolvePendingSelection(observer.Summary, combatCardKnowledge, GetCombatContext().PendingSelection, history);
                pendingSelectionComputed = true;
            }

            return pendingSelection;
        }

        CombatRuntimeState GetRuntimeCombatState()
            => runtimeCombatState ??= CombatRuntimeStateSupport.Read(observer.Summary, combatCardKnowledge);

        AutoCombatAnalysis GetCombatAnalysis()
            => combatAnalysis ??= string.IsNullOrWhiteSpace(screenshotPath) ? EmptyCombatAnalysis() : AutoCombatAnalyzer.Analyze(screenshotPath);

        AutoCombatHandAnalysis GetCombatHandAnalysis()
            => combatHandAnalysis ??= string.IsNullOrWhiteSpace(screenshotPath) ? EmptyCombatHandAnalysis() : AutoCombatHandAnalyzer.Analyze(screenshotPath);

        bool HasSelectedNonEnemyConfirmEvidence()
        {
            var pendingSelection = GetPendingSelection();
            return CombatRuntimeStateSupport.HasRuntimeSelectedNonEnemyConfirmEvidence(observer.Summary, combatCardKnowledge, pendingSelection);
        }

        bool CanResolveCombatEnemyTarget()
        {
            var pendingSelection = GetPendingSelection();
            return !string.IsNullOrWhiteSpace(screenshotPath)
                ? CombatRuntimeStateSupport.CanResolveEnemyTarget(observer.Summary, combatCardKnowledge, pendingSelection, GetCombatAnalysis())
                : CombatRuntimeStateSupport.CanResolveEnemyTargetWithoutScreenshot(observer.Summary, combatCardKnowledge, pendingSelection);
        }

        CombatBarrierEvaluation GetCombatBarrierEvaluation()
            => combatBarrierEvaluation ??= CombatBarrierSupport.Evaluate(
                history,
                observer,
                GetCombatContext(),
                GetRuntimeCombatState(),
                string.IsNullOrWhiteSpace(screenshotPath) ? EmptyCombatAnalysis() : GetCombatAnalysis(),
                HasSelectedNonEnemyConfirmEvidence(),
                CanResolveCombatEnemyTarget(),
                CombatEligibilitySupport.IsCombatPlayerActionWindowClosed(observer.Summary));

        RewardSceneState GetRewardScene()
            => rewardScene ??= AutoDecisionProvider.BuildRewardSceneState(observer, windowBounds, history, screenshotPath);

        EventSceneState GetEventScene()
            => eventScene ??= AutoDecisionProvider.BuildEventSceneState(observer, windowBounds, history, screenshotPath);

        PostNodeHandoffState GetPostNodeHandoffState()
            => postNodeHandoffState ??= AutoDecisionProvider.BuildPostNodeHandoffState(observer, windowBounds, history, screenshotPath);

        ICanonicalNonCombatSceneState? GetCanonicalNonCombatScene()
        {
            if (!canonicalNonCombatSceneComputed)
            {
                canonicalNonCombatScene = AutoDecisionProvider.TryBuildCanonicalNonCombatSceneState(observer, windowBounds, history, screenshotPath);
                canonicalNonCombatSceneComputed = true;
            }

            return canonicalNonCombatScene;
        }

        bool ComputeUseCombatFastPath()
        {
            if (phase != GuiSmokePhase.HandleCombat || observer.InCombat != true)
            {
                return false;
            }

            var combatScreenVisible = MatchesControlFlowScreen(observer, "combat");
            if (!combatScreenVisible)
            {
                return false;
            }

            return true;
        }

        bool ComputeUseRewardFastPath()
        {
            if (phase != GuiSmokePhase.HandleRewards)
            {
                return false;
            }

            if (CardSelectionObserverSignals.TryGetState(observer.Summary) is not null)
            {
                return true;
            }

            var rewardState = RewardObserverSignals.TryGetState(observer.Summary);
            return rewardState is { ForegroundOwned: true }
                   || rewardState is { TeardownInProgress: true }
                   || rewardState is { MapIsCurrentActiveScreen: true }
                   || GuiSmokeNonCombatContractSupport.HasExplicitRewardProgressionAffordance(observer.Summary);
        }

        return new GuiSmokeStepAnalysisContext(
            phase,
            observer,
            windowBounds,
            screenshotPath,
            history,
            combatCardKnowledge,
            ComputeUseCombatFastPath,
            ComputeUseRewardFastPath,
            () => GuiSmokeRewardMapEvidenceSupport.BuildRewardMapLayerState(observer.Summary, windowBounds),
            () =>
            {
                var rewardMapLayer = GuiSmokeRewardMapEvidenceSupport.BuildRewardMapLayerState(observer.Summary, windowBounds);
                return rewardMapLayer.RewardBackNavigationAvailable;
            },
            () => GetRewardScene().ClaimableRewardPresent,
            () => GuiSmokeMapOverlayHeuristics.BuildState(observer, windowBounds, screenshotPath),
            GetRewardScene,
            GetEventScene,
            GetPostNodeHandoffState,
            GetCanonicalNonCombatScene,
            GetCombatContext,
            GetPendingSelection,
            GetRuntimeCombatState,
            GetCombatAnalysis,
            GetCombatHandAnalysis,
            () => CombatEligibilitySupport.IsCombatPlayerActionWindowClosed(observer.Summary),
            HasSelectedNonEnemyConfirmEvidence,
            CanResolveCombatEnemyTarget,
            GetCombatBarrierEvaluation);
    }

    internal static GuiSmokeStepAnalysisContext CreateObserverOnlyAnalysisContext(
        GuiSmokePhase phase,
        ObserverState observer,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        WindowBounds? windowBounds = null)
    {
        return CreateStepAnalysisContext(
            phase,
            observer,
            null,
            history,
            combatCardKnowledge,
            windowBounds);
    }

    internal static GuiSmokeStepAnalysisContext CreateRequestAnalysisContext(GuiSmokeStepRequest request)
    {
        var stateDocument = GuiSmokeReplayArtifactLoader.TryLoadObserverStateSidecar(request.ScreenshotPath);
        return CreateStepAnalysisContext(
            Enum.Parse<GuiSmokePhase>(request.Phase, ignoreCase: true),
            new ObserverState(request.Observer, stateDocument, null, request.Observer.LastEventsTail?.ToArray() ?? Array.Empty<string>()),
            request.ScreenshotPath,
            request.History,
            request.CombatCardKnowledge,
            request.WindowBounds);
    }

    internal static GuiSmokeStepRequest CreateStepRequest(
        string runId,
        string scenarioId,
        int stepIndex,
        GuiSmokePhase phase,
        string screenshotPath,
        WindowCaptureTarget window,
        ObserverState observer,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        string workspaceRoot,
        string sessionRoot,
        string attemptId,
        int attemptOrdinal,
        GuiSmokeStepAnalysisContext? analysisContext = null,
        GuiSmokeSceneRequestContext? sceneContext = null)
    {
        var effectiveScreenshotPath = analysisContext?.ScreenshotPath;
        var requestScreenshotPath = effectiveScreenshotPath ?? string.Empty;
        var serializedHistory = analysisContext?.History ?? BuildSerializedStepHistory(phase, history);
        var combatCardKnowledge = analysisContext?.CombatCardKnowledge ?? GuiSmokeSceneReasoningSupport.LoadCombatCardKnowledge(workspaceRoot, observer);
        var sceneSignature = sceneContext?.SceneSignature ?? GuiSmokeSceneReasoningSupport.ComputeSceneSignatureCore(requestScreenshotPath, observer, phase, analysisContext);
        var useAuthorityFastPath = analysisContext?.UseAuthorityFastPath == true;
        var firstSeenScene = sceneContext?.FirstSeenScene ?? !GuiSmokeSceneReasoningSupport.HasSceneSignatureHistory(sessionRoot, sceneSignature);
        var reasoningMode = sceneContext?.ReasoningMode ?? GuiSmokeSceneReasoningSupport.DetermineReasoningMode(phase, observer, firstSeenScene);
        var knownRecipes = sceneContext?.KnownRecipes
            ?? (useAuthorityFastPath
                ? Array.Empty<KnownRecipeHint>()
                : GuiSmokeSceneReasoningSupport.LoadKnownRecipes(sessionRoot, sceneSignature, phase.ToString()));
        var eventKnowledgeCandidates = useAuthorityFastPath
            ? Array.Empty<EventKnowledgeCandidate>()
            : GuiSmokeSceneReasoningSupport.LoadEventKnowledgeCandidates(workspaceRoot, observer, reasoningMode);
        return new GuiSmokeStepRequest(
            runId,
            scenarioId,
            stepIndex,
            phase.ToString(),
            GuiSmokePromptContractSupport.BuildGoal(phase),
            DateTimeOffset.UtcNow,
            requestScreenshotPath,
            new WindowBounds(window.Bounds.X, window.Bounds.Y, window.Bounds.Width, window.Bounds.Height),
            sceneSignature,
            attemptId,
            attemptOrdinal,
            3,
            firstSeenScene,
            reasoningMode,
            GuiSmokeSceneReasoningSupport.BuildSemanticGoal(phase, observer, reasoningMode),
            observer.Summary,
            knownRecipes,
            eventKnowledgeCandidates,
            combatCardKnowledge,
            Program.BuildAllowedActionsCore(phase, observer, combatCardKnowledge, effectiveScreenshotPath, serializedHistory, analysisContext),
            serializedHistory,
            GuiSmokePromptContractSupport.BuildFailureModeHintCoreWithContext(phase, observer, combatCardKnowledge, effectiveScreenshotPath, serializedHistory, analysisContext),
            GuiSmokeSceneReasoningSupport.BuildDecisionRiskHint(phase, observer, firstSeenScene, reasoningMode));
    }
}
