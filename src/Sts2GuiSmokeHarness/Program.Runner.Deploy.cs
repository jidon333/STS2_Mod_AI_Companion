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
    static async Task<GuiSmokeProcessExecutionResult> RunDeployNativePackageAsync(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        IReadOnlyDictionary<string, string> options,
        GuiSmokeDeployCommand? deployCommand = null)
    {
        var resolvedDeployCommand = deployCommand ?? BuildDeployNativePackageCommand(configuration, workspaceRoot, options);
        if (string.Equals(resolvedDeployCommand.Mode, "in-process", StringComparison.OrdinalIgnoreCase))
        {
            LogHarness($"deploy in-process assembly={resolvedDeployCommand.ToolPath ?? "unknown"} reason={resolvedDeployCommand.Reason}");
            return await RunDeployNativePackageInProcessAsync(configuration, workspaceRoot, options, resolvedDeployCommand).ConfigureAwait(false);
        }

        if (resolvedDeployCommand.ToolPath is not null)
        {
            LogHarness($"deploy subprocess tool={resolvedDeployCommand.ToolPath} reason={resolvedDeployCommand.Reason}");
        }
        else
        {
            LogHarness($"deploy subprocess fallback uses dotnet run --project src\\Sts2ModKit.Tool reason={resolvedDeployCommand.Reason}");
        }

        return await GuiSmokeShared.RunProcessDetailedAsync(
            resolvedDeployCommand.FileName,
            resolvedDeployCommand.Arguments,
            workspaceRoot,
            resolvedDeployCommand.Timeout).ConfigureAwait(false);
    }

    static GuiSmokeDeployCommand BuildDeployNativePackageCommand(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        IReadOnlyDictionary<string, string> options)
    {
        var deployMode = ResolveDeployMode(options);
        if (string.Equals(deployMode, "in-process", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = ResolveDeployArtifactsRoot(configuration, workspaceRoot);
            var runtimeAssemblyRoot = ResolveDeployRuntimeAssemblyRoot(options, workspaceRoot);
            var layoutKind = ResolveDeployLayoutKind(options);
            return new GuiSmokeDeployCommand(
                "in-process",
                "AiCompanionModEntryPoint.DeployNativePackage",
                BuildInProcessDeployArguments(outputRoot, runtimeAssemblyRoot, layoutKind),
                TimeSpan.FromMinutes(2),
                typeof(AiCompanionModEntryPoint).Assembly.Location,
                "default in-process deploy");
        }

        var builtTool = TryFindBuiltDeployToolDll(workspaceRoot);
        if (builtTool is not null)
        {
            return new GuiSmokeDeployCommand(
                "subprocess",
                "dotnet",
                $"\"{builtTool.Path}\" deploy-native-package --include-harness-bridge",
                TimeSpan.FromMinutes(2),
                builtTool.Path,
                builtTool.Reason);
        }

        return new GuiSmokeDeployCommand(
            "subprocess",
            "dotnet",
            "run --project src\\Sts2ModKit.Tool -- deploy-native-package --include-harness-bridge",
            TimeSpan.FromMinutes(5),
            null,
            "built deploy tool unavailable");
    }

    static string ResolveDeployMode(IReadOnlyDictionary<string, string> options)
    {
        if (!options.TryGetValue("--deploy-mode", out var deployModeRaw) || string.IsNullOrWhiteSpace(deployModeRaw))
        {
            return "in-process";
        }

        return deployModeRaw.Trim().ToLowerInvariant() switch
        {
            "in-process" => "in-process",
            "subprocess" => "subprocess",
            _ => throw new InvalidOperationException($"Unsupported deploy mode: {deployModeRaw}"),
        };
    }

    static string ResolveDeployArtifactsRoot(ScaffoldConfiguration configuration, string workspaceRoot)
    {
        return Path.GetFullPath(configuration.GamePaths.ArtifactsRoot, workspaceRoot);
    }

    static string ResolveDeployRuntimeAssemblyRoot(IReadOnlyDictionary<string, string> options, string workspaceRoot)
    {
        if (options.TryGetValue("--runtime-assembly-root", out var explicitRoot))
        {
            return ResolveCliPath(explicitRoot, workspaceRoot);
        }

        var modBuildOutput = Path.Combine(workspaceRoot, "src", "Sts2ModAiCompanion.Mod", "bin", "Debug", "net7.0");
        if (Directory.Exists(modBuildOutput))
        {
            return modBuildOutput;
        }

        return AppContext.BaseDirectory;
    }

    static string ResolveDeployLayoutKind(IReadOnlyDictionary<string, string> options)
    {
        return options.TryGetValue("--deploy-layout", out var requestedLayout) && !string.IsNullOrWhiteSpace(requestedLayout)
            ? requestedLayout
            : "flat";
    }

    static string BuildInProcessDeployArguments(string outputRoot, string runtimeAssemblyRoot, string layoutKind)
    {
        return $"--artifacts-root \"{outputRoot}\" --runtime-assembly-root \"{runtimeAssemblyRoot}\" --layout {layoutKind} --include-harness-bridge";
    }

    static async Task<GuiSmokeProcessExecutionResult> RunDeployNativePackageInProcessAsync(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        IReadOnlyDictionary<string, string> options,
        GuiSmokeDeployCommand command)
    {
        return await RunTimedInProcessDeployAsync(
            command,
            () =>
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var outputRoot = ResolveDeployArtifactsRoot(configuration, workspaceRoot);
                    var runtimeAssemblyRoot = ResolveDeployRuntimeAssemblyRoot(options, workspaceRoot);
                    var layoutKind = ResolveDeployLayoutKind(options);
                    var result = AiCompanionModEntryPoint.DeployNativePackage(configuration, outputRoot, runtimeAssemblyRoot, layoutKind);
                    MaterializeHarnessBridge(workspaceRoot, result.DeployedRoot);
                    stopwatch.Stop();
                    return new GuiSmokeProcessExecutionResult(
                        command.FileName,
                        command.Arguments,
                        0,
                        false,
                        stopwatch.Elapsed,
                        JsonSerializer.Serialize(result, GuiSmokeShared.JsonOptions),
                        string.Empty,
                        null,
                        null,
                        null);
                }
                catch (Exception exception)
                {
                    stopwatch.Stop();
                    return new GuiSmokeProcessExecutionResult(
                        command.FileName,
                        command.Arguments,
                        1,
                        false,
                        stopwatch.Elapsed,
                        string.Empty,
                        exception.ToString(),
                        "deploy-in-process-failure",
                        exception.GetType().Name,
                        exception.Message);
                }
            }).ConfigureAwait(false);
    }

    static async Task<GuiSmokeProcessExecutionResult> RunTimedInProcessDeployAsync(
        GuiSmokeDeployCommand command,
        Func<GuiSmokeProcessExecutionResult> operation)
    {
        var completion = new TaskCompletionSource<GuiSmokeProcessExecutionResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var stopwatch = Stopwatch.StartNew();
        var worker = new Thread(() =>
        {
            try
            {
                completion.TrySetResult(operation());
            }
            catch (Exception exception)
            {
                completion.TrySetResult(
                    new GuiSmokeProcessExecutionResult(
                        command.FileName,
                        command.Arguments,
                        1,
                        false,
                        stopwatch.Elapsed,
                        string.Empty,
                        exception.ToString(),
                        "deploy-in-process-failure",
                        exception.GetType().Name,
                        exception.Message));
            }
        })
        {
            IsBackground = true,
            Name = "gui-smoke-in-process-deploy",
        };

        worker.Start();
        var completedTask = await Task.WhenAny(completion.Task, Task.Delay(command.Timeout)).ConfigureAwait(false);
        if (ReferenceEquals(completedTask, completion.Task))
        {
            return await completion.Task.ConfigureAwait(false);
        }

        stopwatch.Stop();
        return new GuiSmokeProcessExecutionResult(
            command.FileName,
            command.Arguments,
            null,
            true,
            stopwatch.Elapsed,
            string.Empty,
            $"in-process deploy timed out after {command.Timeout.TotalSeconds:F1}s; runner will abort to terminate the background worker.",
            null,
            null,
            null);
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

    static string? BuildDeployCommandFailureReason(GuiSmokeProcessExecutionResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.FailureKind))
        {
            var stderrSummary = SummarizeProcessOutput(result.Stderr);
            var stdoutSummary = SummarizeProcessOutput(result.Stdout);
            var exceptionType = string.IsNullOrWhiteSpace(result.ExceptionType)
                ? "none"
                : result.ExceptionType;
            var exceptionMessage = string.IsNullOrWhiteSpace(result.ExceptionMessage)
                ? "none"
                : SanitizeNoteText(result.ExceptionMessage);
            return $"{result.FailureKind}; exception={exceptionType}: {exceptionMessage}; stderr={stderrSummary}; stdout={stdoutSummary}";
        }

        if (result.TimedOut)
        {
            return $"timeout after {result.Duration.TotalSeconds:F1}s";
        }

        if (result.ExitCode is not null and not 0)
        {
            var stderrSummary = SummarizeProcessOutput(result.Stderr);
            var stdoutSummary = SummarizeProcessOutput(result.Stdout);
            return $"exit-code:{result.ExitCode}; stderr={stderrSummary}; stdout={stdoutSummary}";
        }

        return null;
    }

    static string SummarizeProcessOutput(string? text, int maxLength = 240)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "none";
        }

        var normalized = text.Replace("\r", " ").Replace("\n", " ").Trim();
        if (normalized.Length <= maxLength)
        {
            return normalized;
        }

        return normalized[..maxLength] + "...";
    }

    static GuiSmokeDeployToolSelection? TryFindBuiltDeployToolDll(string workspaceRoot)
    {
        var toolProjectRoot = Path.Combine(workspaceRoot, "src", "Sts2ModKit.Tool");
        var toolBinRoot = Path.Combine(toolProjectRoot, "bin");
        if (!Directory.Exists(toolBinRoot))
        {
            return null;
        }

        var preferredFramework = TryReadDeployToolTargetFramework(toolProjectRoot) ?? "net7.0";
        var preferredCandidates = new[]
        {
            new GuiSmokeDeployToolSelection(
                Path.Combine(toolBinRoot, "Debug", preferredFramework, "Sts2ModKit.Tool.dll"),
                $"preferred exact output path Debug/{preferredFramework}"),
            new GuiSmokeDeployToolSelection(
                Path.Combine(toolBinRoot, "Release", preferredFramework, "Sts2ModKit.Tool.dll"),
                $"fallback exact output path Release/{preferredFramework}"),
        };
        foreach (var preferredCandidate in preferredCandidates)
        {
            if (IsUsableDeployToolArtifact(preferredCandidate.Path))
            {
                return preferredCandidate;
            }
        }

        return Directory.GetFiles(toolBinRoot, "Sts2ModKit.Tool.dll", SearchOption.AllDirectories)
            .Where(IsUsableDeployToolArtifact)
            .Select(path => new GuiSmokeDeployToolSelection(
                path,
                $"validated fallback {DescribeDeployToolCandidate(path, toolBinRoot, preferredFramework)}"))
            .OrderByDescending(selection => ScoreDeployToolCandidate(selection.Path, toolBinRoot, preferredFramework))
            .ThenByDescending(selection => File.GetLastWriteTimeUtc(selection.Path))
            .FirstOrDefault();
    }

    static string? TryReadDeployToolTargetFramework(string toolProjectRoot)
    {
        var projectPath = Path.Combine(toolProjectRoot, "Sts2ModKit.Tool.csproj");
        if (!File.Exists(projectPath))
        {
            return null;
        }

        var projectText = File.ReadAllText(projectPath);
        const string startTag = "<TargetFramework>";
        const string endTag = "</TargetFramework>";
        var startIndex = projectText.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
        if (startIndex < 0)
        {
            return null;
        }

        startIndex += startTag.Length;
        var endIndex = projectText.IndexOf(endTag, startIndex, StringComparison.OrdinalIgnoreCase);
        if (endIndex < 0)
        {
            return null;
        }

        var targetFramework = projectText[startIndex..endIndex].Trim();
        return string.IsNullOrWhiteSpace(targetFramework) ? null : targetFramework;
    }

    static bool IsUsableDeployToolArtifact(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        var runtimeConfigPath = Path.ChangeExtension(path, ".runtimeconfig.json");
        var depsPath = Path.ChangeExtension(path, ".deps.json");
        return File.Exists(runtimeConfigPath)
               && File.Exists(depsPath)
               && new FileInfo(path).Length > 0;
    }

    static int ScoreDeployToolCandidate(string path, string toolBinRoot, string preferredFramework)
    {
        var relativePath = Path.GetRelativePath(toolBinRoot, path).Replace('\\', '/');
        if (string.Equals(relativePath, $"Debug/{preferredFramework}/Sts2ModKit.Tool.dll", StringComparison.OrdinalIgnoreCase))
        {
            return 400;
        }

        if (string.Equals(relativePath, $"Release/{preferredFramework}/Sts2ModKit.Tool.dll", StringComparison.OrdinalIgnoreCase))
        {
            return 300;
        }

        if (relativePath.Contains($"/{preferredFramework}/", StringComparison.OrdinalIgnoreCase))
        {
            return 200;
        }

        return 100;
    }

    static string DescribeDeployToolCandidate(string path, string toolBinRoot, string preferredFramework)
    {
        var relativePath = Path.GetRelativePath(toolBinRoot, path).Replace('\\', '/');
        if (relativePath.Contains($"/{preferredFramework}/", StringComparison.OrdinalIgnoreCase))
        {
            return $"{relativePath} (matches preferred framework {preferredFramework})";
        }

        return $"{relativePath} (fallback outside preferred framework {preferredFramework})";
    }

    static string BuildDeployFailureNote(Exception exception)
    {
        var inner = exception.InnerException is null
            ? string.Empty
            : $" | inner={exception.InnerException.GetType().Name}: {SanitizeNoteText(exception.InnerException.Message)}";
        return $"runner deploy failure: {exception.GetType().Name}: {SanitizeNoteText(exception.Message)}{inner}";
    }

    static string SanitizeNoteText(string? value)
    {
        return (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Trim();
    }
}
