using System.Diagnostics;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Harness.Actions;
using Sts2ModKit.Core.Configuration;

namespace Sts2AiCompanion.Harness.Recovery;

public sealed class RecoveryManager
{
    private readonly IHarnessActionExecutor? _actionExecutor;
    private readonly ScaffoldConfiguration _configuration;

    public RecoveryManager(ScaffoldConfiguration configuration, IHarnessActionExecutor? actionExecutor = null)
    {
        _configuration = configuration;
        _actionExecutor = actionExecutor;
    }

    public async Task<RecoveryAttempt> TryRecoverAsync(string reason, CompanionState state, CancellationToken cancellationToken)
    {
        if (reason.Contains("game-not-running", StringComparison.OrdinalIgnoreCase)
            && _configuration.Harness.AutoLaunchGame)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c start \"\" \"steam://rungameid/2868840\"",
                UseShellExecute = true,
                CreateNoWindow = true,
            });
            return new RecoveryAttempt(true, null, null, "relaunch-game");
        }

        if (_actionExecutor is null)
        {
            return new RecoveryAttempt(false, null, null, "action-executor-unavailable");
        }

        if (reason.Contains("scene-timeout", StringComparison.OrdinalIgnoreCase))
        {
            var action = HarnessAction.Create("cancel", "recovery:scene-timeout", timeoutMs: 10_000);
            var result = await _actionExecutor.ExecuteAsync(
                    action,
                    state,
                    cancellationToken)
                .ConfigureAwait(false);
            return new RecoveryAttempt(
                string.Equals(result.Status, "ok", StringComparison.OrdinalIgnoreCase),
                action,
                result,
                "cancel");
        }

        if (reason.Contains("bridge-stalled", StringComparison.OrdinalIgnoreCase))
        {
            var normalizedScene = (state.Scene.SceneType ?? string.Empty).Trim();
            var isOverlayScene =
                string.Equals(normalizedScene, "blocking-overlay", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedScene, "feedback-overlay", StringComparison.OrdinalIgnoreCase);
            var isCombatScene = string.Equals(normalizedScene, "combat", StringComparison.OrdinalIgnoreCase);
            var handUnavailable =
                reason.Contains("player-hand-unavailable", StringComparison.OrdinalIgnoreCase)
                || reason.Contains("hand-holder-unavailable", StringComparison.OrdinalIgnoreCase);
            var hasOverlayOnlyChoices = state.Choices.List.Count > 0
                                        && state.Choices.List.All(choice => IsOverlayChoice(choice.Label));
            var hasExplicitDismissChoice = state.Choices.List.Any(choice =>
            {
                var label = (choice.Label ?? string.Empty).Trim();
                return string.Equals(label, "Dismisser", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(label, "Continue", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(label, "Cancel", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(label, "Close", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(label, "Back", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(label, "OK", StringComparison.OrdinalIgnoreCase);
            });

            var shouldCancel = isOverlayScene
                               || hasOverlayOnlyChoices
                               || (hasExplicitDismissChoice && !isCombatScene);
            var action = shouldCancel
                ? HarnessAction.Create("cancel", "recovery:bridge-stalled", targetLabel: "__cancel__", timeoutMs: 10_000, retryBudget: 1)
                : HarnessAction.Create("noop", handUnavailable ? "recovery:combat-wait" : "recovery:bridge-stalled", timeoutMs: 10_000);
            var result = await _actionExecutor.ExecuteAsync(
                    action,
                    state,
                    cancellationToken)
                .ConfigureAwait(false);
            return new RecoveryAttempt(
                string.Equals(result.Status, "ok", StringComparison.OrdinalIgnoreCase),
                action,
                result,
                shouldCancel ? "cancel" : "noop");
        }

        return new RecoveryAttempt(false, null, null, "unsupported-recovery");
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
}

public sealed record RecoveryAttempt(
    bool Recovered,
    HarnessAction? Action,
    HarnessActionResult? Result,
    string Strategy);
