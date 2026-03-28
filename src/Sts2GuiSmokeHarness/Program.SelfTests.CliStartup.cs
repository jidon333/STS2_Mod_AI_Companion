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
using static GuiSmokeStepRequestFactory;

internal static partial class Program
{
    private static void RunCliStartupSelfTests()
    {
        Assert(
            string.Equals(ResolveProviderKind(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), isLongRun: true), "auto", StringComparison.OrdinalIgnoreCase),
            "Long-run scenario should default to the auto provider.");
        Assert(
            string.Equals(ResolveProviderKind(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), isLongRun: false), "session", StringComparison.OrdinalIgnoreCase),
            "Boot-to-combat should keep the session provider default.");
        Assert(
            string.Equals(
                ResolveProviderKind(
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["--provider"] = "headless",
                    },
                    isLongRun: true),
                "headless",
                StringComparison.OrdinalIgnoreCase),
            "Explicit headless provider should remain selectable.");

        var missingHeadlessProviderCommandRejected = false;
        try
        {
            ValidateProviderConfiguration(
                "headless",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        }
        catch (InvalidOperationException exception) when (exception.Message.Contains("--provider-command is required for --provider headless.", StringComparison.OrdinalIgnoreCase))
        {
            missingHeadlessProviderCommandRejected = true;
        }

        Assert(missingHeadlessProviderCommandRejected, "Headless provider selection should fail fast before launch when --provider-command is missing.");
        ValidateProviderConfiguration(
            "auto",
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        Assert(
            !WindowLocator.IsHungWindow(new WindowCaptureTarget(IntPtr.Zero, "fallback", new Rectangle(0, 0, 100, 100), true, false), _ => true),
            "Fallback capture targets should never be treated as hung windows.");
        Assert(
            WindowLocator.IsHungWindow(new WindowCaptureTarget(new IntPtr(12345), "test", new Rectangle(0, 0, 100, 100), false, false), _ => true),
            "Explicit window handles should honor the hung-window probe.");

        var point = MouseInputDriver.TransformNormalizedPoint(
            new WindowCaptureTarget(IntPtr.Zero, "test", new Rectangle(100, 200, 1000, 800), false, false),
            0.5,
            0.25);
        Assert(point.X == 600 && point.Y == 400, "Normalized transform should map into client bounds.");

        var request = CreateBaseSelfTestRequest();
        var json = JsonSerializer.Serialize(request, GuiSmokeShared.JsonOptions);
        var roundTrip = JsonSerializer.Deserialize<GuiSmokeStepRequest>(json, GuiSmokeShared.JsonOptions);
        Assert(roundTrip?.Phase == GuiSmokePhase.EnterRun.ToString(), "Request should round-trip.");

        var observerOnlyWindow = new WindowCaptureTarget(IntPtr.Zero, "observer-only", new Rectangle(100, 200, 1000, 800), false, false);
        var observerOnlySummary = new ObserverSummary(
            "event",
            "event",
            false,
            DateTimeOffset.UtcNow,
            "observer-only",
            true,
            "hook",
            "stable",
            null,
            null,
            "runtime",
            null,
            null,
            null,
            new[] { "Proceed" },
            Array.Empty<string>(),
            Array.Empty<ObserverActionNode>(),
            Array.Empty<ObserverChoice>(),
            Array.Empty<ObservedCombatHandCard>());
        var observerOnlyState = new ObserverState(observerOnlySummary, null, null, null);
        var observerOnlyContext = CreateObserverOnlyAnalysisContext(
            GuiSmokePhase.HandleEvent,
            observerOnlyState,
            Array.Empty<GuiSmokeHistoryEntry>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            new WindowBounds(observerOnlyWindow.Bounds.X, observerOnlyWindow.Bounds.Y, observerOnlyWindow.Bounds.Width, observerOnlyWindow.Bounds.Height));
        var observerOnlyRequest = CreateStepRequest(
            "run",
            "boot-to-combat",
            4,
            GuiSmokePhase.HandleEvent,
            "/tmp/observer-only.screen.png",
            observerOnlyWindow,
            observerOnlyState,
            Array.Empty<GuiSmokeHistoryEntry>(),
            Directory.GetCurrentDirectory(),
            Path.GetTempPath(),
            "0001",
            1,
            observerOnlyContext,
            null);
        Assert(string.IsNullOrWhiteSpace(observerOnlyRequest.ScreenshotPath), "Observer-only request should leave screenshot path empty instead of implying a captured frame.");

        var decision = CreateBaseSelfTestDecision();
        var replayOutputPath = ResolveCliPath($"/tmp/gui-smoke-replay-self-test-{Guid.NewGuid():N}.json", Directory.GetCurrentDirectory());
        var replayDump = new GuiSmokeCandidateDumpArtifact(
            request.Phase,
            request.ScreenshotPath,
            decision,
            decision,
            true,
            new GuiSmokeDecisionDebugSummary(
                "test-foreground",
                "test-background",
                new[] { "click continue" },
                new[] { new GuiSmokeSuppressedCandidate("wait", "self-test") },
                "self-test winner"),
            new[]
            {
                new GuiSmokeDecisionCandidateDump(
                    "click continue",
                    "self-test",
                    0.99d,
                    true,
                    null,
                    "100,200,120,80",
                    new GuiSmokeCandidatePoint(0.30d, 0.70d),
                    "self-test-bounds",
                    decision.TargetLabel,
                    decision.ActionKind,
                    decision.Reason),
            });
        var replayDumpJson = SerializeCandidateDumpArtifact(replayDump);
        WriteSerializedCandidateDumpArtifact(replayOutputPath, replayDumpJson);
        Assert(File.Exists(replayOutputPath), "Replay candidate dump should be written to the resolved CLI path.");
        Assert(new FileInfo(replayOutputPath).Length > 0, "Replay candidate dump file should not be empty.");
        Assert(File.ReadAllText(replayOutputPath) == replayDumpJson, "Replay candidate dump file should contain the same JSON that replay-step writes to stdout.");
        File.Delete(replayOutputPath);

        var deployToolWorkspaceRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-deploy-tool-self-test-{Guid.NewGuid():N}");
        try
        {
            var toolProjectRoot = Path.Combine(deployToolWorkspaceRoot, "src", "Sts2ModKit.Tool");
            Directory.CreateDirectory(toolProjectRoot);
            File.WriteAllText(
                Path.Combine(toolProjectRoot, "Sts2ModKit.Tool.csproj"),
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net7.0</TargetFramework>
                  </PropertyGroup>
                </Project>
                """);

            static void WriteDeployToolOutput(string root, string configuration, string framework, string contents)
            {
                var outputRoot = Path.Combine(root, "bin", configuration, framework);
                Directory.CreateDirectory(outputRoot);
                var dllPath = Path.Combine(outputRoot, "Sts2ModKit.Tool.dll");
                File.WriteAllText(dllPath, contents);
                File.WriteAllText(Path.ChangeExtension(dllPath, ".deps.json"), "{}");
                File.WriteAllText(Path.ChangeExtension(dllPath, ".runtimeconfig.json"), "{}");
            }

            WriteDeployToolOutput(toolProjectRoot, "Debug", "net7.0", "debug-net7");
            WriteDeployToolOutput(toolProjectRoot, "Release", "net7.0", "release-net7");
            WriteDeployToolOutput(toolProjectRoot, "Debug", "net8.0", "debug-net8");
            File.SetLastWriteTimeUtc(Path.Combine(toolProjectRoot, "bin", "Debug", "net8.0", "Sts2ModKit.Tool.dll"), DateTime.UtcNow.AddMinutes(5));

            var preferredDeployTool = TryFindBuiltDeployToolDll(deployToolWorkspaceRoot);
            Assert(preferredDeployTool is not null
                   && preferredDeployTool.Path.EndsWith(Path.Combine("Debug", "net7.0", "Sts2ModKit.Tool.dll"), StringComparison.OrdinalIgnoreCase)
                   && preferredDeployTool.Reason.Contains("Debug/net7.0", StringComparison.OrdinalIgnoreCase),
                "Deploy subprocess tool selection should prefer the explicit Debug/net7.0 output over newer but less preferred artifacts.");

            Directory.Delete(Path.Combine(toolProjectRoot, "bin", "Debug", "net7.0"), recursive: true);
            var fallbackDeployTool = TryFindBuiltDeployToolDll(deployToolWorkspaceRoot);
            Assert(fallbackDeployTool is not null
                   && fallbackDeployTool.Path.EndsWith(Path.Combine("Release", "net7.0", "Sts2ModKit.Tool.dll"), StringComparison.OrdinalIgnoreCase)
                   && fallbackDeployTool.Reason.Contains("Release/net7.0", StringComparison.OrdinalIgnoreCase),
                "Deploy subprocess tool selection should fall back to Release/net7.0 when the preferred Debug/net7.0 output is unavailable.");
        }
        finally
        {
            if (Directory.Exists(deployToolWorkspaceRoot))
            {
                Directory.Delete(deployToolWorkspaceRoot, recursive: true);
            }
        }

        var startupTraceRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-startup-trace-self-test-{Guid.NewGuid():N}");
        try
        {
            LongRunArtifacts.InitializeSessionArtifacts(startupTraceRoot, "startup-trace-session", "boot-to-long-run", "headless");
            LongRunArtifacts.RecordStartupStage(startupTraceRoot, "game-stopped-before-deploy", "finished");
            LongRunArtifacts.RecordStartupStage(
                startupTraceRoot,
                "deploy-command-selected",
                "finished",
                "in-process:self-test",
                new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["deployMode"] = "in-process",
                    ["toolPath"] = @"C:\fake\Sts2ModAiCompanion.Mod.dll",
                    ["reason"] = "self-test",
                });
            LongRunArtifacts.RecordStartupStage(startupTraceRoot, "bootstrap-launch-issued", "finished", DateTimeOffset.UtcNow.ToString("O"));
            LongRunArtifacts.RecordStartupStage(startupTraceRoot, "bootstrap-window-detected", "finished");
            LongRunArtifacts.RecordStartupStage(startupTraceRoot, "bootstrap-manual-clean-boot-evaluation-started", "started", "bootstrap/0001.screen.png");
            LongRunArtifacts.RecordStartupStage(startupTraceRoot, "bootstrap-manual-clean-boot-evaluation-finished", "finished", "verified=true");
            LongRunArtifacts.RecordStartupStage(startupTraceRoot, "authoritative-attempt-started", "finished", "attempts/0001");
            LongRunArtifacts.RecordStartupStage(startupTraceRoot, "authoritative-first-screenshot-captured", "finished", "attempts/0001/steps/0001.screen.png");
            LongRunArtifacts.RecordStartupFailure(startupTraceRoot, "deploy-verification-finished", "deploy report missing");

            var startupSummary = JsonSerializer.Deserialize<GuiSmokeStartupSummary>(
                                     File.ReadAllText(Path.Combine(startupTraceRoot, "startup-summary.json")),
                                     GuiSmokeShared.JsonOptions)
                                 ?? throw new InvalidOperationException("Failed to read startup summary self-test artifact.");
            Assert(startupSummary.GameStoppedBeforeDeployRecorded, "Startup summary should record the game-stop stage.");
            Assert(startupSummary.DeployCommandSelected
                   && string.Equals(startupSummary.DeployMode, "in-process", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(startupSummary.SelectedDeployToolPath, @"C:\fake\Sts2ModAiCompanion.Mod.dll", StringComparison.OrdinalIgnoreCase),
                "Startup summary should preserve deploy command selection details.");
            Assert(startupSummary.LaunchIssued && startupSummary.WindowDetected, "Startup summary should treat bootstrap launch/window stages as session startup progress.");
            Assert(startupSummary.ManualCleanBootEvaluationStarted && startupSummary.ManualCleanBootEvaluationFinished, "Startup summary should treat bootstrap manual clean boot evaluation as the startup gate.");
            Assert(startupSummary.FirstAttemptCreated && startupSummary.FirstScreenshotCaptured, "Startup summary should record the first attempt and first screenshot stages.");
            Assert(string.Equals(startupSummary.FailureStage, "deploy-verification-finished", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(startupSummary.FailureReason, "deploy report missing", StringComparison.OrdinalIgnoreCase),
                "Startup summary should preserve the last startup failure stage and reason.");

            var startupPrevalidation = JsonSerializer.Deserialize<GuiSmokePrevalidation>(
                                           File.ReadAllText(Path.Combine(startupTraceRoot, "prevalidation.json")),
                                           GuiSmokeShared.JsonOptions)
                                       ?? throw new InvalidOperationException("Failed to read startup prevalidation self-test artifact.");
            Assert(startupPrevalidation.Notes.Contains("startup-failure:deploy-verification-finished:deploy report missing", StringComparer.OrdinalIgnoreCase),
                "Startup failure notes should be durably recorded in prevalidation.");
            Assert(File.ReadAllLines(Path.Combine(startupTraceRoot, "startup-trace.ndjson")).Length >= 5, "Startup trace should record each staged transition.");

            var processResult = GuiSmokeShared.RunProcessDetailedAsync(
                    Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
                    "/c echo deploy-stdout && echo deploy-stderr 1>&2",
                    Directory.GetCurrentDirectory(),
                    TimeSpan.FromSeconds(10))
                .GetAwaiter()
                .GetResult();
            Assert(processResult.ExitCode == 0 && !processResult.TimedOut, "Detailed process execution should report a successful exit code.");
            Assert(processResult.Stdout.Contains("deploy-stdout", StringComparison.OrdinalIgnoreCase), "Detailed process execution should capture stdout.");
            Assert(processResult.Stderr.Contains("deploy-stderr", StringComparison.OrdinalIgnoreCase), "Detailed process execution should capture stderr.");

            LongRunArtifacts.RecordDeployCommandResult(
                startupTraceRoot,
                new GuiSmokeDeployCommand(
                    "subprocess",
                    "dotnet",
                    "\"C:\\fake\\Sts2ModKit.Tool.dll\" deploy-native-package --include-harness-bridge",
                    TimeSpan.FromMinutes(2),
                    @"C:\fake\Sts2ModKit.Tool.dll",
                    "self-test"),
                processResult,
                failureReason: null);
            startupSummary = JsonSerializer.Deserialize<GuiSmokeStartupSummary>(
                                 File.ReadAllText(Path.Combine(startupTraceRoot, "startup-summary.json")),
                                 GuiSmokeShared.JsonOptions)
                             ?? throw new InvalidOperationException("Failed to read startup summary after deploy command result self-test.");
            Assert(startupSummary.DeployCommandExitCode == 0
                   && startupSummary.DeployCommandTimedOut == false
                   && startupSummary.DeployCommandDurationMs is > 0,
                "Startup summary should capture deploy command exit code, timeout, and duration.");
            var deployCommandSummary = JsonSerializer.Deserialize<GuiSmokeDeployCommandSummary>(
                                           File.ReadAllText(Path.Combine(startupTraceRoot, "deploy-command-summary.json")),
                                           GuiSmokeShared.JsonOptions)
                                       ?? throw new InvalidOperationException("Failed to read deploy command summary self-test artifact.");
            Assert(deployCommandSummary.StdoutTail.Contains("deploy-stdout", StringComparison.OrdinalIgnoreCase)
                   && deployCommandSummary.StderrTail.Contains("deploy-stderr", StringComparison.OrdinalIgnoreCase),
                "Deploy command summary should preserve stdout/stderr tails.");

            var failedProcessResult = GuiSmokeShared.RunProcessDetailedAsync(
                    $"gui-smoke-missing-process-{Guid.NewGuid():N}.exe",
                    string.Empty,
                    Directory.GetCurrentDirectory(),
                    TimeSpan.FromSeconds(5))
                .GetAwaiter()
                .GetResult();
            Assert(string.Equals(failedProcessResult.FailureKind, "process-start-failure", StringComparison.OrdinalIgnoreCase),
                "Detailed process execution should classify process start failures.");

            var failedReason = BuildDeployCommandFailureReason(failedProcessResult);
            Assert(!string.IsNullOrWhiteSpace(failedReason) && failedReason.Contains("process-start-failure", StringComparison.OrdinalIgnoreCase),
                "Deploy command failure reasons should include the process failure kind.");

            LongRunArtifacts.RecordDeployCommandResult(
                startupTraceRoot,
                new GuiSmokeDeployCommand(
                    "subprocess",
                    "dotnet",
                    "\"C:\\missing\\Sts2ModKit.Tool.dll\" deploy-native-package --include-harness-bridge",
                    TimeSpan.FromMinutes(2),
                    @"C:\missing\Sts2ModKit.Tool.dll",
                    "self-test failure"),
                failedProcessResult,
                failedReason);

            startupSummary = JsonSerializer.Deserialize<GuiSmokeStartupSummary>(
                                 File.ReadAllText(Path.Combine(startupTraceRoot, "startup-summary.json")),
                                 GuiSmokeShared.JsonOptions)
                             ?? throw new InvalidOperationException("Failed to read startup summary after deploy command failure self-test.");
            Assert(!string.IsNullOrWhiteSpace(startupSummary.DeployCommandFailureReason)
                   && startupSummary.DeployCommandFailureReason.Contains("process-start-failure", StringComparison.OrdinalIgnoreCase),
                "Startup summary should preserve deploy command process start failures.");
            deployCommandSummary = JsonSerializer.Deserialize<GuiSmokeDeployCommandSummary>(
                                       File.ReadAllText(Path.Combine(startupTraceRoot, "deploy-command-summary.json")),
                                       GuiSmokeShared.JsonOptions)
                                   ?? throw new InvalidOperationException("Failed to read deploy command summary after failure self-test.");
            Assert(!string.IsNullOrWhiteSpace(deployCommandSummary.FailureReason)
                   && deployCommandSummary.FailureReason.Contains("process-start-failure", StringComparison.OrdinalIgnoreCase),
                "Deploy command summary should preserve process-start failure reasons.");
            Assert(string.Equals(deployCommandSummary.FailureKind, "process-start-failure", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(deployCommandSummary.ExceptionType, failedProcessResult.ExceptionType, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(deployCommandSummary.ExceptionMessage, failedProcessResult.ExceptionMessage, StringComparison.Ordinal),
                "Deploy command summary should preserve process failure classification details.");

            var timedOutDeployResult = RunTimedInProcessDeployAsync(
                    new GuiSmokeDeployCommand(
                        "in-process",
                        "AiCompanionModEntryPoint.DeployNativePackage",
                        "--self-test-timeout",
                        TimeSpan.FromMilliseconds(100),
                        @"C:\fake\Sts2ModAiCompanion.Mod.dll",
                        "timeout self-test"),
                    () =>
                    {
                        Thread.Sleep(250);
                        return new GuiSmokeProcessExecutionResult(
                            "AiCompanionModEntryPoint.DeployNativePackage",
                            "--self-test-timeout",
                            0,
                            false,
                            TimeSpan.FromMilliseconds(250),
                            "unexpected-success",
                            string.Empty,
                            null,
                            null,
                            null);
                    })
                .GetAwaiter()
                .GetResult();
            Assert(timedOutDeployResult.TimedOut
                   && timedOutDeployResult.Duration >= TimeSpan.FromMilliseconds(90)
                   && timedOutDeployResult.Duration < TimeSpan.FromSeconds(1),
                "In-process deploy timeout guard should mark the result as timed out without waiting for the worker to finish.");
        }
        finally
        {
            if (Directory.Exists(startupTraceRoot))
            {
                Directory.Delete(startupTraceRoot, recursive: true);
            }
        }
    }
}
