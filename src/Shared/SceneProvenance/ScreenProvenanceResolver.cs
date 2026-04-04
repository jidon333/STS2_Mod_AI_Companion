using System.Text.Json;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.SceneProvenance;

internal static class ScreenProvenanceResolver
{
    public static ScreenProvenanceInput CreateFromLiveSnapshot(LiveExportSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return new ScreenProvenanceInput(
            snapshot.RawCurrentScreen ?? TryGetMeta(snapshot.Meta, "rawCurrentScreen"),
            snapshot.RawObservedScreen ?? TryGetMeta(snapshot.Meta, "rawObservedScreen") ?? TryGetMeta(snapshot.Meta, "screen"),
            snapshot.CurrentScreen,
            snapshot.PublishedCurrentScreen ?? TryGetMeta(snapshot.Meta, "publishedCurrentScreen"),
            snapshot.PublishedVisibleScreen ?? TryGetMeta(snapshot.Meta, "publishedVisibleScreen"),
            snapshot.CompatibilityCurrentScreen ?? TryGetMeta(snapshot.Meta, "compatibilityCurrentScreen"),
            snapshot.CompatibilityLogicalScreen ?? TryGetMeta(snapshot.Meta, "compatLogicalScreen") ?? TryGetMeta(snapshot.Meta, "logicalScreen"),
            snapshot.CompatibilityVisibleScreen ?? TryGetMeta(snapshot.Meta, "compatibilityVisibleScreen") ?? TryGetMeta(snapshot.Meta, "compatVisibleScreen"),
            snapshot.PublishedSceneReady ?? TryParseBool(TryGetMeta(snapshot.Meta, "publishedSceneReady")),
            snapshot.PublishedSceneAuthority ?? TryGetMeta(snapshot.Meta, "publishedSceneAuthority"),
            snapshot.PublishedSceneStability ?? TryGetMeta(snapshot.Meta, "publishedSceneStability"),
            snapshot.CompatibilitySceneReady ?? TryParseBool(TryGetMeta(snapshot.Meta, "compatSceneReady")),
            snapshot.CompatibilitySceneAuthority ?? TryGetMeta(snapshot.Meta, "compatSceneAuthority"),
            snapshot.CompatibilitySceneStability ?? TryGetMeta(snapshot.Meta, "compatSceneStability"),
            snapshot.Encounter?.InCombat);
    }

    public static ScreenProvenanceInput CreateFromObserverDocuments(JsonDocument? stateDocument, JsonDocument? inventoryDocument)
    {
        return new ScreenProvenanceInput(
            TryReadNestedString(stateDocument?.RootElement, "meta", "rawCurrentScreen"),
            TryReadNestedString(stateDocument?.RootElement, "meta", "rawObservedScreen")
            ?? TryReadNestedString(stateDocument?.RootElement, "meta", "screen"),
            TryReadString(stateDocument?.RootElement, "currentScreen"),
            TryReadString(stateDocument?.RootElement, "publishedCurrentScreen")
            ?? TryReadNestedString(stateDocument?.RootElement, "meta", "publishedCurrentScreen"),
            TryReadString(stateDocument?.RootElement, "publishedVisibleScreen")
            ?? TryReadNestedString(stateDocument?.RootElement, "meta", "publishedVisibleScreen"),
            TryReadNestedString(stateDocument?.RootElement, "meta", "compatibilityCurrentScreen"),
            TryReadNestedString(stateDocument?.RootElement, "meta", "compatLogicalScreen"),
            TryReadNestedString(stateDocument?.RootElement, "meta", "compatibilityVisibleScreen")
            ?? TryReadNestedString(stateDocument?.RootElement, "meta", "compatVisibleScreen"),
            TryReadBool(stateDocument?.RootElement, "publishedSceneReady")
            ?? TryReadNestedBool(stateDocument?.RootElement, "meta", "publishedSceneReady"),
            TryReadString(stateDocument?.RootElement, "publishedSceneAuthority")
            ?? TryReadNestedString(stateDocument?.RootElement, "meta", "publishedSceneAuthority"),
            TryReadString(stateDocument?.RootElement, "publishedSceneStability")
            ?? TryReadNestedString(stateDocument?.RootElement, "meta", "publishedSceneStability"),
            TryReadNestedBool(stateDocument?.RootElement, "meta", "compatSceneReady"),
            TryReadNestedString(stateDocument?.RootElement, "meta", "compatSceneAuthority"),
            TryReadNestedString(stateDocument?.RootElement, "meta", "compatSceneStability"),
            TryReadBool(stateDocument?.RootElement, "encounter", "inCombat"))
        {
            InventorySceneType = stateDocument is null
                ? TryReadString(inventoryDocument?.RootElement, "sceneType")
                : null,
            InventoryRawCurrentScreen = TryReadString(inventoryDocument?.RootElement, "rawCurrentScreen")
                                        ?? TryReadString(inventoryDocument?.RootElement, "rawSceneType"),
            InventoryPublishedCurrentScreen = TryReadString(inventoryDocument?.RootElement, "publishedCurrentScreen")
                                              ?? TryReadString(inventoryDocument?.RootElement, "publishedSceneType"),
            InventoryPublishedVisibleScreen = TryReadString(inventoryDocument?.RootElement, "publishedVisibleScreen")
                                              ?? TryReadString(inventoryDocument?.RootElement, "publishedVisibleScene"),
            InventoryCompatibilityCurrentScreen = TryReadString(inventoryDocument?.RootElement, "compatibilityCurrentScreen")
                                                  ?? TryReadString(inventoryDocument?.RootElement, "compatibilitySceneType"),
            InventoryCompatibilityLogicalScreen = TryReadString(inventoryDocument?.RootElement, "compatibilityLogicalScreen")
                                                  ?? TryReadString(inventoryDocument?.RootElement, "compatibilitySceneType"),
            InventoryCompatibilityVisibleScreen = TryReadString(inventoryDocument?.RootElement, "compatibilityVisibleScreen")
                                                  ?? TryReadString(inventoryDocument?.RootElement, "compatibilityVisibleScene"),
            InventoryPublishedSceneReady = TryReadBool(inventoryDocument?.RootElement, "publishedSceneReady"),
            InventoryPublishedSceneAuthority = TryReadString(inventoryDocument?.RootElement, "publishedSceneAuthority"),
            InventoryPublishedSceneStability = TryReadString(inventoryDocument?.RootElement, "publishedSceneStability"),
            InventoryCompatibilitySceneReady = TryReadBool(inventoryDocument?.RootElement, "compatibilitySceneReady"),
            InventoryCompatibilitySceneAuthority = TryReadString(inventoryDocument?.RootElement, "compatibilitySceneAuthority"),
            InventoryCompatibilitySceneStability = TryReadString(inventoryDocument?.RootElement, "compatibilitySceneStability"),
        };
    }

