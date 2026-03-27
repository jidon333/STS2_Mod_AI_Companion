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

static partial class LongRunArtifacts
{
    private static JsonNode? TryReadJsonNode(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(File.ReadAllText(path));
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

    private static bool? TryReadBoolean(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        try
        {
            return node.GetValue<bool>();
        }
        catch (InvalidOperationException)
        {
            try
            {
                var text = node.GetValue<string>();
                return bool.TryParse(text, out var parsed)
                    ? parsed
                    : null;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }

    private static DateTimeOffset? TryReadDateTimeOffset(JsonNode? node)
    {
        var text = TryReadString(node);
        return DateTimeOffset.TryParse(text, out var parsed)
            ? parsed
            : null;
    }

    private static string? TryReadString(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        try
        {
            return node.GetValue<string>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static bool IsCompanionDisabledInSettings(JsonObject? settingsRoot, string companionPckBaseName)
    {
        if (settingsRoot?["mod_settings"] is not JsonObject modSettings
            || modSettings["disabled_mods"] is not JsonArray disabledMods)
        {
            return false;
        }

        foreach (var entry in disabledMods)
        {
            if (entry is not JsonObject disabledMod)
            {
                continue;
            }

            var name = TryReadString(disabledMod["name"]);
            if (string.Equals(name, companionPckBaseName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPassedAttempt(GuiSmokeAttemptIndexEntry entry)
    {
        var completedLike = string.Equals(entry.Status, "passed", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(entry.Status, "completed", StringComparison.OrdinalIgnoreCase);
        if (!completedLike)
        {
            return false;
        }

        if (!string.Equals(entry.TrustStateAtStart, GuiSmokeContractStates.TrustValid, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (entry.LaunchFailed)
        {
            return false;
        }

        return !string.Equals(entry.TerminalCause, "max-steps-reached", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildLogTailBody(string sourcePath, IReadOnlyList<string> matchedLines)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"source: {sourcePath}");
        if (matchedLines.Count == 0)
        {
            builder.AppendLine("<no-matching-lines>");
            return builder.ToString();
        }

        foreach (var line in matchedLines)
        {
            builder.AppendLine(line);
        }

        return builder.ToString();
    }
    private static string TrimOutputTail(string? text, int maxLength = 4000)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text.Replace("\r", string.Empty).Trim();
        return normalized.Length <= maxLength
            ? normalized
            : normalized[^maxLength..];
    }

    private static string? CombineFailureReasons(string? primary, string? secondary)
    {
        if (string.IsNullOrWhiteSpace(primary))
        {
            return string.IsNullOrWhiteSpace(secondary) ? null : secondary;
        }

        if (string.IsNullOrWhiteSpace(secondary))
        {
            return primary;
        }

        return $"{primary} | {secondary}";
    }

    private static void WriteJsonWithFallback<T>(string path, T value, JsonSerializerOptions options)
    {
        try
        {
            WriteJsonAtomicWithRetry(path, value, options);
        }
        catch
        {
            File.WriteAllText(path, JsonSerializer.Serialize(value, options));
        }
    }

    private static string? TryWriteJsonWithFallback<T>(string path, T value, JsonSerializerOptions options, string artifactName)
    {
        try
        {
            WriteJsonWithFallback(path, value, options);
            return null;
        }
        catch (Exception exception)
        {
            return $"summary-persist-failure:{artifactName}:{exception.GetType().Name}:{exception.Message}";
        }
    }

    private static string? TryWritePlainJson<T>(string path, T value, JsonSerializerOptions options, string artifactName)
    {
        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(value, options));
            return null;
        }
        catch (Exception exception)
        {
            return $"summary-persist-failure:{artifactName}:{exception.GetType().Name}:{exception.Message}";
        }
    }
    private static string? TryNormalizeRuntimeConfigForDeployVerification(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var node = JsonNode.Parse(File.ReadAllText(path));
            if (node is not JsonObject root)
            {
                return null;
            }

            if (root["harness"] is not JsonObject harness)
            {
                return null;
            }

            harness["enabled"] = true;
            root.Remove("startupSentinel");
            return root.ToJsonString();
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

    private static string ComputeSha256Utf8(string value)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private static GuiSmokeFileIdentityEvidence DescribeFile(string path)
    {
        var info = new FileInfo(path);
        return new GuiSmokeFileIdentityEvidence(
            path,
            info.Length,
            info.LastWriteTimeUtc,
            ComputeFullFileSha256(path));
    }

    private static int CountScreenshots(string stepsRoot)
    {
        return Directory.Exists(stepsRoot)
            ? Directory.GetFiles(stepsRoot, "*.screen.png", SearchOption.TopDirectoryOnly).Length
            : 0;
    }

    private static string ComputeFullFileSha256(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(stream));
        }
        catch
        {
            return "unavailable";
        }
    }

    private static JsonDocument? TryReadJsonDocument(string path)
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
        catch
        {
            return null;
        }
    }

    private static string? TryReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static T? TryReadJson<T>(string path)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path), GuiSmokeShared.JsonOptions);
        }
        catch (IOException)
        {
            return default;
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static List<T> ReadNdjsonRecords<T>(string path)
    {
        var entries = new List<T>();
        if (!File.Exists(path))
        {
            return entries;
        }

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            T? entry;
            try
            {
                entry = JsonSerializer.Deserialize<T>(line, GuiSmokeShared.JsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (entry is not null)
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

    private static void UpsertNdjson<T>(string path, T entry, Func<T, string> keySelector, string key)
    {
        var existingEntries = new List<T>();
        if (File.Exists(path))
        {
            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                T? existing;
                try
                {
                    existing = JsonSerializer.Deserialize<T>(line, GuiSmokeShared.JsonOptions);
                }
                catch (JsonException)
                {
                    continue;
                }

                if (existing is null)
                {
                    continue;
                }

                if (string.Equals(keySelector(existing), key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                existingEntries.Add(existing);
            }
        }

        existingEntries.Add(entry);
        var lines = existingEntries
            .Select(item => JsonSerializer.Serialize(item, GuiSmokeShared.NdjsonOptions))
            .ToArray();
        WriteJsonAtomicWithRetry(path + ".tmp.json", lines, GuiSmokeShared.JsonOptions);
        File.WriteAllLines(path, lines);
        File.Delete(path + ".tmp.json");
    }

    private static void WriteJsonAtomicWithRetry<T>(
        string path,
        T value,
        JsonSerializerOptions options,
        int maxAttempts = 6,
        int retryDelayMs = 25)
    {
        for (var attempt = 1; attempt <= Math.Max(1, maxAttempts); attempt += 1)
        {
            try
            {
                LiveExportAtomicFileWriter.WriteJsonAtomic(path, value, options);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(retryDelayMs * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(retryDelayMs * attempt);
            }
        }

        LiveExportAtomicFileWriter.WriteJsonAtomic(path, value, options);
    }

    private static string GetGoalContractPath(string sessionRoot) => Path.Combine(sessionRoot, "goal-contract.json");

    private static string GetPrevalidationPath(string sessionRoot) => Path.Combine(sessionRoot, "prevalidation.json");

    private static string GetRestartEventsPath(string sessionRoot) => Path.Combine(sessionRoot, "restart-events.ndjson");

    private static string GetSupervisorStatePath(string sessionRoot) => Path.Combine(sessionRoot, "supervisor-state.json");

    private static string GetStallDiagnosisPath(string sessionRoot) => Path.Combine(sessionRoot, "stall-diagnosis.ndjson");

    private static string GetStartupTracePath(string sessionRoot) => Path.Combine(sessionRoot, "startup-trace.ndjson");

    private static string GetStartupSummaryPath(string sessionRoot) => Path.Combine(sessionRoot, "startup-summary.json");

    private static string GetStartupLogBaselinePath(string sessionRoot) => Path.Combine(sessionRoot, "startup-log-baseline.json");

    private static string GetStartupRuntimeEvidencePath(string sessionRoot) => Path.Combine(sessionRoot, "startup-runtime-evidence.json");

    private static string GetStartupRuntimeCapturesPath(string sessionRoot) => Path.Combine(sessionRoot, "startup-runtime-captures.ndjson");

    private static string GetStartupRuntimeLogTailPath(string sessionRoot) => Path.Combine(sessionRoot, "startup-runtime-log.tail.txt");

    private static string GetStartupRuntimeLogDeltaPath(string sessionRoot) => Path.Combine(sessionRoot, "startup-runtime-log.delta.txt");

    private static string GetStartupGodotLogTailPath(string sessionRoot) => Path.Combine(sessionRoot, "startup-godot-log.tail.txt");

    private static string GetStartupGodotLogDeltaPath(string sessionRoot) => Path.Combine(sessionRoot, "startup-godot-log.delta.txt");

    private static string GetDeployCommandSummaryPath(string sessionRoot) => Path.Combine(sessionRoot, "deploy-command-summary.json");
}
