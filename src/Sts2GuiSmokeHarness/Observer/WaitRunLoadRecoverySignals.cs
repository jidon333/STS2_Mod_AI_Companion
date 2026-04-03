using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

readonly record struct WaitRunLoadRecoveryState(
    bool RecentContinueSubmissionPending,
    bool HasExplicitRunSaveCleanupSurface,
    bool HasExplicitAbandonConfirmSurface,
    bool HasFreshRetrySurface,
    bool HasResolvedRunLoadOutcome,
    bool ShouldRetryEnterRun,
    string RetryReason);

static class WaitRunLoadRecoverySignals
{
    public static bool ShouldRetryEnterRunFromWaitRunLoad(ObserverSummary observer)
        => ShouldRetryEnterRunFromWaitRunLoad(observer, Array.Empty<GuiSmokeHistoryEntry>());

    public static bool ShouldRetryEnterRunFromWaitRunLoad(ObserverSummary observer, IReadOnlyList<GuiSmokeHistoryEntry>? history)
        => GetState(observer, history).ShouldRetryEnterRun;

    public static bool ShouldPreferRunSaveCleanupAfterFailedContinue(ObserverSummary observer, IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        if (!IsReturnedMainMenuSurface(observer)
            || MainMenuRunStartObserverSignals.HasAbandonRunConfirmSurface(observer)
            || !MainMenuRunStartObserverSignals.HasExplicitAbandonRunSurface(observer))
        {
            return false;
        }

        return HasFreshRetryAfterContinuePending(history);
    }

