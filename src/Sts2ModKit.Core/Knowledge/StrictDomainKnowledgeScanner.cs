using System.Text.Json;
using System.Text.RegularExpressions;

namespace Sts2ModKit.Core.Knowledge;

public static class StrictDomainKnowledgeScanner
{
    private static readonly Regex CardClassRegex = new(@"public\s+(?:abstract\s+|sealed\s+)?class\s+(?<name>[A-Za-z0-9_]+)\s*:\s*CardModel", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex RelicClassRegex = new(@"public\s+(?:abstract\s+|sealed\s+)?class\s+(?<name>[A-Za-z0-9_]+)\s*:\s*RelicModel", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PotionClassRegex = new(@"public\s+(?:abstract\s+|sealed\s+)?class\s+(?<name>[A-Za-z0-9_]+)\s*:\s*PotionModel", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex EventClassRegex = new(@"public\s+(?:abstract\s+|sealed\s+)?class\s+(?<name>[A-Za-z0-9_]+)\s*:\s*EventModel", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex RewardClassRegex = new(@"public\s+(?:abstract\s+|sealed\s+)?class\s+(?<name>[A-Za-z0-9_]+)\s*:\s*Reward\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex MerchantClassRegex = new(@"public\s+(?:abstract\s+|sealed\s+)?class\s+(?<name>[A-Za-z0-9_]+)\s*:\s*MerchantEntry\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PowerClassRegex = new(@"public\s+(?:abstract\s+|sealed\s+)?class\s+(?<name>[A-Za-z0-9_]+)\s*:\s*PowerModel\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex IntentClassRegex = new(@"public\s+(?:abstract\s+|sealed\s+)?class\s+(?<name>[A-Za-z0-9_]+)\s*:\s*(?:AbstractIntent|AttackIntent)\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex CardBaseRegex = new(@":\s*base\(\s*(?<cost>[^,\r\n]+?)\s*,\s*CardType\.(?<type>[A-Za-z0-9_]+)\s*,\s*CardRarity\.(?<rarity>[A-Za-z0-9_]+)\s*,\s*TargetType\.(?<target>[A-Za-z0-9_]+)\s*\)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
    private static readonly Regex DynamicVarRegex = new(@"new\s+(?<kind>[A-Za-z0-9_]+Var)(?:<(?<generic>[A-Za-z0-9_]+)>)?\(\s*(?<value>-?[0-9]+(?:\.[0-9]+)?m?)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex UpgradeRegex = new(@"base\.DynamicVars\.(?<name>[A-Za-z0-9_]+)\.UpgradeValueBy\((?<value>[^)]+)\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex TitlePropertyRegex = new(@"public\s+override\s+string\s+Title\s*=>\s*""(?<value>[^""]+)""", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex RelicRarityRegex = new(@"public\s+override\s+RelicRarity\s+Rarity\s*=>\s*RelicRarity\.(?<value>[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PotionRarityRegex = new(@"public\s+override\s+PotionRarity\s+Rarity\s*=>\s*PotionRarity\.(?<value>[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PotionUsageRegex = new(@"public\s+override\s+PotionUsage\s+Usage\s*=>\s*PotionUsage\.(?<value>[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PotionTargetRegex = new(@"public\s+override\s+TargetType\s+TargetType\s*=>\s*TargetType\.(?<value>[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex RewardTypeRegex = new(@"protected\s+override\s+RewardType\s+RewardType\s*=>\s*RewardType\.(?<value>[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex RewardsSetIndexRegex = new(@"public\s+override\s+int\s+RewardsSetIndex\s*=>\s*(?<value>[0-9]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PowerTypeRegex = new(@"public\s+override\s+PowerType\s+Type\s*=>\s*PowerType\.(?<value>[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex PowerStackTypeRegex = new(@"public\s+override\s+PowerStackType\s+StackType\s*=>\s*PowerStackType\.(?<value>[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex IntentTypeRegex = new(@"public\s+override\s+IntentType\s+IntentType\s*=>\s*IntentType\.(?<value>[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex LocStringRegex = new(@"new\s+LocString\(\s*""(?<table>[^""]+)""\s*,\s*""(?<key>[^""]+)""\s*\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex MerchantTalkPrefixRegex = new(@"GetLocStringsWithPrefix\(""(?<prefix>[^""]+)""\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex StaticImagePathRegex = new(@"ImageHelper\.GetImagePath\(""(?<value>[^""]+)""\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
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
        var shops = ParseShopEntries(decompiledRoot, localWarnings);
        var rewards = ParseRewardEntries(decompiledRoot, localWarnings);
        var keywords = ParseKeywordEntries(decompiledRoot, localWarnings);

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
                        ["strictShopCount"] = shops.Count.ToString(),
                        ["strictRewardCount"] = rewards.Count.ToString(),
                        ["strictKeywordCount"] = keywords.Count.ToString(),
                    })
                    .GroupBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.OrdinalIgnoreCase),
            },
            cards,
            relics,
            potions,
            events,
            shops,
            rewards,
            keywords);
    }

    private static string GetDomainDirectory(string decompiledRoot, string domain)
    {
        return Path.Combine(decompiledRoot, "MegaCrit", "Sts2", "Core", "Models", domain);
    }

    private static string GetCoreDirectory(string decompiledRoot, params string[] segments)
    {
        var parts = new List<string> { decompiledRoot, "MegaCrit", "Sts2", "Core" };
        parts.AddRange(segments);
        return Path.Combine(parts.ToArray());
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

    private static IReadOnlyList<StaticKnowledgeEntry> ParseShopEntries(string decompiledRoot, ICollection<string> warnings)
    {
        var entries = new List<StaticKnowledgeEntry>();
        var merchantDirectory = GetCoreDirectory(decompiledRoot, "Entities", "Merchant");
        if (!Directory.Exists(merchantDirectory))
        {
            warnings.Add($"Missing strict shop directory: {merchantDirectory}");
        }
        else
        {
            entries.AddRange(ParseDomainEntries(
                merchantDirectory,
                MerchantClassRegex,
                ParseMerchantEntry,
                warnings,
                ResourceIndex.Empty,
                poolMap: null));
        }

        var merchantRoomPath = GetCoreDirectory(decompiledRoot, "Rooms", "MerchantRoom.cs");
        if (File.Exists(merchantRoomPath))
        {
            var content = File.ReadAllText(merchantRoomPath);
            var entry = ParseMerchantRoomEntry(merchantRoomPath, content);
            if (entry is not null)
            {
                entries.Add(entry);
            }
        }

        return entries
            .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static StaticKnowledgeEntry? ParseMerchantEntry(
        string path,
        string content,
        string className,
        ResourceIndex _,
        IReadOnlyDictionary<string, string>? __)
    {
        if (string.Equals(className, "MerchantEntry", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var fullName = $"MegaCrit.Sts2.Core.Entities.Merchant.{className}";
        var attributes = CreateBaseAttributes(path, fullName, className, resourcePath: null);
        attributes["strictDomain"] = "shop";
        attributes["strictModel"] = "true";
        attributes["classId"] = ToLocalizationStem(className);

        var shopKind = className switch
        {
            "MerchantCardEntry" => "merchant-card-entry",
            "MerchantRelicEntry" => "merchant-relic-entry",
            "MerchantPotionEntry" => "merchant-potion-entry",
            "MerchantCardRemovalEntry" => "merchant-card-removal-service",
            _ => "merchant-entry",
        };
        attributes["shopKind"] = shopKind;

        switch (className)
        {
            case "MerchantCardEntry":
                attributes["priceRule"] = "Common 50 / Uncommon 75 / Rare 150, colorless x1.15, sale 50% off, final merchant RNG 0.95-1.05.";
                attributes["summary"] = "상점에서 카드 구매 슬롯을 담당합니다. 캐릭터 카드 5장과 무색 카드 2장을 채우고, 할인 상품 한 칸을 반값으로 표시합니다.";
                break;
            case "MerchantRelicEntry":
                attributes["priceRule"] = "Relic merchant cost x merchant RNG 0.85-1.15.";
                attributes["summary"] = "상점 유물 슬롯입니다. 일반/희귀 유물 2개와 상점 전용 유물 1개를 채웁니다.";
                break;
            case "MerchantPotionEntry":
                attributes["priceRule"] = "Common 50 / Uncommon 75 / Rare 100, final merchant RNG 0.95-1.05.";
                attributes["summary"] = "상점 포션 슬롯입니다. 전투 외 무작위 포션 3개를 배치합니다.";
                break;
            case "MerchantCardRemovalEntry":
                attributes["priceRule"] = "Base 75, +25 per prior card removal in the run.";
                attributes["summary"] = "덱에서 카드 1장을 제거하는 상점 서비스입니다. 사용 횟수가 늘수록 비용이 상승합니다.";
                attributes["l10nKey"] = "MERCHANT";
                attributes["matchKeys"] = "MERCHANT";
                break;
        }

        return new StaticKnowledgeEntry(
            NormalizeId(fullName),
            className switch
            {
                "MerchantCardEntry" => "카드 판매 슬롯",
                "MerchantRelicEntry" => "유물 판매 슬롯",
                "MerchantPotionEntry" => "포션 판매 슬롯",
                "MerchantCardRemovalEntry" => "카드 제거 서비스",
                _ => PrettifyClassName(className),
            },
            "strict-domain-parse",
            false,
            null,
            new[] { "strict-shop", "strict-model" },
            attributes,
            Array.Empty<StaticKnowledgeOption>());
    }

    private static StaticKnowledgeEntry? ParseMerchantRoomEntry(string path, string content)
    {
        const string className = "MerchantRoom";
        var fullName = "MegaCrit.Sts2.Core.Rooms.MerchantRoom";
        var attributes = CreateBaseAttributes(path, fullName, className, resourcePath: null);
        attributes["strictDomain"] = "shop";
        attributes["strictModel"] = "true";
        attributes["classId"] = ToLocalizationStem(className);
        attributes["shopKind"] = "merchant-room";
        attributes["roomType"] = "Shop";
        attributes["summary"] = "상점 방 자체를 나타냅니다. 진입 시 MerchantInventory를 생성하고 상점 UI와 대사를 준비합니다.";
        attributes["matchKeys"] = "ROOM_MERCHANT | ROOM_UNKNOWN_MERCHANT";
        attributes["l10nKey"] = "ROOM_MERCHANT";

        var talkPrefix = MerchantTalkPrefixRegex.Match(content).Groups["prefix"].Value;
        if (!string.IsNullOrWhiteSpace(talkPrefix))
        {
            attributes["talkPrefix"] = talkPrefix;
        }

        return new StaticKnowledgeEntry(
            NormalizeId(fullName),
            PrettifyClassName(className),
            "strict-domain-parse",
            false,
            null,
            new[] { "strict-shop", "strict-model", "room" },
            attributes,
            Array.Empty<StaticKnowledgeOption>());
    }

    private static IReadOnlyList<StaticKnowledgeEntry> ParseRewardEntries(string decompiledRoot, ICollection<string> warnings)
    {
        var entries = new List<StaticKnowledgeEntry>();
        var rewardDirectory = GetCoreDirectory(decompiledRoot, "Rewards");
        if (!Directory.Exists(rewardDirectory))
        {
            warnings.Add($"Missing strict reward directory: {rewardDirectory}");
        }
        else
        {
            entries.AddRange(ParseDomainEntries(
                rewardDirectory,
                RewardClassRegex,
                ParseRewardEntry,
                warnings,
                ResourceIndex.Empty,
                poolMap: null));

            var linkedRewardSetPath = Path.Combine(rewardDirectory, "LinkedRewardSet.cs");
            if (File.Exists(linkedRewardSetPath))
            {
                var entry = ParseLinkedRewardSetEntry(linkedRewardSetPath, File.ReadAllText(linkedRewardSetPath));
                if (entry is not null)
                {
                    entries.Add(entry);
                }
            }
        }

        return entries
            .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static StaticKnowledgeEntry? ParseRewardEntry(
        string path,
        string content,
        string className,
        ResourceIndex _,
        IReadOnlyDictionary<string, string>? __)
    {
        if (string.Equals(className, "Reward", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var fullName = $"MegaCrit.Sts2.Core.Rewards.{className}";
        var attributes = CreateBaseAttributes(path, fullName, className, ExtractFirstImagePath(content));
        attributes["strictDomain"] = "reward";
        attributes["strictModel"] = "true";
        attributes["classId"] = ToLocalizationStem(className);

        var rewardType = RewardTypeRegex.Match(content).Groups["value"].Value;
        if (!string.IsNullOrWhiteSpace(rewardType))
        {
            attributes["rewardType"] = rewardType;
        }

        var rewardsSetIndex = RewardsSetIndexRegex.Match(content).Groups["value"].Value;
        if (!string.IsNullOrWhiteSpace(rewardsSetIndex))
        {
            attributes["rewardsSetIndex"] = rewardsSetIndex;
        }

        var locKeys = LocStringRegex.Matches(content)
            .Select(match => match.Groups["key"].Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var alias in GetRewardAliases(className))
        {
            if (!locKeys.Contains(alias, StringComparer.OrdinalIgnoreCase))
            {
                locKeys.Add(alias);
            }
        }

        if (locKeys.Count > 0)
        {
            attributes["l10nKey"] = locKeys[0];
            attributes["matchKeys"] = string.Join(" | ", locKeys);
        }

        attributes["summary"] = className switch
        {
            "CardReward" => "전투/이벤트 이후 카드 보상 팩을 제시합니다.",
            "GoldReward" => "골드 보상을 제시합니다. 훔쳐간 골드를 되찾는 변형도 포함됩니다.",
            "PotionReward" => "포션 보상을 제시합니다.",
            "RelicReward" => "유물 보상을 제시합니다.",
            "CardRemovalReward" => "보상 화면에서 카드 제거 서비스를 제시합니다.",
            "SpecialCardReward" => "특수 카드 단일 보상을 제시합니다.",
            _ => "보상 화면에서 선택 가능한 보상 모델입니다.",
        };

        return new StaticKnowledgeEntry(
            NormalizeId(fullName),
            className switch
            {
                "CardReward" => "카드 보상",
                "GoldReward" => "골드 보상",
                "PotionReward" => "포션 보상",
                "RelicReward" => "유물 보상",
                "CardRemovalReward" => "카드 제거 보상",
                "SpecialCardReward" => "특수 카드 보상",
                _ => PrettifyClassName(className),
            },
            "strict-domain-parse",
            false,
            null,
            new[] { "strict-reward", "strict-model" },
            attributes,
            Array.Empty<StaticKnowledgeOption>());
    }

    private static StaticKnowledgeEntry? ParseLinkedRewardSetEntry(string path, string content)
    {
        var fullName = "MegaCrit.Sts2.Core.Rewards.LinkedRewardSet";
        var attributes = CreateBaseAttributes(path, fullName, "LinkedRewardSet", ExtractFirstImagePath(content));
        attributes["strictDomain"] = "reward";
        attributes["strictModel"] = "true";
        attributes["classId"] = "LINKED_REWARD_SET";
        attributes["rewardType"] = "LinkedSet";
        attributes["l10nKey"] = "LINKED_REWARDS";
        attributes["matchKeys"] = "LINKED_REWARDS";
        attributes["summary"] = "여러 보상을 묶어 한 세트에서 하나만 고르게 하는 연결 보상 집합입니다.";

        return new StaticKnowledgeEntry(
            NormalizeId(fullName),
            "연결 보상 세트",
            "strict-domain-parse",
            false,
            null,
            new[] { "strict-reward", "strict-model", "reward-set" },
            attributes,
            Array.Empty<StaticKnowledgeOption>());
    }

    private static IReadOnlyList<StaticKnowledgeEntry> ParseKeywordEntries(string decompiledRoot, ICollection<string> warnings)
    {
        var entries = new List<StaticKnowledgeEntry>();
        var powersDirectory = GetCoreDirectory(decompiledRoot, "Models", "Powers");
        if (!Directory.Exists(powersDirectory))
        {
            warnings.Add($"Missing strict keyword powers directory: {powersDirectory}");
        }
        else
        {
            entries.AddRange(ParseDomainEntries(
                powersDirectory,
                PowerClassRegex,
                ParsePowerEntry,
                warnings,
                ResourceIndex.Empty,
                poolMap: null));
        }

        var intentsDirectory = GetCoreDirectory(decompiledRoot, "MonsterMoves", "Intents");
        if (!Directory.Exists(intentsDirectory))
        {
            warnings.Add($"Missing strict keyword intents directory: {intentsDirectory}");
        }
        else
        {
            entries.AddRange(ParseDomainEntries(
                intentsDirectory,
                IntentClassRegex,
                ParseIntentEntry,
                warnings,
                ResourceIndex.Empty,
                poolMap: null));
        }

        var cardKeywordPath = GetCoreDirectory(decompiledRoot, "Entities", "Cards", "CardKeyword.cs");
        if (File.Exists(cardKeywordPath))
        {
            entries.AddRange(ParseCardKeywordEntries(cardKeywordPath));
        }

        return entries
            .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static StaticKnowledgeEntry? ParsePowerEntry(
        string path,
        string content,
        string className,
        ResourceIndex _,
        IReadOnlyDictionary<string, string>? __)
    {
        if (string.Equals(className, "PowerModel", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var fullName = $"MegaCrit.Sts2.Core.Models.Powers.{className}";
        var attributes = CreateBaseAttributes(path, fullName, className, ExtractFirstImagePath(content));
        attributes["strictDomain"] = "keyword";
        attributes["strictModel"] = "true";
        attributes["keywordKind"] = "power";
        attributes["classId"] = ToLocalizationStem(className);
        attributes["l10nKey"] = ToLocalizationStem(className);
        attributes["matchKeys"] = ToLocalizationStem(className);

        var powerType = PowerTypeRegex.Match(content).Groups["value"].Value;
        if (!string.IsNullOrWhiteSpace(powerType))
        {
            attributes["powerType"] = powerType;
        }

        var stackType = PowerStackTypeRegex.Match(content).Groups["value"].Value;
        if (!string.IsNullOrWhiteSpace(stackType))
        {
            attributes["stackType"] = stackType;
        }

        var powerSummaryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(powerType))
        {
            powerSummaryParts.Add(powerType.Equals("Debuff", StringComparison.OrdinalIgnoreCase)
                ? "전투 중 디버프성 상태를 나타냅니다."
                : "전투 중 버프/지속 효과를 나타냅니다.");
        }

        if (!string.IsNullOrWhiteSpace(stackType))
        {
            powerSummaryParts.Add($"스택 규칙은 {stackType}입니다.");
        }

        attributes["summary"] = powerSummaryParts.Count == 0
            ? $"{PrettifyClassName(className)} 파워 모델입니다."
            : $"{PrettifyClassName(className)} 파워 모델입니다. {string.Join(" ", powerSummaryParts)}";

        return new StaticKnowledgeEntry(
            NormalizeId(fullName),
            PrettifyClassName(className),
            "strict-domain-parse",
            false,
            null,
            new[] { "strict-keyword", "strict-model", "power" },
            attributes,
            Array.Empty<StaticKnowledgeOption>());
    }

    private static StaticKnowledgeEntry? ParseIntentEntry(
        string path,
        string content,
        string className,
        ResourceIndex _,
        IReadOnlyDictionary<string, string>? __)
    {
        if (string.Equals(className, "AbstractIntent", StringComparison.OrdinalIgnoreCase)
            || string.Equals(className, "AttackIntent", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var fullName = $"MegaCrit.Sts2.Core.MonsterMoves.Intents.{className}";
        var attributes = CreateBaseAttributes(path, fullName, className, ExtractFirstImagePath(content));
        attributes["strictDomain"] = "keyword";
        attributes["strictModel"] = "true";
        attributes["keywordKind"] = "intent";
        attributes["classId"] = ToLocalizationStem(className);
        attributes["l10nKey"] = ToLocalizationStem(className);
        attributes["matchKeys"] = ToLocalizationStem(className);

        var intentType = IntentTypeRegex.Match(content).Groups["value"].Value;
        if (!string.IsNullOrWhiteSpace(intentType))
        {
            attributes["intentType"] = intentType;
        }

        attributes["summary"] = className switch
        {
            "BuffIntent" => "몬스터가 자신이나 아군에게 버프를 적용하려는 의도입니다.",
            "CardDebuffIntent" => "몬스터가 상태 이상 카드나 핸드 교란성 디버프를 유발하려는 의도입니다.",
            "DebuffIntent" => "몬스터가 약화, 취약, 힘 감소 같은 디버프를 걸려는 의도입니다.",
            "DefendIntent" => "몬스터가 방어도나 보호 수단을 준비하는 의도입니다.",
            "EscapeIntent" => "몬스터가 전투 이탈 또는 도주 행동을 준비하는 의도입니다.",
            "HealIntent" => "몬스터가 체력을 회복하려는 의도입니다.",
            "HiddenIntent" => "게임이 정확한 행동을 숨기고 있는 의도 상태입니다.",
            "MultiAttackIntent" => "몬스터가 여러 번 적중하는 연속 공격을 준비하는 의도입니다.",
            "SingleAttackIntent" => "몬스터가 단일 타격 공격을 준비하는 의도입니다.",
            "SleepIntent" => "몬스터가 잠들었거나 대기 상태임을 나타내는 의도입니다.",
            "StatusIntent" => "몬스터가 상태 카드 부여나 상태 이상 유발 행동을 준비하는 의도입니다.",
            "StunIntent" => "몬스터가 기절 또는 행동 불능 상태임을 나타내는 의도입니다.",
            "SummonIntent" => "몬스터가 다른 적이나 소환물을 불러내려는 의도입니다.",
            "UnknownIntent" => "현재 파서가 구체 행동을 해석하지 못한 일반 의도입니다.",
            _ => "몬스터 행동 아이콘과 intent 분류를 설명하는 모델입니다.",
        };

        return new StaticKnowledgeEntry(
            NormalizeId(fullName),
            className switch
            {
                "BuffIntent" => "버프 의도",
                "CardDebuffIntent" => "상태 카드 디버프 의도",
                "DebuffIntent" => "디버프 의도",
                "DefendIntent" => "방어 의도",
                "EscapeIntent" => "도주 의도",
                "HealIntent" => "회복 의도",
                "HiddenIntent" => "숨김 의도",
                "MultiAttackIntent" => "연속 공격 의도",
                "SingleAttackIntent" => "단일 공격 의도",
                "SleepIntent" => "수면 의도",
                "StatusIntent" => "상태 이상 의도",
                "StunIntent" => "기절 의도",
                "SummonIntent" => "소환 의도",
                "UnknownIntent" => "알 수 없는 의도",
                _ => PrettifyClassName(className),
            },
            "strict-domain-parse",
            false,
            null,
            new[] { "strict-keyword", "strict-model", "intent" },
            attributes,
            Array.Empty<StaticKnowledgeOption>());
    }

    private static IReadOnlyList<StaticKnowledgeEntry> ParseCardKeywordEntries(string path)
    {
        var content = File.ReadAllText(path);
        var enumBody = content[(content.IndexOf('{') + 1)..content.LastIndexOf('}')];
        var values = enumBody
            .Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Where(value => value != "None")
            .Select(value => value.Split('=', StringSplitOptions.TrimEntries)[0].Trim())
            .Where(value => !ShouldSkipType(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return values
            .Select(value =>
            {
                var attributes = CreateBaseAttributes(path, "MegaCrit.Sts2.Core.Entities.Cards.CardKeyword", value, resourcePath: null);
                attributes["strictDomain"] = "keyword";
                attributes["strictModel"] = "true";
                attributes["keywordKind"] = "card-keyword";
                attributes["classId"] = ToLocalizationStem(value);
                attributes["l10nKey"] = ToLocalizationStem(value);
                attributes["matchKeys"] = ToLocalizationStem(value);
                attributes["summary"] = "카드 본문에서 반복되는 규칙성 키워드입니다.";

                return new StaticKnowledgeEntry(
                    NormalizeId($"MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.{value}"),
                    PrettifyClassName(value),
                    "strict-domain-parse",
                    false,
                    null,
                    new[] { "strict-keyword", "strict-model", "card-keyword" },
                    attributes,
                    Array.Empty<StaticKnowledgeOption>());
            })
            .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
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

    private static string? ExtractFirstImagePath(string content)
    {
        return StaticImagePathRegex.Matches(content)
            .Select(match => match.Groups["value"].Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static IEnumerable<string> GetRewardAliases(string className)
    {
        return className switch
        {
            "CardReward" => new[] { "CARD_REWARD", "COMBAT_REWARD_ADD_CARD" },
            "GoldReward" => new[] { "COMBAT_REWARD_GOLD", "COMBAT_REWARD_GOLD_STOLEN" },
            "PotionReward" => new[] { "POTION_REWARD" },
            "RelicReward" => new[] { "RELIC_REWARD" },
            "CardRemovalReward" => new[] { "COMBAT_REWARD_CARD_REMOVAL" },
            "SpecialCardReward" => new[] { "SPECIAL_CARD_REWARD" },
            _ => Array.Empty<string>(),
        };
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
        public static ResourceIndex Empty { get; } = new(
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase));

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
