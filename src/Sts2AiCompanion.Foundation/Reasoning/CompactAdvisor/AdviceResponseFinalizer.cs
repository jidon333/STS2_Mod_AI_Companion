using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

public static class AdviceResponseFinalizer
{
    public static AdviceResponse Apply(AdviceInputPack inputPack, AdviceResponse response)
    {
        var sceneType = CompactAdvisorScenePolicy.NormalizeSceneType(inputPack.CompactInput?.SceneType ?? inputPack.CurrentScreen);
        var finalized = string.Equals(sceneType, "reward", StringComparison.OrdinalIgnoreCase)
            ? RewardAdviceResponseFinalizer.Apply(inputPack, response)
            : response;
        finalized = string.Equals(sceneType, "combat", StringComparison.OrdinalIgnoreCase)
            ? CombatPreviewResponseFinalizer.Apply(inputPack, finalized)
            : finalized;
        return CompactChoiceLabelFinalizer.Apply(inputPack, finalized);
    }
}
