namespace Sts2ModKit.Core.LiveExport;

public sealed class LiveExportStateTracker
{
    private readonly LiveExportStateTrackerOptions _options;
    private readonly string _sessionId;
    private readonly string _liveRoot;
    private LiveExportSnapshot _snapshot;
    private long _sequence;

    public LiveExportStateTracker(LiveExportStateTrackerOptions options, string liveRoot, string? seedRunId = null)
    {
        _options = options;
        _liveRoot = liveRoot;
        _sessionId = Guid.NewGuid().ToString("N");
        _snapshot = LiveExportSnapshot.CreateEmpty(seedRunId ?? $"pending-{_sessionId[..8]}");
    }

    public LiveExportBatch Apply(LiveExportObservation observation)
    {
        var nextSnapshot = MergeSnapshot(_snapshot, observation);
        var events = new List<LiveExportEventEnvelope>();
        events.AddRange(BuildDerivedEvents(_snapshot, nextSnapshot, observation));
        events.Add(CreateEnvelope(observation.TriggerKind, nextSnapshot, observation.Payload, observation.ObservedAt));

        _snapshot = nextSnapshot;
        var session = new LiveExportSession(
            _sessionId,
            nextSnapshot.RunId,
            nextSnapshot.RunStatus,
            _snapshot.CapturedAt,
            observation.ObservedAt,
            _sequence,
            _liveRoot,
            observation.TriggerKind,
            nextSnapshot.CurrentScreen);

        return new LiveExportBatch(nextSnapshot, session, events);
    }

    public LiveExportReplayResult Replay(IEnumerable<LiveExportObservation> observations)
    {
        var warnings = new List<string>();
        var events = new List<LiveExportEventEnvelope>();
        foreach (var observation in observations)
        {
            try
            {
                var batch = Apply(observation);
                events.AddRange(batch.Events);
            }
            catch (Exception exception)
            {
                warnings.Add(exception.Message);
            }
        }

        return new LiveExportReplayResult(_snapshot, events, warnings);
    }

    private LiveExportSnapshot MergeSnapshot(LiveExportSnapshot previous, LiveExportObservation observation)
    {
        var runId = Coalesce(observation.RunId, previous.RunId);
        var screen = MergeScreen(previous, observation);
        var player = MergePlayer(previous.Player, observation.Player);
        var deck = MergeCards(previous.Deck, observation.Deck, observation);
        var relics = MergeStrings(previous.Relics, observation.Relics, observation);
        var potions = MergeStrings(previous.Potions, observation.Potions, observation);
        var choices = MergeChoices(previous.CurrentChoices, observation.Choices, observation, previous.CurrentScreen, screen);
        var warnings = MergeWarnings(previous.Warnings, observation.Warnings);
        var encounter = MergeEncounter(previous.Encounter, observation.Encounter, observation);

        var recentChanges = previous.RecentChanges
            .Concat(DescribeDiff(previous, screen, player, deck, relics, potions, choices, observation.TriggerKind))
            .TakeLast(_options.MaxRecentChanges)
            .ToArray();

        return previous with
        {
            RunId = runId,
            RunStatus = Coalesce(observation.RunStatus, InferRunStatus(observation.TriggerKind), previous.RunStatus),
            Version = previous.Version + 1,
            CapturedAt = observation.ObservedAt,
            CurrentScreen = screen,
            Act = observation.Act ?? previous.Act,
            Floor = observation.Floor ?? previous.Floor,
            Player = player,
            Deck = deck,
            Relics = relics,
            Potions = potions,
            CurrentChoices = choices,
            RecentChanges = recentChanges,
            Warnings = warnings,
            Encounter = encounter,
            Meta = MergeMeta(previous.Meta, observation.Meta),
        };
    }

