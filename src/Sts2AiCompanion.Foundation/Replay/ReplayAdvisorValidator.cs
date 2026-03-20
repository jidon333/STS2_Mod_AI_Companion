using System.Text.Json;
using Sts2AiCompanion.Foundation.Artifacts;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Foundation.Knowledge;
using Sts2AiCompanion.Foundation.Reasoning;
using Sts2AiCompanion.Foundation.State;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Foundation.Replay;

public sealed class ReplayAdvisorValidator
{
    private readonly ScaffoldConfiguration _configuration;
    private readonly string _workspaceRoot;
    private readonly KnowledgeCatalogService _knowledgeCatalogService;
    private readonly AdvicePromptBuilder _promptBuilder;

    public ReplayAdvisorValidator(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        KnowledgeCatalogService? knowledgeCatalogService = null,
        AdvicePromptBuilder? promptBuilder = null)
    {
        _configuration = configuration;
        _workspaceRoot = workspaceRoot;
        _knowledgeCatalogService = knowledgeCatalogService ?? new KnowledgeCatalogService(configuration, workspaceRoot);
        _promptBuilder = promptBuilder ?? new AdvicePromptBuilder(configuration);
    }

    public async Task<ReplayValidationResult> ValidateAsync(
        string fixtureRoot,
        string? mockAdviceResponsePath,
        CancellationToken cancellationToken)
    {
        try
        {
            var resolvedFixtureRoot = ResolveFixtureRoot(fixtureRoot);
            var snapshotPath = Path.Combine(resolvedFixtureRoot, _configuration.LiveExport.SnapshotFileName);
            var summaryPath = Path.Combine(resolvedFixtureRoot, _configuration.LiveExport.SummaryFileName);
            var sessionPath = Path.Combine(resolvedFixtureRoot, _configuration.LiveExport.SessionFileName);
            var eventsPath = Path.Combine(resolvedFixtureRoot, _configuration.LiveExport.EventsFileName);

            var snapshot = await ReadJsonAsync<LiveExportSnapshot>(snapshotPath, cancellationToken).ConfigureAwait(false)
                ?? throw new FileNotFoundException("Replay fixture snapshot was not found.", snapshotPath);
            snapshot = SanitizeSnapshot(snapshot);
            var session = await ReadJsonAsync<LiveExportSession>(sessionPath, cancellationToken).ConfigureAwait(false);
            var summaryText = File.Exists(summaryPath)
                ? await File.ReadAllTextAsync(summaryPath, cancellationToken).ConfigureAwait(false)
                : "Replay validation summary unavailable.";
            var recentEvents = await ReadEventsAsync(eventsPath, cancellationToken).ConfigureAwait(false);

            var runState = new CompanionRunState(snapshot, session, summaryText, recentEvents, false)
            {
                NormalizedState = CompanionStateMapper.FromLiveExport(snapshot, session, recentEvents)
            };
            var trigger = new AdviceTrigger(
                "replay-validation",
                DateTimeOffset.UtcNow,
                Manual: true,
                BypassMinInterval: true,
                Reason: "replay-validation",
                SourceEvent: recentEvents.LastOrDefault());
            var slice = _knowledgeCatalogService.BuildSlice(
                runState,
                _configuration.Assistant.MaxKnowledgeEntries,
                _configuration.Assistant.MaxKnowledgeBytes);
            var inputPack = _promptBuilder.BuildInputPack(runState, trigger, slice);
            var response = await CreateResponseAsync(runState, slice, inputPack, mockAdviceResponsePath, cancellationToken).ConfigureAwait(false);

            var fixtureName = Path.GetFileName(Path.GetFullPath(resolvedFixtureRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var runId = $"replay-{fixtureName}";
            var paths = CompanionPathResolver.Resolve(_configuration, _workspaceRoot, runId);
            var store = new ArtifactStore(paths);
            store.EnsureRunDirectories();

            var promptPacksRoot = paths.PromptPacksRoot ?? throw new InvalidOperationException("Replay validation prompt pack root was not resolved.");
            var adviceJsonPath = paths.AdviceLatestJsonPath ?? throw new InvalidOperationException("Replay validation advice json path was not resolved.");
            var adviceMarkdownPath = paths.AdviceLatestMarkdownPath ?? throw new InvalidOperationException("Replay validation advice markdown path was not resolved.");
            var adviceLogPath = paths.AdviceLogPath ?? throw new InvalidOperationException("Replay validation advice log path was not resolved.");

            var promptPackPath = Path.Combine(promptPacksRoot, $"{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}-replay.json");
            await store.WriteJsonAsync(promptPackPath, inputPack, cancellationToken).ConfigureAwait(false);
            await store.WriteJsonAsync(adviceJsonPath, response, cancellationToken).ConfigureAwait(false);
            await File.WriteAllTextAsync(adviceMarkdownPath, _promptBuilder.FormatAdviceMarkdown(response), cancellationToken).ConfigureAwait(false);
            await store.AppendNdjsonAsync(adviceLogPath, response, cancellationToken).ConfigureAwait(false);
            await store.WriteJsonAsync(
                paths.CurrentRunStatePath,
                new
                {
                    runId,
                    sourceFixtureRoot = resolvedFixtureRoot,
                    sessionId = response.SessionId,
                    updatedAt = DateTimeOffset.UtcNow,
                },
                cancellationToken).ConfigureAwait(false);
            await store.WriteJsonAsync(
                paths.HostStatusPath,
                new
                {
                    state = "replay-validation",
                    runId,
                    codexAvailable = false,
                    updatedAt = DateTimeOffset.UtcNow,
                    message = "Replay validation completed without live Codex execution.",
                },
                cancellationToken).ConfigureAwait(false);

            return new ReplayValidationResult(
                resolvedFixtureRoot,
                runId,
                promptPackPath,
                adviceJsonPath,
                adviceMarkdownPath,
                slice.Entries.Count,
                response.Status,
                runState.NormalizedState.Scene.SceneType);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Replay validation failed for fixture '{fixtureRoot}': {exception.Message}", exception);
        }
    }

    private async Task<AdviceResponse> CreateResponseAsync(
        CompanionRunState runState,
        KnowledgeSlice slice,
        AdviceInputPack inputPack,
        string? mockAdviceResponsePath,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(mockAdviceResponsePath))
        {
            var mock = await ReadJsonAsync<AdviceResponse>(mockAdviceResponsePath, cancellationToken).ConfigureAwait(false);
            if (mock is not null)
            {
                return RewardRecommendationTraceBuilder.AlignToOptionSet(
                    inputPack,
                    mock with
                {
                    GeneratedAt = DateTimeOffset.UtcNow,
                    RunId = runState.Snapshot.RunId,
                    TriggerKind = "replay-validation",
                });
            }
        }

        var missingInformation = runState.NormalizedState.Unknowns
            .Concat(runState.Snapshot.Warnings.Where(warning => !string.IsNullOrWhiteSpace(warning)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var blockers = new List<string> { "codex-not-invoked-for-replay-validation" };
        if (slice.Entries.Count == 0)
        {
            blockers.Add("knowledge-slice-empty");
        }

        var response = new AdviceResponse(
            "degraded",
            "Replay validation fallback",
            "Prompt pack and knowledge slice were generated without live Codex execution. This artifact confirms the advisor pipeline can normalize state and build advice inputs from a replay fixture.",
            "Review the generated prompt pack and fill any missing state before relying on automated advice.",
            null,
            new[]
            {
                $"screen={runState.NormalizedState.Scene.SceneType}",
                $"choices={runState.NormalizedState.Choices.List.Count}",
                $"knowledge_entries={slice.Entries.Count}",
            },
            new[]
            {
                "This advice was generated without a live Codex session.",
            },
            missingInformation,
            blockers,
            null,
            slice.Entries.Take(5).Select(entry => entry.Id).ToArray(),
            DateTimeOffset.UtcNow,
            runState.Snapshot.RunId,
            "replay-validation",
            null,
            null);
        return RewardRecommendationTraceBuilder.AlignToOptionSet(inputPack, response);
    }

    private string ResolveFixtureRoot(string fixtureRoot)
    {
        var candidate = Path.GetFullPath(fixtureRoot, _workspaceRoot);
        var liveMirrorCandidate = Path.Combine(candidate, "live-mirror");
        return Directory.Exists(liveMirrorCandidate) ? liveMirrorCandidate : candidate;
    }

    private static async Task<T?> ReadJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        return await JsonSerializer.DeserializeAsync<T>(stream, ConfigurationLoader.JsonOptions, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<LiveExportEventEnvelope>> ReadEventsAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<LiveExportEventEnvelope>();
        }

        var events = new List<LiveExportEventEnvelope>();
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var envelope = JsonSerializer.Deserialize<LiveExportEventEnvelope>(line, ConfigurationLoader.JsonOptions);
                if (envelope is not null)
                {
                    events.Add(envelope);
                }
            }
            catch (JsonException)
            {
                // Ignore malformed event lines in replay fixtures.
            }
        }

        return events;
    }

    private static LiveExportSnapshot SanitizeSnapshot(LiveExportSnapshot snapshot)
    {
        return snapshot with
        {
            Player = snapshot.Player ?? LiveExportPlayerSummary.Empty,
            Deck = snapshot.Deck ?? Array.Empty<LiveExportCardSummary>(),
            Relics = snapshot.Relics ?? Array.Empty<string>(),
            Potions = snapshot.Potions ?? Array.Empty<string>(),
            CurrentChoices = snapshot.CurrentChoices ?? Array.Empty<LiveExportChoiceSummary>(),
            RecentChanges = snapshot.RecentChanges ?? Array.Empty<string>(),
            Warnings = snapshot.Warnings ?? Array.Empty<string>(),
            Meta = snapshot.Meta ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase),
        };
    }
}

public sealed record ReplayValidationResult(
    string FixtureRoot,
    string RunId,
    string PromptPackPath,
    string AdviceJsonPath,
    string AdviceMarkdownPath,
    int KnowledgeEntryCount,
    string AdviceStatus,
    string SceneType);
