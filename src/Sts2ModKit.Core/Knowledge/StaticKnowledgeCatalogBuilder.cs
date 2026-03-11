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

    public static StaticKnowledgeCatalog MergeLocalization(
        StaticKnowledgeCatalog baseline,
        StaticKnowledgeLocalizationScan localizationScan,
        StaticKnowledgeMetadata metadata)
    {
        var cards = CreateMap(baseline.Cards);
        var relics = CreateMap(baseline.Relics);
        var potions = CreateMap(baseline.Potions);
        var events = CreateMap(baseline.Events);
        var shops = CreateMap(baseline.Shops);
        var rewards = CreateMap(baseline.Rewards);
        var keywords = CreateMap(baseline.Keywords);

        foreach (var localizationCard in localizationScan.Cards)
        {
            MergeLocalizationCard(cards, localizationCard);
        }

        foreach (var entry in localizationScan.Relics)
        {
            MergeLocalizationEntry(relics, entry, "relic", ".Relics.", "/relic", allowCreateWhenUnmatched: false);
        }

        foreach (var entry in localizationScan.Potions)
        {
            MergeLocalizationEntry(potions, entry, "potion", ".Potions.", "/potion", allowCreateWhenUnmatched: false);
        }

        foreach (var entry in localizationScan.Events)
        {
            MergeLocalizationEntry(events, entry, "event", ".Events.", "/event", allowCreateWhenUnmatched: false);
        }

        foreach (var entry in localizationScan.Shops)
        {
            MergeLocalizationEntry(shops, entry, "shop", ".Merchant.", "/merchant", allowCreateWhenUnmatched: false);
        }

        foreach (var entry in localizationScan.Rewards)
        {
            MergeLocalizationEntry(rewards, entry, "reward", ".Reward", "/reward", allowCreateWhenUnmatched: false);
        }

        foreach (var entry in localizationScan.Keywords)
        {
            MergeLocalizationEntry(keywords, entry, "keyword", string.Empty, string.Empty, allowCreateWhenUnmatched: false);
        }

        return new StaticKnowledgeCatalog(
            DateTimeOffset.UtcNow,
            metadata,
            SortEntries(cards.Values),
            SortEntries(relics.Values),
            SortEntries(potions.Values),
            SortEntries(events.Values),
            SortEntries(shops.Values),
            SortEntries(rewards.Values),
            SortEntries(keywords.Values));
    }

    public static StaticKnowledgeCatalog MergeCardLocalization(
        StaticKnowledgeCatalog baseline,
        StaticKnowledgeLocalizationScan localizationScan,
        StaticKnowledgeMetadata metadata)
    {
        return MergeLocalization(baseline, localizationScan, metadata);
    }

    public static StaticKnowledgeCatalog BuildAssistantCatalog(StaticKnowledgeCatalog catalog)
    {
        return new StaticKnowledgeCatalog(
            catalog.GeneratedAt,
            catalog.Metadata,
            BuildAssistantSection(catalog.Cards, "card"),
            BuildAssistantSection(catalog.Relics, "relic"),
            BuildAssistantSection(catalog.Potions, "potion"),
            BuildAssistantSection(catalog.Events, "event"),
            BuildAssistantSection(catalog.Shops, "shop"),
            BuildAssistantSection(catalog.Rewards, "reward"),
            BuildAssistantSection(catalog.Keywords, "keyword"));
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

    private static void MergeLocalizationCard(
        IDictionary<string, StaticKnowledgeEntry> cards,
        StaticKnowledgeLocalizationCardEntry localizationCard)
    {
        var matchId = FindBestCardMatch(cards.Values, localizationCard);
        if (string.IsNullOrWhiteSpace(matchId))
        {
            return;
        }

        var id = matchId;
        cards.TryGetValue(id, out var existing);
        if (existing is null)
        {
            return;
        }

        var attributes = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var attribute in existing.Attributes)
        {
            attributes[attribute.Key] = attribute.Value;
        }

        var resolvedDescription = LocalizedDescriptionResolver.Resolve(localizationCard.Description, attributes);
        var resolvedSelectionPrompt = LocalizedDescriptionResolver.Resolve(localizationCard.SelectionScreenPrompt, attributes);
        var resolvedEnglishDescription = LocalizedDescriptionResolver.Resolve(localizationCard.EnglishDescription, attributes);
        var resolvedTitle = ResolveLocalizedDisplayName(localizationCard.Title, existing!.Name, localizationCard.KeyStem);
        attributes["l10nKey"] = localizationCard.KeyStem;
        attributes["preferredLocale"] = localizationCard.PreferredLocale;
        attributes["localizedTitleRaw"] = localizationCard.Title;
        attributes["title"] = resolvedTitle;
        attributes["descriptionRaw"] = localizationCard.Description;
        attributes["description"] = resolvedDescription ?? localizationCard.Description;
        attributes["selectionScreenPrompt"] = resolvedSelectionPrompt ?? localizationCard.SelectionScreenPrompt;
        attributes["englishTitle"] = localizationCard.EnglishTitle;
        attributes["englishDescriptionRaw"] = localizationCard.EnglishDescription;
        attributes["englishDescription"] = resolvedEnglishDescription ?? localizationCard.EnglishDescription;
        attributes["sourceFileHint"] = localizationCard.SourceFileHints.Count == 0
            ? null
            : string.Join(" | ", localizationCard.SourceFileHints);
        attributes["sourceLocales"] = localizationCard.Locales.Count == 0
            ? localizationCard.PreferredLocale
            : string.Join(" | ", localizationCard.Locales);

        var tags = (existing?.Tags ?? Array.Empty<string>())
            .Concat(new[] { "localized-card", "l10n" })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        cards[id] = new StaticKnowledgeEntry(
            id,
            resolvedTitle,
            "localization-scan",
            existing!.Observed,
            resolvedDescription ?? localizationCard.Description ?? existing!.RawText ?? localizationCard.Title,
            tags,
            attributes,
            existing!.Options);
    }

    private static void MergeLocalizationEntry(
        IDictionary<string, StaticKnowledgeEntry> target,
        StaticKnowledgeLocalizationEntry localizationEntry,
        string domainTag,
        string fullNameHint,
        string resourceHint,
        bool allowCreateWhenUnmatched = true)
    {
        var matchId = FindBestMatch(target.Values, localizationEntry, fullNameHint, resourceHint);
        if (string.IsNullOrWhiteSpace(matchId) && !allowCreateWhenUnmatched)
        {
            return;
        }

        var id = matchId ?? NormalizeId(localizationEntry.KeyStem);
        target.TryGetValue(id, out var existing);

        var attributes = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (existing is not null)
        {
            foreach (var attribute in existing.Attributes)
            {
                attributes[attribute.Key] = attribute.Value;
            }
        }

        var resolvedDescription = LocalizedDescriptionResolver.Resolve(localizationEntry.Description, attributes);
        var resolvedFlavor = LocalizedDescriptionResolver.Resolve(localizationEntry.Flavor, attributes);
        var resolvedPrompt = LocalizedDescriptionResolver.Resolve(localizationEntry.SelectionScreenPrompt, attributes);
        var resolvedEnglishDescription = LocalizedDescriptionResolver.Resolve(localizationEntry.EnglishDescription, attributes);
        var resolvedTitle = ResolveLocalizedDisplayName(localizationEntry.Title, existing?.Name, localizationEntry.KeyStem);
        attributes["l10nDomain"] = localizationEntry.Domain;
        attributes["l10nKey"] = localizationEntry.KeyStem;
        attributes["preferredLocale"] = localizationEntry.PreferredLocale;
        attributes["localizedTitleRaw"] = localizationEntry.Title;
        attributes["title"] = resolvedTitle;
        attributes["descriptionRaw"] = localizationEntry.Description;
        attributes["description"] = resolvedDescription ?? localizationEntry.Description;
        attributes["flavorRaw"] = localizationEntry.Flavor;
        attributes["flavor"] = resolvedFlavor ?? localizationEntry.Flavor;
        attributes["selectionScreenPrompt"] = resolvedPrompt ?? localizationEntry.SelectionScreenPrompt;
        attributes["englishTitle"] = localizationEntry.EnglishTitle;
        attributes["englishDescriptionRaw"] = localizationEntry.EnglishDescription;
        attributes["englishDescription"] = resolvedEnglishDescription ?? localizationEntry.EnglishDescription;
        attributes["sourceFileHint"] = localizationEntry.SourceFileHints.Count == 0
            ? null
            : string.Join(" | ", localizationEntry.SourceFileHints);
        attributes["sourceLocales"] = localizationEntry.Locales.Count == 0
            ? localizationEntry.PreferredLocale
            : string.Join(" | ", localizationEntry.Locales);

        foreach (var attribute in localizationEntry.Attributes)
        {
            if (!string.IsNullOrWhiteSpace(attribute.Value))
            {
                attributes[attribute.Key] = attribute.Value;
            }
        }

        var tags = (existing?.Tags ?? Array.Empty<string>())
            .Concat(new[] { $"localized-{domainTag}", "l10n" })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        target[id] = new StaticKnowledgeEntry(
            id,
            resolvedTitle,
            "localization-scan",
            existing?.Observed == true,
            resolvedDescription ?? resolvedFlavor ?? localizationEntry.Description ?? localizationEntry.Flavor ?? existing?.RawText ?? localizationEntry.Title,
            tags,
            attributes,
            MergeOptions(existing?.Options, localizationEntry.Options));
    }

    private static string ResolveLocalizedDisplayName(string? localizedTitle, string? existingName, string keyStem)
    {
        if (!string.IsNullOrWhiteSpace(localizedTitle)
            && !LooksLikeSyntheticLocalizedTitle(localizedTitle!, keyStem))
        {
            return localizedTitle!;
        }

        if (!string.IsNullOrWhiteSpace(existingName))
        {
            return existingName!;
        }

        return PrettifyStem(keyStem);
    }

    private static bool LooksLikeSyntheticLocalizedTitle(string title, string keyStem)
    {
        var normalizedTitle = NormalizeForKnowledgeMatch(title);
        var normalizedStem = NormalizeForKnowledgeMatch(keyStem);
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            return true;
        }

        if (normalizedTitle == normalizedStem)
        {
            return true;
        }

        if (title.Contains('_', StringComparison.Ordinal))
        {
            return true;
        }

        var letters = title.Where(char.IsLetter).ToArray();
        if (letters.Length == 0)
        {
            return false;
        }

        var upperOnly = letters.All(character => !char.IsLower(character));
        return upperOnly && !ContainsHangul(title);
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

    private static string? FindBestCardMatch(
        IEnumerable<StaticKnowledgeEntry> cards,
        StaticKnowledgeLocalizationCardEntry localizationCard)
    {
        var normalizedStem = NormalizeForKnowledgeMatch(localizationCard.KeyStem);
        StaticKnowledgeEntry? best = null;
        var bestScore = 0;
        foreach (var entry in cards)
        {
            var score = ScoreCardMatch(entry, normalizedStem);
            if (score <= bestScore)
            {
                continue;
            }

            bestScore = score;
            best = entry;
        }

        return best?.Id;
    }

    private static int ScoreCardMatch(StaticKnowledgeEntry entry, string normalizedStem)
    {
        return ScoreEntryMatch(entry, normalizedStem, ".Cards.", "/card");
    }

    private static string? FindBestMatch(
        IEnumerable<StaticKnowledgeEntry> entries,
        StaticKnowledgeLocalizationEntry localizationEntry,
        string fullNameHint,
        string resourceHint)
    {
        var normalizedStem = NormalizeForKnowledgeMatch(localizationEntry.KeyStem);
        StaticKnowledgeEntry? best = null;
        var bestScore = 0;
        foreach (var entry in entries)
        {
            var score = ScoreEntryMatch(entry, normalizedStem, fullNameHint, resourceHint);
            if (score <= bestScore)
            {
                continue;
            }

            bestScore = score;
            best = entry;
        }

        return best?.Id;
    }

    private static int ScoreEntryMatch(
        StaticKnowledgeEntry entry,
        string normalizedStem,
        string fullNameHint,
        string resourceHint)
    {
        var bestScore = 0;
        var hasStemMatch = false;

        if (NormalizeForKnowledgeMatch(entry.Name) == normalizedStem)
        {
            bestScore = Math.Max(bestScore, 70);
            hasStemMatch = true;
        }

        if (NormalizeForKnowledgeMatch(entry.Id) == normalizedStem)
        {
            bestScore = Math.Max(bestScore, 45);
            hasStemMatch = true;
        }

        if (entry.Attributes.TryGetValue("l10nKey", out var l10nKey) && NormalizeForKnowledgeMatch(l10nKey) == normalizedStem)
        {
            bestScore = Math.Max(bestScore, 120);
            hasStemMatch = true;
        }

        if (entry.Attributes.TryGetValue("matchKeys", out var matchKeys) && !string.IsNullOrWhiteSpace(matchKeys))
        {
            foreach (var matchKey in matchKeys.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (NormalizeForKnowledgeMatch(matchKey) != normalizedStem)
                {
                    continue;
                }

                bestScore = Math.Max(bestScore, 125);
                hasStemMatch = true;
                break;
            }
        }

        if (entry.Attributes.TryGetValue("fullName", out var fullName) && !string.IsNullOrWhiteSpace(fullName))
        {
            var typeName = fullName.Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (NormalizeForKnowledgeMatch(typeName) == normalizedStem)
            {
                bestScore = Math.Max(bestScore, 110);
                hasStemMatch = true;
            }

            if (hasStemMatch && !string.IsNullOrWhiteSpace(fullNameHint) && fullName.Contains(fullNameHint, StringComparison.OrdinalIgnoreCase))
            {
                bestScore += 10;
            }

            if (fullName.Contains('<', StringComparison.Ordinal) || fullName.Contains('+', StringComparison.Ordinal))
            {
                bestScore -= 20;
            }
        }

        if (entry.Attributes.TryGetValue("resourcePath", out var resourcePath) && !string.IsNullOrWhiteSpace(resourcePath))
        {
            var normalizedPath = resourcePath.Replace('\\', '/');
            var fileName = Path.GetFileNameWithoutExtension(normalizedPath);
            if (NormalizeForKnowledgeMatch(fileName) == normalizedStem)
            {
                bestScore = Math.Max(bestScore, 90);
                hasStemMatch = true;
            }

            if (hasStemMatch && !string.IsNullOrWhiteSpace(resourceHint) && normalizedPath.Contains(resourceHint, StringComparison.OrdinalIgnoreCase))
            {
                bestScore += 8;
            }

            if (normalizedPath.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
            {
                bestScore -= 5;
            }
        }

        return bestScore;
    }

    private static string NormalizeForKnowledgeMatch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());
    }

    private static string PrettifyStem(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Unknown";
        }

        return value.Replace('_', ' ').Trim();
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

    private static IReadOnlyList<StaticKnowledgeEntry> BuildAssistantSection(IReadOnlyList<StaticKnowledgeEntry> entries, string domain)
    {
        return entries
            .Where(entry => MatchesAssistantDomain(entry, domain))
            .Where(entry => HasAssistantValue(entry, domain))
            .GroupBy(entry => BuildAssistantGroupingKey(entry), StringComparer.OrdinalIgnoreCase)
            .Select(group => SelectBestAssistantEntry(group, domain))
            .OrderByDescending(entry => entry.Observed)
            .ThenByDescending(entry => entry.Options.Count)
            .ThenByDescending(entry => !string.IsNullOrWhiteSpace(ReadAttribute(entry, "description")))
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool HasAssistantValue(StaticKnowledgeEntry entry, string domain)
    {
        if (LooksCompilerGenerated(entry))
        {
            return false;
        }

        if (entry.Observed || entry.Options.Count > 0)
        {
            return true;
        }

        if (string.Equals(ReadAttribute(entry, "strictModel"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(ReadAttribute(entry, "description"))
            || !string.IsNullOrWhiteSpace(ReadAttribute(entry, "title"))
            || !string.IsNullOrWhiteSpace(ReadAttribute(entry, "flavor")))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(ReadAttribute(entry, "fullName")))
        {
            return true;
        }

        var resourcePath = ReadAttribute(entry, "resourcePath");
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return false;
        }

        return domain switch
        {
            "card" => resourcePath.Contains("/card_portraits/", StringComparison.OrdinalIgnoreCase)
                      || resourcePath.Contains("/Models/Cards/", StringComparison.OrdinalIgnoreCase),
            "relic" => resourcePath.Contains("/relic", StringComparison.OrdinalIgnoreCase),
            "potion" => resourcePath.Contains("/potion", StringComparison.OrdinalIgnoreCase),
            "event" => resourcePath.Contains("/event", StringComparison.OrdinalIgnoreCase),
            "shop" => resourcePath.Contains("merchant", StringComparison.OrdinalIgnoreCase)
                      || resourcePath.Contains("shop", StringComparison.OrdinalIgnoreCase),
            "reward" => resourcePath.Contains("reward", StringComparison.OrdinalIgnoreCase)
                        || resourcePath.Contains("card_selection", StringComparison.OrdinalIgnoreCase),
            "keyword" => resourcePath.Contains("intent", StringComparison.OrdinalIgnoreCase)
                         || resourcePath.Contains("keyword", StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }

    private static string BuildAssistantGroupingKey(StaticKnowledgeEntry entry)
    {
        return NormalizeForKnowledgeMatch(
            ReadAttribute(entry, "l10nKey")
            ?? ResolveTypeName(entry)
            ?? entry.Name
            ?? entry.Id);
    }

    private static StaticKnowledgeEntry SelectBestAssistantEntry(IEnumerable<StaticKnowledgeEntry> group, string domain)
    {
        return group
            .OrderByDescending(entry => ScoreAssistantEntry(entry, domain))
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .First();
    }

    private static int ScoreAssistantEntry(StaticKnowledgeEntry entry, string domain)
    {
        var score = 0;
        if (entry.Observed)
        {
            score += 200;
        }

        if (string.Equals(entry.Source, "localization-scan", StringComparison.OrdinalIgnoreCase))
        {
            score += 120;
        }

        if (!string.IsNullOrWhiteSpace(ReadAttribute(entry, "description")))
        {
            score += 100;
        }

        if (!string.IsNullOrWhiteSpace(ReadAttribute(entry, "title")))
        {
            score += 50;
        }

        if (entry.Options.Count > 0)
        {
            score += 80;
        }

        if (!string.IsNullOrWhiteSpace(ReadAttribute(entry, "fullName")))
        {
            score += 30;
        }

        if (!string.IsNullOrWhiteSpace(ReadAttribute(entry, "resourcePath")))
        {
            score += 20;
        }

        if (entry.Name.StartsWith("\"", StringComparison.Ordinal)
            || entry.Name.StartsWith("<", StringComparison.Ordinal)
            || entry.Name.Contains("filename", StringComparison.OrdinalIgnoreCase)
            || entry.Name.Contains("image", StringComparison.OrdinalIgnoreCase))
        {
            score -= 50;
        }

        if (LooksCompilerGenerated(entry))
        {
            score -= 120;
        }

        if (!MatchesAssistantDomain(entry, domain))
        {
            score -= 200;
        }

        return score;
    }

    private static bool MatchesAssistantDomain(StaticKnowledgeEntry entry, string domain)
    {
        var l10nDomain = ReadAttribute(entry, "l10nDomain");
        if (string.Equals(l10nDomain, domain, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var strictDomain = ReadAttribute(entry, "strictDomain");
        if (string.Equals(strictDomain, domain, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var fullName = ReadAttribute(entry, "fullName");
        var resourcePath = ReadAttribute(entry, "resourcePath");
        return domain switch
        {
            "card" => ContainsAny(fullName, ".Cards.", ".DualWieldCards.")
                      || ContainsAny(resourcePath, "/Models/Cards/", "/card_portraits/"),
            "relic" => ContainsAny(fullName, ".Relics.")
                       || ContainsAny(resourcePath, "/Models/Relics/", "/images/relics/", "/relics/"),
            "potion" => ContainsAny(fullName, ".Potions.")
                        || ContainsAny(resourcePath, "/Models/Potions/", "/images/potions/", "/potions/"),
            "event" => ContainsAny(fullName, ".Events.")
                       || ContainsAny(resourcePath, "/Models/Events/", "/events/"),
            "shop" => ContainsAny(fullName, ".Merchant.")
                      || ContainsAny(resourcePath, "merchant", "shop", "rest_site"),
            "reward" => ContainsAny(fullName, ".Rewards.")
                        || ContainsAny(resourcePath, "reward", "card_selection"),
            "keyword" => ContainsAny(fullName, ".Powers.", ".Intents.")
                         || ContainsAny(resourcePath, "intent", "keyword"),
            _ => false,
        };
    }

    private static bool LooksCompilerGenerated(StaticKnowledgeEntry entry)
    {
        var fullName = ReadAttribute(entry, "fullName");
        return (!string.IsNullOrWhiteSpace(entry.Name)
                && (entry.Name.StartsWith("<", StringComparison.Ordinal)
                    || string.Equals(entry.Name, "<>c", StringComparison.OrdinalIgnoreCase)
                    || entry.Name.Contains("|", StringComparison.Ordinal)))
               || (!string.IsNullOrWhiteSpace(fullName)
                   && (fullName.Contains('<', StringComparison.Ordinal)
                       || fullName.Contains("+<>", StringComparison.Ordinal)
                       || fullName.Contains("|", StringComparison.Ordinal)));
    }

    private static string? ResolveTypeName(StaticKnowledgeEntry entry)
    {
        var fullName = ReadAttribute(entry, "fullName");
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return null;
        }

        var typeName = fullName.Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        if (string.IsNullOrWhiteSpace(typeName) || typeName.Contains('<', StringComparison.Ordinal))
        {
            return null;
        }

        return typeName;
    }

    private static bool ContainsAny(string? value, params string[] needles)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return needles.Any(needle => value.Contains(needle, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ReadAttribute(StaticKnowledgeEntry entry, string key)
    {
        return entry.Attributes.TryGetValue(key, out var value)
            ? value
            : null;
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
