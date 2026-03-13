using System.Text.Json;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModAiCompanion.HarnessBridge;

internal sealed class TraceWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    private readonly string _tracePath;

    public TraceWriter(string tracePath)
    {
        _tracePath = tracePath;
    }

    public void Write(string kind, string? actionId, object? payload)
    {
        var entry = new TraceEnvelope(DateTimeOffset.UtcNow, kind, actionId, payload);
        var line = JsonSerializer.Serialize(entry, JsonOptions) + Environment.NewLine;
        LiveExportAtomicFileWriter.AppendAllTextShared(_tracePath, line);
    }

    private sealed record TraceEnvelope(
        DateTimeOffset Ts,
        string Kind,
        string? ActionId,
        object? Payload);
}
