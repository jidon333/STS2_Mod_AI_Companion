using System.Globalization;
using System.Text.Json;
using Sts2AiCompanion.AdvisorSceneModel;
using static ObserverScreenProvenance;

static class GuiSmokeAdvisorSceneModelBuilder
{
    internal static AdvisorSceneArtifact Build(
        GuiSmokeStepRequest request,
        ObserverState observer,
        GuiSmokeStepAnalysisContext analysisContext,
        string requestPath)
    {
        var sceneType = ResolveSceneType(observer, analysisContext);
        var playerContext = BuildPlayerContext(observer.StateDocument, observer);
        var sourceRefs = new List<string> { "observer.state", "canonical-scene-state" };

        AdvisorSceneArtifact artifact = sceneType switch
        {
            "combat" => BuildCombatArtifact(request, requestPath, observer, analysisContext, playerContext, sourceRefs),
            "reward" => BuildRewardArtifact(request, requestPath, observer, analysisContext, playerContext, sourceRefs),
            "event" => BuildEventArtifact(request, requestPath, observer, analysisContext, playerContext, sourceRefs),
            "rest-site" => BuildRestSiteArtifact(request, requestPath, observer, analysisContext, playerContext, sourceRefs),
            "shop" => BuildShopArtifact(request, requestPath, observer, analysisContext, playerContext, sourceRefs),
            "map" => BuildMapArtifact(request, requestPath, observer, analysisContext, playerContext, sourceRefs),
            _ => BuildUnknownArtifact(request, requestPath, observer, playerContext, sourceRefs),
        };

        return artifact with
        {
            SummaryText = AdvisorSceneSummaryFormatter.BuildSummary(artifact),
        };
    }

