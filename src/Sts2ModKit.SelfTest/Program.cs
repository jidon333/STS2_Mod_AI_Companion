using Sts2AiCompanion.Host;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Diagnostics;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;
using Sts2ModKit.Core.Planning;
using Sts2ModAiCompanion.Mod;
using Sts2ModAiCompanion.Mod.Runtime;

var failures = new List<string>();

Run("configuration loader reads AI companion config", TestConfigurationLoader, failures);
Run("live export path resolver uses modded profile layout", TestLiveExportPathResolver, failures);
Run("snapshot planner includes required files", TestSnapshotPlanner, failures);
Run("snapshot execution copies and verifies files", TestSnapshotExecutionAndVerification, failures);
Run("strict restore removes files created after snapshot", TestStrictRestoreRemovesCreatedFiles, failures);
Run("modded profile sync mirrors vanilla profile after backing up destination", TestModdedProfileSync, failures);
Run("restore plan mirrors snapshot entries", TestRestorePlan, failures);
Run("native package staging captures missing pck requirement", TestMaterializeNativePackage, failures);
Run("native package writes runtime config and matching dll", TestNativePackageContents, failures);
Run("live export atomic writer replaces latest snapshot contents", TestLiveExportAtomicWriter, failures);
Run("live export tracker emits monotonic event sequence", TestLiveExportTrackerSequence, failures);
Run("live export deduplicator suppresses duplicate observations", TestLiveExportDeduplicator, failures);
Run("live export summary contains required sections", TestLiveExportSummaryFormatting, failures);
Run("live export replay stays deterministic", TestLiveExportReplayDeterminism, failures);
Run("static knowledge builder merges observed snapshot data", TestStaticKnowledgeCatalogBuilder, failures);
Run("smoke diagnostics surface startup patch failures", TestSmokeDiagnostics, failures);
Run("runtime reflection invoker supports optional parameters", TestRuntimeReflectionOptionalParameters, failures);
Run("companion path resolver keeps per-run artifacts under companion root", TestCompanionPathResolver, failures);
Run("knowledge catalog service builds a bounded relevant slice", TestKnowledgeCatalogService, failures);
Run("advice prompt builder emits the required prompt sections", TestAdvicePromptBuilder, failures);

if (failures.Count == 0)
{
    Console.WriteLine("All self-tests passed.");
    return 0;
}

Console.Error.WriteLine($"{failures.Count} self-test(s) failed.");
foreach (var failure in failures)
{
    Console.Error.WriteLine($"  - {failure}");
}

return 1;

static void Run(string name, Action test, ICollection<string> failures)
{
    try
    {
        test();
        Console.WriteLine($"[PASS] {name}");
    }
    catch (Exception exception)
    {
        failures.Add($"{name}: {exception.Message}");
        Console.WriteLine($"[FAIL] {name}");
    }
}

static void TestConfigurationLoader()
{
    const string json = """
    {
      "gamePaths": {
        "gameDirectory": "D:\\Games\\Slay the Spire 2",
        "steamAccountId": "111"
      },
      "aiCompanionMod": {
        "name": "My Test Mod",
        "pckName": "my-test-mod.pck"
      }
    }
    """;

    var result = ConfigurationLoader.LoadFromJson(json, "inline");

    Assert(result.Configuration.GamePaths.GameDirectory.EndsWith("Slay the Spire 2", StringComparison.Ordinal), "Expected custom gameDirectory.");
    Assert(result.Configuration.GamePaths.SteamAccountId == "111", "Expected custom steam account id.");
    Assert(result.Configuration.AiCompanionMod.Name == "My Test Mod", "Expected custom mod name.");
    Assert(result.Configuration.AiCompanionMod.PckName == "my-test-mod.pck", "Expected custom pck name.");
    Assert(result.Configuration.LiveExport.Enabled, "Expected live export to remain enabled by default.");
    Assert(result.Configuration.LiveExport.ScenePollingEnabled, "Expected scene polling to remain enabled by default.");
}

static void TestLiveExportPathResolver()
{
    var configuration = ScaffoldConfiguration.CreateLocalDefault();
    var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);

    Assert(layout.LiveRoot.EndsWith(Path.Combine("steam", configuration.GamePaths.SteamAccountId, "modded", "profile1", "ai_companion", "live"), StringComparison.OrdinalIgnoreCase), "Expected live export layout to use the modded profile path.");
    Assert(layout.EventsPath.EndsWith("events.ndjson", StringComparison.OrdinalIgnoreCase), "Expected NDJSON events path.");
}

