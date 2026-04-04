using Sts2AiCompanion.Foundation.Contracts;

internal sealed class FakeFoundationCodexSessionClient : ICodexSessionClient
{
    private readonly Queue<Func<string?, (AdviceResponse Response, string? SessionId)>> handlers = new();

    public IReadOnlyList<string?> SeenSessionIds => seenSessionIds;

    private readonly List<string?> seenSessionIds = new();

    public int RequestCount => seenSessionIds.Count;

    public void Enqueue(Func<string?, (AdviceResponse Response, string? SessionId)> handler)
    {
        handlers.Enqueue(handler);
    }

    public Task<(AdviceResponse Response, string? SessionId)> ExecuteAsync(
        AdviceInputPack inputPack,
        string prompt,
        string? sessionId,
        string? modelOverride,
        string? reasoningEffortOverride,
        CancellationToken cancellationToken)
    {
        seenSessionIds.Add(sessionId);
        if (handlers.Count == 0)
        {
            throw new InvalidOperationException("No fake foundation Codex handler was queued.");
        }

        return Task.FromResult(handlers.Dequeue()(sessionId));
    }
}
