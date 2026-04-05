using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

public static class AdviceResponseFinalizer
{
    public static AdviceResponse Apply(AdviceInputPack inputPack, AdviceResponse response)
    {
        var finalized = RewardAdviceResponseFinalizer.Apply(inputPack, response);
        return CompactChoiceLabelFinalizer.Apply(inputPack, finalized);
    }
}
