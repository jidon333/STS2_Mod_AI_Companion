using FoundationAdviceInputPack = Sts2AiCompanion.Foundation.Contracts.AdviceInputPack;
using FoundationAdviceResponse = Sts2AiCompanion.Foundation.Contracts.AdviceResponse;
using FoundationAdviceTrigger = Sts2AiCompanion.Foundation.Contracts.AdviceTrigger;
using FoundationCodexSessionState = Sts2AiCompanion.Foundation.Contracts.CodexSessionState;
using FoundationKnowledgeSlice = Sts2AiCompanion.Foundation.Contracts.KnowledgeSlice;
using FoundationRunState = Sts2AiCompanion.Foundation.Contracts.CompanionRunState;

namespace Sts2AiCompanion.Host;

internal static class FoundationContractAdapter
{
    public static FoundationRunState ToFoundation(this CompanionRunState runState)
    {
        return new FoundationRunState(
            runState.Snapshot,
            runState.Session,
            runState.SummaryText,
            runState.RecentEvents.ToArray(),
            runState.IsStale)
        {
            NormalizedState = runState.NormalizedState,
        };
    }

    public static CompanionRunState ToHost(this FoundationRunState runState)
    {
        return new CompanionRunState(
            runState.Snapshot,
            runState.Session,
            runState.SummaryText,
            runState.RecentEvents.ToArray(),
            runState.IsStale)
        {
            NormalizedState = runState.NormalizedState,
        };
    }

    public static FoundationAdviceTrigger ToFoundation(this AdviceTrigger trigger)
    {
        return new FoundationAdviceTrigger(
            trigger.Kind,
            trigger.RequestedAt,
            trigger.Manual,
            trigger.BypassMinInterval,
            trigger.Reason,
            trigger.SourceEvent,
            trigger.RetrySourcePromptPackPath);
    }

    public static FoundationKnowledgeSlice ToFoundation(this KnowledgeSlice slice)
    {
        return new FoundationKnowledgeSlice(slice.Entries.ToArray(), slice.ApproximateBytes, slice.Reasons.ToArray());
    }

    public static KnowledgeSlice ToHost(this FoundationKnowledgeSlice slice)
    {
        return new KnowledgeSlice(slice.Entries.ToArray(), slice.ApproximateBytes, slice.Reasons.ToArray());
    }

    public static FoundationAdviceInputPack ToFoundation(this AdviceInputPack inputPack)
    {
        return new FoundationAdviceInputPack(
            inputPack.RunId,
            inputPack.TriggerKind,
            inputPack.CreatedAt,
            inputPack.Manual,
            inputPack.CurrentScreen,
            inputPack.SummaryText,
            inputPack.Snapshot,
            inputPack.RecentEvents.ToArray(),
            inputPack.KnowledgeEntries.ToArray(),
            inputPack.KnowledgeReasons.ToArray(),
            inputPack.ConstraintsText,
            inputPack.NormalizedState,
            inputPack.RewardOptionSet,
            inputPack.RewardAssessmentFacts,
            inputPack.RewardRecommendationTraceSeed,
            inputPack.CompactInput,
            inputPack.StrategyPrinciples);
    }

    public static AdviceInputPack ToHost(this FoundationAdviceInputPack inputPack)
    {
        return new AdviceInputPack(
            inputPack.RunId,
            inputPack.TriggerKind,
            inputPack.CreatedAt,
            inputPack.Manual,
            inputPack.CurrentScreen,
            inputPack.SummaryText,
            inputPack.Snapshot,
            inputPack.RecentEvents.ToArray(),
            inputPack.KnowledgeEntries.ToArray(),
            inputPack.KnowledgeReasons.ToArray(),
            inputPack.ConstraintsText,
            inputPack.NormalizedState,
            inputPack.RewardOptionSet,
            inputPack.RewardAssessmentFacts,
            inputPack.RewardRecommendationTraceSeed,
            inputPack.CompactInput,
            inputPack.StrategyPrinciples);
    }

    public static FoundationAdviceResponse ToFoundation(this AdviceResponse response)
    {
        return new FoundationAdviceResponse(
            response.Status,
            response.Headline,
            response.Summary,
            response.RecommendedAction,
            response.RecommendedChoiceLabel,
            response.ReasoningBullets.ToArray(),
            response.RiskNotes.ToArray(),
            response.MissingInformation.ToArray(),
            response.DecisionBlockers.ToArray(),
            response.Confidence,
            response.KnowledgeRefs.ToArray(),
            response.GeneratedAt,
            response.RunId,
            response.TriggerKind,
            response.SessionId,
            response.RawResponse,
            response.RewardRecommendationTrace,
            response.ConservativeView,
            response.AggressiveView,
            response.FinalView);
    }

    public static AdviceResponse ToHost(this FoundationAdviceResponse response)
    {
        return new AdviceResponse(
            response.Status,
            response.Headline,
            response.Summary,
            response.RecommendedAction,
            response.RecommendedChoiceLabel,
            response.ReasoningBullets.ToArray(),
            response.RiskNotes.ToArray(),
            response.MissingInformation.ToArray(),
            response.DecisionBlockers.ToArray(),
            response.Confidence,
            response.KnowledgeRefs.ToArray(),
            response.GeneratedAt,
            response.RunId,
            response.TriggerKind,
            response.SessionId,
            response.RawResponse,
            response.RewardRecommendationTrace,
            response.ConservativeView,
            response.AggressiveView,
            response.FinalView);
    }

    public static FoundationCodexSessionState ToFoundation(this CodexSessionState state)
    {
        return new FoundationCodexSessionState(state.RunId, state.SessionId, state.CreatedAt, state.UpdatedAt);
    }

    public static CodexSessionState ToHost(this FoundationCodexSessionState state)
    {
        return new CodexSessionState(state.RunId, state.SessionId, state.CreatedAt, state.UpdatedAt);
    }
}
