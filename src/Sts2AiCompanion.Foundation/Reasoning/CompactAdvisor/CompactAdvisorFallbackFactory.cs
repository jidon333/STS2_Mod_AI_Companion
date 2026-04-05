using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

public static class CompactAdvisorFallbackFactory
{
    public static AdviceResponse CreateUnsupportedScene(
        AdviceInputPack inputPack,
        string sceneType)
    {
        var normalizedSceneType = CompactAdvisorScenePolicy.NormalizeSceneType(inputPack.CompactInput?.SceneType ?? sceneType);
        if (string.Equals(normalizedSceneType, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return CreateCombatPreview(
                inputPack,
                "combat-preview-only",
                inputPack.CompactInput?.MissingInformation ?? Array.Empty<string>(),
                inputPack.CompactInput?.DecisionBlockers ?? Array.Empty<string>());
        }

        return new AdviceResponse(
            "degraded",
            "지원되지 않는 장면",
            $"현재 compact advisor MVP는 reward/event/shop 장면의 model-backed 조언과 combat preview만 지원합니다. 현재 장면은 {sceneType} 입니다.",
            "현재 화면을 계속 관찰하고 compact advisor 지원 장면에서 다시 수동 요청하세요.",
            null,
            new[]
            {
                $"scene={normalizedSceneType}",
                "compact advisor manual path is limited to reward/event/shop plus combat preview in this wave.",
            },
            Array.Empty<string>(),
            Array.Empty<string>(),
            new[] { $"unsupported-scene-for-compact-advisor:{normalizedSceneType}" },
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
        if (string.Equals(reasonCode, "combat-preview-only", StringComparison.OrdinalIgnoreCase))
        {
            return CreateCombatPreview(inputPack, reasonCode, missingInformation, decisionBlockers);
        }

        var sceneType = CompactAdvisorScenePolicy.NormalizeSceneType(inputPack.CompactInput?.SceneType ?? inputPack.CurrentScreen);
        return new AdviceResponse(
            "degraded",
            "정보 부족",
            $"현재 {sceneType} compact advisor 입력이 충분히 정리되지 않아 모델 호출을 생략했습니다.",
            "현재 화면과 선택지를 다시 확인하고, 정보가 더 안정된 뒤 수동 요청을 다시 시도하세요.",
            null,
            new[]
            {
                $"compact_reason={reasonCode}",
                $"scene={sceneType}",
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

    public static AdviceResponse CreateCombatPreview(
        AdviceInputPack inputPack,
        string reasonCode,
        IReadOnlyList<string> missingInformation,
        IReadOnlyList<string> decisionBlockers)
    {
        return new AdviceResponse(
            "degraded",
            "전투 미리보기",
            "현재 combat compact preview는 전투 사실 요약만 제공하고 실제 추천은 하지 않습니다.",
            "현재 전투 사실을 검토한 뒤 직접 판단하세요. 추천 자동화는 다음 wave 범위입니다.",
            null,
            new[]
            {
                $"compact_reason={reasonCode}",
                $"scene={inputPack.CompactInput?.SceneType ?? inputPack.CurrentScreen}",
            },
            Array.Empty<string>(),
            missingInformation,
            decisionBlockers
                .Append("combat-preview-only")
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
        var degradedRiskNotes = (response.FinalView?.RiskNotes ?? response.RiskNotes)
            .Append("현재 visible options와 exact match하지 않는 추천 라벨을 숨겼습니다.")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var finalView = response.FinalView ?? new AdvicePerspectiveView(
            "추천 라벨 불일치",
            null,
            "모델이 현재 화면에 보이는 선택지와 정확히 일치하지 않는 추천 라벨을 반환해 추천을 숨깁니다.",
            response.ReasoningBullets,
            degradedRiskNotes);
        return response with
        {
            Status = "degraded",
            Headline = "추천 라벨 불일치",
            Summary = "모델이 현재 화면에 보이는 선택지와 정확히 일치하지 않는 추천 라벨을 반환해 추천을 숨깁니다.",
            RecommendedChoiceLabel = null,
            ReasoningBullets = finalView.ReasoningBullets,
            RiskNotes = degradedRiskNotes,
            DecisionBlockers = response.DecisionBlockers
                .Append("recommended-choice-not-in-current-scene-options")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            RewardRecommendationTrace = inputPack.RewardRecommendationTraceSeed,
            FinalView = finalView with
            {
                Headline = "추천 라벨 불일치",
                RecommendedChoiceLabel = null,
                Summary = "모델이 현재 화면에 보이는 선택지와 정확히 일치하지 않는 추천 라벨을 반환해 추천을 숨깁니다.",
                RiskNotes = degradedRiskNotes,
            },
        };
    }
}
