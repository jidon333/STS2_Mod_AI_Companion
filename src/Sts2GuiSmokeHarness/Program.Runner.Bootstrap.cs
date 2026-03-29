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
using static ObserverScreenProvenance;

internal static partial class Program
{
    static bool IsPreAttemptBootstrapBoundary(GuiSmokePhase phase, IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return phase == GuiSmokePhase.WaitMainMenu && history.Count == 0;
    }

    static bool CanCompleteBootstrap(
        bool manualCleanBootVerified,
        string trustState,
        GuiSmokeStartupRuntimeEvidence startupRuntimeEvidence,
        GuiSmokePhase phase,
        IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return manualCleanBootVerified
               && string.Equals(trustState, GuiSmokeContractStates.TrustValid, StringComparison.OrdinalIgnoreCase)
               && startupRuntimeEvidence.RuntimeExporterInitializedLogged
               && startupRuntimeEvidence.FreshSnapshotPresent
               && IsPreAttemptBootstrapBoundary(phase, history);
    }

    static string ResolveTrustStateAtAttemptStart(string sessionRoot)
    {
        return LongRunArtifacts.RefreshSupervisorState(sessionRoot).TrustState;
    }

    static void WriteObserverBootstrapCopy(string observerStatePath, ObserverState observer)
    {
        var directory = Path.GetDirectoryName(observerStatePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(observerStatePath, JsonSerializer.Serialize(observer, GuiSmokeShared.JsonOptions));
    }

    static async Task<bool> RunBootstrapPhaseAsync(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        IReadOnlyDictionary<string, string> options,
        LiveExportLayout liveLayout,
        HarnessQueueLayout harnessLayout,
        string sessionId,
        string sessionRoot,
        DateTimeOffset sessionDeadline)
    {
        const int TransitionSettleMs = 1000;
        const int ManualCleanBootObserverBootstrapPollMs = 500;
        const int ManualCleanBootObserverBootstrapPollCount = 12;

        var bootstrapRunId = $"{sessionId}-bootstrap";
        var startupStage = "bootstrap-started";
        var bootstrapRoot = Path.Combine(sessionRoot, "bootstrap");
        var keepVideoOnSuccess = options.ContainsKey("--keep-video-on-success");
        Directory.CreateDirectory(bootstrapRoot);
        using var bootstrapVideo = GuiSmokeVideoRecorder.Create(
            workspaceRoot,
            options,
            sessionId,
            bootstrapRunId,
            bootstrapRoot,
            sessionRoot,
            attemptId: null,
            scopeKind: "bootstrap");

        void RecordBootstrapStage(
            string stage,
            string status,
            string? detail = null,
            IReadOnlyDictionary<string, string?>? metadata = null)
        {
            startupStage = stage;
            LongRunArtifacts.RecordStartupStage(sessionRoot, stage, status, detail, metadata);
        }

        void RecordBootstrapFailure(
            string reason,
            IReadOnlyDictionary<string, string?>? metadata = null)
        {
            LongRunArtifacts.RecordStartupFailure(sessionRoot, startupStage, reason, metadata);
        }

        RecordBootstrapStage("bootstrap-started", "finished", bootstrapRunId);

        try
        {
            await StopGameProcessesAsync(TimeSpan.FromSeconds(20)).ConfigureAwait(false);
            EnsureGameNotRunning();

            var launchIssuedAt = DateTimeOffset.UtcNow;
            var startupRuntimeConfig = EnsureStartupRuntimeConfig(configuration, sessionId, bootstrapRunId, launchIssuedAt);
            LongRunArtifacts.RecordStartupLogBaseline(
                sessionRoot,
                configuration,
                bootstrapRunId,
                startupRuntimeConfig.LaunchToken,
                launchIssuedAt);

            RecordBootstrapStage("bootstrap-launch-issued", "started", launchIssuedAt.ToString("O"));
            await GuiSmokeShared.RunProcessAsync(
                Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
                "/c start \"\" \"steam://rungameid/2868840\"",
                workspaceRoot,
                TimeSpan.FromSeconds(10),
                waitForExit: false).ConfigureAwait(false);
            RecordBootstrapStage("bootstrap-launch-issued", "finished", launchIssuedAt.ToString("O"));

            await WaitForLiveGameWindowAsync(launchIssuedAt, TimeSpan.FromMinutes(2)).ConfigureAwait(false);
            RecordBootstrapStage("bootstrap-window-detected", "finished");
            await MaintainLaunchFocusAsync(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
            bootstrapVideo.TryStart(WindowLocator.TryFindSts2Window());
        }
        catch (Exception exception)
        {
            RecordBootstrapFailure($"{exception.GetType().Name}: {exception.Message}");
            bootstrapVideo.Complete(
                keepRecording: true,
                completionReason: $"bootstrap-failed:{exception.GetType().Name}:{exception.Message}");
            try
            {
                await StopGameProcessesAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
            }
            catch
            {
            }

            return false;
        }

        var captureService = new ScreenCaptureService(ResolveCaptureFaultInjectionOptions(options));
        var observerReader = new ObserverSnapshotReader(liveLayout, harnessLayout);
        var phase = GuiSmokePhase.WaitMainMenu;
        var history = Array.Empty<GuiSmokeHistoryEntry>();
        var freshnessFloor = DateTimeOffset.UtcNow.AddSeconds(-5);
        var captureIndex = 0;
        var firstScreenshotRecorded = false;
        var evaluationStarted = false;
        var consecutiveFallbackCapturesWithoutProcess = 0;

        while (DateTimeOffset.UtcNow < sessionDeadline)
        {
            captureIndex += 1;
            var window = WindowLocator.TryFindSts2Window()
                         ?? WindowLocator.GetPrimaryMonitorFallback();
            if (window.IsMinimized)
            {
                window = WindowLocator.EnsureRestored(window);
            }

            if (!window.IsFallback)
            {
                window = WindowLocator.EnsureInteractive(window);
            }

            var capturePrefix = Path.Combine(bootstrapRoot, captureIndex.ToString("0000"));
            var screenshotPath = capturePrefix + ".screen.png";
            var observerStatePath = capturePrefix + ".observer.state.json";
            var captureResult = captureService.TryCaptureDetailed(
                window,
                screenshotPath,
                ScreenCaptureService.CaptureTimeout,
                faultContext: new CaptureFaultInjectionContext("bootstrap", phase.ToString(), captureIndex, null));
            if (!captureResult.Succeeded)
            {
                if (captureResult.FailureKind is CaptureBoundaryFailureKind.TimedOut or CaptureBoundaryFailureKind.Exception)
                {
                    var bootstrapCaptureFailureReason = captureResult.FailureKind == CaptureBoundaryFailureKind.TimedOut
                        ? "bootstrap-capture-timeout"
                        : "bootstrap-capture-exception";
                    RecordBootstrapFailure(bootstrapCaptureFailureReason, new Dictionary<string, string?>
                    {
                        ["detail"] = captureResult.Detail,
                        ["exceptionType"] = captureResult.Exception?.GetType().Name,
                    });
                    bootstrapVideo.Complete(
                        keepRecording: true,
                        completionReason: bootstrapCaptureFailureReason);
                    try
                    {
                        await StopGameProcessesAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                    }
                    catch
                    {
                    }

                    return false;
                }

                if (WindowLocator.IsHungWindow(window))
                {
                    RecordBootstrapFailure("bootstrap-window-not-responding");
                    bootstrapVideo.Complete(
                        keepRecording: true,
                        completionReason: "bootstrap-window-not-responding");
                    try
                    {
                        await StopGameProcessesAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                    }
                    catch
                    {
                    }

                    return false;
                }

                if (window.IsFallback && !HasLiveGameProcess())
                {
                    consecutiveFallbackCapturesWithoutProcess += 1;
                    if (consecutiveFallbackCapturesWithoutProcess >= 3)
                    {
                        RecordBootstrapFailure("bootstrap-process-lost");
                        bootstrapVideo.Complete(
                            keepRecording: true,
                            completionReason: "bootstrap-process-lost");
                        try
                        {
                            await StopGameProcessesAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                        }
                        catch
                        {
                        }

                        return false;
                    }
                }
                else
                {
                    consecutiveFallbackCapturesWithoutProcess = 0;
                }

                await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
                continue;
            }

            consecutiveFallbackCapturesWithoutProcess = 0;
            if (!firstScreenshotRecorded)
            {
                RecordBootstrapStage("bootstrap-first-screenshot-captured", "finished", screenshotPath);
                firstScreenshotRecorded = true;
            }

            var observer = observerReader.Read();
            if (!IsManualCleanBootObserverReady(observer, freshnessFloor))
            {
                observer = await BootstrapManualCleanBootObserverAsync(
                        observer,
                        () => observerReader.Read(includeEventTail: false),
                        freshnessFloor,
                        ManualCleanBootObserverBootstrapPollCount,
                        ManualCleanBootObserverBootstrapPollMs)
                    .ConfigureAwait(false);
            }

            WriteObserverBootstrapCopy(observerStatePath, observer);
            if (!evaluationStarted)
            {
                RecordBootstrapStage("bootstrap-manual-clean-boot-evaluation-started", "started", screenshotPath);
                evaluationStarted = true;
            }

            var startupRuntimeEvidence = LongRunArtifacts.RecordStartupRuntimeEvidence(
                sessionRoot,
                configuration,
                liveLayout,
                harnessLayout,
                stage: "manual-clean-boot-runtime-evidence",
                captureReason: $"bootstrap-wait-main-menu-step-{captureIndex:D4}");

            var manualCleanBootVerified = LongRunArtifacts.TryMarkManualCleanBootVerified(
                sessionRoot,
                harnessLayout,
                observer,
                history,
                screenshotPath,
                observerStatePath,
                freshnessFloor,
                stillInWaitMainMenu: IsPreAttemptBootstrapBoundary(phase, history));
            var trustState = ResolveTrustStateAtAttemptStart(sessionRoot);
            if (CanCompleteBootstrap(manualCleanBootVerified, trustState, startupRuntimeEvidence, phase, history))
            {
                var detail = $"verified={manualCleanBootVerified};trustState={trustState};runtimeExporter={startupRuntimeEvidence.RuntimeExporterInitializedLogged};freshSnapshot={startupRuntimeEvidence.FreshSnapshotPresent}";
                RecordBootstrapStage("bootstrap-manual-clean-boot-evaluation-finished", "finished", detail);
                bootstrapVideo.Complete(
                    keepRecording: keepVideoOnSuccess,
                    completionReason: "bootstrap-succeeded");
                await StopGameProcessesAsync(TimeSpan.FromSeconds(20)).ConfigureAwait(false);
                EnsureGameNotRunning();
                RecordBootstrapStage("bootstrap-finished", "finished", detail);
                return true;
            }

            await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
        }

        RecordBootstrapFailure("bootstrap-timeout");
        bootstrapVideo.Complete(
            keepRecording: true,
            completionReason: "bootstrap-timeout");
        try
        {
            await StopGameProcessesAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        }
        catch
        {
        }

        return false;
    }

    static async Task StopGameProcessesAsync(TimeSpan timeout)
    {
        var relevantNames = GetRelevantGameProcessNames();

        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var processes = Process.GetProcesses()
                .Where(process => relevantNames.Contains(process.ProcessName))
                .ToArray();
            if (processes.Length == 0)
            {
                return;
            }

            foreach (var process in processes)
            {
                try
                {
                    if (process.HasExited)
                    {
                        continue;
                    }

                    LogHarness($"stopping process name={process.ProcessName} pid={process.Id}");
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                }
            }

            await Task.Delay(500).ConfigureAwait(false);
        }
    }

    static bool HasLiveGameProcess()
    {
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                if (!process.HasExited && GetRelevantGameProcessNames().Contains(process.ProcessName))
                {
                    return true;
                }
            }
            catch
            {
            }
        }

