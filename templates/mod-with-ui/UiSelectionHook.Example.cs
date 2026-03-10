using HarmonyLib;

namespace Example.Mod;

[HarmonyPatch]
internal static class UiSelectionHookExample
{
    [HarmonyTargetMethod]
    private static System.Reflection.MethodBase? TargetMethod()
    {
        var type = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NModInfoContainer");
        return type is null ? null : AccessTools.Method(type, "Fill");
    }

    [HarmonyPostfix]
    private static void Postfix(object __instance, object? mod)
    {
        InGameConfigUiExample.RefreshForSelection(__instance, mod);
    }
}
