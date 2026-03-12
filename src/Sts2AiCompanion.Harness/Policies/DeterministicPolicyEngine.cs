using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Harness.Scenarios;

namespace Sts2AiCompanion.Harness.Policies;

public sealed class DeterministicPolicyEngine
{
    public HarnessAction ResolveAction(ScenarioStep step, CompanionState state)
    {
        var reason = $"scenario-step:{step.Name}|scene:{state.Scene.SceneType}";
        return HarnessAction.Create(step.ActionKind, reason, step.TargetLabel);
    }
}
