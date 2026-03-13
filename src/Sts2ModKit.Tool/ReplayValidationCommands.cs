using Sts2AiCompanion.Foundation.Replay;
using Sts2ModKit.Core.Configuration;

namespace Sts2ModKit.Tool;

internal static class ReplayValidationCommands
{
    public static async Task<ReplayValidationResult> ValidateAdvisorReplayAsync(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        string fixtureRoot,
        string? mockResponsePath,
        CancellationToken cancellationToken)
    {
        var validator = new ReplayAdvisorValidator(configuration, workspaceRoot);
        return await validator.ValidateAsync(fixtureRoot, mockResponsePath, cancellationToken).ConfigureAwait(false);
    }

    public static ReplayFixtureBuildResult BuildReplayFixture(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        string sourceRunId)
    {
        var sourcePaths = Sts2AiCompanion.Foundation.Artifacts.CompanionPathResolver.Resolve(configuration, workspaceRoot, sourceRunId);
        if (sourcePaths.LiveMirrorRoot is null || !Directory.Exists(sourcePaths.LiveMirrorRoot))
        {
            throw new DirectoryNotFoundException($"Replay fixture source was not found: {sourcePaths.LiveMirrorRoot}");
        }

        var sanitizedRunId = new string(sourceRunId.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '-' : character).ToArray());
        var fixtureRoot = Path.Combine(workspaceRoot, "tests", "replay-fixtures", sanitizedRunId);
        if (Directory.Exists(fixtureRoot))
        {
            Directory.Delete(fixtureRoot, recursive: true);
        }

        Directory.CreateDirectory(fixtureRoot);
        CopyDirectoryContents(sourcePaths.LiveMirrorRoot, Path.Combine(fixtureRoot, "live-mirror"));
        File.WriteAllText(
            Path.Combine(fixtureRoot, "fixture.json"),
            System.Text.Json.JsonSerializer.Serialize(
                new
                {
                    sourceRunId,
                    sourceLiveMirrorRoot = sourcePaths.LiveMirrorRoot,
                    builtAt = DateTimeOffset.UtcNow,
                },
                ProgramJson.SerializerOptions));

        return new ReplayFixtureBuildResult(sourceRunId, fixtureRoot);
    }

    private static void CopyDirectoryContents(string sourceDirectory, string destinationDirectory)
    {
        foreach (var directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(destinationDirectory, relative));
        }

        foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, file);
            var destinationPath = Path.Combine(destinationDirectory, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(file, destinationPath, overwrite: true);
        }
    }
}

internal sealed record ReplayFixtureBuildResult(string SourceRunId, string FixtureRoot);
