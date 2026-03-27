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
    private static void RunStartupRuntimeEvidenceSelfTests()
    {
        var startupRuntimeEvidenceRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-startup-runtime-evidence-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(startupRuntimeEvidenceRoot);
            var gameRoot = Path.Combine(startupRuntimeEvidenceRoot, "game");
            var modsRoot = Path.Combine(gameRoot, "mods");
            var userDataRoot = Path.Combine(startupRuntimeEvidenceRoot, "userdata");
            var logsRoot = Path.Combine(userDataRoot, "logs");
            var settingsRoot = Path.Combine(userDataRoot, "steam", "self-test");
            Directory.CreateDirectory(modsRoot);
            Directory.CreateDirectory(logsRoot);
            Directory.CreateDirectory(settingsRoot);
            var startupConfiguration = ScaffoldConfiguration.CreateLocalDefault() with
            {
                GamePaths = new GamePathOptions
                {
                    GameDirectory = gameRoot,
                    UserDataRoot = userDataRoot,
                    SteamAccountId = "self-test",
                    ProfileIndex = 1,
                    ArtifactsRoot = "artifacts",
                },
            };
            var startupLiveLayout = LiveExportPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.LiveExport);
            var startupHarnessLayout = HarnessPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.Harness);
            LiveExportPathResolver.EnsureDirectory(startupLiveLayout);
            HarnessPathResolver.EnsureDirectories(startupHarnessLayout);
            LongRunArtifacts.InitializeSessionArtifacts(startupRuntimeEvidenceRoot, "startup-runtime-evidence-session", "boot-to-long-run", "headless");
            WriteRuntimeConfigFixture(startupConfiguration, modsRoot, harnessEnabled: false);
            File.WriteAllBytes(Path.Combine(modsRoot, startupConfiguration.AiCompanionMod.PckName), Array.Empty<byte>());
            File.WriteAllBytes(Path.Combine(modsRoot, "sts2-mod-ai-companion.dll"), Array.Empty<byte>());
            File.WriteAllText(
                Path.Combine(settingsRoot, "settings.save"),
                """{"mod_settings":{"mods_enabled":true}}""");
            var runtimeLogPath = Path.Combine(modsRoot, startupConfiguration.AiCompanionMod.RuntimeLogFileName);
            var godotLogPath = Path.Combine(logsRoot, "godot.log");
            File.WriteAllText(
                runtimeLogPath,
                string.Join(
                    Environment.NewLine,
                    new[]
                    {
                        "[STS2 Mod AI Companion] module initializer bootstrap result: initialized=False process=OldRun",
                        "[STS2 Mod AI Companion] old irrelevant line",
                    }));
            File.WriteAllText(
                godotLogPath,
                string.Join(
                    Environment.NewLine,
                    new[]
                    {
                        "[INFO] Found mod pck file stale-mod.pck",
                        "[INFO] old irrelevant line",
                    }));
            var launchIssuedAt = DateTimeOffset.UtcNow;
            var startupRuntimeConfig = EnsureStartupRuntimeConfig(
                startupConfiguration,
                "startup-runtime-evidence-session",
                "startup-runtime-evidence-run",
                launchIssuedAt);
            WriteStartupSentinelFixture(
                startupConfiguration.GamePaths,
                startupRuntimeConfig.SentinelRelativePath,
                "startup-runtime-evidence-session",
                "startup-runtime-evidence-run",
                startupRuntimeConfig.LaunchToken);
            LongRunArtifacts.RecordStartupLogBaseline(
                startupRuntimeEvidenceRoot,
                startupConfiguration,
                "startup-runtime-evidence-run",
                startupRuntimeConfig.LaunchToken,
                launchIssuedAt);
            File.AppendAllText(
                runtimeLogPath,
                Environment.NewLine + string.Join(
                    Environment.NewLine,
                    new[]
                    {
                        "[STS2 Mod AI Companion] runtime exporter initialized. live_root=C:\\fake\\live",
                        "[STS2 Mod AI Companion] harness bridge initialize result: True root=C:\\fake\\harness live_snapshot=C:\\fake\\state.latest.json poll_ms=250",
                    }));
            File.AppendAllText(
                godotLogPath,
                Environment.NewLine + string.Join(
                    Environment.NewLine,
                    new[]
                    {
                        "[INFO] Loading assembly DLL sts2-mod-ai-companion.dll",
                        "[INFO] Finished mod initialization for 'STS2 Mod AI Companion' (sts2-mod-ai-companion).",
                        "[INFO] [Startup] Time to main menu: 12,869ms",
                    }));
            File.WriteAllText(startupLiveLayout.SnapshotPath, "{}");
            var startupEvidence = LongRunArtifacts.RecordStartupRuntimeEvidence(
                startupRuntimeEvidenceRoot,
                startupConfiguration,
                startupLiveLayout,
                startupHarnessLayout);
            Assert(startupEvidence.RuntimeExporterInitializedLogged, "Startup runtime evidence should record runtime exporter initialization.");
            Assert(startupEvidence.HarnessBridgeInitializeLogged, "Startup runtime evidence should record harness bridge initialization.");
            Assert(startupEvidence.SettingsModsEnabled, "Startup runtime evidence should capture mods_enabled=true from settings.save.");
            Assert(startupEvidence.RuntimeConfigEnabled && startupEvidence.RuntimeConfigHarnessEnabled, "Startup runtime evidence should capture deployed runtime-config enabled flags.");
            Assert(string.Equals(startupEvidence.StartupSentinelLaunchToken, startupRuntimeConfig.LaunchToken, StringComparison.Ordinal), "Startup runtime evidence should capture the rewritten startupSentinel launch token.");
            Assert(startupEvidence.SentinelPresent, "Startup runtime evidence should capture the current-execution sentinel file.");
            Assert(startupEvidence.SentinelSessionMatch && startupEvidence.SentinelRunMatch && startupEvidence.SentinelLaunchTokenMatch, "Startup runtime evidence should verify the sentinel session/run/launch-token match.");
            Assert(startupEvidence.CompanionPckPresent && startupEvidence.CompanionDllPresent, "Startup runtime evidence should capture deployed companion payload presence.");
            Assert(startupEvidence.FreshSnapshotPresent && !startupEvidence.StaleSnapshotObserved && !startupEvidence.NoSnapshotEvidence, "Fresh startup snapshot evidence should stay distinct from stale or absent snapshot states.");
            Assert(string.Equals(startupEvidence.Diagnosis, "runtime-started-snapshots-present", StringComparison.OrdinalIgnoreCase), "Runtime evidence with exporter markers and snapshots should diagnose runtime-started-snapshots-present.");
            Assert(startupEvidence.CaptureCount == 1, "Single startup runtime evidence call should create one startup capture.");
            Assert(string.Equals(startupEvidence.FirstPositiveReason, "godot-reached-main-menu", StringComparison.Ordinal), "First positive startup reason should retain the earliest observed progress signal.");
            Assert(startupEvidence.EverReachedMainMenu && startupEvidence.EverSawRuntimeExporter && startupEvidence.EverSawFreshSnapshot, "Startup reviewer summary should preserve ever-positive startup signals.");
            Assert(startupEvidence.RuntimeLogDeltaMatches.Count == 2 && startupEvidence.GodotLogDeltaMatches.Count == 3, "Startup runtime evidence should use only post-baseline appended lines as delta matches.");
            Assert(File.Exists(Path.Combine(startupRuntimeEvidenceRoot, "startup-runtime-evidence.json")), "Startup runtime evidence should persist a structured artifact.");
            Assert(File.Exists(Path.Combine(startupRuntimeEvidenceRoot, "startup-runtime-captures.ndjson")), "Startup runtime evidence should persist a startup capture sequence artifact.");
            Assert(File.Exists(Path.Combine(startupRuntimeEvidenceRoot, "startup-log-baseline.json")), "Startup runtime evidence should persist the launch-time log baseline artifact.");
            Assert(File.Exists(Path.Combine(startupRuntimeEvidenceRoot, "startup-runtime-log.tail.txt")), "Startup runtime evidence should persist a runtime-log tail artifact.");
            Assert(File.Exists(Path.Combine(startupRuntimeEvidenceRoot, "startup-godot-log.tail.txt")), "Startup runtime evidence should persist a Godot-log tail artifact.");
            Assert(File.Exists(Path.Combine(startupRuntimeEvidenceRoot, "startup-runtime-log.delta.txt")), "Startup runtime evidence should persist a runtime-log delta artifact.");
            Assert(File.Exists(Path.Combine(startupRuntimeEvidenceRoot, "startup-godot-log.delta.txt")), "Startup runtime evidence should persist a Godot-log delta artifact.");
            Assert(ReadNdjsonFixture<GuiSmokeStartupRuntimeCapture>(Path.Combine(startupRuntimeEvidenceRoot, "startup-runtime-captures.ndjson")).Count == 1, "Startup runtime capture sequence should record the latest capture.");
            var runtimeDeltaBody = File.ReadAllText(Path.Combine(startupRuntimeEvidenceRoot, "startup-runtime-log.delta.txt"));
            var godotDeltaBody = File.ReadAllText(Path.Combine(startupRuntimeEvidenceRoot, "startup-godot-log.delta.txt"));
            Assert(!runtimeDeltaBody.Contains("OldRun", StringComparison.OrdinalIgnoreCase), "Runtime-log delta should exclude relevant lines that predated the launch baseline.");
            Assert(!godotDeltaBody.Contains("stale-mod.pck", StringComparison.OrdinalIgnoreCase), "Godot-log delta should exclude relevant lines that predated the launch baseline.");
        }
        finally
        {
            if (Directory.Exists(startupRuntimeEvidenceRoot))
            {
                Directory.Delete(startupRuntimeEvidenceRoot, recursive: true);
            }
        }

        var startupRuntimeTruncatedRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-startup-runtime-truncated-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(startupRuntimeTruncatedRoot);
            var gameRoot = Path.Combine(startupRuntimeTruncatedRoot, "game");
            var modsRoot = Path.Combine(gameRoot, "mods");
            var userDataRoot = Path.Combine(startupRuntimeTruncatedRoot, "userdata");
            var logsRoot = Path.Combine(userDataRoot, "logs");
            var settingsRoot = Path.Combine(userDataRoot, "steam", "self-test");
            Directory.CreateDirectory(modsRoot);
            Directory.CreateDirectory(logsRoot);
            Directory.CreateDirectory(settingsRoot);
            var startupConfiguration = ScaffoldConfiguration.CreateLocalDefault() with
            {
                GamePaths = new GamePathOptions
                {
                    GameDirectory = gameRoot,
                    UserDataRoot = userDataRoot,
                    SteamAccountId = "self-test",
                    ProfileIndex = 1,
                    ArtifactsRoot = "artifacts",
                },
            };
            var startupLiveLayout = LiveExportPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.LiveExport);
            var startupHarnessLayout = HarnessPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.Harness);
            LiveExportPathResolver.EnsureDirectory(startupLiveLayout);
            HarnessPathResolver.EnsureDirectories(startupHarnessLayout);
            LongRunArtifacts.InitializeSessionArtifacts(startupRuntimeTruncatedRoot, "startup-runtime-truncated-session", "boot-to-long-run", "headless");
            WriteRuntimeConfigFixture(startupConfiguration, modsRoot, harnessEnabled: false);
            File.WriteAllBytes(Path.Combine(modsRoot, startupConfiguration.AiCompanionMod.PckName), Array.Empty<byte>());
            File.WriteAllBytes(Path.Combine(modsRoot, "sts2-mod-ai-companion.dll"), Array.Empty<byte>());
            File.WriteAllText(Path.Combine(settingsRoot, "settings.save"), """{"mod_settings":{"mods_enabled":true}}""");
            var runtimeLogPath = Path.Combine(modsRoot, startupConfiguration.AiCompanionMod.RuntimeLogFileName);
            File.WriteAllText(runtimeLogPath, new string('x', 2048));
            var launchIssuedAt = DateTimeOffset.UtcNow;
            var startupRuntimeConfig = EnsureStartupRuntimeConfig(
                startupConfiguration,
                "startup-runtime-truncated-session",
                "startup-runtime-truncated-run",
                launchIssuedAt);
            LongRunArtifacts.RecordStartupLogBaseline(
                startupRuntimeTruncatedRoot,
                startupConfiguration,
                "startup-runtime-truncated-run",
                startupRuntimeConfig.LaunchToken,
                launchIssuedAt);
            File.WriteAllText(
                runtimeLogPath,
                "[STS2 Mod AI Companion] runtime exporter initialized. live_root=C:\\fake\\live");
            var truncatedEvidence = LongRunArtifacts.RecordStartupRuntimeEvidence(
                startupRuntimeTruncatedRoot,
                startupConfiguration,
                startupLiveLayout,
                startupHarnessLayout);
            Assert(truncatedEvidence.RuntimeLogDeltaTreatedAsCurrentExecution, "Runtime-log truncation should be treated as current-execution delta evidence.");
            Assert(truncatedEvidence.RuntimeLogDeltaMatches.Count == 1, "Truncated runtime-log current file should be fully treated as current-execution delta.");
        }
        finally
        {
            if (Directory.Exists(startupRuntimeTruncatedRoot))
            {
                Directory.Delete(startupRuntimeTruncatedRoot, recursive: true);
            }
        }

        var startupRuntimeMissingRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-startup-runtime-missing-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(startupRuntimeMissingRoot);
            var gameRoot = Path.Combine(startupRuntimeMissingRoot, "game");
            var modsRoot = Path.Combine(gameRoot, "mods");
            var userDataRoot = Path.Combine(startupRuntimeMissingRoot, "userdata");
            var logsRoot = Path.Combine(userDataRoot, "logs");
            var settingsRoot = Path.Combine(userDataRoot, "steam", "self-test");
            Directory.CreateDirectory(modsRoot);
            Directory.CreateDirectory(logsRoot);
            Directory.CreateDirectory(settingsRoot);
            var startupConfiguration = ScaffoldConfiguration.CreateLocalDefault() with
            {
                GamePaths = new GamePathOptions
                {
                    GameDirectory = gameRoot,
                    UserDataRoot = userDataRoot,
                    SteamAccountId = "self-test",
                    ProfileIndex = 1,
                    ArtifactsRoot = "artifacts",
                },
            };
            var startupLiveLayout = LiveExportPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.LiveExport);
            var startupHarnessLayout = HarnessPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.Harness);
            LiveExportPathResolver.EnsureDirectory(startupLiveLayout);
            HarnessPathResolver.EnsureDirectories(startupHarnessLayout);
            LongRunArtifacts.InitializeSessionArtifacts(startupRuntimeMissingRoot, "startup-runtime-missing-session", "boot-to-long-run", "headless");
            WriteRuntimeConfigFixture(startupConfiguration, modsRoot, harnessEnabled: true);
            File.WriteAllBytes(Path.Combine(modsRoot, startupConfiguration.AiCompanionMod.PckName), Array.Empty<byte>());
            File.WriteAllBytes(Path.Combine(modsRoot, "sts2-mod-ai-companion.dll"), Array.Empty<byte>());
            File.WriteAllText(
                Path.Combine(settingsRoot, "settings.save"),
                """{"mod_settings":{"mods_enabled":true}}""");
            File.WriteAllText(
                Path.Combine(logsRoot, "godot.log"),
                "[INFO] No ModInitializerAttribute detected. Calling Harmony.PatchAll for Sts2ModAiCompanion.Mod");
            var missingEvidence = LongRunArtifacts.RecordStartupRuntimeEvidence(
                startupRuntimeMissingRoot,
                startupConfiguration,
                startupLiveLayout,
                startupHarnessLayout);
            var missingSupervisor = LongRunArtifacts.RefreshSupervisorState(startupRuntimeMissingRoot);
            Assert(string.Equals(missingEvidence.Diagnosis, "runtime-bootstrap-missing", StringComparison.OrdinalIgnoreCase), "PatchAll-only startup evidence without runtime markers should diagnose runtime-bootstrap-missing.");
            Assert(missingSupervisor.Blockers.Contains("startup-runtime-bootstrap-missing", StringComparer.OrdinalIgnoreCase), "Supervisor should surface runtime-bootstrap-missing as a blocker when observer/export never starts.");
        }
        finally
        {
            if (Directory.Exists(startupRuntimeMissingRoot))
            {
                Directory.Delete(startupRuntimeMissingRoot, recursive: true);
            }
        }

        var startupRuntimeSentinelOnlyRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-startup-runtime-sentinel-only-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(startupRuntimeSentinelOnlyRoot);
            var gameRoot = Path.Combine(startupRuntimeSentinelOnlyRoot, "game");
            var modsRoot = Path.Combine(gameRoot, "mods");
            var userDataRoot = Path.Combine(startupRuntimeSentinelOnlyRoot, "userdata");
            var logsRoot = Path.Combine(userDataRoot, "logs");
            var settingsRoot = Path.Combine(userDataRoot, "steam", "self-test");
            Directory.CreateDirectory(modsRoot);
            Directory.CreateDirectory(logsRoot);
            Directory.CreateDirectory(settingsRoot);
            var startupConfiguration = ScaffoldConfiguration.CreateLocalDefault() with
            {
                GamePaths = new GamePathOptions
                {
                    GameDirectory = gameRoot,
                    UserDataRoot = userDataRoot,
                    SteamAccountId = "self-test",
                    ProfileIndex = 1,
                    ArtifactsRoot = "artifacts",
                },
            };
            var startupLiveLayout = LiveExportPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.LiveExport);
            var startupHarnessLayout = HarnessPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.Harness);
            LiveExportPathResolver.EnsureDirectory(startupLiveLayout);
            HarnessPathResolver.EnsureDirectories(startupHarnessLayout);
            LongRunArtifacts.InitializeSessionArtifacts(startupRuntimeSentinelOnlyRoot, "startup-runtime-sentinel-only-session", "boot-to-long-run", "headless");
            WriteRuntimeConfigFixture(startupConfiguration, modsRoot, harnessEnabled: true);
            File.WriteAllBytes(Path.Combine(modsRoot, startupConfiguration.AiCompanionMod.PckName), Array.Empty<byte>());
            File.WriteAllBytes(Path.Combine(modsRoot, "sts2-mod-ai-companion.dll"), Array.Empty<byte>());
            File.WriteAllText(
                Path.Combine(settingsRoot, "settings.save"),
                """{"mod_settings":{"mods_enabled":true}}""");
            var launchIssuedAt = DateTimeOffset.UtcNow;
            var startupRuntimeConfig = EnsureStartupRuntimeConfig(
                startupConfiguration,
                "startup-runtime-sentinel-only-session",
                "startup-runtime-sentinel-only-run",
                launchIssuedAt);
            WriteStartupSentinelFixture(
                startupConfiguration.GamePaths,
                startupRuntimeConfig.SentinelRelativePath,
                "startup-runtime-sentinel-only-session",
                "startup-runtime-sentinel-only-run",
                startupRuntimeConfig.LaunchToken);
            File.WriteAllText(
                Path.Combine(logsRoot, "godot.log"),
                string.Join(
                    Environment.NewLine,
                    new[]
                    {
                        "[INFO] [Startup] Time to main menu (Godot ticks): 12743ms",
                        "[INFO] [Startup] Time to main menu: 12,869ms",
                    }));
            var sentinelOnlyEvidence = LongRunArtifacts.RecordStartupRuntimeEvidence(
                startupRuntimeSentinelOnlyRoot,
                startupConfiguration,
                startupLiveLayout,
                startupHarnessLayout);
            Assert(string.Equals(sentinelOnlyEvidence.Diagnosis, "runtime-bootstrap-missing", StringComparison.OrdinalIgnoreCase), "Matching current-execution sentinel without exporter markers should diagnose runtime-bootstrap-missing.");
            Assert(sentinelOnlyEvidence.SentinelPresent && sentinelOnlyEvidence.SentinelLaunchTokenMatch, "Current-execution sentinel should be present and match the launch token.");
            Assert(sentinelOnlyEvidence.EverSawCurrentExecutionSentinel, "Sentinel-only startup evidence should preserve current-execution sentinel visibility in the reviewer summary.");
        }
        finally
        {
            if (Directory.Exists(startupRuntimeSentinelOnlyRoot))
            {
                Directory.Delete(startupRuntimeSentinelOnlyRoot, recursive: true);
            }
        }

        var startupRuntimeBridgeMissingRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-startup-runtime-bridge-missing-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(startupRuntimeBridgeMissingRoot);
            var gameRoot = Path.Combine(startupRuntimeBridgeMissingRoot, "game");
            var modsRoot = Path.Combine(gameRoot, "mods");
            var userDataRoot = Path.Combine(startupRuntimeBridgeMissingRoot, "userdata");
            var logsRoot = Path.Combine(userDataRoot, "logs");
            var settingsRoot = Path.Combine(userDataRoot, "steam", "self-test");
            Directory.CreateDirectory(modsRoot);
            Directory.CreateDirectory(logsRoot);
            Directory.CreateDirectory(settingsRoot);
            var startupConfiguration = ScaffoldConfiguration.CreateLocalDefault() with
            {
                GamePaths = new GamePathOptions
                {
                    GameDirectory = gameRoot,
                    UserDataRoot = userDataRoot,
                    SteamAccountId = "self-test",
                    ProfileIndex = 1,
                    ArtifactsRoot = "artifacts",
                },
            };
            var startupLiveLayout = LiveExportPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.LiveExport);
            var startupHarnessLayout = HarnessPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.Harness);
            LiveExportPathResolver.EnsureDirectory(startupLiveLayout);
            HarnessPathResolver.EnsureDirectories(startupHarnessLayout);
            LongRunArtifacts.InitializeSessionArtifacts(startupRuntimeBridgeMissingRoot, "startup-runtime-bridge-missing-session", "boot-to-long-run", "headless");
            WriteRuntimeConfigFixture(startupConfiguration, modsRoot, harnessEnabled: true);
            File.WriteAllBytes(Path.Combine(modsRoot, startupConfiguration.AiCompanionMod.PckName), Array.Empty<byte>());
            File.WriteAllBytes(Path.Combine(modsRoot, "sts2-mod-ai-companion.dll"), Array.Empty<byte>());
            File.WriteAllText(
                Path.Combine(settingsRoot, "settings.save"),
                """{"mod_settings":{"mods_enabled":true}}""");
            var launchIssuedAt = DateTimeOffset.UtcNow;
            var startupRuntimeConfig = EnsureStartupRuntimeConfig(
                startupConfiguration,
                "startup-runtime-bridge-missing-session",
                "startup-runtime-bridge-missing-run",
                launchIssuedAt);
            WriteStartupSentinelFixture(
                startupConfiguration.GamePaths,
                startupRuntimeConfig.SentinelRelativePath,
                "startup-runtime-bridge-missing-session",
                "startup-runtime-bridge-missing-run",
                startupRuntimeConfig.LaunchToken);
            File.WriteAllText(
                Path.Combine(modsRoot, startupConfiguration.AiCompanionMod.RuntimeLogFileName),
                "[STS2 Mod AI Companion] runtime exporter initialized. live_root=C:\\fake\\live");
            File.WriteAllText(
                Path.Combine(logsRoot, "godot.log"),
                "[INFO] [Startup] Time to main menu: 12,869ms");
            var bridgeMissingEvidence = LongRunArtifacts.RecordStartupRuntimeEvidence(
                startupRuntimeBridgeMissingRoot,
                startupConfiguration,
                startupLiveLayout,
                startupHarnessLayout);
            var bridgeMissingSupervisor = LongRunArtifacts.RefreshSupervisorState(startupRuntimeBridgeMissingRoot);
            Assert(string.Equals(bridgeMissingEvidence.Diagnosis, "observer-bootstrap-bridge-missing", StringComparison.OrdinalIgnoreCase), "Exporter markers without observer artifacts should diagnose observer-bootstrap-bridge-missing.");
            Assert(bridgeMissingEvidence.NoSnapshotEvidence && bridgeMissingEvidence.EverSawRuntimeExporter, "Observer-bootstrap-bridge-missing should keep exporter progress while still showing that no fresh snapshot evidence arrived.");
            Assert(bridgeMissingSupervisor.Blockers.Contains("startup-observer-bootstrap-bridge-missing", StringComparer.OrdinalIgnoreCase), "Supervisor should surface observer-bootstrap-bridge-missing as a blocker.");
        }
        finally
        {
            if (Directory.Exists(startupRuntimeBridgeMissingRoot))
            {
                Directory.Delete(startupRuntimeBridgeMissingRoot, recursive: true);
            }
        }

        var startupRuntimeScanMissingRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-startup-runtime-scan-missing-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(startupRuntimeScanMissingRoot);
            var gameRoot = Path.Combine(startupRuntimeScanMissingRoot, "game");
            var modsRoot = Path.Combine(gameRoot, "mods");
            var userDataRoot = Path.Combine(startupRuntimeScanMissingRoot, "userdata");
            var logsRoot = Path.Combine(userDataRoot, "logs");
            var settingsRoot = Path.Combine(userDataRoot, "steam", "self-test");
            Directory.CreateDirectory(modsRoot);
            Directory.CreateDirectory(logsRoot);
            Directory.CreateDirectory(settingsRoot);
            var startupConfiguration = ScaffoldConfiguration.CreateLocalDefault() with
            {
                GamePaths = new GamePathOptions
                {
                    GameDirectory = gameRoot,
                    UserDataRoot = userDataRoot,
                    SteamAccountId = "self-test",
                    ProfileIndex = 1,
                    ArtifactsRoot = "artifacts",
                },
            };
            var startupLiveLayout = LiveExportPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.LiveExport);
            var startupHarnessLayout = HarnessPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.Harness);
            LiveExportPathResolver.EnsureDirectory(startupLiveLayout);
            HarnessPathResolver.EnsureDirectories(startupHarnessLayout);
            LongRunArtifacts.InitializeSessionArtifacts(startupRuntimeScanMissingRoot, "startup-runtime-scan-missing-session", "boot-to-long-run", "headless");
            WriteRuntimeConfigFixture(startupConfiguration, modsRoot, harnessEnabled: true);
            File.WriteAllBytes(Path.Combine(modsRoot, startupConfiguration.AiCompanionMod.PckName), Array.Empty<byte>());
            File.WriteAllBytes(Path.Combine(modsRoot, "sts2-mod-ai-companion.dll"), Array.Empty<byte>());
            File.WriteAllText(
                Path.Combine(settingsRoot, "settings.save"),
                """{"mod_settings":{"mods_enabled":true}}""");
            var launchIssuedAt = DateTimeOffset.UtcNow;
            var startupRuntimeConfig = EnsureStartupRuntimeConfig(
                startupConfiguration,
                "startup-runtime-scan-missing-session",
                "startup-runtime-scan-missing-run",
                launchIssuedAt);
            WriteStartupSentinelFixture(
                startupConfiguration.GamePaths,
                startupRuntimeConfig.SentinelRelativePath,
                "stale-session",
                "stale-run",
                "stale-token");
            File.WriteAllText(
                Path.Combine(logsRoot, "godot.log"),
                string.Join(
                    Environment.NewLine,
                    new[]
                    {
                        "[INFO] [Startup] Time to main menu (Godot ticks): 12743ms",
                        "[INFO] [Startup] Time to main menu: 12,869ms",
                    }));
            var scanMissingEvidence = LongRunArtifacts.RecordStartupRuntimeEvidence(
                startupRuntimeScanMissingRoot,
                startupConfiguration,
                startupLiveLayout,
                startupHarnessLayout);
            var scanMissingSupervisor = LongRunArtifacts.RefreshSupervisorState(startupRuntimeScanMissingRoot);
            Assert(string.Equals(scanMissingEvidence.Diagnosis, "loader-entry-before-initializer-not-proven", StringComparison.OrdinalIgnoreCase), "Main-menu startup without sentinel or exporter markers should diagnose loader-entry-before-initializer-not-proven.");
            Assert(scanMissingEvidence.SentinelPresent && !scanMissingEvidence.SentinelLaunchTokenMatch, "Stale sentinel should remain visible in evidence but should not match the current execution.");
            Assert(!scanMissingEvidence.EverSawCurrentExecutionSentinel, "Mismatched sentinel should stay visible without being promoted to a current-execution sentinel hit.");
            Assert(string.Equals(scanMissingEvidence.FailureEdge, "OneTimeInitialization.ExecuteEssential->ModManager.Initialize->LoadModsInDirRecursive(mods)", StringComparison.OrdinalIgnoreCase), "Main-menu startup without mod-loader signal should point at the ModManager scan edge.");
            Assert(scanMissingSupervisor.Blockers.Contains("startup-loader-entry-before-initializer-not-proven", StringComparer.OrdinalIgnoreCase), "Supervisor should surface loader-entry-before-initializer-not-proven as a startup blocker.");
        }
        finally
        {
            if (Directory.Exists(startupRuntimeScanMissingRoot))
            {
                Directory.Delete(startupRuntimeScanMissingRoot, recursive: true);
            }
        }

        var startupRuntimeTimelineRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-startup-runtime-timeline-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(startupRuntimeTimelineRoot);
            var gameRoot = Path.Combine(startupRuntimeTimelineRoot, "game");
            var modsRoot = Path.Combine(gameRoot, "mods");
            var userDataRoot = Path.Combine(startupRuntimeTimelineRoot, "userdata");
            var logsRoot = Path.Combine(userDataRoot, "logs");
            var settingsRoot = Path.Combine(userDataRoot, "steam", "self-test");
            Directory.CreateDirectory(modsRoot);
            Directory.CreateDirectory(logsRoot);
            Directory.CreateDirectory(settingsRoot);
            var startupConfiguration = ScaffoldConfiguration.CreateLocalDefault() with
            {
                GamePaths = new GamePathOptions
                {
                    GameDirectory = gameRoot,
                    UserDataRoot = userDataRoot,
                    SteamAccountId = "self-test",
                    ProfileIndex = 1,
                    ArtifactsRoot = "artifacts",
                },
            };
            var startupLiveLayout = LiveExportPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.LiveExport);
            var startupHarnessLayout = HarnessPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.Harness);
            LiveExportPathResolver.EnsureDirectory(startupLiveLayout);
            HarnessPathResolver.EnsureDirectories(startupHarnessLayout);
            LongRunArtifacts.InitializeSessionArtifacts(startupRuntimeTimelineRoot, "startup-runtime-timeline-session", "boot-to-long-run", "headless");
            WriteRuntimeConfigFixture(startupConfiguration, modsRoot, harnessEnabled: true);
            File.WriteAllBytes(Path.Combine(modsRoot, startupConfiguration.AiCompanionMod.PckName), Array.Empty<byte>());
            File.WriteAllBytes(Path.Combine(modsRoot, "sts2-mod-ai-companion.dll"), Array.Empty<byte>());
            File.WriteAllText(Path.Combine(settingsRoot, "settings.save"), """{"mod_settings":{"mods_enabled":true}}""");

            var firstLaunchIssuedAt = DateTimeOffset.UtcNow;
            var firstRuntimeConfig = EnsureStartupRuntimeConfig(
                startupConfiguration,
                "startup-runtime-timeline-session",
                "startup-runtime-timeline-run-1",
                firstLaunchIssuedAt);
            LongRunArtifacts.RecordStartupLogBaseline(
                startupRuntimeTimelineRoot,
                startupConfiguration,
                "startup-runtime-timeline-run-1",
                firstRuntimeConfig.LaunchToken,
                firstLaunchIssuedAt);
            File.WriteAllText(
                Path.Combine(logsRoot, "godot.log"),
                "[INFO] [Startup] Time to main menu: 12,869ms");
            var positiveCapture = LongRunArtifacts.RecordStartupRuntimeEvidence(
                startupRuntimeTimelineRoot,
                startupConfiguration,
                startupLiveLayout,
                startupHarnessLayout,
                captureReason: "synthetic-early-main-menu");
            Assert(positiveCapture.GodotReachedMainMenu, "Synthetic startup timeline first capture should retain the early main-menu positive.");

            Thread.Sleep(20);
            var secondLaunchIssuedAt = DateTimeOffset.UtcNow;
            var secondRuntimeConfig = EnsureStartupRuntimeConfig(
                startupConfiguration,
                "startup-runtime-timeline-session",
                "startup-runtime-timeline-run-2",
                secondLaunchIssuedAt);
            LongRunArtifacts.RecordStartupLogBaseline(
                startupRuntimeTimelineRoot,
                startupConfiguration,
                "startup-runtime-timeline-run-2",
                secondRuntimeConfig.LaunchToken,
                secondLaunchIssuedAt);
            File.WriteAllText(Path.Combine(logsRoot, "godot.log"), string.Empty);
            var timelineEvidence = LongRunArtifacts.RecordStartupRuntimeEvidence(
                startupRuntimeTimelineRoot,
                startupConfiguration,
                startupLiveLayout,
                startupHarnessLayout,
                captureReason: "synthetic-late-negative");
            Assert(string.Equals(timelineEvidence.Diagnosis, "loader-entry-before-initializer-not-proven", StringComparison.OrdinalIgnoreCase), "Latest startup timeline diagnosis should stay conservative after the later negative capture.");
            Assert(timelineEvidence.CaptureCount == 2, "Startup runtime timeline summary should retain both captures from the same root.");
            Assert(timelineEvidence.EverReachedMainMenu, "Startup runtime summary should preserve the earlier main-menu positive.");
            Assert(timelineEvidence.FirstPositiveCaptureAt is not null && string.Equals(timelineEvidence.FirstPositiveReason, "godot-reached-main-menu", StringComparison.Ordinal), "Startup runtime summary should record the first positive capture and reason.");
            Assert(ReadNdjsonFixture<GuiSmokeStartupRuntimeCapture>(Path.Combine(startupRuntimeTimelineRoot, "startup-runtime-captures.ndjson")).Count == 2, "Startup runtime capture sequence should persist every capture in order.");
        }
        finally
        {
            if (Directory.Exists(startupRuntimeTimelineRoot))
            {
                Directory.Delete(startupRuntimeTimelineRoot, recursive: true);
            }
        }

        var startupRuntimeStaleSnapshotRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-startup-runtime-stale-snapshot-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(startupRuntimeStaleSnapshotRoot);
            var gameRoot = Path.Combine(startupRuntimeStaleSnapshotRoot, "game");
            var modsRoot = Path.Combine(gameRoot, "mods");
            var userDataRoot = Path.Combine(startupRuntimeStaleSnapshotRoot, "userdata");
            var logsRoot = Path.Combine(userDataRoot, "logs");
            var settingsRoot = Path.Combine(userDataRoot, "steam", "self-test");
            Directory.CreateDirectory(modsRoot);
            Directory.CreateDirectory(logsRoot);
            Directory.CreateDirectory(settingsRoot);
            var startupConfiguration = ScaffoldConfiguration.CreateLocalDefault() with
            {
                GamePaths = new GamePathOptions
                {
                    GameDirectory = gameRoot,
                    UserDataRoot = userDataRoot,
                    SteamAccountId = "self-test",
                    ProfileIndex = 1,
                    ArtifactsRoot = "artifacts",
                },
            };
            var startupLiveLayout = LiveExportPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.LiveExport);
            var startupHarnessLayout = HarnessPathResolver.Resolve(startupConfiguration.GamePaths, startupConfiguration.Harness);
            LiveExportPathResolver.EnsureDirectory(startupLiveLayout);
            HarnessPathResolver.EnsureDirectories(startupHarnessLayout);
            LongRunArtifacts.InitializeSessionArtifacts(startupRuntimeStaleSnapshotRoot, "startup-runtime-stale-snapshot-session", "boot-to-long-run", "headless");
            WriteRuntimeConfigFixture(startupConfiguration, modsRoot, harnessEnabled: true);
            File.WriteAllBytes(Path.Combine(modsRoot, startupConfiguration.AiCompanionMod.PckName), Array.Empty<byte>());
            File.WriteAllBytes(Path.Combine(modsRoot, "sts2-mod-ai-companion.dll"), Array.Empty<byte>());
            File.WriteAllText(Path.Combine(settingsRoot, "settings.save"), """{"mod_settings":{"mods_enabled":true}}""");
            var launchIssuedAt = DateTimeOffset.UtcNow;
            var runtimeConfig = EnsureStartupRuntimeConfig(
                startupConfiguration,
                "startup-runtime-stale-snapshot-session",
                "startup-runtime-stale-snapshot-run",
                launchIssuedAt);
            LongRunArtifacts.RecordStartupLogBaseline(
                startupRuntimeStaleSnapshotRoot,
                startupConfiguration,
                "startup-runtime-stale-snapshot-run",
                runtimeConfig.LaunchToken,
                launchIssuedAt);
            File.WriteAllText(
                Path.Combine(logsRoot, "godot.log"),
                "[INFO] [Startup] Time to main menu: 12,869ms");
            var staleSnapshotEvidence = LongRunArtifacts.RecordStartupRuntimeEvidence(
                startupRuntimeStaleSnapshotRoot,
                startupConfiguration,
                startupLiveLayout,
                startupHarnessLayout,
                captureReason: "synthetic-stale-snapshot",
                staleSnapshotObserved: true);
            Assert(staleSnapshotEvidence.StaleSnapshotObserved, "Stale observer snapshot should remain visible in startup runtime evidence.");
            Assert(!staleSnapshotEvidence.FreshSnapshotPresent, "Stale startup snapshot evidence must not be promoted to fresh snapshot evidence.");
            Assert(!staleSnapshotEvidence.NoSnapshotEvidence, "Stale snapshot evidence should stay distinct from the 'no snapshot evidence' bucket.");
            Assert(staleSnapshotEvidence.EverSawStaleSnapshot && !staleSnapshotEvidence.EverSawFreshSnapshot, "Startup runtime summary should distinguish stale-only snapshot history from fresh snapshots.");
        }
        finally
        {
            if (Directory.Exists(startupRuntimeStaleSnapshotRoot))
            {
                Directory.Delete(startupRuntimeStaleSnapshotRoot, recursive: true);
            }
        }

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
