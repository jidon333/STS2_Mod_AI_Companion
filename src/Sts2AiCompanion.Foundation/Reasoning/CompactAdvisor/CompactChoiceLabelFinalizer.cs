using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

public static class CompactChoiceLabelFinalizer
{
    public static AdviceResponse Apply(AdviceInputPack inputPack, AdviceResponse response)
    {
        if (inputPack.CompactInput is null)
        {
            return response;
        }

        if (string.IsNullOrWhiteSpace(response.RecommendedChoiceLabel))
        {
            return response.DecisionBlockers.Contains("recommended-choice-not-in-option-set", StringComparer.OrdinalIgnoreCase)
                ? CompactAdvisorFallbackFactory.CreateInvalidChoiceLabel(inputPack, response)
                : response;
        }

        var labels = inputPack.CompactInput.VisibleOptions
            .Select(option => option.Label)
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .ToArray();
        if (labels.Contains(response.RecommendedChoiceLabel, StringComparer.Ordinal))
        {
            return response;
        }

        return CompactAdvisorFallbackFactory.CreateInvalidChoiceLabel(inputPack, response);
    }
}
