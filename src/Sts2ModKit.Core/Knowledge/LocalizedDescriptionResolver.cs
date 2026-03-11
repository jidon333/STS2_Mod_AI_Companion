using System.Text.RegularExpressions;

namespace Sts2ModKit.Core.Knowledge;

internal static class LocalizedDescriptionResolver
{
    private static readonly Regex PlaceholderRegex = new(@"\{(?<name>[A-Za-z0-9_]+)(?::(?<modifier>[^{}]+))?\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string? Resolve(string? text, IReadOnlyDictionary<string, string?> attributes)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        return PlaceholderRegex.Replace(text, match => ResolvePlaceholder(match, attributes));
    }

    private static string ResolvePlaceholder(Match match, IReadOnlyDictionary<string, string?> attributes)
    {
        var name = match.Groups["name"].Value;
        if (!TryReadNumericValue(attributes, name, out var rawValue))
        {
            return match.Value;
        }

        var modifier = match.Groups["modifier"].Value;
        if (string.IsNullOrWhiteSpace(modifier))
        {
            return rawValue!;
        }

        if (modifier.StartsWith("plural:", StringComparison.OrdinalIgnoreCase))
        {
            var options = modifier["plural:".Length..].Split('|', StringSplitOptions.None);
            if (options.Length >= 2 && decimal.TryParse(rawValue, out var numeric))
            {
                return numeric == 1m ? options[0] : options[1];
            }

            return rawValue!;
        }

        if (modifier.StartsWith("choose(", StringComparison.OrdinalIgnoreCase))
        {
            var splitIndex = modifier.IndexOf(':');
            if (splitIndex > 0 && decimal.TryParse(rawValue, out var numeric))
            {
                var thresholdSegment = modifier["choose(".Length..splitIndex].TrimEnd(')');
                if (decimal.TryParse(thresholdSegment, out var threshold))
                {
                    var options = modifier[(splitIndex + 1)..].Split('|', StringSplitOptions.None);
                    if (options.Length >= 2)
                    {
                        return numeric == threshold ? options[0] : options[1];
                    }
                }
            }

            return rawValue!;
        }

        if (modifier.StartsWith("diff", StringComparison.OrdinalIgnoreCase)
            || modifier.StartsWith("energyIcons", StringComparison.OrdinalIgnoreCase)
            || modifier.StartsWith("starIcons", StringComparison.OrdinalIgnoreCase)
            || modifier.StartsWith("cond", StringComparison.OrdinalIgnoreCase))
        {
            return rawValue!;
        }

        return rawValue!;
    }

    private static bool TryReadNumericValue(IReadOnlyDictionary<string, string?> attributes, string name, out string? value)
    {
        if (attributes.TryGetValue($"var.{name}", out value) && !string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (attributes.TryGetValue(name, out value) && !string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        value = null;
        return false;
    }
}
