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
    public static HarnessAction Create(
        string kind,
        string reason,
        string? targetLabel = null,
        int timeoutMs = 8_000,
        int retryBudget = 0,
        string safetyClass = "normal",
        string? expectedStateDelta = null,
        IReadOnlyDictionary<string, string?>? metadata = null)
    {
        return new HarnessAction(
            Guid.NewGuid().ToString("N"),
            kind,
            DateTimeOffset.UtcNow,
            null,
            targetLabel,
            null,
            Math.Max(timeoutMs, 1_000),
            Math.Max(retryBudget, 0),
            safetyClass,
            reason,
            expectedStateDelta,
            metadata ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase));
    }
}

public sealed record HarnessActionResult(
    string ActionId,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    string? FailureKind,
    bool Recoverable,
    string? ObservedStateDelta,
    IReadOnlyList<string> ArtifactRefs);

public sealed record HarnessBridgeStatus(
    bool Enabled,
    string Mode,
    string? LastActionId,
    string? LastResultStatus,
    DateTimeOffset UpdatedAt,
    string? Message);
