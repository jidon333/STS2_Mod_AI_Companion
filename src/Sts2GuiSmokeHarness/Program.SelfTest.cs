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
    private static void RunSelfTest()
    {
        RunCliStartupSelfTests();
        RunCaptureReplaySelfTests();
        RunPhaseRoutingSelfTests();
        RunNonCombatForegroundOwnershipSelfTests();
        RunNonCombatDecisionContractsSelfTests();
        RunCombatContractsSelfTests();
        RunEventRewardSubstateSelfTests();
        RunManualCleanBootSelfTests();
        RunStartupRuntimeEvidenceSelfTests();
        RunStallSentinelSelfTests();
        RunLongRunSupervisorSelfTests();
    }

    private static GuiSmokeStepDecision InvokeForegroundAwareNonCombatWaitDecision(
        GuiSmokeStepRequest selfTestRequest,
        string reason,
        bool allowFastForegroundWait = true)
    {
        var method = typeof(AutoDecisionProvider).GetMethod(
            "CreateForegroundAwareNonCombatWaitDecision",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (method is null)
        {
            throw new InvalidOperationException("Self-test could not find CreateForegroundAwareNonCombatWaitDecision.");
        }

        return (GuiSmokeStepDecision)(method.Invoke(null, new object?[] { selfTestRequest, reason, allowFastForegroundWait })
                                      ?? throw new InvalidOperationException("Foreground-aware noncombat wait helper returned null."));
    }

    private static GuiSmokeStepRequest CreateBaseSelfTestRequest()
    {
            return new GuiSmokeStepRequest(
                "run",
                "boot-to-combat",
                3,
                GuiSmokePhase.EnterRun.ToString(),
                "enter-run",
                DateTimeOffset.UtcNow,
                "screen.png",
                new WindowBounds(100, 200, 1000, 800),
                "scene:test",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                new ObserverSummary("main-menu", "main-menu", false, DateTimeOffset.UtcNow, null, null, null, null, null, null, null, null, null, null, new[] { "Continue" }, new[] { "main-menu" }, Array.Empty<ObserverActionNode>(), Array.Empty<ObserverChoice>(), Array.Empty<ObservedCombatHandCard>()),
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                new[] { "click continue", "click singleplayer" },
                Array.Empty<GuiSmokeHistoryEntry>(),
                "menu entry",
                null);
    }

    private static GuiSmokeStepDecision CreateBaseSelfTestDecision()
    {
        return new GuiSmokeStepDecision("act", "click", null, 0.3, 0.7, "continue", "main menu continue", 0.9, "character-select", 1000, true, null);
    }
}
