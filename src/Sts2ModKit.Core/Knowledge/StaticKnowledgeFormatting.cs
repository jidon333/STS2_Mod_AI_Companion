using System.Text;

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
        var lines = new List<string>
        {
            $"count: {entries.Count}",
        };

        lines.AddRange(entries.Take(12).Select(entry =>
        {
            var tagText = entry.Tags.Count == 0 ? "none" : string.Join(", ", entry.Tags);
            var optionsText = entry.Options.Count == 0 ? string.Empty : $" options={entry.Options.Count}";
            return $"- {entry.Name} [{entry.Id}] source={entry.Source} tags={tagText}{optionsText}";
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
}

public static class StaticKnowledgeMarkdownReportFormatter
{
    public static IReadOnlyList<string> WriteReports(
        string markdownRoot,
        StaticKnowledgeCatalog catalog,
        StaticKnowledgeSourceManifest sourceManifest)
    {
        Directory.CreateDirectory(markdownRoot);

        var created = new List<string>();
        created.Add(WriteOverview(markdownRoot, catalog, sourceManifest));
        created.Add(WriteSection(markdownRoot, "cards", "카드", catalog.Cards));
        created.Add(WriteSection(markdownRoot, "relics", "유물", catalog.Relics));
        created.Add(WriteSection(markdownRoot, "potions", "포션", catalog.Potions));
        created.Add(WriteSection(markdownRoot, "events", "이벤트", catalog.Events));
        created.Add(WriteSection(markdownRoot, "shops", "상점", catalog.Shops));
        created.Add(WriteSection(markdownRoot, "rewards", "보상", catalog.Rewards));
        created.Add(WriteSection(markdownRoot, "keywords", "키워드", catalog.Keywords));
        return created;
    }

    private static string WriteOverview(
        string markdownRoot,
        StaticKnowledgeCatalog catalog,
        StaticKnowledgeSourceManifest sourceManifest)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# 정적 분석 리포트");
        builder.AppendLine();
        builder.AppendLine("이 폴더는 `artifacts/knowledge` 아래의 JSON 산출물을 사람이 읽기 쉬운 Markdown으로 다시 정리한 결과입니다.");
        builder.AppendLine();
        builder.AppendLine("## 생성 정보");
        builder.AppendLine();
        builder.AppendLine($"- 생성 시각: `{sourceManifest.GeneratedAt:yyyy-MM-dd HH:mm:ss zzz}`");
        builder.AppendLine($"- 게임 버전: `{catalog.Metadata.ReleaseVersion ?? "unknown"}`");
        builder.AppendLine($"- 릴리즈 커밋: `{catalog.Metadata.ReleaseCommit ?? "unknown"}`");
        builder.AppendLine($"- 지식 루트: `{sourceManifest.KnowledgeRoot}`");
        builder.AppendLine();
        builder.AppendLine("## 해석 주의점");
        builder.AppendLine();
        builder.AppendLine("- 현재 리포트는 `assembly-scan`, `pck-inventory`, `observed-merge` 결과를 합쳐서 만듭니다.");
        builder.AppendLine("- 따라서 모든 항목이 실제 플레이로 검증된 것은 아닙니다.");
        builder.AppendLine("- `관찰 여부: 예` 인 항목이 가장 신뢰도가 높고, 그 외 항목은 정적 후보로 봐야 합니다.");
        builder.AppendLine("- 카드/유물/이벤트의 효과 텍스트는 아직 일부만 확보됩니다. 효과가 없다고 표시되는 것은 데이터가 없어서이지 기능이 없다는 뜻이 아닙니다.");
        builder.AppendLine();
        builder.AppendLine("## 전체 수량");
        builder.AppendLine();
        AppendCount(builder, "카드", catalog.Cards);
        AppendCount(builder, "유물", catalog.Relics);
        AppendCount(builder, "포션", catalog.Potions);
        AppendCount(builder, "이벤트", catalog.Events);
        AppendCount(builder, "상점", catalog.Shops);
        AppendCount(builder, "보상", catalog.Rewards);
        AppendCount(builder, "키워드", catalog.Keywords);
        builder.AppendLine();
        builder.AppendLine("## 파이프라인 단계");
        builder.AppendLine();
        foreach (var step in sourceManifest.Steps)
        {
            var warningSuffix = step.Warnings.Count == 0 ? string.Empty : $" / 경고 {step.Warnings.Count}건";
            builder.AppendLine($"- `{step.Name}`: `{step.Status}`{warningSuffix}");
        }

        if (sourceManifest.Warnings.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("## 공통 경고");
            builder.AppendLine();
            foreach (var warning in sourceManifest.Warnings)
            {
                builder.AppendLine($"- {warning}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## 개별 리포트");
        builder.AppendLine();
        builder.AppendLine("- [카드](./cards.md)");
        builder.AppendLine("- [유물](./relics.md)");
        builder.AppendLine("- [포션](./potions.md)");
        builder.AppendLine("- [이벤트](./events.md)");
        builder.AppendLine("- [상점](./shops.md)");
        builder.AppendLine("- [보상](./rewards.md)");
        builder.AppendLine("- [키워드](./keywords.md)");

        var path = Path.Combine(markdownRoot, "README.md");
        File.WriteAllText(path, builder.ToString());
        return path;
    }

    private static void AppendCount(StringBuilder builder, string label, IReadOnlyList<StaticKnowledgeEntry> entries)
    {
        var curatedCount = BuildReportItems(entries).Count;
        var observedCount = entries.Count(entry => entry.Observed);
        builder.AppendLine($"- {label}: 전체 `{entries.Count}` / 사람이 읽기 좋은 후보 `{curatedCount}` / 실제 관찰 `{observedCount}`");
    }

    private static string WriteSection(string markdownRoot, string fileStem, string title, IReadOnlyList<StaticKnowledgeEntry> entries)
    {
        var items = BuildReportItems(entries);
        var builder = new StringBuilder();
        builder.AppendLine($"# {title} 정적 분석 리포트");
        builder.AppendLine();
        builder.AppendLine($"- 전체 원본 후보 수: `{entries.Count}`");
        builder.AppendLine($"- 사람이 읽기 좋은 후보 수: `{items.Count}`");
        builder.AppendLine($"- 실제 관찰된 항목 수: `{entries.Count(entry => entry.Observed)}`");
        builder.AppendLine();
        builder.AppendLine("이 문서에는 우선 아래 조건을 만족하는 항목만 추려서 넣었습니다.");
        builder.AppendLine();
        builder.AppendLine("- 실제 관찰된 항목");
        builder.AppendLine("- `Models` 계열 클래스 후보");
        builder.AppendLine("- `res://src/Core/Models/...` 계열 리소스 후보");
        builder.AppendLine();

        if (items.Count == 0)
        {
            builder.AppendLine("아직 사람이 읽기 좋은 후보를 만들지 못했습니다.");
        }
        else
        {
            builder.AppendLine("## 항목 목록");
            builder.AppendLine();
            foreach (var item in items)
            {
                builder.AppendLine($"## {item.DisplayName}");
                builder.AppendLine();
                builder.AppendLine($"- 관찰 여부: {(item.Observed ? "예" : "아니오")}");
                builder.AppendLine($"- 근거 소스: {string.Join(", ", item.Sources)}");
                if (!string.IsNullOrWhiteSpace(item.ModelClass))
                {
                    builder.AppendLine($"- 모델 클래스: `{item.ModelClass}`");
                }

                if (!string.IsNullOrWhiteSpace(item.ResourcePath))
                {
                    builder.AppendLine($"- 리소스 경로: `{item.ResourcePath}`");
                }

                if (!string.IsNullOrWhiteSpace(item.Namespace))
                {
                    builder.AppendLine($"- 네임스페이스: `{item.Namespace}`");
                }

                if (item.Tags.Count > 0)
                {
                    builder.AppendLine($"- 태그: {string.Join(", ", item.Tags)}");
                }

                builder.AppendLine($"- 설명/능력: {item.Description}");
                if (item.Options.Count > 0)
                {
                    builder.AppendLine("- 옵션:");
                    foreach (var option in item.Options)
                    {
                        builder.AppendLine($"  - `{option}`");
                    }
                }

                builder.AppendLine();
            }
        }

        var path = Path.Combine(markdownRoot, $"{fileStem}.md");
        File.WriteAllText(path, builder.ToString());
        return path;
    }

    private static IReadOnlyList<KnowledgeReportItem> BuildReportItems(IReadOnlyList<StaticKnowledgeEntry> entries)
    {
        var candidates = entries
            .Select(BuildReportItem)
            .Where(item => item is not null)
            .Cast<KnowledgeReportItem>()
            .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .Select(MergeGroup)
            .OrderByDescending(item => item.Observed)
            .ThenBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return candidates;
    }

    private static KnowledgeReportItem? BuildReportItem(StaticKnowledgeEntry entry)
    {
        var modelClass = TryReadAttribute(entry, "fullName");
        var resourcePath = TryReadAttribute(entry, "resourcePath");
        var name = ChooseDisplayName(entry, modelClass, resourcePath);
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var readable = entry.Observed
            || LooksLikeReadableModelClass(modelClass)
            || LooksLikeReadableResourcePath(resourcePath);
        if (!readable)
        {
            return null;
        }

        var description = BuildDescription(entry, modelClass, resourcePath);
        var key = NormalizeKey(name);
        return new KnowledgeReportItem(
            key,
            name,
            modelClass,
            resourcePath,
            TryReadAttribute(entry, "namespace"),
            description,
            entry.Observed,
            new[] { entry.Source },
            entry.Tags,
            entry.Options.Select(option => string.IsNullOrWhiteSpace(option.Description) ? option.Label : $"{option.Label}: {option.Description}").ToArray());
    }

    private static KnowledgeReportItem MergeGroup(IGrouping<string, KnowledgeReportItem> group)
    {
        var ordered = group
            .OrderByDescending(item => item.Observed)
            .ThenByDescending(item => !string.IsNullOrWhiteSpace(item.ModelClass))
            .ThenByDescending(item => !string.IsNullOrWhiteSpace(item.ResourcePath))
            .ThenBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var best = ordered[0];
        return new KnowledgeReportItem(
            best.Key,
            best.DisplayName,
            ordered.Select(item => item.ModelClass).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)),
            ordered.Select(item => item.ResourcePath).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)),
            ordered.Select(item => item.Namespace).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)),
            ordered.Select(item => item.Description).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "효과 텍스트 또는 상세 설명은 아직 확보되지 않았습니다.",
            ordered.Any(item => item.Observed),
            ordered.SelectMany(item => item.Sources).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            ordered.SelectMany(item => item.Tags).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            ordered.SelectMany(item => item.Options).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static string? ChooseDisplayName(StaticKnowledgeEntry entry, string? modelClass, string? resourcePath)
    {
        if (IsReadableName(entry.Name))
        {
            return entry.Name.Trim();
        }

        if (LooksLikeReadableModelClass(modelClass))
        {
            return ExtractLastTypeSegment(modelClass!);
        }

        if (LooksLikeReadableResourcePath(resourcePath))
        {
            return PrettifyIdentifier(Path.GetFileNameWithoutExtension(resourcePath!.Split('/', StringSplitOptions.RemoveEmptyEntries).Last()).Trim());
        }

        return null;
    }

    private static string BuildDescription(StaticKnowledgeEntry entry, string? modelClass, string? resourcePath)
    {
        if (!string.IsNullOrWhiteSpace(entry.RawText)
            && !LooksLikePath(entry.RawText)
            && !LooksLikeTypeName(entry.RawText)
            && entry.RawText!.Length <= 240)
        {
            return entry.RawText;
        }

        if (entry.Options.Count > 0)
        {
            return $"선택지 {entry.Options.Count}개가 연결되어 있습니다.";
        }

        if (LooksLikeReadableModelClass(modelClass))
        {
            return "게임 DLL의 모델 클래스 후보에서 확인된 항목입니다.";
        }

        if (LooksLikeReadableResourcePath(resourcePath))
        {
            return "게임 PCK 내부 리소스 경로에서 확인된 항목입니다.";
        }

        return "효과 텍스트 또는 상세 설명은 아직 확보되지 않았습니다.";
    }

    private static bool LooksLikeReadableModelClass(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Contains(".Models.", StringComparison.Ordinal)
               && !value.Contains('<', StringComparison.Ordinal)
               && !value.Contains('>', StringComparison.Ordinal)
               && !value.Contains('+', StringComparison.Ordinal);
    }

    private static bool LooksLikeReadableResourcePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.StartsWith("res://src/Core/Models/", StringComparison.OrdinalIgnoreCase)
               && !value.Contains('"', StringComparison.Ordinal)
               && !value.Contains('%', StringComparison.Ordinal)
               && !value.Contains('<', StringComparison.Ordinal)
               && !value.Contains('>', StringComparison.Ordinal);
    }

    private static bool IsReadableName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.All(character => char.IsLetterOrDigit(character) || character is ' ' or '-' or '\'' or '(' or ')' or '/')
               && !value.Contains("filename", StringComparison.OrdinalIgnoreCase)
               && !value.Contains("image", StringComparison.OrdinalIgnoreCase)
               && !value.Contains("display class", StringComparison.OrdinalIgnoreCase)
               && !value.Contains("ctor param init", StringComparison.OrdinalIgnoreCase)
               && !value.Contains("  ", StringComparison.Ordinal)
               && value.Length <= 80;
    }

    private static string ExtractLastTypeSegment(string fullName)
    {
        var last = fullName.Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? fullName;
        return PrettifyIdentifier(last);
    }

    private static bool LooksLikePath(string value)
    {
        return value.Contains("res://", StringComparison.OrdinalIgnoreCase)
               || value.Contains(".png", StringComparison.OrdinalIgnoreCase)
               || value.Contains(".cs", StringComparison.OrdinalIgnoreCase)
               || value.Contains('/', StringComparison.Ordinal)
               || value.Contains('\\', StringComparison.Ordinal);
    }

    private static bool LooksLikeTypeName(string value)
    {
        return value.Contains("MegaCrit.", StringComparison.Ordinal)
               || value.Contains('<', StringComparison.Ordinal)
               || value.Contains('>', StringComparison.Ordinal)
               || value.Contains('+', StringComparison.Ordinal);
    }

    private static string? TryReadAttribute(StaticKnowledgeEntry entry, string key)
    {
        return entry.Attributes.TryGetValue(key, out var value)
            ? value
            : null;
    }

    private static string NormalizeKey(string value)
    {
        var normalized = new string(value
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray())
            .Trim();
        return string.IsNullOrWhiteSpace(normalized) ? "unknown" : normalized;
    }

    private static string PrettifyIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Unknown";
        }

        var builder = new List<char>(value.Length + 8);
        for (var index = 0; index < value.Length; index += 1)
        {
            var character = value[index];
            if (index > 0
                && char.IsUpper(character)
                && char.IsLetterOrDigit(value[index - 1])
                && !char.IsUpper(value[index - 1]))
            {
                builder.Add(' ');
            }

            builder.Add(character);
        }

        return new string(builder.ToArray()).Replace('_', ' ').Replace('-', ' ').Trim();
    }

    private sealed record KnowledgeReportItem(
        string Key,
        string DisplayName,
        string? ModelClass,
        string? ResourcePath,
        string? Namespace,
        string Description,
        bool Observed,
        IReadOnlyList<string> Sources,
        IReadOnlyList<string> Tags,
        IReadOnlyList<string> Options);
}
