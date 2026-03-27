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

internal static partial class Program
{
    private static void RunCombatContractsSelfTests()
    {
        var combatNoOpScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-combat-noop-self-test-{Guid.NewGuid():N}.png");
        var handleCombatParityRequestPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-handle-combat-parity-{Guid.NewGuid():N}.request.json");
        var runtimeStateOnlyScreenshotPath = Path.Combine(Path.GetTempPath(), "sts2-runtime-state-only-self-test-missing.png");

        try
        {
            RunCombatContractsNoOpAndBlockedLaneSelfTests(combatNoOpScreenshotPath);
            RunCombatContractsNonEnemyAndRuntimeStateSelfTests(combatNoOpScreenshotPath);
            RunCombatContractsParityAndBarrierSelfTests(combatNoOpScreenshotPath, handleCombatParityRequestPath, runtimeStateOnlyScreenshotPath);
            RunCombatContractsTargetSelectionSelfTests(combatNoOpScreenshotPath);
            RunCombatContractsFallbacksAndProbeGraceSelfTests(combatNoOpScreenshotPath);
        }
        finally
        {
            if (File.Exists(handleCombatParityRequestPath))
            {
                File.Delete(handleCombatParityRequestPath);
            }

            if (File.Exists(combatNoOpScreenshotPath))
            {
                File.Delete(combatNoOpScreenshotPath);
            }
        }
    }
}
