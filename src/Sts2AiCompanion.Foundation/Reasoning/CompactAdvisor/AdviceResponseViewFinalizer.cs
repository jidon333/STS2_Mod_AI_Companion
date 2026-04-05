using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

internal static class AdviceResponseViewFinalizer
{
    public static AdviceResponse Apply(AdviceInputPack inputPack, AdviceResponse response)
    {
        var labels = inputPack.CompactInput?.VisibleOptions
            .Select(static option => option.Label)
            .Where(static label => !string.IsNullOrWhiteSpace(label))
            .Distinct(StringComparer.Ordinal)
            .ToArray()
            ?? Array.Empty<string>();

        var conservativeView = SanitizeAuxiliaryView(response.ConservativeView, labels);
        var aggressiveView = SanitizeAuxiliaryView(response.AggressiveView, labels);
        var (finalView, finalInvalid) = SanitizeFinalView(response.FinalView, labels);

        var finalized = response with
        {
            ConservativeView = conservativeView,
            AggressiveView = aggressiveView,
            FinalView = finalView,
        };

        if (finalInvalid)
        {
            finalized = finalized with
            {
                DecisionBlockers = finalized.DecisionBlockers
                    .Append("recommended-choice-not-in-current-scene-options")
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
            };
        }

        return ResyncCanonical(finalized);
    }

    private static AdvicePerspectiveView? SanitizeAuxiliaryView(AdvicePerspectiveView? view, IReadOnlyCollection<string> labels)
    {
        if (view is null || string.IsNullOrWhiteSpace(view.RecommendedChoiceLabel) || labels.Count == 0)
        {
            return view;
        }

        if (labels.Contains(view.RecommendedChoiceLabel))
        {
            return view;
        }

        return view with
        {
            RecommendedChoiceLabel = null,
            RiskNotes = view.RiskNotes
                .Append($"추천 라벨 '{view.RecommendedChoiceLabel}'이 현재 visible options와 exact match하지 않아 숨깁니다.")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
        };
    }

    private static (AdvicePerspectiveView? View, bool Invalid) SanitizeFinalView(AdvicePerspectiveView? view, IReadOnlyCollection<string> labels)
    {
        if (view is null || string.IsNullOrWhiteSpace(view.RecommendedChoiceLabel) || labels.Count == 0)
        {
            return (view, false);
        }

        if (labels.Contains(view.RecommendedChoiceLabel))
        {
            return (view, false);
        }

        return (
            view with
            {
                RecommendedChoiceLabel = null,
                RiskNotes = view.RiskNotes
                    .Append($"추천 라벨 '{view.RecommendedChoiceLabel}'이 현재 visible options와 exact match하지 않아 숨깁니다.")
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
            },
            true);
    }

    private static AdviceResponse ResyncCanonical(AdviceResponse response)
    {
        var finalView = response.FinalView ?? new AdvicePerspectiveView(
            response.Headline,
            response.RecommendedChoiceLabel,
            response.Summary,
            response.ReasoningBullets,
            response.RiskNotes);

        return response with
        {
            Headline = finalView.Headline,
            Summary = finalView.Summary,
            RecommendedChoiceLabel = finalView.RecommendedChoiceLabel,
            ReasoningBullets = finalView.ReasoningBullets,
            RiskNotes = finalView.RiskNotes,
            FinalView = finalView,
        };
    }
}
