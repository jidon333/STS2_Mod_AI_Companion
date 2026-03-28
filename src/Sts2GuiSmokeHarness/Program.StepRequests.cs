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
using static GuiSmokeRewardMapEvidenceSupport;
using static ObserverScreenProvenance;

internal static partial class Program
{
    static List<T> ReadNdjsonFixture<T>(string path)
    {
        var entries = new List<T>();
        if (!File.Exists(path))
        {
            return entries;
        }

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var entry = JsonSerializer.Deserialize<T>(line, GuiSmokeShared.JsonOptions);
            if (entry is not null)
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

    static IReadOnlyList<GuiSmokeHistoryEntry> BuildSerializedStepHistory(
        GuiSmokePhase phase,
        IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return phase == GuiSmokePhase.HandleCombat
            ? HandleCombatContextSupport.BuildSerializedHistoryWindow(history)
            : history.TakeLast(5).ToArray();
    }

    static GuiSmokeStepAnalysisContext CreateStepAnalysisContext(
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
        ICanonicalNonCombatSceneState? canonicalNonCombatScene = null;
        var canonicalNonCombatSceneComputed = false;

        ReconstructedHandleCombatContext GetCombatContext()
            => combatContext ??= HandleCombatContextSupport.Reconstruct(history);

        PendingCombatSelection? GetPendingSelection()
        {
            if (!pendingSelectionComputed)
            {
                pendingSelection = CombatRuntimeStateSupport.ResolvePendingSelection(observer.Summary, combatCardKnowledge, GetCombatContext().PendingSelection);
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

        CombatBarrierEvaluation GetCombatBarrierEvaluation()
            => combatBarrierEvaluation ??= CombatBarrierSupport.Evaluate(
                history,
                observer,
                GetCombatContext(),
                GetRuntimeCombatState(),
                GetCombatAnalysis(),
                CombatEligibilitySupport.HasSelectedNonEnemyConfirmEvidence(observer.Summary, combatCardKnowledge, GetCombatAnalysis(), GetPendingSelection()),
                CanResolveEnemyTargetFromStateAnalysis(observer, combatCardKnowledge, GetCombatAnalysis(), GetPendingSelection()),
                CombatEligibilitySupport.IsCombatPlayerActionWindowClosed(observer.Summary));

        RewardSceneState GetRewardScene()
            => rewardScene ??= AutoDecisionProvider.BuildRewardSceneState(observer, windowBounds, history, screenshotPath);

        EventSceneState GetEventScene()
            => eventScene ??= AutoDecisionProvider.BuildEventSceneState(observer, windowBounds, history, screenshotPath);

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

            var combatScreenVisible = MatchesCompatibilityScreen(observer, "combat");
            if (!combatScreenVisible)
            {
                return false;
            }

            var eventScene = GetEventScene();
            if (RewardObserverSignals.IsTerminalRunBoundary(observer.Summary)
                || CardSelectionObserverSignals.TryGetState(observer.Summary) is not null
                || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary)
                || ShopObserverSignals.IsShopAuthorityActive(observer.Summary)
                || RewardObserverSignals.IsRewardAuthorityActive(observer.Summary)
                || GuiSmokeNonCombatContractSupport.HasRestSiteAuthority(observer.Summary)
                || eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.Active
                || eventScene.EventForegroundOwned && eventScene.HasExplicitProgression
                || LooksLikeInspectOverlayState(observer))
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
            () => BuildRewardMapLayerState(observer.Summary, windowBounds),
            () =>
            {
                var rewardMapLayer = BuildRewardMapLayerState(observer.Summary, windowBounds);
                return rewardMapLayer.RewardBackNavigationAvailable || LooksLikeRewardBackNavigationAffordance(observer.Summary, screenshotPath);
            },
            () => HasScreenshotClaimableRewardEvidence(observer.Summary, screenshotPath),
            () => GuiSmokeMapOverlayHeuristics.BuildState(observer, windowBounds, screenshotPath),
            GetRewardScene,
            GetEventScene,
            GetCanonicalNonCombatScene,
            GetCombatContext,
            GetPendingSelection,
            GetRuntimeCombatState,
            GetCombatAnalysis,
            GetCombatHandAnalysis,
            () => CombatEligibilitySupport.IsCombatPlayerActionWindowClosed(observer.Summary),
            () => CombatEligibilitySupport.HasSelectedNonEnemyConfirmEvidence(observer.Summary, combatCardKnowledge, GetCombatAnalysis(), GetPendingSelection()),
            () => CanResolveEnemyTargetFromStateAnalysis(observer, combatCardKnowledge, GetCombatAnalysis(), GetPendingSelection()),
            GetCombatBarrierEvaluation);
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

    static GuiSmokeStepRequest CreateStepRequest(
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
        var serializedHistory = analysisContext?.History ?? BuildSerializedStepHistory(phase, history);
        var combatCardKnowledge = analysisContext?.CombatCardKnowledge ?? LoadCombatCardKnowledge(workspaceRoot, observer);
        var sceneSignature = sceneContext?.SceneSignature ?? ComputeSceneSignatureCore(screenshotPath, observer, phase, analysisContext);
        var useAuthorityFastPath = analysisContext?.UseAuthorityFastPath == true;
        var firstSeenScene = sceneContext?.FirstSeenScene ?? !HasSceneSignatureHistory(sessionRoot, sceneSignature);
        var reasoningMode = sceneContext?.ReasoningMode ?? DetermineReasoningMode(phase, observer, firstSeenScene);
        var knownRecipes = sceneContext?.KnownRecipes
            ?? (useAuthorityFastPath
                ? Array.Empty<KnownRecipeHint>()
                : LoadKnownRecipes(sessionRoot, sceneSignature, phase.ToString()));
        var eventKnowledgeCandidates = useAuthorityFastPath
            ? Array.Empty<EventKnowledgeCandidate>()
            : LoadEventKnowledgeCandidates(workspaceRoot, observer, reasoningMode);
        return new GuiSmokeStepRequest(
            runId,
            scenarioId,
            stepIndex,
            phase.ToString(),
            BuildGoal(phase),
            DateTimeOffset.UtcNow,
            screenshotPath,
            new WindowBounds(window.Bounds.X, window.Bounds.Y, window.Bounds.Width, window.Bounds.Height),
            sceneSignature,
            attemptId,
            attemptOrdinal,
            3,
            firstSeenScene,
            reasoningMode,
            BuildSemanticGoal(phase, observer, reasoningMode),
            observer.Summary,
            knownRecipes,
            eventKnowledgeCandidates,
            combatCardKnowledge,
            BuildAllowedActionsCore(phase, observer, combatCardKnowledge, screenshotPath, serializedHistory, analysisContext),
            serializedHistory,
            BuildFailureModeHintCoreWithContext(phase, observer, combatCardKnowledge, screenshotPath, serializedHistory, analysisContext),
            BuildDecisionRiskHint(phase, observer, firstSeenScene, reasoningMode));
    }
}
