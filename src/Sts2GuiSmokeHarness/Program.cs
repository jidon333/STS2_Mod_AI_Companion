using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Windows.Forms;
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
        ? Path.GetFullPath(explicitRunRoot, workspaceRoot)
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

    if (!options.ContainsKey("--skip-deploy"))
    {
        EnsureGameNotRunning();
        await GuiSmokeShared.RunProcessAsync(
            "dotnet",
            "run --project src\\Sts2ModKit.Tool -- deploy-native-package --include-harness-bridge",
            workspaceRoot,
            TimeSpan.FromMinutes(5)).ConfigureAwait(false);
        EnsureHarnessEnabledInRuntimeConfig(configuration);
    }

    if (isLongRun)
    {
        Directory.CreateDirectory(sessionRoot);
        Directory.CreateDirectory(Path.Combine(sessionRoot, "attempts"));
    }
    else if (Directory.Exists(sessionRoot))
    {
        Directory.Delete(sessionRoot, recursive: true);
    }

    if (isLongRun && options.ContainsKey("--skip-launch"))
    {
        maxAttempts = Math.Min(maxAttempts, 1);
    }

    GuiSmokeAttemptResult? lastAttempt = null;
    var consecutiveLaunchFailures = 0;
    var consecutiveSceneDeadEnds = 0;
    for (var attemptOrdinal = 1; attemptOrdinal <= maxAttempts && DateTimeOffset.UtcNow < sessionDeadline; attemptOrdinal += 1)
    {
        var attemptId = attemptOrdinal.ToString("0000", CultureInfo.InvariantCulture);
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
            sessionDeadline).ConfigureAwait(false);

        if (!isLongRun)
        {
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
            return 1;
        }

        if (consecutiveSceneDeadEnds >= maxSceneDeadEnds)
        {
            LogHarness($"session abort consecutive scene dead-ends={consecutiveSceneDeadEnds}");
            return 1;
        }
    }

    return lastAttempt?.ExitCode ?? 1;
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
    DateTimeOffset sessionDeadline)
{
    const int PassiveWaitMs = 1000;
    const int ActionSettleMinimumMs = 900;
    const int CombatActionSettleMinimumMs = 300;
    const int TransitionSettleMs = 2000;

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

    GuiSmokeAttemptResult CompleteAttempt(int exitCode, string status, string message, bool launchFailed = false)
    {
        logger.CompleteRun(status, message);
        logger.WriteValidationSummary(runId);
        if (isLongRun)
        {
            var selfMetaReview = LongRunArtifacts.WriteAttemptMetaReview(sessionRoot, attemptId, attemptOrdinal, runId, status, message);
            logger.WriteSelfMetaReview(selfMetaReview);
            LongRunArtifacts.WriteSessionArtifacts(sessionRoot, logger, runId, scenarioId, providerKind, attemptId, attemptOrdinal, stepIndex, status, message);
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
            launchFailed);
    }

    if (!options.ContainsKey("--skip-launch"))
    {
        try
        {
            await StopGameProcessesAsync(TimeSpan.FromSeconds(20)).ConfigureAwait(false);
            EnsureGameNotRunning();
            var launchIssuedAt = DateTimeOffset.UtcNow;
            await GuiSmokeShared.RunProcessAsync(
                Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
                "/c start \"\" \"steam://rungameid/2868840\"",
                workspaceRoot,
                TimeSpan.FromSeconds(10),
                waitForExit: false).ConfigureAwait(false);

            await WaitForLiveGameWindowAsync(launchIssuedAt, TimeSpan.FromMinutes(2)).ConfigureAwait(false);
            await MaintainLaunchFocusAsync(TimeSpan.FromSeconds(20)).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                GuiSmokePhase.WaitMainMenu.ToString(),
                $"launch-failed: {exception.Message}",
                null,
                null,
                null));
            return CompleteAttempt(1, "failed", $"launch-failed: {exception.Message}", launchFailed: true);
        }
    }

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
    var useDecisionAgeGuard = !string.Equals(providerKind, "auto", StringComparison.OrdinalIgnoreCase);
    var decisionStaleBudget = useDecisionAgeGuard
        ? TimeSpan.FromSeconds(2)
        : TimeSpan.FromSeconds(30);

    while (DateTimeOffset.UtcNow < waitDeadline)
    {
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
            consecutiveBlackFrames += 1;
            LogHarness($"step={stepIndex} capture unusable; waiting for a valid process frame");
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "capture-unusable", null, null, null));
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
        LogHarness($"step={stepIndex} captured={screenshotPath}");

        var observer = observerReader.Read();
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
            return CompleteAttempt(0, "completed", "returned-main-menu");
        }

        if (!observer.IsFreshSince(freshnessFloor))
        {
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
                return CompleteAttempt(0, "passed", "combat flow accepted by observer");
            }

            await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
            continue;
        }

        if (TryAdvanceAlternateBranch(phase, observer, history, logger, stepIndex, isLongRun, out var alternatePhase))
        {
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
                return CompleteAttempt(0, "passed", "combat flow accepted by observer");
            }

            await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
            continue;
        }

        if (IsPassiveWaitPhase(phase))
        {
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
                return CompleteAttempt(1, "failed", $"timeout at {phase}");
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
        var decision = await provider.GetDecisionAsync(requestPath, decisionPath, TimeSpan.FromMinutes(3), CancellationToken.None).ConfigureAwait(false);
        logger.WriteDecision(decisionPath, decision);
        LogHarness($"step={stepIndex} decision status={decision.Status} action={decision.ActionKind ?? "null"} target={decision.TargetLabel ?? "null"} confidence={decision.Confidence?.ToString("0.00") ?? "null"} reason={decision.Reason ?? "null"}");
        if (isLongRun)
        {
            LongRunArtifacts.MaybeRecordUnknownScene(sessionRoot, request, decision);
        }

        if (string.Equals(decision.Status, "abort", StringComparison.OrdinalIgnoreCase))
        {
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
            return CompleteAttempt(1, "failed", decision.AbortReason ?? "decision aborted");
        }

        if (string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase))
        {
            var waitMinimumMs = GetDecisionWaitMinimumMs(phase);
            LogHarness($"step={stepIndex} wait requested ms={Math.Max(250, decision.WaitMs ?? waitMinimumMs)}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "wait", decision.TargetLabel, DateTimeOffset.UtcNow));
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
            return CompleteAttempt(1, "failed", abortReason);
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
                inputDriver.MoveCursor(clickWindow, 0.460, 0.480);
                LogHarness($"step={stepIndex} cursor primed for non-enemy confirm normalized=(0.460,0.480)");
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
        else
        {
            var clickPoint = MouseInputDriver.TransformNormalizedPoint(clickWindow, decision.NormalizedX!.Value, decision.NormalizedY!.Value);
            LogHarness($"step={stepIndex} click target={decision.TargetLabel ?? "null"} normalized=({decision.NormalizedX:0.000},{decision.NormalizedY:0.000}) absolute=({clickPoint.X},{clickPoint.Y}) bounds={DescribeBounds(clickWindow.Bounds)}");
            inputDriver.Click(clickWindow, decision.NormalizedX!.Value, decision.NormalizedY!.Value);
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "click", decision.TargetLabel, DateTimeOffset.UtcNow));
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

        var postActionObserver = observerReader.Read();
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
                sameActionStallCount));

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

        attemptsByPhase.Clear();
        LogHarness($"step={stepIndex} next phase={phase}");
        var remainingSettleDelayMs = Math.Max(0, settleDelayMs - progressProbeDelayMs);
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
    return CompleteAttempt(1, "failed", "global timeout");
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

