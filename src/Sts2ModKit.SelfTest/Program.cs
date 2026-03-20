using System.Text.Json;
using System.Text;
using Sts2AiCompanion.Foundation.State;
using Sts2AiCompanion.Harness.Actions;
using Sts2AiCompanion.Host;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Diagnostics;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;
using Sts2ModKit.Core.Planning;
using Sts2ModAiCompanion.Mod;
using Sts2ModAiCompanion.Mod.Runtime;
using FoundationKnowledgeCatalogService = Sts2AiCompanion.Foundation.Knowledge.KnowledgeCatalogService;
using FoundationAdvicePromptBuilder = Sts2AiCompanion.Foundation.Reasoning.AdvicePromptBuilder;
using FoundationCodexCliClient = Sts2AiCompanion.Foundation.Reasoning.CodexCliClient;

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
Run("strict domain scanner parses model-backed metadata", TestStrictDomainKnowledgeScanner, failures);
Run("localization scanner extracts card title and description from binary text", TestLocalizationKnowledgeScanner, failures);
Run("localization scan merges into canonical card entries", TestLocalizationKnowledgeMerge, failures);
Run("smoke diagnostics surface startup patch failures", TestSmokeDiagnostics, failures);
Run("runtime reflection invoker supports optional parameters", TestRuntimeReflectionOptionalParameters, failures);
Run("runtime reflection string extraction resolves nested label text", TestRuntimeReflectionStringExtraction, failures);
Run("runtime reflection rejects overlay-like player roots", TestRuntimeReflectionRejectsOverlayLikePlayerRoots, failures);
Run("runtime reflection extracts combat cards from player combat state", TestRuntimeReflectionExtractDeckFromCombatState, failures);
Run("runtime reflection encounter prefers CombatManager IsInProgress", TestRuntimeReflectionEncounterPrefersCombatManagerIsInProgress, failures);
Run("runtime reflection encounter does not override CombatManager IsInProgress false", TestRuntimeReflectionEncounterDoesNotOverrideCombatManagerFalse, failures);
Run("runtime reflection screen resolution prefers overlay screens", TestRuntimeReflectionScreenResolution, failures);
Run("companion scene normalizer prefers main-menu over hidden character-select markers", TestCompanionSceneNormalizerMainMenuPriority, failures);
Run("companion scene normalizer detects blocking overlay from placeholder choices", TestCompanionSceneNormalizerBlockingOverlay, failures);
Run("companion state mapper prefers main-menu over hidden character-select markers", TestCompanionStateMapperMainMenuPriority, failures);
Run("companion state mapper preserves visible and flow scene split", TestCompanionStateMapperVisibleAndFlowSceneSplit, failures);
Run("live export tracker preserves high-value state across partial observations", TestLiveExportTrackerPartialMerge, failures);
Run("live export tracker accepts authoritative combat encounter on high-value screen", TestLiveExportTrackerAcceptsAuthoritativeCombatEncounter, failures);
Run("collector mode records screen episodes and choice diagnostics", TestLiveExportTrackerCollectorMode, failures);
Run("live export tracker keeps authoritative existing-run menu-to-combat transitions", TestLiveExportTrackerMenuToCombatExistingRunAuthority, failures);
Run("live export tracker accepts direct character-select branch without submenu", TestLiveExportTrackerMenuToCombatDirectBranchAuthority, failures);
Run("harness path resolver exposes trace queue path", TestHarnessPathResolver, failures);
Run("bridge action executor round-trips action results through the queue", TestBridgeActionExecutorRoundTrip, failures);
Run("companion path resolver keeps per-run artifacts under companion root", TestCompanionPathResolver, failures);
Run("knowledge catalog service builds a bounded relevant slice", TestKnowledgeCatalogService, failures);
Run("advice prompt builder emits the required prompt sections", TestAdvicePromptBuilder, failures);
Run("host wrappers converge on foundation prompt and knowledge services", TestHostFoundationConvergence, failures);
Run("companion host keeps advice flow while diagnostics stay optional", TestCompanionHostAdviceFlow, failures);
Run("codex cli trace parser extracts thread id from json events", TestCodexCliTraceParser, failures);

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
    Assert(layout.RawObservationsPath.EndsWith("raw-observations.ndjson", StringComparison.OrdinalIgnoreCase), "Expected collector raw observations path.");
    Assert(layout.ScreenTransitionsPath.EndsWith("screen-transitions.ndjson", StringComparison.OrdinalIgnoreCase), "Expected collector screen transitions path.");
    Assert(layout.ChoiceCandidatesPath.EndsWith("choice-candidates.ndjson", StringComparison.OrdinalIgnoreCase), "Expected collector choice candidates path.");
    Assert(layout.ChoiceDecisionsPath.EndsWith("choice-decisions.ndjson", StringComparison.OrdinalIgnoreCase), "Expected collector choice decisions path.");
    Assert(layout.SemanticSnapshotsRoot.EndsWith("semantic-snapshots", StringComparison.OrdinalIgnoreCase), "Expected semantic snapshots directory path.");
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
        Path.Combine(modsRoot, ScaffoldConfiguration.CreateLocalDefault().AiCompanionMod.RuntimeConfigFileName),
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

        Assert(File.Exists(Path.Combine(result.PackageRoot, configuration.AiCompanionMod.RuntimeConfigFileName)), "Native staging package should include the AI companion runtime config file.");
        Assert(File.Exists(Path.Combine(result.PackageRoot, "sts2-mod-ai-companion.dll")), "Native staging package should include the pck-matching managed payload.");
        Assert(File.Exists(Path.Combine(result.PackageRoot, "sts2-mod-ai-companion.json")), "Native staging package should include the loose loader manifest required by the current game loader.");
        Assert(!result.Files.Any(file => file.RelativePath.Contains("loader-sentinel", StringComparison.OrdinalIgnoreCase)), "Native staging package should not include startup sentinel runtime evidence.");
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
        var configJson = File.ReadAllText(Path.Combine(result.PackageRoot, configuration.AiCompanionMod.RuntimeConfigFileName));
        var loaderManifestJson = File.ReadAllText(Path.Combine(result.PackageRoot, "packaged-template.json"));
        var runtimeConfig = System.Text.Json.JsonSerializer.Deserialize<AiCompanionRuntimeConfig>(configJson, ConfigurationLoader.JsonOptions);
        using var loaderManifest = System.Text.Json.JsonDocument.Parse(loaderManifestJson);

        Assert(runtimeConfig is not null, "Expected packaged runtime config to deserialize.");
        Assert(runtimeConfig!.Enabled, "Expected packaged runtime config to enable the AI companion payload.");
        Assert(runtimeConfig.GamePaths.SteamAccountId == configuration.GamePaths.SteamAccountId, "Expected runtime config to carry the game paths used for live export.");
        Assert(runtimeConfig.LiveExport.RelativeLiveRoot == configuration.LiveExport.RelativeLiveRoot, "Expected runtime config to carry live export settings.");
        Assert(File.Exists(Path.Combine(result.PackageRoot, "packaged-template.dll")), "Expected packaged dll to follow the configured pck basename.");
        Assert(loaderManifest.RootElement.GetProperty("id").GetString() == "packaged-template", "Expected the loose loader manifest to use the pck basename as the loader mod id.");
        Assert(loaderManifest.RootElement.GetProperty("has_pck").GetBoolean(), "Expected the loose loader manifest to declare the packaged PCK.");
        Assert(loaderManifest.RootElement.GetProperty("has_dll").GetBoolean(), "Expected the loose loader manifest to declare the packaged DLL.");
        Assert(!Directory.Exists(Path.Combine(result.PackageRoot, "ai_companion", "startup")), "Native staging package should not emit startup sentinel files into the managed mods payload.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestLiveExportTrackerMenuToCombatExistingRunAuthority()
{
    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live");

    var mainMenu = CreateObservation("main-menu", "main-menu", 0, 0, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()) with
    {
        Choices = new[]
        {
            new LiveExportChoiceSummary("choice", "Singleplayer", "singleplayer", null),
            new LiveExportChoiceSummary("choice", "Settings", "settings", null),
        },
    };
    Assert(tracker.Apply(mainMenu).Snapshot.CurrentScreen == "main-menu", "Expected main-menu hook to establish main-menu.");

    var startupPoll = CreateObservation("runtime-poll", "startup", 0, 0, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
    Assert(tracker.Apply(startupPoll).Snapshot.CurrentScreen == "main-menu", "Expected runtime poll not to displace main-menu.");

    var submenu = CreateObservation("singleplayer-submenu", "singleplayer-submenu", 0, 0, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()) with
    {
        Choices = new[]
        {
            new LiveExportChoiceSummary("choice", "Standard", "standard", null),
            new LiveExportChoiceSummary("choice", "Custom", "custom", null),
        },
    };
    Assert(tracker.Apply(submenu).Snapshot.CurrentScreen == "singleplayer-submenu", "Expected submenu hook to establish singleplayer-submenu.");

    var openCharacterSelect = CreateObservation("open-character-select", "singleplayer-submenu", 0, 0, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
    Assert(tracker.Apply(openCharacterSelect).Snapshot.CurrentScreen == "singleplayer-submenu", "Expected open-character-select to keep singleplayer-submenu until character-select is opened.");

    var characterSelect = CreateObservation("character-select", "character-select", 0, 0, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()) with
    {
        Choices = new[]
        {
            new LiveExportChoiceSummary("choice", "Ironclad", "ironclad", null),
            new LiveExportChoiceSummary("choice", "Embark", "embark", null),
        },
    };
    Assert(tracker.Apply(characterSelect).Snapshot.CurrentScreen == "character-select", "Expected character-select hook to keep character-select.");

    var feedbackPoll = CreateObservation("runtime-poll", "feedback-overlay", 0, 0, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
    Assert(tracker.Apply(feedbackPoll).Snapshot.CurrentScreen == "character-select", "Expected transient overlay poll not to displace character-select.");

    var characterSelected = CreateObservation("character-selected", "character-select", 0, 0, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()) with
    {
        Choices = new[]
        {
            new LiveExportChoiceSummary("choice", "Embark", "embark", null),
        },
    };
    Assert(tracker.Apply(characterSelected).Snapshot.CurrentScreen == "character-select", "Expected character-selected to remain on character-select.");

    var map = CreateObservation("map", "map", 1, 0, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
    Assert(tracker.Apply(map).Snapshot.CurrentScreen == "map", "Expected map hook to establish map.");

    var mapPointSelected = CreateObservation("map-point-selected", "map", 1, 0, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
    Assert(tracker.Apply(mapPointSelected).Snapshot.CurrentScreen == "map", "Expected map-point-selected to remain on map.");

    var combatStarted = CreateObservation("combat-started", "combat", 1, 1, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
    Assert(tracker.Apply(combatStarted).Snapshot.CurrentScreen == "combat", "Expected combat-started to establish combat.");
}

static void TestLiveExportTrackerMenuToCombatDirectBranchAuthority()
{
    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live");

    var mainMenu = CreateObservation("main-menu", "main-menu", 0, 0, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()) with
    {
        Choices = new[]
        {
            new LiveExportChoiceSummary("choice", "Singleplayer", "singleplayer", null),
            new LiveExportChoiceSummary("choice", "Settings", "settings", null),
        },
    };
    Assert(tracker.Apply(mainMenu).Snapshot.CurrentScreen == "main-menu", "Expected main-menu hook to establish main-menu.");

    var singleplayerPressed = CreateObservation("singleplayer-button-pressed", "main-menu", 0, 0, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
    Assert(tracker.Apply(singleplayerPressed).Snapshot.CurrentScreen == "main-menu", "Expected singleplayer-button-pressed to remain on main-menu until character-select appears.");

    var characterSelect = CreateObservation("character-select", "character-select", 0, 0, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()) with
    {
        Choices = new[]
        {
            new LiveExportChoiceSummary("choice", "Ironclad", "ironclad", null),
            new LiveExportChoiceSummary("choice", "Embark", "embark", null),
        },
    };
    Assert(tracker.Apply(characterSelect).Snapshot.CurrentScreen == "character-select", "Expected direct branch to allow character-select without submenu.");

    var map = CreateObservation("map", "map", 1, 0, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
    Assert(tracker.Apply(map).Snapshot.CurrentScreen == "map", "Expected map hook to establish map.");

    var combatStarted = CreateObservation("combat-started", "combat", 1, 1, 72, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
    Assert(tracker.Apply(combatStarted).Snapshot.CurrentScreen == "combat", "Expected combat-started to establish combat.");
}

static void TestHarnessPathResolver()
{
    var configuration = ScaffoldConfiguration.CreateLocalDefault();
    var layout = HarnessPathResolver.Resolve(configuration.GamePaths, configuration.Harness);

    Assert(layout.ActionsPath.EndsWith(Path.Combine("inbox", configuration.Harness.ActionsFileName), StringComparison.OrdinalIgnoreCase), "Expected harness actions queue path.");
    Assert(layout.ResultsPath.EndsWith(Path.Combine("outbox", configuration.Harness.ResultsFileName), StringComparison.OrdinalIgnoreCase), "Expected harness results queue path.");
    Assert(layout.TracePath.EndsWith(Path.Combine("outbox", "trace.ndjson"), StringComparison.OrdinalIgnoreCase), "Expected harness trace queue path.");
}

static void TestBridgeActionExecutorRoundTrip()
{
    var root = CreateTempDirectory();
    try
    {
        var layout = new HarnessQueueLayout(
            Path.Combine(root, "profile"),
            Path.Combine(root, "harness"),
            Path.Combine(root, "harness", "inbox"),
            Path.Combine(root, "harness", "inbox", "actions.ndjson"),
            Path.Combine(root, "harness", "outbox"),
            Path.Combine(root, "harness", "outbox", "results.ndjson"),
            Path.Combine(root, "harness", "status.json"),
            Path.Combine(root, "harness", "outbox", "trace.ndjson"),
            Path.Combine(root, "harness", "arm.json"),
            Path.Combine(root, "harness", "outbox", "inventory.latest.json"));
        HarnessPathResolver.EnsureDirectories(layout);

        var executor = new BridgeActionExecutor(layout);
        var action = Sts2AiCompanion.Foundation.Contracts.HarnessAction.Create("noop", "self-test") with
        {
            TimeoutMs = 3_000,
        };
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var resultTask = executor.ExecuteAsync(action, Sts2AiCompanion.Foundation.Contracts.CompanionState.CreateUnknown(), cancellationSource.Token);

        var deadline = DateTimeOffset.UtcNow.AddSeconds(2);
        while (!File.Exists(layout.ActionsPath) && DateTimeOffset.UtcNow < deadline)
        {
            Thread.Sleep(50);
        }

        Assert(File.Exists(layout.ActionsPath), "Expected bridge executor to append an action to the inbox queue.");

        var result = new Sts2AiCompanion.Foundation.Contracts.HarnessActionResult(
            action.ActionId,
            "ok",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            null,
            false,
            "noop",
            new[] { layout.ResultsPath });
        File.AppendAllText(layout.ResultsPath, System.Text.Json.JsonSerializer.Serialize(result) + Environment.NewLine);

        var resolved = resultTask.GetAwaiter().GetResult();
        Assert(string.Equals(resolved.Status, "ok", StringComparison.OrdinalIgnoreCase), "Expected bridge executor to resolve the queued result.");
        Assert(string.Equals(resolved.ActionId, action.ActionId, StringComparison.Ordinal), "Expected bridge executor to match the action id.");
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

static void TestStrictDomainKnowledgeScanner()
{
    var root = CreateTempDirectory();
    try
    {
        var modelsRoot = Path.Combine(root, "MegaCrit", "Sts2", "Core", "Models");
        Directory.CreateDirectory(Path.Combine(modelsRoot, "Cards"));
        Directory.CreateDirectory(Path.Combine(modelsRoot, "CardPools"));
        Directory.CreateDirectory(Path.Combine(modelsRoot, "Relics"));
        Directory.CreateDirectory(Path.Combine(modelsRoot, "RelicPools"));
        Directory.CreateDirectory(Path.Combine(modelsRoot, "Potions"));
        Directory.CreateDirectory(Path.Combine(modelsRoot, "Events"));
        Directory.CreateDirectory(Path.Combine(root, "MegaCrit", "Sts2", "Core", "Rewards"));
        Directory.CreateDirectory(Path.Combine(root, "MegaCrit", "Sts2", "Core", "Entities", "Merchant"));
        Directory.CreateDirectory(Path.Combine(root, "MegaCrit", "Sts2", "Core", "Rooms"));
        Directory.CreateDirectory(Path.Combine(root, "MegaCrit", "Sts2", "Core", "Models", "Powers"));
        Directory.CreateDirectory(Path.Combine(root, "MegaCrit", "Sts2", "Core", "MonsterMoves", "Intents"));
        Directory.CreateDirectory(Path.Combine(root, "MegaCrit", "Sts2", "Core", "Entities", "Cards"));

        File.WriteAllText(
            Path.Combine(modelsRoot, "Cards", "Bash.cs"),
            """
            namespace MegaCrit.Sts2.Core.Models.Cards;
            public sealed class Bash : CardModel
            {
                protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[2]
                {
                    new DamageVar(8m, ValueProp.Move),
                    new PowerVar<VulnerablePower>(2m)
                };

                public Bash()
                    : base(2, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
                {
                }

                protected override void OnUpgrade()
                {
                    base.DynamicVars.Damage.UpgradeValueBy(2m);
                    base.DynamicVars.Vulnerable.UpgradeValueBy(1m);
                }
            }
            """);
        File.WriteAllText(
            Path.Combine(modelsRoot, "CardPools", "IroncladCardPool.cs"),
            """
            namespace MegaCrit.Sts2.Core.Models.CardPools;
            public sealed class IroncladCardPool : CardPoolModel
            {
                public override string Title => "ironclad";

                protected override CardModel[] GenerateAllCards()
                {
                    return new CardModel[]
                    {
                        ModelDb.Card<Bash>()
                    };
                }
            }
            """);
        File.WriteAllText(
            Path.Combine(modelsRoot, "Relics", "Anchor.cs"),
            """
            namespace MegaCrit.Sts2.Core.Models.Relics;
            public sealed class Anchor : RelicModel
            {
                public override RelicRarity Rarity => RelicRarity.Common;
            }
            """);
        File.WriteAllText(
            Path.Combine(modelsRoot, "RelicPools", "IroncladRelicPool.cs"),
            """
            namespace MegaCrit.Sts2.Core.Models.RelicPools;
            public sealed class IroncladRelicPool : RelicPoolModel
            {
                public override string Title => "ironclad";

                protected override RelicModel[] GenerateAllRelics()
                {
                    return new RelicModel[]
                    {
                        ModelDb.Relic<Anchor>()
                    };
                }
            }
            """);
        File.WriteAllText(
            Path.Combine(modelsRoot, "Potions", "SwiftPotion.cs"),
            """
            namespace MegaCrit.Sts2.Core.Models.Potions;
            public sealed class SwiftPotion : PotionModel
            {
                public override PotionRarity Rarity => PotionRarity.Common;
                public override PotionUsage Usage => PotionUsage.CombatOnly;
                public override TargetType TargetType => TargetType.AnyPlayer;
                protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new CardsVar(3) };
            }
            """);
        File.WriteAllText(
            Path.Combine(modelsRoot, "Events", "AbyssalBaths.cs"),
            """
            namespace MegaCrit.Sts2.Core.Models.Events;
            public sealed class AbyssalBaths : EventModel
            {
                protected override IReadOnlyList<EventOption> GenerateInitialOptions()
                {
                    return new EventOption[]
                    {
                        new EventOption(this, Immerse, "ABYSSAL_BATHS.pages.INITIAL.options.IMMERSE"),
                        new EventOption(this, Abstain, "ABYSSAL_BATHS.pages.INITIAL.options.ABSTAIN")
                    };
                }

                private Task Immerse()
                {
                    SetEventState(L10NLookup("ABYSSAL_BATHS.pages.IMMERSE.description"), new EventOption[]
                    {
                        new EventOption(this, Abstain, "ABYSSAL_BATHS.pages.ALL.options.EXIT_BATHS")
                    });
                    return Task.CompletedTask;
                }

                private Task Abstain() => Task.CompletedTask;
            }
            """);
        File.WriteAllText(
            Path.Combine(root, "MegaCrit", "Sts2", "Core", "Entities", "Merchant", "MerchantCardRemovalEntry.cs"),
            """
            namespace MegaCrit.Sts2.Core.Entities.Merchant;
            public sealed class MerchantCardRemovalEntry : MerchantEntry
            {
                public override bool IsStocked => true;

                public override void CalcCost()
                {
                    _cost = 75 + 25 * _player.ExtraFields.CardShopRemovalsUsed;
                }
            }
            """);
        File.WriteAllText(
            Path.Combine(root, "MegaCrit", "Sts2", "Core", "Rooms", "MerchantRoom.cs"),
            """
            namespace MegaCrit.Sts2.Core.Rooms;
            public class MerchantRoom : AbstractRoom
            {
                public static MerchantDialogueSet Dialogue
                {
                    get
                    {
                        LocTable table = LocManager.Instance.GetTable("merchant_room");
                        IReadOnlyList<LocString> locStringsWithPrefix = table.GetLocStringsWithPrefix("MERCHANT.talk.");
                        return MerchantDialogueSet.CreateFromLocStrings(locStringsWithPrefix);
                    }
                }
            }
            """);
        File.WriteAllText(
            Path.Combine(root, "MegaCrit", "Sts2", "Core", "Rewards", "CardReward.cs"),
            """
            namespace MegaCrit.Sts2.Core.Rewards;
            public class CardReward : Reward
            {
                protected override RewardType RewardType => RewardType.Card;
                public override int RewardsSetIndex => 5;
                public override LocString Description => new LocString("gameplay_ui", "COMBAT_REWARD_ADD_CARD");
            }
            """);
        File.WriteAllText(
            Path.Combine(root, "MegaCrit", "Sts2", "Core", "Rewards", "LinkedRewardSet.cs"),
            """
            namespace MegaCrit.Sts2.Core.Rewards;
            public class LinkedRewardSet
            {
            }
            """);
        File.WriteAllText(
            Path.Combine(root, "MegaCrit", "Sts2", "Core", "Models", "Powers", "HelloWorldPower.cs"),
            """
            namespace MegaCrit.Sts2.Core.Models.Powers;
            public sealed class HelloWorldPower : PowerModel
            {
                public override PowerType Type => PowerType.Buff;
                public override PowerStackType StackType => PowerStackType.Counter;
            }
            """);
        File.WriteAllText(
            Path.Combine(root, "MegaCrit", "Sts2", "Core", "MonsterMoves", "Intents", "BuffIntent.cs"),
            """
            namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;
            public class BuffIntent : AbstractIntent
            {
                public override IntentType IntentType => IntentType.Buff;
            }
            """);
        File.WriteAllText(
            Path.Combine(root, "MegaCrit", "Sts2", "Core", "Entities", "Cards", "CardKeyword.cs"),
            """
            namespace MegaCrit.Sts2.Core.Entities.Cards;
            public enum CardKeyword
            {
                None,
                Exhaust,
                Ethereal
            }
            """);

        var rawPckCatalog = new StaticKnowledgeCatalog(
            DateTimeOffset.UtcNow,
            new StaticKnowledgeMetadata("v-test", "commit", DateTimeOffset.UtcNow, new Dictionary<string, string?>()),
            new[]
            {
                CreateResourceEntry("cards", "res://images/packed/card_portraits/ironclad/bash.png"),
            },
            new[]
            {
                CreateResourceEntry("relics", "res://images/relics/anchor.png"),
            },
            new[]
            {
                CreateResourceEntry("potions", "res://images/potions/swift_potion.png"),
            },
            new[]
            {
                CreateResourceEntry("events", "res://events/abyssal_baths.tscn"),
            },
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>());

        var metadata = new StaticKnowledgeMetadata("v-test", "commit", DateTimeOffset.UtcNow, new Dictionary<string, string?>());
        var catalog = StrictDomainKnowledgeScanner.Scan(root, rawPckCatalog, metadata, out var warnings);

        Assert(warnings.Count == 0, $"Strict parser should not emit warnings for a complete synthetic decompile root. Received: {string.Join(" | ", warnings)}");
        var bash = catalog.Cards.Single(entry => entry.Attributes.TryGetValue("className", out var value) && value == "Bash");
        Assert(ReadAttribute(bash, "cost") == "2", "Expected card cost parsed from the CardModel base constructor.");
        Assert(ReadAttribute(bash, "type") == "Attack", "Expected card type parsed from the CardModel base constructor.");
        Assert(ReadAttribute(bash, "rarity") == "Basic", "Expected card rarity parsed from the CardModel base constructor.");
        Assert(ReadAttribute(bash, "target") == "AnyEnemy", "Expected card target parsed from the CardModel base constructor.");
        Assert(ReadAttribute(bash, "pool") == "ironclad", "Expected card pool parsed from CardPools.");
        Assert(ReadAttribute(bash, "var.Damage") == "8", "Expected strict parser to capture canonical dynamic vars.");
        Assert(ReadAttribute(bash, "upgradeSummary") == "Damage+2, Vulnerable+1", "Expected strict parser to capture upgrade hints.");

        var anchor = catalog.Relics.Single(entry => entry.Attributes.TryGetValue("className", out var value) && value == "Anchor");
        Assert(ReadAttribute(anchor, "rarity") == "Common", "Expected relic rarity parsed from the model class.");
        Assert(ReadAttribute(anchor, "pool") == "ironclad", "Expected relic pool parsed from RelicPools.");

        var potion = catalog.Potions.Single(entry => entry.Attributes.TryGetValue("className", out var value) && value == "SwiftPotion");
        Assert(ReadAttribute(potion, "usage") == "CombatOnly", "Expected potion usage parsed from the model class.");
        Assert(ReadAttribute(potion, "target") == "AnyPlayer", "Expected potion target parsed from the model class.");
        Assert(ReadAttribute(potion, "var.Cards") == "3", "Expected potion canonical vars parsed.");

        var abyssalBaths = catalog.Events.Single(entry => entry.Attributes.TryGetValue("className", out var value) && value == "AbyssalBaths");
        Assert(ReadAttribute(abyssalBaths, "optionKeyCount") == "3", "Expected event option keys parsed from EventOption constructor calls.");
        Assert(ReadAttribute(abyssalBaths, "pageKeyCount") == "1", "Expected event page keys parsed from L10NLookup usage.");

        var merchantCardRemoval = catalog.Shops.Single(entry => entry.Attributes.TryGetValue("className", out var value) && value == "MerchantCardRemovalEntry");
        Assert(ReadAttribute(merchantCardRemoval, "shopKind") == "merchant-card-removal-service", "Expected strict shop parser to classify merchant card removal.");
        Assert(ReadAttribute(merchantCardRemoval, "l10nKey") == "MERCHANT", "Expected strict shop parser to attach the merchant service L10N key.");

        var merchantRoom = catalog.Shops.Single(entry => entry.Attributes.TryGetValue("className", out var value) && value == "MerchantRoom");
        Assert(ReadAttribute(merchantRoom, "roomType") == "Shop", "Expected strict shop parser to classify MerchantRoom as a shop room.");

        var cardReward = catalog.Rewards.Single(entry => entry.Attributes.TryGetValue("className", out var value) && value == "CardReward");
        Assert(ReadAttribute(cardReward, "rewardType") == "Card", "Expected strict reward parser to capture reward type.");
        Assert(ReadAttribute(cardReward, "l10nKey") == "COMBAT_REWARD_ADD_CARD", "Expected strict reward parser to capture reward description L10N key.");

        var linkedRewardSet = catalog.Rewards.Single(entry => entry.Attributes.TryGetValue("className", out var value) && value == "LinkedRewardSet");
        Assert(ReadAttribute(linkedRewardSet, "l10nKey") == "LINKED_REWARDS", "Expected strict reward parser to keep linked reward set semantics.");

        var helloWorldPower = catalog.Keywords.Single(entry => entry.Attributes.TryGetValue("className", out var value) && value == "HelloWorldPower");
        Assert(ReadAttribute(helloWorldPower, "keywordKind") == "power", "Expected strict keyword parser to classify powers.");
        Assert(ReadAttribute(helloWorldPower, "l10nKey") == "HELLO_WORLD_POWER", "Expected strict keyword parser to seed power localization keys.");

        var buffIntent = catalog.Keywords.Single(entry => entry.Attributes.TryGetValue("className", out var value) && value == "BuffIntent");
        Assert(ReadAttribute(buffIntent, "keywordKind") == "intent", "Expected strict keyword parser to classify intents.");
        Assert(ReadAttribute(buffIntent, "intentType") == "Buff", "Expected strict keyword parser to capture intent type.");

        var exhaustKeyword = catalog.Keywords.Single(entry => entry.Attributes.TryGetValue("className", out var value) && value == "Exhaust");
        Assert(ReadAttribute(exhaustKeyword, "keywordKind") == "card-keyword", "Expected strict keyword parser to include enum-backed card keywords.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestLocalizationKnowledgeScanner()
{
    var root = CreateTempDirectory();
    try
    {
        var path = Path.Combine(root, "synthetic.pck");
        var payload = """
        localization/eng/cards.json
        "ACROBATICS.title": "Acrobatics",
        "ACROBATICS.description": "Draw 3 cards.",
        localization/kor/cards.json
        "ACROBATICS.title": "곡예",
        "ACROBATICS.description": "카드를 3장 뽑습니다.",
        localization/kor/card_library.json
        "ACROBATICS.selectionScreenPrompt": "카드를 고르세요."
        """;
        File.WriteAllText(path, payload, new UTF8Encoding(false));

        var seedCatalog = new StaticKnowledgeCatalog(
            DateTimeOffset.UtcNow,
            new StaticKnowledgeMetadata("v-test", "commit", DateTimeOffset.UtcNow, new Dictionary<string, string?>()),
            new[]
            {
                new StaticKnowledgeEntry(
                    "megacrit-sts2-core-models-cards-acrobatics",
                    "Acrobatics",
                    "assembly-scan",
                    false,
                    null,
                    new[] { "type-candidate" },
                    new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["fullName"] = "MegaCrit.Sts2.Core.Models.Cards.Acrobatics",
                    },
                    Array.Empty<StaticKnowledgeOption>()),
            },
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>());
        var scan = LocalizationKnowledgeScanner.Scan(path, seedCatalog, out var warnings);

        Assert(warnings.Count == 0, "Synthetic localization scan should not emit warnings.");
        Assert(scan.Cards.Count == 1, $"Expected one localized card but received {scan.Cards.Count}.");
        Assert(scan.Cards[0].PreferredLocale == "kor", "Expected Korean localization to win when available.");
        Assert(scan.Cards[0].Title == "곡예", "Expected Korean card title.");
        Assert(scan.Cards[0].Description == "카드를 3장 뽑습니다.", "Expected Korean card description.");
        Assert(scan.Cards[0].SelectionScreenPrompt == "카드를 고르세요.", "Expected selection screen prompt.");
        Assert(scan.Cards[0].EnglishTitle == "Acrobatics", "Expected English fallback title to be preserved.");
        Assert(scan.Cards[0].EnglishDescription == "Draw 3 cards.", "Expected English fallback description to be preserved.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestLocalizationKnowledgeMerge()
{
    var metadata = new StaticKnowledgeMetadata("v-test", "commit", DateTimeOffset.UtcNow, new Dictionary<string, string?>());
    var baseline = new StaticKnowledgeCatalog(
        DateTimeOffset.UtcNow,
        metadata,
        new[]
        {
            new StaticKnowledgeEntry(
                "megacrit-sts2-core-models-cards-acrobatics",
                "Acrobatics",
                "assembly-scan",
                false,
                "MegaCrit.Sts2.Core.Models.Cards.Acrobatics",
                new[] { "type-candidate" },
                new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["fullName"] = "MegaCrit.Sts2.Core.Models.Cards.Acrobatics",
                    ["resourcePath"] = "res://src/Core/Models/Cards/Acrobatics.cs",
                    ["strictDomain"] = "card",
                    ["strictModel"] = "true",
                    ["var.Cards"] = "3",
                },
                Array.Empty<StaticKnowledgeOption>()),
        },
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>());

    var scan = new StaticKnowledgeLocalizationScan(
        DateTimeOffset.UtcNow,
        "synthetic",
        new[]
        {
            new StaticKnowledgeLocalizationCardEntry(
                "ACROBATICS",
                "kor",
                "곡예",
                "카드를 {Cards:diff()}장 뽑습니다.",
                "카드를 고르세요.",
                "Acrobatics",
                "Draw {Cards:diff()} cards.",
                new[] { "localization/kor/cards.json", "localization/eng/cards.json" },
                new[] { "eng", "kor" }),
            new StaticKnowledgeLocalizationCardEntry(
                "OPTION_HEAL",
                "kor",
                "휴식",
                "최대 체력의 30%만큼 회복합니다.",
                null,
                "Rest",
                "Heal 30% of your max HP.",
                new[] { "localization/kor/cards.json", "localization/eng/cards.json" },
                new[] { "eng", "kor" }),
        },
        Array.Empty<StaticKnowledgeLocalizationEntry>(),
        Array.Empty<StaticKnowledgeLocalizationEntry>(),
        Array.Empty<StaticKnowledgeLocalizationEntry>(),
        Array.Empty<StaticKnowledgeLocalizationEntry>(),
        Array.Empty<StaticKnowledgeLocalizationEntry>(),
        Array.Empty<StaticKnowledgeLocalizationEntry>(),
        new Dictionary<string, string?>(),
        Array.Empty<string>());

    var merged = StaticKnowledgeCatalogBuilder.MergeCardLocalization(baseline, scan, metadata);
    var card = merged.Cards.Single();

    Assert(card.Name == "곡예", $"Expected localization merge to replace the display title but received '{card.Name}'.");
    Assert(card.Source == "localization-scan", "Expected localization merge to promote the card source.");
    Assert(card.Attributes.TryGetValue("description", out var description) && description == "카드를 3장 뽑습니다.", $"Expected localized description attribute to resolve dynamic vars but received '{description}'.");
    Assert(card.Attributes.TryGetValue("selectionScreenPrompt", out var selectionPrompt) && selectionPrompt == "카드를 고르세요.", "Expected localized prompt attribute.");
    Assert(card.Attributes.TryGetValue("englishTitle", out var englishTitle) && englishTitle == "Acrobatics", "Expected English fallback title.");
    Assert(merged.Cards.Count == 1, "Expected strict card localization merge to skip unmatched OPTION_* entries.");
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

static void TestRuntimeReflectionStringExtraction()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod("TryReadString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, new[] { typeof(object), typeof(string[]) }, null);
    Assert(method is not null, "Expected private TryReadString helper.");

    var button = new FakeChoiceButton
    {
        Label = new FakeLabel { Text = "보상 선택" },
        Description = new FakeLocString("카드를 선택하세요."),
        Id = "reward-choice",
    };

    var label = method!.Invoke(null, new object?[] { button, new[] { "Label", "Id" } }) as string;
    var description = method.Invoke(null, new object?[] { button, new[] { "Description" } }) as string;

    Assert(label == "보상 선택", "Expected nested label text to resolve.");
    Assert(description == "카드를 선택하세요.", "Expected LocString.GetFormattedText() to be used for descriptions.");
}

static void TestRuntimeReflectionRejectsOverlayLikePlayerRoots()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod("IsAuthoritativePlayerRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(method is not null, "Expected private IsAuthoritativePlayerRoot helper.");

    var playerRoot = method!.Invoke(null, new object?[] { new FakePlayerEntity() });
    var overlayRoot = method.Invoke(null, new object?[] { new FakeOverlayPlayerContainer() });

    Assert(playerRoot is bool accepted && accepted, "Expected plain player entity root to be accepted.");
    Assert(overlayRoot is bool rejected && !rejected, "Expected overlay-like player container to be rejected.");
}

static void TestRuntimeReflectionExtractDeckFromCombatState()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod("ExtractDeck", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(method is not null, "Expected private ExtractDeck helper.");

    var roots = new object[]
    {
        new FakeCombatPlayerRoot
        {
            PlayerCombatState = new FakePlayerCombatState
            {
                Hand = new object[]
                {
                    new FakeCombatCard { Name = "Strike", CardId = "STRIKE", Cost = 1, Type = "attack" },
                    new FakeCombatCard { Name = "Defend", CardId = "DEFEND", Cost = 1, Type = "skill" },
                },
                DrawPile = new object[]
                {
                    new FakeCombatCard { Name = "Bash", CardId = "BASH", Cost = 2, Type = "attack" },
                },
            },
        },
    };

    var cards = method!.Invoke(null, new object?[] { roots, 16 }) as IReadOnlyList<LiveExportCardSummary>;
    Assert(cards is not null, "Expected ExtractDeck to return a card list.");
    Assert(cards!.Count == 3, $"Expected three combat cards, got {cards.Count}.");
    Assert(cards.Any(card => card.Name == "Strike"), "Expected Strike to be present.");
    Assert(cards.Any(card => card.Name == "Bash"), "Expected Bash to be present.");
}

static void TestRuntimeReflectionEncounterPrefersCombatManagerIsInProgress()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod(
        "ExtractEncounter",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
        null,
        new[] { typeof(IEnumerable<object>), typeof(IDictionary<string, string?>) },
        null);
    Assert(method is not null, "Expected private ExtractEncounter helper.");

    var meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    var roots = new object[]
    {
        new FakeCombatManagerState { IsInProgress = true, IsPlayPhase = true },
        new FakeEncounterState { Name = "Cultist", Type = "Monster", InCombat = false, IsInCombat = false, Turn = 2 },
        new FakeCombatUiNode(),
    };

    var summary = method!.Invoke(null, new object?[] { roots, meta }) as LiveExportEncounterSummary;
    Assert(summary is not null, "Expected ExtractEncounter to return a summary.");
    Assert(summary!.InCombat == true, "Expected CombatManager.IsInProgress=true to establish inCombat=true.");
    Assert(meta.TryGetValue("combatPrimarySource", out var primarySource) && primarySource == "CombatManager.IsInProgress", "Expected meta to record the primary combat source.");
    Assert(meta.TryGetValue("combatPrimaryValue", out var primaryValue) && primaryValue == "true", "Expected meta to record the primary combat value.");
}

static void TestRuntimeReflectionEncounterDoesNotOverrideCombatManagerFalse()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod(
        "ExtractEncounter",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
        null,
        new[] { typeof(IEnumerable<object>), typeof(IDictionary<string, string?>) },
        null);
    Assert(method is not null, "Expected private ExtractEncounter helper.");

    var meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    var roots = new object[]
    {
        new FakeCombatManagerState { IsInProgress = false, IsEnding = true },
        new FakeEncounterState { Name = "Cultist", Type = "Monster", InCombat = true, IsInCombat = true, Turn = 2 },
        new FakeCombatUiNode(),
    };

    var summary = method!.Invoke(null, new object?[] { roots, meta }) as LiveExportEncounterSummary;
    Assert(summary is not null, "Expected ExtractEncounter to return a summary.");
    Assert(summary!.InCombat == false, "Expected CombatManager.IsInProgress=false to win over conflicting fallback signals.");
    Assert(meta.TryGetValue("combatPrimaryValue", out var primaryValue) && primaryValue == "false", "Expected meta to record the false primary combat value.");
}

static void TestRuntimeReflectionScreenResolution()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod("ResolveScreen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(method is not null, "Expected private ResolveScreen helper.");

    var roots = new List<object> { new FakeScreenState { RoomType = "CombatRoom" } };
    var resolved = method!.Invoke(null, new object?[] { null, roots, new[] { "MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NCardRewardSelectionScreen" } }) as string;

    Assert(resolved == "rewards", "Expected overlay reward screen to win over room-type combat fallback.");
}

static void TestLiveExportTrackerPartialMerge()
{
    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live");
    var seed = new LiveExportObservation(
        "choice-list-presented",
        DateTimeOffset.UtcNow.AddSeconds(-1),
        "run-001",
        "active",
        "rest-site",
        1,
        12,
        new LiveExportPlayerSummary("Ironclad", 55, 80, 123, 3, new Dictionary<string, string?>()),
        new[]
        {
            new LiveExportCardSummary("Strike", "CARD.STRIKE", 1, "Attack", false),
        },
        new[] { "RELIC.BURNING_BLOOD" },
        new[] { "POTION.STRENGTH_POTION" },
        new[]
        {
            new LiveExportChoiceSummary("choice", "휴식", "rest", null),
            new LiveExportChoiceSummary("choice", "강화", "smith", null),
        },
        Array.Empty<string>(),
        new LiveExportEncounterSummary("Rest Site", "RestSite", false, null),
        new Dictionary<string, object?>(),
        new Dictionary<string, string?>());
    var first = tracker.Apply(seed).Snapshot;

    var partialPoll = new LiveExportObservation(
        "runtime-poll",
        DateTimeOffset.UtcNow,
        "run-001",
        null,
        "combat",
        null,
        null,
        new LiveExportPlayerSummary(null, null, null, null, null, new Dictionary<string, string?>()),
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<LiveExportChoiceSummary>(),
        new[]
        {
            "state extraction is partial; core run data was not resolved from live objects.",
            "no visible choices resolved for this observation.",
        },
        new LiveExportEncounterSummary(null, null, true, null),
        new Dictionary<string, object?>(),
        new Dictionary<string, string?>());
    var second = tracker.Apply(partialPoll).Snapshot;

    Assert(second.CurrentScreen == "rest-site", "Expected high-value rest-site screen to survive immediate runtime-poll fallback.");
    Assert(second.Player.CurrentHp == first.Player.CurrentHp && second.Player.Gold == first.Player.Gold, "Expected partial player observation not to clear resolved hp/gold.");
    Assert(second.Relics.SequenceEqual(first.Relics, StringComparer.Ordinal), "Expected partial runtime poll not to clear relics.");
    Assert(second.Potions.SequenceEqual(first.Potions, StringComparer.Ordinal), "Expected partial runtime poll not to clear potions.");
    Assert(second.CurrentChoices.Select(choice => choice.Label).SequenceEqual(first.CurrentChoices.Select(choice => choice.Label), StringComparer.Ordinal), "Expected unresolved choice poll not to clear visible choices.");
}

static void TestLiveExportTrackerAcceptsAuthoritativeCombatEncounter()
{
    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live");

    var mapObservation = CreateObservation("map", "map", 1, 1, 80, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()) with
    {
        Encounter = new LiveExportEncounterSummary("Shrinker Beetle", "Monster", false, null),
    };
    var first = tracker.Apply(mapObservation).Snapshot;
    Assert(first.Encounter?.InCombat == false, "Expected initial map encounter to be non-combat.");

    var combatObservation = CreateObservation("runtime-poll", "combat", 1, 1, 80, 99, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()) with
    {
        Encounter = new LiveExportEncounterSummary("Shrinker Beetle", "Monster", true, 1),
        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["combatPrimarySource"] = "CombatManager.IsInProgress",
            ["combatPrimaryValue"] = "true",
        },
    };

    var second = tracker.Apply(combatObservation).Snapshot;
    Assert(second.CurrentScreen == "combat", "Expected authoritative combat observation to set combat screen.");
    Assert(second.Encounter?.InCombat == true, "Expected authoritative combat encounter to replace previous false inCombat value.");
}

static void TestCompanionStateMapperMainMenuPriority()
{
    var snapshot = new LiveExportSnapshot(
        "pending-test",
        "idle",
        1,
        DateTimeOffset.UtcNow,
        "main-menu",
        null,
        null,
        LiveExportPlayerSummary.Empty,
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        new[]
        {
            new LiveExportChoiceSummary("choice", "싱글플레이", null, "싱글플레이"),
            new LiveExportChoiceSummary("choice", "멀티플레이", null, "멀티플레이"),
        },
        new[] { "trigger: runtime-poll" },
        Array.Empty<string>(),
        new LiveExportEncounterSummary("root", null, false, null),
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["currentSceneType"] = "MegaCrit.Sts2.Core.Nodes.NGame",
            ["rootTypeSummary"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NSingleplayerSubmenu",
        });

    var state = CompanionStateMapper.FromLiveExport(snapshot, session: null, Array.Empty<LiveExportEventEnvelope>());

    Assert(state.Scene.SceneType == "main-menu", $"Expected main-menu scene, got {state.Scene.SceneType}.");
}

static void TestCompanionSceneNormalizerMainMenuPriority()
{
    var snapshot = new LiveExportSnapshot(
        "pending-test",
        "idle",
        1,
        DateTimeOffset.UtcNow,
        "main-menu",
        null,
        null,
        LiveExportPlayerSummary.Empty,
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        new[]
        {
            new LiveExportChoiceSummary("choice", "Singleplayer", null, "Singleplayer"),
            new LiveExportChoiceSummary("choice", "Multiplayer", null, "Multiplayer"),
        },
        new[] { "trigger: runtime-poll" },
        Array.Empty<string>(),
        new LiveExportEncounterSummary("root", null, false, null),
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["currentSceneType"] = "MegaCrit.Sts2.Core.Nodes.NGame",
            ["rootTypeSummary"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen",
        });

    var scene = CompanionSceneNormalizer.Normalize(snapshot);

    Assert(scene.SceneType == "main-menu", $"Expected main-menu scene, got {scene.SceneType}.");
}

static void TestCompanionSceneNormalizerBlockingOverlay()
{
    var snapshot = new LiveExportSnapshot(
        "pending-test",
        "idle",
        1,
        DateTimeOffset.UtcNow,
        "main-menu",
        null,
        null,
        LiveExportPlayerSummary.Empty,
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        new[]
        {
            new LiveExportChoiceSummary("choice", "Dismisser", null, null),
        },
        new[] { "trigger: runtime-poll" },
        Array.Empty<string>(),
        new LiveExportEncounterSummary("root", null, false, null),
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["currentSceneType"] = "MegaCrit.Sts2.Core.Nodes.NGame",
            ["rootTypeSummary"] = "MegaCrit.Sts2.Core.Nodes.Multiplayer.NMultiplayerTimeoutOverlay",
        });

    var scene = CompanionSceneNormalizer.Normalize(snapshot);

    Assert(scene.SceneType == "blocking-overlay", $"Expected blocking-overlay scene, got {scene.SceneType}.");
}

static void TestCompanionStateMapperVisibleAndFlowSceneSplit()
{
    var snapshot = new LiveExportSnapshot(
        "pending-test",
        "active",
        1,
        DateTimeOffset.UtcNow,
        "rewards",
        null,
        null,
        LiveExportPlayerSummary.Empty,
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        new[]
        {
            new LiveExportChoiceSummary("choice", "넘기기", null, "넘기기"),
        },
        new[] { "trigger: runtime-poll" },
        Array.Empty<string>(),
        new LiveExportEncounterSummary("root", null, false, null),
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["visibleScreen"] = "map",
            ["flowScreen"] = "rewards",
            ["currentSceneType"] = "MegaCrit.Sts2.Core.Nodes.NGame",
            ["rootTypeSummary"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
        });

    var state = CompanionStateMapper.FromLiveExport(snapshot, session: null, Array.Empty<LiveExportEventEnvelope>());

    Assert(state.Scene.SceneType == "rewards", $"Expected logical rewards scene, got {state.Scene.SceneType}.");
    Assert(state.Scene.VisibleSceneType == "map", $"Expected visible map scene, got {state.Scene.VisibleSceneType}.");
    Assert(state.Scene.FlowSceneType == "rewards", $"Expected flow rewards scene, got {state.Scene.FlowSceneType}.");
}

static void TestLiveExportTrackerCollectorMode()
{
    var tracker = new LiveExportStateTracker(new LiveExportStateTrackerOptions(16, 40, 10, true), @"C:\temp\live");
    var semanticObservation = new LiveExportObservation(
        "reward-screen-opened",
        DateTimeOffset.UtcNow.AddSeconds(-1),
        "run-collector",
        "active",
        "rewards",
        1,
        20,
        new LiveExportPlayerSummary("Ironclad", 48, 80, 92, 3, new Dictionary<string, string?>()),
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        new[]
        {
            new LiveExportChoiceSummary("card", "강타", "BASH", "적에게 피해를 주고 취약을 겁니다."),
            new LiveExportChoiceSummary("card", "수비", "DEFEND_R", "방어도를 얻습니다."),
        },
        Array.Empty<string>(),
        new LiveExportEncounterSummary("Combat Reward", "Reward", false, null),
        new Dictionary<string, object?>(),
        new Dictionary<string, string?>())
    {
        SemanticScreen = "rewards",
        ChoiceCandidates = new[]
        {
            new LiveExportChoiceCandidate("reward._options", "RewardOption", "강타", "BASH", "적에게 피해를 주고 취약을 겁니다.", 100, true, null),
            new LiveExportChoiceCandidate("generic.nodes", "BackButton", "BackButton", "BackButton", null, 0, false, "placeholder-label"),
        },
        ChoiceDecision = new LiveExportChoiceDecision(
            "reward._options",
            true,
            2,
            1,
            "accepted-strict",
            null,
            new[] { "BackButton" }),
    };

    var firstBatch = tracker.Apply(semanticObservation);

    Assert(firstBatch.CollectorStatus is not null && firstBatch.CollectorStatus.CollectorModeEnabled, "Expected collector mode status for collector-enabled tracker.");
    var firstCollectorStatus = firstBatch.CollectorStatus!;
    Assert(firstCollectorStatus.ActiveScreenEpisode == "rewards", "Expected semantic reward screen to start an active episode.");
    Assert(firstCollectorStatus.LastSemanticScreen == "rewards", "Expected collector status to remember the latest semantic screen.");
    Assert(firstCollectorStatus.LastAcceptedExtractorPath == "reward._options", "Expected collector status to remember the accepted extractor path.");
    Assert(firstCollectorStatus.ChoiceExtractionStatus == "resolved (1)", "Expected collector status to report accepted choice count.");
    Assert(firstBatch.ScreenTransitions.Count == 1 && firstBatch.ScreenTransitions[0].After == "rewards", "Expected collector mode to emit a screen transition for the semantic screen.");

    var fallbackPoll = new LiveExportObservation(
        "runtime-poll",
        DateTimeOffset.UtcNow,
        "run-collector",
        "active",
        "combat",
        null,
        null,
        new LiveExportPlayerSummary(null, null, null, null, null, new Dictionary<string, string?>()),
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<LiveExportChoiceSummary>(),
        new[] { "no visible choices resolved for this observation." },
        new LiveExportEncounterSummary("Battle", "Combat", true, 1),
        new Dictionary<string, object?>(),
        new Dictionary<string, string?>())
    {
        ChoiceDecision = new LiveExportChoiceDecision(
            null,
            false,
            1,
            0,
            "fallback-missing",
            "no visible choices resolved for this observation.",
            new[] { "BackButton" }),
    };

    var secondBatch = tracker.Apply(fallbackPoll);

    Assert(secondBatch.Snapshot.CurrentScreen == "rewards", "Expected collector merge to keep the reward episode active across a fallback runtime poll.");
    Assert(secondBatch.ScreenTransitions.Count == 1 && secondBatch.ScreenTransitions[0].KeptPreviousScreen, "Expected collector transition log to mark when a previous semantic screen is kept.");
    Assert(secondBatch.CollectorStatus is not null && secondBatch.CollectorStatus.ActiveScreenEpisode == "rewards", "Expected active screen episode to remain on rewards during fallback polls.");
    var secondCollectorStatus = secondBatch.CollectorStatus!;
    Assert(secondCollectorStatus.LastAcceptedExtractorPath == "reward._options", "Expected last accepted extractor path to persist across fallback polls.");
    Assert(secondCollectorStatus.LastDegradedReason == "no visible choices resolved for this observation.", "Expected degraded reason to reflect the latest failed extraction.");
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
    Assert(paths.AdviceRoot == Path.Combine(paths.RunRoot!, "advice"), "Expected advice directory under the run root.");
    Assert(paths.CodexTracePath == Path.Combine(paths.RunRoot!, "codex-trace.ndjson"), "Expected codex trace path under the run root.");
    Assert(paths.CollectorSummaryPath == Path.Combine(paths.RunRoot!, "collector-summary.json"), "Expected collector summary path under the run root.");
    Assert(paths.HostStatusPath == Path.Combine(paths.RunRoot!, "host-status.json"), "Expected host status to live under the per-run root when run id is present.");
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

    Assert(prompt.Contains("당신은 Slay the Spire 2 조언 어시스턴트입니다.", StringComparison.Ordinal), "Expected prompt preamble.");
    Assert(prompt.Contains("current_state_summary:", StringComparison.Ordinal), "Expected prompt to include the state summary section.");
    Assert(prompt.Contains("knowledge_slice:", StringComparison.Ordinal), "Expected prompt to include the knowledge slice section.");
    Assert(prompt.Contains("response_instructions:", StringComparison.Ordinal), "Expected prompt to include the response instructions section.");
}

static void TestHostFoundationConvergence()
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
        var knowledgeRoot = Path.Combine(root, "artifacts", "knowledge");
        Directory.CreateDirectory(knowledgeRoot);

        var catalog = new StaticKnowledgeCatalog(
            DateTimeOffset.UtcNow,
            new StaticKnowledgeMetadata(null, null, null, new Dictionary<string, string?>()),
            new[]
            {
                new StaticKnowledgeEntry(
                    "pommel-strike",
                    "Pommel Strike",
                    "observed-merge",
                    true,
                    "Deal damage and draw a card.",
                    new[] { "card" },
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
            Array.Empty<StaticKnowledgeEntry>(),
            new[]
            {
                new StaticKnowledgeEntry(
                    "merchant-shop",
                    "Merchant Shop",
                    "strict-domain-scan",
                    true,
                    "Shop screen context.",
                    new[] { "shop" },
                    new Dictionary<string, string?>(),
                    Array.Empty<StaticKnowledgeOption>()),
            },
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<StaticKnowledgeEntry>());
        File.WriteAllText(
            Path.Combine(knowledgeRoot, "catalog.latest.json"),
            JsonSerializer.Serialize(catalog, ConfigurationLoader.JsonOptions));

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
        var batch = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), Path.Combine(root, "live")).Apply(observation);
        var hostRunState = new CompanionRunState(batch.Snapshot, null, "summary", batch.Events, false)
        {
            NormalizedState = CompanionStateMapper.FromLiveExport(batch.Snapshot, null, batch.Events),
        };
        var hostKnowledgeService = new KnowledgeCatalogService(configuration, root);
        var foundationKnowledgeService = new FoundationKnowledgeCatalogService(configuration, root);
        var hostSlice = hostKnowledgeService.BuildSlice(hostRunState, 8, 4096);
        var foundationSlice = foundationKnowledgeService.BuildSlice(ToFoundationRunState(hostRunState), 8, 4096);

        Assert(hostSlice.Entries.Select(entry => entry.Id).SequenceEqual(foundationSlice.Entries.Select(entry => entry.Id), StringComparer.OrdinalIgnoreCase), "Expected host knowledge wrapper to match foundation slice selection.");
        Assert(hostSlice.Reasons.SequenceEqual(foundationSlice.Reasons, StringComparer.OrdinalIgnoreCase), "Expected host knowledge wrapper to preserve foundation reasons.");

        var hostBuilder = new AdvicePromptBuilder(configuration);
        var foundationBuilder = new FoundationAdvicePromptBuilder(configuration);
        var trigger = new AdviceTrigger("choice-list-presented", DateTimeOffset.UtcNow, false, true, "new-choice", batch.Events.Last());
        var hostPack = hostBuilder.BuildInputPack(hostRunState, trigger, hostSlice);
        var foundationPack = foundationBuilder.BuildInputPack(ToFoundationRunState(hostRunState), ToFoundationTrigger(trigger), foundationSlice);

        Assert(hostPack.KnowledgeEntries.Select(entry => entry.Id).SequenceEqual(foundationPack.KnowledgeEntries.Select(entry => entry.Id), StringComparer.OrdinalIgnoreCase), "Expected host prompt builder wrapper to preserve foundation knowledge entries.");
        Assert(string.Equals(hostBuilder.FormatPrompt(hostPack), foundationBuilder.FormatPrompt(foundationPack), StringComparison.Ordinal), "Expected host prompt builder wrapper to format the same prompt as the foundation builder.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestCompanionHostAdviceFlow()
{
    ExecuteCompanionHostAdviceFlowTest(collectorModeEnabled: false);
    ExecuteCompanionHostAdviceFlowTest(collectorModeEnabled: true);
}

static void TestCodexCliTraceParser()
{
    const string trace = """
    {"type":"thread.started","thread_id":"019cdcfc-aefb-76c1-92e7-d36d9d89d3cb"}
    {"type":"turn.started"}
    {"type":"item.completed","item":{"id":"item_0","type":"agent_message","text":"{\"headline\":\"h\",\"summary\":\"s\",\"recommendedAction\":\"a\",\"recommendedChoiceLabel\":null,\"reasoningBullets\":[],\"riskNotes\":[],\"confidence\":null,\"knowledgeRefs\":[]}"}} 
    {"type":"turn.completed"}
    """;

    var (threadId, lastAgentMessageJson) = FoundationCodexCliClient.ParseExecTrace(trace);

    Assert(threadId == "019cdcfc-aefb-76c1-92e7-d36d9d89d3cb", "Expected thread id to be extracted from JSONL events.");
    Assert(!string.IsNullOrWhiteSpace(lastAgentMessageJson) && lastAgentMessageJson.Contains("\"headline\":\"h\"", StringComparison.Ordinal), "Expected final agent message JSON to be captured from item.completed events.");
}

static void ExecuteCompanionHostAdviceFlowTest(bool collectorModeEnabled)
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateCompanionHostTestConfiguration(root, collectorModeEnabled);
        var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        Directory.CreateDirectory(layout.LiveRoot);
        var runId = collectorModeEnabled ? "collector-on-run" : "collector-off-run";
        var snapshot = CreateHostSnapshot(runId, "shop");
        var session = new LiveExportSession("session-001", runId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 1, layout.LiveRoot, "choice-list-presented", "shop");
        var eventPayload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["screenEpisode"] = "shop",
        };
        var events = new[]
        {
            new LiveExportEventEnvelope(DateTimeOffset.UtcNow, 1, runId, "choice-list-presented", "shop", 1, 3, eventPayload),
        };

        WriteJson(layout.SnapshotPath, snapshot);
        WriteJson(layout.SessionPath, session);
        File.WriteAllText(layout.SummaryPath, "shop summary", Encoding.UTF8);
        Directory.CreateDirectory(Path.GetDirectoryName(layout.EventsPath)!);
        File.WriteAllText(layout.EventsPath, string.Join(Environment.NewLine, events.Select(SerializeNdjson)) + Environment.NewLine, Encoding.UTF8);
        File.WriteAllText(layout.RawObservationsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ScreenTransitionsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceCandidatesPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceDecisionsPath, string.Empty, Encoding.UTF8);
        Directory.CreateDirectory(layout.SemanticSnapshotsRoot);

        SeedKnowledgeCatalog(root);
        var fakeClient = new FakeCodexSessionClient();
        var host = new CompanionHost(configuration, root, fakeClient);
        try
        {
            host.RefreshAsync().GetAwaiter().GetResult();
            Assert(
                fakeClient.RequestCount == 1,
                $"Expected refresh to trigger automatic advice when a semantic event is present. state={host.CurrentSnapshot.Status.State} message={host.CurrentSnapshot.Status.Message}");
            Assert(host.CurrentSnapshot.LatestAdvice is not null && host.CurrentSnapshot.LatestAdvice.TriggerKind == "choice-list-presented", "Expected automatic advice to be published after refresh.");
            Assert(host.CurrentSnapshot.RunState?.NormalizedState.Scene.SceneType == "shop", "Expected host run state to carry normalized foundation scene state.");

            var manualRequested = host.RequestManualAdviceAsync().GetAwaiter().GetResult();
            Assert(manualRequested, "Expected manual advice request to succeed after refresh.");
            Assert(fakeClient.RequestCount == 2, "Expected manual advice to invoke the codex client.");

            var retryRequested = host.RequestRetryLastAdviceAsync().GetAwaiter().GetResult();
            Assert(retryRequested, "Expected retry-last advice request to succeed after a prior advice run.");
            Assert(fakeClient.RequestCount == 3, "Expected retry-last advice to invoke the codex client.");

            var runRoot = host.CurrentSnapshot.Paths.RunRoot;
            Assert(!string.IsNullOrWhiteSpace(runRoot) && Directory.Exists(runRoot!), "Expected companion host to materialize a per-run artifact root.");
            Assert(File.Exists(host.CurrentSnapshot.Paths.AdviceLatestJsonPath!), "Expected advice.latest.json to be written.");
            Assert(File.Exists(host.CurrentSnapshot.Paths.AdviceLatestMarkdownPath!), "Expected advice.latest.md to be written.");
            Assert(File.Exists(host.CurrentSnapshot.Paths.AdviceLogPath!), "Expected advice.ndjson to be written.");

            if (collectorModeEnabled)
            {
                Assert(host.CurrentSnapshot.CollectorStatus is { Enabled: true }, "Expected collector mode on to keep collector status enabled.");
                Assert(File.Exists(host.CurrentSnapshot.Paths.CollectorSummaryPath!), "Expected collector summary to be written when collector mode is enabled.");
            }
            else
            {
                Assert(host.CurrentSnapshot.CollectorStatus is { Enabled: false }, "Expected collector mode off to keep collector diagnostics disabled.");
                Assert(host.CurrentSnapshot.Paths.CollectorSummaryPath is not null && !File.Exists(host.CurrentSnapshot.Paths.CollectorSummaryPath), "Expected collector summary not to be written when collector mode is disabled.");
            }
        }
        finally
        {
            host.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static ScaffoldConfiguration CreateCompanionHostTestConfiguration(string root, bool collectorModeEnabled)
{
    var baseConfiguration = ScaffoldConfiguration.CreateLocalDefault();
    return baseConfiguration with
    {
        GamePaths = baseConfiguration.GamePaths with
        {
            GameDirectory = Path.Combine(root, "game"),
            UserDataRoot = Path.Combine(root, "userdata"),
            ArtifactsRoot = Path.Combine(root, "artifacts"),
            SteamAccountId = "test-account",
        },
        LiveExport = baseConfiguration.LiveExport with
        {
            CollectorModeEnabled = collectorModeEnabled,
            RelativeLiveRoot = "ai_companion/live",
        },
        Assistant = baseConfiguration.Assistant with
        {
            AutoAdviceEnabled = true,
        },
    };
}

static LiveExportSnapshot CreateHostSnapshot(string runId, string screen)
{
    return new LiveExportSnapshot(
        runId,
        "active",
        1,
        DateTimeOffset.UtcNow,
        screen,
        1,
        3,
        new LiveExportPlayerSummary("Ironclad", 70, 80, 120, 3, new Dictionary<string, string?>()),
        new[]
        {
            new LiveExportCardSummary("Pommel Strike", "pommel-strike", 1, "Attack", false),
        },
        new[] { "Anchor" },
        Array.Empty<string>(),
        new[]
        {
            new LiveExportChoiceSummary("card", "Pommel Strike", "pommel-strike", "Card for sale."),
        },
        Array.Empty<string>(),
        Array.Empty<string>(),
        null,
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["choice-source"] = "live-export",
            ["choice-extractor"] = "shop._items",
            ["currentSceneType"] = "NMerchantRoom",
            ["rootTypeSummary"] = "NMerchantRoom",
            ["flowScreen"] = screen,
            ["visibleScreen"] = screen,
        });
}

static void SeedKnowledgeCatalog(string root)
{
    var knowledgeRoot = Path.Combine(root, "artifacts", "knowledge");
    Directory.CreateDirectory(knowledgeRoot);
    var catalog = new StaticKnowledgeCatalog(
        DateTimeOffset.UtcNow,
        new StaticKnowledgeMetadata(null, null, null, new Dictionary<string, string?>()),
        new[]
        {
            new StaticKnowledgeEntry(
                "pommel-strike",
                "Pommel Strike",
                "observed-merge",
                true,
                "Deal damage and draw a card.",
                new[] { "card" },
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
        Array.Empty<StaticKnowledgeEntry>(),
        new[]
        {
            new StaticKnowledgeEntry(
                "merchant-shop",
                "Merchant Shop",
                "strict-domain-scan",
                true,
                "Shop screen context.",
                new[] { "shop" },
                new Dictionary<string, string?>(),
                Array.Empty<StaticKnowledgeOption>()),
        },
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>());
    File.WriteAllText(Path.Combine(knowledgeRoot, "catalog.latest.json"), JsonSerializer.Serialize(catalog, ConfigurationLoader.JsonOptions), Encoding.UTF8);
}

static Sts2AiCompanion.Foundation.Contracts.CompanionRunState ToFoundationRunState(CompanionRunState runState)
{
    return new Sts2AiCompanion.Foundation.Contracts.CompanionRunState(
        runState.Snapshot,
        runState.Session,
        runState.SummaryText,
        runState.RecentEvents.ToArray(),
        runState.IsStale)
    {
        NormalizedState = runState.NormalizedState,
    };
}

static Sts2AiCompanion.Foundation.Contracts.AdviceTrigger ToFoundationTrigger(AdviceTrigger trigger)
{
    return new Sts2AiCompanion.Foundation.Contracts.AdviceTrigger(
        trigger.Kind,
        trigger.RequestedAt,
        trigger.Manual,
        trigger.BypassMinInterval,
        trigger.Reason,
        trigger.SourceEvent,
        trigger.RetrySourcePromptPackPath);
}

static void WriteJson<T>(string path, T value)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, JsonSerializer.Serialize(value, ConfigurationLoader.JsonOptions), Encoding.UTF8);
}

static string SerializeNdjson<T>(T value)
{
    return JsonSerializer.Serialize(
        value,
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        });
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static string? ReadAttribute(StaticKnowledgeEntry entry, string key)
{
    return entry.Attributes.TryGetValue(key, out var value)
        ? value
        : null;
}

static StaticKnowledgeEntry CreateResourceEntry(string domain, string resourcePath)
{
    return new StaticKnowledgeEntry(
        resourcePath,
        Path.GetFileNameWithoutExtension(resourcePath),
        "pck-inventory",
        false,
        resourcePath,
        new[] { domain },
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["resourcePath"] = resourcePath,
        },
        Array.Empty<StaticKnowledgeOption>());
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

file sealed class FakeChoiceButton
{
    public object? Label { get; init; }

    public object? Description { get; init; }

    public string? Id { get; init; }
}

file sealed class FakeLabel
{
    public string? Text { get; init; }
}

file sealed class FakeLocString
{
    private readonly string text;

    public FakeLocString(string text)
    {
        this.text = text;
    }

    public string GetFormattedText()
    {
        return text;
    }
}

file sealed class FakeScreenState
{
    public string? RoomType { get; init; }
}

file sealed class FakePlayerEntity
{
    public string Name { get; init; } = "Ironclad";
}

file sealed class FakeOverlayPlayerContainer
{
    public string Name { get; init; } = "MultiplayerTimeoutOverlay";
}

file sealed class FakeCombatPlayerRoot
{
    public object? PlayerCombatState { get; init; }
}

file sealed class FakePlayerCombatState
{
    public object[] Hand { get; init; } = Array.Empty<object>();

    public object[] DrawPile { get; init; } = Array.Empty<object>();
}

file sealed class FakeCombatCard
{
    public string? Name { get; init; }

    public string? CardId { get; init; }

    public int Cost { get; init; }

    public string? Type { get; init; }
}

file sealed class FakeCombatManagerState
{
    public bool IsInProgress { get; init; }

    public bool IsEnding { get; init; }

    public bool IsPlayPhase { get; init; }

    public bool IsEnemyTurnStarted { get; init; }
}

file sealed class FakeEncounterState
{
    public string? Name { get; init; }

    public string? Type { get; init; }

    public bool InCombat { get; init; }

    public bool IsInCombat { get; init; }

    public int? Turn { get; init; }
}

file sealed class FakeCombatUiNode
{
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
