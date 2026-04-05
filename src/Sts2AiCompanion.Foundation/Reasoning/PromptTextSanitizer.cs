using System;
using System.Text.RegularExpressions;

namespace Sts2AiCompanion.Foundation.Reasoning;

internal static partial class PromptTextSanitizer
{
    public static string? SanitizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var sanitized = ColorBbCodeRegex().Replace(text, string.Empty);
        sanitized = NamedColorBbCodeRegex().Replace(sanitized, string.Empty);
        sanitized = ColorXmlRegex().Replace(sanitized, string.Empty);
        sanitized = ImageBbCodeRegex().Replace(sanitized, string.Empty);
        sanitized = FormattingBbCodeRegex().Replace(sanitized, string.Empty);
        sanitized = TokenRegex().Replace(sanitized, string.Empty);
        sanitized = DynamicPlaceholderRegex().Replace(sanitized, string.Empty);
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

    [GeneratedRegex(@"\[(?:/?color(?:=[^\]]+)?)\]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ColorBbCodeRegex();

    [GeneratedRegex(@"\[(?:/?(?:gold|blue|red|green|white|black|purple|orange|yellow|gray|grey))\]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NamedColorBbCodeRegex();

    [GeneratedRegex(@"</?color(?:=[^>]+)?>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ColorXmlRegex();

    [GeneratedRegex(@"\[img\][^\[]*?\[/img\]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ImageBbCodeRegex();

    [GeneratedRegex(@"\[(?:/?(?:b|i|u|s|center|font_size(?:=[^\]]+)?))\]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex FormattingBbCodeRegex();

    [GeneratedRegex(@"\{[A-Za-z0-9_]+\}", RegexOptions.CultureInvariant)]
    private static partial Regex TokenRegex();

    [GeneratedRegex(@"\{[A-Za-z0-9_]+(?::[^{}]+)?\}", RegexOptions.CultureInvariant)]
    private static partial Regex DynamicPlaceholderRegex();

    [GeneratedRegex(@"(?:\|\||::|;;)+", RegexOptions.CultureInvariant)]
    private static partial Regex SeparatorRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();
}