    private static AdvisorSceneArtifact BuildCombatArtifact(
        GuiSmokeStepRequest request,
        string requestPath,
        ObserverState observer,
        GuiSmokeStepAnalysisContext analysisContext,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        var release = analysisContext.CombatReleaseState;
        var handSummary = TryReadMetaString(observer.StateDocument, "combatHandSummary");
        var handCount = TryReadIntMeta(observer, "combatHandCount") ?? observer.CombatHand.Count;
        var drawPileCount = TryReadIntMeta(observer, "drawPileCount");
        var discardPileCount = TryReadIntMeta(observer, "discardPileCount");
        var exhaustPileCount = TryReadIntMeta(observer, "exhaustPileCount");
        var playPileCount = TryReadIntMeta(observer, "playPileCount");
        var enemyIntentSummary = TryReadMetaString(observer.StateDocument, "enemyIntentSummary")
                                 ?? TryReadMetaString(observer.StateDocument, "enemy-intent-summary");
        var details = new AdvisorSceneCombatDetails(
            observer.InCombat == true,
            observer.EncounterKind ?? "unknown",
            ToKebabCase(release.LifecycleStage.ToString()),
            TryReadInt(observer.StateDocument, "encounter", "turn"),
            handCount,
            handSummary,
            drawPileCount,
            discardPileCount,
            exhaustPileCount,
            playPileCount,
            TryReadIntMeta(observer, "combatHittableEnemyCount"),
            TryReadIntMeta(observer, "combatTargetableEnemyCount"),
            TryReadBoolMeta(observer, "combatTargetingInProgress") == true,
            enemyIntentSummary);
        var options = new List<AdvisorSceneOption>();
        foreach (var card in observer.CombatHand)
        {
            options.Add(new AdvisorSceneOption(
                card.Name,
                "combat-hand-card",
                card.Cost?.ToString(CultureInfo.InvariantCulture),
                card.Type,
                true,
                new[] { $"slot:{card.SlotIndex}", "category:hand-card" }));
        }

        foreach (var choice in observer.Choices.Where(static choice => ResolveCombatOptionCategory(choice) is not null))
        {
            options.Add(ToCombatOption(choice));
        }

        var missingFacts = new List<string>();
        var observerGaps = new List<string>();
        if (string.IsNullOrWhiteSpace(details.EnemyIntentSummary))
        {
            missingFacts.Add("combat-enemy-intent-summary-missing");
            observerGaps.Add("observer.state.meta.enemyIntentSummary missing");
        }

        if (playerContext.Energy is null)
        {
            missingFacts.Add("combat-energy-missing");
            observerGaps.Add("observer.state.player.energy missing");
        }

        if (drawPileCount is null || discardPileCount is null || exhaustPileCount is null || playPileCount is null)
        {
            missingFacts.Add("combat-pile-counts-partial");
            observerGaps.Add("observer.state.meta draw/discard/exhaust/play pile counts missing");
        }

        sourceRefs.Add("analysis.combat-release-state");
        sourceRefs.Add("analysis.runtime-combat-state");
        sourceRefs.Add("observer.state.meta.combatHandSummary");
        sourceRefs.Add("observer.state.meta.combatHandCount");
        sourceRefs.Add("observer.state.meta.drawPileCount");
        sourceRefs.Add("observer.state.meta.discardPileCount");
        sourceRefs.Add("observer.state.meta.exhaustPileCount");
        sourceRefs.Add("observer.state.meta.playPileCount");
        return CreateArtifactBase(
                request,
                requestPath,
                observer.CapturedAt,
                "combat",
                details.LifecycleStage,
                "combat",
                playerContext,
                BuildCombatUiSurfaceInventory(observer, details),
                options,
                missingFacts,
                observerGaps,
                new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["scene"] = 0.98,
                    ["player"] = playerContext.CurrentHp is null ? 0.40 : 0.95,
                    ["options"] = options.Count == 0 ? 0.40 : 0.85,
                },
                sourceRefs)
            with
            {
                Combat = details,
                ScreenshotPath = request.ScreenshotPath,
            };
    }

    private static AdvisorSceneArtifact BuildRewardArtifact(
        GuiSmokeStepRequest request,
        string requestPath,
        ObserverState observer,
        GuiSmokeStepAnalysisContext analysisContext,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        var scene = analysisContext.RewardScene;
        var rawOptions = SelectSceneOptions(observer, static option =>
            option.Tags.Any(static tag => tag.StartsWith("reward", StringComparison.OrdinalIgnoreCase))
            || string.Equals(option.Kind, "choice", StringComparison.OrdinalIgnoreCase));
        var rewardEntries = NormalizeRewardEntryOptions(rawOptions);
        var controlOptions = rawOptions
            .Where(static option => string.Equals(option.Kind, "choice", StringComparison.OrdinalIgnoreCase))
            .GroupBy(static option => $"{option.Kind}|{option.Label}", StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.OrderByDescending(ScoreHumanReadableOption).First())
            .ToList();
        var options = rewardEntries
            .Concat(controlOptions)
            .ToList();
        var details = new AdvisorSceneRewardDetails(
            ToKebabCase(scene.ExplicitAction.ToString()),
            ToKebabCase(scene.ReleaseStage.ToString()),
            scene.ClaimableRewardPresent,
            scene.ExplicitProceedVisible,
            scene.RewardChoiceVisible,
            scene.ColorlessChoiceVisible,
            rewardEntries.Count,
            scene.CardProgressionSurfacePresent);
        var missingFacts = new List<string>();
        var observerGaps = new List<string>();
        if (rewardEntries.Any(static option => string.IsNullOrWhiteSpace(option.Description)))
        {
            missingFacts.Add("reward-entry-description-partial");
            observerGaps.Add("observer.state.currentChoices description is partial");
        }

        sourceRefs.Add("analysis.reward-scene-state");
        sourceRefs.Add("observer.state.currentChoices");
        return CreateArtifactBase(
                request,
                requestPath,
                observer.CapturedAt,
                "reward",
                details.ExplicitAction,
                "reward",
                playerContext,
                new[]
                {
                    new AdvisorSceneUiSurface("reward-panel", true, null, "reward entries visible"),
                    new AdvisorSceneUiSurface("reward-proceed", scene.ExplicitProceedVisible, scene.ExplicitProceedVisible, "reward proceed visibility"),
                    new AdvisorSceneUiSurface("reward-card-progression", scene.CardProgressionSurfacePresent, null, "reward child selection"),
                },
                options,
                missingFacts,
                observerGaps,
                new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["scene"] = 0.96,
                    ["options"] = options.Count == 0 ? 0.30 : 0.90,
                },
                sourceRefs)
            with
            {
                Reward = details,
                ScreenshotPath = request.ScreenshotPath,
            };
    }

    private static AdvisorSceneArtifact BuildEventArtifact(
        GuiSmokeStepRequest request,
        string requestPath,
        ObserverState observer,
        GuiSmokeStepAnalysisContext analysisContext,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        var scene = analysisContext.EventScene;
        var ancientPhase = TryReadMetaString(observer.StateDocument, "ancientPhase");
        var eventIdentity = TryResolveEventIdentity(observer.StateDocument);
        var options = SelectSceneOptions(observer, static option =>
            option.Tags.Any(static tag => tag.Contains("event", StringComparison.OrdinalIgnoreCase))
            || string.Equals(option.Kind, "event-option", StringComparison.OrdinalIgnoreCase));
        var details = new AdvisorSceneEventDetails(
            ToKebabCase(scene.ExplicitAction.ToString()),
            ToKebabCase(scene.ReleaseStage.ToString()),
            scene.RewardSubstateActive,
            scene.ExplicitProceedVisible,
            scene.HasExplicitProgression,
            string.IsNullOrWhiteSpace(ancientPhase) ? "none" : ancientPhase!,
            eventIdentity,
            options.Count);
        var missingFacts = new List<string>();
        var observerGaps = new List<string>();
        if (string.IsNullOrWhiteSpace(eventIdentity))
        {
            missingFacts.Add("event-identity-missing");
            observerGaps.Add("observer.state.meta.event identity field missing");
        }

        if (options.Any(static option => string.IsNullOrWhiteSpace(option.Description)))
        {
            missingFacts.Add("event-option-description-partial");
            observerGaps.Add("observer.state.currentChoices description is partial");
        }

        sourceRefs.Add("analysis.event-scene-state");
        sourceRefs.Add("observer.state.currentChoices");
        return CreateArtifactBase(
                request,
                requestPath,
                observer.CapturedAt,
                "event",
                details.ExplicitAction,
                "event",
                playerContext,
                new[]
                {
                    new AdvisorSceneUiSurface("event-options", options.Count > 0, null, "event options"),
                    new AdvisorSceneUiSurface("event-proceed", scene.ExplicitProceedVisible, scene.ExplicitProceedVisible, "event proceed"),
                    new AdvisorSceneUiSurface("event-reward-substate", scene.RewardSubstateActive, null, "event reward substate"),
                },
                options,
                missingFacts,
                observerGaps,
                new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["scene"] = 0.95,
                    ["options"] = options.Count == 0 ? 0.30 : 0.92,
                },
                sourceRefs)
            with
            {
                Event = details,
                ScreenshotPath = request.ScreenshotPath,
            };
    }

    private static AdvisorSceneArtifact BuildRestSiteArtifact(
        GuiSmokeStepRequest request,
        string requestPath,
        ObserverState observer,
        GuiSmokeStepAnalysisContext analysisContext,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        var scene = AutoDecisionProvider.BuildRestSiteSceneState(observer) ?? throw new InvalidOperationException("Rest-site scene state was expected but not resolved.");
        var options = SelectSceneOptions(observer, static option => option.Tags.Any(static tag => tag.Contains("rest-site", StringComparison.OrdinalIgnoreCase)));
        var details = new AdvisorSceneRestSiteDetails(
            ToKebabCase(scene.ReleaseStage.ToString()),
            scene.ExplicitChoiceVisible,
            scene.ExplicitChoiceReady,
            scene.SmithUpgradeActive,
            scene.SmithConfirmVisible,
            scene.ProceedVisible,
            scene.SelectionSettling,
            options.Count,
            scene.SmithUpgradeActive);
        var missingFacts = new List<string>();
        var observerGaps = new List<string>();
        if (scene.SmithUpgradeActive && options.Count == 0)
        {
            missingFacts.Add("rest-site-upgrade-candidates-missing");
            observerGaps.Add("observer.state.currentChoices missing smith candidates");
        }

        sourceRefs.Add("analysis.rest-site-scene-state");
        sourceRefs.Add("observer.state.currentChoices");
        var choiceSurfacePresent = scene.ExplicitChoiceVisible || options.Count > 0;
        var choiceSurfaceEnabled = scene.ExplicitChoiceReady || options.Any(static option => option.Enabled);
        return CreateArtifactBase(
                request,
                requestPath,
                observer.CapturedAt,
                "rest-site",
                ResolveRestSiteStage(scene),
                "rest-site",
                playerContext,
                new[]
                {
                    new AdvisorSceneUiSurface("rest-site-choice", choiceSurfacePresent, choiceSurfaceEnabled, "rest or smith choice"),
                    new AdvisorSceneUiSurface("rest-site-smith", scene.SmithUpgradeActive, scene.SmithConfirmVisible, "smith upgrade surface"),
                    new AdvisorSceneUiSurface("rest-site-proceed", scene.ProceedVisible, scene.ProceedVisible, "rest-site proceed"),
                },
                options,
                missingFacts,
                observerGaps,
                new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["scene"] = 0.95,
                    ["player"] = playerContext.CurrentHp is null ? 0.40 : 0.95,
                    ["options"] = options.Count == 0 ? 0.30 : 0.90,
                },
                sourceRefs)
            with
            {
                RestSite = details,
                ScreenshotPath = request.ScreenshotPath,
            };
    }

    private static AdvisorSceneArtifact BuildShopArtifact(
        GuiSmokeStepRequest request,
        string requestPath,
        ObserverState observer,
        GuiSmokeStepAnalysisContext analysisContext,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        var scene = analysisContext.CanonicalNonCombatScene as ShopSceneState
                    ?? throw new InvalidOperationException("Shop scene state was expected but not resolved.");
        var options = SelectSceneOptions(observer, static option =>
            option.Tags.Any(static tag => tag.Contains("shop", StringComparison.OrdinalIgnoreCase))
            || option.Kind.StartsWith("shop", StringComparison.OrdinalIgnoreCase));
        var serviceCount = options.Count(static option => option.Kind.Contains("service", StringComparison.OrdinalIgnoreCase)
                                                          || option.Kind.Contains("card-removal", StringComparison.OrdinalIgnoreCase));
        var itemCount = options.Count - serviceCount;
        var details = new AdvisorSceneShopDetails(
            scene.ShopState.InventoryOpen,
            scene.ShopState.ProceedEnabled,
            scene.ShopState.BackEnabled,
            scene.ShopState.CardRemovalVisible,
            scene.ShopState.CardRemovalEnabled,
            scene.ShopState.CardRemovalUsed,
            scene.ShopState.OptionCount,
            scene.ShopState.AffordableOptionCount,
            serviceCount,
            itemCount);
        var missingFacts = new List<string>();
        var observerGaps = new List<string>();
        if (options.Any(static option => string.Equals(option.Description, "Merchant inventory slot", StringComparison.OrdinalIgnoreCase)
                                         || string.IsNullOrWhiteSpace(option.Description)))
        {
            missingFacts.Add("shop-item-effect-summary-missing");
            observerGaps.Add("observer.state.currentChoices description lacks item effect summary");
        }

        if (options.All(static option => !ContainsPrice(option.Description)))
        {
            missingFacts.Add("shop-item-price-missing");
            observerGaps.Add("observer.state.currentChoices has no exact price field");
        }

        sourceRefs.Add("analysis.shop-scene-state");
        sourceRefs.Add("observer.state.player.gold");
        sourceRefs.Add("observer.state.currentChoices");
        return CreateArtifactBase(
                request,
                requestPath,
                observer.CapturedAt,
                "shop",
                scene.ShopState.InventoryOpen ? "inventory-open" : "merchant-entry",
                "shop",
                playerContext,
                new[]
                {
                    new AdvisorSceneUiSurface("shop-inventory", scene.ShopState.InventoryOpen, null, "merchant inventory"),
                    new AdvisorSceneUiSurface("shop-back", scene.ShopState.BackVisible, scene.ShopState.BackEnabled, "close inventory"),
                    new AdvisorSceneUiSurface("shop-proceed", scene.ShopState.ProceedEnabled, scene.ShopState.ProceedEnabled, "leave shop"),
                    new AdvisorSceneUiSurface("shop-card-removal", scene.ShopState.CardRemovalVisible, scene.ShopState.CardRemovalEnabled, "card removal service"),
                },
                options,
                missingFacts,
                observerGaps,
                new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["scene"] = 0.95,
                    ["player"] = playerContext.Gold is null ? 0.50 : 0.95,
                    ["options"] = options.Count == 0 ? 0.30 : 0.92,
                },
                sourceRefs)
            with
            {
                Shop = details,
                ScreenshotPath = request.ScreenshotPath,
            };
    }

    private static AdvisorSceneArtifact BuildMapArtifact(
        GuiSmokeStepRequest request,
        string requestPath,
        ObserverState observer,
        GuiSmokeStepAnalysisContext analysisContext,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        var options = SelectSceneOptions(observer, static option => string.Equals(option.Kind, "map-node", StringComparison.OrdinalIgnoreCase));
        var currentNode = TryReadMetaString(observer.StateDocument, "map-node");
        var details = new AdvisorSceneMapDetails(
            analysisContext.MapOverlayState.ForegroundVisible,
            analysisContext.MapOverlayState.CurrentNodeArrowVisible,
            analysisContext.MapOverlayState.ReachableNodeCandidatePresent,
            options.Count,
            currentNode,
            options.Select(static option => option.Label).ToArray());
        var missingFacts = new List<string> { "map-route-context-missing" };
        var observerGaps = new List<string> { "observer export currently exposes only immediate reachable node context" };
        if (string.IsNullOrWhiteSpace(currentNode))
        {
            missingFacts.Add("map-current-node-identity-missing");
            observerGaps.Add("observer.state.meta.map-node missing");
        }

        sourceRefs.Add("analysis.map-overlay-state");
        sourceRefs.Add("observer.state.currentChoices");
        return CreateArtifactBase(
                request,
                requestPath,
                observer.CapturedAt,
                "map",
                ResolveMapStage(analysisContext),
                "map",
                playerContext,
                new[]
                {
                    new AdvisorSceneUiSurface("map-overlay", analysisContext.MapOverlayState.ForegroundVisible, null, "map overlay"),
                    new AdvisorSceneUiSurface("map-current-arrow", analysisContext.MapOverlayState.CurrentNodeArrowVisible, null, "current node arrow"),
                    new AdvisorSceneUiSurface("map-reachable-node", analysisContext.MapOverlayState.ReachableNodeCandidatePresent, null, "reachable node candidate"),
                },
                options,
                missingFacts,
                observerGaps,
                new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["scene"] = 0.93,
                    ["options"] = options.Count == 0 ? 0.30 : 0.88,
                },
                sourceRefs)
            with
            {
                Map = details,
                ScreenshotPath = request.ScreenshotPath,
            };
    }

    private static AdvisorSceneArtifact BuildUnknownArtifact(
        GuiSmokeStepRequest request,
        string requestPath,
        ObserverState observer,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        sourceRefs.Add("observer.state.currentScreen");
        return CreateArtifactBase(
            request,
            requestPath,
            observer.CapturedAt,
            observer.CurrentScreen ?? observer.VisibleScreen ?? "unknown",
            "unknown",
            "unknown",
            playerContext,
            Array.Empty<AdvisorSceneUiSurface>(),
            Array.Empty<AdvisorSceneOption>(),
            new[] { "scene-model-not-implemented" },
            new[] { "no canonical scene model for current screen" },
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["scene"] = 0.20,
            },
            sourceRefs);
    }

    private static AdvisorSceneArtifact CreateArtifactBase(
        GuiSmokeStepRequest request,
        string requestPath,
        DateTimeOffset? capturedAtUtc,
        string sceneType,
        string sceneStage,
        string canonicalOwner,
        AdvisorScenePlayerContext playerContext,
        IReadOnlyList<AdvisorSceneUiSurface> uiSurfaceInventory,
        IReadOnlyList<AdvisorSceneOption> options,
        IReadOnlyList<string> missingFacts,
        IReadOnlyList<string> observerGaps,
        IReadOnlyDictionary<string, double> confidence,
        IReadOnlyList<string> sourceRefs)
    {
        return new AdvisorSceneArtifact(
            AdvisorSceneSchema.Version,
            "replay",
            request.RunId,
            capturedAtUtc,
            null,
            request.AttemptId,
            request.StepIndex,
            request.Phase,
            sceneType,
            sceneStage,
            canonicalOwner,
            string.Empty,
            playerContext,
            uiSurfaceInventory,
            options,
            missingFacts,
            observerGaps,
            confidence,
            sourceRefs)
        {
            RequestPath = requestPath,
            ScreenshotPath = request.ScreenshotPath,
        };
    }

    private static AdvisorScenePlayerContext BuildPlayerContext(JsonDocument? stateDocument, ObserverState observer)
    {
        return new AdvisorScenePlayerContext(
            observer.PlayerCurrentHp,
            observer.PlayerMaxHp,
            observer.PlayerEnergy,
            TryReadInt(stateDocument, "player", "gold"),
            ReadArrayCount(stateDocument, "deck"),
            ReadStringArray(stateDocument, "relics"),
            ReadPotionArray(stateDocument));
    }

    private static IReadOnlyList<AdvisorSceneUiSurface> BuildCombatUiSurfaceInventory(ObserverState observer, AdvisorSceneCombatDetails details)
    {
        return new[]
        {
            new AdvisorSceneUiSurface("combat-hand", details.HandCount > 0, null, "combat hand summary"),
            new AdvisorSceneUiSurface("combat-pile-buttons", observer.Choices.Any(IsCombatPileButtonChoice), null, "draw/discard/exhaust/play pile buttons"),
            new AdvisorSceneUiSurface("combat-targeting", details.TargetingInProgress, details.TargetingInProgress, "targeting arrow state"),
            new AdvisorSceneUiSurface("combat-end-turn", observer.Choices.Any(IsEndTurnChoice), null, "end turn button"),
        };
    }

    private static List<AdvisorSceneOption> SelectSceneOptions(ObserverState observer, Func<AdvisorSceneOption, bool> predicate)
    {
        var options = observer.Choices
            .Select(ToOption)
            .Where(predicate)
            .GroupBy(static option => $"{option.Kind}|{option.Label}|{option.Value}", StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .ToList();
        return options;
    }

    private static AdvisorSceneOption ToOption(ObserverChoice choice)
    {
        return new AdvisorSceneOption(
            choice.Label,
            choice.Kind,
            choice.Value,
            choice.Description,
            choice.Enabled != false,
            choice.SemanticHints.Count == 0 ? Array.Empty<string>() : choice.SemanticHints.ToArray());
    }

    private static AdvisorSceneOption ToCombatOption(ObserverChoice choice)
    {
        var option = ToOption(choice);
        var category = ResolveCombatOptionCategory(choice);
        return string.IsNullOrWhiteSpace(category)
            ? option
            : AddTag(option, $"category:{category}");
    }

    private static AdvisorSceneOption AddTag(AdvisorSceneOption option, string tag)
    {
        if (string.IsNullOrWhiteSpace(tag) || option.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        {
            return option;
        }

        return option with
        {
            Tags = option.Tags.Concat(new[] { tag }).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
        };
    }

    private static string? ResolveCombatOptionCategory(ObserverChoice choice)
    {
        if (IsEndTurnChoice(choice))
        {
            return "combat-action";
        }

        if (IsCombatPileButtonChoice(choice))
        {
            return "pile-button";
        }

        if (IsCombatUtilityChoice(choice))
        {
            return "utility";
        }

        return null;
    }

    private static bool IsEndTurnChoice(ObserverChoice choice)
    {
        return choice.Label.Contains("턴 종료", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("End Turn", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Contains("combat-end-turn", StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsCombatPileButtonChoice(ObserverChoice choice)
    {
        return TryGetCombatPileId(choice) is not null;
    }

    private static bool IsCombatUtilityChoice(ObserverChoice choice)
    {
        return choice.Label.Contains("Ping", StringComparison.OrdinalIgnoreCase)
               || choice.Kind.Contains("ping", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => hint.Contains("ping", StringComparison.OrdinalIgnoreCase));
    }

    private static string? TryGetCombatPileId(ObserverChoice choice)
    {
        var seeds = new[]
        {
            choice.Label,
            choice.Value,
            choice.BindingId,
            choice.NodeId,
            string.Join(' ', choice.SemanticHints),
        };
        foreach (var seed in seeds)
        {
            if (string.IsNullOrWhiteSpace(seed))
            {
                continue;
            }

            if (seed.Contains("DrawPile", StringComparison.OrdinalIgnoreCase))
            {
                return "draw";
            }

            if (seed.Contains("DiscardPile", StringComparison.OrdinalIgnoreCase))
            {
                return "discard";
            }

            if (seed.Contains("ExhaustPile", StringComparison.OrdinalIgnoreCase))
            {
                return "exhaust";
            }

            if (seed.Contains("PlayPile", StringComparison.OrdinalIgnoreCase))
            {
                return "play";
            }
        }

        return null;
    }

    private static List<AdvisorSceneOption> NormalizeRewardEntryOptions(IEnumerable<AdvisorSceneOption> options)
    {
        return options
            .Where(static option => !string.Equals(option.Kind, "choice", StringComparison.OrdinalIgnoreCase))
            .GroupBy(static option => $"{option.Kind}|{option.Label}", StringComparer.OrdinalIgnoreCase)
            .Select(MergeRewardEntryOption)
            .ToList();
    }

    private static AdvisorSceneOption MergeRewardEntryOption(IGrouping<string, AdvisorSceneOption> group)
    {
        var entries = group.ToList();
        var representative = entries[0];
        var value = entries
            .Select(static option => option.Value)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .OrderByDescending(static value => ScoreHumanReadableText(value!, null))
            .FirstOrDefault();
        var description = entries
            .Select(static option => option.Description)
            .Where(static description => !string.IsNullOrWhiteSpace(description))
            .OrderByDescending(description => ScoreHumanReadableText(description!, representative.Label))
            .FirstOrDefault();
        var tags = entries
            .SelectMany(static option => option.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return new AdvisorSceneOption(
            representative.Label,
            representative.Kind,
            value,
            description,
            entries.Any(static option => option.Enabled),
            tags);
    }

    private static int ScoreHumanReadableOption(AdvisorSceneOption option)
    {
        var score = 0;
        if (!string.IsNullOrWhiteSpace(option.Description) && !string.Equals(option.Description, option.Label, StringComparison.OrdinalIgnoreCase))
        {
            score += 4;
        }

        if (!string.IsNullOrWhiteSpace(option.Value) && !option.Value.EndsWith("Reward", StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
        }

        score += option.Tags.Count;
        score += option.Description?.Length ?? 0;
        score += option.Value?.Length ?? 0;
        return score;
    }

    private static int ScoreHumanReadableText(string text, string? label)
    {
        var score = text.Length;
        if (!text.EndsWith("Reward", StringComparison.OrdinalIgnoreCase))
        {
            score += 20;
        }

        if (!string.IsNullOrWhiteSpace(label) && !string.Equals(text, label, StringComparison.OrdinalIgnoreCase))
        {
            score += 8;
        }

        return score;
    }

    private static string ResolveSceneType(ObserverState observer, GuiSmokeStepAnalysisContext analysisContext)
    {
        if (observer.InCombat == true || MatchesControlFlowScreen(observer, "combat"))
        {
            return "combat";
        }

        if (analysisContext.CanonicalNonCombatScene is RewardSceneState)
        {
            return "reward";
        }

        if (analysisContext.CanonicalNonCombatScene is EventSceneState)
        {
            return "event";
        }

        if (analysisContext.CanonicalNonCombatScene is ShopSceneState)
        {
            return "shop";
        }

        if (AutoDecisionProvider.BuildRestSiteSceneState(observer) is not null)
        {
            return "rest-site";
        }

        if (MatchesControlFlowScreen(observer, "map") || analysisContext.MapOverlayState.ForegroundVisible)
        {
            return "map";
        }

        return observer.CurrentScreen ?? observer.VisibleScreen ?? "unknown";
    }

    private static string ResolveRestSiteStage(RestSiteSceneState scene)
    {
        if (scene.SmithUpgradeActive)
        {
            return scene.SmithConfirmVisible ? "smith-confirm" : "smith-grid";
        }

        if (scene.ProceedVisible)
        {
            return "proceed";
        }

        if (scene.SelectionSettling)
        {
            return "selection-settling";
        }

        if (scene.ExplicitChoiceVisible)
        {
            return "explicit-choice";
        }

        return ToKebabCase(scene.ReleaseStage.ToString());
    }

    private static string ResolveMapStage(GuiSmokeStepAnalysisContext analysisContext)
    {
        if (!analysisContext.MapOverlayState.ForegroundVisible)
        {
            return "background";
        }

        if (!analysisContext.MapOverlayState.ExportedReachableNodeCandidatePresent)
        {
            return "map-surface-pending";
        }

        if (analysisContext.MapOverlayState.CurrentNodeArrowVisible)
        {
            return "map-foreground";
        }

        return "map-overlay";
    }

    private static string? TryResolveEventIdentity(JsonDocument? stateDocument)
    {
        if (TryReadMetaString(stateDocument, "ancientEventDetected") == "true")
        {
            return "ancient-event";
        }

        return TryReadMetaString(stateDocument, "eventId")
               ?? TryReadMetaString(stateDocument, "event-id")
               ?? TryReadMetaString(stateDocument, "eventTitle");
    }

    private static string ToKebabCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var builder = new List<char>(value.Length + 8);
        for (var index = 0; index < value.Length; index += 1)
        {
            var current = value[index];
            if (char.IsUpper(current) && index > 0 && char.IsLetterOrDigit(value[index - 1]))
            {
                builder.Add('-');
            }

            builder.Add(char.ToLowerInvariant(current));
        }

        return new string(builder.ToArray());
    }

    private static int ReadArrayCount(JsonDocument? document, string propertyName)
    {
        if (document is null
            || !document.RootElement.TryGetProperty(propertyName, out var element)
            || element.ValueKind != JsonValueKind.Array)
        {
            return 0;
        }

        var count = 0;
        foreach (var _ in element.EnumerateArray())
        {
            count += 1;
        }

        return count;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonDocument? document, string propertyName)
    {
        if (document is null
            || !document.RootElement.TryGetProperty(propertyName, out var element)
            || element.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        var values = new List<string>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                var value = item.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value);
                }
            }
        }

        return values;
    }

    private static IReadOnlyList<string> ReadPotionArray(JsonDocument? document)
    {
        if (document is null
            || !document.RootElement.TryGetProperty("potions", out var element)
            || element.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        var values = new List<string>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                var value = item.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value);
                }

                continue;
            }

            if (item.ValueKind == JsonValueKind.Object)
            {
                if (item.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String)
                {
                    var id = idElement.GetString();
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        values.Add(id);
                    }

                    continue;
                }

                if (item.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String)
                {
                    var name = nameElement.GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        values.Add(name);
                    }
                }
            }
        }

        return values;
    }

    private static int? TryReadInt(JsonDocument? document, string objectPropertyName, string intPropertyName)
    {
        if (document is null
            || !document.RootElement.TryGetProperty(objectPropertyName, out var objectElement)
            || objectElement.ValueKind != JsonValueKind.Object
            || !objectElement.TryGetProperty(intPropertyName, out var valueElement))
        {
            return null;
        }

        if (valueElement.ValueKind == JsonValueKind.Number && valueElement.TryGetInt32(out var number))
        {
            return number;
        }

        if (valueElement.ValueKind == JsonValueKind.String
            && int.TryParse(valueElement.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
        {
            return number;
        }

        return null;
    }

    private static string? TryReadMetaString(JsonDocument? document, string propertyName)
    {
        if (document is null
            || !document.RootElement.TryGetProperty("meta", out var metaElement)
            || metaElement.ValueKind != JsonValueKind.Object
            || !metaElement.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null,
        };
    }

    private static int? TryReadIntMeta(ObserverState observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var raw)
               && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static bool? TryReadBoolMeta(ObserverState observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var raw)
               && bool.TryParse(raw, out var value)
            ? value
            : null;
    }

    private static bool ContainsPrice(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return false;
        }

        return description.Contains("price", StringComparison.OrdinalIgnoreCase)
               || description.Contains("cost", StringComparison.OrdinalIgnoreCase)
               || description.Contains("골드", StringComparison.OrdinalIgnoreCase);
    }
}
