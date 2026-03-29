using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

static class AncientEventObserverSignals
{
    public static string? GetForegroundOwner(ObserverSummary observer)
    {
        return TryGetMetaString(observer, "foregroundOwner");
    }

    public static string? GetForegroundActionLane(ObserverSummary observer)
    {
        return TryGetMetaString(observer, "foregroundActionLane");
    }

    public static bool IsMapForegroundOwner(ObserverSummary observer)
    {
        return string.Equals(GetForegroundOwner(observer), "map", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsEventForegroundOwner(ObserverSummary observer)
    {
        return string.Equals(GetForegroundOwner(observer), "event", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsMapSurfacePending(ObserverSummary observer)
    {
        return IsMapForegroundOwner(observer)
               && TryGetMetaBool(observer, "mapSurfacePending") == true;
    }

    public static bool IsAncientEventDetected(ObserverSummary observer)
    {
        return TryGetMetaBool(observer, "ancientEventDetected") == true;
    }

    public static bool IsDialogueActive(ObserverSummary observer)
    {
        if (IsEventForegroundOwner(observer)
            && string.Equals(GetForegroundActionLane(observer), "ancient-dialogue", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return IsAncientEventDetected(observer)
               && TryGetMetaBool(observer, "ancientDialogueActive") == true;
    }

    public static bool HasExplicitOptionSelection(ObserverSummary observer)
    {
        if (IsEventForegroundOwner(observer)
            && string.Equals(GetForegroundActionLane(observer), "ancient-option", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!IsAncientEventDetected(observer))
        {
            return false;
        }

        var optionCount = TryGetMetaInt(observer, "ancientOptionCount") ?? 0;
        var completionCount = TryGetMetaInt(observer, "ancientCompletionCount") ?? 0;
        if (optionCount > completionCount)
        {
            return true;
        }

        return observer.ActionNodes.Any(node => IsExplicitAncientOptionNode(node) && !IsExplicitAncientCompletionNode(node))
               || observer.Choices.Any(choice => IsExplicitAncientOptionChoice(choice) && !IsExplicitAncientCompletionChoice(choice));
    }

    public static bool HasExplicitCompletionAction(ObserverSummary observer)
    {
        if (IsEventForegroundOwner(observer)
            && string.Equals(GetForegroundActionLane(observer), "ancient-completion", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!IsAncientEventDetected(observer))
        {
            return false;
        }

        var completionCount = TryGetMetaInt(observer, "ancientCompletionCount");
        if (completionCount is > 0)
        {
            return true;
        }

        return observer.ActionNodes.Any(IsExplicitAncientCompletionNode)
               || observer.Choices.Any(IsExplicitAncientCompletionChoice);
    }

    public static bool CompletionUsesDefaultFocus(ObserverSummary observer)
    {
        return HasExplicitCompletionAction(observer)
               && TryGetMetaBool(observer, "ancientCompletionUsesDefaultFocus") == true;
    }

    public static bool CompletionHasFocus(ObserverSummary observer)
    {
        return HasExplicitCompletionAction(observer)
               && TryGetMetaBool(observer, "ancientCompletionHasFocus") == true;
    }

    public static bool HasMapReleaseAuthority(ObserverSummary observer, string? declaringType, string? instanceType)
    {
        if (IsMapForegroundOwner(observer) || TryGetMetaBool(observer, "mapReleaseAuthority") == true)
        {
            return true;
        }

        static bool IsMapScreenTypeName(string? typeName)
        {
            return !string.IsNullOrWhiteSpace(typeName)
                   && typeName.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase);
        }

        if (!IsAncientEventDetected(observer))
        {
            return false;
        }

        if (MatchesControlFlowScreen(observer, "map"))
        {
            return true;
        }

        if (TryGetMetaBool(observer, "mapCurrentActiveScreen") == true)
        {
            return true;
        }

        var activeScreenType = observer.Meta.TryGetValue("activeScreenType", out var activeScreenTypeValue)
            ? activeScreenTypeValue
            : null;
        return IsMapScreenTypeName(activeScreenType)
               || IsMapScreenTypeName(declaringType)
               || IsMapScreenTypeName(instanceType);
    }

    public static bool HasForegroundAuthority(ObserverSummary observer)
    {
        if (IsMapForegroundOwner(observer))
        {
            return false;
        }

        if (IsEventForegroundOwner(observer))
        {
            return true;
        }

        return IsDialogueActive(observer)
               || HasExplicitCompletionAction(observer)
               || HasExplicitOptionSelection(observer);
    }

    public static bool IsAncientDialogueNode(ObserverActionNode node)
    {
        return node.Actionable
               && (string.Equals(node.TypeName, "event-dialogue", StringComparison.OrdinalIgnoreCase)
                   || node.NodeId.StartsWith("ancient-dialogue:", StringComparison.OrdinalIgnoreCase)
                   || node.SemanticHints.Any(static hint => string.Equals(hint, "source:ancient-dialogue-hitbox", StringComparison.OrdinalIgnoreCase)));
    }

    public static bool IsAncientDialogueChoice(ObserverChoice choice)
    {
        return string.Equals(choice.Kind, "event-dialogue", StringComparison.OrdinalIgnoreCase)
               || (choice.NodeId?.StartsWith("ancient-dialogue:", StringComparison.OrdinalIgnoreCase) ?? false)
               || choice.SemanticHints.Any(static hint => string.Equals(hint, "source:ancient-dialogue-hitbox", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsExplicitAncientOptionNode(ObserverActionNode node)
    {
        return node.Actionable
               && ((node.NodeId?.StartsWith("ancient-event-option:", StringComparison.OrdinalIgnoreCase) ?? false)
                   || node.SemanticHints.Any(static hint => string.Equals(hint, "source:ancient-option-button", StringComparison.OrdinalIgnoreCase))
                   || string.Equals(node.TypeName, "event-option", StringComparison.OrdinalIgnoreCase)
                      && node.SemanticHints.Any(static hint => string.Equals(hint, "ancient-event", StringComparison.OrdinalIgnoreCase)));
    }

    public static bool IsExplicitAncientOptionChoice(ObserverChoice choice)
    {
        return string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)
               && ((choice.NodeId?.StartsWith("ancient-event-option:", StringComparison.OrdinalIgnoreCase) ?? false)
                   || choice.SemanticHints.Any(static hint => string.Equals(hint, "source:ancient-option-button", StringComparison.OrdinalIgnoreCase))
                   || choice.SemanticHints.Any(static hint => string.Equals(hint, "ancient-event", StringComparison.OrdinalIgnoreCase)));
    }

    public static bool IsExplicitAncientCompletionNode(ObserverActionNode node)
    {
        return IsExplicitAncientOptionNode(node)
               && node.SemanticHints.Any(static hint =>
                   string.Equals(hint, "ancient-event-completion", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(hint, "option-role:proceed", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsExplicitAncientCompletionChoice(ObserverChoice choice)
    {
        return IsExplicitAncientOptionChoice(choice)
               && choice.SemanticHints.Any(static hint =>
                   string.Equals(hint, "ancient-event-completion", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(hint, "option-role:proceed", StringComparison.OrdinalIgnoreCase));
    }

    public static ObserverActionNode? GetActiveDialogueNode(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return observer.ActionNodes.FirstOrDefault(node =>
            IsAncientDialogueNode(node)
            && HasActiveBounds(node.ScreenBounds, windowBounds));
    }

    public static ObserverChoice? GetActiveDialogueChoice(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return observer.Choices.FirstOrDefault(choice =>
            IsAncientDialogueChoice(choice)
            && HasActiveBounds(choice.ScreenBounds, windowBounds));
    }

    public static IReadOnlyList<ObserverActionNode> GetActiveOptionNodes(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return observer.ActionNodes
            .Where(node =>
                IsExplicitAncientOptionNode(node)
                && !IsExplicitAncientCompletionNode(node)
                && HasActiveBounds(node.ScreenBounds, windowBounds))
            .OrderBy(GetNodeSortYInternal)
            .ThenBy(GetNodeSortXInternal)
            .ToArray();
    }

    public static IReadOnlyList<ObserverChoice> GetActiveOptionChoices(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return observer.Choices
            .Where(choice =>
                IsExplicitAncientOptionChoice(choice)
                && !IsExplicitAncientCompletionChoice(choice)
                && HasActiveBounds(choice.ScreenBounds, windowBounds))
            .OrderBy(GetChoiceSortYInternal)
            .ThenBy(GetChoiceSortXInternal)
            .ToArray();
    }

    public static ObserverActionNode? GetActiveCompletionNode(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return observer.ActionNodes.FirstOrDefault(node =>
            IsExplicitAncientCompletionNode(node)
            && HasActiveBounds(node.ScreenBounds, windowBounds));
    }

    public static ObserverChoice? GetActiveCompletionChoice(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return observer.Choices.FirstOrDefault(choice =>
            IsExplicitAncientCompletionChoice(choice)
            && HasActiveBounds(choice.ScreenBounds, windowBounds));
    }

    private static bool HasActiveBounds(string? rawBounds, WindowBounds? windowBounds)
    {
        if (!TryParseBounds(rawBounds, out var bounds))
        {
            return false;
        }

        var centerX = bounds.X + bounds.Width / 2f;
        var centerY = bounds.Y + bounds.Height / 2f;
        if (centerX >= 0f
            && centerY >= 0f
            && centerX <= 1920f
            && centerY <= 1080f)
        {
            return true;
        }

        if (windowBounds is null)
        {
            return true;
        }

        var windowRect = new RectangleF(windowBounds.X, windowBounds.Y, windowBounds.Width, windowBounds.Height);
        return bounds.Width > 0f
               && bounds.Height > 0f
               && bounds.Right > windowRect.Left
               && bounds.Bottom > windowRect.Top
               && bounds.Left < windowRect.Right
               && bounds.Top < windowRect.Bottom;
    }

    private static float GetNodeSortYInternal(ObserverActionNode node)
    {
        return TryParseBounds(node.ScreenBounds, out var bounds) ? bounds.Y : float.MaxValue;
    }

    private static float GetNodeSortXInternal(ObserverActionNode node)
    {
        return TryParseBounds(node.ScreenBounds, out var bounds) ? bounds.X : float.MaxValue;
    }

    private static float GetChoiceSortYInternal(ObserverChoice choice)
    {
        return TryParseBounds(choice.ScreenBounds, out var bounds) ? bounds.Y : float.MaxValue;
    }

    private static float GetChoiceSortXInternal(ObserverChoice choice)
    {
        return TryParseBounds(choice.ScreenBounds, out var bounds) ? bounds.X : float.MaxValue;
    }

    private static bool TryParseBounds(string? rawBounds, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(rawBounds))
        {
            return false;
        }

        var parts = rawBounds.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
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

    private static bool? TryGetMetaBool(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value)
               ? bool.TryParse(value, out var parsed)
                    ? parsed
                    : null
               : null;
    }

    private static string? TryGetMetaString(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value)
            ? value
            : null;
    }

    private static int? TryGetMetaInt(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value)
               ? int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : null
               : null;
    }
}
