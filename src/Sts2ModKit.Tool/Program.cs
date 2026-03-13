using System.Text.Json;
using Sts2AiCompanion.Host;
using FoundationPathResolver = Sts2AiCompanion.Foundation.Artifacts.CompanionPathResolver;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;
using Sts2ModKit.Core.Planning;
using Sts2ModAiCompanion.Mod;
using Sts2ModKit.Tool;

var command = args.Length == 0 ? "help" : args[0].ToLowerInvariant();
var options = ParseOptions(args.Skip(1).ToArray());
var workspaceRoot = Directory.GetCurrentDirectory();
var configPath = ResolveConfigPath(options, workspaceRoot);
var loadResult = ConfigurationLoader.LoadFromFile(configPath);
var configuration = ApplyPathOverrides(loadResult.Configuration, options);

try
{
    switch (command)
    {
        case "show-config":
            PrintJson(new
            {
                loadResult.ConfigurationSource,
                loadResult.Warnings,
                Configuration = configuration,
            });
            return 0;

        case "show-live-export-layout":
            {
                var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
                PrintJson(new
                {
                    loadResult.ConfigurationSource,
                    LiveExport = configuration.LiveExport,
                    Layout = layout,
                });
                return 0;
            }

        case "prepare-live-smoke":
            {
                var result = LiveSmokeCommands.Prepare(configuration);
                PrintJson(result);
                return 0;
            }

        case "inspect-live-export":
            {
                var lineCount = options.TryGetValue("--tail", out var tailRaw)
                    && int.TryParse(tailRaw, out var parsedTail)
                    ? parsedTail
                    : 20;
                var result = LiveSmokeCommands.Inspect(configuration, lineCount);
                PrintJson(result);
                return 0;
            }

        case "analyze-live-once":
            {
                await using var host = new CompanionHost(configuration, workspaceRoot);
                await host.RefreshAsync().ConfigureAwait(false);
                var triggered = await host.RequestManualAdviceAsync().ConfigureAwait(false);
                PrintJson(new
                {
                    Triggered = triggered,
                    Snapshot = host.CurrentSnapshot,
                });
                return triggered ? 0 : 1;
            }

        case "validate-advisor-replay":
            {
                if (!options.TryGetValue("--fixture", out var fixtureRoot))
                {
                    throw new InvalidOperationException("--fixture is required.");
                }

                var mockResponsePath = options.TryGetValue("--mock-response", out var mockResponse) ? mockResponse : null;
                var result = await ReplayValidationCommands.ValidateAdvisorReplayAsync(
                    configuration,
                    workspaceRoot,
                    fixtureRoot,
                    mockResponsePath,
                    CancellationToken.None).ConfigureAwait(false);
                PrintJson(result);
                return 0;
            }

        case "build-replay-fixture":
            {
                if (!options.TryGetValue("--source-run", out var sourceRun))
                {
                    throw new InvalidOperationException("--source-run is required.");
                }

                var result = ReplayValidationCommands.BuildReplayFixture(configuration, workspaceRoot, sourceRun);
                PrintJson(result);
                return 0;
            }

        case "run-harness-scenario":
            {
                if (!options.TryGetValue("--scenario", out var scenarioPath))
                {
                    throw new InvalidOperationException("--scenario is required.");
                }

                var result = await HarnessCommands.RunScenarioAsync(
                    configuration,
                    workspaceRoot,
                    Path.GetFullPath(scenarioPath, workspaceRoot),
                    CancellationToken.None).ConfigureAwait(false);
                PrintJson(result);
                return string.Equals(result.Status, "passed", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
            }

        case "inspect-harness-run":
            {
                if (!options.TryGetValue("--run-id", out var runId))
                {
                    throw new InvalidOperationException("--run-id is required.");
                }

                var result = HarnessCommands.InspectRun(workspaceRoot, runId);
                PrintJson(result);
                return result.Evaluation?.Status == "passed" ? 0 : 1;
            }

        case "collector-postprocess":
            {
                var godotLineCount = options.TryGetValue("--lines", out var lineRaw)
                    && int.TryParse(lineRaw, out var parsedLines)
                    ? parsedLines
                    : 200;
                var tailCount = options.TryGetValue("--tail", out var tailRaw)
                    && int.TryParse(tailRaw, out var parsedTail)
                    ? parsedTail
                    : 40;
                var knowledgeRoot = ResolveKnowledgeRoot(options, configuration, workspaceRoot);
                await using (var host = new CompanionHost(configuration, workspaceRoot))
                {
                    await host.RefreshAsync().ConfigureAwait(false);
                }
                var godotResult = LiveSmokeCommands.InspectGodotLog(configuration, godotLineCount);
                var liveResult = LiveSmokeCommands.Inspect(configuration, tailCount);
                var knowledgeResult = StaticKnowledgeCommands.Extract(configuration, knowledgeRoot);

                var companionPaths = FoundationPathResolver.Resolve(configuration, workspaceRoot, null);
                var currentRunState = TryReadJsonDocument(companionPaths.CurrentRunStatePath);
                var runId = currentRunState is null
                    ? null
                    : TryReadString(currentRunState.RootElement, "runId");
                var runPaths = FoundationPathResolver.Resolve(configuration, workspaceRoot, runId);
                var collectorSummary = runPaths.CollectorSummaryPath is null
                    ? null
                    : TryReadJsonDocument(runPaths.CollectorSummaryPath);

                PrintJson(new
                {
                    CurrentRunStatePath = companionPaths.CurrentRunStatePath,
                    RunId = runId,
                    GodotLog = godotResult,
                    LiveExport = liveResult,
                    Knowledge = knowledgeResult,
                    CollectorSummaryPath = runPaths.CollectorSummaryPath,
                    CollectorSummary = collectorSummary?.RootElement,
                });
                return 0;
            }

        case "inspect-godot-log":
            {
                var lineCount = options.TryGetValue("--lines", out var lineRaw)
                    && int.TryParse(lineRaw, out var parsedLines)
                    ? parsedLines
                    : 200;
                var result = LiveSmokeCommands.InspectGodotLog(configuration, lineCount);
                PrintJson(result);
                return 0;
            }

        case "extract-static-knowledge":
            {
                var knowledgeRoot = ResolveKnowledgeRoot(options, configuration, workspaceRoot);
                var result = StaticKnowledgeCommands.Extract(configuration, knowledgeRoot);
                PrintJson(result);
                return 0;
            }

        case "inspect-static-knowledge":
            {
                var knowledgeRoot = ResolveKnowledgeRoot(options, configuration, workspaceRoot);
                var result = StaticKnowledgeCommands.Inspect(knowledgeRoot);
                PrintJson(result);
                return 0;
            }

        case "merge-observed-knowledge":
            {
                var knowledgeRoot = ResolveKnowledgeRoot(options, configuration, workspaceRoot);
                var result = StaticKnowledgeCommands.MergeObserved(configuration, knowledgeRoot);
                PrintJson(result);
                return 0;
            }

        case "materialize-native-package":
            {
                var outputRoot = ResolveArtifactsRoot(configuration, workspaceRoot);
                var runtimeAssemblyRoot = ResolveRuntimeAssemblyRoot(options, workspaceRoot);
                var layoutKind = options.TryGetValue("--layout", out var requestedLayout)
                    ? requestedLayout
                    : "flat";
                var result = AiCompanionModEntryPoint.MaterializeNativePackage(configuration, outputRoot, runtimeAssemblyRoot, layoutKind);
                if (options.ContainsKey("--include-harness-bridge"))
                {
                    MaterializeHarnessBridge(workspaceRoot, result.PackageRoot);
                }
                PrintJson(result);
                return 0;
            }

        case "build-native-pck":
            {
                var outputRoot = ResolveArtifactsRoot(configuration, workspaceRoot);
                var runtimeAssemblyRoot = ResolveRuntimeAssemblyRoot(options, workspaceRoot);
                var layoutKind = options.TryGetValue("--layout", out var requestedLayout)
                    ? requestedLayout
                    : "flat";
                var godotExecutablePath = ResolveGodotExecutablePath(options, configuration, workspaceRoot);
                var result = AiCompanionModEntryPoint.BuildNativePck(configuration, outputRoot, runtimeAssemblyRoot, layoutKind, godotExecutablePath, workspaceRoot);
                PrintJson(result);
                return 0;
            }

        case "deploy-native-package":
            {
                var outputRoot = ResolveArtifactsRoot(configuration, workspaceRoot);
                var runtimeAssemblyRoot = ResolveRuntimeAssemblyRoot(options, workspaceRoot);
                var layoutKind = options.TryGetValue("--layout", out var requestedLayout)
                    ? requestedLayout
                    : "flat";
                var result = AiCompanionModEntryPoint.DeployNativePackage(configuration, outputRoot, runtimeAssemblyRoot, layoutKind);
                if (options.ContainsKey("--include-harness-bridge"))
                {
                    MaterializeHarnessBridge(workspaceRoot, result.DeployedRoot);
                }
                PrintJson(result);
                return 0;
            }

        case "dry-run-snapshot":
            {
                var snapshotRoot = ResolveSnapshotRoot(options, configuration, workspaceRoot);
                var plan = CreateSnapshotPlan(configuration, snapshotRoot);
                PrintJson(plan);
                return 0;
            }

        case "snapshot":
            {
                var snapshotRoot = ResolveSnapshotRoot(options, configuration, workspaceRoot);
                var plan = CreateSnapshotPlan(configuration, snapshotRoot);
                var result = SnapshotExecutor.ExecuteSnapshot(plan);
                PrintJson(result);
                return 0;
            }

        case "dry-run-restore":
            {
                var snapshotRoot = ResolveSnapshotRoot(options, configuration, workspaceRoot);
                var plan = ResolveRestorePlan(snapshotRoot, configuration);
                PrintJson(plan);
                return 0;
            }

        case "restore":
            {
                var snapshotRoot = ResolveSnapshotRoot(options, configuration, workspaceRoot);
                var plan = ResolveRestorePlan(snapshotRoot, configuration);
                var result = SnapshotExecutor.ExecuteRestore(plan);
                PrintJson(result);
                return 0;
            }

        case "restore-snapshot-state":
            {
                var snapshotRoot = ResolveSnapshotRoot(options, configuration, workspaceRoot);
                var snapshot = SnapshotExecutor.LoadSnapshotExecutionResult(snapshotRoot);
                var result = SnapshotExecutor.ExecuteRestoreToSnapshotState(snapshot);
                PrintJson(result);
                return 0;
            }

        case "verify-snapshot":
            {
                var snapshotRoot = ResolveSnapshotRoot(options, configuration, workspaceRoot);
                var snapshot = SnapshotExecutor.LoadSnapshotExecutionResult(snapshotRoot);
                var result = SnapshotExecutor.VerifySnapshot(snapshot);
                PrintJson(result);
                return result.AllEntriesMatch ? 0 : 1;
            }

        case "sync-modded-profile":
            {
                var outputRoot = ResolveArtifactsRoot(configuration, workspaceRoot);
                var result = ModdedProfileSync.SyncVanillaToModded(configuration.GamePaths, outputRoot);
                PrintJson(result);
                return 0;
            }

        default:
            WriteUsage();
            return 0;
    }
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

static Dictionary<string, string> ParseOptions(string[] args)
{
    var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var index = 0; index < args.Length; index += 1)
    {
        var current = args[index];
        if (!current.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        if (index + 1 < args.Length && !args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            options[current] = args[index + 1];
            index += 1;
            continue;
        }

        options[current] = "true";
    }

    return options;
}

static string? ResolveConfigPath(IReadOnlyDictionary<string, string> options, string workspaceRoot)
{
    if (options.TryGetValue("--config", out var explicitPath))
    {
        return Path.GetFullPath(explicitPath, workspaceRoot);
    }

    var samplePath = Path.Combine(workspaceRoot, "config", "ai-companion.sample.json");
    return File.Exists(samplePath) ? samplePath : null;
}

static ScaffoldConfiguration ApplyPathOverrides(ScaffoldConfiguration configuration, IReadOnlyDictionary<string, string> options)
{
    var gamePaths = configuration.GamePaths.With(new PartialGamePathOptions
    {
        GameDirectory = options.TryGetValue("--game-dir", out var gameDirectory) ? gameDirectory : null,
        UserDataRoot = options.TryGetValue("--user-data-root", out var userDataRoot) ? userDataRoot : null,
        SteamAccountId = options.TryGetValue("--steam-account-id", out var steamAccountId) ? steamAccountId : null,
        ProfileIndex = options.TryGetValue("--profile-index", out var profileIndexRaw)
            && int.TryParse(profileIndexRaw, out var profileIndex)
            ? profileIndex
            : null,
        ArtifactsRoot = options.TryGetValue("--artifacts-root", out var artifactsRoot) ? artifactsRoot : null,
    });

    return configuration with
    {
        GamePaths = gamePaths,
    };
}

static string ResolveArtifactsRoot(ScaffoldConfiguration configuration, string workspaceRoot)
{
    return Path.GetFullPath(configuration.GamePaths.ArtifactsRoot, workspaceRoot);
}

static JsonDocument? TryReadJsonDocument(string path)
{
    if (!File.Exists(path))
    {
        return null;
    }

    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
    return JsonDocument.Parse(stream);
}

static string? TryReadString(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property)
           && property.ValueKind == JsonValueKind.String
        ? property.GetString()
        : null;
}

static string ResolveKnowledgeRoot(
    IReadOnlyDictionary<string, string> options,
    ScaffoldConfiguration configuration,
    string workspaceRoot)
{
    if (options.TryGetValue("--knowledge-root", out var explicitRoot))
    {
        return Path.GetFullPath(explicitRoot, workspaceRoot);
    }

    return Path.Combine(ResolveArtifactsRoot(configuration, workspaceRoot), "knowledge");
}

static string ResolveRuntimeAssemblyRoot(IReadOnlyDictionary<string, string> options, string workspaceRoot)
{
    if (options.TryGetValue("--runtime-assembly-root", out var explicitRoot))
    {
        return Path.GetFullPath(explicitRoot, workspaceRoot);
    }

    var modBuildOutput = Path.Combine(workspaceRoot, "src", "Sts2ModAiCompanion.Mod", "bin", "Debug", "net7.0");
    if (Directory.Exists(modBuildOutput))
    {
        return modBuildOutput;
    }

    return AppContext.BaseDirectory;
}

static string ResolveGodotExecutablePath(IReadOnlyDictionary<string, string> options, ScaffoldConfiguration configuration, string workspaceRoot)
{
    if (options.TryGetValue("--godot-exe", out var explicitPath))
    {
        return Path.GetFullPath(explicitPath, workspaceRoot);
    }

    if (!string.IsNullOrWhiteSpace(configuration.Assistant.OptionalGodotExe))
    {
        return Path.GetFullPath(configuration.Assistant.OptionalGodotExe!, workspaceRoot);
    }

    return Path.Combine(workspaceRoot, "artifacts", "tools", "Godot_v4.5.1-stable_win64", "Godot_v4.5.1-stable_win64_console.exe");
}

static string ResolveSnapshotRoot(
    IReadOnlyDictionary<string, string> options,
    ScaffoldConfiguration configuration,
    string workspaceRoot)
{
    if (options.TryGetValue("--snapshot-root", out var explicitRoot))
    {
        return Path.GetFullPath(explicitRoot, workspaceRoot);
    }

    return Path.GetFullPath(
        SnapshotPlanner.BuildSnapshotRoot(configuration.GamePaths, DateTimeOffset.Now),
        workspaceRoot);
}

static SnapshotPlan CreateSnapshotPlan(ScaffoldConfiguration configuration, string snapshotRoot)
{
    return SnapshotPlanner.CreateDefaultPlan(
        configuration.GamePaths,
        snapshotRoot,
        trackedModsPaths: BuildTrackedModsPaths(configuration));
}

static RestorePlan ResolveRestorePlan(string snapshotRoot, ScaffoldConfiguration configuration)
{
    var reportPath = SnapshotPlanner.BuildSnapshotReportPath(snapshotRoot);
    if (File.Exists(reportPath))
    {
        var snapshot = SnapshotExecutor.LoadSnapshotExecutionResult(snapshotRoot);
        return SnapshotPlanner.CreateRestorePlan(snapshot);
    }

    var plan = CreateSnapshotPlan(configuration, snapshotRoot);
    return SnapshotPlanner.CreateRestorePlan(plan);
}

static IReadOnlyList<string> BuildTrackedModsPaths(ScaffoldConfiguration configuration)
{
    var modsRoot = Path.Combine(configuration.GamePaths.GameDirectory, "mods");
    var aiDllName = Path.ChangeExtension(configuration.AiCompanionMod.PckName, ".dll");

    var tracked = new List<string>
    {
        Path.Combine(modsRoot, configuration.AiCompanionMod.RuntimeConfigFileName),
        Path.Combine(modsRoot, aiDllName),
        Path.Combine(modsRoot, configuration.AiCompanionMod.PckName),
        Path.Combine(modsRoot, configuration.AiCompanionMod.RuntimeLogFileName),
        Path.Combine(modsRoot, "Sts2ModKit.Core.dll"),
    };

    if (configuration.Harness.Enabled)
    {
        tracked.Add(Path.Combine(modsRoot, "Sts2ModAiCompanion.HarnessBridge.dll"));
        tracked.Add(Path.Combine(modsRoot, "Sts2AiCompanion.Foundation.dll"));
    }

    return tracked;
}

static void WriteUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- show-config [--config path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- show-live-export-layout [--config path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- prepare-live-smoke [--config path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- inspect-live-export [--config path] [--tail 20]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- analyze-live-once [--config path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- validate-advisor-replay --fixture <path> [--config path] [--mock-response path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- build-replay-fixture --source-run <run-id> [--config path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- run-harness-scenario --scenario <path> [--config path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- inspect-harness-run --run-id <run-id>");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- collector-postprocess [--config path] [--knowledge-root path] [--lines 200] [--tail 40]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- inspect-godot-log [--config path] [--lines 200]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- extract-static-knowledge [--config path] [--knowledge-root path] [--godot-exe path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- inspect-static-knowledge [--knowledge-root path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- merge-observed-knowledge [--config path] [--knowledge-root path] [--godot-exe path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- materialize-native-package [--config path] [--artifacts-root path] [--runtime-assembly-root path] [--layout flat|subdir] [--include-harness-bridge]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- build-native-pck [--config path] [--artifacts-root path] [--runtime-assembly-root path] [--layout flat|subdir] [--godot-exe path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- deploy-native-package [--config path] [--artifacts-root path] [--runtime-assembly-root path] [--layout flat|subdir] [--include-harness-bridge]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- dry-run-snapshot [--config path] [--snapshot-root path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- snapshot [--config path] [--snapshot-root path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- dry-run-restore [--config path] [--snapshot-root path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- restore [--config path] [--snapshot-root path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- restore-snapshot-state [--config path] [--snapshot-root path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- verify-snapshot [--snapshot-root path]");
    Console.WriteLine("  dotnet run --project src/Sts2ModKit.Tool -- sync-modded-profile [--config path] [--artifacts-root path]");
}

static void PrintJson<T>(T value)
{
    Console.WriteLine(JsonSerializer.Serialize(value, ProgramJson.SerializerOptions));
}

static void MaterializeHarnessBridge(string workspaceRoot, string destinationRoot)
{
    Directory.CreateDirectory(destinationRoot);

    foreach (var (sourceRoot, fileName) in new[]
             {
                 (Path.Combine(workspaceRoot, "src", "Sts2ModAiCompanion.HarnessBridge", "bin", "Debug", "net7.0"), "Sts2ModAiCompanion.HarnessBridge.dll"),
                 (Path.Combine(workspaceRoot, "src", "Sts2AiCompanion.Foundation", "bin", "Debug", "net7.0"), "Sts2AiCompanion.Foundation.dll"),
             })
    {
        var sourcePath = Path.Combine(sourceRoot, fileName);
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Harness deployment dependency was not found.", sourcePath);
        }

        File.Copy(sourcePath, Path.Combine(destinationRoot, fileName), overwrite: true);
    }
}
