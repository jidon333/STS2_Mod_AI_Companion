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
            Assert(
                string.Equals(repeatedAttackOrderingDecision.Status, "wait", StringComparison.OrdinalIgnoreCase),
                $"Repeated attack-select history without explicit no-op or clear evidence should wait instead of reopening another attack lane or ending the turn. actualStatus={repeatedAttackOrderingDecision.Status ?? "null"} actualAction={repeatedAttackOrderingDecision.ActionKind ?? "null"} target={repeatedAttackOrderingDecision.TargetLabel ?? "null"} reason={repeatedAttackOrderingDecision.Reason ?? "null"}");

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
                staleNonEnemyObserver,
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
                unresolvedTargetObserver,
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

            var historyShadowTargetObserver = new ObserverState(
                unresolvedTargetObserver.Summary with
                {
                    InventoryId = "inv-combat-target-history-shadow",
                    SnapshotVersion = 57,
                    PlayerEnergy = 2,
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi;CombatState.RoundNumber=2;CombatManager.PlayerActionsDisabled=false;CombatManager.EndingPlayerTurnPhaseOne=false;CombatManager.EndingPlayerTurnPhaseTwo=false",
                        ["combatCardPlayPending"] = "false",
                        ["combatTargetingInProgress"] = "false",
                        ["combatSelectedCardSlot"] = null,
                        ["combatSelectedCardType"] = null,
                        ["combatSelectedCardTargetType"] = null,
                        ["combatAwaitingPlaySlots"] = null,
                        ["combatTargetableEnemyCount"] = "0",
                        ["combatHittableEnemyCount"] = "0",
                        ["combatInteractionRevision"] = "8:7:false:false:none",
                        ["combatHistoryStartedCount"] = "8",
                        ["combatHistoryFinishedCount"] = "7",
                        ["combatLastCardPlayStartedCardId"] = "CARD.STRIKE_IRONCLAD",
                        ["combatLastCardPlayFinishedCardId"] = "CARD.DEFEND_IRONCLAD",
                    },
                },
                null,
                null,
                null);
            var historyShadowContext = GuiSmokeStepRequestFactory.CreateStepAnalysisContext(
                GuiSmokePhase.HandleCombat,
                historyShadowTargetObserver,
                combatNoOpScreenshotPath,
                enemyClickHistory,
                nonEnemySettleKnowledge);
            Assert(historyShadowContext.CombatMicroStage.Kind == CombatMicroStageKind.PlayerActionOpen,
                "Combat history shadow without live card-play ownership should reopen to PlayerActionOpen instead of staying in a resolving-card-play wait stage.");
            var historyShadowAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                historyShadowTargetObserver,
                nonEnemySettleKnowledge,
                combatNoOpScreenshotPath,
                enemyClickHistory);
            Assert(historyShadowAllowedActions.Contains("select attack slot 2", StringComparer.OrdinalIgnoreCase),
                "Combat history shadow without live ownership should reopen playable attack actions.");
            Assert(!historyShadowAllowedActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase),
                "Combat history shadow without live ownership should not collapse back to a wait-only allowlist.");
            var historyShadowQuietEvaluator = CombatPostActionObservationSupport.CreateWakeEvaluator(
                unresolvedTargetObserver,
                new GuiSmokeStepDecision("act", "click", null, 0.5, 0.5, "combat enemy target 1", "history shadow should quiet-converge once live ownership clears", 0.5, "combat", 250, true, null),
                enemyClickHistory,
                nonEnemySettleKnowledge,
                new WindowBounds(0, 0, 1280, 720));
            Assert(historyShadowQuietEvaluator(historyShadowTargetObserver) is null, "Combat history-shadow-only post-click state should start quiet convergence on the first fresh open-stage snapshot.");
            Assert(historyShadowQuietEvaluator(historyShadowTargetObserver) is null, "Combat history-shadow-only post-click state should keep observing until the quiet convergence window completes.");
            Assert(historyShadowQuietEvaluator(historyShadowTargetObserver) is null, "Combat history-shadow-only post-click state should still defer until the full quiet convergence window completes.");
            Assert(string.Equals(historyShadowQuietEvaluator(historyShadowTargetObserver), "combat-quiet-convergence:playeractionopen", StringComparison.OrdinalIgnoreCase),
                "Combat history-shadow-only post-click state should settle through quiet convergence once live ownership stays open and stable.");
            var historyShadowDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                67,
                GuiSmokePhase.HandleCombat.ToString(),
                "Combat history shadow without live ownership should reopen a new action instead of hard-waiting for card-play resolution.",
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
                historyShadowTargetObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                nonEnemySettleKnowledge,
                historyShadowAllowedActions,
                enemyClickHistory,
                "Combat history shadow without live ownership should not hard-wait in a stale played-action stage.",
                null));
            Assert(!string.Equals(historyShadowDecision.Status, "wait", StringComparison.OrdinalIgnoreCase),
                "Combat history shadow without live ownership should not yield a wait-only combat decision.");

            var staleAttackSelectionObserver = new ObserverState(
                unresolvedTargetObserver.Summary with
                {
                    InventoryId = "inv-combat-attack-stale-open",
                    SnapshotVersion = 57,
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi;CombatState.RoundNumber=3;CombatManager.PlayerActionsDisabled=false;CombatManager.EndingPlayerTurnPhaseOne=false;CombatManager.EndingPlayerTurnPhaseTwo=false",
                        ["combatCardPlayPending"] = "false",
                        ["combatSelectedCardSlot"] = null,
                        ["combatSelectedCardType"] = null,
                        ["combatTargetingInProgress"] = "false",
                        ["combatTargetableEnemyCount"] = "0",
                        ["combatHittableEnemyCount"] = "0",
                        ["combatTargetSummary"] = "enemy-target:Jaw Worm:1@logical:720,180,180,260@normalized:0.3750,0.1667,0.0938,0.2407",
                        ["combatInteractionRevision"] = "5:5:false:false:none",
                        ["combatHistoryStartedCount"] = "5",
                        ["combatHistoryFinishedCount"] = "5",
                    },
                },
                null,
                null,
                null);
            var attackSelectionSettleHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 3", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            var attackSelectionWakeEvaluator = CombatPostActionObservationSupport.CreateWakeEvaluator(
                staleAttackSelectionObserver,
                new GuiSmokeStepDecision("act", "press-key", "3", null, null, "combat select attack slot 3", "stale attack settle", 0.5, "combat", 250, true, null),
                attackSelectionSettleHistory,
                nonEnemySettleKnowledge,
                new WindowBounds(0, 0, 1280, 720));
            Assert(attackSelectionWakeEvaluator(staleAttackSelectionObserver) is null, "Attack selection settle should not treat an unchanged open combat baseline as a cleared selection.");
            Assert(attackSelectionWakeEvaluator(staleAttackSelectionObserver) is null, "Repeated stale attack-settle polls should not quiet-converge without fresh post-action progress.");
            Assert(attackSelectionWakeEvaluator(staleAttackSelectionObserver) is null, "Stale attack-settle fingerprint should keep deferring while no fresh combat progress appears.");
            Assert(attackSelectionWakeEvaluator(staleAttackSelectionObserver) is null, "Attack selection settle should not complete on an unchanged baseline fingerprint.");

            var progressedAttackSelectionObserver = new ObserverState(
                staleAttackSelectionObserver.Summary with
                {
                    InventoryId = "inv-combat-attack-progressed",
                    SnapshotVersion = 58,
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi;CombatState.RoundNumber=3;CombatManager.PlayerActionsDisabled=false;CombatManager.EndingPlayerTurnPhaseOne=false;CombatManager.EndingPlayerTurnPhaseTwo=false",
                        ["combatCardPlayPending"] = "false",
                        ["combatSelectedCardSlot"] = "3",
                        ["combatSelectedCardType"] = "Attack",
                        ["combatSelectedCardTargetType"] = "AnyEnemy",
                        ["combatTargetingInProgress"] = "false",
                        ["combatTargetableEnemyCount"] = "0",
                        ["combatHittableEnemyCount"] = "0",
                        ["combatTargetSummary"] = "enemy-target:Jaw Worm:1@logical:720,180,180,260@normalized:0.3750,0.1667,0.0938,0.2407",
                        ["combatInteractionRevision"] = "6:5:false:false:none",
                        ["combatHistoryStartedCount"] = "6",
                        ["combatHistoryFinishedCount"] = "5",
                    },
                },
                null,
                null,
                null);
            Assert(string.Equals(attackSelectionWakeEvaluator(progressedAttackSelectionObserver), "combat-enemy-target-ready", StringComparison.OrdinalIgnoreCase), "Attack selection settle should wake once fresh post-action progress rebuilds an explicit attack-target stage.");

            var targetedAttackInFlightObserver = new ObserverState(
                progressedAttackSelectionObserver.Summary with
                {
                    InventoryId = "inv-combat-attack-targeting-in-flight",
                    SnapshotVersion = 59,
                    PlayerEnergy = 2,
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi;CombatState.RoundNumber=7;CombatManager.PlayerActionsDisabled=false;CombatManager.EndingPlayerTurnPhaseOne=false;CombatManager.EndingPlayerTurnPhaseTwo=false",
                        ["combatCardPlayPending"] = "true",
                        ["combatSelectedCardSlot"] = "4",
                        ["combatAwaitingPlaySlots"] = "4",
                        ["combatSelectedCardType"] = "Attack",
                        ["combatSelectedCardTargetType"] = "AnyEnemy",
                        ["combatTargetingInProgress"] = "true",
                        ["combatValidTargetsType"] = "AnyEnemy",
                        ["combatTargetableEnemyCount"] = "0",
                        ["combatHittableEnemyCount"] = "0",
                        ["combatTargetSummary"] = "enemy-target:Jaw Worm:1@logical:720,180,180,260@normalized:0.3750,0.1667,0.0938,0.2407",
                        ["combatInteractionRevision"] = "18:17:true:true:4",
                        ["combatHistoryStartedCount"] = "18",
                        ["combatHistoryFinishedCount"] = "17",
                        ["combatLastCardPlayStartedCardId"] = "CARD.STRIKE_IRONCLAD",
                        ["combatLastCardPlayFinishedCardId"] = "CARD.DEFEND_IRONCLAD",
                    },
                },
                null,
                null,
                null);
            var targetedAttackInFlightKnowledge = new[]
            {
                new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var targetedAttackInFlightHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            var targetedAttackContext = GuiSmokeStepRequestFactory.CreateStepAnalysisContext(
                GuiSmokePhase.HandleCombat,
                targetedAttackInFlightObserver,
                combatNoOpScreenshotPath,
                targetedAttackInFlightHistory,
                targetedAttackInFlightKnowledge);
            Assert(targetedAttackContext.CombatMicroStage.Kind == CombatMicroStageKind.ResolvingAttackTarget,
                "Explicit-target attack lanes should stay in ResolvingAttackTarget even while the play remains in flight.");
            Assert(targetedAttackContext.CanResolveCombatEnemyTarget,
                "Explicit-target attack lanes with runtime target summary should reopen enemy targeting authority instead of collapsing into played-card wait.");
            var targetedAttackAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                targetedAttackInFlightObserver,
                targetedAttackInFlightKnowledge,
                combatNoOpScreenshotPath,
                targetedAttackInFlightHistory);
            Assert(targetedAttackAllowedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase),
                "Explicit-target attack lanes should keep click-enemy available while targeting is active.");
            Assert(!targetedAttackAllowedActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase),
                "Explicit-target attack lanes should not collapse into a wait-only allowlist while target authority is available.");
            var targetedAttackDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                68,
                GuiSmokePhase.HandleCombat.ToString(),
                "Explicit-target attack lanes with in-flight play ownership should still click the enemy target instead of waiting for card-play resolution.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:elite|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                targetedAttackInFlightObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                targetedAttackInFlightKnowledge,
                targetedAttackAllowedActions,
                targetedAttackInFlightHistory,
                "Explicit-target attack lanes should preserve enemy targeting authority until the click resolves.",
                null));
            Assert(string.Equals(targetedAttackDecision.ActionKind, "click", StringComparison.OrdinalIgnoreCase)
                   && targetedAttackDecision.TargetLabel?.StartsWith("combat enemy target ", StringComparison.OrdinalIgnoreCase) == true,
                "Explicit-target attack lanes with in-flight play ownership should still choose an enemy click decision.");

            var allEnemiesAttackObserver = new ObserverState(
                progressedAttackSelectionObserver.Summary with
                {
                    InventoryId = "inv-combat-all-enemies-pending",
                    SnapshotVersion = 59,
                    PlayerEnergy = 3,
                    CombatHand = new[]
                    {
                        new ObservedCombatHandCard(1, "CARD.VICIOUS", "Power", 1),
                        new ObservedCombatHandCard(2, "CARD.BASH", "Attack", 2),
                        new ObservedCombatHandCard(3, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    },
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi;CombatState.RoundNumber=3;CombatManager.PlayerActionsDisabled=false;CombatManager.EndingPlayerTurnPhaseOne=false;CombatManager.EndingPlayerTurnPhaseTwo=false",
                        ["combatCardPlayPending"] = "true",
                        ["combatSelectedCardSlot"] = "1",
                        ["combatSelectedCardType"] = "Attack",
                        ["combatSelectedCardTargetType"] = "AllEnemies",
                        ["combatTargetingInProgress"] = "false",
                        ["combatValidTargetsType"] = "None",
                        ["combatTargetableEnemyCount"] = "0",
                        ["combatHittableEnemyCount"] = "0",
                        ["combatInteractionRevision"] = "6:5:true:false:1",
                        ["combatHistoryStartedCount"] = "6",
                        ["combatHistoryFinishedCount"] = "5",
                    },
                },
                null,
                null,
                null);
            var allEnemiesAttackKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.BREAKTHROUGH", "Attack", "AllEnemies", 1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.BASH", "Attack", "AnyEnemy", 2, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var allEnemiesContext = GuiSmokeStepRequestFactory.CreateStepAnalysisContext(
                GuiSmokePhase.HandleCombat,
                allEnemiesAttackObserver,
                combatNoOpScreenshotPath,
                attackSelectionSettleHistory,
                allEnemiesAttackKnowledge);
            Assert(allEnemiesContext.CombatMicroStage.Kind == CombatMicroStageKind.AwaitingCardPlayConfirm,
                "Targetless all-enemies attacks should rebuild as an awaiting-card-play-confirm stage, not as an unresolved explicit target lane.");
            Assert(!allEnemiesContext.CanResolveCombatEnemyTarget,
                "Targetless all-enemies attacks should keep enemy-target authority closed.");
            var allEnemiesAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                allEnemiesAttackObserver,
                allEnemiesAttackKnowledge,
                combatNoOpScreenshotPath,
                attackSelectionSettleHistory);
            Assert(!allEnemiesAllowedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase),
                "Targetless all-enemies attacks should not reopen explicit enemy click actions.");
            Assert(allEnemiesAllowedActions.Contains("confirm selected attack card", StringComparer.OrdinalIgnoreCase),
                "Targetless all-enemies attacks should export an explicit confirm-selected-attack action.");
            var allEnemiesDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                66,
                GuiSmokePhase.HandleCombat.ToString(),
                "Targetless all-enemies attacks should confirm the selected attack card instead of stalling in an explicit target lane.",
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
                allEnemiesAttackObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                allEnemiesAttackKnowledge,
                allEnemiesAllowedActions,
                attackSelectionSettleHistory,
                "Targetless all-enemies attacks should resolve through an explicit confirm-selected-attack step.",
                null));
            Assert(string.Equals(allEnemiesDecision.TargetLabel, "confirm selected attack card", StringComparison.OrdinalIgnoreCase),
                "Targetless all-enemies attacks should drive an explicit confirm-selected-attack step.");
            Assert(string.Equals(allEnemiesDecision.ActionKind, "confirm-attack-card", StringComparison.OrdinalIgnoreCase),
                "Targetless all-enemies attacks should use the dedicated confirm-attack-card actuator.");

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
