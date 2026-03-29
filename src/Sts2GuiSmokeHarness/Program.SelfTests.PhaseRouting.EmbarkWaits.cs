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
    private static void RunPhaseRoutingEmbarkWaitsSelfTests()
    {
        var embarkEventObserver = new ObserverState(
            new ObserverSummary(
                "event",
                "event",
                false,
                DateTimeOffset.UtcNow,
                "inv-event",
                true,
                "mixed",
                "stable",
                "episode-embark",
                "None",
                "event",
                80,
                80,
                null,
                new[] { "Option A" },
                Array.Empty<string>(),
                new[] { new ObserverActionNode("event-option:0", "event-option", "Option A", "460,750,1000,100", true) },
                new[] { new ObserverChoice("choice", "Option A", "460,750,1000,100") },
                Array.Empty<ObservedCombatHandCard>()),
            null,
            null,
            null);
        Assert(GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(embarkEventObserver, out var postEmbarkPhase) && postEmbarkPhase == GuiSmokePhase.HandleEvent, "Embark should reconcile to HandleEvent when observer already reports an event room.");
        Assert(GetAllowedActions(GuiSmokePhase.Embark, embarkEventObserver).Contains("click event choice", StringComparer.OrdinalIgnoreCase), "Embark allowlist should admit event progression actions when observer is already in an event room.");
        var embarkDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            7,
            GuiSmokePhase.Embark.ToString(),
            "Click Embark to begin the run.",
            DateTimeOffset.UtcNow,
            "screen.png",
            new WindowBounds(0, 0, 1280, 720),
            "phase:embark|screen:event|visible:event|encounter:none|ready:true|stability:stable|room:treasure",
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            embarkEventObserver.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            GetAllowedActions(GuiSmokePhase.Embark, embarkEventObserver),
            Array.Empty<GuiSmokeHistoryEntry>(),
            "phase reconciliation required",
            null));
        Assert(string.Equals(embarkDecision.TargetLabel, "event progression choice", StringComparison.OrdinalIgnoreCase), "Embark decisioning should switch to the event progression choice instead of waiting for an embark button.");
        Assert(TryClassifyDecisionWaitPlateau(GuiSmokePhase.Embark, embarkEventObserver, 2, out var embarkPlateauCause, out _) && string.Equals(embarkPlateauCause, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase), "Embark/event repeated waits should escalate to phase-mismatch-stall.");
        var chooseCharacterRecoveryRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-choose-character-post-embark-{Guid.NewGuid():N}");
        var chooseCharacterRecoveryLogger = new ArtifactRecorder(chooseCharacterRecoveryRoot);
        var chooseCharacterHistory = new List<GuiSmokeHistoryEntry>();
        Assert(
            TryAdvanceAlternateBranch(
                GuiSmokePhase.ChooseCharacter,
                embarkEventObserver,
                chooseCharacterHistory,
                chooseCharacterRecoveryLogger,
                7,
                false,
                out var chooseCharacterRecoveredPhase)
            && chooseCharacterRecoveredPhase == GuiSmokePhase.HandleEvent,
            "ChooseCharacter should yield to HandleEvent when stale character-select phase survives into an already-visible event room.");
        Assert(
            TryClassifyDecisionWaitPlateau(GuiSmokePhase.ChooseCharacter, embarkEventObserver, 2, out var chooseCharacterPlateauCause, out _)
            && string.Equals(chooseCharacterPlateauCause, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase),
            "ChooseCharacter/event repeated waits should escalate to phase-mismatch-stall instead of a generic decision plateau.");
        Assert(TryClassifyDecisionWaitPlateau(GuiSmokePhase.HandleEvent, embarkEventObserver, 5, out var waitPlateauCause, out _) && string.Equals(waitPlateauCause, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase), "Repeated waits in a stable room scene should escalate to decision-wait-plateau.");
    }
}
