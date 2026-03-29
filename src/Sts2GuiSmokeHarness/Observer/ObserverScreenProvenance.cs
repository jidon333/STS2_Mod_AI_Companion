using System;

static class ObserverScreenProvenance
{
    public static string? StrictDirectCurrentScreen(ObserverSummary observer)
        => observer.RawCurrentScreen;

    public static string? StrictDirectCurrentScreen(ObserverState observer)
        => observer.RawCurrentScreen;

    public static string? StrictDirectObservedScreen(ObserverSummary observer)
        => observer.RawObservedScreen;

    public static string? StrictDirectObservedScreen(ObserverState observer)
        => observer.RawObservedScreen;

    public static string? StrictPublishedCurrentScreen(ObserverSummary observer)
        => observer.PublishedCurrentScreen;

    public static string? StrictPublishedCurrentScreen(ObserverState observer)
        => observer.PublishedCurrentScreen;

    public static string? StrictPublishedVisibleScreen(ObserverSummary observer)
        => observer.PublishedVisibleScreen;

    public static string? StrictPublishedVisibleScreen(ObserverState observer)
        => observer.PublishedVisibleScreen;

    public static bool? StrictPublishedSceneReady(ObserverSummary observer)
        => observer.PublishedSceneReady;

    public static bool? StrictPublishedSceneReady(ObserverState observer)
        => observer.PublishedSceneReady;

    public static string? StrictPublishedSceneAuthority(ObserverSummary observer)
        => observer.PublishedSceneAuthority;

    public static string? StrictPublishedSceneAuthority(ObserverState observer)
        => observer.PublishedSceneAuthority;

    public static string? StrictPublishedSceneStability(ObserverSummary observer)
        => observer.PublishedSceneStability;

    public static string? StrictPublishedSceneStability(ObserverState observer)
        => observer.PublishedSceneStability;

    public static string? StrictCompatibilityCurrentScreen(ObserverSummary observer)
        => observer.CompatibilityCurrentScreen;

    public static string? StrictCompatibilityCurrentScreen(ObserverState observer)
        => observer.CompatibilityCurrentScreen;

    public static string? StrictCompatibilityVisibleScreen(ObserverSummary observer)
        => observer.CompatibilityVisibleScreen;

    public static string? StrictCompatibilityVisibleScreen(ObserverState observer)
        => observer.CompatibilityVisibleScreen;

    public static bool? StrictCompatibilitySceneReady(ObserverSummary observer)
        => observer.CompatibilitySceneReady;

    public static bool? StrictCompatibilitySceneReady(ObserverState observer)
        => observer.CompatibilitySceneReady;

    public static string? StrictCompatibilitySceneAuthority(ObserverSummary observer)
        => observer.CompatibilitySceneAuthority;

    public static string? StrictCompatibilitySceneAuthority(ObserverState observer)
        => observer.CompatibilitySceneAuthority;

    public static string? StrictCompatibilitySceneStability(ObserverSummary observer)
        => observer.CompatibilitySceneStability;

    public static string? StrictCompatibilitySceneStability(ObserverState observer)
        => observer.CompatibilitySceneStability;

    public static string? DirectCurrentScreen(ObserverSummary observer)
        => StrictDirectCurrentScreen(observer);

    public static string? DirectCurrentScreen(ObserverState observer)
        => StrictDirectCurrentScreen(observer);

    public static string? DirectObservedScreen(ObserverSummary observer)
        => StrictDirectObservedScreen(observer) ?? StrictDirectCurrentScreen(observer);

    public static string? DirectObservedScreen(ObserverState observer)
        => StrictDirectObservedScreen(observer) ?? StrictDirectCurrentScreen(observer);

    public static string? PublishedCurrentScreen(ObserverSummary observer)
        => StrictPublishedCurrentScreen(observer);

    public static string? PublishedCurrentScreen(ObserverState observer)
        => StrictPublishedCurrentScreen(observer);

    public static string? PublishedVisibleScreen(ObserverSummary observer)
        => StrictPublishedVisibleScreen(observer);

    public static string? PublishedVisibleScreen(ObserverState observer)
        => StrictPublishedVisibleScreen(observer);

    public static bool? PublishedSceneReady(ObserverSummary observer)
        => StrictPublishedSceneReady(observer);

    public static bool? PublishedSceneReady(ObserverState observer)
        => StrictPublishedSceneReady(observer);

    public static string? PublishedSceneAuthority(ObserverSummary observer)
        => StrictPublishedSceneAuthority(observer);

    public static string? PublishedSceneAuthority(ObserverState observer)
        => StrictPublishedSceneAuthority(observer);

    public static string? PublishedSceneStability(ObserverSummary observer)
        => StrictPublishedSceneStability(observer);

    public static string? PublishedSceneStability(ObserverState observer)
        => StrictPublishedSceneStability(observer);

    public static string? ControlFlowCurrentScreen(ObserverSummary observer)
        => StrictPublishedCurrentScreen(observer)
           ?? StrictDirectCurrentScreen(observer)
           ?? StrictCompatibilityCurrentScreen(observer)
           ?? observer.CurrentScreen;

