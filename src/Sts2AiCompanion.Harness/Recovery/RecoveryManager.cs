using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Harness.Recovery;

public sealed class RecoveryManager
{
    public Task<bool> TryRecoverAsync(string stepName, CompanionState state, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
}
