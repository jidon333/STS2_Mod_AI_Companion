using System;
using System.Linq;
using static ObserverScreenProvenance;

static class MainMenuRunStartObserverSignals
{
    public static bool IsRunStartSurfaceReady(ObserverState observer)
        => IsRunStartSurfaceReady(observer.Summary);

    public static bool IsRunStartSurfaceReady(ObserverSummary observer)
    {
        if (HasRunStartActionSurface(observer))
        {
            return true;
        }

        return MatchesCompatibilityScreen(observer, "singleplayer-submenu");
    }

    public static bool IsLogoAnimationOnlyMainMenu(ObserverState observer)
        => IsLogoAnimationOnlyMainMenu(observer.Summary);

    public static bool HasMainMenuRunStartSurface(ObserverState observer)
        => HasMainMenuRunStartSurface(observer.Summary);

    public static bool HasMainMenuRunStartSurface(ObserverSummary observer)
        => HasRunStartActionSurface(observer);

    public static bool IsLogoAnimationOnlyMainMenu(ObserverSummary observer)
    {
        if (HasRunStartActionSurface(observer))
        {
            return false;
        }

        var activeScreenType = TryGetMetaValue(observer, "activeScreenType");
        var rootSceneCurrentType = TryGetMetaValue(observer, "rootSceneCurrentType");
        var choiceExtractorPath = observer.ChoiceExtractorPath
                                  ?? TryGetMetaValue(observer, "choiceExtractorPath")
                                  ?? TryGetMetaValue(observer, "rawChoiceExtractorPath");

        return IsLogoAnimationType(activeScreenType)
               || (IsLogoAnimationType(rootSceneCurrentType)
                   && string.Equals(choiceExtractorPath, "generic", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasRunStartActionSurface(ObserverSummary observer)
    {
        return observer.ActionNodes.Any(IsRunStartActionNode)
               || observer.Choices.Any(IsRunStartChoice)
               || observer.CurrentChoices.Any(IsRunStartLabel);
    }

    private static bool IsRunStartActionNode(ObserverActionNode node)
    {
        return node.Actionable
               && (IsContinueLabel(node.Label)
                   || IsSingleplayerLabel(node.Label)
                   || string.Equals(node.Kind, "continue-run", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(node.Kind, "singleplayer", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(node.NodeId, "main-menu:continue", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(node.NodeId, "main-menu:singleplayer", StringComparison.OrdinalIgnoreCase)
                   || node.SemanticHints.Any(static hint =>
                       !string.IsNullOrWhiteSpace(hint)
                       && (hint.Contains("continue-run", StringComparison.OrdinalIgnoreCase)
                           || hint.Contains("singleplayer", StringComparison.OrdinalIgnoreCase))));
    }

    private static bool IsRunStartChoice(ObserverChoice choice)
    {
        if (choice.Enabled == false)
        {
            return false;
        }

        return IsContinueLabel(choice.Label)
               || IsSingleplayerLabel(choice.Label)
               || string.Equals(choice.Kind, "continue-run", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.Kind, "singleplayer", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.BindingKind, "continue-run", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.BindingKind, "singleplayer", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.Value, "main-menu:continue", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.Value, "main-menu:singleplayer", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.NodeId, "main-menu:continue", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.NodeId, "main-menu:singleplayer", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint =>
                   !string.IsNullOrWhiteSpace(hint)
                   && (hint.Contains("continue-run", StringComparison.OrdinalIgnoreCase)
                       || hint.Contains("singleplayer", StringComparison.OrdinalIgnoreCase)));
    }

    private static bool IsRunStartLabel(string? label)
        => IsContinueLabel(label) || IsSingleplayerLabel(label);

    private static bool IsContinueLabel(string? label)
    {
        return string.Equals(label, "Continue", StringComparison.OrdinalIgnoreCase)
               || string.Equals(label, "\uACC4\uC18D", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSingleplayerLabel(string? label)
    {
        return string.Equals(label, "Singleplayer", StringComparison.OrdinalIgnoreCase)
               || string.Equals(label, "\uC2F1\uAE00", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLogoAnimationType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NLogoAnimation", StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryGetMetaValue(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value) ? value : null;
    }
}
