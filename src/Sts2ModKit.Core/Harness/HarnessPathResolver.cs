using Sts2ModKit.Core.Configuration;

namespace Sts2ModKit.Core.Harness;

public sealed record HarnessQueueLayout(
    string ModdedProfileRoot,
    string HarnessRoot,
    string InboxRoot,
    string ActionsPath,
    string OutboxRoot,
    string ResultsPath,
    string StatusPath,
    string TracePath);

public static class HarnessPathResolver
{
    public static HarnessQueueLayout Resolve(GamePathOptions gamePaths, HarnessOptions options)
    {
        var moddedProfileRoot = LiveExport.LiveExportPathResolver.BuildModdedProfileRoot(gamePaths);
        var harnessRoot = CombineRelativeSegments(moddedProfileRoot, options.RelativeHarnessRoot);
        var inboxRoot = Path.Combine(harnessRoot, "inbox");
        var outboxRoot = Path.Combine(harnessRoot, "outbox");
        return new HarnessQueueLayout(
            moddedProfileRoot,
            harnessRoot,
            inboxRoot,
            Path.Combine(inboxRoot, options.ActionsFileName),
            outboxRoot,
            Path.Combine(outboxRoot, options.ResultsFileName),
            Path.Combine(harnessRoot, options.StatusFileName),
            Path.Combine(outboxRoot, "trace.ndjson"));
    }

    public static void EnsureDirectories(HarnessQueueLayout layout)
    {
        Directory.CreateDirectory(layout.HarnessRoot);
        Directory.CreateDirectory(layout.InboxRoot);
        Directory.CreateDirectory(layout.OutboxRoot);
    }

    private static string CombineRelativeSegments(string root, string relativePath)
    {
        var segments = relativePath
            .Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return segments.Aggregate(root, Path.Combine);
    }
}
