using System.Reflection;
using HarmonyLib;

namespace Sts2ModAiCompanion.Mod.Runtime;

[HarmonyPatch]
internal static class AiCompanionExporterPatches
{
    [HarmonyPrepare]
    private static bool Prepare()
    {
        AiCompanionRuntimeLog.WriteLine($"exporter patch prepare: {AiCompanionRuntimeIdentity.DescribeExecutingAssembly()}");
        var initialized = RuntimeExportContext.EnsureInitialized();
        if (!initialized)
        {
            AiCompanionRuntimeLog.WriteLine("exporter patch prepare skipped because runtime exporter is disabled.");
        }

        return initialized;
    }

    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        var methods = RuntimeHookCatalog.GetTargetMethods().ToArray();
        AiCompanionRuntimeLog.WriteLine($"exporter patch targets selected: {methods.Length}");
        return methods;
    }

    [HarmonyPostfix]
    private static void Postfix(MethodBase __originalMethod, object? __instance, object[]? __args)
    {
        // Avoid requiring __result so postfixes can apply to both void and non-void targets.
        RuntimeExportContext.RecordHookInvocation(__originalMethod, __instance, result: null, __args);
    }
}
