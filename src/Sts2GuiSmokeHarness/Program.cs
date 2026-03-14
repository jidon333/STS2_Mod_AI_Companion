using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
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
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            continue;
        }
        consecutiveBlackFrames = 0;
        LogHarness($"step={stepIndex} captured={screenshotPath}");

        var observer = observerReader.Read();
        logger.WriteObserverCopies(stepPrefix, observer);
        LogHarness($"step={stepIndex} observer logical={observer.CurrentScreen ?? "null"} visible={observer.VisibleScreen ?? "null"} inCombat={observer.InCombat?.ToString() ?? "null"} capturedAt={observer.CapturedAt?.ToString("O") ?? "null"}");

        if (!observer.IsFreshSince(freshnessFloor))
        {
            LogHarness($"step={stepIndex} stale observer snapshot ignored freshnessFloor={freshnessFloor:O}");
            logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "stale-observer", observer.CurrentScreen, observer.InCombat, null));
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
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
                GuiSmokePhase.WaitCombat => GuiSmokePhase.Completed,
                _ => phase,
            };

            if (phase == GuiSmokePhase.Completed)
            {
                logger.CompleteRun("passed", "combat accepted by observer");
                return 0;
            }

            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            continue;
        }

        if (TryAdvanceAlternateBranch(phase, observer, history, logger, stepIndex, out var alternatePhase))
        {
            LogHarness($"step={stepIndex} alternate branch {phase} -> {alternatePhase} from screen={observer.CurrentScreen ?? "null"}");
            phase = alternatePhase;

            if (phase == GuiSmokePhase.Completed)
            {
                logger.CompleteRun("passed", "combat accepted by observer");
                return 0;
            }

            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            continue;
        }

        if (IsPassiveWaitPhase(phase))
        {
            var waitAttempt = IncrementAttempt(attemptsByPhase, phase);
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
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
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
        LogHarness($"step={stepIndex} decision status={decision.Status} action={decision.ActionKind ?? "null"} target={decision.TargetLabel ?? "null"} confidence={decision.Confidence?.ToString("0.00") ?? "null"}");

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
            await Task.Delay(Math.Max(2000, decision.WaitMs ?? 2000)).ConfigureAwait(false);
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
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            continue;
        }

        var clickPoint = MouseInputDriver.TransformNormalizedPoint(clickWindow, decision.NormalizedX!.Value, decision.NormalizedY!.Value);
        LogHarness($"step={stepIndex} click target={decision.TargetLabel ?? "null"} normalized=({decision.NormalizedX:0.000},{decision.NormalizedY:0.000}) absolute=({clickPoint.X},{clickPoint.Y}) bounds={DescribeBounds(clickWindow.Bounds)}");
        inputDriver.Click(clickWindow, decision.NormalizedX!.Value, decision.NormalizedY!.Value);
        history.Add(new GuiSmokeHistoryEntry(phase.ToString(), "click", decision.TargetLabel, DateTimeOffset.UtcNow));
        logger.AppendTrace(new GuiSmokeTraceEntry(DateTimeOffset.UtcNow, stepIndex, phase.ToString(), "click", observer.CurrentScreen, observer.InCombat, decision.TargetLabel));
        LogHarness($"step={stepIndex} click sent target={decision.TargetLabel ?? "null"}");

        phase = phase switch
        {
            GuiSmokePhase.EnterRun => GuiSmokePhase.WaitCharacterSelect,
            GuiSmokePhase.ChooseCharacter => GuiSmokePhase.Embark,
            GuiSmokePhase.Embark => GuiSmokePhase.WaitMap,
            GuiSmokePhase.HandleRewards => GuiSmokePhase.WaitMap,
            GuiSmokePhase.ChooseFirstNode => GuiSmokePhase.WaitCombat,
            _ => phase,
        };

        attemptsByPhase.Clear();
        LogHarness($"step={stepIndex} next phase={phase}");
        await Task.Delay(Math.Max(2000, decision.WaitMs ?? 2000)).ConfigureAwait(false);
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
        "screen.png",
        new WindowBounds(100, 200, 1000, 800),
        new ObserverSummary("main-menu", "main-menu", false, DateTimeOffset.UtcNow, null, new[] { "Continue" }, new[] { "main-menu" }, Array.Empty<ObserverActionNode>()),
        new[] { "click continue", "click singleplayer" },
        Array.Empty<GuiSmokeHistoryEntry>(),
        "menu entry");
    var json = JsonSerializer.Serialize(request, GuiSmokeShared.JsonOptions);
    var roundTrip = JsonSerializer.Deserialize<GuiSmokeStepRequest>(json, GuiSmokeShared.JsonOptions);
    Assert(roundTrip?.Phase == GuiSmokePhase.EnterRun.ToString(), "Request should round-trip.");

    var decision = new GuiSmokeStepDecision("act", "click", 0.3, 0.7, "continue", "main menu continue", 0.9, "character-select", 1000, true, null);
    ValidateDecision(
        GuiSmokePhase.EnterRun,
        request,
        decision);

    var evaluator = new ObserverAcceptanceEvaluator();
    Assert(
        evaluator.IsPhaseSatisfied(
            GuiSmokePhase.WaitCombat,
            new ObserverState(new ObserverSummary("combat", "combat", true, DateTimeOffset.UtcNow, null, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ObserverActionNode>()), null, null, null)),
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
        screenshotPath,
        new WindowBounds(window.Bounds.X, window.Bounds.Y, window.Bounds.Width, window.Bounds.Height),
        observer.Summary,
        GetAllowedActions(phase),
        history.TakeLast(5).ToArray(),
        BuildFailureModeHint(phase));
}

