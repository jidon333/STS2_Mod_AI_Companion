using System.Text.Json;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Harness.Actions;
using Sts2AiCompanion.Harness.Evaluation;
using Sts2AiCompanion.Harness.Policies;
using Sts2AiCompanion.Harness.Recovery;
using Sts2AiCompanion.Harness.State;
using Sts2ModKit.Core.Configuration;

namespace Sts2AiCompanion.Harness.Scenarios;

public sealed class ScenarioRunner
{
    private static readonly JsonSerializerOptions ScenarioJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly LiveCompanionStateSource _stateSource;
    private readonly DeterministicPolicyEngine _policyEngine;
    private readonly IHarnessActionExecutor _actionExecutor;
    private readonly RecoveryManager _recoveryManager;
    private readonly AcceptanceEvaluator _acceptanceEvaluator;
    private readonly ScaffoldConfiguration _configuration;

    public ScenarioRunner(
        LiveCompanionStateSource stateSource,
        DeterministicPolicyEngine policyEngine,
        IHarnessActionExecutor actionExecutor,
        RecoveryManager recoveryManager,
        AcceptanceEvaluator acceptanceEvaluator,
        ScaffoldConfiguration configuration)
    {
        _stateSource = stateSource;
        _policyEngine = policyEngine;
        _actionExecutor = actionExecutor;
        _recoveryManager = recoveryManager;
        _acceptanceEvaluator = acceptanceEvaluator;
        _configuration = configuration;
    }

