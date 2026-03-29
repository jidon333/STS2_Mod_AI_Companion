using System.Reflection;
using HarmonyLib;

namespace Sts2ModAiCompanion.Mod.Runtime;

[HarmonyPatch]
internal static class AiCompanionStartupLifecyclePatches
{
    [HarmonyTargetMethod]
    private static MethodBase? TargetMethod()
    {
        return AccessTools.Method("MegaCrit.Sts2.Core.Nodes.NGame:LaunchMainMenu");
    }

    [HarmonyPrefix]
    private static void Prefix(ref bool skipLogo)
    {
        var config = AiCompanionRuntimeState.GetConfig();
        if (!config.Enabled
            || !config.Harness.Enabled
            || !config.Startup.ForceSkipIntroLogoWhenHarnessEnabled
            || skipLogo)
        {
            return;
        }

        skipLogo = true;
        AiCompanionRuntimeLog.WriteLine("startup lifecycle patch forced skipLogo=true for harness-enabled launch.");
    }
}
