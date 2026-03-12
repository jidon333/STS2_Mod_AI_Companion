using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using Sts2ModKit.Core.Configuration;

namespace Sts2AiCompanion.Host;

public sealed class AdvicePromptBuilder
{
    private readonly ScaffoldConfiguration _configuration;

    public AdvicePromptBuilder(ScaffoldConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AdviceInputPack BuildInputPack(CompanionRunState runState, AdviceTrigger trigger, KnowledgeSlice slice)
    {
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
            + "게임을 대신 플레이하지 말고, 입력에 없는 정보는 추정하지 말고, 현재 상태와 최근 이벤트, 관련 지식 조각만 근거로 판단하세요.");
    }

    public string FormatPrompt(AdviceInputPack inputPack)
    {
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
        builder.AppendLine();
        builder.AppendLine("current_state_summary:");
        builder.AppendLine(inputPack.SummaryText);
        builder.AppendLine();
        builder.AppendLine("recent_events:");
        foreach (var envelope in inputPack.RecentEvents)
        {
            builder.AppendLine($"- {envelope.Ts:O} {envelope.Kind} screen={envelope.Screen} act={envelope.Act?.ToString() ?? "?"} floor={envelope.Floor?.ToString() ?? "?"}");
            if (envelope.Payload.Count > 0)
            {
                builder.AppendLine($"  payload: {JsonSerializer.Serialize(envelope.Payload)}");
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
                builder.AppendLine($"  raw: {entry.RawText}");
            }

            foreach (var option in entry.Options.Take(5))
            {
                builder.AppendLine($"  option: {option.Label} :: {option.Description ?? "추가 정보 없음"}");
            }
        }

        if (inputPack.KnowledgeEntries.Count == 0)
        {
            builder.AppendLine("- none");
        }

        builder.AppendLine();
        builder.AppendLine("response_instructions:");
        builder.AppendLine("- headline: 현재 상황의 핵심을 한 줄로 요약");
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

        return builder.ToString().TrimEnd();
    }
}
