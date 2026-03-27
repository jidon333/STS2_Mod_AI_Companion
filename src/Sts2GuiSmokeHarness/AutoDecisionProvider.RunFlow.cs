sealed partial class AutoDecisionProvider
{
    private static GuiSmokeStepDecision DecideEnterRun(GuiSmokeStepRequest request)
    {
        if (string.Equals(request.Observer.CurrentScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.Observer.VisibleScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase))
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.275,
                0.445,
                "normal mode",
                "The singleplayer submenu is visible. Click the left 'normal' panel to start a standard run.",
                0.94,
                "singleplayer-submenu",
                1200,
                true,
                null);
        }

        return TryFindActionNodeDecision(request, "Continue", "continue")
               ?? TryFindActionNodeDecision(request, "계속", "continue")
               ?? TryFindActionNodeDecision(request, "Singleplayer", "singleplayer")
               ?? TryFindActionNodeDecision(request, "싱글", "singleplayer")
               ?? CreateWaitDecision("main menu actions not yet visible", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideChooseCharacter(GuiSmokeStepRequest request)
    {
        if (string.Equals(request.Observer.CurrentScreen, "character-select", StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.Observer.VisibleScreen, "character-select", StringComparison.OrdinalIgnoreCase))
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.955,
                0.723,
                "character confirm",
                "Ironclad is already highlighted on the character-select screen. Click the right confirm checkmark to continue.",
                0.94,
                "character-select",
                1000,
                true,
                null);
        }

        return TryFindActionNodeDecision(request, "Ironclad", "ironclad")
               ?? CreateWaitDecision("waiting for ironclad node", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideWaitRunLoad(GuiSmokeStepRequest request)
    {
        if (GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(request.Observer, out var postRunLoadPhase))
        {
            return postRunLoadPhase switch
            {
                GuiSmokePhase.HandleRewards => DecideHandleRewards(request with { Phase = GuiSmokePhase.HandleRewards.ToString() }),
                GuiSmokePhase.HandleEvent => DecideHandleEvent(request with { Phase = GuiSmokePhase.HandleEvent.ToString() }),
                GuiSmokePhase.HandleShop => DecideHandleShop(request with { Phase = GuiSmokePhase.HandleShop.ToString() }),
                GuiSmokePhase.HandleCombat => DecideHandleCombat(request with { Phase = GuiSmokePhase.HandleCombat.ToString() }),
                GuiSmokePhase.ChooseFirstNode => DecideChooseFirstNode(request with { Phase = GuiSmokePhase.ChooseFirstNode.ToString() }),
                GuiSmokePhase.ChooseCharacter => DecideChooseCharacter(request with { Phase = GuiSmokePhase.ChooseCharacter.ToString() }),
                _ => CreateWaitDecision("waiting for post-run-load room state", request.Observer.CurrentScreen),
            };
        }

        if (WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(request.Observer))
        {
            return DecideEnterRun(request with { Phase = GuiSmokePhase.EnterRun.ToString() });
        }

        return CreateWaitDecision("waiting for root-scene transition and run load readiness", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideEmbark(GuiSmokeStepRequest request)
    {
        if (string.Equals(request.Observer.CurrentScreen, "character-select", StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.Observer.VisibleScreen, "character-select", StringComparison.OrdinalIgnoreCase))
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.955,
                0.723,
                "character confirm",
                "The run start confirm checkmark is visible on the character-select screen. Click it to embark.",
                0.94,
                "character-select",
                1000,
                true,
                null);
        }

        if (GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(request.Observer, out var postEmbarkPhase))
        {
            return postEmbarkPhase switch
            {
                GuiSmokePhase.HandleRewards => DecideHandleRewards(request with { Phase = GuiSmokePhase.HandleRewards.ToString() }),
                GuiSmokePhase.HandleEvent => DecideHandleEvent(request with { Phase = GuiSmokePhase.HandleEvent.ToString() }),
                GuiSmokePhase.HandleShop => DecideHandleShop(request with { Phase = GuiSmokePhase.HandleShop.ToString() }),
                GuiSmokePhase.HandleCombat => DecideHandleCombat(request with { Phase = GuiSmokePhase.HandleCombat.ToString() }),
                GuiSmokePhase.ChooseFirstNode => DecideChooseFirstNode(request with { Phase = GuiSmokePhase.ChooseFirstNode.ToString() }),
                _ => CreateWaitDecision("waiting for post-embark room state", request.Observer.CurrentScreen),
            };
        }

        return TryFindActionNodeDecision(request, "Embark", "embark")
               ?? CreateWaitDecision("waiting for embark action", request.Observer.CurrentScreen);
    }
}
