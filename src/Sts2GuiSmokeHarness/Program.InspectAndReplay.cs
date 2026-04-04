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
using static ObserverScreenProvenance;
using static GuiSmokeChoicePrimitiveSupport;
using static GuiSmokeFixtureIoSupport;
using static GuiSmokePromptContractSupport;
using static GuiSmokeReplayArtifactSupport;
using static GuiSmokeSceneReasoningSupport;
using static GuiSmokeStepRequestFactory;

internal static partial class Program
{
    static int ReplayAdvisorScene(IReadOnlyDictionary<string, string> options, string workspaceRoot)
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

        var request = TryReadJson<GuiSmokeStepRequest>(resolvedRequestPath)
                      ?? throw new InvalidOperationException($"Failed to parse replay request '{resolvedRequestPath}'.");
        if (!Enum.TryParse<GuiSmokePhase>(request.Phase, ignoreCase: true, out var phase))
        {
            throw new InvalidOperationException($"Unsupported replay phase '{request.Phase}'.");
        }

        var stepPrefix = resolvedRequestPath.EndsWith(".request.json", StringComparison.OrdinalIgnoreCase)
            ? resolvedRequestPath[..^".request.json".Length]
            : Path.Combine(Path.GetDirectoryName(resolvedRequestPath) ?? string.Empty, Path.GetFileNameWithoutExtension(resolvedRequestPath));
        var statePath = stepPrefix + ".observer.state.json";
        var stateDocument = TryLoadJsonDocument(statePath)
                            ?? throw new FileNotFoundException("Observer state sidecar not found.", statePath);
        var eventTailPath = stepPrefix + ".observer.events.tail.json";
        var eventLines = TryReadJson<string[]>(eventTailPath);
        var observer = ObserverSnapshotReader.CreateReplayObserverState(stateDocument, eventLines);
        var analysisContext = CreateStepAnalysisContext(
            phase,
            observer,
            request.ScreenshotPath,
            request.History ?? Array.Empty<GuiSmokeHistoryEntry>(),
            request.CombatCardKnowledge ?? Array.Empty<CombatCardKnowledgeHint>(),
            request.WindowBounds);
        var artifact = GuiSmokeAdvisorSceneModelBuilder.Build(request, observer, analysisContext, resolvedRequestPath);
        var serialized = JsonSerializer.Serialize(artifact, GuiSmokeShared.JsonOptions);
        if (options.TryGetValue("--out", out var outputPath))
        {
            var resolvedOutputPath = ResolveCliPath(outputPath, workspaceRoot);
            Directory.CreateDirectory(Path.GetDirectoryName(resolvedOutputPath) ?? workspaceRoot);
            File.WriteAllText(resolvedOutputPath, serialized);
        }

        Console.WriteLine(serialized);
        return 0;
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

