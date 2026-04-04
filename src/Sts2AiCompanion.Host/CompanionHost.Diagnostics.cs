using System.Text;
using System.Text.Json;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Host;

internal sealed class CompanionHostDiagnosticsService
{
    private static readonly TimeSpan HeavyDiagnosticsRefreshInterval = TimeSpan.FromSeconds(15);

    private readonly ScaffoldConfiguration _configuration;
    private readonly string _workspaceRoot;
    private readonly LiveExportLayout _layout;
    private readonly JsonSerializerOptions _jsonOptions;
    private DateTimeOffset _lastHeavyDiagnosticsRefreshAt;
    private string? _lastHeavyDiagnosticsRunId;

    public CompanionHostDiagnosticsService(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        LiveExportLayout layout,
        JsonSerializerOptions jsonOptions,
        JsonSerializerOptions _)
    {
        _configuration = configuration;
        _workspaceRoot = workspaceRoot;
        _layout = layout;
        _jsonOptions = jsonOptions;
    }

    public CompanionCollectorStatus UpdateArtifacts(
        CompanionRunState runState,
        KnowledgeSlice? latestKnowledgeSlice,
        AdviceResponse? latestAdvice,
        CodexSessionState? sessionState)
    {
        var paths = EnsureRunArtifacts(runState.Snapshot.RunId);
        var refreshHeavyDiagnostics = ShouldRefreshHeavyDiagnostics(runState);
        MirrorLiveArtifacts(runState, paths, sessionState, refreshHeavyDiagnostics);
        var collectorStatus = BuildCollectorStatus(runState, paths, latestKnowledgeSlice, latestAdvice, sessionState);
        if (_configuration.LiveExport.CollectorModeEnabled
            && paths.CollectorSummaryPath is not null
            && refreshHeavyDiagnostics)
        {
            WriteJson(paths.CollectorSummaryPath, BuildCollectorSummary(runState, paths, collectorStatus, latestKnowledgeSlice, latestAdvice, sessionState));
        }

        return collectorStatus;
    }

    private CompanionArtifactPaths EnsureRunArtifacts(string runId)
    {
        var paths = CompanionPathResolver.Resolve(_configuration, _workspaceRoot, runId);
        Directory.CreateDirectory(paths.CompanionRoot);
        if (paths.RunRoot is not null) Directory.CreateDirectory(paths.RunRoot);
        if (paths.LiveMirrorRoot is not null) Directory.CreateDirectory(paths.LiveMirrorRoot);
        if (paths.PromptPacksRoot is not null) Directory.CreateDirectory(paths.PromptPacksRoot);
        if (paths.AdviceRoot is not null) Directory.CreateDirectory(paths.AdviceRoot);
        if (paths.AdvisorSceneRoot is not null) Directory.CreateDirectory(paths.AdvisorSceneRoot);
        return paths;
    }

    private bool ShouldRefreshHeavyDiagnostics(CompanionRunState runState)
    {
        if (!string.Equals(_lastHeavyDiagnosticsRunId, runState.Snapshot.RunId, StringComparison.Ordinal))
        {
            _lastHeavyDiagnosticsRunId = runState.Snapshot.RunId;
            _lastHeavyDiagnosticsRefreshAt = DateTimeOffset.UtcNow;
            return true;
        }

        if (DateTimeOffset.UtcNow - _lastHeavyDiagnosticsRefreshAt < HeavyDiagnosticsRefreshInterval)
        {
            return false;
        }

        _lastHeavyDiagnosticsRefreshAt = DateTimeOffset.UtcNow;
        return true;
    }

