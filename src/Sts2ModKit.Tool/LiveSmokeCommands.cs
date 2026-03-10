using System.Text.Json;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Diagnostics;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModKit.Tool;

internal sealed record LiveSmokePreparationResult(
    string ModsRoot,
    string RuntimeLogPath,
    string LiveRoot,
    bool RuntimeLogDeleted,
    bool LiveRootDeleted,
    IReadOnlyList<string> RemovedModFiles,
    IReadOnlyList<string> PreservedModFiles,
    LiveExportLayout Layout);

internal sealed record LiveExportInspectionResult(
    string ModsRoot,
    LiveExportLayout Layout,
    string RuntimeConfigPath,
    bool RuntimeConfigExists,
    bool LiveRootExists,
    bool RuntimeLogExists,
    string RuntimeLogPath,
    IReadOnlyList<string> RuntimeLogTail,
    object? Session,
    object? Snapshot,
    string? Summary,
    IReadOnlyList<string> EventTail,
    IReadOnlyList<SmokeDiagnosticFinding> Findings,
    IReadOnlyList<object> LiveFiles,
    IReadOnlyList<object> ModFiles);

internal sealed record GameLogInspectionResult(
    string GodotLogPath,
    bool Exists,
    IReadOnlyList<string> InterestingLines,
    IReadOnlyList<string> ErrorLines,
    IReadOnlyList<string> StartupHighlights,
    IReadOnlyList<string> FailureHighlights,
    IReadOnlyList<SmokeDiagnosticFinding> Findings,
    IReadOnlyList<string> Tail);

