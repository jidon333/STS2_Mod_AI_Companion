using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

public static class CombatPreviewResponseFinalizer
{
    public static AdviceResponse Apply(AdviceInputPack inputPack, AdviceResponse response)
    {
        if (!string.Equals(
                CompactAdvisorScenePolicy.NormalizeSceneType(inputPack.CompactInput?.SceneType ?? inputPack.CurrentScreen),
                "combat",
                StringComparison.OrdinalIgnoreCase))
        {
            return response;
        }

        return response with
        {
            Status = "degraded",
            Headline = string.IsNullOrWhiteSpace(response.Headline) ? "전투 미리보기" : response.Headline,
            RecommendedChoiceLabel = null,
            DecisionBlockers = response.DecisionBlockers
                .Append("combat-preview-only")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
        };
    }
}
