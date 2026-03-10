using HarmonyLib;

namespace Sts2ModAiCompanion.Mod.Runtime;

[HarmonyPatch]
internal static class AiCompanionPatchStub
{
    [HarmonyPrepare]
    private static bool Prepare()
    {
        var config = AiCompanionRuntimeState.GetConfig(forceRefresh: true);
        AiCompanionRuntimeLog.WriteLine($"patch bootstrap loaded. enabled={config.Enabled}");
        return false;
    }
}
