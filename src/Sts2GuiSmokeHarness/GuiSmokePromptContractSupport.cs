using System;
using System.Collections.Generic;
using Sts2AiCompanion.Foundation.Contracts;
using static GuiSmokeNonCombatAllowedActionSupport;

static class GuiSmokePromptContractSupport
{
    internal static string BuildGoal(GuiSmokePhase phase)
    {
        return phase switch
        {
            GuiSmokePhase.WaitMainMenu => "Reach main menu and verify observer currentScreen=main-menu.",
            GuiSmokePhase.EnterRun => "Enter a run using Continue first, otherwise Singleplayer.",
            GuiSmokePhase.WaitRunLoad => "Wait for the root-scene transition to finish and for the resumed/new run scene to become actionable.",
            GuiSmokePhase.WaitCharacterSelect => "Wait for the actual singleplayer submenu or character-select flow for a fresh new run.",
            GuiSmokePhase.ChooseCharacter => "Select Ironclad.",
            GuiSmokePhase.Embark => "Click Embark to begin the run.",
            GuiSmokePhase.WaitMap => "Wait until observer logical currentScreen=map. visibleScreen may reach map earlier while reward flow is still active.",
            GuiSmokePhase.HandleRewards => "Resolve the visible reward screen so the run can return to map.",
            GuiSmokePhase.ChooseFirstNode => "Click the first reachable map node.",
            GuiSmokePhase.WaitPostMapNodeRoom => "Reconcile the destination room after clicking a reachable map node.",
            GuiSmokePhase.HandleEvent => "Resolve the event screen. If nothing else is obvious, pick the first visible option.",
            GuiSmokePhase.WaitEventRelease => "Wait for the explicit ancient event completion click to release the event room before map handoff.",
            GuiSmokePhase.HandleShop => "Resolve explicit shop room semantics: open inventory, perform one bounded shop action, then back/proceed.",
            GuiSmokePhase.WaitCombat => "Wait until observer currentScreen=combat and encounter.inCombat=true.",
            GuiSmokePhase.HandleCombat => "Play the combat from the screenshot: choose cards, targets, or end turn until combat resolves.",
            _ => "Complete the scenario.",
        };
    }

    internal static string BuildFailureModeHintCore(
        GuiSmokePhase phase,
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return BuildFailureModeHintCoreWithContext(phase, observer, combatCardKnowledge, screenshotPath, history, null);
    }

