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
        case "run":
            return await RunScenarioAsync(configuration, workspaceRoot, options).ConfigureAwait(false);

        case "inspect-run":
            return InspectRun(options, workspaceRoot);

        case "inspect-session":
            return InspectSession(options, workspaceRoot);

        case "replay-step":
            return ReplayStep(options, workspaceRoot);

        case "replay-test":
            return ReplayGoldenScenes(options, workspaceRoot);

        case "self-test":
            RunSelfTest();
            Console.WriteLine("GUI smoke harness self-test passed.");
            return 0;

        default:
            WriteUsage();
            return 0;
    }
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}

static async Task<int> RunScenarioAsync(
    ScaffoldConfiguration configuration,
    string workspaceRoot,
    IReadOnlyDictionary<string, string> options)
{
    var scenarioId = options.TryGetValue("--scenario", out var scenarioRaw)
        ? scenarioRaw
        : "boot-to-combat";
    var isLongRun = string.Equals(scenarioId, "boot-to-long-run", StringComparison.OrdinalIgnoreCase);
    if (!isLongRun && !string.Equals(scenarioId, "boot-to-combat", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Unsupported scenario: {scenarioId}");
    }

    var providerKind = options.TryGetValue("--provider", out var providerRaw)
        ? providerRaw
        : isLongRun
            ? "headless"
            : "session";
    var liveLayout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
    var harnessLayout = HarnessPathResolver.Resolve(configuration.GamePaths, configuration.Harness);
    var sessionId = $"{DateTimeOffset.Now:yyyyMMdd-HHmmss}-{scenarioId}";
    var sessionRoot = options.TryGetValue("--run-root", out var explicitRunRoot)
        ? ResolveCliPath(explicitRunRoot, workspaceRoot)
        : Path.Combine(workspaceRoot, "artifacts", "gui-smoke", sessionId);
    var sessionDeadline = isLongRun
        ? TryGetPositiveIntOption(options, "--max-session-hours") is { } maxSessionHours
            ? DateTimeOffset.UtcNow.AddHours(maxSessionHours)
            : DateTimeOffset.MaxValue
        : DateTimeOffset.UtcNow.AddMinutes(10);
    var maxAttempts = TryGetPositiveIntOption(options, "--max-attempts")
        ?? (isLongRun ? int.MaxValue : 1);
    var maxConsecutiveLaunchFailures = TryGetPositiveIntOption(options, "--max-consecutive-launch-failures") ?? 3;
    var maxSceneDeadEnds = TryGetPositiveIntOption(options, "--max-scene-dead-ends") ?? 5;
    var maxSteps = TryGetPositiveIntOption(options, "--max-steps");
    var stopOnFirstTerminal = options.ContainsKey("--stop-on-first-terminal");
    var stopOnFirstLoop = options.ContainsKey("--stop-on-first-loop");

    if (isLongRun)
    {
        Directory.CreateDirectory(sessionRoot);
        Directory.CreateDirectory(Path.Combine(sessionRoot, "attempts"));
        LongRunArtifacts.InitializeSessionArtifacts(sessionRoot, sessionId, scenarioId, providerKind);
    }

    void RecordStartupStage(
        string stage,
        string status,
        string? detail = null,
        IReadOnlyDictionary<string, string?>? metadata = null)
    {
        if (!isLongRun)
        {
            return;
        }

        try
        {
            LongRunArtifacts.RecordStartupStage(sessionRoot, stage, status, detail, metadata);
        }
        catch (Exception exception)
        {
            LogHarness($"startup stage record failed stage={stage} status={status} error={exception.GetType().Name}: {exception.Message}");
        }
    }

    void RecordStartupFailure(
        string stage,
        string reason,
        IReadOnlyDictionary<string, string?>? metadata = null)
    {
        if (!isLongRun)
        {
            return;
        }

        try
        {
            LongRunArtifacts.RecordStartupFailure(sessionRoot, stage, reason, metadata);
        }
        catch (Exception exception)
        {
            LogHarness($"startup failure record failed stage={stage} reason={reason} error={exception.GetType().Name}: {exception.Message}");
        }
    }

    if (!options.ContainsKey("--skip-deploy"))
    {
        var startupStage = "game-stopped-before-deploy";
        try
        {
            EnsureGameNotRunning();
            if (isLongRun)
            {
                LongRunArtifacts.RecordGameStoppedBeforeDeployEvidence(sessionRoot);
                RecordStartupStage(startupStage, "finished");
            }

            var deployCommand = BuildDeployNativePackageCommand(configuration, workspaceRoot, options);
            startupStage = "deploy-command-selected";
            RecordStartupStage(
                startupStage,
                "finished",
                $"{deployCommand.Mode}:{deployCommand.Reason}",
                new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["deployMode"] = deployCommand.Mode,
                    ["toolPath"] = deployCommand.ToolPath,
                    ["reason"] = deployCommand.Reason,
                });

            startupStage = "deploy-command-started";
            RecordStartupStage(startupStage, "started");
            var deployResult = await RunDeployNativePackageAsync(configuration, workspaceRoot, options, deployCommand).ConfigureAwait(false);
            var deployFailureReason = BuildDeployCommandFailureReason(deployResult);
            if (isLongRun)
            {
                LongRunArtifacts.RecordDeployCommandResult(sessionRoot, deployCommand, deployResult, deployFailureReason);
            }

            startupStage = "deploy-command-finished";
            if (!string.IsNullOrWhiteSpace(deployFailureReason))
            {
                RecordStartupFailure(
                    startupStage,
                    deployFailureReason,
                    new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["deployMode"] = deployCommand.Mode,
                        ["toolPath"] = deployCommand.ToolPath,
                        ["exitCode"] = deployResult.ExitCode?.ToString(CultureInfo.InvariantCulture),
                        ["timedOut"] = deployResult.TimedOut.ToString(),
                    });
                throw new InvalidOperationException($"Deploy command failed: {deployFailureReason}");
            }

            RecordStartupStage(
                startupStage,
                "finished",
                $"exitCode={deployResult.ExitCode ?? 0};durationMs={deployResult.Duration.TotalMilliseconds:F0}",
                new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["deployMode"] = deployCommand.Mode,
                    ["toolPath"] = deployCommand.ToolPath,
                    ["exitCode"] = deployResult.ExitCode?.ToString(CultureInfo.InvariantCulture) ?? "0",
                    ["timedOut"] = "False",
                    ["durationMs"] = deployResult.Duration.TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture),
                });

            if (isLongRun)
            {
                startupStage = "deploy-verification-started";
                RecordStartupStage(startupStage, "started");
                LongRunArtifacts.RecordDeployVerificationEvidence(
                    sessionRoot,
                    configuration,
                    workspaceRoot,
                    includeHarnessBridge: true);
                var deployPrevalidation = JsonSerializer.Deserialize<GuiSmokePrevalidation>(
                    File.ReadAllText(Path.Combine(sessionRoot, "prevalidation.json")),
                    GuiSmokeShared.JsonOptions);
                startupStage = "deploy-verification-finished";
                RecordStartupStage(
                    startupStage,
                    "finished",
                    deployPrevalidation is null
                        ? "prevalidation-unreadable"
                        : $"modsPayloadReconciled={deployPrevalidation.ModsPayloadReconciled};deployIdentityVerified={deployPrevalidation.DeployIdentityVerified}");
            }
        }
        catch (Exception exception)
        {
            if (isLongRun)
            {
                RecordStartupFailure(startupStage, $"{exception.GetType().Name}: {exception.Message}");
                LongRunArtifacts.UpdatePrevalidation(
                    sessionRoot,
                    note: BuildDeployFailureNote(exception));
                LongRunArtifacts.UpdateRunnerSessionState(
                    sessionRoot,
                    GuiSmokeContractStates.SessionAborted,
                    $"runner aborted during deploy: {exception.GetType().Name}: {exception.Message}");
            }

            throw;
        }
    }
    else if (Directory.Exists(sessionRoot))
    {
        Directory.Delete(sessionRoot, recursive: true);
    }

    if (isLongRun && options.ContainsKey("--skip-deploy"))
    {
        LongRunArtifacts.UpdatePrevalidation(
            sessionRoot,
            note: "skip-deploy was enabled; trust gate remains invalid until required deploy proof is recorded.");
    }

    if (isLongRun && options.ContainsKey("--skip-launch"))
    {
        maxAttempts = Math.Min(maxAttempts, 1);
        LongRunArtifacts.UpdatePrevalidation(
            sessionRoot,
            note: "skip-launch was enabled; manual clean boot proof was not recorded by the runner.");
    }

    if (isLongRun && !options.ContainsKey("--skip-launch"))
    {
        LongRunArtifacts.UpdateRunnerSessionState(
            sessionRoot,
            GuiSmokeContractStates.SessionCollecting,
            "runner performing bootstrap before authoritative attempt 0001.");
        var bootstrapSucceeded = await RunBootstrapPhaseAsync(
            configuration,
            workspaceRoot,
            options,
            liveLayout,
            harnessLayout,
            sessionId,
            sessionRoot,
            sessionDeadline).ConfigureAwait(false);
        if (!bootstrapSucceeded)
        {
            LongRunArtifacts.UpdateRunnerSessionState(
                sessionRoot,
                GuiSmokeContractStates.SessionAborted,
                "runner aborted before authoritative attempt 0001 because bootstrap did not verify manual clean boot.");
            return 1;
        }
    }

    GuiSmokeAttemptResult? lastAttempt = null;
    var consecutiveLaunchFailures = 0;
    var consecutiveSceneDeadEnds = 0;
    for (var attemptOrdinal = 1; attemptOrdinal <= maxAttempts && DateTimeOffset.UtcNow < sessionDeadline; attemptOrdinal += 1)
    {
        var attemptId = attemptOrdinal.ToString("0000", CultureInfo.InvariantCulture);
        var trustStateAtStart = GuiSmokeContractStates.TrustInvalid;
        if (isLongRun)
        {
            if (lastAttempt is not null)
            {
                if (ShouldMarkSessionStalled(lastAttempt))
                {
                    LongRunArtifacts.UpdateRunnerSessionState(
                        sessionRoot,
                        GuiSmokeContractStates.SessionStalled,
                        $"runner observed terminal attempt {lastAttempt.AttemptId} before deciding restart.");
                }

                LongRunArtifacts.RecordRunnerBeginRestart(sessionRoot, lastAttempt, attemptId, attemptOrdinal);
            }

            LongRunArtifacts.UpdateRunnerSessionState(
                sessionRoot,
                GuiSmokeContractStates.SessionCollecting,
                $"runner starting attempt {attemptId}.");
            trustStateAtStart = LongRunArtifacts.RefreshSupervisorState(sessionRoot).TrustState;
        }

        lastAttempt = await RunAttemptAsync(
            configuration,
            workspaceRoot,
            options,
            scenarioId,
            providerKind,
            liveLayout,
            harnessLayout,
            sessionId,
            sessionRoot,
            isLongRun,
            attemptId,
            attemptOrdinal,
            sessionDeadline,
            trustStateAtStart,
            maxSteps).ConfigureAwait(false);

        if (!isLongRun)
        {
            return lastAttempt.ExitCode;
        }

        if (stopOnFirstTerminal)
        {
            LongRunArtifacts.UpdateRunnerSessionState(
                sessionRoot,
                GuiSmokeContractStates.SessionAborted,
                $"runner stopped after first terminal attempt {attemptId}.");
            return lastAttempt.ExitCode;
        }

        if (stopOnFirstLoop && IsLoopLikeAttempt(lastAttempt))
        {
            LongRunArtifacts.UpdateRunnerSessionState(
                sessionRoot,
                GuiSmokeContractStates.SessionAborted,
                $"runner stopped after first loop-classified attempt {attemptId}.");
            return lastAttempt.ExitCode;
        }

        consecutiveLaunchFailures = lastAttempt.LaunchFailed
            ? consecutiveLaunchFailures + 1
            : 0;
        consecutiveSceneDeadEnds = IsSceneDeadEndAttempt(lastAttempt!)
            ? consecutiveSceneDeadEnds + 1
            : 0;

        if (consecutiveLaunchFailures >= maxConsecutiveLaunchFailures)
        {
            LogHarness($"session abort consecutive launch failures={consecutiveLaunchFailures}");
            LongRunArtifacts.UpdateRunnerSessionState(
                sessionRoot,
                GuiSmokeContractStates.SessionAborted,
                $"runner aborted after {consecutiveLaunchFailures} consecutive launch failures.");
            return 1;
        }

        if (consecutiveSceneDeadEnds >= maxSceneDeadEnds)
        {
            LogHarness($"session abort consecutive scene dead-ends={consecutiveSceneDeadEnds}");
            LongRunArtifacts.UpdateRunnerSessionState(
                sessionRoot,
                GuiSmokeContractStates.SessionAborted,
                $"runner aborted after {consecutiveSceneDeadEnds} consecutive scene dead-ends.");
            return 1;
        }
    }

    if (isLongRun)
    {
        var supervisorState = LongRunArtifacts.RefreshSupervisorState(sessionRoot);
        var finalSessionState = string.Equals(supervisorState.MilestoneState, GuiSmokeContractStates.MilestoneDone, StringComparison.OrdinalIgnoreCase)
            ? GuiSmokeContractStates.SessionCompleted
            : GuiSmokeContractStates.SessionAborted;
        LongRunArtifacts.UpdateRunnerSessionState(
            sessionRoot,
            finalSessionState,
            finalSessionState == GuiSmokeContractStates.SessionCompleted
                ? "runner ended after milestone proof was completed."
                : "runner ended before milestone proof completed.");
    }

    return lastAttempt?.ExitCode ?? 1;
}

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
    const int TransitionSettleMs = 2000;
    const int ManualCleanBootObserverBootstrapPollMs = 500;
    const int ManualCleanBootObserverBootstrapPollCount = 12;

    var bootstrapRunId = $"{sessionId}-bootstrap";
    var startupStage = "bootstrap-started";
    var bootstrapRoot = Path.Combine(sessionRoot, "bootstrap");
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

    var captureService = new ScreenCaptureService();
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
        if (!captureService.TryCapture(window, screenshotPath))
        {
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
                    observerReader.Read,
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
                keepRecording: false,
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

static async Task<GuiSmokeAttemptResult> RunAttemptAsync(
    ScaffoldConfiguration configuration,
    string workspaceRoot,
    IReadOnlyDictionary<string, string> options,
    string scenarioId,
    string providerKind,
    LiveExportLayout liveLayout,
    HarnessQueueLayout harnessLayout,
    string sessionId,
    string sessionRoot,
    bool isLongRun,
    string attemptId,
    int attemptOrdinal,
    DateTimeOffset sessionDeadline,
    string trustStateAtStart,
    int? maxSteps)
{
    const int PassiveWaitMs = 1000;
    const int ActionSettleMinimumMs = 900;
    const int CombatActionSettleMinimumMs = 300;
    const int CombatNoOpProbeGraceMs = 350;
    const int TransitionSettleMs = 2000;
    const int ManualCleanBootObserverBootstrapPollMs = 500;
    const int ManualCleanBootObserverBootstrapPollCount = 12;

    var runId = isLongRun
        ? $"{sessionId}-attempt-{attemptId}"
        : sessionId;
    var runRoot = isLongRun
        ? Path.Combine(sessionRoot, "attempts", attemptId)
        : sessionRoot;

    if (Directory.Exists(runRoot))
    {
        Directory.Delete(runRoot, recursive: true);
    }

    Directory.CreateDirectory(runRoot);
    var stepsRoot = Path.Combine(runRoot, "steps");
    Directory.CreateDirectory(stepsRoot);

    var logger = new ArtifactRecorder(runRoot);
    using var attemptVideo = GuiSmokeVideoRecorder.Create(
        workspaceRoot,
        options,
        sessionId,
        runId,
        runRoot,
        sessionRoot,
        attemptId,
        "attempt");
    SetHarnessLogSink(logger.AppendHumanLog);
    logger.WriteRunManifest(new GuiSmokeRunManifest(
        runId,
        scenarioId,
        providerKind,
        DateTimeOffset.UtcNow,
        workspaceRoot,
        liveLayout.LiveRoot,
        harnessLayout.HarnessRoot,
        configuration.GamePaths.GameDirectory));
    var stepIndex = 0;
    var startupStage = "authoritative-attempt-started";
    var isAuthoritativeFirstAttempt = isLongRun && attemptOrdinal == 1;

    void RecordAttemptStartupStage(
        string stage,
        string status,
        string? detail = null,
        IReadOnlyDictionary<string, string?>? metadata = null)
    {
        if (!isAuthoritativeFirstAttempt)
        {
            return;
        }

        startupStage = stage;
        LongRunArtifacts.RecordStartupStage(sessionRoot, stage, status, detail, metadata);
    }

    void RecordAttemptStartupFailure(
        string reason,
        IReadOnlyDictionary<string, string?>? metadata = null)
    {
        if (!isAuthoritativeFirstAttempt)
        {
            return;
        }

        LongRunArtifacts.RecordStartupFailure(sessionRoot, startupStage, reason, metadata);
    }

    if (isAuthoritativeFirstAttempt)
    {
        RecordAttemptStartupStage("authoritative-attempt-started", "finished", runRoot);
    }

    GuiSmokeAttemptResult CompleteAttempt(
        int exitCode,
        string status,
        string message,
        bool launchFailed = false,
        string? terminalCause = null,
        string? failureClass = null)
    {
        logger.CompleteRun(status, message);
        logger.WriteValidationSummary(runId);
        var keepVideo = status switch
        {
            "completed" => false,
            _ => true,
        };
        attemptVideo.Complete(
            keepVideo,
            $"attempt-{status}:{terminalCause ?? failureClass ?? message}");
        if (isLongRun)
        {
            LongRunArtifacts.RecordAttemptTerminal(
                sessionRoot,
                attemptId,
                attemptOrdinal,
                runId,
                terminalCause,
                launchFailed,
                failureClass,
                trustStateAtStart);
            var selfMetaReview = LongRunArtifacts.WriteAttemptMetaReview(sessionRoot, attemptId, attemptOrdinal, runId, status, message, failureClass);
            logger.WriteSelfMetaReview(selfMetaReview);
            LongRunArtifacts.WriteSessionArtifacts(
                sessionRoot,
                logger,
                runId,
                scenarioId,
                providerKind,
                attemptId,
                attemptOrdinal,
                stepIndex,
                status,
                message,
                terminalCause,
                launchFailed,
                failureClass,
                trustStateAtStart);
        }

        return new GuiSmokeAttemptResult(
            attemptId,
            attemptOrdinal,
            runId,
            runRoot,
            exitCode,
            status,
            message,
            stepIndex,
            launchFailed,
            terminalCause,
            failureClass,
            trustStateAtStart);
    }

    try
    {
        if (!options.ContainsKey("--skip-launch"))
        {
            await StopGameProcessesAsync(TimeSpan.FromSeconds(20)).ConfigureAwait(false);
            EnsureGameNotRunning();
            var launchIssuedAt = DateTimeOffset.UtcNow;
            EnsureStartupRuntimeConfig(configuration, sessionId, runId, launchIssuedAt);
            startupStage = "authoritative-attempt-launch-issued";
            await GuiSmokeShared.RunProcessAsync(
                Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
                "/c start \"\" \"steam://rungameid/2868840\"",
                workspaceRoot,
                TimeSpan.FromSeconds(10),
                waitForExit: false).ConfigureAwait(false);
            if (isLongRun)
            {
                LongRunArtifacts.RecordRunnerLaunchIssued(sessionRoot, attemptId, attemptOrdinal, runId, trustStateAtStart);
            }

            startupStage = "authoritative-attempt-window-detected";
            await WaitForLiveGameWindowAsync(launchIssuedAt, TimeSpan.FromMinutes(2)).ConfigureAwait(false);
            await MaintainLaunchFocusAsync(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
            attemptVideo.TryStart(WindowLocator.TryFindSts2Window());
        }
    }
    catch (Exception exception)
    {
        RecordAttemptStartupFailure($"{exception.GetType().Name}: {exception.Message}");
        logger.WriteFailureSummary(new GuiSmokeFailureSummary(
            GuiSmokePhase.WaitMainMenu.ToString(),
            $"launch-failed: {exception.Message}",
            null,
            null,
            null));
        return CompleteAttempt(
            1,
            "failed",
            $"launch-failed: {exception.Message}",
            launchFailed: true,
            terminalCause: "launch-failed",
            failureClass: "launch-runtime-noise");
    }

    try
    {
    IGuiDecisionProvider provider = string.Equals(providerKind, "headless", StringComparison.OrdinalIgnoreCase)
        ? new HeadlessCodexDecisionProvider(options)
        : string.Equals(providerKind, "auto", StringComparison.OrdinalIgnoreCase)
            ? new AutoDecisionProvider()
            : new SessionDecisionProvider();

    var observerReader = new ObserverSnapshotReader(liveLayout, harnessLayout);
    var captureService = new ScreenCaptureService();
    var inputDriver = new MouseInputDriver();
    var evaluator = new ObserverAcceptanceEvaluator();

    var phase = GuiSmokePhase.WaitMainMenu;
    var attemptsByPhase = new Dictionary<GuiSmokePhase, int>();
    var history = new List<GuiSmokeHistoryEntry>();
    var waitDeadline = isLongRun
        ? sessionDeadline
        : DateTimeOffset.UtcNow.AddMinutes(10);
    var freshnessFloor = DateTimeOffset.UtcNow.AddSeconds(-5);
    var consecutiveBlackFrames = 0;
    string? lastActionFingerprint = null;
    var sameActionStallCount = 0;
    var attemptStartedRecorded = false;
    string? lastDecisionWaitFingerprint = null;
    var consecutiveDecisionWaitCount = 0;
    string? lastOverlayLoopFingerprint = null;
    var consecutiveOverlayLoopCount = 0;
    var consecutiveFallbackCapturesWithoutProcess = 0;
    var useDecisionAgeGuard = !string.Equals(providerKind, "auto", StringComparison.OrdinalIgnoreCase);
    var decisionStaleBudget = useDecisionAgeGuard
        ? TimeSpan.FromSeconds(2)
        : TimeSpan.FromSeconds(30);

    void ResetDecisionWaitTracking()
    {
        lastDecisionWaitFingerprint = null;
        consecutiveDecisionWaitCount = 0;
    }

    void ResetOverlayLoopTracking()
    {
        lastOverlayLoopFingerprint = null;
        consecutiveOverlayLoopCount = 0;
    }

    while (DateTimeOffset.UtcNow < waitDeadline)
    {
        if (maxSteps is int stepLimit && stepIndex >= stepLimit)
        {
            return CompleteAttempt(
                0,
                "completed",
                $"max-steps-reached:{stepLimit}",
                terminalCause: "max-steps-reached");
        }

        stepIndex += 1;
        var window = WindowLocator.TryFindSts2Window()
                     ?? WindowLocator.GetPrimaryMonitorFallback();
        if (window.IsMinimized)
        {
            LogHarness($"step={stepIndex} window minimized; restoring title={window.Title}");
            window = WindowLocator.EnsureRestored(window);
            LogHarness($"step={stepIndex} window restored={DescribeWindow(window)}");
        }
        if (!window.IsFallback)
        {
            window = WindowLocator.EnsureInteractive(window);
        }
        LogHarness($"step={stepIndex} phase={phase} window={DescribeWindow(window)}");
        var stepPrefix = Path.Combine(stepsRoot, stepIndex.ToString("0000"));
        var screenshotPath = stepPrefix + ".screen.png";
        if (!captureService.TryCapture(window, screenshotPath))
        {
            ResetDecisionWaitTracking();
            consecutiveBlackFrames += 1;
            LogHarness($"step={stepIndex} capture unusable; waiting for a valid process frame");
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "capture-unusable", null, null, null));
            if (window.IsFallback && !HasLiveGameProcess())
            {
                consecutiveFallbackCapturesWithoutProcess += 1;
                if (consecutiveFallbackCapturesWithoutProcess >= 3)
                {
                    RecordAttemptStartupFailure("process-lost");
                    logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                        phase.ToString(),
                        "process-lost",
                        null,
                        null,
                        screenshotPath));
                    return CompleteAttempt(
                        1,
                        "failed",
                        "process-lost",
                        terminalCause: "process-lost",
                        failureClass: "launch-runtime-noise");
                }
            }
            else
            {
                consecutiveFallbackCapturesWithoutProcess = 0;
            }
            if (!window.IsFallback && consecutiveBlackFrames >= 3)
            {
                var focusWindow = WindowLocator.EnsureInteractive(window);
                LogHarness($"step={stepIndex} black-frame streak={consecutiveBlackFrames}; sending center nudge");
                inputDriver.Click(focusWindow, 0.5, 0.5);
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "black-frame-nudge", "center", DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "black-frame-nudge", null, null, "center"));
                consecutiveBlackFrames = 0;
            }
            await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
            continue;
        }
        consecutiveBlackFrames = 0;
        consecutiveFallbackCapturesWithoutProcess = 0;
        if (isAuthoritativeFirstAttempt && stepIndex == 1)
        {
            RecordAttemptStartupStage("authoritative-first-screenshot-captured", "finished", screenshotPath);
        }
        if (isLongRun && !attemptStartedRecorded && stepIndex == 1)
        {
            LongRunArtifacts.RecordAttemptStarted(sessionRoot, attemptId, attemptOrdinal, runId, trustStateAtStart, screenshotPath);
            attemptStartedRecorded = true;
        }
        LogHarness($"step={stepIndex} captured={screenshotPath}");

        var observer = observerReader.Read();
        if (isLongRun
            && history.Count == 0
            && phase == GuiSmokePhase.WaitMainMenu
            && !IsManualCleanBootObserverReady(observer, freshnessFloor))
        {
            observer = await BootstrapManualCleanBootObserverAsync(
                    observer,
                    observerReader.Read,
                    freshnessFloor,
                    ManualCleanBootObserverBootstrapPollCount,
                    ManualCleanBootObserverBootstrapPollMs)
                .ConfigureAwait(false);
        }

        logger.WriteObserverCopies(stepPrefix, observer);
        LogHarness($"step={stepIndex} observer {DescribeObserverHuman(observer)} capturedAt={observer.CapturedAt?.ToString("O") ?? "null"}");
        var iterationSceneSignature = ComputeSceneSignature(screenshotPath, observer, phase);
        var iterationFirstSeenScene = isLongRun && !HasSceneSignatureHistory(sessionRoot, iterationSceneSignature);
        var iterationReasoningMode = DetermineReasoningMode(phase, observer, iterationFirstSeenScene);

        if (observer.SceneReady == false)
        {
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "scene-not-ready", observer.CurrentScreen, observer.InCombat, null));
        }

        if (isLongRun
            && history.Count > 0
            && phase is not GuiSmokePhase.WaitMainMenu and not GuiSmokePhase.EnterRun and not GuiSmokePhase.WaitCharacterSelect
            && string.Equals(observer.CurrentScreen, "main-menu", StringComparison.OrdinalIgnoreCase))
        {
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "terminal-detected", observer.CurrentScreen, observer.InCombat, "returned-main-menu"));
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                "returned-main-menu",
                observer.CurrentScreen,
                observer.InCombat,
                screenshotPath));
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    iterationSceneSignature,
                    observer,
                    null,
                    null,
                    iterationFirstSeenScene,
                    iterationReasoningMode,
                    false,
                    sameActionStallCount,
                    "terminal-returned-main-menu"));
            return CompleteAttempt(
                0,
                "completed",
                "returned-main-menu",
                terminalCause: "returned-main-menu");
        }

        if (isLongRun
            && history.Count > 0
            && phase is not GuiSmokePhase.WaitMainMenu and not GuiSmokePhase.EnterRun and not GuiSmokePhase.WaitCharacterSelect
            && observer.Summary.PlayerCurrentHp is <= 0)
        {
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "terminal-detected", observer.CurrentScreen, observer.InCombat, "player-defeated"));
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                "player-defeated",
                observer.CurrentScreen,
                observer.InCombat,
                screenshotPath));
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    iterationSceneSignature,
                    observer,
                    null,
                    null,
                    iterationFirstSeenScene,
                    iterationReasoningMode,
                    false,
                    sameActionStallCount,
                    "terminal-player-defeated"));
            return CompleteAttempt(
                0,
                "completed",
                "player-defeated",
                terminalCause: "player-defeated");
        }

        if (!observer.IsFreshSince(freshnessFloor))
        {
            ResetDecisionWaitTracking();
            LogHarness($"step={stepIndex} stale observer snapshot ignored freshnessFloor={freshnessFloor:O}");
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "stale-observer", observer.CurrentScreen, observer.InCombat, null));
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    iterationSceneSignature,
                    observer,
                    null,
                    null,
                    iterationFirstSeenScene,
                    iterationReasoningMode,
                    false,
                    sameActionStallCount,
                    "observer-stale"));
            await Task.Delay(PassiveWaitMs).ConfigureAwait(false);
            continue;
        }

        if (evaluator.IsPhaseSatisfied(phase, observer))
        {
            ResetDecisionWaitTracking();
            LogHarness($"step={stepIndex} observer accepted phase={phase}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "observer-accepted", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "observer-accepted", observer.CurrentScreen, observer.InCombat, null));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "observer-confirmed", observer.CurrentScreen, observer.InCombat, null));
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    iterationSceneSignature,
                    observer,
                    null,
                    null,
                    iterationFirstSeenScene,
                    iterationReasoningMode,
                    false,
                    sameActionStallCount,
                    "observer-confirmed"));
            phase = phase switch
            {
                GuiSmokePhase.WaitMainMenu => GuiSmokePhase.EnterRun,
                GuiSmokePhase.WaitCharacterSelect => GuiSmokePhase.ChooseCharacter,
                GuiSmokePhase.WaitMap => GuiSmokePhase.ChooseFirstNode,
                GuiSmokePhase.WaitCombat => GuiSmokePhase.HandleCombat,
                _ => phase,
            };

            if (phase == GuiSmokePhase.Completed)
            {
                return CompleteAttempt(
                    0,
                    "passed",
                    "combat flow accepted by observer",
                    terminalCause: "combat-flow-accepted");
            }

            await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
            continue;
        }

        if (TryAdvanceAlternateBranch(phase, observer, history, logger, stepIndex, isLongRun, out var alternatePhase))
        {
            ResetDecisionWaitTracking();
            LogHarness($"step={stepIndex} alternate branch {phase} -> {alternatePhase} from screen={observer.CurrentScreen ?? "null"}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    iterationSceneSignature,
                    observer,
                    null,
                    null,
                    iterationFirstSeenScene,
                    iterationReasoningMode,
                    false,
                    sameActionStallCount,
                    $"alternate-branch:{alternatePhase}"));
            phase = alternatePhase;

            if (phase == GuiSmokePhase.Completed)
            {
                return CompleteAttempt(
                    0,
                    "passed",
                    "combat flow accepted by observer",
                    terminalCause: "combat-flow-accepted");
            }

            await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
            continue;
        }

        if (IsPassiveWaitPhase(phase))
        {
            ResetDecisionWaitTracking();
            var waitAttempt = IncrementAttempt(attemptsByPhase, phase);
            if (phase == GuiSmokePhase.WaitCharacterSelect
                && string.Equals(observer.CurrentScreen, "main-menu", StringComparison.OrdinalIgnoreCase)
                && waitAttempt >= 2
                && waitAttempt % 2 == 0)
            {
                LogHarness($"step={stepIndex} still on main-menu after continue; retrying enter-run attempt={waitAttempt}");
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "retry-enter-run", observer.CurrentScreen, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "retry-enter-run", observer.CurrentScreen, observer.InCombat, null));
                phase = GuiSmokePhase.EnterRun;
                await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
                continue;
            }

            if (waitAttempt >= 30)
            {
                LogHarness($"step={stepIndex} timeout waiting for phase={phase} screen={observer.CurrentScreen ?? "null"}");
                logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                    phase.ToString(),
                    $"Timed out waiting for observer phase: {phase}",
                    observer.CurrentScreen,
                    observer.InCombat,
                    screenshotPath));
                return CompleteAttempt(
                    1,
                    "failed",
                    $"timeout at {phase}",
                    terminalCause: "phase-timeout",
                    failureClass: ClassifyFailureForAttempt(phase, observer, "phase-timeout", launchFailed: false));
            }

            LogHarness($"step={stepIndex} waiting phase={phase} attempt={waitAttempt} screen={observer.CurrentScreen ?? "null"}");
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "waiting", observer.CurrentScreen, observer.InCombat, null));
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    iterationSceneSignature,
                    observer,
                    null,
                    null,
                    iterationFirstSeenScene,
                    iterationReasoningMode,
                    false,
                    sameActionStallCount,
                    "passive-wait"));
            await Task.Delay(PassiveWaitMs).ConfigureAwait(false);
            continue;
        }

        var request = CreateStepRequest(
            runId,
            scenarioId,
            stepIndex,
            phase,
            screenshotPath,
            window,
            observer,
            history,
            workspaceRoot,
            sessionRoot,
            attemptId,
            attemptOrdinal);
        var requestPath = stepPrefix + ".request.json";
        var decisionPath = stepPrefix + ".decision.json";
        logger.WriteRequest(requestPath, request);
        LogHarness($"step={stepIndex} request={requestPath}");
        GuiSmokeReplayEvaluation replayEvaluation;
        GuiSmokeStepDecision decision;
        if (provider is AutoDecisionProvider)
        {
            replayEvaluation = EvaluateAutoDecisionWithDiagnostics(requestPath, request);
            decision = replayEvaluation.Decision;
        }
        else
        {
            decision = await provider.GetDecisionAsync(requestPath, decisionPath, TimeSpan.FromMinutes(3), CancellationToken.None).ConfigureAwait(false);
            replayEvaluation = EvaluateAutoDecisionWithDiagnostics(requestPath, request, decision);
        }
        WriteCandidateDumpArtifact(stepPrefix + ".candidates.json", replayEvaluation.CandidateDump);
        logger.WriteDecision(decisionPath, decision);
        LogHarness($"step={stepIndex} decision status={decision.Status} action={decision.ActionKind ?? "null"} target={decision.TargetLabel ?? "null"} confidence={decision.Confidence?.ToString("0.00") ?? "null"} reason={decision.Reason ?? "null"}");
        if (isLongRun)
        {
            LongRunArtifacts.MaybeRecordUnknownScene(sessionRoot, request, decision);
        }

        if (string.Equals(decision.Status, "abort", StringComparison.OrdinalIgnoreCase))
        {
            ResetDecisionWaitTracking();
            LogHarness($"step={stepIndex} aborted reason={decision.AbortReason ?? "null"}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    null,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    sameActionStallCount,
                    "decision-abort"));
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                decision.AbortReason ?? "decision aborted",
                observer.CurrentScreen,
                observer.InCombat,
                screenshotPath));
            return CompleteAttempt(
                1,
                "failed",
                decision.AbortReason ?? "decision aborted",
                terminalCause: "decision-abort",
                failureClass: ClassifyFailureForAttempt(phase, observer, "decision-abort", launchFailed: false));
        }

        if (string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase))
        {
            var waitFingerprint = BuildDecisionWaitFingerprint(phase, request.SceneSignature, observer);
            if (string.Equals(lastDecisionWaitFingerprint, waitFingerprint, StringComparison.Ordinal))
            {
                consecutiveDecisionWaitCount += 1;
            }
            else
            {
                lastDecisionWaitFingerprint = waitFingerprint;
                consecutiveDecisionWaitCount = 1;
            }

            if (TryClassifyDecisionWaitPlateau(phase, observer, consecutiveDecisionWaitCount, out var plateauTerminalCause, out var plateauMessage))
            {
                LogHarness($"step={stepIndex} abort {plateauMessage}");
                AppendProgressIfLongRun(
                    isLongRun,
                    logger,
                    EvaluateStepProgress(
                        stepIndex,
                        phase,
                        request.SceneSignature,
                        observer,
                        null,
                        decision,
                        request.FirstSeenScene,
                        request.ReasoningMode,
                        false,
                        sameActionStallCount,
                        "decision-wait",
                        "wait-plateau"));
                logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                    phase.ToString(),
                    plateauMessage,
                    observer.CurrentScreen,
                    observer.InCombat,
                    screenshotPath));
                return CompleteAttempt(
                    1,
                    "failed",
                    plateauMessage,
                    terminalCause: plateauTerminalCause,
                    failureClass: ClassifyFailureForAttempt(phase, observer, plateauTerminalCause, launchFailed: false));
            }

            if (TryClassifyRestSitePostClickNoOp(phase, request, decision, out var restSitePostClickNoOpMessage))
            {
                var restSiteTerminalCause = IsRestSitePostClickDecisionRisk(decision.DecisionRisk)
                    ? decision.DecisionRisk!
                    : "rest-site-post-click-noop";
                LogHarness($"step={stepIndex} abort {restSitePostClickNoOpMessage}");
                AppendProgressIfLongRun(
                    isLongRun,
                    logger,
                    EvaluateStepProgress(
                        stepIndex,
                        phase,
                        request.SceneSignature,
                        observer,
                        null,
                        decision,
                        request.FirstSeenScene,
                        request.ReasoningMode,
                        false,
                        sameActionStallCount,
                        "rest-site-post-click-noop"));
                logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                    phase.ToString(),
                    restSitePostClickNoOpMessage,
                    observer.CurrentScreen,
                    observer.InCombat,
                    screenshotPath));
                return CompleteAttempt(
                    1,
                    "failed",
                    restSitePostClickNoOpMessage,
                    terminalCause: restSiteTerminalCause,
                    failureClass: ClassifyFailureForAttempt(phase, observer, restSiteTerminalCause, launchFailed: false));
            }

            var waitMinimumMs = GetDecisionWaitMinimumMs(phase);
            LogHarness($"step={stepIndex} wait requested ms={Math.Max(250, decision.WaitMs ?? waitMinimumMs)}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "wait", decision.TargetLabel, DateTimeOffset.UtcNow)
            {
                Metadata = AutoDecisionProvider.BuildRestSiteHistoryMetadataForDecision(request, decision),
            });
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "wait", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    null,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    sameActionStallCount,
                    "decision-wait"));
            await Task.Delay(Math.Max(waitMinimumMs, decision.WaitMs ?? waitMinimumMs)).ConfigureAwait(false);
            continue;
        }

        if (LooksLikeInspectOverlayState(observer) && IsOverlayCleanupTarget(decision.TargetLabel))
        {
            var overlayFingerprint = BuildOverlayLoopFingerprint(request.SceneSignature, observer);
            if (string.Equals(lastOverlayLoopFingerprint, overlayFingerprint, StringComparison.Ordinal))
            {
                consecutiveOverlayLoopCount += 1;
            }
            else
            {
                lastOverlayLoopFingerprint = overlayFingerprint;
                consecutiveOverlayLoopCount = 1;
            }

            if (consecutiveOverlayLoopCount >= 4)
            {
                var overlayLoopMessage = $"inspect-overlay-loop phase={phase} screen={observer.CurrentScreen ?? "null"} overlayAttempts={consecutiveOverlayLoopCount}";
                LogHarness($"step={stepIndex} abort {overlayLoopMessage}");
                AppendProgressIfLongRun(
                    isLongRun,
                    logger,
                    EvaluateStepProgress(
                        stepIndex,
                        phase,
                        request.SceneSignature,
                        observer,
                        null,
                        decision,
                        request.FirstSeenScene,
                        request.ReasoningMode,
                        false,
                        sameActionStallCount,
                        "inspect-overlay-loop"));
                logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                    phase.ToString(),
                    overlayLoopMessage,
                    observer.CurrentScreen,
                    observer.InCombat,
                    screenshotPath));
                return CompleteAttempt(
                    1,
                    "failed",
                    overlayLoopMessage,
                    terminalCause: "inspect-overlay-loop",
                    failureClass: ClassifyFailureForAttempt(phase, observer, "inspect-overlay-loop", launchFailed: false));
            }
        }
        else if (!LooksLikeInspectOverlayState(observer) || !string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase))
        {
            ResetOverlayLoopTracking();
        }
        else if (!IsOverlayCleanupTarget(decision.TargetLabel))
        {
            ResetOverlayLoopTracking();
        }

        ResetDecisionWaitTracking();

        var decisionAge = DateTimeOffset.UtcNow - request.IssuedAt;
        if (useDecisionAgeGuard && decisionAge > decisionStaleBudget)
        {
            LogHarness($"step={stepIndex} recapture required stale-decision ageMs={(int)decisionAge.TotalMilliseconds}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "stale-decision", decision.TargetLabel, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "stale-decision", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    null,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    sameActionStallCount,
                    "decision-stale"));
            await Task.Delay(GetDecisionWaitMinimumMs(phase)).ConfigureAwait(false);
            continue;
        }

        var latestObserver = observerReader.Read();
        if (ShouldRecaptureForObserverDrift(request.Observer, latestObserver))
        {
            LogHarness($"step={stepIndex} recapture required observer-drift requestScreen={request.Observer.CurrentScreen ?? "null"} latestScreen={latestObserver.CurrentScreen ?? "null"} requestVisible={request.Observer.VisibleScreen ?? "null"} latestVisible={latestObserver.VisibleScreen ?? "null"}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "observer-drift", decision.TargetLabel, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "observer-drift", latestObserver.CurrentScreen, latestObserver.InCombat, decision.TargetLabel));
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    latestObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    sameActionStallCount,
                    "observer-drift"));
            await Task.Delay(GetDecisionWaitMinimumMs(phase)).ConfigureAwait(false);
            continue;
        }

        ValidateDecision(phase, request, decision);

        var rewardMapRecoveryAttempt = false;
        if (TryClassifyRewardMapLoop(phase, request, decision, out var rewardMapLoopMessage))
        {
            if (ShouldAllowRewardMapRecovery(request, decision))
            {
                rewardMapRecoveryAttempt = true;
                history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "reward-map-recovery", decision.TargetLabel, DateTimeOffset.UtcNow));
                logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "reward-map-recovery", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
                LogHarness($"step={stepIndex} recovery reward-map-loop delayed target={decision.TargetLabel ?? "null"}");
            }
            else
            {
            LogHarness($"step={stepIndex} abort {rewardMapLoopMessage}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    latestObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    sameActionStallCount,
                    "reward-map-loop"));
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                rewardMapLoopMessage,
                observer.CurrentScreen,
                observer.InCombat,
                screenshotPath));
            return CompleteAttempt(
                1,
                "failed",
                rewardMapLoopMessage,
                terminalCause: "reward-map-loop",
                failureClass: ClassifyFailureForAttempt(phase, observer, "reward-map-loop", launchFailed: false));
            }
        }

        if (TryClassifyMapOverlayNoOpLoop(phase, request, decision, out var mapOverlayNoOpLoopMessage))
        {
            LogHarness($"step={stepIndex} abort {mapOverlayNoOpLoopMessage}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    latestObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    sameActionStallCount,
                    "map-overlay-noop-loop"));
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                mapOverlayNoOpLoopMessage,
                observer.CurrentScreen,
                observer.InCombat,
                screenshotPath));
            return CompleteAttempt(
                1,
                "failed",
                mapOverlayNoOpLoopMessage,
                terminalCause: "map-overlay-noop-loop",
                failureClass: ClassifyFailureForAttempt(phase, observer, "map-overlay-noop-loop", launchFailed: false));
        }

        if (TryClassifyMapTransitionStall(phase, request, decision, out var mapTransitionStallMessage))
        {
            LogHarness($"step={stepIndex} abort {mapTransitionStallMessage}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    latestObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    sameActionStallCount,
                    "map-transition-stall"));
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                mapTransitionStallMessage,
                observer.CurrentScreen,
                observer.InCombat,
                screenshotPath));
            return CompleteAttempt(
                1,
                "failed",
                mapTransitionStallMessage,
                terminalCause: "map-transition-stall",
                failureClass: ClassifyFailureForAttempt(phase, observer, "map-transition-stall", launchFailed: false));
        }

        if (TryClassifyCombatNoOpLoop(phase, request, decision, out var combatNoOpLoopMessage))
        {
            LogHarness($"step={stepIndex} abort {combatNoOpLoopMessage}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    latestObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    sameActionStallCount,
                    "combat-noop-loop"));
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                combatNoOpLoopMessage,
                observer.CurrentScreen,
                observer.InCombat,
                screenshotPath));
            return CompleteAttempt(
                1,
                "failed",
                combatNoOpLoopMessage,
                terminalCause: "combat-noop-loop",
                failureClass: ClassifyFailureForAttempt(phase, observer, "combat-noop-loop", launchFailed: false));
        }

        var screenshotFingerprint = ComputeFileFingerprint(screenshotPath);
        var actionFingerprint = string.Join("|",
            phase.ToString(),
            screenshotFingerprint,
            decision.ActionKind ?? "null",
            decision.TargetLabel ?? "null");
        if (string.Equals(lastActionFingerprint, actionFingerprint, StringComparison.Ordinal))
        {
            sameActionStallCount += 1;
        }
        else
        {
            lastActionFingerprint = actionFingerprint;
            sameActionStallCount = 0;
        }

        if (sameActionStallCount >= GetSameActionStallLimit(phase, decision))
        {
            var abortReason = $"same-action-stall phase={phase} target={decision.TargetLabel ?? "null"} screen={observer.CurrentScreen ?? "null"} inventory={observer.InventoryId ?? "null"}";
            LogHarness($"step={stepIndex} abort {abortReason}");
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    latestObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    sameActionStallCount,
                    "same-action-stall"));
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                abortReason,
                observer.CurrentScreen,
                observer.InCombat,
                screenshotPath));
            return CompleteAttempt(
                1,
                "failed",
                abortReason,
                terminalCause: "same-action-stall",
                failureClass: ClassifyFailureForAttempt(phase, observer, "same-action-stall", launchFailed: false));
        }

        var clickWindow = WindowLocator.Refresh(window);
        if (clickWindow.IsMinimized)
        {
            LogHarness($"step={stepIndex} click blocked: window minimized before action; restoring title={clickWindow.Title}");
            clickWindow = WindowLocator.EnsureRestored(clickWindow);
            LogHarness($"step={stepIndex} click window restored={DescribeWindow(clickWindow)}");
        }
        if (WindowLocator.HasMeaningfulDrift(window, clickWindow))
        {
            LogHarness($"step={stepIndex} recapture required captureBounds={DescribeBounds(window.Bounds)} clickBounds={DescribeBounds(clickWindow.Bounds)}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "recapture-required", decision.TargetLabel, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "recapture-required", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
            AppendProgressIfLongRun(
                isLongRun,
                logger,
                EvaluateStepProgress(
                    stepIndex,
                    phase,
                    request.SceneSignature,
                    observer,
                    latestObserver,
                    decision,
                    request.FirstSeenScene,
                    request.ReasoningMode,
                    false,
                    sameActionStallCount,
                    "window-drift"));
            await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
            continue;
        }

        if (string.Equals(decision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase))
        {
            if (IsNonEnemyCombatSelectionLabel(decision.TargetLabel))
            {
                inputDriver.MoveCursor(clickWindow, GuiSmokeCombatConstants.NonEnemyPrimeNormalizedX, GuiSmokeCombatConstants.NonEnemyPrimeNormalizedY);
                LogHarness($"step={stepIndex} cursor primed for non-enemy staging point normalized=({GuiSmokeCombatConstants.NonEnemyPrimeNormalizedX:0.000},{GuiSmokeCombatConstants.NonEnemyPrimeNormalizedY:0.000})");
            }

            LogHarness($"step={stepIndex} key target={decision.TargetLabel ?? decision.KeyText ?? "null"} key={decision.KeyText ?? "null"}");
            inputDriver.PressKey(clickWindow, decision.KeyText!);
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "press-key", decision.TargetLabel ?? decision.KeyText, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "press-key", observer.CurrentScreen, observer.InCombat, decision.TargetLabel ?? decision.KeyText));
            LogHarness($"step={stepIndex} key sent key={decision.KeyText ?? "null"}");
        }
        else if (string.Equals(decision.ActionKind, "right-click", StringComparison.OrdinalIgnoreCase))
        {
            var rightClickX = decision.NormalizedX ?? 0.5;
            var rightClickY = decision.NormalizedY ?? 0.5;
            var clickPoint = MouseInputDriver.TransformNormalizedPoint(clickWindow, rightClickX, rightClickY);
            LogHarness($"step={stepIndex} right-click target={decision.TargetLabel ?? "null"} normalized=({rightClickX:0.000},{rightClickY:0.000}) absolute=({clickPoint.X},{clickPoint.Y}) bounds={DescribeBounds(clickWindow.Bounds)}");
            inputDriver.RightClick(clickWindow, rightClickX, rightClickY);
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "right-click", decision.TargetLabel, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "right-click", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
            LogHarness($"step={stepIndex} right-click sent target={decision.TargetLabel ?? "null"}");
        }
        else if (string.Equals(decision.ActionKind, "click-current", StringComparison.OrdinalIgnoreCase))
        {
            LogHarness($"step={stepIndex} click-current target={decision.TargetLabel ?? "null"}");
            inputDriver.ClickCurrent(clickWindow);
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "click-current", decision.TargetLabel, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "click-current", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
            LogHarness($"step={stepIndex} click-current sent target={decision.TargetLabel ?? "null"}");
        }
        else if (string.Equals(decision.ActionKind, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase))
        {
            LogHarness($"step={stepIndex} confirm-non-enemy target={decision.TargetLabel ?? "null"} normalized=({GuiSmokeCombatConstants.NonEnemyConfirmNormalizedX:0.000},{GuiSmokeCombatConstants.NonEnemyConfirmNormalizedY:0.000}) holdMs={GuiSmokeCombatConstants.NonEnemyConfirmHoldMs}");
            inputDriver.ConfirmNonEnemy(clickWindow, GuiSmokeCombatConstants.NonEnemyConfirmNormalizedX, GuiSmokeCombatConstants.NonEnemyConfirmNormalizedY, GuiSmokeCombatConstants.NonEnemyConfirmHoldMs);
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "confirm-non-enemy", decision.TargetLabel, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "confirm-non-enemy", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
            LogHarness($"step={stepIndex} confirm-non-enemy sent target={decision.TargetLabel ?? "null"}");
        }
        else
        {
            var clickPoint = MouseInputDriver.TransformNormalizedPoint(clickWindow, decision.NormalizedX!.Value, decision.NormalizedY!.Value);
            LogHarness($"step={stepIndex} click target={decision.TargetLabel ?? "null"} normalized=({decision.NormalizedX:0.000},{decision.NormalizedY:0.000}) absolute=({clickPoint.X},{clickPoint.Y}) bounds={DescribeBounds(clickWindow.Bounds)}");
            inputDriver.Click(clickWindow, decision.NormalizedX!.Value, decision.NormalizedY!.Value);
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "click", decision.TargetLabel, DateTimeOffset.UtcNow)
            {
                Metadata = AutoDecisionProvider.BuildRestSiteHistoryMetadataForDecision(request, decision),
            });
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "click", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
            LogHarness($"step={stepIndex} click sent target={decision.TargetLabel ?? "null"}");
        }

        var completedPhase = phase;
        var recipeRecorded = false;
        if (isLongRun)
        {
            LongRunArtifacts.AppendSceneRecipe(sessionRoot, request, decision);
            recipeRecorded = !string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                             && !string.Equals(decision.Status, "abort", StringComparison.OrdinalIgnoreCase)
                             && !string.IsNullOrWhiteSpace(decision.ActionKind);
        }

        var settleDelayMs = GetActionSettleDelayMs(completedPhase, decision, ActionSettleMinimumMs, CombatActionSettleMinimumMs);
        var progressProbeDelayMs = Math.Min(settleDelayMs, completedPhase == GuiSmokePhase.HandleCombat ? 250 : 500);
        if (progressProbeDelayMs > 0)
        {
            await Task.Delay(progressProbeDelayMs).ConfigureAwait(false);
        }

        var extraProgressSignals = new List<string>();
        if (rewardMapRecoveryAttempt)
        {
            extraProgressSignals.Add("reward-map-recovery-attempt");
        }

        var postActionObserver = observerReader.Read();
        var additionalProbeDelayMs = 0;
        if (ShouldGrantCombatNoOpProbeGrace(completedPhase, observer, postActionObserver, decision))
        {
            additionalProbeDelayMs = CombatNoOpProbeGraceMs;
            extraProgressSignals.Add("combat-noop-probe-grace");
            LogHarness($"step={stepIndex} combat noop probe grace target={decision.TargetLabel ?? "null"} delayMs={additionalProbeDelayMs}");
            await Task.Delay(additionalProbeDelayMs).ConfigureAwait(false);
            postActionObserver = observerReader.Read();
            if (!ShouldGrantCombatNoOpProbeGrace(completedPhase, observer, postActionObserver, decision))
            {
                extraProgressSignals.Add("combat-noop-probe-recovered");
                LogHarness($"step={stepIndex} combat noop probe recovered target={decision.TargetLabel ?? "null"} screen={postActionObserver.CurrentScreen ?? postActionObserver.VisibleScreen ?? "null"}");
            }
        }

        if (TryRecordCombatNoOpObservation(
                completedPhase,
                observer,
                postActionObserver,
                decision,
                history,
                logger,
                stepIndex,
                out var combatNoOpSignal))
        {
            extraProgressSignals.Add(combatNoOpSignal);
        }

        AppendProgressIfLongRun(
            isLongRun,
            logger,
            EvaluateStepProgress(
                stepIndex,
                completedPhase,
                request.SceneSignature,
                observer,
                postActionObserver,
                decision,
                request.FirstSeenScene,
                request.ReasoningMode,
                recipeRecorded,
                sameActionStallCount,
                extraProgressSignals.ToArray()));

        phase = phase switch
        {
            GuiSmokePhase.EnterRun => GuiSmokePhase.WaitCharacterSelect,
            GuiSmokePhase.ChooseCharacter => GuiSmokePhase.Embark,
            GuiSmokePhase.Embark => GuiSmokePhase.WaitMap,
            GuiSmokePhase.HandleRewards => GetPostRewardPhase(decision),
            GuiSmokePhase.ChooseFirstNode => GetPostChooseFirstNodePhase(decision),
            GuiSmokePhase.HandleEvent => GetPostHandleEventPhase(decision),
            GuiSmokePhase.HandleCombat => GuiSmokePhase.HandleCombat,
            _ => phase,
        };

        if (phase == GuiSmokePhase.Embark && GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(postActionObserver, out var postEmbarkPhase))
        {
            LogHarness($"step={stepIndex} post-action phase reconciliation Embark -> {postEmbarkPhase} from screen={postActionObserver.CurrentScreen ?? "null"}");
            phase = postEmbarkPhase;
        }

        attemptsByPhase.Clear();
        LogHarness($"step={stepIndex} next phase={phase}");
        var remainingSettleDelayMs = Math.Max(0, settleDelayMs - progressProbeDelayMs - additionalProbeDelayMs);
        if (remainingSettleDelayMs > 0)
        {
            await Task.Delay(remainingSettleDelayMs).ConfigureAwait(false);
        }
    }

    var finalObserver = observerReader.Read();
    logger.WriteFailureSummary(new GuiSmokeFailureSummary(
        phase.ToString(),
        "Global timeout reached.",
        finalObserver.CurrentScreen,
        finalObserver.InCombat,
        null));
    return CompleteAttempt(
        1,
        "failed",
        "global timeout",
        terminalCause: "global-timeout",
        failureClass: ClassifyFailureForAttempt(phase, finalObserver, "global-timeout", launchFailed: false));
    }
    catch (Exception exception)
    {
        logger.WriteFailureSummary(new GuiSmokeFailureSummary(
            "runner",
            $"unexpected-exception: {exception.Message}",
            null,
            null,
            null));
        return CompleteAttempt(
            1,
            "failed",
            $"unexpected-exception: {exception.Message}",
            terminalCause: "unexpected-exception",
            failureClass: "generic-recovery-failure");
    }
}

static int? TryGetPositiveIntOption(IReadOnlyDictionary<string, string> options, string key)
{
    if (!options.TryGetValue(key, out var raw)
        || !int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
        || parsed <= 0)
    {
        return null;
    }

    return parsed;
}

static int GetDecisionWaitMinimumMs(GuiSmokePhase phase)
{
    return phase == GuiSmokePhase.HandleCombat
        ? 250
        : 750;
}

static int GetActionSettleDelayMs(
    GuiSmokePhase phase,
    GuiSmokeStepDecision decision,
    int defaultMinimumMs,
    int combatMinimumMs)
{
    var minimumMs = phase == GuiSmokePhase.HandleCombat
        ? combatMinimumMs
        : defaultMinimumMs;
    return Math.Max(minimumMs, decision.WaitMs ?? minimumMs);
}

static bool IsNonEnemyCombatSelectionLabel(string? targetLabel)
{
    if (string.IsNullOrWhiteSpace(targetLabel))
    {
        return false;
    }

    return targetLabel.StartsWith("combat select non-enemy slot ", StringComparison.OrdinalIgnoreCase)
           || targetLabel.StartsWith("combat select defend slot ", StringComparison.OrdinalIgnoreCase);
}

static bool ShouldMarkSessionStalled(GuiSmokeAttemptResult result)
{
    return string.Equals(result.Status, "failed", StringComparison.OrdinalIgnoreCase)
           && !result.LaunchFailed
           && !string.Equals(result.FailureClass, "launch-runtime-noise", StringComparison.OrdinalIgnoreCase);
}

static bool IsLoopLikeAttempt(GuiSmokeAttemptResult result)
{
    return (result.TerminalCause?.Contains("loop", StringComparison.OrdinalIgnoreCase) ?? false)
           || (result.TerminalCause?.Contains("stall", StringComparison.OrdinalIgnoreCase) ?? false)
           || IsRestSitePostClickFailureKind(result.TerminalCause)
           || (result.FailureClass?.Contains("loop", StringComparison.OrdinalIgnoreCase) ?? false)
           || (result.FailureClass?.Contains("stall", StringComparison.OrdinalIgnoreCase) ?? false)
           || IsRestSitePostClickFailureKind(result.FailureClass);
}

static string ClassifyFailureForAttempt(
    GuiSmokePhase phase,
    ObserverState? observer,
    string terminalCause,
    bool launchFailed)
{
    if (launchFailed
        || string.Equals(terminalCause, "launch-failed", StringComparison.OrdinalIgnoreCase)
        || string.Equals(terminalCause, "process-lost", StringComparison.OrdinalIgnoreCase))
    {
        return "launch-runtime-noise";
    }

    if (IsSceneAuthorityInvalidFailure(phase, observer))
    {
        return "scene-authority-invalid";
    }

    return terminalCause switch
    {
        "reward-map-loop" => "reward-map-loop",
        "map-overlay-noop-loop" => "map-overlay-noop-loop",
        "map-transition-stall" => "map-transition-stall",
        "phase-mismatch-stall" => "phase-mismatch-stall",
        "decision-wait-plateau" => "decision-wait-plateau",
        "inspect-overlay-loop" => "inspect-overlay-loop",
        "combat-noop-loop" => "combat-noop-loop",
        "rest-site-post-click-noop" => "rest-site-post-click-noop",
        "rest-site-selection-failed" => "rest-site-selection-failed",
        "rest-site-grid-not-visible-after-selection" => "rest-site-grid-not-visible-after-selection",
        "rest-site-grid-observer-miss" => "rest-site-grid-observer-miss",
        "same-action-stall" => "screenshot-heuristic-drift",
        "decision-abort" => "semantic-scene-ambiguity",
        "phase-timeout" => "observer-blindspot",
        "global-timeout" => "observer-blindspot",
        _ => "generic-recovery-failure",
    };
}

static bool IsSceneAuthorityInvalidFailure(GuiSmokePhase phase, ObserverState? observer)
{
    var currentScreen = observer?.CurrentScreen;
    var visibleScreen = observer?.VisibleScreen;
    var sceneAuthority = observer?.SceneAuthority;
    return string.Equals(currentScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase)
           || string.Equals(visibleScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase)
           || string.Equals(currentScreen, "character-select", StringComparison.OrdinalIgnoreCase)
           || string.Equals(visibleScreen, "character-select", StringComparison.OrdinalIgnoreCase)
           || (string.Equals(currentScreen, "main-menu", StringComparison.OrdinalIgnoreCase)
               && (phase is GuiSmokePhase.EnterRun or GuiSmokePhase.WaitCharacterSelect or GuiSmokePhase.ChooseCharacter)
               && !string.Equals(sceneAuthority, "observer", StringComparison.OrdinalIgnoreCase));
}

static bool IsSceneDeadEndAttempt(GuiSmokeAttemptResult result)
{
    if (result.LaunchFailed)
    {
        return false;
    }

    return string.Equals(result.TerminalCause, "same-action-stall", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.TerminalCause, "reward-map-loop", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.TerminalCause, "map-overlay-noop-loop", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.TerminalCause, "map-transition-stall", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.TerminalCause, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.TerminalCause, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.TerminalCause, "inspect-overlay-loop", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.TerminalCause, "combat-noop-loop", StringComparison.OrdinalIgnoreCase)
           || IsRestSitePostClickFailureKind(result.TerminalCause)
           || string.Equals(result.TerminalCause, "decision-abort", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.TerminalCause, "phase-timeout", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.FailureClass, "reward-map-loop", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.FailureClass, "map-overlay-noop-loop", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.FailureClass, "map-transition-stall", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.FailureClass, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.FailureClass, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.FailureClass, "inspect-overlay-loop", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.FailureClass, "combat-noop-loop", StringComparison.OrdinalIgnoreCase)
           || IsRestSitePostClickFailureKind(result.FailureClass)
           || string.Equals(result.FailureClass, "scene-authority-invalid", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.FailureClass, "observer-blindspot", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.FailureClass, "semantic-scene-ambiguity", StringComparison.OrdinalIgnoreCase)
           || string.Equals(result.FailureClass, "screenshot-heuristic-drift", StringComparison.OrdinalIgnoreCase);
}

static bool IsRestSitePostClickFailureKind(string? value)
{
    return string.Equals(value, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, "rest-site-selection-failed", StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, "rest-site-grid-not-visible-after-selection", StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, "rest-site-grid-observer-miss", StringComparison.OrdinalIgnoreCase);
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

static int InspectRun(IReadOnlyDictionary<string, string> options, string workspaceRoot)
{
    if (!options.TryGetValue("--run-root", out var runRoot))
    {
        throw new InvalidOperationException("--run-root is required.");
    }

    var resolvedRoot = ResolveCliPath(runRoot, workspaceRoot);
    var manifestPath = Path.Combine(resolvedRoot, "run.json");
    if (!File.Exists(manifestPath))
    {
        throw new FileNotFoundException("Run manifest not found.", manifestPath);
    }

    var manifest = JsonSerializer.Deserialize<GuiSmokeRunManifest>(File.ReadAllText(manifestPath), GuiSmokeShared.JsonOptions)
                   ?? throw new InvalidOperationException("Failed to parse run manifest.");
    var failurePath = Path.Combine(resolvedRoot, "failure-summary.json");
    GuiSmokeFailureSummary? failure = null;
    if (File.Exists(failurePath))
    {
        failure = JsonSerializer.Deserialize<GuiSmokeFailureSummary>(File.ReadAllText(failurePath), GuiSmokeShared.JsonOptions);
    }

    Console.WriteLine(JsonSerializer.Serialize(new
    {
        manifest.RunId,
        manifest.ScenarioId,
        manifest.Provider,
        manifest.StartedAt,
        Status = manifest.Status ?? "in-progress",
        manifest.ResultMessage,
        Failure = failure,
        Video = TryReadJson<GuiSmokeVideoRecordingMetadata>(Path.Combine(resolvedRoot, "video-recording.json")),
        Steps = Directory.Exists(Path.Combine(resolvedRoot, "steps"))
            ? Directory.GetFiles(Path.Combine(resolvedRoot, "steps"), "*.screen.png").Length
            : 0,
    }, GuiSmokeShared.JsonOptions));
    return 0;
}

static int InspectSession(IReadOnlyDictionary<string, string> options, string workspaceRoot)
{
    if (!options.TryGetValue("--session-root", out var sessionRoot))
    {
        throw new InvalidOperationException("--session-root is required.");
    }

    var resolvedRoot = ResolveCliPath(sessionRoot, workspaceRoot);
    if (!Directory.Exists(resolvedRoot))
    {
        throw new DirectoryNotFoundException(resolvedRoot);
    }

    LongRunArtifacts.RefreshStallSentinel(resolvedRoot);
    var supervisorState = LongRunArtifacts.RefreshSupervisorState(resolvedRoot);
    var goalContract = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(Path.Combine(resolvedRoot, "goal-contract.json")), GuiSmokeShared.JsonOptions);
    var prevalidation = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(Path.Combine(resolvedRoot, "prevalidation.json")), GuiSmokeShared.JsonOptions);
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        SessionRoot = resolvedRoot,
        GoalContract = goalContract,
        Prevalidation = prevalidation,
        SupervisorState = supervisorState,
    }, GuiSmokeShared.JsonOptions));
    return 0;
}

static int ReplayStep(IReadOnlyDictionary<string, string> options, string workspaceRoot)
{
    if (!options.TryGetValue("--request", out var requestPath))
    {
        throw new InvalidOperationException("--request is required.");
    }

    var resolvedRequestPath = ResolveCliPath(requestPath, workspaceRoot);
    if (!File.Exists(resolvedRequestPath))
    {
        throw new FileNotFoundException("Replay request not found.", resolvedRequestPath);
    }

    var trace = new GuiSmokeReplayTracer("replay-step", options.ContainsKey("--trace"));
    var fullRequestRebuild = options.ContainsKey("--full-request-rebuild") || options.ContainsKey("--rebuild-request");
    trace.Info($"request={resolvedRequestPath}");
    var requestLoad = trace.Measure(
        "load-request",
        () => LoadReplayRequest(resolvedRequestPath, trace, fullRequestRebuild),
        fullRequestRebuild ? "full-request-rebuild" : "lightweight-request",
        alwaysLog: true);
    var request = requestLoad.Request;
    trace.Info($"request-ready mode={(requestLoad.FullRequestRebuild ? "full-request-rebuild" : "lightweight-request")} observerStateLoaded={requestLoad.ObserverStateLoaded} scene={request.SceneSignature}");
    GuiSmokeStepDecision? existingDecision = null;
    if (options.TryGetValue("--decision", out var decisionPath))
    {
        var resolvedDecisionPath = ResolveCliPath(decisionPath, workspaceRoot);
        if (!File.Exists(resolvedDecisionPath))
        {
            throw new FileNotFoundException("Replay decision not found.", resolvedDecisionPath);
        }

        existingDecision = trace.Measure(
            "load-actual-decision",
            () => JsonSerializer.Deserialize<GuiSmokeStepDecision>(File.ReadAllText(resolvedDecisionPath), GuiSmokeShared.JsonOptions),
            Path.GetFileName(resolvedDecisionPath),
            alwaysLog: trace.Entries.Count == 0);
    }

    var replayEvaluation = trace.Measure(
        "analyze",
        () => EvaluateAutoDecisionWithDiagnostics(resolvedRequestPath, request, existingDecision),
        Path.GetFileName(request.ScreenshotPath),
        alwaysLog: true);
    var artifact = replayEvaluation.CandidateDump;
    var serializedArtifact = trace.Measure(
        "serialize-result",
        () => SerializeCandidateDumpArtifact(artifact),
        "stdout",
        alwaysLog: true);
    if (options.TryGetValue("--out", out var outputPath))
    {
        var resolvedOutputPath = ResolveCliPath(outputPath, workspaceRoot);
        trace.Measure(
            "write-candidate-dump",
            () =>
            {
                WriteSerializedCandidateDumpArtifact(resolvedOutputPath, serializedArtifact);
                return 0;
            },
            Path.GetFileName(resolvedOutputPath),
            alwaysLog: true);
    }

    var finalDecisionSemanticAction = string.Equals(request.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                                      && CombatDecisionContract.TryMapSemanticAction(request, artifact.FinalDecision, out var mappedSemanticAction)
        ? mappedSemanticAction
        : null;
    trace.Info($"result target={artifact.FinalDecision.TargetLabel ?? artifact.FinalDecision.ActionKind ?? artifact.FinalDecision.Status} semantic={finalDecisionSemanticAction ?? "null"} foreground={artifact.DebugSummary.ForegroundKind ?? "null"} background={artifact.DebugSummary.BackgroundKind ?? "null"} suppressed={BuildSuppressedCandidateSummary(artifact.DebugSummary)}");
    Console.WriteLine(serializedArtifact);
    return 0;
}

static int ReplayGoldenScenes(IReadOnlyDictionary<string, string> options, string workspaceRoot)
{
    var suitePath = options.TryGetValue("--suite", out var explicitSuitePath)
        ? ResolveCliPath(explicitSuitePath, workspaceRoot)
        : Path.Combine(workspaceRoot, "tests", "replay-fixtures", "gui-smoke-golden-scenes.json");
    if (!File.Exists(suitePath))
    {
        throw new FileNotFoundException("Golden scene suite not found.", suitePath);
    }

    var fixtures = JsonSerializer.Deserialize<IReadOnlyList<GuiSmokeReplayGoldenSceneFixture>>(File.ReadAllText(suitePath), GuiSmokeShared.JsonOptions)
                   ?? throw new InvalidOperationException("Failed to parse golden scene suite.");
    var results = new List<object>(fixtures.Count);
    var traceEnabled = options.ContainsKey("--trace");
    var fullRequestRebuild = options.ContainsKey("--full-request-rebuild") || options.ContainsKey("--rebuild-request");
    for (var index = 0; index < fixtures.Count; index += 1)
    {
        var fixture = fixtures[index];
        var fixtureTrace = new GuiSmokeReplayTracer($"replay-test {index + 1}/{fixtures.Count}:{fixture.Name}", traceEnabled);
        var requestPath = ResolveCliPath(fixture.RequestPath, workspaceRoot);
        fixtureTrace.Info($"start request={requestPath}");
        var requestLoad = fixtureTrace.Measure(
            "load-request",
            () => LoadReplayRequest(requestPath, fixtureTrace, fullRequestRebuild),
            fullRequestRebuild ? "full-request-rebuild" : "lightweight-request",
            alwaysLog: true);
        var request = requestLoad.Request;
        fixtureTrace.Info($"request-ready mode={(requestLoad.FullRequestRebuild ? "full-request-rebuild" : "lightweight-request")} observerStateLoaded={requestLoad.ObserverStateLoaded} scene={request.SceneSignature}");
        var artifact = fixtureTrace.Measure(
            "analyze",
            () => EvaluateAutoDecisionWithDiagnostics(requestPath, request).CandidateDump,
            Path.GetFileName(request.ScreenshotPath),
            alwaysLog: true);

        if (!string.IsNullOrWhiteSpace(fixture.ExpectedTargetContains))
        {
            Assert((artifact.FinalDecision.TargetLabel ?? string.Empty).Contains(fixture.ExpectedTargetContains, StringComparison.OrdinalIgnoreCase),
                $"Golden scene '{fixture.Name}' expected target containing '{fixture.ExpectedTargetContains}' but got '{artifact.FinalDecision.TargetLabel ?? "null"}'.");
        }

        if (!string.IsNullOrWhiteSpace(fixture.ExpectedForegroundKind))
        {
            Assert(string.Equals(artifact.DebugSummary.ForegroundKind, fixture.ExpectedForegroundKind, StringComparison.OrdinalIgnoreCase),
                $"Golden scene '{fixture.Name}' expected foreground '{fixture.ExpectedForegroundKind}' but got '{artifact.DebugSummary.ForegroundKind ?? "null"}'.");
        }

        if (!string.IsNullOrWhiteSpace(fixture.ExpectedBackgroundKind))
        {
            Assert(string.Equals(artifact.DebugSummary.BackgroundKind, fixture.ExpectedBackgroundKind, StringComparison.OrdinalIgnoreCase),
                $"Golden scene '{fixture.Name}' expected background '{fixture.ExpectedBackgroundKind}' but got '{artifact.DebugSummary.BackgroundKind ?? "null"}'.");
        }

        foreach (var requiredLabel in fixture.RequiredCandidateLabels)
        {
            Assert(artifact.Candidates.Any(candidate => string.Equals(candidate.Label, requiredLabel, StringComparison.OrdinalIgnoreCase)),
                $"Golden scene '{fixture.Name}' is missing candidate '{requiredLabel}'.");
        }

        foreach (var suppressedLabel in fixture.RequiredSuppressedLabels)
        {
            Assert(artifact.DebugSummary.SuppressedCandidates.Any(candidate => string.Equals(candidate.Label, suppressedLabel, StringComparison.OrdinalIgnoreCase)),
                $"Golden scene '{fixture.Name}' is missing suppressed candidate '{suppressedLabel}'.");
        }

        foreach (var forbiddenTarget in fixture.ForbiddenTargetLabels)
        {
            Assert(!string.Equals(artifact.FinalDecision.TargetLabel, forbiddenTarget, StringComparison.OrdinalIgnoreCase),
                $"Golden scene '{fixture.Name}' unexpectedly selected forbidden target '{forbiddenTarget}'.");
        }

        results.Add(new
        {
            fixture.Name,
            fixture.RequestPath,
            ReplayMode = requestLoad.FullRequestRebuild ? "full-request-rebuild" : "lightweight-request",
            Checks = DescribeGoldenSceneChecks(fixture),
            SelectedTarget = artifact.FinalDecision.TargetLabel,
            artifact.DebugSummary.ForegroundKind,
            artifact.DebugSummary.BackgroundKind,
            artifact.DebugSummary.WinnerSelectionReason,
            Suppressed = artifact.DebugSummary.SuppressedCandidates
                .Select(candidate => $"{candidate.Label}:{candidate.SuppressionReason}")
                .ToArray(),
            CandidateCount = artifact.Candidates.Count,
            ElapsedMs = fixtureTrace.Entries.Sum(static entry => entry.ElapsedMs),
        });
        fixtureTrace.Info($"ok selected={artifact.FinalDecision.TargetLabel ?? artifact.FinalDecision.ActionKind ?? artifact.FinalDecision.Status} foreground={artifact.DebugSummary.ForegroundKind ?? "null"} background={artifact.DebugSummary.BackgroundKind ?? "null"} suppressed={BuildSuppressedCandidateSummary(artifact.DebugSummary)}");
    }

    Console.WriteLine(JsonSerializer.Serialize(new
    {
        SuitePath = suitePath,
        SceneCount = fixtures.Count,
        Results = results,
    }, GuiSmokeShared.JsonOptions));
    return 0;
}

static GuiSmokeReplayRequestLoadResult LoadReplayRequest(string requestPath, GuiSmokeReplayTracer? trace = null, bool fullRequestRebuild = false)
{
    var request = (trace ?? new GuiSmokeReplayTracer("replay-load", verbose: false)).Measure(
        "request-json",
        () =>
        {
            using var requestStream = new FileStream(requestPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return JsonSerializer.Deserialize<GuiSmokeStepRequest>(requestStream, GuiSmokeShared.JsonOptions)
                   ?? throw new InvalidOperationException($"Failed to parse replay request '{requestPath}'.");
        },
        Path.GetFileName(requestPath),
        alwaysLog: false);
    var stepPrefix = requestPath.EndsWith(".request.json", StringComparison.OrdinalIgnoreCase)
        ? requestPath[..^".request.json".Length]
        : Path.Combine(Path.GetDirectoryName(requestPath) ?? string.Empty, Path.GetFileNameWithoutExtension(requestPath));
    var tracer = trace ?? new GuiSmokeReplayTracer("replay-load", verbose: false);
    var stateDocument = tracer.Measure(
        "observer-state-load",
        () => TryLoadJsonDocument(stepPrefix + ".observer.state.json"),
        Path.GetFileName(stepPrefix + ".observer.state.json"),
        alwaysLog: false);
    tracer.Skipped("observer-inventory-load", "replay-uses-request-embedded-action-nodes");
    tracer.Skipped("observer-events-load", "replay-uses-request-embedded-events-tail");
    var observer = new ObserverState(
        request.Observer with
        {
            LastEventsTail = request.Observer.LastEventsTail ?? Array.Empty<string>(),
            ActionNodes = request.Observer.ActionNodes ?? Array.Empty<ObserverActionNode>(),
            Choices = request.Observer.Choices ?? Array.Empty<ObserverChoice>(),
            CombatHand = request.Observer.CombatHand ?? Array.Empty<ObservedCombatHandCard>(),
        },
        stateDocument,
        null,
        request.Observer.LastEventsTail?.ToArray() ?? Array.Empty<string>());
    var phase = Enum.Parse<GuiSmokePhase>(request.Phase, ignoreCase: true);
    var sceneSignature = tracer.Measure(
        "scene-signature",
        () => ComputeSceneSignature(request.ScreenshotPath, observer, phase),
        Path.GetFileName(request.ScreenshotPath),
        alwaysLog: false);
    string[] allowedActions;
    string failureModeHint;
    string? semanticGoal;
    if (fullRequestRebuild)
    {
        allowedActions = tracer.Measure(
            "allowed-actions",
            () => BuildAllowedActions(phase, observer, request.CombatCardKnowledge, request.ScreenshotPath, request.History),
            request.Phase,
            alwaysLog: false);
        failureModeHint = tracer.Measure(
            "failure-mode-hint",
            () => BuildFailureModeHintCore(phase, observer, request.CombatCardKnowledge, request.ScreenshotPath, request.History),
            request.Phase,
            alwaysLog: false);
        semanticGoal = tracer.Measure(
            "semantic-goal",
            () => BuildSemanticGoal(phase, observer, request.ReasoningMode),
            request.ReasoningMode,
            alwaysLog: false);
    }
    else
    {
        allowedActions = request.AllowedActions;
        failureModeHint = request.FailureModeHint;
        semanticGoal = request.SemanticGoal;
        tracer.Skipped("allowed-actions", "reuse-request-artifact");
        tracer.Skipped("failure-mode-hint", "reuse-request-artifact");
        tracer.Skipped("semantic-goal", "reuse-request-artifact");
    }

    return new GuiSmokeReplayRequestLoadResult(
        request with
        {
            KnownRecipes = request.KnownRecipes ?? Array.Empty<KnownRecipeHint>(),
            EventKnowledgeCandidates = request.EventKnowledgeCandidates ?? Array.Empty<EventKnowledgeCandidate>(),
            CombatCardKnowledge = request.CombatCardKnowledge ?? Array.Empty<CombatCardKnowledgeHint>(),
            History = request.History ?? Array.Empty<GuiSmokeHistoryEntry>(),
            Observer = observer.Summary,
            SceneSignature = sceneSignature,
            AllowedActions = allowedActions,
            FailureModeHint = failureModeHint,
            SemanticGoal = semanticGoal,
        },
        fullRequestRebuild,
        stateDocument is not null,
        tracer.Entries.ToArray());
}

static JsonDocument? TryLoadJsonDocument(string path)
{
    if (!File.Exists(path))
    {
        return null;
    }

    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
    return JsonDocument.Parse(stream);
}

static GuiSmokeReplayEvaluation EvaluateAutoDecisionWithDiagnostics(
    string requestPath,
    GuiSmokeStepRequest request,
    GuiSmokeStepDecision? actualDecision = null)
{
    var analysis = AutoDecisionProvider.Analyze(request, actualDecision);
    return new GuiSmokeReplayEvaluation(
        requestPath,
        request,
        analysis.FinalDecision,
        analysis.ToArtifact());
}

static string SerializeCandidateDumpArtifact(GuiSmokeCandidateDumpArtifact artifact)
{
    return JsonSerializer.Serialize(artifact, GuiSmokeShared.JsonOptions);
}

static void WriteCandidateDumpArtifact(string path, GuiSmokeCandidateDumpArtifact artifact)
{
    WriteSerializedCandidateDumpArtifact(path, SerializeCandidateDumpArtifact(artifact));
}

static void WriteSerializedCandidateDumpArtifact(string path, string serializedArtifact)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory());
    var bytes = Encoding.UTF8.GetBytes(serializedArtifact);
    using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
    stream.Write(bytes, 0, bytes.Length);
    stream.Flush(true);
}

static string BuildSuppressedCandidateSummary(GuiSmokeDecisionDebugSummary debugSummary)
{
    if (debugSummary.SuppressedCandidates.Count == 0)
    {
        return "none";
    }

    return string.Join(
        "; ",
        debugSummary.SuppressedCandidates.Select(candidate => $"{candidate.Label}:{candidate.SuppressionReason}"));
}

static string DescribeGoldenSceneChecks(GuiSmokeReplayGoldenSceneFixture fixture)
{
    var checks = new List<string>();
    if (!string.IsNullOrWhiteSpace(fixture.ExpectedTargetContains))
    {
        checks.Add($"target~{fixture.ExpectedTargetContains}");
    }

    if (!string.IsNullOrWhiteSpace(fixture.ExpectedForegroundKind))
    {
        checks.Add($"foreground={fixture.ExpectedForegroundKind}");
    }

    if (!string.IsNullOrWhiteSpace(fixture.ExpectedBackgroundKind))
    {
        checks.Add($"background={fixture.ExpectedBackgroundKind}");
    }

    if (fixture.RequiredCandidateLabels.Count > 0)
    {
        checks.Add($"requires:{string.Join(", ", fixture.RequiredCandidateLabels)}");
    }

    if (fixture.RequiredSuppressedLabels.Count > 0)
    {
        checks.Add($"suppresses:{string.Join(", ", fixture.RequiredSuppressedLabels)}");
    }

    if (fixture.ForbiddenTargetLabels.Count > 0)
    {
        checks.Add($"forbids:{string.Join(", ", fixture.ForbiddenTargetLabels)}");
    }

    return checks.Count == 0
        ? "none"
        : string.Join(" | ", checks);
}

static async Task WaitForLiveGameWindowAsync(DateTimeOffset launchedAt, TimeSpan timeout)
{
    var deadline = DateTimeOffset.UtcNow.Add(timeout);
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
        await Task.Delay(1000).ConfigureAwait(false);
    }

    throw new TimeoutException("Timed out waiting for a live STS2 game window.");
}

static async Task MaintainLaunchFocusAsync(TimeSpan duration, TimeSpan requiredStableWindow)
{
    var deadline = DateTimeOffset.UtcNow.Add(duration);
    DateTimeOffset? stableSince = null;
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

        await Task.Delay(1000).ConfigureAwait(false);
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

static List<T> ReadNdjsonFixture<T>(string path)
{
    var entries = new List<T>();
    if (!File.Exists(path))
    {
        return entries;
    }

    foreach (var line in File.ReadLines(path))
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            continue;
        }

        var entry = JsonSerializer.Deserialize<T>(line, GuiSmokeShared.JsonOptions);
        if (entry is not null)
        {
            entries.Add(entry);
        }
    }

    return entries;
}

static T? TryReadJson<T>(string path)
{
    if (!File.Exists(path))
    {
        return default;
    }

    try
    {
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), GuiSmokeShared.JsonOptions);
    }
    catch
    {
        return default;
    }
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
    return observer.IsFreshSince(freshnessFloor) && !IsUnknownObserverScreen(GetObservedScreen(observer));
}

static string? GetObservedScreen(ObserverState observer)
{
    return IsUnknownObserverScreen(observer.CurrentScreen)
        ? observer.VisibleScreen
        : observer.CurrentScreen;
}

static bool IsUnknownObserverScreen(string? screen)
{
    return string.IsNullOrWhiteSpace(screen)
           || string.Equals(screen, "unknown", StringComparison.OrdinalIgnoreCase);
}

static void RunSelfTest()
{
    var point = MouseInputDriver.TransformNormalizedPoint(
        new WindowCaptureTarget(IntPtr.Zero, "test", new Rectangle(100, 200, 1000, 800), false, false),
        0.5,
        0.25);
    Assert(point.X == 600 && point.Y == 400, "Normalized transform should map into client bounds.");

    var request = new GuiSmokeStepRequest(
        "run",
        "boot-to-combat",
        3,
        GuiSmokePhase.EnterRun.ToString(),
        "enter-run",
        DateTimeOffset.UtcNow,
        "screen.png",
        new WindowBounds(100, 200, 1000, 800),
        "scene:test",
        "0001",
        1,
        3,
        false,
        "tactical",
        null,
        new ObserverSummary("main-menu", "main-menu", false, DateTimeOffset.UtcNow, null, null, null, null, null, null, null, null, null, null, new[] { "Continue" }, new[] { "main-menu" }, Array.Empty<ObserverActionNode>(), Array.Empty<ObserverChoice>(), Array.Empty<ObservedCombatHandCard>()),
        Array.Empty<KnownRecipeHint>(),
        Array.Empty<EventKnowledgeCandidate>(),
        Array.Empty<CombatCardKnowledgeHint>(),
        new[] { "click continue", "click singleplayer" },
        Array.Empty<GuiSmokeHistoryEntry>(),
        "menu entry",
        null);
    var json = JsonSerializer.Serialize(request, GuiSmokeShared.JsonOptions);
    var roundTrip = JsonSerializer.Deserialize<GuiSmokeStepRequest>(json, GuiSmokeShared.JsonOptions);
    Assert(roundTrip?.Phase == GuiSmokePhase.EnterRun.ToString(), "Request should round-trip.");

    var decision = new GuiSmokeStepDecision("act", "click", null, 0.3, 0.7, "continue", "main menu continue", 0.9, "character-select", 1000, true, null);
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

    var videoSkipRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-video-self-test-{Guid.NewGuid():N}");
    try
    {
        Directory.CreateDirectory(videoSkipRoot);
        var missingFfmpegPath = Path.Combine(videoSkipRoot, "missing-ffmpeg.exe");
        var workspaceRoot = Directory.GetCurrentDirectory();
        var cmdWslPath = "/mnt/c/Windows/System32/cmd.exe";
        var cmdWindowsPath = @"C:\Windows\System32\cmd.exe";
        var cmdBindingFromHostPath = GuiSmokeVideoRecorder.ResolveExecutablePathForSelfTest(cmdWslPath, workspaceRoot)
                                   ?? throw new InvalidOperationException("Expected host ffmpeg-style path binding.");
        Assert(File.Exists(cmdBindingFromHostPath.HostPath), "Host binding should resolve to an executable path visible to the current runtime.");
        Assert(string.Equals(cmdBindingFromHostPath.ProcessPath, cmdWindowsPath, StringComparison.OrdinalIgnoreCase), "Host binding should produce Windows process path for child execution.");

        var cmdBindingFromWindowsPath = GuiSmokeVideoRecorder.ResolveExecutablePathForSelfTest(cmdWindowsPath, workspaceRoot)
                                      ?? throw new InvalidOperationException("Expected Windows ffmpeg-style path binding.");
        Assert(File.Exists(cmdBindingFromWindowsPath.HostPath), "Windows binding should resolve to an executable path visible to the current runtime.");
        Assert(string.Equals(cmdBindingFromWindowsPath.ProcessPath, cmdWindowsPath, StringComparison.OrdinalIgnoreCase), "Windows binding should preserve Windows process path.");

        using var recorder = GuiSmokeVideoRecorder.Create(
            workspaceRoot,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["--ffmpeg-path"] = missingFfmpegPath,
            },
            "self-test-session",
            "self-test-run",
            videoSkipRoot,
            videoSkipRoot,
            attemptId: "0001",
            scopeKind: "attempt");
        var started = recorder.TryStart(new WindowCaptureTarget(IntPtr.Zero, "self-test-window", new Rectangle(100, 200, 401, 301), false, false));
        Assert(!started, "Video recorder should skip when ffmpeg is unavailable.");
        recorder.Complete(keepRecording: true, completionReason: "self-test-video-skip");

        var metadata = TryReadJson<GuiSmokeVideoRecordingMetadata>(Path.Combine(videoSkipRoot, "video-recording.json"))
                       ?? throw new InvalidOperationException("Expected video metadata after ffmpeg skip self-test.");
        Assert(string.Equals(metadata.Status, "skipped", StringComparison.OrdinalIgnoreCase), "Missing ffmpeg should produce skipped metadata.");
        Assert(metadata.SkipReason is not null && metadata.SkipReason.Contains("ffmpeg-not-found", StringComparison.OrdinalIgnoreCase), "Video skip metadata should explain missing ffmpeg.");
        Assert(!File.Exists(metadata.OutputPath), "Skipped video capture should not leave a recording file behind.");
    }
    finally
    {
        if (Directory.Exists(videoSkipRoot))
        {
            Directory.Delete(videoSkipRoot, recursive: true);
        }
    }

    ValidateDecision(
        GuiSmokePhase.EnterRun,
        request,
        decision);

    var specialKeyDecision = new GuiSmokeStepDecision("act", "press-key", "Escape", null, null, "cancel selection", "cancel with escape", 0.8, "combat", 500, false, null);
    ValidateDecision(
        GuiSmokePhase.HandleCombat,
        request with { AllowedActions = new[] { "click card", "click enemy", "click end turn", "wait" } },
        specialKeyDecision);

    var autoRewardRequest = request with
    {
        Phase = GuiSmokePhase.HandleRewards.ToString(),
        Observer = new ObserverSummary(
            "rewards",
            "rewards",
            false,
            DateTimeOffset.UtcNow,
            "inv",
            true,
            "mixed",
            "stable",
            "episode-1",
            null,
            null,
            30,
            80,
            null,
            new[] { "skip" },
            Array.Empty<string>(),
            new[]
            {
                new ObserverActionNode("reward:0", "reward-item", "Reward Card", "100,200,120,120", true),
                new ObserverActionNode("reward:1", "button", "Proceed", "900,700,240,90", true),
            },
            Array.Empty<ObserverChoice>(),
            Array.Empty<ObservedCombatHandCard>()),
        AllowedActions = new[] { "click proceed", "click reward", "wait" },
    };
    var autoDecision = AutoDecisionProvider.Decide(autoRewardRequest);
    Assert(autoDecision.ActionKind == "click", "Auto provider should choose a click for reward handling.");
    Assert(autoDecision.TargetLabel == "claim reward item", "Auto provider should prefer the reward item before proceed.");

    var treasureRequest = request with
    {
        Phase = GuiSmokePhase.ChooseFirstNode.ToString(),
        Observer = new ObserverSummary(
            "map",
            "map",
            false,
            DateTimeOffset.UtcNow,
            "inv",
            true,
            "mixed",
            "stable",
            "episode-2",
            "Treasure",
            "generic",
            57,
            80,
            null,
            new[] { "Chest", "Proceed" },
            Array.Empty<string>(),
            Array.Empty<ObserverActionNode>(),
            Array.Empty<ObserverChoice>(),
            Array.Empty<ObservedCombatHandCard>()),
        AllowedActions = new[] { "click treasure chest", "click treasure reward icon", "click proceed", "wait" },
    };
    var treasureDecision = AutoDecisionProvider.Decide(treasureRequest);
    Assert(treasureDecision.ActionKind == "click", "Treasure handling should click.");
    Assert(treasureDecision.TargetLabel == "treasure chest center", "Treasure handling should center-click the chest first.");
    Assert(
        IsRoomProgressTarget("treasure reward icon")
        && GetSameActionStallLimit(
            GuiSmokePhase.ChooseFirstNode,
            new GuiSmokeStepDecision("act", "click", null, 0.5, 0.4, "treasure reward icon", "reward icon", 0.9, "map", 1200, true, null)) == 4,
        "Treasure reward icon should be treated as room progress with the same stall budget as the chest.");

    var combatScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-combat-self-test-{Guid.NewGuid():N}.png");
    try
    {
        using (var bitmap = new Bitmap(1280, 720))
        using (var graphics = Graphics.FromImage(bitmap))
        using (var attackBrush = new SolidBrush(Color.FromArgb(220, 80, 50)))
        {
            graphics.Clear(Color.Black);
            graphics.FillRectangle(attackBrush, new Rectangle(230, 550, 140, 150));
            bitmap.Save(combatScreenshotPath, ImageFormat.Png);
        }

        var combatStartDecision = AutoDecisionProvider.Decide(request with
        {
            Phase = GuiSmokePhase.HandleCombat.ToString(),
            Observer = new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv",
                true,
                "hook",
                "stable",
                "episode-3",
                "Elite",
                "combat",
                57,
                80,
                null,
                new[] { "1턴 종료" },
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                Array.Empty<ObserverChoice>(),
                Array.Empty<ObservedCombatHandCard>()),
            History = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.WaitCombat.ToString(), "observer-accepted", null, DateTimeOffset.UtcNow),
            },
            ScreenshotPath = combatScreenshotPath,
            AllowedActions = new[] { "click card", "click enemy", "click end turn", "wait" },
        });
        Assert(combatStartDecision.ActionKind == "press-key", "Combat opener should start by selecting a visible card with a hotkey.");
        Assert(combatStartDecision.TargetLabel == "combat select attack slot 1", "Combat opener should not skip directly to targeting before selecting a visible attack card.");
    }
    finally
    {
        if (File.Exists(combatScreenshotPath))
        {
            File.Delete(combatScreenshotPath);
        }
    }

    var evaluator = new ObserverAcceptanceEvaluator();
    Assert(
        evaluator.IsPhaseSatisfied(
            GuiSmokePhase.WaitCombat,
            new ObserverState(new ObserverSummary("combat", "combat", true, DateTimeOffset.UtcNow, null, true, "hook", "stable", null, null, null, null, null, null, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ObserverActionNode>(), Array.Empty<ObserverChoice>(), Array.Empty<ObservedCombatHandCard>()), null, null, null)),
        "Combat acceptance should require combat screen and inCombat=true.");

    Assert(
        WindowLocator.HasMeaningfulDrift(
            new WindowCaptureTarget(IntPtr.Zero, "before", new Rectangle(100, 200, 1000, 800), false, false),
            new WindowCaptureTarget(IntPtr.Zero, "after", new Rectangle(140, 200, 1000, 800), false, false)),
        "Bounds drift should be detected when the window moved.");

    Assert(
        TryParseScreenBounds("100,200,40,20", out var parsedBounds)
        && Math.Abs(parsedBounds.X - 100f) < 0.01f
        && Math.Abs(parsedBounds.Width - 40f) < 0.01f,
        "Screen bounds should parse from observer inventory strings.");

    var embarkEventObserver = new ObserverState(
        new ObserverSummary(
            "event",
            "event",
            false,
            DateTimeOffset.UtcNow,
            "inv-event",
            true,
            "mixed",
            "stable",
            "episode-embark",
            "None",
            "event",
            80,
            80,
            null,
            new[] { "Option A" },
            Array.Empty<string>(),
            new[] { new ObserverActionNode("event-option:0", "event-option", "Option A", "460,750,1000,100", true) },
            new[] { new ObserverChoice("choice", "Option A", "460,750,1000,100") },
            Array.Empty<ObservedCombatHandCard>()),
        null,
        null,
        null);
    Assert(GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(embarkEventObserver, out var postEmbarkPhase) && postEmbarkPhase == GuiSmokePhase.HandleEvent, "Embark should reconcile to HandleEvent when observer already reports an event room.");
    Assert(GetAllowedActions(GuiSmokePhase.Embark, embarkEventObserver).Contains("click event choice", StringComparer.OrdinalIgnoreCase), "Embark allowlist should admit event progression actions when observer is already in an event room.");
    var embarkDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
        "run",
        "boot-to-long-run",
        7,
        GuiSmokePhase.Embark.ToString(),
        "Click Embark to begin the run.",
        DateTimeOffset.UtcNow,
        "screen.png",
        new WindowBounds(0, 0, 1280, 720),
        "phase:embark|screen:event|visible:event|encounter:none|ready:true|stability:stable|room:treasure",
        "0001",
        1,
        3,
        true,
        "tactical",
        null,
        embarkEventObserver.Summary,
        Array.Empty<KnownRecipeHint>(),
        Array.Empty<EventKnowledgeCandidate>(),
        Array.Empty<CombatCardKnowledgeHint>(),
        GetAllowedActions(GuiSmokePhase.Embark, embarkEventObserver),
        Array.Empty<GuiSmokeHistoryEntry>(),
        "phase reconciliation required",
        null));
    Assert(string.Equals(embarkDecision.TargetLabel, "event progression choice", StringComparison.OrdinalIgnoreCase), "Embark decisioning should switch to the event progression choice instead of waiting for an embark button.");
    Assert(TryClassifyDecisionWaitPlateau(GuiSmokePhase.Embark, embarkEventObserver, 2, out var embarkPlateauCause, out _) && string.Equals(embarkPlateauCause, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase), "Embark/event repeated waits should escalate to phase-mismatch-stall.");
    Assert(TryClassifyDecisionWaitPlateau(GuiSmokePhase.HandleEvent, embarkEventObserver, 5, out var waitPlateauCause, out _) && string.Equals(waitPlateauCause, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase), "Repeated waits in a stable room scene should escalate to decision-wait-plateau.");

    var mapTransitionScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-map-transition-self-test-{Guid.NewGuid():N}.png");
    try
    {
        using (var bitmap = new Bitmap(1280, 720))
        using (var graphics = Graphics.FromImage(bitmap))
        using (var backgroundBrush = new SolidBrush(Color.FromArgb(194, 168, 125)))
        using (var arrowBrush = new SolidBrush(Color.FromArgb(220, 60, 55)))
        {
            graphics.Clear(Color.Black);
            graphics.FillRectangle(backgroundBrush, new Rectangle(200, 40, 880, 620));
            graphics.FillEllipse(arrowBrush, new Rectangle(590, 455, 100, 80));
            bitmap.Save(mapTransitionScreenshotPath, ImageFormat.Png);
        }

        using var mapTransitionStateDocument = JsonDocument.Parse("""{"meta":{"declaringType":"MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen","instanceType":"MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen"}}""");
        var mapTransitionObserver = new ObserverState(
            new ObserverSummary(
                "event",
                "event",
                false,
                DateTimeOffset.UtcNow,
                "inv-map-transition",
                true,
                "mixed",
                "stable",
                "episode-map-transition",
                "None",
                "event",
                80,
                80,
                null,
                new[] { "진행" },
                new[] { "screen-changed: map", "map-point-selected" },
                new[] { new ObserverActionNode("event-option:0", "event-option", "진행", "460,942,1000,100", true) },
                new[] { new ObserverChoice("choice", "진행", "460,942,1000,100", "진행") },
                Array.Empty<ObservedCombatHandCard>()),
            mapTransitionStateDocument,
            null,
            null);
        Assert(GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(mapTransitionObserver, out var postMapPhase) && postMapPhase == GuiSmokePhase.ChooseFirstNode, "Map-screen authority should win over stale event labels when observer meta already points at NMapScreen.");
        Assert(GetAllowedActions(GuiSmokePhase.HandleEvent, mapTransitionObserver).Contains("click visible map advance", StringComparer.OrdinalIgnoreCase), "Event allowlist should open map affordances when map transition evidence is stronger than the stale event screen.");
    }
    finally
    {
        if (File.Exists(mapTransitionScreenshotPath))
        {
            File.Delete(mapTransitionScreenshotPath);
        }
    }

    var eventContaminationScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-event-contamination-self-test-{Guid.NewGuid():N}.png");
    try
    {
        using (var bitmap = new Bitmap(1280, 720))
        using (var graphics = Graphics.FromImage(bitmap))
        using (var panelBrush = new SolidBrush(Color.FromArgb(90, 64, 42)))
        using (var choiceBrush = new SolidBrush(Color.FromArgb(215, 188, 124)))
        using (var arrowBrush = new SolidBrush(Color.FromArgb(220, 60, 55)))
        {
            graphics.Clear(Color.Black);
            graphics.FillRectangle(panelBrush, new Rectangle(720, 150, 470, 420));
            graphics.FillRectangle(choiceBrush, new Rectangle(880, 430, 280, 66));
            graphics.FillRectangle(choiceBrush, new Rectangle(880, 520, 280, 66));
            graphics.FillEllipse(arrowBrush, new Rectangle(590, 452, 94, 74));
            bitmap.Save(eventContaminationScreenshotPath, ImageFormat.Png);
        }

        using var eventStateDocument = JsonDocument.Parse("""{"meta":{"declaringType":"MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom","instanceType":"MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom"}}""");
        var eventObserverState = new ObserverState(
            new ObserverSummary(
                "event",
                "event",
                false,
                DateTimeOffset.UtcNow,
                "inv-event-foreground",
                true,
                "mixed",
                "stable",
                "episode-event-foreground",
                "None",
                "event",
                80,
                80,
                null,
                new[] { "그래도 휴식한다", "나무들을 베어낸다" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("event-option:0", "event-option", "그래도 휴식한다", "922,596,800,100", true),
                    new ObserverActionNode("event-option:1", "event-option", "나무들을 베어낸다", "922,700,800,100", true),
                },
                new[]
                {
                    new ObserverChoice("choice", "그래도 휴식한다", "922,596,800,100", "그래도 휴식한다"),
                    new ObserverChoice("choice", "나무들을 베어낸다", "922,700,800,100", "나무들을 베어낸다"),
                },
                Array.Empty<ObservedCombatHandCard>()),
            eventStateDocument,
            null,
            null);
        var eventSceneSignature = ComputeSceneSignature(eventContaminationScreenshotPath, eventObserverState, GuiSmokePhase.HandleEvent);
        Assert(eventSceneSignature.Contains("layer:event-foreground", StringComparison.OrdinalIgnoreCase), "Explicit event choices should mark the event as foreground-authoritative.");
        Assert(eventSceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase), "Background map arrows on event scenes should be tagged as contamination.");
        Assert(!eventSceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase), "Event foreground authority should suppress visible map advance tags.");
        Assert(!GetAllowedActions(GuiSmokePhase.HandleEvent, eventObserverState).Contains("click visible map advance", StringComparer.OrdinalIgnoreCase), "HandleEvent allowlist should not expose map fallback when explicit event options are visible.");
        var eventDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            43,
            GuiSmokePhase.HandleEvent.ToString(),
            "Resolve the event choice.",
            DateTimeOffset.UtcNow,
            eventContaminationScreenshotPath,
            new WindowBounds(0, 0, 1280, 720),
            eventSceneSignature,
            "0001",
            1,
            3,
            true,
            "semantic",
            null,
            eventObserverState.Summary,
            Array.Empty<KnownRecipeHint>(),
            new[]
            {
                new EventKnowledgeCandidate(
                    "uneasy-rest-site",
                    "불안한 휴식 장소",
                    "self-test event foreground",
                    new[]
                    {
                        new EventOptionKnowledgeCandidate("그래도 휴식한다", "휴식을 시도한다.", "rest"),
                        new EventOptionKnowledgeCandidate("나무들을 베어낸다", "나무를 베어 길을 연다.", "chop"),
                    }),
            },
            Array.Empty<CombatCardKnowledgeHint>(),
            GetAllowedActions(GuiSmokePhase.HandleEvent, eventObserverState),
            Array.Empty<GuiSmokeHistoryEntry>(),
            "Explicit event options must outrank background map contamination.",
            null));
        Assert(eventDecision.TargetLabel is not null && eventDecision.TargetLabel.Contains("event", StringComparison.OrdinalIgnoreCase), "Event decisioning should click an explicit event option instead of a map fallback when NEventRoom authority is present.");
    }
    finally
    {
        if (File.Exists(eventContaminationScreenshotPath))
        {
            File.Delete(eventContaminationScreenshotPath);
        }
    }

    using (var choiceMetadataDocument = JsonDocument.Parse(
               """
               {
                 "currentChoices": [
                   {
                     "kind": "rest-option",
                     "label": "Smith",
                     "value": "SMITH",
                     "description": "Upgrade a card.",
                     "nodeId": "rest-site:SMITH",
                     "screenBounds": "820,260,200,110",
                     "bindingKind": "rest-site-option",
                     "bindingId": "SMITH",
                     "enabled": true,
                     "iconAssetPath": "res://smith.png",
                     "semanticHints": [
                       "scene:rest-site",
                       "option-id:SMITH",
                       "source:button"
                     ]
                   }
                 ]
               }
               """))
    {
        var parsedMetadataChoices = ObserverSnapshotReader.ParseChoicesForTesting(choiceMetadataDocument);
        Assert(parsedMetadataChoices.Count == 1
               && string.Equals(parsedMetadataChoices[0].NodeId, "rest-site:SMITH", StringComparison.OrdinalIgnoreCase)
               && string.Equals(parsedMetadataChoices[0].BindingKind, "rest-site-option", StringComparison.OrdinalIgnoreCase)
               && string.Equals(parsedMetadataChoices[0].BindingId, "SMITH", StringComparison.OrdinalIgnoreCase)
               && parsedMetadataChoices[0].Enabled == true
               && string.Equals(parsedMetadataChoices[0].IconAssetPath, "res://smith.png", StringComparison.OrdinalIgnoreCase)
               && parsedMetadataChoices[0].SemanticHints.Contains("option-id:SMITH", StringComparer.OrdinalIgnoreCase),
            "Observer choice reader should preserve rest-site binding metadata and semantic hints.");
    }

    using (var legacyChoiceDocument = JsonDocument.Parse(
               """
               {
                 "currentChoices": [
                   {
                     "kind": "choice",
                     "label": "Smith",
                     "value": "smith",
                     "screenBounds": "820,260,200,110"
                   }
                 ]
               }
               """))
    {
        var parsedLegacyChoices = ObserverSnapshotReader.ParseChoicesForTesting(legacyChoiceDocument);
        Assert(parsedLegacyChoices.Count == 1
               && parsedLegacyChoices[0].BindingId is null
               && string.Equals(parsedLegacyChoices[0].Label, "Smith", StringComparison.OrdinalIgnoreCase),
            "Observer choice reader should keep old artifacts readable when choice metadata fields are absent.");
    }

    using (var restSiteMetaDocument = JsonDocument.Parse(
               """
               {
                 "meta": {
                   "restSiteSelectionLastSignal": "after-select-failure",
                   "restSiteSelectionLastOptionId": "SMITH",
                   "restSiteSelectionCurrentStatus": "explicit-choice",
                   "restSiteUpgradeScreenVisible": "false"
                 }
               }
               """))
    {
        var parsedMeta = ObserverSnapshotReader.ParseMetaForTesting(restSiteMetaDocument);
        Assert(string.Equals(parsedMeta["restSiteSelectionLastSignal"], "after-select-failure", StringComparison.OrdinalIgnoreCase)
               && string.Equals(parsedMeta["restSiteSelectionLastOptionId"], "SMITH", StringComparison.OrdinalIgnoreCase)
               && string.Equals(parsedMeta["restSiteSelectionCurrentStatus"], "explicit-choice", StringComparison.OrdinalIgnoreCase),
            "Observer snapshot reader should preserve rest-site transition metadata from snapshot meta.");
    }

    var restSiteMetadataScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-rest-site-metadata-self-test-{Guid.NewGuid():N}.png");
    try
    {
        using (var bitmap = new Bitmap(1280, 720))
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.Black);
            bitmap.Save(restSiteMetadataScreenshotPath, ImageFormat.Png);
        }

        var restSiteMetadataSummary = new ObserverSummary(
            "map",
            "map",
            false,
            DateTimeOffset.UtcNow,
            "inv-rest-site",
            true,
            "mixed",
            "stable",
            "episode-rest-site",
            "RestSite",
            "rest",
            70,
            80,
            null,
            new[] { "Rest", "Smith", "Hatch", "Mend" },
            Array.Empty<string>(),
            Array.Empty<ObserverActionNode>(),
            new[]
            {
                new ObserverChoice("rest-option", "Rest", "520,260,200,110", "HEAL", "Recover HP")
                {
                    NodeId = "rest-site:HEAL",
                    BindingKind = "rest-site-option",
                    BindingId = "HEAL",
                    Enabled = true,
                    SemanticHints = new[] { "scene:rest-site", "option-id:HEAL", "source:button" },
                },
                new ObserverChoice("rest-option", "Smith", "820,260,200,110", "SMITH", "Upgrade a card")
                {
                    NodeId = "rest-site:SMITH",
                    BindingKind = "rest-site-option",
                    BindingId = "SMITH",
                    Enabled = true,
                    IconAssetPath = "res://smith.png",
                    SemanticHints = new[] { "scene:rest-site", "option-id:SMITH", "source:button" },
                },
                new ObserverChoice("rest-option", "Hatch", "1120,260,200,110", "HATCH", "Hatch a card")
                {
                    NodeId = "rest-site:HATCH",
                    BindingKind = "rest-site-option",
                    BindingId = "HATCH",
                    Enabled = true,
                    SemanticHints = new[] { "scene:rest-site", "option-id:HATCH", "source:button" },
                },
                new ObserverChoice("rest-option", "Mend", "1420,260,200,110", "MEND", "Special rest-site option")
                {
                    NodeId = "rest-site:MEND",
                    BindingKind = "rest-site-option",
                    BindingId = "MEND",
                    Enabled = true,
                    SemanticHints = new[] { "scene:rest-site", "option-id:MEND", "source:button" },
                },
            },
            Array.Empty<ObservedCombatHandCard>());
        var restSiteMetadataObserver = new ObserverState(restSiteMetadataSummary, null, null, null);
        var restSiteMetadataRequest = new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            10,
            GuiSmokePhase.ChooseFirstNode.ToString(),
            "Choose the authoritative rest-site option.",
            DateTimeOffset.UtcNow,
            restSiteMetadataScreenshotPath,
            new WindowBounds(0, 0, 1280, 720),
            "phase:choosefirstnode|screen:map|visible:map|encounter:restsite|ready:true|stability:stable|room:rest-site|layer:map-background|layer:map-overlay-foreground",
            "0001",
            1,
            3,
            true,
            "semantic",
            null,
            restSiteMetadataSummary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            GetAllowedActions(GuiSmokePhase.ChooseFirstNode, restSiteMetadataObserver),
            Array.Empty<GuiSmokeHistoryEntry>(),
            "Authoritative rest-site buttons must supply metadata and hitboxes.",
            null);
        var restSiteMetadataAnalysis = AutoDecisionProvider.Analyze(restSiteMetadataRequest);
        Assert(string.Equals(restSiteMetadataAnalysis.FinalDecision.TargetLabel, "rest site: smith", StringComparison.OrdinalIgnoreCase),
            "Metadata-first rest-site decisioning should prefer smith when HP is healthy.");
        var selectedRestSiteCandidate = restSiteMetadataAnalysis.Candidates.Single(candidate => candidate.Selected);
        Assert(!string.IsNullOrWhiteSpace(selectedRestSiteCandidate.RawBounds)
               && !string.IsNullOrWhiteSpace(selectedRestSiteCandidate.BoundsSource)
               && !string.Equals(selectedRestSiteCandidate.BoundsSource, "provider-external", StringComparison.OrdinalIgnoreCase),
            "Selected authoritative rest-site candidate should retain raw bounds and a non-external bounds source.");
        Assert(restSiteMetadataAnalysis.DebugSummary.SuppressedCandidates.Any(candidate => string.Equals(candidate.Label, "click visible map advance", StringComparison.OrdinalIgnoreCase))
               && restSiteMetadataAnalysis.DebugSummary.SuppressedCandidates.Any(candidate => string.Equals(candidate.Label, "click exported reachable node", StringComparison.OrdinalIgnoreCase))
               && restSiteMetadataAnalysis.DebugSummary.SuppressedCandidates.Any(candidate => string.Equals(candidate.Label, "click first reachable node", StringComparison.OrdinalIgnoreCase))
               && restSiteMetadataAnalysis.DebugSummary.SuppressedCandidates.Any(candidate => string.Equals(candidate.Label, "click map back", StringComparison.OrdinalIgnoreCase)),
            "Authoritative rest-site choices should keep map-routing fallbacks suppressed.");
        Assert(restSiteMetadataAnalysis.Candidates.Any(candidate =>
                string.Equals(candidate.Label, "click option:MEND", StringComparison.OrdinalIgnoreCase)
                && string.Equals(candidate.RejectReason, "not-default-auto-pick", StringComparison.OrdinalIgnoreCase)),
            "Additional rest-site options should be visible in candidates but excluded from default auto-pick.");

        var missingHitboxSummary = restSiteMetadataSummary with
        {
            Choices = new[]
            {
                new ObserverChoice("rest-option", "Smith", null, "SMITH", "Upgrade a card")
                {
                    NodeId = "rest-site:SMITH",
                    BindingKind = "rest-site-option",
                    BindingId = "SMITH",
                    Enabled = true,
                    SemanticHints = new[] { "scene:rest-site", "option-id:SMITH", "source:button" },
                },
            },
            CurrentChoices = new[] { "Smith" },
        };
        var missingHitboxRequest = restSiteMetadataRequest with
        {
            Observer = missingHitboxSummary,
            AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, new ObserverState(missingHitboxSummary, null, null, null)),
        };
        var missingHitboxAnalysis = AutoDecisionProvider.Analyze(missingHitboxRequest);
        Assert(string.Equals(missingHitboxAnalysis.FinalDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
               && missingHitboxAnalysis.Candidates.Any(candidate =>
                   string.Equals(candidate.Label, "click rest site choice", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(candidate.RejectReason, "missing-hitbox-for-explicit-choice", StringComparison.OrdinalIgnoreCase)),
            "Authoritative rest-site options without hitboxes should wait and record missing-hitbox-for-explicit-choice instead of using fixed coordinates.");

        var fingerprint = RestSiteChoiceSupport.BuildExplicitChoiceFingerprint(restSiteMetadataSummary);
        var firstSmithClick = new GuiSmokeHistoryEntry(
            GuiSmokePhase.ChooseFirstNode.ToString(),
            "click",
            "rest site: smith",
            DateTimeOffset.UtcNow.AddSeconds(-2))
        {
            Metadata = JsonSerializer.Serialize(
                new RestSiteActionMetadata("explicit-click", "rest site: smith", fingerprint),
                GuiSmokeShared.JsonOptions),
        };
        var graceRequest = restSiteMetadataRequest with
        {
            History = new[] { firstSmithClick },
        };
        var graceDecision = AutoDecisionProvider.Decide(graceRequest);
        Assert(string.Equals(graceDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
               && string.Equals(graceDecision.DecisionRisk, "rest-site-post-click-recapture-grace", StringComparison.OrdinalIgnoreCase)
               && string.Equals(graceDecision.TargetLabel, "rest site: smith", StringComparison.OrdinalIgnoreCase),
            "After a rest-site option click, the next identical fingerprint should yield a single recapture grace wait instead of another smith click.");

        var graceHistoryEntry = new GuiSmokeHistoryEntry(
            GuiSmokePhase.ChooseFirstNode.ToString(),
            "wait",
            graceDecision.TargetLabel,
            DateTimeOffset.UtcNow.AddSeconds(-1))
        {
            Metadata = AutoDecisionProvider.BuildRestSiteHistoryMetadataForDecision(graceRequest, graceDecision),
        };
        var noOpRequest = restSiteMetadataRequest with
        {
            History = new[] { firstSmithClick, graceHistoryEntry },
        };
        var noOpDecision = AutoDecisionProvider.Decide(noOpRequest);
        Assert(string.Equals(noOpDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
               && string.Equals(noOpDecision.DecisionRisk, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase)
               && TryClassifyRestSitePostClickNoOp(GuiSmokePhase.ChooseFirstNode, noOpRequest, noOpDecision, out var noOpMessage)
               && noOpMessage.Contains("rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase),
            "A repeated explicit rest-site fingerprint after grace should escalate to rest-site-post-click-noop and stop repeated smith clicks.");

        var selectionFailedSummary = restSiteMetadataSummary with
        {
            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["restSiteSelectionLastSignal"] = "after-select-failure",
                ["restSiteSelectionLastOptionId"] = "SMITH",
                ["restSiteSelectionCurrentStatus"] = "explicit-choice",
                ["restSiteUpgradeScreenVisible"] = "false",
            },
        };
        var selectionFailedRequest = restSiteMetadataRequest with
        {
            Observer = selectionFailedSummary,
            AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, new ObserverState(selectionFailedSummary, null, null, null)),
            History = new[] { firstSmithClick, graceHistoryEntry },
        };
        var selectionFailedDecision = AutoDecisionProvider.Decide(selectionFailedRequest);
        Assert(string.Equals(selectionFailedDecision.DecisionRisk, "rest-site-selection-failed", StringComparison.OrdinalIgnoreCase)
               && TryClassifyRestSitePostClickNoOp(GuiSmokePhase.ChooseFirstNode, selectionFailedRequest, selectionFailedDecision, out var selectionFailedMessage)
               && selectionFailedMessage.Contains("rest-site-selection-failed", StringComparison.OrdinalIgnoreCase),
            "Rest-site selection failure metadata should surface as rest-site-selection-failed instead of a generic noop.");

        var smithUpgradeSummary = restSiteMetadataSummary with
        {
            CurrentScreen = "upgrade",
            VisibleScreen = "upgrade",
            ChoiceExtractorPath = "rest-smith-upgrade",
            CurrentChoices = new[] { "Strike", "Defend", "Smith Confirm" },
            Choices = new[]
            {
                new ObserverChoice("rest-site-smith-card", "Strike", "420,220,150,210", "CARD.STRIKE_IRONCLAD", "Upgradable card")
                {
                    NodeId = "rest-site:smith-card:card-strike-ironclad",
                    BindingKind = "rest-site-smith-card",
                    BindingId = "CARD.STRIKE_IRONCLAD",
                    Enabled = true,
                    SemanticHints = new[] { "scene:rest-site", "substate:smith-grid", "source:grid-holder" },
                },
                new ObserverChoice("rest-site-smith-card", "Defend", "610,220,150,210", "CARD.DEFEND_IRONCLAD", "Upgradable card")
                {
                    NodeId = "rest-site:smith-card:card-defend-ironclad",
                    BindingKind = "rest-site-smith-card",
                    BindingId = "CARD.DEFEND_IRONCLAD",
                    Enabled = true,
                    SemanticHints = new[] { "scene:rest-site", "substate:smith-grid", "source:grid-holder" },
                },
                new ObserverChoice("rest-site-smith-confirm", "Smith Confirm", "980,520,150,80", "SMITH_CONFIRM", "Confirm smith upgrade")
                {
                    NodeId = "rest-site:smith-confirm",
                    BindingKind = "rest-site-smith-confirm",
                    BindingId = "SMITH_CONFIRM",
                    Enabled = true,
                    SemanticHints = new[] { "scene:rest-site", "substate:smith-confirm", "source:confirm-button" },
                },
            },
            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["restSiteSelectionCurrentStatus"] = "grid-visible",
                ["restSiteSelectionCurrentOptionId"] = "SMITH",
                ["restSiteUpgradeScreenVisible"] = "true",
                ["restSiteUpgradeCardCount"] = "2",
                ["restSiteViewKind"] = "smith-grid",
            },
        };
        var smithUpgradeObserver = new ObserverState(smithUpgradeSummary, null, null, null);
        var smithUpgradeRequest = restSiteMetadataRequest with
        {
            Observer = smithUpgradeSummary,
            AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, smithUpgradeObserver),
        };
        Assert(!HasExplicitRestSiteChoiceAuthority(smithUpgradeObserver, restSiteMetadataScreenshotPath)
               && smithUpgradeRequest.AllowedActions.Contains("click smith card", StringComparer.OrdinalIgnoreCase),
            "Visible smith upgrade state should disable explicit rest-site choice authority and expose smith card actions.");
        var smithUpgradeDecision = AutoDecisionProvider.Decide(smithUpgradeRequest);
        Assert(string.Equals(smithUpgradeDecision.TargetLabel, "rest site: smith confirm", StringComparison.OrdinalIgnoreCase),
            "Observer-exported smith confirm should outrank screenshot fallback once the confirm button is visible.");

        var disabledConfirmSmithUpgradeSummary = smithUpgradeSummary with
        {
            Choices = new[]
            {
                smithUpgradeSummary.Choices[0],
                smithUpgradeSummary.Choices[1],
                smithUpgradeSummary.Choices[2] with
                {
                    Enabled = false,
                },
            },
            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["restSiteSelectionCurrentStatus"] = "confirm-visible",
                ["restSiteSelectionCurrentOptionId"] = "SMITH",
                ["restSiteUpgradeScreenVisible"] = "true",
                ["restSiteUpgradeCardCount"] = "2",
                ["restSiteUpgradeConfirmVisible"] = "true",
                ["restSiteUpgradeConfirmEnabled"] = "false",
                ["restSiteViewKind"] = "smith-confirm",
            },
        };
        var disabledConfirmSmithUpgradeRequest = restSiteMetadataRequest with
        {
            Observer = disabledConfirmSmithUpgradeSummary,
            AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, new ObserverState(disabledConfirmSmithUpgradeSummary, null, null, null)),
        };
        var disabledConfirmSmithUpgradeDecision = AutoDecisionProvider.Decide(disabledConfirmSmithUpgradeRequest);
        Assert(string.Equals(disabledConfirmSmithUpgradeDecision.TargetLabel, "rest site: smith card", StringComparison.OrdinalIgnoreCase),
            "Disabled smith confirm should not be clicked while exported smith card choices remain actionable.");

        var restSiteProceedSummary = restSiteMetadataSummary with
        {
            CurrentScreen = "rest-site",
            VisibleScreen = "rest-site",
            ChoiceExtractorPath = "generic",
            CurrentChoices = new[] { "진행", "불타는 혈액" },
            Choices = new[]
            {
                new ObserverChoice("choice", "진행", "1576.3,761.3,282.45,113.4", null, "진행"),
                new ObserverChoice("relic", "불타는 혈액", "12,82,68,68", "RELIC.BURNING_BLOOD"),
            },
            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["restSiteSelectionLastSignal"] = "after-select-success",
                ["restSiteSelectionLastSuccess"] = "true",
            },
        };
        var restSiteProceedObserver = new ObserverState(restSiteProceedSummary, null, null, null);
        Assert(LooksLikeRestSiteProceedState(restSiteProceedSummary),
            "Rest-site proceed helper should recognize post-confirm proceed-visible observer state.");
        var restSiteProceedRequest = restSiteMetadataRequest with
        {
            Observer = restSiteProceedSummary,
            AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, restSiteProceedObserver),
        };
        var restSiteProceedDecision = AutoDecisionProvider.Decide(restSiteProceedRequest);
        Assert(string.Equals(restSiteProceedDecision.TargetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase),
            "Observer-visible rest-site proceed choice should create a visible proceed decision without waiting for screenshot arrows.");

        var restSiteProceedLabelOnlySummary = restSiteProceedSummary with
        {
            Choices = new[]
            {
                new ObserverChoice("relic", "불타는 혈액", "12,82,68,68", "RELIC.BURNING_BLOOD"),
            },
        };
        Assert(LooksLikeRestSiteProceedState(restSiteProceedLabelOnlySummary),
            "Rest-site proceed helper should still promote post-confirm proceed when currentChoices expose the proceed affordance before structured choice export stabilizes.");
        var restSiteProceedBranchRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-rest-site-proceed-{Guid.NewGuid():N}");
        Directory.CreateDirectory(restSiteProceedBranchRoot);
        try
        {
            var restSiteProceedBranchLogger = new ArtifactRecorder(restSiteProceedBranchRoot);
            var restSiteProceedBranchHistory = new List<GuiSmokeHistoryEntry>();
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitMap,
                    restSiteProceedObserver,
                    restSiteProceedBranchHistory,
                    restSiteProceedBranchLogger,
                    9,
                    true,
                    out var restSiteProceedNextPhase)
                && restSiteProceedNextPhase == GuiSmokePhase.ChooseFirstNode,
                "WaitMap should branch immediately to ChooseFirstNode when a post-confirm rest-site proceed affordance is visible.");
        }
        finally
        {
            try
            {
                Directory.Delete(restSiteProceedBranchRoot, true);
            }
            catch
            {
            }
        }

        var legacyRestSiteRequestPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "artifacts",
            "gui-smoke",
            "verify-longrun-20260316-10",
            "attempts",
            "0001",
            "steps",
            "0010.request.json");
        if (File.Exists(legacyRestSiteRequestPath))
        {
            var legacyReplayRequest = LoadReplayRequest(legacyRestSiteRequestPath).Request;
            var legacyReplayArtifact = EvaluateAutoDecisionWithDiagnostics(legacyRestSiteRequestPath, legacyReplayRequest).CandidateDump;
            Assert(string.Equals(legacyReplayArtifact.FinalDecision.TargetLabel, "rest site: smith", StringComparison.OrdinalIgnoreCase),
                "Legacy replay artifact without binding metadata should still choose rest site: smith.");
        }
    }
    finally
    {
        if (File.Exists(restSiteMetadataScreenshotPath))
        {
            File.Delete(restSiteMetadataScreenshotPath);
        }
    }

    var mapOverlayScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-map-overlay-self-test-{Guid.NewGuid():N}.png");
    try
    {
        using (var bitmap = new Bitmap(1280, 720))
        using (var graphics = Graphics.FromImage(bitmap))
        using (var parchmentBrush = new SolidBrush(Color.FromArgb(201, 176, 132)))
        using (var arrowBrush = new SolidBrush(Color.FromArgb(220, 55, 48)))
        using (var nodeBrush = new SolidBrush(Color.FromArgb(63, 54, 42)))
        using (var legendBrush = new SolidBrush(Color.FromArgb(184, 207, 222)))
        {
            graphics.Clear(Color.Black);
            graphics.FillRectangle(parchmentBrush, new Rectangle(210, 40, 840, 650));
            graphics.FillEllipse(arrowBrush, new Rectangle(892, 414, 54, 46));
            graphics.FillEllipse(nodeBrush, new Rectangle(884, 454, 82, 82));
            graphics.FillEllipse(nodeBrush, new Rectangle(882, 548, 86, 86));
            graphics.FillRectangle(legendBrush, new Rectangle(1038, 193, 176, 285));
            graphics.FillPolygon(arrowBrush, new[]
            {
                new Point(18, 515),
                new Point(66, 478),
                new Point(66, 495),
                new Point(102, 495),
                new Point(102, 535),
                new Point(66, 535),
                new Point(66, 552),
            });
            bitmap.Save(mapOverlayScreenshotPath, ImageFormat.Png);
        }

        using var mapOverlayStateDocument = JsonDocument.Parse("""{"meta":{"declaringType":"MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen","instanceType":"MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen","choiceExtractorPath":"event","mapOverlayVisible":"true"}}""");
        var mapOverlayObserver = new ObserverState(
            new ObserverSummary(
                "event",
                "event",
                false,
                DateTimeOffset.UtcNow,
                "inv-map-overlay",
                true,
                "mixed",
                "stable",
                "episode-map-overlay",
                "None",
                "event",
                80,
                80,
                null,
                new[] { "계속", "LeftArrow", "휴식 (1,2)" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("event-option:0", "event-option", "계속", "922,596,800,100", true),
                    new ObserverActionNode("back:left", "choice", "LeftArrow", "48,930,88,88", true),
                },
                new[]
                {
                    new ObserverChoice("choice", "계속", "922,596,800,100", "계속"),
                    new ObserverChoice("choice", "LeftArrow", "48,930,88,88", "LeftArrow"),
                    new ObserverChoice("map-node", "휴식 (1,2)", "897,581,124,124", "1,2", "type:Rest;coord:1,2"),
                },
                Array.Empty<ObservedCombatHandCard>()),
            mapOverlayStateDocument,
            null,
            null);
        var mapOverlaySignature = ComputeSceneSignature(mapOverlayScreenshotPath, mapOverlayObserver, GuiSmokePhase.ChooseFirstNode);
        Assert(mapOverlaySignature.Contains("layer:map-overlay-foreground", StringComparison.OrdinalIgnoreCase), "Map overlay foreground should be modeled explicitly when NMapScreen is open over a stale event context.");
        Assert(mapOverlaySignature.Contains("stale:event-choice", StringComparison.OrdinalIgnoreCase), "Stale event choices should be marked as stale when the map overlay is foreground.");
        Assert(mapOverlaySignature.Contains("map-back-navigation-available", StringComparison.OrdinalIgnoreCase), "Map overlay should expose the back-navigation affordance.");
        Assert(!GetAllowedActions(GuiSmokePhase.ChooseFirstNode, mapOverlayObserver).Contains("click visible map advance", StringComparer.OrdinalIgnoreCase), "ChooseFirstNode should not expose visible map advance while map overlay foreground is active.");
        Assert(GetAllowedActions(GuiSmokePhase.ChooseFirstNode, mapOverlayObserver).Contains("click exported reachable node", StringComparer.OrdinalIgnoreCase), "ChooseFirstNode should promote exported map points to first-class candidates in mixed map-overlay state.");
        Assert(GetAllowedActions(GuiSmokePhase.ChooseFirstNode, mapOverlayObserver).Contains("click map back", StringComparer.OrdinalIgnoreCase), "ChooseFirstNode should open map back-navigation in mixed map-overlay state.");
        var mapOverlayReplay = AutoDecisionProvider.Analyze(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            15,
            GuiSmokePhase.ChooseFirstNode.ToString(),
            "Click the first reachable map node.",
            DateTimeOffset.UtcNow,
            mapOverlayScreenshotPath,
            new WindowBounds(1, 32, 1280, 720),
            mapOverlaySignature,
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            mapOverlayObserver.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            BuildAllowedActions(GuiSmokePhase.ChooseFirstNode, mapOverlayObserver, Array.Empty<CombatCardKnowledgeHint>(), mapOverlayScreenshotPath, Array.Empty<GuiSmokeHistoryEntry>()),
            new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleEvent.ToString(), "event-resolved-map", null, DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.WaitMap.ToString(), "branch-map", null, DateTimeOffset.UtcNow.AddSeconds(-2)),
            },
            "Use exported reachable map points before any screenshot arrow fallback.",
            null));
        Assert(!string.Equals(mapOverlayReplay.FinalDecision.TargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase), "Map overlay replay analysis should not fall back to the red current-node arrow.");
    }
    finally
    {
        if (File.Exists(mapOverlayScreenshotPath))
        {
            File.Delete(mapOverlayScreenshotPath);
        }
    }

    var rewardRankingScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-reward-ranking-self-test-{Guid.NewGuid():N}.png");
    try
    {
        using (var bitmap = new Bitmap(1280, 720))
        using (var graphics = Graphics.FromImage(bitmap))
        using (var panelBrush = new SolidBrush(Color.FromArgb(222, 205, 168)))
        using (var arrowBrush = new SolidBrush(Color.FromArgb(220, 60, 55)))
        {
            graphics.Clear(Color.Black);
            graphics.FillRectangle(panelBrush, new Rectangle(420, 180, 480, 320));
            graphics.FillEllipse(arrowBrush, new Rectangle(595, 450, 90, 70));
            bitmap.Save(rewardRankingScreenshotPath, ImageFormat.Png);
        }

        var rewardObserverState = new ObserverState(
            new ObserverSummary(
                "rewards",
                "rewards",
                false,
                DateTimeOffset.UtcNow,
                "inv-rewards",
                true,
                "mixed",
                "stable",
                "episode-rewards",
                "Monster",
                "reward",
                80,
                80,
                null,
                new[] { "11 골드", "넘기기" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("reward-item:0", "reward-item", "11 골드", "758,374,402,86", true),
                    new ObserverActionNode("proceed:1", "proceed", "넘기기", "1583,764,269,108", true),
                },
                new[]
                {
                    new ObserverChoice("choice", "11 골드", "758,374,402,86"),
                    new ObserverChoice("choice", "넘기기", "1583,764,269,108"),
                },
                Array.Empty<ObservedCombatHandCard>()),
            null,
            null,
            null);
        var rewardSceneSignature = ComputeSceneSignature(rewardRankingScreenshotPath, rewardObserverState, GuiSmokePhase.HandleRewards);
        Assert(rewardSceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase), "Reward scenes with an explicit reward panel should record background map arrows as contamination, not as primary map authority.");
        Assert(!rewardSceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase), "Reward scenes should not advertise visible map advance while explicit reward affordances are still present.");
        Assert(!GetAllowedActions(GuiSmokePhase.HandleRewards, rewardObserverState).Contains("click visible map advance", StringComparer.OrdinalIgnoreCase), "Reward allowlist should not open visible map advance while explicit reward affordances are present.");
        var rewardDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            33,
            GuiSmokePhase.HandleRewards.ToString(),
            "Resolve the visible reward screen.",
            DateTimeOffset.UtcNow,
            rewardRankingScreenshotPath,
            new WindowBounds(0, 0, 1280, 720),
            rewardSceneSignature,
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            rewardObserverState.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            GetAllowedActions(GuiSmokePhase.HandleRewards, rewardObserverState),
            Array.Empty<GuiSmokeHistoryEntry>(),
            "Reward panel is authoritative over any background map contamination.",
            null));
        Assert(string.Equals(rewardDecision.TargetLabel, "claim reward item", StringComparison.OrdinalIgnoreCase), "Explicit reward items should outrank screenshot-derived map fallback on NRewardsScreen.");
        Assert(TryClassifyRewardMapLoop(
            GuiSmokePhase.HandleRewards,
            new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                37,
                GuiSmokePhase.HandleRewards.ToString(),
                "Guard against reward/map loops.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                rewardSceneSignature,
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                rewardObserverState.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                GetAllowedActions(GuiSmokePhase.HandleRewards, rewardObserverState),
                new[]
                {
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleRewards.ToString(), "click", "visible map advance", DateTimeOffset.UtcNow.AddSeconds(-8)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.WaitMap.ToString(), "branch-reward", null, DateTimeOffset.UtcNow.AddSeconds(-6)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleRewards.ToString(), "click", "visible map advance", DateTimeOffset.UtcNow.AddSeconds(-4)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.WaitMap.ToString(), "branch-reward", null, DateTimeOffset.UtcNow.AddSeconds(-2)),
                },
                "Abort repeated reward/map oscillation before another map fallback click.",
                null),
            new GuiSmokeStepDecision("act", "click", null, 0.5, 0.5, "visible map advance", "looping on background map", 0.5, "rewards", 1200, true, null),
            out _), "Repeated visible-map clicks while explicit reward affordances remain should be classified as reward-map-loop.");

        var layeredRewardObserver = new ObserverState(
            new ObserverSummary(
                "rewards",
                "map",
                false,
                DateTimeOffset.UtcNow,
                "inv-layered-reward",
                false,
                "mixed",
                "stabilizing",
                "episode-layered-reward",
                "None",
                "reward",
                80,
                80,
                null,
                new[] { "11 골드", "넘기기", "LeftArrow" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("reward-item:0", "reward-item", "11 골드", "758,374,402,86", true),
                    new ObserverActionNode("proceed:1", "proceed", "넘기기", "1583,764,269,108", true),
                    new ObserverActionNode("overlay:left", "choice", "LeftArrow", "48,930,88,88", true),
                },
                new[]
                {
                    new ObserverChoice("choice", "11 골드", "758,374,402,86"),
                    new ObserverChoice("choice", "넘기기", "1583,764,269,108"),
                    new ObserverChoice("choice", "LeftArrow", "48,930,88,88"),
                },
                Array.Empty<ObservedCombatHandCard>()),
            null,
            null,
            null);
        var layeredRewardState = BuildRewardMapLayerStateForObserver(layeredRewardObserver.Summary, new WindowBounds(0, 0, 1280, 720));
        Assert(layeredRewardState.RewardPanelVisible, "Layered reward/map state should keep the reward panel as foreground while explicit reward bounds remain usable.");
        Assert(layeredRewardState.MapContextVisible, "Layered reward/map state should preserve background map context instead of forcing a single exclusive screen.");
        Assert(layeredRewardState.RewardBackNavigationAvailable, "Layered reward/map state should record reward back-navigation affordances.");

        var staleRewardObserver = new ObserverState(
            new ObserverSummary(
                "rewards",
                "map",
                false,
                DateTimeOffset.UtcNow,
                "inv-stale-reward",
                false,
                "mixed",
                "stabilizing",
                "episode-stale-reward",
                "None",
                "reward",
                80,
                80,
                null,
                new[] { "넘기기" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("proceed:0", "proceed", "넘기기", "1983,764,269,108", true),
                },
                new[]
                {
                    new ObserverChoice("choice", "넘기기", "1983,764,269,108"),
                },
                Array.Empty<ObservedCombatHandCard>()),
            null,
            null,
            null);
        var staleRewardState = BuildRewardMapLayerStateForObserver(staleRewardObserver.Summary, new WindowBounds(1, 32, 1280, 720));
        Assert(!staleRewardState.RewardPanelVisible, "Reward foreground should clear once only stale off-window reward bounds remain.");
        Assert(staleRewardState.StaleRewardChoicePresent, "Layered reward/map state should mark stale reward choices after panel authority disappears.");
        Assert(staleRewardState.OffWindowBoundsReused, "Off-window reward bounds should be flagged for reward/map loop diagnosis.");
        Assert(GetAllowedActions(GuiSmokePhase.HandleRewards, staleRewardObserver).Contains("click visible map advance", StringComparer.OrdinalIgnoreCase), "When reward authority disappears, HandleRewards should reopen map affordances instead of keeping stale reward-only actions.");
        Assert(!GetAllowedActions(GuiSmokePhase.HandleRewards, staleRewardObserver).Contains("click proceed", StringComparer.OrdinalIgnoreCase), "Stale reward bounds should be hard-rejected from the actionable set once only map context remains.");


        var rewardPolicyObserver = new ObserverState(
            new ObserverSummary(
                "rewards",
                "rewards",
                false,
                DateTimeOffset.UtcNow,
                "inv-reward-policy",
                true,
                "mixed",
                "stable",
                "episode-reward-policy",
                "None",
                "reward",
                80,
                80,
                null,
                new[] { "11 골드", "넘기기", "CARD.BATTLE_TRANCE" },
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                new[]
                {
                    new ObserverChoice("card", "CARD.BATTLE_TRANCE", "628,248,180,254", "CARD.BATTLE_TRANCE", "카드 보상"),
                    new ObserverChoice("choice", "11 골드", "758,374,402,86", null, "11 골드"),
                    new ObserverChoice("choice", "넘기기", "1280,764,269,108", null, "넘기기"),
                },
                Array.Empty<ObservedCombatHandCard>()),
            null,
            null,
            null);
        Assert(BuildAllowedActions(GuiSmokePhase.HandleRewards, rewardPolicyObserver, Array.Empty<CombatCardKnowledgeHint>(), rewardRankingScreenshotPath, Array.Empty<GuiSmokeHistoryEntry>()).Contains("click reward card choice", StringComparer.OrdinalIgnoreCase), "Reward allowlist should keep claimable reward card actions open while a reward card remains visible.");
        var rewardPolicyDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            38,
            GuiSmokePhase.HandleRewards.ToString(),
            "Prefer meaningful reward progression over stale gold/skip bias.",
            DateTimeOffset.UtcNow,
            rewardRankingScreenshotPath,
            new WindowBounds(0, 0, 1280, 720),
            ComputeSceneSignature(rewardRankingScreenshotPath, rewardPolicyObserver, GuiSmokePhase.HandleRewards),
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            rewardPolicyObserver.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            GetAllowedActions(GuiSmokePhase.HandleRewards, rewardPolicyObserver),
            Array.Empty<GuiSmokeHistoryEntry>(),
            "Prefer cards or relics over default gold/skip bias when a real reward remains visible.",
            null));
        Assert(!string.Equals(rewardPolicyDecision.TargetLabel, "reward skip", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(rewardPolicyDecision.TargetLabel, "proceed after resolving rewards", StringComparison.OrdinalIgnoreCase),
            "Reward policy should prefer a visible card or claimable reward over default gold/skip choices.");
        Assert(ShouldAllowRewardMapRecovery(
                new GuiSmokeStepRequest(
                    "run",
                    "boot-to-long-run",
                    39,
                    GuiSmokePhase.HandleRewards.ToString(),
                    "Allow one recapture window after a map recovery click.",
                    DateTimeOffset.UtcNow,
                    rewardRankingScreenshotPath,
                    new WindowBounds(1, 32, 1280, 720),
                    "phase:handlerewards|screen:rewards|visible:map|stale:reward-choice|stale:reward-bounds|layer:map-background|visible:map-arrow",
                    "0001",
                    1,
                    3,
                    true,
                    "tactical",
                    null,
                    staleRewardObserver.Summary,
                    Array.Empty<KnownRecipeHint>(),
                    Array.Empty<EventKnowledgeCandidate>(),
                    Array.Empty<CombatCardKnowledgeHint>(),
                    new[] { "click visible map advance", "click reward back", "wait" },
                    Array.Empty<GuiSmokeHistoryEntry>(),
                    "allow a short recovery window",
                    null),
                new GuiSmokeStepDecision("act", "click", null, 0.7, 0.5, "visible reachable node", "recovery attempt", 0.9, "map", 1200, true, null)),
            "Reward-map recovery should allow a short recapture window after a real map progression click.");
    }
    finally
    {
        if (File.Exists(rewardRankingScreenshotPath))
        {
            File.Delete(rewardRankingScreenshotPath);
        }
    }

    var combatNoOpScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-combat-noop-self-test-{Guid.NewGuid():N}.png");
    var handleCombatParityRequestPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-handle-combat-parity-{Guid.NewGuid():N}.request.json");
    try
    {
        using (var bitmap = new Bitmap(1280, 720))
        {
            bitmap.Save(combatNoOpScreenshotPath, ImageFormat.Png);
        }

        var combatNoOpObserver = new ObserverSummary(
            "combat",
            "combat",
            true,
            DateTimeOffset.UtcNow,
            "inv-combat-noop",
            true,
            "mixed",
            "stable",
            "episode-combat-noop",
            "Combat",
            "combat",
            76,
            80,
            1,
            new[] { "3턴 종료" },
            Array.Empty<string>(),
            new[] { new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true) },
            Array.Empty<ObserverChoice>(),
            new[]
            {
                new ObservedCombatHandCard(3, "CARD.BASH", "Attack", null),
                new ObservedCombatHandCard(4, "CARD.STRIKE_IRONCLAD", "Attack", null),
                new ObservedCombatHandCard(5, "CARD.STRIKE_IRONCLAD", "Attack", null),
            });
        var combatNoOpHistory = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 3", DateTimeOffset.UtcNow.AddSeconds(-10)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "auto-target enemy", DateTimeOffset.UtcNow.AddSeconds(-8)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 3", DateTimeOffset.UtcNow.AddSeconds(-7)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 3", DateTimeOffset.UtcNow.AddSeconds(-6)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "auto-target enemy", DateTimeOffset.UtcNow.AddSeconds(-4)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 3", DateTimeOffset.UtcNow.AddSeconds(-3)),
        };
        var combatNoOpDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            54,
            GuiSmokePhase.HandleCombat.ToString(),
            "Play the turn from the screenshot.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(0, 0, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable",
            "0002",
            2,
            3,
            false,
            "tactical",
            null,
            combatNoOpObserver,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            new[]
            {
                new CombatCardKnowledgeHint(3, "CARD.BASH", "Attack", "AnyEnemy", 2, "self-test"),
                new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(5, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            },
            new[] { "click card", "click enemy", "click end turn", "wait" },
            combatNoOpHistory,
            "prefer a playable card or end turn when energy is insufficient for the previously selected card",
            null));
        Assert(string.Equals(combatNoOpDecision.TargetLabel, "combat select attack slot 4", StringComparison.OrdinalIgnoreCase), "Combat decisioning should skip the unplayable Bash lane and pick a playable Strike instead.");
        Assert(TryClassifyCombatNoOpLoop(
            GuiSmokePhase.HandleCombat,
            new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                55,
                GuiSmokePhase.HandleCombat.ToString(),
                "Guard against combat no-op loops.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable",
                "0002",
                2,
                3,
                false,
                "tactical",
                null,
                combatNoOpObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                new[]
                {
                    new CombatCardKnowledgeHint(3, "CARD.BASH", "Attack", "AnyEnemy", 2, "self-test"),
                    new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                    new CombatCardKnowledgeHint(5, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                },
                new[] { "click card", "click enemy", "click end turn", "wait" },
                combatNoOpHistory,
                "guard the repeated no-op lane",
                null),
            new GuiSmokeStepDecision("act", "press-key", "3", null, null, "combat select attack slot 3", "looping on unplayable Bash", 0.5, "combat", 250, true, null),
            out _), "Repeated unplayable card selection should be classified as a combat-noop-loop before another no-op action is executed.");
        var blockedEnemyActions = BuildAllowedActions(GuiSmokePhase.HandleCombat, new ObserverState(combatNoOpObserver, null, null, null), new[]
        {
            new CombatCardKnowledgeHint(3, "CARD.BASH", "Attack", "AnyEnemy", 1, "self-test"),
        }, combatNoOpScreenshotPath, combatNoOpHistory);
        Assert(!blockedEnemyActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Combat allowlist should close direct enemy targeting when the currently pending attack lane has already produced repeated no-op outcomes.");

        var recoveredPendingSelectionHistory = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-6)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 4", DateTimeOffset.UtcNow.AddSeconds(-5)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-4)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-3)),
        };
        var recoveredPendingSelection = CombatHistorySupport.TryGetPendingCombatSelection(recoveredPendingSelectionHistory);
        Assert(recoveredPendingSelection is { Kind: AutoCombatCardKind.AttackLike, SlotIndex: 4 }, "Combat pending selection recovery should preserve lane 4 across combat-noop and enemy-target labels.");
        Assert(CombatHistorySupport.GetCombatNoOpCountsBySlot(recoveredPendingSelectionHistory).TryGetValue(4, out var recoveredPendingSelectionNoOpCount)
               && recoveredPendingSelectionNoOpCount == 2,
            "Combat no-op counting should attribute enemy-target no-ops back to the recovered lane 4 selection.");

        var artifact0020Observer = new ObserverSummary(
            "combat",
            "combat",
            true,
            DateTimeOffset.UtcNow,
            "inv-artifact-0020",
            true,
            "mixed",
            "stable",
            "episode-artifact-0020",
            "Monster",
            "combat",
            80,
            80,
            3,
            new[] { "압축벌레" },
            Array.Empty<string>(),
            new[]
            {
                new ObserverActionNode("enemy-target:2", "enemy-target", "압축벌레", "1398.24,621.6,83.52,88", true),
            },
            new[]
            {
                new ObserverChoice("enemy-target", "압축벌레", "1398.24,621.6,83.52,88", "압축벌레", "target-source:vfx-spawn-hitbox")
                {
                    NodeId = "enemy-target:2",
                },
            },
            new[]
            {
                new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                new ObservedCombatHandCard(4, "CARD.STRIKE_IRONCLAD", "Attack", 1),
            });
        var artifact0020Knowledge = new[]
        {
            new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
        };
        var artifact0020History = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-10)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-9)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-8)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-7)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 4", DateTimeOffset.UtcNow.AddSeconds(-6)),
        };
        var artifact0020AllowedActions = BuildAllowedActions(
            GuiSmokePhase.HandleCombat,
            new ObserverState(artifact0020Observer, null, null, null),
            artifact0020Knowledge,
            combatNoOpScreenshotPath,
            artifact0020History);
        Assert(!artifact0020AllowedActions.Contains("select attack slot 4", StringComparer.OrdinalIgnoreCase), "Artifact-like 0020 allowlist should close slot 4 after repeated no-op lane evidence.");
        Assert(!artifact0020AllowedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Artifact-like 0020 allowlist should close enemy targeting once the pending lane is blocked.");
        var artifact0020Request = new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            20,
            GuiSmokePhase.HandleCombat.ToString(),
            "Replay the artifact-like blocked lane state.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(1, 32, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            artifact0020Observer,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            artifact0020Knowledge,
            artifact0020AllowedActions,
            artifact0020History,
            "Close illegal attack selection and target retries.",
            null);
        var artifact0020Decision = AutoDecisionProvider.Decide(artifact0020Request);
        Assert(!string.Equals(artifact0020Decision.TargetLabel, "combat select attack slot 4", StringComparison.OrdinalIgnoreCase), "Artifact-like 0020 regression should not reissue illegal slot 4 selection.");
        Assert(CombatDecisionContract.IsAllowed(artifact0020Request, artifact0020Decision, out var artifact0020SemanticAction)
               && !string.Equals(artifact0020SemanticAction, "select attack slot 4", StringComparison.OrdinalIgnoreCase),
            "Artifact-like 0020 regression should end with a legal combat semantic action.");

        var artifact0021Observer = artifact0020Observer with
        {
            InventoryId = "inv-artifact-0021",
            SceneEpisodeId = "episode-artifact-0021",
            CombatHand = new[]
            {
                new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                new ObservedCombatHandCard(4, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                new ObservedCombatHandCard(5, "CARD.STRIKE_IRONCLAD", "Attack", 1),
            },
        };
        var artifact0021Knowledge = new[]
        {
            new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            new CombatCardKnowledgeHint(5, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
        };
        var artifact0021History = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-10)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-9)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 4", DateTimeOffset.UtcNow.AddSeconds(-8)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-7)),
        };
        var artifact0021AllowedActions = BuildAllowedActions(
            GuiSmokePhase.HandleCombat,
            new ObserverState(artifact0021Observer, null, null, null),
            artifact0021Knowledge,
            combatNoOpScreenshotPath,
            artifact0021History);
        Assert(artifact0021AllowedActions.Contains("select attack slot 5", StringComparer.OrdinalIgnoreCase), "Artifact-like 0021 allowlist should keep alternate slot 5 open.");
        Assert(!artifact0021AllowedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Artifact-like 0021 allowlist should keep enemy targeting closed while slot 4 remains blocked.");
        var artifact0021Request = new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            21,
            GuiSmokePhase.HandleCombat.ToString(),
            "Replay the artifact-like blocked target fallback state.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(1, 32, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            artifact0021Observer,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            artifact0021Knowledge,
            artifact0021AllowedActions,
            artifact0021History,
            "Prefer the legal alternate slot over an illegal enemy click.",
            null);
        var artifact0021Decision = AutoDecisionProvider.Decide(artifact0021Request);
        Assert(!(artifact0021Decision.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) ?? false), "Artifact-like 0021 regression should not emit any illegal enemy-target click.");
        Assert(CombatDecisionContract.IsAllowed(artifact0021Request, artifact0021Decision, out var artifact0021SemanticAction)
               && !string.Equals(artifact0021SemanticAction, "click enemy", StringComparison.OrdinalIgnoreCase),
            "Artifact-like 0021 regression should end with a legal non-enemy-click combat semantic action.");

        var combatEligibility0015Observer = new ObserverSummary(
            "combat",
            "combat",
            true,
            DateTimeOffset.UtcNow,
            "inv-combat-eligibility-0015",
            true,
            "mixed",
            "stable",
            "episode-combat-eligibility-0015",
            "Monster",
            "combat-targets",
            73,
            80,
            2,
            new[] { "압축벌레" },
            Array.Empty<string>(),
            new[]
            {
                new ObserverActionNode("enemy-target:2", "enemy-target", "압축벌레", "1398.24,621.6,83.52,88", true),
            },
            new[]
            {
                new ObserverChoice("enemy-target", "압축벌레", "1398.24,621.6,83.52,88", "압축벌레", "target-source:vfx-spawn-hitbox")
                {
                    NodeId = "enemy-target:2",
                },
            },
            new[]
            {
                new ObservedCombatHandCard(1, "CARD.POOR_SLEEP", "Curse", null),
                new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", null),
                new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", null),
                new ObservedCombatHandCard(4, "CARD.STRIKE_IRONCLAD", "Attack", null),
            })
        {
            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
            },
        };
        var combatEligibility0015Knowledge = new[]
        {
            new CombatCardKnowledgeHint(1, "CARD.POOR_SLEEP", "Curse", "None", -1, "self-test"),
            new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
        };
        var combatEligibility0015History = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 2", DateTimeOffset.UtcNow.AddSeconds(-5)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "auto-end turn", DateTimeOffset.UtcNow.AddSeconds(-4)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-3)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target 압축벌레 recenter", DateTimeOffset.UtcNow.AddSeconds(-2)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 2", DateTimeOffset.UtcNow.AddSeconds(-1)),
        };
        var combatEligibility0015Actions = BuildAllowedActions(
            GuiSmokePhase.HandleCombat,
            new ObserverState(combatEligibility0015Observer, null, null, null),
            combatEligibility0015Knowledge,
            combatNoOpScreenshotPath,
            combatEligibility0015History);
        Assert(!combatEligibility0015Actions.Contains("select non-enemy slot 1", StringComparer.OrdinalIgnoreCase), "0015-like combat allowlist should not promote the curse in slot 1 as a playable non-enemy action.");

        var enemyTurn0021Observer = new ObserverSummary(
            "combat",
            "combat",
            true,
            DateTimeOffset.UtcNow,
            "inv-enemy-turn-0021",
            true,
            "mixed",
            "stable",
            "episode-enemy-turn-0021",
            "Monster",
            "combat-targets",
            53,
            80,
            3,
            new[] { "압축벌레" },
            Array.Empty<string>(),
            new[]
            {
                new ObserverActionNode("enemy-target:2", "enemy-target", "압축벌레", "1398.24,621.6,83.52,88", true),
            },
            new[]
            {
                new ObserverChoice("enemy-target", "압축벌레", "1398.24,621.6,83.52,88", "압축벌레", "target-source:vfx-spawn-hitbox")
                {
                    NodeId = "enemy-target:2",
                },
            },
            new[]
            {
                new ObservedCombatHandCard(1, "CARD.POOR_SLEEP", "Curse", null),
                new ObservedCombatHandCard(2, "CARD.BASH", "Attack", null),
                new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", null),
            })
        {
            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=true;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
            },
        };
        var enemyTurn0021Knowledge = new[]
        {
            new CombatCardKnowledgeHint(1, "CARD.POOR_SLEEP", "Curse", "None", -1, "self-test"),
            new CombatCardKnowledgeHint(2, "CARD.BASH", "Attack", "AnyEnemy", 2, "self-test"),
            new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
        };
        var enemyTurn0021Actions = BuildAllowedActions(
            GuiSmokePhase.HandleCombat,
            new ObserverState(enemyTurn0021Observer, null, null, null),
            enemyTurn0021Knowledge,
            combatNoOpScreenshotPath,
            Array.Empty<GuiSmokeHistoryEntry>());
        Assert(enemyTurn0021Actions.Length == 1
               && string.Equals(enemyTurn0021Actions[0], "wait", StringComparison.OrdinalIgnoreCase),
            "0021-like enemy-turn combat allowlist should collapse to wait only.");
        var enemyTurn0021Request = new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            21,
            GuiSmokePhase.HandleCombat.ToString(),
            "Hold during enemy turn.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(1, 32, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            enemyTurn0021Observer,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            enemyTurn0021Knowledge,
            enemyTurn0021Actions,
            Array.Empty<GuiSmokeHistoryEntry>(),
            "Do not act during enemy turn.",
            null);
        var enemyTurn0021Decision = AutoDecisionProvider.Decide(enemyTurn0021Request);
        Assert(string.Equals(enemyTurn0021Decision.Status, "wait", StringComparison.OrdinalIgnoreCase), "0021-like enemy-turn combat decision should wait instead of issuing a player action.");

        var repeatedNonEnemyHistory = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 1", DateTimeOffset.UtcNow.AddSeconds(-4)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 1", DateTimeOffset.UtcNow.AddSeconds(-3)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 1", DateTimeOffset.UtcNow.AddSeconds(-2)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 1", DateTimeOffset.UtcNow.AddSeconds(-1)),
        };
        var repeatedNonEnemyActions = BuildAllowedActions(
            GuiSmokePhase.HandleCombat,
            new ObserverState(combatEligibility0015Observer, null, null, null),
            combatEligibility0015Knowledge,
            combatNoOpScreenshotPath,
            repeatedNonEnemyHistory);
        Assert(!repeatedNonEnemyActions.Contains("select non-enemy slot 1", StringComparer.OrdinalIgnoreCase), "Repeated non-enemy regression should keep slot 1 closed when it only contains a curse.");
        var repeatedNonEnemyRequest = new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            22,
            GuiSmokePhase.HandleCombat.ToString(),
            "Do not repeat the illegal curse selection loop.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(1, 32, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            combatEligibility0015Observer,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            combatEligibility0015Knowledge,
            repeatedNonEnemyActions,
            repeatedNonEnemyHistory,
            "Close the non-enemy repeat loop.",
            null);
        var repeatedNonEnemyDecision = AutoDecisionProvider.Decide(repeatedNonEnemyRequest);
        Assert(!string.Equals(repeatedNonEnemyDecision.TargetLabel, "combat select non-enemy slot 1", StringComparison.OrdinalIgnoreCase), "Repeated non-enemy regression should not reissue slot 1.");
        Assert(!CombatDecisionContract.IsAllowed(
                repeatedNonEnemyRequest with { AllowedActions = new[] { "select non-enemy slot 2", "wait" } },
                new GuiSmokeStepDecision("act", "click-current", null, null, null, "confirm selected non-enemy card", "test", 0.5, "combat", 0, true, null),
                out _),
            "click-current should not be legal without actual selected-state evidence.");

        var pendingNonEnemySlot3History = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 3", DateTimeOffset.UtcNow.AddSeconds(-1)),
        };
        var pendingNonEnemySlot3Observer = new ObserverSummary(
            "combat",
            "combat",
            true,
            DateTimeOffset.UtcNow,
            "inv-pending-non-enemy-slot3",
            true,
            "mixed",
            "stable",
            "episode-pending-non-enemy-slot3",
            "Monster",
            "combat-targets",
            73,
            80,
            2,
            new[] { "압축벌레" },
            Array.Empty<string>(),
            new[]
            {
                new ObserverActionNode("enemy-target:2", "enemy-target", "압축벌레", "1398.24,621.6,83.52,88", true),
            },
            new[]
            {
                new ObserverChoice("enemy-target", "압축벌레", "1398.24,621.6,83.52,88", "압축벌레", "target-source:vfx-spawn-hitbox")
                {
                    NodeId = "enemy-target:2",
                },
            },
            new[]
            {
                new ObservedCombatHandCard(1, "CARD.POOR_SLEEP", "Curse", null),
                new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", null),
                new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", null),
            })
        {
            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
            },
        };
        var pendingNonEnemySlot3Knowledge = new[]
        {
            new CombatCardKnowledgeHint(1, "CARD.POOR_SLEEP", "Curse", "None", -1, "self-test"),
            new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
        };
        var pendingNonEnemySlot3Actions = BuildAllowedActions(
            GuiSmokePhase.HandleCombat,
            new ObserverState(pendingNonEnemySlot3Observer, null, null, null),
            pendingNonEnemySlot3Knowledge,
            combatNoOpScreenshotPath,
            pendingNonEnemySlot3History);
        Assert(!pendingNonEnemySlot3Actions.Contains("select non-enemy slot 3", StringComparer.OrdinalIgnoreCase), "Pending non-enemy slot should not reopen without current selected-state evidence.");
        var pendingNonEnemySlot3Request = new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            23,
            GuiSmokePhase.HandleCombat.ToString(),
            "Do not reselect the same non-enemy slot without evidence that it stayed selected.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(1, 32, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            pendingNonEnemySlot3Observer,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            pendingNonEnemySlot3Knowledge,
            pendingNonEnemySlot3Actions,
            pendingNonEnemySlot3History,
            "Do not reissue the same non-enemy slot without selected-state evidence.",
            null);
        var pendingNonEnemySlot3Decision = AutoDecisionProvider.Decide(pendingNonEnemySlot3Request);
        Assert(!string.Equals(pendingNonEnemySlot3Decision.TargetLabel, "combat select non-enemy slot 3", StringComparison.OrdinalIgnoreCase), "Pending non-enemy slot regression should not reissue slot 3 without selected-state evidence.");

        var recentRetriedNonEnemySlot3History = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 2", DateTimeOffset.UtcNow.AddSeconds(-4)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 3", DateTimeOffset.UtcNow.AddSeconds(-3)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "auto-end turn", DateTimeOffset.UtcNow.AddSeconds(-2)),
        };
        var recentRetriedNonEnemySlot3Actions = BuildAllowedActions(
            GuiSmokePhase.HandleCombat,
            new ObserverState(pendingNonEnemySlot3Observer, null, null, null),
            pendingNonEnemySlot3Knowledge,
            combatNoOpScreenshotPath,
            recentRetriedNonEnemySlot3History);
        Assert(!recentRetriedNonEnemySlot3Actions.Contains("select non-enemy slot 3", StringComparer.OrdinalIgnoreCase), "Recently retried non-enemy slot should stay closed without current selected-state evidence.");
        var recentRetriedNonEnemySlot3Request = new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            24,
            GuiSmokePhase.HandleCombat.ToString(),
            "Do not reopen a recently retried non-enemy slot without selected-state evidence.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(1, 32, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            pendingNonEnemySlot3Observer,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            pendingNonEnemySlot3Knowledge,
            recentRetriedNonEnemySlot3Actions,
            recentRetriedNonEnemySlot3History,
            "Do not reissue a recently retried non-enemy lane without selected-state evidence.",
            null);
        var recentRetriedNonEnemySlot3Decision = AutoDecisionProvider.Decide(recentRetriedNonEnemySlot3Request);
        Assert(!string.Equals(recentRetriedNonEnemySlot3Decision.TargetLabel, "combat select non-enemy slot 3", StringComparison.OrdinalIgnoreCase), "Recently retried non-enemy regression should not reissue slot 3 without selected-state evidence.");

        var runtimePendingNonEnemyObserver = pendingNonEnemySlot3Observer with
        {
            InventoryId = "inv-runtime-pending-non-enemy",
            SceneEpisodeId = "episode-runtime-pending-non-enemy",
            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                ["combatCardPlayPending"] = "true",
                ["combatPlayMode"] = "Combat",
                ["combatSelectedCardSlot"] = "3",
                ["combatSelectedCardType"] = "Skill",
                ["combatSelectedCardTargetType"] = "Self",
                ["combatTargetingInProgress"] = "false",
                ["combatLastCardPlayFinishedCardId"] = "CARD.STRIKE_IRONCLAD",
            },
        };
        var runtimePendingNonEnemyActions = BuildAllowedActions(
            GuiSmokePhase.HandleCombat,
            new ObserverState(runtimePendingNonEnemyObserver, null, null, null),
            pendingNonEnemySlot3Knowledge,
            combatNoOpScreenshotPath,
            pendingNonEnemySlot3History);
        Assert(runtimePendingNonEnemyActions.Contains("confirm selected non-enemy card", StringComparer.OrdinalIgnoreCase), "Runtime pending non-enemy state should export an explicit confirm action.");
        Assert(!runtimePendingNonEnemyActions.Contains("select non-enemy slot 3", StringComparer.OrdinalIgnoreCase), "Runtime pending non-enemy state should keep the same slot closed until confirm resolves.");
        var runtimePendingNonEnemyDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            25,
            GuiSmokePhase.HandleCombat.ToString(),
            "Runtime pending non-enemy state should confirm even without screenshot-selected evidence.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(1, 32, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            runtimePendingNonEnemyObserver,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            pendingNonEnemySlot3Knowledge,
            runtimePendingNonEnemyActions,
            pendingNonEnemySlot3History,
            "Trust runtime pending state before screenshot heuristics.",
            null));
        Assert(string.Equals(runtimePendingNonEnemyDecision.TargetLabel, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase), "Runtime pending non-enemy state should drive confirm instead of reopening the same hotkey lane.");
        Assert(string.Equals(runtimePendingNonEnemyDecision.ActionKind, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase), "Runtime pending non-enemy state should use the dedicated confirm actuator.");
        Assert(CombatDecisionContract.IsAllowed(
                new GuiSmokeStepRequest(
                    "run",
                    "boot-to-long-run",
                    25,
                    GuiSmokePhase.HandleCombat.ToString(),
                    "Confirm pending non-enemy selection.",
                    DateTimeOffset.UtcNow,
                    combatNoOpScreenshotPath,
                    new WindowBounds(1, 32, 1280, 720),
                    "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                    "0001",
                    1,
                    3,
                    false,
                    "tactical",
                    null,
                    runtimePendingNonEnemyObserver,
                    Array.Empty<KnownRecipeHint>(),
                    Array.Empty<EventKnowledgeCandidate>(),
                    pendingNonEnemySlot3Knowledge,
                    new[] { "confirm selected non-enemy card", "click end turn", "wait" },
                    pendingNonEnemySlot3History,
                    "Confirm pending non-enemy selection.",
                    null),
                new GuiSmokeStepDecision("act", "confirm-non-enemy", null, null, null, "confirm selected non-enemy card", "test", 0.5, "combat", 0, true, null),
                out var confirmSemanticAction)
            && string.Equals(confirmSemanticAction, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase),
            "Dedicated non-enemy confirm action should map to explicit confirm semantics.");

        var staleConfirmAfterObserver = runtimePendingNonEnemyObserver with
        {
            InventoryId = "inv-runtime-pending-non-enemy-stale-clear",
            SceneEpisodeId = "episode-runtime-pending-non-enemy-stale-clear",
            Meta = new Dictionary<string, string?>(runtimePendingNonEnemyObserver.Meta, StringComparer.OrdinalIgnoreCase)
            {
                ["combatCardPlayPending"] = "false",
                ["combatSelectedCardSlot"] = null,
                ["combatLastCardPlayFinishedCardId"] = "CARD.STRIKE_IRONCLAD",
            },
        };
        var staleConfirmProgress = EvaluateStepProgress(
            25,
            GuiSmokePhase.HandleCombat,
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            new ObserverState(runtimePendingNonEnemyObserver, null, null, null),
            new ObserverState(staleConfirmAfterObserver, null, null, null),
            new GuiSmokeStepDecision("act", "confirm-non-enemy", null, null, null, "confirm selected non-enemy card", "test", 0.5, "combat", 0, true, null),
            false,
            "tactical",
            false,
            0);
        Assert(!staleConfirmProgress.ActuatorSignals.Contains("non-enemy-confirmed", StringComparer.OrdinalIgnoreCase), "Stale pending clear without energy, hand, or finished-card delta should not count as non-enemy confirm.");

        var consumedConfirmAfterObserver = runtimePendingNonEnemyObserver with
        {
            InventoryId = "inv-runtime-pending-non-enemy-consumed",
            SceneEpisodeId = "episode-runtime-pending-non-enemy-consumed",
            PlayerEnergy = 1,
            CombatHand = new[]
            {
                new ObservedCombatHandCard(1, "CARD.POOR_SLEEP", "Curse", null),
                new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", null),
            },
            Meta = new Dictionary<string, string?>(runtimePendingNonEnemyObserver.Meta, StringComparer.OrdinalIgnoreCase)
            {
                ["combatCardPlayPending"] = "false",
                ["combatSelectedCardSlot"] = null,
                ["combatLastCardPlayFinishedCardId"] = "CARD.DEFEND_IRONCLAD",
            },
        };
        var consumedConfirmProgress = EvaluateStepProgress(
            26,
            GuiSmokePhase.HandleCombat,
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            new ObserverState(runtimePendingNonEnemyObserver, null, null, null),
            new ObserverState(consumedConfirmAfterObserver, null, null, null),
            new GuiSmokeStepDecision("act", "confirm-non-enemy", null, null, null, "confirm selected non-enemy card", "test", 0.5, "combat", 0, true, null),
            false,
            "tactical",
            false,
            0);
        Assert(consumedConfirmProgress.ActuatorSignals.Contains("non-enemy-confirmed", StringComparer.OrdinalIgnoreCase), "Energy, hand, or finished-card delta on the confirm step should count as non-enemy confirm.");

        var runtimeStateOnlyScreenshotPath = Path.Combine(Path.GetTempPath(), "sts2-runtime-state-only-self-test-missing.png");
        var runtimeAttackSelectionObserver = pendingNonEnemySlot3Observer with
        {
            InventoryId = "inv-runtime-attack-selection",
            SceneEpisodeId = "episode-runtime-attack-selection",
            CombatHand = new[]
            {
                new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", 1),
            },
            ActionNodes = Array.Empty<ObserverActionNode>(),
            Choices = Array.Empty<ObserverChoice>(),
            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                ["combatCardPlayPending"] = "true",
                ["combatPlayMode"] = "Combat",
                ["combatSelectedCardSlot"] = "2",
                ["combatSelectedCardType"] = "Attack",
                ["combatSelectedCardTargetType"] = "AnyEnemy",
                ["combatTargetingInProgress"] = "false",
            },
        };
        var runtimeTargetingKnowledge = new[]
        {
            new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
        };
        var runtimeAttackSelectionHistory = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-1)),
        };
        var runtimeAttackSelectionActions = BuildAllowedActions(
            GuiSmokePhase.HandleCombat,
            new ObserverState(runtimeAttackSelectionObserver, null, null, null),
            runtimeTargetingKnowledge,
            runtimeStateOnlyScreenshotPath,
            runtimeAttackSelectionHistory);
        Assert(!runtimeAttackSelectionActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Runtime attack selection without targeting evidence should keep click-enemy closed.");
        var runtimeAttackSelectionDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            26,
            GuiSmokePhase.HandleCombat.ToString(),
            "Do not treat runtime attack selection alone as target-ready.",
            DateTimeOffset.UtcNow,
            runtimeStateOnlyScreenshotPath,
            new WindowBounds(1, 32, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            runtimeAttackSelectionObserver,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            runtimeTargetingKnowledge,
            runtimeAttackSelectionActions,
            runtimeAttackSelectionHistory,
            "Keep selection and targeting separate when runtime only shows a selected attack.",
            null));
        Assert(runtimeAttackSelectionDecision.TargetLabel?.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase) != true
               && runtimeAttackSelectionDecision.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) != true,
            "Runtime attack selection without targeting evidence should not drive an enemy-target click.");

        var runtimeTargetingObserver = runtimeAttackSelectionObserver with
        {
            InventoryId = "inv-runtime-targeting",
            SceneEpisodeId = "episode-runtime-targeting",
            Meta = new Dictionary<string, string?>(runtimeAttackSelectionObserver.Meta, StringComparer.OrdinalIgnoreCase)
            {
                ["combatTargetingInProgress"] = "true",
            },
        };
        var runtimeTargetingActions = BuildAllowedActions(
            GuiSmokePhase.HandleCombat,
            new ObserverState(runtimeTargetingObserver, null, null, null),
            runtimeTargetingKnowledge,
            runtimeStateOnlyScreenshotPath,
            runtimeAttackSelectionHistory);
        Assert(runtimeTargetingActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Runtime targeting state should open click-enemy even when screenshot arrow evidence is absent.");
        var runtimeTargetingDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            27,
            GuiSmokePhase.HandleCombat.ToString(),
            "Runtime targeting state should drive an actual enemy-target lane.",
            DateTimeOffset.UtcNow,
            runtimeStateOnlyScreenshotPath,
            new WindowBounds(1, 32, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            runtimeTargetingObserver,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            runtimeTargetingKnowledge,
            runtimeTargetingActions,
            runtimeAttackSelectionHistory,
            "Use runtime targeting metadata before screenshot heuristics.",
            null));
        Assert(runtimeTargetingDecision.TargetLabel?.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase) == true
               || runtimeTargetingDecision.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) == true,
            "Runtime targeting metadata should drive an enemy-target decision, not reopen card selection.");

        var stalePendingAttackHistory = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-2)),
        };
        var runtimeFinishedSelectionObserver = pendingNonEnemySlot3Observer with
        {
            InventoryId = "inv-runtime-finished-selection",
            SceneEpisodeId = "episode-runtime-finished-selection",
            ActionNodes = Array.Empty<ObserverActionNode>(),
            Choices = Array.Empty<ObserverChoice>(),
            CombatHand = new[]
            {
                new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", 1),
            },
            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                ["combatCardPlayPending"] = "false",
                ["combatTargetingInProgress"] = "false",
                ["combatLastCardPlayFinishedCardId"] = "CARD.STRIKE_IRONCLAD",
            },
        };
        var runtimeFinishedSelectionKnowledge = new[]
        {
            new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
        };
        Assert(CombatRuntimeStateSupport.ResolvePendingSelection(
                runtimeFinishedSelectionObserver,
                runtimeFinishedSelectionKnowledge,
                CombatHistorySupport.TryGetPendingCombatSelection(stalePendingAttackHistory)) is null,
            "Runtime finished-card state should clear stale pending combat selection carryover.");
        var runtimeFinishedSelectionActions = BuildAllowedActions(
            GuiSmokePhase.HandleCombat,
            new ObserverState(runtimeFinishedSelectionObserver, null, null, null),
            runtimeFinishedSelectionKnowledge,
            runtimeStateOnlyScreenshotPath,
            stalePendingAttackHistory);
        Assert(!runtimeFinishedSelectionActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Runtime cleared selection should keep stale enemy-target actions closed.");
        var runtimeFinishedSelectionDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            28,
            GuiSmokePhase.HandleCombat.ToString(),
            "Runtime finished-card state should clear stale pending attack history before choosing the next lane.",
            DateTimeOffset.UtcNow,
            runtimeStateOnlyScreenshotPath,
            new WindowBounds(1, 32, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            runtimeFinishedSelectionObserver,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            runtimeFinishedSelectionKnowledge,
            runtimeFinishedSelectionActions,
            stalePendingAttackHistory,
            "Clear stale runtime attack carryover before selecting the next legal lane.",
            null));
        Assert(string.Equals(runtimeFinishedSelectionDecision.TargetLabel, "combat select non-enemy slot 2", StringComparison.OrdinalIgnoreCase), "Runtime cleared selection should drive a fresh non-enemy selection instead of stale enemy-target carryover.");

        var parityCombatObserver = new ObserverSummary(
            "combat",
            "combat",
            true,
            DateTimeOffset.UtcNow,
            "inv-handle-combat-parity",
            true,
            "mixed",
            "stable",
            "episode-handle-combat-parity",
            "Monster",
            "combat-targets",
            73,
            80,
            3,
            new[] { "압축벌레" },
            Array.Empty<string>(),
            new[]
            {
                new ObserverActionNode("enemy-target:2", "enemy-target", "압축벌레", "1398.24,621.6,83.52,88", true),
            },
            new[]
            {
                new ObserverChoice("enemy-target", "압축벌레", "1398.24,621.6,83.52,88", "압축벌레", "target-source:vfx-spawn-hitbox")
                {
                    NodeId = "enemy-target:2",
                },
            },
            new[]
            {
                new ObservedCombatHandCard(1, "CARD.POOR_SLEEP", "Curse", null),
                new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", null),
                new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", null),
                new ObservedCombatHandCard(4, "CARD.STRIKE_IRONCLAD", "Attack", null),
            })
        {
            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
            },
        };
        var parityCombatKnowledge = new[]
        {
            new CombatCardKnowledgeHint(1, "CARD.POOR_SLEEP", "Curse", "None", -1, "self-test"),
            new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
        };
        var parityFullCombatHistory = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target 압축벌레", DateTimeOffset.UtcNow.AddSeconds(-12)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "auto-end turn", DateTimeOffset.UtcNow.AddSeconds(-11)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "wait", null, DateTimeOffset.UtcNow.AddSeconds(-10)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-9)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 4", DateTimeOffset.UtcNow.AddSeconds(-8)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-7)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 4", DateTimeOffset.UtcNow.AddSeconds(-6)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-5)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 2", DateTimeOffset.UtcNow.AddSeconds(-4)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-3)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 2", DateTimeOffset.UtcNow.AddSeconds(-2)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 2", DateTimeOffset.UtcNow.AddSeconds(-1)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 3", DateTimeOffset.UtcNow),
        };
        var paritySerializedHistory = BuildSerializedStepHistory(GuiSmokePhase.HandleCombat, parityFullCombatHistory);
        Assert(paritySerializedHistory.Count == HandleCombatContextSupport.SerializedHistoryWindow, "HandleCombat request serialization should retain the last 12 combat entries for parity rebuilds.");
        Assert(paritySerializedHistory.All(entry => string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)), "HandleCombat request serialization should only persist combat-phase history.");
        var parityObserverState = new ObserverState(parityCombatObserver, null, null, null);
        var parityAllowedActions = BuildAllowedActions(
            GuiSmokePhase.HandleCombat,
            parityObserverState,
            parityCombatKnowledge,
            combatNoOpScreenshotPath,
            paritySerializedHistory);
        Assert(!parityAllowedActions.Contains("select attack slot 2", StringComparer.OrdinalIgnoreCase), "Step19-like parity regression should keep blocked attack slot 2 closed.");
        Assert(!parityAllowedActions.Contains("select attack slot 4", StringComparer.OrdinalIgnoreCase), "Step19-like parity regression should keep blocked attack slot 4 closed.");
        var parityRequest = new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            19,
            GuiSmokePhase.HandleCombat.ToString(),
            "Preserve enough combat history for live/rebuild parity.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(1, 32, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0002",
            2,
            3,
            false,
            "tactical",
            null,
            parityCombatObserver,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            parityCombatKnowledge,
            parityAllowedActions,
            paritySerializedHistory,
            BuildFailureModeHintCore(
                GuiSmokePhase.HandleCombat,
                parityObserverState,
                parityCombatKnowledge,
                combatNoOpScreenshotPath,
                paritySerializedHistory),
            null);
        File.WriteAllText(handleCombatParityRequestPath, JsonSerializer.Serialize(parityRequest, GuiSmokeShared.JsonOptions), Encoding.UTF8);
        var parityNonRebuild = LoadReplayRequest(handleCombatParityRequestPath, fullRequestRebuild: false).Request;
        var parityRebuilt = LoadReplayRequest(handleCombatParityRequestPath, fullRequestRebuild: true).Request;
        Assert(parityNonRebuild.AllowedActions.OrderBy(static action => action, StringComparer.OrdinalIgnoreCase)
               .SequenceEqual(parityRebuilt.AllowedActions.OrderBy(static action => action, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase),
            "Saved-vs-rebuilt HandleCombat parity should preserve the same allowlist on the synthetic step19-like request.");
        var parityNonRebuildDecision = EvaluateAutoDecisionWithDiagnostics(handleCombatParityRequestPath, parityNonRebuild).Decision;
        var parityRebuiltDecision = EvaluateAutoDecisionWithDiagnostics(handleCombatParityRequestPath, parityRebuilt).Decision;
        Assert(CombatDecisionContract.TryMapSemanticAction(parityNonRebuild, parityNonRebuildDecision, out var parityNonRebuildSemantic), "Saved synthetic HandleCombat parity request should map to a combat semantic action.");
        Assert(CombatDecisionContract.TryMapSemanticAction(parityRebuilt, parityRebuiltDecision, out var parityRebuiltSemantic), "Rebuilt synthetic HandleCombat parity request should map to a combat semantic action.");
        Assert(string.Equals(parityNonRebuildSemantic, parityRebuiltSemantic, StringComparison.OrdinalIgnoreCase), "Step19-like synthetic parity regression should keep saved and rebuilt final semantics aligned.");
        Assert(string.Equals(parityNonRebuildSemantic, "click end turn", StringComparison.OrdinalIgnoreCase), "Step19-like synthetic parity regression should still close on end turn.");
        Assert(!string.Equals(parityRebuiltDecision.TargetLabel, "combat select attack slot 2", StringComparison.OrdinalIgnoreCase), "Step19-like synthetic parity regression should not rebuild to illegal attack slot 2.");

        var slotAlignmentObserver = new ObserverState(
            new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-slot-alignment",
                true,
                "mixed",
                "stable",
                "episode-slot-alignment",
                "Combat",
                "combat",
                80,
                80,
                1,
                new[] { "3턴 종료" },
                Array.Empty<string>(),
                new[] { new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true) },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                }),
            null,
            null,
            null);
        var slotAlignmentKnowledge = new[]
        {
            new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
        };
        var slotAllowedActions = BuildAllowedActions(GuiSmokePhase.HandleCombat, slotAlignmentObserver, slotAlignmentKnowledge, combatNoOpScreenshotPath, Array.Empty<GuiSmokeHistoryEntry>());
        Assert(slotAllowedActions.Contains("select attack slot 2", StringComparer.OrdinalIgnoreCase), "Combat allowlist should expose the actual playable attack slot from observer/knowledge alignment.");
        Assert(!slotAllowedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Combat allowlist should not expose enemy targeting before an actual attack selection is confirmed.");
        var slotAlignmentDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            8,
            GuiSmokePhase.HandleCombat.ToString(),
            "Choose a combat action from the aligned slot map.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(0, 0, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            slotAlignmentObserver.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            slotAlignmentKnowledge,
            slotAllowedActions,
            Array.Empty<GuiSmokeHistoryEntry>(),
            "Only slot 2 is a playable attack.",
            null));
        Assert(string.Equals(slotAlignmentDecision.TargetLabel, "combat select attack slot 2", StringComparison.OrdinalIgnoreCase), "Combat decisioning should align with observer/knowledge slot 2 instead of drifting to a screenshot-only slot.");

        var combatTargetObserver = new ObserverState(
            new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-combat-targets",
                true,
                "mixed",
                "stable",
                "episode-combat-targets",
                "Combat",
                "combat",
                80,
                80,
                3,
                new[] { "Jaw Worm", "Cultist" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("enemy-target:1", "enemy-target", "Jaw Worm", "720,180,180,260", true),
                    new ObserverActionNode("enemy-target:2", "enemy-target", "Cultist", "930,210,180,250", true),
                    new ObserverActionNode("end-turn", "button", "3턴 종료", "1080,620,140,60", true),
                },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(1, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                }),
            null,
            null,
            null);
        var combatTargetDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            9,
            GuiSmokePhase.HandleCombat.ToString(),
            "Prefer actual combat target nodes over fixed normalized enemy anchors.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(0, 0, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            combatTargetObserver.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            },
            new[] { "click card", "click enemy", "click end turn", "wait" },
            new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-2)),
            },
            "Use current-frame enemy target bounds instead of fixed anchors.",
            null));
        Assert(combatTargetDecision.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) == true, "Combat target selection should use exported enemy target nodes instead of fixed normalized labels when current-frame enemy bounds exist.");
        Assert(combatTargetDecision.NormalizedX is > 0.35 and < 0.55, "Enemy target click should resolve from the exported hitbox/body rect, not the old fixed normalized anchor.");

        var combatTargetRetryDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            10,
            GuiSmokePhase.HandleCombat.ToString(),
            "After one no-op target click, try another observed enemy target before giving up.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(0, 0, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            combatTargetObserver.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            },
            new[] { "click card", "click enemy", "click end turn", "wait" },
            new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-5)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target Jaw Worm", DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 1", DateTimeOffset.UtcNow.AddSeconds(-3)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-2)),
            },
            "Try another enemy target before ending the turn.",
            null));
        Assert(combatTargetRetryDecision.TargetLabel?.Contains("Cultist", StringComparison.OrdinalIgnoreCase) == true, "After one no-op enemy click, combat recovery should try another observed enemy target when one is available.");

        var noOpFallbackOrderingObserver = new ObserverState(
            new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-noop-fallback-ordering",
                true,
                "mixed",
                "stable",
                "episode-noop-fallback-ordering",
                "Combat",
                "combat",
                55,
                80,
                2,
                new[] { "Jaw Worm", "3턴 종료" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("enemy-target:1", "enemy-target", "Jaw Worm", "720,180,180,260", true),
                    new ObserverActionNode("end-turn", "button", "3턴 종료", "1080,620,140,60", true),
                },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(1, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                }),
            null,
            null,
            null);
        var noOpFallbackOrderingDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            11,
            GuiSmokePhase.HandleCombat.ToString(),
            "Do not end turn while a safe non-enemy play remains after an attack lane no-op.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(0, 0, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            noOpFallbackOrderingObserver.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            },
            new[] { "click card", "click enemy", "click end turn", "wait" },
            new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-6)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target Jaw Worm", DateTimeOffset.UtcNow.AddSeconds(-5)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 1", DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-3)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target Jaw Worm", DateTimeOffset.UtcNow.AddSeconds(-2)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 1", DateTimeOffset.UtcNow.AddSeconds(-1)),
            },
            "Prefer a remaining safe non-enemy play before ending the turn.",
            null));
        Assert(CombatDecisionContract.TryMapSemanticAction(
                new GuiSmokeStepRequest(
                    "run",
                    "boot-to-long-run",
                    11,
                    GuiSmokePhase.HandleCombat.ToString(),
                    "Do not end turn while a safe non-enemy play remains after an attack lane no-op.",
                    DateTimeOffset.UtcNow,
                    combatNoOpScreenshotPath,
                    new WindowBounds(0, 0, 1280, 720),
                    "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                    "0001",
                    1,
                    3,
                    false,
                    "tactical",
                    null,
                    noOpFallbackOrderingObserver.Summary,
                    Array.Empty<KnownRecipeHint>(),
                    Array.Empty<EventKnowledgeCandidate>(),
                    new[]
                    {
                        new CombatCardKnowledgeHint(1, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                        new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                    },
                    new[] { "click card", "click enemy", "click end turn", "wait" },
                    new[]
                    {
                        new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-6)),
                        new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target Jaw Worm", DateTimeOffset.UtcNow.AddSeconds(-5)),
                        new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 1", DateTimeOffset.UtcNow.AddSeconds(-4)),
                        new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-3)),
                        new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target Jaw Worm", DateTimeOffset.UtcNow.AddSeconds(-2)),
                        new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 1", DateTimeOffset.UtcNow.AddSeconds(-1)),
                    },
                    "Prefer a remaining safe non-enemy play before ending the turn.",
                    null),
                noOpFallbackOrderingDecision,
                out var noOpFallbackOrderingSemantic)
            && !string.Equals(noOpFallbackOrderingSemantic, "click end turn", StringComparison.OrdinalIgnoreCase),
            "Combat decisioning should not fall straight to end turn while another safe combat fallback still exists after a blocked attack lane.");

        var repeatedAttackOrderingObserver = new ObserverState(
            new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-repeated-attack-ordering",
                true,
                "mixed",
                "stable",
                "episode-repeated-attack-ordering",
                "Combat",
                "combat",
                55,
                80,
                2,
                new[] { "3턴 종료" },
                Array.Empty<string>(),
                new[] { new ObserverActionNode("end-turn", "button", "3턴 종료", "1080,620,140,60", true) },
                Array.Empty<ObserverChoice>(),
                Array.Empty<ObservedCombatHandCard>()),
            null,
            null,
            null);
        var repeatedAttackOrderingDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            12,
            GuiSmokePhase.HandleCombat.ToString(),
            "Do not end turn before replaying a knowledge-backed attack slot.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(0, 0, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            repeatedAttackOrderingObserver.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            new[]
            {
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            },
            new[] { "click card", "click end turn", "wait" },
            new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-3)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-2)),
            },
            "Prefer a known playable attack before ending the turn.",
            null));
        Assert(string.Equals(repeatedAttackOrderingDecision.TargetLabel, "combat select attack slot 2", StringComparison.OrdinalIgnoreCase), "Repeated attack-select loop handling should still try a knowledge-backed playable attack before end turn.");

        var noEnemyTargetObserver = new ObserverState(
            new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-no-enemy-target",
                true,
                "mixed",
                "stable",
                "episode-no-enemy-target",
                "Combat",
                "combat",
                65,
                80,
                1,
                new[] { "4턴 종료" },
                Array.Empty<string>(),
                new[] { new ObserverActionNode("end-turn", "button", "4턴 종료", "1604,846,220,90", true) },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                }),
            null,
            null,
            null);
        var noEnemyTargetHistory = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-5)),
        };
        var noEnemyAllowedActions = BuildAllowedActions(GuiSmokePhase.HandleCombat, noEnemyTargetObserver, slotAlignmentKnowledge[..2], combatNoOpScreenshotPath, noEnemyTargetHistory);
        Assert(!noEnemyAllowedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Combat allowlist should keep enemy targeting closed when no playable attack remains in the current observer hand.");
        var noEnemyDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            27,
            GuiSmokePhase.HandleCombat.ToString(),
            "Do not overstate an enemy target when the hand is all defend.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(0, 0, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            noEnemyTargetObserver.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            slotAlignmentKnowledge[..2],
            noEnemyAllowedActions,
            noEnemyTargetHistory,
            "Do not click the enemy without a selected attack.",
            null));
        Assert(!string.Equals(noEnemyDecision.TargetLabel, "auto-target enemy", StringComparison.OrdinalIgnoreCase), "Combat decisioning should not emit auto-target enemy when the observer hand no longer contains an attack.");

        var unchangedCombatProbeObserver = new ObserverState(
            new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-combat-probe-grace",
                true,
                "mixed",
                "stable",
                "episode-combat-probe-grace",
                "Combat",
                "combat",
                60,
                80,
                3,
                new[] { "Jaw Worm" },
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                Array.Empty<ObserverChoice>(),
                new[] { new ObservedCombatHandCard(4, "CARD.STRIKE_IRONCLAD", "Attack", 1) }),
            null,
            null,
            null);
        Assert(ShouldGrantCombatNoOpProbeGrace(
            GuiSmokePhase.HandleCombat,
            unchangedCombatProbeObserver,
            unchangedCombatProbeObserver,
            new GuiSmokeStepDecision("act", "press-key", "4", null, null, "combat select attack slot 4", "probe grace", 0.5, "combat", 250, true, null)),
            "Combat no-op-sensitive actions should allow one grace resample before a same-frame no-op is recorded.");
        Assert(!ShouldGrantCombatNoOpProbeGrace(
            GuiSmokePhase.HandleCombat,
            unchangedCombatProbeObserver,
            new ObserverState(
                unchangedCombatProbeObserver.Summary with
                {
                    CapturedAt = DateTimeOffset.UtcNow.AddMilliseconds(200),
                    PlayerEnergy = 2,
                },
                null,
                null,
                null),
            new GuiSmokeStepDecision("act", "press-key", "E", null, null, "auto-end turn", "no probe grace", 0.5, "combat", 250, true, null)),
            "Combat probe grace should stay closed once a post-action delta is already visible or the action is not no-op-sensitive.");

        var staleBoundsDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            64,
            GuiSmokePhase.HandleCombat.ToString(),
            "Reject stale off-window combat bounds.",
            DateTimeOffset.UtcNow,
            combatNoOpScreenshotPath,
            new WindowBounds(1, 32, 1280, 720),
            "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
            "0002",
            2,
            3,
            false,
            "tactical",
            null,
            new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-stale-bounds",
                true,
                "mixed",
                "stable",
                "episode-stale-bounds",
                "Combat",
                "combat",
                30,
                80,
                0,
                new[] { "1턴 종료" },
                Array.Empty<string>(),
                new[] { new ObserverActionNode("stale-end-turn", "button", "1턴 종료", "1604,846,220,90", true) },
                Array.Empty<ObserverChoice>(),
                Array.Empty<ObservedCombatHandCard>()),
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            new[] { "click end turn", "wait" },
            Array.Empty<GuiSmokeHistoryEntry>(),
            "Reject stale bounds.",
            null));
        Assert(string.Equals(staleBoundsDecision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase)
               && string.Equals(staleBoundsDecision.KeyText, "E", StringComparison.OrdinalIgnoreCase), "Off-window combat action bounds should be hard-rejected so fallback end-turn uses a safe key press instead of stale coordinates.");
    }
    finally
    {
        if (File.Exists(handleCombatParityRequestPath))
        {
            File.Delete(handleCombatParityRequestPath);
        }

        if (File.Exists(combatNoOpScreenshotPath))
        {
            File.Delete(combatNoOpScreenshotPath);
        }
    }

    var colorlessRewardScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-colorless-reward-self-test-{Guid.NewGuid():N}.png");
    try
    {
        using (var bitmap = new Bitmap(1920, 1080))
        using (var graphics = Graphics.FromImage(bitmap))
        using (var brush = new SolidBrush(Color.FromArgb(255, 180, 120)))
        {
            graphics.Clear(Color.Black);
            graphics.FillRectangle(brush, new Rectangle(760, 260, 220, 260));
            bitmap.Save(colorlessRewardScreenshotPath, ImageFormat.Png);
        }

        var colorlessObserver = new ObserverSummary(
            "event",
            "event",
            false,
            DateTimeOffset.UtcNow,
            "inv-colorless",
            true,
            "mixed",
            "stable",
            "episode-colorless",
            null,
            "generic",
            80,
            80,
            null,
            new[] { "Skip", "불타는 혈액", "납 문진", "고정시키기", "주먹다짐" },
            Array.Empty<string>(),
            new[]
            {
                new ObserverActionNode("event-option:0", "event-option", "Skip", "827,862,276,73", true),
                new ObserverActionNode("event-option:1", "event-option", "불타는 혈액", "12,82,68,68", true),
                new ObserverActionNode("event-option:2", "event-option", "납 문진", "80,82,68,68", true),
            },
            new[]
            {
                new ObserverChoice("card", "Skip", "827,862,276,73"),
                new ObserverChoice("relic", "불타는 혈액", "12,82,68,68", "RELIC.BURNING_BLOOD"),
                new ObserverChoice("relic", "납 문진", "80,82,68,68", "RELIC.LEAD_PAPERWEIGHT"),
                new ObserverChoice("card", "고정시키기", null, "CARD.FASTEN"),
                new ObserverChoice("card", "주먹다짐", null, "CARD.FISTICUFFS"),
            },
            Array.Empty<ObservedCombatHandCard>());
        var colorlessObserverState = new ObserverState(colorlessObserver, null, null, null);
        Assert(GetAllowedActions(GuiSmokePhase.HandleEvent, colorlessObserverState).Contains("click colorless card choice", StringComparer.OrdinalIgnoreCase), "Colorless reward state should expose colorless card actions instead of generic event options.");
        var colorlessDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            6,
            GuiSmokePhase.HandleEvent.ToString(),
            "Resolve the event reward follow-up.",
            DateTimeOffset.UtcNow,
            colorlessRewardScreenshotPath,
            new WindowBounds(0, 0, 1920, 1080),
            "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:reward-choice|substate:colorless-card-choice",
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            colorlessObserver,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            GetAllowedActions(GuiSmokePhase.HandleEvent, colorlessObserverState),
            Array.Empty<GuiSmokeHistoryEntry>(),
            "prefer real card choices over inspect affordances",
            null));
        Assert(!string.Equals(colorlessDecision.Status, "wait", StringComparison.OrdinalIgnoreCase),
            "Reward/card substate should emit an action instead of stalling on inspect affordances.");

        var overlayDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            7,
            GuiSmokePhase.HandleEvent.ToString(),
            "Dismiss the inspect overlay before progressing.",
            DateTimeOffset.UtcNow,
            colorlessRewardScreenshotPath,
            new WindowBounds(0, 0, 1920, 1080),
            "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:inspect-overlay|substate:reward-choice",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            colorlessObserver with
            {
                CurrentChoices = new[] { "Backstop", "LeftArrow", "RightArrow", "Skip", "불타는 혈액", "납 문진", "고정시키기", "주먹다짐" },
                ActionNodes = new[]
                {
                    new ObserverActionNode("event-option:0", "event-option", "Backstop", "-192,-108,2304,1296", true),
                    new ObserverActionNode("event-option:1", "event-option", "LeftArrow", "472,476,128,128", true),
                    new ObserverActionNode("event-option:2", "event-option", "RightArrow", "1320,476,128,128", true),
                    new ObserverActionNode("event-option:3", "event-option", "Skip", "827,862,276,73", true),
                },
                Choices = new[]
                {
                    new ObserverChoice("choice", "Backstop", "-192,-108,2304,1296"),
                    new ObserverChoice("choice", "LeftArrow", "472,476,128,128"),
                    new ObserverChoice("choice", "RightArrow", "1320,476,128,128"),
                    new ObserverChoice("card", "Skip", "827,862,276,73"),
                    new ObserverChoice("relic", "불타는 혈액", "12,82,68,68", "RELIC.BURNING_BLOOD"),
                    new ObserverChoice("relic", "납 문진", "80,82,68,68", "RELIC.LEAD_PAPERWEIGHT"),
                    new ObserverChoice("card", "고정시키기", null, "CARD.FASTEN"),
                    new ObserverChoice("card", "주먹다짐", null, "CARD.FISTICUFFS"),
                },
            },
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            new[] { "press escape", "click inspect overlay close", "wait" },
            new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleEvent.ToString(), "click", "overlay backdrop close", DateTimeOffset.UtcNow.AddSeconds(-3)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleEvent.ToString(), "click", "overlay backdrop close", DateTimeOffset.UtcNow.AddSeconds(-1)),
            },
            "overlay recovery required",
            null));
        Assert(string.Equals(overlayDecision.TargetLabel, "inspect overlay escape", StringComparison.OrdinalIgnoreCase), "Repeated inspect overlay closes should escalate to escape instead of repeating the same backdrop click.");
    }
    finally
    {
        if (File.Exists(colorlessRewardScreenshotPath))
        {
            File.Delete(colorlessRewardScreenshotPath);
        }
    }

    var manualCleanBootRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-clean-boot-self-test-{Guid.NewGuid():N}");
    try
    {
        Directory.CreateDirectory(manualCleanBootRoot);
        var harnessRoot = Path.Combine(manualCleanBootRoot, "harness");
        var inboxRoot = Path.Combine(harnessRoot, "inbox");
        var outboxRoot = Path.Combine(harnessRoot, "outbox");
        Directory.CreateDirectory(inboxRoot);
        Directory.CreateDirectory(outboxRoot);
        var harnessLayout = new HarnessQueueLayout(
            manualCleanBootRoot,
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
            harnessLayout.StatusPath,
            JsonSerializer.Serialize(
                new HarnessBridgeStatus(true, "active", null, null, DateTimeOffset.UtcNow, "warming up", null, null),
                GuiSmokeShared.JsonOptions));
        File.WriteAllText(
            harnessLayout.InventoryPath,
            JsonSerializer.Serialize(
                new HarnessNodeInventory(
                    "inventory-clean-boot",
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
        File.WriteAllText(harnessLayout.ActionsPath, "{}" + Environment.NewLine);

        LongRunArtifacts.InitializeSessionArtifacts(manualCleanBootRoot, "clean-boot-session", "boot-to-long-run", "headless");
        var screenshotPath = Path.Combine(manualCleanBootRoot, "main-menu.png");
        var observerPath = Path.Combine(manualCleanBootRoot, "main-menu.observer.json");
        var manualCleanBootFreshnessFloor = DateTimeOffset.UtcNow.AddSeconds(-5);
        File.WriteAllBytes(screenshotPath, Array.Empty<byte>());
        File.WriteAllText(observerPath, "{}");
        Assert(
            LongRunArtifacts.TryMarkManualCleanBootVerified(
                manualCleanBootRoot,
                harnessLayout,
                new ObserverState(
                    new ObserverSummary(
                        "main-menu",
                        "main-menu",
                        false,
                        DateTimeOffset.UtcNow,
                        "inv-main-menu",
                        true,
                        "mixed",
                        "stable",
                        "episode-main-menu",
                        null,
                        "main-menu",
                        null,
                        null,
                        null,
                        new[] { "Singleplayer" },
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
                manualCleanBootFreshnessFloor),
            "Manual clean boot should verify on a clean main-menu first step even when status mode is transiently non-dormant.");
        var cleanBootPrevalidation = JsonSerializer.Deserialize<GuiSmokePrevalidation>(File.ReadAllText(Path.Combine(manualCleanBootRoot, "prevalidation.json")), GuiSmokeShared.JsonOptions)
                                  ?? throw new InvalidOperationException("Failed to read manual clean boot prevalidation self-test.");
        Assert(cleanBootPrevalidation.ManualCleanBootVerified, "Manual clean boot prevalidation should be marked valid on the inert stale-actions path.");
        Assert(cleanBootPrevalidation.ManualCleanBootEvidence?.ActionsQueueClear == true, "Inert stale actions should still satisfy the manual clean boot actions gate.");
        Assert(cleanBootPrevalidation.ManualCleanBootEvidence?.EvaluationNotes?.Contains("stale-actions-observed-but-inert", StringComparer.OrdinalIgnoreCase) == true, "Manual clean boot evidence should explain why stale actions were treated as inert.");
    }
    finally
    {
        if (Directory.Exists(manualCleanBootRoot))
        {
            Directory.Delete(manualCleanBootRoot, recursive: true);
        }
    }

    var manualCleanBootBlockedRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-clean-boot-blocked-self-test-{Guid.NewGuid():N}");
    try
    {
        Directory.CreateDirectory(manualCleanBootBlockedRoot);
        var harnessRoot = Path.Combine(manualCleanBootBlockedRoot, "harness");
        var inboxRoot = Path.Combine(harnessRoot, "inbox");
        var outboxRoot = Path.Combine(harnessRoot, "outbox");
        Directory.CreateDirectory(inboxRoot);
        Directory.CreateDirectory(outboxRoot);
        var harnessLayout = new HarnessQueueLayout(
            manualCleanBootBlockedRoot,
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
                    "inventory-clean-boot-blocked",
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
        File.WriteAllText(
            harnessLayout.ArmSessionPath,
            JsonSerializer.Serialize(
                new HarnessArmSession("token", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(5), "self-test"),
                GuiSmokeShared.JsonOptions));

        LongRunArtifacts.InitializeSessionArtifacts(manualCleanBootBlockedRoot, "clean-boot-blocked-session", "boot-to-long-run", "headless");
        var screenshotPath = Path.Combine(manualCleanBootBlockedRoot, "main-menu.png");
        var observerPath = Path.Combine(manualCleanBootBlockedRoot, "main-menu.observer.json");
        var blockedManualCleanBootFreshnessFloor = DateTimeOffset.UtcNow.AddSeconds(-5);
        File.WriteAllBytes(screenshotPath, Array.Empty<byte>());
        File.WriteAllText(observerPath, "{}");
        Assert(
            !LongRunArtifacts.TryMarkManualCleanBootVerified(
                manualCleanBootBlockedRoot,
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
                blockedManualCleanBootFreshnessFloor),
            "Manual clean boot should remain invalid when an external arm session is present.");
        var blockedPrevalidation = JsonSerializer.Deserialize<GuiSmokePrevalidation>(File.ReadAllText(Path.Combine(manualCleanBootBlockedRoot, "prevalidation.json")), GuiSmokeShared.JsonOptions)
                                 ?? throw new InvalidOperationException("Failed to read blocked manual clean boot prevalidation self-test.");
        Assert(blockedPrevalidation.ManualCleanBootEvidence?.BlockingReasons?.Contains("arm-session-present", StringComparer.OrdinalIgnoreCase) == true, "Manual clean boot failure evidence should record the blocking arm session.");
    }
    finally
    {
        if (Directory.Exists(manualCleanBootBlockedRoot))
        {
            Directory.Delete(manualCleanBootBlockedRoot, recursive: true);
        }
    }

    var manualCleanBootBootstrapRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-clean-boot-bootstrap-self-test-{Guid.NewGuid():N}");
    try
    {
        Directory.CreateDirectory(manualCleanBootBootstrapRoot);
        var harnessRoot = Path.Combine(manualCleanBootBootstrapRoot, "harness");
        var inboxRoot = Path.Combine(harnessRoot, "inbox");
        var outboxRoot = Path.Combine(harnessRoot, "outbox");
        var liveRoot = Path.Combine(manualCleanBootBootstrapRoot, "live");
        Directory.CreateDirectory(inboxRoot);
        Directory.CreateDirectory(outboxRoot);
        Directory.CreateDirectory(liveRoot);
        var harnessLayout = new HarnessQueueLayout(
            manualCleanBootBootstrapRoot,
            harnessRoot,
            inboxRoot,
            Path.Combine(inboxRoot, "actions.ndjson"),
            outboxRoot,
            Path.Combine(outboxRoot, "results.ndjson"),
            Path.Combine(harnessRoot, "status.json"),
            Path.Combine(outboxRoot, "trace.ndjson"),
            Path.Combine(harnessRoot, "arm.json"),
            Path.Combine(outboxRoot, "inventory.latest.json"));
        var liveLayout = new LiveExportLayout(
            manualCleanBootBootstrapRoot,
            liveRoot,
            Path.Combine(liveRoot, "events.ndjson"),
            Path.Combine(liveRoot, "state.latest.json"),
            Path.Combine(liveRoot, "state.latest.txt"),
            Path.Combine(liveRoot, "session.json"))
        {
            SemanticSnapshotsRoot = Path.Combine(liveRoot, "semantic-snapshots"),
        };
        var inventoryCapturedAt = DateTimeOffset.UtcNow;
        File.WriteAllText(
            harnessLayout.InventoryPath,
            JsonSerializer.Serialize(
                new HarnessNodeInventory(
                    "inventory-main-menu-only",
                    inventoryCapturedAt,
                    null,
                    "main-menu",
                    "episode-main-menu-only",
                    "dormant",
                    null,
                    true,
                    "mixed",
                    "stable",
                    Array.Empty<HarnessNodeInventoryItem>()),
                GuiSmokeShared.JsonOptions));

        var inventoryOnlyObserver = new ObserverSnapshotReader(liveLayout, harnessLayout).Read();
        Assert(string.Equals(inventoryOnlyObserver.CurrentScreen, "main-menu", StringComparison.OrdinalIgnoreCase), "Observer should fall back to harness inventory sceneType when state.latest.json is not available.");
        Assert(string.Equals(inventoryOnlyObserver.VisibleScreen, "main-menu", StringComparison.OrdinalIgnoreCase), "Observer visibleScreen should fall back to harness inventory sceneType when state.latest.json is not available.");
        Assert(inventoryOnlyObserver.CapturedAt == inventoryCapturedAt, "Observer capturedAt should fall back to harness inventory when state.latest.json is not available.");

        LongRunArtifacts.InitializeSessionArtifacts(manualCleanBootBootstrapRoot, "clean-boot-bootstrap-session", "boot-to-long-run", "headless");
        var screenshotPath = Path.Combine(manualCleanBootBootstrapRoot, "main-menu.png");
        var observerPath = Path.Combine(manualCleanBootBootstrapRoot, "main-menu.observer.json");
        File.WriteAllBytes(screenshotPath, Array.Empty<byte>());
        File.WriteAllText(observerPath, "{}");
        Assert(
            LongRunArtifacts.TryMarkManualCleanBootVerified(
                manualCleanBootBootstrapRoot,
                harnessLayout,
                inventoryOnlyObserver,
                Array.Empty<GuiSmokeHistoryEntry>(),
                screenshotPath,
                observerPath,
                inventoryCapturedAt.AddSeconds(-5)),
            "Manual clean boot should verify from fresh harness inventory main-menu evidence when state.latest.json is not available yet.");

        var bootstrapStates = new[]
        {
            new ObserverState(
                new ObserverSummary(
                    null,
                    null,
                    false,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
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
            new ObserverState(
                new ObserverSummary(
                    "main-menu",
                    "main-menu",
                    false,
                    DateTimeOffset.UtcNow,
                    null,
                    true,
                    "mixed",
                    "stable",
                    "episode-main-menu-bootstrap",
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
        };
        var bootstrapIndex = 0;
        var bootstrappedObserver = BootstrapManualCleanBootObserverAsync(
                bootstrapStates[bootstrapIndex],
                () => bootstrapStates[Math.Min(++bootstrapIndex, bootstrapStates.Length - 1)],
                DateTimeOffset.UtcNow.AddSeconds(-5),
                2,
                0,
                static _ => Task.CompletedTask)
            .GetAwaiter()
            .GetResult();
        Assert(string.Equals(bootstrappedObserver.CurrentScreen, "main-menu", StringComparison.OrdinalIgnoreCase), "Manual clean boot bootstrap should keep polling until a fresh main-menu observer arrives.");
    }
    finally
    {
        if (Directory.Exists(manualCleanBootBootstrapRoot))
        {
            Directory.Delete(manualCleanBootBootstrapRoot, recursive: true);
        }
    }

    var manualCleanBootObserverNotReadyRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-clean-boot-observer-not-ready-self-test-{Guid.NewGuid():N}");
    try
    {
        Directory.CreateDirectory(manualCleanBootObserverNotReadyRoot);
        var harnessRoot = Path.Combine(manualCleanBootObserverNotReadyRoot, "harness");
        var inboxRoot = Path.Combine(harnessRoot, "inbox");
        var outboxRoot = Path.Combine(harnessRoot, "outbox");
        Directory.CreateDirectory(inboxRoot);
        Directory.CreateDirectory(outboxRoot);
        var harnessLayout = new HarnessQueueLayout(
            manualCleanBootObserverNotReadyRoot,
            harnessRoot,
            inboxRoot,
            Path.Combine(inboxRoot, "actions.ndjson"),
            outboxRoot,
            Path.Combine(outboxRoot, "results.ndjson"),
            Path.Combine(harnessRoot, "status.json"),
            Path.Combine(outboxRoot, "trace.ndjson"),
            Path.Combine(harnessRoot, "arm.json"),
            Path.Combine(outboxRoot, "inventory.latest.json"));
        LongRunArtifacts.InitializeSessionArtifacts(manualCleanBootObserverNotReadyRoot, "clean-boot-observer-not-ready-session", "boot-to-long-run", "headless");
        var screenshotPath = Path.Combine(manualCleanBootObserverNotReadyRoot, "unknown.png");
        var observerPath = Path.Combine(manualCleanBootObserverNotReadyRoot, "unknown.observer.json");
        File.WriteAllBytes(screenshotPath, Array.Empty<byte>());
        File.WriteAllText(observerPath, "{}");
        Assert(
            !LongRunArtifacts.TryMarkManualCleanBootVerified(
                manualCleanBootObserverNotReadyRoot,
                harnessLayout,
                new ObserverState(
                    new ObserverSummary(
                        null,
                        null,
                        false,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
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
                DateTimeOffset.UtcNow.AddSeconds(-5)),
            "Manual clean boot should remain invalid while observer evidence is not ready.");
        var notReadyPrevalidation = JsonSerializer.Deserialize<GuiSmokePrevalidation>(
                                        File.ReadAllText(Path.Combine(manualCleanBootObserverNotReadyRoot, "prevalidation.json")),
                                        GuiSmokeShared.JsonOptions)
                                    ?? throw new InvalidOperationException("Failed to read observer-not-ready manual clean boot self-test.");
        Assert(notReadyPrevalidation.ManualCleanBootEvidence?.BlockingReasons?.Contains("observer-not-ready", StringComparer.OrdinalIgnoreCase) == true, "Manual clean boot should report observer-not-ready instead of observer-not-main-menu:unknown.");
    }
    finally
    {
        if (Directory.Exists(manualCleanBootObserverNotReadyRoot))
        {
            Directory.Delete(manualCleanBootObserverNotReadyRoot, recursive: true);
        }
    }

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

    var restSiteNoOpSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-rest-site-noop-sentinel-self-test-{Guid.NewGuid():N}");
    try
    {
        Directory.CreateDirectory(restSiteNoOpSentinelRoot);
        var runRoot = Path.Combine(restSiteNoOpSentinelRoot, "attempts", "0001");
        Directory.CreateDirectory(runRoot);
        LongRunArtifacts.InitializeSessionArtifacts(restSiteNoOpSentinelRoot, "rest-site-noop-session", "boot-to-long-run", "headless");
        File.WriteAllLines(
            Path.Combine(restSiteNoOpSentinelRoot, "attempt-index.ndjson"),
            new[]
            {
                JsonSerializer.Serialize(
                    new GuiSmokeAttemptIndexEntry(
                        "0001",
                        1,
                        "rest-site-noop-session",
                        "failed",
                        "synthetic rest-site post-click noop",
                        DateTimeOffset.UtcNow.AddSeconds(-30),
                        DateTimeOffset.UtcNow.AddSeconds(-5),
                        6,
                        "rest-site-post-click-noop",
                        false,
                        "rest-site-post-click-noop",
                        GuiSmokeContractStates.TrustValid),
                    GuiSmokeShared.NdjsonOptions),
            });
        File.WriteAllText(
            Path.Combine(runRoot, "failure-summary.json"),
            JsonSerializer.Serialize(
                new GuiSmokeFailureSummary(
                    GuiSmokePhase.ChooseFirstNode.ToString(),
                    "rest-site-post-click-noop synthetic failure",
                    "map",
                    false,
                    "synthetic-rest-site-post-click-noop.png"),
                GuiSmokeShared.JsonOptions));
        LongRunArtifacts.RefreshStallSentinel(restSiteNoOpSentinelRoot);
        var diagnosisEntries = File.ReadLines(Path.Combine(restSiteNoOpSentinelRoot, "stall-diagnosis.ndjson"))
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
            .Where(static entry => entry is not null)
            .Cast<GuiSmokeStallDiagnosisEntry>()
            .ToArray();
        Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic rest-site post-click noop attempt.");
        Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase)
               && diagnosisEntries[0].StallDetected,
            "Stall sentinel should preserve rest-site-post-click-noop as a first-class diagnosis kind.");
    }
    finally
    {
        if (Directory.Exists(restSiteNoOpSentinelRoot))
        {
            Directory.Delete(restSiteNoOpSentinelRoot, recursive: true);
        }
    }

    var stallSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-stall-sentinel-self-test-{Guid.NewGuid():N}");
    try
    {
        Directory.CreateDirectory(stallSentinelRoot);
        var runRoot = Path.Combine(stallSentinelRoot, "attempts", "0001");
        Directory.CreateDirectory(runRoot);
        LongRunArtifacts.InitializeSessionArtifacts(stallSentinelRoot, "stall-session", "boot-to-long-run", "headless");
        var progressPath = Path.Combine(runRoot, "progress.ndjson");
        var progressEntries = new[]
        {
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 7, GuiSmokePhase.Embark.ToString(), "phase:embark|screen:event|visible:event|encounter:none|ready:true|stability:stable|room:treasure|shot:A", "event", null, null, true, false, true, false, false, new[] { "scene-ready-true", "decision-wait" }, Array.Empty<string>()),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 8, GuiSmokePhase.Embark.ToString(), "phase:embark|screen:event|visible:event|encounter:none|ready:true|stability:stable|room:treasure|shot:B", "event", null, null, true, false, true, false, false, new[] { "scene-ready-true", "decision-wait" }, Array.Empty<string>()),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 9, GuiSmokePhase.Embark.ToString(), "phase:embark|screen:event|visible:event|encounter:none|ready:true|stability:stable|room:treasure|shot:C", "event", null, null, true, false, true, false, false, new[] { "scene-ready-true", "decision-wait" }, Array.Empty<string>()),
        };
        File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
        LongRunArtifacts.RefreshStallSentinel(stallSentinelRoot);
        var diagnosisEntries = File.ReadLines(Path.Combine(stallSentinelRoot, "stall-diagnosis.ndjson"))
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
            .Where(static entry => entry is not null)
            .Cast<GuiSmokeStallDiagnosisEntry>()
            .ToArray();
        Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic in-progress attempt.");
        Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase), "Stall sentinel should classify Embark/event repeated waits as phase-mismatch-stall.");
        Assert(diagnosisEntries[0].StallDetected, "Stall sentinel should flag the repeated Embark/event wait plateau as a stall.");
    }
    finally
    {
        if (Directory.Exists(stallSentinelRoot))
        {
            Directory.Delete(stallSentinelRoot, recursive: true);
        }
    }

    var overlayLoopSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-overlay-loop-sentinel-self-test-{Guid.NewGuid():N}");
    try
    {
        Directory.CreateDirectory(overlayLoopSentinelRoot);
        var runRoot = Path.Combine(overlayLoopSentinelRoot, "attempts", "0001");
        Directory.CreateDirectory(runRoot);
        LongRunArtifacts.InitializeSessionArtifacts(overlayLoopSentinelRoot, "overlay-loop-session", "boot-to-long-run", "headless");
        var progressPath = Path.Combine(runRoot, "progress.ndjson");
        var progressEntries = new[]
        {
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-6), 6, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:reward-choice|substate:colorless-card-choice|shot:A", "event", null, "first event option", true, false, true, false, false, new[] { "scene-ready-true", "reward-choice", "colorless-card-choice" }, new[] { "action:click", "target:first event option" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-5), 7, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:inspect-overlay|substate:reward-choice|shot:B", "event", null, "overlay backdrop close", true, false, true, false, false, new[] { "scene-ready-true", "inspect-overlay", "reward-choice" }, new[] { "action:click", "target:overlay backdrop close" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-4), 8, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:event|visible:event|ready:true|stability:stable|substate:inspect-overlay|substate:reward-choice|shot:C", "event", null, null, true, false, true, false, false, new[] { "scene-ready-true", "inspect-overlay", "reward-choice", "alternate-branch:HandleEvent" }, Array.Empty<string>()),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 9, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:inspect-overlay|substate:reward-choice|shot:D", "event", null, "overlay backdrop close", true, false, true, false, false, new[] { "scene-ready-true", "inspect-overlay", "reward-choice" }, new[] { "action:click", "target:overlay backdrop close" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 10, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:event|visible:event|ready:true|stability:stable|substate:inspect-overlay|substate:reward-choice|shot:E", "event", null, null, true, false, true, false, false, new[] { "scene-ready-true", "inspect-overlay", "reward-choice", "alternate-branch:HandleEvent" }, Array.Empty<string>()),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 11, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:inspect-overlay|substate:reward-choice|shot:F", "event", null, "overlay backdrop close", true, false, true, false, false, new[] { "scene-ready-true", "inspect-overlay", "reward-choice" }, new[] { "action:click", "target:overlay backdrop close" }),
        };
        File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
        LongRunArtifacts.RefreshStallSentinel(overlayLoopSentinelRoot);
        var diagnosisEntries = File.ReadLines(Path.Combine(overlayLoopSentinelRoot, "stall-diagnosis.ndjson"))
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
            .Where(static entry => entry is not null)
            .Cast<GuiSmokeStallDiagnosisEntry>()
            .ToArray();
        Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic inspect overlay loop attempt.");
        Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "inspect-overlay-loop", StringComparison.OrdinalIgnoreCase), "Stall sentinel should classify repeated overlay dismissals in reward/event flow as inspect-overlay-loop.");
        Assert(diagnosisEntries[0].StallDetected, "Inspect overlay loop should be flagged as a stall.");
    }
    finally
    {
        if (Directory.Exists(overlayLoopSentinelRoot))
        {
            Directory.Delete(overlayLoopSentinelRoot, recursive: true);
        }
    }

    var mapTransitionSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-map-transition-sentinel-self-test-{Guid.NewGuid():N}");
    try
    {
        Directory.CreateDirectory(mapTransitionSentinelRoot);
        var runRoot = Path.Combine(mapTransitionSentinelRoot, "attempts", "0001");
        Directory.CreateDirectory(runRoot);
        LongRunArtifacts.InitializeSessionArtifacts(mapTransitionSentinelRoot, "map-transition-session", "boot-to-long-run", "headless");
        var progressPath = Path.Combine(runRoot, "progress.ndjson");
        var progressEntries = new[]
        {
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-5), 9, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:map-transition|visible:map-arrow|shot:A", "event", null, "event progression choice", true, false, true, false, false, new[] { "scene-ready-true", "map-transition-evidence" }, new[] { "action:click", "target:event progression choice" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-4), 10, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:event|visible:event|ready:true|stability:stable|substate:map-transition|visible:map-arrow|shot:B", "event", null, null, true, false, false, false, false, new[] { "scene-ready-true", "map-transition-evidence", "alternate-branch:HandleEvent" }, Array.Empty<string>()),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 11, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:map-transition|visible:map-arrow|shot:C", "event", null, "visible proceed", true, false, false, false, false, new[] { "scene-ready-true", "map-transition-evidence" }, new[] { "action:click", "target:visible proceed" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 12, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:event|visible:event|ready:true|stability:stable|substate:map-transition|visible:map-arrow|shot:D", "event", null, null, true, false, false, false, false, new[] { "scene-ready-true", "map-transition-evidence", "alternate-branch:HandleEvent" }, Array.Empty<string>()),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 13, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:map-transition|visible:map-arrow|shot:E", "event", null, "event progression choice", true, false, false, false, false, new[] { "scene-ready-true", "map-transition-evidence" }, new[] { "action:click", "target:event progression choice" }),
        };
        File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
        LongRunArtifacts.RefreshStallSentinel(mapTransitionSentinelRoot);
        var diagnosisEntries = File.ReadLines(Path.Combine(mapTransitionSentinelRoot, "stall-diagnosis.ndjson"))
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
            .Where(static entry => entry is not null)
            .Cast<GuiSmokeStallDiagnosisEntry>()
            .ToArray();
        Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic map-transition loop attempt.");
        Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "map-transition-stall", StringComparison.OrdinalIgnoreCase), "Stall sentinel should classify repeated event/proceed clicks on map-transition evidence as map-transition-stall.");
        Assert(diagnosisEntries[0].StallDetected, "Map transition loop should be flagged as a stall.");
    }
    finally
    {
        if (Directory.Exists(mapTransitionSentinelRoot))
        {
            Directory.Delete(mapTransitionSentinelRoot, recursive: true);
        }
    }

    var latestEventSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-latest-event-sentinel-self-test-{Guid.NewGuid():N}");
    try
    {
        Directory.CreateDirectory(latestEventSentinelRoot);
        var runRoot = Path.Combine(latestEventSentinelRoot, "attempts", "0001");
        Directory.CreateDirectory(runRoot);
        LongRunArtifacts.InitializeSessionArtifacts(latestEventSentinelRoot, "latest-event-session", "boot-to-long-run", "headless");
        var progressPath = Path.Combine(runRoot, "progress.ndjson");
        var progressEntries = new[]
        {
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-6), 5, GuiSmokePhase.HandleRewards.ToString(), "phase:handlerewards|screen:rewards|visible:map|stale:reward-choice|stale:reward-bounds|layer:map-background|visible:map-arrow|shot:R1", "rewards", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "reward-map-layered" }, new[] { "action:click", "target:visible map advance" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-5), 6, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:rewards|visible:map|stale:reward-choice|stale:reward-bounds|layer:map-background|visible:map-arrow|shot:R2", "rewards", null, null, true, false, false, false, false, new[] { "scene-ready-true", "reward-map-layered" }, Array.Empty<string>()),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 43, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|layer:event-foreground|contamination:map-arrow|shot:E1", "event", null, "visible reachable node", true, false, false, false, false, new[] { "scene-ready-true", "map-transition-evidence" }, new[] { "action:click", "target:visible reachable node" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 44, GuiSmokePhase.WaitCombat.ToString(), "phase:waitcombat|screen:event|visible:event|layer:event-foreground|contamination:map-arrow|shot:E2", "event", null, null, true, false, false, false, false, new[] { "scene-ready-true", "alternate-branch:HandleEvent", "map-transition-evidence", "choice-extractor:event", "reward-explicit-progression" }, Array.Empty<string>()),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 45, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|layer:event-foreground|contamination:map-arrow|shot:E3", "event", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "map-transition-evidence" }, new[] { "action:click", "target:visible map advance" }),
        };
        File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
        LongRunArtifacts.RefreshStallSentinel(latestEventSentinelRoot);
        var diagnosisEntries = File.ReadLines(Path.Combine(latestEventSentinelRoot, "stall-diagnosis.ndjson"))
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
            .Where(static entry => entry is not null)
            .Cast<GuiSmokeStallDiagnosisEntry>()
            .ToArray();
        Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the latest-event sentinel attempt.");
        Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "map-transition-stall", StringComparison.OrdinalIgnoreCase), "Latest event/map contamination loop should classify as map-transition-stall.");
        Assert(string.Equals(diagnosisEntries[0].Phase, GuiSmokePhase.HandleEvent.ToString(), StringComparison.OrdinalIgnoreCase), "Latest event authority should win over stale reward summaries in stall diagnosis.");
        Assert(string.Equals(diagnosisEntries[0].ObserverScreen, "event", StringComparison.OrdinalIgnoreCase), "Latest event observer screen should win over stale reward summaries in stall diagnosis.");
    }
    finally
    {
        if (Directory.Exists(latestEventSentinelRoot))
        {
            Directory.Delete(latestEventSentinelRoot, recursive: true);
        }
    }

    var mapOverlaySentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-map-overlay-sentinel-self-test-{Guid.NewGuid():N}");
    try
    {
        Directory.CreateDirectory(mapOverlaySentinelRoot);
        var runRoot = Path.Combine(mapOverlaySentinelRoot, "attempts", "0001");
        Directory.CreateDirectory(runRoot);
        LongRunArtifacts.InitializeSessionArtifacts(mapOverlaySentinelRoot, "map-overlay-session", "boot-to-long-run", "headless");
        var progressPath = Path.Combine(runRoot, "progress.ndjson");
        var progressEntries = new[]
        {
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-5), 15, GuiSmokePhase.ChooseFirstNode.ToString(), "phase:choosefirstnode|screen:event|visible:event|layer:map-overlay-foreground|layer:event-background|stale:event-choice|map-back-navigation-available|current-node-arrow-visible|reachable-node-candidate-present|shot:A", "event", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "map-overlay-visible", "stale-event-choice", "current-node-arrow-visible", "reachable-node-candidate-present" }, new[] { "action:click", "target:visible map advance" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-4), 16, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:event|visible:event|layer:map-overlay-foreground|layer:event-background|stale:event-choice|map-back-navigation-available|current-node-arrow-visible|reachable-node-candidate-present|shot:B", "event", null, null, true, false, false, false, false, new[] { "scene-ready-true", "map-overlay-visible", "stale-event-choice", "alternate-branch:ChooseFirstNode" }, Array.Empty<string>()),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 17, GuiSmokePhase.ChooseFirstNode.ToString(), "phase:choosefirstnode|screen:event|visible:event|layer:map-overlay-foreground|layer:event-background|stale:event-choice|map-back-navigation-available|current-node-arrow-visible|reachable-node-candidate-present|shot:C", "event", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "map-overlay-visible", "stale-event-choice", "current-node-arrow-visible", "reachable-node-candidate-present" }, new[] { "action:click", "target:visible map advance" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 18, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:event|visible:event|layer:map-overlay-foreground|layer:event-background|stale:event-choice|map-back-navigation-available|current-node-arrow-visible|reachable-node-candidate-present|shot:D", "event", null, null, true, false, false, false, false, new[] { "scene-ready-true", "map-overlay-visible", "stale-event-choice", "alternate-branch:ChooseFirstNode" }, Array.Empty<string>()),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 19, GuiSmokePhase.ChooseFirstNode.ToString(), "phase:choosefirstnode|screen:event|visible:event|layer:map-overlay-foreground|layer:event-background|stale:event-choice|map-back-navigation-available|current-node-arrow-visible|reachable-node-candidate-present|shot:E", "event", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "map-overlay-visible", "stale-event-choice", "current-node-arrow-visible", "reachable-node-candidate-present" }, new[] { "action:click", "target:visible map advance" }),
        };
        File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
        LongRunArtifacts.RefreshStallSentinel(mapOverlaySentinelRoot);
        var diagnosisEntries = File.ReadLines(Path.Combine(mapOverlaySentinelRoot, "stall-diagnosis.ndjson"))
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
            .Where(static entry => entry is not null)
            .Cast<GuiSmokeStallDiagnosisEntry>()
            .ToArray();
        Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic map-overlay loop attempt.");
        Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "map-overlay-noop-loop", StringComparison.OrdinalIgnoreCase), "Map overlay current-node arrow repeats should classify as map-overlay-noop-loop.");
        Assert(diagnosisEntries[0].Evidence.Contains("mapOverlayVisible:true", StringComparer.OrdinalIgnoreCase), "Map overlay diagnosis should record map-overlay visibility.");
        Assert(diagnosisEntries[0].Evidence.Contains("staleEventChoicePresent:true", StringComparer.OrdinalIgnoreCase), "Map overlay diagnosis should record stale event choice contamination.");
        Assert(diagnosisEntries[0].Evidence.Contains("repeatedCurrentNodeArrowClick:true", StringComparer.OrdinalIgnoreCase), "Map overlay diagnosis should record repeated current-node-arrow clicks.");
    }
    finally
    {
        if (Directory.Exists(mapOverlaySentinelRoot))
        {
            Directory.Delete(mapOverlaySentinelRoot, recursive: true);
        }
    }

    var rewardMapLoopSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-reward-map-loop-sentinel-self-test-{Guid.NewGuid():N}");
    try
    {
        Directory.CreateDirectory(rewardMapLoopSentinelRoot);
        var runRoot = Path.Combine(rewardMapLoopSentinelRoot, "attempts", "0001");
        Directory.CreateDirectory(runRoot);
        LongRunArtifacts.InitializeSessionArtifacts(rewardMapLoopSentinelRoot, "reward-map-loop-session", "boot-to-long-run", "headless");
        var progressPath = Path.Combine(runRoot, "progress.ndjson");
        var progressEntries = new[]
        {
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-5), 33, GuiSmokePhase.HandleRewards.ToString(), "phase:handlerewards|screen:rewards|visible:rewards|ready:true|stability:stable|contamination:map-arrow|shot:A", "rewards", null, "visible map advance", true, false, true, false, false, new[] { "scene-ready-true", "choice-extractor:reward", "reward-screen-authority", "reward-explicit-progression" }, new[] { "action:click", "target:visible map advance" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-4), 34, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:rewards|visible:rewards|ready:true|stability:stable|contamination:map-arrow|shot:B", "rewards", null, null, true, false, false, false, false, new[] { "scene-ready-true", "choice-extractor:reward", "reward-screen-authority", "reward-explicit-progression", "alternate-branch:HandleRewards" }, Array.Empty<string>()),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 35, GuiSmokePhase.HandleRewards.ToString(), "phase:handlerewards|screen:rewards|visible:rewards|ready:true|stability:stable|contamination:map-arrow|shot:C", "rewards", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "choice-extractor:reward", "reward-screen-authority", "reward-explicit-progression" }, new[] { "action:click", "target:visible map advance" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 36, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:rewards|visible:rewards|ready:true|stability:stable|contamination:map-arrow|shot:D", "rewards", null, null, true, false, false, false, false, new[] { "scene-ready-true", "choice-extractor:reward", "reward-screen-authority", "reward-explicit-progression", "alternate-branch:HandleRewards" }, Array.Empty<string>()),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 37, GuiSmokePhase.HandleRewards.ToString(), "phase:handlerewards|screen:rewards|visible:rewards|ready:true|stability:stable|contamination:map-arrow|shot:E", "rewards", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "choice-extractor:reward", "reward-screen-authority", "reward-explicit-progression" }, new[] { "action:click", "target:visible map advance" }),
        };
        File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
        LongRunArtifacts.RefreshStallSentinel(rewardMapLoopSentinelRoot);
        var diagnosisEntries = File.ReadLines(Path.Combine(rewardMapLoopSentinelRoot, "stall-diagnosis.ndjson"))
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
            .Where(static entry => entry is not null)
            .Cast<GuiSmokeStallDiagnosisEntry>()
            .ToArray();
        Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic reward/map loop attempt.");
        Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "reward-map-loop", StringComparison.OrdinalIgnoreCase), "Stall sentinel should classify repeated map-like clicks on a reward-authoritative screen as reward-map-loop.");
        Assert(diagnosisEntries[0].Evidence.Contains("rewardExplicitChoicesPresent:true", StringComparer.OrdinalIgnoreCase), "Reward/map loop diagnosis should record that explicit reward choices were still present.");
        Assert(diagnosisEntries[0].Evidence.Contains("rewardMapArrowContamination:true", StringComparer.OrdinalIgnoreCase), "Reward/map loop diagnosis should record background map-arrow contamination.");
    }
    finally
    {
        if (Directory.Exists(rewardMapLoopSentinelRoot))
        {
            Directory.Delete(rewardMapLoopSentinelRoot, recursive: true);
        }
    }

    var combatNoOpSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-combat-noop-sentinel-self-test-{Guid.NewGuid():N}");
    try
    {
        Directory.CreateDirectory(combatNoOpSentinelRoot);
        var runRoot = Path.Combine(combatNoOpSentinelRoot, "attempts", "0001");
        Directory.CreateDirectory(runRoot);
        LongRunArtifacts.InitializeSessionArtifacts(combatNoOpSentinelRoot, "combat-noop-session", "boot-to-long-run", "headless");
        var progressPath = Path.Combine(runRoot, "progress.ndjson");
        var progressEntries = new[]
        {
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-6), 53, GuiSmokePhase.HandleRewards.ToString(), "phase:handlerewards|screen:rewards|visible:rewards|layer:reward-foreground|stale:reward-choice|shot:PREV", "rewards", null, "reward skip", false, false, false, false, false, new[] { "scene-ready-true", "reward-screen-authority", "stale-reward-choice" }, new[] { "action:click", "target:reward skip" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-5), 54, GuiSmokePhase.HandleCombat.ToString(), "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable|shot:A", "combat", null, "combat select attack slot 3", true, false, true, false, false, new[] { "scene-ready-true", "combat-energy", "combat-hand", "combat-noop-observed:combat lane slot 3" }, new[] { "action:press-key", "target:combat select attack slot 3" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-4), 55, GuiSmokePhase.HandleCombat.ToString(), "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable|shot:B", "combat", null, "auto-target enemy", false, false, false, false, false, new[] { "scene-ready-true", "combat-energy", "combat-hand", "combat-noop-observed:combat lane slot 3" }, new[] { "action:click", "target:auto-target enemy" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 56, GuiSmokePhase.HandleCombat.ToString(), "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable|shot:C", "combat", null, "combat select attack slot 3", false, false, false, false, false, new[] { "scene-ready-true", "combat-energy", "combat-hand", "combat-noop-observed:combat lane slot 3" }, new[] { "action:press-key", "target:combat select attack slot 3" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 57, GuiSmokePhase.HandleCombat.ToString(), "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable|shot:D", "combat", null, "auto-target enemy recenter", false, false, false, false, false, new[] { "scene-ready-true", "combat-energy", "combat-hand", "combat-noop-observed:combat lane slot 3" }, new[] { "action:click", "target:auto-target enemy recenter" }),
            new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 58, GuiSmokePhase.HandleCombat.ToString(), "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable|shot:E", "combat", null, "combat select attack slot 3", false, false, false, false, false, new[] { "scene-ready-true", "combat-energy", "combat-hand", "combat-noop-observed:combat lane slot 3" }, new[] { "action:press-key", "target:combat select attack slot 3" }),
        };
        File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
        LongRunArtifacts.RefreshStallSentinel(combatNoOpSentinelRoot);
        var diagnosisEntries = File.ReadLines(Path.Combine(combatNoOpSentinelRoot, "stall-diagnosis.ndjson"))
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
            .Where(static entry => entry is not null)
            .Cast<GuiSmokeStallDiagnosisEntry>()
            .ToArray();
        Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic combat no-op loop attempt.");
        Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "combat-noop-loop", StringComparison.OrdinalIgnoreCase), "Stall sentinel should classify repeated attack-select/enemy-click no-op patterns as combat-noop-loop.");
        Assert(diagnosisEntries[0].StallDetected, "Combat no-op loop should be flagged as a stall.");
        Assert(!diagnosisEntries[0].Evidence.Contains("rewardMapLoopTarget:", StringComparer.OrdinalIgnoreCase), "Latest combat authority should keep stale reward evidence from overriding combat no-op diagnosis.");
    }
    finally
    {
        if (Directory.Exists(combatNoOpSentinelRoot))
        {
            Directory.Delete(combatNoOpSentinelRoot, recursive: true);
        }
    }

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

static IReadOnlyList<GuiSmokeHistoryEntry> BuildSerializedStepHistory(
    GuiSmokePhase phase,
    IReadOnlyList<GuiSmokeHistoryEntry> history)
{
    return phase == GuiSmokePhase.HandleCombat
        ? HandleCombatContextSupport.BuildSerializedHistoryWindow(history)
        : history.TakeLast(5).ToArray();
}

static GuiSmokeStepRequest CreateStepRequest(
    string runId,
    string scenarioId,
    int stepIndex,
    GuiSmokePhase phase,
    string screenshotPath,
    WindowCaptureTarget window,
    ObserverState observer,
    IReadOnlyList<GuiSmokeHistoryEntry> history,
    string workspaceRoot,
    string sessionRoot,
    string attemptId,
    int attemptOrdinal)
{
    var serializedHistory = BuildSerializedStepHistory(phase, history);
    var sceneSignature = ComputeSceneSignature(screenshotPath, observer, phase);
    var knownRecipes = LoadKnownRecipes(sessionRoot, sceneSignature, phase.ToString());
    var firstSeenScene = !HasSceneSignatureHistory(sessionRoot, sceneSignature);
    var reasoningMode = DetermineReasoningMode(phase, observer, firstSeenScene);
    var eventKnowledgeCandidates = LoadEventKnowledgeCandidates(workspaceRoot, observer, reasoningMode);
    var combatCardKnowledge = LoadCombatCardKnowledge(workspaceRoot, observer);
    return new GuiSmokeStepRequest(
        runId,
        scenarioId,
        stepIndex,
        phase.ToString(),
        BuildGoal(phase),
        DateTimeOffset.UtcNow,
        screenshotPath,
        new WindowBounds(window.Bounds.X, window.Bounds.Y, window.Bounds.Width, window.Bounds.Height),
        sceneSignature,
        attemptId,
        attemptOrdinal,
        3,
        firstSeenScene,
        reasoningMode,
        BuildSemanticGoal(phase, observer, reasoningMode),
        observer.Summary,
        knownRecipes,
        eventKnowledgeCandidates,
        combatCardKnowledge,
        BuildAllowedActions(phase, observer, combatCardKnowledge, screenshotPath, serializedHistory),
        serializedHistory,
        BuildFailureModeHintCore(phase, observer, combatCardKnowledge, screenshotPath, serializedHistory),
        BuildDecisionRiskHint(phase, observer, firstSeenScene, reasoningMode));
}

static string ComputeSceneSignature(string screenshotPath, ObserverState observer, GuiSmokePhase phase)
{
    var rewardMapLayer = BuildRewardMapLayerStateForObserver(observer.Summary, null);
    var rewardBackNavigationAvailable = rewardMapLayer.RewardBackNavigationAvailable || LooksLikeRewardBackNavigationAffordance(observer.Summary, screenshotPath);
    var claimableRewardPresent = HasScreenshotClaimableRewardEvidence(observer.Summary, screenshotPath);
    var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(observer, null, screenshotPath);
    var tags = new List<string>(capacity: 10)
    {
        $"phase:{phase.ToString().ToLowerInvariant()}",
        $"screen:{(observer.CurrentScreen ?? "unknown").Trim().ToLowerInvariant()}",
        $"visible:{(observer.VisibleScreen ?? "unknown").Trim().ToLowerInvariant()}",
        $"encounter:{(observer.EncounterKind ?? "none").Trim().ToLowerInvariant()}",
        $"ready:{(observer.SceneReady?.ToString() ?? "unknown").ToLowerInvariant()}",
        $"stability:{(observer.SceneStability ?? "unknown").Trim().ToLowerInvariant()}",
    };

    if (LooksLikeTreasureState(observer.Summary))
    {
        tags.Add("room:treasure");
    }

    if (LooksLikeRestSiteState(observer.Summary))
    {
        tags.Add("room:rest-site");
    }

    if (string.Equals(observer.CurrentScreen, "shop", StringComparison.OrdinalIgnoreCase)
        || string.Equals(observer.VisibleScreen, "shop", StringComparison.OrdinalIgnoreCase)
        || string.Equals(observer.EncounterKind, "Shop", StringComparison.OrdinalIgnoreCase)
        || string.Equals(observer.ChoiceExtractorPath, "shop", StringComparison.OrdinalIgnoreCase))
    {
        tags.Add("room:shop");
    }

    if (!ShouldSuppressRoomSubstateHeuristics(phase, observer))
    {
        if (HasExplicitRestSiteChoiceAuthority(observer, screenshotPath))
        {
            tags.Add("layer:rest-site-foreground");
        }

        if (rewardMapLayer.RewardPanelVisible)
        {
            tags.Add("layer:reward-foreground");
        }

        if (rewardMapLayer.MapContextVisible)
        {
            tags.Add("layer:map-background");
        }

        if (mapOverlayState.ForegroundVisible)
        {
            tags.Add("layer:map-overlay-foreground");
        }

        if (mapOverlayState.EventBackgroundPresent)
        {
            tags.Add("layer:event-background");
        }

        if (rewardBackNavigationAvailable)
        {
            tags.Add("layer:reward-back-nav");
        }

        if (rewardMapLayer.StaleRewardChoicePresent)
        {
            tags.Add("stale:reward-choice");
        }

        if (rewardMapLayer.StaleRewardBoundsPresent)
        {
            tags.Add("stale:reward-bounds");
        }

        if (mapOverlayState.StaleEventChoicePresent)
        {
            tags.Add("stale:event-choice");
        }

        if (claimableRewardPresent)
        {
            tags.Add("reward:claimable");
        }

        if (mapOverlayState.MapBackNavigationAvailable)
        {
            tags.Add("map-back-navigation-available");
        }

        if (mapOverlayState.CurrentNodeArrowVisible)
        {
            tags.Add("current-node-arrow-visible");
        }

        if (mapOverlayState.ReachableNodeCandidatePresent)
        {
            tags.Add("reachable-node-candidate-present");
        }

        if (mapOverlayState.ExportedReachableNodeCandidatePresent)
        {
            tags.Add("exported-reachable-node-present");
        }

        if (LooksLikeInspectOverlayState(observer))
        {
            tags.Add("substate:inspect-overlay");
        }

        if (LooksLikeRewardChoiceState(observer))
        {
            tags.Add("substate:reward-choice");
        }

        if (LooksLikeColorlessCardChoiceState(observer))
        {
            tags.Add("substate:colorless-card-choice");
        }

        if (!mapOverlayState.ForegroundVisible
            && HasStrongMapTransitionEvidence(observer)
            && !string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
            && !ShouldPreferRewardProgressionOverMapFallback(observer)
            && !GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer))
        {
            tags.Add("substate:map-transition");
        }

        var mapAnalysis = AutoMapAnalyzer.Analyze(screenshotPath);
        if (mapAnalysis.HasCurrentArrow)
        {
            if (!mapOverlayState.ForegroundVisible)
            {
                tags.Add(rewardMapLayer.RewardPanelVisible && ShouldPreferRewardProgressionOverMapFallback(observer)
                    ? "contamination:map-arrow"
                    : GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer)
                        ? "contamination:map-arrow"
                        : "visible:map-arrow");
            }
        }

        if (GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer))
        {
            tags.Add("layer:event-foreground");
        }
    }

    var screenshotFingerprint = ComputeFileFingerprint(screenshotPath);
    tags.Add($"shot:{screenshotFingerprint[..Math.Min(12, screenshotFingerprint.Length)]}");
    return string.Join("|", tags);
}

static IReadOnlyList<KnownRecipeHint> LoadKnownRecipes(string sessionRoot, string sceneSignature, string phase)
{
    var recipesPath = Path.Combine(sessionRoot, "scene-recipes.ndjson");
    if (!File.Exists(recipesPath))
    {
        return Array.Empty<KnownRecipeHint>();
    }

    var hints = new List<KnownRecipeHint>();
    foreach (var line in File.ReadLines(recipesPath))
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            continue;
        }

        SceneRecipeEntry? entry;
        try
        {
            entry = JsonSerializer.Deserialize<SceneRecipeEntry>(line, GuiSmokeShared.JsonOptions);
        }
        catch (JsonException)
        {
            continue;
        }

        if (entry is null
            || !string.Equals(entry.SceneSignature, sceneSignature, StringComparison.Ordinal)
            || !string.Equals(entry.Phase, phase, StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        hints.Add(new KnownRecipeHint(
            entry.SceneSignature,
            entry.Phase,
            entry.ActionKind,
            entry.TargetLabel,
            entry.ExpectedScreen,
            entry.Reason));
    }

    return hints
        .TakeLast(3)
        .ToArray();
}

static bool HasSceneSignatureHistory(string sessionRoot, string sceneSignature)
{
    foreach (var fileName in new[] { "scene-recipes.ndjson", "unknown-scenes.ndjson" })
    {
        var path = Path.Combine(sessionRoot, fileName);
        if (!File.Exists(path))
        {
            continue;
        }

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || !line.Contains(sceneSignature, StringComparison.Ordinal))
            {
                continue;
            }

            return true;
        }
    }

    return false;
}

static string DetermineReasoningMode(GuiSmokePhase phase, ObserverState observer, bool firstSeenScene)
{
    if (phase == GuiSmokePhase.HandleEvent
        && (firstSeenScene
            || string.Equals(observer.CurrentScreen, "unknown", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.VisibleScreen, "unknown", StringComparison.OrdinalIgnoreCase)
            || observer.Summary.CurrentChoices.Count > 0))
    {
        return "semantic";
    }

    return "tactical";
}

static string? BuildSemanticGoal(GuiSmokePhase phase, ObserverState observer, string reasoningMode)
{
    if (!string.Equals(reasoningMode, "semantic", StringComparison.OrdinalIgnoreCase))
    {
        return null;
    }

    return phase switch
    {
        GuiSmokePhase.HandleEvent => LooksLikeTreasureState(observer.Summary)
            ? "Interpret the room state, identify whether the chest is unopened, opened, or in reward follow-up, and choose the safest progress action."
            : "Interpret the event screen, infer what each visible option means, and choose a low-risk action that keeps the run progressing.",
        GuiSmokePhase.HandleCombat => "Interpret the combat board, hand, and targets before committing to the next action.",
        _ => "Interpret the current screen semantically before acting.",
    };
}

static string? BuildDecisionRiskHint(GuiSmokePhase phase, ObserverState observer, bool firstSeenScene, string reasoningMode)
{
    var hints = new List<string>();
    if (observer.SceneReady == false)
    {
        hints.Add("scene-not-ready");
    }

    if (!string.Equals(observer.SceneStability, "stable", StringComparison.OrdinalIgnoreCase))
    {
        hints.Add($"scene-stability:{observer.SceneStability ?? "unknown"}");
    }

    if (firstSeenScene)
    {
        hints.Add("first-seen-scene");
    }

    if (string.Equals(reasoningMode, "semantic", StringComparison.OrdinalIgnoreCase))
    {
        hints.Add("semantic-scene-ambiguity");
    }

    if (phase == GuiSmokePhase.HandleCombat && observer.PlayerEnergy is null)
    {
        hints.Add("observer-missing-energy");
    }

    return hints.Count == 0
        ? null
        : string.Join(", ", hints);
}

static IReadOnlyList<EventKnowledgeCandidate> LoadEventKnowledgeCandidates(string workspaceRoot, ObserverState observer, string reasoningMode)
{
    if (!string.Equals(reasoningMode, "semantic", StringComparison.OrdinalIgnoreCase))
    {
        return Array.Empty<EventKnowledgeCandidate>();
    }

    var choiceKeys = observer.Summary.CurrentChoices
        .Concat(observer.Summary.Choices.Select(static choice => choice.Label))
        .Where(static label => !string.IsNullOrWhiteSpace(label))
        .Select(NormalizeKnowledgeKey)
        .Where(static label => label.Length > 0)
        .Distinct(StringComparer.Ordinal)
        .ToArray();
    if (choiceKeys.Length == 0)
    {
        return Array.Empty<EventKnowledgeCandidate>();
    }

    var matches = new List<(AssistantEventKnowledge Event, int Score, string Reason)>();
    foreach (var entry in AssistantKnowledgeCatalog.LoadEvents(workspaceRoot))
    {
        var optionKeys = entry.Options
            .Select(static option => NormalizeKnowledgeKey(option.Label))
            .Where(static label => label.Length > 0)
            .ToArray();
        if (optionKeys.Length == 0)
        {
            continue;
        }

        var score = choiceKeys.Count(choiceKey => optionKeys.Contains(choiceKey, StringComparer.Ordinal));
        if (score <= 0)
        {
            continue;
        }

        matches.Add((entry, score, $"matched-options:{score}"));
    }

    return matches
        .OrderByDescending(static match => match.Score)
        .ThenBy(static match => match.Event.Title, StringComparer.OrdinalIgnoreCase)
        .Take(3)
        .Select(match => new EventKnowledgeCandidate(
            match.Event.Id,
            match.Event.Title,
            match.Reason,
            match.Event.Options
                .Take(5)
                .Select(static option => new EventOptionKnowledgeCandidate(
                    option.Label,
                    option.Description,
                    option.OptionKey))
                .ToArray()))
        .ToArray();
}

static IReadOnlyList<CombatCardKnowledgeHint> LoadCombatCardKnowledge(string workspaceRoot, ObserverState observer)
{
    if (observer.CombatHand.Count == 0)
    {
        return Array.Empty<CombatCardKnowledgeHint>();
    }

    var cards = AssistantKnowledgeCatalog.LoadCards(workspaceRoot);
    var hints = new List<CombatCardKnowledgeHint>(observer.CombatHand.Count);
    foreach (var handCard in observer.CombatHand)
    {
        var matchKeys = BuildCombatKnowledgeLookupKeys(handCard.Name);
        if (matchKeys.Count == 0)
        {
            continue;
        }

        var match = cards
            .Select(card => new
            {
                Card = card,
                BestMatchLength = matchKeys
                    .Where(key => card.MatchKeys.Contains(key, StringComparer.Ordinal))
                    .DefaultIfEmpty(string.Empty)
                    .Max(static key => key.Length),
            })
            .Where(static entry => entry.BestMatchLength > 0)
            .OrderByDescending(static entry => entry.BestMatchLength)
            .ThenBy(static entry => entry.Card.Id, StringComparer.OrdinalIgnoreCase)
            .Select(static entry => entry.Card)
            .FirstOrDefault();
        if (match is null)
        {
            continue;
        }

        hints.Add(new CombatCardKnowledgeHint(
            handCard.SlotIndex,
            handCard.Name,
            handCard.Type ?? match.Type,
            match.Target,
            handCard.Cost ?? match.Cost,
            "assistant/cards.json"));
    }

    return hints;
}

static IReadOnlyList<string> BuildCombatKnowledgeLookupKeys(string? cardName)
{
    if (string.IsNullOrWhiteSpace(cardName))
    {
        return Array.Empty<string>();
    }

    var keys = new List<string>();
    var normalizedName = NormalizeKnowledgeKey(cardName);
    AddCombatKnowledgeLookupKey(keys, normalizedName);

    var parts = cardName
        .Split(new[] { '.', '_', '-', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(NormalizeKnowledgeKey)
        .Where(static part => part.Length > 0)
        .ToArray();
    if (parts.Length == 0)
    {
        return keys;
    }

    var trimmedParts = parts[0] == "card"
        ? parts[1..]
        : parts;
    if (trimmedParts.Length == 0)
    {
        return keys;
    }

    AddCombatKnowledgeLookupKey(keys, string.Concat(trimmedParts));
    AddCombatKnowledgeLookupKey(keys, trimmedParts[0]);
    if (trimmedParts.Length > 1 && IsCombatClassSuffix(trimmedParts[^1]))
    {
        AddCombatKnowledgeLookupKey(keys, string.Concat(trimmedParts[..^1]));
    }

    foreach (var part in trimmedParts)
    {
        AddCombatKnowledgeLookupKey(keys, part);
    }

    return keys;
}

static void AddCombatKnowledgeLookupKey(List<string> keys, string? candidate)
{
    if (string.IsNullOrWhiteSpace(candidate)
        || keys.Contains(candidate, StringComparer.Ordinal))
    {
        return;
    }

    keys.Add(candidate);
}

static bool IsCombatClassSuffix(string value)
{
    return value is "ironclad" or "silent" or "defect" or "watcher" or "colorless" or "status" or "curse";
}

static string NormalizeKnowledgeKey(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return string.Empty;
    }

    Span<char> buffer = stackalloc char[value.Length];
    var length = 0;
    foreach (var character in value)
    {
        if (char.IsLetterOrDigit(character))
        {
            buffer[length] = char.ToLowerInvariant(character);
            length += 1;
        }
    }

    return length == 0
        ? string.Empty
        : new string(buffer[..length]);
}

static string[] GetAllowedActions(GuiSmokePhase phase, ObserverState observer)
{
    return BuildAllowedActions(phase, observer, Array.Empty<CombatCardKnowledgeHint>(), null, Array.Empty<GuiSmokeHistoryEntry>());
}

static string[] BuildAllowedActions(
    GuiSmokePhase phase,
    ObserverState observer,
    IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
    string? screenshotPath,
    IReadOnlyList<GuiSmokeHistoryEntry> history)
{
    var rewardMapLayer = BuildRewardMapLayerStateForObserver(observer.Summary, null);
    var rewardBackNavigationAvailable = rewardMapLayer.RewardBackNavigationAvailable || LooksLikeRewardBackNavigationAffordance(observer.Summary, screenshotPath);
    var claimableRewardPresent = HasScreenshotClaimableRewardEvidence(observer.Summary, screenshotPath);
    var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(observer, null, screenshotPath);
    var explicitRestSiteChoiceAuthority = HasExplicitRestSiteChoiceAuthority(observer, screenshotPath);
    if (phase == GuiSmokePhase.Embark && GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(observer, out var observedPhase))
    {
        return BuildAllowedActions(observedPhase, observer, combatCardKnowledge, screenshotPath, history);
    }

    return phase switch
    {
        GuiSmokePhase.EnterRun => new[] { "click continue", "click singleplayer", "click normal mode", "wait" },
        GuiSmokePhase.ChooseCharacter => new[] { "click ironclad", "click character confirm", "wait" },
        GuiSmokePhase.Embark => new[] { "click embark", "click character confirm", "wait" },
        GuiSmokePhase.HandleRewards when LooksLikeInspectOverlayState(observer)
            => new[] { "press escape", "click inspect overlay close", "wait" },
        GuiSmokePhase.HandleRewards when rewardMapLayer.RewardPanelVisible && (claimableRewardPresent || LooksLikeColorlessCardChoiceState(observer))
            => new[] { "click colorless card choice", "click reward skip", "click proceed", "press escape", "wait" },
        GuiSmokePhase.HandleRewards when rewardMapLayer.RewardPanelVisible && (claimableRewardPresent || LooksLikeRewardChoiceState(observer))
            => new[] { "click reward card choice", "click reward choice", "click reward skip", "click proceed", rewardBackNavigationAvailable ? "click reward back" : "press escape", "wait" },
        GuiSmokePhase.HandleRewards when rewardMapLayer.RewardPanelVisible && ShouldPreferRewardProgressionOverMapFallback(observer)
            => claimableRewardPresent
                ? new[] { "click reward card choice", "click reward", "click reward skip", "click proceed", "wait" }
                : new[] { "click reward", "click reward skip", "click proceed", "wait" },
        GuiSmokePhase.HandleRewards when rewardMapLayer.MapContextVisible
            => rewardBackNavigationAvailable
                ? new[] { "click first reachable node", "click visible map advance", "click reward back", "wait" }
                : new[] { "click first reachable node", "click visible map advance", "wait" },
        GuiSmokePhase.HandleRewards => new[] { "click proceed", "click reward", "wait" },
        GuiSmokePhase.ChooseFirstNode when explicitRestSiteChoiceAuthority
            => BuildExplicitRestSiteAllowedActions(observer.Summary),
        GuiSmokePhase.ChooseFirstNode when LooksLikeRestSiteState(observer.Summary)
            => new[] { "click smith card", "click smith confirm", "wait" },
        GuiSmokePhase.ChooseFirstNode when mapOverlayState.ForegroundVisible
            => mapOverlayState.MapBackNavigationAvailable
                ? new[] { "click exported reachable node", "click first reachable node", "click map back", "wait" }
                : new[] { "click exported reachable node", "click first reachable node", "wait" },
        GuiSmokePhase.ChooseFirstNode when LooksLikeTreasureState(observer.Summary)
            => new[] { "click treasure chest", "click treasure reward icon", "click proceed", "wait" },
        GuiSmokePhase.ChooseFirstNode => new[] { "click first reachable node", "click visible map advance", "wait" },
        GuiSmokePhase.HandleEvent when LooksLikeInspectOverlayState(observer)
            => new[] { "press escape", "click inspect overlay close", "wait" },
        GuiSmokePhase.HandleEvent when rewardMapLayer.RewardPanelVisible && (claimableRewardPresent || LooksLikeColorlessCardChoiceState(observer))
            => new[] { "click colorless card choice", "click reward skip", "click proceed", "press escape", "wait" },
        GuiSmokePhase.HandleEvent when rewardMapLayer.RewardPanelVisible && (claimableRewardPresent || LooksLikeRewardChoiceState(observer))
            => new[] { "click reward card choice", "click reward choice", "click reward skip", "click proceed", rewardBackNavigationAvailable ? "click reward back" : "press escape", "wait" },
        GuiSmokePhase.HandleEvent when rewardMapLayer.RewardPanelVisible && ShouldPreferRewardProgressionOverMapFallback(observer)
            => new[] { "click reward", "click reward skip", "click proceed", "wait" },
        GuiSmokePhase.HandleEvent when mapOverlayState.ForegroundVisible
            => mapOverlayState.MapBackNavigationAvailable
                ? new[] { "click exported reachable node", "click first reachable node", "click map back", "wait" }
                : new[] { "click exported reachable node", "click first reachable node", "wait" },
        GuiSmokePhase.HandleEvent when HasStrongMapTransitionEvidence(observer)
                                        && !GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer)
            => new[] { "click first reachable node", "click visible map advance", "click proceed", "wait" },
        GuiSmokePhase.HandleEvent when LooksLikeTreasureState(observer.Summary)
            => new[] { "click treasure chest", "click treasure reward icon", "click proceed", "click first event option", "wait" },
        GuiSmokePhase.HandleEvent => new[] { "click event choice", "click proceed", "wait" },
        GuiSmokePhase.HandleCombat => GetCombatAllowedActions(observer, combatCardKnowledge, screenshotPath, history),
        _ => new[] { "wait" },
    };
}

static bool LooksLikeInspectOverlayState(ObserverState observer)
{
    return observer.CurrentChoices.Any(static label =>
               label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
               || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
               || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase))
           || observer.ActionNodes.Any(static node =>
               node.Label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
}

static bool LooksLikeRewardChoiceState(ObserverState observer)
{
    if (GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary))
    {
        return false;
    }

    var rewardAuthority = HasRewardChoiceAuthority(observer);
    var rewardCardCount = observer.Choices.Count(IsRewardCardChoice);
    var inspectPreviewCount = observer.Choices.Count(static choice =>
        string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
        || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true);
    return rewardCardCount > 0
           || (rewardAuthority && inspectPreviewCount > 0 && observer.Choices.Any(static choice => IsSkipOrProceedLabel(choice.Label)));
}

static bool LooksLikeColorlessCardChoiceState(ObserverState observer)
{
    return !GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary)
           && HasRewardChoiceAuthority(observer)
           && observer.Choices.Any(IsRewardCardChoice)
           && observer.Choices.Any(static choice =>
               string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true);
}

static bool ShouldSuppressRoomSubstateHeuristics(GuiSmokePhase phase, ObserverState observer)
{
    return phase == GuiSmokePhase.HandleCombat
           || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary);
}

static bool HasRewardChoiceAuthority(ObserverState observer)
{
    return HasStrongRewardScreenAuthority(observer)
           || string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase)
           || string.Equals(observer.VisibleScreen, "event", StringComparison.OrdinalIgnoreCase)
           || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
           || string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase);
}

static bool HasStrongRewardScreenAuthority(ObserverState observer)
{
    return HasStrongRewardScreenAuthoritySummary(observer.Summary)
           || IsRewardsScreenType(GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "declaringType"))
           || IsRewardsScreenType(GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "instanceType"));
}

static bool HasStrongRewardScreenAuthoritySummary(ObserverSummary observer)
{
    return GuiSmokeObserverPhaseHeuristics.LooksLikeRewardsState(observer);
}

static bool IsRewardsScreenType(string? typeName)
{
    return !string.IsNullOrWhiteSpace(typeName)
           && typeName.Contains("RewardsScreen", StringComparison.OrdinalIgnoreCase);
}

static bool ShouldPreferRewardProgressionOverMapFallback(ObserverState observer)
{
    return ShouldPreferRewardProgressionOverMapFallbackSummary(observer.Summary);
}

static bool ShouldPreferRewardProgressionOverMapFallbackSummary(ObserverSummary observer)
{
    return HasStrongRewardScreenAuthoritySummary(observer)
           && HasExplicitRewardProgressionAffordance(observer);
}

static bool HasExplicitRewardProgressionAffordance(ObserverSummary observer)
{
    return observer.Choices.Any(IsExplicitRewardProgressionChoice)
           || observer.ActionNodes.Any(IsExplicitRewardProgressionNode);
}

static bool IsExplicitRewardProgressionChoice(ObserverChoice choice)
{
    if (!TryParseNodeBounds(choice.ScreenBounds, out _)
        || IsOverlayChoiceLabel(choice.Label)
        || IsInspectPreviewChoice(choice)
        || IsDismissLikeLabel(choice.Label))
    {
        return false;
    }

    return IsRewardCardChoice(choice)
           || IsSkipOrProceedLabel(choice.Label)
           || choice.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
           || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
           || HasLargeChoiceBounds(choice.ScreenBounds);
}

static bool IsExplicitRewardProgressionNode(ObserverActionNode node)
{
    if (!node.Actionable
        || !TryParseNodeBounds(node.ScreenBounds, out _)
        || IsOverlayChoiceLabel(node.Label)
        || IsInspectPreviewBounds(node.ScreenBounds)
        || IsDismissLikeLabel(node.Label)
        || IsMapNode(node)
        || IsBackNode(node))
    {
        return false;
    }

    return IsProceedNode(node)
           || node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
           || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
           || HasLargeChoiceBounds(node.ScreenBounds);
}

static bool IsRewardCardChoice(ObserverChoice choice)
{
    return string.Equals(choice.Kind, "card", StringComparison.OrdinalIgnoreCase)
           && !IsSkipOrProceedLabel(choice.Label)
           && !IsConfirmLikeLabel(choice.Label)
           && !IsDismissLikeLabel(choice.Label)
           && !IsOverlayChoiceLabel(choice.Label)
           && HasRewardCardLikeBounds(choice.ScreenBounds);
}

static bool HasRewardCardLikeBounds(string? screenBounds)
{
    if (!TryParseNodeBounds(screenBounds, out var bounds))
    {
        return true;
    }

    return bounds.Width >= 120f || bounds.Height >= 150f;
}

static bool IsInspectPreviewChoice(ObserverChoice choice)
{
    return IsOverlayChoiceLabel(choice.Label)
           || string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
           || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
           || IsInspectPreviewBounds(choice.ScreenBounds);
}

static string[] GetCombatAllowedActions(
    ObserverState observer,
    IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
    string? screenshotPath,
    IReadOnlyList<GuiSmokeHistoryEntry> history)
{
    if (CombatEligibilitySupport.IsCombatPlayerActionWindowClosed(observer.Summary))
    {
        return new[] { "wait" };
    }

    var actions = new List<string>();
    var combatContext = HandleCombatContextSupport.Reconstruct(history);
    var blockedCombatNoOpCounts = combatContext.CombatNoOpCountsBySlot;
    var pendingSelection = CombatRuntimeStateSupport.ResolvePendingSelection(observer.Summary, combatCardKnowledge, combatContext.PendingSelection);
    var analysis = string.IsNullOrWhiteSpace(screenshotPath)
        ? new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown)
        : AutoCombatAnalyzer.Analyze(screenshotPath);
    var hasSelectedNonEnemyConfirmEvidence = CombatEligibilitySupport.HasSelectedNonEnemyConfirmEvidence(observer.Summary, combatCardKnowledge, analysis, pendingSelection);
    var keepNonEnemySelectionClosed = pendingSelection?.Kind == AutoCombatCardKind.DefendLike && hasSelectedNonEnemyConfirmEvidence;
    bool ShouldSuppressNonEnemyReselect(int slotIndex)
    {
        if (keepNonEnemySelectionClosed)
        {
            return true;
        }

        if (pendingSelection?.Kind == AutoCombatCardKind.DefendLike
            && pendingSelection.SlotIndex == slotIndex
            && !hasSelectedNonEnemyConfirmEvidence)
        {
            return true;
        }

        return !hasSelectedNonEnemyConfirmEvidence
               && HandleCombatContextSupport.HasRecentNonEnemySelection(combatContext, slotIndex);
    }

    foreach (var slotIndex in GetPlayableCombatAttackSlots(observer, combatCardKnowledge))
    {
        if (!blockedCombatNoOpCounts.TryGetValue(slotIndex, out var noOpCount) || noOpCount < 2)
        {
            actions.Add($"select attack slot {slotIndex}");
        }
    }

    foreach (var slotIndex in GetPlayableCombatNonEnemySlots(observer, combatCardKnowledge))
    {
        if (ShouldSuppressNonEnemyReselect(slotIndex))
        {
            continue;
        }

        actions.Add($"select non-enemy slot {slotIndex}");
    }

    var pendingAttackBlocked = pendingSelection?.Kind == AutoCombatCardKind.AttackLike
                               && blockedCombatNoOpCounts.TryGetValue(pendingSelection.SlotIndex, out var pendingNoOpCount)
                               && pendingNoOpCount >= 2;
    if (!pendingAttackBlocked && CanResolveEnemyTargetFromStateAnalysis(observer, combatCardKnowledge, analysis, pendingSelection))
    {
        actions.Add("click enemy");
    }

    if (hasSelectedNonEnemyConfirmEvidence)
    {
        actions.Add("confirm selected non-enemy card");
    }

    actions.Add("click end turn");

    if (HasCombatSelectionToCancelFromAnalysis(observer, combatCardKnowledge, analysis, pendingSelection))
    {
        actions.Add("right-click cancel selected card");
    }

    if (actions.Count == 1)
    {
        actions.Insert(0, "select card from hand");
    }

    actions.Add("wait");
    return actions
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

static IEnumerable<int> GetPlayableCombatAttackSlots(
    ObserverState observer,
    IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
{
    var knowledgeSlots = combatCardKnowledge
        .Where(card => card.SlotIndex is >= 1 and <= 5)
        .Where(card => IsEnemyTargetCombatCard(card) && IsPlayableAtCurrentEnergy(card, observer.PlayerEnergy))
        .Select(static card => card.SlotIndex);
    var observerSlots = observer.CombatHand
        .Where(card => card.SlotIndex is >= 1 and <= 5)
        .Where(card => IsAttackCombatHandCard(card) && IsObservedCombatCardPlayableAtCurrentEnergy(card, observer.PlayerEnergy, combatCardKnowledge))
        .Select(static card => card.SlotIndex);
    return knowledgeSlots
        .Concat(observerSlots)
        .Distinct()
        .OrderBy(static slotIndex => slotIndex);
}

static IEnumerable<int> GetPlayableCombatNonEnemySlots(
    ObserverState observer,
    IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
{
    var knowledgeSlots = combatCardKnowledge
        .Where(card => card.SlotIndex is >= 1 and <= 5)
        .Where(card => CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatCard(card, observer.PlayerEnergy))
        .Select(static card => card.SlotIndex);
    var observerSlots = observer.CombatHand
        .Where(card => card.SlotIndex is >= 1 and <= 5)
        .Where(card => CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatHandCard(card, observer.PlayerEnergy, combatCardKnowledge))
        .Select(static card => card.SlotIndex);
    return knowledgeSlots
        .Concat(observerSlots)
        .Distinct()
        .OrderBy(static slotIndex => slotIndex);
}


static bool CanResolveEnemyTargetFromCurrentState(
    ObserverState observer,
    IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
    string? screenshotPath,
    IReadOnlyList<GuiSmokeHistoryEntry> history)
{
    var analysis = string.IsNullOrWhiteSpace(screenshotPath)
        ? new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown)
        : AutoCombatAnalyzer.Analyze(screenshotPath);
    return CanResolveEnemyTargetFromStateAnalysis(
        observer,
        combatCardKnowledge,
        analysis,
        CombatRuntimeStateSupport.ResolvePendingSelection(observer.Summary, combatCardKnowledge, HandleCombatContextSupport.Reconstruct(history).PendingSelection));
}

static bool CanResolveEnemyTargetFromStateAnalysis(
    ObserverState observer,
    IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
    AutoCombatAnalysis analysis,
    PendingCombatSelection? pendingSelection)
{
    if (CombatRuntimeStateSupport.CanResolveEnemyTarget(observer.Summary, combatCardKnowledge, pendingSelection, analysis))
    {
        return true;
    }

    if (GetCombatEnemyTargetNodes(observer.Summary).Count > 0)
    {
        return true;
    }

    if (analysis.HasTargetArrow)
    {
        return true;
    }

    if (CombatRuntimeStateSupport.RequiresExplicitTargetingBeforeEnemyClick(observer.Summary, combatCardKnowledge))
    {
        return false;
    }

    if (pendingSelection?.Kind == AutoCombatCardKind.AttackLike)
    {
        var pendingCard = observer.CombatHand.FirstOrDefault(card => card.SlotIndex == pendingSelection.SlotIndex);
        if (pendingCard is not null)
        {
            return IsAttackCombatHandCard(pendingCard)
                   && IsObservedCombatCardPlayableAtCurrentEnergy(pendingCard, observer.PlayerEnergy, combatCardKnowledge);
        }

        var pendingKnowledge = combatCardKnowledge.FirstOrDefault(card => card.SlotIndex == pendingSelection.SlotIndex);
        if (pendingKnowledge is not null)
        {
            return IsEnemyTargetCombatCard(pendingKnowledge)
                   && IsPlayableAtCurrentEnergy(pendingKnowledge, observer.PlayerEnergy);
        }
    }

    return analysis.HasSelectedCard
           && analysis.SelectedCardKind == AutoCombatCardKind.AttackLike
           && (GetPlayableCombatAttackSlots(observer, combatCardKnowledge).Any()
               || (observer.CombatHand.Count == 0 && combatCardKnowledge.Count == 0));
}

static IReadOnlyList<ObserverActionNode> GetCombatEnemyTargetNodes(ObserverSummary observer, WindowBounds? windowBounds = null)
{
    return observer.ActionNodes
        .Where(static node => node.Actionable && IsCombatEnemyTargetNode(node))
        .Where(node => windowBounds is null
            ? TryParseNodeBounds(node.ScreenBounds, out _)
            : HasTopLevelActiveNodeBounds(node.ScreenBounds, windowBounds))
        .OrderBy(static node => GetTopLevelNodeSortX(node))
        .ThenBy(static node => GetTopLevelNodeSortY(node))
        .ToArray();
}

static bool IsCombatEnemyTargetNode(ObserverActionNode node)
{
    return string.Equals(node.Kind, "enemy-target", StringComparison.OrdinalIgnoreCase)
           || node.NodeId.StartsWith("enemy-target:", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("enemy", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("적", StringComparison.OrdinalIgnoreCase);
}

static bool HasTopLevelActiveNodeBounds(string? screenBounds, WindowBounds? windowBounds)
{
    if (HasTopLevelUsableLogicalBounds(screenBounds))
    {
        return true;
    }

    return windowBounds is not null && IsTopLevelBoundsInsideWindow(screenBounds, windowBounds);
}

static float GetTopLevelNodeSortX(ObserverActionNode node)
{
    return TryParseNodeBounds(node.ScreenBounds, out var bounds)
        ? bounds.X
        : float.MaxValue;
}

static float GetTopLevelNodeSortY(ObserverActionNode node)
{
    return TryParseNodeBounds(node.ScreenBounds, out var bounds)
        ? bounds.Y
        : float.MaxValue;
}

static bool HasTopLevelUsableLogicalBounds(string? screenBounds)
{
    return TryParseNodeBounds(screenBounds, out var bounds)
           && bounds.X >= 0f
           && bounds.Y >= 0f
           && bounds.Right <= 1920f
           && bounds.Bottom <= 1080f;
}

static bool IsTopLevelBoundsInsideWindow(string? screenBounds, WindowBounds windowBounds)
{
    return TryParseNodeBounds(screenBounds, out var bounds)
           && bounds.Width > 0f
           && bounds.Height > 0f
           && bounds.X >= windowBounds.X
           && bounds.Y >= windowBounds.Y
           && bounds.Right <= windowBounds.X + windowBounds.Width
           && bounds.Bottom <= windowBounds.Y + windowBounds.Height;
}

static bool HasCombatSelectionToCancelFromAnalysis(
    ObserverState observer,
    IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
    AutoCombatAnalysis analysis,
    PendingCombatSelection? pendingSelection)
{
    if (CombatRuntimeStateSupport.HasSelectionToKeep(observer.Summary, combatCardKnowledge))
    {
        return false;
    }

    return analysis.HasSelectedCard
           && !CanResolveEnemyTargetFromStateAnalysis(observer, combatCardKnowledge, analysis, pendingSelection);
}

static string BuildCombatFailureModeHint(
    ObserverState observer,
    IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
{
    var attackSlots = GetPlayableCombatAttackSlots(observer, combatCardKnowledge).ToArray();
    if (attackSlots.Length > 0)
    {
        return $"Only click the enemy after selecting an attack. Current playable attack slots: {string.Join(", ", attackSlots)}. Ignore stale map or reward contamination when the observer still shows combat.";
    }

    var nonEnemySlots = GetPlayableCombatNonEnemySlots(observer, combatCardKnowledge).ToArray();
    if (nonEnemySlots.Length > 0)
    {
        return $"No playable enemy-target attack is currently confirmed. Use a non-enemy slot ({string.Join(", ", nonEnemySlots)}) or end turn, and do not click the enemy without a selected attack.";
    }

    return "No playable enemy-target attack is currently confirmed. Do not click the enemy without a visible target arrow or a matching selected attack lane; prefer end turn instead.";
}

static bool IsAttackCombatHandCard(ObservedCombatHandCard card)
{
    return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
           || card.Name.Contains("STRIKE", StringComparison.OrdinalIgnoreCase)
           || card.Name.Contains("BASH", StringComparison.OrdinalIgnoreCase);
}

static bool IsEnemyTargetCombatCard(CombatCardKnowledgeHint card)
{
    return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
           || string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
           || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase)
           || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
}

static bool IsPlayableAtCurrentEnergy(CombatCardKnowledgeHint card, int? energy)
{
    if (energy is null || card.Cost is null)
    {
        return true;
    }

    if (card.Cost < 0)
    {
        return energy > 0;
    }

    return card.Cost <= energy;
}

static bool IsObservedCombatCardPlayableAtCurrentEnergy(
    ObservedCombatHandCard card,
    int? energy,
    IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
{
    var resolvedCost = ResolveObservedCombatCardCost(card, combatCardKnowledge);
    if (energy is null || resolvedCost is null)
    {
        return true;
    }

    if (resolvedCost < 0)
    {
        return energy > 0;
    }

    return resolvedCost <= energy;
}

static bool IsSkipLikeLabel(string? label)
{
    return ContainsAny(label, "Skip", "건너", "넘기");
}

static bool IsProceedLikeLabel(string? label)
{
    return ContainsAny(label, "Proceed", "Continue", "진행", "계속");
}

static bool IsConfirmLikeLabel(string? label)
{
    return ContainsAny(label, "Confirm", "확인", "선택");
}

static bool IsDismissLikeLabel(string? label)
{
    return ContainsAny(label, "Cancel", "Close", "닫기", "취소", "Back");
}

static bool IsInspectPreviewBounds(string? screenBounds)
{
    return TryParseNodeBounds(screenBounds, out var bounds)
           && bounds.Width <= 120f
           && bounds.Height <= 120f
           && bounds.Y <= 170f
           && bounds.X <= 200f;
}

static bool HasLargeChoiceBounds(string? screenBounds)
{
    return TryParseNodeBounds(screenBounds, out var bounds)
           && bounds.Width >= 260f
           && bounds.Height >= 60f;
}

static bool IsProceedNode(ObserverActionNode node)
{
    return node.Label.Contains("Proceed", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("Continue", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("진행", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("계속", StringComparison.OrdinalIgnoreCase)
           || node.Kind.Contains("proceed", StringComparison.OrdinalIgnoreCase);
}

static bool IsProceedChoice(ObserverChoice choice)
{
    return !string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
           && !string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
           && IsProceedLikeLabel(choice.Label);
}

static bool IsBackNode(ObserverActionNode node)
{
    return node.Label.Contains("Back", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("뒤", StringComparison.OrdinalIgnoreCase)
           || node.Kind.Contains("back", StringComparison.OrdinalIgnoreCase);
}

static bool IsMapNode(ObserverActionNode node)
{
    return node.NodeId.Contains("map", StringComparison.OrdinalIgnoreCase)
           || node.Kind.Contains("map", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("Map", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("지도", StringComparison.OrdinalIgnoreCase);
}

static bool ContainsAny(string? value, params string[] candidates)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return false;
    }

    return candidates.Any(candidate => value.Contains(candidate, StringComparison.OrdinalIgnoreCase));
}

static bool TryParseNodeBounds(string? raw, out RectangleF bounds)
{
    bounds = default;
    if (string.IsNullOrWhiteSpace(raw))
    {
        return false;
    }

    var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length != 4)
    {
        return false;
    }

    if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
        || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
        || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
        || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
    {
        return false;
    }

    if (width <= 0 || height <= 0)
    {
        return false;
    }

    bounds = new RectangleF(x, y, width, height);
    return true;
}

static bool HasStrongMapTransitionEvidence(ObserverState observer)
{
    return GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer)
           && !GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer);
}

static bool HasStrongMapTransitionEvidenceFromScene(ObserverSummary observer, string? sceneSignature)
{
    return !GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(observer)
           && (GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer)
           || (!string.IsNullOrWhiteSpace(sceneSignature)
               && (sceneSignature.Contains("substate:map-transition", StringComparison.OrdinalIgnoreCase)
                   || sceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase))));
}

static RewardMapLayerState BuildRewardMapLayerStateForObserver(ObserverSummary observer, WindowBounds? windowBounds)
{
    var mapContextVisible = GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer)
                            || string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase);
    var rewardBackNavigationAvailable = observer.CurrentChoices.Any(static label =>
                                            label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                                            || label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                                            || label.Contains("뒤로", StringComparison.OrdinalIgnoreCase)
                                            || label.Contains("Back", StringComparison.OrdinalIgnoreCase))
                                        || observer.ActionNodes.Any(static node =>
                                            node.Actionable
                                            && (node.Label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                                                || node.Label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                                                || node.Label.Contains("뒤로", StringComparison.OrdinalIgnoreCase)
                                                || node.Label.Contains("Back", StringComparison.OrdinalIgnoreCase)));
    var activeRewardChoices = observer.Choices.Count(choice => IsCurrentRewardProgressionChoiceForObserver(choice, windowBounds));
    var activeRewardNodes = observer.ActionNodes.Count(node => IsCurrentRewardProgressionNodeForObserver(node, windowBounds));
    var staleRewardChoices = observer.Choices.Count(choice => IsStaleRewardProgressionChoiceForObserver(choice, windowBounds));
    var staleRewardNodes = observer.ActionNodes.Count(node => IsStaleRewardProgressionNodeForObserver(node, windowBounds));
    var rewardScreenHint = GuiSmokeObserverPhaseHeuristics.LooksLikeRewardsState(observer)
                           || string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(observer.VisibleScreen, "rewards", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase)
                           || observer.Choices.Any(static choice => string.Equals(choice.Kind, "card", StringComparison.OrdinalIgnoreCase)
                                                                   || string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
                                                                   || choice.Value?.StartsWith("CARD.", StringComparison.OrdinalIgnoreCase) == true
                                                                   || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true);
    var rewardPanelVisible = (rewardScreenHint && (activeRewardChoices > 0 || activeRewardNodes > 0))
                             || (rewardScreenHint && !mapContextVisible);
    var staleRewardChoicePresent = !rewardPanelVisible && (staleRewardChoices > 0 || staleRewardNodes > 0);
    var staleRewardBoundsPresent = staleRewardChoicePresent;
    var offWindowBoundsReused = observer.Choices.Any(choice => IsOffWindowBoundsForObserver(choice.ScreenBounds, windowBounds))
                              || observer.ActionNodes.Any(node => IsOffWindowBoundsForObserver(node.ScreenBounds, windowBounds));
    return new RewardMapLayerState(
        rewardPanelVisible,
        mapContextVisible,
        rewardBackNavigationAvailable,
        staleRewardChoicePresent,
        staleRewardBoundsPresent,
        offWindowBoundsReused);
}

static bool LooksLikeRewardBackNavigationAffordance(ObserverSummary observer, string? screenshotPath)
{
    if (observer.CurrentChoices.Any(static label =>
            label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
            || label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
            || label.Contains("뒤로", StringComparison.OrdinalIgnoreCase)
            || label.Contains("Back", StringComparison.OrdinalIgnoreCase))
        || observer.ActionNodes.Any(static node =>
            node.Actionable
            && (node.Label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                || node.Label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                || node.Label.Contains("뒤로", StringComparison.OrdinalIgnoreCase)
                || node.Label.Contains("Back", StringComparison.OrdinalIgnoreCase))))
    {
        return true;
    }

    if (string.IsNullOrWhiteSpace(screenshotPath) || !File.Exists(screenshotPath))
    {
        return false;
    }

    var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(screenshotPath);
    return overlayAnalysis.HasBottomLeftBackArrow
           && !overlayAnalysis.HasCentralOverlayPanel
           && (string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase));
}

static bool HasScreenshotClaimableRewardEvidence(ObserverSummary observer, string? screenshotPath)
{
    if (string.IsNullOrWhiteSpace(screenshotPath) || !File.Exists(screenshotPath))
    {
        return false;
    }

    if (!string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(observer.VisibleScreen, "rewards", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    return AutoEventCardGridAnalyzer.Analyze(screenshotPath).HasSelectableCard;
}

static bool IsCurrentRewardProgressionChoiceForObserver(ObserverChoice choice, WindowBounds? windowBounds)
{
    return IsProgressionLikeRewardChoice(choice)
           && HasActiveRewardBoundsForObserver(choice.ScreenBounds, windowBounds);
}

static bool IsCurrentRewardProgressionNodeForObserver(ObserverActionNode node, WindowBounds? windowBounds)
{
    return node.Actionable
           && IsProgressionLikeRewardNode(node)
           && HasActiveRewardBoundsForObserver(node.ScreenBounds, windowBounds);
}

static bool IsStaleRewardProgressionChoiceForObserver(ObserverChoice choice, WindowBounds? windowBounds)
{
    return IsProgressionLikeRewardChoice(choice)
           && !HasActiveRewardBoundsForObserver(choice.ScreenBounds, windowBounds);
}

static bool IsStaleRewardProgressionNodeForObserver(ObserverActionNode node, WindowBounds? windowBounds)
{
    return node.Actionable
           && IsProgressionLikeRewardNode(node)
           && !HasActiveRewardBoundsForObserver(node.ScreenBounds, windowBounds);
}

static bool IsProgressionLikeRewardChoice(ObserverChoice choice)
{
    if (!TryParseScreenBounds(choice.ScreenBounds, out _))
    {
        return false;
    }

    return choice.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
           || choice.Kind.Contains("card", StringComparison.OrdinalIgnoreCase)
           || choice.Label.Contains("골드", StringComparison.OrdinalIgnoreCase)
           || choice.Label.Contains("gold", StringComparison.OrdinalIgnoreCase)
           || choice.Label.Contains("넘기", StringComparison.OrdinalIgnoreCase)
           || choice.Label.Contains("skip", StringComparison.OrdinalIgnoreCase)
           || choice.Label.Contains("proceed", StringComparison.OrdinalIgnoreCase)
           || choice.Label.Contains("continue", StringComparison.OrdinalIgnoreCase)
           || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
           || choice.Value?.StartsWith("CARD.", StringComparison.OrdinalIgnoreCase) == true;
}

static bool IsProgressionLikeRewardNode(ObserverActionNode node)
{
    if (!TryParseScreenBounds(node.ScreenBounds, out _))
    {
        return false;
    }

    return node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
           || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
           || node.Kind.Contains("proceed", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("골드", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("gold", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("넘기", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("skip", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("proceed", StringComparison.OrdinalIgnoreCase)
           || node.Label.Contains("continue", StringComparison.OrdinalIgnoreCase);
}

static bool HasActiveRewardBoundsForObserver(string? screenBounds, WindowBounds? windowBounds)
{
    if (HasUsableLogicalBoundsForObserver(screenBounds))
    {
        return true;
    }

    return windowBounds is not null && IsBoundsInsideWindowForObserver(screenBounds, windowBounds);
}

static bool IsOffWindowBoundsForObserver(string? screenBounds, WindowBounds? windowBounds)
{
    if (string.IsNullOrWhiteSpace(screenBounds) || windowBounds is null || !TryParseScreenBounds(screenBounds, out _))
    {
        return false;
    }

    return !HasUsableLogicalBoundsForObserver(screenBounds)
           && !IsBoundsInsideWindowForObserver(screenBounds, windowBounds);
}

static bool IsBoundsInsideWindowForObserver(string? screenBounds, WindowBounds windowBounds)
{
    if (!TryParseScreenBounds(screenBounds, out var bounds))
    {
        return false;
    }

    return bounds.Right > windowBounds.X
           && bounds.Bottom > windowBounds.Y
           && bounds.X < windowBounds.X + windowBounds.Width
           && bounds.Y < windowBounds.Y + windowBounds.Height;
}

static bool HasUsableLogicalBoundsForObserver(string? raw)
{
    if (!TryParseScreenBounds(raw, out var bounds))
    {
        return false;
    }

    var centerX = bounds.X + bounds.Width / 2f;
    var centerY = bounds.Y + bounds.Height / 2f;
    return centerX >= 0f
           && centerY >= 0f
           && centerX <= 1920f
           && centerY <= 1080f;
}

static int? ResolveObservedCombatCardCost(ObservedCombatHandCard card, IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
{
    if (card.Cost is not null)
    {
        return card.Cost;
    }

    var slotMatch = combatCardKnowledge.FirstOrDefault(candidate => candidate.SlotIndex == card.SlotIndex);
    if (slotMatch?.Cost is not null)
    {
        return slotMatch.Cost;
    }

    var cardKeys = BuildCombatKnowledgeLookupKeys(card.Name);
    if (cardKeys.Count == 0)
    {
        return null;
    }

    return combatCardKnowledge
        .Where(candidate => candidate.Cost is not null)
        .Where(candidate => BuildCombatKnowledgeLookupKeys(candidate.Name).Any(cardKeys.Contains))
        .Select(static candidate => candidate.Cost)
        .FirstOrDefault();
}

static bool IsOverlayCleanupTarget(string? targetLabel)
{
    return string.Equals(targetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "overlay back", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "overlay close", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "overlay backdrop close", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "inspect overlay escape", StringComparison.OrdinalIgnoreCase);
}

static bool IsOverlayChoiceLabel(string? label)
{
    return !string.IsNullOrWhiteSpace(label)
           && (label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
               || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
               || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
}

static bool IsSkipOrProceedLabel(string? label)
{
    return IsSkipLikeLabel(label)
           || IsProceedLikeLabel(label)
           || IsConfirmLikeLabel(label);
}

static bool LooksLikeShopState(ObserverSummary observer)
{
    return string.Equals(observer.CurrentScreen, "shop", StringComparison.OrdinalIgnoreCase)
           || string.Equals(observer.VisibleScreen, "shop", StringComparison.OrdinalIgnoreCase)
           || string.Equals(observer.EncounterKind, "Shop", StringComparison.OrdinalIgnoreCase)
           || string.Equals(observer.ChoiceExtractorPath, "shop", StringComparison.OrdinalIgnoreCase);
}

static bool LooksLikeTreasureState(ObserverSummary observer)
{
    if (string.Equals(observer.EncounterKind, "Treasure", StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    return observer.CurrentChoices.Any(static label =>
        label.Contains("Chest", StringComparison.OrdinalIgnoreCase)
        || label.Contains("\uC0C1\uC790", StringComparison.OrdinalIgnoreCase));
}

static bool LooksLikeRestSiteState(ObserverSummary observer)
{
    if (string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
        || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase)
        || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer))
    {
        return true;
    }

    if (observer.Choices.Any(static choice =>
            !string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
            && (choice.Label.Contains("\uD734\uC2DD", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("Rest", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("\uC7AC\uB828", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("Smith", StringComparison.OrdinalIgnoreCase))))
    {
        return true;
    }

    return observer.Choices.Count == 0
           && observer.CurrentChoices.Any(static label =>
               label.Contains("\uD734\uC2DD", StringComparison.OrdinalIgnoreCase)
               || label.Contains("Rest", StringComparison.OrdinalIgnoreCase)
               || label.Contains("\uC7AC\uB828", StringComparison.OrdinalIgnoreCase)
               || label.Contains("Smith", StringComparison.OrdinalIgnoreCase));
}

static bool LooksLikeRestSiteProceedState(ObserverSummary observer)
{
    if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer))
    {
        return false;
    }

    var onRestSiteScreen = string.Equals(observer.CurrentScreen, "rest-site", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(observer.VisibleScreen, "rest-site", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase);
    if (!onRestSiteScreen)
    {
        return false;
    }

    var hasProceedActionNode = observer.ActionNodes.Any(static node =>
        node.Actionable
        && IsProceedNode(node)
        && TryParseNodeBounds(node.ScreenBounds, out _));
    var hasProceedChoice = observer.Choices.Any(static choice =>
        IsProceedChoice(choice)
        && HasLargeChoiceBounds(choice.ScreenBounds));
    var hasProceedLabel = observer.CurrentChoices.Any(IsProceedLikeLabel);
    if (!hasProceedActionNode && !hasProceedChoice && !hasProceedLabel)
    {
        return false;
    }

    var hasExplicitRestChoice = observer.Choices.Any(static choice =>
        string.Equals(choice.Kind, "rest-option", StringComparison.OrdinalIgnoreCase)
        || string.Equals(choice.BindingKind, "rest-site-option", StringComparison.OrdinalIgnoreCase)
        || (HasLargeChoiceBounds(choice.ScreenBounds)
            && (choice.Label.Contains("\uD734\uC2DD", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("Rest", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("\uC7AC\uB828", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("Smith", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("\uBD80\uD654", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("Hatch", StringComparison.OrdinalIgnoreCase))));
    if (!hasExplicitRestChoice)
    {
        return true;
    }

    return string.Equals(RestSiteObserverSignals.TryGetMetaValue(observer, "restSiteSelectionLastSuccess"), "true", StringComparison.OrdinalIgnoreCase)
           || string.Equals(RestSiteObserverSignals.TryGetMetaValue(observer, "restSiteSelectionLastSignal"), "after-select-success", StringComparison.OrdinalIgnoreCase)
           || (hasProceedLabel
               && !observer.CurrentChoices.Any(static label =>
                   ContainsAny(label, "휴식", "Rest", "재련", "Smith", "부화", "Hatch")));
}

static bool HasExplicitRestSiteChoiceAuthority(ObserverState observer, string? screenshotPath)
{
    return HasRestSiteAuthority(observer.Summary)
           && HasExplicitRestSiteChoiceAffordance(observer.Summary)
           && !RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer.Summary)
           && !AutoRestSiteCardGridAnalyzer.Analyze(screenshotPath ?? string.Empty).HasSelectableCard;
}

static bool HasRestSiteAuthority(ObserverSummary observer)
{
    return string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
           || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase)
           || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer);
}

static bool HasExplicitRestSiteChoiceAffordance(ObserverSummary observer)
{
    return RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(observer);
}

static string[] BuildExplicitRestSiteAllowedActions(ObserverSummary observer)
{
    return RestSiteChoiceSupport.BuildAllowedActions(observer).ToArray();
}

static bool LooksLikeSingleplayerSubmenuState(ObserverSummary observer)
{
    return string.Equals(observer.CurrentScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase)
           || string.Equals(observer.VisibleScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase);
}

static string BuildGoal(GuiSmokePhase phase)
{
    return phase switch
    {
        GuiSmokePhase.WaitMainMenu => "Reach main menu and verify observer currentScreen=main-menu.",
        GuiSmokePhase.EnterRun => "Enter a run using Continue first, otherwise Singleplayer.",
        GuiSmokePhase.WaitCharacterSelect => "Wait until observer currentScreen=character-select.",
        GuiSmokePhase.ChooseCharacter => "Select Ironclad.",
        GuiSmokePhase.Embark => "Click Embark to begin the run.",
        GuiSmokePhase.WaitMap => "Wait until observer logical currentScreen=map. visibleScreen may reach map earlier while reward flow is still active.",
        GuiSmokePhase.HandleRewards => "Resolve the visible reward screen so the run can return to map.",
        GuiSmokePhase.ChooseFirstNode => "Click the first reachable map node.",
        GuiSmokePhase.HandleEvent => "Resolve the event screen. If nothing else is obvious, pick the first visible option.",
        GuiSmokePhase.WaitCombat => "Wait until observer currentScreen=combat and encounter.inCombat=true.",
        GuiSmokePhase.HandleCombat => "Play the combat from the screenshot: choose cards, targets, or end turn until combat resolves.",
        _ => "Complete the scenario.",
    };
}

static string BuildFailureModeHintCore(
    GuiSmokePhase phase,
    ObserverState observer,
    IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
    string? screenshotPath,
    IReadOnlyList<GuiSmokeHistoryEntry> history)
{
    return phase switch
    {
        GuiSmokePhase.EnterRun => "Continue may be absent. Use Singleplayer only if Continue is not visible.",
        GuiSmokePhase.ChooseCharacter => "Do not click Embark before Ironclad is selected.",
        GuiSmokePhase.HandleRewards when LooksLikeInspectOverlayState(observer)
            => "Inspect overlay is not progression. Close it with escape or the overlay dismiss affordance before choosing any reward.",
        GuiSmokePhase.HandleRewards when LooksLikeColorlessCardChoiceState(observer)
            => "This is a colorless reward choice. Pick a visible card first; do not click the small relic inspect icons in the top-left.",
        GuiSmokePhase.HandleRewards when LooksLikeRewardChoiceState(observer)
            => "Reward follow-up is active. Prefer reward cards or skip/proceed over inspect, preview, or detail affordances.",
        GuiSmokePhase.HandleRewards when ShouldPreferRewardProgressionOverMapFallback(observer)
            => "Reward screen authority is stronger than the background map. Claim the visible reward or use skip/proceed first, and ignore any contaminated map arrow until the reward panel disappears.",
        GuiSmokePhase.HandleRewards when HasStrongMapTransitionEvidence(observer)
            => "Map authority is already stronger than the lingering event label. Prefer a reachable node or screenshot-derived map advance instead of repeating proceed/event clicks.",
        GuiSmokePhase.HandleRewards when string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
            => "AI first: use the screenshot as the primary source. If the map is clearly visible, you may click the first reachable node instead of forcing another reward proceed click.",
        GuiSmokePhase.HandleRewards => "Prefer the proceed arrow when the reward can be skipped; otherwise pick a valid reward card.",
        GuiSmokePhase.ChooseFirstNode when HasExplicitRestSiteChoiceAuthority(observer, screenshotPath)
            => "Rest-site explicit choices are foreground-authoritative here. Prefer 휴식/재련/부화 over any map overlay candidate or current-node arrow. If the smith card grid appears afterward, select a card and then confirm.",
        GuiSmokePhase.ChooseFirstNode when LooksLikeRestSiteState(observer.Summary)
            => "Rest site is screenshot-first. If the explicit rest options are visible, choose one of them before any map candidate. If the smith card grid is visible, click one card first and then click the right-side confirm button.",
        GuiSmokePhase.ChooseFirstNode when LooksLikeTreasureState(observer.Summary)
            => "Treasure rooms are not map-node routing. Closed chest: click the center chest. Open chest: click the floating reward icon, then proceed or return to map.",
        GuiSmokePhase.ChooseFirstNode => "Do not click non-reachable map nodes.",
        GuiSmokePhase.HandleEvent when LooksLikeInspectOverlayState(observer)
            => "Inspect overlay is open inside the room flow. Dismiss it before retrying event, reward, or proceed choices.",
        GuiSmokePhase.HandleEvent when LooksLikeColorlessCardChoiceState(observer)
            => "The event follow-up is a colorless card choice. Select a card from the card area; do not treat relic previews as event options.",
        GuiSmokePhase.HandleEvent when LooksLikeRewardChoiceState(observer)
            => "The event has entered a reward substate. Prefer reward cards, reward choices, or skip/proceed over inspect affordances.",
        GuiSmokePhase.HandleEvent when ShouldPreferRewardProgressionOverMapFallback(observer)
            => "Reward screen authority is stronger than the background map. Resolve the visible reward, skip, or proceed affordance before considering any map fallback.",
        GuiSmokePhase.HandleEvent when HasStrongMapTransitionEvidence(observer)
            => "Map evidence is stronger than the stale event label. Prefer reachable map-node or visible-map-advance actions over repeating event progression.",
        GuiSmokePhase.HandleEvent when LooksLikeTreasureState(observer.Summary)
            => "Treasure state can linger as event. Use the screenshot, not stale observer state: closed chest -> center click, open chest -> floating reward icon, then proceed/map.",
        GuiSmokePhase.HandleEvent => "If the event text is ambiguous, choose a large visible progression option, not inspect affordances or detail overlays.",
        GuiSmokePhase.WaitCombat => "Observer must end with combat screen and inCombat=true.",
        GuiSmokePhase.HandleCombat when !CanResolveEnemyTargetFromCurrentState(observer, combatCardKnowledge, screenshotPath, history)
            => BuildCombatFailureModeHint(observer, combatCardKnowledge),
        GuiSmokePhase.HandleCombat => "AI first: read the full combat board from the screenshot. Cards, targets, energy, and end-turn are visual decisions. The harness only executes the click you choose.",
        _ => "Fail closed when screenshot and observer disagree.",
    };
}

static GuiSmokePhase GetPostRewardPhase(GuiSmokeStepDecision decision)
{
    if (KeepsCurrentRoomPhase(decision.TargetLabel))
    {
        return GuiSmokePhase.HandleRewards;
    }

    if (IsReachableNodeTarget(decision.TargetLabel))
    {
        return GuiSmokePhase.WaitCombat;
    }

    return GuiSmokePhase.WaitMap;
}

static GuiSmokePhase GetPostChooseFirstNodePhase(GuiSmokeStepDecision decision)
{
    return IsReachableNodeTarget(decision.TargetLabel)
        ? GuiSmokePhase.WaitCombat
        : GuiSmokePhase.WaitMap;
}

static GuiSmokePhase GetPostHandleEventPhase(GuiSmokeStepDecision decision)
{
    if (KeepsCurrentRoomPhase(decision.TargetLabel))
    {
        return GuiSmokePhase.HandleEvent;
    }

    return IsRoomProgressTarget(decision.TargetLabel)
        ? GetPostChooseFirstNodePhase(decision)
        : GuiSmokePhase.HandleEvent;
}

static bool IsReachableNodeTarget(string? targetLabel)
{
    return string.Equals(targetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase);
}

static bool IsRoomProgressTarget(string? targetLabel)
{
    return IsReachableNodeTarget(targetLabel)
           || string.Equals(targetLabel, "treasure chest center", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "treasure reward icon", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "overlay back", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "overlay close", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "overlay backdrop close", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "rest site: rest", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "rest site: smith", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "rest site: hatch", StringComparison.OrdinalIgnoreCase)
           || (!string.IsNullOrWhiteSpace(targetLabel)
               && targetLabel.StartsWith("rest site: option:", StringComparison.OrdinalIgnoreCase));
}

static bool KeepsCurrentRoomPhase(string? targetLabel)
{
    return string.Equals(targetLabel, "claim reward item", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "reward choice", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "reward card choice", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "colorless card choice", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "reward skip", StringComparison.OrdinalIgnoreCase)
           || IsOverlayCleanupTarget(targetLabel);
}

static bool IsPassiveWaitPhase(GuiSmokePhase phase)
{
    return phase is GuiSmokePhase.WaitMainMenu
        or GuiSmokePhase.WaitCharacterSelect
        or GuiSmokePhase.WaitMap
        or GuiSmokePhase.WaitCombat;
}

static string BuildDecisionWaitFingerprint(GuiSmokePhase phase, string sceneSignature, ObserverState observer)
{
    return string.Join("|",
        phase.ToString(),
        NormalizeSceneSignatureForPlateau(sceneSignature),
        observer.CurrentScreen ?? "unknown",
        observer.VisibleScreen ?? "unknown",
        observer.ChoiceExtractorPath ?? "unknown",
        GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "declaringType") ?? "unknown",
        observer.InventoryId ?? "unknown",
        BuildActionableStateSignature(observer.ActionNodes));
}

static string BuildOverlayLoopFingerprint(string sceneSignature, ObserverState observer)
{
    var overlayLabels = observer.CurrentChoices
        .Where(label => IsOverlayChoiceLabel(label) || IsSkipOrProceedLabel(label))
        .Take(6);
    return string.Join("|",
        NormalizeSceneSignatureForPlateau(sceneSignature),
        observer.CurrentScreen ?? "unknown",
        observer.VisibleScreen ?? "unknown",
        observer.InventoryId ?? "unknown",
        string.Join(";", overlayLabels));
}

static string NormalizeSceneSignatureForPlateau(string sceneSignature)
{
    if (string.IsNullOrWhiteSpace(sceneSignature))
    {
        return "scene:none";
    }

    var parts = sceneSignature
        .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(static part =>
            !part.StartsWith("shot:", StringComparison.OrdinalIgnoreCase)
            && !part.StartsWith("phase:", StringComparison.OrdinalIgnoreCase))
        .ToArray();
    return parts.Length == 0 ? sceneSignature : string.Join("|", parts);
}

static string BuildActionableStateSignature(IReadOnlyList<ObserverActionNode> actionNodes)
{
    return string.Join(";",
        actionNodes
            .Where(static node => node.Actionable)
            .OrderBy(static node => node.NodeId, StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .Select(node => $"{node.NodeId}:{node.Kind}:{node.Label}"));
}

static bool TryClassifyDecisionWaitPlateau(
    GuiSmokePhase phase,
    ObserverState observer,
    int consecutiveDecisionWaitCount,
    out string terminalCause,
    out string message)
{
    var postEmbarkPhase = GuiSmokePhase.Embark;
    var phaseMismatchObserved = phase == GuiSmokePhase.Embark && GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(observer, out postEmbarkPhase);
    var plateauLimit = phaseMismatchObserved ? 2 : GetDecisionWaitPlateauLimit(phase);
    if (consecutiveDecisionWaitCount < plateauLimit)
    {
        terminalCause = string.Empty;
        message = string.Empty;
        return false;
    }

    if (phaseMismatchObserved)
    {
        terminalCause = "phase-mismatch-stall";
        message = $"phase-mismatch-stall phase={phase} observer={observer.CurrentScreen ?? observer.VisibleScreen ?? "null"} reconciledPhase={postEmbarkPhase} waits={consecutiveDecisionWaitCount}";
        return true;
    }

    terminalCause = "decision-wait-plateau";
    message = $"decision-wait-plateau phase={phase} screen={observer.CurrentScreen ?? "null"} waits={consecutiveDecisionWaitCount} inventory={observer.InventoryId ?? "null"}";
    return true;
}

static int GetDecisionWaitPlateauLimit(GuiSmokePhase phase)
{
    return phase == GuiSmokePhase.HandleCombat ? 4 : 5;
}

static int GetSameActionStallLimit(GuiSmokePhase phase, GuiSmokeStepDecision decision)
{
    if (phase == GuiSmokePhase.HandleCombat)
    {
        return 2;
    }

    return decision.TargetLabel switch
    {
        "continue" => 4,
        "singleplayer" => 4,
        "ironclad" => 4,
        "embark" => 4,
        "event progression choice" => 4,
        "first event option" => 4,
        "reward choice" => 3,
        "reward card choice" => 3,
        "colorless card choice" => 3,
        "reward skip" => 3,
        "visible proceed" => 4,
        "visible map advance" => 4,
        "visible reachable node" => 4,
        "first reachable node" => 4,
        "exported reachable map node" => 4,
        "map back" => 4,
        "treasure chest center" => 4,
        "treasure reward icon" => 4,
        "claim reward item" => 3,
        "rest site: smith card" => 4,
        "rest site: smith confirm" => 4,
        "hidden overlay close" => 4,
        "overlay back" => 4,
        "overlay close" => 4,
        "overlay backdrop close" => 4,
        "inspect overlay escape" => 3,
        _ => 2,
    };
}

static bool TryClassifyRewardMapLoop(
    GuiSmokePhase phase,
    GuiSmokeStepRequest request,
    GuiSmokeStepDecision decision,
    out string message)
{
    message = string.Empty;
    if (phase is not GuiSmokePhase.HandleRewards and not GuiSmokePhase.WaitMap)
    {
        return false;
    }

    var rewardMapLayer = BuildRewardMapLayerStateForObserver(request.Observer, request.WindowBounds);
    var mapContextVisible = rewardMapLayer.MapContextVisible
                            || request.SceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase)
                            || request.SceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)
                            || request.SceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase);
    if ((!rewardMapLayer.RewardPanelVisible && !rewardMapLayer.StaleRewardChoicePresent)
        || !mapContextVisible)
    {
        return false;
    }

    if (!IsRewardMapLoopTarget(decision.TargetLabel) && !IsStaleRewardLoopTarget(decision.TargetLabel))
    {
        return false;
    }

    var repeatedLoopCount = 1;
    for (var index = request.History.Count - 1; index >= 0; index -= 1)
    {
        var entry = request.History[index];
        if (entry.Phase is not nameof(GuiSmokePhase.HandleRewards) and not nameof(GuiSmokePhase.WaitMap))
        {
            break;
        }

        if (IsRewardMapLoopTarget(entry.TargetLabel) || IsStaleRewardLoopTarget(entry.TargetLabel))
        {
            repeatedLoopCount += 1;
            continue;
        }

        if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
            || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
            || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        break;
    }

    if (repeatedLoopCount < 3)
    {
        return false;
    }

    message = $"reward-map-loop phase={phase} target={decision.TargetLabel ?? "null"} observer={request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "null"} repeats={repeatedLoopCount} staleChoice={rewardMapLayer.StaleRewardChoicePresent} staleBounds={rewardMapLayer.StaleRewardBoundsPresent} backNav={rewardMapLayer.RewardBackNavigationAvailable} mapVisible={mapContextVisible} offWindow={rewardMapLayer.OffWindowBoundsReused}";
    return true;
}

static bool TryClassifyMapTransitionStall(
    GuiSmokePhase phase,
    GuiSmokeStepRequest request,
    GuiSmokeStepDecision decision,
    out string message)
{
    message = string.Empty;
    if (phase is not GuiSmokePhase.HandleEvent and not GuiSmokePhase.WaitMap and not GuiSmokePhase.ChooseFirstNode)
    {
        return false;
    }

    if (!HasStrongMapTransitionEvidenceFromScene(request.Observer, request.SceneSignature))
    {
        return false;
    }

    if (!IsMapTransitionLoopTarget(decision.TargetLabel))
    {
        return false;
    }

    var repeatedLoopCount = 1;
    for (var index = request.History.Count - 1; index >= 0; index -= 1)
    {
        var entry = request.History[index];
        if (entry.Phase is not nameof(GuiSmokePhase.HandleEvent) and not nameof(GuiSmokePhase.WaitMap) and not nameof(GuiSmokePhase.ChooseFirstNode))
        {
            break;
        }

        if (IsMapTransitionLoopTarget(entry.TargetLabel))
        {
            repeatedLoopCount += 1;
            continue;
        }

        if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
            || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
            || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        break;
    }

    if (repeatedLoopCount < 3)
    {
        return false;
    }

    message = $"map-transition-stall phase={phase} target={decision.TargetLabel ?? "null"} observer={request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "null"} repeats={repeatedLoopCount}";
    return true;
}

static bool TryClassifyMapOverlayNoOpLoop(
    GuiSmokePhase phase,
    GuiSmokeStepRequest request,
    GuiSmokeStepDecision decision,
    out string message)
{
    message = string.Empty;
    if (phase is not GuiSmokePhase.HandleEvent and not GuiSmokePhase.WaitMap and not GuiSmokePhase.ChooseFirstNode)
    {
        return false;
    }

    var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
    if (!mapOverlayState.ForegroundVisible
        || !mapOverlayState.StaleEventChoicePresent
        || (!mapOverlayState.CurrentNodeArrowVisible && !mapOverlayState.MapBackNavigationAvailable))
    {
        return false;
    }

    if (!IsMapOverlayLoopTarget(decision.TargetLabel))
    {
        return false;
    }

    var repeatedLoopCount = 1;
    var repeatedCurrentNodeArrowClick = string.Equals(decision.TargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
    for (var index = request.History.Count - 1; index >= 0; index -= 1)
    {
        var entry = request.History[index];
        if (entry.Phase is not nameof(GuiSmokePhase.HandleEvent) and not nameof(GuiSmokePhase.WaitMap) and not nameof(GuiSmokePhase.ChooseFirstNode))
        {
            break;
        }

        if (IsMapOverlayLoopTarget(entry.TargetLabel))
        {
            repeatedLoopCount += 1;
            if (string.Equals(entry.TargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase))
            {
                repeatedCurrentNodeArrowClick += 1;
            }

            continue;
        }

        if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
            || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
            || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        break;
    }

    if (repeatedLoopCount < 4 && repeatedCurrentNodeArrowClick < 2)
    {
        return false;
    }

    message = $"map-overlay-noop-loop phase={phase} target={decision.TargetLabel ?? "null"} observer={request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "null"} repeats={repeatedLoopCount} staleEventChoice={mapOverlayState.StaleEventChoicePresent} backNav={mapOverlayState.MapBackNavigationAvailable} currentArrow={mapOverlayState.CurrentNodeArrowVisible} exportedReachable={mapOverlayState.ExportedReachableNodeCandidatePresent}";
    return true;
}

static bool TryClassifyCombatNoOpLoop(
    GuiSmokePhase phase,
    GuiSmokeStepRequest request,
    GuiSmokeStepDecision decision,
    out string message)
{
    message = string.Empty;
    if (phase != GuiSmokePhase.HandleCombat)
    {
        return false;
    }

    var analysis = AutoDecisionProvider.PeekCombatNoOpLoop(request.History);
    if (!analysis.LoopDetected || !analysis.BlockedSlotIndex.HasValue)
    {
        return false;
    }

    var blockedSlot = analysis.BlockedSlotIndex.Value;
    var loopTarget = $"combat select attack slot {blockedSlot}";
    var pendingSelection = AutoDecisionProvider.TryPeekPendingCombatSelection(request.History);
    var decisionRepeatsLoop = string.Equals(decision.TargetLabel, loopTarget, StringComparison.OrdinalIgnoreCase)
                              || ((string.Equals(decision.TargetLabel, "auto-target enemy", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(decision.TargetLabel, "auto-target enemy recenter", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(decision.TargetLabel, "auto-target enemy alternate", StringComparison.OrdinalIgnoreCase)
                                   || (decision.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) ?? false))
                                  && pendingSelection?.Kind == AutoCombatCardKind.AttackLike
                                  && pendingSelection.SlotIndex == blockedSlot);
    if (!decisionRepeatsLoop)
    {
        return false;
    }

    message = $"combat-noop-loop phase={phase} blockedSlot={blockedSlot} energy={request.Observer.PlayerEnergy?.ToString(CultureInfo.InvariantCulture) ?? "null"} repeats={analysis.RepeatedSelectionCount}";
    return true;
}

static bool TryClassifyRestSitePostClickNoOp(
    GuiSmokePhase phase,
    GuiSmokeStepRequest request,
    GuiSmokeStepDecision decision,
    out string message)
{
    message = string.Empty;
    if (phase is not GuiSmokePhase.ChooseFirstNode and not GuiSmokePhase.WaitMap)
    {
        return false;
    }

    if (!string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase)
        || !IsRestSitePostClickDecisionRisk(decision.DecisionRisk)
        || !AutoDecisionProvider.IsExplicitRestSiteOptionTargetLabel(decision.TargetLabel)
        || (!AutoDecisionProvider.HasExplicitRestSiteChoiceAuthorityForRequest(request)
            && !AutoDecisionProvider.HasRecentRestSiteExplicitClickForRequest(request, decision.TargetLabel)))
    {
        return false;
    }

    var evidence = RestSiteObserverSignals.BuildPostClickEvidence(request.Observer, decision.TargetLabel);
    message = $"{decision.DecisionRisk ?? "rest-site-post-click-noop"} phase={phase} target={decision.TargetLabel ?? "null"} observer={request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "null"} fingerprint={RestSiteChoiceSupport.BuildExplicitChoiceFingerprint(request.Observer)} outcome={evidence.Outcome ?? "null"} outcomeEvidence={evidence.OutcomeEvidence ?? "null"} currentStatus={evidence.CurrentStatus ?? "null"} currentOption={evidence.CurrentOptionId ?? "null"} lastSignal={evidence.LastSignal ?? "null"} lastOption={evidence.LastOptionId ?? "null"} upgradeScreenVisible={evidence.UpgradeScreenVisible} smithGridVisible={evidence.SmithGridVisible} smithConfirmVisible={evidence.SmithConfirmVisible} explicitChoices={evidence.ExplicitChoiceVisible} observerMiss={evidence.UpgradeChoiceObserverMiss}";
    return true;
}

static bool IsRestSitePostClickDecisionRisk(string? decisionRisk)
{
    return string.Equals(decisionRisk, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase)
           || string.Equals(decisionRisk, "rest-site-selection-failed", StringComparison.OrdinalIgnoreCase)
           || string.Equals(decisionRisk, "rest-site-grid-not-visible-after-selection", StringComparison.OrdinalIgnoreCase)
           || string.Equals(decisionRisk, "rest-site-grid-observer-miss", StringComparison.OrdinalIgnoreCase);
}

static bool IsMapTransitionLoopTarget(string? targetLabel)
{
    return string.Equals(targetLabel, "event progression choice", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "proceed after resolving rewards", StringComparison.OrdinalIgnoreCase);
}

static bool IsMapOverlayLoopTarget(string? targetLabel)
{
    return string.Equals(targetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "map back", StringComparison.OrdinalIgnoreCase);
}

static bool IsRewardMapLoopTarget(string? targetLabel)
{
    return string.Equals(targetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase);
}

static bool IsStaleRewardLoopTarget(string? targetLabel)
{
    return string.Equals(targetLabel, "reward skip", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "reward choice", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "claim reward item", StringComparison.OrdinalIgnoreCase);
}

static bool ShouldAllowRewardMapRecovery(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
{
    if (!IsRewardMapRecoveryTarget(decision.TargetLabel))
    {
        return false;
    }

    return CountRecentRewardMapRecoveryAttempts(request.History) < 2;
}

static bool IsRewardMapRecoveryTarget(string? targetLabel)
{
    return string.Equals(targetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
           || string.Equals(targetLabel, "reward back", StringComparison.OrdinalIgnoreCase);
}

static int CountRecentRewardMapRecoveryAttempts(IReadOnlyList<GuiSmokeHistoryEntry> history)
{
    var count = 0;
    for (var index = history.Count - 1; index >= 0; index -= 1)
    {
        var entry = history[index];
        if (string.Equals(entry.Action, "reward-map-recovery", StringComparison.OrdinalIgnoreCase))
        {
            count += 1;
            continue;
        }

        if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
            || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
            || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        break;
    }

    return count;
}

static bool TryParseScreenBounds(string? raw, out RectangleF bounds)
{
    bounds = default;
    if (string.IsNullOrWhiteSpace(raw))
    {
        return false;
    }

    var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length != 4)
    {
        return false;
    }

    if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
        || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
        || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
        || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
    {
        return false;
    }

    if (width <= 0 || height <= 0)
    {
        return false;
    }

    bounds = new RectangleF(x, y, width, height);
    return true;
}

static bool TryAdvanceAlternateBranch(
    GuiSmokePhase phase,
    ObserverState observer,
    List<GuiSmokeHistoryEntry> history,
    ArtifactRecorder logger,
    int stepIndex,
    bool isLongRun,
    out GuiSmokePhase nextPhase)
{
    nextPhase = phase;

    if (phase == GuiSmokePhase.WaitCharacterSelect)
    {
        if (LooksLikeSingleplayerSubmenuState(observer.Summary))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-singleplayer-submenu", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-singleplayer-submenu", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.EnterRun;
            return true;
        }

        if (LooksLikeShopState(observer.Summary) || LooksLikeRestSiteState(observer.Summary))
        {
            var branchKind = LooksLikeRestSiteState(observer.Summary) ? "branch-rest-site" : "branch-shop";
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), branchKind, null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), branchKind, observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (GuiSmokeObserverPhaseHeuristics.LooksLikeRewardsState(observer.Summary))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rewards", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rewards", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleRewards;
            return true;
        }

        if (GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-map", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-map", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-combat", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-combat", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleCombat;
            return true;
        }

        if (GuiSmokeObserverPhaseHeuristics.LooksLikeEventState(
                observer.Summary,
                GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "declaringType"),
                GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "instanceType")))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-event", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-event", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleEvent;
            return true;
        }
    }

    if (phase == GuiSmokePhase.Embark && GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(observer, out var postEmbarkPhase))
    {
        var branchKind = postEmbarkPhase switch
        {
            GuiSmokePhase.HandleRewards => "branch-rewards",
            GuiSmokePhase.HandleCombat => "branch-combat",
            GuiSmokePhase.HandleEvent => "branch-event",
            GuiSmokePhase.ChooseFirstNode when LooksLikeRestSiteState(observer.Summary) => "branch-rest-site",
            GuiSmokePhase.ChooseFirstNode when LooksLikeShopState(observer.Summary) => "branch-shop",
            GuiSmokePhase.ChooseFirstNode => "branch-map",
            _ => "branch-room",
        };
        history.Add(new GuiSmokeHistoryEntry(phase.ToString(), branchKind, null, DateTimeOffset.UtcNow));
        logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), branchKind, observer.CurrentScreen, observer.InCombat, null));
        nextPhase = postEmbarkPhase;
        return true;
    }

    if (phase == GuiSmokePhase.WaitMainMenu)
    {
        if (string.Equals(observer.CurrentScreen, "shop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.VisibleScreen, "shop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.EncounterKind, "Shop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.ChoiceExtractorPath, "shop", StringComparison.OrdinalIgnoreCase))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-shop", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-shop", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (string.Equals(observer.CurrentScreen, "character-select", StringComparison.OrdinalIgnoreCase))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-character-select", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-character-select", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseCharacter;
            return true;
        }

        if (string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rewards", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rewards", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleRewards;
            return true;
        }

        if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer.Summary))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rest-site-upgrade", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rest-site-upgrade", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (LooksLikeRestSiteProceedState(observer.Summary))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rest-site-proceed", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rest-site-proceed", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-map", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-map", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (string.Equals(observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase) && observer.InCombat == true)
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-combat", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-combat", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleCombat;
            return true;
        }

        if (GuiSmokeObserverPhaseHeuristics.LooksLikeEventState(
                observer.Summary,
                GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "declaringType"),
                GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "instanceType")))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-event", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-event", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleEvent;
            return true;
        }
    }

    if (phase == GuiSmokePhase.WaitMap)
    {
        if (string.Equals(observer.CurrentScreen, "shop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.VisibleScreen, "shop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.EncounterKind, "Shop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.ChoiceExtractorPath, "shop", StringComparison.OrdinalIgnoreCase))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-shop", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-shop", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rewards", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rewards", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleRewards;
            return true;
        }

        if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer.Summary))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rest-site-upgrade", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rest-site-upgrade", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (LooksLikeRestSiteProceedState(observer.Summary))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rest-site-proceed", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rest-site-proceed", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-map", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-map", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (string.Equals(observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase) && observer.InCombat == true)
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-combat", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-combat", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleCombat;
            return true;
        }

        if (GuiSmokeObserverPhaseHeuristics.LooksLikeEventState(
                observer.Summary,
                GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "declaringType"),
                GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "instanceType")))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-event", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-event", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleEvent;
            return true;
        }
    }

    if (phase == GuiSmokePhase.ChooseFirstNode || phase == GuiSmokePhase.WaitCombat)
    {
        if (string.Equals(observer.CurrentScreen, "shop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.VisibleScreen, "shop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.EncounterKind, "Shop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.ChoiceExtractorPath, "shop", StringComparison.OrdinalIgnoreCase))
        {
            if (phase == GuiSmokePhase.ChooseFirstNode)
            {
                return false;
            }

            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-shop", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-shop", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-rewards", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-rewards", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleRewards;
            return true;
        }

        if (GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer) && phase == GuiSmokePhase.WaitCombat)
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-map", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-map", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (GuiSmokeObserverPhaseHeuristics.LooksLikeEventState(
                observer.Summary,
                GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "declaringType"),
                GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "instanceType")))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-event", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-event", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleEvent;
            return true;
        }
    }

    if (phase == GuiSmokePhase.HandleEvent)
    {
        if (GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "event-resolved-map", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "event-resolved-map", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (string.Equals(observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase) && observer.InCombat == true)
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "event-resolved-combat", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "event-resolved-combat", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleCombat;
            return true;
        }

        if (string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "event-resolved-rewards", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "event-resolved-rewards", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleRewards;
            return true;
        }
    }

    if (phase == GuiSmokePhase.HandleCombat)
    {
        if (string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "combat-resolved-rewards", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "combat-resolved-rewards", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = isLongRun
                ? GuiSmokePhase.HandleRewards
                : GuiSmokePhase.Completed;
            return true;
        }

        if (string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase) && observer.InCombat != true)
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "combat-resolved-map", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "combat-resolved-map", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = isLongRun
                ? GuiSmokePhase.ChooseFirstNode
                : GuiSmokePhase.Completed;
            return true;
        }
    }

    return false;
}

static int IncrementAttempt(Dictionary<GuiSmokePhase, int> attemptsByPhase, GuiSmokePhase phase)
{
    if (!attemptsByPhase.TryGetValue(phase, out var current))
    {
        current = 0;
    }

    current += 1;
    attemptsByPhase[phase] = current;
    return current;
}

static string ComputeFileFingerprint(string path)
{
    return GuiSmokeScreenshotAnalysisCache.GetOrCreate("file-fingerprint", path, () =>
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(stream);
            return Convert.ToHexString(hash.AsSpan(0, 8));
        }
        catch
        {
            return "no-image";
        }
    });
}

static void LogHarness(string message)
{
    var line = $"[gui-smoke {DateTimeOffset.Now:HH:mm:ss}] {message}";
    Console.WriteLine(line);
    GuiSmokeShared.HarnessLogSink?.Invoke(line);
}

static string DescribeObserverHuman(ObserverState observer)
{
    var logical = observer.CurrentScreen ?? "null";
    var visible = observer.VisibleScreen ?? "null";
    var inCombat = observer.InCombat?.ToString() ?? "null";
    var sceneReady = observer.SceneReady?.ToString() ?? "null";
    var sceneStability = observer.SceneStability ?? "null";
    var hp = observer.Summary.PlayerCurrentHp is not null && observer.Summary.PlayerMaxHp is not null
        ? $"{observer.Summary.PlayerCurrentHp}/{observer.Summary.PlayerMaxHp}"
        : "null";
    var energy = observer.PlayerEnergy?.ToString(CultureInfo.InvariantCulture) ?? "null";
    var hand = observer.CombatHand.Count == 0
        ? "-"
        : string.Join(", ", observer.CombatHand.Take(4).Select(card => $"{card.SlotIndex}:{card.Type ?? "?"}/{card.Cost?.ToString(CultureInfo.InvariantCulture) ?? "?"}"));
    var encounter = observer.EncounterKind ?? "null";
    var extractor = observer.ChoiceExtractorPath ?? "null";
    var choices = observer.Summary.CurrentChoices.Take(4).ToArray();
    var choiceText = choices.Length == 0 ? "-" : string.Join(", ", choices);
    return $"logical={logical} visible={visible} ready={sceneReady} stability={sceneStability} inCombat={inCombat} hp={hp} energy={energy} hand=[{hand}] encounter={encounter} extractor={extractor} choices=[{choiceText}]";
}

static void AppendProgressIfLongRun(bool isLongRun, ArtifactRecorder logger, GuiSmokeStepProgress progress)
{
    if (!isLongRun)
    {
        return;
    }

    logger.AppendProgress(progress);
}

static GuiSmokeStepProgress EvaluateStepProgress(
    int stepIndex,
    GuiSmokePhase phase,
    string sceneSignature,
    ObserverState observer,
    ObserverState? postActionObserver,
    GuiSmokeStepDecision? decision,
    bool firstSeenScene,
    string reasoningMode,
    bool recipeRecorded,
    int sameActionStallCount,
    params string[] extraSignals)
{
    var observerSignals = new List<string>();
    var suppressRoomSubstateHeuristics = ShouldSuppressRoomSubstateHeuristics(phase, observer);
    if (observer.SceneReady is not null)
    {
        observerSignals.Add(observer.SceneReady == true ? "scene-ready-true" : "scene-ready-false");
    }

    if (!string.IsNullOrWhiteSpace(observer.SceneAuthority))
    {
        observerSignals.Add($"scene-authority:{observer.SceneAuthority}");
    }

    if (phase == GuiSmokePhase.HandleCombat)
    {
        if (observer.PlayerEnergy is not null)
        {
            observerSignals.Add("combat-energy");
        }

        if (observer.CombatHand.Count > 0)
        {
            observerSignals.Add("combat-hand");
        }
    }

    if (IsSpecificExtractorPath(observer.ChoiceExtractorPath))
    {
        observerSignals.Add($"choice-extractor:{observer.ChoiceExtractorPath}");
    }

    if (!suppressRoomSubstateHeuristics)
    {
        if (sceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase))
        {
            observerSignals.Add("map-background-visible");
        }

        if (sceneSignature.Contains("layer:map-overlay-foreground", StringComparison.OrdinalIgnoreCase))
        {
            observerSignals.Add("map-overlay-visible");
        }

        if (sceneSignature.Contains("map-back-navigation-available", StringComparison.OrdinalIgnoreCase))
        {
            observerSignals.Add("map-back-navigation-available");
        }

        if (sceneSignature.Contains("stale:event-choice", StringComparison.OrdinalIgnoreCase))
        {
            observerSignals.Add("stale-event-choice");
        }

        if (sceneSignature.Contains("current-node-arrow-visible", StringComparison.OrdinalIgnoreCase))
        {
            observerSignals.Add("current-node-arrow-visible");
        }

        if (sceneSignature.Contains("reachable-node-candidate-present", StringComparison.OrdinalIgnoreCase))
        {
            observerSignals.Add("reachable-node-candidate-present");
        }

        if (sceneSignature.Contains("exported-reachable-node-present", StringComparison.OrdinalIgnoreCase))
        {
            observerSignals.Add("exported-reachable-node-present");
        }

        if (sceneSignature.Contains("layer:reward-back-nav", StringComparison.OrdinalIgnoreCase))
        {
            observerSignals.Add("reward-back-navigation-available");
        }

        if (sceneSignature.Contains("stale:reward-choice", StringComparison.OrdinalIgnoreCase))
        {
            observerSignals.Add("stale-reward-choice");
        }

        if (sceneSignature.Contains("stale:reward-bounds", StringComparison.OrdinalIgnoreCase))
        {
            observerSignals.Add("stale-reward-bounds");
        }

        if (sceneSignature.Contains("reward:claimable", StringComparison.OrdinalIgnoreCase))
        {
            observerSignals.Add("claimable-reward-present");
        }

        if (LooksLikeInspectOverlayState(observer))
        {
            observerSignals.Add("inspect-overlay");
        }

        if (LooksLikeRewardChoiceState(observer))
        {
            observerSignals.Add("reward-choice");
        }

        if (LooksLikeColorlessCardChoiceState(observer))
        {
            observerSignals.Add("colorless-card-choice");
        }

        if (HasStrongRewardScreenAuthority(observer))
        {
            observerSignals.Add("reward-screen-authority");
        }

        if (HasExplicitRewardProgressionAffordance(observer.Summary))
        {
            observerSignals.Add("reward-explicit-progression");
        }

        if (HasStrongMapTransitionEvidence(observer)
            && !string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
            && !ShouldPreferRewardProgressionOverMapFallback(observer))
        {
            observerSignals.Add("map-transition-evidence");
        }
    }

    if (postActionObserver is not null && HasMeaningfulObserverDelta(observer, postActionObserver))
    {
        observerSignals.Add("post-action-delta");
    }

    foreach (var signal in extraSignals)
    {
        if (!string.IsNullOrWhiteSpace(signal))
        {
            observerSignals.Add(signal);
        }
    }

    var actuatorSignals = new List<string>();
    if (decision is not null && string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase))
    {
        actuatorSignals.Add($"action:{decision.ActionKind ?? "unknown"}");
        if (!string.IsNullOrWhiteSpace(decision.TargetLabel))
        {
            actuatorSignals.Add($"target:{decision.TargetLabel}");
        }

        if (IsRewardMapRecoveryTarget(decision.TargetLabel))
        {
            actuatorSignals.Add("map-node-candidate-chosen");
        }

        if (sameActionStallCount == 0)
        {
            actuatorSignals.Add("no-repeat-stall");
        }

        if (observer.SceneReady != false)
        {
            actuatorSignals.Add("scene-safe");
        }

        if (postActionObserver is not null && HasMeaningfulObserverDelta(observer, postActionObserver))
        {
            actuatorSignals.Add("post-action-delta");
            if (IsRewardMapRecoveryTarget(decision.TargetLabel))
            {
                actuatorSignals.Add("post-click-recapture-observed");
            }
        }

        if (string.Equals(decision.TargetLabel, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase)
            && postActionObserver is not null
            && LooksLikeNonEnemyConfirmSuccess(observer, postActionObserver))
        {
            actuatorSignals.Add("non-enemy-confirmed");
        }
    }

    if (recipeRecorded)
    {
        actuatorSignals.Add("recipe-recorded");
    }

    var observerProgress = observerSignals.Any(static signal =>
        signal == "scene-ready-true"
        || signal == "combat-energy"
        || signal == "combat-hand"
        || signal == "post-action-delta"
        || signal.StartsWith("choice-extractor:", StringComparison.Ordinal));
    var actuatorProgress = actuatorSignals.Any(static signal =>
        signal == "post-action-delta"
        || signal == "non-enemy-confirmed"
        || signal == "recipe-recorded");
    return new GuiSmokeStepProgress(
        DateTimeOffset.UtcNow,
        stepIndex,
        phase.ToString(),
        sceneSignature,
        observer.CurrentScreen,
        postActionObserver?.CurrentScreen,
        decision?.TargetLabel,
        observerProgress,
        actuatorProgress,
        firstSeenScene,
        string.Equals(reasoningMode, "semantic", StringComparison.OrdinalIgnoreCase),
        recipeRecorded,
        observerSignals,
        actuatorSignals);
}

static bool ShouldGrantCombatNoOpProbeGrace(
    GuiSmokePhase phase,
    ObserverState before,
    ObserverState after,
    GuiSmokeStepDecision decision)
{
    if (phase != GuiSmokePhase.HandleCombat
        || !string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase)
        || !AutoDecisionProvider.IsCombatNoOpSensitiveTarget(decision.TargetLabel))
    {
        return false;
    }

    return before.InCombat == true
           && after.InCombat == true
           && !HasMeaningfulObserverDelta(before, after)
           && string.Equals(after.CurrentScreen ?? after.VisibleScreen, "combat", StringComparison.OrdinalIgnoreCase);
}

static bool IsSpecificExtractorPath(string? choiceExtractorPath)
{
    if (string.IsNullOrWhiteSpace(choiceExtractorPath))
    {
        return false;
    }

    return !string.Equals(choiceExtractorPath, "generic", StringComparison.OrdinalIgnoreCase)
           && !string.Equals(choiceExtractorPath, "unknown", StringComparison.OrdinalIgnoreCase);
}

static bool HasMeaningfulObserverDelta(ObserverState before, ObserverState after)
{
    return !string.Equals(before.CurrentScreen, after.CurrentScreen, StringComparison.OrdinalIgnoreCase)
           || !string.Equals(before.VisibleScreen, after.VisibleScreen, StringComparison.OrdinalIgnoreCase)
           || before.InCombat != after.InCombat
           || before.PlayerEnergy != after.PlayerEnergy
           || before.CombatHand.Count != after.CombatHand.Count
           || !string.Equals(before.InventoryId, after.InventoryId, StringComparison.Ordinal);
}

static bool TryRecordCombatNoOpObservation(
    GuiSmokePhase phase,
    ObserverState before,
    ObserverState after,
    GuiSmokeStepDecision decision,
    List<GuiSmokeHistoryEntry> history,
    ArtifactRecorder logger,
    int stepIndex,
    out string signal)
{
    signal = string.Empty;
    if (phase != GuiSmokePhase.HandleCombat
        || !string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase)
        || !AutoDecisionProvider.IsCombatNoOpSensitiveTarget(decision.TargetLabel))
    {
        return false;
    }

    if (before.InCombat != true
        || after.InCombat != true
        || HasMeaningfulObserverDelta(before, after)
        || !string.Equals(after.CurrentScreen ?? after.VisibleScreen, "combat", StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    var laneLabel = AutoDecisionProvider.ResolveCombatLaneLabel(decision.TargetLabel, history) ?? decision.TargetLabel ?? "combat";
    history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "combat-noop", laneLabel, DateTimeOffset.UtcNow));
    logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "combat-noop", after.CurrentScreen, after.InCombat, laneLabel));
    signal = laneLabel.StartsWith("combat lane slot ", StringComparison.OrdinalIgnoreCase)
        ? $"combat-noop-observed:{laneLabel}"
        : "combat-noop-observed";
    LogHarness($"step={stepIndex} observed combat-noop target={decision.TargetLabel ?? "null"} lane={laneLabel} screen={after.CurrentScreen ?? after.VisibleScreen ?? "null"}");
    return true;
}

static bool LooksLikeNonEnemyConfirmSuccess(ObserverState before, ObserverState after)
{
    var energySpent = before.PlayerEnergy is not null
                      && after.PlayerEnergy is not null
                      && after.PlayerEnergy < before.PlayerEnergy;
    var handCountDropped = after.CombatHand.Count < before.CombatHand.Count;
    var beforeRuntime = CombatRuntimeStateSupport.Read(before.Summary, Array.Empty<CombatCardKnowledgeHint>());
    var afterRuntime = CombatRuntimeStateSupport.Read(after.Summary, Array.Empty<CombatCardKnowledgeHint>());
    var finishedCardChanged = !string.IsNullOrWhiteSpace(afterRuntime.LastCardPlayFinishedCardId)
                              && !string.Equals(beforeRuntime.LastCardPlayFinishedCardId, afterRuntime.LastCardPlayFinishedCardId, StringComparison.OrdinalIgnoreCase);
    var pendingClearedWithMeaningfulDelta = beforeRuntime.PendingSelection?.Kind == AutoCombatCardKind.DefendLike
                                            && afterRuntime.PendingSelection is null
                                            && beforeRuntime.CardPlayPending == true
                                            && afterRuntime.CardPlayPending == false
                                            && (energySpent || handCountDropped || finishedCardChanged);
    return energySpent
           || handCountDropped
           || finishedCardChanged
           || pendingClearedWithMeaningfulDelta;
}

static string DescribeWindow(WindowCaptureTarget target)
{
    return $"{target.Title} fallback={target.IsFallback} minimized={target.IsMinimized} bounds={DescribeBounds(target.Bounds)}";
}

static string DescribeBounds(Rectangle bounds)
{
    return $"{bounds.X},{bounds.Y},{bounds.Width},{bounds.Height}";
}

static void ValidateDecision(GuiSmokePhase phase, GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
{
    if (!string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Only act decisions are valid here.");
    }

    if (string.Equals(decision.ActionKind, "click", StringComparison.OrdinalIgnoreCase))
    {
        if (decision.NormalizedX is null || decision.NormalizedY is null)
        {
            throw new InvalidOperationException("Click decision requires normalized coordinates.");
        }

        if (decision.NormalizedX < 0 || decision.NormalizedX > 1 || decision.NormalizedY < 0 || decision.NormalizedY > 1)
        {
            throw new InvalidOperationException("Normalized coordinates must be within [0,1].");
        }
    }
    else if (string.Equals(decision.ActionKind, "click-current", StringComparison.OrdinalIgnoreCase)
             || string.Equals(decision.ActionKind, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase))
    {
        if (decision.NormalizedX is not null || decision.NormalizedY is not null)
        {
            throw new InvalidOperationException($"{decision.ActionKind} decision must not provide normalized coordinates.");
        }
    }
    else if (string.Equals(decision.ActionKind, "right-click", StringComparison.OrdinalIgnoreCase))
    {
        if ((decision.NormalizedX is null) != (decision.NormalizedY is null))
        {
            throw new InvalidOperationException("right-click decision must provide both normalized coordinates or neither.");
        }

        if (decision.NormalizedX is not null
            && (decision.NormalizedX < 0 || decision.NormalizedX > 1 || decision.NormalizedY < 0 || decision.NormalizedY > 1))
        {
            throw new InvalidOperationException("Normalized coordinates must be within [0,1].");
        }
    }
    else if (string.Equals(decision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase))
    {
        if (string.IsNullOrWhiteSpace(decision.KeyText))
        {
            throw new InvalidOperationException("press-key decision requires keyText.");
        }
    }
    else
    {
        throw new InvalidOperationException("Only click, click-current, confirm-non-enemy, right-click, and press-key actionKind are supported.");
    }

    if (request.AllowedActions.Length == 1 && string.Equals(request.AllowedActions[0], "wait", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Phase {phase} does not allow actions.");
    }
}

static void EnsureGameNotRunning()
{
    if (WindowLocator.TryFindSts2Window() is not null)
    {
        throw new InvalidOperationException("Slay the Spire 2 appears to be running. Close the game before deploy/launch.");
    }
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
        return ResolveCliPath(explicitPath, workspaceRoot);
    }

    var samplePath = Path.Combine(workspaceRoot, "config", "ai-companion.sample.json");
    return File.Exists(samplePath) ? samplePath : null;
}

static string ResolveCliPath(string explicitPath, string workspaceRoot)
{
    if (string.IsNullOrWhiteSpace(explicitPath))
    {
        throw new InvalidOperationException("A non-empty CLI path is required.");
    }

    if (TryConvertWslPathToWindowsPath(explicitPath, out var translatedPath))
    {
        return Path.GetFullPath(translatedPath);
    }

    return Path.IsPathRooted(explicitPath)
        ? Path.GetFullPath(explicitPath)
        : Path.GetFullPath(explicitPath, workspaceRoot);
}

static bool TryConvertWslPathToWindowsPath(string path, out string translatedPath)
{
    translatedPath = string.Empty;
    if (string.IsNullOrWhiteSpace(path))
    {
        return false;
    }

    if (path.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase)
        || (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':'))
    {
        return false;
    }

    var normalized = path.Replace('\\', '/');
    if (normalized.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase)
        && normalized.Length >= 7
        && char.IsLetter(normalized[5])
        && normalized[6] == '/')
    {
        var driveLetter = char.ToUpperInvariant(normalized[5]);
        var remainder = normalized.Length == 7 ? string.Empty : normalized[7..].Replace('/', '\\');
        translatedPath = string.IsNullOrEmpty(remainder)
            ? $"{driveLetter}:\\"
            : $"{driveLetter}:\\{remainder}";
        return true;
    }

    if (!normalized.StartsWith("/", StringComparison.Ordinal))
    {
        return false;
    }

    var distroName = Environment.GetEnvironmentVariable("WSL_DISTRO_NAME");
    if (!string.IsNullOrWhiteSpace(distroName))
    {
        translatedPath = $@"\\wsl.localhost\{distroName}\{normalized.TrimStart('/').Replace('/', '\\')}";
        return true;
    }

    return TryResolveWslPathViaExe(normalized, out translatedPath);
}

static bool TryResolveWslPathViaExe(string wslPath, out string translatedPath)
{
    translatedPath = string.Empty;

    try
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "wsl.exe",
                Arguments = $"wslpath -w \"{wslPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
        };

        process.Start();
        if (!process.WaitForExit(3000) || process.ExitCode != 0)
        {
            return false;
        }

        var stdout = process.StandardOutput.ReadToEnd();
        if (string.IsNullOrWhiteSpace(stdout))
        {
            return false;
        }

        translatedPath = stdout.Trim();
        return true;
    }
    catch
    {
        return false;
    }
}

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

static void WriteUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- run --scenario boot-to-combat|boot-to-long-run --provider session|auto|headless [--provider-command \"<cmd>\"] [--config path] [--run-root path] [--deploy-mode in-process|subprocess] [--runtime-assembly-root path] [--ffmpeg-path path] [--disable-video-capture] [--max-attempts n] [--max-consecutive-launch-failures n] [--max-scene-dead-ends n] [--max-session-hours n] [--max-steps n] [--stop-on-first-terminal] [--stop-on-first-loop]");
    Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- inspect-run --run-root <path>");
    Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- inspect-session --session-root <path>");
    Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- replay-step --request <path> [--decision <path>] [--out <path>] [--trace] [--full-request-rebuild]");
    Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- replay-test [--suite <path>] [--trace] [--full-request-rebuild]");
    Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- self-test");
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static void SetHarnessLogSink(Action<string>? sink)
{
    GuiSmokeShared.HarnessLogSink = sink;
}

static bool ShouldRecaptureForObserverDrift(ObserverSummary requestObserver, ObserverState latestObserver)
{
    if (latestObserver.CapturedAt is null || requestObserver.CapturedAt is null)
    {
        return false;
    }

    if (latestObserver.CapturedAt <= requestObserver.CapturedAt)
    {
        return false;
    }

    if (!string.Equals(requestObserver.CurrentScreen, latestObserver.CurrentScreen, StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    if (!string.Equals(requestObserver.VisibleScreen, latestObserver.VisibleScreen, StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    if (requestObserver.InCombat != latestObserver.InCombat)
    {
        return true;
    }

    if (!string.Equals(requestObserver.InventoryId, latestObserver.InventoryId, StringComparison.Ordinal))
    {
        return true;
    }

    return false;
}

static class GuiSmokeShared
{
    public static Action<string>? HarnessLogSink { get; set; }

    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static JsonSerializerOptions NdjsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static async Task RunProcessAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        TimeSpan timeout,
        bool waitForExit = true)
    {
        var result = await RunProcessDetailedAsync(fileName, arguments, workingDirectory, timeout, waitForExit).ConfigureAwait(false);
        if (!waitForExit)
        {
            return;
        }

        if (result.TimedOut)
        {
            throw new TimeoutException(
                $"Process timed out after {timeout}: {fileName} {arguments}{Environment.NewLine}stdout:{Environment.NewLine}{result.Stdout}{Environment.NewLine}stderr:{Environment.NewLine}{result.Stderr}");
        }

        if (!string.IsNullOrWhiteSpace(result.FailureKind))
        {
            throw new InvalidOperationException(
                $"Process failed before exit: {result.FailureKind} ({result.ExceptionType}: {result.ExceptionMessage}) {fileName} {arguments}{Environment.NewLine}stdout:{Environment.NewLine}{result.Stdout}{Environment.NewLine}stderr:{Environment.NewLine}{result.Stderr}");
        }

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Process failed with exit code {result.ExitCode}: {fileName} {arguments}{Environment.NewLine}stdout:{Environment.NewLine}{result.Stdout}{Environment.NewLine}stderr:{Environment.NewLine}{result.Stderr}");
        }
    }

    public static async Task<GuiSmokeProcessExecutionResult> RunProcessDetailedAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        TimeSpan timeout,
        bool waitForExit = true)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = waitForExit,
                RedirectStandardError = waitForExit,
                CreateNoWindow = true,
            },
        };

        var stopwatch = Stopwatch.StartNew();
        try
        {
            process.Start();
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(
                fileName,
                arguments,
                null,
                false,
                stopwatch.Elapsed,
                string.Empty,
                string.Empty,
                "process-start-failure",
                exception.GetType().Name,
                exception.Message);
        }

        if (!waitForExit)
        {
            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(fileName, arguments, null, false, stopwatch.Elapsed, string.Empty, string.Empty, null, null, null);
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        using var timeoutCts = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
            }

            try
            {
                await process.WaitForExitAsync().ConfigureAwait(false);
            }
            catch
            {
            }

            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(
                fileName,
                arguments,
                process.HasExited ? process.ExitCode : null,
                true,
                stopwatch.Elapsed,
                await stdoutTask.ConfigureAwait(false),
                await stderrTask.ConfigureAwait(false),
                null,
                null,
                null);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(
                fileName,
                arguments,
                process.HasExited ? process.ExitCode : null,
                false,
                stopwatch.Elapsed,
                await TryReadProcessOutputAsync(stdoutTask).ConfigureAwait(false),
                await TryReadProcessOutputAsync(stderrTask).ConfigureAwait(false),
                "process-wait-failure",
                exception.GetType().Name,
                exception.Message);
        }

        string stdout;
        string stderr;
        try
        {
            stdout = await stdoutTask.ConfigureAwait(false);
            stderr = await stderrTask.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(
                fileName,
                arguments,
                process.ExitCode,
                false,
                stopwatch.Elapsed,
                await TryReadProcessOutputAsync(stdoutTask).ConfigureAwait(false),
                await TryReadProcessOutputAsync(stderrTask).ConfigureAwait(false),
                "output-drain-failure",
                exception.GetType().Name,
                exception.Message);
        }

        stopwatch.Stop();
        return new GuiSmokeProcessExecutionResult(
            fileName,
            arguments,
            process.ExitCode,
            false,
            stopwatch.Elapsed,
            stdout,
            stderr,
            null,
            null,
            null);
    }

    private static async Task<string> TryReadProcessOutputAsync(Task<string>? task)
    {
        if (task is null)
        {
            return string.Empty;
        }

        try
        {
            return await task.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            return $"<output-read-failed:{exception.GetType().Name}:{exception.Message}>";
        }
    }
}

enum GuiSmokePhase
{
    WaitMainMenu,
    EnterRun,
    WaitCharacterSelect,
    ChooseCharacter,
    Embark,
    WaitMap,
    HandleRewards,
    ChooseFirstNode,
    WaitCombat,
    HandleEvent,
    HandleCombat,
    Completed,
}

sealed record GuiSmokeRunManifest(
    string RunId,
    string ScenarioId,
    string Provider,
    DateTimeOffset StartedAt,
    string WorkspaceRoot,
    string LiveRoot,
    string HarnessRoot,
    string GameDirectory)
{
    public string? Status { get; set; }
    public string? ResultMessage { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

sealed record GuiSmokeSessionSummary(
    string SessionId,
    string ScenarioId,
    string Provider,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    int AttemptCount,
    int TerminalAttemptCount,
    string? ActiveAttemptId,
    DateTimeOffset LastEventAt,
    int PassedAttempts,
    int FailedAttempts,
    int TotalSteps,
    IReadOnlyDictionary<string, int> ValidationEvents);

sealed record GuiSmokeAttemptResult(
    string AttemptId,
    int AttemptOrdinal,
    string RunId,
    string RunRoot,
    int ExitCode,
    string Status,
    string Message,
    int StepCount,
    bool LaunchFailed,
    string? TerminalCause,
    string? FailureClass,
    string TrustStateAtStart);

sealed record GuiSmokeAttemptIndexEntry(
    string AttemptId,
    int AttemptOrdinal,
    string RunId,
    string Status,
    string? ResultMessage,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    int StepCount,
    string? TerminalCause,
    bool LaunchFailed,
    string? FailureClass,
    string TrustStateAtStart);

sealed record GuiSmokeFailureSummary(
    string Phase,
    string Message,
    string? ObserverScreen,
    bool? ObserverInCombat,
    string? ScreenshotPath);

sealed record GuiSmokeDeployToolSelection(
    string Path,
    string Reason);

sealed record GuiSmokeDeployCommand(
    string Mode,
    string FileName,
    string Arguments,
    TimeSpan Timeout,
    string? ToolPath,
    string Reason);

sealed record GuiSmokeProcessExecutionResult(
    string FileName,
    string Arguments,
    int? ExitCode,
    bool TimedOut,
    TimeSpan Duration,
    string Stdout,
    string Stderr,
    string? FailureKind,
    string? ExceptionType,
    string? ExceptionMessage);

sealed record GuiSmokeValidationSummary(
    string RunId,
    int TotalTraceEntries,
    IReadOnlyDictionary<string, int> EventCounts,
    IReadOnlyDictionary<string, int> SceneCounts);

sealed record GuiSmokeTraceEntry(
    DateTimeOffset RecordedAt,
    int StepIndex,
    string Phase,
    string EventKind,
    string? ObserverScreen,
    bool? ObserverInCombat,
    string? TargetLabel);

sealed record GuiSmokeStepProgress(
    DateTimeOffset RecordedAt,
    int StepIndex,
    string Phase,
    string SceneSignature,
    string? ObserverScreen,
    string? PostActionScreen,
    string? DecisionTargetLabel,
    bool ObserverProgress,
    bool ActuatorProgress,
    bool FirstSeenScene,
    bool SemanticReasoningActive,
    bool RecipeRecorded,
    IReadOnlyList<string> ObserverSignals,
    IReadOnlyList<string> ActuatorSignals);

sealed record GuiSmokeSelfMetaReview(
    DateTimeOffset RecordedAt,
    string AttemptId,
    int AttemptOrdinal,
    string RunId,
    string Status,
    string? ResultMessage,
    bool PlateauDetected,
    string DominantFailureClass,
    string DirectionRisk,
    IReadOnlyList<string> Evidence,
    IReadOnlyList<string> NextAttemptAdjustments,
    double ObserverCoverageRatio,
    double ActuatorSuccessRatio,
    int NovelSceneCount,
    int SameActionStallCount,
    int StepCount);

sealed record GuiSmokeStepRequest(
    string RunId,
    string ScenarioId,
    int StepIndex,
    string Phase,
    string Goal,
    DateTimeOffset IssuedAt,
    string ScreenshotPath,
    WindowBounds WindowBounds,
    string SceneSignature,
    string AttemptId,
    int AttemptOrdinal,
    int BoundedExplorationBudget,
    bool FirstSeenScene,
    string ReasoningMode,
    string? SemanticGoal,
    ObserverSummary Observer,
    IReadOnlyList<KnownRecipeHint> KnownRecipes,
    IReadOnlyList<EventKnowledgeCandidate> EventKnowledgeCandidates,
    IReadOnlyList<CombatCardKnowledgeHint> CombatCardKnowledge,
    string[] AllowedActions,
    IReadOnlyList<GuiSmokeHistoryEntry> History,
    string FailureModeHint,
    string? DecisionRiskHint);

sealed record KnownRecipeHint(
    string SceneSignature,
    string Phase,
    string ActionKind,
    string? TargetLabel,
    string? ExpectedScreen,
    string? Reason);

sealed record SceneRecipeEntry(
    DateTimeOffset RecordedAt,
    string SceneSignature,
    string Phase,
    string ActionKind,
    string? TargetLabel,
    string? ExpectedScreen,
    string? Reason,
    string ScreenshotPath);

sealed record UnknownSceneEntry(
    DateTimeOffset RecordedAt,
    string SceneSignature,
    string Phase,
    string ScreenshotPath,
    string? ObserverScreen,
    string? VisibleScreen,
    string? Reason);

sealed record EventKnowledgeCandidate(
    string EventId,
    string Title,
    string? MatchReason,
    IReadOnlyList<EventOptionKnowledgeCandidate> Options);

sealed record EventOptionKnowledgeCandidate(
    string Label,
    string? Description,
    string? OptionKey);

sealed record CombatCardKnowledgeHint(
    int SlotIndex,
    string Name,
    string? Type,
    string? Target,
    int? Cost,
    string MatchSource);

sealed record GuiSmokeStepDecision(
    string Status,
    string? ActionKind,
    string? KeyText,
    double? NormalizedX,
    double? NormalizedY,
    string? TargetLabel,
    string? Reason,
    double? Confidence,
    string? ExpectedScreen,
    int? WaitMs,
    bool? RequiresRecapture,
    string? AbortReason,
    string? SceneInterpretation = null,
    string? ExpectedDelta = null,
    string? DecisionRisk = null);

sealed record GuiSmokeCandidatePoint(
    double X,
    double Y);

sealed record GuiSmokeSuppressedCandidate(
    string Label,
    string SuppressionReason);

sealed record GuiSmokeDecisionCandidateDump(
    string Label,
    string Source,
    double Score,
    bool Selected,
    string? RejectReason,
    string? RawBounds,
    GuiSmokeCandidatePoint? NormalizedPoint,
    string? BoundsSource,
    string? TargetLabel,
    string? ActionKind,
    string? Reason);

sealed record GuiSmokeDecisionDebugSummary(
    string? ForegroundKind,
    string? BackgroundKind,
    IReadOnlyList<string> ActiveCandidateSet,
    IReadOnlyList<GuiSmokeSuppressedCandidate> SuppressedCandidates,
    string WinnerSelectionReason);

sealed record GuiSmokeCandidateDumpArtifact(
    string Phase,
    string ScreenshotPath,
    GuiSmokeStepDecision PredictedDecision,
    GuiSmokeStepDecision FinalDecision,
    bool MatchesPredictedDecision,
    GuiSmokeDecisionDebugSummary DebugSummary,
    IReadOnlyList<GuiSmokeDecisionCandidateDump> Candidates);

sealed record GuiSmokeDecisionAnalysis(
    string Phase,
    string ScreenshotPath,
    GuiSmokeStepDecision PredictedDecision,
    GuiSmokeStepDecision FinalDecision,
    GuiSmokeDecisionDebugSummary DebugSummary,
    IReadOnlyList<GuiSmokeDecisionCandidateDump> Candidates,
    bool MatchesPredictedDecision)
{
    public GuiSmokeCandidateDumpArtifact ToArtifact()
    {
        return new GuiSmokeCandidateDumpArtifact(
            Phase,
            ScreenshotPath,
            PredictedDecision,
            FinalDecision,
            MatchesPredictedDecision,
            DebugSummary,
            Candidates);
    }
}

sealed record GuiSmokeReplayGoldenSceneFixture(
    string Name,
    string RequestPath,
    string? ExpectedTargetContains,
    string? ExpectedForegroundKind,
    string? ExpectedBackgroundKind,
    IReadOnlyList<string> RequiredCandidateLabels,
    IReadOnlyList<string> RequiredSuppressedLabels,
    IReadOnlyList<string> ForbiddenTargetLabels);

sealed record GuiSmokeReplayEvaluation(
    string RequestPath,
    GuiSmokeStepRequest Request,
    GuiSmokeStepDecision Decision,
    GuiSmokeCandidateDumpArtifact CandidateDump);

sealed record GuiSmokeReplayTimingEntry(
    string Stage,
    long ElapsedMs,
    string? Detail = null);

sealed record GuiSmokeReplayRequestLoadResult(
    GuiSmokeStepRequest Request,
    bool FullRequestRebuild,
    bool ObserverStateLoaded,
    IReadOnlyList<GuiSmokeReplayTimingEntry> Timings);

sealed class GuiSmokeReplayTracer
{
    private readonly string _scope;
    private readonly bool _verbose;
    private readonly List<GuiSmokeReplayTimingEntry> _entries = new();

    public GuiSmokeReplayTracer(string scope, bool verbose)
    {
        _scope = scope;
        _verbose = verbose;
    }

    public IReadOnlyList<GuiSmokeReplayTimingEntry> Entries => _entries;

    public void Info(string message)
    {
        Console.Error.WriteLine($"[{_scope}] {message}");
    }

    public void Skipped(string stage, string detail)
    {
        _entries.Add(new GuiSmokeReplayTimingEntry(stage, 0, $"skipped:{detail}"));
        if (_verbose)
        {
            Info($"{stage} skipped ({detail})");
        }
    }

    public T Measure<T>(string stage, Func<T> action, string? detail = null, bool alwaysLog = false)
    {
        if (_verbose || alwaysLog)
        {
            Info($"start {stage}{FormatDetail(detail)}");
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            return action();
        }
        finally
        {
            stopwatch.Stop();
            var entry = new GuiSmokeReplayTimingEntry(stage, stopwatch.ElapsedMilliseconds, detail);
            _entries.Add(entry);
            if (_verbose || alwaysLog)
            {
                Info($"done {stage} {stopwatch.ElapsedMilliseconds}ms{FormatDetail(detail)}");
            }
        }
    }

    private static string FormatDetail(string? detail)
    {
        return string.IsNullOrWhiteSpace(detail)
            ? string.Empty
            : $" ({detail})";
    }
}

sealed record GuiSmokeHistoryEntry(
    string Phase,
    string Action,
    string? TargetLabel,
    DateTimeOffset RecordedAt)
{
    public string? Metadata { get; init; }
}

sealed record WindowBounds(
    int X,
    int Y,
    int Width,
    int Height);

sealed record RewardMapLayerState(
    bool RewardPanelVisible,
    bool MapContextVisible,
    bool RewardBackNavigationAvailable,
    bool StaleRewardChoicePresent,
    bool StaleRewardBoundsPresent,
    bool OffWindowBoundsReused);

sealed record MapOverlayState(
    bool ForegroundVisible,
    bool EventBackgroundPresent,
    bool MapBackNavigationAvailable,
    bool StaleEventChoicePresent,
    bool CurrentNodeArrowVisible,
    bool ReachableNodeCandidatePresent,
    bool ExportedReachableNodeCandidatePresent);

static class GuiSmokeDecisionDebug
{
    public static void SetSceneModel(string? foregroundKind, string? backgroundKind)
    {
    }

    public static void ReplaceActiveCandidates(params string[] labels)
    {
    }

    public static void Suppress(string label, string reason)
    {
    }

    public static GuiSmokeStepDecision? TraceCandidate(
        string label,
        string source,
        double score,
        GuiSmokeStepDecision? decision,
        string rejectReason)
    {
        return decision;
    }
}

static class GuiSmokeReplayArtifactLoader
{
    public static JsonDocument? TryLoadObserverStateSidecar(string? screenshotPath)
    {
        if (string.IsNullOrWhiteSpace(screenshotPath))
        {
            return null;
        }

        var sidecarPath = screenshotPath.EndsWith(".screen.png", StringComparison.OrdinalIgnoreCase)
            ? screenshotPath[..^".screen.png".Length] + ".observer.state.json"
            : Path.ChangeExtension(screenshotPath, ".observer.state.json");
        if (!File.Exists(sidecarPath))
        {
            return null;
        }

        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("observer-state-sidecar", sidecarPath, () =>
        {
            using var stream = new FileStream(sidecarPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return JsonDocument.Parse(stream);
        });
    }
}

static class GuiSmokeScreenshotAnalysisCache
{
    private readonly record struct CacheKey(
        string Kind,
        string FullPath,
        long Length,
        long LastWriteUtcTicks);

    private static readonly ConcurrentDictionary<CacheKey, object> Entries = new();

    public static T GetOrCreate<T>(string kind, string screenshotPath, Func<T> factory)
    {
        if (!TryCreateKey(kind, screenshotPath, out var key))
        {
            return factory();
        }

        if (Entries.TryGetValue(key, out var existing) && existing is T typedExisting)
        {
            return typedExisting;
        }

        var created = factory();
        Entries[key] = created!;
        if (Entries.Count > 1024)
        {
            Entries.Clear();
        }

        return created;
    }

    private static bool TryCreateKey(string kind, string screenshotPath, out CacheKey key)
    {
        key = default;
        if (string.IsNullOrWhiteSpace(screenshotPath))
        {
            return false;
        }

        try
        {
            var fileInfo = new FileInfo(screenshotPath);
            if (!fileInfo.Exists)
            {
                return false;
            }

            key = new CacheKey(
                kind,
                fileInfo.FullName,
                fileInfo.Length,
                fileInfo.LastWriteTimeUtc.Ticks);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

sealed record ObserverSummary(
    string? CurrentScreen,
    string? VisibleScreen,
    bool? InCombat,
    DateTimeOffset? CapturedAt,
    string? InventoryId,
    bool? SceneReady,
    string? SceneAuthority,
    string? SceneStability,
    string? SceneEpisodeId,
    string? EncounterKind,
    string? ChoiceExtractorPath,
    int? PlayerCurrentHp,
    int? PlayerMaxHp,
    int? PlayerEnergy,
    IReadOnlyList<string> CurrentChoices,
    IReadOnlyList<string> LastEventsTail,
    IReadOnlyList<ObserverActionNode> ActionNodes,
    IReadOnlyList<ObserverChoice> Choices,
    IReadOnlyList<ObservedCombatHandCard> CombatHand)
{
    public IReadOnlyDictionary<string, string?> Meta { get; init; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
}

sealed record ObserverActionNode(
    string NodeId,
    string Kind,
    string Label,
    string? ScreenBounds,
    bool Actionable);

sealed record ObserverChoice(
    string Kind,
    string Label,
    string? ScreenBounds,
    string? Value = null,
    string? Description = null)
{
    public string? NodeId { get; init; }

    public string? BindingKind { get; init; }

    public string? BindingId { get; init; }

    public bool? Enabled { get; init; }

    public string? IconAssetPath { get; init; }

    public IReadOnlyList<string> SemanticHints { get; init; } = Array.Empty<string>();
}

sealed record ObservedCombatHandCard(
    int SlotIndex,
    string Name,
    string? Type,
    int? Cost);

sealed record RestSiteObservedChoice(
    string OptionId,
    string TargetLabel,
    string CandidateLabel,
    string Label,
    string? Description,
    bool HasMetadata,
    bool IsEnabled,
    bool IsDefaultAutoPick,
    ObserverChoice? Choice,
    ObserverActionNode? ActionNode);

sealed record RestSiteDecisionCandidate(
    string Label,
    string Source,
    double Score,
    string? RejectReason,
    string? RawBounds,
    string? BoundsSource,
    GuiSmokeStepDecision? Decision);

sealed record RestSiteActionMetadata(
    string Kind,
    string TargetLabel,
    string Fingerprint);

enum RestSiteExplicitChoiceRepeatState
{
    None,
    GraceNeeded,
    NoOpDetected,
}

sealed record ObserverState(
    ObserverSummary Summary,
    JsonDocument? StateDocument,
    JsonDocument? InventoryDocument,
    string[]? EventLines)
{
    public string? CurrentScreen => Summary.CurrentScreen;
    public string? VisibleScreen => Summary.VisibleScreen;
    public bool? InCombat => Summary.InCombat;
    public string? InventoryId => Summary.InventoryId;
    public bool? SceneReady => Summary.SceneReady;
    public string? SceneAuthority => Summary.SceneAuthority;
    public string? SceneStability => Summary.SceneStability;
    public int? PlayerEnergy => Summary.PlayerEnergy;
    public IReadOnlyList<string> CurrentChoices => Summary.CurrentChoices;
    public IReadOnlyList<ObserverActionNode> ActionNodes => Summary.ActionNodes;
    public IReadOnlyList<ObserverChoice> Choices => Summary.Choices;
    public IReadOnlyList<ObservedCombatHandCard> CombatHand => Summary.CombatHand;
    public IReadOnlyDictionary<string, string?> Meta => Summary.Meta;
    public DateTimeOffset? CapturedAt => Summary.CapturedAt;

    public bool IsFreshSince(DateTimeOffset threshold)
    {
        return CapturedAt is not null && CapturedAt >= threshold;
    }

    public string? EncounterKind => Summary.EncounterKind;
    public string? ChoiceExtractorPath => Summary.ChoiceExtractorPath;
}

sealed record RestSitePostClickEvidence(
    string Classification,
    string? Outcome,
    string? OutcomeEvidence,
    string? CurrentStatus,
    string? CurrentOptionId,
    string? LastSignal,
    string? LastOptionId,
    bool UpgradeScreenVisible,
    bool ExplicitChoiceVisible,
    bool SmithGridVisible,
    bool SmithConfirmVisible,
    bool UpgradeChoiceObserverMiss);

static class RestSiteObserverSignals
{
    public static string? TryGetMetaValue(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value) ? value : null;
    }

    public static bool HasSmithUpgradeVisible(ObserverSummary observer)
    {
        return HasSmithGridVisible(observer) || HasSmithConfirmVisible(observer) || HasSmithUpgradeScreenVisible(observer);
    }

    public static bool HasSmithUpgradeScreenVisible(ObserverSummary observer)
    {
        return string.Equals(observer.CurrentScreen, "upgrade", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.VisibleScreen, "upgrade", StringComparison.OrdinalIgnoreCase)
               || string.Equals(TryGetMetaValue(observer, "restSiteUpgradeScreenVisible"), "true", StringComparison.OrdinalIgnoreCase)
               || string.Equals(TryGetMetaValue(observer, "restSiteViewKind"), "smith-grid-observer-miss", StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasSmithGridVisible(ObserverSummary observer)
    {
        return observer.Choices.Any(static choice => string.Equals(choice.Kind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase))
               || string.Equals(TryGetMetaValue(observer, "restSiteSelectionCurrentStatus"), "grid-visible", StringComparison.OrdinalIgnoreCase)
               || (string.Equals(TryGetMetaValue(observer, "restSiteUpgradeScreenVisible"), "true", StringComparison.OrdinalIgnoreCase)
                   && TryParseMetaInt(observer, "restSiteUpgradeCardCount") > 0);
    }

    public static bool HasSmithConfirmVisible(ObserverSummary observer)
    {
        return observer.Choices.Any(static choice => string.Equals(choice.Kind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase))
               || string.Equals(TryGetMetaValue(observer, "restSiteSelectionCurrentStatus"), "confirm-visible", StringComparison.OrdinalIgnoreCase)
               || string.Equals(TryGetMetaValue(observer, "restSiteUpgradeConfirmVisible"), "true", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsRestSiteSmithUpgradeState(ObserverSummary observer)
    {
        return string.Equals(observer.ChoiceExtractorPath, "rest-smith-upgrade", StringComparison.OrdinalIgnoreCase)
               || HasSmithUpgradeScreenVisible(observer)
               || string.Equals(TryGetMetaValue(observer, "restSiteViewKind"), "smith-grid", StringComparison.OrdinalIgnoreCase)
               || string.Equals(TryGetMetaValue(observer, "restSiteViewKind"), "smith-confirm", StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasExportedSmithUpgradeChoices(ObserverSummary observer)
    {
        return observer.Choices.Any(static choice =>
            string.Equals(choice.Kind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.BindingKind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.Kind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.BindingKind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase));
    }

    public static ObserverChoice? TryGetSmithConfirmChoice(ObserverSummary observer)
    {
        return observer.Choices
            .Where(static choice =>
                string.Equals(choice.Kind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.BindingKind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static choice => choice.Enabled == false ? 1 : 0)
            .ThenBy(static choice => string.IsNullOrWhiteSpace(choice.ScreenBounds) ? 1 : 0)
            .FirstOrDefault();
    }

    public static ObserverChoice? TryGetFirstSmithCardChoice(ObserverSummary observer)
    {
        return observer.Choices
            .Where(static choice =>
                string.Equals(choice.Kind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.BindingKind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static choice => choice.Enabled == false ? 1 : 0)
            .ThenBy(static choice => string.IsNullOrWhiteSpace(choice.ScreenBounds) ? 1 : 0)
            .ThenBy(static choice => TryGetSortX(choice.ScreenBounds))
            .ThenBy(static choice => TryGetSortY(choice.ScreenBounds))
            .FirstOrDefault();
    }

    public static RestSitePostClickEvidence BuildPostClickEvidence(ObserverSummary observer, string? targetLabel)
    {
        var normalizedTarget = RestSiteChoiceSupport.NormalizeOptionId(RestSiteChoiceSupport.MapLabelToOptionId(targetLabel) ?? targetLabel);
        var explicitChoiceVisible = RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(observer);
        var smithGridVisible = HasSmithGridVisible(observer);
        var smithConfirmVisible = HasSmithConfirmVisible(observer);
        var upgradeScreenVisible = HasSmithUpgradeScreenVisible(observer);
        var outcome = TryGetMetaValue(observer, "restSiteSelectionOutcome");
        var outcomeEvidence = TryGetMetaValue(observer, "restSiteSelectionOutcomeEvidence");
        var currentStatus = TryGetMetaValue(observer, "restSiteSelectionCurrentStatus");
        var currentOptionId = RestSiteChoiceSupport.NormalizeOptionId(TryGetMetaValue(observer, "restSiteSelectionCurrentOptionId"));
        var lastSignal = TryGetMetaValue(observer, "restSiteSelectionLastSignal");
        var lastOptionId = RestSiteChoiceSupport.NormalizeOptionId(TryGetMetaValue(observer, "restSiteSelectionLastOptionId"));
        var upgradeChoiceObserverMiss = string.Equals(TryGetMetaValue(observer, "restSiteUpgradeObserverMiss"), "true", StringComparison.OrdinalIgnoreCase)
                                        || (string.Equals(currentStatus, "grid-visible", StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(currentStatus, "confirm-visible", StringComparison.OrdinalIgnoreCase)
                                            || upgradeScreenVisible)
                                        && !observer.Choices.Any(static choice =>
                                            string.Equals(choice.Kind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(choice.Kind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase));

        var classification = "rest-site-post-click-noop";
        if (string.Equals(normalizedTarget, "SMITH", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(outcome, "failure", StringComparison.OrdinalIgnoreCase)
                || (string.Equals(lastOptionId, "SMITH", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(lastSignal, "after-select-failure", StringComparison.OrdinalIgnoreCase))
                || string.Equals(currentStatus, "selection-failed", StringComparison.OrdinalIgnoreCase))
            {
                classification = "rest-site-selection-failed";
            }
            else if (upgradeChoiceObserverMiss
                     || (string.Equals(outcome, "success", StringComparison.OrdinalIgnoreCase)
                         && upgradeScreenVisible
                         && !smithGridVisible
                         && !smithConfirmVisible))
            {
                classification = "rest-site-grid-observer-miss";
            }
            else if (string.Equals(outcome, "in-progress", StringComparison.OrdinalIgnoreCase)
                     || (string.Equals(lastOptionId, "SMITH", StringComparison.OrdinalIgnoreCase)
                      && (string.Equals(lastSignal, "before-select", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(lastSignal, "after-select-success", StringComparison.OrdinalIgnoreCase)))
                     || (string.Equals(currentOptionId, "SMITH", StringComparison.OrdinalIgnoreCase)
                         && (string.Equals(currentStatus, "selecting", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(currentStatus, "options-disabled", StringComparison.OrdinalIgnoreCase))))
            {
                classification = "rest-site-grid-not-visible-after-selection";
            }
        }

        return new RestSitePostClickEvidence(
            classification,
            outcome,
            outcomeEvidence,
            currentStatus,
            currentOptionId,
            lastSignal,
            lastOptionId,
            upgradeScreenVisible,
            explicitChoiceVisible,
            smithGridVisible,
            smithConfirmVisible,
            upgradeChoiceObserverMiss);
    }

    private static int TryParseMetaInt(ObserverSummary observer, string key)
    {
        return int.TryParse(TryGetMetaValue(observer, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private static double TryGetSortX(string? rawBounds)
        => TryParseBoundsInternal(rawBounds, out var bounds) ? bounds.X : double.MaxValue;

    private static double TryGetSortY(string? rawBounds)
        => TryParseBoundsInternal(rawBounds, out var bounds) ? bounds.Y : double.MaxValue;

    private static bool TryParseBoundsInternal(string? rawBounds, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(rawBounds))
        {
            return false;
        }

        var parts = rawBounds.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4
            || !float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height)
            || width <= 0f
            || height <= 0f)
        {
            return false;
        }

        bounds = new RectangleF(x, y, width, height);
        return true;
    }
}

static class RestSiteChoiceSupport
{
    private static readonly string[] DefaultAutoPickIds = { "HEAL", "SMITH", "HATCH" };

    public static IReadOnlyList<RestSiteObservedChoice> GetObservedChoices(ObserverSummary observer)
    {
        var metadataChoices = observer.Choices
            .Where(HasAuthoritativeMetadata)
            .Select(choice => CreateObservedChoiceFromMetadata(observer, choice))
            .Where(static choice => choice is not null)
            .Cast<RestSiteObservedChoice>()
            .GroupBy(static choice => choice.OptionId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderBy(static choice => string.IsNullOrWhiteSpace(choice.Choice?.ScreenBounds) ? 1 : 0)
                .ThenBy(static choice => TryGetSortX(choice.Choice?.ScreenBounds))
                .ThenBy(static choice => TryGetSortY(choice.Choice?.ScreenBounds))
                .ThenBy(static choice => choice.Label, StringComparer.Ordinal)
                .First())
            .OrderBy(static choice => TryGetSortX(choice.Choice?.ScreenBounds))
            .ThenBy(static choice => TryGetSortY(choice.Choice?.ScreenBounds))
            .ThenBy(static choice => choice.OptionId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (metadataChoices.Length > 0)
        {
            return metadataChoices;
        }

        return DefaultAutoPickIds
            .Select(optionId => CreateObservedChoiceFromLegacy(observer, optionId))
            .Where(static choice => choice is not null)
            .Cast<RestSiteObservedChoice>()
            .ToArray();
    }

    public static bool HasExplicitRestSiteChoiceAffordance(ObserverSummary observer)
        => GetObservedChoices(observer).Count > 0;

    public static bool HasRestSiteRestChoice(ObserverSummary observer)
        => GetObservedChoices(observer).Any(static choice => string.Equals(choice.OptionId, "HEAL", StringComparison.OrdinalIgnoreCase));

    public static bool HasRestSiteSmithChoice(ObserverSummary observer)
        => GetObservedChoices(observer).Any(static choice => string.Equals(choice.OptionId, "SMITH", StringComparison.OrdinalIgnoreCase));

    public static bool HasRestSiteHatchChoice(ObserverSummary observer)
        => GetObservedChoices(observer).Any(static choice => string.Equals(choice.OptionId, "HATCH", StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<string> BuildAllowedActions(ObserverSummary observer)
    {
        return GetObservedChoices(observer)
            .Select(static choice => choice.CandidateLabel)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Concat(new[] { "click smith card", "click smith confirm", "wait" })
            .ToArray();
    }

    public static IReadOnlyList<string> BuildCandidateLabels(ObserverSummary observer)
        => BuildAllowedActions(observer);

    public static bool HasAuthoritativeMetadata(ObserverSummary observer)
        => GetObservedChoices(observer).Any(static choice => choice.HasMetadata);

    public static bool HasAuthoritativeMetadata(ObserverChoice choice)
    {
        if (choice is null)
        {
            return false;
        }

        return string.Equals(choice.BindingKind, "rest-site-option", StringComparison.OrdinalIgnoreCase)
               || string.Equals(choice.Kind, "rest-option", StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(choice.NodeId)
                   && choice.NodeId.StartsWith("rest-site:", StringComparison.OrdinalIgnoreCase)
                   && !choice.NodeId.Contains("smith-card", StringComparison.OrdinalIgnoreCase)
                   && !choice.NodeId.Contains("smith-confirm", StringComparison.OrdinalIgnoreCase))
               || choice.SemanticHints.Any(static hint => hint.StartsWith("option-id:", StringComparison.OrdinalIgnoreCase));
    }

    public static string? NormalizeOptionId(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var trimmed = raw.Trim();
        if (trimmed.StartsWith("rest-site:", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed["rest-site:".Length..];
        }

        if (trimmed.StartsWith("option:", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed["option:".Length..];
        }

        if (trimmed.StartsWith("OPTION_", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed["OPTION_".Length..];
        }

        trimmed = trimmed.ToUpperInvariant();
        return trimmed switch
        {
            "REST" => "HEAL",
            "SMITH" => "SMITH",
            "HATCH" => "HATCH",
            "HEAL" => "HEAL",
            _ when string.IsNullOrWhiteSpace(trimmed) => null,
            _ => trimmed,
        };
    }

    public static string? MapLabelToOptionId(string? label)
    {
        if (!string.IsNullOrWhiteSpace(label))
        {
            if (label.Contains("rest site: smith", StringComparison.OrdinalIgnoreCase)
                || label.Contains("rest-site:smith", StringComparison.OrdinalIgnoreCase))
            {
                return "SMITH";
            }

            if (label.Contains("rest site: rest", StringComparison.OrdinalIgnoreCase)
                || label.Contains("rest-site:rest", StringComparison.OrdinalIgnoreCase))
            {
                return "HEAL";
            }

            if (label.Contains("rest site: hatch", StringComparison.OrdinalIgnoreCase)
                || label.Contains("rest-site:hatch", StringComparison.OrdinalIgnoreCase))
            {
                return "HATCH";
            }
        }

        if (HasRestSiteRestLabelInternal(label))
        {
            return "HEAL";
        }

        if (HasRestSiteSmithLabelInternal(label))
        {
            return "SMITH";
        }

        if (HasRestSiteHatchLabelInternal(label))
        {
            return "HATCH";
        }

        return null;
    }

    public static string? TryResolveOptionId(ObserverChoice choice)
    {
        var semanticOptionId = choice.SemanticHints
            .FirstOrDefault(static hint => hint.StartsWith("option-id:", StringComparison.OrdinalIgnoreCase));
        return NormalizeOptionId(
            choice.BindingId
            ?? ExtractOptionIdFromNodeId(choice.NodeId)
            ?? choice.Value
            ?? semanticOptionId?["option-id:".Length..]
            ?? MapLabelToOptionId(choice.Label));
    }

    public static string MapOptionIdToTargetLabel(string optionId)
    {
        return NormalizeOptionId(optionId) switch
        {
            "HEAL" => "rest site: rest",
            "SMITH" => "rest site: smith",
            "HATCH" => "rest site: hatch",
            { Length: > 0 } normalized => $"rest site: option:{normalized}",
            _ => "rest site: smith",
        };
    }

    public static string MapOptionIdToCandidateLabel(string optionId)
    {
        return NormalizeOptionId(optionId) switch
        {
            "HEAL" => "click rest site choice",
            "SMITH" => "click rest site choice",
            "HATCH" => "click rest site choice",
            { Length: > 0 } normalized => $"click option:{normalized}",
            _ => "click rest site choice",
        };
    }

    public static string BuildExplicitChoiceFingerprint(ObserverSummary observer)
    {
        var options = GetObservedChoices(observer)
            .Select(static choice => $"{choice.OptionId}:{choice.IsEnabled.ToString().ToLowerInvariant()}")
            .ToArray();
        return string.Join(
            "|",
            "rest-site",
            observer.EncounterKind ?? "none",
            observer.ChoiceExtractorPath ?? "unknown",
            string.Join(",", options));
    }

    public static string? DetermineAutoPickOptionId(ObserverSummary observer)
    {
        var options = GetObservedChoices(observer);
        if (options.Count == 0)
        {
            return null;
        }

        var hasMetadata = options.Any(static option => option.HasMetadata);
        static bool IsActionable(RestSiteObservedChoice option, bool hasMetadata)
            => !hasMetadata || option.IsEnabled;

        var maxHp = observer.PlayerMaxHp ?? 0;
        var currentHp = observer.PlayerCurrentHp ?? 0;
        var shouldRest = options.Any(option => string.Equals(option.OptionId, "HEAL", StringComparison.OrdinalIgnoreCase) && IsActionable(option, hasMetadata))
                         && (maxHp <= 0 || currentHp <= Math.Ceiling(maxHp * 0.70d));
        if (shouldRest)
        {
            return "HEAL";
        }

        if (options.Any(option => string.Equals(option.OptionId, "SMITH", StringComparison.OrdinalIgnoreCase) && IsActionable(option, hasMetadata)))
        {
            return "SMITH";
        }

        if (options.Any(option => string.Equals(option.OptionId, "HATCH", StringComparison.OrdinalIgnoreCase) && IsActionable(option, hasMetadata)))
        {
            return "HATCH";
        }

        return null;
    }

    private static RestSiteObservedChoice? CreateObservedChoiceFromMetadata(ObserverSummary observer, ObserverChoice choice)
    {
        var optionId = TryResolveOptionId(choice);
        if (string.IsNullOrWhiteSpace(optionId))
        {
            return null;
        }

        var normalizedOptionId = NormalizeOptionId(optionId);
        if (string.IsNullOrWhiteSpace(normalizedOptionId))
        {
            return null;
        }

        var matchingNode = FindMatchingActionNode(observer, choice, normalizedOptionId);
        return new RestSiteObservedChoice(
            normalizedOptionId,
            MapOptionIdToTargetLabel(normalizedOptionId),
            MapOptionIdToCandidateLabel(normalizedOptionId),
            choice.Label,
            choice.Description,
            HasMetadata: true,
            IsEnabled: choice.Enabled ?? matchingNode?.Actionable ?? true,
            IsDefaultAutoPick: DefaultAutoPickIds.Contains(normalizedOptionId, StringComparer.OrdinalIgnoreCase),
            Choice: choice,
            ActionNode: matchingNode);
    }

    private static RestSiteObservedChoice? CreateObservedChoiceFromLegacy(ObserverSummary observer, string optionId)
    {
        var matchingChoice = observer.Choices.FirstOrDefault(choice => MatchesOption(choice.Label, optionId));
        var matchingNode = observer.ActionNodes.FirstOrDefault(node => node.Actionable && MatchesOption(node.Label, optionId));
        if (matchingChoice is null
            && matchingNode is null
            && !observer.CurrentChoices.Any(label => MatchesOption(label, optionId)))
        {
            return null;
        }

        return new RestSiteObservedChoice(
            optionId,
            MapOptionIdToTargetLabel(optionId),
            MapOptionIdToCandidateLabel(optionId),
            matchingChoice?.Label ?? matchingNode?.Label ?? MapOptionIdToTargetLabel(optionId),
            matchingChoice?.Description,
            HasMetadata: false,
            IsEnabled: matchingNode?.Actionable ?? true,
            IsDefaultAutoPick: true,
            Choice: matchingChoice,
            ActionNode: matchingNode);
    }

    private static ObserverActionNode? FindMatchingActionNode(ObserverSummary observer, ObserverChoice choice, string optionId)
    {
        if (!string.IsNullOrWhiteSpace(choice.NodeId))
        {
            var nodeById = observer.ActionNodes.FirstOrDefault(node => string.Equals(node.NodeId, choice.NodeId, StringComparison.OrdinalIgnoreCase));
            if (nodeById is not null)
            {
                return nodeById;
            }
        }

        return observer.ActionNodes.FirstOrDefault(node =>
            MatchesOption(node.Label, optionId)
            || string.Equals(ExtractOptionIdFromNodeId(node.NodeId), optionId, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ExtractOptionIdFromNodeId(string? nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return null;
        }

        var prefix = "rest-site:";
        return nodeId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? NormalizeOptionId(nodeId[prefix.Length..])
            : null;
    }

    private static bool MatchesOption(string? label, string optionId)
    {
        return NormalizeOptionId(optionId) switch
        {
            "HEAL" => HasRestSiteRestLabelInternal(label),
            "SMITH" => HasRestSiteSmithLabelInternal(label),
            "HATCH" => HasRestSiteHatchLabelInternal(label),
            _ => false,
        };
    }

    private static double TryGetSortX(string? rawBounds)
        => TryParseBoundsInternal(rawBounds, out var bounds) ? bounds.X : double.MaxValue;

    private static double TryGetSortY(string? rawBounds)
        => TryParseBoundsInternal(rawBounds, out var bounds) ? bounds.Y : double.MaxValue;

    private static bool HasRestSiteRestLabelInternal(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("휴식", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Rest", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasRestSiteSmithLabelInternal(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("재련", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Smith", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasRestSiteHatchLabelInternal(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("부화", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Hatch", StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryParseBoundsInternal(string? rawBounds, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(rawBounds))
        {
            return false;
        }

        var parts = rawBounds.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4
            || !float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height)
            || width <= 0f
            || height <= 0f)
        {
            return false;
        }

        bounds = new RectangleF(x, y, width, height);
        return true;
    }
}

static class GuiSmokeForegroundHeuristics
{
    public static bool ShouldPreferEventProgressionOverMapFallback(ObserverState observer)
    {
        var declaringType = TryReadMetaString(observer.StateDocument, "declaringType");
        var instanceType = TryReadMetaString(observer.StateDocument, "instanceType");
        if (ContainsMapAuthority(declaringType) || ContainsMapAuthority(instanceType))
        {
            return false;
        }

        return HasEventForegroundAuthority(observer.Summary)
               || ContainsEventAuthority(declaringType)
               || ContainsEventAuthority(instanceType);
    }

    public static bool ShouldPreferEventProgressionOverMapFallback(ObserverSummary observer)
    {
        return HasEventForegroundAuthority(observer);
    }

    private static bool HasEventForegroundAuthority(ObserverSummary observer)
    {
        if (string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.VisibleScreen, "rewards", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var eventScreenAuthority = string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(observer.VisibleScreen, "event", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(observer.ChoiceExtractorPath, "event", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(observer.ChoiceExtractorPath, "room-event", StringComparison.OrdinalIgnoreCase);
        if (!eventScreenAuthority)
        {
            return false;
        }

        return observer.ActionNodes.Any(node =>
                   node.Actionable
                   && node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
                   && HasUsableBounds(node.ScreenBounds))
               || observer.Choices.Any(choice =>
                   HasUsableBounds(choice.ScreenBounds)
                   && !LooksLikeProceedOrSkip(choice.Label));
    }

    private static bool HasUsableBounds(string? rawBounds)
    {
        return TryParseBounds(rawBounds, out _, out _, out _, out _);
    }

    private static bool LooksLikeProceedOrSkip(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        return label.Contains("넘기", StringComparison.OrdinalIgnoreCase)
               || label.Contains("skip", StringComparison.OrdinalIgnoreCase)
               || label.Contains("proceed", StringComparison.OrdinalIgnoreCase)
               || label.Contains("continue", StringComparison.OrdinalIgnoreCase)
               || label.Contains("진행", StringComparison.OrdinalIgnoreCase)
               || label.Contains("확인", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsEventAuthority(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NEventRoom", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsMapAuthority(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryReadMetaString(JsonDocument? document, string propertyName)
    {
        if (document is null)
        {
            return null;
        }

        if (document.RootElement.TryGetProperty("meta", out var metaElement)
            && metaElement.ValueKind == JsonValueKind.Object
            && metaElement.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return null;
    }

    private static bool TryParseBounds(string? rawBounds, out float x, out float y, out float width, out float height)
    {
        x = default;
        y = default;
        width = default;
        height = default;
        if (string.IsNullOrWhiteSpace(rawBounds))
        {
            return false;
        }

        var parts = rawBounds.Split(',', StringSplitOptions.TrimEntries);
        return parts.Length == 4
               && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x)
               && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y)
               && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out width)
               && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out height)
               && width > 0f
               && height > 0f;
    }
}

static class GuiSmokeMapOverlayHeuristics
{
    public static MapOverlayState BuildState(ObserverState observer, WindowBounds? windowBounds, string? screenshotPath)
    {
        return BuildStateCore(observer.Summary, observer.StateDocument, windowBounds, screenshotPath);
    }

    public static MapOverlayState BuildState(ObserverSummary observer, WindowBounds? windowBounds, string? screenshotPath)
    {
        var stateDocument = GuiSmokeReplayArtifactLoader.TryLoadObserverStateSidecar(screenshotPath);
        return BuildStateCore(observer, stateDocument, windowBounds, screenshotPath);
    }

    private static MapOverlayState BuildStateCore(ObserverSummary observer, JsonDocument? stateDocument, WindowBounds? windowBounds, string? screenshotPath)
    {
        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(screenshotPath ?? string.Empty);
        var mapAnalysis = AutoMapAnalyzer.Analyze(screenshotPath ?? string.Empty);
        var declaringType = TryReadMetaString(stateDocument, "declaringType");
        var instanceType = TryReadMetaString(stateDocument, "instanceType");
        var rootTypeSummary = TryReadMetaString(stateDocument, "rootTypeSummary");
        var hasMapAuthority = ContainsMapAuthority(declaringType)
                              || ContainsMapAuthority(instanceType)
                              || ContainsMapAuthority(rootTypeSummary)
                              || string.Equals(TryReadMetaString(stateDocument, "mapOverlayVisible"), "true", StringComparison.OrdinalIgnoreCase);
        var exportedReachableNodeCandidatePresent = observer.Choices.Any(choice =>
                                                      string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
                                                      && HasActiveBounds(choice.ScreenBounds, windowBounds))
                                                  || observer.ActionNodes.Any(node =>
                                                      node.Actionable
                                                      && string.Equals(node.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
                                                      && HasActiveBounds(node.ScreenBounds, windowBounds));
        var staleEventChoicePresent = HasEventChoiceEvidence(observer, windowBounds);
        var eventBackgroundPresent = string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(observer.VisibleScreen, "event", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(observer.ChoiceExtractorPath, "event", StringComparison.OrdinalIgnoreCase)
                                     || staleEventChoicePresent;
        var mapBackNavigationAvailable = overlayAnalysis.HasBottomLeftBackArrow
                                         || observer.CurrentChoices.Any(IsBackChoiceLabel)
                                         || observer.ActionNodes.Any(node => node.Actionable && IsBackChoiceLabel(node.Label));
        var foregroundVisible = hasMapAuthority
                                && (overlayAnalysis.HasBottomLeftBackArrow
                                    || exportedReachableNodeCandidatePresent
                                    || mapAnalysis.HasReachableNode
                                    || mapAnalysis.HasCurrentArrow);
        return new MapOverlayState(
            foregroundVisible,
            eventBackgroundPresent,
            mapBackNavigationAvailable,
            foregroundVisible && staleEventChoicePresent,
            foregroundVisible && mapAnalysis.HasCurrentArrow,
            foregroundVisible && (exportedReachableNodeCandidatePresent || mapAnalysis.HasReachableNode),
            foregroundVisible && exportedReachableNodeCandidatePresent);
    }

    private static bool HasEventChoiceEvidence(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return observer.Choices.Any(choice =>
                   HasActiveBounds(choice.ScreenBounds, windowBounds)
                   && (string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase))
                   && !IsMapChoice(choice)
                   && !IsBackChoiceLabel(choice.Label))
               || observer.ActionNodes.Any(node =>
                   node.Actionable
                   && HasActiveBounds(node.ScreenBounds, windowBounds)
                   && node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
                   && !IsBackChoiceLabel(node.Label));
    }

    private static bool HasActiveBounds(string? rawBounds, WindowBounds? windowBounds)
    {
        if (!TryParseBounds(rawBounds, out var x, out var y, out var width, out var height))
        {
            return false;
        }

        if (windowBounds is null)
        {
            return true;
        }

        var rect = new RectangleF(x, y, width, height);
        var windowRect = new RectangleF(windowBounds.X, windowBounds.Y, windowBounds.Width, windowBounds.Height);
        return rect.Width > 0f
               && rect.Height > 0f
               && rect.Right > windowRect.Left
               && rect.Bottom > windowRect.Top
               && rect.Left < windowRect.Right
               && rect.Top < windowRect.Bottom;
    }

    private static bool TryParseBounds(string? rawBounds, out float x, out float y, out float width, out float height)
    {
        x = default;
        y = default;
        width = default;
        height = default;
        if (string.IsNullOrWhiteSpace(rawBounds))
        {
            return false;
        }

        var parts = rawBounds.Split(',', StringSplitOptions.TrimEntries);
        return parts.Length == 4
               && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x)
               && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y)
               && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out width)
               && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out height)
               && width > 0f
               && height > 0f;
    }

    private static bool IsBackChoiceLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        return label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
               || label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
               || label.Contains("뒤로", StringComparison.OrdinalIgnoreCase)
               || label.Contains("Back", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMapChoice(ObserverChoice choice)
    {
        return string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
               || (choice.Value?.Contains(",", StringComparison.OrdinalIgnoreCase) ?? false) && (choice.Description?.Contains("coord:", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private static bool ContainsMapAuthority(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryReadMetaString(JsonDocument? document, string propertyName)
    {
        if (document is null)
        {
            return null;
        }

        if (document.RootElement.TryGetProperty("meta", out var metaElement)
            && metaElement.ValueKind == JsonValueKind.Object
            && metaElement.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return null;
    }
}

static class GuiSmokeObserverPhaseHeuristics
{
    public static bool TryGetPostEmbarkPhase(ObserverState observer, out GuiSmokePhase nextPhase)
    {
        return TryGetPostEmbarkPhase(
            observer.Summary,
            TryReadObserverMetaString(observer.StateDocument, "declaringType"),
            TryReadObserverMetaString(observer.StateDocument, "instanceType"),
            out nextPhase);
    }

    public static bool TryGetPostEmbarkPhase(ObserverSummary observer, out GuiSmokePhase nextPhase)
    {
        return TryGetPostEmbarkPhase(observer, null, null, out nextPhase);
    }

    public static bool TryGetPostEmbarkPhase(ObserverSummary observer, string? declaringType, out GuiSmokePhase nextPhase)
    {
        return TryGetPostEmbarkPhase(observer, declaringType, null, out nextPhase);
    }

    public static bool TryGetPostEmbarkPhase(ObserverSummary observer, string? declaringType, string? instanceType, out GuiSmokePhase nextPhase)
    {
        if (LooksLikeRewardsState(observer))
        {
            nextPhase = GuiSmokePhase.HandleRewards;
            return true;
        }

        if (LooksLikeCombatState(observer))
        {
            nextPhase = GuiSmokePhase.HandleCombat;
            return true;
        }

        if (LooksLikeMapState(observer, declaringType, instanceType)
            || LooksLikeShopState(observer)
            || LooksLikeRestSiteState(observer))
        {
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (LooksLikeEventState(observer, declaringType, instanceType))
        {
            nextPhase = GuiSmokePhase.HandleEvent;
            return true;
        }

        nextPhase = default;
        return false;
    }

    public static bool LooksLikeRewardsState(ObserverSummary observer)
    {
        return string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.VisibleScreen, "rewards", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase)
               || observer.ActionNodes.Any(static node =>
                   node.Actionable
                   && (node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
                       || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)));
    }

    public static bool LooksLikeMapState(ObserverState observer)
    {
        return LooksLikeMapState(
            observer.Summary,
            TryReadObserverMetaString(observer.StateDocument, "declaringType"),
            TryReadObserverMetaString(observer.StateDocument, "instanceType"));
    }

    public static bool LooksLikeMapState(ObserverSummary observer)
    {
        return LooksLikeMapState(observer, null, null);
    }

    public static bool LooksLikeMapState(ObserverSummary observer, string? declaringType)
    {
        return LooksLikeMapState(observer, declaringType, null);
    }

    public static bool LooksLikeMapState(ObserverSummary observer, string? declaringType, string? instanceType)
    {
        return string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
               || IsMapScreenType(declaringType)
               || IsMapScreenType(instanceType)
               || observer.LastEventsTail.Any(IsMapTransitionEventTail);
    }

    public static bool LooksLikeCombatState(ObserverSummary observer)
    {
        return (string.Equals(observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase)
                || string.Equals(observer.VisibleScreen, "combat", StringComparison.OrdinalIgnoreCase))
               && observer.InCombat == true;
    }

    public static bool LooksLikeEventState(ObserverSummary observer, string? declaringType)
    {
        return LooksLikeEventState(observer, declaringType, null);
    }

    public static bool LooksLikeEventState(ObserverSummary observer, string? declaringType, string? instanceType)
    {
        if (LooksLikeMapState(observer, declaringType, instanceType))
        {
            return false;
        }

        if (string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.VisibleScreen, "event", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.ChoiceExtractorPath, "event", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(declaringType)
            && (declaringType.Contains("EventRoom", StringComparison.OrdinalIgnoreCase)
                || declaringType.Contains(".Events.", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return observer.ActionNodes.Any(static node =>
            node.Actionable
            && (node.NodeId.StartsWith("event-option:", StringComparison.OrdinalIgnoreCase)
                || string.Equals(node.Kind, "event-option", StringComparison.OrdinalIgnoreCase)
                || node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)));
    }

    private static bool IsMapScreenType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("Map.NMapScreen", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMapTransitionEventTail(string? eventTail)
    {
        return !string.IsNullOrWhiteSpace(eventTail)
               && (eventTail.Contains("screen-changed: map", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("map-point-selected", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("\"screen\":\"map\"", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("\"currentScreen\":\"map\"", StringComparison.OrdinalIgnoreCase));
    }

    public static string? TryReadObserverMetaString(JsonDocument? stateDocument, string propertyName)
    {
        if (stateDocument is null
            || !stateDocument.RootElement.TryGetProperty("meta", out var metaElement)
            || metaElement.ValueKind != JsonValueKind.Object
            || !metaElement.TryGetProperty(propertyName, out var valueElement)
            || valueElement.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return valueElement.GetString();
    }

    private static bool LooksLikeShopState(ObserverSummary observer)
    {
        return string.Equals(observer.CurrentScreen, "shop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.VisibleScreen, "shop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.EncounterKind, "Shop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "shop", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeRestSiteState(ObserverSummary observer)
    {
        if (string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase)
            || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer))
        {
            return true;
        }

        if (observer.Choices.Any(static choice =>
                !string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
                && (choice.Label.Contains("휴식", StringComparison.OrdinalIgnoreCase)
                    || choice.Label.Contains("Rest", StringComparison.OrdinalIgnoreCase)
                    || choice.Label.Contains("재련", StringComparison.OrdinalIgnoreCase)
                    || choice.Label.Contains("Smith", StringComparison.OrdinalIgnoreCase))))
        {
            return true;
        }

        return observer.Choices.Count == 0
               && observer.CurrentChoices.Any(static label =>
                   label.Contains("휴식", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Rest", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("재련", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Smith", StringComparison.OrdinalIgnoreCase));
    }
}

interface IGuiDecisionProvider
{
    Task<GuiSmokeStepDecision> GetDecisionAsync(string requestPath, string decisionPath, TimeSpan timeout, CancellationToken cancellationToken);
}

static class GuiSmokeCombatConstants
{
    public const double NonEnemyPrimeNormalizedX = 0.280;
    public const double NonEnemyPrimeNormalizedY = 0.620;
    public const double NonEnemyConfirmNormalizedX = 0.500;
    public const double NonEnemyConfirmNormalizedY = 0.560;
    public const int NonEnemyConfirmHoldMs = 75;
    public static readonly (double X, double Y, string Label)[] EnemyTargetCandidates =
    {
        (0.744, 0.542, "auto-target enemy"),
        (0.708, 0.532, "auto-target enemy recenter"),
        (0.778, 0.556, "auto-target enemy alternate"),
    };
}

sealed class SessionDecisionProvider : IGuiDecisionProvider
{
    public async Task<GuiSmokeStepDecision> GetDecisionAsync(string requestPath, string decisionPath, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(decisionPath))
            {
                using var stream = new FileStream(decisionPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                var parsed = await JsonSerializer.DeserializeAsync<GuiSmokeStepDecision>(stream, GuiSmokeShared.JsonOptions, cancellationToken).ConfigureAwait(false);
                if (parsed is not null)
                {
                    return parsed;
                }
            }

            await Task.Delay(500, cancellationToken).ConfigureAwait(false);
        }

        throw new TimeoutException($"Timed out waiting for decision file: {decisionPath}");
    }
}

sealed class AutoDecisionProvider : IGuiDecisionProvider
{
    private sealed class CandidateWorkItem
    {
        public required string Label { get; init; }
        public required string Source { get; init; }
        public required double Score { get; init; }
        public string? RejectReason { get; set; }
        public string? RawBounds { get; init; }
        public GuiSmokeCandidatePoint? NormalizedPoint { get; init; }
        public string? BoundsSource { get; init; }
        public string? TargetLabel { get; init; }
        public string? ActionKind { get; init; }
        public string? Reason { get; init; }
        public bool Selected { get; set; }
        public GuiSmokeStepDecision? Decision { get; init; }
    }

    private sealed class DecisionAnalysisBuilder
    {
        private readonly GuiSmokeStepRequest _request;
        private readonly List<CandidateWorkItem> _candidates = new();
        private readonly List<string> _activeCandidateSet = new();
        private readonly List<GuiSmokeSuppressedCandidate> _suppressedCandidates = new();
        private GuiSmokeStepDecision? _predictedDecision;
        private string _winnerSelectionReason = "no viable candidate";

        public DecisionAnalysisBuilder(GuiSmokeStepRequest request, string? foregroundKind, string? backgroundKind)
        {
            _request = request;
            ForegroundKind = foregroundKind;
            BackgroundKind = backgroundKind;
        }

        public string? ForegroundKind { get; }

        public string? BackgroundKind { get; }

        public void AddSuppressed(string label, string reason)
        {
            _suppressedCandidates.Add(new GuiSmokeSuppressedCandidate(label, reason));
        }

        public void Consider(
            string label,
            string source,
            double score,
            Func<GuiSmokeStepDecision?> factory,
            string rejectReason,
            string? rawBounds = null,
            string? boundsSource = null,
            GuiSmokeCandidatePoint? normalizedPoint = null)
        {
            _activeCandidateSet.Add(label);
            GuiSmokeStepDecision? decision = null;
            string? candidateRejectReason = null;
            try
            {
                decision = factory();
            }
            catch (Exception exception)
            {
                candidateRejectReason = $"factory-exception:{exception.GetType().Name}";
            }

            if (decision is null && candidateRejectReason is null)
            {
                candidateRejectReason = rejectReason;
            }

            if (decision is not null && _predictedDecision is not null)
            {
                candidateRejectReason = $"lower-priority-than:{_predictedDecision.TargetLabel ?? _predictedDecision.ActionKind ?? _predictedDecision.Status}";
            }

            if (decision is not null && _predictedDecision is null)
            {
                _predictedDecision = decision;
                _winnerSelectionReason = $"selected first viable candidate '{label}' from {source}.";
            }

            var point = normalizedPoint;
            if (point is null && decision?.NormalizedX is { } normalizedX && decision.NormalizedY is { } normalizedY)
            {
                point = new GuiSmokeCandidatePoint(normalizedX, normalizedY);
            }

            _candidates.Add(new CandidateWorkItem
            {
                Label = label,
                Source = source,
                Score = score,
                RejectReason = candidateRejectReason,
                RawBounds = rawBounds,
                NormalizedPoint = point,
                BoundsSource = boundsSource,
                TargetLabel = decision?.TargetLabel,
                ActionKind = decision?.ActionKind,
                Reason = decision?.Reason,
                Decision = decision,
            });
        }

        public GuiSmokeDecisionAnalysis Build(GuiSmokeStepDecision fallbackDecision, GuiSmokeStepDecision? actualDecision = null)
        {
            _predictedDecision ??= fallbackDecision;
            var finalDecision = actualDecision ?? _predictedDecision;
            var matchesPredicted = AreEquivalent(_predictedDecision, finalDecision);
            var selectedCandidate = _candidates.FirstOrDefault(candidate => AreEquivalent(candidate.Decision, finalDecision));
            if (selectedCandidate is not null)
            {
                selectedCandidate.Selected = true;
            }
            else
            {
                _candidates.Add(CreateExternalDecisionCandidate(finalDecision));
                _winnerSelectionReason = matchesPredicted
                    ? _winnerSelectionReason
                    : $"provider selected '{finalDecision.TargetLabel ?? finalDecision.ActionKind ?? finalDecision.Status}' outside the predicted candidate set.";
            }

            if (!matchesPredicted)
            {
                var predictedCandidate = _candidates.FirstOrDefault(candidate => AreEquivalent(candidate.Decision, _predictedDecision));
                if (predictedCandidate is not null && !predictedCandidate.Selected)
                {
                    predictedCandidate.RejectReason ??= "not-selected-by-provider";
                }
            }

            return new GuiSmokeDecisionAnalysis(
                _request.Phase,
                _request.ScreenshotPath,
                _predictedDecision,
                finalDecision,
                new GuiSmokeDecisionDebugSummary(
                    ForegroundKind,
                    BackgroundKind,
                    _activeCandidateSet.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                    _suppressedCandidates,
                    _winnerSelectionReason),
                _candidates.Select(candidate => new GuiSmokeDecisionCandidateDump(
                    candidate.Label,
                    candidate.Source,
                    candidate.Score,
                    candidate.Selected,
                    candidate.RejectReason,
                    candidate.RawBounds,
                    candidate.NormalizedPoint,
                    candidate.BoundsSource,
                    candidate.TargetLabel,
                    candidate.ActionKind,
                    candidate.Reason)).ToArray(),
                matchesPredicted);
        }

        private static CandidateWorkItem CreateExternalDecisionCandidate(GuiSmokeStepDecision decision)
        {
            return new CandidateWorkItem
            {
                Label = decision.TargetLabel ?? decision.ActionKind ?? decision.Status,
                Source = "provider:external",
                Score = 1.10d,
                Selected = true,
                RejectReason = null,
                RawBounds = null,
                NormalizedPoint = decision.NormalizedX is { } x && decision.NormalizedY is { } y
                    ? new GuiSmokeCandidatePoint(x, y)
                    : null,
                BoundsSource = decision.ActionKind is "click" or "right-click" ? "provider-external" : "provider-external-nonclick",
                TargetLabel = decision.TargetLabel,
                ActionKind = decision.ActionKind,
                Reason = decision.Reason,
                Decision = decision,
            };
        }

        private static bool AreEquivalent(GuiSmokeStepDecision? left, GuiSmokeStepDecision? right)
        {
            if (left is null || right is null)
            {
                return false;
            }

            return string.Equals(left.Status, right.Status, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(left.ActionKind, right.ActionKind, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(left.TargetLabel, right.TargetLabel, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(left.KeyText, right.KeyText, StringComparison.OrdinalIgnoreCase);
        }
    }

    public async Task<GuiSmokeStepDecision> GetDecisionAsync(string requestPath, string decisionPath, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var stream = new FileStream(requestPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var request = await JsonSerializer.DeserializeAsync<GuiSmokeStepRequest>(stream, GuiSmokeShared.JsonOptions, cancellationToken).ConfigureAwait(false)
                     ?? throw new InvalidOperationException("Failed to parse step request.");
        return Decide(request);
    }

    public static GuiSmokeDecisionAnalysis Analyze(GuiSmokeStepRequest request, GuiSmokeStepDecision? actualDecision = null)
    {
        var phase = Enum.Parse<GuiSmokePhase>(request.Phase, ignoreCase: true);
        return phase switch
        {
            GuiSmokePhase.HandleEvent => AnalyzeHandleEvent(request, actualDecision),
            GuiSmokePhase.HandleRewards => AnalyzeHandleRewards(request, actualDecision),
            GuiSmokePhase.ChooseFirstNode => AnalyzeChooseFirstNode(request, actualDecision),
            GuiSmokePhase.HandleCombat => AnalyzeGenericPhase(request, actualDecision, () => DecideHandleCombat(request), "combat", null),
            GuiSmokePhase.EnterRun => AnalyzeGenericPhase(request, actualDecision, () => DecideEnterRun(request), "main-menu", null),
            GuiSmokePhase.ChooseCharacter => AnalyzeGenericPhase(request, actualDecision, () => DecideChooseCharacter(request), "character-select", null),
            GuiSmokePhase.Embark => AnalyzeGenericPhase(request, actualDecision, () => DecideEmbark(request), "embark", null),
            _ => AnalyzeGenericPhase(request, actualDecision, () => CreateWaitDecision("waiting for passive phase", request.Observer.CurrentScreen), request.Observer.CurrentScreen, null),
        };
    }

    public static GuiSmokeStepDecision Decide(GuiSmokeStepRequest request)
    {
        return Analyze(request).FinalDecision;
    }

    private static GuiSmokeStepDecision DecideEnterRun(GuiSmokeStepRequest request)
    {
        if (string.Equals(request.Observer.CurrentScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.Observer.VisibleScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase))
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.275,
                0.445,
                "normal mode",
                "The singleplayer submenu is visible. Click the left 'normal' panel to start a standard run.",
                0.94,
                "singleplayer-submenu",
                1200,
                true,
                null);
        }

        return TryFindActionNodeDecision(request, "Continue", "continue")
               ?? TryFindActionNodeDecision(request, "\uACC4\uC18D", "continue")
               ?? TryFindActionNodeDecision(request, "Singleplayer", "singleplayer")
               ?? TryFindActionNodeDecision(request, "\uC2F1\uAE00", "singleplayer")
               ?? CreateWaitDecision("main menu actions not yet visible", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideChooseCharacter(GuiSmokeStepRequest request)
    {
        if (string.Equals(request.Observer.CurrentScreen, "character-select", StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.Observer.VisibleScreen, "character-select", StringComparison.OrdinalIgnoreCase))
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.955,
                0.723,
                "character confirm",
                "Ironclad is already highlighted on the character-select screen. Click the right confirm checkmark to continue.",
                0.94,
                "character-select",
                1000,
                true,
                null);
        }

        return TryFindActionNodeDecision(request, "Ironclad", "ironclad")
               ?? CreateWaitDecision("waiting for ironclad node", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideEmbark(GuiSmokeStepRequest request)
    {
        if (string.Equals(request.Observer.CurrentScreen, "character-select", StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.Observer.VisibleScreen, "character-select", StringComparison.OrdinalIgnoreCase))
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.955,
                0.723,
                "character confirm",
                "The run start confirm checkmark is visible on the character-select screen. Click it to embark.",
                0.94,
                "character-select",
                1000,
                true,
                null);
        }

        if (GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(request.Observer, out var postEmbarkPhase))
        {
            return postEmbarkPhase switch
            {
                GuiSmokePhase.HandleRewards => DecideHandleRewards(request with { Phase = GuiSmokePhase.HandleRewards.ToString() }),
                GuiSmokePhase.HandleEvent => DecideHandleEvent(request with { Phase = GuiSmokePhase.HandleEvent.ToString() }),
                GuiSmokePhase.HandleCombat => DecideHandleCombat(request with { Phase = GuiSmokePhase.HandleCombat.ToString() }),
                GuiSmokePhase.ChooseFirstNode => DecideChooseFirstNode(request with { Phase = GuiSmokePhase.ChooseFirstNode.ToString() }),
                _ => CreateWaitDecision("waiting for post-embark room state", request.Observer.CurrentScreen),
            };
        }

        return TryFindActionNodeDecision(request, "Embark", "embark")
               ?? CreateWaitDecision("waiting for embark action", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideHandleRewards(GuiSmokeStepRequest request)
    {
        var rewardMapLayer = BuildRewardMapLayerState(request.Observer, request.WindowBounds);
        var overlayDecision = TryCreateRoomOverlayCleanupDecision(request);
        if (overlayDecision is not null)
        {
            return overlayDecision;
        }

        var explicitRewardDecision = TryCreateExplicitRewardResolutionDecision(request);
        if (explicitRewardDecision is not null)
        {
            return explicitRewardDecision;
        }

        var rewardBackDecision = TryCreateRewardBackNavigationDecision(request);
        if (rewardBackDecision is not null)
        {
            return rewardBackDecision;
        }

        if ((rewardMapLayer.MapContextVisible
             || LooksLikeScreenshotFirstRoomState(request))
            && !rewardMapLayer.RewardPanelVisible)
        {
            var roomDecision = TryCreateScreenshotFirstRoomDecision(request);
            if (roomDecision is not null)
            {
                return roomDecision;
            }
        }

        return CreateWaitDecision("waiting for reward actions", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideChooseFirstNode(GuiSmokeStepRequest request)
    {
        if (HasExplicitRestSiteChoiceAuthority(request))
        {
            GuiSmokeDecisionDebug.SetSceneModel("rest-site", "map");
            GuiSmokeDecisionDebug.ReplaceActiveCandidates(BuildExplicitRestSiteCandidateLabels(request.Observer));
            GuiSmokeDecisionDebug.Suppress("click exported reachable node", "rest-site explicit choices outrank exported map routing");
            GuiSmokeDecisionDebug.Suppress("click first reachable node", "rest-site explicit choices outrank screenshot map routing");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "rest-site explicit choices suppress current-node-arrow fallback");
            GuiSmokeDecisionDebug.Suppress("click map back", "rest-site explicit choices are the progression lane");
            return GuiSmokeDecisionDebug.TraceCandidate(
                       "rest site explicit choice",
                       "rest-site-choice",
                       0.98,
                       TryCreateRestSiteDecision(request),
                       "rest-site explicit choices are not visible")
                   ?? GuiSmokeDecisionDebug.TraceCandidate(
                       "rest site upgrade",
                       "rest-site-upgrade",
                       0.92,
                       TryCreateRestSiteUpgradeDecision(request),
                       "rest-site upgrade grid is not currently actionable")
                   ?? CreateWaitDecision("waiting for an explicit rest-site choice", request.Observer.CurrentScreen);
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (mapOverlayState.ForegroundVisible)
        {
            GuiSmokeDecisionDebug.SetSceneModel("map-overlay", mapOverlayState.EventBackgroundPresent ? "event-context" : "map-context");
            GuiSmokeDecisionDebug.ReplaceActiveCandidates(
                mapOverlayState.MapBackNavigationAvailable
                    ? new[] { "click exported reachable node", "click first reachable node", "click map back", "wait" }
                    : new[] { "click exported reachable node", "click first reachable node", "wait" });
            if (mapOverlayState.StaleEventChoicePresent)
            {
                GuiSmokeDecisionDebug.Suppress("click event choice", "map overlay foreground suppresses stale event choices");
                GuiSmokeDecisionDebug.Suppress("click proceed", "map overlay foreground suppresses stale event proceed choices");
            }

            GuiSmokeDecisionDebug.Suppress("click visible map advance", "map overlay foreground suppresses current-node-arrow fallback");
            return GuiSmokeDecisionDebug.TraceCandidate(
                       "exported reachable map node",
                       "observer-map-node",
                       0.95,
                       TryCreateExportedReachableMapPointDecision(request),
                       "no exported reachable map node bounds available")
                   ?? GuiSmokeDecisionDebug.TraceCandidate(
                       "map back",
                       "map-overlay-back-nav",
                       0.86,
                       TryCreateMapBackNavigationDecision(request),
                       "map overlay back navigation is not available")
                   ?? GuiSmokeDecisionDebug.TraceCandidate(
                       "visible reachable node",
                       "screenshot-reachable-node",
                       0.90,
                       TryFindFirstReachableMapNodeDecision(request),
                       "no screenshot-reachable next node was detected")
                   ?? CreateWaitDecision("waiting for exported or screenshot-reachable map node", request.Observer.CurrentScreen);
        }

        if (LooksLikeScreenshotFirstRoomState(request))
        {
            var roomDecision = GuiSmokeDecisionDebug.TraceCandidate(
                "screenshot first room",
                "room-screenshot-fallback",
                0.75,
                TryCreateScreenshotFirstRoomDecision(request),
                "no explicit screenshot-first room action was available");
            if (roomDecision is not null)
            {
                return roomDecision;
            }
        }

        return GuiSmokeDecisionDebug.TraceCandidate(
                   "exported reachable map node",
                   "observer-map-node",
                   0.95,
                   TryCreateExportedReachableMapPointDecision(request),
                   "no exported reachable map node bounds available")
               ?? GuiSmokeDecisionDebug.TraceCandidate(
                   "visible map advance",
                   "screenshot-current-arrow",
                   0.78,
                   TryFindVisibleMapAdvanceDecision(request),
                   "no current-node-arrow fallback was permitted")
               ?? CreateWaitDecision("waiting for reachable map node", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideHandleShop(GuiSmokeStepRequest request)
    {
        return TryCreateHiddenOverlayCleanupDecision(request)
               ?? TryCreateVisibleProceedDecision(request)
               ?? CreateWaitDecision("waiting for shop exit or stable shop UI", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideHandleEvent(GuiSmokeStepRequest request)
    {
        var preferEventForeground = GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(request.Observer);
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        var strongEventForegroundChoice = HasStrongForegroundEventChoice(request);
        if (mapOverlayState.ForegroundVisible && !strongEventForegroundChoice)
        {
            GuiSmokeDecisionDebug.SetSceneModel("map-overlay", mapOverlayState.EventBackgroundPresent ? "event-context" : "map-context");
            GuiSmokeDecisionDebug.ReplaceActiveCandidates(
                mapOverlayState.MapBackNavigationAvailable
                    ? new[] { "click exported reachable node", "click first reachable node", "click map back", "wait" }
                    : new[] { "click exported reachable node", "click first reachable node", "wait" });
            GuiSmokeDecisionDebug.Suppress("click event choice", "map overlay foreground suppresses stale event choices");
            GuiSmokeDecisionDebug.Suppress("click proceed", "map overlay foreground suppresses stale event proceed choices");
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "map overlay foreground suppresses current-node-arrow fallback");
            return GuiSmokeDecisionDebug.TraceCandidate(
                       "exported reachable map node",
                       "observer-map-node",
                       0.95,
                       TryCreateExportedReachableMapPointDecision(request),
                       "no exported reachable map node bounds available")
                   ?? GuiSmokeDecisionDebug.TraceCandidate(
                       "map back",
                       "map-overlay-back-nav",
                       0.86,
                       TryCreateMapBackNavigationDecision(request),
                       "map overlay back navigation is not available")
                   ?? GuiSmokeDecisionDebug.TraceCandidate(
                       "visible reachable node",
                       "screenshot-reachable-node",
                       0.90,
                       TryFindFirstReachableMapNodeDecision(request),
                       "no screenshot-reachable next node was detected")
                   ?? CreateWaitDecision("waiting for map-overlay foreground resolution", request.Observer.CurrentScreen);
        }

        if (preferEventForeground)
        {
            GuiSmokeDecisionDebug.SetSceneModel("event", request.SceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase) ? "map-context" : null);
            GuiSmokeDecisionDebug.Suppress("click visible map advance", "event foreground keeps map-arrow evidence in the background only");
        }

        var overlayDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "inspect overlay cleanup",
            "overlay-cleanup",
            0.96,
            TryCreateRoomOverlayCleanupDecision(request),
            "no inspect overlay cleanup affordance is visible");
        if (overlayDecision is not null)
        {
            return overlayDecision;
        }

        var explicitRewardDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "explicit reward resolution",
            "reward-foreground",
            0.94,
            TryCreateExplicitRewardResolutionDecision(request),
            "no explicit reward foreground affordance is available");
        if (explicitRewardDecision is not null)
        {
            return explicitRewardDecision;
        }

        var rewardBackDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "reward back",
            "reward-back-nav",
            0.84,
            TryCreateRewardBackNavigationDecision(request),
            "reward back navigation is not available");
        if (rewardBackDecision is not null)
        {
            return rewardBackDecision;
        }

        if (preferEventForeground)
        {
            var semanticDecision = GuiSmokeDecisionDebug.TraceCandidate(
                "semantic event option",
                "event-semantic",
                0.92,
                TryCreateSemanticEventDecision(request),
                "no semantic event option matched current screenshot and observer evidence");
            if (semanticDecision is not null)
            {
                return semanticDecision;
            }

            var explicitEventChoice = GuiSmokeDecisionDebug.TraceCandidate(
                "explicit event choice",
                "event-choice",
                0.90,
                TryCreateEventProgressChoiceDecision(request),
                "no explicit event choice has usable bounds");
            if (explicitEventChoice is not null)
            {
                return explicitEventChoice;
            }
        }

        if (LooksLikeMapTransitionState(request))
        {
            return DecideChooseFirstNode(request with { Phase = GuiSmokePhase.ChooseFirstNode.ToString() });
        }

        var roomDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "screenshot first room",
            "room-screenshot-fallback",
            0.75,
            TryCreateScreenshotFirstRoomDecision(request),
            "no screenshot-first room action was available");
        if (roomDecision is not null)
        {
            return roomDecision;
        }

        if (!preferEventForeground)
        {
            var semanticDecision = GuiSmokeDecisionDebug.TraceCandidate(
                "semantic event option",
                "event-semantic",
                0.92,
                TryCreateSemanticEventDecision(request),
                "no semantic event option matched current screenshot and observer evidence");
            if (semanticDecision is not null)
            {
                return semanticDecision;
            }

            var explicitEventChoice = GuiSmokeDecisionDebug.TraceCandidate(
                "explicit event choice",
                "event-choice",
                0.90,
                TryCreateEventProgressChoiceDecision(request),
                "no explicit event choice has usable bounds");
            if (explicitEventChoice is not null)
            {
                return explicitEventChoice;
            }
        }

        return CreateWaitDecision("waiting for an explicit event progression choice", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision? TryCreateExplicitRewardResolutionDecision(GuiSmokeStepRequest request)
    {
        var rewardMapLayer = BuildRewardMapLayerState(request.Observer, request.WindowBounds);
        var rewardChoiceDecision = TryCreateRewardChoiceDecision(request);
        if (rewardChoiceDecision is not null)
        {
            return rewardChoiceDecision;
        }

        if (!rewardMapLayer.RewardPanelVisible
            && !HasStrongRewardScreenAuthority(request.Observer)
            && !LooksLikeRewardChoiceState(request.Observer)
            && !LooksLikeColorlessCardChoiceState(request.Observer))
        {
            return null;
        }

        var screenshotClaimableDecision = TryCreateScreenshotClaimableRewardDecision(request);
        if (screenshotClaimableDecision is not null)
        {
            return screenshotClaimableDecision;
        }

        var claimedRewardRecently = request.History.Any(entry =>
            string.Equals(entry.Phase, GuiSmokePhase.HandleRewards.ToString(), StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.TargetLabel, "claim reward item", StringComparison.OrdinalIgnoreCase));

        var rewardNode = request.Observer.ActionNodes
            .Where(node => IsCurrentRewardProgressionNode(node, request.WindowBounds))
            .Where(node => !IsProceedNode(node))
            .OrderByDescending(ScoreExplicitRewardProgressionNode)
            .ThenBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        if (rewardNode is not null && !claimedRewardRecently)
        {
            return CreateClickDecisionFromNode(request, rewardNode, "claim reward item");
        }

        var rewardChoice = request.Observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, request.WindowBounds))
            .Where(choice => !IsSkipOrProceedLabel(choice.Label))
            .OrderByDescending(ScoreExplicitRewardProgressionChoice)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (rewardChoice is not null && !claimedRewardRecently)
        {
            return CreateClickDecisionFromChoice(
                request,
                rewardChoice,
                "claim reward item",
                $"Reward choice '{rewardChoice.Label}' is explicitly visible. Claim it before using any proceed or map fallback.",
                0.93,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "rewards",
                1400);
        }

        var proceedChoice = request.Observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, request.WindowBounds))
            .Where(choice => IsSkipOrProceedLabel(choice.Label))
            .OrderByDescending(ScoreExplicitRewardProgressionChoice)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (proceedChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                proceedChoice,
                IsSkipLikeLabel(proceedChoice.Label) ? "reward skip" : "proceed after resolving rewards",
                $"Reward proceed choice '{proceedChoice.Label}' is explicitly visible. Use it before any screenshot-derived map fallback.",
                0.92,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "rewards",
                1400);
        }

        var proceedNode = request.Observer.ActionNodes
            .Where(node => IsCurrentRewardProgressionNode(node, request.WindowBounds))
            .Where(IsProceedNode)
            .OrderByDescending(ScoreExplicitRewardProgressionNode)
            .ThenBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        if (proceedNode is not null)
        {
            return CreateClickDecisionFromNode(request, proceedNode, "proceed after resolving rewards");
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateScreenshotClaimableRewardDecision(GuiSmokeStepRequest request)
    {
        if (!HasScreenshotClaimableRewardEvidenceInScreenshot(request.Observer, request.ScreenshotPath))
        {
            return null;
        }

        var rewardCardTarget = LooksLikeColorlessCardChoiceState(request.Observer)
            ? "colorless card choice"
            : "reward card choice";
        var cardGridAnalysis = AutoEventCardGridAnalyzer.Analyze(request.ScreenshotPath);
        if (!cardGridAnalysis.HasSelectableCard)
        {
            return null;
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            cardGridAnalysis.CardNormalizedX,
            cardGridAnalysis.CardNormalizedY,
            rewardCardTarget,
            "A claimable reward card is still visible in the screenshot. Choose it before falling back to skip, proceed, or map routing.",
            0.95,
            request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "rewards",
            1400,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateRoomOverlayCleanupDecision(GuiSmokeStepRequest request)
    {
        if (!LooksLikeInspectOverlayState(request.Observer))
        {
            return null;
        }

        var overlayCleanupAttempts = CountRecentOverlayCleanupAttempts(request.History);
        if (overlayCleanupAttempts >= 2)
        {
            return new GuiSmokeStepDecision(
                "act",
                "press-key",
                "Escape",
                null,
                null,
                "inspect overlay escape",
                "Inspect overlay remained open after repeated dismiss clicks. Send Escape before retrying any room progression.",
                0.84,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1000,
                true,
                null);
        }

        return TryCreateHiddenOverlayCleanupDecision(request)
               ?? TryCreateOverlayAdvanceDecision(request);
    }

    private static GuiSmokeStepDecision? TryCreateRewardBackNavigationDecision(GuiSmokeStepRequest request)
    {
        var rewardMapLayer = BuildRewardMapLayerState(request.Observer, request.WindowBounds);
        var backNavigationAvailable = rewardMapLayer.RewardBackNavigationAvailable
                                      || LooksLikeRewardBackNavigationAffordanceInScreenshot(request.Observer, request.ScreenshotPath);
        if (!backNavigationAvailable || !rewardMapLayer.MapContextVisible)
        {
            return null;
        }

        if (!HasScreenshotClaimableRewardEvidenceInScreenshot(request.Observer, request.ScreenshotPath)
            && !rewardMapLayer.StaleRewardChoicePresent)
        {
            return null;
        }

        var backNode = request.Observer.ActionNodes.FirstOrDefault(node => node.Actionable && IsBackNode(node) && HasActiveRewardBounds(node.ScreenBounds, request.WindowBounds));
        if (backNode is not null)
        {
            return CreateClickDecisionFromNode(request, backNode, "reward back");
        }

        if (LooksLikeRewardBackNavigationAffordanceInScreenshot(request.Observer, request.ScreenshotPath))
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.045,
                0.905,
                "reward back",
                "Map context is visible, but the bottom-left back arrow indicates the reward panel can be reopened. Return to rewards before abandoning claimable loot.",
                0.91,
                "rewards",
                1200,
                true,
                null);
        }

        return null;
    }

    private static bool LooksLikeRewardBackNavigationAffordanceInScreenshot(ObserverSummary observer, string? screenshotPath)
    {
        if (BuildRewardMapLayerState(observer, null).RewardBackNavigationAvailable)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(screenshotPath) || !File.Exists(screenshotPath))
        {
            return false;
        }

        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(screenshotPath);
        return overlayAnalysis.HasBottomLeftBackArrow
               && !overlayAnalysis.HasCentralOverlayPanel
               && (string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasScreenshotClaimableRewardEvidenceInScreenshot(ObserverSummary observer, string? screenshotPath)
    {
        if (string.IsNullOrWhiteSpace(screenshotPath) || !File.Exists(screenshotPath))
        {
            return false;
        }

        if (!string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(observer.VisibleScreen, "rewards", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return AutoEventCardGridAnalyzer.Analyze(screenshotPath).HasSelectableCard;
    }

    private static GuiSmokeStepDecision? TryCreateRewardChoiceDecision(GuiSmokeStepRequest request)
    {
        if (!LooksLikeRewardChoiceState(request.Observer)
            || !BuildRewardMapLayerState(request.Observer, request.WindowBounds).RewardPanelVisible)
        {
            return null;
        }

        var cardChoiceDecision = TryCreateCardRewardChoiceDecision(request);
        if (cardChoiceDecision is not null)
        {
            return cardChoiceDecision;
        }

        var bestChoice = request.Observer.Choices
            .Where(choice => ScoreProgressionChoice(choice) > 0 && IsCurrentRewardProgressionChoice(choice, request.WindowBounds))
            .OrderByDescending(ScoreProgressionChoice)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (bestChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                bestChoice,
                GetProgressChoiceTargetLabel(bestChoice, request.Observer),
                BuildProgressChoiceReason(bestChoice, request.Observer),
                0.91,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1400);
        }

        var bestNode = request.Observer.ActionNodes
            .Where(node => node.Actionable && IsCurrentRewardProgressionNode(node, request.WindowBounds) && ScoreProgressionNode(node) > 0)
            .OrderByDescending(ScoreProgressionNode)
            .ThenBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        if (bestNode is not null)
        {
            return CreateClickDecisionFromNode(request, bestNode, GetProgressChoiceTargetLabel(bestNode.Label, request.Observer));
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateCardRewardChoiceDecision(GuiSmokeStepRequest request)
    {
        var rewardCardTarget = LooksLikeColorlessCardChoiceState(request.Observer)
            ? "colorless card choice"
            : "reward card choice";
        if (!request.Observer.Choices.Any(choice => IsRewardCardChoice(choice) && HasActiveRewardBounds(choice.ScreenBounds, request.WindowBounds)))
        {
            return null;
        }

        var lastTarget = request.History
            .LastOrDefault(entry =>
                string.Equals(entry.Phase, request.Phase, StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase))
            ?.TargetLabel;
        var confirmChoice = request.Observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, request.WindowBounds) && IsConfirmLikeLabel(choice.Label))
            .OrderByDescending(GetChoiceSortX)
            .FirstOrDefault();
        var cardGridAnalysis = AutoEventCardGridAnalyzer.Analyze(request.ScreenshotPath);
        var canChooseCard = !string.Equals(lastTarget, rewardCardTarget, StringComparison.OrdinalIgnoreCase);
        if (canChooseCard && cardGridAnalysis.HasSelectableCard)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                cardGridAnalysis.CardNormalizedX,
                cardGridAnalysis.CardNormalizedY,
                rewardCardTarget,
                LooksLikeColorlessCardChoiceState(request.Observer)
                    ? "Colorless reward choice is visible. Select a card from the card area instead of clicking the relic inspect icons."
                    : "Reward card choice is visible. Select a card before pressing confirm or skip.",
                0.94,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1400,
                true,
                null);
        }

        if (confirmChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                confirmChoice,
                "event confirm",
                "Card selection substate is active. Confirm the current card selection.",
                0.92,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1400);
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateEventProgressChoiceDecision(GuiSmokeStepRequest request)
    {
        var bestNode = request.Observer.ActionNodes
            .Where(node => node.Actionable && TryParseNodeBounds(node.ScreenBounds, out _) && ScoreProgressionNode(node) > 0)
            .OrderByDescending(ScoreProgressionNode)
            .ThenBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        if (bestNode is not null)
        {
            return CreateClickDecisionFromNode(request, bestNode, GetProgressChoiceTargetLabel(bestNode.Label, request.Observer));
        }

        var bestChoice = request.Observer.Choices
            .Where(choice => ScoreProgressionChoice(choice) > 0 && TryParseNodeBounds(choice.ScreenBounds, out _))
            .OrderByDescending(ScoreProgressionChoice)
            .ThenBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (bestChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                bestChoice,
                GetProgressChoiceTargetLabel(bestChoice, request.Observer),
                BuildProgressChoiceReason(bestChoice, request.Observer),
                0.90,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1400);
        }

        return null;
    }

    private static GuiSmokeStepDecision DecideHandleCombat(GuiSmokeStepRequest request)
    {
        var analysis = AutoCombatAnalyzer.Analyze(request.ScreenshotPath);
        var handAnalysis = AutoCombatHandAnalyzer.Analyze(request.ScreenshotPath);
        var combatContext = HandleCombatContextSupport.Reconstruct(request.History);
        var pendingSelection = CombatRuntimeStateSupport.ResolvePendingSelection(request.Observer, request.CombatCardKnowledge, combatContext.PendingSelection);
        var runtimeCombatState = CombatRuntimeStateSupport.Read(request.Observer, request.CombatCardKnowledge);
        if (CombatEligibilitySupport.IsCombatPlayerActionWindowClosed(request.Observer))
        {
            return CreateWaitDecision("observer reports enemy turn or a closed combat play phase", request.Observer.CurrentScreen);
        }

        var hasSelectedNonEnemyConfirmEvidence = CombatEligibilitySupport.HasSelectedNonEnemyConfirmEvidence(request.Observer, request.CombatCardKnowledge, analysis, pendingSelection);
        var enemyTargetOpportunity = CanResolveEnemyTargetFromCurrentState(request.Observer, request.CombatCardKnowledge, analysis, pendingSelection);
        var combatNoOpLoop = combatContext.CombatNoOpLoop;
        var combatNoOpCountsBySlot = combatContext.CombatNoOpCountsBySlot;
        var repeatedNonEnemyLoop = combatContext.RepeatedNonEnemyLoop;
        var repeatedAttackSelectionLoop = combatContext.RepeatedAttackSelectionLoop;
        var observerHasAttackCard = request.Observer.CombatHand.Any(card =>
            card.SlotIndex >= 1
            && card.SlotIndex <= 5
            && IsAttackCombatHandCard(card)
            && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge));
        var observerHandHasOnlyNonEnemyOrInertCards = request.Observer.CombatHand.Count > 0
            && request.Observer.CombatHand.All(card =>
                IsNonEnemyCombatHandCard(card)
                || IsInertCombatHandCard(card)
                || !IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge));
        var hardBlockedAttackSlots = combatNoOpCountsBySlot
            .Where(static pair => pair.Value >= 2)
            .Select(static pair => pair.Key)
            .ToHashSet();
        var softBlockedAttackSlots = combatNoOpCountsBySlot
            .Where(static pair => pair.Value >= 1)
            .Select(static pair => pair.Key)
            .ToHashSet();
        var alternatePlayableAttackSlots = request.CombatCardKnowledge
            .Where(card => card.SlotIndex is >= 1 and <= 5)
            .Where(card => IsEnemyTargetCombatCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy))
            .Select(static card => card.SlotIndex)
            .Concat(request.Observer.CombatHand
                .Where(card => card.SlotIndex is >= 1 and <= 5)
                .Where(card => IsAttackCombatHandCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge))
                .Select(static card => card.SlotIndex))
            .Distinct()
            .Where(slotIndex => pendingSelection?.SlotIndex is null || slotIndex != pendingSelection.SlotIndex)
            .Where(slotIndex => !hardBlockedAttackSlots.Contains(slotIndex))
            .OrderBy(static slotIndex => slotIndex)
            .ToArray();
        var pendingSelectionNoOpCount = pendingSelection?.Kind == AutoCombatCardKind.AttackLike && pendingSelection.SlotIndex is >= 1 and <= 5
            ? HandleCombatContextSupport.GetCombatNoOpCountForSlot(combatContext, pendingSelection.SlotIndex)
            : 0;
        bool ShouldSuppressPendingNonEnemyReselect(int slotIndex)
        {
            if (pendingSelection?.Kind == AutoCombatCardKind.DefendLike
                && hasSelectedNonEnemyConfirmEvidence)
            {
                return true;
            }

            if (pendingSelection?.Kind == AutoCombatCardKind.DefendLike
                && pendingSelection.SlotIndex == slotIndex
                && !hasSelectedNonEnemyConfirmEvidence)
            {
                return true;
            }

            return !hasSelectedNonEnemyConfirmEvidence
                   && HandleCombatContextSupport.HasRecentNonEnemySelection(combatContext, slotIndex);
        }

        bool TryUseCombatDecision(GuiSmokeStepDecision? candidate, out GuiSmokeStepDecision allowedDecision)
        {
            allowedDecision = default!;
            if (candidate is null || !CombatDecisionContract.IsAllowed(request, candidate, out _))
            {
                return false;
            }

            allowedDecision = candidate;
            return true;
        }

        GuiSmokeStepDecision CloseWithLegalCombatFallback()
        {
            var fallbackDecision = new GuiSmokeStepDecision(
                "act",
                "press-key",
                "E",
                null,
                null,
                "auto-end turn",
                "No clear playable card remains in the screenshot. End the turn.",
                0.88,
                "combat",
                450,
                true,
                null);
            if (TryUseCombatDecision(fallbackDecision, out var allowedFallback))
            {
                return allowedFallback;
            }

            return CreateWaitDecision("waiting for legal combat action", request.Observer.CurrentScreen);
        }

        if (analysis.HasTargetArrow)
        {
            if (TryCreateCombatEnemyTargetDecision(request, pendingSelection, pendingSelectionNoOpCount, alternatePlayableAttackSlots, out var targetDecision)
                && TryUseCombatDecision(targetDecision, out var allowedTargetDecision))
            {
                return allowedTargetDecision;
            }
        }

        if (request.Observer.PlayerEnergy is <= 0)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "press-key",
                "E",
                null,
                null,
                "auto-end turn",
                "Observer reports no remaining energy. End the turn instead of retrying non-playable cards.",
                0.92,
                "combat",
                450,
                true,
                null), out var allowedNoEnergyDecision))
            {
                return allowedNoEnergyDecision;
            }
        }

        if (hasSelectedNonEnemyConfirmEvidence)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "confirm-non-enemy",
                null,
                null,
                null,
                "confirm selected non-enemy card",
                "A self or non-enemy targeted card is selected. Move to the explicit confirm point, hold left mouse briefly, then release to finish the play.",
                0.82,
                "combat",
                300,
                true,
                null), out var allowedNonEnemyConfirmDecision))
            {
                return allowedNonEnemyConfirmDecision;
            }
        }

        if (analysis.HasSelectedCard
            && analysis.SelectedCardKind == AutoCombatCardKind.AttackLike
            && pendingSelection?.Kind == AutoCombatCardKind.AttackLike
            && enemyTargetOpportunity)
        {
            if (TryCreateCombatEnemyTargetDecision(request, pendingSelection, pendingSelectionNoOpCount, alternatePlayableAttackSlots, out var targetDecision)
                && TryUseCombatDecision(targetDecision, out var allowedTargetDecision))
            {
                return allowedTargetDecision;
            }
        }

        if (pendingSelection?.Kind == AutoCombatCardKind.AttackLike
            && enemyTargetOpportunity)
        {
            if (TryCreateCombatEnemyTargetDecision(request, pendingSelection, pendingSelectionNoOpCount, alternatePlayableAttackSlots, out var targetDecision)
                && TryUseCombatDecision(targetDecision, out var allowedTargetDecision))
            {
                return allowedTargetDecision;
            }
        }

        if (pendingSelection?.Kind == AutoCombatCardKind.AttackLike
            && analysis.HasSelectedCard
            && !enemyTargetOpportunity)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "right-click",
                null,
                null,
                null,
                "cancel unresolved selected card",
                "A stale attack selection is still highlighted, but the observer no longer shows a matching playable attack. Cancel it before choosing a new card or ending the turn.",
                0.80,
                "combat",
                250,
                true,
                null), out var allowedCancelDecision))
            {
                return allowedCancelDecision;
            }
        }

        if (hasSelectedNonEnemyConfirmEvidence)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "confirm-non-enemy",
                null,
                null,
                null,
                "confirm selected non-enemy card",
                "A non-enemy card overlay is still selected. Use the explicit confirm point with a brief held mouse press instead of reissuing the slot hotkey.",
                0.78,
                "combat",
                300,
                true,
                null), out var allowedSelectedDefendDecision))
            {
                return allowedSelectedDefendDecision;
            }
        }

        var knowledgeAttackSlot = request.CombatCardKnowledge
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => IsEnemyTargetCombatCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy))
            .Where(card => !hardBlockedAttackSlots.Contains(card.SlotIndex))
            .Where(card => pendingSelectionNoOpCount <= 0 || !softBlockedAttackSlots.Contains(card.SlotIndex) || card.SlotIndex != pendingSelection?.SlotIndex)
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var observerAttackSlot = request.Observer.CombatHand
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => IsAttackCombatHandCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge))
            .Where(card => !hardBlockedAttackSlots.Contains(card.SlotIndex))
            .Where(card => pendingSelectionNoOpCount <= 0 || !softBlockedAttackSlots.Contains(card.SlotIndex) || card.SlotIndex != pendingSelection?.SlotIndex)
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var attackFallbackBlockedByObserver = request.Observer.CombatHand.Count > 0
            && (observerHandHasOnlyNonEnemyOrInertCards || !observerHasAttackCard);
        var attackSlot = !attackFallbackBlockedByObserver && knowledgeAttackSlot is not null
            ? new AutoCombatHandSlotAnalysis(
                knowledgeAttackSlot.SlotIndex,
                true,
                AutoCombatCardKind.AttackLike,
                double.MaxValue,
                double.MaxValue,
                0,
                0)
            : !attackFallbackBlockedByObserver && observerAttackSlot is not null
                ? new AutoCombatHandSlotAnalysis(
                observerAttackSlot.SlotIndex,
                true,
                AutoCombatCardKind.AttackLike,
                double.MaxValue,
                double.MaxValue,
                0,
                0)
                : attackFallbackBlockedByObserver
                    ? null
                    : handAnalysis.Slots
                    .Where(slot =>
                        slot.IsVisible
                        && slot.Kind == AutoCombatCardKind.AttackLike
                        && IsCompatibleScreenshotCombatSlot(slot, request.Observer, request.CombatCardKnowledge, expectEnemyTarget: true)
                        && !hardBlockedAttackSlots.Contains(slot.SlotIndex)
                        && (pendingSelectionNoOpCount <= 0 || !softBlockedAttackSlots.Contains(slot.SlotIndex) || slot.SlotIndex != pendingSelection?.SlotIndex))
                    .OrderByDescending(static slot => slot.RedBlueDelta)
                    .ThenByDescending(static slot => slot.Brightness)
                    .FirstOrDefault();
        if (attackSlot is not null)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "press-key",
                GetCombatSlotHotkey(attackSlot.SlotIndex),
                null,
                null,
                $"combat select attack slot {attackSlot.SlotIndex}",
                hardBlockedAttackSlots.Contains(attackSlot.SlotIndex) || pendingSelectionNoOpCount > 0
                    ? "Recent combat history shows no board delta on another lane. Switch to a different playable attack slot before trying to target the enemy again."
                    : "The screenshot still shows a playable attack card in hand. Use the corresponding hotkey first, then target the enemy.",
                hardBlockedAttackSlots.Count == 0 && pendingSelectionNoOpCount == 0 ? 0.80 : 0.88,
                "combat",
                250,
                true,
                null), out var allowedAttackSlotDecision))
            {
                return allowedAttackSlotDecision;
            }
        }

        var knowledgeNonEnemySlot = request.CombatCardKnowledge
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatCard(card, request.Observer.PlayerEnergy))
            .Where(card => !ShouldSuppressPendingNonEnemyReselect(card.SlotIndex))
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var observerNonEnemySlot = request.Observer.CombatHand
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatHandCard(card, request.Observer.PlayerEnergy, request.CombatCardKnowledge))
            .Where(card => !ShouldSuppressPendingNonEnemyReselect(card.SlotIndex))
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var nonEnemySlot = knowledgeNonEnemySlot is not null
            ? new AutoCombatHandSlotAnalysis(
                knowledgeNonEnemySlot.SlotIndex,
                true,
                AutoCombatCardKind.DefendLike,
                double.MinValue,
                double.MaxValue,
                0,
                0)
            : observerNonEnemySlot is not null
                ? new AutoCombatHandSlotAnalysis(
                observerNonEnemySlot.SlotIndex,
                true,
                AutoCombatCardKind.DefendLike,
                double.MinValue,
                double.MaxValue,
                0,
                0)
                : handAnalysis.Slots
                .Where(slot =>
                    slot.IsVisible
                    && slot.Kind == AutoCombatCardKind.DefendLike
                    && IsCompatibleScreenshotCombatSlot(slot, request.Observer, request.CombatCardKnowledge, expectEnemyTarget: false)
                    && !ShouldSuppressPendingNonEnemyReselect(slot.SlotIndex))
                .OrderBy(static slot => slot.RedBlueDelta)
                .ThenByDescending(static slot => slot.Brightness)
                .FirstOrDefault();
        if (nonEnemySlot is not null)
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "press-key",
                GetCombatSlotHotkey(nonEnemySlot.SlotIndex),
                null,
                null,
                $"combat select non-enemy slot {nonEnemySlot.SlotIndex}",
                "Only non-enemy cards remain in hand. Use the corresponding hotkey, then resolve the self or non-enemy confirmation.",
                0.74,
                "combat",
                250,
                true,
                null), out var allowedNonEnemySlotDecision))
            {
                return allowedNonEnemySlotDecision;
            }
        }

        var shouldDeferLoopEndTurn = runtimeCombatState.KeepsCardPlayOpen
                                     || CombatRuntimeStateSupport.ShouldDeferLoopEndTurn(
                                         request.Observer,
                                         request.CombatCardKnowledge);

        if (analysis.HasSelectedCard && HasRecentCombatCardSelection(request.History))
        {
            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "right-click",
                null,
                null,
                null,
                "cancel unresolved selected card",
                "A lingering selected card is still visible after the prior combat action. Cancel it before continuing.",
                0.72,
                "combat",
                250,
                true,
                null), out var allowedLingeringSelectionDecision))
            {
                return allowedLingeringSelectionDecision;
            }
        }

        if (repeatedNonEnemyLoop && !shouldDeferLoopEndTurn)
        {
            var endTurnNode = FindEndTurnActionNode(request);
            if (endTurnNode is not null
                && TryUseCombatDecision(
                    CreateClickDecisionFromNode(request, endTurnNode, "end turn after repeated non-enemy loop"),
                    out var allowedRepeatedNonEnemyDecision))
            {
                return allowedRepeatedNonEnemyDecision;
            }

            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "press-key",
                "E",
                null,
                null,
                "end turn after repeated non-enemy loop",
                "Recent combat history shows a repeated non-enemy select/confirm loop. End the turn instead of repeating the same sequence.",
                0.86,
                "combat",
                400,
                true,
                null), out var allowedRepeatedNonEnemyFallback))
            {
                return allowedRepeatedNonEnemyFallback;
            }
        }

        if (repeatedAttackSelectionLoop && !observerHasAttackCard && !shouldDeferLoopEndTurn)
        {
            var endTurnNode = FindEndTurnActionNode(request);
            if (endTurnNode is not null
                && TryUseCombatDecision(
                    CreateClickDecisionFromNode(request, endTurnNode, "end turn after repeated attack-select loop"),
                    out var allowedRepeatedAttackDecision))
            {
                return allowedRepeatedAttackDecision;
            }

            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "press-key",
                "E",
                null,
                null,
                "end turn after repeated attack-select loop",
                "Recent combat history shows repeated attack hotkeys without a matching observer attack card. End the turn instead of looping on screenshot drift.",
                0.88,
                "combat",
                400,
                true,
                null), out var allowedRepeatedAttackFallback))
            {
                return allowedRepeatedAttackFallback;
            }
        }

        if (combatNoOpLoop.LoopDetected && !shouldDeferLoopEndTurn)
        {
            var endTurnNode = FindEndTurnActionNode(request);
            if (endTurnNode is not null
                && TryUseCombatDecision(
                    CreateClickDecisionFromNode(request, endTurnNode, "end turn after combat no-op loop"),
                    out var allowedCombatNoOpDecision))
            {
                return allowedCombatNoOpDecision;
            }

            if (TryUseCombatDecision(new GuiSmokeStepDecision(
                "act",
                "press-key",
                "E",
                null,
                null,
                "end turn after combat no-op loop",
                "Combat has repeated the same card-select and enemy-target sequence without progress. End the turn instead of looping on an unproductive lane.",
                0.90,
                "combat",
                450,
                true,
                null), out var allowedCombatNoOpFallback))
            {
                return allowedCombatNoOpFallback;
            }
        }

        return CloseWithLegalCombatFallback();
    }

    private static GuiSmokeDecisionAnalysis AnalyzeGenericPhase(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision? actualDecision,
        Func<GuiSmokeStepDecision> factory,
        string? foregroundKind,
        string? backgroundKind)
    {
        var builder = new DecisionAnalysisBuilder(request, foregroundKind, backgroundKind);
        var predictedDecision = factory();
        builder.Consider(
            predictedDecision.TargetLabel ?? predictedDecision.ActionKind ?? predictedDecision.Status,
            "phase-generic",
            predictedDecision.Confidence ?? 0.50d,
            () => predictedDecision,
            "generic-phase-candidate-unavailable",
            boundsSource: predictedDecision.ActionKind is "click" or "right-click" ? "decision-normalized" : "decision-nonpoint");
        return builder.Build(CreateWaitDecision("waiting for passive phase", request.Observer.CurrentScreen), actualDecision);
    }

    private static GuiSmokeDecisionAnalysis AnalyzeHandleRewards(GuiSmokeStepRequest request, GuiSmokeStepDecision? actualDecision)
    {
        var rewardMapLayer = BuildRewardMapLayerState(request.Observer, request.WindowBounds);
        var (foregroundKind, backgroundKind) = DescribeForegroundBackground(request);
        var builder = new DecisionAnalysisBuilder(request, foregroundKind, backgroundKind);

        var overlayDecision = TryCreateRoomOverlayCleanupDecision(request);
        builder.Consider("click inspect overlay close", "overlay-cleanup", 1.00d, () => overlayDecision, "no-room-overlay-cleanup", boundsSource: "overlay-cleanup");

        var explicitRewardDecision = TryCreateExplicitRewardResolutionDecision(request);
        builder.Consider(
            ToCandidateLabel(explicitRewardDecision, "click reward choice"),
            "reward-explicit",
            0.95d,
            () => explicitRewardDecision,
            "no-explicit-reward-progression",
            rawBounds: TryFindRewardBounds(request),
            boundsSource: "observer-reward");

        var rewardBackDecision = TryCreateRewardBackNavigationDecision(request);
        builder.Consider("click reward back", "reward-back-nav", 0.88d, () => rewardBackDecision, "reward-back-not-available", rawBounds: TryFindBackBounds(request), boundsSource: "observer-back");

        if ((rewardMapLayer.MapContextVisible || LooksLikeScreenshotFirstRoomState(request)) && !rewardMapLayer.RewardPanelVisible)
        {
            var roomDecision = TryCreateScreenshotFirstRoomDecision(request);
            builder.Consider(
                ToCandidateLabel(roomDecision, "click room fallback"),
                "screenshot-room-fallback",
                0.70d,
                () => roomDecision,
                "no-room-fallback-available",
                boundsSource: "screenshot-room");
        }
        else
        {
            builder.AddSuppressed("click first reachable node", "reward-foreground-keeps-map-fallback-suppressed");
            builder.AddSuppressed("click visible map advance", "reward-foreground-keeps-map-fallback-suppressed");
        }

        return builder.Build(CreateWaitDecision("waiting for reward actions", request.Observer.CurrentScreen), actualDecision);
    }

    private static GuiSmokeDecisionAnalysis AnalyzeChooseFirstNode(GuiSmokeStepRequest request, GuiSmokeStepDecision? actualDecision)
    {
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        var (foregroundKind, backgroundKind) = DescribeForegroundBackground(request);
        var builder = new DecisionAnalysisBuilder(request, foregroundKind, backgroundKind);

        if (HasExplicitRestSiteChoiceAuthority(request))
        {
            builder.AddSuppressed("click exported reachable node", "rest-site-explicit-choices-outrank-exported-map-routing");
            builder.AddSuppressed("click first reachable node", "rest-site-explicit-choices-outrank-screenshot-map-routing");
            builder.AddSuppressed("click visible map advance", "rest-site-explicit-choices-suppress-current-node-arrow");
            builder.AddSuppressed("click map back", "rest-site-explicit-choices-are-the-progression-lane");
            foreach (var restSiteCandidate in BuildRestSiteDecisionCandidates(request))
            {
                var capturedCandidate = restSiteCandidate;
                builder.Consider(
                    capturedCandidate.Label,
                    capturedCandidate.Source,
                    capturedCandidate.Score,
                    () => capturedCandidate.Decision,
                    capturedCandidate.RejectReason ?? "no-rest-site-explicit-choice",
                    rawBounds: capturedCandidate.RawBounds,
                    boundsSource: capturedCandidate.BoundsSource);
            }

            builder.Consider(
                ToCandidateLabel(TryCreateRestSiteUpgradeDecision(request), "click smith card"),
                "rest-site-upgrade",
                0.92d,
                () => TryCreateRestSiteUpgradeDecision(request),
                "rest-site-upgrade-grid-not-visible");
            return builder.Build(CreateWaitDecision("waiting for an explicit rest-site choice", request.Observer.CurrentScreen), actualDecision);
        }

        if (LooksLikeRestSiteState(request.Observer)
            && TryCreateRestSiteUpgradeDecision(request) is { } observerUpgradeDecision)
        {
            builder.AddSuppressed("click exported reachable node", "rest-site-upgrade-outranks-exported-map-routing");
            builder.AddSuppressed("click first reachable node", "rest-site-upgrade-outranks-screenshot-map-routing");
            builder.AddSuppressed("click visible map advance", "rest-site-upgrade-suppresses-current-node-arrow");
            builder.AddSuppressed("click map back", "rest-site-upgrade-remains-foreground-authority");
            builder.Consider(
                ToCandidateLabel(observerUpgradeDecision, "click smith card"),
                "rest-site-upgrade",
                observerUpgradeDecision.TargetLabel?.Contains("confirm", StringComparison.OrdinalIgnoreCase) == true ? 0.95d : 0.92d,
                () => observerUpgradeDecision,
                "rest-site-upgrade-grid-not-visible",
                rawBounds: TryFindRestSiteUpgradeBounds(request, observerUpgradeDecision),
                boundsSource: TryResolveRestSiteUpgradeBoundsSource(request, observerUpgradeDecision));
            return builder.Build(CreateWaitDecision("waiting for smith grid or confirm state", request.Observer.CurrentScreen), actualDecision);
        }

        if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(request.Observer)
            && HasRecentRestSiteExplicitClick(request, "rest site: smith"))
        {
            builder.AddSuppressed("click exported reachable node", "rest-site-upgrade-observer-state-suppresses-map-routing");
            builder.AddSuppressed("click first reachable node", "rest-site-upgrade-observer-state-suppresses-map-routing");
            builder.AddSuppressed("click visible map advance", "rest-site-upgrade-observer-state-suppresses-current-node-arrow");
            builder.AddSuppressed("click map back", "rest-site-upgrade-observer-state-remains-foreground-authority");
            var observerMissDecision = CreateRestSitePostClickNoOpWaitDecision(request, "rest site: smith", request.Observer.CurrentScreen);
            builder.Consider(
                "click smith card",
                "rest-site-upgrade",
                0.92d,
                () => TryCreateRestSiteUpgradeDecision(request),
                "rest-site-upgrade-grid-not-visible");
            builder.Consider(
                "wait",
                "rest-site-upgrade-observer",
                0.97d,
                () => observerMissDecision,
                "rest-site-upgrade-post-click-state-unavailable");
            return builder.Build(observerMissDecision, actualDecision);
        }

        if (mapOverlayState.ForegroundVisible)
        {
            if (mapOverlayState.StaleEventChoicePresent)
            {
                builder.AddSuppressed("click event choice", "map-overlay-foreground-removes-stale-event-choice");
            }

            if (mapOverlayState.CurrentNodeArrowVisible)
            {
                builder.AddSuppressed("click visible map advance", "current-node-arrow-is-not-a-reachable-node");
            }

            builder.Consider(
                "click exported reachable node",
                "observer-export:map-node",
                1.00d,
                () => TryCreateExportedReachableMapPointDecision(request),
                "no-exported-reachable-node",
                rawBounds: TryFindMapNodeBounds(request),
                boundsSource: "observer-map-node");
            builder.Consider(
                "click map back",
                "overlay-back-navigation",
                0.90d,
                () => TryCreateMapBackNavigationDecision(request),
                "map-back-navigation-unavailable",
                rawBounds: TryFindBackBounds(request),
                boundsSource: "overlay-back");
            builder.Consider(
                "click first reachable node",
                "screenshot-reachable-node",
                0.82d,
                () => TryFindFirstReachableMapNodeDecision(request),
                "no-screenshot-reachable-node",
                boundsSource: "screenshot-map-node");
            return builder.Build(CreateWaitDecision("waiting for exported or screenshot-reachable map node", request.Observer.CurrentScreen), actualDecision);
        }

        if (LooksLikeScreenshotFirstRoomState(request))
        {
            if (AutoMapAnalyzer.Analyze(request.ScreenshotPath).HasCurrentArrow)
            {
                builder.AddSuppressed("click visible map advance", "room-explicit-choice-takes-priority-over-current-node-arrow");
            }

            var roomDecision = TryCreateScreenshotFirstRoomDecision(request);
            builder.Consider(
                ToCandidateLabel(roomDecision, "click room explicit choice"),
                "screenshot-room-explicit",
                0.96d,
                () => roomDecision,
                "no-room-explicit-choice");
            return builder.Build(CreateWaitDecision("waiting for reachable map node", request.Observer.CurrentScreen), actualDecision);
        }

        builder.Consider(
            "click exported reachable node",
            "observer-export:map-node",
            0.96d,
            () => TryCreateExportedReachableMapPointDecision(request),
            "no-exported-reachable-node",
            rawBounds: TryFindMapNodeBounds(request),
            boundsSource: "observer-map-node");
        builder.Consider(
            "click visible map advance",
            "screenshot-current-arrow",
            0.62d,
            () => TryFindVisibleMapAdvanceDecision(request),
            "visible-map-advance-suppressed-or-unavailable",
            boundsSource: "screenshot-map-arrow");
        return builder.Build(CreateWaitDecision("waiting for reachable map node", request.Observer.CurrentScreen), actualDecision);
    }

    private static GuiSmokeDecisionAnalysis AnalyzeHandleEvent(GuiSmokeStepRequest request, GuiSmokeStepDecision? actualDecision)
    {
        var preferEventForeground = GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(request.Observer);
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        var strongEventForegroundChoice = HasStrongForegroundEventChoice(request);
        var (foregroundKind, backgroundKind) = DescribeForegroundBackground(request);
        var builder = new DecisionAnalysisBuilder(request, foregroundKind, backgroundKind);

        var overlayDecision = TryCreateRoomOverlayCleanupDecision(request);
        builder.Consider("click inspect overlay close", "overlay-cleanup", 1.00d, () => overlayDecision, "no-room-overlay-cleanup", boundsSource: "overlay-cleanup");

        var explicitRewardDecision = TryCreateExplicitRewardResolutionDecision(request);
        builder.Consider(
            ToCandidateLabel(explicitRewardDecision, "click reward choice"),
            "reward-explicit",
            0.96d,
            () => explicitRewardDecision,
            "no-reward-resolution-needed",
            rawBounds: TryFindRewardBounds(request),
            boundsSource: "observer-reward");

        var rewardBackDecision = TryCreateRewardBackNavigationDecision(request);
        builder.Consider("click reward back", "reward-back-nav", 0.90d, () => rewardBackDecision, "reward-back-not-available", rawBounds: TryFindBackBounds(request), boundsSource: "observer-back");

        if (mapOverlayState.ForegroundVisible && !strongEventForegroundChoice)
        {
            if (mapOverlayState.StaleEventChoicePresent)
            {
                builder.AddSuppressed("click event choice", "map-overlay-foreground-removes-stale-event-choice");
            }

            if (mapOverlayState.CurrentNodeArrowVisible)
            {
                builder.AddSuppressed("click visible map advance", "current-node-arrow-is-not-a-reachable-node");
            }

            builder.Consider("click exported reachable node", "observer-export:map-node", 0.88d, () => TryCreateExportedReachableMapPointDecision(request), "no-exported-reachable-node", rawBounds: TryFindMapNodeBounds(request), boundsSource: "observer-map-node");
            builder.Consider("click map back", "overlay-back-navigation", 0.84d, () => TryCreateMapBackNavigationDecision(request), "map-back-navigation-unavailable", rawBounds: TryFindBackBounds(request), boundsSource: "overlay-back");
            builder.Consider("click first reachable node", "screenshot-reachable-node", 0.78d, () => TryFindFirstReachableMapNodeDecision(request), "no-screenshot-reachable-node", boundsSource: "screenshot-map-node");
            return builder.Build(CreateWaitDecision("waiting for an explicit event progression choice", request.Observer.CurrentScreen), actualDecision);
        }

        if (preferEventForeground
            && (request.SceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase)
                || request.SceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase)
                || request.SceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)))
        {
            builder.AddSuppressed("click visible map advance", "event-foreground-suppresses-background-map-contamination");
        }

        var semanticDecision = TryCreateSemanticEventDecision(request);
        builder.Consider("click event choice", "semantic-event", preferEventForeground ? 0.98d : 0.72d, () => semanticDecision, "no-semantic-event-choice", rawBounds: TryFindEventChoiceBounds(request), boundsSource: "observer-event");

        var explicitEventChoice = TryCreateEventProgressChoiceDecision(request);
        builder.Consider("click event choice", "observer:event-choice", preferEventForeground ? 0.94d : 0.68d, () => explicitEventChoice, "no-explicit-event-choice", rawBounds: TryFindEventChoiceBounds(request), boundsSource: "observer-event");

        if (LooksLikeMapTransitionState(request))
        {
            builder.Consider("click first reachable node", "branch:choose-first-node", 0.66d, () => DecideChooseFirstNode(request with { Phase = GuiSmokePhase.ChooseFirstNode.ToString() }), "no-map-transition-candidate", boundsSource: "branch-choose-first-node");
        }
        else
        {
            builder.AddSuppressed("click first reachable node", "event-foreground-keeps-map-transition-branch-suppressed");
        }

        var roomDecision = TryCreateScreenshotFirstRoomDecision(request);
        builder.Consider(ToCandidateLabel(roomDecision, "click room fallback"), "screenshot-room-fallback", 0.60d, () => roomDecision, "no-room-fallback-available", boundsSource: "screenshot-room");

        return builder.Build(CreateWaitDecision("waiting for an explicit event progression choice", request.Observer.CurrentScreen), actualDecision);
    }

    private static (string? ForegroundKind, string? BackgroundKind) DescribeForegroundBackground(GuiSmokeStepRequest request)
    {
        if (HasExplicitRestSiteChoiceAuthority(request))
        {
            return ("rest-site", "map");
        }

        if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(request.Observer))
        {
            return (RestSiteObserverSignals.HasSmithConfirmVisible(request.Observer) ? "rest-site-smith-confirm" : "rest-site-smith-grid", "rest-site");
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (mapOverlayState.ForegroundVisible && !HasStrongForegroundEventChoice(request))
        {
            return ("map-overlay", mapOverlayState.EventBackgroundPresent ? "event" : "map");
        }

        var rewardMapLayer = BuildRewardMapLayerState(request.Observer, request.WindowBounds);
        if (rewardMapLayer.RewardPanelVisible)
        {
            return ("reward", rewardMapLayer.MapContextVisible ? "map" : null);
        }

        if (GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(request.Observer))
        {
            return ("event", request.SceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase) ? "map" : null);
        }

        if (LooksLikeRestSiteState(request.Observer))
        {
            return ("rest-site", request.SceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase) ? "map" : null);
        }

        if (string.Equals(request.Observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return ("combat", null);
        }

        if (string.Equals(request.Observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase))
        {
            return ("map", null);
        }

        return (request.Observer.CurrentScreen, request.Observer.VisibleScreen);
    }

    private static bool HasExplicitRestSiteChoiceAuthority(GuiSmokeStepRequest request)
    {
        return HasRestSiteAuthority(request.Observer)
               && HasExplicitRestSiteChoiceAffordance(request.Observer)
               && !RestSiteObserverSignals.IsRestSiteSmithUpgradeState(request.Observer)
               && !AutoRestSiteCardGridAnalyzer.Analyze(request.ScreenshotPath).HasSelectableCard;
    }

    private static bool HasRestSiteAuthority(ObserverSummary observer)
    {
        return string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase)
               || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer);
    }

    private static string[] BuildExplicitRestSiteCandidateLabels(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.BuildCandidateLabels(observer).ToArray();
    }

    private static bool HasExplicitRestSiteChoiceAffordance(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(observer);
    }

    private static bool HasRestSiteRestChoice(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.HasRestSiteRestChoice(observer);
    }

    private static bool HasRestSiteSmithChoice(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.HasRestSiteSmithChoice(observer);
    }

    private static bool HasRestSiteHatchChoice(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.HasRestSiteHatchChoice(observer);
    }

    private static bool IsExplicitRestSiteChoiceLabel(string? label)
    {
        return HasRestSiteRestLabel(label)
               || HasRestSiteSmithLabel(label)
               || HasRestSiteHatchLabel(label);
    }

    private static bool HasRestSiteRestLabel(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("휴식", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Rest", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasRestSiteSmithLabel(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("재련", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Smith", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasRestSiteHatchLabel(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("부화", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Hatch", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasStrongForegroundEventChoice(GuiSmokeStepRequest request)
    {
        static bool IsGenericContinueLabel(string? label)
        {
            return !string.IsNullOrWhiteSpace(label)
                   && (label.Contains("계속", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("Continue", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("Proceed", StringComparison.OrdinalIgnoreCase));
        }

        var activeEventChoices = request.Observer.Choices.Any(choice =>
            HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds)
            && !IsGenericContinueLabel(choice.Label)
            && !IsBackChoiceLabel(choice.Label)
            && (string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)));
        if (activeEventChoices)
        {
            return true;
        }

        return request.Observer.ActionNodes.Any(node =>
            node.Actionable
            && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds)
            && !IsGenericContinueLabel(node.Label)
            && !IsBackChoiceLabel(node.Label)
            && node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesRestSiteTarget(string? label, string targetLabel)
    {
        return RestSiteChoiceSupport.MapOptionIdToTargetLabel(RestSiteChoiceSupport.MapLabelToOptionId(label) ?? string.Empty)
            .Equals(targetLabel, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildRestSiteChoiceReason(string targetLabel, int currentHp, int maxHp)
    {
        return targetLabel switch
        {
            "rest site: rest" => $"Rest site detected from explicit choices. HP {currentHp}/{maxHp} favors healing before routing again.",
            "rest site: hatch" => "Rest site detected from explicit choices. Hatch is visible and no stronger rest/smith lane is available.",
            _ => "Rest site detected from explicit choices. HP is healthy enough to prefer smithing over generic map routing.",
        };
    }

    private static IReadOnlyList<RestSiteDecisionCandidate> BuildRestSiteDecisionCandidates(GuiSmokeStepRequest request)
    {
        var observedChoices = RestSiteChoiceSupport.GetObservedChoices(request.Observer);
        if (observedChoices.Count == 0)
        {
            return Array.Empty<RestSiteDecisionCandidate>();
        }

        var selectedOptionId = RestSiteChoiceSupport.DetermineAutoPickOptionId(request.Observer);
        return observedChoices
            .Select(choice => BuildRestSiteDecisionCandidate(request, choice, selectedOptionId))
            .ToArray();
    }

    private static RestSiteDecisionCandidate BuildRestSiteDecisionCandidate(
        GuiSmokeStepRequest request,
        RestSiteObservedChoice choice,
        string? selectedOptionId)
    {
        var targetLabel = choice.TargetLabel;
        var rawBounds = choice.HasMetadata
            ? choice.Choice?.ScreenBounds
            : choice.Choice?.ScreenBounds ?? choice.ActionNode?.ScreenBounds;
        var boundsSource = choice.HasMetadata
            ? !string.IsNullOrWhiteSpace(choice.Choice?.ScreenBounds) ? "observer-rest-site-choice" : null
            : !string.IsNullOrWhiteSpace(choice.Choice?.ScreenBounds)
                ? "observer-rest-site-choice-legacy"
                : !string.IsNullOrWhiteSpace(choice.ActionNode?.ScreenBounds)
                    ? "observer-rest-site-node-legacy"
                    : "fixed-rest-site-fallback";
        var rejectReason = "no-rest-site-decision";
        GuiSmokeStepDecision? decision = null;

        if (!choice.IsDefaultAutoPick)
        {
            rejectReason = "not-default-auto-pick";
        }
        else if (string.IsNullOrWhiteSpace(selectedOptionId)
                 || !string.Equals(choice.OptionId, selectedOptionId, StringComparison.OrdinalIgnoreCase))
        {
            rejectReason = "not-selected-by-rest-site-policy";
        }
        else if (choice.HasMetadata && !choice.IsEnabled)
        {
            rejectReason = "disabled-explicit-choice";
        }
        else
        {
            var repeatState = GetRestSiteExplicitChoiceRepeatState(request, targetLabel);
            if (repeatState == RestSiteExplicitChoiceRepeatState.NoOpDetected)
            {
                decision = CreateRestSitePostClickNoOpWaitDecision(request, targetLabel, request.Observer.CurrentScreen);
            }
            else if (repeatState == RestSiteExplicitChoiceRepeatState.GraceNeeded)
            {
                decision = CreateRestSiteGraceWaitDecision(targetLabel, request.Observer.CurrentScreen);
            }
            else if (choice.HasMetadata)
            {
                if (string.IsNullOrWhiteSpace(choice.Choice?.ScreenBounds))
                {
                    rejectReason = "missing-hitbox-for-explicit-choice";
                }
                else if (choice.Choice is null || !HasActiveNodeBounds(choice.Choice.ScreenBounds, request.WindowBounds))
                {
                    rejectReason = "stale-hitbox-for-explicit-choice";
                }
                else
                {
                    var currentHp = request.Observer.PlayerCurrentHp ?? 0;
                    var maxHp = request.Observer.PlayerMaxHp ?? 0;
                    decision = CreateClickDecisionFromChoice(
                        request,
                        choice.Choice,
                        targetLabel,
                        BuildRestSiteChoiceReason(targetLabel, currentHp, maxHp),
                        0.92,
                        request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                        1400);
                }
            }
            else
            {
                decision = TryCreateLegacyRestSiteDecision(request, choice);
                rejectReason = decision is null
                    ? "legacy-rest-site-choice-unavailable"
                    : "legacy-rest-site-choice-selected";
            }
        }

        return new RestSiteDecisionCandidate(
            choice.CandidateLabel,
            choice.HasMetadata ? "observer-rest-site-choice" : "legacy-rest-site-choice",
            GetRestSiteCandidateScore(choice.OptionId),
            rejectReason,
            rawBounds,
            boundsSource,
            decision);
    }

    private static GuiSmokeStepDecision? TryCreateLegacyRestSiteDecision(GuiSmokeStepRequest request, RestSiteObservedChoice choice)
    {
        var currentHp = request.Observer.PlayerCurrentHp ?? 0;
        var maxHp = request.Observer.PlayerMaxHp ?? 0;
        if (choice.Choice is not null
            && HasActiveNodeBounds(choice.Choice.ScreenBounds, request.WindowBounds))
        {
            return CreateClickDecisionFromChoice(
                request,
                choice.Choice,
                choice.TargetLabel,
                BuildRestSiteChoiceReason(choice.TargetLabel, currentHp, maxHp),
                0.92,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                1400);
        }

        if (choice.ActionNode is not null
            && choice.ActionNode.Actionable
            && HasActiveNodeBounds(choice.ActionNode.ScreenBounds, request.WindowBounds))
        {
            return CreateClickDecisionFromNode(request, choice.ActionNode, choice.TargetLabel);
        }

        var normalizedX = choice.TargetLabel switch
        {
            "rest site: rest" => 0.405,
            "rest site: smith" => 0.575,
            "rest site: hatch" => 0.745,
            _ => 0.575,
        };
        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            0.305,
            choice.TargetLabel,
            BuildRestSiteChoiceReason(choice.TargetLabel, currentHp, maxHp),
            0.86,
            "map",
            1500,
            true,
            null);
    }

    private static double GetRestSiteCandidateScore(string optionId)
    {
        return RestSiteChoiceSupport.NormalizeOptionId(optionId) switch
        {
            "HEAL" => 1.00d,
            "SMITH" => 0.98d,
            "HATCH" => 0.94d,
            _ => 0.72d,
        };
    }

    private static GuiSmokeStepDecision CreateRestSiteGraceWaitDecision(string targetLabel, string? expectedScreen)
    {
        return new GuiSmokeStepDecision(
            "wait",
            null,
            null,
            null,
            null,
            targetLabel,
            $"Rest-site explicit choice '{targetLabel}' persisted after a click. Allow one recapture before escalating to a no-op diagnosis.",
            0.70d,
            expectedScreen,
            700,
            true,
            null,
            DecisionRisk: "rest-site-post-click-recapture-grace");
    }

    private static GuiSmokeStepDecision CreateRestSitePostClickNoOpWaitDecision(GuiSmokeStepRequest request, string targetLabel, string? expectedScreen)
    {
        var evidence = RestSiteObserverSignals.BuildPostClickEvidence(request.Observer, targetLabel);
        var reason = evidence.Classification switch
        {
            "rest-site-selection-failed" => $"Rest-site explicit choice '{targetLabel}' reported an after-select failure in observer metadata. Stop repeating the click and surface the failed selection directly.",
            "rest-site-grid-not-visible-after-selection" => $"Rest-site explicit choice '{targetLabel}' was accepted for selection, but no smith grid or confirm state became observer-visible after grace recapture.",
            "rest-site-grid-observer-miss" => $"Rest-site smith upgrade screen became runtime-visible, but the observer did not export card or confirm hitboxes. Stop repeating the smith click and surface the observer miss.",
            _ => $"Rest-site explicit choice '{targetLabel}' remained on the same fingerprint after grace recapture. Stop repeating the click and escalate to rest-site-post-click-noop.",
        };
        return new GuiSmokeStepDecision(
            "wait",
            null,
            null,
            null,
            null,
            targetLabel,
            reason,
            0.72d,
            expectedScreen,
            250,
            true,
            null,
            DecisionRisk: evidence.Classification);
    }

    private static RestSiteExplicitChoiceRepeatState GetRestSiteExplicitChoiceRepeatState(GuiSmokeStepRequest request, string targetLabel)
    {
        if (!HasExplicitRestSiteChoiceAuthority(request)
            || !IsExplicitRestSiteOptionTarget(targetLabel))
        {
            return RestSiteExplicitChoiceRepeatState.None;
        }

        var fingerprint = RestSiteChoiceSupport.BuildExplicitChoiceFingerprint(request.Observer);
        for (var index = request.History.Count - 1; index >= 0; index -= 1)
        {
            var entry = request.History[index];
            if (TryParseRestSiteActionMetadata(entry.Metadata, out var metadata))
            {
                if (!string.Equals(metadata.TargetLabel, targetLabel, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(metadata.Fingerprint, fingerprint, StringComparison.Ordinal))
                {
                    return RestSiteExplicitChoiceRepeatState.None;
                }

                return string.Equals(metadata.Kind, "explicit-grace-wait", StringComparison.OrdinalIgnoreCase)
                    ? RestSiteExplicitChoiceRepeatState.NoOpDetected
                    : string.Equals(metadata.Kind, "explicit-click", StringComparison.OrdinalIgnoreCase)
                        ? RestSiteExplicitChoiceRepeatState.GraceNeeded
                        : RestSiteExplicitChoiceRepeatState.None;
            }

            if (entry.Action is "click" or "click-current" or "confirm-non-enemy" or "right-click" or "press-key")
            {
                return RestSiteExplicitChoiceRepeatState.None;
            }
        }

        return RestSiteExplicitChoiceRepeatState.None;
    }

    private static bool HasRecentRestSiteExplicitClick(GuiSmokeStepRequest request, string? targetLabel)
    {
        if (!IsExplicitRestSiteOptionTarget(targetLabel))
        {
            return false;
        }

        for (var index = request.History.Count - 1; index >= 0; index -= 1)
        {
            var entry = request.History[index];
            if (!TryParseRestSiteActionMetadata(entry.Metadata, out var metadata))
            {
                continue;
            }

            return string.Equals(metadata.Kind, "explicit-click", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(metadata.TargetLabel, targetLabel, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool IsExplicitRestSiteOptionTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "rest site: rest", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: smith", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: hatch", StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(targetLabel)
                   && targetLabel.StartsWith("rest site: option:", StringComparison.OrdinalIgnoreCase));
    }

    private static string? BuildRestSiteHistoryMetadata(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        if (!HasExplicitRestSiteChoiceAuthority(request)
            || !IsExplicitRestSiteOptionTarget(decision.TargetLabel))
        {
            return null;
        }

        var kind = string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(decision.ActionKind, "click", StringComparison.OrdinalIgnoreCase)
            ? "explicit-click"
            : string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase)
              && string.Equals(decision.DecisionRisk, "rest-site-post-click-recapture-grace", StringComparison.OrdinalIgnoreCase)
                ? "explicit-grace-wait"
                : null;
        if (kind is null)
        {
            return null;
        }

        return JsonSerializer.Serialize(
            new RestSiteActionMetadata(
                kind,
                decision.TargetLabel!,
                RestSiteChoiceSupport.BuildExplicitChoiceFingerprint(request.Observer)),
            GuiSmokeShared.JsonOptions);
    }

    private static bool TryParseRestSiteActionMetadata(string? metadata, out RestSiteActionMetadata parsed)
    {
        parsed = default!;
        if (string.IsNullOrWhiteSpace(metadata))
        {
            return false;
        }

        try
        {
            parsed = JsonSerializer.Deserialize<RestSiteActionMetadata>(metadata, GuiSmokeShared.JsonOptions)!;
            return parsed is not null
                   && !string.IsNullOrWhiteSpace(parsed.Kind)
                   && !string.IsNullOrWhiteSpace(parsed.TargetLabel)
                   && !string.IsNullOrWhiteSpace(parsed.Fingerprint);
        }
        catch
        {
            return false;
        }
    }

    public static string? BuildRestSiteHistoryMetadataForDecision(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        return BuildRestSiteHistoryMetadata(request, decision);
    }

    public static bool HasRecentRestSiteExplicitClickForRequest(GuiSmokeStepRequest request, string? targetLabel)
    {
        return HasRecentRestSiteExplicitClick(request, targetLabel);
    }

    public static bool HasExplicitRestSiteChoiceAuthorityForRequest(GuiSmokeStepRequest request)
    {
        return HasExplicitRestSiteChoiceAuthority(request);
    }

    public static bool IsExplicitRestSiteOptionTargetLabel(string? targetLabel)
    {
        return IsExplicitRestSiteOptionTarget(targetLabel);
    }

    private static string ToCandidateLabel(GuiSmokeStepDecision? decision, string fallback)
    {
        return decision?.TargetLabel switch
        {
            { } target when target.Contains("reward", StringComparison.OrdinalIgnoreCase) => "click reward choice",
            { } target when target.Contains("event", StringComparison.OrdinalIgnoreCase) => "click event choice",
            { } target when target.Contains("rest site:", StringComparison.OrdinalIgnoreCase) => "click rest site choice",
            { } target when target.Contains("smith card", StringComparison.OrdinalIgnoreCase) => "click smith card",
            { } target when target.Contains("smith confirm", StringComparison.OrdinalIgnoreCase) => "click smith confirm",
            { } target when target.Contains("map back", StringComparison.OrdinalIgnoreCase) => "click map back",
            { } target when target.Contains("exported reachable map node", StringComparison.OrdinalIgnoreCase) => "click exported reachable node",
            { } target when target.Contains("visible map advance", StringComparison.OrdinalIgnoreCase) => "click visible map advance",
            _ => fallback,
        };
    }

    private static string? TryFindMapNodeBounds(GuiSmokeStepRequest request)
    {
        return request.Observer.Choices.FirstOrDefault(choice =>
                   string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
                   && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds))?.ScreenBounds
               ?? request.Observer.ActionNodes.FirstOrDefault(node =>
                   node.Actionable
                   && string.Equals(node.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
                   && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds))?.ScreenBounds;
    }

    private static string? TryFindBackBounds(GuiSmokeStepRequest request)
    {
        return request.Observer.ActionNodes.FirstOrDefault(node =>
                   node.Actionable
                   && IsBackNode(node)
                   && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds))?.ScreenBounds
               ?? request.Observer.Choices.FirstOrDefault(choice =>
                   IsBackChoiceLabel(choice.Label)
                   && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds))?.ScreenBounds;
    }

    private static string? TryFindEventChoiceBounds(GuiSmokeStepRequest request)
    {
        return request.Observer.ActionNodes.FirstOrDefault(node =>
                   node.Actionable
                   && node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
                   && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds))?.ScreenBounds
               ?? request.Observer.Choices.FirstOrDefault(choice =>
                   string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
                   && !IsBackChoiceLabel(choice.Label)
                   && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds))?.ScreenBounds;
    }

    private static string? TryFindRewardBounds(GuiSmokeStepRequest request)
    {
        return request.Observer.ActionNodes.FirstOrDefault(node =>
                   node.Actionable
                   && IsExplicitRewardProgressionNode(node)
                   && HasActiveRewardBounds(node.ScreenBounds, request.WindowBounds))?.ScreenBounds
               ?? request.Observer.Choices.FirstOrDefault(choice =>
                   IsExplicitRewardProgressionChoice(choice)
                   && HasActiveRewardBounds(choice.ScreenBounds, request.WindowBounds))?.ScreenBounds;
    }

    private static bool HasRecentCombatCardSelection(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return CombatHistorySupport.TryGetPendingCombatSelection(history) is not null;
    }

    private static bool IsCompatibleScreenshotCombatSlot(
        AutoCombatHandSlotAnalysis slot,
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        bool expectEnemyTarget)
    {
        var observerCard = observer.CombatHand.FirstOrDefault(card => card.SlotIndex == slot.SlotIndex);
        if (observerCard is not null)
        {
            return expectEnemyTarget
                ? IsAttackCombatHandCard(observerCard) && IsPlayableAtCurrentEnergy(observerCard, observer.PlayerEnergy, combatCardKnowledge)
                : CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatHandCard(observerCard, observer.PlayerEnergy, combatCardKnowledge);
        }

        var knowledgeCard = combatCardKnowledge.FirstOrDefault(card => card.SlotIndex == slot.SlotIndex);
        if (knowledgeCard is not null)
        {
            return expectEnemyTarget
                ? IsEnemyTargetCombatCard(knowledgeCard) && IsPlayableAtCurrentEnergy(knowledgeCard, observer.PlayerEnergy)
                : CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatCard(knowledgeCard, observer.PlayerEnergy);
        }

        return observer.CombatHand.Count == 0 && combatCardKnowledge.Count == 0;
    }

    private static bool TryCreateCombatEnemyTargetDecision(
        GuiSmokeStepRequest request,
        PendingCombatSelection? pendingSelection,
        int pendingSelectionNoOpCount,
        IReadOnlyList<int> alternatePlayableAttackSlots,
        out GuiSmokeStepDecision decision)
    {
        if (pendingSelection?.Kind != AutoCombatCardKind.AttackLike)
        {
            decision = default!;
            return false;
        }

        if (pendingSelectionNoOpCount >= 2)
        {
            if (alternatePlayableAttackSlots.Count > 0)
            {
                decision = new GuiSmokeStepDecision(
                    "act",
                    "press-key",
                    GetCombatSlotHotkey(alternatePlayableAttackSlots[0]),
                    null,
                    null,
                    $"combat select attack slot {alternatePlayableAttackSlots[0]}",
                    $"Recent combat history shows no board delta after targeting from slot {pendingSelection.SlotIndex}. Switch to another playable attack lane before trying to target the enemy again.",
                    0.91,
                    "combat",
                    250,
                    true,
                    null);
                return true;
            }

            decision = default!;
            return false;
        }

        if (TryCreateCombatEnemyTargetDecisionFromObservedNodes(request, pendingSelection, pendingSelectionNoOpCount, out decision))
        {
            return true;
        }

        var targetCandidateIndex = Math.Clamp(pendingSelectionNoOpCount, 0, GuiSmokeCombatConstants.EnemyTargetCandidates.Length - 1);
        var targetCandidate = GuiSmokeCombatConstants.EnemyTargetCandidates[targetCandidateIndex];
        var reason = pendingSelectionNoOpCount == 0
            ? "An attack card is selected. Click the enemy body to resolve it."
            : $"The previous enemy click from slot {pendingSelection.SlotIndex} produced no board delta. Recenter the target click before abandoning the lane.";
        decision = new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            targetCandidate.X,
            targetCandidate.Y,
            targetCandidate.Label,
            reason,
            pendingSelectionNoOpCount == 0 ? 0.90 : 0.86,
            "combat",
            350,
            true,
            null);
        return true;
    }

    private static bool TryCreateCombatEnemyTargetDecisionFromObservedNodes(
        GuiSmokeStepRequest request,
        PendingCombatSelection pendingSelection,
        int pendingSelectionNoOpCount,
        out GuiSmokeStepDecision decision)
    {
        var targetNodes = GetCombatEnemyTargetNodes(request.Observer, request.WindowBounds);
        if (targetNodes.Count == 0)
        {
            decision = default!;
            return false;
        }

        var targetNode = pendingSelectionNoOpCount == 0
            ? targetNodes[0]
            : targetNodes[Math.Clamp(pendingSelectionNoOpCount, 0, targetNodes.Count - 1)];
        var targetLabel = BuildCombatEnemyTargetLabel(targetNode, pendingSelectionNoOpCount);
        decision = CreateCombatEnemyTargetDecisionFromNode(request, targetNode, targetLabel, pendingSelectionNoOpCount);
        return true;
    }

    private static bool CanResolveEnemyTargetFromCurrentState(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        AutoCombatAnalysis analysis,
        PendingCombatSelection? pendingSelection)
    {
        if (CombatRuntimeStateSupport.CanResolveEnemyTarget(observer, combatCardKnowledge, pendingSelection, analysis))
        {
            return true;
        }

        if (GetCombatEnemyTargetNodes(observer).Count > 0)
        {
            return true;
        }

        if (analysis.HasTargetArrow)
        {
            return true;
        }

        if (CombatRuntimeStateSupport.RequiresExplicitTargetingBeforeEnemyClick(observer, combatCardKnowledge))
        {
            return false;
        }

        if (pendingSelection?.Kind == AutoCombatCardKind.AttackLike)
        {
            var pendingCard = observer.CombatHand.FirstOrDefault(card => card.SlotIndex == pendingSelection.SlotIndex);
            if (pendingCard is not null)
            {
                return IsAttackCombatHandCard(pendingCard)
                       && IsPlayableAtCurrentEnergy(pendingCard, observer.PlayerEnergy, combatCardKnowledge);
            }

            var pendingKnowledge = combatCardKnowledge.FirstOrDefault(card => card.SlotIndex == pendingSelection.SlotIndex);
            if (pendingKnowledge is not null)
            {
                return IsEnemyTargetCombatCard(pendingKnowledge)
                       && IsPlayableAtCurrentEnergy(pendingKnowledge, observer.PlayerEnergy);
            }
        }

        return analysis.HasSelectedCard
               && analysis.SelectedCardKind == AutoCombatCardKind.AttackLike
               && (observer.CombatHand.Any(card =>
                       card.SlotIndex is >= 1 and <= 5
                       && IsAttackCombatHandCard(card)
                       && IsPlayableAtCurrentEnergy(card, observer.PlayerEnergy, combatCardKnowledge))
                   || (observer.CombatHand.Count == 0 && combatCardKnowledge.Count == 0));
    }

    public static CombatNoOpLoopAnalysis PeekCombatNoOpLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return AnalyzeCombatNoOpLoop(history);
    }

    public static PendingCombatSelection? TryPeekPendingCombatSelection(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return CombatHistorySupport.TryGetPendingCombatSelection(history);
    }

    public static bool IsCombatNoOpSensitiveTarget(string? targetLabel)
    {
        return targetLabel is not null
               && (targetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(targetLabel, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase));
    }

    public static string? ResolveCombatLaneLabel(string? targetLabel, IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return CombatHistorySupport.ResolveCombatLaneLabel(targetLabel, history);
    }

    private static int? ExtractCombatLaneSlotIndex(string? targetLabel)
    {
        return CombatHistorySupport.ExtractCombatLaneSlotIndex(targetLabel);
    }

    public static IReadOnlyDictionary<int, int> GetCombatNoOpCountsBySlot(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return CombatHistorySupport.GetCombatNoOpCountsBySlot(history);
    }

    private static int GetCombatNoOpCountForSlot(IReadOnlyList<GuiSmokeHistoryEntry> history, int slotIndex)
    {
        return HandleCombatContextSupport.GetCombatNoOpCountForSlot(HandleCombatContextSupport.Reconstruct(history), slotIndex);
    }

    private static bool HasRecentRepeatedNonEnemyLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return HandleCombatContextSupport.Reconstruct(history).RepeatedNonEnemyLoop;
    }

    private static bool HasRecentRepeatedAttackSelectionLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return HandleCombatContextSupport.Reconstruct(history).RepeatedAttackSelectionLoop;
    }

    private static CombatNoOpLoopAnalysis AnalyzeCombatNoOpLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return HandleCombatContextSupport.Reconstruct(history).CombatNoOpLoop;
    }

    private static bool IsNonEnemySelectionLabel(string? targetLabel)
    {
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return false;
        }

        return targetLabel.StartsWith("combat select non-enemy slot ", StringComparison.OrdinalIgnoreCase)
               || targetLabel.StartsWith("combat select defend slot ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParsePendingCombatSelection(string targetLabel, out PendingCombatSelection? selection)
    {
        return CombatHistorySupport.TryParsePendingCombatSelection(targetLabel, out selection);
    }

    private static int? ExtractFirstDigit(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        foreach (var character in value)
        {
            if (char.IsDigit(character))
            {
                return character - '0';
            }
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryFindActionNodeDecision(GuiSmokeStepRequest request, string contains, string targetLabel)
    {
        var node = request.Observer.ActionNodes.FirstOrDefault(candidate =>
            candidate.Actionable &&
            candidate.Label.Contains(contains, StringComparison.OrdinalIgnoreCase));
        return node is null ? null : CreateClickDecisionFromNode(request, node, targetLabel);
    }

    private static GuiSmokeStepDecision? TryFindFirstReachableMapNodeDecision(GuiSmokeStepRequest request)
    {
        if (HasExplicitRestSiteChoiceAuthority(request))
        {
            return null;
        }

        var analysis = AutoMapAnalyzer.Analyze(request.ScreenshotPath);
        if (analysis.HasReachableNode)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                analysis.ReachableNodeNormalizedX,
                analysis.ReachableNodeNormalizedY,
                "visible reachable node",
                "The screenshot shows the current map arrow and a connected reachable node. Click the reachable node directly.",
                0.90,
                "map",
                1500,
                true,
                null);
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateExportedReachableMapPointDecision(GuiSmokeStepRequest request)
    {
        if (HasExplicitRestSiteChoiceAuthority(request))
        {
            return null;
        }

        var choice = request.Observer.Choices
            .Where(choice =>
                string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
                && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds))
            .OrderBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (choice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                choice,
                "exported reachable map node",
                $"Use the exported reachable map point '{choice.Label}' instead of clicking the red current-node arrow.",
                0.95,
                "map",
                1400);
        }

        var node = request.Observer.ActionNodes
            .Where(node =>
                node.Actionable
                && string.Equals(node.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
                && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds))
            .OrderBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        return node is null ? null : CreateClickDecisionFromNode(request, node, "exported reachable map node");
    }

    private static GuiSmokeStepDecision? TryCreateMapBackNavigationDecision(GuiSmokeStepRequest request)
    {
        if (HasExplicitRestSiteChoiceAuthority(request))
        {
            return null;
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (!mapOverlayState.ForegroundVisible || !mapOverlayState.MapBackNavigationAvailable)
        {
            return null;
        }

        var backNode = request.Observer.ActionNodes.FirstOrDefault(node => node.Actionable && IsBackNode(node) && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds));
        if (backNode is not null)
        {
            return CreateClickDecisionFromNode(request, backNode, "map back");
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            0.045,
            0.905,
            "map back",
            "Map overlay foreground is visible and the bottom-left red back arrow can return to the underlying event context.",
            0.86,
            "map",
            1200,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateMapOverlayForegroundDecision(GuiSmokeStepRequest request)
    {
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (!mapOverlayState.ForegroundVisible)
        {
            return null;
        }

        return TryCreateExportedReachableMapPointDecision(request)
               ?? TryCreateMapBackNavigationDecision(request)
               ?? TryFindFirstReachableMapNodeDecision(request);
    }

    private static string GetCombatSlotHotkey(int slotIndex)
    {
        return slotIndex == 10
            ? "0"
            : slotIndex.ToString(CultureInfo.InvariantCulture);
    }

    private static ObserverActionNode? FindEndTurnActionNode(GuiSmokeStepRequest request)
    {
        return FindEndTurnActionNode(request.Observer, request.WindowBounds);
    }

    private static ObserverActionNode? FindEndTurnActionNode(ObserverSummary observer, WindowBounds? windowBounds = null)
    {
        return observer.ActionNodes.FirstOrDefault(node =>
            node.Actionable
            && (string.Equals(node.Label, "1턴 종료", StringComparison.OrdinalIgnoreCase)
                || node.Label.Contains("턴 종료", StringComparison.OrdinalIgnoreCase)
                || node.Label.Contains("End Turn", StringComparison.OrdinalIgnoreCase))
            && (windowBounds is null
                ? TryParseNodeBounds(node.ScreenBounds, out _)
                : HasActiveNodeBounds(node.ScreenBounds, windowBounds)));
    }

    private static IReadOnlyList<ObserverActionNode> GetCombatEnemyTargetNodes(ObserverSummary observer, WindowBounds? windowBounds = null)
    {
        return observer.ActionNodes
            .Where(static node => node.Actionable && IsCombatEnemyTargetNode(node))
            .Where(node => windowBounds is null
                ? TryParseNodeBounds(node.ScreenBounds, out _)
                : HasActiveNodeBounds(node.ScreenBounds, windowBounds))
            .OrderBy(static node => GetNodeSortX(node))
            .ThenBy(static node => GetNodeSortY(node))
            .ToArray();
    }

    private static bool IsCombatEnemyTargetNode(ObserverActionNode node)
    {
        return string.Equals(node.Kind, "enemy-target", StringComparison.OrdinalIgnoreCase)
               || node.NodeId.StartsWith("enemy-target:", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("enemy", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("적", StringComparison.OrdinalIgnoreCase);
    }

    private static GuiSmokeStepDecision? TryCreateSemanticEventDecision(GuiSmokeStepRequest request)
    {
        if (!string.Equals(request.ReasoningMode, "semantic", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var preferredLabels = request.EventKnowledgeCandidates
            .SelectMany(static candidate => candidate.Options)
            .OrderByDescending(static option => ScoreSemanticEventOption(option.Label, option.Description))
            .Select(static option => option.Label)
            .ToArray();
        foreach (var label in preferredLabels)
        {
            var choice = request.Observer.Choices.FirstOrDefault(candidate =>
                string.Equals(candidate.Label, label, StringComparison.OrdinalIgnoreCase)
                && TryParseNodeBounds(candidate.ScreenBounds, out _));
            if (choice is null || !TryParseNodeBounds(choice.ScreenBounds, out var bounds))
            {
                continue;
            }

            var centerX = bounds.X + bounds.Width / 2f;
            var centerY = bounds.Y + bounds.Height / 2f;
            var leadingCandidate = request.EventKnowledgeCandidates.FirstOrDefault(candidate =>
                candidate.Options.Any(option => string.Equals(option.Label, label, StringComparison.OrdinalIgnoreCase)));
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                Math.Clamp(centerX / 1920f, 0d, 1d),
                Math.Clamp(centerY / 1080f, 0d, 1d),
                $"semantic event option: {label}",
                $"Semantic event reasoning selected '{label}' from {leadingCandidate?.Title ?? "event knowledge"}.",
                0.86,
                "event",
                1400,
                true,
                null,
                leadingCandidate is null ? "semantic event match" : $"event candidate: {leadingCandidate.Title}",
                "event option changes page, grants reward, or advances room flow",
                request.DecisionRiskHint);
        }

        return null;
    }

    private static int ScoreSemanticEventOption(string? label, string? description)
    {
        var score = 0;
        if (ContainsAny(label, "떠", "leave", "continue", "확인", "proceed", "take"))
        {
            score += 5;
        }

        if (ContainsAny(description, "획득", "gain", "얻", "heal", "회복", "gold", "카드"))
        {
            score += 3;
        }

        if (ContainsAny(description, "잃", "lose", "피해", "damage", "hp", "wound", "부상"))
        {
            score -= 4;
        }

        return score;
    }

    private static bool ContainsAny(string? value, params string[] candidates)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return candidates.Any(candidate => value.Contains(candidate, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsAttackCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("STRIKE", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("BASH", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEnemyTargetCombatCard(CombatCardKnowledgeHint card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Skill", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Type, "Power", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("DEFEND", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInertCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Status", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Type, "Curse", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyCombatCard(CombatCardKnowledgeHint card)
    {
        if (string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
            || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Equals(card.Type, "Power", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "Self", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "None", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllAllies", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyAlly", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlayableAtCurrentEnergy(ObservedCombatHandCard card, int? energy)
    {
        if (IsInertCombatHandCard(card) && card.Cost is null)
        {
            return false;
        }

        if (energy is null || card.Cost is null)
        {
            return true;
        }

        if (card.Cost < 0)
        {
            return energy > 0;
        }

        return card.Cost <= energy;
    }

    private static bool IsPlayableAtCurrentEnergy(
        ObservedCombatHandCard card,
        int? energy,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var resolvedCost = ResolveCombatCardCost(card, combatCardKnowledge);
        return IsPlayableAtCurrentEnergy(card with { Cost = resolvedCost }, energy);
    }

    private static bool IsPlayableAtCurrentEnergy(CombatCardKnowledgeHint card, int? energy)
    {
        if (energy is null || card.Cost is null)
        {
            return true;
        }

        if (card.Cost < 0)
        {
            return energy > 0;
        }

        return card.Cost <= energy;
    }

    private static int? ResolveCombatCardCost(ObservedCombatHandCard card, IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        if (card.Cost is not null)
        {
            return card.Cost;
        }

        var slotMatch = combatCardKnowledge.FirstOrDefault(candidate => candidate.SlotIndex == card.SlotIndex);
        if (slotMatch?.Cost is not null)
        {
            return slotMatch.Cost;
        }

        var cardKeys = BuildCombatKnowledgeLookupKeysForCombat(card.Name);
        if (cardKeys.Count == 0)
        {
            return null;
        }

        return combatCardKnowledge
            .Where(candidate => candidate.Cost is not null)
            .Where(candidate =>
            {
                var candidateKeys = BuildCombatKnowledgeLookupKeysForCombat(candidate.Name);
                return candidateKeys.Any(cardKeys.Contains);
            })
            .Select(static candidate => candidate.Cost)
            .FirstOrDefault();
    }

    private static IReadOnlyList<string> BuildCombatKnowledgeLookupKeysForCombat(string? cardName)
    {
        if (string.IsNullOrWhiteSpace(cardName))
        {
            return Array.Empty<string>();
        }

        var keys = new List<string>();
        var normalizedName = NormalizeCombatLookupKey(cardName);
        AddCombatLookupKey(keys, normalizedName);

        var parts = cardName
            .Split(new[] { '.', '_', '-', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeCombatLookupKey)
            .Where(static part => part.Length > 0)
            .ToArray();
        if (parts.Length == 0)
        {
            return keys;
        }

        var trimmedParts = parts[0] == "card"
            ? parts[1..]
            : parts;
        if (trimmedParts.Length == 0)
        {
            return keys;
        }

        AddCombatLookupKey(keys, string.Concat(trimmedParts));
        AddCombatLookupKey(keys, trimmedParts[0]);
        if (trimmedParts.Length > 1 && IsCombatLookupClassSuffix(trimmedParts[^1]))
        {
            AddCombatLookupKey(keys, string.Concat(trimmedParts[..^1]));
        }

        foreach (var part in trimmedParts)
        {
            AddCombatLookupKey(keys, part);
        }

        return keys;
    }

    private static void AddCombatLookupKey(List<string> keys, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate)
            || keys.Contains(candidate, StringComparer.Ordinal))
        {
            return;
        }

        keys.Add(candidate);
    }

    private static bool IsCombatLookupClassSuffix(string value)
    {
        return value is "ironclad" or "silent" or "defect" or "watcher" or "colorless" or "status" or "curse";
    }

    private static string NormalizeCombatLookupKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var buffer = new char[value.Length];
        var length = 0;
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                buffer[length] = char.ToLowerInvariant(character);
                length += 1;
            }
        }

        return length == 0
            ? string.Empty
            : new string(buffer, 0, length);
    }

    private static GuiSmokeStepDecision? TryCreateScreenshotFirstRoomDecision(GuiSmokeStepRequest request)
    {
        var rewardMapLayer = BuildRewardMapLayerState(request.Observer, request.WindowBounds);
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (!rewardMapLayer.RewardPanelVisible
            && !GuiSmokeForegroundHeuristics.ShouldPreferEventProgressionOverMapFallback(request.Observer)
            && !mapOverlayState.ForegroundVisible)
        {
            var visibleMapNodeDecision = GuiSmokeDecisionDebug.TraceCandidate(
                "visible reachable node",
                "screenshot-reachable-node",
                0.90,
                TryFindFirstReachableMapNodeDecision(request),
                "no screenshot-reachable map node was detected");
            if (visibleMapNodeDecision is not null)
            {
                return visibleMapNodeDecision;
            }

            var visibleMapAdvanceDecision = GuiSmokeDecisionDebug.TraceCandidate(
                "visible map advance",
                "screenshot-current-arrow",
                0.78,
                TryFindVisibleMapAdvanceDecision(request),
                "no current-node-arrow fallback was permitted");
            if (visibleMapAdvanceDecision is not null)
            {
                return visibleMapAdvanceDecision;
            }
        }

        if (LooksLikeRestSiteState(request.Observer))
        {
            GuiSmokeDecisionDebug.SetSceneModel("rest-site", "map-context");
        }

        var restSiteUpgradeDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "rest site upgrade",
            "rest-site-upgrade",
            0.91,
            TryCreateRestSiteUpgradeDecision(request),
            "rest-site upgrade grid is not currently actionable");
        if (restSiteUpgradeDecision is not null)
        {
            return restSiteUpgradeDecision;
        }

        var treasureDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "treasure room",
            "treasure-room",
            0.90,
            TryCreateTreasureChestDecision(request),
            "treasure room affordance is not visible");
        if (treasureDecision is not null)
        {
            return treasureDecision;
        }

        if (LooksLikeShopState(request.Observer))
        {
            var shopDecision = DecideHandleShop(request);
            return string.Equals(shopDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                ? null
                : shopDecision;
        }

        return GuiSmokeDecisionDebug.TraceCandidate(
                   "rest site explicit choice",
                   "rest-site-choice",
                   0.89,
                   TryCreateRestSiteDecision(request),
                   "rest-site explicit choices are not visible")
               ?? GuiSmokeDecisionDebug.TraceCandidate(
                   "hidden overlay cleanup",
                   "overlay-cleanup",
                   0.70,
                   TryCreateHiddenOverlayCleanupDecision(request),
                   "no hidden overlay cleanup affordance is available")
               ?? GuiSmokeDecisionDebug.TraceCandidate(
                   "overlay advance",
                   "overlay-advance",
                   0.68,
                   TryCreateOverlayAdvanceDecision(request),
                   "no overlay advance affordance is available")
               ?? GuiSmokeDecisionDebug.TraceCandidate(
                   "visible proceed",
                   "visible-proceed",
                   0.67,
                   TryCreateVisibleProceedDecision(request),
                   "no visible proceed button is available");
    }

    private static GuiSmokeStepDecision? TryCreateTreasureChestDecision(GuiSmokeStepRequest request)
    {
        if (!LooksLikeTreasureState(request.Observer) || HasOverlayChoiceState(request.Observer))
        {
            return null;
        }

        var treasureAnalysis = AutoTreasureAnalyzer.Analyze(request.ScreenshotPath);
        if (treasureAnalysis.HasClosedChestHighlight)
        {
            return CreateTreasureChestCenterDecision(request);
        }

        if (treasureAnalysis.HasRewardIcon)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                treasureAnalysis.RewardIconNormalizedX,
                treasureAnalysis.RewardIconNormalizedY,
                "treasure reward icon",
                "Treasure chest is already open in the screenshot. Click the floating reward icon instead of the chest body.",
                0.98,
                "map",
                1500,
                true,
                null);
        }

        var visibleProceedDecision = TryCreateVisibleProceedDecision(request);
        if (visibleProceedDecision is not null)
        {
            return visibleProceedDecision;
        }

        var hasTreasureInteractionHistory = request.History.Any(entry =>
            string.Equals(entry.TargetLabel, "treasure chest center", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entry.TargetLabel, "treasure reward icon", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entry.TargetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase));
        if (hasTreasureInteractionHistory)
        {
            return null;
        }

        return CreateTreasureChestCenterDecision(request);
    }

    private static GuiSmokeStepDecision CreateTreasureChestCenterDecision(GuiSmokeStepRequest request)
    {
        var chestChoice = request.Observer.Choices.FirstOrDefault(choice =>
            choice.Label.Contains("Chest", StringComparison.OrdinalIgnoreCase)
            || choice.Label.Contains("\uC0C1\uC790", StringComparison.OrdinalIgnoreCase));
        var normalizedX = 0.500d;
        var normalizedY = 0.560d;
        if (chestChoice is not null
            && TryParseNodeBounds(chestChoice.ScreenBounds, out var chestBounds)
            && HasUsableLogicalBounds(chestChoice.ScreenBounds))
        {
            var centerX = chestBounds.X + chestBounds.Width / 2f;
            var centerY = chestBounds.Y + chestBounds.Height / 2f;
            normalizedX = Math.Clamp(centerX / 1920f, 0.35d, 0.65d);
            normalizedY = Math.Clamp(centerY / 1080f, 0.35d, 0.70d);
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            "treasure chest center",
            "Treasure chest room detected. Open the center chest before trying map routing or proceed actions.",
            0.96,
            "map",
            1500,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateRestSiteUpgradeDecision(GuiSmokeStepRequest request)
    {
        if (!LooksLikeRestSiteState(request.Observer))
        {
            return null;
        }

        var observerConfirmChoice = RestSiteObserverSignals.TryGetSmithConfirmChoice(request.Observer);
        if (observerConfirmChoice is not null
            && observerConfirmChoice.Enabled != false
            && !string.IsNullOrWhiteSpace(observerConfirmChoice.ScreenBounds)
            && HasActiveNodeBounds(observerConfirmChoice.ScreenBounds, request.WindowBounds))
        {
            return CreateClickDecisionFromChoice(
                request,
                observerConfirmChoice,
                "rest site: smith confirm",
                "Rest site smith confirm is runtime-visible. Confirm the selected upgrade instead of repeating the smith option click.",
                0.95,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "upgrade",
                1400);
        }

        var observerSmithCardChoice = RestSiteObserverSignals.TryGetFirstSmithCardChoice(request.Observer);
        if (observerSmithCardChoice is not null
            && observerSmithCardChoice.Enabled != false
            && !string.IsNullOrWhiteSpace(observerSmithCardChoice.ScreenBounds)
            && HasActiveNodeBounds(observerSmithCardChoice.ScreenBounds, request.WindowBounds))
        {
            return CreateClickDecisionFromChoice(
                request,
                observerSmithCardChoice,
                "rest site: smith card",
                "Rest site smith grid is runtime-visible. Select an exported upgrade card hitbox instead of relying on screenshot-only inference.",
                0.94,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "upgrade",
                1400);
        }

        if (RestSiteObserverSignals.HasExportedSmithUpgradeChoices(request.Observer))
        {
            return null;
        }

        var lastUpgradeAction = request.History
            .Where(entry =>
                string.Equals(entry.TargetLabel, "rest site: smith card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.TargetLabel, "rest site: smith confirm", StringComparison.OrdinalIgnoreCase))
            .LastOrDefault();
        if (lastUpgradeAction is not null
            && string.Equals(lastUpgradeAction.TargetLabel, "rest site: smith card", StringComparison.OrdinalIgnoreCase))
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.949,
                0.716,
                "rest site: smith confirm",
                "Rest site upgrade comparison is visible. Click the right-side confirm button after selecting the card.",
                0.94,
                "map",
                1400,
                true,
                null);
        }

        var analysis = AutoRestSiteCardGridAnalyzer.Analyze(request.ScreenshotPath);
        if (!analysis.HasSelectableCard)
        {
            return null;
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            analysis.CardNormalizedX,
            analysis.CardNormalizedY,
            "rest site: smith card",
            "Rest site upgrade card grid is visible. Select a visible card before confirming with V.",
            0.91,
            "map",
            1400,
            true,
            null);
    }

    private static string? TryFindRestSiteUpgradeBounds(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        return decision.TargetLabel switch
        {
            { } label when label.Contains("smith confirm", StringComparison.OrdinalIgnoreCase)
                => RestSiteObserverSignals.TryGetSmithConfirmChoice(request.Observer)?.ScreenBounds,
            { } label when label.Contains("smith card", StringComparison.OrdinalIgnoreCase)
                => RestSiteObserverSignals.TryGetFirstSmithCardChoice(request.Observer)?.ScreenBounds,
            _ => null,
        };
    }

    private static string? TryResolveRestSiteUpgradeBoundsSource(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        return decision.TargetLabel switch
        {
            { } label when label.Contains("smith confirm", StringComparison.OrdinalIgnoreCase)
                && RestSiteObserverSignals.TryGetSmithConfirmChoice(request.Observer) is not null => "observer-rest-site-smith-confirm",
            { } label when label.Contains("smith card", StringComparison.OrdinalIgnoreCase)
                && RestSiteObserverSignals.TryGetFirstSmithCardChoice(request.Observer) is not null => "observer-rest-site-smith-card",
            _ => "screenshot-rest-site-upgrade",
        };
    }

    private static GuiSmokeStepDecision? TryFindVisibleMapAdvanceDecision(GuiSmokeStepRequest request)
    {
        if (HasExplicitRestSiteChoiceAuthority(request))
        {
            return null;
        }

        if (BuildRewardMapLayerState(request.Observer, request.WindowBounds).RewardPanelVisible)
        {
            return null;
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (mapOverlayState.ForegroundVisible || mapOverlayState.ExportedReachableNodeCandidatePresent)
        {
            return null;
        }

        var attempt = request.History.Count(entry =>
            string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.TargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase));

        var analysis = AutoMapAnalyzer.Analyze(request.ScreenshotPath);
        if (!analysis.HasCurrentArrow)
        {
            return null;
        }

        var offset = attempt switch
        {
            0 => new PointF(0f, -0.105f),
            1 => new PointF(-0.060f, -0.110f),
            2 => new PointF(0.060f, -0.110f),
            _ => new PointF(0f, -0.135f),
        };

        var normalizedX = Math.Clamp(analysis.ArrowNormalizedX + offset.X, 0.08f, 0.92f);
        var normalizedY = Math.Clamp(analysis.ArrowNormalizedY + offset.Y, 0.10f, 0.86f);
        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            "visible map advance",
            $"Visible map is present while logical flow remains '{request.Observer.CurrentScreen}'. Advance from the red current-node arrow using screenshot-derived positioning (attempt {attempt + 1}).",
            0.78,
            "map",
            1500,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateRestSiteDecision(GuiSmokeStepRequest request)
    {
        return BuildRestSiteDecisionCandidates(request)
            .Select(static candidate => candidate.Decision)
            .FirstOrDefault(static decision => decision is not null);
    }

    private static GuiSmokeStepDecision? TryCreateVisibleProceedDecision(GuiSmokeStepRequest request)
    {
        var proceedNode = request.Observer.ActionNodes.FirstOrDefault(node =>
            node.Actionable
            && IsProceedNode(node)
            && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds));
        if (proceedNode is not null)
        {
            return CreateClickDecisionFromNode(request, proceedNode, "visible proceed");
        }

        var proceedChoice = request.Observer.Choices.FirstOrDefault(choice =>
            !string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
            && IsProceedLikeLabel(choice.Label)
            && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds));
        if (proceedChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                proceedChoice,
                "visible proceed",
                "Observer-exported proceed choice is visible. Advance the room flow before attempting any map click.",
                0.96,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                1400);
        }

        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(request.ScreenshotPath);
        if (!overlayAnalysis.HasRightProceedArrow || overlayAnalysis.HasBottomLeftBackArrow || overlayAnalysis.HasCentralOverlayPanel)
        {
            return null;
        }

        if (overlayAnalysis.RightProceedNormalizedX is not null && overlayAnalysis.RightProceedNormalizedY is not null)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                overlayAnalysis.RightProceedNormalizedX,
                overlayAnalysis.RightProceedNormalizedY,
                "visible proceed",
                "The screenshot shows a right-side proceed arrow cluster without an active overlay back arrow. Advance the room flow before attempting any map click.",
                0.95,
                "map",
                1400,
                true,
                null);
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            0.885,
            0.835,
            "visible proceed",
            "The screenshot shows the large right-side proceed arrow without an active overlay back arrow. Advance the room flow before attempting any map click.",
            0.95,
            "map",
            1400,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateHiddenOverlayCleanupDecision(GuiSmokeStepRequest request)
    {
        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(request.ScreenshotPath);
        if (overlayAnalysis.HasCentralOverlayPanel)
        {
            return null;
        }

        var choices = request.Observer.CurrentChoices;
        var hasOverlayChoices = choices.Any(static label =>
            label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
            || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
            || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
        if (!hasOverlayChoices)
        {
            return null;
        }

        var cleanupAttempts = request.History.Count(entry =>
            string.Equals(entry.TargetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase));
        if (cleanupAttempts >= 2)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.180,
                0.520,
                "hidden overlay close",
                "Overlay controls remain active in the room state even though no central panel is visible. Retry backdrop dismissal before progressing.",
                0.72,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1200,
                true,
                null);
        }

        return new GuiSmokeStepDecision(
            "act",
            "press-key",
            "Escape",
            null,
            null,
            "hidden overlay close",
            "Overlay controls remain active behind the visible room. Send escape/cancel before trying to progress.",
            0.84,
            request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
            1000,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateOverlayAdvanceDecision(GuiSmokeStepRequest request)
    {
        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(request.ScreenshotPath);
        if (!overlayAnalysis.HasCentralOverlayPanel)
        {
            return null;
        }

        var choices = request.Observer.CurrentChoices;
        var hasBackstop = choices.Any(static label => label.Contains("Backstop", StringComparison.OrdinalIgnoreCase));
        var hasLeftArrow = choices.Any(static label => label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase));
        var hasRightArrow = choices.Any(static label => label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
        if (!hasBackstop && !hasLeftArrow && !hasRightArrow)
        {
            return null;
        }

        var recentOverlayBackCount = CountRecentOverlayCleanupAttempts(request.History);

        if (recentOverlayBackCount >= 3)
        {
            return new GuiSmokeStepDecision(
                "act",
                "press-key",
                "Escape",
                null,
                null,
                "overlay close",
                "Overlay remains open after repeated dismiss attempts. Send escape before trying to progress again.",
                0.72,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1200,
                true,
                null);
        }

        if (overlayAnalysis.HasBottomLeftBackArrow)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.045,
                0.905,
                "overlay back",
                "An inspect overlay is present above the room flow. Close it via the visible back arrow before trying to progress.",
                0.93,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1200,
                true,
                null);
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            0.180,
            0.520,
            "overlay backdrop close",
            "A centered inspect overlay is visible without a dedicated back arrow. Click the dark backdrop to dismiss it before trying to progress.",
            0.88,
            request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
            1200,
            true,
            null);
    }

    private static bool IsProceedNode(ObserverActionNode node)
    {
        return node.Label.Contains("\uC9C4\uD589", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("\uB118\uAE30\uAE30", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Proceed", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Continue", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBackNode(ObserverActionNode node)
    {
        return node.Label.Contains("\uB4A4\uB85C", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Back", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMapNode(ObserverActionNode node)
    {
        return node.Kind.Contains("map", StringComparison.OrdinalIgnoreCase)
               || node.NodeId.Contains("map", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Map", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeTreasureState(ObserverSummary observer)
    {
        if (string.Equals(observer.EncounterKind, "Treasure", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return observer.CurrentChoices.Any(static label =>
            label.Contains("Chest", StringComparison.OrdinalIgnoreCase)
            || label.Contains("\uC0C1\uC790", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeRestSiteState(ObserverSummary observer)
    {
        if (string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase)
            || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer))
        {
            return true;
        }

        if (observer.Choices.Any(static choice =>
                !string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
                && (choice.Label.Contains("\uD734\uC2DD", StringComparison.OrdinalIgnoreCase)
                    || choice.Label.Contains("Rest", StringComparison.OrdinalIgnoreCase)
                    || choice.Label.Contains("\uC7AC\uB828", StringComparison.OrdinalIgnoreCase)
                    || choice.Label.Contains("Smith", StringComparison.OrdinalIgnoreCase))))
        {
            return true;
        }

        return observer.Choices.Count == 0
               && observer.CurrentChoices.Any(static label =>
                   label.Contains("\uD734\uC2DD", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Rest", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("\uC7AC\uB828", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Smith", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeScreenshotFirstRoomState(GuiSmokeStepRequest request)
    {
        return LooksLikeRestSiteState(request.Observer)
               || LooksLikeTreasureState(request.Observer)
               || LooksLikeShopState(request.Observer)
               || (string.Equals(request.Observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
                   && request.Observer.Choices.Any(choice =>
                       string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)));
    }

    private static bool LooksLikeMapTransitionState(GuiSmokeStepRequest request)
    {
        if (HasExplicitRestSiteChoiceAuthority(request))
        {
            return false;
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        return mapOverlayState.ForegroundVisible
               || string.Equals(request.Observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
               || string.Equals(request.Observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
               || request.SceneSignature.Contains("substate:map-transition", StringComparison.OrdinalIgnoreCase)
               || request.SceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase)
               || request.SceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)
               || request.Observer.LastEventsTail.Any(eventTail =>
                   eventTail.Contains("screen-changed: map", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("map-point-selected", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("\"screen\":\"map\"", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("\"currentScreen\":\"map\"", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsBackChoiceLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        return label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
               || label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
               || label.Contains("\uB4A4\uB85C", StringComparison.OrdinalIgnoreCase)
               || label.Contains("Back", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasOverlayChoiceState(ObserverSummary observer)
    {
        return observer.CurrentChoices.Any(static label =>
            label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
            || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
            || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
    }

    private static RewardMapLayerState BuildRewardMapLayerState(ObserverSummary observer, WindowBounds? windowBounds)
    {
        var mapContextVisible = GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer)
                                || string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase);
        var rewardBackNavigationAvailable = HasOverlayChoiceState(observer)
                                            || observer.ActionNodes.Any(static node => node.Actionable && IsBackNode(node));
        var activeRewardChoices = observer.Choices.Where(choice => IsCurrentRewardProgressionChoice(choice, windowBounds)).ToArray();
        var activeRewardNodes = observer.ActionNodes.Where(node => IsCurrentRewardProgressionNode(node, windowBounds)).ToArray();
        var staleRewardChoices = observer.Choices.Where(choice => IsStaleRewardProgressionChoice(choice, windowBounds)).ToArray();
        var staleRewardNodes = observer.ActionNodes.Where(node => IsStaleRewardProgressionNode(node, windowBounds)).ToArray();
        var explicitRewardChoicesPresent = activeRewardChoices.Length > 0 || activeRewardNodes.Length > 0;
        var rewardScreenHint = HasStrongRewardScreenAuthority(observer)
                               || string.Equals(observer.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(observer.VisibleScreen, "rewards", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase)
                               || LooksLikeRewardChoiceState(observer)
                               || LooksLikeColorlessCardChoiceState(observer);
        var rewardPanelVisible = (rewardScreenHint && explicitRewardChoicesPresent)
                                 || (rewardScreenHint && !mapContextVisible)
                                 || (string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
                                     && rewardBackNavigationAvailable
                                     && explicitRewardChoicesPresent);
        var staleRewardChoicePresent = !rewardPanelVisible && (staleRewardChoices.Length > 0 || staleRewardNodes.Length > 0);
        var offWindowBoundsReused = staleRewardChoices.Any(choice => IsOffWindowBounds(choice.ScreenBounds, windowBounds))
                                  || staleRewardNodes.Any(node => IsOffWindowBounds(node.ScreenBounds, windowBounds));
        var staleRewardBoundsPresent = staleRewardChoicePresent && (staleRewardChoices.Length > 0 || staleRewardNodes.Length > 0);
        return new RewardMapLayerState(
            rewardPanelVisible,
            mapContextVisible,
            rewardBackNavigationAvailable,
            staleRewardChoicePresent,
            staleRewardBoundsPresent,
            offWindowBoundsReused);
    }

    private static bool LooksLikeInspectOverlayState(ObserverSummary observer)
    {
        return HasOverlayChoiceState(observer)
               || observer.ActionNodes.Any(static node => IsOverlayChoiceLabel(node.Label));
    }

    private static bool HasRewardChoiceAuthority(ObserverSummary observer)
    {
        return HasStrongRewardScreenAuthority(observer)
               || string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.VisibleScreen, "event", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasStrongRewardScreenAuthority(ObserverSummary observer)
    {
        return GuiSmokeObserverPhaseHeuristics.LooksLikeRewardsState(observer);
    }

    private static bool ShouldSuppressRoomSubstateHeuristics(GuiSmokePhase phase, ObserverSummary observer)
    {
        return phase == GuiSmokePhase.HandleCombat
               || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer);
    }

    private static bool ShouldPreferRewardProgressionOverMapFallback(ObserverSummary observer)
    {
        var rewardMapLayer = BuildRewardMapLayerState(observer, null);
        return rewardMapLayer.RewardPanelVisible
               && HasExplicitRewardProgressionAffordance(observer);
    }

    private static bool HasExplicitRewardProgressionAffordance(ObserverSummary observer)
    {
        return observer.Choices.Any(choice => IsCurrentRewardProgressionChoice(choice, null))
               || observer.ActionNodes.Any(node => IsCurrentRewardProgressionNode(node, null));
    }

    private static bool IsCurrentRewardProgressionChoice(ObserverChoice choice, WindowBounds? windowBounds)
    {
        return IsExplicitRewardProgressionChoice(choice)
               && HasActiveRewardBounds(choice.ScreenBounds, windowBounds);
    }

    private static bool IsCurrentRewardProgressionNode(ObserverActionNode node, WindowBounds? windowBounds)
    {
        return IsExplicitRewardProgressionNode(node)
               && HasActiveRewardBounds(node.ScreenBounds, windowBounds);
    }

    private static bool IsStaleRewardProgressionChoice(ObserverChoice choice, WindowBounds? windowBounds)
    {
        return IsExplicitRewardProgressionChoice(choice)
               && !HasActiveRewardBounds(choice.ScreenBounds, windowBounds);
    }

    private static bool IsStaleRewardProgressionNode(ObserverActionNode node, WindowBounds? windowBounds)
    {
        return IsExplicitRewardProgressionNode(node)
               && !HasActiveRewardBounds(node.ScreenBounds, windowBounds);
    }

    private static bool HasActiveRewardBounds(string? screenBounds, WindowBounds? windowBounds)
    {
        if (HasUsableLogicalBounds(screenBounds))
        {
            return true;
        }

        return windowBounds is not null && IsBoundsInsideWindow(screenBounds, windowBounds);
    }

    private static bool HasActiveNodeBounds(string? screenBounds, WindowBounds? windowBounds)
    {
        if (HasUsableLogicalBounds(screenBounds))
        {
            return true;
        }

        return windowBounds is not null && IsBoundsInsideWindow(screenBounds, windowBounds);
    }

    private static bool IsOffWindowBounds(string? screenBounds, WindowBounds? windowBounds)
    {
        if (string.IsNullOrWhiteSpace(screenBounds) || windowBounds is null || !TryParseNodeBounds(screenBounds, out _))
        {
            return false;
        }

        return !HasUsableLogicalBounds(screenBounds)
               && !IsBoundsInsideWindow(screenBounds, windowBounds);
    }

    private static bool IsBoundsInsideWindow(string? screenBounds, WindowBounds windowBounds)
    {
        if (!TryParseNodeBounds(screenBounds, out var bounds))
        {
            return false;
        }

        return bounds.Right > windowBounds.X
               && bounds.Bottom > windowBounds.Y
               && bounds.X < windowBounds.X + windowBounds.Width
               && bounds.Y < windowBounds.Y + windowBounds.Height;
    }

    private static bool IsExplicitRewardProgressionChoice(ObserverChoice choice)
    {
        if (!TryParseNodeBounds(choice.ScreenBounds, out _)
            || IsOverlayChoiceLabel(choice.Label)
            || IsInspectPreviewChoice(choice)
            || IsDismissLikeLabel(choice.Label))
        {
            return false;
        }

        return IsRewardCardChoice(choice)
               || IsSkipOrProceedLabel(choice.Label)
               || choice.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || HasLargeChoiceBounds(choice.ScreenBounds);
    }

    private static bool IsExplicitRewardProgressionNode(ObserverActionNode node)
    {
        if (!node.Actionable
            || !TryParseNodeBounds(node.ScreenBounds, out _)
            || IsOverlayChoiceLabel(node.Label)
            || IsInspectPreviewBounds(node.ScreenBounds)
            || IsDismissLikeLabel(node.Label)
            || IsMapNode(node)
            || IsBackNode(node))
        {
            return false;
        }

        return IsProceedNode(node)
               || node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || HasLargeChoiceBounds(node.ScreenBounds);
    }

    private static bool LooksLikeRewardChoiceState(ObserverSummary observer)
    {
        if (GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer))
        {
            return false;
        }

        var rewardCardCount = observer.Choices.Count(IsRewardCardChoice);
        var inspectPreviewCount = observer.Choices.Count(IsInspectPreviewChoice);
        return rewardCardCount > 0
               || (HasRewardChoiceAuthority(observer) && inspectPreviewCount > 0 && observer.Choices.Any(static choice => IsSkipOrProceedLabel(choice.Label)));
    }

    private static bool LooksLikeColorlessCardChoiceState(ObserverSummary observer)
    {
        return !GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer)
               && HasRewardChoiceAuthority(observer)
               && observer.Choices.Any(IsRewardCardChoice)
               && observer.Choices.Any(IsInspectPreviewChoice);
    }

    private static bool IsRewardCardChoice(ObserverChoice choice)
    {
        return string.Equals(choice.Kind, "card", StringComparison.OrdinalIgnoreCase)
               && !IsSkipOrProceedLabel(choice.Label)
               && !IsConfirmLikeLabel(choice.Label)
               && !IsDismissLikeLabel(choice.Label)
               && !IsOverlayChoiceLabel(choice.Label)
               && HasRewardCardLikeBounds(choice.ScreenBounds);
    }

    private static bool HasRewardCardLikeBounds(string? screenBounds)
    {
        if (!TryParseNodeBounds(screenBounds, out var bounds))
        {
            return true;
        }

        return bounds.Width >= 120f || bounds.Height >= 150f;
    }

    private static bool IsInspectPreviewChoice(ObserverChoice choice)
    {
        return IsOverlayChoiceLabel(choice.Label)
               || string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || IsInspectPreviewBounds(choice.ScreenBounds);
    }

    private static int ScoreExplicitRewardProgressionChoice(ObserverChoice choice)
    {
        if (IsRewardCardChoice(choice))
        {
            return 280;
        }

        if (IsRelicRewardChoice(choice))
        {
            return 250;
        }

        if (IsGoldRewardChoice(choice))
        {
            return 180;
        }

        if (IsSkipLikeLabel(choice.Label))
        {
            return 70;
        }

        if (IsProceedLikeLabel(choice.Label) || IsConfirmLikeLabel(choice.Label))
        {
            return 110;
        }

        if (choice.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
            || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true)
        {
            return 220;
        }

        if (HasLargeChoiceBounds(choice.ScreenBounds))
        {
            return 200;
        }

        return ScoreProgressionChoice(choice);
    }

    private static int ScoreExplicitRewardProgressionNode(ObserverActionNode node)
    {
        if (IsProceedNode(node))
        {
            return 110;
        }

        if (node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
            || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase))
        {
            return 220;
        }

        if (HasLargeChoiceBounds(node.ScreenBounds))
        {
            return 200;
        }

        return ScoreProgressionNode(node);
    }

    private static bool IsOverlayChoiceLabel(string? label)
    {
        return ContainsAny(label, "Backstop", "LeftArrow", "RightArrow");
    }

    private static bool IsSkipOrProceedLabel(string? label)
    {
        return IsSkipLikeLabel(label)
               || IsProceedLikeLabel(label)
               || IsConfirmLikeLabel(label);
    }

    private static bool IsSkipLikeLabel(string? label)
    {
        return ContainsAny(label, "Skip", "건너", "넘기");
    }

    private static bool IsProceedLikeLabel(string? label)
    {
        return ContainsAny(label, "Proceed", "Continue", "진행", "계속");
    }

    private static bool IsConfirmLikeLabel(string? label)
    {
        return ContainsAny(label, "Confirm", "확인", "선택");
    }

    private static bool IsDismissLikeLabel(string? label)
    {
        return ContainsAny(label, "Cancel", "Close", "닫기", "취소", "Back");
    }

    private static bool IsInspectPreviewBounds(string? screenBounds)
    {
        return TryParseNodeBounds(screenBounds, out var bounds)
               && bounds.Width <= 120f
               && bounds.Height <= 120f
               && bounds.Y <= 170f
               && bounds.X <= 200f;
    }

    private static bool HasLargeChoiceBounds(string? screenBounds)
    {
        return TryParseNodeBounds(screenBounds, out var bounds)
               && bounds.Width >= 260f
               && bounds.Height >= 60f;
    }

    private static bool IsOverlayCleanupTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay back", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay backdrop close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "inspect overlay escape", StringComparison.OrdinalIgnoreCase);
    }

    private static int CountRecentOverlayCleanupAttempts(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var count = 0;
        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (IsOverlayCleanupTarget(entry.TargetLabel))
            {
                count += 1;
                continue;
            }

            if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            break;
        }

        return count;
    }

    private static int ScoreProgressionChoice(ObserverChoice choice)
    {
        if (IsOverlayChoiceLabel(choice.Label))
        {
            return -400;
        }

        if (IsInspectPreviewChoice(choice))
        {
            return -240;
        }

        if (IsDismissLikeLabel(choice.Label))
        {
            return -120;
        }

        if (IsRewardCardChoice(choice))
        {
            return 240;
        }

        if (IsRelicRewardChoice(choice))
        {
            return 210;
        }

        if (IsGoldRewardChoice(choice))
        {
            return 120;
        }

        if (IsSkipLikeLabel(choice.Label))
        {
            return 20;
        }

        if (IsProceedLikeLabel(choice.Label) || IsConfirmLikeLabel(choice.Label))
        {
            return 60;
        }

        if (HasLargeChoiceBounds(choice.ScreenBounds))
        {
            return 120;
        }

        return TryParseNodeBounds(choice.ScreenBounds, out _) ? 20 : 0;
    }

    private static int ScoreProgressionNode(ObserverActionNode node)
    {
        if (IsOverlayChoiceLabel(node.Label))
        {
            return -400;
        }

        if (IsInspectPreviewBounds(node.ScreenBounds))
        {
            return -240;
        }

        if (IsDismissLikeLabel(node.Label))
        {
            return -120;
        }

        if (IsSkipLikeLabel(node.Label))
        {
            return 70;
        }

        if (IsProceedLikeLabel(node.Label) || IsConfirmLikeLabel(node.Label))
        {
            return 60;
        }

        if (HasLargeChoiceBounds(node.ScreenBounds))
        {
            return 120;
        }

        if (node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
            || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
            || node.Kind.Contains("choice", StringComparison.OrdinalIgnoreCase))
        {
            return 80;
        }

        return TryParseNodeBounds(node.ScreenBounds, out _) ? 20 : 0;
    }

    private static float GetChoiceSortY(ObserverChoice choice)
    {
        return TryParseNodeBounds(choice.ScreenBounds, out var bounds) ? bounds.Y : float.MaxValue;
    }

    private static float GetChoiceSortX(ObserverChoice choice)
    {
        return TryParseNodeBounds(choice.ScreenBounds, out var bounds) ? bounds.X : float.MaxValue;
    }

    private static float GetNodeSortY(ObserverActionNode node)
    {
        return TryParseNodeBounds(node.ScreenBounds, out var bounds) ? bounds.Y : float.MaxValue;
    }

    private static float GetNodeSortX(ObserverActionNode node)
    {
        return TryParseNodeBounds(node.ScreenBounds, out var bounds) ? bounds.X : float.MaxValue;
    }

    private static string GetProgressChoiceTargetLabel(ObserverChoice choice, ObserverSummary observer)
    {
        return GetProgressChoiceTargetLabel(choice.Label, observer);
    }

    private static string GetProgressChoiceTargetLabel(string? label, ObserverSummary observer)
    {
        if (IsSkipLikeLabel(label))
        {
            return "reward skip";
        }

        if (LooksLikeColorlessCardChoiceState(observer) && observer.Choices.Any(IsRewardCardChoice))
        {
            return "colorless card choice";
        }

        if (LooksLikeRewardChoiceState(observer))
        {
            return "reward choice";
        }

        return "event progression choice";
    }

    private static bool IsGoldRewardChoice(ObserverChoice choice)
    {
        return ContainsAny(choice.Label, "골드", "gold")
               || ContainsAny(choice.Description, "골드", "gold")
               || ContainsAny(choice.Value, "GOLD.");
    }

    private static bool IsRelicRewardChoice(ObserverChoice choice)
    {
        return string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || ContainsAny(choice.Description, "relic", "유물");
    }

    private static string BuildProgressChoiceReason(ObserverChoice choice, ObserverSummary observer)
    {
        if (IsSkipLikeLabel(choice.Label))
        {
            return $"Progression skip '{choice.Label}' is visible. Prefer it over inspect or preview affordances.";
        }

        if (LooksLikeColorlessCardChoiceState(observer) && observer.Choices.Any(IsRewardCardChoice))
        {
            return $"Colorless reward choice '{choice.Label}' is visible. Click a real card option, not the relic inspect icons.";
        }

        if (LooksLikeRewardChoiceState(observer))
        {
            return $"Reward progression choice '{choice.Label}' is visible. Prefer it over inspect or preview affordances.";
        }

        return $"Event progression choice '{choice.Label}' is visible. Use the large room option instead of inspect affordances.";
    }

    private static bool LooksLikeShopState(ObserverSummary observer)
    {
        return string.Equals(observer.CurrentScreen, "shop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.VisibleScreen, "shop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.EncounterKind, "Shop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "shop", StringComparison.OrdinalIgnoreCase);
    }

    private static GuiSmokeStepDecision CreateClickDecisionFromNode(GuiSmokeStepRequest request, ObserverActionNode node, string targetLabel)
    {
        if (!TryParseNodeBounds(node.ScreenBounds, out var bounds))
        {
            throw new InvalidOperationException($"Action node '{node.Label}' does not include screen bounds.");
        }
        if (!TryResolveNormalizedBounds(request.WindowBounds, node.ScreenBounds, bounds, out var normalizedX, out var normalizedY, out var boundsSource))
        {
            throw new InvalidOperationException($"Action node '{node.Label}' uses stale or off-window bounds '{node.ScreenBounds}'.");
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            targetLabel,
            $"Auto provider selected node '{node.Label}' using {boundsSource} bounds.",
            0.92,
            null,
            1200,
            true,
                null);
    }

    private static GuiSmokeStepDecision CreateCombatEnemyTargetDecisionFromNode(
        GuiSmokeStepRequest request,
        ObserverActionNode node,
        string targetLabel,
        int retryCount)
    {
        if (!TryParseNodeBounds(node.ScreenBounds, out var bounds))
        {
            throw new InvalidOperationException($"Enemy target node '{node.Label}' does not include screen bounds.");
        }

        var anchor = retryCount switch
        {
            <= 0 => (X: 0.50f, Y: 0.52f, Suffix: "body"),
            1 => (X: 0.50f, Y: 0.40f, Suffix: "upper-body"),
            2 => (X: 0.62f, Y: 0.48f, Suffix: "right-body"),
            _ => (X: 0.38f, Y: 0.48f, Suffix: "left-body"),
        };
        if (!TryResolveNormalizedPointFromBounds(request.WindowBounds, node.ScreenBounds, bounds, anchor.X, anchor.Y, out var normalizedX, out var normalizedY, out var boundsSource))
        {
            throw new InvalidOperationException($"Enemy target node '{node.Label}' uses stale or off-window bounds '{node.ScreenBounds}'.");
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            targetLabel,
            $"Auto provider selected enemy target '{node.Label}' using {boundsSource} bounds at {anchor.Suffix}.",
            retryCount == 0 ? 0.94 : 0.90,
            "combat",
            800,
            true,
            null);
    }

    private static string BuildCombatEnemyTargetLabel(ObserverActionNode node, int retryCount)
    {
        var baseLabel = string.IsNullOrWhiteSpace(node.Label)
            ? "combat enemy target"
            : $"combat enemy target {node.Label.Trim()}";
        return retryCount switch
        {
            <= 0 => baseLabel,
            1 => $"{baseLabel} recenter",
            2 => $"{baseLabel} alternate",
            _ => $"{baseLabel} fallback",
        };
    }

    private static GuiSmokeStepDecision CreateClickDecisionFromChoice(
        GuiSmokeStepRequest request,
        ObserverChoice choice,
        string targetLabel,
        string reason,
        double confidence,
        string expectedScreen,
        int waitMs)
    {
        if (!TryParseNodeBounds(choice.ScreenBounds, out var bounds))
        {
            throw new InvalidOperationException($"Observer choice '{choice.Label}' does not include screen bounds.");
        }
        if (!TryResolveNormalizedBounds(request.WindowBounds, choice.ScreenBounds, bounds, out var normalizedX, out var normalizedY, out _))
        {
            throw new InvalidOperationException($"Observer choice '{choice.Label}' uses stale or off-window bounds '{choice.ScreenBounds}'.");
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            targetLabel,
            reason,
            confidence,
            expectedScreen,
            waitMs,
            true,
            null);
    }

    private static bool HasUsableLogicalBounds(string? raw)
    {
        if (!TryParseNodeBounds(raw, out var bounds))
        {
            return false;
        }

        var centerX = bounds.X + bounds.Width / 2f;
        var centerY = bounds.Y + bounds.Height / 2f;
        return centerX >= 0f
               && centerY >= 0f
               && centerX <= 1920f
               && centerY <= 1080f;
    }

    private static bool TryResolveNormalizedBounds(
        WindowBounds windowBounds,
        string? rawBounds,
        RectangleF bounds,
        out double normalizedX,
        out double normalizedY,
        out string boundsSource)
    {
        return TryResolveNormalizedPointFromBounds(windowBounds, rawBounds, bounds, 0.5f, 0.5f, out normalizedX, out normalizedY, out boundsSource);
    }

    private static bool TryResolveNormalizedPointFromBounds(
        WindowBounds windowBounds,
        string? rawBounds,
        RectangleF bounds,
        float anchorX,
        float anchorY,
        out double normalizedX,
        out double normalizedY,
        out string boundsSource)
    {
        normalizedX = default;
        normalizedY = default;
        boundsSource = "unknown";

        var pointX = bounds.X + bounds.Width * anchorX;
        var pointY = bounds.Y + bounds.Height * anchorY;
        if (HasUsableLogicalBounds(rawBounds))
        {
            normalizedX = Math.Clamp(pointX / 1920f, 0d, 1d);
            normalizedY = Math.Clamp(pointY / 1080f, 0d, 1d);
            boundsSource = "logical";
            return true;
        }

        if (!IsBoundsInsideWindow(rawBounds, windowBounds))
        {
            return false;
        }

        normalizedX = Math.Clamp((pointX - windowBounds.X) / Math.Max(1d, windowBounds.Width), 0d, 1d);
        normalizedY = Math.Clamp((pointY - windowBounds.Y) / Math.Max(1d, windowBounds.Height), 0d, 1d);
        boundsSource = "window";
        return true;
    }

    private static GuiSmokeStepDecision CreateWaitDecision(string reason, string? expectedScreen)
    {
        return new GuiSmokeStepDecision(
            "wait",
            null,
            null,
            null,
            null,
            null,
            reason,
            0.60,
            expectedScreen,
            2000,
            true,
            null);
    }

    private static bool TryParseNodeBounds(string? raw, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
        {
            return false;
        }

        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
        {
            return false;
        }

        if (width <= 0 || height <= 0)
        {
            return false;
        }

        bounds = new RectangleF(x, y, width, height);
        return true;
    }
}

sealed class HeadlessCodexDecisionProvider : IGuiDecisionProvider
{
    private readonly IReadOnlyDictionary<string, string> _options;

    public HeadlessCodexDecisionProvider(IReadOnlyDictionary<string, string> options)
    {
        _options = options;
    }

    public async Task<GuiSmokeStepDecision> GetDecisionAsync(string requestPath, string decisionPath, TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (!_options.TryGetValue("--provider-command", out var providerCommand) || string.IsNullOrWhiteSpace(providerCommand))
        {
            throw new InvalidOperationException("--provider-command is required for --provider headless.");
        }

        var expanded = providerCommand
            .Replace("{request}", requestPath, StringComparison.OrdinalIgnoreCase)
            .Replace("{decision}", decisionPath, StringComparison.OrdinalIgnoreCase);
        await GuiSmokeShared.RunProcessAsync(
            Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
            $"/d /c {expanded}",
            Directory.GetCurrentDirectory(),
            timeout).ConfigureAwait(false);

        if (!File.Exists(decisionPath))
        {
            throw new FileNotFoundException("Headless provider did not create a decision file.", decisionPath);
        }

        using var stream = new FileStream(decisionPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var parsed = await JsonSerializer.DeserializeAsync<GuiSmokeStepDecision>(stream, GuiSmokeShared.JsonOptions, cancellationToken).ConfigureAwait(false);
        return parsed ?? throw new InvalidOperationException("Failed to parse decision file.");
    }
}

sealed class AutoMapAnalysis
{
    public static readonly AutoMapAnalysis None = new(false, 0.5f, 0.5f, false, 0.5f, 0.5f);

    public AutoMapAnalysis(
        bool hasCurrentArrow,
        float arrowNormalizedX,
        float arrowNormalizedY,
        bool hasReachableNode,
        float reachableNodeNormalizedX,
        float reachableNodeNormalizedY)
    {
        HasCurrentArrow = hasCurrentArrow;
        ArrowNormalizedX = arrowNormalizedX;
        ArrowNormalizedY = arrowNormalizedY;
        HasReachableNode = hasReachableNode;
        ReachableNodeNormalizedX = reachableNodeNormalizedX;
        ReachableNodeNormalizedY = reachableNodeNormalizedY;
    }

    public bool HasCurrentArrow { get; }

    public float ArrowNormalizedX { get; }

    public float ArrowNormalizedY { get; }

    public bool HasReachableNode { get; }

    public float ReachableNodeNormalizedX { get; }

    public float ReachableNodeNormalizedY { get; }
}

sealed record AutoTreasureAnalysis(
    bool HasClosedChestHighlight,
    bool HasRewardIcon,
    double RewardIconNormalizedX,
    double RewardIconNormalizedY);

sealed record AutoRestSiteCardGridAnalysis(
    bool HasSelectableCard,
    double CardNormalizedX,
    double CardNormalizedY);

sealed record AutoEventCardGridAnalysis(
    bool HasSelectableCard,
    double CardNormalizedX,
    double CardNormalizedY);

static class AutoRestSiteCardGridAnalyzer
{
    private static readonly AutoRestSiteCardGridAnalysis None = new(false, 0.5d, 0.5d);

    public static AutoRestSiteCardGridAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("rest-site-card-grid", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoRestSiteCardGridAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return None;
        }

        try
        {
            var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(screenshotPath);
            if (!overlayAnalysis.HasBottomLeftBackArrow)
            {
                return None;
            }

            using var bitmap = new Bitmap(screenshotPath);
            var points = new List<Point>();
            for (var y = (int)(bitmap.Height * 0.08); y < (int)(bitmap.Height * 0.95); y += 4)
            {
                for (var x = (int)(bitmap.Width * 0.08); x < (int)(bitmap.Width * 0.92); x += 4)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    var brightness = (pixel.R + pixel.G + pixel.B) / 3.0;
                    var saturation = Math.Max(pixel.R, Math.Max(pixel.G, pixel.B))
                                     - Math.Min(pixel.R, Math.Min(pixel.G, pixel.B));
                    if (brightness >= 70 && saturation >= 28)
                    {
                        points.Add(new Point(x, y));
                    }
                }
            }

            if (points.Count < 600)
            {
                return None;
            }

            var candidates = FindClusters(points, 12, 12)
                .Select(cluster =>
                {
                    var minX = cluster.Min(static point => point.X);
                    var maxX = cluster.Max(static point => point.X);
                    var minY = cluster.Min(static point => point.Y);
                    var maxY = cluster.Max(static point => point.Y);
                    var width = maxX - minX;
                    var height = maxY - minY;
                    var centerX = cluster.Average(static point => point.X);
                    var centerY = cluster.Average(static point => point.Y);
                    return new
                    {
                        cluster,
                        width,
                        height,
                        centerX,
                        centerY,
                        score = Math.Abs(centerX - bitmap.Width / 2d) + Math.Abs(centerY - bitmap.Height / 2d),
                    };
                })
                .Where(entry =>
                    entry.cluster.Count >= 180
                    && entry.width is >= 110 and <= 220
                    && entry.height is >= 170 and <= 280
                    && entry.centerX >= bitmap.Width * 0.18
                    && entry.centerX <= bitmap.Width * 0.82
                    && entry.centerY >= bitmap.Height * 0.16
                    && entry.centerY <= bitmap.Height * 0.82)
                .OrderBy(entry => entry.score)
                .FirstOrDefault();
            if (candidates is null)
            {
                return None;
            }

            return new AutoRestSiteCardGridAnalysis(
                true,
                Math.Clamp(candidates.centerX / bitmap.Width, 0.10d, 0.90d),
                Math.Clamp(candidates.centerY / bitmap.Height, 0.10d, 0.90d));
        }
        catch (ArgumentException)
        {
            return None;
        }
        catch (IOException)
        {
            return None;
        }
    }

    private static List<List<Point>> FindClusters(List<Point> points, int maxDx, int maxDy)
    {
        var remaining = new HashSet<int>(Enumerable.Range(0, points.Count));
        var clusters = new List<List<Point>>();

        while (remaining.Count > 0)
        {
            var seedIndex = remaining.First();
            remaining.Remove(seedIndex);
            var queue = new Queue<int>();
            var cluster = new List<Point>();
            queue.Enqueue(seedIndex);

            while (queue.Count > 0)
            {
                var currentIndex = queue.Dequeue();
                var current = points[currentIndex];
                cluster.Add(current);

                var neighbors = remaining
                    .Where(index =>
                    {
                        var other = points[index];
                        return Math.Abs(other.X - current.X) <= maxDx
                               && Math.Abs(other.Y - current.Y) <= maxDy;
                    })
                    .ToArray();

                foreach (var neighbor in neighbors)
                {
                    remaining.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }
}

static class AutoEventCardGridAnalyzer
{
    private static readonly AutoEventCardGridAnalysis None = new(false, 0.5d, 0.5d);

    public static AutoEventCardGridAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("event-card-grid", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoEventCardGridAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return None;
        }

        try
        {
            using var bitmap = new Bitmap(screenshotPath);
            var points = new List<Point>();
            for (var y = (int)(bitmap.Height * 0.14); y < (int)(bitmap.Height * 0.82); y += 4)
            {
                for (var x = (int)(bitmap.Width * 0.14); x < (int)(bitmap.Width * 0.86); x += 4)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    var brightness = (pixel.R + pixel.G + pixel.B) / 3.0;
                    var saturation = Math.Max(pixel.R, Math.Max(pixel.G, pixel.B))
                                     - Math.Min(pixel.R, Math.Min(pixel.G, pixel.B));
                    if (brightness >= 70 && saturation >= 24)
                    {
                        points.Add(new Point(x, y));
                    }
                }
            }

            if (points.Count < 500)
            {
                return None;
            }

            var candidate = FindClusters(points, 12, 12)
                .Select(cluster =>
                {
                    var minX = cluster.Min(static point => point.X);
                    var maxX = cluster.Max(static point => point.X);
                    var minY = cluster.Min(static point => point.Y);
                    var maxY = cluster.Max(static point => point.Y);
                    var width = maxX - minX;
                    var height = maxY - minY;
                    var centerX = cluster.Average(static point => point.X);
                    var centerY = cluster.Average(static point => point.Y);
                    return new
                    {
                        cluster,
                        width,
                        height,
                        centerX,
                        centerY,
                        score = Math.Abs(centerX - bitmap.Width / 2d) + Math.Abs(centerY - bitmap.Height / 2d),
                    };
                })
                .Where(entry =>
                    entry.cluster.Count >= 160
                    && entry.width is >= 100 and <= 240
                    && entry.height is >= 150 and <= 300
                    && entry.centerX >= bitmap.Width * 0.18
                    && entry.centerX <= bitmap.Width * 0.82
                    && entry.centerY >= bitmap.Height * 0.18
                    && entry.centerY <= bitmap.Height * 0.78)
                .OrderBy(entry => entry.score)
                .FirstOrDefault();
            if (candidate is null)
            {
                return None;
            }

            return new AutoEventCardGridAnalysis(
                true,
                Math.Clamp(candidate.centerX / bitmap.Width, 0.12d, 0.88d),
                Math.Clamp(candidate.centerY / bitmap.Height, 0.12d, 0.82d));
        }
        catch (ArgumentException)
        {
            return None;
        }
        catch (IOException)
        {
            return None;
        }
    }

    private static List<List<Point>> FindClusters(List<Point> points, int maxDx, int maxDy)
    {
        var remaining = new HashSet<int>(Enumerable.Range(0, points.Count));
        var clusters = new List<List<Point>>();

        while (remaining.Count > 0)
        {
            var seedIndex = remaining.First();
            remaining.Remove(seedIndex);
            var queue = new Queue<int>();
            var cluster = new List<Point>();
            queue.Enqueue(seedIndex);

            while (queue.Count > 0)
            {
                var currentIndex = queue.Dequeue();
                var current = points[currentIndex];
                cluster.Add(current);

                var neighbors = remaining
                    .Where(index =>
                    {
                        var other = points[index];
                        return Math.Abs(other.X - current.X) <= maxDx
                               && Math.Abs(other.Y - current.Y) <= maxDy;
                    })
                    .ToArray();

                foreach (var neighbor in neighbors)
                {
                    remaining.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }
}

static class AutoTreasureAnalyzer
{
    private static readonly AutoTreasureAnalysis None = new(false, false, 0.5d, 0.5d);

    public static AutoTreasureAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("treasure", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoTreasureAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return None;
        }

        try
        {
            using var bitmap = new Bitmap(screenshotPath);
            var hasClosedChestHighlight = HasClosedChestHighlight(bitmap);
            var points = new List<Point>();
            var xStart = (int)(bitmap.Width * 0.35);
            var xEnd = (int)(bitmap.Width * 0.65);
            var yStart = (int)(bitmap.Height * 0.28);
            var yEnd = (int)(bitmap.Height * 0.62);

            for (var y = yStart; y < yEnd; y += 1)
            {
                for (var x = xStart; x < xEnd; x += 1)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    var brightness = (pixel.R + pixel.G + pixel.B) / 3.0;
                    if (pixel.B >= 120
                        && pixel.B >= pixel.R + 25
                        && pixel.B >= pixel.G + 20
                        && brightness >= 70)
                    {
                        points.Add(new Point(x, y));
                    }
                }
            }

            if (points.Count < 90)
            {
                return new AutoTreasureAnalysis(hasClosedChestHighlight, false, 0.5d, 0.5d);
            }

            var rewardCluster = FindLargestCluster(points, 14, 14);
            if (rewardCluster.Count < 80)
            {
                return new AutoTreasureAnalysis(hasClosedChestHighlight, false, 0.5d, 0.5d);
            }

            var minX = rewardCluster.Min(static point => point.X);
            var maxX = rewardCluster.Max(static point => point.X);
            var minY = rewardCluster.Min(static point => point.Y);
            var maxY = rewardCluster.Max(static point => point.Y);
            var width = maxX - minX;
            var height = maxY - minY;
            var centroidX = rewardCluster.Average(static point => point.X);
            var centroidY = rewardCluster.Average(static point => point.Y);
            var normalizedX = centroidX / Math.Max(1d, bitmap.Width);
            var normalizedY = centroidY / Math.Max(1d, bitmap.Height);

            if (width is < 18 or > 120
                || height is < 18 or > 120
                || normalizedX is < 0.40 or > 0.60
                || normalizedY is < 0.30 or > 0.55)
            {
                return new AutoTreasureAnalysis(hasClosedChestHighlight, false, 0.5d, 0.5d);
            }

            return new AutoTreasureAnalysis(hasClosedChestHighlight, true, normalizedX, normalizedY);
        }
        catch (ArgumentException)
        {
            return None;
        }
        catch (IOException)
        {
            return None;
        }
    }

    private static bool HasClosedChestHighlight(Bitmap bitmap)
    {
        var count = 0;
        var xStart = (int)(bitmap.Width * 0.25);
        var xEnd = (int)(bitmap.Width * 0.78);
        var yStart = (int)(bitmap.Height * 0.28);
        var yEnd = (int)(bitmap.Height * 0.82);

        for (var y = yStart; y < yEnd; y += 1)
        {
            for (var x = xStart; x < xEnd; x += 1)
            {
                var pixel = bitmap.GetPixel(x, y);
                var brightness = (pixel.R + pixel.G + pixel.B) / 3.0;
                if (brightness >= 225
                    && Math.Abs(pixel.R - pixel.G) <= 20
                    && Math.Abs(pixel.G - pixel.B) <= 20
                    && Math.Abs(pixel.R - pixel.B) <= 20)
                {
                    count += 1;
                }
            }
        }

        return count >= 1200;
    }

    private static List<Point> FindLargestCluster(List<Point> points, int maxDx, int maxDy)
    {
        var remaining = new HashSet<int>(Enumerable.Range(0, points.Count));
        var largest = new List<Point>();

        while (remaining.Count > 0)
        {
            var seedIndex = remaining.First();
            remaining.Remove(seedIndex);
            var queue = new Queue<int>();
            var cluster = new List<Point>();
            queue.Enqueue(seedIndex);

            while (queue.Count > 0)
            {
                var currentIndex = queue.Dequeue();
                var current = points[currentIndex];
                cluster.Add(current);

                var neighbors = remaining
                    .Where(index =>
                    {
                        var other = points[index];
                        return Math.Abs(other.X - current.X) <= maxDx
                               && Math.Abs(other.Y - current.Y) <= maxDy;
                    })
                    .ToArray();

                foreach (var neighbor in neighbors)
                {
                    remaining.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            if (cluster.Count > largest.Count)
            {
                largest = cluster;
            }
        }

        return largest;
    }
}

static class AutoMapAnalyzer
{
    public static AutoMapAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("map", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoMapAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return AutoMapAnalysis.None;
        }

        try
        {
            using var bitmap = new Bitmap(screenshotPath);
            var directArrow = TryFindCurrentArrow(bitmap);
            if (directArrow is not null)
            {
                var reachableNode = TryFindReachableNode(bitmap, directArrow.Value.X, directArrow.Value.Y);
                return new AutoMapAnalysis(
                    true,
                    (float)(directArrow.Value.X / bitmap.Width),
                    (float)(directArrow.Value.Y / bitmap.Height),
                    reachableNode is not null,
                    reachableNode is null ? 0.5f : (float)(reachableNode.Value.X / bitmap.Width),
                    reachableNode is null ? 0.5f : (float)(reachableNode.Value.Y / bitmap.Height));
            }

            var samples = new List<Point>();

            var yStart = (int)(bitmap.Height * 0.22);
            var yEnd = (int)(bitmap.Height * 0.90);
            var xStart = (int)(bitmap.Width * 0.20);
            var xEnd = (int)(bitmap.Width * 0.82);

            for (var y = yStart; y < yEnd; y += 2)
            {
                for (var x = xStart; x < xEnd; x += 2)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    if (pixel.R >= 170 && pixel.G <= 110 && pixel.B <= 110 && pixel.R - pixel.G >= 70)
                    {
                        samples.Add(new Point(x, y));
                    }
                }
            }

            if (samples.Count < 12)
            {
                return AutoMapAnalysis.None;
            }

            var bestCluster = FindBestArrowCluster(samples);
            if (bestCluster.Count < 8)
            {
                return AutoMapAnalysis.None;
            }

            var centroidX = bestCluster.Average(static point => point.X);
            var centroidY = bestCluster.Average(static point => point.Y);
            var fallbackReachableNode = TryFindReachableNode(bitmap, centroidX, centroidY);
            return new AutoMapAnalysis(
                true,
                (float)(centroidX / bitmap.Width),
                (float)(centroidY / bitmap.Height),
                fallbackReachableNode is not null,
                fallbackReachableNode is null ? 0.5f : (float)(fallbackReachableNode.Value.X / bitmap.Width),
                fallbackReachableNode is null ? 0.5f : (float)(fallbackReachableNode.Value.Y / bitmap.Height));
        }
        catch (ArgumentException)
        {
            return AutoMapAnalysis.None;
        }
        catch (IOException)
        {
            return AutoMapAnalysis.None;
        }
    }

    private static PointF? TryFindCurrentArrow(Bitmap bitmap)
    {
        var xStart = (int)(bitmap.Width * 0.25);
        var xEnd = (int)(bitmap.Width * 0.70);
        var yStart = (int)(bitmap.Height * 0.25);
        var yEnd = (int)(bitmap.Height * 0.70);
        var samples = new List<Point>();

        for (var y = yStart; y < yEnd; y += 2)
        {
            for (var x = xStart; x < xEnd; x += 2)
            {
                var pixel = bitmap.GetPixel(x, y);
                if (pixel.R >= 185 && pixel.G <= 105 && pixel.B <= 105 && pixel.R - pixel.G >= 80)
                {
                    samples.Add(new Point(x, y));
                }
            }
        }

        if (samples.Count < 8)
        {
            return null;
        }

        var clusters = FindClusters(samples, 16, 16);
        var candidate = clusters
            .Where(cluster => cluster.Count >= 6)
            .Select(cluster =>
            {
                var centroidX = cluster.Average(static point => point.X);
                var centroidY = cluster.Average(static point => point.Y);
                return new
                {
                    cluster,
                    centroidX,
                    centroidY,
                };
            })
            .Where(entry =>
                entry.centroidX >= bitmap.Width * 0.30
                && entry.centroidX <= bitmap.Width * 0.62
                && entry.centroidY >= bitmap.Height * 0.38
                && entry.centroidY <= bitmap.Height * 0.74)
            .OrderByDescending(entry => entry.centroidY)
            .ThenByDescending(entry => entry.cluster.Count)
            .FirstOrDefault();

        if (candidate is not null)
        {
            return new PointF((float)candidate.centroidX, (float)candidate.centroidY);
        }

        return new PointF(
            (float)samples.Average(static point => point.X),
            (float)samples.Average(static point => point.Y));
    }

    private static List<Point> FindBestArrowCluster(List<Point> samples)
    {
        var remaining = new HashSet<int>(Enumerable.Range(0, samples.Count));
        var bestCluster = new List<Point>();

        while (remaining.Count > 0)
        {
            var seedIndex = remaining.First();
            remaining.Remove(seedIndex);
            var queue = new Queue<int>();
            queue.Enqueue(seedIndex);
            var cluster = new List<Point>();

            while (queue.Count > 0)
            {
                var currentIndex = queue.Dequeue();
                var current = samples[currentIndex];
                cluster.Add(current);

                var neighbors = remaining
                    .Where(index =>
                    {
                        var other = samples[index];
                        return Math.Abs(other.X - current.X) <= 18 && Math.Abs(other.Y - current.Y) <= 18;
                    })
                    .ToArray();

                foreach (var neighbor in neighbors)
                {
                    remaining.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            if (cluster.Count > bestCluster.Count)
            {
                bestCluster = cluster;
            }
        }

        return bestCluster;
    }

    private static PointF? TryFindReachableNode(Bitmap bitmap, double currentArrowX, double currentArrowY)
    {
        var xMin = Math.Max(0, (int)(currentArrowX - bitmap.Width * 0.14));
        var xMax = Math.Min(bitmap.Width - 1, (int)(currentArrowX + bitmap.Width * 0.14));
        var yMin = Math.Max(0, (int)(currentArrowY - bitmap.Height * 0.24));
        var yMax = Math.Min(bitmap.Height - 1, (int)(currentArrowY + bitmap.Height * 0.26));
        var samples = new List<Point>();

        for (var y = yMin; y <= yMax; y += 2)
        {
            for (var x = xMin; x <= xMax; x += 2)
            {
                var distance = Math.Sqrt(Math.Pow(x - currentArrowX, 2) + Math.Pow(y - currentArrowY, 2));
                if (distance <= 40)
                {
                    continue;
                }

                var pixel = bitmap.GetPixel(x, y);
                if (pixel.R <= 80 && pixel.G <= 80 && pixel.B <= 80)
                {
                    samples.Add(new Point(x, y));
                }
            }
        }

        if (samples.Count < 24)
        {
            return null;
        }

        var clusters = FindClusters(samples, 18, 18);
        var candidates = clusters
            .Where(cluster => cluster.Count >= 10)
            .Select(cluster =>
            {
                var centroidX = cluster.Average(static point => point.X);
                var centroidY = cluster.Average(static point => point.Y);
                var dx = Math.Abs(centroidX - currentArrowX);
                var dy = Math.Abs(centroidY - currentArrowY);
                var score = dx * 1.5 + dy;
                return new { cluster, centroidX, centroidY, dx, dy, score };
            })
            .Where(entry => entry.dx <= bitmap.Width * 0.10 && entry.dy >= 28 && entry.dy <= bitmap.Height * 0.20)
            .ToArray();

        var candidate = candidates
            .Where(entry => entry.centroidY < currentArrowY - 24)
            .OrderBy(entry => entry.score)
            .FirstOrDefault()
            ?? candidates
                .OrderBy(entry => entry.score)
                .FirstOrDefault();

        return candidate is null ? null : new PointF((float)candidate.centroidX, (float)candidate.centroidY);
    }

    private static List<List<Point>> FindClusters(List<Point> samples, int maxDx, int maxDy)
    {
        var remaining = new HashSet<int>(Enumerable.Range(0, samples.Count));
        var clusters = new List<List<Point>>();

        while (remaining.Count > 0)
        {
            var seedIndex = remaining.First();
            remaining.Remove(seedIndex);
            var queue = new Queue<int>();
            queue.Enqueue(seedIndex);
            var cluster = new List<Point>();

            while (queue.Count > 0)
            {
                var currentIndex = queue.Dequeue();
                var current = samples[currentIndex];
                cluster.Add(current);

                var neighbors = remaining
                    .Where(index =>
                    {
                        var other = samples[index];
                        return Math.Abs(other.X - current.X) <= maxDx && Math.Abs(other.Y - current.Y) <= maxDy;
                    })
                    .ToArray();

                foreach (var neighbor in neighbors)
                {
                    remaining.Remove(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }
}

sealed record AutoOverlayUiAnalysis(
    bool HasCentralOverlayPanel,
    bool HasBottomLeftBackArrow,
    bool HasRightProceedArrow,
    double? RightProceedNormalizedX,
    double? RightProceedNormalizedY);

static class AutoOverlayUiAnalyzer
{
    public static AutoOverlayUiAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("overlay-ui", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoOverlayUiAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return new AutoOverlayUiAnalysis(false, false, false, null, null);
        }

        try
        {
            using var bitmap = new Bitmap(screenshotPath);
            var center = AverageColor(bitmap, 0.32, 0.16, 0.68, 0.88);
            var left = AverageColor(bitmap, 0.05, 0.20, 0.25, 0.82);
            var right = AverageColor(bitmap, 0.75, 0.20, 0.95, 0.82);
            var hasCentralOverlayPanel = center.Brightness >= 35
                                         && center.B >= center.R - 10
                                         && center.B >= center.G - 10
                                         && center.Brightness - Math.Max(left.Brightness, right.Brightness) >= 20;
            var hasBottomLeftBackArrow = CountArrowLikePixels(bitmap, 0.00, 0.72, 0.18, 0.98) >= 18;
            var proceedCentroid = TryFindArrowLikeCentroid(bitmap, 0.78, 0.68, 1.00, 0.98);
            var hasRightProceedArrow = proceedCentroid is not null || CountArrowLikePixels(bitmap, 0.78, 0.68, 1.00, 0.98) >= 2;
            return new AutoOverlayUiAnalysis(
                hasCentralOverlayPanel,
                hasBottomLeftBackArrow,
                hasRightProceedArrow,
                proceedCentroid is null ? null : proceedCentroid.Value.X / Math.Max(1f, bitmap.Width),
                proceedCentroid is null ? null : proceedCentroid.Value.Y / Math.Max(1f, bitmap.Height));
        }
        catch (ArgumentException)
        {
            return new AutoOverlayUiAnalysis(false, false, false, null, null);
        }
        catch (IOException)
        {
            return new AutoOverlayUiAnalysis(false, false, false, null, null);
        }
    }

    private static int CountArrowLikePixels(Bitmap bitmap, double left, double top, double right, double bottom)
    {
        var count = 0;
        AutoCombatAnalyzer.ForEachSample(bitmap, left, top, right, bottom, 28, 20, color =>
        {
            if (color.R >= 140 && color.R - color.G >= 35 && color.G >= 35 && color.G <= 185 && color.B <= 120)
            {
                count += 1;
            }
        });

        return count;
    }

    private static PointF? TryFindArrowLikeCentroid(Bitmap bitmap, double left, double top, double right, double bottom)
    {
        var sumX = 0d;
        var sumY = 0d;
        var count = 0;
        AutoCombatAnalyzer.ForEachSample(bitmap, left, top, right, bottom, 40, 28, (color, x, y) =>
        {
            if (color.R >= 140 && color.R - color.G >= 35 && color.G >= 35 && color.G <= 185 && color.B <= 120)
            {
                sumX += x;
                sumY += y;
                count += 1;
            }
        });

        if (count < 4)
        {
            return null;
        }

        return new PointF((float)(sumX / count), (float)(sumY / count));
    }

    private static (double R, double G, double B, double Brightness) AverageColor(Bitmap bitmap, double left, double top, double right, double bottom)
    {
        var totalR = 0d;
        var totalG = 0d;
        var totalB = 0d;
        var total = 0;
        AutoCombatAnalyzer.ForEachSample(bitmap, left, top, right, bottom, 18, 18, color =>
        {
            totalR += color.R;
            totalG += color.G;
            totalB += color.B;
            total += 1;
        });

        if (total == 0)
        {
            return (0, 0, 0, 0);
        }

        var averageR = totalR / total;
        var averageG = totalG / total;
        var averageB = totalB / total;
        return (averageR, averageG, averageB, (averageR + averageG + averageB) / 3.0);
    }
}

enum AutoCombatCardKind
{
    Unknown,
    AttackLike,
    DefendLike,
}

enum AutoCombatOverlayBand
{
    None,
    Left,
    Center,
    Right,
}

sealed record AutoCombatAnalysis(
    bool HasSelectedCard,
    AutoCombatOverlayBand SelectedOverlayBand,
    bool HasTargetArrow,
    bool HasSelfTargetBrackets,
    AutoCombatCardKind SelectedCardKind);

sealed record AutoCombatHandSlotAnalysis(
    int SlotIndex,
    bool IsVisible,
    AutoCombatCardKind Kind,
    double RedBlueDelta,
    double Brightness,
    double CenterX,
    double CenterY);

sealed record AutoCombatHandAnalysis(
    IReadOnlyList<AutoCombatHandSlotAnalysis> Slots)
{
    public AutoCombatHandSlotAnalysis? TryGetSlot(int slotIndex)
    {
        return Slots.FirstOrDefault(slot => slot.SlotIndex == slotIndex);
    }
}

sealed record PendingCombatSelection(
    int SlotIndex,
    AutoCombatCardKind Kind);

sealed record CombatNoOpLoopAnalysis(
    bool LoopDetected,
    int? BlockedSlotIndex,
    int RepeatedSelectionCount);

sealed record ReconstructedHandleCombatContext(
    IReadOnlyList<GuiSmokeHistoryEntry> CombatHistory,
    PendingCombatSelection? PendingSelection,
    IReadOnlyDictionary<int, int> CombatNoOpCountsBySlot,
    CombatNoOpLoopAnalysis CombatNoOpLoop,
    bool RepeatedNonEnemyLoop,
    bool RepeatedAttackSelectionLoop);

static class HandleCombatContextSupport
{
    public const int SerializedHistoryWindow = 12;

    public static IReadOnlyList<GuiSmokeHistoryEntry> BuildSerializedHistoryWindow(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return history
            .Where(entry => string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            .TakeLast(SerializedHistoryWindow)
            .ToArray();
    }

    public static ReconstructedHandleCombatContext Reconstruct(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var combatHistory = BuildSerializedHistoryWindow(history);
        var pendingSelection = CombatHistorySupport.TryGetPendingCombatSelection(combatHistory);
        var combatNoOpCountsBySlot = CombatHistorySupport.GetCombatNoOpCountsBySlot(combatHistory);
        return new ReconstructedHandleCombatContext(
            combatHistory,
            pendingSelection,
            combatNoOpCountsBySlot,
            AnalyzeCombatNoOpLoop(combatHistory, combatNoOpCountsBySlot),
            HasRecentRepeatedNonEnemyLoop(combatHistory),
            HasRecentRepeatedAttackSelectionLoop(combatHistory));
    }

    public static bool HasRecentNonEnemySelection(ReconstructedHandleCombatContext context, int slotIndex)
    {
        return CombatHistorySupport.HasRecentNonEnemySelection(context.CombatHistory, slotIndex);
    }

    public static int GetCombatNoOpCountForSlot(ReconstructedHandleCombatContext context, int slotIndex)
    {
        return context.CombatNoOpCountsBySlot.TryGetValue(slotIndex, out var count)
            ? count
            : 0;
    }

    private static bool HasRecentRepeatedNonEnemyLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var labels = history
            .Select(static entry => entry.TargetLabel)
            .Where(static label =>
                !string.IsNullOrWhiteSpace(label)
                && (IsNonEnemySelectionLabel(label)
                    || string.Equals(label, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase)))
            .TakeLast(6)
            .ToArray();
        if (labels.Length < 4)
        {
            return false;
        }

        if (labels.Length >= 4
            && IsNonEnemySelectionLabel(labels[0])
            && string.Equals(labels[1], "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase)
            && IsNonEnemySelectionLabel(labels[2])
            && string.Equals(labels[3], "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var recentSelectCount = labels.Count(IsNonEnemySelectionLabel);
        if (recentSelectCount >= 3)
        {
            var distinctSelectionLabels = labels
                .Where(IsNonEnemySelectionLabel)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
            if (distinctSelectionLabels == 1)
            {
                return true;
            }
        }

        if (labels.Length >= 5)
        {
            var trailingWindow = labels.TakeLast(5).ToArray();
            var allowedMixedLoop = trailingWindow.All(label =>
                IsNonEnemySelectionLabel(label)
                || string.Equals(label, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase));
            if (allowedMixedLoop && trailingWindow.Count(IsNonEnemySelectionLabel) >= 3)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasRecentRepeatedAttackSelectionLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var labels = history
            .Select(static entry => entry.TargetLabel)
            .Where(static label => !string.IsNullOrWhiteSpace(label))
            .TakeLast(6)
            .ToArray();
        if (labels.Length < 3)
        {
            return false;
        }

        var attackSelections = labels
            .Where(static label => label is not null && label.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase))
            .Select(static label => label!)
            .TakeLast(4)
            .ToArray();
        if (attackSelections.Length < 3)
        {
            return false;
        }

        var distinctAttackSelections = attackSelections
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (distinctAttackSelections > 1)
        {
            return true;
        }

        return attackSelections.Length >= 3;
    }

    private static CombatNoOpLoopAnalysis AnalyzeCombatNoOpLoop(
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        IReadOnlyDictionary<int, int> recentNoOpCounts)
    {
        if (history.Count < 4 || recentNoOpCounts.Count == 0)
        {
            return new CombatNoOpLoopAnalysis(false, null, 0);
        }

        var mostRecentBlockedSlot = history
            .Reverse()
            .Select(entry => CombatHistorySupport.ExtractCombatLaneSlotIndex(entry.TargetLabel))
            .FirstOrDefault(static slotIndex => slotIndex.HasValue);
        if (!mostRecentBlockedSlot.HasValue)
        {
            mostRecentBlockedSlot = recentNoOpCounts
                .OrderByDescending(static pair => pair.Value)
                .ThenBy(static pair => pair.Key)
                .Select(static pair => (int?)pair.Key)
                .FirstOrDefault();
        }

        if (!mostRecentBlockedSlot.HasValue)
        {
            return new CombatNoOpLoopAnalysis(false, null, 0);
        }

        var recentEnemyTargetCount = history.Count(entry =>
            string.Equals(entry.TargetLabel, "auto-target enemy", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entry.TargetLabel, "auto-target enemy recenter", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entry.TargetLabel, "auto-target enemy alternate", StringComparison.OrdinalIgnoreCase)
            || (entry.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) ?? false));
        var repeatedSameSlotCount = recentNoOpCounts.TryGetValue(mostRecentBlockedSlot.Value, out var blockedCount)
            ? blockedCount
            : 0;
        var loopDetected = repeatedSameSlotCount >= 2 && recentEnemyTargetCount >= 2;
        return new CombatNoOpLoopAnalysis(loopDetected, mostRecentBlockedSlot, repeatedSameSlotCount);
    }

    private static bool IsNonEnemySelectionLabel(string? targetLabel)
    {
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return false;
        }

        return targetLabel.StartsWith("combat select non-enemy slot ", StringComparison.OrdinalIgnoreCase)
               || targetLabel.StartsWith("combat select defend slot ", StringComparison.OrdinalIgnoreCase);
    }
}

static class CombatHistorySupport
{
    public static PendingCombatSelection? TryGetPendingCombatSelection(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return TryGetPendingCombatSelection(history, history.Count);
    }

    public static PendingCombatSelection? TryGetPendingCombatSelection(IReadOnlyList<GuiSmokeHistoryEntry> history, int endExclusive)
    {
        for (var index = Math.Min(history.Count, endExclusive) - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryParsePendingCombatSelection(entry.TargetLabel, out var selection))
            {
                return selection;
            }

            if (IsSelectionClearingEntry(entry))
            {
                return null;
            }

            if (IsNeutralCombatLabel(entry.TargetLabel))
            {
                continue;
            }

            if (IsCombatDecisionAction(entry.Action))
            {
                return null;
            }
        }

        return null;
    }

    public static IReadOnlyDictionary<int, int> GetCombatNoOpCountsBySlot(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var counts = new Dictionary<int, int>();
        for (var index = 0; index < history.Count; index += 1)
        {
            var entry = history[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                || !string.Equals(entry.Action, "combat-noop", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!TryResolveCombatLaneSlotIndex(entry.TargetLabel, history, index + 1, out var slotIndex))
            {
                continue;
            }

            counts[slotIndex] = counts.TryGetValue(slotIndex, out var count)
                ? count + 1
                : 1;
        }

        return counts;
    }

    public static string? ResolveCombatLaneLabel(string? targetLabel, IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return TryResolveCombatLaneSlotIndex(targetLabel, history, history.Count, out var slotIndex)
            ? $"combat lane slot {slotIndex}"
            : targetLabel;
    }

    public static bool HasRecentNonEnemySelection(IReadOnlyList<GuiSmokeHistoryEntry> history, int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            return false;
        }

        return history
            .Where(entry => string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            .TakeLast(8)
            .Any(entry =>
                TryParsePendingCombatSelection(entry.TargetLabel, out var selection)
                && selection is { Kind: AutoCombatCardKind.DefendLike }
                && selection.SlotIndex == slotIndex);
    }

    public static bool TryParsePendingCombatSelection(string? targetLabel, out PendingCombatSelection? selection)
    {
        selection = null;
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return false;
        }

        if (targetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase)
            && ExtractFirstDigit(targetLabel) is { } attackSlot
            && IsValidSlotIndex(attackSlot))
        {
            selection = new PendingCombatSelection(attackSlot, AutoCombatCardKind.AttackLike);
            return true;
        }

        if ((targetLabel.StartsWith("combat select non-enemy slot ", StringComparison.OrdinalIgnoreCase)
             || targetLabel.StartsWith("combat select defend slot ", StringComparison.OrdinalIgnoreCase))
            && ExtractFirstDigit(targetLabel) is { } nonEnemySlot
            && IsValidSlotIndex(nonEnemySlot))
        {
            selection = new PendingCombatSelection(nonEnemySlot, AutoCombatCardKind.DefendLike);
            return true;
        }

        if (targetLabel.StartsWith("auto-select slot ", StringComparison.OrdinalIgnoreCase)
            && ExtractFirstDigit(targetLabel) is { } legacySlot
            && IsValidSlotIndex(legacySlot))
        {
            selection = new PendingCombatSelection(legacySlot, AutoCombatCardKind.AttackLike);
            return true;
        }

        return false;
    }

    public static int? ExtractCombatLaneSlotIndex(string? targetLabel)
    {
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return null;
        }

        if (targetLabel.StartsWith("combat lane slot ", StringComparison.OrdinalIgnoreCase)
            || targetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase))
        {
            return ExtractFirstDigit(targetLabel);
        }

        return null;
    }

    public static bool IsCombatEnemyTargetLabel(string? targetLabel)
    {
        return targetLabel is not null
               && (targetLabel.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsCombatEndTurnLabel(string? targetLabel)
    {
        return !string.IsNullOrWhiteSpace(targetLabel)
               && (targetLabel.Contains("end turn", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.Contains("턴 종료", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsCombatCancelSelectionLabel(string? targetLabel)
    {
        return !string.IsNullOrWhiteSpace(targetLabel)
               && (targetLabel.Contains("cancel", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.Contains("취소", StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryResolveCombatLaneSlotIndex(
        string? targetLabel,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        int endExclusive,
        out int slotIndex)
    {
        slotIndex = default;
        if (ExtractCombatLaneSlotIndex(targetLabel) is { } directSlot
            && IsValidSlotIndex(directSlot))
        {
            slotIndex = directSlot;
            return true;
        }

        if (!IsCombatEnemyTargetLabel(targetLabel))
        {
            return false;
        }

        if (TryGetPendingCombatSelection(history, endExclusive) is { Kind: AutoCombatCardKind.AttackLike } scopedSelection
            && IsValidSlotIndex(scopedSelection.SlotIndex))
        {
            slotIndex = scopedSelection.SlotIndex;
            return true;
        }

        if (TryGetPendingCombatSelection(history) is { Kind: AutoCombatCardKind.AttackLike } recoveredSelection
            && IsValidSlotIndex(recoveredSelection.SlotIndex))
        {
            slotIndex = recoveredSelection.SlotIndex;
            return true;
        }

        return false;
    }

    private static bool IsSelectionClearingEntry(GuiSmokeHistoryEntry entry)
    {
        return IsCombatEndTurnLabel(entry.TargetLabel)
               || IsCombatCancelSelectionLabel(entry.TargetLabel)
               || string.Equals(entry.TargetLabel, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNeutralCombatLabel(string? targetLabel)
    {
        return string.IsNullOrWhiteSpace(targetLabel)
               || targetLabel.StartsWith("combat lane slot ", StringComparison.OrdinalIgnoreCase)
               || IsCombatEnemyTargetLabel(targetLabel)
               || string.Equals(targetLabel, "observer-accepted", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCombatDecisionAction(string action)
    {
        return string.Equals(action, "click", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "click-current", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "right-click", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "press-key", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex is >= 1 and <= 5;
    }

    private static int? ExtractFirstDigit(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        foreach (var character in value)
        {
            if (char.IsDigit(character))
            {
                return character - '0';
            }
        }

        return null;
    }
}

sealed record CombatRuntimeState(
    bool? CardPlayPending,
    string? PlayMode,
    PendingCombatSelection? PendingSelection,
    bool? TargetingInProgress,
    string? HoveredTargetKind,
    string? HoveredTargetId,
    string? HoveredTargetLabel,
    string? LastCardPlayStartedCardId,
    string? LastCardPlayFinishedCardId,
    string? LastCardPlayFinishedCardName)
{
    public bool KeepsCardPlayOpen => CardPlayPending == true || TargetingInProgress == true;

    public bool ExplicitlyClearedSelection => CardPlayPending == false && TargetingInProgress != true;

    public bool HasExplicitEnemyTargetingEvidence =>
        TargetingInProgress == true
        || string.Equals(HoveredTargetKind, "enemy", StringComparison.OrdinalIgnoreCase);

    public bool HasAttackSelectionWithoutExplicitTargeting =>
        PendingSelection?.Kind == AutoCombatCardKind.AttackLike
        && !HasExplicitEnemyTargetingEvidence;
}

static class CombatRuntimeStateSupport
{
    public static CombatRuntimeState Read(ObserverSummary observer, IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var selectedCardSlot = TryGetMetaInt(observer, "combatSelectedCardSlot");
        var awaitingPlaySlots = ParseSlotList(TryGetMetaValue(observer, "combatAwaitingPlaySlots"));
        if (selectedCardSlot is null && awaitingPlaySlots.Count > 0)
        {
            selectedCardSlot = awaitingPlaySlots[0];
        }

        var selectedCardTargetType = TryGetMetaValue(observer, "combatSelectedCardTargetType");
        var selectedCardType = TryGetMetaValue(observer, "combatSelectedCardType");
        var pendingSelection = TryResolvePendingSelection(selectedCardSlot, selectedCardTargetType, selectedCardType, observer, combatCardKnowledge);

        return new CombatRuntimeState(
            TryGetMetaBool(observer, "combatCardPlayPending"),
            TryGetMetaValue(observer, "combatPlayMode"),
            pendingSelection,
            TryGetMetaBool(observer, "combatTargetingInProgress"),
            TryGetMetaValue(observer, "combatHoveredTargetKind"),
            TryGetMetaValue(observer, "combatHoveredTargetId"),
            TryGetMetaValue(observer, "combatHoveredTargetLabel"),
            TryGetMetaValue(observer, "combatLastCardPlayStartedCardId"),
            TryGetMetaValue(observer, "combatLastCardPlayFinishedCardId"),
            TryGetMetaValue(observer, "combatLastCardPlayFinishedCardName"));
    }

    public static PendingCombatSelection? ResolvePendingSelection(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? historyPendingSelection)
    {
        var runtime = Read(observer, combatCardKnowledge);
        if (runtime.PendingSelection is not null)
        {
            return runtime.PendingSelection;
        }

        if (runtime.ExplicitlyClearedSelection)
        {
            return null;
        }

        return historyPendingSelection;
    }

    public static bool HasNonEnemyConfirmEvidence(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? pendingSelection,
        AutoCombatAnalysis analysis)
    {
        var runtime = Read(observer, combatCardKnowledge);
        if (runtime.PendingSelection?.Kind == AutoCombatCardKind.DefendLike
            && runtime.PendingSelection.SlotIndex is >= 1 and <= 5
            && runtime.TargetingInProgress != true)
        {
            return true;
        }

        return pendingSelection?.Kind == AutoCombatCardKind.DefendLike
               && pendingSelection.SlotIndex is >= 1 and <= 5
               && ((analysis.HasSelectedCard && analysis.SelectedCardKind == AutoCombatCardKind.DefendLike)
                   || analysis.HasSelfTargetBrackets);
    }

    public static bool CanResolveEnemyTarget(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        PendingCombatSelection? pendingSelection,
        AutoCombatAnalysis analysis)
    {
        var runtime = Read(observer, combatCardKnowledge);
        return runtime.PendingSelection?.Kind == AutoCombatCardKind.AttackLike
               && runtime.HasExplicitEnemyTargetingEvidence;
    }

    public static bool RequiresExplicitTargetingBeforeEnemyClick(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        return Read(observer, combatCardKnowledge).HasAttackSelectionWithoutExplicitTargeting;
    }

    public static bool HasSelectionToKeep(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var runtime = Read(observer, combatCardKnowledge);
        return runtime.KeepsCardPlayOpen || runtime.PendingSelection is not null;
    }

    public static bool ShouldDeferLoopEndTurn(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        return Read(observer, combatCardKnowledge).KeepsCardPlayOpen;
    }

    private static PendingCombatSelection? TryResolvePendingSelection(
        int? selectedCardSlot,
        string? selectedCardTargetType,
        string? selectedCardType,
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        if (!selectedCardSlot.HasValue || selectedCardSlot.Value is < 1 or > 5)
        {
            return null;
        }

        var slotIndex = selectedCardSlot.Value;

        if (IsEnemyTargetType(selectedCardTargetType)
            || string.Equals(selectedCardType, "Attack", StringComparison.OrdinalIgnoreCase))
        {
            return new PendingCombatSelection(slotIndex, AutoCombatCardKind.AttackLike);
        }

        if (IsNonEnemyTargetType(selectedCardTargetType)
            || string.Equals(selectedCardType, "Skill", StringComparison.OrdinalIgnoreCase)
            || string.Equals(selectedCardType, "Power", StringComparison.OrdinalIgnoreCase))
        {
            return new PendingCombatSelection(slotIndex, AutoCombatCardKind.DefendLike);
        }

        var observerCard = observer.CombatHand.FirstOrDefault(card => card.SlotIndex == slotIndex);
        if (observerCard is not null)
        {
            return IsObservedAttackCard(observerCard)
                ? new PendingCombatSelection(slotIndex, AutoCombatCardKind.AttackLike)
                : CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatHandCard(observerCard, observer.PlayerEnergy, combatCardKnowledge)
                    ? new PendingCombatSelection(slotIndex, AutoCombatCardKind.DefendLike)
                    : null;
        }

        var knowledgeCard = combatCardKnowledge.FirstOrDefault(card => card.SlotIndex == slotIndex);
        if (knowledgeCard is not null)
        {
            return IsKnowledgeEnemyTargetCard(knowledgeCard)
                ? new PendingCombatSelection(slotIndex, AutoCombatCardKind.AttackLike)
                : CombatEligibilitySupport.IsPlayableAutoNonEnemyCombatCard(knowledgeCard, observer.PlayerEnergy)
                    ? new PendingCombatSelection(slotIndex, AutoCombatCardKind.DefendLike)
                    : null;
        }

        return null;
    }

    private static bool IsEnemyTargetType(string? targetType)
    {
        return string.Equals(targetType, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "RandomEnemy", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyTargetType(string? targetType)
    {
        return string.Equals(targetType, "Self", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "None", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "AllEnemies", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetType, "AllAllies", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsObservedAttackCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("STRIKE", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("BASH", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsKnowledgeEnemyTargetCard(CombatCardKnowledgeHint card)
    {
        return string.Equals(card.Type, "Attack", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<int> ParseSlotList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<int>();
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(segment => int.TryParse(segment, NumberStyles.Integer, CultureInfo.InvariantCulture, out var slotIndex) ? slotIndex : -1)
            .Where(slotIndex => slotIndex is >= 1 and <= 5)
            .Distinct()
            .OrderBy(static slotIndex => slotIndex)
            .ToArray();
    }

    private static string? TryGetMetaValue(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value) ? value : null;
    }

    private static bool? TryGetMetaBool(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    private static int? TryGetMetaInt(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}

static class CombatEligibilitySupport
{
    public static bool IsCombatPlayerActionWindowClosed(ObserverSummary observer)
    {
        if (TryGetCombatCrossCheckFlag(observer, "CombatManager.IsEnemyTurnStarted", out var enemyTurnStarted)
            && enemyTurnStarted)
        {
            return true;
        }

        if (TryGetCombatCrossCheckFlag(observer, "CombatManager.IsPlayPhase", out var isPlayPhase)
            && !isPlayPhase)
        {
            return true;
        }

        return false;
    }

    public static bool IsPlayableAutoNonEnemyCombatCard(CombatCardKnowledgeHint card, int? energy)
    {
        return IsNonEnemyCombatCard(card)
               && IsAutoNonEnemyPromotionEligible(card)
               && IsPlayableAtCurrentEnergy(card, energy);
    }

    public static bool IsPlayableAutoNonEnemyCombatHandCard(
        ObservedCombatHandCard card,
        int? energy,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        if (!IsNonEnemyCombatHandCard(card) || IsInertCombatHandCard(card))
        {
            return false;
        }

        if (ResolveObservedCombatCardKnowledge(card, combatCardKnowledge) is { } knowledgeCard
            && !IsAutoNonEnemyPromotionEligible(knowledgeCard))
        {
            return false;
        }

        var resolvedCost = ResolveObservedCombatCardCost(card, combatCardKnowledge);
        if (resolvedCost is < 0)
        {
            return false;
        }

        if (energy is null || resolvedCost is null)
        {
            return true;
        }

        return resolvedCost <= energy;
    }

    public static bool HasSelectedNonEnemyConfirmEvidence(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        AutoCombatAnalysis analysis,
        PendingCombatSelection? pendingSelection)
    {
        return CombatRuntimeStateSupport.HasNonEnemyConfirmEvidence(observer, combatCardKnowledge, pendingSelection, analysis);
    }

    public static bool HasSelectedNonEnemyConfirmEvidence(GuiSmokeStepRequest request)
    {
        var pendingSelection = CombatRuntimeStateSupport.ResolvePendingSelection(request.Observer, request.CombatCardKnowledge, CombatHistorySupport.TryGetPendingCombatSelection(request.History));
        var analysis = string.IsNullOrWhiteSpace(request.ScreenshotPath)
            ? new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown)
            : AutoCombatAnalyzer.Analyze(request.ScreenshotPath);
        return HasSelectedNonEnemyConfirmEvidence(request.Observer, request.CombatCardKnowledge, analysis, pendingSelection);
    }

    private static bool TryGetCombatCrossCheckFlag(ObserverSummary observer, string key, out bool value)
    {
        value = false;
        if (observer.Meta.TryGetValue(key, out var directValue)
            && bool.TryParse(directValue, out value))
        {
            return true;
        }

        if (!observer.Meta.TryGetValue("combatCrossCheck", out var combatCrossCheck)
            || string.IsNullOrWhiteSpace(combatCrossCheck))
        {
            return false;
        }

        foreach (var segment in combatCrossCheck.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separatorIndex = segment.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var candidateKey = segment[..separatorIndex].Trim();
            if (!string.Equals(candidateKey, key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return bool.TryParse(segment[(separatorIndex + 1)..].Trim(), out value);
        }

        return false;
    }

    private static bool IsAutoNonEnemyPromotionEligible(CombatCardKnowledgeHint card)
    {
        return !string.Equals(card.Type, "Status", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(card.Type, "Curse", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(card.Target, "None", StringComparison.OrdinalIgnoreCase)
               && card.Cost is not < 0;
    }

    private static bool IsNonEnemyCombatCard(CombatCardKnowledgeHint card)
    {
        if (string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
            || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Equals(card.Type, "Power", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "Self", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "None", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllAllies", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyAlly", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllEnemies", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Skill", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Type, "Power", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("DEFEND", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInertCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Status", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Type, "Curse", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlayableAtCurrentEnergy(CombatCardKnowledgeHint card, int? energy)
    {
        if (energy is null || card.Cost is null)
        {
            return true;
        }

        if (card.Cost < 0)
        {
            return energy > 0;
        }

        return card.Cost <= energy;
    }

    private static int? ResolveObservedCombatCardCost(
        ObservedCombatHandCard card,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        if (card.Cost is not null)
        {
            return card.Cost;
        }

        var slotMatch = combatCardKnowledge.FirstOrDefault(candidate => candidate.SlotIndex == card.SlotIndex);
        if (slotMatch?.Cost is not null)
        {
            return slotMatch.Cost;
        }

        var cardKeys = BuildLookupKeys(card.Name);
        if (cardKeys.Count == 0)
        {
            return null;
        }

        return combatCardKnowledge
            .Where(candidate => candidate.Cost is not null)
            .Where(candidate => BuildLookupKeys(candidate.Name).Any(cardKeys.Contains))
            .Select(static candidate => candidate.Cost)
            .FirstOrDefault();
    }

    private static CombatCardKnowledgeHint? ResolveObservedCombatCardKnowledge(
        ObservedCombatHandCard card,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var slotMatch = combatCardKnowledge.FirstOrDefault(candidate => candidate.SlotIndex == card.SlotIndex);
        if (slotMatch is not null)
        {
            return slotMatch;
        }

        var cardKeys = BuildLookupKeys(card.Name);
        if (cardKeys.Count == 0)
        {
            return null;
        }

        return combatCardKnowledge.FirstOrDefault(candidate =>
            BuildLookupKeys(candidate.Name).Any(cardKeys.Contains));
    }

    private static IReadOnlyList<string> BuildLookupKeys(string? cardName)
    {
        if (string.IsNullOrWhiteSpace(cardName))
        {
            return Array.Empty<string>();
        }

        var keys = new List<string>();
        var normalizedName = NormalizeLookupKey(cardName);
        AddLookupKey(keys, normalizedName);

        var parts = cardName
            .Split(new[] { '.', '_', '-', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeLookupKey)
            .Where(static part => part.Length > 0)
            .ToArray();
        if (parts.Length == 0)
        {
            return keys;
        }

        var trimmedParts = parts[0] == "card"
            ? parts[1..]
            : parts;
        if (trimmedParts.Length == 0)
        {
            return keys;
        }

        AddLookupKey(keys, string.Concat(trimmedParts));
        AddLookupKey(keys, trimmedParts[0]);
        if (trimmedParts.Length > 1 && IsCombatClassSuffix(trimmedParts[^1]))
        {
            AddLookupKey(keys, string.Concat(trimmedParts[..^1]));
        }

        foreach (var part in trimmedParts)
        {
            AddLookupKey(keys, part);
        }

        return keys;
    }

    private static void AddLookupKey(List<string> keys, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate)
            || keys.Contains(candidate, StringComparer.Ordinal))
        {
            return;
        }

        keys.Add(candidate);
    }

    private static bool IsCombatClassSuffix(string value)
    {
        return value is "ironclad" or "silent" or "defect" or "watcher" or "colorless" or "status" or "curse";
    }

    private static string NormalizeLookupKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        Span<char> buffer = stackalloc char[value.Length];
        var length = 0;
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                buffer[length] = char.ToLowerInvariant(character);
                length += 1;
            }
        }

        return length == 0
            ? string.Empty
            : new string(buffer[..length]);
    }
}

static class CombatDecisionContract
{
    public static bool TryMapSemanticAction(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string semanticAction)
    {
        return TryMapSemanticAction(request, decision, out semanticAction, out _);
    }

    public static bool IsAllowed(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string? semanticAction)
    {
        semanticAction = null;
        if (!TryMapSemanticAction(request, decision, out var mappedAction, out var allowLegacyCardAliases))
        {
            return false;
        }

        semanticAction = mappedAction;
        if (request.AllowedActions.Contains(mappedAction, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        return allowLegacyCardAliases
               && (request.AllowedActions.Contains("click card", StringComparer.OrdinalIgnoreCase)
                   || request.AllowedActions.Contains("select card from hand", StringComparer.OrdinalIgnoreCase));
    }

    private static bool TryMapSemanticAction(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string semanticAction,
        out bool allowLegacyCardAliases)
    {
        semanticAction = string.Empty;
        allowLegacyCardAliases = false;

        if (string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase))
        {
            semanticAction = "wait";
            return true;
        }

        if (!string.Equals(request.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(decision.ActionKind, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase))
        {
            if (CombatEligibilitySupport.HasSelectedNonEnemyConfirmEvidence(request))
            {
                semanticAction = "confirm selected non-enemy card";
                return true;
            }

            return false;
        }

        if (string.Equals(decision.ActionKind, "click-current", StringComparison.OrdinalIgnoreCase))
        {
            if (CombatEligibilitySupport.HasSelectedNonEnemyConfirmEvidence(request))
            {
                semanticAction = "confirm selected non-enemy card";
                return true;
            }

            return false;
        }

        if (CombatHistorySupport.TryParsePendingCombatSelection(decision.TargetLabel, out var selection)
            && selection is not null)
        {
            semanticAction = selection.Kind == AutoCombatCardKind.AttackLike
                ? $"select attack slot {selection.SlotIndex}"
                : $"select non-enemy slot {selection.SlotIndex}";
            allowLegacyCardAliases = true;
            return true;
        }

        if (CombatHistorySupport.IsCombatEnemyTargetLabel(decision.TargetLabel))
        {
            semanticAction = "click enemy";
            return true;
        }

        if (CombatHistorySupport.IsCombatEndTurnLabel(decision.TargetLabel)
            || (string.Equals(decision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase)
                && string.Equals(decision.KeyText, "E", StringComparison.OrdinalIgnoreCase)))
        {
            semanticAction = "click end turn";
            return true;
        }

        if (CombatHistorySupport.IsCombatCancelSelectionLabel(decision.TargetLabel)
            || string.Equals(decision.ActionKind, "right-click", StringComparison.OrdinalIgnoreCase))
        {
            semanticAction = "right-click cancel selected card";
            return true;
        }

        if (string.Equals(decision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase)
            && decision.KeyText?.Length == 1
            && char.IsDigit(decision.KeyText[0])
            && decision.KeyText[0] is >= '1' and <= '5')
        {
            semanticAction = $"select attack slot {decision.KeyText[0]}";
            allowLegacyCardAliases = true;
            return true;
        }

        return false;
    }
}

static class AutoCombatAnalyzer
{
    public static AutoCombatAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("combat", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoCombatAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown);
        }

        try
        {
            using var bitmap = new Bitmap(screenshotPath);
            var selectedOverlayBand = FindSelectedOverlay(bitmap);
            var hasSelectedCard = selectedOverlayBand is not AutoCombatOverlayBand.None;
            var hasTargetArrow = HasTargetArrow(bitmap);
            var hasSelfTargetBrackets = HasSelfTargetBrackets(bitmap);
            var selectedCardKind = hasSelectedCard
                ? ClassifySelectedCard(bitmap, selectedOverlayBand)
                : AutoCombatCardKind.Unknown;
            return new AutoCombatAnalysis(hasSelectedCard, selectedOverlayBand, hasTargetArrow, hasSelfTargetBrackets, selectedCardKind);
        }
        catch (ArgumentException)
        {
            return new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown);
        }
        catch (IOException)
        {
            return new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown);
        }
    }

    private static AutoCombatOverlayBand FindSelectedOverlay(Bitmap bitmap)
    {
        var candidates = new[]
        {
            (Band: AutoCombatOverlayBand.Left, Bounds: (Left: 0.12, Top: 0.48, Right: 0.38, Bottom: 0.88)),
            (Band: AutoCombatOverlayBand.Center, Bounds: (Left: 0.38, Top: 0.48, Right: 0.62, Bottom: 0.88)),
            (Band: AutoCombatOverlayBand.Right, Bounds: (Left: 0.62, Top: 0.48, Right: 0.88, Bottom: 0.88)),
        };

        var bestBand = AutoCombatOverlayBand.None;
        var bestScore = 0;
        foreach (var candidate in candidates)
        {
            var cyanPixels = CountMatchingPixels(bitmap, candidate.Bounds.Left, candidate.Bounds.Top, candidate.Bounds.Right, candidate.Bounds.Bottom, color =>
                color.B > 150 && color.G > 130 && color.R < 140);
            var brightNonBackground = CountMatchingPixels(bitmap, candidate.Bounds.Left, candidate.Bounds.Top, candidate.Bounds.Right, candidate.Bounds.Bottom, color =>
                (color.R + color.G + color.B) / 3.0 > 70
                && (Math.Abs(color.R - color.G) > 15 || Math.Abs(color.G - color.B) > 15 || Math.Abs(color.R - color.B) > 15));
            var score = (cyanPixels * 3) + brightNonBackground;
            if (score > bestScore)
            {
                bestScore = score;
                bestBand = candidate.Band;
            }
        }

        return bestScore >= 42 ? bestBand : AutoCombatOverlayBand.None;
    }

    private static bool HasTargetArrow(Bitmap bitmap)
    {
        var grayLikePixels = CountMatchingPixels(bitmap, 0.40, 0.22, 0.60, 0.60, color =>
        {
            var max = Math.Max(color.R, Math.Max(color.G, color.B));
            var min = Math.Min(color.R, Math.Min(color.G, color.B));
            var delta = max - min;
            var brightness = (color.R + color.G + color.B) / 3.0;
            return delta < 20 && brightness is > 90 and < 235;
        });
        return grayLikePixels >= 25;
    }

    private static bool HasSelfTargetBrackets(Bitmap bitmap)
    {
        var yellowPixels = CountMatchingPixels(bitmap, 0.14, 0.34, 0.36, 0.76, color =>
            color.R > 160 && color.G > 125 && color.B < 120);
        return yellowPixels >= 8;
    }

    private static AutoCombatCardKind ClassifySelectedCard(Bitmap bitmap, AutoCombatOverlayBand band)
    {
        var bounds = GetOverlayContentBounds(band);
        var sample = AverageColor(bitmap, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
        if (sample.B > sample.R + 12)
        {
            return AutoCombatCardKind.DefendLike;
        }

        if (sample.R > sample.B + 12)
        {
            return AutoCombatCardKind.AttackLike;
        }

        return AutoCombatCardKind.Unknown;
    }

    private static int CountMatchingPixels(Bitmap bitmap, double left, double top, double right, double bottom, Func<Color, bool> predicate)
    {
        var count = 0;
        ForEachSample(bitmap, left, top, right, bottom, 18, 18, color =>
        {
            if (predicate(color))
            {
                count += 1;
            }
        });
        return count;
    }

    private static (double R, double G, double B, double Brightness) AverageColor(Bitmap bitmap, double left, double top, double right, double bottom)
    {
        var totalR = 0d;
        var totalG = 0d;
        var totalB = 0d;
        var total = 0;
        ForEachSample(bitmap, left, top, right, bottom, 16, 16, color =>
        {
            totalR += color.R;
            totalG += color.G;
            totalB += color.B;
            total += 1;
        });

        if (total == 0)
        {
            return (0, 0, 0, 0);
        }

        var averageR = totalR / total;
        var averageG = totalG / total;
        var averageB = totalB / total;
        return (averageR, averageG, averageB, (averageR + averageG + averageB) / 3.0);
    }

    internal static void ForEachSample(Bitmap bitmap, double left, double top, double right, double bottom, int columns, int rows, Action<Color> visitor)
    {
        ForEachSample(bitmap, left, top, right, bottom, columns, rows, (color, _, _) => visitor(color));
    }

    internal static void ForEachSample(Bitmap bitmap, double left, double top, double right, double bottom, int columns, int rows, Action<Color, int, int> visitor)
    {
        for (var row = 0; row < rows; row += 1)
        {
            var y = (int)Math.Round((bitmap.Height - 1) * Lerp(top, bottom, row / (double)Math.Max(1, rows - 1)));
            for (var column = 0; column < columns; column += 1)
            {
                var x = (int)Math.Round((bitmap.Width - 1) * Lerp(left, right, column / (double)Math.Max(1, columns - 1)));
                visitor(bitmap.GetPixel(x, y), x, y);
            }
        }
    }

    private static double Lerp(double from, double to, double t)
    {
        return from + ((to - from) * t);
    }

    private static (double Left, double Top, double Right, double Bottom) GetOverlayBounds(AutoCombatOverlayBand band)
    {
        return band switch
        {
            AutoCombatOverlayBand.Left => (0.12, 0.48, 0.38, 0.88),
            AutoCombatOverlayBand.Center => (0.38, 0.48, 0.62, 0.88),
            AutoCombatOverlayBand.Right => (0.62, 0.48, 0.88, 0.88),
            _ => (0.38, 0.48, 0.62, 0.88),
        };
    }

    private static (double Left, double Top, double Right, double Bottom) GetOverlayContentBounds(AutoCombatOverlayBand band)
    {
        return band switch
        {
            AutoCombatOverlayBand.Left => (0.16, 0.40, 0.34, 0.82),
            AutoCombatOverlayBand.Center => (0.42, 0.44, 0.58, 0.84),
            AutoCombatOverlayBand.Right => (0.66, 0.36, 0.82, 0.80),
            _ => (0.42, 0.44, 0.58, 0.84),
        };
    }
}

sealed class ArtifactRecorder
{
    private readonly string _runRoot;
    private readonly string _tracePath;
    private readonly string _progressPath;
    private readonly string _manifestPath;
    private readonly string _humanLogPath;

    public ArtifactRecorder(string runRoot)
    {
        _runRoot = runRoot;
        _tracePath = Path.Combine(runRoot, "trace.ndjson");
        _progressPath = Path.Combine(runRoot, "progress.ndjson");
        _manifestPath = Path.Combine(runRoot, "run.json");
        _humanLogPath = Path.Combine(runRoot, "run.log");
    }

    public string RunRoot => _runRoot;

    public void WriteRunManifest(GuiSmokeRunManifest manifest)
    {
        LiveExportAtomicFileWriter.WriteJsonAtomic(_manifestPath, manifest, GuiSmokeShared.JsonOptions);
    }

    public void CompleteRun(string status, string message)
    {
        var manifest = JsonSerializer.Deserialize<GuiSmokeRunManifest>(File.ReadAllText(_manifestPath), GuiSmokeShared.JsonOptions)
                       ?? throw new InvalidOperationException("Failed to reload run manifest.");
        manifest.Status = status;
        manifest.ResultMessage = message;
        manifest.CompletedAt = DateTimeOffset.UtcNow;
        LiveExportAtomicFileWriter.WriteJsonAtomic(_manifestPath, manifest, GuiSmokeShared.JsonOptions);
    }

    public void WriteFailureSummary(GuiSmokeFailureSummary summary)
    {
        LiveExportAtomicFileWriter.WriteJsonAtomic(Path.Combine(_runRoot, "failure-summary.json"), summary, GuiSmokeShared.JsonOptions);
    }

    public void AppendTrace(GuiSmokeTraceEntry entry)
    {
        LiveExportAtomicFileWriter.AppendAllTextShared(
            _tracePath,
            JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions) + Environment.NewLine);
    }

    public void WriteObserverCopies(string stepPrefix, ObserverState observer)
    {
        if (observer.StateDocument is not null)
        {
            File.WriteAllText(stepPrefix + ".observer.state.json", observer.StateDocument.RootElement.GetRawText());
        }

        if (observer.InventoryDocument is not null)
        {
            File.WriteAllText(stepPrefix + ".observer.inventory.json", observer.InventoryDocument.RootElement.GetRawText());
        }

        if (observer.EventLines is { Length: > 0 })
        {
            File.WriteAllText(stepPrefix + ".observer.events.tail.json", JsonSerializer.Serialize(observer.EventLines, GuiSmokeShared.JsonOptions));
        }
    }

    public void WriteRequest(string path, GuiSmokeStepRequest request)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(request, GuiSmokeShared.JsonOptions));
    }

    public void WriteDecision(string path, GuiSmokeStepDecision decision)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(decision, GuiSmokeShared.JsonOptions));
    }

    public void AppendProgress(GuiSmokeStepProgress entry)
    {
        LiveExportAtomicFileWriter.AppendAllTextShared(
            _progressPath,
            JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions) + Environment.NewLine);
    }

    public void WriteSelfMetaReview(GuiSmokeSelfMetaReview review)
    {
        LiveExportAtomicFileWriter.WriteJsonAtomic(Path.Combine(_runRoot, "self-meta-review.json"), review, GuiSmokeShared.JsonOptions);
    }

    public void AppendHumanLog(string line)
    {
        LiveExportAtomicFileWriter.AppendAllTextShared(
            _humanLogPath,
            line + Environment.NewLine);
    }

    public void WriteValidationSummary(string runId)
    {
        var eventCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var sceneCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var totalTraceEntries = 0;

        if (File.Exists(_tracePath))
        {
            foreach (var line in File.ReadLines(_tracePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                GuiSmokeTraceEntry? entry;
                try
                {
                    entry = JsonSerializer.Deserialize<GuiSmokeTraceEntry>(line, GuiSmokeShared.JsonOptions);
                }
                catch (JsonException)
                {
                    continue;
                }

                if (entry is null)
                {
                    continue;
                }

                totalTraceEntries += 1;
                IncrementCounter(eventCounts, entry.EventKind);
                IncrementCounter(sceneCounts, entry.ObserverScreen ?? "unknown");
            }
        }

        var summary = new GuiSmokeValidationSummary(
            runId,
            totalTraceEntries,
            eventCounts,
            sceneCounts);
        LiveExportAtomicFileWriter.WriteJsonAtomic(Path.Combine(_runRoot, "validation-summary.json"), summary, GuiSmokeShared.JsonOptions);
    }

    private static void IncrementCounter(Dictionary<string, int> counters, string key)
    {
        counters[key] = counters.TryGetValue(key, out var current)
            ? current + 1
            : 1;
    }
}

static partial class LongRunArtifacts
{
    public static GuiSmokeSelfMetaReview WriteAttemptMetaReview(
        string sessionRoot,
        string attemptId,
        int attemptOrdinal,
        string runId,
        string status,
        string? resultMessage,
        string? explicitFailureClass = null)
    {
        var currentProgress = ReadNdjson<GuiSmokeStepProgress>(Path.Combine(sessionRoot, "attempts", attemptId, "progress.ndjson"));
        var previousReviews = ReadNdjson<GuiSmokeSelfMetaReview>(Path.Combine(sessionRoot, "meta-reviews.ndjson"));
        var observerCoverageRatio = currentProgress.Count == 0
            ? 0d
            : currentProgress.Count(static entry => entry.ObserverProgress) / (double)currentProgress.Count;
        var actuatorSteps = currentProgress.Count(static entry => entry.ActuatorSignals.Count > 0);
        var actuatorSuccessRatio = actuatorSteps == 0
            ? 0d
            : currentProgress.Count(static entry => entry.ActuatorProgress) / (double)actuatorSteps;
        var novelSceneCount = currentProgress.Count(static entry => entry.FirstSeenScene);
        var sameActionStallCount = currentProgress.Count(entry => entry.ObserverSignals.Contains("same-action-stall", StringComparer.OrdinalIgnoreCase));
        var plateauDetected = DetectProgressPlateau(previousReviews, observerCoverageRatio, actuatorSuccessRatio, novelSceneCount, sameActionStallCount);
        var dominantFailureClass = DetermineDominantFailureClass(currentProgress, status, resultMessage, explicitFailureClass);
        var evidence = BuildReviewEvidence(currentProgress, observerCoverageRatio, actuatorSuccessRatio, novelSceneCount, sameActionStallCount, status, resultMessage);
        var directionRisk = plateauDetected
            ? $"direction-risk:{dominantFailureClass}"
            : "direction-stable";
        var nextAttemptAdjustments = BuildNextAttemptAdjustments(dominantFailureClass, plateauDetected);
        var review = new GuiSmokeSelfMetaReview(
            DateTimeOffset.UtcNow,
            attemptId,
            attemptOrdinal,
            runId,
            status,
            resultMessage,
            plateauDetected,
            dominantFailureClass,
            directionRisk,
            evidence,
            nextAttemptAdjustments,
            observerCoverageRatio,
            actuatorSuccessRatio,
            novelSceneCount,
            sameActionStallCount,
            currentProgress.Count);
        AppendNdjson(Path.Combine(sessionRoot, "meta-reviews.ndjson"), review);
        return review;
    }

    public static void WriteSessionArtifacts(
        string sessionRoot,
        ArtifactRecorder logger,
        string runId,
        string scenarioId,
        string providerKind,
        string attemptId,
        int attemptOrdinal,
        int stepCount,
        string status,
        string resultMessage,
        string? terminalCause,
        bool launchFailed,
        string? failureClass,
        string trustStateAtStart)
    {
        var runManifestPath = Path.Combine(logger.RunRoot, "run.json");
        var runManifest = File.Exists(runManifestPath)
            ? JsonSerializer.Deserialize<GuiSmokeRunManifest>(File.ReadAllText(runManifestPath), GuiSmokeShared.JsonOptions)
            : null;
        var attemptEntry = new GuiSmokeAttemptIndexEntry(
            attemptId,
            attemptOrdinal,
            runId,
            status,
            resultMessage,
            runManifest?.StartedAt ?? DateTimeOffset.UtcNow,
            runManifest?.CompletedAt ?? DateTimeOffset.UtcNow,
            stepCount,
            terminalCause,
            launchFailed,
            failureClass,
            trustStateAtStart);

        var attemptIndexPath = Path.Combine(sessionRoot, "attempt-index.ndjson");
        AppendUniqueNdjson(attemptIndexPath, attemptEntry, static existing => existing.RunId, attemptEntry.RunId);
        LongRunArtifacts.RefreshSessionSummary(sessionRoot);
        RefreshStallSentinel(sessionRoot);
        RefreshSupervisorState(sessionRoot);
    }

    public static void AppendSceneRecipe(string sessionRoot, GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        if (string.IsNullOrWhiteSpace(sessionRoot)
            || string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase)
            || string.Equals(decision.Status, "abort", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(decision.ActionKind))
        {
            return;
        }

        var recipe = new SceneRecipeEntry(
            DateTimeOffset.UtcNow,
            request.SceneSignature,
            request.Phase,
            decision.ActionKind,
            decision.TargetLabel,
            decision.ExpectedScreen,
            decision.Reason,
            request.ScreenshotPath);
        AppendNdjson(Path.Combine(sessionRoot, "scene-recipes.ndjson"), recipe);
    }

    public static void MaybeRecordUnknownScene(string sessionRoot, GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        var observerScreenUnknown = string.Equals(request.Observer.CurrentScreen, "unknown", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(request.Observer.VisibleScreen, "unknown", StringComparison.OrdinalIgnoreCase);
        if (!observerScreenUnknown)
        {
            return;
        }

        var entry = new UnknownSceneEntry(
            DateTimeOffset.UtcNow,
            request.SceneSignature,
            request.Phase,
            request.ScreenshotPath,
            request.Observer.CurrentScreen,
            request.Observer.VisibleScreen,
            decision.AbortReason ?? decision.Reason);
        AppendUniqueNdjson(Path.Combine(sessionRoot, "unknown-scenes.ndjson"), entry, static existing => existing.SceneSignature, entry.SceneSignature);
    }

    private static void AppendNdjson<T>(string path, T entry)
    {
        LiveExportAtomicFileWriter.AppendAllTextShared(
            path,
            JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions) + Environment.NewLine);
    }

    private static void AppendUniqueNdjson<T>(string path, T entry, Func<T, string> keySelector, string key)
    {
        if (File.Exists(path))
        {
            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                T? existing;
                try
                {
                    existing = JsonSerializer.Deserialize<T>(line, GuiSmokeShared.JsonOptions);
                }
                catch (JsonException)
                {
                    continue;
                }

                if (existing is not null && string.Equals(keySelector(existing), key, StringComparison.Ordinal))
                {
                    return;
                }
            }
        }

        AppendNdjson(path, entry);
    }

    private static List<T> ReadNdjson<T>(string path)
    {
        var entries = new List<T>();
        if (!File.Exists(path))
        {
            return entries;
        }

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            T? entry;
            try
            {
                entry = JsonSerializer.Deserialize<T>(line, GuiSmokeShared.JsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (entry is not null)
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

    private static bool DetectProgressPlateau(
        IReadOnlyList<GuiSmokeSelfMetaReview> previousReviews,
        double observerCoverageRatio,
        double actuatorSuccessRatio,
        int novelSceneCount,
        int sameActionStallCount)
    {
        var window = previousReviews.TakeLast(2).ToArray();
        if (window.Length < 2)
        {
            return false;
        }

        var bestObserverCoverage = window.Max(static review => review.ObserverCoverageRatio);
        var bestActuatorSuccess = window.Max(static review => review.ActuatorSuccessRatio);
        var bestNovelSceneCount = window.Max(static review => review.NovelSceneCount);
        var lowestSameActionStallCount = window.Min(static review => review.SameActionStallCount);
        return observerCoverageRatio <= bestObserverCoverage + 0.01
               && actuatorSuccessRatio <= bestActuatorSuccess + 0.01
               && novelSceneCount <= bestNovelSceneCount
               && sameActionStallCount >= lowestSameActionStallCount;
    }

    private static string DetermineDominantFailureClass(
        IReadOnlyList<GuiSmokeStepProgress> progressEntries,
        string status,
        string? resultMessage,
        string? explicitFailureClass = null)
    {
        if (!string.IsNullOrWhiteSpace(explicitFailureClass))
        {
            return explicitFailureClass;
        }

        if (resultMessage?.Contains("launch", StringComparison.OrdinalIgnoreCase) == true
            || progressEntries.Any(entry => entry.ObserverSignals.Contains("black-frame-nudge", StringComparer.OrdinalIgnoreCase)))
        {
            return "launch-runtime-noise";
        }

        var observerBlindspots = progressEntries.Count(entry =>
            entry.Phase == GuiSmokePhase.HandleCombat.ToString()
            && (!entry.ObserverSignals.Contains("combat-energy", StringComparer.OrdinalIgnoreCase)
                || !entry.ObserverSignals.Contains("combat-hand", StringComparer.OrdinalIgnoreCase)));
        var actuatorConfirmFailures = progressEntries.Count(entry =>
            entry.DecisionTargetLabel is not null
            && entry.DecisionTargetLabel.Contains("confirm", StringComparison.OrdinalIgnoreCase)
            && !entry.ActuatorSignals.Contains("non-enemy-confirmed", StringComparer.OrdinalIgnoreCase)
            && !entry.ActuatorSignals.Contains("post-action-delta", StringComparer.OrdinalIgnoreCase));
        var semanticAmbiguity = progressEntries.Count(static entry => entry.SemanticReasoningActive && entry.FirstSeenScene && !entry.ActuatorProgress);
        var screenshotDrift = progressEntries.Count(entry => entry.ObserverSignals.Contains("same-action-stall", StringComparer.OrdinalIgnoreCase));
        if (observerBlindspots >= actuatorConfirmFailures && observerBlindspots >= semanticAmbiguity && observerBlindspots >= screenshotDrift)
        {
            return "observer-blindspot";
        }

        if (actuatorConfirmFailures >= semanticAmbiguity && actuatorConfirmFailures >= screenshotDrift)
        {
            return "actuator-confirm-failure";
        }

        if (semanticAmbiguity >= screenshotDrift)
        {
            return "semantic-scene-ambiguity";
        }

        if (string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase))
        {
            return "screenshot-heuristic-drift";
        }

        return "observer-blindspot";
    }

    private static IReadOnlyList<string> BuildReviewEvidence(
        IReadOnlyList<GuiSmokeStepProgress> progressEntries,
        double observerCoverageRatio,
        double actuatorSuccessRatio,
        int novelSceneCount,
        int sameActionStallCount,
        string status,
        string? resultMessage)
    {
        return new[]
        {
            $"observer-coverage:{observerCoverageRatio:0.000}",
            $"actuator-success:{actuatorSuccessRatio:0.000}",
            $"novel-scenes:{novelSceneCount}",
            $"same-action-stalls:{sameActionStallCount}",
            $"status:{status}",
            $"result:{resultMessage ?? "null"}",
            $"semantic-steps:{progressEntries.Count(static entry => entry.SemanticReasoningActive)}",
        };
    }

    private static IReadOnlyList<string> BuildNextAttemptAdjustments(string dominantFailureClass, bool plateauDetected)
    {
        var adjustments = new List<string>();
        if (plateauDetected)
        {
            adjustments.Add("run self-meta review before trusting the next direction change");
        }

        switch (dominantFailureClass)
        {
            case "observer-blindspot":
                adjustments.Add("prioritize decompiled-source observer candidates for missing combat or room state");
                adjustments.Add("treat screenshot as authority while expanding observer coverage");
                break;
            case "actuator-confirm-failure":
                adjustments.Add("tighten non-enemy confirm success checks using hand or energy delta");
                adjustments.Add("prefer low-risk repeatable confirmations over aggressive extra clicks");
                break;
            case "semantic-scene-ambiguity":
                adjustments.Add("expand semantic event candidates from assistant/events.json and observed choices");
                adjustments.Add("prefer safe progress options over speculative rare outcomes");
                break;
            case "launch-runtime-noise":
                adjustments.Add("re-verify focus and deployment identity before debugging gameplay behavior");
                break;
            default:
                adjustments.Add("review screenshot-first heuristics for drift and repeated stalls");
                break;
        }

        return adjustments;
    }
}

sealed record AssistantCardKnowledge(
    string Id,
    string Name,
    string? Type,
    string? Target,
    int? Cost,
    IReadOnlyList<string> MatchKeys);

sealed record AssistantEventKnowledge(
    string Id,
    string Title,
    IReadOnlyList<AssistantEventOptionKnowledge> Options);

sealed record AssistantEventOptionKnowledge(
    string Label,
    string? Description,
    string? OptionKey);

static class AssistantKnowledgeCatalog
{
    private static readonly object Sync = new();
    private static string? _workspaceRoot;
    private static IReadOnlyList<AssistantCardKnowledge>? _cards;
    private static IReadOnlyList<AssistantEventKnowledge>? _events;

    public static IReadOnlyList<AssistantCardKnowledge> LoadCards(string workspaceRoot)
    {
        lock (Sync)
        {
            EnsureLoaded(workspaceRoot);
            return _cards ?? Array.Empty<AssistantCardKnowledge>();
        }
    }

    public static IReadOnlyList<AssistantEventKnowledge> LoadEvents(string workspaceRoot)
    {
        lock (Sync)
        {
            EnsureLoaded(workspaceRoot);
            return _events ?? Array.Empty<AssistantEventKnowledge>();
        }
    }

    private static void EnsureLoaded(string workspaceRoot)
    {
        if (string.Equals(_workspaceRoot, workspaceRoot, StringComparison.Ordinal) && _cards is not null && _events is not null)
        {
            return;
        }

        _workspaceRoot = workspaceRoot;
        _cards = LoadCardsCore(Path.Combine(workspaceRoot, "artifacts", "knowledge", "assistant", "cards.json"));
        _events = LoadEventsCore(Path.Combine(workspaceRoot, "artifacts", "knowledge", "assistant", "events.json"));
    }

    private static IReadOnlyList<AssistantCardKnowledge> LoadCardsCore(string path)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<AssistantCardKnowledge>();
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<AssistantCardKnowledge>();
            }

            var cards = new List<AssistantCardKnowledge>();
            foreach (var element in document.RootElement.EnumerateArray())
            {
                var id = TryReadString(element, "id");
                var name = TryReadString(element, "name") ?? TryReadString(element, "title");
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                cards.Add(new AssistantCardKnowledge(
                    id,
                    name,
                    TryReadString(element, "type"),
                    TryReadString(element, "target"),
                    TryReadInt32(element, "cost"),
                    BuildCardMatchKeys(element, name)));
            }

            return cards;
        }
        catch
        {
            return Array.Empty<AssistantCardKnowledge>();
        }
    }

    private static IReadOnlyList<AssistantEventKnowledge> LoadEventsCore(string path)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<AssistantEventKnowledge>();
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<AssistantEventKnowledge>();
            }

            var events = new List<AssistantEventKnowledge>();
            foreach (var element in document.RootElement.EnumerateArray())
            {
                var id = TryReadString(element, "id");
                var title = TryReadString(element, "title") ?? TryReadString(element, "name");
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                var options = new List<AssistantEventOptionKnowledge>();
                if (element.TryGetProperty("options", out var optionsElement) && optionsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var option in optionsElement.EnumerateArray())
                    {
                        var label = TryReadString(option, "label");
                        if (string.IsNullOrWhiteSpace(label))
                        {
                            continue;
                        }

                        options.Add(new AssistantEventOptionKnowledge(
                            label,
                            TryReadString(option, "description"),
                            TryReadNestedString(option, "attributes", "optionKey")));
                    }
                }

                events.Add(new AssistantEventKnowledge(id, title, options));
            }

            return events;
        }
        catch
        {
            return Array.Empty<AssistantEventKnowledge>();
        }
    }

    private static IReadOnlyList<string> BuildCardMatchKeys(JsonElement element, string name)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal)
        {
            NormalizeKey(name),
        };

        var title = TryReadString(element, "title");
        if (!string.IsNullOrWhiteSpace(title))
        {
            keys.Add(NormalizeKey(title));
        }

        var englishTitle = TryReadString(element, "englishTitle");
        if (!string.IsNullOrWhiteSpace(englishTitle))
        {
            keys.Add(NormalizeKey(englishTitle));
        }

        var classId = TryReadString(element, "classId");
        if (!string.IsNullOrWhiteSpace(classId))
        {
            keys.Add(NormalizeKey(classId));
        }

        return keys.Where(static key => key.Length > 0).ToArray();
    }

    private static string NormalizeKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var buffer = new char[value.Length];
        var length = 0;
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                buffer[length] = char.ToLowerInvariant(character);
                length += 1;
            }
        }

        return length == 0
            ? string.Empty
            : new string(buffer, 0, length);
    }

    private static string? TryReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static string? TryReadNestedString(JsonElement element, string objectPropertyName, string propertyName)
    {
        return element.TryGetProperty(objectPropertyName, out var objectProperty)
               && objectProperty.ValueKind == JsonValueKind.Object
               && objectProperty.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static int? TryReadInt32(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetInt32(out var numeric) => numeric,
            JsonValueKind.String when int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null,
        };
    }
}

static class AutoCombatHandAnalyzer
{
    private static readonly (int SlotIndex, double Left, double Top, double Right, double Bottom, double CenterX, double CenterY)[] SlotRegions =
    {
        (1, 0.18, 0.76, 0.32, 0.96, 0.25, 0.875),
        (2, 0.29, 0.75, 0.43, 0.96, 0.36, 0.865),
        (3, 0.42, 0.74, 0.56, 0.96, 0.49, 0.855),
        (4, 0.55, 0.75, 0.69, 0.96, 0.62, 0.865),
        (5, 0.68, 0.76, 0.82, 0.97, 0.75, 0.875),
    };

    public static AutoCombatHandAnalysis Analyze(string screenshotPath)
    {
        return GuiSmokeScreenshotAnalysisCache.GetOrCreate("combat-hand", screenshotPath, () => AnalyzeCore(screenshotPath));
    }

    private static AutoCombatHandAnalysis AnalyzeCore(string screenshotPath)
    {
        if (!File.Exists(screenshotPath))
        {
            return new AutoCombatHandAnalysis(Array.Empty<AutoCombatHandSlotAnalysis>());
        }

        try
        {
            using var bitmap = new Bitmap(screenshotPath);
            var slots = SlotRegions
                .Select(region =>
                {
                    var sample = AverageColor(bitmap, region.Left, region.Top, region.Right, region.Bottom);
                    var isVisible = sample.Brightness >= 42 && (sample.R + sample.G + sample.B) >= 140;
                    var redBlueDelta = sample.R - sample.B;
                    var kind = !isVisible
                        ? AutoCombatCardKind.Unknown
                        : redBlueDelta >= 20
                            ? AutoCombatCardKind.AttackLike
                            : AutoCombatCardKind.DefendLike;
                    return new AutoCombatHandSlotAnalysis(
                        region.SlotIndex,
                        isVisible,
                        kind,
                        redBlueDelta,
                        sample.Brightness,
                        region.CenterX,
                        region.CenterY);
                })
                .ToArray();
            return new AutoCombatHandAnalysis(slots);
        }
        catch (ArgumentException)
        {
            return new AutoCombatHandAnalysis(Array.Empty<AutoCombatHandSlotAnalysis>());
        }
        catch (IOException)
        {
            return new AutoCombatHandAnalysis(Array.Empty<AutoCombatHandSlotAnalysis>());
        }
    }

    private static (double R, double G, double B, double Brightness) AverageColor(Bitmap bitmap, double left, double top, double right, double bottom)
    {
        var totalR = 0d;
        var totalG = 0d;
        var totalB = 0d;
        var total = 0;
        AutoCombatAnalyzer.ForEachSample(bitmap, left, top, right, bottom, 14, 14, color =>
        {
            totalR += color.R;
            totalG += color.G;
            totalB += color.B;
            total += 1;
        });

        if (total == 0)
        {
            return (0, 0, 0, 0);
        }

        var averageR = totalR / total;
        var averageG = totalG / total;
        var averageB = totalB / total;
        return (averageR, averageG, averageB, (averageR + averageG + averageB) / 3.0);
    }
}

sealed class ObserverSnapshotReader
{
    private readonly LiveExportLayout _liveLayout;
    private readonly HarnessQueueLayout _harnessLayout;

    public ObserverSnapshotReader(LiveExportLayout liveLayout, HarnessQueueLayout harnessLayout)
    {
        _liveLayout = liveLayout;
        _harnessLayout = harnessLayout;
    }

    public ObserverState Read()
    {
        JsonDocument? stateDocument = TryReadJson(_liveLayout.SnapshotPath);
        JsonDocument? inventoryDocument = TryReadJson(_harnessLayout.InventoryPath);
        var eventLines = TryReadTail(_liveLayout.EventsPath, 10);

        var inventorySceneType = TryReadString(inventoryDocument?.RootElement, "sceneType");
        var currentScreen = TryReadString(stateDocument?.RootElement, "currentScreen")
                            ?? TryReadNestedString(stateDocument?.RootElement, "meta", "screen")
                            ?? TryReadNestedString(stateDocument?.RootElement, "meta", "logicalScreen")
                            ?? TryReadNestedString(stateDocument?.RootElement, "meta", "flowScreen")
                            ?? inventorySceneType;
        var visibleScreen = TryReadNestedString(stateDocument?.RootElement, "meta", "visibleScreen")
                            ?? currentScreen
                            ?? inventorySceneType;
        var inCombat = TryReadBool(stateDocument?.RootElement, "encounter", "inCombat");
        var capturedAt = TryReadDateTimeOffset(stateDocument?.RootElement, "capturedAt")
                         ?? TryReadDateTimeOffset(inventoryDocument?.RootElement, "capturedAt");
        var inventoryId = inventoryDocument is null
            ? null
            : TryReadString(inventoryDocument.RootElement, "inventoryId");
        var sceneReady = TryReadNestedBool(stateDocument?.RootElement, "meta", "sceneReady")
                         ?? TryReadBool(inventoryDocument?.RootElement, "sceneReady");
        var sceneAuthority = TryReadNestedString(stateDocument?.RootElement, "meta", "sceneAuthority")
                             ?? TryReadString(inventoryDocument?.RootElement, "sceneAuthority");
        var sceneStability = TryReadNestedString(stateDocument?.RootElement, "meta", "sceneStability")
                             ?? TryReadString(inventoryDocument?.RootElement, "sceneStability");
        var sceneEpisodeId = TryReadString(inventoryDocument?.RootElement, "sceneEpisodeId")
                             ?? TryReadNestedString(stateDocument?.RootElement, "meta", "screen-episode");
        var encounterKind = TryReadNestedString(stateDocument?.RootElement, "encounter", "kind");
        var choiceExtractorPath = TryReadNestedString(stateDocument?.RootElement, "meta", "choiceExtractorPath");
        var playerCurrentHp = TryReadInt32(stateDocument?.RootElement, "player", "currentHp");
        var playerMaxHp = TryReadInt32(stateDocument?.RootElement, "player", "maxHp");
        var playerEnergy = TryReadInt32(stateDocument?.RootElement, "player", "energy");
        var combatHand = ParseCombatHandSummary(TryReadNestedString(stateDocument?.RootElement, "meta", "combatHandSummary"));
        var currentChoices = ReadChoiceLabels(stateDocument);
        var meta = ReadMetaDictionary(stateDocument);

        return new ObserverState(
            new ObserverSummary(currentScreen, visibleScreen, inCombat, capturedAt, inventoryId, sceneReady, sceneAuthority, sceneStability, sceneEpisodeId, encounterKind, choiceExtractorPath, playerCurrentHp, playerMaxHp, playerEnergy, currentChoices, eventLines ?? Array.Empty<string>(), ReadActionNodes(inventoryDocument), ReadChoices(stateDocument), combatHand)
            {
                Meta = meta,
            },
            stateDocument,
            inventoryDocument,
            eventLines);
    }

    public static IReadOnlyList<ObserverChoice> ParseChoicesForTesting(JsonDocument? document)
    {
        return ReadChoices(document);
    }

    public static IReadOnlyDictionary<string, string?> ParseMetaForTesting(JsonDocument? document)
    {
        return ReadMetaDictionary(document);
    }

    private static JsonDocument? TryReadJson(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return JsonDocument.Parse(stream);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string[]? TryReadTail(string path, int lines)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream);
            var queue = new Queue<string>(Math.Max(1, lines));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line is null)
                {
                    continue;
                }

                queue.Enqueue(line);
                while (queue.Count > lines)
                {
                    queue.Dequeue();
                }
            }

            return queue.ToArray();
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static IReadOnlyList<string> ReadChoiceLabels(JsonDocument? document)
    {
        if (document is null
            || !document.RootElement.TryGetProperty("currentChoices", out var choicesElement)
            || choicesElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        var labels = new List<string>();
        foreach (var choice in choicesElement.EnumerateArray())
        {
            if (choice.ValueKind == JsonValueKind.String)
            {
                labels.Add(choice.GetString() ?? string.Empty);
                continue;
            }

            if (choice.ValueKind == JsonValueKind.Object && choice.TryGetProperty("label", out var labelElement))
            {
                labels.Add(labelElement.GetString() ?? string.Empty);
            }
        }

        return labels;
    }

    private static IReadOnlyList<ObserverActionNode> ReadActionNodes(JsonDocument? document)
    {
        if (document is null
            || !document.RootElement.TryGetProperty("nodes", out var nodesElement)
            || nodesElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ObserverActionNode>();
        }

        var nodes = new List<ObserverActionNode>();
        foreach (var node in nodesElement.EnumerateArray())
        {
            if (node.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var nodeId = TryReadString(node, "nodeId");
            var kind = TryReadString(node, "kind");
            var label = TryReadString(node, "label");
            var screenBounds = TryReadString(node, "screenBounds");
            var actionable = node.TryGetProperty("actionable", out var actionableElement)
                             && actionableElement.ValueKind == JsonValueKind.True;
            if (string.IsNullOrWhiteSpace(nodeId)
                || string.IsNullOrWhiteSpace(kind)
                || string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            nodes.Add(new ObserverActionNode(nodeId, kind, label, screenBounds, actionable));
        }

        return nodes;
    }

    private static IReadOnlyList<ObserverChoice> ReadChoices(JsonDocument? document)
    {
        if (document is null
            || !document.RootElement.TryGetProperty("currentChoices", out var choicesElement)
            || choicesElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ObserverChoice>();
        }

        var choices = new List<ObserverChoice>();
        foreach (var choice in choicesElement.EnumerateArray())
        {
            if (choice.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var kind = TryReadString(choice, "kind") ?? "choice";
            var label = TryReadString(choice, "label");
            var screenBounds = TryReadString(choice, "screenBounds");
            var value = TryReadString(choice, "value");
            var description = TryReadString(choice, "description");
            var nodeId = TryReadString(choice, "nodeId");
            var bindingKind = TryReadString(choice, "bindingKind");
            var bindingId = TryReadString(choice, "bindingId");
            var enabled = TryReadBool(choice, "enabled");
            var iconAssetPath = TryReadString(choice, "iconAssetPath");
            var semanticHints = TryReadStringArray(choice, "semanticHints");
            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            choices.Add(new ObserverChoice(kind, label, screenBounds, value, description)
            {
                NodeId = nodeId,
                BindingKind = bindingKind,
                BindingId = bindingId,
                Enabled = enabled,
                IconAssetPath = iconAssetPath,
                SemanticHints = semanticHints,
            });
        }

        return choices;
    }

    private static IReadOnlyDictionary<string, string?> ReadMetaDictionary(JsonDocument? document)
    {
        if (document is null
            || !document.RootElement.TryGetProperty("meta", out var metaElement)
            || metaElement.ValueKind != JsonValueKind.Object)
        {
            return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }

        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in metaElement.EnumerateObject())
        {
            values[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Number => property.Value.GetRawText(),
                JsonValueKind.Null => null,
                _ => property.Value.GetRawText(),
            };
        }

        return values;
    }

    private static IReadOnlyList<ObservedCombatHandCard> ParseCombatHandSummary(string? summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return Array.Empty<ObservedCombatHandCard>();
        }

        var cards = new List<ObservedCombatHandCard>();
        foreach (var part in summary.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var colonIndex = part.IndexOf(':');
            if (colonIndex <= 0
                || !int.TryParse(part[..colonIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out var slotIndex))
            {
                continue;
            }

            var payload = part[(colonIndex + 1)..].Split('|', StringSplitOptions.None);
            if (payload.Length == 0 || string.IsNullOrWhiteSpace(payload[0]))
            {
                continue;
            }

            int? cost = payload.Length >= 3 && int.TryParse(payload[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedCost)
                ? parsedCost
                : null;
            cards.Add(new ObservedCombatHandCard(
                slotIndex,
                payload[0],
                payload.Length >= 2 ? payload[1] : null,
                cost));
        }

        return cards;
    }

    private static string? TryReadString(JsonElement? element, string propertyName)
    {
        return element.HasValue
               && element.Value.ValueKind == JsonValueKind.Object
               && element.Value.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static IReadOnlyList<string> TryReadStringArray(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return property
            .EnumerateArray()
            .Where(static item => item.ValueKind == JsonValueKind.String)
            .Select(static item => item.GetString())
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Cast<string>()
            .ToArray();
    }

    private static string? TryReadNestedString(JsonElement? element, string objectPropertyName, string stringPropertyName)
    {
        return element.HasValue
               && element.Value.ValueKind == JsonValueKind.Object
               && element.Value.TryGetProperty(objectPropertyName, out var objectProperty)
               && objectProperty.ValueKind == JsonValueKind.Object
               && objectProperty.TryGetProperty(stringPropertyName, out var stringProperty)
               && stringProperty.ValueKind == JsonValueKind.String
            ? stringProperty.GetString()
            : null;
    }

    private static bool? TryReadNestedBool(JsonElement? element, string objectPropertyName, string boolPropertyName)
    {
        if (!element.HasValue
            || element.Value.ValueKind != JsonValueKind.Object
            || !element.Value.TryGetProperty(objectPropertyName, out var objectProperty)
            || objectProperty.ValueKind != JsonValueKind.Object
            || !objectProperty.TryGetProperty(boolPropertyName, out var boolProperty))
        {
            return null;
        }

        return boolProperty.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(boolProperty.GetString(), out var parsed) => parsed,
            _ => null,
        };
    }

    private static bool? TryReadBool(JsonElement? element, string objectPropertyName, string boolPropertyName)
    {
        if (!element.HasValue
            || element.Value.ValueKind != JsonValueKind.Object
            || !element.Value.TryGetProperty(objectPropertyName, out var objectProperty)
            || objectProperty.ValueKind != JsonValueKind.Object
            || !objectProperty.TryGetProperty(boolPropertyName, out var boolProperty))
        {
            return null;
        }

        return boolProperty.ValueKind == JsonValueKind.True
            ? true
            : boolProperty.ValueKind == JsonValueKind.False
                ? false
                : null;
    }

    private static bool? TryReadBool(JsonElement? element, string propertyName)
    {
        if (!element.HasValue
            || element.Value.ValueKind != JsonValueKind.Object
            || !element.Value.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(property.GetString(), out var parsed) => parsed,
            _ => null,
        };
    }

    private static int? TryReadInt32(JsonElement? element, string objectPropertyName, string intPropertyName)
    {
        if (!element.HasValue
            || element.Value.ValueKind != JsonValueKind.Object
            || !element.Value.TryGetProperty(objectPropertyName, out var objectProperty)
            || objectProperty.ValueKind != JsonValueKind.Object
            || !objectProperty.TryGetProperty(intPropertyName, out var intProperty))
        {
            return null;
        }

        return intProperty.ValueKind switch
        {
            JsonValueKind.Number when intProperty.TryGetInt32(out var numericValue) => numericValue,
            JsonValueKind.String when int.TryParse(intProperty.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stringValue) => stringValue,
            _ => null,
        };
    }

    private static DateTimeOffset? TryReadDateTimeOffset(JsonElement? element, string propertyName)
    {
        return element.HasValue
               && element.Value.ValueKind == JsonValueKind.Object
               && element.Value.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.String
               && DateTimeOffset.TryParse(property.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : null;
    }
}

sealed class ObserverAcceptanceEvaluator
{
    public bool IsPhaseSatisfied(GuiSmokePhase phase, ObserverState observer)
    {
        var sceneReady = observer.SceneReady != false;
        return phase switch
        {
            GuiSmokePhase.WaitMainMenu => sceneReady && string.Equals(observer.CurrentScreen, "main-menu", StringComparison.OrdinalIgnoreCase),
            GuiSmokePhase.WaitCharacterSelect => sceneReady && string.Equals(observer.CurrentScreen, "character-select", StringComparison.OrdinalIgnoreCase),
            GuiSmokePhase.WaitMap => sceneReady && string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase),
            GuiSmokePhase.WaitCombat => string.Equals(observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase)
                && sceneReady
                && observer.InCombat == true,
            GuiSmokePhase.HandleCombat => false,
            _ => false,
        };
    }
}

sealed class ScreenCaptureService
{
    public bool TryCapture(WindowCaptureTarget target, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        using var bitmap = TryCaptureProcessWindow(target);
        if (bitmap is null)
        {
            return false;
        }

        bitmap.Save(outputPath, ImageFormat.Png);
        return true;
    }

    private static Bitmap? TryCaptureProcessWindow(WindowCaptureTarget target)
    {
        if (target.IsFallback || target.Handle == IntPtr.Zero)
        {
            return null;
        }

        Bitmap? lastCapture = null;
        for (var attempt = 0; attempt < 5; attempt += 1)
        {
            target = WindowLocator.EnsureInteractive(target);
            lastCapture?.Dispose();
            lastCapture = TryCaptureWindowClient(target);
            if (lastCapture is not null && !IsMostlyBlack(lastCapture))
            {
                return lastCapture;
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        lastCapture?.Dispose();
        return null;
    }

    private static Bitmap? TryCaptureWindowClient(WindowCaptureTarget target)
    {
        if (target.IsFallback || target.Handle == IntPtr.Zero)
        {
            return null;
        }

        return NativeMethods.TryCaptureClientArea(target.Handle, target.Bounds, out var bitmap)
            ? bitmap
            : null;
    }

    private static bool IsMostlyBlack(Bitmap bitmap)
    {
        var sampleColumns = 12;
        var sampleRows = 8;
        var brightSamples = 0;
        var totalSamples = 0;

        for (var row = 0; row < sampleRows; row += 1)
        {
            var y = (int)Math.Round((bitmap.Height - 1) * (row / (double)Math.Max(1, sampleRows - 1)));
            for (var column = 0; column < sampleColumns; column += 1)
            {
                var x = (int)Math.Round((bitmap.Width - 1) * (column / (double)Math.Max(1, sampleColumns - 1)));
                var pixel = bitmap.GetPixel(x, y);
                var brightness = (pixel.R + pixel.G + pixel.B) / 3.0;
                if (brightness >= 12)
                {
                    brightSamples += 1;
                }

                totalSamples += 1;
            }
        }

        return brightSamples <= Math.Max(1, totalSamples / 20);
    }
}

sealed class MouseInputDriver
{
    private static void PrepareWindow(WindowCaptureTarget target, int delayMs)
    {
        if (target.Handle != IntPtr.Zero)
        {
            NativeMethods.SetForegroundWindow(target.Handle);
            Thread.Sleep(delayMs);
        }
    }

    public void MoveCursor(WindowCaptureTarget target, double normalizedX, double normalizedY)
    {
        PrepareWindow(target, 100);

        var point = TransformNormalizedPoint(target, normalizedX, normalizedY);
        NativeMethods.SetCursorPos(point.X, point.Y);
    }

    public void Click(WindowCaptureTarget target, double normalizedX, double normalizedY)
    {
        PrepareWindow(target, 200);

        var point = TransformNormalizedPoint(target, normalizedX, normalizedY);
        NativeMethods.SetCursorPos(point.X, point.Y);
        var inputs = new[]
        {
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTDOWN),
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTUP),
        };
        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException("Failed to send mouse input.");
        }
    }

    public void ClickCurrent(WindowCaptureTarget target)
    {
        PrepareWindow(target, 200);

        var inputs = new[]
        {
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTDOWN),
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTUP),
        };
        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException("Failed to send current-cursor mouse input.");
        }
    }

    public void ConfirmNonEnemy(WindowCaptureTarget target, double normalizedX, double normalizedY, int holdMs)
    {
        PrepareWindow(target, 200);
        var point = TransformNormalizedPoint(target, normalizedX, normalizedY);
        NativeMethods.SetCursorPos(point.X, point.Y);

        var down = new[]
        {
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTDOWN),
        };
        var sentDown = NativeMethods.SendInput((uint)down.Length, down, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sentDown != down.Length)
        {
            throw new InvalidOperationException("Failed to send non-enemy confirm mouse-down input.");
        }

        Thread.Sleep(Math.Max(holdMs, 16));

        var up = new[]
        {
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTUP),
        };
        var sentUp = NativeMethods.SendInput((uint)up.Length, up, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sentUp != up.Length)
        {
            throw new InvalidOperationException("Failed to send non-enemy confirm mouse-up input.");
        }
    }

    public void RightClick(WindowCaptureTarget target, double normalizedX, double normalizedY)
    {
        PrepareWindow(target, 200);

        var point = TransformNormalizedPoint(target, normalizedX, normalizedY);
        NativeMethods.SetCursorPos(point.X, point.Y);
        var inputs = new[]
        {
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_RIGHTDOWN),
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_RIGHTUP),
        };
        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException("Failed to send right-click mouse input.");
        }
    }

    public static Point TransformNormalizedPoint(WindowCaptureTarget target, double normalizedX, double normalizedY)
    {
        var clampedX = Math.Clamp(normalizedX, 0d, 1d);
        var clampedY = Math.Clamp(normalizedY, 0d, 1d);
        var pixelX = target.Bounds.X + (int)Math.Round((target.Bounds.Width - 1) * clampedX);
        var pixelY = target.Bounds.Y + (int)Math.Round((target.Bounds.Height - 1) * clampedY);
        return new Point(pixelX, pixelY);
    }

    public void PressKey(WindowCaptureTarget target, string keyText)
    {
        PrepareWindow(target, 200);

        var virtualKey = ResolveVirtualKey(keyText);
        var inputs = new[]
        {
            NativeMethods.CreateKeyboardInput(virtualKey, 0),
            NativeMethods.CreateKeyboardInput(virtualKey, NativeMethods.KEYEVENTF_KEYUP),
        };
        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException($"Failed to send keyboard input for key '{keyText}'.");
        }
    }

    private static ushort ResolveVirtualKey(string keyText)
    {
        var trimmed = keyText.Trim();
        if (string.Equals(trimmed, "Escape", StringComparison.OrdinalIgnoreCase)
            || string.Equals(trimmed, "Esc", StringComparison.OrdinalIgnoreCase))
        {
            return NativeMethods.VK_ESCAPE;
        }

        if (string.Equals(trimmed, "Enter", StringComparison.OrdinalIgnoreCase))
        {
            return NativeMethods.VK_RETURN;
        }

        if (string.Equals(trimmed, "Space", StringComparison.OrdinalIgnoreCase))
        {
            return NativeMethods.VK_SPACE;
        }

        if (trimmed.Length != 1)
        {
            throw new InvalidOperationException($"Unsupported keyText '{keyText}'.");
        }

        var virtualKey = NativeMethods.VkKeyScan(trimmed[0]);
        if (virtualKey == -1)
        {
            throw new InvalidOperationException($"Unable to resolve keyText '{keyText}' to a virtual key.");
        }

        return (ushort)(virtualKey & 0xFF);
    }
}

sealed record WindowCaptureTarget(
    IntPtr Handle,
    string Title,
    Rectangle Bounds,
    bool IsFallback,
    bool IsMinimized);

static class WindowLocator
{
    public static WindowCaptureTarget? TryFindSts2Window()
    {
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                if (process.MainWindowHandle == IntPtr.Zero
                    || string.IsNullOrWhiteSpace(process.MainWindowTitle)
                    || process.HasExited)
                {
                    continue;
                }

                if (!process.MainWindowTitle.Contains("Slay the Spire 2", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!NativeMethods.TryGetClientBounds(process.MainWindowHandle, out var bounds))
                {
                    continue;
                }

                return new WindowCaptureTarget(
                    process.MainWindowHandle,
                    process.MainWindowTitle,
                    bounds,
                    false,
                    NativeMethods.IsIconic(process.MainWindowHandle));
            }
            catch
            {
            }
        }

        return null;
    }

    public static WindowCaptureTarget GetPrimaryMonitorFallback()
    {
        var bounds = SystemInformation.VirtualScreen;
        return new WindowCaptureTarget(IntPtr.Zero, "virtual-screen-fallback", bounds, true, false);
    }

    public static WindowCaptureTarget Refresh(WindowCaptureTarget target)
    {
        if (target.IsFallback || target.Handle == IntPtr.Zero)
        {
            return GetPrimaryMonitorFallback();
        }

        if (NativeMethods.TryGetClientBounds(target.Handle, out var bounds))
        {
            return target with
            {
                Bounds = bounds,
                IsMinimized = NativeMethods.IsIconic(target.Handle),
            };
        }

        return target;
    }

    public static WindowCaptureTarget EnsureRestored(WindowCaptureTarget target)
    {
        if (target.IsFallback || target.Handle == IntPtr.Zero || !target.IsMinimized)
        {
            return target;
        }

        NativeMethods.ShowWindow(target.Handle, NativeMethods.SW_RESTORE);
        Thread.Sleep(300);
        return Refresh(target);
    }

    public static WindowCaptureTarget EnsureInteractive(WindowCaptureTarget target)
    {
        if (target.IsFallback || target.Handle == IntPtr.Zero)
        {
            return target;
        }

        target = EnsureRestored(target);
        NativeMethods.ShowWindow(target.Handle, NativeMethods.SW_RESTORE);
        NativeMethods.BringWindowToTop(target.Handle);
        NativeMethods.SetForegroundWindow(target.Handle);
        Thread.Sleep(500);
        return Refresh(target);
    }

    public static bool HasMeaningfulDrift(WindowCaptureTarget before, WindowCaptureTarget after, int tolerancePixels = 8)
    {
        return Math.Abs(before.Bounds.X - after.Bounds.X) > tolerancePixels
               || Math.Abs(before.Bounds.Y - after.Bounds.Y) > tolerancePixels
               || Math.Abs(before.Bounds.Width - after.Bounds.Width) > tolerancePixels
               || Math.Abs(before.Bounds.Height - after.Bounds.Height) > tolerancePixels;
    }
}

static class NativeMethods
{
    public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    public const uint MOUSEEVENTF_LEFTUP = 0x0004;
    public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    public const uint KEYEVENTF_KEYUP = 0x0002;
    public const ushort VK_ESCAPE = 0x1B;
    public const ushort VK_RETURN = 0x0D;
    public const ushort VK_SPACE = 0x20;
    private const uint INPUT_MOUSE = 0;
    private const uint INPUT_KEYBOARD = 1;
    private const uint PW_RENDERFULLCONTENT = 0x00000002;
    public const int SW_RESTORE = 9;

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern short VkKeyScan(char ch);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    public static bool TryGetClientBounds(IntPtr handle, out Rectangle bounds)
    {
        bounds = Rectangle.Empty;
        if (!GetClientRect(handle, out var rect))
        {
            return false;
        }

        var topLeft = new POINT { X = rect.Left, Y = rect.Top };
        var bottomRight = new POINT { X = rect.Right, Y = rect.Bottom };
        if (!ClientToScreen(handle, ref topLeft) || !ClientToScreen(handle, ref bottomRight))
        {
            return false;
        }

        bounds = Rectangle.FromLTRB(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
        return bounds.Width > 0 && bounds.Height > 0;
    }

    public static bool TryCaptureClientArea(IntPtr handle, Rectangle clientBounds, out Bitmap bitmap)
    {
        bitmap = null!;
        if (!GetWindowRect(handle, out var windowRect))
        {
            return false;
        }

        var fullWindowBounds = Rectangle.FromLTRB(windowRect.Left, windowRect.Top, windowRect.Right, windowRect.Bottom);
        if (fullWindowBounds.Width <= 0 || fullWindowBounds.Height <= 0)
        {
            return false;
        }

        using var fullWindowBitmap = new Bitmap(fullWindowBounds.Width, fullWindowBounds.Height);
        using (var graphics = Graphics.FromImage(fullWindowBitmap))
        {
            var hdc = graphics.GetHdc();
            try
            {
                if (!PrintWindow(handle, hdc, PW_RENDERFULLCONTENT))
                {
                    return false;
                }
            }
            finally
            {
                graphics.ReleaseHdc(hdc);
            }
        }

        var cropArea = new Rectangle(
            clientBounds.X - fullWindowBounds.X,
            clientBounds.Y - fullWindowBounds.Y,
            clientBounds.Width,
            clientBounds.Height);
        if (cropArea.X < 0
            || cropArea.Y < 0
            || cropArea.Right > fullWindowBounds.Width
            || cropArea.Bottom > fullWindowBounds.Height
            || cropArea.Width <= 0
            || cropArea.Height <= 0)
        {
            return false;
        }

        bitmap = fullWindowBitmap.Clone(cropArea, fullWindowBitmap.PixelFormat);
        return true;
    }

    public static INPUT CreateMouseInput(int dx, int dy, uint flags)
    {
        return new INPUT
        {
            type = INPUT_MOUSE,
            U = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = dx,
                    dy = dy,
                    dwFlags = flags,
                }
            }
        };
    }

    public static INPUT CreateKeyboardInput(ushort virtualKey, uint flags)
    {
        return new INPUT
        {
            type = INPUT_KEYBOARD,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = virtualKey,
                    wScan = 0,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero,
                }
            }
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;

        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}

sealed record GuiSmokeVideoRecordingMetadata(
    string ScopeKind,
    string SessionId,
    string RunId,
    string RootPath,
    string SessionRoot,
    string OutputPath)
{
    public string? AttemptId { get; init; }

    public string Status { get; set; } = "pending";

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? RecordingStartedAt { get; set; }

    public DateTimeOffset? RecordingEndedAt { get; set; }

    public string? FfmpegPath { get; set; }

    public bool FfmpegAvailable { get; set; }

    public string? CommandLine { get; set; }

    public bool WindowScopedCaptureRequested { get; set; }

    public string? WindowTitle { get; set; }

    public WindowBounds? CaptureBounds { get; set; }

    public bool? Kept { get; set; }

    public string? CompletionReason { get; set; }

    public string? SkipReason { get; set; }

    public int? ExitCode { get; set; }
}

sealed class GuiSmokeVideoRecorder : IDisposable
{
    private const int StopTimeoutMs = 5000;
    private const int MinimumCaptureDimension = 2;
    private const int CaptureFramerate = 15;

    private readonly string _workspaceRoot;
    private readonly string _metadataPath;
    private readonly GuiSmokeVideoRecordingMetadata _metadata;
    private string? _ffmpegProcessPath;
    private Process? _process;
    private bool _completed;
    private bool _disposed;

    private GuiSmokeVideoRecorder(
        string workspaceRoot,
        string rootPath,
        GuiSmokeVideoRecordingMetadata metadata)
    {
        _workspaceRoot = workspaceRoot;
        _metadataPath = Path.Combine(rootPath, "video-recording.json");
        _metadata = metadata;
        PersistMetadata();
    }

    public static GuiSmokeVideoRecorder Create(
        string workspaceRoot,
        IReadOnlyDictionary<string, string> options,
        string sessionId,
        string runId,
        string rootPath,
        string sessionRoot,
        string? attemptId,
        string scopeKind)
    {
        Directory.CreateDirectory(rootPath);
        var outputPath = Path.Combine(rootPath, "video.review.mkv");
        var metadata = new GuiSmokeVideoRecordingMetadata(
            scopeKind,
            sessionId,
            runId,
            rootPath,
            sessionRoot,
            outputPath)
        {
            AttemptId = attemptId,
        };
        var recorder = new GuiSmokeVideoRecorder(workspaceRoot, rootPath, metadata);
        recorder.InitializeAvailability(options);
        return recorder;
    }

    public bool TryStart(WindowCaptureTarget? target)
    {
        if (_completed || _process is not null)
        {
            return false;
        }

        if (string.Equals(_metadata.Status, "skipped", StringComparison.OrdinalIgnoreCase)
            || string.Equals(_metadata.Status, "start-failed", StringComparison.OrdinalIgnoreCase))
        {
            PersistMetadata();
            return false;
        }

        if (target is null)
        {
            MarkSkipped("window-not-found");
            return false;
        }

        var bounds = NormalizeCaptureBounds(target.Bounds);
        if (bounds.Width < MinimumCaptureDimension || bounds.Height < MinimumCaptureDimension)
        {
            MarkSkipped("window-bounds-invalid");
            return false;
        }

        _metadata.WindowScopedCaptureRequested = !target.IsFallback;
        _metadata.WindowTitle = target.Title;
        _metadata.CaptureBounds = new WindowBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);

        var outputProcessPath = GetProcessCompatiblePath(_metadata.OutputPath);
        var workingDirectory = GetProcessCompatiblePath(_workspaceRoot);
        var executablePath = _metadata.FfmpegPath!;
        var commandPath = _ffmpegProcessPath ?? executablePath;
        var arguments = BuildFfmpegArguments(bounds, outputProcessPath);
        _metadata.CommandLine = QuoteCommand(commandPath, arguments);
        try
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                },
            };
            _process.Start();
            _metadata.Status = "recording";
            _metadata.RecordingStartedAt = DateTimeOffset.UtcNow;
            PersistMetadata();
            return true;
        }
        catch (Exception exception)
        {
            _metadata.Status = "start-failed";
            _metadata.SkipReason = $"ffmpeg-start-failed:{exception.GetType().Name}:{SanitizeText(exception.Message)}";
            PersistMetadata();
            _process?.Dispose();
            _process = null;
            return false;
        }
    }

    public void Complete(bool keepRecording, string completionReason)
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        _metadata.CompletionReason = completionReason;
        _metadata.RecordingEndedAt = DateTimeOffset.UtcNow;

        if (_process is not null)
        {
            StopProcess();
        }

        if (_process is not null)
        {
            _metadata.ExitCode = _process.HasExited ? _process.ExitCode : null;
            _process.Dispose();
            _process = null;
        }

        var outputExists = File.Exists(_metadata.OutputPath);
        if (string.Equals(_metadata.Status, "recording", StringComparison.OrdinalIgnoreCase))
        {
            if (outputExists && keepRecording)
            {
                _metadata.Status = "kept";
                _metadata.Kept = true;
            }
            else if (outputExists)
            {
                if (TryDeleteOutput())
                {
                    _metadata.Status = "deleted";
                    _metadata.Kept = false;
                }
            }
            else
            {
                _metadata.Status = keepRecording ? "kept-missing-output" : "deleted-missing-output";
                _metadata.Kept = keepRecording;
            }
        }
        else if (string.Equals(_metadata.Status, "pending", StringComparison.OrdinalIgnoreCase))
        {
            _metadata.Status = "skipped";
            _metadata.SkipReason ??= "recording-never-started";
            _metadata.Kept = false;
        }
        else if (!string.Equals(_metadata.Status, "kept", StringComparison.OrdinalIgnoreCase)
                 && !string.Equals(_metadata.Status, "deleted", StringComparison.OrdinalIgnoreCase))
        {
            _metadata.Kept ??= false;
        }

        PersistMetadata();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (!_completed)
        {
            Complete(keepRecording: false, completionReason: "disposed-without-explicit-complete");
        }
    }

    private void InitializeAvailability(IReadOnlyDictionary<string, string> options)
    {
        if (options.ContainsKey("--disable-video-capture"))
        {
            _metadata.Status = "skipped";
            _metadata.SkipReason = "video-capture-disabled";
            PersistMetadata();
            return;
        }

        var ffmpegPath = ResolveFfmpegPath(options, _workspaceRoot);
        if (ffmpegPath is null)
        {
            _metadata.Status = "skipped";
            _metadata.FfmpegAvailable = false;
            _metadata.SkipReason = "ffmpeg-not-found";
            PersistMetadata();
            return;
        }

        _metadata.FfmpegPath = ffmpegPath.HostPath;
        _ffmpegProcessPath = ffmpegPath.ProcessPath;
        _metadata.FfmpegAvailable = true;
        PersistMetadata();
    }

    private static VideoPathBinding? ResolveFfmpegPath(IReadOnlyDictionary<string, string> options, string workspaceRoot)
    {
        if (options.TryGetValue("--ffmpeg-path", out var explicitPath))
        {
            return ResolveExecutablePath(explicitPath, workspaceRoot);
        }

        var envPath = Environment.GetEnvironmentVariable("STS2_GUI_SMOKE_FFMPEG_PATH");
        if (!string.IsNullOrWhiteSpace(envPath))
        {
            return ResolveExecutablePath(envPath, workspaceRoot);
        }

        var pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return null;
        }

        foreach (var pathEntry in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            foreach (var candidateName in new[] { "ffmpeg.exe", "ffmpeg" })
            {
                try
                {
                    var candidatePath = Path.Combine(pathEntry, candidateName);
                    if (File.Exists(candidatePath))
                    {
                        var fullHostPath = Path.GetFullPath(candidatePath);
                        return new VideoPathBinding(fullHostPath, GetProcessCompatiblePath(fullHostPath));
                    }
                }
                catch
                {
                }
            }
        }

        return null;
    }

    internal static VideoPathBinding? ResolveExecutablePathForSelfTest(string explicitPath, string workspaceRoot)
    {
        return ResolveExecutablePath(explicitPath, workspaceRoot);
    }

    private static Rectangle NormalizeCaptureBounds(Rectangle bounds)
    {
        var width = Math.Max(MinimumCaptureDimension, bounds.Width);
        var height = Math.Max(MinimumCaptureDimension, bounds.Height);
        if (width % 2 != 0)
        {
            width -= 1;
        }

        if (height % 2 != 0)
        {
            height -= 1;
        }

        width = Math.Max(MinimumCaptureDimension, width);
        height = Math.Max(MinimumCaptureDimension, height);
        return new Rectangle(bounds.X, bounds.Y, width, height);
    }

    private static string BuildFfmpegArguments(Rectangle bounds, string outputPath)
    {
        return string.Join(
            " ",
            "-hide_banner",
            "-loglevel error",
            "-nostats",
            "-y",
            "-f gdigrab",
            $"-framerate {CaptureFramerate.ToString(CultureInfo.InvariantCulture)}",
            $"-offset_x {bounds.X.ToString(CultureInfo.InvariantCulture)}",
            $"-offset_y {bounds.Y.ToString(CultureInfo.InvariantCulture)}",
            $"-video_size {bounds.Width.ToString(CultureInfo.InvariantCulture)}x{bounds.Height.ToString(CultureInfo.InvariantCulture)}",
            "-draw_mouse 0",
            "-i desktop",
            "-c:v libx264",
            "-preset ultrafast",
            "-pix_fmt yuv420p",
            QuoteArgument(outputPath));
    }

    private static string QuoteArgument(string value)
    {
        return $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
    }

    private static string QuoteCommand(string fileName, string arguments)
    {
        return $"{QuoteArgument(fileName)} {arguments}";
    }

    private void MarkSkipped(string reason)
    {
        _metadata.Status = "skipped";
        _metadata.SkipReason = reason;
        _metadata.Kept = false;
        PersistMetadata();
    }

    private void StopProcess()
    {
        if (_process is null)
        {
            return;
        }

        try
        {
            if (!_process.HasExited)
            {
                _process.StandardInput.WriteLine("q");
                _process.StandardInput.Flush();
            }
        }
        catch
        {
        }

        try
        {
            if (!_process.WaitForExit(StopTimeoutMs) && !_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                _process.WaitForExit(StopTimeoutMs);
            }
        }
        catch
        {
            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
            }
        }
    }

    private bool TryDeleteOutput()
    {
        try
        {
            if (File.Exists(_metadata.OutputPath))
            {
                File.Delete(_metadata.OutputPath);
            }

            return true;
        }
        catch (Exception exception)
        {
            _metadata.Status = "kept-delete-failed";
            _metadata.Kept = true;
            _metadata.CompletionReason = $"{_metadata.CompletionReason};delete-failed:{exception.GetType().Name}:{SanitizeText(exception.Message)}";
            return false;
        }
    }

    private void PersistMetadata()
    {
        var directory = Path.GetDirectoryName(_metadataPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_metadataPath, JsonSerializer.Serialize(_metadata, GuiSmokeShared.JsonOptions));
    }

    private static VideoPathBinding? ResolveExecutablePath(string explicitPath, string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(explicitPath))
        {
            throw new InvalidOperationException("A non-empty ffmpeg path is required.");
        }

        if (TryNormalizeWindowsPath(explicitPath, out var processPath))
        {
            return TryBindExecutable(processPath, fallbackHostPath: null);
        }

        if (TryConvertHostPathToWindows(explicitPath, out processPath))
        {
            return TryBindExecutable(processPath, fallbackHostPath: explicitPath);
        }

        var fullPath = Path.IsPathRooted(explicitPath)
            ? Path.GetFullPath(explicitPath)
            : Path.GetFullPath(explicitPath, workspaceRoot);
        if (TryConvertHostPathToWindows(fullPath, out processPath))
        {
            return TryBindExecutable(processPath, fallbackHostPath: fullPath);
        }

        return File.Exists(fullPath)
            ? new VideoPathBinding(fullPath, fullPath)
            : null;
    }

    private static VideoPathBinding? TryBindExecutable(string processPath, string? fallbackHostPath)
    {
        foreach (var candidate in EnumerateExecutableHostCandidates(processPath, fallbackHostPath))
        {
            try
            {
                var fullHostPath = Path.GetFullPath(candidate);
                if (File.Exists(fullHostPath))
                {
                    return new VideoPathBinding(fullHostPath, processPath);
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateExecutableHostCandidates(string processPath, string? fallbackHostPath)
    {
        if (OperatingSystem.IsWindows())
        {
            yield return processPath;
        }

        if (!string.IsNullOrWhiteSpace(fallbackHostPath))
        {
            yield return fallbackHostPath;
        }

        if (TryConvertWindowsPathToWslHost(processPath, out var translatedHostPath))
        {
            yield return translatedHostPath;
        }
    }

    private static string SanitizeText(string? value)
    {
        return (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Trim();
    }

    private static string GetProcessCompatiblePath(string path)
    {
        if (TryConvertHostPathToWindows(path, out var translatedPath))
        {
            return translatedPath;
        }

        return Path.GetFullPath(path);
    }

    private static string NormalizeWindowsProcessPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        return path.Replace('/', '\\');
    }

    private static bool TryNormalizeWindowsPath(string path, out string translatedPath)
    {
        translatedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        if (path.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase)
            || !(path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':'))
        {
            return false;
        }

        var driveLetter = char.ToUpperInvariant(path[0]);
        var suffix = path[2..].Replace('/', '\\');
        translatedPath = $"{driveLetter}:{suffix}";
        return true;
    }

    private static bool TryConvertWindowsPathToWslHost(string path, out string translatedPath)
    {
        translatedPath = string.Empty;
        if (!TryNormalizeWindowsPath(path, out var normalizedWindowsPath))
        {
            return false;
        }

        var driveLetter = char.ToLowerInvariant(normalizedWindowsPath[0]);
        var suffix = normalizedWindowsPath[2..].Replace('\\', '/');
        translatedPath = $"/mnt/{driveLetter}{suffix}";
        return true;
    }

    private static bool TryConvertHostPathToWindows(string path, out string translatedPath)
    {
        translatedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        if (path.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase)
            || (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':'))
        {
            return false;
        }

        var normalized = path.Replace('\\', '/');
        if (!normalized.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase)
            || normalized.Length < 7
            || !char.IsLetter(normalized[5])
            || normalized[6] != '/')
        {
            return false;
        }

        var driveLetter = char.ToUpperInvariant(normalized[5]);
        translatedPath = $"{driveLetter}:{normalized[6..].Replace('/', '\\')}";
        return true;
    }
}

sealed record VideoPathBinding(string HostPath, string ProcessPath);
