using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;

sealed class ArtifactRecorder
{
    private readonly string _runRoot;
    private readonly string _tracePath;
    private readonly string _progressPath;
    private readonly string _manifestPath;
    private readonly string _humanLogPath;
    private string? _lastObserverInventoryJson;
    private string? _lastObserverEventsJson;

    public ArtifactRecorder(string runRoot)
    {
        _runRoot = runRoot;
        _tracePath = Path.Combine(runRoot, "trace.ndjson");
        _progressPath = Path.Combine(runRoot, "progress.ndjson");
        _manifestPath = Path.Combine(runRoot, "run.json");
        _humanLogPath = Path.Combine(runRoot, "run.log");
    }

    public string RunRoot => _runRoot;

    public void WriteRunManifest(GuiSmokeRunManifest manifest)
    {
        LiveExportAtomicFileWriter.WriteJsonAtomic(_manifestPath, manifest, GuiSmokeShared.JsonOptions);
    }

    public void CompleteRun(string status, string message)
    {
        var manifest = JsonSerializer.Deserialize<GuiSmokeRunManifest>(File.ReadAllText(_manifestPath), GuiSmokeShared.JsonOptions)
                       ?? throw new InvalidOperationException("Failed to reload run manifest.");
        manifest.Status = status;
        manifest.ResultMessage = message;
        manifest.CompletedAt = DateTimeOffset.UtcNow;
        LiveExportAtomicFileWriter.WriteJsonAtomic(_manifestPath, manifest, GuiSmokeShared.JsonOptions);
    }

    public void WriteFailureSummary(GuiSmokeFailureSummary summary)
    {
        LiveExportAtomicFileWriter.WriteJsonAtomic(Path.Combine(_runRoot, "failure-summary.json"), summary, GuiSmokeShared.JsonOptions);
    }

    public void AppendTrace(GuiSmokeTraceEntry entry)
    {
        LiveExportAtomicFileWriter.AppendAllTextShared(
            _tracePath,
            JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions) + Environment.NewLine);
    }

    public void WriteObserverCopies(string stepPrefix, ObserverState observer)
    {
        if (observer.StateDocument is not null)
        {
            File.WriteAllText(stepPrefix + ".observer.state.json", observer.StateDocument.RootElement.GetRawText());
        }

        if (observer.InventoryDocument is not null)
        {
            var inventoryJson = observer.InventoryDocument.RootElement.GetRawText();
            if (!string.Equals(_lastObserverInventoryJson, inventoryJson, StringComparison.Ordinal))
            {
                File.WriteAllText(stepPrefix + ".observer.inventory.json", inventoryJson);
                _lastObserverInventoryJson = inventoryJson;
            }
        }

        if (observer.EventLines is { Length: > 0 })
        {
            var eventsJson = JsonSerializer.Serialize(observer.EventLines, GuiSmokeShared.JsonOptions);
            if (!string.Equals(_lastObserverEventsJson, eventsJson, StringComparison.Ordinal))
            {
                File.WriteAllText(stepPrefix + ".observer.events.tail.json", eventsJson);
                _lastObserverEventsJson = eventsJson;
            }
        }
    }

    public void WriteRequest(string path, GuiSmokeStepRequest request)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(request, GuiSmokeShared.JsonOptions));
    }

    public void WriteDecision(string path, GuiSmokeStepDecision decision)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(decision, GuiSmokeShared.JsonOptions));
    }

    public void AppendProgress(GuiSmokeStepProgress entry)
    {
        LiveExportAtomicFileWriter.AppendAllTextShared(
            _progressPath,
            JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions) + Environment.NewLine);
    }

    public void WriteSelfMetaReview(GuiSmokeSelfMetaReview review)
    {
        LiveExportAtomicFileWriter.WriteJsonAtomic(Path.Combine(_runRoot, "self-meta-review.json"), review, GuiSmokeShared.JsonOptions);
    }

    public void AppendHumanLog(string line)
    {
        LiveExportAtomicFileWriter.AppendAllTextShared(
            _humanLogPath,
            line + Environment.NewLine);
    }

    public void WriteValidationSummary(string runId)
    {
        var eventCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var sceneCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var totalTraceEntries = 0;

        if (File.Exists(_tracePath))
        {
            foreach (var line in File.ReadLines(_tracePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                GuiSmokeTraceEntry? entry;
                try
                {
                    entry = JsonSerializer.Deserialize<GuiSmokeTraceEntry>(line, GuiSmokeShared.JsonOptions);
                }
                catch (JsonException)
                {
                    continue;
                }

                if (entry is null)
                {
                    continue;
                }

                totalTraceEntries += 1;
                IncrementCounter(eventCounts, entry.EventKind);
                IncrementCounter(sceneCounts, entry.ObserverScreen ?? "unknown");
            }
        }

        var summary = new GuiSmokeValidationSummary(
            runId,
            totalTraceEntries,
            eventCounts,
            sceneCounts);
        LiveExportAtomicFileWriter.WriteJsonAtomic(Path.Combine(_runRoot, "validation-summary.json"), summary, GuiSmokeShared.JsonOptions);
    }

    private static void IncrementCounter(Dictionary<string, int> counters, string key)
    {
        counters[key] = counters.TryGetValue(key, out var current)
            ? current + 1
            : 1;
    }
}
