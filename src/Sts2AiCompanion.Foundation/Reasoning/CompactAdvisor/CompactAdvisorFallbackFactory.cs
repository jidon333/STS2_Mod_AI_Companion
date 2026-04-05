using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

public static class CompactAdvisorFallbackFactory
{
    public static AdviceResponse CreateUnsupportedScene(
        AdviceInputPack inputPack,
        string sceneType)
    {
        return new AdviceResponse(
            "degraded",
            "지원되지 않는 장면",
            $"현재 compact advisor MVP는 reward/event 장면만 지원합니다. 현재 장면은 {sceneType} 입니다.",
            "현재 화면을 계속 관찰하고 reward 또는 event 장면에서 다시 수동 요청하세요.",
            null,
            new[]
            {
                $"scene={sceneType}",
                "compact advisor manual path is limited to reward/event in this wave.",
            },
            Array.Empty<string>(),
            Array.Empty<string>(),
            new[] { $"unsupported-scene-for-compact-advisor:{sceneType}" },
            null,
            Array.Empty<string>(),
            DateTimeOffset.UtcNow,
            inputPack.RunId,
            inputPack.TriggerKind,
            null,
            null,
            inputPack.RewardRecommendationTraceSeed);
    }

    public static AdviceResponse CreateInsufficientCompactInput(
        AdviceInputPack inputPack,
        string reasonCode,
        IReadOnlyList<string> missingInformation,
        IReadOnlyList<string> decisionBlockers)
    {
        return new AdviceResponse(
            "degraded",
            "정보 부족",
            "현재 reward/event 장면에 대한 compact advisor 입력이 충분히 정리되지 않아 모델 호출을 생략했습니다.",
            "현재 화면과 선택지를 다시 확인하고, 정보가 더 안정된 뒤 수동 요청을 다시 시도하세요.",
            null,
            new[]
            {
                $"compact_reason={reasonCode}",
                $"scene={inputPack.CompactInput?.SceneType ?? inputPack.CurrentScreen}",
            },
            Array.Empty<string>(),
            missingInformation,
            decisionBlockers
                .Append(reasonCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            null,
            inputPack.CompactInput?.KnowledgeEntries.Select(entry => entry.Id).Take(6).ToArray()
                ?? Array.Empty<string>(),
            DateTimeOffset.UtcNow,
            inputPack.RunId,
            inputPack.TriggerKind,
            null,
            null,
            inputPack.RewardRecommendationTraceSeed);
    }

    public static AdviceResponse CreateInvalidChoiceLabel(
        AdviceInputPack inputPack,
        AdviceResponse response)
    {
        return response with
        {
            Status = "degraded",
            Headline = "추천 라벨 불일치",
            Summary = "모델이 현재 화면에 보이는 선택지와 정확히 일치하지 않는 추천 라벨을 반환해 추천을 숨깁니다.",
            RecommendedChoiceLabel = null,
            DecisionBlockers = response.DecisionBlockers
                .Append("recommended-choice-not-in-current-scene-options")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            RewardRecommendationTrace = inputPack.RewardRecommendationTraceSeed,
        };
    }
}
