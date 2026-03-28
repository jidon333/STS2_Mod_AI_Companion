using System.Text.Json;
using System.Threading;
using System.Security.Cryptography;
using Sts2ModKit.Core.Configuration;

namespace Sts2ModKit.Core.LiveExport;

public static class LiveExportPathResolver
{
    public static string BuildModdedProfileRoot(GamePathOptions gamePaths)
    {
        return Path.Combine(
            gamePaths.UserDataRoot,
            "steam",
            gamePaths.SteamAccountId,
            "modded",
            $"profile{gamePaths.ProfileIndex}");
    }

    public static LiveExportLayout Resolve(GamePathOptions gamePaths, LiveExportOptions options)
    {
        var moddedProfileRoot = BuildModdedProfileRoot(gamePaths);
        var liveRoot = CombineRelativeSegments(moddedProfileRoot, options.RelativeLiveRoot);
        return new LiveExportLayout(
            moddedProfileRoot,
            liveRoot,
            Path.Combine(liveRoot, options.EventsFileName),
            Path.Combine(liveRoot, options.SnapshotFileName),
            Path.Combine(liveRoot, options.SummaryFileName),
            Path.Combine(liveRoot, options.SessionFileName))
        {
            RawObservationsPath = Path.Combine(liveRoot, options.RawObservationsFileName),
            ScreenTransitionsPath = Path.Combine(liveRoot, options.ScreenTransitionsFileName),
            ChoiceCandidatesPath = Path.Combine(liveRoot, options.ChoiceCandidatesFileName),
            ChoiceDecisionsPath = Path.Combine(liveRoot, options.ChoiceDecisionsFileName),
            SemanticSnapshotsRoot = Path.Combine(liveRoot, options.SemanticSnapshotsFolderName),
        };
    }

    public static void EnsureDirectory(LiveExportLayout layout)
    {
        Directory.CreateDirectory(layout.LiveRoot);
        Directory.CreateDirectory(layout.SemanticSnapshotsRoot);
    }

    private static string CombineRelativeSegments(string root, string relativePath)
    {
        var segments = relativePath
            .Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return segments.Aggregate(root, Path.Combine);
    }
}

public sealed class LiveExportDeduplicator
{
    private readonly object _sync = new();
    private readonly Dictionary<LiveExportDedupKey, DateTimeOffset> _seen = new();
    private readonly TimeSpan _window;

    public LiveExportDeduplicator(TimeSpan window)
    {
        _window = window;
    }

    public bool ShouldSuppress(LiveExportObservation observation)
    {
        lock (_sync)
        {
            var key = new LiveExportDedupKey(
                observation.TriggerKind,
                observation.Screen ?? string.Empty,
                BuildPayloadSignature(observation.Payload));

            var now = observation.ObservedAt;
            if (_seen.TryGetValue(key, out var lastSeenAt) && now - lastSeenAt < _window)
            {
                return true;
            }

            _seen[key] = now;
            return false;
        }
    }

    private static string BuildPayloadSignature(IReadOnlyDictionary<string, object?> payload)
    {
        if (payload.Count == 0)
        {
            return "<empty>";
        }

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        });
    }
}

public static class LiveExportAtomicFileWriter
{
    public static void WriteAllTextAtomic(string path, string contents, int maxAttempts = 6, int retryDelayMs = 25)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var tempPath = path + ".tmp";

        for (var attempt = 1; attempt <= Math.Max(1, maxAttempts); attempt += 1)
        {
            try
            {
                File.WriteAllText(tempPath, contents);

                if (File.Exists(path))
                {
                    File.Replace(tempPath, path, destinationBackupFileName: null, ignoreMetadataErrors: true);
                    return;
                }

                File.Move(tempPath, path);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                TryDeleteTemp(tempPath);
                Thread.Sleep(retryDelayMs * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                TryDeleteTemp(tempPath);
                Thread.Sleep(retryDelayMs * attempt);
            }
        }

        File.WriteAllText(tempPath, contents);
        if (File.Exists(path))
        {
            File.Replace(tempPath, path, destinationBackupFileName: null, ignoreMetadataErrors: true);
            return;
        }

        File.Move(tempPath, path);
    }

    public static void WriteJsonAtomic<T>(string path, T value)
    {
        WriteJsonAtomic(path, value, options: null);
    }

    public static void WriteJsonAtomic<T>(string path, T value, JsonSerializerOptions? options = null)
    {
        options ??= new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        WriteAllTextAtomic(path, JsonSerializer.Serialize(value, options));
    }

    public static void AppendAllTextShared(string path, string contents, int maxAttempts = 5, int retryDelayMs = 20)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        for (var attempt = 1; attempt <= Math.Max(1, maxAttempts); attempt += 1)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
                using var writer = new StreamWriter(stream);
                writer.Write(contents);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(retryDelayMs);
            }
        }

        using var fallbackStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
        using var fallbackWriter = new StreamWriter(fallbackStream);
        fallbackWriter.Write(contents);
    }

    public static string ComputeSha256(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var sha256 = SHA256.Create();
        return Convert.ToHexString(sha256.ComputeHash(stream));
    }

    private static void TryDeleteTemp(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
        catch
        {
        }
    }
}
