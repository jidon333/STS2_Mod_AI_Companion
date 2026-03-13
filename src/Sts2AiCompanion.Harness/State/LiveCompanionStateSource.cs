using System.Text.Json;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Foundation.State;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Harness.State;

public sealed class LiveCompanionStateSource
{
    private readonly LiveExportLayout _layout;
    private readonly HarnessQueueLayout _harnessLayout;
    private long _lastObservedSeq;

    public LiveCompanionStateSource(ScaffoldConfiguration configuration)
    {
        _layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        _harnessLayout = HarnessPathResolver.Resolve(configuration.GamePaths, configuration.Harness);
    }

    public async Task<CompanionState> ReadAsync(CancellationToken cancellationToken)
    {
        var snapshot = await TryReadJsonAsync<LiveExportSnapshot>(_layout.SnapshotPath, cancellationToken).ConfigureAwait(false);
        if (snapshot is null)
        {
            return CompanionState.CreateUnknown();
        }

        snapshot = SanitizeSnapshot(snapshot);
        var session = await TryReadJsonAsync<LiveExportSession>(_layout.SessionPath, cancellationToken).ConfigureAwait(false);
        var recentEvents = await ReadNewEventsAsync(_layout.EventsPath, cancellationToken).ConfigureAwait(false);
        var state = CompanionStateMapper.FromLiveExport(snapshot, session, recentEvents);
        var latestHarnessResult = await TryReadLatestHarnessResultAsync(cancellationToken).ConfigureAwait(false);
        return ApplyHarnessOverlay(state, latestHarnessResult);
    }