static bool IsSceneDeadEndAttempt(GuiSmokeAttemptResult result)
{
    if (result.LaunchFailed)
    {
        return false;
    }

    return result.Message.Contains("same-action-stall", StringComparison.OrdinalIgnoreCase)
           || result.Message.Contains("dead-end", StringComparison.OrdinalIgnoreCase)
           || result.Message.Contains("decision aborted", StringComparison.OrdinalIgnoreCase)
           || result.Message.Contains("timeout at Handle", StringComparison.OrdinalIgnoreCase);
}

static async Task StopGameProcessesAsync(TimeSpan timeout)
{
    var relevantNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "SlayTheSpire2",
        "crashpad_handler",
    };

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

static int InspectRun(IReadOnlyDictionary<string, string> options, string workspaceRoot)
{
    if (!options.TryGetValue("--run-root", out var runRoot))
    {
        throw new InvalidOperationException("--run-root is required.");
    }

    var resolvedRoot = Path.GetFullPath(runRoot, workspaceRoot);
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
        Steps = Directory.Exists(Path.Combine(resolvedRoot, "steps"))
            ? Directory.GetFiles(Path.Combine(resolvedRoot, "steps"), "*.screen.png").Length
            : 0,
    }, GuiSmokeShared.JsonOptions));
    return 0;
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

static async Task MaintainLaunchFocusAsync(TimeSpan duration)
{
    var deadline = DateTimeOffset.UtcNow.Add(duration);
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
        }

        await Task.Delay(1000).ConfigureAwait(false);
    }
}

static void EnsureHarnessEnabledInRuntimeConfig(ScaffoldConfiguration configuration)
{
    var runtimeConfigPath = Path.Combine(configuration.GamePaths.GameDirectory, "mods", "sts2-mod-ai-companion.config.json");
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

    harnessObject["enabled"] = true;
    File.WriteAllText(runtimeConfigPath, rootObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    LogHarness($"runtime config ensured harness.enabled=true path={runtimeConfigPath}");
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
        GetAllowedActions(phase, observer),
        history.TakeLast(5).ToArray(),
        BuildFailureModeHint(phase, observer),
        BuildDecisionRiskHint(phase, observer, firstSeenScene, reasoningMode));
}

