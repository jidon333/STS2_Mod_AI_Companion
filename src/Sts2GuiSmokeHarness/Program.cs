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
    const int PassiveWaitMs = 1000;
    const int DecisionWaitMinimumMs = 750;
    const int ActionSettleMinimumMs = 900;
    const int TransitionSettleMs = 2000;

    var scenarioId = options.TryGetValue("--scenario", out var scenarioRaw)
        ? scenarioRaw
        : "boot-to-combat";
    if (!string.Equals(scenarioId, "boot-to-combat", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Unsupported scenario: {scenarioId}");
    }

    var providerKind = options.TryGetValue("--provider", out var providerRaw)
        ? providerRaw
        : "session";
    var liveLayout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
    var harnessLayout = HarnessPathResolver.Resolve(configuration.GamePaths, configuration.Harness);
    var runId = $"{DateTimeOffset.Now:yyyyMMdd-HHmmss}-{scenarioId}";
    var runRoot = options.TryGetValue("--run-root", out var explicitRunRoot)
        ? Path.GetFullPath(explicitRunRoot, workspaceRoot)
        : Path.Combine(workspaceRoot, "artifacts", "gui-smoke", runId);

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

    if (!options.ContainsKey("--skip-launch"))
    {
        EnsureGameNotRunning();
        var launchIssuedAt = DateTimeOffset.UtcNow;
        await GuiSmokeShared.RunProcessAsync(
            Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
            "/c start \"\" \"steam://rungameid/2868840\"",
            workspaceRoot,
            TimeSpan.FromSeconds(10),
            waitForExit: false).ConfigureAwait(false);

        await WaitForLiveGameWindowAsync(launchIssuedAt, TimeSpan.FromMinutes(2)).ConfigureAwait(false);
        await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
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
    var stepIndex = 0;
    var attemptsByPhase = new Dictionary<GuiSmokePhase, int>();
    var history = new List<GuiSmokeHistoryEntry>();
    var waitDeadline = DateTimeOffset.UtcNow.AddMinutes(10);
    var freshnessFloor = DateTimeOffset.UtcNow.AddSeconds(-5);
    var consecutiveBlackFrames = 0;
    string? lastActionFingerprint = null;
    var sameActionStallCount = 0;
    var decisionStaleBudget = TimeSpan.FromSeconds(2);

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

        if (!observer.IsFreshSince(freshnessFloor))
        {
            LogHarness($"step={stepIndex} stale observer snapshot ignored freshnessFloor={freshnessFloor:O}");
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "stale-observer", observer.CurrentScreen, observer.InCombat, null));
            await Task.Delay(PassiveWaitMs).ConfigureAwait(false);
            continue;
        }

        if (evaluator.IsPhaseSatisfied(phase, observer))
        {
            LogHarness($"step={stepIndex} observer accepted phase={phase}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "observer-accepted", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "observer-accepted", observer.CurrentScreen, observer.InCombat, null));
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
                logger.CompleteRun("passed", "combat flow accepted by observer");
                return 0;
            }

            await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
            continue;
        }

        if (TryAdvanceAlternateBranch(phase, observer, history, logger, stepIndex, out var alternatePhase))
        {
            LogHarness($"step={stepIndex} alternate branch {phase} -> {alternatePhase} from screen={observer.CurrentScreen ?? "null"}");
            phase = alternatePhase;

            if (phase == GuiSmokePhase.Completed)
            {
                logger.CompleteRun("passed", "combat flow accepted by observer");
                return 0;
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
                logger.CompleteRun("failed", $"timeout at {phase}");
                return 1;
            }

            LogHarness($"step={stepIndex} waiting phase={phase} attempt={waitAttempt} screen={observer.CurrentScreen ?? "null"}");
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "waiting", observer.CurrentScreen, observer.InCombat, null));
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
            history);
        var requestPath = stepPrefix + ".request.json";
        var decisionPath = stepPrefix + ".decision.json";
        logger.WriteRequest(requestPath, request);
        LogHarness($"step={stepIndex} request={requestPath}");
        var decision = await provider.GetDecisionAsync(requestPath, decisionPath, TimeSpan.FromMinutes(3), CancellationToken.None).ConfigureAwait(false);
        logger.WriteDecision(decisionPath, decision);
        LogHarness($"step={stepIndex} decision status={decision.Status} action={decision.ActionKind ?? "null"} target={decision.TargetLabel ?? "null"} confidence={decision.Confidence?.ToString("0.00") ?? "null"} reason={decision.Reason ?? "null"}");

        if (string.Equals(decision.Status, "abort", StringComparison.OrdinalIgnoreCase))
        {
            LogHarness($"step={stepIndex} aborted reason={decision.AbortReason ?? "null"}");
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                decision.AbortReason ?? "decision aborted",
                observer.CurrentScreen,
                observer.InCombat,
                screenshotPath));
            logger.CompleteRun("failed", decision.AbortReason ?? "decision aborted");
            return 1;
        }

        if (string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase))
        {
            LogHarness($"step={stepIndex} wait requested ms={Math.Max(250, decision.WaitMs ?? 1000)}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "wait", decision.TargetLabel, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "wait", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
            await Task.Delay(Math.Max(DecisionWaitMinimumMs, decision.WaitMs ?? DecisionWaitMinimumMs)).ConfigureAwait(false);
            continue;
        }

        var decisionAge = DateTimeOffset.UtcNow - request.IssuedAt;
        if (decisionAge > decisionStaleBudget)
        {
            LogHarness($"step={stepIndex} recapture required stale-decision ageMs={(int)decisionAge.TotalMilliseconds}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "stale-decision", decision.TargetLabel, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "stale-decision", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
            await Task.Delay(DecisionWaitMinimumMs).ConfigureAwait(false);
            continue;
        }

        var latestObserver = observerReader.Read();
        if (ShouldRecaptureForObserverDrift(request.Observer, latestObserver))
        {
            LogHarness($"step={stepIndex} recapture required observer-drift requestScreen={request.Observer.CurrentScreen ?? "null"} latestScreen={latestObserver.CurrentScreen ?? "null"} requestVisible={request.Observer.VisibleScreen ?? "null"} latestVisible={latestObserver.VisibleScreen ?? "null"}");
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "observer-drift", decision.TargetLabel, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "observer-drift", latestObserver.CurrentScreen, latestObserver.InCombat, decision.TargetLabel));
            await Task.Delay(DecisionWaitMinimumMs).ConfigureAwait(false);
            continue;
        }

        ValidateDecision(phase, request, decision);
        var actionFingerprint = string.Join("|",
            phase.ToString(),
            observer.CurrentScreen ?? "null",
            observer.VisibleScreen ?? "null",
            observer.InventoryId ?? "null",
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

        if (sameActionStallCount >= 2)
        {
            var abortReason = $"same-action-stall phase={phase} target={decision.TargetLabel ?? "null"} screen={observer.CurrentScreen ?? "null"} inventory={observer.InventoryId ?? "null"}";
            LogHarness($"step={stepIndex} abort {abortReason}");
            logger.WriteFailureSummary(new GuiSmokeFailureSummary(
                phase.ToString(),
                abortReason,
                observer.CurrentScreen,
                observer.InCombat,
                screenshotPath));
            logger.CompleteRun("failed", abortReason);
            return 1;
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
            await Task.Delay(TransitionSettleMs).ConfigureAwait(false);
            continue;
        }

        if (string.Equals(decision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase))
        {
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

        phase = phase switch
        {
            GuiSmokePhase.EnterRun => GuiSmokePhase.WaitCharacterSelect,
            GuiSmokePhase.ChooseCharacter => GuiSmokePhase.Embark,
            GuiSmokePhase.Embark => GuiSmokePhase.WaitMap,
            GuiSmokePhase.HandleRewards => GetPostRewardPhase(decision),
            GuiSmokePhase.ChooseFirstNode => GuiSmokePhase.WaitCombat,
            GuiSmokePhase.HandleEvent => GuiSmokePhase.HandleEvent,
            GuiSmokePhase.HandleCombat => GuiSmokePhase.HandleCombat,
            _ => phase,
        };

        attemptsByPhase.Clear();
        LogHarness($"step={stepIndex} next phase={phase}");
        await Task.Delay(Math.Max(ActionSettleMinimumMs, decision.WaitMs ?? ActionSettleMinimumMs)).ConfigureAwait(false);
    }

    logger.WriteFailureSummary(new GuiSmokeFailureSummary(
        phase.ToString(),
        "Global timeout reached.",
        observerReader.Read().CurrentScreen,
        observerReader.Read().InCombat,
        null));
    logger.CompleteRun("failed", "global timeout");
    return 1;
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
        new ObserverSummary("main-menu", "main-menu", false, DateTimeOffset.UtcNow, null, null, null, null, null, new[] { "Continue" }, new[] { "main-menu" }, Array.Empty<ObserverActionNode>(), Array.Empty<ObserverChoice>()),
        new[] { "click continue", "click singleplayer" },
        Array.Empty<GuiSmokeHistoryEntry>(),
        "menu entry");
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
            null,
            null,
            30,
            80,
            new[] { "skip" },
            Array.Empty<string>(),
            new[]
            {
                new ObserverActionNode("reward:0", "reward-item", "Reward Card", "100,200,120,120", true),
                new ObserverActionNode("reward:1", "button", "Proceed", "900,700,240,90", true),
            },
            Array.Empty<ObserverChoice>()),
        AllowedActions = new[] { "click proceed", "click reward", "wait" },
    };
    var autoDecision = AutoDecisionProvider.Decide(autoRewardRequest);
    Assert(autoDecision.ActionKind == "click", "Auto provider should choose a click for reward handling.");
    Assert(autoDecision.TargetLabel == "claim reward item", "Auto provider should prefer the reward item before proceed.");

    var evaluator = new ObserverAcceptanceEvaluator();
    Assert(
        evaluator.IsPhaseSatisfied(
            GuiSmokePhase.WaitCombat,
            new ObserverState(new ObserverSummary("combat", "combat", true, DateTimeOffset.UtcNow, null, null, null, null, null, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ObserverActionNode>(), Array.Empty<ObserverChoice>()), null, null, null)),
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
    IReadOnlyList<GuiSmokeHistoryEntry> history)
{
    return new GuiSmokeStepRequest(
        runId,
        scenarioId,
        stepIndex,
        phase.ToString(),
        BuildGoal(phase),
        DateTimeOffset.UtcNow,
        screenshotPath,
        new WindowBounds(window.Bounds.X, window.Bounds.Y, window.Bounds.Width, window.Bounds.Height),
        observer.Summary,
        GetAllowedActions(phase, observer),
        history.TakeLast(5).ToArray(),
        BuildFailureModeHint(phase, observer));
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
        GuiSmokePhase.ChooseFirstNode => new[] { "click first reachable node", "wait" },
        GuiSmokePhase.HandleEvent => new[] { "click first event option", "wait" },
        GuiSmokePhase.HandleCombat => new[] { "click card", "click enemy", "click end turn", "wait" },
        _ => new[] { "wait" },
    };
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
        GuiSmokePhase.ChooseFirstNode => "Do not click non-reachable map nodes.",
        GuiSmokePhase.HandleEvent => "If the event text is ambiguous, choose the first visible option.",
        GuiSmokePhase.WaitCombat => "Observer must end with combat screen and inCombat=true.",
        GuiSmokePhase.HandleCombat => "AI first: read the full combat board from the screenshot. Cards, targets, energy, and end-turn are visual decisions. The harness only executes the click you choose.",
        _ => "Fail closed when screenshot and observer disagree.",
    };
}

