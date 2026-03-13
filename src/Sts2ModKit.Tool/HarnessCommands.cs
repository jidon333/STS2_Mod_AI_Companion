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
        try
        {
            stage = "enable-harness-config";
            var harnessConfiguration = configuration with
            {
                Harness = configuration.Harness with { Enabled = true },
            };
            var startupTargetScene = await TryReadStartupTargetSceneAsync(scenarioPath, cancellationToken).ConfigureAwait(false);
            var liveLayout = LiveExportPathResolver.Resolve(harnessConfiguration.GamePaths, harnessConfiguration.LiveExport);
            var queueLayout = HarnessPathResolver.Resolve(harnessConfiguration.GamePaths, harnessConfiguration.Harness);

            stage = "stop-running-game";
            StopGameIfRunning();
            stage = "restore-latest-snapshot";
            TryRestoreLatestSnapshotState(harnessConfiguration, workspaceRoot);
            stage = "clear-active-run-save";
            ClearHarnessRunProgress(harnessConfiguration);
            stage = "reset-live-export";
            ResetLiveArtifacts(liveLayout);
            stage = "reset-harness-queue";
            ResetHarnessQueue(queueLayout);
            stage = "deploy-harness";
            await DeployHarnessAsync(harnessConfiguration, workspaceRoot, cancellationToken).ConfigureAwait(false);
            stage = "ensure-game-running";
            EnsureGameRunning(harnessConfiguration);
            stage = "wait-for-harness-bridge";
            await WaitForHarnessBridgeReadyAsync(queueLayout, harnessConfiguration, cancellationToken).ConfigureAwait(false);

            stage = "construct-services";
            var stateSource = new LiveCompanionStateSource(harnessConfiguration);
            var actionExecutor = new BridgeActionExecutor(queueLayout);
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
        CancellationToken cancellationToken)
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
                    if (status is not null && status.Enabled)
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
        yield return Path.Combine(moddedProfileRoot, "saves", "current_run.save");
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
        foreach (var path in new[] { layout.ActionsPath, layout.ResultsPath, layout.StatusPath, layout.TracePath })
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
        File.Copy(sourcePath, destinationPath, overwrite: true);
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
