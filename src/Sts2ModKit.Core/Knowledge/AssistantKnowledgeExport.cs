using System.Text.Json;

namespace Sts2ModKit.Core.Knowledge;

public static class AssistantKnowledgeExportWriter
{
    public static IReadOnlyList<string> Write(string assistantRoot, StaticKnowledgeCatalog catalog)
    {
        Directory.CreateDirectory(assistantRoot);

        var cards = ProjectEntries(catalog.Cards, "card");
        var relics = ProjectEntries(catalog.Relics, "relic");
        var potions = ProjectEntries(catalog.Potions, "potion");
        var events = ProjectEntries(catalog.Events, "event");
        var shops = ProjectEntries(catalog.Shops, "shop");
        var rewards = ProjectEntries(catalog.Rewards, "reward");
        var keywords = ProjectEntries(catalog.Keywords, "keyword");

        var created = new List<string>
        {
            WriteJson(Path.Combine(assistantRoot, "cards.json"), cards),
            WriteJson(Path.Combine(assistantRoot, "relics.json"), relics),
            WriteJson(Path.Combine(assistantRoot, "potions.json"), potions),
            WriteJson(Path.Combine(assistantRoot, "events.json"), events),
            WriteJson(Path.Combine(assistantRoot, "shops.json"), shops),
            WriteJson(Path.Combine(assistantRoot, "rewards.json"), rewards),
            WriteJson(Path.Combine(assistantRoot, "keywords.json"), keywords),
            WriteJson(
                Path.Combine(assistantRoot, "index.json"),
                new
                {
                    generatedAt = catalog.GeneratedAt,
                    metadata = new
                    {
                        releaseVersion = catalog.Metadata.ReleaseVersion,
                        releaseCommit = catalog.Metadata.ReleaseCommit,
                        releaseDate = catalog.Metadata.ReleaseDate,
                        primaryLocale = "kor",
                        description = "AI companion-oriented static knowledge exports with localized titles, descriptions, options, and structural hints.",
                    },
                    counts = new
                    {
                        cards = cards.Count,
                        relics = relics.Count,
                        potions = potions.Count,
                        events = events.Count,
                        shops = shops.Count,
                        rewards = rewards.Count,
                        keywords = keywords.Count,
                    },
                    files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["cards"] = "cards.json",
                        ["relics"] = "relics.json",
                        ["potions"] = "potions.json",
                        ["events"] = "events.json",
                        ["shops"] = "shops.json",
                        ["rewards"] = "rewards.json",
                        ["keywords"] = "keywords.json",
                    },
                    provenance = new
                    {
                        primarySources = new[]
                        {
                            "release-scan",
                            "decompile-scan",
                            "strict-domain-parse",
                            "assembly-scan",
                            "pck-inventory",
                            "localization-scan",
                            "observed-merge",
                        },
                        externalCrossCheckHints = new[]
                        {
                            "https://spire-codex.com/",
                        },
                        notes = new[]
                        {
                            "Use localized description fields first when reasoning about cards, relics, events, shops, rewards, and keywords.",
                            "Use modelClass/resourcePath/sourceFileHint as structural backup when localized text is incomplete.",
                            "Cross-check live currentChoices and live state against these static exports before making advice.",
                        },
                    },
                })
        };

