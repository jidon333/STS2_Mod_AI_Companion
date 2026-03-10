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
        var screen = Coalesce(observation.Screen, previous.CurrentScreen, "unknown");
        var player = observation.Player ?? previous.Player;
        var deck = observation.Deck is null
            ? previous.Deck
            : observation.Deck.Take(_options.MaxDeckEntries).ToArray();
        var relics = observation.Relics is null
            ? previous.Relics
            : observation.Relics.ToArray();
        var potions = observation.Potions is null
            ? previous.Potions
            : observation.Potions.ToArray();
        var choices = observation.Choices is null
            ? previous.CurrentChoices
            : observation.Choices.Take(_options.MaxChoiceEntries).ToArray();
        var warnings = MergeWarnings(previous.Warnings, observation.Warnings);
        var encounter = observation.Encounter ?? previous.Encounter;

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
}
