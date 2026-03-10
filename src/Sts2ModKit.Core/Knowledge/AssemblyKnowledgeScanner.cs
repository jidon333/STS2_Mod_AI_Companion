using System.Reflection;

namespace Sts2ModKit.Core.Knowledge;

public static class AssemblyKnowledgeScanner
{
    private static readonly (string Marker, string Section)[] SectionMarkers =
    {
        ("card", "cards"),
        ("relic", "relics"),
        ("potion", "potions"),
        ("event", "events"),
        ("merchant", "shops"),
        ("shop", "shops"),
        ("reward", "rewards"),
        ("keyword", "keywords"),
        ("intent", "keywords"),
        ("encounter", "keywords"),
    };

    public static StaticKnowledgeCatalog Scan(
        string gameAssemblyPath,
        string managedRoot,
        StaticKnowledgeMetadata metadata,
        out IReadOnlyList<string> warnings)
    {
        var localWarnings = new List<string>();
        if (!File.Exists(gameAssemblyPath))
        {
            localWarnings.Add($"Missing assembly scan target: {gameAssemblyPath}");
            warnings = localWarnings;
            return StaticKnowledgeCatalog.CreateEmpty(metadata);
        }

        if (!Directory.Exists(managedRoot))
        {
            localWarnings.Add($"Missing managed assembly directory: {managedRoot}");
            warnings = localWarnings;
            return StaticKnowledgeCatalog.CreateEmpty(metadata);
        }

        var managedAssemblies = Directory.GetFiles(managedRoot, "*.dll", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(managedRoot, "*.exe", SearchOption.TopDirectoryOnly))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var resolverPaths = managedAssemblies
            .Append(typeof(object).Assembly.Location)
            .Append(typeof(Enumerable).Assembly.Location)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var resolver = new PathAssemblyResolver(resolverPaths);
        using var metadataLoadContext = new MetadataLoadContext(resolver);
        var assembly = metadataLoadContext.LoadFromAssemblyPath(gameAssemblyPath);

        var cards = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var relics = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var potions = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var events = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var shops = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var rewards = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var keywords = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in GetTypesSafely(assembly))
        {
            var section = MatchSection(type.FullName, type.Name);
            if (section is not null)
            {
                UpsertTypeEntry(section, type, cards, relics, potions, events, shops, rewards, keywords);
            }

            foreach (var memberName in GetMemberSeeds(type))
            {
                var memberSection = MatchSection(memberName, memberName);
                if (memberSection is null)
                {
                    continue;
                }

                UpsertSeedEntry(memberSection, memberName, $"{type.FullName}.{memberName}", cards, relics, potions, events, shops, rewards, keywords);
            }
        }

