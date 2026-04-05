using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

internal sealed class RewardCompactInputBuilder
{
    private readonly RewardOptionSetBuilder _rewardOptionSetBuilder = new();
    private readonly RewardAssessmentFactsBuilder _rewardAssessmentFactsBuilder = new();

    public CompactAdvisorBuildResult Build(CompanionRunState runState, KnowledgeSlice boundedSlice)
    {
        var options = CompactAdvisorBuilderShared.BuildVisibleOptions(GetRewardSourceItems(runState), runState.Snapshot.CurrentChoices);
        var missingInformation = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var decisionBlockers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (options.Count == 0)
        {
            decisionBlockers.Add("reward-compact-input-insufficient");
            return new CompactAdvisorBuildResult(false, null, Array.Empty<string>(), decisionBlockers.ToArray(), "reward-compact-input-insufficient");
        }

        AddDuplicateLabelBlockers(options, "reward", decisionBlockers);

        var rewardOptionSet = _rewardOptionSetBuilder.Build(runState);
        var rewardAssessmentFacts = _rewardAssessmentFactsBuilder.Build(runState, boundedSlice, rewardOptionSet);
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

        var compactKnowledge = CompactAdvisorBuilderShared.FilterKnowledgeEntries(
            boundedSlice,
            options.SelectMany(option => new[] { option.Label, option.Value })
                .Append("reward"));
        var rewardFacts = new RewardCompactFacts(
            runState.NormalizedState.Reward.RewardType ?? "generic",
            options.Any(IsSkipOption),
            rewardAssessmentFacts?.FactLines ?? BuildRewardFallbackFactLines(runState, options),
            rewardAssessmentFacts?.MissingInformation ?? missingInformation.ToArray(),
            rewardAssessmentFacts?.KnowledgeRefs ?? compactKnowledge.Select(entry => entry.Id).ToArray());
        var compactInput = new RewardEventCompactAdvisorInput(
            SceneType: "reward",
            SceneStage: ResolveRewardStage(runState),
            CanonicalOwner: "reward",
            RunContext: CompactAdvisorBuilderShared.BuildRunContext(runState),
            PlayerSummary: CompactAdvisorBuilderShared.BuildPlayerSummary(runState),
            VisibleOptions: options,
            KnowledgeEntries: compactKnowledge,
            RecentEvents: CompactAdvisorBuilderShared.FilterRecentEvents(runState.RecentEvents, IsRelevantRewardEvent),
            MissingInformation: missingInformation.ToArray(),
            DecisionBlockers: decisionBlockers.ToArray(),
            RewardFacts: rewardFacts);
        return decisionBlockers.Contains("reward-compact-input-insufficient")
            ? new CompactAdvisorBuildResult(false, compactInput, compactInput.MissingInformation, compactInput.DecisionBlockers, "reward-compact-input-insufficient")
            : new CompactAdvisorBuildResult(true, compactInput, compactInput.MissingInformation, compactInput.DecisionBlockers, "supported");
    }

    private static IReadOnlyList<CompanionChoiceItem> GetRewardSourceItems(CompanionRunState runState)
    {
        return runState.NormalizedState.Reward.Entries.Count > 0
            ? runState.NormalizedState.Reward.Entries
            : runState.NormalizedState.Choices.List;
    }

    private static void AddDuplicateLabelBlockers(
        IReadOnlyList<CompactAdvisorOption> options,
        string sceneType,
        HashSet<string> decisionBlockers)
    {
        var duplicateLabels = options
            .GroupBy(option => option.Label, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        foreach (var label in duplicateLabels)
        {
            decisionBlockers.Add($"{sceneType}-duplicate-option-label:{label}");
        }

        if (duplicateLabels.Length > 0)
        {
            decisionBlockers.Add($"{sceneType}-compact-input-insufficient");
        }
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
        return CompactAdvisorBuilderShared.FirstNonBlank(
            CompactAdvisorBuilderShared.TryGetMeta(runState, "foregroundActionLane"),
            "reward-claim")!;
    }

    private static bool IsRelevantRewardEvent(LiveExportEventEnvelope envelope)
    {
        return string.Equals(envelope.Screen, "reward", StringComparison.OrdinalIgnoreCase)
               || string.Equals(envelope.Screen, "rewards", StringComparison.OrdinalIgnoreCase)
               || envelope.Kind is "reward-opened" or "reward-screen-opened" or "choice-list-presented";
    }

    private static bool IsSkipOption(CompactAdvisorOption option)
    {
        return option.Tags.Contains("skip-option", StringComparer.OrdinalIgnoreCase)
               || CompactAdvisorBuilderShared.IsSkipText(option.Label)
               || CompactAdvisorBuilderShared.IsSkipText(option.Value);
    }
}
