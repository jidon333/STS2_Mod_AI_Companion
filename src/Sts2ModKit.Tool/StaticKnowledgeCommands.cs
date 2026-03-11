using System.Text.Json;
using System.Text.RegularExpressions;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModKit.Tool;

internal sealed record StaticKnowledgeExtractionResult(
    string KnowledgeRoot,
    string CatalogPath,
    string SummaryPath,
    string SourceManifestPath,
    string AssemblyScanPath,
    string PckInventoryPath,
    string ObservedMergePath,
    string MarkdownRoot,
    string MarkdownOverviewPath,
    object Counts,
    object SourceCounts,
    IReadOnlyList<string> Warnings);

internal sealed record StaticKnowledgeInspectionResult(
    string KnowledgeRoot,
    bool CatalogExists,
    bool SummaryExists,
    bool SourceManifestExists,
    bool AssemblyScanExists,
    bool PckInventoryExists,
    bool ObservedMergeExists,
    bool MarkdownExists,
    string MarkdownRoot,
    object? Metadata,
    object? Counts,
    object? SourceCounts,
    IReadOnlyList<StaticKnowledgePipelineStep> Steps,
    IReadOnlyList<string> SampleCards,
    IReadOnlyList<string> SampleRelics,
    IReadOnlyList<string> SampleEvents);

internal sealed record GameReleaseInfo(
    string? Commit,
    string? Version,
    DateTimeOffset? Date,
    string? Branch);

internal static class StaticKnowledgeCommands
{
    public static StaticKnowledgeExtractionResult Extract(ScaffoldConfiguration configuration, string knowledgeRoot)
    {
        return ExtractInternal(configuration, knowledgeRoot, "extract-static-knowledge");
    }

    public static StaticKnowledgeExtractionResult MergeObserved(ScaffoldConfiguration configuration, string knowledgeRoot)
    {
        return ExtractInternal(configuration, knowledgeRoot, "merge-observed-knowledge");
    }

    public static StaticKnowledgeInspectionResult Inspect(string knowledgeRoot)
    {
        var catalogPath = Path.Combine(knowledgeRoot, "catalog.latest.json");
        var summaryPath = Path.Combine(knowledgeRoot, "catalog.latest.txt");
        var sourceManifestPath = Path.Combine(knowledgeRoot, "source-manifest.json");
        var assemblyScanPath = Path.Combine(knowledgeRoot, "assembly-scan.json");
        var pckInventoryPath = Path.Combine(knowledgeRoot, "pck-inventory.json");
        var observedMergePath = Path.Combine(knowledgeRoot, "observed-merge.json");
        var markdownRoot = Path.Combine(knowledgeRoot, "markdown");
        var catalog = TryReadJson<StaticKnowledgeCatalog>(catalogPath);
        var sourceManifest = TryReadJson<StaticKnowledgeSourceManifest>(sourceManifestPath);

        return new StaticKnowledgeInspectionResult(
            knowledgeRoot,
            File.Exists(catalogPath),
            File.Exists(summaryPath),
            File.Exists(sourceManifestPath),
            File.Exists(assemblyScanPath),
            File.Exists(pckInventoryPath),
            File.Exists(observedMergePath),
            Directory.Exists(markdownRoot),
            markdownRoot,
            catalog?.Metadata ?? sourceManifest?.Metadata,
            catalog is null ? null : BuildCounts(catalog),
            catalog is null ? null : BuildSourceCounts(catalog),
            sourceManifest?.Steps ?? Array.Empty<StaticKnowledgePipelineStep>(),
            catalog?.Cards.Take(8).Select(entry => entry.Name).ToArray() ?? Array.Empty<string>(),
            catalog?.Relics.Take(8).Select(entry => entry.Name).ToArray() ?? Array.Empty<string>(),
            catalog?.Events.Take(8).Select(entry => entry.Name).ToArray() ?? Array.Empty<string>());
    }

