using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;

static class GuiSmokeForegroundHeuristics
{
    public static bool ShouldPreferEventProgressionOverMapFallback(ObserverState observer)
    {
        if (NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer))
        {
            return false;
        }

        var declaringType = TryReadMetaString(observer.StateDocument, "declaringType");
        var instanceType = TryReadMetaString(observer.StateDocument, "instanceType");
        if (ContainsMapAuthority(declaringType) || ContainsMapAuthority(instanceType))
        {
            return false;
        }

        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, null);
        return eventScene.EventForegroundOwned
               && eventScene.ReleaseStage == EventReleaseStage.Active;
    }

    public static bool ShouldPreferEventProgressionOverMapFallback(ObserverSummary observer)
    {
        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, null);
        return eventScene.EventForegroundOwned
               && eventScene.ReleaseStage == EventReleaseStage.Active;
    }

    private static bool HasEventForegroundAuthority(ObserverSummary observer)
    {
        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, null);
        return eventScene.EventForegroundOwned
               && eventScene.ReleaseStage == EventReleaseStage.Active
               && eventScene.HasExplicitProgression;
    }

    private static bool HasUsableBounds(string? rawBounds)
    {
        return TryParseBounds(rawBounds, out _, out _, out _, out _);
    }

    private static bool LooksLikeProceedOrSkip(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        return label.Contains("넘기", StringComparison.OrdinalIgnoreCase)
               || label.Contains("skip", StringComparison.OrdinalIgnoreCase)
               || label.Contains("proceed", StringComparison.OrdinalIgnoreCase)
               || label.Contains("continue", StringComparison.OrdinalIgnoreCase)
               || label.Contains("진행", StringComparison.OrdinalIgnoreCase)
               || label.Contains("확인", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsEventAuthority(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NEventRoom", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsMapAuthority(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryReadMetaString(JsonDocument? document, string propertyName)
    {
        if (document is null)
        {
            return null;
        }

        if (document.RootElement.TryGetProperty("meta", out var metaElement)
            && metaElement.ValueKind == JsonValueKind.Object
            && metaElement.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return null;
    }

    private static bool TryParseBounds(string? rawBounds, out float x, out float y, out float width, out float height)
    {
        x = default;
        y = default;
        width = default;
        height = default;
        if (string.IsNullOrWhiteSpace(rawBounds))
        {
            return false;
        }

        var parts = rawBounds.Split(',', StringSplitOptions.TrimEntries);
        return parts.Length == 4
               && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x)
               && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y)
               && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out width)
               && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out height)
               && width > 0f
               && height > 0f;
    }
}
