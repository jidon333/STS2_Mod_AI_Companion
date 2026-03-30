using System.Drawing;
using System.Globalization;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

sealed partial class AutoDecisionProvider
{
    private static GuiSmokeStepDecision DecideHandleRewards(GuiSmokeStepRequest request, GuiSmokeStepAnalysisContext? analysisContext = null)
    {
        var canonicalScene = TryBuildCanonicalNonCombatSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath);
        if (canonicalScene is ShopSceneState { ReleaseStage: NonCombatReleaseStage.Active })
        {
            return DecideHandleShop(request with { Phase = GuiSmokePhase.HandleShop.ToString() });
        }

        if (canonicalScene is RestSiteSceneState { ReleaseStage: NonCombatReleaseStage.Active })
        {
            return DecideChooseFirstNode(
                request with { Phase = GuiSmokePhase.ChooseFirstNode.ToString() },
                analysisContext);
        }

        var rewardScene = analysisContext?.RewardScene ?? BuildRewardSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath);
        var overlayDecision = TryCreateRoomOverlayCleanupDecision(request);
        if (overlayDecision is not null)
        {
            return overlayDecision;
        }

        var cardSelectionDecision = TryCreateCardSelectionDecision(request);
        if (cardSelectionDecision is not null)
        {
            return cardSelectionDecision;
        }

        var explicitRewardDecision = TryCreateExplicitRewardResolutionDecision(request, rewardScene);
        if (explicitRewardDecision is not null)
        {
            return explicitRewardDecision;
        }

        var rewardBackDecision = TryCreateRewardBackNavigationDecision(request, rewardScene);
        if (rewardBackDecision is not null)
        {
            return rewardBackDecision;
        }

        if (rewardScene.LayerState.TerminalRunBoundary)
        {
            return CreateWaitDecision("terminal reward boundary is active; do not reopen gameplay map fallback from reward aftermath.", ResolveObserverScreen(request.Observer));
        }

        if (rewardScene.ReleaseToMapPending
            || (!rewardScene.RewardForegroundOwned
                && GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(request.Observer)))
        {
            return CreateRewardOwnershipReleaseWaitDecision(request);
        }

        return CreateForegroundAwareNonCombatWaitDecision(request, "waiting for reward actions");
    }

    private static GuiSmokeStepDecision CreateRewardOwnershipReleaseWaitDecision(GuiSmokeStepRequest request)
    {
        return CreateWaitDecision("waiting for reward ownership release to map/post-room reconciliation", ResolveObserverScreen(request.Observer));
    }

    private static GuiSmokeStepDecision DecideChooseFirstNode(GuiSmokeStepRequest request, GuiSmokeStepAnalysisContext? analysisContext = null)
    {
        var chooseFirstNodeLane = GuiSmokeChooseFirstNodeLaneSupport.Resolve(
            new ObserverState(request.Observer, null, null, request.Observer.LastEventsTail?.ToArray() ?? Array.Empty<string>()),
            request.WindowBounds,
            request.ScreenshotPath,
            request.History,
            analysisContext);
        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.RestSiteExplicitChoice)
        {
            return DecideChooseFirstNodeRestSiteExplicitChoice(request);
        }

        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.RestSiteSmithUpgrade)
        {
            return DecideChooseFirstNodeRestSiteSmithUpgrade(request);
        }

        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.RestSiteProceed)
        {
            return DecideChooseFirstNodeRestSiteProceed(request);
        }

        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.RestSiteSelectionSettling)
        {
            return DecideChooseFirstNodeRestSiteSelectionSettling(request);
        }

        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.TreasureRoom)
        {
            return TryCreateChooseFirstNodeTreasureRoomDecision(request)
                   ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for treasure room progression");
        }

        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.ShopRoom)
        {
            GuiSmokeDecisionDebug.SetSceneModel("shop", "room-context");
            var shopState = ShopObserverSignals.TryGetState(request.Observer)!;
            GuiSmokeDecisionDebug.ReplaceActiveCandidates(ShopObserverSignals.BuildAllowedActions(request.Observer, shopState, ShopObserverSignals.HasRecentPurchase(request.History)));
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "shop-owner-active-preserves-room-lane");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "shop-owner-active-preserves-room-lane");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "shop-owner-active-suppresses-map-arrow-contamination");
            GuiSmokeDecisionDebug.Suppress("click map back", "shop-owner-active-preserves-room-lane");
            var shopDecision = DecideHandleShop(request);
            return string.Equals(shopDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                ? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit shop room progression")
                : shopDecision;
        }

        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.EventRecovery)
        {
            return DecideHandleEvent(request with { Phase = GuiSmokePhase.HandleEvent.ToString() }, analysisContext);
        }

        var mapOverlayState = analysisContext?.MapOverlayState ?? GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.AncientMapPending)
        {
            GuiSmokeDecisionDebug.SetSceneModel("map", "ancient-event");
            GuiSmokeDecisionDebug.ReplaceActiveCandidates(new[] { "wait" });
            return CreateForegroundAwareNonCombatWaitDecision(request, "waiting for ancient event post-proceed map surface");
        }

        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.MapOverlay)
        {
            GuiSmokeDecisionDebug.SetSceneModel("map-overlay", mapOverlayState.EventBackgroundPresent ? "event-context" : "map-context");
            GuiSmokeDecisionDebug.ReplaceActiveCandidates(
                mapOverlayState.MapBackNavigationAvailable
                    ? new[] { "click exported reachable node", "click map back", "wait" }
                    : new[] { "click exported reachable node", "wait" });
            if (mapOverlayState.StaleEventChoicePresent)
            {
                GuiSmokeDecisionDebug.Suppress("click event choice", "map overlay foreground suppresses stale event choices");
                GuiSmokeDecisionDebug.Suppress("click proceed", "map overlay foreground suppresses stale event proceed choices");
            }

            GuiSmokeDecisionDebug.Suppress("click visible map advance", "map overlay foreground suppresses current-node-arrow fallback");
            return TryCreateMapOverlayRoutingDecision(request, "waiting for exported or screenshot-reachable map node");
        }

        return GuiSmokeDecisionDebug.TraceCandidate(
                   "exported reachable map node",
                   "observer-map-node",
                   0.95,
                   TryCreateExportedReachableMapPointDecision(request),
                   "no exported reachable map node bounds available")
               ?? (GuiSmokeNonCombatContractSupport.AllowsAnyMapRoutingAction(request)
                   ? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for reachable map node")
                   : GuiSmokeNonCombatContractSupport.CreateMapRoutingContractWaitDecision(
                       request,
                       "map progression evidence is present but request allowlist does not permit map routing; waiting for exporter/phase reconciliation"));
    }

    private static GuiSmokeStepDecision DecideHandleShop(GuiSmokeStepRequest request)
    {
        var overlayDecision = TryCreateRoomOverlayCleanupDecision(request);
        if (overlayDecision is not null)
        {
            return overlayDecision;
        }

        var cardSelectionDecision = TryCreateCardSelectionDecision(request);
        if (cardSelectionDecision is not null)
        {
            return cardSelectionDecision;
        }

        var shopState = ShopObserverSignals.TryGetState(request.Observer);
        if (shopState is null)
        {
            return CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit shop room authority");
        }

        var screen = ResolveObserverScreen(request.Observer, "shop");
        var alreadyPurchased = ShopObserverSignals.HasRecentPurchase(request.History);
        if (!shopState.InventoryOpen && alreadyPurchased && shopState.ProceedEnabled)
        {
            var proceedChoice = ShopObserverSignals.TryGetProceedChoice(request.Observer);
            if (proceedChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    proceedChoice,
                    "shop proceed",
                    "A bounded shop purchase already happened and proceed is re-enabled. Leave the shop before reopening inventory.",
                    0.93,
                    screen,
                    1300);
            }
        }

        if (!shopState.InventoryOpen && shopState.MerchantButtonEnabled)
        {
            var merchantChoice = ShopObserverSignals.TryGetMerchantButtonChoice(request.Observer);
            if (merchantChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    merchantChoice,
                    "shop open inventory",
                    "Shop room is foreground-active and inventory is closed. Open the merchant inventory before any purchase or proceed action.",
                    0.97,
                    screen,
                    1300);
            }
        }

        if (shopState.InventoryOpen && !alreadyPurchased)
        {
            var relicChoice = ShopObserverSignals.GetAffordableRelicChoices(request.Observer).FirstOrDefault();
            if (relicChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    relicChoice,
                    "shop buy relic",
                    "Merchant inventory is open. Buy one affordable relic before cards, potions, back, or proceed.",
                    0.96,
                    screen,
                    1400);
            }

            var cardChoice = ShopObserverSignals.GetAffordableCardChoices(request.Observer).FirstOrDefault();
            if (cardChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    cardChoice,
                    "shop buy card",
                    "Merchant inventory is open. No affordable relic was chosen, so buy one affordable card before backing out.",
                    0.95,
                    screen,
                    1400);
            }

            var potionChoice = ShopObserverSignals.GetAffordablePotionChoices(request.Observer).FirstOrDefault();
            if (potionChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    potionChoice,
                    "shop buy potion",
                    "Merchant inventory is open. No relic or card purchase is selected, so buy one affordable potion before backing out.",
                    0.94,
                    screen,
                    1400);
            }

            if (shopState.CardRemovalEnabled)
            {
                var cardRemovalChoice = ShopObserverSignals.TryGetCardRemovalChoice(request.Observer);
                if (cardRemovalChoice is not null)
                {
                    return CreateClickDecisionFromChoice(
                        request,
                        cardRemovalChoice,
                        "shop card removal",
                        "Card removal is a distinct merchant service. Use its explicit shop service affordance instead of treating it like a normal card purchase.",
                        0.93,
                        screen,
                        1400);
                }
            }
        }

        if (shopState.InventoryOpen && shopState.BackVisible && shopState.BackEnabled)
        {
            var backChoice = ShopObserverSignals.TryGetBackChoice(request.Observer);
            if (backChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    backChoice,
                    "shop back",
                    "Shop inventory is open and there is no further bounded purchase to make. Close the inventory before proceeding.",
                    0.92,
                    screen,
                    1200);
            }
        }

        if (!shopState.InventoryOpen && shopState.ProceedEnabled)
        {
            var proceedChoice = ShopObserverSignals.TryGetProceedChoice(request.Observer);
            if (proceedChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    proceedChoice,
                    "shop proceed",
                    "Merchant inventory is closed and proceed is re-enabled. Leave the shop explicitly instead of waiting for map routing.",
                    0.91,
                    screen,
                    1300);
            }
        }

        return CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit shop inventory/proceed affordances");
    }

    private static GuiSmokeStepDecision DecideHandleEvent(GuiSmokeStepRequest request, GuiSmokeStepAnalysisContext? analysisContext = null)
    {
        var eventScene = analysisContext?.EventScene ?? BuildEventSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath);
        if (eventScene.RewardSubstateActive)
        {
            return DecideHandleRewards(request with { Phase = GuiSmokePhase.HandleRewards.ToString() }, analysisContext);
        }

        var forceEventProgressionAfterCardSelection = eventScene.ForceProgressionAfterCardSelection;
        var eventOwnerActive = eventScene.EventForegroundOwned
                               && eventScene.ReleaseStage == EventReleaseStage.Active;
        var mapOverlayState = eventScene.MapOverlayState;
        var explicitEventRecoveryAuthority = HasExplicitEventRecoveryAuthority(request.Observer, request.WindowBounds, request.History, eventScene);
        var strongEventForegroundChoice = eventScene.StrongForegroundChoice
                                         || forceEventProgressionAfterCardSelection
                                         || explicitEventRecoveryAuthority;
        if (mapOverlayState.ForegroundVisible && !strongEventForegroundChoice)
        {
            GuiSmokeDecisionDebug.SetSceneModel("map-overlay", mapOverlayState.EventBackgroundPresent ? "event-context" : "map-context");
            GuiSmokeDecisionDebug.ReplaceActiveCandidates(
                mapOverlayState.MapBackNavigationAvailable
                    ? new[] { "click exported reachable node", "click map back", "wait" }
                    : new[] { "click exported reachable node", "wait" });
            GuiSmokeDecisionDebug.Suppress("click event choice", "map overlay foreground suppresses stale event choices");
            GuiSmokeDecisionDebug.Suppress("click proceed", "map overlay foreground suppresses stale event proceed choices");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "map overlay foreground suppresses current-node-arrow fallback");
            return TryCreateMapOverlayRoutingDecision(request, "waiting for map-overlay foreground resolution");
        }

        if (eventOwnerActive)
        {
            GuiSmokeDecisionDebug.SetSceneModel("event", request.SceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase) ? "map-context" : null);
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "event-owner-active-suppresses-map-arrow-contamination");
        }

        var overlayDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "inspect overlay cleanup",
            "overlay-cleanup",
            0.96,
            TryCreateRoomOverlayCleanupDecision(request),
            "no inspect overlay cleanup affordance is visible");
        if (overlayDecision is not null)
        {
            return overlayDecision;
        }

        var cardSelectionDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "card-selection subtype",
            "card-selection-runtime",
            0.96,
            TryCreateCardSelectionDecision(request),
            "no subtype-specific card-selection affordance is currently actionable");
        if (cardSelectionDecision is not null)
        {
            return cardSelectionDecision;
        }

        var treasureDecision = TryCreateTreasureRoomLaneDecision(
            request,
            "waiting for treasure room progression",
            suppressEventChoice: true);
        if (treasureDecision is not null)
        {
            return treasureDecision;
        }

        var explicitRewardDecision = TryCreateRewardForegroundDecision(request);
        if (explicitRewardDecision is not null)
        {
            return explicitRewardDecision;
        }

        var ancientDialogueDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "ancient dialogue advance",
            "ancient-dialogue",
            0.95,
            TryCreateAncientDialogueAdvanceDecision(request),
            "no explicit ancient dialogue hitbox has usable bounds");
        if (AncientEventObserverSignals.IsDialogueActive(request.Observer))
        {
            GuiSmokeDecisionDebug.SetSceneModel("ancient-event-dialogue", "event-context");
            GuiSmokeDecisionDebug.Suppress("click event choice", "ancient dialogue must finish before option selection");
            GuiSmokeDecisionDebug.Suppress("click proceed", "ancient dialogue does not use generic proceed");
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "event-owner-active-preserves-room-lane");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "event-owner-active-preserves-room-lane");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "event-owner-active-suppresses-map-arrow-contamination");
            return ancientDialogueDecision ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit ancient dialogue hitbox");
        }

        var ancientCompletionDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "ancient event completion",
            "ancient-completion-button",
            0.94,
            TryCreateAncientEventCompletionDecision(request),
            "no explicit ancient completion button has usable bounds");
        if (AncientEventObserverSignals.HasExplicitCompletionAction(request.Observer))
        {
            GuiSmokeDecisionDebug.SetSceneModel("ancient-event-completion", "event-context");
            GuiSmokeDecisionDebug.Suppress("click proceed", "ancient completion remains event-owned through the explicit NEventOptionButton proceed lane");
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "event-owner-active-preserves-room-lane");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "event-owner-active-preserves-room-lane");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "event-owner-active-suppresses-map-arrow-contamination");
            return ancientCompletionDecision ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit ancient event completion");
        }

        if (eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.ReleasePending)
        {
            GuiSmokeDecisionDebug.SetSceneModel("event-release-pending", eventScene.MapContextVisible ? "map-context" : "event-context");
            GuiSmokeDecisionDebug.Suppress("click proceed", "event release-pending suppresses same proceed reissue");
            GuiSmokeDecisionDebug.Suppress("click event choice", "event release-pending suppresses stale event reissue");
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "event release-pending waits for explicit release before map routing");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "event release-pending waits for explicit release before map routing");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "event release-pending suppresses stale map contamination");
            return CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit event release");
        }

        var ancientOptionDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "explicit ancient event option",
            "ancient-option-buttons",
            0.93,
            TryCreateAncientEventOptionDecision(request),
            "no explicit ancient event option button has usable bounds");
        if (AncientEventObserverSignals.HasExplicitOptionSelection(request.Observer))
        {
            GuiSmokeDecisionDebug.SetSceneModel("ancient-event-options", "event-context");
            GuiSmokeDecisionDebug.Suppress("click proceed", "ancient options should be selected from explicit option buttons");
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "event-owner-active-preserves-room-lane");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "event-owner-active-preserves-room-lane");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "event-owner-active-suppresses-map-arrow-contamination");
            return ancientOptionDecision ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit ancient event option buttons");
        }

        var ancientOptionContractReconciliationDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "ancient option contract reconciliation",
            "ancient-option-contract-reconciliation",
            0.935,
            TryCreateAncientOptionContractReconciliationDecision(request),
            "no same-family event-option button remains for bounded reconciliation");
        if (AncientEventObserverSignals.HasOptionContractMismatch(request.Observer))
        {
            GuiSmokeDecisionDebug.SetSceneModel("event-choice", "event-context");
            GuiSmokeDecisionDebug.Suppress("click proceed", "ancient option contract mismatch suppresses stale proceed inference");
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "event-owner-active-preserves-room-lane");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "event-owner-active-preserves-room-lane");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "event-owner-active-suppresses-map-arrow-contamination");
            return ancientOptionContractReconciliationDecision
                   ?? CreateAncientOptionContractMismatchAbortDecision(
                       request,
                       "ancient option contract mismatch: foreground lane claims explicit ancient buttons, but only generic event-option buttons remain actionable",
                       "ancient-event-option-contract-mismatch");
        }

        var explicitProceedDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "explicit event proceed",
            "event-proceed-explicit",
            0.95,
            TryCreateExplicitEventProceedDecision(request),
            "no explicit event proceed affordance has usable bounds");
        if (explicitProceedDecision is not null)
        {
            return explicitProceedDecision;
        }

        if (eventOwnerActive)
        {
            var eventProgressionDecision = TryCreateEventProgressionDecision(request);
            if (eventProgressionDecision is not null)
            {
                return eventProgressionDecision;
            }
        }

        if (LooksLikeMapTransitionState(request))
        {
            return DecideChooseFirstNode(request with { Phase = GuiSmokePhase.ChooseFirstNode.ToString() }, analysisContext);
        }

        if (!eventOwnerActive)
        {
            var eventProgressionDecision = TryCreateEventProgressionDecision(request);
            if (eventProgressionDecision is not null)
            {
                return eventProgressionDecision;
            }
        }

        return CreateForegroundAwareNonCombatWaitDecision(request, "waiting for an explicit event progression choice");
    }

    private static GuiSmokeStepDecision? TryCreateCardSelectionDecision(GuiSmokeStepRequest request)
    {
        var state = CardSelectionObserverSignals.TryGetState(request.Observer);
        if (state is null)
        {
            return null;
        }

        return state.ScreenType switch
        {
            "reward-pick" => TryCreateRewardPickDecision(request, state),
            "simple-select" => TryCreateSubtypeCardSelectionDecision(request, state, "simple select choice", "simple select confirm"),
            "bundle-select" => TryCreateSubtypeCardSelectionDecision(request, state, "bundle select choice", "bundle select confirm"),
            "relic-select" => TryCreateSubtypeCardSelectionDecision(request, state, "relic select choice", "relic select confirm"),
            "transform" => TryCreateSubtypeCardSelectionDecision(request, state, "transform select card", "transform confirm"),
            "deck-remove" => TryCreateSubtypeCardSelectionDecision(request, state, "deck remove select card", "deck remove confirm"),
            "upgrade" => TryCreateSubtypeCardSelectionDecision(request, state, "upgrade select card", "upgrade confirm"),
            _ => null,
        };
    }

    private static GuiSmokeStepDecision? TryCreateRewardPickDecision(GuiSmokeStepRequest request, CardSelectionSubtypeState state)
    {
        var explicitChoice = CardSelectionObserverSignals.GetCardChoices(request.Observer, state)
            .Where(choice => choice.Enabled != false && HasActiveRewardBounds(choice.ScreenBounds, request.WindowBounds))
            .OrderBy(GetChoiceSortX)
            .ThenBy(GetChoiceSortY)
            .FirstOrDefault();
        if (explicitChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                explicitChoice,
                BuildCardSelectionCardTargetLabel(state.ScreenType, explicitChoice, 0),
                "Reward-pick subtype is runtime-visible. Pick a reward card directly instead of entering any count/confirm flow.",
                0.96,
                ResolveObserverScreen(request.Observer, "card-choice"),
                1400);
        }

        return CreateForegroundAwareNonCombatWaitDecision(
            request,
            "waiting for explicit reward-pick card surface");
    }

    private static GuiSmokeStepDecision? TryCreateSubtypeCardSelectionDecision(
        GuiSmokeStepRequest request,
        CardSelectionSubtypeState state,
        string selectTargetLabel,
        string confirmTargetLabel)
    {
        if (CardSelectionObserverSignals.IsConfirmReady(state))
        {
            var confirmChoice = CardSelectionObserverSignals.TryGetConfirmChoice(request.Observer, state);
            if (confirmChoice is not null
                && confirmChoice.Enabled != false
                && HasActiveRewardBounds(confirmChoice.ScreenBounds, request.WindowBounds))
            {
                return CreateClickDecisionFromChoice(
                    request,
                    confirmChoice,
                    confirmTargetLabel,
                    $"Card-selection subtype '{state.ScreenType}' is confirm-ready. Use the exported confirm affordance instead of repeating a card click.",
                    0.95,
                    ResolveObserverScreen(request.Observer, state.ScreenType),
                    1400);
            }

            if (state.ScreenType is "simple-select" or "bundle-select")
            {
                return CreateForegroundAwareNonCombatWaitDecision(
                    request,
                    $"waiting for explicit {state.ScreenType} confirm surface");
            }
        }

        var cardChoices = CardSelectionObserverSignals.GetCardChoices(request.Observer, state)
            .Where(choice => choice.Enabled != false && HasActiveRewardBounds(choice.ScreenBounds, request.WindowBounds))
            .ToArray();
        if (cardChoices.Length > 0)
        {
            var preferredChoice = SelectSubtypeCardChoice(request, state, cardChoices);
            if (preferredChoice is not null)
            {
                var choiceIndex = Array.IndexOf(cardChoices, preferredChoice);
                return CreateClickDecisionFromChoice(
                    request,
                    preferredChoice,
                    BuildCardSelectionCardTargetLabel(state.ScreenType, preferredChoice, choiceIndex),
                    $"Card-selection subtype '{state.ScreenType}' is active. Advance it with an explicit exported card hitbox before any generic reward fallback.",
                    0.95,
                    ResolveObserverScreen(request.Observer, state.ScreenType),
                    1400);
            }
        }

        if (state.ScreenType is "simple-select" or "bundle-select" or "relic-select")
        {
            return CreateForegroundAwareNonCombatWaitDecision(
                request,
                $"waiting for explicit {state.ScreenType} selection surface");
        }

        return CreateForegroundAwareNonCombatWaitDecision(
            request,
            $"waiting for explicit {state.ScreenType} selection or confirm surface");
    }

    private static ObserverChoice? SelectSubtypeCardChoice(
        GuiSmokeStepRequest request,
        CardSelectionSubtypeState state,
        IReadOnlyList<ObserverChoice> cardChoices)
    {
        var preferred = cardChoices
            .Where(choice => !CardSelectionObserverSignals.IsSelectedCardChoice(choice))
            .ToArray();
        if (preferred.Length == 0)
        {
            preferred = cardChoices.ToArray();
        }

        var lastTarget = request.History.LastOrDefault(entry =>
            string.Equals(entry.Phase, request.Phase, StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase)
            && entry.TargetLabel?.StartsWith(state.ScreenType.Replace('-', ' '), StringComparison.OrdinalIgnoreCase) == true)?.TargetLabel;

        foreach (var candidate in preferred)
        {
            var targetLabel = BuildCardSelectionCardTargetLabel(state.ScreenType, candidate, Array.IndexOf(cardChoices.ToArray(), candidate));
            if (!string.Equals(lastTarget, targetLabel, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return preferred.FirstOrDefault();
    }

    private static string BuildCardSelectionCardTargetLabel(string screenType, ObserverChoice choice, int index)
    {
        var prefix = screenType switch
        {
            "reward-pick" => "reward pick card",
            "simple-select" => "simple select choice",
            "bundle-select" => "bundle select choice",
            "relic-select" => "relic select choice",
            "deck-remove" => "deck remove select card",
            "upgrade" => "upgrade select card",
            _ => "transform select card",
        };
        var suffix = string.IsNullOrWhiteSpace(choice.Label) ? $" #{index + 1}" : $" {choice.Label}";
        return prefix + suffix;
    }

    private static GuiSmokeStepDecision? TryCreateExplicitRewardResolutionDecision(GuiSmokeStepRequest request)
    {
        return TryCreateExplicitRewardResolutionDecision(
            request,
            BuildRewardSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath));
    }

    private static GuiSmokeStepDecision? TryCreateExplicitRewardResolutionDecision(GuiSmokeStepRequest request, RewardSceneState rewardScene)
    {
        var rewardChoiceDecision = TryCreateRewardChoiceDecision(request, rewardScene);
        if (rewardChoiceDecision is not null)
        {
            return rewardChoiceDecision;
        }

        var claimableRewardChoices = GetClaimableRewardProgressionChoices(request.Observer, request.WindowBounds, rewardScene.ScreenState);
        var claimableRewardNodes = GetClaimableRewardProgressionNodes(request.Observer, request.WindowBounds, rewardScene.ScreenState);
        var activeRewardChoices = request.Observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, request.WindowBounds))
            .ToArray();

        if (!rewardScene.RewardForegroundOwned
            || rewardScene.ReleaseStage != RewardReleaseStage.Active
            || rewardScene.ExplicitAction is not RewardExplicitActionKind.SkipProceed and not RewardExplicitActionKind.Claim and not RewardExplicitActionKind.CardChoice and not RewardExplicitActionKind.ColorlessChoice)
        {
            return null;
        }

        var rewardNode = claimableRewardNodes
            .OrderByDescending(ScoreExplicitRewardProgressionNode)
            .ThenBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        if (rewardNode is not null)
        {
            return CreateExplicitRewardDecisionFromNode(request, rewardNode, activeRewardChoices, "claim");
        }

        var rewardChoice = claimableRewardChoices
            .OrderByDescending(ScoreExplicitRewardProgressionChoice)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (rewardChoice is not null)
        {
            return CreateExplicitRewardDecisionFromChoice(request, rewardChoice, "claim");
        }

        var proceedChoice = request.Observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, request.WindowBounds))
            .Where(choice => IsSkipOrProceedLabel(choice.Label))
            .OrderByDescending(ScoreExplicitRewardProgressionChoice)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (proceedChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                proceedChoice,
                IsSkipLikeLabel(proceedChoice.Label) ? "reward skip" : "proceed after resolving rewards",
                $"Reward proceed choice '{proceedChoice.Label}' is explicitly visible. Use it before any screenshot-derived map fallback.",
                0.92,
                ResolveObserverScreen(request.Observer, "rewards"),
                1400);
        }

        var proceedNode = request.Observer.ActionNodes
            .Where(node => IsCurrentRewardProgressionNode(node, request.WindowBounds, rewardScene.ScreenState, activeRewardChoices))
            .Where(IsProceedNode)
            .OrderByDescending(ScoreExplicitRewardProgressionNode)
            .ThenBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        if (proceedNode is not null)
        {
            return CreateClickDecisionFromNode(request, proceedNode, "proceed after resolving rewards");
        }

        if (rewardScene.ScreenState is { ProceedVisible: true, ProceedEnabled: true }
            && !rewardScene.ClaimableRewardPresent)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.895,
                0.757,
                "proceed after resolving rewards",
                "Reward runtime metadata reports an enabled proceed button even though no explicit reward hitbox was exported. Use the standard bottom-right reward proceed anchor before any map fallback.",
                0.88,
                ResolveObserverScreen(request.Observer, "rewards"),
                1400,
                true,
                null);
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateRoomOverlayCleanupDecision(GuiSmokeStepRequest request)
    {
        if (!LooksLikeInspectOverlayState(request.Observer))
        {
            return null;
        }

        var overlayCleanupAttempts = CountRecentOverlayCleanupAttempts(request.History);
        if (overlayCleanupAttempts >= 2)
        {
            return new GuiSmokeStepDecision(
                "act",
                "press-key",
                "Escape",
                null,
                null,
                "inspect overlay escape",
                "Inspect overlay remained open after repeated dismiss clicks. Send Escape before retrying any room progression.",
                0.84,
                ResolveObserverScreen(request.Observer, "event"),
                1000,
                true,
                null);
        }

        return TryCreateHiddenOverlayCleanupDecision(request)
               ?? TryCreateOverlayAdvanceDecision(request);
    }

    private static GuiSmokeStepDecision? TryCreateRewardBackNavigationDecision(GuiSmokeStepRequest request)
    {
        return TryCreateRewardBackNavigationDecision(
            request,
            BuildRewardSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath));
    }

    private static GuiSmokeStepDecision? TryCreateRewardBackNavigationDecision(GuiSmokeStepRequest request, RewardSceneState rewardScene)
    {
        if (!rewardScene.RewardForegroundOwned
            || rewardScene.ReleaseStage != RewardReleaseStage.Active
            || rewardScene.ExplicitAction != RewardExplicitActionKind.Back
            || rewardScene.LayerState.MapCurrentActiveScreen)
        {
            return null;
        }

        var backNavigationAvailable = rewardScene.LayerState.RewardBackNavigationAvailable;
        if (!backNavigationAvailable || !rewardScene.LayerState.MapContextVisible)
        {
            return null;
        }

        if (!rewardScene.LayerState.StaleRewardChoicePresent)
        {
            return null;
        }

        var backNode = request.Observer.ActionNodes.FirstOrDefault(node => node.Actionable && IsBackNode(node) && HasActiveRewardBounds(node.ScreenBounds, request.WindowBounds));
        if (backNode is not null)
        {
            return CreateClickDecisionFromNode(request, backNode, "reward back");
        }

        return null;
    }

    private static bool LooksLikeRewardBackNavigationAffordanceInScreenshot(ObserverSummary observer, string? screenshotPath)
    {
        if (BuildRewardMapLayerState(observer, null).RewardBackNavigationAvailable)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(screenshotPath) || !File.Exists(screenshotPath))
        {
            return false;
        }

        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(screenshotPath);
        return overlayAnalysis.HasBottomLeftBackArrow
               && !overlayAnalysis.HasCentralOverlayPanel
               && (string.Equals(ObserverScreenProvenance.ControlFlowVisibleScreen(observer), "map", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(ObserverScreenProvenance.ControlFlowCurrentScreen(observer), "rewards", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase));
    }

    private static GuiSmokeStepDecision? TryCreateRewardChoiceDecision(GuiSmokeStepRequest request)
    {
        return TryCreateRewardChoiceDecision(
            request,
            BuildRewardSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath));
    }

    private static GuiSmokeStepDecision? TryCreateRewardChoiceDecision(GuiSmokeStepRequest request, RewardSceneState rewardScene)
    {
        if (!rewardScene.RewardForegroundOwned
            || rewardScene.ReleaseStage != RewardReleaseStage.Active
            || rewardScene.ExplicitAction is not RewardExplicitActionKind.Claim and not RewardExplicitActionKind.CardChoice and not RewardExplicitActionKind.ColorlessChoice)
        {
            return null;
        }

        var claimableRewardChoices = GetClaimableRewardProgressionChoices(request.Observer, request.WindowBounds, rewardScene.ScreenState);
        var claimableRewardNodes = GetClaimableRewardProgressionNodes(request.Observer, request.WindowBounds, rewardScene.ScreenState);

        if (rewardScene.ExplicitAction is RewardExplicitActionKind.CardChoice or RewardExplicitActionKind.ColorlessChoice)
        {
            return TryCreateCardRewardChoiceDecision(request)
                   ?? CreatePhaseWaitDecision(
                       GuiSmokePhase.HandleRewards,
                       "waiting for explicit reward card surface",
                       ResolveObserverScreen(request.Observer, "rewards"));
        }

        var bestChoice = claimableRewardChoices
            .OrderByDescending(ScoreExplicitRewardProgressionChoice)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (bestChoice is not null)
        {
            return CreateExplicitRewardDecisionFromChoice(request, bestChoice, "claim");
        }

        var bestNode = claimableRewardNodes
            .OrderByDescending(ScoreExplicitRewardProgressionNode)
            .ThenBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        if (bestNode is not null)
        {
            return CreateExplicitRewardDecisionFromNode(request, bestNode, request.Observer.Choices, "claim");
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateCardRewardChoiceDecision(GuiSmokeStepRequest request)
    {
        var rewardCardTarget = GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(request.Observer)
            ? "colorless card choice"
            : "reward card choice";
        var explicitRewardCardChoice = request.Observer.Choices
            .Where(choice => IsRewardCardChoice(choice) && HasActiveRewardBounds(choice.ScreenBounds, request.WindowBounds))
            .OrderByDescending(ScoreExplicitRewardProgressionChoice)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (explicitRewardCardChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                explicitRewardCardChoice,
                rewardCardTarget,
                "Reward card row is explicitly visible. Open it before using skip or any map fallback.",
                0.95,
                ResolveObserverScreen(request.Observer, "rewards"),
                1400);
        }

        var semanticRewardCardChoices = request.Observer.Choices
            .Where(IsSemanticRewardCardChoiceWithoutBounds)
            .ToArray();
        if (semanticRewardCardChoices.Length > 0)
        {
            var preferredSemanticChoice = SelectRewardCardSemanticChoice(request, semanticRewardCardChoices);
            var semanticChoiceIndex = Array.IndexOf(semanticRewardCardChoices, preferredSemanticChoice);
            var rewardCardPoint = ResolveRewardCardChoiceAnchor(semanticChoiceIndex, semanticRewardCardChoices.Length);
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                rewardCardPoint.X,
                rewardCardPoint.Y,
                rewardCardTarget,
                "Reward card semantics are explicitly visible even though the exporter omitted per-card bounds. Use the canonical reward-card lane anchor before any inspect affordance or wait fallback.",
                0.82,
                ResolveObserverScreen(request.Observer, "rewards"),
                1400,
                true,
                null);
        }

        var lastTarget = request.History
            .LastOrDefault(entry =>
                string.Equals(entry.Phase, request.Phase, StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase))
            ?.TargetLabel;
        var confirmChoice = request.Observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, request.WindowBounds) && IsConfirmLikeLabel(choice.Label))
            .OrderByDescending(GetChoiceSortX)
            .FirstOrDefault();
        var canChooseCard = !string.Equals(lastTarget, rewardCardTarget, StringComparison.OrdinalIgnoreCase);
        if (canChooseCard)
        {
            return null;
        }

        if (confirmChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                confirmChoice,
                "event confirm",
                "Card selection substate is active. Confirm the current card selection.",
                0.92,
                ResolveObserverScreen(request.Observer, "event"),
                1400);
        }

        return null;
    }

    private static ObserverChoice SelectRewardCardSemanticChoice(
        GuiSmokeStepRequest request,
        IReadOnlyList<ObserverChoice> rewardCardChoices)
    {
        var lastTarget = request.History
            .LastOrDefault(entry =>
                string.Equals(entry.Phase, request.Phase, StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase))
            ?.TargetLabel;
        if (!string.Equals(lastTarget, "reward card choice", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(lastTarget, "colorless card choice", StringComparison.OrdinalIgnoreCase))
        {
            return rewardCardChoices[0];
        }

        return rewardCardChoices.Count > 1
            ? rewardCardChoices[1]
            : rewardCardChoices[0];
    }

    private static GuiSmokeCandidatePoint ResolveRewardCardChoiceAnchor(int index, int count)
    {
        if (count <= 1)
        {
            return new GuiSmokeCandidatePoint(0.50, 0.42);
        }

        if (count == 2)
        {
            return index <= 0
                ? new GuiSmokeCandidatePoint(0.43, 0.42)
                : new GuiSmokeCandidatePoint(0.57, 0.42);
        }

        return index switch
        {
            <= 0 => new GuiSmokeCandidatePoint(0.33, 0.42),
            1 => new GuiSmokeCandidatePoint(0.50, 0.42),
            _ => new GuiSmokeCandidatePoint(0.67, 0.42),
        };
    }

    private static bool IsSemanticRewardCardChoiceWithoutBounds(ObserverChoice choice)
    {
        if (!string.Equals(choice.Kind, "card", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(choice.ScreenBounds)
            || IsSkipOrProceedLabel(choice.Label)
            || IsConfirmLikeLabel(choice.Label))
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(choice.BindingId)
               || !string.IsNullOrWhiteSpace(choice.Value)
               || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint =>
                   string.Equals(hint, "reward-card", StringComparison.OrdinalIgnoreCase)
                   || hint.StartsWith("reward-card:", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(hint, "reward:card-choice", StringComparison.OrdinalIgnoreCase));
    }

    private static GuiSmokeStepDecision CreateExplicitRewardDecisionFromChoice(
        GuiSmokeStepRequest request,
        ObserverChoice choice,
        string mode)
    {
        var (targetLabel, reason, confidence) = BuildExplicitRewardDecisionMetadata(choice, request.Observer, mode);
        return CreateClickDecisionFromChoice(
            request,
            choice,
            targetLabel,
            reason,
            confidence,
            ResolveObserverScreen(request.Observer, "rewards"),
            1400);
    }

    private static GuiSmokeStepDecision CreateExplicitRewardDecisionFromNode(
        GuiSmokeStepRequest request,
        ObserverActionNode node,
        IReadOnlyList<ObserverChoice> activeRewardChoices,
        string mode)
    {
        var (targetLabel, reason, confidence) = BuildExplicitRewardDecisionMetadata(node, activeRewardChoices, request.Observer, mode);
        return CreateClickDecisionFromNode(
            request,
            node,
            targetLabel,
            reason,
            confidence,
            ResolveObserverScreen(request.Observer, "rewards"),
            1400);
    }

    private static (string TargetLabel, string Reason, double Confidence) BuildExplicitRewardDecisionMetadata(
        ObserverChoice choice,
        ObserverSummary observer,
        string mode)
    {
        if (GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer) && IsRewardCardChoice(choice))
        {
            return ("colorless card choice", "Colorless reward choice is explicitly visible. Open it before using skip or proceed.", 0.95);
        }

        if (IsRewardCardChoice(choice))
        {
            return ("reward card choice", "Reward card row is explicitly visible. Open it before using skip or proceed.", 0.95);
        }

        if (IsPotionRewardChoice(choice))
        {
            return ("claim reward potion", $"Reward potion '{choice.Label}' is explicitly visible. Claim it before using proceed or map fallback.", 0.93);
        }

        if (IsRelicRewardChoice(choice))
        {
            return ("claim reward relic", $"Reward relic '{choice.Label}' is explicitly visible. Claim it before using proceed or map fallback.", 0.94);
        }

        if (IsGoldRewardChoice(choice))
        {
            return ("claim reward gold", $"Reward gold '{choice.Label}' is explicitly visible. Claim it before using proceed or map fallback.", 0.92);
        }

        var reason = mode == "claim"
            ? $"Reward choice '{choice.Label}' is explicitly visible. Claim it before using any proceed or map fallback."
            : $"Reward choice '{choice.Label}' is explicitly visible. Open it before using skip or any map fallback.";
        return ("claim reward item", reason, 0.92);
    }

    private static (string TargetLabel, string Reason, double Confidence) BuildExplicitRewardDecisionMetadata(
        ObserverActionNode node,
        IReadOnlyList<ObserverChoice> activeRewardChoices,
        ObserverSummary observer,
        string mode)
    {
        if (GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer) && IsCardRewardNode(node, activeRewardChoices))
        {
            return ("colorless card choice", $"Reward node '{node.Label}' opens a colorless reward choice. Open it before using skip or proceed.", 0.95);
        }

        if (IsCardRewardNode(node, activeRewardChoices))
        {
            return ("reward card choice", $"Reward node '{node.Label}' opens a reward card choice. Open it before using skip or proceed.", 0.95);
        }

        if (IsPotionRewardNode(node, activeRewardChoices))
        {
            return ("claim reward potion", $"Reward node '{node.Label}' is an explicit potion reward. Claim it before using proceed or map fallback.", 0.93);
        }

        if (IsRelicRewardNode(node, activeRewardChoices))
        {
            return ("claim reward relic", $"Reward node '{node.Label}' is an explicit relic reward. Claim it before using proceed or map fallback.", 0.94);
        }

        if (IsGoldRewardNode(node, activeRewardChoices))
        {
            return ("claim reward gold", $"Reward node '{node.Label}' is an explicit gold reward. Claim it before using proceed or map fallback.", 0.92);
        }

        var reason = mode == "claim"
            ? $"Reward node '{node.Label}' is explicitly visible. Claim it before using any proceed or map fallback."
            : $"Reward node '{node.Label}' is explicitly visible. Open it before using skip or any map fallback.";
        return ("claim reward item", reason, 0.92);
    }

    private static GuiSmokeStepDecision? TryCreateEventProgressChoiceDecision(GuiSmokeStepRequest request)
    {
        if (!GuiSmokeNonCombatContractSupport.AllowsAction(request, "click event choice"))
        {
            return null;
        }

        var bestAncientDialogueNode = AncientEventObserverSignals.GetActiveDialogueNode(request.Observer, request.WindowBounds);
        if (bestAncientDialogueNode is not null)
        {
            return CreateClickDecisionFromNode(request, bestAncientDialogueNode, "ancient dialogue advance");
        }

        var bestAncientDialogueChoice = AncientEventObserverSignals.GetActiveDialogueChoice(request.Observer, request.WindowBounds);
        if (bestAncientDialogueChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                bestAncientDialogueChoice,
                "ancient dialogue advance",
                "Ancient event dialogue is still active. Advance it using the explicit dialogue hitbox before selecting an option.",
                0.94,
                ResolveObserverScreen(request.Observer, "event"),
                800);
        }

        var bestNode = request.Observer.ActionNodes
            .Where(node =>
                node.Actionable
                && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds)
                && !AncientEventObserverSignals.IsAncientDialogueNode(node)
                && !EventProceedObserverSignals.IsExplicitEventProceedNode(node)
                && IsExplicitEventChoiceNode(node))
            .OrderByDescending(static node => HasEventChoiceSemantic(node.SemanticHints, includeProceed: false) ? 1 : 0)
            .ThenBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        if (bestNode is not null)
        {
            return CreateClickDecisionFromNode(request, bestNode, GetProgressChoiceTargetLabel(bestNode, request.Observer));
        }

        var bestChoice = request.Observer.Choices
            .Where(choice =>
                HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds)
                && !AncientEventObserverSignals.IsAncientDialogueChoice(choice)
                && !EventProceedObserverSignals.IsExplicitEventProceedChoice(choice)
                && IsExplicitEventChoiceChoice(choice))
            .OrderByDescending(static choice => HasEventChoiceSemantic(choice.SemanticHints, includeProceed: false) ? 1 : 0)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (bestChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                bestChoice,
                GetProgressChoiceTargetLabel(bestChoice, request.Observer),
                BuildProgressChoiceReason(bestChoice, request.Observer),
                0.90,
                ResolveObserverScreen(request.Observer, "event"),
                1400);
        }

        return null;
    }

    private static bool IsExplicitEventChoiceNode(ObserverActionNode node)
    {
        return (node.NodeId?.StartsWith("event-option:", StringComparison.OrdinalIgnoreCase) ?? false)
               || node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
               || string.Equals(node.TypeName, "event-option", StringComparison.OrdinalIgnoreCase)
               || HasEventChoiceSemantic(node.SemanticHints, includeProceed: false);
    }

    private static bool IsExplicitEventChoiceChoice(ObserverChoice choice)
    {
        return string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.BindingKind, "event-option", StringComparison.OrdinalIgnoreCase)
               || HasEventChoiceSemantic(choice.SemanticHints, includeProceed: false);
    }

    private static bool HasEventChoiceSemantic(IReadOnlyList<string> semanticHints, bool includeProceed)
    {
        return semanticHints.Any(hint =>
            string.Equals(hint, "kind:event-option", StringComparison.OrdinalIgnoreCase)
            || string.Equals(hint, "source:event-option", StringComparison.OrdinalIgnoreCase)
            || string.Equals(hint, "source:event-option-button", StringComparison.OrdinalIgnoreCase)
            || string.Equals(hint, "option-role:choice", StringComparison.OrdinalIgnoreCase)
            || (includeProceed && (string.Equals(hint, "option-role:proceed", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(hint, "event-proceed", StringComparison.OrdinalIgnoreCase))));
    }

    private static GuiSmokeStepDecision? TryCreateAncientDialogueAdvanceDecision(GuiSmokeStepRequest request)
    {
        if (!GuiSmokeNonCombatContractSupport.AllowsAction(request, "click ancient dialogue advance"))
        {
            return null;
        }

        var node = AncientEventObserverSignals.GetActiveDialogueNode(request.Observer, request.WindowBounds);
        if (node is not null)
        {
            return CreateClickDecisionFromNode(request, node, "ancient dialogue advance");
        }

        var choice = AncientEventObserverSignals.GetActiveDialogueChoice(request.Observer, request.WindowBounds);
        if (choice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                choice,
                "ancient dialogue advance",
                "Ancient event dialogue is still active. Advance it through the explicit dialogue hitbox before selecting a reward option.",
                0.95,
                ResolveObserverScreen(request.Observer, "event"),
                800);
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateAncientEventOptionDecision(GuiSmokeStepRequest request)
    {
        if (!GuiSmokeNonCombatContractSupport.AllowsAction(request, "click event choice"))
        {
            return null;
        }

        var node = AncientEventObserverSignals.GetActiveOptionNodes(request.Observer, request.WindowBounds)
            .FirstOrDefault();
        if (node is not null)
        {
            return CreateClickDecisionFromNode(request, node, "event progression choice");
        }

        var choice = AncientEventObserverSignals.GetActiveOptionChoices(request.Observer, request.WindowBounds)
            .FirstOrDefault();
        if (choice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                choice,
                "event progression choice",
                $"Ancient event option '{choice.Label}' is exported from an explicit NEventOptionButton. Prefer its real button bounds over generic title/layout pseudo-choices.",
                0.94,
                ResolveObserverScreen(request.Observer, "event"),
                1400);
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateAncientOptionContractReconciliationDecision(GuiSmokeStepRequest request)
    {
        if (!GuiSmokeNonCombatContractSupport.AllowsAction(request, "click event choice"))
        {
            return null;
        }

        var node = AncientEventObserverSignals.GetActiveReconciledOptionNodes(request.Observer, request.WindowBounds)
            .FirstOrDefault();
        if (node is not null)
        {
            return CreateClickDecisionFromNode(
                request,
                node,
                "event progression choice",
                $"Ancient option metadata is stale for this event owner. Bounded reconciliation is using the same actionable NEventOptionButton family exported as generic event choice '{node.Label}'.",
                0.93,
                ResolveObserverScreen(request.Observer, "event"),
                1400);
        }

        var choice = AncientEventObserverSignals.GetActiveReconciledOptionChoices(request.Observer, request.WindowBounds)
            .FirstOrDefault();
        if (choice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                choice,
                "event progression choice",
                $"Ancient option metadata is stale for this event owner. Bounded reconciliation is using the same actionable NEventOptionButton family exported as generic event choice '{choice.Label}'.",
                0.93,
                ResolveObserverScreen(request.Observer, "event"),
                1400);
        }

        return null;
    }

    private static GuiSmokeStepDecision CreateAncientOptionContractMismatchAbortDecision(
        GuiSmokeStepRequest request,
        string reason,
        string decisionRisk)
    {
        return new GuiSmokeStepDecision(
            "abort",
            null,
            null,
            null,
            null,
            null,
            reason,
            0.0,
            ResolveObserverScreen(request.Observer, "event"),
            null,
            true,
            reason,
            null,
            null,
            decisionRisk);
    }

    private static GuiSmokeStepDecision? TryCreateAncientEventCompletionDecision(GuiSmokeStepRequest request)
    {
        if (!GuiSmokeNonCombatContractSupport.AllowsAction(request, "click ancient event completion")
            && !GuiSmokeNonCombatContractSupport.AllowsAction(request, "click event choice"))
        {
            return null;
        }

        if (AncientEventObserverSignals.CompletionUsesDefaultFocus(request.Observer)
            && AncientEventObserverSignals.CompletionHasFocus(request.Observer))
        {
            return CreateNonCombatPressKeyDecision(
                "Enter",
                "ancient event completion",
                "Ancient completion is exported from the default-focused NEventOptionButton and the control already has focus. Use ui_select (Enter) so the canonical focused control handles the release.",
                0.96,
                ResolveObserverScreen(request.Observer, "event"),
                1400);
        }

        var node = AncientEventObserverSignals.GetActiveCompletionNode(request.Observer, request.WindowBounds);
        if (node is not null)
        {
            return CreateClickDecisionFromNode(
                request,
                node,
                "ancient event completion",
                AncientEventObserverSignals.CompletionUsesDefaultFocus(request.Observer)
                    ? "Ancient completion is exported from an explicit NEventOptionButton, but the control does not currently have focus. Use a hover-primed click so NClickableControl can enter its focused mouse-release path before event release."
                    : $"Ancient event completion '{node.Label}' is still exported from an explicit NEventOptionButton proceed lane. Finish the event before handing off to map routing.",
                AncientEventObserverSignals.CompletionUsesDefaultFocus(request.Observer) ? 0.94 : 0.95,
                ResolveObserverScreen(request.Observer, "event"),
                1400);
        }

        var choice = AncientEventObserverSignals.GetActiveCompletionChoice(request.Observer, request.WindowBounds);
        if (choice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                choice,
                "ancient event completion",
                AncientEventObserverSignals.CompletionUsesDefaultFocus(request.Observer)
                    ? "Ancient completion is exported from an explicit NEventOptionButton, but the control does not currently have focus. Use a hover-primed click so NClickableControl can enter its focused mouse-release path before event release."
                    : $"Ancient event completion '{choice.Label}' is still exported from an explicit NEventOptionButton proceed lane. Finish the event before handing off to map routing.",
                AncientEventObserverSignals.CompletionUsesDefaultFocus(request.Observer) ? 0.94 : 0.95,
                ResolveObserverScreen(request.Observer, "event"),
                1400);
        }

        return null;
    }
}
