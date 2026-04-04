using System.Text.Json;
using Sts2AiCompanion.AdvisorSceneModel;

namespace Sts2AiCompanion.Host;

public sealed partial class CompanionHost
{
    private AdvisorSceneArtifact UpdateSceneArtifacts(CompanionRunState runState)
    {
        var artifact = CompanionLiveSceneModelBuilder.Build(runState) with
        {
            PublishedAtUtc = DateTimeOffset.UtcNow,
        };
        var paths = EnsureRunArtifacts(runState.Snapshot.RunId);
        if (!string.IsNullOrWhiteSpace(paths.AdvisorSceneLatestJsonPath))
        {
            WriteJson(paths.AdvisorSceneLatestJsonPath!, artifact);
        }

        if (!string.IsNullOrWhiteSpace(paths.AdvisorSceneLogPath))
        {
            var payload = JsonSerializer.Serialize(artifact, _ndjsonOptions);
            var semanticPayload = BuildAdvisorSceneSemanticPayload(artifact);
            if (!string.Equals(_lastLoggedSceneModelPayload, semanticPayload, StringComparison.Ordinal))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(paths.AdvisorSceneLogPath!)!);
                File.AppendAllText(paths.AdvisorSceneLogPath!, payload + Environment.NewLine);
                _lastLoggedSceneModelPayload = semanticPayload;
            }
        }

        return artifact;
    }

    private string BuildAdvisorSceneSemanticPayload(AdvisorSceneArtifact artifact)
    {
        return JsonSerializer.Serialize(
            artifact with
            {
                CapturedAtUtc = null,
                PublishedAtUtc = null,
            },
            _ndjsonOptions);
    }
}
