using System.Text.Json;
using System.Text.RegularExpressions;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModKit.Tool;

internal sealed record StaticKnowledgeExtractionResult(
    string KnowledgeRoot,
    string CatalogPath,
    string AssistantCatalogPath,
    string SummaryPath,
    string AssistantSummaryPath,
    string AssistantRoot,
    string SourceManifestPath,
    string DecompileScanPath,
    string StrictDomainScanPath,
    string AssemblyScanPath,
    string PckInventoryPath,
    string LocalizationScanPath,
    string ObservedMergePath,
    string MarkdownRoot,
    string MarkdownOverviewPath,
    object Counts,
    object SourceCounts,
    IReadOnlyList<string> Warnings);

internal sealed record StaticKnowledgeInspectionResult(
    string KnowledgeRoot,
    bool CatalogExists,
    bool AssistantCatalogExists,
    bool SummaryExists,
    bool AssistantSummaryExists,
    bool AssistantRootExists,
    bool SourceManifestExists,
    bool DecompileScanExists,
    bool StrictDomainScanExists,
    bool AssemblyScanExists,
    bool PckInventoryExists,
    bool LocalizationScanExists,
    bool ObservedMergeExists,
    bool MarkdownExists,
    string AssistantRoot,
    string MarkdownRoot,
    string DecompiledRoot,
    object? Metadata,
    object? Counts,
    object? SourceCounts,
    object? LocalizationStats,
    IReadOnlyList<StaticKnowledgePipelineStep> Steps,
    IReadOnlyList<string> SampleCards,
    IReadOnlyList<string> SampleRelics,
    IReadOnlyList<string> SampleEvents);

internal sealed record GameReleaseInfo(
    string? Commit,
    string? Version,
    DateTimeOffset? Date,
    string? Branch);

