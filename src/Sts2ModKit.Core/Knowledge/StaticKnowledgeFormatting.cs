using System.Text;
using System.Text.RegularExpressions;

namespace Sts2ModKit.Core.Knowledge;

public static class StaticKnowledgeSummaryFormatter
{
    public static string Format(StaticKnowledgeCatalog catalog)
    {
        var builder = new StringBuilder();

        AppendSection(builder, "metadata", new[]
        {
            $"generated_at: {catalog.GeneratedAt:O}",
            $"release_version: {catalog.Metadata.ReleaseVersion ?? "unknown"}",
            $"release_commit: {catalog.Metadata.ReleaseCommit ?? "unknown"}",
            $"release_date: {catalog.Metadata.ReleaseDate?.ToString("O") ?? "unknown"}",
        }.Concat(catalog.Metadata.Stats
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .Select(entry => $"{entry.Key}: {entry.Value ?? "unknown"}")));

        AppendEntrySection(builder, "cards", catalog.Cards);
        AppendEntrySection(builder, "relics", catalog.Relics);
        AppendEntrySection(builder, "potions", catalog.Potions);
        AppendEntrySection(builder, "events", catalog.Events);
        AppendEntrySection(builder, "shops", catalog.Shops);
        AppendEntrySection(builder, "rewards", catalog.Rewards);
        AppendEntrySection(builder, "keywords", catalog.Keywords);

        return builder.ToString().TrimEnd();
    }

    private static void AppendEntrySection(StringBuilder builder, string title, IReadOnlyList<StaticKnowledgeEntry> entries)
    {
        var lines = new List<string> { $"count: {entries.Count}" };
        lines.AddRange(entries.Take(12).Select(entry =>
        {
            var tagText = entry.Tags.Count == 0 ? "none" : string.Join(", ", entry.Tags);
            var optionsText = entry.Options.Count == 0 ? string.Empty : $" options={entry.Options.Count}";
            var description = TryReadAttribute(entry, "description");
            var descriptionSuffix = string.IsNullOrWhiteSpace(description)
                ? string.Empty
                : $" description={TrimForSummary(description!, 80)}";
            return $"- {entry.Name} [{entry.Id}] source={entry.Source} tags={tagText}{optionsText}{descriptionSuffix}";
        }));

        if (entries.Count > 12)
        {
            lines.Add($"... {entries.Count - 12} more");
        }

        AppendSection(builder, title, lines);
    }

    private static void AppendSection(StringBuilder builder, string title, IEnumerable<string> lines)
    {
        builder.AppendLine(title);
        foreach (var line in lines)
        {
            builder.AppendLine(line);
        }

        builder.AppendLine();
    }

    private static string? TryReadAttribute(StaticKnowledgeEntry entry, string key)
    {
        return entry.Attributes.TryGetValue(key, out var value)
            ? value
            : null;
    }

    private static string TrimForSummary(string value, int maxLength)
    {
        var collapsed = value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();
        return collapsed.Length <= maxLength
            ? collapsed
            : $"{collapsed[..maxLength].TrimEnd()}...";
    }
}

