using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

internal static class CompactAdvisorBuilderShared
{
    private const int CompactKnowledgeMaxEntries = 6;
    private const int CompactKnowledgeMaxBytes = 4096;

    public static IReadOnlyList<CompactAdvisorOption> BuildVisibleOptions(
        IReadOnlyList<CompanionChoiceItem> sourceItems,
        IReadOnlyList<LiveExportChoiceSummary> snapshotChoices)
    {
        return sourceItems
            .Select(item =>
            {
                var snapshotChoice = MatchSnapshotChoice(snapshotChoices, item);
                return new CompactAdvisorOption(
                    item.Kind,
                    item.Label,
                    item.Value,
                    item.Description,
                    snapshotChoice?.Enabled ?? true,
                    BuildOptionTags(item, snapshotChoice));
            })
            .ToArray();
    }

    public static IReadOnlyList<CompactKnowledgeEntry> FilterKnowledgeEntries(
        KnowledgeSlice boundedSlice,
        IEnumerable<string?> seeds)
    {
        var normalizedSeeds = seeds
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToArray();
        var filtered = new List<CompactKnowledgeEntry>();
        var bytes = 0;
        foreach (var entry in boundedSlice.Entries)
        {
            if (!MatchesAnySeed(entry, normalizedSeeds))
            {
                continue;
            }

            var compact = new CompactKnowledgeEntry(
                entry.Id,
                entry.Name,
                FirstNonBlank(entry.RawText, entry.Options.FirstOrDefault()?.Description));
            var delta = compact.Id.Length + compact.Name.Length + (compact.Summary?.Length ?? 0);
            if (filtered.Count >= CompactKnowledgeMaxEntries || bytes + delta > CompactKnowledgeMaxBytes)
            {
                break;
            }

            filtered.Add(compact);
            bytes += delta;
        }

        return filtered;
    }

    public static IReadOnlyList<CompactKnowledgeEntry> FilterEventKnowledgeEntries(
        KnowledgeSlice boundedSlice,
        IEnumerable<string?> strictSeeds)
    {
        var normalizedSeeds = strictSeeds
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var filtered = new List<CompactKnowledgeEntry>();
        var bytes = 0;
        foreach (var entry in boundedSlice.Entries)
        {
            if (!MatchesAnyEventSeed(entry, normalizedSeeds))
            {
                continue;
            }

            var compact = new CompactKnowledgeEntry(
                entry.Id,
                entry.Name,
                FirstNonBlank(entry.RawText, entry.Options.FirstOrDefault()?.Description));
            var delta = compact.Id.Length + compact.Name.Length + (compact.Summary?.Length ?? 0);
            if (filtered.Count >= CompactKnowledgeMaxEntries || bytes + delta > CompactKnowledgeMaxBytes)
            {
                break;
            }

            filtered.Add(compact);
            bytes += delta;
        }

        return filtered;
    }

    public static IReadOnlyList<CompactRecentEvent> FilterRecentEvents(
        IReadOnlyList<LiveExportEventEnvelope> recentEvents,
        Func<LiveExportEventEnvelope, bool> predicate)
    {
        return recentEvents
            .Where(predicate)
            .TakeLast(3)
            .Select(envelope => new CompactRecentEvent(
                envelope.Kind,
                envelope.Screen,
                envelope.Act,
                envelope.Floor,
                SummarizePayload(envelope.Payload)))
            .ToArray();
    }

    public static CompactRunContext BuildRunContext(CompanionRunState runState)
    {
        return new CompactRunContext(
            runState.Snapshot.Act,
            runState.Snapshot.Floor,
            runState.Snapshot.Player.CurrentHp,
            runState.Snapshot.Player.MaxHp,
            runState.Snapshot.Player.Gold,
            runState.Snapshot.Relics.Count,
            runState.Snapshot.Potions.Count,
            runState.Snapshot.Deck.Count);
    }