    public static WaitRunLoadRecoveryState GetState(ObserverSummary observer, IReadOnlyList<GuiSmokeHistoryEntry>? history = null)
    {
        var transitionState = RootSceneTransitionObserverSignals.TryGetState(observer);
        if (transitionState?.TransitionInProgress == true
            || transitionState?.RootSceneIsRun == true
            || transitionState?.CurrentRunNodePresent == true)
        {
            return new WaitRunLoadRecoveryState(
                RecentContinueSubmissionPending: false,
                HasExplicitRunSaveCleanupSurface: false,
                HasExplicitAbandonConfirmSurface: false,
                HasFreshRetrySurface: false,
                HasResolvedRunLoadOutcome: true,
                ShouldRetryEnterRun: false,
                RetryReason: "None");
        }

        var mainMenuSurfaceVisible = MatchesControlFlowScreen(observer, "main-menu")
                                     || string.Equals(observer.ChoiceExtractorPath, "main-menu", StringComparison.OrdinalIgnoreCase)
                                     || transitionState?.RootSceneIsMainMenu == true;
        if (!mainMenuSurfaceVisible)
        {
            return new WaitRunLoadRecoveryState(
                RecentContinueSubmissionPending: false,
                HasExplicitRunSaveCleanupSurface: false,
                HasExplicitAbandonConfirmSurface: false,
                HasFreshRetrySurface: false,
                HasResolvedRunLoadOutcome: false,
                ShouldRetryEnterRun: false,
                RetryReason: "None");
        }

        var hasExplicitRunSaveCleanupSurface = MainMenuRunStartObserverSignals.HasRunSaveCleanupSurface(observer);
        var hasExplicitAbandonConfirmSurface = MainMenuRunStartObserverSignals.HasAbandonRunConfirmSurface(observer);
        var hasFreshRetrySurface = HasExplicitMainMenuRetrySurface(observer);
        var hasResolvedRunLoadOutcome = HasResolvedRunLoadOutcome(observer);
        var stableReadyMainMenu = IsStableReadyMainMenu(observer);
        var recentContinueSubmission = HasRecentContinueSubmission(history);
        var continuePendingWaitCount = CountPendingWaitsSinceContinueSubmission(history);
        var boundedRetryAfterContinuePending = recentContinueSubmission
                                              && hasFreshRetrySurface
                                              && continuePendingWaitCount >= 10
                                              && !hasResolvedRunLoadOutcome
                                              && !hasExplicitAbandonConfirmSurface;
        var recentContinueSubmissionPending = recentContinueSubmission
                                             && !hasResolvedRunLoadOutcome
                                             && !boundedRetryAfterContinuePending
                                             && !(hasFreshRetrySurface && stableReadyMainMenu)
                                             && !hasExplicitAbandonConfirmSurface;

        if (hasExplicitAbandonConfirmSurface)
        {
            return new WaitRunLoadRecoveryState(
                recentContinueSubmissionPending,
                hasExplicitRunSaveCleanupSurface,
                hasExplicitAbandonConfirmSurface,
                hasFreshRetrySurface,
                hasResolvedRunLoadOutcome,
                ShouldRetryEnterRun: true,
                RetryReason: "AbandonConfirm");
        }

        if (boundedRetryAfterContinuePending)
        {
            return new WaitRunLoadRecoveryState(
                recentContinueSubmissionPending,
                hasExplicitRunSaveCleanupSurface,
                hasExplicitAbandonConfirmSurface,
                hasFreshRetrySurface,
                hasResolvedRunLoadOutcome,
                ShouldRetryEnterRun: true,
                RetryReason: "FreshRetryAfterContinuePending");
        }

        if (recentContinueSubmissionPending)
        {
            return new WaitRunLoadRecoveryState(
                recentContinueSubmissionPending,
                hasExplicitRunSaveCleanupSurface,
                hasExplicitAbandonConfirmSurface,
                hasFreshRetrySurface,
                hasResolvedRunLoadOutcome,
                ShouldRetryEnterRun: false,
                RetryReason: "SubmissionPendingWait");
        }

        if (hasExplicitRunSaveCleanupSurface)
        {
            return new WaitRunLoadRecoveryState(
                recentContinueSubmissionPending,
                hasExplicitRunSaveCleanupSurface,
                hasExplicitAbandonConfirmSurface,
                hasFreshRetrySurface,
                hasResolvedRunLoadOutcome,
                ShouldRetryEnterRun: true,
                RetryReason: "RunSaveCleanup");
        }

        if (hasFreshRetrySurface)
        {
            return new WaitRunLoadRecoveryState(
                recentContinueSubmissionPending,
                hasExplicitRunSaveCleanupSurface,
                hasExplicitAbandonConfirmSurface,
                hasFreshRetrySurface,
                hasResolvedRunLoadOutcome,
                ShouldRetryEnterRun: true,
                RetryReason: "FreshStableMainMenuRetry");
        }

        if (ControlFlowSceneReady(observer) == false
            || !string.Equals(ControlFlowSceneStability(observer), "stable", StringComparison.OrdinalIgnoreCase))
        {
            return new WaitRunLoadRecoveryState(
                recentContinueSubmissionPending,
                hasExplicitRunSaveCleanupSurface,
                hasExplicitAbandonConfirmSurface,
                hasFreshRetrySurface,
                hasResolvedRunLoadOutcome,
                ShouldRetryEnterRun: false,
                RetryReason: "None");
        }

        var shouldRetryEnterRun = MainMenuRunStartObserverSignals.HasMainMenuRunStartSurface(observer)
                                  || observer.CurrentChoices.Any(IsContinueRunLabel);
        return new WaitRunLoadRecoveryState(
            recentContinueSubmissionPending,
            hasExplicitRunSaveCleanupSurface,
            hasExplicitAbandonConfirmSurface,
            hasFreshRetrySurface,
            hasResolvedRunLoadOutcome,
            ShouldRetryEnterRun: shouldRetryEnterRun,
            RetryReason: shouldRetryEnterRun ? "FreshStableMainMenuRetry" : "None");
    }

    private static bool HasResolvedRunLoadOutcome(ObserverSummary observer)
    {
        var transitionState = RootSceneTransitionObserverSignals.TryGetState(observer);
        if (transitionState?.RootSceneIsRun == true
            || transitionState?.CurrentRunNodePresent == true)
        {
            return true;
        }

        if (MatchesControlFlowScreen(observer, "singleplayer-submenu")
            || MatchesControlFlowScreen(observer, "character-select"))
        {
            return true;
        }

        return GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(observer, out _);
    }

    private static bool IsStableReadyMainMenu(ObserverSummary observer)
    {
        return string.Equals(ControlFlowSceneStability(observer), "stable", StringComparison.OrdinalIgnoreCase)
               && ControlFlowSceneReady(observer) != false
               && MatchesControlFlowScreen(observer, "main-menu");
    }

