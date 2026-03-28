using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModAiCompanion.Mod;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;
using static GuiSmokeChoicePrimitiveSupport;

internal static partial class Program
{
    static async Task<GuiSmokeAttemptResult> RunAttemptAsync(
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
        int attemptOrdinal,
        DateTimeOffset sessionDeadline,
        string trustStateAtStart,
        int? maxSteps)
    {
        var runId = isLongRun
            ? $"{sessionId}-attempt-{attemptId}"
            : sessionId;
        var keepVideoOnSuccess = options.ContainsKey("--keep-video-on-success");
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
        using var attemptVideo = GuiSmokeVideoRecorder.Create(
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
        var sceneHistoryIndex = GuiSmokeSessionSceneHistoryIndex.Load(sessionRoot);
        var stepIndex = 0;
        var startupStage = "authoritative-attempt-started";
        var isAuthoritativeFirstAttempt = isLongRun && attemptOrdinal == 1;

        void RecordAttemptStartupStage(
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

        void RecordAttemptStartupFailure(
            string reason,
            IReadOnlyDictionary<string, string?>? metadata = null)
        {
            if (!isAuthoritativeFirstAttempt)
            {
                return;
            }

            LongRunArtifacts.RecordStartupFailure(sessionRoot, startupStage, reason, metadata);
        }

        if (isAuthoritativeFirstAttempt)
        {
            RecordAttemptStartupStage("authoritative-attempt-started", "finished", runRoot);
        }

        GuiSmokeAttemptResult CompleteAttempt(
            int exitCode,
            string status,
            string message,
            bool launchFailed = false,
            string? terminalCause = null,
            string? failureClass = null)
        {
            logger.CompleteRun(status, message);
            logger.WriteValidationSummary(runId);
            var keepVideo = true;
            attemptVideo.Complete(
                keepVideo,
                $"attempt-{status}:{terminalCause ?? failureClass ?? message}");
            if (isLongRun)
            {
                LongRunArtifacts.RecordAttemptTerminal(
                    sessionRoot,
                    attemptId,
                    attemptOrdinal,
                    runId,
                    terminalCause,
                    launchFailed,
                    failureClass,
                    trustStateAtStart);
                var selfMetaReview = LongRunArtifacts.WriteAttemptMetaReview(sessionRoot, attemptId, attemptOrdinal, runId, status, message, failureClass);
                logger.WriteSelfMetaReview(selfMetaReview);
                LongRunArtifacts.WriteSessionArtifacts(
                    sessionRoot,
                    logger,
                    runId,
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
                runId,
                runRoot,
                exitCode,
                status,
                message,
                stepIndex,
                launchFailed,
                terminalCause,
                failureClass,
                trustStateAtStart);
        }

        try
        {
            if (!options.ContainsKey("--skip-launch"))
            {
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
                if (isLongRun)
                {
                    LongRunArtifacts.RecordRunnerLaunchIssued(sessionRoot, attemptId, attemptOrdinal, runId, trustStateAtStart);
                }

                startupStage = "authoritative-attempt-window-detected";
                await WaitForLiveGameWindowAsync(launchIssuedAt, TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                await MaintainLaunchFocusAsync(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
                attemptVideo.TryStart(WindowLocator.TryFindSts2Window());
            }
        }
        catch (Exception exception)
        {
            RecordAttemptStartupFailure($"{exception.GetType().Name}: {exception.Message}");
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                GuiSmokePhase.WaitMainMenu.ToString(),
                $"launch-failed: {exception.Message}",
                null,
                null,
                null));
            return CompleteAttempt(
                1,
                "failed",
                $"launch-failed: {exception.Message}",
                launchFailed: true,
                terminalCause: "launch-failed",
                failureClass: "launch-runtime-noise");
        }

        try
        {
            return await RunAttemptLoopAsync(
                workspaceRoot,
                options,
                scenarioId,
                providerKind,
                liveLayout,
                harnessLayout,
                sessionRoot,
                isLongRun,
                attemptId,
                attemptOrdinal,
                sessionDeadline,
                trustStateAtStart,
                maxSteps,
                runId,
                stepsRoot,
                sceneHistoryIndex,
                logger,
                reason => RecordAttemptStartupFailure(reason),
                (currentStepIndex, exitCode, status, message, launchFailed, terminalCause, failureClass) =>
                {
                    stepIndex = currentStepIndex;
                    return CompleteAttempt(exitCode, status, message, launchFailed, terminalCause, failureClass);
                }).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                "runner",
                $"unexpected-exception: {exception.Message}",
                null,
                null,
                null));
            return CompleteAttempt(
                1,
                "failed",
                $"unexpected-exception: {exception.Message}",
                terminalCause: "unexpected-exception",
                failureClass: "generic-recovery-failure");
        }
    }

    static int? TryGetPositiveIntOption(IReadOnlyDictionary<string, string> options, string key)
    {
        if (!options.TryGetValue(key, out var raw)
            || !int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            || parsed <= 0)
        {
            return null;
        }

        return parsed;
    }
}
