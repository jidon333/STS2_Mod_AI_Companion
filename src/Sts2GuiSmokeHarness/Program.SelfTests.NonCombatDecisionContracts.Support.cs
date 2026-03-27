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
    private static ObserverState CreateStaleRewardObserver()
    {
        return new ObserverState(
            new ObserverSummary(
                "rewards",
                "map",
                false,
                DateTimeOffset.UtcNow,
                "inv-stale-reward",
                false,
                "mixed",
                "stabilizing",
                "episode-stale-reward",
                "None",
                "reward",
                80,
                80,
                null,
                new[] { "넘기기" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("proceed:0", "proceed", "넘기기", "1983,764,269,108", true),
                },
                new[]
                {
                    new ObserverChoice("choice", "넘기기", "1983,764,269,108"),
                },
                Array.Empty<ObservedCombatHandCard>()),
            null,
            null,
            null);
    }
}
