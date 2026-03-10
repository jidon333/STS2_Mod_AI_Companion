using System.Reflection;
using HarmonyLib;

namespace Sts2ModAiCompanion.Mod.Runtime;

internal sealed record RuntimeHookCandidate(
    string Category,
    string SemanticKind,
    string TypeName,
    string[] MethodNames,
    string? ScreenOverride = null);

internal sealed record RuntimeHookBinding(
    RuntimeHookCandidate Candidate,
    MethodBase Method);

internal static class RuntimeHookCatalog
{
    private static readonly object Sync = new();
    private static Dictionary<MethodBase, RuntimeHookBinding>? _bindings;
    private static List<string>? _skippedCandidates;
    private static bool _loggedSummary;

    private static readonly RuntimeHookCandidate[] Candidates =
    {
        new("run-lifecycle", "run-started", "MegaCrit.Sts2.Core.Runs.RunManager", new[] { "StartRun", "BeginRun", "CreateRun" }),
        new("run-lifecycle", "run-loaded", "MegaCrit.Sts2.Core.Runs.RunManager", new[] { "LoadRun", "ContinueRun", "ResumeRun" }),
        new("run-lifecycle", "run-ended", "MegaCrit.Sts2.Core.Runs.RunManager", new[] { "EndRun", "WinRun", "LoseRun", "GameOver" }),
        new("save", "save-persisted", "MegaCrit.Sts2.Core.Saves.SaveManager", new[] { "Save", "Write", "Persist", "Flush" }),
        new("save", "save-persisted", "MegaCrit.Sts2.Core.Saves.Managers.RunSaveManager", new[] { "Save", "Write", "Persist", "Flush" }),
        new("combat", "combat-started", "MegaCrit.Sts2.Core.Combat.CombatManager", new[] { "StartCombat", "BeginCombat" }, "combat"),
        new("combat", "combat-ended", "MegaCrit.Sts2.Core.Combat.CombatManager", new[] { "EndCombat", "FinishCombat", "CompleteCombat" }, "combat"),
        new("combat", "turn-started", "MegaCrit.Sts2.Core.Combat.CombatState", new[] { "StartTurn", "BeginTurn", "NextTurn" }, "combat"),
        new("combat", "turn-ended", "MegaCrit.Sts2.Core.Combat.CombatState", new[] { "EndTurn", "FinishTurn" }, "combat"),
        new("screen", "map-node-entered", "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen", new[] { "Open", "Setup" }, "map"),
        new("screen", "reward-screen-opened", "MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NCardRewardSelectionScreen", new[] { "Open", "Setup" }, "rewards"),
        new("screen", "reward-screen-opened", "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen", new[] { "Open", "Setup" }, "rewards"),
        new("screen", "choice-list-presented", "MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NChooseACardSelectionScreen", new[] { "Open", "Setup" }, "card-choice"),
        new("screen", "choice-list-presented", "MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NDeckUpgradeSelectScreen", new[] { "Open", "Setup" }, "upgrade"),
        new("screen", "choice-list-presented", "MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NDeckTransformSelectScreen", new[] { "Open", "Setup" }, "transform"),
        new("screen", "event-screen-opened", "MegaCrit.Sts2.Core.Nodes.Events.NEventLayout", new[] { "Open", "Setup" }, "event"),
        new("screen", "event-screen-opened", "MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom", new[] { "Open", "Setup" }, "event"),
        new("screen", "choice-list-presented", "MegaCrit.Sts2.Core.Nodes.Rooms.NRestSiteRoom", new[] { "Open", "Setup" }, "rest-site"),
        new("screen", "choice-list-presented", "MegaCrit.Sts2.Core.Nodes.Screens.Shops.NMerchantInventory", new[] { "Open", "Setup", "Fill" }, "shop"),
    };

    public static IReadOnlyCollection<MethodBase> GetTargetMethods()
    {
        EnsureBindings();
        LogSummaryOnce();
        return _bindings!.Keys.ToArray();
    }

