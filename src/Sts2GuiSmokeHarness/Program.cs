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

static class CombatBarrierPolicy
{
    public const int HandleCombatWaitMinimumMs = 140;
    public const int HandleCombatWaitPlateauLimit = 4;

    public static bool IsStableCombatEntryObserver(ObserverState observer)
    {
        return string.Equals(observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase)
               && observer.SceneReady == true
               && string.Equals(observer.SceneStability, "stable", StringComparison.OrdinalIgnoreCase)
               && observer.InCombat == true;
    }
}

static class GuiSmokeShared
{
    public static Action<string>? HarnessLogSink { get; set; }

    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static JsonSerializerOptions NdjsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static async Task RunProcessAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        TimeSpan timeout,
        bool waitForExit = true)
    {
        var result = await RunProcessDetailedAsync(fileName, arguments, workingDirectory, timeout, waitForExit).ConfigureAwait(false);
        if (!waitForExit)
        {
            return;
        }

        if (result.TimedOut)
        {
            throw new TimeoutException(
                $"Process timed out after {timeout}: {fileName} {arguments}{Environment.NewLine}stdout:{Environment.NewLine}{result.Stdout}{Environment.NewLine}stderr:{Environment.NewLine}{result.Stderr}");
        }

        if (!string.IsNullOrWhiteSpace(result.FailureKind))
        {
            throw new InvalidOperationException(
                $"Process failed before exit: {result.FailureKind} ({result.ExceptionType}: {result.ExceptionMessage}) {fileName} {arguments}{Environment.NewLine}stdout:{Environment.NewLine}{result.Stdout}{Environment.NewLine}stderr:{Environment.NewLine}{result.Stderr}");
        }

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Process failed with exit code {result.ExitCode}: {fileName} {arguments}{Environment.NewLine}stdout:{Environment.NewLine}{result.Stdout}{Environment.NewLine}stderr:{Environment.NewLine}{result.Stderr}");
        }
    }

    public static async Task<GuiSmokeProcessExecutionResult> RunProcessDetailedAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        TimeSpan timeout,
        bool waitForExit = true)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = waitForExit,
                RedirectStandardError = waitForExit,
                CreateNoWindow = true,
            },
        };

        var stopwatch = Stopwatch.StartNew();
        try
        {
            process.Start();
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(
                fileName,
                arguments,
                null,
                false,
                stopwatch.Elapsed,
                string.Empty,
                string.Empty,
                "process-start-failure",
                exception.GetType().Name,
                exception.Message);
        }

        if (!waitForExit)
        {
            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(fileName, arguments, null, false, stopwatch.Elapsed, string.Empty, string.Empty, null, null, null);
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        using var timeoutCts = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
            }

            try
            {
                await process.WaitForExitAsync().ConfigureAwait(false);
            }
            catch
            {
            }

            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(
                fileName,
                arguments,
                process.HasExited ? process.ExitCode : null,
                true,
                stopwatch.Elapsed,
                await stdoutTask.ConfigureAwait(false),
                await stderrTask.ConfigureAwait(false),
                null,
                null,
                null);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(
                fileName,
                arguments,
                process.HasExited ? process.ExitCode : null,
                false,
                stopwatch.Elapsed,
                await TryReadProcessOutputAsync(stdoutTask).ConfigureAwait(false),
                await TryReadProcessOutputAsync(stderrTask).ConfigureAwait(false),
                "process-wait-failure",
                exception.GetType().Name,
                exception.Message);
        }

        string stdout;
        string stderr;
        try
        {
            stdout = await stdoutTask.ConfigureAwait(false);
            stderr = await stderrTask.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(
                fileName,
                arguments,
                process.ExitCode,
                false,
                stopwatch.Elapsed,
                await TryReadProcessOutputAsync(stdoutTask).ConfigureAwait(false),
                await TryReadProcessOutputAsync(stderrTask).ConfigureAwait(false),
                "output-drain-failure",
                exception.GetType().Name,
                exception.Message);
        }

        stopwatch.Stop();
        return new GuiSmokeProcessExecutionResult(
            fileName,
            arguments,
            process.ExitCode,
            false,
            stopwatch.Elapsed,
            stdout,
            stderr,
            null,
            null,
            null);
    }

    private static async Task<string> TryReadProcessOutputAsync(Task<string>? task)
    {
        if (task is null)
        {
            return string.Empty;
        }

        try
        {
            return await task.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            return $"<output-read-failed:{exception.GetType().Name}:{exception.Message}>";
        }
    }
}

sealed class GuiSmokeSessionSceneHistoryIndex
{
    private readonly HashSet<string> _seenSignatures = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<KnownRecipeHint>> _knownRecipesByKey = new(StringComparer.Ordinal);

    public static GuiSmokeSessionSceneHistoryIndex Load(string sessionRoot)
    {
        var index = new GuiSmokeSessionSceneHistoryIndex();
        index.LoadFromSessionArtifacts(sessionRoot);
        return index;
    }

    public bool HasSeen(string sceneSignature)
    {
        return _seenSignatures.Contains(sceneSignature);
    }

    public IReadOnlyList<KnownRecipeHint> GetKnownRecipes(string sceneSignature, string phase)
    {
        return _knownRecipesByKey.TryGetValue(BuildRecipeKey(sceneSignature, phase), out var hints)
            ? hints
            : Array.Empty<KnownRecipeHint>();
    }

    public void NoteRecipe(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        if (string.IsNullOrWhiteSpace(request.SceneSignature)
            || string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase)
            || string.Equals(decision.Status, "abort", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(decision.ActionKind))
        {
            return;
        }

        _seenSignatures.Add(request.SceneSignature);
        var key = BuildRecipeKey(request.SceneSignature, request.Phase);
        if (!_knownRecipesByKey.TryGetValue(key, out var hints))
        {
            hints = new List<KnownRecipeHint>(capacity: 3);
            _knownRecipesByKey[key] = hints;
        }

        hints.Add(new KnownRecipeHint(
            request.SceneSignature,
            request.Phase,
            decision.ActionKind!,
            decision.TargetLabel,
            decision.ExpectedScreen,
            decision.Reason));

        if (hints.Count > 3)
        {
            hints.RemoveRange(0, hints.Count - 3);
        }
    }

    public void NoteUnknownScene(GuiSmokeStepRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SceneSignature))
        {
            return;
        }

        _seenSignatures.Add(request.SceneSignature);
    }

    private void LoadFromSessionArtifacts(string sessionRoot)
    {
        LoadRecipes(Path.Combine(sessionRoot, "scene-recipes.ndjson"));
        LoadUnknownScenes(Path.Combine(sessionRoot, "unknown-scenes.ndjson"));
    }

    private void LoadRecipes(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        foreach (var line in File.ReadLines(path))
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

            if (entry is null)
            {
                continue;
            }

            _seenSignatures.Add(entry.SceneSignature);
            var key = BuildRecipeKey(entry.SceneSignature, entry.Phase);
            if (!_knownRecipesByKey.TryGetValue(key, out var hints))
            {
                hints = new List<KnownRecipeHint>(capacity: 3);
                _knownRecipesByKey[key] = hints;
            }

            hints.Add(new KnownRecipeHint(
                entry.SceneSignature,
                entry.Phase,
                entry.ActionKind,
                entry.TargetLabel,
                entry.ExpectedScreen,
                entry.Reason));

            if (hints.Count > 3)
            {
                hints.RemoveRange(0, hints.Count - 3);
            }
        }
    }

    private void LoadUnknownScenes(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            UnknownSceneEntry? entry;
            try
            {
                entry = JsonSerializer.Deserialize<UnknownSceneEntry>(line, GuiSmokeShared.JsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (entry is not null && !string.IsNullOrWhiteSpace(entry.SceneSignature))
            {
                _seenSignatures.Add(entry.SceneSignature);
            }
        }
    }

    private static string BuildRecipeKey(string sceneSignature, string phase)
    {
        return string.Concat(sceneSignature, "::", phase);
    }
}

sealed record GuiSmokeStepRequest(
    string RunId,
    string ScenarioId,
    int StepIndex,
    string Phase,
    string Goal,
    DateTimeOffset IssuedAt,
    string ScreenshotPath,
    WindowBounds WindowBounds,
    string SceneSignature,
    string AttemptId,
    int AttemptOrdinal,
    int BoundedExplorationBudget,
    bool FirstSeenScene,
    string ReasoningMode,
    string? SemanticGoal,
    ObserverSummary Observer,
    IReadOnlyList<KnownRecipeHint> KnownRecipes,
    IReadOnlyList<EventKnowledgeCandidate> EventKnowledgeCandidates,
    IReadOnlyList<CombatCardKnowledgeHint> CombatCardKnowledge,
    string[] AllowedActions,
    IReadOnlyList<GuiSmokeHistoryEntry> History,
    string FailureModeHint,
    string? DecisionRiskHint);

sealed record GuiSmokeSceneRequestContext(
    string SceneSignature,
    bool FirstSeenScene,
    string ReasoningMode,
    IReadOnlyList<KnownRecipeHint> KnownRecipes);

sealed record KnownRecipeHint(
    string SceneSignature,
    string Phase,
    string ActionKind,
    string? TargetLabel,
    string? ExpectedScreen,
    string? Reason);

sealed record SceneRecipeEntry(
    DateTimeOffset RecordedAt,
    string SceneSignature,
    string Phase,
    string ActionKind,
    string? TargetLabel,
    string? ExpectedScreen,
    string? Reason,
    string ScreenshotPath);

sealed record UnknownSceneEntry(
    DateTimeOffset RecordedAt,
    string SceneSignature,
    string Phase,
    string ScreenshotPath,
    string? ObserverScreen,
    string? VisibleScreen,
    string? Reason);

sealed record EventKnowledgeCandidate(
    string EventId,
    string Title,
    string? MatchReason,
    IReadOnlyList<EventOptionKnowledgeCandidate> Options);

sealed record EventOptionKnowledgeCandidate(
    string Label,
    string? Description,
    string? OptionKey);

sealed record CombatCardKnowledgeHint(
    int SlotIndex,
    string Name,
    string? Type,
    string? Target,
    int? Cost,
    string MatchSource);

sealed record GuiSmokeStepDecision(
    string Status,
    string? ActionKind,
    string? KeyText,
    double? NormalizedX,
    double? NormalizedY,
    string? TargetLabel,
    string? Reason,
    double? Confidence,
    string? ExpectedScreen,
    int? WaitMs,
    bool? RequiresRecapture,
    string? AbortReason,
    string? SceneInterpretation = null,
    string? ExpectedDelta = null,
    string? DecisionRisk = null);

sealed record GuiSmokeCandidatePoint(
    double X,
    double Y);

sealed record GuiSmokeSuppressedCandidate(
    string Label,
    string SuppressionReason);

sealed record GuiSmokeDecisionCandidateDump(
    string Label,
    string Source,
    double Score,
    bool Selected,
    string? RejectReason,
    string? RawBounds,
    GuiSmokeCandidatePoint? NormalizedPoint,
    string? BoundsSource,
    string? TargetLabel,
    string? ActionKind,
    string? Reason);

sealed record GuiSmokeDecisionDebugSummary(
    string? ForegroundKind,
    string? BackgroundKind,
    IReadOnlyList<string> ActiveCandidateSet,
    IReadOnlyList<GuiSmokeSuppressedCandidate> SuppressedCandidates,
    string WinnerSelectionReason);

sealed record GuiSmokeCandidateDumpArtifact(
    string Phase,
    string ScreenshotPath,
    GuiSmokeStepDecision PredictedDecision,
    GuiSmokeStepDecision FinalDecision,
    bool MatchesPredictedDecision,
    GuiSmokeDecisionDebugSummary DebugSummary,
    IReadOnlyList<GuiSmokeDecisionCandidateDump> Candidates);

sealed record GuiSmokeDecisionAnalysis(
    string Phase,
    string ScreenshotPath,
    GuiSmokeStepDecision PredictedDecision,
    GuiSmokeStepDecision FinalDecision,
    GuiSmokeDecisionDebugSummary DebugSummary,
    IReadOnlyList<GuiSmokeDecisionCandidateDump> Candidates,
    bool MatchesPredictedDecision)
{
    public GuiSmokeCandidateDumpArtifact ToArtifact()
    {
        return new GuiSmokeCandidateDumpArtifact(
            Phase,
            ScreenshotPath,
            PredictedDecision,
            FinalDecision,
            MatchesPredictedDecision,
            DebugSummary,
            Candidates);
    }
}

sealed record GuiSmokeReplayEvaluation(
    string RequestPath,
    GuiSmokeStepRequest Request,
    GuiSmokeStepDecision Decision,
    GuiSmokeCandidateDumpArtifact CandidateDump);

sealed record GuiSmokeReplayRequestLoadResult(
    GuiSmokeStepRequest Request,
    bool FullRequestRebuild,
    bool ObserverStateLoaded,
    IReadOnlyList<GuiSmokeReplayTimingEntry> Timings);

sealed class GuiSmokeReplayTracer
{
    private readonly string _scope;
    private readonly bool _verbose;
    private readonly List<GuiSmokeReplayTimingEntry> _entries = new();

    public GuiSmokeReplayTracer(string scope, bool verbose)
    {
        _scope = scope;
        _verbose = verbose;
    }

    public IReadOnlyList<GuiSmokeReplayTimingEntry> Entries => _entries;

    public void Info(string message)
    {
        Console.Error.WriteLine($"[{_scope}] {message}");
    }

    public void Skipped(string stage, string detail)
    {
        _entries.Add(new GuiSmokeReplayTimingEntry(stage, 0, $"skipped:{detail}"));
        if (_verbose)
        {
            Info($"{stage} skipped ({detail})");
        }
    }

    public T Measure<T>(string stage, Func<T> action, string? detail = null, bool alwaysLog = false)
    {
        if (_verbose || alwaysLog)
        {
            Info($"start {stage}{FormatDetail(detail)}");
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            return action();
        }
        finally
        {
            stopwatch.Stop();
            var entry = new GuiSmokeReplayTimingEntry(stage, stopwatch.ElapsedMilliseconds, detail);
            _entries.Add(entry);
            if (_verbose || alwaysLog)
            {
                Info($"done {stage} {stopwatch.ElapsedMilliseconds}ms{FormatDetail(detail)}");
            }
        }
    }

    private static string FormatDetail(string? detail)
    {
        return string.IsNullOrWhiteSpace(detail)
            ? string.Empty
            : $" ({detail})";
    }
}

sealed class GuiSmokeStepAnalysisContext
{
    private static readonly RewardMapLayerState EmptyRewardMapLayerState = new(false, false, false, false, false, false, false, false, false, false, false);
    private static readonly MapOverlayState EmptyMapOverlayState = new(false, false, false, false, false, false, false);

    private readonly GuiSmokePhase _phase;
    private readonly ObserverState _observer;
    private readonly string? _screenshotPath;
    private readonly IReadOnlyList<GuiSmokeHistoryEntry> _history;
    private readonly IReadOnlyList<CombatCardKnowledgeHint> _combatCardKnowledge;
    private readonly Func<bool> _useCombatFastPathFactory;
    private readonly Func<bool> _useRewardFastPathFactory;
    private readonly Func<RewardMapLayerState> _rewardMapLayerStateFactory;
    private readonly Func<bool> _rewardBackNavigationAvailableFactory;
    private readonly Func<bool> _claimableRewardPresentFactory;
    private readonly Func<MapOverlayState> _mapOverlayStateFactory;
    private readonly Func<ReconstructedHandleCombatContext> _combatContextFactory;
    private readonly Func<PendingCombatSelection?> _pendingCombatSelectionFactory;
    private readonly Func<CombatRuntimeState> _runtimeCombatStateFactory;
    private readonly Func<AutoCombatAnalysis> _combatAnalysisFactory;
    private readonly Func<AutoCombatHandAnalysis> _combatHandAnalysisFactory;
    private readonly Func<bool> _combatPlayerActionWindowClosedFactory;
    private readonly Func<bool> _hasSelectedNonEnemyConfirmEvidenceFactory;
    private readonly Func<bool> _canResolveCombatEnemyTargetFactory;
    private readonly Func<CombatBarrierEvaluation> _combatBarrierEvaluationFactory;

    private bool? _useCombatFastPath;
    private bool? _useRewardFastPath;
    private RewardMapLayerState? _rewardMapLayerState;
    private bool? _rewardBackNavigationAvailable;
    private bool? _claimableRewardPresent;
    private MapOverlayState? _mapOverlayState;
    private ReconstructedHandleCombatContext? _combatContext;
    private PendingCombatSelection? _pendingCombatSelection;
    private bool _pendingCombatSelectionComputed;
    private CombatRuntimeState? _runtimeCombatState;
    private AutoCombatAnalysis? _combatAnalysis;
    private AutoCombatHandAnalysis? _combatHandAnalysis;
    private bool? _combatPlayerActionWindowClosed;
    private bool? _hasSelectedNonEnemyConfirmEvidence;
    private bool? _canResolveCombatEnemyTarget;
    private CombatBarrierEvaluation? _combatBarrierEvaluation;

    public GuiSmokeStepAnalysisContext(
        GuiSmokePhase phase,
        ObserverState observer,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        Func<bool> useCombatFastPathFactory,
        Func<bool> useRewardFastPathFactory,
        Func<RewardMapLayerState> rewardMapLayerStateFactory,
        Func<bool> rewardBackNavigationAvailableFactory,
        Func<bool> claimableRewardPresentFactory,
        Func<MapOverlayState> mapOverlayStateFactory,
        Func<ReconstructedHandleCombatContext> combatContextFactory,
        Func<PendingCombatSelection?> pendingCombatSelectionFactory,
        Func<CombatRuntimeState> runtimeCombatStateFactory,
        Func<AutoCombatAnalysis> combatAnalysisFactory,
        Func<AutoCombatHandAnalysis> combatHandAnalysisFactory,
        Func<bool> combatPlayerActionWindowClosedFactory,
        Func<bool> hasSelectedNonEnemyConfirmEvidenceFactory,
        Func<bool> canResolveCombatEnemyTargetFactory,
        Func<CombatBarrierEvaluation> combatBarrierEvaluationFactory)
    {
        _phase = phase;
        _observer = observer;
        _screenshotPath = screenshotPath;
        _history = history;
        _combatCardKnowledge = combatCardKnowledge;
        _useCombatFastPathFactory = useCombatFastPathFactory;
        _useRewardFastPathFactory = useRewardFastPathFactory;
        _rewardMapLayerStateFactory = rewardMapLayerStateFactory;
        _rewardBackNavigationAvailableFactory = rewardBackNavigationAvailableFactory;
        _claimableRewardPresentFactory = claimableRewardPresentFactory;
        _mapOverlayStateFactory = mapOverlayStateFactory;
        _combatContextFactory = combatContextFactory;
        _pendingCombatSelectionFactory = pendingCombatSelectionFactory;
        _runtimeCombatStateFactory = runtimeCombatStateFactory;
        _combatAnalysisFactory = combatAnalysisFactory;
        _combatHandAnalysisFactory = combatHandAnalysisFactory;
        _combatPlayerActionWindowClosedFactory = combatPlayerActionWindowClosedFactory;
        _hasSelectedNonEnemyConfirmEvidenceFactory = hasSelectedNonEnemyConfirmEvidenceFactory;
        _canResolveCombatEnemyTargetFactory = canResolveCombatEnemyTargetFactory;
        _combatBarrierEvaluationFactory = combatBarrierEvaluationFactory;
    }

    public GuiSmokePhase Phase => _phase;

    public ObserverState Observer => _observer;

    public string? ScreenshotPath => _screenshotPath;

    public IReadOnlyList<GuiSmokeHistoryEntry> History => _history;

    public IReadOnlyList<CombatCardKnowledgeHint> CombatCardKnowledge => _combatCardKnowledge;

    public bool UseCombatFastPath => _useCombatFastPath ??= _useCombatFastPathFactory();

    public bool UseRewardFastPath => _useRewardFastPath ??= _useRewardFastPathFactory();

    public bool UseAuthorityFastPath => UseCombatFastPath || UseRewardFastPath;

    public RewardMapLayerState RewardMapLayerState => _rewardMapLayerState ??= _rewardMapLayerStateFactory();

    public bool RewardBackNavigationAvailable => _rewardBackNavigationAvailable ??= _rewardBackNavigationAvailableFactory();

    public bool ClaimableRewardPresent => _claimableRewardPresent ??= _claimableRewardPresentFactory();

    public MapOverlayState MapOverlayState => _mapOverlayState ??= _mapOverlayStateFactory();

    public ReconstructedHandleCombatContext CombatContext => _combatContext ??= _combatContextFactory();

    public PendingCombatSelection? PendingCombatSelection
    {
        get
        {
            if (!_pendingCombatSelectionComputed)
            {
                _pendingCombatSelection = _pendingCombatSelectionFactory();
                _pendingCombatSelectionComputed = true;
            }

            return _pendingCombatSelection;
        }
    }

    public CombatRuntimeState RuntimeCombatState => _runtimeCombatState ??= _runtimeCombatStateFactory();

    public AutoCombatAnalysis CombatAnalysis => _combatAnalysis ??= _combatAnalysisFactory();

    public AutoCombatHandAnalysis CombatHandAnalysis => _combatHandAnalysis ??= _combatHandAnalysisFactory();

    public bool CombatPlayerActionWindowClosed => _combatPlayerActionWindowClosed ??= _combatPlayerActionWindowClosedFactory();

    public bool HasSelectedNonEnemyConfirmEvidence => _hasSelectedNonEnemyConfirmEvidence ??= _hasSelectedNonEnemyConfirmEvidenceFactory();

    public bool CanResolveCombatEnemyTarget => _canResolveCombatEnemyTarget ??= _canResolveCombatEnemyTargetFactory();

    public CombatBarrierEvaluation CombatBarrierEvaluation => _combatBarrierEvaluation ??= _combatBarrierEvaluationFactory();

    public static GuiSmokeStepAnalysisContext CreateForHandleCombatRequest(GuiSmokeStepRequest request)
    {
        var observer = new ObserverState(request.Observer, null, null, request.Observer.LastEventsTail?.ToArray() ?? Array.Empty<string>());
        var history = request.History;
        var combatCardKnowledge = request.CombatCardKnowledge;
        var screenshotPath = request.ScreenshotPath;
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
            => combatAnalysis ??= string.IsNullOrWhiteSpace(screenshotPath)
                ? new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown)
                : AutoCombatAnalyzer.Analyze(screenshotPath);

        AutoCombatHandAnalysis GetCombatHandAnalysis()
            => combatHandAnalysis ??= string.IsNullOrWhiteSpace(screenshotPath)
                ? new AutoCombatHandAnalysis(Array.Empty<AutoCombatHandSlotAnalysis>())
                : AutoCombatHandAnalyzer.Analyze(screenshotPath);

        CombatBarrierEvaluation GetCombatBarrierEvaluation()
            => combatBarrierEvaluation ??= CombatBarrierSupport.Evaluate(
                history,
                observer,
                GetCombatContext(),
                GetRuntimeCombatState(),
                GetCombatAnalysis(),
                CombatEligibilitySupport.HasSelectedNonEnemyConfirmEvidence(observer.Summary, combatCardKnowledge, GetCombatAnalysis(), GetPendingSelection()),
                CanResolveCombatEnemyTarget(),
                CombatEligibilitySupport.IsCombatPlayerActionWindowClosed(observer.Summary));

        bool CanResolveCombatEnemyTarget()
        {
            var analysis = GetCombatAnalysis();
            var pending = GetPendingSelection();
            var runtime = GetRuntimeCombatState();
            static bool IsAttackHandCard(ObservedCombatHandCard card)
            {
                return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
                       || card.Name.Contains("STRIKE", StringComparison.OrdinalIgnoreCase)
                       || card.Name.Contains("BASH", StringComparison.OrdinalIgnoreCase);
            }

            static bool IsEnemyTargetKnowledgeCard(CombatCardKnowledgeHint card)
            {
                return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
            }

            static bool IsKnowledgePlayableAtEnergy(CombatCardKnowledgeHint card, int? energy)
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

            static bool IsObservedPlayableAtEnergy(
                ObservedCombatHandCard card,
                int? energy,
                IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
            {
                var resolvedCost = card.Cost
                                   ?? combatCardKnowledge.FirstOrDefault(candidate => candidate.SlotIndex == card.SlotIndex)?.Cost;
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

            static IEnumerable<int> GetPlayableAttackSlots(
                ObserverState observer,
                IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
            {
                var knowledgeSlots = combatCardKnowledge
                    .Where(card => card.SlotIndex is >= 1 and <= 5)
                    .Where(card => IsEnemyTargetKnowledgeCard(card) && IsKnowledgePlayableAtEnergy(card, observer.PlayerEnergy))
                    .Select(static card => card.SlotIndex);
                var observerSlots = observer.CombatHand
                    .Where(card => card.SlotIndex is >= 1 and <= 5)
                    .Where(card => IsAttackHandCard(card) && IsObservedPlayableAtEnergy(card, observer.PlayerEnergy, combatCardKnowledge))
                    .Select(static card => card.SlotIndex);
                return knowledgeSlots.Concat(observerSlots).Distinct().OrderBy(static slotIndex => slotIndex);
            }

            if (CombatRuntimeStateSupport.CanResolveEnemyTarget(observer.Summary, combatCardKnowledge, pending, analysis))
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

            if (pending?.Kind == AutoCombatCardKind.AttackLike)
            {
                var pendingCard = observer.CombatHand.FirstOrDefault(card => card.SlotIndex == pending.SlotIndex);
                if (pendingCard is not null)
                {
                    return IsAttackHandCard(pendingCard)
                           && IsObservedPlayableAtEnergy(pendingCard, observer.PlayerEnergy, combatCardKnowledge);
                }

                var pendingKnowledge = combatCardKnowledge.FirstOrDefault(card => card.SlotIndex == pending.SlotIndex);
                if (pendingKnowledge is not null)
                {
                    return IsEnemyTargetKnowledgeCard(pendingKnowledge)
                           && IsKnowledgePlayableAtEnergy(pendingKnowledge, observer.PlayerEnergy);
                }
            }

            return analysis.HasSelectedCard
                   && analysis.SelectedCardKind == AutoCombatCardKind.AttackLike
                   && (GetPlayableAttackSlots(observer, combatCardKnowledge).Any()
                       || (observer.CombatHand.Count == 0 && combatCardKnowledge.Count == 0));
        }

        static bool LooksLikeInspectOverlayForeground(ObserverSummary summary)
        {
            return summary.CurrentChoices.Any(static label =>
                       label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase))
                   || summary.ActionNodes.Any(static node =>
                       node.Label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                       || node.Label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                       || node.Label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
        }

        static bool HasRestSiteAuthorityForCombat(ObserverSummary summary)
        {
            return string.Equals(summary.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(summary.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase)
                   || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(summary);
        }

        static bool HasExplicitEventForegroundForCombat(ObserverSummary summary)
        {
            var eventAuthority = string.Equals(summary.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(summary.VisibleScreen, "event", StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(summary.ChoiceExtractorPath, "event", StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(summary.ChoiceExtractorPath, "room-event", StringComparison.OrdinalIgnoreCase);
            if (!eventAuthority)
            {
                return false;
            }

            return summary.ActionNodes.Any(static node =>
                       node.Actionable
                       && !string.IsNullOrWhiteSpace(node.ScreenBounds)
                       && (node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
                           || node.Label.Contains("Proceed", StringComparison.OrdinalIgnoreCase)
                           || node.Label.Contains("Continue", StringComparison.OrdinalIgnoreCase)
                           || node.Label.Contains("진행", StringComparison.OrdinalIgnoreCase)
                           || node.Label.Contains("계속", StringComparison.OrdinalIgnoreCase)))
                   || summary.Choices.Any(static choice =>
                       !string.IsNullOrWhiteSpace(choice.ScreenBounds)
                       && (string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)));
        }

        bool ComputeUseCombatFastPath()
        {
            if (!string.Equals(request.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                || observer.InCombat != true)
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
                || HasRestSiteAuthorityForCombat(observer.Summary)
                || GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer)
                || HasExplicitEventForegroundForCombat(observer.Summary)
                || LooksLikeInspectOverlayForeground(observer.Summary))
            {
                return false;
            }

            return true;
        }

        return new GuiSmokeStepAnalysisContext(
            GuiSmokePhase.HandleCombat,
            observer,
            screenshotPath,
            history,
            combatCardKnowledge,
            ComputeUseCombatFastPath,
            () => false,
            () => EmptyRewardMapLayerState with { TerminalRunBoundary = RewardObserverSignals.IsTerminalRunBoundary(observer.Summary) },
            () => false,
            () => false,
            () => EmptyMapOverlayState,
            GetCombatContext,
            GetPendingSelection,
            GetRuntimeCombatState,
            GetCombatAnalysis,
            GetCombatHandAnalysis,
            () => CombatEligibilitySupport.IsCombatPlayerActionWindowClosed(observer.Summary),
            () => CombatEligibilitySupport.HasSelectedNonEnemyConfirmEvidence(observer.Summary, combatCardKnowledge, GetCombatAnalysis(), GetPendingSelection()),
            CanResolveCombatEnemyTarget,
            GetCombatBarrierEvaluation);
    }
}

sealed record ObserverSummary(
    string? CurrentScreen,
    string? VisibleScreen,
    bool? InCombat,
    DateTimeOffset? CapturedAt,
    string? InventoryId,
    bool? SceneReady,
    string? SceneAuthority,
    string? SceneStability,
    string? SceneEpisodeId,
    string? EncounterKind,
    string? ChoiceExtractorPath,
    int? PlayerCurrentHp,
    int? PlayerMaxHp,
    int? PlayerEnergy,
    IReadOnlyList<string> CurrentChoices,
    IReadOnlyList<string> LastEventsTail,
    IReadOnlyList<ObserverActionNode> ActionNodes,
    IReadOnlyList<ObserverChoice> Choices,
    IReadOnlyList<ObservedCombatHandCard> CombatHand)
{
    public long? SnapshotVersion { get; init; }

    public IReadOnlyDictionary<string, string?> Meta { get; init; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
}

sealed record ObserverActionNode(
    string NodeId,
    string Kind,
    string Label,
    string? ScreenBounds,
    bool Actionable)
{
    public string? TypeName { get; init; }

    public IReadOnlyList<string> SemanticHints { get; init; } = Array.Empty<string>();
}

sealed record ObserverChoice(
    string Kind,
    string Label,
    string? ScreenBounds,
    string? Value = null,
    string? Description = null)
{
    public string? NodeId { get; init; }

    public string? BindingKind { get; init; }

    public string? BindingId { get; init; }

    public bool? Enabled { get; init; }

    public string? IconAssetPath { get; init; }

    public IReadOnlyList<string> SemanticHints { get; init; } = Array.Empty<string>();
}

sealed record ObservedCombatHandCard(
    int SlotIndex,
    string Name,
    string? Type,
    int? Cost);

sealed record RestSiteObservedChoice(
    string OptionId,
    string TargetLabel,
    string CandidateLabel,
    string Label,
    string? Description,
    bool HasMetadata,
    bool IsEnabled,
    bool IsDefaultAutoPick,
    ObserverChoice? Choice,
    ObserverActionNode? ActionNode);

sealed record TreasureRoomSubtypeState(
    bool RoomDetected,
    bool ChestClickable,
    bool ChestOpened,
    int RelicHolderCount,
    int VisibleRelicHolderCount,
    int EnabledRelicHolderCount,
    bool ProceedEnabled,
    bool InspectOverlayVisible,
    IReadOnlyList<string> RelicHolderIds,
    string? RootType);

sealed record ShopRoomState(
    bool RoomDetected,
    bool RoomVisible,
    bool ForegroundOwned,
    bool TeardownInProgress,
    bool ShopIsCurrentActiveScreen,
    bool MapIsCurrentActiveScreen,
    string? ActiveScreenType,
    bool InventoryOpen,
    bool MerchantButtonVisible,
    bool MerchantButtonEnabled,
    bool ProceedEnabled,
    bool BackVisible,
    bool BackEnabled,
    int OptionCount,
    int AffordableOptionCount,
    IReadOnlyList<string> AffordableOptionIds,
    IReadOnlyList<string> AffordableRelicIds,
    IReadOnlyList<string> AffordableCardIds,
    IReadOnlyList<string> AffordablePotionIds,
    bool CardRemovalVisible,
    bool CardRemovalEnabled,
    bool CardRemovalEnoughGold,
    bool CardRemovalUsed,
    string? RootType);

sealed record RewardScreenState(
    bool ScreenDetected,
    bool ScreenVisible,
    bool ForegroundOwned,
    bool TeardownInProgress,
    bool RewardIsCurrentActiveScreen,
    bool RewardIsTopOverlay,
    bool MapIsCurrentActiveScreen,
    string? ActiveScreenType,
    bool ProceedVisible,
    bool ProceedEnabled,
    int VisibleButtonCount,
    int EnabledButtonCount,
    bool TerminalRunBoundary,
    bool GameOverScreenDetected,
    bool UnlockScreenDetected,
    bool TimelineUnlockDetected,
    bool MainMenuReturnDetected,
    string? RootType);

sealed record RestSiteDecisionCandidate(
    string Label,
    string Source,
    double Score,
    string? RejectReason,
    string? RawBounds,
    string? BoundsSource,
    GuiSmokeStepDecision? Decision);

sealed record RestSiteActionMetadata(
    string Kind,
    string TargetLabel,
    string Fingerprint);

enum RestSiteExplicitChoiceRepeatState
{
    None,
    GraceNeeded,
    NoOpDetected,
}

sealed record ObserverState(
    ObserverSummary Summary,
    JsonDocument? StateDocument,
    JsonDocument? InventoryDocument,
    string[]? EventLines)
{
    public string? CurrentScreen => Summary.CurrentScreen;
    public string? VisibleScreen => Summary.VisibleScreen;
    public bool? InCombat => Summary.InCombat;
    public string? InventoryId => Summary.InventoryId;
    public bool? SceneReady => Summary.SceneReady;
    public string? SceneAuthority => Summary.SceneAuthority;
    public string? SceneStability => Summary.SceneStability;
    public int? PlayerEnergy => Summary.PlayerEnergy;
    public IReadOnlyList<string> CurrentChoices => Summary.CurrentChoices;
    public IReadOnlyList<ObserverActionNode> ActionNodes => Summary.ActionNodes;
    public IReadOnlyList<ObserverChoice> Choices => Summary.Choices;
    public IReadOnlyList<ObservedCombatHandCard> CombatHand => Summary.CombatHand;
    public IReadOnlyDictionary<string, string?> Meta => Summary.Meta;
    public DateTimeOffset? CapturedAt => Summary.CapturedAt;
    public long? SnapshotVersion => Summary.SnapshotVersion;

    public bool IsFreshSince(DateTimeOffset threshold)
    {
        return CapturedAt is not null && CapturedAt >= threshold;
    }

    public string? EncounterKind => Summary.EncounterKind;
    public string? ChoiceExtractorPath => Summary.ChoiceExtractorPath;
}

enum NonCombatForegroundOwner
{
    Unknown,
    Combat,
    Reward,
    Shop,
    RestSite,
    Map,
    Event,
}

sealed record RootSceneTransitionState(
    bool TransitionInProgress,
    string? RootSceneCurrentType,
    bool RootSceneIsMainMenu,
    bool RootSceneIsRun,
    bool CurrentRunNodePresent,
    string? CurrentRunRoomType,
    string? CurrentRunRoomSceneType);

sealed record RestSitePostClickEvidence(
    string Classification,
    string? Outcome,
    string? OutcomeEvidence,
    string? CurrentStatus,
    string? CurrentOptionId,
    string? LastSignal,
    string? LastOptionId,
    bool UpgradeScreenVisible,
    bool ExplicitChoiceVisible,
    bool SmithGridVisible,
    bool SmithConfirmVisible,
    bool UpgradeChoiceObserverMiss);

sealed record CardSelectionSubtypeState(
    string ScreenType,
    string? Prompt,
    int? MinSelect,
    int? MaxSelect,
    int SelectedCount,
    bool? RequireManualConfirmation,
    bool? Cancelable,
    bool PreviewVisible,
    bool MainConfirmEnabled,
    bool PreviewConfirmEnabled,
    string? PreviewMode,
    IReadOnlyList<string> SelectedCardIds,
    string? RootType);

interface IGuiDecisionProvider
{
    Task<GuiSmokeStepDecision> GetDecisionAsync(string requestPath, string decisionPath, TimeSpan timeout, CancellationToken cancellationToken);
}

static class GuiSmokeCombatConstants
{
    public const double NonEnemyPrimeNormalizedX = 0.280;
    public const double NonEnemyPrimeNormalizedY = 0.620;
    public const double NonEnemyConfirmNormalizedX = 0.500;
    public const double NonEnemyConfirmNormalizedY = 0.560;
    public const int NonEnemyConfirmHoldMs = 75;
    public static readonly (double X, double Y, string Label)[] EnemyTargetCandidates =
    {
        (0.744, 0.542, "auto-target enemy"),
        (0.708, 0.532, "auto-target enemy recenter"),
        (0.778, 0.556, "auto-target enemy alternate"),
    };
}

sealed class SessionDecisionProvider : IGuiDecisionProvider
{
    public async Task<GuiSmokeStepDecision> GetDecisionAsync(string requestPath, string decisionPath, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(decisionPath))
            {
                using var stream = new FileStream(decisionPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                var parsed = await JsonSerializer.DeserializeAsync<GuiSmokeStepDecision>(stream, GuiSmokeShared.JsonOptions, cancellationToken).ConfigureAwait(false);
                if (parsed is not null)
                {
                    return parsed;
                }
            }

            await Task.Delay(500, cancellationToken).ConfigureAwait(false);
        }

        throw new TimeoutException($"Timed out waiting for decision file: {decisionPath}");
    }
}

sealed partial class AutoDecisionProvider : IGuiDecisionProvider
{
    private static GuiSmokeStepDecision DecideHandleRewards(GuiSmokeStepRequest request)
    {
        var rewardScene = BuildRewardSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath);
        var overlayDecision = TryCreateRoomOverlayCleanupDecision(request);
        if (overlayDecision is not null)
        {
            return overlayDecision;
        }

        var cardSelectionDecision = TryCreateCardSelectionDecision(request);
        if (cardSelectionDecision is not null)
        {
            return cardSelectionDecision;
        }

        var explicitRewardDecision = TryCreateExplicitRewardResolutionDecision(request, rewardScene);
        if (explicitRewardDecision is not null)
        {
            return explicitRewardDecision;
        }

        var rewardBackDecision = TryCreateRewardBackNavigationDecision(request, rewardScene);
        if (rewardBackDecision is not null)
        {
            return rewardBackDecision;
        }

        if (rewardScene.LayerState.TerminalRunBoundary)
        {
            return CreateWaitDecision("terminal reward boundary is active; do not reopen gameplay map fallback from reward aftermath.", request.Observer.CurrentScreen);
        }

        if (rewardScene.ReleaseToMapPending
            || (!rewardScene.RewardForegroundOwned
                && GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(request.Observer)))
        {
            return CreateRewardOwnershipReleaseWaitDecision(request);
        }

        return CreateForegroundAwareNonCombatWaitDecision(request, "waiting for reward actions");
    }

    private static GuiSmokeStepDecision CreateRewardOwnershipReleaseWaitDecision(GuiSmokeStepRequest request)
    {
        return CreateWaitDecision("waiting for reward ownership release to map/post-room reconciliation", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideChooseFirstNode(GuiSmokeStepRequest request)
    {
        if (HasExplicitRestSiteChoiceAuthority(request))
        {
            GuiSmokeDecisionDebug.SetSceneModel("rest-site", "map");
            GuiSmokeDecisionDebug.ReplaceActiveCandidates(BuildExplicitRestSiteCandidateLabels(request.Observer));
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "rest-site explicit choices outrank exported map routing");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "rest-site explicit choices outrank screenshot map routing");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "rest-site explicit choices suppress current-node-arrow fallback");
            GuiSmokeDecisionDebug.Suppress("click map back", "rest-site explicit choices are the progression lane");
            return GuiSmokeDecisionDebug.TraceCandidate(
                       "rest site explicit choice",
                       "rest-site-choice",
                       0.98,
                       TryCreateRestSiteDecision(request),
                       "rest-site explicit choices are not visible")
                   ?? GuiSmokeDecisionDebug.TraceCandidate(
                       "rest site upgrade",
                       "rest-site-upgrade",
                       0.92,
                       TryCreateRestSiteUpgradeDecision(request),
                       "rest-site upgrade grid is not currently actionable")
                   ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for an explicit rest-site choice");
        }

        var treasureDecision = TryCreateTreasureRoomDecision(request);
        if (TreasureRoomObserverSignals.IsTreasureAuthorityActive(request.Observer))
        {
            GuiSmokeDecisionDebug.SetSceneModel("treasure-room", "room-context");
            GuiSmokeDecisionDebug.ReplaceActiveCandidates(TreasureRoomObserverSignals.BuildAllowedActions(TreasureRoomObserverSignals.TryGetState(request.Observer)!).Append("wait").Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "treasure-room explicit affordances outrank map routing");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "treasure-room explicit affordances outrank screenshot map routing");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "treasure-room explicit affordances suppress map-arrow fallback");
            GuiSmokeDecisionDebug.Suppress("click map back", "treasure-room progression is chest-holder-proceed, not map back");
            return treasureDecision ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for treasure room progression");
        }

        if (ShopObserverSignals.IsShopAuthorityActive(request.Observer))
        {
            GuiSmokeDecisionDebug.SetSceneModel("shop", "room-context");
            var shopState = ShopObserverSignals.TryGetState(request.Observer)!;
            GuiSmokeDecisionDebug.ReplaceActiveCandidates(ShopObserverSignals.BuildAllowedActions(request.Observer, shopState, ShopObserverSignals.HasRecentPurchase(request.History)));
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "shop foreground outranks map routing");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "shop foreground outranks screenshot map routing");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "shop foreground suppresses current-node-arrow fallback");
            GuiSmokeDecisionDebug.Suppress("click map back", "shop foreground uses merchant/back/proceed affordances instead of map overlay cleanup");
            var shopDecision = DecideHandleShop(request);
            return string.Equals(shopDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                ? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit shop room progression")
                : shopDecision;
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (mapOverlayState.ForegroundVisible)
        {
            GuiSmokeDecisionDebug.SetSceneModel("map-overlay", mapOverlayState.EventBackgroundPresent ? "event-context" : "map-context");
            GuiSmokeDecisionDebug.ReplaceActiveCandidates(
                mapOverlayState.MapBackNavigationAvailable
                    ? new[] { "click exported reachable node", "click first reachable node", "click map back", "wait" }
                    : new[] { "click exported reachable node", "click first reachable node", "wait" });
            if (mapOverlayState.StaleEventChoicePresent)
            {
                GuiSmokeDecisionDebug.Suppress("click event choice", "map overlay foreground suppresses stale event choices");
                GuiSmokeDecisionDebug.Suppress("click proceed", "map overlay foreground suppresses stale event proceed choices");
            }

            GuiSmokeDecisionDebug.Suppress("click visible map advance", "map overlay foreground suppresses current-node-arrow fallback");
            if (!GuiSmokeNonCombatContractSupport.AllowsAnyMapRoutingAction(request))
            {
                return GuiSmokeNonCombatContractSupport.CreateMapRoutingContractWaitDecision(
                    request,
                    "map overlay is visible but request allowlist does not permit map routing; waiting for exporter/phase reconciliation");
            }

            return GuiSmokeDecisionDebug.TraceCandidate(
                       "exported reachable map node",
                       "observer-map-node",
                       0.95,
                       TryCreateExportedReachableMapPointDecision(request),
                       "no exported reachable map node bounds available")
                   ?? GuiSmokeDecisionDebug.TraceCandidate(
                       "map back",
                       "map-overlay-back-nav",
                       0.86,
                       TryCreateMapBackNavigationDecision(request),
                       "map overlay back navigation is not available")
                   ?? GuiSmokeDecisionDebug.TraceCandidate(
                       "visible reachable node",
                       "screenshot-reachable-node",
                       0.90,
                       TryFindFirstReachableMapNodeDecision(request),
                       "no screenshot-reachable next node was detected")
                   ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for exported or screenshot-reachable map node");
        }

        if (LooksLikeScreenshotFirstRoomState(request))
        {
            var roomDecision = GuiSmokeDecisionDebug.TraceCandidate(
                "screenshot first room",
                "room-screenshot-fallback",
                0.75,
                TryCreateScreenshotFirstRoomDecision(request),
                "no explicit screenshot-first room action was available");
            if (roomDecision is not null)
            {
                return roomDecision;
            }
        }

        return GuiSmokeDecisionDebug.TraceCandidate(
                   "exported reachable map node",
                   "observer-map-node",
                   0.95,
                   TryCreateExportedReachableMapPointDecision(request),
                   "no exported reachable map node bounds available")
               ?? GuiSmokeDecisionDebug.TraceCandidate(
                   "visible map advance",
                   "screenshot-current-arrow",
                   0.78,
                   TryFindVisibleMapAdvanceDecision(request),
                   "no current-node-arrow fallback was permitted")
               ?? (GuiSmokeNonCombatContractSupport.AllowsAnyMapRoutingAction(request)
                   ? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for reachable map node")
                   : GuiSmokeNonCombatContractSupport.CreateMapRoutingContractWaitDecision(
                       request,
                       "map progression evidence is present but request allowlist does not permit map routing; waiting for exporter/phase reconciliation"));
    }

    private static GuiSmokeStepDecision DecideHandleShop(GuiSmokeStepRequest request)
    {
        var overlayDecision = TryCreateRoomOverlayCleanupDecision(request);
        if (overlayDecision is not null)
        {
            return overlayDecision;
        }

        var cardSelectionDecision = TryCreateCardSelectionDecision(request);
        if (cardSelectionDecision is not null)
        {
            return cardSelectionDecision;
        }

        var shopState = ShopObserverSignals.TryGetState(request.Observer);
        if (shopState is null)
        {
            return CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit shop room authority");
        }

        var screen = request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "shop";
        var alreadyPurchased = ShopObserverSignals.HasRecentPurchase(request.History);
        if (!shopState.InventoryOpen && alreadyPurchased && shopState.ProceedEnabled)
        {
            var proceedChoice = ShopObserverSignals.TryGetProceedChoice(request.Observer);
            if (proceedChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    proceedChoice,
                    "shop proceed",
                    "A bounded shop purchase already happened and proceed is re-enabled. Leave the shop before reopening inventory.",
                    0.93,
                    screen,
                    1300);
            }
        }

        if (!shopState.InventoryOpen && shopState.MerchantButtonEnabled)
        {
            var merchantChoice = ShopObserverSignals.TryGetMerchantButtonChoice(request.Observer);
            if (merchantChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    merchantChoice,
                    "shop open inventory",
                    "Shop room is foreground-active and inventory is closed. Open the merchant inventory before any purchase or proceed action.",
                    0.97,
                    screen,
                    1300);
            }
        }

        if (shopState.InventoryOpen && !alreadyPurchased)
        {
            var relicChoice = ShopObserverSignals.GetAffordableRelicChoices(request.Observer).FirstOrDefault();
            if (relicChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    relicChoice,
                    "shop buy relic",
                    "Merchant inventory is open. Buy one affordable relic before cards, potions, back, or proceed.",
                    0.96,
                    screen,
                    1400);
            }

            var cardChoice = ShopObserverSignals.GetAffordableCardChoices(request.Observer).FirstOrDefault();
            if (cardChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    cardChoice,
                    "shop buy card",
                    "Merchant inventory is open. No affordable relic was chosen, so buy one affordable card before backing out.",
                    0.95,
                    screen,
                    1400);
            }

            var potionChoice = ShopObserverSignals.GetAffordablePotionChoices(request.Observer).FirstOrDefault();
            if (potionChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    potionChoice,
                    "shop buy potion",
                    "Merchant inventory is open. No relic or card purchase is selected, so buy one affordable potion before backing out.",
                    0.94,
                    screen,
                    1400);
            }

            if (shopState.CardRemovalEnabled)
            {
                var cardRemovalChoice = ShopObserverSignals.TryGetCardRemovalChoice(request.Observer);
                if (cardRemovalChoice is not null)
                {
                    return CreateClickDecisionFromChoice(
                        request,
                        cardRemovalChoice,
                        "shop card removal",
                        "Card removal is a distinct merchant service. Use its explicit shop service affordance instead of treating it like a normal card purchase.",
                        0.93,
                        screen,
                        1400);
                }
            }
        }

        if (shopState.InventoryOpen && shopState.BackVisible && shopState.BackEnabled)
        {
            var backChoice = ShopObserverSignals.TryGetBackChoice(request.Observer);
            if (backChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    backChoice,
                    "shop back",
                    "Shop inventory is open and there is no further bounded purchase to make. Close the inventory before proceeding.",
                    0.92,
                    screen,
                    1200);
            }
        }

        if (!shopState.InventoryOpen && shopState.ProceedEnabled)
        {
            var proceedChoice = ShopObserverSignals.TryGetProceedChoice(request.Observer);
            if (proceedChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    proceedChoice,
                    "shop proceed",
                    "Merchant inventory is closed and proceed is re-enabled. Leave the shop explicitly instead of waiting for map routing.",
                    0.91,
                    screen,
                    1300);
            }
        }

        return CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit shop inventory/proceed affordances");
    }

    private static GuiSmokeStepDecision DecideHandleEvent(GuiSmokeStepRequest request)
    {
        var eventScene = BuildEventSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath);
        if (eventScene.RewardSubstateActive)
        {
            return DecideHandleRewards(request with { Phase = GuiSmokePhase.HandleRewards.ToString() });
        }

        var forceEventProgressionAfterCardSelection = eventScene.ForceProgressionAfterCardSelection;
        var preferEventForeground = eventScene.EventForegroundOwned
                                    && eventScene.ReleaseStage == EventReleaseStage.Active;
        var mapOverlayState = eventScene.MapOverlayState;
        var strongEventForegroundChoice = eventScene.StrongForegroundChoice || forceEventProgressionAfterCardSelection;
        if (mapOverlayState.ForegroundVisible && !strongEventForegroundChoice)
        {
            GuiSmokeDecisionDebug.SetSceneModel("map-overlay", mapOverlayState.EventBackgroundPresent ? "event-context" : "map-context");
            GuiSmokeDecisionDebug.ReplaceActiveCandidates(
                mapOverlayState.MapBackNavigationAvailable
                    ? new[] { "click exported reachable node", "click first reachable node", "click map back", "wait" }
                    : new[] { "click exported reachable node", "click first reachable node", "wait" });
            GuiSmokeDecisionDebug.Suppress("click event choice", "map overlay foreground suppresses stale event choices");
            GuiSmokeDecisionDebug.Suppress("click proceed", "map overlay foreground suppresses stale event proceed choices");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "map overlay foreground suppresses current-node-arrow fallback");
            return GuiSmokeDecisionDebug.TraceCandidate(
                       "exported reachable map node",
                       "observer-map-node",
                       0.95,
                       TryCreateExportedReachableMapPointDecision(request),
                       "no exported reachable map node bounds available")
                   ?? GuiSmokeDecisionDebug.TraceCandidate(
                       "map back",
                       "map-overlay-back-nav",
                       0.86,
                       TryCreateMapBackNavigationDecision(request),
                       "map overlay back navigation is not available")
                   ?? GuiSmokeDecisionDebug.TraceCandidate(
                       "visible reachable node",
                       "screenshot-reachable-node",
                       0.90,
                       TryFindFirstReachableMapNodeDecision(request),
                       "no screenshot-reachable next node was detected")
                   ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for map-overlay foreground resolution");
        }

        if (preferEventForeground)
        {
            GuiSmokeDecisionDebug.SetSceneModel("event", request.SceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase) ? "map-context" : null);
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "event foreground keeps map-arrow evidence in the background only");
        }

        var overlayDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "inspect overlay cleanup",
            "overlay-cleanup",
            0.96,
            TryCreateRoomOverlayCleanupDecision(request),
            "no inspect overlay cleanup affordance is visible");
        if (overlayDecision is not null)
        {
            return overlayDecision;
        }

        var cardSelectionDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "card-selection subtype",
            "card-selection-runtime",
            0.96,
            TryCreateCardSelectionDecision(request),
            "no subtype-specific card-selection affordance is currently actionable");
        if (cardSelectionDecision is not null)
        {
            return cardSelectionDecision;
        }

        var treasureDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "treasure room",
            "treasure-room-runtime",
            0.95,
            TryCreateTreasureRoomDecision(request),
            "no explicit treasure room affordance is currently actionable");
        if (TreasureRoomObserverSignals.IsTreasureAuthorityActive(request.Observer))
        {
            GuiSmokeDecisionDebug.SetSceneModel("treasure-room", "room-context");
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "treasure-room explicit affordances outrank map routing");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "treasure-room explicit affordances outrank screenshot map routing");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "treasure-room explicit affordances suppress map-arrow fallback");
            GuiSmokeDecisionDebug.Suppress("click event choice", "treasure-room explicit affordances outrank generic event choices");
            return treasureDecision ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for treasure room progression");
        }

        var explicitRewardDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "explicit reward resolution",
            "reward-foreground",
            0.94,
            TryCreateExplicitRewardResolutionDecision(request),
            "no explicit reward foreground affordance is available");
        if (explicitRewardDecision is not null)
        {
            return explicitRewardDecision;
        }

        var rewardBackDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "reward back",
            "reward-back-nav",
            0.84,
            TryCreateRewardBackNavigationDecision(request),
            "reward back navigation is not available");
        if (rewardBackDecision is not null)
        {
            return rewardBackDecision;
        }

        var ancientDialogueDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "ancient dialogue advance",
            "ancient-dialogue",
            0.95,
            TryCreateAncientDialogueAdvanceDecision(request),
            "no explicit ancient dialogue hitbox has usable bounds");
        if (AncientEventObserverSignals.IsDialogueActive(request.Observer))
        {
            GuiSmokeDecisionDebug.SetSceneModel("ancient-event-dialogue", "event-context");
            GuiSmokeDecisionDebug.Suppress("click event choice", "ancient dialogue must finish before option selection");
            GuiSmokeDecisionDebug.Suppress("click proceed", "ancient dialogue does not use generic proceed");
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "ancient event foreground outranks map routing");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "ancient event foreground outranks screenshot map routing");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "ancient event foreground suppresses map-arrow fallback");
            return ancientDialogueDecision ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit ancient dialogue hitbox");
        }

        var ancientCompletionDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "ancient event completion",
            "ancient-completion-button",
            0.94,
            TryCreateAncientEventCompletionDecision(request),
            "no explicit ancient completion button has usable bounds");
        if (AncientEventObserverSignals.HasExplicitCompletionAction(request.Observer))
        {
            GuiSmokeDecisionDebug.SetSceneModel("ancient-event-completion", "event-context");
            GuiSmokeDecisionDebug.Suppress("click proceed", "ancient completion remains event-owned through the explicit NEventOptionButton proceed lane");
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "ancient event completion outranks map routing");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "ancient event completion outranks screenshot map routing");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "ancient event completion suppresses map-arrow fallback");
            return ancientCompletionDecision ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit ancient event completion");
        }

        if (eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.ReleasePending)
        {
            GuiSmokeDecisionDebug.SetSceneModel("event-release-pending", eventScene.MapContextVisible ? "map-context" : "event-context");
            GuiSmokeDecisionDebug.Suppress("click proceed", "event release-pending suppresses same proceed reissue");
            GuiSmokeDecisionDebug.Suppress("click event choice", "event release-pending suppresses stale event reissue");
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "event release-pending waits for explicit release before map routing");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "event release-pending waits for explicit release before map routing");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "event release-pending suppresses stale map contamination");
            return CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit event release");
        }

        var ancientOptionDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "explicit ancient event option",
            "ancient-option-buttons",
            0.93,
            TryCreateAncientEventOptionDecision(request),
            "no explicit ancient event option button has usable bounds");
        if (AncientEventObserverSignals.HasExplicitOptionSelection(request.Observer))
        {
            GuiSmokeDecisionDebug.SetSceneModel("ancient-event-options", "event-context");
            GuiSmokeDecisionDebug.Suppress("click proceed", "ancient options should be selected from explicit option buttons");
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "ancient event option selection outranks map routing");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "ancient event option selection outranks screenshot map routing");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "ancient event option selection suppresses map-arrow fallback");
            return ancientOptionDecision ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit ancient event option buttons");
        }

        if (preferEventForeground)
        {
            var semanticDecision = GuiSmokeDecisionDebug.TraceCandidate(
                "semantic event option",
                "event-semantic",
                0.92,
                TryCreateSemanticEventDecision(request),
                "no semantic event option matched current screenshot and observer evidence");
            if (semanticDecision is not null)
            {
                return semanticDecision;
            }

            var explicitEventChoice = GuiSmokeDecisionDebug.TraceCandidate(
                "explicit event choice",
                "event-choice",
                0.90,
                TryCreateEventProgressChoiceDecision(request),
                "no explicit event choice has usable bounds");
            if (explicitEventChoice is not null)
            {
                return explicitEventChoice;
            }
        }

        if (LooksLikeMapTransitionState(request))
        {
            return DecideChooseFirstNode(request with { Phase = GuiSmokePhase.ChooseFirstNode.ToString() });
        }

        var roomDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "screenshot first room",
            "room-screenshot-fallback",
            0.75,
            TryCreateScreenshotFirstRoomDecision(request),
            "no screenshot-first room action was available");
        if (roomDecision is not null)
        {
            return roomDecision;
        }

        if (!preferEventForeground)
        {
            var semanticDecision = GuiSmokeDecisionDebug.TraceCandidate(
                "semantic event option",
                "event-semantic",
                0.92,
                TryCreateSemanticEventDecision(request),
                "no semantic event option matched current screenshot and observer evidence");
            if (semanticDecision is not null)
            {
                return semanticDecision;
            }

            var explicitEventChoice = GuiSmokeDecisionDebug.TraceCandidate(
                "explicit event choice",
                "event-choice",
                0.90,
                TryCreateEventProgressChoiceDecision(request),
                "no explicit event choice has usable bounds");
            if (explicitEventChoice is not null)
            {
                return explicitEventChoice;
            }
        }

        return CreateForegroundAwareNonCombatWaitDecision(request, "waiting for an explicit event progression choice");
    }

    private static GuiSmokeStepDecision? TryCreateCardSelectionDecision(GuiSmokeStepRequest request)
    {
        var state = CardSelectionObserverSignals.TryGetState(request.Observer);
        if (state is null)
        {
            return null;
        }

        return state.ScreenType switch
        {
            "reward-pick" => TryCreateRewardPickDecision(request, state),
            "transform" => TryCreateSubtypeCardSelectionDecision(request, state, "transform select card", "transform confirm"),
            "deck-remove" => TryCreateSubtypeCardSelectionDecision(request, state, "deck remove select card", "deck remove confirm"),
            "upgrade" => TryCreateSubtypeCardSelectionDecision(request, state, "upgrade select card", "upgrade confirm"),
            _ => null,
        };
    }

    private static GuiSmokeStepDecision? TryCreateRewardPickDecision(GuiSmokeStepRequest request, CardSelectionSubtypeState state)
    {
        var explicitChoice = CardSelectionObserverSignals.GetCardChoices(request.Observer, state)
            .Where(choice => choice.Enabled != false && HasActiveRewardBounds(choice.ScreenBounds, request.WindowBounds))
            .OrderBy(GetChoiceSortX)
            .ThenBy(GetChoiceSortY)
            .FirstOrDefault();
        if (explicitChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                explicitChoice,
                BuildCardSelectionCardTargetLabel(state.ScreenType, explicitChoice, 0),
                "Reward-pick subtype is runtime-visible. Pick a reward card directly instead of entering any count/confirm flow.",
                0.96,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "card-choice",
                1400);
        }

        return TryCreateScreenshotSubtypeCardSelectionDecision(request, state, "reward pick card");
    }

    private static GuiSmokeStepDecision? TryCreateSubtypeCardSelectionDecision(
        GuiSmokeStepRequest request,
        CardSelectionSubtypeState state,
        string selectTargetLabel,
        string confirmTargetLabel)
    {
        if (CardSelectionObserverSignals.IsConfirmReady(state))
        {
            var confirmChoice = CardSelectionObserverSignals.TryGetConfirmChoice(request.Observer, state);
            if (confirmChoice is not null
                && confirmChoice.Enabled != false
                && HasActiveRewardBounds(confirmChoice.ScreenBounds, request.WindowBounds))
            {
                return CreateClickDecisionFromChoice(
                    request,
                    confirmChoice,
                    confirmTargetLabel,
                    $"Card-selection subtype '{state.ScreenType}' is confirm-ready. Use the exported confirm affordance instead of repeating a card click.",
                    0.95,
                    request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? state.ScreenType,
                    1400);
            }
        }

        var cardChoices = CardSelectionObserverSignals.GetCardChoices(request.Observer, state)
            .Where(choice => choice.Enabled != false && HasActiveRewardBounds(choice.ScreenBounds, request.WindowBounds))
            .ToArray();
        if (cardChoices.Length > 0)
        {
            var preferredChoice = SelectSubtypeCardChoice(request, state, cardChoices);
            if (preferredChoice is not null)
            {
                var choiceIndex = Array.IndexOf(cardChoices, preferredChoice);
                return CreateClickDecisionFromChoice(
                    request,
                    preferredChoice,
                    BuildCardSelectionCardTargetLabel(state.ScreenType, preferredChoice, choiceIndex),
                    $"Card-selection subtype '{state.ScreenType}' is active. Advance it with an explicit exported card hitbox before any generic reward fallback.",
                    0.95,
                    request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? state.ScreenType,
                    1400);
            }
        }

        return TryCreateScreenshotSubtypeCardSelectionDecision(request, state, selectTargetLabel);
    }

    private static ObserverChoice? SelectSubtypeCardChoice(
        GuiSmokeStepRequest request,
        CardSelectionSubtypeState state,
        IReadOnlyList<ObserverChoice> cardChoices)
    {
        var preferred = cardChoices
            .Where(choice => !CardSelectionObserverSignals.IsSelectedCardChoice(choice))
            .ToArray();
        if (preferred.Length == 0)
        {
            preferred = cardChoices.ToArray();
        }

        var lastTarget = request.History.LastOrDefault(entry =>
            string.Equals(entry.Phase, request.Phase, StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase)
            && entry.TargetLabel?.StartsWith(state.ScreenType.Replace('-', ' '), StringComparison.OrdinalIgnoreCase) == true)?.TargetLabel;

        foreach (var candidate in preferred)
        {
            var targetLabel = BuildCardSelectionCardTargetLabel(state.ScreenType, candidate, Array.IndexOf(cardChoices.ToArray(), candidate));
            if (!string.Equals(lastTarget, targetLabel, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return preferred.FirstOrDefault();
    }

    private static GuiSmokeStepDecision? TryCreateScreenshotSubtypeCardSelectionDecision(
        GuiSmokeStepRequest request,
        CardSelectionSubtypeState state,
        string targetLabel)
    {
        var analysis = AutoEventCardGridAnalyzer.Analyze(request.ScreenshotPath);
        if (!analysis.HasSelectableCard)
        {
            return null;
        }

        var variantIndex = CountRecentCardSelectionRepeats(request.History, targetLabel) % 3;
        var adjustedX = variantIndex switch
        {
            1 => Math.Clamp(analysis.CardNormalizedX - 0.12d, 0.12d, 0.88d),
            2 => Math.Clamp(analysis.CardNormalizedX + 0.12d, 0.12d, 0.88d),
            _ => analysis.CardNormalizedX,
        };
        var variantSuffix = variantIndex switch
        {
            1 => " left",
            2 => " right",
            _ => string.Empty,
        };

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            adjustedX,
            analysis.CardNormalizedY,
            targetLabel + variantSuffix,
            $"Card-selection subtype '{state.ScreenType}' is active, but no exported card bounds are available. Use bounded card-grid fallback for this subtype only.",
            0.88,
            request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? state.ScreenType,
            1400,
            true,
            null);
    }

    private static int CountRecentCardSelectionRepeats(IReadOnlyList<GuiSmokeHistoryEntry> history, string targetLabel)
    {
        var count = 0;
        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (!string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                    || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase)
                    || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                break;
            }

            if (!string.Equals(entry.TargetLabel, targetLabel, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(entry.TargetLabel, targetLabel + " left", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(entry.TargetLabel, targetLabel + " right", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            count += 1;
        }

        return count;
    }

    private static string BuildCardSelectionCardTargetLabel(string screenType, ObserverChoice choice, int index)
    {
        var prefix = screenType switch
        {
            "reward-pick" => "reward pick card",
            "deck-remove" => "deck remove select card",
            "upgrade" => "upgrade select card",
            _ => "transform select card",
        };
        var suffix = string.IsNullOrWhiteSpace(choice.Label) ? $" #{index + 1}" : $" {choice.Label}";
        return prefix + suffix;
    }

    private static GuiSmokeStepDecision? TryCreateExplicitRewardResolutionDecision(GuiSmokeStepRequest request)
    {
        return TryCreateExplicitRewardResolutionDecision(
            request,
            BuildRewardSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath));
    }

    private static GuiSmokeStepDecision? TryCreateExplicitRewardResolutionDecision(GuiSmokeStepRequest request, RewardSceneState rewardScene)
    {
        var rewardChoiceDecision = TryCreateRewardChoiceDecision(request, rewardScene);
        if (rewardChoiceDecision is not null)
        {
            return rewardChoiceDecision;
        }

        if (!rewardScene.RewardForegroundOwned
            || rewardScene.ReleaseStage != RewardReleaseStage.Active
            || rewardScene.ExplicitAction is not RewardExplicitActionKind.SkipProceed and not RewardExplicitActionKind.Claim and not RewardExplicitActionKind.CardChoice and not RewardExplicitActionKind.ColorlessChoice)
        {
            return null;
        }

        var rewardNode = request.Observer.ActionNodes
            .Where(node => IsCurrentRewardProgressionNode(node, request.WindowBounds))
            .Where(node => !IsProceedNode(node))
            .OrderByDescending(ScoreExplicitRewardProgressionNode)
            .ThenBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        if (rewardNode is not null)
        {
            return CreateClickDecisionFromNode(request, rewardNode, "claim reward item");
        }

        var rewardChoice = request.Observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, request.WindowBounds))
            .Where(choice => !IsSkipOrProceedLabel(choice.Label))
            .OrderByDescending(ScoreExplicitRewardProgressionChoice)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (rewardChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                rewardChoice,
                "claim reward item",
                $"Reward choice '{rewardChoice.Label}' is explicitly visible. Claim it before using any proceed or map fallback.",
                0.93,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "rewards",
                1400);
        }

        var proceedChoice = request.Observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, request.WindowBounds))
            .Where(choice => IsSkipOrProceedLabel(choice.Label))
            .OrderByDescending(ScoreExplicitRewardProgressionChoice)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (proceedChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                proceedChoice,
                IsSkipLikeLabel(proceedChoice.Label) ? "reward skip" : "proceed after resolving rewards",
                $"Reward proceed choice '{proceedChoice.Label}' is explicitly visible. Use it before any screenshot-derived map fallback.",
                0.92,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "rewards",
                1400);
        }

        var proceedNode = request.Observer.ActionNodes
            .Where(node => IsCurrentRewardProgressionNode(node, request.WindowBounds))
            .Where(IsProceedNode)
            .OrderByDescending(ScoreExplicitRewardProgressionNode)
            .ThenBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        if (proceedNode is not null)
        {
            return CreateClickDecisionFromNode(request, proceedNode, "proceed after resolving rewards");
        }

        var screenshotClaimableDecision = TryCreateScreenshotClaimableRewardDecision(request);
        if (screenshotClaimableDecision is not null)
        {
            return screenshotClaimableDecision;
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateScreenshotClaimableRewardDecision(GuiSmokeStepRequest request)
    {
        if (!HasScreenshotClaimableRewardEvidenceInScreenshot(request.Observer, request.ScreenshotPath))
        {
            return null;
        }

        var rewardCardTarget = GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(request.Observer)
            ? "colorless card choice"
            : "reward card choice";
        var rewardRowAnalysis = AutoRewardRowAnalyzer.Analyze(request.ScreenshotPath);
        if (rewardRowAnalysis.HasSelectableRewardRow)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                rewardRowAnalysis.RowNormalizedX,
                rewardRowAnalysis.RowNormalizedY,
                rewardCardTarget,
                "A reward card row is still visible in the screenshot. Click the row before using skip, proceed, or map fallback.",
                0.94,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "rewards",
                1400,
                true,
                null);
        }

        var cardGridAnalysis = AutoEventCardGridAnalyzer.Analyze(request.ScreenshotPath);
        if (!cardGridAnalysis.HasSelectableCard)
        {
            return null;
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            cardGridAnalysis.CardNormalizedX,
            cardGridAnalysis.CardNormalizedY,
            rewardCardTarget,
            "A claimable reward card is still visible in the screenshot. Choose it before falling back to skip, proceed, or map routing.",
            0.95,
            request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "rewards",
            1400,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateRoomOverlayCleanupDecision(GuiSmokeStepRequest request)
    {
        if (!LooksLikeInspectOverlayState(request.Observer))
        {
            return null;
        }

        var overlayCleanupAttempts = CountRecentOverlayCleanupAttempts(request.History);
        if (overlayCleanupAttempts >= 2)
        {
            return new GuiSmokeStepDecision(
                "act",
                "press-key",
                "Escape",
                null,
                null,
                "inspect overlay escape",
                "Inspect overlay remained open after repeated dismiss clicks. Send Escape before retrying any room progression.",
                0.84,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1000,
                true,
                null);
        }

        return TryCreateHiddenOverlayCleanupDecision(request)
               ?? TryCreateOverlayAdvanceDecision(request);
    }

    private static GuiSmokeStepDecision? TryCreateRewardBackNavigationDecision(GuiSmokeStepRequest request)
    {
        return TryCreateRewardBackNavigationDecision(
            request,
            BuildRewardSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath));
    }

    private static GuiSmokeStepDecision? TryCreateRewardBackNavigationDecision(GuiSmokeStepRequest request, RewardSceneState rewardScene)
    {
        if (!rewardScene.RewardForegroundOwned
            || rewardScene.ReleaseStage != RewardReleaseStage.Active
            || rewardScene.ExplicitAction != RewardExplicitActionKind.Back
            || rewardScene.LayerState.MapCurrentActiveScreen)
        {
            return null;
        }

        var backNavigationAvailable = rewardScene.LayerState.RewardBackNavigationAvailable;
        if (!backNavigationAvailable || !rewardScene.LayerState.MapContextVisible)
        {
            return null;
        }

        if (!rewardScene.LayerState.StaleRewardChoicePresent)
        {
            return null;
        }

        var backNode = request.Observer.ActionNodes.FirstOrDefault(node => node.Actionable && IsBackNode(node) && HasActiveRewardBounds(node.ScreenBounds, request.WindowBounds));
        if (backNode is not null)
        {
            return CreateClickDecisionFromNode(request, backNode, "reward back");
        }

        return null;
    }

    private static bool LooksLikeRewardBackNavigationAffordanceInScreenshot(ObserverSummary observer, string? screenshotPath)
    {
        if (BuildRewardMapLayerState(observer, null).RewardBackNavigationAvailable)
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

    private static bool HasScreenshotClaimableRewardEvidenceInScreenshot(ObserverSummary observer, string? screenshotPath)
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

        return AutoRewardRowAnalyzer.Analyze(screenshotPath).HasSelectableRewardRow
               || AutoEventCardGridAnalyzer.Analyze(screenshotPath).HasSelectableCard;
    }

    private static GuiSmokeStepDecision? TryCreateRewardChoiceDecision(GuiSmokeStepRequest request)
    {
        return TryCreateRewardChoiceDecision(
            request,
            BuildRewardSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath));
    }

    private static GuiSmokeStepDecision? TryCreateRewardChoiceDecision(GuiSmokeStepRequest request, RewardSceneState rewardScene)
    {
        if (!rewardScene.RewardForegroundOwned
            || rewardScene.ReleaseStage != RewardReleaseStage.Active
            || rewardScene.ExplicitAction is not RewardExplicitActionKind.Claim and not RewardExplicitActionKind.CardChoice and not RewardExplicitActionKind.ColorlessChoice)
        {
            return null;
        }

        var cardChoiceDecision = TryCreateCardRewardChoiceDecision(request);
        if (cardChoiceDecision is not null)
        {
            return cardChoiceDecision;
        }

        var bestChoice = request.Observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, request.WindowBounds))
            .Where(choice => !IsSkipOrProceedLabel(choice.Label))
            .OrderByDescending(ScoreExplicitRewardProgressionChoice)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (bestChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                bestChoice,
                "claim reward item",
                $"Reward choice '{bestChoice.Label}' is explicitly visible. Claim it before using any proceed or map fallback.",
                0.93,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "rewards",
                1400);
        }

        var bestNode = request.Observer.ActionNodes
            .Where(node => node.Actionable && IsCurrentRewardProgressionNode(node, request.WindowBounds))
            .Where(node => !IsProceedNode(node))
            .OrderByDescending(ScoreExplicitRewardProgressionNode)
            .ThenBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        if (bestNode is not null)
        {
            return CreateClickDecisionFromNode(request, bestNode, "claim reward item");
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateCardRewardChoiceDecision(GuiSmokeStepRequest request)
    {
        var rewardCardTarget = GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(request.Observer)
            ? "colorless card choice"
            : "reward card choice";
        var explicitRewardCardChoice = request.Observer.Choices
            .Where(choice => IsRewardCardChoice(choice) && HasActiveRewardBounds(choice.ScreenBounds, request.WindowBounds))
            .OrderByDescending(ScoreExplicitRewardProgressionChoice)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (explicitRewardCardChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                explicitRewardCardChoice,
                rewardCardTarget,
                "Reward card row is explicitly visible. Open it before using skip or any screenshot-derived fallback.",
                0.95,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "rewards",
                1400);
        }

        var hasExplicitRewardCardChoice = false;
        var cardGridAnalysis = AutoEventCardGridAnalyzer.Analyze(request.ScreenshotPath);
        var rewardRowAnalysis = AutoRewardRowAnalyzer.Analyze(request.ScreenshotPath);
        var canRescueMissingRewardCard = !hasExplicitRewardCardChoice
                                         && GuiSmokeRewardSceneSignals.HasRewardChoiceAuthority(request.Observer)
                                         && BuildRewardMapLayerState(request.Observer, request.WindowBounds).RewardPanelVisible
                                         && (rewardRowAnalysis.HasSelectableRewardRow || cardGridAnalysis.HasSelectableCard)
                                         && request.Observer.Choices.All(static choice => !IsCurrentRewardProgressionChoice(choice, null) || IsSkipOrProceedLabel(choice.Label));
        if (!hasExplicitRewardCardChoice && !canRescueMissingRewardCard)
        {
            return null;
        }

        var lastTarget = request.History
            .LastOrDefault(entry =>
                string.Equals(entry.Phase, request.Phase, StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase))
            ?.TargetLabel;
        var confirmChoice = request.Observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, request.WindowBounds) && IsConfirmLikeLabel(choice.Label))
            .OrderByDescending(GetChoiceSortX)
            .FirstOrDefault();
        var canChooseCard = !string.Equals(lastTarget, rewardCardTarget, StringComparison.OrdinalIgnoreCase);
        if (canChooseCard && rewardRowAnalysis.HasSelectableRewardRow)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                rewardRowAnalysis.RowNormalizedX,
                rewardRowAnalysis.RowNormalizedY,
                rewardCardTarget,
                "A reward card row is still visible in the screenshot. Open it before pressing confirm or skip.",
                0.94,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "rewards",
                1400,
                true,
                null);
        }

        if (canChooseCard && cardGridAnalysis.HasSelectableCard)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                cardGridAnalysis.CardNormalizedX,
                cardGridAnalysis.CardNormalizedY,
                rewardCardTarget,
                GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(request.Observer)
                    ? "Colorless reward choice is visible. Select a card from the card area instead of clicking the relic inspect icons."
                    : "Reward card choice is visible. Select a card before pressing confirm or skip.",
                0.94,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1400,
                true,
                null);
        }

        if (confirmChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                confirmChoice,
                "event confirm",
                "Card selection substate is active. Confirm the current card selection.",
                0.92,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1400);
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateEventProgressChoiceDecision(GuiSmokeStepRequest request)
    {
        var bestAncientDialogueNode = AncientEventObserverSignals.GetActiveDialogueNode(request.Observer, request.WindowBounds);
        if (bestAncientDialogueNode is not null)
        {
            return CreateClickDecisionFromNode(request, bestAncientDialogueNode, "ancient dialogue advance");
        }

        var bestAncientDialogueChoice = AncientEventObserverSignals.GetActiveDialogueChoice(request.Observer, request.WindowBounds);
        if (bestAncientDialogueChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                bestAncientDialogueChoice,
                "ancient dialogue advance",
                "Ancient event dialogue is still active. Advance it using the explicit dialogue hitbox before selecting an option.",
                0.94,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                800);
        }

        var bestNode = request.Observer.ActionNodes
            .Where(node =>
                node.Actionable
                && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds)
                && !AncientEventObserverSignals.IsAncientDialogueNode(node)
                && ScoreProgressionNode(node) > 0)
            .OrderByDescending(ScoreProgressionNode)
            .ThenBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        if (bestNode is not null)
        {
            return CreateClickDecisionFromNode(request, bestNode, GetProgressChoiceTargetLabel(bestNode, request.Observer));
        }

        var bestChoice = request.Observer.Choices
            .Where(choice =>
                HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds)
                && !AncientEventObserverSignals.IsAncientDialogueChoice(choice)
                && ScoreProgressionChoice(choice) > 0)
            .OrderByDescending(ScoreProgressionChoice)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (bestChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                bestChoice,
                GetProgressChoiceTargetLabel(bestChoice, request.Observer),
                BuildProgressChoiceReason(bestChoice, request.Observer),
                0.90,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1400);
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateAncientDialogueAdvanceDecision(GuiSmokeStepRequest request)
    {
        if (!GuiSmokeNonCombatContractSupport.AllowsAction(request, "click ancient dialogue advance"))
        {
            return null;
        }

        var node = AncientEventObserverSignals.GetActiveDialogueNode(request.Observer, request.WindowBounds);
        if (node is not null)
        {
            return CreateClickDecisionFromNode(request, node, "ancient dialogue advance");
        }

        var choice = AncientEventObserverSignals.GetActiveDialogueChoice(request.Observer, request.WindowBounds);
        if (choice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                choice,
                "ancient dialogue advance",
                "Ancient event dialogue is still active. Advance it through the explicit dialogue hitbox before selecting a reward option.",
                0.95,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                800);
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateAncientEventOptionDecision(GuiSmokeStepRequest request)
    {
        if (!GuiSmokeNonCombatContractSupport.AllowsAction(request, "click event choice"))
        {
            return null;
        }

        var node = AncientEventObserverSignals.GetActiveOptionNodes(request.Observer, request.WindowBounds)
            .FirstOrDefault();
        if (node is not null)
        {
            return CreateClickDecisionFromNode(request, node, "event progression choice");
        }

        var choice = AncientEventObserverSignals.GetActiveOptionChoices(request.Observer, request.WindowBounds)
            .FirstOrDefault();
        if (choice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                choice,
                "event progression choice",
                $"Ancient event option '{choice.Label}' is exported from an explicit NEventOptionButton. Prefer its real button bounds over generic title/layout pseudo-choices.",
                0.94,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1400);
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateAncientEventCompletionDecision(GuiSmokeStepRequest request)
    {
        if (!GuiSmokeNonCombatContractSupport.AllowsAction(request, "click ancient event completion")
            && !GuiSmokeNonCombatContractSupport.AllowsAction(request, "click event choice"))
        {
            return null;
        }

        if (AncientEventObserverSignals.CompletionUsesDefaultFocus(request.Observer)
            && AncientEventObserverSignals.CompletionHasFocus(request.Observer))
        {
            return CreateNonCombatPressKeyDecision(
                "Enter",
                "ancient event completion",
                "Ancient completion is exported from the default-focused NEventOptionButton and the control already has focus. Use ui_select (Enter) so the canonical focused control handles the release.",
                0.96,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1400);
        }

        var node = AncientEventObserverSignals.GetActiveCompletionNode(request.Observer, request.WindowBounds);
        if (node is not null)
        {
            return CreateClickDecisionFromNode(
                request,
                node,
                "ancient event completion",
                AncientEventObserverSignals.CompletionUsesDefaultFocus(request.Observer)
                    ? "Ancient completion is exported from an explicit NEventOptionButton, but the control does not currently have focus. Use a hover-primed click so NClickableControl can enter its focused mouse-release path before event release."
                    : $"Ancient event completion '{node.Label}' is still exported from an explicit NEventOptionButton proceed lane. Finish the event before handing off to map routing.",
                AncientEventObserverSignals.CompletionUsesDefaultFocus(request.Observer) ? 0.94 : 0.95,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1400);
        }

        var choice = AncientEventObserverSignals.GetActiveCompletionChoice(request.Observer, request.WindowBounds);
        if (choice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                choice,
                "ancient event completion",
                AncientEventObserverSignals.CompletionUsesDefaultFocus(request.Observer)
                    ? "Ancient completion is exported from an explicit NEventOptionButton, but the control does not currently have focus. Use a hover-primed click so NClickableControl can enter its focused mouse-release path before event release."
                    : $"Ancient event completion '{choice.Label}' is still exported from an explicit NEventOptionButton proceed lane. Finish the event before handing off to map routing.",
                AncientEventObserverSignals.CompletionUsesDefaultFocus(request.Observer) ? 0.94 : 0.95,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1400);
        }

        return null;
    }

    private static GuiSmokeStepDecision DecideHandleCombat(GuiSmokeStepRequest request, GuiSmokeStepAnalysisContext? analysisContext = null)
    {
        var context = analysisContext ?? GuiSmokeStepAnalysisContext.CreateForHandleCombatRequest(request);
        var analysis = context.CombatAnalysis;
        var handAnalysis = context.CombatHandAnalysis;
        var combatContext = context.CombatContext;
        var pendingSelection = context.PendingCombatSelection;
        var runtimeCombatState = context.RuntimeCombatState;
        var combatBarrier = context.CombatBarrierEvaluation;
        if (context.CombatPlayerActionWindowClosed)
        {
            return CreatePhaseWaitDecision(GuiSmokePhase.HandleCombat, "observer reports enemy turn or a closed combat play phase", request.Observer.CurrentScreen);
        }

        if (combatBarrier.IsActive && combatBarrier.IsHardWaitBarrier)
        {
            return CreateCombatBarrierWaitDecision(combatBarrier, request.Observer.CurrentScreen);
        }

        var hasSelectedNonEnemyConfirmEvidence = context.HasSelectedNonEnemyConfirmEvidence;
        var enemyTargetOpportunity = context.CanResolveCombatEnemyTarget;
        var combatNoOpLoop = combatContext.CombatNoOpLoop;
        var combatNoOpCountsBySlot = combatContext.CombatNoOpCountsBySlot;
        var repeatedNonEnemyLoop = combatContext.RepeatedNonEnemyLoop;
        var repeatedAttackSelectionLoop = combatContext.RepeatedAttackSelectionLoop;
        var observerHasAttackCard = request.Observer.CombatHand.Any(card =>
            card.SlotIndex >= 1
            && card.SlotIndex <= 5
            && IsAttackCombatHandCard(card)
            && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge));
        var observerHandHasOnlyNonEnemyOrInertCards = request.Observer.CombatHand.Count > 0
            && request.Observer.CombatHand.All(card =>
                IsNonEnemyCombatHandCard(card)
                || IsInertCombatHandCard(card)
                || !IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge));
        var hardBlockedAttackSlots = combatNoOpCountsBySlot
            .Where(static pair => pair.Value >= 2)
            .Select(static pair => pair.Key)
            .ToHashSet();
        var softBlockedAttackSlots = combatNoOpCountsBySlot
            .Where(static pair => pair.Value >= 1)
            .Select(static pair => pair.Key)
            .ToHashSet();
        var alternatePlayableAttackSlots = request.CombatCardKnowledge
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .Where(card => IsEnemyTargetCombatCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy))
            .Select(static card => card.SlotIndex)
            .Concat(request.Observer.CombatHand
                .Where(card => card.SlotIndex is >= 1 and <= 5)
                .Where(card => IsAttackCombatHandCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge))
                .Select(static card => card.SlotIndex))
            .Distinct()
            .Where(slotIndex => pendingSelection?.SlotIndex is null || slotIndex != pendingSelection.SlotIndex)
            .Where(slotIndex => !hardBlockedAttackSlots.Contains(slotIndex))
            .OrderBy(static slotIndex => slotIndex)
            .ToArray();
        var pendingSelectionNoOpCount = pendingSelection?.Kind == AutoCombatCardKind.AttackLike && pendingSelection.SlotIndex is >= 1 and <= 5
            ? HandleCombatContextSupport.GetCombatNoOpCountForSlot(combatContext, pendingSelection.SlotIndex)
            : 0;
        bool ShouldSuppressPendingNonEnemyReselect(int slotIndex)
        {
            if (pendingSelection?.Kind == AutoCombatCardKind.DefendLike
                && hasSelectedNonEnemyConfirmEvidence)
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

        bool TryUseCombatDecision(GuiSmokeStepDecision? candidate, out GuiSmokeStepDecision allowedDecision)
        {
            allowedDecision = default!;
            if (candidate is null || !CombatDecisionContract.IsAllowed(request, candidate, out _))
            {
                return false;
            }

            allowedDecision = candidate;
            return true;
        }

        GuiSmokeStepDecision CloseWithLegalCombatFallback()
        {
            var fallbackDecision = CreateCombatPressKeyDecision(
                "E",
                "auto-end turn",
                "No clear playable card remains in the screenshot. End the turn.",
                0.88,
                200);
            if (TryUseCombatDecision(fallbackDecision, out var allowedFallback))
            {
                return allowedFallback;
            }

            return CreatePhaseWaitDecision(GuiSmokePhase.HandleCombat, "waiting for legal combat action", request.Observer.CurrentScreen);
        }

        if (analysis.HasTargetArrow)
        {
            if (TryCreateCombatEnemyTargetDecision(request, pendingSelection, pendingSelectionNoOpCount, alternatePlayableAttackSlots, out var targetDecision)
                && TryUseCombatDecision(targetDecision, out var allowedTargetDecision))
            {
                return allowedTargetDecision;
            }
        }

        if (request.Observer.PlayerEnergy is <= 0)
        {
            if (TryUseCombatDecision(CreateCombatPressKeyDecision(
                "E",
                "auto-end turn",
                "Observer reports no remaining energy. End the turn instead of retrying non-playable cards.",
                0.92,
                200), out var allowedNoEnergyDecision))
            {
                return allowedNoEnergyDecision;
            }
        }

        if (hasSelectedNonEnemyConfirmEvidence)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "confirm-non-enemy",
                null,
                null,
                null,
                "confirm selected non-enemy card",
                "A self or non-enemy targeted card is selected. Move to the explicit confirm point, hold left mouse briefly, then release to finish the play.",
                0.82,
                "combat",
                150,
                true,
                null), out var allowedNonEnemyConfirmDecision))
            {
                return allowedNonEnemyConfirmDecision;
            }
        }

        if (analysis.HasSelectedCard
            && analysis.SelectedCardKind == AutoCombatCardKind.AttackLike
            && pendingSelection?.Kind == AutoCombatCardKind.AttackLike
            && enemyTargetOpportunity)
        {
            if (TryCreateCombatEnemyTargetDecision(request, pendingSelection, pendingSelectionNoOpCount, alternatePlayableAttackSlots, out var targetDecision)
                && TryUseCombatDecision(targetDecision, out var allowedTargetDecision))
            {
                return allowedTargetDecision;
            }
        }

        if (pendingSelection?.Kind == AutoCombatCardKind.AttackLike
            && enemyTargetOpportunity)
        {
            if (TryCreateCombatEnemyTargetDecision(request, pendingSelection, pendingSelectionNoOpCount, alternatePlayableAttackSlots, out var targetDecision)
                && TryUseCombatDecision(targetDecision, out var allowedTargetDecision))
            {
                return allowedTargetDecision;
            }
        }

        if (pendingSelection?.Kind == AutoCombatCardKind.AttackLike
            && analysis.HasSelectedCard
            && !enemyTargetOpportunity)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "right-click",
                null,
                null,
                null,
                "cancel unresolved selected card",
                "A stale attack selection is still highlighted, but the observer no longer shows a matching playable attack. Cancel it before choosing a new card or ending the turn.",
                0.80,
                "combat",
                250,
                true,
                null), out var allowedCancelDecision))
            {
                return allowedCancelDecision;
            }
        }

        if (hasSelectedNonEnemyConfirmEvidence)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "confirm-non-enemy",
                null,
                null,
                null,
                "confirm selected non-enemy card",
                "A non-enemy card overlay is still selected. Use the explicit confirm point with a brief held mouse press instead of reissuing the slot hotkey.",
                0.78,
                "combat",
                150,
                true,
                null), out var allowedSelectedDefendDecision))
            {
                return allowedSelectedDefendDecision;
            }
        }

        var knowledgeAttackSlot = request.CombatCardKnowledge
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => IsEnemyTargetCombatCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy))
            .Where(card => !hardBlockedAttackSlots.Contains(card.SlotIndex))
            .Where(card => !CombatBarrierSupport.SuppressesAttackSlot(combatBarrier, card.SlotIndex))
            .Where(card => pendingSelectionNoOpCount <= 0 || !softBlockedAttackSlots.Contains(card.SlotIndex) || card.SlotIndex != pendingSelection?.SlotIndex)
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var observerAttackSlot = request.Observer.CombatHand
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => IsAttackCombatHandCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge))
            .Where(card => !hardBlockedAttackSlots.Contains(card.SlotIndex))
            .Where(card => !CombatBarrierSupport.SuppressesAttackSlot(combatBarrier, card.SlotIndex))
            .Where(card => pendingSelectionNoOpCount <= 0 || !softBlockedAttackSlots.Contains(card.SlotIndex) || card.SlotIndex != pendingSelection?.SlotIndex)
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var attackFallbackBlockedByObserver = request.Observer.CombatHand.Count > 0
            && (observerHandHasOnlyNonEnemyOrInertCards || !observerHasAttackCard);
        var attackSlot = !attackFallbackBlockedByObserver && knowledgeAttackSlot is not null
            ? new AutoCombatHandSlotAnalysis(
                knowledgeAttackSlot.SlotIndex,
                true,
                AutoCombatCardKind.AttackLike,
                double.MaxValue,
                double.MaxValue,
                0,
                0)
            : !attackFallbackBlockedByObserver && observerAttackSlot is not null
                ? new AutoCombatHandSlotAnalysis(
                observerAttackSlot.SlotIndex,
                true,
                AutoCombatCardKind.AttackLike,
                double.MaxValue,
                double.MaxValue,
                0,
                0)
                : attackFallbackBlockedByObserver
                    ? null
                    : handAnalysis.Slots
                    .Where(slot =>
                        slot.IsVisible
                        && slot.Kind == AutoCombatCardKind.AttackLike
                        && IsCompatibleScreenshotCombatSlot(slot, request.Observer, request.CombatCardKnowledge, expectEnemyTarget: true)
                        && !hardBlockedAttackSlots.Contains(slot.SlotIndex)
                        && !CombatBarrierSupport.SuppressesAttackSlot(combatBarrier, slot.SlotIndex)
                        && (pendingSelectionNoOpCount <= 0 || !softBlockedAttackSlots.Contains(slot.SlotIndex) || slot.SlotIndex != pendingSelection?.SlotIndex))
                    .OrderByDescending(static slot => slot.RedBlueDelta)
                    .ThenByDescending(static slot => slot.Brightness)
                    .FirstOrDefault();
        if (attackSlot is not null)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "press-key",
                GetCombatSlotHotkey(attackSlot.SlotIndex),
                null,
                null,
                $"combat select attack slot {attackSlot.SlotIndex}",
                hardBlockedAttackSlots.Contains(attackSlot.SlotIndex) || pendingSelectionNoOpCount > 0
                    ? "Recent combat history shows no board delta on another lane. Switch to a different playable attack slot before trying to target the enemy again."
                    : "The screenshot still shows a playable attack card in hand. Use the corresponding hotkey first, then target the enemy.",
                hardBlockedAttackSlots.Count == 0 && pendingSelectionNoOpCount == 0 ? 0.80 : 0.88,
                "combat",
                120,
                true,
                null), out var allowedAttackSlotDecision))
            {
                return allowedAttackSlotDecision;
            }
        }

        var knowledgeNonEnemySlot = request.CombatCardKnowledge
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatCard(card, request.Observer.PlayerEnergy))
            .Where(card => !CombatBarrierSupport.SuppressesNonEnemySlot(combatBarrier, card.SlotIndex))
            .Where(card => !ShouldSuppressPendingNonEnemyReselect(card.SlotIndex))
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var observerNonEnemySlot = request.Observer.CombatHand
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatHandCard(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge))
            .Where(card => !CombatBarrierSupport.SuppressesNonEnemySlot(combatBarrier, card.SlotIndex))
            .Where(card => !ShouldSuppressPendingNonEnemyReselect(card.SlotIndex))
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var nonEnemySlot = knowledgeNonEnemySlot is not null
            ? new AutoCombatHandSlotAnalysis(
                knowledgeNonEnemySlot.SlotIndex,
                true,
                AutoCombatCardKind.DefendLike,
                double.MinValue,
                double.MaxValue,
                0,
                0)
            : observerNonEnemySlot is not null
                ? new AutoCombatHandSlotAnalysis(
                observerNonEnemySlot.SlotIndex,
                true,
                AutoCombatCardKind.DefendLike,
                double.MinValue,
                double.MaxValue,
                0,
                0)
                : handAnalysis.Slots
                .Where(slot =>
                    slot.IsVisible
                    && slot.Kind == AutoCombatCardKind.DefendLike
                    && IsCompatibleScreenshotCombatSlot(slot, request.Observer, request.CombatCardKnowledge, expectEnemyTarget: false)
                    && !CombatBarrierSupport.SuppressesNonEnemySlot(combatBarrier, slot.SlotIndex)
                    && !ShouldSuppressPendingNonEnemyReselect(slot.SlotIndex))
                .OrderBy(static slot => slot.RedBlueDelta)
                .ThenByDescending(static slot => slot.Brightness)
                .FirstOrDefault();
        if (nonEnemySlot is not null)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "press-key",
                GetCombatSlotHotkey(nonEnemySlot.SlotIndex),
                null,
                null,
                $"combat select non-enemy slot {nonEnemySlot.SlotIndex}",
                "Only non-enemy cards remain in hand. Use the corresponding hotkey, then resolve the self or non-enemy confirmation.",
                0.74,
                "combat",
                120,
                true,
                null), out var allowedNonEnemySlotDecision))
            {
                return allowedNonEnemySlotDecision;
            }
        }

        var blockedOpenAttackSelection = pendingSelection?.Kind == AutoCombatCardKind.AttackLike
                                         && pendingSelection.SlotIndex is >= 1 and <= 5
                                         && CombatRuntimeStateSupport.HasSelectionToKeep(request.Observer, request.CombatCardKnowledge)
                                         && HandleCombatContextSupport.GetCombatNoOpCountForSlot(combatContext, pendingSelection.SlotIndex) >= 2;
        if (blockedOpenAttackSelection)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "right-click",
                null,
                null,
                null,
                "cancel unresolved selected card",
                $"The selected attack lane {pendingSelection!.SlotIndex} is still open after repeated no-op outcomes. Cancel it before ending the turn or choosing another lane.",
                0.84,
                "combat",
                250,
                true,
                null), out var allowedBlockedSelectionCancelDecision))
            {
                return allowedBlockedSelectionCancelDecision;
            }
        }

        var shouldDeferLoopEndTurn = runtimeCombatState.KeepsCardPlayOpen
                                     || CombatRuntimeStateSupport.ShouldDeferLoopEndTurn(
                                         request.Observer,
                                         request.CombatCardKnowledge);

        if (analysis.HasSelectedCard && HasRecentCombatCardSelection(request.History))
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "right-click",
                null,
                null,
                null,
                "cancel unresolved selected card",
                "A lingering selected card is still visible after the prior combat action. Cancel it before continuing.",
                0.72,
                "combat",
                250,
                true,
                null), out var allowedLingeringSelectionDecision))
            {
                return allowedLingeringSelectionDecision;
            }
        }

        if (repeatedNonEnemyLoop && !shouldDeferLoopEndTurn)
        {
            var endTurnNode = FindEndTurnActionNode(request);
            if (endTurnNode is not null
                && TryUseCombatDecision(
                    CreateCombatEndTurnDecisionFromNode(request, endTurnNode, "end turn after repeated non-enemy loop"),
                    out var allowedRepeatedNonEnemyDecision))
            {
                return allowedRepeatedNonEnemyDecision;
            }

            if (TryUseCombatDecision(CreateCombatPressKeyDecision(
                "E",
                "end turn after repeated non-enemy loop",
                "Recent combat history shows a repeated non-enemy select/confirm loop. End the turn instead of repeating the same sequence.",
                0.86,
                200), out var allowedRepeatedNonEnemyFallback))
            {
                return allowedRepeatedNonEnemyFallback;
            }
        }

        if (repeatedAttackSelectionLoop && !observerHasAttackCard && !shouldDeferLoopEndTurn)
        {
            var endTurnNode = FindEndTurnActionNode(request);
            if (endTurnNode is not null
                && TryUseCombatDecision(
                    CreateCombatEndTurnDecisionFromNode(request, endTurnNode, "end turn after repeated attack-select loop"),
                    out var allowedRepeatedAttackDecision))
            {
                return allowedRepeatedAttackDecision;
            }

            if (TryUseCombatDecision(CreateCombatPressKeyDecision(
                "E",
                "end turn after repeated attack-select loop",
                "Recent combat history shows repeated attack hotkeys without a matching observer attack card. End the turn instead of looping on screenshot drift.",
                0.88,
                200), out var allowedRepeatedAttackFallback))
            {
                return allowedRepeatedAttackFallback;
            }
        }

        if (combatNoOpLoop.LoopDetected && !shouldDeferLoopEndTurn)
        {
            var endTurnNode = FindEndTurnActionNode(request);
            if (endTurnNode is not null
                && TryUseCombatDecision(
                    CreateCombatEndTurnDecisionFromNode(request, endTurnNode, "end turn after combat no-op loop"),
                    out var allowedCombatNoOpDecision))
            {
                return allowedCombatNoOpDecision;
            }

            if (TryUseCombatDecision(CreateCombatPressKeyDecision(
                "E",
                "end turn after combat no-op loop",
                "Combat has repeated the same card-select and enemy-target sequence without progress. End the turn instead of looping on an unproductive lane.",
                0.90,
                200), out var allowedCombatNoOpFallback))
            {
                return allowedCombatNoOpFallback;
            }
        }

        return CloseWithLegalCombatFallback();
    }

    private static GuiSmokeDecisionAnalysis AnalyzeGenericPhase(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision? actualDecision,
        Func<GuiSmokeStepDecision> factory,
        string? foregroundKind,
        string? backgroundKind)
    {
        var builder = new DecisionAnalysisBuilder(request, foregroundKind, backgroundKind);
        var predictedDecision = factory();
        builder.Consider(
            predictedDecision.TargetLabel ?? predictedDecision.ActionKind ?? predictedDecision.Status,
            "phase-generic",
            predictedDecision.Confidence ?? 0.50d,
            () => predictedDecision,
            "generic-phase-candidate-unavailable",
            boundsSource: predictedDecision.ActionKind is "click" or "right-click" ? "decision-normalized" : "decision-nonpoint");
        return builder.Build(CreateWaitDecision("waiting for passive phase", request.Observer.CurrentScreen), actualDecision);
    }

    private static GuiSmokeDecisionAnalysis AnalyzeHandleRewards(GuiSmokeStepRequest request, GuiSmokeStepDecision? actualDecision)
    {
        var rewardScene = BuildRewardSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath);
        var rewardMapLayer = rewardScene.LayerState;
        var rewardState = rewardScene.ScreenState;
        var (foregroundKind, backgroundKind) = DescribeForegroundBackground(request);
        var builder = new DecisionAnalysisBuilder(request, foregroundKind, backgroundKind);

        var overlayDecision = TryCreateRoomOverlayCleanupDecision(request);
        builder.Consider("click inspect overlay close", "overlay-cleanup", 1.00d, () => overlayDecision, "no-room-overlay-cleanup", boundsSource: "overlay-cleanup");

        var cardSelectionDecision = TryCreateCardSelectionDecision(request);
        builder.Consider(
            ToCandidateLabel(cardSelectionDecision, "click card-selection subtype"),
            "card-selection-runtime",
            0.98d,
            () => cardSelectionDecision,
            "no-card-selection-subtype-decision");
        if (CardSelectionObserverSignals.IsCardSelectionState(request.Observer))
        {
            builder.AddSuppressed("click reward choice", "card-selection-subtype-foreground-suppresses-reward-row-reuse");
            builder.AddSuppressed("click reward back", "card-selection-subtype-foreground-suppresses-reward-back-nav");
            builder.AddSuppressed("click first reachable node", "card-selection-subtype-foreground-suppresses-map-fallback");
            builder.AddSuppressed("click visible map advance", "card-selection-subtype-foreground-suppresses-map-fallback");
            return builder.Build(CreateWaitDecision("waiting for card-selection subtype progression", request.Observer.CurrentScreen), actualDecision);
        }

        var explicitRewardDecision = TryCreateExplicitRewardResolutionDecision(request, rewardScene);
        builder.Consider(
            ToCandidateLabel(explicitRewardDecision, "click reward choice"),
            "reward-explicit",
            0.95d,
            () => explicitRewardDecision,
            "no-explicit-reward-progression",
            rawBounds: TryFindRewardBounds(request),
            boundsSource: "observer-reward");

        var rewardBackDecision = TryCreateRewardBackNavigationDecision(request, rewardScene);
        builder.Consider("click reward back", "reward-back-nav", 0.88d, () => rewardBackDecision, "reward-back-not-available", rawBounds: TryFindBackBounds(request), boundsSource: "observer-back");

        if (rewardState is { TerminalRunBoundary: true })
        {
            builder.AddSuppressed("click first reachable node", "terminal-boundary-suppresses-gameplay-map-fallback");
            builder.AddSuppressed("click visible map advance", "terminal-boundary-suppresses-gameplay-map-fallback");
        }
        else if (rewardScene.ReleaseToMapPending
                 || (!rewardScene.RewardForegroundOwned
                     && GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(request.Observer)))
        {
            builder.AddSuppressed("click first reachable node", "reward-ownership-release-defers-map-routing-to-waitmap-reconciliation");
            builder.AddSuppressed("click visible map advance", "reward-ownership-release-defers-map-routing-to-waitmap-reconciliation");
            return builder.Build(CreateRewardOwnershipReleaseWaitDecision(request), actualDecision);
        }
        else
        {
            builder.AddSuppressed("click first reachable node", "reward-foreground-keeps-map-fallback-suppressed");
            builder.AddSuppressed("click visible map advance", "reward-foreground-keeps-map-fallback-suppressed");
        }

        return builder.Build(CreateWaitDecision("waiting for reward actions", request.Observer.CurrentScreen), actualDecision);
    }

    private static GuiSmokeDecisionAnalysis AnalyzeChooseFirstNode(GuiSmokeStepRequest request, GuiSmokeStepDecision? actualDecision)
    {
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        var (foregroundKind, backgroundKind) = DescribeForegroundBackground(request);
        var builder = new DecisionAnalysisBuilder(request, foregroundKind, backgroundKind);

        if (HasExplicitRestSiteChoiceAuthority(request))
        {
            builder.AddSuppressed("click exported reachable node", "rest-site-explicit-choices-outrank-exported-map-routing");
            builder.AddSuppressed("click first reachable node", "rest-site-explicit-choices-outrank-screenshot-map-routing");
            builder.AddSuppressed("click visible map advance", "rest-site-explicit-choices-suppress-current-node-arrow");
            builder.AddSuppressed("click map back", "rest-site-explicit-choices-are-the-progression-lane");
            foreach (var restSiteCandidate in BuildRestSiteDecisionCandidates(request))
            {
                var capturedCandidate = restSiteCandidate;
                builder.Consider(
                    capturedCandidate.Label,
                    capturedCandidate.Source,
                    capturedCandidate.Score,
                    () => capturedCandidate.Decision,
                    capturedCandidate.RejectReason ?? "no-rest-site-explicit-choice",
                    rawBounds: capturedCandidate.RawBounds,
                    boundsSource: capturedCandidate.BoundsSource);
            }

            builder.Consider(
                ToCandidateLabel(TryCreateRestSiteUpgradeDecision(request), "click smith card"),
                "rest-site-upgrade",
                0.92d,
                () => TryCreateRestSiteUpgradeDecision(request),
                "rest-site-upgrade-grid-not-visible");
            return builder.Build(CreateWaitDecision("waiting for an explicit rest-site choice", request.Observer.CurrentScreen), actualDecision);
        }

        if (LooksLikeRestSiteState(request.Observer)
            && TryCreateRestSiteUpgradeDecision(request) is { } observerUpgradeDecision)
        {
            builder.AddSuppressed("click exported reachable node", "rest-site-upgrade-outranks-exported-map-routing");
            builder.AddSuppressed("click first reachable node", "rest-site-upgrade-outranks-screenshot-map-routing");
            builder.AddSuppressed("click visible map advance", "rest-site-upgrade-suppresses-current-node-arrow");
            builder.AddSuppressed("click map back", "rest-site-upgrade-remains-foreground-authority");
            builder.Consider(
                ToCandidateLabel(observerUpgradeDecision, "click smith card"),
                "rest-site-upgrade",
                observerUpgradeDecision.TargetLabel?.Contains("confirm", StringComparison.OrdinalIgnoreCase) == true ? 0.95d : 0.92d,
                () => observerUpgradeDecision,
                "rest-site-upgrade-grid-not-visible",
                rawBounds: TryFindRestSiteUpgradeBounds(request, observerUpgradeDecision),
                boundsSource: TryResolveRestSiteUpgradeBoundsSource(request, observerUpgradeDecision));
            return builder.Build(CreateWaitDecision("waiting for smith grid or confirm state", request.Observer.CurrentScreen), actualDecision);
        }

        if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(request.Observer)
            && HasRecentRestSiteExplicitClick(request, "rest site: smith"))
        {
            builder.AddSuppressed("click exported reachable node", "rest-site-upgrade-observer-state-suppresses-map-routing");
            builder.AddSuppressed("click first reachable node", "rest-site-upgrade-observer-state-suppresses-map-routing");
            builder.AddSuppressed("click visible map advance", "rest-site-upgrade-observer-state-suppresses-current-node-arrow");
            builder.AddSuppressed("click map back", "rest-site-upgrade-observer-state-remains-foreground-authority");
            var observerMissDecision = CreateRestSitePostClickNoOpWaitDecision(request, "rest site: smith", request.Observer.CurrentScreen);
            builder.Consider(
                "click smith card",
                "rest-site-upgrade",
                0.92d,
                () => TryCreateRestSiteUpgradeDecision(request),
                "rest-site-upgrade-grid-not-visible");
            builder.Consider(
                "wait",
                "rest-site-upgrade-observer",
                0.97d,
                () => observerMissDecision,
                "rest-site-upgrade-post-click-state-unavailable");
            return builder.Build(observerMissDecision, actualDecision);
        }

        if (TreasureRoomObserverSignals.IsTreasureAuthorityActive(request.Observer))
        {
            builder.AddSuppressed("click exported reachable node", "treasure-room-explicit-affordances-outrank-map-routing");
            builder.AddSuppressed("click first reachable node", "treasure-room-explicit-affordances-outrank-screenshot-map-routing");
            builder.AddSuppressed("click visible map advance", "treasure-room-explicit-affordances-suppress-current-node-arrow");
            builder.AddSuppressed("click map back", "treasure-room-progression-is-chest-holder-proceed");
            var treasureDecision = TryCreateTreasureRoomDecision(request);
            builder.Consider(
                ToCandidateLabel(treasureDecision, "click treasure room action"),
                "treasure-room-runtime",
                0.98d,
                () => treasureDecision,
                "no-treasure-room-actionable-affordance");
            return builder.Build(CreateWaitDecision("waiting for treasure room progression", request.Observer.CurrentScreen), actualDecision);
        }

        if (mapOverlayState.ForegroundVisible)
        {
            if (mapOverlayState.StaleEventChoicePresent)
            {
                builder.AddSuppressed("click event choice", "map-overlay-foreground-removes-stale-event-choice");
            }

            if (mapOverlayState.CurrentNodeArrowVisible)
            {
                builder.AddSuppressed("click visible map advance", "current-node-arrow-is-not-a-reachable-node");
            }

            if (!GuiSmokeNonCombatContractSupport.AllowsAnyMapRoutingAction(request))
            {
                builder.AddSuppressed("click exported reachable node", "request-allowlist-excludes-map-routing");
                builder.AddSuppressed("click first reachable node", "request-allowlist-excludes-map-routing");
                builder.AddSuppressed("click map back", "request-allowlist-excludes-map-routing");
                builder.AddSuppressed("click visible map advance", "request-allowlist-excludes-map-routing");
                return builder.Build(
                    GuiSmokeNonCombatContractSupport.CreateMapRoutingContractWaitDecision(
                        request,
                        "map overlay is visible but request allowlist does not permit map routing; waiting for exporter/phase reconciliation"),
                    actualDecision);
            }

            builder.Consider(
                "click exported reachable node",
                "observer-export:map-node",
                1.00d,
                () => TryCreateExportedReachableMapPointDecision(request),
                "no-exported-reachable-node",
                rawBounds: TryFindMapNodeBounds(request),
                boundsSource: "observer-map-node");
            builder.Consider(
                "click map back",
                "overlay-back-navigation",
                0.90d,
                () => TryCreateMapBackNavigationDecision(request),
                "map-back-navigation-unavailable",
                rawBounds: TryFindBackBounds(request),
                boundsSource: "overlay-back");
            builder.Consider(
                "click first reachable node",
                "screenshot-reachable-node",
                0.82d,
                () => TryFindFirstReachableMapNodeDecision(request),
                "no-screenshot-reachable-node",
                boundsSource: "screenshot-map-node");
            return builder.Build(CreateWaitDecision("waiting for exported or screenshot-reachable map node", request.Observer.CurrentScreen), actualDecision);
        }

        if (LooksLikeScreenshotFirstRoomState(request))
        {
            if (AutoMapAnalyzer.Analyze(request.ScreenshotPath).HasCurrentArrow)
            {
                builder.AddSuppressed("click visible map advance", "room-explicit-choice-takes-priority-over-current-node-arrow");
            }

            var roomDecision = TryCreateScreenshotFirstRoomDecision(request);
            builder.Consider(
                ToCandidateLabel(roomDecision, "click room explicit choice"),
                "screenshot-room-explicit",
                0.96d,
                () => roomDecision,
                "no-room-explicit-choice");
            return builder.Build(CreateWaitDecision("waiting for reachable map node", request.Observer.CurrentScreen), actualDecision);
        }

        builder.Consider(
            "click exported reachable node",
            "observer-export:map-node",
            0.96d,
            () => TryCreateExportedReachableMapPointDecision(request),
            "no-exported-reachable-node",
            rawBounds: TryFindMapNodeBounds(request),
            boundsSource: "observer-map-node");
        builder.Consider(
            "click visible map advance",
            "screenshot-current-arrow",
            0.62d,
            () => TryFindVisibleMapAdvanceDecision(request),
            "visible-map-advance-suppressed-or-unavailable",
            boundsSource: "screenshot-map-arrow");
        return builder.Build(CreateWaitDecision("waiting for reachable map node", request.Observer.CurrentScreen), actualDecision);
    }

    private static GuiSmokeDecisionAnalysis AnalyzeHandleEvent(GuiSmokeStepRequest request, GuiSmokeStepDecision? actualDecision)
    {
        var eventScene = BuildEventSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath);
        if (eventScene.RewardSubstateActive)
        {
            return AnalyzeHandleRewards(request with { Phase = GuiSmokePhase.HandleRewards.ToString() }, actualDecision);
        }

        var forceEventProgressionAfterCardSelection = eventScene.ForceProgressionAfterCardSelection;
        var preferEventForeground = eventScene.EventForegroundOwned
                                    && eventScene.ReleaseStage == EventReleaseStage.Active;
        var mapOverlayState = eventScene.MapOverlayState;
        var strongEventForegroundChoice = eventScene.StrongForegroundChoice || forceEventProgressionAfterCardSelection;
        var (foregroundKind, backgroundKind) = DescribeForegroundBackground(request);
        var builder = new DecisionAnalysisBuilder(request, foregroundKind, backgroundKind);

        var overlayDecision = TryCreateRoomOverlayCleanupDecision(request);
        builder.Consider("click inspect overlay close", "overlay-cleanup", 1.00d, () => overlayDecision, "no-room-overlay-cleanup", boundsSource: "overlay-cleanup");

        var cardSelectionDecision = TryCreateCardSelectionDecision(request);
        builder.Consider(
            ToCandidateLabel(cardSelectionDecision, "click card-selection subtype"),
            "card-selection-runtime",
            0.99d,
            () => cardSelectionDecision,
            "no-card-selection-subtype-decision");
        if (CardSelectionObserverSignals.IsCardSelectionState(request.Observer))
        {
            builder.AddSuppressed("click reward choice", "card-selection-subtype-foreground-suppresses-reward-row-reuse");
            builder.AddSuppressed("click reward back", "card-selection-subtype-foreground-suppresses-reward-back-nav");
            builder.AddSuppressed("click event choice", "card-selection-subtype-foreground-suppresses-generic-event-choice");
            builder.AddSuppressed("click first reachable node", "card-selection-subtype-foreground-suppresses-map-fallback");
            builder.AddSuppressed("click room fallback", "card-selection-subtype-foreground-suppresses-room-fallback");
            return builder.Build(CreateWaitDecision("waiting for card-selection subtype progression", request.Observer.CurrentScreen), actualDecision);
        }

        var treasureDecision = TryCreateTreasureRoomDecision(request);
        builder.Consider(
            ToCandidateLabel(treasureDecision, "click treasure room action"),
            "treasure-room-runtime",
            0.97d,
            () => treasureDecision,
            "no-treasure-room-actionable-affordance");
        if (TreasureRoomObserverSignals.IsTreasureAuthorityActive(request.Observer))
        {
            builder.AddSuppressed("click exported reachable node", "treasure-room-explicit-affordances-outrank-map-routing");
            builder.AddSuppressed("click first reachable node", "treasure-room-explicit-affordances-outrank-screenshot-map-routing");
            builder.AddSuppressed("click visible map advance", "treasure-room-explicit-affordances-suppress-current-node-arrow");
            builder.AddSuppressed("click event choice", "treasure-room-explicit-affordances-outrank-generic-event-choice");
            builder.AddSuppressed("click proceed", "treasure-room-explicit-proceed-is-handled-by-treasure-state-machine");
            return builder.Build(CreateWaitDecision("waiting for treasure room progression", request.Observer.CurrentScreen), actualDecision);
        }

        var explicitRewardDecision = TryCreateExplicitRewardResolutionDecision(request);
        builder.Consider(
            ToCandidateLabel(explicitRewardDecision, "click reward choice"),
            "reward-explicit",
            0.96d,
            () => explicitRewardDecision,
            "no-reward-resolution-needed",
            rawBounds: TryFindRewardBounds(request),
            boundsSource: "observer-reward");

        var rewardBackDecision = TryCreateRewardBackNavigationDecision(request);
        builder.Consider("click reward back", "reward-back-nav", 0.90d, () => rewardBackDecision, "reward-back-not-available", rawBounds: TryFindBackBounds(request), boundsSource: "observer-back");

        var ancientDialogueDecision = TryCreateAncientDialogueAdvanceDecision(request);
        builder.Consider(
            "click ancient dialogue advance",
            "ancient-dialogue",
            0.98d,
            () => ancientDialogueDecision,
            "no-ancient-dialogue-hitbox",
            rawBounds: TryFindEventChoiceBounds(request),
            boundsSource: "observer-ancient-dialogue");
        if (AncientEventObserverSignals.IsDialogueActive(request.Observer))
        {
            builder.AddSuppressed("click event choice", "ancient-dialogue-must-finish-before-option-selection");
            builder.AddSuppressed("click proceed", "ancient-dialogue-does-not-use-generic-proceed");
            builder.AddSuppressed("click exported reachable node", "ancient-event-dialogue-outranks-map-routing");
            builder.AddSuppressed("click first reachable node", "ancient-event-dialogue-outranks-screenshot-map-routing");
            builder.AddSuppressed("click visible map advance", "ancient-event-dialogue-suppresses-map-arrow-fallback");
            builder.AddSuppressed("click room fallback", "ancient-event-dialogue-suppresses-room-fallback");
            return builder.Build(CreateWaitDecision("waiting for explicit ancient dialogue hitbox", request.Observer.CurrentScreen), actualDecision);
        }

        var ancientCompletionDecision = TryCreateAncientEventCompletionDecision(request);
        builder.Consider(
            "click ancient event completion",
            "ancient-completion-button",
            0.97d,
            () => ancientCompletionDecision,
            "no-explicit-ancient-completion-button",
            rawBounds: TryFindEventChoiceBounds(request),
            boundsSource: "observer-ancient-completion");
        if (AncientEventObserverSignals.HasExplicitCompletionAction(request.Observer))
        {
            builder.AddSuppressed("click proceed", "ancient-completion-remains-event-owned-through-explicit-proceed-button");
            builder.AddSuppressed("click exported reachable node", "ancient-event-completion-outranks-map-routing");
            builder.AddSuppressed("click first reachable node", "ancient-event-completion-outranks-screenshot-map-routing");
            builder.AddSuppressed("click visible map advance", "ancient-event-completion-suppresses-map-arrow-fallback");
            builder.AddSuppressed("click room fallback", "ancient-event-completion-suppresses-room-fallback");
            return builder.Build(CreateWaitDecision("waiting for explicit ancient event completion", request.Observer.CurrentScreen), actualDecision);
        }

        if (eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.ReleasePending)
        {
            builder.AddSuppressed("click proceed", "event-release-pending-suppresses-same-proceed-reissue");
            builder.AddSuppressed("click event choice", "event-release-pending-suppresses-stale-event-choice-reissue");
            builder.AddSuppressed("click exported reachable node", "event-release-pending-waits-for-explicit-release-before-map-routing");
            builder.AddSuppressed("click first reachable node", "event-release-pending-waits-for-explicit-release-before-map-routing");
            builder.AddSuppressed("click visible map advance", "event-release-pending-suppresses-map-contamination");
            builder.AddSuppressed("click room fallback", "event-release-pending-suppresses-room-fallback");
            return builder.Build(CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit event release"), actualDecision);
        }

        var ancientOptionDecision = TryCreateAncientEventOptionDecision(request);
        builder.Consider(
            "click event choice",
            "ancient-option-buttons",
            0.96d,
            () => ancientOptionDecision,
            "no-explicit-ancient-option-button",
            rawBounds: TryFindEventChoiceBounds(request),
            boundsSource: "observer-ancient-option");
        if (AncientEventObserverSignals.HasExplicitOptionSelection(request.Observer))
        {
            builder.AddSuppressed("click proceed", "ancient-event-options-should-use-explicit-option-buttons");
            builder.AddSuppressed("click exported reachable node", "ancient-event-options-outrank-map-routing");
            builder.AddSuppressed("click first reachable node", "ancient-event-options-outrank-screenshot-map-routing");
            builder.AddSuppressed("click visible map advance", "ancient-event-options-suppress-map-arrow-fallback");
            builder.AddSuppressed("click room fallback", "ancient-event-options-suppress-room-fallback");
            return builder.Build(CreateWaitDecision("waiting for explicit ancient event option buttons", request.Observer.CurrentScreen), actualDecision);
        }

        if (mapOverlayState.ForegroundVisible && !strongEventForegroundChoice)
        {
            if (mapOverlayState.StaleEventChoicePresent)
            {
                builder.AddSuppressed("click event choice", "map-overlay-foreground-removes-stale-event-choice");
            }

            if (mapOverlayState.CurrentNodeArrowVisible)
            {
                builder.AddSuppressed("click visible map advance", "current-node-arrow-is-not-a-reachable-node");
            }

            builder.Consider("click exported reachable node", "observer-export:map-node", 0.88d, () => TryCreateExportedReachableMapPointDecision(request), "no-exported-reachable-node", rawBounds: TryFindMapNodeBounds(request), boundsSource: "observer-map-node");
            builder.Consider("click map back", "overlay-back-navigation", 0.84d, () => TryCreateMapBackNavigationDecision(request), "map-back-navigation-unavailable", rawBounds: TryFindBackBounds(request), boundsSource: "overlay-back");
            builder.Consider("click first reachable node", "screenshot-reachable-node", 0.78d, () => TryFindFirstReachableMapNodeDecision(request), "no-screenshot-reachable-node", boundsSource: "screenshot-map-node");
            return builder.Build(CreateWaitDecision("waiting for an explicit event progression choice", request.Observer.CurrentScreen), actualDecision);
        }

        if (forceEventProgressionAfterCardSelection)
        {
            builder.AddSuppressed("click exported reachable node", "post-card-selection-event-explicit-progression-outranks-map-routing");
            builder.AddSuppressed("click first reachable node", "post-card-selection-event-explicit-progression-outranks-map-routing");
            builder.AddSuppressed("click visible map advance", "post-card-selection-event-explicit-progression-outranks-map-routing");
        }

        if (preferEventForeground
            && (request.SceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase)
                || request.SceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase)
                || request.SceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)))
        {
            builder.AddSuppressed("click visible map advance", "event-foreground-suppresses-background-map-contamination");
        }

        var semanticDecision = TryCreateSemanticEventDecision(request);
        builder.Consider("click event choice", "semantic-event", preferEventForeground ? 0.98d : 0.72d, () => semanticDecision, "no-semantic-event-choice", rawBounds: TryFindEventChoiceBounds(request), boundsSource: "observer-event");

        var explicitEventChoice = TryCreateEventProgressChoiceDecision(request);
        builder.Consider("click event choice", "observer:event-choice", preferEventForeground ? 0.94d : 0.68d, () => explicitEventChoice, "no-explicit-event-choice", rawBounds: TryFindEventChoiceBounds(request), boundsSource: "observer-event");

        if (LooksLikeMapTransitionState(request))
        {
            builder.Consider("click first reachable node", "branch:choose-first-node", 0.66d, () => DecideChooseFirstNode(request with { Phase = GuiSmokePhase.ChooseFirstNode.ToString() }), "no-map-transition-candidate", boundsSource: "branch-choose-first-node");
        }
        else
        {
            builder.AddSuppressed("click first reachable node", "event-foreground-keeps-map-transition-branch-suppressed");
        }

        var roomDecision = TryCreateScreenshotFirstRoomDecision(request);
        builder.Consider(ToCandidateLabel(roomDecision, "click room fallback"), "screenshot-room-fallback", 0.60d, () => roomDecision, "no-room-fallback-available", boundsSource: "screenshot-room");

        return builder.Build(CreateWaitDecision("waiting for an explicit event progression choice", request.Observer.CurrentScreen), actualDecision);
    }

    private static (string? ForegroundKind, string? BackgroundKind) DescribeForegroundBackground(GuiSmokeStepRequest request)
    {
        if (HasExplicitRestSiteChoiceAuthority(request))
        {
            return ("rest-site", "map");
        }

        if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(request.Observer))
        {
            return (RestSiteObserverSignals.HasSmithConfirmVisible(request.Observer) ? "rest-site-smith-confirm" : "rest-site-smith-grid", "rest-site");
        }

        if (CardSelectionObserverSignals.TryGetState(request.Observer) is { } cardSelectionState)
        {
            return ($"card-selection:{cardSelectionState.ScreenType}", request.Observer.CurrentScreen);
        }

        var eventScene = BuildEventSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath);
        if (eventScene.RewardSubstateActive)
        {
            return ("reward", eventScene.RewardScene.LayerState.MapContextVisible ? "map" : null);
        }

        if (eventScene.MapOverlayState.ForegroundVisible && !eventScene.StrongForegroundChoice)
        {
            return ("map-overlay", eventScene.MapOverlayState.EventBackgroundPresent ? "event" : "map");
        }

        if (eventScene.ExplicitAction == EventExplicitActionKind.AncientDialogue)
        {
            return ("ancient-event-dialogue", "event");
        }

        if (eventScene.ExplicitAction == EventExplicitActionKind.AncientCompletion)
        {
            return ("ancient-event-completion", "event");
        }

        if (eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.ReleasePending)
        {
            return ("event-release-pending", eventScene.MapContextVisible ? "map" : null);
        }

        if (eventScene.ExplicitAction == EventExplicitActionKind.AncientOption)
        {
            return ("ancient-event-options", "event");
        }

        if (eventScene.EventForegroundOwned)
        {
            return ("event", request.SceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase) ? "map" : null);
        }

        if (BuildShopSceneState(request.Observer, request.History) is { ReleaseStage: NonCombatReleaseStage.Active } shopScene)
        {
            return (shopScene.ForegroundDebugKind, shopScene.BackgroundDebugKind);
        }

        if (BuildTreasureSceneState(request.Observer) is { } treasureScene)
        {
            return (treasureScene.ForegroundDebugKind, treasureScene.BackgroundDebugKind);
        }

        if (LooksLikeRestSiteState(request.Observer))
        {
            return ("rest-site", request.SceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase) ? "map" : null);
        }

        if (string.Equals(request.Observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return ("combat", null);
        }

        if (string.Equals(request.Observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase))
        {
            return ("map", null);
        }

        return (request.Observer.CurrentScreen, request.Observer.VisibleScreen);
    }

    private static bool HasExplicitRestSiteChoiceAuthority(GuiSmokeStepRequest request)
    {
        return HasRestSiteAuthority(request.Observer)
               && HasExplicitRestSiteChoiceAffordance(request.Observer)
               && !RestSiteObserverSignals.IsRestSiteSmithUpgradeState(request.Observer)
               && !AutoRestSiteCardGridAnalyzer.Analyze(request.ScreenshotPath).HasSelectableCard;
    }

    private static bool HasRestSiteAuthority(ObserverSummary observer)
    {
        return string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase)
               || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer);
    }

    private static string[] BuildExplicitRestSiteCandidateLabels(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.BuildCandidateLabels(observer).ToArray();
    }

    private static bool HasExplicitRestSiteChoiceAffordance(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(observer);
    }

    private static bool HasRestSiteRestChoice(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.HasRestSiteRestChoice(observer);
    }

    private static bool HasRestSiteSmithChoice(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.HasRestSiteSmithChoice(observer);
    }

    private static bool HasRestSiteHatchChoice(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.HasRestSiteHatchChoice(observer);
    }

    private static bool IsExplicitRestSiteChoiceLabel(string? label)
    {
        return HasRestSiteRestLabel(label)
               || HasRestSiteSmithLabel(label)
               || HasRestSiteHatchLabel(label);
    }

    private static bool HasRestSiteRestLabel(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("휴식", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Rest", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasRestSiteSmithLabel(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("재련", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Smith", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasRestSiteHatchLabel(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("부화", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Hatch", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasStrongForegroundEventChoice(GuiSmokeStepRequest request)
    {
        var eventScene = BuildEventSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath);
        return eventScene.StrongForegroundChoice;
    }

    private static bool HasExplicitEventProceedAuthority(ObserverState observer, WindowBounds? windowBounds)
    {
        return EventProceedObserverSignals.HasExplicitEventProceedAuthority(observer, windowBounds);
    }

    private static bool HasExplicitEventProceedAuthority(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return EventProceedObserverSignals.HasExplicitEventProceedAuthority(observer, windowBounds);
    }

    private static bool HasEventChoiceAuthority(ObserverSummary observer)
    {
        return string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.VisibleScreen, "event", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "event", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "room-event", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExplicitEventProceedNode(ObserverActionNode node)
    {
        return node.Actionable
               && ((node.NodeId?.StartsWith("event-option:", StringComparison.OrdinalIgnoreCase) ?? false)
                   || node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase))
               && HasExplicitEventProceedSemantic(node.SemanticHints);
    }

    private static bool IsExplicitEventProceedChoice(ObserverChoice choice)
    {
        return !string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
               && HasExplicitEventProceedSemantic(choice.SemanticHints);
    }

    private static bool HasExplicitEventProceedSemantic(IReadOnlyList<string> semanticHints)
    {
        return semanticHints.Any(static hint =>
            string.Equals(hint, "option-role:proceed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(hint, "event-proceed", StringComparison.OrdinalIgnoreCase));
    }

    private static bool ShouldPrioritizeExplicitEventProgressionAfterCardSelection(ObserverState observer, IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return HasRecentCardSelectionSubtypeAftermath(history)
               && HasExplicitEventProgressionChoiceVisible(observer, null);
    }

    private static bool ShouldPrioritizeExplicitEventProgressionAfterCardSelection(GuiSmokeStepRequest request)
    {
        return HasRecentCardSelectionSubtypeAftermath(request.History)
               && HasExplicitEventProgressionChoiceVisible(request.Observer, request.WindowBounds);
    }

    private static bool HasRecentCardSelectionSubtypeAftermath(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        static bool IsSubtypeTarget(string? targetLabel)
        {
            return !string.IsNullOrWhiteSpace(targetLabel)
                   && (targetLabel.StartsWith("transform ", StringComparison.OrdinalIgnoreCase)
                       || targetLabel.StartsWith("deck remove ", StringComparison.OrdinalIgnoreCase)
                       || targetLabel.StartsWith("upgrade ", StringComparison.OrdinalIgnoreCase));
        }

        for (var index = history.Count - 1; index >= 0 && index >= history.Count - 6; index -= 1)
        {
            var entry = history[index];
            if (string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase)
                && IsSubtypeTarget(entry.TargetLabel))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasExplicitEventProgressionChoiceVisible(ObserverState observer, WindowBounds? windowBounds)
    {
        var eventScene = BuildEventSceneState(observer, windowBounds);
        return eventScene.EventForegroundOwned
               && eventScene.ReleaseStage == EventReleaseStage.Active
               && eventScene.HasExplicitProgression;
    }

    private static bool HasExplicitEventProgressionChoiceVisible(ObserverSummary observer, WindowBounds? windowBounds)
    {
        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, windowBounds);
        return eventScene.EventForegroundOwned
               && eventScene.ReleaseStage == EventReleaseStage.Active
               && eventScene.HasExplicitProgression;
    }

    internal static bool HasRawExplicitEventChoiceVisible(ObserverSummary observer, WindowBounds? windowBounds)
    {
        static bool IsGenericContinueLabel(string? label)
        {
            return !string.IsNullOrWhiteSpace(label)
                   && (label.Contains("계속", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("Continue", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("Proceed", StringComparison.OrdinalIgnoreCase));
        }

        return observer.ActionNodes.Any(node =>
                   node.Actionable
                   && HasActiveNodeBounds(node.ScreenBounds, windowBounds)
                   && !MapNodeSourceSupport.IsExplicitMapPointNode(node)
                   && !IsGenericContinueLabel(node.Label)
                   && !IsBackChoiceLabel(node.Label)
                   && (node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(node.TypeName, "event-option", StringComparison.OrdinalIgnoreCase)))
               || observer.Choices.Any(choice =>
                   HasActiveNodeBounds(choice.ScreenBounds, windowBounds)
                   && !MapNodeSourceSupport.IsExplicitMapPointChoice(choice)
                   && !IsGenericContinueLabel(choice.Label)
                   && !IsBackChoiceLabel(choice.Label)
                   && (string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(choice.BindingKind, "event-option", StringComparison.OrdinalIgnoreCase)));
    }

    internal static bool HasRawEventProgressionSurface(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return observer.ActionNodes.Any(node =>
                   node.Actionable
                   && HasActiveNodeBounds(node.ScreenBounds, windowBounds)
                   && !MapNodeSourceSupport.IsExplicitMapPointNode(node)
                   && ScoreProgressionNode(node) > 0)
               || observer.Choices.Any(choice =>
                   HasActiveNodeBounds(choice.ScreenBounds, windowBounds)
                   && !MapNodeSourceSupport.IsExplicitMapPointChoice(choice)
                   && ScoreProgressionChoice(choice) > 0);
    }

    private static bool MatchesRestSiteTarget(string? label, string targetLabel)
    {
        return RestSiteChoiceSupport.MapOptionIdToTargetLabel(RestSiteChoiceSupport.MapLabelToOptionId(label) ?? string.Empty)
            .Equals(targetLabel, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildRestSiteChoiceReason(string targetLabel, int currentHp, int maxHp)
    {
        return targetLabel switch
        {
            "rest site: rest" => $"Rest site detected from explicit choices. HP {currentHp}/{maxHp} favors healing before routing again.",
            "rest site: hatch" => "Rest site detected from explicit choices. Hatch is visible and no stronger rest/smith lane is available.",
            _ => "Rest site detected from explicit choices. HP is healthy enough to prefer smithing over generic map routing.",
        };
    }

    private static IReadOnlyList<RestSiteDecisionCandidate> BuildRestSiteDecisionCandidates(GuiSmokeStepRequest request)
    {
        var observedChoices = RestSiteChoiceSupport.GetObservedChoices(request.Observer);
        if (observedChoices.Count == 0)
        {
            return Array.Empty<RestSiteDecisionCandidate>();
        }

        var selectedOptionId = RestSiteChoiceSupport.DetermineAutoPickOptionId(request.Observer);
        return observedChoices
            .Select(choice => BuildRestSiteDecisionCandidate(request, choice, selectedOptionId))
            .ToArray();
    }

    private static RestSiteDecisionCandidate BuildRestSiteDecisionCandidate(
        GuiSmokeStepRequest request,
        RestSiteObservedChoice choice,
        string? selectedOptionId)
    {
        var targetLabel = choice.TargetLabel;
        var rawBounds = choice.HasMetadata
            ? choice.Choice?.ScreenBounds
            : choice.Choice?.ScreenBounds ?? choice.ActionNode?.ScreenBounds;
        var boundsSource = choice.HasMetadata
            ? !string.IsNullOrWhiteSpace(choice.Choice?.ScreenBounds) ? "observer-rest-site-choice" : null
            : !string.IsNullOrWhiteSpace(choice.Choice?.ScreenBounds)
                ? "observer-rest-site-choice-legacy"
                : !string.IsNullOrWhiteSpace(choice.ActionNode?.ScreenBounds)
                    ? "observer-rest-site-node-legacy"
                    : "fixed-rest-site-fallback";
        var rejectReason = "no-rest-site-decision";
        GuiSmokeStepDecision? decision = null;

        if (!choice.IsDefaultAutoPick)
        {
            rejectReason = "not-default-auto-pick";
        }
        else if (string.IsNullOrWhiteSpace(selectedOptionId)
                 || !string.Equals(choice.OptionId, selectedOptionId, StringComparison.OrdinalIgnoreCase))
        {
            rejectReason = "not-selected-by-rest-site-policy";
        }
        else if (choice.HasMetadata && !choice.IsEnabled)
        {
            rejectReason = "disabled-explicit-choice";
        }
        else
        {
            var repeatState = GetRestSiteExplicitChoiceRepeatState(request, targetLabel);
            if (repeatState == RestSiteExplicitChoiceRepeatState.NoOpDetected)
            {
                decision = CreateRestSitePostClickNoOpWaitDecision(request, targetLabel, request.Observer.CurrentScreen);
            }
            else if (repeatState == RestSiteExplicitChoiceRepeatState.GraceNeeded)
            {
                decision = CreateRestSiteGraceWaitDecision(targetLabel, request.Observer.CurrentScreen);
            }
            else if (choice.HasMetadata)
            {
                if (string.IsNullOrWhiteSpace(choice.Choice?.ScreenBounds))
                {
                    rejectReason = "missing-hitbox-for-explicit-choice";
                }
                else if (choice.Choice is null || !HasActiveNodeBounds(choice.Choice.ScreenBounds, request.WindowBounds))
                {
                    rejectReason = "stale-hitbox-for-explicit-choice";
                }
                else
                {
                    var currentHp = request.Observer.PlayerCurrentHp ?? 0;
                    var maxHp = request.Observer.PlayerMaxHp ?? 0;
                    decision = CreateClickDecisionFromChoice(
                        request,
                        choice.Choice,
                        targetLabel,
                        BuildRestSiteChoiceReason(targetLabel, currentHp, maxHp),
                        0.92,
                        request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                        1400);
                }
            }
            else
            {
                decision = TryCreateLegacyRestSiteDecision(request, choice);
                rejectReason = decision is null
                    ? "legacy-rest-site-choice-unavailable"
                    : "legacy-rest-site-choice-selected";
            }
        }

        return new RestSiteDecisionCandidate(
            choice.CandidateLabel,
            choice.HasMetadata ? "observer-rest-site-choice" : "legacy-rest-site-choice",
            GetRestSiteCandidateScore(choice.OptionId),
            rejectReason,
            rawBounds,
            boundsSource,
            decision);
    }

    private static GuiSmokeStepDecision? TryCreateLegacyRestSiteDecision(GuiSmokeStepRequest request, RestSiteObservedChoice choice)
    {
        var currentHp = request.Observer.PlayerCurrentHp ?? 0;
        var maxHp = request.Observer.PlayerMaxHp ?? 0;
        if (choice.Choice is not null
            && HasActiveNodeBounds(choice.Choice.ScreenBounds, request.WindowBounds))
        {
            return CreateClickDecisionFromChoice(
                request,
                choice.Choice,
                choice.TargetLabel,
                BuildRestSiteChoiceReason(choice.TargetLabel, currentHp, maxHp),
                0.92,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                1400);
        }

        if (choice.ActionNode is not null
            && choice.ActionNode.Actionable
            && HasActiveNodeBounds(choice.ActionNode.ScreenBounds, request.WindowBounds))
        {
            return CreateClickDecisionFromNode(request, choice.ActionNode, choice.TargetLabel);
        }

        var normalizedX = choice.TargetLabel switch
        {
            "rest site: rest" => 0.405,
            "rest site: smith" => 0.575,
            "rest site: hatch" => 0.745,
            _ => 0.575,
        };
        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            0.305,
            choice.TargetLabel,
            BuildRestSiteChoiceReason(choice.TargetLabel, currentHp, maxHp),
            0.86,
            "map",
            1500,
            true,
            null);
    }

    private static double GetRestSiteCandidateScore(string optionId)
    {
        return RestSiteChoiceSupport.NormalizeOptionId(optionId) switch
        {
            "HEAL" => 1.00d,
            "SMITH" => 0.98d,
            "HATCH" => 0.94d,
            _ => 0.72d,
        };
    }

    private static GuiSmokeStepDecision CreateRestSiteGraceWaitDecision(string targetLabel, string? expectedScreen)
    {
        return new GuiSmokeStepDecision(
            "wait",
            null,
            null,
            null,
            null,
            targetLabel,
            $"Rest-site explicit choice '{targetLabel}' persisted after a click. Allow one recapture before escalating to a no-op diagnosis.",
            0.70d,
            expectedScreen,
            700,
            true,
            null,
            DecisionRisk: "rest-site-post-click-recapture-grace");
    }

    private static GuiSmokeStepDecision CreateRestSitePostClickNoOpWaitDecision(GuiSmokeStepRequest request, string targetLabel, string? expectedScreen)
    {
        var evidence = RestSiteObserverSignals.BuildPostClickEvidence(request.Observer, targetLabel);
        var reason = evidence.Classification switch
        {
            "rest-site-selection-failed" => $"Rest-site explicit choice '{targetLabel}' reported an after-select failure in observer metadata. Stop repeating the click and surface the failed selection directly.",
            "rest-site-grid-not-visible-after-selection" => $"Rest-site explicit choice '{targetLabel}' was accepted for selection, but no smith grid or confirm state became observer-visible after grace recapture.",
            "rest-site-grid-observer-miss" => $"Rest-site smith upgrade screen became runtime-visible, but the observer did not export card or confirm hitboxes. Stop repeating the smith click and surface the observer miss.",
            _ => $"Rest-site explicit choice '{targetLabel}' remained on the same fingerprint after grace recapture. Stop repeating the click and escalate to rest-site-post-click-noop.",
        };
        return new GuiSmokeStepDecision(
            "wait",
            null,
            null,
            null,
            null,
            targetLabel,
            reason,
            0.72d,
            expectedScreen,
            250,
            true,
            null,
            DecisionRisk: evidence.Classification);
    }

    private static RestSiteExplicitChoiceRepeatState GetRestSiteExplicitChoiceRepeatState(GuiSmokeStepRequest request, string targetLabel)
    {
        if (!HasExplicitRestSiteChoiceAuthority(request)
            || !IsExplicitRestSiteOptionTarget(targetLabel))
        {
            return RestSiteExplicitChoiceRepeatState.None;
        }

        var fingerprint = RestSiteChoiceSupport.BuildExplicitChoiceFingerprint(request.Observer);
        for (var index = request.History.Count - 1; index >= 0; index -= 1)
        {
            var entry = request.History[index];
            if (TryParseRestSiteActionMetadata(entry.Metadata, out var metadata))
            {
                if (!string.Equals(metadata.TargetLabel, targetLabel, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(metadata.Fingerprint, fingerprint, StringComparison.Ordinal))
                {
                    return RestSiteExplicitChoiceRepeatState.None;
                }

                return string.Equals(metadata.Kind, "explicit-grace-wait", StringComparison.OrdinalIgnoreCase)
                    ? RestSiteExplicitChoiceRepeatState.NoOpDetected
                    : string.Equals(metadata.Kind, "explicit-click", StringComparison.OrdinalIgnoreCase)
                        ? RestSiteExplicitChoiceRepeatState.GraceNeeded
                        : RestSiteExplicitChoiceRepeatState.None;
            }

            if (entry.Action is "click" or "click-current" or "confirm-non-enemy" or "right-click" or "press-key")
            {
                return RestSiteExplicitChoiceRepeatState.None;
            }
        }

        return RestSiteExplicitChoiceRepeatState.None;
    }

    private static bool HasRecentRestSiteExplicitClick(GuiSmokeStepRequest request, string? targetLabel)
    {
        if (!IsExplicitRestSiteOptionTarget(targetLabel))
        {
            return false;
        }

        for (var index = request.History.Count - 1; index >= 0; index -= 1)
        {
            var entry = request.History[index];
            if (!TryParseRestSiteActionMetadata(entry.Metadata, out var metadata))
            {
                continue;
            }

            return string.Equals(metadata.Kind, "explicit-click", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(metadata.TargetLabel, targetLabel, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool IsExplicitRestSiteOptionTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "rest site: rest", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: smith", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: hatch", StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(targetLabel)
                   && targetLabel.StartsWith("rest site: option:", StringComparison.OrdinalIgnoreCase));
    }

    private static string? BuildRestSiteHistoryMetadata(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        if (!HasExplicitRestSiteChoiceAuthority(request)
            || !IsExplicitRestSiteOptionTarget(decision.TargetLabel))
        {
            return null;
        }

        var kind = string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(decision.ActionKind, "click", StringComparison.OrdinalIgnoreCase)
            ? "explicit-click"
            : string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase)
              && string.Equals(decision.DecisionRisk, "rest-site-post-click-recapture-grace", StringComparison.OrdinalIgnoreCase)
                ? "explicit-grace-wait"
                : null;
        if (kind is null)
        {
            return null;
        }

        return JsonSerializer.Serialize(
            new RestSiteActionMetadata(
                kind,
                decision.TargetLabel!,
                RestSiteChoiceSupport.BuildExplicitChoiceFingerprint(request.Observer)),
            GuiSmokeShared.JsonOptions);
    }

    private static bool TryParseRestSiteActionMetadata(string? metadata, out RestSiteActionMetadata parsed)
    {
        parsed = default!;
        if (string.IsNullOrWhiteSpace(metadata))
        {
            return false;
        }

        try
        {
            parsed = JsonSerializer.Deserialize<RestSiteActionMetadata>(metadata, GuiSmokeShared.JsonOptions)!;
            return parsed is not null
                   && !string.IsNullOrWhiteSpace(parsed.Kind)
                   && !string.IsNullOrWhiteSpace(parsed.TargetLabel)
                   && !string.IsNullOrWhiteSpace(parsed.Fingerprint);
        }
        catch
        {
            return false;
        }
    }

    public static string? BuildRestSiteHistoryMetadataForDecision(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        return BuildRestSiteHistoryMetadata(request, decision);
    }

    public static bool HasRecentRestSiteExplicitClickForRequest(GuiSmokeStepRequest request, string? targetLabel)
    {
        return HasRecentRestSiteExplicitClick(request, targetLabel);
    }

    public static bool HasExplicitRestSiteChoiceAuthorityForRequest(GuiSmokeStepRequest request)
    {
        return HasExplicitRestSiteChoiceAuthority(request);
    }

    public static bool IsExplicitRestSiteOptionTargetLabel(string? targetLabel)
    {
        return IsExplicitRestSiteOptionTarget(targetLabel);
    }

    private static string ToCandidateLabel(GuiSmokeStepDecision? decision, string fallback)
    {
        return decision?.TargetLabel switch
        {
            { } target when target.Contains("reward", StringComparison.OrdinalIgnoreCase) => "click reward choice",
            { } target when target.Contains("ancient dialogue", StringComparison.OrdinalIgnoreCase) => "click ancient dialogue advance",
            { } target when target.Contains("ancient event completion", StringComparison.OrdinalIgnoreCase) => "click ancient event completion",
            { } target when target.Contains("event", StringComparison.OrdinalIgnoreCase) => "click event choice",
            { } target when target.Contains("rest site:", StringComparison.OrdinalIgnoreCase) => "click rest site choice",
            { } target when target.Contains("smith card", StringComparison.OrdinalIgnoreCase) => "click smith card",
            { } target when target.Contains("smith confirm", StringComparison.OrdinalIgnoreCase) => "click smith confirm",
            { } target when target.Contains("map back", StringComparison.OrdinalIgnoreCase) => "click map back",
            { } target when target.Contains("exported reachable map node", StringComparison.OrdinalIgnoreCase) => "click exported reachable node",
            { } target when target.Contains("visible map advance", StringComparison.OrdinalIgnoreCase) => "click visible map advance",
            _ => fallback,
        };
    }

    private static bool AllowsAction(GuiSmokeStepRequest request, string action)
    {
        return request.AllowedActions.Contains(action, StringComparer.OrdinalIgnoreCase);
    }

    private static string? TryFindMapNodeBounds(GuiSmokeStepRequest request)
    {
        return request.Observer.Choices.FirstOrDefault(choice =>
                   MapNodeSourceSupport.IsExplicitMapPointChoice(choice)
                   && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds))?.ScreenBounds
               ?? request.Observer.ActionNodes.FirstOrDefault(node =>
                   node.Actionable
                   && MapNodeSourceSupport.IsExplicitMapPointNode(node)
                   && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds))?.ScreenBounds;
    }

    private static string? TryFindBackBounds(GuiSmokeStepRequest request)
    {
        return request.Observer.ActionNodes.FirstOrDefault(node =>
                   node.Actionable
                   && IsBackNode(node)
                   && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds))?.ScreenBounds
               ?? request.Observer.Choices.FirstOrDefault(choice =>
                   IsBackChoiceLabel(choice.Label)
                   && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds))?.ScreenBounds;
    }

    private static string? TryFindEventChoiceBounds(GuiSmokeStepRequest request)
    {
        return AncientEventObserverSignals.GetActiveDialogueNode(request.Observer, request.WindowBounds)?.ScreenBounds
               ?? AncientEventObserverSignals.GetActiveDialogueChoice(request.Observer, request.WindowBounds)?.ScreenBounds
               ?? AncientEventObserverSignals.GetActiveCompletionNode(request.Observer, request.WindowBounds)?.ScreenBounds
               ?? AncientEventObserverSignals.GetActiveCompletionChoice(request.Observer, request.WindowBounds)?.ScreenBounds
               ?? AncientEventObserverSignals.GetActiveOptionNodes(request.Observer, request.WindowBounds).FirstOrDefault()?.ScreenBounds
               ?? AncientEventObserverSignals.GetActiveOptionChoices(request.Observer, request.WindowBounds).FirstOrDefault()?.ScreenBounds
               ?? request.Observer.ActionNodes.FirstOrDefault(node =>
                   node.Actionable
                   && node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
                   && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds))?.ScreenBounds
               ?? request.Observer.Choices.FirstOrDefault(choice =>
                   string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
                   && !IsBackChoiceLabel(choice.Label)
                   && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds))?.ScreenBounds;
    }

    private static string? TryFindRewardBounds(GuiSmokeStepRequest request)
    {
        return request.Observer.ActionNodes.FirstOrDefault(node =>
                   node.Actionable
                   && IsExplicitRewardProgressionNode(node)
                   && HasActiveRewardBounds(node.ScreenBounds, request.WindowBounds))?.ScreenBounds
               ?? request.Observer.Choices.FirstOrDefault(choice =>
                   IsExplicitRewardProgressionChoice(choice)
                   && HasActiveRewardBounds(choice.ScreenBounds, request.WindowBounds))?.ScreenBounds;
    }

    private static bool HasRecentCombatCardSelection(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return CombatHistorySupport.TryGetPendingCombatSelection(history) is not null;
    }

    private static bool IsCompatibleScreenshotCombatSlot(
        AutoCombatHandSlotAnalysis slot,
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        bool expectEnemyTarget)
    {
        var observerCard = observer.CombatHand.FirstOrDefault(card => card.SlotIndex == slot.SlotIndex);
        if (observerCard is not null)
        {
            return expectEnemyTarget
                ? IsAttackCombatHandCard(observerCard) && IsPlayableAtCurrentEnergy(observerCard, observer.PlayerEnergy, combatCardKnowledge)
                : CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatHandCard(observerCard, observer.PlayerEnergy, combatCardKnowledge);
        }

        var knowledgeCard = combatCardKnowledge.FirstOrDefault(card => card.SlotIndex == slot.SlotIndex);
        if (knowledgeCard is not null)
        {
            return expectEnemyTarget
                ? IsEnemyTargetCombatCard(knowledgeCard) && IsPlayableAtCurrentEnergy(knowledgeCard, observer.PlayerEnergy)
                : CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatCard(knowledgeCard, observer.PlayerEnergy);
        }

        return observer.CombatHand.Count == 0 && combatCardKnowledge.Count == 0;
    }

    private static bool TryCreateCombatEnemyTargetDecision(
        GuiSmokeStepRequest request,
        PendingCombatSelection? pendingSelection,
        int pendingSelectionNoOpCount,
        IReadOnlyList<int> alternatePlayableAttackSlots,
        out GuiSmokeStepDecision decision)
    {
        if (pendingSelection?.Kind != AutoCombatCardKind.AttackLike)
        {
            decision = default!;
            return false;
        }

        if (pendingSelectionNoOpCount >= 2)
        {
            if (alternatePlayableAttackSlots.Count > 0)
            {
                decision = new GuiSmokeStepDecision(
                    "act",
                    "press-key",
                    GetCombatSlotHotkey(alternatePlayableAttackSlots[0]),
                    null,
                    null,
                    $"combat select attack slot {alternatePlayableAttackSlots[0]}",
                    $"Recent combat history shows no board delta after targeting from slot {pendingSelection.SlotIndex}. Switch to another playable attack lane before trying to target the enemy again.",
                    0.91,
                    "combat",
                    120,
                    true,
                    null);
                return true;
            }

            decision = default!;
            return false;
        }

        if (TryCreateCombatEnemyTargetDecisionFromObservedNodes(request, pendingSelection, pendingSelectionNoOpCount, out decision))
        {
            return true;
        }

        if (CombatTargetabilitySupport.HasExplicitCombatEnemyTargetNodeSource(request.Observer)
            || CombatTargetabilitySupport.HasExplicitTargetableEnemyAuthority(request.Observer))
        {
            decision = CreatePhaseWaitDecision(
                GuiSmokePhase.HandleCombat,
                CombatTargetabilitySupport.DescribeMissingCombatEnemyTargetDecisionSource(request.Observer, request.WindowBounds),
                "combat");
            return true;
        }

        var targetCandidateIndex = Math.Clamp(pendingSelectionNoOpCount, 0, GuiSmokeCombatConstants.EnemyTargetCandidates.Length - 1);
        var targetCandidate = GuiSmokeCombatConstants.EnemyTargetCandidates[targetCandidateIndex];
        var reason = pendingSelectionNoOpCount == 0
            ? "An attack card is selected. Click the enemy body to resolve it."
            : $"The previous enemy click from slot {pendingSelection.SlotIndex} produced no board delta. Recenter the target click before abandoning the lane.";
        decision = new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            targetCandidate.X,
            targetCandidate.Y,
            targetCandidate.Label,
            reason,
            pendingSelectionNoOpCount == 0 ? 0.90 : 0.86,
            "combat",
            300,
            true,
            null);
        return true;
    }

    private static bool TryCreateCombatEnemyTargetDecisionFromObservedNodes(
        GuiSmokeStepRequest request,
        PendingCombatSelection pendingSelection,
        int pendingSelectionNoOpCount,
        out GuiSmokeStepDecision decision)
    {
        var targetNodes = GetCombatEnemyTargetNodes(request.Observer, request.WindowBounds);
        if (targetNodes.Count == 0)
        {
            decision = default!;
            return false;
        }

        var targetNode = pendingSelectionNoOpCount == 0
            ? targetNodes[0]
            : targetNodes[Math.Clamp(pendingSelectionNoOpCount, 0, targetNodes.Count - 1)];
        var targetLabel = BuildCombatEnemyTargetLabel(targetNode, pendingSelectionNoOpCount);
        decision = CreateCombatEnemyTargetDecisionFromNode(request, targetNode, targetLabel, pendingSelectionNoOpCount);
        return true;
    }

    private static bool CanResolveEnemyTargetFromCurrentState(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        AutoCombatAnalysis analysis,
        PendingCombatSelection? pendingSelection)
    {
        var runtime = CombatRuntimeStateSupport.Read(observer, combatCardKnowledge);
        if (CombatRuntimeStateSupport.CanResolveEnemyTarget(observer, combatCardKnowledge, pendingSelection, analysis))
        {
            return true;
        }

        if (runtime.HasExplicitHittableEnemyAuthority)
        {
            return false;
        }

        if (CombatTargetabilitySupport.GetCombatEnemyTargetNodes(observer).Count > 0)
        {
            return true;
        }

        if (analysis.HasTargetArrow)
        {
            return true;
        }

        if (CombatRuntimeStateSupport.RequiresExplicitTargetingBeforeEnemyClick(observer, combatCardKnowledge))
        {
            return false;
        }

        if (pendingSelection?.Kind == AutoCombatCardKind.AttackLike)
        {
            var pendingCard = observer.CombatHand.FirstOrDefault(card => card.SlotIndex == pendingSelection.SlotIndex);
            if (pendingCard is not null)
            {
                return IsAttackCombatHandCard(pendingCard)
                       && IsPlayableAtCurrentEnergy(pendingCard, observer.PlayerEnergy, combatCardKnowledge);
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
               && (observer.CombatHand.Any(card =>
                       card.SlotIndex is >= 1 and <= 5
                       && IsAttackCombatHandCard(card)
                       && IsPlayableAtCurrentEnergy(card, observer.PlayerEnergy, combatCardKnowledge))
                   || (observer.CombatHand.Count == 0 && combatCardKnowledge.Count == 0));
    }

    public static CombatNoOpLoopAnalysis PeekCombatNoOpLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return AnalyzeCombatNoOpLoop(history);
    }

    public static PendingCombatSelection? TryPeekPendingCombatSelection(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return CombatHistorySupport.TryGetPendingCombatSelection(history);
    }

    public static bool IsCombatNoOpSensitiveTarget(string? targetLabel)
    {
        return targetLabel is not null
               && (targetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(targetLabel, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase));
    }

    public static string? ResolveCombatLaneLabel(string? targetLabel, IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return CombatHistorySupport.ResolveCombatLaneLabel(targetLabel, history);
    }

    private static int? ExtractCombatLaneSlotIndex(string? targetLabel)
    {
        return CombatHistorySupport.ExtractCombatLaneSlotIndex(targetLabel);
    }

    public static IReadOnlyDictionary<int, int> GetCombatNoOpCountsBySlot(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return CombatHistorySupport.GetCombatNoOpCountsBySlot(history);
    }

    private static int GetCombatNoOpCountForSlot(IReadOnlyList<GuiSmokeHistoryEntry> history, int slotIndex)
    {
        return HandleCombatContextSupport.GetCombatNoOpCountForSlot(HandleCombatContextSupport.Reconstruct(history), slotIndex);
    }

    private static bool HasRecentRepeatedNonEnemyLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return HandleCombatContextSupport.Reconstruct(history).RepeatedNonEnemyLoop;
    }

    private static bool HasRecentRepeatedAttackSelectionLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return HandleCombatContextSupport.Reconstruct(history).RepeatedAttackSelectionLoop;
    }

    private static CombatNoOpLoopAnalysis AnalyzeCombatNoOpLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return HandleCombatContextSupport.Reconstruct(history).CombatNoOpLoop;
    }

    private static bool IsNonEnemySelectionLabel(string? targetLabel)
    {
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return false;
        }

        return targetLabel.StartsWith("combat select non-enemy slot ", StringComparison.OrdinalIgnoreCase)
               || targetLabel.StartsWith("combat select defend slot ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParsePendingCombatSelection(string targetLabel, out PendingCombatSelection? selection)
    {
        return CombatHistorySupport.TryParsePendingCombatSelection(targetLabel, out selection);
    }

    private static int? ExtractFirstDigit(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        foreach (var character in value)
        {
            if (char.IsDigit(character))
            {
                return character - '0';
            }
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryFindActionNodeDecision(GuiSmokeStepRequest request, string contains, string targetLabel)
    {
        var node = request.Observer.ActionNodes.FirstOrDefault(candidate =>
            candidate.Actionable &&
            candidate.Label.Contains(contains, StringComparison.OrdinalIgnoreCase));
        return node is null ? null : CreateClickDecisionFromNode(request, node, targetLabel);
    }

    private static GuiSmokeStepDecision? TryFindFirstReachableMapNodeDecision(GuiSmokeStepRequest request)
    {
        if (HasContradictoryForegroundOwnerAgainstMapFallback(request)
            || !GuiSmokeNonCombatContractSupport.AllowsAction(request, "click first reachable node"))
        {
            return null;
        }

        var analysis = AutoMapAnalyzer.Analyze(request.ScreenshotPath);
        if (analysis.HasReachableNode)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                analysis.ReachableNodeNormalizedX,
                analysis.ReachableNodeNormalizedY,
                "visible reachable node",
                "The screenshot shows the current map arrow and a connected reachable node. Click the reachable node directly.",
                0.90,
                "map",
                1500,
                true,
                null);
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateExportedReachableMapPointDecision(GuiSmokeStepRequest request)
    {
        if (HasContradictoryForegroundOwnerAgainstMapFallback(request)
            || !GuiSmokeNonCombatContractSupport.AllowsAction(request, "click exported reachable node"))
        {
            return null;
        }

        var choice = request.Observer.Choices
            .Where(choice =>
                MapNodeSourceSupport.IsExplicitMapPointChoice(choice)
                && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds))
            .OrderBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (choice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                choice,
                "exported reachable map node",
                $"Use the exported reachable map point '{choice.Label}' instead of clicking the red current-node arrow.",
                0.95,
                "map",
                1400);
        }

        var node = request.Observer.ActionNodes
            .Where(node =>
                node.Actionable
                && MapNodeSourceSupport.IsExplicitMapPointNode(node)
                && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds))
            .OrderBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        return node is null ? null : CreateClickDecisionFromNode(request, node, "exported reachable map node");
    }

    private static GuiSmokeStepDecision? TryCreateMapBackNavigationDecision(GuiSmokeStepRequest request)
    {
        if (HasContradictoryForegroundOwnerAgainstMapFallback(request)
            || !GuiSmokeNonCombatContractSupport.AllowsAction(request, "click map back"))
        {
            return null;
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (!mapOverlayState.ForegroundVisible || !mapOverlayState.MapBackNavigationAvailable)
        {
            return null;
        }

        var backNode = request.Observer.ActionNodes.FirstOrDefault(node => node.Actionable && IsBackNode(node) && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds));
        if (backNode is not null)
        {
            return CreateClickDecisionFromNode(request, backNode, "map back");
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            0.045,
            0.905,
            "map back",
            "Map overlay foreground is visible and the bottom-left red back arrow can return to the underlying event context.",
            0.86,
            "map",
            1200,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateMapOverlayForegroundDecision(GuiSmokeStepRequest request)
    {
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (!mapOverlayState.ForegroundVisible)
        {
            return null;
        }

        return TryCreateExportedReachableMapPointDecision(request)
               ?? TryCreateMapBackNavigationDecision(request)
               ?? TryFindFirstReachableMapNodeDecision(request);
    }

    private static string GetCombatSlotHotkey(int slotIndex)
    {
        return slotIndex == 10
            ? "0"
            : slotIndex.ToString(CultureInfo.InvariantCulture);
    }

    private static ObserverActionNode? FindEndTurnActionNode(GuiSmokeStepRequest request)
    {
        return FindEndTurnActionNode(request.Observer, request.WindowBounds);
    }

    private static ObserverActionNode? FindEndTurnActionNode(ObserverSummary observer, WindowBounds? windowBounds = null)
    {
        return observer.ActionNodes.FirstOrDefault(node =>
            node.Actionable
            && (string.Equals(node.Label, "1턴 종료", StringComparison.OrdinalIgnoreCase)
                || node.Label.Contains("턴 종료", StringComparison.OrdinalIgnoreCase)
                || node.Label.Contains("End Turn", StringComparison.OrdinalIgnoreCase))
            && (windowBounds is null
                ? TryParseNodeBounds(node.ScreenBounds, out _)
                : HasActiveNodeBounds(node.ScreenBounds, windowBounds)));
    }

    private static IReadOnlyList<ObserverActionNode> GetCombatEnemyTargetNodes(ObserverSummary observer, WindowBounds? windowBounds = null)
    {
        return CombatTargetabilitySupport.GetCombatEnemyTargetNodes(observer, windowBounds)
            .Where(node => windowBounds is null
                ? TryParseNodeBounds(node.ScreenBounds, out _)
                : HasActiveNodeBounds(node.ScreenBounds, windowBounds))
            .OrderBy(static node => GetNodeSortX(node))
            .ThenBy(static node => GetNodeSortY(node))
            .ToArray();
    }

    private static bool IsCombatEnemyTargetNode(ObserverActionNode node)
    {
        return string.Equals(node.Kind, "enemy-target", StringComparison.OrdinalIgnoreCase)
               || node.NodeId.StartsWith("enemy-target:", StringComparison.OrdinalIgnoreCase)
               || string.Equals(node.TypeName, "enemy-target", StringComparison.OrdinalIgnoreCase)
               || node.SemanticHints.Any(static hint =>
                   string.Equals(hint, "combat-targetable", StringComparison.OrdinalIgnoreCase)
                   || hint.StartsWith("target-id:", StringComparison.OrdinalIgnoreCase));
    }

    private static GuiSmokeStepDecision? TryCreateSemanticEventDecision(GuiSmokeStepRequest request)
    {
        if (!string.Equals(request.ReasoningMode, "semantic", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var preferredLabels = request.EventKnowledgeCandidates
            .SelectMany(static candidate => candidate.Options)
            .OrderByDescending(static option => ScoreSemanticEventOption(option.Label, option.Description))
            .Select(static option => option.Label)
            .ToArray();
        foreach (var label in preferredLabels)
        {
            var choice = request.Observer.Choices.FirstOrDefault(candidate =>
                string.Equals(candidate.Label, label, StringComparison.OrdinalIgnoreCase)
                && TryParseNodeBounds(candidate.ScreenBounds, out _));
            if (choice is null || !TryParseNodeBounds(choice.ScreenBounds, out var bounds))
            {
                continue;
            }

            var centerX = bounds.X + bounds.Width / 2f;
            var centerY = bounds.Y + bounds.Height / 2f;
            var leadingCandidate = request.EventKnowledgeCandidates.FirstOrDefault(candidate =>
                candidate.Options.Any(option => string.Equals(option.Label, label, StringComparison.OrdinalIgnoreCase)));
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                Math.Clamp(centerX / 1920f, 0d, 1d),
                Math.Clamp(centerY / 1080f, 0d, 1d),
                $"semantic event option: {label}",
                $"Semantic event reasoning selected '{label}' from {leadingCandidate?.Title ?? "event knowledge"}.",
                0.86,
                "event",
                1400,
                true,
                null,
                leadingCandidate is null ? "semantic event match" : $"event candidate: {leadingCandidate.Title}",
                "event option changes page, grants reward, or advances room flow",
                request.DecisionRiskHint);
        }

        return null;
    }

    private static int ScoreSemanticEventOption(string? label, string? description)
    {
        var score = 0;
        if (ContainsAny(label, "떠", "leave", "continue", "확인", "proceed", "take"))
        {
            score += 5;
        }

        if (ContainsAny(description, "획득", "gain", "얻", "heal", "회복", "gold", "카드"))
        {
            score += 3;
        }

        if (ContainsAny(description, "잃", "lose", "피해", "damage", "hp", "wound", "부상"))
        {
            score -= 4;
        }

        return score;
    }

    private static bool IsAttackCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("STRIKE", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("BASH", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEnemyTargetCombatCard(CombatCardKnowledgeHint card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Skill", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Type, "Power", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("DEFEND", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInertCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Status", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Type, "Curse", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyCombatCard(CombatCardKnowledgeHint card)
    {
        if (string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
            || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Equals(card.Type, "Power", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "Self", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "None", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllAllies", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyAlly", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlayableAtCurrentEnergy(ObservedCombatHandCard card, int? energy)
    {
        if (IsInertCombatHandCard(card) && card.Cost is null)
        {
            return false;
        }

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

    private static bool IsPlayableAtCurrentEnergy(
        ObservedCombatHandCard card,
        int? energy,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var resolvedCost = ResolveCombatCardCost(card, combatCardKnowledge);
        return IsPlayableAtCurrentEnergy(card with { Cost = resolvedCost }, energy);
    }

    private static bool IsPlayableAtCurrentEnergy(CombatCardKnowledgeHint card, int? energy)
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

    private static int? ResolveCombatCardCost(ObservedCombatHandCard card, IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
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

        var cardKeys = BuildCombatKnowledgeLookupKeysForCombat(card.Name);
        if (cardKeys.Count == 0)
        {
            return null;
        }

        return combatCardKnowledge
            .Where(candidate => candidate.Cost is not null)
            .Where(candidate =>
            {
                var candidateKeys = BuildCombatKnowledgeLookupKeysForCombat(candidate.Name);
                return candidateKeys.Any(cardKeys.Contains);
            })
            .Select(static candidate => candidate.Cost)
            .FirstOrDefault();
    }

    private static IReadOnlyList<string> BuildCombatKnowledgeLookupKeysForCombat(string? cardName)
    {
        if (string.IsNullOrWhiteSpace(cardName))
        {
            return Array.Empty<string>();
        }

        var keys = new List<string>();
        var normalizedName = NormalizeCombatLookupKey(cardName);
        AddCombatLookupKey(keys, normalizedName);

        var parts = cardName
            .Split(new[] { '.', '_', '-', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeCombatLookupKey)
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

        AddCombatLookupKey(keys, string.Concat(trimmedParts));
        AddCombatLookupKey(keys, trimmedParts[0]);
        if (trimmedParts.Length > 1 && IsCombatLookupClassSuffix(trimmedParts[^1]))
        {
            AddCombatLookupKey(keys, string.Concat(trimmedParts[..^1]));
        }

        foreach (var part in trimmedParts)
        {
            AddCombatLookupKey(keys, part);
        }

        return keys;
    }

    private static void AddCombatLookupKey(List<string> keys, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate)
            || keys.Contains(candidate, StringComparer.Ordinal))
        {
            return;
        }

        keys.Add(candidate);
    }

    private static bool IsCombatLookupClassSuffix(string value)
    {
        return value is "ironclad" or "silent" or "defect" or "watcher" or "colorless" or "status" or "curse";
    }

    private static string NormalizeCombatLookupKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var buffer = new char[value.Length];
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
            : new string(buffer, 0, length);
    }

    private static GuiSmokeStepDecision? TryCreateScreenshotFirstRoomDecision(GuiSmokeStepRequest request)
    {
        var rewardMapLayer = BuildRewardMapLayerState(request.Observer, request.WindowBounds);
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (!rewardMapLayer.RewardPanelVisible
            && !GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(request.Observer)
            && !mapOverlayState.ForegroundVisible)
        {
            var visibleMapNodeDecision = GuiSmokeDecisionDebug.TraceCandidate(
                "visible reachable node",
                "screenshot-reachable-node",
                0.90,
                TryFindFirstReachableMapNodeDecision(request),
                "no screenshot-reachable map node was detected");
            if (visibleMapNodeDecision is not null)
            {
                return visibleMapNodeDecision;
            }

            var visibleMapAdvanceDecision = GuiSmokeDecisionDebug.TraceCandidate(
                "visible map advance",
                "screenshot-current-arrow",
                0.78,
                TryFindVisibleMapAdvanceDecision(request),
                "no current-node-arrow fallback was permitted");
            if (visibleMapAdvanceDecision is not null)
            {
                return visibleMapAdvanceDecision;
            }
        }

        if (LooksLikeRestSiteState(request.Observer))
        {
            GuiSmokeDecisionDebug.SetSceneModel("rest-site", "map-context");
        }

        var restSiteUpgradeDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "rest site upgrade",
            "rest-site-upgrade",
            0.91,
            TryCreateRestSiteUpgradeDecision(request),
            "rest-site upgrade grid is not currently actionable");
        if (restSiteUpgradeDecision is not null)
        {
            return restSiteUpgradeDecision;
        }

        var treasureDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "treasure room",
            "treasure-room",
            0.90,
            TryCreateTreasureRoomDecision(request),
            "treasure room affordance is not visible");
        if (treasureDecision is not null)
        {
            return treasureDecision;
        }

        if (LooksLikeShopState(request.Observer))
        {
            var shopDecision = DecideHandleShop(request);
            return string.Equals(shopDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                ? null
                : shopDecision;
        }

        return GuiSmokeDecisionDebug.TraceCandidate(
                   "rest site explicit choice",
                   "rest-site-choice",
                   0.89,
                   TryCreateRestSiteDecision(request),
                   "rest-site explicit choices are not visible")
               ?? GuiSmokeDecisionDebug.TraceCandidate(
                   "hidden overlay cleanup",
                   "overlay-cleanup",
                   0.70,
                   TryCreateHiddenOverlayCleanupDecision(request),
                   "no hidden overlay cleanup affordance is available")
               ?? GuiSmokeDecisionDebug.TraceCandidate(
                   "overlay advance",
                   "overlay-advance",
                   0.68,
                   TryCreateOverlayAdvanceDecision(request),
                   "no overlay advance affordance is available")
               ?? GuiSmokeDecisionDebug.TraceCandidate(
                   "visible proceed",
                   "visible-proceed",
                   0.67,
                   TryCreateVisibleProceedDecision(request),
                   "no visible proceed button is available");
    }

    private static GuiSmokeStepDecision? TryCreateTreasureRoomDecision(GuiSmokeStepRequest request)
    {
        var treasureState = TreasureRoomObserverSignals.TryGetState(request.Observer);
        if (treasureState is null || !treasureState.RoomDetected)
        {
            return null;
        }

        if (treasureState.InspectOverlayVisible)
        {
            var overlayBackChoice = TreasureRoomObserverSignals.TryGetOverlayBackChoice(request.Observer);
            if (overlayBackChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    overlayBackChoice,
                    "treasure overlay back",
                    "Treasure inspect overlay is foreground-visible. Close it before retrying any chest, relic-holder, or proceed action.",
                    0.98,
                    request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                    1200);
            }

            return null;
        }

        if (!treasureState.ChestOpened && treasureState.ChestClickable)
        {
            var chestChoice = TreasureRoomObserverSignals.TryGetChestChoice(request.Observer);
            if (chestChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    chestChoice,
                    "treasure chest",
                    "Treasure room authority is active. Open the chest before any relic holder or proceed action.",
                    0.98,
                    request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                    1500);
            }

            var treasureAnalysis = AutoTreasureAnalyzer.Analyze(request.ScreenshotPath);
            if (treasureAnalysis.HasClosedChestHighlight)
            {
                return CreateTreasureChestCenterDecision(request);
            }
        }

        if (treasureState.ProceedEnabled)
        {
            var proceedChoice = TreasureRoomObserverSignals.TryGetProceedChoice(request.Observer);
            if (proceedChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    proceedChoice,
                    "treasure proceed",
                    "Treasure relic picking is finished. Use the explicit treasure proceed affordance before any map routing fallback.",
                    0.96,
                    request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                    1400);
            }
        }

        if (WasRecentTreasureProceedAction(request.History))
        {
            return null;
        }

        var relicHolderChoice = TreasureRoomObserverSignals.GetRelicHolderChoices(request.Observer).FirstOrDefault();
        if (relicHolderChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                relicHolderChoice,
                "treasure relic holder",
                "Treasure chest is open and proceed is not yet authoritative. Click the explicit treasure relic holder, not a top-bar inventory relic icon or map node.",
                0.97,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                1500);
        }

        return null;
    }

    private static bool WasRecentTreasureProceedAction(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        for (var index = history.Count - 1; index >= 0 && index >= history.Count - 5; index -= 1)
        {
            var entry = history[index];
            if (string.Equals(entry.TargetLabel, "treasure proceed", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(entry.Action, "observer-accepted", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(entry.Action, "recapture-required", StringComparison.OrdinalIgnoreCase)
                && !entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return false;
    }

    private static GuiSmokeStepDecision CreateTreasureChestCenterDecision(GuiSmokeStepRequest request)
    {
        var chestChoice = request.Observer.Choices.FirstOrDefault(choice =>
            choice.Label.Contains("Chest", StringComparison.OrdinalIgnoreCase)
            || choice.Label.Contains("\uC0C1\uC790", StringComparison.OrdinalIgnoreCase));
        var normalizedX = 0.500d;
        var normalizedY = 0.560d;
        if (chestChoice is not null
            && TryParseNodeBounds(chestChoice.ScreenBounds, out var chestBounds)
            && HasUsableLogicalBounds(chestChoice.ScreenBounds))
        {
            var centerX = chestBounds.X + chestBounds.Width / 2f;
            var centerY = chestBounds.Y + chestBounds.Height / 2f;
            normalizedX = Math.Clamp(centerX / 1920f, 0.35d, 0.65d);
            normalizedY = Math.Clamp(centerY / 1080f, 0.35d, 0.70d);
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            "treasure chest center",
            "Treasure chest room detected. Open the center chest before trying map routing or proceed actions.",
            0.96,
            "map",
            1500,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateRestSiteUpgradeDecision(GuiSmokeStepRequest request)
    {
        if (!LooksLikeRestSiteState(request.Observer))
        {
            return null;
        }

        var observerConfirmChoice = RestSiteObserverSignals.TryGetSmithConfirmChoice(request.Observer);
        if (observerConfirmChoice is not null
            && observerConfirmChoice.Enabled != false
            && !string.IsNullOrWhiteSpace(observerConfirmChoice.ScreenBounds)
            && HasActiveNodeBounds(observerConfirmChoice.ScreenBounds, request.WindowBounds))
        {
            return CreateClickDecisionFromChoice(
                request,
                observerConfirmChoice,
                "rest site: smith confirm",
                "Rest site smith confirm is runtime-visible. Confirm the selected upgrade instead of repeating the smith option click.",
                0.95,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "upgrade",
                1400);
        }

        var observerSmithCardChoice = RestSiteObserverSignals.TryGetFirstSmithCardChoice(request.Observer);
        if (observerSmithCardChoice is not null
            && observerSmithCardChoice.Enabled != false
            && !string.IsNullOrWhiteSpace(observerSmithCardChoice.ScreenBounds)
            && HasActiveNodeBounds(observerSmithCardChoice.ScreenBounds, request.WindowBounds))
        {
            return CreateClickDecisionFromChoice(
                request,
                observerSmithCardChoice,
                "rest site: smith card",
                "Rest site smith grid is runtime-visible. Select an exported upgrade card hitbox instead of relying on screenshot-only inference.",
                0.94,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "upgrade",
                1400);
        }

        if (RestSiteObserverSignals.HasExportedSmithUpgradeChoices(request.Observer))
        {
            return null;
        }

        var lastUpgradeAction = request.History
            .Where(entry =>
                string.Equals(entry.TargetLabel, "rest site: smith card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.TargetLabel, "rest site: smith confirm", StringComparison.OrdinalIgnoreCase))
            .LastOrDefault();
        if (lastUpgradeAction is not null
            && string.Equals(lastUpgradeAction.TargetLabel, "rest site: smith card", StringComparison.OrdinalIgnoreCase))
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.949,
                0.716,
                "rest site: smith confirm",
                "Rest site upgrade comparison is visible. Click the right-side confirm button after selecting the card.",
                0.94,
                "map",
                1400,
                true,
                null);
        }

        var analysis = AutoRestSiteCardGridAnalyzer.Analyze(request.ScreenshotPath);
        if (!analysis.HasSelectableCard)
        {
            return null;
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            analysis.CardNormalizedX,
            analysis.CardNormalizedY,
            "rest site: smith card",
            "Rest site upgrade card grid is visible. Select a visible card before confirming with V.",
            0.91,
            "map",
            1400,
            true,
            null);
    }

    private static string? TryFindRestSiteUpgradeBounds(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        return decision.TargetLabel switch
        {
            { } label when label.Contains("smith confirm", StringComparison.OrdinalIgnoreCase)
                => RestSiteObserverSignals.TryGetSmithConfirmChoice(request.Observer)?.ScreenBounds,
            { } label when label.Contains("smith card", StringComparison.OrdinalIgnoreCase)
                => RestSiteObserverSignals.TryGetFirstSmithCardChoice(request.Observer)?.ScreenBounds,
            _ => null,
        };
    }

    private static string? TryResolveRestSiteUpgradeBoundsSource(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        return decision.TargetLabel switch
        {
            { } label when label.Contains("smith confirm", StringComparison.OrdinalIgnoreCase)
                && RestSiteObserverSignals.TryGetSmithConfirmChoice(request.Observer) is not null => "observer-rest-site-smith-confirm",
            { } label when label.Contains("smith card", StringComparison.OrdinalIgnoreCase)
                && RestSiteObserverSignals.TryGetFirstSmithCardChoice(request.Observer) is not null => "observer-rest-site-smith-card",
            _ => "screenshot-rest-site-upgrade",
        };
    }

    private static bool HasContradictoryForegroundOwnerAgainstMapFallback(GuiSmokeStepRequest request)
    {
        var canonicalScene = TryBuildCanonicalNonCombatSceneState(
            request.Observer,
            request.WindowBounds,
            request.History,
            request.ScreenshotPath);
        return RewardObserverSignals.IsTerminalRunBoundary(request.Observer)
               || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(request.Observer)
               || CardSelectionObserverSignals.TryGetState(request.Observer) is not null
               || HasExplicitRestSiteChoiceAuthority(request)
               || canonicalScene is { CanonicalForegroundOwner: not NonCombatCanonicalForegroundOwner.Unknown and not NonCombatCanonicalForegroundOwner.Map };
    }

    private static GuiSmokeStepDecision? TryFindVisibleMapAdvanceDecision(GuiSmokeStepRequest request)
    {
        if (HasContradictoryForegroundOwnerAgainstMapFallback(request)
            || !GuiSmokeNonCombatContractSupport.AllowsAction(request, "click visible map advance"))
        {
            return null;
        }

        if (BuildRewardMapLayerState(request.Observer, request.WindowBounds).RewardPanelVisible)
        {
            return null;
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (mapOverlayState.ForegroundVisible || mapOverlayState.ExportedReachableNodeCandidatePresent)
        {
            return null;
        }

        var attempt = request.History.Count(entry =>
            string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.TargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase));

        var analysis = AutoMapAnalyzer.Analyze(request.ScreenshotPath);
        if (!analysis.HasCurrentArrow)
        {
            return null;
        }

        var offset = attempt switch
        {
            0 => new PointF(0f, -0.105f),
            1 => new PointF(-0.060f, -0.110f),
            2 => new PointF(0.060f, -0.110f),
            _ => new PointF(0f, -0.135f),
        };

        var normalizedX = Math.Clamp(analysis.ArrowNormalizedX + offset.X, 0.08f, 0.92f);
        var normalizedY = Math.Clamp(analysis.ArrowNormalizedY + offset.Y, 0.10f, 0.86f);
        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            "visible map advance",
            $"Visible map is present while logical flow remains '{request.Observer.CurrentScreen}'. Advance from the red current-node arrow using screenshot-derived positioning (attempt {attempt + 1}).",
            0.78,
            "map",
            1500,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateRestSiteDecision(GuiSmokeStepRequest request)
    {
        return BuildRestSiteDecisionCandidates(request)
            .Select(static candidate => candidate.Decision)
            .FirstOrDefault(static decision => decision is not null);
    }

    private static GuiSmokeStepDecision? TryCreateVisibleProceedDecision(GuiSmokeStepRequest request)
    {
        if (!GuiSmokeNonCombatContractSupport.AllowsAction(request, "click proceed"))
        {
            return null;
        }

        var proceedNode = request.Observer.ActionNodes.FirstOrDefault(node =>
            node.Actionable
            && IsProceedNode(node)
            && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds));
        if (proceedNode is not null)
        {
            return CreateClickDecisionFromNode(request, proceedNode, "visible proceed");
        }

        var proceedChoice = request.Observer.Choices.FirstOrDefault(choice =>
            !string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
            && IsProceedLikeLabel(choice.Label)
            && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds));
        if (proceedChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                proceedChoice,
                "visible proceed",
                "Observer-exported proceed choice is visible. Advance the room flow before attempting any map click.",
                0.96,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                1400);
        }

        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(request.ScreenshotPath);
        if (!overlayAnalysis.HasRightProceedArrow || overlayAnalysis.HasBottomLeftBackArrow || overlayAnalysis.HasCentralOverlayPanel)
        {
            return null;
        }

        if (overlayAnalysis.RightProceedNormalizedX is not null && overlayAnalysis.RightProceedNormalizedY is not null)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                overlayAnalysis.RightProceedNormalizedX,
                overlayAnalysis.RightProceedNormalizedY,
                "visible proceed",
                "The screenshot shows a right-side proceed arrow cluster without an active overlay back arrow. Advance the room flow before attempting any map click.",
                0.95,
                "map",
                1400,
                true,
                null);
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            0.885,
            0.835,
            "visible proceed",
            "The screenshot shows the large right-side proceed arrow without an active overlay back arrow. Advance the room flow before attempting any map click.",
            0.95,
            "map",
            1400,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateHiddenOverlayCleanupDecision(GuiSmokeStepRequest request)
    {
        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(request.ScreenshotPath);
        if (overlayAnalysis.HasCentralOverlayPanel)
        {
            return null;
        }

        var choices = request.Observer.CurrentChoices;
        var hasOverlayChoices = choices.Any(static label =>
            label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
            || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
            || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
        if (!hasOverlayChoices)
        {
            return null;
        }

        var cleanupAttempts = request.History.Count(entry =>
            string.Equals(entry.TargetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase));
        if (cleanupAttempts >= 2)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.180,
                0.520,
                "hidden overlay close",
                "Overlay controls remain active in the room state even though no central panel is visible. Retry backdrop dismissal before progressing.",
                0.72,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1200,
                true,
                null);
        }

        return new GuiSmokeStepDecision(
            "act",
            "press-key",
            "Escape",
            null,
            null,
            "hidden overlay close",
            "Overlay controls remain active behind the visible room. Send escape/cancel before trying to progress.",
            0.84,
            request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
            1000,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateOverlayAdvanceDecision(GuiSmokeStepRequest request)
    {
        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(request.ScreenshotPath);
        if (!overlayAnalysis.HasCentralOverlayPanel)
        {
            return null;
        }

        var choices = request.Observer.CurrentChoices;
        var hasBackstop = choices.Any(static label => label.Contains("Backstop", StringComparison.OrdinalIgnoreCase));
        var hasLeftArrow = choices.Any(static label => label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase));
        var hasRightArrow = choices.Any(static label => label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
        if (!hasBackstop && !hasLeftArrow && !hasRightArrow)
        {
            return null;
        }

        var recentOverlayBackCount = CountRecentOverlayCleanupAttempts(request.History);

        if (recentOverlayBackCount >= 3)
        {
            return new GuiSmokeStepDecision(
                "act",
                "press-key",
                "Escape",
                null,
                null,
                "overlay close",
                "Overlay remains open after repeated dismiss attempts. Send escape before trying to progress again.",
                0.72,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1200,
                true,
                null);
        }

        if (overlayAnalysis.HasBottomLeftBackArrow)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.045,
                0.905,
                "overlay back",
                "An inspect overlay is present above the room flow. Close it via the visible back arrow before trying to progress.",
                0.93,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1200,
                true,
                null);
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            0.180,
            0.520,
            "overlay backdrop close",
            "A centered inspect overlay is visible without a dedicated back arrow. Click the dark backdrop to dismiss it before trying to progress.",
            0.88,
            request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
            1200,
            true,
            null);
    }

    private static bool IsProceedNode(ObserverActionNode node)
    {
        return node.Label.Contains("\uC9C4\uD589", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("\uB118\uAE30\uAE30", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Proceed", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Continue", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBackNode(ObserverActionNode node)
    {
        return node.Label.Contains("\uB4A4\uB85C", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Back", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMapNode(ObserverActionNode node)
    {
        return node.Kind.Contains("map", StringComparison.OrdinalIgnoreCase)
               || node.NodeId.Contains("map", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Map", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeTreasureState(ObserverSummary observer)
    {
        if (string.Equals(observer.EncounterKind, "Treasure", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return observer.CurrentChoices.Any(static label =>
            label.Contains("Chest", StringComparison.OrdinalIgnoreCase)
            || label.Contains("\uC0C1\uC790", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeRestSiteState(ObserverSummary observer)
    {
        return GuiSmokeNonCombatContractSupport.LooksLikeRestSiteState(observer);
    }

    private static bool LooksLikeScreenshotFirstRoomState(GuiSmokeStepRequest request)
    {
        return LooksLikeRestSiteState(request.Observer)
               || LooksLikeTreasureState(request.Observer)
               || LooksLikeShopState(request.Observer)
               || (string.Equals(request.Observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
                   && request.Observer.Choices.Any(choice =>
                       string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)));
    }

    private static bool LooksLikeMapTransitionState(GuiSmokeStepRequest request)
    {
        if (HasContradictoryForegroundOwnerAgainstMapFallback(request))
        {
            return false;
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        return mapOverlayState.ForegroundVisible
               || string.Equals(request.Observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
               || string.Equals(request.Observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
               || request.SceneSignature.Contains("substate:map-transition", StringComparison.OrdinalIgnoreCase)
               || request.SceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase)
               || request.SceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)
               || request.Observer.LastEventsTail.Any(eventTail =>
                   eventTail.Contains("screen-changed: map", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("map-point-selected", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("\"screen\":\"map\"", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("\"currentScreen\":\"map\"", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsBackChoiceLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        return label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
               || label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
               || label.Contains("\uB4A4\uB85C", StringComparison.OrdinalIgnoreCase)
               || label.Contains("Back", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasOverlayChoiceState(ObserverSummary observer)
    {
        return observer.CurrentChoices.Any(static label =>
            label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
            || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
            || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
    }

    internal static RewardSceneState BuildRewardSceneState(
        ObserverState observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        return BuildRewardSceneState(observer.Summary, windowBounds, history, screenshotPath);
    }

    internal static RewardSceneState BuildRewardSceneState(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        var rewardState = RewardObserverSignals.TryGetState(observer);
        var mapContextVisible = rewardState?.MapIsCurrentActiveScreen == true
                                || GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer)
                                || string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase);
        var rewardBackNavigationAvailable = HasOverlayChoiceState(observer)
                                            || observer.ActionNodes.Any(static node => node.Actionable && IsBackNode(node));
        var activeRewardChoices = observer.Choices.Where(choice => IsCurrentRewardProgressionChoice(choice, windowBounds)).ToArray();
        var activeRewardNodes = observer.ActionNodes.Where(node => IsCurrentRewardProgressionNode(node, windowBounds)).ToArray();
        var staleRewardChoices = observer.Choices.Where(choice => IsStaleRewardProgressionChoice(choice, windowBounds)).ToArray();
        var staleRewardNodes = observer.ActionNodes.Where(node => IsStaleRewardProgressionNode(node, windowBounds)).ToArray();
        var explicitRewardChoicesPresent = activeRewardChoices.Length > 0 || activeRewardNodes.Length > 0;
        var rewardContextVisible = rewardState?.ScreenVisible == true
                                   || RewardObserverSignals.IsRewardAuthorityActive(observer)
                                   || GuiSmokeObserverPhaseHeuristics.LooksLikeRewardsState(observer)
                                   || string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(observer.VisibleScreen, "rewards", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase);
        var intrinsicRewardCardAuthority = observer.Choices.Any(IsRewardCardChoice);
        var fallbackRewardChoiceAuthority = !CardSelectionObserverSignals.IsNonRewardCountConfirmFamily(observer)
                                            && !GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer)
                                            && (intrinsicRewardCardAuthority
                                                || (rewardContextVisible
                                                    && observer.Choices.Any(IsInspectPreviewChoice)
                                                    && observer.Choices.Any(static choice => IsSkipOrProceedLabel(choice.Label))));
        var rewardScreenHint = rewardContextVisible || fallbackRewardChoiceAuthority;
        var rewardForegroundOwned = rewardState?.ForegroundOwned == true
                                    || (rewardScreenHint
                                        && rewardState?.MapIsCurrentActiveScreen != true
                                        && (explicitRewardChoicesPresent || fallbackRewardChoiceAuthority));
        var staleRewardChoicePresent = !rewardForegroundOwned && (staleRewardChoices.Length > 0 || staleRewardNodes.Length > 0);
        var offWindowBoundsReused = staleRewardChoices.Any(choice => IsOffWindowBounds(choice.ScreenBounds, windowBounds))
                                  || staleRewardNodes.Any(node => IsOffWindowBounds(node.ScreenBounds, windowBounds));
        var layerState = new RewardMapLayerState(
            RewardPanelVisible: rewardForegroundOwned,
            MapContextVisible: mapContextVisible,
            RewardBackNavigationAvailable: rewardBackNavigationAvailable,
            StaleRewardChoicePresent: staleRewardChoicePresent,
            StaleRewardBoundsPresent: staleRewardChoicePresent && (staleRewardChoices.Length > 0 || staleRewardNodes.Length > 0),
            OffWindowBoundsReused: offWindowBoundsReused,
            RewardForegroundOwned: rewardForegroundOwned,
            RewardTeardownInProgress: rewardState?.TeardownInProgress == true,
            RewardIsCurrentActiveScreen: rewardState?.RewardIsCurrentActiveScreen == true,
            MapCurrentActiveScreen: rewardState?.MapIsCurrentActiveScreen == true,
            TerminalRunBoundary: rewardState?.TerminalRunBoundary == true);

        var rewardChoiceVisible = !CardSelectionObserverSignals.IsNonRewardCountConfirmFamily(observer)
                                  && !GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer)
                                  && rewardScreenHint
                                  && (observer.Choices.Count(IsRewardCardChoice) > 0
                                      || (observer.Choices.Count(IsInspectPreviewChoice) > 0
                                          && observer.Choices.Any(static choice => IsSkipOrProceedLabel(choice.Label))));
        var colorlessChoiceVisible = !CardSelectionObserverSignals.IsNonRewardCountConfirmFamily(observer)
                                     && !GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer)
                                     && rewardScreenHint
                                     && observer.Choices.Any(IsRewardCardChoice)
                                     && observer.Choices.Any(IsInspectPreviewChoice);
        var explicitProceedVisible = activeRewardChoices.Any(choice => IsSkipOrProceedLabel(choice.Label))
                                     || activeRewardNodes.Any(IsProceedNode);
        var claimableRewardPresent = activeRewardChoices.Any(choice => !IsSkipOrProceedLabel(choice.Label))
                                     || activeRewardNodes.Any(node => !IsProceedNode(node))
                                     || (!string.IsNullOrWhiteSpace(screenshotPath)
                                         && HasScreenshotClaimableRewardEvidenceInScreenshot(observer, screenshotPath));
        var suppressSameSkipReissue = rewardForegroundOwned
                                      && explicitProceedVisible
                                      && !claimableRewardPresent
                                      && HasRecentRewardSkipReleaseIntent(history);
        var canonicalOwner = rewardForegroundOwned
            ? NonCombatForegroundOwner.Reward
            : (layerState.RewardTeardownInProgress || layerState.MapCurrentActiveScreen
                ? NonCombatForegroundOwner.Map
                : NonCombatForegroundOwner.Unknown);
        var releaseStage = canonicalOwner == NonCombatForegroundOwner.Reward
            ? (suppressSameSkipReissue ? RewardReleaseStage.ReleasePending : RewardReleaseStage.Active)
            : ((layerState.RewardTeardownInProgress || layerState.MapCurrentActiveScreen)
                ? RewardReleaseStage.Released
                : RewardReleaseStage.None);
        var explicitAction = releaseStage != RewardReleaseStage.Active
            ? RewardExplicitActionKind.None
            : colorlessChoiceVisible
                ? RewardExplicitActionKind.ColorlessChoice
                : rewardChoiceVisible || claimableRewardPresent
                    ? (rewardChoiceVisible ? RewardExplicitActionKind.CardChoice : RewardExplicitActionKind.Claim)
                    : rewardBackNavigationAvailable && mapContextVisible && staleRewardChoicePresent && !layerState.MapCurrentActiveScreen
                        ? RewardExplicitActionKind.Back
                        : explicitProceedVisible
                            ? RewardExplicitActionKind.SkipProceed
                            : RewardExplicitActionKind.None;

        return new RewardSceneState(
            layerState,
            rewardState,
            canonicalOwner,
            releaseStage,
            explicitAction,
            rewardChoiceVisible,
            colorlessChoiceVisible,
            claimableRewardPresent,
            explicitProceedVisible,
            suppressSameSkipReissue);
    }

    internal static EventSceneState BuildEventSceneState(
        ObserverState observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        return BuildEventSceneState(observer.Summary, windowBounds, history, screenshotPath);
    }

    internal static EventSceneState BuildEventSceneState(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        var rewardScene = BuildRewardSceneState(observer, windowBounds, history, screenshotPath);
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(observer, windowBounds, screenshotPath);
        var mapExplicitOwner = NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer);
        var ancientDialogueActive = AncientEventObserverSignals.IsDialogueActive(observer);
        var ancientCompletionActive = AncientEventObserverSignals.HasExplicitCompletionAction(observer);
        var ancientOptionActive = AncientEventObserverSignals.HasExplicitOptionSelection(observer);
        var eventChoiceAuthority = EventProceedObserverSignals.HasEventChoiceAuthority(observer);
        var genericEventProgressVisible = eventChoiceAuthority
                                          && !rewardScene.RewardForegroundOwned
                                          && HasRawEventProgressionSurface(observer, windowBounds);
        var explicitProceedVisible = eventChoiceAuthority
                                     && !mapExplicitOwner
                                     && !rewardScene.RewardForegroundOwned
                                     && EventProceedObserverSignals.HasExplicitEventProceedSignal(observer, windowBounds);
        var activeEventChoiceVisible = eventChoiceAuthority
                                       && !mapExplicitOwner
                                       && !rewardScene.RewardForegroundOwned
                                       && HasRawExplicitEventChoiceVisible(observer, windowBounds);
        var forceProgressionAfterCardSelection = HasRecentCardSelectionSubtypeAftermath(history ?? Array.Empty<GuiSmokeHistoryEntry>())
                                                && (explicitProceedVisible || activeEventChoiceVisible || genericEventProgressVisible);
        var rewardSubstateActive = rewardScene.RewardForegroundOwned || rewardScene.ReleaseStage == RewardReleaseStage.ReleasePending;
        var mapContextVisible = mapOverlayState.ForegroundVisible
                                || string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
                                || rewardScene.LayerState.MapContextVisible
                                || mapExplicitOwner;
        var hasExplicitProgression = ancientDialogueActive
                                     || ancientCompletionActive
                                     || ancientOptionActive
                                     || explicitProceedVisible
                                     || activeEventChoiceVisible
                                     || genericEventProgressVisible
                                     || forceProgressionAfterCardSelection;
        var strongForegroundChoice = ancientDialogueActive
                                     || ancientCompletionActive
                                     || ancientOptionActive
                                     || explicitProceedVisible
                                     || activeEventChoiceVisible
                                     || forceProgressionAfterCardSelection;
        var suppressSameProceedReissue = !rewardSubstateActive
                                         && (ancientCompletionActive || explicitProceedVisible)
                                         && HasRecentEventReleaseIntent(history);
        var canonicalOwner = rewardSubstateActive
            ? NonCombatForegroundOwner.Reward
            : mapExplicitOwner
                ? NonCombatForegroundOwner.Map
                : mapOverlayState.ForegroundVisible && !strongForegroundChoice
                    ? NonCombatForegroundOwner.Map
                    : ancientDialogueActive || ancientCompletionActive || ancientOptionActive || (eventChoiceAuthority && hasExplicitProgression)
                        ? NonCombatForegroundOwner.Event
                        : NonCombatForegroundOwner.Unknown;
        var releaseStage = rewardSubstateActive
            ? EventReleaseStage.Released
            : canonicalOwner == NonCombatForegroundOwner.Event
                ? suppressSameProceedReissue
                    ? EventReleaseStage.ReleasePending
                    : EventReleaseStage.Active
                : HasRecentEventReleaseIntent(history) && (mapExplicitOwner || mapOverlayState.ForegroundVisible)
                    ? EventReleaseStage.Released
                    : EventReleaseStage.None;
        var explicitAction = rewardSubstateActive
            ? EventExplicitActionKind.RewardSubstate
            : ancientDialogueActive
                ? EventExplicitActionKind.AncientDialogue
                : ancientCompletionActive
                    ? EventExplicitActionKind.AncientCompletion
                    : ancientOptionActive
                        ? EventExplicitActionKind.AncientOption
                        : explicitProceedVisible
                            ? EventExplicitActionKind.Proceed
                            : activeEventChoiceVisible || forceProgressionAfterCardSelection
                                ? EventExplicitActionKind.EventChoice
                                : EventExplicitActionKind.None;

        return new EventSceneState(
            canonicalOwner,
            releaseStage,
            explicitAction,
            rewardScene,
            mapOverlayState,
            mapContextVisible,
            rewardSubstateActive,
            hasExplicitProgression,
            strongForegroundChoice,
            forceProgressionAfterCardSelection,
            explicitProceedVisible,
            suppressSameProceedReissue);
    }

    internal static ShopSceneState? BuildShopSceneState(
        ObserverState observer,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null)
    {
        var state = ShopObserverSignals.TryGetState(observer.Summary);
        return state is null
            ? null
            : new ShopSceneState(state, ShopObserverSignals.HasRecentPurchase(history ?? Array.Empty<GuiSmokeHistoryEntry>()));
    }

    internal static ShopSceneState? BuildShopSceneState(
        ObserverSummary observer,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null)
    {
        return BuildShopSceneState(new ObserverState(observer, null, null, null), history);
    }

    internal static RestSiteSceneState? BuildRestSiteSceneState(ObserverState observer)
    {
        var explicitScreenAuthority = string.Equals(observer.CurrentScreen, "rest-site", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(observer.VisibleScreen, "rest-site", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase);
        var smithUpgradeActive = RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer.Summary);
        var hasAuthoritativeChoiceMetadata = RestSiteChoiceSupport.HasAuthoritativeMetadata(observer.Summary);
        var explicitChoiceVisible = hasAuthoritativeChoiceMetadata
                                    || (explicitScreenAuthority
                                        && RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(observer.Summary));
        var proceedVisible = explicitScreenAuthority
                             && GuiSmokeNonCombatContractSupport.LooksLikeRestSiteProceedState(observer.Summary);
        if (!smithUpgradeActive && !explicitChoiceVisible && !proceedVisible)
        {
            return null;
        }

        var mapContextVisible = string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
                                || NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer.Summary);
        return new RestSiteSceneState(
            explicitChoiceVisible,
            smithUpgradeActive,
            RestSiteObserverSignals.HasSmithConfirmVisible(observer.Summary),
            proceedVisible,
            mapContextVisible);
    }

    internal static RestSiteSceneState? BuildRestSiteSceneState(ObserverSummary observer)
    {
        return BuildRestSiteSceneState(new ObserverState(observer, null, null, null));
    }

    internal static TreasureSceneState? BuildTreasureSceneState(ObserverState observer)
    {
        var state = TreasureRoomObserverSignals.TryGetState(observer.Summary);
        return state is null ? null : new TreasureSceneState(state);
    }

    internal static TreasureSceneState? BuildTreasureSceneState(ObserverSummary observer)
    {
        return BuildTreasureSceneState(new ObserverState(observer, null, null, null));
    }

    internal static ICanonicalNonCombatSceneState? TryBuildCanonicalNonCombatSceneState(
        ObserverState observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(observer.Summary)
            || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary))
        {
            return null;
        }

        var rewardScene = BuildRewardSceneState(observer, windowBounds, history, screenshotPath);
        if (rewardScene.RewardForegroundOwned || rewardScene.ReleaseStage != RewardReleaseStage.None)
        {
            return rewardScene;
        }

        if (BuildShopSceneState(observer, history) is { ReleaseStage: not NonCombatReleaseStage.None } shopScene)
        {
            return shopScene;
        }

        if (BuildRestSiteSceneState(observer) is { } restSiteScene)
        {
            return restSiteScene;
        }

        if (BuildTreasureSceneState(observer) is { } treasureScene)
        {
            return treasureScene;
        }

        var eventScene = BuildEventSceneState(observer, windowBounds, history, screenshotPath);
        if (eventScene.RewardSubstateActive)
        {
            return eventScene.RewardScene;
        }

        if (eventScene.EventForegroundOwned || eventScene.ReleaseStage != EventReleaseStage.None)
        {
            return eventScene;
        }

        return null;
    }

    internal static ICanonicalNonCombatSceneState? TryBuildCanonicalNonCombatSceneState(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        return TryBuildCanonicalNonCombatSceneState(new ObserverState(observer, null, null, null), windowBounds, history, screenshotPath);
    }

    private static bool HasRecentRewardSkipReleaseIntent(IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        if (history is null || history.Count == 0)
        {
            return false;
        }

        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Action, "observer-accepted", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Action, "recapture-required", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return string.Equals(entry.TargetLabel, "reward skip", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(entry.TargetLabel, "proceed after resolving rewards", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    internal static bool HasRecentEventReleaseIntent(IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        if (history is null || history.Count == 0)
        {
            return false;
        }

        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Action, "observer-accepted", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Action, "recapture-required", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return string.Equals(entry.TargetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(entry.TargetLabel, "ancient event completion", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static RewardMapLayerState BuildRewardMapLayerState(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return BuildRewardSceneState(observer, windowBounds).LayerState;
    }

    private static bool LooksLikeInspectOverlayState(ObserverSummary observer)
    {
        return HasOverlayChoiceState(observer)
               || observer.ActionNodes.Any(static node => IsOverlayChoiceLabel(node.Label));
    }

    private static bool ShouldSuppressRoomSubstateHeuristics(GuiSmokePhase phase, ObserverSummary observer)
    {
        return phase == GuiSmokePhase.HandleCombat
               || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer);
    }

    private static bool HasExplicitRewardProgressionAffordance(ObserverSummary observer)
    {
        return observer.Choices.Any(choice => IsCurrentRewardProgressionChoice(choice, null))
               || observer.ActionNodes.Any(node => IsCurrentRewardProgressionNode(node, null));
    }

    private static bool IsCurrentRewardProgressionChoice(ObserverChoice choice, WindowBounds? windowBounds)
    {
        return IsExplicitRewardProgressionChoice(choice)
               && HasActiveRewardBounds(choice.ScreenBounds, windowBounds);
    }

    private static bool IsCurrentRewardProgressionNode(ObserverActionNode node, WindowBounds? windowBounds)
    {
        return IsExplicitRewardProgressionNode(node)
               && HasActiveRewardBounds(node.ScreenBounds, windowBounds);
    }

    private static bool IsStaleRewardProgressionChoice(ObserverChoice choice, WindowBounds? windowBounds)
    {
        return IsExplicitRewardProgressionChoice(choice)
               && !HasActiveRewardBounds(choice.ScreenBounds, windowBounds);
    }

    private static bool IsStaleRewardProgressionNode(ObserverActionNode node, WindowBounds? windowBounds)
    {
        return IsExplicitRewardProgressionNode(node)
               && !HasActiveRewardBounds(node.ScreenBounds, windowBounds);
    }

    private static bool HasActiveRewardBounds(string? screenBounds, WindowBounds? windowBounds)
    {
        if (HasUsableLogicalBounds(screenBounds))
        {
            return true;
        }

        return windowBounds is not null && IsBoundsInsideWindow(screenBounds, windowBounds);
    }

    private static bool HasActiveNodeBounds(string? screenBounds, WindowBounds? windowBounds)
    {
        if (HasUsableLogicalBounds(screenBounds))
        {
            return true;
        }

        return windowBounds is not null && IsBoundsInsideWindow(screenBounds, windowBounds);
    }

    private static bool IsOffWindowBounds(string? screenBounds, WindowBounds? windowBounds)
    {
        if (string.IsNullOrWhiteSpace(screenBounds) || windowBounds is null || !TryParseNodeBounds(screenBounds, out _))
        {
            return false;
        }

        return !HasUsableLogicalBounds(screenBounds)
               && !IsBoundsInsideWindow(screenBounds, windowBounds);
    }

    private static bool IsBoundsInsideWindow(string? screenBounds, WindowBounds windowBounds)
    {
        if (!TryParseNodeBounds(screenBounds, out var bounds))
        {
            return false;
        }

        return bounds.Right > windowBounds.X
               && bounds.Bottom > windowBounds.Y
               && bounds.X < windowBounds.X + windowBounds.Width
               && bounds.Y < windowBounds.Y + windowBounds.Height;
    }

    private static bool IsExplicitRewardProgressionChoice(ObserverChoice choice)
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

    private static bool IsExplicitRewardProgressionNode(ObserverActionNode node)
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

    private static bool IsRewardCardChoice(ObserverChoice choice)
    {
        if (choice.Kind.StartsWith("transform-", StringComparison.OrdinalIgnoreCase)
            || choice.Kind.StartsWith("deck-remove-", StringComparison.OrdinalIgnoreCase)
            || choice.Kind.StartsWith("upgrade-", StringComparison.OrdinalIgnoreCase)
            || choice.SemanticHints.Any(static hint =>
                string.Equals(hint, "card-selection:transform", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "card-selection:deck-remove", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "card-selection:upgrade", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return (string.Equals(choice.Kind, "card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.Kind, "reward-card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.Kind, "reward-pick-card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(choice.BindingId, "CardReward", StringComparison.OrdinalIgnoreCase)
                || choice.SemanticHints.Any(static hint => string.Equals(hint, "reward-card", StringComparison.OrdinalIgnoreCase)
                                                           || string.Equals(hint, "reward-pick", StringComparison.OrdinalIgnoreCase)
                                                           || string.Equals(hint, "reward-type:CardReward", StringComparison.OrdinalIgnoreCase)))
               && !IsSkipOrProceedLabel(choice.Label)
               && !IsConfirmLikeLabel(choice.Label)
               && !IsDismissLikeLabel(choice.Label)
               && !IsOverlayChoiceLabel(choice.Label)
               && HasRewardCardLikeBounds(choice.ScreenBounds);
    }

    private static bool HasRewardCardLikeBounds(string? screenBounds)
    {
        if (!TryParseNodeBounds(screenBounds, out var bounds))
        {
            return true;
        }

        return bounds.Width >= 120f || bounds.Height >= 150f;
    }

    private static bool IsInspectPreviewChoice(ObserverChoice choice)
    {
        return IsOverlayChoiceLabel(choice.Label)
               || string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || IsInspectPreviewBounds(choice.ScreenBounds);
    }

    private static int ScoreExplicitRewardProgressionChoice(ObserverChoice choice)
    {
        if (IsRewardCardChoice(choice))
        {
            return 280;
        }

        if (IsPotionRewardChoice(choice))
        {
            return 220;
        }

        if (IsRelicRewardChoice(choice))
        {
            return 250;
        }

        if (IsGoldRewardChoice(choice))
        {
            return 180;
        }

        if (IsSkipLikeLabel(choice.Label))
        {
            return 70;
        }

        if (IsProceedLikeLabel(choice.Label) || IsConfirmLikeLabel(choice.Label))
        {
            return 110;
        }

        if (choice.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
            || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true)
        {
            return 220;
        }

        if (HasLargeChoiceBounds(choice.ScreenBounds))
        {
            return 200;
        }

        return ScoreProgressionChoice(choice);
    }

    private static int ScoreExplicitRewardProgressionNode(ObserverActionNode node)
    {
        if (IsProceedNode(node))
        {
            return 110;
        }

        if (node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
            || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase))
        {
            return 220;
        }

        if (HasLargeChoiceBounds(node.ScreenBounds))
        {
            return 200;
        }

        return ScoreProgressionNode(node);
    }

    private static bool IsOverlayCleanupTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay back", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay backdrop close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "inspect overlay escape", StringComparison.OrdinalIgnoreCase);
    }

    private static int CountRecentOverlayCleanupAttempts(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var count = 0;
        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (IsOverlayCleanupTarget(entry.TargetLabel))
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

    private static int ScoreProgressionChoice(ObserverChoice choice)
    {
        if (IsOverlayChoiceLabel(choice.Label))
        {
            return -400;
        }

        if (IsInspectPreviewChoice(choice))
        {
            return -240;
        }

        if (IsDismissLikeLabel(choice.Label))
        {
            return -120;
        }

        if (IsExplicitEventProceedChoice(choice))
        {
            return 180;
        }

        if (IsRewardCardChoice(choice))
        {
            return 240;
        }

        if (IsRelicRewardChoice(choice))
        {
            return 210;
        }

        if (IsGoldRewardChoice(choice))
        {
            return 120;
        }

        if (IsSkipLikeLabel(choice.Label))
        {
            return 20;
        }

        if (IsProceedLikeLabel(choice.Label) || IsConfirmLikeLabel(choice.Label))
        {
            return 60;
        }

        if (HasLargeChoiceBounds(choice.ScreenBounds))
        {
            return 120;
        }

        return TryParseNodeBounds(choice.ScreenBounds, out _) ? 20 : 0;
    }

    private static int ScoreProgressionNode(ObserverActionNode node)
    {
        if (IsOverlayChoiceLabel(node.Label))
        {
            return -400;
        }

        if (IsInspectPreviewBounds(node.ScreenBounds))
        {
            return -240;
        }

        if (IsDismissLikeLabel(node.Label))
        {
            return -120;
        }

        if (IsExplicitEventProceedNode(node))
        {
            return 180;
        }

        if (IsSkipLikeLabel(node.Label))
        {
            return 70;
        }

        if (IsProceedLikeLabel(node.Label) || IsConfirmLikeLabel(node.Label))
        {
            return 60;
        }

        if (HasLargeChoiceBounds(node.ScreenBounds))
        {
            return 120;
        }

        if (node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
            || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
            || node.Kind.Contains("choice", StringComparison.OrdinalIgnoreCase))
        {
            return 80;
        }

        return TryParseNodeBounds(node.ScreenBounds, out _) ? 20 : 0;
    }

    private static float GetChoiceSortY(ObserverChoice choice)
    {
        return TryParseNodeBounds(choice.ScreenBounds, out var bounds) ? bounds.Y : float.MaxValue;
    }

    private static float GetChoiceSortX(ObserverChoice choice)
    {
        return TryParseNodeBounds(choice.ScreenBounds, out var bounds) ? bounds.X : float.MaxValue;
    }

    private static float GetNodeSortY(ObserverActionNode node)
    {
        return TryParseNodeBounds(node.ScreenBounds, out var bounds) ? bounds.Y : float.MaxValue;
    }

    private static float GetNodeSortX(ObserverActionNode node)
    {
        return TryParseNodeBounds(node.ScreenBounds, out var bounds) ? bounds.X : float.MaxValue;
    }

    private static string GetProgressChoiceTargetLabel(ObserverActionNode node, ObserverSummary observer)
    {
        if (AncientEventObserverSignals.IsAncientDialogueNode(node))
        {
            return "ancient dialogue advance";
        }

        if (AncientEventObserverSignals.IsExplicitAncientCompletionNode(node))
        {
            return "ancient event completion";
        }

        if (IsExplicitEventProceedNode(node))
        {
            return "visible proceed";
        }

        return GetProgressChoiceTargetLabel(node.Label, observer);
    }

    private static string GetProgressChoiceTargetLabel(ObserverChoice choice, ObserverSummary observer)
    {
        if (AncientEventObserverSignals.IsAncientDialogueChoice(choice))
        {
            return "ancient dialogue advance";
        }

        if (AncientEventObserverSignals.IsExplicitAncientCompletionChoice(choice))
        {
            return "ancient event completion";
        }

        if (IsExplicitEventProceedChoice(choice))
        {
            return "visible proceed";
        }

        return GetProgressChoiceTargetLabel(choice.Label, observer);
    }

    private static string GetProgressChoiceTargetLabel(string? label, ObserverSummary observer)
    {
        if (IsSkipLikeLabel(label))
        {
            return "reward skip";
        }

        if (GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer) && observer.Choices.Any(IsRewardCardChoice))
        {
            return "colorless card choice";
        }

        if (GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer))
        {
            return "reward choice";
        }

        return "event progression choice";
    }

    private static bool IsGoldRewardChoice(ObserverChoice choice)
    {
        return ContainsAny(choice.Label, "골드", "gold")
               || ContainsAny(choice.Description, "골드", "gold")
               || ContainsAny(choice.Value, "GOLD.")
               || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                  && string.Equals(choice.BindingId, "GoldReward", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => string.Equals(hint, "reward-gold", StringComparison.OrdinalIgnoreCase)
                                                          || string.Equals(hint, "reward-type:GoldReward", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPotionRewardChoice(ObserverChoice choice)
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

    private static bool IsRelicRewardChoice(ObserverChoice choice)
    {
        return string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || ContainsAny(choice.Description, "relic", "유물")
               || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                  && string.Equals(choice.BindingId, "RelicReward", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => string.Equals(hint, "reward-relic", StringComparison.OrdinalIgnoreCase)
                                                          || string.Equals(hint, "reward-type:RelicReward", StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildProgressChoiceReason(ObserverChoice choice, ObserverSummary observer)
    {
        if (AncientEventObserverSignals.IsAncientDialogueChoice(choice))
        {
            return "Ancient event dialogue is still active. Advance it through the explicit dialogue hitbox before selecting an option.";
        }

        if (AncientEventObserverSignals.IsExplicitAncientCompletionChoice(choice))
        {
            return $"Ancient event completion '{choice.Label}' is still exported from an explicit NEventOptionButton proceed lane. Finish the event before handing off to map routing.";
        }

        if (IsExplicitEventProceedChoice(choice))
        {
            return $"Explicit event proceed '{choice.Label}' is exported from EventOption.IsProceed authority. Advance the event before considering any stale map overlay candidate.";
        }

        if (IsSkipLikeLabel(choice.Label))
        {
            return $"Progression skip '{choice.Label}' is visible. Prefer it over inspect or preview affordances.";
        }

        if (GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer) && observer.Choices.Any(IsRewardCardChoice))
        {
            return $"Colorless reward choice '{choice.Label}' is visible. Click a real card option, not the relic inspect icons.";
        }

        if (GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer))
        {
            return $"Reward progression choice '{choice.Label}' is visible. Prefer it over inspect or preview affordances.";
        }

        return $"Event progression choice '{choice.Label}' is visible. Use the large room option instead of inspect affordances.";
    }

    private static bool LooksLikeShopState(ObserverSummary observer)
    {
        return ShopObserverSignals.IsShopAuthorityActive(observer);
    }

    private static GuiSmokeStepDecision CreateClickDecisionFromNode(GuiSmokeStepRequest request, ObserverActionNode node, string targetLabel)
    {
        if (!TryParseNodeBounds(node.ScreenBounds, out var bounds))
        {
            throw new InvalidOperationException($"Action node '{node.Label}' does not include screen bounds.");
        }
        if (!TryResolveNormalizedBounds(request.WindowBounds, node.ScreenBounds, bounds, out var normalizedX, out var normalizedY, out var boundsSource))
        {
            throw new InvalidOperationException($"Action node '{node.Label}' uses stale or off-window bounds '{node.ScreenBounds}'.");
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            targetLabel,
            $"Auto provider selected node '{node.Label}' using {boundsSource} bounds.",
            0.92,
            null,
            1200,
            true,
                null);
    }

    private static GuiSmokeStepDecision CreateClickDecisionFromNode(
        GuiSmokeStepRequest request,
        ObserverActionNode node,
        string targetLabel,
        string reason,
        double confidence,
        string? expectedScreen,
        int waitMs)
    {
        var decision = CreateClickDecisionFromNode(request, node, targetLabel);
        return decision with
        {
            Reason = reason,
            Confidence = confidence,
            ExpectedScreen = expectedScreen,
            WaitMs = waitMs,
        };
    }

    private static GuiSmokeStepDecision CreateCombatEnemyTargetDecisionFromNode(
        GuiSmokeStepRequest request,
        ObserverActionNode node,
        string targetLabel,
        int retryCount)
    {
        if (!TryParseNodeBounds(node.ScreenBounds, out var bounds))
        {
            throw new InvalidOperationException($"Enemy target node '{node.Label}' does not include screen bounds.");
        }

        var anchor = retryCount switch
        {
            <= 0 => (X: 0.50f, Y: 0.52f, Suffix: "body"),
            1 => (X: 0.50f, Y: 0.40f, Suffix: "upper-body"),
            2 => (X: 0.62f, Y: 0.48f, Suffix: "right-body"),
            _ => (X: 0.38f, Y: 0.48f, Suffix: "left-body"),
        };
        if (!TryResolveNormalizedPointFromBounds(request.WindowBounds, node.ScreenBounds, bounds, anchor.X, anchor.Y, out var normalizedX, out var normalizedY, out var boundsSource))
        {
            throw new InvalidOperationException($"Enemy target node '{node.Label}' uses stale or off-window bounds '{node.ScreenBounds}'.");
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            targetLabel,
            $"Auto provider selected enemy target '{node.Label}' using {boundsSource} bounds at {anchor.Suffix}.",
            retryCount == 0 ? 0.94 : 0.90,
            "combat",
            300,
            true,
            null);
    }

    private static GuiSmokeStepDecision CreateCombatEndTurnDecisionFromNode(
        GuiSmokeStepRequest request,
        ObserverActionNode node,
        string targetLabel)
    {
        var decision = CreateClickDecisionFromNode(request, node, targetLabel);
        return decision with
        {
            ExpectedScreen = "combat",
            WaitMs = 200,
        };
    }

    private static GuiSmokeStepDecision CreateCombatPressKeyDecision(
        string keyText,
        string targetLabel,
        string reason,
        double confidence,
        int waitMs)
    {
        return new GuiSmokeStepDecision(
            "act",
            "press-key",
            keyText,
            null,
            null,
            targetLabel,
            reason,
            confidence,
            "combat",
            waitMs,
            true,
            null);
    }

    private static GuiSmokeStepDecision CreateNonCombatPressKeyDecision(
        string keyText,
        string targetLabel,
        string reason,
        double confidence,
        string expectedScreen,
        int waitMs)
    {
        return new GuiSmokeStepDecision(
            "act",
            "press-key",
            keyText,
            null,
            null,
            targetLabel,
            reason,
            confidence,
            expectedScreen,
            waitMs,
            true,
            null);
    }

    private static string BuildCombatEnemyTargetLabel(ObserverActionNode node, int retryCount)
    {
        var baseLabel = string.IsNullOrWhiteSpace(node.Label)
            ? "combat enemy target"
            : $"combat enemy target {node.Label.Trim()}";
        return retryCount switch
        {
            <= 0 => baseLabel,
            1 => $"{baseLabel} recenter",
            2 => $"{baseLabel} alternate",
            _ => $"{baseLabel} fallback",
        };
    }

    private static GuiSmokeStepDecision CreateClickDecisionFromChoice(
        GuiSmokeStepRequest request,
        ObserverChoice choice,
        string targetLabel,
        string reason,
        double confidence,
        string expectedScreen,
        int waitMs)
    {
        if (!TryParseNodeBounds(choice.ScreenBounds, out var bounds))
        {
            throw new InvalidOperationException($"Observer choice '{choice.Label}' does not include screen bounds.");
        }
        if (!TryResolveNormalizedBounds(request.WindowBounds, choice.ScreenBounds, bounds, out var normalizedX, out var normalizedY, out _))
        {
            throw new InvalidOperationException($"Observer choice '{choice.Label}' uses stale or off-window bounds '{choice.ScreenBounds}'.");
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            targetLabel,
            reason,
            confidence,
            expectedScreen,
            waitMs,
            true,
            null);
    }

    private static bool HasUsableLogicalBounds(string? raw)
    {
        if (!TryParseNodeBounds(raw, out var bounds))
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

    private static bool TryResolveNormalizedBounds(
        WindowBounds windowBounds,
        string? rawBounds,
        RectangleF bounds,
        out double normalizedX,
        out double normalizedY,
        out string boundsSource)
    {
        return TryResolveNormalizedPointFromBounds(windowBounds, rawBounds, bounds, 0.5f, 0.5f, out normalizedX, out normalizedY, out boundsSource);
    }

    private static bool TryResolveNormalizedPointFromBounds(
        WindowBounds windowBounds,
        string? rawBounds,
        RectangleF bounds,
        float anchorX,
        float anchorY,
        out double normalizedX,
        out double normalizedY,
        out string boundsSource)
    {
        normalizedX = default;
        normalizedY = default;
        boundsSource = "unknown";

        var pointX = bounds.X + bounds.Width * anchorX;
        var pointY = bounds.Y + bounds.Height * anchorY;
        if (HasUsableLogicalBounds(rawBounds))
        {
            normalizedX = Math.Clamp(pointX / 1920f, 0d, 1d);
            normalizedY = Math.Clamp(pointY / 1080f, 0d, 1d);
            boundsSource = "logical";
            return true;
        }

        if (!IsBoundsInsideWindow(rawBounds, windowBounds))
        {
            return false;
        }

        normalizedX = Math.Clamp((pointX - windowBounds.X) / Math.Max(1d, windowBounds.Width), 0d, 1d);
        normalizedY = Math.Clamp((pointY - windowBounds.Y) / Math.Max(1d, windowBounds.Height), 0d, 1d);
        boundsSource = "window";
        return true;
    }

    private static GuiSmokeStepDecision CreateWaitDecision(string reason, string? expectedScreen)
    {
        return CreateWaitDecision(reason, expectedScreen, 2000);
    }

    private static GuiSmokeStepDecision CreateWaitDecision(string reason, string? expectedScreen, int waitMs)
    {
        return new GuiSmokeStepDecision(
            "wait",
            null,
            null,
            null,
            null,
            null,
            reason,
            0.60,
            expectedScreen,
            waitMs,
            true,
            null);
    }

    private static GuiSmokeStepDecision CreateForegroundAwareNonCombatWaitDecision(
        GuiSmokeStepRequest request,
        string reason,
        bool allowFastForegroundWait = true)
    {
        var waitMs = allowFastForegroundWait && ShouldUseFastNonCombatForegroundWait(request)
            ? 400
            : 2000;
        return CreateWaitDecision(reason, request.Observer.CurrentScreen, waitMs);
    }

    private static bool ShouldUseFastNonCombatForegroundWait(GuiSmokeStepRequest request)
    {
        if (request.Observer.SceneReady == false
            || !string.Equals(request.Observer.SceneStability, "stable", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (RootSceneTransitionObserverSignals.TryGetState(request.Observer)?.TransitionInProgress == true)
        {
            return false;
        }

        if (request.Phase is not nameof(GuiSmokePhase.HandleEvent)
            and not nameof(GuiSmokePhase.ChooseFirstNode)
            and not nameof(GuiSmokePhase.HandleRewards)
            and not nameof(GuiSmokePhase.HandleShop))
        {
            return false;
        }

        if (TryBuildCanonicalNonCombatSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath) is { } canonicalScene)
        {
            return canonicalScene.AllowsFastForegroundWait;
        }

        return NonCombatForegroundOwnership.Resolve(request.Observer) == NonCombatForegroundOwner.Map;
    }

    public static string? BuildHistoryMetadataForDecision(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        return AutoDecisionProvider.BuildRestSiteHistoryMetadataForDecision(request, decision)
               ?? CombatBarrierSupport.BuildHistoryMetadataForDecision(request, decision);
    }

    private static GuiSmokeStepDecision CreatePhaseWaitDecision(GuiSmokePhase phase, string reason, string? expectedScreen)
    {
        var waitMs = phase == GuiSmokePhase.HandleCombat ? 500 : 2000;
        return new GuiSmokeStepDecision(
            "wait",
            null,
            null,
            null,
            null,
            null,
            reason,
            0.60,
            expectedScreen,
            waitMs,
            true,
            null);
    }

    public static GuiSmokeStepDecision CreateCombatBarrierWaitDecision(CombatBarrierEvaluation barrier, string? expectedScreen)
    {
        return new GuiSmokeStepDecision(
            "wait",
            null,
            null,
            null,
            null,
            null,
            $"combat barrier wait barrier={barrier.Kind} source={barrier.SourceAction ?? "null"} reason={barrier.Reason}",
            0.66,
            expectedScreen,
            CombatBarrierPolicy.HandleCombatWaitMinimumMs,
            true,
            null);
    }

    private static bool TryParseNodeBounds(string? raw, out RectangleF bounds)
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
}

sealed class HeadlessCodexDecisionProvider : IGuiDecisionProvider
{
    private readonly IReadOnlyDictionary<string, string> _options;

    public HeadlessCodexDecisionProvider(IReadOnlyDictionary<string, string> options)
    {
        _options = options;
    }

    public async Task<GuiSmokeStepDecision> GetDecisionAsync(string requestPath, string decisionPath, TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (!_options.TryGetValue("--provider-command", out var providerCommand) || string.IsNullOrWhiteSpace(providerCommand))
        {
            throw new InvalidOperationException("--provider-command is required for --provider headless.");
        }

        var expanded = providerCommand
            .Replace("{request}", requestPath, StringComparison.OrdinalIgnoreCase)
            .Replace("{decision}", decisionPath, StringComparison.OrdinalIgnoreCase);
        await GuiSmokeShared.RunProcessAsync(
            Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
            $"/d /c {expanded}",
            Directory.GetCurrentDirectory(),
            timeout).ConfigureAwait(false);

        if (!File.Exists(decisionPath))
        {
            throw new FileNotFoundException("Headless provider did not create a decision file.", decisionPath);
        }

        using var stream = new FileStream(decisionPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var parsed = await JsonSerializer.DeserializeAsync<GuiSmokeStepDecision>(stream, GuiSmokeShared.JsonOptions, cancellationToken).ConfigureAwait(false);
        return parsed ?? throw new InvalidOperationException("Failed to parse decision file.");
    }
}

sealed class AutoMapAnalysis
{
    public static readonly AutoMapAnalysis None = new(false, 0.5f, 0.5f, false, 0.5f, 0.5f);

    public AutoMapAnalysis(
        bool hasCurrentArrow,
        float arrowNormalizedX,
        float arrowNormalizedY,
        bool hasReachableNode,
        float reachableNodeNormalizedX,
        float reachableNodeNormalizedY)
    {
        HasCurrentArrow = hasCurrentArrow;
        ArrowNormalizedX = arrowNormalizedX;
        ArrowNormalizedY = arrowNormalizedY;
        HasReachableNode = hasReachableNode;
        ReachableNodeNormalizedX = reachableNodeNormalizedX;
        ReachableNodeNormalizedY = reachableNodeNormalizedY;
    }

    public bool HasCurrentArrow { get; }

    public float ArrowNormalizedX { get; }

    public float ArrowNormalizedY { get; }

    public bool HasReachableNode { get; }

    public float ReachableNodeNormalizedX { get; }

    public float ReachableNodeNormalizedY { get; }
}

sealed record AutoTreasureAnalysis(
    bool HasClosedChestHighlight,
    bool HasRewardIcon,
    double RewardIconNormalizedX,
    double RewardIconNormalizedY);

sealed record AutoRestSiteCardGridAnalysis(
    bool HasSelectableCard,
    double CardNormalizedX,
    double CardNormalizedY);

sealed record AutoEventCardGridAnalysis(
    bool HasSelectableCard,
    double CardNormalizedX,
    double CardNormalizedY);

sealed record AutoRewardRowAnalysis(
    bool HasSelectableRewardRow,
    double RowNormalizedX,
    double RowNormalizedY);

static class AutoRestSiteCardGridAnalyzer
{
    private static readonly AutoRestSiteCardGridAnalysis None = new(false, 0.5d, 0.5d);

    public static AutoRestSiteCardGridAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("rest-site-card-grid", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoRestSiteCardGridAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return None;
        }

        try
        {
            var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(screenshotPath);
            if (!overlayAnalysis.HasBottomLeftBackArrow)
            {
                return None;
            }

            using var bitmap = new Bitmap(screenshotPath);
            var points = new List<Point>();
            for (var y = (int)(bitmap.Height * 0.08); y < (int)(bitmap.Height * 0.95); y += 4)
            {
                for (var x = (int)(bitmap.Width * 0.08); x < (int)(bitmap.Width * 0.92); x += 4)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    var brightness = (pixel.R + pixel.G + pixel.B) / 3.0;
                    var saturation = Math.Max(pixel.R, Math.Max(pixel.G, pixel.B))
                                     - Math.Min(pixel.R, Math.Min(pixel.G, pixel.B));
                    if (brightness >= 70 && saturation >= 28)
                    {
                        points.Add(new Point(x, y));
                    }
                }
            }

            if (points.Count < 600)
            {
                return None;
            }

            var candidates = FindClusters(points, 12, 12)
                .Select(cluster =>
                {
                    var minX = cluster.Min(static point => point.X);
                    var maxX = cluster.Max(static point => point.X);
                    var minY = cluster.Min(static point => point.Y);
                    var maxY = cluster.Max(static point => point.Y);
                    var width = maxX - minX;
                    var height = maxY - minY;
                    var centerX = cluster.Average(static point => point.X);
                    var centerY = cluster.Average(static point => point.Y);
                    return new
                    {
                        cluster,
                        width,
                        height,
                        centerX,
                        centerY,
                        score = Math.Abs(centerX - bitmap.Width / 2d) + Math.Abs(centerY - bitmap.Height / 2d),
                    };
                })
                .Where(entry =>
                    entry.cluster.Count >= 180
                    && entry.width is >= 110 and <= 220
                    && entry.height is >= 170 and <= 280
                    && entry.centerX >= bitmap.Width * 0.18
                    && entry.centerX <= bitmap.Width * 0.82
                    && entry.centerY >= bitmap.Height * 0.16
                    && entry.centerY <= bitmap.Height * 0.82)
                .OrderBy(entry => entry.score)
                .FirstOrDefault();
            if (candidates is null)
            {
                return None;
            }

            return new AutoRestSiteCardGridAnalysis(
                true,
                Math.Clamp(candidates.centerX / bitmap.Width, 0.10d, 0.90d),
                Math.Clamp(candidates.centerY / bitmap.Height, 0.10d, 0.90d));
        }
        catch (ArgumentException)
        {
            return None;
        }
        catch (IOException)
        {
            return None;
        }
    }

    private static List<List<Point>> FindClusters(List<Point> points, int maxDx, int maxDy)
    {
        var remaining = new HashSet<int>(Enumerable.Range(0, points.Count));
        var clusters = new List<List<Point>>();

        while (remaining.Count > 0)
        {
            var seedIndex = remaining.First();
            remaining.Remove(seedIndex);
            var queue = new Queue<int>();
            var cluster = new List<Point>();
            queue.Enqueue(seedIndex);

            while (queue.Count > 0)
            {
                var currentIndex = queue.Dequeue();
                var current = points[currentIndex];
                cluster.Add(current);

                var neighbors = remaining
                    .Where(index =>
                    {
                        var other = points[index];
                        return Math.Abs(other.X - current.X) <= maxDx
                               && Math.Abs(other.Y - current.Y) <= maxDy;
                    })
                    .ToArray();

                foreach (var neighbor in neighbors)
                {
                    remaining.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }
}

static class AutoEventCardGridAnalyzer
{
    private static readonly AutoEventCardGridAnalysis None = new(false, 0.5d, 0.5d);

    public static AutoEventCardGridAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("event-card-grid", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoEventCardGridAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return None;
        }

        try
        {
            using var bitmap = new Bitmap(screenshotPath);
            var points = new List<Point>();
            for (var y = (int)(bitmap.Height * 0.14); y < (int)(bitmap.Height * 0.82); y += 4)
            {
                for (var x = (int)(bitmap.Width * 0.14); x < (int)(bitmap.Width * 0.86); x += 4)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    var brightness = (pixel.R + pixel.G + pixel.B) / 3.0;
                    var saturation = Math.Max(pixel.R, Math.Max(pixel.G, pixel.B))
                                     - Math.Min(pixel.R, Math.Min(pixel.G, pixel.B));
                    if (brightness >= 70 && saturation >= 24)
                    {
                        points.Add(new Point(x, y));
                    }
                }
            }

            if (points.Count < 500)
            {
                return None;
            }

            var candidate = FindClusters(points, 12, 12)
                .Select(cluster =>
                {
                    var minX = cluster.Min(static point => point.X);
                    var maxX = cluster.Max(static point => point.X);
                    var minY = cluster.Min(static point => point.Y);
                    var maxY = cluster.Max(static point => point.Y);
                    var width = maxX - minX;
                    var height = maxY - minY;
                    var centerX = cluster.Average(static point => point.X);
                    var centerY = cluster.Average(static point => point.Y);
                    return new
                    {
                        cluster,
                        width,
                        height,
                        centerX,
                        centerY,
                        score = Math.Abs(centerX - bitmap.Width / 2d) + Math.Abs(centerY - bitmap.Height / 2d),
                    };
                })
                .Where(entry =>
                    entry.cluster.Count >= 160
                    && entry.width is >= 100 and <= 240
                    && entry.height is >= 150 and <= 300
                    && entry.centerX >= bitmap.Width * 0.18
                    && entry.centerX <= bitmap.Width * 0.82
                    && entry.centerY >= bitmap.Height * 0.18
                    && entry.centerY <= bitmap.Height * 0.78)
                .OrderBy(entry => entry.score)
                .FirstOrDefault();
            if (candidate is null)
            {
                return None;
            }

            return new AutoEventCardGridAnalysis(
                true,
                Math.Clamp(candidate.centerX / bitmap.Width, 0.12d, 0.88d),
                Math.Clamp(candidate.centerY / bitmap.Height, 0.12d, 0.82d));
        }
        catch (ArgumentException)
        {
            return None;
        }
        catch (IOException)
        {
            return None;
        }
    }

    private static List<List<Point>> FindClusters(List<Point> points, int maxDx, int maxDy)
    {
        var remaining = new HashSet<int>(Enumerable.Range(0, points.Count));
        var clusters = new List<List<Point>>();

        while (remaining.Count > 0)
        {
            var seedIndex = remaining.First();
            remaining.Remove(seedIndex);
            var queue = new Queue<int>();
            var cluster = new List<Point>();
            queue.Enqueue(seedIndex);

            while (queue.Count > 0)
            {
                var currentIndex = queue.Dequeue();
                var current = points[currentIndex];
                cluster.Add(current);

                var neighbors = remaining
                    .Where(index =>
                    {
                        var other = points[index];
                        return Math.Abs(other.X - current.X) <= maxDx
                               && Math.Abs(other.Y - current.Y) <= maxDy;
                    })
                    .ToArray();

                foreach (var neighbor in neighbors)
                {
                    remaining.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }
}

static class AutoRewardRowAnalyzer
{
    private static readonly AutoRewardRowAnalysis None = new(false, 0.5d, 0.5d);

    public static AutoRewardRowAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("reward-row", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoRewardRowAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return None;
        }

        try
        {
            using var bitmap = new Bitmap(screenshotPath);
            var points = new List<Point>();
            var xStart = (int)(bitmap.Width * 0.32);
            var xEnd = (int)(bitmap.Width * 0.68);
            var yStart = (int)(bitmap.Height * 0.22);
            var yEnd = (int)(bitmap.Height * 0.52);
            for (var y = yStart; y < yEnd; y += 2)
            {
                for (var x = xStart; x < xEnd; x += 2)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    var brightness = (pixel.R + pixel.G + pixel.B) / 3.0;
                    var saturation = Math.Max(pixel.R, Math.Max(pixel.G, pixel.B))
                                     - Math.Min(pixel.R, Math.Min(pixel.G, pixel.B));
                    if (brightness >= 95
                        && saturation >= 45
                        && pixel.G >= 120
                        && pixel.B >= 120
                        && pixel.R <= 150)
                    {
                        points.Add(new Point(x, y));
                    }
                }
            }

            if (points.Count < 180)
            {
                return None;
            }

            var candidate = FindClusters(points, 10, 8)
                .Select(cluster =>
                {
                    var minX = cluster.Min(static point => point.X);
                    var maxX = cluster.Max(static point => point.X);
                    var minY = cluster.Min(static point => point.Y);
                    var maxY = cluster.Max(static point => point.Y);
                    var width = maxX - minX;
                    var height = maxY - minY;
                    var centerX = cluster.Average(static point => point.X);
                    var centerY = cluster.Average(static point => point.Y);
                    return new
                    {
                        cluster,
                        width,
                        height,
                        centerX,
                        centerY,
                        score = Math.Abs(centerX - bitmap.Width / 2d) + Math.Abs(centerY - bitmap.Height * 0.38d),
                    };
                })
                .Where(entry =>
                    entry.cluster.Count >= 120
                    && entry.width is >= 140 and <= 340
                    && entry.height is >= 30 and <= 110
                    && entry.centerX >= bitmap.Width * 0.36
                    && entry.centerX <= bitmap.Width * 0.64
                    && entry.centerY >= bitmap.Height * 0.24
                    && entry.centerY <= bitmap.Height * 0.48)
                .OrderBy(entry => entry.score)
                .FirstOrDefault();
            if (candidate is null)
            {
                return None;
            }

            return new AutoRewardRowAnalysis(
                true,
                Math.Clamp(candidate.centerX / bitmap.Width, 0.20d, 0.80d),
                Math.Clamp(candidate.centerY / bitmap.Height, 0.16d, 0.60d));
        }
        catch (ArgumentException)
        {
            return None;
        }
        catch (IOException)
        {
            return None;
        }
    }

    private static List<List<Point>> FindClusters(List<Point> points, int maxDx, int maxDy)
    {
        var remaining = new HashSet<int>(Enumerable.Range(0, points.Count));
        var clusters = new List<List<Point>>();

        while (remaining.Count > 0)
        {
            var seedIndex = remaining.First();
            remaining.Remove(seedIndex);
            var queue = new Queue<int>();
            var cluster = new List<Point>();
            queue.Enqueue(seedIndex);

            while (queue.Count > 0)
            {
                var currentIndex = queue.Dequeue();
                var current = points[currentIndex];
                cluster.Add(current);

                var neighbors = remaining
                    .Where(index =>
                    {
                        var other = points[index];
                        return Math.Abs(other.X - current.X) <= maxDx
                               && Math.Abs(other.Y - current.Y) <= maxDy;
                    })
                    .ToArray();

                foreach (var neighbor in neighbors)
                {
                    remaining.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }
}

static class AutoTreasureAnalyzer
{
    private static readonly AutoTreasureAnalysis None = new(false, false, 0.5d, 0.5d);

    public static AutoTreasureAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("treasure", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoTreasureAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return None;
        }

        try
        {
            using var bitmap = new Bitmap(screenshotPath);
            var hasClosedChestHighlight = HasClosedChestHighlight(bitmap);
            var points = new List<Point>();
            var xStart = (int)(bitmap.Width * 0.35);
            var xEnd = (int)(bitmap.Width * 0.65);
            var yStart = (int)(bitmap.Height * 0.28);
            var yEnd = (int)(bitmap.Height * 0.62);

            for (var y = yStart; y < yEnd; y += 1)
            {
                for (var x = xStart; x < xEnd; x += 1)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    var brightness = (pixel.R + pixel.G + pixel.B) / 3.0;
                    if (pixel.B >= 120
                        && pixel.B >= pixel.R + 25
                        && pixel.B >= pixel.G + 20
                        && brightness >= 70)
                    {
                        points.Add(new Point(x, y));
                    }
                }
            }

            if (points.Count < 90)
            {
                return new AutoTreasureAnalysis(hasClosedChestHighlight, false, 0.5d, 0.5d);
            }

            var rewardCluster = FindLargestCluster(points, 14, 14);
            if (rewardCluster.Count < 80)
            {
                return new AutoTreasureAnalysis(hasClosedChestHighlight, false, 0.5d, 0.5d);
            }

            var minX = rewardCluster.Min(static point => point.X);
            var maxX = rewardCluster.Max(static point => point.X);
            var minY = rewardCluster.Min(static point => point.Y);
            var maxY = rewardCluster.Max(static point => point.Y);
            var width = maxX - minX;
            var height = maxY - minY;
            var centroidX = rewardCluster.Average(static point => point.X);
            var centroidY = rewardCluster.Average(static point => point.Y);
            var normalizedX = centroidX / Math.Max(1d, bitmap.Width);
            var normalizedY = centroidY / Math.Max(1d, bitmap.Height);

            if (width is < 18 or > 120
                || height is < 18 or > 120
                || normalizedX is < 0.40 or > 0.60
                || normalizedY is < 0.30 or > 0.55)
            {
                return new AutoTreasureAnalysis(hasClosedChestHighlight, false, 0.5d, 0.5d);
            }

            return new AutoTreasureAnalysis(hasClosedChestHighlight, true, normalizedX, normalizedY);
        }
        catch (ArgumentException)
        {
            return None;
        }
        catch (IOException)
        {
            return None;
        }
    }

    private static bool HasClosedChestHighlight(Bitmap bitmap)
    {
        var count = 0;
        var xStart = (int)(bitmap.Width * 0.25);
        var xEnd = (int)(bitmap.Width * 0.78);
        var yStart = (int)(bitmap.Height * 0.28);
        var yEnd = (int)(bitmap.Height * 0.82);

        for (var y = yStart; y < yEnd; y += 1)
        {
            for (var x = xStart; x < xEnd; x += 1)
            {
                var pixel = bitmap.GetPixel(x, y);
                var brightness = (pixel.R + pixel.G + pixel.B) / 3.0;
                if (brightness >= 225
                    && Math.Abs(pixel.R - pixel.G) <= 20
                    && Math.Abs(pixel.G - pixel.B) <= 20
                    && Math.Abs(pixel.R - pixel.B) <= 20)
                {
                    count += 1;
                }
            }
        }

        return count >= 1200;
    }

    private static List<Point> FindLargestCluster(List<Point> points, int maxDx, int maxDy)
    {
        var remaining = new HashSet<int>(Enumerable.Range(0, points.Count));
        var largest = new List<Point>();

        while (remaining.Count > 0)
        {
            var seedIndex = remaining.First();
            remaining.Remove(seedIndex);
            var queue = new Queue<int>();
            var cluster = new List<Point>();
            queue.Enqueue(seedIndex);

            while (queue.Count > 0)
            {
                var currentIndex = queue.Dequeue();
                var current = points[currentIndex];
                cluster.Add(current);

                var neighbors = remaining
                    .Where(index =>
                    {
                        var other = points[index];
                        return Math.Abs(other.X - current.X) <= maxDx
                               && Math.Abs(other.Y - current.Y) <= maxDy;
                    })
                    .ToArray();

                foreach (var neighbor in neighbors)
                {
                    remaining.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            if (cluster.Count > largest.Count)
            {
                largest = cluster;
            }
        }

        return largest;
    }
}

static class AutoMapAnalyzer
{
    public static AutoMapAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("map", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoMapAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return AutoMapAnalysis.None;
        }

        try
        {
            using var bitmap = new Bitmap(screenshotPath);
            var directArrow = TryFindCurrentArrow(bitmap);
            if (directArrow is not null)
            {
                var reachableNode = TryFindReachableNode(bitmap, directArrow.Value.X, directArrow.Value.Y);
                return new AutoMapAnalysis(
                    true,
                    (float)(directArrow.Value.X / bitmap.Width),
                    (float)(directArrow.Value.Y / bitmap.Height),
                    reachableNode is not null,
                    reachableNode is null ? 0.5f : (float)(reachableNode.Value.X / bitmap.Width),
                    reachableNode is null ? 0.5f : (float)(reachableNode.Value.Y / bitmap.Height));
            }

            var samples = new List<Point>();

            var yStart = (int)(bitmap.Height * 0.22);
            var yEnd = (int)(bitmap.Height * 0.90);
            var xStart = (int)(bitmap.Width * 0.20);
            var xEnd = (int)(bitmap.Width * 0.82);

            for (var y = yStart; y < yEnd; y += 2)
            {
                for (var x = xStart; x < xEnd; x += 2)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    if (pixel.R >= 170 && pixel.G <= 110 && pixel.B <= 110 && pixel.R - pixel.G >= 70)
                    {
                        samples.Add(new Point(x, y));
                    }
                }
            }

            if (samples.Count < 12)
            {
                return AutoMapAnalysis.None;
            }

            var bestCluster = FindBestArrowCluster(samples);
            if (bestCluster.Count < 8)
            {
                return AutoMapAnalysis.None;
            }

            var centroidX = bestCluster.Average(static point => point.X);
            var centroidY = bestCluster.Average(static point => point.Y);
            var fallbackReachableNode = TryFindReachableNode(bitmap, centroidX, centroidY);
            return new AutoMapAnalysis(
                true,
                (float)(centroidX / bitmap.Width),
                (float)(centroidY / bitmap.Height),
                fallbackReachableNode is not null,
                fallbackReachableNode is null ? 0.5f : (float)(fallbackReachableNode.Value.X / bitmap.Width),
                fallbackReachableNode is null ? 0.5f : (float)(fallbackReachableNode.Value.Y / bitmap.Height));
        }
        catch (ArgumentException)
        {
            return AutoMapAnalysis.None;
        }
        catch (IOException)
        {
            return AutoMapAnalysis.None;
        }
    }

    private static PointF? TryFindCurrentArrow(Bitmap bitmap)
    {
        var xStart = (int)(bitmap.Width * 0.25);
        var xEnd = (int)(bitmap.Width * 0.70);
        var yStart = (int)(bitmap.Height * 0.25);
        var yEnd = (int)(bitmap.Height * 0.70);
        var samples = new List<Point>();

        for (var y = yStart; y < yEnd; y += 2)
        {
            for (var x = xStart; x < xEnd; x += 2)
            {
                var pixel = bitmap.GetPixel(x, y);
                if (pixel.R >= 185 && pixel.G <= 105 && pixel.B <= 105 && pixel.R - pixel.G >= 80)
                {
                    samples.Add(new Point(x, y));
                }
            }
        }

        if (samples.Count < 8)
        {
            return null;
        }

        var clusters = FindClusters(samples, 16, 16);
        var candidate = clusters
            .Where(cluster => cluster.Count >= 6)
            .Select(cluster =>
            {
                var centroidX = cluster.Average(static point => point.X);
                var centroidY = cluster.Average(static point => point.Y);
                return new
                {
                    cluster,
                    centroidX,
                    centroidY,
                };
            })
            .Where(entry =>
                entry.centroidX >= bitmap.Width * 0.30
                && entry.centroidX <= bitmap.Width * 0.62
                && entry.centroidY >= bitmap.Height * 0.38
                && entry.centroidY <= bitmap.Height * 0.74)
            .OrderByDescending(entry => entry.centroidY)
            .ThenByDescending(entry => entry.cluster.Count)
            .FirstOrDefault();

        if (candidate is not null)
        {
            return new PointF((float)candidate.centroidX, (float)candidate.centroidY);
        }

        return new PointF(
            (float)samples.Average(static point => point.X),
            (float)samples.Average(static point => point.Y));
    }

    private static List<Point> FindBestArrowCluster(List<Point> samples)
    {
        var remaining = new HashSet<int>(Enumerable.Range(0, samples.Count));
        var bestCluster = new List<Point>();

        while (remaining.Count > 0)
        {
            var seedIndex = remaining.First();
            remaining.Remove(seedIndex);
            var queue = new Queue<int>();
            queue.Enqueue(seedIndex);
            var cluster = new List<Point>();

            while (queue.Count > 0)
            {
                var currentIndex = queue.Dequeue();
                var current = samples[currentIndex];
                cluster.Add(current);

                var neighbors = remaining
                    .Where(index =>
                    {
                        var other = samples[index];
                        return Math.Abs(other.X - current.X) <= 18 && Math.Abs(other.Y - current.Y) <= 18;
                    })
                    .ToArray();

                foreach (var neighbor in neighbors)
                {
                    remaining.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            if (cluster.Count > bestCluster.Count)
            {
                bestCluster = cluster;
            }
        }

        return bestCluster;
    }

    private static PointF? TryFindReachableNode(Bitmap bitmap, double currentArrowX, double currentArrowY)
    {
        var xMin = Math.Max(0, (int)(currentArrowX - bitmap.Width * 0.14));
        var xMax = Math.Min(bitmap.Width - 1, (int)(currentArrowX + bitmap.Width * 0.14));
        var yMin = Math.Max(0, (int)(currentArrowY - bitmap.Height * 0.24));
        var yMax = Math.Min(bitmap.Height - 1, (int)(currentArrowY + bitmap.Height * 0.26));
        var samples = new List<Point>();

        for (var y = yMin; y <= yMax; y += 2)
        {
            for (var x = xMin; x <= xMax; x += 2)
            {
                var distance = Math.Sqrt(Math.Pow(x - currentArrowX, 2) + Math.Pow(y - currentArrowY, 2));
                if (distance <= 40)
                {
                    continue;
                }

                var pixel = bitmap.GetPixel(x, y);
                if (pixel.R <= 80 && pixel.G <= 80 && pixel.B <= 80)
                {
                    samples.Add(new Point(x, y));
                }
            }
        }

        if (samples.Count < 24)
        {
            return null;
        }

        var clusters = FindClusters(samples, 18, 18);
        var candidates = clusters
            .Where(cluster => cluster.Count >= 10)
            .Select(cluster =>
            {
                var centroidX = cluster.Average(static point => point.X);
                var centroidY = cluster.Average(static point => point.Y);
                var dx = Math.Abs(centroidX - currentArrowX);
                var dy = Math.Abs(centroidY - currentArrowY);
                var score = dx * 1.5 + dy;
                return new { cluster, centroidX, centroidY, dx, dy, score };
            })
            .Where(entry => entry.dx <= bitmap.Width * 0.10 && entry.dy >= 28 && entry.dy <= bitmap.Height * 0.20)
            .ToArray();

        var candidate = candidates
            .Where(entry => entry.centroidY < currentArrowY - 24)
            .OrderBy(entry => entry.score)
            .FirstOrDefault()
            ?? candidates
                .OrderBy(entry => entry.score)
                .FirstOrDefault();

        return candidate is null ? null : new PointF((float)candidate.centroidX, (float)candidate.centroidY);
    }

    private static List<List<Point>> FindClusters(List<Point> samples, int maxDx, int maxDy)
    {
        var remaining = new HashSet<int>(Enumerable.Range(0, samples.Count));
        var clusters = new List<List<Point>>();

        while (remaining.Count > 0)
        {
            var seedIndex = remaining.First();
            remaining.Remove(seedIndex);
            var queue = new Queue<int>();
            queue.Enqueue(seedIndex);
            var cluster = new List<Point>();

            while (queue.Count > 0)
            {
                var currentIndex = queue.Dequeue();
                var current = samples[currentIndex];
                cluster.Add(current);

                var neighbors = remaining
                    .Where(index =>
                    {
                        var other = samples[index];
                        return Math.Abs(other.X - current.X) <= maxDx && Math.Abs(other.Y - current.Y) <= maxDy;
                    })
                    .ToArray();

                foreach (var neighbor in neighbors)
                {
                    remaining.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }
}

sealed record AutoOverlayUiAnalysis(
    bool HasCentralOverlayPanel,
    bool HasBottomLeftBackArrow,
    bool HasRightProceedArrow,
    double? RightProceedNormalizedX,
    double? RightProceedNormalizedY);

static class AutoOverlayUiAnalyzer
{
    public static AutoOverlayUiAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("overlay-ui", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoOverlayUiAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return new AutoOverlayUiAnalysis(false, false, false, null, null);
        }

        try
        {
            using var bitmap = new Bitmap(screenshotPath);
            var center = AverageColor(bitmap, 0.32, 0.16, 0.68, 0.88);
            var left = AverageColor(bitmap, 0.05, 0.20, 0.25, 0.82);
            var right = AverageColor(bitmap, 0.75, 0.20, 0.95, 0.82);
            var hasCentralOverlayPanel = center.Brightness >= 35
                                         && center.B >= center.R - 10
                                         && center.B >= center.G - 10
                                         && center.Brightness - Math.Max(left.Brightness, right.Brightness) >= 20;
            var hasBottomLeftBackArrow = CountArrowLikePixels(bitmap, 0.00, 0.72, 0.18, 0.98) >= 18;
            var proceedCentroid = TryFindArrowLikeCentroid(bitmap, 0.78, 0.68, 1.00, 0.98);
            var hasRightProceedArrow = proceedCentroid is not null || CountArrowLikePixels(bitmap, 0.78, 0.68, 1.00, 0.98) >= 2;
            return new AutoOverlayUiAnalysis(
                hasCentralOverlayPanel,
                hasBottomLeftBackArrow,
                hasRightProceedArrow,
                proceedCentroid is null ? null : proceedCentroid.Value.X / Math.Max(1f, bitmap.Width),
                proceedCentroid is null ? null : proceedCentroid.Value.Y / Math.Max(1f, bitmap.Height));
        }
        catch (ArgumentException)
        {
            return new AutoOverlayUiAnalysis(false, false, false, null, null);
        }
        catch (IOException)
        {
            return new AutoOverlayUiAnalysis(false, false, false, null, null);
        }
    }

    private static int CountArrowLikePixels(Bitmap bitmap, double left, double top, double right, double bottom)
    {
        var count = 0;
        AutoCombatAnalyzer.ForEachSample(bitmap, left, top, right, bottom, 28, 20, color =>
        {
            if (color.R >= 140 && color.R - color.G >= 35 && color.G >= 35 && color.G <= 185 && color.B <= 120)
            {
                count += 1;
            }
        });

        return count;
    }

    private static PointF? TryFindArrowLikeCentroid(Bitmap bitmap, double left, double top, double right, double bottom)
    {
        var sumX = 0d;
        var sumY = 0d;
        var count = 0;
        AutoCombatAnalyzer.ForEachSample(bitmap, left, top, right, bottom, 40, 28, (color, x, y) =>
        {
            if (color.R >= 140 && color.R - color.G >= 35 && color.G >= 35 && color.G <= 185 && color.B <= 120)
            {
                sumX += x;
                sumY += y;
                count += 1;
            }
        });

        if (count < 4)
        {
            return null;
        }

        return new PointF((float)(sumX / count), (float)(sumY / count));
    }

    private static (double R, double G, double B, double Brightness) AverageColor(Bitmap bitmap, double left, double top, double right, double bottom)
    {
        var totalR = 0d;
        var totalG = 0d;
        var totalB = 0d;
        var total = 0;
        AutoCombatAnalyzer.ForEachSample(bitmap, left, top, right, bottom, 18, 18, color =>
        {
            totalR += color.R;
            totalG += color.G;
            totalB += color.B;
            total += 1;
        });

        if (total == 0)
        {
            return (0, 0, 0, 0);
        }

        var averageR = totalR / total;
        var averageG = totalG / total;
        var averageB = totalB / total;
        return (averageR, averageG, averageB, (averageR + averageG + averageB) / 3.0);
    }
}

enum AutoCombatCardKind
{
    Unknown,
    AttackLike,
    DefendLike,
}

enum AutoCombatOverlayBand
{
    None,
    Left,
    Center,
    Right,
}

sealed record AutoCombatAnalysis(
    bool HasSelectedCard,
    AutoCombatOverlayBand SelectedOverlayBand,
    bool HasTargetArrow,
    bool HasSelfTargetBrackets,
    AutoCombatCardKind SelectedCardKind);

sealed record AutoCombatHandSlotAnalysis(
    int SlotIndex,
    bool IsVisible,
    AutoCombatCardKind Kind,
    double RedBlueDelta,
    double Brightness,
    double CenterX,
    double CenterY);

sealed record AutoCombatHandAnalysis(
    IReadOnlyList<AutoCombatHandSlotAnalysis> Slots)
{
    public AutoCombatHandSlotAnalysis? TryGetSlot(int slotIndex)
    {
        return Slots.FirstOrDefault(slot => slot.SlotIndex == slotIndex);
    }
}

sealed record PendingCombatSelection(
    int SlotIndex,
    AutoCombatCardKind Kind);

sealed record CombatNoOpLoopAnalysis(
    bool LoopDetected,
    int? BlockedSlotIndex,
    int RepeatedSelectionCount);

sealed record ReconstructedHandleCombatContext(
    IReadOnlyList<GuiSmokeHistoryEntry> CombatHistory,
    PendingCombatSelection? PendingSelection,
    IReadOnlyDictionary<int, int> CombatNoOpCountsBySlot,
    CombatNoOpLoopAnalysis CombatNoOpLoop,
    bool RepeatedNonEnemyLoop,
    bool RepeatedAttackSelectionLoop);

static class HandleCombatContextSupport
{
    public const int SerializedHistoryWindow = 12;

    public static IReadOnlyList<GuiSmokeHistoryEntry> BuildSerializedHistoryWindow(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return history
            .Where(entry => string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            .TakeLast(SerializedHistoryWindow)
            .ToArray();
    }

    public static ReconstructedHandleCombatContext Reconstruct(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var combatHistory = BuildSerializedHistoryWindow(history);
        var pendingSelection = CombatHistorySupport.TryGetPendingCombatSelection(combatHistory);
        var combatNoOpCountsBySlot = CombatHistorySupport.GetCombatNoOpCountsBySlot(combatHistory);
        return new ReconstructedHandleCombatContext(
            combatHistory,
            pendingSelection,
            combatNoOpCountsBySlot,
            AnalyzeCombatNoOpLoop(combatHistory, combatNoOpCountsBySlot),
            HasRecentRepeatedNonEnemyLoop(combatHistory),
            HasRecentRepeatedAttackSelectionLoop(combatHistory));
    }

    public static bool HasRecentNonEnemySelection(ReconstructedHandleCombatContext context, int slotIndex)
    {
        return CombatHistorySupport.HasRecentNonEnemySelection(context.CombatHistory, slotIndex);
    }

    public static int GetCombatNoOpCountForSlot(ReconstructedHandleCombatContext context, int slotIndex)
    {
        return context.CombatNoOpCountsBySlot.TryGetValue(slotIndex, out var count)
            ? count
            : 0;
    }

    private static bool HasRecentRepeatedNonEnemyLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var labels = history
            .Select(static entry => entry.TargetLabel)
            .Where(static label =>
                !string.IsNullOrWhiteSpace(label)
                && (IsNonEnemySelectionLabel(label)
                    || string.Equals(label, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase)))
            .TakeLast(6)
            .ToArray();
        if (labels.Length < 4)
        {
            return false;
        }

        if (labels.Length >= 4
            && IsNonEnemySelectionLabel(labels[0])
            && string.Equals(labels[1], "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase)
            && IsNonEnemySelectionLabel(labels[2])
            && string.Equals(labels[3], "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var recentSelectCount = labels.Count(IsNonEnemySelectionLabel);
        if (recentSelectCount >= 3)
        {
            var distinctSelectionLabels = labels
                .Where(IsNonEnemySelectionLabel)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
            if (distinctSelectionLabels == 1)
            {
                return true;
            }
        }

        if (labels.Length >= 5)
        {
            var trailingWindow = labels.TakeLast(5).ToArray();
            var allowedMixedLoop = trailingWindow.All(label =>
                IsNonEnemySelectionLabel(label)
                || string.Equals(label, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase));
            if (allowedMixedLoop && trailingWindow.Count(IsNonEnemySelectionLabel) >= 3)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasRecentRepeatedAttackSelectionLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var labels = history
            .Select(static entry => entry.TargetLabel)
            .Where(static label => !string.IsNullOrWhiteSpace(label))
            .TakeLast(6)
            .ToArray();
        if (labels.Length < 3)
        {
            return false;
        }

        var attackSelections = labels
            .Where(static label => label is not null && label.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase))
            .Select(static label => label!)
            .TakeLast(4)
            .ToArray();
        if (attackSelections.Length < 3)
        {
            return false;
        }

        var distinctAttackSelections = attackSelections
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (distinctAttackSelections > 1)
        {
            return true;
        }

        return attackSelections.Length >= 3;
    }

    private static CombatNoOpLoopAnalysis AnalyzeCombatNoOpLoop(
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        IReadOnlyDictionary<int, int> recentNoOpCounts)
    {
        if (history.Count < 4 || recentNoOpCounts.Count == 0)
        {
            return new CombatNoOpLoopAnalysis(false, null, 0);
        }

        var mostRecentBlockedSlot = history
            .Reverse()
            .Select(entry => CombatHistorySupport.ExtractCombatLaneSlotIndex(entry.TargetLabel))
            .FirstOrDefault(static slotIndex => slotIndex.HasValue);
        if (!mostRecentBlockedSlot.HasValue)
        {
            mostRecentBlockedSlot = recentNoOpCounts
                .OrderByDescending(static pair => pair.Value)
                .ThenBy(static pair => pair.Key)
                .Select(static pair => (int?)pair.Key)
                .FirstOrDefault();
        }

        if (!mostRecentBlockedSlot.HasValue)
        {
            return new CombatNoOpLoopAnalysis(false, null, 0);
        }

        var recentEnemyTargetCount = history.Count(entry =>
            string.Equals(entry.TargetLabel, "auto-target enemy", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entry.TargetLabel, "auto-target enemy recenter", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entry.TargetLabel, "auto-target enemy alternate", StringComparison.OrdinalIgnoreCase)
            || (entry.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) ?? false));
        var repeatedSameSlotCount = recentNoOpCounts.TryGetValue(mostRecentBlockedSlot.Value, out var blockedCount)
            ? blockedCount
            : 0;
        var loopDetected = repeatedSameSlotCount >= 2 && recentEnemyTargetCount >= 2;
        return new CombatNoOpLoopAnalysis(loopDetected, mostRecentBlockedSlot, repeatedSameSlotCount);
    }

    private static bool IsNonEnemySelectionLabel(string? targetLabel)
    {
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return false;
        }

        return targetLabel.StartsWith("combat select non-enemy slot ", StringComparison.OrdinalIgnoreCase)
               || targetLabel.StartsWith("combat select defend slot ", StringComparison.OrdinalIgnoreCase);
    }
}

enum CombatBarrierKind
{
    None,
    AttackSelect,
    EnemyClick,
    NonEnemySelect,
    CancelSelection,
    EndTurn,
}

sealed record CombatBarrierEvaluation(
    bool IsActive,
    CombatBarrierKind Kind,
    string Reason,
    string? SourceAction,
    int? SourceSlotIndex,
    long? ArmedSnapshotVersion,
    DateTimeOffset? ArmedCapturedAt,
    bool ReleaseSatisfied,
    bool OverWaitRisk)
{
    public bool IsHardWaitBarrier => Kind is CombatBarrierKind.EnemyClick or CombatBarrierKind.CancelSelection or CombatBarrierKind.EndTurn;
}

sealed record CombatBarrierHistoryMetadata(
    long? SnapshotVersion,
    DateTimeOffset? CapturedAt,
    string? ScreenEpisodeId,
    string? InteractionRevision,
    int? HistoryStartedCount,
    int? HistoryFinishedCount,
    string? LastFinishedCardId)
{
    public int? RoundNumber { get; init; }

    public bool? PlayerActionsDisabled { get; init; }

    public bool? EndingPlayerTurnPhaseOne { get; init; }

    public bool? EndingPlayerTurnPhaseTwo { get; init; }
}

static class CombatBarrierSupport
{
    private sealed record BarrierSource(
        CombatBarrierKind Kind,
        GuiSmokeHistoryEntry Entry,
        int? SlotIndex,
        CombatBarrierHistoryMetadata? Metadata);

    public static CombatBarrierEvaluation Evaluate(
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        ObserverState observer,
        ReconstructedHandleCombatContext combatContext,
        CombatRuntimeState runtime,
        AutoCombatAnalysis analysis,
        bool hasSelectedNonEnemyConfirmEvidence,
        bool canResolveCombatEnemyTarget,
        bool combatPlayerActionWindowClosed)
    {
        var source = TryFindSource(history);
        if (source is null)
        {
            return Inactive;
        }

        var freshSnapshotSeen = HasFreshSnapshotSinceSource(observer, source.Metadata, source.Entry);
        return source.Kind switch
        {
            CombatBarrierKind.AttackSelect => EvaluateAttackSelectBarrier(
                source,
                freshSnapshotSeen,
                runtime,
                analysis,
                canResolveCombatEnemyTarget),
            CombatBarrierKind.EnemyClick => EvaluateEnemyClickBarrier(
                source,
                observer,
                freshSnapshotSeen,
                runtime,
                canResolveCombatEnemyTarget),
            CombatBarrierKind.NonEnemySelect => EvaluateNonEnemySelectBarrier(
                source,
                freshSnapshotSeen,
                runtime,
                hasSelectedNonEnemyConfirmEvidence),
            CombatBarrierKind.CancelSelection => EvaluateCancelSelectionBarrier(
                source,
                freshSnapshotSeen,
                runtime,
                hasSelectedNonEnemyConfirmEvidence),
            CombatBarrierKind.EndTurn => EvaluateEndTurnBarrier(
                source,
                observer,
                freshSnapshotSeen,
                runtime,
                combatPlayerActionWindowClosed),
            _ => Inactive,
        };
    }

    public static bool SuppressesAttackSlot(CombatBarrierEvaluation barrier, int slotIndex)
    {
        return barrier.IsActive
               && barrier.Kind == CombatBarrierKind.AttackSelect
               && barrier.SourceSlotIndex == slotIndex;
    }

    public static bool SuppressesNonEnemySlot(CombatBarrierEvaluation barrier, int slotIndex)
    {
        return barrier.IsActive
               && barrier.Kind == CombatBarrierKind.NonEnemySelect
               && barrier.SourceSlotIndex == slotIndex;
    }

    public static string? BuildHistoryMetadataForDecision(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        if (!string.Equals(request.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
            || !string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!TryResolveBarrierKind(decision.TargetLabel, decision.ActionKind, out _))
        {
            return null;
        }

        var runtime = CombatRuntimeStateSupport.Read(request.Observer, request.CombatCardKnowledge);
        return JsonSerializer.Serialize(
            new CombatBarrierHistoryMetadata(
                request.Observer.SnapshotVersion,
                request.Observer.CapturedAt,
                request.Observer.SceneEpisodeId,
                runtime.InteractionRevision,
                runtime.HistoryStartedCount,
                runtime.HistoryFinishedCount,
                runtime.LastCardPlayFinishedCardId)
            {
                RoundNumber = runtime.RoundNumber,
                PlayerActionsDisabled = runtime.PlayerActionsDisabled,
                EndingPlayerTurnPhaseOne = runtime.EndingPlayerTurnPhaseOne,
                EndingPlayerTurnPhaseTwo = runtime.EndingPlayerTurnPhaseTwo,
            },
            GuiSmokeShared.JsonOptions);
    }

    public static bool TryClassifyWaitPlateau(
        GuiSmokeStepRequest request,
        GuiSmokeStepAnalysisContext context,
        int consecutiveDecisionWaitCount,
        out string terminalCause,
        out string message)
    {
        var barrier = context.CombatBarrierEvaluation;
        if (!barrier.IsActive
            || !barrier.IsHardWaitBarrier
            || !barrier.OverWaitRisk
            || consecutiveDecisionWaitCount < CombatBarrierPolicy.HandleCombatWaitPlateauLimit)
        {
            terminalCause = string.Empty;
            message = string.Empty;
            return false;
        }

        terminalCause = "combat-barrier-wait-plateau";
        message = $"combat-barrier-wait-plateau phase=HandleCombat barrier={barrier.Kind} source={barrier.SourceAction ?? "null"} waits={consecutiveDecisionWaitCount} armedSnapshot={barrier.ArmedSnapshotVersion?.ToString(CultureInfo.InvariantCulture) ?? "none"} currentSnapshot={request.Observer.SnapshotVersion?.ToString(CultureInfo.InvariantCulture) ?? "none"} inventory={request.Observer.InventoryId ?? "null"}";
        return true;
    }

    private static CombatBarrierEvaluation EvaluateAttackSelectBarrier(
        BarrierSource source,
        bool freshSnapshotSeen,
        CombatRuntimeState runtime,
        AutoCombatAnalysis analysis,
        bool canResolveCombatEnemyTarget)
    {
        if (freshSnapshotSeen
            && (canResolveCombatEnemyTarget || analysis.HasTargetArrow))
        {
            return Released(source, "enemy-target authority surfaced after attack selection");
        }

        if (freshSnapshotSeen
            && runtime.ExplicitlyClearedSelection
            && runtime.PendingSelection is null
            && !runtime.HasCardSelectionEvidence)
        {
            return Released(source, "attack selection cleared without pending target authority");
        }

        return Active(
            source,
            freshSnapshotSeen
                ? "attack selection is still awaiting targetable enemy authority or explicit clear"
                : "waiting for a fresh post-attack-select snapshot",
            false,
            freshSnapshotSeen);
    }

    private static CombatBarrierEvaluation EvaluateEnemyClickBarrier(
        BarrierSource source,
        ObserverState observer,
        bool freshSnapshotSeen,
        CombatRuntimeState runtime,
        bool canResolveCombatEnemyTarget)
    {
        if (!freshSnapshotSeen)
        {
            return Active(source, "waiting for a fresh post-enemy-click snapshot", false, true);
        }

        if (canResolveCombatEnemyTarget)
        {
            return Released(source, "targeting authority is still alive after the click");
        }

        var finishedCountAdvanced = source.Metadata?.HistoryFinishedCount is not null
                                    && runtime.HistoryFinishedCount is not null
                                    && runtime.HistoryFinishedCount > source.Metadata.HistoryFinishedCount;
        var finishedCardChanged = !string.IsNullOrWhiteSpace(runtime.LastCardPlayFinishedCardId)
                                  && !string.Equals(runtime.LastCardPlayFinishedCardId, source.Metadata?.LastFinishedCardId, StringComparison.OrdinalIgnoreCase);
        var explicitClear = runtime.ExplicitlyClearedSelection
                            && !runtime.HasExplicitTargetableEnemy
                            && !runtime.HasExplicitHittableEnemy
                            && runtime.TargetingInProgress != true;
        if (finishedCountAdvanced || finishedCardChanged || explicitClear)
        {
            return Released(source, "enemy click resolved into finish or clear evidence");
        }

        return Active(
            source,
            $"enemy click is still awaiting finish/clear evidence screen={observer.CurrentScreen ?? observer.VisibleScreen ?? "null"}",
            false,
            true);
    }

    private static CombatBarrierEvaluation EvaluateNonEnemySelectBarrier(
        BarrierSource source,
        bool freshSnapshotSeen,
        CombatRuntimeState runtime,
        bool hasSelectedNonEnemyConfirmEvidence)
    {
        if (freshSnapshotSeen && hasSelectedNonEnemyConfirmEvidence)
        {
            return Released(source, "non-enemy confirm evidence surfaced");
        }

        if (freshSnapshotSeen
            && runtime.ExplicitlyClearedSelection
            && runtime.PendingSelection is null
            && !runtime.HasCardSelectionEvidence
            && !hasSelectedNonEnemyConfirmEvidence)
        {
            return Released(source, "non-enemy selection cleared without confirm evidence");
        }

        return Active(
            source,
            freshSnapshotSeen
                ? "non-enemy selection is still awaiting confirm evidence or explicit clear"
                : "waiting for a fresh post-non-enemy-select snapshot",
            false,
            freshSnapshotSeen);
    }

    private static CombatBarrierEvaluation EvaluateCancelSelectionBarrier(
        BarrierSource source,
        bool freshSnapshotSeen,
        CombatRuntimeState runtime,
        bool hasSelectedNonEnemyConfirmEvidence)
    {
        if (freshSnapshotSeen
            && !runtime.HasCardSelectionEvidence
            && runtime.PendingSelection is null
            && !hasSelectedNonEnemyConfirmEvidence)
        {
            return Released(source, "selection cancel completed");
        }

        return Active(
            source,
            freshSnapshotSeen
                ? "cancel selection is still awaiting a cleared combat snapshot"
                : "waiting for a fresh post-cancel snapshot",
            false,
            freshSnapshotSeen);
    }

    private static CombatBarrierEvaluation EvaluateEndTurnBarrier(
        BarrierSource source,
        ObserverState observer,
        bool freshSnapshotSeen,
        CombatRuntimeState runtime,
        bool combatPlayerActionWindowClosed)
    {
        if (observer.InCombat != true
            || !string.Equals(observer.CurrentScreen ?? observer.VisibleScreen, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return Released(source, "combat exited after end turn");
        }

        var reopenedPlayerWindow = IsReopenedPlayerActionWindow(observer.Summary, runtime);
        var roundAdvanced = runtime.RoundNumber is not null
                            && source.Metadata?.RoundNumber is not null
                            && runtime.RoundNumber > source.Metadata.RoundNumber;
        if (roundAdvanced && reopenedPlayerWindow)
        {
            return Released(
                source,
                $"next player turn reopened after round advanced from {source.Metadata!.RoundNumber!.Value.ToString(CultureInfo.InvariantCulture)} to {runtime.RoundNumber!.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (!freshSnapshotSeen)
        {
            return Active(source, "waiting for a fresh post-end-turn snapshot", false, true);
        }

        if (HasEndTurnTransitionAcknowledgement(observer.Summary, runtime, combatPlayerActionWindowClosed))
        {
            return Active(source, "end turn acknowledged; waiting for the next round reopen", false, false);
        }

        return Active(
            source,
            "end turn has not yet been acknowledged by combat turn transition",
            false,
            true);
    }

    private static bool HasEndTurnTransitionAcknowledgement(
        ObserverSummary observer,
        CombatRuntimeState runtime,
        bool combatPlayerActionWindowClosed)
    {
        return combatPlayerActionWindowClosed
               || runtime.PlayerActionsDisabled == true
               || runtime.EndingPlayerTurnPhaseOne == true
               || runtime.EndingPlayerTurnPhaseTwo == true
               || CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsEnemyTurnStarted", "CombatManager.IsEnemyTurnStarted") == true
               || CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsPlayPhase", "CombatManager.IsPlayPhase") == false;
    }

    private static bool IsReopenedPlayerActionWindow(ObserverSummary observer, CombatRuntimeState runtime)
    {
        var isPlayPhase = CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsPlayPhase", "CombatManager.IsPlayPhase");
        var isEnemyTurnStarted = CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsEnemyTurnStarted", "CombatManager.IsEnemyTurnStarted");
        return isPlayPhase == true
               && isEnemyTurnStarted == false
               && runtime.PlayerActionsDisabled == false
               && runtime.EndingPlayerTurnPhaseOne == false
               && runtime.EndingPlayerTurnPhaseTwo == false;
    }

    public static string? TryBuildSafeTransitProgressFingerprint(CombatBarrierEvaluation barrier, ObserverSummary observer)
    {
        if (!barrier.IsActive
            || !barrier.IsHardWaitBarrier
            || barrier.Kind != CombatBarrierKind.EndTurn
            || barrier.OverWaitRisk)
        {
            return null;
        }

        return string.Join("|",
            observer.SnapshotVersion?.ToString(CultureInfo.InvariantCulture) ?? "snapshot:none",
            observer.CapturedAt?.ToString("O", CultureInfo.InvariantCulture) ?? "captured:none",
            CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatRoundNumber", "CombatState.RoundNumber")?.ToString(CultureInfo.InvariantCulture) ?? "round:none",
            FormatNullableBool(CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsPlayPhase", "CombatManager.IsPlayPhase")),
            FormatNullableBool(CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsEnemyTurnStarted", "CombatManager.IsEnemyTurnStarted")),
            FormatNullableBool(CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatPlayerActionsDisabled", "CombatManager.PlayerActionsDisabled")),
            FormatNullableBool(CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatEndingPlayerTurnPhaseOne", "CombatManager.EndingPlayerTurnPhaseOne")),
            FormatNullableBool(CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatEndingPlayerTurnPhaseTwo", "CombatManager.EndingPlayerTurnPhaseTwo")));
    }

    private static string FormatNullableBool(bool? value)
    {
        return value switch
        {
            true => "true",
            false => "false",
            _ => "null",
        };
    }

    private static CombatBarrierEvaluation Released(BarrierSource source, string reason)
    {
        return new CombatBarrierEvaluation(
            false,
            source.Kind,
            reason,
            source.Entry.TargetLabel,
            source.SlotIndex,
            source.Metadata?.SnapshotVersion,
            source.Metadata?.CapturedAt ?? source.Entry.RecordedAt,
            true,
            false);
    }

    private static CombatBarrierEvaluation Active(BarrierSource source, string reason, bool releaseSatisfied, bool overWaitRisk)
    {
        return new CombatBarrierEvaluation(
            true,
            source.Kind,
            reason,
            source.Entry.TargetLabel,
            source.SlotIndex,
            source.Metadata?.SnapshotVersion,
            source.Metadata?.CapturedAt ?? source.Entry.RecordedAt,
            releaseSatisfied,
            overWaitRisk);
    }

    private static BarrierSource? TryFindSource(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryResolveBarrierKind(entry.TargetLabel, entry.Action, out var kind))
            {
                var slotIndex = CombatHistorySupport.TryParsePendingCombatSelection(entry.TargetLabel, out var selection)
                    ? selection?.SlotIndex
                    : null;
                return new BarrierSource(kind, entry, slotIndex, TryParseHistoryMetadata(entry.Metadata));
            }

            if (IsMeaningfulCombatAction(entry.Action))
            {
                return null;
            }
        }

        return null;
    }

    private static bool HasFreshSnapshotSinceSource(
        ObserverState observer,
        CombatBarrierHistoryMetadata? metadata,
        GuiSmokeHistoryEntry entry)
    {
        if (metadata?.SnapshotVersion is not null
            && observer.SnapshotVersion is not null
            && observer.SnapshotVersion > metadata.SnapshotVersion)
        {
            return true;
        }

        if (metadata?.CapturedAt is not null
            && observer.CapturedAt is not null
            && observer.CapturedAt > metadata.CapturedAt)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(metadata?.ScreenEpisodeId)
            && !string.Equals(observer.Summary.SceneEpisodeId, metadata.ScreenEpisodeId, StringComparison.Ordinal))
        {
            return true;
        }

        return observer.CapturedAt is not null && observer.CapturedAt > entry.RecordedAt;
    }

    private static CombatBarrierHistoryMetadata? TryParseHistoryMetadata(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
        {
            return null;
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<CombatBarrierHistoryMetadata>(metadata, GuiSmokeShared.JsonOptions);
            return parsed is not null
                   && (parsed.SnapshotVersion is not null
                       || parsed.CapturedAt is not null
                       || !string.IsNullOrWhiteSpace(parsed.ScreenEpisodeId)
                       || !string.IsNullOrWhiteSpace(parsed.InteractionRevision))
                ? parsed
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool TryResolveBarrierKind(string? targetLabel, string? action, out CombatBarrierKind kind)
    {
        if (CombatHistorySupport.TryParsePendingCombatSelection(targetLabel, out var selection))
        {
            kind = selection?.Kind == AutoCombatCardKind.AttackLike
                ? CombatBarrierKind.AttackSelect
                : CombatBarrierKind.NonEnemySelect;
            return kind != CombatBarrierKind.None;
        }

        if (IsCombatEnemyTargetLabel(targetLabel))
        {
            kind = CombatBarrierKind.EnemyClick;
            return true;
        }

        if (string.Equals(action, "right-click", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(targetLabel))
        {
            kind = CombatBarrierKind.CancelSelection;
            return true;
        }

        if (IsCombatEndTurnActionLabel(targetLabel))
        {
            kind = CombatBarrierKind.EndTurn;
            return true;
        }

        kind = CombatBarrierKind.None;
        return false;
    }

    private static bool IsMeaningfulCombatAction(string action)
    {
        return string.Equals(action, "click", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "click-current", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "right-click", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "press-key", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCombatEndTurnActionLabel(string? targetLabel)
    {
        return targetLabel is not null
               && (string.Equals(targetLabel, "click end turn", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.Contains("end turn", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.Contains("auto-end turn", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsCombatEnemyTargetLabel(string? targetLabel)
    {
        return targetLabel is not null
               && (targetLabel.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase));
    }

    public static CombatBarrierEvaluation Inactive { get; } = new(
        false,
        CombatBarrierKind.None,
        string.Empty,
        null,
        null,
        null,
        null,
        false,
        false);
}

static class CombatHistorySupport
{
    public static PendingCombatSelection? TryGetPendingCombatSelection(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return TryGetPendingCombatSelection(history, history.Count);
    }

    public static PendingCombatSelection? TryGetPendingCombatSelection(IReadOnlyList<GuiSmokeHistoryEntry> history, int endExclusive)
    {
        for (var index = Math.Min(history.Count, endExclusive) - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryParsePendingCombatSelection(entry.TargetLabel, out var selection))
            {
                return selection;
            }

            if (IsSelectionClearingEntry(entry))
            {
                return null;
            }

            if (IsNeutralCombatLabel(entry.TargetLabel))
            {
                continue;
            }

            if (IsCombatDecisionAction(entry.Action))
            {
                return null;
            }
        }

        return null;
    }

    public static IReadOnlyDictionary<int, int> GetCombatNoOpCountsBySlot(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var counts = new Dictionary<int, int>();
        for (var index = 0; index < history.Count; index += 1)
        {
            var entry = history[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                || !string.Equals(entry.Action, "combat-noop", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!TryResolveCombatLaneSlotIndex(entry.TargetLabel, history, index + 1, out var slotIndex))
            {
                continue;
            }

            counts[slotIndex] = counts.TryGetValue(slotIndex, out var count)
                ? count + 1
                : 1;
        }

        return counts;
    }

    public static string? ResolveCombatLaneLabel(string? targetLabel, IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return TryResolveCombatLaneSlotIndex(targetLabel, history, history.Count, out var slotIndex)
            ? $"combat lane slot {slotIndex}"
            : targetLabel;
    }

    public static bool HasRecentNonEnemySelection(IReadOnlyList<GuiSmokeHistoryEntry> history, int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            return false;
        }

        return history
            .Where(entry => string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            .TakeLast(8)
            .Any(entry =>
                TryParsePendingCombatSelection(entry.TargetLabel, out var selection)
                && selection is { Kind: AutoCombatCardKind.DefendLike }
                && selection.SlotIndex == slotIndex);
    }

    public static bool TryParsePendingCombatSelection(string? targetLabel, out PendingCombatSelection? selection)
    {
        selection = null;
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return false;
        }

        if (targetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase)
            && ExtractFirstDigit(targetLabel) is { } attackSlot
            && IsValidSlotIndex(attackSlot))
        {
            selection = new PendingCombatSelection(attackSlot, AutoCombatCardKind.AttackLike);
            return true;
        }

        if ((targetLabel.StartsWith("combat select non-enemy slot ", StringComparison.OrdinalIgnoreCase)
             || targetLabel.StartsWith("combat select defend slot ", StringComparison.OrdinalIgnoreCase))
            && ExtractFirstDigit(targetLabel) is { } nonEnemySlot
            && IsValidSlotIndex(nonEnemySlot))
        {
            selection = new PendingCombatSelection(nonEnemySlot, AutoCombatCardKind.DefendLike);
            return true;
        }

        if (targetLabel.StartsWith("auto-select slot ", StringComparison.OrdinalIgnoreCase)
            && ExtractFirstDigit(targetLabel) is { } legacySlot
            && IsValidSlotIndex(legacySlot))
        {
            selection = new PendingCombatSelection(legacySlot, AutoCombatCardKind.AttackLike);
            return true;
        }

        return false;
    }

    public static int? ExtractCombatLaneSlotIndex(string? targetLabel)
    {
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return null;
        }

        if (targetLabel.StartsWith("combat lane slot ", StringComparison.OrdinalIgnoreCase)
            || targetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase))
        {
            return ExtractFirstDigit(targetLabel);
        }

        return null;
    }

    public static bool IsCombatEnemyTargetLabel(string? targetLabel)
    {
        return targetLabel is not null
               && (targetLabel.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsCombatEndTurnLabel(string? targetLabel)
    {
        return !string.IsNullOrWhiteSpace(targetLabel)
               && (targetLabel.Contains("end turn", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.Contains("턴 종료", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsCombatCancelSelectionLabel(string? targetLabel)
    {
        return !string.IsNullOrWhiteSpace(targetLabel)
               && (targetLabel.Contains("cancel", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.Contains("취소", StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryResolveCombatLaneSlotIndex(
        string? targetLabel,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        int endExclusive,
        out int slotIndex)
    {
        slotIndex = default;
        if (ExtractCombatLaneSlotIndex(targetLabel) is { } directSlot
            && IsValidSlotIndex(directSlot))
        {
            slotIndex = directSlot;
            return true;
        }

        if (!IsCombatEnemyTargetLabel(targetLabel))
        {
            return false;
        }

        if (TryGetPendingCombatSelection(history, endExclusive) is { Kind: AutoCombatCardKind.AttackLike } scopedSelection
            && IsValidSlotIndex(scopedSelection.SlotIndex))
        {
            slotIndex = scopedSelection.SlotIndex;
            return true;
        }

        if (TryGetPendingCombatSelection(history) is { Kind: AutoCombatCardKind.AttackLike } recoveredSelection
            && IsValidSlotIndex(recoveredSelection.SlotIndex))
        {
            slotIndex = recoveredSelection.SlotIndex;
            return true;
        }

        return false;
    }

    private static bool IsSelectionClearingEntry(GuiSmokeHistoryEntry entry)
    {
        return IsCombatEndTurnLabel(entry.TargetLabel)
               || IsCombatCancelSelectionLabel(entry.TargetLabel)
               || string.Equals(entry.TargetLabel, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNeutralCombatLabel(string? targetLabel)
    {
        return string.IsNullOrWhiteSpace(targetLabel)
               || targetLabel.StartsWith("combat lane slot ", StringComparison.OrdinalIgnoreCase)
               || IsCombatEnemyTargetLabel(targetLabel)
               || string.Equals(targetLabel, "observer-accepted", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCombatDecisionAction(string action)
    {
        return string.Equals(action, "click", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "click-current", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "right-click", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "press-key", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex is >= 1 and <= 5;
    }

    private static int? ExtractFirstDigit(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        foreach (var character in value)
        {
            if (char.IsDigit(character))
            {
                return character - '0';
            }
        }

        return null;
    }
}

sealed record CombatRuntimeState(
    bool? CardPlayPending,
    string? PlayMode,
    PendingCombatSelection? PendingSelection,
    bool? TargetingInProgress,
    string? ValidTargetsType,
    int? TargetableEnemyCount,
    IReadOnlyList<string> TargetableEnemyIds,
    int? HittableEnemyCount,
    IReadOnlyList<string> HittableEnemyIds,
    string? HoveredTargetKind,
    string? HoveredTargetId,
    string? HoveredTargetLabel,
    bool? HoveredTargetIsHittable,
    string? LastCardPlayStartedCardId,
    string? LastCardPlayFinishedCardId,
    string? LastCardPlayFinishedCardName)
{
    public int? RoundNumber { get; init; }

    public bool? PlayerActionsDisabled { get; init; }

    public bool? EndingPlayerTurnPhaseOne { get; init; }

    public bool? EndingPlayerTurnPhaseTwo { get; init; }

    public int? HistoryStartedCount { get; init; }

    public int? HistoryFinishedCount { get; init; }

    public string? InteractionRevision { get; init; }

    public string? ScreenEpisodeId { get; init; }

    public bool KeepsCardPlayOpen => CardPlayPending == true || TargetingInProgress == true;

    public bool ExplicitlyClearedSelection => CardPlayPending == false && TargetingInProgress != true;

    public bool HasCardSelectionEvidence => PendingSelection is not null || KeepsCardPlayOpen;

    public bool HasExplicitEnemyTargetingEvidence =>
        TargetingInProgress == true
        || string.Equals(HoveredTargetKind, "enemy", StringComparison.OrdinalIgnoreCase);

    public bool HasExplicitTargetableEnemyAuthority => TargetableEnemyCount is not null;

    public bool HasExplicitTargetableEnemy => TargetableEnemyCount > 0 || TargetableEnemyIds.Count > 0;

    public bool HasExplicitHittableEnemyAuthority => HittableEnemyCount is not null;

    public bool HasExplicitHittableEnemy => HittableEnemyCount > 0 || HittableEnemyIds.Count > 0;

    public bool HasAttackSelectionWithoutExplicitTargeting =>
        PendingSelection?.Kind == AutoCombatCardKind.AttackLike
        && !HasExplicitEnemyTargetingEvidence;
}

static class CombatAuthoritySupport
{
    public static string? TryGetMetaValue(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value) ? value : null;
    }

    public static bool? TryGetBoolOrCrossCheck(ObserverSummary observer, string key, string crossCheckKey)
    {
        if (observer.Meta.TryGetValue(key, out var value)
            && bool.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return TryGetCrossCheckValue(observer, crossCheckKey, out value) && bool.TryParse(value, out parsed)
            ? parsed
            : null;
    }

    public static int? TryGetIntOrCrossCheck(ObserverSummary observer, string key, string crossCheckKey)
    {
        if (observer.Meta.TryGetValue(key, out var value)
            && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return TryGetCrossCheckValue(observer, crossCheckKey, out value)
               && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed)
            ? parsed
            : null;
    }

    private static bool TryGetCrossCheckValue(ObserverSummary observer, string key, out string value)
    {
        value = string.Empty;
        if (!observer.Meta.TryGetValue("combatCrossCheck", out var combatCrossCheck)
            || string.IsNullOrWhiteSpace(combatCrossCheck))
        {
            return false;
        }

        foreach (var segment in combatCrossCheck.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separatorIndex = segment.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var candidateKey = segment[..separatorIndex].Trim();
            if (!string.Equals(candidateKey, key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            value = segment[(separatorIndex + 1)..].Trim();
            return true;
        }

        return false;
    }
}

static class CombatRuntimeStateSupport
{
    public static CombatRuntimeState Read(ObserverSummary observer, IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var selectedCardSlot = CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatSelectedCardSlot", "combatSelectedCardSlot");
        var awaitingPlaySlots = ParseSlotList(CombatAuthoritySupport.TryGetMetaValue(observer, "combatAwaitingPlaySlots"));
        if (selectedCardSlot is null && awaitingPlaySlots.Count > 0)
        {
            selectedCardSlot = awaitingPlaySlots[0];
        }

        var selectedCardTargetType = CombatAuthoritySupport.TryGetMetaValue(observer, "combatSelectedCardTargetType");
        var selectedCardType = CombatAuthoritySupport.TryGetMetaValue(observer, "combatSelectedCardType");
        var pendingSelection = TryResolvePendingSelection(selectedCardSlot, selectedCardTargetType, selectedCardType, observer, combatCardKnowledge);

        return new CombatRuntimeState(
            CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatCardPlayPending", "combatCardPlayPending"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatPlayMode"),
            pendingSelection,
            CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatTargetingInProgress", "combatTargetingInProgress"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatValidTargetsType"),
            CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatTargetableEnemyCount", "combatTargetableEnemyCount"),
            ParseIdList(CombatAuthoritySupport.TryGetMetaValue(observer, "combatTargetableEnemyIds")),
            CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatHittableEnemyCount", "combatHittableEnemyCount"),
            ParseIdList(CombatAuthoritySupport.TryGetMetaValue(observer, "combatHittableEnemyIds")),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatHoveredTargetKind"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatHoveredTargetId"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatHoveredTargetLabel"),
            CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatHoveredTargetIsHittable", "combatHoveredTargetIsHittable"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatLastCardPlayStartedCardId"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatLastCardPlayFinishedCardId"),
            CombatAuthoritySupport.TryGetMetaValue(observer, "combatLastCardPlayFinishedCardName"))
        {
            RoundNumber = CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatRoundNumber", "CombatState.RoundNumber"),
            PlayerActionsDisabled = CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatPlayerActionsDisabled", "CombatManager.PlayerActionsDisabled"),
            EndingPlayerTurnPhaseOne = CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatEndingPlayerTurnPhaseOne", "CombatManager.EndingPlayerTurnPhaseOne"),
            EndingPlayerTurnPhaseTwo = CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatEndingPlayerTurnPhaseTwo", "CombatManager.EndingPlayerTurnPhaseTwo"),
            HistoryStartedCount = CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatHistoryStartedCount", "combatHistoryStartedCount"),
            HistoryFinishedCount = CombatAuthoritySupport.TryGetIntOrCrossCheck(observer, "combatHistoryFinishedCount", "combatHistoryFinishedCount"),
            InteractionRevision = CombatAuthoritySupport.TryGetMetaValue(observer, "combatInteractionRevision"),
            ScreenEpisodeId = observer.SceneEpisodeId,
        };
    }

    public static PendingCombatSelection? ResolvePendingSelection(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? historyPendingSelection)
    {
        var runtime = Read(observer, combatCardKnowledge);
        if (runtime.PendingSelection is not null)
        {
            return runtime.PendingSelection;
        }

        if (runtime.ExplicitlyClearedSelection)
        {
            return null;
        }

        return historyPendingSelection;
    }

    public static bool HasNonEnemyConfirmEvidence(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? pendingSelection,
        AutoCombatAnalysis analysis)
    {
        var runtime = Read(observer, combatCardKnowledge);
        if (runtime.PendingSelection?.Kind == AutoCombatCardKind.DefendLike
            && runtime.PendingSelection.SlotIndex is >= 1 and <= 5
            && runtime.TargetingInProgress != true)
        {
            return true;
        }

        return pendingSelection?.Kind == AutoCombatCardKind.DefendLike
               && pendingSelection.SlotIndex is >= 1 and <= 5
               && ((analysis.HasSelectedCard && analysis.SelectedCardKind == AutoCombatCardKind.DefendLike)
                   || analysis.HasSelfTargetBrackets);
    }

    public static bool CanResolveEnemyTarget(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? pendingSelection,
        AutoCombatAnalysis analysis)
    {
        var runtime = Read(observer, combatCardKnowledge);
        return runtime.PendingSelection?.Kind == AutoCombatCardKind.AttackLike
               && runtime.HasExplicitEnemyTargetingEvidence
               && (!runtime.HasExplicitHittableEnemyAuthority || runtime.HasExplicitHittableEnemy);
    }

    public static bool RequiresExplicitTargetingBeforeEnemyClick(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        return Read(observer, combatCardKnowledge).HasAttackSelectionWithoutExplicitTargeting;
    }

    public static bool HasSelectionToKeep(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var runtime = Read(observer, combatCardKnowledge);
        return runtime.KeepsCardPlayOpen || runtime.PendingSelection is not null;
    }

    public static bool ShouldDeferLoopEndTurn(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        return Read(observer, combatCardKnowledge).KeepsCardPlayOpen;
    }

    private static PendingCombatSelection? TryResolvePendingSelection(
        int? selectedCardSlot,
        string? selectedCardTargetType,
        string? selectedCardType,
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        if (!selectedCardSlot.HasValue || selectedCardSlot.Value is < 1 or > 5)
        {
            return null;
        }

        var slotIndex = selectedCardSlot.Value;

        if (IsEnemyTargetType(selectedCardTargetType)
            || string.Equals(selectedCardType, "Attack", StringComparison.OrdinalIgnoreCase))
        {
            return new PendingCombatSelection(slotIndex, AutoCombatCardKind.AttackLike);
        }

        if (IsNonEnemyTargetType(selectedCardTargetType)
            || string.Equals(selectedCardType, "Skill", StringComparison.OrdinalIgnoreCase)
            || string.Equals(selectedCardType, "Power", StringComparison.OrdinalIgnoreCase))
        {
            return new PendingCombatSelection(slotIndex, AutoCombatCardKind.DefendLike);
        }

        var observerCard = observer.CombatHand.FirstOrDefault(card => card.SlotIndex == slotIndex);
        if (observerCard is not null)
        {
            return IsObservedAttackCard(observerCard)
                ? new PendingCombatSelection(slotIndex, AutoCombatCardKind.AttackLike)
                : CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatHandCard(observerCard, observer.PlayerEnergy, combatCardKnowledge)
                    ? new PendingCombatSelection(slotIndex, AutoCombatCardKind.DefendLike)
                    : null;
        }

        var knowledgeCard = combatCardKnowledge.FirstOrDefault(card => card.SlotIndex == slotIndex);
        if (knowledgeCard is not null)
        {
            return IsKnowledgeEnemyTargetCard(knowledgeCard)
                ? new PendingCombatSelection(slotIndex, AutoCombatCardKind.AttackLike)
                : CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatCard(knowledgeCard, observer.PlayerEnergy)
                    ? new PendingCombatSelection(slotIndex, AutoCombatCardKind.DefendLike)
                    : null;
        }

        return null;
    }

    private static bool IsEnemyTargetType(string? targetType)
    {
        return string.Equals(targetType, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "RandomEnemy", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyTargetType(string? targetType)
    {
        return string.Equals(targetType, "Self", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "None", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "AllEnemies", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "AllAllies", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsObservedAttackCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("STRIKE", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("BASH", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsKnowledgeEnemyTargetCard(CombatCardKnowledgeHint card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<int> ParseSlotList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<int>();
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(segment => int.TryParse(segment, NumberStyles.Integer, CultureInfo.InvariantCulture, out var slotIndex) ? slotIndex : -1)
            .Where(slotIndex => slotIndex is >= 1 and <= 5)
            .Distinct()
            .OrderBy(static slotIndex => slotIndex)
            .ToArray();
    }

    private static IReadOnlyList<string> ParseIdList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static segment => !string.IsNullOrWhiteSpace(segment))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

}

static class CombatEligibilitySupport
{
    public static bool IsCombatPlayerActionWindowClosed(ObserverSummary observer)
    {
        if (CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatPlayerActionsDisabled", "CombatManager.PlayerActionsDisabled") == true)
        {
            return true;
        }

        if (CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatEndingPlayerTurnPhaseOne", "CombatManager.EndingPlayerTurnPhaseOne") == true)
        {
            return true;
        }

        if (CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatEndingPlayerTurnPhaseTwo", "CombatManager.EndingPlayerTurnPhaseTwo") == true)
        {
            return true;
        }

        if (CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsEnemyTurnStarted", "CombatManager.IsEnemyTurnStarted") == true)
        {
            return true;
        }

        if (CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsPlayPhase", "CombatManager.IsPlayPhase") == false)
        {
            return true;
        }

        return false;
    }

    public static bool IsPlayableAutoNonEnemyCombatCard(CombatCardKnowledgeHint card, int? energy)
    {
        return IsNonEnemyCombatCard(card)
               && IsAutoNonEnemyPromotionEligible(card)
               && IsPlayableAtCurrentEnergy(card, energy);
    }

    public static bool IsPlayableAutoNonEnemyCombatHandCard(
        ObservedCombatHandCard card,
        int? energy,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        if (!IsNonEnemyCombatHandCard(card) || IsInertCombatHandCard(card))
        {
            return false;
        }

        if (ResolveObservedCombatCardKnowledge(card, combatCardKnowledge) is { } knowledgeCard
            && !IsAutoNonEnemyPromotionEligible(knowledgeCard))
        {
            return false;
        }

        var resolvedCost = ResolveObservedCombatCardCost(card, combatCardKnowledge);
        if (resolvedCost is < 0)
        {
            return false;
        }

        if (energy is null || resolvedCost is null)
        {
            return true;
        }

        return resolvedCost <= energy;
    }

    public static bool HasSelectedNonEnemyConfirmEvidence(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        AutoCombatAnalysis analysis,
        PendingCombatSelection? pendingSelection)
    {
        return CombatRuntimeStateSupport.HasNonEnemyConfirmEvidence(observer, combatCardKnowledge, pendingSelection, analysis);
    }

    public static bool HasSelectedNonEnemyConfirmEvidence(GuiSmokeStepRequest request)
    {
        var pendingSelection = CombatRuntimeStateSupport.ResolvePendingSelection(request.Observer, request.CombatCardKnowledge, CombatHistorySupport.TryGetPendingCombatSelection(request.History));
        var analysis = string.IsNullOrWhiteSpace(request.ScreenshotPath)
            ? new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown)
            : AutoCombatAnalyzer.Analyze(request.ScreenshotPath);
        return HasSelectedNonEnemyConfirmEvidence(request.Observer, request.CombatCardKnowledge, analysis, pendingSelection);
    }

    private static bool IsAutoNonEnemyPromotionEligible(CombatCardKnowledgeHint card)
    {
        return !string.Equals(card.Type, "Status", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(card.Type, "Curse", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(card.Target, "None", StringComparison.OrdinalIgnoreCase)
               && card.Cost is not < 0;
    }

    private static bool IsNonEnemyCombatCard(CombatCardKnowledgeHint card)
    {
        if (string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
            || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Equals(card.Type, "Power", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "Self", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "None", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllAllies", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyAlly", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Skill", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Type, "Power", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("DEFEND", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInertCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Status", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Type, "Curse", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlayableAtCurrentEnergy(CombatCardKnowledgeHint card, int? energy)
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

    private static int? ResolveObservedCombatCardCost(
        ObservedCombatHandCard card,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
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

        var cardKeys = BuildLookupKeys(card.Name);
        if (cardKeys.Count == 0)
        {
            return null;
        }

        return combatCardKnowledge
            .Where(candidate => candidate.Cost is not null)
            .Where(candidate => BuildLookupKeys(candidate.Name).Any(cardKeys.Contains))
            .Select(static candidate => candidate.Cost)
            .FirstOrDefault();
    }

    private static CombatCardKnowledgeHint? ResolveObservedCombatCardKnowledge(
        ObservedCombatHandCard card,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var slotMatch = combatCardKnowledge.FirstOrDefault(candidate => candidate.SlotIndex == card.SlotIndex);
        if (slotMatch is not null)
        {
            return slotMatch;
        }

        var cardKeys = BuildLookupKeys(card.Name);
        if (cardKeys.Count == 0)
        {
            return null;
        }

        return combatCardKnowledge.FirstOrDefault(candidate =>
            BuildLookupKeys(candidate.Name).Any(cardKeys.Contains));
    }

    private static IReadOnlyList<string> BuildLookupKeys(string? cardName)
    {
        if (string.IsNullOrWhiteSpace(cardName))
        {
            return Array.Empty<string>();
        }

        var keys = new List<string>();
        var normalizedName = NormalizeLookupKey(cardName);
        AddLookupKey(keys, normalizedName);

        var parts = cardName
            .Split(new[] { '.', '_', '-', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeLookupKey)
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

        AddLookupKey(keys, string.Concat(trimmedParts));
        AddLookupKey(keys, trimmedParts[0]);
        if (trimmedParts.Length > 1 && IsCombatClassSuffix(trimmedParts[^1]))
        {
            AddLookupKey(keys, string.Concat(trimmedParts[..^1]));
        }

        foreach (var part in trimmedParts)
        {
            AddLookupKey(keys, part);
        }

        return keys;
    }

    private static void AddLookupKey(List<string> keys, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate)
            || keys.Contains(candidate, StringComparer.Ordinal))
        {
            return;
        }

        keys.Add(candidate);
    }

    private static bool IsCombatClassSuffix(string value)
    {
        return value is "ironclad" or "silent" or "defect" or "watcher" or "colorless" or "status" or "curse";
    }

    private static string NormalizeLookupKey(string? value)
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
}

static class CombatDecisionContract
{
    public static bool TryMapSemanticAction(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string semanticAction)
    {
        return TryMapSemanticAction(request, decision, out semanticAction, out _);
    }

    public static bool IsAllowed(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string? semanticAction)
    {
        semanticAction = null;
        if (!TryMapSemanticAction(request, decision, out var mappedAction, out var allowLegacyCardAliases))
        {
            return false;
        }

        semanticAction = mappedAction;
        if (request.AllowedActions.Contains(mappedAction, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        return allowLegacyCardAliases
               && (request.AllowedActions.Contains("click card", StringComparer.OrdinalIgnoreCase)
                   || request.AllowedActions.Contains("select card from hand", StringComparer.OrdinalIgnoreCase));
    }

    private static bool TryMapSemanticAction(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string semanticAction,
        out bool allowLegacyCardAliases)
    {
        semanticAction = string.Empty;
        allowLegacyCardAliases = false;

        if (string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase))
        {
            semanticAction = "wait";
            return true;
        }

        if (!string.Equals(request.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(decision.ActionKind, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase))
        {
            if (CombatEligibilitySupport.HasSelectedNonEnemyConfirmEvidence(request))
            {
                semanticAction = "confirm selected non-enemy card";
                return true;
            }

            return false;
        }

        if (string.Equals(decision.ActionKind, "click-current", StringComparison.OrdinalIgnoreCase))
        {
            if (CombatEligibilitySupport.HasSelectedNonEnemyConfirmEvidence(request))
            {
                semanticAction = "confirm selected non-enemy card";
                return true;
            }

            return false;
        }

        if (CombatHistorySupport.TryParsePendingCombatSelection(decision.TargetLabel, out var selection)
            && selection is not null)
        {
            semanticAction = selection.Kind == AutoCombatCardKind.AttackLike
                ? $"select attack slot {selection.SlotIndex}"
                : $"select non-enemy slot {selection.SlotIndex}";
            allowLegacyCardAliases = true;
            return true;
        }

        if (CombatHistorySupport.IsCombatEnemyTargetLabel(decision.TargetLabel))
        {
            semanticAction = "click enemy";
            return true;
        }

        if (CombatHistorySupport.IsCombatEndTurnLabel(decision.TargetLabel)
            || (string.Equals(decision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase)
                && string.Equals(decision.KeyText, "E", StringComparison.OrdinalIgnoreCase)))
        {
            semanticAction = "click end turn";
            return true;
        }

        if (CombatHistorySupport.IsCombatCancelSelectionLabel(decision.TargetLabel)
            || string.Equals(decision.ActionKind, "right-click", StringComparison.OrdinalIgnoreCase))
        {
            semanticAction = "right-click cancel selected card";
            return true;
        }

        if (string.Equals(decision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase)
            && decision.KeyText?.Length == 1
            && char.IsDigit(decision.KeyText[0])
            && decision.KeyText[0] is >= '1' and <= '5')
        {
            semanticAction = $"select attack slot {decision.KeyText[0]}";
            allowLegacyCardAliases = true;
            return true;
        }

        return false;
    }
}

static class AutoCombatAnalyzer
{
    public static AutoCombatAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("combat", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoCombatAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown);
        }

        try
        {
            using var bitmap = new Bitmap(screenshotPath);
            var selectedOverlayBand = FindSelectedOverlay(bitmap);
            var hasSelectedCard = selectedOverlayBand is not AutoCombatOverlayBand.None;
            var hasTargetArrow = HasTargetArrow(bitmap);
            var hasSelfTargetBrackets = HasSelfTargetBrackets(bitmap);
            var selectedCardKind = hasSelectedCard
                ? ClassifySelectedCard(bitmap, selectedOverlayBand)
                : AutoCombatCardKind.Unknown;
            return new AutoCombatAnalysis(hasSelectedCard, selectedOverlayBand, hasTargetArrow, hasSelfTargetBrackets, selectedCardKind);
        }
        catch (ArgumentException)
        {
            return new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown);
        }
        catch (IOException)
        {
            return new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown);
        }
    }

    private static AutoCombatOverlayBand FindSelectedOverlay(Bitmap bitmap)
    {
        var candidates = new[]
        {
            (Band: AutoCombatOverlayBand.Left, Bounds: (Left: 0.12, Top: 0.48, Right: 0.38, Bottom: 0.88)),
            (Band: AutoCombatOverlayBand.Center, Bounds: (Left: 0.38, Top: 0.48, Right: 0.62, Bottom: 0.88)),
            (Band: AutoCombatOverlayBand.Right, Bounds: (Left: 0.62, Top: 0.48, Right: 0.88, Bottom: 0.88)),
        };

        var bestBand = AutoCombatOverlayBand.None;
        var bestScore = 0;
        foreach (var candidate in candidates)
        {
            var cyanPixels = CountMatchingPixels(bitmap, candidate.Bounds.Left, candidate.Bounds.Top, candidate.Bounds.Right, candidate.Bounds.Bottom, color =>
                color.B > 150 && color.G > 130 && color.R < 140);
            var brightNonBackground = CountMatchingPixels(bitmap, candidate.Bounds.Left, candidate.Bounds.Top, candidate.Bounds.Right, candidate.Bounds.Bottom, color =>
                (color.R + color.G + color.B) / 3.0 > 70
                && (Math.Abs(color.R - color.G) > 15 || Math.Abs(color.G - color.B) > 15 || Math.Abs(color.R - color.B) > 15));
            var score = (cyanPixels * 3) + brightNonBackground;
            if (score > bestScore)
            {
                bestScore = score;
                bestBand = candidate.Band;
            }
        }

        return bestScore >= 42 ? bestBand : AutoCombatOverlayBand.None;
    }

    private static bool HasTargetArrow(Bitmap bitmap)
    {
        var grayLikePixels = CountMatchingPixels(bitmap, 0.40, 0.22, 0.60, 0.60, color =>
        {
            var max = Math.Max(color.R, Math.Max(color.G, color.B));
            var min = Math.Min(color.R, Math.Min(color.G, color.B));
            var delta = max - min;
            var brightness = (color.R + color.G + color.B) / 3.0;
            return delta < 20 && brightness is > 90 and < 235;
        });
        return grayLikePixels >= 25;
    }

    private static bool HasSelfTargetBrackets(Bitmap bitmap)
    {
        var yellowPixels = CountMatchingPixels(bitmap, 0.14, 0.34, 0.36, 0.76, color =>
            color.R > 160 && color.G > 125 && color.B < 120);
        return yellowPixels >= 8;
    }

    private static AutoCombatCardKind ClassifySelectedCard(Bitmap bitmap, AutoCombatOverlayBand band)
    {
        var bounds = GetOverlayContentBounds(band);
        var sample = AverageColor(bitmap, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
        if (sample.B > sample.R + 12)
        {
            return AutoCombatCardKind.DefendLike;
        }

        if (sample.R > sample.B + 12)
        {
            return AutoCombatCardKind.AttackLike;
        }

        return AutoCombatCardKind.Unknown;
    }

    private static int CountMatchingPixels(Bitmap bitmap, double left, double top, double right, double bottom, Func<Color, bool> predicate)
    {
        var count = 0;
        ForEachSample(bitmap, left, top, right, bottom, 18, 18, color =>
        {
            if (predicate(color))
            {
                count += 1;
            }
        });
        return count;
    }

    private static (double R, double G, double B, double Brightness) AverageColor(Bitmap bitmap, double left, double top, double right, double bottom)
    {
        var totalR = 0d;
        var totalG = 0d;
        var totalB = 0d;
        var total = 0;
        ForEachSample(bitmap, left, top, right, bottom, 16, 16, color =>
        {
            totalR += color.R;
            totalG += color.G;
            totalB += color.B;
            total += 1;
        });

        if (total == 0)
        {
            return (0, 0, 0, 0);
        }

        var averageR = totalR / total;
        var averageG = totalG / total;
        var averageB = totalB / total;
        return (averageR, averageG, averageB, (averageR + averageG + averageB) / 3.0);
    }

    internal static void ForEachSample(Bitmap bitmap, double left, double top, double right, double bottom, int columns, int rows, Action<Color> visitor)
    {
        ForEachSample(bitmap, left, top, right, bottom, columns, rows, (color, _, _) => visitor(color));
    }

    internal static void ForEachSample(Bitmap bitmap, double left, double top, double right, double bottom, int columns, int rows, Action<Color, int, int> visitor)
    {
        for (var row = 0; row < rows; row += 1)
        {
            var y = (int)Math.Round((bitmap.Height - 1) * Lerp(top, bottom, row / (double)Math.Max(1, rows - 1)));
            for (var column = 0; column < columns; column += 1)
            {
                var x = (int)Math.Round((bitmap.Width - 1) * Lerp(left, right, column / (double)Math.Max(1, columns - 1)));
                visitor(bitmap.GetPixel(x, y), x, y);
            }
        }
    }

    private static double Lerp(double from, double to, double t)
    {
        return from + ((to - from) * t);
    }

    private static (double Left, double Top, double Right, double Bottom) GetOverlayBounds(AutoCombatOverlayBand band)
    {
        return band switch
        {
            AutoCombatOverlayBand.Left => (0.12, 0.48, 0.38, 0.88),
            AutoCombatOverlayBand.Center => (0.38, 0.48, 0.62, 0.88),
            AutoCombatOverlayBand.Right => (0.62, 0.48, 0.88, 0.88),
            _ => (0.38, 0.48, 0.62, 0.88),
        };
    }

    private static (double Left, double Top, double Right, double Bottom) GetOverlayContentBounds(AutoCombatOverlayBand band)
    {
        return band switch
        {
            AutoCombatOverlayBand.Left => (0.16, 0.40, 0.34, 0.82),
            AutoCombatOverlayBand.Center => (0.42, 0.44, 0.58, 0.84),
            AutoCombatOverlayBand.Right => (0.66, 0.36, 0.82, 0.80),
            _ => (0.42, 0.44, 0.58, 0.84),
        };
    }
}

sealed record AssistantCardKnowledge(
    string Id,
    string Name,
    string? Type,
    string? Target,
    int? Cost,
    IReadOnlyList<string> MatchKeys);

sealed record AssistantEventKnowledge(
    string Id,
    string Title,
    IReadOnlyList<AssistantEventOptionKnowledge> Options);

sealed record AssistantEventOptionKnowledge(
    string Label,
    string? Description,
    string? OptionKey);

static class AutoCombatHandAnalyzer
{
    private static readonly (int SlotIndex, double Left, double Top, double Right, double Bottom, double CenterX, double CenterY)[] SlotRegions =
    {
        (1, 0.18, 0.76, 0.32, 0.96, 0.25, 0.875),
        (2, 0.29, 0.75, 0.43, 0.96, 0.36, 0.865),
        (3, 0.42, 0.74, 0.56, 0.96, 0.49, 0.855),
        (4, 0.55, 0.75, 0.69, 0.96, 0.62, 0.865),
        (5, 0.68, 0.76, 0.82, 0.97, 0.75, 0.875),
    };

    public static AutoCombatHandAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("combat-hand", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoCombatHandAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return new AutoCombatHandAnalysis(Array.Empty<AutoCombatHandSlotAnalysis>());
        }

        try
        {
            using var bitmap = new Bitmap(screenshotPath);
            var slots = SlotRegions
                .Select(region =>
                {
                    var sample = AverageColor(bitmap, region.Left, region.Top, region.Right, region.Bottom);
                    var isVisible = sample.Brightness >= 42 && (sample.R + sample.G + sample.B) >= 140;
                    var redBlueDelta = sample.R - sample.B;
                    var kind = !isVisible
                        ? AutoCombatCardKind.Unknown
                        : redBlueDelta >= 20
                            ? AutoCombatCardKind.AttackLike
                            : AutoCombatCardKind.DefendLike;
                    return new AutoCombatHandSlotAnalysis(
                        region.SlotIndex,
                        isVisible,
                        kind,
                        redBlueDelta,
                        sample.Brightness,
                        region.CenterX,
                        region.CenterY);
                })
                .ToArray();
            return new AutoCombatHandAnalysis(slots);
        }
        catch (ArgumentException)
        {
            return new AutoCombatHandAnalysis(Array.Empty<AutoCombatHandSlotAnalysis>());
        }
        catch (IOException)
        {
            return new AutoCombatHandAnalysis(Array.Empty<AutoCombatHandSlotAnalysis>());
        }
    }

    private static (double R, double G, double B, double Brightness) AverageColor(Bitmap bitmap, double left, double top, double right, double bottom)
    {
        var totalR = 0d;
        var totalG = 0d;
        var totalB = 0d;
        var total = 0;
        AutoCombatAnalyzer.ForEachSample(bitmap, left, top, right, bottom, 14, 14, color =>
        {
            totalR += color.R;
            totalG += color.G;
            totalB += color.B;
            total += 1;
        });

        if (total == 0)
        {
            return (0, 0, 0, 0);
        }

        var averageR = totalR / total;
        var averageG = totalG / total;
        var averageB = totalB / total;
        return (averageR, averageG, averageB, (averageR + averageG + averageB) / 3.0);
    }
}

sealed record WindowCaptureTarget(
    IntPtr Handle,
    string Title,
    Rectangle Bounds,
    bool IsFallback,
    bool IsMinimized);

enum CaptureBoundaryFailureKind
{
    None,
    UnusableFrame,
    TimedOut,
    Exception,
}

sealed record CaptureBoundaryResult(
    bool Succeeded,
    CaptureBoundaryFailureKind FailureKind,
    string? Detail,
    Exception? Exception = null);

sealed record GuiSmokeVideoRecordingMetadata(
    string ScopeKind,
    string SessionId,
    string RunId,
    string RootPath,
    string SessionRoot,
    string OutputPath)
{
    public string? AttemptId { get; init; }

    public string Status { get; set; } = "pending";

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? RecordingStartedAt { get; set; }

    public DateTimeOffset? RecordingEndedAt { get; set; }

    public string? FfmpegPath { get; set; }

    public bool FfmpegAvailable { get; set; }

    public string? CommandLine { get; set; }

    public bool WindowScopedCaptureRequested { get; set; }

    public string? CaptureMode { get; set; }

    public string? CaptureInputPattern { get; set; }

    public string? CaptureModeNote { get; set; }

    public string? WindowTitle { get; set; }

    public WindowBounds? CaptureBounds { get; set; }

    public bool? Kept { get; set; }

    public string? CompletionReason { get; set; }

    public string? SkipReason { get; set; }

    public int? ExitCode { get; set; }
}

sealed record VideoPathBinding(string HostPath, string ProcessPath);
sealed record GuiSmokeFfmpegCaptureSupport(bool SupportsGdigrab);