static string ComputeSceneSignature(string screenshotPath, ObserverState observer, GuiSmokePhase phase)
{
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

    var mapAnalysis = AutoMapAnalyzer.Analyze(screenshotPath);
    if (mapAnalysis.HasCurrentArrow)
    {
        tags.Add("visible:map-arrow");
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
        var normalizedName = NormalizeKnowledgeKey(handCard.Name);
        if (normalizedName.Length == 0)
        {
            continue;
        }

        var match = cards.FirstOrDefault(card => card.MatchKeys.Contains(normalizedName, StringComparer.Ordinal));
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
    return phase switch
    {
        GuiSmokePhase.EnterRun => new[] { "click continue", "click singleplayer", "wait" },
        GuiSmokePhase.ChooseCharacter => new[] { "click ironclad", "wait" },
        GuiSmokePhase.Embark => new[] { "click embark", "wait" },
        GuiSmokePhase.HandleRewards when string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
            => new[] { "click proceed", "click reward", "click first reachable node", "wait" },
        GuiSmokePhase.HandleRewards => new[] { "click proceed", "click reward", "wait" },
        GuiSmokePhase.ChooseFirstNode when LooksLikeRestSiteState(observer.Summary)
            => new[] { "click rest option", "click smith card", "click smith confirm", "wait" },
        GuiSmokePhase.ChooseFirstNode when LooksLikeTreasureState(observer.Summary)
            => new[] { "click treasure chest", "click treasure reward icon", "click proceed", "wait" },
        GuiSmokePhase.ChooseFirstNode => new[] { "click first reachable node", "wait" },
        GuiSmokePhase.HandleEvent when LooksLikeTreasureState(observer.Summary)
            => new[] { "click treasure chest", "click treasure reward icon", "click proceed", "click first event option", "wait" },
        GuiSmokePhase.HandleEvent => new[] { "click first event option", "wait" },
        GuiSmokePhase.HandleCombat => new[] { "click card", "click enemy", "click end turn", "wait" },
        _ => new[] { "wait" },
    };
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
        || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    return observer.CurrentChoices.Any(static label =>
        label.Contains("\uD734\uC2DD", StringComparison.OrdinalIgnoreCase)
        || label.Contains("Rest", StringComparison.OrdinalIgnoreCase)
        || label.Contains("\uC7AC\uB828", StringComparison.OrdinalIgnoreCase)
        || label.Contains("Smith", StringComparison.OrdinalIgnoreCase));
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

static string BuildFailureModeHint(GuiSmokePhase phase, ObserverState observer)
{
    return phase switch
    {
        GuiSmokePhase.EnterRun => "Continue may be absent. Use Singleplayer only if Continue is not visible.",
        GuiSmokePhase.ChooseCharacter => "Do not click Embark before Ironclad is selected.",
        GuiSmokePhase.HandleRewards when string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
            => "AI first: use the screenshot as the primary source. If the map is clearly visible, you may click the first reachable node instead of forcing another reward proceed click.",
        GuiSmokePhase.HandleRewards => "Prefer the proceed arrow when the reward can be skipped; otherwise pick a valid reward card.",
        GuiSmokePhase.ChooseFirstNode when LooksLikeRestSiteState(observer.Summary)
            => "Rest site is screenshot-first. If the two rest options are visible, choose rest or smith. If the smith card grid is visible, click one card first and then click the right-side confirm button.",
        GuiSmokePhase.ChooseFirstNode when LooksLikeTreasureState(observer.Summary)
            => "Treasure rooms are not map-node routing. Closed chest: click the center chest. Open chest: click the floating reward icon, then proceed or return to map.",
        GuiSmokePhase.ChooseFirstNode => "Do not click non-reachable map nodes.",
        GuiSmokePhase.HandleEvent when LooksLikeTreasureState(observer.Summary)
            => "Treasure state can linger as event. Use the screenshot, not stale observer state: closed chest -> center click, open chest -> floating reward icon, then proceed/map.",
        GuiSmokePhase.HandleEvent => "If the event text is ambiguous, choose the first visible option.",
        GuiSmokePhase.WaitCombat => "Observer must end with combat screen and inCombat=true.",
        GuiSmokePhase.HandleCombat => "AI first: read the full combat board from the screenshot. Cards, targets, energy, and end-turn are visual decisions. The harness only executes the click you choose.",
        _ => "Fail closed when screenshot and observer disagree.",
    };
}

static GuiSmokePhase GetPostRewardPhase(GuiSmokeStepDecision decision)
{
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
           || string.Equals(targetLabel, "rest site: smith", StringComparison.OrdinalIgnoreCase);
}

static bool IsPassiveWaitPhase(GuiSmokePhase phase)
{
    return phase is GuiSmokePhase.WaitMainMenu
        or GuiSmokePhase.WaitCharacterSelect
        or GuiSmokePhase.WaitMap
        or GuiSmokePhase.WaitCombat;
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
        "first event option" => 4,
        "visible proceed" => 4,
        "visible map advance" => 4,
        "visible reachable node" => 4,
        "first reachable node" => 4,
        "treasure chest center" => 4,
        "treasure reward icon" => 4,
        "rest site: smith card" => 4,
        "rest site: smith confirm" => 4,
        "hidden overlay close" => 4,
        "overlay back" => 4,
        "overlay close" => 4,
        "overlay backdrop close" => 4,
        _ => 2,
    };
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

        if (string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase))
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

        if (string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-event", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-event", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleEvent;
            return true;
        }
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

        if (string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase))
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

        if (string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase))
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

        if (string.Equals(observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase) && observer.InCombat == true)
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-combat", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-combat", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleCombat;
            return true;
        }

        if (string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase))
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

        if (string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase) && phase == GuiSmokePhase.WaitCombat)
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-map", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-map", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase))
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "branch-event", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "branch-event", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.HandleEvent;
            return true;
        }
    }

    if (phase == GuiSmokePhase.HandleEvent)
    {
        if (string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase))
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
        }

        if (IsNonEnemyCombatSelectionLabel(decision.TargetLabel) && postActionObserver is not null && LooksLikeNonEnemyConfirmSuccess(observer, postActionObserver))
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

static bool LooksLikeNonEnemyConfirmSuccess(ObserverState before, ObserverState after)
{
    if (before.PlayerEnergy is not null && after.PlayerEnergy is not null && after.PlayerEnergy < before.PlayerEnergy)
    {
        return true;
    }

    return after.CombatHand.Count < before.CombatHand.Count
           || HasMeaningfulObserverDelta(before, after);
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
        throw new InvalidOperationException("Only click, right-click, and press-key actionKind are supported.");
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

static void WriteUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- run --scenario boot-to-combat|boot-to-long-run --provider session|auto|headless [--provider-command \"<cmd>\"] [--config path] [--run-root path] [--max-attempts n] [--max-consecutive-launch-failures n] [--max-scene-dead-ends n] [--max-session-hours n]");
    Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- inspect-run --run-root <path>");
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

        process.Start();
        if (!waitForExit)
        {
            return;
        }

        using var timeoutCts = new CancellationTokenSource(timeout);
        await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
        if (process.ExitCode != 0)
        {
            var stdout = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
            var stderr = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
            throw new InvalidOperationException(
                $"Process failed: {fileName} {arguments}{Environment.NewLine}stdout:{Environment.NewLine}{stdout}{Environment.NewLine}stderr:{Environment.NewLine}{stderr}");
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
    bool LaunchFailed);

sealed record GuiSmokeAttemptIndexEntry(
    string AttemptId,
    int AttemptOrdinal,
    string RunId,
    string Status,
    string? ResultMessage,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    int StepCount);

sealed record GuiSmokeFailureSummary(
    string Phase,
    string Message,
    string? ObserverScreen,
    bool? ObserverInCombat,
    string? ScreenshotPath);

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

sealed record GuiSmokeHistoryEntry(
    string Phase,
    string Action,
    string? TargetLabel,
    DateTimeOffset RecordedAt);

sealed record WindowBounds(
    int X,
    int Y,
    int Width,
    int Height);

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
    IReadOnlyList<ObservedCombatHandCard> CombatHand);

