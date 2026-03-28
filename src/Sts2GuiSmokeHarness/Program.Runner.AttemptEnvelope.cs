using System.Collections.Generic;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;

internal static partial class Program
{
    sealed record AttemptRunScope(
        string RunId,
        string RunRoot,
        string StepsRoot,
        ArtifactRecorder Logger,
        GuiSmokeVideoRecorder AttemptVideo,
        GuiSmokeSessionSceneHistoryIndex SceneHistoryIndex,
        bool IsAuthoritativeFirstAttempt);

    static AttemptRunScope InitializeAttemptRun(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        IReadOnlyDictionary<string, string> options,
        string scenarioId,
        string providerKind,
        LiveExportLayout liveLayout,
        HarnessQueueLayout harnessLayout,
        string sessionId,
        string sessionRoot,
        bool isLongRun,
        string attemptId,
        int attemptOrdinal)
    {
        var runId = isLongRun
            ? $"{sessionId}-attempt-{attemptId}"
            : sessionId;
        var runRoot = isLongRun
            ? Path.Combine(sessionRoot, "attempts", attemptId)
            : sessionRoot;

        if (Directory.Exists(runRoot))
        {
            Directory.Delete(runRoot, recursive: true);
        }

        Directory.CreateDirectory(runRoot);
        var stepsRoot = Path.Combine(runRoot, "steps");
        Directory.CreateDirectory(stepsRoot);

        var logger = new ArtifactRecorder(runRoot);
        SetHarnessLogSink(logger.AppendHumanLog);
        var attemptVideo = GuiSmokeVideoRecorder.Create(
            workspaceRoot,
            options,
            sessionId,
            runId,
            runRoot,
            sessionRoot,
            attemptId,
            "attempt");
        logger.WriteRunManifest(new GuiSmokeRunManifest(
            runId,
            scenarioId,
            providerKind,
            DateTimeOffset.UtcNow,
            workspaceRoot,
            liveLayout.LiveRoot,
            harnessLayout.HarnessRoot,
            configuration.GamePaths.GameDirectory));

        return new AttemptRunScope(
            runId,
            runRoot,
            stepsRoot,
            logger,
            attemptVideo,
            GuiSmokeSessionSceneHistoryIndex.Load(sessionRoot),
            isLongRun && attemptOrdinal == 1);
    }

    static void RecordAttemptStartupStage(
        string sessionRoot,
        bool isAuthoritativeFirstAttempt,
        ref string startupStage,
        string stage,
        string status,
        string? detail = null,
        IReadOnlyDictionary<string, string?>? metadata = null)
    {
        if (!isAuthoritativeFirstAttempt)
        {
            return;
        }

        startupStage = stage;
        LongRunArtifacts.RecordStartupStage(sessionRoot, stage, status, detail, metadata);
    }

    static void RecordAttemptStartupFailure(
        string sessionRoot,
        bool isAuthoritativeFirstAttempt,
        string startupStage,
        string reason,
        IReadOnlyDictionary<string, string?>? metadata = null)
    {
        if (!isAuthoritativeFirstAttempt)
        {
            return;
        }

        LongRunArtifacts.RecordStartupFailure(sessionRoot, startupStage, reason, metadata);
    }

    static GuiSmokeAttemptResult CompleteAttemptRun(
        string scenarioId,
        string providerKind,
        string sessionRoot,
        bool isLongRun,
        string attemptId,
        int attemptOrdinal,
        string trustStateAtStart,
        AttemptRunScope attemptRun,
        int stepIndex,
        int exitCode,
        string status,
        string message,
        bool launchFailed = false,
        string? terminalCause = null,
        string? failureClass = null)
    {
        attemptRun.Logger.CompleteRun(status, message);
        attemptRun.Logger.WriteValidationSummary(attemptRun.RunId);
        attemptRun.AttemptVideo.Complete(
            keepRecording: true,
            completionReason: $"attempt-{status}:{terminalCause ?? failureClass ?? message}");
        if (isLongRun)
        {
            LongRunArtifacts.RecordAttemptTerminal(
                sessionRoot,
                attemptId,
                attemptOrdinal,
                attemptRun.RunId,
                terminalCause,
                launchFailed,
                failureClass,
                trustStateAtStart);
            var selfMetaReview = LongRunArtifacts.WriteAttemptMetaReview(
                sessionRoot,
                attemptId,
                attemptOrdinal,
                attemptRun.RunId,
                status,
                message,
                failureClass);
            attemptRun.Logger.WriteSelfMetaReview(selfMetaReview);
            LongRunArtifacts.WriteSessionArtifacts(
                sessionRoot,
                attemptRun.Logger,
                attemptRun.RunId,
                scenarioId,
                providerKind,
                attemptId,
                attemptOrdinal,
                stepIndex,
                status,
                message,
                terminalCause,
                launchFailed,
                failureClass,
                trustStateAtStart);
        }

        return new GuiSmokeAttemptResult(
            attemptId,
            attemptOrdinal,
            attemptRun.RunId,
            attemptRun.RunRoot,
            exitCode,
            status,
            message,
            stepIndex,
            launchFailed,
            terminalCause,
            failureClass,
            trustStateAtStart);
    }

    static async Task<string> LaunchAttemptWindowAndStartVideoAsync(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        IReadOnlyDictionary<string, string> options,
        string sessionId,
        string runId,
        string sessionRoot,
        string attemptId,
        int attemptOrdinal,
        string trustStateAtStart,
        AttemptRunScope attemptRun,
        string startupStage)
    {
        if (options.ContainsKey("--skip-launch"))
        {
            return startupStage;
        }

        await StopGameProcessesAsync(TimeSpan.FromSeconds(20)).ConfigureAwait(false);
        EnsureGameNotRunning();
        var launchIssuedAt = DateTimeOffset.UtcNow;
        EnsureStartupRuntimeConfig(configuration, sessionId, runId, launchIssuedAt);
        startupStage = "authoritative-attempt-launch-issued";
        await GuiSmokeShared.RunProcessAsync(
            Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
            "/c start \"\" \"steam://rungameid/2868840\"",
            workspaceRoot,
            TimeSpan.FromSeconds(10),
            waitForExit: false).ConfigureAwait(false);
        if (attemptRun.IsAuthoritativeFirstAttempt)
        {
            LongRunArtifacts.RecordRunnerLaunchIssued(sessionRoot, attemptId, attemptOrdinal, runId, trustStateAtStart);
        }

        startupStage = "authoritative-attempt-window-detected";
        await WaitForLiveGameWindowAsync(launchIssuedAt, TimeSpan.FromMinutes(2)).ConfigureAwait(false);
        await MaintainLaunchFocusAsync(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(2)).ConfigureAwait(false);
        await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
        attemptRun.AttemptVideo.TryStart(WindowLocator.TryFindSts2Window());
        return startupStage;
    }
}
