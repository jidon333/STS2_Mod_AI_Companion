using System.Text;

namespace Sts2ModKit.Core.Knowledge;

public static class PckInventoryScanner
{
    private static readonly string[] PathMarkers =
    {
        "res://",
        ".tscn",
        ".tres",
        ".png",
        ".json",
        ".txt",
    };

    public static StaticKnowledgeCatalog Scan(
        string pckPath,
        StaticKnowledgeMetadata metadata,
        out IReadOnlyList<string> warnings)
    {
        var localWarnings = new List<string>();
        if (!File.Exists(pckPath))
        {
            localWarnings.Add($"Missing PCK inventory target: {pckPath}");
            warnings = localWarnings;
            return StaticKnowledgeCatalog.CreateEmpty(metadata);
        }

        var resourcePaths = ReadResourcePathCandidates(pckPath).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (resourcePaths.Length == 0)
        {
            localWarnings.Add("No resource path candidates were recovered from the PCK binary scan.");
        }

        warnings = localWarnings;
        return BuildCatalog(resourcePaths, metadata);
    }

    private static StaticKnowledgeCatalog BuildCatalog(IReadOnlyList<string> resourcePaths, StaticKnowledgeMetadata metadata)
    {
        var cards = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var relics = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var potions = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var events = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var shops = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var rewards = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var keywords = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in resourcePaths)
        {
            var lower = path.ToLowerInvariant();
            var name = Path.GetFileNameWithoutExtension(path);
            var entry = new StaticKnowledgeEntry(
                NormalizeId(path),
                Prettify(name),
                "pck-inventory",
                false,
                path,
                new[] { "resource-path" },
                new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["resourcePath"] = path,
                },
                Array.Empty<StaticKnowledgeOption>());

            if (lower.Contains("card", StringComparison.Ordinal))
            {
                cards[entry.Id] = entry;
            }
            else if (lower.Contains("relic", StringComparison.Ordinal))
            {
                relics[entry.Id] = entry;
            }
            else if (lower.Contains("potion", StringComparison.Ordinal))
            {
                potions[entry.Id] = entry;
            }
            else if (lower.Contains("event", StringComparison.Ordinal))
            {
                events[entry.Id] = entry;
            }
            else if (lower.Contains("merchant", StringComparison.Ordinal) || lower.Contains("shop", StringComparison.Ordinal))
            {
                shops[entry.Id] = entry;
            }
            else if (lower.Contains("reward", StringComparison.Ordinal))
            {
                rewards[entry.Id] = entry;
            }
            else if (lower.Contains("keyword", StringComparison.Ordinal) || lower.Contains("intent", StringComparison.Ordinal))
            {
                keywords[entry.Id] = entry;
            }
        }

        return new StaticKnowledgeCatalog(
            DateTimeOffset.UtcNow,
            metadata with
            {
                Stats = metadata.Stats
                    .Concat(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["pckInventoryMode"] = "binary-string-scan",
                        ["pckResourcePathCount"] = resourcePaths.Count.ToString(),
                    })
                    .GroupBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.OrdinalIgnoreCase),
            },
            cards.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            relics.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            potions.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            events.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            shops.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            rewards.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            keywords.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static IEnumerable<string> ReadResourcePathCandidates(string pckPath)
    {
        var bytes = File.ReadAllBytes(pckPath);
        var builder = new List<byte>(256);
        foreach (var value in bytes)
        {
            if (value >= 32 && value <= 126)
            {
                builder.Add(value);
                continue;
            }

            foreach (var candidate in Flush(builder))
            {
                yield return candidate;
            }
        }

        foreach (var candidate in Flush(builder))
        {
            yield return candidate;
        }
    }

    private static IEnumerable<string> Flush(List<byte> bytes)
    {
        if (bytes.Count < 12)
        {
            bytes.Clear();
            yield break;
        }

        var text = Encoding.UTF8.GetString(bytes.ToArray());
        bytes.Clear();

        foreach (var segment in text.Split('\0', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = segment.Trim();
            if (trimmed.Length < 12)
            {
                continue;
            }

            if (!PathMarkers.Any(marker => trimmed.Contains(marker, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            yield return trimmed.Replace('\\', '/');
        }
    }

    private static string NormalizeId(string value)
    {
        return new string(value
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray())
            .Trim('-');
    }

    private static string Prettify(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Unknown";
        }

        return value.Replace('_', ' ').Replace('-', ' ').Trim();
    }
}
