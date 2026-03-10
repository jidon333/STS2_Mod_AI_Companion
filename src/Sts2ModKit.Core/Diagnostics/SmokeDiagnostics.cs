namespace Sts2ModKit.Core.Diagnostics;

public sealed record SmokeDiagnosticFinding(
    string Code,
    string Severity,
    string Summary,
    string? Detail = null);

public sealed record SmokeLogAnalysis(
    IReadOnlyList<string> InterestingLines,
    IReadOnlyList<string> ErrorLines,
    IReadOnlyList<string> StartupHighlights,
    IReadOnlyList<string> FailureHighlights,
    IReadOnlyList<SmokeDiagnosticFinding> Findings,
    IReadOnlyList<string> Tail);

public static class SmokeDiagnostics
{
    public static SmokeLogAnalysis AnalyzeGodotLog(IReadOnlyList<string> lines, int tailCount)
    {
        var safeTailCount = Math.Max(1, tailCount);
        var tail = lines.TakeLast(safeTailCount).ToArray();
        var startupWindow = lines.Take(Math.Min(lines.Count, 240)).ToArray();

        var interestingLines = tail
            .Where(line =>
                ContainsAny(
                    line,
                    "sts2-mod-ai-companion",
                    "ai companion",
                    "Harmony.PatchAll",
                    "mod initialization",
                    "runtime exporter",
                    "mod_manifest",
                    "mods"))
            .ToArray();

        var errorLines = tail
            .Where(line =>
                ContainsAny(
                    line,
                    "error",
                    "exception",
                    "failed",
                    "cannot get result from void method"))
            .ToArray();

        var startupHighlights = startupWindow
            .Where(line =>
                ContainsAny(
                    line,
                    "sts2-mod-ai-companion",
                    "Harmony.PatchAll",
                    "PatchAll",
                    "mod initialization",
                    "runtime exporter",
                    "Cannot get result from void method",
                    "HarmonyException"))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var failureHighlights = lines
            .Where(line =>
                ContainsAny(
                    line,
                    "Cannot get result from void method",
                    "HarmonyException",
                    "Patching exception",
                    "Exception caught while trying to run PatchAll",
                    "runtime exporter hook failure",
                    "runtime exporter worker failure",
                    "runtime exporter poll failure"))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var findings = new List<SmokeDiagnosticFinding>();
        if (failureHighlights.Any(line =>
                ContainsAny(
                    line,
                    "Cannot get result from void method",
                    "HarmonyException",
                    "Patching exception",
                    "Exception caught while trying to run PatchAll")))
        {
            findings.Add(new SmokeDiagnosticFinding(
                "HarmonyPatchAllStartupFailure",
                "error",
                "Harmony PatchAll failed during startup.",
                string.Join(Environment.NewLine, failureHighlights.Take(6))));
        }

        return new SmokeLogAnalysis(
            interestingLines,
            errorLines,
            startupHighlights,
            failureHighlights,
            findings,
            tail);
    }

    public static IReadOnlyList<SmokeDiagnosticFinding> AnalyzeLiveExport(
        bool runtimeConfigExists,
        bool liveRootExists,
        bool runtimeLogExists,
        bool sessionExists,
        bool snapshotExists,
        bool summaryExists,
        bool eventsExist,
        IReadOnlyCollection<string> deployedModFiles,
        string liveRoot)
    {
        var findings = new List<SmokeDiagnosticFinding>();
        var hasDeployedPayload = runtimeConfigExists && deployedModFiles.Count > 0;

        if (hasDeployedPayload && !liveRootExists)
        {
            findings.Add(new SmokeDiagnosticFinding(
                "DeployedVsLiveRootMismatch",
                "error",
                "AI companion payload is deployed, but the live export root was not created.",
                $"live_root={liveRoot}; deployed_files={string.Join(", ", deployedModFiles.OrderBy(name => name, StringComparer.OrdinalIgnoreCase))}"));
        }

        if (hasDeployedPayload && !runtimeLogExists)
        {
            findings.Add(new SmokeDiagnosticFinding(
                "RuntimeLogMissing",
                liveRootExists ? "warning" : "error",
                "The AI companion runtime log was not produced for this run."));
        }

        if (liveRootExists && !(sessionExists || snapshotExists || summaryExists || eventsExist))
        {
            findings.Add(new SmokeDiagnosticFinding(
                "LiveArtifactsMissing",
                "error",
                "The live export root exists, but no exporter artifacts were written."));
        }

        return findings;
    }

    private static bool ContainsAny(string line, params string[] patterns)
    {
        return patterns.Any(pattern => line.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}
