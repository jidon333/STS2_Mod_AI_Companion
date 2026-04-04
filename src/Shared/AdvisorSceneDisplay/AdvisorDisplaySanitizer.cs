using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sts2AiCompanion.AdvisorSceneDisplay;

public static partial class AdvisorDisplaySanitizer
{
    private static readonly string[] DomainPrefixes =
    {
        "CARD",
        "RELIC",
        "POTION",
        "EVENT",
        "REWARD",
        "SHOP",
        "KEYWORD",
    };

    public static string? SanitizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var sanitized = ColorBbCodeRegex().Replace(text, string.Empty);
        sanitized = NamedColorBbCodeRegex().Replace(sanitized, string.Empty);
        sanitized = ColorXmlRegex().Replace(sanitized, string.Empty);
        sanitized = TokenRegex().Replace(sanitized, string.Empty);
        sanitized = SeparatorRegex().Replace(sanitized, " ");
        sanitized = WhitespaceRegex().Replace(sanitized, " ");
        sanitized = sanitized
            .Replace(" ,", ",", StringComparison.Ordinal)
            .Replace(" .", ".", StringComparison.Ordinal)
            .Replace(" :", ":", StringComparison.Ordinal)
            .Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? null : sanitized;
    }

    public static string Sanitize(string? text)
    {
        return SanitizeText(text) ?? string.Empty;
    }

    public static bool IsPlaceholderDescription(string? text, string? title = null)
    {
        var sanitized = SanitizeText(text);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(title)
            && string.Equals(sanitized, SanitizeText(title), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return sanitized.Equals("Merchant inventory slot", StringComparison.OrdinalIgnoreCase)
               || sanitized.Equals("Card for sale.", StringComparison.OrdinalIgnoreCase)
               || sanitized.Equals("Card for sale", StringComparison.OrdinalIgnoreCase)
               || sanitized.Equals("Reward Button", StringComparison.OrdinalIgnoreCase)
               || sanitized.Equals("Reward", StringComparison.OrdinalIgnoreCase)
               || sanitized.Equals("Shop", StringComparison.OrdinalIgnoreCase);
    }

    public static string? PrettifyIdentifier(string? raw)
    {
        var sanitized = SanitizeText(raw);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return null;
        }

        var spaced = CamelBoundaryRegex().Replace(sanitized, "$1 $2");
        var tokens = spaced
            .Split(new[] { '.', '_', '-', ':', '/', '\\', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        if (tokens.Count > 1 && DomainPrefixes.Contains(tokens[0], StringComparer.OrdinalIgnoreCase))
        {
            tokens.RemoveAt(0);
        }

        if (tokens.Count == 0)
        {
            return null;
        }

        var joined = string.Join(" ", tokens);
        return MostlyIdentifierRegex().IsMatch(joined)
            ? CultureInfo.InvariantCulture.TextInfo.ToTitleCase(joined.ToLowerInvariant())
            : joined;
    }

    [GeneratedRegex(@"\[(?:/?color(?:=[^\]]+)?)\]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ColorBbCodeRegex();

    [GeneratedRegex(@"\[(?:/?(?:gold|blue|red|green|white|black|purple|orange|yellow|gray|grey))\]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NamedColorBbCodeRegex();

    [GeneratedRegex(@"</?color(?:=[^>]+)?>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ColorXmlRegex();

    [GeneratedRegex(@"\{[A-Za-z0-9_]+\}", RegexOptions.CultureInvariant)]
    private static partial Regex TokenRegex();

    [GeneratedRegex(@"(?:\|\||::|;;)+", RegexOptions.CultureInvariant)]
    private static partial Regex SeparatorRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"([a-z])([A-Z])", RegexOptions.CultureInvariant)]
    private static partial Regex CamelBoundaryRegex();

    [GeneratedRegex(@"^[A-Za-z0-9 ]+$", RegexOptions.CultureInvariant)]
    private static partial Regex MostlyIdentifierRegex();
}
