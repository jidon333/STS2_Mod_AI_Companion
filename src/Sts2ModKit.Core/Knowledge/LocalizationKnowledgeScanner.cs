using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Sts2ModKit.Core.Knowledge;

public static class LocalizationKnowledgeScanner
{
    private const int ByteBufferSize = 2 * 1024 * 1024;
    private const int ProcessThresholdChars = 2_000_000;
    private const int TailOverlapChars = 65_536;

    private static readonly Regex LocalizationPathRegex = new(
        @"localization/(?<locale>[a-z]{3})/(?<file>[^""'\s]+?\.json)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex LocalizationEntryRegex = new(
        "\"(?<key>[A-Za-z0-9_]+(?:\\.[A-Za-z0-9_]+)+)\"\\s*:\\s*\"(?<value>(?:\\\\.|[^\"\\\\])*)\"",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static StaticKnowledgeLocalizationScan Scan(
        string pckPath,
        StaticKnowledgeCatalog seedCatalog,
        out IReadOnlyList<string> warnings)
    {
        var localWarnings = new List<string>();
        if (!File.Exists(pckPath))
        {
            localWarnings.Add($"Missing localization scan target: {pckPath}");
            warnings = localWarnings;
            return CreateEmpty(pckPath, localWarnings);
        }

        var seeds = BuildSeeds(seedCatalog);
        var decoder = Encoding.UTF8.GetDecoder();
        var bytes = new byte[ByteBufferSize];
        var chars = new char[Encoding.UTF8.GetMaxCharCount(ByteBufferSize)];
        var pending = new StringBuilder(ProcessThresholdChars + TailOverlapChars);
        var state = new ScanState();
        var records = new ScanRecords();
        var totalMatches = 0;

        using var stream = new FileStream(
            pckPath,
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.Read,
            });

        while (true)
        {
            var bytesRead = stream.Read(bytes, 0, bytes.Length);
            if (bytesRead == 0)
            {
                break;
            }

            decoder.Convert(bytes, 0, bytesRead, chars, 0, chars.Length, flush: false, out _, out var charsUsed, out _);
            pending.Append(chars, 0, charsUsed);
            if (pending.Length < ProcessThresholdChars)
            {
                continue;
            }

            var processLength = Math.Max(0, pending.Length - TailOverlapChars);
            totalMatches += ProcessChunk(pending.ToString(0, processLength), state, records, seeds);
            pending.Remove(0, processLength);
        }

        decoder.Convert(Array.Empty<byte>(), 0, 0, chars, 0, chars.Length, flush: true, out _, out var finalCharsUsed, out _);
        if (finalCharsUsed > 0)
        {
            pending.Append(chars, 0, finalCharsUsed);
        }

        if (pending.Length > 0)
        {
            totalMatches += ProcessChunk(pending.ToString(), state, records, seeds);
        }

        var cards = records.Cards.Values.Select(record => record.ToCardImmutable()).OrderBy(record => record.Title ?? record.KeyStem, StringComparer.OrdinalIgnoreCase).ToArray();
        var relics = records.Relics.Values.Select(record => record.ToImmutable("relic")).OrderBy(record => record.Title ?? record.KeyStem, StringComparer.OrdinalIgnoreCase).ToArray();
        var potions = records.Potions.Values.Select(record => record.ToImmutable("potion")).OrderBy(record => record.Title ?? record.KeyStem, StringComparer.OrdinalIgnoreCase).ToArray();
        var events = records.Events.Values.Select(record => record.ToImmutable()).OrderBy(record => record.Title ?? record.KeyStem, StringComparer.OrdinalIgnoreCase).ToArray();
        var shops = records.Shops.Values.Select(record => record.ToImmutable("shop")).OrderBy(record => record.Title ?? record.KeyStem, StringComparer.OrdinalIgnoreCase).ToArray();
        var rewards = records.Rewards.Values.Select(record => record.ToImmutable("reward")).OrderBy(record => record.Title ?? record.KeyStem, StringComparer.OrdinalIgnoreCase).ToArray();
        var keywords = records.Keywords.Values.Select(record => record.ToImmutable("keyword")).OrderBy(record => record.Title ?? record.KeyStem, StringComparer.OrdinalIgnoreCase).ToArray();

        if (cards.Length == 0)
        {
            localWarnings.Add("No card localization strings were recovered from the PCK scan.");
        }

        var stats = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["mode"] = "utf8-streaming-string-scan",
            ["rawMatchCount"] = totalMatches.ToString(),
            ["cards"] = cards.Length.ToString(),
            ["cardTitles"] = cards.Count(entry => !string.IsNullOrWhiteSpace(entry.Title)).ToString(),
            ["cardDescriptions"] = cards.Count(entry => !string.IsNullOrWhiteSpace(entry.Description)).ToString(),
            ["cardSelectionPrompts"] = cards.Count(entry => !string.IsNullOrWhiteSpace(entry.SelectionScreenPrompt)).ToString(),
            ["cardKoreanPreferred"] = cards.Count(entry => string.Equals(entry.PreferredLocale, "kor", StringComparison.OrdinalIgnoreCase)).ToString(),
            ["cardEnglishPreferred"] = cards.Count(entry => string.Equals(entry.PreferredLocale, "eng", StringComparison.OrdinalIgnoreCase)).ToString(),
            ["relics"] = relics.Length.ToString(),
            ["relicDescriptions"] = relics.Count(entry => !string.IsNullOrWhiteSpace(entry.Description)).ToString(),
            ["potions"] = potions.Length.ToString(),
            ["potionDescriptions"] = potions.Count(entry => !string.IsNullOrWhiteSpace(entry.Description)).ToString(),
            ["events"] = events.Length.ToString(),
            ["eventDescriptions"] = events.Count(entry => !string.IsNullOrWhiteSpace(entry.Description)).ToString(),
            ["eventOptions"] = events.Sum(entry => entry.Options.Count).ToString(),
            ["shops"] = shops.Length.ToString(),
            ["shopDescriptions"] = shops.Count(entry => !string.IsNullOrWhiteSpace(entry.Description)).ToString(),
            ["rewards"] = rewards.Length.ToString(),
            ["rewardDescriptions"] = rewards.Count(entry => !string.IsNullOrWhiteSpace(entry.Description)).ToString(),
            ["keywords"] = keywords.Length.ToString(),
            ["keywordDescriptions"] = keywords.Count(entry => !string.IsNullOrWhiteSpace(entry.Description)).ToString(),
        };