    private static StaticKnowledgeExtractionResult ExtractInternal(ScaffoldConfiguration configuration, string knowledgeRoot, string source)
    {
        Directory.CreateDirectory(knowledgeRoot);

        var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        var catalogPath = Path.Combine(knowledgeRoot, "catalog.latest.json");
        var summaryPath = Path.Combine(knowledgeRoot, "catalog.latest.txt");
        var sourceManifestPath = Path.Combine(knowledgeRoot, "source-manifest.json");
        var assemblyScanPath = Path.Combine(knowledgeRoot, "assembly-scan.json");
        var pckInventoryPath = Path.Combine(knowledgeRoot, "pck-inventory.json");
        var observedMergePath = Path.Combine(knowledgeRoot, "observed-merge.json");
        var markdownRoot = Path.Combine(knowledgeRoot, "markdown");
        var markdownOverviewPath = Path.Combine(markdownRoot, "README.md");
        var warnings = new List<string>();
        var steps = new List<StaticKnowledgePipelineStep>();

        var baseline = TryReadJson<StaticKnowledgeCatalog>(catalogPath);
        var snapshot = TryReadJson<LiveExportSnapshot>(layout.SnapshotPath);
        var events = ReadNdjson<LiveExportEventEnvelope>(layout.EventsPath, warnings);
        if (snapshot is null && events.Count == 0)
        {
            warnings.Add("No live export snapshot or events were available. The knowledge catalog may contain only previously merged observations and source metadata.");
        }

        var releaseInfoPath = Path.Combine(configuration.GamePaths.GameDirectory, "release_info.json");
        var releaseInfo = TryReadJson<GameReleaseInfo>(releaseInfoPath);
        var atlasStats = ReadAtlasStats(Path.Combine(configuration.GamePaths.UserDataRoot, "logs", "godot.log"));
        atlasStats["source"] = source;
        atlasStats["eventsLoaded"] = events.Count.ToString();
        atlasStats["snapshotLoaded"] = (snapshot is not null).ToString();

        var metadata = new StaticKnowledgeMetadata(
            releaseInfo?.Version,
            releaseInfo?.Commit,
            releaseInfo?.Date,
            atlasStats);
        steps.Add(new StaticKnowledgePipelineStep(
            "release-scan",
            File.Exists(releaseInfoPath) ? "completed" : "warning",
            releaseInfoPath,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["releaseVersion"] = releaseInfo?.Version,
                ["releaseCommit"] = releaseInfo?.Commit,
                ["eventsLoaded"] = events.Count.ToString(),
                ["snapshotLoaded"] = (snapshot is not null).ToString(),
            },
            Array.Empty<string>()));

        var managedRoot = Path.Combine(configuration.GamePaths.GameDirectory, "data_sts2_windows_x86_64");
        var assemblyPath = Path.Combine(managedRoot, "sts2.dll");
        var assemblyCatalog = AssemblyKnowledgeScanner.Scan(assemblyPath, managedRoot, metadata, out var assemblyWarnings);
        warnings.AddRange(assemblyWarnings);
        WriteJson(assemblyScanPath, assemblyCatalog);
        steps.Add(new StaticKnowledgePipelineStep(
            "assembly-scan",
            assemblyWarnings.Count == 0 ? "completed" : "warning",
            assemblyScanPath,
            ToStringMap(BuildCounts(assemblyCatalog)),
            assemblyWarnings));

        var pckPath = Path.Combine(configuration.GamePaths.GameDirectory, "SlayTheSpire2.pck");
        var pckCatalog = PckInventoryScanner.Scan(pckPath, metadata, out var pckWarnings);
        if (!string.IsNullOrWhiteSpace(configuration.Assistant.OptionalGodotExe))
        {
            pckWarnings = pckWarnings
                .Concat(new[] { $"Configured optional Godot executable: {configuration.Assistant.OptionalGodotExe}" })
                .ToArray();
        }

        warnings.AddRange(pckWarnings);
        WriteJson(pckInventoryPath, pckCatalog);
        steps.Add(new StaticKnowledgePipelineStep(
            "pck-inventory",
            pckWarnings.Count == 0 ? "completed" : "warning",
            pckInventoryPath,
            ToStringMap(BuildCounts(pckCatalog)),
            pckWarnings));

        var observedCatalog = StaticKnowledgeCatalogBuilder.BuildFromObserved(null, snapshot, events, metadata);
        WriteJson(observedMergePath, observedCatalog);
        steps.Add(new StaticKnowledgePipelineStep(
            "observed-merge",
            (snapshot is null && events.Count == 0) ? "warning" : "completed",
            observedMergePath,
            ToStringMap(BuildCounts(observedCatalog)),
            snapshot is null && events.Count == 0
                ? new[] { "No live snapshot or events were available for observed-merge." }
                : Array.Empty<string>()));

        var seedCatalog = StaticKnowledgeCatalogBuilder.MergeCatalogs(metadata, baseline, assemblyCatalog, pckCatalog);
        var catalog = StaticKnowledgeCatalogBuilder.BuildFromObserved(seedCatalog, snapshot, events, metadata);
        steps.Add(new StaticKnowledgePipelineStep(
            "catalog-build",
            "completed",
            catalogPath,
            ToStringMap(BuildCounts(catalog)),
            Array.Empty<string>()));

        var sourceManifest = new StaticKnowledgeSourceManifest(
            DateTimeOffset.UtcNow,
            knowledgeRoot,
            metadata,
            BuildSourceFiles(configuration, layout, catalogPath, assemblyScanPath, pckInventoryPath, observedMergePath),
            steps,
            warnings);

