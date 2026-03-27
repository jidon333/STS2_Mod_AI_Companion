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
    private static void RunEventRewardSubstateSelfTests()
    {
        var colorlessRewardScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-colorless-reward-self-test-{Guid.NewGuid():N}.png");
        try
        {
            using (var bitmap = new Bitmap(1920, 1080))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var brush = new SolidBrush(Color.FromArgb(255, 180, 120)))
            {
                graphics.Clear(Color.Black);
                graphics.FillRectangle(brush, new Rectangle(760, 260, 220, 260));
                bitmap.Save(colorlessRewardScreenshotPath, ImageFormat.Png);
            }

            var colorlessObserver = new ObserverSummary(
                "event",
                "event",
                false,
                DateTimeOffset.UtcNow,
                "inv-colorless",
                true,
                "mixed",
                "stable",
                "episode-colorless",
                null,
                "generic",
                80,
                80,
                null,
                new[] { "Skip", "불타는 혈액", "납 문진", "고정시키기", "주먹다짐" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("event-option:0", "event-option", "Skip", "827,862,276,73", true),
                    new ObserverActionNode("event-option:1", "event-option", "불타는 혈액", "12,82,68,68", true),
                    new ObserverActionNode("event-option:2", "event-option", "납 문진", "80,82,68,68", true),
                },
                new[]
                {
                    new ObserverChoice("card", "Skip", "827,862,276,73"),
                    new ObserverChoice("relic", "불타는 혈액", "12,82,68,68", "RELIC.BURNING_BLOOD"),
                    new ObserverChoice("relic", "납 문진", "80,82,68,68", "RELIC.LEAD_PAPERWEIGHT"),
                    new ObserverChoice("card", "고정시키기", null, "CARD.FASTEN"),
                    new ObserverChoice("card", "주먹다짐", null, "CARD.FISTICUFFS"),
                },
                Array.Empty<ObservedCombatHandCard>());
            var colorlessObserverState = new ObserverState(colorlessObserver, null, null, null);
            var colorlessEventScene = AutoDecisionProvider.BuildEventSceneState(colorlessObserverState, new WindowBounds(0, 0, 1920, 1080), Array.Empty<GuiSmokeHistoryEntry>(), colorlessRewardScreenshotPath);
            Assert(colorlessEventScene.CanonicalForegroundOwner == NonCombatForegroundOwner.Reward
                   && colorlessEventScene.ExplicitAction == EventExplicitActionKind.RewardSubstate,
                "Event reward follow-up should canonicalize to the reward foreground lane instead of generic event ownership.");
            Assert(GetAllowedActions(GuiSmokePhase.HandleEvent, colorlessObserverState).Contains("click colorless card choice", StringComparer.OrdinalIgnoreCase), "Colorless reward state should expose colorless card actions instead of generic event options.");
            var colorlessDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                6,
                GuiSmokePhase.HandleEvent.ToString(),
                "Resolve the event reward follow-up.",
                DateTimeOffset.UtcNow,
                colorlessRewardScreenshotPath,
                new WindowBounds(0, 0, 1920, 1080),
                "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:reward-choice|substate:colorless-card-choice",
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                colorlessObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                GetAllowedActions(GuiSmokePhase.HandleEvent, colorlessObserverState),
                Array.Empty<GuiSmokeHistoryEntry>(),
                "prefer real card choices over inspect affordances",
                null));
            Assert(!string.Equals(colorlessDecision.Status, "wait", StringComparison.OrdinalIgnoreCase),
                "Reward/card substate should emit an action instead of stalling on inspect affordances.");

            var overlayDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                7,
                GuiSmokePhase.HandleEvent.ToString(),
                "Dismiss the inspect overlay before progressing.",
                DateTimeOffset.UtcNow,
                colorlessRewardScreenshotPath,
                new WindowBounds(0, 0, 1920, 1080),
                "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:inspect-overlay|substate:reward-choice",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                colorlessObserver with
                {
                    CurrentChoices = new[] { "Backstop", "LeftArrow", "RightArrow", "Skip", "불타는 혈액", "납 문진", "고정시키기", "주먹다짐" },
                    ActionNodes = new[]
                    {
                        new ObserverActionNode("event-option:0", "event-option", "Backstop", "-192,-108,2304,1296", true),
                        new ObserverActionNode("event-option:1", "event-option", "LeftArrow", "472,476,128,128", true),
                        new ObserverActionNode("event-option:2", "event-option", "RightArrow", "1320,476,128,128", true),
                        new ObserverActionNode("event-option:3", "event-option", "Skip", "827,862,276,73", true),
                    },
                    Choices = new[]
                    {
                        new ObserverChoice("choice", "Backstop", "-192,-108,2304,1296"),
                        new ObserverChoice("choice", "LeftArrow", "472,476,128,128"),
                        new ObserverChoice("choice", "RightArrow", "1320,476,128,128"),
                        new ObserverChoice("card", "Skip", "827,862,276,73"),
                        new ObserverChoice("relic", "불타는 혈액", "12,82,68,68", "RELIC.BURNING_BLOOD"),
                        new ObserverChoice("relic", "납 문진", "80,82,68,68", "RELIC.LEAD_PAPERWEIGHT"),
                        new ObserverChoice("card", "고정시키기", null, "CARD.FASTEN"),
                        new ObserverChoice("card", "주먹다짐", null, "CARD.FISTICUFFS"),
                    },
                },
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                new[] { "press escape", "click inspect overlay close", "wait" },
                new[]
                {
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleEvent.ToString(), "click", "overlay backdrop close", DateTimeOffset.UtcNow.AddSeconds(-3)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleEvent.ToString(), "click", "overlay backdrop close", DateTimeOffset.UtcNow.AddSeconds(-1)),
                },
                "overlay recovery required",
                null));
            Assert(string.Equals(overlayDecision.TargetLabel, "inspect overlay escape", StringComparison.OrdinalIgnoreCase), "Repeated inspect overlay closes should escalate to escape instead of repeating the same backdrop click.");
        }
        finally
        {
            if (File.Exists(colorlessRewardScreenshotPath))
            {
                File.Delete(colorlessRewardScreenshotPath);
            }
        }
    }
}
