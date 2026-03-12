using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Harness.Scenarios;

namespace Sts2AiCompanion.Harness.Evaluation;

public sealed class AcceptanceEvaluator
{
    public AcceptanceReport Evaluate(
        ScenarioDefinition scenario,
        CompanionState finalState,
        IReadOnlyList<HarnessAction> actions,
        IReadOnlyList<HarnessActionResult> results)
    {
        var failedSteps = results.Count(result => !string.Equals(result.Status, "ok", StringComparison.OrdinalIgnoreCase));
        return new AcceptanceReport(
            scenario.Id,
            failedSteps == 0 ? "pending-implementation" : "failed",
            actions.Count,
            failedSteps,
            finalState.Scene.SceneType,
            finalState.Run.RunId);
    }
}

public sealed record AcceptanceReport(
    string ScenarioId,
    string Status,
    int TotalActions,
    int FailedActions,
    string FinalScene,
    string? RunId);
