using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModAiCompanion.Mod;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;
using static GuiSmokeChoicePrimitiveSupport;

static class CombatBarrierPolicy
{
    public const int HandleCombatWaitMinimumMs = 140;
    public const int HandleCombatWaitPlateauLimit = 4;

    public static bool IsStableCombatEntryObserver(ObserverState observer)
    {
        return string.Equals(observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase)
               && observer.SceneReady == true
               && string.Equals(observer.SceneStability, "stable", StringComparison.OrdinalIgnoreCase)
               && observer.InCombat == true;
    }
}
