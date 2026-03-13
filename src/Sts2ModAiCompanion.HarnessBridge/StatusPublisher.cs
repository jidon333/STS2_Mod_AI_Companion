using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModAiCompanion.HarnessBridge;

internal sealed class StatusPublisher
{
    private readonly string _statusPath;

    public StatusPublisher(string statusPath)
    {
        _statusPath = statusPath;
    }

    public void Write(HarnessBridgeStatus status)
    {
        LiveExportAtomicFileWriter.WriteJsonAtomic(_statusPath, status);
    }
}
