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
        var transitionState = RootSceneTransitionObserverSignals.TryGetState(observer);
        if (transitionState?.TransitionInProgress == true
            || transitionState?.RootSceneIsRun == true
            || transitionState?.CurrentRunNodePresent == true)
        {
            return false;
        }

        var mainMenuSurfaceVisible = MatchesControlFlowScreen(observer, "main-menu")
                                     || string.Equals(observer.ChoiceExtractorPath, "main-menu", StringComparison.OrdinalIgnoreCase)
                                     || transitionState?.RootSceneIsMainMenu == true;
        if (!mainMenuSurfaceVisible)
        {
            return false;
        }

        if (HasExplicitRunSaveRecoverySurface(observer))
        {
            return true;
        }

        if (HasExplicitMainMenuRetrySurface(observer))
        {
            return true;
        }

        if (ControlFlowSceneReady(observer) == false
            || !string.Equals(ControlFlowSceneStability(observer), "stable", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return MainMenuRunStartObserverSignals.HasMainMenuRunStartSurface(observer)
               || observer.CurrentChoices.Any(IsContinueRunLabel);
    }

    private static bool HasExplicitRunSaveRecoverySurface(ObserverSummary observer)
    {
        return MainMenuRunStartObserverSignals.HasRunSaveCleanupSurface(observer)
               || MainMenuRunStartObserverSignals.HasAbandonRunConfirmSurface(observer);
    }

    private static bool HasExplicitMainMenuRetrySurface(ObserverSummary observer)
    {
        var choiceExtractorPath = observer.ChoiceExtractorPath
                                  ?? (observer.Meta.TryGetValue("choiceExtractorPath", out var publishedChoiceExtractorPath)
                                      ? publishedChoiceExtractorPath
                                      : null)
                                  ?? (observer.Meta.TryGetValue("rawChoiceExtractorPath", out var rawChoiceExtractorPath)
                                      ? rawChoiceExtractorPath
                                      : null);

        return string.Equals(choiceExtractorPath, "main-menu", StringComparison.OrdinalIgnoreCase)
               && IsMainMenuScreenTypeName(observer.Meta.TryGetValue("activeScreenType", out var activeScreenType) ? activeScreenType : null)
               && !MainMenuRunStartObserverSignals.IsLogoAnimationOnlyMainMenu(observer)
               && !MainMenuRunStartObserverSignals.ShouldWaitForStableRunStartSurface(observer)
               && MainMenuRunStartObserverSignals.HasMainMenuRunStartSurface(observer);
    }

    private static bool IsMainMenuScreenTypeName(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NMainMenu", StringComparison.OrdinalIgnoreCase);
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
