using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModAiCompanion.Mod.Runtime;

internal static class RuntimeExportContext
{
    private static readonly object Sync = new();
    private static RuntimeExportWorker? _worker;
    private static bool _processExitSubscribed;

    public static bool EnsureInitialized()
    {
        if (_worker is not null)
        {
            return true;
        }

        lock (Sync)
        {
            if (_worker is not null)
            {
                return true;
            }

            var config = AiCompanionRuntimeState.GetConfig(forceRefresh: true);
            if (!config.Enabled || !config.LiveExport.Enabled)
            {
                AiCompanionRuntimeLog.WriteLine("runtime exporter disabled by config.");
                return false;
            }

            var layout = LiveExportPathResolver.Resolve(config.GamePaths, config.LiveExport);
            LiveExportPathResolver.EnsureDirectory(layout);

            _worker = new RuntimeExportWorker(config, layout);
            _worker.Enqueue(LiveExportObservation.Create("app-started", screen: "bootstrap"));

            if (!_processExitSubscribed)
            {
                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
                _processExitSubscribed = true;
            }

            AiCompanionRuntimeLog.WriteLine($"runtime exporter initialized. live_root={layout.LiveRoot}");
            AiCompanionRuntimeLog.WriteLine(
                $"runtime exporter config: discovery={config.LiveExport.DiscoveryMode} scene_polling={config.LiveExport.ScenePollingEnabled} interval_ms={config.LiveExport.ScenePollingIntervalMs} duplicate_suppression_ms={config.LiveExport.DuplicateSuppressionMs}");
            AiCompanionRuntimeLog.WriteLine(
                $"runtime exporter identity: {AiCompanionRuntimeIdentity.DescribeExecutingAssembly()}");
            return true;
        }
    }

    public static void RecordHookInvocation(MethodBase method, object? instance, object? result, object?[]? args)
    {
        if (!EnsureInitialized() || _worker is null)
        {
            return;
        }

        try
        {
            var binding = RuntimeHookCatalog.TryGetBinding(method);
            if (binding is null)
            {
                return;
            }

            var config = AiCompanionRuntimeState.GetConfig();
            var observation = RuntimeSnapshotReflectionExtractor.Capture(binding, config, instance, args, result);
            if (config.LiveExport.DiscoveryMode)
            {
                AiCompanionRuntimeLog.WriteLine(
                    $"hook observed: kind={binding.Candidate.SemanticKind} method={method.DeclaringType?.FullName}.{method.Name} screen={observation.Screen ?? "unknown"}");
            }

            _worker.Enqueue(observation);
        }
        catch (Exception exception)
        {
            AiCompanionRuntimeLog.WriteLine($"runtime exporter hook failure: {exception.Message}");
        }
    }

    private static void OnProcessExit(object? sender, EventArgs args)
    {
        try
        {
            _worker?.Enqueue(LiveExportObservation.Create("app-stopped", screen: "shutdown"));
        }
        catch
        {
            // Best effort only during shutdown.
        }
    }
}

internal sealed class RuntimeExportWorker
{
    private readonly AiCompanionRuntimeConfig _config;
    private readonly LiveExportLayout _layout;
    private readonly BlockingCollection<LiveExportObservation> _queue;
    private readonly LiveExportStateTracker _tracker;
    private readonly LiveExportDeduplicator _deduplicator;
    private readonly Thread _thread;
    private readonly Thread? _pollThread;

    public RuntimeExportWorker(AiCompanionRuntimeConfig config, LiveExportLayout layout)
    {
        _config = config;
        _layout = layout;
        _queue = new BlockingCollection<LiveExportObservation>(new ConcurrentQueue<LiveExportObservation>());
        _tracker = new LiveExportStateTracker(
            new LiveExportStateTrackerOptions(
                config.LiveExport.MaxRecentChanges,
                config.LiveExport.MaxDeckEntries,
                config.LiveExport.MaxChoiceEntries),
            layout.LiveRoot);
        _deduplicator = new LiveExportDeduplicator(TimeSpan.FromMilliseconds(config.LiveExport.DuplicateSuppressionMs));
        _thread = new Thread(Run)
        {
            IsBackground = true,
            Name = "STS2 Mod AI Companion Exporter",
        };

        _thread.Start();

        if (config.LiveExport.ScenePollingEnabled)
        {
            _pollThread = new Thread(PollLoop)
            {
                IsBackground = true,
                Name = "STS2 Mod AI Companion Scene Poller",
            };
            _pollThread.Start();
        }
    }

    public void Enqueue(LiveExportObservation observation)
    {
        if (_deduplicator.ShouldSuppress(observation))
        {
            return;
        }

        _queue.Add(observation);
    }

    private void Run()
    {
        foreach (var observation in _queue.GetConsumingEnumerable())
        {
            try
            {
                var batch = _tracker.Apply(observation);
                WriteBatch(batch);
            }
            catch (Exception exception)
            {
                AiCompanionRuntimeLog.WriteLine($"runtime exporter worker failure: {exception.Message}");
            }
        }
    }

    private void PollLoop()
    {
        while (true)
        {
            var config = AiCompanionRuntimeState.GetConfig();
            var delayMs = Math.Max(config.LiveExport.ScenePollingIntervalMs, 250);
            Thread.Sleep(delayMs);

            if (!config.Enabled || !config.LiveExport.Enabled || !config.LiveExport.ScenePollingEnabled)
            {
                continue;
            }

            try
            {
                var observation = RuntimeSnapshotReflectionExtractor.CaptureFromRuntime(config);
                if (observation is not null)
                {
                    Enqueue(observation);
                }
            }
            catch (Exception exception)
            {
                AiCompanionRuntimeLog.WriteLine($"runtime exporter poll failure: {exception.Message}");
            }
        }
    }

    private void WriteBatch(LiveExportBatch batch)
    {
        Directory.CreateDirectory(_layout.LiveRoot);

        foreach (var envelope in batch.Events)
        {
            AppendEvent(envelope);
        }

        LiveExportAtomicFileWriter.WriteAllTextAtomic(
            _layout.SnapshotPath,
            JsonSerializer.Serialize(batch.Snapshot, PrettyJsonOptions));
        LiveExportAtomicFileWriter.WriteAllTextAtomic(_layout.SummaryPath, LiveExportSummaryFormatter.Format(batch.Snapshot));
        LiveExportAtomicFileWriter.WriteAllTextAtomic(
            _layout.SessionPath,
            JsonSerializer.Serialize(batch.Session, PrettyJsonOptions));
    }

    private void AppendEvent(LiveExportEventEnvelope envelope)
    {
        var json = JsonSerializer.Serialize(envelope, CompactJsonOptions);
        File.AppendAllText(_layout.EventsPath, json + Environment.NewLine);
    }

    private static JsonSerializerOptions CompactJsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private static JsonSerializerOptions PrettyJsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
}
