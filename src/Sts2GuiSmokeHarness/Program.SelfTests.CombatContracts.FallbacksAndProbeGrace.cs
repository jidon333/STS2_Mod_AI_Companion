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

            var nonEnemySelectDecision = new GuiSmokeStepDecision("act", "press-key", "3", null, null, "combat select non-enemy slot 3", "stage-aware settle", 0.5, "combat", 120, true, null);
            Assert(CombatPostActionObservationSupport.GetMinimumSettleDelayMs(nonEnemySelectDecision) == 1200, "Non-enemy selection settle should reserve a wider combat settle budget.");
            var attackSelectDecision = new GuiSmokeStepDecision("act", "press-key", "4", null, null, "combat select attack slot 4", "stage-aware settle", 0.5, "combat", 120, true, null);
            Assert(CombatPostActionObservationSupport.GetMinimumSettleDelayMs(attackSelectDecision) == 900, "Attack selection settle should use the combat lane settle budget.");

            var combatSettleCapturedAt = DateTimeOffset.UtcNow;
            var nonEnemySettleHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 3", DateTimeOffset.UtcNow),
            };
            var nonEnemySettleKnowledge = new[]
            {
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            };
            var staleNonEnemyObserver = new ObserverState(
                new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    combatSettleCapturedAt,
                    "inv-combat-non-enemy-stale",
                    true,
                    "hook",
                    "stable",
                    "episode-combat-non-enemy-stale",
                    "Combat",
                    "combat",
                    60,
                    80,
                    1,
                    new[] { "1턴 종료" },
                    Array.Empty<string>(),
                    Array.Empty<ObserverActionNode>(),
                    Array.Empty<ObserverChoice>(),
                    new[]
                    {
                        new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                        new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    })
                {
                    SnapshotVersion = 41,
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                        ["combatCardPlayPending"] = "false",
                        ["combatTargetingInProgress"] = "false",
                        ["combatHistoryStartedCount"] = "4",
                        ["combatHistoryFinishedCount"] = "2",
                        ["combatInteractionRevision"] = "4:2:false:false:none",
                    },
                },
                null,
                null,
                null);
            var pendingNonEnemyObserver = new ObserverState(
                staleNonEnemyObserver.Summary with
                {
                    CapturedAt = combatSettleCapturedAt.AddMilliseconds(600),
                    InventoryId = "inv-combat-non-enemy-pending",
                    SnapshotVersion = 42,
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                        ["combatCardPlayPending"] = "true",
                        ["combatTargetingInProgress"] = "false",
                        ["combatSelectedCardSlot"] = "3",
                        ["combatSelectedCardType"] = "Skill",
                        ["combatSelectedCardTargetType"] = "Self",
                        ["combatAwaitingPlaySlots"] = "3",
                        ["combatHistoryStartedCount"] = "5",
                        ["combatHistoryFinishedCount"] = "2",
                        ["combatInteractionRevision"] = "5:2:true:false:none",
                    },
                },
                null,
                null,
                null);
            var nonEnemyWakeEvaluator = CombatPostActionObservationSupport.CreateWakeEvaluator(
                nonEnemySelectDecision,
                nonEnemySettleHistory,
                nonEnemySettleKnowledge,
                new WindowBounds(0, 0, 1280, 720));
            Assert(nonEnemyWakeEvaluator(staleNonEnemyObserver) is null, "Combat non-enemy settle should not treat the stale post-action snapshot as actionable convergence.");
            Assert(string.Equals(nonEnemyWakeEvaluator(pendingNonEnemyObserver), "combat-non-enemy-confirm-ready", StringComparison.OrdinalIgnoreCase), "Combat non-enemy settle should wake only after runtime confirm evidence surfaces.");

            var enemyClickHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target 1", DateTimeOffset.UtcNow),
            };
            var unresolvedTargetObserver = new ObserverState(
                staleNonEnemyObserver.Summary with
                {
                    InventoryId = "inv-combat-target-quiet",
                    SnapshotVersion = 55,
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                        ["combatCardPlayPending"] = "true",
                        ["combatTargetingInProgress"] = "false",
                        ["combatSelectedCardSlot"] = "2",
                        ["combatSelectedCardType"] = "Attack",
                        ["combatSelectedCardTargetType"] = "AnyEnemy",
                        ["combatHistoryStartedCount"] = "7",
                        ["combatHistoryFinishedCount"] = "3",
                        ["combatInteractionRevision"] = "7:3:true:false:none",
                    },
                },
                null,
                null,
                null);
            var targetQuietEvaluator = CombatPostActionObservationSupport.CreateWakeEvaluator(
                new GuiSmokeStepDecision("act", "click", null, 0.5, 0.5, "combat enemy target 1", "quiet convergence", 0.5, "combat", 250, true, null),
                enemyClickHistory,
                nonEnemySettleKnowledge,
                new WindowBounds(0, 0, 1280, 720));
            Assert(targetQuietEvaluator(unresolvedTargetObserver) is null, "First unresolved combat target snapshot should start, not finish, quiet convergence.");
            Assert(targetQuietEvaluator(unresolvedTargetObserver) is null, "Second unresolved combat target snapshot should still be within the quiet convergence window.");
            Assert(targetQuietEvaluator(unresolvedTargetObserver) is null, "Third unresolved combat target snapshot should still defer combat settle completion.");
            Assert(targetQuietEvaluator(unresolvedTargetObserver) is null, "Stable unresolved combat target state should not finish settle while the selected attack lane is still unresolved.");

            var resolvedTargetObserver = new ObserverState(
                unresolvedTargetObserver.Summary with
                {
                    InventoryId = "inv-combat-target-resolved",
                    SnapshotVersion = 56,
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                        ["combatCardPlayPending"] = "false",
                        ["combatTargetingInProgress"] = "false",
                        ["combatHistoryStartedCount"] = "7",
                        ["combatHistoryFinishedCount"] = "4",
                        ["combatInteractionRevision"] = "7:4:false:false:none",
                        ["combatLastCardPlayFinishedCardId"] = "CARD.STRIKE_IRONCLAD",
                    },
                },
                null,
                null,
                null);
            Assert(string.Equals(targetQuietEvaluator(resolvedTargetObserver), "combat-enemy-click-resolved", StringComparison.OrdinalIgnoreCase), "Combat enemy target settle should wake once the selected attack lane fully resolves back to an open combat state.");

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
