using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Knowledge;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

public sealed class CompactAdvisorInputBuilder
{
    private readonly RewardCompactInputBuilder _rewardBuilder = new();
    private readonly EventCompactInputBuilder _eventBuilder = new();
    private readonly ShopCompactInputBuilder _shopBuilder = new();
    private readonly CombatCompactPreviewBuilder _combatBuilder = new();

    public CompactAdvisorBuildResult Build(CompanionRunState runState, KnowledgeSlice boundedSlice)
    {
        ArgumentNullException.ThrowIfNull(runState);
        ArgumentNullException.ThrowIfNull(boundedSlice);

        return CompactAdvisorScenePolicy.NormalizeSceneType(runState.NormalizedState.Scene.SceneType) switch
        {
            "reward" => _rewardBuilder.Build(runState, boundedSlice),
            "event" => _eventBuilder.Build(runState, boundedSlice),
            "shop" => _shopBuilder.Build(runState, boundedSlice),
            "combat" => _combatBuilder.Build(runState, boundedSlice),
            var sceneType => new CompactAdvisorBuildResult(
                false,
                null,
                Array.Empty<string>(),
                new[] { $"unsupported-scene-for-compact-advisor:{sceneType}" },
                $"unsupported-scene-for-compact-advisor:{sceneType}"),
        };
    }
}
