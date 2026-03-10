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
