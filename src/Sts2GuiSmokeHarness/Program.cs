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
    static async Task<int> Main(string[] args)
    {
        var command = args.Length == 0 ? "help" : args[0].ToLowerInvariant();
        var options = ParseOptions(args.Skip(1).ToArray());
        var workspaceRoot = Directory.GetCurrentDirectory();
        var configPath = ResolveConfigPath(options, workspaceRoot);
        var loadResult = ConfigurationLoader.LoadFromFile(configPath);
        var configuration = ApplyPathOverrides(loadResult.Configuration, options);

        try
        {
            switch (command)
            {
                case "run":
                    return await RunScenarioAsync(configuration, workspaceRoot, options).ConfigureAwait(false);

                case "inspect-run":
                    return InspectRun(options, workspaceRoot);

                case "inspect-session":
                    return InspectSession(options, workspaceRoot);

                case "replay-step":
                    return ReplayStep(options, workspaceRoot);

                case "replay-test":
                    return ReplayGoldenScenes(options, workspaceRoot);

                case "replay-parity-test":
                    return ReplayParityScenes(options, workspaceRoot);

                case "self-test":
                    RunSelfTest();
                    Console.WriteLine("GUI smoke harness self-test passed.");
                    return 0;

                default:
                    WriteUsage();
                    return 0;
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

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
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
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

        bool ComputeUseCombatFastPath()
        {
            bool HasExplicitEventForeground()
            {
                var eventAuthority = string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(observer.VisibleScreen, "event", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(observer.ChoiceExtractorPath, "event", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(observer.ChoiceExtractorPath, "room-event", StringComparison.OrdinalIgnoreCase);
                if (!eventAuthority)
                {
                    return false;
                }

                return observer.ActionNodes.Any(static node =>
                           node.Actionable
                           && !string.IsNullOrWhiteSpace(node.ScreenBounds)
                           && (node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
                               || node.Label.Contains("Proceed", StringComparison.OrdinalIgnoreCase)
                               || node.Label.Contains("Continue", StringComparison.OrdinalIgnoreCase)
                               || node.Label.Contains("진행", StringComparison.OrdinalIgnoreCase)
                               || node.Label.Contains("계속", StringComparison.OrdinalIgnoreCase)))
                       || observer.Choices.Any(static choice =>
                           !string.IsNullOrWhiteSpace(choice.ScreenBounds)
                           && (string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)));
            }

            if (phase != GuiSmokePhase.HandleCombat || observer.InCombat != true)
            {
                return false;
            }

            var combatScreenVisible = string.Equals(observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(observer.VisibleScreen, "combat", StringComparison.OrdinalIgnoreCase);
            if (!combatScreenVisible)
            {
                return false;
            }

            if (RewardObserverSignals.IsTerminalRunBoundary(observer.Summary)
                || CardSelectionObserverSignals.TryGetState(observer.Summary) is not null
                || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary)
                || ShopObserverSignals.IsShopAuthorityActive(observer.Summary)
                || RewardObserverSignals.IsRewardAuthorityActive(observer.Summary)
                || HasRestSiteAuthority(observer.Summary)
                || GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer)
                || HasExplicitEventForeground()
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
                   || HasExplicitRewardProgressionAffordance(observer.Summary);
        }

        return new GuiSmokeStepAnalysisContext(
            phase,
            observer,
            screenshotPath,
            history,
            combatCardKnowledge,
            ComputeUseCombatFastPath,
            ComputeUseRewardFastPath,
            () => BuildRewardMapLayerStateForObserver(observer.Summary, null),
            () =>
            {
                var rewardMapLayer = BuildRewardMapLayerStateForObserver(observer.Summary, null);
                return rewardMapLayer.RewardBackNavigationAvailable || LooksLikeRewardBackNavigationAffordance(observer.Summary, screenshotPath);
            },
            () => HasScreenshotClaimableRewardEvidence(observer.Summary, screenshotPath),
            () => GuiSmokeMapOverlayHeuristics.BuildState(observer, null, screenshotPath),
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

    static GuiSmokeStepAnalysisContext CreateRequestAnalysisContext(GuiSmokeStepRequest request)
    {
        return CreateStepAnalysisContext(
            Enum.Parse<GuiSmokePhase>(request.Phase, ignoreCase: true),
            new ObserverState(request.Observer, null, null, request.Observer.LastEventsTail?.ToArray() ?? Array.Empty<string>()),
            request.ScreenshotPath,
            request.History,
            request.CombatCardKnowledge);
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

    static string ComputeSceneSignature(string screenshotPath, ObserverState observer, GuiSmokePhase phase)
    {
        return ComputeSceneSignatureCore(screenshotPath, observer, phase, null);
    }

    static string ComputeSceneSignatureCore(string screenshotPath, ObserverState observer, GuiSmokePhase phase, GuiSmokeStepAnalysisContext? analysisContext)
    {
        var context = analysisContext ?? CreateStepAnalysisContext(phase, observer, screenshotPath, Array.Empty<GuiSmokeHistoryEntry>(), Array.Empty<CombatCardKnowledgeHint>());
        if (context.UseCombatFastPath)
        {
            var combatRuntimeState = context.RuntimeCombatState;
            var combatAnalysis = context.CombatAnalysis;
            var combatTags = new List<string>(capacity: 10)
            {
                $"phase:{phase.ToString().ToLowerInvariant()}",
                $"screen:{(observer.CurrentScreen ?? "unknown").Trim().ToLowerInvariant()}",
                $"visible:{(observer.VisibleScreen ?? "unknown").Trim().ToLowerInvariant()}",
                $"encounter:{(observer.EncounterKind ?? "none").Trim().ToLowerInvariant()}",
                $"ready:{(observer.SceneReady?.ToString() ?? "unknown").ToLowerInvariant()}",
                $"stability:{(observer.SceneStability ?? "unknown").Trim().ToLowerInvariant()}",
                "combat:fast-path",
                $"combat-targeting:{((combatRuntimeState.TargetingInProgress == true || combatAnalysis.HasTargetArrow) ? "active" : "inactive")}",
                $"combat-hittable:{(combatRuntimeState.HittableEnemyCount?.ToString(CultureInfo.InvariantCulture) ?? "unknown")}",
            };

            if (context.PendingCombatSelection is { } pendingSelection)
            {
                combatTags.Add($"combat-selection:{pendingSelection.Kind.ToString().ToLowerInvariant()}");
                combatTags.Add($"combat-slot:{pendingSelection.SlotIndex.ToString(CultureInfo.InvariantCulture)}");
            }
            else if (combatAnalysis.HasSelectedCard)
            {
                combatTags.Add($"combat-selection:{combatAnalysis.SelectedCardKind.ToString().ToLowerInvariant()}");
            }

            if (combatRuntimeState.KeepsCardPlayOpen)
            {
                combatTags.Add("combat-play-open");
            }
            return string.Join("|", combatTags);
        }

        if (phase == GuiSmokePhase.HandleRewards && context.UseRewardFastPath)
        {
            return ComputeRewardFastPathSceneSignature(observer, context);
        }

        var cardSelectionState = CardSelectionObserverSignals.TryGetState(observer.Summary);
        var rewardMapLayer = context.RewardMapLayerState;
        var rewardBackNavigationAvailable = context.RewardBackNavigationAvailable;
        var claimableRewardPresent = context.ClaimableRewardPresent;
        var mapOverlayState = context.MapOverlayState;
        var tags = new List<string>(capacity: 10)
        {
            $"phase:{phase.ToString().ToLowerInvariant()}",
            $"screen:{(observer.CurrentScreen ?? "unknown").Trim().ToLowerInvariant()}",
            $"visible:{(observer.VisibleScreen ?? "unknown").Trim().ToLowerInvariant()}",
            $"encounter:{(observer.EncounterKind ?? "none").Trim().ToLowerInvariant()}",
            $"ready:{(observer.SceneReady?.ToString() ?? "unknown").ToLowerInvariant()}",
            $"stability:{(observer.SceneStability ?? "unknown").Trim().ToLowerInvariant()}",
        };

        if (cardSelectionState is not null)
        {
            tags.Add($"card-selection:{cardSelectionState.ScreenType}");
            tags.Add($"card-selection-selected:{cardSelectionState.SelectedCount.ToString(CultureInfo.InvariantCulture)}");
            if (cardSelectionState.PreviewVisible)
            {
                tags.Add($"card-selection-preview:{cardSelectionState.PreviewMode ?? "visible"}");
            }
        }

        if (LooksLikeTreasureState(observer.Summary))
        {
            tags.Add("room:treasure");
        }

        if (LooksLikeRestSiteState(observer.Summary))
        {
            tags.Add("room:rest-site");
        }

        var shopState = ShopObserverSignals.TryGetState(observer.Summary);
        if (shopState is { ForegroundOwned: true })
        {
            tags.Add("room:shop");
        }
        else if (shopState is { RoomVisible: true })
        {
            tags.Add("room-visible:shop");
        }

        if (!ShouldSuppressRoomSubstateHeuristics(phase, observer))
        {
            if (HasExplicitRestSiteChoiceAuthority(observer, screenshotPath))
            {
                tags.Add("layer:rest-site-foreground");
            }

            if (rewardMapLayer.RewardForegroundOwned)
            {
                tags.Add("layer:reward-foreground");
            }
            else if (rewardMapLayer.RewardTeardownInProgress)
            {
                tags.Add("layer:reward-teardown");
            }

            if (rewardMapLayer.MapContextVisible)
            {
                tags.Add("layer:map-background");
            }

            if (mapOverlayState.ForegroundVisible)
            {
                tags.Add("layer:map-overlay-foreground");
            }

            if (mapOverlayState.EventBackgroundPresent)
            {
                tags.Add("layer:event-background");
            }

            if (rewardBackNavigationAvailable)
            {
                tags.Add("layer:reward-back-nav");
            }

            if (rewardMapLayer.StaleRewardChoicePresent)
            {
                tags.Add("stale:reward-choice");
            }

            if (rewardMapLayer.StaleRewardBoundsPresent)
            {
                tags.Add("stale:reward-bounds");
            }

            if (mapOverlayState.StaleEventChoicePresent)
            {
                tags.Add("stale:event-choice");
            }

            if (claimableRewardPresent)
            {
                tags.Add("reward:claimable");
            }

            if (mapOverlayState.MapBackNavigationAvailable)
            {
                tags.Add("map-back-navigation-available");
            }

            if (mapOverlayState.CurrentNodeArrowVisible)
            {
                tags.Add("current-node-arrow-visible");
            }

            if (mapOverlayState.ReachableNodeCandidatePresent)
            {
                tags.Add("reachable-node-candidate-present");
            }

            if (mapOverlayState.ExportedReachableNodeCandidatePresent)
            {
                tags.Add("exported-reachable-node-present");
            }

            if (LooksLikeInspectOverlayState(observer))
            {
                tags.Add("substate:inspect-overlay");
            }

            if (GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer))
            {
                tags.Add("substate:reward-choice");
            }

            if (GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer))
            {
                tags.Add("substate:colorless-card-choice");
            }

            if (!mapOverlayState.ForegroundVisible
                && HasStrongMapTransitionEvidence(observer)
                && !string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
                && !GuiSmokeRewardSceneSignals.ShouldPreferRewardProgressionOverMapFallback(observer)
                && !GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer))
            {
                tags.Add("substate:map-transition");
            }

            var mapAnalysis = AutoMapAnalyzer.Analyze(screenshotPath);
            if (mapAnalysis.HasCurrentArrow)
            {
                if (!mapOverlayState.ForegroundVisible)
                {
                    tags.Add(rewardMapLayer.RewardPanelVisible && GuiSmokeRewardSceneSignals.ShouldPreferRewardProgressionOverMapFallback(observer)
                        ? "contamination:map-arrow"
                        : GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer)
                            ? "contamination:map-arrow"
                            : "visible:map-arrow");
                }
            }

            if (GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer))
            {
                tags.Add("layer:event-foreground");
            }
        }

        if (rewardMapLayer.TerminalRunBoundary)
        {
            tags.Add("terminal-run-boundary");
        }

        var screenshotFingerprint = ComputeFileFingerprint(screenshotPath);
        tags.Add($"shot:{screenshotFingerprint[..Math.Min(12, screenshotFingerprint.Length)]}");
        return string.Join("|", tags);
    }

    static string ComputeRewardFastPathSceneSignature(ObserverState observer, GuiSmokeStepAnalysisContext context)
    {
        var rewardState = RewardObserverSignals.TryGetState(observer.Summary);
        var cardSelectionState = CardSelectionObserverSignals.TryGetState(observer.Summary);
        var rewardTags = new List<string>(capacity: 12)
        {
            "phase:handlerewards",
            $"screen:{(observer.CurrentScreen ?? "unknown").Trim().ToLowerInvariant()}",
            $"visible:{(observer.VisibleScreen ?? "unknown").Trim().ToLowerInvariant()}",
            $"encounter:{(observer.EncounterKind ?? "none").Trim().ToLowerInvariant()}",
            $"ready:{(observer.SceneReady?.ToString() ?? "unknown").ToLowerInvariant()}",
            $"stability:{(observer.SceneStability ?? "unknown").Trim().ToLowerInvariant()}",
            "reward:fast-path",
        };

        if (cardSelectionState is not null)
        {
            rewardTags.Add($"card-selection:{cardSelectionState.ScreenType}");
            rewardTags.Add($"card-selection-selected:{cardSelectionState.SelectedCount.ToString(CultureInfo.InvariantCulture)}");
            if (cardSelectionState.PreviewVisible)
            {
                rewardTags.Add($"card-selection-preview:{cardSelectionState.PreviewMode ?? "visible"}");
            }
        }

        if (rewardState is not null)
        {
            rewardTags.Add(rewardState.ForegroundOwned ? "reward-owner:foreground" : rewardState.TeardownInProgress ? "reward-owner:teardown" : "reward-owner:visible");
            if (rewardState.RewardIsCurrentActiveScreen)
            {
                rewardTags.Add("reward-current-active");
            }

            if (rewardState.MapIsCurrentActiveScreen)
            {
                rewardTags.Add("map-current-active");
            }

            if (rewardState.TerminalRunBoundary)
            {
                rewardTags.Add("terminal-run-boundary");
            }

            if (rewardState.ProceedVisible)
            {
                rewardTags.Add(rewardState.ProceedEnabled ? "reward-proceed:enabled" : "reward-proceed:visible");
            }
        }

        if (HasExplicitRewardProgressionAffordance(observer.Summary))
        {
            rewardTags.Add("reward-explicit-progression");
        }

        if (GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer))
        {
            rewardTags.Add("substate:colorless-card-choice");
        }
        else if (GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer))
        {
            rewardTags.Add("substate:reward-choice");
        }

        return string.Join("|", rewardTags);
    }

    static IReadOnlyList<KnownRecipeHint> LoadKnownRecipes(string sessionRoot, string sceneSignature, string phase)
    {
        var recipesPath = Path.Combine(sessionRoot, "scene-recipes.ndjson");
        if (!File.Exists(recipesPath))
        {
            return Array.Empty<KnownRecipeHint>();
        }

        var hints = new List<KnownRecipeHint>();
        foreach (var line in File.ReadLines(recipesPath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            SceneRecipeEntry? entry;
            try
            {
                entry = JsonSerializer.Deserialize<SceneRecipeEntry>(line, GuiSmokeShared.JsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (entry is null
                || !string.Equals(entry.SceneSignature, sceneSignature, StringComparison.Ordinal)
                || !string.Equals(entry.Phase, phase, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            hints.Add(new KnownRecipeHint(
                entry.SceneSignature,
                entry.Phase,
                entry.ActionKind,
                entry.TargetLabel,
                entry.ExpectedScreen,
                entry.Reason));
        }

        return hints
            .TakeLast(3)
            .ToArray();
    }

    static bool HasSceneSignatureHistory(string sessionRoot, string sceneSignature)
    {
        foreach (var fileName in new[] { "scene-recipes.ndjson", "unknown-scenes.ndjson" })
        {
            var path = Path.Combine(sessionRoot, fileName);
            if (!File.Exists(path))
            {
                continue;
            }

            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains(sceneSignature, StringComparison.Ordinal))
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }

    static string DetermineReasoningMode(GuiSmokePhase phase, ObserverState observer, bool firstSeenScene)
    {
        if (phase != GuiSmokePhase.HandleEvent)
        {
            return "tactical";
        }

        var foregroundOwner = NonCombatForegroundOwnership.Resolve(observer);
        if (foregroundOwner is NonCombatForegroundOwner.Map
            or NonCombatForegroundOwner.Reward
            or NonCombatForegroundOwner.Shop
            or NonCombatForegroundOwner.RestSite)
        {
            return "tactical";
        }

        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, null);
        if (eventScene.EventForegroundOwned && eventScene.ExplicitAction == EventExplicitActionKind.EventChoice)
        {
            return (firstSeenScene
                    || string.Equals(observer.CurrentScreen, "unknown", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(observer.VisibleScreen, "unknown", StringComparison.OrdinalIgnoreCase)
                    || observer.Summary.CurrentChoices.Count > 0)
                ? "semantic"
                : "tactical";
        }

        if (AncientEventObserverSignals.HasForegroundAuthority(observer.Summary)
            || EventProceedObserverSignals.HasExplicitEventProceedAuthority(observer, null))
        {
            return "tactical";
        }

        if (firstSeenScene
            || string.Equals(observer.CurrentScreen, "unknown", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.VisibleScreen, "unknown", StringComparison.OrdinalIgnoreCase)
            || observer.Summary.CurrentChoices.Count > 0)
        {
            return "semantic";
        }

        return "tactical";
    }

    static string? BuildSemanticGoal(GuiSmokePhase phase, ObserverState observer, string reasoningMode)
    {
        if (!string.Equals(reasoningMode, "semantic", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return phase switch
        {
            GuiSmokePhase.HandleEvent => LooksLikeTreasureState(observer.Summary)
                ? "Interpret the room state, identify whether the chest is unopened, opened, or in reward follow-up, and choose the safest progress action."
                : "Interpret the event screen, infer what each visible option means, and choose a low-risk action that keeps the run progressing.",
            GuiSmokePhase.HandleCombat => "Interpret the combat board, hand, and targets before committing to the next action.",
            _ => "Interpret the current screen semantically before acting.",
        };
    }

    static string? BuildDecisionRiskHint(GuiSmokePhase phase, ObserverState observer, bool firstSeenScene, string reasoningMode)
    {
        var hints = new List<string>();
        if (observer.SceneReady == false)
        {
            hints.Add("scene-not-ready");
        }

        if (!string.Equals(observer.SceneStability, "stable", StringComparison.OrdinalIgnoreCase))
        {
            hints.Add($"scene-stability:{observer.SceneStability ?? "unknown"}");
        }

        if (firstSeenScene)
        {
            hints.Add("first-seen-scene");
        }

        if (string.Equals(reasoningMode, "semantic", StringComparison.OrdinalIgnoreCase))
        {
            hints.Add("semantic-scene-ambiguity");
        }

        if (phase == GuiSmokePhase.HandleCombat && observer.PlayerEnergy is null)
        {
            hints.Add("observer-missing-energy");
        }

        return hints.Count == 0
            ? null
            : string.Join(", ", hints);
    }

    static IReadOnlyList<EventKnowledgeCandidate> LoadEventKnowledgeCandidates(string workspaceRoot, ObserverState observer, string reasoningMode)
    {
        if (!string.Equals(reasoningMode, "semantic", StringComparison.OrdinalIgnoreCase))
        {
            return Array.Empty<EventKnowledgeCandidate>();
        }

        var choiceKeys = observer.Summary.CurrentChoices
            .Concat(observer.Summary.Choices.Select(static choice => choice.Label))
            .Where(static label => !string.IsNullOrWhiteSpace(label))
            .Select(NormalizeKnowledgeKey)
            .Where(static label => label.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (choiceKeys.Length == 0)
        {
            return Array.Empty<EventKnowledgeCandidate>();
        }

        var matches = new List<(AssistantEventKnowledge Event, int Score, string Reason)>();
        foreach (var entry in AssistantKnowledgeCatalog.LoadEvents(workspaceRoot))
        {
            var optionKeys = entry.Options
                .Select(static option => NormalizeKnowledgeKey(option.Label))
                .Where(static label => label.Length > 0)
                .ToArray();
            if (optionKeys.Length == 0)
            {
                continue;
            }

            var score = choiceKeys.Count(choiceKey => optionKeys.Contains(choiceKey, StringComparer.Ordinal));
            if (score <= 0)
            {
                continue;
            }

            matches.Add((entry, score, $"matched-options:{score}"));
        }

        return matches
            .OrderByDescending(static match => match.Score)
            .ThenBy(static match => match.Event.Title, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select(match => new EventKnowledgeCandidate(
                match.Event.Id,
                match.Event.Title,
                match.Reason,
                match.Event.Options
                    .Take(5)
                    .Select(static option => new EventOptionKnowledgeCandidate(
                        option.Label,
                        option.Description,
                        option.OptionKey))
                    .ToArray()))
            .ToArray();
    }

    static IReadOnlyList<CombatCardKnowledgeHint> LoadCombatCardKnowledge(string workspaceRoot, ObserverState observer)
    {
        if (observer.CombatHand.Count == 0)
        {
            return Array.Empty<CombatCardKnowledgeHint>();
        }

        var cards = AssistantKnowledgeCatalog.LoadCards(workspaceRoot);
        var hints = new List<CombatCardKnowledgeHint>(observer.CombatHand.Count);
        foreach (var handCard in observer.CombatHand)
        {
            var matchKeys = BuildCombatKnowledgeLookupKeys(handCard.Name);
            if (matchKeys.Count == 0)
            {
                continue;
            }

            var match = cards
                .Select(card => new
                {
                    Card = card,
                    BestMatchLength = matchKeys
                        .Where(key => card.MatchKeys.Contains(key, StringComparer.Ordinal))
                        .DefaultIfEmpty(string.Empty)
                        .Max(static key => key.Length),
                })
                .Where(static entry => entry.BestMatchLength > 0)
                .OrderByDescending(static entry => entry.BestMatchLength)
                .ThenBy(static entry => entry.Card.Id, StringComparer.OrdinalIgnoreCase)
                .Select(static entry => entry.Card)
                .FirstOrDefault();
            if (match is null)
            {
                continue;
            }

            hints.Add(new CombatCardKnowledgeHint(
                handCard.SlotIndex,
                handCard.Name,
                handCard.Type ?? match.Type,
                match.Target,
                handCard.Cost ?? match.Cost,
                "assistant/cards.json"));
        }

        return hints;
    }

    static IReadOnlyList<string> BuildCombatKnowledgeLookupKeys(string? cardName)
    {
        if (string.IsNullOrWhiteSpace(cardName))
        {
            return Array.Empty<string>();
        }

        var keys = new List<string>();
        var normalizedName = NormalizeKnowledgeKey(cardName);
        AddCombatKnowledgeLookupKey(keys, normalizedName);

        var parts = cardName
            .Split(new[] { '.', '_', '-', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeKnowledgeKey)
            .Where(static part => part.Length > 0)
            .ToArray();
        if (parts.Length == 0)
        {
            return keys;
        }

        var trimmedParts = parts[0] == "card"
            ? parts[1..]
            : parts;
        if (trimmedParts.Length == 0)
        {
            return keys;
        }

        AddCombatKnowledgeLookupKey(keys, string.Concat(trimmedParts));
        AddCombatKnowledgeLookupKey(keys, trimmedParts[0]);
        if (trimmedParts.Length > 1 && IsCombatClassSuffix(trimmedParts[^1]))
        {
            AddCombatKnowledgeLookupKey(keys, string.Concat(trimmedParts[..^1]));
        }

        foreach (var part in trimmedParts)
        {
            AddCombatKnowledgeLookupKey(keys, part);
        }

        return keys;
    }

    static void AddCombatKnowledgeLookupKey(List<string> keys, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate)
            || keys.Contains(candidate, StringComparer.Ordinal))
        {
            return;
        }

        keys.Add(candidate);
    }

    static bool IsCombatClassSuffix(string value)
    {
        return value is "ironclad" or "silent" or "defect" or "watcher" or "colorless" or "status" or "curse";
    }

    static string NormalizeKnowledgeKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        Span<char> buffer = stackalloc char[value.Length];
        var length = 0;
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                buffer[length] = char.ToLowerInvariant(character);
                length += 1;
            }
        }

        return length == 0
            ? string.Empty
            : new string(buffer[..length]);
    }

    static string[] GetAllowedActions(GuiSmokePhase phase, ObserverState observer)
    {
        return BuildAllowedActions(phase, observer, Array.Empty<CombatCardKnowledgeHint>(), null, Array.Empty<GuiSmokeHistoryEntry>());
    }

    static string[] BuildAllowedActions(
        GuiSmokePhase phase,
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return BuildAllowedActionsCore(phase, observer, combatCardKnowledge, screenshotPath, history, null);
    }

    static string[] BuildAllowedActionsCore(
        GuiSmokePhase phase,
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        GuiSmokeStepAnalysisContext? analysisContext)
    {
        var context = analysisContext ?? CreateStepAnalysisContext(phase, observer, screenshotPath, history, combatCardKnowledge);
        if (context.UseCombatFastPath)
        {
            return GetCombatAllowedActions(observer, combatCardKnowledge, screenshotPath, history, context);
        }

        if (phase == GuiSmokePhase.HandleRewards && context.UseRewardFastPath)
        {
            return BuildRewardAllowedActionsFastPath(observer, context);
        }

        var cardSelectionState = CardSelectionObserverSignals.TryGetState(observer.Summary);
        var treasureState = TreasureRoomObserverSignals.TryGetState(observer.Summary);
        var forceEventProgressionAfterCardSelection = ShouldPrioritizeExplicitEventProgressionAfterCardSelectionForAllowlist(observer, history);
        var explicitEventProceedAuthority = EventProceedObserverSignals.HasExplicitEventProceedAuthority(observer.Summary, null);
        var rewardMapLayer = context.RewardMapLayerState;
        var explicitRewardProgressionPresent = observer.Summary.Choices.Any(choice => IsCurrentRewardProgressionChoiceForObserver(choice, null))
                                              || observer.Summary.ActionNodes.Any(node => IsCurrentRewardProgressionNodeForObserver(node, null));
        var rewardForegroundVisible = rewardMapLayer.RewardForegroundOwned;
        var rewardBackNavigationAvailable = context.RewardBackNavigationAvailable;
        var claimableRewardPresent = context.ClaimableRewardPresent;
        var mapOverlayState = context.MapOverlayState;
        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, null, history, screenshotPath);
        var mapForegroundOwnership = MapForegroundReconciliation.HasMapForegroundOwnership(observer, history);
        var ancientMapOwner = AncientEventObserverSignals.IsMapForegroundOwner(observer.Summary);
        var ancientMapSurfacePending = AncientEventObserverSignals.IsMapSurfacePending(observer.Summary);
        var explicitRestSiteChoiceAuthority = HasExplicitRestSiteChoiceAuthority(observer, screenshotPath);
        if (phase == GuiSmokePhase.WaitRunLoad && GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(observer, out var postRunLoadPhase))
        {
            return BuildAllowedActions(postRunLoadPhase, observer, combatCardKnowledge, screenshotPath, history);
        }

        if (phase == GuiSmokePhase.WaitRunLoad && WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(observer.Summary))
        {
            return BuildAllowedActions(GuiSmokePhase.EnterRun, observer, combatCardKnowledge, screenshotPath, history);
        }

        if (phase == GuiSmokePhase.Embark && GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(observer, out var observedPhase))
        {
            return BuildAllowedActions(observedPhase, observer, combatCardKnowledge, screenshotPath, history);
        }

        return phase switch
        {
            GuiSmokePhase.EnterRun => new[] { "click continue", "click singleplayer", "click normal mode", "wait" },
            GuiSmokePhase.WaitRunLoad => new[] { "wait" },
            GuiSmokePhase.ChooseCharacter => new[] { "click ironclad", "click character confirm", "wait" },
            GuiSmokePhase.Embark => new[] { "click embark", "click character confirm", "wait" },
            GuiSmokePhase.HandleRewards
                => BuildRewardAllowedActions(observer, context),
            GuiSmokePhase.ChooseFirstNode when ancientMapOwner && ancientMapSurfacePending
                => new[] { "wait" },
            GuiSmokePhase.ChooseFirstNode when ancientMapOwner
                => new[] { "click exported reachable node", "click visible map advance", "wait" },
            GuiSmokePhase.ChooseFirstNode when explicitRestSiteChoiceAuthority
                => BuildExplicitRestSiteAllowedActions(observer.Summary),
            GuiSmokePhase.ChooseFirstNode when LooksLikeRestSiteProceedState(observer.Summary)
                => new[] { "click proceed", "wait" },
            GuiSmokePhase.ChooseFirstNode when treasureState is { RoomDetected: true }
                => TreasureRoomObserverSignals.BuildAllowedActions(treasureState),
            GuiSmokePhase.ChooseFirstNode when mapOverlayState.ForegroundVisible
                => mapOverlayState.MapBackNavigationAvailable
                    ? new[] { "click exported reachable node", "click first reachable node", "click map back", "wait" }
                    : new[] { "click exported reachable node", "click first reachable node", "wait" },
            GuiSmokePhase.ChooseFirstNode when mapForegroundOwnership
                => new[] { "click exported reachable node", "click visible map advance", "wait" },
            GuiSmokePhase.ChooseFirstNode when LooksLikeRestSiteState(observer.Summary)
                => new[] { "click smith card", "click smith confirm", "wait" },
            GuiSmokePhase.ChooseFirstNode => new[] { "click exported reachable node", "click visible map advance", "wait" },
            GuiSmokePhase.HandleEvent when LooksLikeInspectOverlayState(observer)
                => new[] { "press escape", "click inspect overlay close", "wait" },
            GuiSmokePhase.HandleEvent when cardSelectionState is not null
                => BuildCardSelectionAllowedActions(cardSelectionState),
            GuiSmokePhase.HandleEvent when eventScene.RewardSubstateActive
                => BuildRewardAllowedActionsFromState(observer, eventScene.RewardScene),
            GuiSmokePhase.HandleEvent when eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.ReleasePending
                => new[] { "wait" },
            GuiSmokePhase.HandleEvent when rewardMapLayer.RewardPanelVisible && (claimableRewardPresent || GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer))
                => new[] { "click colorless card choice", "click reward skip", "click proceed", "press escape", "wait" },
            GuiSmokePhase.HandleEvent when rewardMapLayer.RewardPanelVisible && (claimableRewardPresent || GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer))
                => new[] { "click reward card choice", "click reward choice", "click reward skip", "click proceed", rewardBackNavigationAvailable ? "click reward back" : "press escape", "wait" },
            GuiSmokePhase.HandleEvent when rewardMapLayer.RewardPanelVisible && GuiSmokeRewardSceneSignals.ShouldPreferRewardProgressionOverMapFallback(observer)
                => new[] { "click reward", "click reward skip", "click proceed", "wait" },
            GuiSmokePhase.HandleEvent when ancientMapOwner && ancientMapSurfacePending
                => new[] { "wait" },
            GuiSmokePhase.HandleEvent when AncientEventObserverSignals.IsDialogueActive(observer.Summary)
                => new[] { "click ancient dialogue advance", "wait" },
            GuiSmokePhase.HandleEvent when AncientEventObserverSignals.HasExplicitCompletionAction(observer.Summary)
                => new[] { "click ancient event completion", "wait" },
            GuiSmokePhase.HandleEvent when AncientEventObserverSignals.HasExplicitOptionSelection(observer.Summary)
                => new[] { "click event choice", "wait" },
            GuiSmokePhase.HandleEvent when forceEventProgressionAfterCardSelection
                => new[] { "click event choice", "click proceed", "wait" },
            GuiSmokePhase.HandleEvent when treasureState is { RoomDetected: true }
                => TreasureRoomObserverSignals.BuildAllowedActions(treasureState),
            GuiSmokePhase.HandleEvent when mapOverlayState.ForegroundVisible && !eventScene.StrongForegroundChoice && !explicitEventProceedAuthority
                => mapOverlayState.MapBackNavigationAvailable
                    ? new[] { "click exported reachable node", "click first reachable node", "click map back", "wait" }
                    : new[] { "click exported reachable node", "click first reachable node", "wait" },
            GuiSmokePhase.HandleEvent when HasStrongMapTransitionEvidence(observer)
                                            && !forceEventProgressionAfterCardSelection
                                            && !GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer)
                => new[] { "click first reachable node", "click visible map advance", "click proceed", "wait" },
            GuiSmokePhase.HandleEvent => new[] { "click event choice", "click proceed", "wait" },
            GuiSmokePhase.WaitEventRelease => new[] { "wait" },
            GuiSmokePhase.HandleShop => BuildShopAllowedActions(observer.Summary, history),
            GuiSmokePhase.HandleCombat => GetCombatAllowedActions(observer, combatCardKnowledge, screenshotPath, history),
            _ => new[] { "wait" },
        };
    }

    static string[] BuildShopAllowedActions(ObserverSummary observer, IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var state = ShopObserverSignals.TryGetState(observer);
        if (state is null)
        {
            return new[] { "wait" };
        }

        if (LooksLikeInspectOverlayState(new ObserverState(observer, null, null, null)))
        {
            return new[] { "press escape", "click inspect overlay close", "wait" };
        }

        if (CardSelectionObserverSignals.TryGetState(observer) is not null)
        {
            return BuildCardSelectionAllowedActions(CardSelectionObserverSignals.TryGetState(observer)!);
        }

        return ShopObserverSignals.BuildAllowedActions(observer, state, alreadyPurchased: ShopObserverSignals.HasRecentPurchase(history));
    }

    static string[] BuildRewardAllowedActionsFastPath(ObserverState observer, GuiSmokeStepAnalysisContext context)
    {
        return BuildRewardAllowedActions(observer, context);
    }

    static string[] BuildRewardAllowedActions(ObserverState observer, GuiSmokeStepAnalysisContext context)
    {
        if (LooksLikeInspectOverlayState(observer))
        {
            return new[] { "press escape", "click inspect overlay close", "wait" };
        }

        var cardSelectionState = CardSelectionObserverSignals.TryGetState(observer.Summary);
        if (cardSelectionState is not null)
        {
            return BuildCardSelectionAllowedActions(cardSelectionState);
        }

        var rewardScene = AutoDecisionProvider.BuildRewardSceneState(observer, null, context.History, context.ScreenshotPath);
        return BuildRewardAllowedActionsFromState(observer, rewardScene);
    }

    static string[] BuildRewardAllowedActionsFromState(ObserverState observer, RewardSceneState rewardScene)
    {
        if (LooksLikeInspectOverlayState(observer))
        {
            return new[] { "press escape", "click inspect overlay close", "wait" };
        }

        var cardSelectionState = CardSelectionObserverSignals.TryGetState(observer.Summary);
        if (cardSelectionState is not null)
        {
            return BuildCardSelectionAllowedActions(cardSelectionState);
        }

        if (rewardScene.LayerState.TerminalRunBoundary || rewardScene.ReleaseToMapPending)
        {
            return new[] { "wait" };
        }

        return rewardScene.ExplicitAction switch
        {
            RewardExplicitActionKind.ColorlessChoice
                => new[] { "click colorless card choice", "click reward skip", "click proceed", "press escape", "wait" },
            RewardExplicitActionKind.CardChoice
                => new[] { "click reward card choice", "click reward choice", "click reward skip", "click proceed", rewardScene.LayerState.RewardBackNavigationAvailable ? "click reward back" : "press escape", "wait" },
            RewardExplicitActionKind.Claim
                => rewardScene.RewardChoiceVisible
                    ? new[] { "click reward card choice", "click reward choice", "click reward skip", "click proceed", rewardScene.LayerState.RewardBackNavigationAvailable ? "click reward back" : "press escape", "wait" }
                    : new[] { "click reward", "click reward skip", "click proceed", "wait" },
            RewardExplicitActionKind.SkipProceed
                => new[] { "click reward skip", "click proceed", "wait" },
            RewardExplicitActionKind.Back
                => new[] { "click reward back", "wait" },
            _ => new[] { "wait" },
        };
    }

    static bool LooksLikeInspectOverlayState(ObserverState observer)
    {
        return observer.CurrentChoices.Any(static label =>
                   label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase))
               || observer.ActionNodes.Any(static node =>
                   node.Label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                   || node.Label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                   || node.Label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
    }

    static string[] BuildCardSelectionAllowedActions(CardSelectionSubtypeState state)
    {
        return state.ScreenType switch
        {
            "reward-pick" => new[] { "reward pick card", "wait" },
            "transform" when CardSelectionObserverSignals.IsConfirmReady(state)
                => new[] { "transform confirm", "wait" },
            "transform" => new[] { "transform select card", "wait" },
            "deck-remove" when CardSelectionObserverSignals.IsConfirmReady(state)
                => new[] { "deck remove confirm", "wait" },
            "deck-remove" => new[] { "deck remove select card", "wait" },
            "upgrade" when CardSelectionObserverSignals.IsConfirmReady(state)
                => new[] { "upgrade confirm", "wait" },
            "upgrade" => new[] { "upgrade select card", "wait" },
            _ => new[] { "wait" },
        };
    }

    static bool ShouldPrioritizeExplicitEventProgressionAfterCardSelectionForAllowlist(
        ObserverState observer,
        IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        static bool IsProgressionLabel(string? label)
        {
            return !string.IsNullOrWhiteSpace(label)
                   && (label.Contains("계속", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("Continue", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("Proceed", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("진행", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("확인", StringComparison.OrdinalIgnoreCase));
        }

        static bool IsSubtypeTarget(string? targetLabel)
        {
            return !string.IsNullOrWhiteSpace(targetLabel)
                   && (targetLabel.StartsWith("transform ", StringComparison.OrdinalIgnoreCase)
                       || targetLabel.StartsWith("deck remove ", StringComparison.OrdinalIgnoreCase)
                       || targetLabel.StartsWith("upgrade ", StringComparison.OrdinalIgnoreCase));
        }

        var recentSubtypeAftermath = false;
        for (var index = history.Count - 1; index >= 0 && index >= history.Count - 6; index -= 1)
        {
            var entry = history[index];
            if (string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase)
                && IsSubtypeTarget(entry.TargetLabel))
            {
                recentSubtypeAftermath = true;
                break;
            }
        }

        if (!recentSubtypeAftermath)
        {
            return false;
        }

        var eventAuthority = string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(observer.VisibleScreen, "event", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(observer.ChoiceExtractorPath, "event", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(observer.ChoiceExtractorPath, "room-event", StringComparison.OrdinalIgnoreCase);
        if (!eventAuthority)
        {
            return false;
        }

        return observer.ActionNodes.Any(node =>
                   node.Actionable
                   && TryParseScreenBounds(node.ScreenBounds, out _)
                   && IsProgressionLabel(node.Label))
               || observer.Choices.Any(choice =>
                   TryParseScreenBounds(choice.ScreenBounds, out _)
                   && IsProgressionLabel(choice.Label));
    }

    static bool ShouldSuppressRoomSubstateHeuristics(GuiSmokePhase phase, ObserverState observer)
    {
        return phase == GuiSmokePhase.HandleCombat
               || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary);
    }

    static bool HasExplicitRewardProgressionAffordance(ObserverSummary observer)
    {
        return observer.Choices.Any(IsExplicitRewardProgressionChoice)
               || observer.ActionNodes.Any(IsExplicitRewardProgressionNode);
    }

    static bool IsExplicitRewardProgressionChoice(ObserverChoice choice)
    {
        if (!TryParseNodeBounds(choice.ScreenBounds, out _)
            || IsOverlayChoiceLabel(choice.Label)
            || IsInspectPreviewChoice(choice)
            || IsDismissLikeLabel(choice.Label))
        {
            return false;
        }

        return IsRewardCardChoice(choice)
               || IsPotionRewardChoice(choice)
               || IsGoldRewardChoice(choice)
               || IsRelicRewardChoice(choice)
               || IsSkipOrProceedLabel(choice.Label)
               || choice.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || HasLargeChoiceBounds(choice.ScreenBounds);
    }

    static bool IsExplicitRewardProgressionNode(ObserverActionNode node)
    {
        if (!node.Actionable
            || !TryParseNodeBounds(node.ScreenBounds, out _)
            || IsOverlayChoiceLabel(node.Label)
            || IsInspectPreviewBounds(node.ScreenBounds)
            || IsDismissLikeLabel(node.Label)
            || IsMapNode(node)
            || IsBackNode(node))
        {
            return false;
        }

        return IsProceedNode(node)
               || node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || HasLargeChoiceBounds(node.ScreenBounds);
    }

    static bool IsRewardCardChoice(ObserverChoice choice)
    {
        return (string.Equals(choice.Kind, "card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.Kind, "reward-card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(choice.BindingId, "CardReward", StringComparison.OrdinalIgnoreCase)
                || choice.SemanticHints.Any(static hint => string.Equals(hint, "reward-card", StringComparison.OrdinalIgnoreCase)
                                                           || string.Equals(hint, "reward-type:CardReward", StringComparison.OrdinalIgnoreCase)))
               && !IsSkipOrProceedLabel(choice.Label)
               && !IsConfirmLikeLabel(choice.Label)
               && !IsDismissLikeLabel(choice.Label)
               && !IsOverlayChoiceLabel(choice.Label)
               && HasRewardCardLikeBounds(choice.ScreenBounds);
    }

    static bool HasRewardCardLikeBounds(string? screenBounds)
    {
        if (!TryParseNodeBounds(screenBounds, out var bounds))
        {
            return true;
        }

        return bounds.Width >= 120f || bounds.Height >= 150f;
    }

    static bool IsGoldRewardChoice(ObserverChoice choice)
    {
        return ContainsAny(choice.Label, "골드", "gold")
               || ContainsAny(choice.Description, "골드", "gold")
               || ContainsAny(choice.Value, "GOLD.")
               || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                  && string.Equals(choice.BindingId, "GoldReward", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => string.Equals(hint, "reward-gold", StringComparison.OrdinalIgnoreCase)
                                                          || string.Equals(hint, "reward-type:GoldReward", StringComparison.OrdinalIgnoreCase));
    }

    static bool IsPotionRewardChoice(ObserverChoice choice)
    {
        return string.Equals(choice.Kind, "potion", StringComparison.OrdinalIgnoreCase)
               || ContainsAny(choice.Label, "포션", "potion")
               || ContainsAny(choice.Description, "포션", "potion")
               || choice.Value?.StartsWith("POTION.", StringComparison.OrdinalIgnoreCase) == true
               || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                  && string.Equals(choice.BindingId, "PotionReward", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => string.Equals(hint, "reward-potion", StringComparison.OrdinalIgnoreCase)
                                                          || string.Equals(hint, "reward-type:PotionReward", StringComparison.OrdinalIgnoreCase));
    }

    static bool IsRelicRewardChoice(ObserverChoice choice)
    {
        return string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || ContainsAny(choice.Description, "relic", "유물")
               || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                  && string.Equals(choice.BindingId, "RelicReward", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => string.Equals(hint, "reward-relic", StringComparison.OrdinalIgnoreCase)
                                                          || string.Equals(hint, "reward-type:RelicReward", StringComparison.OrdinalIgnoreCase));
    }

    static bool IsInspectPreviewChoice(ObserverChoice choice)
    {
        return IsOverlayChoiceLabel(choice.Label)
               || string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || IsInspectPreviewBounds(choice.ScreenBounds);
    }

    static string[] GetCombatAllowedActions(
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        GuiSmokeStepAnalysisContext? analysisContext = null)
    {
        var context = analysisContext ?? CreateStepAnalysisContext(GuiSmokePhase.HandleCombat, observer, screenshotPath, history, combatCardKnowledge);
        if (context.CombatPlayerActionWindowClosed)
        {
            return new[] { "wait" };
        }

        var combatBarrier = context.CombatBarrierEvaluation;
        if (combatBarrier.IsActive && combatBarrier.IsHardWaitBarrier)
        {
            return new[] { "wait" };
        }

        var actions = new List<string>();
        var combatContext = context.CombatContext;
        var blockedCombatNoOpCounts = combatContext.CombatNoOpCountsBySlot;
        var pendingSelection = context.PendingCombatSelection;
        var analysis = context.CombatAnalysis;
        var hasSelectedNonEnemyConfirmEvidence = context.HasSelectedNonEnemyConfirmEvidence;
        var keepNonEnemySelectionClosed = pendingSelection?.Kind == AutoCombatCardKind.DefendLike && hasSelectedNonEnemyConfirmEvidence;
        bool ShouldSuppressNonEnemyReselect(int slotIndex)
        {
            if (keepNonEnemySelectionClosed)
            {
                return true;
            }

            if (pendingSelection?.Kind == AutoCombatCardKind.DefendLike
                && pendingSelection.SlotIndex == slotIndex
                && !hasSelectedNonEnemyConfirmEvidence)
            {
                return true;
            }

            return !hasSelectedNonEnemyConfirmEvidence
                   && HandleCombatContextSupport.HasRecentNonEnemySelection(combatContext, slotIndex);
        }

        foreach (var slotIndex in GetPlayableCombatAttackSlots(observer, combatCardKnowledge))
        {
            if (CombatBarrierSupport.SuppressesAttackSlot(combatBarrier, slotIndex))
            {
                continue;
            }

            if (!blockedCombatNoOpCounts.TryGetValue(slotIndex, out var noOpCount) || noOpCount < 2)
            {
                actions.Add($"select attack slot {slotIndex}");
            }
        }

        foreach (var slotIndex in GetPlayableCombatNonEnemySlots(observer, combatCardKnowledge))
        {
            if (CombatBarrierSupport.SuppressesNonEnemySlot(combatBarrier, slotIndex))
            {
                continue;
            }

            if (ShouldSuppressNonEnemyReselect(slotIndex))
            {
                continue;
            }

            actions.Add($"select non-enemy slot {slotIndex}");
        }

        var pendingAttackBlocked = pendingSelection?.Kind == AutoCombatCardKind.AttackLike
                                   && blockedCombatNoOpCounts.TryGetValue(pendingSelection.SlotIndex, out var pendingNoOpCount)
                                   && pendingNoOpCount >= 2;
        if (!pendingAttackBlocked && context.CanResolveCombatEnemyTarget)
        {
            actions.Add("click enemy");
        }

        if (hasSelectedNonEnemyConfirmEvidence)
        {
            actions.Add("confirm selected non-enemy card");
        }

        actions.Add("click end turn");

        if (HasCombatSelectionToCancelFromAnalysis(observer, combatCardKnowledge, analysis, pendingSelection, combatContext))
        {
            actions.Add("right-click cancel selected card");
        }

        if (actions.Count == 1)
        {
            actions.Insert(0, "select card from hand");
        }

        actions.Add("wait");
        return actions
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    static IEnumerable<int> GetPlayableCombatAttackSlots(
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var knowledgeSlots = combatCardKnowledge
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .Where(card => IsEnemyTargetCombatCard(card) && IsPlayableAtCurrentEnergy(card, observer.PlayerEnergy))
            .Select(static card => card.SlotIndex);
        var observerSlots = observer.CombatHand
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .Where(card => IsAttackCombatHandCard(card) && IsObservedCombatCardPlayableAtCurrentEnergy(card, observer.PlayerEnergy, combatCardKnowledge))
            .Select(static card => card.SlotIndex);
        return knowledgeSlots
            .Concat(observerSlots)
            .Distinct()
            .OrderBy(static slotIndex => slotIndex);
    }

    static IEnumerable<int> GetPlayableCombatNonEnemySlots(
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var knowledgeSlots = combatCardKnowledge
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .Where(card => CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatCard(card, observer.PlayerEnergy))
            .Select(static card => card.SlotIndex);
        var observerSlots = observer.CombatHand
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .Where(card => CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatHandCard(card, observer.PlayerEnergy, combatCardKnowledge))
            .Select(static card => card.SlotIndex);
        return knowledgeSlots
            .Concat(observerSlots)
            .Distinct()
            .OrderBy(static slotIndex => slotIndex);
    }

    static bool CanResolveEnemyTargetFromStateAnalysis(
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        AutoCombatAnalysis analysis,
        PendingCombatSelection? pendingSelection)
    {
        var runtime = CombatRuntimeStateSupport.Read(observer.Summary, combatCardKnowledge);
        if (CombatRuntimeStateSupport.CanResolveEnemyTarget(observer.Summary, combatCardKnowledge, pendingSelection, analysis))
        {
            return true;
        }

        if (runtime.HasExplicitHittableEnemyAuthority)
        {
            return false;
        }

        if (CombatTargetabilitySupport.GetCombatEnemyTargetNodes(observer.Summary).Count > 0)
        {
            return true;
        }

        if (analysis.HasTargetArrow)
        {
            return true;
        }

        if (CombatRuntimeStateSupport.RequiresExplicitTargetingBeforeEnemyClick(observer.Summary, combatCardKnowledge))
        {
            return false;
        }

        if (pendingSelection?.Kind == AutoCombatCardKind.AttackLike)
        {
            var pendingCard = observer.CombatHand.FirstOrDefault(card => card.SlotIndex == pendingSelection.SlotIndex);
            if (pendingCard is not null)
            {
                return IsAttackCombatHandCard(pendingCard)
                       && IsObservedCombatCardPlayableAtCurrentEnergy(pendingCard, observer.PlayerEnergy, combatCardKnowledge);
            }

            var pendingKnowledge = combatCardKnowledge.FirstOrDefault(card => card.SlotIndex == pendingSelection.SlotIndex);
            if (pendingKnowledge is not null)
            {
                return IsEnemyTargetCombatCard(pendingKnowledge)
                       && IsPlayableAtCurrentEnergy(pendingKnowledge, observer.PlayerEnergy);
            }
        }

        return analysis.HasSelectedCard
               && analysis.SelectedCardKind == AutoCombatCardKind.AttackLike
               && (GetPlayableCombatAttackSlots(observer, combatCardKnowledge).Any()
                   || (observer.CombatHand.Count == 0 && combatCardKnowledge.Count == 0));
    }

    static bool HasCombatSelectionToCancelFromAnalysis(
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        AutoCombatAnalysis analysis,
        PendingCombatSelection? pendingSelection,
        ReconstructedHandleCombatContext? combatContext = null)
    {
        if (HasBlockedOpenAttackSelectionToCancel(observer.Summary, combatCardKnowledge, pendingSelection, combatContext))
        {
            return true;
        }

        if (CombatRuntimeStateSupport.HasSelectionToKeep(observer.Summary, combatCardKnowledge))
        {
            return false;
        }

        return analysis.HasSelectedCard
               && !CanResolveEnemyTargetFromStateAnalysis(observer, combatCardKnowledge, analysis, pendingSelection);
    }

    static bool HasBlockedOpenAttackSelectionToCancel(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? pendingSelection,
        ReconstructedHandleCombatContext? combatContext)
    {
        if (combatContext is null
            || pendingSelection?.Kind != AutoCombatCardKind.AttackLike
            || pendingSelection.SlotIndex is < 1 or > 5
            || !CombatRuntimeStateSupport.HasSelectionToKeep(observer, combatCardKnowledge))
        {
            return false;
        }

        return HandleCombatContextSupport.GetCombatNoOpCountForSlot(combatContext, pendingSelection.SlotIndex) >= 2;
    }

    static string BuildCombatFailureModeHint(
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var attackSlots = GetPlayableCombatAttackSlots(observer, combatCardKnowledge).ToArray();
        if (attackSlots.Length > 0)
        {
            return $"Only click the enemy after selecting an attack. Current playable attack slots: {string.Join(", ", attackSlots)}. Ignore stale map or reward contamination when the observer still shows combat.";
        }

        var nonEnemySlots = GetPlayableCombatNonEnemySlots(observer, combatCardKnowledge).ToArray();
        if (nonEnemySlots.Length > 0)
        {
            return $"No playable enemy-target attack is currently confirmed. Use a non-enemy slot ({string.Join(", ", nonEnemySlots)}) or end turn, and do not click the enemy without a selected attack.";
        }

        return "No playable enemy-target attack is currently confirmed. Do not click the enemy without a visible target arrow or a matching selected attack lane; prefer end turn instead.";
    }

    static bool IsAttackCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("STRIKE", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("BASH", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsEnemyTargetCombatCard(CombatCardKnowledgeHint card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsPlayableAtCurrentEnergy(CombatCardKnowledgeHint card, int? energy)
    {
        if (energy is null || card.Cost is null)
        {
            return true;
        }

        if (card.Cost < 0)
        {
            return energy > 0;
        }

        return card.Cost <= energy;
    }

    static bool IsObservedCombatCardPlayableAtCurrentEnergy(
        ObservedCombatHandCard card,
        int? energy,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var resolvedCost = ResolveObservedCombatCardCost(card, combatCardKnowledge);
        if (energy is null || resolvedCost is null)
        {
            return true;
        }

        if (resolvedCost < 0)
        {
            return energy > 0;
        }

        return resolvedCost <= energy;
    }

    static bool IsProceedNode(ObserverActionNode node)
    {
        return EventProceedObserverSignals.HasExplicitEventProceedSemantic(node.SemanticHints)
               || node.Label.Contains("Proceed", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Continue", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("진행", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("계속", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("proceed", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsBackNode(ObserverActionNode node)
    {
        return node.Label.Contains("Back", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("뒤", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("back", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsMapNode(ObserverActionNode node)
    {
        return node.NodeId.Contains("map", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("map", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Map", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("지도", StringComparison.OrdinalIgnoreCase);
    }

    static bool TryParseNodeBounds(string? raw, out RectangleF bounds)
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

    static bool HasStrongMapTransitionEvidence(ObserverState observer)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(observer.Summary)
            || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary)
            || CardSelectionObserverSignals.TryGetState(observer.Summary) is not null
            || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary)
            || ShopObserverSignals.IsShopAuthorityActive(observer.Summary)
            || RewardObserverSignals.IsRewardAuthorityActive(observer.Summary)
            || GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer))
        {
            return false;
        }

        return GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer)
               && !GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer);
    }

    static bool HasStrongMapTransitionEvidenceFromScene(ObserverSummary observer, string? sceneSignature)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(observer)
            || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer)
            || CardSelectionObserverSignals.TryGetState(observer) is not null
            || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer)
            || ShopObserverSignals.IsShopAuthorityActive(observer)
            || RewardObserverSignals.IsRewardAuthorityActive(observer)
            || GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer))
        {
            return false;
        }

        return !GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer)
               && (GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer)
               || (!string.IsNullOrWhiteSpace(sceneSignature)
                   && (sceneSignature.Contains("substate:map-transition", StringComparison.OrdinalIgnoreCase)
                       || sceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase))));
    }

    static RewardMapLayerState BuildRewardMapLayerStateForObserver(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return AutoDecisionProvider.BuildRewardSceneState(observer, windowBounds).LayerState;
    }

    static bool LooksLikeRewardBackNavigationAffordance(ObserverSummary observer, string? screenshotPath)
    {
        if (observer.CurrentChoices.Any(static label =>
                label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                || label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                || label.Contains("뒤로", StringComparison.OrdinalIgnoreCase)
                || label.Contains("Back", StringComparison.OrdinalIgnoreCase))
            || observer.ActionNodes.Any(static node =>
                node.Actionable
                && (node.Label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                    || node.Label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                    || node.Label.Contains("뒤로", StringComparison.OrdinalIgnoreCase)
                    || node.Label.Contains("Back", StringComparison.OrdinalIgnoreCase))))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(screenshotPath) || !File.Exists(screenshotPath))
        {
            return false;
        }

        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(screenshotPath);
        return overlayAnalysis.HasBottomLeftBackArrow
               && !overlayAnalysis.HasCentralOverlayPanel
               && (string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase));
    }

    static bool HasScreenshotClaimableRewardEvidence(ObserverSummary observer, string? screenshotPath)
    {
        if (string.IsNullOrWhiteSpace(screenshotPath) || !File.Exists(screenshotPath))
        {
            return false;
        }

        if (!string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(observer.VisibleScreen, "rewards", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return AutoEventCardGridAnalyzer.Analyze(screenshotPath).HasSelectableCard;
    }

    static bool IsCurrentRewardProgressionChoiceForObserver(ObserverChoice choice, WindowBounds? windowBounds)
    {
        return IsProgressionLikeRewardChoice(choice)
               && HasActiveRewardBoundsForObserver(choice.ScreenBounds, windowBounds);
    }

    static bool IsCurrentRewardProgressionNodeForObserver(ObserverActionNode node, WindowBounds? windowBounds)
    {
        return node.Actionable
               && IsProgressionLikeRewardNode(node)
               && HasActiveRewardBoundsForObserver(node.ScreenBounds, windowBounds);
    }

    static bool IsProgressionLikeRewardChoice(ObserverChoice choice)
    {
        if (!TryParseScreenBounds(choice.ScreenBounds, out _))
        {
            return false;
        }

        return choice.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || choice.Kind.Contains("card", StringComparison.OrdinalIgnoreCase)
               || choice.Kind.Contains("potion", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => hint.StartsWith("reward", StringComparison.OrdinalIgnoreCase))
               || choice.Label.Contains("골드", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("gold", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("포션", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("potion", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("넘기", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("skip", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("proceed", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("continue", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || choice.Value?.StartsWith("CARD.", StringComparison.OrdinalIgnoreCase) == true
               || choice.Value?.StartsWith("POTION.", StringComparison.OrdinalIgnoreCase) == true
               || ContainsAny(choice.Description, "포션", "potion")
               || HasLargeChoiceBounds(choice.ScreenBounds);
    }

    static bool IsProgressionLikeRewardNode(ObserverActionNode node)
    {
        if (!TryParseScreenBounds(node.ScreenBounds, out _))
        {
            return false;
        }

        return node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("proceed", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("골드", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("gold", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("포션", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("potion", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("넘기", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("skip", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("proceed", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("continue", StringComparison.OrdinalIgnoreCase)
               || HasLargeChoiceBounds(node.ScreenBounds);
    }

    static bool HasActiveRewardBoundsForObserver(string? screenBounds, WindowBounds? windowBounds)
    {
        if (HasUsableLogicalBoundsForObserver(screenBounds))
        {
            return true;
        }

        return windowBounds is not null && IsBoundsInsideWindowForObserver(screenBounds, windowBounds);
    }

    static bool IsBoundsInsideWindowForObserver(string? screenBounds, WindowBounds windowBounds)
    {
        if (!TryParseScreenBounds(screenBounds, out var bounds))
        {
            return false;
        }

        return bounds.Right > windowBounds.X
               && bounds.Bottom > windowBounds.Y
               && bounds.X < windowBounds.X + windowBounds.Width
               && bounds.Y < windowBounds.Y + windowBounds.Height;
    }

    static bool HasUsableLogicalBoundsForObserver(string? raw)
    {
        if (!TryParseScreenBounds(raw, out var bounds))
        {
            return false;
        }

        var centerX = bounds.X + bounds.Width / 2f;
        var centerY = bounds.Y + bounds.Height / 2f;
        return centerX >= 0f
               && centerY >= 0f
               && centerX <= 1920f
               && centerY <= 1080f;
    }

    static int? ResolveObservedCombatCardCost(ObservedCombatHandCard card, IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        if (card.Cost is not null)
        {
            return card.Cost;
        }

        var slotMatch = combatCardKnowledge.FirstOrDefault(candidate => candidate.SlotIndex == card.SlotIndex);
        if (slotMatch?.Cost is not null)
        {
            return slotMatch.Cost;
        }

        var cardKeys = BuildCombatKnowledgeLookupKeys(card.Name);
        if (cardKeys.Count == 0)
        {
            return null;
        }

        return combatCardKnowledge
            .Where(candidate => candidate.Cost is not null)
            .Where(candidate => BuildCombatKnowledgeLookupKeys(candidate.Name).Any(cardKeys.Contains))
            .Select(static candidate => candidate.Cost)
            .FirstOrDefault();
    }

    static bool IsOverlayCleanupTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay back", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay backdrop close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "inspect overlay escape", StringComparison.OrdinalIgnoreCase);
    }

    static bool LooksLikeTreasureState(ObserverSummary observer)
    {
        if (TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer)
            || string.Equals(observer.EncounterKind, "Treasure", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.ChoiceExtractorPath, "treasure", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return observer.CurrentChoices.Any(static label =>
            label.Contains("Chest", StringComparison.OrdinalIgnoreCase)
            || label.Contains("\uC0C1\uC790", StringComparison.OrdinalIgnoreCase));
    }

    static bool LooksLikeRestSiteState(ObserverSummary observer)
    {
        return GuiSmokeNonCombatContractSupport.LooksLikeRestSiteState(observer);
    }

    static bool LooksLikeRestSiteProceedState(ObserverSummary observer)
    {
        return GuiSmokeNonCombatContractSupport.LooksLikeRestSiteProceedState(observer);
    }

    static bool HasExplicitRestSiteChoiceAuthority(ObserverState observer, string? screenshotPath)
    {
        return HasRestSiteAuthority(observer.Summary)
               && HasExplicitRestSiteChoiceAffordance(observer.Summary)
               && !RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer.Summary)
               && !AutoRestSiteCardGridAnalyzer.Analyze(screenshotPath ?? string.Empty).HasSelectableCard;
    }

    static bool HasRestSiteAuthority(ObserverSummary observer)
    {
        return string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase)
               || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer);
    }

    static bool HasExplicitRestSiteChoiceAffordance(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(observer);
    }

    static string[] BuildExplicitRestSiteAllowedActions(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.BuildAllowedActions(observer).ToArray();
    }

    static bool LooksLikeSingleplayerSubmenuState(ObserverSummary observer)
    {
        return string.Equals(observer.CurrentScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.VisibleScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase);
    }

    static string BuildGoal(GuiSmokePhase phase)
    {
        return phase switch
        {
            GuiSmokePhase.WaitMainMenu => "Reach main menu and verify observer currentScreen=main-menu.",
            GuiSmokePhase.EnterRun => "Enter a run using Continue first, otherwise Singleplayer.",
            GuiSmokePhase.WaitRunLoad => "Wait for the root-scene transition to finish and for the resumed/new run scene to become actionable.",
            GuiSmokePhase.WaitCharacterSelect => "Wait for the actual singleplayer submenu or character-select flow for a fresh new run.",
            GuiSmokePhase.ChooseCharacter => "Select Ironclad.",
            GuiSmokePhase.Embark => "Click Embark to begin the run.",
            GuiSmokePhase.WaitMap => "Wait until observer logical currentScreen=map. visibleScreen may reach map earlier while reward flow is still active.",
            GuiSmokePhase.HandleRewards => "Resolve the visible reward screen so the run can return to map.",
            GuiSmokePhase.ChooseFirstNode => "Click the first reachable map node.",
            GuiSmokePhase.WaitPostMapNodeRoom => "Reconcile the destination room after clicking a reachable map node.",
            GuiSmokePhase.HandleEvent => "Resolve the event screen. If nothing else is obvious, pick the first visible option.",
            GuiSmokePhase.WaitEventRelease => "Wait for the explicit ancient event completion click to release the event room before map handoff.",
            GuiSmokePhase.HandleShop => "Resolve explicit shop room semantics: open inventory, perform one bounded shop action, then back/proceed.",
            GuiSmokePhase.WaitCombat => "Wait until observer currentScreen=combat and encounter.inCombat=true.",
            GuiSmokePhase.HandleCombat => "Play the combat from the screenshot: choose cards, targets, or end turn until combat resolves.",
            _ => "Complete the scenario.",
        };
    }

    static string BuildFailureModeHintCore(
        GuiSmokePhase phase,
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return BuildFailureModeHintCoreWithContext(phase, observer, combatCardKnowledge, screenshotPath, history, null);
    }

    static string BuildFailureModeHintCoreWithContext(
        GuiSmokePhase phase,
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        GuiSmokeStepAnalysisContext? analysisContext)
    {
        var context = analysisContext ?? CreateStepAnalysisContext(phase, observer, screenshotPath, history, combatCardKnowledge);
        if (phase == GuiSmokePhase.HandleCombat)
        {
            return !context.CanResolveCombatEnemyTarget
                ? BuildCombatFailureModeHint(observer, combatCardKnowledge)
                : "AI first: trust observer/runtime combat state. Use selected-card, targetability, hittability, energy, and player-action-window truth before any screenshot fallback; only inspect the screenshot when explicit combat authority is missing or contradictory.";
        }

        if (phase == GuiSmokePhase.WaitRunLoad)
        {
            return WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(observer.Summary)
                ? "Stable main-menu continue authority is still foreground-visible with no transition evidence. Reopen EnterRun and retry Continue instead of idling in WaitRunLoad."
                : "AI first: trust transition/runtime root-scene state. While transitionInProgress is true, or rootSceneIsRun/currentRunNodePresent is not yet true, wait without acting; only branch once the resumed or newly created run scene becomes explicit.";
        }

        if (phase == GuiSmokePhase.HandleRewards && context.UseRewardFastPath)
        {
            var rewardMapLayer = context.RewardMapLayerState;
            if (LooksLikeInspectOverlayState(observer))
            {
                return "Inspect overlay is not progression. Close it before resolving the explicit reward foreground.";
            }

            if (CardSelectionObserverSignals.TryGetState(observer.Summary) is { } cardSelectionState)
            {
                return cardSelectionState.ScreenType switch
                {
                    "reward-pick" => "AI first: trust the explicit reward-pick card-selection subtype and choose a visible reward card before any screenshot fallback.",
                    _ => "AI first: trust the explicit reward card-selection subtype and use its select/confirm semantics before any screenshot fallback.",
                };
            }

            if (rewardMapLayer.RewardTeardownInProgress || rewardMapLayer.MapCurrentActiveScreen)
            {
                return "Reward ownership has already dropped or map is current. Release to map/post-room reconciliation instead of forcing reward or map fallback from stale visuals.";
            }

            return "AI first: trust reward observer/runtime state. Use reward foreground ownership, explicit reward choices, and proceed availability before any screenshot fallback; only inspect the screenshot when explicit reward authority is missing or contradictory.";
        }

        if (phase == GuiSmokePhase.HandleEvent)
        {
            var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, null, history, screenshotPath);
            if (eventScene.RewardSubstateActive)
            {
                return "AI first: this event is currently in a reward substate. Reuse the canonical reward lane, finish reward claim/skip/proceed, and do not fall back to generic event or map routing until reward ownership releases.";
            }

            if (eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.ReleasePending)
            {
                return "Explicit event proceed/completion was already fired on the same authority band. Wait for release or handoff instead of reissuing the same proceed click or reopening map routing early.";
            }
        }

        return phase switch
        {
            GuiSmokePhase.EnterRun => "Continue may be absent. Use Singleplayer only if Continue is not visible.",
            GuiSmokePhase.WaitCharacterSelect => "Only actual singleplayer submenu or character-select authority belongs here. Do not use this phase for Continue or root-scene loading.",
            GuiSmokePhase.ChooseCharacter => "Do not click Embark before Ironclad is selected.",
            GuiSmokePhase.HandleRewards when LooksLikeInspectOverlayState(observer)
                => "Inspect overlay is not progression. Close it with escape or the overlay dismiss affordance before choosing any reward.",
            GuiSmokePhase.HandleRewards when GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer)
                => "This is a colorless reward choice. Pick a visible card first; do not click the small relic inspect icons in the top-left.",
            GuiSmokePhase.HandleRewards when GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer)
                => "Reward follow-up is active. Prefer reward cards or skip/proceed over inspect, preview, or detail affordances.",
            GuiSmokePhase.HandleRewards when GuiSmokeRewardSceneSignals.ShouldPreferRewardProgressionOverMapFallback(observer)
                => "Reward screen authority is stronger than the background map. Claim the visible reward or use skip/proceed first, and ignore any contaminated map arrow until the reward panel disappears.",
            GuiSmokePhase.HandleRewards when HasStrongMapTransitionEvidence(observer)
                => "Map authority is already stronger than the lingering event label. Prefer a reachable node or screenshot-derived map advance instead of repeating proceed/event clicks.",
            GuiSmokePhase.HandleRewards when string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
                => "AI first: use the screenshot as the primary source. If the map is clearly visible, you may click the first reachable node instead of forcing another reward proceed click.",
            GuiSmokePhase.HandleRewards => "Prefer the proceed arrow when the reward can be skipped; otherwise pick a valid reward card.",
            GuiSmokePhase.ChooseFirstNode when HasExplicitRestSiteChoiceAuthority(observer, screenshotPath)
                => "Rest-site explicit choices are foreground-authoritative here. Prefer 휴식/재련/부화 over any map overlay candidate or current-node arrow. If the smith card grid appears afterward, select a card and then confirm.",
            GuiSmokePhase.ChooseFirstNode when LooksLikeRestSiteState(observer.Summary)
                => "Rest site is screenshot-first. If the explicit rest options are visible, choose one of them before any map candidate. If the smith card grid is visible, click one card first and then click the right-side confirm button.",
            GuiSmokePhase.ChooseFirstNode when LooksLikeTreasureState(observer.Summary)
                => "Treasure room authority is explicit. Use chest -> treasure relic holder -> treasure proceed, and ignore top-left inventory relic icons or map-node contamination.",
            GuiSmokePhase.ChooseFirstNode => "Do not click non-reachable map nodes.",
            GuiSmokePhase.HandleShop => "Shop authority is explicit. Open inventory if needed, buy only typed affordable shop entries, keep card removal separate from normal purchases, then back/proceed.",
            GuiSmokePhase.HandleEvent when LooksLikeInspectOverlayState(observer)
                => "Inspect overlay is open inside the room flow. Dismiss it before retrying event, reward, or proceed choices.",
            GuiSmokePhase.HandleEvent when GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer)
                => "The event follow-up is a colorless card choice. Select a card from the card area; do not treat relic previews as event options.",
            GuiSmokePhase.HandleEvent when GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer)
                => "The event has entered a reward substate. Prefer reward cards, reward choices, or skip/proceed over inspect affordances.",
            GuiSmokePhase.HandleEvent when GuiSmokeRewardSceneSignals.ShouldPreferRewardProgressionOverMapFallback(observer)
                => "Reward screen authority is stronger than the background map. Resolve the visible reward, skip, or proceed affordance before considering any map fallback.",
            GuiSmokePhase.HandleEvent when HasStrongMapTransitionEvidence(observer)
                => "Map evidence is stronger than the stale event label. Prefer reachable map-node or visible-map-advance actions over repeating event progression.",
            GuiSmokePhase.HandleEvent when LooksLikeTreasureState(observer.Summary)
                => "Treasure authority can linger on the event phase. Prefer explicit treasure chest, treasure relic holder, or treasure proceed over generic event or map routing.",
            GuiSmokePhase.HandleEvent => "If the event text is ambiguous, choose a large visible progression option, not inspect affordances or detail overlays.",
            GuiSmokePhase.WaitEventRelease => "Ancient proceed was already clicked. Wait for event-room release or a concrete next room state instead of re-clicking the same proceed button.",
            GuiSmokePhase.WaitPostMapNodeRoom => "A reachable node starts room entry, not combat-only flow. Reconcile the destination room from observer truth before waiting or routing again.",
            GuiSmokePhase.WaitCombat => "Observer must end with combat screen and inCombat=true.",
            _ => "Fail closed when screenshot and observer disagree.",
        };
    }

    static GuiSmokePhase GetPostEnterRunPhase(GuiSmokeStepDecision decision)
    {
        return string.Equals(decision.TargetLabel, "continue", StringComparison.OrdinalIgnoreCase)
            ? GuiSmokePhase.WaitRunLoad
            : GuiSmokePhase.WaitCharacterSelect;
    }

    static GuiSmokePhase GetPostRewardPhase(GuiSmokeStepDecision decision)
    {
        if (IsRewardReleaseTarget(decision.TargetLabel))
        {
            return GuiSmokePhase.WaitMap;
        }

        if (KeepsCurrentRoomPhase(decision.TargetLabel))
        {
            return GuiSmokePhase.HandleRewards;
        }

        if (IsReachableNodeTarget(decision.TargetLabel))
        {
            return GuiSmokePhase.WaitPostMapNodeRoom;
        }

        return GuiSmokePhase.WaitMap;
    }

    static GuiSmokePhase GetPostChooseFirstNodePhase(GuiSmokeStepDecision decision)
    {
        return IsReachableNodeTarget(decision.TargetLabel)
            ? GuiSmokePhase.WaitPostMapNodeRoom
            : GuiSmokePhase.WaitMap;
    }

    static GuiSmokePhase GetPostHandleEventPhase(GuiSmokeStepDecision decision)
    {
        if (IsAncientEventCompletionTarget(decision.TargetLabel))
        {
            return GuiSmokePhase.WaitEventRelease;
        }

        if (KeepsCurrentRoomPhase(decision.TargetLabel))
        {
            return GuiSmokePhase.HandleEvent;
        }

        return IsRoomProgressTarget(decision.TargetLabel)
            ? GetPostChooseFirstNodePhase(decision)
            : GuiSmokePhase.HandleEvent;
    }

    static bool IsReachableNodeTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsAncientEventCompletionTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "ancient event completion", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsRoomProgressTarget(string? targetLabel)
    {
        return IsReachableNodeTarget(targetLabel)
               || string.Equals(targetLabel, "treasure chest center", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "treasure chest", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "treasure relic holder", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "treasure proceed", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay back", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay backdrop close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "treasure overlay back", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: rest", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: smith", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: hatch", StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(targetLabel)
                   && targetLabel.StartsWith("rest site: option:", StringComparison.OrdinalIgnoreCase));
    }

    static bool KeepsCurrentRoomPhase(string? targetLabel)
    {
        return string.Equals(targetLabel, "claim reward item", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "reward choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "reward card choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "colorless card choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "reward skip", StringComparison.OrdinalIgnoreCase)
               || IsOverlayCleanupTarget(targetLabel);
    }

    static bool IsRewardReleaseTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "reward skip", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "proceed after resolving rewards", StringComparison.OrdinalIgnoreCase);
    }

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
        if (eventScene.EventForegroundOwned
            && eventScene.ReleaseStage == EventReleaseStage.Active)
        {
            return true;
        }

        return EventProceedObserverSignals.HasEventChoiceAuthority(observer.Summary)
               && !NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer)
               && !ShouldRouteToHandleRewards(observer, history, screenshotPath)
               && !AutoDecisionProvider.HasRecentEventReleaseIntent(history)
               && AutoDecisionProvider.HasRawEventProgressionSurface(observer.Summary, null);
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

            if (HasExplicitRewardProgressionAffordance(observer.Summary))
            {
                observerSignals.Add("reward-explicit-progression");
            }

            if (HasStrongMapTransitionEvidence(observer)
                && !string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
                && !GuiSmokeRewardSceneSignals.ShouldPreferRewardProgressionOverMapFallback(observer))
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

            if (IsRewardMapRecoveryTarget(decision.TargetLabel))
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
                if (IsRewardMapRecoveryTarget(decision.TargetLabel))
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

