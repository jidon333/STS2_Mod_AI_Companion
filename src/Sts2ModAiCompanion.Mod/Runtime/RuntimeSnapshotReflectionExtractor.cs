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
        var screen = screenOverride
                     ?? TryReadString(roots, "CurrentScreen", "Screen", "ScreenName", "RoomType")
                     ?? InferScreen(screenCandidates);
        var runId = TryReadString(roots, "RunId", "Id", "Seed", "SaveId");
        var act = TryReadInt(roots, "Act", "ActNum", "CurrentAct");
        var floor = TryReadInt(roots, "Floor", "CurrentFloor", "FloorNum");
        var player = ExtractPlayerSummary(roots);
        var deck = ExtractDeck(roots, config.LiveExport.MaxDeckEntries);
        var relics = ExtractStringList(roots, config.LiveExport.MaxDeckEntries, "Relics", "OwnedRelics", "InventoryRelics");
        var potions = ExtractStringList(roots, config.LiveExport.MaxChoiceEntries, "Potions", "OwnedPotions", "PotionSlots");
        var choices = ExtractChoices(instance, args, roots, config.LiveExport.MaxChoiceEntries);
        var encounter = ExtractEncounter(roots);
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
        AddIfUseful(choiceRoots, instance);
        if (args is not null)
        {
            foreach (var arg in args)
            {
                AddIfUseful(choiceRoots, arg);
                foreach (var item in ExpandEnumerable(arg))
                {
                    AddIfUseful(choiceRoots, item);
                }
            }
        }

        foreach (var root in roots)
        {
            AddIfUseful(choiceRoots, TryGetMemberValue(root, "Choices"));
            AddIfUseful(choiceRoots, TryGetMemberValue(root, "Options"));
            AddIfUseful(choiceRoots, TryGetMemberValue(root, "Rewards"));
            AddIfUseful(choiceRoots, TryGetMemberValue(root, "Buttons"));
        }

        var choices = new List<LiveExportChoiceSummary>();
        foreach (var item in FindEnumerableItems(choiceRoots, "Choices", "Options", "Rewards", "Buttons", "Cards"))
        {
            var label = TryReadString(item, "Label", "Text", "Name", "DisplayName", "Id", "CardName");
            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            choices.Add(new LiveExportChoiceSummary(
                InferChoiceKind(item.GetType().FullName),
                label,
                TryReadString(item, "Value", "Id", "CardId"),
                TryReadString(item, "Description", "Tooltip", "Body", "Text")));
        }

        return choices
            .DistinctBy(choice => $"{choice.Kind}|{choice.Label}|{choice.Value}")
            .Take(maxEntries)
            .ToArray();
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

        if (joined.Contains("Combat", StringComparison.OrdinalIgnoreCase))
        {
            return "combat";
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
        return value?.ToString();
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
                .FirstOrDefault(candidate => string.Equals(candidate.Name, memberName, StringComparison.OrdinalIgnoreCase));
            if (property is not null && property.GetIndexParameters().Length == 0)
            {
                return property.GetValue(source is Type ? null : source);
            }

            var field = type.GetFields(flags)
                .FirstOrDefault(candidate => string.Equals(candidate.Name, memberName, StringComparison.OrdinalIgnoreCase));
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