    public static ScreenProvenanceResult Resolve(ScreenProvenanceInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var rawCurrentScreen = FirstNonEmpty(
            input.RawCurrentScreen,
            input.InventoryRawCurrentScreen);
        var rawObservedScreen = FirstNonEmpty(
            input.RawObservedScreen,
            rawCurrentScreen,
            input.InventoryRawCurrentScreen);
        var publishedCurrentScreen = FirstNonEmpty(
            input.PublishedCurrentScreen,
            input.InventoryPublishedCurrentScreen);
        var publishedVisibleScreen = FirstNonEmpty(
            input.PublishedVisibleScreen,
            input.InventoryPublishedVisibleScreen);
        var compatibilityLogicalScreen = FirstNonEmpty(
            input.CompatibilityLogicalScreen,
            input.InventoryCompatibilityLogicalScreen,
            input.InventoryCompatibilityCurrentScreen);
        var compatibilityCurrentScreen = FirstNonEmpty(
            input.CompatibilityCurrentScreen,
            compatibilityLogicalScreen,
            input.InventoryCompatibilityCurrentScreen,
            input.InventoryCompatibilityLogicalScreen);
        var compatibilityVisibleScreen = FirstNonEmpty(
            input.CompatibilityVisibleScreen,
            input.InventoryCompatibilityVisibleScreen,
            compatibilityLogicalScreen,
            compatibilityCurrentScreen);
        var publishedSceneReady = input.PublishedSceneReady ?? input.InventoryPublishedSceneReady;
        var publishedSceneAuthority = FirstNonEmpty(input.PublishedSceneAuthority, input.InventoryPublishedSceneAuthority);
        var publishedSceneStability = FirstNonEmpty(input.PublishedSceneStability, input.InventoryPublishedSceneStability);
        var compatibilitySceneReady = input.CompatibilitySceneReady ?? input.InventoryCompatibilitySceneReady;
        var compatibilitySceneAuthority = FirstNonEmpty(input.CompatibilitySceneAuthority, input.InventoryCompatibilitySceneAuthority);
        var compatibilitySceneStability = FirstNonEmpty(input.CompatibilitySceneStability, input.InventoryCompatibilitySceneStability);

        var resolvedCurrentScreen = FirstNonEmpty(
            publishedCurrentScreen,
            rawCurrentScreen,
            rawObservedScreen,
            input.InventorySceneType);
        var resolvedVisibleScreen = FirstNonEmpty(
            publishedVisibleScreen,
            rawObservedScreen,
            rawCurrentScreen,
            input.InventorySceneType);
        var currentSource = ResolveCurrentSource(
            publishedCurrentScreen,
            rawCurrentScreen,
            rawObservedScreen,
            input.InventorySceneType);
        var visibleSource = ResolveVisibleSource(
            publishedVisibleScreen,
            rawObservedScreen,
            rawCurrentScreen,
            input.InventorySceneType);
        var combatPromotionApplied = input.EncounterInCombat == true
                                     && string.Equals(Normalize(input.TrackerCurrentScreen), "combat", StringComparison.OrdinalIgnoreCase);
        if (combatPromotionApplied)
        {
            resolvedCurrentScreen = "combat";
            resolvedVisibleScreen = "combat";
            currentSource = "combat-promotion";
            visibleSource = "combat-promotion";
        }

        return new ScreenProvenanceResult(
            resolvedCurrentScreen,
            resolvedVisibleScreen,
            publishedSceneReady,
            publishedSceneAuthority,
            publishedSceneStability,
            ComposeProvenanceSource(currentSource, visibleSource, combatPromotionApplied),
            combatPromotionApplied)
        {
            RawCurrentScreen = rawCurrentScreen,
            RawObservedScreen = rawObservedScreen,
            PublishedCurrentScreen = publishedCurrentScreen,
            PublishedVisibleScreen = publishedVisibleScreen,
            PublishedSceneReady = publishedSceneReady,
            PublishedSceneAuthority = publishedSceneAuthority,
            PublishedSceneStability = publishedSceneStability,
            CompatibilityCurrentScreen = compatibilityCurrentScreen,
            CompatibilityLogicalScreen = compatibilityLogicalScreen,
            CompatibilityVisibleScreen = compatibilityVisibleScreen,
            CompatibilitySceneReady = compatibilitySceneReady,
            CompatibilitySceneAuthority = compatibilitySceneAuthority,
            CompatibilitySceneStability = compatibilitySceneStability,
        };
    }

