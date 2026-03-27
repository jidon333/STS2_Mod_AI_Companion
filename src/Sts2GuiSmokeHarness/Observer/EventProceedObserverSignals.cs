using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;

static class EventProceedObserverSignals
{
    public static bool HasExplicitEventProceedAuthority(ObserverState observer, WindowBounds? windowBounds)
    {
        return HasExplicitEventProceedAuthority(observer.Summary, windowBounds);
    }

    public static bool HasExplicitEventProceedAuthority(ObserverSummary observer, WindowBounds? windowBounds)
    {
        if (!HasEventChoiceAuthority(observer))
        {
            return false;
        }

        if (NonCombatForegroundOwnership.Resolve(observer) is not NonCombatForegroundOwner.Event and not NonCombatForegroundOwner.Unknown)
        {
            return false;
        }

        return HasExplicitEventProceedSignal(observer, windowBounds);
    }

    public static bool HasExplicitEventProceedSignal(ObserverSummary observer, WindowBounds? windowBounds)
    {
        if (TryGetMetaBool(observer, "eventProceedOptionEnabled") == true
            || TryGetMetaBool(observer, "eventProceedOptionVisible") == true)
        {
            return true;
        }

        return observer.ActionNodes.Any(node =>
                   IsExplicitEventProceedNode(node)
                   && HasActiveBounds(node.ScreenBounds, windowBounds))
               || observer.Choices.Any(choice =>
                   IsExplicitEventProceedChoice(choice)
                   && HasActiveBounds(choice.ScreenBounds, windowBounds));
    }

    public static bool HasEventChoiceAuthority(ObserverSummary observer)
    {
        return string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.VisibleScreen, "event", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "event", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "room-event", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsExplicitEventProceedNode(ObserverActionNode node)
    {
        return node.Actionable
               && ((node.NodeId?.StartsWith("event-option:", StringComparison.OrdinalIgnoreCase) ?? false)
                   || node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase))
               && HasExplicitEventProceedSemantic(node.SemanticHints);
    }

    public static bool IsExplicitEventProceedChoice(ObserverChoice choice)
    {
        return !string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
               && HasExplicitEventProceedSemantic(choice.SemanticHints);
    }

    public static bool HasExplicitEventProceedSemantic(IReadOnlyList<string> semanticHints)
    {
        return semanticHints.Any(static hint =>
            string.Equals(hint, "option-role:proceed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(hint, "event-proceed", StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryGetMetaBool(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value)
               && bool.TryParse(value, out var parsed)
               && parsed;
    }

    private static bool HasActiveBounds(string? screenBounds, WindowBounds? windowBounds)
    {
        if (HasUsableLogicalBounds(screenBounds))
        {
            return true;
        }

        return windowBounds is not null && IsBoundsInsideWindow(screenBounds, windowBounds);
    }

    private static bool HasUsableLogicalBounds(string? screenBounds)
    {
        return TryParseBounds(screenBounds, out var bounds)
               && bounds.X >= 0f
               && bounds.Y >= 0f
               && bounds.Right <= 1920f
               && bounds.Bottom <= 1080f;
    }

    private static bool IsBoundsInsideWindow(string? screenBounds, WindowBounds windowBounds)
    {
        return TryParseBounds(screenBounds, out var bounds)
               && bounds.Right > windowBounds.X
               && bounds.Bottom > windowBounds.Y
               && bounds.X < windowBounds.X + windowBounds.Width
               && bounds.Y < windowBounds.Y + windowBounds.Height;
    }

    private static bool TryParseBounds(string? rawBounds, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(rawBounds))
        {
            return false;
        }

        var parts = rawBounds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 4
            || !float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height)
            || width <= 0f
            || height <= 0f)
        {
            return false;
        }

        bounds = new RectangleF(x, y, width, height);
        return true;
    }
}
