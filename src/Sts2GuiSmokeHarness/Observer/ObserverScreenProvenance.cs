using System;

static class ObserverScreenProvenance
{
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

    public static string? DisplayScreen(ObserverSummary observer)
        => CompatibilityCurrentScreen(observer) ?? CompatibilityVisibleScreen(observer);

    public static string? DisplayScreen(ObserverState observer)
        => CompatibilityCurrentScreen(observer) ?? CompatibilityVisibleScreen(observer);
}