    internal static string BuildFailureModeHintCoreWithContext(
        GuiSmokePhase phase,
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        GuiSmokeStepAnalysisContext? analysisContext)
    {
        var context = analysisContext ?? GuiSmokeStepRequestFactory.CreateStepAnalysisContext(phase, observer, screenshotPath, history, combatCardKnowledge);
        var rewardScene = context.RewardScene;
        var preferRewardProgressionOverMapFallback = rewardScene.RewardForegroundOwned
                                                     && rewardScene.ReleaseStage == RewardReleaseStage.Active
                                                     && rewardScene.ExplicitProceedVisible;
        if (phase == GuiSmokePhase.HandleCombat)
        {
            return !context.CanResolveCombatEnemyTarget
                ? Program.BuildCombatFailureModeHint(observer, combatCardKnowledge)
                : "AI first: trust observer/runtime combat state. Use selected-card, targetability, hittability, energy, and player-action-window truth before any screenshot fallback; only inspect the screenshot when explicit combat authority is missing or contradictory.";
        }

        if (phase == GuiSmokePhase.WaitRunLoad)
        {
            return WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(observer.Summary)
                ? "Stable main-menu continue authority is still foreground-visible with no transition evidence. Reopen EnterRun and retry Continue instead of idling in WaitRunLoad."
                : "AI first: trust transition/runtime root-scene state. While transitionInProgress is true, or rootSceneIsRun/currentRunNodePresent is not yet true, wait without acting; only branch once the resumed or newly created run scene becomes explicit.";
        }

        if (phase == GuiSmokePhase.HandleRewards && context.UseRewardFastPath)
        {
            var rewardMapLayer = context.RewardMapLayerState;
            if (LooksLikeInspectOverlayState(observer))
            {
                return "Inspect overlay is not progression. Close it before resolving the explicit reward foreground.";
            }

            if (CardSelectionObserverSignals.TryGetState(observer.Summary) is { } cardSelectionState)
            {
                return cardSelectionState.ScreenType switch
                {
                    "reward-pick" => "AI first: trust the explicit reward-pick card-selection subtype and choose a visible reward card before any screenshot fallback.",
                    _ => "AI first: trust the explicit reward card-selection subtype and use its select/confirm semantics before any screenshot fallback.",
                };
            }

            if (rewardMapLayer.RewardTeardownInProgress || rewardMapLayer.MapCurrentActiveScreen)
            {
                return "Reward ownership has already dropped or map is current. Release to map/post-room reconciliation instead of forcing reward or map fallback from stale visuals.";
            }

            return "AI first: trust reward observer/runtime state. Use reward foreground ownership, explicit reward choices, and proceed availability before any screenshot fallback; only inspect the screenshot when explicit reward authority is missing or contradictory.";
        }

        if (phase == GuiSmokePhase.HandleEvent)
        {
            var eventScene = context.EventScene;
            if (eventScene.RewardSubstateActive)
            {
                return "AI first: this event is currently in a reward substate. Reuse the canonical reward lane, finish reward claim/skip/proceed, and do not fall back to generic event or map routing until reward ownership releases.";
            }

            if (eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.ReleasePending)
            {
                return "Explicit event proceed/completion was already fired on the same authority band. Wait for release or handoff instead of reissuing the same proceed click or reopening map routing early.";
            }
        }

        return phase switch
        {
            GuiSmokePhase.EnterRun => "Continue may be absent. Use Singleplayer only if Continue is not visible.",
            GuiSmokePhase.WaitCharacterSelect => "Only actual singleplayer submenu or character-select authority belongs here. Do not use this phase for Continue or root-scene loading.",
            GuiSmokePhase.ChooseCharacter => "Do not click Embark before Ironclad is selected.",
            GuiSmokePhase.HandleRewards when LooksLikeInspectOverlayState(observer)
                => "Inspect overlay is not progression. Close it with escape or the overlay dismiss affordance before choosing any reward.",
            GuiSmokePhase.HandleRewards when GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer)
                => "This is a colorless reward choice. Pick a visible card first; do not click the small relic inspect icons in the top-left.",
            GuiSmokePhase.HandleRewards when GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer)
                => "Reward follow-up is active. Prefer reward cards or skip/proceed over inspect, preview, or detail affordances.",
            GuiSmokePhase.HandleRewards when preferRewardProgressionOverMapFallback
                => "Reward screen authority is stronger than the background map. Claim the visible reward or use skip/proceed first, and ignore any contaminated map arrow until the reward panel disappears.",
            GuiSmokePhase.HandleRewards when GuiSmokeNonCombatContractSupport.HasStrongMapTransitionEvidence(observer)
                => "Map authority is already stronger than the lingering event label. Prefer a reachable node or screenshot-derived map advance instead of repeating proceed/event clicks.",
            GuiSmokePhase.HandleRewards when string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
                => "AI first: use the screenshot as the primary source. If the map is clearly visible, you may click the first reachable node instead of forcing another reward proceed click.",
            GuiSmokePhase.HandleRewards => "Prefer the proceed arrow when the reward can be skipped; otherwise pick a valid reward card.",
            GuiSmokePhase.ChooseFirstNode when GuiSmokeNonCombatContractSupport.HasExplicitRestSiteChoiceAuthority(observer, screenshotPath)
                => "Rest-site explicit choices are foreground-authoritative here. Prefer 휴식/재련/부화 over any map overlay candidate or current-node arrow. If the smith card grid appears afterward, select a card and then click the right-side confirm button.",
            GuiSmokePhase.ChooseFirstNode when GuiSmokeNonCombatContractSupport.LooksLikeRestSiteState(observer.Summary)
                => "Rest site is screenshot-first. If the explicit rest options are visible, choose one of them before any map candidate. If the smith card grid is visible, click one card first and then click the right-side confirm button.",
            GuiSmokePhase.ChooseFirstNode when TreasureRoomObserverSignals.LooksLikeTreasureState(observer.Summary)
                => "Treasure room authority is explicit. Use chest -> treasure relic holder -> treasure proceed, and ignore top-left inventory relic icons or map-node contamination.",
            GuiSmokePhase.ChooseFirstNode => "Do not click non-reachable map nodes.",
            GuiSmokePhase.HandleShop => "Shop authority is explicit. Open inventory if needed, buy only typed affordable shop entries, keep card removal separate from normal purchases, then back/proceed.",
            GuiSmokePhase.HandleEvent when LooksLikeInspectOverlayState(observer)
                => "Inspect overlay is open inside the room flow. Dismiss it before retrying event, reward, or proceed choices.",
            GuiSmokePhase.HandleEvent when GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer)
                => "The event follow-up is a colorless card choice. Select a card from the card area; do not treat relic previews as event options.",
            GuiSmokePhase.HandleEvent when GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer)
                => "The event has entered a reward substate. Prefer reward cards, reward choices, or skip/proceed over inspect affordances.",
            GuiSmokePhase.HandleEvent when preferRewardProgressionOverMapFallback
                => "Reward screen authority is stronger than the background map. Resolve the visible reward, skip, or proceed affordance before considering any map fallback.",
            GuiSmokePhase.HandleEvent when GuiSmokeNonCombatContractSupport.HasStrongMapTransitionEvidence(observer)
                => "Map evidence is stronger than the stale event label. Prefer reachable map-node or visible-map-advance actions over repeating event progression.",
            GuiSmokePhase.HandleEvent when TreasureRoomObserverSignals.LooksLikeTreasureState(observer.Summary)
                => "Treasure authority can linger on the event phase. Prefer explicit treasure chest, treasure relic holder, or treasure proceed over generic event or map routing.",
            GuiSmokePhase.HandleEvent => "If the event text is ambiguous, choose a large visible progression option, not inspect affordances or detail overlays.",
            GuiSmokePhase.WaitEventRelease => "Ancient proceed was already clicked. Wait for event-room release or a concrete next room state instead of re-clicking the same proceed button.",
            GuiSmokePhase.WaitPostMapNodeRoom => "A reachable node starts room entry, not combat-only flow. Reconcile the destination room from observer truth before waiting or routing again.",
            GuiSmokePhase.WaitCombat => "Observer must end with combat screen and inCombat=true.",
            _ => "Fail closed when screenshot and observer disagree.",
        };
    }
}
