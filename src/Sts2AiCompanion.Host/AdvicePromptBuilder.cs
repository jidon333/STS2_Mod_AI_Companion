using Sts2ModKit.Core.Configuration;
using FoundationAdvicePromptBuilder = Sts2AiCompanion.Foundation.Reasoning.AdvicePromptBuilder;

namespace Sts2AiCompanion.Host;

public sealed class AdvicePromptBuilder
{
    private readonly FoundationAdvicePromptBuilder _inner;

    public AdvicePromptBuilder(ScaffoldConfiguration configuration)
        : this(new FoundationAdvicePromptBuilder(configuration))
    {
    }

    internal AdvicePromptBuilder(FoundationAdvicePromptBuilder inner)
    {
        _inner = inner;
    }

    public AdviceInputPack BuildInputPack(CompanionRunState runState, AdviceTrigger trigger, KnowledgeSlice slice)
    {
        return _inner.BuildInputPack(runState.ToFoundation(), trigger.ToFoundation(), slice.ToFoundation()).ToHost();
    }

    public string FormatPrompt(AdviceInputPack inputPack)
    {
        return _inner.FormatPrompt(inputPack.ToFoundation());
    }

    public string FormatAdviceMarkdown(AdviceResponse response)
    {
        return _inner.FormatAdviceMarkdown(response.ToFoundation());
    }
}
