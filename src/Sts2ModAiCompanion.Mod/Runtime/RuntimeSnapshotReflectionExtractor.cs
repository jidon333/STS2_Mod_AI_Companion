using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModAiCompanion.Mod.Runtime;

internal static class RuntimeSnapshotReflectionExtractor
{
    private static readonly string[] SingletonPropertyNames = { "Instance", "Current", "Singleton", "State" };
    private static readonly string[] NestedRootNames = { "Run", "RunState", "State", "Player", "PlayerCombatState", "Character", "Encounter", "Combat", "CombatState", "Manager" };
    private static readonly string[] SceneNodeKeywords =
    {
        "Screen",
        "Room",
        "Map",
        "Reward",
        "Event",
        "Combat",
        "Battle",
        "Merchant",
        "Shop",
        "Rest",
        "Menu",
        "Run",
        "Player",
        "Character",
        "Choice",
        "Card",
    };

    private static readonly string[] ChoiceMemberNames =
    {
        "Option",
        "Reward",
        "Entry",
        "Card",
        "Model",
        "CardNode",
        "CardModel",
        "Choices",
        "Options",
        "Rewards",
        "Buttons",
        "RewardButtons",
        "RewardAlternatives",
        "CurrentOptions",
        "Cards",
        "MerchantSlots",
        "Inventory",
        "CharacterCardEntries",
        "ColorlessCardEntries",
        "RelicEntries",
        "PotionEntries",
        "CardRemovalEntry",
        "_options",
        "_extraOptions",
        "_rewardButtons",
        "_rewardAlternativesContainer",
        "_cardRow",
        "_characterCardContainer",
        "_colorlessCardContainer",
        "_relicContainer",
        "_potionContainer",
        "_cardRemovalNode",
        "_slotsContainer",
        "_label",
        "_titleLabel",
        "_descriptionLabel",
    };

    private static readonly HashSet<string> PlaceholderChoiceLabels = new(StringComparer.Ordinal)
    {
        "BackButton",
        "Back",
        "SendButton",
        "PlayQueue",
        "Card",
        "CardGrid",
        "RewardsScreen",
        "NCardRewardSelectionScreen",
        "CardTrailIronclad",
        "Hitbox",
        "RelicInventory",
        "CardPreviewContainer",
        "GridCardPreviewContainer",
        "EventCardPreviewContainer",
        "MessyCardPreviewContainer",
        "MerchantCardHolder",
        "MerchantRelicHolder",
        "MerchantPotionHolder",
        "HappyCultist",
        "Heart",
        "SlimeSad",
    };

    private sealed record RuntimeSceneProbe(
        string? SceneName,
        string? CurrentSceneType,
        string? SceneRootType,
        string? Screen,
        int TraversedNodeCount,
        IReadOnlyList<string> VisibleNodeTypes,
        IReadOnlyList<string> InterestingNodeTypes);

    private sealed record ChoiceExtractionResult(
        IReadOnlyList<LiveExportChoiceSummary> Choices,
        IReadOnlyList<LiveExportChoiceCandidate> Candidates,
        LiveExportChoiceDecision Decision)
    {
        public IReadOnlyDictionary<string, string?> Meta { get; init; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    }

    private sealed record RestSiteExtractorCandidate(
        object Source,
        string TypeName,
        string SourceKind,
        string OptionId,
        string Label,
        string? Description,
        string? ScreenBounds,
        bool? Enabled,
        string? IconAssetPath,
        int Score,
        string? RejectReason,
        LiveExportChoiceSummary Summary);

    private sealed record RestSiteButtonObservation(
        int VisibleButtonCount,
        string? HoveredOptionId,
        string? ExecutingOptionId,
        bool OptionsInteractive,
        string? VisibleOptionSummary);

    private sealed record RestSiteUpgradeCardCandidate(
        object Holder,
        string Label,
        string? CardId,
        string ScreenBounds,
        bool Enabled,
        string BoundsSource);

    private sealed record RestSiteUpgradeObservation(
        bool ScreenDetected,
        bool ScreenVisible,
        IReadOnlyList<RestSiteUpgradeCardCandidate> VisibleCards,
        bool ConfirmVisible,
        bool ConfirmEnabled,
        string? ConfirmBounds,
        int SelectedCardCount,
        string? SelectedCardsSummary,
        string? PreviewMode,
        bool ObserverMiss);

    private sealed record CardSelectionCardCandidate(
        object Holder,
        string Label,
        string? CardId,
        string ScreenBounds,
        bool Enabled,
        bool Selected,
        string BoundsSource);

    private sealed record CardSelectionObservation(
        string ScreenType,
        bool ScreenDetected,
        bool ScreenVisible,
        string? RootType,
        string? Prompt,
        int? MinSelect,
        int? MaxSelect,
        int SelectedCount,
        bool? RequireManualConfirmation,
        bool? Cancelable,
        bool PreviewVisible,
        bool MainConfirmEnabled,
        bool PreviewConfirmEnabled,
        string? MainConfirmBounds,
        string? PreviewConfirmBounds,
        string? PreviewMode,
        IReadOnlyList<string> SelectedCardIds,
        IReadOnlyList<CardSelectionCardCandidate> VisibleCards);

    private sealed record TreasureRoomRelicHolderCandidate(
        object Holder,
        string Label,
        string? RelicId,
        string ScreenBounds,
        bool Enabled,
        string BoundsSource);

    private sealed record TreasureRoomObservation(
        bool RoomDetected,
        bool RoomVisible,
        string? RootType,
        bool ChestClickable,
        bool ChestOpened,
        bool RelicCollectionOpen,
        bool SharedRelicPickingActive,
        bool MapScreenOpen,
        string? ChestBounds,
        int RelicHolderCount,
        int VisibleRelicHolderCount,
        int EnabledRelicHolderCount,
        bool ProceedEnabled,
        string? ProceedBounds,
        IReadOnlyList<string> RelicHolderIds,
        IReadOnlyList<TreasureRoomRelicHolderCandidate> VisibleRelicHolders);

    private sealed record ShopOptionCandidate(
        object Slot,
        string OptionType,
        string Label,
        string? EntryId,
        string ScreenBounds,
        bool Enabled,
        bool IsStocked,
        bool EnoughGold,
        bool Used,
        string BoundsSource);

    private sealed record ShopObservation(
        bool RoomDetected,
        bool RoomVisible,
        bool ForegroundOwned,
        bool TeardownInProgress,
        bool ShopIsCurrentActiveScreen,
        bool MapIsCurrentActiveScreen,
        string? ActiveScreenType,
        string? RootType,
        bool InventoryOpen,
        bool MerchantButtonVisible,
        bool MerchantButtonEnabled,
        string? MerchantButtonBounds,
        bool ProceedEnabled,
        string? ProceedBounds,
        bool BackVisible,
        bool BackEnabled,
        string? BackBounds,
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
        IReadOnlyList<ShopOptionCandidate> VisibleOptions);

    private sealed record RewardObservation(
        bool ScreenDetected,
        bool ScreenVisible,
        bool ForegroundOwned,
        bool TeardownInProgress,
        bool RewardIsCurrentActiveScreen,
        bool RewardIsTopOverlay,
        bool MapIsCurrentActiveScreen,
        string? ActiveScreenType,
        string? TopOverlayType,
        string? RootType,
        bool ProceedVisible,
        bool ProceedEnabled,
        int VisibleButtonCount,
        int EnabledButtonCount,
        bool? HasOpenPotionSlots,
        bool TerminalRunBoundary,
        bool GameOverScreenDetected,
        bool UnlockScreenDetected,
        bool TimelineUnlockDetected,
        bool MainMenuReturnDetected);

    private sealed record RootSceneTransitionObservation(
        bool TransitionInProgress,
        string? RootSceneCurrentType,
        bool RootSceneIsMainMenu,
        bool RootSceneIsRun,
        bool CurrentRunNodePresent,
        string? CurrentRunRoomType,
        string? CurrentRunRoomSceneType);

    public static LiveExportObservation Capture(
        RuntimeHookBinding binding,
        AiCompanionRuntimeConfig config,
        object? instance,
        object?[]? args,
        object? result)
    {
        var observedAt = DateTimeOffset.UtcNow;
        var roots = GatherRoots(instance, args, result);
        var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["method"] = binding.Method.Name,
            ["declaringType"] = binding.Method.DeclaringType?.FullName,
            ["hookCategory"] = binding.Candidate.Category,
        };
        var meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["semanticKind"] = binding.Candidate.SemanticKind,
            ["declaringType"] = binding.Method.DeclaringType?.FullName,
        };

        if (instance is not null)
        {
            meta["instanceType"] = instance.GetType().FullName;
        }

        AppendRestSiteLifecycleMetadata(binding, observedAt, args, payload, meta);

