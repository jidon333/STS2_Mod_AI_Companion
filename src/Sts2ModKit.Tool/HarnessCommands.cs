using System.Diagnostics;
using System.Text.Json;
using Sts2AiCompanion.Foundation.Artifacts;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Harness.Actions;
using Sts2AiCompanion.Harness.Evaluation;
using Sts2AiCompanion.Harness.Policies;
using Sts2AiCompanion.Harness.Recovery;
using Sts2AiCompanion.Harness.Scenarios;
using Sts2AiCompanion.Harness.State;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;
using Sts2ModKit.Core.Planning;
using Sts2ModAiCompanion.Mod;
using Sts2ModAiCompanion.Mod.Runtime;

namespace Sts2ModKit.Tool;

internal static class HarnessCommands
{
    public static async Task<HarnessScenarioCommandResult> RunScenarioAsync(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        string scenarioPath,
        CancellationToken cancellationToken)
    {
        var stage = "initialize";
        HarnessQueueLayout? queueLayout = null;
        try
        {
            stage = "enable-harness-config";
            var harnessConfiguration = configuration with
            {
                Harness = configuration.Harness with { Enabled = true },
            };
            var startupTargetScene = await TryReadStartupTargetSceneAsync(scenarioPath, cancellationToken).ConfigureAwait(false);
            var liveLayout = LiveExportPathResolver.Resolve(harnessConfiguration.GamePaths, harnessConfiguration.LiveExport);
            queueLayout = HarnessPathResolver.Resolve(harnessConfiguration.GamePaths, harnessConfiguration.Harness);

            stage = "stop-running-game";
            StopGameIfRunning();
            stage = "restore-latest-snapshot";
            TryRestoreLatestSnapshotState(harnessConfiguration, workspaceRoot);
            stage = "sync-modded-profile";
            SyncHarnessProfileState(harnessConfiguration, workspaceRoot);
            stage = "clear-active-run-save";
            ClearHarnessRunProgress(harnessConfiguration);
            stage = "reset-live-export";
            ResetLiveArtifacts(liveLayout);
            stage = "reset-runtime-log";
            ResetRuntimeLog(harnessConfiguration);
            stage = "reset-harness-queue";
            ResetHarnessQueue(queueLayout);
            stage = "deploy-harness";
            await DeployHarnessAsync(harnessConfiguration, workspaceRoot, cancellationToken).ConfigureAwait(false);
            stage = "ensure-game-running";
            EnsureGameRunning(harnessConfiguration);
            stage = "wait-for-harness-bridge";
            await WaitForHarnessBridgeReadyAsync(queueLayout, harnessConfiguration, cancellationToken).ConfigureAwait(false);
            stage = "arm-harness-bridge";
            var armSession = CreateHarnessArmSession(harnessConfiguration, "run-harness-scenario");
            await WriteHarnessArmSessionAsync(queueLayout, armSession, cancellationToken).ConfigureAwait(false);
            stage = "wait-for-harness-arm";
            await WaitForHarnessBridgeReadyAsync(queueLayout, harnessConfiguration, cancellationToken, requiredMode: "armed").ConfigureAwait(false);

            stage = "construct-services";
            var stateSource = new LiveCompanionStateSource(harnessConfiguration);
            var actionExecutor = new BridgeActionExecutor(queueLayout, armSession.SessionToken);
            var recoveryManager = new RecoveryManager(harnessConfiguration, actionExecutor);
            var evaluator = new AcceptanceEvaluator();
            var policyEngine = new DeterministicPolicyEngine(harnessConfiguration);
            var runner = new ScenarioRunner(stateSource, policyEngine, actionExecutor, recoveryManager, evaluator, harnessConfiguration);

            stage = "bootstrap-startup-state";
            var initialState = await BootstrapHarnessStartupAsync(
                stateSource,
                actionExecutor,
                harnessConfiguration,
                startupTargetScene,
                cancellationToken).ConfigureAwait(false);
            stage = "run-scenario";
            var runResult = await runner.RunAsync(scenarioPath, initialState, cancellationToken).ConfigureAwait(false);

            stage = "persist-harness-artifacts";
            var runId = !string.IsNullOrWhiteSpace(runResult.FinalState.Run.RunId)
                ? runResult.FinalState.Run.RunId!
                : $"harness-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}";
            var harnessRunRoot = BuildHarnessRunRoot(workspaceRoot, runId);
            Directory.CreateDirectory(harnessRunRoot);

            var scenarioOutputPath = Path.Combine(harnessRunRoot, "scenario.json");
            var actionsPath = Path.Combine(harnessRunRoot, "actions.ndjson");
            var resultsPath = Path.Combine(harnessRunRoot, "results.ndjson");
            var tracePath = Path.Combine(harnessRunRoot, "trace.ndjson");
            var evaluationPath = Path.Combine(harnessRunRoot, "evaluation.json");
            var replayBundleRoot = Path.Combine(harnessRunRoot, "replay-bundle");

            await File.WriteAllTextAsync(scenarioOutputPath, await File.ReadAllTextAsync(scenarioPath, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
            await WriteNdjsonAsync(actionsPath, runResult.Actions, cancellationToken).ConfigureAwait(false);
            await WriteNdjsonAsync(resultsPath, runResult.Results, cancellationToken).ConfigureAwait(false);
            CopyIfExists(queueLayout.TracePath, tracePath);
            await File.WriteAllTextAsync(
                evaluationPath,
                JsonSerializer.Serialize(runResult.Evaluation, ProgramJson.SerializerOptions),
                cancellationToken).ConfigureAwait(false);
            stage = "write-replay-bundle";
            await WriteReplayBundleAsync(harnessConfiguration, workspaceRoot, runId, replayBundleRoot, cancellationToken).ConfigureAwait(false);

            return new HarnessScenarioCommandResult(
                scenarioPath,
                runId,
                runResult.Evaluation.Status,
                scenarioOutputPath,
                actionsPath,
                resultsPath,
                evaluationPath,
                replayBundleRoot,
                runResult.FinalState.Scene.SceneType,
                runResult.Evaluation.MissingRequiredScenes);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Harness scenario failed during '{stage}' for '{scenarioPath}': {exception.Message}", exception);
        }
        finally
        {
            if (queueLayout is not null)
            {
                TryDisarmHarness(queueLayout);
            }
        }
    }

    public static HarnessRunInspectionResult InspectRun(string workspaceRoot, string runId)
    {
        var harnessRunRoot = BuildHarnessRunRoot(workspaceRoot, runId);
        var evaluationPath = Path.Combine(harnessRunRoot, "evaluation.json");
        var actionsPath = Path.Combine(harnessRunRoot, "actions.ndjson");
        var resultsPath = Path.Combine(harnessRunRoot, "results.ndjson");

        var evaluation = File.Exists(evaluationPath)
            ? JsonSerializer.Deserialize<AcceptanceReport>(File.ReadAllText(evaluationPath), ProgramJson.SerializerOptions)
            : null;

        return new HarnessRunInspectionResult(
            runId,
            harnessRunRoot,
            File.Exists(actionsPath) ? File.ReadLines(actionsPath).Count() : 0,
            File.Exists(resultsPath) ? File.ReadLines(resultsPath).Count() : 0,
            evaluation);
    }

    public static async Task<HarnessArmSession> ArmSessionAsync(
        ScaffoldConfiguration configuration,
        string reason,
        CancellationToken cancellationToken)
    {
        var layout = HarnessPathResolver.Resolve(configuration.GamePaths, configuration.Harness);
        HarnessPathResolver.EnsureDirectories(layout);
        var session = CreateHarnessArmSession(configuration, reason);
        await WriteHarnessArmSessionAsync(layout, session, cancellationToken).ConfigureAwait(false);
        return session;
    }

    public static HarnessControlInspectionResult DisarmSession(ScaffoldConfiguration configuration)
    {
        var layout = HarnessPathResolver.Resolve(configuration.GamePaths, configuration.Harness);
        HarnessPathResolver.EnsureDirectories(layout);
        TryDisarmHarness(layout);
        return InspectControl(configuration);
    }

    public static HarnessControlInspectionResult InspectControl(ScaffoldConfiguration configuration)
    {
        var layout = HarnessPathResolver.Resolve(configuration.GamePaths, configuration.Harness);
        HarnessPathResolver.EnsureDirectories(layout);
        var armSession = TryReadActiveArmSession(layout);
        var inventory = TryReadInventory(layout);
        return new HarnessControlInspectionResult(
            layout.HarnessRoot,
            layout.StatusPath,
            layout.ArmSessionPath,
            layout.InventoryPath,
            TryReadBridgeStatus(layout),
            armSession,
            armSession is not null && armSession.ExpiresAt <= DateTimeOffset.UtcNow,
            inventory?.InventoryId,
            inventory?.SceneType,
            inventory?.Mode,
            inventory?.BlockingModal,
            inventory?.Nodes.Count ?? 0);
    }

    public static async Task<HarnessActionResult> DispatchNodeAsync(
        ScaffoldConfiguration configuration,
        string inventoryId,
        string nodeId,
        CancellationToken cancellationToken)
    {
        var layout = HarnessPathResolver.Resolve(configuration.GamePaths, configuration.Harness);
        HarnessPathResolver.EnsureDirectories(layout);

        var armSession = TryReadActiveArmSession(layout)
                         ?? throw new InvalidOperationException($"Harness arm session is missing: {layout.ArmSessionPath}");
        var inventory = TryReadInventory(layout)
                        ?? throw new InvalidOperationException($"Harness node inventory is missing: {layout.InventoryPath}");
        if (!string.Equals(inventory.InventoryId, inventoryId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Requested inventory '{inventoryId}' does not match current inventory '{inventory.InventoryId}'.");
        }

        var targetNode = inventory.Nodes.FirstOrDefault(node => string.Equals(node.NodeId, nodeId, StringComparison.Ordinal))
                         ?? throw new InvalidOperationException($"Node '{nodeId}' was not found in inventory '{inventoryId}'.");
        var metadata = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["inventoryId"] = inventoryId,
        };
        var action = HarnessAction.Create(
            "dispatch_node",
            $"dispatch-node:{nodeId}",
            timeoutMs: Math.Max(configuration.Harness.StepTimeoutMs, 10_000),
            safetyClass: "test-only",
            metadata: metadata) with
        {
            TargetRef = nodeId,
            TargetLabel = targetNode.Label,
        };

        var executor = new BridgeActionExecutor(layout, armSession.SessionToken);
        return await executor.ExecuteAsync(action, CompanionState.CreateUnknown(inventory.RunId), cancellationToken).ConfigureAwait(false);
    }

    private static async Task DeployHarnessAsync(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        CancellationToken cancellationToken)
    {
        var outputRoot = Path.GetFullPath(configuration.GamePaths.ArtifactsRoot, workspaceRoot);
        var runtimeAssemblyRoot = ResolveRuntimeAssemblyRoot(workspaceRoot);
        AiCompanionModEntryPoint.DeployNativePackage(configuration, outputRoot, runtimeAssemblyRoot, "flat");

        var deployedModsRoot = Path.Combine(configuration.GamePaths.GameDirectory, "mods");
        var bridgeOutputRoot = Path.Combine(workspaceRoot, "src", "Sts2ModAiCompanion.HarnessBridge", "bin", "Debug", "net7.0");
        var foundationOutputRoot = Path.Combine(workspaceRoot, "src", "Sts2AiCompanion.Foundation", "bin", "Debug", "net7.0");

        foreach (var fileName in new[] { "Sts2ModAiCompanion.HarnessBridge.dll" })
        {
            var sourcePath = Path.Combine(bridgeOutputRoot, fileName);
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Harness bridge artifact was not found.", sourcePath);
            }

            File.Copy(sourcePath, Path.Combine(deployedModsRoot, fileName), overwrite: true);
        }

        foreach (var fileName in new[] { "Sts2AiCompanion.Foundation.dll" })
        {
            var sourcePath = Path.Combine(foundationOutputRoot, fileName);
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Foundation artifact was not found.", sourcePath);
            }

            File.Copy(sourcePath, Path.Combine(deployedModsRoot, fileName), overwrite: true);
        }

        var runtimeConfigPath = Path.Combine(deployedModsRoot, configuration.AiCompanionMod.RuntimeConfigFileName);
        var runtimeConfig = new AiCompanionRuntimeConfig
        {
            Enabled = configuration.LiveExport.Enabled,
            GamePaths = configuration.GamePaths,
            LiveExport = configuration.LiveExport,
            Harness = configuration.Harness with { Enabled = true },
        };
        await File.WriteAllTextAsync(
            runtimeConfigPath,
            JsonSerializer.Serialize(runtimeConfig, ProgramJson.SerializerOptions),
            cancellationToken).ConfigureAwait(false);

        var deployedRuntimeConfig = JsonSerializer.Deserialize<AiCompanionRuntimeConfig>(
            await File.ReadAllTextAsync(runtimeConfigPath, cancellationToken).ConfigureAwait(false),
            ProgramJson.SerializerOptions);
        if (deployedRuntimeConfig?.Harness.Enabled != true)
        {
            throw new InvalidOperationException($"Harness runtime config was not persisted with harness enabled: {runtimeConfigPath}");
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    private static void EnsureGameRunning(ScaffoldConfiguration configuration)
    {
        if (Process.GetProcessesByName("SlayTheSpire2").Length > 0)
        {
            return;
        }

        if (!configuration.Harness.AutoLaunchGame)
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c start \"\" \"steam://rungameid/2868840\"",
            UseShellExecute = true,
            CreateNoWindow = true,
        });
    }

