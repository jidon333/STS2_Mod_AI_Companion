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

    public static string[] BuildCardSelectionAllowedActions(ObserverSummary observer, CardSelectionSubtypeState state)
    {
        var hasExplicitCardChoices = CardSelectionObserverSignals.GetCardChoices(observer, state)
            .Any(choice => choice.Enabled != false && !string.IsNullOrWhiteSpace(choice.ScreenBounds));
        var hasExplicitConfirmChoice = CardSelectionObserverSignals.TryGetConfirmChoice(observer, state) is { Enabled: not false, ScreenBounds.Length: > 0 };

        return state.ScreenType switch
        {
            "reward-pick" when hasExplicitCardChoices => new[] { "reward pick card", "wait" },
            "reward-pick" => new[] { "wait" },
            "simple-select" when CardSelectionObserverSignals.IsConfirmReady(state) && hasExplicitConfirmChoice
                => new[] { "simple select confirm", "wait" },
            "simple-select" when hasExplicitCardChoices => new[] { "simple select choice", "wait" },
            "simple-select" => new[] { "wait" },
            "bundle-select" when CardSelectionObserverSignals.IsConfirmReady(state) && hasExplicitConfirmChoice
                => new[] { "bundle select confirm", "wait" },
            "bundle-select" when hasExplicitCardChoices => new[] { "bundle select choice", "wait" },
            "bundle-select" => new[] { "wait" },
            "relic-select" when hasExplicitCardChoices => new[] { "relic select choice", "wait" },
            "relic-select" => new[] { "wait" },
            "transform" when CardSelectionObserverSignals.IsConfirmReady(state) && hasExplicitConfirmChoice
                => new[] { "transform confirm", "wait" },
            "transform" when hasExplicitCardChoices => new[] { "transform select card", "wait" },
            "transform" => new[] { "wait" },
            "deck-remove" when CardSelectionObserverSignals.IsConfirmReady(state) && hasExplicitConfirmChoice
                => new[] { "deck remove confirm", "wait" },
            "deck-remove" when hasExplicitCardChoices => new[] { "deck remove select card", "wait" },
            "deck-remove" => new[] { "wait" },
            "upgrade" when CardSelectionObserverSignals.IsConfirmReady(state) && hasExplicitConfirmChoice
                => new[] { "upgrade confirm", "wait" },
            "upgrade" when hasExplicitCardChoices => new[] { "upgrade select card", "wait" },
            "upgrade" => new[] { "wait" },
            _ => new[] { "wait" },
        };
    }

    public static bool ShouldSuppressRoomSubstateHeuristics(GuiSmokePhase phase, ObserverState observer)
    {
        return phase == GuiSmokePhase.HandleCombat
               || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary);
    }
}
