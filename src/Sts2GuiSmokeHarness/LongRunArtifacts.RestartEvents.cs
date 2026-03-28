using System;
using System.IO;

static partial class LongRunArtifacts
{
    public static void RecordAttemptStarted(
        string sessionRoot,
        string attemptId,
        int attemptOrdinal,
        string runId,
        string trustStateAtStart,
        string firstScreenPath)
    {
        AppendNdjson(
            GetRestartEventsPath(sessionRoot),
            new GuiSmokeRestartEvent(
                DateTimeOffset.UtcNow,
                GuiSmokeContractStates.EventNextAttemptStarted,
                Path.GetFileName(sessionRoot),
                attemptId,
                attemptOrdinal,
                runId,
                null,
                null,
                null,
                null,
                null,
                trustStateAtStart,
                firstScreenPath));
        RefreshSessionSummary(sessionRoot);
        RefreshSupervisorState(sessionRoot);
    }

    public static void RecordRunnerLaunchIssued(
        string sessionRoot,
        string attemptId,
        int attemptOrdinal,
        string runId,
        string trustStateAtStart)
    {
        AppendNdjson(
            GetRestartEventsPath(sessionRoot),
            new GuiSmokeRestartEvent(
                DateTimeOffset.UtcNow,
                GuiSmokeContractStates.EventRunnerLaunchIssued,
                Path.GetFileName(sessionRoot),
                attemptId,
                attemptOrdinal,
                runId,
                null,
                null,
                null,
                null,
                null,
                trustStateAtStart,
                null));
        RefreshSessionSummary(sessionRoot);
        RefreshSupervisorState(sessionRoot);
    }

    public static void RecordRunnerBeginRestart(
        string sessionRoot,
        GuiSmokeAttemptResult previousAttempt,
        string nextAttemptId,
        int nextAttemptOrdinal)
    {
        AppendNdjson(
            GetRestartEventsPath(sessionRoot),
            new GuiSmokeRestartEvent(
                DateTimeOffset.UtcNow,
                GuiSmokeContractStates.EventRunnerBeginRestart,
                Path.GetFileName(sessionRoot),
                nextAttemptId,
                nextAttemptOrdinal,
                null,
                previousAttempt.AttemptId,
                previousAttempt.AttemptOrdinal,
                previousAttempt.TerminalCause,
                previousAttempt.LaunchFailed,
                previousAttempt.FailureClass,
                previousAttempt.TrustStateAtStart,
                null));
        RefreshSessionSummary(sessionRoot);
        RefreshSupervisorState(sessionRoot);
    }

    public static void RecordAttemptTerminal(
        string sessionRoot,
        string attemptId,
        int attemptOrdinal,
        string runId,
        string? terminalCause,
        bool launchFailed,
        string? failureClass,
        string trustStateAtStart)
    {
        AppendNdjson(
            GetRestartEventsPath(sessionRoot),
            new GuiSmokeRestartEvent(
                DateTimeOffset.UtcNow,
                GuiSmokeContractStates.EventAttemptTerminal,
                Path.GetFileName(sessionRoot),
                attemptId,
                attemptOrdinal,
                runId,
                null,
                null,
                terminalCause,
                launchFailed,
                failureClass,
                trustStateAtStart,
                null));
        RefreshSessionSummary(sessionRoot);
    }
}
