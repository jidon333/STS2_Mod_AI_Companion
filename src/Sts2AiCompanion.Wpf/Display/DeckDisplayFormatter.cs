using System;
using System.Collections.Generic;
using System.Linq;
using Sts2AiCompanion.AdvisorSceneDisplay;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Wpf.Display;

internal static class DeckDisplayFormatter
{
    public static string FormatDeck(IReadOnlyList<LiveExportCardSummary> deck, AdvisorKnowledgeDisplayResolver resolver)
    {
        if (deck.Count == 0)
        {
            return "덱 정보를 아직 읽지 못했습니다.";
        }

        var lines = new List<string>
        {
            $"총 카드 수: {deck.Count}",
            string.Empty,
        };

        lines.AddRange(
            deck.GroupBy(card => resolver.ResolveDisplayText(card.Id, card.Name), StringComparer.OrdinalIgnoreCase)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .Select(FormatDeckGroup));

        return string.Join(Environment.NewLine, lines);
    }

    public static string FormatNamedListSection(string title, IReadOnlyList<string> items, AdvisorKnowledgeDisplayResolver resolver)
    {
        var lines = new List<string> { title };

        if (items.Count == 0)
        {
            lines.Add("- 없음");
            return string.Join(Environment.NewLine, lines);
        }

        lines.AddRange(items.Select(item => $"- {resolver.ResolveDisplayText(item)}"));
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatDeckGroup(IGrouping<string, LiveExportCardSummary> group)
    {
        var count = group.Count();
        var line = $"- {group.Key} x {count}";

        var costs = group.Select(card => card.Cost).Distinct().ToArray();
        if (costs.Length == 1 && costs[0] is not null)
        {
            line += $" / cost {costs[0]}";
        }

        var types = group
            .Select(card => AdvisorDisplaySanitizer.Sanitize(card.Type))
            .Where(type => !string.IsNullOrWhiteSpace(type))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (types.Length == 1)
        {
            line += $" / {types[0]}";
        }

        if (group.All(card => card.Upgraded == true))
        {
            line += " / 강화";
        }
        else if (group.Any(card => card.Upgraded == true))
        {
            line += " / 일부 강화";
        }

        return line;
    }
}