static void TestSnapshotPlanner()
{
    var options = new GamePathOptions
    {
        GameDirectory = @"D:\Fake\Slay the Spire 2",
        UserDataRoot = @"C:\Users\Test\AppData\Roaming\SlayTheSpire2",
        SteamAccountId = "1234567890",
        ProfileIndex = 1,
        ArtifactsRoot = "artifacts",
    };

    var modsRoot = Path.Combine(options.GameDirectory, "mods");
    var existingSpeedModPath = Path.Combine(modsRoot, "sts2-speed-skeleton.pck");
    var trackedAiPaths = new[]
    {
        Path.Combine(modsRoot, "sts2-mod-ai-companion.config.json"),
        Path.Combine(modsRoot, "sts2-mod-ai-companion.dll"),
        Path.Combine(modsRoot, "sts2-mod-ai-companion.pck"),
        Path.Combine(modsRoot, "Sts2ModKit.Core.dll"),
    };

    var probe = new FakeProbe(
        new[]
        {
            Path.Combine(options.GameDirectory, "release_info.json"),
            Path.Combine(options.UserDataRoot, "steam", options.SteamAccountId, "settings.save"),
            existingSpeedModPath,
        },
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [modsRoot] = new[] { existingSpeedModPath },
        });

    var plan = SnapshotPlanner.CreateDefaultPlan(options, @"C:\workspace\artifacts\snapshots\test", probe, trackedAiPaths);

    Assert(plan.Entries.Any(entry => entry.BackupPath.EndsWith(Path.Combine("game", "release_info.json"), StringComparison.OrdinalIgnoreCase)), "Missing release_info snapshot entry.");
    Assert(plan.Entries.Any(entry => entry.SourcePath == existingSpeedModPath && entry.Exists), "Expected snapshot plan to include existing foreign mods files for restore.");
    Assert(plan.Entries.Any(entry => entry.SourcePath == trackedAiPaths[0] && !entry.Exists), "Expected snapshot plan to track missing AI deploy files so restore can delete them later.");
    Assert(plan.Entries.Count(entry => entry.Exists) == 3, "Fake probe should mark exactly three files as existing.");
}

static void TestRestorePlan()
{
    var options = GamePathOptions.CreateLocalDefault();
    var plan = SnapshotPlanner.CreateDefaultPlan(options, @"C:\workspace\artifacts\snapshots\test", new FakeProbe());
    var restore = SnapshotPlanner.CreateRestorePlan(plan, new FakeProbe(new[] { plan.Entries[0].BackupPath }));

    Assert(restore.Entries.Count == plan.Entries.Count, "Restore plan should mirror snapshot entry count.");
    Assert(restore.Entries[0].DestinationPath == plan.Entries[0].SourcePath, "Restore destination should map back to the original source path.");
    Assert(restore.Entries[0].BackupExists, "Fake probe should mark the first backup path as existing.");
}

