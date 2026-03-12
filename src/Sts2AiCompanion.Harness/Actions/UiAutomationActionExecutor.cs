using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Harness.Actions;

public sealed class UiAutomationActionExecutor : IHarnessActionExecutor
{
    public Task<HarnessActionResult> ExecuteAsync(HarnessAction action, CompanionState state, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HarnessActionResult(
            "not-implemented",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            "ui-automation-not-implemented",
            false,
            null,
            Array.Empty<string>()));
    }
}
