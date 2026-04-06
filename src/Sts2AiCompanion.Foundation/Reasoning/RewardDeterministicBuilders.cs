using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Foundation.State;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Foundation.Reasoning;

public sealed class RewardOptionSetBuilder
{
    public RewardOptionSet? Build(CompanionRunState runState)
    {
        if (!IsCardRewardScene(runState))
        {
            return null;
        }

        var sourceEntries = GetRewardSourceEntries(runState);
        if (sourceEntries.Count == 0)
        {
            return null;
        }

        var options = sourceEntries
            .Select((entry, index) => new RewardOption(
                index,
                entry.Kind,
                entry.Label,
                entry.Value,
                entry.Description,
                IsSkipOption(entry)))
            .ToArray();

        var skipAllowed = options.Any(option => option.IsSkipOption);
        var summaryText = string.Join(
            " | ",
            options.Select(option =>
            {
                var skipMarker = option.IsSkipOption ? " skip" : string.Empty;
                var valuePart = string.IsNullOrWhiteSpace(option.Value) ? string.Empty : $" value={option.Value}";
                return $"[{option.Ordinal}] {option.Label} kind={option.Kind}{valuePart}{skipMarker}";
            }));

        return new RewardOptionSet(runState.NormalizedState.Scene.SceneType, skipAllowed, summaryText, options);
    }

