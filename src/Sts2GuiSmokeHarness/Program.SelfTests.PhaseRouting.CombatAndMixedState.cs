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
using static GuiSmokePostActionPhaseSupport;

internal static partial class Program
{
    private static void RunPhaseRoutingCombatAndMixedStateSelfTests(GuiSmokeStepRequest request)
    {
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
                new ObserverState(
                    new ObserverSummary("combat", "combat", true, DateTimeOffset.UtcNow, null, true, "hook", "stable", null, null, null, null, null, null, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ObserverActionNode>(), Array.Empty<ObserverChoice>(), Array.Empty<ObservedCombatHandCard>())
                    {
                        PublishedCurrentScreen = "combat",
                        PublishedVisibleScreen = "combat",
                        PublishedSceneReady = true,
                        PublishedSceneAuthority = "hook",
                        PublishedSceneStability = "stable",
                    },
                    null,
                    null,
                    null)),
            "Combat acceptance should require combat screen and inCombat=true.");
        Assert(
            !evaluator.IsPhaseSatisfied(
                GuiSmokePhase.WaitCombat,
                new ObserverState(
                    new ObserverSummary("combat", "combat", true, DateTimeOffset.UtcNow, null, true, "hook", "transitioning", null, null, null, null, null, null, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ObserverActionNode>(), Array.Empty<ObserverChoice>(), Array.Empty<ObservedCombatHandCard>())
                    {
                        PublishedCurrentScreen = "combat",
                        PublishedVisibleScreen = "combat",
                        PublishedSceneReady = true,
                        PublishedSceneAuthority = "hook",
                        PublishedSceneStability = "transitioning",
                    },
                    null,
                    null,
                    null)),
            "WaitCombat should reject combat-looking snapshots until sceneStability reaches stable.");
        Assert(
            !evaluator.IsPhaseSatisfied(
                GuiSmokePhase.WaitCombat,
                new ObserverState(
                    new ObserverSummary("combat", "combat", true, DateTimeOffset.UtcNow, null, false, "hook", "stable", null, null, null, null, null, null, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ObserverActionNode>(), Array.Empty<ObserverChoice>(), Array.Empty<ObservedCombatHandCard>())
                    {
                        PublishedCurrentScreen = "combat",
                        PublishedVisibleScreen = "combat",
                        PublishedSceneReady = false,
                        PublishedSceneAuthority = "hook",
                        PublishedSceneStability = "stable",
                    },
                    null,
                    null,
                    null)),
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
                PublishedCurrentScreen = "map",
                PublishedVisibleScreen = "map",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stable",
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
    }
}