internal static class LiveSmokeCommands
{
    public static LiveSmokePreparationResult Prepare(ScaffoldConfiguration configuration)
    {
        var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        var modsRoot = Path.Combine(configuration.GamePaths.GameDirectory, "mods");
        var runtimeLogPath = Path.Combine(modsRoot, configuration.AiCompanionMod.RuntimeLogFileName);
        var runtimeLogDeleted = false;
        var liveRootDeleted = false;
        var removedModFiles = new List<string>();

        if (Directory.Exists(modsRoot))
        {
            foreach (var file in Directory.GetFiles(modsRoot, "*", SearchOption.TopDirectoryOnly))
            {
                var fileName = Path.GetFileName(file);
                if (ShouldPreserveForAiCompanionSmoke(fileName, configuration))
                {
                    continue;
                }

                File.Delete(file);
                removedModFiles.Add(fileName);

                if (string.Equals(file, runtimeLogPath, StringComparison.OrdinalIgnoreCase))
                {
                    runtimeLogDeleted = true;
                }
            }
        }

        if (Directory.Exists(layout.LiveRoot))
        {
            Directory.Delete(layout.LiveRoot, recursive: true);
            liveRootDeleted = true;
        }

        var preservedModFiles = Directory.Exists(modsRoot)
            ? Directory.GetFiles(modsRoot, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .Cast<string>()
                .ToArray()
            : Array.Empty<string>();

        return new LiveSmokePreparationResult(
            modsRoot,
            runtimeLogPath,
            layout.LiveRoot,
            runtimeLogDeleted,
            liveRootDeleted,
            removedModFiles.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray(),
            preservedModFiles,
            layout);
    }

    public static LiveExportInspectionResult Inspect(ScaffoldConfiguration configuration, int tailLines)
    {
        var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        var modsRoot = Path.Combine(configuration.GamePaths.GameDirectory, "mods");
        var runtimeLogPath = Path.Combine(modsRoot, configuration.AiCompanionMod.RuntimeLogFileName);
        var runtimeConfigPath = Path.Combine(modsRoot, configuration.AiCompanionMod.RuntimeConfigFileName);
        var liveRootExists = Directory.Exists(layout.LiveRoot);
        var runtimeLogExists = File.Exists(runtimeLogPath);
        var sessionExists = File.Exists(layout.SessionPath);
        var snapshotExists = File.Exists(layout.SnapshotPath);
        var summaryExists = File.Exists(layout.SummaryPath);
        var eventsExist = File.Exists(layout.EventsPath);
        var modFiles = Directory.Exists(modsRoot)
            ? Directory.GetFiles(modsRoot)
                .Where(path =>
                    Path.GetFileName(path).Contains("sts2-mod-ai-companion", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(Path.GetFileName(path), "Sts2ModKit.Core.dll", StringComparison.OrdinalIgnoreCase))
                .Select(path => new
                {
                    Name = Path.GetFileName(path),
                    Size = new FileInfo(path).Length,
                    LastWriteTime = File.GetLastWriteTime(path),
                })
                .Cast<object>()
                .ToArray()
            : Array.Empty<object>();
        var deployedNames = modFiles
            .Select(file => file.GetType().GetProperty("Name")?.GetValue(file)?.ToString())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToArray();
        var findings = SmokeDiagnostics.AnalyzeLiveExport(
            File.Exists(runtimeConfigPath),
            liveRootExists,
            runtimeLogExists,
            sessionExists,
            snapshotExists,
            summaryExists,
            eventsExist,
            deployedNames,
            layout.LiveRoot);

        return new LiveExportInspectionResult(
            modsRoot,
            layout,
            runtimeConfigPath,
            File.Exists(runtimeConfigPath),
            liveRootExists,
            runtimeLogExists,
            runtimeLogPath,
            runtimeLogExists ? ReadTailShared(runtimeLogPath, tailLines) : Array.Empty<string>(),
            TryReadJson(layout.SessionPath),
            TryReadJson(layout.SnapshotPath),
            summaryExists ? ReadAllTextShared(layout.SummaryPath) : null,
            eventsExist ? ReadTailShared(layout.EventsPath, tailLines) : Array.Empty<string>(),
            findings,
            liveRootExists
                ? Directory.GetFiles(layout.LiveRoot)
                    .Select(path => new
                    {
                        Name = Path.GetFileName(path),
                        Size = new FileInfo(path).Length,
                        LastWriteTime = File.GetLastWriteTime(path),
                    })
                    .Cast<object>()
                    .ToArray()
                : Array.Empty<object>(),
            modFiles);
    }

    public static GameLogInspectionResult InspectGodotLog(ScaffoldConfiguration configuration, int lineCount)
    {
        var logsRoot = Path.Combine(configuration.GamePaths.UserDataRoot, "logs");
        var godotLogPath = Path.Combine(logsRoot, "godot.log");
        if (!File.Exists(godotLogPath))
        {
            return new GameLogInspectionResult(
                godotLogPath,
                false,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<SmokeDiagnosticFinding>(),
                Array.Empty<string>());
        }

        var lines = ReadAllLinesShared(godotLogPath);
        var analysis = SmokeDiagnostics.AnalyzeGodotLog(lines, lineCount);

        return new GameLogInspectionResult(
            godotLogPath,
            true,
            analysis.InterestingLines,
            analysis.ErrorLines,
            analysis.StartupHighlights,
            analysis.FailureHighlights,
            analysis.Findings,
            analysis.Tail);
    }

    private static IReadOnlyList<string> ReadTailShared(string path, int lineCount)
    {
        return ReadAllLinesShared(path)
            .TakeLast(Math.Max(1, lineCount))
            .ToArray();
    }

    private static bool ShouldPreserveForAiCompanionSmoke(string fileName, ScaffoldConfiguration configuration)
    {
        var aiDllName = Path.ChangeExtension(configuration.AiCompanionMod.PckName, ".dll");
        return string.Equals(fileName, configuration.AiCompanionMod.RuntimeConfigFileName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, aiDllName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, configuration.AiCompanionMod.PckName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "Sts2ModKit.Core.dll", StringComparison.OrdinalIgnoreCase);
    }

    private static object? TryReadJson(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        using var document = JsonDocument.Parse(ReadAllTextShared(path));
        return JsonSerializer.Deserialize<object>(document.RootElement.GetRawText(), ConfigurationLoader.JsonOptions);
    }

    private static string ReadAllTextShared(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static IReadOnlyList<string> ReadAllLinesShared(string path)
    {
        return ReadAllTextShared(path)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .ToArray();
    }
}
