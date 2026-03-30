using static ObserverScreenProvenance;

sealed partial class AutoDecisionProvider
{
    private static GuiSmokeStepDecision DecideEnterRun(GuiSmokeStepRequest request)
    {
        if (MainMenuRunStartObserverSignals.HasAbandonRunConfirmSurface(request.Observer))
        {
            return TryFindLabeledSurfaceDecision(
                       request,
                       MainMenuRunStartObserverSignals.IsPopupConfirmLabel,
                       "confirm abandon run",
                       "The abandon-run confirmation popup is open on the main menu. Confirm it so the stale run save is cleared before starting a fresh run.",
                       0.95,
                       1200)
                   ?? CreateWaitDecision("waiting for abandon-run confirmation button", ControlFlowCurrentScreen(request.Observer));
        }

        if (MainMenuRunStartObserverSignals.HasRunSaveCleanupSurface(request.Observer))
        {
            return TryFindLabeledSurfaceDecision(
                       request,
                       MainMenuRunStartObserverSignals.IsAbandonRunLabel,
                       "abandon run",
                       "The main menu is still showing a persisted run-save surface. Clear the stale run with Abandon Run before reopening the fresh new-run path.",
                       0.95,
                       1200)
                   ?? CreateWaitDecision("waiting for abandon-run action", ControlFlowCurrentScreen(request.Observer));
        }

        if (MatchesControlFlowScreen(request.Observer, "singleplayer-submenu"))
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

        if (MainMenuRunStartObserverSignals.ShouldWaitForStableRunStartSurface(request.Observer))
        {
            return CreateWaitDecision("waiting for a stable main-menu run-start surface", ControlFlowCurrentScreen(request.Observer));
        }

        return TryFindActionNodeDecision(request, "Continue", "continue")
               ?? TryFindActionNodeDecision(request, "계속", "continue")
               ?? TryFindActionNodeDecision(request, "Singleplayer", "singleplayer")
               ?? TryFindActionNodeDecision(request, "싱글", "singleplayer")
               ?? CreateWaitDecision("main menu actions not yet visible", ControlFlowCurrentScreen(request.Observer));
    }

    private static GuiSmokeStepDecision? TryFindLabeledSurfaceDecision(
        GuiSmokeStepRequest request,
        Func<string?, bool> labelPredicate,
        string targetLabel,
        string reason,
        double confidence,
        int waitMs)
    {
        var node = request.Observer.ActionNodes.FirstOrDefault(candidate =>
            candidate.Actionable && labelPredicate(candidate.Label));
        if (node is not null)
        {
            return CreateClickDecisionFromNode(request, node, targetLabel);
        }

        var choice = request.Observer.Choices.FirstOrDefault(candidate =>
            (candidate.Enabled ?? true) && labelPredicate(candidate.Label));
        return choice is null
            ? null
            : CreateClickDecisionFromChoice(request, choice, targetLabel, reason, confidence, "main-menu", waitMs);
    }

    private static GuiSmokeStepDecision DecideChooseCharacter(GuiSmokeStepRequest request)
    {
        if (MatchesControlFlowScreen(request.Observer, "character-select"))
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
               ?? CreateWaitDecision("waiting for ironclad node", ControlFlowCurrentScreen(request.Observer));
    }

    private static GuiSmokeStepDecision DecideWaitRunLoad(GuiSmokeStepRequest request, GuiSmokeStepAnalysisContext? analysisContext = null)
    {
        if (GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(request.Observer, out var postRunLoadPhase))
        {
            return postRunLoadPhase switch
            {
                GuiSmokePhase.HandleRewards => DecideHandleRewards(request with { Phase = GuiSmokePhase.HandleRewards.ToString() }, analysisContext),
                GuiSmokePhase.HandleEvent => DecideHandleEvent(request with { Phase = GuiSmokePhase.HandleEvent.ToString() }, analysisContext),
                GuiSmokePhase.HandleShop => DecideHandleShop(request with { Phase = GuiSmokePhase.HandleShop.ToString() }),
                GuiSmokePhase.HandleCombat => DecideHandleCombat(request with { Phase = GuiSmokePhase.HandleCombat.ToString() }),
                GuiSmokePhase.ChooseFirstNode => DecideChooseFirstNode(request with { Phase = GuiSmokePhase.ChooseFirstNode.ToString() }, analysisContext),
                GuiSmokePhase.ChooseCharacter => DecideChooseCharacter(request with { Phase = GuiSmokePhase.ChooseCharacter.ToString() }),
                _ => CreateWaitDecision("waiting for post-run-load room state", ControlFlowCurrentScreen(request.Observer)),
            };
        }

        if (WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(request.Observer))
        {
            return DecideEnterRun(request with { Phase = GuiSmokePhase.EnterRun.ToString() });
        }

        return CreateWaitDecision("waiting for root-scene transition and run load readiness", ControlFlowCurrentScreen(request.Observer));
    }

    private static GuiSmokeStepDecision DecideEmbark(GuiSmokeStepRequest request, GuiSmokeStepAnalysisContext? analysisContext = null)
    {
        if (MatchesControlFlowScreen(request.Observer, "character-select"))
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
                GuiSmokePhase.HandleRewards => DecideHandleRewards(request with { Phase = GuiSmokePhase.HandleRewards.ToString() }, analysisContext),
                GuiSmokePhase.HandleEvent => DecideHandleEvent(request with { Phase = GuiSmokePhase.HandleEvent.ToString() }, analysisContext),
                GuiSmokePhase.HandleShop => DecideHandleShop(request with { Phase = GuiSmokePhase.HandleShop.ToString() }),
                GuiSmokePhase.HandleCombat => DecideHandleCombat(request with { Phase = GuiSmokePhase.HandleCombat.ToString() }),
                GuiSmokePhase.ChooseFirstNode => DecideChooseFirstNode(request with { Phase = GuiSmokePhase.ChooseFirstNode.ToString() }, analysisContext),
                _ => CreateWaitDecision("waiting for post-embark room state", ControlFlowCurrentScreen(request.Observer)),
            };
        }

        return TryFindActionNodeDecision(request, "Embark", "embark")
               ?? CreateWaitDecision("waiting for embark action", ControlFlowCurrentScreen(request.Observer));
    }
}