static void TestSnapshotExecutionAndVerification()
{
    var root = CreateTempDirectory();
    try
    {
        var gameDirectory = Path.Combine(root, "game");
        var userDataRoot = Path.Combine(root, "userdata");
        var steamDirectory = Path.Combine(userDataRoot, "steam", "1234567890");
        var saveDirectory = Path.Combine(steamDirectory, "profile1", "saves");
        Directory.CreateDirectory(gameDirectory);
        Directory.CreateDirectory(saveDirectory);

        var releaseInfoPath = Path.Combine(gameDirectory, "release_info.json");
        var settingsPath = Path.Combine(steamDirectory, "settings.save");
        var prefsPath = Path.Combine(saveDirectory, "prefs.save");
        File.WriteAllText(releaseInfoPath, """{"version":"test"}""");
        File.WriteAllText(settingsPath, """{"screenshake":"true"}""");
        File.WriteAllText(prefsPath, """{"fast_mode":"fast"}""");

        var options = new GamePathOptions
        {
            GameDirectory = gameDirectory,
            UserDataRoot = userDataRoot,
            SteamAccountId = "1234567890",
            ProfileIndex = 1,
            ArtifactsRoot = Path.Combine(root, "artifacts"),
        };

        var snapshotRoot = SnapshotPlanner.BuildSnapshotRoot(options, new DateTimeOffset(2026, 3, 7, 10, 0, 0, TimeSpan.Zero));
        var plan = SnapshotPlanner.CreateDefaultPlan(options, snapshotRoot);
        var snapshot = SnapshotExecutor.ExecuteSnapshot(plan);

        Assert(snapshot.Entries.Count(entry => entry.Status == "copied") == 3, "Expected three copied files in the snapshot result.");
        Assert(File.Exists(Path.Combine(snapshotRoot, "game", "release_info.json")), "Snapshot should contain release_info.json.");

        var verification = SnapshotExecutor.VerifySnapshot(snapshot);
        Assert(verification.AllEntriesMatch, "Unchanged files should verify successfully.");

        File.WriteAllText(settingsPath, """{"screenshake":"false"}""");
        var changedVerification = SnapshotExecutor.VerifySnapshot(snapshot);
        Assert(!changedVerification.AllEntriesMatch, "Changing a source file should fail verification.");
        Assert(changedVerification.Entries.Any(entry => entry.SourcePath == settingsPath && entry.Status == "changed"), "Expected settings.save to be marked as changed.");

        var restorePlan = SnapshotPlanner.CreateRestorePlan(snapshot);
        SnapshotExecutor.ExecuteRestore(restorePlan);
        Assert(File.ReadAllText(settingsPath).Contains("true", StringComparison.Ordinal), "Restore should put the original settings file back.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestStrictRestoreRemovesCreatedFiles()
{
    var root = CreateTempDirectory();
    try
    {
        var snapshotRoot = Path.Combine(root, "snapshot");
        var createdFilePath = Path.Combine(root, "game", "release_info.json");
        Directory.CreateDirectory(Path.GetDirectoryName(createdFilePath)!);
        File.WriteAllText(createdFilePath, "created later");

        var snapshot = new SnapshotExecutionResult(
            snapshotRoot,
            Path.Combine(snapshotRoot, "snapshot-report.json"),
            DateTimeOffset.UtcNow,
            new[]
            {
                new SnapshotExecutionEntry(
                    "GameInstall",
                    createdFilePath,
                    Path.Combine(snapshotRoot, "game", "release_info.json"),
                    false,
                    "missing",
                    null,
                    null),
            },
            Array.Empty<string>());

        var restore = SnapshotExecutor.ExecuteRestoreToSnapshotState(snapshot);

        Assert(!File.Exists(createdFilePath), "Strict restore should delete files that did not exist at snapshot time.");
        Assert(restore.Entries.Any(entry => entry.Status == "deleted-created-after-snapshot"), "Strict restore should report deletion of created files.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestModdedProfileSync()
{
    var root = CreateTempDirectory();
    try
    {
        var userDataRoot = Path.Combine(root, "userdata");
        var steamRoot = Path.Combine(userDataRoot, "steam", "1234567890");
        var sourceRoot = Path.Combine(steamRoot, "profile1");
        var destinationRoot = Path.Combine(steamRoot, "modded", "profile1");
        var sourceSaves = Path.Combine(sourceRoot, "saves");
        var destinationSaves = Path.Combine(destinationRoot, "saves");
        Directory.CreateDirectory(sourceSaves);
        Directory.CreateDirectory(destinationSaves);

        File.WriteAllText(Path.Combine(sourceSaves, "progress.save"), "vanilla-progress");
        File.WriteAllText(Path.Combine(sourceSaves, "prefs.save"), "vanilla-prefs");
        File.WriteAllText(Path.Combine(destinationSaves, "progress.save"), "old-modded-progress");

        var options = new GamePathOptions
        {
            GameDirectory = Path.Combine(root, "game"),
            UserDataRoot = userDataRoot,
            SteamAccountId = "1234567890",
            ProfileIndex = 1,
            ArtifactsRoot = Path.Combine(root, "artifacts"),
        };

        var result = ModdedProfileSync.SyncVanillaToModded(options, options.ArtifactsRoot, new DateTimeOffset(2026, 3, 7, 12, 0, 0, TimeSpan.Zero));

        Assert(File.Exists(Path.Combine(result.BackupRoot, "modded-profile-before-sync", "saves", "progress.save")), "Sync should back up the previous modded progress.");
        Assert(File.ReadAllText(Path.Combine(destinationSaves, "progress.save")) == "vanilla-progress", "Sync should replace modded progress with vanilla progress.");
        Assert(File.ReadAllText(Path.Combine(destinationSaves, "prefs.save")) == "vanilla-prefs", "Sync should copy additional vanilla files.");
        Assert(result.Files.Count == 2, $"Expected two copied files but received {result.Files.Count}.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestMaterializeNativePackage()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = ScaffoldConfiguration.CreateLocalDefault() with
        {
            GamePaths = ScaffoldConfiguration.CreateLocalDefault().GamePaths with
            {
                GameDirectory = Path.Combine(root, "game"),
                ArtifactsRoot = Path.Combine(root, "artifacts"),
            },
        };

        var result = AiCompanionModEntryPoint.MaterializeNativePackage(configuration, configuration.GamePaths.ArtifactsRoot, AppContext.BaseDirectory, "subdir");

        Assert(File.Exists(Path.Combine(result.PackageRoot, "sts2-mod-ai-companion.config.json")), "Native staging package should include the AI companion runtime config file.");
        Assert(File.Exists(Path.Combine(result.PackageRoot, "sts2-mod-ai-companion.dll")), "Native staging package should include the pck-matching managed payload.");
        Assert(!File.Exists(Path.Combine(result.PackageRoot, "mod_manifest.json")), "Native staging package should keep mod_manifest.json inside the generated pck rather than as a loose file.");
        Assert(result.MissingArtifacts.Any(artifact => artifact.RelativePath.EndsWith(".pck", StringComparison.OrdinalIgnoreCase)), "Native staging package should report that a .pck artifact is still missing.");
        Assert(result.LayoutKind == "subdir", "Native staging package should normalize the requested layout kind.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestNativePackageContents()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = ScaffoldConfiguration.CreateLocalDefault() with
        {
            GamePaths = ScaffoldConfiguration.CreateLocalDefault().GamePaths with
            {
                GameDirectory = Path.Combine(root, "game"),
                ArtifactsRoot = Path.Combine(root, "artifacts"),
            },
            AiCompanionMod = ScaffoldConfiguration.CreateLocalDefault().AiCompanionMod with
            {
                Name = "Packaged AI Companion",
                PckName = "packaged-template.pck",
                PackageFolderName = "PackagedAiCompanion",
            },
        };

        var result = AiCompanionModEntryPoint.MaterializeNativePackage(configuration, configuration.GamePaths.ArtifactsRoot, AppContext.BaseDirectory, "flat");
        var configJson = File.ReadAllText(Path.Combine(result.PackageRoot, "sts2-mod-ai-companion.config.json"));
        var runtimeConfig = System.Text.Json.JsonSerializer.Deserialize<AiCompanionRuntimeConfig>(configJson, ConfigurationLoader.JsonOptions);

        Assert(runtimeConfig is not null, "Expected packaged runtime config to deserialize.");
        Assert(runtimeConfig!.Enabled, "Expected packaged runtime config to enable the AI companion payload.");
        Assert(runtimeConfig.GamePaths.SteamAccountId == configuration.GamePaths.SteamAccountId, "Expected runtime config to carry the game paths used for live export.");
        Assert(runtimeConfig.LiveExport.RelativeLiveRoot == configuration.LiveExport.RelativeLiveRoot, "Expected runtime config to carry live export settings.");
        Assert(File.Exists(Path.Combine(result.PackageRoot, "packaged-template.dll")), "Expected packaged dll to follow the configured pck basename.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestLiveExportAtomicWriter()
{
    var root = CreateTempDirectory();
    try
    {
        var path = Path.Combine(root, "state.latest.json");
        LiveExportAtomicFileWriter.WriteAllTextAtomic(path, "first");
        LiveExportAtomicFileWriter.WriteAllTextAtomic(path, "second");

        Assert(File.ReadAllText(path) == "second", "Expected atomic writer to replace the previous contents.");
        Assert(!File.Exists(path + ".tmp"), "Expected atomic writer temp file to be cleaned up.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestLiveExportTrackerSequence()
{
    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live");
    var first = tracker.Apply(CreateObservation("run-started", "map", 1, 1, 70, 99, new[] { "Strike" }, new[] { "Anchor" }, Array.Empty<string>()));
    var second = tracker.Apply(CreateObservation("reward-screen-opened", "rewards", 1, 1, 70, 120, new[] { "Strike", "Defend" }, new[] { "Anchor", "Kunai" }, new[] { "Card Reward" }));

    Assert(first.Events.Count == 1, "Expected the first observation to emit a single event.");
    Assert(second.Events.All(evt => evt.Seq > first.Events[0].Seq), "Expected all later events to use a higher sequence number.");
    Assert(second.Events.Any(evt => evt.Kind == "screen-changed"), "Expected screen changes to emit a derived event.");
    Assert(second.Events.Any(evt => evt.Kind == "card-added"), "Expected snapshot diff to emit a card add event.");
    Assert(second.Events.Any(evt => evt.Kind == "relic-gained"), "Expected snapshot diff to emit a relic gain event.");
    Assert(second.Events.Last().Kind == "reward-screen-opened", "Expected the final event in the batch to be the trigger event itself.");
}

static void TestLiveExportDeduplicator()
{
    var deduplicator = new LiveExportDeduplicator(TimeSpan.FromMilliseconds(300));
    var observation = CreateObservation("turn-started", "combat", 1, 2, 50, 30, new[] { "Strike" }, Array.Empty<string>(), Array.Empty<string>()) with
    {
        ObservedAt = new DateTimeOffset(2026, 3, 10, 9, 0, 0, TimeSpan.Zero),
    };

    Assert(!deduplicator.ShouldSuppress(observation), "Expected the first observation to pass through the deduplicator.");
    Assert(deduplicator.ShouldSuppress(observation with { ObservedAt = observation.ObservedAt.AddMilliseconds(100) }), "Expected a duplicate observation inside the suppression window to be dropped.");
    Assert(!deduplicator.ShouldSuppress(observation with { ObservedAt = observation.ObservedAt.AddMilliseconds(400) }), "Expected the same observation outside the suppression window to pass through.");
}

static void TestLiveExportSummaryFormatting()
{
    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live");
    var batch = tracker.Apply(CreateObservation("choice-list-presented", "shop", 2, 12, 55, 180, new[] { "Strike", "Defend" }, new[] { "Anchor" }, new[] { "Swift Potion" }));
    var summary = LiveExportSummaryFormatter.Format(batch.Snapshot);

    Assert(summary.Contains("run status", StringComparison.Ordinal), "Expected summary to include the run status section.");
    Assert(summary.Contains("current screen / encounter", StringComparison.Ordinal), "Expected summary to include the screen section.");
    Assert(summary.Contains("player summary", StringComparison.Ordinal), "Expected summary to include the player section.");
    Assert(summary.Contains("current choices", StringComparison.Ordinal), "Expected summary to include the choices section.");
}

static void TestLiveExportReplayDeterminism()
{
    var observations = new[]
    {
        CreateObservation("run-started", "map", 1, 1, 70, 99, new[] { "Strike" }, new[] { "Anchor" }, Array.Empty<string>()),
        CreateObservation("choice-list-presented", "event", 1, 2, 68, 88, new[] { "Strike", "Defend" }, new[] { "Anchor" }, Array.Empty<string>()),
        CreateObservation("combat-started", "combat", 1, 3, 60, 42, new[] { "Strike", "Defend" }, new[] { "Anchor" }, new[] { "Dexterity Potion" }),
    };

    var trackerA = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live-a");
    var trackerB = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live-b");
    var replayA = trackerA.Replay(observations);
    var replayB = trackerB.Replay(observations);

    Assert(replayA.Snapshot.RunStatus == replayB.Snapshot.RunStatus, "Expected replay run status to be deterministic.");
    Assert(replayA.Snapshot.CurrentScreen == replayB.Snapshot.CurrentScreen, "Expected replay screen to be deterministic.");
    Assert(replayA.Snapshot.Deck.Select(card => card.Name).SequenceEqual(replayB.Snapshot.Deck.Select(card => card.Name), StringComparer.Ordinal), "Expected replay deck contents to match.");
    Assert(replayA.Events.Select(evt => evt.Kind).SequenceEqual(replayB.Events.Select(evt => evt.Kind), StringComparer.Ordinal), "Expected replay event kinds to match.");
}

static void TestStaticKnowledgeCatalogBuilder()
{
    var metadata = new StaticKnowledgeMetadata(
        "v0.98.2",
        "f4eeecc6",
        new DateTimeOffset(2026, 3, 6, 15, 52, 37, TimeSpan.FromHours(-8)),
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["atlas:card_atlas"] = "840",
        });
    var observation = CreateObservation(
        "choice-list-presented",
        "shop",
        2,
        12,
        55,
        180,
        new[] { "Strike", "Defend" },
        new[] { "Anchor" },
        new[] { "Swift Potion" }) with
    {
        Choices = new[]
        {
            new LiveExportChoiceSummary("card", "Pommel Strike", "pommel-strike", "Gain block and draw."),
            new LiveExportChoiceSummary("relic", "Bag of Marbles", "bag-of-marbles", "Apply Vulnerable on turn one."),
        },
    };
    var snapshot = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\knowledge").Apply(observation).Snapshot;

    var catalog = StaticKnowledgeCatalogBuilder.BuildFromObserved(
        baseline: null,
        snapshot,
        new[]
        {
            new LiveExportEventEnvelope(
                DateTimeOffset.UtcNow,
                1,
                "run-001",
                "card-added",
                "rewards",
                2,
                12,
                new Dictionary<string, object?> { ["item"] = "Pommel Strike" }),
        },
        metadata);

    Assert(catalog.Cards.Any(entry => entry.Name == "Strike"), "Expected observed deck cards to appear in the catalog.");
    Assert(catalog.Cards.Any(entry => entry.Name == "Pommel Strike"), "Expected observed card choices to appear in the catalog.");
    Assert(catalog.Relics.Any(entry => entry.Name == "Anchor"), "Expected observed relics to appear in the catalog.");
    Assert(catalog.Potions.Any(entry => entry.Name == "Swift Potion"), "Expected observed potions to appear in the catalog.");
    Assert(catalog.Shops.Any(entry => entry.Options.Any(option => option.Label == "Pommel Strike")), "Expected observed shop choices to produce a shop catalog entry.");
    Assert(catalog.Keywords.Any(entry => entry.Name == "attack"), "Expected card type keywords to be captured.");
}

static void TestSmokeDiagnostics()
{
    var lines = new[]
    {
        "[INFO] Loading assembly DLL sts2-mod-ai-companion.dll",
        "[INFO] No ModInitializerAttribute detected. Calling Harmony.PatchAll for Sts2ModAiCompanion.Mod, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null",
        "[ERROR] Exception caught while trying to run PatchAll on assembly Sts2ModAiCompanion.Mod",
        "HarmonyLib.HarmonyException: Patching exception in method virtual System.Void MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen::_Ready()",
        "System.Exception: Cannot get result from void method virtual System.Void MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen::_Ready()",
    };

    var analysis = SmokeDiagnostics.AnalyzeGodotLog(lines, 3);
    var liveFindings = SmokeDiagnostics.AnalyzeLiveExport(
        runtimeConfigExists: true,
        liveRootExists: false,
        runtimeLogExists: false,
        sessionExists: false,
        snapshotExists: false,
        summaryExists: false,
        eventsExist: false,
        deployedModFiles: new[] { "sts2-mod-ai-companion.dll", "sts2-mod-ai-companion.pck" },
        liveRoot: @"C:\Users\Test\AppData\Roaming\SlayTheSpire2\steam\123\modded\profile1\ai_companion\live");

    Assert(analysis.Findings.Any(finding => finding.Code == "HarmonyPatchAllStartupFailure"), "Expected PatchAll startup failures to surface as a finding.");
    Assert(analysis.StartupHighlights.Any(line => line.Contains("Harmony.PatchAll", StringComparison.OrdinalIgnoreCase)), "Expected startup highlights to include PatchAll lines.");
    Assert(liveFindings.Any(finding => finding.Code == "DeployedVsLiveRootMismatch"), "Expected missing live root to surface as a deployed-vs-live-root mismatch.");
}

static void TestRuntimeReflectionOptionalParameters()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod("TryInvokeMethod", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(method is not null, "Expected private TryInvokeMethod helper.");

    var probe = new OptionalParameterProbe();
    var count = method!.Invoke(null, new object?[] { probe, "GetChildCount", Array.Empty<object?>() });
    var child = method.Invoke(null, new object?[] { probe, "GetChild", new object?[] { 3 } });

    Assert(count is int intCount && intCount == 1, "Expected optional bool parameter to default when omitted.");
    Assert(string.Equals(child?.ToString(), "child-3-False", StringComparison.Ordinal), "Expected optional bool parameter to default to false when omitted.");
}

static void TestCompanionPathResolver()
{
    var configuration = ScaffoldConfiguration.CreateLocalDefault() with
    {
        GamePaths = ScaffoldConfiguration.CreateLocalDefault().GamePaths with
        {
            ArtifactsRoot = "artifacts",
        },
        Assistant = ScaffoldConfiguration.CreateLocalDefault().Assistant with
        {
            CompanionArtifactsRelativeRoot = "companion",
        },
    };

    var paths = CompanionPathResolver.Resolve(configuration, @"C:\workspace\repo", "run:001/alpha");

    Assert(paths.CompanionRoot == Path.Combine(@"C:\workspace\repo", "artifacts", "companion"), "Expected companion root under artifacts.");
    Assert(paths.RunRoot == Path.Combine(@"C:\workspace\repo", "artifacts", "companion", "run-001-alpha"), "Expected sanitized per-run directory.");
    Assert(paths.RunRoot is not null, "Expected the resolver to produce a run root for a non-empty run id.");
    Assert(paths.PromptPacksRoot == Path.Combine(paths.RunRoot!, "prompt-packs"), "Expected prompt pack directory under the run root.");
}

static void TestKnowledgeCatalogService()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = ScaffoldConfiguration.CreateLocalDefault() with
        {
            GamePaths = ScaffoldConfiguration.CreateLocalDefault().GamePaths with
            {
                ArtifactsRoot = Path.Combine(root, "artifacts"),
            },
        };
        var knowledgeRoot = Path.Combine(configuration.GamePaths.ArtifactsRoot, "knowledge");
        Directory.CreateDirectory(knowledgeRoot);

        var catalog = new StaticKnowledgeCatalog(
            DateTimeOffset.UtcNow,
            new StaticKnowledgeMetadata("v-test", "commit", DateTimeOffset.UtcNow, new Dictionary<string, string?>()),
            new[]
            {
                new StaticKnowledgeEntry(
                    "pommel-strike",
                    "Pommel Strike",
                    "observed-merge",
                    true,
                    "Draw 1 card.",
                    new[] { "card", "attack" },
                    new Dictionary<string, string?>(),
                    Array.Empty<StaticKnowledgeOption>()),
            },
            new[]
            {
                new StaticKnowledgeEntry(
                    "anchor",
                    "Anchor",
                    "observed-merge",
                    true,
                    "Gain block on the first turn.",
                    new[] { "relic" },
                    new Dictionary<string, string?>(),
                    Array.Empty<StaticKnowledgeOption>()),
            },
            Array.Empty<StaticKnowledgeEntry>(),
            new[]
            {
                new StaticKnowledgeEntry(
                    "gremlin-match",
                    "Gremlin Match",
                    "assembly-scan",
                    false,
                    "An event example.",
                    new[] { "event" },
                    new Dictionary<string, string?>(),
                    Array.Empty<StaticKnowledgeOption>()),
            },
            new[]
            {
                new StaticKnowledgeEntry(
                    "merchant-shop",
                    "Merchant Shop",
                    "observed-merge",
                    true,
                    "Shop context.",
                    new[] { "shop" },
                    new Dictionary<string, string?>(),
                    new[]
                    {
                        new StaticKnowledgeOption("pommel-strike", "Pommel Strike", "Card for sale.", new Dictionary<string, string?>()),
                    }),
            },
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>());

        File.WriteAllText(
            Path.Combine(knowledgeRoot, "catalog.latest.json"),
            System.Text.Json.JsonSerializer.Serialize(catalog, ConfigurationLoader.JsonOptions));

        var observation = CreateObservation(
            "choice-list-presented",
            "shop",
            1,
            4,
            60,
            120,
            new[] { "Pommel Strike" },
            new[] { "Anchor" },
            Array.Empty<string>()) with
        {
            Choices = new[]
            {
                new LiveExportChoiceSummary("card", "Pommel Strike", "pommel-strike", "Card for sale."),
            },
        };
        var snapshot = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), Path.Combine(root, "live")).Apply(observation).Snapshot;
        var runState = new CompanionRunState(snapshot, null, "summary", Array.Empty<LiveExportEventEnvelope>(), false);

        var service = new KnowledgeCatalogService(configuration, root);
        var slice = service.BuildSlice(runState, 8, 4096);

        Assert(slice.Entries.Any(entry => entry.Name == "Pommel Strike"), "Expected the slice to include the matching card.");
        Assert(slice.Entries.Any(entry => entry.Name == "Anchor"), "Expected the slice to include the matching relic.");
        Assert(slice.Entries.Any(entry => entry.Name == "Merchant Shop"), "Expected the slice to include screen-context knowledge.");
        Assert(slice.Reasons.Contains("screen:shop", StringComparer.OrdinalIgnoreCase), "Expected the slice to record the screen reason.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestAdvicePromptBuilder()
{
    var configuration = ScaffoldConfiguration.CreateLocalDefault();
    var builder = new AdvicePromptBuilder(configuration);
    var observation = CreateObservation(
        "choice-list-presented",
        "event",
        2,
        9,
        48,
        88,
        new[] { "Strike" },
        new[] { "Anchor" },
        Array.Empty<string>()) with
    {
        Choices = new[]
        {
            new LiveExportChoiceSummary("event", "Accept", "accept", "Take the risky reward."),
        },
    };
    var batch = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\assistant").Apply(observation);
    var runState = new CompanionRunState(batch.Snapshot, null, "state summary", batch.Events, false);
    var slice = new KnowledgeSlice(
        new[]
        {
            new StaticKnowledgeEntry(
                "accept",
                "Accept",
                "observed-merge",
                true,
                "An example choice.",
                new[] { "event" },
                new Dictionary<string, string?>(),
                Array.Empty<StaticKnowledgeOption>()),
        },
        32,
        new[] { "choice-label" });
    var trigger = new AdviceTrigger("choice-list-presented", DateTimeOffset.UtcNow, false, true, "new-choice", batch.Events.Last());

    var inputPack = builder.BuildInputPack(runState, trigger, slice);
    var prompt = builder.FormatPrompt(inputPack);

    Assert(prompt.Contains("You are a Slay the Spire 2 advice assistant.", StringComparison.Ordinal), "Expected prompt preamble.");
    Assert(prompt.Contains("current_state_summary:", StringComparison.Ordinal), "Expected prompt to include the state summary section.");
    Assert(prompt.Contains("knowledge_slice:", StringComparison.Ordinal), "Expected prompt to include the knowledge slice section.");
    Assert(prompt.Contains("response_instructions:", StringComparison.Ordinal), "Expected prompt to include the response instructions section.");
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static string CreateTempDirectory()
{
    var path = Path.Combine(Path.GetTempPath(), "Sts2ModAiCompanionTests", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(path);
    return path;
}

static void SafeDeleteDirectory(string path)
{
    if (!Directory.Exists(path))
    {
        return;
    }

    Directory.Delete(path, recursive: true);
}

static LiveExportObservation CreateObservation(
    string triggerKind,
    string screen,
    int act,
    int floor,
    int currentHp,
    int gold,
    IEnumerable<string> deck,
    IEnumerable<string> relics,
    IEnumerable<string> potions)
{
    return new LiveExportObservation(
        triggerKind,
        DateTimeOffset.UtcNow,
        "run-001",
        null,
        screen,
        act,
        floor,
        new LiveExportPlayerSummary("Ironclad", currentHp, 80, gold, 3, new Dictionary<string, string?>()),
        deck.Select(card => new LiveExportCardSummary(card, card.ToLowerInvariant(), 1, "attack", card.Contains('+'))).ToArray(),
        relics.ToArray(),
        potions.ToArray(),
        new[] { new LiveExportChoiceSummary("choice", "Take reward", "take", "Accept the visible reward.") },
        Array.Empty<string>(),
        new LiveExportEncounterSummary("Cultist", "combat", screen == "combat", screen == "combat" ? 1 : null),
        new Dictionary<string, object?>
        {
            ["screen"] = screen,
        },
        new Dictionary<string, string?>());
}

file sealed class OptionalParameterProbe
{
    public int GetChildCount(bool includeInternal = false)
    {
        return includeInternal ? 2 : 1;
    }

    public string GetChild(int index, bool includeInternal = false)
    {
        return $"child-{index}-{includeInternal}";
    }
}

sealed class FakeProbe : IFileStateProbe
{
    private readonly HashSet<string> existingPaths;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> filesByDirectory;

    public FakeProbe(IEnumerable<string>? existingPaths = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? filesByDirectory = null)
    {
        this.existingPaths = new HashSet<string>(existingPaths ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        this.filesByDirectory = filesByDirectory ?? new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
    }

    public bool FileExists(string path)
    {
        return existingPaths.Contains(path);
    }

    public IReadOnlyList<string> EnumerateFiles(string directoryPath)
    {
        return filesByDirectory.TryGetValue(directoryPath, out var files)
            ? files
            : Array.Empty<string>();
    }
}
