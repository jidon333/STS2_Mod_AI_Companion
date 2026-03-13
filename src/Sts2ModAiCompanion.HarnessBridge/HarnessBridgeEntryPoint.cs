using Sts2ModKit.Core.Harness;

namespace Sts2ModAiCompanion.HarnessBridge;

public static class HarnessBridgeEntryPoint
{
    private static readonly object Sync = new();
    private static HarnessBridgeHost? _host;

    public static string BridgeVersion => "0.3.0-observer";

    public static bool Initialize(HarnessQueueLayout layout, string liveSnapshotPath, int pollIntervalMs)
    {
        lock (Sync)
        {
            if (_host is not null)
            {
                return true;
            }

            HarnessPathResolver.EnsureDirectories(layout);
            _host = new HarnessBridgeHost(layout, liveSnapshotPath, pollIntervalMs);
            _host.Start();
            return true;
        }
    }
}