        warnings = localWarnings;
        return new StaticKnowledgeLocalizationScan(
            DateTimeOffset.UtcNow,
            pckPath,
            cards,
            relics,
            potions,
            events,
            shops,
            rewards,
            keywords,
            stats,
            localWarnings);
    }

    private static StaticKnowledgeLocalizationScan CreateEmpty(string pckPath, IReadOnlyList<string> warnings)
    {
        return new StaticKnowledgeLocalizationScan(
            DateTimeOffset.UtcNow,
            pckPath,
            Array.Empty<StaticKnowledgeLocalizationCardEntry>(),
            Array.Empty<StaticKnowledgeLocalizationEntry>(),
            Array.Empty<StaticKnowledgeLocalizationEntry>(),
            Array.Empty<StaticKnowledgeLocalizationEntry>(),
            Array.Empty<StaticKnowledgeLocalizationEntry>(),
            Array.Empty<StaticKnowledgeLocalizationEntry>(),
            Array.Empty<StaticKnowledgeLocalizationEntry>(),
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase),
            warnings);
    }

    private static int ProcessChunk(string text, ScanState state, ScanRecords records, SeedSets seeds)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var tokens = new List<ScanToken>();
        foreach (Match match in LocalizationPathRegex.Matches(text))
        {
            tokens.Add(new ScanToken(match.Index, "path", match.Groups["locale"].Value.ToLowerInvariant(), match.Groups["file"].Value.Replace('\\', '/'), null, null));
        }

        foreach (Match match in LocalizationEntryRegex.Matches(text))
        {
            tokens.Add(new ScanToken(match.Index, "entry", null, null, match.Groups["key"].Value, UnescapeJsonString(match.Groups["value"].Value)));
        }

        if (tokens.Count == 0)
        {
            return 0;
        }

        var matches = 0;
        foreach (var token in tokens.OrderBy(token => token.Index).ThenBy(token => token.Kind, StringComparer.Ordinal))
        {
            if (string.Equals(token.Kind, "path", StringComparison.Ordinal))
            {
                state.CurrentLocale = token.Locale;
                state.CurrentFileHint = $"localization/{token.Locale}/{token.FileName}";
                continue;
            }

            if (string.IsNullOrWhiteSpace(token.Key) || string.IsNullOrWhiteSpace(token.Value))
            {
                continue;
            }

            var fileName = GetFileName(state.CurrentFileHint);
            var domain = ClassifyEntry(token.Key!, fileName, seeds);
            if (domain is null)
            {
                continue;
            }

            var locale = InferLocale(token.Value!, state.CurrentLocale, state.CurrentFileHint);
            var fileHint = string.IsNullOrWhiteSpace(state.CurrentFileHint)
                ? $"localization/{locale}/{fileName}"
                : state.CurrentFileHint!;

            if (ProcessEntry(domain, token.Key!, token.Value!, locale, fileHint, records))
            {
                matches += 1;
            }
        }

        return matches;
    }

    private static bool ProcessEntry(string domain, string key, string value, string locale, string fileHint, ScanRecords records)
    {
        return domain switch
        {
            "card" => ProcessCardEntry(key, value, locale, fileHint, records.Cards),
            "relic" => ProcessSimpleEntry("relic", key, value, locale, fileHint, records.Relics),
            "potion" => ProcessSimpleEntry("potion", key, value, locale, fileHint, records.Potions),
            "event" => ProcessEventEntry(key, value, locale, fileHint, records.Events),
            "shop" => ProcessSimpleEntry("shop", key, value, locale, fileHint, records.Shops),
            "reward" => ProcessSimpleEntry("reward", key, value, locale, fileHint, records.Rewards),
            "keyword" => ProcessSimpleEntry("keyword", key, value, locale, fileHint, records.Keywords),
            _ => false,
        };
    }

    private static bool ProcessCardEntry(string key, string value, string locale, string fileHint, IDictionary<string, MutableCardRecord> records)
    {
        var segments = key.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            return false;
        }

        var field = segments[^1];
        if (!field.Equals("title", StringComparison.OrdinalIgnoreCase)
            && !field.Equals("description", StringComparison.OrdinalIgnoreCase)
            && !field.Equals("selectionScreenPrompt", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var normalizedStem = NormalizeMatchKey(segments[0]);
        if (!records.TryGetValue(normalizedStem, out var record))
        {
            record = new MutableCardRecord(segments[0]);
            records[normalizedStem] = record;
        }

        record.Apply(locale, field, value, fileHint);
        return true;
    }

    private static bool ProcessSimpleEntry(string domain, string key, string value, string locale, string fileHint, IDictionary<string, MutableLocalizationRecord> records)
    {
        var segments = key.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            return false;
        }

        var normalizedStem = NormalizeMatchKey(segments[0]);
        if (!records.TryGetValue(normalizedStem, out var record))
        {
            record = new MutableLocalizationRecord(domain, segments[0]);
            records[normalizedStem] = record;
        }

        var field = segments[^1];
        if (field.Equals("title", StringComparison.OrdinalIgnoreCase)
            || field.Equals("description", StringComparison.OrdinalIgnoreCase)
            || field.Equals("flavor", StringComparison.OrdinalIgnoreCase)
            || field.Equals("selectionScreenPrompt", StringComparison.OrdinalIgnoreCase)
            || field.Equals("unlockInfo", StringComparison.OrdinalIgnoreCase))
        {
            record.ApplyField(locale, field, value, fileHint);
            return true;
        }

        if (field.StartsWith("line", StringComparison.OrdinalIgnoreCase)
            || (domain is "shop" or "reward" && segments.Length >= 3))
        {
            record.ApplyNote(locale, value, fileHint);
            return true;
        }

        return false;
    }

    private static bool ProcessEventEntry(string key, string value, string locale, string fileHint, IDictionary<string, MutableEventRecord> records)
    {
        var segments = key.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            return false;
        }

        var normalizedStem = NormalizeMatchKey(segments[0]);
        if (!records.TryGetValue(normalizedStem, out var record))
        {
            record = new MutableEventRecord(segments[0]);
            records[normalizedStem] = record;
        }

        if (segments.Length == 2 && segments[1].Equals("title", StringComparison.OrdinalIgnoreCase))
        {
            record.ApplyField(locale, "title", value, fileHint);
            return true;
        }

        if (segments.Length == 2 && segments[1].Equals("description", StringComparison.OrdinalIgnoreCase))
        {
            record.ApplyField(locale, "description", value, fileHint);
            return true;
        }

        if (segments.Length == 4
            && segments[1].Equals("pages", StringComparison.OrdinalIgnoreCase)
            && segments[3].Equals("description", StringComparison.OrdinalIgnoreCase))
        {
            record.ApplyPageDescription(locale, segments[2], value, fileHint);
            return true;
        }

        if (segments.Length == 6
            && segments[1].Equals("pages", StringComparison.OrdinalIgnoreCase)
            && segments[3].Equals("options", StringComparison.OrdinalIgnoreCase)
            && (segments[5].Equals("title", StringComparison.OrdinalIgnoreCase)
                || segments[5].Equals("description", StringComparison.OrdinalIgnoreCase)))
        {
            record.ApplyOption(locale, segments[2], segments[4], segments[5], value, fileHint);
            return true;
        }

        if (segments.Length >= 4
            && segments[1].Equals("talk", StringComparison.OrdinalIgnoreCase)
            && segments[^1].StartsWith("line", StringComparison.OrdinalIgnoreCase))
        {
            record.ApplyTalk(locale, value, fileHint);
            return true;
        }

        if (segments[^1].Equals("description", StringComparison.OrdinalIgnoreCase))
        {
            record.ApplyNote(locale, value, fileHint);
            return true;
        }

        return false;
    }

    private static string GetFileName(string? currentFileHint)
    {
        return string.IsNullOrWhiteSpace(currentFileHint)
            ? string.Empty
            : Path.GetFileName(currentFileHint.Replace('\\', '/'));
    }

    private static string? ClassifyEntry(string key, string fileNameHint, SeedSets seeds)
    {
        var segments = key.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            return null;
        }

        var field = segments[^1];
        var normalizedStem = NormalizeMatchKey(segments[0]);

        if (key.Contains(".pages.", StringComparison.OrdinalIgnoreCase)
            || key.Contains(".talk.", StringComparison.OrdinalIgnoreCase)
            || key.EndsWith(".loss", StringComparison.OrdinalIgnoreCase)
            || key.EndsWith(".win", StringComparison.OrdinalIgnoreCase)
            || key.EndsWith(".customRewardDescription", StringComparison.OrdinalIgnoreCase)
            || seeds.Events.Contains(normalizedStem))
        {
            return "event";
        }

        if (field.Equals("selectionScreenPrompt", StringComparison.OrdinalIgnoreCase)
            && seeds.Cards.Contains(normalizedStem))
        {
            return "card";
        }

        if (LooksLikePotionKey(key, normalizedStem))
        {
            return "potion";
        }

        if (LooksLikeKeywordKey(key, normalizedStem))
        {
            return "keyword";
        }

        if (LooksLikeShopKey(key, normalizedStem))
        {
            return "shop";
        }

        if (LooksLikeRewardKey(key, normalizedStem))
        {
            return "reward";
        }

        if (seeds.Cards.Contains(normalizedStem))
        {
            return "card";
        }

        if (seeds.Relics.Contains(normalizedStem))
        {
            return "relic";
        }

        if (seeds.Potions.Contains(normalizedStem))
        {
            return "potion";
        }

        if (seeds.Keywords.Contains(normalizedStem))
        {
            return "keyword";
        }

        if (seeds.Shops.Contains(normalizedStem))
        {
            return "shop";
        }

        if (seeds.Rewards.Contains(normalizedStem))
        {
            return "reward";
        }

        if (fileNameHint.Equals("merchant_room.json", StringComparison.OrdinalIgnoreCase)
            || fileNameHint.Equals("rest_site_ui.json", StringComparison.OrdinalIgnoreCase))
        {
            return "shop";
        }

        if (fileNameHint.Equals("card_reward_ui.json", StringComparison.OrdinalIgnoreCase)
            || fileNameHint.Equals("card_selection.json", StringComparison.OrdinalIgnoreCase))
        {
            return "reward";
        }

        if (fileNameHint.Equals("cards.json", StringComparison.OrdinalIgnoreCase)
            || fileNameHint.Equals("card_library.json", StringComparison.OrdinalIgnoreCase))
        {
            return seeds.Cards.Contains(normalizedStem) ? "card" : null;
        }

        if (fileNameHint.Equals("relics.json", StringComparison.OrdinalIgnoreCase)
            || fileNameHint.Equals("relic_collection.json", StringComparison.OrdinalIgnoreCase)
            || fileNameHint.Equals("inspect_relic_screen.json", StringComparison.OrdinalIgnoreCase))
        {
            return seeds.Relics.Contains(normalizedStem) ? "relic" : null;
        }

        if (fileNameHint.Equals("potions.json", StringComparison.OrdinalIgnoreCase)
            || fileNameHint.Equals("potion_lab.json", StringComparison.OrdinalIgnoreCase))
        {
            return seeds.Potions.Contains(normalizedStem) ? "potion" : null;
        }

        if (fileNameHint.Equals("card_keywords.json", StringComparison.OrdinalIgnoreCase)
            || fileNameHint.Equals("intents.json", StringComparison.OrdinalIgnoreCase))
        {
            return "keyword";
        }

        if (key.Contains("MERCHANT", StringComparison.OrdinalIgnoreCase)
            || key.Contains("SHOP", StringComparison.OrdinalIgnoreCase)
            || key.Contains("REST", StringComparison.OrdinalIgnoreCase))
        {
            return "shop";
        }

        if (key.Contains("REWARD", StringComparison.OrdinalIgnoreCase)
            || key.Contains("CARD_SELECTION", StringComparison.OrdinalIgnoreCase))
        {
            return "reward";
        }

        return null;
    }

    private static SeedSets BuildSeeds(StaticKnowledgeCatalog seedCatalog)
    {
        var allEntries = seedCatalog.Cards
            .Concat(seedCatalog.Relics)
            .Concat(seedCatalog.Potions)
            .Concat(seedCatalog.Events)
            .Concat(seedCatalog.Shops)
            .Concat(seedCatalog.Rewards)
            .Concat(seedCatalog.Keywords)
            .ToArray();

        return new SeedSets(
            BuildSeedSet(allEntries, "card"),
            BuildSeedSet(allEntries, "relic"),
            BuildSeedSet(allEntries, "potion"),
            BuildSeedSet(allEntries, "event"),
            BuildSeedSet(allEntries, "shop"),
            BuildSeedSet(allEntries, "reward"),
            BuildSeedSet(allEntries, "keyword"));
    }

    private static HashSet<string> BuildSeedSet(IEnumerable<StaticKnowledgeEntry> entries, string domain)
    {
        var seeds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries)
        {
            if (!MatchesDomainSeed(entry, domain))
            {
                continue;
            }

            AddSeed(seeds, entry.Name);
            AddSeed(seeds, entry.Id);
            AddSeed(seeds, ReadAttribute(entry, "l10nKey"));

            if (entry.Attributes.TryGetValue("fullName", out var fullName) && !string.IsNullOrWhiteSpace(fullName))
            {
                AddSeed(seeds, fullName.Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault());
            }

            if (entry.Attributes.TryGetValue("resourcePath", out var resourcePath) && !string.IsNullOrWhiteSpace(resourcePath))
            {
                var normalizedPath = resourcePath.Replace('\\', '/');
                AddSeed(seeds, Path.GetFileNameWithoutExtension(normalizedPath));
            }
        }

        return seeds;
    }

    private static bool MatchesDomainSeed(StaticKnowledgeEntry entry, string domain)
    {
        var fullName = ReadAttribute(entry, "fullName");
        var resourcePath = ReadAttribute(entry, "resourcePath");
        var l10nDomain = ReadAttribute(entry, "l10nDomain");
        var upperName = entry.Name ?? string.Empty;
        var upperId = entry.Id ?? string.Empty;

        if (string.Equals(l10nDomain, domain, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return domain switch
        {
            "card" => ContainsAny(fullName, ".Cards.", ".DualWieldCards.")
                      || ContainsAny(resourcePath, "/Models/Cards/", "/card_portraits/"),
            "relic" => ContainsAny(fullName, ".Relics.")
                       || ContainsAny(resourcePath, "/Models/Relics/", "/images/relics/", "/relics/"),
            "potion" => ContainsAny(fullName, ".Potions.")
                        || ContainsAny(resourcePath, "/Models/Potions/", "/images/potions/", "/potions/")
                        || upperName.Contains("Potion", StringComparison.OrdinalIgnoreCase)
                        || upperId.Contains("potion", StringComparison.OrdinalIgnoreCase),
            "event" => ContainsAny(fullName, ".Events.")
                       || ContainsAny(resourcePath, "/Models/Events/", "/events/")
                       || upperId.Contains("event", StringComparison.OrdinalIgnoreCase),
            "shop" => ContainsAny(fullName, ".Merchant.")
                      || ContainsAny(resourcePath, "merchant", "shop", "rest_site")
                      || upperName.Contains("Merchant", StringComparison.OrdinalIgnoreCase)
                      || upperId.Contains("merchant", StringComparison.OrdinalIgnoreCase),
            "reward" => ContainsAny(fullName, ".Rewards.")
                        || ContainsAny(resourcePath, "reward", "card_selection")
                        || upperId.Contains("reward", StringComparison.OrdinalIgnoreCase),
            "keyword" => ContainsAny(fullName, ".Powers.", ".Intents.")
                         || ContainsAny(resourcePath, "intent", "keyword")
                         || upperId.EndsWith("power", StringComparison.OrdinalIgnoreCase)
                         || upperId.Contains("intent", StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }

    private static string? ReadAttribute(StaticKnowledgeEntry entry, string key)
    {
        return entry.Attributes.TryGetValue(key, out var value)
            ? value
            : null;
    }

    private static bool ContainsAny(string? value, params string[] needles)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return needles.Any(needle => value.Contains(needle, StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikePotionKey(string key, string normalizedStem)
    {
        if (normalizedStem.EndsWith("potion", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return key.Contains("POTION", StringComparison.OrdinalIgnoreCase)
               && !key.Contains(".pages.", StringComparison.OrdinalIgnoreCase)
               && !key.Contains(".talk.", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeKeywordKey(string key, string normalizedStem)
    {
        if (normalizedStem.EndsWith("power", StringComparison.OrdinalIgnoreCase)
            || normalizedStem.EndsWith("intent", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return key.Equals("BLOCK.title", StringComparison.OrdinalIgnoreCase)
               || key.Equals("BLOCK.description", StringComparison.OrdinalIgnoreCase)
               || key.Equals("ENERGY.title", StringComparison.OrdinalIgnoreCase)
               || key.Equals("ENERGY.description", StringComparison.OrdinalIgnoreCase)
               || key.Equals("ENERGY_COUNT.title", StringComparison.OrdinalIgnoreCase)
               || key.Equals("ENERGY_COUNT.description", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeShopKey(string key, string normalizedStem)
    {
        if (normalizedStem.Contains("merchant", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return key.StartsWith("MERCHANT.", StringComparison.OrdinalIgnoreCase)
               || key.StartsWith("ROOM_MERCHANT.", StringComparison.OrdinalIgnoreCase)
               || key.StartsWith("ROOM_UNKNOWN_MERCHANT.", StringComparison.OrdinalIgnoreCase)
               || key.StartsWith("LEGEND_MERCHANT.", StringComparison.OrdinalIgnoreCase)
               || key.Contains(".hoverTip.", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeRewardKey(string key, string normalizedStem)
    {
        return normalizedStem.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || key.StartsWith("CARD_SELECTION.", StringComparison.OrdinalIgnoreCase)
               || key.StartsWith("CARD_REWARD_UI.", StringComparison.OrdinalIgnoreCase)
               || key.StartsWith("HISTORY_ENTRY.", StringComparison.OrdinalIgnoreCase);
    }

    private static void AddSeed(ISet<string> target, string? value)
    {
        var normalized = NormalizeMatchKey(value);
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            target.Add(normalized);
        }
    }

    private static string InferLocale(string value, string? currentLocale, string? currentFileHint)
    {
        if (ContainsHangul(value))
        {
            return "kor";
        }

        if (ContainsCjkWithoutHangul(value))
        {
            return "cjk";
        }

        if (!string.IsNullOrWhiteSpace(currentFileHint))
        {
            if (currentFileHint.Contains("/kor/", StringComparison.OrdinalIgnoreCase))
            {
                return "kor";
            }

            if (currentFileHint.Contains("/eng/", StringComparison.OrdinalIgnoreCase))
            {
                return "eng";
            }
        }

        if (!string.IsNullOrWhiteSpace(currentLocale))
        {
            return currentLocale!;
        }

        return LooksMostlyLatin(value) ? "latin" : "unknown";
    }

    private static bool ContainsHangul(string value)
    {
        foreach (var character in value)
        {
            if (character >= '\uAC00' && character <= '\uD7A3')
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsCjkWithoutHangul(string value)
    {
        foreach (var character in value)
        {
            if ((character >= '\u4E00' && character <= '\u9FFF')
                || (character >= '\u3400' && character <= '\u4DBF')
                || (character >= '\u3040' && character <= '\u30FF'))
            {
                return true;
            }
        }

        return false;
    }

    private static bool LooksMostlyLatin(string value)
    {
        var letters = 0;
        var latinLetters = 0;
        foreach (var character in value)
        {
            if (!char.IsLetter(character))
            {
                continue;
            }

            letters += 1;
            if ((character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z'))
            {
                latinLetters += 1;
            }
        }

        return letters == 0 || latinLetters * 2 >= letters;
    }

    private static string NormalizeMatchKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    }

    private static string PrettifyStem(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Unknown";
        }

        var result = value.Replace('_', ' ').Trim();
        return string.IsNullOrWhiteSpace(result) ? value : result;
    }

    private static string UnescapeJsonString(string escaped)
    {
        try
        {
            return JsonSerializer.Deserialize<string>($"\"{escaped}\"") ?? string.Empty;
        }
        catch (JsonException)
        {
            return escaped
                .Replace("\\\"", "\"", StringComparison.Ordinal)
                .Replace("\\n", "\n", StringComparison.Ordinal)
                .Replace("\\r", "\r", StringComparison.Ordinal)
                .Replace("\\t", "\t", StringComparison.Ordinal)
                .Replace("\\\\", "\\", StringComparison.Ordinal);
        }
    }

    private sealed class ScanState
    {
        public string? CurrentLocale { get; set; }

        public string? CurrentFileHint { get; set; }
    }

    private sealed class ScanRecords
    {
        public Dictionary<string, MutableCardRecord> Cards { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, MutableLocalizationRecord> Relics { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, MutableLocalizationRecord> Potions { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, MutableEventRecord> Events { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, MutableLocalizationRecord> Shops { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, MutableLocalizationRecord> Rewards { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, MutableLocalizationRecord> Keywords { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed record SeedSets(
        HashSet<string> Cards,
        HashSet<string> Relics,
        HashSet<string> Potions,
        HashSet<string> Events,
        HashSet<string> Shops,
        HashSet<string> Rewards,
        HashSet<string> Keywords);

    private class MutableLocalizationRecord
    {
        private readonly Dictionary<string, Dictionary<string, string>> _valuesByLocaleAndField = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<string>> _notesByLocale = new(StringComparer.OrdinalIgnoreCase);
        private readonly SortedSet<string> _sourceFileHints = new(StringComparer.OrdinalIgnoreCase);
        private readonly SortedSet<string> _locales = new(StringComparer.OrdinalIgnoreCase);

        public MutableLocalizationRecord(string domain, string keyStem)
        {
            Domain = domain;
            KeyStem = keyStem;
        }

        public string Domain { get; }

        public string KeyStem { get; }

        public void ApplyField(string locale, string field, string value, string fileHint)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            _locales.Add(locale);
            _sourceFileHints.Add(fileHint);

            if (!_valuesByLocaleAndField.TryGetValue(locale, out var fields))
            {
                fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _valuesByLocaleAndField[locale] = fields;
            }

            fields[field] = value.Trim();
        }

        public void ApplyNote(string locale, string value, string fileHint)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            _locales.Add(locale);
            _sourceFileHints.Add(fileHint);

            if (!_notesByLocale.TryGetValue(locale, out var notes))
            {
                notes = new List<string>();
                _notesByLocale[locale] = notes;
            }

            var trimmed = value.Trim();
            if (!notes.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            {
                notes.Add(trimmed);
            }
        }

        public StaticKnowledgeLocalizationEntry ToImmutable(string domainOverride)
        {
            var preferredLocale = ChoosePreferredLocale();
            var notes = GetNotes(preferredLocale);
            var title = SelectPreferredField(preferredLocale, "title", fallbackToStem: true);
            var description = SelectPreferredField(preferredLocale, "description")
                ?? SelectPreferredField(preferredLocale, "flavor")
                ?? notes.FirstOrDefault();
            var flavor = SelectPreferredField(preferredLocale, "flavor");
            var selectionScreenPrompt = SelectPreferredField(preferredLocale, "selectionScreenPrompt");
            var unlockInfo = SelectPreferredField(preferredLocale, "unlockInfo");
            var englishTitle = FindEnglishField("title");
            var englishDescription = FindEnglishField("description");

            var attributes = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["unlockInfo"] = unlockInfo,
                ["notePreview"] = notes.Count == 0 ? null : string.Join(" | ", notes.Take(4)),
                ["noteCount"] = notes.Count == 0 ? null : notes.Count.ToString(),
            };

            return new StaticKnowledgeLocalizationEntry(
                domainOverride,
                KeyStem,
                preferredLocale,
                title,
                description,
                flavor,
                selectionScreenPrompt,
                englishTitle,
                englishDescription,
                Array.Empty<StaticKnowledgeOption>(),
                attributes,
                _sourceFileHints.ToArray(),
                _locales.ToArray());
        }

        protected string? GetField(string locale, string field)
        {
            return _valuesByLocaleAndField.TryGetValue(locale, out var fields)
                   && fields.TryGetValue(field, out var value)
                ? value
                : null;
        }

        protected string? GetAnyField(string field)
        {
            foreach (var locale in _valuesByLocaleAndField.Keys.OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
            {
                var value = GetField(locale, field);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        protected IReadOnlyList<string> GetNotes(string locale)
        {
            return _notesByLocale.TryGetValue(locale, out var notes)
                ? notes
                : Array.Empty<string>();
        }

        protected string? FindEnglishField(string field)
        {
            if (!string.IsNullOrWhiteSpace(GetField("eng", field)))
            {
                return GetField("eng", field);
            }

            foreach (var locale in _valuesByLocaleAndField.Keys.Where(locale => string.Equals(locale, "latin", StringComparison.OrdinalIgnoreCase)))
            {
                var value = GetField(locale, field);
                if (!string.IsNullOrWhiteSpace(value) && NormalizeMatchKey(GetField(locale, "title")) == NormalizeMatchKey(KeyStem))
                {
                    return value;
                }
            }

            return null;
        }

        protected string? SelectPreferredField(string preferredLocale, string field, bool fallbackToStem = false)
        {
            var direct = GetField(preferredLocale, field);
            if (IsLocaleCompatibleValue(preferredLocale, field, direct))
            {
                return direct;
            }

            var english = FindEnglishField(field);
            if (!string.IsNullOrWhiteSpace(english))
            {
                return english;
            }

            if (fallbackToStem && field.Equals("title", StringComparison.OrdinalIgnoreCase))
            {
                return PrettifyStem(KeyStem);
            }

            return GetAnyField(field);
        }

        private static bool IsLocaleCompatibleValue(string preferredLocale, string field, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (!field.Equals("title", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(preferredLocale, "kor", StringComparison.OrdinalIgnoreCase))
            {
                return !ContainsCjkWithoutHangul(value) || ContainsHangul(value);
            }

            if (string.Equals(preferredLocale, "eng", StringComparison.OrdinalIgnoreCase)
                || string.Equals(preferredLocale, "latin", StringComparison.OrdinalIgnoreCase))
            {
                return !ContainsCjkWithoutHangul(value) || LooksMostlyLatin(value);
            }

            return true;
        }

        private string ChoosePreferredLocale()
        {
            if (_locales.Contains("kor") && HasMeaningfulLocaleData("kor"))
            {
                return "kor";
            }

            if (_locales.Contains("eng") && HasMeaningfulLocaleData("eng"))
            {
                return "eng";
            }

            foreach (var locale in _locales)
            {
                if (string.Equals(locale, "latin", StringComparison.OrdinalIgnoreCase)
                    && HasMeaningfulLocaleData(locale)
                    && NormalizeMatchKey(GetField(locale, "title")) == NormalizeMatchKey(KeyStem))
                {
                    return locale;
                }
            }

            return _locales.FirstOrDefault(HasMeaningfulLocaleData) ?? _locales.FirstOrDefault() ?? "eng";
        }

        private bool HasMeaningfulLocaleData(string locale)
        {
            return !string.IsNullOrWhiteSpace(GetField(locale, "title"))
                   || !string.IsNullOrWhiteSpace(GetField(locale, "description"))
                   || !string.IsNullOrWhiteSpace(GetField(locale, "flavor"))
                   || !string.IsNullOrWhiteSpace(GetField(locale, "selectionScreenPrompt"))
                   || GetNotes(locale).Count > 0;
        }
    }

    private sealed class MutableCardRecord : MutableLocalizationRecord
    {
        public MutableCardRecord(string keyStem)
            : base("card", keyStem)
        {
        }

        public void Apply(string locale, string field, string value, string fileHint)
        {
            ApplyField(locale, field, value, fileHint);
        }

        public StaticKnowledgeLocalizationCardEntry ToCardImmutable()
        {
            var immutable = ToImmutable("card");
            return new StaticKnowledgeLocalizationCardEntry(
                immutable.KeyStem,
                immutable.PreferredLocale,
                immutable.Title,
                immutable.Description,
                immutable.SelectionScreenPrompt,
                immutable.EnglishTitle,
                immutable.EnglishDescription,
                immutable.SourceFileHints,
                immutable.Locales);
        }
    }

    private sealed class MutableEventRecord : MutableLocalizationRecord
    {
        private readonly Dictionary<string, Dictionary<string, string>> _pageDescriptionsByLocaleAndPage = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _optionFieldsByLocaleAndKey = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<string>> _talkLinesByLocale = new(StringComparer.OrdinalIgnoreCase);

        public MutableEventRecord(string keyStem)
            : base("event", keyStem)
        {
        }

        public void ApplyPageDescription(string locale, string pageKey, string value, string fileHint)
        {
            ApplyField(locale, $"page:{pageKey}", value, fileHint);
            if (!_pageDescriptionsByLocaleAndPage.TryGetValue(locale, out var pages))
            {
                pages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _pageDescriptionsByLocaleAndPage[locale] = pages;
            }

            pages[pageKey] = value.Trim();
        }

        public void ApplyOption(string locale, string pageKey, string optionKey, string field, string value, string fileHint)
        {
            ApplyField(locale, $"option:{pageKey}:{optionKey}:{field}", value, fileHint);
            if (!_optionFieldsByLocaleAndKey.TryGetValue(locale, out var options))
            {
                options = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                _optionFieldsByLocaleAndKey[locale] = options;
            }

            var compoundKey = $"{pageKey}:{optionKey}";
            if (!options.TryGetValue(compoundKey, out var fields))
            {
                fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["page"] = pageKey,
                    ["optionKey"] = optionKey,
                };
                options[compoundKey] = fields;
            }

            fields[field] = value.Trim();
        }

        public void ApplyTalk(string locale, string value, string fileHint)
        {
            ApplyNote(locale, value, fileHint);
            if (!_talkLinesByLocale.TryGetValue(locale, out var lines))
            {
                lines = new List<string>();
                _talkLinesByLocale[locale] = lines;
            }

            var trimmed = value.Trim();
            if (!lines.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            {
                lines.Add(trimmed);
            }
        }

        public StaticKnowledgeLocalizationEntry ToImmutable()
        {
            var baseEntry = base.ToImmutable("event");
            var pages = GetPages(baseEntry.PreferredLocale);
            IReadOnlyList<string> talkLines = _talkLinesByLocale.TryGetValue(baseEntry.PreferredLocale, out var lines)
                ? lines
                : Array.Empty<string>();
            var options = GetOptions(baseEntry.PreferredLocale);
            var description = baseEntry.Description
                ?? (pages.TryGetValue("INITIAL", out var initial) ? initial : pages.Values.FirstOrDefault())
                ?? talkLines.FirstOrDefault();

            var attributes = new Dictionary<string, string?>(baseEntry.Attributes, StringComparer.OrdinalIgnoreCase)
            {
                ["pageCount"] = pages.Count == 0 ? null : pages.Count.ToString(),
                ["optionCount"] = options.Count == 0 ? null : options.Count.ToString(),
                ["pageSummary"] = pages.Count == 0 ? null : string.Join(" | ", pages.Take(3).Select(entry => $"{entry.Key}: {entry.Value}")),
                ["talkPreview"] = talkLines.Count == 0 ? null : string.Join(" | ", talkLines.Take(3)),
            };

            return baseEntry with
            {
                Description = description,
                Options = options,
                Attributes = attributes,
            };
        }

        private IReadOnlyDictionary<string, string> GetPages(string locale)
        {
            if (_pageDescriptionsByLocaleAndPage.TryGetValue(locale, out var pages))
            {
                return pages;
            }

            if (_pageDescriptionsByLocaleAndPage.TryGetValue("eng", out var english))
            {
                return english;
            }

            return _pageDescriptionsByLocaleAndPage.Values.FirstOrDefault()
                   ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private IReadOnlyList<StaticKnowledgeOption> GetOptions(string locale)
        {
            Dictionary<string, Dictionary<string, string>>? source = null;
            if (_optionFieldsByLocaleAndKey.TryGetValue(locale, out var localized))
            {
                source = localized;
            }
            else if (_optionFieldsByLocaleAndKey.TryGetValue("eng", out var english))
            {
                source = english;
            }
            else
            {
                source = _optionFieldsByLocaleAndKey.Values.FirstOrDefault();
            }

            if (source is null)
            {
                return Array.Empty<StaticKnowledgeOption>();
            }

            return source.Values
                .Select(fields =>
                {
                    var pageKey = fields.TryGetValue("page", out var page) ? page : "INITIAL";
                    var optionKey = fields.TryGetValue("optionKey", out var option) ? option : "OPTION";
                    var label = fields.TryGetValue("title", out var title) && !string.IsNullOrWhiteSpace(title)
                        ? title
                        : PrettifyStem(optionKey);
                    fields.TryGetValue("description", out var description);
                    return new StaticKnowledgeOption(
                        NormalizeMatchKey($"{KeyStem}-{pageKey}-{optionKey}"),
                        label,
                        description,
                        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["page"] = pageKey,
                            ["optionKey"] = optionKey,
                        });
                })
                .OrderBy(option => option.Label, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    private sealed record ScanToken(int Index, string Kind, string? Locale, string? FileName, string? Key, string? Value);
}