    internal static bool IsCardRewardScene(CompanionRunState runState)
    {
        if (!IsRewardScene(runState.NormalizedState.Scene.SceneType))
        {
            return false;
        }

        if (string.Equals(runState.NormalizedState.Reward.RewardType, "card", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var visibleEntries = runState.NormalizedState.Reward.Entries.Count > 0
            ? runState.NormalizedState.Reward.Entries
            : runState.NormalizedState.Choices.List;
        return visibleEntries.Any(entry => string.Equals(entry.Kind, "card", StringComparison.OrdinalIgnoreCase));
    }

    internal static IReadOnlyList<CompanionChoiceItem> GetRewardSourceEntries(CompanionRunState runState)
    {
        var canonicalSnapshotChoices = GetCanonicalCardRewardSnapshotChoices(runState.Snapshot.CurrentChoices);
        if (canonicalSnapshotChoices.Count > 0)
        {
            return canonicalSnapshotChoices
                .Select(choice => new CompanionChoiceItem(
                    choice.Kind,
                    choice.Label,
                    choice.Value,
                    choice.Description))
                .ToArray();
        }

        return runState.NormalizedState.Reward.Entries.Count > 0
            ? runState.NormalizedState.Reward.Entries
            : runState.NormalizedState.Choices.List;
    }

    internal static IReadOnlyList<LiveExportChoiceSummary> GetCanonicalCardRewardSnapshotChoices(
        IReadOnlyList<LiveExportChoiceSummary> snapshotChoices)
    {
        var sanitizedChoices = CompanionSceneNormalizer.SanitizeChoices(snapshotChoices);
        var rewardPickChoices = sanitizedChoices
            .Where(IsRewardPickChoice)
            .GroupBy(
                choice => $"{choice.Kind}|{choice.Label}|{choice.Value ?? string.Empty}",
                StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
        if (rewardPickChoices.Length == 0)
        {
            return Array.Empty<LiveExportChoiceSummary>();
        }

        var canonicalChoices = new List<LiveExportChoiceSummary>(rewardPickChoices);
        var skipChoice = SelectCanonicalSkipChoice(sanitizedChoices);
        if (skipChoice is not null)
        {
            canonicalChoices.Add(skipChoice);
        }

        return canonicalChoices;
    }

    private static bool IsRewardScene(string? sceneType)
    {
        return string.Equals(sceneType, "reward", StringComparison.OrdinalIgnoreCase)
               || string.Equals(sceneType, "rewards", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRewardPickChoice(LiveExportChoiceSummary choice)
    {
        return string.Equals(choice.Kind, "reward-pick-card", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Contains("reward-pick", StringComparer.OrdinalIgnoreCase)
               || (string.Equals(choice.BindingKind, "card-selection-card", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(choice.BindingId, "reward-pick", StringComparison.OrdinalIgnoreCase));
    }

    private static LiveExportChoiceSummary? SelectCanonicalSkipChoice(IReadOnlyList<LiveExportChoiceSummary> snapshotChoices)
    {
        return snapshotChoices
            .Where(IsRewardSkipChoice)
            .OrderByDescending(choice => string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(choice => !string.IsNullOrWhiteSpace(choice.NodeId))
            .ThenByDescending(choice => choice.Enabled == true)
            .ThenBy(choice => choice.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    private static bool IsRewardSkipChoice(LiveExportChoiceSummary choice)
    {
        if (!ContainsSkipMarker(choice.Label) && !ContainsSkipMarker(choice.Value))
        {
            return false;
        }

        return string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.Value, "ui_cancel", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Contains("skip-option", StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsSkipOption(CompanionChoiceItem entry)
    {
        return ContainsSkipMarker(entry.Label) || ContainsSkipMarker(entry.Value) || ContainsSkipMarker(entry.Description);
    }

    private static bool ContainsSkipMarker(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Contains("skip", StringComparison.OrdinalIgnoreCase)
               || value.Contains("넘기기", StringComparison.OrdinalIgnoreCase)
               || value.Contains("건너뛰기", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class RewardAssessmentFactsBuilder
{
    private readonly RewardOptionFactExtractor _directFactExtractor = new();
    private readonly RewardExactKnowledgeResolver _knowledgeResolver = new();

    public RewardAssessmentFacts? Build(
        CompanionRunState runState,
        KnowledgeSlice slice,
        RewardOptionSet? optionSet,
        StaticKnowledgeCatalog? catalog = null)
    {
        if (optionSet is null || !RewardOptionSetBuilder.IsCardRewardScene(runState))
        {
            return null;
        }

        var deck = runState.Snapshot.Deck;
        var attackCount = deck.Count(card => string.Equals(card.Type, "attack", StringComparison.OrdinalIgnoreCase));
        var skillCount = deck.Count(card => string.Equals(card.Type, "skill", StringComparison.OrdinalIgnoreCase));
        var powerCount = deck.Count(card => string.Equals(card.Type, "power", StringComparison.OrdinalIgnoreCase));
        var drawTaggedCardCount = CountTaggedDeckCards(deck, slice, catalog, HasDrawTag);
        var blockTaggedCardCount = CountTaggedDeckCards(deck, slice, catalog, HasBlockTag);
        var energyTaggedCardCount = CountTaggedDeckCards(deck, slice, catalog, HasEnergyTag);
        var synergyHints = new List<string>();
        var antiSynergyHints = new List<string>();
        var missingInformation = new List<string>();
        var knowledgeRefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var directFacts = _directFactExtractor.Extract(optionSet)
            .Where(facts => !facts.Option.IsSkipOption)
            .ToArray();

        foreach (var facts in directFacts)
        {
            var option = facts.Option;
            var match = _knowledgeResolver.Resolve(facts, slice, catalog);
            var hasDrawSignal = facts.HasDrawSignal;
            var hasBlockSignal = facts.HasBlockSignal;
            var hasEnergySignal = facts.HasEnergySignal;
            var usedKnowledge = false;
            if (match is not null)
            {
                if (!hasDrawSignal && HasDrawTag(match.Entry))
                {
                    hasDrawSignal = true;
                    usedKnowledge = true;
                }

                if (!hasBlockSignal && HasBlockTag(match.Entry))
                {
                    hasBlockSignal = true;
                    usedKnowledge = true;
                }

                if (!hasEnergySignal && HasEnergyTag(match.Entry))
                {
                    hasEnergySignal = true;
                    usedKnowledge = true;
                }

                if (usedKnowledge)
                {
                    knowledgeRefs.Add(match.Entry.Id);
                }
            }

            if (hasDrawSignal && drawTaggedCardCount == 0)
            {
                synergyHints.Add($"{option.Label}: adds draw support");
            }

            if (hasBlockSignal && blockTaggedCardCount == 0)
            {
                synergyHints.Add($"{option.Label}: adds block support");
            }

            if (hasEnergySignal && energyTaggedCardCount == 0)
            {
                synergyHints.Add($"{option.Label}: adds energy support");
            }

            if (!facts.HasUsableDescription && match is null)
            {
                missingInformation.Add($"reward-option-knowledge-missing:{option.Label}");
            }

            if (DeckAlreadyContains(deck, option))
            {
                antiSynergyHints.Add($"{option.Label}: duplicate copy already in deck");
            }
        }

        if (!optionSet.SkipAllowed)
        {
            missingInformation.Add("reward-skip-option-not-visible");
        }

        var deckSize = deck.Count;
        var attackPressure = DescribePressure(attackCount, deckSize);
        var defensePressure = DescribePressure(blockTaggedCardCount, deckSize);
        var drawSupportLevel = DescribeSupport(drawTaggedCardCount);
        var energySupportLevel = DescribeSupport(energyTaggedCardCount);
        var knowledgeFingerprint = BuildKnowledgeFingerprint(slice);
        var factLines = new[]
        {
            $"deck_size={deckSize}",
            $"attack_cards={attackCount}",
            $"skill_cards={skillCount}",
            $"power_cards={powerCount}",
            $"draw_support_cards={drawTaggedCardCount}",
            $"block_support_cards={blockTaggedCardCount}",
            $"energy_support_cards={energyTaggedCardCount}",
            $"attack_pressure={attackPressure}",
            $"defense_pressure={defensePressure}",
            $"draw_support_level={drawSupportLevel}",
            $"energy_support_level={energySupportLevel}",
            $"skip_allowed={optionSet.SkipAllowed}",
            $"visible_reward_options={string.Join(", ", optionSet.Options.Select(option => option.Label))}",
        }
        .Concat(synergyHints.Select(hint => $"synergy:{hint}"))
        .Concat(antiSynergyHints.Select(hint => $"anti_synergy:{hint}"))
        .Concat(missingInformation.Select(flag => $"missing:{flag}"))
        .ToArray();

        return new RewardAssessmentFacts(
            knowledgeFingerprint,
            deckSize,
            attackCount,
            skillCount,
            powerCount,
            drawTaggedCardCount,
            blockTaggedCardCount,
            energyTaggedCardCount,
            attackPressure,
            defensePressure,
            drawSupportLevel,
            energySupportLevel,
            synergyHints.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            antiSynergyHints.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            missingInformation.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            factLines,
            knowledgeRefs.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static int CountTaggedDeckCards(
        IReadOnlyList<Sts2ModKit.Core.LiveExport.LiveExportCardSummary> deck,
        KnowledgeSlice slice,
        StaticKnowledgeCatalog? catalog,
        Func<StaticKnowledgeEntry, bool> predicate)
    {
        var knowledgeResolver = new RewardExactKnowledgeResolver();
        return deck.Count(card =>
        {
            var match = knowledgeResolver.Resolve(card, slice, catalog);
            return match is not null && predicate(match.Entry);
        });
    }

    private static bool DeckAlreadyContains(IReadOnlyList<Sts2ModKit.Core.LiveExport.LiveExportCardSummary> deck, RewardOption option)
    {
        return deck.Any(card =>
            string.Equals(card.Name, option.Label, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(option.Value) && string.Equals(card.Id, option.Value, StringComparison.OrdinalIgnoreCase)));
    }

    private static bool HasDrawTag(StaticKnowledgeEntry entry)
    {
        return RewardOptionFactExtractor.HasDrawSignal(entry.RawText);
    }

    private static bool HasBlockTag(StaticKnowledgeEntry entry)
    {
        return RewardOptionFactExtractor.HasBlockSignal(entry.RawText);
    }

    private static bool HasEnergyTag(StaticKnowledgeEntry entry)
    {
        return RewardOptionFactExtractor.HasEnergySignal(entry.RawText);
    }

    private static string DescribePressure(int count, int deckSize)
    {
        if (count == 0)
        {
            return "none";
        }

        if (deckSize <= 0)
        {
            return "unknown";
        }

        var ratio = (double)count / deckSize;
        if (ratio < 0.20)
        {
            return "low";
        }

        if (ratio > 0.45)
        {
            return "high";
        }

        return "balanced";
    }

    private static string DescribeSupport(int count)
    {
        return count switch
        {
            <= 0 => "missing",
            1 => "light",
            _ => "present",
        };
    }

    private static string BuildKnowledgeFingerprint(KnowledgeSlice slice)
    {
        var ids = slice.Entries
            .Select(entry => entry.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return ids.Length == 0 ? "knowledge:none" : $"knowledge:{string.Join("|", ids)}";
    }

}

internal static class RewardRecommendationTraceBuilder
{
    public static RewardRecommendationTrace? Build(RewardOptionSet? optionSet, RewardAssessmentFacts? assessmentFacts)
    {
        if (optionSet is null || assessmentFacts is null)
        {
            return null;
        }

        return new RewardRecommendationTrace(
            assessmentFacts.KnowledgeFingerprint,
            optionSet.Options.Select(option => option.Label).ToArray(),
            assessmentFacts.FactLines.ToArray(),
            assessmentFacts.KnowledgeRefs.ToArray(),
            assessmentFacts.MissingInformation.ToArray());
    }

}

public static class RewardAdviceResponseFinalizer
{
    public static AdviceResponse Apply(AdviceInputPack inputPack, AdviceResponse response)
    {
        if (inputPack.RewardOptionSet is null)
        {
            return response with { RewardRecommendationTrace = inputPack.RewardRecommendationTraceSeed };
        }

        var matchedLabel = inputPack.RewardOptionSet.Options
            .Select(option => option.Label)
            .FirstOrDefault(label => string.Equals(label, response.RecommendedChoiceLabel, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(response.RecommendedChoiceLabel))
        {
            return response with { RewardRecommendationTrace = inputPack.RewardRecommendationTraceSeed };
        }

        if (matchedLabel is not null)
        {
            return response with
            {
                RecommendedChoiceLabel = matchedLabel,
                RewardRecommendationTrace = inputPack.RewardRecommendationTraceSeed,
            };
        }

        return response with
        {
            RecommendedChoiceLabel = null,
            DecisionBlockers = response.DecisionBlockers
                .Append("recommended-choice-not-in-option-set")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            RewardRecommendationTrace = inputPack.RewardRecommendationTraceSeed,
        };
    }
}