static GuiSmokePhase GetPostRewardPhase(GuiSmokeStepDecision decision)
{
    if (string.Equals(decision.TargetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase))
    {
        return GuiSmokePhase.WaitCombat;
    }

    return GuiSmokePhase.WaitMap;
}

static bool IsPassiveWaitPhase(GuiSmokePhase phase)
{
    return phase is GuiSmokePhase.WaitMainMenu
        or GuiSmokePhase.WaitCharacterSelect
        or GuiSmokePhase.WaitMap
        or GuiSmokePhase.WaitCombat;
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
            nextPhase = GuiSmokePhase.Completed;
            return true;
        }

        if (string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase) && observer.InCombat != true)
        {
            history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "combat-resolved-map", null, DateTimeOffset.UtcNow));
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "combat-resolved-map", observer.CurrentScreen, observer.InCombat, null));
            nextPhase = GuiSmokePhase.Completed;
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
    var hp = observer.Summary.PlayerCurrentHp is not null && observer.Summary.PlayerMaxHp is not null
        ? $"{observer.Summary.PlayerCurrentHp}/{observer.Summary.PlayerMaxHp}"
        : "null";
    var encounter = observer.EncounterKind ?? "null";
    var extractor = observer.ChoiceExtractorPath ?? "null";
    var choices = observer.Summary.CurrentChoices.Take(4).ToArray();
    var choiceText = choices.Length == 0 ? "-" : string.Join(", ", choices);
    return $"logical={logical} visible={visible} inCombat={inCombat} hp={hp} encounter={encounter} extractor={extractor} choices=[{choiceText}]";
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
    Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- run --scenario boot-to-combat --provider session|headless [--provider-command \"<cmd>\"] [--config path]");
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

