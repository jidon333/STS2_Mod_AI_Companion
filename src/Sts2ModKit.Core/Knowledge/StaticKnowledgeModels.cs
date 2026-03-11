namespace Sts2ModKit.Core.Knowledge;

public sealed record StaticKnowledgeOption(
    string Id,
    string Label,
    string? Description,
    IReadOnlyDictionary<string, string?> Attributes);

public sealed record StaticKnowledgeEntry(
    string Id,
    string Name,
    string Source,
    bool Observed,
    string? RawText,
    IReadOnlyList<string> Tags,
    IReadOnlyDictionary<string, string?> Attributes,
    IReadOnlyList<StaticKnowledgeOption> Options);

public sealed record StaticKnowledgeSourceFile(
    string Kind,
    string Path,
    bool Exists,
    long? Size,
    DateTimeOffset? LastWriteTime);

public sealed record StaticKnowledgePipelineStep(
    string Name,
    string Status,
    string? OutputPath,
    IReadOnlyDictionary<string, string?> Stats,
    IReadOnlyList<string> Warnings);

public sealed record StaticKnowledgeLocalizationCardEntry(
    string KeyStem,
    string PreferredLocale,
    string? Title,
    string? Description,
    string? SelectionScreenPrompt,
    string? EnglishTitle,
    string? EnglishDescription,
    IReadOnlyList<string> SourceFileHints,
    IReadOnlyList<string> Locales);

public sealed record StaticKnowledgeLocalizationEntry(
    string Domain,
    string KeyStem,
    string PreferredLocale,
    string? Title,
    string? Description,
    string? Flavor,
    string? SelectionScreenPrompt,
    string? EnglishTitle,
    string? EnglishDescription,
    IReadOnlyList<StaticKnowledgeOption> Options,
    IReadOnlyDictionary<string, string?> Attributes,
    IReadOnlyList<string> SourceFileHints,
    IReadOnlyList<string> Locales);

public sealed record StaticKnowledgeLocalizationScan(
    DateTimeOffset GeneratedAt,
    string SourcePath,
    IReadOnlyList<StaticKnowledgeLocalizationCardEntry> Cards,
    IReadOnlyList<StaticKnowledgeLocalizationEntry> Relics,
    IReadOnlyList<StaticKnowledgeLocalizationEntry> Potions,
    IReadOnlyList<StaticKnowledgeLocalizationEntry> Events,
    IReadOnlyList<StaticKnowledgeLocalizationEntry> Shops,
    IReadOnlyList<StaticKnowledgeLocalizationEntry> Rewards,
    IReadOnlyList<StaticKnowledgeLocalizationEntry> Keywords,
    IReadOnlyDictionary<string, string?> Stats,
    IReadOnlyList<string> Warnings);

public sealed record StaticKnowledgeMetadata(
    string? ReleaseVersion,
    string? ReleaseCommit,
    DateTimeOffset? ReleaseDate,
    IReadOnlyDictionary<string, string?> Stats);

public sealed record StaticKnowledgeSourceManifest(
    DateTimeOffset GeneratedAt,
    string KnowledgeRoot,
    StaticKnowledgeMetadata Metadata,
    IReadOnlyList<StaticKnowledgeSourceFile> Sources,
    IReadOnlyList<StaticKnowledgePipelineStep> Steps,
    IReadOnlyList<string> Warnings);

public sealed record StaticKnowledgeCatalog(
    DateTimeOffset GeneratedAt,
    StaticKnowledgeMetadata Metadata,
    IReadOnlyList<StaticKnowledgeEntry> Cards,
    IReadOnlyList<StaticKnowledgeEntry> Relics,
    IReadOnlyList<StaticKnowledgeEntry> Potions,
    IReadOnlyList<StaticKnowledgeEntry> Events,
    IReadOnlyList<StaticKnowledgeEntry> Shops,
    IReadOnlyList<StaticKnowledgeEntry> Rewards,
    IReadOnlyList<StaticKnowledgeEntry> Keywords)
{
    public static StaticKnowledgeCatalog CreateEmpty(StaticKnowledgeMetadata? metadata = null)
    {
        return new StaticKnowledgeCatalog(
            DateTimeOffset.UtcNow,
            metadata ?? new StaticKnowledgeMetadata(null, null, null, new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>());
    }
}
