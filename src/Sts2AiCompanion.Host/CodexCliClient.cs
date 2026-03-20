using Sts2ModKit.Core.Configuration;
using FoundationCodexCliClient = Sts2AiCompanion.Foundation.Reasoning.CodexCliClient;
using FoundationCodexSessionClient = Sts2AiCompanion.Foundation.Contracts.ICodexSessionClient;

namespace Sts2AiCompanion.Host;

public sealed class CodexCliClient : ICodexSessionClient
{
    private readonly FoundationCodexSessionClient _inner;

    public CodexCliClient(ScaffoldConfiguration configuration, string workspaceRoot)
        : this(new FoundationCodexCliClient(configuration, workspaceRoot))
    {
    }

    internal CodexCliClient(FoundationCodexSessionClient inner)
    {
        _inner = inner;
    }

    public async Task<(AdviceResponse Response, string? SessionId)> ExecuteAsync(
        AdviceInputPack inputPack,
        string prompt,
        string? sessionId,
        string? modelOverride,
        string? reasoningEffortOverride,
        CancellationToken cancellationToken)
    {
        var (response, resolvedSessionId) = await _inner.ExecuteAsync(
            inputPack.ToFoundation(),
            prompt,
            sessionId,
            modelOverride,
            reasoningEffortOverride,
            cancellationToken).ConfigureAwait(false);

        return (response.ToHost(), resolvedSessionId);
    }
}
