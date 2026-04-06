using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Foundation.Reasoning;

public sealed record RewardOptionDirectFacts(
    RewardOption Option,
    string? SanitizedDescription,
    bool HasUsableDescription,
    bool HasDrawSignal,
    bool HasBlockSignal,
    bool HasEnergySignal,
    bool HasDamageSignal,
    string? ValueSeed,
    string? NormalizedValueSeed,
    string? ClassIdSeed,
    string? L10nKeySeed,
    IReadOnlyList<string> NameSeeds);

public sealed record RewardKnowledgeMatch(
    StaticKnowledgeEntry Entry,
    string MatchKind,
    bool FromBoundedSlice);

public sealed class RewardOptionFactExtractor
{
    public IReadOnlyList<RewardOptionDirectFacts> Extract(RewardOptionSet optionSet)
    {
        ArgumentNullException.ThrowIfNull(optionSet);

        return optionSet.Options
            .Select(Extract)
            .ToArray();
    }

    public RewardOptionDirectFacts Extract(RewardOption option)
    {
        ArgumentNullException.ThrowIfNull(option);

        var sanitizedDescription = PromptTextSanitizer.SanitizeText(option.Description);
        var valueSeed = NormalizeValueSeed(option.Value);
        var classIdSeed = BuildClassKey(option.Value);
        var nameSeeds = BuildNameSeeds(option.Label);

        return new RewardOptionDirectFacts(
            option,
            sanitizedDescription,
            HasUsableDescription(sanitizedDescription),
            HasDrawSignal(sanitizedDescription),
            HasBlockSignal(sanitizedDescription),
            HasEnergySignal(sanitizedDescription),
            HasDamageSignal(sanitizedDescription),
            option.Value,
            valueSeed,
            classIdSeed,
            classIdSeed,
            nameSeeds);
    }

    internal static bool HasDrawSignal(string? text)
    {
        return ContainsAny(text, "draw", "카드를 뽑", "카드를 1장 뽑", "카드를 2장 뽑", "카드를 3장 뽑", "draw a card");
    }

    internal static bool HasBlockSignal(string? text)
    {
        return ContainsAny(text, "block", "방어도");
    }

    internal static bool HasEnergySignal(string? text)
    {
        return ContainsAny(text, "energy", "에너지");
    }

    internal static bool HasDamageSignal(string? text)
    {
        return ContainsAny(text, "damage", "피해");
    }

