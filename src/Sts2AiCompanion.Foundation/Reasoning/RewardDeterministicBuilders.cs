using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Knowledge;

namespace Sts2AiCompanion.Foundation.Reasoning;

public sealed class RewardOptionSetBuilder
{
    public RewardOptionSet? Build(CompanionRunState runState)
    {
        if (!IsCardRewardScene(runState))
        {
            return null;
        }

        var sourceEntries = runState.NormalizedState.Reward.Entries.Count > 0
            ? runState.NormalizedState.Reward.Entries
            : runState.NormalizedState.Choices.List;
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

    private static bool IsRewardScene(string? sceneType)
    {
        return string.Equals(sceneType, "reward", StringComparison.OrdinalIgnoreCase)
               || string.Equals(sceneType, "rewards", StringComparison.OrdinalIgnoreCase);
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
    public RewardAssessmentFacts? Build(
        CompanionRunState runState,
        KnowledgeSlice slice,
        RewardOptionSet? optionSet)
    {
        if (optionSet is null || !RewardOptionSetBuilder.IsCardRewardScene(runState))
        {
            return null;
        }

        var deck = runState.Snapshot.Deck;
        var attackCount = deck.Count(card => string.Equals(card.Type, "attack", StringComparison.OrdinalIgnoreCase));
        var skillCount = deck.Count(card => string.Equals(card.Type, "skill", StringComparison.OrdinalIgnoreCase));
        var powerCount = deck.Count(card => string.Equals(card.Type, "power", StringComparison.OrdinalIgnoreCase));
        var drawTaggedCardCount = CountTaggedDeckCards(deck, slice.Entries, HasDrawTag);
        var blockTaggedCardCount = CountTaggedDeckCards(deck, slice.Entries, HasBlockTag);
        var energyTaggedCardCount = CountTaggedDeckCards(deck, slice.Entries, HasEnergyTag);
        var synergyHints = new List<string>();
        var antiSynergyHints = new List<string>();
        var missingInformation = new List<string>();
        var knowledgeRefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var option in optionSet.Options.Where(option => !option.IsSkipOption))
        {
            var match = FindKnowledgeMatch(option, slice.Entries);
            if (match is null)
            {
                missingInformation.Add($"reward-option-knowledge-missing:{option.Label}");
                continue;
            }

            knowledgeRefs.Add(match.Id);

            if (HasDrawTag(match) && drawTaggedCardCount == 0)
            {
                synergyHints.Add($"{option.Label}: adds draw support");
            }

            if (HasBlockTag(match) && blockTaggedCardCount == 0)
            {
                synergyHints.Add($"{option.Label}: adds block support");
            }

            if (HasEnergyTag(match) && energyTaggedCardCount == 0)
            {
                synergyHints.Add($"{option.Label}: adds energy support");
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
        IReadOnlyList<StaticKnowledgeEntry> knowledgeEntries,
        Func<StaticKnowledgeEntry, bool> predicate)
    {
        return deck.Count(card =>
        {
            var match = FindKnowledgeMatch(card, knowledgeEntries);
            return match is not null && predicate(match);
        });
    }

    private static StaticKnowledgeEntry? FindKnowledgeMatch(RewardOption option, IReadOnlyList<StaticKnowledgeEntry> knowledgeEntries)
    {
        return knowledgeEntries.FirstOrDefault(entry => Matches(option.Label, option.Value, entry));
    }

    private static StaticKnowledgeEntry? FindKnowledgeMatch(Sts2ModKit.Core.LiveExport.LiveExportCardSummary card, IReadOnlyList<StaticKnowledgeEntry> knowledgeEntries)
    {
        return knowledgeEntries.FirstOrDefault(entry => Matches(card.Name, card.Id, entry));
    }

    private static bool Matches(string? label, string? value, StaticKnowledgeEntry entry)
    {
        return MatchesSeed(label, entry) || MatchesSeed(value, entry);
    }

    private static bool MatchesSeed(string? seed, StaticKnowledgeEntry entry)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            return false;
        }

        var normalizedSeed = Normalize(seed);
        return string.Equals(entry.Id, normalizedSeed, StringComparison.OrdinalIgnoreCase)
               || string.Equals(entry.Name, seed, StringComparison.OrdinalIgnoreCase)
               || entry.Name.Contains(seed, StringComparison.OrdinalIgnoreCase);
    }

    private static bool DeckAlreadyContains(IReadOnlyList<Sts2ModKit.Core.LiveExport.LiveExportCardSummary> deck, RewardOption option)
    {
        return deck.Any(card =>
            string.Equals(card.Name, option.Label, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(option.Value) && string.Equals(card.Id, option.Value, StringComparison.OrdinalIgnoreCase)));
    }

    private static bool HasDrawTag(StaticKnowledgeEntry entry)
    {
        return ContainsAny(entry.RawText, "draw", "카드를 뽑", "draw a card");
    }

    private static bool HasBlockTag(StaticKnowledgeEntry entry)
    {
        return ContainsAny(entry.RawText, "block", "방어도");
    }

    private static bool HasEnergyTag(StaticKnowledgeEntry entry)
    {
        return ContainsAny(entry.RawText, "energy", "에너지");
    }

    private static bool ContainsAny(string? rawText, params string[] markers)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return false;
        }

