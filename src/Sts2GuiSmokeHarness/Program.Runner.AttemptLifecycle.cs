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
        var attemptRun = InitializeAttemptRun(
            configuration,
            workspaceRoot,
            options,
            scenarioId,
            providerKind,
            liveLayout,
            harnessLayout,
            sessionId,
            sessionRoot,
            isLongRun,
            attemptId,
            attemptOrdinal);
        using var attemptVideo = attemptRun.AttemptVideo;
        var stepIndex = 0;
        var startupStage = "authoritative-attempt-started";

        if (attemptRun.IsAuthoritativeFirstAttempt)
        {
            RecordAttemptStartupStage(
                sessionRoot,
                attemptRun.IsAuthoritativeFirstAttempt,
                ref startupStage,
                "authoritative-attempt-started",
                "finished",
                attemptRun.RunRoot);
        }

        try
        {
            startupStage = await LaunchAttemptWindowAndStartVideoAsync(
                configuration,
                workspaceRoot,
                options,
                sessionId,
                attemptRun.RunId,
                sessionRoot,
                attemptId,
                attemptOrdinal,
                trustStateAtStart,
                attemptRun,
                startupStage).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            RecordAttemptStartupFailure(
                sessionRoot,
                attemptRun.IsAuthoritativeFirstAttempt,
                startupStage,
                $"{exception.GetType().Name}: {exception.Message}");
            attemptRun.Logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                GuiSmokePhase.WaitMainMenu.ToString(),
                $"launch-failed: {exception.Message}",
                null,
                null,
                null));
            return CompleteAttemptRun(
                scenarioId,
                providerKind,
                sessionRoot,
                isLongRun,
                attemptId,
                attemptOrdinal,
                trustStateAtStart,
                attemptRun,
                stepIndex,
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
                attemptRun.RunId,
                attemptRun.StepsRoot,
                attemptRun.SceneHistoryIndex,
                attemptRun.Logger,
                attemptRun.IsAuthoritativeFirstAttempt,
                (stage, status, detail) => RecordAttemptStartupStage(sessionRoot, attemptRun.IsAuthoritativeFirstAttempt, ref startupStage, stage, status, detail),
                reason => RecordAttemptStartupFailure(sessionRoot, attemptRun.IsAuthoritativeFirstAttempt, startupStage, reason),
                (currentStepIndex, exitCode, status, message, launchFailed, terminalCause, failureClass) =>
                {
                    stepIndex = currentStepIndex;
                    return CompleteAttemptRun(
                        scenarioId,
                        providerKind,
                        sessionRoot,
                        isLongRun,
                        attemptId,
                        attemptOrdinal,
                        trustStateAtStart,
                        attemptRun,
                        stepIndex,
                        exitCode,
                        status,
                        message,
                        launchFailed,
                        terminalCause,
                        failureClass);
                }).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            attemptRun.Logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                "runner",
                $"unexpected-exception: {exception.Message}",
                null,
                null,
                null));
            return CompleteAttemptRun(
                scenarioId,
                providerKind,
                sessionRoot,
                isLongRun,
                attemptId,
                attemptOrdinal,
                trustStateAtStart,
                attemptRun,
                stepIndex,
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
