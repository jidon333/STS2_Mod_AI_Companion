using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Harness.Actions;

public interface IHarnessActionExecutor
{
    Task<HarnessActionResult> ExecuteAsync(HarnessAction action, CompanionState state, CancellationToken cancellationToken);
}