public static class StaticKnowledgeMarkdownReportFormatter
{
    private static readonly KnowledgeSectionSpec[] SectionSpecs =
    {
        new(
            "cards",
            "cards",
            K("\\uCE74\\uB4DC"),
            new[]
            {
                K("\\uCE74\\uB4DC \\uBCF4\\uC0C1 \\uD654\\uBA74"),
                K("\\uC0C1\\uC810 \\uAD6C\\uB9E4 \\uD6C4\\uBCF4"),
                K("\\uC774\\uBCA4\\uD2B8 \\uD2B9\\uC218 \\uC120\\uD0DD\\uC9C0"),
                K("\\uD604\\uC7AC \\uB369 \\uAD6C\\uC131 \\uD30C\\uC545"),
            },
            new[]
            {
                K("\\uCE74\\uB4DC \\uD45C\\uC2DC\\uBA85"),
                K("\\uD55C\\uAD6D\\uC5B4/\\uC601\\uC5B4 L10N \\uC124\\uBA85"),
                K("\\uC120\\uD0DD \\uD504\\uB86C\\uD504\\uD2B8"),
                K("\\uBAA8\\uB378 \\uD074\\uB798\\uC2A4\\uC640 \\uCD08\\uC0C1\\uD654 \\uACBD\\uB85C"),
            },
            new[]
            {
                K("\\uAC15\\uD654 \\uB2E8\\uACC4\\uBCC4 \\uCC28\\uC774"),
                K("\\uC2E4\\uC2DC\\uAC04 \\uC804\\uD22C \\uC0C1\\uD638\\uC791\\uC6A9"),
                K("\\uC2E4\\uD50C\\uB808\\uC774 \\uAD50\\uCC28 \\uAC80\\uC99D"),
            },
            new[]
            {
                "localization/kor/cards.json",
                "localization/eng/cards.json",
                "localization/kor/card_library.json",
            }),
        new(
            "relics",
            "relics",
            K("\\uC720\\uBB3C"),
            new[]
            {
                K("\\uC720\\uBB3C \\uBCF4\\uC0C1 \\uD654\\uBA74"),
                K("\\uC0C1\\uC810 \\uAD6C\\uB9E4"),
                K("\\uC774\\uBCA4\\uD2B8 \\uBCF4\\uC0C1"),
                K("\\uD604\\uC7AC \\uBCF4\\uC720 \\uC720\\uBB3C \\uBAA9\\uB85D"),
            },
            new[]
            {
                K("\\uC720\\uBB3C \\uD45C\\uC2DC\\uBA85"),
                K("\\uD55C\\uAD6D\\uC5B4 \\uC124\\uBA85"),
                K("\\uBAA8\\uB378 \\uD074\\uB798\\uC2A4\\uC640 \\uB9AC\\uC18C\\uC2A4 \\uACBD\\uB85C"),
            },
            new[]
            {
                K("\\uBC1C\\uB3D9 \\uD0C0\\uC774\\uBC0D\\uACFC \\uC911\\uBCF5 \\uADDC\\uCE59"),
                K("\\uAC8C\\uC784 \\uB0B4 \\uC2E4\\uC99D"),
            },
            new[]
            {
                "localization/kor/relics.json",
                "localization/eng/relics.json",
                "localization/kor/relic_collection.json",
            }),
        new(
            "potions",
            "potions",
            K("\\uD3EC\\uC158"),
            new[]
            {
                K("\\uC804\\uD22C \\uBCF4\\uC0C1 \\uD654\\uBA74"),
                K("\\uC0C1\\uC810 \\uAD6C\\uB9E4"),
                K("\\uD604\\uC7AC \\uD3EC\\uC158 \\uC2AC\\uB86F"),
            },
            new[]
            {
                K("\\uD3EC\\uC158 \\uD45C\\uC2DC\\uBA85"),
                K("\\uD55C\\uAD6D\\uC5B4 \\uC124\\uBA85"),
                K("\\uBAA8\\uB378 \\uD074\\uB798\\uC2A4\\uC640 \\uB9AC\\uC18C\\uC2A4 \\uACBD\\uB85C"),
            },
            new[]
            {
                K("\\uC0AC\\uC6A9 \\uC870\\uAC74"),
                K("\\uAC00\\uACA9\\uACFC \\uD68D\\uB4DD \\uADDC\\uCE59"),
            },
            new[]
            {
                "localization/kor/potions.json",
                "localization/eng/potions.json",
                "localization/kor/potion_lab.json",
            }),
        new(
            "events",
            "events",
            K("\\uC774\\uBCA4\\uD2B8"),
            new[]
            {
                K("\\uC774\\uBCA4\\uD2B8 \\uBC29 \\uC9C4\\uC785"),
                K("\\uC120\\uD0DD\\uC9C0 \\uD45C\\uC2DC"),
                K("\\uD398\\uC774\\uC9C0 \\uBCF8\\uBB38 \\uD30C\\uC545"),
            },
            new[]
            {
                K("\\uC774\\uBCA4\\uD2B8 \\uC81C\\uBAA9"),
                K("\\uD398\\uC774\\uC9C0 \\uBCF8\\uBB38"),
                K("\\uC120\\uD0DD\\uC9C0 \\uC81C\\uBAA9/\\uC124\\uBA85"),
                K("\\uB300\\uC0AC \\uD504\\uB9AC\\uBDF0"),
            },
            new[]
            {
                K("\\uBD84\\uAE30 \\uACB0\\uACFC \\uBCF4\\uAC15"),
                K("\\uC2E4\\uD50C\\uB808\\uC774 \\uC120\\uD0DD\\uC9C0 \\uAD50\\uCC28 \\uAC80\\uC99D"),
            },
            new[]
            {
                "localization/kor/events.json",
                "localization/eng/events.json",
            }),
        new(
            "shops",
            "shops",
            K("\\uC0C1\\uC810"),
            new[]
            {
                K("\\uC0C1\\uC778 \\uBC29 \\uC9C4\\uC785"),
                K("\\uAD6C\\uB9E4 \\uD6C4\\uBCF4 \\uD45C\\uC2DC"),
                K("\\uCE74\\uB4DC \\uC81C\\uAC70 \\uC11C\\uBE44\\uC2A4"),
            },
            new[]
            {
                K("\\uC0C1\\uC810 UI \\uBB38\\uAD6C"),
                K("\\uCE74\\uB4DC \\uC81C\\uAC70 \\uC11C\\uBE44\\uC2A4 \\uC124\\uBA85"),
                K("\\uC0C1\\uC778 \\uAD00\\uB828 \\uD150\\uD2B8"),
            },
            new[]
            {
                K("\\uC2E4\\uC81C \\uC0C1\\uD488 \\uD480"),
                K("\\uAC00\\uACA9 \\uADDC\\uCE59"),
            },
            new[]
            {
                "localization/kor/merchant_room.json",
                "localization/eng/merchant_room.json",
            }),
        new(
            "rewards",
            "rewards",
            K("\\uBCF4\\uC0C1"),
            new[]
            {
                K("\\uC804\\uD22C \\uBCF4\\uC0C1 \\uD654\\uBA74"),
                K("\\uC774\\uBCA4\\uD2B8 \\uBCF4\\uC0C1 \\uD654\\uBA74"),
                K("\\uCE74\\uB4DC \\uC120\\uD0DD UI"),
            },
            new[]
            {
                K("\\uBCF4\\uC0C1 UI \\uBB38\\uAD6C"),
                K("\\uC120\\uD0DD \\uD654\\uBA74 \\uB9AC\\uC18C\\uC2A4 \\uD78C\\uD2B8"),
            },
            new[]
            {
                K("\\uC2E4\\uC81C \\uBCF4\\uC0C1 \\uD480 \\uAD6C\\uC131"),
                K("\\uAD00\\uCC30 \\uAE30\\uBC18 \\uBCF4\\uAC15"),
            },
            new[]
            {
                "localization/kor/card_reward_ui.json",
                "localization/eng/card_reward_ui.json",
                "localization/kor/card_selection.json",
            }),
        new(
            "keywords",
            "keywords",
            K("\\uD0A4\\uC6CC\\uB4DC/\\uC758\\uB3C4"),
            new[]
            {
                K("\\uCE74\\uB4DC \\uC124\\uBA85 \\uD574\\uC11D"),
                K("\\uC0C1\\uD0DC \\uC774\\uC0C1/\\uBC84\\uD504 \\uD30C\\uC545"),
                K("\\uBAAC\\uC2A4\\uD130 Intent \\uD574\\uC11D"),
            },
            new[]
            {
                K("\\uD0A4\\uC6CC\\uB4DC \\uC81C\\uBAA9"),
                K("\\uD55C\\uAD6D\\uC5B4 \\uC124\\uBA85"),
                K("\\uAD00\\uB828 \\uD30C\\uC6CC/Intent \\uD074\\uB798\\uC2A4"),
            },
            new[]
            {
                K("\\uC2E4\\uC81C \\uCE74\\uB4DC \\uBB38\\uAD6C\\uC640 \\uAD50\\uCC28 \\uAC80\\uC99D"),
                K("\\uAC8C\\uC784 \\uB0B4 \\uC758\\uB3C4 \\uB85C\\uC9C1\\uACFC\\uC758 \\uC5F0\\uACC4"),
            },
            new[]
            {
                "localization/kor/card_keywords.json",
                "localization/eng/card_keywords.json",
                "localization/kor/intents.json",
            }),
    };

    public static IReadOnlyList<string> WriteReports(string markdownRoot, StaticKnowledgeCatalog catalog, StaticKnowledgeSourceManifest sourceManifest)
    {
        Directory.CreateDirectory(markdownRoot);

        var created = new List<string>
        {
            WriteOverview(markdownRoot, catalog, sourceManifest),
            WriteGameplayGuide(markdownRoot, catalog),
        };

        foreach (var spec in SectionSpecs)
        {
            created.Add(WriteSection(markdownRoot, spec, GetEntries(catalog, spec.Key)));
        }

        return created;
    }

    private static IReadOnlyList<StaticKnowledgeEntry> GetEntries(StaticKnowledgeCatalog catalog, string key)
    {
        return key switch
        {
            "cards" => catalog.Cards,
            "relics" => catalog.Relics,
            "potions" => catalog.Potions,
            "events" => catalog.Events,
            "shops" => catalog.Shops,
            "rewards" => catalog.Rewards,
            "keywords" => catalog.Keywords,
            _ => Array.Empty<StaticKnowledgeEntry>(),
        };
    }