internal sealed record ObservedKnowledgeInput(
    string SourceId,
    LiveExportSnapshot? Snapshot,
    IReadOnlyList<LiveExportEventEnvelope> Events);

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
        var assistantCatalogPath = Path.Combine(knowledgeRoot, "catalog.assistant.json");
        var summaryPath = Path.Combine(knowledgeRoot, "catalog.latest.txt");
        var assistantSummaryPath = Path.Combine(knowledgeRoot, "catalog.assistant.txt");
        var assistantRoot = Path.Combine(knowledgeRoot, "assistant");
        var sourceManifestPath = Path.Combine(knowledgeRoot, "source-manifest.json");
        var decompileScanPath = Path.Combine(knowledgeRoot, "decompile-scan.json");
        var strictDomainScanPath = Path.Combine(knowledgeRoot, "strict-domain-scan.json");
        var assemblyScanPath = Path.Combine(knowledgeRoot, "assembly-scan.json");
        var pckInventoryPath = Path.Combine(knowledgeRoot, "pck-inventory.json");
        var localizationScanPath = Path.Combine(knowledgeRoot, "localization-scan.json");
        var observedMergePath = Path.Combine(knowledgeRoot, "observed-merge.json");
        var markdownRoot = Path.Combine(knowledgeRoot, "markdown");
        var catalog = TryReadJson<StaticKnowledgeCatalog>(catalogPath);
        var sourceManifest = TryReadJson<StaticKnowledgeSourceManifest>(sourceManifestPath);
        var localizationScan = TryReadJson<StaticKnowledgeLocalizationScan>(localizationScanPath);
        var decompileScan = TryReadJson<StaticKnowledgeDecompileScan>(decompileScanPath);

        return new StaticKnowledgeInspectionResult(
            knowledgeRoot,
            File.Exists(catalogPath),
            File.Exists(assistantCatalogPath),
            File.Exists(summaryPath),
            File.Exists(assistantSummaryPath),
            Directory.Exists(assistantRoot),
            File.Exists(sourceManifestPath),
            File.Exists(decompileScanPath),
            File.Exists(strictDomainScanPath),
            File.Exists(assemblyScanPath),
            File.Exists(pckInventoryPath),
            File.Exists(localizationScanPath),
            File.Exists(observedMergePath),
            Directory.Exists(markdownRoot),
            assistantRoot,
            markdownRoot,
            decompileScan?.DecompiledRoot ?? Path.Combine(knowledgeRoot, "decompiled"),
            catalog?.Metadata ?? sourceManifest?.Metadata,
            catalog is null ? null : BuildCounts(catalog),
            catalog is null ? null : BuildSourceCounts(catalog),
            localizationScan is null ? null : BuildLocalizationCounts(localizationScan),
            sourceManifest?.Steps ?? Array.Empty<StaticKnowledgePipelineStep>(),
            catalog is null ? Array.Empty<string>() : BuildSampleEntries(catalog.Cards),
            catalog is null ? Array.Empty<string>() : BuildSampleEntries(catalog.Relics),
            catalog is null ? Array.Empty<string>() : BuildSampleEntries(catalog.Events));
    }

    private static StaticKnowledgeExtractionResult ExtractInternal(ScaffoldConfiguration configuration, string knowledgeRoot, string source)
    {
        Directory.CreateDirectory(knowledgeRoot);

        var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        var catalogPath = Path.Combine(knowledgeRoot, "catalog.latest.json");
        var assistantCatalogPath = Path.Combine(knowledgeRoot, "catalog.assistant.json");
        var summaryPath = Path.Combine(knowledgeRoot, "catalog.latest.txt");
        var assistantSummaryPath = Path.Combine(knowledgeRoot, "catalog.assistant.txt");
        var assistantRoot = Path.Combine(knowledgeRoot, "assistant");
        var sourceManifestPath = Path.Combine(knowledgeRoot, "source-manifest.json");
        var decompileScanPath = Path.Combine(knowledgeRoot, "decompile-scan.json");
        var strictDomainScanPath = Path.Combine(knowledgeRoot, "strict-domain-scan.json");
        var decompiledRoot = Path.Combine(knowledgeRoot, "decompiled");
        var assemblyScanPath = Path.Combine(knowledgeRoot, "assembly-scan.json");
        var pckInventoryPath = Path.Combine(knowledgeRoot, "pck-inventory.json");
        var localizationScanPath = Path.Combine(knowledgeRoot, "localization-scan.json");
        var observedMergePath = Path.Combine(knowledgeRoot, "observed-merge.json");
        var markdownRoot = Path.Combine(knowledgeRoot, "markdown");
        var markdownOverviewPath = Path.Combine(markdownRoot, "README.md");
        var warnings = new List<string>();
        var steps = new List<StaticKnowledgePipelineStep>();

        var observedInputs = ReadObservedInputs(configuration, knowledgeRoot, layout, warnings);
        if (observedInputs.All(input => input.Snapshot is null && input.Events.Count == 0))
        {
            warnings.Add("No live export snapshot or events were available. The knowledge catalog may contain only source-derived canonical entries.");
        }

        var releaseInfoPath = Path.Combine(configuration.GamePaths.GameDirectory, "release_info.json");
        var releaseInfo = TryReadJson<GameReleaseInfo>(releaseInfoPath);
        var atlasStats = ReadAtlasStats(Path.Combine(configuration.GamePaths.UserDataRoot, "logs", "godot.log"));
        atlasStats["source"] = source;
        atlasStats["eventsLoaded"] = observedInputs.Sum(input => input.Events.Count).ToString();
        atlasStats["snapshotLoaded"] = observedInputs.Any(input => input.Snapshot is not null).ToString();
        atlasStats["observedSources"] = observedInputs.Count.ToString();

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
                ["eventsLoaded"] = observedInputs.Sum(input => input.Events.Count).ToString(),
                ["snapshotLoaded"] = observedInputs.Any(input => input.Snapshot is not null).ToString(),
                ["observedSources"] = observedInputs.Count.ToString(),
            },
            Array.Empty<string>()));

        var managedRoot = Path.Combine(configuration.GamePaths.GameDirectory, "data_sts2_windows_x86_64");
        var assemblyPath = Path.Combine(managedRoot, "sts2.dll");
        var decompileScan = DecompileKnowledgeScanner.EnsureDecompiled(assemblyPath, decompiledRoot, out var decompileWarnings);
        warnings.AddRange(decompileWarnings);
        WriteJson(decompileScanPath, decompileScan);
        steps.Add(new StaticKnowledgePipelineStep(
            "decompile-scan",
            decompileWarnings.Count == 0 ? "completed" : "warning",
            decompileScanPath,
            decompileScan.Stats,
            decompileWarnings));

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

        var strictDomainCatalog = StrictDomainKnowledgeScanner.Scan(decompiledRoot, pckCatalog, metadata, out var strictWarnings);
        warnings.AddRange(strictWarnings);
        WriteJson(strictDomainScanPath, strictDomainCatalog);
        steps.Add(new StaticKnowledgePipelineStep(
            "strict-domain-parse",
            strictWarnings.Count == 0 ? "completed" : "warning",
            strictDomainScanPath,
            ToStringMap(BuildCounts(strictDomainCatalog)),
            strictWarnings));

        var rawAuxiliaryCatalog = BuildAuxiliaryCatalog(metadata, assemblyCatalog, pckCatalog);
        var seedCatalog = StaticKnowledgeCatalogBuilder.MergeCatalogs(metadata, strictDomainCatalog, rawAuxiliaryCatalog);
        var localizationScan = LocalizationKnowledgeScanner.Scan(pckPath, seedCatalog, out var localizationWarnings);
        warnings.AddRange(localizationWarnings);
        WriteJson(localizationScanPath, localizationScan);
        steps.Add(new StaticKnowledgePipelineStep(
            "localization-scan",
            localizationWarnings.Count == 0 ? "completed" : "warning",
            localizationScanPath,
            ToStringMap(BuildLocalizationCounts(localizationScan)),
            localizationWarnings));
        seedCatalog = StaticKnowledgeCatalogBuilder.MergeLocalization(seedCatalog, localizationScan, metadata);

        StaticKnowledgeCatalog? observedCatalog = null;
        foreach (var input in observedInputs)
        {
            observedCatalog = StaticKnowledgeCatalogBuilder.BuildFromObserved(observedCatalog, input.Snapshot, input.Events, metadata);
        }
        observedCatalog ??= StaticKnowledgeCatalog.CreateEmpty();
        WriteJson(observedMergePath, observedCatalog);
        steps.Add(new StaticKnowledgePipelineStep(
            "observed-merge",
            observedInputs.All(input => input.Snapshot is null && input.Events.Count == 0) ? "warning" : "completed",
            observedMergePath,
            ToStringMap(BuildCounts(observedCatalog)),
            observedInputs.All(input => input.Snapshot is null && input.Events.Count == 0)
                ? new[] { "No live snapshot or events were available for observed-merge." }
                : Array.Empty<string>()));
        var catalog = seedCatalog;
        foreach (var input in observedInputs)
        {
            catalog = StaticKnowledgeCatalogBuilder.BuildFromObserved(catalog, input.Snapshot, input.Events, metadata);
        }
        var assistantCatalog = StaticKnowledgeCatalogBuilder.BuildAssistantCatalog(catalog);
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
            BuildSourceFiles(configuration, layout, catalogPath, decompileScanPath, strictDomainScanPath, assemblyScanPath, pckInventoryPath, localizationScanPath, observedMergePath),
            steps,
            warnings);

        WriteJson(catalogPath, catalog);
        File.WriteAllText(summaryPath, StaticKnowledgeSummaryFormatter.Format(catalog));
        WriteJson(assistantCatalogPath, assistantCatalog);
        File.WriteAllText(assistantSummaryPath, StaticKnowledgeSummaryFormatter.Format(assistantCatalog));
        AssistantKnowledgeExportWriter.Write(assistantRoot, assistantCatalog);
        WriteJson(sourceManifestPath, sourceManifest);
        StaticKnowledgeMarkdownReportFormatter.WriteReports(markdownRoot, catalog, sourceManifest);

        return new StaticKnowledgeExtractionResult(
            knowledgeRoot,
            catalogPath,
            assistantCatalogPath,
            summaryPath,
            assistantSummaryPath,
            assistantRoot,
            sourceManifestPath,
            decompileScanPath,
            strictDomainScanPath,
            assemblyScanPath,
            pckInventoryPath,
            localizationScanPath,
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
        string decompileScanPath,
        string strictDomainScanPath,
        string assemblyScanPath,
        string pckInventoryPath,
        string localizationScanPath,
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
            DescribeSourceFile("decompile-scan", decompileScanPath),
            DescribeSourceFile("strict-domain-scan", strictDomainScanPath),
            DescribeSourceFile("assembly-scan", assemblyScanPath),
            DescribeSourceFile("pck-inventory", pckInventoryPath),
            DescribeSourceFile("localization-scan", localizationScanPath),
            DescribeSourceFile("observed-merge", observedMergePath),
        };
    }

    private static StaticKnowledgeCatalog BuildAuxiliaryCatalog(
        StaticKnowledgeMetadata metadata,
        StaticKnowledgeCatalog assemblyCatalog,
        StaticKnowledgeCatalog pckCatalog)
    {
        return new StaticKnowledgeCatalog(
            DateTimeOffset.UtcNow,
            metadata,
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>());
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

    private static IReadOnlyList<ObservedKnowledgeInput> ReadObservedInputs(
        ScaffoldConfiguration configuration,
        string knowledgeRoot,
        LiveExportLayout layout,
        ICollection<string> warnings)
    {
        var inputs = new List<ObservedKnowledgeInput>
        {
            new(
                "live-root",
                TryReadJson<LiveExportSnapshot>(layout.SnapshotPath),
                ReadNdjson<LiveExportEventEnvelope>(layout.EventsPath, warnings)),
        };

        var artifactsRoot = Directory.GetParent(knowledgeRoot)?.FullName;
        if (string.IsNullOrWhiteSpace(artifactsRoot))
        {
            return inputs;
        }

        var companionRoot = Path.Combine(artifactsRoot, configuration.Assistant.CompanionArtifactsRelativeRoot);
        if (!Directory.Exists(companionRoot))
        {
            return inputs;
        }

        foreach (var runDirectory in Directory.EnumerateDirectories(companionRoot))
        {
            var liveMirrorRoot = Path.Combine(runDirectory, "live-mirror");
            if (!Directory.Exists(liveMirrorRoot))
            {
                continue;
            }

            var snapshotPath = Path.Combine(liveMirrorRoot, "state.latest.json");
            var eventsPath = Path.Combine(liveMirrorRoot, "events.ndjson");
            var snapshot = TryReadJson<LiveExportSnapshot>(snapshotPath);
            var events = ReadNdjson<LiveExportEventEnvelope>(eventsPath, warnings);
            if (snapshot is null && events.Count == 0)
            {
                continue;
            }

            inputs.Add(new(Path.GetFileName(runDirectory), snapshot, events));
        }

        return inputs;
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

    private static object BuildLocalizationCounts(StaticKnowledgeLocalizationScan scan)
    {
        return new
        {
            cards = scan.Cards.Count,
            titles = scan.Cards.Count(card => !string.IsNullOrWhiteSpace(card.Title)),
            descriptions = scan.Cards.Count(card => !string.IsNullOrWhiteSpace(card.Description)),
            selectionPrompts = scan.Cards.Count(card => !string.IsNullOrWhiteSpace(card.SelectionScreenPrompt)),
            koreanPreferred = scan.Cards.Count(card => string.Equals(card.PreferredLocale, "kor", StringComparison.OrdinalIgnoreCase)),
            englishPreferred = scan.Cards.Count(card => string.Equals(card.PreferredLocale, "eng", StringComparison.OrdinalIgnoreCase)),
            relics = scan.Relics.Count,
            relicDescriptions = scan.Relics.Count(entry => !string.IsNullOrWhiteSpace(entry.Description)),
            potions = scan.Potions.Count,
            potionDescriptions = scan.Potions.Count(entry => !string.IsNullOrWhiteSpace(entry.Description)),
            events = scan.Events.Count,
            eventDescriptions = scan.Events.Count(entry => !string.IsNullOrWhiteSpace(entry.Description)),
            eventOptions = scan.Events.Sum(entry => entry.Options.Count),
            shops = scan.Shops.Count,
            shopDescriptions = scan.Shops.Count(entry => !string.IsNullOrWhiteSpace(entry.Description)),
            rewards = scan.Rewards.Count,
            rewardDescriptions = scan.Rewards.Count(entry => !string.IsNullOrWhiteSpace(entry.Description)),
            keywords = scan.Keywords.Count,
            keywordDescriptions = scan.Keywords.Count(entry => !string.IsNullOrWhiteSpace(entry.Description)),
        };
    }

    private static IReadOnlyList<string> BuildSampleEntries(IReadOnlyList<StaticKnowledgeEntry> entries)
    {
        return entries
            .OrderByDescending(entry => !string.IsNullOrWhiteSpace(TryReadAttribute(entry, "description")))
            .ThenByDescending(entry => string.Equals(entry.Source, "localization-scan", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(entry => !string.IsNullOrWhiteSpace(TryReadAttribute(entry, "fullName")))
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .Select(entry =>
            {
                var description = TryReadAttribute(entry, "description");
                return string.IsNullOrWhiteSpace(description)
                    ? entry.Name
                    : $"{entry.Name}: {TrimForSample(description!, 72)}";
            })
            .ToArray();
    }

    private static string? TryReadAttribute(StaticKnowledgeEntry entry, string key)
    {
        return entry.Attributes.TryGetValue(key, out var value)
            ? value
            : null;
    }

    private static string TrimForSample(string value, int maxLength)
    {
        var collapsed = value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();
        return collapsed.Length <= maxLength
            ? collapsed
            : $"{collapsed[..maxLength].TrimEnd()}...";
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
