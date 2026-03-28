using static GuiSmokeChoicePrimitiveSupport;

static class GuiSmokeNonCombatAllowedActionSupport
{
    public static string[] BuildMapOverlayRoutingAllowedActions(MapOverlayState mapOverlayState)
    {
        return mapOverlayState.MapBackNavigationAvailable
            ? new[] { "click exported reachable node", "click first reachable node", "click map back", "wait" }
            : new[] { "click exported reachable node", "click first reachable node", "wait" };
    }

    public static string[] BuildMapForegroundRoutingAllowedActions()
    {
        return new[] { "click exported reachable node", "click visible map advance", "wait" };
    }

    public static bool LooksLikeInspectOverlayState(ObserverState observer)
    {
        return observer.CurrentChoices.Any(static label =>
                   label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase))
               || observer.ActionNodes.Any(static node =>
                   node.Label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                   || node.Label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                   || node.Label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
    }

    public static string[] BuildCardSelectionAllowedActions(CardSelectionSubtypeState state)
    {
        return state.ScreenType switch
        {
            "reward-pick" => new[] { "reward pick card", "wait" },
            "transform" when CardSelectionObserverSignals.IsConfirmReady(state)
                => new[] { "transform confirm", "wait" },
            "transform" => new[] { "transform select card", "wait" },
            "deck-remove" when CardSelectionObserverSignals.IsConfirmReady(state)
                => new[] { "deck remove confirm", "wait" },
            "deck-remove" => new[] { "deck remove select card", "wait" },
            "upgrade" when CardSelectionObserverSignals.IsConfirmReady(state)
                => new[] { "upgrade confirm", "wait" },
            "upgrade" => new[] { "upgrade select card", "wait" },
            _ => new[] { "wait" },
        };
    }

    public static bool ShouldSuppressRoomSubstateHeuristics(GuiSmokePhase phase, ObserverState observer)
    {
        return phase == GuiSmokePhase.HandleCombat
               || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary);
    }
}