    static int ReplayParityScenes(IReadOnlyDictionary<string, string> options, string workspaceRoot)
    {
        var suitePath = options.TryGetValue("--suite", out var explicitSuitePath)
            ? ResolveCliPath(explicitSuitePath, workspaceRoot)
            : Path.Combine(workspaceRoot, "tests", "replay-fixtures", "gui-smoke-parity-scenes.json");
        if (!File.Exists(suitePath))
        {
            throw new FileNotFoundException("Replay parity suite not found.", suitePath);
        }

        var fixtures = JsonSerializer.Deserialize<IReadOnlyList<GuiSmokeReplayParityFixture>>(File.ReadAllText(suitePath), GuiSmokeShared.JsonOptions)
                       ?? throw new InvalidOperationException("Failed to parse replay parity suite.");
        var results = new List<object>(fixtures.Count);
        var traceEnabled = options.ContainsKey("--trace");
        for (var index = 0; index < fixtures.Count; index += 1)
        {
            var fixture = fixtures[index];
            var trace = new GuiSmokeReplayTracer($"replay-parity {index + 1}/{fixtures.Count}:{fixture.Name}", traceEnabled);
            var requestPath = ResolveCliPath(fixture.RequestPath, workspaceRoot);
            trace.Info($"start request={requestPath}");

            var lightweightLoad = trace.Measure(
                "load-request-lightweight",
                () => LoadReplayRequest(requestPath, trace, fullRequestRebuild: false),
                "lightweight-request",
                alwaysLog: true);
            var rebuiltLoad = trace.Measure(
                "load-request-rebuilt",
                () => LoadReplayRequest(requestPath, trace, fullRequestRebuild: true),
                "full-request-rebuild",
                alwaysLog: true);
            var lightweightEvaluation = trace.Measure(
                "analyze-lightweight",
                () => EvaluateAutoDecisionWithDiagnostics(requestPath, lightweightLoad.Request),
                Path.GetFileName(lightweightLoad.Request.ScreenshotPath),
                alwaysLog: true);
            var rebuiltEvaluation = trace.Measure(
                "analyze-rebuilt",
                () => EvaluateAutoDecisionWithDiagnostics(requestPath, rebuiltLoad.Request),
                Path.GetFileName(rebuiltLoad.Request.ScreenshotPath),
                alwaysLog: true);

            var lightweightSummary = BuildReplayParityDecisionSummary(lightweightLoad.Request, lightweightEvaluation.Decision);
            var rebuiltSummary = BuildReplayParityDecisionSummary(rebuiltLoad.Request, rebuiltEvaluation.Decision);
            var lightweightAllowedActionKey = BuildReplayParityAllowedActionKey(lightweightLoad.Request.AllowedActions);
            var rebuiltAllowedActionKey = BuildReplayParityAllowedActionKey(rebuiltLoad.Request.AllowedActions);
            Assert(string.Equals(lightweightSummary, rebuiltSummary, StringComparison.OrdinalIgnoreCase),
                $"Replay parity scene '{fixture.Name}' drifted: lightweight='{lightweightSummary}' rebuilt='{rebuiltSummary}'.");
            Assert(string.Equals(lightweightAllowedActionKey, rebuiltAllowedActionKey, StringComparison.OrdinalIgnoreCase),
                $"Replay parity scene '{fixture.Name}' changed allowed-action semantics between saved and rebuilt requests. lightweight='{lightweightAllowedActionKey}' rebuilt='{rebuiltAllowedActionKey}'.");

            if (!string.IsNullOrWhiteSpace(fixture.ExpectedStatus))
            {
                Assert(string.Equals(lightweightEvaluation.Decision.Status, fixture.ExpectedStatus, StringComparison.OrdinalIgnoreCase)
                       && string.Equals(rebuiltEvaluation.Decision.Status, fixture.ExpectedStatus, StringComparison.OrdinalIgnoreCase),
                    $"Replay parity scene '{fixture.Name}' expected status '{fixture.ExpectedStatus}'.");
            }

            if (!string.IsNullOrWhiteSpace(fixture.ExpectedActionKind))
            {
                Assert(string.Equals(lightweightEvaluation.Decision.ActionKind, fixture.ExpectedActionKind, StringComparison.OrdinalIgnoreCase)
                       && string.Equals(rebuiltEvaluation.Decision.ActionKind, fixture.ExpectedActionKind, StringComparison.OrdinalIgnoreCase),
                    $"Replay parity scene '{fixture.Name}' expected action kind '{fixture.ExpectedActionKind}'.");
            }

            if (!string.IsNullOrWhiteSpace(fixture.ExpectedTargetContains))
            {
                Assert((lightweightEvaluation.Decision.TargetLabel ?? string.Empty).Contains(fixture.ExpectedTargetContains, StringComparison.OrdinalIgnoreCase)
                       && (rebuiltEvaluation.Decision.TargetLabel ?? string.Empty).Contains(fixture.ExpectedTargetContains, StringComparison.OrdinalIgnoreCase),
                    $"Replay parity scene '{fixture.Name}' expected target containing '{fixture.ExpectedTargetContains}'.");
            }

            if (!string.IsNullOrWhiteSpace(fixture.ExpectedForegroundOwner))
            {
                Assert(string.Equals(TryGetObserverMeta(lightweightLoad.Request.Observer, "foregroundOwner"), fixture.ExpectedForegroundOwner, StringComparison.OrdinalIgnoreCase),
                    $"Replay parity scene '{fixture.Name}' expected foreground owner '{fixture.ExpectedForegroundOwner}'.");
            }

            if (!string.IsNullOrWhiteSpace(fixture.ExpectedForegroundActionLane))
            {
                Assert(string.Equals(TryGetObserverMeta(lightweightLoad.Request.Observer, "foregroundActionLane"), fixture.ExpectedForegroundActionLane, StringComparison.OrdinalIgnoreCase),
                    $"Replay parity scene '{fixture.Name}' expected foreground action lane '{fixture.ExpectedForegroundActionLane}'.");
            }

            if (!string.IsNullOrWhiteSpace(fixture.ExpectedRebuiltForegroundOwner))
            {
                Assert(string.Equals(TryGetObserverMeta(rebuiltLoad.Request.Observer, "foregroundOwner"), fixture.ExpectedRebuiltForegroundOwner, StringComparison.OrdinalIgnoreCase),
                    $"Replay parity scene '{fixture.Name}' expected rebuilt foreground owner '{fixture.ExpectedRebuiltForegroundOwner}'.");
            }

            if (!string.IsNullOrWhiteSpace(fixture.ExpectedRebuiltForegroundActionLane))
            {
                Assert(string.Equals(TryGetObserverMeta(rebuiltLoad.Request.Observer, "foregroundActionLane"), fixture.ExpectedRebuiltForegroundActionLane, StringComparison.OrdinalIgnoreCase),
                    $"Replay parity scene '{fixture.Name}' expected rebuilt foreground action lane '{fixture.ExpectedRebuiltForegroundActionLane}'.");
            }

            if (!string.IsNullOrWhiteSpace(fixture.ExpectedChoiceExtractorPath))
            {
                Assert(string.Equals(lightweightLoad.Request.Observer.ChoiceExtractorPath, fixture.ExpectedChoiceExtractorPath, StringComparison.OrdinalIgnoreCase),
                    $"Replay parity scene '{fixture.Name}' expected choice extractor '{fixture.ExpectedChoiceExtractorPath}'.");
            }

            results.Add(new
            {
                fixture.Name,
                fixture.RequestPath,
                ObserverStateLoaded = new
                {
                    Lightweight = lightweightLoad.ObserverStateLoaded,
                    Rebuilt = rebuiltLoad.ObserverStateLoaded,
                },
                RequestContract = new
                {
                    LightweightForegroundOwner = TryGetObserverMeta(lightweightLoad.Request.Observer, "foregroundOwner"),
                    LightweightForegroundActionLane = TryGetObserverMeta(lightweightLoad.Request.Observer, "foregroundActionLane"),
                    RebuiltForegroundOwner = TryGetObserverMeta(rebuiltLoad.Request.Observer, "foregroundOwner"),
                    RebuiltForegroundActionLane = TryGetObserverMeta(rebuiltLoad.Request.Observer, "foregroundActionLane"),
                    ChoiceExtractorPath = lightweightLoad.Request.Observer.ChoiceExtractorPath,
                },
                Lightweight = new
                {
                    Decision = lightweightEvaluation.Decision.TargetLabel,
                    Summary = lightweightSummary,
                    AllowedActions = lightweightLoad.Request.AllowedActions,
                },
                Rebuilt = new
                {
                    Decision = rebuiltEvaluation.Decision.TargetLabel,
                    Summary = rebuiltSummary,
                    AllowedActions = rebuiltLoad.Request.AllowedActions,
                },
                ElapsedMs = trace.Entries.Sum(static entry => entry.ElapsedMs),
            });
            trace.Info($"ok summary={lightweightSummary} foreground={TryGetObserverMeta(lightweightLoad.Request.Observer, "foregroundOwner") ?? "null"} rebuiltForeground={TryGetObserverMeta(rebuiltLoad.Request.Observer, "foregroundOwner") ?? "null"} lane={TryGetObserverMeta(lightweightLoad.Request.Observer, "foregroundActionLane") ?? "null"} rebuiltLane={TryGetObserverMeta(rebuiltLoad.Request.Observer, "foregroundActionLane") ?? "null"}");
        }

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            SuitePath = suitePath,
            SceneCount = fixtures.Count,
            Results = results,
        }, GuiSmokeShared.JsonOptions));
        return 0;
    }

}