        WriteJson(catalogPath, catalog);
        File.WriteAllText(summaryPath, StaticKnowledgeSummaryFormatter.Format(catalog));
        WriteJson(sourceManifestPath, sourceManifest);
        StaticKnowledgeMarkdownReportFormatter.WriteReports(markdownRoot, catalog, sourceManifest);

        return new StaticKnowledgeExtractionResult(
            knowledgeRoot,
            catalogPath,
            summaryPath,
            sourceManifestPath,
            assemblyScanPath,
            pckInventoryPath,
            observedMergePath,
            markdownRoot,
            markdownOverviewPath,
            BuildCounts(catalog),
            BuildSourceCounts(catalog),
            warnings);
    }

    private static IReadOnlyList<StaticKnowledgeSourceFile> BuildSourceFiles(
        ScaffoldConfiguration configuration,
        LiveExportLayout layout,
        string catalogPath,
        string assemblyScanPath,
        string pckInventoryPath,
        string observedMergePath)
    {
        var gameRoot = configuration.GamePaths.GameDirectory;
        var userDataRoot = configuration.GamePaths.UserDataRoot;
        return new[]
        {
            DescribeSourceFile("release-info", Path.Combine(gameRoot, "release_info.json")),
            DescribeSourceFile("game-assembly", Path.Combine(gameRoot, "data_sts2_windows_x86_64", "sts2.dll")),
            DescribeSourceFile("game-pck", Path.Combine(gameRoot, "SlayTheSpire2.pck")),
            DescribeSourceFile("godot-log", Path.Combine(userDataRoot, "logs", "godot.log")),
            DescribeSourceFile("live-events", layout.EventsPath),
            DescribeSourceFile("live-snapshot", layout.SnapshotPath),
            DescribeSourceFile("live-summary", layout.SummaryPath),
            DescribeSourceFile("live-session", layout.SessionPath),
            DescribeSourceFile("catalog-previous", catalogPath),
            DescribeSourceFile("assembly-scan", assemblyScanPath),
            DescribeSourceFile("pck-inventory", pckInventoryPath),
            DescribeSourceFile("observed-merge", observedMergePath),
        };
    }

    private static StaticKnowledgeSourceFile DescribeSourceFile(string kind, string path)
    {
        if (!File.Exists(path))
        {
            return new StaticKnowledgeSourceFile(kind, path, false, null, null);
        }

        var info = new FileInfo(path);
        return new StaticKnowledgeSourceFile(kind, path, true, info.Length, info.LastWriteTimeUtc);
    }

    private static object BuildCounts(StaticKnowledgeCatalog catalog)
    {
        return new
        {
            cards = catalog.Cards.Count,
            relics = catalog.Relics.Count,
            potions = catalog.Potions.Count,
            events = catalog.Events.Count,
            shops = catalog.Shops.Count,
            rewards = catalog.Rewards.Count,
            keywords = catalog.Keywords.Count,
        };
    }

    private static object BuildSourceCounts(StaticKnowledgeCatalog catalog)
    {
        var groups = catalog.Cards
            .Concat(catalog.Relics)
            .Concat(catalog.Potions)
            .Concat(catalog.Events)
            .Concat(catalog.Shops)
            .Concat(catalog.Rewards)
            .Concat(catalog.Keywords)
            .GroupBy(entry => entry.Source, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        return groups;
    }

    private static IReadOnlyDictionary<string, string?> ToStringMap(object counts)
    {
        return counts.GetType()
            .GetProperties()
            .ToDictionary(property => property.Name, property => property.GetValue(counts)?.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, string?> ReadAtlasStats(string godotLogPath)
    {
        var stats = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(godotLogPath))
        {
            return stats;
        }

        foreach (var line in ReadAllLinesShared(godotLogPath))
        {
            var match = Regex.Match(line, @"Loaded\s+([a-zA-Z0-9_]+)\s+with\s+(\d+)\s+sprites", RegexOptions.CultureInvariant);
            if (!match.Success)
            {
                continue;
            }

            stats[$"atlas:{match.Groups[1].Value}"] = match.Groups[2].Value;
        }

        return stats;
    }

    private static IReadOnlyList<T> ReadNdjson<T>(string path, ICollection<string> warnings)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<T>();
        }

        var results = new List<T>();
        var lineNumber = 0;
        foreach (var line in ReadAllLinesShared(path))
        {
            lineNumber += 1;
            try
            {
                var parsed = JsonSerializer.Deserialize<T>(line, ConfigurationLoader.JsonOptions);
                if (parsed is not null)
                {
                    results.Add(parsed);
                }
            }
            catch (JsonException exception)
            {
                warnings.Add($"Ignored malformed NDJSON line {lineNumber} in {Path.GetFileName(path)}: {exception.Message}");
            }
        }

        return results;
    }

    private static T? TryReadJson<T>(string path)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(ReadAllTextShared(path), ConfigurationLoader.JsonOptions);
    }

    private static void WriteJson<T>(string path, T value)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        }));
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
