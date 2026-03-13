using System.Text.Json;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Harness;

namespace Sts2AiCompanion.Harness.Actions;

public sealed class BridgeActionExecutor : IHarnessActionExecutor
{
    private const int TimeoutGraceMs = 3_000;
    private const string SessionTokenMetadataKey = "sessionToken";
    private readonly HarnessQueueLayout _layout;
    private readonly string? _sessionToken;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public BridgeActionExecutor(HarnessQueueLayout layout, string? sessionToken = null)
    {
        _layout = layout;
        _sessionToken = string.IsNullOrWhiteSpace(sessionToken) ? null : sessionToken.Trim();
        HarnessPathResolver.EnsureDirectories(layout);
    }

    public async Task<HarnessActionResult> ExecuteAsync(HarnessAction action, CompanionState state, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var queuedAction = StampSessionToken(action);
        var payload = new
        {
            action = queuedAction,
            state,
        };

        await AppendActionAsync(
            JsonSerializer.Serialize(payload, _jsonOptions) + Environment.NewLine,
            cancellationToken).ConfigureAwait(false);

        var deadline = startedAt.AddMilliseconds(Math.Max(queuedAction.TimeoutMs, 1_000) + TimeoutGraceMs);
        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var existing = await TryReadResultAsync(queuedAction.ActionId, cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                return existing;
            }

            var bridgeStatus = await TryReadStatusAsync(cancellationToken).ConfigureAwait(false);
            if (bridgeStatus is not null
                && string.Equals(bridgeStatus.LastActionId, queuedAction.ActionId, StringComparison.Ordinal)
                && string.Equals(bridgeStatus.LastResultStatus, "pending", StringComparison.OrdinalIgnoreCase))
            {
                await Task.Delay(250, cancellationToken).ConfigureAwait(false);
                continue;
            }

            await Task.Delay(250, cancellationToken).ConfigureAwait(false);
        }

        return new HarnessActionResult(
            queuedAction.ActionId,
            "timeout",
            startedAt,
            DateTimeOffset.UtcNow,
            "bridge-timeout",
            true,
            null,
            Array.Empty<string>());
    }

    private HarnessAction StampSessionToken(HarnessAction action)
    {
        if (string.IsNullOrWhiteSpace(_sessionToken))
        {
            return action;
        }

        var metadata = new Dictionary<string, string?>(action.Metadata, StringComparer.OrdinalIgnoreCase)
        {
            [SessionTokenMetadataKey] = _sessionToken,
        };

        return action with { Metadata = metadata };
    }

    private async Task AppendActionAsync(string payload, CancellationToken cancellationToken)
    {
        var attempts = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await using var stream = new FileStream(
                    _layout.ActionsPath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.ReadWrite | FileShare.Delete);
                await using var writer = new StreamWriter(stream);
                await writer.WriteAsync(payload.AsMemory(), cancellationToken).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
                return;
            }
            catch (IOException) when (attempts < 20)
            {
                attempts += 1;
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task<HarnessActionResult?> TryReadResultAsync(string actionId, CancellationToken cancellationToken)
    {
        if (!File.Exists(_layout.ResultsPath))
        {
            return null;
        }

        using var stream = new FileStream(_layout.ResultsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
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
                var parsed = JsonSerializer.Deserialize<HarnessActionResult>(line, _jsonOptions);
                if (parsed is not null && string.Equals(parsed.ActionId, actionId, StringComparison.Ordinal))
                {
                    return parsed;
                }
            }
            catch (JsonException)
            {
                // Fall through to the manual parser below.
            }

            var fallback = TryParseResultFallback(line);
            if (fallback is not null && string.Equals(fallback.ActionId, actionId, StringComparison.Ordinal))
            {
                return fallback;
            }
        }

        return null;
    }

    private async Task<HarnessBridgeStatus?> TryReadStatusAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_layout.StatusPath))
        {
            return null;
        }

        await using var stream = new FileStream(_layout.StatusPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        return await JsonSerializer.DeserializeAsync<HarnessBridgeStatus>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false);
    }

    private static HarnessActionResult? TryParseResultFallback(string line)
    {
        try
        {
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var parsedActionId = ReadString(root, "actionId");
            if (string.IsNullOrWhiteSpace(parsedActionId))
            {
                return null;
            }

            return new HarnessActionResult(
                parsedActionId,
                ReadString(root, "status") ?? "unknown",
                ReadDateTimeOffset(root, "startedAt"),
                ReadDateTimeOffset(root, "completedAt"),
                ReadString(root, "failureKind"),
                ReadBool(root, "recoverable"),
                ReadString(root, "observedStateDelta"),
                ReadStringArray(root, "artifactRefs"));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ReadString(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static DateTimeOffset ReadDateTimeOffset(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var value) &&
            value.ValueKind == JsonValueKind.String &&
            DateTimeOffset.TryParse(value.GetString(), out var parsed))
        {
            return parsed;
        }

        return DateTimeOffset.UtcNow;
    }

    private static bool ReadBool(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value) &&
               value.ValueKind is JsonValueKind.True or JsonValueKind.False &&
               value.GetBoolean();
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        var results = new List<string>();
        foreach (var element in value.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(element.GetString()))
            {
                results.Add(element.GetString()!);
            }
        }

        return results;
    }
}