    public async Task<ScenarioRunResult> RunAsync(string scenarioPath, CompanionState initialState, CancellationToken cancellationToken)
    {
        var scenario = JsonSerializer.Deserialize<ScenarioDefinition>(
                           await File.ReadAllTextAsync(scenarioPath, cancellationToken).ConfigureAwait(false),
                           ScenarioJsonOptions)
            ?? throw new InvalidOperationException($"Scenario could not be parsed: {scenarioPath}");
        scenario = NormalizeScenario(scenario, scenarioPath);

        var actionLog = new List<HarnessAction>();
        var resultLog = new List<HarnessActionResult>();
        var stepReports = new List<ScenarioStepReport>();
        var currentState = initialState;

        foreach (var step in scenario.Steps)
        {
            try
            {
                var seenScenes = new List<string>();
                if (stepReports.Count > 0 || string.IsNullOrWhiteSpace(currentState.Scene.SceneType)
                    || string.Equals(currentState.Scene.SceneType, "unknown", StringComparison.OrdinalIgnoreCase))
                {
                    currentState = await _stateSource.ReadAsync(cancellationToken).ConfigureAwait(false);
                }

                if (!string.IsNullOrWhiteSpace(currentState.Scene.SceneType))
                {
                    seenScenes.Add(currentState.Scene.SceneType);
                }

                if (CanSkipStep(step, currentState.Scene.SceneType))
                {
                    stepReports.Add(new ScenarioStepReport(
                        step.Name,
                        "passed",
                        seenScenes,
                        Array.Empty<HarnessAction>(),
                        Array.Empty<HarnessActionResult>(),
                        "already-at-or-beyond-expected-scene"));
                    continue;
                }

                currentState = await WaitForSceneAsync(step.TargetScene, seenScenes, cancellationToken).ConfigureAwait(false);
                if (!SceneMatches(currentState.Scene.SceneType, step.TargetScene))
                {
                    var recovery = await _recoveryManager.TryRecoverAsync("scene-timeout", currentState, cancellationToken).ConfigureAwait(false);
                    var recoveryActions = recovery.Action is not null ? new[] { recovery.Action } : Array.Empty<HarnessAction>();
                    var recoveryResults = recovery.Result is not null ? new[] { recovery.Result } : Array.Empty<HarnessActionResult>();
                    if (recovery.Action is not null)
                    {
                        actionLog.Add(recovery.Action);
                    }

                    if (recovery.Result is not null)
                    {
                        resultLog.Add(recovery.Result);
                    }

                    stepReports.Add(new ScenarioStepReport(
                        step.Name,
                        "failed",
                        seenScenes,
                        recoveryActions,
                        recoveryResults,
                        recovery.Recovered ? "scene-timeout-recovered-unsuccessfully" : $"scene-timeout:{recovery.Strategy}"));
                    if (!recovery.Recovered)
                    {
                        break;
                    }

                    currentState = await WaitForSceneAsync(step.TargetScene, seenScenes, cancellationToken).ConfigureAwait(false);
                    if (!SceneMatches(currentState.Scene.SceneType, step.TargetScene))
                    {
                        break;
                    }
                }

                var stepActions = new List<HarnessAction>();
                var stepResults = new List<HarnessActionResult>();
                var stepStatus = "failed";
                string? failureReason = null;
                var stepDeadline = DateTimeOffset.UtcNow.AddMilliseconds(Math.Max(_configuration.Harness.StepTimeoutMs, 1_000));
                var attemptCount = 0;
                var maxAttempts = GetMaxAttempts(step);

                while (true)
                {
                    if (!SceneMatchesOrProgressesPast(currentState.Scene.SceneType, step.TargetScene))
                    {
                        currentState = await WaitForSceneAsync(
                                step.TargetScene,
                                seenScenes,
                                cancellationToken,
                                timeoutMsOverride: Math.Min(_configuration.Harness.StepTimeoutMs, 5_000))
                            .ConfigureAwait(false);
                        if (!SceneMatchesOrProgressesPast(currentState.Scene.SceneType, step.TargetScene))
                        {
                            failureReason = $"scene-regressed:{currentState.Scene.SceneType}";
                            break;
                        }
                    }

                    if (string.Equals(step.ActionKind, "combat-basic", StringComparison.OrdinalIgnoreCase))
                    {
                        currentState = await WaitForCombatReadyAsync(currentState, seenScenes, stepDeadline, cancellationToken)
                            .ConfigureAwait(false);
                        if (!IsCombatReady(currentState))
                        {
                            failureReason = $"combat-not-ready:{currentState.Scene.SceneType}";
                            break;
                        }
                    }

                    var action = _policyEngine.ResolveAction(step, currentState);
                    attemptCount += 1;
                    actionLog.Add(action);
                    stepActions.Add(action);

                    var result = await _actionExecutor.ExecuteAsync(action, currentState, cancellationToken).ConfigureAwait(false);
                    resultLog.Add(result);
                    stepResults.Add(result);

                    if (!string.Equals(result.Status, "ok", StringComparison.OrdinalIgnoreCase))
                    {
                        currentState = await _stateSource.ReadAsync(cancellationToken).ConfigureAwait(false);
                        if (result.Recoverable && attemptCount < maxAttempts && DateTimeOffset.UtcNow < stepDeadline)
                        {
                            var recovery = await _recoveryManager
                                .TryRecoverAsync($"bridge-stalled:{result.FailureKind}", currentState, cancellationToken)
                                .ConfigureAwait(false);
                            if (recovery.Action is not null)
                            {
                                actionLog.Add(recovery.Action);
                                stepActions.Add(recovery.Action);
                            }

                            if (recovery.Result is not null)
                            {
                                resultLog.Add(recovery.Result);
                                stepResults.Add(recovery.Result);
                            }

                            failureReason = recovery.Recovered ? "bridge-stalled-after-recovery" : $"bridge-stalled:{recovery.Strategy}";
                            if (!recovery.Recovered)
                            {
                                break;
                            }

                            currentState = await _stateSource.ReadAsync(cancellationToken).ConfigureAwait(false);
                            continue;
                        }

                        failureReason = result.FailureKind ?? "action-failed";
                        break;
                    }

                    var expectedWaitBudgetMs = step.ActionKind.ToLowerInvariant() switch
                    {
                        "combat-basic" => Math.Max(action.TimeoutMs, 1_000),
                        "choose_map_node" => Math.Max(action.TimeoutMs, 15_000),
                        "choose_reward" => Math.Max(action.TimeoutMs, 8_000),
                        _ => Math.Max(Math.Min(action.TimeoutMs, 5_000), 1_000),
                    };
                    currentState = await WaitForSceneAsync(
                            step.ExpectedResult,
                            seenScenes,
                            cancellationToken,
                            expectedWaitBudgetMs,
                            acceptProgression: true)
                        .ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(step.ExpectedResult) || SceneMatchesOrProgressesPast(currentState.Scene.SceneType, step.ExpectedResult))
                    {
                        stepStatus = "passed";
                        failureReason = null;
                        break;
                    }

                    failureReason = $"expected-scene:{step.ExpectedResult} actual:{currentState.Scene.SceneType}";
                    var retryLimit = string.Equals(step.ActionKind, "combat-basic", StringComparison.OrdinalIgnoreCase)
                        ? maxAttempts
                        : Math.Min(maxAttempts, Math.Max(action.RetryBudget + 1, 1));
                    if (attemptCount < retryLimit && DateTimeOffset.UtcNow < stepDeadline)
                    {
                        continue;
                    }

                    if (!ShouldRepeatAction(step, currentState, stepDeadline))
                    {
                        break;
                    }
                }

                stepReports.Add(new ScenarioStepReport(step.Name, stepStatus, seenScenes, stepActions, stepResults, failureReason));
                if (!string.Equals(stepStatus, "passed", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Scenario step '{step.Name}' failed while targeting '{step.TargetScene}' and expecting '{step.ExpectedResult}': {exception.Message}",
                    exception);
            }
        }

        var evaluation = _acceptanceEvaluator.Evaluate(scenario, currentState, stepReports);
        return new ScenarioRunResult(scenario, actionLog, resultLog, stepReports, evaluation, currentState);
    }