    private static string ComposeProvenanceSource(string currentSource, string visibleSource, bool combatPromotionApplied)
    {
        var source = string.Equals(currentSource, visibleSource, StringComparison.Ordinal)
            ? currentSource
            : $"current:{currentSource};visible:{visibleSource}";
        return combatPromotionApplied && !source.Contains("combat-promotion", StringComparison.Ordinal)
            ? source + "+combat-promotion"
            : source;
    }

    private static string ResolveCurrentSource(
        string? publishedCurrentScreen,
        string? rawCurrentScreen,
        string? rawObservedScreen,
        string? inventorySceneType)
    {
        if (!string.IsNullOrWhiteSpace(publishedCurrentScreen))
        {
            return "published-current";
        }

        if (!string.IsNullOrWhiteSpace(rawCurrentScreen))
        {
            return "raw-current";
        }

        if (!string.IsNullOrWhiteSpace(rawObservedScreen))
        {
            return "raw-observed";
        }

        return !string.IsNullOrWhiteSpace(inventorySceneType)
            ? "inventory-legacy"
            : "none";
    }

    private static string ResolveVisibleSource(
        string? publishedVisibleScreen,
        string? rawObservedScreen,
        string? rawCurrentScreen,
        string? inventorySceneType)
    {
        if (!string.IsNullOrWhiteSpace(publishedVisibleScreen))
        {
            return "published-visible";
        }

        if (!string.IsNullOrWhiteSpace(rawObservedScreen))
        {
            return "raw-observed";
        }

        if (!string.IsNullOrWhiteSpace(rawCurrentScreen))
        {
            return "raw-current";
        }

        return !string.IsNullOrWhiteSpace(inventorySceneType)
            ? "inventory-legacy"
            : "none";
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            var normalized = Normalize(value);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                return normalized;
            }
        }

        return null;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string? TryGetMeta(IReadOnlyDictionary<string, string?> meta, string key)
    {
        return meta.TryGetValue(key, out var value)
            ? value
            : null;
    }

    private static bool? TryParseBool(string? value)
    {
        return bool.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    private static string? TryReadString(JsonElement? root, string propertyName)
    {
        if (root is not { ValueKind: JsonValueKind.Object } element
            || !element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            JsonValueKind.True => bool.TrueString.ToLowerInvariant(),
            JsonValueKind.False => bool.FalseString.ToLowerInvariant(),
            _ => null,
        };
    }

    private static string? TryReadNestedString(JsonElement? root, string propertyName, string nestedPropertyName)
    {
        if (root is not { ValueKind: JsonValueKind.Object } element
            || !element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Object
            || !property.TryGetProperty(nestedPropertyName, out var nested))
        {
            return null;
        }

        return nested.ValueKind switch
        {
            JsonValueKind.String => nested.GetString(),
            JsonValueKind.Number => nested.GetRawText(),
            JsonValueKind.True => bool.TrueString.ToLowerInvariant(),
            JsonValueKind.False => bool.FalseString.ToLowerInvariant(),
            _ => null,
        };
    }

    private static bool? TryReadBool(JsonElement? root, string propertyName)
    {
        if (root is not { ValueKind: JsonValueKind.Object } element
            || !element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(property.GetString(), out var parsed) => parsed,
            _ => null,
        };
    }

    private static bool? TryReadBool(JsonElement? root, string propertyName, string nestedPropertyName)
    {
        if (root is not { ValueKind: JsonValueKind.Object } element
            || !element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Object
            || !property.TryGetProperty(nestedPropertyName, out var nested))
        {
            return null;
        }

        return nested.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(nested.GetString(), out var parsed) => parsed,
            _ => null,
        };
    }

    private static bool? TryReadNestedBool(JsonElement? root, string propertyName, string nestedPropertyName)
    {
        return TryReadBool(root, propertyName, nestedPropertyName);
    }
}
