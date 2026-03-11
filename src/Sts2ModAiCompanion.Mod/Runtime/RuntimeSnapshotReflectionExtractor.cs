using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModAiCompanion.Mod.Runtime;

internal static class RuntimeSnapshotReflectionExtractor
{
    private static readonly string[] SingletonPropertyNames = { "Instance", "Current", "Singleton", "State" };
    private static readonly string[] NestedRootNames = { "Run", "RunState", "State", "Player", "Character", "Encounter", "Combat", "Manager" };
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

    private sealed record RuntimeSceneProbe(
        string? SceneName,
        string? CurrentSceneType,
        string? SceneRootType,
        string? Screen,
        int TraversedNodeCount,
        IReadOnlyList<string> VisibleNodeTypes,
        IReadOnlyList<string> InterestingNodeTypes);

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
        var choices = ExtractChoices(instance, args, roots, config.LiveExport.MaxChoiceEntries);
        var encounter = ExtractEncounter(roots);
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
            meta);
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
                string.Join(" ", visibleNodeTypes.Take(24)),
                string.Join(" ", interestingNodeTypes.Take(24))),
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
        var playerRoots = FindRoots(roots, "Player", "Character", "Hero", "Creature");
        var name = TryReadString(playerRoots, "Name", "CharacterName", "DisplayName");
        var currentHp = TryReadInt(playerRoots, "CurrentHp", "CurrentHealth", "Hp", "Health");
        var maxHp = TryReadInt(playerRoots, "MaxHp", "MaxHealth", "HealthMax");
        var gold = TryReadInt(playerRoots, "Gold", "CurrentGold");
        var energy = TryReadInt(playerRoots, "Energy", "CurrentEnergy");
        var resources = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in new[] { "Block", "Strength", "Dexterity", "Focus", "Mana", "Miracle", "OrbSlots" })
        {
            var value = TryReadString(playerRoots, key);
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
        foreach (var item in FindEnumerableItems(roots, "MasterDeck", "Deck", "Cards", "DrawPile"))
        {
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

    private static IReadOnlyList<string> ExtractStringList(IEnumerable<object> roots, int maxEntries, params string[] collectionNames)
    {
        return FindEnumerableItems(roots, collectionNames)
            .Select(item => TryReadString(item, "Name", "DisplayName", "Id", "RelicId", "PotionId") ?? item.GetType().Name)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(maxEntries)
            .ToArray();
    }

    private static IReadOnlyList<LiveExportChoiceSummary> ExtractChoices(
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
                         "Choices",
                         "Options",
                         "Rewards",
                         "Buttons",
                         "CurrentOptions",
                         "RewardButtons",
                         "RewardAlternatives",
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
                     "Buttons"))
        {
            AddIfUseful(candidateItems, item);
        }

        var choices = candidateItems
            .Select(TryCreateChoiceSummary)
            .Where(choice => choice is not null)
            .Cast<LiveExportChoiceSummary>()
            .DistinctBy(choice => $"{choice.Kind}|{choice.Label}|{choice.Value}")
            .Take(maxEntries)
            .ToArray();

        return choices;
    }

    private static LiveExportEncounterSummary? ExtractEncounter(IEnumerable<object> roots)
    {
        var encounterRoots = FindRoots(roots, "Encounter", "Room", "Combat", "Battle", "Monster");
        var name = TryReadString(encounterRoots, "Name", "EncounterName", "RoomName", "DisplayName");
        var kind = TryReadString(encounterRoots, "Type", "EncounterType", "RoomType");
        var turn = TryReadInt(encounterRoots, "Turn", "TurnNumber", "CurrentTurn");
        var inCombat = TryReadBool(encounterRoots, "IsInProgress", "InCombat", "IsInCombat");

        if (name is null && kind is null && turn is null && inCombat is null)
        {
            return null;
        }

        return new LiveExportEncounterSummary(name, kind, inCombat, turn);
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

        if (joined.Contains("CharacterSelect", StringComparison.OrdinalIgnoreCase)
            || joined.Contains("NewRun", StringComparison.OrdinalIgnoreCase))
        {
            return "character-select";
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

            foreach (var memberName in new[]
                     {
                         "Option",
                         "Reward",
                         "Entry",
                         "Card",
                         "Model",
                         "Title",
                         "Label",
                         "Choices",
                         "Options",
                         "Rewards",
                         "Buttons",
                         "RewardButtons",
                         "RewardAlternatives",
                         "CurrentOptions",
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
            TryResolveChoiceDescription(item));
    }

    private static bool LooksLikeChoiceCandidate(object item)
    {
        var typeName = item.GetType().FullName ?? item.GetType().Name;
        return typeName.Contains("Button", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Option", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Reward", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Merchant", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Slot", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Choice", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Card", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Potion", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Relic", StringComparison.OrdinalIgnoreCase)
               || TryGetMemberValue(item, "Option") is not null
               || TryGetMemberValue(item, "Reward") is not null
               || TryGetMemberValue(item, "Entry") is not null
               || TryGetMemberValue(item, "Card") is not null;
    }

    private static string? TryResolveChoiceLabel(object item)
    {
        return FirstNonEmpty(
            TryReadString(item, "Label", "Title", "OptionName", "Text", "DisplayName", "CardName", "Name"),
            TryReadString(item, "Option", "Reward", "Entry", "Card", "Model"),
            TryReadString(TryGetMemberValue(item, "Option"), "Title", "Label", "Name", "Description"),
            TryReadString(TryGetMemberValue(item, "Reward"), "Description", "Title", "Label", "Name"),
            TryReadString(TryGetMemberValue(item, "Entry"), "DisplayName", "Title", "Description", "Name"),
            TryReadString(TryGetMemberValue(item, "Card"), "CardName", "Name", "DisplayName", "Id", "CardId"),
            TryReadString(TryGetMemberValue(item, "Model"), "CardName", "Name", "DisplayName", "Id", "CardId"),
            TryReadString(TryGetMemberValue(item, "Title"), "Text", "Value"),
            TryReadString(TryGetMemberValue(item, "Label"), "Text", "Value"),
            TryReadString(item, "Id", "CardId"));
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
            TryReadString(item, "Description", "Tooltip", "Body"),
            TryReadString(TryGetMemberValue(item, "Option"), "Description", "Body"),
            TryReadString(TryGetMemberValue(item, "Reward"), "Description", "Body"),
            TryReadString(TryGetMemberValue(item, "Entry"), "DisplayName", "Description", "Body"),
            TryReadString(TryGetMemberValue(item, "Card"), "Description", "Body"),
            TryReadString(TryGetMemberValue(item, "Model"), "Description", "Body"));
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
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
                TryConvertToDisplayString(TryInvokeMethod(value, "GetFormattedText"), depth + 1),
                TryConvertToDisplayString(TryInvokeMethod(value, "GetRawText"), depth + 1),
                TryConvertToDisplayString(TryGetMemberValue(value, "Text"), depth + 1),
                TryConvertToDisplayString(TryGetMemberValue(value, "Value"), depth + 1),
                TryConvertToDisplayString(TryGetMemberValue(value, "Key"), depth + 1));
        }

        foreach (var methodName in new[] { "GetFormattedText", "GetRawText", "GetParsedText", "GetText" })
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
            yield break;
        }

        if (candidate is not System.Collections.IEnumerable enumerable)
        {
            yield break;
        }

        foreach (var item in enumerable)
        {
            if (item is not null)
            {
                yield return item;
            }
        }
    }
}