sealed record GuiSmokeFailureSummary(
    string Phase,
    string Message,
    string? ObserverScreen,
    bool? ObserverInCombat,
    string? ScreenshotPath);

sealed record GuiSmokeTraceEntry(
    DateTimeOffset RecordedAt,
    int StepIndex,
    string Phase,
    string EventKind,
    string? ObserverScreen,
    bool? ObserverInCombat,
    string? TargetLabel);

sealed record GuiSmokeStepRequest(
    string RunId,
    string ScenarioId,
    int StepIndex,
    string Phase,
    string Goal,
    DateTimeOffset IssuedAt,
    string ScreenshotPath,
    WindowBounds WindowBounds,
    ObserverSummary Observer,
    string[] AllowedActions,
    IReadOnlyList<GuiSmokeHistoryEntry> History,
    string FailureModeHint);

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
    string? AbortReason);

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
    string? EncounterKind,
    string? ChoiceExtractorPath,
    int? PlayerCurrentHp,
    int? PlayerMaxHp,
    IReadOnlyList<string> CurrentChoices,
    IReadOnlyList<string> LastEventsTail,
    IReadOnlyList<ObserverActionNode> ActionNodes,
    IReadOnlyList<ObserverChoice> Choices);

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
    public IReadOnlyList<ObserverActionNode> ActionNodes => Summary.ActionNodes;
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
        if (string.Equals(request.Observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase))
        {
            var mapDecision = TryFindVisibleMapAdvanceDecision(request)
                              ?? TryFindFirstReachableMapNodeDecision(request);
            if (mapDecision is not null)
            {
                return mapDecision;
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
        var visibleMapNodeDecision = TryFindFirstReachableMapNodeDecision(request);
        if (visibleMapNodeDecision is not null)
        {
            return visibleMapNodeDecision;
        }

        if (LooksLikeShopState(request.Observer))
        {
            return DecideHandleShop(request);
        }

        return TryCreateRestSiteDecision(request)
               ?? TryCreateHiddenOverlayCleanupDecision(request)
               ?? TryCreateOverlayAdvanceDecision(request)
               ?? TryCreateVisibleProceedDecision(request)
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
        var mapAnalysis = AutoMapAnalyzer.Analyze(request.ScreenshotPath);
        if (mapAnalysis.HasCurrentArrow && mapAnalysis.HasReachableNode)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                Math.Clamp(mapAnalysis.ReachableNodeNormalizedX, 0.08f, 0.92f),
                Math.Clamp(mapAnalysis.ReachableNodeNormalizedY, 0.10f, 0.86f),
                "first reachable node",
                "Visible scene is already the map even though observer still reports event. Follow the directly connected reachable node from the current arrow.",
                0.95,
                "map",
                1400,
                true,
                null);
        }

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
            null);
    }

    private static GuiSmokeStepDecision DecideHandleCombat(GuiSmokeStepRequest request)
    {
        var analysis = AutoCombatAnalyzer.Analyze(request.ScreenshotPath);
        if (analysis.HasTargetArrow)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.744,
                0.542,
                "auto-target enemy",
                "A targeting arrow is visible. Resolve the selected attack on the enemy body.",
                0.93,
                "combat",
                1200,
                true,
                null);
        }

        if (analysis.HasSelectedCard)
        {
            if (analysis.SelectedCardKind == AutoCombatCardKind.DefendLike)
            {
                return new GuiSmokeStepDecision(
                    "act",
                    "click",
                    null,
                    0.500,
                    0.770,
                    "confirm selected defend",
                    "A selected blue defense card is visible. Click the selected card again to resolve it.",
                    0.82,
                    "combat",
                    1200,
                    true,
                    null);
            }

            return new GuiSmokeStepDecision(
                "act",
                "right-click",
                null,
                null,
                null,
                "cancel unresolved selected card",
                "A selected card is visible without a targeting arrow. Cancel the selection and continue.",
                0.75,
                "combat",
                800,
                true,
                null);
        }

        var triedHotkeys = request.History
            .Where(entry => string.Equals(entry.Action, "press-key", StringComparison.OrdinalIgnoreCase))
            .Select(entry => ExtractFirstDigit(entry.TargetLabel))
            .Where(static digit => digit is not null)
            .Select(static digit => digit!.Value)
            .Distinct()
            .ToHashSet();

        var nextHotkey = Enumerable.Range(1, 5).FirstOrDefault(slot => !triedHotkeys.Contains(slot));
        if (nextHotkey != 0)
        {
            return new GuiSmokeStepDecision(
                "act",
                "press-key",
                nextHotkey.ToString(CultureInfo.InvariantCulture),
                null,
                null,
                $"auto-select slot {nextHotkey}",
                "No selected card is visible. Select the next unseen hand slot by hotkey.",
                0.70,
                "combat",
                1000,
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
            "No productive combat action remains. End the turn.",
            0.88,
            "combat",
            1500,
            true,
            null);
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

        if (!analysis.HasCurrentArrow || LooksLikeShopState(request.Observer))
        {
            return null;
        }

        var node = request.Observer.ActionNodes
            .Where(node => node.Actionable && IsMapNode(node) && HasUsableLogicalBounds(node.ScreenBounds))
            .OrderBy(node => TryParseNodeBounds(node.ScreenBounds, out var bounds) ? bounds.X : float.MaxValue)
            .ThenBy(node => TryParseNodeBounds(node.ScreenBounds, out var bounds) ? bounds.Y : float.MaxValue)
            .FirstOrDefault();
        return node is null ? null : CreateClickDecisionFromNode(request, node, "first reachable node");
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

static class AutoMapAnalyzer
{
    public static AutoMapAnalysis Analyze(string screenshotPath)
    {
        using var bitmap = new Bitmap(screenshotPath);
        var samples = new List<Point>();

        var yStart = (int)(bitmap.Height * 0.30);
        var yEnd = (int)(bitmap.Height * 0.92);
        var xStart = (int)(bitmap.Width * 0.12);
        var xEnd = (int)(bitmap.Width * 0.88);

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
        var reachableNode = TryFindReachableNode(bitmap, centroidX, centroidY);
        return new AutoMapAnalysis(
            true,
            (float)(centroidX / bitmap.Width),
            (float)(centroidY / bitmap.Height),
            reachableNode is not null,
            reachableNode is null ? 0.5f : (float)(reachableNode.Value.X / bitmap.Width),
            reachableNode is null ? 0.5f : (float)(reachableNode.Value.Y / bitmap.Height));
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
            .Where(entry => entry.centroidY > currentArrowY + 32)
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

sealed record AutoCombatAnalysis(
    bool HasSelectedCard,
    bool HasTargetArrow,
    AutoCombatCardKind SelectedCardKind);

static class AutoCombatAnalyzer
{
    public static AutoCombatAnalysis Analyze(string screenshotPath)
    {
        using var bitmap = new Bitmap(screenshotPath);
        var hasSelectedCard = HasSelectedCard(bitmap);
        var hasTargetArrow = hasSelectedCard && HasTargetArrow(bitmap);
        var selectedCardKind = hasSelectedCard ? ClassifySelectedCard(bitmap) : AutoCombatCardKind.Unknown;
        return new AutoCombatAnalysis(hasSelectedCard, hasTargetArrow, selectedCardKind);
    }

    private static bool HasSelectedCard(Bitmap bitmap)
    {
        var sample = AverageColor(bitmap, 0.43, 0.63, 0.57, 0.90);
        return sample.Brightness > 55;
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

    private static AutoCombatCardKind ClassifySelectedCard(Bitmap bitmap)
    {
        var sample = AverageColor(bitmap, 0.44, 0.66, 0.56, 0.88);
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
}

sealed class ArtifactRecorder
{
    private readonly string _runRoot;
    private readonly string _tracePath;
    private readonly string _manifestPath;
    private readonly string _humanLogPath;

    public ArtifactRecorder(string runRoot)
    {
        _runRoot = runRoot;
        _tracePath = Path.Combine(runRoot, "trace.ndjson");
        _manifestPath = Path.Combine(runRoot, "run.json");
        _humanLogPath = Path.Combine(runRoot, "run.log");
    }

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
            JsonSerializer.Serialize(entry, GuiSmokeShared.JsonOptions) + Environment.NewLine);
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

    public void AppendHumanLog(string line)
    {
        LiveExportAtomicFileWriter.AppendAllTextShared(
            _humanLogPath,
            line + Environment.NewLine);
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
        var encounterKind = TryReadNestedString(stateDocument?.RootElement, "encounter", "kind");
        var choiceExtractorPath = TryReadNestedString(stateDocument?.RootElement, "meta", "choiceExtractorPath");
        var playerCurrentHp = TryReadInt32(stateDocument?.RootElement, "player", "currentHp");
        var playerMaxHp = TryReadInt32(stateDocument?.RootElement, "player", "maxHp");
        var currentChoices = ReadChoiceLabels(stateDocument);

        return new ObserverState(
            new ObserverSummary(currentScreen, visibleScreen, inCombat, capturedAt, inventoryId, encounterKind, choiceExtractorPath, playerCurrentHp, playerMaxHp, currentChoices, eventLines ?? Array.Empty<string>(), ReadActionNodes(inventoryDocument), ReadChoices(stateDocument)),
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
        return phase switch
        {
            GuiSmokePhase.WaitMainMenu => string.Equals(observer.CurrentScreen, "main-menu", StringComparison.OrdinalIgnoreCase),
            GuiSmokePhase.WaitCharacterSelect => string.Equals(observer.CurrentScreen, "character-select", StringComparison.OrdinalIgnoreCase),
            GuiSmokePhase.WaitMap => string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase),
            GuiSmokePhase.WaitCombat => string.Equals(observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase)
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