    private static async Task WaitForHarnessBridgeReadyAsync(
        HarnessQueueLayout layout,
        ScaffoldConfiguration configuration,
        CancellationToken cancellationToken,
        string? requiredMode = null)
    {
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(Math.Max(configuration.Harness.StepTimeoutMs, 45_000));
        string? lastStatusContents = null;

        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (File.Exists(layout.StatusPath))
            {
                try
                {
                    await using var stream = new FileStream(layout.StatusPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    var status = await JsonSerializer.DeserializeAsync<HarnessBridgeStatus>(stream, ProgramJson.SerializerOptions, cancellationToken).ConfigureAwait(false);
                    if (status is not null
                        && status.Enabled
                        && (string.IsNullOrWhiteSpace(requiredMode)
                            || string.Equals(status.Mode, requiredMode, StringComparison.OrdinalIgnoreCase)))
                    {
                        return;
                    }

                    lastStatusContents = status is null
                        ? "<null>"
                        : JsonSerializer.Serialize(status, ProgramJson.SerializerOptions);
                }
                catch (IOException)
                {
                    // The bridge may still be writing the status file.
                }
                catch (JsonException)
                {
                    // The bridge may still be writing the status file.
                }
            }

            await Task.Delay(Math.Max(configuration.Harness.PollIntervalMs, 250), cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException(
            $"Harness bridge did not become ready within the expected time. status_path={layout.StatusPath} last_status={lastStatusContents ?? "<missing>"}");
    }

    private static HarnessArmSession CreateHarnessArmSession(ScaffoldConfiguration configuration, string reason)
    {
        var issuedAt = DateTimeOffset.UtcNow;
        var ttl = TimeSpan.FromMilliseconds(Math.Max(configuration.Harness.StepTimeoutMs * 8, 600_000));
        return new HarnessArmSession(
            Guid.NewGuid().ToString("N"),
            issuedAt,
            issuedAt.Add(ttl),
            reason);
    }

    private static async Task WriteHarnessArmSessionAsync(
        HarnessQueueLayout layout,
        HarnessArmSession session,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(layout.ArmSessionPath)!);
        await File.WriteAllTextAsync(
            layout.ArmSessionPath,
            JsonSerializer.Serialize(session, ProgramJson.SerializerOptions),
            cancellationToken).ConfigureAwait(false);
    }

    private static void TryDisarmHarness(HarnessQueueLayout layout)
    {
        try
        {
            if (File.Exists(layout.ArmSessionPath))
            {
                File.Delete(layout.ArmSessionPath);
            }
        }
        catch
        {
            // Best-effort cleanup so later manual boots stay dormant.
        }
    }

    private static void StopGameIfRunning()
    {
        foreach (var process in Process.GetProcessesByName("SlayTheSpire2"))
        {
            try
            {
                if (!process.HasExited && process.MainWindowHandle != IntPtr.Zero)
                {
                    process.CloseMainWindow();
                    if (process.WaitForExit(10_000))
                    {
                        continue;
                    }
                }

                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    process.WaitForExit(10_000);
                }
            }
            catch
            {
                // Best-effort shutdown before test-only deploy.
            }
        }
    }

