using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;

static class GuiSmokeMapOverlayHeuristics
{
    public static MapOverlayState BuildState(ObserverState observer, WindowBounds? windowBounds, string? screenshotPath)
    {
        return BuildStateCore(observer.Summary, observer.StateDocument, windowBounds, screenshotPath);
    }

    public static MapOverlayState BuildState(ObserverSummary observer, WindowBounds? windowBounds, string? screenshotPath)
    {
        var stateDocument = GuiSmokeReplayArtifactLoader.TryLoadObserverStateSidecar(screenshotPath);
        return BuildStateCore(observer, stateDocument, windowBounds, screenshotPath);
    }

    private static MapOverlayState BuildStateCore(ObserverSummary observer, JsonDocument? stateDocument, WindowBounds? windowBounds, string? screenshotPath)
    {
        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(screenshotPath ?? string.Empty);
        var mapAnalysis = AutoMapAnalyzer.Analyze(screenshotPath ?? string.Empty);
        var declaringType = TryReadMetaString(stateDocument, "declaringType");
        var instanceType = TryReadMetaString(stateDocument, "instanceType");
        var rootTypeSummary = TryReadMetaString(stateDocument, "rootTypeSummary");
        var hasMapAuthority = ContainsMapAuthority(declaringType)
                              || ContainsMapAuthority(instanceType)
                              || ContainsMapAuthority(rootTypeSummary)
                              || string.Equals(TryReadMetaString(stateDocument, "mapOverlayVisible"), "true", StringComparison.OrdinalIgnoreCase);
        var exportedReachableNodeCandidatePresent = observer.Choices.Any(choice =>
                                                      MapNodeSourceSupport.IsExplicitMapPointChoice(choice)
                                                      && HasActiveBounds(choice.ScreenBounds, windowBounds))
                                                  || observer.ActionNodes.Any(node =>
                                                      node.Actionable
                                                      && MapNodeSourceSupport.IsExplicitMapPointNode(node)
                                                      && HasActiveBounds(node.ScreenBounds, windowBounds));
        var staleEventChoicePresent = HasEventChoiceEvidence(observer, windowBounds);
        var eventBackgroundPresent = string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(observer.VisibleScreen, "event", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(observer.ChoiceExtractorPath, "event", StringComparison.OrdinalIgnoreCase)
                                     || staleEventChoicePresent;
        var mapBackNavigationAvailable = overlayAnalysis.HasBottomLeftBackArrow
                                         || observer.CurrentChoices.Any(IsBackChoiceLabel)
                                         || observer.ActionNodes.Any(node => node.Actionable && IsBackChoiceLabel(node.Label));
        var foregroundVisible = hasMapAuthority
                                && (overlayAnalysis.HasBottomLeftBackArrow
                                    || exportedReachableNodeCandidatePresent
                                    || mapAnalysis.HasReachableNode
                                    || mapAnalysis.HasCurrentArrow);
        return new MapOverlayState(
            foregroundVisible,
            eventBackgroundPresent,
            mapBackNavigationAvailable,
            foregroundVisible && staleEventChoicePresent,
            foregroundVisible && mapAnalysis.HasCurrentArrow,
            foregroundVisible && (exportedReachableNodeCandidatePresent || mapAnalysis.HasReachableNode),
            foregroundVisible && exportedReachableNodeCandidatePresent);
    }

    private static bool HasEventChoiceEvidence(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return observer.Choices.Any(choice =>
                   HasActiveBounds(choice.ScreenBounds, windowBounds)
                   && (string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase))
                   && !IsMapChoice(choice)
                   && !IsBackChoiceLabel(choice.Label))
               || observer.ActionNodes.Any(node =>
                   node.Actionable
                   && HasActiveBounds(node.ScreenBounds, windowBounds)
                   && node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
                   && !IsBackChoiceLabel(node.Label));
    }

    private static bool HasActiveBounds(string? rawBounds, WindowBounds? windowBounds)
    {
        if (!TryParseBounds(rawBounds, out var x, out var y, out var width, out var height))
        {
            return false;
        }

        if (windowBounds is null)
        {
            return true;
        }

        var rect = new RectangleF(x, y, width, height);
        var windowRect = new RectangleF(windowBounds.X, windowBounds.Y, windowBounds.Width, windowBounds.Height);
        return rect.Width > 0f
               && rect.Height > 0f
               && rect.Right > windowRect.Left
               && rect.Bottom > windowRect.Top
               && rect.Left < windowRect.Right
               && rect.Top < windowRect.Bottom;
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

    private static bool IsBackChoiceLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        return label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
               || label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
               || label.Contains("뒤로", StringComparison.OrdinalIgnoreCase)
               || label.Contains("Back", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMapChoice(ObserverChoice choice)
    {
        return MapNodeSourceSupport.IsExplicitMapPointChoice(choice);
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
}
