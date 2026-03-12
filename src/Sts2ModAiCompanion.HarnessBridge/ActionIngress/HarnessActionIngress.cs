using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2ModAiCompanion.HarnessBridge.ActionIngress;

public interface IHarnessActionIngress
{
    Task<HarnessActionResult> SubmitAsync(HarnessAction action, CancellationToken cancellationToken);
}

public sealed record HarnessIngressStatus(
    bool Enabled,
    string Mode,
    string? LastActionId,
    DateTimeOffset UpdatedAt);