    internal static bool HasUsableDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return false;
        }

        return !description.Contains("설명이 비어", StringComparison.OrdinalIgnoreCase)
               && !description.Contains("추가 정보 없음", StringComparison.OrdinalIgnoreCase)
               && !description.Equals("unknown", StringComparison.OrdinalIgnoreCase)
               && !description.Equals("none", StringComparison.OrdinalIgnoreCase);
    }

    internal static string NormalizeLookup(string value)
    {
        return new string(value
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray())
            .Trim('-');
    }

    internal static string? NormalizeValueSeed(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var stripped = StripCardPrefix(value);
        return NormalizeLookup(stripped ?? value);
    }

    internal static string? BuildClassKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var stripped = StripCardPrefix(value);
        if (string.IsNullOrWhiteSpace(stripped))
        {
            return null;
        }

        var builder = new char[stripped.Length];
        for (var i = 0; i < stripped.Length; i++)
        {
            var character = stripped[i];
            builder[i] = char.IsLetterOrDigit(character) ? char.ToUpperInvariant(character) : '_';
        }

        return new string(builder).Trim('_');
    }

    private static string? StripCardPrefix(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith("CARD.", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed["CARD.".Length..];
        }

        var lastDot = trimmed.LastIndexOf('.');
        return lastDot >= 0 && lastDot + 1 < trimmed.Length
            ? trimmed[(lastDot + 1)..]
            : trimmed;
    }

    private static IReadOnlyList<string> BuildNameSeeds(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return Array.Empty<string>();
        }

        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            label.Trim(),
        };
        var sanitized = PromptTextSanitizer.SanitizeText(label);
        if (!string.IsNullOrWhiteSpace(sanitized))
        {
            values.Add(sanitized);
        }

        return values.ToArray();
    }

    private static bool ContainsAny(string? rawText, params string[] markers)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return false;
        }

        return markers.Any(marker => rawText.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class RewardExactKnowledgeResolver
{
    public RewardKnowledgeMatch? Resolve(
        RewardOptionDirectFacts directFacts,
        KnowledgeSlice boundedSlice,
        StaticKnowledgeCatalog? catalog = null)
    {
        ArgumentNullException.ThrowIfNull(directFacts);
        ArgumentNullException.ThrowIfNull(boundedSlice);

        return ResolveCore(
            boundedSlice.Entries,
            EnumerateCatalogEntries(catalog, boundedSlice.Entries),
            directFacts.ValueSeed,
            directFacts.NormalizedValueSeed,
            directFacts.ClassIdSeed,
            directFacts.L10nKeySeed,
            directFacts.NameSeeds);
    }

    public RewardKnowledgeMatch? Resolve(
        LiveExportCardSummary card,
        KnowledgeSlice boundedSlice,
        StaticKnowledgeCatalog? catalog = null)
    {
        ArgumentNullException.ThrowIfNull(card);
        ArgumentNullException.ThrowIfNull(boundedSlice);

        var nameSeeds = string.IsNullOrWhiteSpace(card.Name)
            ? Array.Empty<string>()
            : new[] { card.Name.Trim() };
        var classIdSeed = RewardOptionFactExtractor.BuildClassKey(card.Id);
        return ResolveCore(
            boundedSlice.Entries,
            EnumerateCatalogEntries(catalog, boundedSlice.Entries),
            card.Id,
            RewardOptionFactExtractor.NormalizeValueSeed(card.Id),
            classIdSeed,
            classIdSeed,
            nameSeeds);
    }

    private static RewardKnowledgeMatch? ResolveCore(
        IReadOnlyList<StaticKnowledgeEntry> sliceEntries,
        IReadOnlyList<StaticKnowledgeEntry> catalogEntries,
        string? rawValueSeed,
        string? normalizedValueSeed,
        string? classIdSeed,
        string? l10nKeySeed,
        IReadOnlyList<string> nameSeeds)
    {
        return FindByExactId(sliceEntries, rawValueSeed, true)
               ?? FindByExactId(catalogEntries, rawValueSeed, false)
               ?? FindByNormalizedEntryId(sliceEntries, normalizedValueSeed, true)
               ?? FindByNormalizedEntryId(catalogEntries, normalizedValueSeed, false)
               ?? FindByAttribute(sliceEntries, "classId", classIdSeed, true)
               ?? FindByAttribute(catalogEntries, "classId", classIdSeed, false)
               ?? FindByAttribute(sliceEntries, "l10nKey", l10nKeySeed, true)
               ?? FindByAttribute(catalogEntries, "l10nKey", l10nKeySeed, false)
               ?? FindByName(sliceEntries, nameSeeds, true)
               ?? FindByName(catalogEntries, nameSeeds, false);
    }

    private static RewardKnowledgeMatch? FindByExactId(
        IReadOnlyList<StaticKnowledgeEntry> entries,
        string? seed,
        bool fromBoundedSlice)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            return null;
        }

        var match = entries.FirstOrDefault(entry => string.Equals(entry.Id, seed, StringComparison.OrdinalIgnoreCase));
        return match is null ? null : new RewardKnowledgeMatch(match, "entry-id", fromBoundedSlice);
    }

    private static RewardKnowledgeMatch? FindByNormalizedEntryId(
        IReadOnlyList<StaticKnowledgeEntry> entries,
        string? seed,
        bool fromBoundedSlice)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            return null;
        }

        var match = entries.FirstOrDefault(entry =>
            string.Equals(RewardOptionFactExtractor.NormalizeLookup(entry.Id), seed, StringComparison.OrdinalIgnoreCase));
        return match is null ? null : new RewardKnowledgeMatch(match, "normalized-entry-id", fromBoundedSlice);
    }

    private static RewardKnowledgeMatch? FindByAttribute(
        IReadOnlyList<StaticKnowledgeEntry> entries,
        string key,
        string? seed,
        bool fromBoundedSlice)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            return null;
        }

        var match = entries.FirstOrDefault(entry =>
            entry.Attributes.TryGetValue(key, out var value)
            && string.Equals(value, seed, StringComparison.OrdinalIgnoreCase));
        return match is null ? null : new RewardKnowledgeMatch(match, key, fromBoundedSlice);
    }

    private static RewardKnowledgeMatch? FindByName(
        IReadOnlyList<StaticKnowledgeEntry> entries,
        IReadOnlyList<string> seeds,
        bool fromBoundedSlice)
    {
        foreach (var seed in seeds.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            var normalizedSeed = RewardOptionFactExtractor.NormalizeLookup(seed);
            var match = entries.FirstOrDefault(entry =>
                string.Equals(entry.Name, seed, StringComparison.OrdinalIgnoreCase)
                || MatchesNamedAttribute(entry, "title", seed)
                || MatchesNamedAttribute(entry, "localizedTitleRaw", seed)
                || string.Equals(RewardOptionFactExtractor.NormalizeLookup(entry.Name), normalizedSeed, StringComparison.OrdinalIgnoreCase)
                || MatchesNormalizedNamedAttribute(entry, "title", normalizedSeed)
                || MatchesNormalizedNamedAttribute(entry, "localizedTitleRaw", normalizedSeed));
            if (match is not null)
            {
                return new RewardKnowledgeMatch(match, "name", fromBoundedSlice);
            }
        }

        return null;
    }

    private static bool MatchesNamedAttribute(StaticKnowledgeEntry entry, string key, string seed)
    {
        return entry.Attributes.TryGetValue(key, out var value)
               && string.Equals(value, seed, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesNormalizedNamedAttribute(StaticKnowledgeEntry entry, string key, string normalizedSeed)
    {
        return entry.Attributes.TryGetValue(key, out var value)
               && !string.IsNullOrWhiteSpace(value)
               && string.Equals(RewardOptionFactExtractor.NormalizeLookup(value), normalizedSeed, StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<StaticKnowledgeEntry> EnumerateCatalogEntries(
        StaticKnowledgeCatalog? catalog,
        IReadOnlyList<StaticKnowledgeEntry> sliceEntries)
    {
        if (catalog is null)
        {
            return Array.Empty<StaticKnowledgeEntry>();
        }

        var seenIds = sliceEntries
            .Select(entry => entry.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return catalog.Cards
            .Where(entry => seenIds.Add(entry.Id))
            .ToArray();
    }
}
