using System;
using System.Linq;
using static ObserverScreenProvenance;

static class MainMenuRunStartObserverSignals
{
    public static bool IsRunStartSurfaceReady(ObserverState observer)
        => IsRunStartSurfaceReady(observer.Summary);

    public static bool IsRunStartSurfaceReady(ObserverSummary observer)
    {
        if (HasPublishedBootstrapVisibility(observer))
        {
            return false;
        }

        if (HasCollapsedMainMenuActionLayout(observer))
        {
            return false;
        }

        if (HasRunStartActionSurface(observer))
        {
            return true;
        }

        return MatchesControlFlowScreen(observer, "singleplayer-submenu");
    }

    public static bool IsLogoAnimationOnlyMainMenu(ObserverState observer)
        => IsLogoAnimationOnlyMainMenu(observer.Summary);

    public static bool HasMainMenuRunStartSurface(ObserverState observer)
        => HasMainMenuRunStartSurface(observer.Summary);

    public static bool HasMainMenuRunStartSurface(ObserverSummary observer)
        => HasRunStartActionSurface(observer);

    public static bool ShouldWaitForStableRunStartSurface(ObserverState observer)
        => ShouldWaitForStableRunStartSurface(observer.Summary);

    public static bool ShouldWaitForStableRunStartSurface(ObserverSummary observer)
    {
        return !MatchesControlFlowScreen(observer, "singleplayer-submenu")
               && (HasCollapsedMainMenuActionLayout(observer)
                   || (HasContinueRunStartSurface(observer)
                       && HasSingleplayerRunStartSurface(observer)));
    }

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

