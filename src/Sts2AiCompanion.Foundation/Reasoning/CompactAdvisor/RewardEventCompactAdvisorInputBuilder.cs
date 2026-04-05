using System.Text.RegularExpressions;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

public sealed class RewardEventCompactAdvisorInputBuilder
{
    private const int CompactKnowledgeMaxEntries = 6;
    private const int CompactKnowledgeMaxBytes = 4096;
    private readonly RewardOptionSetBuilder rewardOptionSetBuilder = new();
    private readonly RewardAssessmentFactsBuilder rewardAssessmentFactsBuilder = new();

    public CompactAdvisorBuildResult Build(CompanionRunState runState, KnowledgeSlice boundedSlice)
    {
        ArgumentNullException.ThrowIfNull(runState);
        ArgumentNullException.ThrowIfNull(boundedSlice);

        var sceneType = NormalizeSceneType(runState.NormalizedState.Scene.SceneType);
        return sceneType switch
        {
            "reward" => BuildReward(runState, boundedSlice),
            "rewards" => BuildReward(runState, boundedSlice),
            "event" => BuildEvent(runState, boundedSlice),
            _ => new CompactAdvisorBuildResult(
                false,
                null,
                Array.Empty<string>(),
                new[] { $"unsupported-scene-for-compact-advisor:{sceneType}" },
                $"unsupported-scene-for-compact-advisor:{sceneType}"),
        };
    }

    private CompactAdvisorBuildResult BuildReward(CompanionRunState runState, KnowledgeSlice boundedSlice)
    {
        var options = BuildVisibleOptions(runState, rewardLike: true);
        var missingInformation = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var decisionBlockers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (options.Count == 0)
        {
            decisionBlockers.Add("reward-compact-input-insufficient");
            return new CompactAdvisorBuildResult(false, null, Array.Empty<string>(), decisionBlockers.ToArray(), "reward-compact-input-insufficient");
        }

        foreach (var label in options
                     .GroupBy(option => option.Label, StringComparer.Ordinal)
                     .Where(group => group.Count() > 1)
                     .Select(group => group.Key))
        {
            decisionBlockers.Add($"reward-duplicate-option-label:{label}");
        }

        var rewardOptionSet = rewardOptionSetBuilder.Build(runState);
        var rewardAssessmentFacts = rewardAssessmentFactsBuilder.Build(runState, boundedSlice, rewardOptionSet);
        if (rewardAssessmentFacts is not null)
        {
            foreach (var missing in rewardAssessmentFacts.MissingInformation)
            {
                missingInformation.Add(missing);
            }
        }

        if (rewardOptionSet is not null)
        {
            var visibleLabels = options.Select(option => option.Label).ToHashSet(StringComparer.Ordinal);
            var mappedCandidateCount = 0;
            foreach (var option in rewardOptionSet.Options.Where(option => !option.IsSkipOption))
            {
                if (visibleLabels.Contains(option.Label))
                {
                    mappedCandidateCount += 1;
                }
                else
                {
                    missingInformation.Add($"reward-option-label-mapping-missing:{option.Label}");
                }
            }

            if (mappedCandidateCount == 0)
            {
                decisionBlockers.Add("reward-compact-input-insufficient");
            }
        }

        var compactKnowledge = FilterKnowledgeEntries(
            boundedSlice,
            "reward",
            runState.NormalizedState.Event.EventId,
            runState.NormalizedState.Event.PageId,
            options);
        var rewardFacts = new RewardCompactFacts(
            runState.NormalizedState.Reward.RewardType ?? "generic",
            options.Any(option => IsSkipOption(option)),
            rewardAssessmentFacts?.FactLines
                ?? BuildRewardFallbackFactLines(runState, options),
            rewardAssessmentFacts?.MissingInformation
                ?? missingInformation.ToArray(),
            rewardAssessmentFacts?.KnowledgeRefs
                ?? compactKnowledge.Select(entry => entry.Id).ToArray());
        var compactInput = new RewardEventCompactAdvisorInput(
            SceneType: "reward",
            SceneStage: ResolveRewardStage(runState),
            CanonicalOwner: "reward",
            RunContext: BuildRunContext(runState),
            PlayerSummary: BuildPlayerSummary(runState),
            VisibleOptions: options,
            KnowledgeEntries: compactKnowledge,
            RecentEvents: FilterRecentEvents(runState.RecentEvents, "reward"),
            MissingInformation: missingInformation.ToArray(),
            DecisionBlockers: decisionBlockers.ToArray(),
            RewardFacts: rewardFacts,
            EventFacts: null);
        return decisionBlockers.Contains("reward-compact-input-insufficient")
            ? new CompactAdvisorBuildResult(false, compactInput, compactInput.MissingInformation, compactInput.DecisionBlockers, "reward-compact-input-insufficient")
            : new CompactAdvisorBuildResult(true, compactInput, compactInput.MissingInformation, compactInput.DecisionBlockers, "supported");
    }

