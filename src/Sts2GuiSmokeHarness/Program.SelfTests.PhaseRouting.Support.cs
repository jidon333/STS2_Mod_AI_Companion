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
    private static ObserverState CreatePhaseRoutingRewardMixedStateObserver()
    {
        return new ObserverState(
            new ObserverSummary(
                "map",
                "map",
                false,
                DateTimeOffset.UtcNow,
                null,
                true,
                "mixed",
                "stable",
                null,
                null,
                "reward",
                76,
                80,
                null,
                new[] { "넘기기" },
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                new[]
                {
                    new ObserverChoice("reward-card", "Reward Card", "460,250,240,340", "CARD.TEST", "Reward card"),
                },
                Array.Empty<ObservedCombatHandCard>()),
            null,
            null,
            null);
    }

    private static ObserverState CreatePhaseRoutingCharacterSelectObserver()
    {
        return new ObserverState(
            new ObserverSummary(
                "character-select",
                "character-select",
                false,
                DateTimeOffset.UtcNow,
                null,
                true,
                "stable",
                "stable",
                null,
                null,
                null,
                80,
                80,
                null,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                Array.Empty<ObserverChoice>(),
                Array.Empty<ObservedCombatHandCard>()),
            null,
            null,
            null);
    }
}
