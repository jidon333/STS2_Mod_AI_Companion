using Sts2AiCompanion.Host;

internal sealed class FakeCodexSessionClient : ICodexSessionClient
{
    public int RequestCount { get; private set; }

    public Task<(AdviceResponse Response, string? SessionId)> ExecuteAsync(
        AdviceInputPack inputPack,
        string prompt,
        string? sessionId,
        string? modelOverride,
        string? reasoningEffortOverride,
        CancellationToken cancellationToken)
    {
        RequestCount += 1;
        var resolvedSessionId = sessionId ?? "fake-session-001";
        return Task.FromResult<(AdviceResponse Response, string? SessionId)>((
            new AdviceResponse(
                "ok",
                $"headline-{inputPack.TriggerKind}",
                $"summary-{inputPack.TriggerKind}",
                $"action-{inputPack.TriggerKind}",
                inputPack.KnowledgeEntries.FirstOrDefault()?.Name,
                new[] { "reason-1", "reason-2" },
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                0.8,
                inputPack.KnowledgeEntries.Take(3).Select(entry => entry.Id).ToArray(),
                DateTimeOffset.UtcNow,
                inputPack.RunId,
                inputPack.TriggerKind,
                resolvedSessionId,
                "{\"status\":\"ok\"}"),
            resolvedSessionId));
    }
}
