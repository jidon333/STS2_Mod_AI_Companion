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
    private static void RunCombatContractsNoOpAndBlockedLaneSelfTests(string combatNoOpScreenshotPath)
    {
            using (var bitmap = new Bitmap(1280, 720))
            {
                bitmap.Save(combatNoOpScreenshotPath, ImageFormat.Png);
            }

            var combatNoOpObserver = new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-combat-noop",
                true,
                "mixed",
                "stable",
                "episode-combat-noop",
                "Combat",
                "combat",
                76,
                80,
                1,
                new[] { "3턴 종료" },
                Array.Empty<string>(),
                new[] { new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true) },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(3, "CARD.BASH", "Attack", null),
                    new ObservedCombatHandCard(4, "CARD.STRIKE_IRONCLAD", "Attack", null),
                    new ObservedCombatHandCard(5, "CARD.STRIKE_IRONCLAD", "Attack", null),
                })
            {
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "false",
                    ["combatTargetingInProgress"] = "false",
                    ["combatLifecycleLane"] = "attacklike",
                    ["combatLifecyclePhase"] = "cleared",
                    ["combatLifecycleSource"] = "runtime-clear",
                },
            };
            var combatNoOpHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 3", DateTimeOffset.UtcNow.AddSeconds(-10)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "auto-target enemy", DateTimeOffset.UtcNow.AddSeconds(-8)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 3", DateTimeOffset.UtcNow.AddSeconds(-7)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 3", DateTimeOffset.UtcNow.AddSeconds(-6)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "auto-target enemy", DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 3", DateTimeOffset.UtcNow.AddSeconds(-3)),
            };
            var combatNoOpKnowledge = new[]
            {
                new CombatCardKnowledgeHint(3, "CARD.BASH", "Attack", "AnyEnemy", 2, "self-test"),
                new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(5, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var combatNoOpAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(combatNoOpObserver, null, null, null),
                combatNoOpKnowledge,
                combatNoOpScreenshotPath,
                combatNoOpHistory);
            Assert(!combatNoOpAllowedActions.Contains("select attack slot 3", StringComparer.OrdinalIgnoreCase), "Combat allowlist should keep the repeated no-op Bash lane closed.");
            Assert(combatNoOpAllowedActions.Contains("select attack slot 4", StringComparer.OrdinalIgnoreCase)
                   || combatNoOpAllowedActions.Contains("select attack slot 5", StringComparer.OrdinalIgnoreCase),
                "Combat allowlist should still offer one of the playable Strike lanes.");
            var combatNoOpDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                54,
                GuiSmokePhase.HandleCombat.ToString(),
                "Play the turn from the screenshot.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable",
                "0002",
                2,
                3,
                false,
                "tactical",
                null,
                combatNoOpObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                combatNoOpKnowledge,
                combatNoOpAllowedActions,
                combatNoOpHistory,
                "prefer a playable card or end turn when energy is insufficient for the previously selected card",
                null));
            Assert(!string.Equals(combatNoOpDecision.TargetLabel, "combat select attack slot 3", StringComparison.OrdinalIgnoreCase)
                   && (string.Equals(combatNoOpDecision.TargetLabel, "combat select attack slot 4", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(combatNoOpDecision.TargetLabel, "combat select attack slot 5", StringComparison.OrdinalIgnoreCase)),
                "Combat decisioning should skip the unplayable Bash lane and pick one of the playable Strike lanes instead.");
            Assert(TryClassifyCombatNoOpLoop(
                GuiSmokePhase.HandleCombat,
                new GuiSmokeStepRequest(
                    "run",
                    "boot-to-long-run",
                    55,
                    GuiSmokePhase.HandleCombat.ToString(),
                    "Guard against combat no-op loops.",
                    DateTimeOffset.UtcNow,
                    combatNoOpScreenshotPath,
                    new WindowBounds(0, 0, 1280, 720),
                    "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable",
                    "0002",
                    2,
                    3,
                    false,
                    "tactical",
                    null,
                    combatNoOpObserver,
                    Array.Empty<KnownRecipeHint>(),
                    Array.Empty<EventKnowledgeCandidate>(),
                    new[]
                    {
                        new CombatCardKnowledgeHint(3, "CARD.BASH", "Attack", "AnyEnemy", 2, "self-test"),
                        new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                        new CombatCardKnowledgeHint(5, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                    },
                    new[] { "click card", "click enemy", "click end turn", "wait" },
                    combatNoOpHistory,
                    "guard the repeated no-op lane",
                    null),
                new GuiSmokeStepDecision("act", "press-key", "3", null, null, "combat select attack slot 3", "looping on unplayable Bash", 0.5, "combat", 250, true, null),
                out _), "Repeated unplayable card selection should be classified as a combat-noop-loop before another no-op action is executed.");
            var blockedEnemyActions = BuildAllowedActions(GuiSmokePhase.HandleCombat, new ObserverState(combatNoOpObserver, null, null, null), new[]
            {
                new CombatCardKnowledgeHint(3, "CARD.BASH", "Attack", "AnyEnemy", 1, "self-test"),
            }, combatNoOpScreenshotPath, combatNoOpHistory);
            Assert(!blockedEnemyActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Combat allowlist should close direct enemy targeting when the currently pending attack lane has already produced repeated no-op outcomes.");

            var recoveredPendingSelectionHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-6)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 4", DateTimeOffset.UtcNow.AddSeconds(-5)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-3)),
            };
            var recoveredPendingSelection = CombatHistorySupport.TryGetPendingCombatSelection(recoveredPendingSelectionHistory);
            Assert(recoveredPendingSelection is { Kind: AutoCombatCardKind.AttackLike, SlotIndex: 4 }, "Combat pending selection recovery should preserve lane 4 across combat-noop and enemy-target labels.");
            Assert(CombatHistorySupport.GetCombatNoOpCountsBySlot(recoveredPendingSelectionHistory).TryGetValue(4, out var recoveredPendingSelectionNoOpCount)
                   && recoveredPendingSelectionNoOpCount == 2,
                "Combat no-op counting should attribute enemy-target no-ops back to the recovered lane 4 selection.");

            var artifact0020Observer = new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-artifact-0020",
                true,
                "mixed",
                "stable",
                "episode-artifact-0020",
                "Monster",
                "combat",
                80,
                80,
                3,
                new[] { "압축벌레" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("enemy-target:2", "enemy-target", "압축벌레", "1398.24,621.6,83.52,88", true),
                },
                new[]
                {
                    new ObserverChoice("enemy-target", "압축벌레", "1398.24,621.6,83.52,88", "압축벌레", "target-source:vfx-spawn-hitbox")
                    {
                        NodeId = "enemy-target:2",
                    },
                },
                new[]
                {
                    new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    new ObservedCombatHandCard(4, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                });
            var artifact0020Knowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var artifact0020History = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-10)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-9)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-8)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-7)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 4", DateTimeOffset.UtcNow.AddSeconds(-6)),
            };
            var artifact0020AllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(artifact0020Observer, null, null, null),
                artifact0020Knowledge,
                combatNoOpScreenshotPath,
                artifact0020History);
            Assert(!artifact0020AllowedActions.Contains("select attack slot 4", StringComparer.OrdinalIgnoreCase), "Artifact-like 0020 allowlist should close slot 4 after repeated no-op lane evidence.");
            Assert(!artifact0020AllowedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Artifact-like 0020 allowlist should close enemy targeting once the pending lane is blocked.");
            var artifact0020Request = new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                20,
                GuiSmokePhase.HandleCombat.ToString(),
                "Replay the artifact-like blocked lane state.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                artifact0020Observer,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                artifact0020Knowledge,
                artifact0020AllowedActions,
                artifact0020History,
                "Close illegal attack selection and target retries.",
                null);
            var artifact0020Decision = AutoDecisionProvider.Decide(artifact0020Request);
            Assert(!string.Equals(artifact0020Decision.TargetLabel, "combat select attack slot 4", StringComparison.OrdinalIgnoreCase), "Artifact-like 0020 regression should not reissue illegal slot 4 selection.");
            Assert(CombatDecisionContract.IsAllowed(artifact0020Request, artifact0020Decision, out var artifact0020SemanticAction)
                   && !string.Equals(artifact0020SemanticAction, "select attack slot 4", StringComparison.OrdinalIgnoreCase),
                "Artifact-like 0020 regression should end with a legal combat semantic action.");

            var artifact0021Observer = artifact0020Observer with
            {
                InventoryId = "inv-artifact-0021",
                SceneEpisodeId = "episode-artifact-0021",
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    new ObservedCombatHandCard(4, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    new ObservedCombatHandCard(5, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                },
            };
            var artifact0021Knowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(5, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var artifact0021History = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-10)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-9)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 4", DateTimeOffset.UtcNow.AddSeconds(-8)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-7)),
            };
            var artifact0021AllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(artifact0021Observer, null, null, null),
                artifact0021Knowledge,
                combatNoOpScreenshotPath,
                artifact0021History);
            Assert(artifact0021AllowedActions.Contains("select attack slot 5", StringComparer.OrdinalIgnoreCase), "Artifact-like 0021 allowlist should keep alternate slot 5 open.");
            Assert(!artifact0021AllowedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Artifact-like 0021 allowlist should keep enemy targeting closed while slot 4 remains blocked.");
            var artifact0021Request = new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                21,
                GuiSmokePhase.HandleCombat.ToString(),
                "Replay the artifact-like blocked target fallback state.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                artifact0021Observer,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                artifact0021Knowledge,
                artifact0021AllowedActions,
                artifact0021History,
                "Prefer the legal alternate slot over an illegal enemy click.",
                null);
            var artifact0021Decision = AutoDecisionProvider.Decide(artifact0021Request);
            Assert(!(artifact0021Decision.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) ?? false), "Artifact-like 0021 regression should not emit any illegal enemy-target click.");
            Assert(CombatDecisionContract.IsAllowed(artifact0021Request, artifact0021Decision, out var artifact0021SemanticAction)
                   && !string.Equals(artifact0021SemanticAction, "click enemy", StringComparison.OrdinalIgnoreCase),
                "Artifact-like 0021 regression should end with a legal non-enemy-click combat semantic action.");
    }
}