        return markers.Any(marker => rawText.Contains(marker, StringComparison.OrdinalIgnoreCase));
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

    private static string Normalize(string value)
    {
        return new string(value
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray())
            .Trim('-');
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
        var trace = inputPack.RewardRecommendationTraceSeed;
        var blockers = new List<string>(response.DecisionBlockers);
        if (string.IsNullOrWhiteSpace(response.RecommendedChoiceLabel))
        {
            blockers.Add("reward-recommendation-missing");
            return response with
            {
                DecisionBlockers = blockers.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                ReasoningBullets = BuildReasoningBullets(inputPack, null, response.ReasoningBullets),
                MissingInformation = MergeResponseList(response.MissingInformation, trace?.MissingInformation),
                KnowledgeRefs = MergeResponseList(response.KnowledgeRefs, trace?.InputKnowledgeRefs),
                RewardRecommendationTrace = trace,
            };
        }

        if (matchedLabel is not null)
        {
            return response with
            {
                RecommendedChoiceLabel = matchedLabel,
                ReasoningBullets = BuildReasoningBullets(inputPack, matchedLabel, response.ReasoningBullets),
                MissingInformation = MergeResponseList(response.MissingInformation, trace?.MissingInformation),
                KnowledgeRefs = MergeResponseList(response.KnowledgeRefs, trace?.InputKnowledgeRefs),
                RewardRecommendationTrace = trace,
            };
        }

        blockers.Add("recommended-choice-not-in-option-set");
        blockers.Add("reward-recommendation-missing");
        return response with
        {
            RecommendedChoiceLabel = null,
            DecisionBlockers = blockers.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            ReasoningBullets = BuildReasoningBullets(inputPack, null, response.ReasoningBullets),
            MissingInformation = MergeResponseList(response.MissingInformation, trace?.MissingInformation),
            KnowledgeRefs = MergeResponseList(response.KnowledgeRefs, trace?.InputKnowledgeRefs),
            RewardRecommendationTrace = trace,
        };
    }

    private static IReadOnlyList<string> BuildReasoningBullets(
        AdviceInputPack inputPack,
        string? recommendedLabel,
        IReadOnlyList<string> existingBullets)
    {
        var bullets = existingBullets
            .Where(bullet => !string.IsNullOrWhiteSpace(bullet))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var optionSet = inputPack.RewardOptionSet;
        var facts = inputPack.RewardAssessmentFacts;
        if (optionSet is null)
        {
            return bullets;
        }

        if (bullets.Count < 2)
        {
            bullets.Add($"현재 보이는 card reward 옵션: {string.Join(", ", optionSet.Options.Select(option => option.Label))}");
        }

        if (!string.IsNullOrWhiteSpace(recommendedLabel) && bullets.Count < 3)
        {
            bullets.Add($"추천 라벨 '{recommendedLabel}'은 현재 visible option set 안에 있습니다.");
        }

        if (facts is not null)
        {
            foreach (var hint in facts.SynergyHints.Where(hint => MatchesLabel(hint, recommendedLabel)))
            {
                if (bullets.Count >= 5) break;
                bullets.Add($"deterministic 사실: {hint}");
            }

            foreach (var hint in facts.AntiSynergyHints.Where(hint => MatchesLabel(hint, recommendedLabel)))
            {
                if (bullets.Count >= 5) break;
                bullets.Add($"deterministic 사실: {hint}");
            }

            if (bullets.Count < 5)
            {
                bullets.Add($"현재 덱 facts: attack_pressure={facts.AttackPressure}, defense_pressure={facts.DefensePressure}, draw_support={facts.DrawSupportLevel}, energy_support={facts.EnergySupportLevel}");
            }
        }

        return bullets
            .Where(bullet => !string.IsNullOrWhiteSpace(bullet))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToArray();
    }

    private static IReadOnlyList<string> MergeResponseList(IReadOnlyList<string> primary, IReadOnlyList<string>? secondary)
    {
        return primary
            .Concat(secondary ?? Array.Empty<string>())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool MatchesLabel(string hint, string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && hint.StartsWith(label + ":", StringComparison.OrdinalIgnoreCase);
    }
}
