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
    private static void RunCombatContractsTargetSelectionSelfTests(string combatNoOpScreenshotPath)
    {
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
                        new ObserverActionNode("enemy-target:jaw-worm:1", "enemy-target", "Jaw Worm", "720,180,180,260", true)
                        {
                            TypeName = "enemy-target",
                            SemanticHints = new[] { "combat-targetable", "combat-hittable", "source:target-manager", "target-id:Jaw Worm" },
                        },
                        new ObserverActionNode("enemy-target:cultist:2", "enemy-target", "Cultist", "930,210,180,250", true)
                        {
                            TypeName = "enemy-target",
                            SemanticHints = new[] { "combat-targetable", "combat-hittable", "source:target-manager", "target-id:Cultist" },
                        },
                        new ObserverActionNode("end-turn", "button", "3턴 종료", "1080,620,140,60", true),
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
                        ["combatTargetCount"] = "2",
                        ["combatTargetCoordinateSpace"] = "logical-render",
                        ["combatTargetClickCoordinateSpace"] = "current-window-normalized",
                        ["combatTargetSummary"] = "enemy-target:Jaw Worm:1@logical:720,180,180,260@normalized:0.3750,0.1667,0.0938,0.2407;enemy-target:Cultist:2@logical:930,210,180,250@normalized:0.4844,0.1944,0.0938,0.2315",
                        ["combatTargetableEnemyCount"] = "2",
                        ["combatTargetableEnemyIds"] = "Jaw Worm,Cultist",
                        ["combatHittableEnemyCount"] = "2",
                        ["combatHittableEnemyIds"] = "Jaw Worm,Cultist",
                    },
                },
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
            Assert(
                combatTargetRetryDecision.TargetLabel?.Contains("Cultist", StringComparison.OrdinalIgnoreCase) == true,
                $"After one no-op enemy click, combat recovery should try another observed enemy target when one is available. Actual target='{combatTargetRetryDecision.TargetLabel ?? "<null>"}'.");

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
            Assert(TryParseCombatTargetBounds("1389.943,674.675,72,88", out var logicalBoundsRect), "Expected valid logical target bounds.");
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

    }

    private static bool TryParseCombatTargetBounds(string? raw, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
        {
            return false;
        }

        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
        {
            return false;
        }

        if (width <= 0 || height <= 0)
        {
            return false;
        }

        bounds = new RectangleF(x, y, width, height);
        return true;
    }
}