    private static bool HasRecentContinueSubmission(IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        if (history is null || history.Count == 0)
        {
            return false;
        }

        foreach (var entry in history.Reverse())
        {
            if (string.Equals(entry.Phase, GuiSmokePhase.WaitRunLoad.ToString(), StringComparison.OrdinalIgnoreCase)
                && (string.Equals(entry.Action, "retry-enter-run", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            return string.Equals(entry.Phase, GuiSmokePhase.EnterRun.ToString(), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(entry.TargetLabel, "continue", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static int CountPendingWaitsSinceContinueSubmission(IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        if (history is null || history.Count == 0)
        {
            return 0;
        }

        var waitCount = 0;
        foreach (var entry in history.Reverse())
        {
            if (string.Equals(entry.Phase, GuiSmokePhase.WaitRunLoad.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase))
            {
                waitCount++;
                continue;
            }

            if (string.Equals(entry.Phase, GuiSmokePhase.WaitRunLoad.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.Action, "retry-enter-run", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.Equals(entry.Phase, GuiSmokePhase.EnterRun.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.TargetLabel, "continue", StringComparison.OrdinalIgnoreCase))
            {
                return waitCount;
            }

            break;
        }

        return 0;
    }

    private static bool HasExplicitMainMenuRetrySurface(ObserverSummary observer)
    {
        var choiceExtractorPath = observer.ChoiceExtractorPath
                                  ?? (observer.Meta.TryGetValue("choiceExtractorPath", out var publishedChoiceExtractorPath)
                                      ? publishedChoiceExtractorPath
                                      : null)
                                  ?? (observer.Meta.TryGetValue("rawChoiceExtractorPath", out var rawChoiceExtractorPath)
                                      ? rawChoiceExtractorPath
                                      : null);

        return string.Equals(choiceExtractorPath, "main-menu", StringComparison.OrdinalIgnoreCase)
               && IsMainMenuScreenTypeName(observer.Meta.TryGetValue("activeScreenType", out var activeScreenType) ? activeScreenType : null)
               && !MainMenuRunStartObserverSignals.IsLogoAnimationOnlyMainMenu(observer)
               && !MainMenuRunStartObserverSignals.ShouldWaitForStableRunStartSurface(observer)
               && MainMenuRunStartObserverSignals.HasMainMenuRunStartSurface(observer);
    }

    private static bool HasFreshRetryAfterContinuePending(IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        if (history is null || history.Count == 0)
        {
            return false;
        }

        foreach (var entry in history.Reverse())
        {
            if (string.Equals(entry.Phase, GuiSmokePhase.WaitRunLoad.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return string.Equals(entry.Phase, GuiSmokePhase.WaitRunLoad.ToString(), StringComparison.OrdinalIgnoreCase)
                   && string.Equals(entry.Action, "retry-enter-run", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(entry.Metadata, "FreshRetryAfterContinuePending", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool IsReturnedMainMenuSurface(ObserverSummary observer)
    {
        return (MatchesControlFlowScreen(observer, "main-menu")
                || string.Equals(observer.ChoiceExtractorPath, "main-menu", StringComparison.OrdinalIgnoreCase)
                || string.Equals(observer.Meta.TryGetValue("choiceExtractorPath", out var choiceExtractorPath) ? choiceExtractorPath : null, "main-menu", StringComparison.OrdinalIgnoreCase)
                || string.Equals(observer.Meta.TryGetValue("rawChoiceExtractorPath", out var rawChoiceExtractorPath) ? rawChoiceExtractorPath : null, "main-menu", StringComparison.OrdinalIgnoreCase)
                || string.Equals(observer.Meta.TryGetValue("rootSceneIsMainMenu", out var rootSceneIsMainMenu) ? rootSceneIsMainMenu : null, "true", StringComparison.OrdinalIgnoreCase))
               && (string.Equals(observer.Meta.TryGetValue("terminalRunBoundary", out var terminalRunBoundary) ? terminalRunBoundary : null, "true", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(observer.Meta.TryGetValue("mainMenuReturnDetected", out var mainMenuReturnDetected) ? mainMenuReturnDetected : null, "true", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsMainMenuScreenTypeName(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NMainMenu", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsContinueRunLabel(string? label)
    {
        return string.Equals(label, "Continue", StringComparison.OrdinalIgnoreCase)
               || string.Equals(label, "\uACC4\uC18D", StringComparison.OrdinalIgnoreCase);
    }
}