    private static string WriteOverview(string markdownRoot, StaticKnowledgeCatalog catalog, StaticKnowledgeSourceManifest sourceManifest)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# " + K("\\uC815\\uC801 \\uBD84\\uC11D Markdown \\uB9AC\\uD3EC\\uD2B8"));
        builder.AppendLine();
        builder.AppendLine(K("\\uC774 \\uD3F4\\uB354\\uB294 `artifacts/knowledge` \\uC544\\uB798 JSON \\uC0B0\\uCD9C\\uBB3C\\uC744 \\uC0AC\\uB78C\\uC774 \\uC77D\\uAE30 \\uC26C\\uC6B4 \\uB9C8\\uD06C\\uB2E4\\uC6B4 \\uD615\\uD0DC\\uB85C \\uC7AC\\uAD6C\\uC131\\uD55C \\uAC83\\uC785\\uB2C8\\uB2E4."));
        builder.AppendLine(K("\\uD575\\uC2EC \\uBAA9\\uC801\\uC740 `\\uC2E4\\uC81C \\uD50C\\uB808\\uC774 \\uC911 AI\\uAC00 \\uBB34\\uC5C7\\uC744 \\uC54C \\uC218 \\uC788\\uB294\\uC9C0`, `\\uC5B4\\uB290 \\uD56D\\uBAA9\\uC774 L10N \\uBCF8\\uBB38\\uAE4C\\uC9C0 \\uC5F0\\uACB0\\uB418\\uC5C8\\uB294\\uC9C0`, `\\uC544\\uC9C1 \\uBB34\\uC5C7\\uC774 \\uAD50\\uCC28 \\uAC80\\uC99D \\uB300\\uC0C1\\uC778\\uC9C0`\\uB97C \\uBE60\\uB974\\uAC8C \\uD30C\\uC545\\uD558\\uB294 \\uAC83\\uC785\\uB2C8\\uB2E4."));
        builder.AppendLine();
        builder.AppendLine("## " + K("\\uD604\\uC7AC \\uC0C1\\uD0DC \\uC694\\uC57D"));
        builder.AppendLine();
        builder.AppendLine("- " + K("\\uCE74\\uB4DC\\uB294 \\uD55C\\uAD6D\\uC5B4 \\uC81C\\uBAA9\\uACFC \\uC124\\uBA85\\uC774 \\uB9CE\\uC774 \\uCC44\\uC6CC\\uC838 \\uC788\\uC2B5\\uB2C8\\uB2E4."));
        builder.AppendLine("- " + K("\\uC774\\uBCA4\\uD2B8\\uB294 \\uC81C\\uBAA9, \\uD398\\uC774\\uC9C0 \\uBCF8\\uBB38, \\uC120\\uD0DD\\uC9C0 \\uC815\\uBCF4\\uAC00 \\uBD80\\uBD84\\uC801\\uC73C\\uB85C \\uC5F0\\uACB0\\uB418\\uC5B4 \\uC788\\uC2B5\\uB2C8\\uB2E4."));
        builder.AppendLine("- " + K("\\uC720\\uBB3C, \\uD3EC\\uC158, \\uD0A4\\uC6CC\\uB4DC, \\uC0C1\\uC810 \\uBB38\\uAD6C\\uB294 coverage\\uAC00 \\uACC4\\uC18D \\uB113\\uC5B4\\uC9C0\\uB294 \\uC911\\uC774\\uBA70, \\uC2E4\\uD50C\\uB808\\uC774 \\uAD50\\uCC28 \\uAC80\\uC99D\\uC774 \\uB0A8\\uC544 \\uC788\\uC2B5\\uB2C8\\uB2E4."));
        builder.AppendLine("- " + K("\\uC5B4\\uC2DC\\uC2A4\\uD134\\uD2B8 AI\\uAC00 \\uC9C1\\uC811 \\uC77D\\uC744 \\uC218 \\uC788\\uB294 JSON\\uC740 `catalog.assistant.json`\\uACFC `assistant/*.json`\\uC785\\uB2C8\\uB2E4."));
        builder.AppendLine();
        builder.AppendLine("## " + K("\\uC0B0\\uCD9C\\uBB3C \\uC704\\uCE58"));
        builder.AppendLine();
        builder.AppendLine($"- `{sourceManifest.KnowledgeRoot}`");
        builder.AppendLine("- `catalog.latest.json` / `catalog.latest.txt`");
        builder.AppendLine("- `catalog.assistant.json` / `catalog.assistant.txt`");
        builder.AppendLine("- `assistant/cards.json`, `assistant/relics.json`, `assistant/events.json` ...");
        builder.AppendLine();
        builder.AppendLine("## " + K("\\uC704\\uCE58\\uBCC4 \\uBC14\\uB85C \\uBCF4\\uAE30"));
        builder.AppendLine();
        foreach (var spec in SectionSpecs)
        {
            builder.AppendLine($"- [{spec.Title}](./{spec.FileStem}.md): {string.Join(", ", spec.GameplayMoments)}");
        }

        builder.AppendLine();
        builder.AppendLine("## " + K("\\uC804\\uCCB4 \\uC218\\uB7C9"));
        builder.AppendLine();
        AppendCount(builder, K("\\uCE74\\uB4DC"), catalog.Cards);
        AppendCount(builder, K("\\uC720\\uBB3C"), catalog.Relics);
        AppendCount(builder, K("\\uD3EC\\uC158"), catalog.Potions);
        AppendCount(builder, K("\\uC774\\uBCA4\\uD2B8"), catalog.Events);
        AppendCount(builder, K("\\uC0C1\\uC810"), catalog.Shops);
        AppendCount(builder, K("\\uBCF4\\uC0C1"), catalog.Rewards);
        AppendCount(builder, K("\\uD0A4\\uC6CC\\uB4DC/\\uC758\\uB3C4"), catalog.Keywords);
        builder.AppendLine();
        builder.AppendLine("## " + K("\\uD30C\\uC774\\uD504\\uB77C\\uC778 \\uC0C1\\uD0DC"));
        builder.AppendLine();
        foreach (var step in sourceManifest.Steps)
        {
            var warningSuffix = step.Warnings.Count == 0 ? string.Empty : $" / {K("\\uACBD\\uACE0")} {step.Warnings.Count}{K("\\uAC74")}";
            builder.AppendLine($"- `{step.Name}`: `{step.Status}`{warningSuffix}");
        }

        if (sourceManifest.Warnings.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("## " + K("\\uACF5\\uD1B5 \\uACBD\\uACE0"));
            builder.AppendLine();
            foreach (var warning in sourceManifest.Warnings)
            {
                builder.AppendLine($"- {warning}");
            }
        }

