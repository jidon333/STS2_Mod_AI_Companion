using System.Text;
using System.Text.Json;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;
using Sts2ModKit.Core.Configuration;

namespace Sts2AiCompanion.Foundation.Reasoning;

public sealed class AdvicePromptBuilder
{
    private readonly ScaffoldConfiguration _configuration;
    private readonly RewardOptionSetBuilder _rewardOptionSetBuilder = new();
    private readonly RewardAssessmentFactsBuilder _rewardAssessmentFactsBuilder = new();

    public AdvicePromptBuilder(ScaffoldConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AdviceInputPack BuildInputPack(
        CompanionRunState runState,
        AdviceTrigger trigger,
        KnowledgeSlice slice,
        RewardEventCompactAdvisorInput? compactInput = null)
    {
        var rewardOptionSet = _rewardOptionSetBuilder.Build(runState);
        var rewardAssessmentFacts = _rewardAssessmentFactsBuilder.Build(runState, slice, rewardOptionSet);
        var rewardRecommendationTrace = RewardRecommendationTraceBuilder.Build(rewardOptionSet, rewardAssessmentFacts);

        return new AdviceInputPack(
            runState.Snapshot.RunId,
            trigger.Kind,
            DateTimeOffset.UtcNow,
            trigger.Manual,
            runState.Snapshot.CurrentScreen,
            runState.SummaryText,
            runState.Snapshot,
            runState.RecentEvents.TakeLast(_configuration.Assistant.RecentEventsCount).ToArray(),
            slice.Entries,
            slice.Reasons,
            "당신은 Slay the Spire 2 전략 조언 어시스턴트입니다. "
            + "게임을 대신 플레이하지 말고, 입력에 없는 정보는 추정하지 말고, "
            + "현재 상태와 최근 이벤트, 관련 지식 조각만 근거로 판단하세요.",
            runState.NormalizedState,
            rewardOptionSet,
            rewardAssessmentFacts,
            rewardRecommendationTrace,
            compactInput);
    }

    public string FormatPrompt(AdviceInputPack inputPack)
    {
        if (inputPack.CompactInput is not null)
        {
            return FormatCompactPrompt(inputPack);
        }

        var builder = new StringBuilder();
        builder.AppendLine("당신은 Slay the Spire 2 조언 어시스턴트입니다.");
        builder.AppendLine("- 게임을 대신 플레이하지 마세요.");
        builder.AppendLine("- 현재 상태, 현재 선택지, 최근 이벤트, 관련 지식 조각만 근거로 사용하세요.");
        builder.AppendLine("- 정보가 부족하면 추정하지 말고 무엇이 부족한지 명시하세요.");
        builder.AppendLine("- 카드/유물/이벤트 설명이 부족하면 그 사실을 그대로 적으세요.");
        builder.AppendLine("- 반드시 한국어로 답하세요.");
        builder.AppendLine("- 반드시 JSON 스키마만 반환하세요.");
        builder.AppendLine();
        builder.AppendLine($"trigger: {inputPack.TriggerKind}");
        builder.AppendLine($"manual: {inputPack.Manual}");
        builder.AppendLine($"run_id: {inputPack.RunId}");
        builder.AppendLine($"screen: {inputPack.CurrentScreen}");
        if (inputPack.NormalizedState is not null)
        {
            builder.AppendLine($"normalized_scene: {inputPack.NormalizedState.Scene.SceneType}");
            builder.AppendLine($"normalized_visible_scene: {inputPack.NormalizedState.Scene.VisibleSceneType}");
            builder.AppendLine($"normalized_flow_scene: {inputPack.NormalizedState.Scene.FlowSceneType}");
        }
        builder.AppendLine();
        builder.AppendLine("current_state_summary:");
        builder.AppendLine(SanitizePromptText(inputPack.SummaryText));
        builder.AppendLine();
        builder.AppendLine("recent_events:");
        foreach (var envelope in inputPack.RecentEvents)
        {
            builder.AppendLine($"- {envelope.Ts:O} {envelope.Kind} screen={envelope.Screen} act={envelope.Act?.ToString() ?? "?"} floor={envelope.Floor?.ToString() ?? "?"}");
            if (envelope.Payload.Count > 0)
            {
                builder.AppendLine($"  payload: {SanitizePromptText(JsonSerializer.Serialize(envelope.Payload))}");
            }
        }

        if (inputPack.RecentEvents.Count == 0)
        {
            builder.AppendLine("- none");
        }

        builder.AppendLine();
        builder.AppendLine("knowledge_slice:");
        foreach (var entry in inputPack.KnowledgeEntries)
        {
            var tags = entry.Tags.Count == 0 ? "none" : string.Join(", ", entry.Tags);
            builder.AppendLine($"- {entry.Name} [{entry.Id}] source={entry.Source} observed={entry.Observed} tags={tags}");
            if (!string.IsNullOrWhiteSpace(entry.RawText))
            {
                builder.AppendLine($"  raw: {SanitizePromptText(entry.RawText)}");
            }

            foreach (var option in entry.Options.Take(5))
            {
                builder.AppendLine($"  option: {SanitizePromptText(option.Label)} :: {SanitizePromptText(option.Description) ?? "추가 정보 없음"}");
            }
        }

        if (inputPack.KnowledgeEntries.Count == 0)
        {
            builder.AppendLine("- none");
        }

        if (inputPack.RewardOptionSet is not null)
        {
            builder.AppendLine();
            builder.AppendLine("reward_option_set:");
            builder.AppendLine($"- scene: {inputPack.RewardOptionSet.SceneType}");
            builder.AppendLine($"- skip_allowed: {inputPack.RewardOptionSet.SkipAllowed}");
            builder.AppendLine($"- summary: {inputPack.RewardOptionSet.SummaryText}");
            foreach (var option in inputPack.RewardOptionSet.Options)
            {
                var skipMarker = option.IsSkipOption ? " skip" : string.Empty;
                builder.AppendLine($"- [{option.Ordinal}] {option.Label} kind={option.Kind}{skipMarker}");
                if (!string.IsNullOrWhiteSpace(option.Value))
                {
                    builder.AppendLine($"  value: {option.Value}");
                }

                if (!string.IsNullOrWhiteSpace(option.Description))
                {
                    builder.AppendLine($"  description: {SanitizePromptText(option.Description)}");
                }
            }
        }

        if (inputPack.RewardAssessmentFacts is not null)
        {
            builder.AppendLine();
            builder.AppendLine("reward_assessment_facts:");
            builder.AppendLine($"- knowledge_fingerprint: {inputPack.RewardAssessmentFacts.KnowledgeFingerprint}");
            foreach (var fact in inputPack.RewardAssessmentFacts.FactLines)
            {
                builder.AppendLine($"- {fact}");
            }
        }

        if (inputPack.RewardRecommendationTraceSeed is not null)
        {
            builder.AppendLine();
            builder.AppendLine("reward_trace_seed:");
            builder.AppendLine($"- knowledge_fingerprint: {inputPack.RewardRecommendationTraceSeed.KnowledgeFingerprint}");
            builder.AppendLine($"- candidate_labels: {string.Join(", ", inputPack.RewardRecommendationTraceSeed.CandidateLabels)}");
            builder.AppendLine($"- deterministic_knowledge_refs: {string.Join(", ", inputPack.RewardRecommendationTraceSeed.InputKnowledgeRefs)}");
            foreach (var missing in inputPack.RewardRecommendationTraceSeed.MissingInformation)
            {
                builder.AppendLine($"- missing: {missing}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("response_instructions:");
        if (inputPack.RewardOptionSet is not null)
        {
            builder.AppendLine("- reward scene에서는 recommendedChoiceLabel을 reward_option_set에 보이는 label과 정확히 일치시키세요.");
            builder.AppendLine("- reward scene reasoningBullets에는 reward_assessment_facts 또는 knowledge_slice에서 직접 확인 가능한 사실을 2개 이상 포함하세요.");
            builder.AppendLine("- reward scene에서 추천 근거가 부족하면 missingInformation과 decisionBlockers를 비우지 말고 직접 채우세요.");
        }
        builder.AppendLine("- headline: 현재 상황의 핵심 판단을 한 줄로 요약");
        builder.AppendLine("- summary: 2~4문장으로 현재 판단과 다음 확인 사항 설명");
        builder.AppendLine("- recommendedAction: 지금 가장 합리적인 다음 행동");
        builder.AppendLine("- recommendedChoiceLabel: 현재 선택지 중 추천이 있으면 정확한 이름, 없으면 null");
        builder.AppendLine("- reasoningBullets: 2~5개의 근거");
        builder.AppendLine("- riskNotes: 0~5개의 리스크 또는 경고");
        builder.AppendLine("- missingInformation: 판단에 필요하지만 비어 있는 정보 목록");
        builder.AppendLine("- decisionBlockers: 현재 확정 추천을 막는 직접 차단 요인 목록");
        builder.AppendLine("- confidence: 0.0에서 1.0 사이 숫자");
        builder.AppendLine("- knowledgeRefs: 근거로 사용한 지식 항목 id 또는 이름");
        builder.AppendLine("- 정보가 부족해도 summary만 얼버무리지 말고 missingInformation과 decisionBlockers를 반드시 채우세요.");
        return builder.ToString().TrimEnd();
    }

    private static string FormatCompactPrompt(AdviceInputPack inputPack)
    {
        var compact = inputPack.CompactInput
            ?? throw new InvalidOperationException("CompactInput is required for compact prompt formatting.");
        var sceneType = CompactAdvisorScenePolicy.NormalizeSceneType(compact.SceneType);
        var builder = new StringBuilder();
        builder.AppendLine("당신은 Slay the Spire 2 조언 어시스턴트입니다.");
        builder.AppendLine("- 현재 compact input만 사용하세요.");
        builder.AppendLine("- compact input에 없는 정보는 추정하지 마세요.");
        builder.AppendLine("- 추천이 가능해도 recommendedChoiceLabel은 visible_options에 있는 label과 정확히 일치해야 합니다.");
        builder.AppendLine("- 불충분하면 recommendedChoiceLabel=null 로 두고 missingInformation/decisionBlockers를 채우세요.");
        if (compact.CombatFacts?.PreviewOnly == true)
        {
            builder.AppendLine("- combat preview에서는 recommendedChoiceLabel을 항상 null 로 두고 현재 facts와 missing info만 요약하세요.");
        }

        builder.AppendLine("- 반드시 한국어 JSON 스키마만 반환하세요.");
        builder.AppendLine();
        builder.AppendLine($"trigger: {inputPack.TriggerKind}");
        builder.AppendLine($"manual: {inputPack.Manual}");
        builder.AppendLine($"run_id: {inputPack.RunId}");
        builder.AppendLine("scene_identity:");
        builder.AppendLine($"- scene_type: {compact.SceneType}");
        builder.AppendLine($"- scene_stage: {compact.SceneStage}");
        builder.AppendLine($"- canonical_owner: {compact.CanonicalOwner}");
        builder.AppendLine();
        builder.AppendLine("run_context:");
        if (compact.RunContext.Act is not null)
        {
            builder.AppendLine($"- act: {compact.RunContext.Act}");
        }

        if (compact.RunContext.Floor is not null)
        {
            builder.AppendLine($"- floor: {compact.RunContext.Floor}");
        }

        builder.AppendLine($"- hp: {(compact.RunContext.CurrentHp?.ToString() ?? "?")}/{(compact.RunContext.MaxHp?.ToString() ?? "?")}");
        builder.AppendLine($"- gold: {compact.RunContext.Gold?.ToString() ?? "?"}");
        builder.AppendLine($"- relic_count: {compact.RunContext.RelicCount}");
        builder.AppendLine($"- potion_count: {compact.RunContext.PotionCount}");
        builder.AppendLine($"- deck_count: {compact.RunContext.DeckCount}");
        builder.AppendLine();
        builder.AppendLine("player_summary:");
        builder.AppendLine($"- deck: {compact.PlayerSummary.DeckSummary}");
        builder.AppendLine($"- key_relics: {(compact.PlayerSummary.KeyRelics.Count == 0 ? "none" : string.Join(", ", compact.PlayerSummary.KeyRelics))}");
        builder.AppendLine($"- key_potions: {(compact.PlayerSummary.KeyPotions.Count == 0 ? "none" : string.Join(", ", compact.PlayerSummary.KeyPotions))}");
        builder.AppendLine();
        builder.AppendLine("visible_options:");
        foreach (var option in compact.VisibleOptions)
        {
            builder.AppendLine($"- label: {option.Label}");
            builder.AppendLine($"  enabled: {option.Enabled}");
            builder.AppendLine($"  kind: {option.Kind}");
            if (!string.IsNullOrWhiteSpace(option.Value))
            {
                builder.AppendLine($"  value: {option.Value}");
            }

            if (!string.IsNullOrWhiteSpace(option.Description))
            {
                builder.AppendLine($"  description: {SanitizePromptText(option.Description)}");
            }
        }

        if (compact.VisibleOptions.Count == 0)
        {
            builder.AppendLine("- none");
        }

        if (compact.RewardFacts is not null)
        {
            builder.AppendLine();
            builder.AppendLine("reward_facts:");
            builder.AppendLine($"- reward_type: {compact.RewardFacts.RewardType ?? "unknown"}");
            builder.AppendLine($"- skip_allowed: {compact.RewardFacts.SkipAllowed}");
            foreach (var fact in compact.RewardFacts.FactLines)
            {
                builder.AppendLine($"- {fact}");
            }
        }

        if (compact.EventFacts is not null)
        {
            builder.AppendLine();
            builder.AppendLine("event_facts:");
            builder.AppendLine($"- event_id: {compact.EventFacts.EventId ?? "unknown"}");
            if (string.Equals(compact.EventFacts.EventId, "Neow", StringComparison.OrdinalIgnoreCase))
            {
                builder.AppendLine("- opening_event: true");
            }

            builder.AppendLine($"- event_identity_missing: {compact.EventFacts.EventIdentityMissing}");
            builder.AppendLine($"- reward_child_active: {compact.EventFacts.RewardChildActive}");
            builder.AppendLine($"- proceed_visible: {compact.EventFacts.ProceedVisible}");
            foreach (var optionFact in compact.EventFacts.OptionFacts)
            {
                builder.AppendLine($"- option: {optionFact.Label} enabled={optionFact.Enabled} value={optionFact.Value ?? "null"}");
                foreach (var effect in optionFact.Effects)
                {
                    builder.AppendLine($"  effect: {effect.Kind} amount={effect.Amount?.ToString() ?? "?"} text={effect.Text}");
                }

                foreach (var missing in optionFact.MissingInformation)
                {
                    builder.AppendLine($"  missing: {missing}");
                }
            }
        }

        if (compact.ShopFacts is not null)
        {
            builder.AppendLine();
            builder.AppendLine("shop_facts:");
            builder.AppendLine($"- inventory_open: {compact.ShopFacts.InventoryOpen}");
            builder.AppendLine($"- item_count: {compact.ShopFacts.ItemCount}");
            builder.AppendLine($"- service_count: {compact.ShopFacts.ServiceCount}");
            builder.AppendLine($"- affordable_option_count: {compact.ShopFacts.AffordableOptionCount}");
            builder.AppendLine($"- card_removal_visible: {compact.ShopFacts.CardRemovalVisible}");
            builder.AppendLine($"- card_removal_available: {compact.ShopFacts.CardRemovalAvailable}");
            builder.AppendLine($"- prices_known: {compact.ShopFacts.PricesKnown}");
            foreach (var missing in compact.ShopFacts.MissingInformation)
            {
                builder.AppendLine($"- missing: {missing}");
            }
        }

        if (compact.CombatFacts is not null)
        {
            builder.AppendLine();
            builder.AppendLine("combat_preview_facts:");
            builder.AppendLine($"- preview_only: {compact.CombatFacts.PreviewOnly}");
            builder.AppendLine($"- hp: {(compact.CombatFacts.CurrentHp?.ToString() ?? "?")}/{(compact.CombatFacts.MaxHp?.ToString() ?? "?")}");
            builder.AppendLine($"- energy: {compact.CombatFacts.Energy?.ToString() ?? "?"}");
            builder.AppendLine($"- turn_number: {compact.CombatFacts.TurnNumber?.ToString() ?? "?"}");
            builder.AppendLine($"- round_number: {compact.CombatFacts.RoundNumber?.ToString() ?? "?"}");
            builder.AppendLine($"- hand_count: {compact.CombatFacts.HandCount?.ToString() ?? "?"}");
            builder.AppendLine($"- hand_summary: {compact.CombatFacts.HandSummary ?? "unknown"}");
            builder.AppendLine($"- draw_pile_count: {compact.CombatFacts.DrawPileCount?.ToString() ?? "?"}");
            builder.AppendLine($"- discard_pile_count: {compact.CombatFacts.DiscardPileCount?.ToString() ?? "?"}");
            builder.AppendLine($"- exhaust_pile_count: {compact.CombatFacts.ExhaustPileCount?.ToString() ?? "?"}");
            builder.AppendLine($"- play_pile_count: {compact.CombatFacts.PlayPileCount?.ToString() ?? "?"}");
            builder.AppendLine($"- targetable_enemy_count: {compact.CombatFacts.TargetableEnemyCount?.ToString() ?? "?"}");
            builder.AppendLine($"- hittable_enemy_count: {compact.CombatFacts.HittableEnemyCount?.ToString() ?? "?"}");
            builder.AppendLine($"- targeting_in_progress: {compact.CombatFacts.TargetingInProgress}");
            builder.AppendLine($"- target_summary: {compact.CombatFacts.TargetSummary ?? "unknown"}");
            builder.AppendLine($"- enemy_intent_summary: {compact.CombatFacts.EnemyIntentSummary ?? "unknown"}");
            foreach (var missing in compact.CombatFacts.MissingInformation)
            {
                builder.AppendLine($"- missing: {missing}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("recent_events:");
        foreach (var recentEvent in compact.RecentEvents)
        {
            builder.AppendLine($"- {recentEvent.Kind} screen={recentEvent.Screen} act={recentEvent.Act?.ToString() ?? "?"} floor={recentEvent.Floor?.ToString() ?? "?"}");
            if (!string.IsNullOrWhiteSpace(recentEvent.Summary))
            {
                builder.AppendLine($"  summary: {SanitizePromptText(recentEvent.Summary)}");
            }
        }

        if (compact.RecentEvents.Count == 0)
        {
            builder.AppendLine("- none");
        }

        builder.AppendLine();
        builder.AppendLine("knowledge_slice:");
        foreach (var knowledge in compact.KnowledgeEntries)
        {
            builder.AppendLine($"- {knowledge.Name} [{knowledge.Id}]");
            if (!string.IsNullOrWhiteSpace(knowledge.Summary))
            {
                builder.AppendLine($"  summary: {SanitizePromptText(knowledge.Summary)}");
            }
        }

        if (compact.KnowledgeEntries.Count == 0)
        {
            builder.AppendLine("- none");
        }

        builder.AppendLine();
        builder.AppendLine("missing_information:");
        foreach (var missing in compact.MissingInformation.DefaultIfEmpty("none"))
        {
            builder.AppendLine($"- {missing}");
        }

        builder.AppendLine();
        builder.AppendLine("decision_blockers:");
        foreach (var blocker in compact.DecisionBlockers.DefaultIfEmpty("none"))
        {
            builder.AppendLine($"- {blocker}");
        }

        builder.AppendLine();
        builder.AppendLine("response_instructions:");
        builder.AppendLine("- visible_options 안의 label 하나만 recommendedChoiceLabel 로 선택하거나, 확정 불가면 null 로 두세요.");
        switch (sceneType)
        {
            case "reward":
                builder.AppendLine("- reward facts와 visible_options에서 직접 확인 가능한 근거만 reasoningBullets에 쓰세요.");
                break;
            case "event":
                builder.AppendLine("- event facts와 visible_options의 명시적 비용/보상만 근거로 쓰세요.");
                if (compact.EventFacts?.OptionFacts.Any(static option =>
                        option.Effects.Any(static effect => effect.Kind is "target_filter" or "target_card_filter")) == true
                    && compact.DecisionBlockers.Count == 0)
                {
                    builder.AppendLine("- event_facts에 target_filter 또는 target_card_filter가 있어도, 그 값이 이미 명시적 후속 선택 조건이라면 그 자체만으로 추천을 보류하지 마세요.");
                    builder.AppendLine("- player_summary.deck에 기본 카드 구성이나 현재 덱 요약이 보이면, exact target 카드가 아직 선택되지 않았더라도 옵션의 broad tradeoff 비교는 가능합니다.");
                    builder.AppendLine("- 공격/방어 방향 비교는 compact input의 덱 요약과 명시적 effect를 기준으로 하세요. 별도의 추가 우선순위 필드가 없다는 이유만으로 decisionBlockers를 만들지 마세요.");
                }
                if (string.Equals(compact.EventFacts?.EventId, "Neow", StringComparison.OrdinalIgnoreCase))
                {
                    builder.AppendLine("- Neow는 런 시작 이벤트입니다. act/floor/path 정보가 비어 있어도 그 자체를 missingInformation이나 decisionBlockers로 올리지 마세요.");
                    builder.AppendLine("- `니오우의 비탄`의 added_card_effect와 `은 도가니`의 고정 장단점은 이미 확정된 사실로 취급하세요.");
                }
                break;
            case "shop":
                builder.AppendLine("- affordability, option kind, visible descriptions, player resources만 근거로 쓰세요.");
                builder.AppendLine("- 장기 synergy, boss pressure, 보이지 않는 가격은 추정하지 마세요.");
                break;
            case "combat":
                builder.AppendLine("- combat preview에서는 추천 라벨을 만들지 말고 현재 facts와 missing info만 요약하세요.");
                break;
            default:
                builder.AppendLine("- scene-specific facts와 visible_options에서 직접 확인 가능한 근거만 reasoningBullets에 쓰세요.");
                break;
        }

        builder.AppendLine("- guessed effect, guessed option, guessed label 금지.");
        builder.AppendLine("- missingInformation과 decisionBlockers를 비우지 말고 compact input의 공백을 그대로 반영하세요.");
        return builder.ToString().TrimEnd();
    }

    private static string? SanitizePromptText(string? text)
    {
        return PromptTextSanitizer.SanitizeText(text) ?? text;
    }

    public string FormatAdviceMarkdown(AdviceResponse response)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {response.Headline}");
        builder.AppendLine();
        builder.AppendLine(response.Summary);
        builder.AppendLine();
        builder.AppendLine($"- trigger: {response.TriggerKind}");
        builder.AppendLine($"- run_id: {response.RunId}");
        builder.AppendLine($"- recommended_action: {response.RecommendedAction}");
        builder.AppendLine($"- recommended_choice: {response.RecommendedChoiceLabel ?? "없음"}");
        builder.AppendLine($"- confidence: {(response.Confidence?.ToString("0.00") ?? "미상")}");
        builder.AppendLine();
        builder.AppendLine("## 근거");
        foreach (var bullet in response.ReasoningBullets.DefaultIfEmpty("없음"))
        {
            builder.AppendLine($"- {bullet}");
        }

        builder.AppendLine();
        builder.AppendLine("## 리스크");
        foreach (var note in response.RiskNotes.DefaultIfEmpty("없음"))
        {
            builder.AppendLine($"- {note}");
        }

        builder.AppendLine();
        builder.AppendLine("## 부족한 정보");
        foreach (var item in response.MissingInformation.DefaultIfEmpty("없음"))
        {
            builder.AppendLine($"- {item}");
        }

        builder.AppendLine();
        builder.AppendLine("## 판단 차단 요인");
        foreach (var item in response.DecisionBlockers.DefaultIfEmpty("없음"))
        {
            builder.AppendLine($"- {item}");
        }

        builder.AppendLine();
        builder.AppendLine("## 참고 지식");
        foreach (var reference in response.KnowledgeRefs.DefaultIfEmpty("없음"))
        {
            builder.AppendLine($"- {reference}");
        }

        if (response.RewardRecommendationTrace is not null)
        {
            builder.AppendLine();
            builder.AppendLine("## Reward Deterministic Trace");
            builder.AppendLine($"- knowledge_fingerprint: {response.RewardRecommendationTrace.KnowledgeFingerprint}");
            builder.AppendLine($"- candidate_labels: {string.Join(", ", response.RewardRecommendationTrace.CandidateLabels)}");
            builder.AppendLine("### Deterministic Facts");
            foreach (var fact in response.RewardRecommendationTrace.AssessmentFactLines.DefaultIfEmpty("없음"))
            {
                builder.AppendLine($"- {fact}");
            }

            builder.AppendLine("### Deterministic Input Knowledge");
            foreach (var reference in response.RewardRecommendationTrace.InputKnowledgeRefs.DefaultIfEmpty("없음"))
            {
                builder.AppendLine($"- {reference}");
            }
        }

        return builder.ToString().TrimEnd();
    }
}
