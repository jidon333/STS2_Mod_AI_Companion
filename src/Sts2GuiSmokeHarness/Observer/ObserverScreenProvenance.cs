using System;

static class ObserverScreenProvenance
{
    public static string? DirectCurrentScreen(ObserverSummary observer)
        => observer.RawCurrentScreen ?? observer.CurrentScreen;

    public static string? DirectCurrentScreen(ObserverState observer)
        => observer.RawCurrentScreen ?? observer.CurrentScreen;

    public static string? DirectObservedScreen(ObserverSummary observer)
        => observer.RawObservedScreen ?? observer.RawCurrentScreen ?? observer.CurrentScreen;

    public static string? DirectObservedScreen(ObserverState observer)
        => observer.RawObservedScreen ?? observer.RawCurrentScreen ?? observer.CurrentScreen;

    public static string? PublishedCurrentScreen(ObserverSummary observer)
        => observer.PublishedCurrentScreen ?? observer.CurrentScreen;

    public static string? PublishedCurrentScreen(ObserverState observer)
        => observer.PublishedCurrentScreen ?? observer.CurrentScreen;

    public static string? PublishedVisibleScreen(ObserverSummary observer)
        => observer.PublishedVisibleScreen ?? observer.VisibleScreen;

    public static string? PublishedVisibleScreen(ObserverState observer)
        => observer.PublishedVisibleScreen ?? observer.VisibleScreen;

    public static bool? PublishedSceneReady(ObserverSummary observer)
        => observer.PublishedSceneReady ?? observer.SceneReady;

    public static bool? PublishedSceneReady(ObserverState observer)
        => observer.PublishedSceneReady ?? observer.SceneReady;

    public static string? PublishedSceneAuthority(ObserverSummary observer)
        => observer.PublishedSceneAuthority ?? observer.SceneAuthority;

    public static string? PublishedSceneAuthority(ObserverState observer)
        => observer.PublishedSceneAuthority ?? observer.SceneAuthority;

    public static string? PublishedSceneStability(ObserverSummary observer)
        => observer.PublishedSceneStability ?? observer.SceneStability;

    public static string? PublishedSceneStability(ObserverState observer)
        => observer.PublishedSceneStability ?? observer.SceneStability;

    public static string? CompatibilityCurrentScreen(ObserverSummary observer)
        => observer.CompatibilityCurrentScreen ?? observer.CurrentScreen;

    public static string? CompatibilityCurrentScreen(ObserverState observer)
        => observer.CompatibilityCurrentScreen ?? observer.CurrentScreen;

    public static string? CompatibilityVisibleScreen(ObserverSummary observer)
        => observer.CompatibilityVisibleScreen ?? observer.VisibleScreen;

    public static string? CompatibilityVisibleScreen(ObserverState observer)
        => observer.CompatibilityVisibleScreen ?? observer.VisibleScreen;

    public static bool? CompatibilitySceneReady(ObserverSummary observer)
        => observer.CompatibilitySceneReady ?? observer.SceneReady;

    public static bool? CompatibilitySceneReady(ObserverState observer)
        => observer.CompatibilitySceneReady ?? observer.SceneReady;

    public static string? CompatibilitySceneAuthority(ObserverSummary observer)
        => observer.CompatibilitySceneAuthority ?? observer.SceneAuthority;

    public static string? CompatibilitySceneAuthority(ObserverState observer)
        => observer.CompatibilitySceneAuthority ?? observer.SceneAuthority;

    public static string? CompatibilitySceneStability(ObserverSummary observer)
        => observer.CompatibilitySceneStability ?? observer.SceneStability;

    public static string? CompatibilitySceneStability(ObserverState observer)
        => observer.CompatibilitySceneStability ?? observer.SceneStability;

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
        => CompatibilityCurrentScreen(observer) ?? CompatibilityVisibleScreen(observer);

    public static string? DisplayScreen(ObserverState observer)
        => CompatibilityCurrentScreen(observer) ?? CompatibilityVisibleScreen(observer);

    public static string? DisplayPublishedScreen(ObserverSummary observer)
        => PublishedCurrentScreen(observer) ?? PublishedVisibleScreen(observer);

    public static string? DisplayPublishedScreen(ObserverState observer)
        => PublishedCurrentScreen(observer) ?? PublishedVisibleScreen(observer);
}
