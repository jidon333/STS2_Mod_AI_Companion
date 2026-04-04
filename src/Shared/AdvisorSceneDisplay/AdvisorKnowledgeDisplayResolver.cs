using System;
using System.Collections.Generic;
using System.Linq;
using Sts2AiCompanion.AdvisorSceneModel;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.AdvisorSceneDisplay;

public sealed class AdvisorKnowledgeDisplayResolver
{
    private readonly StaticKnowledgeCatalog _catalog;
    private readonly IReadOnlyList<StaticKnowledgeEntry> _allEntries;

    public AdvisorKnowledgeDisplayResolver(StaticKnowledgeCatalog? catalog)
    {
        _catalog = catalog ?? StaticKnowledgeCatalog.CreateEmpty();
        _allEntries = _catalog.Cards
            .Concat(_catalog.Relics)
            .Concat(_catalog.Potions)
            .Concat(_catalog.Events)
            .Concat(_catalog.Shops)
            .Concat(_catalog.Rewards)
            .Concat(_catalog.Keywords)
            .ToArray();
    }

    public AdvisorDisplayResolvedText ResolveCard(LiveExportCardSummary card)
    {
        var match = FindBestMatch(_catalog.Cards, card.Id, card.Name);
        return BuildResolvedText(card.Name, card.Id, null, match);
    }

    public AdvisorDisplayResolvedText ResolveRelic(string? raw)
    {
        var match = FindBestMatch(_catalog.Relics, raw);
        return BuildResolvedText(raw, raw, null, match);
    }

    public AdvisorDisplayResolvedText ResolvePotion(string? raw)
    {
        var match = FindBestMatch(_catalog.Potions, raw);
        return BuildResolvedText(raw, raw, null, match);
    }

    public AdvisorDisplayResolvedText ResolveSceneOption(AdvisorSceneOption option)
    {
        var entries = SelectCandidateEntries(option);
        var match = FindBestMatch(entries, option.Value, option.Label, option.Description);
        return BuildResolvedText(option.Label, option.Value, option.Description, match);
    }

    public string ResolveDisplayText(string? primary, string? fallback = null)
    {
        return ResolveLabel(primary, fallback);
    }

    public string ResolveLabel(string? primary, string? fallback = null)
    {
        var match = FindBestMatch(_allEntries, primary, fallback);
        if (match is not null)
        {
            return BuildResolvedText(primary, fallback, null, match).Title;
        }

        var primarySanitized = AdvisorDisplaySanitizer.SanitizeText(primary);
        var fallbackSanitized = AdvisorDisplaySanitizer.SanitizeText(fallback);
        var primaryPretty = AdvisorDisplaySanitizer.PrettifyIdentifier(primary);
        var fallbackPretty = AdvisorDisplaySanitizer.PrettifyIdentifier(fallback);
        var candidate = fallbackSanitized
                        ?? (LooksLikeRawIdentifier(primarySanitized) ? primaryPretty : primarySanitized)
                        ?? primaryPretty
                        ?? fallbackPretty;
        return string.IsNullOrWhiteSpace(candidate) ? "없음" : candidate;
    }

    public string? ResolveDescription(string? candidateDescription, params string?[] fallbackSeeds)
    {
        if (!AdvisorDisplaySanitizer.IsPlaceholderDescription(candidateDescription))
        {
            return AdvisorDisplaySanitizer.SanitizeText(candidateDescription);
        }

        var match = FindBestMatch(_allEntries, fallbackSeeds.Append(candidateDescription).ToArray());
        return GetEntryDescription(match);
    }

    private AdvisorDisplayResolvedText BuildResolvedText(
        string? rawLabel,
        string? rawValue,
        string? rawDescription,
        StaticKnowledgeEntry? match)
    {
        var matchedTitle = AdvisorDisplaySanitizer.SanitizeText(match?.Name);
        var title = matchedTitle
                    ?? AdvisorDisplaySanitizer.SanitizeText(rawLabel)
                    ?? AdvisorDisplaySanitizer.PrettifyIdentifier(rawValue)
                    ?? "Unknown";
        var knowledgeDescription = GetEntryDescription(match);
        var description = !AdvisorDisplaySanitizer.IsPlaceholderDescription(rawDescription, title)
            ? AdvisorDisplaySanitizer.SanitizeText(rawDescription)
            : knowledgeDescription;
        if (AdvisorDisplaySanitizer.IsPlaceholderDescription(description, title))
        {
            description = null;
        }

        return new AdvisorDisplayResolvedText(
            title,
            description,
            match?.Id,
            match is not null && (!string.Equals(title, rawLabel, StringComparison.OrdinalIgnoreCase) || !string.Equals(description, rawDescription, StringComparison.OrdinalIgnoreCase)));
    }