    public static RuntimeHookBinding? TryGetBinding(MethodBase method)
    {
        EnsureBindings();
        return _bindings!.TryGetValue(method, out var binding)
            ? binding
            : _bindings.Values.FirstOrDefault(candidate =>
                string.Equals(candidate.Method.DeclaringType?.FullName, method.DeclaringType?.FullName, StringComparison.Ordinal)
                && string.Equals(candidate.Method.Name, method.Name, StringComparison.Ordinal));
    }

    public static IReadOnlyList<string> DescribeBindings()
    {
        EnsureBindings();
        return _bindings!.Values
            .OrderBy(binding => binding.Candidate.Category, StringComparer.Ordinal)
            .ThenBy(binding => binding.Method.DeclaringType?.FullName, StringComparer.Ordinal)
            .ThenBy(binding => binding.Method.Name, StringComparer.Ordinal)
            .Select(binding => $"{binding.Candidate.Category}:{binding.Candidate.SemanticKind}:{binding.Method.DeclaringType?.FullName}.{binding.Method.Name}")
            .ToArray();
    }

    private static void EnsureBindings()
    {
        if (_bindings is not null)
        {
            return;
        }

        lock (Sync)
        {
            if (_bindings is not null)
            {
                return;
            }

            var bindings = new Dictionary<MethodBase, RuntimeHookBinding>();
            var skippedCandidates = new List<string>();
            foreach (var candidate in Candidates)
            {
                var type = RuntimeTypeLocator.TryFindType(candidate.TypeName);
                if (type is null)
                {
                    skippedCandidates.Add($"{candidate.Category}:{candidate.SemanticKind}:{candidate.TypeName}:missing-type");
                    continue;
                }

                var selectedMethods = SelectMethods(type, candidate.MethodNames).ToArray();
                if (selectedMethods.Length == 0)
                {
                    skippedCandidates.Add($"{candidate.Category}:{candidate.SemanticKind}:{candidate.TypeName}:no-method-match[{string.Join(", ", candidate.MethodNames)}]");
                    continue;
                }

                foreach (var method in selectedMethods)
                {
                    bindings.TryAdd(method, new RuntimeHookBinding(candidate, method));
                }
            }

            _bindings = bindings;
            _skippedCandidates = skippedCandidates;
        }
    }

    private static IEnumerable<MethodBase> SelectMethods(Type type, IReadOnlyCollection<string> methodNames)
    {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        foreach (var methodName in methodNames)
        {
            foreach (var method in type
                         .GetMethods(flags)
                         .Where(method => string.Equals(method.Name, methodName, StringComparison.Ordinal))
                         .Where(method => !method.IsAbstract && !method.IsGenericMethod && !method.IsSpecialName))
            {
                yield return method;
            }
        }
    }

    private static void LogSummaryOnce()
    {
        if (_loggedSummary)
        {
            return;
        }

        lock (Sync)
        {
            if (_loggedSummary)
            {
                return;
            }

            AiCompanionRuntimeLog.WriteLine($"hook bindings discovered: {_bindings!.Count}");
            foreach (var skippedCandidate in _skippedCandidates ?? Enumerable.Empty<string>())
            {
                AiCompanionRuntimeLog.WriteLine($"hook candidate skipped: {skippedCandidate}");
            }

            foreach (var line in DescribeBindings())
            {
                AiCompanionRuntimeLog.WriteLine($"hook binding: {line}");
            }

            _loggedSummary = true;
        }
    }
}

internal static class RuntimeTypeLocator
{
    public static Type? TryFindType(string typeName)
    {
        return AccessTools.TypeByName(typeName)
               ?? AppDomain.CurrentDomain
                   .GetAssemblies()
                   .SelectMany(GetTypesSafely)
                   .FirstOrDefault(type =>
                       string.Equals(type.FullName, typeName, StringComparison.Ordinal)
                       || string.Equals(type.Name, typeName, StringComparison.Ordinal)
                       || string.Equals(type.FullName, typeName.Replace('+', '.'), StringComparison.Ordinal)
                       || type.FullName?.EndsWith(typeName, StringComparison.Ordinal) == true);
    }

    private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
        catch
        {
            return Array.Empty<Type>();
        }
    }
}
