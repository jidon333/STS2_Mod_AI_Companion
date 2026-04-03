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
    private static void RunCombatContractsNonEnemyAndRuntimeStateSelfTests(string combatNoOpScreenshotPath)
    {
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
            Assert(!pendingNonEnemySlot3Actions.Contains("click end turn", StringComparer.OrdinalIgnoreCase), "Pending non-enemy slot should keep end turn closed until the lane resolves.");
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

            var suppressedNonEnemyConvergenceObserver = new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-suppressed-non-enemy-convergence",
                true,
                "mixed",
                "stable",
                "episode-suppressed-non-enemy-convergence",
                "Monster",
                "combat",
                58,
                80,
                1,
                new[] { "5턴 종료" },
                Array.Empty<string>(),
                new[] { new ObserverActionNode("end-turn", "button", "5턴 종료", "1604,846,220,90", true) },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(1, "CARD.SLIMED", "Status", null),
                    new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", null),
                    new ObservedCombatHandCard(3, "CARD.SLIMED", "Status", null),
                })
            {
                SnapshotVersion = 343,
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                    ["combatCardPlayPending"] = "false",
                    ["combatTargetingInProgress"] = "false",
                    ["combatPlayMode"] = "Play",
                    ["combatHistoryStartedCount"] = "13",
                    ["combatHistoryFinishedCount"] = "13",
                    ["combatInteractionRevision"] = "13:13:false:false:none",
                    ["combatLastCardPlayStartedCardId"] = "CARD.DEFEND_IRONCLAD",
                    ["combatLastCardPlayFinishedCardId"] = "CARD.DEFEND_IRONCLAD",
                    ["combatTargetSummary"] = "enemy-target:슬라임:2@logical:1154,646.6,72,88@normalized:0.601,0.5987,0.0375,0.0815",
                },
            };
            var suppressedNonEnemyConvergenceKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.SLIMED", "Status", "None", -1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.SLIMED", "Status", "None", -1, "self-test"),
            };
            var suppressedNonEnemyConvergenceHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 2", DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "confirm-non-enemy", "confirm selected non-enemy card", DateTimeOffset.UtcNow.AddSeconds(-3)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 2", DateTimeOffset.UtcNow.AddSeconds(-2)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "confirm-non-enemy", "confirm selected non-enemy card", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            var suppressedNonEnemyConvergenceActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(suppressedNonEnemyConvergenceObserver, null, null, null),
                suppressedNonEnemyConvergenceKnowledge,
                combatNoOpScreenshotPath,
                suppressedNonEnemyConvergenceHistory);
            Assert(!suppressedNonEnemyConvergenceActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "A recently suppressed non-enemy lane should not reopen generic enemy targeting from stale target summary residue.");
            Assert(suppressedNonEnemyConvergenceActions.Contains("click end turn", StringComparer.OrdinalIgnoreCase), "Once the non-enemy lane has fully cleared, legacy suppression should not keep end turn closed.");
            var suppressedNonEnemyConvergenceDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                26,
                GuiSmokePhase.HandleCombat.ToString(),
                "Do not let a cleared non-enemy suppression heuristic block the legal end-turn fallback.",
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
                suppressedNonEnemyConvergenceObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                suppressedNonEnemyConvergenceKnowledge,
                suppressedNonEnemyConvergenceActions,
                suppressedNonEnemyConvergenceHistory,
                "Legacy recent-lane suppression must not trap the combat loop in wait once the lane is already quiescent.",
                null));
            Assert(string.Equals(suppressedNonEnemyConvergenceDecision.TargetLabel, "auto-end turn", StringComparison.OrdinalIgnoreCase), "A cleared non-enemy suppression heuristic should fall through to the legal end-turn fallback.");

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

            var runtimeSimpleSelectObserver = runtimePendingNonEnemyObserver with
            {
                InventoryId = "inv-runtime-simple-select",
                SceneEpisodeId = "episode-runtime-simple-select",
                PlayerEnergy = 2,
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", null),
                    new ObservedCombatHandCard(2, "CARD.MANGLE", "Attack", null),
                },
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                    ["combatCardPlayPending"] = "false",
                    ["combatPlayMode"] = "SimpleSelect",
                    ["combatTargetingInProgress"] = "false",
                    ["combatLastCardPlayStartedCardId"] = "CARD.BRAND",
                    ["combatLastCardPlayStartedCardName"] = "낙인",
                    ["combatLastCardPlayFinishedCardId"] = "CARD.ANGER",
                },
            };
            var runtimeSimpleSelectKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.MANGLE", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var runtimeSimpleSelectActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimeSimpleSelectObserver, null, null, null),
                runtimeSimpleSelectKnowledge,
                combatNoOpScreenshotPath,
                pendingNonEnemySlot3History);
            Assert(runtimeSimpleSelectActions.Contains("select card from hand", StringComparer.OrdinalIgnoreCase), "Combat SimpleSelect runtime should expose a follow-up hand-card selection action.");
            Assert(!runtimeSimpleSelectActions.Contains("confirm selected non-enemy card", StringComparer.OrdinalIgnoreCase), "Combat SimpleSelect runtime should not masquerade as a generic non-enemy confirm.");
            Assert(!runtimeSimpleSelectActions.Contains("click end turn", StringComparer.OrdinalIgnoreCase), "Combat SimpleSelect runtime should keep end turn closed until the follow-up hand-card choice resolves.");
            var runtimeSimpleSelectDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                25,
                GuiSmokePhase.HandleCombat.ToString(),
                "Resolve combat SimpleSelect by choosing a card from hand.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable|combat-selection:unknown",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                runtimeSimpleSelectObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                runtimeSimpleSelectKnowledge,
                runtimeSimpleSelectActions,
                pendingNonEnemySlot3History,
                "Choose a hand card for the follow-up combat selection.",
                null));
            Assert(string.Equals(runtimeSimpleSelectDecision.TargetLabel, "combat select hand slot 1", StringComparison.OrdinalIgnoreCase), "Combat SimpleSelect runtime should choose a deterministic hand-card slot instead of confirming or ending the turn.");
            Assert(string.Equals(runtimeSimpleSelectDecision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase), "Combat SimpleSelect runtime should use a hand-slot hotkey for the follow-up choice.");
            Assert(CombatDecisionContract.IsAllowed(
                    new GuiSmokeStepRequest(
                        "run",
                        "boot-to-long-run",
                        25,
                        GuiSmokePhase.HandleCombat.ToString(),
                        "Resolve combat SimpleSelect by choosing a card from hand.",
                        DateTimeOffset.UtcNow,
                        combatNoOpScreenshotPath,
                        new WindowBounds(1, 32, 1280, 720),
                        "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable|combat-selection:unknown",
                        "0001",
                        1,
                        3,
                        false,
                        "tactical",
                        null,
                        runtimeSimpleSelectObserver,
                        Array.Empty<KnownRecipeHint>(),
                        Array.Empty<EventKnowledgeCandidate>(),
                        runtimeSimpleSelectKnowledge,
                        new[] { "select card from hand", "wait" },
                        pendingNonEnemySlot3History,
                        "Choose a hand card for the follow-up combat selection.",
                        null),
                    new GuiSmokeStepDecision("act", "press-key", "1", null, null, "combat select hand slot 1", "test", 0.5, "combat", 0, true, null),
                    out var handSelectSemanticAction)
                    && string.Equals(handSelectSemanticAction, "select card from hand", StringComparison.OrdinalIgnoreCase),
                "Combat hand-selection lane should map to explicit select-card semantics.");

            var runtimeSimpleSelectConfirmObserver = runtimeSimpleSelectObserver with
            {
                Meta = new Dictionary<string, string?>(runtimeSimpleSelectObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatHandSelectionSelectedCount"] = "1",
                    ["combatHandSelectionSelectedCardIds"] = "CARD.DEFEND_IRONCLAD",
                    ["combatHandSelectionConfirmEnabled"] = "true",
                }
            };
            var runtimeSimpleSelectConfirmActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimeSimpleSelectConfirmObserver, null, null, null),
                runtimeSimpleSelectKnowledge,
                combatNoOpScreenshotPath,
                pendingNonEnemySlot3History);
            Assert(runtimeSimpleSelectConfirmActions.Contains("confirm selected hand card", StringComparer.OrdinalIgnoreCase), "Combat SimpleSelect with runtime-selected hand state should expose an explicit confirm action.");
            Assert(!runtimeSimpleSelectConfirmActions.Contains("select card from hand", StringComparer.OrdinalIgnoreCase), "Combat SimpleSelect confirm stage should not reopen raw hand selection while runtime state still tracks a selected card.");
            var runtimeSimpleSelectConfirmDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                25,
                GuiSmokePhase.HandleCombat.ToString(),
                "Confirm the selected combat hand card.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable|combat-selection:unknown|combat-play-open",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                runtimeSimpleSelectConfirmObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                runtimeSimpleSelectKnowledge,
                runtimeSimpleSelectConfirmActions,
                pendingNonEnemySlot3History,
                "Confirm the selected combat hand card.",
                null));
            Assert(string.Equals(runtimeSimpleSelectConfirmDecision.TargetLabel, "confirm selected hand card", StringComparison.OrdinalIgnoreCase), "Combat SimpleSelect confirm stage should follow runtime hand-selection truth instead of image heuristics.");
            Assert(CombatDecisionContract.IsAllowed(
                    new GuiSmokeStepRequest(
                        "run",
                        "boot-to-long-run",
                        25,
                        GuiSmokePhase.HandleCombat.ToString(),
                        "Confirm the selected combat hand card.",
                        DateTimeOffset.UtcNow,
                        combatNoOpScreenshotPath,
                        new WindowBounds(1, 32, 1280, 720),
                        "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable|combat-selection:unknown|combat-play-open",
                        "0001",
                        1,
                        3,
                        false,
                        "tactical",
                        null,
                        runtimeSimpleSelectConfirmObserver,
                        Array.Empty<KnownRecipeHint>(),
                        Array.Empty<EventKnowledgeCandidate>(),
                        runtimeSimpleSelectKnowledge,
                        new[] { "confirm selected hand card", "wait" },
                        pendingNonEnemySlot3History,
                        "Confirm the selected combat hand card.",
                        null),
                    runtimeSimpleSelectConfirmDecision,
                    out var handConfirmSemanticAction)
                && string.Equals(handConfirmSemanticAction, "confirm selected hand card", StringComparison.OrdinalIgnoreCase),
                "Combat hand-selection confirm should map to explicit confirm semantics.");

            var handConfirmResolvingObserver = runtimeSimpleSelectObserver with
            {
                InventoryId = "inv-hand-confirm-resolving",
                SceneEpisodeId = "episode-hand-confirm-resolving",
                PlayerEnergy = 0,
                CurrentChoices = new[] { "DrawPile", "DiscardPile", "ExhaustPile", "5턴 종료", "핑" },
                ActionNodes = new[]
                {
                    new ObserverActionNode("end-turn", "choice", "5턴 종료", "1604,846,220,90", true),
                },
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(1, "CARD.INFECTION", "Status", 0),
                    new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                },
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi;CombatState.RoundNumber=5;CombatManager.PlayerActionsDisabled=false;CombatManager.EndingPlayerTurnPhaseOne=false;CombatManager.EndingPlayerTurnPhaseTwo=false",
                    ["combatCardPlayPending"] = "false",
                    ["combatTargetingInProgress"] = "false",
                    ["combatTargetableEnemyCount"] = "0",
                    ["combatHittableEnemyCount"] = "0",
                    ["combatHistoryStartedCount"] = "14",
                    ["combatHistoryFinishedCount"] = "13",
                    ["combatInteractionRevision"] = "14:13:false:false:none",
                    ["combatLastCardPlayStartedCardId"] = "CARD.BURNING_PACT",
                    ["combatLastCardPlayFinishedCardId"] = "CARD.STRIKE_IRONCLAD",
                    ["combatPlayerActionsDisabled"] = "false",
                    ["combatEndingPlayerTurnPhaseOne"] = "false",
                    ["combatEndingPlayerTurnPhaseTwo"] = "false",
                    ["combatRoundNumber"] = "5",
                },
            };
            var handConfirmResolvingKnowledge = new[]
            {
                new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            };
            var handConfirmResolvingHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select hand slot 1", DateTimeOffset.UtcNow.AddSeconds(-2)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "confirm selected hand card", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            var handConfirmResolvingState = new ObserverState(handConfirmResolvingObserver, null, null, null);
            var handConfirmResolvingContext = GuiSmokeStepRequestFactory.CreateStepAnalysisContext(
                GuiSmokePhase.HandleCombat,
                handConfirmResolvingState,
                combatNoOpScreenshotPath,
                handConfirmResolvingHistory,
                handConfirmResolvingKnowledge);
            Assert(handConfirmResolvingContext.CombatMicroStage.Kind == CombatMicroStageKind.ResolvingCardPlay,
                $"A confirmed combat hand-card play with started>finished counts should stay in ResolvingCardPlay until the queued action drains. actualStage={handConfirmResolvingContext.CombatMicroStage.Kind}");
            var handConfirmResolvingActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                handConfirmResolvingState,
                handConfirmResolvingKnowledge,
                combatNoOpScreenshotPath,
                handConfirmResolvingHistory);
            Assert(handConfirmResolvingActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase),
                $"A confirmed combat hand-card play should keep end turn closed while the player-driven action is still in flight. actual=[{string.Join(", ", handConfirmResolvingActions)}]");
            var handConfirmResolvingDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                26,
                GuiSmokePhase.HandleCombat.ToString(),
                "Wait for the confirmed combat hand-card play to finish before ending the turn.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:elite|ready:true|stability:stable|combat-play-open",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                handConfirmResolvingObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                handConfirmResolvingKnowledge,
                handConfirmResolvingActions,
                handConfirmResolvingHistory,
                "Do not end turn while the confirmed combat hand-card play is still resolving.",
                null));
            Assert(string.Equals(handConfirmResolvingDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(handConfirmResolvingDecision.TargetLabel, "auto-end turn", StringComparison.OrdinalIgnoreCase),
                $"A confirmed combat hand-card play should wait instead of falling through to auto-end turn. actual={handConfirmResolvingDecision.Status}/{handConfirmResolvingDecision.TargetLabel ?? handConfirmResolvingDecision.Reason ?? "null"}");

            var runtimeUpgradeSelectObserver = runtimeSimpleSelectObserver with
            {
                InventoryId = "inv-runtime-upgrade-select",
                SceneEpisodeId = "episode-runtime-upgrade-select",
                PlayerEnergy = 0,
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", null),
                    new ObservedCombatHandCard(2, "CARD.PERFECTED_STRIKE", "Attack", null),
                },
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi;CombatState.RoundNumber=2;CombatManager.PlayerActionsDisabled=false;CombatManager.EndingPlayerTurnPhaseOne=false;CombatManager.EndingPlayerTurnPhaseTwo=false",
                    ["combatCardPlayPending"] = "false",
                    ["combatPlayMode"] = "UpgradeSelect",
                    ["combatTargetingInProgress"] = "false",
                    ["combatTargetableEnemyCount"] = "0",
                    ["combatHittableEnemyCount"] = "0",
                    ["combatTargetSummary"] = "enemy-target:Bygone Effigy:2@logical:1426.976,280.592,114.048,221.76@normalized:0.7432,0.2598,0.0594,0.2053",
                    ["combatHistoryStartedCount"] = "6",
                    ["combatHistoryFinishedCount"] = "5",
                    ["combatInteractionRevision"] = "6:5:false:false:none",
                    ["combatLastCardPlayStartedCardId"] = "CARD.ARMAMENTS",
                    ["combatLastCardPlayFinishedCardId"] = "CARD.SWORD_BOOMERANG",
                },
            };
            var runtimeUpgradeSelectKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.PERFECTED_STRIKE", "Attack", "AnyEnemy", 2, "self-test"),
            };
            var runtimeUpgradeSelectHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-8)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "confirm-attack-card", "confirm selected attack card", DateTimeOffset.UtcNow.AddSeconds(-7)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 1", DateTimeOffset.UtcNow.AddSeconds(-6)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "confirm-non-enemy", "confirm selected non-enemy card", DateTimeOffset.UtcNow.AddSeconds(-5)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "auto-end turn", DateTimeOffset.UtcNow.AddSeconds(-1)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "wait", null, DateTimeOffset.UtcNow),
            };
            var runtimeUpgradeSelectActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimeUpgradeSelectObserver, null, null, null),
                runtimeUpgradeSelectKnowledge,
                combatNoOpScreenshotPath,
                runtimeUpgradeSelectHistory);
            Assert(runtimeUpgradeSelectActions.Contains("select card from hand", StringComparer.OrdinalIgnoreCase),
                "Combat UpgradeSelect runtime should expose a follow-up hand-card selection action.");
            Assert(!runtimeUpgradeSelectActions.Contains("click end turn", StringComparer.OrdinalIgnoreCase),
                "Combat UpgradeSelect runtime should keep end turn closed until the upgrade hand-card choice resolves.");
            var runtimeUpgradeSelectDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                46,
                GuiSmokePhase.HandleCombat.ToString(),
                "Resolve combat UpgradeSelect by choosing a card from hand instead of replaying end turn.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:elite|ready:true|stability:stable|combat-selection:upgrade-select",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                runtimeUpgradeSelectObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                runtimeUpgradeSelectKnowledge,
                runtimeUpgradeSelectActions,
                runtimeUpgradeSelectHistory,
                "Choose a hand card for the combat upgrade-select overlay.",
                null));
            Assert(string.Equals(runtimeUpgradeSelectDecision.TargetLabel, "combat select hand slot 1", StringComparison.OrdinalIgnoreCase),
                "Combat UpgradeSelect runtime should choose a hand-card slot instead of replaying auto-end turn.");

            var combatOverlaySimpleSelectObserver = runtimePendingNonEnemyObserver with
            {
                InventoryId = "inv-combat-overlay-simple-select",
                SceneEpisodeId = "episode-combat-overlay-simple-select",
                PlayerEnergy = 0,
                CurrentChoices = new[] { "DrawPile", "DiscardPile", "ExhaustPile", "1턴 종료" },
                Choices = new[]
                {
                    new ObserverChoice("choice", "DrawPile", null),
                    new ObserverChoice("choice", "DiscardPile", null),
                    new ObserverChoice("choice", "ExhaustPile", null),
                    new ObserverChoice("choice", "1턴 종료", null),
                },
                Meta = new Dictionary<string, string?>(runtimePendingNonEnemyObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                    ["combatCardPlayPending"] = "false",
                    ["combatPlayMode"] = "Play",
                    ["combatTargetingInProgress"] = "false",
                    ["combatSelectedCardSlot"] = null,
                    ["combatAwaitingPlaySlots"] = null,
                    ["combatHistoryStartedCount"] = "7",
                    ["combatHistoryFinishedCount"] = "6",
                    ["combatInteractionRevision"] = "7:6:false:false:none",
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NSimpleCardSelectScreen",
                    ["rawCurrentActiveScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NSimpleCardSelectScreen",
                    ["rawTopOverlayType"] = "MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NSimpleCardSelectScreen",
                    ["rootTypeSummary"] = "MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NSimpleCardSelectScreen MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom",
                    ["cardSelectionScreenDetected"] = "false",
                    ["cardSelectionScreenType"] = null,
                    ["cardSelectionPrompt"] = null,
                    ["cardSelectionMinSelect"] = null,
                    ["cardSelectionMaxSelect"] = null,
                    ["cardSelectionSelectedCount"] = "0",
                    ["cardSelectionRequireManualConfirmation"] = null,
                    ["cardSelectionCancelable"] = null,
                    ["cardSelectionMainConfirmEnabled"] = "false",
                    ["cardSelectionPreviewConfirmEnabled"] = "false",
                },
            };
            var combatOverlaySimpleSelectActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(combatOverlaySimpleSelectObserver, null, null, null),
                runtimeSimpleSelectKnowledge,
                combatNoOpScreenshotPath,
                pendingNonEnemySlot3History);
            Assert(!combatOverlaySimpleSelectActions.Contains("simple select choice", StringComparer.OrdinalIgnoreCase),
                "Combat should not reopen a broad simple-select overlay lane until explicit subtype card-selection choices are exported.");
            Assert(!combatOverlaySimpleSelectActions.Contains("click end turn", StringComparer.OrdinalIgnoreCase),
                "Combat simple-select overlay should keep end turn closed while the follow-up selection screen is foreground-owned.");
            Assert(combatOverlaySimpleSelectActions.Contains("wait", StringComparer.OrdinalIgnoreCase),
                "Combat simple-select overlay should wait when the owner screen is explicit but subtype choices are not yet exported.");

            var combatOverlaySimpleSelectExplicitObserver = combatOverlaySimpleSelectObserver with
            {
                InventoryId = "inv-combat-overlay-simple-select-explicit",
                SceneEpisodeId = "episode-combat-overlay-simple-select-explicit",
                ChoiceExtractorPath = "card-selection-simple-select",
                CurrentChoices = new[] { "DrawPile", "DiscardPile", "ExhaustPile", "1턴 종료", "Confirm", "타격", "수비" },
                Choices = new[]
                {
                    new ObserverChoice("choice", "DrawPile", null),
                    new ObserverChoice("choice", "DiscardPile", null),
                    new ObserverChoice("choice", "ExhaustPile", null),
                    new ObserverChoice("choice", "1턴 종료", null),
                    new ObserverChoice("simple-select-confirm", "Confirm", "956,618,188,50")
                    {
                        BindingKind = "card-selection-confirm",
                        BindingId = "main",
                        SemanticHints = new[] { "card-selection:simple-select", "confirm-mode:main" },
                        Enabled = false,
                    },
                    new ObserverChoice("simple-select-card", "타격", "360,164,144,192")
                    {
                        SemanticHints = new[] { "card-selection:simple-select" },
                        Enabled = true,
                    },
                    new ObserverChoice("simple-select-card", "수비", "560,164,144,192")
                    {
                        SemanticHints = new[] { "card-selection:simple-select" },
                        Enabled = true,
                    },
                },
                Meta = new Dictionary<string, string?>(combatOverlaySimpleSelectObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["cardSelectionScreenDetected"] = "true",
                    ["cardSelectionScreenType"] = "simple-select",
                    ["cardSelectionPrompt"] = "뽑을 카드 더미 맨 위에 놓을 카드를 선택하세요.",
                    ["cardSelectionMinSelect"] = "1",
                    ["cardSelectionMaxSelect"] = "1",
                    ["cardSelectionSelectedCount"] = "0",
                    ["cardSelectionRequireManualConfirmation"] = "false",
                    ["cardSelectionCancelable"] = "false",
                    ["cardSelectionMainConfirmEnabled"] = "false",
                    ["cardSelectionPreviewConfirmEnabled"] = "false",
                },
            };
            var combatOverlaySimpleSelectExplicitActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(combatOverlaySimpleSelectExplicitObserver, null, null, null),
                runtimeSimpleSelectKnowledge,
                combatNoOpScreenshotPath,
                pendingNonEnemySlot3History);
            Assert(combatOverlaySimpleSelectExplicitActions.Contains("simple select choice", StringComparer.OrdinalIgnoreCase),
                "Explicit combat simple-select overlay should expose simple-select choice actions.");
            Assert(!combatOverlaySimpleSelectExplicitActions.Contains("click end turn", StringComparer.OrdinalIgnoreCase),
                "Explicit combat simple-select overlay should not reopen end turn.");
            var combatOverlaySimpleSelectDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                25,
                GuiSmokePhase.HandleCombat.ToString(),
                "Resolve the combat simple-select overlay before ending the turn.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable|card-selection:simple-select|card-selection-selected:0",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                combatOverlaySimpleSelectExplicitObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                runtimeSimpleSelectKnowledge,
                combatOverlaySimpleSelectExplicitActions,
                pendingNonEnemySlot3History,
                "Resolve the combat simple-select overlay.",
                null));
            Assert(combatOverlaySimpleSelectDecision.TargetLabel?.StartsWith("simple select choice", StringComparison.OrdinalIgnoreCase) == true,
                "Combat should choose an explicit simple-select overlay card instead of ending the turn.");
            Assert(CombatDecisionContract.IsAllowed(
                    new GuiSmokeStepRequest(
                        "run",
                        "boot-to-long-run",
                        25,
                        GuiSmokePhase.HandleCombat.ToString(),
                        "Resolve the combat simple-select overlay before ending the turn.",
                        DateTimeOffset.UtcNow,
                        combatNoOpScreenshotPath,
                        new WindowBounds(1, 32, 1280, 720),
                        "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable|card-selection:simple-select|card-selection-selected:0",
                        "0001",
                        1,
                        3,
                        false,
                        "tactical",
                        null,
                        combatOverlaySimpleSelectExplicitObserver,
                        Array.Empty<KnownRecipeHint>(),
                        Array.Empty<EventKnowledgeCandidate>(),
                        runtimeSimpleSelectKnowledge,
                        new[] { "simple select choice", "wait" },
                        pendingNonEnemySlot3History,
                        "Resolve the combat simple-select overlay.",
                        null),
                    combatOverlaySimpleSelectDecision,
                    out var combatOverlaySimpleSelectSemanticAction)
                && string.Equals(combatOverlaySimpleSelectSemanticAction, "simple select choice", StringComparison.OrdinalIgnoreCase),
                "Combat simple-select overlay actions should map to explicit simple-select semantics.");

            var combatOverlaySimpleSelectConfirmObserver = combatOverlaySimpleSelectExplicitObserver with
            {
                InventoryId = "inv-combat-overlay-simple-select-confirm",
                SceneEpisodeId = "episode-combat-overlay-simple-select-confirm",
                Meta = new Dictionary<string, string?>(combatOverlaySimpleSelectExplicitObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["cardSelectionSelectedCount"] = "1",
                    ["cardSelectionMainConfirmEnabled"] = "true",
                },
                Choices = new[]
                {
                    new ObserverChoice("choice", "DrawPile", null),
                    new ObserverChoice("choice", "DiscardPile", null),
                    new ObserverChoice("choice", "ExhaustPile", null),
                    new ObserverChoice("choice", "1턴 종료", null),
                    new ObserverChoice("simple-select-confirm", "Confirm", "956,618,188,50")
                    {
                        BindingKind = "card-selection-confirm",
                        BindingId = "main",
                        SemanticHints = new[] { "card-selection:simple-select", "confirm-mode:main" },
                        Enabled = true,
                    },
                    new ObserverChoice("simple-select-card", "타격", "360,164,144,192")
                    {
                        SemanticHints = new[] { "card-selection:simple-select", "selected-card" },
                        Enabled = true,
                    },
                    new ObserverChoice("simple-select-card", "수비", "560,164,144,192")
                    {
                        SemanticHints = new[] { "card-selection:simple-select" },
                        Enabled = true,
                    },
                },
            };
            var combatOverlaySimpleSelectConfirmActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(combatOverlaySimpleSelectConfirmObserver, null, null, null),
                runtimeSimpleSelectKnowledge,
                combatNoOpScreenshotPath,
                pendingNonEnemySlot3History);
            Assert(combatOverlaySimpleSelectConfirmActions.Contains("simple select confirm", StringComparer.OrdinalIgnoreCase),
                "Combat simple-select confirm-ready overlay should expose explicit confirm instead of end turn.");
            var combatOverlaySimpleSelectConfirmDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                25,
                GuiSmokePhase.HandleCombat.ToString(),
                "Confirm the combat simple-select overlay.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable|card-selection:simple-select|card-selection-selected:1",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                combatOverlaySimpleSelectConfirmObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                runtimeSimpleSelectKnowledge,
                combatOverlaySimpleSelectConfirmActions,
                pendingNonEnemySlot3History,
                "Confirm the combat simple-select overlay.",
                null));
            Assert(string.Equals(combatOverlaySimpleSelectConfirmDecision.TargetLabel, "simple select confirm", StringComparison.OrdinalIgnoreCase),
                "Combat should confirm a selected simple-select overlay instead of reopening end turn.");

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
            Assert(!runtimeAttackSelectionActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase),
                $"Runtime attack selection without targeting evidence should keep click-enemy closed. actual=[{string.Join(", ", runtimeAttackSelectionActions)}]");
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

            var runtimeTargetSummaryObserver = runtimeAttackSelectionObserver with
            {
                InventoryId = "inv-runtime-target-summary",
                SceneEpisodeId = "episode-runtime-target-summary",
                Meta = new Dictionary<string, string?>(runtimeAttackSelectionObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatTargetableEnemyCount"] = "0",
                    ["combatTargetableEnemyIds"] = null,
                    ["combatHittableEnemyCount"] = "0",
                    ["combatHittableEnemyIds"] = null,
                    ["combatTargetCount"] = "1",
                    ["combatTargetCoordinateSpace"] = "logical-render",
                    ["combatTargetClickCoordinateSpace"] = "current-window-normalized",
                    ["combatTargetSummary"] = "enemy-target:Jaw Worm:1@logical:720,180,180,260@normalized:0.3750,0.1667,0.0938,0.2407",
                },
            };
            var runtimeTargetSummaryActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimeTargetSummaryObserver, null, null, null),
                runtimeTargetingKnowledge,
                runtimeStateOnlyScreenshotPath,
                runtimeAttackSelectionHistory);
            Assert(runtimeTargetSummaryActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Runtime target summary should open click-enemy even when aggregate hittable counts remain zero.");
            var runtimeTargetSummaryDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                26,
                GuiSmokePhase.HandleCombat.ToString(),
                "Prefer runtime target summary over stale zero-count target aggregates when an attack selection is still open.",
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
                runtimeTargetSummaryObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                runtimeTargetingKnowledge,
                runtimeTargetSummaryActions,
                runtimeAttackSelectionHistory,
                "Use runtime target-summary body bounds before falling back to screenshot-only targeting.",
                null));
            Assert(runtimeTargetSummaryDecision.TargetLabel?.StartsWith("combat enemy target Jaw Worm", StringComparison.OrdinalIgnoreCase) == true,
                "Runtime target summary should drive an explicit combat enemy-target click instead of leaving the attack lane unresolved.");

            var runtimeTargetSummaryHistoryCarryObserver = runtimeTargetSummaryObserver with
            {
                InventoryId = "inv-runtime-target-summary-history-carry",
                SceneEpisodeId = "episode-runtime-target-summary-history-carry",
                Meta = new Dictionary<string, string?>(runtimeTargetSummaryObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "false",
                    ["combatSelectedCardSlot"] = null,
                    ["combatSelectedCardType"] = "Attack",
                    ["combatTargetingInProgress"] = "false",
                    ["combatInteractionRevision"] = "5:5:false:false:none",
                    ["combatHistoryStartedCount"] = "5",
                    ["combatHistoryFinishedCount"] = "5",
                },
            };
            var runtimeTargetSummaryHistoryCarryHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 3", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            var reconstructedPendingAttack = CombatRuntimeStateSupport.ResolvePendingSelection(
                runtimeTargetSummaryHistoryCarryObserver,
                runtimeTargetingKnowledge,
                CombatHistorySupport.TryGetPendingCombatSelection(runtimeTargetSummaryHistoryCarryHistory));
            Assert(reconstructedPendingAttack is null,
                "Runtime target summary plus stale selected-card metadata should not resurrect the old attack lane after current-frame ownership clears.");
            var runtimeTargetSummaryHistoryCarryActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimeTargetSummaryHistoryCarryObserver, null, null, null),
                runtimeTargetingKnowledge,
                runtimeStateOnlyScreenshotPath,
                runtimeTargetSummaryHistoryCarryHistory);
            Assert(!runtimeTargetSummaryHistoryCarryActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase),
                "Runtime target summary plus recent attack history should keep click-enemy closed once current-frame attack ownership clears.");
            Assert(runtimeTargetSummaryHistoryCarryActions.Contains("wait", StringComparer.OrdinalIgnoreCase)
                   && !runtimeTargetSummaryHistoryCarryActions.Any(action => action.StartsWith("select attack slot ", StringComparison.OrdinalIgnoreCase)),
                "Runtime target summary plus recent attack history should wait for fresh combat selection truth instead of immediately reopening another attack lane.");
            var runtimeTargetSummaryHistoryCarryDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                26,
                GuiSmokePhase.HandleCombat.ToString(),
                "Wait for fresh combat selection truth when target summary survives after current-frame attack ownership already cleared.",
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
                runtimeTargetSummaryHistoryCarryObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                runtimeTargetingKnowledge,
                runtimeTargetSummaryHistoryCarryActions,
                runtimeTargetSummaryHistoryCarryHistory,
                "Do not rebuild a stale attack lane from runtime target summary after current-frame ownership cleared.",
                null));
            Assert(runtimeTargetSummaryHistoryCarryDecision.Status == "wait"
                   && runtimeTargetSummaryHistoryCarryDecision.TargetLabel is null,
                "Runtime target summary plus recent attack-lane history should wait instead of driving enemy targeting or reopening another slot.");

            var runtimeTargetSummaryNoEnergyObserver = runtimeTargetSummaryHistoryCarryObserver with
            {
                PlayerEnergy = 0,
            };
            var runtimeTargetSummaryNoEnergyActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimeTargetSummaryNoEnergyObserver, null, null, null),
                runtimeTargetingKnowledge,
                runtimeStateOnlyScreenshotPath,
                runtimeTargetSummaryHistoryCarryHistory);
            Assert(runtimeTargetSummaryNoEnergyActions.Contains("click end turn", StringComparer.OrdinalIgnoreCase)
                   && runtimeTargetSummaryNoEnergyActions.Contains("wait", StringComparer.OrdinalIgnoreCase)
                   && !runtimeTargetSummaryNoEnergyActions.Any(action => action.StartsWith("select attack slot ", StringComparison.OrdinalIgnoreCase)),
                "Stale target diagnostics with no remaining energy should keep attack reentry closed but preserve legal end-turn authority.");
            var runtimeTargetSummaryNoEnergyDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                27,
                GuiSmokePhase.HandleCombat.ToString(),
                "No-energy stale target diagnostics should end the turn instead of plateauing behind attack-lane residue.",
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
                runtimeTargetSummaryNoEnergyObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                runtimeTargetingKnowledge,
                runtimeTargetSummaryNoEnergyActions,
                runtimeTargetSummaryHistoryCarryHistory,
                "Do not let stale target diagnostics suppress legal end-turn closure after attack ownership clears.",
                null));
            Assert(string.Equals(runtimeTargetSummaryNoEnergyDecision.TargetLabel, "auto-end turn", StringComparison.OrdinalIgnoreCase),
                "No-energy stale target diagnostics should fall through to auto-end turn instead of a wait-only plateau.");

            var reopenedAttackSelectionMetadata = JsonSerializer.Serialize(
                new CombatBarrierHistoryMetadata(
                    55,
                    DateTimeOffset.UtcNow.AddSeconds(-1),
                    "combat",
                    "7:6:false:false:none",
                    7,
                    6,
                    "CARD.STRIKE_IRONCLAD")
                {
                    RoundNumber = 3,
                    PlayerActionsDisabled = false,
                    EndingPlayerTurnPhaseOne = false,
                    EndingPlayerTurnPhaseTwo = false,
                },
                GuiSmokeShared.JsonOptions);
            var reopenedAttackSelectionHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-1))
                {
                    Metadata = reopenedAttackSelectionMetadata,
                },
            };
            var reopenedAttackSelectionObserver = runtimeTargetSummaryHistoryCarryObserver with
            {
                InventoryId = "inv-runtime-attack-reopen-after-cleared-selection",
                SceneEpisodeId = "episode-runtime-attack-reopen-after-cleared-selection",
                PlayerEnergy = 1,
                CurrentChoices = new[] { "DrawPile", "DiscardPile", "ExhaustPile", "3턴 종료", "핑" },
                ActionNodes = new[]
                {
                    new ObserverActionNode("end-turn", "choice", "3턴 종료", "1604,846,220,90", true),
                },
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(1, "CARD.BASH", "Attack", 2),
                    new ObservedCombatHandCard(2, "CARD.SPOILS_MAP", "Quest", 0),
                    new ObservedCombatHandCard(3, "CARD.IRON_WAVE", "Attack", 1),
                },
                Meta = new Dictionary<string, string?>(runtimeTargetSummaryHistoryCarryObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatTargetSummary"] = null,
                    ["combatSelectedCardSlot"] = null,
                    ["combatSelectedCardType"] = "Attack",
                    ["combatSelectedCardTargetType"] = "AnyEnemy",
                    ["combatCardPlayPending"] = "false",
                    ["combatTargetingInProgress"] = "false",
                    ["combatInteractionRevision"] = "7:7:false:false:none",
                    ["combatHistoryStartedCount"] = "7",
                    ["combatHistoryFinishedCount"] = "7",
                    ["combatLastCardPlayStartedCardId"] = "CARD.IRON_WAVE",
                    ["combatLastCardPlayFinishedCardId"] = "CARD.IRON_WAVE",
                    ["combatTargetableEnemyCount"] = "0",
                    ["combatTargetableEnemyIds"] = null,
                    ["combatHittableEnemyCount"] = "0",
                    ["combatHittableEnemyIds"] = null,
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi;CombatState.RoundNumber=3;CombatManager.PlayerActionsDisabled=false;CombatManager.EndingPlayerTurnPhaseOne=false;CombatManager.EndingPlayerTurnPhaseTwo=false",
                },
            };
            var reopenedAttackSelectionKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.BASH", "Attack", "AnyEnemy", 2, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.SPOILS_MAP", "Quest", "None", 0, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.IRON_WAVE", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var reopenedAttackSelectionState = new ObserverState(reopenedAttackSelectionObserver, null, null, null);
            var reopenedAttackSelectionContext = GuiSmokeStepRequestFactory.CreateStepAnalysisContext(
                GuiSmokePhase.HandleCombat,
                reopenedAttackSelectionState,
                runtimeStateOnlyScreenshotPath,
                reopenedAttackSelectionHistory,
                reopenedAttackSelectionKnowledge);
            var reopenedAttackSelectionActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                reopenedAttackSelectionState,
                reopenedAttackSelectionKnowledge,
                runtimeStateOnlyScreenshotPath,
                reopenedAttackSelectionHistory);
            Assert(!reopenedAttackSelectionContext.CombatBarrierEvaluation.IsActive
                   || reopenedAttackSelectionContext.CombatBarrierEvaluation.Kind != CombatBarrierKind.AttackSelect,
                $"Post-selection player-play reopen should retire the old AttackSelect barrier. actualBarrier={reopenedAttackSelectionContext.CombatBarrierEvaluation.Kind} reason={reopenedAttackSelectionContext.CombatBarrierEvaluation.Reason}");
            Assert(reopenedAttackSelectionActions.Contains("select attack slot 3", StringComparer.OrdinalIgnoreCase),
                $"Post-selection progress with player-play reopen should reopen the next explicit attack lane instead of staying wait-only. actual=[{string.Join(", ", reopenedAttackSelectionActions)}] lifecycle={reopenedAttackSelectionContext.CombatReleaseState.LifecycleStage} micro={reopenedAttackSelectionContext.CombatMicroStage.Kind} pending={reopenedAttackSelectionContext.PendingCombatSelection?.Kind.ToString() ?? "null"} barrier={reopenedAttackSelectionContext.CombatBarrierEvaluation.Kind} runtimePending={reopenedAttackSelectionContext.RuntimeCombatState.PendingSelection?.Kind.ToString() ?? "null"} targetSummary={reopenedAttackSelectionObserver.Meta.GetValueOrDefault("combatTargetSummary") ?? "null"} selectedType={reopenedAttackSelectionContext.RuntimeCombatState.SelectedCardType ?? "null"} selectedTargetType={reopenedAttackSelectionContext.RuntimeCombatState.SelectedCardTargetType ?? "null"} retireTail={CombatRuntimeStateSupport.ShouldRetireResolvedAttackSelectionTail(reopenedAttackSelectionObserver, reopenedAttackSelectionKnowledge, reopenedAttackSelectionContext.CombatContext.PendingSelection, reopenedAttackSelectionHistory)} staleTail={CombatRuntimeStateSupport.HasResidualAttackSelectionTail(reopenedAttackSelectionObserver, reopenedAttackSelectionKnowledge, reopenedAttackSelectionContext.CombatContext.PendingSelection, reopenedAttackSelectionHistory)}");
            Assert(!reopenedAttackSelectionActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase),
                "Resolved attack selection metadata should not collapse a reopened player-play frame into wait-only.");
            var reopenedAttackSelectionDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                28,
                GuiSmokePhase.HandleCombat.ToString(),
                "Retire stale AnyEnemy attack-selection residue once post-selection progress confirms the player-play lane reopened.",
                DateTimeOffset.UtcNow,
                runtimeStateOnlyScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:boss|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                reopenedAttackSelectionObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                reopenedAttackSelectionKnowledge,
                reopenedAttackSelectionActions,
                reopenedAttackSelectionHistory,
                "Do not let stale selected-attack metadata block a reopened AnyEnemy attack slot after post-selection progress.",
                null));
            Assert(!string.Equals(reopenedAttackSelectionDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(reopenedAttackSelectionDecision.TargetLabel, "combat select attack slot 3", StringComparison.OrdinalIgnoreCase),
                $"Reopened player-play combat after cleared attack selection should reissue the remaining explicit playable attack slot. actual={reopenedAttackSelectionDecision.Status}/{reopenedAttackSelectionDecision.TargetLabel}/{reopenedAttackSelectionDecision.Reason}");

            var reopenedAttackSelectionWithStaleTargetSummaryObserver = reopenedAttackSelectionObserver with
            {
                InventoryId = "inv-runtime-attack-reopen-after-cleared-selection-stale-summary",
                SceneEpisodeId = "episode-runtime-attack-reopen-after-cleared-selection-stale-summary",
                Meta = new Dictionary<string, string?>(reopenedAttackSelectionObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatTargetSummary"] = "enemy-target:Jaw Worm:1@logical:720,180,180,260@normalized:0.3750,0.1667,0.0938,0.2407",
                },
            };
            var reopenedAttackSelectionWithStaleTargetSummaryActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(reopenedAttackSelectionWithStaleTargetSummaryObserver, null, null, null),
                reopenedAttackSelectionKnowledge,
                runtimeStateOnlyScreenshotPath,
                reopenedAttackSelectionHistory);
            Assert(reopenedAttackSelectionWithStaleTargetSummaryActions.Contains("select attack slot 3", StringComparer.OrdinalIgnoreCase),
                $"Post-selection reopen should retire stale target-summary residue when no live target surface remains. actual=[{string.Join(", ", reopenedAttackSelectionWithStaleTargetSummaryActions)}]");
            Assert(!reopenedAttackSelectionWithStaleTargetSummaryActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase),
                "Stale target-summary residue without live target authority should not keep click-enemy open after the player-play lane reopened.");
            var reopenedAttackSelectionWithStaleTargetSummaryDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                29,
                GuiSmokePhase.HandleCombat.ToString(),
                "Retire stale target-summary residue once post-selection progress confirms the player-play lane reopened.",
                DateTimeOffset.UtcNow,
                runtimeStateOnlyScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:boss|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                reopenedAttackSelectionWithStaleTargetSummaryObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                reopenedAttackSelectionKnowledge,
                reopenedAttackSelectionWithStaleTargetSummaryActions,
                reopenedAttackSelectionHistory,
                "Do not let stale target-summary metadata override a reopened player-play lane after explicit clear.",
                null));
            Assert(!string.Equals(reopenedAttackSelectionWithStaleTargetSummaryDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(reopenedAttackSelectionWithStaleTargetSummaryDecision.TargetLabel, "combat select attack slot 3", StringComparison.OrdinalIgnoreCase),
                $"Reopened player-play combat should still select the remaining attack slot when only stale target-summary residue remains. actual={reopenedAttackSelectionWithStaleTargetSummaryDecision.Status}/{reopenedAttackSelectionWithStaleTargetSummaryDecision.TargetLabel}/{reopenedAttackSelectionWithStaleTargetSummaryDecision.Reason}");

            var runtimeTargetSummaryAfterDriftHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 3", DateTimeOffset.UtcNow.AddSeconds(-2)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "observer-drift", "combat enemy target Jaw Worm recenter", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            Assert(CombatRuntimeStateSupport.ResolvePendingSelection(
                    runtimeTargetSummaryHistoryCarryObserver,
                    runtimeTargetingKnowledge,
                    CombatHistorySupport.TryGetPendingCombatSelection(runtimeTargetSummaryAfterDriftHistory),
                    runtimeTargetSummaryAfterDriftHistory) is null,
                "Observer-drift history alone should not keep the recent attack lane alive without fresh current-frame ownership.");
            var runtimeTargetSummaryAfterDriftActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimeTargetSummaryHistoryCarryObserver, null, null, null),
                runtimeTargetingKnowledge,
                runtimeStateOnlyScreenshotPath,
                runtimeTargetSummaryAfterDriftHistory);
            Assert(!runtimeTargetSummaryAfterDriftActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase)
                   && runtimeTargetSummaryAfterDriftActions.Any(action => action.StartsWith("select attack slot ", StringComparison.OrdinalIgnoreCase)),
                "Observer-drift history alone should reopen a fresh explicit attack slot instead of re-clicking the stale target.");

            var targetSummaryStaleFinishedMetadata = JsonSerializer.Serialize(
                new CombatBarrierHistoryMetadata(
                    61,
                    DateTimeOffset.UtcNow.AddSeconds(-1),
                    "map",
                    "6:6:false:false:none",
                    6,
                    6,
                    "CARD.DEFEND_IRONCLAD")
                {
                    RoundNumber = 3,
                    PlayerActionsDisabled = false,
                    EndingPlayerTurnPhaseOne = false,
                    EndingPlayerTurnPhaseTwo = false,
                },
                GuiSmokeShared.JsonOptions);
            var targetSummaryOnlyCarryObserver = runtimeTargetSummaryObserver with
            {
                InventoryId = "inv-runtime-target-summary-only-carry",
                SceneEpisodeId = null,
                PlayerEnergy = 3,
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(1, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    new ObservedCombatHandCard(3, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    new ObservedCombatHandCard(4, "CARD.GRAPPLE", "Attack", 1),
                    new ObservedCombatHandCard(5, "CARD.SWORD_BOOMERANG", "Attack", 1),
                },
                Meta = new Dictionary<string, string?>(runtimeTargetSummaryObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "false",
                    ["combatSelectedCardSlot"] = null,
                    ["combatSelectedCardType"] = null,
                    ["combatSelectedCardTargetType"] = null,
                    ["combatTargetingInProgress"] = "false",
                    ["combatTargetableEnemyCount"] = "0",
                    ["combatTargetableEnemyIds"] = null,
                    ["combatHittableEnemyCount"] = "0",
                    ["combatHittableEnemyIds"] = null,
                    ["combatInteractionRevision"] = "6:6:false:false:none",
                    ["combatHistoryStartedCount"] = "6",
                    ["combatHistoryFinishedCount"] = "6",
                    ["combatLastCardPlayFinishedCardId"] = "CARD.DEFEND_IRONCLAD",
                    ["combatRoundNumber"] = "3",
                    ["combatTargetSummary"] = "enemy-target:Jaw Worm:1@logical:720,180,180,260@normalized:0.3750,0.1667,0.0938,0.2407",
                },
            };
            var targetSummaryOnlyCarryKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(4, "CARD.GRAPPLE", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(5, "CARD.SWORD_BOOMERANG", "Attack", "RandomEnemy", 1, "self-test"),
            };
            var targetSummaryOnlyCarryHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-1))
                {
                    Metadata = targetSummaryStaleFinishedMetadata,
                },
            };
            Assert(CombatRuntimeStateSupport.ResolvePendingSelection(
                    targetSummaryOnlyCarryObserver,
                    targetSummaryOnlyCarryKnowledge,
                    CombatHistorySupport.TryGetPendingCombatSelection(targetSummaryOnlyCarryHistory),
                    targetSummaryOnlyCarryHistory) is null,
                "Target summary alone should not resurrect a cleared attack lane after selected-card fields and in-card-play ownership are gone.");
            var targetSummaryOnlyCarryActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(targetSummaryOnlyCarryObserver, null, null, null),
                targetSummaryOnlyCarryKnowledge,
                runtimeStateOnlyScreenshotPath,
                targetSummaryOnlyCarryHistory);
            Assert(!targetSummaryOnlyCarryActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase),
                "Target summary alone should keep click-enemy closed after the attack lane is explicitly cleared.");
            var targetSummaryOnlyCarryDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                30,
                GuiSmokePhase.HandleCombat.ToString(),
                "Do not resurrect a cleared attack lane from target-summary diagnostics alone.",
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
                targetSummaryOnlyCarryObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                targetSummaryOnlyCarryKnowledge,
                targetSummaryOnlyCarryActions,
                targetSummaryOnlyCarryHistory,
                "Target summary diagnostics alone should not reopen enemy targeting or confirm lanes.",
                null));
            Assert(targetSummaryOnlyCarryDecision.TargetLabel?.StartsWith("combat enemy target Jaw Worm", StringComparison.OrdinalIgnoreCase) != true,
                "Target summary diagnostics alone should not emit a stale enemy-target click.");

            var staleTargetSummaryAfterTargetAttemptObserver = targetSummaryOnlyCarryObserver with
            {
                InventoryId = "inv-runtime-target-summary-after-target-attempt",
                SceneEpisodeId = "episode-runtime-target-summary-after-target-attempt",
                ChoiceExtractorPath = "combat",
                CurrentChoices = new[] { "DrawPile", "DiscardPile", "ExhaustPile", "1턴 종료" },
                ActionNodes = Array.Empty<ObserverActionNode>(),
                Choices = new[]
                {
                    new ObserverChoice("choice", "DrawPile", null),
                    new ObserverChoice("card", "DiscardPile", null),
                    new ObserverChoice("choice", "ExhaustPile", null),
                    new ObserverChoice("choice", "1턴 종료", null),
                },
                Meta = new Dictionary<string, string?>(targetSummaryOnlyCarryObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "false",
                    ["combatSelectedCardSlot"] = null,
                    ["combatSelectedCardType"] = "Attack",
                    ["combatSelectedCardTargetType"] = "AnyEnemy",
                    ["combatTargetingInProgress"] = "false",
                    ["combatTargetableEnemyCount"] = "0",
                    ["combatTargetableEnemyIds"] = null,
                    ["combatHittableEnemyCount"] = "0",
                    ["combatHittableEnemyIds"] = null,
                    ["combatHoveredTargetKind"] = null,
                    ["combatHoveredTargetId"] = null,
                    ["combatHoveredTargetLabel"] = null,
                    ["combatHoveredTargetIsHittable"] = null,
                },
            };
            var staleTargetSummaryAfterTargetAttemptHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-2))
                {
                    Metadata = targetSummaryStaleFinishedMetadata,
                },
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "observer-drift", "combat enemy target Jaw Worm recenter", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            Assert(CombatRuntimeStateSupport.ResolvePendingSelection(
                    staleTargetSummaryAfterTargetAttemptObserver,
                    targetSummaryOnlyCarryKnowledge,
                    CombatHistorySupport.TryGetPendingCombatSelection(staleTargetSummaryAfterTargetAttemptHistory),
                    staleTargetSummaryAfterTargetAttemptHistory) is null,
                "Once an enemy-target attempt has already happened, stale attack metadata plus target summary should not keep the old attack lane alive without a live target surface.");
            var staleTargetSummaryAfterTargetAttemptActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(staleTargetSummaryAfterTargetAttemptObserver, null, null, null),
                targetSummaryOnlyCarryKnowledge,
                runtimeStateOnlyScreenshotPath,
                staleTargetSummaryAfterTargetAttemptHistory);
            Assert(!staleTargetSummaryAfterTargetAttemptActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase),
                "Stale post-target-attempt combat state should not reopen click-enemy from target-summary carryover alone.");
            Assert(staleTargetSummaryAfterTargetAttemptActions.Any(action => action.StartsWith("select attack slot ", StringComparison.OrdinalIgnoreCase)),
                "Stale post-target-attempt combat state should reopen a fresh attack-slot choice instead of waiting on the old lane.");
            var staleTargetSummaryAfterTargetAttemptDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                31,
                GuiSmokePhase.HandleCombat.ToString(),
                "After an enemy-target attempt already happened, stale target-summary carryover should reopen player-action selection instead of waiting on the old lane.",
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
                staleTargetSummaryAfterTargetAttemptObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                targetSummaryOnlyCarryKnowledge,
                staleTargetSummaryAfterTargetAttemptActions,
                staleTargetSummaryAfterTargetAttemptHistory,
                "Drop stale attack-lane carryover after a downstream enemy-target attempt when no live target surface remains.",
                null));
            Assert(staleTargetSummaryAfterTargetAttemptDecision.TargetLabel?.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase) == true,
                "Stale post-target-attempt carryover should reopen a fresh attack-slot decision instead of re-clicking an enemy or waiting.");

            var staleCombatLaneCarryMetadata = JsonSerializer.Serialize(
                new CombatBarrierHistoryMetadata(
                    82,
                    DateTimeOffset.UtcNow.AddSeconds(-2),
                    "episode-runtime-target-summary-history-carry",
                    "6:6:false:false:none",
                    6,
                    6,
                    "CARD.STRIKE_IRONCLAD")
                {
                    RoundNumber = 3,
                    PlayerActionsDisabled = false,
                    EndingPlayerTurnPhaseOne = false,
                    EndingPlayerTurnPhaseTwo = false,
                },
                GuiSmokeShared.JsonOptions);
            var freshCombatTargetSummaryObserver = runtimeTargetSummaryObserver with
            {
                InventoryId = "inv-runtime-target-summary-fresh-combat",
                SceneEpisodeId = "episode-runtime-target-summary-fresh-combat",
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(3, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    new ObservedCombatHandCard(4, "CARD.BASH", "Attack", 2),
                },
                Meta = new Dictionary<string, string?>(runtimeTargetSummaryObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "false",
                    ["combatSelectedCardSlot"] = null,
                    ["combatSelectedCardType"] = null,
                    ["combatSelectedCardTargetType"] = null,
                    ["combatTargetingInProgress"] = "false",
                    ["combatTargetableEnemyCount"] = "0",
                    ["combatTargetableEnemyIds"] = null,
                    ["combatHittableEnemyCount"] = "0",
                    ["combatHittableEnemyIds"] = null,
                    ["combatHistoryStartedCount"] = null,
                    ["combatHistoryFinishedCount"] = null,
                    ["combatLastCardPlayFinishedCardId"] = null,
                    ["combatRoundNumber"] = "1",
                    ["combatInteractionRevision"] = CombatRuntimeStateSupport.DefaultInteractionRevision,
                    ["combatTargetSummary"] = "enemy-target:Slaver:2@logical:840,210,180,260@normalized:0.4375,0.1944,0.0938,0.2407",
                },
            };
            var freshCombatHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-2))
                {
                    Metadata = staleCombatLaneCarryMetadata,
                },
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 2", DateTimeOffset.UtcNow.AddSeconds(-1)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-resolved-map", null, DateTimeOffset.UtcNow),
            };
            var freshCombatTargetSummaryContext = GuiSmokeStepRequestFactory.CreateStepAnalysisContext(
                GuiSmokePhase.HandleCombat,
                new ObserverState(freshCombatTargetSummaryObserver, null, null, null),
                runtimeStateOnlyScreenshotPath,
                freshCombatHistory,
                new[]
                {
                    new CombatCardKnowledgeHint(3, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                    new CombatCardKnowledgeHint(4, "CARD.BASH", "Attack", "AnyEnemy", 2, "self-test"),
                });
            Assert(freshCombatTargetSummaryContext.CombatContext.PendingSelection is null,
                "Fresh combat start should discard the previous combat's selected attack lane instead of carrying slot 2 forward.");
            Assert(freshCombatTargetSummaryContext.CombatContext.CombatNoOpCountsBySlot.Count == 0,
                "Fresh combat start should also clear stale combat-noop counts from the previous encounter.");
            Assert(freshCombatTargetSummaryContext.PendingCombatSelection is null
                   && freshCombatTargetSummaryContext.CombatMicroStage.Kind == CombatMicroStageKind.PlayerActionOpen,
                "Fresh combat target summary should rebuild as a new open player-action stage, not as an unresolved carried attack lane.");
            Assert(!GuiSmokeSceneReasoningSupport.ComputeSceneSignatureCore(
                    runtimeStateOnlyScreenshotPath,
                    new ObserverState(freshCombatTargetSummaryObserver, null, null, null),
                    GuiSmokePhase.HandleCombat,
                    freshCombatTargetSummaryContext)
                    .Contains("combat-slot:2", StringComparison.OrdinalIgnoreCase),
                "Fresh combat scene signature should not leak the previous encounter's combat slot tag.");
            var freshCombatTargetSummaryActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(freshCombatTargetSummaryObserver, null, null, null),
                new[]
                {
                    new CombatCardKnowledgeHint(3, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                    new CombatCardKnowledgeHint(4, "CARD.BASH", "Attack", "AnyEnemy", 2, "self-test"),
                },
                runtimeStateOnlyScreenshotPath,
                freshCombatHistory);
            var freshCombatTargetSummaryDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                27,
                GuiSmokePhase.HandleCombat.ToString(),
                "Fresh combat must start from the current hand instead of clicking an enemy from the previous encounter's stale lane.",
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
                freshCombatTargetSummaryObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                new[]
                {
                    new CombatCardKnowledgeHint(3, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                    new CombatCardKnowledgeHint(4, "CARD.BASH", "Attack", "AnyEnemy", 2, "self-test"),
                },
                freshCombatTargetSummaryActions,
                freshCombatHistory,
                "Drop stale combat lane carry when a brand-new combat opens after reward/map handoff.",
                null));
            Assert(freshCombatTargetSummaryDecision.TargetLabel?.StartsWith("combat select attack slot 3", StringComparison.OrdinalIgnoreCase) == true,
                "Fresh combat should select a current playable attack slot before any enemy-target click is considered.");

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
            Assert(!runtimeTargetingActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Runtime targeting state without explicit target-node source should keep click-enemy closed.");
            var runtimeTargetingDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                27,
                GuiSmokePhase.HandleCombat.ToString(),
                "Runtime targeting state without explicit target-node source should wait instead of fabricating an enemy click.",
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
                "Use runtime targeting metadata only when explicit target-node source is present.",
                null));
            Assert(!string.Equals(runtimeTargetingDecision.TargetLabel, "click enemy", StringComparison.OrdinalIgnoreCase)
                   && runtimeTargetingDecision.TargetLabel?.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase) != true
                   && runtimeTargetingDecision.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) != true,
                "Runtime targeting metadata without explicit target-node source should wait instead of emitting an enemy-target click.");

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

            var staleNonAttackMetadataObserver = runtimeFinishedSelectionObserver with
            {
                InventoryId = "inv-runtime-stale-non-attack-metadata",
                SceneEpisodeId = "episode-runtime-stale-non-attack-metadata",
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                    ["combatCardPlayPending"] = "false",
                    ["combatTargetingInProgress"] = "false",
                    ["combatSelectedCardSlot"] = null,
                    ["combatSelectedCardType"] = "Skill",
                    ["combatSelectedCardTargetType"] = "Self",
                    ["combatLastCardPlayFinishedCardId"] = "CARD.DEFEND_IRONCLAD",
                },
            };
            var staleNonAttackMetadataContext = GuiSmokeStepRequestFactory.CreateStepAnalysisContext(
                GuiSmokePhase.HandleCombat,
                new ObserverState(staleNonAttackMetadataObserver, null, null, null),
                runtimeStateOnlyScreenshotPath,
                stalePendingAttackHistory,
                runtimeFinishedSelectionKnowledge);
            Assert(staleNonAttackMetadataContext.RuntimeCombatState.ExplicitlyClearedSelection,
                "Non-attack selection metadata should still count as a cleared attack lane.");
            Assert(staleNonAttackMetadataContext.CombatMicroStage.Kind != CombatMicroStageKind.AwaitingCardPlayConfirm,
                "Stale non-attack selection metadata should not reopen the selected-attack confirm lane.");
            var staleNonAttackMetadataActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(staleNonAttackMetadataObserver, null, null, null),
                runtimeFinishedSelectionKnowledge,
                runtimeStateOnlyScreenshotPath,
                stalePendingAttackHistory);
            Assert(!staleNonAttackMetadataActions.Contains("confirm selected attack card", StringComparer.OrdinalIgnoreCase),
                "Stale non-attack selection metadata should keep confirm selected attack card closed.");

            var carryoverRandomEnemyHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-2)),
            };
            var carryoverRandomEnemyObserver = runtimeFinishedSelectionObserver with
            {
                InventoryId = "inv-runtime-random-enemy-carryover",
                SceneEpisodeId = "episode-runtime-random-enemy-carryover",
                PlayerEnergy = 3,
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(1, "CARD.SWORD_BOOMERANG", "Attack", 1),
                    new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    new ObservedCombatHandCard(3, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                },
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                    ["combatCardPlayPending"] = "false",
                    ["combatTargetingInProgress"] = "false",
                    ["combatSelectedCardSlot"] = null,
                    ["combatSelectedCardType"] = "Attack",
                    ["combatSelectedCardTargetType"] = "RandomEnemy",
                    ["combatLastCardPlayFinishedCardId"] = null,
                },
            };
            var carryoverRandomEnemyKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.SWORD_BOOMERANG", "Attack", "RandomEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            };
            Assert(CombatRuntimeStateSupport.Read(carryoverRandomEnemyObserver, carryoverRandomEnemyKnowledge).ExplicitlyClearedSelection,
                "Random-enemy attack metadata without current-frame ownership should count as a cleared selection.");
            Assert(CombatRuntimeStateSupport.ResolvePendingSelection(
                    carryoverRandomEnemyObserver,
                    carryoverRandomEnemyKnowledge,
                    CombatHistorySupport.TryGetPendingCombatSelection(carryoverRandomEnemyHistory)) is null,
                "Selected random-enemy metadata alone should not preserve the old lane once current-frame ownership is gone.");
            var carryoverRandomEnemyActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(carryoverRandomEnemyObserver, null, null, null),
                carryoverRandomEnemyKnowledge,
                runtimeStateOnlyScreenshotPath,
                carryoverRandomEnemyHistory);
            Assert(!carryoverRandomEnemyActions.Contains("confirm selected attack card", StringComparer.OrdinalIgnoreCase),
                "Random-enemy attack metadata without live in-card-play ownership should keep confirm selected attack card closed.");
            Assert(carryoverRandomEnemyActions.Contains("wait", StringComparer.OrdinalIgnoreCase)
                   && !carryoverRandomEnemyActions.Any(action => action.StartsWith("select attack slot ", StringComparison.OrdinalIgnoreCase)),
                "Random-enemy attack metadata without live ownership should wait for fresh combat selection truth instead of reopening another attack lane.");
            var carryoverRandomEnemyDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                29,
                GuiSmokePhase.HandleCombat.ToString(),
                "Random-enemy attack metadata without live in-card-play ownership should not reopen a synthetic confirm lane.",
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
                carryoverRandomEnemyObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                carryoverRandomEnemyKnowledge,
                carryoverRandomEnemyActions,
                carryoverRandomEnemyHistory,
                "Do not synthesize confirm-selected-attack from stale random-enemy metadata alone.",
                null));
            Assert(carryoverRandomEnemyDecision.Status == "wait"
                   && carryoverRandomEnemyDecision.TargetLabel is null,
                "Random-enemy attack metadata without live in-card-play ownership should wait instead of reopening another attack lane.");

    }
}
