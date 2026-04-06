using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Knowledge;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

public sealed class RewardEventCompactAdvisorInputBuilder
{
    private readonly CompactAdvisorInputBuilder _inner = new();

    public CompactAdvisorBuildResult Build(
        CompanionRunState runState,
        KnowledgeSlice boundedSlice,
        StaticKnowledgeCatalog? catalog = null)
    {
        return _inner.Build(runState, boundedSlice, catalog);
    }
}
