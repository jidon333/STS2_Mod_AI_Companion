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
    private static void RunStartupRuntimeEvidenceGovernanceSelfTests()
    {
        var deployVerificationRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-deploy-verify-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(deployVerificationRoot);
            var workspace = Path.Combine(deployVerificationRoot, "workspace");
            var artifactsRoot = Path.Combine(workspace, "artifacts", "native-package-layout", "flat");
            var sourceModsRoot = Path.Combine(artifactsRoot, "mods");
            var gameRoot = Path.Combine(deployVerificationRoot, "game");
            var deployedModsRoot = Path.Combine(gameRoot, "mods");
            Directory.CreateDirectory(sourceModsRoot);
            Directory.CreateDirectory(deployedModsRoot);

            var sourceConfigPath = Path.Combine(sourceModsRoot, "sts2-mod-ai-companion.runtime-config");
            var deployedConfigPath = Path.Combine(deployedModsRoot, "sts2-mod-ai-companion.runtime-config");
            File.WriteAllText(sourceConfigPath, """{"enabled":true,"harness":{"enabled":false},"liveExport":{"collectorModeEnabled":true},"startupSentinel":{"sessionId":"source-session","runId":"source-run","launchToken":"source-token","launchIssuedAtUtc":"2026-03-19T00:00:00.0000000+00:00","sentinelRelativePath":"ai_companion/startup/loader-sentinel.latest.json"}}""");
            File.WriteAllText(deployedConfigPath, """{"enabled":true,"harness":{"enabled":true},"liveExport":{"collectorModeEnabled":true},"startupSentinel":{"sessionId":"deployed-session","runId":"deployed-run","launchToken":"deployed-token","launchIssuedAtUtc":"2026-03-19T00:01:00.0000000+00:00","sentinelRelativePath":"ai_companion/startup/loader-sentinel.latest.json"}}""");
            File.WriteAllText(
                Path.Combine(artifactsRoot, "native-deploy-report.json"),
                JsonSerializer.Serialize(
                    new
                    {
                        sourcePackageRoot = sourceModsRoot,
                        deployedRoot = deployedModsRoot,
                        files = new[]
                        {
                            new
                            {
                                sourcePath = sourceConfigPath,
                                destinationPath = deployedConfigPath,
                            },
                        },
                    },
                    GuiSmokeShared.JsonOptions));

            var deployConfiguration = ScaffoldConfiguration.CreateLocalDefault() with
            {
                GamePaths = new GamePathOptions
                {
                    GameDirectory = gameRoot,
                    UserDataRoot = Path.Combine(deployVerificationRoot, "userdata"),
                    SteamAccountId = "self-test",
                    ProfileIndex = 1,
                    ArtifactsRoot = "artifacts",
                },
            };
            var deploySessionRoot = Path.Combine(deployVerificationRoot, "session");
            Directory.CreateDirectory(Path.Combine(deploySessionRoot, "attempts"));
            LongRunArtifacts.InitializeSessionArtifacts(deploySessionRoot, "deploy-session", "boot-to-long-run", "headless");
            LongRunArtifacts.RecordDeployVerificationEvidence(deploySessionRoot, deployConfiguration, workspace, includeHarnessBridge: false);
            var deployPrevalidation = JsonSerializer.Deserialize<GuiSmokePrevalidation>(File.ReadAllText(Path.Combine(deploySessionRoot, "prevalidation.json")), GuiSmokeShared.JsonOptions)
                                     ?? throw new InvalidOperationException("Failed to read deploy prevalidation self-test.");
            Assert(deployPrevalidation.DeployIdentityVerified, "Deploy verification should stay valid after the runner's intentional harness-enabled and startupSentinel rewrite.");
            Assert(deployPrevalidation.DeployEvidence?.HashMismatches.Count == 0, "Intentional runtime-config startupSentinel rewrite should not register as a deploy hash mismatch.");
        }
        finally
        {
            if (Directory.Exists(deployVerificationRoot))
            {
                Directory.Delete(deployVerificationRoot, recursive: true);
            }
        }

        var trustResampleRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-bootstrap-trust-resample-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(trustResampleRoot);
            LongRunArtifacts.InitializeSessionArtifacts(trustResampleRoot, "bootstrap-trust-resample-session", "boot-to-long-run", "headless");
            LongRunArtifacts.UpdatePrevalidation(
                trustResampleRoot,
                gameStoppedBeforeDeploy: true,
                modsPayloadReconciled: true,
                deployIdentityVerified: true,
                manualCleanBootVerified: true);
            var cachedTrustState = ResolveTrustStateAtAttemptStart(trustResampleRoot);
            Assert(string.Equals(cachedTrustState, GuiSmokeContractStates.TrustValid, StringComparison.OrdinalIgnoreCase), "Bootstrap trust resample self-test should begin from a valid post-bootstrap root.");
            LongRunArtifacts.UpdatePrevalidation(
                trustResampleRoot,
                manualCleanBootVerified: false);
            var resampledTrustState = ResolveTrustStateAtAttemptStart(trustResampleRoot);
            Assert(string.Equals(resampledTrustState, GuiSmokeContractStates.TrustInvalid, StringComparison.OrdinalIgnoreCase), "Authoritative attempt trust should be resampled from supervisor state after bootstrap instead of carrying a stale in-memory valid flag.");
        }
        finally
        {
            if (Directory.Exists(trustResampleRoot))
            {
                Directory.Delete(trustResampleRoot, recursive: true);
            }
        }

        var bootstrapOnlyRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-bootstrap-only-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(bootstrapOnlyRoot);
            LongRunArtifacts.InitializeSessionArtifacts(bootstrapOnlyRoot, "bootstrap-only-session", "boot-to-long-run", "headless");
            LongRunArtifacts.UpdatePrevalidation(
                bootstrapOnlyRoot,
                gameStoppedBeforeDeploy: true,
                modsPayloadReconciled: true,
                deployIdentityVerified: true,
                manualCleanBootVerified: true);
            var bootstrapOnlySupervisor = LongRunArtifacts.RefreshSupervisorState(bootstrapOnlyRoot);
            var bootstrapOnlySummary = JsonSerializer.Deserialize<GuiSmokeSessionSummary>(
                                           File.ReadAllText(Path.Combine(bootstrapOnlyRoot, "session-summary.json")),
                                           GuiSmokeShared.JsonOptions)
                                       ?? throw new InvalidOperationException("Failed to read bootstrap-only session summary self-test artifact.");
            var bootstrapOnlyAttempts = ReadNdjsonFixture<GuiSmokeAttemptIndexEntry>(Path.Combine(bootstrapOnlyRoot, "attempt-index.ndjson"));
            var bootstrapOnlyEvents = ReadNdjsonFixture<GuiSmokeRestartEvent>(Path.Combine(bootstrapOnlyRoot, "restart-events.ndjson"));
            Assert(string.Equals(bootstrapOnlySupervisor.TrustState, GuiSmokeContractStates.TrustValid, StringComparison.OrdinalIgnoreCase), "Bootstrap-only session should allow root trust to become valid before any gameplay attempt exists.");
            Assert(bootstrapOnlyAttempts.Count == 0, "Bootstrap-only session should not create attempt-index entries before the authoritative first attempt opens.");
            Assert(bootstrapOnlyEvents.Count == 0, "Bootstrap-only session should not record authoritative restart events before the first gameplay attempt opens.");
            Assert(bootstrapOnlySummary.AttemptCount == 0 && bootstrapOnlySummary.ActiveAttemptId is null, "Bootstrap-only session summary should report zero attempts and no active attempt.");
            Assert(bootstrapOnlySupervisor.ExpectedCurrentAttemptId is null, "Bootstrap-only session should not infer a current authoritative attempt before any chronology events exist.");
            Assert(bootstrapOnlySupervisor.LastTerminalAttemptId is null && bootstrapOnlySupervisor.LastAttemptId is null, "Bootstrap-only session should not report terminal attempt ids before any authoritative attempt exists.");
        }
        finally
        {
            if (Directory.Exists(bootstrapOnlyRoot))
            {
                Directory.Delete(bootstrapOnlyRoot, recursive: true);
            }
        }

        var bootstrapPhaseBoundaryRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-bootstrap-boundary-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(bootstrapPhaseBoundaryRoot);
            var harnessRoot = Path.Combine(bootstrapPhaseBoundaryRoot, "harness");
            var inboxRoot = Path.Combine(harnessRoot, "inbox");
            var outboxRoot = Path.Combine(harnessRoot, "outbox");
            Directory.CreateDirectory(inboxRoot);
            Directory.CreateDirectory(outboxRoot);
            var harnessLayout = new HarnessQueueLayout(
                bootstrapPhaseBoundaryRoot,
                harnessRoot,
                inboxRoot,
                Path.Combine(inboxRoot, "actions.ndjson"),
                outboxRoot,
                Path.Combine(outboxRoot, "results.ndjson"),
                Path.Combine(harnessRoot, "status.json"),
                Path.Combine(outboxRoot, "trace.ndjson"),
                Path.Combine(harnessRoot, "arm.json"),
                Path.Combine(outboxRoot, "inventory.latest.json"));
            File.WriteAllText(
                harnessLayout.InventoryPath,
                JsonSerializer.Serialize(
                    new HarnessNodeInventory(
                        "inventory-bootstrap-boundary",
                        DateTimeOffset.UtcNow,
                        "pending",
                        "main-menu",
                        null,
                        "dormant",
                        null,
                        true,
                        "mixed",
                        "stable",
                        Array.Empty<HarnessNodeInventoryItem>()),
                    GuiSmokeShared.JsonOptions));
            LongRunArtifacts.InitializeSessionArtifacts(bootstrapPhaseBoundaryRoot, "bootstrap-boundary-session", "boot-to-long-run", "headless");
            var screenshotPath = Path.Combine(bootstrapPhaseBoundaryRoot, "main-menu.png");
            var observerPath = Path.Combine(bootstrapPhaseBoundaryRoot, "main-menu.observer.json");
            File.WriteAllBytes(screenshotPath, Array.Empty<byte>());
            File.WriteAllText(observerPath, "{}");
            Assert(
                !LongRunArtifacts.TryMarkManualCleanBootVerified(
                    bootstrapPhaseBoundaryRoot,
                    harnessLayout,
                    new ObserverState(
                        new ObserverSummary(
                            "main-menu",
                            "main-menu",
                            false,
                            DateTimeOffset.UtcNow,
                            "inv-bootstrap-boundary",
                            true,
                            "mixed",
                            "stable",
                            "episode-bootstrap-boundary",
                            null,
                            "main-menu",
                            null,
                            null,
                            null,
                            Array.Empty<string>(),
                            Array.Empty<string>(),
                            Array.Empty<ObserverActionNode>(),
                            Array.Empty<ObserverChoice>(),
                            Array.Empty<ObservedCombatHandCard>()),
                        null,
                        null,
                        null),
                    Array.Empty<GuiSmokeHistoryEntry>(),
                    screenshotPath,
                    observerPath,
                    DateTimeOffset.UtcNow.AddSeconds(-5),
                    stillInWaitMainMenu: false),
                "Bootstrap completion should stay blocked when manual clean boot proof is observed outside the WaitMainMenu pre-attempt boundary.");
            var boundaryPrevalidation = JsonSerializer.Deserialize<GuiSmokePrevalidation>(
                                            File.ReadAllText(Path.Combine(bootstrapPhaseBoundaryRoot, "prevalidation.json")),
                                            GuiSmokeShared.JsonOptions)
                                        ?? throw new InvalidOperationException("Failed to read bootstrap boundary prevalidation self-test artifact.");
            var boundarySummary = JsonSerializer.Deserialize<GuiSmokeSessionSummary>(
                                      File.ReadAllText(Path.Combine(bootstrapPhaseBoundaryRoot, "session-summary.json")),
                                      GuiSmokeShared.JsonOptions)
                                  ?? throw new InvalidOperationException("Failed to read bootstrap boundary session summary self-test artifact.");
            var boundaryAttempts = ReadNdjsonFixture<GuiSmokeAttemptIndexEntry>(Path.Combine(bootstrapPhaseBoundaryRoot, "attempt-index.ndjson"));
            var boundaryEvents = ReadNdjsonFixture<GuiSmokeRestartEvent>(Path.Combine(bootstrapPhaseBoundaryRoot, "restart-events.ndjson"));
            Assert(boundaryPrevalidation.ManualCleanBootEvidence?.BlockingReasons?.Contains("not-wait-main-menu-phase", StringComparer.OrdinalIgnoreCase) == true, "Bootstrap boundary failures should record the not-wait-main-menu-phase blocker.");
            Assert(boundaryAttempts.Count == 0 && boundaryEvents.Count == 0, "Bootstrap failure before the pre-attempt boundary should not create authoritative attempt artifacts.");
            Assert(boundarySummary.AttemptCount == 0 && boundarySummary.ActiveAttemptId is null, "Bootstrap failure before the pre-attempt boundary should keep the session summary at zero attempts.");
        }
        finally
        {
            if (Directory.Exists(bootstrapPhaseBoundaryRoot))
            {
                Directory.Delete(bootstrapPhaseBoundaryRoot, recursive: true);
            }
        }

        var relaunchOrderingRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-bootstrap-relaunch-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(relaunchOrderingRoot);
            LongRunArtifacts.InitializeSessionArtifacts(relaunchOrderingRoot, "bootstrap-relaunch-session", "boot-to-long-run", "headless");
            LongRunArtifacts.UpdatePrevalidation(
                relaunchOrderingRoot,
                gameStoppedBeforeDeploy: true,
                modsPayloadReconciled: true,
                deployIdentityVerified: true,
                manualCleanBootVerified: true);
            var relaunchTrustState = ResolveTrustStateAtAttemptStart(relaunchOrderingRoot);
            Assert(string.Equals(relaunchTrustState, GuiSmokeContractStates.TrustValid, StringComparison.OrdinalIgnoreCase), "Bootstrap relaunch self-test should resample valid trust from the root before authoritative attempt 0001 starts.");
            LongRunArtifacts.RecordRunnerLaunchIssued(relaunchOrderingRoot, "0001", 1, "bootstrap-relaunch-attempt-0001", relaunchTrustState);
            var relaunchFirstScreen = Path.Combine(relaunchOrderingRoot, "attempts", "0001", "steps", "0001.screen.png");
            Directory.CreateDirectory(Path.GetDirectoryName(relaunchFirstScreen)!);
            File.WriteAllBytes(relaunchFirstScreen, Array.Empty<byte>());
            LongRunArtifacts.RecordAttemptStarted(
                relaunchOrderingRoot,
                "0001",
                1,
                "bootstrap-relaunch-attempt-0001",
                relaunchTrustState,
                relaunchFirstScreen);
            var relaunchEvents = ReadNdjsonFixture<GuiSmokeRestartEvent>(Path.Combine(relaunchOrderingRoot, "restart-events.ndjson"));
            Assert(relaunchEvents.Count == 2, "Relaunch sequencing should only emit runner-launch-issued and next-attempt-started once the authoritative attempt opens.");
            Assert(string.Equals(relaunchEvents[0].AttemptId, "0001", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(relaunchEvents[0].EventType, GuiSmokeContractStates.EventRunnerLaunchIssued, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(relaunchEvents[0].TrustStateAtStart, GuiSmokeContractStates.TrustValid, StringComparison.OrdinalIgnoreCase),
                "The second launch should become authoritative attempt 0001 with a valid trustStateAtStart.");
            Assert(string.Equals(relaunchEvents[1].AttemptId, "0001", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(relaunchEvents[1].EventType, GuiSmokeContractStates.EventNextAttemptStarted, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(relaunchEvents[1].TrustStateAtStart, GuiSmokeContractStates.TrustValid, StringComparison.OrdinalIgnoreCase),
                "Authoritative attempt 0001 should preserve valid trust on next-attempt-started.");
        }
        finally
        {
            if (Directory.Exists(relaunchOrderingRoot))
            {
                Directory.Delete(relaunchOrderingRoot, recursive: true);
            }
        }

        var eventOrderingRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-event-order-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(eventOrderingRoot);
            LongRunArtifacts.InitializeSessionArtifacts(eventOrderingRoot, "event-order-session", "boot-to-long-run", "headless");
            LongRunArtifacts.RecordRunnerLaunchIssued(eventOrderingRoot, "0001", 1, "event-order-session-attempt-0001", GuiSmokeContractStates.TrustInvalid);
            var firstScreenPath = Path.Combine(eventOrderingRoot, "attempts", "0001", "steps", "0001.screen.png");
            Directory.CreateDirectory(Path.GetDirectoryName(firstScreenPath)!);
            File.WriteAllBytes(firstScreenPath, Array.Empty<byte>());
            LongRunArtifacts.RecordAttemptStarted(eventOrderingRoot, "0001", 1, "event-order-session-attempt-0001", GuiSmokeContractStates.TrustInvalid, firstScreenPath);
            var restartEvents = File.ReadLines(Path.Combine(eventOrderingRoot, "restart-events.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeRestartEvent>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeRestartEvent>()
                .ToArray();
            Assert(restartEvents.Length == 2, "Expected launch-issued and next-attempt-started restart events.");
            Assert(string.Equals(restartEvents[0].EventType, GuiSmokeContractStates.EventRunnerLaunchIssued, StringComparison.OrdinalIgnoreCase), "runner-launch-issued should be recorded before next-attempt-started.");
            Assert(string.Equals(restartEvents[1].EventType, GuiSmokeContractStates.EventNextAttemptStarted, StringComparison.OrdinalIgnoreCase), "next-attempt-started should be recorded at first-screen proof time.");
        }
        finally
        {
            if (Directory.Exists(eventOrderingRoot))
            {
                Directory.Delete(eventOrderingRoot, recursive: true);
            }
        }

        var launchIssuedGapRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-launch-issued-gap-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(launchIssuedGapRoot);
            LongRunArtifacts.InitializeSessionArtifacts(launchIssuedGapRoot, "launch-issued-gap-session", "boot-to-long-run", "headless");
            LongRunArtifacts.UpdatePrevalidation(
                launchIssuedGapRoot,
                gameStoppedBeforeDeploy: true,
                modsPayloadReconciled: true,
                deployIdentityVerified: true,
                manualCleanBootVerified: true);
            LongRunArtifacts.UpdateRunnerSessionState(launchIssuedGapRoot, GuiSmokeContractStates.SessionCollecting);
            var gapAttemptOneRoot = Path.Combine(launchIssuedGapRoot, "attempts", "0001");
            Directory.CreateDirectory(Path.Combine(gapAttemptOneRoot, "steps"));
            var gapAttemptOneFirstScreen = Path.Combine(gapAttemptOneRoot, "steps", "0001.screen.png");
            File.WriteAllBytes(gapAttemptOneFirstScreen, Array.Empty<byte>());
            var gapPreviousAttempt = new GuiSmokeAttemptResult(
                "0001",
                1,
                "launch-issued-gap-attempt-0001",
                gapAttemptOneRoot,
                0,
                "completed",
                "max-steps-reached:8",
                8,
                LaunchFailed: false,
                TerminalCause: "max-steps-reached",
                FailureClass: null,
                TrustStateAtStart: GuiSmokeContractStates.TrustInvalid);
            LongRunArtifacts.RecordAttemptTerminal(
                launchIssuedGapRoot,
                "0001",
                1,
                "launch-issued-gap-attempt-0001",
                "max-steps-reached",
                launchFailed: false,
                failureClass: null,
                trustStateAtStart: GuiSmokeContractStates.TrustInvalid);
            LongRunArtifacts.WriteSessionArtifacts(
                launchIssuedGapRoot,
                new ArtifactRecorder(gapAttemptOneRoot),
                "launch-issued-gap-attempt-0001",
                "boot-to-long-run",
                "headless",
                "0001",
                1,
                8,
                "completed",
                "max-steps-reached:8",
                "max-steps-reached",
                launchFailed: false,
                failureClass: null,
                trustStateAtStart: GuiSmokeContractStates.TrustInvalid);
            LongRunArtifacts.RecordRunnerBeginRestart(launchIssuedGapRoot, gapPreviousAttempt, "0002", 2);
            LongRunArtifacts.RecordRunnerLaunchIssued(
                launchIssuedGapRoot,
                "0002",
                2,
                "launch-issued-gap-attempt-0002",
                GuiSmokeContractStates.TrustValid);
            var gapSummary = JsonSerializer.Deserialize<GuiSmokeSessionSummary>(
                                 File.ReadAllText(Path.Combine(launchIssuedGapRoot, "session-summary.json")),
                                 GuiSmokeShared.JsonOptions)
                             ?? throw new InvalidOperationException("Failed to read launch-issued gap session summary self-test artifact.");
            var gapSupervisor = LongRunArtifacts.RefreshSupervisorState(launchIssuedGapRoot);
            Assert(gapSummary.AttemptCount == 2, "Launch-issued gap chronology should count authoritative attempt 0002 immediately at runner-launch-issued.");
            Assert(string.Equals(gapSummary.ActiveAttemptId, "0002", StringComparison.OrdinalIgnoreCase), "Session summary should treat a launch-issued/no-first-screen gap as active attempt 0002.");
            Assert(string.Equals(gapSupervisor.ExpectedCurrentAttemptId, "0002", StringComparison.OrdinalIgnoreCase), "Supervisor should match session summary current attempt during a launch-issued/no-first-screen gap.");
            Assert(string.Equals(gapSupervisor.LastTerminalAttemptId, "0001", StringComparison.OrdinalIgnoreCase), "Supervisor should keep the last terminal attempt separate from the current launch-issued target.");
            Assert(string.Equals(gapSupervisor.LastAttemptId, "0001", StringComparison.OrdinalIgnoreCase), "Legacy lastAttemptId should alias the last terminal attempt during chronology gap cases.");
            Assert(string.Equals(gapSupervisor.LatestRestartTargetAttemptId, "0002", StringComparison.OrdinalIgnoreCase), "Supervisor should expose the latest restart target from chronology.");
            Assert(gapSupervisor.LatestNextAttemptId is null, "Supervisor should not invent latestNextAttemptId before next-attempt-started is recorded.");
        }
        finally
        {
            if (Directory.Exists(launchIssuedGapRoot))
            {
                Directory.Delete(launchIssuedGapRoot, recursive: true);
            }
        }

        var supervisorWriteStabilityRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-supervisor-write-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(supervisorWriteStabilityRoot);
            LongRunArtifacts.InitializeSessionArtifacts(supervisorWriteStabilityRoot, "supervisor-write-session", "boot-to-long-run", "headless");
            var goalPath = Path.Combine(supervisorWriteStabilityRoot, "goal-contract.json");
            LongRunArtifacts.UpdateRunnerSessionState(supervisorWriteStabilityRoot, GuiSmokeContractStates.SessionCollecting);
            var goalAfterRunnerUpdate = JsonSerializer.Deserialize<GuiSmokeGoalContract>(File.ReadAllText(goalPath), GuiSmokeShared.JsonOptions)
                                        ?? throw new InvalidOperationException("Failed to read goal contract after runner update self-test.");
            Thread.Sleep(40);
            var supervisorState = LongRunArtifacts.RefreshSupervisorState(supervisorWriteStabilityRoot);
            var goalAfterSupervisorRefresh = JsonSerializer.Deserialize<GuiSmokeGoalContract>(File.ReadAllText(goalPath), GuiSmokeShared.JsonOptions)
                                            ?? throw new InvalidOperationException("Failed to read goal contract after supervisor refresh self-test.");
            Assert(goalAfterSupervisorRefresh.UpdatedAt == goalAfterRunnerUpdate.UpdatedAt, "Supervisor refresh should not rewrite goal-contract when trust and milestone state did not change.");
            Assert(string.Equals(supervisorState.SessionState, GuiSmokeContractStates.SessionCollecting, StringComparison.OrdinalIgnoreCase), "Supervisor refresh should preserve the runner session state while avoiding redundant goal writes.");
        }
        finally
        {
            if (Directory.Exists(supervisorWriteStabilityRoot))
            {
                Directory.Delete(supervisorWriteStabilityRoot, recursive: true);
            }
        }
    }
}
