using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Configuration;

namespace Sts2AiCompanion.Foundation.Artifacts;

public static class CompanionPathResolver
{
    public static string ResolveArtifactsRoot(ScaffoldConfiguration configuration, string workspaceRoot)
    {
        return Path.GetFullPath(configuration.GamePaths.ArtifactsRoot, workspaceRoot);
    }

    public static string ResolveKnowledgeRoot(ScaffoldConfiguration configuration, string workspaceRoot)
    {
        return Path.Combine(ResolveArtifactsRoot(configuration, workspaceRoot), "knowledge");
    }

    public static CompanionArtifactPaths Resolve(ScaffoldConfiguration configuration, string workspaceRoot, string? runId)
    {
        var companionRoot = Path.Combine(
            ResolveArtifactsRoot(configuration, workspaceRoot),
            configuration.Assistant.CompanionArtifactsRelativeRoot);

        var safeRunId = string.IsNullOrWhiteSpace(runId) ? null : SanitizeSegment(runId!);
        var runRoot = safeRunId is null ? null : Path.Combine(companionRoot, safeRunId);
        var liveMirrorRoot = runRoot is null ? null : Path.Combine(runRoot, "live-mirror");
        var promptPacksRoot = runRoot is null ? null : Path.Combine(runRoot, "prompt-packs");
        var adviceRoot = runRoot is null ? null : Path.Combine(runRoot, "advice");

        return new CompanionArtifactPaths(
            companionRoot,
            Path.Combine(companionRoot, "current-run.json"),
            runRoot,
            liveMirrorRoot,
            promptPacksRoot,
            adviceRoot,
            adviceRoot is null ? null : Path.Combine(adviceRoot, "advice.ndjson"),
            adviceRoot is null ? null : Path.Combine(adviceRoot, "advice.latest.json"),
            adviceRoot is null ? null : Path.Combine(adviceRoot, "advice.latest.md"),
            runRoot is null ? null : Path.Combine(runRoot, "codex-session.json"),
            runRoot is null ? null : Path.Combine(runRoot, "codex-trace.ndjson"),
            runRoot is null ? null : Path.Combine(runRoot, "collector-summary.json"),
            runRoot is null ? Path.Combine(companionRoot, "host-status.json") : Path.Combine(runRoot, "host-status.json"));
    }

    private static string SanitizeSegment(string value)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var normalized = new string(value.Select(character => invalid.Contains(character) ? '-' : character).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? "unknown-run" : normalized;
    }
}
