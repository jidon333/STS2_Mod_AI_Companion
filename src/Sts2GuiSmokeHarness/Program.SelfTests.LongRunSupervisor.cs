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
    private static void RunLongRunSupervisorSelfTests()
    {
        var longRunSessionRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-long-run-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(longRunSessionRoot);
            LongRunArtifacts.InitializeSessionArtifacts(longRunSessionRoot, "self-test-session", "boot-to-long-run", "headless");
            LongRunArtifacts.UpdatePrevalidation(
                longRunSessionRoot,
                gameStoppedBeforeDeploy: true,
                modsPayloadReconciled: true,
                deployIdentityVerified: true,
                manualCleanBootVerified: true);
            LongRunArtifacts.UpdateRunnerSessionState(longRunSessionRoot, GuiSmokeContractStates.SessionCollecting);

            var attemptOneRunRoot = Path.Combine(longRunSessionRoot, "attempts", "0001");
            Directory.CreateDirectory(Path.Combine(attemptOneRunRoot, "steps"));
            var attemptOneLogger = new ArtifactRecorder(attemptOneRunRoot);
            var attemptOneManifest = new GuiSmokeRunManifest(
                "self-test-session-attempt-0001",
                "boot-to-long-run",
                "headless",
                DateTimeOffset.UtcNow.AddMinutes(-2),
                "workspace",
                "live",
                "harness",
                "game")
            {
                Status = "completed",
                ResultMessage = "player-defeated",
                CompletedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            };
            File.WriteAllText(Path.Combine(attemptOneRunRoot, "run.json"), JsonSerializer.Serialize(attemptOneManifest, GuiSmokeShared.JsonOptions));
            var attemptOneValidation = new GuiSmokeValidationSummary(
                "self-test-session-attempt-0001",
                1,
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["observer-confirmed"] = 1,
                },
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["map"] = 1,
                });
            File.WriteAllText(Path.Combine(attemptOneRunRoot, "validation-summary.json"), JsonSerializer.Serialize(attemptOneValidation, GuiSmokeShared.JsonOptions));
            var attemptOneFirstScreen = Path.Combine(attemptOneRunRoot, "steps", "0001.screen.png");
            File.WriteAllBytes(attemptOneFirstScreen, Array.Empty<byte>());
            LongRunArtifacts.RecordAttemptTerminal(
                longRunSessionRoot,
                "0001",
                1,
                "self-test-session-attempt-0001",
                "player-defeated",
                launchFailed: false,
                failureClass: null,
                trustStateAtStart: GuiSmokeContractStates.TrustValid);
            LongRunArtifacts.WriteSessionArtifacts(
                longRunSessionRoot,
                attemptOneLogger,
                "self-test-session-attempt-0001",
                "boot-to-long-run",
                "headless",
                "0001",
                1,
                1,
                "completed",
                "player-defeated",
                "player-defeated",
                launchFailed: false,
                failureClass: null,
                trustStateAtStart: GuiSmokeContractStates.TrustValid);

            var attemptEntries = File.ReadLines(Path.Combine(longRunSessionRoot, "attempt-index.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeAttemptIndexEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeAttemptIndexEntry>()
                .ToArray();
            Assert(attemptEntries.Length == 1, "Expected one attempt index entry for the synthetic long-run session.");
            Assert(attemptEntries[0].TerminalCause == "player-defeated", "Attempt index should store explicit terminal cause.");
            Assert(attemptEntries[0].TrustStateAtStart == GuiSmokeContractStates.TrustValid, "Attempt index should store trust state at attempt start.");

            var previousAttempt = new GuiSmokeAttemptResult(
                "0001",
                1,
                "self-test-session-attempt-0001",
                attemptOneRunRoot,
                0,
                "completed",
                "player-defeated",
                1,
                LaunchFailed: false,
                TerminalCause: "player-defeated",
                FailureClass: null,
                TrustStateAtStart: GuiSmokeContractStates.TrustValid);
            LongRunArtifacts.RecordRunnerBeginRestart(longRunSessionRoot, previousAttempt, "0002", 2);

            var attemptTwoRunRoot = Path.Combine(longRunSessionRoot, "attempts", "0002");
            Directory.CreateDirectory(Path.Combine(attemptTwoRunRoot, "steps"));
            var attemptTwoFirstScreen = Path.Combine(attemptTwoRunRoot, "steps", "0001.screen.png");
            File.WriteAllBytes(attemptTwoFirstScreen, Array.Empty<byte>());
            LongRunArtifacts.RecordAttemptStarted(
                longRunSessionRoot,
                "0002",
                2,
                "self-test-session-attempt-0002",
                GuiSmokeContractStates.TrustValid,
                attemptTwoFirstScreen);
            var sessionSummary = JsonSerializer.Deserialize<GuiSmokeSessionSummary>(
                                     File.ReadAllText(Path.Combine(longRunSessionRoot, "session-summary.json")),
                                     GuiSmokeShared.JsonOptions)
                                 ?? throw new InvalidOperationException("Failed to read long-run session summary self-test artifact.");
            Assert(sessionSummary.AttemptCount == 2, "Session summary should count started attempts, not only terminalized attempts.");
            Assert(sessionSummary.TerminalAttemptCount == 1, "Session summary should preserve terminal attempt count separately from started attempt count.");
            Assert(string.Equals(sessionSummary.ActiveAttemptId, "0002", StringComparison.OrdinalIgnoreCase), "Session summary should expose the active started attempt id.");
            Assert(sessionSummary.PassedAttempts == 1 && sessionSummary.FailedAttempts == 0, "Session summary should keep valid completed attempts counted as passed.");

            var supervisorState = LongRunArtifacts.RefreshSupervisorState(longRunSessionRoot);
            Assert(supervisorState.TrustState == GuiSmokeContractStates.TrustValid, "Supervisor should mark trust valid when all prevalidation gates are present.");
            Assert(supervisorState.MilestoneState == GuiSmokeContractStates.MilestoneDone, "Supervisor should require terminal, restart, and next attempt first-screen proof before reporting done.");
            Assert(string.Equals(supervisorState.ExpectedCurrentAttemptId, sessionSummary.ActiveAttemptId, StringComparison.OrdinalIgnoreCase), "Supervisor current attempt should match the reviewer-facing session summary for a canonical long-run chronology.");
            Assert(string.Equals(supervisorState.LastTerminalAttemptId, "0001", StringComparison.OrdinalIgnoreCase), "Canonical long-run chronology should preserve the last terminal attempt id.");
            Assert(string.Equals(supervisorState.LastAttemptId, supervisorState.LastTerminalAttemptId, StringComparison.OrdinalIgnoreCase), "Legacy lastAttemptId should alias lastTerminalAttemptId instead of competing with current attempt semantics.");
            Assert(string.Equals(supervisorState.LatestRestartTargetAttemptId, "0002", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(supervisorState.LatestNextAttemptId, "0002", StringComparison.OrdinalIgnoreCase),
                "Canonical long-run chronology should expose restart target and latest next-attempt id separately while they still agree for attempt 0002.");
            Assert(File.Exists(Path.Combine(longRunSessionRoot, "goal-contract.json")), "Expected goal-contract.json to be created.");
            Assert(File.Exists(Path.Combine(longRunSessionRoot, "prevalidation.json")), "Expected prevalidation.json to be created.");
            Assert(File.Exists(Path.Combine(longRunSessionRoot, "restart-events.ndjson")), "Expected restart-events.ndjson to be created.");
            Assert(File.Exists(Path.Combine(longRunSessionRoot, "supervisor-state.json")), "Expected supervisor-state.json to be created.");
            Assert(File.Exists(Path.Combine(longRunSessionRoot, "stall-diagnosis.ndjson")), "Expected stall-diagnosis.ndjson to be created.");
        }
        finally
        {
            if (Directory.Exists(longRunSessionRoot))
            {
                Directory.Delete(longRunSessionRoot, recursive: true);
            }
        }

        var invalidSummaryRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-invalid-summary-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(invalidSummaryRoot);
            LongRunArtifacts.InitializeSessionArtifacts(invalidSummaryRoot, "invalid-summary-session", "boot-to-long-run", "headless");
            Directory.CreateDirectory(Path.Combine(invalidSummaryRoot, "attempts", "0001", "steps"));
            var invalidAttemptEntry = new GuiSmokeAttemptIndexEntry(
                "0001",
                1,
                "invalid-summary-attempt-0001",
                "completed",
                "max-steps-reached:25",
                DateTimeOffset.UtcNow.AddMinutes(-2),
                DateTimeOffset.UtcNow.AddMinutes(-1),
                25,
                "max-steps-reached",
                LaunchFailed: false,
                FailureClass: null,
                TrustStateAtStart: GuiSmokeContractStates.TrustInvalid);
            File.WriteAllText(
                Path.Combine(invalidSummaryRoot, "attempt-index.ndjson"),
                JsonSerializer.Serialize(invalidAttemptEntry, GuiSmokeShared.NdjsonOptions) + Environment.NewLine);
            LongRunArtifacts.RefreshSessionSummary(invalidSummaryRoot);
            var previousInvalidAttempt = new GuiSmokeAttemptResult(
                "0001",
                1,
                "invalid-summary-attempt-0001",
                Path.Combine(invalidSummaryRoot, "attempts", "0001"),
                0,
                "completed",
                "max-steps-reached:25",
                25,
                LaunchFailed: false,
                TerminalCause: "max-steps-reached",
                FailureClass: null,
                TrustStateAtStart: GuiSmokeContractStates.TrustInvalid);
            LongRunArtifacts.RecordRunnerBeginRestart(invalidSummaryRoot, previousInvalidAttempt, "0002", 2);
            LongRunArtifacts.RecordRunnerLaunchIssued(
                invalidSummaryRoot,
                "0002",
                2,
                "invalid-summary-attempt-0002",
                GuiSmokeContractStates.TrustValid);

            var invalidAttemptTwoRunRoot = Path.Combine(invalidSummaryRoot, "attempts", "0002");
            Directory.CreateDirectory(Path.Combine(invalidAttemptTwoRunRoot, "steps"));
            var invalidAttemptTwoFirstScreen = Path.Combine(invalidAttemptTwoRunRoot, "steps", "0001.screen.png");
            File.WriteAllBytes(invalidAttemptTwoFirstScreen, Array.Empty<byte>());
            LongRunArtifacts.RecordAttemptStarted(
                invalidSummaryRoot,
                "0002",
                2,
                "invalid-summary-attempt-0002",
                GuiSmokeContractStates.TrustValid,
                invalidAttemptTwoFirstScreen);

            var invalidSummary = JsonSerializer.Deserialize<GuiSmokeSessionSummary>(
                                     File.ReadAllText(Path.Combine(invalidSummaryRoot, "session-summary.json")),
                                     GuiSmokeShared.JsonOptions)
                                 ?? throw new InvalidOperationException("Failed to read invalid summary self-test artifact.");
            Assert(invalidSummary.AttemptCount == 2, "Session summary should immediately reflect attempt 0002 start in invalid/max-steps sessions.");
            Assert(invalidSummary.TerminalAttemptCount == 1, "Invalid/max-steps session summary should preserve terminal attempt count separately.");
            Assert(string.Equals(invalidSummary.ActiveAttemptId, "0002", StringComparison.OrdinalIgnoreCase), "Invalid/max-steps session summary should expose active attempt 0002.");
            Assert(invalidSummary.PassedAttempts == 0 && invalidSummary.FailedAttempts == 1, "Invalid/max-steps attempts should not be counted as passed.");
            var invalidSupervisor = LongRunArtifacts.RefreshSupervisorState(invalidSummaryRoot);
            Assert(string.Equals(invalidSupervisor.ExpectedCurrentAttemptId, "0002", StringComparison.OrdinalIgnoreCase), "Legacy invalid-0001/valid-0002 chronology should still expose attempt 0002 as the current attempt.");
            Assert(string.Equals(invalidSupervisor.LastTerminalAttemptId, "0001", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(invalidSupervisor.LastAttemptId, "0001", StringComparison.OrdinalIgnoreCase),
                "Legacy invalid-0001/valid-0002 chronology should keep lastAttemptId as a last-terminal alias.");
            Assert(string.Equals(invalidSupervisor.LatestRestartTargetAttemptId, "0002", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(invalidSupervisor.LatestNextAttemptId, "0002", StringComparison.OrdinalIgnoreCase),
                "Legacy invalid-0001/valid-0002 chronology should continue to expose restart target and latest next attempt separately.");
        }
        finally
        {
            if (Directory.Exists(invalidSummaryRoot))
            {
                Directory.Delete(invalidSummaryRoot, recursive: true);
            }
        }

        var negativeMilestoneRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-negative-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(negativeMilestoneRoot);
            LongRunArtifacts.InitializeSessionArtifacts(negativeMilestoneRoot, "negative-session", "boot-to-long-run", "headless");
            var invalidSupervisor = LongRunArtifacts.RefreshSupervisorState(negativeMilestoneRoot);
            Assert(invalidSupervisor.TrustState == GuiSmokeContractStates.TrustInvalid, "Trust should remain invalid until all prevalidation gates have evidence.");
            Assert(invalidSupervisor.MilestoneState == GuiSmokeContractStates.MilestoneInProgress, "Milestone should not advance while trust is invalid.");

            LongRunArtifacts.UpdatePrevalidation(
                negativeMilestoneRoot,
                gameStoppedBeforeDeploy: true,
                modsPayloadReconciled: true,
                deployIdentityVerified: true,
                manualCleanBootVerified: true);
            LongRunArtifacts.UpdateRunnerSessionState(negativeMilestoneRoot, GuiSmokeContractStates.SessionCollecting);

            var attemptRoot = Path.Combine(negativeMilestoneRoot, "attempts", "0001");
            Directory.CreateDirectory(Path.Combine(attemptRoot, "steps"));
            var logger = new ArtifactRecorder(attemptRoot);
            var manifest = new GuiSmokeRunManifest(
                "negative-session-attempt-0001",
                "boot-to-long-run",
                "headless",
                DateTimeOffset.UtcNow.AddMinutes(-2),
                "workspace",
                "live",
                "harness",
                "game")
            {
                Status = "completed",
                ResultMessage = "player-defeated",
                CompletedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            };
            File.WriteAllText(Path.Combine(attemptRoot, "run.json"), JsonSerializer.Serialize(manifest, GuiSmokeShared.JsonOptions));
            File.WriteAllText(
                Path.Combine(attemptRoot, "validation-summary.json"),
                JsonSerializer.Serialize(
                    new GuiSmokeValidationSummary(
                        "negative-session-attempt-0001",
                        1,
                        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
                        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)),
                    GuiSmokeShared.JsonOptions));
            LongRunArtifacts.RecordAttemptTerminal(
                negativeMilestoneRoot,
                "0001",
                1,
                "negative-session-attempt-0001",
                "player-defeated",
                launchFailed: false,
                failureClass: null,
                trustStateAtStart: GuiSmokeContractStates.TrustValid);
            LongRunArtifacts.WriteSessionArtifacts(
                negativeMilestoneRoot,
                logger,
                "negative-session-attempt-0001",
                "boot-to-long-run",
                "headless",
                "0001",
                1,
                0,
                "completed",
                "player-defeated",
                "player-defeated",
                launchFailed: false,
                failureClass: null,
                trustStateAtStart: GuiSmokeContractStates.TrustValid);

            var terminalOnlySupervisor = LongRunArtifacts.RefreshSupervisorState(negativeMilestoneRoot);
            Assert(terminalOnlySupervisor.MilestoneState == GuiSmokeContractStates.MilestoneTerminalSeen, "Milestone should stop at terminal_seen when restart proof is missing.");
            Assert(terminalOnlySupervisor.Blockers.Contains("restart-missing-after:0001"), "Supervisor should report missing restart proof.");

            var previousAttempt = new GuiSmokeAttemptResult(
                "0001",
                1,
                "negative-session-attempt-0001",
                attemptRoot,
                0,
                "completed",
                "player-defeated",
                0,
                LaunchFailed: false,
                TerminalCause: "player-defeated",
                FailureClass: null,
                TrustStateAtStart: GuiSmokeContractStates.TrustValid);
            LongRunArtifacts.RecordRunnerBeginRestart(negativeMilestoneRoot, previousAttempt, "0002", 2);
            LongRunArtifacts.RecordAttemptStarted(
                negativeMilestoneRoot,
                "0002",
                2,
                "negative-session-attempt-0002",
                GuiSmokeContractStates.TrustValid,
                Path.Combine(negativeMilestoneRoot, "attempts", "0002", "steps", "0001.screen.png"));

            var missingFirstScreenSupervisor = LongRunArtifacts.RefreshSupervisorState(negativeMilestoneRoot);
            Assert(missingFirstScreenSupervisor.MilestoneState == GuiSmokeContractStates.MilestoneRestartSeen, "Milestone should stop at restart_seen when attempt N+1 first screen is missing.");
            Assert(missingFirstScreenSupervisor.Blockers.Contains("next-attempt-first-screen-missing:0002"), "Supervisor should report missing next-attempt first-screen proof.");

            LongRunArtifacts.UpdateRunnerSessionState(negativeMilestoneRoot, GuiSmokeContractStates.SessionAborted);
            var failedSupervisor = LongRunArtifacts.RefreshSupervisorState(negativeMilestoneRoot);
            Assert(failedSupervisor.MilestoneState == GuiSmokeContractStates.MilestoneFailed, "Milestone should become failed when the session aborts before completion proof.");
        }
        finally
        {
            if (Directory.Exists(negativeMilestoneRoot))
            {
                Directory.Delete(negativeMilestoneRoot, recursive: true);
            }
        }

        var healthRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-health-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(healthRoot);
            LongRunArtifacts.InitializeSessionArtifacts(healthRoot, "health-session", "boot-to-long-run", "headless");
            LongRunArtifacts.UpdatePrevalidation(
                healthRoot,
                gameStoppedBeforeDeploy: true,
                modsPayloadReconciled: true,
                deployIdentityVerified: true,
                manualCleanBootVerified: true);
            LongRunArtifacts.UpdateRunnerSessionState(healthRoot, GuiSmokeContractStates.SessionCollecting);
            LongRunArtifacts.RecordAttemptStarted(
                healthRoot,
                "0001",
                1,
                "health-session-attempt-0001",
                GuiSmokeContractStates.TrustValid,
                Path.Combine(healthRoot, "attempts", "0001", "steps", "0001.screen.png"));

            var healthSupervisor = LongRunArtifacts.RefreshSupervisorStateForTesting(
                healthRoot,
                now: DateTimeOffset.UtcNow.AddMinutes(10),
                relevantProcessObserved: true,
                windowDetected: true,
                runnerOwnerAlive: false);
            Assert(healthSupervisor.HealthClassifications.Contains("runner-dead"), "Supervisor should classify missing runner owner heartbeat as runner-dead.");
            Assert(healthSupervisor.HealthClassifications.Contains("no-artifact-heartbeat"), "Supervisor should classify stale runner artifacts as no-artifact-heartbeat.");
            Assert(healthSupervisor.HealthClassifications.Contains("window-detected-no-step"), "Supervisor should classify a live window without step artifacts as window-detected-no-step.");
        }
        finally
        {
            if (Directory.Exists(healthRoot))
            {
                Directory.Delete(healthRoot, recursive: true);
            }
        }
    }
}
