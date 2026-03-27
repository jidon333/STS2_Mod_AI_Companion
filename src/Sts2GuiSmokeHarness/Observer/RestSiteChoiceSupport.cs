using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;

static class RestSiteChoiceSupport
{
    private static readonly string[] DefaultAutoPickIds = { "HEAL", "SMITH", "HATCH" };

    public static IReadOnlyList<RestSiteObservedChoice> GetObservedChoices(ObserverSummary observer)
    {
        var metadataChoices = observer.Choices
            .Where(HasAuthoritativeMetadata)
            .Select(choice => CreateObservedChoiceFromMetadata(observer, choice))
            .Where(static choice => choice is not null)
            .Cast<RestSiteObservedChoice>()
            .GroupBy(static choice => choice.OptionId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderBy(static choice => string.IsNullOrWhiteSpace(choice.Choice?.ScreenBounds) ? 1 : 0)
                .ThenBy(static choice => TryGetSortX(choice.Choice?.ScreenBounds))
                .ThenBy(static choice => TryGetSortY(choice.Choice?.ScreenBounds))
                .ThenBy(static choice => choice.Label, StringComparer.Ordinal)
                .First())
            .OrderBy(static choice => TryGetSortX(choice.Choice?.ScreenBounds))
            .ThenBy(static choice => TryGetSortY(choice.Choice?.ScreenBounds))
            .ThenBy(static choice => choice.OptionId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (metadataChoices.Length > 0)
        {
            return metadataChoices;
        }

        return DefaultAutoPickIds
            .Select(optionId => CreateObservedChoiceFromLegacy(observer, optionId))
            .Where(static choice => choice is not null)
            .Cast<RestSiteObservedChoice>()
            .ToArray();
    }

    public static bool HasExplicitRestSiteChoiceAffordance(ObserverSummary observer)
        => GetObservedChoices(observer).Count > 0;

    public static bool HasRestSiteRestChoice(ObserverSummary observer)
        => GetObservedChoices(observer).Any(static choice => string.Equals(choice.OptionId, "HEAL", StringComparison.OrdinalIgnoreCase));

    public static bool HasRestSiteSmithChoice(ObserverSummary observer)
        => GetObservedChoices(observer).Any(static choice => string.Equals(choice.OptionId, "SMITH", StringComparison.OrdinalIgnoreCase));

    public static bool HasRestSiteHatchChoice(ObserverSummary observer)
        => GetObservedChoices(observer).Any(static choice => string.Equals(choice.OptionId, "HATCH", StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<string> BuildAllowedActions(ObserverSummary observer)
    {
        return GetObservedChoices(observer)
            .Select(static choice => choice.CandidateLabel)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Concat(new[] { "click smith card", "click smith confirm", "wait" })
            .ToArray();
    }

    public static IReadOnlyList<string> BuildCandidateLabels(ObserverSummary observer)
        => BuildAllowedActions(observer);

    public static bool HasAuthoritativeMetadata(ObserverSummary observer)
        => GetObservedChoices(observer).Any(static choice => choice.HasMetadata);

    public static bool HasAuthoritativeMetadata(ObserverChoice choice)
    {
        if (choice is null)
        {
            return false;
        }

        return string.Equals(choice.BindingKind, "rest-site-option", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.Kind, "rest-option", StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(choice.NodeId)
                   && choice.NodeId.StartsWith("rest-site:", StringComparison.OrdinalIgnoreCase)
                   && !choice.NodeId.Contains("smith-card", StringComparison.OrdinalIgnoreCase)
                   && !choice.NodeId.Contains("smith-confirm", StringComparison.OrdinalIgnoreCase))
               || choice.SemanticHints.Any(static hint => hint.StartsWith("option-id:", StringComparison.OrdinalIgnoreCase));
    }

    public static string? NormalizeOptionId(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var trimmed = raw.Trim();
        if (trimmed.StartsWith("rest-site:", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed["rest-site:".Length..];
        }

        if (trimmed.StartsWith("option:", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed["option:".Length..];
        }

        if (trimmed.StartsWith("OPTION_", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed["OPTION_".Length..];
        }

        trimmed = trimmed.ToUpperInvariant();
        return trimmed switch
        {
            "REST" => "HEAL",
            "SMITH" => "SMITH",
            "HATCH" => "HATCH",
            "HEAL" => "HEAL",
            _ when string.IsNullOrWhiteSpace(trimmed) => null,
            _ => trimmed,
        };
    }

    public static string? MapLabelToOptionId(string? label)
    {
        if (!string.IsNullOrWhiteSpace(label))
        {
            if (label.Contains("rest site: smith", StringComparison.OrdinalIgnoreCase)
                || label.Contains("rest-site:smith", StringComparison.OrdinalIgnoreCase))
            {
                return "SMITH";
            }

            if (label.Contains("rest site: rest", StringComparison.OrdinalIgnoreCase)
                || label.Contains("rest-site:rest", StringComparison.OrdinalIgnoreCase))
            {
                return "HEAL";
            }

            if (label.Contains("rest site: hatch", StringComparison.OrdinalIgnoreCase)
                || label.Contains("rest-site:hatch", StringComparison.OrdinalIgnoreCase))
            {
                return "HATCH";
            }
        }

        if (HasRestSiteRestLabelInternal(label))
        {
            return "HEAL";
        }

        if (HasRestSiteSmithLabelInternal(label))
        {
            return "SMITH";
        }

        if (HasRestSiteHatchLabelInternal(label))
        {
            return "HATCH";
        }

        return null;
    }

    public static string? TryResolveOptionId(ObserverChoice choice)
    {
        var semanticOptionId = choice.SemanticHints
            .FirstOrDefault(static hint => hint.StartsWith("option-id:", StringComparison.OrdinalIgnoreCase));
        return NormalizeOptionId(
            choice.BindingId
            ?? ExtractOptionIdFromNodeId(choice.NodeId)
            ?? choice.Value
            ?? semanticOptionId?["option-id:".Length..]
            ?? MapLabelToOptionId(choice.Label));
    }

    public static string MapOptionIdToTargetLabel(string optionId)
    {
        return NormalizeOptionId(optionId) switch
        {
            "HEAL" => "rest site: rest",
            "SMITH" => "rest site: smith",
            "HATCH" => "rest site: hatch",
            { Length: > 0 } normalized => $"rest site: option:{normalized}",
            _ => "rest site: smith",
        };
    }

    public static string MapOptionIdToCandidateLabel(string optionId)
    {
        return NormalizeOptionId(optionId) switch
        {
            "HEAL" => "click rest site choice",
            "SMITH" => "click rest site choice",
            "HATCH" => "click rest site choice",
            { Length: > 0 } normalized => $"click option:{normalized}",
            _ => "click rest site choice",
        };
    }

    public static string BuildExplicitChoiceFingerprint(ObserverSummary observer)
    {
        var options = GetObservedChoices(observer)
            .Select(static choice => $"{choice.OptionId}:{choice.IsEnabled.ToString().ToLowerInvariant()}")
            .ToArray();
        return string.Join(
            "|",
            "rest-site",
            observer.EncounterKind ?? "none",
            observer.ChoiceExtractorPath ?? "unknown",
            string.Join(",", options));
    }

    public static string? DetermineAutoPickOptionId(ObserverSummary observer)
    {
        var options = GetObservedChoices(observer);
        if (options.Count == 0)
        {
            return null;
        }

        var hasMetadata = options.Any(static option => option.HasMetadata);
        static bool IsActionable(RestSiteObservedChoice option, bool hasMetadata)
            => !hasMetadata || option.IsEnabled;

        var maxHp = observer.PlayerMaxHp ?? 0;
        var currentHp = observer.PlayerCurrentHp ?? 0;
        var shouldRest = options.Any(option => string.Equals(option.OptionId, "HEAL", StringComparison.OrdinalIgnoreCase) && IsActionable(option, hasMetadata))
                         && (maxHp <= 0 || currentHp <= Math.Ceiling(maxHp * 0.70d));
        if (shouldRest)
        {
            return "HEAL";
        }

        if (options.Any(option => string.Equals(option.OptionId, "SMITH", StringComparison.OrdinalIgnoreCase) && IsActionable(option, hasMetadata)))
        {
            return "SMITH";
        }

        if (options.Any(option => string.Equals(option.OptionId, "HATCH", StringComparison.OrdinalIgnoreCase) && IsActionable(option, hasMetadata)))
        {
            return "HATCH";
        }

        return null;
    }

    private static RestSiteObservedChoice? CreateObservedChoiceFromMetadata(ObserverSummary observer, ObserverChoice choice)
    {
        var optionId = TryResolveOptionId(choice);
        if (string.IsNullOrWhiteSpace(optionId))
        {
            return null;
        }

        var normalizedOptionId = NormalizeOptionId(optionId);
        if (string.IsNullOrWhiteSpace(normalizedOptionId))
        {
            return null;
        }

        var matchingNode = FindMatchingActionNode(observer, choice, normalizedOptionId);
        return new RestSiteObservedChoice(
            normalizedOptionId,
            MapOptionIdToTargetLabel(normalizedOptionId),
            MapOptionIdToCandidateLabel(normalizedOptionId),
            choice.Label,
            choice.Description,
            HasMetadata: true,
            IsEnabled: choice.Enabled ?? matchingNode?.Actionable ?? true,
            IsDefaultAutoPick: DefaultAutoPickIds.Contains(normalizedOptionId, StringComparer.OrdinalIgnoreCase),
            Choice: choice,
            ActionNode: matchingNode);
    }

    private static RestSiteObservedChoice? CreateObservedChoiceFromLegacy(ObserverSummary observer, string optionId)
    {
        var matchingChoice = observer.Choices.FirstOrDefault(choice => MatchesOption(choice.Label, optionId));
        var matchingNode = observer.ActionNodes.FirstOrDefault(node => node.Actionable && MatchesOption(node.Label, optionId));
        if (matchingChoice is null
            && matchingNode is null
            && !observer.CurrentChoices.Any(label => MatchesOption(label, optionId)))
        {
            return null;
        }

        return new RestSiteObservedChoice(
            optionId,
            MapOptionIdToTargetLabel(optionId),
            MapOptionIdToCandidateLabel(optionId),
            matchingChoice?.Label ?? matchingNode?.Label ?? MapOptionIdToTargetLabel(optionId),
            matchingChoice?.Description,
            HasMetadata: false,
            IsEnabled: matchingNode?.Actionable ?? true,
            IsDefaultAutoPick: true,
            Choice: matchingChoice,
            ActionNode: matchingNode);
    }

    private static ObserverActionNode? FindMatchingActionNode(ObserverSummary observer, ObserverChoice choice, string optionId)
    {
        if (!string.IsNullOrWhiteSpace(choice.NodeId))
        {
            var nodeById = observer.ActionNodes.FirstOrDefault(node => string.Equals(node.NodeId, choice.NodeId, StringComparison.OrdinalIgnoreCase));
            if (nodeById is not null)
            {
                return nodeById;
            }
        }

        return observer.ActionNodes.FirstOrDefault(node =>
            MatchesOption(node.Label, optionId)
            || string.Equals(ExtractOptionIdFromNodeId(node.NodeId), optionId, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ExtractOptionIdFromNodeId(string? nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return null;
        }

        var prefix = "rest-site:";
        return nodeId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? NormalizeOptionId(nodeId[prefix.Length..])
            : null;
    }

    private static bool MatchesOption(string? label, string optionId)
    {
        return NormalizeOptionId(optionId) switch
        {
            "HEAL" => HasRestSiteRestLabelInternal(label),
            "SMITH" => HasRestSiteSmithLabelInternal(label),
            "HATCH" => HasRestSiteHatchLabelInternal(label),
            _ => false,
        };
    }

    private static double TryGetSortX(string? rawBounds)
        => TryParseBoundsInternal(rawBounds, out var bounds) ? bounds.X : double.MaxValue;

    private static double TryGetSortY(string? rawBounds)
        => TryParseBoundsInternal(rawBounds, out var bounds) ? bounds.Y : double.MaxValue;

    private static bool HasRestSiteRestLabelInternal(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("휴식", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Rest", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasRestSiteSmithLabelInternal(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("재련", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Smith", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasRestSiteHatchLabelInternal(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("부화", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Hatch", StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryParseBoundsInternal(string? rawBounds, out RectangleF bounds)
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
}