        return false;
    }

    static HashSet<string> GetRelevantGameProcessNames()
    {
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SlayTheSpire2",
            "crashpad_handler",
        };
    }

    static async Task WaitForLiveGameWindowAsync(DateTimeOffset launchedAt, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        var pollMs = GetLaunchPollingIntervalMs();
        while (DateTimeOffset.UtcNow < deadline)
        {
            var window = WindowLocator.TryFindSts2Window();
            if (window is not null)
            {
                if (window.IsMinimized)
                {
                    LogHarness($"window detected minimized; restoring title={window.Title}");
                    window = WindowLocator.EnsureRestored(window);
                }
                window = WindowLocator.EnsureInteractive(window);
                LogHarness($"window detected after launch title={window.Title} bounds={DescribeBounds(window.Bounds)}");
                return;
            }

            LogHarness($"waiting for STS2 window launchIssuedAt={launchedAt:O}");
            await Task.Delay(pollMs).ConfigureAwait(false);
        }

        throw new TimeoutException("Timed out waiting for a live STS2 game window.");
    }

    static async Task MaintainLaunchFocusAsync(TimeSpan duration, TimeSpan requiredStableWindow)
    {
        var deadline = DateTimeOffset.UtcNow.Add(duration);
        DateTimeOffset? stableSince = null;
        var pollMs = GetLaunchPollingIntervalMs();
        while (DateTimeOffset.UtcNow < deadline)
        {
            var window = WindowLocator.TryFindSts2Window();
            if (window is not null)
            {
                if (window.IsMinimized)
                {
                    window = WindowLocator.EnsureRestored(window);
                }

                window = WindowLocator.EnsureInteractive(window);
                LogHarness($"launch focus check title={window.Title} bounds={DescribeBounds(window.Bounds)}");
                stableSince ??= DateTimeOffset.UtcNow;
                if (DateTimeOffset.UtcNow - stableSince >= requiredStableWindow)
                {
                    return;
                }
            }
            else
            {
                stableSince = null;
            }

            await Task.Delay(pollMs).ConfigureAwait(false);
        }
    }

    static (string LaunchToken, string SentinelRelativePath) EnsureStartupRuntimeConfig(
        ScaffoldConfiguration configuration,
        string sessionId,
        string runId,
        DateTimeOffset launchIssuedAtUtc)
    {
        var runtimeConfigPath = Path.Combine(configuration.GamePaths.GameDirectory, "mods", configuration.AiCompanionMod.RuntimeConfigFileName);
        if (!File.Exists(runtimeConfigPath))
        {
            throw new FileNotFoundException("Runtime config was not deployed.", runtimeConfigPath);
        }

        var rootNode = JsonNode.Parse(File.ReadAllText(runtimeConfigPath))
                       ?? throw new InvalidOperationException("Failed to parse runtime config.");
        var rootObject = rootNode.AsObject();
        if (rootObject["harness"] is not JsonObject harnessObject)
        {
            throw new InvalidOperationException("Runtime config does not contain a harness section.");
        }

        var launchToken = Guid.NewGuid().ToString("N");
        const string sentinelRelativePath = "ai_companion/startup/loader-sentinel.latest.json";
        harnessObject["enabled"] = true;
        rootObject["startupSentinel"] = new JsonObject
        {
            ["sessionId"] = sessionId,
            ["runId"] = runId,
            ["launchToken"] = launchToken,
            ["launchIssuedAtUtc"] = launchIssuedAtUtc.ToString("O"),
            ["sentinelRelativePath"] = sentinelRelativePath,
        };
        File.WriteAllText(runtimeConfigPath, rootObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        LogHarness($"runtime config ensured harness.enabled=true startupSentinel.launchToken={launchToken} path={runtimeConfigPath}");
        return (launchToken, sentinelRelativePath);
    }

    static string WriteStartupSentinelFixture(
        GamePathOptions gamePaths,
        string sentinelRelativePath,
        string sessionId,
        string runId,
        string launchToken)
    {
        var sentinelPath = Path.Combine(gamePaths.UserDataRoot, sentinelRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(sentinelPath)!);
        var sentinelRoot = new JsonObject
        {
            ["capturedAtUtc"] = DateTimeOffset.UtcNow.ToString("O"),
            ["sessionId"] = sessionId,
            ["runId"] = runId,
            ["launchToken"] = launchToken,
            ["processName"] = "SlayTheSpire2",
            ["assemblyPath"] = @"D:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\mods\sts2-mod-ai-companion.dll",
        };
        File.WriteAllText(sentinelPath, sentinelRoot.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        return sentinelPath;
    }

    static void WriteRuntimeConfigFixture(
        ScaffoldConfiguration configuration,
        string modsRoot,
        bool harnessEnabled)
    {
        var runtimeConfigPath = Path.Combine(modsRoot, configuration.AiCompanionMod.RuntimeConfigFileName);
        var runtimeConfig = new JsonObject
        {
            ["enabled"] = true,
            ["gamePaths"] = JsonSerializer.SerializeToNode(configuration.GamePaths, GuiSmokeShared.JsonOptions),
            ["liveExport"] = JsonSerializer.SerializeToNode(configuration.LiveExport, GuiSmokeShared.JsonOptions),
            ["harness"] = JsonSerializer.SerializeToNode(configuration.Harness with { Enabled = harnessEnabled }, GuiSmokeShared.JsonOptions),
        };
        File.WriteAllText(runtimeConfigPath, runtimeConfig.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    static async Task<ObserverState> BootstrapManualCleanBootObserverAsync(
        ObserverState observer,
        Func<ObserverState> readObserver,
        DateTimeOffset freshnessFloor,
        int maxAdditionalPolls,
        int pollDelayMs,
        Func<int, Task>? delayAsync = null)
    {
        if (IsManualCleanBootObserverReady(observer, freshnessFloor))
        {
            return observer;
        }

        var delay = delayAsync ?? (milliseconds => Task.Delay(milliseconds));
        for (var pollIndex = 0; pollIndex < maxAdditionalPolls; pollIndex += 1)
        {
            await delay(pollDelayMs).ConfigureAwait(false);
            observer = readObserver();
            if (IsManualCleanBootObserverReady(observer, freshnessFloor))
            {
                break;
            }
        }

        return observer;
    }

    static bool IsManualCleanBootObserverReady(ObserverState observer, DateTimeOffset freshnessFloor)
    {
        return observer.IsFreshSince(freshnessFloor)
               && MainMenuRunStartObserverSignals.IsRunStartSurfaceReady(observer);
    }
}