        var path = Path.Combine(markdownRoot, "README.md");
        File.WriteAllText(path, builder.ToString());
        return path;
    }

    private static string WriteGameplayGuide(string markdownRoot, StaticKnowledgeCatalog catalog)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# " + K("\\uD50C\\uB808\\uC774 \\uAE30\\uC900 \\uC77D\\uAE30 \\uAC00\\uC774\\uB4DC"));
        builder.AppendLine();
        builder.AppendLine(K("\\uC774 \\uBB38\\uC11C\\uB294 `\\uC2E4\\uC81C \\uAC8C\\uC784 \\uD50C\\uB808\\uC774 \\uC911` \\uC5B4\\uB5A4 \\uD654\\uBA74\\uC5D0\\uC11C \\uC774 \\uC815\\uC801 \\uC9C0\\uC2DD \\uC0B0\\uCD9C\\uBB3C\\uC744 \\uCC38\\uC870\\uD558\\uBA74 \\uB418\\uB294\\uC9C0\\uB97C \\uC124\\uBA85\\uD569\\uB2C8\\uB2E4."));
        builder.AppendLine();
        builder.AppendLine("## " + K("\\uAE30\\uBCF8 \\uC6D0\\uCE59"));
        builder.AppendLine();
        builder.AppendLine("- " + K("\\uCE74\\uB4DC, \\uC720\\uBB3C, \\uD3EC\\uC158, \\uC774\\uBCA4\\uD2B8, \\uC0C1\\uC810 \\uBB38\\uAD6C\\uB294 \\uC624\\uD504\\uB77C\\uC778 \\uC9C0\\uC2DD\\uC73C\\uB85C \\uBA3C\\uC800 \\uC77D\\uACE0, \\uC2E4\\uC2DC\\uAC04 state/latest \\uD30C\\uC77C\\uACFC \\uD568\\uAED8 \\uBCF4\\uC544\\uC57C \\uD569\\uB2C8\\uB2E4."));
        builder.AppendLine("- " + K("\\uC124\\uBA85\\uC774 \\uCC44\\uC6CC\\uC9C4 \\uD56D\\uBAA9\\uC740 AI\\uAC00 \\uC9C1\\uC811 \\uADF8 \\uBCF8\\uBB38\\uC744 \\uCC38\\uC870\\uD560 \\uC218 \\uC788\\uACE0, \\uC124\\uBA85\\uC774 \\uBE44\\uC5B4 \\uC788\\uB294 \\uD56D\\uBAA9\\uC740 \\uAD6C\\uC870 \\uC815\\uBCF4\\uC640 \\uAD00\\uCC30 \\uB85C\\uADF8\\uAC00 \\uC8FC\\uC694 \\uADFC\\uAC70\\uAC00 \\uB429\\uB2C8\\uB2E4."));
        builder.AppendLine("- " + K("\\uC2E4\\uD50C\\uB808\\uC774 \\uAD50\\uCC28 \\uAC80\\uC99D\\uC740 reward/event/shop/rest/combat \\uD654\\uBA74\\uC744 \\uC9C0\\uB098\\uBA70 \\uC774\\uB8E8\\uC5B4\\uC838\\uC57C \\uD569\\uB2C8\\uB2E4."));
        builder.AppendLine();
        builder.AppendLine("## " + K("\\uD654\\uBA74\\uBCC4 \\uCC38\\uC870 \\uC21C\\uC11C"));
        builder.AppendLine();
        builder.AppendLine("1. " + K("\\uBA54\\uC778 \\uBA54\\uB274 / \\uB7F0 \\uC2DC\\uC791: state.latest.txt, session.json, catalog.assistant.json \\uC5F0\\uACB0 \\uC0C1\\uD0DC \\uD655\\uC778"));
        builder.AppendLine("2. " + K("\\uCE74\\uB4DC \\uBCF4\\uC0C1: cards.md, assistant/cards.json, currentChoices \\uAD50\\uCC28 \\uD655\\uC778"));
        builder.AppendLine("3. " + K("\\uC774\\uBCA4\\uD2B8: events.md, assistant/events.json, page/options \\uC815\\uBCF4 \\uD655\\uC778"));
        builder.AppendLine("4. " + K("\\uC0C1\\uC810 / \\uD734\\uC2DD: shops.md, rewards.md, currentChoices \\uC640 \\uC124\\uBA85 \\uBB38\\uAD6C \\uD655\\uC778"));
        builder.AppendLine("5. " + K("\\uC804\\uD22C / \\uD134 \\uC2DC\\uC791: cards.md, relics.md, keywords.md \\uC640 \\uC2E4\\uC2DC\\uAC04 \\uB371/\\uC720\\uBB3C \\uC0C1\\uD0DC \\uD568\\uAED8 \\uD574\\uC11D"));
        builder.AppendLine();
        builder.AppendLine("## " + K("\\uD604\\uC7AC \\uBC14\\uB85C \\uC0AC\\uC6A9\\uD560 \\uC218 \\uC788\\uB294 \\uAC83"));
        builder.AppendLine();
        builder.AppendLine($"- {K("\\uCE74\\uB4DC \\uC124\\uBA85 \\uCC44\\uC6C0 \\uD56D\\uBAA9")}: {catalog.Cards.Count(entry => !string.IsNullOrWhiteSpace(ReadAttribute(entry, "description")))}");
        builder.AppendLine($"- {K("\\uC720\\uBB3C \\uC124\\uBA85 \\uCC44\\uC6C0 \\uD56D\\uBAA9")}: {catalog.Relics.Count(entry => !string.IsNullOrWhiteSpace(ReadAttribute(entry, "description")))}");
        builder.AppendLine($"- {K("\\uD3EC\\uC158 \\uC124\\uBA85 \\uCC44\\uC6C0 \\uD56D\\uBAA9")}: {catalog.Potions.Count(entry => !string.IsNullOrWhiteSpace(ReadAttribute(entry, "description")))}");
        builder.AppendLine($"- {K("\\uC774\\uBCA4\\uD2B8 \\uC124\\uBA85 \\uB610\\uB294 \\uC120\\uD0DD\\uC9C0 \\uD655\\uC778 \\uD56D\\uBAA9")}: {catalog.Events.Count(entry => !string.IsNullOrWhiteSpace(ReadAttribute(entry, "description")) || entry.Options.Count > 0)}");
        builder.AppendLine();
        builder.AppendLine("## " + K("\\uC544\\uC9C1 \\uB0A8\\uC740 \\uAC83"));
        builder.AppendLine();
        builder.AppendLine("- " + K("\\uBAA8\\uB4E0 \\uD56D\\uBAA9\\uC774 \\uD50C\\uB808\\uC774 \\uACB0\\uACFC\\uAE4C\\uC9C0 100% \\uAC80\\uC99D\\uB41C \\uAC83\\uC740 \\uC544\\uB2D9\\uB2C8\\uB2E4."));
        builder.AppendLine("- " + K("\\uBC84\\uD504, \\uD0A4\\uC6CC\\uB4DC, \\uC0C1\\uC810 \\uADDC\\uCE59, \\uD2B9\\uC218 \\uC774\\uBCA4\\uD2B8 \\uBD84\\uAE30\\uB294 \\uC2E4\\uD50C\\uB808\\uC774 \\uAD50\\uCC28 \\uAC80\\uC99D\\uC73C\\uB85C \\uACC4\\uC18D \\uB2E4\\uB4EC\\uC5B4\\uC57C \\uD569\\uB2C8\\uB2E4."));
        builder.AppendLine("- " + K("\\uC5B4\\uC2DC\\uC2A4\\uD134\\uD2B8 AI\\uB294 \\uD56D\\uC0C1 \\uC774 \\uC815\\uC801 \\uCE74\\uD0C8\\uB85C\\uADF8\\uB97C state.latest.* \\uC640 \\uD568\\uAED8 \\uC77D\\uC5B4\\uC57C \\uD569\\uB2C8\\uB2E4."));

        var path = Path.Combine(markdownRoot, "PLAY_GUIDE.md");
        File.WriteAllText(path, builder.ToString());
        return path;
    }

    private static string WriteSection(string markdownRoot, KnowledgeSectionSpec spec, IReadOnlyList<StaticKnowledgeEntry> entries)
    {
        var builder = new StringBuilder();
        var items = BuildReportItems(entries, spec.Key);
        var describedCount = items.Count(item => item.HasStructuredDescription);
        var localizedCount = items.Count(item => item.HasLocalization);
        var optionCount = items.Count(item => item.OptionLines.Count > 0);

        builder.AppendLine("# " + spec.Title);
        builder.AppendLine();
        builder.AppendLine($"- {K("\\uC804\\uCCB4 \\uD56D\\uBAA9 \\uC218")}: {items.Count}");
        builder.AppendLine($"- {K("\\uC124\\uBA85 \\uBCF8\\uBB38\\uC774 \\uCC44\\uC6CC\\uC9C4 \\uD56D\\uBAA9")}: {describedCount}");
        builder.AppendLine($"- {K("L10N \\uD0A4 \\uB610\\uB294 \\uC81C\\uBAA9\\uC774 \\uC5F0\\uACB0\\uB41C \\uD56D\\uBAA9")}: {localizedCount}");
        builder.AppendLine($"- {K("\\uC120\\uD0DD\\uC9C0/\\uC635\\uC158 \\uC815\\uBCF4\\uAC00 \\uC788\\uB294 \\uD56D\\uBAA9")}: {optionCount}");
        builder.AppendLine();
        builder.AppendLine("## " + K("\\uC774 \\uC139\\uC158\\uC774 \\uB3C4\\uC640\\uC8FC\\uB294 \\uD50C\\uB808\\uC774 \\uC7A5\\uBA74"));
        builder.AppendLine();
        foreach (var gameplayMoment in spec.GameplayMoments)
        {
            builder.AppendLine($"- {gameplayMoment}");
        }

        builder.AppendLine();
        builder.AppendLine("## " + K("\\uD604\\uC7AC \\uC774 \\uC139\\uC158\\uC5D0\\uC11C \\uD655\\uC778\\uB41C \\uAC83"));
        builder.AppendLine();
        foreach (var verifiedData in spec.VerifiedData)
        {
            builder.AppendLine($"- {verifiedData}");
        }

        builder.AppendLine();
        builder.AppendLine("## " + K("\\uC544\\uC9C1 \\uB0A8\\uC740 \\uC810"));
        builder.AppendLine();
        foreach (var missingData in spec.MissingData)
        {
            builder.AppendLine($"- {missingData}");
        }

        builder.AppendLine();
        builder.AppendLine("## " + K("\\uC8FC\\uC694 L10N/\\uB9AC\\uC18C\\uC2A4 \\uD78C\\uD2B8"));
        builder.AppendLine();
        foreach (var sourceHint in spec.SourceHints)
        {
            builder.AppendLine($"- `{sourceHint}`");
        }

        builder.AppendLine();
        builder.AppendLine("## " + K("\\uD56D\\uBAA9 \\uBAA9\\uB85D"));
        builder.AppendLine();

        foreach (var item in items)
        {
            builder.AppendLine($"### {item.Title}");
            builder.AppendLine();
            builder.AppendLine($"- {K("ID")}: `{item.Id}`");
            builder.AppendLine($"- {K("\\uADF8\\uB8F9/\\uD480 \\uCD94\\uC815")}: {item.Group}");
            builder.AppendLine($"- {K("\\uD50C\\uB808\\uC774 \\uC911 \\uCC38\\uC870 \\uC2DC\\uC810")}: {item.GameplayUse}");
            builder.AppendLine($"- {K("\\uC124\\uBA85 \\uC0C1\\uD0DC")}: {item.AbilityStatus}");
            builder.AppendLine($"- {K("\\uD575\\uC2EC \\uC124\\uBA85")}: {item.Description}");

            if (!string.IsNullOrWhiteSpace(item.SelectionPrompt))
            {
                builder.AppendLine($"- {K("\\uC120\\uD0DD \\uD504\\uB86C\\uD504\\uD2B8")}: {item.SelectionPrompt}");
            }

            if (item.OptionLines.Count > 0)
            {
                builder.AppendLine($"- {K("\\uD655\\uC778\\uB41C \\uC120\\uD0DD\\uC9C0")}:");
                foreach (var optionLine in item.OptionLines)
                {
                    builder.AppendLine($"  - {optionLine}");
                }
            }

            if (!string.IsNullOrWhiteSpace(item.NotePreview))
            {
                builder.AppendLine($"- {K("\\uBD80\\uAC00 \\uBA54\\uBAA8")}: {item.NotePreview}");
            }

            if (!string.IsNullOrWhiteSpace(item.PageSummary))
            {
                builder.AppendLine($"- {K("\\uD398\\uC774\\uC9C0 \\uC694\\uC57D")}: {item.PageSummary}");
            }

            if (!string.IsNullOrWhiteSpace(item.TalkPreview))
            {
                builder.AppendLine($"- {K("\\uB300\\uC0AC \\uD504\\uB9AC\\uBDF0")}: {item.TalkPreview}");
            }

            builder.AppendLine($"- {K("\\uAD00\\uCC30 \\uB85C\\uADF8 \\uBC18\\uC601")}: {(item.Observed ? K("\\uC608") : K("\\uC544\\uB2C8\\uC624"))}");
            builder.AppendLine($"- {K("\\uC8FC\\uC694 \\uC18C\\uC2A4")}: `{item.Source}`");

            if (!string.IsNullOrWhiteSpace(item.PreferredLocale))
            {
                builder.AppendLine($"- {K("\\uC120\\uD638 locale")}: `{item.PreferredLocale}`");
            }

            if (!string.IsNullOrWhiteSpace(item.L10nKey))
            {
                builder.AppendLine($"- {K("L10N key")}: `{item.L10nKey}`");
            }

            if (!string.IsNullOrWhiteSpace(item.ModelClass))
            {
                builder.AppendLine($"- {K("\\uBAA8\\uB378 \\uD074\\uB798\\uC2A4")}: `{item.ModelClass}`");
            }

            if (!string.IsNullOrWhiteSpace(item.ResourcePath))
            {
                builder.AppendLine($"- {K("\\uB9AC\\uC18C\\uC2A4 \\uACBD\\uB85C")}: `{item.ResourcePath}`");
            }

            if (!string.IsNullOrWhiteSpace(item.SourceFileHint))
            {
                builder.AppendLine($"- {K("\\uCD94\\uCD9C \\uD30C\\uC77C \\uD78C\\uD2B8")}: `{item.SourceFileHint}`");
            }

            if (!string.IsNullOrWhiteSpace(item.EnglishFallback))
            {
                builder.AppendLine($"- {K("\\uC601\\uC5B4 fallback")}: {item.EnglishFallback}");
            }

            builder.AppendLine();
        }

        var path = Path.Combine(markdownRoot, $"{spec.FileStem}.md");
        File.WriteAllText(path, builder.ToString());
        return path;
    }

    private static IReadOnlyList<KnowledgeReportItem> BuildReportItems(IReadOnlyList<StaticKnowledgeEntry> entries, string domain)
    {
        return entries
            .Select(entry => BuildReportItem(entry, domain))
            .OrderByDescending(item => item.HasStructuredDescription)
            .ThenByDescending(item => item.Observed)
            .ThenByDescending(item => item.OptionLines.Count)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static KnowledgeReportItem BuildReportItem(StaticKnowledgeEntry entry, string domain)
    {
        var title = ReadAttribute(entry, "title") ?? entry.Name;
        var description = BuildDescription(entry);
        var selectionPrompt = SanitizeSingleLine(ReadAttribute(entry, "selectionScreenPrompt"));
        var modelClass = ReadAttribute(entry, "fullName");
        var resourcePath = ReadAttribute(entry, "resourcePath");
        var englishTitle = ReadAttribute(entry, "englishTitle");
        var englishDescription = ReadAttribute(entry, "englishDescription");
        var optionLines = entry.Options
            .Take(8)
            .Select(BuildOptionLine)
            .ToArray();

        return new KnowledgeReportItem(
            entry.Id,
            title,
            InferGroup(entry, domain),
            InferGameplayUse(entry, domain),
            InferAbilityStatus(entry),
            description,
            selectionPrompt,
            optionLines,
            SanitizeSingleLine(ReadAttribute(entry, "notePreview")),
            SanitizeSingleLine(ReadAttribute(entry, "pageSummary")),
            SanitizeSingleLine(ReadAttribute(entry, "talkPreview")),
            entry.Observed,
            entry.Source,
            ReadAttribute(entry, "preferredLocale"),
            ReadAttribute(entry, "l10nKey"),
            modelClass,
            resourcePath,
            ReadAttribute(entry, "sourceFileHint"),
            BuildEnglishFallback(englishTitle, englishDescription),
            HasLocalization(entry),
            HasStructuredDescription(entry));
    }

    private static string BuildDescription(StaticKnowledgeEntry entry)
    {
        var description = SanitizeParagraph(ReadAttribute(entry, "description"));
        if (!string.IsNullOrWhiteSpace(description))
        {
            return description!;
        }

        var flavor = SanitizeParagraph(ReadAttribute(entry, "flavor"));
        if (!string.IsNullOrWhiteSpace(flavor))
        {
            return flavor!;
        }

        var pageSummary = SanitizeSingleLine(ReadAttribute(entry, "pageSummary"));
        if (!string.IsNullOrWhiteSpace(pageSummary))
        {
            return pageSummary!;
        }

        var notePreview = SanitizeSingleLine(ReadAttribute(entry, "notePreview"));
        if (!string.IsNullOrWhiteSpace(notePreview))
        {
            return notePreview!;
        }

        var talkPreview = SanitizeSingleLine(ReadAttribute(entry, "talkPreview"));
        if (!string.IsNullOrWhiteSpace(talkPreview))
        {
            return talkPreview!;
        }

        if (entry.Options.Count > 0)
        {
            var labels = string.Join(", ", entry.Options.Take(4).Select(option => option.Label));
            return $"{K("\\uC120\\uD0DD\\uC9C0 \\uC815\\uBCF4\\uB294 \\uD655\\uC778\\uB418\\uC5C8\\uC9C0\\uB9CC \\uC0C1\\uC138 \\uC124\\uBA85 \\uBCF8\\uBB38\\uC740 \\uC544\\uC9C1 \\uBE44\\uC5B4 \\uC788\\uC2B5\\uB2C8\\uB2E4.")} {labels}";
        }

        if (HasLocalization(entry))
        {
            return K("\\uC774\\uB984\\uACFC L10N \\uD0A4\\uB294 \\uC5F0\\uACB0\\uB418\\uC5C8\\uC9C0\\uB9CC, \\uD6A8\\uACFC \\uC124\\uBA85 \\uBCF8\\uBB38\\uC740 \\uC544\\uC9C1 \\uBE44\\uC5B4 \\uC788\\uC2B5\\uB2C8\\uB2E4.");
        }

        if (!string.IsNullOrWhiteSpace(ReadAttribute(entry, "fullName"))
            || !string.IsNullOrWhiteSpace(ReadAttribute(entry, "resourcePath")))
        {
            return K("\\uAD6C\\uC870 \\uC815\\uBCF4\\uB294 \\uD655\\uC778\\uB418\\uC5C8\\uC9C0\\uB9CC, \\uD50C\\uB808\\uC774 \\uD6A8\\uACFC\\uB97C \\uC124\\uBA85\\uD560 \\uBCF8\\uBB38\\uC740 \\uC544\\uC9C1 \\uD655\\uBCF4\\uB418\\uC9C0 \\uC54A\\uC558\\uC2B5\\uB2C8\\uB2E4.");
        }

        return K("\\uD604\\uC7AC \\uD56D\\uBAA9\\uC740 \\uC815\\uC801 \\uD6C4\\uBCF4\\uB85C\\uB9CC \\uC2DD\\uBCC4\\uB418\\uC5C8\\uACE0, \\uD50C\\uB808\\uC774 \\uC758\\uBBF8\\uB97C \\uD574\\uC11D\\uD560 \\uADFC\\uAC70\\uAC00 \\uBD80\\uC871\\uD569\\uB2C8\\uB2E4.");
    }

    private static string InferAbilityStatus(StaticKnowledgeEntry entry)
    {
        if (HasStructuredDescription(entry))
        {
            return K("\\uD55C\\uAD6D\\uC5B4 \\uB610\\uB294 \\uC601\\uC5B4 \\uC124\\uBA85 \\uBCF8\\uBB38\\uAE4C\\uC9C0 \\uC5F0\\uACB0\\uB41C \\uC0C1\\uD0DC\\uC785\\uB2C8\\uB2E4.");
        }

        if (entry.Options.Count > 0 || !string.IsNullOrWhiteSpace(ReadAttribute(entry, "pageSummary")))
        {
            return K("\\uC120\\uD0DD\\uC9C0 \\uB610\\uB294 \\uD398\\uC774\\uC9C0 \\uB2E8\\uC704 \\uC815\\uBCF4\\uAE4C\\uC9C0\\uB294 \\uC5F0\\uACB0\\uB418\\uC5C8\\uC9C0\\uB9CC, \\uC804\\uCCB4 \\uC124\\uBA85 \\uC644\\uC131\\uB3C4\\uB294 \\uB354 \\uB2E4\\uB4EC\\uC5B4\\uC57C \\uD569\\uB2C8\\uB2E4.");
        }

        if (HasLocalization(entry))
        {
            return K("\\uC774\\uB984\\uC740 L10N\\uACFC \\uC5F0\\uACB0\\uB418\\uC5C8\\uC9C0\\uB9CC, \\uC124\\uBA85 \\uBCF8\\uBB38 \\uD655\\uBCF4\\uAC00 \\uB354 \\uD544\\uC694\\uD569\\uB2C8\\uB2E4.");
        }

        if (!string.IsNullOrWhiteSpace(ReadAttribute(entry, "fullName"))
            || !string.IsNullOrWhiteSpace(ReadAttribute(entry, "resourcePath")))
        {
            return K("\\uBAA8\\uB378 \\uD074\\uB798\\uC2A4, \\uB9AC\\uC18C\\uC2A4 \\uACBD\\uB85C, \\uAD00\\uCC30 \\uD78C\\uD2B8 \\uC218\\uC900\\uC758 \\uAD6C\\uC870 \\uC815\\uBCF4\\uB9CC \\uC5F0\\uACB0\\uB41C \\uC0C1\\uD0DC\\uC785\\uB2C8\\uB2E4.");
        }

        return K("\\uD50C\\uB808\\uC774 \\uD574\\uC11D\\uC5D0 \\uD544\\uC694\\uD55C \\uC815\\uBCF4\\uAC00 \\uC544\\uC9C1 \\uCDA9\\uBD84\\uD558\\uC9C0 \\uC54A\\uC2B5\\uB2C8\\uB2E4.");
    }

    private static string InferGroup(StaticKnowledgeEntry entry, string domain)
    {
        return domain switch
        {
            "cards" => MergeGroup(
                TryExtractCardPool(entry),
                ReadAttribute(entry, "type"),
                ReadAttribute(entry, "cost") is { Length: > 0 } cost ? $"{K("\\uCF54\\uC2A4\\uD2B8")} {cost}" : null),
            "relics" => MergeGroup(
                TryExtractCharacterGroup(entry),
                entry.Tags.FirstOrDefault(tag => !string.Equals(tag, "l10n", StringComparison.OrdinalIgnoreCase) && !tag.StartsWith("localized-", StringComparison.OrdinalIgnoreCase))),
            "potions" => MergeGroup(
                TryExtractCharacterGroup(entry),
                K("\\uD3EC\\uC158 \\uAD00\\uB828 \\uD56D\\uBAA9")),
            "events" => MergeGroup(
                entry.Options.Count > 0 ? K("\\uC120\\uD0DD\\uC9C0 \\uD655\\uC778\\uB428") : null,
                !string.IsNullOrWhiteSpace(ReadAttribute(entry, "pageSummary")) ? K("\\uD398\\uC774\\uC9C0 \\uBCF8\\uBB38 \\uD655\\uC778\\uB428") : null),
            "shops" => MergeGroup(
                K("\\uC0C1\\uC810/UI"),
                entry.Options.Count > 0 ? K("\\uAD6C\\uB9E4 \\uD6C4\\uBCF4 \\uD655\\uC778\\uB428") : null),
            "rewards" => MergeGroup(
                K("\\uBCF4\\uC0C1/UI"),
                entry.Options.Count > 0 ? K("\\uC120\\uD0DD\\uC9C0 \\uD655\\uC778\\uB428") : null),
            "keywords" => MergeGroup(
                TryExtractKeywordGroup(entry),
                !string.IsNullOrWhiteSpace(ReadAttribute(entry, "description")) ? K("\\uC124\\uBA85 \\uD655\\uBCF4") : null),
            _ => K("\\uBBF8\\uBD84\\uB958"),
        };
    }

    private static string InferGameplayUse(StaticKnowledgeEntry entry, string domain)
    {
        return domain switch
        {
            "cards" => K("\\uCE74\\uB4DC \\uBCF4\\uC0C1, \\uC0C1\\uC810 \\uAD6C\\uB9E4, \\uB371 \\uC810\\uAC80 \\uC7A5\\uBA74\\uC5D0\\uC11C \\uC9C1\\uC811 \\uCC38\\uC870"),
            "relics" => K("\\uC720\\uBB3C \\uBCF4\\uC0C1, \\uC774\\uBCA4\\uD2B8 \\uBCF4\\uC0C1, \\uD604\\uC7AC \\uC720\\uBB3C \\uBAA9\\uB85D \\uD574\\uC11D"),
            "potions" => K("\\uC804\\uD22C \\uBCF4\\uC0C1, \\uC0C1\\uC810 \\uAD6C\\uB9E4, \\uD604\\uC7AC \\uD3EC\\uC158 \\uC2AC\\uB86F \\uD574\\uC11D"),
            "events" => entry.Options.Count > 0
                ? K("\\uC774\\uBCA4\\uD2B8 \\uBCF8\\uBB38\\uACFC \\uC120\\uD0DD\\uC9C0 \\uBE44\\uAD50 \\uC2DC \\uCC38\\uC870")
                : K("\\uC774\\uBCA4\\uD2B8 \\uBC29 \\uC81C\\uBAA9/\\uBCF8\\uBB38 \\uD30C\\uC545 \\uC2DC \\uCC38\\uC870"),
            "shops" => K("\\uC0C1\\uC810 \\uC9C4\\uC785 \\uD6C4 \\uCE74\\uB4DC/\\uC11C\\uBE44\\uC2A4 \\uBE44\\uAD50 \\uC2DC \\uCC38\\uC870"),
            "rewards" => K("\\uBCF4\\uC0C1 \\uD654\\uBA74\\uC5D0\\uC11C \\uC120\\uD0DD\\uC9C0 \\uBE44\\uAD50 \\uC2DC \\uCC38\\uC870"),
            "keywords" => K("\\uCE74\\uB4DC \\uBB38\\uAD6C, \\uC0C1\\uD0DC \\uC774\\uC0C1, Intent \\uD574\\uC11D \\uC2DC \\uCC38\\uC870"),
            _ => K("\\uD50C\\uB808\\uC774 \\uCC38\\uACE0\\uC6A9"),
        };
    }

    private static string? TryExtractCardPool(StaticKnowledgeEntry entry)
    {
        var source = $"{ReadAttribute(entry, "resourcePath")} {ReadAttribute(entry, "fullName")} {entry.Name}".ToLowerInvariant();
        return source switch
        {
            var value when value.Contains("ironclad", StringComparison.Ordinal) => K("\\uC544\\uC774\\uC5B8\\uD074\\uB798\\uB4DC"),
            var value when value.Contains("silent", StringComparison.Ordinal) => K("\\uC0AC\\uC77C\\uB7F0\\uD2B8"),
            var value when value.Contains("defect", StringComparison.Ordinal) => K("\\uB514\\uD399\\uD2B8"),
            var value when value.Contains("watcher", StringComparison.Ordinal) => K("\\uC640\\uCC98"),
            var value when value.Contains("colorless", StringComparison.Ordinal) => K("\\uBB34\\uC0C9"),
            var value when value.Contains("curse", StringComparison.Ordinal) => K("\\uC800\\uC8FC"),
            _ => null,
        };
    }

    private static string? TryExtractCharacterGroup(StaticKnowledgeEntry entry)
    {
        var source = $"{ReadAttribute(entry, "resourcePath")} {ReadAttribute(entry, "fullName")} {entry.Name}".ToLowerInvariant();
        return source switch
        {
            var value when value.Contains("ironclad", StringComparison.Ordinal) => K("\\uC544\\uC774\\uC5B8\\uD074\\uB798\\uB4DC \\uAD00\\uB828"),
            var value when value.Contains("silent", StringComparison.Ordinal) => K("\\uC0AC\\uC77C\\uB7F0\\uD2B8 \\uAD00\\uB828"),
            var value when value.Contains("defect", StringComparison.Ordinal) => K("\\uB514\\uD399\\uD2B8 \\uAD00\\uB828"),
            var value when value.Contains("watcher", StringComparison.Ordinal) => K("\\uC640\\uCC98 \\uAD00\\uB828"),
            _ => null,
        };
    }

    private static string? TryExtractKeywordGroup(StaticKnowledgeEntry entry)
    {
        var fullName = ReadAttribute(entry, "fullName") ?? string.Empty;
        var l10nKey = ReadAttribute(entry, "l10nKey") ?? string.Empty;
        if (fullName.Contains(".Intents.", StringComparison.OrdinalIgnoreCase) || l10nKey.EndsWith("_INTENT", StringComparison.OrdinalIgnoreCase))
        {
            return K("\\uC758\\uB3C4/Intent");
        }

        if (fullName.Contains(".Powers.", StringComparison.OrdinalIgnoreCase) || l10nKey.EndsWith("_POWER", StringComparison.OrdinalIgnoreCase))
        {
            return K("\\uC0C1\\uD0DC\\uC774\\uC0C1/\\uBC84\\uD504");
        }

        return K("\\uD0A4\\uC6CC\\uB4DC");
    }

    private static string MergeGroup(params string?[] values)
    {
        var merged = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return merged.Length == 0
            ? K("\\uBBF8\\uBD84\\uB958")
            : string.Join(", ", merged);
    }

    private static string BuildOptionLine(StaticKnowledgeOption option)
    {
        var description = SanitizeSingleLine(option.Description);
        return string.IsNullOrWhiteSpace(description)
            ? option.Label
            : $"{option.Label}: {description}";
    }

    private static bool HasLocalization(StaticKnowledgeEntry entry)
    {
        return !string.IsNullOrWhiteSpace(ReadAttribute(entry, "l10nKey"))
               || !string.IsNullOrWhiteSpace(ReadAttribute(entry, "title"))
               || !string.IsNullOrWhiteSpace(ReadAttribute(entry, "preferredLocale"));
    }

    private static bool HasStructuredDescription(StaticKnowledgeEntry entry)
    {
        return !string.IsNullOrWhiteSpace(ReadAttribute(entry, "description"))
               || !string.IsNullOrWhiteSpace(ReadAttribute(entry, "flavor"))
               || !string.IsNullOrWhiteSpace(ReadAttribute(entry, "pageSummary"))
               || !string.IsNullOrWhiteSpace(ReadAttribute(entry, "notePreview"))
               || !string.IsNullOrWhiteSpace(ReadAttribute(entry, "talkPreview"));
    }

    private static string? BuildEnglishFallback(string? englishTitle, string? englishDescription)
    {
        englishTitle = SanitizeSingleLine(englishTitle);
        englishDescription = SanitizeSingleLine(englishDescription);
        if (string.IsNullOrWhiteSpace(englishTitle) && string.IsNullOrWhiteSpace(englishDescription))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(englishDescription))
        {
            return englishTitle;
        }

        if (string.IsNullOrWhiteSpace(englishTitle))
        {
            return englishDescription;
        }

        return $"{englishTitle}: {englishDescription}";
    }

    private static void AppendCount(StringBuilder builder, string label, IReadOnlyList<StaticKnowledgeEntry> entries)
    {
        var described = entries.Count(HasStructuredDescription);
        var localized = entries.Count(HasLocalization);
        builder.AppendLine($"- {label}: {entries.Count} ({K("\\uC124\\uBA85")} {described}, L10N {localized})");
    }

    private static string? ReadAttribute(StaticKnowledgeEntry entry, string key)
    {
        return entry.Attributes.TryGetValue(key, out var value)
            ? value
            : null;
    }

    private static string? SanitizeSingleLine(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var sanitized = Regex.Replace(value, "\\s+", " ").Trim();
        return string.IsNullOrWhiteSpace(sanitized)
            ? null
            : sanitized;
    }

    private static string? SanitizeParagraph(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Replace("\r", string.Empty, StringComparison.Ordinal).Trim();
        normalized = Regex.Replace(normalized, "\n{3,}", "\n\n");
        return string.IsNullOrWhiteSpace(normalized)
            ? null
            : normalized;
    }

    private static string K(string escaped)
    {
        return Regex.Unescape(escaped);
    }
}

internal sealed record KnowledgeSectionSpec(
    string Key,
    string FileStem,
    string Title,
    IReadOnlyList<string> GameplayMoments,
    IReadOnlyList<string> VerifiedData,
    IReadOnlyList<string> MissingData,
    IReadOnlyList<string> SourceHints);

internal sealed record KnowledgeReportItem(
    string Id,
    string Title,
    string Group,
    string GameplayUse,
    string AbilityStatus,
    string Description,
    string? SelectionPrompt,
    IReadOnlyList<string> OptionLines,
    string? NotePreview,
    string? PageSummary,
    string? TalkPreview,
    bool Observed,
    string Source,
    string? PreferredLocale,
    string? L10nKey,
    string? ModelClass,
    string? ResourcePath,
    string? SourceFileHint,
    string? EnglishFallback,
    bool HasLocalization,
    bool HasStructuredDescription);
