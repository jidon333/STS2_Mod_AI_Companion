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
        RunPhaseRoutingSelfTestBands();
        RunNonCombatForegroundOwnershipSelfTests();
        RunNonCombatDecisionContractBands();
        RunCombatContractBands();
        RunEventRewardSubstateSelfTests();
        RunManualCleanBootSelfTests();
        RunObserverProvenanceParitySelfTests();
        RunStartupRuntimeEvidenceSelfTests();
        RunStallSentinelSelfTests();
        RunLongRunSupervisorSelfTests();
    }

    private static void RunPhaseRoutingSelfTestBands()
    {
        var request = CreateBaseSelfTestRequest();

        RunPhaseRoutingCombatAndMixedStateSelfTests(request);
        RunPhaseRoutingEnterRunAndPostNodeSelfTests();
        RunPhaseRoutingRunLoadRecoverySelfTests();
        RunPhaseRoutingEmbarkWaitsSelfTests();
    }

    private static void RunNonCombatDecisionContractBands()
    {
        var rewardRankingScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-reward-ranking-self-test-{Guid.NewGuid():N}.png");
        try
        {
            using (var bitmap = new Bitmap(1280, 720))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var panelBrush = new SolidBrush(Color.FromArgb(222, 205, 168)))
            using (var arrowBrush = new SolidBrush(Color.FromArgb(220, 60, 55)))
            {
                graphics.Clear(Color.Black);
                graphics.FillRectangle(panelBrush, new Rectangle(420, 180, 480, 320));
                graphics.FillEllipse(arrowBrush, new Rectangle(595, 450, 90, 70));
                bitmap.Save(rewardRankingScreenshotPath, ImageFormat.Png);
            }

            RunNonCombatRewardDecisionContractSelfTests(rewardRankingScreenshotPath);
            RunNonCombatSubtypeDecisionContractSelfTests(rewardRankingScreenshotPath);
        }
        finally
        {
            if (File.Exists(rewardRankingScreenshotPath))
            {
                File.Delete(rewardRankingScreenshotPath);
            }
        }
    }

    private static void RunCombatContractBands()
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