    private async Task<IReadOnlyList<LiveExportEventEnvelope>> ReadNewEventsAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<LiveExportEventEnvelope>();
        }

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream);
        var events = new List<LiveExportEventEnvelope>();
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
                if (envelope is not null && envelope.Seq > _lastObservedSeq)
                {
                    events.Add(envelope);
                    _lastObservedSeq = Math.Max(_lastObservedSeq, envelope.Seq);
                }
            }
            catch (JsonException)
            {
                // Ignore malformed event lines.
            }
        }

        return events;
    }

    private static async Task<T?> TryReadJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt += 1)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!File.Exists(path))
                {
                    await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                return await JsonSerializer.DeserializeAsync<T>(stream, ConfigurationLoader.JsonOptions, cancellationToken).ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }
            catch (DirectoryNotFoundException)
            {
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }
            catch (IOException)
            {
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }
            catch (JsonException)
            {
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }
        }

        return default;
    }

    private async Task<HarnessActionResult?> TryReadLatestHarnessResultAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_harnessLayout.ResultsPath))
        {
            return null;
        }

        HarnessActionResult? latest = null;
        using var stream = new FileStream(_harnessLayout.ResultsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
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
                var parsed = JsonSerializer.Deserialize<HarnessActionResult>(line, ConfigurationLoader.JsonOptions);
                if (parsed is null)
                {
                    continue;
                }

                if (latest is null || parsed.CompletedAt > latest.CompletedAt)
                {
                    latest = parsed;
                }
            }
            catch (JsonException)
            {
                // Ignore malformed result lines.
            }
        }

        return latest;
    }

    private static CompanionState ApplyHarnessOverlay(CompanionState state, HarnessActionResult? latestHarnessResult)
    {
        if (latestHarnessResult is null
            || !string.Equals(latestHarnessResult.Status, "ok", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(latestHarnessResult.ObservedStateDelta))
        {
            return state;
        }

        if (DateTimeOffset.UtcNow - latestHarnessResult.CompletedAt > TimeSpan.FromSeconds(5))
        {
            return state;
        }

        if (state.CapturedAt - latestHarnessResult.CompletedAt > TimeSpan.FromSeconds(2))
        {
            return state;
        }

        var overlayScene = TryInferSceneFromObservedDelta(latestHarnessResult.ObservedStateDelta);
        if (string.IsNullOrWhiteSpace(overlayScene))
        {
            return state;
        }

        if (!ShouldPromoteHarnessScene(state, overlayScene))
        {
            return state;
        }

        if (SceneRank(overlayScene) <= SceneRank(state.Scene.SceneType))
        {
            return state;
        }

        var warnings = state.Warnings.Concat(new[] { $"harness-overlay:{overlayScene}" }).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var unknowns = state.Unknowns
            .Where(value => !string.Equals(value, "scene-unknown", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var confidence = new Dictionary<string, double>(state.Confidence, StringComparer.OrdinalIgnoreCase)
        {
            ["scene"] = Math.Max(state.Confidence.TryGetValue("scene", out var existing) ? existing : 0.0, 0.9),
        };

        return state with
        {
            Scene = state.Scene with
            {
                SceneType = overlayScene,
                SemanticSceneType = overlayScene,
                Confidence = confidence["scene"],
                Source = "harness-result",
            },
            Transition = state.Transition with
            {
                CurrentScene = overlayScene,
                Marker = $"harness-result:{latestHarnessResult.ActionId}",
            },
            Warnings = warnings,
            Unknowns = unknowns,
            Confidence = confidence,
        };
    }

    private static bool ShouldPromoteHarnessScene(CompanionState state, string overlayScene)
    {
        if (!string.Equals(overlayScene, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(state.Scene.SceneType, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return HasCombatEvidence(state);
    }

    private static string? TryInferSceneFromObservedDelta(string observedStateDelta)
    {
        if (string.IsNullOrWhiteSpace(observedStateDelta))
        {
            return null;
        }

        var segments = observedStateDelta
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .ToArray();

        foreach (var segment in segments)
        {
            if (TryInferSceneFromSegment(segment, out var inferred))
            {
                return inferred;
            }
        }

        return null;
    }

    private static bool TryInferSceneFromSegment(string segment, out string? scene)
    {
        var lowered = segment.Trim().ToLowerInvariant();
        scene = lowered switch
        {
            "singleplayer-button-pressed" => "main-menu",
            "push-character-select" => "character-select",
            "character-select" => "character-select",
            "character-selected" => "character-select",
            "open-character-select" => "character-select",
            "singleplayer-submenu" => "singleplayer-submenu",
            "main-menu" => "main-menu",
            "map" => "map",
            "map-node-entered" => "map",
            "map-point-selected" => "map",
            "combat" => "combat",
            "already-combat" => "combat",
            "rewards" => "rewards",
            "reward" => "rewards",
            "event" => "event",
            "shop" => "shop",
            "rest" => "rest-site",
            "rest-site" => "rest-site",
            _ => null,
        };
        if (!string.IsNullOrWhiteSpace(scene))
        {
            return true;
        }

        if (lowered.StartsWith("live-export:", StringComparison.Ordinal))
        {
            var candidate = lowered["live-export:".Length..];
            scene = candidate switch
            {
                "reward" => "rewards",
                "rest" => "rest-site",
                _ => candidate,
            };
            return !string.IsNullOrWhiteSpace(scene);
        }

        if (lowered.StartsWith("screen:", StringComparison.Ordinal))
        {
            var candidate = lowered["screen:".Length..];
            scene = candidate switch
            {
                "reward" => "rewards",
                "rest" => "rest-site",
                _ => candidate,
            };
            return !string.IsNullOrWhiteSpace(scene);
        }

        if (lowered.StartsWith("run-started:", StringComparison.Ordinal))
        {
            var candidate = lowered["run-started:".Length..];
            if (candidate.Contains("map", StringComparison.Ordinal))
            {
                scene = "map";
                return true;
            }

            if (candidate.Contains("reward", StringComparison.Ordinal))
            {
                scene = "rewards";
                return true;
            }

            if (candidate.Contains("combat", StringComparison.Ordinal))
            {
                scene = "combat";
                return true;
            }
        }

        scene = null;
        return false;
    }

    private static bool HasCombatEvidence(CompanionState state)
    {
        if (state.Combat.Turn is > 0 || !string.IsNullOrWhiteSpace(state.Combat.HandSummary))
        {
            return true;
        }

        return state.Choices.List.Any(choice => IsLikelyCombatChoice(choice.Label));
    }

    private static bool IsLikelyCombatChoice(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        var normalized = label.Trim();
        if (normalized.Equals("Dismisser", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Exclaim", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Question", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("BackButton", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Send!", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return normalized.Contains("strike", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("defend", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("bash", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("card", StringComparison.OrdinalIgnoreCase);
    }

    private static int SceneRank(string? sceneType)
    {
        var normalized = sceneType?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "startup" => 0,
            "main-menu" => 1,
            "singleplayer-submenu" => 2,
            "character-select" => 3,
            "map" => 4,
            "combat" => 5,
            "rewards" => 6,
            "event" => 7,
            "shop" => 8,
            "rest-site" => 9,
            _ => -1,
        };
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
