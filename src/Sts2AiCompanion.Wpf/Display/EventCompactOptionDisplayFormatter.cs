using System;
using System.Collections.Generic;
using System.Linq;
using Sts2AiCompanion.AdvisorSceneDisplay;
using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Wpf.Display;

internal static class EventCompactOptionDisplayFormatter
{
    public static string Format(RewardEventCompactAdvisorInput compact)
    {
        if (compact.EventFacts is null || compact.VisibleOptions.Count == 0)
        {
            return "없음";
        }

        var factMap = compact.EventFacts.OptionFacts
            .ToDictionary(static fact => fact.Label, StringComparer.Ordinal);
        var lines = new List<string>();
        foreach (var option in compact.VisibleOptions)
        {
            factMap.TryGetValue(option.Label, out var fact);
            lines.Add(FormatOptionLine(option, fact));
        }

        return lines.Count == 0 ? "없음" : string.Join(Environment.NewLine, lines);
    }

    private static string FormatOptionLine(
        CompactAdvisorOption option,
        EventCompactOptionFact? fact)
    {
        var label = AdvisorDisplaySanitizer.SanitizeText(option.Label) ?? option.Label;
        var description = BuildDescription(option, fact);
        var status = option.Enabled ? "활성" : "비활성";

        return string.IsNullOrWhiteSpace(description)
            ? $"- {label} [{status}]"
            : $"- {label} [{status}] :: {description}";
    }

    private static string? BuildDescription(
        CompactAdvisorOption option,
        EventCompactOptionFact? fact)
    {
        var segments = new List<string>();
        var baseDescription = AdvisorDisplaySanitizer.SanitizeText(option.Description);
        if (!string.IsNullOrWhiteSpace(baseDescription))
        {
            segments.Add(baseDescription);
        }

        if (fact is not null)
        {
            foreach (var effect in fact.Effects)
            {
                var text = AdvisorDisplaySanitizer.SanitizeText(effect.Text);
                if (string.IsNullOrWhiteSpace(text)
                    || segments.Contains(text, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                segments.Add(text);
            }
        }

        return segments.Count == 0 ? null : string.Join(" ", segments);
    }
}
