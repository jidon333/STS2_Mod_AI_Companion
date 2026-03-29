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
        => observer.CompatibilityCurrentScreen ?? observer.CompatibilityLogicalScreen;

    public static string? StrictCompatibilityCurrentScreen(ObserverState observer)
        => observer.CompatibilityCurrentScreen ?? observer.CompatibilityLogicalScreen;

    public static string? StrictCompatibilityVisibleScreen(ObserverSummary observer)
        => observer.CompatibilityVisibleScreen ?? observer.CompatibilityVisibleObservedScreen;

    public static string? StrictCompatibilityVisibleScreen(ObserverState observer)
        => observer.CompatibilityVisibleScreen ?? observer.CompatibilityVisibleObservedScreen;

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
        => StrictDirectCurrentScreen(observer) ?? observer.CurrentScreen;

    public static string? DirectCurrentScreen(ObserverState observer)
        => StrictDirectCurrentScreen(observer) ?? observer.CurrentScreen;

    public static string? DirectObservedScreen(ObserverSummary observer)
        => StrictDirectObservedScreen(observer) ?? StrictDirectCurrentScreen(observer) ?? observer.CurrentScreen;

    public static string? DirectObservedScreen(ObserverState observer)
        => StrictDirectObservedScreen(observer) ?? StrictDirectCurrentScreen(observer) ?? observer.CurrentScreen;

    public static string? PublishedCurrentScreen(ObserverSummary observer)
        => StrictPublishedCurrentScreen(observer) ?? observer.CurrentScreen;

    public static string? PublishedCurrentScreen(ObserverState observer)
        => StrictPublishedCurrentScreen(observer) ?? observer.CurrentScreen;

    public static string? PublishedVisibleScreen(ObserverSummary observer)
        => StrictPublishedVisibleScreen(observer) ?? observer.VisibleScreen;

    public static string? PublishedVisibleScreen(ObserverState observer)
        => StrictPublishedVisibleScreen(observer) ?? observer.VisibleScreen;

    public static bool? PublishedSceneReady(ObserverSummary observer)
        => StrictPublishedSceneReady(observer) ?? observer.SceneReady;

    public static bool? PublishedSceneReady(ObserverState observer)
        => StrictPublishedSceneReady(observer) ?? observer.SceneReady;

    public static string? PublishedSceneAuthority(ObserverSummary observer)
        => StrictPublishedSceneAuthority(observer) ?? observer.SceneAuthority;

    public static string? PublishedSceneAuthority(ObserverState observer)
        => StrictPublishedSceneAuthority(observer) ?? observer.SceneAuthority;

    public static string? PublishedSceneStability(ObserverSummary observer)
        => StrictPublishedSceneStability(observer) ?? observer.SceneStability;

    public static string? PublishedSceneStability(ObserverState observer)
        => StrictPublishedSceneStability(observer) ?? observer.SceneStability;

    public static string? CompatibilityCurrentScreen(ObserverSummary observer)
        => StrictCompatibilityCurrentScreen(observer) ?? observer.CurrentScreen;

    public static string? CompatibilityCurrentScreen(ObserverState observer)
        => StrictCompatibilityCurrentScreen(observer) ?? observer.CurrentScreen;

    public static string? CompatibilityVisibleScreen(ObserverSummary observer)
        => StrictCompatibilityVisibleScreen(observer) ?? observer.VisibleScreen;

    public static string? CompatibilityVisibleScreen(ObserverState observer)
        => StrictCompatibilityVisibleScreen(observer) ?? observer.VisibleScreen;

    public static bool? CompatibilitySceneReady(ObserverSummary observer)
        => StrictCompatibilitySceneReady(observer) ?? observer.SceneReady;

    public static bool? CompatibilitySceneReady(ObserverState observer)
        => StrictCompatibilitySceneReady(observer) ?? observer.SceneReady;

    public static string? CompatibilitySceneAuthority(ObserverSummary observer)
        => StrictCompatibilitySceneAuthority(observer) ?? observer.SceneAuthority;

    public static string? CompatibilitySceneAuthority(ObserverState observer)
        => StrictCompatibilitySceneAuthority(observer) ?? observer.SceneAuthority;

    public static string? CompatibilitySceneStability(ObserverSummary observer)
        => StrictCompatibilitySceneStability(observer) ?? observer.SceneStability;

    public static string? CompatibilitySceneStability(ObserverState observer)
        => StrictCompatibilitySceneStability(observer) ?? observer.SceneStability;

    public static bool MatchesCompatibilityScreen(ObserverSummary observer, string screen)
    {
        return string.Equals(CompatibilityCurrentScreen(observer), screen, StringComparison.OrdinalIgnoreCase)
               || string.Equals(CompatibilityVisibleScreen(observer), screen, StringComparison.OrdinalIgnoreCase);
    }

    public static bool MatchesCompatibilityScreen(ObserverState observer, string screen)
    {
        return string.Equals(CompatibilityCurrentScreen(observer), screen, StringComparison.OrdinalIgnoreCase)
               || string.Equals(CompatibilityVisibleScreen(observer), screen, StringComparison.OrdinalIgnoreCase);
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
        => observer.CurrentScreen ?? observer.VisibleScreen;

    public static string? DisplayScreen(ObserverState observer)
        => observer.CurrentScreen ?? observer.VisibleScreen;

    public static string? DisplayPublishedScreen(ObserverSummary observer)
        => PublishedCurrentScreen(observer) ?? PublishedVisibleScreen(observer);

    public static string? DisplayPublishedScreen(ObserverState observer)
        => PublishedCurrentScreen(observer) ?? PublishedVisibleScreen(observer);
}
