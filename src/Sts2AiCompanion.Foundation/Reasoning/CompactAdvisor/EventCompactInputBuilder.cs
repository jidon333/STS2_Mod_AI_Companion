using System.Text.RegularExpressions;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

internal sealed class EventCompactInputBuilder
{
    private sealed record EventVisibleOptionSource(
        CompactAdvisorOption Option,
        LiveExportChoiceSummary? ExportedChoice);

    public CompactAdvisorBuildResult Build(CompanionRunState runState, KnowledgeSlice boundedSlice)
    {
        var optionSources = BuildVisibleOptions(runState);
        var options = optionSources
            .Select(static source => source.Option)
            .ToArray();
        var missingInformation = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var decisionBlockers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (options.Length == 0)
        {
            decisionBlockers.Add("event-compact-input-insufficient");
            return new CompactAdvisorBuildResult(false, null, Array.Empty<string>(), decisionBlockers.ToArray(), "event-compact-input-insufficient");
        }

        AddDuplicateLabelBlockers(options, "event", decisionBlockers);
        var eventId = CompactAdvisorBuilderShared.FirstNonBlank(
            runState.NormalizedState.Event.EventId,
            CompactAdvisorBuilderShared.TryGetMeta(runState, "event-id"),
            InferEventIdentityFromExportedDetails(optionSources),
            InferEventIdentityFromBindings(optionSources),
            NormalizeEncounterName(runState.Snapshot.Encounter?.Name),
            InferAncientEventIdentity(runState, optionSources));

        var optionFacts = new List<EventCompactOptionFact>();
        foreach (var optionSource in optionSources)
        {
            var effects = ExtractEventEffects(eventId, optionSource);
            var optionMissing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var option = optionSource.Option;
            if (effects.Count == 0 && option.Enabled && !LooksLikeProceedOption(option))
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
            optionSources.SelectMany(GetEventKnowledgeSeeds)
                .Append("event")
                .Append(eventId)
                .Append(pageId)
                .Concat(GetEventKnowledgeSeeds(eventId, optionSources)));
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

    private static IReadOnlyList<EventVisibleOptionSource> BuildVisibleOptions(CompanionRunState runState)
    {
        var eventChoices = runState.Snapshot.CurrentChoices
            .Where(IsEventChoice)
            .ToArray();
        if (eventChoices.Length == 0)
        {
            return CompactAdvisorBuilderShared.BuildVisibleOptions(GetEventSourceItems(runState), runState.Snapshot.CurrentChoices)
                .Select(static option => new EventVisibleOptionSource(option, null))
                .ToArray();
        }

        var canonicalChoices = eventChoices
            .GroupBy(BuildEventChoiceGroupKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(ScoreEventChoice)
                .First())
            .ToArray();

        return canonicalChoices
            .Select(summary => new CompactAdvisorOption(
                summary.Kind,
                summary.Label,
                summary.Value,
                ResolveEventOptionDescription(summary),
                summary.Enabled ?? true,
                BuildOptionTags(summary)))
            .Zip(canonicalChoices, static (option, choice) => new EventVisibleOptionSource(option, choice))
            .ToArray();
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
               || encounterName.Equals("root", StringComparison.OrdinalIgnoreCase)
               || encounterName.Equals("game", StringComparison.OrdinalIgnoreCase)
            ? null
            : encounterName;
    }

    private static string? InferEventIdentityFromExportedDetails(IReadOnlyList<EventVisibleOptionSource> options)
    {
        foreach (var option in options)
        {
            var detail = option.ExportedChoice?.EventOptionDetail;
            var optionKey = CompactAdvisorBuilderShared.FirstNonBlank(
                detail?.OptionKey,
                detail?.OptionBindingId,
                option.ExportedChoice?.BindingId);
            if (optionKey is not null
                && optionKey.StartsWith("NEOW.", StringComparison.OrdinalIgnoreCase))
            {
                return "Neow";
            }

            if (optionKey is not null
                && optionKey.StartsWith("WOOD_CARVINGS.", StringComparison.OrdinalIgnoreCase))
            {
                return "WoodCarvings";
            }

            var resultRelicId = CompactAdvisorBuilderShared.FirstNonBlank(
                detail?.ResultRelic?.Id,
                detail?.ResultRelic?.Title);
            if (MatchesStrictRelicId(resultRelicId, "NeowsTorment")
                || MatchesStrictRelicId(resultRelicId, "SilverCrucible")
                || MatchesStrictRelicId(resultRelicId, "LeadPaperweight"))
            {
                return "Neow";
            }
        }

        return null;
    }

    private static string? InferEventIdentityFromBindings(IReadOnlyList<EventVisibleOptionSource> options)
    {
        var bindingIds = options
            .Select(source => source.ExportedChoice?.BindingId)
            .Where(bindingId => !string.IsNullOrWhiteSpace(bindingId))
            .Cast<string>()
            .ToArray();
        if (bindingIds.Length == 0)
        {
            return null;
        }

        if (bindingIds.Any(bindingId => bindingId.StartsWith("WOOD_CARVINGS.", StringComparison.OrdinalIgnoreCase)))
        {
            return "WoodCarvings";
        }

        return null;
    }

    private static string? InferAncientEventIdentity(
        CompanionRunState runState,
        IReadOnlyList<EventVisibleOptionSource> options)
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

        if (options.Any(static option =>
            option.ExportedChoice?.EventOptionDetail?.OptionKey?.StartsWith("NEOW.", StringComparison.OrdinalIgnoreCase) == true))
        {
            return "Neow";
        }

        var normalizedLabels = options
            .Select(option => PromptTextSanitizer.SanitizeText(option.Option.Label))
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

    private static string? ResolveEventOptionDescription(LiveExportChoiceSummary choice)
    {
        var detailDescription = choice.EventOptionDetail?.EvaluatedDescription;
        if (!string.IsNullOrWhiteSpace(detailDescription))
        {
            return detailDescription;
        }

        var compatibilityDescription = CanonicalizeKnownEventDescription(choice);
        if (!string.IsNullOrWhiteSpace(compatibilityDescription)
            && LooksLikeUnresolvedEventDescription(choice.Description))
        {
            return compatibilityDescription;
        }

        return CompactAdvisorBuilderShared.FirstNonBlank(
            choice.Description,
            compatibilityDescription);
    }

    private static List<EventCompactEffect> ExtractEventEffects(string? eventId, EventVisibleOptionSource optionSource)
    {
        var effects = new List<EventCompactEffect>();
        var option = optionSource.Option;
        var detail = optionSource.ExportedChoice?.EventOptionDetail;
        var optionKey = CompactAdvisorBuilderShared.FirstNonBlank(
            detail?.OptionKey,
            detail?.OptionBindingId,
            optionSource.ExportedChoice?.BindingId);
        var label = option.Label;
        var description = CompactAdvisorBuilderShared.FirstNonBlank(
            detail?.EvaluatedDescription,
            option.Description);

        void AddIfMissing(string kind, int? amount, string text)
        {
            var sanitizedText = PromptTextSanitizer.SanitizeText(text) ?? text;
            if (effects.Any(effect => string.Equals(effect.Kind, kind, StringComparison.OrdinalIgnoreCase)
                                      && effect.Amount == amount
                                      && string.Equals(effect.Text, sanitizedText, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            effects.Add(new EventCompactEffect(kind, amount, sanitizedText));
        }

        AddTypedDetailEffects(detail, AddIfMissing);
        AddDescriptionEffects(description, label, AddIfMissing);
        AddHoverTipEffects(detail, description, AddIfMissing);
        AddStrictEventFallbackEffects(eventId, optionKey, detail, AddIfMissing);
        AddCompatibilityKnownAncientOptionEffects(PromptTextSanitizer.Sanitize(label), AddIfMissing);
        AddCompatibilityKnownEventOptionEffects(eventId, PromptTextSanitizer.Sanitize(label), AddIfMissing);

        return effects;
    }

    private static void AddTypedDetailEffects(
        LiveExportEventOptionDetail? detail,
        Action<string, int?, string> addIfMissing)
    {
        if (detail is null)
        {
            return;
        }

        AddModelEffects("card", detail.ResultCard, addIfMissing);
        AddModelEffects("relic", detail.ResultRelic, addIfMissing);
        AddModelEffects("enchantment", detail.ResultEnchantment, addIfMissing);
        AddModelEffects("power", detail.ResultPower, addIfMissing);

        if (!string.IsNullOrWhiteSpace(detail.TargetFilter))
        {
            addIfMissing("target_filter", 1, detail.TargetFilter);
        }
        else if (!string.IsNullOrWhiteSpace(detail.TargetSelectorHint))
        {
            addIfMissing("target_filter", 1, detail.TargetSelectorHint);
        }
    }

    private static void AddModelEffects(
        string modelKind,
        LiveExportModelSummary? model,
        Action<string, int?, string> addIfMissing)
    {
        if (model is null)
        {
            return;
        }

        var name = CompactAdvisorBuilderShared.FirstNonBlank(model.Title, model.Id);
        if (!string.IsNullOrWhiteSpace(name))
        {
            addIfMissing($"result_{modelKind}", 1, name);
        }

        if (!string.IsNullOrWhiteSpace(model.Description))
        {
            addIfMissing($"result_{modelKind}_effect", 1, model.Description);
        }
    }

    private static void AddDescriptionEffects(
        string? description,
        string? label,
        Action<string, int?, string> addIfMissing)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return;
        }

        var sanitizedDescription = PromptTextSanitizer.Sanitize(description);
        var sanitizedLabel = PromptTextSanitizer.Sanitize(label);

        foreach (Match match in Regex.Matches(sanitizedDescription, @"최대 체력을\s*(\d+)\s*잃", RegexOptions.IgnoreCase))
        {
            addIfMissing("hp_loss", ParseAmount(match, 1), match.Value);
        }

        foreach (Match match in Regex.Matches(sanitizedDescription, @"체력을\s*(\d+)\s*회복", RegexOptions.IgnoreCase))
        {
            addIfMissing("hp_gain", ParseAmount(match, 1), match.Value);
        }

        foreach (Match match in Regex.Matches(sanitizedDescription, @"(\d+)\s*골드를?\s*잃", RegexOptions.IgnoreCase))
        {
            addIfMissing("gold_loss", ParseAmount(match, 1), match.Value);
        }

        foreach (Match match in Regex.Matches(sanitizedDescription, @"(\d+)\s*골드를?\s*(얻|획득)", RegexOptions.IgnoreCase))
        {
            addIfMissing("gold_gain", ParseAmount(match, 1), match.Value);
        }

        foreach (Match match in Regex.Matches(sanitizedDescription, @"무색 카드\s*(\d+)장\s*중\s*(\d+)장을?\s*선택해\s*덱에\s*추가", RegexOptions.IgnoreCase))
        {
            addIfMissing("card_gain", ParseAmount(match, 2), match.Value);
            addIfMissing("colorless_card_choice", ParseAmount(match, 1), match.Value);
        }

        foreach (Match match in Regex.Matches(sanitizedDescription, @"덱에\s*.+?\s*(\d+)장을?\s*추가", RegexOptions.IgnoreCase))
        {
            addIfMissing("card_gain", ParseAmount(match, 1), match.Value);
        }

        if (sanitizedDescription.Contains("덱에", StringComparison.OrdinalIgnoreCase)
            && sanitizedDescription.Contains("추가", StringComparison.OrdinalIgnoreCase))
        {
            addIfMissing("card_gain", null, sanitizedDescription);
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

            addIfMissing("card_reward_upgrade", amount, sanitizedDescription);
        }

        if (sanitizedDescription.Contains("강화", StringComparison.OrdinalIgnoreCase))
        {
            addIfMissing("card_upgrade", null, "카드 강화");
        }

        if (sanitizedDescription.Contains("변형", StringComparison.OrdinalIgnoreCase) || sanitizedDescription.Contains("변환", StringComparison.OrdinalIgnoreCase))
        {
            addIfMissing("card_transform", null, "카드 변형");
        }

        if (sanitizedDescription.Contains("인챈트", StringComparison.OrdinalIgnoreCase))
        {
            addIfMissing("card_enchant", 1, sanitizedDescription);
        }

        if (sanitizedDescription.Contains("제거", StringComparison.OrdinalIgnoreCase) || sanitizedDescription.Contains("삭제", StringComparison.OrdinalIgnoreCase))
        {
            addIfMissing("card_remove", null, "카드 제거");
        }

        if (sanitizedDescription.Contains("카드를", StringComparison.OrdinalIgnoreCase) && sanitizedDescription.Contains("얻", StringComparison.OrdinalIgnoreCase))
        {
            addIfMissing("card_gain", null, "카드 획득");
        }

        if (sanitizedDescription.Contains("보물 상자", StringComparison.OrdinalIgnoreCase)
            && sanitizedDescription.Contains("비어", StringComparison.OrdinalIgnoreCase))
        {
            addIfMissing("treasure_chest_empty", 1, sanitizedDescription);
        }

        if (sanitizedDescription.Contains("유물을", StringComparison.OrdinalIgnoreCase)
            || sanitizedDescription.Contains("유물", StringComparison.OrdinalIgnoreCase) && sanitizedDescription.Contains("얻", StringComparison.OrdinalIgnoreCase))
        {
            addIfMissing("relic_gain", null, "유물 획득");
        }

        if (sanitizedDescription.Contains("포션", StringComparison.OrdinalIgnoreCase) && sanitizedDescription.Contains("얻", StringComparison.OrdinalIgnoreCase))
        {
            addIfMissing("potion_gain", null, "포션 획득");
        }
    }

    private static void AddHoverTipEffects(
        LiveExportEventOptionDetail? detail,
        string? description,
        Action<string, int?, string> addIfMissing)
    {
        if (detail is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(detail.HoverTipTitle))
        {
            addIfMissing("hover_tip", 1, detail.HoverTipTitle);
        }

        if (!string.IsNullOrWhiteSpace(detail.HoverTipDescription))
        {
            addIfMissing("hover_tip_effect", 1, detail.HoverTipDescription);
        }
    }

    private static void AddStrictEventFallbackEffects(
        string? eventId,
        string? optionKey,
        LiveExportEventOptionDetail? detail,
        Action<string, int?, string> addIfMissing)
    {
        var resultCardId = CompactAdvisorBuilderShared.FirstNonBlank(detail?.ResultCard?.Id, detail?.ResultCard?.Title);
        var resultRelicId = CompactAdvisorBuilderShared.FirstNonBlank(detail?.ResultRelic?.Id, detail?.ResultRelic?.Title);
        var hoverTipId = CompactAdvisorBuilderShared.FirstNonBlank(detail?.HoverTipId, detail?.HoverTipTitle);

        if (MatchesStrictCardId(resultCardId, "Peck"))
        {
            addIfMissing("transform_target_card", 1, "쪼기");
            addIfMissing("result_card_effect", 1, "쪼기는 비용 1 공격 카드로, 피해 2를 3번 줍니다.");
        }

        if (MatchesStrictCardId(resultCardId, "ToricToughness"))
        {
            addIfMissing("transform_target_card", 1, "고리형 강인함");
            addIfMissing("result_card_effect", 1, "고리형 강인함은 비용 2 스킬 카드로, 방어도 5를 얻고 다음 2턴 동안 턴 시작 시 방어도 5를 얻습니다.");
        }

        if (MatchesStrictCardId(resultCardId, "NeowsFury"))
        {
            addIfMissing("card_gain", 1, "니오우의 격분 1장을 덱에 추가");
            addIfMissing("added_card_effect", 1, "니오우의 격분은 비용 1 공격 카드로, 피해 10을 주고 버린 카드 더미에서 무작위 카드 2장을 손으로 가져오며 소멸");
        }

        if (MatchesStrictRelicId(resultRelicId, "LeadPaperweight"))
        {
            addIfMissing("relic_gain", null, "유물 획득");
            addIfMissing("card_gain", 1, "무색 카드 2장 중 1장을 선택해 덱에 추가");
            addIfMissing("colorless_card_choice", 2, "무색 카드 2장 중 1장을 선택");
        }

        if (MatchesStrictRelicId(resultRelicId, "NeowsTorment"))
        {
            addIfMissing("relic_gain", null, "유물 획득");
            addIfMissing("card_gain", 1, "니오우의 격분 1장을 덱에 추가");
        }

        if (MatchesStrictRelicId(resultRelicId, "SilverCrucible"))
        {
            addIfMissing("relic_gain", null, "유물 획득");
            addIfMissing("card_reward_upgrade", 3, "처음 3번의 카드 보상이 강화된 상태로 등장");
            addIfMissing("treasure_chest_empty", 1, "처음으로 여는 보물 상자가 비어 있음");
        }

        if (MatchesStrictModelId(hoverTipId, "Slither"))
        {
            addIfMissing("result_enchantment", 1, CompactAdvisorBuilderShared.FirstNonBlank(detail?.HoverTipTitle, "미끈거림")!);
            addIfMissing(
                "result_enchantment_effect",
                1,
                CompactAdvisorBuilderShared.FirstNonBlank(
                    detail?.HoverTipDescription,
                    "미끈거림은 해당 카드를 뽑았을 때 이번 전투 동안 비용을 무작위 0~3으로 바꿉니다.")!);
        }

        if (optionKey is null)
        {
            return;
        }

        if (optionKey.EndsWith(".BIRD", StringComparison.OrdinalIgnoreCase)
            || optionKey.EndsWith(".TORUS", StringComparison.OrdinalIgnoreCase))
        {
            addIfMissing("target_filter", 1, "제거 가능한 기본 카드 1장을 선택해야 합니다.");
        }

        if (optionKey.EndsWith(".SNAKE", StringComparison.OrdinalIgnoreCase))
        {
            addIfMissing("target_filter", 1, "플레이 가능하고 X비용이 아닌 카드 1장을 선택해야 합니다.");
        }
    }

    private static bool MatchesStrictCardId(string? value, string expected)
    {
        return MatchesStrictModelId(value, expected, $"CARD.{expected}");
    }

    private static bool MatchesStrictRelicId(string? value, string expected)
    {
        return MatchesStrictModelId(value, expected, $"RELIC.{expected}");
    }

    private static bool MatchesStrictModelId(string? value, params string[] candidates)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return candidates.Any(candidate =>
            string.Equals(value, candidate, StringComparison.OrdinalIgnoreCase)
            || value.EndsWith($".{candidate}", StringComparison.OrdinalIgnoreCase));
    }

    // Temporary compatibility path until the runtime typed seam covers these events broadly.
    private static void AddCompatibilityKnownAncientOptionEffects(string? sanitizedLabel, Action<string, int?, string> addIfMissing)
    {
        if (string.IsNullOrWhiteSpace(sanitizedLabel))
        {
            return;
        }

        switch (sanitizedLabel)
        {
            case "납 문진":
                addIfMissing("relic_gain", null, "유물 획득");
                addIfMissing("card_gain", 1, "무색 카드 2장 중 1장을 선택해 덱에 추가");
                addIfMissing("colorless_card_choice", 2, "무색 카드 2장 중 1장을 선택");
                break;
            case "니오우의 비탄":
                addIfMissing("relic_gain", null, "유물 획득");
                addIfMissing("card_gain", 1, "니오우의 격분 1장을 덱에 추가");
                addIfMissing("added_card_effect", 1, "니오우의 격분은 비용 1 공격 카드로, 피해 10을 주고 버린 카드 더미에서 무작위 카드 2장을 손으로 가져오며 소멸");
                break;
            case "은 도가니":
                addIfMissing("relic_gain", null, "유물 획득");
                addIfMissing("card_reward_upgrade", 3, "처음 3번의 카드 보상이 강화된 상태로 등장");
                addIfMissing("treasure_chest_empty", 1, "처음으로 여는 보물 상자가 비어 있음");
                break;
        }
    }

    // Temporary compatibility path until the runtime typed seam covers these events broadly.
    private static void AddCompatibilityKnownEventOptionEffects(string? eventId, string? sanitizedLabel, Action<string, int?, string> addIfMissing)
    {
        if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(sanitizedLabel))
        {
            return;
        }

        if (string.Equals(eventId, "WoodCarvings", StringComparison.OrdinalIgnoreCase))
        {
            switch (sanitizedLabel)
            {
                case "새":
                    addIfMissing("card_transform", 1, "시작 카드를 1장 선택해 쪼기로 변화시킵니다.");
                    addIfMissing("transform_target_card", 1, "쪼기");
                    addIfMissing("result_card_effect", 1, "쪼기는 비용 1 공격 카드로, 피해 2를 3번 줍니다.");
                    addIfMissing("target_card_filter", 1, "제거 가능한 기본 카드 1장을 선택해야 합니다.");
                    break;
                case "뱀":
                    addIfMissing("card_enchant", 1, "카드 1장에 미끈거림을 인챈트합니다.");
                    addIfMissing("result_enchantment", 1, "미끈거림");
                    addIfMissing("result_enchantment_effect", 1, "미끈거림은 해당 카드를 뽑았을 때 이번 전투 동안 비용을 무작위 0~3으로 바꿉니다.");
                    addIfMissing("target_card_filter", 1, "플레이 가능하고 X비용이 아닌 카드 1장을 선택해야 합니다.");
                    break;
                case "고리":
                    addIfMissing("card_transform", 1, "시작 카드를 1장 선택해 고리형 강인함으로 변화시킵니다.");
                    addIfMissing("transform_target_card", 1, "고리형 강인함");
                    addIfMissing("result_card_effect", 1, "고리형 강인함은 비용 2 스킬 카드로, 방어도 5를 얻고 다음 2턴 동안 턴 시작 시 방어도 5를 얻습니다.");
                    addIfMissing("target_card_filter", 1, "제거 가능한 기본 카드 1장을 선택해야 합니다.");
                    break;
            }
        }
    }