static string[] GetAllowedActions(GuiSmokePhase phase)
{
    return phase switch
    {
        GuiSmokePhase.EnterRun => new[] { "click continue", "click singleplayer", "wait" },
        GuiSmokePhase.ChooseCharacter => new[] { "click ironclad", "wait" },
        GuiSmokePhase.Embark => new[] { "click embark", "wait" },
        GuiSmokePhase.HandleRewards => new[] { "click proceed", "click reward", "wait" },
        GuiSmokePhase.ChooseFirstNode => new[] { "click first reachable node", "wait" },
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
        GuiSmokePhase.WaitCombat => "Wait until observer currentScreen=combat and encounter.inCombat=true.",
        _ => "Complete the scenario.",
    };
}

static string BuildFailureModeHint(GuiSmokePhase phase)
{
    return phase switch
    {
        GuiSmokePhase.EnterRun => "Continue may be absent. Use Singleplayer only if Continue is not visible.",
        GuiSmokePhase.ChooseCharacter => "Do not click Embark before Ironclad is selected.",
        GuiSmokePhase.HandleRewards => "Prefer the proceed arrow when the reward can be skipped; otherwise pick a valid reward card.",
        GuiSmokePhase.ChooseFirstNode => "Do not click non-reachable map nodes.",
        GuiSmokePhase.WaitCombat => "Observer must end with combat screen and inCombat=true.",
        _ => "Fail closed when screenshot and observer disagree.",
    };
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
            nextPhase = GuiSmokePhase.Completed;
            return true;
        }
    }

    if (phase == GuiSmokePhase.WaitMainMenu)
    {
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
            nextPhase = GuiSmokePhase.Completed;
            return true;
        }
    }

    if (phase == GuiSmokePhase.WaitMap)
    {
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
            nextPhase = GuiSmokePhase.Completed;
            return true;
        }
    }

    if (phase == GuiSmokePhase.ChooseFirstNode || phase == GuiSmokePhase.WaitCombat)
    {
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

static void LogHarness(string message)
{
    Console.WriteLine($"[gui-smoke {DateTimeOffset.Now:HH:mm:ss}] {message}");
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

    if (!string.Equals(decision.ActionKind, "click", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Only click actionKind is supported.");
    }

    if (decision.NormalizedX is null || decision.NormalizedY is null)
    {
        throw new InvalidOperationException("Click decision requires normalized coordinates.");
    }

    if (decision.NormalizedX < 0 || decision.NormalizedX > 1 || decision.NormalizedY < 0 || decision.NormalizedY > 1)
    {
        throw new InvalidOperationException("Normalized coordinates must be within [0,1].");
    }

    if (request.AllowedActions.Length == 1 && string.Equals(request.AllowedActions[0], "wait", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Phase {phase} does not allow clicks.");
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

static class GuiSmokeShared
{
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
    string ScreenshotPath,
    WindowBounds WindowBounds,
    ObserverSummary Observer,
    string[] AllowedActions,
    IReadOnlyList<GuiSmokeHistoryEntry> History,
    string FailureModeHint);

sealed record GuiSmokeStepDecision(
    string Status,
    string? ActionKind,
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
    IReadOnlyList<string> CurrentChoices,
    IReadOnlyList<string> LastEventsTail,
    IReadOnlyList<ObserverActionNode> ActionNodes);

sealed record ObserverActionNode(
    string NodeId,
    string Kind,
    string Label,
    string? ScreenBounds,
    bool Actionable);

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

sealed class ArtifactRecorder
{
    private readonly string _runRoot;
    private readonly string _tracePath;
    private readonly string _manifestPath;

    public ArtifactRecorder(string runRoot)
    {
        _runRoot = runRoot;
        _tracePath = Path.Combine(runRoot, "trace.ndjson");
        _manifestPath = Path.Combine(runRoot, "run.json");
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
        var currentChoices = ReadChoiceLabels(stateDocument);

        return new ObserverState(
            new ObserverSummary(currentScreen, visibleScreen, inCombat, capturedAt, inventoryId, currentChoices, eventLines ?? Array.Empty<string>(), ReadActionNodes(inventoryDocument)),
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

    public static Point TransformNormalizedPoint(WindowCaptureTarget target, double normalizedX, double normalizedY)
    {
        var clampedX = Math.Clamp(normalizedX, 0d, 1d);
        var clampedY = Math.Clamp(normalizedY, 0d, 1d);
        var pixelX = target.Bounds.X + (int)Math.Round((target.Bounds.Width - 1) * clampedX);
        var pixelY = target.Bounds.Y + (int)Math.Round((target.Bounds.Height - 1) * clampedY);
        return new Point(pixelX, pixelY);
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
    private const uint INPUT_MOUSE = 0;
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