        return BuildObservation(
            binding.Candidate.SemanticKind,
            observedAt,
            roots,
            config,
            binding.Candidate.ScreenOverride,
            instance,
            args,
            payload,
            meta,
            instance?.GetType().FullName,
            binding.Method.DeclaringType?.FullName);
    }

    public static LiveExportObservation? CaptureFromRuntime(AiCompanionRuntimeConfig config)
    {
        var observedAt = DateTimeOffset.UtcNow;
        var roots = GatherRuntimeRoots(config, out var probe);
        if (roots.Count == 0)
        {
            return null;
        }

        var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["source"] = "scene-poll",
            ["rootCount"] = roots.Count,
        };
        if (!string.IsNullOrWhiteSpace(probe.SceneName))
        {
            payload["sceneName"] = probe.SceneName;
        }

        if (!string.IsNullOrWhiteSpace(probe.CurrentSceneType))
        {
            payload["currentSceneType"] = probe.CurrentSceneType;
        }

        if (!string.IsNullOrWhiteSpace(probe.SceneRootType))
        {
            payload["sceneRootType"] = probe.SceneRootType;
        }

        payload["traversedNodeCount"] = probe.TraversedNodeCount;

        if (probe.VisibleNodeTypes.Count > 0)
        {
            payload["visibleNodeTypes"] = probe.VisibleNodeTypes.Take(12).ToArray();
        }

        if (probe.InterestingNodeTypes.Count > 0)
        {
            payload["interestingNodeTypes"] = probe.InterestingNodeTypes.Take(16).ToArray();
        }

        var meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["semanticKind"] = "runtime-poll",
            ["source"] = "scene-poll",
            ["currentSceneType"] = probe.CurrentSceneType,
            ["sceneRootType"] = probe.SceneRootType,
        };

        return BuildObservation(
            "runtime-poll",
            observedAt,
            roots,
            config,
            probe.Screen,
            instance: null,
            args: null,
            payload,
            meta,
            probe.SceneName,
            string.Join(" ", probe.VisibleNodeTypes));
    }

    private static LiveExportObservation BuildObservation(
        string triggerKind,
        DateTimeOffset observedAt,
        IReadOnlyList<object> roots,
        AiCompanionRuntimeConfig config,
        string? screenOverride,
        object? instance,
        object?[]? args,
        Dictionary<string, object?> payload,
        Dictionary<string, string?> meta,
        params string?[] screenCandidates)
    {
        var runId = ResolveRunId(roots);
        var act = TryReadInt(roots, "Act", "ActNum", "CurrentAct");
        var floor = TryReadInt(roots, "Floor", "CurrentFloor", "FloorNum");
        var player = ExtractPlayerSummary(roots);
        var hasOpenPotionSlots = TryReadHasOpenPotionSlots(roots);
        var deck = ExtractDeck(roots, config.LiveExport.MaxDeckEntries);
        var relics = ExtractStringList(roots, config.LiveExport.MaxDeckEntries, "Relics", "OwnedRelics", "InventoryRelics");
        var potions = ExtractStringList(roots, config.LiveExport.MaxChoiceEntries, "Potions", "OwnedPotions", "PotionSlots");
        var rootSceneObservation = ObserveRootSceneTransition(roots);
        var currentActiveScreen = TryResolveCurrentActiveScreen(roots);
        var currentActiveScreenType = currentActiveScreen?.GetType().FullName ?? currentActiveScreen?.GetType().Name;
        var choiceResult = ExtractChoices(
            triggerKind,
            screenOverride,
            instance,
            args,
            roots,
            config.LiveExport.MaxChoiceEntries,
            currentActiveScreen,
            currentActiveScreenType,
            rootSceneObservation);
        var choices = choiceResult.Choices.ToList();
        var encounter = ExtractEncounter(roots, meta);
        var combatHand = encounter?.InCombat == true
            ? ExtractCombatHand(roots, config.LiveExport.MaxChoiceEntries)
            : Array.Empty<LiveExportCardSummary>();
        var rawCurrentScreenValue = TryReadString(roots, "CurrentScreen", "Screen", "ScreenName");
        var rawRoomTypeValue = TryReadString(roots, "RoomType");
        var rootTypeSummary = string.Join(
            " ",
            roots
                .Select(root => root.GetType().FullName ?? root.GetType().Name)
                .Distinct(StringComparer.Ordinal)
                .Take(96));
        // rootTypeSummary is a broad global type bag that always includes singleton managers.
        // Keep it for diagnostics, but do not let it decide the primary screen.
        var screen = ResolveScreen(screenOverride, roots, screenCandidates);
        if (string.Equals(screen, "unknown", StringComparison.Ordinal)
            && encounter?.InCombat == true)
        {
            screen = "combat";
        }

        AppendRootSceneTransitionMetadata(rootSceneObservation, meta, payload);

        var cardSelectionObservation = ObserveCardSelection(roots, screen);
        AppendCardSelectionRuntimeMetadata(cardSelectionObservation, meta, payload);
        if (cardSelectionObservation.ScreenVisible)
        {
            foreach (var choice in CreateCardSelectionChoices(cardSelectionObservation))
            {
                AddChoiceSummary(choices, choice, config.LiveExport.MaxChoiceEntries);
            }

            var extractorPath = GetCardSelectionExtractorPath(cardSelectionObservation.ScreenType);
            meta["choiceExtractorPath"] = extractorPath;
            meta["rawChoiceExtractorPath"] = extractorPath;
            payload["choiceExtractorPath"] = extractorPath;
            payload["rawChoiceExtractorPath"] = extractorPath;
        }

        var treasureRoomObservation = ObserveTreasureRoom(roots, screen);
        if (treasureRoomObservation.RoomVisible)
        {
            RemoveTreasureInventoryChoiceContamination(choices);
            foreach (var choice in CreateTreasureRoomChoices(treasureRoomObservation))
            {
                AddChoiceSummary(choices, choice, config.LiveExport.MaxChoiceEntries);
            }

            meta["choiceExtractorPath"] = "treasure";
            payload["choiceExtractorPath"] = "treasure";
        }
        AppendTreasureRoomRuntimeMetadata(treasureRoomObservation, choices, meta, payload);

        var shopObservation = ObserveShopRoom(roots, screen);
        if (shopObservation.ForegroundOwned)
        {
            RemoveShopChoiceContamination(choices, shopObservation);
            var mergedChoices = new List<LiveExportChoiceSummary>();
            foreach (var choice in CreateShopChoices(shopObservation))
            {
                AddChoiceSummary(mergedChoices, choice, config.LiveExport.MaxChoiceEntries);
            }

            foreach (var choice in choices)
            {
                AddChoiceSummary(mergedChoices, choice, config.LiveExport.MaxChoiceEntries);
            }

            choices.Clear();
            choices.AddRange(mergedChoices);

            meta["choiceExtractorPath"] = "shop";
            meta["rawChoiceExtractorPath"] = "shop";
            payload["choiceExtractorPath"] = "shop";
            payload["rawChoiceExtractorPath"] = "shop";
        }
        AppendShopRuntimeMetadata(shopObservation, meta, payload);

        var rewardObservation = ObserveRewardScreen(roots, screen);
        AppendRewardRuntimeMetadata(rewardObservation, meta, payload);
        if (hasOpenPotionSlots is not null)
        {
            meta["hasOpenPotionSlots"] = hasOpenPotionSlots == true ? "true" : "false";
            payload["hasOpenPotionSlots"] = hasOpenPotionSlots == true;
        }
        if (rewardObservation.ForegroundOwned)
        {
            meta["choiceExtractorPath"] = "reward";
            meta["rawChoiceExtractorPath"] = "reward";
            payload["choiceExtractorPath"] = "reward";
            payload["rawChoiceExtractorPath"] = "reward";
        }

        AppendEventProceedRuntimeMetadata(choices, screen, meta, payload);

        var warnings = BuildWarnings(player, deck, relics, potions, choices);

        AppendChoiceExtractionMetadata(choiceResult, meta, payload);

        meta["screen"] = screen;
        meta["publishedCurrentScreen"] = screen;
        meta["rawObservedScreen"] = screen;
        meta["rootTypeSummary"] = rootTypeSummary;
        meta["rawRootTypeSummary"] = rootTypeSummary;
        payload["publishedCurrentScreen"] = screen;
        payload["rawObservedScreen"] = screen;
        payload["rawRootTypeSummary"] = rootTypeSummary;
        if (!string.IsNullOrWhiteSpace(rawCurrentScreenValue))
        {
            meta["rawCurrentScreen"] = rawCurrentScreenValue;
            meta["rawCurrentScreenValue"] = rawCurrentScreenValue;
            payload["rawCurrentScreen"] = rawCurrentScreenValue;
            payload["rawCurrentScreenValue"] = rawCurrentScreenValue;
        }

        if (!string.IsNullOrWhiteSpace(currentActiveScreenType))
        {
            meta["rawCurrentActiveScreenType"] = currentActiveScreenType;
            payload["rawCurrentActiveScreenType"] = currentActiveScreenType;
        }

        if (!string.IsNullOrWhiteSpace(rawRoomTypeValue))
        {
            meta["rawRoomTypeValue"] = rawRoomTypeValue;
            payload["rawRoomTypeValue"] = rawRoomTypeValue;
        }

        if (!string.IsNullOrWhiteSpace(choiceResult.Decision.ExtractorPath)
            && !cardSelectionObservation.ScreenVisible)
        {
            meta["choiceExtractorPath"] = choiceResult.Decision.ExtractorPath;
            meta["rawChoiceExtractorPath"] = choiceResult.Decision.ExtractorPath;
            payload["choiceExtractorPath"] = choiceResult.Decision.ExtractorPath;
            payload["rawChoiceExtractorPath"] = choiceResult.Decision.ExtractorPath;
        }

        if (combatHand.Count > 0)
        {
            meta["combatHandSummary"] = FormatCombatHandSummary(combatHand);
            meta["combatHandCount"] = combatHand.Count.ToString(CultureInfo.InvariantCulture);
        }

        var combatTargetChoices = choices
            .Where(choice => string.Equals(choice.Kind, "enemy-target", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (combatTargetChoices.Length > 0)
        {
            meta["combatTargetCount"] = combatTargetChoices.Length.ToString(CultureInfo.InvariantCulture);
            meta["combatTargetCoordinateSpace"] = "logical-render";
            meta["combatTargetClickCoordinateSpace"] = "current-window-normalized";
            meta["combatTargetSummary"] = string.Join(
                ";",
                combatTargetChoices.Select(choice => $"{choice.NodeId ?? choice.Label}@logical:{choice.ScreenBounds}@normalized:{FormatNormalizedBounds(choice.ScreenBounds)}"));
        }

        var mapPointChoices = choices
            .Where(choice => string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var mapPointCandidates = choiceResult.Candidates
            .Where(static candidate => string.Equals(candidate.ExtractorPath, "map", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (mapPointChoices.Length > 0)
        {
            meta["mapPointCount"] = mapPointChoices.Length.ToString(CultureInfo.InvariantCulture);
            meta["mapPointSummary"] = string.Join(
                ";",
                mapPointChoices.Select(choice => $"{choice.NodeId ?? choice.Label}@logical:{choice.ScreenBounds}@normalized:{FormatNormalizedBounds(choice.ScreenBounds)}"));
            if (string.Equals(screen, "event", StringComparison.OrdinalIgnoreCase))
            {
                meta["mapOverlayVisible"] = "true";
            }
        }
        else if (mapPointCandidates.Length > 0)
        {
            meta["mapPointCount"] = mapPointCandidates.Length.ToString(CultureInfo.InvariantCulture);
            payload["mapPointCount"] = mapPointCandidates.Length;
            var rejectSummary = BuildMapPointRejectSummary(mapPointCandidates) ?? choiceResult.Decision.FailureReason;
            if (!string.IsNullOrWhiteSpace(rejectSummary))
            {
                meta["mapPointRejectSummary"] = rejectSummary;
                payload["mapPointRejectSummary"] = rejectSummary;
            }
        }

        if (act is not null)
        {
            payload["act"] = act;
        }

        if (floor is not null)
        {
            payload["floor"] = floor;
        }

        if (choices.Count > 0)
        {
            payload["choiceCount"] = choices.Count;
            payload["choiceSignature"] = BuildChoiceSignature(choices);
        }

        AppendRestSiteRuntimeMetadata(roots, choices, screen, meta, payload);
        AppendCombatRuntimeMetadata(roots, encounter, meta, payload);

        return new LiveExportObservation(
            triggerKind,
            observedAt,
            runId,
            InferRunStatus(triggerKind, encounter, screen),
            screen,
            act,
            floor,
            player,
            deck,
            relics,
            potions,
            choices,
            warnings,
            encounter,
            payload,
            meta)
        {
            ChoiceCandidates = choiceResult.Candidates,
            ChoiceDecision = choiceResult.Decision,
            SemanticScreen = IsSemanticHighValueScreen(triggerKind, screen) ? screen : null,
        };
    }

    private static string? ResolveRunId(IReadOnlyList<object> roots)
    {
        var exact = TryReadString(roots, "RunId");
        if (IsPlausibleRunId(exact))
        {
            return exact;
        }

        var candidateRoots = new List<object>();
        foreach (var root in roots)
        {
            if (LooksLikeRunContext(root))
            {
                AddIfUseful(candidateRoots, root);
            }

            AddIfUseful(candidateRoots, TryGetMemberValue(root, "Run"));
            AddIfUseful(candidateRoots, TryGetMemberValue(root, "RunState"));
            AddIfUseful(candidateRoots, TryGetMemberValue(root, "Save"));
            AddIfUseful(candidateRoots, TryGetMemberValue(root, "SaveState"));
        }

        foreach (var root in candidateRoots)
        {
            foreach (var memberName in new[] { "RunId", "SaveId", "Seed", "Id" })
            {
                var candidate = TryReadString(root, memberName);
                if (IsPlausibleRunId(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static bool LooksLikeRunContext(object root)
    {
        var typeName = root is Type type
            ? type.FullName ?? type.Name
            : root.GetType().FullName ?? root.GetType().Name;

        return typeName.Contains("Run", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Save", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Session", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Profile", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlausibleRunId(string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        if (candidate.StartsWith("pending-", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (candidate.StartsWith("CARD.", StringComparison.OrdinalIgnoreCase)
            || candidate.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase)
            || candidate.StartsWith("POTION.", StringComparison.OrdinalIgnoreCase)
            || candidate.StartsWith("EVENT.", StringComparison.OrdinalIgnoreCase)
            || candidate.StartsWith("OPTION_", StringComparison.OrdinalIgnoreCase)
            || candidate.EndsWith("_POWER", StringComparison.OrdinalIgnoreCase)
            || candidate.EndsWith("_BUTTON", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (candidate.All(char.IsDigit))
        {
            return true;
        }

        return candidate.Length >= 8
               && candidate.All(character => char.IsLetterOrDigit(character) || character is '-' or '_');
    }

    private static IReadOnlyList<object> GatherRoots(object? instance, object?[]? args, object? result)
    {
        var roots = new List<object>();
        AddIfUseful(roots, instance);
        AddIfUseful(roots, result);

        if (args is not null)
        {
            foreach (var arg in args)
            {
                AddIfUseful(roots, arg);
            }
        }

        foreach (var typeName in new[]
                 {
                     "MegaCrit.Sts2.Core.Runs.RunManager",
                     "MegaCrit.Sts2.Core.Runs.RunState",
                     "MegaCrit.Sts2.Core.Combat.CombatManager",
                     "MegaCrit.Sts2.Core.Combat.CombatState",
                     "MegaCrit.Sts2.Core.Saves.SaveManager",
                     "MegaCrit.Sts2.Core.Saves.Managers.RunSaveManager",
                 })
        {
            var type = RuntimeTypeLocator.TryFindType(typeName);
            if (type is null)
            {
                continue;
            }

            AddIfUseful(roots, type);
            foreach (var singletonName in SingletonPropertyNames)
            {
                AddIfUseful(roots, TryGetMemberValue(type, singletonName));
            }
        }

        for (var index = 0; index < roots.Count; index += 1)
        {
            foreach (var nestedRootName in NestedRootNames)
            {
                AddIfUseful(roots, TryGetMemberValue(roots[index], nestedRootName));
            }
        }

        return roots;
    }

    private static IReadOnlyList<object> GatherRuntimeRoots(AiCompanionRuntimeConfig config, out RuntimeSceneProbe probe)
    {
        var roots = new List<object>(GatherRoots(instance: null, args: null, result: null));
        var visibleNodeTypes = new List<string>();
        var interestingNodeTypes = new List<string>();
        var traversedNodeCount = 0;

        var mainLoop = TryResolveMainLoop();
        AddIfUseful(roots, mainLoop);

        var sceneRoot = mainLoop is null ? null : TryGetMemberValue(mainLoop, "Root");
        var currentScene = mainLoop is null ? null : TryGetMemberValue(mainLoop, "CurrentScene");
        AddIfUseful(roots, sceneRoot);
        AddIfUseful(roots, currentScene);

        var traversalRoot = currentScene ?? sceneRoot;
        foreach (var node in EnumerateSceneNodes(traversalRoot, config.LiveExport.ScenePollingMaxNodes))
        {
            traversedNodeCount += 1;
            if (!IsInterestingSceneNode(node))
            {
                continue;
            }

            interestingNodeTypes.Add(node.GetType().FullName ?? node.GetType().Name);
            AddIfUseful(roots, node);
            foreach (var nestedRootName in NestedRootNames.Concat(new[] { "Screen", "Room", "Layout", "Model", "Controller" }))
            {
                AddIfUseful(roots, TryGetMemberValue(node, nestedRootName));
            }

            if (IsVisibleNode(node))
            {
                visibleNodeTypes.Add(node.GetType().FullName ?? node.GetType().Name);
            }
        }

        probe = new RuntimeSceneProbe(
            TryReadString(currentScene ?? sceneRoot, "Name", "SceneFilePath", "SceneFile"),
            currentScene?.GetType().FullName,
            sceneRoot?.GetType().FullName,
            InferScreen(
                TryReadString(currentScene ?? sceneRoot, "Name", "SceneFilePath", "SceneFile"),
                currentScene?.GetType().FullName,
                sceneRoot?.GetType().FullName,
                string.Join(" ", visibleNodeTypes.Take(96)),
                string.Join(" ", interestingNodeTypes.Take(96))),
            traversedNodeCount,
            visibleNodeTypes
                .Distinct(StringComparer.Ordinal)
                .Take(24)
                .ToArray(),
            interestingNodeTypes
                .Distinct(StringComparer.Ordinal)
                .Take(32)
                .ToArray());
        return roots;
    }

    private static LiveExportPlayerSummary ExtractPlayerSummary(IEnumerable<object> roots)
    {
        var playerRoots = roots
            .SelectMany(root => new[]
            {
                root,
                TryGetMemberValue(root, "Player"),
                TryGetMemberValue(root, "Character"),
                TryGetMemberValue(root, "Hero"),
                TryGetMemberValue(root, "Creature"),
            })
            .Where(root => root is not null)
            .Cast<object>()
            .Where(IsAuthoritativePlayerRoot)
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();
        if (playerRoots.Length == 0)
        {
            playerRoots = FindRoots(roots, "Player", "Character", "Hero", "Creature")
                .Where(IsAuthoritativePlayerRoot)
                .DistinctBy(RuntimeHelpers.GetHashCode)
                .ToArray();
        }

        var playerCombatRoots = roots
            .SelectMany(root => new[]
            {
                TryGetMemberValue(root, "PlayerCombatState"),
                TryGetMemberValue(root, "CombatState"),
            })
            .Concat(FindRoots(roots, "PlayerCombatState"))
            .Where(root => root is not null)
            .Cast<object>()
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();

        var name = TryReadString(playerRoots, "Name", "CharacterName", "DisplayName");
        if (LooksLikeNonAuthoritativePlayerName(name))
        {
            name = null;
        }

        var currentHp = TryReadInt(playerRoots, "CurrentHp", "CurrentHealth", "Hp", "Health");
        var maxHp = TryReadInt(playerRoots, "MaxHp", "MaxHealth", "HealthMax");
        var gold = TryReadInt(playerRoots, "Gold", "CurrentGold");
        var energy = TryReadInt(playerRoots, "Energy", "CurrentEnergy")
                     ?? TryReadInt(playerCombatRoots, "Energy", "CurrentEnergy", "_energy");
        var resources = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in new[] { "Block", "Strength", "Dexterity", "Focus", "Mana", "Miracle", "OrbSlots" })
        {
            var value = TryReadString(playerRoots, key)
                        ?? TryReadString(playerCombatRoots, key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                resources[key] = value;
            }
        }

        return new LiveExportPlayerSummary(name, currentHp, maxHp, gold, energy, resources);
    }

    private static bool? TryReadHasOpenPotionSlots(IEnumerable<object> roots)
    {
        var playerRoots = roots
            .SelectMany(root => new[]
            {
                root,
                TryGetMemberValue(root, "Player"),
                TryGetMemberValue(root, "Character"),
                TryGetMemberValue(root, "Hero"),
                TryGetMemberValue(root, "Creature"),
            })
            .Where(root => root is not null)
            .Cast<object>()
            .Where(IsAuthoritativePlayerRoot)
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();
        if (playerRoots.Length == 0)
        {
            playerRoots = FindRoots(roots, "Player", "Character", "Hero", "Creature")
                .Where(IsAuthoritativePlayerRoot)
                .DistinctBy(RuntimeHelpers.GetHashCode)
                .ToArray();
        }

        return TryReadBool(playerRoots, "HasOpenPotionSlots");
    }

    private static IReadOnlyList<LiveExportCardSummary> ExtractDeck(IEnumerable<object> roots, int maxEntries)
    {
        var cards = new List<LiveExportCardSummary>();
        var deckRoots = roots
            .Concat(FindRoots(roots, "PlayerCombatState", "CombatState", "Deck", "Hand", "DrawPile", "DiscardPile", "ExhaustPile", "PlayPile"))
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();

        foreach (var item in FindEnumerableItems(
                     deckRoots,
                     "MasterDeck",
                     "Deck",
                     "Cards",
                     "AllCards",
                     "Hand",
                     "DrawPile",
                     "DiscardPile",
                     "ExhaustPile",
                     "PlayPile"))
        {
            if (!LooksLikeCardLikeItem(item))
            {
                continue;
            }

            var name = TryReadString(item, "Name", "CardName", "DisplayName", "Id", "CardId");
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            cards.Add(new LiveExportCardSummary(
                name,
                TryReadString(item, "Id", "CardId", "Name"),
                TryReadInt(item, "Cost", "EnergyCost", "ManaCost"),
                TryReadString(item, "Type", "CardType"),
                TryReadBool(item, "Upgraded", "IsUpgraded")));
        }

        return cards
            .DistinctBy(card => $"{card.Name}|{card.Id}|{card.Cost}|{card.Type}|{card.Upgraded}")
            .Take(maxEntries)
            .ToArray();
    }

    private static IReadOnlyList<LiveExportCardSummary> ExtractCombatHand(IEnumerable<object> roots, int maxEntries)
    {
        var cards = new List<LiveExportCardSummary>();
        var seen = new HashSet<int>();
        var uiHandRoots = roots
            .SelectMany(root => new[]
            {
                TryGetMemberValue(root, "Ui"),
                TryGetMemberValue(root, "Hand"),
            })
            .Concat(FindRoots(roots, "Ui", "Hand"))
            .Where(root => root is not null)
            .Cast<object>()
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();

        foreach (var holder in FindEnumerableItems(uiHandRoots, "ActiveHolders", "Holders", "CardHolderContainer"))
        {
            if (!seen.Add(RuntimeHelpers.GetHashCode(holder)))
            {
                continue;
            }

            var cardModel = TryGetMemberValue(TryGetMemberValue(holder, "CardNode"), "Model")
                            ?? TryGetMemberValue(holder, "CardModel")
                            ?? TryGetMemberValue(holder, "Model")
                            ?? TryGetMemberValue(holder, "Card");
            if (!LooksLikeCardLikeItem(cardModel))
            {
                continue;
            }

            var summary = TryCreateCardSummary(cardModel);
            if (summary is null)
            {
                continue;
            }

            cards.Add(summary);
            if (cards.Count >= maxEntries)
            {
                return cards;
            }
        }

        var handRoots = roots
            .Concat(FindRoots(roots, "PlayerCombatState", "CombatState", "Hand", "HandCards"))
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();

        foreach (var item in FindEnumerableItems(handRoots, "Hand", "HandCards", "Cards"))
        {
            if (!LooksLikeCardLikeItem(item) || !seen.Add(RuntimeHelpers.GetHashCode(item)))
            {
                continue;
            }

            var summary = TryCreateCardSummary(item);
            if (summary is null)
            {
                continue;
            }

            cards.Add(summary);

            if (cards.Count >= maxEntries)
            {
                break;
            }
        }

        return cards;
    }

    private static LiveExportCardSummary? TryCreateCardSummary(object? item)
    {
        if (!LooksLikeCardLikeItem(item))
        {
            return null;
        }

        var name = TryReadString(item, "Name", "CardName", "DisplayName", "Id", "CardId");
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return new LiveExportCardSummary(
            name,
            TryReadString(item, "Id", "CardId", "Name"),
            TryReadInt(item, "Cost", "CurrentCost", "DisplayedCost", "EnergyCost", "ManaCost"),
            TryReadString(item, "Type", "CardType"),
            TryReadBool(item, "Upgraded", "IsUpgraded"));
    }

    private static string FormatCombatHandSummary(IReadOnlyList<LiveExportCardSummary> cards)
    {
        return string.Join(
            ";",
            cards.Select((card, index) =>
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"{index + 1}:{(card.Id ?? card.Name)}|{card.Type ?? "unknown"}|{(card.Cost?.ToString(CultureInfo.InvariantCulture) ?? "?")}")));
    }

    private static IReadOnlyList<string> ExtractStringList(IEnumerable<object> roots, int maxEntries, params string[] collectionNames)
    {
        return FindEnumerableItems(roots, collectionNames)
            .Select(item => TryReadString(item, "Name", "DisplayName", "Id", "RelicId", "PotionId") ?? item.GetType().Name)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(maxEntries)
            .ToArray();
    }

    private static ChoiceExtractionResult ExtractChoices(
        string triggerKind,
        string? screenHint,
        object? instance,
        object?[]? args,
        IEnumerable<object> roots,
        int maxEntries,
        object? currentActiveScreen,
        string? currentActiveScreenType,
        RootSceneTransitionObservation rootSceneObservation)
    {
        var choiceRoots = new List<object>();
        var candidateItems = new List<object>();
        AddIfUseful(choiceRoots, instance);
        if (args is not null)
        {
            foreach (var arg in args)
            {
                AddIfUseful(choiceRoots, arg);
                foreach (var item in ExpandEnumerable(arg))
                {
                    AddIfUseful(choiceRoots, item);
                    AddIfUseful(candidateItems, item);
                }
            }
        }

        foreach (var root in roots)
        {
            AddIfUseful(choiceRoots, root);
            AddChoiceCandidate(candidateItems, root, maxDepth: 2);
        }

        foreach (var root in roots)
        {
            foreach (var memberName in new[]
                     {
                         "Hand",
                         "HandCards",
                         "RewardContainer",
                         "RewardsContainer",
                         "ChoicesContainer",
                         "CharacterCardContainer",
                         "ColorlessCardContainer",
                         "RelicContainer",
                         "PotionContainer",
                         "CardRemovalNode",
                     })
            {
                var candidate = TryGetMemberValue(root, memberName);
                AddIfUseful(choiceRoots, candidate);
                AddChoiceCandidate(candidateItems, candidate, maxDepth: 3);
                foreach (var item in ExpandEnumerable(candidate))
                {
                    AddIfUseful(choiceRoots, item);
                    AddChoiceCandidate(candidateItems, item, maxDepth: 2);
                }
            }

            foreach (var memberName in ChoiceMemberNames)
            {
                var candidate = TryGetMemberValue(root, memberName);
                AddIfUseful(choiceRoots, candidate);
                AddChoiceCandidate(candidateItems, candidate, maxDepth: 3);
                foreach (var item in ExpandEnumerable(candidate))
                {
                    AddIfUseful(choiceRoots, item);
                    AddChoiceCandidate(candidateItems, item, maxDepth: 2);
                }
            }
        }

        AddMapAuthorityRoots(
            choiceRoots,
            candidateItems,
            roots,
            currentActiveScreen,
            currentActiveScreenType,
            rootSceneObservation);

        foreach (var item in FindEnumerableItems(
                     choiceRoots,
                     "Choices",
                     "Options",
                     "Rewards",
                     "Buttons",
                     "Cards",
                     "CurrentOptions",
                     "RewardButtons",
                     "RewardAlternatives",
                     "Hand",
                     "HandCards",
                     "MerchantSlots",
                     "RewardButtons",
                     "Buttons",
                     "Cards",
                     "CharacterCardEntries",
                     "ColorlessCardEntries",
                     "RelicEntries",
                     "PotionEntries",
                     "_options",
                     "_rewardButtons"))
        {
            AddIfUseful(candidateItems, item);
        }

        var strictAttempts = new List<ChoiceExtractionResult>();

        if (LooksLikeMainMenuContext(triggerKind, screenHint, choiceRoots))
        {
            strictAttempts.Add(ExtractMainMenuChoices(choiceRoots, maxEntries));
        }

        if (LooksLikeRewardContext(triggerKind, screenHint, choiceRoots))
        {
            strictAttempts.Add(
                EvaluateChoiceSet(
                    "reward",
                    CollectRewardChoiceItems(choiceRoots),
                    maxEntries,
                    strictExtractor: true));
        }

        ChoiceExtractionResult? restSiteUpgradeStrict = null;
        ChoiceExtractionResult? ancientEventStrict = null;
        if (LooksLikeRestSiteUpgradeContext(triggerKind, screenHint, choiceRoots))
        {
            restSiteUpgradeStrict = ExtractRestSiteSmithUpgradeChoices(choiceRoots, maxEntries, screenHint);
            strictAttempts.Add(restSiteUpgradeStrict);
        }

        if (LooksLikeRestContext(triggerKind, screenHint, choiceRoots))
        {
            strictAttempts.Add(ExtractRestSiteChoices(choiceRoots, maxEntries));
        }

        if (LooksLikeEventContext(triggerKind, screenHint, choiceRoots))
        {
            ancientEventStrict = ExtractAncientEventChoices(choiceRoots, maxEntries);
            strictAttempts.Add(ancientEventStrict);
            if (!IsAncientEventDetected(ancientEventStrict))
            {
                strictAttempts.Add(
                    EvaluateChoiceSet(
                        "event",
                        CollectEventChoiceItems(choiceRoots),
                        maxEntries,
                        strictExtractor: true));
            }
        }

        if (LooksLikeShopContext(triggerKind, screenHint, choiceRoots))
        {
            strictAttempts.Add(
                EvaluateChoiceSet(
                    "shop",
                    CollectShopChoiceItems(choiceRoots),
                    maxEntries,
                    strictExtractor: true));
        }

        if (LooksLikeCombatContext(triggerKind, screenHint, choiceRoots))
        {
            strictAttempts.Add(ExtractCombatChoices(choiceRoots, maxEntries));
            strictAttempts.Add(
                EvaluateChoiceSet(
                    "combat",
                    CollectCombatChoiceItems(choiceRoots),
                    maxEntries,
                    strictExtractor: true));
        }

        if (LooksLikeMapContext(triggerKind, screenHint, choiceRoots, currentActiveScreenType, rootSceneObservation))
        {
            strictAttempts.Add(ExtractMapChoices(choiceRoots, maxEntries));
        }

        var strictSuccess = strictAttempts.FirstOrDefault(result => result.Choices.Count > 0);
        var mapAttempt = strictAttempts.FirstOrDefault(result =>
            string.Equals(result.Decision.ExtractorPath, "map", StringComparison.OrdinalIgnoreCase));
        var mapStrict = mapAttempt is { Choices.Count: > 0 }
            ? mapAttempt
            : null;
        var eventStrict = strictAttempts.FirstOrDefault(result =>
            string.Equals(result.Decision.ExtractorPath, "event", StringComparison.OrdinalIgnoreCase)
            && result.Choices.Count > 0);
        var normalizedAncientOwnership = NormalizeAncientForegroundOwnership(
            ancientEventStrict,
            mapAttempt,
            currentActiveScreenType,
            rootSceneObservation);
        if (mapStrict is not null && eventStrict is not null)
        {
            strictSuccess = MergeMixedContextChoices(eventStrict, mapStrict, maxEntries);
        }
        else if (mapStrict is not null
                 && HasMapForegroundAuthority(currentActiveScreenType, rootSceneObservation))
        {
            strictSuccess = mapStrict;
        }

        if (restSiteUpgradeStrict is not null
            && string.Equals(screenHint, "upgrade", StringComparison.OrdinalIgnoreCase))
        {
            return restSiteUpgradeStrict with
            {
                Candidates = strictAttempts
                    .SelectMany(result => result.Candidates)
                    .Take(512)
                    .ToArray(),
                Decision = restSiteUpgradeStrict.Decision with
                {
                    FailureReason = JoinFailureReasons(strictAttempts.Select(result => result.Decision.FailureReason), null),
                },
            };
        }

        if (normalizedAncientOwnership is not null)
        {
            return normalizedAncientOwnership with
            {
                Candidates = strictAttempts
                    .SelectMany(result => result.Candidates)
                    .Take(512)
                    .ToArray(),
                Decision = normalizedAncientOwnership.Decision with
                {
                    FailureReason = JoinFailureReasons(strictAttempts.Select(result => result.Decision.FailureReason), null),
                },
            };
        }

        if (ancientEventStrict is not null
            && IsAncientEventDetected(ancientEventStrict))
        {
            return ancientEventStrict with
            {
                Candidates = strictAttempts
                    .SelectMany(result => result.Candidates)
                    .Take(512)
                    .ToArray(),
                Decision = ancientEventStrict.Decision with
                {
                    FailureReason = JoinFailureReasons(strictAttempts.Select(result => result.Decision.FailureReason), null),
                },
            };
        }

        if (strictSuccess is not null)
        {
            return strictSuccess;
        }

        var generic = EvaluateChoiceSet("generic", candidateItems, maxEntries, strictExtractor: false);
        if (strictAttempts.Count == 0)
        {
            return generic;
        }

        return generic with
        {
            Candidates = strictAttempts
                .SelectMany(result => result.Candidates)
                .Concat(generic.Candidates)
                .Take(512)
                .ToArray(),
            Decision = generic.Decision with
            {
                FailureReason = JoinFailureReasons(strictAttempts.Select(result => result.Decision.FailureReason), generic.Decision.FailureReason),
            },
        };
    }

    private static ChoiceExtractionResult MergeMixedContextChoices(ChoiceExtractionResult primary, ChoiceExtractionResult secondary, int maxEntries)
    {
        var choices = primary.Choices
            .Concat(secondary.Choices)
            .GroupBy(choice => $"{choice.Kind}|{choice.Label}|{choice.ScreenBounds}|{choice.NodeId}", StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .Take(maxEntries)
            .ToArray();
        var candidates = primary.Candidates
            .Concat(secondary.Candidates)
            .Take(512)
            .ToArray();
        var failureReason = JoinFailureReasons(
            new[]
            {
                primary.Decision.FailureReason,
                secondary.Decision.FailureReason,
            },
            null);
        return new ChoiceExtractionResult(
            choices,
            candidates,
            new LiveExportChoiceDecision(
                "event+map",
                UsedStrictExtractor: true,
                CandidateCount: primary.Decision.CandidateCount + secondary.Decision.CandidateCount,
                AcceptedCount: choices.Length,
                choices.Length == 0 ? "none" : "accepted",
                failureReason,
                primary.Decision.PlaceholderLabels
                    .Concat(secondary.Decision.PlaceholderLabels)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(64)
                    .ToArray()));
    }

    private static ChoiceExtractionResult? NormalizeAncientForegroundOwnership(
        ChoiceExtractionResult? ancientEventStrict,
        ChoiceExtractionResult? mapAttempt,
        string? currentActiveScreenType,
        RootSceneTransitionObservation rootSceneObservation)
    {
        if (ancientEventStrict is null || !IsAncientEventDetected(ancientEventStrict))
        {
            return null;
        }

        var ancientPhase = ancientEventStrict.Meta.TryGetValue("ancientPhase", out var phaseValue)
            ? phaseValue
            : null;
        var mapReleaseAuthority = HasAncientMapReleaseAuthority(mapAttempt, currentActiveScreenType, rootSceneObservation);
        if (!mapReleaseAuthority)
        {
            return WithForegroundMetadata(
                ancientEventStrict,
                ancientEventStrict.Meta,
                "event",
                ancientPhase switch
                {
                    "dialogue" => "ancient-dialogue",
                    "options" => "ancient-option",
                    "completion" => "ancient-completion",
                    _ => "ancient-option",
                },
                eventTeardownInProgress: false,
                mapReleaseAuthority: false,
                mapSurfacePending: false,
                ancientPhase: ancientPhase);
        }

        if (mapAttempt is not null && mapAttempt.Choices.Count > 0)
        {
            return WithForegroundMetadata(
                mapAttempt,
                ancientEventStrict.Meta,
                "map",
                "map-node",
                eventTeardownInProgress: true,
                mapReleaseAuthority: true,
                mapSurfacePending: false,
                ancientPhase: "teardown");
        }

        return WithForegroundMetadata(
            new ChoiceExtractionResult(
                Array.Empty<LiveExportChoiceSummary>(),
                mapAttempt?.Candidates ?? Array.Empty<LiveExportChoiceCandidate>(),
                new LiveExportChoiceDecision(
                    "map",
                    UsedStrictExtractor: true,
                    CandidateCount: mapAttempt?.Candidates.Count ?? 0,
                    AcceptedCount: 0,
                    "none",
                    "map release authority is active but reachable map-node surface is still pending",
                    Array.Empty<string>()))
            {
                Meta = mapAttempt?.Meta is not null
                    ? new Dictionary<string, string?>(mapAttempt.Meta, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase),
            },
            ancientEventStrict.Meta,
            "map",
            "none",
            eventTeardownInProgress: true,
            mapReleaseAuthority: true,
            mapSurfacePending: true,
            ancientPhase: "teardown");
    }

    private static ChoiceExtractionResult WithForegroundMetadata(
        ChoiceExtractionResult result,
        IReadOnlyDictionary<string, string?>? supplementalMeta,
        string foregroundOwner,
        string foregroundActionLane,
        bool eventTeardownInProgress,
        bool mapReleaseAuthority,
        bool mapSurfacePending,
        string? ancientPhase)
    {
        var metadata = new Dictionary<string, string?>(result.Meta, StringComparer.OrdinalIgnoreCase)
        {
            ["foregroundOwner"] = foregroundOwner,
            ["foregroundActionLane"] = foregroundActionLane,
            ["eventTeardownInProgress"] = eventTeardownInProgress ? "true" : "false",
            ["mapReleaseAuthority"] = mapReleaseAuthority ? "true" : "false",
            ["mapSurfacePending"] = mapSurfacePending ? "true" : "false",
        };
        if (!string.IsNullOrWhiteSpace(ancientPhase))
        {
            metadata["ancientPhase"] = ancientPhase;
        }

        if (supplementalMeta is not null)
        {
            foreach (var entry in supplementalMeta)
            {
                metadata.TryAdd(entry.Key, entry.Value);
            }
        }

        return result with
        {
            Meta = metadata,
        };
    }

    private static bool HasAncientMapReleaseAuthority(
        ChoiceExtractionResult? mapAttempt,
        string? currentActiveScreenType,
        RootSceneTransitionObservation rootSceneObservation)
    {
        if (!HasMapForegroundAuthority(currentActiveScreenType, rootSceneObservation))
        {
            return false;
        }

        if (mapAttempt is null)
        {
            return false;
        }

        if (mapAttempt.Candidates.Count > 0 || mapAttempt.Choices.Count > 0)
        {
            return true;
        }

        return mapAttempt.Meta.TryGetValue("mapPointCount", out var mapPointCount)
               && int.TryParse(mapPointCount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
               && parsed > 0;
    }

    private static LiveExportEncounterSummary? ExtractEncounter(
        IEnumerable<object> roots,
        IDictionary<string, string?>? meta = null)
    {
        var encounterRoots = FindRoots(roots, "Encounter", "Room", "Combat", "Battle", "Monster");
        var combatManagerRoots = roots
            .Where(root =>
            {
                var typeName = root.GetType().FullName ?? root.GetType().Name;
                return typeName.Contains("CombatManager", StringComparison.OrdinalIgnoreCase);
            })
            .Concat(FindRoots(roots, "CombatManager"))
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();
        var name = TryReadString(encounterRoots, "Name", "EncounterName", "RoomName", "DisplayName");
        var kind = TryReadString(encounterRoots, "Type", "EncounterType", "RoomType");
        var turn = TryReadInt(encounterRoots, "Turn", "TurnNumber", "CurrentTurn");
        var isInProgress = TryReadBool(combatManagerRoots, "IsInProgress");
        var isEnding = TryReadBool(combatManagerRoots, "IsEnding", "IsOverOrEnding");
        var isPlayPhase = TryReadBool(combatManagerRoots, "IsPlayPhase");
        var isEnemyTurnStarted = TryReadBool(combatManagerRoots, "IsEnemyTurnStarted");
        var fallbackInCombat = TryReadBool(encounterRoots, "IsInProgress", "InCombat", "IsInCombat");

        bool? inCombat;
        string? combatPrimarySource;
        if (isInProgress is not null)
        {
            inCombat = isInProgress;
            combatPrimarySource = "CombatManager.IsInProgress";
        }
        else
        {
            inCombat = fallbackInCombat;
            combatPrimarySource = fallbackInCombat is null ? null : "Encounter.InCombat";
        }

        if (meta is not null)
        {
            meta["combatPrimarySource"] = combatPrimarySource;
            meta["combatPrimaryValue"] = inCombat?.ToString().ToLowerInvariant();

            var crossChecks = new List<string>();
            if (isPlayPhase is not null)
            {
                crossChecks.Add($"CombatManager.IsPlayPhase={isPlayPhase.Value.ToString().ToLowerInvariant()}");
            }

            if (isEnemyTurnStarted is not null)
            {
                crossChecks.Add($"CombatManager.IsEnemyTurnStarted={isEnemyTurnStarted.Value.ToString().ToLowerInvariant()}");
            }

            if (isEnding is not null)
            {
                crossChecks.Add($"CombatManager.IsEnding={isEnding.Value.ToString().ToLowerInvariant()}");
            }

            if (roots.Any(root =>
                {
                    var typeName = root.GetType().FullName ?? root.GetType().Name;
                    return typeName.Contains("NCombatRoom", StringComparison.OrdinalIgnoreCase);
                }))
            {
                crossChecks.Add("node:NCombatRoom");
            }

            if (roots.Any(root =>
                {
                    var typeName = root.GetType().FullName ?? root.GetType().Name;
                    return typeName.Contains("NCombatUi", StringComparison.OrdinalIgnoreCase);
                }))
            {
                crossChecks.Add("node:NCombatUi");
            }

            meta["combatCrossCheck"] = crossChecks.Count == 0
                ? null
                : string.Join(";", crossChecks);
        }

        if (name is null && kind is null && turn is null && inCombat is null)
        {
            return null;
        }

        return new LiveExportEncounterSummary(name, kind, inCombat, turn);
    }

    private static void AppendCombatRuntimeMetadata(
        IReadOnlyList<object> roots,
        LiveExportEncounterSummary? encounter,
        IDictionary<string, string?> meta,
        IDictionary<string, object?> payload)
    {
        if (encounter?.InCombat != true
            && !roots.Any(root =>
            {
                var typeName = root.GetType().FullName ?? root.GetType().Name;
                return typeName.Contains("Combat", StringComparison.OrdinalIgnoreCase)
                       || typeName.Contains("NPlayerHand", StringComparison.OrdinalIgnoreCase)
                       || typeName.Contains("NTargetManager", StringComparison.OrdinalIgnoreCase);
            }))
        {
            return;
        }

        var combatManagerRoots = roots
            .Where(root =>
            {
                var typeName = root.GetType().FullName ?? root.GetType().Name;
                return typeName.Contains("CombatManager", StringComparison.OrdinalIgnoreCase);
            })
            .Concat(FindRoots(roots, "CombatManager"))
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();
        var combatStateRoots = roots
            .Where(root =>
            {
                var typeName = root.GetType().FullName ?? root.GetType().Name;
                return typeName.Contains("CombatState", StringComparison.OrdinalIgnoreCase);
            })
            .Concat(combatManagerRoots.Select(root =>
                    TryGetMemberValue(root, "_state")
                    ?? TryGetMemberValue(root, "State")
                    ?? TryGetMemberValue(root, "CurrentState"))
                .Where(static root => root is not null)
                .Cast<object>())
            .Concat(FindRoots(roots, "CombatState"))
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();
        var handRoot = roots
            .Where(root =>
            {
                var typeName = root.GetType().FullName ?? root.GetType().Name;
                return typeName.Contains("NPlayerHand", StringComparison.OrdinalIgnoreCase);
            })
            .Concat(FindRoots(roots, "Hand"))
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .FirstOrDefault(root =>
                TryReadBool(root, "InCardPlay") is not null
                || TryReadString(root, "CurrentMode") is not null
                || TryGetMemberValue(root, "_currentCardPlay") is not null
                || TryGetMemberValue(root, "_holdersAwaitingQueue") is not null);
        var targetManagerRoot = roots
            .Where(root =>
            {
                var typeName = root.GetType().FullName ?? root.GetType().Name;
                return typeName.Contains("NTargetManager", StringComparison.OrdinalIgnoreCase);
            })
            .Concat(FindRoots(roots, "TargetManager"))
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .FirstOrDefault(root =>
                TryReadBool(root, "IsInSelection") is not null
                || TryGetMemberValue(root, "HoveredNode") is not null
                || TryGetMemberValue(root, "LastTargetingFinishedFrame") is not null);
        var historyRoot = roots
            .Where(root =>
            {
                var typeName = root.GetType().FullName ?? root.GetType().Name;
                return typeName.Contains("CombatHistory", StringComparison.OrdinalIgnoreCase);
            })
            .Concat(FindRoots(roots, "History"))
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .FirstOrDefault(root =>
                TryGetMemberValue(root, "CardPlaysStarted") is not null
                || TryGetMemberValue(root, "CardPlaysFinished") is not null
                || TryGetMemberValue(root, "Entries") is not null);
        var combatRoundNumber = TryReadInt(combatStateRoots, "RoundNumber");
        var playerActionsDisabled = TryReadBool(combatManagerRoots, "PlayerActionsDisabled");
        var endingPlayerTurnPhaseOne = TryReadBool(combatManagerRoots, "EndingPlayerTurnPhaseOne");
        var endingPlayerTurnPhaseTwo = TryReadBool(combatManagerRoots, "EndingPlayerTurnPhaseTwo");

        var cardPlayPending = TryReadBool(handRoot, "InCardPlay");
        var playMode = TryReadString(handRoot, "CurrentMode");
        var currentCardPlay = TryGetMemberValue(handRoot!, "_currentCardPlay")
                              ?? TryGetMemberValue(handRoot!, "CurrentCardPlay");
        var selectedHandCards = ExpandEnumerable(TryGetMemberValue(handRoot!, "_selectedCards")).ToArray();
        var selectedCard = TryExtractCombatCardFromPlay(currentCardPlay)
                           ?? (selectedHandCards.Length > 0 ? selectedHandCards[^1] : null);
        var selectedCardId = TryReadString(selectedCard, "Id", "CardId", "Name", "Title");
        var selectedCardName = TryReadString(selectedCard, "Title", "DisplayName", "Name", "Id", "CardId");
        var selectedCardType = TryReadString(selectedCard, "Type", "CardType");
        var selectedCardTargetType = TryReadString(selectedCard, "TargetType", "Target");
        var awaitingSlots = ExtractAwaitingPlaySlots(TryGetMemberValue(handRoot!, "_holdersAwaitingQueue"));
        var selectedCardSlot = ResolveCurrentCardPlaySlot(handRoot, awaitingSlots);
        var handSelectionSelectedCardIds = selectedHandCards
            .Select(static card => FirstNonEmpty(
                TryReadString(card, "Id", "CardId", "Name", "Title"),
                TryReadString(card, "Title", "DisplayName", "Name", "Id", "CardId")))
            .Where(static cardId => !string.IsNullOrWhiteSpace(cardId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()!;
        var handSelectionConfirmButton = TryGetMemberValue(handRoot!, "_selectModeConfirmButton");
        var handSelectionConfirmEnabled = handSelectionConfirmButton is null
            ? null
            : (TryResolveInteractiveEnabled(handSelectionConfirmButton)
               ?? TryResolveControlEnabled(handSelectionConfirmButton));

        var targetingInProgress = TryReadBool(targetManagerRoot, "IsInSelection");
        var validTargetsType = TryConvertToDisplayString(
            TryGetMemberValue(targetManagerRoot!, "_validTargetsType")
            ?? TryGetMemberValue(targetManagerRoot!, "ValidTargetsType"));
        var hoveredNode = TryGetMemberValue(targetManagerRoot!, "HoveredNode");
        var hoveredTargetKind = InferCombatHoveredTargetKind(hoveredNode);
        var hoveredTargetId = TryExtractCombatTargetId(hoveredNode);
        var hoveredTargetLabel = TryExtractCombatTargetLabel(hoveredNode);
        var targetingLastFinishedFrame = TryConvertToDisplayString(TryGetMemberValue(targetManagerRoot!, "LastTargetingFinishedFrame"));
        var combatEnemyTargetEvaluations = targetManagerRoot is null
            ? Array.Empty<CombatEnemyTargetEvaluation>()
            : CollectCombatEnemyTargetEvaluations(roots).ToArray();
        var targetableEnemyChoices = combatEnemyTargetEvaluations
            .Where(static evaluation => evaluation.TargetableForDiagnostics)
            .Select(static evaluation => evaluation.Choice)
            .Where(static choice => choice is not null)
            .Cast<LiveExportChoiceSummary>()
            .ToArray();
        var targetableEnemyIds = targetableEnemyChoices
            .Select(choice => choice.BindingId ?? choice.Value ?? choice.NodeId)
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var hittableEnemyChoices = combatEnemyTargetEvaluations
            .Where(static evaluation => evaluation.HittableForExport)
            .Select(static evaluation => evaluation.Choice)
            .Where(static choice => choice is not null)
            .Cast<LiveExportChoiceSummary>()
            .ToArray();
        var hittableEnemyIds = hittableEnemyChoices
            .Select(choice => choice.BindingId ?? choice.Value ?? choice.NodeId)
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var hoveredTargetEntity = hoveredNode is null ? null : TryGetMemberValue(hoveredNode, "Entity");
        var hoveredTargetIsHittable = TryReadBool(hoveredTargetEntity, "IsHittable");

        var lastStartedEntry = TryGetLastCombatHistoryEntry(historyRoot, "CardPlaysStarted", "CardPlayStartedEntry");
        var lastFinishedEntry = TryGetLastCombatHistoryEntry(historyRoot, "CardPlaysFinished", "CardPlayFinishedEntry");
        var historyStartedCount = CountCombatHistoryEntries(historyRoot, "CardPlaysStarted", "CardPlayStartedEntry");
        var historyFinishedCount = CountCombatHistoryEntries(historyRoot, "CardPlaysFinished", "CardPlayFinishedEntry");
        var lastStartedPlay = TryGetMemberValue(lastStartedEntry!, "CardPlay");
        var lastFinishedPlay = TryGetMemberValue(lastFinishedEntry!, "CardPlay");
        var lastStartedCard = TryExtractCombatCardFromPlay(lastStartedPlay);
        var lastFinishedCard = TryExtractCombatCardFromPlay(lastFinishedPlay);
        var lastStartedCardId = TryReadString(lastStartedCard, "Id", "CardId", "Name", "Title");
        var lastStartedCardName = TryReadString(lastStartedCard, "Title", "DisplayName", "Name", "Id", "CardId");
        var lastStartedTargetId = TryExtractCombatTargetId(TryGetMemberValue(lastStartedPlay!, "Target"));
        var lastFinishedCardId = TryReadString(lastFinishedCard, "Id", "CardId", "Name", "Title");
        var lastFinishedCardName = TryReadString(lastFinishedCard, "Title", "DisplayName", "Name", "Id", "CardId");
        var lastFinishedTargetId = TryExtractCombatTargetId(TryGetMemberValue(lastFinishedPlay!, "Target"));
        var interactionRevision = string.Join(":",
            historyStartedCount?.ToString(CultureInfo.InvariantCulture) ?? "none",
            historyFinishedCount?.ToString(CultureInfo.InvariantCulture) ?? "none",
            cardPlayPending?.ToString().ToLowerInvariant() ?? "none",
            targetingInProgress?.ToString().ToLowerInvariant() ?? "none",
            selectedCardSlot?.ToString(CultureInfo.InvariantCulture) ?? "none");

        meta["combatRuntimeStateAuthority"] = "runtime-reflection";
        meta["combatRoundNumber"] = combatRoundNumber?.ToString(CultureInfo.InvariantCulture);
        meta["combatPlayerActionsDisabled"] = playerActionsDisabled?.ToString().ToLowerInvariant();
        meta["combatEndingPlayerTurnPhaseOne"] = endingPlayerTurnPhaseOne?.ToString().ToLowerInvariant();
        meta["combatEndingPlayerTurnPhaseTwo"] = endingPlayerTurnPhaseTwo?.ToString().ToLowerInvariant();
        meta["combatCardPlayPending"] = cardPlayPending?.ToString().ToLowerInvariant();
        meta["combatPlayMode"] = playMode;
        meta["combatSelectedCardId"] = selectedCardId;
        meta["combatSelectedCardName"] = selectedCardName;
        meta["combatSelectedCardType"] = selectedCardType;
        meta["combatSelectedCardTargetType"] = selectedCardTargetType;
        meta["combatSelectedCardSlot"] = selectedCardSlot?.ToString(CultureInfo.InvariantCulture);
        meta["combatAwaitingPlayCount"] = awaitingSlots.Count.ToString(CultureInfo.InvariantCulture);
        meta["combatAwaitingPlaySlots"] = awaitingSlots.Count == 0
            ? null
            : string.Join(",", awaitingSlots.Select(slot => slot.ToString(CultureInfo.InvariantCulture)));
        meta["combatHandSelectionSelectedCount"] = selectedHandCards.Length.ToString(CultureInfo.InvariantCulture);
        meta["combatHandSelectionSelectedCardIds"] = handSelectionSelectedCardIds.Length == 0
            ? null
            : string.Join(",", handSelectionSelectedCardIds);
        meta["combatHandSelectionConfirmEnabled"] = handSelectionConfirmEnabled?.ToString().ToLowerInvariant();
        meta["combatTargetingInProgress"] = targetingInProgress?.ToString().ToLowerInvariant();
        meta["combatValidTargetsType"] = validTargetsType;
        meta["combatTargetableEnemyCount"] = targetManagerRoot is null
            ? null
            : targetableEnemyChoices.Length.ToString(CultureInfo.InvariantCulture);
        meta["combatTargetableEnemyIds"] = targetableEnemyIds.Length == 0
            ? null
            : string.Join(",", targetableEnemyIds);
        meta["combatHittableEnemyCount"] = targetManagerRoot is null
            ? null
            : hittableEnemyChoices.Length.ToString(CultureInfo.InvariantCulture);
        meta["combatHittableEnemyIds"] = hittableEnemyIds.Length == 0
            ? null
            : string.Join(",", hittableEnemyIds);
        meta["combatHoveredTargetKind"] = hoveredTargetKind;
        meta["combatHoveredTargetId"] = hoveredTargetId;
        meta["combatHoveredTargetLabel"] = hoveredTargetLabel;
        meta["combatHoveredTargetIsHittable"] = hoveredTargetIsHittable?.ToString().ToLowerInvariant();
        meta["combatTargetingLastFinishedFrame"] = targetingLastFinishedFrame;
        meta["combatHistoryStartedCount"] = historyStartedCount?.ToString(CultureInfo.InvariantCulture);
        meta["combatHistoryFinishedCount"] = historyFinishedCount?.ToString(CultureInfo.InvariantCulture);
        meta["combatInteractionRevision"] = interactionRevision;
        meta["combatLastCardPlayStartedCardId"] = lastStartedCardId;
        meta["combatLastCardPlayStartedCardName"] = lastStartedCardName;
        meta["combatLastCardPlayStartedTargetId"] = lastStartedTargetId;
        meta["combatLastCardPlayFinishedCardId"] = lastFinishedCardId;
        meta["combatLastCardPlayFinishedCardName"] = lastFinishedCardName;
        meta["combatLastCardPlayFinishedTargetId"] = lastFinishedTargetId;
        meta["combatLastCardPlayFinishedSuccess"] = null;
        var combatCrossCheckSegments = meta.TryGetValue("combatCrossCheck", out var existingCombatCrossCheck)
                                       && !string.IsNullOrWhiteSpace(existingCombatCrossCheck)
            ? existingCombatCrossCheck.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : new List<string>();
        void AppendCombatCrossCheck(string segment)
        {
            if (string.IsNullOrWhiteSpace(segment)
                || combatCrossCheckSegments.Contains(segment, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            combatCrossCheckSegments.Add(segment);
        }

        if (combatRoundNumber is not null)
        {
            AppendCombatCrossCheck($"CombatState.RoundNumber={combatRoundNumber.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (playerActionsDisabled is not null)
        {
            AppendCombatCrossCheck($"CombatManager.PlayerActionsDisabled={playerActionsDisabled.Value.ToString().ToLowerInvariant()}");
        }

        if (endingPlayerTurnPhaseOne is not null)
        {
            AppendCombatCrossCheck($"CombatManager.EndingPlayerTurnPhaseOne={endingPlayerTurnPhaseOne.Value.ToString().ToLowerInvariant()}");
        }

        if (endingPlayerTurnPhaseTwo is not null)
        {
            AppendCombatCrossCheck($"CombatManager.EndingPlayerTurnPhaseTwo={endingPlayerTurnPhaseTwo.Value.ToString().ToLowerInvariant()}");
        }

        meta["combatCrossCheck"] = combatCrossCheckSegments.Count == 0
            ? null
            : string.Join(";", combatCrossCheckSegments);

        if (combatRoundNumber is not null)
        {
            payload["combatRoundNumber"] = combatRoundNumber.Value;
        }

        if (playerActionsDisabled is not null)
        {
            payload["combatPlayerActionsDisabled"] = playerActionsDisabled.Value;
        }

        if (endingPlayerTurnPhaseOne is not null)
        {
            payload["combatEndingPlayerTurnPhaseOne"] = endingPlayerTurnPhaseOne.Value;
        }

        if (endingPlayerTurnPhaseTwo is not null)
        {
            payload["combatEndingPlayerTurnPhaseTwo"] = endingPlayerTurnPhaseTwo.Value;
        }

        if (cardPlayPending is not null)
        {
            payload["combatCardPlayPending"] = cardPlayPending.Value;
        }

        if (!string.IsNullOrWhiteSpace(playMode))
        {
            payload["combatPlayMode"] = playMode;
        }

        if (selectedCardSlot is not null)
        {
            payload["combatSelectedCardSlot"] = selectedCardSlot.Value;
        }

        if (awaitingSlots.Count > 0)
        {
            payload["combatAwaitingPlaySlots"] = awaitingSlots.ToArray();
        }

        payload["combatHandSelectionSelectedCount"] = selectedHandCards.Length;

        if (handSelectionSelectedCardIds.Length > 0)
        {
            payload["combatHandSelectionSelectedCardIds"] = handSelectionSelectedCardIds;
        }

        if (handSelectionConfirmEnabled is not null)
        {
            payload["combatHandSelectionConfirmEnabled"] = handSelectionConfirmEnabled.Value;
        }

        if (targetingInProgress is not null)
        {
            payload["combatTargetingInProgress"] = targetingInProgress.Value;
        }

        if (historyStartedCount is not null)
        {
            payload["combatHistoryStartedCount"] = historyStartedCount.Value;
        }

        if (historyFinishedCount is not null)
        {
            payload["combatHistoryFinishedCount"] = historyFinishedCount.Value;
        }

        payload["combatInteractionRevision"] = interactionRevision;

        if (!string.IsNullOrWhiteSpace(validTargetsType))
        {
            payload["combatValidTargetsType"] = validTargetsType;
        }

        if (targetManagerRoot is not null)
        {
            payload["combatTargetableEnemyCount"] = targetableEnemyChoices.Length;
            payload["combatTargetableEnemyIds"] = targetableEnemyIds;
            payload["combatHittableEnemyCount"] = hittableEnemyChoices.Length;
            payload["combatHittableEnemyIds"] = hittableEnemyIds;
        }

        if (hoveredTargetIsHittable is not null)
        {
            payload["combatHoveredTargetIsHittable"] = hoveredTargetIsHittable.Value;
        }

        if (!string.IsNullOrWhiteSpace(lastFinishedCardId))
        {
            payload["combatLastCardPlayFinishedCardId"] = lastFinishedCardId;
        }
    }

    private static object? TryExtractCombatCardFromPlay(object? cardPlay)
    {
        if (cardPlay is null)
        {
            return null;
        }

        return TryGetMemberValue(cardPlay, "Card")
               ?? TryGetMemberValue(TryGetMemberValue(cardPlay, "CardNode"), "Model")
               ?? TryGetMemberValue(TryGetMemberValue(cardPlay, "Holder"), "CardModel")
               ?? TryGetMemberValue(TryGetMemberValue(cardPlay, "Holder"), "Card")
               ?? TryGetMemberValue(TryGetMemberValue(cardPlay, "Holder"), "CardNode");
    }

    private static int? ResolveCurrentCardPlaySlot(object? handRoot, IReadOnlyList<int> awaitingSlots)
    {
        var draggedHolderIndex = TryReadInt(handRoot, "_draggedHolderIndex");
        if (draggedHolderIndex is >= 0)
        {
            return draggedHolderIndex.Value + 1;
        }

        return awaitingSlots.Count > 0
            ? awaitingSlots[0]
            : null;
    }

    private static IReadOnlyList<int> ExtractAwaitingPlaySlots(object? awaitingQueue)
    {
        if (awaitingQueue is null)
        {
            return Array.Empty<int>();
        }

        var slots = new List<int>();
        void AddSlot(object? value)
        {
            var slotIndex = value switch
            {
                byte number => number + 1,
                short number => number + 1,
                int number => number + 1,
                long number when number is >= 0 and <= int.MaxValue - 1 => (int)number + 1,
                string text when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => parsed + 1,
                _ => -1,
            };
            if (slotIndex is >= 1 and <= 10)
            {
                slots.Add(slotIndex);
            }
        }

        if (awaitingQueue is System.Collections.IDictionary dictionary)
        {
            foreach (System.Collections.DictionaryEntry entry in dictionary)
            {
                AddSlot(entry.Value);
            }
        }
        else if (awaitingQueue is System.Collections.IEnumerable enumerable and not string)
        {
            foreach (var entry in enumerable)
            {
                AddSlot(TryGetMemberValue(entry!, "Value"));
            }
        }

        return slots
            .Distinct()
            .OrderBy(static slot => slot)
            .ToArray();
    }

    private static string? InferCombatHoveredTargetKind(object? hoveredNode)
    {
        if (hoveredNode is null)
        {
            return null;
        }

        var entity = TryGetMemberValue(hoveredNode, "Entity")
                     ?? TryGetMemberValue(hoveredNode, "Player")
                     ?? TryGetMemberValue(hoveredNode, "Creature");
        if (TryReadBool(entity, "IsPlayer") == true)
        {
            return "player";
        }

        if (TryReadBool(entity, "IsPet", "IsFriendly", "IsAlly") == true)
        {
            return "ally";
        }

        var typeName = hoveredNode.GetType().FullName ?? hoveredNode.GetType().Name;
        if (typeName.Contains("Creature", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Monster", StringComparison.OrdinalIgnoreCase))
        {
            return "enemy";
        }

        return typeName;
    }

    private static string? TryExtractCombatTargetId(object? target)
    {
        if (target is null)
        {
            return null;
        }

        return TryReadString(target, "Id", "CardId", "Name", "DisplayName", "MonsterName")
               ?? TryReadString(TryGetMemberValue(target, "Monster"), "Id", "Name", "DisplayName")
               ?? TryReadString(TryGetMemberValue(target, "Entity"), "Id", "Name", "DisplayName")
               ?? TryReadString(TryGetMemberValue(target, "Player"), "Id", "Name", "DisplayName");
    }

    private static string? TryExtractCombatTargetLabel(object? target)
    {
        if (target is null)
        {
            return null;
        }

        return TryReadString(target, "DisplayName", "Name", "Id", "MonsterName")
               ?? TryReadString(TryGetMemberValue(target, "Monster"), "DisplayName", "Name", "Id")
               ?? TryReadString(TryGetMemberValue(target, "Entity"), "DisplayName", "Name", "Id")
               ?? TryReadString(TryGetMemberValue(target, "Player"), "DisplayName", "Name", "Id");
    }

    private static object? TryGetLastCombatHistoryEntry(object? historyRoot, string collectionMemberName, string entryTypeName)
    {
        if (historyRoot is null)
        {
            return null;
        }

        var explicitEntries = ExpandEnumerable(TryGetMemberValue(historyRoot, collectionMemberName)).ToArray();
        if (explicitEntries.Length > 0)
        {
            return explicitEntries[^1];
        }

        return ExpandEnumerable(TryGetMemberValue(historyRoot, "Entries"))
            .LastOrDefault(entry =>
            {
                var typeName = entry.GetType().FullName ?? entry.GetType().Name;
                return typeName.Contains(entryTypeName, StringComparison.OrdinalIgnoreCase);
            });
    }

    private static int? CountCombatHistoryEntries(object? historyRoot, string collectionMemberName, string entryTypeName)
    {
        if (historyRoot is null)
        {
            return null;
        }

        var explicitEntries = ExpandEnumerable(TryGetMemberValue(historyRoot, collectionMemberName)).ToArray();
        if (explicitEntries.Length > 0)
        {
            return explicitEntries.Length;
        }

        var filteredEntries = ExpandEnumerable(TryGetMemberValue(historyRoot, "Entries"))
            .Where(entry =>
            {
                var typeName = entry.GetType().FullName ?? entry.GetType().Name;
                return typeName.Contains(entryTypeName, StringComparison.OrdinalIgnoreCase);
            })
            .ToArray();
        return filteredEntries.Length == 0
            ? null
            : filteredEntries.Length;
    }

    private static ChoiceExtractionResult EvaluateChoiceSet(
        string extractorPath,
        IEnumerable<object> items,
        int maxEntries,
        bool strictExtractor)
    {
        var evaluated = items
            .Where(item => item is not null)
            .DistinctBy(item => RuntimeHelpers.GetHashCode(item))
            .Select(item =>
            {
                var summary = TryCreateChoiceSummary(item);
                var score = ScoreChoiceCandidate(item);
                var label = summary?.Label ?? TryResolveChoiceLabel(item);
                var description = summary?.Description ?? TryResolveChoiceDescription(item);
                var value = summary?.Value ?? TryResolveChoiceValue(item);
                var placeholder = summary is not null && LooksLikePlaceholderChoice(summary, score);
                var rejectReason = summary is null
                    ? "no-summary"
                    : placeholder
                        ? "placeholder-label"
                        : null;

                return new
                {
                    Item = item,
                    Summary = summary,
                    Score = score,
                    Label = label,
                    Description = description,
                    Value = value,
                    RejectReason = rejectReason,
                };
            })
            .ToArray();

        var selectedKeys = evaluated
            .Where(entry => entry.Summary is not null && entry.RejectReason is null)
            .Select(entry => (Summary: entry.Summary!, entry.Score))
            .OrderByDescending(entry => entry.Score)
            .ThenBy(entry => entry.Summary.Kind, StringComparer.Ordinal)
            .ThenBy(entry => entry.Summary.Label, StringComparer.Ordinal)
            .DistinctBy(entry => $"{entry.Summary.Kind}|{entry.Summary.Label}|{entry.Summary.Value}")
            .Take(maxEntries)
            .Select(entry => $"{entry.Summary.Kind}|{entry.Summary.Label}|{entry.Summary.Value}")
            .ToHashSet(StringComparer.Ordinal);

        var candidates = evaluated
            .Select(entry =>
            {
                var accepted = entry.Summary is not null
                    && selectedKeys.Contains($"{entry.Summary.Kind}|{entry.Summary.Label}|{entry.Summary.Value}");
                var rejectReason = entry.RejectReason;
                if (!accepted && rejectReason is null)
                {
                    rejectReason = "not-selected";
                }

                return new LiveExportChoiceCandidate(
                    extractorPath,
                    entry.Item.GetType().FullName ?? entry.Item.GetType().Name,
                    entry.Label,
                    entry.Value,
                    entry.Description,
                    entry.Score,
                    accepted,
                    rejectReason);
            })
            .ToArray();

        var choices = evaluated
            .Where(entry => entry.Summary is not null)
            .Select(entry => entry.Summary!)
            .Where(summary => selectedKeys.Contains($"{summary.Kind}|{summary.Label}|{summary.Value}"))
            .ToArray();

        var placeholders = candidates
            .Where(candidate => string.Equals(candidate.RejectReason, "placeholder-label", StringComparison.Ordinal))
            .Select(candidate => candidate.Label)
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(label => label, StringComparer.Ordinal)
            .ToArray();

        var outcome = choices.Length > 0 ? "accepted" : strictExtractor ? "strict-miss" : "missing";
        var failureReason = choices.Length > 0
            ? null
            : strictExtractor
                ? $"strict extractor '{extractorPath}' did not resolve any valid choices."
                : "generic fallback did not resolve any valid choices.";

        return new ChoiceExtractionResult(
            choices,
            candidates,
            new LiveExportChoiceDecision(
                extractorPath,
                strictExtractor,
                candidates.Length,
                choices.Length,
                outcome,
                failureReason,
                placeholders));
    }

    private static IEnumerable<object> CollectRewardChoiceItems(IEnumerable<object> choiceRoots)
    {
        return CollectChoiceItems(
            choiceRoots,
            "Reward",
            "_options",
            "_extraOptions",
            "_rewardButtons",
            "RewardButtons",
            "Rewards",
            "RewardAlternatives",
            "RewardOptions",
            "ExtraOptions");
    }

    private static IEnumerable<object> CollectRestChoiceItems(IEnumerable<object> choiceRoots)
    {
        return CollectChoiceItems(
            choiceRoots,
            "Rest",
            "Options",
            "CurrentOptions",
            "Buttons",
            "_options",
            "_restSiteOptions");
    }

    private static ChoiceExtractionResult ExtractRestSiteChoices(IEnumerable<object> roots, int maxEntries)
    {
        var candidates = new List<RestSiteExtractorCandidate>();
        foreach (var button in CollectRestSiteButtons(roots)
                     .DistinctBy(RuntimeHelpers.GetHashCode))
        {
            var candidate = TryCreateRestSiteButtonCandidate(button);
            if (candidate is not null)
            {
                candidates.Add(candidate);
            }
        }

        foreach (var option in CollectRestSiteRawOptions(roots)
                     .DistinctBy(RuntimeHelpers.GetHashCode))
        {
            var candidate = TryCreateRestSiteRawOptionCandidate(option);
            if (candidate is not null)
            {
                candidates.Add(candidate);
            }
        }

        if (candidates.Count == 0)
        {
            return new ChoiceExtractionResult(
                Array.Empty<LiveExportChoiceSummary>(),
                Array.Empty<LiveExportChoiceCandidate>(),
                new LiveExportChoiceDecision(
                    "rest",
                    UsedStrictExtractor: true,
                    CandidateCount: 0,
                    AcceptedCount: 0,
                    "strict-miss",
                    "strict extractor 'rest' did not resolve any rest-site buttons or options.",
                    Array.Empty<string>()));
        }

        var grouped = candidates
            .GroupBy(static candidate => candidate.OptionId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var winners = grouped
            .Select(static group => group
                .OrderBy(static candidate => candidate.SourceKind == "button" && !string.IsNullOrWhiteSpace(candidate.ScreenBounds) ? 0 : candidate.SourceKind == "button" ? 1 : 2)
                .ThenByDescending(static candidate => candidate.Score)
                .ThenBy(static candidate => candidate.Label, StringComparer.Ordinal)
                .First())
            .ToArray();
        var acceptedChoiceKeys = winners
            .Take(maxEntries)
            .Select(static candidate => candidate.OptionId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var choices = winners
            .Where(candidate => acceptedChoiceKeys.Contains(candidate.OptionId))
            .Select(static candidate => candidate.Summary)
            .OrderBy(static choice => TryGetBoundsSortX(choice.ScreenBounds))
            .ThenBy(static choice => TryGetBoundsSortY(choice.ScreenBounds))
            .ThenBy(static choice => choice.BindingId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static choice => choice.Label, StringComparer.Ordinal)
            .Take(maxEntries)
            .ToArray();

        var strictCandidates = candidates
            .Select(candidate =>
            {
                var accepted = acceptedChoiceKeys.Contains(candidate.OptionId)
                               && winners.Any(winner => ReferenceEquals(winner, candidate));
                var rejectReason = accepted
                    ? null
                    : candidate.RejectReason;
                if (!accepted && string.IsNullOrWhiteSpace(rejectReason))
                {
                    rejectReason = grouped.First(group => string.Equals(group.Key, candidate.OptionId, StringComparison.OrdinalIgnoreCase))
                        .Any(groupCandidate => groupCandidate.SourceKind == "button" && !ReferenceEquals(groupCandidate, candidate))
                        && string.Equals(candidate.SourceKind, "raw-option", StringComparison.OrdinalIgnoreCase)
                        ? "shadowed-by-button-candidate"
                        : "not-selected";
                }

                return new LiveExportChoiceCandidate(
                    "rest",
                    candidate.TypeName,
                    candidate.Label,
                    candidate.OptionId,
                    candidate.Description,
                    candidate.Score,
                    accepted,
                    rejectReason);
            })
            .ToArray();

        var failureReason = choices.Length == 0
            ? "strict extractor 'rest' did not resolve any valid rest-site choices."
            : null;
        return new ChoiceExtractionResult(
            choices,
            strictCandidates,
            new LiveExportChoiceDecision(
                "rest",
                UsedStrictExtractor: true,
                CandidateCount: strictCandidates.Length,
                AcceptedCount: choices.Length,
                choices.Length == 0 ? "strict-miss" : "accepted",
                failureReason,
                Array.Empty<string>()));
    }

    private static ChoiceExtractionResult ExtractRestSiteSmithUpgradeChoices(IEnumerable<object> roots, int maxEntries, string? screenHint)
    {
        var upgradeObservation = ObserveRestSiteUpgrade(roots, screenHint);
        if (!upgradeObservation.ScreenDetected
            || (upgradeObservation.VisibleCards.Count == 0 && !upgradeObservation.ConfirmVisible))
        {
            return new ChoiceExtractionResult(
                Array.Empty<LiveExportChoiceSummary>(),
                Array.Empty<LiveExportChoiceCandidate>(),
                new LiveExportChoiceDecision(
                    "rest-smith-upgrade",
                    UsedStrictExtractor: true,
                    CandidateCount: 0,
                    AcceptedCount: 0,
                    upgradeObservation.ScreenVisible ? "observer-miss" : "strict-miss",
                    upgradeObservation.ScreenVisible
                        ? "strict extractor 'rest-smith-upgrade' detected the smith upgrade screen, but did not resolve upgrade card or confirm hitboxes."
                        : "strict extractor 'rest-smith-upgrade' did not detect an active smith upgrade screen.",
                    Array.Empty<string>()));
        }

        var includeConfirmChoice = upgradeObservation.ConfirmVisible
                                 && !string.IsNullOrWhiteSpace(upgradeObservation.ConfirmBounds);
        var maxCardEntries = includeConfirmChoice
            ? Math.Max(0, maxEntries - 1)
            : maxEntries;

        var choices = upgradeObservation.VisibleCards
            .OrderBy(static card => TryGetBoundsSortX(card.ScreenBounds))
            .ThenBy(static card => TryGetBoundsSortY(card.ScreenBounds))
            .Select((card, index) =>
            {
                var bindingId = SanitizeNodeKey(card.CardId ?? card.Label ?? $"card-{index + 1}");
                return new LiveExportChoiceSummary(
                    "rest-site-smith-card",
                    card.Label,
                    card.CardId ?? bindingId,
                    "rest-site smith upgrade card")
                {
                    NodeId = $"rest-site:smith-card:{bindingId}",
                    ScreenBounds = card.ScreenBounds,
                    BindingKind = "rest-site-smith-card",
                    BindingId = card.CardId ?? bindingId,
                    Enabled = card.Enabled,
                    SemanticHints = new[]
                    {
                        "scene:rest-site",
                        "substate:smith-grid",
                        $"source:{card.BoundsSource}",
                    },
                };
            })
            .Take(maxCardEntries)
            .ToList();

        if (includeConfirmChoice)
        {
            choices.Add(
                new LiveExportChoiceSummary(
                    "rest-site-smith-confirm",
                    "Smith Confirm",
                    "SMITH_CONFIRM",
                    "rest-site smith confirm button")
                {
                    NodeId = "rest-site:smith-confirm",
                    ScreenBounds = upgradeObservation.ConfirmBounds,
                    BindingKind = "rest-site-smith-confirm",
                    BindingId = "SMITH_CONFIRM",
                    Enabled = upgradeObservation.ConfirmEnabled,
                    SemanticHints = new[]
                    {
                        "scene:rest-site",
                        "substate:smith-confirm",
                        "source:confirm-button",
                    },
                });
        }

        var strictCandidates = choices
            .Select(choice => new LiveExportChoiceCandidate(
                "rest-smith-upgrade",
                choice.Kind,
                choice.Label,
                choice.Value,
                choice.Description,
                choice.Kind == "rest-site-smith-confirm" ? 125 : 120,
                Accepted: true,
                RejectReason: null))
            .ToArray();

        return new ChoiceExtractionResult(
            choices,
            strictCandidates,
            new LiveExportChoiceDecision(
                "rest-smith-upgrade",
                UsedStrictExtractor: true,
                CandidateCount: strictCandidates.Length,
                AcceptedCount: choices.Count,
                "accepted",
                null,
                Array.Empty<string>()));
    }

    private static IEnumerable<object> CollectRestSiteButtons(IEnumerable<object> roots)
    {
        var buttons = new List<object>();
        foreach (var root in roots)
        {
            AddRestSiteButton(buttons, root);

            var roomTypeName = root.GetType().FullName ?? root.GetType().Name;
            if (!roomTypeName.Contains("NRestSiteRoom", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var choicesContainer = TryGetMemberValue(root, "_choicesContainer")
                                   ?? TryGetMemberValue(root, "ChoicesContainer");
            AddRestSiteButton(buttons, choicesContainer);
            foreach (var child in TryEnumerateChildren(choicesContainer).Take(32))
            {
                AddRestSiteButton(buttons, child);
            }
        }

        return buttons;
    }

    private static IEnumerable<object> CollectRestSiteRawOptions(IEnumerable<object> roots)
    {
        var options = new List<object>();
        foreach (var root in roots)
        {
            AddRestSiteOption(options, root);
            AddRestSiteOption(options, TryGetMemberValue(root, "Option"));
            AddRestSiteOption(options, TryGetMemberValue(root, "_option"));

            var typeName = root.GetType().FullName ?? root.GetType().Name;
            if (!typeName.Contains("NRestSiteRoom", StringComparison.OrdinalIgnoreCase)
                && !typeName.Contains("RestSiteRoom", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var option in ExpandEnumerable(TryGetMemberValue(root, "Options")))
            {
                AddRestSiteOption(options, option);
            }
        }

        return options;
    }

    private static void AddRestSiteButton(List<object> buttons, object? candidate)
    {
        if (candidate is null)
        {
            return;
        }

        var typeName = candidate.GetType().FullName ?? candidate.GetType().Name;
        if (typeName.Contains("NRestSiteButton", StringComparison.OrdinalIgnoreCase))
        {
            AddIfUseful(buttons, candidate);
        }
    }

    private static void AddRestSiteOption(List<object> options, object? candidate)
    {
        if (candidate is null)
        {
            return;
        }

        var typeName = candidate.GetType().FullName ?? candidate.GetType().Name;
        if (typeName.Contains("RestSiteOption", StringComparison.OrdinalIgnoreCase))
        {
            AddIfUseful(options, candidate);
        }
    }

    private static RestSiteButtonObservation ObserveRestSiteButtons(IEnumerable<object> roots)
    {
        var visibleButtons = new List<(string OptionId, bool Enabled, bool Executing)>();
        string? hoveredOptionId = null;

        foreach (var room in roots.Where(static root =>
                     (root.GetType().FullName ?? root.GetType().Name).Contains("NRestSiteRoom", StringComparison.OrdinalIgnoreCase))
                     .DistinctBy(RuntimeHelpers.GetHashCode))
        {
            var focusedButton = TryGetMemberValue(room, "_lastFocused");
            var focusedOption = focusedButton is null ? null : TryGetMemberValue(focusedButton, "Option") ?? TryGetMemberValue(focusedButton, "_option");
            hoveredOptionId ??= focusedOption is null ? null : TryResolveRestSiteOptionId(focusedOption, focusedButton!);
        }

        foreach (var button in CollectRestSiteButtons(roots).DistinctBy(RuntimeHelpers.GetHashCode))
        {
            var option = TryGetMemberValue(button, "Option") ?? TryGetMemberValue(button, "_option");
            if (option is null)
            {
                continue;
            }

            var optionId = TryResolveRestSiteOptionId(option, button);
            if (string.IsNullOrWhiteSpace(optionId))
            {
                continue;
            }

            if (!IsVisibleNode(button) && string.IsNullOrWhiteSpace(TryResolveScreenBounds(button)))
            {
                continue;
            }

            var enabled = TryResolveRestSiteButtonEnabled(button, option);
            var executing = TryReadBool(button, "_executingOption") == true;
            visibleButtons.Add((optionId, enabled, executing));
        }

        var optionsInteractive = visibleButtons.Count > 0 && visibleButtons.All(static button => button.Enabled && !button.Executing);
        var visibleOptionSummary = visibleButtons.Count == 0
            ? null
            : string.Join(
                ",",
                visibleButtons.Select(static button =>
                    $"{button.OptionId}:{(button.Executing ? "executing" : button.Enabled ? "enabled" : "disabled")}"));

        return new RestSiteButtonObservation(
            visibleButtons.Count,
            hoveredOptionId,
            visibleButtons.FirstOrDefault(static button => button.Executing).OptionId,
            optionsInteractive,
            visibleOptionSummary);
    }

    private static RestSiteUpgradeObservation ObserveRestSiteUpgrade(IEnumerable<object> roots, string? screenHint)
    {
        var upgradeScreens = roots.Where(static root =>
                (root.GetType().FullName ?? root.GetType().Name).Contains("NDeckUpgradeSelectScreen", StringComparison.OrdinalIgnoreCase))
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();
        var visibleCards = new List<RestSiteUpgradeCardCandidate>();
        object? visibleConfirmButton = null;
        string? previewMode = null;
        var selectedCardLabels = new List<string>();
        var selectedCardCount = 0;
        var screenDetected = upgradeScreens.Length > 0;
        var forceScreenVisible = string.Equals(screenHint, "upgrade", StringComparison.OrdinalIgnoreCase);
        var visibleUpgradeScreen = false;

        foreach (var screen in upgradeScreens)
        {
            if (!forceScreenVisible && !IsVisibleNode(screen))
            {
                continue;
            }

            visibleUpgradeScreen = true;

            foreach (var node in EnumerateUpgradeCardHolders(screen))
            {
                var cardCandidate = TryCreateRestSiteUpgradeCardCandidate(node);
                if (cardCandidate is null)
                {
                    continue;
                }

                visibleCards.Add(cardCandidate);
            }

            visibleConfirmButton ??= TryResolveVisibleUpgradeConfirmButton(screen, out previewMode);

            foreach (var selectedCard in ExpandEnumerable(TryGetMemberValue(screen, "_selectedCards")))
            {
                selectedCardCount += 1;
                var label = FirstNonEmpty(
                    TryReadString(selectedCard, "Id", "CardId", "Name"),
                    TryResolveChoiceLabel(selectedCard));
                if (!string.IsNullOrWhiteSpace(label))
                {
                    selectedCardLabels.Add(label);
                }
            }
        }

        var confirmBounds = TryResolveInteractiveScreenBounds(visibleConfirmButton, out _);
        var confirmVisible = visibleConfirmButton is not null && !string.IsNullOrWhiteSpace(confirmBounds);
        var confirmEnabled = confirmVisible && (TryResolveInteractiveEnabled(visibleConfirmButton) ?? true);
        var screenVisible = visibleUpgradeScreen || visibleCards.Count > 0 || confirmVisible || (screenDetected && forceScreenVisible);
        var observerMiss = screenVisible && visibleCards.Count == 0 && !confirmVisible;

        return new RestSiteUpgradeObservation(
            ScreenDetected: screenDetected,
            ScreenVisible: screenVisible,
            VisibleCards: visibleCards
                .OrderBy(static card => TryGetBoundsSortX(card.ScreenBounds))
                .ThenBy(static card => TryGetBoundsSortY(card.ScreenBounds))
                .ToArray(),
            ConfirmVisible: confirmVisible,
            ConfirmEnabled: confirmEnabled,
            ConfirmBounds: confirmBounds,
            SelectedCardCount: selectedCardCount,
            SelectedCardsSummary: selectedCardLabels.Count == 0
                ? null
                : string.Join(",", selectedCardLabels.Distinct(StringComparer.OrdinalIgnoreCase)),
            PreviewMode: previewMode,
            ObserverMiss: observerMiss);
    }

    private static IEnumerable<object> EnumerateUpgradeCardHolders(object screen)
    {
        var seen = new HashSet<int>();

        var grid = TryGetMemberValue(screen, "_grid") ?? TryGetMemberValue(screen, "Grid");
        if (grid is not null)
        {
            foreach (var holder in ExpandEnumerable(TryGetMemberValue(grid, "CurrentlyDisplayedCardHolders")))
            {
                if (holder is not null && seen.Add(RuntimeHelpers.GetHashCode(holder)))
                {
                    yield return holder;
                }
            }

            foreach (var row in ExpandEnumerable(TryGetMemberValue(grid, "_cardRows")))
            {
                foreach (var holder in ExpandEnumerable(row))
                {
                    if (holder is not null && seen.Add(RuntimeHelpers.GetHashCode(holder)))
                    {
                        yield return holder;
                    }
                }
            }

            foreach (var card in ExpandEnumerable(TryGetMemberValue(grid, "CurrentlyDisplayedCards")))
            {
                var holder = card is null ? null : TryInvokeMethod(grid, "GetCardHolder", card);
                if (holder is not null && seen.Add(RuntimeHelpers.GetHashCode(holder)))
                {
                    yield return holder;
                }
            }

            foreach (var card in ExpandEnumerable(TryGetMemberValue(grid, "_cards")))
            {
                var holder = card is null ? null : TryInvokeMethod(grid, "GetCardHolder", card);
                if (holder is not null && seen.Add(RuntimeHelpers.GetHashCode(holder)))
                {
                    yield return holder;
                }
            }

            foreach (var holder in ExpandEnumerable(TryInvokeMethod(grid, "GetTopRowOfCardNodes")))
            {
                if (holder is not null && seen.Add(RuntimeHelpers.GetHashCode(holder)))
                {
                    yield return holder;
                }
            }
        }

        foreach (var node in EnumerateDescendants(screen, 256))
        {
            var typeName = node.GetType().FullName ?? node.GetType().Name;
            if (!typeName.Contains("NGridCardHolder", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (seen.Add(RuntimeHelpers.GetHashCode(node)))
            {
                yield return node;
            }
        }
    }

    private static object? TryResolveVisibleUpgradeConfirmButton(object screen, out string? previewMode)
    {
        previewMode = null;

        var singlePreviewContainer = TryGetMemberValue(screen, "_upgradeSinglePreviewContainer");
        if (singlePreviewContainer is not null && IsVisibleNode(singlePreviewContainer))
        {
            previewMode = "single";
            return TryGetMemberValue(screen, "_singlePreviewConfirmButton");
        }

        var multiPreviewContainer = TryGetMemberValue(screen, "_upgradeMultiPreviewContainer");
        if (multiPreviewContainer is not null && IsVisibleNode(multiPreviewContainer))
        {
            previewMode = "multi";
            return TryGetMemberValue(screen, "_multiPreviewConfirmButton");
        }

        return null;
    }

    private static RestSiteUpgradeCardCandidate? TryCreateRestSiteUpgradeCardCandidate(object holder)
    {
        var screenBounds = TryResolveInteractiveScreenBounds(holder, out var boundsSource);
        if (string.IsNullOrWhiteSpace(screenBounds))
        {
            return null;
        }

        var hitbox = TryGetMemberValue(holder, "Hitbox") ?? TryGetMemberValue(holder, "_hitbox");
        if (!IsVisibleNode(holder)
            && (hitbox is null || !IsVisibleNode(hitbox)))
        {
            return null;
        }

        var cardModel = TryGetMemberValue(holder, "CardModel")
                        ?? TryGetMemberValue(holder, "Card")
                        ?? TryGetMemberValue(holder, "CardNode");
        var label = FirstNonEmpty(
            TryResolveChoiceLabel(cardModel ?? holder),
            TryReadString(cardModel, "Title", "CardName", "DisplayName", "Name", "Id", "CardId"),
            $"Smith Card {RuntimeHelpers.GetHashCode(holder)}");
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        var cardId = FirstNonEmpty(
            TryReadString(cardModel, "Id", "CardId", "Name", "Title"),
            TryReadString(TryGetMemberValue(holder, "CardNode"), "Id", "CardId", "Name", "Title"),
            TryReadString(TryGetMemberValue(holder, "Card"), "Id", "CardId", "Name", "Title"));
        return new RestSiteUpgradeCardCandidate(
            holder,
            label,
            cardId,
            screenBounds,
            TryResolveInteractiveEnabled(holder) ?? true,
            boundsSource);
    }

    private static CardSelectionObservation ObserveCardSelection(IEnumerable<object> roots, string? screenHint)
    {
        var screens = roots
            .Where(static root => TryResolveCardSelectionScreenType(root) is not null)
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .Select(root => new
            {
                Screen = root,
                ScreenType = TryResolveCardSelectionScreenType(root)!,
            })
            .ToArray();
        if (screens.Length == 0)
        {
            return new CardSelectionObservation(
                "unknown-card-select",
                ScreenDetected: false,
                ScreenVisible: false,
                RootType: null,
                Prompt: null,
                MinSelect: null,
                MaxSelect: null,
                SelectedCount: 0,
                RequireManualConfirmation: null,
                Cancelable: null,
                PreviewVisible: false,
                MainConfirmEnabled: false,
                PreviewConfirmEnabled: false,
                MainConfirmBounds: null,
                PreviewConfirmBounds: null,
                PreviewMode: null,
                SelectedCardIds: Array.Empty<string>(),
                VisibleCards: Array.Empty<CardSelectionCardCandidate>());
        }

        var preferredScreen = screens
            .OrderByDescending(entry => string.Equals(entry.ScreenType, screenHint, StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .ThenByDescending(entry => IsVisibleNode(entry.Screen) ? 1 : 0)
            .First();
        var screen = preferredScreen.Screen;
        var screenType = preferredScreen.ScreenType;
        var rootType = screen.GetType().FullName ?? screen.GetType().Name;
        var prefs = TryGetMemberValue(screen, "_prefs") ?? TryGetMemberValue(screen, "Prefs");
        var prompt = ResolveCardSelectionPrompt(screen, prefs);
        var minSelect = screenType switch
        {
            "reward-pick" => 1,
            "bundle-select" => 1,
            "relic-select" => 1,
            _ => TryReadInt(prefs, "MinSelect"),
        };
        var maxSelect = screenType switch
        {
            "reward-pick" => 1,
            "bundle-select" => 1,
            "relic-select" => 1,
            _ => TryReadInt(prefs, "MaxSelect"),
        };
        var requireManualConfirmation = screenType switch
        {
            "reward-pick" => false,
            "bundle-select" => true,
            "relic-select" => false,
            _ => TryReadBool(prefs, "RequireManualConfirmation"),
        };
        var cancelable = screenType switch
        {
            "reward-pick" => false,
            "bundle-select" => true,
            "relic-select" => true,
            _ => TryReadBool(prefs, "Cancelable"),
        };
        var selectedCards = screenType switch
        {
            "reward-pick" => Array.Empty<object>(),
            "bundle-select" => ExpandEnumerable(TryGetMemberValue(screen, "_selectedBundle")).ToArray(),
            "relic-select" when TryReadBool(screen, "_relicSelected") == true => new[] { screen },
            "relic-select" => Array.Empty<object>(),
            _ => ExpandEnumerable(TryGetMemberValue(screen, "_selectedCards")).ToArray(),
        };
        var selectedCardIds = ResolveCardSelectionSelectedCardIds(selectedCards);
        var visibleCards = EnumerateCardSelectionCandidates(screen, screenType, selectedCardIds)
            .OrderBy(static card => TryGetBoundsSortX(card.ScreenBounds))
            .ThenBy(static card => TryGetBoundsSortY(card.ScreenBounds))
            .ToArray();
        var selectedCount = screenType switch
        {
            "reward-pick" => 0,
            "bundle-select" => selectedCards.Length,
            "relic-select" => TryReadBool(screen, "_relicSelected") == true ? 1 : 0,
            _ => selectedCards.Length,
        };
        var mainConfirmButton = screenType switch
        {
            "transform" or "deck-remove" or "simple-select" => TryGetMemberValue(screen, "_confirmButton"),
            "bundle-select" => TryGetMemberValue(screen, "_previewConfirmButton") ?? TryGetMemberValue(screen, "_confirmButton"),
            _ => null,
        };
        var mainConfirmBounds = TryResolveInteractiveScreenBounds(mainConfirmButton, out _);
        var mainConfirmVisible = !string.IsNullOrWhiteSpace(mainConfirmBounds);
        var mainConfirmEnabled = mainConfirmVisible && (TryResolveInteractiveEnabled(mainConfirmButton) ?? TryResolveControlEnabled(mainConfirmButton) ?? true);
        var previewVisible = false;
        var previewConfirmEnabled = false;
        string? previewConfirmBounds = null;
        string? previewMode = null;

        switch (screenType)
        {
            case "transform":
            {
                var previewContainer = TryGetMemberValue(screen, "_previewContainer");
                previewVisible = IsVisibleNode(previewContainer);
                previewMode = previewVisible ? "transform-preview" : null;
                var previewConfirmButton = TryGetMemberValue(screen, "_previewConfirmButton");
                previewConfirmBounds = TryResolveInteractiveScreenBounds(previewConfirmButton, out _);
                previewConfirmEnabled = !string.IsNullOrWhiteSpace(previewConfirmBounds)
                                        && (TryResolveInteractiveEnabled(previewConfirmButton) ?? TryResolveControlEnabled(previewConfirmButton) ?? true);
                break;
            }
            case "deck-remove":
            {
                var previewContainer = TryGetMemberValue(screen, "_previewContainer");
                previewVisible = IsVisibleNode(previewContainer);
                previewMode = previewVisible ? "deck-preview" : null;
                var previewConfirmButton = TryGetMemberValue(screen, "_previewConfirmButton");
                previewConfirmBounds = TryResolveInteractiveScreenBounds(previewConfirmButton, out _);
                previewConfirmEnabled = !string.IsNullOrWhiteSpace(previewConfirmBounds)
                                        && (TryResolveInteractiveEnabled(previewConfirmButton) ?? TryResolveControlEnabled(previewConfirmButton) ?? true);
                break;
            }
            case "upgrade":
            {
                var previewConfirmButton = TryResolveVisibleUpgradeConfirmButton(screen, out var upgradePreviewMode);
                previewVisible = previewConfirmButton is not null;
                previewMode = upgradePreviewMode switch
                {
                    "single" => "upgrade-single-preview",
                    "multi" => "upgrade-multi-preview",
                    _ => null,
                };
                previewConfirmBounds = TryResolveInteractiveScreenBounds(previewConfirmButton, out _);
                previewConfirmEnabled = !string.IsNullOrWhiteSpace(previewConfirmBounds)
                                        && (TryResolveInteractiveEnabled(previewConfirmButton) ?? TryResolveControlEnabled(previewConfirmButton) ?? true);
                break;
            }
        }

        var screenVisible = IsVisibleNode(screen)
                            || visibleCards.Length > 0
                            || mainConfirmVisible
                            || previewVisible
                            || string.Equals(screenType, screenHint, StringComparison.OrdinalIgnoreCase);
        return new CardSelectionObservation(
            screenType,
            ScreenDetected: true,
            ScreenVisible: screenVisible,
            RootType: rootType,
            Prompt: prompt,
            MinSelect: minSelect,
            MaxSelect: maxSelect,
            SelectedCount: selectedCount,
            RequireManualConfirmation: requireManualConfirmation,
            Cancelable: cancelable,
            PreviewVisible: previewVisible,
            MainConfirmEnabled: mainConfirmEnabled,
            PreviewConfirmEnabled: previewConfirmEnabled,
            MainConfirmBounds: mainConfirmBounds,
            PreviewConfirmBounds: previewConfirmBounds,
            PreviewMode: previewMode,
            SelectedCardIds: selectedCardIds,
            VisibleCards: visibleCards);
    }

    private static string? TryResolveCardSelectionScreenType(object candidate)
    {
        var typeName = candidate.GetType().FullName ?? candidate.GetType().Name;
        if (typeName.Contains("NCardRewardSelectionScreen", StringComparison.OrdinalIgnoreCase))
        {
            return "reward-pick";
        }

        if (typeName.Contains("NChooseACardSelectionScreen", StringComparison.OrdinalIgnoreCase))
        {
            return "reward-pick";
        }

        if (typeName.Contains("NDeckTransformSelectScreen", StringComparison.OrdinalIgnoreCase))
        {
            return "transform";
        }

        if (typeName.Contains("NDeckUpgradeSelectScreen", StringComparison.OrdinalIgnoreCase))
        {
            return "upgrade";
        }

        if (typeName.Contains("NDeckCardSelectScreen", StringComparison.OrdinalIgnoreCase))
        {
            return "deck-remove";
        }

        if (typeName.Contains("NSimpleCardSelectScreen", StringComparison.OrdinalIgnoreCase))
        {
            return "simple-select";
        }

        if (typeName.Contains("NChooseABundleSelectionScreen", StringComparison.OrdinalIgnoreCase))
        {
            return "bundle-select";
        }

        if (typeName.Contains("NChooseARelicSelection", StringComparison.OrdinalIgnoreCase))
        {
            return "relic-select";
        }

        return null;
    }

    private static string? ResolveCardSelectionPrompt(object screen, object? prefs)
    {
        var prompt = TryReadString(prefs, "Prompt");
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            return prompt;
        }

        prompt = TryReadString(TryGetMemberValue(screen, "_infoLabel"), "Text");
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            return prompt;
        }

        var banner = TryGetMemberValue(screen, "_banner");
        prompt = FirstNonEmpty(
            TryResolveChoiceLabel(banner),
            TryReadString(TryGetMemberValue(banner, "label"), "Text"),
            TryReadString(TryGetMemberValue(banner, "Label"), "Text"),
            TryReadString(banner, "label", "Label"));
        return string.IsNullOrWhiteSpace(prompt) ? null : prompt;
    }

    private static IReadOnlyList<string> ResolveCardSelectionSelectedCardIds(IEnumerable<object> selectedCards)
    {
        return selectedCards
            .Select(static card => FirstNonEmpty(
                TryReadString(card, "Id", "CardId", "Name", "Title"),
                TryResolveChoiceLabel(card)))
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()!;
    }

    private static IReadOnlyList<CardSelectionCardCandidate> EnumerateCardSelectionCandidates(
        object screen,
        string screenType,
        IReadOnlyList<string> selectedCardIds)
    {
        IEnumerable<object> holders = screenType switch
        {
            "reward-pick" => EnumerateRewardPickCardHolders(screen),
            "simple-select" => EnumerateUpgradeCardHolders(screen),
            _ => EnumerateUpgradeCardHolders(screen),
        };

        return holders
            .Select(holder => TryCreateCardSelectionCandidate(holder, selectedCardIds))
            .Where(static candidate => candidate is not null)
            .Cast<CardSelectionCardCandidate>()
            .ToArray();
    }

    private static IEnumerable<object> EnumerateRewardPickCardHolders(object screen)
    {
        var seen = new HashSet<int>();
        var cardRow = TryGetMemberValue(screen, "_cardRow") ?? TryGetMemberValue(screen, "CardRow");
        foreach (var holder in ExpandEnumerable(TryEnumerateChildren(cardRow)))
        {
            if (holder is not null && seen.Add(RuntimeHelpers.GetHashCode(holder)))
            {
                yield return holder;
            }
        }

        foreach (var holder in ExpandEnumerable(cardRow))
        {
            if (holder is not null && seen.Add(RuntimeHelpers.GetHashCode(holder)))
            {
                yield return holder;
            }
        }
    }

    private static CardSelectionCardCandidate? TryCreateCardSelectionCandidate(object holder, IReadOnlyList<string> selectedCardIds)
    {
        var screenBounds = TryResolveInteractiveScreenBounds(holder, out var boundsSource);
        if (string.IsNullOrWhiteSpace(screenBounds))
        {
            return null;
        }

        var cardModel = TryGetMemberValue(holder, "CardModel")
                        ?? TryGetMemberValue(holder, "Card")
                        ?? TryGetMemberValue(holder, "CardNode")
                        ?? TryGetMemberValue(holder, "Model");
        var label = FirstNonEmpty(
            TryResolveChoiceLabel(cardModel ?? holder),
            TryReadString(cardModel, "Title", "CardName", "DisplayName", "Name", "Id", "CardId"),
            TryReadString(holder, "Title", "CardName", "DisplayName", "Name", "Id", "CardId"));
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        var cardId = FirstNonEmpty(
            TryReadString(cardModel, "Id", "CardId", "Name", "Title"),
            TryReadString(holder, "Id", "CardId", "Name", "Title"));
        var selected = !string.IsNullOrWhiteSpace(cardId) && selectedCardIds.Contains(cardId, StringComparer.OrdinalIgnoreCase)
                       || selectedCardIds.Contains(label, StringComparer.OrdinalIgnoreCase);
        return new CardSelectionCardCandidate(
            holder,
            label,
            cardId,
            screenBounds,
            TryResolveInteractiveEnabled(holder) ?? true,
            selected,
            boundsSource);
    }

    private static bool TryResolveRestSiteButtonEnabled(object button, object? option)
    {
        var isUnclickable = TryReadBool(button, "_isUnclickable");
        if (isUnclickable is not null)
        {
            return !isUnclickable.Value;
        }

        return TryResolveControlEnabled(button)
               ?? TryReadBool(option, "IsEnabled")
               ?? true;
    }

    private static TreasureRoomObservation ObserveTreasureRoom(IEnumerable<object> roots, string? screenHint)
    {
        var rooms = roots
            .Where(static root => IsTreasureRoomType(root.GetType().FullName ?? root.GetType().Name))
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();
        if (rooms.Length == 0)
        {
            return new TreasureRoomObservation(
                RoomDetected: false,
                RoomVisible: false,
                RootType: null,
                ChestClickable: false,
                ChestOpened: false,
                RelicCollectionOpen: false,
                SharedRelicPickingActive: false,
                MapScreenOpen: false,
                ChestBounds: null,
                RelicHolderCount: 0,
                VisibleRelicHolderCount: 0,
                EnabledRelicHolderCount: 0,
                ProceedEnabled: false,
                ProceedBounds: null,
                RelicHolderIds: Array.Empty<string>(),
                VisibleRelicHolders: Array.Empty<TreasureRoomRelicHolderCandidate>());
        }

        var room = rooms
            .OrderByDescending(static candidate => IsVisibleNode(candidate) ? 1 : 0)
            .ThenByDescending(candidate => string.Equals(screenHint, "map", StringComparison.OrdinalIgnoreCase)
                                           || string.Equals(screenHint, "event", StringComparison.OrdinalIgnoreCase)
                ? 1
                : 0)
            .First();
        var roomType = room.GetType().FullName ?? room.GetType().Name;

        var chest = FindTreasureChest(room, roots);
        var chestBounds = TryResolveInteractiveScreenBounds(chest, out _);
        var chestClickable = !string.IsNullOrWhiteSpace(chestBounds)
                             && (TryResolveInteractiveEnabled(chest) ?? TryResolveControlEnabled(chest) ?? true);

        var proceedButton = FindTreasureProceedButton(room, roots);
        var proceedBounds = TryResolveInteractiveScreenBounds(proceedButton, out _);
        var proceedEnabled = !string.IsNullOrWhiteSpace(proceedBounds)
                             && (TryResolveInteractiveEnabled(proceedButton) ?? TryResolveControlEnabled(proceedButton) ?? true);

        var holders = EnumerateTreasureRelicHolders(room, roots)
            .Select(TryCreateTreasureRelicHolderCandidate)
            .Where(static candidate => candidate is not null)
            .Cast<TreasureRoomRelicHolderCandidate>()
            .ToArray();
        var relicHolderIds = holders
            .Select(static holder => holder.RelicId ?? holder.Label)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()!;
        var visibleRelicHolderCount = holders.Length;
        var enabledRelicHolderCount = holders.Count(static holder => holder.Enabled);
        var relicCollectionOpen = TryReadBool(room, "_isRelicCollectionOpen", "IsRelicCollectionOpen", "RelicCollectionOpen") == true;
        var sharedRelicPickingActive = TryResolveSharedRelicPickingActive(roots) == true;
        var mapScreenOpen = TryResolveMapScreenOpen(roots) == true;
        var chestOpenedSignal = TryReadBool(room, "ChestOpened", "IsChestOpened", "_chestOpened")
                                ?? TryReadBool(chest, "Opened", "IsOpened", "_opened");
        var chestOpened = chestOpenedSignal == true
                          || visibleRelicHolderCount > 0
                          || proceedEnabled;
        var staleAfterProceedResidue = mapScreenOpen
                                       && !sharedRelicPickingActive
                                       && !relicCollectionOpen
                                       && !proceedEnabled;
        if (staleAfterProceedResidue)
        {
            holders = Array.Empty<TreasureRoomRelicHolderCandidate>();
            relicHolderIds = Array.Empty<string>();
            visibleRelicHolderCount = 0;
            enabledRelicHolderCount = 0;
        }

        var roomVisible = !staleAfterProceedResidue
                          && (IsVisibleNode(room)
                              || chestClickable
                              || relicCollectionOpen
                              || sharedRelicPickingActive
                              || visibleRelicHolderCount > 0
                              || proceedEnabled);
        var roomDetected = !staleAfterProceedResidue
                           && (roomVisible
                               || relicCollectionOpen
                               || sharedRelicPickingActive
                               || chestClickable
                               || proceedEnabled);
        return new TreasureRoomObservation(
            RoomDetected: roomDetected,
            RoomVisible: roomVisible,
            RootType: roomType,
            ChestClickable: chestClickable,
            ChestOpened: chestOpened,
            RelicCollectionOpen: relicCollectionOpen,
            SharedRelicPickingActive: sharedRelicPickingActive,
            MapScreenOpen: mapScreenOpen,
            ChestBounds: chestBounds,
            RelicHolderCount: holders.Length,
            VisibleRelicHolderCount: visibleRelicHolderCount,
            EnabledRelicHolderCount: enabledRelicHolderCount,
            ProceedEnabled: proceedEnabled,
            ProceedBounds: proceedBounds,
            RelicHolderIds: relicHolderIds,
            VisibleRelicHolders: holders);
    }

    private static bool? TryResolveSharedRelicPickingActive(IEnumerable<object> roots)
    {
        foreach (var root in roots)
        {
            var tracker = TryGetScreenStateTracker(root);
            var active = TryReadBool(tracker, "IsInSharedRelicPickingScreen", "_isInSharedRelicPicking", "SharedRelicPickingActive", "_sharedRelicPickingActive");
            if (active is not null)
            {
                return active;
            }
        }

        return null;
    }

    private static object? TryGetScreenStateTracker(object? root)
    {
        if (root is null)
        {
            return null;
        }

        var typeName = root.GetType().FullName ?? root.GetType().Name;
        if (typeName.Contains("ScreenStateTracker", StringComparison.OrdinalIgnoreCase))
        {
            return root;
        }

        return TryGetMemberValue(root, "ScreenStateTracker")
               ?? TryGetMemberValue(root, "_screenStateTracker");
    }

    private static bool? TryResolveMapScreenOpen(IEnumerable<object> roots)
    {
        foreach (var root in roots)
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            if (!typeName.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var isOpen = TryReadBool(root, "IsOpen", "_isOpen", "Open");
            if (isOpen is not null)
            {
                return isOpen;
            }

            if (IsVisibleNode(root))
            {
                return true;
            }
        }

        return null;
    }

    private static bool IsTreasureRoomType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && (typeName.Contains("NTreasureRoom", StringComparison.OrdinalIgnoreCase)
                   || typeName.Contains(".TreasureRoom", StringComparison.OrdinalIgnoreCase));
    }

    private static object? FindTreasureChest(object room, IEnumerable<object> roots)
    {
        var direct = TryGetMemberValue(room, "Chest")
                     ?? TryGetMemberValue(room, "_chest")
                     ?? TryGetMemberValue(room, "TreasureButton")
                     ?? TryGetMemberValue(room, "_treasureButton");
        if (direct is not null)
        {
            return direct;
        }

        return roots.FirstOrDefault(static root =>
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            return typeName.Contains("NTreasureButton", StringComparison.OrdinalIgnoreCase);
        });
    }

    private static object? FindTreasureProceedButton(object room, IEnumerable<object> roots)
    {
        var direct = TryGetMemberValue(room, "ProceedButton")
                     ?? TryGetMemberValue(room, "_proceedButton");
        if (direct is not null)
        {
            return direct;
        }

        return roots.FirstOrDefault(static root =>
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            return typeName.Contains("NProceedButton", StringComparison.OrdinalIgnoreCase);
        });
    }

    private static IEnumerable<object> EnumerateTreasureRelicHolders(object room, IEnumerable<object> roots)
    {
        var seen = new HashSet<int>();
        foreach (var candidate in EnumerateTreasureHolderContainers(room, roots))
        {
            if (candidate is null)
            {
                continue;
            }

            foreach (var holder in ExpandEnumerable(candidate))
            {
                if (holder is not null
                    && IsTreasureRelicHolderType(holder.GetType().FullName ?? holder.GetType().Name)
                    && seen.Add(RuntimeHelpers.GetHashCode(holder)))
                {
                    yield return holder;
                }
            }

            foreach (var child in TryEnumerateChildren(candidate))
            {
                if (child is not null
                    && IsTreasureRelicHolderType(child.GetType().FullName ?? child.GetType().Name)
                    && seen.Add(RuntimeHelpers.GetHashCode(child)))
                {
                    yield return child;
                }
            }
        }
    }

    private static IEnumerable<object?> EnumerateTreasureHolderContainers(object room, IEnumerable<object> roots)
    {
        yield return TryGetMemberValue(room, "_relicCollection");
        yield return TryGetMemberValue(room, "RelicCollection");
        yield return TryGetMemberValue(room, "_treasureRoomRelicCollection");

        foreach (var root in roots)
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            if (typeName.Contains("NTreasureRoomRelicCollection", StringComparison.OrdinalIgnoreCase))
            {
                yield return root;
                yield return TryGetMemberValue(root, "SingleplayerRelicHolder");
                yield return TryGetMemberValue(root, "_singleplayerRelicHolder");
                yield return TryGetMemberValue(root, "_holdersInUse");
                yield return TryGetMemberValue(root, "HoldersInUse");
                yield return TryGetMemberValue(root, "_holders");
            }
        }
    }

    private static bool IsTreasureRelicHolderType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && (typeName.Contains("TreasureRoomRelicHolder", StringComparison.OrdinalIgnoreCase)
                   || typeName.Contains("TreasureRelicHolder", StringComparison.OrdinalIgnoreCase)
                   || typeName.Contains("SingleplayerRelicHolder", StringComparison.OrdinalIgnoreCase));
    }

    private static TreasureRoomRelicHolderCandidate? TryCreateTreasureRelicHolderCandidate(object holder)
    {
        var screenBounds = TryResolveInteractiveScreenBounds(holder, out var boundsSource);
        if (string.IsNullOrWhiteSpace(screenBounds))
        {
            return null;
        }

        var relic = TryGetMemberValue(holder, "Relic")
                    ?? TryGetMemberValue(holder, "Model")
                    ?? TryGetMemberValue(holder, "_relic")
                    ?? TryGetMemberValue(holder, "_model");
        var label = FirstNonEmpty(
            TryResolveChoiceLabel(relic ?? holder),
            TryReadString(relic, "Title", "DisplayName", "Name", "Id"),
            TryReadString(holder, "Title", "DisplayName", "Name", "Id"));
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        var relicId = FirstNonEmpty(
            TryReadString(relic, "Id", "Name", "Title"),
            TryReadString(holder, "Id", "Name", "Title"));
        return new TreasureRoomRelicHolderCandidate(
            holder,
            label,
            relicId,
            screenBounds,
            TryResolveInteractiveEnabled(holder) ?? TryResolveControlEnabled(holder) ?? true,
            boundsSource);
    }

    private static ShopObservation ObserveShopRoom(IEnumerable<object> roots, string? screenHint)
    {
        var rooms = roots
            .Where(static root => IsShopRoomType(root.GetType().FullName ?? root.GetType().Name))
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();
        if (rooms.Length == 0)
        {
            return new ShopObservation(
                RoomDetected: false,
                RoomVisible: false,
                ForegroundOwned: false,
                TeardownInProgress: false,
                ShopIsCurrentActiveScreen: false,
                MapIsCurrentActiveScreen: false,
                ActiveScreenType: null,
                RootType: null,
                InventoryOpen: false,
                MerchantButtonVisible: false,
                MerchantButtonEnabled: false,
                MerchantButtonBounds: null,
                ProceedEnabled: false,
                ProceedBounds: null,
                BackVisible: false,
                BackEnabled: false,
                BackBounds: null,
                OptionCount: 0,
                AffordableOptionCount: 0,
                AffordableOptionIds: Array.Empty<string>(),
                AffordableRelicIds: Array.Empty<string>(),
                AffordableCardIds: Array.Empty<string>(),
                AffordablePotionIds: Array.Empty<string>(),
                CardRemovalVisible: false,
                CardRemovalEnabled: false,
                CardRemovalEnoughGold: false,
                CardRemovalUsed: false,
                VisibleOptions: Array.Empty<ShopOptionCandidate>());
        }

        var room = rooms
            .OrderByDescending(static candidate => IsVisibleNode(candidate) ? 1 : 0)
            .ThenByDescending(candidate => string.Equals(screenHint, "shop", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .First();
        var rootType = room.GetType().FullName ?? room.GetType().Name;
        var activeScreen = TryResolveCurrentActiveScreen(roots);
        var activeScreenType = activeScreen?.GetType().FullName ?? activeScreen?.GetType().Name;
        var shopIsCurrentActiveScreen = IsShopActiveScreenType(activeScreenType);
        var mapIsCurrentActiveScreen = IsMapScreenType(activeScreenType);
        var inventory = TryGetMemberValue(room, "Inventory")
                        ?? roots.FirstOrDefault(static root => IsMerchantInventoryType(root.GetType().FullName ?? root.GetType().Name));
        var inventoryOpen = TryReadBool(inventory, "IsOpen") == true;
        var merchantButton = TryGetMemberValue(room, "MerchantButton")
                             ?? roots.FirstOrDefault(static root => IsMerchantButtonType(root.GetType().FullName ?? root.GetType().Name));
        var merchantButtonBounds = TryResolveInteractiveScreenBounds(merchantButton, out _);
        var merchantButtonVisible = !inventoryOpen
                                    && !string.IsNullOrWhiteSpace(merchantButtonBounds)
                                    && IsVisibleNode(merchantButton);
        var merchantButtonEnabled = merchantButtonVisible
                                    && (TryResolveInteractiveEnabled(merchantButton) ?? TryResolveControlEnabled(merchantButton) ?? true);

        var proceedButton = TryGetMemberValue(room, "ProceedButton")
                            ?? TryGetMemberValue(room, "_proceedButton")
                            ?? roots.FirstOrDefault(static root => IsProceedButtonType(root.GetType().FullName ?? root.GetType().Name));
        var proceedBounds = TryResolveInteractiveScreenBounds(proceedButton, out _);
        var proceedEnabled = !string.IsNullOrWhiteSpace(proceedBounds)
                             && (TryResolveInteractiveEnabled(proceedButton) ?? TryResolveControlEnabled(proceedButton) ?? true);

        var backButton = TryGetMemberValue(inventory, "_backButton")
                         ?? TryGetMemberValue(inventory, "BackButton");
        var backBounds = TryResolveInteractiveScreenBounds(backButton, out _);
        var backVisible = inventoryOpen
                          && !string.IsNullOrWhiteSpace(backBounds)
                          && IsVisibleNode(backButton);
        var backEnabled = backVisible
                          && (TryResolveInteractiveEnabled(backButton) ?? TryResolveControlEnabled(backButton) ?? true);

        var visibleOptions = EnumerateMerchantSlots(inventory)
            .Select(TryCreateShopOptionCandidate)
            .Where(static candidate => candidate is not null)
            .Cast<ShopOptionCandidate>()
            .OrderBy(static candidate => TryGetBoundsSortX(candidate.ScreenBounds))
            .ThenBy(static candidate => TryGetBoundsSortY(candidate.ScreenBounds))
            .ToArray();
        var affordableOptions = visibleOptions
            .Where(static option => option.Enabled && option.IsStocked && option.EnoughGold && !option.Used)
            .ToArray();
        var affordableOptionIds = affordableOptions
            .Select(static option => option.EntryId ?? option.Label)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()!;
        var affordableRelicIds = affordableOptions
            .Where(static option => string.Equals(option.OptionType, "relic", StringComparison.OrdinalIgnoreCase))
            .Select(static option => option.EntryId ?? option.Label)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()!;
        var affordableCardIds = affordableOptions
            .Where(static option => string.Equals(option.OptionType, "card", StringComparison.OrdinalIgnoreCase))
            .Select(static option => option.EntryId ?? option.Label)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()!;
        var affordablePotionIds = affordableOptions
            .Where(static option => string.Equals(option.OptionType, "potion", StringComparison.OrdinalIgnoreCase))
            .Select(static option => option.EntryId ?? option.Label)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()!;
        var cardRemoval = visibleOptions.FirstOrDefault(static option => string.Equals(option.OptionType, "card-removal", StringComparison.OrdinalIgnoreCase));
        var roomVisible = string.Equals(screenHint, "shop", StringComparison.OrdinalIgnoreCase)
                          || IsVisibleNode(room)
                          || inventoryOpen
                          || merchantButtonVisible
                          || backVisible
                          || proceedEnabled
                          || visibleOptions.Length > 0;
        var foregroundOwned = shopIsCurrentActiveScreen
                              || inventoryOpen
                              || merchantButtonEnabled
                              || proceedEnabled
                              || (backVisible && backEnabled);
        var teardownInProgress = roomVisible
                                 && !foregroundOwned
                                 && !inventoryOpen
                                 && !backVisible
                                 && !proceedEnabled
                                 && (mapIsCurrentActiveScreen
                                     || (!merchantButtonEnabled && merchantButtonVisible));
        return new ShopObservation(
            RoomDetected: true,
            RoomVisible: roomVisible,
            ForegroundOwned: foregroundOwned,
            TeardownInProgress: teardownInProgress,
            ShopIsCurrentActiveScreen: shopIsCurrentActiveScreen,
            MapIsCurrentActiveScreen: mapIsCurrentActiveScreen,
            ActiveScreenType: activeScreenType,
            RootType: rootType,
            InventoryOpen: inventoryOpen,
            MerchantButtonVisible: merchantButtonVisible,
            MerchantButtonEnabled: merchantButtonEnabled,
            MerchantButtonBounds: merchantButtonBounds,
            ProceedEnabled: proceedEnabled,
            ProceedBounds: proceedBounds,
            BackVisible: backVisible,
            BackEnabled: backEnabled,
            BackBounds: backBounds,
            OptionCount: visibleOptions.Length,
            AffordableOptionCount: affordableOptions.Length,
            AffordableOptionIds: affordableOptionIds,
            AffordableRelicIds: affordableRelicIds,
            AffordableCardIds: affordableCardIds,
            AffordablePotionIds: affordablePotionIds,
            CardRemovalVisible: cardRemoval is not null,
            CardRemovalEnabled: cardRemoval?.Enabled == true && cardRemoval.IsStocked && !cardRemoval.Used,
            CardRemovalEnoughGold: cardRemoval?.EnoughGold == true,
            CardRemovalUsed: cardRemoval?.Used == true,
            VisibleOptions: visibleOptions);
    }

    private static RewardObservation ObserveRewardScreen(IEnumerable<object> roots, string? screenHint)
    {
        var rewardScreens = roots
            .Where(static root => IsRewardScreenType(root.GetType().FullName ?? root.GetType().Name))
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();
        var activeScreen = TryResolveCurrentActiveScreen(roots);
        var activeScreenType = activeScreen?.GetType().FullName ?? activeScreen?.GetType().Name;
        var overlayTop = TryResolveOverlayStackTop(roots);
        var overlayTopType = overlayTop?.GetType().FullName ?? overlayTop?.GetType().Name;
        var rewardIsCurrentActiveScreen = IsRewardScreenType(activeScreenType);
        var rewardIsTopOverlay = IsRewardScreenType(overlayTopType);
        var mapIsCurrentActiveScreen = IsMapScreenType(activeScreenType);
        var gameOverScreenDetected = rewardScreens.Any(static screen => IsGameOverScreenType(screen.GetType().FullName ?? screen.GetType().Name))
                                     || IsGameOverScreenType(activeScreenType);
        var timelineUnlockDetected = rewardScreens.Any(static screen => IsTimelineUnlockScreenType(screen.GetType().FullName ?? screen.GetType().Name))
                                     || IsTimelineUnlockScreenType(activeScreenType);
        var unlockScreenDetected = rewardScreens.Any(static screen => IsUnlockScreenType(screen.GetType().FullName ?? screen.GetType().Name))
                                   || IsUnlockScreenType(activeScreenType);
        var mainMenuReturnDetected = IsMainMenuActiveScreenType(activeScreenType)
                                     || string.Equals(screenHint, "main-menu", StringComparison.OrdinalIgnoreCase);
        var terminalRunBoundary = gameOverScreenDetected || timelineUnlockDetected || unlockScreenDetected || mainMenuReturnDetected;
        if (rewardScreens.Length == 0 && !rewardIsCurrentActiveScreen && !rewardIsTopOverlay)
        {
            return new RewardObservation(
                ScreenDetected: false,
                ScreenVisible: false,
                ForegroundOwned: false,
                TeardownInProgress: false,
                RewardIsCurrentActiveScreen: false,
                RewardIsTopOverlay: false,
                MapIsCurrentActiveScreen: mapIsCurrentActiveScreen,
                ActiveScreenType: activeScreenType,
                TopOverlayType: overlayTopType,
                RootType: null,
                ProceedVisible: false,
                ProceedEnabled: false,
                VisibleButtonCount: 0,
                EnabledButtonCount: 0,
                HasOpenPotionSlots: TryReadHasOpenPotionSlots(roots),
                TerminalRunBoundary: terminalRunBoundary,
                GameOverScreenDetected: gameOverScreenDetected,
                UnlockScreenDetected: unlockScreenDetected,
                TimelineUnlockDetected: timelineUnlockDetected,
                MainMenuReturnDetected: mainMenuReturnDetected);
        }

        var rewardScreen = rewardScreens
            .OrderByDescending(static candidate => IsVisibleNode(candidate) ? 1 : 0)
            .FirstOrDefault()
            ?? activeScreen
            ?? overlayTop;
        var rootType = rewardScreen?.GetType().FullName ?? rewardScreen?.GetType().Name;
        var proceedButton = rewardScreen is null
            ? null
            : TryGetMemberValue(rewardScreen, "ProceedButton")
              ?? TryGetMemberValue(rewardScreen, "_proceedButton");
        var proceedBounds = TryResolveInteractiveScreenBounds(proceedButton, out _);
        var proceedVisible = !string.IsNullOrWhiteSpace(proceedBounds) && IsVisibleNode(proceedButton);
        var proceedEnabled = proceedVisible
                             && (TryResolveInteractiveEnabled(proceedButton) ?? TryResolveControlEnabled(proceedButton) ?? true);
        var rewardButtons = EnumerateRewardButtons(rewardScreen).ToArray();
        var visibleButtonCount = rewardButtons.Count(static button =>
            !string.IsNullOrWhiteSpace(TryResolveInteractiveScreenBounds(button, out _)) && IsVisibleNode(button));
        var enabledButtonCount = rewardButtons.Count(button =>
            !string.IsNullOrWhiteSpace(TryResolveInteractiveScreenBounds(button, out _))
            && IsVisibleNode(button)
            && (TryResolveInteractiveEnabled(button) ?? TryResolveControlEnabled(button) ?? true));
        var hasOpenPotionSlots = TryReadHasOpenPotionSlots(roots);
        var screenDetected = rewardScreen is not null;
        var screenVisible = rewardIsCurrentActiveScreen
                            || rewardIsTopOverlay
                            || string.Equals(screenHint, "rewards", StringComparison.OrdinalIgnoreCase)
                            || (rewardScreen is not null && IsVisibleNode(rewardScreen))
                            || proceedVisible
                            || visibleButtonCount > 0;
        var actionableRewardAffordance = proceedEnabled || enabledButtonCount > 0;
        var staleTopOverlayTeardown = rewardIsTopOverlay
                                      && !rewardIsCurrentActiveScreen
                                      && mapIsCurrentActiveScreen
                                      && !actionableRewardAffordance;
        var foregroundOwned = !terminalRunBoundary
                              && !staleTopOverlayTeardown
                              && (rewardIsCurrentActiveScreen
                                  || proceedEnabled
                                  || enabledButtonCount > 0
                                  || (rewardIsTopOverlay && actionableRewardAffordance && !mapIsCurrentActiveScreen));
        var teardownInProgress = screenVisible
                                 && !terminalRunBoundary
                                 && (staleTopOverlayTeardown
                                     || (!foregroundOwned
                                         && (mapIsCurrentActiveScreen
                                             || string.Equals(screenHint, "map", StringComparison.OrdinalIgnoreCase))));
        return new RewardObservation(
            ScreenDetected: screenDetected,
            ScreenVisible: screenVisible,
            ForegroundOwned: foregroundOwned,
            TeardownInProgress: teardownInProgress,
            RewardIsCurrentActiveScreen: rewardIsCurrentActiveScreen,
            RewardIsTopOverlay: rewardIsTopOverlay,
            MapIsCurrentActiveScreen: mapIsCurrentActiveScreen,
            ActiveScreenType: activeScreenType,
            TopOverlayType: overlayTopType,
            RootType: rootType,
            ProceedVisible: proceedVisible,
            ProceedEnabled: proceedEnabled,
            VisibleButtonCount: visibleButtonCount,
            EnabledButtonCount: enabledButtonCount,
            HasOpenPotionSlots: hasOpenPotionSlots,
            TerminalRunBoundary: terminalRunBoundary,
            GameOverScreenDetected: gameOverScreenDetected,
            UnlockScreenDetected: unlockScreenDetected,
            TimelineUnlockDetected: timelineUnlockDetected,
            MainMenuReturnDetected: mainMenuReturnDetected);
    }

    private static bool IsShopRoomType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NMerchantRoom", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRewardScreenType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("RewardsScreen", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGameOverScreenType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("GameOverScreen", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTimelineUnlockScreenType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("UnlockTimelineScreen", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUnlockScreenType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("UnlockScreen", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMainMenuActiveScreenType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && (typeName.Contains("MainMenu", StringComparison.OrdinalIgnoreCase)
                   || typeName.Contains("Title", StringComparison.OrdinalIgnoreCase)
                   || typeName.Contains("FrontEnd", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsMerchantInventoryType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NMerchantInventory", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMerchantButtonType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NMerchantButton", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsShopActiveScreenType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && (typeName.Contains("NMerchantRoom", StringComparison.OrdinalIgnoreCase)
                   || typeName.Contains("NMerchantInventory", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsMapScreenType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMapPointType(Type? type)
    {
        for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
        {
            var typeName = current.FullName ?? current.Name;
            if (typeName.Contains("NMapPoint", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("MapPoint", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static object? TryResolveOverlayStackTop(IEnumerable<object> roots)
    {
        foreach (var root in roots)
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            if (typeName.Contains("NOverlayStack", StringComparison.OrdinalIgnoreCase))
            {
                return TryInvokeMethod(root, "Peek")
                       ?? TryGetMemberValue(root, "_stack")
                       ?? TryGetMemberValue(root, "Current");
            }
        }

        var overlayStackType = FindLoadedType("MegaCrit.Sts2.Core.Nodes.Screens.Overlays.NOverlayStack");
        if (overlayStackType is null)
        {
            return null;
        }

        try
        {
            var instance = overlayStackType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            return instance is null
                ? null
                : TryInvokeMethod(instance, "Peek");
        }
        catch
        {
            return null;
        }
    }

    private static IEnumerable<object> EnumerateRewardButtons(object? rewardScreen)
    {
        if (rewardScreen is null)
        {
            yield break;
        }

        var seen = new HashSet<int>();
        foreach (var button in ExpandEnumerable(TryGetMemberValue(rewardScreen, "_rewardButtons")))
        {
            if (button is not null && seen.Add(RuntimeHelpers.GetHashCode(button)))
            {
                yield return button;
            }
        }

        foreach (var child in TryEnumerateChildren(TryGetMemberValue(rewardScreen, "_rewardAlternativesContainer")))
        {
            if (child is not null && seen.Add(RuntimeHelpers.GetHashCode(child)))
            {
                yield return child;
            }
        }
    }

    private static object? TryResolveCurrentActiveScreen(IEnumerable<object> roots)
    {
        foreach (var root in roots)
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            if (typeName.Contains("ActiveScreenContext", StringComparison.OrdinalIgnoreCase))
            {
                return TryInvokeMethod(root, "GetCurrentScreen");
            }
        }

        var activeScreenContextType = FindLoadedType("MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext.ActiveScreenContext");
        if (activeScreenContextType is null)
        {
            return null;
        }

        try
        {
            var instance = activeScreenContextType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            return instance is null
                ? null
                : TryInvokeMethod(instance, "GetCurrentScreen");
        }
        catch
        {
            return null;
        }
    }

    private static object? TryResolveGameInstance(IEnumerable<object> roots)
    {
        foreach (var root in roots)
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            if (string.Equals(typeName, "MegaCrit.Sts2.Core.Nodes.NGame", StringComparison.Ordinal))
            {
                return root;
            }
        }

        var gameType = FindLoadedType("MegaCrit.Sts2.Core.Nodes.NGame");
        if (gameType is null)
        {
            return null;
        }

        try
        {
            return gameType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null)
                   ?? gameType.GetProperty("Current", BindingFlags.Public | BindingFlags.Static)?.GetValue(null)
                   ?? gameType.GetProperty("Singleton", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        }
        catch
        {
            return null;
        }
    }

    private static RootSceneTransitionObservation ObserveRootSceneTransition(IEnumerable<object> roots)
    {
        var game = TryResolveGameInstance(roots);
        var transition = game is null ? null : TryGetMemberValue(game, "Transition");
        var rootSceneContainer = game is null ? null : TryGetMemberValue(game, "RootSceneContainer");
        var currentRunNode = game is null
            ? null
            : TryGetMemberValue(game, "CurrentRunNode")
              ?? TryGetMemberValue(game, "_currentRunNode");
        var mainLoop = TryResolveMainLoop();
        var rootScene = rootSceneContainer is null
            ? null
            : TryGetMemberValue(rootSceneContainer, "CurrentScene");
        rootScene ??= currentRunNode;
        if (rootScene is null && mainLoop is not null)
        {
            rootScene = TryGetMemberValue(mainLoop, "CurrentScene");
        }

        var currentRunRoom = currentRunNode is null
            ? null
            : TryGetMemberValue(currentRunNode, "CurrentRoom")
              ?? TryGetMemberValue(currentRunNode, "_currentRoom")
              ?? TryGetMemberValue(currentRunNode, "Room");
        var currentRunRoomScene = currentRunRoom is null
            ? null
            : TryGetMemberValue(currentRunRoom, "CurrentScene")
              ?? TryGetMemberValue(currentRunRoom, "RoomScene")
              ?? TryGetMemberValue(currentRunRoom, "Screen");

        var rootSceneCurrentType = rootScene?.GetType().FullName ?? rootScene?.GetType().Name;
        var currentRunRoomType = currentRunRoom?.GetType().FullName ?? currentRunRoom?.GetType().Name;
        var currentRunRoomSceneType = currentRunRoomScene?.GetType().FullName ?? currentRunRoomScene?.GetType().Name;

        return new RootSceneTransitionObservation(
            TryReadBool(transition, "InTransition") == true,
            rootSceneCurrentType,
            rootSceneCurrentType?.Contains("MainMenu", StringComparison.OrdinalIgnoreCase) == true,
            rootSceneCurrentType?.Contains(".NRun", StringComparison.OrdinalIgnoreCase) == true
            || rootSceneCurrentType?.EndsWith(".NRun", StringComparison.OrdinalIgnoreCase) == true,
            currentRunNode is not null,
            currentRunRoomType,
            currentRunRoomSceneType);
    }

    private static void AppendRootSceneTransitionMetadata(
        RootSceneTransitionObservation observation,
        Dictionary<string, string?> meta,
        Dictionary<string, object?> payload)
    {
        meta["transitionInProgress"] = observation.TransitionInProgress ? "true" : "false";
        meta["rootSceneCurrentType"] = observation.RootSceneCurrentType;
        meta["rawRootSceneCurrentType"] = observation.RootSceneCurrentType;
        meta["rootSceneIsMainMenu"] = observation.RootSceneIsMainMenu ? "true" : "false";
        meta["rootSceneIsRun"] = observation.RootSceneIsRun ? "true" : "false";
        meta["currentRunNodePresent"] = observation.CurrentRunNodePresent ? "true" : "false";
        meta["currentRunRoomType"] = observation.CurrentRunRoomType;
        meta["rawCurrentRunRoomType"] = observation.CurrentRunRoomType;
        meta["currentRunRoomSceneType"] = observation.CurrentRunRoomSceneType;
        meta["rawCurrentRunRoomSceneType"] = observation.CurrentRunRoomSceneType;

        payload["transitionInProgress"] = observation.TransitionInProgress;
        payload["rootSceneCurrentType"] = observation.RootSceneCurrentType;
        payload["rawRootSceneCurrentType"] = observation.RootSceneCurrentType;
        payload["rootSceneIsMainMenu"] = observation.RootSceneIsMainMenu;
        payload["rootSceneIsRun"] = observation.RootSceneIsRun;
        payload["currentRunNodePresent"] = observation.CurrentRunNodePresent;
        payload["currentRunRoomType"] = observation.CurrentRunRoomType;
        payload["rawCurrentRunRoomType"] = observation.CurrentRunRoomType;
        payload["currentRunRoomSceneType"] = observation.CurrentRunRoomSceneType;
        payload["rawCurrentRunRoomSceneType"] = observation.CurrentRunRoomSceneType;
    }

    private static Type? FindLoadedType(string fullName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var type = assembly.GetType(fullName, throwOnError: false, ignoreCase: false);
                if (type is not null)
                {
                    return type;
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private static bool IsProceedButtonType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NProceedButton", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<object> EnumerateMerchantSlots(object? inventory)
    {
        if (inventory is null)
        {
            yield break;
        }

        var seen = new HashSet<int>();
        foreach (var slot in ExpandEnumerable(TryInvokeMethod(inventory, "GetAllSlots")))
        {
            if (slot is not null && seen.Add(RuntimeHelpers.GetHashCode(slot)))
            {
                yield return slot;
            }
        }

        var cardRemoval = TryGetMemberValue(inventory, "_cardRemovalNode");
        if (cardRemoval is not null && seen.Add(RuntimeHelpers.GetHashCode(cardRemoval)))
        {
            yield return cardRemoval;
        }
    }

    private static ShopOptionCandidate? TryCreateShopOptionCandidate(object slot)
    {
        var typeName = slot.GetType().FullName ?? slot.GetType().Name;
        if (!typeName.Contains("NMerchant", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var screenBounds = TryResolveInteractiveScreenBounds(slot, out var boundsSource);
        if (string.IsNullOrWhiteSpace(screenBounds))
        {
            return null;
        }

        var entry = TryGetMemberValue(slot, "Entry");
        var entryTypeName = entry?.GetType().FullName ?? entry?.GetType().Name;
        var optionType = ResolveShopOptionType(typeName, entryTypeName);
        if (optionType is null)
        {
            return null;
        }

        var typedSource = TryResolveTypedShopOptionSource(slot, entry, optionType);
        var label = FirstNonEmpty(
            typedSource.Label,
            TryResolveChoiceLabel(slot),
            TryResolveChoiceLabel(entry),
            TryReadString(entry, "Title", "DisplayName", "Name", "Id"),
            TryReadString(slot, "Title", "DisplayName", "Name", "Id"));
        if (string.IsNullOrWhiteSpace(label)
            || IsPlaceholderChoiceLabel(label, slot))
        {
            return null;
        }

        var entryId = typedSource.EntryId ?? ResolveShopOptionId(slot, entry, optionType, label);
        var enabled = TryResolveInteractiveEnabled(slot) ?? TryResolveControlEnabled(slot) ?? true;
        var isStocked = TryReadBool(entry, "IsStocked") ?? true;
        var enoughGold = TryReadBool(entry, "EnoughGold") ?? false;
        var used = TryReadBool(entry, "Used") ?? false;
        return new ShopOptionCandidate(
            slot,
            optionType,
            label,
            entryId,
            screenBounds,
            enabled,
            isStocked,
            enoughGold,
            used,
            boundsSource);
    }

    private static string? ResolveShopOptionType(string? slotTypeName, string? entryTypeName)
    {
        if ((!string.IsNullOrWhiteSpace(slotTypeName) && slotTypeName.Contains("CardRemoval", StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(entryTypeName) && entryTypeName.Contains("CardRemoval", StringComparison.OrdinalIgnoreCase)))
        {
            return "card-removal";
        }

        if ((!string.IsNullOrWhiteSpace(slotTypeName) && slotTypeName.Contains("Relic", StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(entryTypeName) && entryTypeName.Contains("Relic", StringComparison.OrdinalIgnoreCase)))
        {
            return "relic";
        }

        if ((!string.IsNullOrWhiteSpace(slotTypeName) && slotTypeName.Contains("Potion", StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(entryTypeName) && entryTypeName.Contains("Potion", StringComparison.OrdinalIgnoreCase)))
        {
            return "potion";
        }

        if ((!string.IsNullOrWhiteSpace(slotTypeName) && slotTypeName.Contains("Card", StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(entryTypeName) && entryTypeName.Contains("Card", StringComparison.OrdinalIgnoreCase)))
        {
            return "card";
        }

        return null;
    }

    private static (string? Label, string? EntryId) TryResolveTypedShopOptionSource(object slot, object? entry, string optionType)
    {
        switch (optionType)
        {
            case "card":
            {
                var creationResult = TryGetMemberValue(entry, "CreationResult");
                var card = TryGetMemberValue(creationResult, "Card")
                           ?? TryGetMemberValue(slot, "Card")
                           ?? TryGetMemberValue(slot, "CardModel")
                           ?? TryGetMemberValue(TryGetMemberValue(slot, "_cardNode"), "Model")
                           ?? TryGetMemberValue(TryGetMemberValue(slot, "CardNode"), "Model")
                           ?? TryGetMemberValue(entry, "Card")
                           ?? TryGetMemberValue(entry, "CardModel");
                return (
                    SanitizeShopOptionLabel(FirstNonEmpty(
                        TryReadString(card, "Title", "CardName", "DisplayName", "Name", "Id", "CardId"),
                        TryReadString(creationResult, "Title", "DisplayName", "Name", "Id"))),
                    SanitizeShopOptionIdentity(FirstNonEmpty(
                        TryReadString(card, "Id", "CardId", "Name", "Title"),
                        TryReadString(creationResult, "Id", "CardId", "Name", "Title"))));
            }
            case "relic":
            {
                var relic = TryGetMemberValue(entry, "Model")
                            ?? TryGetMemberValue(slot, "Relic")
                            ?? TryGetMemberValue(slot, "Model")
                            ?? TryGetMemberValue(TryGetMemberValue(slot, "_relicNode"), "Model")
                            ?? TryGetMemberValue(TryGetMemberValue(slot, "RelicNode"), "Model");
                return (
                    SanitizeShopOptionLabel(FirstNonEmpty(
                        TryReadString(relic, "Title", "DisplayName", "Name", "Id"),
                        TryReadString(entry, "Title", "DisplayName", "Name", "Id"))),
                    SanitizeShopOptionIdentity(FirstNonEmpty(
                        TryReadString(relic, "Id", "Name", "Title"),
                        TryReadString(entry, "Id", "Name", "Title"))));
            }
            case "potion":
            {
                var potion = TryGetMemberValue(entry, "Model")
                             ?? TryGetMemberValue(slot, "Potion")
                             ?? TryGetMemberValue(slot, "Model")
                             ?? TryGetMemberValue(TryGetMemberValue(slot, "_potionNode"), "Model")
                             ?? TryGetMemberValue(TryGetMemberValue(slot, "PotionNode"), "Model");
                return (
                    SanitizeShopOptionLabel(FirstNonEmpty(
                        TryReadString(potion, "Title", "DisplayName", "Name", "Id"),
                        TryReadString(entry, "Title", "DisplayName", "Name", "Id"))),
                    SanitizeShopOptionIdentity(FirstNonEmpty(
                        TryReadString(potion, "Id", "Name", "Title"),
                        TryReadString(entry, "Id", "Name", "Title"))));
            }
            case "card-removal":
            {
                var label = SanitizeShopOptionLabel(FirstNonEmpty(
                    TryReadString(entry, "Title", "DisplayName", "Name", "Id"),
                    TryReadString(slot, "Title", "DisplayName", "Name", "Id")))
                    ?? "카드 제거 서비스";
                return (label, "service:card-removal");
            }
            default:
                return (null, null);
        }
    }

    private static string? SanitizeShopOptionLabel(string? label)
    {
        return string.IsNullOrWhiteSpace(label) || IsPlaceholderChoiceLabel(label)
            ? null
            : label;
    }

    private static string? SanitizeShopOptionIdentity(string? value)
    {
        return string.IsNullOrWhiteSpace(value) || IsPlaceholderChoiceLabel(value)
            ? null
            : value;
    }

    private static string ResolveShopOptionId(object slot, object? entry, string optionType, string label)
    {
        var creationResult = TryGetMemberValue(entry, "CreationResult");
        var cardLike = TryGetMemberValue(creationResult, "Card")
                       ?? TryGetMemberValue(slot, "Card")
                       ?? TryGetMemberValue(slot, "CardModel")
                       ?? TryGetMemberValue(slot, "Model")
                       ?? TryGetMemberValue(TryGetMemberValue(slot, "_cardNode"), "Model")
                       ?? TryGetMemberValue(TryGetMemberValue(slot, "CardNode"), "Model")
                       ?? TryGetMemberValue(entry, "Card")
                       ?? TryGetMemberValue(entry, "CardModel");
        var relicLike = TryGetMemberValue(entry, "Model")
                        ?? TryGetMemberValue(slot, "Relic")
                        ?? TryGetMemberValue(slot, "Model");
        var potionLike = TryGetMemberValue(entry, "Model")
                         ?? TryGetMemberValue(slot, "Potion")
                         ?? TryGetMemberValue(slot, "Model");
        return SanitizeShopOptionIdentity(FirstNonEmpty(
                   TryReadString(cardLike, "Id", "CardId", "Name", "Title"),
                   TryReadString(relicLike, "Id", "Name", "Title"),
                   TryReadString(potionLike, "Id", "Name", "Title"),
                   TryReadString(entry, "Id", "Name", "Title"),
                   TryReadString(slot, "Id", "Name", "Title")))
               ?? $"{optionType}:{SanitizeNodeKey(label)}";
    }

    private static bool? TryResolveControlEnabled(object? control)
    {
        if (control is null)
        {
            return null;
        }

        var enabled = TryReadBool(control, "IsEnabled", "Enabled", "_isEnabled");
        if (enabled is not null)
        {
            return enabled;
        }

        var disabled = TryReadBool(control, "Disabled", "IsDisabled", "_isDisabled");
        return disabled is null ? null : !disabled.Value;
    }

    private static RestSiteExtractorCandidate? TryCreateRestSiteButtonCandidate(object button)
    {
        var option = TryGetMemberValue(button, "Option") ?? TryGetMemberValue(button, "_option");
        if (option is null)
        {
            return null;
        }

        return TryCreateRestSiteCandidate(
            button,
            option,
            "button",
            TryResolveScreenBounds(button),
            score: !string.IsNullOrWhiteSpace(TryResolveScreenBounds(button)) ? 120 : 95,
            rejectReason: string.IsNullOrWhiteSpace(TryResolveScreenBounds(button))
                ? "missing-hitbox-for-explicit-choice"
                : null);
    }

    private static RestSiteExtractorCandidate? TryCreateRestSiteRawOptionCandidate(object option)
    {
        return TryCreateRestSiteCandidate(
            option,
            option,
            "raw-option",
            screenBounds: null,
            score: 70,
            rejectReason: null);
    }

    private static RestSiteExtractorCandidate? TryCreateRestSiteCandidate(
        object source,
        object option,
        string sourceKind,
        string? screenBounds,
        int score,
        string? rejectReason)
    {
        var optionId = TryResolveRestSiteOptionId(option, source);
        if (string.IsNullOrWhiteSpace(optionId))
        {
            return null;
        }

        var label = FirstNonEmpty(
            TryResolveChoiceLabel(source),
            TryResolveChoiceLabel(option),
            optionId);
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        var description = FirstNonEmpty(
            TryResolveChoiceDescription(source),
            TryResolveChoiceDescription(option));
        var enabled = TryReadBool(option, "IsEnabled");
        var iconAssetPath = TryResolveRestSiteIconAssetPath(option, source);
        var typeName = source.GetType().FullName ?? source.GetType().Name;
        var normalizedOptionId = optionId.Trim().ToUpperInvariant();
        var summary = new LiveExportChoiceSummary(
            "rest-option",
            label.Trim(),
            normalizedOptionId,
            description)
        {
            NodeId = $"rest-site:{normalizedOptionId}",
            ScreenBounds = screenBounds,
            BindingKind = "rest-site-option",
            BindingId = normalizedOptionId,
            Enabled = enabled,
            IconAssetPath = iconAssetPath,
            SemanticHints = BuildRestSiteSemanticHints(normalizedOptionId, sourceKind),
        };

        return new RestSiteExtractorCandidate(
            source,
            typeName,
            sourceKind,
            normalizedOptionId,
            summary.Label,
            summary.Description,
            summary.ScreenBounds,
            summary.Enabled,
            summary.IconAssetPath,
            score,
            rejectReason,
            summary);
    }

    private static string? TryResolveRestSiteOptionId(object option, object source)
    {
        var raw = FirstNonEmpty(
            TryReadString(option, "OptionId", "Id", "Value"),
            TryReadString(source, "OptionId", "Id", "Value"),
            TryReadString(TryGetMemberValue(option, "Title"), "Key", "Value"),
            TryReadString(TryGetMemberValue(source, "Title"), "Key", "Value"));
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var trimmed = raw.Trim();
        const string optionPrefix = "OPTION_";
        if (trimmed.StartsWith(optionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[optionPrefix.Length..];
        }

        return trimmed.ToUpperInvariant();
    }

    private static string? TryResolveRestSiteIconAssetPath(object option, object source)
    {
        foreach (var assetPath in ExpandEnumerable(TryGetMemberValue(option, "AssetPaths"))
                     .Select(TryConvertToDisplayString)
                     .Where(static value => !string.IsNullOrWhiteSpace(value))
                     .Cast<string>())
        {
            return assetPath;
        }

        return FirstNonEmpty(
            TryReadString(TryGetMemberValue(option, "Icon"), "ResourcePath", "Path"),
            TryReadString(TryGetMemberValue(source, "Icon"), "ResourcePath", "Path"),
            TryReadString(TryGetMemberValue(source, "_icon"), "ResourcePath", "Path"));
    }

    private static IReadOnlyList<string> BuildRestSiteSemanticHints(string optionId, string sourceKind)
    {
        return new[]
        {
            "scene:rest-site",
            $"option-id:{optionId}",
            $"source:{sourceKind}",
        };
    }

    private static IEnumerable<object> CollectEventChoiceItems(IEnumerable<object> choiceRoots)
    {
        return CollectChoiceItems(
            choiceRoots,
            "Event",
            "Options",
            "Buttons",
            "Choices",
            "CurrentOptions",
            "_options",
            "_eventOptions",
            "_optionButtons");
    }

    private static ChoiceExtractionResult ExtractAncientEventChoices(IEnumerable<object> roots, int maxEntries)
    {
        var layouts = FindAncientEventLayouts(roots)
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();
        if (layouts.Length == 0)
        {
            return new ChoiceExtractionResult(
                Array.Empty<LiveExportChoiceSummary>(),
                Array.Empty<LiveExportChoiceCandidate>(),
                new LiveExportChoiceDecision(
                    "event",
                    UsedStrictExtractor: true,
                    CandidateCount: 0,
                    AcceptedCount: 0,
                    "none",
                    null,
                    Array.Empty<string>()));
        }

        var optionButtons = layouts
            .SelectMany(FindAncientEventOptionButtons)
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();
        var defaultFocusedControl = layouts
            .Select(layout => TryGetMemberValue(layout, "DefaultFocusedControl"))
            .FirstOrDefault(static control => control is not null);
        var candidates = new List<LiveExportChoiceCandidate>();
        var choices = new List<LiveExportChoiceSummary>();
        string? ancientCompletionBoundsSource = null;
        string? ancientCompletionControlType = null;
        bool? ancientCompletionUsesDefaultFocus = null;
        bool? ancientCompletionHasFocus = null;
        var dialogueOnLastLine = layouts
            .Select(layout => TryReadBool(layout, "IsDialogueOnLastLine"))
            .Where(static value => value is not null)
            .Select(static value => value!.Value)
            .DefaultIfEmpty()
            .Last();
        var dialogueHitbox = layouts
            .Select(TryFindAncientDialogueHitbox)
            .FirstOrDefault(static node => node is not null);
        var dialogueVisible = dialogueHitbox is not null && IsVisibleNode(dialogueHitbox);
        var dialogueEnabled = dialogueHitbox is not null
                              && (TryResolveInteractiveEnabled(dialogueHitbox)
                                  ?? TryResolveControlEnabled(dialogueHitbox)
                                  ?? TryReadBool(dialogueHitbox, "IsEnabled", "Enabled")
                                  ?? true);
        var hasAuthoritativeDialogueState = layouts.Any(layout => TryReadBool(layout, "IsDialogueOnLastLine") is not null);
        var dialogueActive = hasAuthoritativeDialogueState
            ? !dialogueOnLastLine
            : dialogueVisible && dialogueEnabled;
        if (dialogueActive && dialogueHitbox is not null)
        {
            var authoritativeLayout = layouts.FirstOrDefault() ?? dialogueHitbox;
            var dialogueSummary = TryCreateAncientDialogueChoiceSummary(authoritativeLayout, dialogueHitbox, out var rejectReason);
            candidates.Add(new LiveExportChoiceCandidate(
                "event",
                dialogueHitbox.GetType().FullName ?? dialogueHitbox.GetType().Name,
                dialogueSummary?.Label ?? "Ancient dialogue",
                dialogueSummary?.Value,
                dialogueSummary?.Description,
                dialogueSummary is null ? 90 : 150,
                Accepted: dialogueSummary is not null,
                RejectReason: rejectReason));
            if (dialogueSummary is not null)
            {
                AddChoiceSummary(choices, dialogueSummary, maxEntries);
            }
        }
        else
        {
            foreach (var button in optionButtons)
            {
                var summary = TryCreateAncientEventOptionChoiceSummary(
                    button,
                    defaultFocusedControl,
                    out var rejectReason,
                    out var boundsSource,
                    out var usesDefaultFocus,
                    out var controlType,
                    out var hasFocus);
                candidates.Add(new LiveExportChoiceCandidate(
                    "event",
                    button.GetType().FullName ?? button.GetType().Name,
                    summary?.Label ?? TryResolveChoiceLabel(button) ?? TryReadString(TryGetMemberValue(button, "Option"), "Title", "Label", "Name"),
                    summary?.Value,
                    summary?.Description,
                    summary is null ? 80 : 140,
                    Accepted: summary is not null,
                    RejectReason: rejectReason));
                if (summary is not null)
                {
                    AddChoiceSummary(choices, summary, maxEntries);
                    if (IsAncientEventCompletionChoice(summary))
                    {
                        ancientCompletionBoundsSource ??= boundsSource;
                        ancientCompletionUsesDefaultFocus ??= usesDefaultFocus;
                        ancientCompletionControlType ??= controlType;
                        ancientCompletionHasFocus ??= hasFocus;
                    }
                }
            }
        }

        var enabledButtons = choices
            .Count(choice => string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase));
        var completionButtons = choices.Count(IsAncientEventCompletionChoice);

        var rejectSummary = BuildEventRejectSummary(candidates);
        var metadata = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["ancientEventDetected"] = "true",
            ["ancientDialogueActive"] = dialogueActive ? "true" : "false",
            ["ancientDialogueHitboxVisible"] = dialogueVisible ? "true" : "false",
            ["ancientDialogueHitboxEnabled"] = dialogueEnabled ? "true" : "false",
            ["ancientOptionCount"] = enabledButtons.ToString(CultureInfo.InvariantCulture),
            ["ancientCompletionActive"] = completionButtons > 0 ? "true" : "false",
            ["ancientCompletionCount"] = completionButtons.ToString(CultureInfo.InvariantCulture),
            ["ancientPhase"] = dialogueActive
                ? "dialogue"
                : completionButtons > 0
                    ? "completion"
                    : enabledButtons > 0
                        ? "options"
                        : "await-options",
            ["ancientEventExtractionPath"] = choices.Any(choice => string.Equals(choice.Kind, "event-dialogue", StringComparison.OrdinalIgnoreCase))
                ? "ancient-dialogue-hitbox"
                : completionButtons > 0
                    ? "ancient-completion-button"
                : enabledButtons > 0
                    ? "ancient-option-buttons"
                    : "ancient-await-options",
        };
        if (choices.Any(choice => string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)))
        {
            metadata["ancientOptionSummary"] = string.Join(
                ";",
                choices
                    .Where(choice => string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase))
                    .Take(6)
                    .Select(choice => $"{choice.NodeId ?? choice.Label}@{choice.ScreenBounds ?? "no-bounds"}"));
        }
        if (completionButtons > 0)
        {
            metadata["ancientCompletionSummary"] = string.Join(
                ";",
                choices
                    .Where(IsAncientEventCompletionChoice)
                    .Take(3)
                    .Select(choice => $"{choice.NodeId ?? choice.Label}@{choice.ScreenBounds ?? "no-bounds"}"));
            if (!string.IsNullOrWhiteSpace(ancientCompletionBoundsSource))
            {
                metadata["ancientCompletionBoundsSource"] = ancientCompletionBoundsSource;
            }

            if (!string.IsNullOrWhiteSpace(ancientCompletionControlType))
            {
                metadata["ancientCompletionControlType"] = ancientCompletionControlType;
            }

            if (ancientCompletionUsesDefaultFocus.HasValue)
            {
                metadata["ancientCompletionUsesDefaultFocus"] = ancientCompletionUsesDefaultFocus.Value ? "true" : "false";
            }

            if (ancientCompletionHasFocus.HasValue)
            {
                metadata["ancientCompletionHasFocus"] = ancientCompletionHasFocus.Value ? "true" : "false";
            }
        }

        if (!string.IsNullOrWhiteSpace(rejectSummary))
        {
            metadata["eventRejectSummary"] = rejectSummary;
        }

        return new ChoiceExtractionResult(
            choices,
            candidates,
            new LiveExportChoiceDecision(
                "event",
                UsedStrictExtractor: true,
                CandidateCount: candidates.Count,
                AcceptedCount: choices.Count,
                choices.Count == 0 ? "none" : "accepted",
                choices.Count == 0
                    ? rejectSummary ?? "ancient event detected but no explicit dialogue hitbox or enabled option button was actionable"
                    : rejectSummary,
                Array.Empty<string>()))
        {
            Meta = metadata,
        };
    }

    private static IEnumerable<object> FindAncientEventLayouts(IEnumerable<object> roots)
    {
        foreach (var root in roots)
        {
            foreach (var item in EnumerateDescendantsAndSelf(root, maxDepth: 6))
            {
                if (IsAncientEventLayoutType(item.GetType()))
                {
                    yield return item;
                }
            }
        }
    }

    private static IEnumerable<object> FindAncientEventOptionButtons(object layout)
    {
        return EnumerateDescendantsAndSelf(layout, maxDepth: 6)
            .Where(static item => IsAncientEventOptionButtonType(item.GetType()));
    }

    private static bool IsEnabledAncientEventOptionButton(object button)
    {
        var option = TryGetMemberValue(button, "Option");
        var locked = TryReadBool(option, "IsLocked") == true;
        if (locked)
        {
            return false;
        }

        var enabled = TryResolveInteractiveEnabled(button)
                      ?? TryResolveControlEnabled(button)
                      ?? TryReadBool(button, "IsEnabled", "Enabled")
                      ?? true;
        return enabled;
    }

    private static LiveExportChoiceSummary? TryCreateAncientEventOptionChoiceSummary(
        object button,
        object? defaultFocusedControl,
        out string? rejectReason,
        out string? boundsSource,
        out bool usesDefaultFocus,
        out string? controlType,
        out bool? hasFocus)
    {
        rejectReason = null;
        boundsSource = null;
        usesDefaultFocus = false;
        controlType = null;
        hasFocus = null;
        if (!IsAncientEventOptionButtonType(button.GetType()))
        {
            rejectReason = "not-ancient-option-button";
            return null;
        }

        if (!IsVisibleNode(button))
        {
            rejectReason = "hidden";
            return null;
        }

        var enabled = TryResolveInteractiveEnabled(button)
                      ?? TryResolveControlEnabled(button)
                      ?? TryReadBool(button, "IsEnabled", "Enabled")
                      ?? true;
        if (!enabled)
        {
            rejectReason = "disabled";
            return null;
        }

        var option = TryGetMemberValue(button, "Option");
        if (TryReadBool(option, "IsLocked") == true)
        {
            rejectReason = "locked";
            return null;
        }

        var interactiveControl = TryResolveInteractiveControl(button, out var interactiveBoundsSource) ?? button;
        boundsSource = interactiveBoundsSource;
        controlType = interactiveControl.GetType().FullName ?? interactiveControl.GetType().Name;
        usesDefaultFocus = defaultFocusedControl is not null
                           && (ReferenceEquals(interactiveControl, defaultFocusedControl)
                               || ReferenceEquals(button, defaultFocusedControl));
        hasFocus = TryResolveControlHasFocus(interactiveControl)
                   ?? TryResolveControlHasFocus(button);

        var screenBounds = TryResolveInteractiveScreenBounds(button, out interactiveBoundsSource);
        boundsSource = interactiveBoundsSource;
        if (string.IsNullOrWhiteSpace(screenBounds))
        {
            rejectReason = "missing-interactive-bounds";
            return null;
        }
        if (!HasUsableAncientEventBounds(screenBounds))
        {
            rejectReason = "offscreen-interactive-bounds";
            return null;
        }

        var label = FirstNonEmpty(
            TryReadString(option, "Title", "Label", "Name"),
            TryResolveChoiceLabel(button));
        if (string.IsNullOrWhiteSpace(label))
        {
            rejectReason = "missing-label";
            return null;
        }

        var index = TryReadInt(button, "Index");
        var value = FirstNonEmpty(
            TryReadString(option, "Id", "Name"),
            index?.ToString(CultureInfo.InvariantCulture));
        var description = FirstNonEmpty(
            TryReadString(option, "Description", "Body"),
            TryResolveChoiceDescription(button));
        var isProceed = TryReadBool(option, "IsProceed") == true;
        var semanticHints = new List<string>
        {
            "scene:event",
            "ancient-event",
            "source:ancient-option-button",
            isProceed ? "option-role:proceed" : "option-role:choice",
        };
        if (index is not null)
        {
            semanticHints.Add($"option-index:{index.Value}");
        }

        if (isProceed)
        {
            semanticHints.Add("ancient-event-completion");
        }

        return new LiveExportChoiceSummary(
            "event-option",
            label.Trim(),
            value,
            description)
        {
            NodeId = index is not null
                ? $"ancient-event-option:{index.Value}"
                : $"ancient-event-option:{SanitizeNodeKey(label)}",
            ScreenBounds = screenBounds,
            Enabled = true,
            SemanticHints = semanticHints,
        };
    }

    private static bool? TryResolveControlHasFocus(object? item)
    {
        if (item is null)
        {
            return null;
        }

        return TryConvertToBool(TryInvokeMethod(item, "HasFocus"))
               ?? TryReadBool(item, "IsFocused");
    }

    private static LiveExportChoiceSummary? TryCreateAncientDialogueChoiceSummary(object layout, object dialogueHitbox, out string? rejectReason)
    {
        rejectReason = null;
        if (!IsVisibleNode(dialogueHitbox))
        {
            rejectReason = "dialogue-hitbox-hidden";
            return null;
        }

        var enabled = TryResolveInteractiveEnabled(dialogueHitbox)
                      ?? TryResolveControlEnabled(dialogueHitbox)
                      ?? TryReadBool(dialogueHitbox, "IsEnabled", "Enabled")
                      ?? true;
        if (!enabled)
        {
            rejectReason = "dialogue-hitbox-disabled";
            return null;
        }

        var screenBounds = TryResolveAncientDialogueScreenBounds(layout, dialogueHitbox, out _);
        if (string.IsNullOrWhiteSpace(screenBounds))
        {
            rejectReason = "dialogue-hitbox-missing-bounds";
            return null;
        }
        if (!HasUsableAncientEventBounds(screenBounds))
        {
            rejectReason = "dialogue-hitbox-offscreen-bounds";
            return null;
        }

        return new LiveExportChoiceSummary(
            "event-dialogue",
            "Ancient dialogue",
            "dialogue-hitbox",
            "Advance the ancient event dialogue using the explicit %DialogueHitbox.")
        {
            NodeId = "ancient-dialogue:advance",
            ScreenBounds = screenBounds,
            Enabled = true,
            SemanticHints = new[] { "scene:event", "ancient-event", "source:ancient-dialogue-hitbox" },
        };
    }

    private static object? TryFindAncientDialogueHitbox(object layout)
    {
        var explicitNode = TryInvokeMethod(layout, "GetNodeOrNull", "%DialogueHitbox");
        if (explicitNode is not null)
        {
            return explicitNode;
        }

        return EnumerateDescendantsAndSelf(layout, maxDepth: 6)
            .FirstOrDefault(static item => HasNamedDialogueHitbox(item));
    }

    private static string? TryResolveAncientDialogueScreenBounds(object layout, object dialogueHitbox, out string boundsSource)
    {
        var hitboxBounds = TryResolveInteractiveScreenBounds(dialogueHitbox, out boundsSource);
        if (HasUsableAncientEventBounds(hitboxBounds))
        {
            return hitboxBounds;
        }

        var fakeNextButton = TryReadValue(layout, "_fakeNextButton", "FakeNextButton")
                             ?? TryInvokeMethod(layout, "GetNodeOrNull", "%FakeNextButton")
                             ?? TryInvokeMethod(layout, "GetNodeOrNull", "FakeNextButton");
        var fakeNextButtonBounds = fakeNextButton is null
            ? null
            : TryResolveScreenBounds(fakeNextButton);
        if (HasUsableAncientEventBounds(fakeNextButtonBounds))
        {
            boundsSource = "fake-next-button";
            return fakeNextButtonBounds;
        }

        var fakeNextButtonContainer = TryReadValue(layout, "_fakeNextButtonContainer", "FakeNextButtonContainer")
                                      ?? TryInvokeMethod(layout, "GetNodeOrNull", "%FakeNextButtonContainer")
                                      ?? TryInvokeMethod(layout, "GetNodeOrNull", "FakeNextButtonContainer");
        var fakeNextButtonContainerBounds = fakeNextButtonContainer is null
            ? null
            : TryResolveScreenBounds(fakeNextButtonContainer);
        if (HasUsableAncientEventBounds(fakeNextButtonContainerBounds))
        {
            boundsSource = "fake-next-button-container";
            return fakeNextButtonContainerBounds;
        }

        return hitboxBounds;
    }

    private static bool HasUsableAncientEventBounds(string? screenBounds)
    {
        if (string.IsNullOrWhiteSpace(screenBounds))
        {
            return false;
        }

        var parts = screenBounds.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4
            || !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            || !double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
        {
            return false;
        }

        if (width <= 0d || height <= 0d)
        {
            return false;
        }

        var centerX = x + width / 2d;
        var centerY = y + height / 2d;
        return centerX >= 0d
               && centerY >= 0d
               && centerX <= 1920d
               && centerY <= 1080d;
    }

    private static bool IsAncientEventCompletionChoice(LiveExportChoiceSummary choice)
    {
        return choice.SemanticHints.Any(static hint =>
            string.Equals(hint, "ancient-event-completion", StringComparison.OrdinalIgnoreCase)
            || string.Equals(hint, "option-role:proceed", StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<object> EnumerateDescendantsAndSelf(object root, int maxDepth)
    {
        if (root is null || maxDepth < 0)
        {
            yield break;
        }

        var seen = new HashSet<int>();
        var queue = new Queue<(object Item, int Depth)>();
        queue.Enqueue((root, 0));
        while (queue.Count > 0)
        {
            var (item, depth) = queue.Dequeue();
            if (!seen.Add(RuntimeHelpers.GetHashCode(item)))
            {
                continue;
            }

            yield return item;
            if (depth >= maxDepth)
            {
                continue;
            }

            foreach (var child in TryEnumerateChildren(item).Take(64))
            {
                queue.Enqueue((child, depth + 1));
            }
        }
    }

    private static bool IsAncientEventDetected(ChoiceExtractionResult result)
    {
        return result.Meta.TryGetValue("ancientEventDetected", out var detected)
               && string.Equals(detected, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAncientEventLayoutType(Type type)
    {
        var typeName = type.FullName ?? type.Name;
        return typeName.Contains("NAncientEventLayout", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAncientEventOptionButtonType(Type type)
    {
        var typeName = type.FullName ?? type.Name;
        return typeName.Contains("NEventOptionButton", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasNamedDialogueHitbox(object item)
    {
        var name = TryReadString(item, "Name")
                   ?? TryConvertToDisplayString(TryGetMemberValue(item, "Name"));
        return !string.IsNullOrWhiteSpace(name)
               && (string.Equals(name, "%DialogueHitbox", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(name, "DialogueHitbox", StringComparison.OrdinalIgnoreCase));
    }

    private static string? BuildEventRejectSummary(IReadOnlyList<LiveExportChoiceCandidate> candidates)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        if (candidates.Any(static candidate => candidate.Accepted))
        {
            return null;
        }

        var rejectCounts = candidates
            .Where(static candidate => !candidate.Accepted && !string.IsNullOrWhiteSpace(candidate.RejectReason))
            .GroupBy(candidate => candidate.RejectReason!, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(static group => group.Count())
            .ThenBy(static group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .Select(group => $"{group.Key}={group.Count()}")
            .ToArray();
        if (rejectCounts.Length == 0)
        {
            return $"event candidates seen={candidates.Count} but none were accepted";
        }

        return $"event candidates seen={candidates.Count} but filtered: {string.Join(",", rejectCounts)}";
    }

    private static IEnumerable<object> CollectShopChoiceItems(IEnumerable<object> choiceRoots)
    {
        return CollectChoiceItems(
            choiceRoots,
            "Merchant",
            "MerchantSlots",
            "CharacterCardEntries",
            "ColorlessCardEntries",
            "RelicEntries",
            "PotionEntries",
            "CardRemovalEntry",
            "CardRemovalServiceEntry",
            "RemovalEntry",
            "ServiceEntries",
            "ShopEntries");
    }

    private static IEnumerable<object> CollectCombatChoiceItems(IEnumerable<object> choiceRoots)
    {
        return CollectChoiceItems(
            choiceRoots,
            "Combat",
            "Hand",
            "Cards",
            "CardNodes",
            "DrawPile",
            "DiscardPile",
            "ExhaustPile",
            "EndTurnButton",
            "PingButton");
    }

    private static ChoiceExtractionResult ExtractCombatChoices(IEnumerable<object> roots, int maxEntries)
    {
        var choices = CollectCombatEnemyTargetChoices(roots)
            .Take(maxEntries)
            .ToArray();
        var candidates = choices
            .Select(choice => new LiveExportChoiceCandidate(
                "combat-targets",
                "NCreature",
                choice.Label,
                choice.Value,
                choice.Description,
                100,
                Accepted: true,
                RejectReason: null))
            .ToArray();
        return new ChoiceExtractionResult(
            choices,
            candidates,
            new LiveExportChoiceDecision(
                "combat-targets",
                UsedStrictExtractor: true,
                CandidateCount: candidates.Length,
                AcceptedCount: choices.Length,
                choices.Length == 0 ? "none" : "accepted",
                choices.Length == 0 ? "no interactable enemy hitboxes resolved for combat targeting" : null,
                Array.Empty<string>()));
    }

    private static IReadOnlyList<LiveExportChoiceSummary> CollectCombatEnemyTargetChoices(IEnumerable<object> roots)
    {
        return CollectCombatEnemyTargetEvaluations(roots)
            .Where(static evaluation => evaluation.HittableForExport)
            .Select(static evaluation => evaluation.Choice)
            .Where(static choice => choice is not null)
            .Cast<LiveExportChoiceSummary>()
            .ToArray();
    }

    private static IReadOnlyList<CombatEnemyTargetEvaluation> CollectCombatEnemyTargetEvaluations(IEnumerable<object> roots)
    {
        var rootArray = roots
            .Where(static root => root is not null)
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();
        var targetManagerRoot = rootArray.FirstOrDefault(root =>
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            return typeName.Contains("NTargetManager", StringComparison.OrdinalIgnoreCase)
                   || TryReadBool(root, "IsInSelection") is not null
                   || TryGetMemberValue(root, "HoveredNode") is not null
                   || TryGetMemberValue(root, "_validTargetsType") is not null;
        });
        var targetingInProgress = TryReadBool(targetManagerRoot, "IsInSelection");
        var validTargetsType = TryConvertToDisplayString(
            TryGetMemberValue(targetManagerRoot!, "_validTargetsType")
            ?? TryGetMemberValue(targetManagerRoot!, "ValidTargetsType"));

        if (targetManagerRoot is not null && targetingInProgress != true)
        {
            return Array.Empty<CombatEnemyTargetEvaluation>();
        }

        var creatures = new List<object>();
        foreach (var root in rootArray)
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            if (typeName.Contains("NCreature", StringComparison.OrdinalIgnoreCase))
            {
                AddIfUseful(creatures, root);
            }

            if (typeName.Contains("NCombatRoom", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var creature in ExpandEnumerable(TryGetMemberValue(root, "CreatureNodes")))
                {
                    AddIfUseful(creatures, creature);
                }
            }

            foreach (var creature in ExpandEnumerable(TryGetMemberValue(root, "CreatureNodes")))
            {
                AddIfUseful(creatures, creature);
            }
        }

        return creatures
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .Select((creature, index) => EvaluateCombatEnemyTarget(creature, index + 1, targetManagerRoot, validTargetsType))
            .Where(static evaluation => evaluation is not null)
            .Cast<CombatEnemyTargetEvaluation>()
            .OrderBy(static evaluation => TryGetBoundsSortX(evaluation.Choice?.ScreenBounds))
            .ThenBy(static evaluation => TryGetBoundsSortY(evaluation.Choice?.ScreenBounds))
            .ToArray();
    }

    private static CombatEnemyTargetEvaluation? EvaluateCombatEnemyTarget(
        object creature,
        int ordinal,
        object? targetManagerRoot,
        string? validTargetsType)
    {
        var typeName = creature.GetType().FullName ?? creature.GetType().Name;
        if (!typeName.Contains("NCreature", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var visible = TryReadBool(creature, "Visible", "IsVisible", "VisibleInTree");
        var interactable = TryReadBool(creature, "IsInteractable");
        if (visible == false || interactable == false)
        {
            return null;
        }

        var entity = TryGetMemberValue(creature, "Entity");
        if (TryReadBool(entity, "IsPlayer") == true
            || TryReadBool(entity, "IsPet") == true
            || TryReadBool(entity, "IsFriendly") == true)
        {
            return null;
        }

        var allowedByTargetManager = TryConvertToBool(TryInvokeMethod(targetManagerRoot!, "AllowedToTargetNode", creature))
                                     ?? TryConvertToBool(TryInvokeMethod(targetManagerRoot!, "AllowedToTargetCreature", entity));
        var allowedByTargetingHook = TryEvaluateHookTargetability(targetManagerRoot, entity, out var targetingPreventerType);
        var isHittable = TryReadBool(entity, "IsHittable");
        var allowedByHittingHook = TryEvaluateHookHittability(targetManagerRoot, entity);
        var hitboxInputEnabled = TryIsCombatEnemyHitboxInputEnabled(creature);

        if (targetManagerRoot is not null && allowedByTargetManager != true)
        {
            return null;
        }

        if (allowedByTargetingHook == false)
        {
            return null;
        }

        var label = ResolveCombatEnemyTargetLabel(creature, entity, ordinal);
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        var targetId = TryExtractCombatTargetId(entity) ?? TryExtractCombatTargetId(creature);

        if (!TryResolveCombatEnemyTargetBounds(creature, out var screenBounds, out var targetSource)
            || string.IsNullOrWhiteSpace(screenBounds))
        {
            return null;
        }
        var targetableForDiagnostics = allowedByTargetManager == true && allowedByTargetingHook != false;
        var hittableForExport = targetableForDiagnostics
                                && isHittable != false
                                && allowedByHittingHook != false
                                && hitboxInputEnabled != false;
        var choice = new LiveExportChoiceSummary(
            "enemy-target",
            label,
            targetId,
            $"target-source:{targetSource}|coord-space:logical-render|click-space:current-window-normalized|normalized:{FormatNormalizedBounds(screenBounds)}")
        {
            NodeId = string.IsNullOrWhiteSpace(targetId)
                ? $"enemy-target:{ordinal}"
                : $"enemy-target:{SanitizeNodeKey(targetId)}:{ordinal}",
            ScreenBounds = screenBounds,
            BindingKind = "combat-target",
            BindingId = targetId,
            Enabled = hittableForExport,
            SemanticHints = BuildCombatEnemyTargetSemanticHints(
                targetId,
                validTargetsType,
                targetingPreventerType,
                isHittable,
                allowedByHittingHook,
                hitboxInputEnabled,
                hittableForExport),
        };

        return new CombatEnemyTargetEvaluation(choice, targetableForDiagnostics, hittableForExport);
    }

    private static IReadOnlyList<string> BuildCombatEnemyTargetSemanticHints(
        string? targetId,
        string? validTargetsType,
        string? preventerType,
        bool? isHittable,
        bool? allowedByHittingHook,
        bool? hitboxInputEnabled,
        bool hittableForExport)
    {
        var hints = new List<string>
        {
            "combat-targetable",
            "source:target-manager",
        };
        if (hittableForExport)
        {
            hints.Add("combat-hittable");
        }
        if (!string.IsNullOrWhiteSpace(validTargetsType))
        {
            hints.Add($"valid-target-type:{validTargetsType}");
        }

        if (!string.IsNullOrWhiteSpace(targetId))
        {
            hints.Add($"target-id:{targetId}");
        }

        if (!string.IsNullOrWhiteSpace(preventerType))
        {
            hints.Add($"targeting-preventer:{preventerType}");
        }

        if (isHittable is not null)
        {
            hints.Add($"is-hittable:{isHittable.Value.ToString().ToLowerInvariant()}");
        }

        if (allowedByHittingHook is not null)
        {
            hints.Add($"hook-hitting:{allowedByHittingHook.Value.ToString().ToLowerInvariant()}");
        }

        if (hitboxInputEnabled is not null)
        {
            hints.Add($"hitbox-input-enabled:{hitboxInputEnabled.Value.ToString().ToLowerInvariant()}");
        }

        return hints;
    }

    private static bool? TryEvaluateHookTargetability(object? targetManagerRoot, object? entity, out string? preventerType)
    {
        preventerType = null;
        if (targetManagerRoot is null || entity is null)
        {
            return null;
        }

        try
        {
            var combatState = TryGetMemberValue(entity, "CombatState");
            if (combatState is null)
            {
                return null;
            }

            var hookType = targetManagerRoot.GetType().Assembly.GetType("MegaCrit.Sts2.Core.Hooks.Hook");
            var method = hookType?.GetMethod(
                "ShouldAllowTargeting",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method is null)
            {
                return null;
            }

            var args = new object?[] { combatState, entity, null };
            var result = method.Invoke(null, args);
            preventerType = args[2]?.GetType().FullName ?? args[2]?.GetType().Name;
            return TryConvertToBool(result);
        }
        catch
        {
            return null;
        }
    }

    private static bool? TryEvaluateHookHittability(object? targetManagerRoot, object? entity)
    {
        if (targetManagerRoot is null || entity is null)
        {
            return null;
        }

        try
        {
            var combatState = TryGetMemberValue(entity, "CombatState");
            if (combatState is null)
            {
                return null;
            }

            var hookType = targetManagerRoot.GetType().Assembly.GetType("MegaCrit.Sts2.Core.Hooks.Hook");
            var method = hookType?.GetMethod(
                "ShouldAllowHitting",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method is null)
            {
                return null;
            }

            var result = method.Invoke(null, new[] { combatState, entity });
            return TryConvertToBool(result);
        }
        catch
        {
            return null;
        }
    }

    private static bool? TryIsCombatEnemyHitboxInputEnabled(object creature)
    {
        var hitbox = TryGetMemberValue(creature, "Hitbox");
        if (hitbox is null)
        {
            return null;
        }

        var mouseFilter = TryGetMemberValue(hitbox, "MouseFilter");
        var mouseFilterName = TryConvertToDisplayString(mouseFilter);
        if (string.Equals(mouseFilterName, "Ignore", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var mouseFilterValue = TryConvertToInt(mouseFilter);
        if (mouseFilterValue == 2)
        {
            return false;
        }

        return mouseFilter is null ? null : true;
    }

    private sealed record CombatEnemyTargetEvaluation(
        LiveExportChoiceSummary Choice,
        bool TargetableForDiagnostics,
        bool HittableForExport);

    private static string ResolveCombatEnemyTargetLabel(object creature, object? entity, int ordinal)
    {
        var rawName = TryReadString(entity, "DisplayName", "Name", "Id", "MonsterName")
                      ?? TryReadString(creature, "DisplayName", "Name");
        if (string.IsNullOrWhiteSpace(rawName))
        {
            return $"Enemy {ordinal}";
        }

        return rawName.Trim();
    }

    private static bool TryResolveCombatEnemyTargetBounds(object creature, out string? screenBounds, out string targetSource)
    {
        screenBounds = null;
        targetSource = "none";

        var hitbox = TryGetMemberValue(creature, "Hitbox") ?? creature;
        var hitboxBounds = TryResolveScreenBounds(hitbox) ?? TryResolveScreenBounds(creature);
        if (string.IsNullOrWhiteSpace(hitboxBounds))
        {
            return false;
        }

        if (!TryParseBounds(hitboxBounds, out var hitboxX, out var hitboxY, out var hitboxWidth, out var hitboxHeight))
        {
            return false;
        }

        var vfxSpawnPosition = TryGetMemberValue(creature, "VfxSpawnPosition");
        var vfxX = TryReadDouble(vfxSpawnPosition, "X", "x");
        var vfxY = TryReadDouble(vfxSpawnPosition, "Y", "y");
        if (vfxX is not null && vfxY is not null)
        {
            var width = Clamp(hitboxWidth * 0.36d, 72d, Math.Max(72d, hitboxWidth * 0.78d));
            var height = Clamp(hitboxHeight * 0.42d, 88d, Math.Max(88d, hitboxHeight * 0.82d));
            var targetX = Clamp(vfxX.Value - width / 2d, hitboxX, hitboxX + hitboxWidth - width);
            var targetY = Clamp(vfxY.Value - height * 0.55d, hitboxY, hitboxY + hitboxHeight - height);
            screenBounds = FormatBounds(targetX, targetY, width, height);
            targetSource = "vfx-spawn-hitbox";
            return true;
        }

        var bodyX = hitboxX + hitboxWidth * 0.22d;
        var bodyY = hitboxY + hitboxHeight * 0.18d;
        var bodyWidth = hitboxWidth * 0.56d;
        var bodyHeight = hitboxHeight * 0.62d;
        screenBounds = FormatBounds(bodyX, bodyY, bodyWidth, bodyHeight);
        targetSource = "hitbox-body";
        return true;
    }

    private static bool TryParseBounds(string rawBounds, out double x, out double y, out double width, out double height)
    {
        x = default;
        y = default;
        width = default;
        height = default;

        var parts = rawBounds.Split(',', StringSplitOptions.TrimEntries);
        return parts.Length == 4
               && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x)
               && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y)
               && double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out width)
               && double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out height)
               && width > 0d
               && height > 0d;
    }

    private static string FormatBounds(double x, double y, double width, double height)
    {
        return string.Create(CultureInfo.InvariantCulture, $"{x:0.###},{y:0.###},{width:0.###},{height:0.###}");
    }

    private static string FormatNormalizedBounds(string? rawBounds)
    {
        if (!TryParseBounds(rawBounds ?? string.Empty, out var x, out var y, out var width, out var height))
        {
            return "unknown";
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{Clamp01(x / 1920d):0.####},{Clamp01(y / 1080d):0.####},{Clamp01(width / 1920d):0.####},{Clamp01(height / 1080d):0.####}");
    }

    private static double Clamp01(double value)
    {
        if (value < 0d)
        {
            return 0d;
        }

        if (value > 1d)
        {
            return 1d;
        }

        return value;
    }

    private static double TryGetBoundsSortX(string? rawBounds)
    {
        return !string.IsNullOrWhiteSpace(rawBounds) && TryParseBounds(rawBounds, out var x, out _, out _, out _)
            ? x
            : double.MaxValue;
    }

    private static double TryGetBoundsSortY(string? rawBounds)
    {
        return !string.IsNullOrWhiteSpace(rawBounds) && TryParseBounds(rawBounds, out _, out var y, out _, out _)
            ? y
            : double.MaxValue;
    }

    private static double Clamp(double value, double min, double max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    private static IEnumerable<object> CollectChoiceItems(
        IEnumerable<object> choiceRoots,
        string typeHint,
        params string[] memberNames)
    {
        var results = new List<object>();
        foreach (var root in choiceRoots)
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            if (!typeName.Contains(typeHint, StringComparison.OrdinalIgnoreCase)
                && memberNames.All(memberName => TryGetMemberValue(root, memberName) is null))
            {
                continue;
            }

            if (LooksLikeStrictChoiceRoot(root))
            {
                AddChoiceCandidate(results, root, maxDepth: 2);
            }

            foreach (var memberName in memberNames)
            {
                var value = TryGetMemberValue(root, memberName);
                if (LooksLikeStrictChoiceRoot(value))
                {
                    AddChoiceCandidate(results, value, maxDepth: 3);
                }

                foreach (var item in ExpandEnumerable(value))
                {
                    if (LooksLikeStrictChoiceRoot(item))
                    {
                        AddChoiceCandidate(results, item, maxDepth: 2);
                    }
                }
            }
        }

        return results;
    }

    private static bool LooksLikeRewardContext(string triggerKind, string? screenHint, IEnumerable<object> roots)
    {
        return triggerKind.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || MatchesScreenHint(screenHint, "reward", "rewards")
               || roots.Any(root => (root.GetType().FullName ?? root.GetType().Name).Contains("Reward", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeRestContext(string triggerKind, string? screenHint, IEnumerable<object> roots)
    {
        return triggerKind.Contains("rest", StringComparison.OrdinalIgnoreCase)
               || MatchesScreenHint(screenHint, "rest", "rest-site", "campfire")
               || roots.Any(root =>
               {
                   var typeName = root.GetType().FullName ?? root.GetType().Name;
                   return typeName.Contains("Rest", StringComparison.OrdinalIgnoreCase)
                          || typeName.Contains("Campfire", StringComparison.OrdinalIgnoreCase);
               });
    }

    private static bool LooksLikeRestSiteUpgradeContext(string triggerKind, string? screenHint, IEnumerable<object> roots)
    {
        if (!LooksLikeRestContext(triggerKind, screenHint, roots))
        {
            return false;
        }

        return roots.Any(root =>
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            return typeName.Contains("NDeckUpgradeSelectScreen", StringComparison.OrdinalIgnoreCase);
        });
    }

    private static bool LooksLikeEventContext(string triggerKind, string? screenHint, IEnumerable<object> roots)
    {
        return triggerKind.Contains("event", StringComparison.OrdinalIgnoreCase)
               || MatchesScreenHint(screenHint, "event")
               || roots.Any(root => (root.GetType().FullName ?? root.GetType().Name).Contains("Event", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeShopContext(string triggerKind, string? screenHint, IEnumerable<object> roots)
    {
        return triggerKind.Contains("shop", StringComparison.OrdinalIgnoreCase)
               || triggerKind.Contains("merchant", StringComparison.OrdinalIgnoreCase)
               || MatchesScreenHint(screenHint, "shop")
               || roots.Any(root =>
               {
                   var typeName = root.GetType().FullName ?? root.GetType().Name;
                   return typeName.Contains("Shop", StringComparison.OrdinalIgnoreCase)
                          || typeName.Contains("Merchant", StringComparison.OrdinalIgnoreCase);
               });
    }

    private static bool LooksLikeCombatContext(string triggerKind, string? screenHint, IEnumerable<object> roots)
    {
        return triggerKind.Contains("combat", StringComparison.OrdinalIgnoreCase)
               || MatchesScreenHint(screenHint, "combat")
               || roots.Any(root =>
               {
                   var typeName = root.GetType().FullName ?? root.GetType().Name;
                   return typeName.Contains("Combat", StringComparison.OrdinalIgnoreCase)
                          || typeName.Contains("PlayerCombatState", StringComparison.OrdinalIgnoreCase);
               });
    }

    private static bool MatchesScreenHint(string? screenHint, params string[] values)
    {
        return !string.IsNullOrWhiteSpace(screenHint)
               && values.Any(value => string.Equals(screenHint, value, StringComparison.OrdinalIgnoreCase));
    }

    private static string? JoinFailureReasons(IEnumerable<string?> reasons, string? trailingReason)
    {
        var parts = reasons
            .Append(trailingReason)
            .Where(reason => !string.IsNullOrWhiteSpace(reason))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return parts.Length == 0 ? null : string.Join(" | ", parts);
    }

    private static object? TryResolveMainLoop()
    {
        var engineType = RuntimeTypeLocator.TryFindType("Godot.Engine");
        if (engineType is null)
        {
            return null;
        }

        return TryInvokeMethod(engineType, "GetMainLoop")
               ?? TryGetMemberValue(engineType, "MainLoop");
    }

    private static IEnumerable<object> EnumerateSceneNodes(object? root, int maxNodes)
    {
        if (root is null)
        {
            yield break;
        }

        var queue = new Queue<object>();
        var seen = new HashSet<int>();
        queue.Enqueue(root);
        seen.Add(RuntimeHelpers.GetHashCode(root));

        while (queue.Count > 0 && seen.Count <= maxNodes)
        {
            var current = queue.Dequeue();
            yield return current;

            foreach (var child in TryEnumerateChildren(current))
            {
                var key = RuntimeHelpers.GetHashCode(child);
                if (!seen.Add(key))
                {
                    continue;
                }

                queue.Enqueue(child);
                if (seen.Count >= maxNodes)
                {
                    break;
                }
            }
        }
    }

    private static IEnumerable<object> TryEnumerateChildren(object root)
    {
        var seen = new HashSet<int>();
        foreach (var candidate in new[]
                 {
                     TryInvokeMethod(root, "GetChildren", true),
                     TryInvokeMethod(root, "GetChildren"),
                 })
        {
            foreach (var item in ExpandEnumerable(candidate))
            {
                var key = RuntimeHelpers.GetHashCode(item);
                if (seen.Add(key))
                {
                    yield return item;
                }
            }
        }

        var childCount = TryConvertToInt(TryInvokeMethod(root, "GetChildCount"));
        if (childCount is null or <= 0)
        {
            yield break;
        }

        for (var index = 0; index < childCount.Value; index += 1)
        {
            var child = TryInvokeMethod(root, "GetChild", index);
            if (child is not null && seen.Add(RuntimeHelpers.GetHashCode(child)))
            {
                yield return child;
            }
        }
    }

    private static IEnumerable<object> EnumerateDescendants(object root, int maxNodes)
    {
        var queue = new Queue<object>();
        var seen = new HashSet<int>();
        queue.Enqueue(root);
        seen.Add(RuntimeHelpers.GetHashCode(root));

        while (queue.Count > 0 && seen.Count <= maxNodes)
        {
            var current = queue.Dequeue();
            yield return current;

            foreach (var child in TryEnumerateChildren(current))
            {
                if (seen.Add(RuntimeHelpers.GetHashCode(child)))
                {
                    queue.Enqueue(child);
                }
            }
        }
    }

    private static bool IsInterestingSceneNode(object node)
    {
        var typeName = node.GetType().FullName ?? node.GetType().Name;
        return IsVisibleNode(node)
               || SceneNodeKeywords.Any(keyword => typeName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsVisibleNode(object node)
    {
        return TryReadBool(node, "Visible", "IsVisible", "VisibleInTree")
               ?? TryConvertToBool(TryInvokeMethod(node, "IsVisibleInTree"))
               ?? false;
    }

    private static IReadOnlyList<string> BuildWarnings(
        LiveExportPlayerSummary player,
        IReadOnlyList<LiveExportCardSummary> deck,
        IReadOnlyList<string> relics,
        IReadOnlyList<string> potions,
        IReadOnlyList<LiveExportChoiceSummary> choices)
    {
        var warnings = new List<string>();
        if (player.CurrentHp is null && deck.Count == 0 && relics.Count == 0 && potions.Count == 0)
        {
            warnings.Add("state extraction is partial; core run data was not resolved from live objects.");
        }

        if (choices.Count == 0)
        {
            warnings.Add("no visible choices resolved for this observation.");
        }

        return warnings.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static void AppendRestSiteLifecycleMetadata(
        RuntimeHookBinding binding,
        DateTimeOffset observedAt,
        object?[]? args,
        IDictionary<string, object?> payload,
        IDictionary<string, string?> meta)
    {
        if (!string.Equals(binding.Candidate.Category, "rest-site", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var option = args?.FirstOrDefault(arg =>
            arg is not null
            && (arg.GetType().FullName ?? arg.GetType().Name).Contains("RestSiteOption", StringComparison.OrdinalIgnoreCase));
        var optionId = option is null ? null : TryResolveRestSiteOptionId(option, option);
        if (string.IsNullOrWhiteSpace(optionId))
        {
            return;
        }

        string signal;
        string? successValue = null;
        switch (binding.Candidate.SemanticKind)
        {
            case "rest-site-option-selection-started":
                signal = "before-select";
                break;
            case "rest-site-option-selection-finished":
            {
                var success = args is { Length: >= 2 } ? TryConvertToBool(args[1]) : null;
                signal = success == true ? "after-select-success" : "after-select-failure";
                successValue = success?.ToString().ToLowerInvariant();
                payload["restSiteSelectionSuccess"] = success;
                break;
            }
            default:
                return;
        }

        meta["restSiteSelectionLastSignal"] = signal;
        meta["restSiteSelectionLastOptionId"] = optionId;
        meta["restSiteSelectionLastSuccess"] = successValue;
        meta["restSiteSelectionLastSignalAt"] = observedAt.ToString("O", CultureInfo.InvariantCulture);
        payload["restSiteSelectionSignal"] = signal;
        payload["restSiteSelectionOptionId"] = optionId;
    }

    private static void AppendRestSiteRuntimeMetadata(
        IReadOnlyList<object> roots,
        IReadOnlyList<LiveExportChoiceSummary> choices,
        string screen,
        IDictionary<string, string?> meta,
        IDictionary<string, object?> payload)
    {
        var buttonObservation = ObserveRestSiteButtons(roots);
        var upgradeObservation = ObserveRestSiteUpgrade(roots, screen);
        var restSiteRelevant = choices.Any(static choice =>
                                  string.Equals(choice.Kind, "rest-option", StringComparison.OrdinalIgnoreCase)
                                  || string.Equals(choice.Kind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase)
                                  || string.Equals(choice.Kind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase))
                               || upgradeObservation.ScreenDetected
                               || roots.Any(static root =>
                               {
                                   var typeName = root.GetType().FullName ?? root.GetType().Name;
                                   return typeName.Contains("NRestSiteRoom", StringComparison.OrdinalIgnoreCase)
                                          || typeName.Contains("NRestSiteButton", StringComparison.OrdinalIgnoreCase)
                                          || typeName.Contains("RestSiteOption", StringComparison.OrdinalIgnoreCase);
                               });

        var lastSignal = meta.TryGetValue("restSiteSelectionLastSignal", out var rawLastSignal) ? rawLastSignal : null;
        var lastOptionId = meta.TryGetValue("restSiteSelectionLastOptionId", out var rawLastOptionId) ? rawLastOptionId : null;
        var currentStatus = DetermineRestSiteCurrentStatus(buttonObservation, upgradeObservation, choices, lastSignal);
        var currentOptionId = DetermineRestSiteCurrentOptionId(buttonObservation, upgradeObservation);
        var viewKind = DetermineRestSiteViewKind(buttonObservation, upgradeObservation, choices);
        var transitionAuthority = DetermineRestSiteTransitionAuthority(buttonObservation, upgradeObservation);
        var selectionOutcome = DetermineRestSiteSelectionOutcome(buttonObservation, upgradeObservation, lastSignal);
        var selectionOutcomeEvidence = DetermineRestSiteSelectionOutcomeEvidence(buttonObservation, upgradeObservation, lastSignal);
        var observedOptionId = currentOptionId ?? lastOptionId;

        meta["restSiteButtonsVisible"] = buttonObservation.VisibleButtonCount > 0 ? "true" : "false";
        meta["restSiteHoveredOptionId"] = restSiteRelevant ? buttonObservation.HoveredOptionId : null;
        meta["restSiteExecutingOptionId"] = restSiteRelevant ? buttonObservation.ExecutingOptionId : null;
        meta["restSiteOptionsInteractive"] = buttonObservation.OptionsInteractive ? "true" : "false";
        meta["restSiteVisibleOptionSummary"] = restSiteRelevant ? buttonObservation.VisibleOptionSummary : null;
        meta["restSiteUpgradeScreenVisible"] = upgradeObservation.ScreenVisible ? "true" : "false";
        meta["restSiteUpgradeScreenDetected"] = upgradeObservation.ScreenDetected ? "true" : "false";
        meta["restSiteUpgradeObserverMiss"] = upgradeObservation.ObserverMiss ? "true" : "false";
        meta["restSiteUpgradeCardCount"] = upgradeObservation.VisibleCards.Count.ToString(CultureInfo.InvariantCulture);
        meta["restSiteUpgradeCardSummary"] = upgradeObservation.VisibleCards.Count == 0
            ? null
            : string.Join(";", upgradeObservation.VisibleCards.Select(static card => $"{card.CardId ?? SanitizeNodeKey(card.Label)}@{card.ScreenBounds}"));
        meta["restSiteUpgradeConfirmVisible"] = upgradeObservation.ConfirmVisible ? "true" : "false";
        meta["restSiteUpgradeConfirmEnabled"] = upgradeObservation.ConfirmEnabled ? "true" : "false";
        meta["restSiteUpgradeConfirmBounds"] = upgradeObservation.ConfirmBounds;
        meta["restSiteUpgradeSelectedCardCount"] = upgradeObservation.SelectedCardCount.ToString(CultureInfo.InvariantCulture);
        meta["restSiteUpgradeSelectedCards"] = upgradeObservation.SelectedCardsSummary;
        meta["restSiteUpgradePreviewMode"] = upgradeObservation.PreviewMode;
        meta["restSiteSelectionCurrentStatus"] = restSiteRelevant ? currentStatus : null;
        meta["restSiteSelectionCurrentOptionId"] = restSiteRelevant ? currentOptionId : null;
        meta["restSiteSelectionObservedOptionId"] = restSiteRelevant ? observedOptionId : null;
        meta["restSiteSelectionOutcome"] = restSiteRelevant ? selectionOutcome : null;
        meta["restSiteSelectionOutcomeEvidence"] = restSiteRelevant ? selectionOutcomeEvidence : null;
        meta["restSiteViewKind"] = restSiteRelevant ? viewKind : null;
        meta["restSiteTransitionAuthority"] = restSiteRelevant ? transitionAuthority : null;

        payload["restSiteButtonsVisible"] = buttonObservation.VisibleButtonCount > 0;
        payload["restSiteUpgradeScreenVisible"] = upgradeObservation.ScreenVisible;
        payload["restSiteUpgradeScreenDetected"] = upgradeObservation.ScreenDetected;
        payload["restSiteUpgradeObserverMiss"] = upgradeObservation.ObserverMiss;
        payload["restSiteUpgradeCardCount"] = upgradeObservation.VisibleCards.Count;
        payload["restSiteUpgradeConfirmVisible"] = upgradeObservation.ConfirmVisible;
        payload["restSiteUpgradeConfirmEnabled"] = upgradeObservation.ConfirmEnabled;
        if (!string.IsNullOrWhiteSpace(currentStatus))
        {
            payload["restSiteSelectionCurrentStatus"] = currentStatus;
        }

        if (!string.IsNullOrWhiteSpace(currentOptionId))
        {
            payload["restSiteSelectionCurrentOptionId"] = currentOptionId;
        }

        if (!string.IsNullOrWhiteSpace(observedOptionId))
        {
            payload["restSiteSelectionObservedOptionId"] = observedOptionId;
        }

        if (!string.IsNullOrWhiteSpace(selectionOutcome))
        {
            payload["restSiteSelectionOutcome"] = selectionOutcome;
        }

        if (!string.IsNullOrWhiteSpace(viewKind))
        {
            payload["restSiteViewKind"] = viewKind;
        }
    }

    private static void AppendCardSelectionRuntimeMetadata(
        CardSelectionObservation observation,
        IDictionary<string, string?> meta,
        IDictionary<string, object?> payload)
    {
        meta["cardSelectionScreenDetected"] = observation.ScreenDetected ? "true" : "false";
        meta["cardSelectionScreenType"] = observation.ScreenDetected ? observation.ScreenType : null;
        meta["cardSelectionPrompt"] = observation.Prompt;
        meta["cardSelectionMinSelect"] = observation.MinSelect?.ToString(CultureInfo.InvariantCulture);
        meta["cardSelectionMaxSelect"] = observation.MaxSelect?.ToString(CultureInfo.InvariantCulture);
        meta["cardSelectionSelectedCount"] = observation.SelectedCount.ToString(CultureInfo.InvariantCulture);
        meta["cardSelectionRequireManualConfirmation"] = observation.RequireManualConfirmation?.ToString().ToLowerInvariant();
        meta["cardSelectionCancelable"] = observation.Cancelable?.ToString().ToLowerInvariant();
        meta["cardSelectionPreviewVisible"] = observation.PreviewVisible ? "true" : "false";
        meta["cardSelectionMainConfirmEnabled"] = observation.MainConfirmEnabled ? "true" : "false";
        meta["cardSelectionPreviewConfirmEnabled"] = observation.PreviewConfirmEnabled ? "true" : "false";
        meta["cardSelectionPreviewMode"] = observation.PreviewMode;
        meta["cardSelectionSelectedCardIds"] = observation.SelectedCardIds.Count == 0
            ? null
            : string.Join(",", observation.SelectedCardIds);
        meta["cardSelectionRootType"] = observation.RootType;
        meta["cardSelectionVisibleCardCount"] = observation.VisibleCards.Count.ToString(CultureInfo.InvariantCulture);

        payload["cardSelectionScreenDetected"] = observation.ScreenDetected;
        payload["cardSelectionScreenType"] = observation.ScreenDetected ? observation.ScreenType : null;
        payload["cardSelectionPrompt"] = observation.Prompt;
        payload["cardSelectionMinSelect"] = observation.MinSelect;
        payload["cardSelectionMaxSelect"] = observation.MaxSelect;
        payload["cardSelectionSelectedCount"] = observation.SelectedCount;
        payload["cardSelectionRequireManualConfirmation"] = observation.RequireManualConfirmation;
        payload["cardSelectionCancelable"] = observation.Cancelable;
        payload["cardSelectionPreviewVisible"] = observation.PreviewVisible;
        payload["cardSelectionMainConfirmEnabled"] = observation.MainConfirmEnabled;
        payload["cardSelectionPreviewConfirmEnabled"] = observation.PreviewConfirmEnabled;
        payload["cardSelectionPreviewMode"] = observation.PreviewMode;
        payload["cardSelectionRootType"] = observation.RootType;
        if (observation.SelectedCardIds.Count > 0)
        {
            payload["cardSelectionSelectedCardIds"] = observation.SelectedCardIds.ToArray();
        }
    }

    private static void AppendTreasureRoomRuntimeMetadata(
        TreasureRoomObservation observation,
        IReadOnlyList<LiveExportChoiceSummary> choices,
        IDictionary<string, string?> meta,
        IDictionary<string, object?> payload)
    {
        var inspectOverlayVisible = observation.RoomDetected && choices.Any(static choice =>
            string.Equals(choice.Kind, "treasure-overlay-back", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.Kind, "overlay-back", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.Label, "Backstop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.Label, "LeftArrow", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.Label, "RightArrow", StringComparison.OrdinalIgnoreCase));
        var holderBounds = observation.VisibleRelicHolders.Count == 0
            ? null
            : string.Join(";", observation.VisibleRelicHolders.Select(static holder => holder.ScreenBounds));
        var holderIds = observation.RelicHolderIds.Count == 0
            ? null
            : string.Join(",", observation.RelicHolderIds);

        meta["treasureRoomDetected"] = observation.RoomDetected ? "true" : "false";
        meta["treasureChestClickable"] = observation.ChestClickable ? "true" : "false";
        meta["treasureChestOpened"] = observation.ChestOpened ? "true" : "false";
        meta["treasureRelicCollectionOpen"] = observation.RelicCollectionOpen ? "true" : "false";
        meta["sharedRelicPickingActive"] = observation.SharedRelicPickingActive ? "true" : "false";
        meta["mapScreenOpen"] = observation.MapScreenOpen ? "true" : "false";
        meta["treasureRelicHolderCount"] = observation.RelicHolderCount.ToString(CultureInfo.InvariantCulture);
        meta["treasureVisibleRelicHolderCount"] = observation.VisibleRelicHolderCount.ToString(CultureInfo.InvariantCulture);
        meta["treasureEnabledRelicHolderCount"] = observation.EnabledRelicHolderCount.ToString(CultureInfo.InvariantCulture);
        meta["treasureProceedEnabled"] = observation.ProceedEnabled ? "true" : "false";
        meta["treasureInspectOverlayVisible"] = inspectOverlayVisible ? "true" : "false";
        meta["treasureRelicHolderBounds"] = holderBounds;
        meta["treasureRelicHolderIds"] = holderIds;
        meta["treasureRoomRootType"] = observation.RootType;

        payload["treasureRoomDetected"] = observation.RoomDetected;
        payload["treasureChestClickable"] = observation.ChestClickable;
        payload["treasureChestOpened"] = observation.ChestOpened;
        payload["treasureRelicCollectionOpen"] = observation.RelicCollectionOpen;
        payload["sharedRelicPickingActive"] = observation.SharedRelicPickingActive;
        payload["mapScreenOpen"] = observation.MapScreenOpen;
        payload["treasureRelicHolderCount"] = observation.RelicHolderCount;
        payload["treasureVisibleRelicHolderCount"] = observation.VisibleRelicHolderCount;
        payload["treasureEnabledRelicHolderCount"] = observation.EnabledRelicHolderCount;
        payload["treasureProceedEnabled"] = observation.ProceedEnabled;
        payload["treasureInspectOverlayVisible"] = inspectOverlayVisible;
        payload["treasureRoomRootType"] = observation.RootType;
        if (!string.IsNullOrWhiteSpace(holderBounds))
        {
            payload["treasureRelicHolderBounds"] = observation.VisibleRelicHolders
                .Select(static holder => holder.ScreenBounds)
                .ToArray();
        }

        if (observation.RelicHolderIds.Count > 0)
        {
            payload["treasureRelicHolderIds"] = observation.RelicHolderIds.ToArray();
        }
    }

    private static void AppendShopRuntimeMetadata(
        ShopObservation observation,
        IDictionary<string, string?> meta,
        IDictionary<string, object?> payload)
    {
        meta["shopRoomDetected"] = observation.RoomDetected ? "true" : "false";
        meta["shopRoomVisible"] = observation.RoomVisible ? "true" : "false";
        meta["shopForegroundOwned"] = observation.ForegroundOwned ? "true" : "false";
        meta["shopTeardownInProgress"] = observation.TeardownInProgress ? "true" : "false";
        meta["shopIsCurrentActiveScreen"] = observation.ShopIsCurrentActiveScreen ? "true" : "false";
        meta["mapCurrentActiveScreen"] = observation.MapIsCurrentActiveScreen ? "true" : "false";
        meta["activeScreenType"] = observation.ActiveScreenType;
        meta["shopRootType"] = observation.RootType;
        meta["shopInventoryOpen"] = observation.InventoryOpen ? "true" : "false";
        meta["shopMerchantButtonVisible"] = observation.MerchantButtonVisible ? "true" : "false";
        meta["shopMerchantButtonEnabled"] = observation.MerchantButtonEnabled ? "true" : "false";
        meta["shopProceedEnabled"] = observation.ProceedEnabled ? "true" : "false";
        meta["shopBackVisible"] = observation.BackVisible ? "true" : "false";
        meta["shopBackEnabled"] = observation.BackEnabled ? "true" : "false";
        meta["shopOptionCount"] = observation.OptionCount.ToString(CultureInfo.InvariantCulture);
        meta["shopAffordableOptionCount"] = observation.AffordableOptionCount.ToString(CultureInfo.InvariantCulture);
        meta["shopAffordableOptionIds"] = observation.AffordableOptionIds.Count == 0 ? null : string.Join(",", observation.AffordableOptionIds);
        meta["shopAffordableRelicIds"] = observation.AffordableRelicIds.Count == 0 ? null : string.Join(",", observation.AffordableRelicIds);
        meta["shopAffordableCardIds"] = observation.AffordableCardIds.Count == 0 ? null : string.Join(",", observation.AffordableCardIds);
        meta["shopAffordablePotionIds"] = observation.AffordablePotionIds.Count == 0 ? null : string.Join(",", observation.AffordablePotionIds);
        meta["shopCardRemovalVisible"] = observation.CardRemovalVisible ? "true" : "false";
        meta["shopCardRemovalEnabled"] = observation.CardRemovalEnabled ? "true" : "false";
        meta["shopCardRemovalEnoughGold"] = observation.CardRemovalEnoughGold ? "true" : "false";
        meta["shopCardRemovalUsed"] = observation.CardRemovalUsed ? "true" : "false";

        payload["shopRoomDetected"] = observation.RoomDetected;
        payload["shopRoomVisible"] = observation.RoomVisible;
        payload["shopForegroundOwned"] = observation.ForegroundOwned;
        payload["shopTeardownInProgress"] = observation.TeardownInProgress;
        payload["shopIsCurrentActiveScreen"] = observation.ShopIsCurrentActiveScreen;
        payload["mapCurrentActiveScreen"] = observation.MapIsCurrentActiveScreen;
        payload["activeScreenType"] = observation.ActiveScreenType;
        payload["shopRootType"] = observation.RootType;
        payload["shopInventoryOpen"] = observation.InventoryOpen;
        payload["shopMerchantButtonVisible"] = observation.MerchantButtonVisible;
        payload["shopMerchantButtonEnabled"] = observation.MerchantButtonEnabled;
        payload["shopProceedEnabled"] = observation.ProceedEnabled;
        payload["shopBackVisible"] = observation.BackVisible;
        payload["shopBackEnabled"] = observation.BackEnabled;
        payload["shopOptionCount"] = observation.OptionCount;
        payload["shopAffordableOptionCount"] = observation.AffordableOptionCount;
        payload["shopCardRemovalVisible"] = observation.CardRemovalVisible;
        payload["shopCardRemovalEnabled"] = observation.CardRemovalEnabled;
        payload["shopCardRemovalEnoughGold"] = observation.CardRemovalEnoughGold;
        payload["shopCardRemovalUsed"] = observation.CardRemovalUsed;
        if (observation.AffordableOptionIds.Count > 0)
        {
            payload["shopAffordableOptionIds"] = observation.AffordableOptionIds.ToArray();
        }

        if (observation.AffordableRelicIds.Count > 0)
        {
            payload["shopAffordableRelicIds"] = observation.AffordableRelicIds.ToArray();
        }

        if (observation.AffordableCardIds.Count > 0)
        {
            payload["shopAffordableCardIds"] = observation.AffordableCardIds.ToArray();
        }

        if (observation.AffordablePotionIds.Count > 0)
        {
            payload["shopAffordablePotionIds"] = observation.AffordablePotionIds.ToArray();
        }
    }

    private static void AppendRewardRuntimeMetadata(
        RewardObservation observation,
        IDictionary<string, string?> meta,
        IDictionary<string, object?> payload)
    {
        meta["rewardScreenDetected"] = observation.ScreenDetected ? "true" : "false";
        meta["rewardScreenVisible"] = observation.ScreenVisible ? "true" : "false";
        meta["rewardForegroundOwned"] = observation.ForegroundOwned ? "true" : "false";
        meta["rewardTeardownInProgress"] = observation.TeardownInProgress ? "true" : "false";
        meta["rewardIsCurrentActiveScreen"] = observation.RewardIsCurrentActiveScreen ? "true" : "false";
        meta["rewardIsTopOverlay"] = observation.RewardIsTopOverlay ? "true" : "false";
        meta["mapCurrentActiveScreen"] = observation.MapIsCurrentActiveScreen ? "true" : "false";
        meta["activeScreenType"] = observation.ActiveScreenType;
        meta["rawTopOverlayType"] = observation.TopOverlayType;
        meta["rewardScreenRootType"] = observation.RootType;
        meta["rawRewardScreenRootType"] = observation.RootType;
        meta["rewardProceedVisible"] = observation.ProceedVisible ? "true" : "false";
        meta["rewardProceedEnabled"] = observation.ProceedEnabled ? "true" : "false";
        meta["rewardVisibleButtonCount"] = observation.VisibleButtonCount.ToString(CultureInfo.InvariantCulture);
        meta["rewardEnabledButtonCount"] = observation.EnabledButtonCount.ToString(CultureInfo.InvariantCulture);
        if (observation.HasOpenPotionSlots is not null)
        {
            meta["hasOpenPotionSlots"] = observation.HasOpenPotionSlots == true ? "true" : "false";
        }
        meta["terminalRunBoundary"] = observation.TerminalRunBoundary ? "true" : "false";
        meta["gameOverScreenDetected"] = observation.GameOverScreenDetected ? "true" : "false";
        meta["unlockScreenDetected"] = observation.UnlockScreenDetected ? "true" : "false";
        meta["timelineUnlockDetected"] = observation.TimelineUnlockDetected ? "true" : "false";
        meta["mainMenuReturnDetected"] = observation.MainMenuReturnDetected ? "true" : "false";

        payload["rewardScreenDetected"] = observation.ScreenDetected;
        payload["rewardScreenVisible"] = observation.ScreenVisible;
        payload["rewardForegroundOwned"] = observation.ForegroundOwned;
        payload["rewardTeardownInProgress"] = observation.TeardownInProgress;
        payload["rewardIsCurrentActiveScreen"] = observation.RewardIsCurrentActiveScreen;
        payload["rewardIsTopOverlay"] = observation.RewardIsTopOverlay;
        payload["mapCurrentActiveScreen"] = observation.MapIsCurrentActiveScreen;
        payload["activeScreenType"] = observation.ActiveScreenType;
        payload["rawTopOverlayType"] = observation.TopOverlayType;
        payload["rewardScreenRootType"] = observation.RootType;
        payload["rawRewardScreenRootType"] = observation.RootType;
        payload["rewardProceedVisible"] = observation.ProceedVisible;
        payload["rewardProceedEnabled"] = observation.ProceedEnabled;
        payload["rewardVisibleButtonCount"] = observation.VisibleButtonCount;
        payload["rewardEnabledButtonCount"] = observation.EnabledButtonCount;
        if (observation.HasOpenPotionSlots is not null)
        {
            payload["hasOpenPotionSlots"] = observation.HasOpenPotionSlots == true;
        }
        payload["terminalRunBoundary"] = observation.TerminalRunBoundary;
        payload["gameOverScreenDetected"] = observation.GameOverScreenDetected;
        payload["unlockScreenDetected"] = observation.UnlockScreenDetected;
        payload["timelineUnlockDetected"] = observation.TimelineUnlockDetected;
        payload["mainMenuReturnDetected"] = observation.MainMenuReturnDetected;
    }

    private static IReadOnlyList<LiveExportChoiceSummary> CreateCardSelectionChoices(CardSelectionObservation observation)
    {
        if (!observation.ScreenVisible)
        {
            return Array.Empty<LiveExportChoiceSummary>();
        }

        var choices = new List<LiveExportChoiceSummary>();
        var cardKind = observation.ScreenType switch
        {
            "transform" => "transform-card",
            "deck-remove" => "deck-remove-card",
            "upgrade" => "upgrade-card",
            "reward-pick" => "reward-pick-card",
            "simple-select" => "simple-select-card",
            "bundle-select" => "bundle-select-card",
            "relic-select" => "relic-select-card",
            _ => "card-selection-card",
        };
        var confirmKind = observation.ScreenType switch
        {
            "transform" => "transform-confirm",
            "deck-remove" => "deck-remove-confirm",
            "upgrade" => "upgrade-confirm",
            "simple-select" => "simple-select-confirm",
            "bundle-select" => "bundle-select-confirm",
            _ => null,
        };

        if (!string.IsNullOrWhiteSpace(confirmKind))
        {
            if (!string.IsNullOrWhiteSpace(observation.PreviewConfirmBounds))
            {
                choices.Add(new LiveExportChoiceSummary(confirmKind, "Confirm", "preview-confirm", observation.Prompt)
                {
                    ScreenBounds = observation.PreviewConfirmBounds,
                    NodeId = $"card-selection:{observation.ScreenType}:preview-confirm",
                    BindingKind = "card-selection-confirm",
                    BindingId = "preview",
                    SemanticHints = BuildCardSelectionConfirmSemanticHints(observation.ScreenType, "preview"),
                    Enabled = observation.PreviewConfirmEnabled,
                });
            }

            if (!string.IsNullOrWhiteSpace(observation.MainConfirmBounds))
            {
                choices.Add(new LiveExportChoiceSummary(confirmKind, "Confirm", "main-confirm", observation.Prompt)
                {
                    ScreenBounds = observation.MainConfirmBounds,
                    NodeId = $"card-selection:{observation.ScreenType}:main-confirm",
                    BindingKind = "card-selection-confirm",
                    BindingId = "main",
                    SemanticHints = BuildCardSelectionConfirmSemanticHints(observation.ScreenType, "main"),
                    Enabled = observation.MainConfirmEnabled,
                });
            }
        }

        var cardIndex = 0;
        foreach (var card in observation.VisibleCards)
        {
            cardIndex += 1;
            choices.Add(new LiveExportChoiceSummary(
                cardKind,
                card.Label,
                card.CardId ?? SanitizeNodeKey(card.Label),
                observation.Prompt)
            {
                ScreenBounds = card.ScreenBounds,
                NodeId = $"card-selection:{observation.ScreenType}:card:{cardIndex}",
                BindingKind = "card-selection-card",
                BindingId = observation.ScreenType,
                SemanticHints = BuildCardSelectionSemanticHints(observation.ScreenType, card.Selected),
                Enabled = card.Enabled,
            });
        }

        return choices;
    }

    private static IReadOnlyList<LiveExportChoiceSummary> CreateTreasureRoomChoices(TreasureRoomObservation observation)
    {
        if (!observation.RoomVisible)
        {
            return Array.Empty<LiveExportChoiceSummary>();
        }

        var choices = new List<LiveExportChoiceSummary>();
        if (observation.ChestClickable && !string.IsNullOrWhiteSpace(observation.ChestBounds))
        {
            choices.Add(new LiveExportChoiceSummary(
                "treasure-chest",
                "Chest",
                "treasure:chest",
                "Treasure room chest")
            {
                NodeId = "treasure:chest",
                ScreenBounds = observation.ChestBounds,
                BindingKind = "treasure-room",
                BindingId = "chest",
                Enabled = true,
                SemanticHints = new[] { "treasure-room", "treasure-chest" },
            });
        }

        var holderOrdinal = 0;
        foreach (var holder in observation.VisibleRelicHolders)
        {
            choices.Add(new LiveExportChoiceSummary(
                "treasure-relic-holder",
                holder.Label,
                holder.RelicId,
                "Treasure room relic holder")
            {
                NodeId = $"treasure:holder:{holderOrdinal}",
                ScreenBounds = holder.ScreenBounds,
                BindingKind = "treasure-room",
                BindingId = holder.RelicId ?? $"holder:{holderOrdinal}",
                Enabled = holder.Enabled,
                SemanticHints = new[] { "treasure-room", "treasure-relic-holder" },
            });
            holderOrdinal += 1;
        }

        if (observation.ProceedEnabled && !string.IsNullOrWhiteSpace(observation.ProceedBounds))
        {
            choices.Add(new LiveExportChoiceSummary(
                "treasure-proceed",
                "Proceed",
                "treasure:proceed",
                "Treasure room proceed")
            {
                NodeId = "treasure:proceed",
                ScreenBounds = observation.ProceedBounds,
                BindingKind = "treasure-room",
                BindingId = "proceed",
                Enabled = true,
                SemanticHints = new[] { "treasure-room", "treasure-proceed" },
            });
        }

        return choices;
    }

    private static IReadOnlyList<LiveExportChoiceSummary> CreateShopChoices(ShopObservation observation)
    {
        if (!observation.ForegroundOwned)
        {
            return Array.Empty<LiveExportChoiceSummary>();
        }

        var choices = new List<LiveExportChoiceSummary>();
        if (observation.MerchantButtonVisible && !string.IsNullOrWhiteSpace(observation.MerchantButtonBounds))
        {
            choices.Add(new LiveExportChoiceSummary(
                "shop-open-inventory",
                "Merchant",
                "shop:merchant",
                "Open merchant inventory")
            {
                NodeId = "shop:merchant",
                ScreenBounds = observation.MerchantButtonBounds,
                BindingKind = "shop-room",
                BindingId = "merchant-button",
                Enabled = observation.MerchantButtonEnabled,
                SemanticHints = new[] { "scene:shop", "shop-action:open-inventory", "source:merchant-button" },
            });
        }

        if (observation.BackVisible && !string.IsNullOrWhiteSpace(observation.BackBounds))
        {
            choices.Add(new LiveExportChoiceSummary(
                "shop-back",
                "Back",
                "shop:back",
                "Close merchant inventory")
            {
                NodeId = "shop:back",
                ScreenBounds = observation.BackBounds,
                BindingKind = "shop-room",
                BindingId = "back",
                Enabled = observation.BackEnabled,
                SemanticHints = new[] { "scene:shop", "shop-action:back", "source:back-button" },
            });
        }

        if (observation.ProceedEnabled && !string.IsNullOrWhiteSpace(observation.ProceedBounds))
        {
            choices.Add(new LiveExportChoiceSummary(
                "shop-proceed",
                "Proceed",
                "shop:proceed",
                "Leave the shop")
            {
                NodeId = "shop:proceed",
                ScreenBounds = observation.ProceedBounds,
                BindingKind = "shop-room",
                BindingId = "proceed",
                Enabled = true,
                SemanticHints = new[] { "scene:shop", "shop-action:proceed", "source:proceed-button" },
            });
        }

        var visibleOptions = observation.VisibleOptions
            .OrderByDescending(static candidate => candidate.Enabled && candidate.IsStocked && candidate.EnoughGold && !candidate.Used ? 1 : 0)
            .ThenBy(static candidate => GetShopOptionPriority(candidate.OptionType))
            .ThenBy(static candidate => TryGetBoundsSortX(candidate.ScreenBounds))
            .ThenBy(static candidate => TryGetBoundsSortY(candidate.ScreenBounds))
            .ToArray();
        var optionOrdinal = 0;
        foreach (var option in visibleOptions)
        {
            optionOrdinal += 1;
            var optionKind = option.OptionType switch
            {
                "relic" => "shop-option:relic",
                "card" => "shop-option:card",
                "potion" => "shop-option:potion",
                "card-removal" => "shop-card-removal",
                _ => "shop-option",
            };
            var bindingKind = option.OptionType == "card-removal" ? "shop-card-removal" : "shop-option";
            var bindingId = option.EntryId ?? $"{option.OptionType}:{optionOrdinal}";
            var semanticHints = new List<string>
            {
                "scene:shop",
                $"shop-type:{option.OptionType}",
                $"stocked:{option.IsStocked.ToString().ToLowerInvariant()}",
                $"enough-gold:{option.EnoughGold.ToString().ToLowerInvariant()}",
                $"used:{option.Used.ToString().ToLowerInvariant()}",
                $"source:{option.BoundsSource}",
            };
            choices.Add(new LiveExportChoiceSummary(
                optionKind,
                option.Label,
                option.EntryId,
                option.OptionType == "card-removal" ? "Merchant card removal service" : "Merchant inventory slot")
            {
                NodeId = option.OptionType == "card-removal"
                    ? "shop:card-removal"
                    : $"shop:{option.OptionType}:{SanitizeNodeKey(bindingId)}",
                ScreenBounds = option.ScreenBounds,
                BindingKind = bindingKind,
                BindingId = bindingId,
                Enabled = option.Enabled && option.IsStocked && option.EnoughGold && !option.Used,
                SemanticHints = semanticHints,
            });
        }

        return choices;
    }

    private static void RemoveShopChoiceContamination(List<LiveExportChoiceSummary> choices, ShopObservation observation)
    {
        if (!observation.RoomVisible)
        {
            return;
        }

        var explicitOptionLabels = observation.VisibleOptions
            .Select(static option => option.Label)
            .Where(static label => !string.IsNullOrWhiteSpace(label))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var index = choices.Count - 1; index >= 0; index -= 1)
        {
            var choice = choices[index];
            if (!choice.Kind.StartsWith("shop-", StringComparison.OrdinalIgnoreCase)
                && choice.Kind is "relic" or "potion" or "card" or "choice" or "proceed" or "overlay-dismiss" or "overlay-back")
            {
                choices.RemoveAt(index);
                continue;
            }

            if (explicitOptionLabels.Contains(choice.Label)
                && choice.Kind is "relic" or "potion" or "card" or "choice")
            {
                choices.RemoveAt(index);
            }
        }
    }

    private static int GetShopOptionPriority(string optionType)
    {
        return optionType switch
        {
            "card-removal" => 0,
            "relic" => 1,
            "card" => 2,
            "potion" => 3,
            _ => 9,
        };
    }

    private static IReadOnlyList<string> BuildCardSelectionSemanticHints(string screenType, bool selected)
    {
        var hints = new List<string> { $"card-selection:{screenType}" };
        if (selected)
        {
            hints.Add("selected-card");
        }

        if (string.Equals(screenType, "reward-pick", StringComparison.OrdinalIgnoreCase))
        {
            hints.Add("reward-pick");
        }
        else if (string.Equals(screenType, "simple-select", StringComparison.OrdinalIgnoreCase))
        {
            hints.Add("simple-select");
        }
        else if (string.Equals(screenType, "bundle-select", StringComparison.OrdinalIgnoreCase))
        {
            hints.Add("bundle-select");
        }
        else if (string.Equals(screenType, "relic-select", StringComparison.OrdinalIgnoreCase))
        {
            hints.Add("relic-select");
        }

        return hints;
    }

    private static IReadOnlyList<string> BuildCardSelectionConfirmSemanticHints(string screenType, string mode)
    {
        return new[]
        {
            $"card-selection:{screenType}",
            $"confirm-mode:{mode}",
        };
    }

    private static string GetCardSelectionExtractorPath(string screenType)
    {
        return screenType switch
        {
            "transform" => "card-selection-transform",
            "deck-remove" => "card-selection-deck-remove",
            "upgrade" => "card-selection-upgrade",
            "reward-pick" => "card-selection-reward-pick",
            "simple-select" => "card-selection-simple-select",
            "bundle-select" => "card-selection-bundle-select",
            "relic-select" => "card-selection-relic-select",
            _ => "card-selection-unknown",
        };
    }

    private static void AddChoiceSummary(List<LiveExportChoiceSummary> choices, LiveExportChoiceSummary choice, int maxEntries)
    {
        if (choices.Any(existing =>
                string.Equals(existing.Kind, choice.Kind, StringComparison.OrdinalIgnoreCase)
                && string.Equals(existing.Label, choice.Label, StringComparison.OrdinalIgnoreCase)
                && string.Equals(existing.ScreenBounds, choice.ScreenBounds, StringComparison.OrdinalIgnoreCase)
                && string.Equals(existing.NodeId, choice.NodeId, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        if (choices.Count >= maxEntries)
        {
            return;
        }

        choices.Add(choice);
    }

    private static void RemoveTreasureInventoryChoiceContamination(List<LiveExportChoiceSummary> choices)
    {
        for (var index = choices.Count - 1; index >= 0; index -= 1)
        {
            if (string.Equals(choices[index].Kind, "relic", StringComparison.OrdinalIgnoreCase))
            {
                choices.RemoveAt(index);
            }
        }
    }

    private static string BuildChoiceSignature(IReadOnlyList<LiveExportChoiceSummary> choices)
    {
        return string.Join(
            ";",
            choices.Take(12).Select(static choice =>
                $"{choice.Kind}:{choice.BindingId ?? choice.NodeId ?? choice.Label}:{choice.Enabled?.ToString().ToLowerInvariant() ?? "null"}"));
    }

    private static string? DetermineRestSiteCurrentStatus(
        RestSiteButtonObservation buttonObservation,
        RestSiteUpgradeObservation upgradeObservation,
        IReadOnlyList<LiveExportChoiceSummary> choices,
        string? lastSignal)
    {
        if (upgradeObservation.ConfirmVisible)
        {
            return "confirm-visible";
        }

        if (upgradeObservation.VisibleCards.Count > 0)
        {
            return "grid-visible";
        }

        if (upgradeObservation.ObserverMiss)
        {
            return "grid-observer-miss";
        }

        if (!string.IsNullOrWhiteSpace(buttonObservation.ExecutingOptionId))
        {
            return "selecting";
        }

        if (string.Equals(lastSignal, "after-select-failure", StringComparison.OrdinalIgnoreCase))
        {
            return "selection-failed";
        }

        if (buttonObservation.VisibleButtonCount > 0
            && choices.Any(static choice => string.Equals(choice.Kind, "rest-option", StringComparison.OrdinalIgnoreCase)))
        {
            return "explicit-choice";
        }

        if (buttonObservation.VisibleButtonCount > 0 && !buttonObservation.OptionsInteractive)
        {
            return "options-disabled";
        }

        return null;
    }

    private static string? DetermineRestSiteCurrentOptionId(
        RestSiteButtonObservation buttonObservation,
        RestSiteUpgradeObservation upgradeObservation)
    {
        if (upgradeObservation.ScreenVisible)
        {
            return "SMITH";
        }

        return buttonObservation.ExecutingOptionId ?? buttonObservation.HoveredOptionId;
    }

    private static string? DetermineRestSiteViewKind(
        RestSiteButtonObservation buttonObservation,
        RestSiteUpgradeObservation upgradeObservation,
        IReadOnlyList<LiveExportChoiceSummary> choices)
    {
        if (upgradeObservation.ConfirmVisible)
        {
            return "smith-confirm";
        }

        if (upgradeObservation.VisibleCards.Count > 0)
        {
            return "smith-grid";
        }

        if (upgradeObservation.ObserverMiss)
        {
            return "smith-grid-observer-miss";
        }

        if (buttonObservation.VisibleButtonCount > 0
            || choices.Any(static choice => string.Equals(choice.Kind, "rest-option", StringComparison.OrdinalIgnoreCase)))
        {
            return "explicit-choice";
        }

        return null;
    }

    private static string? DetermineRestSiteTransitionAuthority(
        RestSiteButtonObservation buttonObservation,
        RestSiteUpgradeObservation upgradeObservation)
    {
        if (upgradeObservation.ConfirmVisible || upgradeObservation.VisibleCards.Count > 0)
        {
            return "runtime-poll:deck-upgrade-screen";
        }

        if (upgradeObservation.ScreenVisible)
        {
            return "runtime-screen:upgrade";
        }

        if (buttonObservation.VisibleButtonCount > 0 || !string.IsNullOrWhiteSpace(buttonObservation.ExecutingOptionId))
        {
            return "runtime-poll:rest-site-button";
        }

        return null;
    }

    private static string? DetermineRestSiteSelectionOutcome(
        RestSiteButtonObservation buttonObservation,
        RestSiteUpgradeObservation upgradeObservation,
        string? lastSignal)
    {
        if (string.Equals(lastSignal, "after-select-failure", StringComparison.OrdinalIgnoreCase))
        {
            return "failure";
        }

        if (upgradeObservation.ScreenVisible
            || string.Equals(lastSignal, "after-select-success", StringComparison.OrdinalIgnoreCase))
        {
            return "success";
        }

        if (!string.IsNullOrWhiteSpace(buttonObservation.ExecutingOptionId)
            || string.Equals(lastSignal, "before-select", StringComparison.OrdinalIgnoreCase))
        {
            return "in-progress";
        }

        return null;
    }

    private static string? DetermineRestSiteSelectionOutcomeEvidence(
        RestSiteButtonObservation buttonObservation,
        RestSiteUpgradeObservation upgradeObservation,
        string? lastSignal)
    {
        if (string.Equals(lastSignal, "after-select-failure", StringComparison.OrdinalIgnoreCase))
        {
            return "hook:after-select-failure";
        }

        if (upgradeObservation.ConfirmVisible)
        {
            return "runtime-poll:smith-confirm";
        }

        if (upgradeObservation.VisibleCards.Count > 0)
        {
            return "runtime-poll:smith-grid";
        }

        if (upgradeObservation.ObserverMiss)
        {
            return "runtime-screen:upgrade-without-exported-hitboxes";
        }

        if (string.Equals(lastSignal, "after-select-success", StringComparison.OrdinalIgnoreCase))
        {
            return "hook:after-select-success";
        }

        if (!string.IsNullOrWhiteSpace(buttonObservation.ExecutingOptionId))
        {
            return "runtime-poll:executing-option";
        }

        if (string.Equals(lastSignal, "before-select", StringComparison.OrdinalIgnoreCase))
        {
            return "hook:before-select";
        }

        return null;
    }

    private static IEnumerable<object> FindRoots(IEnumerable<object> roots, params string[] memberNames)
    {
        var results = new List<object>();
        foreach (var root in roots)
        {
            AddIfUseful(results, root);
            foreach (var memberName in memberNames)
            {
                AddIfUseful(results, TryGetMemberValue(root, memberName));
            }
        }

        return results;
    }

    private static IEnumerable<object> FindEnumerableItems(IEnumerable<object> roots, params string[] memberNames)
    {
        foreach (var root in roots)
        {
            foreach (var memberName in memberNames)
            {
                var candidate = TryGetMemberValue(root, memberName);
                if (candidate is IEnumerable<object> objectEnumerable)
                {
                    foreach (var item in objectEnumerable)
                    {
                        if (item is not null)
                        {
                            yield return item;
                        }
                    }

                    continue;
                }

                if (candidate is System.Collections.IEnumerable enumerable and not string)
                {
                    foreach (var item in enumerable)
                    {
                        if (item is not null)
                        {
                            yield return item;
                        }
                    }
                }
            }
        }
    }

    private static string? InferRunStatus(string semanticKind, LiveExportEncounterSummary? encounter, string? screen)
    {
        if (string.Equals(screen, "main-menu", StringComparison.Ordinal)
            || string.Equals(screen, "modding", StringComparison.Ordinal)
            || string.Equals(screen, "bootstrap", StringComparison.Ordinal))
        {
            return "idle";
        }

        if (string.Equals(screen, "rewards", StringComparison.Ordinal)
            || string.Equals(screen, "event", StringComparison.Ordinal)
            || string.Equals(screen, "rest-site", StringComparison.Ordinal)
            || string.Equals(screen, "upgrade", StringComparison.Ordinal)
            || string.Equals(screen, "transform", StringComparison.Ordinal)
            || string.Equals(screen, "card-choice", StringComparison.Ordinal)
            || string.Equals(screen, "shop", StringComparison.Ordinal)
            || string.Equals(screen, "map", StringComparison.Ordinal)
            || string.Equals(screen, "character-select", StringComparison.Ordinal))
        {
            return "active";
        }

        return semanticKind switch
        {
            "run-ended" => "ended",
            "combat-started" => "combat",
            "combat-ended" => "active",
            "run-started" => "active",
            "run-loaded" => "active",
            "runtime-poll" when encounter?.InCombat == true => "combat",
            _ => encounter?.InCombat == true ? "combat" : null,
        };
    }

    private static bool IsSemanticHighValueScreen(string triggerKind, string? screen)
    {
        if (triggerKind.Contains("reward", StringComparison.OrdinalIgnoreCase)
            || triggerKind.Contains("event", StringComparison.OrdinalIgnoreCase)
            || triggerKind.Contains("shop", StringComparison.OrdinalIgnoreCase)
            || triggerKind.Contains("rest", StringComparison.OrdinalIgnoreCase)
            || triggerKind.Contains("choice", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return screen is "rewards" or "event" or "shop" or "rest-site" or "card-choice" or "upgrade" or "transform";
    }

    private static string InferChoiceKind(string? typeName, string? rewardTypeName = null)
    {
        if (!string.IsNullOrWhiteSpace(rewardTypeName))
        {
            if (rewardTypeName.Contains("CardReward", StringComparison.OrdinalIgnoreCase))
            {
                return "card";
            }

            if (rewardTypeName.Contains("RelicReward", StringComparison.OrdinalIgnoreCase))
            {
                return "relic";
            }

            if (rewardTypeName.Contains("PotionReward", StringComparison.OrdinalIgnoreCase))
            {
                return "potion";
            }

            if (rewardTypeName.Contains("GoldReward", StringComparison.OrdinalIgnoreCase))
            {
                return "gold";
            }
        }

        if (string.IsNullOrWhiteSpace(typeName))
        {
            return "choice";
        }

        if (typeName.Contains("Card", StringComparison.OrdinalIgnoreCase))
        {
            return "card";
        }

        if (typeName.Contains("Relic", StringComparison.OrdinalIgnoreCase))
        {
            return "relic";
        }

        if (typeName.Contains("Potion", StringComparison.OrdinalIgnoreCase))
        {
            return "potion";
        }

        return "choice";
    }

    private static string InferScreen(params string?[] candidates)
    {
        var joined = string.Join(" ", candidates.Where(candidate => !string.IsNullOrWhiteSpace(candidate)));
        if (joined.Contains("CharacterSelect", StringComparison.OrdinalIgnoreCase)
            || joined.Contains("NewRun", StringComparison.OrdinalIgnoreCase))
        {
            return "character-select";
        }

        if (joined.Contains("SingleplayerSubmenu", StringComparison.OrdinalIgnoreCase))
        {
            return "singleplayer-submenu";
        }

        if (joined.Contains("MainMenu", StringComparison.OrdinalIgnoreCase))
        {
            return "main-menu";
        }

        if (joined.Contains("Title", StringComparison.OrdinalIgnoreCase)
            || joined.Contains("FrontEnd", StringComparison.OrdinalIgnoreCase))
        {
            return "main-menu";
        }

        if (joined.Contains("Modding", StringComparison.OrdinalIgnoreCase))
        {
            return "modding";
        }

        if (joined.Contains("DeckUpgradeSelect", StringComparison.OrdinalIgnoreCase)
            || joined.Contains("UpgradeSelect", StringComparison.OrdinalIgnoreCase))
        {
            return "upgrade";
        }

        if (joined.Contains("TransformSelect", StringComparison.OrdinalIgnoreCase)
            || joined.Contains("DeckTransform", StringComparison.OrdinalIgnoreCase))
        {
            return "transform";
        }

        if (joined.Contains("ChooseACardSelection", StringComparison.OrdinalIgnoreCase))
        {
            return "card-choice";
        }

        if (joined.Contains("Event", StringComparison.OrdinalIgnoreCase))
        {
            return "event";
        }

        if (joined.Contains("Reward", StringComparison.OrdinalIgnoreCase))
        {
            return "rewards";
        }

        if (joined.Contains("Rest", StringComparison.OrdinalIgnoreCase))
        {
            return "rest-site";
        }

        if (joined.Contains("Shop", StringComparison.OrdinalIgnoreCase) || joined.Contains("Merchant", StringComparison.OrdinalIgnoreCase))
        {
            return "shop";
        }

        if (joined.Contains("Map", StringComparison.OrdinalIgnoreCase))
        {
            return "map";
        }

        if (joined.Contains("Combat", StringComparison.OrdinalIgnoreCase))
        {
            return "combat";
        }

        return "unknown";
    }

    private static string ResolveScreen(string? screenOverride, IReadOnlyList<object> roots, params string?[] screenCandidates)
    {
        if (!string.IsNullOrWhiteSpace(screenOverride))
        {
            return screenOverride;
        }

        var sceneScreen = InferScreen(screenCandidates);
        if (!string.Equals(sceneScreen, "unknown", StringComparison.Ordinal))
        {
            return sceneScreen;
        }

        var explicitScreen = InferScreen(
            TryReadString(roots, "CurrentScreen"),
            TryReadString(roots, "Screen"),
            TryReadString(roots, "ScreenName"));
        if (!string.Equals(explicitScreen, "unknown", StringComparison.Ordinal))
        {
            return explicitScreen;
        }

        var roomScreen = InferScreen(TryReadString(roots, "RoomType"));
        if (!string.Equals(roomScreen, "unknown", StringComparison.Ordinal))
        {
            return roomScreen;
        }

        return "unknown";
    }

    private static string? TryReadString(IEnumerable<object> roots, params string[] memberNames)
    {
        foreach (var root in roots)
        {
            var value = TryReadString(root, memberNames);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string? TryReadString(object? root, params string[] memberNames)
    {
        var value = TryReadValue(root, memberNames);
        return TryConvertToDisplayString(value);
    }

    private static int? TryReadInt(IEnumerable<object> roots, params string[] memberNames)
    {
        foreach (var root in roots)
        {
            var value = TryReadInt(root, memberNames);
            if (value is not null)
            {
                return value;
            }
        }

        return null;
    }

    private static int? TryReadInt(object? root, params string[] memberNames)
    {
        var value = TryReadValue(root, memberNames);
        return value switch
        {
            byte byteValue => byteValue,
            short shortValue => shortValue,
            int intValue => intValue,
            long longValue when longValue is <= int.MaxValue and >= int.MinValue => (int)longValue,
            string stringValue when int.TryParse(stringValue, out var parsed) => parsed,
            _ => null,
        };
    }

    private static bool? TryReadBool(IEnumerable<object> roots, params string[] memberNames)
    {
        foreach (var root in roots)
        {
            var value = TryReadBool(root, memberNames);
            if (value is not null)
            {
                return value;
            }
        }

        return null;
    }

    private static bool? TryReadBool(object? root, params string[] memberNames)
    {
        var value = TryReadValue(root, memberNames);
        return value switch
        {
            bool boolValue => boolValue,
            string stringValue when bool.TryParse(stringValue, out var parsed) => parsed,
            _ => null,
        };
    }

    private static double? TryReadDouble(object? root, params string[] memberNames)
    {
        var value = TryReadValue(root, memberNames);
        return TryConvertToDouble(value);
    }

    private static object? TryReadValue(object? root, params string[] memberNames)
    {
        if (root is null)
        {
            return null;
        }

        foreach (var memberName in memberNames)
        {
            var directValue = TryGetMemberValue(root, memberName);
            if (directValue is not null)
            {
                return directValue;
            }
        }

        foreach (var nestedRootName in NestedRootNames)
        {
            var nestedRoot = TryGetMemberValue(root, nestedRootName);
            if (nestedRoot is null)
            {
                continue;
            }

            foreach (var memberName in memberNames)
            {
                var nestedValue = TryGetMemberValue(nestedRoot, memberName);
                if (nestedValue is not null)
                {
                    return nestedValue;
                }
            }
        }

        return null;
    }

    private static object? TryGetMemberValue(object source, string memberName)
    {
        try
        {
            var type = source as Type ?? source.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var property = type.GetProperties(flags)
                .FirstOrDefault(candidate => MemberNameMatches(candidate.Name, memberName));
            if (property is not null && property.GetIndexParameters().Length == 0)
            {
                return property.GetValue(source is Type ? null : source);
            }

            var field = type.GetFields(flags)
                .FirstOrDefault(candidate => MemberNameMatches(candidate.Name, memberName));
            if (field is not null)
            {
                return field.GetValue(source is Type ? null : source);
            }
        }
        catch
        {
            // Ignore live reflection failures inside the game process.
        }

        return null;
    }

    private static double? TryConvertToDouble(object? value)
    {
        if (value is null)
        {
            return null;
        }

        return value switch
        {
            byte number => number,
            sbyte number => number,
            short number => number,
            ushort number => number,
            int number => number,
            uint number => number,
            long number => number,
            ulong number => number,
            float number => number,
            double number => number,
            decimal number => (double)number,
            string stringValue when double.TryParse(
                stringValue,
                NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture,
                out var parsed) => parsed,
            _ => null,
        };
    }

    private static bool MemberNameMatches(string candidateName, string requestedName)
    {
        static string Normalize(string value)
        {
            return value.TrimStart('_');
        }

        return string.Equals(candidateName, requestedName, StringComparison.OrdinalIgnoreCase)
               || string.Equals(Normalize(candidateName), Normalize(requestedName), StringComparison.OrdinalIgnoreCase);
    }

    private static object? TryInvokeMethod(object source, string methodName, params object?[]? args)
    {
        try
        {
            var type = source as Type ?? source.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var providedArguments = args ?? Array.Empty<object?>();
            var methods = type.GetMethods(flags)
                .Where(candidate => string.Equals(candidate.Name, methodName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(candidate => candidate.GetParameters().Length)
                .ToArray();

            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length < providedArguments.Length)
                {
                    continue;
                }

                if (parameters.Skip(providedArguments.Length).Any(parameter => !parameter.IsOptional))
                {
                    continue;
                }

                var invocationArguments = new object?[parameters.Length];
                var compatible = true;
                for (var index = 0; index < providedArguments.Length; index += 1)
                {
                    var argument = providedArguments[index];
                    if (!CanAssignArgument(parameters[index].ParameterType, argument))
                    {
                        compatible = false;
                        break;
                    }

                    invocationArguments[index] = argument;
                }

                if (!compatible)
                {
                    continue;
                }

                for (var index = providedArguments.Length; index < parameters.Length; index += 1)
                {
                    invocationArguments[index] = GetDefaultArgumentValue(parameters[index]);
                }

                return method.Invoke(source is Type ? null : source, invocationArguments);
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static bool CanAssignArgument(Type parameterType, object? argument)
    {
        if (argument is null)
        {
            return !parameterType.IsValueType || Nullable.GetUnderlyingType(parameterType) is not null;
        }

        return parameterType.IsInstanceOfType(argument);
    }

    private static object? GetDefaultArgumentValue(ParameterInfo parameter)
    {
        if (parameter.HasDefaultValue)
        {
            return parameter.DefaultValue is DBNull
                ? GetUnsetValue(parameter.ParameterType)
                : parameter.DefaultValue;
        }

        return GetUnsetValue(parameter.ParameterType);
    }

    private static object? GetUnsetValue(Type type)
    {
        return type.IsValueType
            ? Activator.CreateInstance(type)
            : null;
    }

    private static int? TryConvertToInt(object? value)
    {
        return value switch
        {
            byte byteValue => byteValue,
            short shortValue => shortValue,
            int intValue => intValue,
            long longValue when longValue is <= int.MaxValue and >= int.MinValue => (int)longValue,
            string stringValue when int.TryParse(stringValue, out var parsed) => parsed,
            _ => null,
        };
    }

    private static bool? TryConvertToBool(object? value)
    {
        return value switch
        {
            bool boolValue => boolValue,
            string stringValue when bool.TryParse(stringValue, out var parsed) => parsed,
            _ => null,
        };
    }

    private static void AddIfUseful(ICollection<object> target, object? candidate)
    {
        if (candidate is null or string)
        {
            return;
        }

        if (candidate is System.Collections.IEnumerable and not Type)
        {
            return;
        }

        var key = RuntimeHelpers.GetHashCode(candidate);
        if (target.Any(existing => RuntimeHelpers.GetHashCode(existing) == key))
        {
            return;
        }

        target.Add(candidate);
    }

    private static void AddChoiceCandidate(ICollection<object> target, object? candidate, int maxDepth)
    {
        if (candidate is null)
        {
            return;
        }

        var queue = new Queue<(object Item, int Depth)>();
        var seen = new HashSet<int>();
        queue.Enqueue((candidate, 0));

        while (queue.Count > 0)
        {
            var (item, depth) = queue.Dequeue();
            if (!seen.Add(RuntimeHelpers.GetHashCode(item)))
            {
                continue;
            }

            AddIfUseful(target, item);
            if (depth >= maxDepth)
            {
                continue;
            }

            if (LooksLikePreviewLayoutOrContainerType(item.GetType().FullName ?? item.GetType().Name)
                && !HasMeaningfulChoiceData(item))
            {
                continue;
            }

            foreach (var memberName in new[]
                     {
                         "Title",
                         "Label",
                     })
            {
                var nested = TryGetMemberValue(item, memberName);
                if (nested is null)
                {
                    continue;
                }

                foreach (var expanded in ExpandEnumerable(nested).Prepend(nested))
                {
                    if (expanded is not null and not string)
                    {
                        queue.Enqueue((expanded, depth + 1));
                    }
                }
            }

            foreach (var memberName in ChoiceMemberNames)
            {
                var nested = TryGetMemberValue(item, memberName);
                if (nested is null)
                {
                    continue;
                }

                foreach (var expanded in ExpandEnumerable(nested).Prepend(nested))
                {
                    if (expanded is not null and not string)
                    {
                        queue.Enqueue((expanded, depth + 1));
                    }
                }
            }

            foreach (var child in TryEnumerateChildren(item).Take(64))
            {
                queue.Enqueue((child, depth + 1));
            }
        }
    }

    private static LiveExportChoiceSummary? TryCreateChoiceSummary(object item)
    {
        if (!LooksLikeChoiceCandidate(item))
        {
            return null;
        }

        var rewardTypeName = TryResolveRewardTypeName(item);
        var label = TryResolveChoiceLabel(item);
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        var eventOptionBindingId = TryResolveEventOptionBindingId(item);
        var eventOptionChoice = !string.IsNullOrWhiteSpace(eventOptionBindingId);
        var eventOptionEnabled = eventOptionChoice
            ? TryResolveEventOptionEnabled(item)
            : null;
        var choiceKind = eventOptionChoice
            ? "event-option"
            : InferChoiceKind(item.GetType().FullName, rewardTypeName);
        var bindingKind = eventOptionChoice
            ? "event-option"
            : rewardTypeName is null ? null : "reward-type";
        var bindingId = eventOptionChoice
            ? eventOptionBindingId
            : rewardTypeName is null ? null : GetRewardTypeId(rewardTypeName);
        var nodeId = eventOptionChoice
            ? TryResolveEventOptionNodeId(item, eventOptionBindingId!, label)
            : null;

        return new LiveExportChoiceSummary(
            choiceKind,
            label,
            TryResolveChoiceValue(item, rewardTypeName),
            TryResolveChoiceDescription(item))
        {
            NodeId = nodeId,
            ScreenBounds = TryResolveScreenBounds(item),
            BindingKind = bindingKind,
            BindingId = bindingId,
            Enabled = eventOptionEnabled,
            SemanticHints = BuildChoiceSemanticHints(item, rewardTypeName),
        };
    }

    private static void AppendChoiceExtractionMetadata(
        ChoiceExtractionResult choiceResult,
        IDictionary<string, string?> meta,
        IDictionary<string, object?> payload)
    {
        foreach (var entry in choiceResult.Meta)
        {
            if (string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Value))
            {
                continue;
            }

            meta[entry.Key] = entry.Value;
            payload[entry.Key] = entry.Value;
        }
    }

    private static bool LooksLikeMapContext(
        string triggerKind,
        string? screenHint,
        IEnumerable<object> choiceRoots,
        string? currentActiveScreenType,
        RootSceneTransitionObservation rootSceneObservation)
    {
        if (string.Equals(screenHint, "map", StringComparison.OrdinalIgnoreCase)
            || string.Equals(triggerKind, "map", StringComparison.OrdinalIgnoreCase)
            || string.Equals(triggerKind, "map-point-selected", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (HasMapForegroundAuthority(currentActiveScreenType, rootSceneObservation))
        {
            return true;
        }

        return choiceRoots.Any(root =>
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            return typeName.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase)
                   || IsMapPointType(root.GetType());
        });
    }

    private static bool LooksLikeMainMenuContext(string triggerKind, string? screenHint, IEnumerable<object> choiceRoots)
    {
        if (string.Equals(screenHint, "main-menu", StringComparison.OrdinalIgnoreCase)
            || string.Equals(triggerKind, "main-menu", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return choiceRoots.Any(root =>
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            return typeName.Contains("NMainMenu", StringComparison.OrdinalIgnoreCase)
                   || typeName.Contains("NMainMenuTextButton", StringComparison.OrdinalIgnoreCase)
                   || typeName.Contains("NMainMenuContinueButton", StringComparison.OrdinalIgnoreCase);
        });
    }

    private static ChoiceExtractionResult ExtractMainMenuChoices(IEnumerable<object> roots, int maxEntries)
    {
        var buttons = new List<object>();
        foreach (var root in roots)
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            if (typeName.Contains("NMainMenuTextButton", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("NMainMenuContinueButton", StringComparison.OrdinalIgnoreCase))
            {
                AddIfUseful(buttons, root);
            }

            if (!typeName.Contains("NMainMenu", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var memberName in new[]
                     {
                         "MainMenuButtons",
                         "_continueButton",
                         "_singleplayerButton",
                         "_multiplayerButton",
                         "_timelineButton",
                         "_settingsButton",
                         "_compendiumButton",
                         "_quitButton",
                         "_abandonRunButton",
                     })
            {
                var candidate = TryGetMemberValue(root, memberName);
                AddIfUseful(buttons, candidate);
                foreach (var item in ExpandEnumerable(candidate))
                {
                    AddIfUseful(buttons, item);
                }
            }
        }

        var choices = buttons
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .Select(TryCreateMainMenuChoiceSummary)
            .Where(choice => choice is not null)
            .Cast<LiveExportChoiceSummary>()
            .OrderBy(choice => choice.NodeId, StringComparer.Ordinal)
            .Take(maxEntries)
            .ToArray();

        var candidates = choices
            .Select(choice => new LiveExportChoiceCandidate(
                "main-menu",
                "NMainMenuTextButton",
                choice.Label,
                choice.Value,
                choice.Description,
                100,
                Accepted: true,
                RejectReason: null))
            .ToArray();

        return new ChoiceExtractionResult(
            choices,
            candidates,
            new LiveExportChoiceDecision(
                "main-menu",
                UsedStrictExtractor: true,
                CandidateCount: candidates.Length,
                AcceptedCount: choices.Length,
                choices.Length == 0 ? "none" : "accepted",
                choices.Length == 0 ? "no visible main menu buttons with bounds resolved" : null,
                Array.Empty<string>()));
    }

    private static LiveExportChoiceSummary? TryCreateMainMenuChoiceSummary(object item)
    {
        var typeName = item.GetType().FullName ?? item.GetType().Name;
        if (!typeName.Contains("NMainMenu", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (TryReadBool(item, "Visible", "IsVisible") == false)
        {
            return null;
        }

        if (TryReadBool(item, "IsEnabled", "Enabled", "Disabled") == false)
        {
            return null;
        }

        var screenBounds = TryResolveScreenBounds(item);
        if (string.IsNullOrWhiteSpace(screenBounds))
        {
            return null;
        }

        var label = TryResolveChoiceLabel(item);
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        var normalizedLabel = label.Trim();
        var kind = normalizedLabel.Contains("continue", StringComparison.OrdinalIgnoreCase)
                   || normalizedLabel.Contains("계속", StringComparison.OrdinalIgnoreCase)
            ? "continue-run"
            : "menu-action";
        var nodeId = normalizedLabel.ToLowerInvariant() switch
        {
            var text when text.Contains("continue", StringComparison.Ordinal) || text.Contains("계속", StringComparison.Ordinal) => "main-menu:continue",
            var text when text.Contains("single", StringComparison.Ordinal) || text.Contains("싱글", StringComparison.Ordinal) => "main-menu:singleplayer",
            var text when text.Contains("multi", StringComparison.Ordinal) || text.Contains("멀티", StringComparison.Ordinal) => "main-menu:multiplayer",
            var text when text.Contains("setting", StringComparison.Ordinal) || text.Contains("설정", StringComparison.Ordinal) => "main-menu:settings",
            var text when text.Contains("quit", StringComparison.Ordinal) || text.Contains("종료", StringComparison.Ordinal) => "main-menu:quit",
            _ => $"main-menu:{SanitizeNodeKey(normalizedLabel)}",
        };

        return new LiveExportChoiceSummary(
            kind,
            normalizedLabel,
            nodeId,
            typeName)
        {
            NodeId = nodeId,
            ScreenBounds = screenBounds,
        };
    }

    private static ChoiceExtractionResult ExtractMapChoices(IEnumerable<object> roots, int maxEntries)
    {
        var mapPoints = new List<object>();
        foreach (var root in roots)
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            if (IsMapPointType(root.GetType()))
            {
                AddIfUseful(mapPoints, root);
            }

            if (!typeName.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var dictionary = TryGetMemberValue(root, "_mapPointDictionary");
            var values = dictionary is null ? null : TryGetMemberValue(dictionary, "Values");
            foreach (var point in ExpandEnumerable(values))
            {
                AddIfUseful(mapPoints, point);
            }
        }

        var distinctMapPoints = mapPoints
            .DistinctBy(RuntimeHelpers.GetHashCode)
            .ToArray();
        var candidates = distinctMapPoints
            .Select(CreateMapChoiceCandidate)
            .ToArray();
        var choices = distinctMapPoints
            .Select(item => TryCreateMapChoiceSummary(item, out _))
            .Where(choice => choice is not null)
            .Cast<LiveExportChoiceSummary>()
            .OrderBy(choice => choice.NodeId, StringComparer.Ordinal)
            .Take(maxEntries)
            .ToArray();
        var rejectSummary = BuildMapPointRejectSummary(candidates);

        return new ChoiceExtractionResult(
            choices,
            candidates,
            new LiveExportChoiceDecision(
                "map",
                UsedStrictExtractor: true,
                CandidateCount: candidates.Length,
                AcceptedCount: choices.Length,
                choices.Length == 0 ? "none" : "accepted",
                choices.Length == 0
                    ? rejectSummary ?? "no reachable map points with bounds resolved"
                    : rejectSummary,
                Array.Empty<string>()));
    }

    private static LiveExportChoiceSummary? TryCreateMapChoiceSummary(object item)
    {
        return TryCreateMapChoiceSummary(item, out _);
    }

    private static LiveExportChoiceSummary? TryCreateMapChoiceSummary(object item, out string? rejectReason)
    {
        rejectReason = null;
        var typeName = item.GetType().FullName ?? item.GetType().Name;
        if (!IsMapPointType(item.GetType()))
        {
            rejectReason = "not-map-point";
            return null;
        }

        if (TryReadBool(item, "IsEnabled") != true)
        {
            rejectReason = "disabled";
            return null;
        }

        var screenBounds = TryResolveScreenBounds(item);
        if (string.IsNullOrWhiteSpace(screenBounds))
        {
            rejectReason = "missing-bounds";
            return null;
        }

        var point = TryGetMemberValue(item, "Point");
        var coord = point is null ? null : TryGetMemberValue(point, "coord");
        var row = TryReadInt(coord, "row", "Row");
        var col = TryReadInt(coord, "col", "Col");
        var pointType = TryReadString(point, "PointType", "Type", "RoomType");
        var state = TryReadString(item, "State");

        var label = pointType switch
        {
            { Length: > 0 } => row is not null && col is not null
                ? $"{pointType} ({row},{col})"
                : pointType,
            _ => row is not null && col is not null
                ? $"Map ({row},{col})"
                : "Map",
        };

        var descriptionParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(pointType))
        {
            descriptionParts.Add($"type:{pointType}");
        }

        if (!string.IsNullOrWhiteSpace(state))
        {
            descriptionParts.Add($"state:{state}");
        }

        if (row is not null && col is not null)
        {
            descriptionParts.Add($"coord:{row},{col}");
        }

        return new LiveExportChoiceSummary(
            "map-node",
            label,
            row is not null && col is not null ? $"{row},{col}" : null,
            descriptionParts.Count == 0 ? null : string.Join(";", descriptionParts))
        {
            NodeId = row is not null && col is not null
                ? $"map:{row}:{col}"
                : $"map:{RuntimeHelpers.GetHashCode(item)}",
            ScreenBounds = screenBounds,
        };
    }

    private static void AddMapAuthorityRoots(
        ICollection<object> choiceRoots,
        ICollection<object> candidateItems,
        IEnumerable<object> roots,
        object? currentActiveScreen,
        string? currentActiveScreenType,
        RootSceneTransitionObservation rootSceneObservation)
    {
        if (!HasMapForegroundAuthority(currentActiveScreenType, rootSceneObservation))
        {
            return;
        }

        foreach (var mapRoot in ResolveMapRoots(roots, currentActiveScreen))
        {
            AddIfUseful(choiceRoots, mapRoot);
            AddChoiceCandidate(candidateItems, mapRoot, maxDepth: 2);

            var typeName = mapRoot.GetType().FullName ?? mapRoot.GetType().Name;
            if (!IsMapScreenType(typeName))
            {
                continue;
            }

            var dictionary = TryGetMemberValue(mapRoot, "_mapPointDictionary");
            AddIfUseful(choiceRoots, dictionary);
            var values = dictionary is null ? null : TryGetMemberValue(dictionary, "Values");
            AddIfUseful(choiceRoots, values);
            foreach (var point in ExpandEnumerable(values))
            {
                AddIfUseful(choiceRoots, point);
                AddChoiceCandidate(candidateItems, point, maxDepth: 1);
            }
        }
    }

    private static IEnumerable<object> ResolveMapRoots(IEnumerable<object> roots, object? currentActiveScreen)
    {
        var mapRoots = new List<object>();
        AddIfUseful(mapRoots, currentActiveScreen);

        foreach (var root in roots)
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            if (IsMapScreenType(typeName))
            {
                AddIfUseful(mapRoots, root);
            }

            var mapScreen = TryGetMemberValue(root, "MapScreen")
                            ?? TryGetMemberValue(TryGetMemberValue(root, "GlobalUi"), "MapScreen")
                            ?? TryGetMemberValue(TryGetMemberValue(root, "GlobalUI"), "MapScreen")
                            ?? TryGetMemberValue(root, "_mapScreen");
            AddIfUseful(mapRoots, mapScreen);
        }

        var mapScreenType = FindLoadedType("MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen");
        if (mapScreenType is not null)
        {
            foreach (var singletonName in SingletonPropertyNames)
            {
                AddIfUseful(mapRoots, TryGetMemberValue(mapScreenType, singletonName));
            }
        }

        return mapRoots;
    }

    private static bool HasMapForegroundAuthority(string? currentActiveScreenType, RootSceneTransitionObservation rootSceneObservation)
    {
        return IsMapScreenType(currentActiveScreenType)
               || IsMapScreenType(rootSceneObservation.CurrentRunRoomSceneType)
               || IsMapScreenType(rootSceneObservation.RootSceneCurrentType);
    }

    private static LiveExportChoiceCandidate CreateMapChoiceCandidate(object item)
    {
        var typeName = item.GetType().FullName ?? item.GetType().Name;
        var summary = TryCreateMapChoiceSummary(item, out var rejectReason);
        return new LiveExportChoiceCandidate(
            "map",
            typeName,
            summary?.Label ?? TryResolveChoiceLabel(item) ?? typeName,
            summary?.Value,
            summary?.Description,
            summary is null ? 70 : 100,
            Accepted: summary is not null,
            RejectReason: rejectReason);
    }

    private static string? BuildMapPointRejectSummary(IReadOnlyList<LiveExportChoiceCandidate> candidates)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        if (candidates.Any(static candidate => candidate.Accepted))
        {
            return null;
        }

        var rejectCounts = candidates
            .Where(static candidate => !candidate.Accepted && !string.IsNullOrWhiteSpace(candidate.RejectReason))
            .GroupBy(candidate => candidate.RejectReason!, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(static group => group.Count())
            .ThenBy(static group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .Select(group => $"{group.Key}={group.Count()}")
            .ToArray();
        if (rejectCounts.Length == 0)
        {
            return $"map points seen={candidates.Count} but none were accepted";
        }

        return $"map points seen={candidates.Count} but filtered: {string.Join(",", rejectCounts)}";
    }

    private static bool IsMapAwareExtractorPath(string? extractorPath)
    {
        return !string.IsNullOrWhiteSpace(extractorPath)
               && extractorPath.Contains("map", StringComparison.OrdinalIgnoreCase);
    }

    private static string SanitizeNodeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
            else if (builder.Length == 0 || builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        return builder.ToString().Trim('-');
    }

    private static string? TryResolveScreenBounds(object item)
    {
        var globalRect = TryInvokeMethod(item, "GetGlobalRect");
        var position = (globalRect is null ? null : TryGetMemberValue(globalRect, "Position"))
            ?? TryGetMemberValue(item, "GlobalPosition")
            ?? TryGetMemberValue(item, "Position");
        var size = (globalRect is null ? null : TryGetMemberValue(globalRect, "Size"))
            ?? TryGetMemberValue(item, "Size");

        var x = TryReadDouble(position, "X", "x");
        var y = TryReadDouble(position, "Y", "y");
        var width = TryReadDouble(size, "X", "Width", "w");
        var height = TryReadDouble(size, "Y", "Height", "h");
        if (x is null || y is null || width is null || height is null)
        {
            return null;
        }

        if (width <= 0 || height <= 0)
        {
            return null;
        }

        return string.Create(CultureInfo.InvariantCulture, $"{x.Value:0.###},{y.Value:0.###},{width.Value:0.###},{height.Value:0.###}");
    }

    private static string? TryResolveInteractiveScreenBounds(object? item, out string boundsSource)
    {
        boundsSource = "control";
        if (item is null)
        {
            return null;
        }

        var interactiveControl = TryResolveInteractiveControl(item, out boundsSource);
        if (interactiveControl is not null)
        {
            var interactiveBounds = TryResolveScreenBounds(interactiveControl);
            if (!string.IsNullOrWhiteSpace(interactiveBounds))
            {
                return interactiveBounds;
            }
        }

        boundsSource = "holder";
        return TryResolveScreenBounds(item);
    }

    private static bool? TryResolveInteractiveEnabled(object? item)
    {
        if (item is null)
        {
            return null;
        }

        var interactiveControl = TryResolveInteractiveControl(item, out _);
        return TryResolveControlEnabled(interactiveControl)
               ?? TryResolveControlEnabled(item)
               ?? TryReadBool(item, "_isClickable");
    }

    private static object? TryResolveInteractiveControl(object? item, out string boundsSource)
    {
        boundsSource = "control";
        if (item is null)
        {
            return null;
        }

        var hitbox = TryGetMemberValue(item, "Hitbox") ?? TryGetMemberValue(item, "_hitbox");
        if (hitbox is not null)
        {
            boundsSource = "grid-hitbox";
            return hitbox;
        }

        var typeName = item.GetType().FullName ?? item.GetType().Name;
        if (typeName.Contains("NConfirmButton", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("NClickableControl", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("NButton", StringComparison.OrdinalIgnoreCase))
        {
            boundsSource = "confirm-button";
        }

        return item;
    }

    private static bool LooksLikeChoiceCandidate(object item)
    {
        var typeName = item.GetType().FullName ?? item.GetType().Name;
        if (LooksLikePreviewLayoutOrContainerType(typeName))
        {
            return HasMeaningfulChoiceData(item) && LooksLikeStrictChoiceRoot(item);
        }

        if (LooksLikeStrictChoiceRoot(item))
        {
            return true;
        }

        var label = TryResolveChoiceLabel(item);
        return !string.IsNullOrWhiteSpace(label)
               && !IsPlaceholderChoiceLabel(label, item)
               && (typeName.Contains("Choice", StringComparison.OrdinalIgnoreCase)
                   || typeName.Contains("Button", StringComparison.OrdinalIgnoreCase)
                   || typeName.Contains("Option", StringComparison.OrdinalIgnoreCase));
    }

    private static string? TryResolveChoiceLabel(object item)
    {
        var label = FirstNonEmpty(
            TryReadString(TryGetMemberValue(item, "Card"), "Title", "CardName", "DisplayName", "Name", "Id", "CardId"),
            TryReadString(TryGetMemberValue(item, "Model"), "Title", "CardName", "DisplayName", "Name", "Id", "CardId"),
            TryReadString(TryGetMemberValue(item, "CardNode"), "Title", "CardName", "DisplayName", "Name", "Id", "CardId"),
            TryReadString(TryGetMemberValue(item, "CardModel"), "Title", "CardName", "DisplayName", "Name", "Id", "CardId"),
            TryReadString(TryGetMemberValue(item, "Entry"), "DisplayName", "Title", "Name"),
            TryReadString(TryGetMemberValue(item, "Option"), "Title", "Label", "Name", "Description"),
            TryReadString(TryGetMemberValue(item, "Reward"), "Description", "Title", "Label", "Name"),
            TryReadString(TryGetMemberValue(item, "_label"), "Text", "Value"),
            TryReadString(TryGetMemberValue(item, "_titleLabel"), "Text", "Value"),
            TryReadString(item, "DisplayName", "CardName", "OptionName", "Title", "Label", "Text", "Name"),
            TryReadString(TryGetMemberValue(item, "Title"), "Text", "Value"),
            TryReadString(TryGetMemberValue(item, "Label"), "Text", "Value"),
            TryResolveNestedChoiceText(item, maxDepth: 2),
            TryReadString(item, "Id", "CardId"));
        return IsPlaceholderChoiceLabel(label, item) ? null : label;
    }

    private static string? TryResolveChoiceValue(object item, string? rewardTypeName = null)
    {
        return FirstNonEmpty(
            TryReadString(item, "Value", "Id", "CardId", "Hotkey"),
            TryReadString(TryGetMemberValue(item, "Option"), "Id", "Name", "Title"),
            TryReadString(TryGetMemberValue(item, "Reward"), "Id", "Name", "Description"),
            TryReadString(TryGetMemberValue(item, "Entry"), "Id", "Name"),
            TryReadString(TryGetMemberValue(item, "Card"), "Id", "CardId", "Name"),
            TryReadString(TryGetMemberValue(item, "Model"), "Id", "CardId", "Name"),
            rewardTypeName is null ? null : GetRewardTypeId(rewardTypeName));
    }

    private static string? TryResolveChoiceDescription(object item)
    {
        return FirstNonEmpty(
            TryReadString(TryGetMemberValue(item, "_label"), "Text", "Value"),
            TryReadString(TryGetMemberValue(item, "_descriptionLabel"), "Text", "Value"),
            TryReadString(item, "Description", "Tooltip", "Body"),
            TryReadString(TryGetMemberValue(item, "Option"), "Description", "Body"),
            TryReadString(TryGetMemberValue(item, "Reward"), "Description", "Body"),
            TryReadString(TryGetMemberValue(item, "Entry"), "DisplayName", "Description", "Body"),
            TryReadString(TryGetMemberValue(item, "Card"), "Description", "Body"),
            TryReadString(TryGetMemberValue(item, "Model"), "Description", "Body"),
            TryResolveNestedChoiceText(item, maxDepth: 3));
    }

    private static string? TryResolveNestedChoiceText(object? root, int maxDepth)
    {
        if (root is null || maxDepth < 0)
        {
            return null;
        }

        var queue = new Queue<(object Item, int Depth)>();
        var seen = new HashSet<int>();
        queue.Enqueue((root, 0));

        while (queue.Count > 0)
        {
            var (item, depth) = queue.Dequeue();
            if (!seen.Add(RuntimeHelpers.GetHashCode(item)))
            {
                continue;
            }

            if (depth > 0 && LooksLikePreviewLayoutOrContainerType(item.GetType().FullName ?? item.GetType().Name))
            {
                continue;
            }

            foreach (var memberName in new[]
                     {
                         "Text",
                         "BbcodeText",
                         "DisplayedText",
                         "TooltipText",
                         "Description",
                         "Body",
                         "Label",
                         "Title",
                         "Value",
                         "Name",
                     })
            {
                var rendered = TryConvertToDisplayString(TryGetMemberValue(item, memberName));
                if (!string.IsNullOrWhiteSpace(rendered))
                {
                    return rendered;
                }
            }

            if (depth >= maxDepth)
            {
                continue;
            }

            foreach (var memberName in new[] { "Option", "Reward", "Entry", "Card", "Model", "Label", "Title" })
            {
                var nested = TryGetMemberValue(item, memberName);
                if (nested is not null and not string)
                {
                    queue.Enqueue((nested, depth + 1));
                }
            }

            foreach (var child in TryEnumerateChildren(item).Take(64))
            {
                queue.Enqueue((child, depth + 1));
            }
        }

        return null;
    }

    private static bool HasMeaningfulChoiceData(object item)
    {
        return TryGetMemberValue(item, "Option") is not null
               || TryGetMemberValue(item, "Reward") is not null
               || TryGetMemberValue(item, "Entry") is not null
               || TryGetMemberValue(item, "Card") is not null
               || TryGetMemberValue(item, "Model") is not null
               || TryGetMemberValue(item, "CardNode") is not null
               || TryGetMemberValue(item, "CardModel") is not null
               || TryGetMemberValue(item, "_label") is not null
               || TryGetMemberValue(item, "_titleLabel") is not null
               || TryGetMemberValue(item, "_descriptionLabel") is not null
               || TryGetMemberValue(item, "Price") is not null;
    }

    private static bool LooksLikePlaceholderChoice(LiveExportChoiceSummary summary, int score)
    {
        if (score >= 6)
        {
            return false;
        }

        if (IsRewardCardChoiceSummary(summary))
        {
            return false;
        }

        if (PlaceholderChoiceLabels.Contains(summary.Label))
        {
            return true;
        }

        if (summary.Label.IndexOf(' ') < 0
            && summary.Label.IndexOfAny("._-".ToCharArray()) < 0
            && (summary.Label.EndsWith("Button", StringComparison.Ordinal)
                || summary.Label.EndsWith("Container", StringComparison.Ordinal)
                || summary.Label.EndsWith("Inventory", StringComparison.Ordinal)))
        {
            return true;
        }

        if (summary.Label.StartsWith("@Control@", StringComparison.Ordinal)
            || summary.Label.StartsWith("@Node", StringComparison.Ordinal)
            || summary.Label.StartsWith("@Node2D@", StringComparison.Ordinal))
        {
            return true;
        }

        if (summary.Label.StartsWith("MerchantCardHolder", StringComparison.Ordinal)
            || summary.Label.StartsWith("MerchantRelicHolder", StringComparison.Ordinal)
            || summary.Label.StartsWith("MerchantPotionHolder", StringComparison.Ordinal))
        {
            return true;
        }

        if (summary.Label.EndsWith("선택하세요.", StringComparison.Ordinal)
            || summary.Label.EndsWith("선택해 주세요.", StringComparison.Ordinal))
        {
            return true;
        }

        if (summary.Label.Equals("PlayQueue", StringComparison.Ordinal)
            || summary.Label.Equals("CardTrailIronclad", StringComparison.Ordinal)
            || summary.Label.Equals("RewardsScreen", StringComparison.Ordinal)
            || summary.Label.Equals("NCardRewardSelectionScreen", StringComparison.Ordinal)
            || summary.Label.Equals("Hitbox", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private static int ScoreChoiceCandidate(object item)
    {
        var score = 0;
        var typeName = item.GetType().FullName ?? item.GetType().Name;
        var label = TryResolveChoiceLabel(item);
        var description = TryResolveChoiceDescription(item);
        var value = TryResolveChoiceValue(item);

        if (typeName.Contains("CardCreationResult", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("CardModel", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("NGridCardHolder", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("NCardReward", StringComparison.OrdinalIgnoreCase))
        {
            score += 8;
        }

        if (typeName.Contains("RewardButton", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Reward", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("MerchantSlot", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("MerchantCard", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("MerchantRelic", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("MerchantPotion", StringComparison.OrdinalIgnoreCase))
        {
            score += 6;
        }

        if (TryGetMemberValue(item, "Card") is not null
            || TryGetMemberValue(item, "Reward") is not null
            || TryGetMemberValue(item, "Entry") is not null
            || TryGetMemberValue(item, "Option") is not null
            || TryGetMemberValue(item, "Model") is not null
            || TryGetMemberValue(item, "_label") is not null)
        {
            score += 5;
        }

        if (!string.IsNullOrWhiteSpace(label))
        {
            score += 1;
            if (label.Any(character => char.IsWhiteSpace(character) || character > 127))
            {
                score += 2;
            }
        }

        if (!string.IsNullOrWhiteSpace(description) && !string.Equals(description, label, StringComparison.Ordinal))
        {
            score += 2;
        }

        if (!string.IsNullOrWhiteSpace(value) && !string.Equals(value, label, StringComparison.Ordinal))
        {
            score += 1;
        }

        if (LooksLikePreviewLayoutOrContainerType(typeName))
        {
            score -= 8;
        }

        if (typeName.Contains("Screen", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Layout", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Holder", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("BackButton", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("PlayQueue", StringComparison.OrdinalIgnoreCase))
        {
            score -= 10;
        }

        if (!string.IsNullOrWhiteSpace(label)
            && (label.StartsWith("@Control@", StringComparison.Ordinal)
                || label.StartsWith("@Node", StringComparison.Ordinal)
                || label.EndsWith("선택하세요.", StringComparison.Ordinal)
                || label.EndsWith("선택해 주세요.", StringComparison.Ordinal)))
        {
            score -= 8;
        }

        return score;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static bool LooksLikeStrictChoiceRoot(object? item)
    {
        if (item is null or string)
        {
            return false;
        }

        var typeName = item.GetType().FullName ?? item.GetType().Name;
        if (LooksLikePreviewLayoutOrContainerType(typeName))
        {
            return HasMeaningfulChoiceData(item)
                   && (TryGetMemberValue(item, "Card") is not null
                       || TryGetMemberValue(item, "Reward") is not null
                       || TryGetMemberValue(item, "Entry") is not null
                       || TryGetMemberValue(item, "Option") is not null
                       || TryGetMemberValue(item, "Model") is not null);
        }

        return typeName.Contains("RewardButton", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("CardCreationResult", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("MerchantSlot", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("MerchantCard", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("MerchantRelic", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("MerchantPotion", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("ProceedButton", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("NCard", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("EndTurnButton", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("PingButton", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("DrawPileButton", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("DiscardPileButton", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("ExhaustPileButton", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Option", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Reward", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Entry", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Choice", StringComparison.OrdinalIgnoreCase)
               || TryGetMemberValue(item, "Card") is not null
               || TryGetMemberValue(item, "Reward") is not null
               || TryGetMemberValue(item, "Entry") is not null
               || TryGetMemberValue(item, "Option") is not null
               || TryGetMemberValue(item, "Model") is not null;
    }

    private static bool LooksLikePreviewLayoutOrContainerType(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return false;
        }

        return typeName.Contains("Preview", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Container", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Layout", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Inventory", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Reaction", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Hitbox", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Holder", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Cursor", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Trail", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("PlayQueue", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlaceholderChoiceLabel(string? label, object? item = null)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        if (ShouldPreserveRewardPlaceholderLabel(item, label))
        {
            return false;
        }

        if (PlaceholderChoiceLabels.Contains(label))
        {
            return true;
        }

        if (label.IndexOf(' ') < 0
            && label.IndexOfAny("._-".ToCharArray()) < 0
            && (label.EndsWith("Button", StringComparison.Ordinal)
                || label.EndsWith("Container", StringComparison.Ordinal)
                || label.EndsWith("Inventory", StringComparison.Ordinal)
                || label.EndsWith("Holder", StringComparison.Ordinal)
                || label.EndsWith("Layout", StringComparison.Ordinal)
                || label.EndsWith("Screen", StringComparison.Ordinal)))
        {
            return true;
        }

        if (label.StartsWith("@Control@", StringComparison.Ordinal)
            || label.StartsWith("@Node", StringComparison.Ordinal)
            || label.StartsWith("@Node2D@", StringComparison.Ordinal)
            || label.StartsWith("MerchantCardHolder", StringComparison.Ordinal)
            || label.StartsWith("MerchantRelicHolder", StringComparison.Ordinal)
            || label.StartsWith("MerchantPotionHolder", StringComparison.Ordinal))
        {
            return true;
        }

        if (label.EndsWith("선택하세요.", StringComparison.Ordinal)
            || label.EndsWith("선택해 주세요.", StringComparison.Ordinal)
            || label.Equals("PlayQueue", StringComparison.Ordinal)
            || label.Equals("CardTrailIronclad", StringComparison.Ordinal)
            || label.Equals("RewardsScreen", StringComparison.Ordinal)
            || label.Equals("NCardRewardSelectionScreen", StringComparison.Ordinal)
            || label.Equals("Hitbox", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private static string? TryResolveRewardTypeName(object? item)
    {
        if (item is null)
        {
            return null;
        }

        var reward = TryGetMemberValue(item, "Reward");
        if (reward is not null)
        {
            return reward.GetType().FullName ?? reward.GetType().Name;
        }

        var typeName = item.GetType().FullName ?? item.GetType().Name;
        return typeName.Contains("Reward", StringComparison.OrdinalIgnoreCase)
               && !typeName.Contains("RewardButton", StringComparison.OrdinalIgnoreCase)
               ? typeName
               : null;
    }

    private static bool ShouldPreserveRewardPlaceholderLabel(object? item, string label)
    {
        if (item is null
            || (!label.EndsWith("선택하세요.", StringComparison.Ordinal)
                && !label.EndsWith("선택해 주세요.", StringComparison.Ordinal)))
        {
            return false;
        }

        var rewardTypeName = TryResolveRewardTypeName(item);
        return rewardTypeName?.Contains("CardReward", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool IsRewardCardChoiceSummary(LiveExportChoiceSummary summary)
    {
        return string.Equals(summary.Kind, "card", StringComparison.OrdinalIgnoreCase)
               && string.Equals(summary.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
               && string.Equals(summary.BindingId, "CardReward", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<string> BuildChoiceSemanticHints(object item, string? rewardTypeName)
    {
        var hints = new List<string>();
        if (!string.IsNullOrWhiteSpace(rewardTypeName))
        {
            var rewardTypeId = GetRewardTypeId(rewardTypeName);
            hints.Add("reward");
            if (rewardTypeName.Contains("CardReward", StringComparison.OrdinalIgnoreCase))
            {
                hints.Add("reward-card");
            }
            else if (rewardTypeName.Contains("PotionReward", StringComparison.OrdinalIgnoreCase))
            {
                hints.Add("reward-potion");
            }
            else if (rewardTypeName.Contains("GoldReward", StringComparison.OrdinalIgnoreCase))
            {
                hints.Add("reward-gold");
            }
            else if (rewardTypeName.Contains("RelicReward", StringComparison.OrdinalIgnoreCase))
            {
                hints.Add("reward-relic");
            }

            hints.Add($"reward-type:{rewardTypeId}");
        }

        if (!string.IsNullOrWhiteSpace(TryResolveEventOptionBindingId(item)))
        {
            hints.Add("scene:event");
            hints.Add("kind:event-option");
            hints.Add(IsEventOptionButtonType(item.GetType().FullName ?? item.GetType().Name)
                ? "source:event-option-button"
                : "source:event-option");
            hints.Add(TryReadBool(TryGetMemberValue(item, "Option") ?? item, "IsProceed") == true
                ? "option-role:proceed"
                : "option-role:choice");
            if (TryReadBool(TryGetMemberValue(item, "Option") ?? item, "IsProceed") == true)
            {
                hints.Add("event-proceed");
            }
        }

        return hints.Count == 0
            ? Array.Empty<string>()
            : hints;
    }

    private static void AppendEventProceedRuntimeMetadata(
        IReadOnlyList<LiveExportChoiceSummary> choices,
        string screen,
        IDictionary<string, string?> meta,
        IDictionary<string, object?> payload)
    {
        var eventAuthority = string.Equals(screen, "event", StringComparison.OrdinalIgnoreCase)
                             || choices.Any(static choice =>
                                 string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)
                                 || choice.SemanticHints.Any(static hint =>
                                     string.Equals(hint, "scene:event", StringComparison.OrdinalIgnoreCase)));
        var proceedChoices = eventAuthority
            ? choices
                .Where(static choice => choice.SemanticHints.Any(static hint =>
                    string.Equals(hint, "option-role:proceed", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(hint, "event-proceed", StringComparison.OrdinalIgnoreCase)))
                .GroupBy(static choice => choice.BindingId
                                          ?? choice.NodeId
                                          ?? choice.Label,
                    StringComparer.OrdinalIgnoreCase)
                .Select(static group => group.First())
                .ToArray()
            : Array.Empty<LiveExportChoiceSummary>();
        var enabledProceedChoices = proceedChoices.Count(static choice => choice.Enabled != false);

        meta["eventProceedOptionVisible"] = proceedChoices.Length > 0 ? "true" : "false";
        meta["eventProceedOptionEnabled"] = enabledProceedChoices > 0 ? "true" : "false";
        meta["eventProceedOptionCount"] = proceedChoices.Length.ToString(CultureInfo.InvariantCulture);
        payload["eventProceedOptionVisible"] = proceedChoices.Length > 0;
        payload["eventProceedOptionEnabled"] = enabledProceedChoices > 0;
        payload["eventProceedOptionCount"] = proceedChoices.Length;
    }

    private static string? TryResolveEventOptionBindingId(object item)
    {
        var option = TryGetMemberValue(item, "Option");
        var typeName = item.GetType().FullName ?? item.GetType().Name;
        if (option is null
            && !typeName.Contains("EventOption", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return FirstNonEmpty(
            TryReadString(option ?? item, "TextKey", "Id", "Name"),
            TryReadString(item, "Index"),
            TryReadString(option, "Title"));
    }

    private static bool? TryResolveEventOptionEnabled(object item)
    {
        var option = TryGetMemberValue(item, "Option") ?? item;
        if (TryReadBool(option, "IsLocked") == true)
        {
            return false;
        }

        return TryResolveInteractiveEnabled(item)
               ?? TryResolveControlEnabled(item)
               ?? TryReadBool(item, "IsEnabled", "Enabled")
               ?? true;
    }

    private static string TryResolveEventOptionNodeId(object item, string bindingId, string label)
    {
        var index = TryReadInt(item, "Index");
        if (index is not null)
        {
            return $"event-option:{index.Value}";
        }

        var key = !string.IsNullOrWhiteSpace(bindingId)
            ? bindingId
            : label;
        return $"event-option:{SanitizeNodeKey(key)}";
    }

    private static bool IsEventOptionButtonType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NEventOptionButton", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetRewardTypeId(string typeName)
    {
        if (typeName.Contains("CardReward", StringComparison.OrdinalIgnoreCase))
        {
            return "CardReward";
        }

        if (typeName.Contains("PotionReward", StringComparison.OrdinalIgnoreCase))
        {
            return "PotionReward";
        }

        if (typeName.Contains("GoldReward", StringComparison.OrdinalIgnoreCase))
        {
            return "GoldReward";
        }

        if (typeName.Contains("RelicReward", StringComparison.OrdinalIgnoreCase))
        {
            return "RelicReward";
        }

        return GetShortTypeName(typeName);
    }

    private static string GetShortTypeName(string typeName)
    {
        var separatorIndex = typeName.LastIndexOf('.');
        return separatorIndex >= 0 && separatorIndex < typeName.Length - 1
            ? typeName[(separatorIndex + 1)..]
            : typeName;
    }

    private static bool IsAuthoritativePlayerRoot(object root)
    {
        var typeName = root.GetType().FullName ?? root.GetType().Name;
        if (LooksLikePreviewLayoutOrContainerType(typeName)
            || typeName.StartsWith("Godot.", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("addons.", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains(".Nodes.", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Screen", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Reward", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Merchant", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Event", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Rest", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Overlay", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Timeout", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Feedback", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Cursor", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return typeName.Contains("Player", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Character", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Hero", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Creature", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeNonAuthoritativePlayerName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Contains("Screen", StringComparison.OrdinalIgnoreCase)
               || value.Contains("Layout", StringComparison.OrdinalIgnoreCase)
               || value.Contains("Container", StringComparison.OrdinalIgnoreCase)
               || value.Contains("Holder", StringComparison.OrdinalIgnoreCase)
               || value.Contains("Rewards", StringComparison.OrdinalIgnoreCase)
               || value.Contains("Overlay", StringComparison.OrdinalIgnoreCase)
               || value.Contains("Timeout", StringComparison.OrdinalIgnoreCase)
               || value.Contains("Feedback", StringComparison.OrdinalIgnoreCase)
               || value.Contains("Multiplayer", StringComparison.OrdinalIgnoreCase)
               || value.Contains("Merchant", StringComparison.OrdinalIgnoreCase)
               || value.Contains("Event", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeCardLikeItem(object item)
    {
        var typeName = item.GetType().FullName ?? item.GetType().Name;
        if (LooksLikePreviewLayoutOrContainerType(typeName)
            || typeName.Contains(".Nodes.", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Button", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Pile", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Queue", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Container", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return typeName.Contains("Card", StringComparison.OrdinalIgnoreCase)
               || TryGetMemberValue(item, "CardId") is not null
               || TryGetMemberValue(item, "EnergyCost") is not null
               || TryGetMemberValue(item, "ManaCost") is not null
               || TryGetMemberValue(item, "Cost") is not null;
    }

    private static string? TryConvertToDisplayString(object? value, int depth = 0)
    {
        if (value is null || depth > 4)
        {
            return null;
        }

        switch (value)
        {
            case string stringValue:
                return NormalizeDisplayString(stringValue);
            case char character:
                return character.ToString();
            case bool boolValue:
                return boolValue ? "true" : "false";
            case byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal:
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            case Enum enumValue:
                return NormalizeDisplayString(enumValue.ToString());
        }

        var type = value.GetType();
        var typeName = type.FullName ?? type.Name;

        if (typeName.Contains("LocString", StringComparison.OrdinalIgnoreCase))
        {
            return FirstNonEmpty(
                TryConvertToDisplayString(TryInvokeMethod(value, "GetRawText"), depth + 1),
                TryConvertToDisplayString(TryInvokeMethod(value, "GetFormattedText"), depth + 1),
                TryConvertToDisplayString(TryGetMemberValue(value, "Text"), depth + 1),
                TryConvertToDisplayString(TryGetMemberValue(value, "Value"), depth + 1),
                TryConvertToDisplayString(TryGetMemberValue(value, "Key"), depth + 1));
        }

        foreach (var methodName in new[] { "GetRawText", "GetParsedText", "GetText", "GetFormattedText" })
        {
            var invoked = TryInvokeMethod(value, methodName);
            var rendered = TryConvertToDisplayString(invoked, depth + 1);
            if (!string.IsNullOrWhiteSpace(rendered))
            {
                return rendered;
            }
        }

        foreach (var memberName in new[]
                 {
                     "Text",
                     "BbcodeText",
                     "DisplayedText",
                     "TooltipText",
                     "Label",
                     "Title",
                     "Description",
                     "OptionName",
                     "DisplayName",
                     "CardName",
                     "Value",
                     "Name",
                     "Key",
                     "Id",
                 })
        {
            var nested = TryGetMemberValue(value, memberName);
            if (nested is null || ReferenceEquals(nested, value))
            {
                continue;
            }

            var rendered = TryConvertToDisplayString(nested, depth + 1);
            if (!string.IsNullOrWhiteSpace(rendered))
            {
                return rendered;
            }
        }

        return NormalizeDisplayString(value.ToString(), typeName, type.Name);
    }

    private static string? NormalizeDisplayString(string? value, params string[] disallowedTypeNames)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.StartsWith('<')
            && normalized.EndsWith('>')
            && normalized.Contains('#', StringComparison.Ordinal))
        {
            return null;
        }

        if (normalized.StartsWith("System.Collections", StringComparison.Ordinal)
            || normalized.StartsWith("MegaCrit.", StringComparison.Ordinal))
        {
            return null;
        }

        if (disallowedTypeNames.Any(typeName => string.Equals(normalized, typeName, StringComparison.Ordinal)))
        {
            return null;
        }

        return normalized;
    }

    private static IEnumerable<object> ExpandEnumerable(object? candidate)
    {
        if (candidate is null or string)
        {
            return Array.Empty<object>();
        }

        if (candidate is not System.Collections.IEnumerable enumerable)
        {
            return Array.Empty<object>();
        }

        var results = new List<object>();
        System.Collections.IEnumerator? enumerator = null;
        try
        {
            enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is not null)
                {
                    results.Add(enumerator.Current);
                }
            }
        }
        catch
        {
            return Array.Empty<object>();
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }

        return results;
    }
}
