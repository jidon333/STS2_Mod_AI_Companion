using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

static class WaitRunLoadRecoverySignals
{
    public static bool ShouldRetryEnterRunFromWaitRunLoad(ObserverSummary observer)
    {
        if (CompatibilitySceneReady(observer) == false
            || !string.Equals(CompatibilitySceneStability(observer), "stable", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var transitionState = RootSceneTransitionObserverSignals.TryGetState(observer);
        if (transitionState?.TransitionInProgress == true
            || transitionState?.RootSceneIsRun == true
            || transitionState?.CurrentRunNodePresent == true)
        {
            return false;
        }

        var mainMenuSurfaceVisible = MatchesCompatibilityScreen(observer, "main-menu")
                                     || string.Equals(observer.ChoiceExtractorPath, "main-menu", StringComparison.OrdinalIgnoreCase)
                                     || transitionState?.RootSceneIsMainMenu == true;
        if (!mainMenuSurfaceVisible)
        {
            return false;
        }

        return observer.ActionNodes.Any(IsActionableContinueRunNode)
               || observer.Choices.Any(IsContinueRunChoice)
               || observer.CurrentChoices.Any(IsContinueRunLabel);
    }

    private static bool IsActionableContinueRunNode(ObserverActionNode node)
    {
        return node.Actionable
               && (string.Equals(node.Kind, "continue-run", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(node.NodeId, "main-menu:continue", StringComparison.OrdinalIgnoreCase)
                   || node.SemanticHints.Any(static hint => hint.Contains("continue-run", StringComparison.OrdinalIgnoreCase))
                   || IsContinueRunLabel(node.Label));
    }

    private static bool IsContinueRunChoice(ObserverChoice choice)
    {
        return string.Equals(choice.Kind, "continue-run", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.BindingKind, "continue-run", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.Value, "main-menu:continue", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.NodeId, "main-menu:continue", StringComparison.OrdinalIgnoreCase)
               || (choice.Enabled ?? true) && IsContinueRunLabel(choice.Label);
    }

    private static bool IsContinueRunLabel(string? label)
    {
        return string.Equals(label, "Continue", StringComparison.OrdinalIgnoreCase)
               || string.Equals(label, "\uACC4\uC18D", StringComparison.OrdinalIgnoreCase);
    }
}
