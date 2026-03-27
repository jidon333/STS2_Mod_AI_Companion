using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;

static class MapNodeSourceSupport
{
    public static bool IsExplicitMapPointChoice(ObserverChoice choice)
    {
        if (!string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(choice.NodeId)
            && choice.NodeId.StartsWith("map:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if ((choice.Description?.Contains("coord:", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            return true;
        }

        return choice.SemanticHints.Any(static hint =>
            !string.IsNullOrWhiteSpace(hint)
            && (hint.StartsWith("coord:", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "raw-kind:map-node", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "source:map-choice", StringComparison.OrdinalIgnoreCase)));
    }

    public static bool IsExplicitMapPointNode(ObserverActionNode node)
    {
        if (!string.Equals(node.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
            || IsTopBarInventoryBounds(node.ScreenBounds))
        {
            return false;
        }

        if (string.Equals(node.TypeName, "relic", StringComparison.OrdinalIgnoreCase)
            || node.SemanticHints.Any(static hint => string.Equals(hint, "raw-kind:relic", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(node.NodeId)
            && node.NodeId.StartsWith("map:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(node.TypeName, "map-node", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return node.SemanticHints.Any(static hint =>
            !string.IsNullOrWhiteSpace(hint)
            && (hint.StartsWith("coord:", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "raw-kind:map-node", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "source:map-choice", StringComparison.OrdinalIgnoreCase)));
    }

    private static bool IsTopBarInventoryBounds(string? screenBounds)
    {
        if (string.IsNullOrWhiteSpace(screenBounds))
        {
            return false;
        }

        var parts = screenBounds.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4
            || !float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
        {
            return false;
        }

        return width <= 120f
               && height <= 120f
               && y <= 170f
               && x <= 200f;
    }
}
