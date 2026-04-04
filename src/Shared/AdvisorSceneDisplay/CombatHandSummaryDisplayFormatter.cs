using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sts2AiCompanion.AdvisorSceneDisplay;

public static class CombatHandSummaryDisplayFormatter
{
    public static string? Format(string? summary, AdvisorKnowledgeDisplayResolver resolver)
    {
        var cards = Parse(summary);
        if (cards.Count == 0)
        {
            return AdvisorDisplaySanitizer.SanitizeText(summary);
        }

        return string.Join(
            ", ",
            cards.Select(card =>
            {
                var title = resolver.ResolveDisplayText(card.RawName);
                var metadata = new List<string>();
                var type = LocalizeCardType(card.Type);
                if (!string.IsNullOrWhiteSpace(type))
                {
                    metadata.Add(type!);
                }

                if (card.Cost is not null)
                {
                    metadata.Add($"비용 {card.Cost.Value.ToString(CultureInfo.InvariantCulture)}");
                }

                return metadata.Count == 0
                    ? $"{card.SlotIndex}. {title}"
                    : $"{card.SlotIndex}. {title} ({string.Join(", ", metadata)})";
            }));
    }

    private static IReadOnlyList<CombatHandSummaryItem> Parse(string? summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return Array.Empty<CombatHandSummaryItem>();
        }

        var cards = new List<CombatHandSummaryItem>();
        foreach (var part in summary.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var colonIndex = part.IndexOf(':');
            if (colonIndex <= 0
                || !int.TryParse(part[..colonIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out var slotIndex))
            {
                continue;
            }

            var payload = part[(colonIndex + 1)..].Split('|', StringSplitOptions.None);
            if (payload.Length == 0 || string.IsNullOrWhiteSpace(payload[0]))
            {
                continue;
            }

            int? cost = payload.Length >= 3 && int.TryParse(payload[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedCost)
                ? parsedCost
                : null;
            cards.Add(new CombatHandSummaryItem(
                slotIndex,
                payload[0],
                payload.Length >= 2 ? payload[1] : null,
                cost));
        }

        return cards;
    }

    private static string? LocalizeCardType(string? rawType)
    {
        var normalized = AdvisorDisplaySanitizer.SanitizeText(rawType);
        return normalized?.Trim().ToLowerInvariant() switch
        {
            null or "" or "unknown" => null,
            "attack" => "공격",
            "skill" => "스킬",
            "power" => "파워",
            "status" => "상태",
            "curse" => "저주",
            _ => AdvisorDisplaySanitizer.PrettifyIdentifier(normalized) ?? normalized,
        };
    }

    private sealed record CombatHandSummaryItem(
        int SlotIndex,
        string RawName,
        string? Type,
        int? Cost);
}
