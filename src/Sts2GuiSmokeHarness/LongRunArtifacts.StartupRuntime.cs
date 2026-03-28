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

static partial class LongRunArtifacts
{
    public static GuiSmokeStartupRuntimeEvidence RecordStartupRuntimeEvidence(
        string sessionRoot,
        ScaffoldConfiguration configuration,
        LiveExportLayout liveLayout,
        HarnessQueueLayout harnessLayout,
        string stage = "manual-clean-boot-runtime-evidence",
        string? captureReason = null,
        bool staleSnapshotObserved = false)
    {
        var latestEvidence = BuildStartupRuntimeEvidence(
            sessionRoot,
            configuration,
            liveLayout,
            harnessLayout,
            stage,
            captureReason,
            staleSnapshotObserved);
        File.WriteAllText(latestEvidence.RuntimeLogTailPath, BuildLogTailBody(latestEvidence.RuntimeLogPath, latestEvidence.RuntimeLogMatches), new UTF8Encoding(false));
        File.WriteAllText(latestEvidence.GodotLogTailPath, BuildLogTailBody(latestEvidence.GodotLogPath, latestEvidence.GodotLogMatches), new UTF8Encoding(false));
        File.WriteAllText(latestEvidence.RuntimeLogDeltaPath, BuildLogTailBody(latestEvidence.RuntimeLogPath, latestEvidence.RuntimeLogDeltaMatches), new UTF8Encoding(false));
        File.WriteAllText(latestEvidence.GodotLogDeltaPath, BuildLogTailBody(latestEvidence.GodotLogPath, latestEvidence.GodotLogDeltaMatches), new UTF8Encoding(false));

        AppendNdjson(
            GetStartupRuntimeCapturesPath(sessionRoot),
            CreateStartupRuntimeCapture(latestEvidence));

        var captures = ReadStartupRuntimeCaptures(sessionRoot, latestEvidence);
        var evidence = ApplyStartupRuntimeCaptureSummary(latestEvidence, captures);
        WriteJsonAtomicWithRetry(GetStartupRuntimeEvidencePath(sessionRoot), evidence, GuiSmokeShared.JsonOptions);

        AppendPrevalidationNoteWithoutRefresh(sessionRoot, $"startup-runtime-diagnosis:{evidence.Diagnosis}");
        if (evidence.FirstPositiveCaptureAt is { } firstPositiveAt && !string.IsNullOrWhiteSpace(evidence.FirstPositiveReason))
        {
            AppendPrevalidationNoteWithoutRefresh(sessionRoot, $"startup-runtime-first-positive:{firstPositiveAt:O}:{evidence.FirstPositiveReason}");
        }

        if (evidence.EverSawStaleSnapshot)
        {
            AppendPrevalidationNoteWithoutRefresh(sessionRoot, "startup-runtime-ever-stale-snapshot:true");
        }

        RecordStartupStage(
            sessionRoot,
            stage,
            "finished",
            evidence.Diagnosis,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["captureReason"] = evidence.LatestCaptureReason,
                ["captureCount"] = evidence.CaptureCount.ToString(),
                ["firstPositiveCaptureAt"] = evidence.FirstPositiveCaptureAt?.ToString("O"),
                ["firstPositiveReason"] = evidence.FirstPositiveReason,
                ["everReachedMainMenu"] = evidence.EverReachedMainMenu.ToString(),
                ["everSawCurrentExecutionSentinel"] = evidence.EverSawCurrentExecutionSentinel.ToString(),
                ["everSawRuntimeExporter"] = evidence.EverSawRuntimeExporter.ToString(),
                ["everSawHarnessBridge"] = evidence.EverSawHarnessBridge.ToString(),
                ["everSawFreshSnapshot"] = evidence.EverSawFreshSnapshot.ToString(),
                ["everSawStaleSnapshot"] = evidence.EverSawStaleSnapshot.ToString(),
                ["everSawLoaderSignal"] = evidence.EverSawLoaderSignal.ToString(),
                ["failureEdge"] = evidence.FailureEdge,
                ["runtimeLogPresent"] = evidence.RuntimeLogPresent.ToString(),
                ["runtimeLogDeltaMatches"] = evidence.RuntimeLogDeltaMatches.Count.ToString(),
                ["runtimeLogDeltaTreatedAsCurrentExecution"] = evidence.RuntimeLogDeltaTreatedAsCurrentExecution.ToString(),
                ["moduleInitializerBootstrapLogged"] = evidence.ModuleInitializerBootstrapLogged.ToString(),
                ["runtimeExporterInitializedLogged"] = evidence.RuntimeExporterInitializedLogged.ToString(),
                ["harnessBridgeInitializeLogged"] = evidence.HarnessBridgeInitializeLogged.ToString(),
                ["godotLogPresent"] = evidence.GodotLogPresent.ToString(),
                ["godotLogDeltaMatches"] = evidence.GodotLogDeltaMatches.Count.ToString(),
                ["godotLogDeltaTreatedAsCurrentExecution"] = evidence.GodotLogDeltaTreatedAsCurrentExecution.ToString(),
                ["godotReachedMainMenu"] = evidence.GodotReachedMainMenu.ToString(),
                ["loaderSawAnyModLoaderSignal"] = evidence.LoaderSawAnyModLoaderSignal.ToString(),
                ["loaderSawModsDirectoryScan"] = evidence.LoaderSawModsDirectoryScan.ToString(),
                ["loaderSawCompanionPckScan"] = evidence.LoaderSawCompanionPckScan.ToString(),
                ["loaderSawCompanionAssembly"] = evidence.LoaderSawCompanionAssembly.ToString(),
                ["loaderSawPatchAll"] = evidence.LoaderSawPatchAll.ToString(),
                ["loaderSawPatchAllFailure"] = evidence.LoaderSawPatchAllFailure.ToString(),
                ["loaderSawModInitialization"] = evidence.LoaderSawModInitialization.ToString(),
                ["loaderSawModLoadFailure"] = evidence.LoaderSawModLoadFailure.ToString(),
                ["settingsModsEnabled"] = evidence.SettingsModsEnabled.ToString(),
                ["settingsCompanionDisabled"] = evidence.SettingsCompanionDisabled.ToString(),
                ["runtimeConfigEnabled"] = evidence.RuntimeConfigEnabled.ToString(),
                ["runtimeConfigHarnessEnabled"] = evidence.RuntimeConfigHarnessEnabled.ToString(),
                ["companionPckPresent"] = evidence.CompanionPckPresent.ToString(),
                ["companionDllPresent"] = evidence.CompanionDllPresent.ToString(),
                ["companionRuntimeConfigPresent"] = evidence.CompanionRuntimeConfigPresent.ToString(),
                ["liveSnapshotPresent"] = evidence.LiveSnapshotPresent.ToString(),
                ["freshSnapshotPresent"] = evidence.FreshSnapshotPresent.ToString(),
                ["staleSnapshotObserved"] = evidence.StaleSnapshotObserved.ToString(),
                ["noSnapshotEvidence"] = evidence.NoSnapshotEvidence.ToString(),
                ["harnessInventoryPresent"] = evidence.HarnessInventoryPresent.ToString(),
                ["harnessStatusPresent"] = evidence.HarnessStatusPresent.ToString(),
                ["diagnosis"] = evidence.Diagnosis,
            });
        return evidence;
    }

    public static GuiSmokeStartupLogBaseline RecordStartupLogBaseline(
        string sessionRoot,
        ScaffoldConfiguration configuration,
        string runId,
        string launchToken,
        DateTimeOffset launchIssuedAtUtc)
    {
        var modsRoot = Path.Combine(configuration.GamePaths.GameDirectory, "mods");
        var runtimeLogPath = Path.Combine(modsRoot, configuration.AiCompanionMod.RuntimeLogFileName);
        var godotLogPath = Path.Combine(configuration.GamePaths.UserDataRoot, "logs", "godot.log");
        var runtimeInfo = File.Exists(runtimeLogPath) ? new FileInfo(runtimeLogPath) : null;
        var godotInfo = File.Exists(godotLogPath) ? new FileInfo(godotLogPath) : null;
        var baseline = new GuiSmokeStartupLogBaseline(
            RecordedAt: DateTimeOffset.UtcNow,
            SessionId: Path.GetFileName(sessionRoot),
            RunId: runId,
            LaunchToken: launchToken,
            LaunchIssuedAtUtc: launchIssuedAtUtc,
            RuntimeLogPath: runtimeLogPath,
            RuntimeLogPresent: runtimeInfo is not null,
            RuntimeLogSizeBytes: runtimeInfo?.Length ?? 0,
            RuntimeLogLastWriteAt: runtimeInfo?.LastWriteTimeUtc,
            GodotLogPath: godotLogPath,
            GodotLogPresent: godotInfo is not null,
            GodotLogSizeBytes: godotInfo?.Length ?? 0,
            GodotLogLastWriteAt: godotInfo?.LastWriteTimeUtc);
        WriteJsonAtomicWithRetry(GetStartupLogBaselinePath(sessionRoot), baseline, GuiSmokeShared.JsonOptions);
        return baseline;
    }

    private static GuiSmokeStartupRuntimeEvidence BuildStartupRuntimeEvidence(
        string sessionRoot,
        ScaffoldConfiguration configuration,
        LiveExportLayout liveLayout,
        HarnessQueueLayout harnessLayout,
        string stage,
        string? captureReason,
        bool staleSnapshotObservedOverride)
    {
        var companionPckName = configuration.AiCompanionMod.PckName;
        var companionPckBaseName = Path.GetFileNameWithoutExtension(companionPckName);
        var expectedAssemblyFileName = companionPckBaseName + ".dll";
        var modsRoot = Path.Combine(configuration.GamePaths.GameDirectory, "mods");
        var runtimeLogPath = Path.Combine(modsRoot, configuration.AiCompanionMod.RuntimeLogFileName);
        var runtimeConfigPath = Path.Combine(modsRoot, configuration.AiCompanionMod.RuntimeConfigFileName);
        var companionPckPath = Path.Combine(modsRoot, companionPckName);
        var companionDllPath = Path.Combine(modsRoot, expectedAssemblyFileName);
        var packagedManifestPath = ResolvePackagedManifestPath(sessionRoot);
        var settingsPath = ResolveSettingsSavePath(configuration.GamePaths);
        var godotLogPath = Path.Combine(configuration.GamePaths.UserDataRoot, "logs", "godot.log");
        var startupLogBaseline = TryReadJson<GuiSmokeStartupLogBaseline>(GetStartupLogBaselinePath(sessionRoot));
        var runtimeLogMatches = ReadRelevantLogTail(runtimeLogPath, IsRelevantRuntimeLogLine);
        var godotLogMatches = ReadRelevantLogTail(godotLogPath, IsRelevantGodotLogLine);
        var runtimeLogDelta = startupLogBaseline is null
            ? new GuiSmokeRelevantLogDelta(Array.Empty<string>(), TreatedAsCurrentExecution: false)
            : ReadRelevantLogDelta(
                runtimeLogPath,
                startupLogBaseline.RuntimeLogSizeBytes,
                IsRelevantRuntimeLogLine,
                startupLogBaseline.RuntimeLogPresent);
        var godotLogDelta = startupLogBaseline is null
            ? new GuiSmokeRelevantLogDelta(Array.Empty<string>(), TreatedAsCurrentExecution: false)
            : ReadRelevantLogDelta(
                godotLogPath,
                startupLogBaseline.GodotLogSizeBytes,
                IsRelevantGodotLogLine,
                startupLogBaseline.GodotLogPresent);
        var runtimeDiagnosticLines = startupLogBaseline is null ? runtimeLogMatches : runtimeLogDelta.Matches;
        var godotDiagnosticLines = startupLogBaseline is null ? godotLogMatches : godotLogDelta.Matches;
        var runtimeLogInfo = File.Exists(runtimeLogPath) ? new FileInfo(runtimeLogPath) : null;
        var godotLogInfo = File.Exists(godotLogPath) ? new FileInfo(godotLogPath) : null;
        var moduleInitializerLogged = runtimeDiagnosticLines.Any(static line => line.Contains("module initializer bootstrap result", StringComparison.OrdinalIgnoreCase));
        var runtimeExporterLogged = runtimeDiagnosticLines.Any(static line => line.Contains("runtime exporter initialized", StringComparison.OrdinalIgnoreCase));
        var harnessBridgeLogged = runtimeDiagnosticLines.Any(static line => line.Contains("harness bridge initialize result", StringComparison.OrdinalIgnoreCase));
        var godotReachedMainMenu = godotDiagnosticLines.Any(static line => line.Contains("Time to main menu", StringComparison.OrdinalIgnoreCase));
        var loaderSawModsDirectoryScan = godotDiagnosticLines.Any(static line => line.Contains("Found mod pck file", StringComparison.OrdinalIgnoreCase));
        var loaderSawCompanionPckScan = godotDiagnosticLines.Any(line =>
            line.Contains("Found mod pck file", StringComparison.OrdinalIgnoreCase)
            && line.Contains(companionPckName, StringComparison.OrdinalIgnoreCase));
        var loaderSawCompanionSkippedWarning = godotDiagnosticLines.Any(line =>
            line.Contains("Skipping loading mod", StringComparison.OrdinalIgnoreCase)
            && line.Contains(companionPckName, StringComparison.OrdinalIgnoreCase)
            && line.Contains("mods warning", StringComparison.OrdinalIgnoreCase));
        var loaderSawCompanionDisabled = godotDiagnosticLines.Any(line =>
            line.Contains("Skipping loading mod", StringComparison.OrdinalIgnoreCase)
            && line.Contains(companionPckName, StringComparison.OrdinalIgnoreCase)
            && line.Contains("disabled in settings", StringComparison.OrdinalIgnoreCase));
        var loaderSawCompanionDuplicate = godotDiagnosticLines.Any(line =>
            line.Contains("Tried to load mod with PCK name", StringComparison.OrdinalIgnoreCase)
            && line.Contains(companionPckBaseName, StringComparison.OrdinalIgnoreCase));
        var loaderSawCompanionAssembly = godotDiagnosticLines.Any(line =>
            line.Contains("Loading assembly DLL", StringComparison.OrdinalIgnoreCase)
            && line.Contains(expectedAssemblyFileName, StringComparison.OrdinalIgnoreCase));
        var loaderSawInitializerCall = godotDiagnosticLines.Any(line =>
            line.Contains("Calling initializer method of type", StringComparison.OrdinalIgnoreCase)
            && (line.Contains(companionPckBaseName, StringComparison.OrdinalIgnoreCase)
                || line.Contains(configuration.AiCompanionMod.Name, StringComparison.OrdinalIgnoreCase)
                || line.Contains(expectedAssemblyFileName, StringComparison.OrdinalIgnoreCase)));
        var loaderSawNoModInitializerAttribute = godotDiagnosticLines.Any(static line =>
            line.Contains("No ModInitializerAttribute detected", StringComparison.OrdinalIgnoreCase));
        var loaderSawPatchAll = loaderSawNoModInitializerAttribute
                                || godotDiagnosticLines.Any(static line => line.Contains("Harmony.PatchAll", StringComparison.OrdinalIgnoreCase) || line.Contains("PatchAll", StringComparison.OrdinalIgnoreCase));
        var loaderSawPatchAllFailure = godotDiagnosticLines.Any(static line => line.Contains("Exception caught while trying to run PatchAll", StringComparison.OrdinalIgnoreCase));
        var loaderSawModLoadFailure = godotDiagnosticLines.Any(line =>
            line.Contains("Error loading mod", StringComparison.OrdinalIgnoreCase)
            && line.Contains(companionPckBaseName, StringComparison.OrdinalIgnoreCase));
        var loaderSawModInitialization = godotDiagnosticLines.Any(line =>
            line.Contains("Finished mod initialization", StringComparison.OrdinalIgnoreCase)
            && (line.Contains(companionPckBaseName, StringComparison.OrdinalIgnoreCase)
                || line.Contains(configuration.AiCompanionMod.Name, StringComparison.OrdinalIgnoreCase)));
        var loaderSawAnyModLoaderSignal = loaderSawModsDirectoryScan
                                          || loaderSawCompanionSkippedWarning
                                          || loaderSawCompanionDisabled
                                          || loaderSawCompanionDuplicate
                                          || loaderSawCompanionAssembly
                                          || loaderSawInitializerCall
                                          || loaderSawNoModInitializerAttribute
                                          || loaderSawPatchAll
                                          || loaderSawPatchAllFailure
                                          || loaderSawModInitialization
                                          || loaderSawModLoadFailure;
        var runtimeConfigPresent = File.Exists(runtimeConfigPath);
        var runtimeConfigRoot = TryReadJsonNode(runtimeConfigPath) as JsonObject;
        var runtimeConfigEnabled = TryReadBoolean(runtimeConfigRoot?["enabled"]) ?? false;
        var runtimeConfigHarnessEnabled = TryReadBoolean(runtimeConfigRoot?["harness"]?["enabled"]) ?? false;
        var runtimeConfigUserDataRoot = TryReadString(runtimeConfigRoot?["gamePaths"]?["userDataRoot"]);
        var startupSentinelRoot = runtimeConfigRoot?["startupSentinel"] as JsonObject;
        var startupSentinelSessionId = TryReadString(startupSentinelRoot?["sessionId"]);
        var startupSentinelRunId = TryReadString(startupSentinelRoot?["runId"]);
        var startupSentinelLaunchToken = TryReadString(startupSentinelRoot?["launchToken"]);
        var startupSentinelLaunchIssuedAtUtc = TryReadDateTimeOffset(startupSentinelRoot?["launchIssuedAtUtc"]);
        var startupSentinelRelativePath = TryReadString(startupSentinelRoot?["sentinelRelativePath"]);
        var startupSentinelEvidence = ReadStartupSentinelEvidence(
            runtimeConfigUserDataRoot,
            startupSentinelRelativePath,
            startupSentinelSessionId,
            startupSentinelRunId,
            startupSentinelLaunchToken);
        var currentExecutionSentinelObserved = startupSentinelEvidence.Present
                                               && startupSentinelEvidence.SessionMatch
                                               && startupSentinelEvidence.RunMatch
                                               && startupSentinelEvidence.LaunchTokenMatch;
        var settingsPresent = !string.IsNullOrWhiteSpace(settingsPath) && File.Exists(settingsPath);
        var settingsRoot = TryReadJsonNode(settingsPath) as JsonObject;
        var settingsModsEnabled = TryReadBoolean(settingsRoot?["mod_settings"]?["mods_enabled"]) ?? false;
        var settingsCompanionDisabled = IsCompanionDisabledInSettings(settingsRoot, companionPckBaseName);
        var packagedManifestPresent = File.Exists(packagedManifestPath);
        var packagedManifestRoot = TryReadJsonNode(packagedManifestPath) as JsonObject;
        var packagedManifestPckName = TryReadString(packagedManifestRoot?["pck_name"]);
        var liveSnapshotInfo = File.Exists(liveLayout.SnapshotPath) ? new FileInfo(liveLayout.SnapshotPath) : null;
        var liveSnapshotPresent = liveSnapshotInfo is not null;
        var freshSnapshotPresent = liveSnapshotInfo is not null && IsStartupArtifactFreshForCurrentRun(liveSnapshotInfo.LastWriteTimeUtc, startupLogBaseline?.LaunchIssuedAtUtc);
        var staleSnapshotObserved = staleSnapshotObservedOverride || (liveSnapshotInfo is not null && !freshSnapshotPresent);
        var noSnapshotEvidence = !freshSnapshotPresent && !staleSnapshotObserved;
        var harnessInventoryPresent = File.Exists(harnessLayout.InventoryPath);
        var harnessStatusPresent = File.Exists(harnessLayout.StatusPath);
        var expectedLoadChain = "NGame.GameStartup -> OneTimeInitialization.ExecuteEssential -> ModManager.Initialize -> LoadModsInDirRecursive(mods) -> TryLoadModFromPck -> AssemblyLoadContext.LoadFromAssemblyPath(primary dll) -> CLR ModuleInitializer(startup sentinel) -> Harmony.PatchAll -> RuntimeExportContext.EnsureInitialized";
        var failureEdge = DetermineStartupRuntimeFailureEdge(
            runtimeConfigPresent,
            runtimeConfigEnabled,
            settingsPresent,
            settingsModsEnabled,
            settingsCompanionDisabled,
            File.Exists(companionPckPath),
            File.Exists(companionDllPath),
            loaderSawPatchAllFailure,
            loaderSawModLoadFailure,
            moduleInitializerLogged,
            runtimeExporterLogged,
            harnessBridgeLogged,
            freshSnapshotPresent,
            harnessInventoryPresent,
            harnessStatusPresent,
            godotReachedMainMenu,
            loaderSawAnyModLoaderSignal,
            loaderSawModsDirectoryScan,
            loaderSawCompanionPckScan,
            loaderSawCompanionSkippedWarning,
            loaderSawCompanionDisabled,
            loaderSawCompanionDuplicate,
            loaderSawCompanionAssembly,
            loaderSawInitializerCall,
            loaderSawNoModInitializerAttribute,
            loaderSawModInitialization,
            loaderSawPatchAll,
            currentExecutionSentinelObserved);
        return new GuiSmokeStartupRuntimeEvidence(
            DateTimeOffset.UtcNow,
            expectedLoadChain,
            failureEdge,
            modsRoot,
            File.Exists(companionPckPath),
            File.Exists(companionDllPath),
            File.Exists(runtimeConfigPath),
            runtimeConfigPath,
            runtimeConfigPresent,
            runtimeConfigEnabled,
            runtimeConfigHarnessEnabled,
            settingsPath,
            settingsPresent,
            settingsModsEnabled,
            settingsCompanionDisabled,
            packagedManifestPath,
            packagedManifestPresent,
            packagedManifestPckName,
            expectedAssemblyFileName,
            GetStartupLogBaselinePath(sessionRoot),
            startupLogBaseline?.RecordedAt,
            startupSentinelSessionId,
            startupSentinelRunId,
            startupSentinelLaunchToken,
            startupSentinelLaunchIssuedAtUtc,
            startupSentinelRelativePath,
            runtimeLogPath,
            runtimeLogInfo is not null,
            runtimeLogInfo?.LastWriteTimeUtc,
            GetStartupRuntimeLogTailPath(sessionRoot),
            runtimeLogMatches,
            GetStartupRuntimeLogDeltaPath(sessionRoot),
            runtimeLogDelta.Matches,
            runtimeLogDelta.TreatedAsCurrentExecution,
            moduleInitializerLogged,
            runtimeExporterLogged,
            harnessBridgeLogged,
            godotLogPath,
            godotLogInfo is not null,
            godotLogInfo?.LastWriteTimeUtc,
            GetStartupGodotLogTailPath(sessionRoot),
            godotLogMatches,
            GetStartupGodotLogDeltaPath(sessionRoot),
            godotLogDelta.Matches,
            godotLogDelta.TreatedAsCurrentExecution,
            godotReachedMainMenu,
            loaderSawAnyModLoaderSignal,
            loaderSawModsDirectoryScan,
            loaderSawCompanionPckScan,
            loaderSawCompanionSkippedWarning,
            loaderSawCompanionDisabled,
            loaderSawCompanionDuplicate,
            loaderSawCompanionAssembly,
            loaderSawInitializerCall,
            loaderSawNoModInitializerAttribute,
            loaderSawModInitialization,
            loaderSawPatchAll,
            loaderSawPatchAllFailure,
            loaderSawModLoadFailure,
            liveLayout.SnapshotPath,
            liveSnapshotPresent,
            harnessLayout.InventoryPath,
            harnessInventoryPresent,
            harnessLayout.StatusPath,
            harnessStatusPresent,
            DetermineStartupRuntimeDiagnosis(
                moduleInitializerLogged,
                runtimeExporterLogged,
                harnessBridgeLogged,
                currentExecutionSentinelObserved,
                loaderSawCompanionAssembly,
                loaderSawInitializerCall,
                loaderSawNoModInitializerAttribute,
                loaderSawModInitialization,
                loaderSawPatchAll,
                loaderSawPatchAllFailure,
                loaderSawModLoadFailure,
                freshSnapshotPresent,
                harnessInventoryPresent,
                harnessStatusPresent),
            startupSentinelEvidence.Path,
            startupSentinelEvidence.Present,
            startupSentinelEvidence.LastWriteAt,
            startupSentinelEvidence.SessionMatch,
            startupSentinelEvidence.RunMatch,
            startupSentinelEvidence.LaunchTokenMatch,
            LatestCaptureStage: stage,
            LatestCaptureReason: string.IsNullOrWhiteSpace(captureReason) ? stage : captureReason,
            FreshSnapshotPresent: freshSnapshotPresent,
            StaleSnapshotObserved: staleSnapshotObserved,
            NoSnapshotEvidence: noSnapshotEvidence);
    }

    private static bool IsStartupArtifactFreshForCurrentRun(DateTimeOffset lastWriteAtUtc, DateTimeOffset? launchIssuedAtUtc)
    {
        if (launchIssuedAtUtc is null)
        {
            return true;
        }

        return lastWriteAtUtc >= launchIssuedAtUtc.Value.AddSeconds(-1);
    }

    private static GuiSmokeStartupRuntimeCapture CreateStartupRuntimeCapture(GuiSmokeStartupRuntimeEvidence evidence)
    {
        return new GuiSmokeStartupRuntimeCapture(
            evidence.CapturedAt,
            evidence.LatestCaptureStage ?? "manual-clean-boot-runtime-evidence",
            evidence.LatestCaptureReason ?? evidence.LatestCaptureStage ?? "manual-clean-boot-runtime-evidence",
            evidence.Diagnosis,
            evidence.FailureEdge,
            evidence.GodotReachedMainMenu,
            evidence.SentinelPresent,
            evidence.SentinelSessionMatch,
            evidence.SentinelRunMatch,
            evidence.SentinelLaunchTokenMatch,
            evidence.RuntimeExporterInitializedLogged,
            evidence.HarnessBridgeInitializeLogged,
            evidence.LiveSnapshotPresent,
            evidence.FreshSnapshotPresent,
            evidence.StaleSnapshotObserved,
            evidence.NoSnapshotEvidence,
            evidence.LoaderSawAnyModLoaderSignal,
            evidence.LoaderSawCompanionAssembly,
            evidence.LoaderSawInitializerCall,
            evidence.LoaderSawPatchAll,
            evidence.RuntimeLogDeltaMatches.Count,
            evidence.GodotLogDeltaMatches.Count);
    }

    private static IReadOnlyList<GuiSmokeStartupRuntimeCapture> ReadStartupRuntimeCaptures(
        string sessionRoot,
        GuiSmokeStartupRuntimeEvidence? latestEvidence = null)
    {
        var captures = ReadNdjsonRecords<GuiSmokeStartupRuntimeCapture>(GetStartupRuntimeCapturesPath(sessionRoot));
        if (captures.Count > 0)
        {
            return captures;
        }

        return latestEvidence is null
            ? Array.Empty<GuiSmokeStartupRuntimeCapture>()
            : new[] { CreateStartupRuntimeCapture(latestEvidence) };
    }

    private static GuiSmokeStartupRuntimeEvidence ApplyStartupRuntimeCaptureSummary(
        GuiSmokeStartupRuntimeEvidence latestEvidence,
        IReadOnlyList<GuiSmokeStartupRuntimeCapture> captures)
    {
        var orderedCaptures = captures
            .OrderBy(static capture => capture.CapturedAt)
            .ToArray();
        var latestCapture = orderedCaptures.LastOrDefault();
        var firstPositive = orderedCaptures
            .Select(capture => new { Capture = capture, Reason = DetermineStartupRuntimePositiveReason(capture) })
            .FirstOrDefault(static entry => entry.Reason is not null);

        return latestEvidence with
        {
            LatestCaptureStage = latestCapture?.Stage ?? latestEvidence.LatestCaptureStage,
            LatestCaptureReason = latestCapture?.CaptureReason ?? latestEvidence.LatestCaptureReason,
            CaptureCount = orderedCaptures.Length,
            FirstPositiveCaptureAt = firstPositive?.Capture.CapturedAt,
            FirstPositiveReason = firstPositive?.Reason,
            LastCaptureAt = latestCapture?.CapturedAt ?? latestEvidence.CapturedAt,
            EverReachedMainMenu = orderedCaptures.Any(static capture => capture.GodotReachedMainMenu),
            EverSawCurrentExecutionSentinel = orderedCaptures.Any(static capture =>
                capture.SentinelPresent
                && capture.SentinelSessionMatch
                && capture.SentinelRunMatch
                && capture.SentinelLaunchTokenMatch),
            EverSawRuntimeExporter = orderedCaptures.Any(static capture => capture.RuntimeExporterInitializedLogged),
            EverSawHarnessBridge = orderedCaptures.Any(static capture => capture.HarnessBridgeInitializeLogged),
            EverSawFreshSnapshot = orderedCaptures.Any(static capture => capture.FreshSnapshotPresent),
            EverSawStaleSnapshot = orderedCaptures.Any(static capture => capture.StaleSnapshotObserved),
            EverSawLoaderSignal = orderedCaptures.Any(static capture => capture.LoaderSawAnyModLoaderSignal),
        };
    }

    private static string? DetermineStartupRuntimePositiveReason(GuiSmokeStartupRuntimeCapture capture)
    {
        if (capture.GodotReachedMainMenu)
        {
            return "godot-reached-main-menu";
        }

        if (capture.SentinelPresent
            && capture.SentinelSessionMatch
            && capture.SentinelRunMatch
            && capture.SentinelLaunchTokenMatch)
        {
            return "current-execution-sentinel";
        }

        if (capture.RuntimeExporterInitializedLogged)
        {
            return "runtime-exporter-initialized";
        }

        if (capture.HarnessBridgeInitializeLogged)
        {
            return "harness-bridge-initialized";
        }

        if (capture.FreshSnapshotPresent)
        {
            return "fresh-snapshot-present";
        }

        if (capture.StaleSnapshotObserved)
        {
            return "stale-snapshot-observed";
        }

        if (capture.LoaderSawAnyModLoaderSignal)
        {
            return "loader-signal-observed";
        }

        return null;
    }

    private static string DetermineStartupRuntimeDiagnosis(
        bool moduleInitializerLogged,
        bool runtimeExporterLogged,
        bool harnessBridgeLogged,
        bool currentExecutionSentinelObserved,
        bool loaderSawCompanionAssembly,
        bool loaderSawInitializerCall,
        bool loaderSawNoModInitializerAttribute,
        bool loaderSawModInitialization,
        bool loaderSawPatchAll,
        bool loaderSawPatchAllFailure,
        bool loaderSawModLoadFailure,
        bool freshSnapshotPresent,
        bool harnessInventoryPresent,
        bool harnessStatusPresent)
    {
        var runtimeStarted = runtimeExporterLogged || harnessBridgeLogged;
        var observerArtifactsPresent = freshSnapshotPresent || harnessInventoryPresent || harnessStatusPresent;
        if (runtimeStarted)
        {
            return observerArtifactsPresent
                ? "runtime-started-snapshots-present"
                : "observer-bootstrap-bridge-missing";
        }

        if (currentExecutionSentinelObserved
            || moduleInitializerLogged
            || loaderSawCompanionAssembly
            || loaderSawInitializerCall
            || loaderSawNoModInitializerAttribute
            || loaderSawModInitialization
            || loaderSawPatchAll
            || loaderSawPatchAllFailure
            || loaderSawModLoadFailure)
        {
            return "runtime-bootstrap-missing";
        }

        return "loader-entry-before-initializer-not-proven";
    }

    private static string DetermineStartupRuntimeFailureEdge(
        bool runtimeConfigPresent,
        bool runtimeConfigEnabled,
        bool settingsPresent,
        bool settingsModsEnabled,
        bool settingsCompanionDisabled,
        bool companionPckPresent,
        bool companionDllPresent,
        bool loaderSawPatchAllFailure,
        bool loaderSawModLoadFailure,
        bool moduleInitializerLogged,
        bool runtimeExporterLogged,
        bool harnessBridgeLogged,
        bool freshSnapshotPresent,
        bool harnessInventoryPresent,
        bool harnessStatusPresent,
        bool godotReachedMainMenu,
        bool loaderSawAnyModLoaderSignal,
        bool loaderSawModsDirectoryScan,
        bool loaderSawCompanionPckScan,
        bool loaderSawCompanionSkippedWarning,
        bool loaderSawCompanionDisabled,
        bool loaderSawCompanionDuplicate,
        bool loaderSawCompanionAssembly,
        bool loaderSawInitializerCall,
        bool loaderSawNoModInitializerAttribute,
        bool loaderSawModInitialization,
        bool loaderSawPatchAll,
        bool currentExecutionSentinelObserved)
    {
        var runtimeStarted = runtimeExporterLogged || harnessBridgeLogged;
        var observerArtifactsPresent = freshSnapshotPresent || harnessInventoryPresent || harnessStatusPresent;
        if (runtimeStarted)
        {
            return observerArtifactsPresent
                ? "RuntimeExportContext.EnsureInitialized->observer-snapshots"
                : "RuntimeExportContext.EnsureInitialized->observer-snapshots-missing";
        }

        if (!runtimeConfigPresent || !companionPckPresent || !companionDllPresent)
        {
            return "companion-payload-missing";
        }

        if (!runtimeConfigEnabled)
        {
            return "runtime-config-disabled";
        }

        if (!settingsPresent || !settingsModsEnabled)
        {
            return "mods-disabled-in-settings";
        }

        if (settingsCompanionDisabled || loaderSawCompanionDisabled)
        {
            return "companion-disabled-in-settings";
        }

        if (loaderSawCompanionSkippedWarning)
        {
            return "ModManager.TryLoadModFromPck(companion)->PlayerAgreedToModLoading";
        }

        if (loaderSawCompanionDuplicate)
        {
            return "ModManager.TryLoadModFromPck(companion)->duplicate-pck-name";
        }

        if (loaderSawPatchAllFailure || loaderSawModLoadFailure)
        {
            return "ModManager.TryLoadModFromPck(companion)->load-or-patch-failure";
        }

        if (currentExecutionSentinelObserved
            || moduleInitializerLogged
            || loaderSawCompanionAssembly
            || loaderSawInitializerCall
            || loaderSawNoModInitializerAttribute
            || loaderSawModInitialization
            || loaderSawPatchAll)
        {
            return "CLR.ModuleInitializer(startup-sentinel)|Harmony.PatchAll->RuntimeExportContext.EnsureInitialized";
        }

        if (loaderSawModsDirectoryScan && !loaderSawCompanionPckScan)
        {
            return "ModManager.Initialize->LoadModsInDirRecursive(mods)->TryLoadModFromPck(companion)";
        }

        if (!loaderSawAnyModLoaderSignal)
        {
            return godotReachedMainMenu
                ? "OneTimeInitialization.ExecuteEssential->ModManager.Initialize->LoadModsInDirRecursive(mods)"
                : "NGame.GameStartup->OneTimeInitialization.ExecuteEssential->ModManager.Initialize";
        }

        return "mod-load-chain-not-observed";
    }

    private static string NormalizeStartupRuntimeDiagnosis(string? diagnosis)
    {
        if (string.Equals(diagnosis, "runtime-started-observer-missing", StringComparison.OrdinalIgnoreCase))
        {
            return "observer-bootstrap-bridge-missing";
        }

        if (string.Equals(diagnosis, "runtime-loader-failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosis, "runtime-loader-preconditions-blocked", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosis, "runtime-loader-entry-not-observed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosis, "runtime-loader-scan-not-observed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosis, "runtime-loader-not-observed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosis, "loader-entry-before-initializer-failure", StringComparison.OrdinalIgnoreCase))
        {
            return "loader-entry-before-initializer-not-proven";
        }

        return string.IsNullOrWhiteSpace(diagnosis)
            ? "loader-entry-before-initializer-not-proven"
            : diagnosis;
    }

    private static GuiSmokeStartupSentinelEvidence ReadStartupSentinelEvidence(
        string? userDataRoot,
        string? sentinelRelativePath,
        string? expectedSessionId,
        string? expectedRunId,
        string? expectedLaunchToken)
    {
        var sentinelPath = TryResolveContainedPath(userDataRoot, sentinelRelativePath);
        if (string.IsNullOrWhiteSpace(sentinelPath))
        {
            return new GuiSmokeStartupSentinelEvidence(null, false, null, false, false, false);
        }

        if (!File.Exists(sentinelPath))
        {
            return new GuiSmokeStartupSentinelEvidence(sentinelPath, false, null, false, false, false);
        }

        var sentinelRoot = TryReadJsonNode(sentinelPath) as JsonObject;
        var sentinelSessionId = TryReadString(sentinelRoot?["sessionId"]);
        var sentinelRunId = TryReadString(sentinelRoot?["runId"]);
        var sentinelLaunchToken = TryReadString(sentinelRoot?["launchToken"]);
        return new GuiSmokeStartupSentinelEvidence(
            sentinelPath,
            true,
            File.GetLastWriteTimeUtc(sentinelPath),
            string.Equals(sentinelSessionId, expectedSessionId, StringComparison.Ordinal),
            string.Equals(sentinelRunId, expectedRunId, StringComparison.Ordinal),
            string.Equals(sentinelLaunchToken, expectedLaunchToken, StringComparison.Ordinal));
    }

    private static string? TryResolveContainedPath(string? userDataRoot, string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(userDataRoot) || string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
        {
            return null;
        }

        var fullRoot = Path.GetFullPath(userDataRoot);
        var combinedPath = Path.GetFullPath(Path.Combine(fullRoot, relativePath));
        var rootWithSeparator = fullRoot.EndsWith(Path.DirectorySeparatorChar)
            ? fullRoot
            : fullRoot + Path.DirectorySeparatorChar;

        return combinedPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)
            ? combinedPath
            : null;
    }

    private static bool IsRelevantRuntimeLogLine(string line)
    {
        return line.Contains("module initializer bootstrap result", StringComparison.OrdinalIgnoreCase)
               || line.Contains("runtime exporter initialized", StringComparison.OrdinalIgnoreCase)
               || line.Contains("harness bridge initialize result", StringComparison.OrdinalIgnoreCase)
               || line.Contains("game mod initializer", StringComparison.OrdinalIgnoreCase)
               || line.Contains("runtime config refreshed", StringComparison.OrdinalIgnoreCase)
               || line.Contains("exporter patch prepare", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRelevantGodotLogLine(string line)
    {
        return line.Contains("Time to main menu", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Found mod pck file", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Skipping loading mod", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Calling initializer method of type", StringComparison.OrdinalIgnoreCase)
               || line.Contains("No ModInitializerAttribute detected", StringComparison.OrdinalIgnoreCase)
               || line.Contains("sts2-mod-ai-companion", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Sts2ModAiCompanion", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Harmony.PatchAll", StringComparison.OrdinalIgnoreCase)
               || line.Contains("PatchAll", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Tried to load mod with PCK name", StringComparison.OrdinalIgnoreCase)
               || line.Contains("RUNNING MODDED", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Error loading mod", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Loading assembly DLL", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Finished mod initialization", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<string> ReadRelevantLogTail(string path, Func<string, bool> predicate, int maxLines = 200)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<string>();
        }

        try
        {
            return File.ReadLines(path)
                .Where(predicate)
                .TakeLast(maxLines)
                .ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static GuiSmokeRelevantLogDelta ReadRelevantLogDelta(
        string path,
        long baselineSizeBytes,
        Func<string, bool> predicate,
        bool baselineFilePresent,
        int maxLines = 200)
    {
        if (!File.Exists(path))
        {
            return new GuiSmokeRelevantLogDelta(Array.Empty<string>(), TreatedAsCurrentExecution: false);
        }

        try
        {
            var fileInfo = new FileInfo(path);
            var treatCurrentFileAsDelta = !baselineFilePresent || fileInfo.Length < baselineSizeBytes;
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            if (!treatCurrentFileAsDelta && baselineSizeBytes > 0)
            {
                stream.Seek(Math.Min(baselineSizeBytes, stream.Length), SeekOrigin.Begin);
            }

            using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
            var matches = new Queue<string>();
            while (reader.ReadLine() is { } line)
            {
                if (!predicate(line))
                {
                    continue;
                }

                if (matches.Count >= maxLines)
                {
                    matches.Dequeue();
                }

                matches.Enqueue(line);
            }

            return new GuiSmokeRelevantLogDelta(matches.ToArray(), treatCurrentFileAsDelta);
        }
        catch
        {
            return new GuiSmokeRelevantLogDelta(Array.Empty<string>(), TreatedAsCurrentExecution: false);
        }
    }

    private static string ResolvePackagedManifestPath(string sessionRoot)
    {
        var workspaceRoot = Directory.GetCurrentDirectory();
        return Path.Combine(workspaceRoot, "artifacts", "native-package-layout", "flat", "export-project", "mod_manifest.json");
    }

    private static string ResolveSettingsSavePath(GamePathOptions gamePaths)
    {
        var directPath = Path.Combine(gamePaths.UserDataRoot, "steam", gamePaths.SteamAccountId, "settings.save");
        if (File.Exists(directPath))
        {
            return directPath;
        }

        var steamRoot = Path.Combine(gamePaths.UserDataRoot, "steam");
        if (!Directory.Exists(steamRoot))
        {
            return directPath;
        }

        try
        {
            return Directory.EnumerateFiles(steamRoot, "settings.save", SearchOption.AllDirectories)
                .OrderByDescending(static path => File.GetLastWriteTimeUtc(path))
                .FirstOrDefault()
                ?? directPath;
        }
        catch
        {
            return directPath;
        }
    }
}
