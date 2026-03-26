namespace Sts2ModKit.Core.LiveExport;

public sealed class LiveExportStateTracker
{
    private readonly LiveExportStateTrackerOptions _options;
    private readonly string _sessionId;
    private readonly string _liveRoot;
    private LiveExportSnapshot _snapshot;
    private long _sequence;
    private string? _activeScreenEpisode;
    private DateTimeOffset? _activeScreenEpisodeStartedAt;
    private string? _lastSemanticScreen;
    private string? _lastAcceptedExtractorPath;
    private string? _lastDegradedReason;

    public LiveExportStateTracker(LiveExportStateTrackerOptions options, string liveRoot, string? seedRunId = null)
    {
        _options = options;
        _liveRoot = liveRoot;
        _sessionId = Guid.NewGuid().ToString("N");
        _snapshot = LiveExportSnapshot.CreateEmpty(seedRunId ?? $"pending-{_sessionId[..8]}");
    }

    public LiveExportBatch Apply(LiveExportObservation observation)
    {
        var previousSnapshot = _snapshot;
        var nextSnapshot = MergeSnapshot(_snapshot, observation);
        var collectorStatus = UpdateCollectorStatus(nextSnapshot, observation);
        nextSnapshot = nextSnapshot with
        {
            Meta = ApplyScreenMeta(nextSnapshot.Meta, previousSnapshot.Meta, observation, nextSnapshot.CurrentScreen),
        };
        var events = new List<LiveExportEventEnvelope>();
        events.AddRange(BuildDerivedEvents(previousSnapshot, nextSnapshot, observation));
        events.Add(CreateEnvelope(observation.TriggerKind, nextSnapshot, observation.Payload, observation.ObservedAt));
        var screenTransitions = BuildScreenTransitions(previousSnapshot, nextSnapshot, observation);

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

        return new LiveExportBatch(nextSnapshot, session, events)
        {
            SourceObservation = observation,
            ScreenTransitions = screenTransitions,
            CollectorStatus = collectorStatus,
        };
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

    private IReadOnlyList<LiveExportScreenTransition> BuildScreenTransitions(
        LiveExportSnapshot previous,
        LiveExportSnapshot next,
        LiveExportObservation observation)
    {
        if (!_options.CollectorModeEnabled)
        {
            return Array.Empty<LiveExportScreenTransition>();
        }

        var incoming = Coalesce(observation.SemanticScreen, observation.Screen, previous.CurrentScreen, "unknown");
        var keptPrevious = string.Equals(next.CurrentScreen, previous.CurrentScreen, StringComparison.Ordinal)
            && !string.Equals(incoming, next.CurrentScreen, StringComparison.Ordinal);
        if (!keptPrevious
            && string.Equals(previous.CurrentScreen, next.CurrentScreen, StringComparison.Ordinal)
            && string.IsNullOrWhiteSpace(observation.SemanticScreen))
        {
            return Array.Empty<LiveExportScreenTransition>();
        }

        var reason = keptPrevious
            ? "collector-kept-high-value-screen"
            : string.Equals(previous.CurrentScreen, next.CurrentScreen, StringComparison.Ordinal)
                ? "semantic-screen-confirmed"
                : "screen-updated";

        return new[]
        {
            new LiveExportScreenTransition(
                observation.ObservedAt,
                observation.TriggerKind,
                previous.CurrentScreen,
                incoming,
                next.CurrentScreen,
                reason,
                keptPrevious),
        };
    }

    private LiveExportCollectorStatus UpdateCollectorStatus(
        LiveExportSnapshot snapshot,
        LiveExportObservation observation)
    {
        if (!_options.CollectorModeEnabled)
        {
            return new LiveExportCollectorStatus(false, null, null, null, "disabled", null);
        }

        if (!string.IsNullOrWhiteSpace(observation.SemanticScreen))
        {
            _lastSemanticScreen = observation.SemanticScreen;
            _activeScreenEpisode = observation.SemanticScreen;
            _activeScreenEpisodeStartedAt = observation.ObservedAt;
        }
        else if (!string.IsNullOrWhiteSpace(snapshot.CurrentScreen) && !IsFallbackScreen(snapshot.CurrentScreen))
        {
            _activeScreenEpisode = snapshot.CurrentScreen;
            _activeScreenEpisodeStartedAt ??= observation.ObservedAt;
        }
        else if (_activeScreenEpisodeStartedAt is not null
                 && observation.ObservedAt - _activeScreenEpisodeStartedAt > TimeSpan.FromSeconds(15))
        {
            _activeScreenEpisode = null;
            _activeScreenEpisodeStartedAt = null;
        }

        if (!string.IsNullOrWhiteSpace(observation.ChoiceDecision?.ExtractorPath)
            && observation.ChoiceDecision.AcceptedCount > 0)
        {
            _lastAcceptedExtractorPath = observation.ChoiceDecision.ExtractorPath;
        }

        var degradedReason = observation.ChoiceDecision?.FailureReason
                             ?? observation.Warnings?.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(degradedReason))
        {
            _lastDegradedReason = degradedReason;
        }

        var choiceStatus = observation.ChoiceDecision switch
        {
            { AcceptedCount: > 0 } decision => $"resolved ({decision.AcceptedCount})",
            { CandidateCount: > 0 } => "missing",
            _ when observation.Choices?.Count > 0 => $"resolved ({observation.Choices.Count})",
            _ => "not-seen",
        };

        return new LiveExportCollectorStatus(
            true,
            _activeScreenEpisode,
            _lastSemanticScreen,
            _lastAcceptedExtractorPath,
            choiceStatus,
            _lastDegradedReason);
    }

