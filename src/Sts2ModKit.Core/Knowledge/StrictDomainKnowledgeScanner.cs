using System.Text.Json;
using System.Text.RegularExpressions;

namespace Sts2ModKit.Core.Knowledge;

public static class StrictDomainKnowledgeScanner
{
    private static readonly Regex CardClassRegex = new(@"public\s+(?:abstract\s+|sealed\s+)?class\s+(?<name>[A-Za-z0-9_]+)\s*:\s*CardModel", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex RelicClassRegex = new(@"public\s+(?:abstract\s+|sealed\s+)?class\s+(?<name>[A-Za-z0-9_]+)\s*:\s*RelicModel", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PotionClassRegex = new(@"public\s+(?:abstract\s+|sealed\s+)?class\s+(?<name>[A-Za-z0-9_]+)\s*:\s*PotionModel", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex EventClassRegex = new(@"public\s+(?:abstract\s+|sealed\s+)?class\s+(?<name>[A-Za-z0-9_]+)\s*:\s*EventModel", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex CardBaseRegex = new(@":\s*base\(\s*(?<cost>[^,\r\n]+?)\s*,\s*CardType\.(?<type>[A-Za-z0-9_]+)\s*,\s*CardRarity\.(?<rarity>[A-Za-z0-9_]+)\s*,\s*TargetType\.(?<target>[A-Za-z0-9_]+)\s*\)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
    private static readonly Regex DynamicVarRegex = new(@"new\s+(?<kind>[A-Za-z0-9_]+Var)(?:<(?<generic>[A-Za-z0-9_]+)>)?\(\s*(?<value>-?[0-9]+(?:\.[0-9]+)?m?)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex UpgradeRegex = new(@"base\.DynamicVars\.(?<name>[A-Za-z0-9_]+)\.UpgradeValueBy\((?<value>[^)]+)\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex TitlePropertyRegex = new(@"public\s+override\s+string\s+Title\s*=>\s*""(?<value>[^""]+)""", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex RelicRarityRegex = new(@"public\s+override\s+RelicRarity\s+Rarity\s*=>\s*RelicRarity\.(?<value>[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PotionRarityRegex = new(@"public\s+override\s+PotionRarity\s+Rarity\s*=>\s*PotionRarity\.(?<value>[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PotionUsageRegex = new(@"public\s+override\s+PotionUsage\s+Usage\s*=>\s*PotionUsage\.(?<value>[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PotionTargetRegex = new(@"public\s+override\s+TargetType\s+TargetType\s*=>\s*TargetType\.(?<value>[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex EventOptionKeyRegex = new(@"""(?<value>[A-Z0-9_]+\.pages\.[A-Z0-9_]+\.options\.[A-Z0-9_]+)""", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex EventPageKeyRegex = new(@"""(?<value>[A-Z0-9_]+\.pages\.[A-Z0-9_]+\.description)""", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static StaticKnowledgeCatalog Scan(
        string decompiledRoot,
        StaticKnowledgeCatalog rawPckCatalog,
        StaticKnowledgeMetadata metadata,
        out IReadOnlyList<string> warnings)
    {
        var localWarnings = new List<string>();
        if (!Directory.Exists(decompiledRoot))
        {
            localWarnings.Add($"Missing decompiled root: {decompiledRoot}");
            warnings = localWarnings;
            return StaticKnowledgeCatalog.CreateEmpty(metadata);
        }

        var resourceIndex = ResourceIndex.Build(rawPckCatalog);
        var cardPools = ParsePoolMap(GetDomainDirectory(decompiledRoot, "CardPools"), "Card", localWarnings);
        var relicPools = ParsePoolMap(GetDomainDirectory(decompiledRoot, "RelicPools"), "Relic", localWarnings);

        var cards = ParseDomainEntries(
            GetDomainDirectory(decompiledRoot, "Cards"),
            CardClassRegex,
            ParseCardEntry,
            localWarnings,
            resourceIndex,
            cardPools);
        var relics = ParseDomainEntries(
            GetDomainDirectory(decompiledRoot, "Relics"),
            RelicClassRegex,
            ParseRelicEntry,
            localWarnings,
            resourceIndex,
            relicPools);
        var potions = ParseDomainEntries(
            GetDomainDirectory(decompiledRoot, "Potions"),
            PotionClassRegex,
            ParsePotionEntry,
            localWarnings,
            resourceIndex,
            poolMap: null);
        var events = ParseDomainEntries(
            GetDomainDirectory(decompiledRoot, "Events"),
            EventClassRegex,
            ParseEventEntry,
            localWarnings,
            resourceIndex,
            poolMap: null);

        warnings = localWarnings;
        return new StaticKnowledgeCatalog(
            DateTimeOffset.UtcNow,
            metadata with
            {
                Stats = metadata.Stats
                    .Concat(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["strictDomainMode"] = "decompiled-model-parser",
                        ["strictCardCount"] = cards.Count.ToString(),
                        ["strictRelicCount"] = relics.Count.ToString(),
                        ["strictPotionCount"] = potions.Count.ToString(),
                        ["strictEventCount"] = events.Count.ToString(),
                    })
                    .GroupBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.OrdinalIgnoreCase),
            },
            cards,
            relics,
            potions,
            events,
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>());
    }

    private static string GetDomainDirectory(string decompiledRoot, string domain)
    {
        return Path.Combine(decompiledRoot, "MegaCrit", "Sts2", "Core", "Models", domain);
    }

    private static IReadOnlyDictionary<string, string> ParsePoolMap(string directory, string modelKind, ICollection<string> warnings)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(directory))
        {
            warnings.Add($"Missing strict pool directory: {directory}");
            return map;
        }

        foreach (var path in Directory.GetFiles(directory, "*.cs", SearchOption.TopDirectoryOnly))
        {
            var fileName = Path.GetFileName(path);
            if (ShouldSkipType(Path.GetFileNameWithoutExtension(path)))
            {
                continue;
            }

            var content = File.ReadAllText(path);
            var title = TitlePropertyRegex.Match(content).Groups["value"].Value;
            if (string.IsNullOrWhiteSpace(title))
            {
                title = Path.GetFileNameWithoutExtension(path)
                    .Replace(modelKind + "Pool", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim();
            }

            var regex = new Regex($@"ModelDb\.{modelKind}<(?<name>[A-Za-z0-9_]+)>\(\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            foreach (Match match in regex.Matches(content))
            {
                var name = match.Groups["name"].Value;
                if (ShouldSkipType(name))
                {
                    continue;
                }

                map[name] = title;
            }
        }

        return map;
    }

    private static IReadOnlyList<StaticKnowledgeEntry> ParseDomainEntries(
        string directory,
        Regex classRegex,
        Func<string, string, string, ResourceIndex, IReadOnlyDictionary<string, string>?, StaticKnowledgeEntry?> parser,
        ICollection<string> warnings,
        ResourceIndex resourceIndex,
        IReadOnlyDictionary<string, string>? poolMap)
    {
        if (!Directory.Exists(directory))
        {
            warnings.Add($"Missing strict domain directory: {directory}");
            return Array.Empty<StaticKnowledgeEntry>();
        }

        var entries = new List<StaticKnowledgeEntry>();
        foreach (var path in Directory.GetFiles(directory, "*.cs", SearchOption.TopDirectoryOnly))
        {
            var content = File.ReadAllText(path);
            var match = classRegex.Match(content);
            if (!match.Success)
            {
                continue;
            }

            var className = match.Groups["name"].Value;
            if (ShouldSkipType(className))
            {
                continue;
            }

            var entry = parser(path, content, className, resourceIndex, poolMap);
            if (entry is not null)
            {
                entries.Add(entry);
            }
        }

        return entries
            .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static StaticKnowledgeEntry? ParseCardEntry(
        string path,
        string content,
        string className,
        ResourceIndex resourceIndex,
        IReadOnlyDictionary<string, string>? poolMap)
    {
        var fullName = $"MegaCrit.Sts2.Core.Models.Cards.{className}";
        var attributes = CreateBaseAttributes(path, fullName, className, resourceIndex.FindCardPath(className));
        attributes["strictDomain"] = "card";
        attributes["strictModel"] = "true";
        attributes["classId"] = ToLocalizationStem(className);

        var baseMatch = CardBaseRegex.Match(content);
        if (baseMatch.Success)
        {
            attributes["cost"] = NormalizeNumericText(baseMatch.Groups["cost"].Value);
            attributes["type"] = baseMatch.Groups["type"].Value;
            attributes["rarity"] = baseMatch.Groups["rarity"].Value;
            attributes["target"] = baseMatch.Groups["target"].Value;
        }

        if (poolMap is not null && poolMap.TryGetValue(className, out var pool))
        {
            attributes["pool"] = pool;
            attributes["color"] = pool;
        }

        AttachDynamicVars(attributes, content);
        AttachUpgradeHints(attributes, content);

        return new StaticKnowledgeEntry(
            NormalizeId(fullName),
            PrettifyClassName(className),
            "strict-domain-parse",
            false,
            null,
            new[] { "strict-card", "strict-model" },
            attributes,
            Array.Empty<StaticKnowledgeOption>());
    }

    private static StaticKnowledgeEntry? ParseRelicEntry(
        string path,
        string content,
        string className,
        ResourceIndex resourceIndex,
        IReadOnlyDictionary<string, string>? poolMap)
    {
        var fullName = $"MegaCrit.Sts2.Core.Models.Relics.{className}";
        var attributes = CreateBaseAttributes(path, fullName, className, resourceIndex.FindRelicPath(className));
        attributes["strictDomain"] = "relic";
        attributes["strictModel"] = "true";
        attributes["classId"] = ToLocalizationStem(className);

        var rarity = RelicRarityRegex.Match(content).Groups["value"].Value;
        if (!string.IsNullOrWhiteSpace(rarity))
        {
            attributes["rarity"] = rarity;
        }

        if (poolMap is not null && poolMap.TryGetValue(className, out var pool))
        {
            attributes["pool"] = pool;
        }

        AttachDynamicVars(attributes, content);

        return new StaticKnowledgeEntry(
            NormalizeId(fullName),
            PrettifyClassName(className),
            "strict-domain-parse",
            false,
            null,
            new[] { "strict-relic", "strict-model" },
            attributes,
            Array.Empty<StaticKnowledgeOption>());
    }

    private static StaticKnowledgeEntry? ParsePotionEntry(
        string path,
        string content,
        string className,
        ResourceIndex resourceIndex,
        IReadOnlyDictionary<string, string>? _)
    {
        var fullName = $"MegaCrit.Sts2.Core.Models.Potions.{className}";
        var attributes = CreateBaseAttributes(path, fullName, className, resourceIndex.FindPotionPath(className));
        attributes["strictDomain"] = "potion";
        attributes["strictModel"] = "true";
        attributes["classId"] = ToLocalizationStem(className);

        var rarity = PotionRarityRegex.Match(content).Groups["value"].Value;
        if (!string.IsNullOrWhiteSpace(rarity))
        {
            attributes["rarity"] = rarity;
        }

        var usage = PotionUsageRegex.Match(content).Groups["value"].Value;
        if (!string.IsNullOrWhiteSpace(usage))
        {
            attributes["usage"] = usage;
        }

        var target = PotionTargetRegex.Match(content).Groups["value"].Value;
        if (!string.IsNullOrWhiteSpace(target))
        {
            attributes["target"] = target;
        }

        AttachDynamicVars(attributes, content);

        return new StaticKnowledgeEntry(
            NormalizeId(fullName),
            PrettifyClassName(className),
            "strict-domain-parse",
            false,
            null,
            new[] { "strict-potion", "strict-model" },
            attributes,
            Array.Empty<StaticKnowledgeOption>());
    }

    private static StaticKnowledgeEntry? ParseEventEntry(
        string path,
        string content,
        string className,
        ResourceIndex resourceIndex,
        IReadOnlyDictionary<string, string>? _)
    {
        var fullName = $"MegaCrit.Sts2.Core.Models.Events.{className}";
        var attributes = CreateBaseAttributes(path, fullName, className, resourceIndex.FindEventPath(className));
        attributes["strictDomain"] = "event";
        attributes["strictModel"] = "true";
        attributes["classId"] = ToLocalizationStem(className);

        var pageKeys = EventPageKeyRegex.Matches(content)
            .Select(match => match.Groups["value"].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (pageKeys.Length > 0)
        {
            attributes["pageKeyCount"] = pageKeys.Length.ToString();
            attributes["pageKeyPreview"] = string.Join(" | ", pageKeys.Take(4));
        }

        var optionKeys = EventOptionKeyRegex.Matches(content)
            .Select(match => match.Groups["value"].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (optionKeys.Length > 0)
        {
            attributes["optionKeyCount"] = optionKeys.Length.ToString();
            attributes["optionKeyPreview"] = string.Join(" | ", optionKeys.Take(4));
        }

        AttachDynamicVars(attributes, content);

        return new StaticKnowledgeEntry(
            NormalizeId(fullName),
            PrettifyClassName(className),
            "strict-domain-parse",
            false,
            null,
            new[] { "strict-event", "strict-model" },
            attributes,
            Array.Empty<StaticKnowledgeOption>());
    }

    private static Dictionary<string, string?> CreateBaseAttributes(string path, string fullName, string className, string? resourcePath)
    {
        return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["fullName"] = fullName,
            ["className"] = className,
            ["resourcePath"] = resourcePath,
            ["strictSourceFile"] = path,
        };
    }

    private static void AttachDynamicVars(IDictionary<string, string?> attributes, string content)
    {
        var vars = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in DynamicVarRegex.Matches(content))
        {
            var name = ResolveDynamicVarName(match.Groups["kind"].Value, match.Groups["generic"].Value);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            vars[name] = NormalizeNumericText(match.Groups["value"].Value);
        }

        foreach (var entry in vars)
        {
            attributes[$"var.{entry.Key}"] = entry.Value;
        }

        if (vars.Count > 0)
        {
            attributes["dynamicVarsJson"] = JsonSerializer.Serialize(vars, new JsonSerializerOptions { WriteIndented = false });
        }
    }

    private static void AttachUpgradeHints(IDictionary<string, string?> attributes, string content)
    {
        var upgrades = UpgradeRegex.Matches(content)
            .Select(match => $"{match.Groups["name"].Value}+{NormalizeNumericText(match.Groups["value"].Value)}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (upgrades.Length > 0)
        {
            attributes["upgradeSummary"] = string.Join(", ", upgrades);
        }
    }

    private static string ResolveDynamicVarName(string kind, string generic)
    {
        return kind switch
        {
            "DamageVar" => "Damage",
            "BlockVar" => "Block",
            "CardsVar" => "Cards",
            "EnergyVar" => "Energy",
            "HealVar" => "Heal",
            "MaxHpVar" => "MaxHp",
            "GoldVar" => "Gold",
            "HitCountVar" => "HitCount",
            "CostVar" => "Cost",
            "PowerVar" when !string.IsNullOrWhiteSpace(generic) => generic,
            _ => string.Empty,
        };
    }

    private static bool ShouldSkipType(string className)
    {
        return string.IsNullOrWhiteSpace(className)
               || className.StartsWith("Mock", StringComparison.OrdinalIgnoreCase)
               || className.StartsWith("Deprecated", StringComparison.OrdinalIgnoreCase)
               || className.Contains("<", StringComparison.Ordinal)
               || className.Contains("DisplayClass", StringComparison.Ordinal)
               || string.Equals(className, "<>c", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeNumericText(string value)
    {
        return value.Trim().TrimEnd('m', 'M');
    }

    private static string NormalizeId(string value)
    {
        return new string(value
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray())
            .Trim('-');
    }

    private static string PrettifyClassName(string className)
    {
        return Regex.Replace(className, "(?<!^)([A-Z])", " $1");
    }

    private static string ToLocalizationStem(string className)
    {
        var withUnderscores = Regex.Replace(className, "([a-z0-9])([A-Z])", "$1_$2");
        return withUnderscores.ToUpperInvariant();
    }

    private sealed class ResourceIndex
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _cardPaths;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _relicPaths;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _potionPaths;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _eventPaths;

        private ResourceIndex(
            IReadOnlyDictionary<string, IReadOnlyList<string>> cardPaths,
            IReadOnlyDictionary<string, IReadOnlyList<string>> relicPaths,
            IReadOnlyDictionary<string, IReadOnlyList<string>> potionPaths,
            IReadOnlyDictionary<string, IReadOnlyList<string>> eventPaths)
        {
            _cardPaths = cardPaths;
            _relicPaths = relicPaths;
            _potionPaths = potionPaths;
            _eventPaths = eventPaths;
        }

        public static ResourceIndex Build(StaticKnowledgeCatalog rawPckCatalog)
        {
            return new ResourceIndex(
                BuildMap(rawPckCatalog.Cards),
                BuildMap(rawPckCatalog.Relics),
                BuildMap(rawPckCatalog.Potions),
                BuildMap(rawPckCatalog.Events));
        }

        public string? FindCardPath(string className)
        {
            return FindBestPath(_cardPaths, className, "/card_portraits/", "/Models/Cards/");
        }

        public string? FindRelicPath(string className)
        {
            return FindBestPath(_relicPaths, className, "/images/relics/", "/Models/Relics/", "/relics/");
        }

        public string? FindPotionPath(string className)
        {
            return FindBestPath(_potionPaths, className, "/images/potions/", "/Models/Potions/", "/potions/");
        }

        public string? FindEventPath(string className)
        {
            return FindBestPath(_eventPaths, className, "/events/", "/Models/Events/");
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildMap(IEnumerable<StaticKnowledgeEntry> entries)
        {
            var map = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in entries
                         .Select(entry => entry.Attributes.TryGetValue("resourcePath", out var resourcePath) ? resourcePath : null)
                         .Where(resourcePath => !string.IsNullOrWhiteSpace(resourcePath))
                         .Cast<string>())
            {
                var fileName = Path.GetFileNameWithoutExtension(path.Replace('\\', '/'));
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    continue;
                }

                var key = NormalizeMatchKey(fileName);
                if (!map.TryGetValue(key, out var paths))
                {
                    paths = new List<string>();
                    map[key] = paths;
                }

                if (!paths.Contains(path, StringComparer.OrdinalIgnoreCase))
                {
                    paths.Add(path);
                }
            }

            return map.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<string>)pair.Value, StringComparer.OrdinalIgnoreCase);
        }

        private static string? FindBestPath(IReadOnlyDictionary<string, IReadOnlyList<string>> map, string className, params string[] preferredMarkers)
        {
            var key = NormalizeMatchKey(className);
            if (!map.TryGetValue(key, out var paths) || paths.Count == 0)
            {
                return null;
            }

            return paths
                .OrderByDescending(path => preferredMarkers.Any(marker => path.Contains(marker, StringComparison.OrdinalIgnoreCase)))
                .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }
    }

    private static string NormalizeMatchKey(string value)
    {
        return new string(value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());
    }
}
