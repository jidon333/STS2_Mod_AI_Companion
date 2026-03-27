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
    private static void RunPhaseRoutingSelfTests()
    {
        var request = CreateBaseSelfTestRequest();
        var decision = CreateBaseSelfTestDecision();

        var combatScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-combat-self-test-{Guid.NewGuid():N}.png");
        try
        {
            using (var bitmap = new Bitmap(1280, 720))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var attackBrush = new SolidBrush(Color.FromArgb(220, 80, 50)))
            {
                graphics.Clear(Color.Black);
                graphics.FillRectangle(attackBrush, new Rectangle(230, 550, 140, 150));
                bitmap.Save(combatScreenshotPath, ImageFormat.Png);
            }

            var combatStartDecision = AutoDecisionProvider.Decide(request with
            {
                Phase = GuiSmokePhase.HandleCombat.ToString(),
                Observer = new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    DateTimeOffset.UtcNow,
                    "inv",
                    true,
                    "hook",
                    "stable",
                    "episode-3",
                    "Elite",
                    "combat",
                    57,
                    80,
                    null,
                    new[] { "1턴 종료" },
                    Array.Empty<string>(),
                    Array.Empty<ObserverActionNode>(),
                    Array.Empty<ObserverChoice>(),
                    Array.Empty<ObservedCombatHandCard>()),
                History = new[]
                {
                    new GuiSmokeHistoryEntry(GuiSmokePhase.WaitCombat.ToString(), "observer-accepted", null, DateTimeOffset.UtcNow),
                },
                ScreenshotPath = combatScreenshotPath,
                AllowedActions = new[] { "click card", "click enemy", "click end turn", "wait" },
            });
            Assert(combatStartDecision.ActionKind == "press-key", "Combat opener should start by selecting a visible card with a hotkey.");
            Assert(combatStartDecision.TargetLabel == "combat select attack slot 1", "Combat opener should not skip directly to targeting before selecting a visible attack card.");
        }
        finally
        {
            if (File.Exists(combatScreenshotPath))
            {
                File.Delete(combatScreenshotPath);
            }
        }

        var evaluator = new ObserverAcceptanceEvaluator();
        Assert(
            evaluator.IsPhaseSatisfied(
                GuiSmokePhase.WaitCombat,
                new ObserverState(new ObserverSummary("combat", "combat", true, DateTimeOffset.UtcNow, null, true, "hook", "stable", null, null, null, null, null, null, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ObserverActionNode>(), Array.Empty<ObserverChoice>(), Array.Empty<ObservedCombatHandCard>()), null, null, null)),
            "Combat acceptance should require combat screen and inCombat=true.");
        Assert(
            !evaluator.IsPhaseSatisfied(
                GuiSmokePhase.WaitCombat,
                new ObserverState(new ObserverSummary("combat", "combat", true, DateTimeOffset.UtcNow, null, true, "hook", "transitioning", null, null, null, null, null, null, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ObserverActionNode>(), Array.Empty<ObserverChoice>(), Array.Empty<ObservedCombatHandCard>()), null, null, null)),
            "WaitCombat should reject combat-looking snapshots until sceneStability reaches stable.");
        Assert(
            !evaluator.IsPhaseSatisfied(
                GuiSmokePhase.WaitCombat,
                new ObserverState(new ObserverSummary("combat", "combat", true, DateTimeOffset.UtcNow, null, false, "hook", "stable", null, null, null, null, null, null, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ObserverActionNode>(), Array.Empty<ObserverChoice>(), Array.Empty<ObservedCombatHandCard>()), null, null, null)),
            "WaitCombat should reject combat-looking snapshots until sceneReady is true.");

        var treasureMapVisibleObserver = new ObserverState(
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
                "Treasure",
                "generic",
                76,
                80,
                null,
                new[] { "Chest", "진행", "타격용 인형" },
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                new[]
                {
                    new ObserverChoice("treasure-relic-holder", "타격용 인형", "857.792,357.208,204,204", "Relic", "Treasure room relic holder")
                    {
                        BindingKind = "treasure-room",
                        BindingId = "Relic",
                        Enabled = true,
                        SemanticHints = new[] { "treasure-room", "treasure-relic-holder" },
                    },
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                Meta = new Dictionary<string, string?>
                {
                    ["treasureRoomDetected"] = "true",
                    ["treasureChestOpened"] = "true",
                    ["treasureEnabledRelicHolderCount"] = "1",
                    ["treasureProceedEnabled"] = "false",
                },
            },
            null,
            null,
            null);
        Assert(
            !evaluator.IsPhaseSatisfied(
                GuiSmokePhase.WaitMap,
                treasureMapVisibleObserver,
                new[] { new GuiSmokeHistoryEntry(GuiSmokePhase.ChooseFirstNode.ToString(), "click", "treasure proceed", DateTimeOffset.UtcNow) }),
            "WaitMap should not accept map-visible treasure aftermath while stronger treasure authority remains.");

        var mapOnlyAfterTreasureObserver = treasureMapVisibleObserver with
        {
            Summary = treasureMapVisibleObserver.Summary with
            {
                EncounterKind = null,
                CurrentChoices = Array.Empty<string>(),
                Choices = Array.Empty<ObserverChoice>(),
                Meta = new Dictionary<string, string?>
                {
                    ["treasureRoomDetected"] = "false",
                    ["treasureChestOpened"] = "false",
                    ["treasureEnabledRelicHolderCount"] = "0",
                    ["treasureProceedEnabled"] = "false",
                },
            },
        };
        Assert(
            evaluator.IsPhaseSatisfied(
                GuiSmokePhase.WaitMap,
                mapOnlyAfterTreasureObserver,
                new[] { new GuiSmokeHistoryEntry(GuiSmokePhase.ChooseFirstNode.ToString(), "click", "treasure proceed", DateTimeOffset.UtcNow) }),
            "WaitMap should accept map-visible aftermath once treasure authority has actually cleared.");

        var rewardMixedStateObserver = new ObserverState(
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
        Assert(
            !evaluator.IsPhaseSatisfied(GuiSmokePhase.WaitMap, rewardMixedStateObserver),
            "Reward mixed-state should keep modal foreground authority over map-visible fallback.");

        var cardSelectionMixedStateObserver = new ObserverState(
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
                "card-selection-transform",
                76,
                80,
                null,
                new[] { "Choose cards" },
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                new[]
                {
                    new ObserverChoice("transform-card", "Card A", "400,250,180,260", "CARD.A", "Transform card")
                    {
                        BindingKind = "card-selection-card",
                        BindingId = "CARD.A",
                        Enabled = true,
                        SemanticHints = new[] { "card-selection:transform" },
                    },
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                Meta = new Dictionary<string, string?>
                {
                    ["cardSelectionScreenDetected"] = "true",
                    ["cardSelectionScreenType"] = "transform",
                    ["cardSelectionMaxSelect"] = "2",
                    ["cardSelectionSelectedCount"] = "1",
                },
            },
            null,
            null,
            null);
        Assert(
            !evaluator.IsPhaseSatisfied(GuiSmokePhase.WaitMap, cardSelectionMixedStateObserver),
            "Card-selection mixed-state should beat map-visible WaitMap acceptance.");

        var waitMapMixedStateBranchRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-waitmap-mixed-branch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(waitMapMixedStateBranchRoot);
        try
        {
            var waitMapMixedStateLogger = new ArtifactRecorder(waitMapMixedStateBranchRoot);
            var unstableCombatBranchObserver = new ObserverState(
                new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    DateTimeOffset.UtcNow,
                    null,
                    true,
                    "hook",
                    "transitioning",
                    "episode-unstable-combat",
                    "Combat",
                    "generic",
                    76,
                    80,
                    3,
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    Array.Empty<ObserverActionNode>(),
                    Array.Empty<ObserverChoice>(),
                    Array.Empty<ObservedCombatHandCard>()),
                null,
                null,
                null);
            Assert(
                !TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitCombat,
                    unstableCombatBranchObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitMapMixedStateLogger,
                    5,
                    true,
                    out _),
                "Combat alternate-branch recovery should not promote unstable combat-looking snapshots into HandleCombat.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitMap,
                    treasureMapVisibleObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitMapMixedStateLogger,
                    6,
                    true,
                    out var treasureReopenPhase)
                && treasureReopenPhase == GuiSmokePhase.ChooseFirstNode,
                "WaitMap should reopen the treasure branch when treasure authority remains after map becomes visible.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitMap,
                    rewardMixedStateObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitMapMixedStateLogger,
                    7,
                    true,
                    out var rewardReopenPhase)
                && rewardReopenPhase == GuiSmokePhase.HandleRewards,
                "WaitMap should reopen reward handling instead of idling when reward authority remains over a visible map.");

            var eventMixedStateObserver = new ObserverState(
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
                    "event",
                    76,
                    80,
                    null,
                    new[] { "계속" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("event-option:continue", "event-option", "계속", "860,560,260,88", true),
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "계속", "860,560,260,88", null, "Event continue"),
                    },
                    Array.Empty<ObservedCombatHandCard>()),
                null,
                null,
                null);
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitMap,
                    eventMixedStateObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitMapMixedStateLogger,
                    8,
                    true,
                    out var eventReopenPhase)
                && eventReopenPhase == GuiSmokePhase.HandleEvent,
                "WaitMap should reopen explicit event progression when map becomes visible before the event foreground fully tears down.");

            var ancientCompletionMixedObserver = new ObserverState(
                new ObserverSummary(
                    "event",
                    "event",
                    false,
                    DateTimeOffset.UtcNow,
                    null,
                    true,
                    "mixed",
                    "stable",
                    null,
                    null,
                    "event",
                    80,
                    80,
                    null,
                    new[] { "진행" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("ancient-event-option:0", "event-option", "진행", "460,942,1000,100", true)
                        {
                            SemanticHints = new[] { "scene:event", "ancient-event", "source:ancient-option-button", "option-role:proceed", "ancient-event-completion" },
                        },
                    },
                    new[]
                    {
                        new ObserverChoice("event-option", "진행", "460,942,1000,100", "0", "[gold][b]진행[/b][/gold]")
                        {
                            NodeId = "ancient-event-option:0",
                            Enabled = true,
                            SemanticHints = new[] { "scene:event", "ancient-event", "source:ancient-option-button", "option-role:proceed", "ancient-event-completion" },
                        },
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["ancientEventDetected"] = "true",
                        ["ancientDialogueActive"] = "false",
                        ["ancientOptionCount"] = "1",
                        ["ancientCompletionActive"] = "true",
                        ["ancientCompletionCount"] = "1",
                        ["ancientEventExtractionPath"] = "ancient-completion-button",
                        ["ancientCompletionUsesDefaultFocus"] = "true",
                        ["ancientCompletionHasFocus"] = "true",
                        ["mapCurrentActiveScreen"] = "true",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    },
                },
                null,
                null,
                null);
            Assert(
                GuiSmokeObserverPhaseHeuristics.LooksLikeEventState(
                    ancientCompletionMixedObserver.Summary,
                    "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen"),
                "Explicit ancient post-choice completion should keep event foreground authoritative even when map-active background truth is present.");
            Assert(
                !TryAdvanceAlternateBranch(
                    GuiSmokePhase.HandleEvent,
                    ancientCompletionMixedObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitMapMixedStateLogger,
                    9,
                    true,
                    out _),
                "HandleEvent should not hand off to ChooseFirstNode while an explicit ancient completion button is still active.");
            var ancientCompletionPendingObserver = ancientCompletionMixedObserver with
            {
                Summary = ancientCompletionMixedObserver.Summary with
                {
                    Meta = new Dictionary<string, string?>(ancientCompletionMixedObserver.Summary.Meta, StringComparer.OrdinalIgnoreCase)
                    {
                        ["ancientEventDetected"] = "true",
                        ["ancientDialogueActive"] = "false",
                        ["ancientOptionCount"] = "1",
                        ["ancientCompletionActive"] = "true",
                        ["ancientCompletionCount"] = "1",
                        ["ancientEventExtractionPath"] = "ancient-completion-button",
                        ["ancientCompletionUsesDefaultFocus"] = "true",
                        ["ancientCompletionHasFocus"] = "true",
                        ["mapCurrentActiveScreen"] = "false",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom",
                    },
                },
            };
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.ChooseFirstNode,
                    ancientCompletionPendingObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitMapMixedStateLogger,
                    10,
                    true,
                    out var ancientCompletionRecoveryPhase)
                && ancientCompletionRecoveryPhase == GuiSmokePhase.HandleEvent,
                "ChooseFirstNode drift should recover back to HandleEvent while explicit ancient completion remains active.");
            Assert(
                !TryAdvanceAlternateBranch(
                    GuiSmokePhase.ChooseFirstNode,
                    ancientCompletionMixedObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitMapMixedStateLogger,
                    11,
                    true,
                    out _),
                "ChooseFirstNode should not reopen HandleEvent once ancient proceed has already opened the map as the current active screen.");

            var ancientCompletionReleasedObserver = new ObserverState(
                new ObserverSummary(
                    "event",
                    "event",
                    false,
                    DateTimeOffset.UtcNow,
                    null,
                    true,
                    "mixed",
                    "stable",
                    null,
                    null,
                    "map",
                    80,
                    80,
                    null,
                    Array.Empty<string>(),
                    new[] { "{\"kind\":\"screen-changed\",\"screen\":\"map\"}" },
                    Array.Empty<ObserverActionNode>(),
                    new[]
                    {
                        new ObserverChoice("map-node", "Monster (1,3)", "904,524,56,56", "1,3", null)
                        {
                            NodeId = "map:1:3",
                            Enabled = true,
                        },
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["ancientEventDetected"] = "true",
                        ["ancientDialogueActive"] = "false",
                        ["ancientOptionCount"] = "0",
                        ["ancientCompletionActive"] = "false",
                        ["ancientCompletionCount"] = "0",
                        ["mapCurrentActiveScreen"] = "true",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    },
                },
                null,
                null,
                null);
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.HandleEvent,
                    ancientCompletionReleasedObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitMapMixedStateLogger,
                    12,
                    true,
                    out var ancientCompletionReleasedPhase)
                && ancientCompletionReleasedPhase == GuiSmokePhase.ChooseFirstNode,
                "Once the explicit ancient completion lane is gone, HandleEvent should release control to map routing.");
            Assert(
                GetAllowedActions(GuiSmokePhase.HandleEvent, ancientCompletionMixedObserver).Contains("click ancient event completion", StringComparer.OrdinalIgnoreCase)
                && !GetAllowedActions(GuiSmokePhase.HandleEvent, ancientCompletionMixedObserver).Contains("click event choice", StringComparer.OrdinalIgnoreCase),
                "HandleEvent allowlist should expose ancient completion as its own explicit action instead of collapsing it into generic event choice.");
            var ancientCompletionRequest = new GuiSmokeStepRequest(
                "run",
                "ancient-completion-self-test",
                12,
                GuiSmokePhase.HandleEvent.ToString(),
                "handle-event",
                DateTimeOffset.UtcNow,
                "ancient-completion.png",
                new WindowBounds(0, 0, 1280, 720),
                "scene:ancient-completion",
                "0001",
                1,
                1,
                true,
                "semantic",
                "Resolve the ancient event.",
                ancientCompletionMixedObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                new[] { "click ancient event completion", "wait" },
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Ancient completion should resolve through the explicit proceed lane.",
                null);
            var ancientCompletionDecision = AutoDecisionProvider.Decide(ancientCompletionRequest);
            Assert(
                string.Equals(ancientCompletionDecision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase)
                && string.Equals(ancientCompletionDecision.KeyText, "Enter", StringComparison.OrdinalIgnoreCase),
                "Ancient completion should use ui_select (Enter) when the explicit proceed button is also the default-focused control.");
            var ancientCompletionNeedsHoverObserver = ancientCompletionMixedObserver with
            {
                Summary = ancientCompletionMixedObserver.Summary with
                {
                    Meta = new Dictionary<string, string?>(ancientCompletionMixedObserver.Summary.Meta, StringComparer.OrdinalIgnoreCase)
                    {
                        ["ancientEventDetected"] = "true",
                        ["ancientDialogueActive"] = "false",
                        ["ancientOptionCount"] = "1",
                        ["ancientCompletionActive"] = "true",
                        ["ancientCompletionCount"] = "1",
                        ["ancientEventExtractionPath"] = "ancient-completion-button",
                        ["ancientCompletionUsesDefaultFocus"] = "true",
                        ["ancientCompletionHasFocus"] = "false",
                        ["mapCurrentActiveScreen"] = "true",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    },
                },
            };
            var ancientCompletionHoverRequest = ancientCompletionRequest with
            {
                Observer = ancientCompletionNeedsHoverObserver.Summary,
            };
            var ancientCompletionHoverDecision = AutoDecisionProvider.Decide(ancientCompletionHoverRequest);
            Assert(
                string.Equals(ancientCompletionHoverDecision.ActionKind, "click", StringComparison.OrdinalIgnoreCase),
                "Ancient completion should fall back to an explicit button click when the proceed control is the default focus candidate but does not actually have focus.");
            Assert(
                GetPostHandleEventPhase(ancientCompletionDecision) == GuiSmokePhase.WaitEventRelease,
                "Ancient completion clicks should enter a passive event-release wait phase instead of re-entering HandleEvent.");
            Assert(
                !TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitEventRelease,
                    ancientCompletionPendingObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitMapMixedStateLogger,
                    13,
                    true,
                    out _),
                "WaitEventRelease should remain passive while the explicit ancient completion control is still foreground-active and map release authority has not yet appeared.");
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitEventRelease,
                    ancientCompletionMixedObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitMapMixedStateLogger,
                    14,
                    true,
                    out var ancientEventReleasePhaseFromMapActive)
                && ancientEventReleasePhaseFromMapActive == GuiSmokePhase.ChooseFirstNode,
                "WaitEventRelease should hand off to map routing once ancient proceed has opened the map as the current active screen, even if stale ancient completion residue is still visible.");
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitEventRelease,
                    ancientCompletionReleasedObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitMapMixedStateLogger,
                    15,
                    true,
                    out var ancientEventReleasePhase)
                && ancientEventReleasePhase == GuiSmokePhase.ChooseFirstNode,
                "WaitEventRelease should hand off to map routing once ancient completion is actually gone and map authority remains.");
            Assert(
                GuiSmokeNonCombatContractSupport.TryMapNonCombatAllowedAction(ancientCompletionDecision, out var mappedAncientCompletionAction)
                && string.Equals(mappedAncientCompletionAction, "click ancient event completion", StringComparison.OrdinalIgnoreCase),
                "Ancient completion target labels should map to their own non-combat allowlist action.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitMap,
                    cardSelectionMixedStateObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitMapMixedStateLogger,
                    16,
                    true,
                    out var cardSelectionReopenPhase)
                && cardSelectionReopenPhase == GuiSmokePhase.ChooseFirstNode,
                "WaitMap should reopen card-selection handling while subtype authority remains visible over the map.");
        }
        finally
        {
            try
            {
                Directory.Delete(waitMapMixedStateBranchRoot, true);
            }
            catch
            {
            }
        }

        var postEnterRunCharacterSelectObserver = new ObserverState(
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
        Assert(
            GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(postEnterRunCharacterSelectObserver, out var postEnterRunCharacterSelectPhase)
            && postEnterRunCharacterSelectPhase == GuiSmokePhase.ChooseCharacter,
            "Run-load reconciliation should still recognize fresh new-run character-select flows once they are actually visible.");

        var continuePreferredDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            2,
            GuiSmokePhase.EnterRun.ToString(),
            "Enter a run using Continue first, otherwise Singleplayer.",
            DateTimeOffset.UtcNow,
            "screen.png",
            new WindowBounds(0, 0, 1280, 720),
            "phase:enter-run|screen:main-menu|visible:main-menu|ready:true",
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-main-menu",
                true,
                "main-menu",
                "stable",
                null,
                null,
                null,
                null,
                null,
                null,
                new[] { "Continue", "Singleplayer" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("continue", "action", "Continue", "620,560,420,96", true),
                    new ObserverActionNode("singleplayer", "action", "Singleplayer", "620,680,420,96", true),
                },
                new[]
                {
                    new ObserverChoice("choice", "Continue", "620,560,420,96"),
                    new ObserverChoice("choice", "Singleplayer", "620,680,420,96"),
                },
                Array.Empty<ObservedCombatHandCard>()),
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            Array.Empty<string>(),
            Array.Empty<GuiSmokeHistoryEntry>(),
            string.Empty,
            null));
        Assert(
            string.Equals(continuePreferredDecision.TargetLabel, "continue", StringComparison.OrdinalIgnoreCase),
            "EnterRun should still prefer Continue when it is visible on the main menu.");
        Assert(
            GetPostEnterRunPhase(continuePreferredDecision) == GuiSmokePhase.WaitRunLoad,
            "Continue should hand off to neutral run-load waiting, not WaitCharacterSelect.");

        var reachableNodeDecision = new GuiSmokeStepDecision("act", "click", null, null, null, "visible reachable node", "Map node selected.", 0.9, null, null, null, null);
        Assert(
            GetPostChooseFirstNodePhase(reachableNodeDecision) == GuiSmokePhase.WaitPostMapNodeRoom,
            "Reachable map-node clicks should enter neutral post-node room reconciliation instead of combat-only waiting.");
        var exportedReachableNodeDecision = new GuiSmokeStepDecision("act", "click", null, null, null, "exported reachable map node", "Map node selected from explicit export.", 0.95, null, null, null, null);
        Assert(
            GetPostChooseFirstNodePhase(exportedReachableNodeDecision) == GuiSmokePhase.WaitPostMapNodeRoom,
            "Exported reachable map-node clicks should enter neutral post-node room reconciliation instead of falling back to WaitMap.");

        var combatAfterMapTransitionObserver = new ObserverState(
            new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                null,
                true,
                "mixed",
                "stable",
                null,
                "Boss",
                "combat",
                38,
                80,
                3,
                new[] { "1턴 종료" },
                new[]
                {
                    "{\"kind\":\"map-point-selected\",\"screen\":\"map\"}",
                    "{\"kind\":\"screen-changed\",\"screen\":\"map\"}",
                    "{\"kind\":\"combat-started\",\"screen\":\"combat\"}",
                },
                Array.Empty<ObserverActionNode>(),
                Array.Empty<ObserverChoice>(),
                Array.Empty<ObservedCombatHandCard>())
            {
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom",
                    ["combatPrimaryValue"] = "true",
                },
            },
            null,
            null,
            null);
        Assert(
            GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(combatAfterMapTransitionObserver.Summary),
            "Combat observer with NCombatRoom authority should still be recognized as combat after a map-node click.");
        Assert(
            !GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(combatAfterMapTransitionObserver),
            "Stale map transition tails must not outvote stronger combat truth once the destination room is combat.");
        Assert(
            !evaluator.IsPhaseSatisfied(GuiSmokePhase.WaitMap, combatAfterMapTransitionObserver),
            "WaitMap should not accept a combat observer just because stale map-transition tails are still present.");

        var waitPostMapNodeBranchRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-wait-post-map-node-branch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(waitPostMapNodeBranchRoot);
        try
        {
            var waitPostMapNodeLogger = new ArtifactRecorder(waitPostMapNodeBranchRoot);
            Assert(
                GuiSmokeObserverPhaseHeuristics.TryGetPostMapNodePhase(
                    new ObserverState(
                        new ObserverSummary(
                            "combat",
                            "combat",
                            true,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "hook",
                            "stable",
                            null,
                            "Monster",
                            "combat",
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
                        null),
                    out var postMapNodeCombatPhase)
                && postMapNodeCombatPhase == GuiSmokePhase.HandleCombat,
                "Post-node reconciliation should still recognize combat destinations.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitPostMapNodeRoom,
                    new ObserverState(
                        new ObserverSummary(
                            "combat",
                            "combat",
                            true,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "hook",
                            "stable",
                            null,
                            "Monster",
                            "combat",
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
                        null),
                    new List<GuiSmokeHistoryEntry>(),
                    waitPostMapNodeLogger,
                    10,
                    true,
                    out var waitPostMapNodeCombatBranchPhase)
                && waitPostMapNodeCombatBranchPhase == GuiSmokePhase.HandleCombat,
                "WaitPostMapNodeRoom should reopen combat handling when the destination room is combat.");
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.ChooseFirstNode,
                    combatAfterMapTransitionObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitPostMapNodeLogger,
                    11,
                    true,
                    out var chooseFirstNodeCombatBranchPhase)
                && chooseFirstNodeCombatBranchPhase == GuiSmokePhase.HandleCombat,
                "ChooseFirstNode should recover directly to combat handling if phase bookkeeping drifts after a real map-node click.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitPostMapNodeRoom,
                    new ObserverState(
                        new ObserverSummary(
                            "rest-site",
                            "rest-site",
                            false,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "rest",
                            "stable",
                            null,
                            "RestSite",
                            "rest",
                            64,
                            80,
                            null,
                            new[] { "휴식", "재련", "부화" },
                            Array.Empty<string>(),
                            Array.Empty<ObserverActionNode>(),
                            new[]
                            {
                                new ObserverChoice("rest-option", "휴식", "520,360,220,160", null, "Rest")
                                {
                                    NodeId = "rest-site:HEAL",
                                    BindingKind = "rest-site-option",
                                    Enabled = true,
                                    SemanticHints = new[] { "scene:rest-site", "option-id:HEAL", "source:button" },
                                },
                            },
                            Array.Empty<ObservedCombatHandCard>()),
                        null,
                        null,
                        null),
                    new List<GuiSmokeHistoryEntry>(),
                    waitPostMapNodeLogger,
                    11,
                    true,
                    out var waitPostMapNodeRestSitePhase)
                && waitPostMapNodeRestSitePhase == GuiSmokePhase.ChooseFirstNode,
                "WaitPostMapNodeRoom should reopen rest-site handling instead of stalling in combat wait.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitPostMapNodeRoom,
                    new ObserverState(
                        new ObserverSummary(
                            "shop",
                            "shop",
                            false,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "shop",
                            "stable",
                            null,
                            "Shop",
                            "shop",
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
                        null),
                    new List<GuiSmokeHistoryEntry>(),
                    waitPostMapNodeLogger,
                    12,
                    true,
                    out var waitPostMapNodeShopPhase)
                && waitPostMapNodeShopPhase == GuiSmokePhase.HandleShop,
                "WaitPostMapNodeRoom should reopen shop handling when the destination room is shop.");

            ObserverState CreateShopObserver(
                bool inventoryOpen,
                bool merchantButtonVisible = false,
                bool merchantButtonEnabled = false,
                bool proceedEnabled = false,
                bool backVisible = false,
                bool backEnabled = false,
                bool roomVisible = true,
                bool foregroundOwned = true,
                bool teardownInProgress = false,
                bool shopIsCurrentActiveScreen = true,
                bool mapCurrentActiveScreen = false,
                string? activeScreenType = "MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantRoom",
                int affordableOptionCount = 0,
                bool cardRemovalVisible = false,
                bool cardRemovalEnabled = false,
                bool cardRemovalEnoughGold = false,
                bool cardRemovalUsed = false,
                ObserverChoice[]? choices = null)
            {
                var summary = new ObserverSummary(
                    "shop",
                    "shop",
                    false,
                    DateTimeOffset.UtcNow,
                    null,
                    true,
                    "shop",
                    "stable",
                    null,
                    "Shop",
                    "shop",
                    80,
                    80,
                    null,
                    choices?.Select(static choice => choice.Label).ToArray() ?? Array.Empty<string>(),
                    Array.Empty<string>(),
                    Array.Empty<ObserverActionNode>(),
                    choices ?? Array.Empty<ObserverChoice>(),
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["shopRoomDetected"] = "true",
                        ["shopRoomVisible"] = roomVisible.ToString().ToLowerInvariant(),
                        ["shopForegroundOwned"] = foregroundOwned.ToString().ToLowerInvariant(),
                        ["shopTeardownInProgress"] = teardownInProgress.ToString().ToLowerInvariant(),
                        ["shopIsCurrentActiveScreen"] = shopIsCurrentActiveScreen.ToString().ToLowerInvariant(),
                        ["mapCurrentActiveScreen"] = mapCurrentActiveScreen.ToString().ToLowerInvariant(),
                        ["activeScreenType"] = activeScreenType,
                        ["shopRootType"] = "MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantRoom",
                        ["shopInventoryOpen"] = inventoryOpen.ToString().ToLowerInvariant(),
                        ["shopMerchantButtonVisible"] = merchantButtonVisible.ToString().ToLowerInvariant(),
                        ["shopMerchantButtonEnabled"] = merchantButtonEnabled.ToString().ToLowerInvariant(),
                        ["shopProceedEnabled"] = proceedEnabled.ToString().ToLowerInvariant(),
                        ["shopBackVisible"] = backVisible.ToString().ToLowerInvariant(),
                        ["shopBackEnabled"] = backEnabled.ToString().ToLowerInvariant(),
                        ["shopOptionCount"] = (choices?.Count(static choice => choice.Kind.StartsWith("shop-option", StringComparison.OrdinalIgnoreCase) || string.Equals(choice.Kind, "shop-card-removal", StringComparison.OrdinalIgnoreCase)) ?? 0).ToString(CultureInfo.InvariantCulture),
                        ["shopAffordableOptionCount"] = affordableOptionCount.ToString(CultureInfo.InvariantCulture),
                        ["shopCardRemovalVisible"] = cardRemovalVisible.ToString().ToLowerInvariant(),
                        ["shopCardRemovalEnabled"] = cardRemovalEnabled.ToString().ToLowerInvariant(),
                        ["shopCardRemovalEnoughGold"] = cardRemovalEnoughGold.ToString().ToLowerInvariant(),
                        ["shopCardRemovalUsed"] = cardRemovalUsed.ToString().ToLowerInvariant(),
                    },
                };
                return new ObserverState(summary, null, null, null);
            }

            GuiSmokeStepDecision DecideShop(ObserverState observer, IReadOnlyList<GuiSmokeHistoryEntry>? history = null)
            {
                history ??= Array.Empty<GuiSmokeHistoryEntry>();
                return AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                    "run",
                    "shop-self-test",
                    12,
                    GuiSmokePhase.HandleShop.ToString(),
                    "Resolve explicit shop room semantics.",
                    DateTimeOffset.UtcNow,
                    "screen.png",
                    new WindowBounds(0, 0, 1280, 720),
                    "phase:shop|screen:shop|visible:shop|encounter:shop|ready:true|stability:stable|room:shop",
                    "shop-self-test",
                    1,
                    1,
                    true,
                    "tactical",
                    null,
                    observer.Summary,
                    Array.Empty<KnownRecipeHint>(),
                    Array.Empty<EventKnowledgeCandidate>(),
                    Array.Empty<CombatCardKnowledgeHint>(),
                    BuildAllowedActions(GuiSmokePhase.HandleShop, observer, Array.Empty<CombatCardKnowledgeHint>(), null, history),
                    history,
                    "shop self-test",
                    null));
            }

            var embarkShopObserver = CreateShopObserver(inventoryOpen: true);
            Assert(GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(embarkShopObserver, out var postEmbarkShopPhase) && postEmbarkShopPhase == GuiSmokePhase.HandleShop, "Embark should reconcile to HandleShop when observer already reports a shop room.");

            var shopOpenInventoryObserver = CreateShopObserver(
                inventoryOpen: false,
                merchantButtonVisible: true,
                merchantButtonEnabled: true,
                choices: new[]
                {
                    new ObserverChoice("shop-open-inventory", "Merchant", "420,320,240,180")
                    {
                        BindingKind = "shop-room",
                        BindingId = "merchant-button",
                        Enabled = true,
                    },
                });
            Assert(BuildAllowedActions(GuiSmokePhase.HandleShop, shopOpenInventoryObserver, Array.Empty<CombatCardKnowledgeHint>(), null, Array.Empty<GuiSmokeHistoryEntry>()).Contains("click shop open inventory", StringComparer.OrdinalIgnoreCase), "HandleShop allowlist should open merchant inventory when it is closed and the merchant button is enabled.");
            Assert(string.Equals(DecideShop(shopOpenInventoryObserver).TargetLabel, "shop open inventory", StringComparison.OrdinalIgnoreCase), "HandleShop should click the explicit merchant button when inventory is closed.");
            Assert(AutoDecisionProvider.BuildShopSceneState(shopOpenInventoryObserver, Array.Empty<GuiSmokeHistoryEntry>()) is
                {
                    CanonicalForegroundOwner: NonCombatCanonicalForegroundOwner.Shop,
                    HandoffTarget: NonCombatHandoffTarget.HandleShop,
                    AllowsFastForegroundWait: true,
                },
                "Shop wrapper should preserve shop ownership, HandleShop handoff, and fast-wait eligibility.");

            var shopRelicObserver = CreateShopObserver(
                inventoryOpen: true,
                backVisible: true,
                backEnabled: true,
                affordableOptionCount: 1,
                choices: new[]
                {
                    new ObserverChoice("shop-option:relic", "게임용 말", "420,320,180,180")
                    {
                        BindingKind = "shop-option",
                        BindingId = "RELIC.GAME_PIECE",
                        Enabled = true,
                        SemanticHints = new[] { "scene:shop", "shop-type:relic" },
                    },
                    new ObserverChoice("shop-back", "Back", "220,160,140,100")
                    {
                        BindingKind = "shop-room",
                        BindingId = "back",
                        Enabled = true,
                    },
                });
            Assert(string.Equals(DecideShop(shopRelicObserver).TargetLabel, "shop buy relic", StringComparison.OrdinalIgnoreCase), "HandleShop should buy an affordable relic before backing out.");

            var shopCardObserver = CreateShopObserver(
                inventoryOpen: true,
                backVisible: true,
                backEnabled: true,
                affordableOptionCount: 1,
                choices: new[]
                {
                    new ObserverChoice("shop-option:card", "몸풀기", "420,320,180,180")
                    {
                        BindingKind = "shop-option",
                        BindingId = "CARD.WARM_UP",
                        Enabled = true,
                        SemanticHints = new[] { "scene:shop", "shop-type:card" },
                    },
                });
            Assert(string.Equals(DecideShop(shopCardObserver).TargetLabel, "shop buy card", StringComparison.OrdinalIgnoreCase), "HandleShop should buy an affordable card when no relic is chosen.");

            var shopPotionObserver = CreateShopObserver(
                inventoryOpen: true,
                backVisible: true,
                backEnabled: true,
                affordableOptionCount: 1,
                choices: new[]
                {
                    new ObserverChoice("shop-option:potion", "힘 포션", "420,320,180,180")
                    {
                        BindingKind = "shop-option",
                        BindingId = "POTION.STRENGTH_POTION",
                        Enabled = true,
                        SemanticHints = new[] { "scene:shop", "shop-type:potion" },
                    },
                });
            Assert(string.Equals(DecideShop(shopPotionObserver).TargetLabel, "shop buy potion", StringComparison.OrdinalIgnoreCase), "HandleShop should buy an affordable potion when no relic or card is chosen.");

            var shopCardRemovalObserver = CreateShopObserver(
                inventoryOpen: true,
                backVisible: true,
                backEnabled: true,
                affordableOptionCount: 0,
                cardRemovalVisible: true,
                cardRemovalEnabled: true,
                cardRemovalEnoughGold: true,
                choices: new[]
                {
                    new ObserverChoice("shop-card-removal", "카드 제거 서비스", "420,320,180,180")
                    {
                        BindingKind = "shop-card-removal",
                        BindingId = "card-removal",
                        Enabled = true,
                        SemanticHints = new[] { "scene:shop", "shop-type:card-removal", "enough-gold:true" },
                    },
                });
            Assert(string.Equals(DecideShop(shopCardRemovalObserver).TargetLabel, "shop card removal", StringComparison.OrdinalIgnoreCase), "HandleShop should treat card removal as a distinct shop service, not a normal card purchase.");

            var shopBackObserver = CreateShopObserver(
                inventoryOpen: true,
                backVisible: true,
                backEnabled: true,
                choices: new[]
                {
                    new ObserverChoice("shop-back", "Back", "220,160,140,100")
                    {
                        BindingKind = "shop-room",
                        BindingId = "back",
                        Enabled = true,
                    },
                });
            Assert(string.Equals(DecideShop(shopBackObserver).TargetLabel, "shop back", StringComparison.OrdinalIgnoreCase), "HandleShop should close inventory explicitly when no bounded purchase remains.");

            var shopProceedObserver = CreateShopObserver(
                inventoryOpen: false,
                proceedEnabled: true,
                choices: new[]
                {
                    new ObserverChoice("shop-proceed", "Proceed", "980,820,240,120")
                    {
                        BindingKind = "shop-room",
                        BindingId = "proceed",
                        Enabled = true,
                    },
                });
            Assert(string.Equals(DecideShop(shopProceedObserver).TargetLabel, "shop proceed", StringComparison.OrdinalIgnoreCase), "HandleShop should click explicit shop proceed when inventory is closed and proceed is enabled.");

            var staleVisibleShopObserver = CreateShopObserver(
                inventoryOpen: false,
                merchantButtonVisible: true,
                merchantButtonEnabled: false,
                proceedEnabled: false,
                backVisible: false,
                backEnabled: false,
                roomVisible: true,
                foregroundOwned: false,
                teardownInProgress: true,
                shopIsCurrentActiveScreen: false,
                mapCurrentActiveScreen: true,
                activeScreenType: "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                choices: new[]
                {
                    new ObserverChoice("shop-open-inventory", "Merchant", "420,320,240,180")
                    {
                        BindingKind = "shop-room",
                        BindingId = "merchant-button",
                        Enabled = false,
                    },
                });
            Assert(!ShopObserverSignals.IsShopAuthorityActive(staleVisibleShopObserver.Summary), "Visible-but-disabled merchant remnants should not keep shop foreground authority after proceed teardown starts.");
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.HandleShop,
                    staleVisibleShopObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    new ArtifactRecorder(Path.Combine(Path.GetTempPath(), $"gui-smoke-handle-shop-release-{Guid.NewGuid():N}")),
                    12,
                    true,
                    out var postShopPhase)
                && postShopPhase == GuiSmokePhase.WaitMap,
                "HandleShop should release ownership to map aftermath reconciliation when shop teardown is active.");

            var shopProceedAfterPurchaseActions = BuildAllowedActions(
                GuiSmokePhase.HandleShop,
                CreateShopObserver(
                    inventoryOpen: false,
                    merchantButtonVisible: true,
                    merchantButtonEnabled: true,
                    proceedEnabled: true,
                    choices: new[]
                    {
                        new ObserverChoice("shop-open-inventory", "Merchant", "420,320,240,180")
                        {
                            BindingKind = "shop-room",
                            BindingId = "merchant-button",
                            Enabled = true,
                        },
                        new ObserverChoice("shop-proceed", "Proceed", "980,820,240,120")
                        {
                            BindingKind = "shop-room",
                            BindingId = "proceed",
                            Enabled = true,
                        },
                    }),
                Array.Empty<CombatCardKnowledgeHint>(),
                null,
                new[]
                {
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleShop.ToString(), "click", "shop buy card", DateTimeOffset.UtcNow.AddSeconds(-2))
                    {
                        Metadata = "bought one card",
                    },
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleShop.ToString(), "click", "shop back", DateTimeOffset.UtcNow.AddSeconds(-1))
                    {
                        Metadata = "closed inventory",
                    },
                });
            Assert(shopProceedAfterPurchaseActions.Contains("click shop proceed", StringComparer.OrdinalIgnoreCase), "HandleShop allowlist should prioritize explicit proceed after a bounded purchase closes the inventory.");
            Assert(string.Equals(
                    AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                        "run",
                        "boot-to-long-run",
                        12,
                        GuiSmokePhase.HandleShop.ToString(),
                        "Prefer shop proceed after a bounded purchase.",
                        DateTimeOffset.UtcNow,
                        null,
                        new WindowBounds(0, 0, 1280, 720),
                        "phase:handleshop|screen:shop",
                        "0001",
                        1,
                        3,
                        true,
                        "tactical",
                        null,
                        CreateShopObserver(
                            inventoryOpen: false,
                            merchantButtonVisible: true,
                            merchantButtonEnabled: true,
                            proceedEnabled: true,
                            choices: new[]
                            {
                                new ObserverChoice("shop-open-inventory", "Merchant", "420,320,240,180")
                                {
                                    BindingKind = "shop-room",
                                    BindingId = "merchant-button",
                                    Enabled = true,
                                },
                                new ObserverChoice("shop-proceed", "Proceed", "980,820,240,120")
                                {
                                    BindingKind = "shop-room",
                                    BindingId = "proceed",
                                    Enabled = true,
                                },
                            }).Summary,
                        Array.Empty<KnownRecipeHint>(),
                        Array.Empty<EventKnowledgeCandidate>(),
                        Array.Empty<CombatCardKnowledgeHint>(),
                        shopProceedAfterPurchaseActions,
                        new[]
                        {
                            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleShop.ToString(), "click", "shop buy card", DateTimeOffset.UtcNow.AddSeconds(-2))
                            {
                                Metadata = "bought one card",
                            },
                            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleShop.ToString(), "click", "shop back", DateTimeOffset.UtcNow.AddSeconds(-1))
                            {
                                Metadata = "closed inventory",
                            },
                        },
                        "Prefer explicit proceed after a bounded shop purchase.",
                        null)).TargetLabel,
                    "shop proceed",
                    StringComparison.OrdinalIgnoreCase), "HandleShop should click proceed before reopening inventory once a bounded purchase already happened.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitPostMapNodeRoom,
                    new ObserverState(
                        new ObserverSummary(
                            "event",
                            "event",
                            false,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "event",
                            "stable",
                            null,
                            "None",
                            "event",
                            80,
                            80,
                            null,
                            new[] { "선택지" },
                            Array.Empty<string>(),
                            new[] { new ObserverActionNode("event-option:0", "event-option", "선택지", "500,700,800,100", true) },
                            new[] { new ObserverChoice("choice", "선택지", "500,700,800,100") },
                            Array.Empty<ObservedCombatHandCard>()),
                        null,
                        null,
                        null),
                    new List<GuiSmokeHistoryEntry>(),
                    waitPostMapNodeLogger,
                    13,
                    true,
                    out var waitPostMapNodeEventPhase)
                && waitPostMapNodeEventPhase == GuiSmokePhase.HandleEvent,
                "WaitPostMapNodeRoom should reopen event handling when the destination room is an event.");
        }
        finally
        {
            try
            {
                Directory.Delete(waitPostMapNodeBranchRoot, true);
            }
            catch
            {
            }
        }

        var postEnterRunTreasureObserver = new ObserverState(
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
                "Treasure",
                "generic",
                76,
                80,
                null,
                new[] { "Chest", "진행", "Chest" },
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                new[]
                {
                    new ObserverChoice("treasure-chest", "Chest", "820,360,280,240", "Chest", "Treasure chest")
                    {
                        BindingKind = "treasure-room",
                        BindingId = "chest",
                        Enabled = true,
                        SemanticHints = new[] { "treasure-room", "treasure-chest" },
                    },
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                Meta = new Dictionary<string, string?>
                {
                    ["treasureRoomDetected"] = "true",
                    ["treasureChestClickable"] = "true",
                    ["treasureChestOpened"] = "false",
                    ["treasureEnabledRelicHolderCount"] = "0",
                    ["treasureProceedEnabled"] = "false",
                    ["mapScreenOpen"] = "false",
                },
            },
            null,
            null,
            null);
        Assert(
            GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(postEnterRunTreasureObserver, out var postEnterRunTreasurePhase)
            && postEnterRunTreasurePhase == GuiSmokePhase.ChooseFirstNode,
            "Run-load reconciliation should resolve to ChooseFirstNode when Continue resumes directly into a treasure room.");

        var waitRunLoadTransitionObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                null,
                false,
                "transition",
                "transition",
                null,
                null,
                null,
                null,
                null,
                null,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                Array.Empty<ObserverChoice>(),
                Array.Empty<ObservedCombatHandCard>())
            {
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["transitionInProgress"] = "true",
                    ["rootSceneIsMainMenu"] = "true",
                    ["rootSceneIsRun"] = "false",
                    ["currentRunNodePresent"] = "false",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                },
            },
            null,
            null,
            null);
        Assert(
            !GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(waitRunLoadTransitionObserver, out _),
            "Run-load reconciliation should not branch while explicit transition truth is still active.");
        Assert(
            RootSceneTransitionObserverSignals.ShouldTreatCaptureAsTransitionWait(GuiSmokePhase.WaitRunLoad, waitRunLoadTransitionObserver.Summary),
            "Explicit run-load transition truth should suppress generic black-frame recovery nudges.");
        Assert(
            BuildAllowedActions(GuiSmokePhase.WaitRunLoad, waitRunLoadTransitionObserver, Array.Empty<CombatCardKnowledgeHint>(), null, Array.Empty<GuiSmokeHistoryEntry>()).SequenceEqual(new[] { "wait" }),
            "WaitRunLoad should remain wait-only while explicit transition truth is still active.");

        var waitRunLoadNotReadyRewardObserver = new ObserverState(
            new ObserverSummary(
                "rewards",
                "map",
                false,
                DateTimeOffset.UtcNow,
                null,
                false,
                "mixed",
                "stabilizing",
                null,
                null,
                "reward",
                59,
                80,
                0,
                new[] { "{gold} 골드", "덱에 추가할 카드를 선택하세요.", "넘기기", "{gold} 골드" },
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                new[]
                {
                    new ObserverChoice("gold", "GoldReward", "460,250,240,340"),
                    new ObserverChoice("card", "CardReward", "740,250,240,340"),
                    new ObserverChoice("choice", "넘기기", "860,900,220,80")
                    {
                        Enabled = true,
                    },
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["transitionInProgress"] = "false",
                    ["rootSceneIsMainMenu"] = "false",
                    ["rootSceneIsRun"] = "true",
                    ["currentRunNodePresent"] = "true",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.NRun",
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                    ["rewardScreenDetected"] = "true",
                    ["rewardScreenVisible"] = "true",
                    ["rewardForegroundOwned"] = "true",
                    ["rewardTeardownInProgress"] = "false",
                    ["rewardIsCurrentActiveScreen"] = "true",
                    ["rewardIsTopOverlay"] = "true",
                    ["rewardProceedVisible"] = "true",
                    ["rewardProceedEnabled"] = "true",
                    ["rewardVisibleButtonCount"] = "2",
                    ["rewardEnabledButtonCount"] = "2",
                    ["mapCurrentActiveScreen"] = "false",
                    ["choiceExtractorPath"] = "reward",
                },
            },
            null,
            null,
            null);
        Assert(
            GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(waitRunLoadNotReadyRewardObserver, out var waitRunLoadNotReadyRewardPhase)
            && waitRunLoadNotReadyRewardPhase == GuiSmokePhase.HandleRewards,
            "WaitRunLoad should hand off to rewards when non-ready resumed run authority is already explicitly reward-owned.");

        var waitRunLoadNotReadyMapObserver = new ObserverState(
            new ObserverSummary(
                "map",
                "map",
                false,
                DateTimeOffset.UtcNow,
                null,
                false,
                "mixed",
                "stabilizing",
                null,
                null,
                "generic",
                59,
                80,
                0,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                Array.Empty<ObserverChoice>(),
                Array.Empty<ObservedCombatHandCard>())
            {
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["transitionInProgress"] = "false",
                    ["rootSceneIsMainMenu"] = "false",
                    ["rootSceneIsRun"] = "true",
                    ["currentRunNodePresent"] = "true",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.NRun",
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    ["mapCurrentActiveScreen"] = "true",
                },
            },
            null,
            null,
            null);
        Assert(
            !GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(waitRunLoadNotReadyMapObserver, out _),
            "WaitRunLoad should stay blocked while scene is not ready when only map authority is present.");

        var waitRunLoadStuckContinueObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                null,
                true,
                "mixed",
                "stable",
                "main-menu",
                null,
                null,
                24,
                24,
                null,
                new[] { "\uACC4\uC18D", "\uBA40\uD2F0\uD50C\uB808\uC774", "\uC885\uB8CC" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("main-menu:continue", "continue-run", "\uACC4\uC18D", "676,659,200,50", true)
                    {
                        TypeName = "continue-run",
                        SemanticHints = new[] { "scene:main-menu", "kind:continue-run", "value:main-menu:continue" },
                    },
                },
                new[]
                {
                    new ObserverChoice("continue-run", "\uACC4\uC18D", "676,659,200,50", "main-menu:continue", "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenuContinueButton")
                    {
                        NodeId = "main-menu:continue",
                        BindingKind = "continue-run",
                        Enabled = true,
                    },
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["transitionInProgress"] = "false",
                    ["rootSceneIsMainMenu"] = "true",
                    ["rootSceneIsRun"] = "false",
                    ["currentRunNodePresent"] = "false",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    ["terminalRunBoundary"] = "true",
                    ["mainMenuReturnDetected"] = "true",
                    ["choiceExtractorPath"] = "main-menu",
                },
            },
            null,
            null,
            null);
        Assert(
            WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(waitRunLoadStuckContinueObserver.Summary),
            "Stable main-menu continue authority without transition evidence should reopen EnterRun from WaitRunLoad.");
        Assert(
            !RootSceneTransitionObserverSignals.ShouldTreatCaptureAsTransitionWait(GuiSmokePhase.WaitRunLoad, waitRunLoadStuckContinueObserver.Summary),
            "Stable main-menu continue authority should not be treated as an in-flight run-load transition.");
        var waitRunLoadRetryActions = BuildAllowedActions(
            GuiSmokePhase.WaitRunLoad,
            waitRunLoadStuckContinueObserver,
            Array.Empty<CombatCardKnowledgeHint>(),
            null,
            Array.Empty<GuiSmokeHistoryEntry>());
        Assert(
            waitRunLoadRetryActions.Contains("click continue", StringComparer.OrdinalIgnoreCase)
            && waitRunLoadRetryActions.Contains("wait", StringComparer.OrdinalIgnoreCase),
            "WaitRunLoad should reopen EnterRun actions when Continue remains visible on a stable main menu.");
        var waitRunLoadRetryDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            6,
            GuiSmokePhase.WaitRunLoad.ToString(),
            "Wait for run-load readiness.",
            DateTimeOffset.UtcNow,
            "screen.png",
            new WindowBounds(0, 0, 1280, 720),
            "phase:wait-run-load|screen:main-menu|visible:main-menu|ready:true|stability:stable",
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            waitRunLoadStuckContinueObserver.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            waitRunLoadRetryActions,
            Array.Empty<GuiSmokeHistoryEntry>(),
            string.Empty,
            null));
        Assert(
            string.Equals(waitRunLoadRetryDecision.ActionKind, "click", StringComparison.OrdinalIgnoreCase)
            && string.Equals(waitRunLoadRetryDecision.TargetLabel, "continue", StringComparison.OrdinalIgnoreCase),
            "WaitRunLoad should retry Continue instead of returning a passive wait decision when the stable main-menu continue surface persists.");

        var waitRunLoadTerminalMainMenuObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                null,
                true,
                "mixed",
                "stable",
                "main-menu",
                null,
                null,
                24,
                24,
                null,
                new[] { "\uBA40\uD2F0\uD50C\uB808\uC774", "\uC885\uB8CC" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("main-menu:multiplayer", "menu-action", "\uBA40\uD2F0\uD50C\uB808\uC774", "676,759,200,50", true),
                },
                new[]
                {
                    new ObserverChoice("menu-action", "\uBA40\uD2F0\uD50C\uB808\uC774", "676,759,200,50", "main-menu:multiplayer", "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenuTextButton")
                    {
                        NodeId = "main-menu:multiplayer",
                        Enabled = true,
                    },
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["transitionInProgress"] = "false",
                    ["rootSceneIsMainMenu"] = "true",
                    ["rootSceneIsRun"] = "false",
                    ["currentRunNodePresent"] = "false",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    ["terminalRunBoundary"] = "true",
                    ["mainMenuReturnDetected"] = "true",
                    ["choiceExtractorPath"] = "main-menu",
                },
            },
            null,
            null,
            null);
        Assert(
            !WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(waitRunLoadTerminalMainMenuObserver.Summary),
            "Terminal main-menu returns without Continue should not be retried as run-load recovery.");

        var waitCharacterSelectBranchRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-wait-character-select-branch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(waitCharacterSelectBranchRoot);
        try
        {
            var waitCharacterSelectLogger = new ArtifactRecorder(waitCharacterSelectBranchRoot);
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitRunLoad,
                    waitRunLoadStuckContinueObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    4,
                    true,
                    out var waitRunLoadRetryPhase)
                && waitRunLoadRetryPhase == GuiSmokePhase.EnterRun,
                "WaitRunLoad should bounce back to EnterRun when Continue remains visible on a stable main-menu surface without transition evidence.");

            Assert(
                !TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitRunLoad,
                    waitRunLoadTerminalMainMenuObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    5,
                    true,
                    out _),
                "WaitRunLoad should not misclassify terminal main-menu returns without Continue as run-load retries.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitRunLoad,
                    postEnterRunTreasureObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    6,
                    true,
                    out var waitRunLoadTreasurePhase)
                && waitRunLoadTreasurePhase == GuiSmokePhase.ChooseFirstNode,
                "WaitRunLoad should reopen treasure handling when Continue resumes directly into a treasure room.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitRunLoad,
                    rewardMixedStateObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    7,
                    true,
                    out var waitRunLoadRewardPhase)
                && waitRunLoadRewardPhase == GuiSmokePhase.HandleRewards,
                "WaitRunLoad should branch to rewards when Continue resumes into a reward foreground.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitRunLoad,
                    new ObserverState(
                        new ObserverSummary(
                            "event",
                            "event",
                            false,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "mixed",
                            "stable",
                            null,
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
                        null),
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    8,
                    true,
                    out var waitRunLoadEventPhase)
                && waitRunLoadEventPhase == GuiSmokePhase.HandleEvent,
                "WaitRunLoad should branch to event handling when Continue resumes into an event room.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitRunLoad,
                    new ObserverState(
                        new ObserverSummary(
                            "combat",
                            "combat",
                            true,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "hook",
                            "stable",
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            Array.Empty<string>(),
                            Array.Empty<string>(),
                            Array.Empty<ObserverActionNode>(),
                            Array.Empty<ObserverChoice>(),
                            Array.Empty<ObservedCombatHandCard>()),
                        null,
                        null,
                        null),
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    9,
                    true,
                    out var waitRunLoadCombatPhase)
                && waitRunLoadCombatPhase == GuiSmokePhase.HandleCombat,
                "WaitRunLoad should branch to combat handling when Continue resumes into combat.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitCharacterSelect,
                    postEnterRunCharacterSelectObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    10,
                    true,
                    out var waitCharacterSelectPhase)
                && waitCharacterSelectPhase == GuiSmokePhase.ChooseCharacter,
                "WaitCharacterSelect should only reopen the actual character-select flow.");
        }
        finally
        {
            try
            {
                Directory.Delete(waitCharacterSelectBranchRoot, true);
            }
            catch
            {
            }
        }

        Assert(
            WindowLocator.HasMeaningfulDrift(
                new WindowCaptureTarget(IntPtr.Zero, "before", new Rectangle(100, 200, 1000, 800), false, false),
                new WindowCaptureTarget(IntPtr.Zero, "after", new Rectangle(140, 200, 1000, 800), false, false)),
            "Bounds drift should be detected when the window moved.");

        Assert(
            TryParseScreenBounds("100,200,40,20", out var parsedBounds)
            && Math.Abs(parsedBounds.X - 100f) < 0.01f
            && Math.Abs(parsedBounds.Width - 40f) < 0.01f,
            "Screen bounds should parse from observer inventory strings.");

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
        Assert(TryClassifyDecisionWaitPlateau(GuiSmokePhase.HandleEvent, embarkEventObserver, 5, out var waitPlateauCause, out _) && string.Equals(waitPlateauCause, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase), "Repeated waits in a stable room scene should escalate to decision-wait-plateau.");
    }
}
