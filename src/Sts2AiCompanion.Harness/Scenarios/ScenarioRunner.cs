using System.Text.Json;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Harness.Actions;
using Sts2AiCompanion.Harness.Evaluation;
using Sts2AiCompanion.Harness.Policies;
using Sts2AiCompanion.Harness.Recovery;

namespace Sts2AiCompanion.Harness.Scenarios;

public sealed class ScenarioRunner
{
    private readonly DeterministicPolicyEngine _policyEngine;
    private readonly IHarnessActionExecutor _actionExecutor;
    private readonly RecoveryManager _recoveryManager;
    private readonly AcceptanceEvaluator _acceptanceEvaluator;

    public ScenarioRunner(
        DeterministicPolicyEngine policyEngine,
        IHarnessActionExecutor actionExecutor,
        RecoveryManager recoveryManager,
        AcceptanceEvaluator acceptanceEvaluator)
    {
        _policyEngine = policyEngine;
        _actionExecutor = actionExecutor;
        _recoveryManager = recoveryManager;
        _acceptanceEvaluator = acceptanceEvaluator;
    }

    public async Task<ScenarioRunResult> RunAsync(string scenarioPath, CompanionState initialState, CancellationToken cancellationToken)
    {
        var scenario = JsonSerializer.Deserialize<ScenarioDefinition>(await File.ReadAllTextAsync(scenarioPath, cancellationToken).ConfigureAwait(false))
            ?? throw new InvalidOperationException($"Scenario could not be parsed: {scenarioPath}");

        var actionLog = new List<HarnessAction>();
        var resultLog = new List<HarnessActionResult>();
        var currentState = initialState;

        foreach (var step in scenario.Steps)
        {
            var action = _policyEngine.ResolveAction(step, currentState);
            actionLog.Add(action);

            var result = await _actionExecutor.ExecuteAsync(action, currentState, cancellationToken).ConfigureAwait(false);
            resultLog.Add(result);
            if (!string.Equals(result.Status, "ok", StringComparison.OrdinalIgnoreCase) && result.Recoverable)
            {
                await _recoveryManager.TryRecoverAsync(step.Name, currentState, cancellationToken).ConfigureAwait(false);
            }
        }

        var evaluation = _acceptanceEvaluator.Evaluate(scenario, currentState, actionLog, resultLog);
        return new ScenarioRunResult(scenario, actionLog, resultLog, evaluation);
    }
}

public sealed record ScenarioDefinition(
    string Id,
    string Description,
    IReadOnlyList<ScenarioStep> Steps);

public sealed record ScenarioStep(
    string Name,
    string TargetScene,
    string ActionKind,
    string? TargetLabel,
    string? ExpectedResult);

public sealed record ScenarioRunResult(
    ScenarioDefinition Scenario,
    IReadOnlyList<HarnessAction> Actions,
    IReadOnlyList<HarnessActionResult> Results,
    AcceptanceReport Evaluation);