    private IReadOnlyList<StaticKnowledgeEntry> SelectCandidateEntries(AdvisorSceneOption option)
    {
        if (LooksLikeCard(option))
        {
            return _catalog.Cards;
        }

        if (LooksLikeRelic(option))
        {
            return _catalog.Relics;
        }

        if (LooksLikePotion(option))
        {
            return _catalog.Potions;
        }

        if (LooksLikeEvent(option))
        {
            return _catalog.Events;
        }

        if (LooksLikeShop(option))
        {
            return _catalog.Shops;
        }

        if (LooksLikeReward(option))
        {
            return _catalog.Rewards;
        }

        return _catalog.Cards
            .Concat(_catalog.Relics)
            .Concat(_catalog.Potions)
            .Concat(_catalog.Events)
            .Concat(_catalog.Shops)
            .Concat(_catalog.Rewards)
            .ToArray();
    }

    private static StaticKnowledgeEntry? FindBestMatch(IEnumerable<StaticKnowledgeEntry> entries, params string?[] seeds)
    {
        var normalizedSeeds = ExpandSeeds(seeds)
            .Where(static seed => !string.IsNullOrWhiteSpace(seed))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (normalizedSeeds.Length == 0)
        {
            return null;
        }

        foreach (var seed in normalizedSeeds)
        {
            var normalized = Normalize(seed!);
            var exactId = entries.FirstOrDefault(entry => string.Equals(entry.Id, normalized, StringComparison.OrdinalIgnoreCase));
            if (exactId is not null)
            {
                return exactId;
            }
        }

        foreach (var seed in normalizedSeeds)
        {
            var exactAttribute = entries.FirstOrDefault(entry => EntryExactKeys(entry)
                .Any(key => string.Equals(key, seed, StringComparison.OrdinalIgnoreCase)));
            if (exactAttribute is not null)
            {
                return exactAttribute;
            }
        }

        foreach (var seed in normalizedSeeds)
        {
            var exactName = entries.FirstOrDefault(entry => string.Equals(entry.Name, seed, StringComparison.OrdinalIgnoreCase));
            if (exactName is not null)
            {
                return exactName;
            }
        }

        return entries
            .Select(entry => new
            {
                Entry = entry,
                Score = normalizedSeeds.Max(seed => ScoreEntry(entry, seed!)),
            })
            .Where(static candidate => candidate.Score > 0)
            .OrderByDescending(static candidate => candidate.Score)
            .ThenBy(candidate => candidate.Entry.Name, StringComparer.OrdinalIgnoreCase)
            .Select(static candidate => candidate.Entry)
            .FirstOrDefault();
    }

    private static int ScoreEntry(StaticKnowledgeEntry entry, string seed)
    {
        var score = 0;
        if (entry.Name.Contains(seed, StringComparison.OrdinalIgnoreCase))
        {
            score += 6;
        }

        if (!string.IsNullOrWhiteSpace(entry.RawText) && entry.RawText.Contains(seed, StringComparison.OrdinalIgnoreCase))
        {
            score += 3;
        }

        if (entry.Attributes.Values.Any(value => !string.IsNullOrWhiteSpace(value) && value.Contains(seed, StringComparison.OrdinalIgnoreCase)))
        {
            score += 2;
        }

        if (EntryExactKeys(entry).Any(key => string.Equals(key, seed, StringComparison.OrdinalIgnoreCase)))
        {
            score += 8;
        }

        var prettySeed = AdvisorDisplaySanitizer.PrettifyIdentifier(seed);
        if (!string.IsNullOrWhiteSpace(prettySeed)
            && string.Equals(entry.Name, prettySeed, StringComparison.OrdinalIgnoreCase))
        {
            score += 4;
        }

        return score;
    }