    private static bool ShouldRepeatAction(ScenarioStep step, CompanionState currentState, DateTimeOffset stepDeadline)
    {
        if (DateTimeOffset.UtcNow >= stepDeadline)
        {
            return false;
        }

        if (string.Equals(step.ActionKind, "combat-basic", StringComparison.OrdinalIgnoreCase))
        {
            return !SceneMatchesOrProgressesPast(currentState.Scene.SceneType, step.ExpectedResult);
        }

        return false;
    }

    private static int GetMaxAttempts(ScenarioStep step)
    {
        return step.ActionKind.ToLowerInvariant() switch
        {
            "click_button" => 5,
            "choose_map_node" => 4,
            "choose_reward" => 4,
            "combat-basic" => 20,
            _ => 3,
        };
    }

    private async Task<CompanionState> WaitForSceneAsync(
        string? expectedScene,
        ICollection<string> seenScenes,
        CancellationToken cancellationToken,
        int? timeoutMsOverride = null,
        bool acceptProgression = false)
    {
        if (string.IsNullOrWhiteSpace(expectedScene))
        {
            return await _stateSource.ReadAsync(cancellationToken).ConfigureAwait(false);
        }

        var timeout = TimeSpan.FromMilliseconds(Math.Max(timeoutMsOverride ?? _configuration.Harness.StepTimeoutMs, 1_000));
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        CompanionState? lastState = null;
        DateTimeOffset? matchedSince = null;
        var stabilityWindow = TimeSpan.FromMilliseconds(GetSceneStabilityWindowMs(expectedScene));
        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lastState = await _stateSource.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(lastState.Scene.SceneType))
            {
                seenScenes.Add(lastState.Scene.SceneType);
            }

            if (SceneMatches(lastState.Scene.SceneType, expectedScene))
            {
                matchedSince ??= DateTimeOffset.UtcNow;
                if (DateTimeOffset.UtcNow - matchedSince.Value >= stabilityWindow)
                {
                    return lastState;
                }
            }
            else if (acceptProgression && SceneMatchesOrProgressesPast(lastState.Scene.SceneType, expectedScene))
            {
                return lastState;
            }
            else
            {
                matchedSince = null;
            }

