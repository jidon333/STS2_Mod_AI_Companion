using System.Text.Json;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModAiCompanion.HarnessBridge;

internal sealed class LiveSnapshotReader
{
    public LiveExportSnapshot? TryRead(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return JsonSerializer.Deserialize<LiveExportSnapshot>(stream, ConfigurationLoader.JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