    private IEnumerable<LiveExportEventEnvelope> BuildDerivedEvents(
        LiveExportSnapshot previous,
        LiveExportSnapshot next,
        LiveExportObservation observation)
    {
        if (previous.Version == 0)
        {
            yield break;
        }

        if (!string.Equals(previous.CurrentScreen, next.CurrentScreen, StringComparison.Ordinal))
        {
            yield return CreateEnvelope(
                "screen-changed",
                next,
                new Dictionary<string, object?>
                {
                    ["before"] = previous.CurrentScreen,
                    ["after"] = next.CurrentScreen,
                },
                observation.ObservedAt);
        }

        var previousChoiceLabels = previous.CurrentChoices.Select(choice => choice.Label).ToArray();
        var nextChoiceLabels = next.CurrentChoices.Select(choice => choice.Label).ToArray();
        if (!previousChoiceLabels.SequenceEqual(nextChoiceLabels, StringComparer.Ordinal)
            && nextChoiceLabels.Length > 0
            && !string.Equals(observation.TriggerKind, "choice-list-presented", StringComparison.Ordinal))
        {
            yield return CreateEnvelope(
                "choice-list-presented",
                next,
                new Dictionary<string, object?>
                {
                    ["choices"] = nextChoiceLabels,
                },
                observation.ObservedAt);
        }

        if (previous.Player.CurrentHp != next.Player.CurrentHp || previous.Player.MaxHp != next.Player.MaxHp)
        {
            yield return CreateEnvelope(
                "hp-changed",
                next,
                new Dictionary<string, object?>
                {
                    ["before"] = previous.Player.CurrentHp,
                    ["after"] = next.Player.CurrentHp,
                },
                observation.ObservedAt);
        }

        if (previous.Player.Gold != next.Player.Gold)
        {
            yield return CreateEnvelope(
                "gold-changed",
                next,
                new Dictionary<string, object?>
                {
                    ["before"] = previous.Player.Gold,
                    ["after"] = next.Player.Gold,
                },
                observation.ObservedAt);
        }

        foreach (var cardEvent in BuildListDifferenceEvents(previous.Deck.Select(card => card.Name), next.Deck.Select(card => card.Name), "card"))
        {
            yield return CreateEnvelope(cardEvent.Kind, next, cardEvent.Payload, observation.ObservedAt);
        }

        foreach (var relicEvent in BuildListDifferenceEvents(previous.Relics, next.Relics, "relic"))
        {
            yield return CreateEnvelope(relicEvent.Kind, next, relicEvent.Payload, observation.ObservedAt);
        }

        if (!previous.Potions.SequenceEqual(next.Potions, StringComparer.Ordinal))
        {
            yield return CreateEnvelope(
                "potion-changed",
                next,
                new Dictionary<string, object?>
                {
                    ["before"] = previous.Potions.ToArray(),
                    ["after"] = next.Potions.ToArray(),
                },
                observation.ObservedAt);
        }
    }

