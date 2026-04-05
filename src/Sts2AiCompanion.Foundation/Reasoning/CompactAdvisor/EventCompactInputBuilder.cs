using System.Text.RegularExpressions;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

internal sealed class EventCompactInputBuilder
{
    public CompactAdvisorBuildResult Build(CompanionRunState runState, KnowledgeSlice boundedSlice)
    {
        var options = CompactAdvisorBuilderShared.BuildVisibleOptions(GetEventSourceItems(runState), runState.Snapshot.CurrentChoices);
        var missingInformation = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var decisionBlockers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (options.Count == 0)
        {
            decisionBlockers.Add("event-compact-input-insufficient");
            return new CompactAdvisorBuildResult(false, null, Array.Empty<string>(), decisionBlockers.ToArray(), "event-compact-input-insufficient");
        }

        AddDuplicateLabelBlockers(options, "event", decisionBlockers);

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

        var eventId = CompactAdvisorBuilderShared.FirstNonBlank(
            runState.NormalizedState.Event.EventId,
            CompactAdvisorBuilderShared.TryGetMeta(runState, "event-id"));
        var pageId = CompactAdvisorBuilderShared.FirstNonBlank(
            runState.NormalizedState.Event.PageId,
            CompactAdvisorBuilderShared.TryGetMeta(runState, "event-page-id"));
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

        var compactKnowledge = CompactAdvisorBuilderShared.FilterKnowledgeEntries(
            boundedSlice,
            options.SelectMany(option => new[] { option.Label, option.Value })
                .Append("event")
                .Append(eventId)
                .Append(pageId));
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
            RunContext: CompactAdvisorBuilderShared.BuildRunContext(runState),
            PlayerSummary: CompactAdvisorBuilderShared.BuildPlayerSummary(runState),
            VisibleOptions: options,
            KnowledgeEntries: compactKnowledge,
            RecentEvents: CompactAdvisorBuilderShared.FilterRecentEvents(runState.RecentEvents, IsRelevantEvent),
            MissingInformation: missingInformation.ToArray(),
            DecisionBlockers: decisionBlockers.ToArray(),
            EventFacts: eventFacts);
        return decisionBlockers.Contains("event-compact-input-insufficient")
            ? new CompactAdvisorBuildResult(false, compactInput, compactInput.MissingInformation, compactInput.DecisionBlockers, "event-compact-input-insufficient")
            : new CompactAdvisorBuildResult(true, compactInput, compactInput.MissingInformation, compactInput.DecisionBlockers, "supported");
    }

    private static IReadOnlyList<CompanionChoiceItem> GetEventSourceItems(CompanionRunState runState)
    {
        return runState.NormalizedState.Event.Options.Count > 0
            ? runState.NormalizedState.Event.Options
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

    private static string ResolveEventStage(CompanionRunState runState)
    {
        return CompactAdvisorBuilderShared.FirstNonBlank(
            CompactAdvisorBuilderShared.TryGetMeta(runState, "foregroundActionLane"),
            "event-option")!;
    }

    private static bool IsRelevantEvent(LiveExportEventEnvelope envelope)
    {
        return string.Equals(envelope.Screen, "event", StringComparison.OrdinalIgnoreCase)
               || envelope.Kind is "event-opened" or "event-screen-opened" or "choice-list-presented";
    }

    private static bool LooksLikeProceedOption(CompactAdvisorOption option)
    {
        return option.Kind.Contains("proceed", StringComparison.OrdinalIgnoreCase)
               || option.Label.Contains("계속", StringComparison.OrdinalIgnoreCase)
               || option.Label.Contains("진행", StringComparison.OrdinalIgnoreCase)
               || option.Label.Contains("완료", StringComparison.OrdinalIgnoreCase);
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

        if (description.Contains("유물을", StringComparison.OrdinalIgnoreCase)
            || description.Contains("유물", StringComparison.OrdinalIgnoreCase) && description.Contains("얻", StringComparison.OrdinalIgnoreCase))
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
}