    public static string? ControlFlowCurrentScreen(ObserverState observer)
        => StrictPublishedCurrentScreen(observer)
           ?? StrictDirectCurrentScreen(observer)
           ?? StrictCompatibilityCurrentScreen(observer)
           ?? observer.CurrentScreen;

    public static string? ControlFlowVisibleScreen(ObserverSummary observer)
        => StrictPublishedVisibleScreen(observer)
           ?? StrictDirectObservedScreen(observer)
           ?? StrictDirectCurrentScreen(observer)
           ?? StrictCompatibilityVisibleScreen(observer)
           ?? observer.VisibleScreen
           ?? observer.CurrentScreen;

    public static string? ControlFlowVisibleScreen(ObserverState observer)
        => StrictPublishedVisibleScreen(observer)
           ?? StrictDirectObservedScreen(observer)
           ?? StrictDirectCurrentScreen(observer)
           ?? StrictCompatibilityVisibleScreen(observer)
           ?? observer.VisibleScreen
           ?? observer.CurrentScreen;

    public static bool? ControlFlowSceneReady(ObserverSummary observer)
        => StrictPublishedSceneReady(observer)
           ?? StrictCompatibilitySceneReady(observer)
           ?? observer.SceneReady;

    public static bool? ControlFlowSceneReady(ObserverState observer)
        => StrictPublishedSceneReady(observer)
           ?? StrictCompatibilitySceneReady(observer)
           ?? observer.SceneReady;

    public static string? ControlFlowSceneAuthority(ObserverSummary observer)
        => StrictPublishedSceneAuthority(observer)
           ?? StrictCompatibilitySceneAuthority(observer)
           ?? observer.SceneAuthority;

    public static string? ControlFlowSceneAuthority(ObserverState observer)
        => StrictPublishedSceneAuthority(observer)
           ?? StrictCompatibilitySceneAuthority(observer)
           ?? observer.SceneAuthority;

    public static string? ControlFlowSceneStability(ObserverSummary observer)
        => StrictPublishedSceneStability(observer)
           ?? StrictCompatibilitySceneStability(observer)
           ?? observer.SceneStability;

    public static string? ControlFlowSceneStability(ObserverState observer)
        => StrictPublishedSceneStability(observer)
           ?? StrictCompatibilitySceneStability(observer)
           ?? observer.SceneStability;

    public static bool MatchesControlFlowScreen(ObserverSummary observer, string screen)
    {
        return string.Equals(ControlFlowCurrentScreen(observer), screen, StringComparison.OrdinalIgnoreCase)
               || string.Equals(ControlFlowVisibleScreen(observer), screen, StringComparison.OrdinalIgnoreCase)
               || string.Equals(DirectCurrentScreen(observer), screen, StringComparison.OrdinalIgnoreCase)
               || string.Equals(DirectObservedScreen(observer), screen, StringComparison.OrdinalIgnoreCase);
    }

    public static bool MatchesControlFlowScreen(ObserverState observer, string screen)
    {
        return string.Equals(ControlFlowCurrentScreen(observer), screen, StringComparison.OrdinalIgnoreCase)
               || string.Equals(ControlFlowVisibleScreen(observer), screen, StringComparison.OrdinalIgnoreCase)
               || string.Equals(DirectCurrentScreen(observer), screen, StringComparison.OrdinalIgnoreCase)
               || string.Equals(DirectObservedScreen(observer), screen, StringComparison.OrdinalIgnoreCase);
    }

    public static bool MatchesDirectScreen(ObserverSummary observer, string screen)
    {
        return string.Equals(DirectCurrentScreen(observer), screen, StringComparison.OrdinalIgnoreCase)
               || string.Equals(DirectObservedScreen(observer), screen, StringComparison.OrdinalIgnoreCase);
    }

    public static bool MatchesDirectScreen(ObserverState observer, string screen)
    {
        return string.Equals(DirectCurrentScreen(observer), screen, StringComparison.OrdinalIgnoreCase)
               || string.Equals(DirectObservedScreen(observer), screen, StringComparison.OrdinalIgnoreCase);
    }

    public static bool MatchesPublishedScreen(ObserverSummary observer, string screen)
    {
        return string.Equals(PublishedCurrentScreen(observer), screen, StringComparison.OrdinalIgnoreCase)
               || string.Equals(PublishedVisibleScreen(observer), screen, StringComparison.OrdinalIgnoreCase);
    }

    public static bool MatchesPublishedScreen(ObserverState observer, string screen)
    {
        return string.Equals(PublishedCurrentScreen(observer), screen, StringComparison.OrdinalIgnoreCase)
               || string.Equals(PublishedVisibleScreen(observer), screen, StringComparison.OrdinalIgnoreCase);
    }

    public static string? DisplayScreen(ObserverSummary observer)
        => DisplayControlFlowScreen(observer);

    public static string? DisplayScreen(ObserverState observer)
        => DisplayControlFlowScreen(observer);

    public static string? DisplayControlFlowScreen(ObserverSummary observer)
        => ControlFlowCurrentScreen(observer) ?? ControlFlowVisibleScreen(observer);

    public static string? DisplayControlFlowScreen(ObserverState observer)
        => ControlFlowCurrentScreen(observer) ?? ControlFlowVisibleScreen(observer);
}