    private static bool HasPublishedBootstrapVisibility(ObserverSummary observer)
    {
        var publishedVisibleScreen = observer.PublishedVisibleScreen
                                   ?? TryGetMetaValue(observer, "publishedVisibleScreen");

        return string.Equals(publishedVisibleScreen, "bootstrap", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasRunStartActionSurface(ObserverSummary observer)
    {
        return observer.ActionNodes.Any(IsBoundedRunStartActionNode)
               || observer.Choices.Any(IsBoundedRunStartChoice);
    }

    private static bool HasContinueRunStartSurface(ObserverSummary observer)
    {
        return observer.ActionNodes.Any(IsBoundedContinueRunActionNode)
               || observer.Choices.Any(IsBoundedContinueRunChoice);
    }

    private static bool HasSingleplayerRunStartSurface(ObserverSummary observer)
    {
        return observer.ActionNodes.Any(IsBoundedSingleplayerRunActionNode)
               || observer.Choices.Any(IsBoundedSingleplayerRunChoice);
    }

    private static bool IsContinueRunNode(ObserverActionNode node)
    {
        return IsContinueLabel(node.Label)
               || string.Equals(node.Kind, "continue-run", StringComparison.OrdinalIgnoreCase)
               || string.Equals(node.NodeId, "main-menu:continue", StringComparison.OrdinalIgnoreCase)
               || node.SemanticHints.Any(static hint =>
                   !string.IsNullOrWhiteSpace(hint)
                   && hint.Contains("continue-run", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSingleplayerRunNode(ObserverActionNode node)
    {
        return IsSingleplayerLabel(node.Label)
               || string.Equals(node.Kind, "singleplayer", StringComparison.OrdinalIgnoreCase)
               || string.Equals(node.NodeId, "main-menu:singleplayer", StringComparison.OrdinalIgnoreCase)
               || node.SemanticHints.Any(static hint =>
                   !string.IsNullOrWhiteSpace(hint)
                   && hint.Contains("singleplayer", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsContinueRunChoice(ObserverChoice choice)
    {
        if (choice.Enabled == false)
        {
            return false;
        }

        return IsContinueLabel(choice.Label)
               || string.Equals(choice.Kind, "continue-run", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.BindingKind, "continue-run", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.Value, "main-menu:continue", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.NodeId, "main-menu:continue", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint =>
                   !string.IsNullOrWhiteSpace(hint)
                   && hint.Contains("continue-run", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSingleplayerRunChoice(ObserverChoice choice)
    {
        if (choice.Enabled == false)
        {
            return false;
        }

        return IsSingleplayerLabel(choice.Label)
               || string.Equals(choice.Kind, "singleplayer", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.BindingKind, "singleplayer", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.Value, "main-menu:singleplayer", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.NodeId, "main-menu:singleplayer", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint =>
                   !string.IsNullOrWhiteSpace(hint)
                   && hint.Contains("singleplayer", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsRunStartActionNode(ObserverActionNode node)
    {
        return node.Actionable
               && (IsContinueRunNode(node) || IsSingleplayerRunNode(node));
    }

    private static bool IsRunStartChoice(ObserverChoice choice)
    {
        return IsContinueRunChoice(choice) || IsSingleplayerRunChoice(choice);
    }

    private static bool IsBoundedRunStartActionNode(ObserverActionNode node)
        => IsRunStartActionNode(node) && HasUsableBounds(node.ScreenBounds);

    private static bool IsBoundedContinueRunActionNode(ObserverActionNode node)
        => node.Actionable && IsContinueRunNode(node) && HasUsableBounds(node.ScreenBounds);

    private static bool IsBoundedSingleplayerRunActionNode(ObserverActionNode node)
        => node.Actionable && IsSingleplayerRunNode(node) && HasUsableBounds(node.ScreenBounds);

    private static bool IsBoundedRunStartChoice(ObserverChoice choice)
        => IsRunStartChoice(choice) && HasUsableBounds(choice.ScreenBounds);

    private static bool IsBoundedContinueRunChoice(ObserverChoice choice)
        => IsContinueRunChoice(choice) && HasUsableBounds(choice.ScreenBounds);

    private static bool IsBoundedSingleplayerRunChoice(ObserverChoice choice)
        => IsSingleplayerRunChoice(choice) && HasUsableBounds(choice.ScreenBounds);

    private static bool HasCollapsedMainMenuActionLayout(ObserverSummary observer)
    {
        var actionNodeSurfaces = observer.ActionNodes
            .Where(IsBoundedMainMenuActionNode)
            .Select(node => (node.Label, Bounds: NormalizeBounds(node.ScreenBounds)))
            .Where(static entry => !string.IsNullOrWhiteSpace(entry.Bounds))
            .ToArray();
        if (HasCollapsedMainMenuActionLayout(actionNodeSurfaces))
        {
            return true;
        }

        var choiceSurfaces = observer.Choices
            .Where(IsBoundedMainMenuActionChoice)
            .Select(choice => (choice.Label, Bounds: NormalizeBounds(choice.ScreenBounds)))
            .Where(static entry => !string.IsNullOrWhiteSpace(entry.Bounds))
            .ToArray();
        return HasCollapsedMainMenuActionLayout(choiceSurfaces);
    }

    private static bool HasCollapsedMainMenuActionLayout((string Label, string? Bounds)[] surfaces)
    {
        if (surfaces.Length < 2)
        {
            return false;
        }

        var distinctLabels = surfaces
            .Select(static entry => entry.Label?.Trim())
            .Where(static label => !string.IsNullOrWhiteSpace(label))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(2)
            .Count();
        if (distinctLabels < 2)
        {
            return false;
        }

        var distinctBounds = surfaces
            .Select(static entry => entry.Bounds)
            .Where(static bounds => !string.IsNullOrWhiteSpace(bounds))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(2)
            .Count();
        return distinctBounds <= 1;
    }

    private static bool IsBoundedMainMenuActionNode(ObserverActionNode node)
        => node.Actionable
           && HasUsableBounds(node.ScreenBounds)
           && IsMainMenuActionNode(node);

    private static bool IsBoundedMainMenuActionChoice(ObserverChoice choice)
        => HasUsableBounds(choice.ScreenBounds)
           && IsMainMenuActionChoice(choice);

    private static bool IsMainMenuActionNode(ObserverActionNode node)
    {
        return IsRunStartActionNode(node)
               || string.Equals(node.Kind, "menu-action", StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(node.NodeId)
                   && node.NodeId.StartsWith("main-menu:", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsMainMenuActionChoice(ObserverChoice choice)
    {
        return IsRunStartChoice(choice)
               || string.Equals(choice.Kind, "menu-action", StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(choice.NodeId)
                   && choice.NodeId.StartsWith("main-menu:", StringComparison.OrdinalIgnoreCase))
               || (!string.IsNullOrWhiteSpace(choice.Value)
                   && choice.Value.StartsWith("main-menu:", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasUsableBounds(string? bounds)
        => !string.IsNullOrWhiteSpace(bounds);

    private static string? NormalizeBounds(string? bounds)
        => string.IsNullOrWhiteSpace(bounds) ? null : bounds.Trim();

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