    public static CompactPlayerSummary BuildPlayerSummary(CompanionRunState runState)
    {
        var deckCounts = runState.Snapshot.Deck
            .GroupBy(card => string.IsNullOrWhiteSpace(card.Name) ? card.Id ?? "Unknown Card" : card.Name, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .Select(group => $"{group.Key} x{group.Count()}")
            .ToArray();
        var deckSummary = deckCounts.Length == 0
            ? "deck-summary-unavailable"
            : $"{runState.Snapshot.Deck.Count} cards :: {string.Join(", ", deckCounts)}";
        return new CompactPlayerSummary(
            deckSummary,
            runState.Snapshot.Relics.Take(5).ToArray(),
            runState.Snapshot.Potions.Take(3).ToArray());
    }

    public static string? TryGetMeta(CompanionRunState runState, string key)
    {
        return runState.Snapshot.Meta.TryGetValue(key, out var value) ? value : null;
    }

    public static string? FirstNonBlank(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    public static string Normalize(string value)
    {
        return new string(value
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray())
            .Trim('-');
    }

    public static IReadOnlyList<string> BuildOptionTags(CompanionChoiceItem item, LiveExportChoiceSummary? snapshotChoice)
    {
        var tags = new List<string>();
        if (IsSkipText(item.Label) || IsSkipText(item.Value))
        {
            tags.Add("skip-option");
        }

        if (snapshotChoice?.SemanticHints is not null)
        {
            tags.AddRange(snapshotChoice.SemanticHints.Where(hint => !string.IsNullOrWhiteSpace(hint)));
        }

        return tags
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static LiveExportChoiceSummary? MatchSnapshotChoice(
        IReadOnlyList<LiveExportChoiceSummary> snapshotChoices,
        CompanionChoiceItem item)
    {
        return snapshotChoices.FirstOrDefault(choice =>
                   string.Equals(choice.Label, item.Label, StringComparison.Ordinal)
                   && string.Equals(choice.Kind, item.Kind, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(choice.Value ?? string.Empty, item.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase))
               ?? snapshotChoices.FirstOrDefault(choice =>
                   string.Equals(choice.Label, item.Label, StringComparison.Ordinal)
                   && string.Equals(choice.Value ?? string.Empty, item.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase))
               ?? snapshotChoices.FirstOrDefault(choice =>
                   string.Equals(choice.Label, item.Label, StringComparison.Ordinal));
    }

    public static bool IsSkipText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Contains("skip", StringComparison.OrdinalIgnoreCase)
               || value.Contains("넘기기", StringComparison.OrdinalIgnoreCase)
               || value.Contains("건너뛰기", StringComparison.OrdinalIgnoreCase);
    }

    public static string? SummarizePayload(IReadOnlyDictionary<string, object?> payload)
    {
        if (payload.Count == 0)
        {
            return null;
        }

        return string.Join(", ", payload.Take(3).Select(pair => $"{pair.Key}={pair.Value}"));
    }

    private static bool MatchesAnySeed(StaticKnowledgeEntry entry, IReadOnlyList<string> seeds)
    {
        foreach (var seed in seeds)
        {
            if (string.Equals(entry.Id, Normalize(seed), StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Name, seed, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(entry.RawText) && entry.RawText.Contains(seed, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesAnyEventSeed(StaticKnowledgeEntry entry, IReadOnlyList<string> seeds)
    {
        foreach (var seed in seeds)
        {
            if (MatchesEventSeed(entry, seed))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesEventSeed(StaticKnowledgeEntry entry, string seed)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            return false;
        }

        foreach (var candidate in EnumerateEventKnowledgeCandidates(entry))
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            if (string.Equals(candidate, seed, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(Collapse(candidate), Collapse(seed), StringComparison.Ordinal))
            {
                return true;
            }

            if (HasNormalizedIdSuffix(candidate, seed))
            {
                return true;
            }

            if (HasBindingPrefix(candidate, seed))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string?> EnumerateEventKnowledgeCandidates(StaticKnowledgeEntry entry)
    {
        yield return entry.Id;
        yield return entry.Name;

        foreach (var value in entry.Attributes.Values)
        {
            yield return value;
        }

        foreach (var option in entry.Options)
        {
            yield return option.Id;
            yield return option.Label;
            foreach (var value in option.Attributes.Values)
            {
                yield return value;
            }
        }
    }

    private static bool HasNormalizedIdSuffix(string candidate, string seed)
    {
        var collapsedCandidate = Collapse(candidate);
        var collapsedSeed = Collapse(seed);
        if (string.IsNullOrWhiteSpace(collapsedCandidate) || string.IsNullOrWhiteSpace(collapsedSeed))
        {
            return false;
        }

        return collapsedCandidate.Length > collapsedSeed.Length
               && collapsedCandidate.EndsWith(collapsedSeed, StringComparison.Ordinal);
    }

    private static bool HasBindingPrefix(string candidate, string seed)
    {
        var prefix = candidate
            .Split('.', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return false;
        }

        return string.Equals(Collapse(prefix), Collapse(seed), StringComparison.Ordinal);
    }

    private static string Collapse(string value)
    {
        return new string(value
            .Trim()
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
    }
}