    private static void TryRestoreLatestSnapshotState(ScaffoldConfiguration configuration, string workspaceRoot)
    {
        var snapshotRoot = TryResolveHarnessBaselineSnapshotRoot(configuration, workspaceRoot);
        if (string.IsNullOrWhiteSpace(snapshotRoot))
        {
            return;
        }

        var snapshot = SnapshotExecutor.LoadSnapshotExecutionResult(snapshotRoot);
        SnapshotExecutor.ExecuteRestoreToSnapshotState(snapshot);
    }

    private static void SyncHarnessProfileState(ScaffoldConfiguration configuration, string workspaceRoot)
    {
        var outputRoot = Path.GetFullPath(configuration.GamePaths.ArtifactsRoot, workspaceRoot);
        ModdedProfileSync.SyncVanillaToModded(configuration.GamePaths, outputRoot);
    }

    private static void ClearHarnessRunProgress(ScaffoldConfiguration configuration)
    {
        foreach (var path in EnumerateHarnessRunProgressPaths(configuration))
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Best-effort cleanup for unattended harness startup.
            }
        }
    }

    private static IEnumerable<string> EnumerateHarnessRunProgressPaths(ScaffoldConfiguration configuration)
    {
        var moddedProfileRoot = LiveExportPathResolver.BuildModdedProfileRoot(configuration.GamePaths);
        var vanillaProfileRoot = Path.Combine(
            configuration.GamePaths.UserDataRoot,
            "steam",
            configuration.GamePaths.SteamAccountId,
            $"profile{configuration.GamePaths.ProfileIndex}");
        yield return Path.Combine(moddedProfileRoot, "saves", "current_run.save");
        yield return Path.Combine(moddedProfileRoot, "saves", "current_run.save.backup");
        yield return Path.Combine(moddedProfileRoot, "saves", "progress.save");
        yield return Path.Combine(moddedProfileRoot, "saves", "progress.save.backup");
        yield return Path.Combine(vanillaProfileRoot, "saves", "current_run.save");
        yield return Path.Combine(vanillaProfileRoot, "saves", "current_run.save.backup");
    }

    private static void ResetRuntimeLog(ScaffoldConfiguration configuration)
    {
        var runtimeLogPath = Path.Combine(configuration.GamePaths.GameDirectory, "mods", "sts2-mod-ai-companion.runtime.log");
        try
        {
            if (File.Exists(runtimeLogPath))
            {
                File.Delete(runtimeLogPath);
            }
        }
        catch
        {
            // Best-effort runtime log reset so collector diagnostics stay scoped to the latest harness run.
        }
    }

    private static string? TryResolveHarnessBaselineSnapshotRoot(ScaffoldConfiguration configuration, string workspaceRoot)
    {
        var snapshotsRoot = Path.GetFullPath(Path.Combine(configuration.GamePaths.ArtifactsRoot, "snapshots"), workspaceRoot);
        if (!Directory.Exists(snapshotsRoot))
        {
            return null;
        }

        var reports = Directory.GetFiles(snapshotsRoot, "snapshot-report.json", SearchOption.AllDirectories)
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .ToArray();
        if (reports.Length == 0)
        {
            return null;
        }

        foreach (var report in reports)
        {
            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(report.FullName));
                if (!document.RootElement.TryGetProperty("entries", out var entries)
                    || entries.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var isCleanRunSnapshot = entries.EnumerateArray().Any(entry =>
                {
                    var sourcePath = TryReadJsonString(entry, "sourcePath");
                    if (string.IsNullOrWhiteSpace(sourcePath)
                        || !sourcePath.EndsWith("current_run.save", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    return entry.TryGetProperty("sourceExistedAtSnapshot", out var existed)
                           && existed.ValueKind == JsonValueKind.False;
                });

                if (isCleanRunSnapshot)
                {
                    return report.DirectoryName;
                }
            }
            catch
            {
                // Best-effort snapshot selection for unattended harness runs.
            }
        }

        return reports[0].DirectoryName;
    }

    private static string? TryReadJsonString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static async Task WriteReplayBundleAsync(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        string runId,
        string replayBundleRoot,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(replayBundleRoot);

        var liveLayout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        CopyIfExists(liveLayout.SnapshotPath, Path.Combine(replayBundleRoot, configuration.LiveExport.SnapshotFileName));
        CopyIfExists(liveLayout.SessionPath, Path.Combine(replayBundleRoot, configuration.LiveExport.SessionFileName));
        CopyIfExists(liveLayout.SummaryPath, Path.Combine(replayBundleRoot, configuration.LiveExport.SummaryFileName));
        CopyIfExists(liveLayout.EventsPath, Path.Combine(replayBundleRoot, configuration.LiveExport.EventsFileName));

        var companionPaths = CompanionPathResolver.Resolve(configuration, workspaceRoot, runId);
        if (companionPaths.LiveMirrorRoot is not null && Directory.Exists(companionPaths.LiveMirrorRoot))
        {
            CopyDirectoryContents(companionPaths.LiveMirrorRoot, Path.Combine(replayBundleRoot, "live-mirror"));
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    private static async Task WriteNdjsonAsync<T>(string path, IEnumerable<T> values, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var lines = values.Select(value => JsonSerializer.Serialize(value, ProgramJson.SerializerOptions));
        await File.WriteAllLinesAsync(path, lines, cancellationToken).ConfigureAwait(false);
    }

    private static string ResolveRuntimeAssemblyRoot(string workspaceRoot)
    {
        return Path.Combine(workspaceRoot, "src", "Sts2ModAiCompanion.Mod", "bin", "Debug", "net7.0");
    }

    private static string BuildHarnessRunRoot(string workspaceRoot, string runId)
    {
        var safeRunId = new string(runId.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '-' : character).ToArray());
        return Path.Combine(workspaceRoot, "artifacts", "harness", safeRunId);
    }

    private static async Task<CompanionState> WaitForFreshStartupStateAsync(
        LiveCompanionStateSource stateSource,
        ScaffoldConfiguration configuration,
        string? targetScene,
        CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(Math.Max(configuration.Harness.StepTimeoutMs, 45_000));
        CompanionState? lastState = null;

        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lastState = await stateSource.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(lastState.Scene.SceneType)
                && !string.Equals(lastState.Scene.SceneType, "unknown", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(lastState.Scene.SceneType, "bootstrap", StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(targetScene) || SceneMatches(lastState.Scene.SceneType, targetScene)))
            {
                return lastState;
            }

            await Task.Delay(Math.Max(configuration.Harness.PollIntervalMs, 250), cancellationToken).ConfigureAwait(false);
        }

        return lastState ?? CompanionState.CreateUnknown();
    }

    private static async Task<CompanionState> BootstrapHarnessStartupAsync(
        LiveCompanionStateSource stateSource,
        IHarnessActionExecutor actionExecutor,
        ScaffoldConfiguration configuration,
        string? startupTargetScene,
        CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(Math.Max(configuration.Harness.StepTimeoutMs * 2, 90_000));
        CompanionState? lastState = null;
        var lastBootstrapAttemptAt = DateTimeOffset.MinValue;

        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lastState = await WaitForFreshStartupStateAsync(
                stateSource,
                configuration,
                startupTargetScene,
                cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(startupTargetScene)
                ? IsInteractiveStartupScene(lastState.Scene.SceneType)
                : SceneMatches(lastState.Scene.SceneType, startupTargetScene))
            {
                return lastState;
            }

            if (ShouldBacktrackToStartupTarget(lastState.Scene.SceneType, startupTargetScene)
                && DateTimeOffset.UtcNow - lastBootstrapAttemptAt >= TimeSpan.FromSeconds(1))
            {
                lastBootstrapAttemptAt = DateTimeOffset.UtcNow;
                if (await TryBootstrapDismissAsync(
                        actionExecutor,
                        lastState,
                        "__cancel__",
                        startupTargetScene,
                        cancellationToken).ConfigureAwait(false))
                {
                    await Task.Delay(Math.Max(configuration.Harness.PollIntervalMs, 500), cancellationToken).ConfigureAwait(false);
                    continue;
                }
            }

            if (!IsBlockingStartupScene(lastState.Scene.SceneType)
                || DateTimeOffset.UtcNow - lastBootstrapAttemptAt < TimeSpan.FromSeconds(1))
            {
                await Task.Delay(Math.Max(configuration.Harness.PollIntervalMs, 250), cancellationToken).ConfigureAwait(false);
                continue;
            }

            lastBootstrapAttemptAt = DateTimeOffset.UtcNow;
            var dismissResult = await TryBootstrapDismissAsync(
                actionExecutor,
                lastState,
                "__cancel__",
                startupTargetScene,
                cancellationToken).ConfigureAwait(false);
            if (!dismissResult)
            {
                await TryBootstrapDismissAsync(
                    actionExecutor,
                    lastState,
                    "__confirm__",
                    startupTargetScene,
                    cancellationToken).ConfigureAwait(false);
            }

            await Task.Delay(Math.Max(configuration.Harness.PollIntervalMs, 500), cancellationToken).ConfigureAwait(false);
        }

        return lastState ?? CompanionState.CreateUnknown();
    }

    private static async Task<bool> TryBootstrapDismissAsync(
        IHarnessActionExecutor actionExecutor,
        CompanionState state,
        string targetLabel,
        string? startupTargetScene,
        CancellationToken cancellationToken)
    {
        var action = HarnessAction.Create(
            "click_button",
            "bootstrap:dismiss-overlay",
            targetLabel: targetLabel,
            timeoutMs: 10_000,
            expectedStateDelta: startupTargetScene,
            metadata: new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["bootstrap"] = "true",
                ["startupTargetScene"] = startupTargetScene,
            });
        var result = await actionExecutor.ExecuteAsync(action, state, cancellationToken).ConfigureAwait(false);
        return string.Equals(result.Status, "ok", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string?> TryReadStartupTargetSceneAsync(string scenarioPath, CancellationToken cancellationToken)
    {
        try
        {
            var document = JsonSerializer.Deserialize<ScenarioDefinition>(
                await File.ReadAllTextAsync(scenarioPath, cancellationToken).ConfigureAwait(false),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return document?.Steps?.FirstOrDefault()?.TargetScene;
        }
        catch
        {
            return null;
        }
    }

    private static bool SceneMatches(string? actual, string? expected)
    {
        return string.Equals(NormalizeScene(actual), NormalizeScene(expected), StringComparison.Ordinal);
    }

    private static bool ShouldBacktrackToStartupTarget(string? actual, string? expected)
    {
        var normalizedExpected = NormalizeScene(expected);
        if (string.IsNullOrWhiteSpace(normalizedExpected))
        {
            return false;
        }

        return StartupSceneRank.TryGetValue(NormalizeScene(actual), out var actualRank)
               && StartupSceneRank.TryGetValue(normalizedExpected, out var expectedRank)
               && actualRank > expectedRank;
    }

    private static string NormalizeScene(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "reward" => "rewards",
            "rest" => "rest-site",
            _ => normalized ?? string.Empty,
        };
    }

    private static readonly IReadOnlyDictionary<string, int> StartupSceneRank = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["startup"] = 0,
        ["main-menu"] = 1,
        ["singleplayer-submenu"] = 2,
        ["character-select"] = 3,
        ["map"] = 4,
    };

    private static bool IsBlockingStartupScene(string? sceneType)
    {
        return string.Equals(sceneType, "feedback-overlay", StringComparison.OrdinalIgnoreCase)
               || string.Equals(sceneType, "blocking-overlay", StringComparison.OrdinalIgnoreCase)
               || string.Equals(sceneType, "startup", StringComparison.OrdinalIgnoreCase)
               || string.Equals(sceneType, "bootstrap", StringComparison.OrdinalIgnoreCase)
               || string.Equals(sceneType, "unknown", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInteractiveStartupScene(string? sceneType)
    {
        return string.Equals(sceneType, "main-menu", StringComparison.OrdinalIgnoreCase)
               || string.Equals(sceneType, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase)
               || string.Equals(sceneType, "character-select", StringComparison.OrdinalIgnoreCase)
               || string.Equals(sceneType, "map", StringComparison.OrdinalIgnoreCase);
    }

    private static void ResetHarnessQueue(HarnessQueueLayout layout)
    {
        HarnessPathResolver.EnsureDirectories(layout);
        foreach (var path in new[] { layout.ActionsPath, layout.ResultsPath, layout.StatusPath, layout.TracePath, layout.ArmSessionPath, layout.InventoryPath })
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Best-effort queue cleanup before a new unattended run.
            }
        }
    }

    private static void ResetLiveArtifacts(LiveExportLayout layout)
    {
        foreach (var path in new[]
                 {
                     layout.EventsPath,
                     layout.SnapshotPath,
                     layout.SummaryPath,
                     layout.SessionPath,
                     layout.RawObservationsPath,
                     layout.ScreenTransitionsPath,
                     layout.ChoiceCandidatesPath,
                     layout.ChoiceDecisionsPath,
                 })
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Best-effort live artifact reset between unattended runs.
            }
        }

        try
        {
            if (Directory.Exists(layout.SemanticSnapshotsRoot))
            {
                Directory.Delete(layout.SemanticSnapshotsRoot, recursive: true);
            }
        }
        catch
        {
            // Best-effort semantic snapshot cleanup between unattended runs.
        }
    }

    private static void CopyIfExists(string sourcePath, string destinationPath)
    {
        if (!File.Exists(sourcePath))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt += 1)
        {
            try
            {
                using var sourceStream = new FileStream(
                    sourcePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);
                using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                sourceStream.CopyTo(destinationStream);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100 * attempt);
            }
        }
    }

    private static void CopyDirectoryContents(string sourceDirectory, string destinationDirectory)
    {
        foreach (var directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(destinationDirectory, relative));
        }

        foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, file);
            var destinationPath = Path.Combine(destinationDirectory, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(file, destinationPath, overwrite: true);
        }
    }

    private static HarnessArmSession? TryReadActiveArmSession(HarnessQueueLayout layout)
    {
        if (!File.Exists(layout.ArmSessionPath))
        {
            return null;
        }

        try
        {
            var session = JsonSerializer.Deserialize<HarnessArmSession>(
                File.ReadAllText(layout.ArmSessionPath),
                ProgramJson.SerializerOptions);
            if (session is null || string.IsNullOrWhiteSpace(session.SessionToken))
            {
                return null;
            }

            return session.ExpiresAt <= DateTimeOffset.UtcNow ? null : session;
        }
        catch
        {
            return null;
        }
    }

    private static HarnessBridgeStatus? TryReadBridgeStatus(HarnessQueueLayout layout)
    {
        if (!File.Exists(layout.StatusPath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<HarnessBridgeStatus>(
                File.ReadAllText(layout.StatusPath),
                ProgramJson.SerializerOptions);
        }
        catch
        {
            return null;
        }
    }

    private static HarnessNodeInventory? TryReadInventory(HarnessQueueLayout layout)
    {
        if (!File.Exists(layout.InventoryPath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<HarnessNodeInventory>(
                File.ReadAllText(layout.InventoryPath),
                ProgramJson.SerializerOptions);
        }
        catch
        {
            return null;
        }
    }
}

internal sealed record HarnessScenarioCommandResult(
    string ScenarioPath,
    string RunId,
    string Status,
    string ScenarioOutputPath,
    string ActionsPath,
    string ResultsPath,
    string EvaluationPath,
    string ReplayBundleRoot,
    string FinalScene,
    IReadOnlyList<string> MissingRequiredScenes);

internal sealed record HarnessRunInspectionResult(
    string RunId,
    string RunRoot,
    int ActionCount,
    int ResultCount,
    AcceptanceReport? Evaluation);

internal sealed record HarnessControlInspectionResult(
    string HarnessRoot,
    string StatusPath,
    string ArmSessionPath,
    string InventoryPath,
    HarnessBridgeStatus? Status,
    HarnessArmSession? ArmSession,
    bool ArmSessionExpired,
    string? InventoryId,
    string? SceneType,
    string? InventoryMode,
    string? BlockingModal,
    int NodeCount);