    private static bool IsEventChoice(LiveExportChoiceSummary choice)
    {
        return string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => string.Equals(hint, "scene:event", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeUnresolvedEventDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return true;
        }

        var sanitized = PromptTextSanitizer.Sanitize(description);
        return sanitized.Contains("선택해 로 변화시킵니다.", StringComparison.OrdinalIgnoreCase)
               || sanitized.Contains("선택해 으로 변화시킵니다.", StringComparison.OrdinalIgnoreCase)
               || sanitized.Contains("카드 1장에 을 인챈트합니다.", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildEventChoiceGroupKey(LiveExportChoiceSummary choice)
    {
        return CompactAdvisorBuilderShared.FirstNonBlank(
                   choice.BindingId,
                   choice.NodeId,
                   $"{choice.Label}|{PromptTextSanitizer.SanitizeText(choice.Description)}")
               ?? choice.Label;
    }

    private static int ScoreEventChoice(LiveExportChoiceSummary choice)
    {
        var score = 0;
        if (choice.SemanticHints.Any(static hint => string.Equals(hint, "source:event-option-button", StringComparison.OrdinalIgnoreCase)))
        {
            score += 100;
        }

        if (!string.IsNullOrWhiteSpace(choice.Value))
        {
            score += 10;
        }

        if (!string.IsNullOrWhiteSpace(choice.Description))
        {
            score += 5;
        }

        if (choice.Enabled == true)
        {
            score += 2;
        }

        if (!string.IsNullOrWhiteSpace(choice.ScreenBounds))
        {
            score += 1;
        }

        return score;
    }

    private static IReadOnlyList<string> BuildOptionTags(LiveExportChoiceSummary choice)
    {
        var tags = new List<string>();
        if (CompactAdvisorBuilderShared.IsSkipText(choice.Label) || CompactAdvisorBuilderShared.IsSkipText(choice.Value))
        {
            tags.Add("skip-option");
        }

        tags.AddRange(choice.SemanticHints.Where(hint => !string.IsNullOrWhiteSpace(hint)));
        return tags
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? CanonicalizeKnownEventDescription(LiveExportChoiceSummary choice)
    {
        var bindingId = choice.BindingId;
        if (string.IsNullOrWhiteSpace(bindingId))
        {
            return choice.Description;
        }

        if (bindingId.EndsWith(".BIRD", StringComparison.OrdinalIgnoreCase))
        {
            return "시작 카드를 1장 선택해 쪼기로 변화시킵니다.";
        }

        if (bindingId.EndsWith(".SNAKE", StringComparison.OrdinalIgnoreCase))
        {
            return "카드 1장에 미끈거림을 인챈트합니다.";
        }

        if (bindingId.EndsWith(".SNAKE_LOCKED", StringComparison.OrdinalIgnoreCase))
        {
            return "미끈거림을 인챈트할 수 있는 카드가 없습니다.";
        }

        if (bindingId.EndsWith(".TORUS", StringComparison.OrdinalIgnoreCase))
        {
            return "시작 카드를 1장 선택해 고리형 강인함으로 변화시킵니다.";
        }

        return choice.Description;
    }

    private static IEnumerable<string?> GetEventKnowledgeSeeds(EventVisibleOptionSource option)
    {
        yield return option.Option.Label;
        yield return option.Option.Value;

        var detail = option.ExportedChoice?.EventOptionDetail;
        if (detail is null)
        {
            yield break;
        }

        yield return detail.OptionKey;
        yield return detail.OptionBindingId;
        yield return detail.HoverTipTitle;
        yield return detail.ResultCard?.Id;
        yield return detail.ResultCard?.Title;
        yield return detail.ResultRelic?.Id;
        yield return detail.ResultRelic?.Title;
        yield return detail.ResultEnchantment?.Id;
        yield return detail.ResultEnchantment?.Title;
        yield return detail.ResultPower?.Id;
        yield return detail.ResultPower?.Title;
    }

    private static IEnumerable<string?> GetEventKnowledgeSeeds(string? eventId, IReadOnlyList<EventVisibleOptionSource> options)
    {
        yield return eventId;

        var bindingPrefix = options
            .Select(option => CompactAdvisorBuilderShared.FirstNonBlank(
                option.ExportedChoice?.EventOptionDetail?.OptionKey,
                option.ExportedChoice?.EventOptionDetail?.OptionBindingId,
                option.ExportedChoice?.BindingId))
            .Where(bindingId => !string.IsNullOrWhiteSpace(bindingId))
            .Select(bindingId => bindingId!.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
            .FirstOrDefault(prefix => !string.IsNullOrWhiteSpace(prefix));
        yield return bindingPrefix;

        if (string.Equals(eventId, "WoodCarvings", StringComparison.OrdinalIgnoreCase)
            || string.Equals(bindingPrefix, "WOOD_CARVINGS", StringComparison.OrdinalIgnoreCase))
        {
            yield return "나무 조각";
            yield return "WOOD_CARVINGS";
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
