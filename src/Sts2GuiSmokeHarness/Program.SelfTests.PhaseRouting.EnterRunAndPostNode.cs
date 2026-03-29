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
    private static void RunPhaseRoutingEnterRunAndPostNodeSelfTests()
    {
        var evaluator = new ObserverAcceptanceEvaluator();
        var postEnterRunCharacterSelectObserver = CreatePhaseRoutingCharacterSelectObserver();
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
        var readyMainMenuObserver = new ObserverState(
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
            null,
            null,
            null);
        Assert(
            evaluator.IsPhaseSatisfied(GuiSmokePhase.WaitMainMenu, readyMainMenuObserver),
            "WaitMainMenu should still accept fresh exported Continue or Singleplayer run-start surfaces.");

        var publishedCharacterSelectObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-character-select-published",
                false,
                "compat",
                "stabilizing",
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
                PublishedCurrentScreen = "character-select",
                PublishedVisibleScreen = "character-select",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "published",
                PublishedSceneStability = "stable",
                CompatibilityCurrentScreen = "main-menu",
                CompatibilityVisibleScreen = "main-menu",
                CompatibilitySceneReady = false,
                CompatibilitySceneAuthority = "compat",
                CompatibilitySceneStability = "stabilizing",
            },
            null,
            null,
            null);
        Assert(
            evaluator.IsPhaseSatisfied(GuiSmokePhase.WaitCharacterSelect, publishedCharacterSelectObserver),
            "WaitCharacterSelect should accept published character-select readiness even when compatibility aliases still lag on main-menu.");
        Assert(
            GuiSmokeObserverPhaseHeuristics.TryGetPostCharacterSelectPhase(publishedCharacterSelectObserver, out var publishedCharacterSelectPhase)
            && publishedCharacterSelectPhase == GuiSmokePhase.ChooseCharacter,
            "Post-character-select routing should prefer published character-select provenance over stale compatibility aliases.");

        var logoAnimationMainMenuObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-logo-main-menu",
                true,
                "mixed",
                "stable",
                "episode-logo-main-menu",
                null,
                "generic",
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
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NLogoAnimation",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NLogoAnimation",
                    ["rootSceneIsMainMenu"] = "true",
                    ["choiceExtractorPath"] = "generic",
                },
            },
            null,
            null,
            null);
        Assert(
            MainMenuRunStartObserverSignals.IsLogoAnimationOnlyMainMenu(logoAnimationMainMenuObserver),
            "Main-menu readiness helper should recognize logo-animation-only bootstrap states.");
        Assert(
            !evaluator.IsPhaseSatisfied(GuiSmokePhase.WaitMainMenu, logoAnimationMainMenuObserver),
            "WaitMainMenu must not accept a logo-animation-only main-menu state before run-start actions exist.");

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
                "Shop scene state should preserve shop ownership, HandleShop handoff, and fast-wait eligibility.");

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
                        "shop.png",
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
    }
}
