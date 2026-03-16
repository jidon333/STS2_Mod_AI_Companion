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
        LiveExportChoiceDecision Decision);

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
        var deck = ExtractDeck(roots, config.LiveExport.MaxDeckEntries);
        var relics = ExtractStringList(roots, config.LiveExport.MaxDeckEntries, "Relics", "OwnedRelics", "InventoryRelics");
        var potions = ExtractStringList(roots, config.LiveExport.MaxChoiceEntries, "Potions", "OwnedPotions", "PotionSlots");
        var choiceResult = ExtractChoices(
            triggerKind,
            screenOverride,
            instance,
            args,
            roots,
            config.LiveExport.MaxChoiceEntries);
        var choices = choiceResult.Choices;
        var encounter = ExtractEncounter(roots, meta);
        var combatHand = encounter?.InCombat == true
            ? ExtractCombatHand(roots, config.LiveExport.MaxChoiceEntries)
            : Array.Empty<LiveExportCardSummary>();
        var rootTypeSummary = string.Join(
            " ",
            roots
                .Select(root => root.GetType().FullName ?? root.GetType().Name)
                .Distinct(StringComparer.Ordinal)
                .Take(96));
        var screen = ResolveScreen(screenOverride, roots, new[] { rootTypeSummary }.Concat(screenCandidates).ToArray());
        if (string.Equals(screen, "unknown", StringComparison.Ordinal)
            && encounter?.InCombat == true)
        {
            screen = "combat";
        }
        var warnings = BuildWarnings(player, deck, relics, potions, choices);

        meta["screen"] = screen;
        meta["rootTypeSummary"] = rootTypeSummary;
        if (!string.IsNullOrWhiteSpace(choiceResult.Decision.ExtractorPath))
        {
            meta["choiceExtractorPath"] = choiceResult.Decision.ExtractorPath;
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
            meta["combatTargetSummary"] = string.Join(
                ";",
                combatTargetChoices.Select(choice => $"{choice.NodeId ?? choice.Label}@{choice.ScreenBounds}"));
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
        }

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
        int maxEntries)
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

        if (LooksLikeRestContext(triggerKind, screenHint, choiceRoots))
        {
            strictAttempts.Add(
                EvaluateChoiceSet(
                    "rest",
                    CollectRestChoiceItems(choiceRoots),
                    maxEntries,
                    strictExtractor: true));
        }

        if (LooksLikeEventContext(triggerKind, screenHint, choiceRoots))
        {
            strictAttempts.Add(
                EvaluateChoiceSet(
                    "event",
                    CollectEventChoiceItems(choiceRoots),
                    maxEntries,
                    strictExtractor: true));
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

        if (LooksLikeMapContext(triggerKind, screenHint, choiceRoots))
        {
            strictAttempts.Add(ExtractMapChoices(choiceRoots, maxEntries));
        }

        var strictSuccess = strictAttempts.FirstOrDefault(result => result.Choices.Count > 0);
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
        var creatures = new List<object>();
        foreach (var root in roots)
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
            .Select((creature, index) => TryCreateCombatEnemyTargetChoiceSummary(creature, index + 1))
            .Where(static choice => choice is not null)
            .Cast<LiveExportChoiceSummary>()
            .OrderBy(static choice => TryGetBoundsSortX(choice.ScreenBounds))
            .ThenBy(static choice => TryGetBoundsSortY(choice.ScreenBounds))
            .ToArray();
    }

    private static LiveExportChoiceSummary? TryCreateCombatEnemyTargetChoiceSummary(object creature, int ordinal)
    {
        var typeName = creature.GetType().FullName ?? creature.GetType().Name;
        if (!typeName.Contains("NCreature", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (TryReadBool(creature, "Visible", "IsVisible", "VisibleInTree") == false
            || TryReadBool(creature, "IsInteractable") == false)
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

        var label = ResolveCombatEnemyTargetLabel(creature, entity, ordinal);
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        if (!TryResolveCombatEnemyTargetBounds(creature, out var screenBounds, out var targetSource)
            || string.IsNullOrWhiteSpace(screenBounds))
        {
            return null;
        }

        return new LiveExportChoiceSummary(
            "enemy-target",
            label,
            TryReadString(entity, "Id", "Name", "DisplayName"),
            $"target-source:{targetSource}")
        {
            NodeId = $"enemy-target:{ordinal}",
            ScreenBounds = screenBounds,
        };
    }

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

    private static string InferChoiceKind(string? typeName)
    {
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

        if (joined.Contains("Event", StringComparison.OrdinalIgnoreCase))
        {
            return "event";
        }

        if (joined.Contains("Reward", StringComparison.OrdinalIgnoreCase))
        {
            return "rewards";
        }

        if (joined.Contains("Map", StringComparison.OrdinalIgnoreCase))
        {
            return "map";
        }

        if (joined.Contains("Rest", StringComparison.OrdinalIgnoreCase))
        {
            return "rest-site";
        }

        if (joined.Contains("Shop", StringComparison.OrdinalIgnoreCase) || joined.Contains("Merchant", StringComparison.OrdinalIgnoreCase))
        {
            return "shop";
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

        var label = TryResolveChoiceLabel(item);
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        return new LiveExportChoiceSummary(
            InferChoiceKind(item.GetType().FullName),
            label,
            TryResolveChoiceValue(item),
            TryResolveChoiceDescription(item))
        {
            ScreenBounds = TryResolveScreenBounds(item),
        };
    }

    private static bool LooksLikeMapContext(string triggerKind, string? screenHint, IEnumerable<object> choiceRoots)
    {
        if (string.Equals(screenHint, "map", StringComparison.OrdinalIgnoreCase)
            || string.Equals(triggerKind, "map", StringComparison.OrdinalIgnoreCase)
            || string.Equals(triggerKind, "map-point-selected", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return choiceRoots.Any(root =>
        {
            var typeName = root.GetType().FullName ?? root.GetType().Name;
            return typeName.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase)
                   || typeName.Contains("NMapPoint", StringComparison.OrdinalIgnoreCase);
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
            if (typeName.Contains("NMapPoint", StringComparison.OrdinalIgnoreCase))
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

        var choices = mapPoints
            .Select(TryCreateMapChoiceSummary)
            .Where(choice => choice is not null)
            .Cast<LiveExportChoiceSummary>()
            .OrderBy(choice => choice.NodeId, StringComparer.Ordinal)
            .Take(maxEntries)
            .ToArray();

        var candidates = choices
            .Select(choice => new LiveExportChoiceCandidate(
                "map",
                "NMapPoint",
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
                "map",
                UsedStrictExtractor: true,
                CandidateCount: candidates.Length,
                AcceptedCount: choices.Length,
                choices.Length == 0 ? "none" : "accepted",
                choices.Length == 0 ? "no reachable map points with bounds resolved" : null,
                Array.Empty<string>()));
    }

    private static LiveExportChoiceSummary? TryCreateMapChoiceSummary(object item)
    {
        var typeName = item.GetType().FullName ?? item.GetType().Name;
        if (!typeName.Contains("NMapPoint", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (TryReadBool(item, "IsEnabled") != true)
        {
            return null;
        }

        var screenBounds = TryResolveScreenBounds(item);
        if (string.IsNullOrWhiteSpace(screenBounds))
        {
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
               && !IsPlaceholderChoiceLabel(label)
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
        return IsPlaceholderChoiceLabel(label) ? null : label;
    }

    private static string? TryResolveChoiceValue(object item)
    {
        return FirstNonEmpty(
            TryReadString(item, "Value", "Id", "CardId", "Hotkey"),
            TryReadString(TryGetMemberValue(item, "Option"), "Id", "Name", "Title"),
            TryReadString(TryGetMemberValue(item, "Reward"), "Id", "Name", "Description"),
            TryReadString(TryGetMemberValue(item, "Entry"), "Id", "Name"),
            TryReadString(TryGetMemberValue(item, "Card"), "Id", "CardId", "Name"),
            TryReadString(TryGetMemberValue(item, "Model"), "Id", "CardId", "Name"));
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

    private static bool IsPlaceholderChoiceLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
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