    private CompactAdvisorBuildResult BuildEvent(CompanionRunState runState, KnowledgeSlice boundedSlice)
    {
        var options = BuildVisibleOptions(runState, rewardLike: false);
        var missingInformation = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var decisionBlockers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (options.Count == 0)
        {
            decisionBlockers.Add("event-compact-input-insufficient");
            return new CompactAdvisorBuildResult(false, null, Array.Empty<string>(), decisionBlockers.ToArray(), "event-compact-input-insufficient");
        }

        var duplicateLabels = options
            .GroupBy(option => option.Label, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        foreach (var label in duplicateLabels)
        {
            decisionBlockers.Add($"event-duplicate-option-label:{label}");
        }

        var optionFacts = new List<EventCompactOptionFact>();
        foreach (var option in options)
        {
            var effects = ExtractEventEffects(option.Description);
            var optionMissing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (effects.Count == 0 && !LooksLikeProceedOption(option))
            {
                optionMissing.Add($"event-option-effects-missing:{option.Label}");
            }

            foreach (var missing in optionMissing)
            {
                missingInformation.Add(missing);
            }

            optionFacts.Add(new EventCompactOptionFact(
                option.Label,
                option.Value,
                option.Enabled,
                effects,
                optionMissing.ToArray()));
        }

        var eventId = FirstNonBlank(
            runState.NormalizedState.Event.EventId,
            TryGetMeta(runState, "event-id"));
        var pageId = FirstNonBlank(
            runState.NormalizedState.Event.PageId,
            TryGetMeta(runState, "event-page-id"));
        var rewardChildActive = options.Any(option =>
            option.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
            || option.Kind.Contains("card", StringComparison.OrdinalIgnoreCase)
            || option.Tags.Contains("event-child", StringComparer.OrdinalIgnoreCase));
        var proceedVisible = options.Any(LooksLikeProceedOption);
        if (string.IsNullOrWhiteSpace(eventId))
        {
            missingInformation.Add("event-identity-missing");
        }

        if (optionFacts.All(fact => fact.Effects.Count == 0) && !proceedVisible)
        {
            decisionBlockers.Add("event-compact-input-insufficient");
        }

        var compactKnowledge = FilterKnowledgeEntries(
            boundedSlice,
            "event",
            eventId,
            pageId,
            options);
        var eventFacts = new EventCompactFacts(
            eventId,
            string.IsNullOrWhiteSpace(eventId),
            rewardChildActive,
            proceedVisible,
            optionFacts,
            missingInformation.ToArray());
        var compactInput = new RewardEventCompactAdvisorInput(
            SceneType: "event",
            SceneStage: ResolveEventStage(runState),
            CanonicalOwner: "event",
            RunContext: BuildRunContext(runState),
            PlayerSummary: BuildPlayerSummary(runState),
            VisibleOptions: options,
            KnowledgeEntries: compactKnowledge,
            RecentEvents: FilterRecentEvents(runState.RecentEvents, "event"),
            MissingInformation: missingInformation.ToArray(),
            DecisionBlockers: decisionBlockers.ToArray(),
            RewardFacts: null,
            EventFacts: eventFacts);
        return decisionBlockers.Contains("event-compact-input-insufficient")
            ? new CompactAdvisorBuildResult(false, compactInput, compactInput.MissingInformation, compactInput.DecisionBlockers, "event-compact-input-insufficient")
            : new CompactAdvisorBuildResult(true, compactInput, compactInput.MissingInformation, compactInput.DecisionBlockers, "supported");
    }

    private static IReadOnlyList<CompactAdvisorOption> BuildVisibleOptions(CompanionRunState runState, bool rewardLike)
    {
        var sourceItems = rewardLike
            ? (runState.NormalizedState.Reward.Entries.Count > 0
                ? runState.NormalizedState.Reward.Entries
                : runState.NormalizedState.Choices.List)
            : (runState.NormalizedState.Event.Options.Count > 0
                ? runState.NormalizedState.Event.Options
                : runState.NormalizedState.Choices.List);

        return sourceItems
            .Select(item =>
            {
                var snapshotChoice = MatchSnapshotChoice(runState.Snapshot.CurrentChoices, item);
                return new CompactAdvisorOption(
                    item.Kind,
                    item.Label,
                    item.Value,
                    item.Description,
                    snapshotChoice?.Enabled ?? true,
                    BuildOptionTags(item, snapshotChoice));
            })
            .ToArray();
    }

    private static LiveExportChoiceSummary? MatchSnapshotChoice(
        IReadOnlyList<LiveExportChoiceSummary> snapshotChoices,
        CompanionChoiceItem item)
    {
        return snapshotChoices.FirstOrDefault(choice =>
            string.Equals(choice.Label, item.Label, StringComparison.Ordinal)
            && string.Equals(choice.Kind, item.Kind, StringComparison.OrdinalIgnoreCase)
            && string.Equals(choice.Value ?? string.Empty, item.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            ?? snapshotChoices.FirstOrDefault(choice =>
                string.Equals(choice.Label, item.Label, StringComparison.Ordinal)
                && string.Equals(choice.Value ?? string.Empty, item.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            ?? snapshotChoices.FirstOrDefault(choice =>
                string.Equals(choice.Label, item.Label, StringComparison.Ordinal));
    }

    private static IReadOnlyList<string> BuildOptionTags(CompanionChoiceItem item, LiveExportChoiceSummary? snapshotChoice)
    {
        var tags = new List<string>();
        if (IsSkipText(item.Label) || IsSkipText(item.Value))
        {
            tags.Add("skip-option");
        }

        if (snapshotChoice?.SemanticHints is not null)
        {
            tags.AddRange(snapshotChoice.SemanticHints.Where(hint => !string.IsNullOrWhiteSpace(hint)));
        }

        return tags
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<CompactKnowledgeEntry> FilterKnowledgeEntries(
        KnowledgeSlice boundedSlice,
        string sceneType,
        string? eventId,
        string? pageId,
        IReadOnlyList<CompactAdvisorOption> options)
    {
        var seeds = options
            .SelectMany(option => new[] { option.Label, option.Value })
            .Append(sceneType)
            .Append(eventId)
            .Append(pageId)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToArray();
        var filtered = new List<CompactKnowledgeEntry>();
        var bytes = 0;
        foreach (var entry in boundedSlice.Entries)
        {
            if (!MatchesAnySeed(entry, seeds))
            {
                continue;
            }

            var compact = new CompactKnowledgeEntry(
                entry.Id,
                entry.Name,
                FirstNonBlank(entry.RawText, entry.Options.FirstOrDefault()?.Description));
            var delta = compact.Id.Length + compact.Name.Length + (compact.Summary?.Length ?? 0);
            if (filtered.Count >= CompactKnowledgeMaxEntries || bytes + delta > CompactKnowledgeMaxBytes)
            {
                break;
            }

            filtered.Add(compact);
            bytes += delta;
        }

        return filtered;
    }

    private static bool MatchesAnySeed(StaticKnowledgeEntry entry, IReadOnlyList<string> seeds)
    {
        foreach (var seed in seeds)
        {
            if (string.Equals(entry.Id, Normalize(seed), StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Name, seed, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(entry.RawText) && entry.RawText.Contains(seed, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyList<CompactRecentEvent> FilterRecentEvents(
        IReadOnlyList<LiveExportEventEnvelope> recentEvents,
        string sceneType)
    {
        var filtered = recentEvents
            .Where(envelope => IsRelevantRecentEvent(envelope, sceneType))
            .TakeLast(3)
            .Select(envelope => new CompactRecentEvent(
                envelope.Kind,
                envelope.Screen,
                envelope.Act,
                envelope.Floor,
                SummarizePayload(envelope.Payload)))
            .ToArray();
        return filtered;
    }

    private static bool IsRelevantRecentEvent(LiveExportEventEnvelope envelope, string sceneType)
    {
        if (string.Equals(sceneType, "reward", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(envelope.Screen, "reward", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(envelope.Screen, "rewards", StringComparison.OrdinalIgnoreCase)
                   || envelope.Kind is "reward-opened" or "reward-screen-opened" or "choice-list-presented";
        }

        return string.Equals(envelope.Screen, "event", StringComparison.OrdinalIgnoreCase)
               || envelope.Kind is "event-opened" or "event-screen-opened" or "choice-list-presented";
    }

    private static string? SummarizePayload(IReadOnlyDictionary<string, object?> payload)
    {
        if (payload.Count == 0)
        {
            return null;
        }

        return string.Join(", ", payload.Take(3).Select(pair => $"{pair.Key}={pair.Value}"));
    }

    private static CompactRunContext BuildRunContext(CompanionRunState runState)
    {
        return new CompactRunContext(
            runState.Snapshot.Act,
            runState.Snapshot.Floor,
            runState.Snapshot.Player.CurrentHp,
            runState.Snapshot.Player.MaxHp,
            runState.Snapshot.Player.Gold,
            runState.Snapshot.Relics.Count,
            runState.Snapshot.Potions.Count,
            runState.Snapshot.Deck.Count);
    }

    private static CompactPlayerSummary BuildPlayerSummary(CompanionRunState runState)
    {
        var deckCounts = runState.Snapshot.Deck
            .GroupBy(card => string.IsNullOrWhiteSpace(card.Name) ? card.Id ?? "Unknown Card" : card.Name, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .Select(group => $"{group.Key} x{group.Count()}")
            .ToArray();
        var deckSummary = deckCounts.Length == 0
            ? "deck-summary-unavailable"
            : $"{runState.Snapshot.Deck.Count} cards :: {string.Join(", ", deckCounts)}";
        return new CompactPlayerSummary(
            deckSummary,
            runState.Snapshot.Relics.Take(5).ToArray(),
            runState.Snapshot.Potions.Take(3).ToArray());
    }

    private static IReadOnlyList<string> BuildRewardFallbackFactLines(CompanionRunState runState, IReadOnlyList<CompactAdvisorOption> options)
    {
        return new[]
        {
            $"reward_type={runState.NormalizedState.Reward.RewardType ?? "generic"}",
            $"visible_reward_options={string.Join(", ", options.Select(option => option.Label))}",
            $"deck_size={runState.Snapshot.Deck.Count}",
            $"relic_count={runState.Snapshot.Relics.Count}",
            $"potion_count={runState.Snapshot.Potions.Count}",
        };
    }

    private static string ResolveRewardStage(CompanionRunState runState)
    {
        var lane = TryGetMeta(runState, "foregroundActionLane");
        return FirstNonBlank(lane, "reward-claim")!;
    }

    private static string ResolveEventStage(CompanionRunState runState)
    {
        var lane = TryGetMeta(runState, "foregroundActionLane");
        return FirstNonBlank(lane, "event-option")!;
    }

    private static string? TryGetMeta(CompanionRunState runState, string key)
    {
        return runState.Snapshot.Meta.TryGetValue(key, out var value) ? value : null;
    }

    private static List<EventCompactEffect> ExtractEventEffects(string? description)
    {
        var effects = new List<EventCompactEffect>();
        if (string.IsNullOrWhiteSpace(description))
        {
            return effects;
        }

        void Add(string kind, int? amount, string text)
        {
            effects.Add(new EventCompactEffect(kind, amount, text));
        }

        foreach (Match match in Regex.Matches(description, @"최대 체력을\s*(\d+)\s*잃", RegexOptions.IgnoreCase))
        {
            Add("hp_loss", ParseAmount(match, 1), match.Value);
        }

        foreach (Match match in Regex.Matches(description, @"체력을\s*(\d+)\s*회복", RegexOptions.IgnoreCase))
        {
            Add("hp_gain", ParseAmount(match, 1), match.Value);
        }

        foreach (Match match in Regex.Matches(description, @"(\d+)\s*골드를?\s*잃", RegexOptions.IgnoreCase))
        {
            Add("gold_loss", ParseAmount(match, 1), match.Value);
        }

        foreach (Match match in Regex.Matches(description, @"(\d+)\s*골드를?\s*(얻|획득)", RegexOptions.IgnoreCase))
        {
            Add("gold_gain", ParseAmount(match, 1), match.Value);
        }

        if (description.Contains("강화", StringComparison.OrdinalIgnoreCase))
        {
            Add("card_upgrade", null, "카드 강화");
        }

        if (description.Contains("변형", StringComparison.OrdinalIgnoreCase) || description.Contains("변환", StringComparison.OrdinalIgnoreCase))
        {
            Add("card_transform", null, "카드 변형");
        }

        if (description.Contains("제거", StringComparison.OrdinalIgnoreCase) || description.Contains("삭제", StringComparison.OrdinalIgnoreCase))
        {
            Add("card_remove", null, "카드 제거");
        }

        if (description.Contains("카드를", StringComparison.OrdinalIgnoreCase) && description.Contains("얻", StringComparison.OrdinalIgnoreCase))
        {
            Add("card_gain", null, "카드 획득");
        }

        if (description.Contains("유물을", StringComparison.OrdinalIgnoreCase) || description.Contains("유물", StringComparison.OrdinalIgnoreCase) && description.Contains("얻", StringComparison.OrdinalIgnoreCase))
        {
            Add("relic_gain", null, "유물 획득");
        }

        if (description.Contains("포션", StringComparison.OrdinalIgnoreCase) && description.Contains("얻", StringComparison.OrdinalIgnoreCase))
        {
            Add("potion_gain", null, "포션 획득");
        }

        return effects
            .DistinctBy(effect => $"{effect.Kind}:{effect.Amount}:{effect.Text}", StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int? ParseAmount(Match match, int groupIndex)
    {
        return int.TryParse(match.Groups[groupIndex].Value, out var parsed) ? parsed : null;
    }

    private static bool LooksLikeProceedOption(CompactAdvisorOption option)
    {
        return option.Kind.Contains("proceed", StringComparison.OrdinalIgnoreCase)
               || option.Label.Contains("계속", StringComparison.OrdinalIgnoreCase)
               || option.Label.Contains("진행", StringComparison.OrdinalIgnoreCase)
               || option.Label.Contains("완료", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSkipOption(CompactAdvisorOption option)
    {
        return option.Tags.Contains("skip-option", StringComparer.OrdinalIgnoreCase)
               || IsSkipText(option.Label)
               || IsSkipText(option.Value);
    }

    private static bool IsSkipText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Contains("skip", StringComparison.OrdinalIgnoreCase)
               || value.Contains("넘기기", StringComparison.OrdinalIgnoreCase)
               || value.Contains("건너뛰기", StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string value)
    {
        return new string(value
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray())
            .Trim('-');
    }

    private static string NormalizeSceneType(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim().ToLowerInvariant();
    }

    private static string? FirstNonBlank(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
