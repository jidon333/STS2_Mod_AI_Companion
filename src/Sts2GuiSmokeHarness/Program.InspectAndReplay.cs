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
                $"Replay parity scene '{fixture.Name}' changed allowed-action semantics between saved and rebuilt requests.");

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
                    ForegroundOwner = TryGetObserverMeta(lightweightLoad.Request.Observer, "foregroundOwner"),
                    ForegroundActionLane = TryGetObserverMeta(lightweightLoad.Request.Observer, "foregroundActionLane"),
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
            trace.Info($"ok summary={lightweightSummary} foreground={TryGetObserverMeta(lightweightLoad.Request.Observer, "foregroundOwner") ?? "null"} lane={TryGetObserverMeta(lightweightLoad.Request.Observer, "foregroundActionLane") ?? "null"}");
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
        var analysisContext = CreateStepAnalysisContext(
            phase,
            observer,
            request.ScreenshotPath,
            request.History,
            request.CombatCardKnowledge,
            request.WindowBounds);
        var sceneSignature = tracer.Measure(
            "scene-signature",
            () => ComputeSceneSignatureCore(request.ScreenshotPath, observer, phase, analysisContext),
            Path.GetFileName(request.ScreenshotPath),
            alwaysLog: false);
        string[] allowedActions;
        string failureModeHint;
        string? semanticGoal;
        if (fullRequestRebuild)
        {
            allowedActions = tracer.Measure(
                "allowed-actions",
                () => BuildAllowedActionsCore(phase, observer, request.CombatCardKnowledge, request.ScreenshotPath, request.History, analysisContext),
                request.Phase,
                alwaysLog: false);
            failureModeHint = tracer.Measure(
                "failure-mode-hint",
                () => BuildFailureModeHintCoreWithContext(phase, observer, request.CombatCardKnowledge, request.ScreenshotPath, request.History, analysisContext),
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
        GuiSmokeStepDecision? actualDecision = null,
        GuiSmokeStepAnalysisContext? analysisContext = null)
    {
        var analysis = AutoDecisionProvider.Analyze(request, actualDecision, analysisContext);
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

    static bool ShouldPersistCandidateDumpArtifact(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        if (string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase)
            || string.Equals(decision.Status, "abort", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (request.FirstSeenScene
            || !string.IsNullOrWhiteSpace(decision.DecisionRisk)
            || string.Equals(request.ReasoningMode, "semantic", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(decision.TargetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decision.TargetLabel, "map back", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decision.TargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decision.TargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase);
    }

    static bool ShouldRecordUnknownScene(GuiSmokeStepRequest request)
    {
        return string.Equals(request.Observer.CurrentScreen, "unknown", StringComparison.OrdinalIgnoreCase)
               || string.Equals(request.Observer.VisibleScreen, "unknown", StringComparison.OrdinalIgnoreCase);
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

    static string BuildReplayParityDecisionSummary(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        string? combatSemanticAction = null;
        if (string.Equals(request.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
            && CombatDecisionContract.TryMapSemanticAction(request, decision, out var mappedSemanticAction))
        {
            combatSemanticAction = mappedSemanticAction;
        }

        static string Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "null"
                : value.Trim().ToLowerInvariant();
        }

        static string FormatCoord(double? value)
        {
            return value is null
                ? "null"
                : value.Value.ToString("0.000", CultureInfo.InvariantCulture);
        }

        return string.Join(
            "|",
            new[]
            {
                $"status:{Normalize(decision.Status)}",
                $"action:{Normalize(decision.ActionKind)}",
                $"key:{Normalize(decision.KeyText)}",
                $"target:{Normalize(decision.TargetLabel)}",
                $"x:{FormatCoord(decision.NormalizedX)}",
                $"y:{FormatCoord(decision.NormalizedY)}",
                $"combat:{Normalize(combatSemanticAction)}",
            });
    }

    static string BuildReplayParityAllowedActionKey(IReadOnlyList<string> allowedActions)
    {
        return string.Join(
            "|",
            allowedActions
                .OrderBy(static action => action, StringComparer.OrdinalIgnoreCase)
                .Select(static action => action.Trim().ToLowerInvariant()));
    }

    static string? TryGetObserverMeta(ObserverSummary observer, string key)
    {
        if (observer.Meta is null)
        {
            return null;
        }

        return observer.Meta.TryGetValue(key, out var value)
            ? value
            : null;
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
}
