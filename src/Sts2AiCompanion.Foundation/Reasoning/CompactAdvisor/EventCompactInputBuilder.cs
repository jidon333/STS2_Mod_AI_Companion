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
            var effects = ExtractEventEffects(option.Label, option.Description);
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
            CompactAdvisorBuilderShared.TryGetMeta(runState, "event-id"),
            NormalizeEncounterName(runState.Snapshot.Encounter?.Name),
            InferAncientEventIdentity(runState, options));
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

    private static string? NormalizeEncounterName(string? encounterName)
    {
        if (string.IsNullOrWhiteSpace(encounterName))
        {
            return null;
        }

        return encounterName.Equals("EventRoom", StringComparison.OrdinalIgnoreCase)
            ? null
            : encounterName;
    }

    private static string? InferAncientEventIdentity(
        CompanionRunState runState,
        IReadOnlyList<CompactAdvisorOption> options)
    {
        var foregroundLane = CompactAdvisorBuilderShared.TryGetMeta(runState, "foregroundActionLane");
        var ancientDetected = string.Equals(CompactAdvisorBuilderShared.TryGetMeta(runState, "ancientEventDetected"), "true", StringComparison.OrdinalIgnoreCase);
        var isAncient = ancientDetected
                        || string.Equals(foregroundLane, "ancient-dialogue", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(foregroundLane, "ancient-option", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(foregroundLane, "ancient-completion", StringComparison.OrdinalIgnoreCase);
        if (!isAncient)
        {
            return null;
        }

        var normalizedLabels = options
            .Select(option => PromptTextSanitizer.SanitizeText(option.Label))
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Cast<string>()
            .ToArray();

        if (normalizedLabels.Any(label =>
                string.Equals(label, "납 문진", StringComparison.OrdinalIgnoreCase)
                || string.Equals(label, "니오우의 비탄", StringComparison.OrdinalIgnoreCase)
                || string.Equals(label, "은 도가니", StringComparison.OrdinalIgnoreCase)))
        {
            return "Neow";
        }

        return null;
    }

    private static List<EventCompactEffect> ExtractEventEffects(string? label, string? description)
    {
        var effects = new List<EventCompactEffect>();
        if (string.IsNullOrWhiteSpace(description))
        {
            return effects;
        }

        var sanitizedDescription = PromptTextSanitizer.Sanitize(description);
        var sanitizedLabel = PromptTextSanitizer.Sanitize(label);

        void Add(string kind, int? amount, string text)
        {
            var sanitizedText = PromptTextSanitizer.SanitizeText(text) ?? text;
            effects.Add(new EventCompactEffect(kind, amount, sanitizedText));
        }

        foreach (Match match in Regex.Matches(sanitizedDescription, @"최대 체력을\s*(\d+)\s*잃", RegexOptions.IgnoreCase))
        {
            Add("hp_loss", ParseAmount(match, 1), match.Value);
        }

        foreach (Match match in Regex.Matches(sanitizedDescription, @"체력을\s*(\d+)\s*회복", RegexOptions.IgnoreCase))
        {
            Add("hp_gain", ParseAmount(match, 1), match.Value);
        }

        foreach (Match match in Regex.Matches(sanitizedDescription, @"(\d+)\s*골드를?\s*잃", RegexOptions.IgnoreCase))
        {
            Add("gold_loss", ParseAmount(match, 1), match.Value);
        }

        foreach (Match match in Regex.Matches(sanitizedDescription, @"(\d+)\s*골드를?\s*(얻|획득)", RegexOptions.IgnoreCase))
        {
            Add("gold_gain", ParseAmount(match, 1), match.Value);
        }

        foreach (Match match in Regex.Matches(sanitizedDescription, @"무색 카드\s*(\d+)장\s*중\s*(\d+)장을?\s*선택해\s*덱에\s*추가", RegexOptions.IgnoreCase))
        {
            Add("card_gain", ParseAmount(match, 2), match.Value);
            Add("colorless_card_choice", ParseAmount(match, 1), match.Value);
        }

        foreach (Match match in Regex.Matches(sanitizedDescription, @"덱에\s*.+?\s*(\d+)장을?\s*추가", RegexOptions.IgnoreCase))
        {
            Add("card_gain", ParseAmount(match, 1), match.Value);
        }

        if (sanitizedDescription.Contains("덱에", StringComparison.OrdinalIgnoreCase)
            && sanitizedDescription.Contains("추가", StringComparison.OrdinalIgnoreCase)
            && !effects.Any(effect => string.Equals(effect.Kind, "card_gain", StringComparison.OrdinalIgnoreCase)))
        {
            Add("card_gain", null, sanitizedDescription);
        }

        if (sanitizedDescription.Contains("카드 보상", StringComparison.OrdinalIgnoreCase)
            && sanitizedDescription.Contains("강화", StringComparison.OrdinalIgnoreCase))
        {
            var amount = TryParseEmbeddedAmount(sanitizedDescription);
            if (amount is null
                && string.Equals(sanitizedLabel, "은 도가니", StringComparison.OrdinalIgnoreCase))
            {
                amount = 3;
            }

            Add("card_reward_upgrade", amount, sanitizedDescription);
        }

        if (sanitizedDescription.Contains("강화", StringComparison.OrdinalIgnoreCase)
            && !effects.Any(effect => string.Equals(effect.Kind, "card_reward_upgrade", StringComparison.OrdinalIgnoreCase)))
        {
            Add("card_upgrade", null, "카드 강화");
        }

        if (sanitizedDescription.Contains("변형", StringComparison.OrdinalIgnoreCase) || sanitizedDescription.Contains("변환", StringComparison.OrdinalIgnoreCase))
        {
            Add("card_transform", null, "카드 변형");
        }

        if (sanitizedDescription.Contains("제거", StringComparison.OrdinalIgnoreCase) || sanitizedDescription.Contains("삭제", StringComparison.OrdinalIgnoreCase))
        {
            Add("card_remove", null, "카드 제거");
        }

        if (sanitizedDescription.Contains("카드를", StringComparison.OrdinalIgnoreCase) && sanitizedDescription.Contains("얻", StringComparison.OrdinalIgnoreCase))
        {
            Add("card_gain", null, "카드 획득");
        }

        if (sanitizedDescription.Contains("보물 상자", StringComparison.OrdinalIgnoreCase)
            && sanitizedDescription.Contains("비어", StringComparison.OrdinalIgnoreCase))
        {
            Add("treasure_chest_empty", 1, sanitizedDescription);
        }

        if (sanitizedDescription.Contains("유물을", StringComparison.OrdinalIgnoreCase)
            || sanitizedDescription.Contains("유물", StringComparison.OrdinalIgnoreCase) && sanitizedDescription.Contains("얻", StringComparison.OrdinalIgnoreCase))
        {
            Add("relic_gain", null, "유물 획득");
        }

        if (sanitizedDescription.Contains("포션", StringComparison.OrdinalIgnoreCase) && sanitizedDescription.Contains("얻", StringComparison.OrdinalIgnoreCase))
        {
            Add("potion_gain", null, "포션 획득");
        }

        AddKnownAncientOptionEffects(sanitizedLabel, effects);

        return effects
            .DistinctBy(effect => $"{effect.Kind}:{effect.Amount}:{effect.Text}", StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void AddKnownAncientOptionEffects(string? sanitizedLabel, List<EventCompactEffect> effects)
    {
        if (string.IsNullOrWhiteSpace(sanitizedLabel))
        {
            return;
        }

        void AddIfMissing(string kind, int? amount, string text)
        {
            if (effects.Any(effect => string.Equals(effect.Kind, kind, StringComparison.OrdinalIgnoreCase)
                                      && effect.Amount == amount
                                      && string.Equals(effect.Text, text, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            effects.Add(new EventCompactEffect(kind, amount, text));
        }

        switch (sanitizedLabel)
        {
            case "납 문진":
                AddIfMissing("relic_gain", null, "유물 획득");
                AddIfMissing("card_gain", 1, "무색 카드 2장 중 1장을 선택해 덱에 추가");
                AddIfMissing("colorless_card_choice", 2, "무색 카드 2장 중 1장을 선택");
                break;
            case "니오우의 비탄":
                AddIfMissing("relic_gain", null, "유물 획득");
                AddIfMissing("card_gain", 1, "니오우의 격분 1장을 덱에 추가");
                break;
            case "은 도가니":
                AddIfMissing("relic_gain", null, "유물 획득");
                AddIfMissing("card_reward_upgrade", 3, "처음 3번의 카드 보상이 강화된 상태로 등장");
                AddIfMissing("treasure_chest_empty", 1, "처음으로 여는 보물 상자가 비어 있음");
                break;
        }
    }

    private static int? TryParseEmbeddedAmount(string text)
    {
        var numericMatch = Regex.Match(text, @"(\d+)\s*번", RegexOptions.IgnoreCase);
        if (numericMatch.Success)
        {
            return ParseAmount(numericMatch, 1);
        }

        return null;
    }

    private static int? ParseAmount(Match match, int groupIndex)
    {
        return int.TryParse(match.Groups[groupIndex].Value, out var parsed) ? parsed : null;
    }
}
