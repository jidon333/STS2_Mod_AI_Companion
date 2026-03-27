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
    private static void RunCombatContractsFallbacksAndProbeGraceSelfTests(string combatNoOpScreenshotPath)
    {
            var repeatedAttackOrderingObserver = new ObserverState(
                new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    DateTimeOffset.UtcNow,
                    "inv-repeated-attack-ordering",
                    true,
                    "mixed",
                    "stable",
                    "episode-repeated-attack-ordering",
                    "Combat",
                    "combat",
                    55,
                    80,
                    2,
                    new[] { "3턴 종료" },
                    Array.Empty<string>(),
                    new[] { new ObserverActionNode("end-turn", "button", "3턴 종료", "1080,620,140,60", true) },
                    Array.Empty<ObserverChoice>(),
                    Array.Empty<ObservedCombatHandCard>()),
                null,
                null,
                null);
            var repeatedAttackOrderingDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                12,
                GuiSmokePhase.HandleCombat.ToString(),
                "Do not end turn before replaying a knowledge-backed attack slot.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                repeatedAttackOrderingObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                new[]
                {
                    new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                    new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                },
                new[] { "click card", "click end turn", "wait" },
                new[]
                {
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-4)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-3)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-2)),
                },
                "Prefer a known playable attack before ending the turn.",
                null));
            Assert(string.Equals(repeatedAttackOrderingDecision.TargetLabel, "combat select attack slot 2", StringComparison.OrdinalIgnoreCase), "Repeated attack-select loop handling should still try a knowledge-backed playable attack before end turn.");

            var noEnemyTargetObserver = new ObserverState(
                new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    DateTimeOffset.UtcNow,
                    "inv-no-enemy-target",
                    true,
                    "mixed",
                    "stable",
                    "episode-no-enemy-target",
                    "Combat",
                    "combat",
                    65,
                    80,
                    1,
                    new[] { "4턴 종료" },
                    Array.Empty<string>(),
                    new[] { new ObserverActionNode("end-turn", "button", "4턴 종료", "1604,846,220,90", true) },
                    Array.Empty<ObserverChoice>(),
                    new[]
                    {
                        new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                        new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    }),
                null,
                null,
                null);
            var noEnemyTargetHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-5)),
            };
            var noEnemyTargetKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var noEnemyAllowedActions = BuildAllowedActions(GuiSmokePhase.HandleCombat, noEnemyTargetObserver, noEnemyTargetKnowledge, combatNoOpScreenshotPath, noEnemyTargetHistory);
            Assert(!noEnemyAllowedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Combat allowlist should keep enemy targeting closed when no playable attack remains in the current observer hand.");
            var noEnemyDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                27,
                GuiSmokePhase.HandleCombat.ToString(),
                "Do not overstate an enemy target when the hand is all defend.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                noEnemyTargetObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                noEnemyTargetKnowledge,
                noEnemyAllowedActions,
                noEnemyTargetHistory,
                "Do not click the enemy without a selected attack.",
                null));
            Assert(!string.Equals(noEnemyDecision.TargetLabel, "auto-target enemy", StringComparison.OrdinalIgnoreCase), "Combat decisioning should not emit auto-target enemy when the observer hand no longer contains an attack.");

            var unchangedCombatProbeObserver = new ObserverState(
                new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    DateTimeOffset.UtcNow,
                    "inv-combat-probe-grace",
                    true,
                    "mixed",
                    "stable",
                    "episode-combat-probe-grace",
                    "Combat",
                    "combat",
                    60,
                    80,
                    3,
                    new[] { "Jaw Worm" },
                    Array.Empty<string>(),
                    Array.Empty<ObserverActionNode>(),
                    Array.Empty<ObserverChoice>(),
                    new[] { new ObservedCombatHandCard(4, "CARD.STRIKE_IRONCLAD", "Attack", 1) }),
                null,
                null,
                null);
            Assert(ShouldGrantCombatNoOpProbeGrace(
                GuiSmokePhase.HandleCombat,
                unchangedCombatProbeObserver,
                unchangedCombatProbeObserver,
                new GuiSmokeStepDecision("act", "press-key", "4", null, null, "combat select attack slot 4", "probe grace", 0.5, "combat", 250, true, null)),
                "Combat no-op-sensitive actions should allow one grace resample before a same-frame no-op is recorded.");
            Assert(!ShouldGrantCombatNoOpProbeGrace(
                GuiSmokePhase.HandleCombat,
                unchangedCombatProbeObserver,
                new ObserverState(
                    unchangedCombatProbeObserver.Summary with
                    {
                        CapturedAt = DateTimeOffset.UtcNow.AddMilliseconds(200),
                        PlayerEnergy = 2,
                    },
                    null,
                    null,
                    null),
                new GuiSmokeStepDecision("act", "press-key", "E", null, null, "auto-end turn", "no probe grace", 0.5, "combat", 250, true, null)),
                "Combat probe grace should stay closed once a post-action delta is already visible or the action is not no-op-sensitive.");

            var staleBoundsDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                64,
                GuiSmokePhase.HandleCombat.ToString(),
                "Reject stale off-window combat bounds.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0002",
                2,
                3,
                false,
                "tactical",
                null,
                new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    DateTimeOffset.UtcNow,
                    "inv-stale-bounds",
                    true,
                    "mixed",
                    "stable",
                    "episode-stale-bounds",
                    "Combat",
                    "combat",
                    30,
                    80,
                    0,
                    new[] { "1턴 종료" },
                    Array.Empty<string>(),
                    new[] { new ObserverActionNode("stale-end-turn", "button", "1턴 종료", "1604,846,220,90", true) },
                    Array.Empty<ObserverChoice>(),
                    Array.Empty<ObservedCombatHandCard>()),
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                new[] { "click end turn", "wait" },
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Reject stale bounds.",
                null));
            Assert(string.Equals(staleBoundsDecision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(staleBoundsDecision.KeyText, "E", StringComparison.OrdinalIgnoreCase), "Off-window combat action bounds should be hard-rejected so fallback end-turn uses a safe key press instead of stale coordinates.");
    }
}