        warnings = localWarnings;
        return new StaticKnowledgeCatalog(
            DateTimeOffset.UtcNow,
            metadata with
            {
                Stats = MergeStats(
                    metadata.Stats,
                    new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["assemblyScanMode"] = "metadata-load-context",
                        ["assemblyScanRoot"] = managedRoot,
                    }),
            },
            cards.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            relics.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            potions.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            events.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            shops.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            rewards.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            keywords.Values.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
    }

    private static IEnumerable<string> GetMemberSeeds(Type type)
    {
        const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        foreach (var field in type.GetFields(Flags))
        {
            yield return field.Name;
        }

        foreach (var property in type.GetProperties(Flags))
        {
            yield return property.Name;
        }

        foreach (var method in type.GetMethods(Flags))
        {
            if (!method.IsSpecialName)
            {
                yield return method.Name;
            }
        }
    }

    private static string? MatchSection(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var normalized = candidate.ToLowerInvariant();
            foreach (var (marker, section) in SectionMarkers)
            {
                if (normalized.Contains(marker, StringComparison.Ordinal))
                {
                    return section;
                }
            }
        }

        return null;
    }

    private static void UpsertTypeEntry(
        string section,
        Type type,
        IDictionary<string, StaticKnowledgeEntry> cards,
        IDictionary<string, StaticKnowledgeEntry> relics,
        IDictionary<string, StaticKnowledgeEntry> potions,
        IDictionary<string, StaticKnowledgeEntry> events,
        IDictionary<string, StaticKnowledgeEntry> shops,
        IDictionary<string, StaticKnowledgeEntry> rewards,
        IDictionary<string, StaticKnowledgeEntry> keywords)
    {
        var target = ResolveSection(section, cards, relics, potions, events, shops, rewards, keywords);
        var id = NormalizeId(type.FullName ?? type.Name);
        var displayName = Prettify(type.Name);
        target[id] = MergeEntry(
            target.TryGetValue(id, out var existing) ? existing : null,
            new StaticKnowledgeEntry(
                id,
                displayName,
                "assembly-scan",
                false,
                type.FullName,
                new[] { "type-candidate" },
                new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["namespace"] = type.Namespace,
                    ["assembly"] = type.Assembly.GetName().Name,
                    ["fullName"] = type.FullName,
                },
                Array.Empty<StaticKnowledgeOption>()));
    }

    private static void UpsertSeedEntry(
        string section,
        string seed,
        string rawText,
        IDictionary<string, StaticKnowledgeEntry> cards,
        IDictionary<string, StaticKnowledgeEntry> relics,
        IDictionary<string, StaticKnowledgeEntry> potions,
        IDictionary<string, StaticKnowledgeEntry> events,
        IDictionary<string, StaticKnowledgeEntry> shops,
        IDictionary<string, StaticKnowledgeEntry> rewards,
        IDictionary<string, StaticKnowledgeEntry> keywords)
    {
        var target = ResolveSection(section, cards, relics, potions, events, shops, rewards, keywords);
        var id = NormalizeId(seed);
        target[id] = MergeEntry(
            target.TryGetValue(id, out var existing) ? existing : null,
            new StaticKnowledgeEntry(
                id,
                Prettify(seed),
                "assembly-scan",
                false,
                rawText,
                new[] { "member-candidate" },
                new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase),
                Array.Empty<StaticKnowledgeOption>()));
    }

    private static IDictionary<string, StaticKnowledgeEntry> ResolveSection(
        string section,
        IDictionary<string, StaticKnowledgeEntry> cards,
        IDictionary<string, StaticKnowledgeEntry> relics,
        IDictionary<string, StaticKnowledgeEntry> potions,
        IDictionary<string, StaticKnowledgeEntry> events,
        IDictionary<string, StaticKnowledgeEntry> shops,
        IDictionary<string, StaticKnowledgeEntry> rewards,
        IDictionary<string, StaticKnowledgeEntry> keywords)
    {
        return section switch
        {
            "cards" => cards,
            "relics" => relics,
            "potions" => potions,
            "events" => events,
            "shops" => shops,
            "rewards" => rewards,
            _ => keywords,
        };
    }

    private static StaticKnowledgeEntry MergeEntry(StaticKnowledgeEntry? existing, StaticKnowledgeEntry incoming)
    {
        if (existing is null)
        {
            return incoming;
        }

        return existing with
        {
            Tags = existing.Tags
                .Concat(incoming.Tags)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            Attributes = existing.Attributes
                .Concat(incoming.Attributes)
                .GroupBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.OrdinalIgnoreCase),
        };
    }

    private static IReadOnlyDictionary<string, string?> MergeStats(
        IReadOnlyDictionary<string, string?> left,
        IReadOnlyDictionary<string, string?> right)
    {
        var merged = new Dictionary<string, string?>(left, StringComparer.OrdinalIgnoreCase);
        foreach (var entry in right)
        {
            merged[entry.Key] = entry.Value;
        }

        return merged;
    }

    private static string Prettify(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Unknown";
        }

        var builder = new List<char>(value.Length + 8);
        for (var index = 0; index < value.Length; index += 1)
        {
            var character = value[index];
            if (index > 0 && char.IsUpper(character) && char.IsLetterOrDigit(value[index - 1]) && !char.IsUpper(value[index - 1]))
            {
                builder.Add(' ');
            }

            builder.Add(character);
        }

        return new string(builder.ToArray()).Replace('_', ' ').Trim();
    }

    private static string NormalizeId(string value)
    {
        var normalized = new string(value
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray())
            .Trim('-');
        return string.IsNullOrWhiteSpace(normalized) ? "unknown" : normalized;
    }
}