    private void MirrorLiveArtifacts(CompanionRunState runState, CompanionArtifactPaths paths, CodexSessionState? sessionState, bool includeHeavyArtifacts)
    {
        if (paths.LiveMirrorRoot is null)
        {
            return;
        }

        Directory.CreateDirectory(paths.LiveMirrorRoot);
        CopyIfChanged(_layout.SnapshotPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.SnapshotPath)));
        CopyIfChanged(_layout.SummaryPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.SummaryPath)));
        CopyIfChanged(_layout.SessionPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.SessionPath)));
        if (includeHeavyArtifacts)
        {
            CopyIfChanged(_layout.EventsPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.EventsPath)));
            CopyIfChanged(_layout.RawObservationsPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.RawObservationsPath)));
            CopyIfChanged(_layout.ScreenTransitionsPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.ScreenTransitionsPath)));
            CopyIfChanged(_layout.ChoiceCandidatesPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.ChoiceCandidatesPath)));
            CopyIfChanged(_layout.ChoiceDecisionsPath, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.ChoiceDecisionsPath)));
            CopyDirectoryIfChanged(_layout.SemanticSnapshotsRoot, Path.Combine(paths.LiveMirrorRoot, Path.GetFileName(_layout.SemanticSnapshotsRoot)));
        }
        WriteJson(
            paths.CurrentRunStatePath,
            new
            {
                runId = runState.Snapshot.RunId,
                screen = runState.Snapshot.CurrentScreen,
                capturedAt = runState.Snapshot.CapturedAt,
                liveRoot = _layout.LiveRoot,
                sessionId = sessionState?.SessionId,
                collectorModeEnabled = _configuration.LiveExport.CollectorModeEnabled,
            });
    }

    private CompanionCollectorStatus BuildCollectorStatus(
        CompanionRunState runState,
        CompanionArtifactPaths paths,
        KnowledgeSlice? latestKnowledgeSlice,
        AdviceResponse? latestAdvice,
        CodexSessionState? sessionState)
    {
        if (!_configuration.LiveExport.CollectorModeEnabled)
        {
            return new CompanionCollectorStatus(false, null, null, null, null, null, 0, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), sessionState?.SessionId, "collector mode disabled");
        }

        var latestDecision = ReadLastJsonObject(paths.LiveMirrorRoot, Path.GetFileName(_layout.ChoiceDecisionsPath));
        var lastSemanticScreen = runState.RecentEvents.LastOrDefault(IsSemanticEvent)?.Screen;
        var normalizedScene = runState.NormalizedState.Scene.SemanticSceneType;
        var activeEpisode = IsHighValueScreen(normalizedScene) ? normalizedScene : lastSemanticScreen;
        var acceptedExtractorPath = TryReadNestedString(latestDecision, "decision", "extractorPath");
        var acceptedCount = TryReadNestedInt(latestDecision, "decision", "acceptedCount");
        var failureReason = TryReadNestedString(latestDecision, "decision", "failureReason");
        var choiceStatus = acceptedCount > 0
            ? $"resolved ({acceptedCount})"
            : runState.Snapshot.CurrentChoices.Count > 0
                ? $"resolved ({runState.Snapshot.CurrentChoices.Count})"
                : latestDecision is not null ? "missing" : "not-seen";
        var degradedReason = failureReason
                             ?? latestAdvice?.DecisionBlockers.FirstOrDefault()
                             ?? latestAdvice?.MissingInformation.FirstOrDefault()
                             ?? (latestAdvice is { Status: not "ok" } ? latestAdvice.Summary : null);
        var notes = string.Join(Environment.NewLine, new[]
        {
            "collector mode: on",
            $"latest semantic screen: {lastSemanticScreen ?? "none"}",
            $"normalized scene: {normalizedScene}",
            $"active screen episode: {activeEpisode ?? "none"}",
            $"choice extraction: {choiceStatus}",
            $"extractor path: {acceptedExtractorPath ?? "none"}",
            $"last degraded reason: {degradedReason ?? "none"}",
            $"knowledge entries used: {latestKnowledgeSlice?.Entries.Count ?? 0}",
            $"knowledge reasons: {(latestKnowledgeSlice?.Reasons.Count > 0 ? string.Join(", ", latestKnowledgeSlice.Reasons.Take(4)) : "none")}",
            $"knowledge refs: {(latestAdvice?.KnowledgeRefs.Count > 0 ? string.Join(", ", latestAdvice.KnowledgeRefs.Take(4)) : "none")}",
            $"missing information: {(latestAdvice?.MissingInformation.Count > 0 ? string.Join(", ", latestAdvice.MissingInformation.Take(4)) : "none")}",
            $"decision blockers: {(latestAdvice?.DecisionBlockers.Count > 0 ? string.Join(", ", latestAdvice.DecisionBlockers.Take(4)) : "none")}",
            $"session id: {sessionState?.SessionId ?? "none"}",
        });
        return new CompanionCollectorStatus(
            true,
            activeEpisode,
            lastSemanticScreen,
            choiceStatus,
            acceptedExtractorPath,
            degradedReason,
            latestKnowledgeSlice?.Entries.Count ?? 0,
            latestKnowledgeSlice?.Reasons ?? Array.Empty<string>(),
            latestAdvice?.KnowledgeRefs ?? Array.Empty<string>(),
            latestAdvice?.MissingInformation ?? Array.Empty<string>(),
            latestAdvice?.DecisionBlockers ?? Array.Empty<string>(),
            sessionState?.SessionId,
            notes);
    }

    private CompanionCollectorSummary BuildCollectorSummary(
        CompanionRunState runState,
        CompanionArtifactPaths paths,
        CompanionCollectorStatus collectorStatus,
        KnowledgeSlice? latestKnowledgeSlice,
        AdviceResponse? latestAdvice,
        CodexSessionState? sessionState)
    {
        var transitions = ReadJsonLines(paths.LiveMirrorRoot, Path.GetFileName(_layout.ScreenTransitionsPath));
        var decisions = ReadJsonLines(paths.LiveMirrorRoot, Path.GetFileName(_layout.ChoiceDecisionsPath));
        var candidates = ReadJsonLines(paths.LiveMirrorRoot, Path.GetFileName(_layout.ChoiceCandidatesPath));
        var adviceLog = paths.AdviceLogPath is not null && File.Exists(paths.AdviceLogPath) ? ReadJsonLines(paths.AdviceLogPath) : Array.Empty<JsonDocument>();
        var traceLog = paths.CodexTracePath is not null && File.Exists(paths.CodexTracePath) ? ReadJsonLines(paths.CodexTracePath) : Array.Empty<JsonDocument>();
        var runtimeFatalErrors = ReadRuntimeFatalErrors();
        var timeline = transitions
            .Select(d =>
            {
                var root = d.RootElement;
                return $"{TryReadString(root, "triggerKind") ?? "unknown"}: {TryReadString(root, "before") ?? "unknown"} -> {TryReadString(root, "after") ?? "unknown"} (incoming={TryReadString(root, "incoming") ?? "unknown"}, reason={TryReadString(root, "reason") ?? "none"})";
            })
            .TakeLast(24)
            .ToArray();
        var semanticCounts = runState.RecentEvents
            .Where(IsSemanticEvent)
            .GroupBy(e => e.Kind, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        var missingChoices = decisions
            .Where(d => (TryReadNestedInt(d, "decision", "acceptedCount") ?? 0) == 0)
            .Select(d => $"{TryReadString(d.RootElement, "screen") ?? "unknown"} via {TryReadNestedString(d, "decision", "extractorPath") ?? "unknown"} ({TryReadNestedString(d, "decision", "failureReason") ?? "unknown"})")
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var placeholderLabels = candidates
            .Where(d => string.Equals(TryReadNestedString(d, "candidate", "rejectReason"), "placeholder-label", StringComparison.Ordinal))
            .Select(d => TryReadNestedString(d, "candidate", "label"))
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(v => v, StringComparer.Ordinal)
            .Take(64)
            .ToArray();
        var autoAdviceFailures = adviceLog
            .Where(d =>
            {
                var root = d.RootElement;
                var status = TryReadString(root, "status");
                var triggerKind = TryReadString(root, "triggerKind");
                return !string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase)
                       && !string.Equals(triggerKind, "manual", StringComparison.OrdinalIgnoreCase)
                       && !string.Equals(triggerKind, "retry-last", StringComparison.OrdinalIgnoreCase);
            })
            .Select(d =>
            {
                var root = d.RootElement;
                return $"{TryReadString(root, "triggerKind") ?? "unknown"}: {TryReadString(root, "summary") ?? "degraded"}";
            })
            .TakeLast(12)
            .ToArray();
        var missingInformationObserved = SummarizeFrequency(adviceLog.SelectMany(d => ReadStringArray(d.RootElement, "missingInformation")), 64);
        var decisionBlockersObserved = SummarizeFrequency(adviceLog.SelectMany(d => ReadStringArray(d.RootElement, "decisionBlockers")), 64);
        return new CompanionCollectorSummary(
            runState.Snapshot.RunId,
            DateTimeOffset.UtcNow,
            timeline,
            semanticCounts,
            missingChoices,
            placeholderLabels,
            autoAdviceFailures,
            missingInformationObserved,
            decisionBlockersObserved,
            sessionState?.SessionId is null ? "missing-session-id" : "session-tracked",
            ReadObservedMergeCounts(),
            BuildRequestLatencySummary(traceLog),
            BuildDuplicateTriggerSummary(traceLog),
            BuildScreenOverwriteSummary(transitions),
            BuildStateRegressionSummary(runState),
            BuildKnowledgeUsageSummary(adviceLog, traceLog),
            runtimeFatalErrors,
            BuildAppHangSuspicionIndicators(paths, runtimeFatalErrors, traceLog),
            BuildRecommendedFixes(missingChoices, placeholderLabels, autoAdviceFailures, missingInformationObserved, decisionBlockersObserved, collectorStatus));
    }

    private void WriteJson<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(value, _jsonOptions));
    }

    private IReadOnlyDictionary<string, int> ReadObservedMergeCounts()
    {
        var path = Path.Combine(CompanionPathResolver.ResolveKnowledgeRoot(_configuration, _workspaceRoot), "observed-merge.json");
        if (!File.Exists(path))
        {
            return new Dictionary<string, int>(StringComparer.Ordinal);
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.EnumerateObject().ToDictionary(
            property => property.Name,
            property => property.Value.ValueKind switch
            {
                JsonValueKind.Array => property.Value.GetArrayLength(),
                JsonValueKind.Object => property.Value.EnumerateObject().Count(),
                _ => 0,
            },
            StringComparer.OrdinalIgnoreCase);
    }

    private IReadOnlyList<string> ReadRuntimeFatalErrors()
    {
        var path = Path.Combine(_configuration.GamePaths.GameDirectory, "mods", _configuration.AiCompanionMod.RuntimeLogFileName);
        if (!File.Exists(path))
        {
            return Array.Empty<string>();
        }

        return ReadAllLinesShared(path)
            .Where(line => line.Contains("Method not found", StringComparison.OrdinalIgnoreCase)
                           || line.Contains("runtime exporter worker failure", StringComparison.OrdinalIgnoreCase)
                           || line.Contains("runtime exporter poll failure", StringComparison.OrdinalIgnoreCase)
                           || line.Contains("hook failure", StringComparison.OrdinalIgnoreCase)
                           || line.Contains("disposed", StringComparison.OrdinalIgnoreCase)
                           || line.Contains("format", StringComparison.OrdinalIgnoreCase) && line.Contains("LocString", StringComparison.OrdinalIgnoreCase))
            .TakeLast(48)
            .ToArray();
    }

    private static void CopyIfChanged(string sourcePath, string destinationPath)
    {
        if (!File.Exists(sourcePath))
        {
            return;
        }

        var sourceInfo = new FileInfo(sourcePath);
        if (File.Exists(destinationPath))
        {
            var destinationInfo = new FileInfo(destinationPath);
            if (sourceInfo.Length == destinationInfo.Length
                && sourceInfo.LastWriteTimeUtc == destinationInfo.LastWriteTimeUtc)
            {
                return;
            }
        }

        File.Copy(sourcePath, destinationPath, overwrite: true);
        File.SetLastWriteTimeUtc(destinationPath, sourceInfo.LastWriteTimeUtc);
    }

    private static void CopyDirectoryIfChanged(string sourcePath, string destinationPath)
    {
        if (!Directory.Exists(sourcePath))
        {
            return;
        }

        Directory.CreateDirectory(destinationPath);
        foreach (var file in Directory.GetFiles(sourcePath))
        {
            CopyIfChanged(file, Path.Combine(destinationPath, Path.GetFileName(file)));
        }
    }

    private static IReadOnlyList<string> ReadAllLinesShared(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd()
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .ToArray();
    }

    private static IReadOnlyList<string> BuildRequestLatencySummary(IReadOnlyList<JsonDocument> traceLog)
    {
        var durations = traceLog
            .Where(d => string.Equals(TryReadString(d.RootElement, "kind"), "request-finished", StringComparison.Ordinal))
            .Select(d => TryReadInt(d.RootElement, "durationMs"))
            .Where(v => v is not null)
            .Select(v => v!.Value)
            .OrderBy(v => v)
            .ToArray();
        if (durations.Length == 0)
        {
            return Array.Empty<string>();
        }

        static int Percentile(int[] sorted, double percentile)
        {
            var index = (int)Math.Ceiling(sorted.Length * percentile) - 1;
            return sorted[Math.Clamp(index, 0, sorted.Length - 1)];
        }

        return new[]
        {
            $"count={durations.Length}",
            $"min_ms={durations[0]}",
            $"p50_ms={Percentile(durations, 0.50)}",
            $"p90_ms={Percentile(durations, 0.90)}",
            $"avg_ms={(int)durations.Average()}",
            $"max_ms={durations[^1]}",
        };
    }

    private static IReadOnlyList<string> BuildDuplicateTriggerSummary(IReadOnlyList<JsonDocument> traceLog)
    {
        return traceLog
            .Select(d => TryReadString(d.RootElement, "kind"))
            .Where(kind => kind is "request-coalesced" or "request-superseded" or "request-canceled" or "request-retried")
            .GroupBy(kind => kind!, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => $"{group.Key}={group.Count()}")
            .ToArray();
    }

    private static IReadOnlyList<string> BuildScreenOverwriteSummary(IReadOnlyList<JsonDocument> transitions)
    {
        return transitions
            .Where(d => d.RootElement.TryGetProperty("keptPreviousScreen", out var kept) && kept.ValueKind == JsonValueKind.True)
            .Select(d =>
            {
                var root = d.RootElement;
                return $"{TryReadString(root, "triggerKind") ?? "unknown"} kept {TryReadString(root, "before") ?? "unknown"} over {TryReadString(root, "incoming") ?? "unknown"}";
            })
            .TakeLast(24)
            .ToArray();
    }

    private static IReadOnlyList<string> BuildStateRegressionSummary(CompanionRunState runState)
    {
        return runState.Snapshot.Warnings
            .Where(warning => warning.StartsWith("state-regression:", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.Ordinal)
            .Take(64)
            .ToArray();
    }

    private static IReadOnlyList<string> BuildKnowledgeUsageSummary(IReadOnlyList<JsonDocument> adviceLog, IReadOnlyList<JsonDocument> traceLog)
    {
        var knowledgeRefCounts = adviceLog.Select(d => ReadStringArray(d.RootElement, "knowledgeRefs").Count).ToArray();
        var latestTrace = traceLog.LastOrDefault(d => string.Equals(TryReadString(d.RootElement, "kind"), "request-finished", StringComparison.Ordinal));
        var latestTraceKnowledgeCount = latestTrace is null ? null : TryReadInt(latestTrace.RootElement, "knowledgeEntriesUsedCount");
        var latestReasons = latestTrace is null ? Array.Empty<string>() : ReadStringArray(latestTrace.RootElement, "knowledgeReasons");
        var blockedByState = adviceLog.Count(d => ReadStringArray(d.RootElement, "missingInformation").Count > 0 && ReadStringArray(d.RootElement, "knowledgeRefs").Count > 0);
        var knowledgeEmpty = adviceLog.Count(d => ReadStringArray(d.RootElement, "knowledgeRefs").Count == 0 && ReadStringArray(d.RootElement, "missingInformation").Count > 0);
        var topRefs = SummarizeFrequency(adviceLog.SelectMany(d => ReadStringArray(d.RootElement, "knowledgeRefs")), 6);
        return new[]
        {
            $"responses_with_knowledge_refs={knowledgeRefCounts.Count(count => count > 0)}",
            $"max_knowledge_refs={knowledgeRefCounts.DefaultIfEmpty(0).Max()}",
            $"latest_prompt_knowledge_entries={latestTraceKnowledgeCount?.ToString() ?? "0"}",
            $"latest_prompt_knowledge_reasons={(latestReasons.Count > 0 ? string.Join(", ", latestReasons.Take(4)) : "none")}",
            $"knowledge_present_but_blocked_by_runtime={blockedByState}",
            $"knowledge_slice_empty={knowledgeEmpty}",
        }.Concat(topRefs.Select(item => $"top_ref={item}")).ToArray();
    }

    private static IReadOnlyList<string> BuildAppHangSuspicionIndicators(CompanionArtifactPaths paths, IReadOnlyList<string> runtimeFatalErrors, IReadOnlyList<JsonDocument> traceLog)
    {
        var failedRequests = traceLog.Count(d =>
        {
            var kind = TryReadString(d.RootElement, "kind");
            return kind is "request-failed" or "request-canceled";
        });
        var disposedFailures = runtimeFatalErrors.Count(line => line.Contains("disposed", StringComparison.OrdinalIgnoreCase));
        var exporterFailures = runtimeFatalErrors.Count(line => line.Contains("runtime exporter worker failure", StringComparison.OrdinalIgnoreCase));
        var lastFinishedAt = traceLog
            .Where(d => string.Equals(TryReadString(d.RootElement, "kind"), "request-finished", StringComparison.Ordinal))
            .Select(d => TryReadString(d.RootElement, "generatedAt"))
            .LastOrDefault();
        var semanticRoot = paths.LiveMirrorRoot is null ? null : Path.Combine(paths.LiveMirrorRoot, "semantic-snapshots");
        var semanticFiles = semanticRoot is not null && Directory.Exists(semanticRoot)
            ? Directory.GetFiles(semanticRoot, "*.json", SearchOption.TopDirectoryOnly)
            : Array.Empty<string>();
        var lastSemanticSnapshotAt = semanticFiles.Select(File.GetLastWriteTimeUtc).DefaultIfEmpty().Max();
        return new[]
        {
            $"runtime_fatal_error_count={runtimeFatalErrors.Count}",
            $"exporter_worker_failure_count={exporterFailures}",
            $"disposed_object_failure_count={disposedFailures}",
            $"failed_or_canceled_requests={failedRequests}",
            $"semantic_snapshot_count={semanticFiles.Length}",
            $"last_semantic_snapshot_at={(lastSemanticSnapshotAt == default ? "none" : new DateTimeOffset(lastSemanticSnapshotAt, TimeSpan.Zero).ToString("O"))}",
            $"last_successful_advice_at={lastFinishedAt ?? "none"}",
        };
    }

    private static IReadOnlyList<string> BuildRecommendedFixes(
        IReadOnlyList<string> missingChoices,
        IReadOnlyList<string> placeholderLabels,
        IReadOnlyList<string> autoAdviceFailures,
        IReadOnlyList<string> missingInformationObserved,
        IReadOnlyList<string> decisionBlockersObserved,
        CompanionCollectorStatus? collectorStatus)
    {
        var fixes = new List<string>();
        if (missingChoices.Count > 0) fixes.Add("Strengthen strict choice extractors for the affected screens before trusting generic fallback.");
        if (placeholderLabels.Count > 0) fixes.Add("Expand placeholder filtering for UI/internal node names in generic choice extraction.");
        if (autoAdviceFailures.Count > 0) fixes.Add("Inspect prompt packs and Codex trace for degraded auto advice failures.");
        if (missingInformationObserved.Any(item => item.Contains("price", StringComparison.OrdinalIgnoreCase))) fixes.Add("Shop extraction still misses concrete price or item identity fields.");
        if (missingInformationObserved.Any(item => item.Contains("item identity", StringComparison.OrdinalIgnoreCase) || item.Contains("상품", StringComparison.OrdinalIgnoreCase))) fixes.Add("Shop and reward extraction still miss concrete item identity fields.");
        if (collectorStatus?.SessionId is null) fixes.Add("Gameplay triggers are not reusing or persisting a Codex session yet.");
        if (decisionBlockersObserved.Any(item => item.Contains("deck", StringComparison.OrdinalIgnoreCase))) fixes.Add("Deck/state merge is still dropping authoritative deck information on overlay screens.");
        if (decisionBlockersObserved.Any(item => item.Contains("price", StringComparison.OrdinalIgnoreCase) || item.Contains("context", StringComparison.OrdinalIgnoreCase))) fixes.Add("Advice is blocked by missing shop price or deck context; treat those as first-class collector blockers.");
        if (fixes.Count == 0) fixes.Add("No high-priority collector blocker detected. Expand gameplay screen coverage.");
        return fixes;
    }

    private static IReadOnlyList<string> SummarizeFrequency(IEnumerable<string> values, int maxItems)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value.Trim(), StringComparer.Ordinal)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Take(maxItems)
            .Select(group => $"{group.Count()}x {group.Key}")
            .ToArray();
    }

    private static bool IsSemanticEvent(LiveExportEventEnvelope envelope)
    {
        return envelope.Kind is "choice-list-presented" or "reward-opened" or "reward-screen-opened" or "event-opened" or "event-screen-opened" or "shop-opened" or "rest-opened";
    }

    private static bool IsHighValueScreen(string? screen)
    {
        return screen is "rewards" or "event" or "shop" or "rest-site" or "card-choice" or "upgrade" or "transform";
    }

    private static IReadOnlyList<JsonDocument> ReadJsonLines(string? directory, string fileName)
    {
        return string.IsNullOrWhiteSpace(directory) ? Array.Empty<JsonDocument>() : ReadJsonLines(Path.Combine(directory, fileName));
    }

    private static IReadOnlyList<JsonDocument> ReadJsonLines(string path)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<JsonDocument>();
        }

        var content = TryReadAllText(path);
        if (string.IsNullOrWhiteSpace(content))
        {
            return Array.Empty<JsonDocument>();
        }

        var docs = new List<JsonDocument>();
        foreach (var line in SplitJsonObjects(content))
        {
            try
            {
                docs.Add(JsonDocument.Parse(line));
            }
            catch (JsonException)
            {
            }
        }

        return docs;
    }

    private static string? TryReadAllText(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static JsonDocument? ReadLastJsonObject(string? directory, string fileName)
    {
        var lines = ReadJsonLines(directory, fileName);
        return lines.Count == 0 ? null : lines[^1];
    }

    private static string? TryReadString(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object
               && element.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static int? TryReadInt(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object
               && element.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.Number
               && property.TryGetInt32(out var value)
            ? value
            : null;
    }

    private static string? TryReadNestedString(JsonDocument? document, string parentProperty, string propertyName)
    {
        return document is not null
               && document.RootElement.ValueKind == JsonValueKind.Object
               && document.RootElement.TryGetProperty(parentProperty, out var parent)
               && parent.ValueKind == JsonValueKind.Object
               && parent.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static int? TryReadNestedInt(JsonDocument? document, string parentProperty, string propertyName)
    {
        return document is not null
               && document.RootElement.ValueKind == JsonValueKind.Object
               && document.RootElement.TryGetProperty(parentProperty, out var parent)
               && parent.ValueKind == JsonValueKind.Object
               && parent.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.Number
               && property.TryGetInt32(out var value)
            ? value
            : null;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return property
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToArray();
    }

    private static IEnumerable<string> SplitJsonObjects(string content)
    {
        var utf8 = Encoding.UTF8.GetBytes(content);
        var offset = 0;
        var chunks = new List<string>();

        while (offset < utf8.Length)
        {
            while (offset < utf8.Length && char.IsWhiteSpace((char)utf8[offset]))
            {
                offset += 1;
            }

            if (offset >= utf8.Length)
            {
                break;
            }

            var reader = new Utf8JsonReader(utf8.AsSpan(offset), isFinalBlock: true, state: default);
            try
            {
                using var document = JsonDocument.ParseValue(ref reader);
                chunks.Add(document.RootElement.GetRawText());
                offset += (int)reader.BytesConsumed;
            }
            catch (JsonException)
            {
                break;
            }
        }

        return chunks;
    }
}