    private static string? GetEntryDescription(StaticKnowledgeEntry? match)
    {
        if (match is null)
        {
            return null;
        }

        if (!AdvisorDisplaySanitizer.IsPlaceholderDescription(match.RawText, match.Name))
        {
            return AdvisorDisplaySanitizer.SanitizeText(match.RawText);
        }

        if (match.Attributes.TryGetValue("description", out var description)
            && !AdvisorDisplaySanitizer.IsPlaceholderDescription(description, match.Name))
        {
            return AdvisorDisplaySanitizer.SanitizeText(description);
        }

        if (match.Attributes.TryGetValue("englishDescription", out var englishDescription)
            && !AdvisorDisplaySanitizer.IsPlaceholderDescription(englishDescription, match.Name))
        {
            return AdvisorDisplaySanitizer.SanitizeText(englishDescription);
        }

        return null;
    }

    private static bool LooksLikeCard(AdvisorSceneOption option)
    {
        return option.Kind.Contains("card", StringComparison.OrdinalIgnoreCase)
               || option.Tags.Any(static tag => tag.Contains("reward-card", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeRelic(AdvisorSceneOption option)
    {
        return option.Kind.Contains("relic", StringComparison.OrdinalIgnoreCase)
               || option.Tags.Any(static tag => tag.Contains("reward-relic", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikePotion(AdvisorSceneOption option)
    {
        return option.Kind.Contains("potion", StringComparison.OrdinalIgnoreCase)
               || option.Tags.Any(static tag => tag.Contains("reward-potion", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeEvent(AdvisorSceneOption option)
    {
        return option.Kind.Contains("event", StringComparison.OrdinalIgnoreCase)
               || option.Tags.Any(static tag => tag.Contains("scene:event", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeShop(AdvisorSceneOption option)
    {
        return option.Kind.Contains("shop", StringComparison.OrdinalIgnoreCase)
               || option.Tags.Any(static tag => tag.Contains("scene:shop", StringComparison.OrdinalIgnoreCase)
                                                || tag.Contains("binding-kind:shop-room", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeReward(AdvisorSceneOption option)
    {
        return option.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || option.Tags.Any(static tag => tag.Contains("reward", StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string value)
    {
        return new string(value
            .ToLowerInvariant()
            .Select(static character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray())
            .Trim('-');
    }

    private static IEnumerable<string> ExpandSeeds(IEnumerable<string?> seeds)
    {
        foreach (var seed in seeds)
        {
            if (string.IsNullOrWhiteSpace(seed))
            {
                continue;
            }

            var trimmed = seed.Trim();
            yield return trimmed;

            var sanitized = AdvisorDisplaySanitizer.SanitizeText(trimmed);
            if (!string.IsNullOrWhiteSpace(sanitized))
            {
                yield return sanitized!;
            }

            var pretty = AdvisorDisplaySanitizer.PrettifyIdentifier(trimmed);
            if (!string.IsNullOrWhiteSpace(pretty))
            {
                yield return pretty!;
            }

            var stripped = StripDomainPrefix(trimmed);
            if (!string.IsNullOrWhiteSpace(stripped)
                && !string.Equals(stripped, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                yield return stripped!;
                yield return stripped.Replace('_', ' ');
            }
        }
    }

    private static IEnumerable<string> EntryExactKeys(StaticKnowledgeEntry entry)
    {
        yield return Normalize(entry.Id);

        if (!string.IsNullOrWhiteSpace(entry.Name))
        {
            yield return entry.Name;
        }

        foreach (var attributeKey in new[] { "classId", "l10nKey", "title", "localizedTitleRaw", "englishTitle", "className" })
        {
            if (entry.Attributes.TryGetValue(attributeKey, out var attributeValue)
                && !string.IsNullOrWhiteSpace(attributeValue))
            {
                yield return attributeValue!;
                yield return Normalize(attributeValue!);
            }
        }
    }

    private static string StripDomainPrefix(string value)
    {
        foreach (var prefix in new[] { "CARD.", "RELIC.", "POTION.", "EVENT.", "KEYWORD." })
        {
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return value[prefix.Length..];
            }
        }

        return value;
    }

    private static bool LooksLikeRawIdentifier(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
               && (value.Contains('_', StringComparison.Ordinal)
                   || value.Contains('.', StringComparison.Ordinal)
                   || value.Contains(':', StringComparison.Ordinal)
                   || value.Contains('/', StringComparison.Ordinal)
                   || value.Contains('\\', StringComparison.Ordinal));
    }
}

public sealed record AdvisorDisplayResolvedText(
    string Title,
    string? Description,
    string? KnowledgeId,
    bool UsedKnowledge);
