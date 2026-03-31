using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using static ObserverScreenProvenance;

static class GuiSmokeReplayArtifactSupport
{
    internal static GuiSmokeReplayRequestLoadResult LoadReplayRequest(string requestPath, GuiSmokeReplayTracer? trace = null, bool fullRequestRebuild = false)
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
            () => GuiSmokeFixtureIoSupport.TryLoadJsonDocument(stepPrefix + ".observer.state.json"),
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
        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, request.WindowBounds, request.History, request.ScreenshotPath);
        var canonicalObserver = fullRequestRebuild
            ? new ObserverState(
                NormalizeReplayObserverMeta(observer.Summary, eventScene),
                stateDocument,
                null,
                observer.LastEventsTail?.ToArray() ?? Array.Empty<string>())
            : observer;
        var analysisContext = GuiSmokeStepRequestFactory.CreateStepAnalysisContext(
            phase,
            canonicalObserver,
            request.ScreenshotPath,
            request.History,
            request.CombatCardKnowledge,
            request.WindowBounds);
        var sceneSignature = tracer.Measure(
            "scene-signature",
            () => GuiSmokeSceneReasoningSupport.ComputeSceneSignatureCore(request.ScreenshotPath, canonicalObserver, phase, analysisContext),
            Path.GetFileName(request.ScreenshotPath),
            alwaysLog: false);
        string[] allowedActions;
        string failureModeHint;
        string? semanticGoal;
        if (fullRequestRebuild)
        {
            allowedActions = tracer.Measure(
                "allowed-actions",
                () => Program.BuildAllowedActionsCore(phase, canonicalObserver, request.CombatCardKnowledge, request.ScreenshotPath, request.History, analysisContext),
                request.Phase,
                alwaysLog: false);
            failureModeHint = tracer.Measure(
                "failure-mode-hint",
                () => GuiSmokePromptContractSupport.BuildFailureModeHintCoreWithContext(phase, canonicalObserver, request.CombatCardKnowledge, request.ScreenshotPath, request.History, analysisContext),
                request.Phase,
                alwaysLog: false);
            semanticGoal = tracer.Measure(
                "semantic-goal",
                () => GuiSmokeSceneReasoningSupport.BuildSemanticGoal(phase, canonicalObserver, request.ReasoningMode),
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
                Observer = canonicalObserver.Summary,
                SceneSignature = sceneSignature,
                AllowedActions = allowedActions,
                FailureModeHint = failureModeHint,
                SemanticGoal = semanticGoal,
            },
            fullRequestRebuild,
            stateDocument is not null,
            tracer.Entries.ToArray());
    }

    private static ObserverSummary NormalizeReplayObserverMeta(ObserverSummary observer, EventSceneState eventScene)
    {
        if (!eventScene.AncientContract.HasLaneSurfaceMismatch)
        {
            return observer;
        }

        var meta = new Dictionary<string, string?>(observer.Meta, StringComparer.OrdinalIgnoreCase)
        {
            ["foregroundOwner"] = "event",
            ["foregroundActionLane"] = "event-choice",
            ["choiceExtractorPath"] = "event",
            ["eventTeardownInProgress"] = "false",
            ["mapReleaseAuthority"] = "false",
            ["mapSurfacePending"] = "false",
        };

        foreach (var key in new[]
                 {
                     "ancientPhase",
                     "ancientEventExtractionPath",
                     "ancientOptionCount",
                     "ancientCompletionCount",
                     "ancientOptionSummary",
                     "ancientCompletionSummary",
                     "ancientCompletionBoundsSource",
                     "ancientCompletionControlType",
                     "ancientCompletionUsesDefaultFocus",
                     "ancientCompletionHasFocus",
                 })
        {
            meta.Remove(key);
        }

        return observer with
        {
            Meta = meta,
        };
    }

    internal static GuiSmokeReplayEvaluation EvaluateAutoDecisionWithDiagnostics(
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

    internal static string SerializeCandidateDumpArtifact(GuiSmokeCandidateDumpArtifact artifact)
    {
        return JsonSerializer.Serialize(artifact, GuiSmokeShared.JsonOptions);
    }

    internal static void WriteCandidateDumpArtifact(string path, GuiSmokeCandidateDumpArtifact artifact)
    {
        WriteSerializedCandidateDumpArtifact(path, SerializeCandidateDumpArtifact(artifact));
    }

    internal static bool ShouldPersistCandidateDumpArtifact(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
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

    internal static bool ShouldRecordUnknownScene(GuiSmokeStepRequest request)
    {
        return string.Equals(DisplayScreen(request.Observer), "unknown", StringComparison.OrdinalIgnoreCase);
    }

    internal static string BuildSuppressedCandidateSummary(GuiSmokeDecisionDebugSummary debugSummary)
    {
        if (debugSummary.SuppressedCandidates.Count == 0)
        {
            return "none";
        }

        return string.Join(
            "; ",
            debugSummary.SuppressedCandidates.Select(candidate => $"{candidate.Label}:{candidate.SuppressionReason}"));
    }

    internal static string DescribeGoldenSceneChecks(GuiSmokeReplayGoldenSceneFixture fixture)
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

    internal static string BuildReplayParityDecisionSummary(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
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

    internal static string BuildReplayParityAllowedActionKey(IReadOnlyList<string> allowedActions)
    {
        return string.Join(
            "|",
            allowedActions
                .OrderBy(static action => action, StringComparer.OrdinalIgnoreCase)
                .Select(static action => action.Trim().ToLowerInvariant()));
    }

    internal static string? TryGetObserverMeta(ObserverSummary observer, string key)
    {
        if (observer.Meta is null)
        {
            return null;
        }

        return observer.Meta.TryGetValue(key, out var value)
            ? value
            : null;
    }

    internal static void WriteSerializedCandidateDumpArtifact(string path, string serializedArtifact)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory());
        var bytes = Encoding.UTF8.GetBytes(serializedArtifact);
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        stream.Write(bytes, 0, bytes.Length);
        stream.Flush(true);
    }
}
