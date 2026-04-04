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
        var foundationInput = inputPack.ToFoundation();
        var (response, resolvedSessionId) = await _inner.ExecuteAsync(
            foundationInput,
            prompt,
            sessionId,
            modelOverride,
            reasoningEffortOverride,
            cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(sessionId) && ShouldRetryWithoutSession(response))
        {
            var retry = await _inner.ExecuteAsync(
                foundationInput,
                prompt,
                null,
                modelOverride,
                reasoningEffortOverride,
                cancellationToken).ConfigureAwait(false);
            return (retry.Response.ToHost(), retry.SessionId);
        }

        return (response.ToHost(), resolvedSessionId);
    }

    private static bool ShouldRetryWithoutSession(Sts2AiCompanion.Foundation.Contracts.AdviceResponse response)
    {
        if (!string.Equals(response.Status, "degraded", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ContainsContextOverflowDiagnostic(response.Summary)
            || response.DecisionBlockers.Any(ContainsContextOverflowDiagnostic)
            || response.RiskNotes.Any(ContainsContextOverflowDiagnostic)
            || ContainsContextOverflowDiagnostic(response.RawResponse);
    }

    private static bool ContainsContextOverflowDiagnostic(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return text.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase)
            || text.Contains("context window", StringComparison.OrdinalIgnoreCase)
            || text.Contains("입력 크기를 줄여야", StringComparison.OrdinalIgnoreCase)
            || text.Contains("모델 컨텍스트 한도", StringComparison.OrdinalIgnoreCase);
    }
}
