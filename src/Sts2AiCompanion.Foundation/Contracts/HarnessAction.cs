namespace Sts2AiCompanion.Foundation.Contracts;

public sealed record HarnessAction(
    string ActionId,
    string Kind,
    DateTimeOffset RequestedAt,
    string? TargetRef,
    string? TargetLabel,
    string? TargetValue,
    int TimeoutMs,
    int RetryBudget,
    string SafetyClass,
    string Reason,
    string? ExpectedStateDelta,
    IReadOnlyDictionary<string, string?> Metadata)
{
    public static HarnessAction Create(string kind, string reason, string? targetLabel = null)
    {
        return new HarnessAction(
            Guid.NewGuid().ToString("N"),
            kind,
            DateTimeOffset.UtcNow,
            null,
            targetLabel,
            null,
            5000,
            0,
            "normal",
            reason,
            null,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase));
    }
}

public sealed record HarnessActionResult(
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    string? FailureKind,
    bool Recoverable,
    string? ObservedStateDelta,
    IReadOnlyList<string> ArtifactRefs);