    private LiveExportSnapshot MergeSnapshot(LiveExportSnapshot previous, LiveExportObservation observation)
    {
        var regressionWarnings = new List<string>();
        var runId = Coalesce(observation.RunId, previous.RunId);
        var screen = MergeScreen(previous, observation);
        var player = MergePlayer(previous.Player, observation.Player, screen, regressionWarnings);
        var deck = MergeCards(previous.Deck, observation.Deck, observation, screen, "deck", regressionWarnings);
        var relics = MergeStrings(previous.Relics, observation.Relics, observation, screen, "relics", regressionWarnings);
        var potions = MergeStrings(previous.Potions, observation.Potions, observation, screen, "potions", regressionWarnings);
        var choices = MergeChoices(previous.CurrentChoices, observation.Choices, observation, previous.CurrentScreen, screen, previous.CapturedAt);
        var warnings = MergeWarnings(previous.Warnings, MergeWarnings(observation.Warnings, regressionWarnings));
        var encounter = MergeEncounter(previous.Encounter, observation.Encounter, observation, screen, regressionWarnings);
        if (!string.Equals(screen, "combat", StringComparison.OrdinalIgnoreCase)
            && encounter?.InCombat == true)
        {
            warnings = MergeWarnings(warnings, new[] { $"state-regression: combat-conflict-with-screen:{screen}" });
        }
        var meta = ApplyScreenMeta(MergeMeta(previous.Meta, observation.Meta), previous.Meta, observation, screen);

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
            Meta = meta,
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

    private IReadOnlyDictionary<string, string?> ApplyScreenMeta(
        IReadOnlyDictionary<string, string?> mergedMeta,
        IReadOnlyDictionary<string, string?> previousMeta,
        LiveExportObservation observation,
        string logicalScreen)
    {
        var visibleScreen = ResolveVisibleScreen(previousMeta, mergedMeta, observation, logicalScreen);
        var sceneReady = ResolveSceneReady(logicalScreen, visibleScreen, observation, mergedMeta);
        var sceneAuthority = ResolveSceneAuthority(observation, mergedMeta);
        var sceneStability = ResolveSceneStability(logicalScreen, sceneReady, mergedMeta);
        var updated = new Dictionary<string, string?>(mergedMeta, StringComparer.OrdinalIgnoreCase)
        {
            ["screen"] = logicalScreen,
            ["logicalScreen"] = logicalScreen,
            ["flowScreen"] = logicalScreen,
            ["visibleScreen"] = visibleScreen,
            ["screen-episode"] = _activeScreenEpisode,
            ["sceneReady"] = sceneReady ? "true" : "false",
            ["sceneAuthority"] = sceneAuthority,
            ["sceneStability"] = sceneStability,
            ["readyMarker"] = sceneReady ? observation.TriggerKind : "waiting-for-stable-scene",
        };

        if (mergedMeta.TryGetValue("screen", out var rawObservedScreen)
            && !string.IsNullOrWhiteSpace(rawObservedScreen))
        {
            updated["rawObservedScreen"] = rawObservedScreen;
        }

        return updated;
    }

    private static bool ResolveSceneReady(
        string logicalScreen,
        string visibleScreen,
        LiveExportObservation observation,
        IReadOnlyDictionary<string, string?> mergedMeta)
    {
        if (string.IsNullOrWhiteSpace(logicalScreen)
            || logicalScreen is "unknown" or "startup" or "bootstrap" or "shutdown")
        {
            return false;
        }

        if (IsBlockingModal(mergedMeta))
        {
            return false;
        }

        if (string.Equals(logicalScreen, "rewards", StringComparison.OrdinalIgnoreCase)
            && string.Equals(visibleScreen, "map", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(logicalScreen, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return observation.Encounter?.InCombat == true;
        }

        return true;
    }

    private static string ResolveSceneAuthority(
        LiveExportObservation observation,
        IReadOnlyDictionary<string, string?> mergedMeta)
    {
        if (!string.Equals(observation.TriggerKind, "runtime-poll", StringComparison.OrdinalIgnoreCase))
        {
            return "hook";
        }

        if (ContainsTypeMarker(mergedMeta, "currentSceneType", "N")
            || ContainsTypeMarker(mergedMeta, "rootTypeSummary", "N"))
        {
            return "mixed";
        }

        return "polling";
    }

    private static string ResolveSceneStability(
        string logicalScreen,
        bool sceneReady,
        IReadOnlyDictionary<string, string?> mergedMeta)
    {
        if (IsBlockingModal(mergedMeta))
        {
            return "blocked";
        }

        if (string.IsNullOrWhiteSpace(logicalScreen)
            || logicalScreen is "unknown" or "startup" or "bootstrap" or "shutdown")
        {
            return "transient";
        }

        return sceneReady ? "stable" : "stabilizing";
    }

    private static string ResolveVisibleScreen(
        IReadOnlyDictionary<string, string?> previousMeta,
        IReadOnlyDictionary<string, string?> mergedMeta,
        LiveExportObservation observation,
        string logicalScreen)
    {
        if (string.IsNullOrWhiteSpace(logicalScreen))
        {
            return "unknown";
        }

        if (string.Equals(logicalScreen, "rewards", StringComparison.OrdinalIgnoreCase))
        {
            if (LooksLikeVisibleMap(mergedMeta))
            {
                return "map";
            }

            if (previousMeta.TryGetValue("visibleScreen", out var previousVisible)
                && string.Equals(previousVisible, "map", StringComparison.OrdinalIgnoreCase)
                && HasMapMarker(mergedMeta))
            {
                return "map";
            }

            if (LooksLikeVisibleRewards(mergedMeta))
            {
                return "rewards";
            }
        }

        return logicalScreen;
    }

    private static bool LooksLikeVisibleMap(IReadOnlyDictionary<string, string?> meta)
    {
        return ContainsTypeMarker(meta, "instanceType", "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen")
               || ContainsTypeMarker(meta, "declaringType", "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen");
    }

    private static bool LooksLikeVisibleRewards(IReadOnlyDictionary<string, string?> meta)
    {
        return ContainsTypeMarker(meta, "instanceType", "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen")
               || ContainsTypeMarker(meta, "declaringType", "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen")
               || ContainsTypeMarker(meta, "instanceType", "MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NCardRewardSelectionScreen")
               || ContainsTypeMarker(meta, "declaringType", "MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NCardRewardSelectionScreen");
    }

    private static bool HasMapMarker(IReadOnlyDictionary<string, string?> meta)
    {
        return meta.TryGetValue("rootTypeSummary", out var rootTypeSummary)
               && !string.IsNullOrWhiteSpace(rootTypeSummary)
               && rootTypeSummary.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBlockingModal(IReadOnlyDictionary<string, string?> meta)
    {
        return meta.TryGetValue("modal-blocking", out var blockingValue)
               && bool.TryParse(blockingValue, out var blocking)
               && blocking;
    }

    private static bool ContainsTypeMarker(
        IReadOnlyDictionary<string, string?> meta,
        string key,
        string marker)
    {
        return meta.TryGetValue(key, out var value)
               && !string.IsNullOrWhiteSpace(value)
               && value.Contains(marker, StringComparison.OrdinalIgnoreCase);
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

    private string MergeScreen(LiveExportSnapshot previous, LiveExportObservation observation)
    {
        var incoming = Coalesce(observation.SemanticScreen, observation.Screen, previous.CurrentScreen, "unknown");
        if (!string.Equals(observation.TriggerKind, "runtime-poll", StringComparison.Ordinal))
        {
            return incoming;
        }

        var stickyScreen = _activeScreenEpisode ?? previous.CurrentScreen;
        var withinEpisodeWindow = _activeScreenEpisodeStartedAt is not null
                                  && observation.ObservedAt - _activeScreenEpisodeStartedAt <= TimeSpan.FromSeconds(45);
        if (observation.Encounter?.InCombat == true
            && (string.Equals(previous.CurrentScreen, "combat", StringComparison.Ordinal)
                || previous.Encounter?.InCombat == true)
            && !string.Equals(incoming, "combat", StringComparison.Ordinal))
        {
            return "combat";
        }

        if (string.Equals(incoming, "combat", StringComparison.Ordinal)
            && observation.Encounter?.InCombat == true
            && observation.Meta.TryGetValue("combatPrimarySource", out var combatPrimarySource)
            && string.Equals(combatPrimarySource, "CombatManager.IsInProgress", StringComparison.Ordinal)
            && observation.Meta.TryGetValue("combatPrimaryValue", out var combatPrimaryValue)
            && string.Equals(combatPrimaryValue, "true", StringComparison.OrdinalIgnoreCase))
        {
            return "combat";
        }

        if (string.Equals(stickyScreen, "combat", StringComparison.Ordinal)
            && (observation.Encounter?.InCombat == true
                || previous.Encounter?.InCombat == true
                || withinEpisodeWindow))
        {
            return stickyScreen;
        }

        if (!IsStickyHighValueScreen(stickyScreen)
            || !IsFallbackScreen(incoming))
        {
            return incoming;
        }

        if (HasPartialStateWarning(observation)
            || HasChoiceResolutionWarning(observation)
            || (previous.CurrentChoices.Count > 0 && (observation.Choices?.Count ?? 0) == 0)
            || (IsStickyHighValueScreen(previous.CurrentScreen) && observation.Encounter?.InCombat == true)
            || withinEpisodeWindow
            || observation.ObservedAt - previous.CapturedAt <= TimeSpan.FromSeconds(20))
        {
            return stickyScreen;
        }

        return incoming;
    }

    private static LiveExportPlayerSummary MergePlayer(
        LiveExportPlayerSummary previous,
        LiveExportPlayerSummary? incoming,
        string screen,
        ICollection<string> regressionWarnings)
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

        var incomingName = incoming.Name;
        if (!string.IsNullOrWhiteSpace(incomingName)
            && !IsAuthoritativePlayerName(incomingName, screen)
            && !string.IsNullOrWhiteSpace(previous.Name))
        {
            regressionWarnings.Add($"state-regression: preserved-player-name over non-authoritative value {incomingName}");
            incomingName = previous.Name;
        }

        return new LiveExportPlayerSummary(
            incomingName ?? previous.Name,
            incoming.CurrentHp ?? previous.CurrentHp,
            incoming.MaxHp ?? previous.MaxHp,
            incoming.Gold ?? previous.Gold,
            incoming.Energy ?? previous.Energy,
            resources);
    }

    private IReadOnlyList<LiveExportCardSummary> MergeCards(
        IReadOnlyList<LiveExportCardSummary> previous,
        IReadOnlyList<LiveExportCardSummary>? incoming,
        LiveExportObservation observation,
        string screen,
        string label,
        ICollection<string> regressionWarnings)
    {
        if (incoming is null)
        {
            return previous;
        }

        if (incoming.Count == 0 && ShouldPreservePreviousCollection(previous.Count, observation, screen))
        {
            regressionWarnings.Add($"state-regression: preserved-{label}-from-empty");
            return previous;
        }

        return incoming.Take(_options.MaxDeckEntries).ToArray();
    }

    private static IReadOnlyList<string> MergeStrings(
        IReadOnlyList<string> previous,
        IReadOnlyList<string>? incoming,
        LiveExportObservation observation,
        string screen,
        string label,
        ICollection<string> regressionWarnings)
    {
        if (incoming is null)
        {
            return previous;
        }

        if (incoming.Count == 0 && ShouldPreservePreviousCollection(previous.Count, observation, screen))
        {
            regressionWarnings.Add($"state-regression: preserved-{label}-from-empty");
            return previous;
        }

        return incoming.ToArray();
    }

    private IReadOnlyList<LiveExportChoiceSummary> MergeChoices(
        IReadOnlyList<LiveExportChoiceSummary> previous,
        IReadOnlyList<LiveExportChoiceSummary>? incoming,
        LiveExportObservation observation,
        string previousScreen,
        string nextScreen,
        DateTimeOffset previousCapturedAt)
    {
        if (incoming is null)
        {
            return previous;
        }

        incoming = incoming
            .Where(choice => !IsOverlayChoice(choice))
            .Take(_options.MaxChoiceEntries)
            .ToArray();

        if (incoming.Count == 0
            && previous.Count > 0
            && (IsChoiceLikeScreen(previousScreen) || IsChoiceLikeScreen(nextScreen))
            && (string.Equals(previousScreen, nextScreen, StringComparison.Ordinal)
                || (string.Equals(observation.TriggerKind, "runtime-poll", StringComparison.Ordinal) && IsFallbackScreen(nextScreen)))
            && (HasChoiceResolutionWarning(observation)
                || string.Equals(observation.TriggerKind, "runtime-poll", StringComparison.Ordinal)
                || !string.IsNullOrWhiteSpace(observation.SemanticScreen)
                || observation.ObservedAt - previousCapturedAt <= TimeSpan.FromSeconds(20)))
        {
            return previous;
        }

        return incoming.Take(_options.MaxChoiceEntries).ToArray();
    }

    private static LiveExportEncounterSummary? MergeEncounter(
        LiveExportEncounterSummary? previous,
        LiveExportEncounterSummary? incoming,
        LiveExportObservation observation,
        string screen,
        ICollection<string> regressionWarnings)
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

        if (IsStickyHighValueScreen(screen)
            && incoming.InCombat == true
            && previous is not null
            && previous.InCombat != true)
        {
            if (observation.Meta.TryGetValue("combatPrimarySource", out var combatPrimarySource)
                && string.Equals(combatPrimarySource, "CombatManager.IsInProgress", StringComparison.Ordinal))
            {
                return incoming;
            }

            regressionWarnings.Add($"state-regression: preserved-encounter-over-combat-conflict:{screen}");
            return previous with
            {
                Name = previous.Name ?? incoming.Name,
                Kind = previous.Kind ?? incoming.Kind,
                Turn = previous.Turn ?? incoming.Turn,
            };
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

    private static bool ShouldPreservePreviousCollection(int previousCount, LiveExportObservation observation, string screen)
    {
        return previousCount > 0
               && (HasPartialStateWarning(observation)
                   || IsStickyHighValueScreen(screen)
                   || !string.IsNullOrWhiteSpace(observation.SemanticScreen));
    }

    private static bool IsStickyHighValueScreen(string screen)
    {
        return screen is "main-menu"
            or "singleplayer-submenu"
            or "character-select"
            or "map"
            or "combat"
            or "rewards"
            or "event"
            or "rest-site"
            or "shop"
            or "card-choice"
            or "upgrade"
            or "transform";
    }

    private static bool IsChoiceLikeScreen(string screen)
    {
        return IsStickyHighValueScreen(screen);
    }

    private static bool IsFallbackScreen(string screen)
    {
        return screen is ""
            or "unknown"
            or "startup"
            or "bootstrap"
            or "combat"
            or "feedback-overlay"
            or "blocking-overlay";
    }

    private static bool IsAuthoritativePlayerName(string incomingName, string screen)
    {
        if (string.IsNullOrWhiteSpace(incomingName))
        {
            return false;
        }

        if (string.Equals(incomingName, screen, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !incomingName.Contains("Screen", StringComparison.OrdinalIgnoreCase)
               && !incomingName.Contains("Room", StringComparison.OrdinalIgnoreCase)
               && !incomingName.Contains("Layout", StringComparison.OrdinalIgnoreCase)
               && !incomingName.Contains("Overlay", StringComparison.OrdinalIgnoreCase)
               && !incomingName.Contains("Feedback", StringComparison.OrdinalIgnoreCase)
               && !incomingName.Contains("Rewards", StringComparison.OrdinalIgnoreCase)
               && !incomingName.Contains("Merchant", StringComparison.OrdinalIgnoreCase)
               && !incomingName.Contains("Holder", StringComparison.OrdinalIgnoreCase)
               && !incomingName.Contains("Container", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOverlayChoice(LiveExportChoiceSummary choice)
    {
        var label = choice.Label?.Trim();
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        return string.Equals(label, "Dismisser", StringComparison.OrdinalIgnoreCase)
               || string.Equals(label, "Exclaim", StringComparison.OrdinalIgnoreCase)
               || string.Equals(label, "Question", StringComparison.OrdinalIgnoreCase)
               || string.Equals(label, "BackButton", StringComparison.OrdinalIgnoreCase)
               || string.Equals(label, "Send!", StringComparison.OrdinalIgnoreCase);
    }
}
