using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModKit.Core.Knowledge;

public static class StaticKnowledgeCatalogBuilder
{
    public static StaticKnowledgeCatalog MergeCatalogs(
        StaticKnowledgeMetadata metadata,
        params StaticKnowledgeCatalog?[] catalogs)
    {
        var cards = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var relics = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var potions = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var eventEntries = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var shopEntries = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var rewardEntries = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var keywords = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var catalog in catalogs.Where(catalog => catalog is not null))
        {
            MergeEntries(catalog!.Cards, cards);
            MergeEntries(catalog.Relics, relics);
            MergeEntries(catalog.Potions, potions);
            MergeEntries(catalog.Events, eventEntries);
            MergeEntries(catalog.Shops, shopEntries);
            MergeEntries(catalog.Rewards, rewardEntries);
            MergeEntries(catalog.Keywords, keywords);
        }

        return new StaticKnowledgeCatalog(
            DateTimeOffset.UtcNow,
            metadata,
            SortEntries(cards.Values),
            SortEntries(relics.Values),
            SortEntries(potions.Values),
            SortEntries(eventEntries.Values),
            SortEntries(shopEntries.Values),
            SortEntries(rewardEntries.Values),
            SortEntries(keywords.Values));
    }

    public static StaticKnowledgeCatalog BuildFromObserved(
        StaticKnowledgeCatalog? baseline,
        LiveExportSnapshot? snapshot,
        IReadOnlyList<LiveExportEventEnvelope> events,
        StaticKnowledgeMetadata metadata)
    {
        var cards = CreateMap(baseline?.Cards);
        var relics = CreateMap(baseline?.Relics);
        var potions = CreateMap(baseline?.Potions);
        var eventEntries = CreateMap(baseline?.Events);
        var shopEntries = CreateMap(baseline?.Shops);
        var rewardEntries = CreateMap(baseline?.Rewards);
        var keywords = CreateMap(baseline?.Keywords);

        if (snapshot is not null)
        {
            MergeSnapshot(snapshot, cards, relics, potions, eventEntries, shopEntries, rewardEntries, keywords);
        }

        foreach (var envelope in events)
        {
            MergeEventEnvelope(envelope, cards, relics, potions, eventEntries, shopEntries, rewardEntries);
        }

        return new StaticKnowledgeCatalog(
            DateTimeOffset.UtcNow,
            metadata,
            SortEntries(cards.Values),
            SortEntries(relics.Values),
            SortEntries(potions.Values),
            SortEntries(eventEntries.Values),
            SortEntries(shopEntries.Values),
            SortEntries(rewardEntries.Values),
            SortEntries(keywords.Values));
    }

    private static void MergeSnapshot(
        LiveExportSnapshot snapshot,
        IDictionary<string, StaticKnowledgeEntry> cards,
        IDictionary<string, StaticKnowledgeEntry> relics,
        IDictionary<string, StaticKnowledgeEntry> potions,
        IDictionary<string, StaticKnowledgeEntry> eventEntries,
        IDictionary<string, StaticKnowledgeEntry> shopEntries,
        IDictionary<string, StaticKnowledgeEntry> rewardEntries,
        IDictionary<string, StaticKnowledgeEntry> keywords)
    {
        foreach (var card in snapshot.Deck)
        {
            UpsertEntry(
                cards,
                preferredId: card.Id,
                fallbackName: card.Name,
                source: "live-deck",
                observed: true,
                rawText: card.Name,
                tags: new[] { "deck" },
                attributes: new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["type"] = card.Type,
                    ["cost"] = card.Cost?.ToString(),
                    ["upgraded"] = card.Upgraded?.ToString(),
                });

            if (!string.IsNullOrWhiteSpace(card.Type))
            {
                UpsertEntry(
                    keywords,
                    preferredId: card.Type,
                    fallbackName: card.Type!,
                    source: "live-deck",
                    observed: true,
                    rawText: card.Type,
                    tags: new[] { "card-type" },
                    attributes: new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase));
            }
        }

        foreach (var relic in snapshot.Relics)
        {
            UpsertSimpleObserved(relics, relic, "live-relics", "relic");
        }

        foreach (var potion in snapshot.Potions)
        {
            UpsertSimpleObserved(potions, potion, "live-potions", "potion");
        }

        foreach (var choice in snapshot.CurrentChoices)
        {
            switch (choice.Kind.ToLowerInvariant())
            {
                case "card":
                    UpsertEntry(cards, choice.Value, choice.Label, "live-choice", true, choice.Description, new[] { "choice" }, MakeChoiceAttributes(choice));
                    break;
                case "relic":
                    UpsertEntry(relics, choice.Value, choice.Label, "live-choice", true, choice.Description, new[] { "choice" }, MakeChoiceAttributes(choice));
                    break;
                case "potion":
                    UpsertEntry(potions, choice.Value, choice.Label, "live-choice", true, choice.Description, new[] { "choice" }, MakeChoiceAttributes(choice));
                    break;
            }
        }

        MergeScreenEntry(snapshot, eventEntries, shopEntries, rewardEntries);
    }

    private static IReadOnlyDictionary<string, string?> MakeChoiceAttributes(LiveExportChoiceSummary choice)
    {
        return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["choiceKind"] = choice.Kind,
            ["value"] = choice.Value,
        };
    }

    private static void MergeScreenEntry(
        LiveExportSnapshot snapshot,
        IDictionary<string, StaticKnowledgeEntry> eventEntries,
        IDictionary<string, StaticKnowledgeEntry> shopEntries,
        IDictionary<string, StaticKnowledgeEntry> rewardEntries)
    {
        if (snapshot.CurrentChoices.Count == 0)
        {
            return;
        }

        var options = snapshot.CurrentChoices
            .Select(choice => new StaticKnowledgeOption(
                NormalizeId(choice.Value ?? choice.Label),
                choice.Label,
                choice.Description,
                new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["kind"] = choice.Kind,
                    ["value"] = choice.Value,
                }))
            .ToArray();

        var seed = string.Join("|", snapshot.CurrentChoices.Select(choice => $"{choice.Kind}:{choice.Label}:{choice.Value}"));
        var attributes = new Dictionary<string, string?>(snapshot.Meta, StringComparer.OrdinalIgnoreCase)
        {
            ["screen"] = snapshot.CurrentScreen,
            ["act"] = snapshot.Act?.ToString(),
            ["floor"] = snapshot.Floor?.ToString(),
        };

        switch (snapshot.CurrentScreen)
        {
            case "event":
                UpsertEntry(
                    eventEntries,
                    BuildSyntheticId("event", snapshot.Meta, seed),
                    snapshot.Meta.TryGetValue("sceneRootType", out var sceneRootType) && !string.IsNullOrWhiteSpace(sceneRootType) ? sceneRootType! : "Observed Event",
                    "live-snapshot",
                    true,
                    string.Join(" | ", snapshot.CurrentChoices.Select(choice => choice.Label)),
                    new[] { "event" },
                    attributes,
                    options);
                break;

            case "shop":
                UpsertEntry(shopEntries, BuildSyntheticId("shop", snapshot.Meta, seed), "Observed Shop", "live-snapshot", true, string.Join(" | ", snapshot.CurrentChoices.Select(choice => choice.Label)), new[] { "shop" }, attributes, options);
                break;

            case "rewards":
                UpsertEntry(rewardEntries, BuildSyntheticId("reward", snapshot.Meta, seed), "Observed Reward", "live-snapshot", true, string.Join(" | ", snapshot.CurrentChoices.Select(choice => choice.Label)), new[] { "reward" }, attributes, options);
                break;
        }
    }

    private static void MergeEventEnvelope(
        LiveExportEventEnvelope envelope,
        IDictionary<string, StaticKnowledgeEntry> cards,
        IDictionary<string, StaticKnowledgeEntry> relics,
        IDictionary<string, StaticKnowledgeEntry> potions,
        IDictionary<string, StaticKnowledgeEntry> eventEntries,
        IDictionary<string, StaticKnowledgeEntry> shopEntries,
        IDictionary<string, StaticKnowledgeEntry> rewardEntries)
    {
        switch (envelope.Kind)
        {
            case "card-added":
            case "card-removed":
                if (TryReadPayloadString(envelope.Payload, "item", out var cardName))
                {
                    UpsertEntry(cards, cardName, cardName, $"live-event:{envelope.Kind}", true, cardName, new[] { envelope.Kind }, EnvelopeAttributes(envelope));
                }
                break;

            case "relic-gained":
            case "relic-lost":
                if (TryReadPayloadString(envelope.Payload, "item", out var relicName))
                {
                    UpsertEntry(relics, relicName, relicName, $"live-event:{envelope.Kind}", true, relicName, new[] { envelope.Kind }, EnvelopeAttributes(envelope));
                }
                break;

            case "potion-changed":
                foreach (var potionName in ReadPayloadStringList(envelope.Payload, "after"))
                {
                    UpsertSimpleObserved(potions, potionName, "live-event:potion-changed", "potion");
                }
                break;

            case "event-screen-opened":
                UpsertEntry(eventEntries, BuildSyntheticId("event", new Dictionary<string, string?>(), JsonSerializer.Serialize(envelope.Payload)), "Observed Event", $"live-event:{envelope.Kind}", true, JsonSerializer.Serialize(envelope.Payload), new[] { "event" }, EnvelopeAttributes(envelope));
                break;

            case "reward-screen-opened":
                UpsertEntry(rewardEntries, BuildSyntheticId("reward", new Dictionary<string, string?>(), JsonSerializer.Serialize(envelope.Payload)), "Observed Reward", $"live-event:{envelope.Kind}", true, JsonSerializer.Serialize(envelope.Payload), new[] { "reward" }, EnvelopeAttributes(envelope));
                break;

            case "choice-list-presented":
                MergeChoiceEvent(envelope, eventEntries, shopEntries, rewardEntries);
                break;
        }
    }

    private static IReadOnlyDictionary<string, string?> EnvelopeAttributes(LiveExportEventEnvelope envelope)
    {
        return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["screen"] = envelope.Screen,
            ["act"] = envelope.Act?.ToString(),
            ["floor"] = envelope.Floor?.ToString(),
        };
    }

    private static void MergeChoiceEvent(
        LiveExportEventEnvelope envelope,
        IDictionary<string, StaticKnowledgeEntry> eventEntries,
        IDictionary<string, StaticKnowledgeEntry> shopEntries,
        IDictionary<string, StaticKnowledgeEntry> rewardEntries)
    {
        var options = ReadPayloadStringList(envelope.Payload, "choices")
            .Select(choice => new StaticKnowledgeOption(
                NormalizeId(choice),
                choice,
                null,
                new Dictionary<string, string?>()))
            .ToArray();
        if (options.Length == 0)
        {
            return;
        }

        var attributes = EnvelopeAttributes(envelope);
        var seed = string.Join("|", options.Select(option => option.Label));
        if (string.Equals(envelope.Screen, "shop", StringComparison.OrdinalIgnoreCase))
        {
            UpsertEntry(shopEntries, BuildSyntheticId("shop", new Dictionary<string, string?>(), seed), "Observed Shop", $"live-event:{envelope.Kind}", true, seed, new[] { "shop" }, attributes, options);
        }
        else if (string.Equals(envelope.Screen, "event", StringComparison.OrdinalIgnoreCase))
        {
            UpsertEntry(eventEntries, BuildSyntheticId("event", new Dictionary<string, string?>(), seed), "Observed Event", $"live-event:{envelope.Kind}", true, seed, new[] { "event" }, attributes, options);
        }
        else if (string.Equals(envelope.Screen, "rewards", StringComparison.OrdinalIgnoreCase))
        {
            UpsertEntry(rewardEntries, BuildSyntheticId("reward", new Dictionary<string, string?>(), seed), "Observed Reward", $"live-event:{envelope.Kind}", true, seed, new[] { "reward" }, attributes, options);
        }
    }

    private static void UpsertSimpleObserved(IDictionary<string, StaticKnowledgeEntry> target, string value, string source, string tag)
    {
        UpsertEntry(target, value, value, source, true, value, new[] { tag }, new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase));
    }

    private static void UpsertEntry(
        IDictionary<string, StaticKnowledgeEntry> target,
        string? preferredId,
        string fallbackName,
        string source,
        bool observed,
        string? rawText,
        IEnumerable<string>? tags,
        IReadOnlyDictionary<string, string?>? attributes,
        IReadOnlyList<StaticKnowledgeOption>? options = null)
    {
        var id = NormalizeId(preferredId ?? fallbackName);
        target.TryGetValue(id, out var existing);

        var mergedTags = (existing?.Tags ?? Array.Empty<string>())
            .Concat(tags ?? Array.Empty<string>())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var mergedAttributes = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (existing is not null)
        {
            foreach (var entry in existing.Attributes)
            {
                mergedAttributes[entry.Key] = entry.Value;
            }
        }

        if (attributes is not null)
        {
            foreach (var entry in attributes)
            {
                if (!string.IsNullOrWhiteSpace(entry.Value))
                {
                    mergedAttributes[entry.Key] = entry.Value;
                }
            }
        }

        var mergedOptions = MergeOptions(existing?.Options, options);
        target[id] = new StaticKnowledgeEntry(
            id,
            existing?.Name ?? fallbackName,
            existing?.Source ?? source,
            observed || existing?.Observed == true,
            existing?.RawText ?? rawText,
            mergedTags,
            mergedAttributes,
            mergedOptions);
    }

    private static IReadOnlyList<StaticKnowledgeOption> MergeOptions(IReadOnlyList<StaticKnowledgeOption>? existing, IReadOnlyList<StaticKnowledgeOption>? incoming)
    {
        var merged = new Dictionary<string, StaticKnowledgeOption>(StringComparer.OrdinalIgnoreCase);
        foreach (var option in existing ?? Array.Empty<StaticKnowledgeOption>())
        {
            merged[option.Id] = option;
        }

        foreach (var option in incoming ?? Array.Empty<StaticKnowledgeOption>())
        {
            merged[option.Id] = option;
        }

        return merged.Values.OrderBy(option => option.Label, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static Dictionary<string, StaticKnowledgeEntry> CreateMap(IReadOnlyList<StaticKnowledgeEntry>? entries)
    {
        return (entries ?? Array.Empty<StaticKnowledgeEntry>())
            .ToDictionary(entry => entry.Id, entry => entry, StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<StaticKnowledgeEntry> SortEntries(IEnumerable<StaticKnowledgeEntry> entries)
    {
        return entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ThenBy(entry => entry.Id, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void MergeEntries(IEnumerable<StaticKnowledgeEntry> source, IDictionary<string, StaticKnowledgeEntry> target)
    {
        foreach (var entry in source)
        {
            target.TryGetValue(entry.Id, out var existing);
            target[entry.Id] = existing is null
                ? entry
                : existing with
                {
                    Observed = existing.Observed || entry.Observed,
                    RawText = existing.RawText ?? entry.RawText,
                    Tags = existing.Tags
                        .Concat(entry.Tags)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
                        .ToArray(),
                    Attributes = existing.Attributes
                        .Concat(entry.Attributes)
                        .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.OrdinalIgnoreCase),
                    Options = existing.Options
                        .Concat(entry.Options)
                        .GroupBy(option => option.Id, StringComparer.OrdinalIgnoreCase)
                        .Select(group => group.Last())
                        .OrderBy(option => option.Label, StringComparer.OrdinalIgnoreCase)
                        .ToArray(),
                };
        }
    }

    private static string BuildSyntheticId(string prefix, IReadOnlyDictionary<string, string?> meta, string seed)
    {
        var discriminator = meta.TryGetValue("sceneRootType", out var sceneRootType) && !string.IsNullOrWhiteSpace(sceneRootType)
            ? sceneRootType
            : meta.TryGetValue("currentSceneType", out var currentSceneType) && !string.IsNullOrWhiteSpace(currentSceneType)
                ? currentSceneType
                : seed;
        using var sha = SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(discriminator ?? seed))).ToLowerInvariant();
        return $"{prefix}-{hash[..12]}";
    }

    private static string NormalizeId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value.Trim())
        {
            builder.Append(char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '-');
        }

        var normalized = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(normalized)
            ? Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant()[..12]
            : normalized;
    }

    private static bool TryReadPayloadString(IReadOnlyDictionary<string, object?> payload, string key, out string value)
    {
        if (payload.TryGetValue(key, out var raw) && TryConvertToString(raw, out value))
        {
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static IReadOnlyList<string> ReadPayloadStringList(IReadOnlyDictionary<string, object?> payload, string key)
    {
        if (!payload.TryGetValue(key, out var raw) || raw is null)
        {
            return Array.Empty<string>();
        }

        return raw switch
        {
            JsonElement { ValueKind: JsonValueKind.Array } array => array.EnumerateArray().Select(item => item.ToString()).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal).ToArray(),
            IEnumerable<object> enumerable => enumerable.Select(item => item?.ToString()).Where(value => !string.IsNullOrWhiteSpace(value)).Cast<string>().Distinct(StringComparer.Ordinal).ToArray(),
            _ when TryConvertToString(raw, out var singleValue) => new[] { singleValue },
            _ => Array.Empty<string>(),
        };
    }

    private static bool TryConvertToString(object? value, out string text)
    {
        switch (value)
        {
            case null:
                text = string.Empty;
                return false;
            case JsonElement { ValueKind: JsonValueKind.String } element:
                text = element.GetString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(text);
            case JsonElement element:
                text = element.ToString();
                return !string.IsNullOrWhiteSpace(text);
            default:
                text = value.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(text);
        }
    }
}