sealed record ObserverActionNode(
    string NodeId,
    string Kind,
    string Label,
    string? ScreenBounds,
    bool Actionable);

sealed record ObserverChoice(
    string Kind,
    string Label,
    string? ScreenBounds);

sealed record ObservedCombatHandCard(
    int SlotIndex,
    string Name,
    string? Type,
    int? Cost);

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
    public IReadOnlyList<ObserverActionNode> ActionNodes => Summary.ActionNodes;
    public IReadOnlyList<ObservedCombatHandCard> CombatHand => Summary.CombatHand;
    public DateTimeOffset? CapturedAt => Summary.CapturedAt;

    public bool IsFreshSince(DateTimeOffset threshold)
    {
        return CapturedAt is not null && CapturedAt >= threshold;
    }

    public string? EncounterKind => Summary.EncounterKind;
    public string? ChoiceExtractorPath => Summary.ChoiceExtractorPath;
}

interface IGuiDecisionProvider
{
    Task<GuiSmokeStepDecision> GetDecisionAsync(string requestPath, string decisionPath, TimeSpan timeout, CancellationToken cancellationToken);
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
    public async Task<GuiSmokeStepDecision> GetDecisionAsync(string requestPath, string decisionPath, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var stream = new FileStream(requestPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var request = await JsonSerializer.DeserializeAsync<GuiSmokeStepRequest>(stream, GuiSmokeShared.JsonOptions, cancellationToken).ConfigureAwait(false)
                     ?? throw new InvalidOperationException("Failed to parse step request.");
        return Decide(request);
    }

    public static GuiSmokeStepDecision Decide(GuiSmokeStepRequest request)
    {
        var phase = Enum.Parse<GuiSmokePhase>(request.Phase, ignoreCase: true);
        return phase switch
        {
            GuiSmokePhase.EnterRun => DecideEnterRun(request),
            GuiSmokePhase.ChooseCharacter => DecideChooseCharacter(request),
            GuiSmokePhase.Embark => DecideEmbark(request),
            GuiSmokePhase.HandleRewards => DecideHandleRewards(request),
            GuiSmokePhase.ChooseFirstNode => DecideChooseFirstNode(request),
            GuiSmokePhase.HandleEvent => DecideHandleEvent(request),
            GuiSmokePhase.HandleCombat => DecideHandleCombat(request),
            _ => CreateWaitDecision("waiting for passive phase", request.Observer.CurrentScreen),
        };
    }

    private static GuiSmokeStepDecision DecideEnterRun(GuiSmokeStepRequest request)
    {
        return TryFindActionNodeDecision(request, "Continue", "continue")
               ?? TryFindActionNodeDecision(request, "\uACC4\uC18D", "continue")
               ?? TryFindActionNodeDecision(request, "Singleplayer", "singleplayer")
               ?? TryFindActionNodeDecision(request, "\uC2F1\uAE00", "singleplayer")
               ?? CreateWaitDecision("main menu actions not yet visible", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideChooseCharacter(GuiSmokeStepRequest request)
    {
        return TryFindActionNodeDecision(request, "Ironclad", "ironclad")
               ?? CreateWaitDecision("waiting for ironclad node", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideEmbark(GuiSmokeStepRequest request)
    {
        return TryFindActionNodeDecision(request, "Embark", "embark")
               ?? CreateWaitDecision("waiting for embark action", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideHandleRewards(GuiSmokeStepRequest request)
    {
        if (string.Equals(request.Observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
            || LooksLikeScreenshotFirstRoomState(request))
        {
            var roomDecision = TryCreateScreenshotFirstRoomDecision(request);
            if (roomDecision is not null)
            {
                return roomDecision;
            }
        }

        var claimedRewardRecently = request.History.Any(entry =>
            string.Equals(entry.Phase, GuiSmokePhase.HandleRewards.ToString(), StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.TargetLabel, "claim reward item", StringComparison.OrdinalIgnoreCase));

        var rewardNode = request.Observer.ActionNodes
            .FirstOrDefault(node => node.Actionable
                                    && node.ScreenBounds is not null
                                    && !IsProceedNode(node)
                                    && !IsBackNode(node)
                                    && !IsMapNode(node));
        if (rewardNode is not null && !claimedRewardRecently)
        {
            return CreateClickDecisionFromNode(request, rewardNode, "claim reward item");
        }

        var proceedNode = request.Observer.ActionNodes.FirstOrDefault(node => node.Actionable && IsProceedNode(node));
        if (proceedNode is not null)
        {
            return CreateClickDecisionFromNode(request, proceedNode, "proceed after resolving rewards");
        }

        return CreateWaitDecision("waiting for reward actions", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideChooseFirstNode(GuiSmokeStepRequest request)
    {
        if (LooksLikeScreenshotFirstRoomState(request))
        {
            var roomDecision = TryCreateScreenshotFirstRoomDecision(request);
            if (roomDecision is not null)
            {
                return roomDecision;
            }
        }

        return CreateWaitDecision("waiting for reachable map node", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideHandleShop(GuiSmokeStepRequest request)
    {
        return TryCreateHiddenOverlayCleanupDecision(request)
               ?? TryCreateVisibleProceedDecision(request)
               ?? CreateWaitDecision("waiting for shop exit or stable shop UI", request.Observer.CurrentScreen);
    }

    private static GuiSmokeStepDecision DecideHandleEvent(GuiSmokeStepRequest request)
    {
        var hasCardGrid = request.Observer.Choices.Any(choice =>
            string.Equals(choice.Kind, "card", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.Label, "CardGrid", StringComparison.OrdinalIgnoreCase));
        if (hasCardGrid)
        {
            var confirmChoice = request.Observer.Choices
                .Where(choice =>
                    !string.IsNullOrWhiteSpace(choice.ScreenBounds)
                    && (string.Equals(choice.Label, "Confirm", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(choice.Label, "확인", StringComparison.OrdinalIgnoreCase)))
                .Where(choice => TryParseNodeBounds(choice.ScreenBounds, out var confirmBounds) && HasUsableLogicalBounds(choice.ScreenBounds))
                .OrderByDescending(choice => TryParseNodeBounds(choice.ScreenBounds, out var confirmBounds) ? confirmBounds.X : float.MinValue)
                .FirstOrDefault();
            if (confirmChoice is not null && TryParseNodeBounds(confirmChoice.ScreenBounds, out var confirmBounds))
            {
                var centerX = confirmBounds.X + confirmBounds.Width / 2f;
                var centerY = confirmBounds.Y + confirmBounds.Height / 2f;
                return new GuiSmokeStepDecision(
                    "act",
                    "click",
                    null,
                    Math.Clamp(centerX / 1920f, 0d, 1d),
                    Math.Clamp(centerY / 1080f, 0d, 1d),
                    "event confirm",
                    "Card selection substate is active. Confirm the already selected card.",
                    0.98,
                    "event",
                    1400,
                    true,
                    null);
            }
        }

        var roomDecision = TryCreateScreenshotFirstRoomDecision(request);
        if (roomDecision is not null)
        {
            return roomDecision;
        }

        var semanticDecision = TryCreateSemanticEventDecision(request);
        if (semanticDecision is not null)
        {
            return semanticDecision;
        }

        var firstChoice = request.Observer.Choices
            .Where(choice =>
                !string.IsNullOrWhiteSpace(choice.ScreenBounds)
                && !string.Equals(choice.Label, "Confirm", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(choice.Label, "확인", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(choice.Label, "Cancel", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(choice.Label, "취소", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(choice.Label, "Close", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(choice.Label, "닫기", StringComparison.OrdinalIgnoreCase))
            .OrderBy(choice => TryParseNodeBounds(choice.ScreenBounds, out var bounds) ? bounds.Y : float.MaxValue)
            .ThenBy(choice => TryParseNodeBounds(choice.ScreenBounds, out var bounds) ? bounds.X : float.MaxValue)
            .FirstOrDefault();
        if (firstChoice is not null && TryParseNodeBounds(firstChoice.ScreenBounds, out var bounds))
        {
            var centerX = bounds.X + bounds.Width / 2f;
            var centerY = bounds.Y + bounds.Height / 2f;
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                Math.Clamp(centerX / 1920f, 0d, 1d),
                Math.Clamp(centerY / 1080f, 0d, 1d),
                "first event option",
                $"Event room is visible. Choose the first option '{firstChoice.Label}'.",
                0.92,
                "event",
                1400,
                true,
                null);
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            0.700,
            0.430,
            "first event option",
            "Event room is visible. Choose the first visible option by default.",
            0.80,
            "event",
            1400,
            true,
            null,
            request.EventKnowledgeCandidates.Count > 0
                ? $"event candidate: {request.EventKnowledgeCandidates[0].Title}"
                : "event fallback",
            "event option advances or reveals next state",
            request.DecisionRiskHint);
    }

    private static GuiSmokeStepDecision DecideHandleCombat(GuiSmokeStepRequest request)
    {
        var analysis = AutoCombatAnalyzer.Analyze(request.ScreenshotPath);
        var handAnalysis = AutoCombatHandAnalyzer.Analyze(request.ScreenshotPath);
        var pendingSelection = TryGetPendingCombatSelection(request.History);
        var repeatedNonEnemyLoop = HasRecentRepeatedNonEnemyLoop(request.History);
        if (analysis.HasTargetArrow)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.744,
                0.542,
                "auto-target enemy",
                "An attack card is selected. Click the enemy body to resolve it.",
                0.93,
                "combat",
                350,
                true,
                null);
        }

        if (request.Observer.PlayerEnergy is <= 0)
        {
            return new GuiSmokeStepDecision(
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
                null);
        }

        if (analysis.HasSelfTargetBrackets
            && (analysis.SelectedCardKind == AutoCombatCardKind.DefendLike
                || pendingSelection?.Kind == AutoCombatCardKind.DefendLike))
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.460,
                0.480,
                "confirm selected non-enemy card",
                "A self or non-enemy targeted card is selected. Click the safe center area to confirm it.",
                0.82,
                "combat",
                300,
                true,
                null);
        }

        if (analysis.HasSelectedCard
            && analysis.SelectedCardKind == AutoCombatCardKind.AttackLike
            && pendingSelection?.Kind == AutoCombatCardKind.AttackLike)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.744,
                0.542,
                "auto-target enemy",
                "An attack card overlay is still selected. Click the enemy body to resolve it.",
                0.88,
                "combat",
                350,
                true,
                null);
        }

        if (analysis.HasSelectedCard
            && analysis.SelectedCardKind == AutoCombatCardKind.DefendLike)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.460,
                0.480,
                "confirm selected non-enemy card",
                "A non-enemy card overlay is still selected. Click the safe center area to confirm it.",
                0.78,
                "combat",
                300,
                true,
                null);
        }

        if (repeatedNonEnemyLoop)
        {
            return new GuiSmokeStepDecision(
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
                null);
        }

        var knowledgeAttackSlot = request.CombatCardKnowledge
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => IsEnemyTargetCombatCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy))
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var observerAttackSlot = request.Observer.CombatHand
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => IsAttackCombatHandCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy))
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var attackSlot = knowledgeAttackSlot is not null
            ? new AutoCombatHandSlotAnalysis(
                knowledgeAttackSlot.SlotIndex,
                true,
                AutoCombatCardKind.AttackLike,
                double.MaxValue,
                double.MaxValue,
                0,
                0)
            : observerAttackSlot is not null
                ? new AutoCombatHandSlotAnalysis(
                observerAttackSlot.SlotIndex,
                true,
                AutoCombatCardKind.AttackLike,
                double.MaxValue,
                double.MaxValue,
                0,
                0)
                : handAnalysis.Slots
                .Where(static slot => slot.IsVisible && slot.Kind == AutoCombatCardKind.AttackLike)
                .OrderByDescending(static slot => slot.RedBlueDelta)
                .ThenByDescending(static slot => slot.Brightness)
                .FirstOrDefault();
        if (attackSlot is not null)
        {
            return new GuiSmokeStepDecision(
                "act",
                "press-key",
                attackSlot.SlotIndex.ToString(CultureInfo.InvariantCulture),
                null,
                null,
                $"combat select attack slot {attackSlot.SlotIndex}",
                "The screenshot still shows an attack card in hand. Use the corresponding hotkey first, then target the enemy.",
                0.80,
                "combat",
                250,
                true,
                null);
        }

        var knowledgeNonEnemySlot = request.CombatCardKnowledge
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => IsNonEnemyCombatCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy))
            .OrderBy(card => card.SlotIndex)
            .FirstOrDefault();
        var observerNonEnemySlot = request.Observer.CombatHand
            .Where(card => card.SlotIndex >= 1 && card.SlotIndex <= 5)
            .Where(card => IsNonEnemyCombatHandCard(card) && IsPlayableAtCurrentEnergy(card, request.Observer.PlayerEnergy))
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
                .Where(static slot => slot.IsVisible && slot.Kind == AutoCombatCardKind.DefendLike)
                .OrderBy(static slot => slot.RedBlueDelta)
                .ThenByDescending(static slot => slot.Brightness)
                .FirstOrDefault();
        if (nonEnemySlot is not null)
        {
            return new GuiSmokeStepDecision(
                "act",
                "press-key",
                nonEnemySlot.SlotIndex.ToString(CultureInfo.InvariantCulture),
                null,
                null,
                $"combat select non-enemy slot {nonEnemySlot.SlotIndex}",
                "Only non-enemy cards remain in hand. Use the corresponding hotkey, then resolve the self or non-enemy confirmation.",
                0.74,
                "combat",
                250,
                true,
                null);
        }

        if (analysis.HasSelectedCard && HasRecentCombatCardSelection(request.History))
        {
            return new GuiSmokeStepDecision(
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
                null);
        }

        return new GuiSmokeStepDecision(
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
    }

    private static bool HasRecentCombatCardSelection(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return TryGetPendingCombatSelection(history) is not null;
    }

    private static bool HasRecentRepeatedNonEnemyLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var labels = history
            .Where(entry => string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            .Select(static entry => entry.TargetLabel)
            .Where(static label =>
                !string.IsNullOrWhiteSpace(label)
                && (IsNonEnemySelectionLabel(label)
                    || string.Equals(label, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase)))
            .TakeLast(4)
            .ToArray();
        if (labels.Length < 4)
        {
            return false;
        }

        return IsNonEnemySelectionLabel(labels[0])
               && string.Equals(labels[1], "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase)
               && IsNonEnemySelectionLabel(labels[2])
               && string.Equals(labels[3], "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase);
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

    private static PendingCombatSelection? TryGetPendingCombatSelection(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.TargetLabel))
            {
                return null;
            }

            if (TryParsePendingCombatSelection(entry.TargetLabel, out var selection))
            {
                return selection;
            }

            if (entry.TargetLabel.StartsWith("auto-select slot ", StringComparison.OrdinalIgnoreCase)
                && ExtractFirstDigit(entry.TargetLabel) is { } legacySlot
                && legacySlot >= 1
                && legacySlot <= 5)
            {
                return new PendingCombatSelection(legacySlot, AutoCombatCardKind.AttackLike);
            }

            return null;
        }

        return null;
    }

    private static bool TryParsePendingCombatSelection(string targetLabel, out PendingCombatSelection? selection)
    {
        selection = null;
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return false;
        }

        if (targetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase)
            && ExtractFirstDigit(targetLabel) is { } attackSlot
            && attackSlot >= 1
            && attackSlot <= 5)
        {
            selection = new PendingCombatSelection(attackSlot, AutoCombatCardKind.AttackLike);
            return true;
        }

        if ((targetLabel.StartsWith("combat select non-enemy slot ", StringComparison.OrdinalIgnoreCase)
             || targetLabel.StartsWith("combat select defend slot ", StringComparison.OrdinalIgnoreCase))
            && ExtractFirstDigit(targetLabel) is { } nonEnemySlot
            && nonEnemySlot >= 1
            && nonEnemySlot <= 5)
        {
            selection = new PendingCombatSelection(nonEnemySlot, AutoCombatCardKind.DefendLike);
            return true;
        }

        return false;
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

    private static GuiSmokeStepDecision? TryCreateScreenshotFirstRoomDecision(GuiSmokeStepRequest request)
    {
        var visibleMapNodeDecision = TryFindFirstReachableMapNodeDecision(request);
        if (visibleMapNodeDecision is not null)
        {
            return visibleMapNodeDecision;
        }

        var restSiteUpgradeDecision = TryCreateRestSiteUpgradeDecision(request);
        if (restSiteUpgradeDecision is not null)
        {
            return restSiteUpgradeDecision;
        }

        var treasureDecision = TryCreateTreasureChestDecision(request);
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

        return TryCreateRestSiteDecision(request)
               ?? TryCreateHiddenOverlayCleanupDecision(request)
               ?? TryCreateOverlayAdvanceDecision(request)
               ?? TryCreateVisibleProceedDecision(request);
    }

    private static bool LooksLikeScreenshotFirstRoomState(GuiSmokeStepRequest request)
    {
        if (LooksLikeTreasureState(request.Observer)
            || LooksLikeRestSiteState(request.Observer)
            || HasOverlayChoiceState(request.Observer))
        {
            return true;
        }

        var mapAnalysis = AutoMapAnalyzer.Analyze(request.ScreenshotPath);
        return mapAnalysis.HasCurrentArrow;
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

    private static GuiSmokeStepDecision? TryFindVisibleMapAdvanceDecision(GuiSmokeStepRequest request)
    {
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
        var choices = request.Observer.CurrentChoices;
        var hasRest = choices.Any(static label => label.Contains("\uD734\uC2DD", StringComparison.OrdinalIgnoreCase) || label.Contains("Rest", StringComparison.OrdinalIgnoreCase));
        var hasSmith = choices.Any(static label => label.Contains("\uC7AC\uB828", StringComparison.OrdinalIgnoreCase) || label.Contains("Smith", StringComparison.OrdinalIgnoreCase));
        if (!hasRest && !hasSmith)
        {
            return null;
        }

        var maxHp = request.Observer.PlayerMaxHp ?? 0;
        var currentHp = request.Observer.PlayerCurrentHp ?? 0;
        var shouldRest = hasRest && (maxHp <= 0 || currentHp <= Math.Ceiling(maxHp * 0.70));
        var targetLabel = shouldRest ? "rest site: rest" : "rest site: smith";
        var normalizedX = shouldRest ? 0.405 : 0.575;
        var normalizedY = 0.305;
        var reason = shouldRest
            ? $"Rest site detected from choices. HP {currentHp}/{maxHp} favors healing."
            : "Rest site detected from choices. HP is healthy enough to prefer smithing.";
        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            targetLabel,
            reason,
            0.86,
            "map",
            1500,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateVisibleProceedDecision(GuiSmokeStepRequest request)
    {
        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(request.ScreenshotPath);
        if (!overlayAnalysis.HasRightProceedArrow || overlayAnalysis.HasBottomLeftBackArrow || overlayAnalysis.HasCentralOverlayPanel)
        {
            return null;
        }

        var proceedNode = request.Observer.ActionNodes.FirstOrDefault(node => node.Actionable && IsProceedNode(node));
        if (proceedNode is not null && TryParseNodeBounds(proceedNode.ScreenBounds, out _))
        {
            return CreateClickDecisionFromNode(request, proceedNode, "visible proceed");
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
                "Overlay controls remain in observer output even though no panel is visible. Retry backdrop dismissal before proceeding.",
                0.72,
                "map",
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
            "Overlay controls remain active behind the visible room. Send cancel/back before trying to proceed.",
            0.84,
            "map",
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

        var recentOverlayBackCount = request.History.Count(entry =>
            string.Equals(entry.Phase, GuiSmokePhase.ChooseFirstNode.ToString(), StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.TargetLabel, "overlay back", StringComparison.OrdinalIgnoreCase));

        if (recentOverlayBackCount >= 3)
        {
            return new GuiSmokeStepDecision(
                "act",
                "press-key",
                "Escape",
                null,
                null,
                "overlay close",
                "Overlay remains open after repeated back clicks. Try cancel/back as a fallback.",
                0.72,
                "map",
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
                "An inspect/compendium overlay is present above the map or rest site. Close it via the visible bottom-left back arrow before trying to progress.",
                0.93,
                "map",
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
            "map",
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
            || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return observer.CurrentChoices.Any(static label =>
            label.Contains("\uD734\uC2DD", StringComparison.OrdinalIgnoreCase)
            || label.Contains("Rest", StringComparison.OrdinalIgnoreCase)
            || label.Contains("\uC7AC\uB828", StringComparison.OrdinalIgnoreCase)
            || label.Contains("Smith", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasOverlayChoiceState(ObserverSummary observer)
    {
        return observer.CurrentChoices.Any(static label =>
            label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
            || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
            || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
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

        var centerX = bounds.X + bounds.Width / 2f;
        var centerY = bounds.Y + bounds.Height / 2f;
        double normalizedX;
        double normalizedY;

        if (HasUsableLogicalBounds(node.ScreenBounds))
        {
            normalizedX = centerX / 1920f;
            normalizedY = centerY / 1080f;
        }
        else if (bounds.Right > request.WindowBounds.X
                 && bounds.Bottom > request.WindowBounds.Y
                 && bounds.X < request.WindowBounds.X + request.WindowBounds.Width
                 && bounds.Y < request.WindowBounds.Y + request.WindowBounds.Height)
        {
            normalizedX = (centerX - request.WindowBounds.X) / request.WindowBounds.Width;
            normalizedY = (centerY - request.WindowBounds.Y) / request.WindowBounds.Height;
        }
        else
        {
            normalizedX = centerX / Math.Max(1d, request.WindowBounds.Width);
            normalizedY = centerY / Math.Max(1d, request.WindowBounds.Height);
        }

        normalizedX = Math.Clamp(normalizedX, 0d, 1d);
        normalizedY = Math.Clamp(normalizedY, 0d, 1d);
        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            targetLabel,
            $"Auto provider selected node '{node.Label}'.",
            0.92,
            null,
            1200,
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

static class AutoRestSiteCardGridAnalyzer
{
    private static readonly AutoRestSiteCardGridAnalysis None = new(false, 0.5d, 0.5d);

    public static AutoRestSiteCardGridAnalysis Analyze(string screenshotPath)
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

static class AutoTreasureAnalyzer
{
    private static readonly AutoTreasureAnalysis None = new(false, false, 0.5d, 0.5d);

    public static AutoTreasureAnalysis Analyze(string screenshotPath)
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

static class AutoCombatAnalyzer
{
    public static AutoCombatAnalysis Analyze(string screenshotPath)
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

static class LongRunArtifacts
{
    public static GuiSmokeSelfMetaReview WriteAttemptMetaReview(
        string sessionRoot,
        string attemptId,
        int attemptOrdinal,
        string runId,
        string status,
        string? resultMessage)
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
        var dominantFailureClass = DetermineDominantFailureClass(currentProgress, status, resultMessage);
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
        string resultMessage)
    {
        var validationPath = Path.Combine(logger.RunRoot, "validation-summary.json");
        GuiSmokeValidationSummary? validationSummary = null;
        if (File.Exists(validationPath))
        {
            validationSummary = JsonSerializer.Deserialize<GuiSmokeValidationSummary>(File.ReadAllText(validationPath), GuiSmokeShared.JsonOptions);
        }

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
            stepCount);

        var attemptIndexPath = Path.Combine(sessionRoot, "attempt-index.ndjson");
        AppendUniqueNdjson(attemptIndexPath, attemptEntry, static existing => existing.RunId, attemptEntry.RunId);
        var attemptEntries = ReadNdjson<GuiSmokeAttemptIndexEntry>(attemptIndexPath);
        var startedAt = attemptEntries.Count == 0
            ? runManifest?.StartedAt ?? DateTimeOffset.UtcNow
            : attemptEntries.Min(static entry => entry.StartedAt);
        var completedAt = attemptEntries.Count == 0
            ? runManifest?.CompletedAt ?? DateTimeOffset.UtcNow
            : attemptEntries
                .Select(static entry => entry.CompletedAt ?? entry.StartedAt)
                .Max();
        var totalSteps = attemptEntries.Sum(static entry => entry.StepCount);
        var passedAttempts = attemptEntries.Count(static entry =>
            string.Equals(entry.Status, "passed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entry.Status, "completed", StringComparison.OrdinalIgnoreCase));
        var failedAttempts = attemptEntries.Count - passedAttempts;
        var validationEvents = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in attemptEntries)
        {
            var attemptValidationPath = Path.Combine(sessionRoot, "attempts", entry.AttemptId, "validation-summary.json");
            if (!File.Exists(attemptValidationPath))
            {
                continue;
            }

            GuiSmokeValidationSummary? attemptValidation;
            try
            {
                attemptValidation = JsonSerializer.Deserialize<GuiSmokeValidationSummary>(File.ReadAllText(attemptValidationPath), GuiSmokeShared.JsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (attemptValidation is null)
            {
                continue;
            }

            foreach (var pair in attemptValidation.EventCounts)
            {
                validationEvents[pair.Key] = validationEvents.TryGetValue(pair.Key, out var current)
                    ? current + pair.Value
                    : pair.Value;
            }
        }

        if (validationEvents.Count == 0 && validationSummary is not null)
        {
            foreach (var pair in validationSummary.EventCounts)
            {
                validationEvents[pair.Key] = pair.Value;
            }
        }

        var sessionSummary = new GuiSmokeSessionSummary(
            SessionId: Path.GetFileName(sessionRoot),
            ScenarioId: scenarioId,
            Provider: providerKind,
            StartedAt: startedAt,
            CompletedAt: completedAt,
            AttemptCount: attemptEntries.Count,
            PassedAttempts: passedAttempts,
            FailedAttempts: failedAttempts,
            TotalSteps: totalSteps,
            ValidationEvents: validationEvents);
        LiveExportAtomicFileWriter.WriteJsonAtomic(Path.Combine(sessionRoot, "session-summary.json"), sessionSummary, GuiSmokeShared.JsonOptions);
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
        string? resultMessage)
    {
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

        var currentScreen = TryReadString(stateDocument?.RootElement, "currentScreen");
        var visibleScreen = TryReadNestedString(stateDocument?.RootElement, "meta", "visibleScreen") ?? currentScreen;
        var inCombat = TryReadBool(stateDocument?.RootElement, "encounter", "inCombat");
        var capturedAt = TryReadDateTimeOffset(stateDocument?.RootElement, "capturedAt");
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

        return new ObserverState(
            new ObserverSummary(currentScreen, visibleScreen, inCombat, capturedAt, inventoryId, sceneReady, sceneAuthority, sceneStability, sceneEpisodeId, encounterKind, choiceExtractorPath, playerCurrentHp, playerMaxHp, playerEnergy, currentChoices, eventLines ?? Array.Empty<string>(), ReadActionNodes(inventoryDocument), ReadChoices(stateDocument), combatHand),
            stateDocument,
            inventoryDocument,
            eventLines);
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

        return File.ReadLines(path)
            .TakeLast(lines)
            .ToArray();
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
            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            choices.Add(new ObserverChoice(kind, label, screenBounds));
        }

        return choices;
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
    public void MoveCursor(WindowCaptureTarget target, double normalizedX, double normalizedY)
    {
        if (target.Handle != IntPtr.Zero)
        {
            NativeMethods.SetForegroundWindow(target.Handle);
            Thread.Sleep(100);
        }

        var point = TransformNormalizedPoint(target, normalizedX, normalizedY);
        NativeMethods.SetCursorPos(point.X, point.Y);
    }

    public void Click(WindowCaptureTarget target, double normalizedX, double normalizedY)
    {
        if (target.Handle != IntPtr.Zero)
        {
            NativeMethods.SetForegroundWindow(target.Handle);
            Thread.Sleep(200);
        }

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

    public void RightClick(WindowCaptureTarget target, double normalizedX, double normalizedY)
    {
        if (target.Handle != IntPtr.Zero)
        {
            NativeMethods.SetForegroundWindow(target.Handle);
            Thread.Sleep(200);
        }

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
        if (target.Handle != IntPtr.Zero)
        {
            NativeMethods.SetForegroundWindow(target.Handle);
            Thread.Sleep(200);
        }

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
