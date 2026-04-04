using System.Globalization;
using Sts2AiCompanion.AdvisorSceneModel;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Foundation.State;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Host;

internal static class CompanionLiveSceneModelBuilder
{
    internal static AdvisorSceneArtifact Build(CompanionRunState runState)
    {
        var sceneType = ResolveSceneType(runState);
        var playerContext = BuildPlayerContext(runState.Snapshot);
        var sourceRefs = new List<string> { "live-export.snapshot", "foundation.normalized-state.scene" };

        var artifact = sceneType switch
        {
            "combat" => BuildCombatArtifact(runState, playerContext, sourceRefs),
            "reward" => BuildRewardArtifact(runState, playerContext, sourceRefs),
            "event" => BuildEventArtifact(runState, playerContext, sourceRefs),
            "rest-site" => BuildRestSiteArtifact(runState, playerContext, sourceRefs),
            "shop" => BuildShopArtifact(runState, playerContext, sourceRefs),
            "map" => BuildMapArtifact(runState, playerContext, sourceRefs),
            _ => BuildUnknownArtifact(runState, playerContext, sourceRefs),
        };

        return artifact with
        {
            SummaryText = AdvisorSceneSummaryFormatter.BuildSummary(artifact),
        };
    }

    private static AdvisorSceneArtifact BuildCombatArtifact(
        CompanionRunState runState,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        var snapshot = runState.Snapshot;
        var choices = CompanionSceneNormalizer.SanitizeChoices(snapshot.CurrentChoices);
        var handChoices = choices.Where(IsCombatHandChoice).ToArray();
        var stage = ResolveCombatStage(snapshot, handChoices);
        var targetCount = TryReadIntMeta(snapshot.Meta, "combatTargetCount");
        var targetableEnemyCount = TryReadIntMeta(snapshot.Meta, "combatTargetableEnemyCount");
        var targetingInProgress = TryReadBoolMeta(snapshot.Meta, "combatTargetingInProgress") == true;
        var handSummary = TryGetMeta(snapshot.Meta, "combatHandSummary") ?? runState.NormalizedState.Combat.HandSummary;
        var enemyIntentSummary = TryGetMeta(snapshot.Meta, "enemyIntentSummary")
                                 ?? TryGetMeta(snapshot.Meta, "enemy-intent-summary")
                                 ?? runState.NormalizedState.Combat.EnemyIntentSummary;
        var options = handChoices
            .Select(ToOption)
            .Concat(choices.Where(IsEndTurnChoice).Select(ToOption))
            .ToList();
        var details = new AdvisorSceneCombatDetails(
            snapshot.Encounter?.InCombat ?? true,
            snapshot.Encounter?.Kind ?? "unknown",
            stage,
            snapshot.Encounter?.Turn,
            handChoices.Length,
            handSummary,
            targetCount,
            targetableEnemyCount,
            targetingInProgress,
            enemyIntentSummary);
        var missingFacts = new List<string>();
        var observerGaps = new List<string>();
        if (string.IsNullOrWhiteSpace(enemyIntentSummary))
        {
            missingFacts.Add("combat-enemy-intent-summary-missing");
            observerGaps.Add("live-export.snapshot.meta.enemyIntentSummary missing");
        }

        if (playerContext.Energy is null)
        {
            missingFacts.Add("combat-energy-missing");
            observerGaps.Add("live-export.snapshot.player.energy missing");
        }

        sourceRefs.Add("live-export.snapshot.meta.foregroundOwner");
        sourceRefs.Add("live-export.snapshot.meta.foregroundActionLane");
        sourceRefs.Add("live-export.snapshot.meta.combatHandSummary");
        sourceRefs.Add("live-export.snapshot.meta.combatTargetCount");
        sourceRefs.Add("live-export.snapshot.currentChoices");
        return CreateArtifactBase(
                runState,
                "combat",
                stage,
                ResolveCanonicalOwner(runState, "combat"),
                playerContext,
                new[]
                {
                    new AdvisorSceneUiSurface("combat-hand", handChoices.Length > 0 || !string.IsNullOrWhiteSpace(handSummary), null, "combat hand summary"),
                    new AdvisorSceneUiSurface("combat-targeting", targetingInProgress || (targetCount ?? 0) > 0, targetingInProgress, "targeting arrow state"),
                    new AdvisorSceneUiSurface("combat-end-turn", choices.Any(IsEndTurnChoice), null, "end turn button"),
                },
                options,
                missingFacts,
                observerGaps,
                new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["scene"] = 0.94,
                    ["player"] = playerContext.CurrentHp is null ? 0.40 : 0.95,
                    ["options"] = options.Count == 0 ? 0.35 : 0.86,
                },
                sourceRefs)
            with
            {
                Combat = details,
            };
    }

    private static AdvisorSceneArtifact BuildRewardArtifact(
        CompanionRunState runState,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        var snapshot = runState.Snapshot;
        var choices = CompanionSceneNormalizer.SanitizeChoices(snapshot.CurrentChoices);
        var childChoiceOptions = choices
            .Where(IsRewardChildChoice)
            .Select(ToOption)
            .GroupBy(static option => $"{option.Kind}|{option.Label}|{option.Value}", StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .ToList();
        var rewardEntryOptions = NormalizeRewardEntryOptions(
            choices.Where(IsRewardEntryChoice).Select(ToOption));
        var controlOptions = choices
            .Where(IsRewardControlChoice)
            .Select(ToOption)
            .GroupBy(static option => $"{option.Kind}|{option.Label}|{option.Value}", StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.OrderByDescending(ScoreHumanReadableOption).First())
            .ToList();
        var stage = ResolveRewardStage(runState, rewardEntryOptions.Count, childChoiceOptions.Count > 0);
        var options = childChoiceOptions.Count > 0
            ? childChoiceOptions
            : rewardEntryOptions.Concat(controlOptions).ToList();
        var explicitProceedVisible = TryReadBoolMeta(snapshot.Meta, "rewardProceedVisible") == true
                                     || controlOptions.Any(IsProceedLikeOption);
        var details = new AdvisorSceneRewardDetails(
            stage,
            stage,
            rewardEntryOptions.Count > 0,
            explicitProceedVisible,
            rewardEntryOptions.Count > 0,
            rewardEntryOptions.Any(static option => option.Kind.Contains("colorless", StringComparison.OrdinalIgnoreCase)
                                                    || option.Tags.Any(static tag => tag.Contains("colorless", StringComparison.OrdinalIgnoreCase))),
            childChoiceOptions.Count > 0 ? childChoiceOptions.Count : rewardEntryOptions.Count,
            childChoiceOptions.Count > 0);
        var missingFacts = new List<string>();
        var observerGaps = new List<string>();
        if (options.Any(static option => string.IsNullOrWhiteSpace(option.Description)))
        {
            missingFacts.Add("reward-entry-description-partial");
            observerGaps.Add("live-export.snapshot.currentChoices description is partial");
        }

        sourceRefs.Add("live-export.snapshot.meta.foregroundOwner");
        sourceRefs.Add("live-export.snapshot.meta.foregroundActionLane");
        sourceRefs.Add("live-export.snapshot.meta.rewardProceedVisible");
        sourceRefs.Add("live-export.snapshot.currentChoices");
        return CreateArtifactBase(
                runState,
                "reward",
                stage,
                ResolveCanonicalOwner(runState, "reward"),
                playerContext,
                new[]
                {
                    new AdvisorSceneUiSurface("reward-panel", rewardEntryOptions.Count > 0, null, "reward entries visible"),
                    new AdvisorSceneUiSurface("reward-proceed", explicitProceedVisible, explicitProceedVisible, "reward proceed visibility"),
                    new AdvisorSceneUiSurface("reward-card-progression", childChoiceOptions.Count > 0, null, "reward child selection"),
                },
                options,
                missingFacts,
                observerGaps,
                new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["scene"] = 0.93,
                    ["options"] = options.Count == 0 ? 0.30 : 0.90,
                },
                sourceRefs)
            with
            {
                Reward = details,
            };
    }

    private static AdvisorSceneArtifact BuildEventArtifact(
        CompanionRunState runState,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        var snapshot = runState.Snapshot;
        var choices = CompanionSceneNormalizer.SanitizeChoices(snapshot.CurrentChoices);
        var visibleOptions = SelectEventVisibleOptions(runState, choices);
        var stage = ResolveEventStage(runState, visibleOptions);
        var eventIdentity = ResolveEventIdentity(runState);
        var explicitProceedVisible = TryReadBoolMeta(snapshot.Meta, "eventProceedOptionVisible") == true
                                     || visibleOptions.Any(IsProceedLikeOption);
        var rewardSubstateActive = stage.Contains("reward", StringComparison.OrdinalIgnoreCase)
                                   || visibleOptions.Any(IsRewardLikeOption);
        var details = new AdvisorSceneEventDetails(
            stage,
            stage,
            rewardSubstateActive,
            explicitProceedVisible,
            explicitProceedVisible || visibleOptions.Count > 0,
            TryGetMeta(snapshot.Meta, "ancientPhase")
            ?? NormalizeLane(TryGetMeta(snapshot.Meta, "foregroundActionLane"))
            ?? "none",
            eventIdentity,
            visibleOptions.Count);
        var missingFacts = new List<string>();
        var observerGaps = new List<string>();
        if (string.IsNullOrWhiteSpace(eventIdentity))
        {
            missingFacts.Add("event-identity-missing");
            observerGaps.Add("live-export.snapshot.meta.event identity fields missing");
        }

        if (visibleOptions.Any(static option => string.IsNullOrWhiteSpace(option.Description)))
        {
            missingFacts.Add("event-option-description-partial");
            observerGaps.Add("live-export.snapshot.currentChoices description is partial");
        }

        sourceRefs.Add("live-export.snapshot.meta.foregroundOwner");
        sourceRefs.Add("live-export.snapshot.meta.foregroundActionLane");
        sourceRefs.Add("live-export.snapshot.meta.ancientPhase");
        sourceRefs.Add("live-export.snapshot.currentChoices");
        return CreateArtifactBase(
                runState,
                "event",
                stage,
                ResolveCanonicalOwner(runState, "event"),
                playerContext,
                new[]
                {
                    new AdvisorSceneUiSurface("event-options", visibleOptions.Count > 0, null, "event options"),
                    new AdvisorSceneUiSurface("event-proceed", explicitProceedVisible, explicitProceedVisible, "event proceed"),
                    new AdvisorSceneUiSurface("event-reward-substate", rewardSubstateActive, null, "event reward substate"),
                },
                visibleOptions,
                missingFacts,
                observerGaps,
                new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["scene"] = 0.92,
                    ["options"] = visibleOptions.Count == 0 ? 0.30 : 0.92,
                },
                sourceRefs)
            with
            {
                Event = details,
            };
    }

    private static AdvisorSceneArtifact BuildRestSiteArtifact(
        CompanionRunState runState,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        var snapshot = runState.Snapshot;
        var choices = CompanionSceneNormalizer.SanitizeChoices(snapshot.CurrentChoices);
        var options = choices
            .Where(IsRestSiteChoice)
            .Select(ToOption)
            .GroupBy(static option => $"{option.Kind}|{option.Label}|{option.Value}", StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .ToList();
        var stage = ResolveRestSiteStage(snapshot, options);
        var smithUpgradeActive = TryReadBoolMeta(snapshot.Meta, "restSiteUpgradeScreenVisible") == true;
        var smithConfirmVisible = TryReadBoolMeta(snapshot.Meta, "restSiteUpgradeConfirmVisible") == true;
        var explicitChoiceVisible = TryReadBoolMeta(snapshot.Meta, "restSiteButtonsVisible") == true || options.Count > 0;
        var explicitChoiceReady = TryReadBoolMeta(snapshot.Meta, "restSiteButtonsClickReady") == true || options.Any(static option => option.Enabled);
        var proceedVisible = TryReadBoolMeta(snapshot.Meta, "restSiteProceedVisible") == true;
        var selectionSettling = string.Equals(stage, "selection-settling", StringComparison.OrdinalIgnoreCase);
        var details = new AdvisorSceneRestSiteDetails(
            stage,
            explicitChoiceVisible,
            explicitChoiceReady,
            smithUpgradeActive,
            smithConfirmVisible,
            proceedVisible,
            selectionSettling,
            options.Count,
            smithUpgradeActive);
        var missingFacts = new List<string>();
        var observerGaps = new List<string>();
        if (smithUpgradeActive && options.Count == 0)
        {
            missingFacts.Add("rest-site-upgrade-candidates-missing");
            observerGaps.Add("live-export.snapshot.currentChoices missing smith candidates");
        }

        sourceRefs.Add("live-export.snapshot.meta.restSiteButtonsVisible");
        sourceRefs.Add("live-export.snapshot.meta.restSiteProceedVisible");
        sourceRefs.Add("live-export.snapshot.meta.restSiteUpgradeScreenVisible");
        sourceRefs.Add("live-export.snapshot.currentChoices");
        return CreateArtifactBase(
                runState,
                "rest-site",
                stage,
                ResolveCanonicalOwner(runState, "rest-site"),
                playerContext,
                new[]
                {
                    new AdvisorSceneUiSurface("rest-site-choice", explicitChoiceVisible, explicitChoiceReady, "rest or smith choice"),
                    new AdvisorSceneUiSurface("rest-site-smith", smithUpgradeActive, smithConfirmVisible, "smith upgrade surface"),
                    new AdvisorSceneUiSurface("rest-site-proceed", proceedVisible, proceedVisible, "rest-site proceed"),
                },
                options,
                missingFacts,
                observerGaps,
                new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["scene"] = 0.92,
                    ["player"] = playerContext.CurrentHp is null ? 0.40 : 0.95,
                    ["options"] = options.Count == 0 ? 0.35 : 0.90,
                },
                sourceRefs)
            with
            {
                RestSite = details,
            };
    }

    private static AdvisorSceneArtifact BuildShopArtifact(
        CompanionRunState runState,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        var snapshot = runState.Snapshot;
        var choices = CompanionSceneNormalizer.SanitizeChoices(snapshot.CurrentChoices);
        var options = choices
            .Where(IsShopChoice)
            .Select(ToOption)
            .GroupBy(static option => $"{option.Kind}|{option.Label}|{option.Value}", StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .ToList();
        var inventoryOpen = TryReadBoolMeta(snapshot.Meta, "shopInventoryOpen") == true;
        var proceedEnabled = TryReadBoolMeta(snapshot.Meta, "shopProceedEnabled") == true || options.Any(static option => string.Equals(option.Kind, "shop-proceed", StringComparison.OrdinalIgnoreCase) && option.Enabled);
        var backEnabled = TryReadBoolMeta(snapshot.Meta, "shopBackEnabled") == true || options.Any(static option => string.Equals(option.Kind, "shop-back", StringComparison.OrdinalIgnoreCase) && option.Enabled);
        var cardRemovalVisible = TryReadBoolMeta(snapshot.Meta, "shopCardRemovalVisible") == true || options.Any(static option => string.Equals(option.Kind, "shop-card-removal", StringComparison.OrdinalIgnoreCase));
        var cardRemovalEnabled = TryReadBoolMeta(snapshot.Meta, "shopCardRemovalEnabled") == true || options.Any(static option => string.Equals(option.Kind, "shop-card-removal", StringComparison.OrdinalIgnoreCase) && option.Enabled);
        var cardRemovalUsed = TryReadBoolMeta(snapshot.Meta, "shopCardRemovalUsed") == true;
        var affordableOptionCount = TryReadIntMeta(snapshot.Meta, "shopAffordableOptionCount")
                                    ?? options.Count(static option => option.Enabled && !IsBackOrProceedOption(option));
        var serviceCount = options.Count(IsShopServiceOption);
        var itemCount = options.Count(IsShopItemOption);
        var details = new AdvisorSceneShopDetails(
            inventoryOpen,
            proceedEnabled,
            backEnabled,
            cardRemovalVisible,
            cardRemovalEnabled,
            cardRemovalUsed,
            options.Count,
            affordableOptionCount,
            serviceCount,
            itemCount);
        var missingFacts = new List<string>();
        var observerGaps = new List<string>();
        if (options.Any(static option => string.Equals(option.Description, "Merchant inventory slot", StringComparison.OrdinalIgnoreCase)
                                         || string.IsNullOrWhiteSpace(option.Description)))
        {
            missingFacts.Add("shop-item-effect-summary-missing");
            observerGaps.Add("live-export.snapshot.currentChoices description lacks item effect summary");
        }

        if (options.Where(IsShopItemOption).All(static option => !ContainsPrice(option.Description)))
        {
            missingFacts.Add("shop-item-price-missing");
            observerGaps.Add("live-export.snapshot.currentChoices has no exact price field");
        }

        sourceRefs.Add("live-export.snapshot.meta.shopInventoryOpen");
        sourceRefs.Add("live-export.snapshot.meta.shopAffordableOptionCount");
        sourceRefs.Add("live-export.snapshot.player.gold");
        sourceRefs.Add("live-export.snapshot.currentChoices");
        return CreateArtifactBase(
                runState,
                "shop",
                inventoryOpen ? "inventory-open" : "merchant-entry",
                ResolveCanonicalOwner(runState, "shop"),
                playerContext,
                new[]
                {
                    new AdvisorSceneUiSurface("shop-inventory", inventoryOpen, null, "merchant inventory"),
                    new AdvisorSceneUiSurface("shop-back", options.Any(static option => string.Equals(option.Kind, "shop-back", StringComparison.OrdinalIgnoreCase)), backEnabled, "close inventory"),
                    new AdvisorSceneUiSurface("shop-proceed", options.Any(static option => string.Equals(option.Kind, "shop-proceed", StringComparison.OrdinalIgnoreCase)), proceedEnabled, "leave shop"),
                    new AdvisorSceneUiSurface("shop-card-removal", cardRemovalVisible, cardRemovalEnabled, "card removal service"),
                },
                options,
                missingFacts,
                observerGaps,
                new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["scene"] = 0.94,
                    ["player"] = playerContext.Gold is null ? 0.50 : 0.95,
                    ["options"] = options.Count == 0 ? 0.30 : 0.92,
                },
                sourceRefs)
            with
            {
                Shop = details,
            };
    }

    private static AdvisorSceneArtifact BuildMapArtifact(
        CompanionRunState runState,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        var snapshot = runState.Snapshot;
        var choices = CompanionSceneNormalizer.SanitizeChoices(snapshot.CurrentChoices);
        var options = choices
            .Where(static choice => string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase))
            .Select(ToOption)
            .GroupBy(static option => $"{option.Kind}|{option.Label}|{option.Value}", StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .ToList();
        var foregroundOwner = NormalizeSceneToken(TryGetMeta(snapshot.Meta, "foregroundOwner"));
        var mapReleaseAuthority = TryReadBoolMeta(snapshot.Meta, "mapReleaseAuthority") == true;
        var mapSurfacePending = TryReadBoolMeta(snapshot.Meta, "mapSurfacePending") == true;
        var currentNodeArrowVisible = TryReadBoolMeta(snapshot.Meta, "mapCurrentNodeArrowVisible") == true;
        var reachableNodeCount = options.Count > 0 ? options.Count : (TryReadIntMeta(snapshot.Meta, "mapPointCount") ?? 0);
        var reachableNodePresent = reachableNodeCount > 0;
        var overlayVisible = reachableNodePresent
                             || currentNodeArrowVisible
                             || (string.Equals(foregroundOwner, "map", StringComparison.OrdinalIgnoreCase) && !mapSurfacePending);
        var stage = overlayVisible
            ? "map-overlay"
            : (mapReleaseAuthority && mapSurfacePending ? "map-surface-pending" : "map-overlay");
        var currentNode = runState.NormalizedState.Map.CurrentNode ?? TryGetMeta(snapshot.Meta, "map-node");
        var details = new AdvisorSceneMapDetails(
            overlayVisible,
            currentNodeArrowVisible,
            reachableNodePresent,
            reachableNodeCount,
            currentNode,
            options.Select(static option => option.Label).ToArray());
        var missingFacts = new List<string> { "map-route-context-missing" };
        var observerGaps = new List<string> { "live export currently exposes only immediate reachable node context" };
        if (string.IsNullOrWhiteSpace(currentNode))
        {
            missingFacts.Add("map-current-node-identity-missing");
            observerGaps.Add("live-export.snapshot.meta.map-node missing");
        }

        sourceRefs.Add("live-export.snapshot.meta.foregroundOwner");
        sourceRefs.Add("live-export.snapshot.meta.mapReleaseAuthority");
        sourceRefs.Add("live-export.snapshot.meta.mapSurfacePending");
        sourceRefs.Add("live-export.snapshot.meta.mapCurrentNodeArrowVisible");
        sourceRefs.Add("live-export.snapshot.meta.mapPointCount");
        sourceRefs.Add("live-export.snapshot.currentChoices");
        return CreateArtifactBase(
                runState,
                "map",
                stage,
                ResolveCanonicalOwner(runState, "map"),
                playerContext,
                new[]
                {
                    new AdvisorSceneUiSurface("map-overlay", details.ForegroundVisible, null, "map overlay"),
                    new AdvisorSceneUiSurface("map-current-arrow", details.CurrentNodeArrowVisible, null, "current node arrow"),
                    new AdvisorSceneUiSurface("map-reachable-node", details.ReachableNodePresent, null, "reachable node candidate"),
                },
                options,
                missingFacts,
                observerGaps,
                new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["scene"] = 0.90,
                    ["options"] = options.Count == 0 ? 0.30 : 0.88,
                },
                sourceRefs)
            with
            {
                Map = details,
            };
    }

    private static AdvisorSceneArtifact BuildUnknownArtifact(
        CompanionRunState runState,
        AdvisorScenePlayerContext playerContext,
        List<string> sourceRefs)
    {
        sourceRefs.Add("live-export.snapshot.currentScreen");
        return CreateArtifactBase(
            runState,
            NormalizeSceneToken(runState.Snapshot.CurrentScreen) ?? "unknown",
            "unknown",
            ResolveCanonicalOwner(runState, "unknown"),
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
        CompanionRunState runState,
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
            "live",
            runState.Snapshot.RunId,
            null,
            null,
            null,
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
            sourceRefs);
    }

    private static AdvisorScenePlayerContext BuildPlayerContext(LiveExportSnapshot snapshot)
    {
        return new AdvisorScenePlayerContext(
            snapshot.Player.CurrentHp,
            snapshot.Player.MaxHp,
            snapshot.Player.Energy,
            snapshot.Player.Gold,
            snapshot.Deck.Count,
            snapshot.Relics,
            snapshot.Potions);
    }

    private static IReadOnlyList<AdvisorSceneOption> SelectEventVisibleOptions(CompanionRunState runState, IReadOnlyList<LiveExportChoiceSummary> choices)
    {
        var eventOptions = choices.Where(IsEventChoice).Select(ToOption).ToList();
        if (eventOptions.Count > 0)
        {
            return eventOptions
                .GroupBy(static option => $"{option.Kind}|{option.Label}|{option.Value}", StringComparer.OrdinalIgnoreCase)
                .Select(static group => group.First())
                .ToList();
        }

        if (string.Equals(ResolveCanonicalOwner(runState, "event"), "event", StringComparison.OrdinalIgnoreCase))
        {
            return choices
                .Select(ToOption)
                .Where(static option => !IsMapOption(option))
                .GroupBy(static option => $"{option.Kind}|{option.Label}|{option.Value}", StringComparer.OrdinalIgnoreCase)
                .Select(static group => group.First())
                .ToList();
        }

        return Array.Empty<AdvisorSceneOption>();
    }

    private static AdvisorSceneOption ToOption(LiveExportChoiceSummary choice)
    {
        var tags = choice.SemanticHints
            .Where(static hint => !string.IsNullOrWhiteSpace(hint))
            .ToList();
        if (!string.IsNullOrWhiteSpace(choice.BindingKind))
        {
            tags.Add($"binding-kind:{choice.BindingKind}");
        }

        if (!string.IsNullOrWhiteSpace(choice.BindingId))
        {
            tags.Add($"binding-id:{choice.BindingId}");
        }

        if (!string.IsNullOrWhiteSpace(choice.NodeId))
        {
            tags.Add($"node:{choice.NodeId}");
        }

        return new AdvisorSceneOption(
            choice.Label,
            choice.Kind,
            choice.Value,
            choice.Description,
            choice.Enabled != false,
            tags.Count == 0 ? Array.Empty<string>() : tags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static List<AdvisorSceneOption> NormalizeRewardEntryOptions(IEnumerable<AdvisorSceneOption> options)
    {
        return options
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

    private static string ResolveSceneType(CompanionRunState runState)
    {
        var snapshot = runState.Snapshot;
        var foregroundOwner = NormalizeSceneToken(TryGetMeta(snapshot.Meta, "foregroundOwner"));
        if (IsAdvisorSceneType(foregroundOwner))
        {
            return foregroundOwner!;
        }

        if (HasRewardSceneMarkers(snapshot))
        {
            return "reward";
        }

        if (HasEventSceneMarkers(snapshot))
        {
            return "event";
        }

        if (HasRestSiteSceneMarkers(snapshot))
        {
            return "rest-site";
        }

        if (HasShopSceneMarkers(snapshot))
        {
            return "shop";
        }

        if (HasMapSceneMarkers(snapshot))
        {
            return "map";
        }

        if (HasCombatSceneMarkers(snapshot))
        {
            return "combat";
        }

        var normalized = NormalizeSceneToken(runState.NormalizedState.Scene.SemanticSceneType);
        if (IsAdvisorSceneType(normalized))
        {
            return normalized!;
        }

        var choiceDerived = DeriveSceneTypeFromChoices(snapshot.CurrentChoices);
        if (choiceDerived is not null)
        {
            return choiceDerived;
        }

        return NormalizeSceneToken(snapshot.CurrentScreen) ?? "unknown";
    }

    private static string ResolveCanonicalOwner(CompanionRunState runState, string fallbackOwner)
    {
        var foregroundOwner = NormalizeSceneToken(TryGetMeta(runState.Snapshot.Meta, "foregroundOwner"));
        if (IsAdvisorSceneType(foregroundOwner))
        {
            return foregroundOwner!;
        }

        var normalized = NormalizeSceneToken(runState.NormalizedState.Scene.SemanticSceneType);
        if (IsAdvisorSceneType(normalized))
        {
            return normalized!;
        }

        return fallbackOwner;
    }

    private static string ResolveCombatStage(LiveExportSnapshot snapshot, IReadOnlyList<LiveExportChoiceSummary> handChoices)
    {
        var targetCount = TryReadIntMeta(snapshot.Meta, "combatTargetCount");
        if ((targetCount ?? 0) > 0 || TryReadBoolMeta(snapshot.Meta, "combatTargetingInProgress") == true)
        {
            return "targeting";
        }

        if (!string.IsNullOrWhiteSpace(TryGetMeta(snapshot.Meta, "combatHandSummary")) || handChoices.Count > 0)
        {
            return "player-play-open";
        }

        return "combat-runtime";
    }

    private static string ResolveRewardStage(CompanionRunState runState, int rewardEntryCount, bool childChoicePresent)
    {
        if (childChoicePresent)
        {
            return "child-choice";
        }

        if (rewardEntryCount > 0)
        {
            return "claim";
        }

        if (string.Equals(ResolveCanonicalOwner(runState, "reward"), "reward", StringComparison.OrdinalIgnoreCase))
        {
            return "released";
        }

        return "released";
    }

    private static string ResolveEventStage(CompanionRunState runState, IReadOnlyList<AdvisorSceneOption> visibleOptions)
    {
        var lane = NormalizeLane(TryGetMeta(runState.Snapshot.Meta, "foregroundActionLane"));
        if (!string.IsNullOrWhiteSpace(lane) && !string.Equals(lane, "none", StringComparison.OrdinalIgnoreCase))
        {
            return lane!;
        }

        if (visibleOptions.Count > 0)
        {
            var ancientDetected = TryReadBoolMeta(runState.Snapshot.Meta, "ancientEventDetected") == true
                                  || !string.IsNullOrWhiteSpace(TryGetMeta(runState.Snapshot.Meta, "ancientPhase"));
            if (ancientDetected)
            {
                return "ancient-option";
            }

            return visibleOptions.Any(IsProceedLikeOption) ? "event-proceed" : "event-choice";
        }

        return "event-runtime";
    }

    private static string ResolveRestSiteStage(LiveExportSnapshot snapshot, IReadOnlyList<AdvisorSceneOption> options)
    {
        if (TryReadBoolMeta(snapshot.Meta, "restSiteUpgradeScreenVisible") == true)
        {
            return TryReadBoolMeta(snapshot.Meta, "restSiteUpgradeConfirmVisible") == true
                ? "smith-confirm"
                : "smith-grid";
        }

        if (TryReadBoolMeta(snapshot.Meta, "restSiteProceedVisible") == true)
        {
            return "proceed";
        }

        var selectionOutcome = TryGetMeta(snapshot.Meta, "restSiteSelectionOutcome");
        var selectionStatus = NormalizeLane(TryGetMeta(snapshot.Meta, "restSiteSelectionCurrentStatus"));
        if (IsRestSelectionSettling(selectionOutcome, selectionStatus))
        {
            return "selection-settling";
        }

        if (TryReadBoolMeta(snapshot.Meta, "restSiteButtonsVisible") == true || options.Count > 0)
        {
            return "explicit-choice";
        }

        return "release-pending";
    }

    private static string? ResolveEventIdentity(CompanionRunState runState)
    {
        if (TryReadBoolMeta(runState.Snapshot.Meta, "ancientEventDetected") == true
            || !string.IsNullOrWhiteSpace(TryGetMeta(runState.Snapshot.Meta, "ancientPhase")))
        {
            return "ancient-event";
        }

        return runState.NormalizedState.Event.EventId
               ?? TryGetMeta(runState.Snapshot.Meta, "eventId")
               ?? TryGetMeta(runState.Snapshot.Meta, "event-id")
               ?? TryGetMeta(runState.Snapshot.Meta, "eventTitle");
    }

    private static bool HasRewardSceneMarkers(LiveExportSnapshot snapshot)
    {
        return TryGetMeta(snapshot.Meta, "rewardProceedVisible") is not null
               || TryGetMeta(snapshot.Meta, "foregroundActionLane")?.Contains("reward", StringComparison.OrdinalIgnoreCase) == true
               || DeriveSceneTypeFromChoices(snapshot.CurrentChoices) == "reward";
    }

    private static bool HasEventSceneMarkers(LiveExportSnapshot snapshot)
    {
        return TryGetMeta(snapshot.Meta, "ancientPhase") is not null
               || TryGetMeta(snapshot.Meta, "eventProceedOptionVisible") is not null
               || TryGetMeta(snapshot.Meta, "foregroundActionLane")?.Contains("event", StringComparison.OrdinalIgnoreCase) == true
               || DeriveSceneTypeFromChoices(snapshot.CurrentChoices) == "event";
    }

    private static bool HasRestSiteSceneMarkers(LiveExportSnapshot snapshot)
    {
        return snapshot.Meta.Keys.Any(static key => key.StartsWith("restSite", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasShopSceneMarkers(LiveExportSnapshot snapshot)
    {
        return snapshot.Meta.Keys.Any(static key => key.StartsWith("shop", StringComparison.OrdinalIgnoreCase))
               || DeriveSceneTypeFromChoices(snapshot.CurrentChoices) == "shop";
    }

    private static bool HasMapSceneMarkers(LiveExportSnapshot snapshot)
    {
        return TryGetMeta(snapshot.Meta, "mapReleaseAuthority") is not null
               || TryGetMeta(snapshot.Meta, "mapSurfacePending") is not null
               || TryGetMeta(snapshot.Meta, "mapPointCount") is not null
               || DeriveSceneTypeFromChoices(snapshot.CurrentChoices) == "map";
    }

    private static bool HasCombatSceneMarkers(LiveExportSnapshot snapshot)
    {
        return snapshot.Encounter?.InCombat == true
               || TryGetMeta(snapshot.Meta, "combatHandSummary") is not null
               || TryGetMeta(snapshot.Meta, "combatTargetCount") is not null;
    }

    private static string? DeriveSceneTypeFromChoices(IReadOnlyList<LiveExportChoiceSummary> choices)
    {
        if (choices.Any(static choice => string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)))
        {
            return "map";
        }

        if (choices.Any(IsEventChoice))
        {
            return "event";
        }

        if (choices.Any(IsShopChoice))
        {
            return "shop";
        }

        if (choices.Any(IsRestSiteChoice))
        {
            return "rest-site";
        }

        if (choices.Any(IsRewardEntryChoice) || choices.Any(IsRewardChildChoice))
        {
            return "reward";
        }

        if (choices.Any(IsCombatHandChoice))
        {
            return "combat";
        }

        return null;
    }

    private static bool IsAdvisorSceneType(string? value)
    {
        return value is "combat" or "reward" or "event" or "rest-site" or "shop" or "map";
    }

    private static string? NormalizeSceneToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "rewards" => "reward",
            "rest" => "rest-site",
            "campfire" => "rest-site",
            _ => value.Trim().ToLowerInvariant(),
        };
    }

    private static string? NormalizeLane(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().Replace('_', '-').ToLowerInvariant();
    }

    private static string? TryGetMeta(IReadOnlyDictionary<string, string?> meta, string key)
    {
        return meta.TryGetValue(key, out var value) ? value : null;
    }

    private static int? TryReadIntMeta(IReadOnlyDictionary<string, string?> meta, string key)
    {
        return meta.TryGetValue(key, out var raw)
               && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static bool? TryReadBoolMeta(IReadOnlyDictionary<string, string?> meta, string key)
    {
        return meta.TryGetValue(key, out var raw)
               && bool.TryParse(raw, out var value)
            ? value
            : null;
    }

    private static bool IsRestSelectionSettling(string? selectionOutcome, string? selectionStatus)
    {
        if (!string.IsNullOrWhiteSpace(selectionOutcome)
            && !string.Equals(selectionOutcome, "success", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(selectionOutcome, "none", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return selectionStatus is "selection-settling" or "transitioning" or "awaiting-release";
    }

    private static bool IsCombatHandChoice(LiveExportChoiceSummary choice)
    {
        return string.Equals(choice.Kind, "card", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.Kind, "combat-hand-card", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEndTurnChoice(LiveExportChoiceSummary choice)
    {
        return choice.Label.Contains("턴 종료", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("End Turn", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Contains("combat-end-turn", StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsRewardEntryChoice(LiveExportChoiceSummary choice)
    {
        if (IsRewardChildChoice(choice) || IsRewardControlChoice(choice))
        {
            return false;
        }

        return choice.SemanticHints.Any(static hint => hint.StartsWith("reward", StringComparison.OrdinalIgnoreCase))
               || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.Kind, "reward", StringComparison.OrdinalIgnoreCase)
               || choice.Kind.EndsWith("reward", StringComparison.OrdinalIgnoreCase)
               || choice.Kind is "card" or "relic" or "potion" or "gold";
    }

    private static bool IsRewardChildChoice(LiveExportChoiceSummary choice)
    {
        return choice.Kind.Contains("reward-pick", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => hint.Contains("reward-pick", StringComparison.OrdinalIgnoreCase)
                                                          || hint.Contains("reward-child", StringComparison.OrdinalIgnoreCase))
               || string.Equals(choice.BindingKind, "reward-pick", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRewardControlChoice(LiveExportChoiceSummary choice)
    {
        return string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.Kind, "skip", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Contains("option-role:proceed", StringComparer.OrdinalIgnoreCase)
               || choice.Label.Contains("넘기기", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("skip", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("continue", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("계속", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEventChoice(LiveExportChoiceSummary choice)
    {
        return string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.BindingKind, "event-option", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => hint.Contains("event", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsRewardLikeOption(AdvisorSceneOption option)
    {
        return option.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || option.Tags.Any(static tag => tag.Contains("reward", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsProceedLikeOption(AdvisorSceneOption option)
    {
        return option.Tags.Any(static tag => tag.Contains("option-role:proceed", StringComparison.OrdinalIgnoreCase))
               || option.Label.Contains("계속", StringComparison.OrdinalIgnoreCase)
               || option.Label.Contains("진행", StringComparison.OrdinalIgnoreCase)
               || option.Label.Contains("continue", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRestSiteChoice(LiveExportChoiceSummary choice)
    {
        return choice.Kind.Contains("rest", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => hint.Contains("rest", StringComparison.OrdinalIgnoreCase)
                                                          || hint.Contains("smith", StringComparison.OrdinalIgnoreCase))
               || choice.Label.Contains("휴식", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("재련", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsShopChoice(LiveExportChoiceSummary choice)
    {
        return choice.Kind.StartsWith("shop", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => hint.Contains("shop", StringComparison.OrdinalIgnoreCase))
               || string.Equals(choice.BindingKind, "shop-option", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBackOrProceedOption(AdvisorSceneOption option)
    {
        return string.Equals(option.Kind, "shop-back", StringComparison.OrdinalIgnoreCase)
               || string.Equals(option.Kind, "shop-proceed", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsShopServiceOption(AdvisorSceneOption option)
    {
        return option.Kind.Contains("service", StringComparison.OrdinalIgnoreCase)
               || option.Kind.Contains("card-removal", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsShopItemOption(AdvisorSceneOption option)
    {
        return option.Kind.StartsWith("shop-option", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMapOption(AdvisorSceneOption option)
    {
        return string.Equals(option.Kind, "map-node", StringComparison.OrdinalIgnoreCase);
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