            await Task.Delay(Math.Max(_configuration.Harness.PollIntervalMs, 250), cancellationToken).ConfigureAwait(false);
        }

        return lastState ?? CompanionState.CreateUnknown();
    }

    private async Task<CompanionState> WaitForCombatReadyAsync(
        CompanionState initialState,
        ICollection<string> seenScenes,
        DateTimeOffset deadline,
        CancellationToken cancellationToken)
    {
        var currentState = initialState;
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (IsCombatReady(currentState))
            {
                return currentState;
            }

            await Task.Delay(Math.Max(_configuration.Harness.PollIntervalMs, 250), cancellationToken).ConfigureAwait(false);
            currentState = await _stateSource.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(currentState.Scene.SceneType))
            {
                seenScenes.Add(currentState.Scene.SceneType);
            }

            if (!SceneMatchesOrProgressesPast(currentState.Scene.SceneType, "combat"))
            {
                return currentState;
            }
        }

        return currentState;
    }

    private static bool IsCombatReady(CompanionState state)
    {
        if (!string.Equals(NormalizeScene(state.Scene.SceneType), "combat", StringComparison.Ordinal))
        {
            return false;
        }

        if (HasOverlayChoiceNoise(state))
        {
            return false;
        }

        if (state.Combat.Turn is > 0)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(state.Combat.HandSummary))
        {
            return true;
        }

        if (state.Choices.List.Count == 0)
        {
            return false;
        }

        return state.Choices.List.Any(choice => IsLikelyCombatChoice(choice.Label));
    }

    private static bool HasOverlayChoiceNoise(CompanionState state)
    {
        if (string.Equals(state.Scene.SceneType, "blocking-overlay", StringComparison.OrdinalIgnoreCase)
            || string.Equals(state.Scene.SceneType, "feedback-overlay", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return state.Choices.List.Count > 0
               && state.Choices.List.All(choice => IsOverlayChoice(choice.Label));
    }

    private static bool IsOverlayChoice(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        var normalized = label.Trim();
        return normalized.Equals("Dismisser", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("Exclaim", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("Question", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("BackButton", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("Send!", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLikelyCombatChoice(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        var normalized = label.Trim();
        if (IsOverlayChoice(normalized))
        {
            return false;
        }

        return normalized.Contains("strike", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("defend", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("bash", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("card", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("강", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("방", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("타", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool SceneMatchesOrProgressesPast(string? actual, string? expected)
    {
        var normalizedActual = NormalizeScene(actual);
        var normalizedExpected = NormalizeScene(expected);
        if (string.Equals(normalizedActual, normalizedExpected, StringComparison.Ordinal))
        {
            return true;
        }

        return SceneProgression.TryGetValue(normalizedActual, out var actualRank)
               && SceneProgression.TryGetValue(normalizedExpected, out var expectedRank)
               && actualRank >= expectedRank;
    }

    internal static bool SceneMatches(string? actual, string? expected)
    {
        return string.Equals(NormalizeScene(actual), NormalizeScene(expected), StringComparison.Ordinal);
    }

    private static bool CanSkipStep(ScenarioStep step, string? currentScene)
    {
        if (RequiresExplicitExecution(step.ActionKind))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(step.ExpectedResult))
        {
            return false;
        }

        if (SceneMatches(step.TargetScene, step.ExpectedResult))
        {
            return false;
        }

        return SceneMatchesOrProgressesPast(currentScene, step.ExpectedResult);
    }

    private static bool RequiresExplicitExecution(string? actionKind)
    {
        return actionKind?.Trim().ToLowerInvariant() switch
        {
            "choose_map_node" => true,
            "choose_reward" => true,
            _ => false,
        };
    }

    private static int GetSceneStabilityWindowMs(string? scene)
    {
        return NormalizeScene(scene) switch
        {
            "main-menu" => 1_500,
            "singleplayer-submenu" => 1_000,
            "character-select" => 1_000,
            "map" => 1_500,
            "combat" => 1_000,
            "rewards" => 1_000,
            _ => 750,
        };
    }

    private static string NormalizeScene(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "reward" => "rewards",
            "rest" => "rest-site",
            _ => normalized ?? string.Empty,
        };
    }

    private static readonly IReadOnlyDictionary<string, int> SceneProgression = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["startup"] = 0,
        ["main-menu"] = 1,
        ["singleplayer-submenu"] = 2,
        ["character-select"] = 3,
        ["map"] = 4,
        ["combat"] = 5,
        ["rewards"] = 6,
    };

    private static ScenarioDefinition NormalizeScenario(ScenarioDefinition scenario, string scenarioPath)
    {
        if (string.IsNullOrWhiteSpace(scenario.Id))
        {
            throw new InvalidOperationException($"Scenario id is required: {scenarioPath}");
        }

        var steps = scenario.Steps?
            .Where(step => step is not null)
            .Select((step, index) => NormalizeStep(step!, index, scenarioPath))
            .ToArray()
            ?? Array.Empty<ScenarioStep>();

        if (steps.Length == 0)
        {
            throw new InvalidOperationException($"Scenario must contain at least one step: {scenarioPath}");
        }

        return scenario with
        {
            Description = scenario.Description ?? string.Empty,
            Steps = steps,
        };
    }

    private static ScenarioStep NormalizeStep(ScenarioStep step, int index, string scenarioPath)
    {
        var name = string.IsNullOrWhiteSpace(step.Name) ? $"step-{index + 1}" : step.Name.Trim();
        if (string.IsNullOrWhiteSpace(step.ActionKind))
        {
            throw new InvalidOperationException($"Scenario step '{name}' is missing actionKind: {scenarioPath}");
        }

        return step with
        {
            Name = name,
            TargetScene = step.TargetScene?.Trim() ?? string.Empty,
            ActionKind = step.ActionKind.Trim(),
            TargetLabel = string.IsNullOrWhiteSpace(step.TargetLabel) ? null : step.TargetLabel.Trim(),
            ExpectedResult = string.IsNullOrWhiteSpace(step.ExpectedResult) ? null : step.ExpectedResult.Trim(),
        };
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
    IReadOnlyList<ScenarioStepReport> Steps,
    AcceptanceReport Evaluation,
    CompanionState FinalState);

public sealed record ScenarioStepReport(
    string Name,
    string Status,
    IReadOnlyList<string> SeenScenes,
    IReadOnlyList<HarnessAction> Actions,
    IReadOnlyList<HarnessActionResult> Results,
    string? FailureReason);
