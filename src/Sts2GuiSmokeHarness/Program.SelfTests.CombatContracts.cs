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
        try
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

            var combatEligibility0015Observer = new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-combat-eligibility-0015",
                true,
                "mixed",
                "stable",
                "episode-combat-eligibility-0015",
                "Monster",
                "combat-targets",
                73,
                80,
                2,
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
                    new ObservedCombatHandCard(1, "CARD.POOR_SLEEP", "Curse", null),
                    new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", null),
                    new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", null),
                    new ObservedCombatHandCard(4, "CARD.STRIKE_IRONCLAD", "Attack", null),
                })
            {
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                },
            };
            var combatEligibility0015Knowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.POOR_SLEEP", "Curse", "None", -1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var combatEligibility0015History = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 2", DateTimeOffset.UtcNow.AddSeconds(-5)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "auto-end turn", DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-3)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-2)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 2", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            var combatEligibility0015Actions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(combatEligibility0015Observer, null, null, null),
                combatEligibility0015Knowledge,
                combatNoOpScreenshotPath,
                combatEligibility0015History);
            Assert(!combatEligibility0015Actions.Contains("select non-enemy slot 1", StringComparer.OrdinalIgnoreCase), "0015-like combat allowlist should not promote the curse in slot 1 as a playable non-enemy action.");

            var enemyTurn0021Observer = new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-enemy-turn-0021",
                true,
                "mixed",
                "stable",
                "episode-enemy-turn-0021",
                "Monster",
                "combat-targets",
                53,
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
                    new ObservedCombatHandCard(1, "CARD.POOR_SLEEP", "Curse", null),
                    new ObservedCombatHandCard(2, "CARD.BASH", "Attack", null),
                    new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", null),
                })
            {
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=true;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                },
            };
            var enemyTurn0021Knowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.POOR_SLEEP", "Curse", "None", -1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.BASH", "Attack", "AnyEnemy", 2, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            };
            var enemyTurn0021Actions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(enemyTurn0021Observer, null, null, null),
                enemyTurn0021Knowledge,
                combatNoOpScreenshotPath,
                Array.Empty<GuiSmokeHistoryEntry>());
            Assert(enemyTurn0021Actions.Length == 1
                   && string.Equals(enemyTurn0021Actions[0], "wait", StringComparison.OrdinalIgnoreCase),
                "0021-like enemy-turn combat allowlist should collapse to wait only.");
            var enemyTurn0021Request = new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                21,
                GuiSmokePhase.HandleCombat.ToString(),
                "Hold during enemy turn.",
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
                enemyTurn0021Observer,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                enemyTurn0021Knowledge,
                enemyTurn0021Actions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Do not act during enemy turn.",
                null);
            var enemyTurn0021Decision = AutoDecisionProvider.Decide(enemyTurn0021Request);
            Assert(string.Equals(enemyTurn0021Decision.Status, "wait", StringComparison.OrdinalIgnoreCase), "0021-like enemy-turn combat decision should wait instead of issuing a player action.");

            var repeatedNonEnemyHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 1", DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 1", DateTimeOffset.UtcNow.AddSeconds(-3)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 1", DateTimeOffset.UtcNow.AddSeconds(-2)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 1", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            var repeatedNonEnemyActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(combatEligibility0015Observer, null, null, null),
                combatEligibility0015Knowledge,
                combatNoOpScreenshotPath,
                repeatedNonEnemyHistory);
            Assert(!repeatedNonEnemyActions.Contains("select non-enemy slot 1", StringComparer.OrdinalIgnoreCase), "Repeated non-enemy regression should keep slot 1 closed when it only contains a curse.");
            var repeatedNonEnemyRequest = new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                22,
                GuiSmokePhase.HandleCombat.ToString(),
                "Do not repeat the illegal curse selection loop.",
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
                combatEligibility0015Observer,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                combatEligibility0015Knowledge,
                repeatedNonEnemyActions,
                repeatedNonEnemyHistory,
                "Close the non-enemy repeat loop.",
                null);
            var repeatedNonEnemyDecision = AutoDecisionProvider.Decide(repeatedNonEnemyRequest);
            Assert(!string.Equals(repeatedNonEnemyDecision.TargetLabel, "combat select non-enemy slot 1", StringComparison.OrdinalIgnoreCase), "Repeated non-enemy regression should not reissue slot 1.");
            Assert(!CombatDecisionContract.IsAllowed(
                    repeatedNonEnemyRequest with { AllowedActions = new[] { "select non-enemy slot 2", "wait" } },
                    new GuiSmokeStepDecision("act", "click-current", null, null, null, "confirm selected non-enemy card", "test", 0.5, "combat", 0, true, null),
                    out _),
                "click-current should not be legal without actual selected-state evidence.");

            var pendingNonEnemySlot3History = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 3", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            var pendingNonEnemySlot3Observer = new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-pending-non-enemy-slot3",
                true,
                "mixed",
                "stable",
                "episode-pending-non-enemy-slot3",
                "Monster",
                "combat-targets",
                73,
                80,
                2,
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
                    new ObservedCombatHandCard(1, "CARD.POOR_SLEEP", "Curse", null),
                    new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", null),
                    new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", null),
                })
            {
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                },
            };
            var pendingNonEnemySlot3Knowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.POOR_SLEEP", "Curse", "None", -1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            };
            var pendingNonEnemySlot3Actions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(pendingNonEnemySlot3Observer, null, null, null),
                pendingNonEnemySlot3Knowledge,
                combatNoOpScreenshotPath,
                pendingNonEnemySlot3History);
            Assert(!pendingNonEnemySlot3Actions.Contains("select non-enemy slot 3", StringComparer.OrdinalIgnoreCase), "Pending non-enemy slot should not reopen without current selected-state evidence.");
            var pendingNonEnemySlot3Request = new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                23,
                GuiSmokePhase.HandleCombat.ToString(),
                "Do not reselect the same non-enemy slot without evidence that it stayed selected.",
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
                pendingNonEnemySlot3Observer,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                pendingNonEnemySlot3Knowledge,
                pendingNonEnemySlot3Actions,
                pendingNonEnemySlot3History,
                "Do not reissue the same non-enemy slot without selected-state evidence.",
                null);
            var pendingNonEnemySlot3Decision = AutoDecisionProvider.Decide(pendingNonEnemySlot3Request);
            Assert(!string.Equals(pendingNonEnemySlot3Decision.TargetLabel, "combat select non-enemy slot 3", StringComparison.OrdinalIgnoreCase), "Pending non-enemy slot regression should not reissue slot 3 without selected-state evidence.");

            var recentRetriedNonEnemySlot3History = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 2", DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 3", DateTimeOffset.UtcNow.AddSeconds(-3)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "auto-end turn", DateTimeOffset.UtcNow.AddSeconds(-2)),
            };
            var recentRetriedNonEnemySlot3Actions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(pendingNonEnemySlot3Observer, null, null, null),
                pendingNonEnemySlot3Knowledge,
                combatNoOpScreenshotPath,
                recentRetriedNonEnemySlot3History);
            Assert(!recentRetriedNonEnemySlot3Actions.Contains("select non-enemy slot 3", StringComparer.OrdinalIgnoreCase), "Recently retried non-enemy slot should stay closed without current selected-state evidence.");
            var recentRetriedNonEnemySlot3Request = new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                24,
                GuiSmokePhase.HandleCombat.ToString(),
                "Do not reopen a recently retried non-enemy slot without selected-state evidence.",
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
                pendingNonEnemySlot3Observer,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                pendingNonEnemySlot3Knowledge,
                recentRetriedNonEnemySlot3Actions,
                recentRetriedNonEnemySlot3History,
                "Do not reissue a recently retried non-enemy lane without selected-state evidence.",
                null);
            var recentRetriedNonEnemySlot3Decision = AutoDecisionProvider.Decide(recentRetriedNonEnemySlot3Request);
            Assert(!string.Equals(recentRetriedNonEnemySlot3Decision.TargetLabel, "combat select non-enemy slot 3", StringComparison.OrdinalIgnoreCase), "Recently retried non-enemy regression should not reissue slot 3 without selected-state evidence.");

            var runtimePendingNonEnemyObserver = pendingNonEnemySlot3Observer with
            {
                InventoryId = "inv-runtime-pending-non-enemy",
                SceneEpisodeId = "episode-runtime-pending-non-enemy",
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                    ["combatCardPlayPending"] = "true",
                    ["combatPlayMode"] = "Combat",
                    ["combatSelectedCardSlot"] = "3",
                    ["combatSelectedCardType"] = "Skill",
                    ["combatSelectedCardTargetType"] = "Self",
                    ["combatTargetingInProgress"] = "false",
                    ["combatLastCardPlayFinishedCardId"] = "CARD.STRIKE_IRONCLAD",
                },
            };
            var runtimePendingNonEnemyActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimePendingNonEnemyObserver, null, null, null),
                pendingNonEnemySlot3Knowledge,
                combatNoOpScreenshotPath,
                pendingNonEnemySlot3History);
            Assert(runtimePendingNonEnemyActions.Contains("confirm selected non-enemy card", StringComparer.OrdinalIgnoreCase), "Runtime pending non-enemy state should export an explicit confirm action.");
            Assert(!runtimePendingNonEnemyActions.Contains("select non-enemy slot 3", StringComparer.OrdinalIgnoreCase), "Runtime pending non-enemy state should keep the same slot closed until confirm resolves.");
            var runtimePendingNonEnemyDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                25,
                GuiSmokePhase.HandleCombat.ToString(),
                "Runtime pending non-enemy state should confirm even without screenshot-selected evidence.",
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
                runtimePendingNonEnemyObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                pendingNonEnemySlot3Knowledge,
                runtimePendingNonEnemyActions,
                pendingNonEnemySlot3History,
                "Trust runtime pending state before screenshot heuristics.",
                null));
            Assert(string.Equals(runtimePendingNonEnemyDecision.TargetLabel, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase), "Runtime pending non-enemy state should drive confirm instead of reopening the same hotkey lane.");
            Assert(string.Equals(runtimePendingNonEnemyDecision.ActionKind, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase), "Runtime pending non-enemy state should use the dedicated confirm actuator.");
            Assert(CombatDecisionContract.IsAllowed(
                    new GuiSmokeStepRequest(
                        "run",
                        "boot-to-long-run",
                        25,
                        GuiSmokePhase.HandleCombat.ToString(),
                        "Confirm pending non-enemy selection.",
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
                        runtimePendingNonEnemyObserver,
                        Array.Empty<KnownRecipeHint>(),
                        Array.Empty<EventKnowledgeCandidate>(),
                        pendingNonEnemySlot3Knowledge,
                        new[] { "confirm selected non-enemy card", "click end turn", "wait" },
                        pendingNonEnemySlot3History,
                        "Confirm pending non-enemy selection.",
                        null),
                    new GuiSmokeStepDecision("act", "confirm-non-enemy", null, null, null, "confirm selected non-enemy card", "test", 0.5, "combat", 0, true, null),
                    out var confirmSemanticAction)
                && string.Equals(confirmSemanticAction, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase),
                "Dedicated non-enemy confirm action should map to explicit confirm semantics.");

            var staleConfirmAfterObserver = runtimePendingNonEnemyObserver with
            {
                InventoryId = "inv-runtime-pending-non-enemy-stale-clear",
                SceneEpisodeId = "episode-runtime-pending-non-enemy-stale-clear",
                Meta = new Dictionary<string, string?>(runtimePendingNonEnemyObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "false",
                    ["combatSelectedCardSlot"] = null,
                    ["combatLastCardPlayFinishedCardId"] = "CARD.STRIKE_IRONCLAD",
                },
            };
            var staleConfirmProgress = EvaluateStepProgress(
                25,
                GuiSmokePhase.HandleCombat,
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                new ObserverState(runtimePendingNonEnemyObserver, null, null, null),
                new ObserverState(staleConfirmAfterObserver, null, null, null),
                new GuiSmokeStepDecision("act", "confirm-non-enemy", null, null, null, "confirm selected non-enemy card", "test", 0.5, "combat", 0, true, null),
                false,
                "tactical",
                false,
                0);
            Assert(!staleConfirmProgress.ActuatorSignals.Contains("non-enemy-confirmed", StringComparer.OrdinalIgnoreCase), "Stale pending clear without energy, hand, or finished-card delta should not count as non-enemy confirm.");

            var consumedConfirmAfterObserver = runtimePendingNonEnemyObserver with
            {
                InventoryId = "inv-runtime-pending-non-enemy-consumed",
                SceneEpisodeId = "episode-runtime-pending-non-enemy-consumed",
                PlayerEnergy = 1,
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(1, "CARD.POOR_SLEEP", "Curse", null),
                    new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", null),
                },
                Meta = new Dictionary<string, string?>(runtimePendingNonEnemyObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "false",
                    ["combatSelectedCardSlot"] = null,
                    ["combatLastCardPlayFinishedCardId"] = "CARD.DEFEND_IRONCLAD",
                },
            };
            var consumedConfirmProgress = EvaluateStepProgress(
                26,
                GuiSmokePhase.HandleCombat,
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                new ObserverState(runtimePendingNonEnemyObserver, null, null, null),
                new ObserverState(consumedConfirmAfterObserver, null, null, null),
                new GuiSmokeStepDecision("act", "confirm-non-enemy", null, null, null, "confirm selected non-enemy card", "test", 0.5, "combat", 0, true, null),
                false,
                "tactical",
                false,
                0);
            Assert(consumedConfirmProgress.ActuatorSignals.Contains("non-enemy-confirmed", StringComparer.OrdinalIgnoreCase), "Energy, hand, or finished-card delta on the confirm step should count as non-enemy confirm.");

            var runtimeStateOnlyScreenshotPath = Path.Combine(Path.GetTempPath(), "sts2-runtime-state-only-self-test-missing.png");
            var runtimeAttackSelectionObserver = pendingNonEnemySlot3Observer with
            {
                InventoryId = "inv-runtime-attack-selection",
                SceneEpisodeId = "episode-runtime-attack-selection",
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                },
                ActionNodes = Array.Empty<ObserverActionNode>(),
                Choices = Array.Empty<ObserverChoice>(),
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                    ["combatCardPlayPending"] = "true",
                    ["combatPlayMode"] = "Combat",
                    ["combatSelectedCardSlot"] = "2",
                    ["combatSelectedCardType"] = "Attack",
                    ["combatSelectedCardTargetType"] = "AnyEnemy",
                    ["combatTargetingInProgress"] = "false",
                },
            };
            var runtimeTargetingKnowledge = new[]
            {
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var runtimeAttackSelectionHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            var runtimeAttackSelectionActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimeAttackSelectionObserver, null, null, null),
                runtimeTargetingKnowledge,
                runtimeStateOnlyScreenshotPath,
                runtimeAttackSelectionHistory);
            Assert(!runtimeAttackSelectionActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Runtime attack selection without targeting evidence should keep click-enemy closed.");
            var runtimeAttackSelectionDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                26,
                GuiSmokePhase.HandleCombat.ToString(),
                "Do not treat runtime attack selection alone as target-ready.",
                DateTimeOffset.UtcNow,
                runtimeStateOnlyScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                runtimeAttackSelectionObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                runtimeTargetingKnowledge,
                runtimeAttackSelectionActions,
                runtimeAttackSelectionHistory,
                "Keep selection and targeting separate when runtime only shows a selected attack.",
                null));
            Assert(runtimeAttackSelectionDecision.TargetLabel?.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase) != true
                   && runtimeAttackSelectionDecision.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) != true,
                "Runtime attack selection without targeting evidence should not drive an enemy-target click.");

            var runtimeTargetingObserver = runtimeAttackSelectionObserver with
            {
                InventoryId = "inv-runtime-targeting",
                SceneEpisodeId = "episode-runtime-targeting",
                Meta = new Dictionary<string, string?>(runtimeAttackSelectionObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatTargetingInProgress"] = "true",
                },
            };
            var runtimeTargetingActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimeTargetingObserver, null, null, null),
                runtimeTargetingKnowledge,
                runtimeStateOnlyScreenshotPath,
                runtimeAttackSelectionHistory);
            Assert(runtimeTargetingActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Runtime targeting state should open click-enemy even when screenshot arrow evidence is absent.");
            var runtimeTargetingDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                27,
                GuiSmokePhase.HandleCombat.ToString(),
                "Runtime targeting state should drive an actual enemy-target lane.",
                DateTimeOffset.UtcNow,
                runtimeStateOnlyScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                runtimeTargetingObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                runtimeTargetingKnowledge,
                runtimeTargetingActions,
                runtimeAttackSelectionHistory,
                "Use runtime targeting metadata before screenshot heuristics.",
                null));
            Assert(runtimeTargetingDecision.TargetLabel?.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase) == true
                   || runtimeTargetingDecision.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) == true,
                "Runtime targeting metadata should drive an enemy-target decision, not reopen card selection.");

            var runtimeHittableEnemyObserver = runtimeTargetingObserver with
            {
                InventoryId = "inv-runtime-hittable-enemy",
                SceneEpisodeId = "episode-runtime-hittable-enemy",
                ActionNodes = new[]
                {
                    new ObserverActionNode("enemy-target:jaw-worm:1", "enemy-target", "Jaw Worm", "720,180,180,260", true)
                    {
                        TypeName = "enemy-target",
                        SemanticHints = new[] { "combat-targetable", "target-id:MONSTER.JAW_WORM", "is-hittable:false", "hook-hitting:false" },
                    },
                    new ObserverActionNode("enemy-target:cultist:2", "enemy-target", "Cultist", "930,210,180,250", true)
                    {
                        TypeName = "enemy-target",
                        SemanticHints = new[] { "combat-targetable", "combat-hittable", "target-id:MONSTER.CULTIST", "is-hittable:true", "hook-hitting:true" },
                    },
                },
                Meta = new Dictionary<string, string?>(runtimeTargetingObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatTargetingInProgress"] = "true",
                    ["combatValidTargetsType"] = "AnyEnemy",
                    ["combatTargetableEnemyCount"] = "2",
                    ["combatTargetableEnemyIds"] = "MONSTER.JAW_WORM,MONSTER.CULTIST",
                    ["combatHittableEnemyCount"] = "1",
                    ["combatHittableEnemyIds"] = "MONSTER.CULTIST",
                    ["combatHoveredTargetId"] = "MONSTER.JAW_WORM",
                    ["combatHoveredTargetLabel"] = "Jaw Worm",
                    ["combatHoveredTargetIsHittable"] = "false",
                },
            };
            var runtimeHittableEnemyActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimeHittableEnemyObserver, null, null, null),
                runtimeTargetingKnowledge,
                runtimeStateOnlyScreenshotPath,
                runtimeAttackSelectionHistory);
            Assert(runtimeHittableEnemyActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Explicit runtime hittability with one allowed enemy should keep the click-enemy lane open.");
            var runtimeHittableEnemyDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                28,
                GuiSmokePhase.HandleCombat.ToString(),
                "Choose only the enemy that runtime still marks as hittable.",
                DateTimeOffset.UtcNow,
                runtimeStateOnlyScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                runtimeHittableEnemyObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                runtimeTargetingKnowledge,
                runtimeHittableEnemyActions,
                runtimeAttackSelectionHistory,
                "Runtime hittability should exclude enemies that remain targetable but are not actually hover-valid or hittable.",
                null));
            Assert(runtimeHittableEnemyDecision.TargetLabel?.Contains("Cultist", StringComparison.OrdinalIgnoreCase) == true, "Runtime hittable-enemy metadata should select the only actionable enemy.");

            var runtimeUnhittableEnemyObserver = runtimeTargetingObserver with
            {
                InventoryId = "inv-runtime-unhittable-enemies",
                SceneEpisodeId = "episode-runtime-unhittable-enemies",
                ActionNodes = new[]
                {
                    new ObserverActionNode("enemy-target:jaw-worm:1", "enemy-target", "Jaw Worm", "720,180,180,260", true)
                    {
                        TypeName = "enemy-target",
                        SemanticHints = new[] { "combat-targetable", "target-id:MONSTER.JAW_WORM", "is-hittable:false", "hook-hitting:false" },
                    },
                },
                Meta = new Dictionary<string, string?>(runtimeTargetingObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatTargetingInProgress"] = "true",
                    ["combatValidTargetsType"] = "AnyEnemy",
                    ["combatTargetableEnemyCount"] = "1",
                    ["combatTargetableEnemyIds"] = "MONSTER.JAW_WORM",
                    ["combatHittableEnemyCount"] = "0",
                    ["combatHittableEnemyIds"] = null,
                    ["combatHoveredTargetId"] = "MONSTER.JAW_WORM",
                    ["combatHoveredTargetLabel"] = "Jaw Worm",
                    ["combatHoveredTargetIsHittable"] = "false",
                },
            };
            var runtimeUnhittableEnemyActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimeUnhittableEnemyObserver, null, null, null),
                runtimeTargetingKnowledge,
                combatNoOpScreenshotPath,
                runtimeAttackSelectionHistory);
            Assert(!runtimeUnhittableEnemyActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Explicit runtime hittability count=0 should keep click-enemy closed even if a stale target node or target arrow remains visible.");
            var runtimeUnhittableEnemyDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                29,
                GuiSmokePhase.HandleCombat.ToString(),
                "Do not attack when runtime explicitly reports that no enemy is hittable.",
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
                runtimeUnhittableEnemyObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                runtimeTargetingKnowledge,
                runtimeUnhittableEnemyActions,
                runtimeAttackSelectionHistory,
                "Explicit runtime hittability=0 should block screenshot-only enemy clicks.",
                null));
            Assert(runtimeUnhittableEnemyDecision.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) != true
                   && runtimeUnhittableEnemyDecision.TargetLabel?.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase) != true,
                "Explicit runtime hittability=0 should prevent combat enemy clicks.");

            var stalePendingAttackHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-2)),
            };
            var runtimeFinishedSelectionObserver = pendingNonEnemySlot3Observer with
            {
                InventoryId = "inv-runtime-finished-selection",
                SceneEpisodeId = "episode-runtime-finished-selection",
                ActionNodes = Array.Empty<ObserverActionNode>(),
                Choices = Array.Empty<ObserverChoice>(),
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                },
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                    ["combatCardPlayPending"] = "false",
                    ["combatTargetingInProgress"] = "false",
                    ["combatLastCardPlayFinishedCardId"] = "CARD.STRIKE_IRONCLAD",
                },
            };
            var runtimeFinishedSelectionKnowledge = new[]
            {
                new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            };
            Assert(CombatRuntimeStateSupport.ResolvePendingSelection(
                    runtimeFinishedSelectionObserver,
                    runtimeFinishedSelectionKnowledge,
                    CombatHistorySupport.TryGetPendingCombatSelection(stalePendingAttackHistory)) is null,
                "Runtime finished-card state should clear stale pending combat selection carryover.");
            var runtimeFinishedSelectionActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimeFinishedSelectionObserver, null, null, null),
                runtimeFinishedSelectionKnowledge,
                runtimeStateOnlyScreenshotPath,
                stalePendingAttackHistory);
            Assert(!runtimeFinishedSelectionActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Runtime cleared selection should keep stale enemy-target actions closed.");
            var runtimeFinishedSelectionDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                28,
                GuiSmokePhase.HandleCombat.ToString(),
                "Runtime finished-card state should clear stale pending attack history before choosing the next lane.",
                DateTimeOffset.UtcNow,
                runtimeStateOnlyScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                runtimeFinishedSelectionObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                runtimeFinishedSelectionKnowledge,
                runtimeFinishedSelectionActions,
                stalePendingAttackHistory,
                "Clear stale runtime attack carryover before selecting the next legal lane.",
                null));
            Assert(string.Equals(runtimeFinishedSelectionDecision.TargetLabel, "combat select non-enemy slot 2", StringComparison.OrdinalIgnoreCase), "Runtime cleared selection should drive a fresh non-enemy selection instead of stale enemy-target carryover.");

            var parityCombatObserver = new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-handle-combat-parity",
                true,
                "mixed",
                "stable",
                "episode-handle-combat-parity",
                "Monster",
                "combat-targets",
                73,
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
                    new ObservedCombatHandCard(1, "CARD.POOR_SLEEP", "Curse", null),
                    new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", null),
                    new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", null),
                    new ObservedCombatHandCard(4, "CARD.STRIKE_IRONCLAD", "Attack", null),
                })
            {
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                },
            };
            var parityCombatKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.POOR_SLEEP", "Curse", "None", -1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var parityFullCombatHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target 압축벌레", DateTimeOffset.UtcNow.AddSeconds(-12)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "auto-end turn", DateTimeOffset.UtcNow.AddSeconds(-11)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "wait", null, DateTimeOffset.UtcNow.AddSeconds(-10)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-9)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 4", DateTimeOffset.UtcNow.AddSeconds(-8)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-7)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 4", DateTimeOffset.UtcNow.AddSeconds(-6)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-5)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 2", DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-3)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 2", DateTimeOffset.UtcNow.AddSeconds(-2)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 2", DateTimeOffset.UtcNow.AddSeconds(-1)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 3", DateTimeOffset.UtcNow),
            };
            var paritySerializedHistory = BuildSerializedStepHistory(GuiSmokePhase.HandleCombat, parityFullCombatHistory);
            Assert(paritySerializedHistory.Count == HandleCombatContextSupport.SerializedHistoryWindow, "HandleCombat request serialization should retain the last 12 combat entries for parity rebuilds.");
            Assert(paritySerializedHistory.All(entry => string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)), "HandleCombat request serialization should only persist combat-phase history.");
            var parityObserverState = new ObserverState(parityCombatObserver, null, null, null);
            var parityAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                parityObserverState,
                parityCombatKnowledge,
                combatNoOpScreenshotPath,
                paritySerializedHistory);
            Assert(!parityAllowedActions.Contains("select attack slot 2", StringComparer.OrdinalIgnoreCase), "Step19-like parity regression should keep blocked attack slot 2 closed.");
            Assert(!parityAllowedActions.Contains("select attack slot 4", StringComparer.OrdinalIgnoreCase), "Step19-like parity regression should keep blocked attack slot 4 closed.");
            var parityRequest = new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                19,
                GuiSmokePhase.HandleCombat.ToString(),
                "Preserve enough combat history for live/rebuild parity.",
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
                parityCombatObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                parityCombatKnowledge,
                parityAllowedActions,
                paritySerializedHistory,
                BuildFailureModeHintCore(
                    GuiSmokePhase.HandleCombat,
                    parityObserverState,
                    parityCombatKnowledge,
                    combatNoOpScreenshotPath,
                    paritySerializedHistory),
                null);
            File.WriteAllText(handleCombatParityRequestPath, JsonSerializer.Serialize(parityRequest, GuiSmokeShared.JsonOptions), Encoding.UTF8);
            var parityNonRebuild = LoadReplayRequest(handleCombatParityRequestPath, fullRequestRebuild: false).Request;
            var parityRebuilt = LoadReplayRequest(handleCombatParityRequestPath, fullRequestRebuild: true).Request;
            Assert(parityNonRebuild.AllowedActions.OrderBy(static action => action, StringComparer.OrdinalIgnoreCase)
                   .SequenceEqual(parityRebuilt.AllowedActions.OrderBy(static action => action, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase),
                "Saved-vs-rebuilt HandleCombat parity should preserve the same allowlist on the synthetic step19-like request.");
            var parityNonRebuildDecision = EvaluateAutoDecisionWithDiagnostics(handleCombatParityRequestPath, parityNonRebuild).Decision;
            var parityRebuiltDecision = EvaluateAutoDecisionWithDiagnostics(handleCombatParityRequestPath, parityRebuilt).Decision;
            Assert(CombatDecisionContract.TryMapSemanticAction(parityNonRebuild, parityNonRebuildDecision, out var parityNonRebuildSemantic), "Saved synthetic HandleCombat parity request should map to a combat semantic action.");
            Assert(CombatDecisionContract.TryMapSemanticAction(parityRebuilt, parityRebuiltDecision, out var parityRebuiltSemantic), "Rebuilt synthetic HandleCombat parity request should map to a combat semantic action.");
            Assert(string.Equals(parityNonRebuildSemantic, parityRebuiltSemantic, StringComparison.OrdinalIgnoreCase), "Step19-like synthetic parity regression should keep saved and rebuilt final semantics aligned.");
            Assert(string.Equals(parityNonRebuildSemantic, "click end turn", StringComparison.OrdinalIgnoreCase), "Step19-like synthetic parity regression should still close on end turn.");
            Assert(!string.Equals(parityRebuiltDecision.TargetLabel, "combat select attack slot 2", StringComparison.OrdinalIgnoreCase), "Step19-like synthetic parity regression should not rebuild to illegal attack slot 2.");

            var combatFastPathContext = CreateStepAnalysisContext(
                GuiSmokePhase.HandleCombat,
                parityObserverState,
                combatNoOpScreenshotPath,
                paritySerializedHistory,
                parityCombatKnowledge);
            Assert(combatFastPathContext.UseCombatFastPath, "Explicit combat authority without contradictory room ownership should enable the combat fast path.");
            var combatFastPathSignature = ComputeSceneSignatureCore(combatNoOpScreenshotPath, parityObserverState, GuiSmokePhase.HandleCombat, combatFastPathContext);
            Assert(combatFastPathSignature.Contains("combat:fast-path", StringComparison.OrdinalIgnoreCase), "Combat fast path scene signatures should mark the fast-path contract explicitly.");
            Assert(!combatFastPathSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase), "Combat fast path scene signatures should not add reward/map contamination layers.");
            Assert(!combatFastPathSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase), "Combat fast path scene signatures should not add screenshot map-arrow contamination.");
            var parityContextAnalysis = AutoDecisionProvider.Analyze(parityRequest, analysisContext: CreateRequestAnalysisContext(parityRequest));
            Assert(CombatDecisionContract.TryMapSemanticAction(parityRequest, parityContextAnalysis.FinalDecision, out var parityContextSemantic), "Context-backed combat analysis should still map to a legal combat semantic action.");
            Assert(string.Equals(parityContextSemantic, "click end turn", StringComparison.OrdinalIgnoreCase), "Context-backed combat analysis should preserve the same HandleCombat parity outcome.");

            GuiSmokeStepRequest BuildBarrierRequest(
                string stepId,
                int stepNumber,
                ObserverSummary barrierObserver,
                IReadOnlyList<CombatCardKnowledgeHint> barrierKnowledge,
                IReadOnlyList<string> barrierAllowedActions,
                IReadOnlyList<GuiSmokeHistoryEntry> barrierHistory,
                string hint)
            {
                return new GuiSmokeStepRequest(
                    "run",
                    "boot-to-long-run",
                    stepNumber,
                    GuiSmokePhase.HandleCombat.ToString(),
                    "Combat barrier self-test request.",
                    DateTimeOffset.UtcNow,
                    runtimeStateOnlyScreenshotPath,
                    new WindowBounds(1, 32, 1280, 720),
                    "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                    stepId,
                    1,
                    3,
                    false,
                    "tactical",
                    null,
                    barrierObserver,
                    Array.Empty<KnownRecipeHint>(),
                    Array.Empty<EventKnowledgeCandidate>(),
                    barrierKnowledge,
                    barrierAllowedActions.ToArray(),
                    barrierHistory,
                    hint,
                    null);
            }

            var combatBarrierCapturedAt = DateTimeOffset.UtcNow;
            var enemyClickBarrierObserver = new ObserverSummary(
                "combat",
                "combat",
                true,
                combatBarrierCapturedAt,
                "inv-enemy-click-barrier",
                true,
                "hook",
                "stable",
                "episode-enemy-click-barrier",
                "Monster",
                "generic",
                70,
                80,
                1,
                new[] { "3턴 종료" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true),
                },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                })
            {
                SnapshotVersion = 10,
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "true",
                    ["combatSelectedCardSlot"] = "2",
                    ["combatSelectedCardType"] = "Attack",
                    ["combatSelectedCardTargetType"] = "AnyEnemy",
                    ["combatTargetingInProgress"] = "true",
                    ["combatHistoryStartedCount"] = "1",
                    ["combatHistoryFinishedCount"] = "0",
                    ["combatInteractionRevision"] = "1:0:true:true:2",
                },
            };
            var enemyBarrierKnowledge = new[]
            {
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var enemyBarrierSeedRequest = BuildBarrierRequest(
                "0003",
                30,
                enemyClickBarrierObserver,
                enemyBarrierKnowledge,
                new[] { "wait" },
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Seed enemy click barrier metadata.");
            var enemyBarrierMetadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(
                enemyBarrierSeedRequest,
                new GuiSmokeStepDecision("act", "click", null, 0.5, 0.5, "combat enemy target Cultist", "seed", 0.9, "combat", 300, true, null));
            var enemyBarrierHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target Cultist", DateTimeOffset.UtcNow)
                {
                    Metadata = enemyBarrierMetadata,
                },
            };
            var enemyBarrierActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(enemyClickBarrierObserver, null, null, null),
                enemyBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                enemyBarrierHistory);
            Assert(enemyBarrierActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase), "EnemyClick hard barrier should collapse the allowlist to wait until a fresh authoritative snapshot arrives.");
            var enemyBarrierRequest = BuildBarrierRequest(
                "0004",
                31,
                enemyClickBarrierObserver,
                enemyBarrierKnowledge,
                enemyBarrierActions,
                enemyBarrierHistory,
                "EnemyClick barrier should wait locally before another provider decision.");
            var enemyBarrierDecision = AutoDecisionProvider.Decide(enemyBarrierRequest);
            Assert(string.Equals(enemyBarrierDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                   && enemyBarrierDecision.Reason?.Contains("combat barrier wait", StringComparison.OrdinalIgnoreCase) == true,
                "EnemyClick hard barrier should synthesize a combat barrier wait decision.");
            Assert(CombatBarrierSupport.TryClassifyWaitPlateau(
                    enemyBarrierRequest,
                    CreateRequestAnalysisContext(enemyBarrierRequest),
                    4,
                    out var barrierWaitCause,
                    out _)
                   && string.Equals(barrierWaitCause, "combat-barrier-wait-plateau", StringComparison.OrdinalIgnoreCase),
                "Repeated unresolved hard-barrier waits should classify as combat-barrier-wait-plateau.");

            var freshEnemyBarrierObserver = enemyClickBarrierObserver with
            {
                CapturedAt = combatBarrierCapturedAt.AddMilliseconds(250),
                InventoryId = "inv-enemy-click-barrier-fresh",
                SnapshotVersion = 11,
                Meta = new Dictionary<string, string?>(enemyClickBarrierObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "false",
                    ["combatTargetingInProgress"] = "false",
                    ["combatHistoryFinishedCount"] = "1",
                    ["combatInteractionRevision"] = "1:1:false:false:none",
                    ["combatLastCardPlayFinishedCardId"] = "CARD.STRIKE_IRONCLAD",
                },
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                },
            };
            Assert(!CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    new ObserverState(freshEnemyBarrierObserver, null, null, null),
                    runtimeStateOnlyScreenshotPath,
                    enemyBarrierHistory,
                    new[] { new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test") })
                    .CombatBarrierEvaluation.IsActive,
                "EnemyClick barrier should release once a fresh finished-card snapshot arrives.");

            var attackBarrierCapturedAt = DateTimeOffset.UtcNow;
            var attackBarrierObserver = new ObserverSummary(
                "combat",
                "combat",
                true,
                attackBarrierCapturedAt,
                "inv-attack-barrier",
                true,
                "hook",
                "stable",
                "episode-attack-barrier",
                "Monster",
                "combat",
                74,
                80,
                2,
                new[] { "3턴 종료" },
                Array.Empty<string>(),
                new[] { new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true) },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    new ObservedCombatHandCard(4, "CARD.BASH", "Attack", 2),
                })
            {
                SnapshotVersion = 20,
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "true",
                    ["combatSelectedCardSlot"] = "2",
                    ["combatSelectedCardType"] = "Attack",
                    ["combatSelectedCardTargetType"] = "AnyEnemy",
                    ["combatTargetingInProgress"] = "false",
                    ["combatHistoryStartedCount"] = "3",
                    ["combatHistoryFinishedCount"] = "2",
                    ["combatInteractionRevision"] = "3:2:true:false:2",
                },
            };
            var attackBarrierKnowledge = new[]
            {
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(4, "CARD.BASH", "Attack", "AnyEnemy", 2, "self-test"),
            };
            var attackBarrierSeedRequest = BuildBarrierRequest("0005", 32, attackBarrierObserver, attackBarrierKnowledge, new[] { "wait" }, Array.Empty<GuiSmokeHistoryEntry>(), "Seed attack barrier metadata.");
            var attackBarrierHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow)
                {
                    Metadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(
                        attackBarrierSeedRequest,
                        new GuiSmokeStepDecision("act", "press-key", "2", null, null, "combat select attack slot 2", "seed", 0.8, "combat", 120, true, null)),
                },
            };
            var attackBarrierActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(attackBarrierObserver, null, null, null),
                attackBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                attackBarrierHistory);
            Assert(!attackBarrierActions.Contains("select attack slot 2", StringComparer.OrdinalIgnoreCase), "AttackSelect barrier should suppress same-slot attack reissue while unresolved.");
            Assert(attackBarrierActions.Contains("select attack slot 4", StringComparer.OrdinalIgnoreCase), "AttackSelect barrier should keep alternate legal attack lanes available.");
            Assert(attackBarrierActions.Contains("click end turn", StringComparer.OrdinalIgnoreCase), "AttackSelect barrier should keep end-turn fallback available.");

            var attackBarrierReleasedObserver = attackBarrierObserver with
            {
                CapturedAt = attackBarrierCapturedAt.AddMilliseconds(250),
                InventoryId = "inv-attack-barrier-released",
                SnapshotVersion = 21,
                ChoiceExtractorPath = "combat-targets",
                CurrentChoices = new[] { "Cultist" },
                ActionNodes = new[]
                {
                    new ObserverActionNode("enemy-target:cultist:1", "enemy-target", "Cultist", "930,210,180,250", true),
                },
                Choices = new[]
                {
                    new ObserverChoice("enemy-target", "Cultist", "930,210,180,250", "MONSTER.CULTIST", "Enemy target")
                    {
                        NodeId = "enemy-target:cultist:1",
                        Enabled = true,
                    },
                },
                Meta = new Dictionary<string, string?>(attackBarrierObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "true",
                    ["combatSelectedCardSlot"] = "2",
                    ["combatSelectedCardType"] = "Attack",
                    ["combatSelectedCardTargetType"] = "AnyEnemy",
                    ["combatTargetingInProgress"] = "true",
                    ["combatTargetableEnemyCount"] = "1",
                    ["combatHittableEnemyCount"] = "1",
                    ["combatInteractionRevision"] = "3:2:true:true:2",
                },
            };
            var attackBarrierReleasedActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(attackBarrierReleasedObserver, null, null, null),
                attackBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                attackBarrierHistory);
            Assert(attackBarrierReleasedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "AttackSelect barrier should release as soon as explicit target authority appears.");

            var nonEnemyBarrierCapturedAt = DateTimeOffset.UtcNow;
            var nonEnemyBarrierObserver = new ObserverSummary(
                "combat",
                "combat",
                true,
                nonEnemyBarrierCapturedAt,
                "inv-non-enemy-barrier",
                true,
                "hook",
                "stable",
                "episode-non-enemy-barrier",
                "Monster",
                "combat",
                78,
                80,
                1,
                new[] { "3턴 종료" },
                Array.Empty<string>(),
                new[] { new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true) },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                })
            {
                SnapshotVersion = 30,
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "true",
                    ["combatTargetingInProgress"] = "false",
                    ["combatHistoryStartedCount"] = "4",
                    ["combatHistoryFinishedCount"] = "2",
                    ["combatInteractionRevision"] = "4:2:true:false:none",
                },
            };
            var nonEnemyBarrierKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            };
            var nonEnemyBarrierSeedRequest = BuildBarrierRequest("0006", 33, nonEnemyBarrierObserver, nonEnemyBarrierKnowledge, new[] { "wait" }, Array.Empty<GuiSmokeHistoryEntry>(), "Seed non-enemy barrier metadata.");
            var nonEnemyBarrierHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 1", DateTimeOffset.UtcNow)
                {
                    Metadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(
                        nonEnemyBarrierSeedRequest,
                        new GuiSmokeStepDecision("act", "press-key", "1", null, null, "combat select non-enemy slot 1", "seed", 0.8, "combat", 120, true, null)),
                },
            };
            var nonEnemyBarrierActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(nonEnemyBarrierObserver, null, null, null),
                nonEnemyBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                nonEnemyBarrierHistory);
            Assert(!nonEnemyBarrierActions.Contains("select non-enemy slot 1", StringComparer.OrdinalIgnoreCase), "NonEnemySelect barrier should suppress same-slot non-enemy reissue while unresolved.");
            Assert(nonEnemyBarrierActions.Contains("select non-enemy slot 3", StringComparer.OrdinalIgnoreCase), "NonEnemySelect barrier should keep alternate non-enemy slots available.");
            Assert(nonEnemyBarrierActions.Contains("click end turn", StringComparer.OrdinalIgnoreCase), "NonEnemySelect barrier should keep end-turn fallback available.");

            var endTurnBarrierCapturedAt = DateTimeOffset.UtcNow;
            var endTurnBarrierKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            };
            var endTurnBarrierSeedObserver = new ObserverSummary(
                "combat",
                "combat",
                true,
                endTurnBarrierCapturedAt,
                "inv-end-turn-barrier-seed",
                true,
                "hook",
                "stable",
                "episode-end-turn-barrier",
                "Monster",
                "combat",
                82,
                80,
                2,
                new[] { "2턴 종료" },
                Array.Empty<string>(),
                new[] { new ObserverActionNode("end-turn", "button", "2턴 종료", "1604,846,220,90", true) },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                })
            {
                SnapshotVersion = 40,
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatRoundNumber"] = "2",
                    ["combatPlayerActionsDisabled"] = "false",
                    ["combatEndingPlayerTurnPhaseOne"] = "false",
                    ["combatEndingPlayerTurnPhaseTwo"] = "false",
                    ["combatHistoryStartedCount"] = "5",
                    ["combatHistoryFinishedCount"] = "5",
                    ["combatInteractionRevision"] = "5:5:false:false:none",
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false",
                },
            };
            var endTurnBarrierSeedRequest = BuildBarrierRequest(
                "0007",
                34,
                endTurnBarrierSeedObserver,
                endTurnBarrierKnowledge,
                new[] { "wait" },
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Seed end-turn barrier metadata.");
            var endTurnBarrierMetadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(
                endTurnBarrierSeedRequest,
                new GuiSmokeStepDecision("act", "press-key", "E", null, null, "auto-end turn", "seed", 0.8, "combat", 120, true, null));
            Assert(endTurnBarrierMetadata is not null, "EndTurn barrier seed metadata should serialize the armed round.");
            var endTurnBarrierHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "auto-end turn", DateTimeOffset.UtcNow)
                {
                    Metadata = endTurnBarrierMetadata,
                },
            };
            var endTurnBarrierAckObserver = endTurnBarrierSeedObserver with
            {
                CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(250),
                InventoryId = "inv-end-turn-barrier-ack",
                SnapshotVersion = 41,
                Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatRoundNumber"] = "2",
                    ["combatPlayerActionsDisabled"] = "true",
                    ["combatEndingPlayerTurnPhaseOne"] = "true",
                    ["combatEndingPlayerTurnPhaseTwo"] = "false",
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false",
                },
            };
            var endTurnBarrierAckActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(endTurnBarrierAckObserver, null, null, null),
                endTurnBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                endTurnBarrierHistory);
            Assert(endTurnBarrierAckActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase), "EndTurn barrier should stay wait-only after acknowledgement but before the next round reopens.");
            Assert(CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    new ObserverState(endTurnBarrierAckObserver, null, null, null),
                    runtimeStateOnlyScreenshotPath,
                    endTurnBarrierHistory,
                    endTurnBarrierKnowledge)
                    .CombatBarrierEvaluation.IsActive,
                "EndTurn barrier should remain active during the acknowledged closed-window band.");
            Assert(!CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    new ObserverState(endTurnBarrierAckObserver, null, null, null),
                    runtimeStateOnlyScreenshotPath,
                    endTurnBarrierHistory,
                    endTurnBarrierKnowledge)
                    .CombatBarrierEvaluation.OverWaitRisk,
                "Acknowledged EndTurn transit should not be flagged as an over-wait plateau risk by itself.");

            var endTurnTransitProgressObservers = new[]
            {
                endTurnBarrierSeedObserver with
                {
                    CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(250),
                    InventoryId = "inv-end-turn-transit-1",
                    SnapshotVersion = 53,
                    Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatRoundNumber"] = "1",
                        ["combatPlayerActionsDisabled"] = "true",
                        ["combatEndingPlayerTurnPhaseOne"] = "false",
                        ["combatEndingPlayerTurnPhaseTwo"] = "true",
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false",
                    },
                },
                endTurnBarrierSeedObserver with
                {
                    CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(500),
                    InventoryId = "inv-end-turn-transit-2",
                    SnapshotVersion = 54,
                    Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatRoundNumber"] = "1",
                        ["combatPlayerActionsDisabled"] = "true",
                        ["combatEndingPlayerTurnPhaseOne"] = "false",
                        ["combatEndingPlayerTurnPhaseTwo"] = "true",
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=true;CombatManager.IsEnding=false",
                    },
                },
                endTurnBarrierSeedObserver with
                {
                    CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(750),
                    InventoryId = "inv-end-turn-transit-3",
                    SnapshotVersion = 55,
                    Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatRoundNumber"] = "2",
                        ["combatPlayerActionsDisabled"] = "false",
                        ["combatEndingPlayerTurnPhaseOne"] = "false",
                        ["combatEndingPlayerTurnPhaseTwo"] = "true",
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=true;CombatManager.IsEnding=false",
                    },
                },
                endTurnBarrierSeedObserver with
                {
                    CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(1000),
                    InventoryId = "inv-end-turn-transit-4",
                    SnapshotVersion = 56,
                    Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatRoundNumber"] = "2",
                        ["combatPlayerActionsDisabled"] = "false",
                        ["combatEndingPlayerTurnPhaseOne"] = "false",
                        ["combatEndingPlayerTurnPhaseTwo"] = "true",
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=true;CombatManager.IsEnding=false",
                    },
                },
            };
            string? lastEndTurnTransitFingerprint = null;
            var endTurnTransitWaitCount = 0;
            for (var index = 0; index < endTurnTransitProgressObservers.Length; index += 1)
            {
                var transitObserverState = new ObserverState(endTurnTransitProgressObservers[index], null, null, null);
                var transitActions = BuildAllowedActions(
                    GuiSmokePhase.HandleCombat,
                    transitObserverState,
                    endTurnBarrierKnowledge,
                    runtimeStateOnlyScreenshotPath,
                    endTurnBarrierHistory);
                Assert(transitActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase), "Acknowledged EndTurn transit should remain wait-only before the player window reopens.");
                var transitRequest = BuildBarrierRequest(
                    "0007-transit",
                    40 + index,
                    endTurnTransitProgressObservers[index],
                    endTurnBarrierKnowledge,
                    transitActions,
                    endTurnBarrierHistory,
                    "Acknowledged EndTurn transit progress should remain a safe wait.");
                var transitContext = CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    transitObserverState,
                    runtimeStateOnlyScreenshotPath,
                    endTurnBarrierHistory,
                    endTurnBarrierKnowledge);
                Assert(transitContext.CombatBarrierEvaluation.IsActive
                       && !transitContext.CombatBarrierEvaluation.OverWaitRisk,
                    "Acknowledged EndTurn transit should stay active without being classified as an over-wait risk.");
                var transitFingerprint = BuildDecisionWaitFingerprint(
                    GuiSmokePhase.HandleCombat,
                    transitRequest.SceneSignature,
                    transitObserverState,
                    transitContext);
                endTurnTransitWaitCount = string.Equals(lastEndTurnTransitFingerprint, transitFingerprint, StringComparison.Ordinal)
                    ? endTurnTransitWaitCount + 1
                    : 1;
                lastEndTurnTransitFingerprint = transitFingerprint;
                Assert(
                    !CombatBarrierSupport.TryClassifyWaitPlateau(transitRequest, transitContext, endTurnTransitWaitCount, out _, out _),
                    "Acknowledged EndTurn transit with authority progress should not classify as combat-barrier-wait-plateau.");
                Assert(
                    !TryClassifyDecisionWaitPlateau(GuiSmokePhase.HandleCombat, transitObserverState, endTurnTransitWaitCount, out _, out _),
                    "Acknowledged EndTurn transit with authority progress should not classify as generic decision-wait-plateau.");
            }

            var frozenEndTurnTransitObserver = endTurnBarrierSeedObserver with
            {
                CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(1200),
                InventoryId = "inv-end-turn-transit-frozen",
                SnapshotVersion = 57,
                Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatRoundNumber"] = "2",
                    ["combatPlayerActionsDisabled"] = "false",
                    ["combatEndingPlayerTurnPhaseOne"] = "false",
                    ["combatEndingPlayerTurnPhaseTwo"] = "true",
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=true;CombatManager.IsEnding=false",
                },
            };
            string? lastFrozenEndTurnTransitFingerprint = null;
            var frozenEndTurnTransitWaitCount = 0;
            GuiSmokeStepRequest? frozenTransitRequest = null;
            GuiSmokeStepAnalysisContext? frozenTransitContext = null;
            for (var index = 0; index < 4; index += 1)
            {
                var frozenObserverState = new ObserverState(frozenEndTurnTransitObserver, null, null, null);
                var frozenActions = BuildAllowedActions(
                    GuiSmokePhase.HandleCombat,
                    frozenObserverState,
                    endTurnBarrierKnowledge,
                    runtimeStateOnlyScreenshotPath,
                    endTurnBarrierHistory);
                frozenTransitRequest = BuildBarrierRequest(
                    "0007-frozen",
                    50 + index,
                    frozenEndTurnTransitObserver,
                    endTurnBarrierKnowledge,
                    frozenActions,
                    endTurnBarrierHistory,
                    "Frozen acknowledged EndTurn transit may still stall.");
                frozenTransitContext = CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    frozenObserverState,
                    runtimeStateOnlyScreenshotPath,
                    endTurnBarrierHistory,
                    endTurnBarrierKnowledge);
                var frozenFingerprint = BuildDecisionWaitFingerprint(
                    GuiSmokePhase.HandleCombat,
                    frozenTransitRequest.SceneSignature,
                    frozenObserverState,
                    frozenTransitContext);
                frozenEndTurnTransitWaitCount = string.Equals(lastFrozenEndTurnTransitFingerprint, frozenFingerprint, StringComparison.Ordinal)
                    ? frozenEndTurnTransitWaitCount + 1
                    : 1;
                lastFrozenEndTurnTransitFingerprint = frozenFingerprint;
            }
            Assert(frozenTransitRequest is not null && frozenTransitContext is not null, "Frozen transit fixtures should be created for stall classification.");
            Assert(
                !CombatBarrierSupport.TryClassifyWaitPlateau(frozenTransitRequest!, frozenTransitContext!, frozenEndTurnTransitWaitCount, out _, out _),
                "Acknowledged EndTurn transit should not use combat-barrier-wait-plateau even when frozen.");
            Assert(
                TryClassifyDecisionWaitPlateau(GuiSmokePhase.HandleCombat, new ObserverState(frozenEndTurnTransitObserver, null, null, null), frozenEndTurnTransitWaitCount, out var frozenTransitCause, out _)
                && string.Equals(frozenTransitCause, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase),
                "Frozen acknowledged EndTurn transit should still be stallable via the generic wait plateau path.");

            var endTurnBarrierReopenedObserver = endTurnBarrierSeedObserver with
            {
                CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(650),
                InventoryId = "inv-end-turn-barrier-reopened",
                SnapshotVersion = 42,
                PlayerEnergy = 3,
                CurrentChoices = new[] { "3턴 종료" },
                ActionNodes = new[] { new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true) },
                Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatRoundNumber"] = "3",
                    ["combatPlayerActionsDisabled"] = "false",
                    ["combatEndingPlayerTurnPhaseOne"] = "false",
                    ["combatEndingPlayerTurnPhaseTwo"] = "false",
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false",
                },
            };
            var endTurnBarrierReopenedActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(endTurnBarrierReopenedObserver, null, null, null),
                endTurnBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                endTurnBarrierHistory);
            Assert(!endTurnBarrierReopenedActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase), "EndTurn barrier should release once the next player turn reopens on a higher round.");
            Assert(CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    new ObserverState(endTurnBarrierReopenedObserver, null, null, null),
                    runtimeStateOnlyScreenshotPath,
                    endTurnBarrierHistory,
                    endTurnBarrierKnowledge)
                    .CombatBarrierEvaluation.IsActive == false,
                "EndTurn barrier should be inactive after round-advanced player-turn reopen.");
            var endTurnBarrierReopenedRequest = BuildBarrierRequest(
                "0008",
                35,
                endTurnBarrierReopenedObserver,
                endTurnBarrierKnowledge,
                endTurnBarrierReopenedActions,
                endTurnBarrierHistory,
                "Release end-turn barrier after the next player turn reopens.");
            var endTurnBarrierReopenedDecision = AutoDecisionProvider.Decide(endTurnBarrierReopenedRequest);
            Assert(!string.Equals(endTurnBarrierReopenedDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                   || endTurnBarrierReopenedDecision.Reason?.Contains("combat barrier wait", StringComparison.OrdinalIgnoreCase) != true,
                "EndTurn barrier release should prevent combat barrier wait reentry on the reopened player turn.");

            var stickyEndTurnBarrierHistory = new[]
            {
                endTurnBarrierHistory[0],
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "wait", null, DateTimeOffset.UtcNow.AddMilliseconds(100)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "wait", null, DateTimeOffset.UtcNow.AddMilliseconds(200)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "wait", null, DateTimeOffset.UtcNow.AddMilliseconds(300)),
            };
            var stickyEndTurnBarrierActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(endTurnBarrierReopenedObserver, null, null, null),
                endTurnBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                stickyEndTurnBarrierHistory);
            Assert(!stickyEndTurnBarrierActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase), "EndTurn sticky release should ignore trailing wait-only history once the round has advanced and player control reopened.");
            Assert(CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    new ObserverState(endTurnBarrierReopenedObserver, null, null, null),
                    runtimeStateOnlyScreenshotPath,
                    stickyEndTurnBarrierHistory,
                    endTurnBarrierKnowledge)
                    .CombatBarrierEvaluation.IsActive == false,
                "EndTurn sticky release should keep the same auto-end-turn source from re-arming after a reopened player turn.");

            var slotAlignmentObserver = new ObserverState(
                new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    DateTimeOffset.UtcNow,
                    "inv-slot-alignment",
                    true,
                    "mixed",
                    "stable",
                    "episode-slot-alignment",
                    "Combat",
                    "combat",
                    80,
                    80,
                    1,
                    new[] { "3턴 종료" },
                    Array.Empty<string>(),
                    new[] { new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true) },
                    Array.Empty<ObserverChoice>(),
                    new[]
                    {
                        new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                        new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                        new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    }),
                null,
                null,
                null);
            var slotAlignmentKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            };
            var slotAllowedActions = BuildAllowedActions(GuiSmokePhase.HandleCombat, slotAlignmentObserver, slotAlignmentKnowledge, combatNoOpScreenshotPath, Array.Empty<GuiSmokeHistoryEntry>());
            Assert(slotAllowedActions.Contains("select attack slot 2", StringComparer.OrdinalIgnoreCase), "Combat allowlist should expose the actual playable attack slot from observer/knowledge alignment.");
            Assert(!slotAllowedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Combat allowlist should not expose enemy targeting before an actual attack selection is confirmed.");
            var slotAlignmentDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                8,
                GuiSmokePhase.HandleCombat.ToString(),
                "Choose a combat action from the aligned slot map.",
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
                slotAlignmentObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                slotAlignmentKnowledge,
                slotAllowedActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Only slot 2 is a playable attack.",
                null));
            Assert(string.Equals(slotAlignmentDecision.TargetLabel, "combat select attack slot 2", StringComparison.OrdinalIgnoreCase), "Combat decisioning should align with observer/knowledge slot 2 instead of drifting to a screenshot-only slot.");

            var combatTargetObserver = new ObserverState(
                new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    DateTimeOffset.UtcNow,
                    "inv-combat-targets",
                    true,
                    "mixed",
                    "stable",
                    "episode-combat-targets",
                    "Combat",
                    "combat",
                    80,
                    80,
                    3,
                    new[] { "Jaw Worm", "Cultist" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("enemy-target:1", "enemy-target", "Jaw Worm", "720,180,180,260", true),
                        new ObserverActionNode("enemy-target:2", "enemy-target", "Cultist", "930,210,180,250", true),
                        new ObserverActionNode("end-turn", "button", "3턴 종료", "1080,620,140,60", true),
                    },
                    Array.Empty<ObserverChoice>(),
                    new[]
                    {
                        new ObservedCombatHandCard(1, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    }),
                null,
                null,
                null);
            var combatTargetDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                9,
                GuiSmokePhase.HandleCombat.ToString(),
                "Prefer actual combat target nodes over fixed normalized enemy anchors.",
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
                combatTargetObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                new[]
                {
                    new CombatCardKnowledgeHint(1, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                },
                new[] { "click card", "click enemy", "click end turn", "wait" },
                new[]
                {
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-2)),
                },
                "Use current-frame enemy target bounds instead of fixed anchors.",
                null));
            Assert(combatTargetDecision.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) == true, "Combat target selection should use exported enemy target nodes instead of fixed normalized labels when current-frame enemy bounds exist.");
            Assert(combatTargetDecision.NormalizedX is > 0.35 and < 0.55, "Enemy target click should resolve from the exported hitbox/body rect, not the old fixed normalized anchor.");

            var combatTargetRetryDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                10,
                GuiSmokePhase.HandleCombat.ToString(),
                "After one no-op target click, try another observed enemy target before giving up.",
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
                combatTargetObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                new[]
                {
                    new CombatCardKnowledgeHint(1, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                },
                new[] { "click card", "click enemy", "click end turn", "wait" },
                new[]
                {
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-5)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target Jaw Worm", DateTimeOffset.UtcNow.AddSeconds(-4)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 1", DateTimeOffset.UtcNow.AddSeconds(-3)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-2)),
                },
                "Try another enemy target before ending the turn.",
                null));
            Assert(combatTargetRetryDecision.TargetLabel?.Contains("Cultist", StringComparison.OrdinalIgnoreCase) == true, "After one no-op enemy click, combat recovery should try another observed enemy target when one is available.");

            var logicalBoundsTargetObserver = new ObserverState(
                new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    DateTimeOffset.UtcNow,
                    "inv-combat-logical-target-bounds",
                    true,
                    "mixed",
                    "stable",
                    "episode-combat-logical-target-bounds",
                    "Combat",
                    "combat",
                    80,
                    80,
                    2,
                    new[] { "나뭇잎 슬라임 (중)" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("enemy-target:slime-medium:2", "enemy-target", "나뭇잎 슬라임 (중)", "1389.943,674.675,72,88", true)
                        {
                            TypeName = "enemy-target",
                            SemanticHints = new[] { "combat-targetable", "combat-hittable", "source:target-manager", "target-id:나뭇잎 슬라임 (중)" },
                        },
                    },
                    Array.Empty<ObserverChoice>(),
                    new[]
                    {
                        new ObservedCombatHandCard(1, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    })
                {
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatCardPlayPending"] = "true",
                        ["combatSelectedCardSlot"] = "1",
                        ["combatSelectedCardType"] = "Attack",
                        ["combatSelectedCardTargetType"] = "AnyEnemy",
                        ["combatTargetingInProgress"] = "true",
                        ["combatTargetableEnemyCount"] = "1",
                        ["combatTargetableEnemyIds"] = "나뭇잎 슬라임 (중)",
                        ["combatHittableEnemyCount"] = "1",
                        ["combatHittableEnemyIds"] = "나뭇잎 슬라임 (중)",
                    },
                },
                null,
                null,
                null);
            var logicalBoundsTargetDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                10,
                GuiSmokePhase.HandleCombat.ToString(),
                "Use logical target-manager bounds even when the absolute target rect lies outside the window-relative overlap.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(8, 48, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                logicalBoundsTargetObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                new[]
                {
                    new CombatCardKnowledgeHint(1, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                },
                new[] { "click enemy", "click end turn", "wait" },
                new[]
                {
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-1)),
                },
                "Prefer explicit target-manager logical bounds over any fixed combat target fallback.",
                null));
            Assert(logicalBoundsTargetDecision.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) == true, "Explicit target-manager nodes with usable logical bounds should produce combat enemy target decisions.");
            Assert(logicalBoundsTargetDecision.TargetLabel?.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase) != true, "Explicit target-manager nodes should suppress the old fixed auto-target fallback.");
            Assert(TryParseNodeBounds("1389.943,674.675,72,88", out var logicalBoundsRect), "Expected valid logical target bounds.");
            var logicalClickX = logicalBoundsTargetDecision.NormalizedX.GetValueOrDefault() * 1920d;
            var logicalClickY = logicalBoundsTargetDecision.NormalizedY.GetValueOrDefault() * 1080d;
            Assert(logicalClickX >= logicalBoundsRect.Left && logicalClickX <= logicalBoundsRect.Right && logicalClickY >= logicalBoundsRect.Top && logicalClickY <= logicalBoundsRect.Bottom, "Explicit combat enemy target click should stay inside the chosen target-manager node bounds.");

            var unusableExplicitTargetObserver = new ObserverState(
                logicalBoundsTargetObserver.Summary with
                {
                    InventoryId = "inv-combat-unusable-explicit-target",
                    SceneEpisodeId = "episode-combat-unusable-explicit-target",
                    ActionNodes = new[]
                    {
                        new ObserverActionNode("enemy-target:slime-medium:2", "enemy-target", "나뭇잎 슬라임 (중)", "2600,680,72,88", true)
                        {
                            TypeName = "enemy-target",
                            SemanticHints = new[] { "combat-targetable", "combat-hittable", "source:target-manager", "target-id:나뭇잎 슬라임 (중)" },
                        },
                    },
                },
                null,
                null,
                null);
            var unusableExplicitTargetDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                11,
                GuiSmokePhase.HandleCombat.ToString(),
                "If explicit combat targeting authority exists but no usable node survives, do not fall back to a blind auto-target click.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(8, 48, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                unusableExplicitTargetObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                new[]
                {
                    new CombatCardKnowledgeHint(1, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                },
                new[] { "click enemy", "click end turn", "wait" },
                new[]
                {
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-1)),
                },
                "Wait for a usable explicit enemy node instead of emitting a blind auto-target click.",
                null));
            Assert(string.Equals(unusableExplicitTargetDecision.Status, "wait", StringComparison.OrdinalIgnoreCase), "Explicit combat targeting authority without usable node bounds should release to wait, not blind click.");
            Assert(unusableExplicitTargetDecision.TargetLabel?.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase) != true, "Explicit runtime authority with unusable target nodes must not emit the fixed auto-target fallback.");

            var noOpFallbackOrderingObserver = new ObserverState(
                new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    DateTimeOffset.UtcNow,
                    "inv-noop-fallback-ordering",
                    true,
                    "mixed",
                    "stable",
                    "episode-noop-fallback-ordering",
                    "Combat",
                    "combat",
                    55,
                    80,
                    2,
                    new[] { "Jaw Worm", "3턴 종료" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("enemy-target:1", "enemy-target", "Jaw Worm", "720,180,180,260", true),
                        new ObserverActionNode("end-turn", "button", "3턴 종료", "1080,620,140,60", true),
                    },
                    Array.Empty<ObserverChoice>(),
                    new[]
                    {
                        new ObservedCombatHandCard(1, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                        new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    }),
                null,
                null,
                null);
            var noOpFallbackOrderingDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                11,
                GuiSmokePhase.HandleCombat.ToString(),
                "Do not end turn while a safe non-enemy play remains after an attack lane no-op.",
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
                noOpFallbackOrderingObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                new[]
                {
                    new CombatCardKnowledgeHint(1, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                    new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                },
                new[] { "click card", "click enemy", "click end turn", "wait" },
                new[]
                {
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-6)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target Jaw Worm", DateTimeOffset.UtcNow.AddSeconds(-5)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 1", DateTimeOffset.UtcNow.AddSeconds(-4)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-3)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target Jaw Worm", DateTimeOffset.UtcNow.AddSeconds(-2)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 1", DateTimeOffset.UtcNow.AddSeconds(-1)),
                },
                "Prefer a remaining safe non-enemy play before ending the turn.",
                null));
            Assert(CombatDecisionContract.TryMapSemanticAction(
                    new GuiSmokeStepRequest(
                        "run",
                        "boot-to-long-run",
                        11,
                        GuiSmokePhase.HandleCombat.ToString(),
                        "Do not end turn while a safe non-enemy play remains after an attack lane no-op.",
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
                        noOpFallbackOrderingObserver.Summary,
                        Array.Empty<KnownRecipeHint>(),
                        Array.Empty<EventKnowledgeCandidate>(),
                        new[]
                        {
                            new CombatCardKnowledgeHint(1, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                            new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                        },
                        new[] { "click card", "click enemy", "click end turn", "wait" },
                        new[]
                        {
                            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-6)),
                            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target Jaw Worm", DateTimeOffset.UtcNow.AddSeconds(-5)),
                            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 1", DateTimeOffset.UtcNow.AddSeconds(-4)),
                            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-3)),
                            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target Jaw Worm", DateTimeOffset.UtcNow.AddSeconds(-2)),
                            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 1", DateTimeOffset.UtcNow.AddSeconds(-1)),
                        },
                        "Prefer a remaining safe non-enemy play before ending the turn.",
                        null),
                    noOpFallbackOrderingDecision,
                    out var noOpFallbackOrderingSemantic)
                && !string.Equals(noOpFallbackOrderingSemantic, "click end turn", StringComparison.OrdinalIgnoreCase),
                "Combat decisioning should not fall straight to end turn while another safe combat fallback still exists after a blocked attack lane.");

            var blockedOpenAttackSelectionObserver = new ObserverState(
                new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    DateTimeOffset.UtcNow,
                    "inv-blocked-open-attack-selection",
                    true,
                    "mixed",
                    "stable",
                    "episode-blocked-open-attack-selection",
                    "Monster",
                    "combat-targets",
                    1,
                    80,
                    2,
                    new[] { "Jaw Worm" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("enemy-target:jaw-worm:1", "enemy-target", "Jaw Worm", "720,180,180,260", true)
                        {
                            TypeName = "enemy-target",
                            SemanticHints = new[] { "combat-targetable", "combat-hittable", "source:target-manager", "target-id:Jaw Worm" },
                        },
                    },
                    Array.Empty<ObserverChoice>(),
                    new[]
                    {
                        new ObservedCombatHandCard(1, "CARD.NEOWS_FURY", "Attack", 1),
                        new ObservedCombatHandCard(2, "CARD.TRUE_GRIT", "Skill", 1),
                        new ObservedCombatHandCard(3, "CARD.SLIMED", "Status", 1),
                    })
                {
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatCardPlayPending"] = "true",
                        ["combatPlayMode"] = "Play",
                        ["combatSelectedCardSlot"] = "1",
                        ["combatSelectedCardType"] = "Attack",
                        ["combatSelectedCardTargetType"] = "AnyEnemy",
                        ["combatTargetingInProgress"] = "true",
                        ["combatTargetableEnemyCount"] = "1",
                        ["combatTargetableEnemyIds"] = "Jaw Worm",
                        ["combatRoundNumber"] = "5",
                        ["combatPlayerActionsDisabled"] = "false",
                        ["combatEndingPlayerTurnPhaseOne"] = "false",
                        ["combatEndingPlayerTurnPhaseTwo"] = "false",
                        ["combatHistoryStartedCount"] = "11",
                        ["combatHistoryFinishedCount"] = "11",
                        ["combatInteractionRevision"] = "11:11:true:true:1",
                    },
                },
                null,
                null,
                null);
            var blockedOpenAttackSelectionHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-6)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target Jaw Worm", DateTimeOffset.UtcNow.AddSeconds(-5)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 1", DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-3)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 1", DateTimeOffset.UtcNow.AddSeconds(-2)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 2", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            var blockedOpenAttackSelectionAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                blockedOpenAttackSelectionObserver,
                new[]
                {
                    new CombatCardKnowledgeHint(1, "CARD.NEOWS_FURY", "Attack", "AnyEnemy", 1, "self-test"),
                    new CombatCardKnowledgeHint(2, "CARD.TRUE_GRIT", "Skill", "Self", 1, "self-test"),
                    new CombatCardKnowledgeHint(3, "CARD.SLIMED", "Status", "None", 1, "self-test"),
                },
                combatNoOpScreenshotPath,
                blockedOpenAttackSelectionHistory);
            Assert(blockedOpenAttackSelectionAllowedActions.Contains("right-click cancel selected card", StringComparer.OrdinalIgnoreCase), "Blocked open attack selections should expose an explicit cancel lane before end turn fallback.");
            var blockedOpenAttackSelectionDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                11,
                GuiSmokePhase.HandleCombat.ToString(),
                "Cancel a blocked open attack selection before falling back to end turn.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable|combat-targeting:active|combat-selection:attacklike|combat-slot:1|combat-play-open",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                blockedOpenAttackSelectionObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                new[]
                {
                    new CombatCardKnowledgeHint(1, "CARD.NEOWS_FURY", "Attack", "AnyEnemy", 1, "self-test"),
                    new CombatCardKnowledgeHint(2, "CARD.TRUE_GRIT", "Skill", "Self", 1, "self-test"),
                    new CombatCardKnowledgeHint(3, "CARD.SLIMED", "Status", "None", 1, "self-test"),
                },
                blockedOpenAttackSelectionAllowedActions,
                blockedOpenAttackSelectionHistory,
                "Cancel a blocked open attack selection before ending the turn.",
                null));
            Assert(CombatDecisionContract.TryMapSemanticAction(
                    new GuiSmokeStepRequest(
                        "run",
                        "boot-to-long-run",
                        11,
                        GuiSmokePhase.HandleCombat.ToString(),
                        "Cancel a blocked open attack selection before falling back to end turn.",
                        DateTimeOffset.UtcNow,
                        combatNoOpScreenshotPath,
                        new WindowBounds(0, 0, 1280, 720),
                        "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable|combat-targeting:active|combat-selection:attacklike|combat-slot:1|combat-play-open",
                        "0001",
                        1,
                        3,
                        false,
                        "tactical",
                        null,
                        blockedOpenAttackSelectionObserver.Summary,
                        Array.Empty<KnownRecipeHint>(),
                        Array.Empty<EventKnowledgeCandidate>(),
                        new[]
                        {
                            new CombatCardKnowledgeHint(1, "CARD.NEOWS_FURY", "Attack", "AnyEnemy", 1, "self-test"),
                            new CombatCardKnowledgeHint(2, "CARD.TRUE_GRIT", "Skill", "Self", 1, "self-test"),
                            new CombatCardKnowledgeHint(3, "CARD.SLIMED", "Status", "None", 1, "self-test"),
                        },
                        blockedOpenAttackSelectionAllowedActions,
                        blockedOpenAttackSelectionHistory,
                        "Cancel a blocked open attack selection before ending the turn.",
                        null),
                    blockedOpenAttackSelectionDecision,
                    out var blockedOpenAttackSelectionSemantic)
                && string.Equals(blockedOpenAttackSelectionSemantic, "right-click cancel selected card", StringComparison.OrdinalIgnoreCase),
                "Blocked open attack selections should cancel the lingering card instead of falling straight to end turn.");

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
            var noEnemyAllowedActions = BuildAllowedActions(GuiSmokePhase.HandleCombat, noEnemyTargetObserver, slotAlignmentKnowledge[..2], combatNoOpScreenshotPath, noEnemyTargetHistory);
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
                slotAlignmentKnowledge[..2],
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