    private static IEnumerable<(string Kind, IReadOnlyDictionary<string, object?> Payload)> BuildListDifferenceEvents(
        IEnumerable<string> previousItems,
        IEnumerable<string> nextItems,
        string label)
    {
        var previousCounts = previousItems
            .GroupBy(item => item, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var nextCounts = nextItems
            .GroupBy(item => item, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

        foreach (var entry in nextCounts)
        {
            previousCounts.TryGetValue(entry.Key, out var previousCount);
            if (entry.Value > previousCount)
            {
                yield return (
                    GetAddedEventKind(label),
                    new Dictionary<string, object?>
                    {
                        ["item"] = entry.Key,
                        ["count"] = entry.Value - previousCount,
                    });
            }
        }

        foreach (var entry in previousCounts)
        {
            nextCounts.TryGetValue(entry.Key, out var nextCount);
            if (entry.Value > nextCount)
            {
                yield return (
                    GetRemovedEventKind(label),
                    new Dictionary<string, object?>
                    {
                        ["item"] = entry.Key,
                        ["count"] = entry.Value - nextCount,
                    });
            }
        }
    }

    private static string GetAddedEventKind(string label)
    {
        return label switch
        {
            "card" => "card-added",
            "relic" => "relic-gained",
            _ => $"{label}-gained",
        };
    }

    private static string GetRemovedEventKind(string label)
    {
        return label switch
        {
            "card" => "card-removed",
            "relic" => "relic-lost",
            _ => $"{label}-lost",
        };
    }

    private LiveExportEventEnvelope CreateEnvelope(
        string kind,
        LiveExportSnapshot snapshot,
        IReadOnlyDictionary<string, object?> payload,
        DateTimeOffset observedAt)
    {
        _sequence += 1;
        return new LiveExportEventEnvelope(
            observedAt,
            _sequence,
            snapshot.RunId,
            kind,
            snapshot.CurrentScreen,
            snapshot.Act,
            snapshot.Floor,
            payload);
    }

    private static IReadOnlyList<string> DescribeDiff(
        LiveExportSnapshot previous,
        string screen,
        LiveExportPlayerSummary player,
        IReadOnlyList<LiveExportCardSummary> deck,
        IReadOnlyList<string> relics,
        IReadOnlyList<string> potions,
        IReadOnlyList<LiveExportChoiceSummary> choices,
        string triggerKind)
    {
        var changes = new List<string>
        {
            $"trigger: {triggerKind}",
        };

        if (previous.Player.CurrentHp != player.CurrentHp || previous.Player.MaxHp != player.MaxHp)
        {
            changes.Add($"hp -> {player.CurrentHp}/{player.MaxHp}");
        }

        if (previous.Player.Gold != player.Gold)
        {
            changes.Add($"gold -> {player.Gold}");
        }

        if (!previous.Relics.SequenceEqual(relics, StringComparer.Ordinal))
        {
            changes.Add($"relics -> {string.Join(", ", relics.DefaultIfEmpty("none"))}");
        }

        if (!string.Equals(previous.CurrentScreen, screen, StringComparison.Ordinal))
        {
            changes.Add($"screen -> {screen}");
        }

        if (!previous.Potions.SequenceEqual(potions, StringComparer.Ordinal))
        {
            changes.Add($"potions -> {string.Join(", ", potions.DefaultIfEmpty("none"))}");
        }

        if (!previous.Deck.Select(card => card.Name).SequenceEqual(deck.Select(card => card.Name), StringComparer.Ordinal))
        {
            changes.Add($"deck_count -> {deck.Count}");
        }

        if (!previous.CurrentChoices.Select(choice => choice.Label).SequenceEqual(choices.Select(choice => choice.Label), StringComparer.Ordinal))
        {
            changes.Add($"choices -> {choices.Count}");
        }

        return changes;
    }

    private static IReadOnlyList<string> MergeWarnings(
        IReadOnlyList<string> previousWarnings,
        IReadOnlyList<string>? newWarnings)
    {
        if (newWarnings is null || newWarnings.Count == 0)
        {
            return previousWarnings;
        }

        return previousWarnings
            .Concat(newWarnings)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyDictionary<string, string?> MergeMeta(
        IReadOnlyDictionary<string, string?> previousMeta,
        IReadOnlyDictionary<string, string?> newMeta)
    {
        if (newMeta.Count == 0)
        {
            return previousMeta;
        }

        var merged = new Dictionary<string, string?>(previousMeta, StringComparer.OrdinalIgnoreCase);
        foreach (var entry in newMeta)
        {
            merged[entry.Key] = entry.Value;
        }

        return merged;
    }

    private static string? InferRunStatus(string triggerKind)
    {
        return triggerKind switch
        {
            "run-started" => "active",
            "run-loaded" => "active",
            "combat-started" => "combat",
            "combat-ended" => "active",
            "run-ended" => "ended",
            _ => null,
        };
    }

    private static string Coalesce(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }

    private static string MergeScreen(LiveExportSnapshot previous, LiveExportObservation observation)
    {
        var incoming = Coalesce(observation.Screen, previous.CurrentScreen, "unknown");
        if (!string.Equals(observation.TriggerKind, "runtime-poll", StringComparison.Ordinal))
        {
            return incoming;
        }

        if (!IsStickyHighValueScreen(previous.CurrentScreen)
            || !IsFallbackScreen(incoming)
            || observation.ObservedAt - previous.CapturedAt > TimeSpan.FromSeconds(2))
        {
            return incoming;
        }

        return previous.CurrentScreen;
    }

    private static LiveExportPlayerSummary MergePlayer(LiveExportPlayerSummary previous, LiveExportPlayerSummary? incoming)
    {
        if (incoming is null)
        {
            return previous;
        }

        var resources = new Dictionary<string, string?>(previous.Resources, StringComparer.OrdinalIgnoreCase);
        foreach (var resource in incoming.Resources)
        {
            if (!string.IsNullOrWhiteSpace(resource.Value))
            {
                resources[resource.Key] = resource.Value;
            }
        }

        return new LiveExportPlayerSummary(
            incoming.Name ?? previous.Name,
            incoming.CurrentHp ?? previous.CurrentHp,
            incoming.MaxHp ?? previous.MaxHp,
            incoming.Gold ?? previous.Gold,
            incoming.Energy ?? previous.Energy,
            resources);
    }

    private IReadOnlyList<LiveExportCardSummary> MergeCards(
        IReadOnlyList<LiveExportCardSummary> previous,
        IReadOnlyList<LiveExportCardSummary>? incoming,
        LiveExportObservation observation)
    {
        if (incoming is null)
        {
            return previous;
        }

        if (incoming.Count == 0 && ShouldPreservePreviousCollection(previous.Count, observation))
        {
            return previous;
        }

        return incoming.Take(_options.MaxDeckEntries).ToArray();
    }

    private static IReadOnlyList<string> MergeStrings(
        IReadOnlyList<string> previous,
        IReadOnlyList<string>? incoming,
        LiveExportObservation observation)
    {
        if (incoming is null)
        {
            return previous;
        }

        if (incoming.Count == 0 && ShouldPreservePreviousCollection(previous.Count, observation))
        {
            return previous;
        }

        return incoming.ToArray();
    }

    private IReadOnlyList<LiveExportChoiceSummary> MergeChoices(
        IReadOnlyList<LiveExportChoiceSummary> previous,
        IReadOnlyList<LiveExportChoiceSummary>? incoming,
        LiveExportObservation observation,
        string previousScreen,
        string nextScreen)
    {
        if (incoming is null)
        {
            return previous;
        }

        if (incoming.Count == 0
            && previous.Count > 0
            && IsChoiceLikeScreen(previousScreen)
            && (string.Equals(previousScreen, nextScreen, StringComparison.Ordinal)
                || (string.Equals(observation.TriggerKind, "runtime-poll", StringComparison.Ordinal) && IsFallbackScreen(nextScreen)))
            && HasChoiceResolutionWarning(observation))
        {
            return previous;
        }

        return incoming.Take(_options.MaxChoiceEntries).ToArray();
    }

    private static LiveExportEncounterSummary? MergeEncounter(
        LiveExportEncounterSummary? previous,
        LiveExportEncounterSummary? incoming,
        LiveExportObservation observation)
    {
        if (incoming is null)
        {
            return previous;
        }

        if (HasPartialStateWarning(observation))
        {
            return new LiveExportEncounterSummary(
                incoming.Name ?? previous?.Name,
                incoming.Kind ?? previous?.Kind,
                incoming.InCombat ?? previous?.InCombat,
                incoming.Turn ?? previous?.Turn);
        }

        return incoming;
    }

    private static bool HasPartialStateWarning(LiveExportObservation observation)
    {
        return observation.Warnings?.Any(warning => warning.Contains("core run data was not resolved", StringComparison.OrdinalIgnoreCase)) == true;
    }

    private static bool HasChoiceResolutionWarning(LiveExportObservation observation)
    {
        return observation.Warnings?.Any(warning => warning.Contains("no visible choices resolved", StringComparison.OrdinalIgnoreCase)) == true;
    }

    private static bool ShouldPreservePreviousCollection(int previousCount, LiveExportObservation observation)
    {
        return previousCount > 0 && HasPartialStateWarning(observation);
    }

    private static bool IsStickyHighValueScreen(string screen)
    {
        return screen is "rewards" or "event" or "rest-site" or "shop" or "card-choice" or "upgrade" or "transform";
    }

    private static bool IsChoiceLikeScreen(string screen)
    {
        return IsStickyHighValueScreen(screen);
    }

    private static bool IsFallbackScreen(string screen)
    {
        return screen is "" or "unknown" or "combat" or "map";
    }
}
