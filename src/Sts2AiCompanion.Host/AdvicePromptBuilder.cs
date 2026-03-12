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
            "당신은 Slay the Spire 2 조언 어시스턴트입니다. 게임 플레이를 자동화하지 말고, 없는 정보를 지어내지 말고, 현재 상태와 최근 이벤트와 제공된 지식 조각만 사용해 한국어로 답하세요.");
    }

    public string FormatPrompt(AdviceInputPack inputPack)
    {
        var builder = new StringBuilder();
        builder.AppendLine("당신은 Slay the Spire 2 조언 어시스턴트입니다.");
        builder.AppendLine("- 게임 플레이를 자동화하지 마세요.");
        builder.AppendLine("- 현재 상태, 현재 선택지, 최근 이벤트, 관련 지식 조각만 근거로 사용하세요.");
        builder.AppendLine("- 데이터가 부족하면 추측하지 말고 부족하다고 명시하세요.");
        builder.AppendLine("- 반드시 한국어로 답하세요.");
        builder.AppendLine("- JSON 스키마를 정확히 따르세요.");
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
                builder.AppendLine($"  option: {option.Label} :: {option.Description ?? "세부 정보 없음"}");
            }
        }

        if (inputPack.KnowledgeEntries.Count == 0)
        {
            builder.AppendLine("- none");
        }

        builder.AppendLine();
        builder.AppendLine("response_instructions:");
        builder.AppendLine("- headline: 현재 상황을 한 줄로 요약");
        builder.AppendLine("- summary: 2~4문장의 간결한 조언");
        builder.AppendLine("- recommendedAction: 지금 가장 좋은 다음 행동");
        builder.AppendLine("- recommendedChoiceLabel: 선택지가 있을 때 추천 선택지 라벨");
        builder.AppendLine("- reasoningBullets: 2~5개의 짧은 근거");
        builder.AppendLine("- riskNotes: 0~3개의 불확실성 또는 리스크 메모");
        builder.AppendLine("- confidence: 0.0에서 1.0 사이 숫자");
        builder.AppendLine("- knowledgeRefs: 근거로 사용한 id 또는 이름");
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
        builder.AppendLine("## 참고 지식");
        foreach (var reference in response.KnowledgeRefs.DefaultIfEmpty("없음"))
        {
            builder.AppendLine($"- {reference}");
        }

        return builder.ToString().TrimEnd();
    }
}
