namespace Example.Mod;

internal static class InGameConfigUiExample
{
    public static void RefreshForSelection(object infoContainer, object? mod)
    {
        // Example flow:
        // 1. Check whether the selected mod is your mod.
        // 2. Create a small panel on first use.
        // 3. Load editable JSON config.
        // 4. Update the visible text.
        // 5. Save config on button press.
        // 6. Let runtime hot-reload pick the new values up.
    }
}
