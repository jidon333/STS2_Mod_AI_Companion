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
            Assert(reconstructedPendingAttack is { Kind: AutoCombatCardKind.AttackLike, SlotIndex: 3 },
                "Runtime target summary should preserve the recent attack-lane selection instead of clearing it before enemy targeting resolves.");
            var runtimeTargetSummaryHistoryCarryActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(runtimeTargetSummaryHistoryCarryObserver, null, null, null),
                runtimeTargetingKnowledge,
                runtimeStateOnlyScreenshotPath,
                runtimeTargetSummaryHistoryCarryHistory);
            Assert(runtimeTargetSummaryHistoryCarryActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase),
                "Runtime target summary plus recent attack history should keep click-enemy open even when selected-slot metadata briefly clears.");
            var runtimeTargetSummaryHistoryCarryDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                26,
                GuiSmokePhase.HandleCombat.ToString(),
                "Rebuild the unresolved attack lane from runtime target summary before retrying the same slot.",
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
                "Use runtime target summary and recent attack-lane history before repeating the same attack-slot hotkey.",
                null));
            Assert(runtimeTargetSummaryHistoryCarryDecision.TargetLabel?.StartsWith("combat enemy target Jaw Worm", StringComparison.OrdinalIgnoreCase) == true,
                "Runtime target summary plus recent attack-lane history should drive enemy targeting instead of reopening the same attack slot.");

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
                    targetSummaryOnlyCarryHistory) is { Kind: AutoCombatCardKind.AttackLike, SlotIndex: 1 },
                "Unchanged post-select runtime metadata plus target summary should preserve the recent attack lane even when selected-card fields briefly clear.");
            var targetSummaryOnlyCarryActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(targetSummaryOnlyCarryObserver, null, null, null),
                targetSummaryOnlyCarryKnowledge,
                runtimeStateOnlyScreenshotPath,
                targetSummaryOnlyCarryHistory);
            Assert(targetSummaryOnlyCarryActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase),
                "Target summary carryover should reopen click-enemy instead of forcing another attack-slot hotkey.");
            var targetSummaryOnlyCarryDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                30,
                GuiSmokePhase.HandleCombat.ToString(),
                "Preserve a just-selected attack lane from unchanged runtime metadata before repeating the same hotkey.",
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
                "Use unchanged runtime metadata plus target summary to continue the current attack lane.",
                null));
            Assert(targetSummaryOnlyCarryDecision.TargetLabel?.StartsWith("combat enemy target Jaw Worm", StringComparison.OrdinalIgnoreCase) == true,
                "Target summary carryover should target the enemy instead of repeating the same attack slot.");

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
            Assert(CombatRuntimeStateSupport.ResolvePendingSelection(
                    carryoverRandomEnemyObserver,
                    carryoverRandomEnemyKnowledge,
                    CombatHistorySupport.TryGetPendingCombatSelection(carryoverRandomEnemyHistory)) is { Kind: AutoCombatCardKind.AttackLike, SlotIndex: 1 },
                "Selected attack metadata without a slot id should still preserve the last attack lane when runtime reports a random-enemy attack selection.");
            var carryoverRandomEnemyActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(carryoverRandomEnemyObserver, null, null, null),
                carryoverRandomEnemyKnowledge,
                runtimeStateOnlyScreenshotPath,
                carryoverRandomEnemyHistory);
            Assert(carryoverRandomEnemyActions.Contains("confirm selected attack card", StringComparer.OrdinalIgnoreCase),
                "Random-enemy attack carryover should reopen confirm selected attack card instead of repeating the same slot.");
            var carryoverRandomEnemyDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                29,
                GuiSmokePhase.HandleCombat.ToString(),
                "Random-enemy attack carryover should rebuild the selected lane and confirm it instead of retrying the same slot key.",
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
                "Preserve selected attack metadata when runtime drops slot id but still reports a random-enemy attack selection.",
                null));
            Assert(string.Equals(carryoverRandomEnemyDecision.TargetLabel, "confirm selected attack card", StringComparison.OrdinalIgnoreCase),
                "Random-enemy attack carryover should confirm the rebuilt selected attack lane.");

    }
}