        return created;
    }

    private static IReadOnlyList<object> ProjectEntries(IReadOnlyList<StaticKnowledgeEntry> entries, string domain)
    {
        return entries
            .Select(entry => new
            {
                id = entry.Id,
                domain,
                name = entry.Name,
                observed = entry.Observed,
                source = entry.Source,
                tags = entry.Tags,
                title = ReadAttribute(entry, "title") ?? entry.Name,
                description = ResolveDescription(entry),
                selectionScreenPrompt = ReadAttribute(entry, "selectionScreenPrompt"),
                englishTitle = ReadAttribute(entry, "englishTitle"),
                englishDescription = ReadAttribute(entry, "englishDescription"),
                preferredLocale = ReadAttribute(entry, "preferredLocale"),
                l10nKey = ReadAttribute(entry, "l10nKey"),
                modelClass = ReadAttribute(entry, "fullName"),
                className = ReadAttribute(entry, "className"),
                classId = ReadAttribute(entry, "classId"),
                strictDomain = ReadAttribute(entry, "strictDomain"),
                strictModel = ReadAttribute(entry, "strictModel"),
                matchKeys = ReadAttribute(entry, "matchKeys"),
                pool = ReadAttribute(entry, "pool"),
                color = ReadAttribute(entry, "color"),
                cost = ReadAttribute(entry, "cost"),
                type = ReadAttribute(entry, "type"),
                rarity = ReadAttribute(entry, "rarity"),
                target = ReadAttribute(entry, "target"),
                usage = ReadAttribute(entry, "usage"),
                summary = ReadAttribute(entry, "summary"),
                shopKind = ReadAttribute(entry, "shopKind"),
                priceRule = ReadAttribute(entry, "priceRule"),
                roomType = ReadAttribute(entry, "roomType"),
                talkPrefix = ReadAttribute(entry, "talkPrefix"),
                rewardType = ReadAttribute(entry, "rewardType"),
                rewardsSetIndex = ReadAttribute(entry, "rewardsSetIndex"),
                keywordKind = ReadAttribute(entry, "keywordKind"),
                powerType = ReadAttribute(entry, "powerType"),
                stackType = ReadAttribute(entry, "stackType"),
                intentType = ReadAttribute(entry, "intentType"),
                dynamicVars = ReadAttribute(entry, "dynamicVarsJson"),
                upgradeSummary = ReadAttribute(entry, "upgradeSummary"),
                resourcePath = ReadAttribute(entry, "resourcePath"),
                sourceFileHint = ReadAttribute(entry, "sourceFileHint"),
                sourceLocales = ReadAttribute(entry, "sourceLocales"),
                notePreview = ReadAttribute(entry, "notePreview"),
                pageSummary = ReadAttribute(entry, "pageSummary"),
                talkPreview = ReadAttribute(entry, "talkPreview"),
                optionCount = entry.Options.Count,
                options = entry.Options.Select(option => new
                {
                    id = option.Id,
                    label = option.Label,
                    description = option.Description,
                    attributes = option.Attributes,
                }).ToArray(),
            })
            .ToArray();
    }

    private static string? ResolveDescription(StaticKnowledgeEntry entry)
    {
        var description = ReadAttribute(entry, "description");
        if (!LooksLikeSyntheticText(description, entry))
        {
            return description;
        }

        var flavor = ReadAttribute(entry, "flavor");
        if (!string.IsNullOrWhiteSpace(flavor))
        {
            return flavor;
        }

        var summary = ReadAttribute(entry, "summary");
        if (!string.IsNullOrWhiteSpace(summary))
        {
            return summary;
        }

        return entry.RawText;
    }

    private static bool LooksLikeSyntheticText(string? value, StaticKnowledgeEntry entry)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var normalizedValue = NormalizeSyntheticMatch(value);
        var normalizedKey = NormalizeSyntheticMatch(ReadAttribute(entry, "l10nKey"));
        if (!string.IsNullOrWhiteSpace(normalizedKey) && normalizedValue == normalizedKey)
        {
            return true;
        }

        var letters = value.Where(char.IsLetter).ToArray();
        return value.Contains('_', StringComparison.Ordinal)
               || (letters.Length > 0 && letters.All(character => character <= 127 && !char.IsLower(character)));
    }

    private static string NormalizeSyntheticMatch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    }

    private static string? ReadAttribute(StaticKnowledgeEntry entry, string key)
    {
        return entry.Attributes.TryGetValue(key, out var value)
            ? value
            : null;
    }

    private static string WriteJson(string path, object value)
    {
        File.WriteAllText(
            path,
            JsonSerializer.Serialize(
                value,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                }));
        return path;
    }
}
