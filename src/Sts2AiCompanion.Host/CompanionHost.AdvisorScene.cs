using System.Text.Json;
using Sts2AiCompanion.AdvisorSceneModel;

namespace Sts2AiCompanion.Host;

public sealed partial class CompanionHost
{
    private AdvisorSceneArtifact UpdateSceneArtifacts(CompanionRunState runState)
    {
        var artifact = CompanionLiveSceneModelBuilder.Build(runState);
        var paths = EnsureRunArtifacts(runState.Snapshot.RunId);
        if (!string.IsNullOrWhiteSpace(paths.AdvisorSceneLatestJsonPath))
        {
            WriteJson(paths.AdvisorSceneLatestJsonPath!, artifact);
        }

        if (!string.IsNullOrWhiteSpace(paths.AdvisorSceneLogPath))
        {
            var payload = JsonSerializer.Serialize(artifact, _ndjsonOptions);
            if (!string.Equals(_lastLoggedSceneModelPayload, payload, StringComparison.Ordinal))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(paths.AdvisorSceneLogPath!)!);
                File.AppendAllText(paths.AdvisorSceneLogPath!, payload + Environment.NewLine);
                _lastLoggedSceneModelPayload = payload;
            }
        }

        return artifact;
    }
}
