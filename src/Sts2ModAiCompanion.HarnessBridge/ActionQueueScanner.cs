using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sts2ModAiCompanion.HarnessBridge;

internal sealed class ActionQueueScanner
{
    private readonly HashSet<string> _seenActionIds = new(StringComparer.Ordinal);

    public IReadOnlyList<QueuedHarnessAction> ReadPendingActions(string actionsPath)
    {
        if (!File.Exists(actionsPath))
        {
            return Array.Empty<QueuedHarnessAction>();
        }

        var pending = new List<QueuedHarnessAction>();
        foreach (var line in File.ReadLines(actionsPath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var action = TryParse(line);
            if (action is null)
            {
                continue;
            }

            if (_seenActionIds.Add(action.ActionId))
            {
                pending.Add(action);
            }
        }

        return pending;
    }

    private static QueuedHarnessAction? TryParse(string line)
    {
        try
        {
            using var document = JsonDocument.Parse(line);
            if (!document.RootElement.TryGetProperty("action", out var actionElement)
                || actionElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var actionId = ReadString(actionElement, "actionId") ?? ComputeHash(line);
            var kind = ReadString(actionElement, "kind") ?? "unknown";
            var targetLabel = ReadString(actionElement, "targetLabel");
            var requestedAt = ReadDateTimeOffset(actionElement, "requestedAt");
            string? sessionToken = null;
            if (actionElement.TryGetProperty("metadata", out var metadata)
                && metadata.ValueKind == JsonValueKind.Object)
            {
                sessionToken = ReadString(metadata, "sessionToken");
            }

            return new QueuedHarnessAction(actionId, kind, targetLabel, sessionToken, requestedAt);
        }
        catch (JsonException)
        {
            return new QueuedHarnessAction(ComputeHash(line), "invalid-json", null, null, null);
        }
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static DateTimeOffset? ReadDateTimeOffset(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.String
               && DateTimeOffset.TryParse(property.GetString(), out var parsed)
            ? parsed
            : null;
    }

    private static string ComputeHash(string line)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(line));
        return Convert.ToHexString(bytes[..8]);
    }

    internal sealed record QueuedHarnessAction(
        string ActionId,
        string Kind,
        string? TargetLabel,
        string? SessionToken,
        DateTimeOffset? RequestedAt);
}
