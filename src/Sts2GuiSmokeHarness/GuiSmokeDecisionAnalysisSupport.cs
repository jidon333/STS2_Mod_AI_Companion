using System.Collections.Concurrent;
using System.Text.Json;

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
