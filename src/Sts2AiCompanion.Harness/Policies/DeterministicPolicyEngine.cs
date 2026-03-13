using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Harness.Scenarios;
using Sts2ModKit.Core.Configuration;

namespace Sts2AiCompanion.Harness.Policies;

public sealed class DeterministicPolicyEngine
{
    private readonly int _defaultTimeoutMs;
    private readonly int _combatTimeoutMs;

    public DeterministicPolicyEngine(ScaffoldConfiguration? configuration = null)
    {
        var configuredTimeout = configuration?.Harness.StepTimeoutMs ?? HarnessOptions.Defaults.StepTimeoutMs;
        _defaultTimeoutMs = Math.Max(configuredTimeout, 1_000);
        _combatTimeoutMs = Math.Max(Math.Min(_defaultTimeoutMs, 30_000), 10_000);
    }

    public HarnessAction ResolveAction(ScenarioStep step, CompanionState state)
    {
        if (string.Equals(step.ActionKind, "combat-basic", StringComparison.OrdinalIgnoreCase))
        {
            return ResolveCombatAction(step, state);
        }

        var reason = $"scenario-step:{step.Name}|scene:{state.Scene.SceneType}";
        var metadata = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["targetScene"] = step.TargetScene,
            ["expectedResult"] = step.ExpectedResult,
            ["stepName"] = step.Name,
        };
        var retryBudget = step.ActionKind.ToLowerInvariant() switch
        {
            "click_button" => 3,
            "choose_map_node" => 2,
            "choose_reward" => 2,
            "confirm" => 2,
            "cancel" => 2,
            _ => 1,
        };

        return new HarnessAction(
            Guid.NewGuid().ToString("N"),
            step.ActionKind,
            DateTimeOffset.UtcNow,
            step.Name,
            step.TargetLabel,
            null,
            _defaultTimeoutMs,
            retryBudget,
            "test-only",
            reason,
            step.ExpectedResult,
            metadata);
    }

    private HarnessAction ResolveCombatAction(ScenarioStep step, CompanionState state)
    {
        var playableChoices = state.Choices.List
            .Where(choice => !string.IsNullOrWhiteSpace(choice.Label))
            .Where(choice =>
                choice.Kind.Contains("card", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("card", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("strike", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("defend", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("bash", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("강타", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("타격", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("수비", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var hasHandEvidence = !string.IsNullOrWhiteSpace(state.Combat.HandSummary);
        var hasEndTurnChoice = state.Choices.List.Any(choice =>
        {
            if (string.IsNullOrWhiteSpace(choice.Label))
            {
                return false;
            }

            return choice.Label.Contains("end turn", StringComparison.OrdinalIgnoreCase)
                   || choice.Label.Contains("turn end", StringComparison.OrdinalIgnoreCase)
                   || choice.Label.Contains("턴 종료", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(choice.Kind, "end_turn", StringComparison.OrdinalIgnoreCase);
        });
        var energy = state.Player.Energy.GetValueOrDefault();

        var chosenKind = playableChoices.Length > 0 || hasHandEvidence
            ? "click_card"
            : hasEndTurnChoice || (state.Combat.Turn is > 0 && energy <= 0)
                ? "end_turn"
                : "noop";
        var targetLabel = chosenKind switch
        {
            "click_card" => "__first_playable__",
            "end_turn" => "__end_turn__",
            _ => "__wait__",
        };
        var timeoutMs = string.Equals(chosenKind, "noop", StringComparison.Ordinal)
            ? Math.Min(_combatTimeoutMs, 5_000)
            : _combatTimeoutMs;
        var metadata = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["targetScene"] = step.TargetScene,
            ["expectedResult"] = step.ExpectedResult,
            ["stepName"] = step.Name,
            ["strategy"] = "combat-basic",
        };

        return new HarnessAction(
            Guid.NewGuid().ToString("N"),
            chosenKind,
            DateTimeOffset.UtcNow,
            step.Name,
            targetLabel,
            null,
            timeoutMs,
            1,
            "test-only",
            $"scenario-step:{step.Name}|scene:{state.Scene.SceneType}|combat-basic",
            step.ExpectedResult,
            metadata);
    }
}
