using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Harness.Scenarios;

namespace Sts2AiCompanion.Harness.Evaluation;

public sealed class AcceptanceEvaluator
{
    public AcceptanceReport Evaluate(
        ScenarioDefinition scenario,
        CompanionState finalState,
        IReadOnlyList<ScenarioStepReport> stepReports)
    {
        var totalActions = stepReports.Sum(step => step.Actions.Count);
        var failedSteps = stepReports.Count(step => !string.Equals(step.Status, "passed", StringComparison.OrdinalIgnoreCase));
        var requiredScenes = scenario.Steps.Select(step => step.TargetScene).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var satisfiedScenes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < scenario.Steps.Count; index += 1)
        {
            var step = scenario.Steps[index];
            if (index < stepReports.Count)
            {
                var report = stepReports[index];
                if (string.Equals(report.Status, "passed", StringComparison.OrdinalIgnoreCase)
                    || report.SeenScenes.Any(seen => ScenarioRunner.SceneMatchesOrProgressesPast(seen, step.TargetScene)))
                {
                    satisfiedScenes.Add(step.TargetScene);
                }
            }
        }

        var missingScenes = requiredScenes.Except(satisfiedScenes, StringComparer.OrdinalIgnoreCase).ToArray();
        var finalExpected = scenario.Steps.LastOrDefault()?.ExpectedResult;
        var finalSceneMatches = string.IsNullOrWhiteSpace(finalExpected)
                                || ScenarioRunner.SceneMatchesOrProgressesPast(finalState.Scene.SceneType, finalExpected);
        var passed = failedSteps == 0 && missingScenes.Length == 0 && finalSceneMatches;
        return new AcceptanceReport(
            scenario.Id,
            passed ? "passed" : "failed",
            totalActions,
            failedSteps,
            finalState.Scene.SceneType,
            finalState.Run.RunId,
            missingScenes,
            finalSceneMatches);
    }

}

public sealed record AcceptanceReport(
    string ScenarioId,
    string Status,
    int TotalActions,
    int FailedActions,
    string FinalScene,
    string? RunId,
    IReadOnlyList<string> MissingRequiredScenes,
    bool FinalSceneMatchesExpected);
