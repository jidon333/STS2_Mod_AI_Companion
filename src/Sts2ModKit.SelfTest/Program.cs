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
using Sts2ModAiCompanion.HarnessBridge;
using Sts2ModAiCompanion.Mod;
using Sts2ModAiCompanion.Mod.Runtime;
using FoundationKnowledgeCatalogService = Sts2AiCompanion.Foundation.Knowledge.KnowledgeCatalogService;
using FoundationAdvicePromptBuilder = Sts2AiCompanion.Foundation.Reasoning.AdvicePromptBuilder;
using FoundationCodexCliClient = Sts2AiCompanion.Foundation.Reasoning.CodexCliClient;
using FoundationReplayAdvisorValidator = Sts2AiCompanion.Foundation.Replay.ReplayAdvisorValidator;
using RewardAssessmentFactsBuilder = Sts2AiCompanion.Foundation.Reasoning.RewardAssessmentFactsBuilder;
using RewardOptionSetBuilder = Sts2AiCompanion.Foundation.Reasoning.RewardOptionSetBuilder;

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
Run("runtime reflection keeps reward-backed card rows and reward type metadata", TestRuntimeReflectionRewardBackedCardChoiceExtraction, failures);
Run("runtime reflection exports transform card-selection subtype metadata", TestRuntimeReflectionTransformCardSelectionExport, failures);
Run("runtime reflection keeps reward-pick separate from confirm-driven card selection", TestRuntimeReflectionRewardPickCardSelectionExport, failures);
Run("runtime reflection exports deck-remove card-selection preview semantics", TestRuntimeReflectionDeckRemoveCardSelectionExport, failures);
Run("runtime reflection exports treasure room chest holder and proceed semantics", TestRuntimeReflectionTreasureRoomExport, failures);
Run("runtime reflection exports explicit shop room semantics and typed shop choices", TestRuntimeReflectionShopExport, failures);
Run("runtime reflection exports reward foreground ownership and teardown semantics", TestRuntimeReflectionRewardOwnershipExport, failures);
Run("runtime reflection exports map-node from mixed rest-site aftermath authority", TestRuntimeReflectionMixedRestAftermathMapExport, failures);
Run("runtime reflection reports filtered mixed-aftermath map-point diagnostics", TestRuntimeReflectionMixedRestAftermathMapDiagnostics, failures);
Run("runtime reflection exports explicit ancient dialogue advance before Neow options", TestRuntimeReflectionAncientEventDialogueExport, failures);
Run("runtime reflection exports explicit ancient option buttons and suppresses pseudo-choice duplicates", TestRuntimeReflectionAncientEventOptionExport, failures);
Run("runtime reflection marks ancient post-choice completion buttons explicitly", TestRuntimeReflectionAncientEventCompletionExport, failures);
Run("runtime reflection exports generic event proceed semantics from EventOption.IsProceed", TestRuntimeReflectionGenericEventProceedExport, failures);
Run("runtime reflection normalizes ancient mixed post-proceed ownership to map lane", TestRuntimeReflectionAncientMixedPostProceedOwnershipNormalization, failures);
Run("runtime reflection keeps map owner when post-proceed map surface is pending", TestRuntimeReflectionAncientMixedPostProceedMapPending, failures);
Run("inventory publisher preserves strict map-node source contract", TestInventoryPublisherMapNodeSourceCorrection, failures);
Run("tracker and inventory preserve raw and compatibility screen provenance", TestTrackerAndInventoryPreserveScreenProvenance, failures);
Run("inventory publisher prefers explicit compatibility scene provenance", TestInventoryPublisherPrefersCompatibilitySceneProvenance, failures);
Run("runtime reflection rejects overlay-like player roots", TestRuntimeReflectionRejectsOverlayLikePlayerRoots, failures);
Run("runtime reflection extracts combat cards from player combat state", TestRuntimeReflectionExtractDeckFromCombatState, failures);
Run("runtime reflection encounter prefers CombatManager IsInProgress", TestRuntimeReflectionEncounterPrefersCombatManagerIsInProgress, failures);
Run("runtime reflection encounter does not override CombatManager IsInProgress false", TestRuntimeReflectionEncounterDoesNotOverrideCombatManagerFalse, failures);
Run("runtime reflection screen resolution prefers overlay screens", TestRuntimeReflectionScreenResolution, failures);
Run("runtime reflection exports combat play state metadata", TestRuntimeReflectionCombatMetadataExport, failures);
Run("runtime reflection capture clears combat slot and success inference", TestRuntimeReflectionCaptureClearsCombatMetadata, failures);
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
Run("reward deterministic builders are reproducible", TestRewardDeterministicBuilders, failures);
Run("reward deterministic layer stays off for non-card rewards", TestRewardNonCardFallback, failures);
Run("reward deterministic layer falls back outside reward scenes", TestRewardDeterministicFallback, failures);
Run("reward live and replay paths share deterministic context", TestRewardLiveReplayDeterministicContext, failures);
Run("reward finalizer only normalizes labels and attaches trace", TestRewardFinalizerMinimalNormalization, failures);
Run("reward model rationale survives without heavy backfill", TestRewardModelRationaleArtifacts, failures);
Run("reward fixtures include a non-first winning choice", TestRewardNonFirstChoiceScenario, failures);
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

static void TestRuntimeReflectionRewardBackedCardChoiceExtraction()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var summaryMethod = extractorType!.GetMethod("TryCreateChoiceSummary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    var placeholderMethod = extractorType.GetMethod("LooksLikePlaceholderChoice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(summaryMethod is not null, "Expected private TryCreateChoiceSummary helper.");
    Assert(placeholderMethod is not null, "Expected private LooksLikePlaceholderChoice helper.");

    var rewardButton = new FakeRewardButton
    {
        Reward = new FakeCardReward
        {
            Description = new FakeLocString("덱에 추가할 카드를 선택하세요."),
        },
        _label = new FakeLabel { Text = "덱에 추가할 카드를 선택하세요." },
        Position = new FakeVector2(758, 278),
        Size = new FakeVector2(402, 86),
    };

    var summary = summaryMethod!.Invoke(null, new object?[] { rewardButton }) as LiveExportChoiceSummary;
    Assert(summary is not null, "Expected reward-backed card row to produce a choice summary.");
    Assert(summary!.Kind == "card", "Expected CardReward rows to export as card choices.");
    Assert(summary.Label == "덱에 추가할 카드를 선택하세요.", "Expected CardReward placeholder label to be preserved.");
    Assert(summary.BindingKind == "reward-type" && summary.BindingId == "CardReward", "Expected reward type binding metadata for CardReward rows.");
    Assert(summary.SemanticHints.Contains("reward-card", StringComparer.OrdinalIgnoreCase), "Expected reward card semantic hint on CardReward rows.");
    Assert(summary.ScreenBounds == "758,278,402,86", "Expected reward row bounds to be preserved.");

    var placeholder = placeholderMethod!.Invoke(null, new object?[] { summary, 5 });
    Assert(placeholder is bool keep && !keep, "Expected reward-backed CardReward rows not to be discarded as placeholders.");
}

static void TestRuntimeReflectionTransformCardSelectionExport()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod("ObserveCardSelection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(method is not null, "Expected private ObserveCardSelection helper.");

    var transformScreen = new FakeNDeckTransformSelectScreen
    {
        Visible = true,
        _prefs = new FakeCardSelectorPrefs(new FakeLocString("변화시킬 카드를 2장 선택하세요."), 2, 2, false, true),
        _selectedCards = new object[]
        {
            new FakeCardModel { Id = "CARD.DEFEND_IRONCLAD", Name = "수비" },
        },
        _confirmButton = new FakeClickableControl { Visible = true, Enabled = false, Position = new FakeVector2(1700, 720), Size = new FakeVector2(180, 110) },
        _previewContainer = new FakeContainer { Visible = false },
        _previewConfirmButton = new FakeClickableControl { Visible = false, Enabled = false, Position = new FakeVector2(1700, 720), Size = new FakeVector2(180, 110) },
        _grid = new FakeGrid
        {
            CurrentlyDisplayedCardHolders = new object[]
            {
                new FakeGridCardHolder(new FakeCardModel { Id = "CARD.STRIKE_IRONCLAD", Name = "타격" }, 460, 300),
                new FakeGridCardHolder(new FakeCardModel { Id = "CARD.DEFEND_IRONCLAD", Name = "수비" }, 760, 300),
            },
        },
    };

    var observation = method!.Invoke(null, new object?[] { new object[] { transformScreen }, "transform" });
    Assert(observation is not null, "Expected transform screen observation.");
    Assert(ReadProperty(observation!, "ScreenType") as string == "transform", "Expected transform subtype.");
    Assert((bool)(ReadProperty(observation!, "ScreenVisible") ?? false), "Expected transform screen to be visible.");
    Assert((int)(ReadProperty(observation!, "SelectedCount") ?? -1) == 1, "Expected selected count to export.");
    Assert((int?)(ReadProperty(observation!, "MinSelect") ?? -1) == 2, "Expected transform min-select to export.");
    Assert((int?)(ReadProperty(observation!, "MaxSelect") ?? -1) == 2, "Expected transform max-select to export.");
    Assert((bool)(ReadProperty(observation!, "PreviewVisible") ?? true) == false, "Expected transform preview to stay closed.");
    Assert(ReadProperty(observation!, "Prompt") as string == "변화시킬 카드를 2장 선택하세요.", "Expected transform prompt to export.");
}

static void TestRuntimeReflectionRewardPickCardSelectionExport()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod("ObserveCardSelection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(method is not null, "Expected private ObserveCardSelection helper.");

    var rewardScreen = new FakeNCardRewardSelectionScreen
    {
        Visible = true,
        _banner = new FakeBanner { label = new FakeLabel { Text = "카드를 고르세요." } },
        _cardRow = new object[]
        {
            new FakeGridCardHolder(new FakeCardModel { Id = "CARD.SHRUG_IT_OFF", Name = "몸통 박치기" }, 520, 280),
            new FakeGridCardHolder(new FakeCardModel { Id = "CARD.BATTLE_TRANCE", Name = "전투 최면" }, 860, 280),
        },
    };

    var observation = method!.Invoke(null, new object?[] { new object[] { rewardScreen }, "card-choice" });
    Assert(observation is not null, "Expected reward-pick observation.");
    Assert(ReadProperty(observation!, "ScreenType") as string == "reward-pick", "Expected reward-pick subtype.");
    Assert((int)(ReadProperty(observation!, "SelectedCount") ?? -1) == 0, "Reward-pick should not export selected-count progress.");
    Assert((bool)(ReadProperty(observation!, "PreviewVisible") ?? true) == false, "Reward-pick should not export preview-visible state.");
    Assert((bool)(ReadProperty(observation!, "MainConfirmEnabled") ?? true) == false, "Reward-pick should not expose main confirm.");
    Assert((bool)(ReadProperty(observation!, "PreviewConfirmEnabled") ?? true) == false, "Reward-pick should not expose preview confirm.");
}

static void TestRuntimeReflectionDeckRemoveCardSelectionExport()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod("ObserveCardSelection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(method is not null, "Expected private ObserveCardSelection helper.");

    var removeScreen = new FakeNDeckCardSelectScreen
    {
        Visible = true,
        _prefs = new FakeCardSelectorPrefs(new FakeLocString("제거할 카드를 고르세요."), 1, 2, true, true),
        _selectedCards = new object[]
        {
            new FakeCardModel { Id = "CARD.STRIKE_IRONCLAD", Name = "타격" },
        },
        _confirmButton = new FakeClickableControl { Visible = true, Enabled = true, Position = new FakeVector2(1700, 720), Size = new FakeVector2(180, 110) },
        _previewContainer = new FakeContainer { Visible = false },
        _previewConfirmButton = new FakeClickableControl { Visible = false, Enabled = false, Position = new FakeVector2(1700, 720), Size = new FakeVector2(180, 110) },
        _grid = new FakeGrid
        {
            CurrentlyDisplayedCardHolders = new object[]
            {
                new FakeGridCardHolder(new FakeCardModel { Id = "CARD.STRIKE_IRONCLAD", Name = "타격" }, 520, 280),
                new FakeGridCardHolder(new FakeCardModel { Id = "CARD.DEFEND_IRONCLAD", Name = "수비" }, 860, 280),
            },
        },
    };

    var observation = method!.Invoke(null, new object?[] { new object[] { removeScreen }, "event" });
    Assert(observation is not null, "Expected deck-remove observation.");
    Assert(ReadProperty(observation!, "ScreenType") as string == "deck-remove", "Expected deck-remove subtype.");
    Assert((bool)(ReadProperty(observation!, "MainConfirmEnabled") ?? false), "Expected deck-remove main confirm to export.");
    Assert((bool)(ReadProperty(observation!, "PreviewVisible") ?? true) == false, "Expected preview to stay closed.");
    Assert((int)(ReadProperty(observation!, "SelectedCount") ?? -1) == 1, "Expected selected-count to export for deck-remove.");
}

static void TestRuntimeReflectionTreasureRoomExport()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var observeMethod = extractorType!.GetMethod("ObserveTreasureRoom", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(observeMethod is not null, "Expected private ObserveTreasureRoom helper.");

    var treasureRoom = new FakeNTreasureRoom
    {
        Visible = true,
        Chest = new FakeTreasureButton { Visible = true, Enabled = true, Position = new FakeVector2(602, 367), Size = new FakeVector2(800, 500) },
        ProceedButton = new FakeClickableControl { Visible = true, Enabled = false, Position = new FakeVector2(1700, 760), Size = new FakeVector2(220, 110) },
        _relicCollection = new FakeNTreasureRoomRelicCollection
        {
            _holdersInUse = new object[]
            {
                new FakeTreasureRelicHolder(new FakeRelicModel { Id = "RELIC.ANCHOR", Name = "닻" }, 620, 300, enabled: true),
                new FakeTreasureRelicHolder(new FakeRelicModel { Id = "RELIC.BAG_OF_PREPARATION", Name = "가방" }, 900, 300, enabled: false),
            },
        },
    };

    var observation = observeMethod!.Invoke(null, new object?[] { new object[] { treasureRoom, treasureRoom.Chest!, treasureRoom.ProceedButton!, treasureRoom._relicCollection! }, "map" });
    Assert(observation is not null, "Expected treasure room observation.");
    Assert((bool)(ReadProperty(observation!, "RoomDetected") ?? false), "Expected treasure room detection.");
    Assert((bool)(ReadProperty(observation!, "ChestClickable") ?? false), "Expected chest clickable export.");
    Assert((bool)(ReadProperty(observation!, "ChestOpened") ?? false), "Visible treasure relic holders should imply that the chest-opened state is already active.");
    Assert((int)(ReadProperty(observation!, "RelicHolderCount") ?? -1) == 2, "Expected relic-holder count to export.");
    Assert((int)(ReadProperty(observation!, "VisibleRelicHolderCount") ?? -1) == 2, "Expected visible relic-holder count to export.");
    Assert((int)(ReadProperty(observation!, "EnabledRelicHolderCount") ?? -1) == 1, "Expected enabled relic-holder count to export.");
    Assert((bool)(ReadProperty(observation!, "ProceedEnabled") ?? true) == false, "Expected proceed to remain disabled before relic pick.");

    var staleAfterProceedRoom = new FakeNTreasureRoom
    {
        Visible = true,
        _isRelicCollectionOpen = false,
        Chest = new FakeTreasureButton { Opened = true, Visible = true, Enabled = false, Position = new FakeVector2(602, 367), Size = new FakeVector2(800, 500) },
        ProceedButton = new FakeClickableControl { Visible = true, Enabled = false, Position = new FakeVector2(1700, 760), Size = new FakeVector2(220, 110) },
        _relicCollection = new FakeNTreasureRoomRelicCollection
        {
            _holdersInUse = new object[]
            {
                new FakeTreasureRelicHolder(new FakeRelicModel { Id = "RELIC.STRIKE_DUMMY", Name = "타격용 인형" }, 620, 300, enabled: true),
            },
        },
    };
    var staleAfterProceedObservation = observeMethod.Invoke(
        null,
        new object?[]
        {
            new object[]
            {
                staleAfterProceedRoom,
                staleAfterProceedRoom.Chest!,
                staleAfterProceedRoom.ProceedButton!,
                staleAfterProceedRoom._relicCollection!,
                new FakeNMapScreen { IsOpen = true, Visible = true },
                new FakeScreenStateTracker { IsInSharedRelicPickingScreen = false },
                new FakeNRun { ScreenStateTracker = new FakeScreenStateTracker { IsInSharedRelicPickingScreen = false } },
            },
            "map",
        });
    Assert(staleAfterProceedObservation is not null, "Expected stale-after-proceed treasure observation.");
    Assert((bool)(ReadProperty(staleAfterProceedObservation!, "RoomDetected") ?? true) == false, "Map-open aftermath with no shared relic picking and no open relic collection should not keep treasure authority foreground-active.");
    Assert((int)(ReadProperty(staleAfterProceedObservation!, "EnabledRelicHolderCount") ?? -1) == 0, "Stale post-proceed holder residue should not remain enabled treasure authority.");
}

static void TestRuntimeReflectionShopExport()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var observeMethod = extractorType!.GetMethod("ObserveShopRoom", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    var createChoicesMethod = extractorType.GetMethod("CreateShopChoices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(observeMethod is not null, "Expected private ObserveShopRoom helper.");
    Assert(createChoicesMethod is not null, "Expected private CreateShopChoices helper.");

    var merchantButton = new FakeClickableControl { Visible = true, Enabled = false, Position = new FakeVector2(180, 640), Size = new FakeVector2(240, 160) };
    var proceedButton = new FakeClickableControl { Visible = true, Enabled = false, Position = new FakeVector2(1680, 760), Size = new FakeVector2(220, 110) };
    var backButton = new FakeClickableControl { Visible = true, Enabled = true, Position = new FakeVector2(140, 120), Size = new FakeVector2(160, 110) };
    var relicSlot = new FakeNMerchantRelic("게임용 말", "RELIC.GAME_PIECE", x: 420, y: 260, enabled: true, stocked: true, enoughGold: true);
    var cardSlot = new FakeNMerchantCard("몸풀기", "CARD.WARM_UP", x: 700, y: 260, enabled: true, stocked: true, enoughGold: false);
    var potionSlot = new FakeNMerchantPotion("힘 포션", "POTION.STRENGTH_POTION", x: 980, y: 260, enabled: true, stocked: true, enoughGold: true);
    var cardRemovalSlot = new FakeNMerchantCardRemoval("카드 제거 서비스", x: 1260, y: 260, enabled: true, enoughGold: true, used: false);
    var inventory = new FakeNMerchantInventory
    {
        Visible = true,
        IsOpen = true,
        _backButton = backButton,
        Slots = new object[] { relicSlot, cardSlot, potionSlot, cardRemovalSlot },
    };
    var shopRoom = new FakeNMerchantRoom
    {
        Visible = true,
        Inventory = inventory,
        MerchantButton = merchantButton,
        ProceedButton = proceedButton,
    };

    var observation = observeMethod!.Invoke(null, new object?[] { new object[] { shopRoom, inventory, merchantButton, proceedButton, backButton }, "shop" });
    Assert(observation is not null, "Expected shop observation.");
    Assert((bool)(ReadProperty(observation!, "RoomDetected") ?? false), "Expected shop room detection.");
    Assert((bool)(ReadProperty(observation!, "InventoryOpen") ?? false), "Expected shop inventory-open export.");
    Assert((bool)(ReadProperty(observation!, "BackVisible") ?? false), "Expected shop back visibility export.");
    Assert((int)(ReadProperty(observation!, "OptionCount") ?? -1) == 4, "Expected shop option count to include visible slots and card removal.");
    Assert((int)(ReadProperty(observation!, "AffordableOptionCount") ?? -1) == 3, "Expected affordable option count to include relic, potion, and card removal.");
    var affordableRelicIds = ((System.Collections.IEnumerable?)ReadProperty(observation!, "AffordableRelicIds"))?.Cast<string>().ToArray() ?? Array.Empty<string>();
    var affordablePotionIds = ((System.Collections.IEnumerable?)ReadProperty(observation!, "AffordablePotionIds"))?.Cast<string>().ToArray() ?? Array.Empty<string>();
    Assert(affordableRelicIds.Length == 1 && string.Equals(affordableRelicIds[0], "RELIC.GAME_PIECE", StringComparison.OrdinalIgnoreCase), "Expected affordable relic ids to use the actual relic model id.");
    Assert(affordablePotionIds.Length == 1 && string.Equals(affordablePotionIds[0], "POTION.STRENGTH_POTION", StringComparison.OrdinalIgnoreCase), "Expected affordable potion ids to use the actual potion model id.");
    Assert((bool)(ReadProperty(observation!, "CardRemovalVisible") ?? false), "Expected card removal visibility export.");
    Assert((bool)(ReadProperty(observation!, "CardRemovalEnabled") ?? false), "Expected card removal enabled export.");
    Assert((bool)(ReadProperty(observation!, "CardRemovalEnoughGold") ?? false), "Expected card removal enough-gold export.");
    Assert((bool)(ReadProperty(observation!, "CardRemovalUsed") ?? true) == false, "Expected unused card-removal service export.");

    var choices = createChoicesMethod!.Invoke(null, new[] { observation }) as System.Collections.IEnumerable;
    Assert(choices is not null, "Expected explicit shop choices.");
    var shopChoices = choices!.Cast<object>().ToArray();
    Assert(shopChoices.Any(choice => string.Equals((string?)ReadProperty(choice, "Kind"), "shop-option:relic", StringComparison.OrdinalIgnoreCase)), "Expected typed relic shop choice.");
    Assert(shopChoices.Any(choice => string.Equals((string?)ReadProperty(choice, "Kind"), "shop-option:potion", StringComparison.OrdinalIgnoreCase)), "Expected typed potion shop choice.");
    Assert(shopChoices.Any(choice => string.Equals((string?)ReadProperty(choice, "Kind"), "shop-card-removal", StringComparison.OrdinalIgnoreCase)), "Expected explicit card-removal shop service choice.");
    Assert(shopChoices.Any(choice => string.Equals((string?)ReadProperty(choice, "Kind"), "shop-back", StringComparison.OrdinalIgnoreCase)), "Expected explicit shop back choice.");
    Assert(shopChoices.Any(choice => string.Equals((string?)ReadProperty(choice, "Kind"), "shop-option:relic", StringComparison.OrdinalIgnoreCase)
                                     && string.Equals((string?)ReadProperty(choice, "Value"), "RELIC.GAME_PIECE", StringComparison.OrdinalIgnoreCase)
                                     && string.Equals((string?)ReadProperty(choice, "Label"), "게임용 말", StringComparison.OrdinalIgnoreCase)), "Expected relic shop choice to use actual relic identity, not a holder placeholder.");
    Assert(shopChoices.Any(choice => string.Equals((string?)ReadProperty(choice, "Kind"), "shop-option:card", StringComparison.OrdinalIgnoreCase)
                                     && string.Equals((string?)ReadProperty(choice, "Value"), "CARD.WARM_UP", StringComparison.OrdinalIgnoreCase)
                                     && string.Equals((string?)ReadProperty(choice, "Label"), "몸풀기", StringComparison.OrdinalIgnoreCase)), "Expected card shop choice to use actual card identity, not a holder placeholder.");
    Assert(shopChoices.Any(choice => string.Equals((string?)ReadProperty(choice, "Kind"), "shop-option:potion", StringComparison.OrdinalIgnoreCase)
                                     && string.Equals((string?)ReadProperty(choice, "Value"), "POTION.STRENGTH_POTION", StringComparison.OrdinalIgnoreCase)
                                     && string.Equals((string?)ReadProperty(choice, "Label"), "힘 포션", StringComparison.OrdinalIgnoreCase)), "Expected potion shop choice to use actual potion identity.");
    Assert(shopChoices.Any(choice => string.Equals((string?)ReadProperty(choice, "Kind"), "shop-card-removal", StringComparison.OrdinalIgnoreCase)
                                     && string.Equals((string?)ReadProperty(choice, "Value"), "service:card-removal", StringComparison.OrdinalIgnoreCase)), "Expected card-removal service to use explicit service identity.");

    var removeContaminationMethod = extractorType.GetMethod("RemoveShopChoiceContamination", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    var addChoiceSummaryMethod = extractorType.GetMethod("AddChoiceSummary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(removeContaminationMethod is not null, "Expected private RemoveShopChoiceContamination helper.");
    Assert(addChoiceSummaryMethod is not null, "Expected private AddChoiceSummary helper.");
    var crowdedChoices = new List<LiveExportChoiceSummary>
    {
        new("relic", "게임용 말", "RELIC.GAME_PIECE", "Generic merchant display"),
        new("relic", "소뿔모양 걸이", "RELIC.HORN_CLEAT", "Generic merchant display"),
        new("relic", "유황", "RELIC.BRIMSTONE", "Generic merchant display"),
        new("potion", "강장제", "POTION.FORTIFIER", "Generic merchant display"),
        new("potion", "힘 포션", "POTION.STRENGTH_POTION", "Generic merchant display"),
        new("potion", "신속 포션", "POTION.SWIFT_POTION", "Generic merchant display"),
    };
    removeContaminationMethod!.Invoke(null, new object?[] { crowdedChoices, observation });
    var mergedChoices = new List<LiveExportChoiceSummary>();
    foreach (var choice in shopChoices.Cast<LiveExportChoiceSummary>())
    {
        addChoiceSummaryMethod!.Invoke(null, new object?[] { mergedChoices, choice, 10 });
    }

    foreach (var choice in crowdedChoices)
    {
        addChoiceSummaryMethod.Invoke(null, new object?[] { mergedChoices, choice, 10 });
    }

    Assert(mergedChoices.Any(choice => string.Equals(choice.Kind, "shop-back", StringComparison.OrdinalIgnoreCase)), "Explicit shop back should survive the capped emitted choice set.");

    var enabledProceedButton = new FakeClickableControl { Visible = true, Enabled = true, Position = new FakeVector2(1680, 760), Size = new FakeVector2(220, 110) };
    var closedInventory = new FakeNMerchantInventory
    {
        Visible = true,
        IsOpen = false,
        _backButton = backButton,
        Slots = new object[] { relicSlot, cardSlot, potionSlot, cardRemovalSlot },
    };
    var closedShopRoom = new FakeNMerchantRoom
    {
        Visible = true,
        Inventory = closedInventory,
        MerchantButton = merchantButton,
        ProceedButton = enabledProceedButton,
    };
    var proceedObservation = observeMethod.Invoke(null, new object?[] { new object[] { closedShopRoom, closedInventory, merchantButton, enabledProceedButton, backButton }, "shop" });
    Assert(proceedObservation is not null, "Expected proceed shop observation.");
    var proceedChoices = createChoicesMethod.Invoke(null, new[] { proceedObservation }) as System.Collections.IEnumerable;
    Assert(proceedChoices is not null && proceedChoices.Cast<object>().Any(choice => string.Equals((string?)ReadProperty(choice, "Kind"), "shop-proceed", StringComparison.OrdinalIgnoreCase)), "Expected explicit shop proceed choice when inventory is closed and proceed is enabled.");

    var teardownMerchantButton = new FakeClickableControl { Visible = true, Enabled = false, Position = new FakeVector2(180, 640), Size = new FakeVector2(240, 160) };
    var teardownProceedButton = new FakeClickableControl { Visible = true, Enabled = false, Position = new FakeVector2(1680, 760), Size = new FakeVector2(220, 110) };
    var teardownInventory = new FakeNMerchantInventory
    {
        Visible = true,
        IsOpen = false,
        _backButton = backButton,
        Slots = new object[] { relicSlot, cardSlot, potionSlot, cardRemovalSlot },
    };
    var teardownShopRoom = new FakeNMerchantRoom
    {
        Visible = true,
        Inventory = teardownInventory,
        MerchantButton = teardownMerchantButton,
        ProceedButton = teardownProceedButton,
    };
    var activeScreenContext = new FakeActiveScreenContext
    {
        CurrentScreen = new FakeNMapScreen { IsOpen = true, Visible = true },
    };
    var teardownObservation = observeMethod.Invoke(null, new object?[] { new object[] { teardownShopRoom, teardownInventory, teardownMerchantButton, teardownProceedButton, backButton, activeScreenContext }, "shop" });
    Assert(teardownObservation is not null, "Expected shop teardown observation.");
    Assert((bool)(ReadProperty(teardownObservation!, "RoomVisible") ?? false), "Expected stale merchant room visibility to remain exported for diagnostics.");
    Assert((bool)(ReadProperty(teardownObservation!, "ForegroundOwned") ?? true) == false, "Expected shop foreground ownership to drop once inventory is closed, controls are disabled, and map is current.");
    Assert((bool)(ReadProperty(teardownObservation!, "TeardownInProgress") ?? false), "Expected explicit shop teardown export once proceed aftermath starts.");
    Assert((bool)(ReadProperty(teardownObservation!, "MapIsCurrentActiveScreen") ?? false), "Expected map current active screen export during shop proceed aftermath.");
    var teardownChoices = createChoicesMethod.Invoke(null, new[] { teardownObservation }) as System.Collections.IEnumerable;
    Assert(teardownChoices is not null && !teardownChoices.Cast<object>().Any(), "Expected no actionable shop choices once shop foreground ownership is gone.");

    var buildInventoryMethod = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.InventoryPublisher")
        ?.GetMethod("BuildInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(buildInventoryMethod is not null, "Expected private InventoryPublisher.BuildInventory helper.");
    var normalizedShopScene = new CompanionNormalizedScene("shop", "shop", 1.0, "test");
    var snapshot = CreateInventoryPublisherSnapshot(shopChoices.Cast<LiveExportChoiceSummary>().ToArray());
    var inventoryNodes = buildInventoryMethod!.Invoke(null, new object?[] { snapshot, "dormant", normalizedShopScene }) as HarnessNodeInventory;
    Assert(inventoryNodes is not null, "Expected inventory publisher to build a shop node inventory.");
    Assert(inventoryNodes!.Nodes.Any(node => string.Equals(node.Kind, "shop-option:relic", StringComparison.OrdinalIgnoreCase)), "Inventory publisher should preserve typed shop relic kinds.");
    Assert(inventoryNodes.Nodes.Any(node => string.Equals(node.Kind, "shop-card-removal", StringComparison.OrdinalIgnoreCase)), "Inventory publisher should preserve explicit shop card-removal kinds.");
}

static void TestRuntimeReflectionRewardOwnershipExport()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var observeMethod = extractorType!.GetMethod("ObserveRewardScreen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(observeMethod is not null, "Expected private ObserveRewardScreen helper.");

    var rewardButton = new FakeClickableControl
    {
        Visible = true,
        Enabled = true,
        Position = new FakeVector2(760, 360),
        Size = new FakeVector2(380, 120),
    };
    var proceedButton = new FakeClickableControl
    {
        Visible = true,
        Enabled = true,
        Position = new FakeVector2(1680, 760),
        Size = new FakeVector2(220, 110),
    };
    var rewardScreen = new FakeNRewardsScreen
    {
        Visible = true,
        _proceedButton = proceedButton,
        _rewardButtons = new object[] { rewardButton },
    };
    var activeRewardContext = new FakeActiveScreenContext
    {
        CurrentScreen = rewardScreen,
    };
    var activeRewardObservation = observeMethod!.Invoke(null, new object?[] { new object[] { rewardScreen, rewardButton, proceedButton, activeRewardContext }, "rewards" });
    Assert(activeRewardObservation is not null, "Expected reward observation.");
    Assert((bool)(ReadProperty(activeRewardObservation!, "ScreenDetected") ?? false), "Expected reward screen detection.");
    Assert((bool)(ReadProperty(activeRewardObservation!, "ForegroundOwned") ?? false), "Expected reward foreground ownership while reward screen is current.");
    Assert((bool)(ReadProperty(activeRewardObservation!, "RewardIsCurrentActiveScreen") ?? false), "Expected reward current-active-screen export.");

    var disabledRewardButton = new FakeClickableControl
    {
        Visible = true,
        Enabled = false,
        Position = new FakeVector2(760, 360),
        Size = new FakeVector2(380, 120),
    };
    var disabledProceedButton = new FakeClickableControl
    {
        Visible = true,
        Enabled = false,
        Position = new FakeVector2(1680, 760),
        Size = new FakeVector2(220, 110),
    };
    var staleRewardScreen = new FakeNRewardsScreen
    {
        Visible = true,
        _proceedButton = disabledProceedButton,
        _rewardButtons = new object[] { disabledRewardButton },
    };
    var activeMapContext = new FakeActiveScreenContext
    {
        CurrentScreen = new FakeNMapScreen { IsOpen = true, Visible = true },
    };
    var rewardAftermathObservation = observeMethod.Invoke(null, new object?[] { new object[] { staleRewardScreen, disabledRewardButton, disabledProceedButton, activeMapContext }, "rewards" });
    Assert(rewardAftermathObservation is not null, "Expected reward aftermath observation.");
    Assert((bool)(ReadProperty(rewardAftermathObservation!, "ScreenVisible") ?? false), "Expected stale reward visibility to remain exported for diagnostics.");
    Assert((bool)(ReadProperty(rewardAftermathObservation!, "ForegroundOwned") ?? true) == false, "Expected reward foreground ownership to drop once map becomes current and reward controls are disabled.");
    Assert((bool)(ReadProperty(rewardAftermathObservation!, "TeardownInProgress") ?? false), "Expected reward teardown export once map is current and reward visuals merely linger.");
    Assert((bool)(ReadProperty(rewardAftermathObservation!, "MapIsCurrentActiveScreen") ?? false), "Expected map current active screen export during reward proceed aftermath.");

    var terminalBoundaryContext = new FakeActiveScreenContext
    {
        CurrentScreen = new FakeNGameOverScreen(),
    };
    var terminalObservation = observeMethod.Invoke(null, new object?[] { new object[] { terminalBoundaryContext }, "unknown" });
    Assert(terminalObservation is not null, "Expected terminal boundary observation.");
    Assert((bool)(ReadProperty(terminalObservation!, "TerminalRunBoundary") ?? false), "Expected terminal run boundary export when game-over screen becomes current.");
    Assert((bool)(ReadProperty(terminalObservation!, "GameOverScreenDetected") ?? false), "Expected explicit game-over screen detection.");
}

static void TestRuntimeReflectionMixedRestAftermathMapExport()
{
    var bossPoint = new FakeNBossMapPoint("Boss", row: 15, col: 0, x: 1044, y: 186, enabled: true, width: 136, height: 136);
    var mapScreen = new FakeNMapScreen
    {
        IsOpen = true,
        Visible = true,
        _mapPointDictionary = new Dictionary<object, object>
        {
            [bossPoint.Point.coord] = bossPoint,
        },
    };
    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeActiveScreenContext
            {
                CurrentScreen = mapScreen,
            },
            mapScreen,
            new FakeRestSiteOption
            {
                OptionId = "SMITH",
                Label = "Smith",
            },
        },
        "rest-site");

    Assert(observation.Meta.TryGetValue("choiceExtractorPath", out var extractorPath)
           && string.Equals(extractorPath, "map", StringComparison.OrdinalIgnoreCase),
        "Mixed rest-site aftermath should switch choice extraction onto explicit map authority.");
    Assert(observation.Meta.TryGetValue("mapCurrentActiveScreen", out var mapCurrentActiveScreen)
           && string.Equals(mapCurrentActiveScreen, "true", StringComparison.OrdinalIgnoreCase),
        "Mixed rest-site aftermath should preserve current active-screen map authority.");
    Assert(observation.Meta.TryGetValue("activeScreenType", out var activeScreenType)
           && activeScreenType?.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase) == true,
        "Mixed rest-site aftermath should export NMapScreen as the active screen type.");

    var exportedMapNode = observation.Choices.SingleOrDefault(choice => string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase));
    Assert(exportedMapNode is not null, "Mixed rest-site aftermath should still export an explicit reachable map-node.");
    Assert(string.Equals(exportedMapNode!.Value, "15,0", StringComparison.OrdinalIgnoreCase)
           && string.Equals(exportedMapNode.NodeId, "map:15:0", StringComparison.OrdinalIgnoreCase),
        "Boss-adjacent next node should use the ordinary map coordinate export path.");
    Assert(exportedMapNode.Description?.Contains("type:Boss", StringComparison.OrdinalIgnoreCase) == true
           && exportedMapNode.Description.Contains("coord:15,0", StringComparison.OrdinalIgnoreCase),
        "Boss-adjacent next node should preserve ordinary NMapPoint-style type and coord metadata.");
}

static void TestRuntimeReflectionMixedRestAftermathMapDiagnostics()
{
    var disabledBossPoint = new FakeNBossMapPoint("Boss", row: 15, col: 0, x: 1044, y: 186, enabled: false, width: 136, height: 136);
    var mapScreen = new FakeNMapScreen
    {
        IsOpen = true,
        Visible = true,
        _mapPointDictionary = new Dictionary<object, object>
        {
            [disabledBossPoint.Point.coord] = disabledBossPoint,
        },
    };
    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeActiveScreenContext
            {
                CurrentScreen = mapScreen,
            },
            mapScreen,
        },
        "rest-site");

    Assert(!observation.Choices.Any(choice => string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)),
        "Disabled mixed-aftermath map points should not be exported as actionable map-node choices.");
    Assert(observation.Meta.TryGetValue("mapPointCount", out var mapPointCount)
           && string.Equals(mapPointCount, "1", StringComparison.OrdinalIgnoreCase),
        "Mixed-aftermath map diagnostics should record how many map points were seen even when they were filtered.");
    Assert(observation.Meta.TryGetValue("mapPointRejectSummary", out var mapPointRejectSummary)
           && mapPointRejectSummary?.Contains("disabled=1", StringComparison.OrdinalIgnoreCase) == true,
        "Mixed-aftermath map diagnostics should explain when all seen map points were filtered as disabled.");
}

static void TestRuntimeReflectionAncientEventDialogueExport()
{
    var dialogueHitbox = new FakeDialogueHitboxNButton
    {
        Name = "DialogueHitbox",
        Visible = true,
        Enabled = true,
        Position = new FakeVector2(0, 0),
        Size = new FakeVector2(0, 0),
    };
    var fakeNextButton = new FakeSceneNode
    {
        Name = "FakeNextButton",
        Visible = true,
        Position = new FakeVector2(1550, 940),
        Size = new FakeVector2(180, 70),
    };
    var precreatedOptionButtons = new object[]
    {
        new FakeNEventOptionButton(0, "니오우의 비탄", "덱에 카드를 추가합니다.", 460, 1100, enabled: true),
        new FakeNEventOptionButton(1, "비전 두루마리", "희귀 카드를 얻습니다.", 460, 1196, enabled: true),
        new FakeNEventOptionButton(2, "두루마리 상자", "카드 팩을 선택합니다.", 460, 1292, enabled: true),
    };
    var ancientLayout = new FakeNAncientEventLayout
    {
        Visible = true,
        IsDialogueOnLastLine = false,
        _fakeNextButton = fakeNextButton,
        Children = new object[] { dialogueHitbox, fakeNextButton }.Concat(precreatedOptionButtons).ToArray(),
    };
    var eventRoom = new FakeNEventRoom
    {
        Visible = true,
        Children = new object[] { ancientLayout },
    };
    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeActiveScreenContext
            {
                CurrentScreen = eventRoom,
            },
            eventRoom,
        },
        "event");

    Assert(observation.Meta.TryGetValue("choiceExtractorPath", out var extractorPath)
           && string.Equals(extractorPath, "event", StringComparison.OrdinalIgnoreCase),
        "Ancient dialogue phase should still use the explicit event extractor path.");
    Assert(observation.Meta.TryGetValue("ancientEventDetected", out var ancientDetected)
           && string.Equals(ancientDetected, "true", StringComparison.OrdinalIgnoreCase),
        "Ancient dialogue phase should export explicit ancient-event detection.");
    Assert(observation.Meta.TryGetValue("ancientDialogueActive", out var ancientDialogueActive)
           && string.Equals(ancientDialogueActive, "true", StringComparison.OrdinalIgnoreCase),
        "Ancient dialogue phase should export explicit dialogue-active truth.");
    Assert(observation.Meta.TryGetValue("ancientEventExtractionPath", out var ancientPath)
           && string.Equals(ancientPath, "ancient-dialogue-hitbox", StringComparison.OrdinalIgnoreCase),
        "Ancient dialogue phase should report the dialogue-hitbox extraction path.");
    Assert(observation.Meta.TryGetValue("foregroundOwner", out var foregroundOwner)
           && string.Equals(foregroundOwner, "event", StringComparison.OrdinalIgnoreCase),
        "Ancient dialogue phase should remain event-owned.");
    Assert(observation.Meta.TryGetValue("foregroundActionLane", out var foregroundLane)
           && string.Equals(foregroundLane, "ancient-dialogue", StringComparison.OrdinalIgnoreCase),
        "Ancient dialogue phase should export the ancient-dialogue foreground lane.");
    Assert(observation.Meta.TryGetValue("ancientOptionCount", out var ancientOptionCount)
           && string.Equals(ancientOptionCount, "0", StringComparison.OrdinalIgnoreCase),
        "Ancient dialogue phase should not pretend option buttons are already enabled.");

    var dialogueChoice = observation.Choices.SingleOrDefault(choice => string.Equals(choice.Kind, "event-dialogue", StringComparison.OrdinalIgnoreCase));
    Assert(dialogueChoice is not null, "Ancient dialogue phase should export an explicit dialogue-advance choice.");
    Assert(string.Equals(dialogueChoice!.NodeId, "ancient-dialogue:advance", StringComparison.OrdinalIgnoreCase)
           && string.Equals(dialogueChoice.ScreenBounds, "1550,940,180,70", StringComparison.OrdinalIgnoreCase),
        "Ancient dialogue advance should fall back to the runtime fake-next-button bounds when the dialogue hitbox itself has no usable bounds.");
}

static void TestRuntimeReflectionAncientEventOptionExport()
{
    var optionButtons = new object[]
    {
        new FakeNEventOptionButton(
            0,
            "니오우의 비탄",
            "덱에 니오우의 격분을 1장 추가합니다.",
            460,
            360,
            enabled: true),
        new FakeNEventOptionButton(
            1,
            "비전 두루마리",
            "무작위 희귀 카드를 1장 얻습니다.",
            460,
            468,
            enabled: true),
        new FakeNEventOptionButton(
            2,
            "두루마리 상자",
            "모든 골드를 잃고 카드 팩을 선택합니다.",
            460,
            576,
            enabled: true),
        new FakeAncientPseudoChoice(
            "니오우의 비탄",
            "덱에 니오우의 격분을 1장 추가합니다.",
            460,
            1100),
    };
    var ancientLayout = new FakeNAncientEventLayout
    {
        Visible = true,
        IsDialogueOnLastLine = true,
        Children = optionButtons,
    };
    var eventRoom = new FakeNEventRoom
    {
        Visible = true,
        Children = new object[] { ancientLayout },
    };
    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeActiveScreenContext
            {
                CurrentScreen = eventRoom,
            },
            eventRoom,
        },
        "event");

    Assert(observation.Meta.TryGetValue("ancientEventDetected", out var ancientDetected)
           && string.Equals(ancientDetected, "true", StringComparison.OrdinalIgnoreCase),
        "Ancient option phase should preserve ancient-event detection.");
    Assert(observation.Meta.TryGetValue("ancientDialogueActive", out var ancientDialogueActive)
           && string.Equals(ancientDialogueActive, "false", StringComparison.OrdinalIgnoreCase),
        "Ancient option phase should clear dialogue-active truth once buttons are enabled.");
    Assert(observation.Meta.TryGetValue("ancientOptionCount", out var ancientOptionCount)
           && string.Equals(ancientOptionCount, "3", StringComparison.OrdinalIgnoreCase),
        "Neow-like ancient option phase should report the enabled option-button count.");
    Assert(observation.Meta.TryGetValue("ancientEventExtractionPath", out var ancientPath)
           && string.Equals(ancientPath, "ancient-option-buttons", StringComparison.OrdinalIgnoreCase),
        "Ancient option phase should report the explicit button extraction path.");
    Assert(observation.Meta.TryGetValue("foregroundOwner", out var foregroundOwner)
           && string.Equals(foregroundOwner, "event", StringComparison.OrdinalIgnoreCase),
        "Ancient option phase should remain event-owned.");
    Assert(observation.Meta.TryGetValue("foregroundActionLane", out var foregroundLane)
           && string.Equals(foregroundLane, "ancient-option", StringComparison.OrdinalIgnoreCase),
        "Ancient option phase should export the ancient-option foreground lane.");

    var exportedOptions = observation.Choices
        .Where(choice => string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase))
        .OrderBy(choice => choice.NodeId, StringComparer.OrdinalIgnoreCase)
        .ToArray();
    Assert(exportedOptions.Length == 3, "Ancient option phase should export only the three explicit option buttons.");
    Assert(exportedOptions.All(choice => !string.IsNullOrWhiteSpace(choice.ScreenBounds)
                                         && !choice.ScreenBounds!.Contains("1100", StringComparison.OrdinalIgnoreCase)),
        "Ancient option phase should use the real button bounds instead of off-window pseudo-choice bounds.");
    Assert(exportedOptions.Any(choice => string.Equals(choice.NodeId, "ancient-event-option:0", StringComparison.OrdinalIgnoreCase)
                                         && choice.SemanticHints.Contains("source:ancient-option-button", StringComparer.OrdinalIgnoreCase)),
        "Ancient option phase should retain explicit button-source hints for the harness contract.");
}

static void TestRuntimeReflectionAncientEventCompletionExport()
{
    var proceedButton = new FakeNEventOptionButton(
        0,
        "진행",
        "[gold][b]진행[/b][/gold]",
        460,
        942,
        enabled: true,
        isProceed: true);
    proceedButton.HasFocusState = true;
    var ancientLayout = new FakeNAncientEventLayout
    {
        Visible = true,
        IsDialogueOnLastLine = true,
        DefaultFocusedControl = proceedButton,
        Children = new object[]
        {
            proceedButton,
        },
    };
    var eventRoom = new FakeNEventRoom
    {
        Visible = true,
        Children = new object[] { ancientLayout },
    };
    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeActiveScreenContext
            {
                CurrentScreen = eventRoom,
            },
            eventRoom,
        },
        "event");

    Assert(observation.Meta.TryGetValue("ancientCompletionActive", out var ancientCompletionActive)
           && string.Equals(ancientCompletionActive, "true", StringComparison.OrdinalIgnoreCase),
        "Ancient post-choice proceed should export explicit completion-active truth.");
    Assert(observation.Meta.TryGetValue("ancientCompletionCount", out var ancientCompletionCount)
           && string.Equals(ancientCompletionCount, "1", StringComparison.OrdinalIgnoreCase),
        "Ancient post-choice proceed should report the explicit completion-button count.");
    Assert(observation.Meta.TryGetValue("ancientEventExtractionPath", out var ancientPath)
           && string.Equals(ancientPath, "ancient-completion-button", StringComparison.OrdinalIgnoreCase),
        "Ancient post-choice proceed should report the completion-button extraction path.");
    Assert(observation.Meta.TryGetValue("foregroundOwner", out var foregroundOwner)
           && string.Equals(foregroundOwner, "event", StringComparison.OrdinalIgnoreCase),
        "Ancient completion should remain event-owned before map release truth appears.");
    Assert(observation.Meta.TryGetValue("foregroundActionLane", out var foregroundLane)
           && string.Equals(foregroundLane, "ancient-completion", StringComparison.OrdinalIgnoreCase),
        "Ancient completion should export the ancient-completion foreground lane.");
    Assert(observation.Meta.TryGetValue("ancientCompletionUsesDefaultFocus", out var usesDefaultFocus)
           && string.Equals(usesDefaultFocus, "true", StringComparison.OrdinalIgnoreCase),
        "Ancient completion export should record when the proceed button matches the layout default focused control.");
    Assert(observation.Meta.TryGetValue("ancientCompletionHasFocus", out var hasFocus)
           && string.Equals(hasFocus, "true", StringComparison.OrdinalIgnoreCase),
        "Ancient completion export should record whether the explicit proceed control actually has focus.");
    Assert(observation.Meta.TryGetValue("ancientCompletionBoundsSource", out var boundsSource)
           && !string.IsNullOrWhiteSpace(boundsSource),
        "Ancient completion export should report which interactive bounds source was used for the proceed button.");

    var completionChoice = observation.Choices.Single(choice => string.Equals(choice.NodeId, "ancient-event-option:0", StringComparison.OrdinalIgnoreCase));
    Assert(completionChoice.SemanticHints.Contains("ancient-event-completion", StringComparer.OrdinalIgnoreCase),
        "Ancient post-choice proceed should carry explicit completion semantic hints.");
    Assert(completionChoice.SemanticHints.Contains("option-role:proceed", StringComparer.OrdinalIgnoreCase),
        "Ancient post-choice proceed should preserve the canonical Option.IsProceed role.");
}

static void TestRuntimeReflectionGenericEventProceedExport()
{
    var proceedButton = new FakeNEventOptionButton(
        0,
        "계속",
        "[gold][b]계속[/b][/gold]",
        460,
        942,
        enabled: true,
        isProceed: true);
    var eventRoom = new FakeNEventRoom
    {
        Visible = true,
        Children = new object[] { proceedButton },
    };
    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeActiveScreenContext
            {
                CurrentScreen = eventRoom,
            },
            eventRoom,
            proceedButton,
        },
        "event");

    Assert(observation.Meta.TryGetValue("choiceExtractorPath", out var extractorPath)
           && string.Equals(extractorPath, "event", StringComparison.OrdinalIgnoreCase),
        "Generic event proceed should remain on the explicit event extractor path.");
    Assert(observation.Meta.TryGetValue("eventProceedOptionVisible", out var proceedVisible)
           && string.Equals(proceedVisible, "true", StringComparison.OrdinalIgnoreCase),
        "Generic event proceed should export visible proceed authority.");
    Assert(observation.Meta.TryGetValue("eventProceedOptionEnabled", out var proceedEnabled)
           && string.Equals(proceedEnabled, "true", StringComparison.OrdinalIgnoreCase),
        "Generic event proceed should export enabled proceed authority.");
    Assert(observation.Meta.TryGetValue("eventProceedOptionCount", out var proceedCount)
           && string.Equals(proceedCount, "1", StringComparison.OrdinalIgnoreCase),
        "Generic event proceed should export proceed option count.");

    var proceedChoice = observation.Choices.First(choice => choice.SemanticHints.Contains("option-role:proceed", StringComparer.OrdinalIgnoreCase));
    Assert(string.Equals(proceedChoice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)
           && string.Equals(proceedChoice.NodeId, "event-option:0", StringComparison.OrdinalIgnoreCase)
           && string.Equals(proceedChoice.BindingKind, "event-option", StringComparison.OrdinalIgnoreCase)
           && string.Equals(proceedChoice.BindingId, "option-0", StringComparison.OrdinalIgnoreCase)
           && proceedChoice.Enabled == true,
        "Generic event proceed should export stable event-option identity and enabled metadata.");
    Assert(proceedChoice.SemanticHints.Contains("source:event-option-button", StringComparer.OrdinalIgnoreCase)
           && proceedChoice.SemanticHints.Contains("event-proceed", StringComparer.OrdinalIgnoreCase),
        "Generic event proceed should preserve explicit EventOption.IsProceed semantic hints.");
}

static void TestRuntimeReflectionAncientMixedPostProceedOwnershipNormalization()
{
    var proceedButton = new FakeNEventOptionButton(
        0,
        "진행",
        "[gold][b]진행[/b][/gold]",
        460,
        942,
        enabled: true,
        isProceed: true);
    var ancientLayout = new FakeNAncientEventLayout
    {
        Visible = true,
        IsDialogueOnLastLine = true,
        DefaultFocusedControl = proceedButton,
        Children = new object[] { proceedButton },
    };
    var eventRoom = new FakeNEventRoom
    {
        Visible = true,
        Children = new object[] { ancientLayout },
    };
    var mapScreen = new FakeNMapScreen
    {
        Visible = true,
        IsOpen = true,
        _mapPointDictionary = new Dictionary<string, object>
        {
            ["1,3"] = new FakeNMapPointNode("Monster", 1, 3, 440, 300, enabled: true, width: 92, height: 92),
            ["1,6"] = new FakeNMapPointNode("Monster", 1, 6, 760, 300, enabled: true, width: 92, height: 92),
        },
    };

    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeActiveScreenContext
            {
                CurrentScreen = mapScreen,
            },
            eventRoom,
            mapScreen,
        },
        "event");

    Assert(observation.Meta.TryGetValue("foregroundOwner", out var foregroundOwner)
           && string.Equals(foregroundOwner, "map", StringComparison.OrdinalIgnoreCase),
        "Mixed post-proceed state should normalize foreground ownership to map once map release truth is active.");
    Assert(observation.Meta.TryGetValue("foregroundActionLane", out var foregroundLane)
           && string.Equals(foregroundLane, "map-node", StringComparison.OrdinalIgnoreCase),
        "Mixed post-proceed state should normalize the foreground action lane to map-node.");
    Assert(observation.Meta.TryGetValue("choiceExtractorPath", out var extractorPath)
           && string.Equals(extractorPath, "map", StringComparison.OrdinalIgnoreCase),
        "Mixed post-proceed state should switch the choice extractor path to map.");
    Assert(observation.Meta.TryGetValue("eventTeardownInProgress", out var eventTeardown)
           && string.Equals(eventTeardown, "true", StringComparison.OrdinalIgnoreCase),
        "Mixed post-proceed state should mark ancient residue as teardown/background state.");
    Assert(observation.Meta.TryGetValue("mapReleaseAuthority", out var mapReleaseAuthority)
           && string.Equals(mapReleaseAuthority, "true", StringComparison.OrdinalIgnoreCase),
        "Mixed post-proceed state should record map release authority.");
    Assert(observation.Choices.Any(choice => string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)),
        "Mixed post-proceed state should surface actionable map-node choices.");
    Assert(!observation.Choices.Any(choice => string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)),
        "Mixed post-proceed state should not keep ancient completion residue as the foreground action lane.");
}

static void TestRuntimeReflectionAncientMixedPostProceedMapPending()
{
    var proceedButton = new FakeNEventOptionButton(
        0,
        "진행",
        "[gold][b]진행[/b][/gold]",
        460,
        942,
        enabled: true,
        isProceed: true);
    var ancientLayout = new FakeNAncientEventLayout
    {
        Visible = true,
        IsDialogueOnLastLine = true,
        Children = new object[] { proceedButton },
    };
    var eventRoom = new FakeNEventRoom
    {
        Visible = true,
        Children = new object[] { ancientLayout },
    };
    var mapScreen = new FakeNMapScreen
    {
        Visible = true,
        IsOpen = true,
        _mapPointDictionary = new Dictionary<string, object>
        {
            ["1,3"] = new FakeNMapPointNode("Monster", 1, 3, 440, 300, enabled: false, width: 92, height: 92),
        },
    };

    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeActiveScreenContext
            {
                CurrentScreen = mapScreen,
            },
            eventRoom,
            mapScreen,
        },
        "event");

    Assert(observation.Meta.TryGetValue("foregroundOwner", out var foregroundOwner)
           && string.Equals(foregroundOwner, "map", StringComparison.OrdinalIgnoreCase),
        "Map release truth should keep ownership on map even if no actionable node surface is ready yet.");
    Assert(observation.Meta.TryGetValue("foregroundActionLane", out var foregroundLane)
           && string.Equals(foregroundLane, "none", StringComparison.OrdinalIgnoreCase),
        "Map-pending post-proceed state should expose no foreground action lane until a map node is actionable.");
    Assert(observation.Meta.TryGetValue("mapSurfacePending", out var mapSurfacePending)
           && string.Equals(mapSurfacePending, "true", StringComparison.OrdinalIgnoreCase),
        "Map-pending post-proceed state should explicitly mark map surface pending.");
    Assert(observation.Meta.TryGetValue("choiceExtractorPath", out var extractorPath)
           && string.Equals(extractorPath, "map", StringComparison.OrdinalIgnoreCase),
        "Map-pending post-proceed state should still use the map extractor path.");
    Assert(!observation.Choices.Any(choice => string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)),
        "Map-pending post-proceed state should not reactivate ancient completion as the foreground lane.");
}

static void TestInventoryPublisherMapNodeSourceCorrection()
{
    var buildInventoryMethod = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.InventoryPublisher")
        ?.GetMethod("BuildInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(buildInventoryMethod is not null, "Expected private InventoryPublisher.BuildInventory helper.");

    var normalizedMapScene = new CompanionNormalizedScene("map", "map", 1.0, "test");
    var snapshot = CreateInventoryPublisherSnapshot(
        new[]
        {
            new LiveExportChoiceSummary("relic", "불타는 혈액", "RELIC.BURNING_BLOOD", "Inventory relic")
            {
                ScreenBounds = "12,82,68,68",
            },
            new LiveExportChoiceSummary("choice", "Chest", null, "Chest")
            {
                ScreenBounds = "602,367,800,500",
            },
            new LiveExportChoiceSummary("choice", "진행", null, "진행")
            {
                ScreenBounds = "1983,764,269,108",
            },
            new LiveExportChoiceSummary("map-node", "휴식 (1,2)", "1,2", "type:Rest;coord:1,2")
            {
                NodeId = "map:1:2",
                ScreenBounds = "897,581,124,124",
            },
        });

    var inventory = buildInventoryMethod!.Invoke(null, new object?[] { snapshot, "dormant", normalizedMapScene }) as HarnessNodeInventory;
    Assert(inventory is not null, "Expected inventory publisher to build a node inventory.");
    Assert(inventory!.Nodes.Count == 4, "Expected inventory publisher to keep all source choices.");

    var burningBlood = inventory.Nodes.First(node => string.Equals(node.Label, "불타는 혈액", StringComparison.OrdinalIgnoreCase));
    Assert(!string.Equals(burningBlood.Kind, "map-node", StringComparison.OrdinalIgnoreCase), "Owned relics must not be promoted to fake map-node inventory entries.");

    var chest = inventory.Nodes.First(node => string.Equals(node.Label, "Chest", StringComparison.OrdinalIgnoreCase));
    Assert(!string.Equals(chest.Kind, "map-node", StringComparison.OrdinalIgnoreCase), "Generic chest affordances must not be promoted to fake map-node inventory entries.");

    var proceed = inventory.Nodes.First(node => string.Equals(node.Label, "진행", StringComparison.OrdinalIgnoreCase));
    Assert(!string.Equals(proceed.Kind, "map-node", StringComparison.OrdinalIgnoreCase), "Generic proceed affordances must not be promoted to fake map-node inventory entries.");

    var restNode = inventory.Nodes.First(node => string.Equals(node.NodeId, "map:1:2", StringComparison.OrdinalIgnoreCase));
    Assert(string.Equals(restNode.Kind, "map-node", StringComparison.OrdinalIgnoreCase), "Real map points must remain map-node inventory entries.");
    Assert(restNode.SemanticHints.Contains("coord:1,2", StringComparer.OrdinalIgnoreCase), "Real map points should preserve coordinate evidence in inventory semantic hints.");
    Assert(restNode.SemanticHints.Contains("source:map-choice", StringComparer.OrdinalIgnoreCase), "Real map points should retain explicit map-choice source hints.");
}

static void TestTrackerAndInventoryPreserveScreenProvenance()
{
    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live");
    var observation = new LiveExportObservation(
        "runtime-poll",
        DateTimeOffset.UtcNow,
        "run-provenance-self-test",
        "active",
        "rewards",
        null,
        null,
        LiveExportPlayerSummary.Empty,
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<LiveExportChoiceSummary>(),
        Array.Empty<string>(),
        null,
        new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase),
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["screen"] = "rewards",
            ["rawObservedScreen"] = "rewards",
            ["instanceType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
            ["currentSceneType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
            ["rootTypeSummary"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
        });

    var snapshot = tracker.Apply(observation).Snapshot;
    Assert(string.Equals(snapshot.CurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker legacy current screen should stay on the compatibility logical screen.");
    Assert(string.Equals(snapshot.RawObservedScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should preserve the raw observed screen separately from compatibility logical screen.");
    Assert(string.Equals(snapshot.CompatibilityLogicalScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should expose the compatibility logical screen explicitly.");
    Assert(string.Equals(snapshot.CompatibilityVisibleScreen, "map", StringComparison.OrdinalIgnoreCase), "Tracker should expose compatibility visible-screen shaping separately.");
    Assert(snapshot.CompatibilitySceneReady == false, "Reward/map mixed aftermath should demote compatibility scene-ready while leaving raw screen intact.");
    Assert(snapshot.Meta.TryGetValue("screen", out var rawScreenMeta)
           && string.Equals(rawScreenMeta, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker meta 'screen' should now preserve raw observed screen.");
    Assert(snapshot.Meta.TryGetValue("compatLogicalScreen", out var compatLogicalScreen)
           && string.Equals(compatLogicalScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should write compatibility logical screen into dedicated meta.");
    Assert(snapshot.Meta.TryGetValue("compatVisibleScreen", out var compatVisibleScreen)
           && string.Equals(compatVisibleScreen, "map", StringComparison.OrdinalIgnoreCase), "Tracker should write compatibility visible screen into dedicated meta.");

    var buildInventoryMethod = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.InventoryPublisher")
        ?.GetMethod("BuildInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(buildInventoryMethod is not null, "Expected private InventoryPublisher.BuildInventory helper.");
    var normalizedScene = new CompanionNormalizedScene("map", "map", 1.0, "test");
    var inventory = buildInventoryMethod!.Invoke(null, new object?[] { snapshot, "dormant", normalizedScene }) as HarnessNodeInventory;
    Assert(inventory is not null, "Expected inventory publisher to build an inventory from the tracker snapshot.");
    Assert(string.Equals(inventory!.RawSceneType, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory should preserve raw scene type separately from compatibility scene type.");
    Assert(string.Equals(inventory.RawCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory should expose raw current screen explicitly instead of forcing consumers to overload rawSceneType.");
    Assert(string.Equals(inventory.SceneType, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory legacy scene type should remain the compatibility scene type.");
    Assert(string.Equals(inventory.CompatibilitySceneType, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory should expose compatibility scene type explicitly.");
    Assert(string.Equals(inventory.CompatibilityCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory should expose compatibility current screen explicitly instead of overloading sceneType.");
    Assert(string.Equals(inventory.CompatibilityVisibleScene, "map", StringComparison.OrdinalIgnoreCase), "Inventory should preserve compatibility visible scene for downstream diagnostics.");
    Assert(string.Equals(inventory.CompatibilityVisibleScreen, "map", StringComparison.OrdinalIgnoreCase), "Inventory should expose compatibility visible screen explicitly for downstream consumers.");
    Assert(inventory.CompatibilitySceneReady == false, "Inventory should preserve compatibility scene-ready instead of recomputing raw winner truth.");
}

static void TestInventoryPublisherPrefersCompatibilitySceneProvenance()
{
    var buildInventoryMethod = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.InventoryPublisher")
        ?.GetMethod("BuildInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(buildInventoryMethod is not null, "Expected private InventoryPublisher.BuildInventory helper.");

    var snapshot = CreateInventoryPublisherSnapshot(
        Array.Empty<LiveExportChoiceSummary>()) with
    {
        CurrentScreen = "rewards",
        RawObservedScreen = "rewards",
        CompatibilityLogicalScreen = "rewards",
        CompatibilityVisibleScreen = "map",
        CompatibilitySceneReady = false,
        CompatibilitySceneAuthority = "mixed",
        CompatibilitySceneStability = "stabilizing",
        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["logicalScreen"] = "event",
            ["flowScreen"] = "event",
            ["visibleScreen"] = "event",
            ["sceneReady"] = "true",
            ["sceneAuthority"] = "polling",
            ["sceneStability"] = "stable",
        },
    };

    var normalizedScene = new CompanionNormalizedScene("event", "event", 1.0, "test");
    var inventory = buildInventoryMethod!.Invoke(null, new object?[] { snapshot, "dormant", normalizedScene }) as HarnessNodeInventory;
    Assert(inventory is not null, "Expected inventory publisher to build an inventory.");
    Assert(string.Equals(inventory!.SceneType, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory scene type should follow explicit compatibility logical screen, not legacy logicalScreen/flowScreen meta.");
    Assert(string.Equals(inventory.CompatibilityVisibleScene, "map", StringComparison.OrdinalIgnoreCase), "Inventory visible scene should follow explicit compatibility visible screen, not legacy visibleScreen meta.");
    Assert(inventory.CompatibilitySceneReady == false, "Inventory scene-ready should preserve explicit compatibility truth.");
    Assert(string.Equals(inventory.CompatibilitySceneAuthority, "mixed", StringComparison.OrdinalIgnoreCase), "Inventory scene authority should preserve explicit compatibility truth.");
    Assert(string.Equals(inventory.CompatibilitySceneStability, "stabilizing", StringComparison.OrdinalIgnoreCase), "Inventory scene stability should preserve explicit compatibility truth.");
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

static void TestRuntimeReflectionCombatMetadataExport()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod(
        "AppendCombatRuntimeMetadata",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(method is not null, "Expected private combat runtime metadata export helper.");

    var meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
    {
        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false",
    };
    var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    var roots = new object[]
    {
        new FakeRuntimePlayerHand
        {
            InCardPlay = true,
            CurrentMode = "Combat",
            _draggedHolderIndex = 1,
            _holdersAwaitingQueue = new Dictionary<object, int> { [new object()] = 1 },
            _currentCardPlay = new FakeRuntimeCardPlay
            {
                Holder = new FakeRuntimeHandHolder
                {
                    CardModel = new FakeRuntimeCombatCard
                    {
                        Id = "CARD.DEFEND_IRONCLAD",
                        Title = "Defend",
                        Type = "Skill",
                        TargetType = "Self",
                    },
                },
            },
        },
        new FakeRuntimeTargetManager
        {
            IsInSelection = true,
            HoveredNode = new FakeRuntimeTargetNode
            {
                Id = "MONSTER.JAW_WORM",
                DisplayName = "Jaw Worm",
                Entity = new FakeRuntimeTargetEntity
                {
                    Id = "MONSTER.JAW_WORM",
                    DisplayName = "Jaw Worm",
                },
            },
            LastTargetingFinishedFrame = 77,
        },
        new FakeRuntimeCombatHistory
        {
            CardPlaysStarted = new object[]
            {
                new FakeRuntimeCombatHistoryEntry
                {
                    CardPlay = new FakeRuntimeCardPlay
                    {
                        Card = new FakeRuntimeCombatCard
                        {
                            Id = "CARD.STRIKE_IRONCLAD",
                            Title = "Strike",
                            Type = "Attack",
                            TargetType = "AnyEnemy",
                        },
                        Target = new FakeRuntimeTargetNode
                        {
                            Id = "MONSTER.JAW_WORM",
                            DisplayName = "Jaw Worm",
                        },
                    },
                },
            },
            CardPlaysFinished = new object[]
            {
                new FakeRuntimeCombatHistoryEntry
                {
                    CardPlay = new FakeRuntimeCardPlay
                    {
                        Card = new FakeRuntimeCombatCard
                        {
                            Id = "CARD.DEFEND_IRONCLAD",
                            Title = "Defend",
                            Type = "Skill",
                            TargetType = "Self",
                        },
                    },
                },
            },
        },
        new FakeCombatManagerState
        {
            IsInProgress = true,
            IsPlayPhase = true,
            PlayerActionsDisabled = false,
            EndingPlayerTurnPhaseOne = false,
            EndingPlayerTurnPhaseTwo = true,
        },
        new FakeCombatState { RoundNumber = 3 },
    };

    method!.Invoke(null, new object?[] { roots, new LiveExportEncounterSummary("Cultist", "Monster", true, 1), meta, payload });

    Assert(meta.TryGetValue("combatCardPlayPending", out var combatCardPlayPending) && combatCardPlayPending == "true", "Expected runtime export to mark combatCardPlayPending.");
    Assert(meta.TryGetValue("combatSelectedCardId", out var selectedCardId) && selectedCardId == "CARD.DEFEND_IRONCLAD", "Expected runtime export to include selected card id.");
    Assert(meta.TryGetValue("combatSelectedCardTargetType", out var selectedCardTargetType) && selectedCardTargetType == "Self", "Expected runtime export to include selected card target type.");
    Assert(meta.TryGetValue("combatSelectedCardSlot", out var selectedCardSlot) && selectedCardSlot == "2", "Expected runtime export to include the active hand slot.");
    Assert(meta.TryGetValue("combatTargetingInProgress", out var targetingInProgress) && targetingInProgress == "true", "Expected runtime export to include targeting state.");
    Assert(meta.TryGetValue("combatHoveredTargetLabel", out var hoveredTargetLabel) && hoveredTargetLabel == "Jaw Worm", "Expected runtime export to include hovered target label.");
    Assert(meta.TryGetValue("combatHistoryStartedCount", out var startedCount) && startedCount == "1", "Expected runtime export to include combat history started count.");
    Assert(meta.TryGetValue("combatHistoryFinishedCount", out var finishedCount) && finishedCount == "1", "Expected runtime export to include combat history finished count.");
    Assert(meta.TryGetValue("combatInteractionRevision", out var interactionRevision) && interactionRevision == "1:1:true:true:2", "Expected runtime export to include the raw combat interaction revision.");
    Assert(meta.TryGetValue("combatRoundNumber", out var combatRoundNumber) && combatRoundNumber == "3", "Expected runtime export to include combatRoundNumber.");
    Assert(meta.TryGetValue("combatPlayerActionsDisabled", out var combatPlayerActionsDisabled) && combatPlayerActionsDisabled == "false", "Expected runtime export to include PlayerActionsDisabled.");
    Assert(meta.TryGetValue("combatEndingPlayerTurnPhaseOne", out var endingPhaseOne) && endingPhaseOne == "false", "Expected runtime export to include EndingPlayerTurnPhaseOne.");
    Assert(meta.TryGetValue("combatEndingPlayerTurnPhaseTwo", out var endingPhaseTwo) && endingPhaseTwo == "true", "Expected runtime export to include EndingPlayerTurnPhaseTwo.");
    Assert(meta.TryGetValue("combatCrossCheck", out var combatCrossCheck)
           && combatCrossCheck is not null
           && combatCrossCheck.Contains("CombatManager.IsPlayPhase=true", StringComparison.OrdinalIgnoreCase)
           && combatCrossCheck.Contains("CombatManager.IsEnemyTurnStarted=false", StringComparison.OrdinalIgnoreCase)
           && combatCrossCheck.Contains("CombatState.RoundNumber=3", StringComparison.OrdinalIgnoreCase)
           && combatCrossCheck.Contains("CombatManager.PlayerActionsDisabled=false", StringComparison.OrdinalIgnoreCase)
           && combatCrossCheck.Contains("CombatManager.EndingPlayerTurnPhaseOne=false", StringComparison.OrdinalIgnoreCase)
           && combatCrossCheck.Contains("CombatManager.EndingPlayerTurnPhaseTwo=true", StringComparison.OrdinalIgnoreCase),
        "Expected runtime export to append round and phase truth into combatCrossCheck without losing existing segments.");
    Assert(meta.TryGetValue("combatLastCardPlayFinishedCardId", out var finishedCardId) && finishedCardId == "CARD.DEFEND_IRONCLAD", "Expected runtime export to include the last finished card id.");
    Assert(meta.TryGetValue("combatLastCardPlayFinishedSuccess", out var finishedSuccess) && finishedSuccess is null, "Expected runtime export not to infer explicit finished success from combat history alone.");
    Assert(payload.TryGetValue("combatRoundNumber", out var payloadRoundNumber) && payloadRoundNumber is 3, "Expected runtime payload to include combatRoundNumber.");
    Assert(payload.TryGetValue("combatPlayerActionsDisabled", out var payloadPlayerActionsDisabled) && payloadPlayerActionsDisabled is false, "Expected runtime payload to include PlayerActionsDisabled.");
    Assert(payload.TryGetValue("combatEndingPlayerTurnPhaseOne", out var payloadEndingPhaseOne) && payloadEndingPhaseOne is false, "Expected runtime payload to include EndingPlayerTurnPhaseOne.");
    Assert(payload.TryGetValue("combatEndingPlayerTurnPhaseTwo", out var payloadEndingPhaseTwo) && payloadEndingPhaseTwo is true, "Expected runtime payload to include EndingPlayerTurnPhaseTwo.");
    Assert(payload.TryGetValue("combatHistoryStartedCount", out var payloadStartedCount) && payloadStartedCount is 1, "Expected runtime payload to include combat history started count.");
    Assert(payload.TryGetValue("combatHistoryFinishedCount", out var payloadFinishedCount) && payloadFinishedCount is 1, "Expected runtime payload to include combat history finished count.");
    Assert(payload.TryGetValue("combatInteractionRevision", out var payloadInteractionRevision) && string.Equals(payloadInteractionRevision as string, "1:1:true:true:2", StringComparison.Ordinal), "Expected runtime payload to include the combat interaction revision.");
}

static void TestRuntimeReflectionCaptureClearsCombatMetadata()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var captureMethod = extractorType!.GetMethod("Capture", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
    Assert(captureMethod is not null, "Expected public runtime snapshot capture entry point.");

    var observeMethod = typeof(FakeRuntimeCaptureHook).GetMethod(nameof(FakeRuntimeCaptureHook.Observe));
    Assert(observeMethod is not null, "Expected fake runtime capture hook method.");

    var binding = CreateRuntimeHookBinding(observeMethod!, "runtime-poll", "combat");
    var captureObservation = captureMethod!.Invoke(null, new object?[]
    {
        binding,
        AiCompanionRuntimeConfig.Defaults,
        null,
        new object?[]
        {
            new FakeRuntimePlayerHand
            {
                InCardPlay = false,
                CurrentMode = "Combat",
                _draggedHolderIndex = -1,
                _holdersAwaitingQueue = new Dictionary<object, int>(),
            },
            new FakeRuntimeCombatHistory
            {
                CardPlaysFinished = new object[]
                {
                    new FakeRuntimeCombatHistoryEntry
                    {
                        CardPlay = new FakeRuntimeCardPlay
                        {
                            Card = new FakeRuntimeCombatCard
                            {
                                Id = "CARD.DEFEND_IRONCLAD",
                                Title = "Defend",
                                Type = "Skill",
                                TargetType = "Self",
                            },
                        },
                    },
                },
            },
            new FakeCombatManagerState
            {
                IsInProgress = true,
                IsPlayPhase = true,
                PlayerActionsDisabled = false,
                EndingPlayerTurnPhaseOne = false,
                EndingPlayerTurnPhaseTwo = false,
            },
            new FakeCombatState(),
        },
        null,
    }) as LiveExportObservation;
    Assert(captureObservation is not null, "Expected Capture to return a live export observation.");
    Assert(captureObservation!.Meta.TryGetValue("combatSelectedCardSlot", out var selectedCardSlot) && selectedCardSlot is null, "Expected Capture to clear combatSelectedCardSlot when no active slot exists.");
    Assert(captureObservation.Meta.TryGetValue("combatLastCardPlayFinishedSuccess", out var finishedSuccess) && finishedSuccess is null, "Expected Capture to keep finished-success null without an explicit success source.");
    Assert(captureObservation.Meta.TryGetValue("combatRoundNumber", out var clearedRoundNumber) && clearedRoundNumber is null, "Expected Capture to clear combatRoundNumber when no combat state authority exists.");
    Assert(captureObservation.Meta.TryGetValue("combatPlayerActionsDisabled", out var clearedPlayerActionsDisabled) && clearedPlayerActionsDisabled == "false", "Expected Capture to export PlayerActionsDisabled=false when the combat manager is reopened.");
    Assert(captureObservation.Meta.TryGetValue("combatEndingPlayerTurnPhaseOne", out var clearedPhaseOne) && clearedPhaseOne == "false", "Expected Capture to export EndingPlayerTurnPhaseOne=false when phase one is inactive.");
    Assert(captureObservation.Meta.TryGetValue("combatEndingPlayerTurnPhaseTwo", out var clearedPhaseTwo) && clearedPhaseTwo == "false", "Expected Capture to export EndingPlayerTurnPhaseTwo=false when phase two is inactive.");

    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\combat");
    tracker.Apply(CreateObservation("runtime-poll", "combat", 1, 1, 70, 120, new[] { "Strike" }, Array.Empty<string>(), Array.Empty<string>()) with
    {
        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["combatSelectedCardSlot"] = "2",
            ["combatLastCardPlayFinishedSuccess"] = "true",
            ["combatRoundNumber"] = "9",
            ["combatPlayerActionsDisabled"] = "true",
            ["combatEndingPlayerTurnPhaseOne"] = "true",
            ["combatEndingPlayerTurnPhaseTwo"] = "true",
        },
    });
    var merged = tracker.Apply(captureObservation with
    {
        RunId = "run-001",
        RunStatus = "active",
        Screen = "combat",
    }).Snapshot;
    Assert(merged.Meta.TryGetValue("combatSelectedCardSlot", out var mergedSlot) && mergedSlot is null, "Expected tracker merge to clear stale combatSelectedCardSlot when Capture reports no active slot.");
    Assert(merged.Meta.TryGetValue("combatLastCardPlayFinishedSuccess", out var mergedSuccess) && mergedSuccess is null, "Expected tracker merge to clear stale finished-success inference when Capture reports null.");
    Assert(merged.Meta.TryGetValue("combatRoundNumber", out var mergedRoundNumber) && mergedRoundNumber is null, "Expected tracker merge to clear stale combatRoundNumber when Capture reports no current combat state.");
    Assert(merged.Meta.TryGetValue("combatPlayerActionsDisabled", out var mergedPlayerActionsDisabled) && mergedPlayerActionsDisabled == "false", "Expected tracker merge to overwrite stale PlayerActionsDisabled with the current reopened value.");
    Assert(merged.Meta.TryGetValue("combatEndingPlayerTurnPhaseOne", out var mergedPhaseOne) && mergedPhaseOne == "false", "Expected tracker merge to overwrite stale EndingPlayerTurnPhaseOne with the current reopened value.");
    Assert(merged.Meta.TryGetValue("combatEndingPlayerTurnPhaseTwo", out var mergedPhaseTwo) && mergedPhaseTwo == "false", "Expected tracker merge to overwrite stale EndingPlayerTurnPhaseTwo with the current reopened value.");
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
            ["currentSceneType"] = "MegaCrit.Sts2.Core.Nodes.NGame",
            ["rootTypeSummary"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
        })
    {
        CompatibilityLogicalScreen = "rewards",
        CompatibilityVisibleScreen = "map",
    };

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
    Assert(firstBatch.Snapshot.Meta.TryGetValue("screen-episode", out var firstScreenEpisode) && firstScreenEpisode == "rewards", "Expected tracker snapshot meta to export the active screen episode.");
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
    Assert(secondBatch.Snapshot.Meta.TryGetValue("screen-episode", out var secondScreenEpisode) && secondScreenEpisode == "rewards", "Expected snapshot meta to preserve the active screen episode across fallback polls.");
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

static void TestRewardDeterministicBuilders()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateRewardTestConfiguration(root);
        SeedRewardKnowledgeCatalog(root);
        var runState = CreateRewardRunState("reward-deterministic-run");
        var knowledgeService = new FoundationKnowledgeCatalogService(configuration, root);
        var slice = knowledgeService.BuildSlice(ToFoundationRunState(runState), 16, 8192);
        var optionBuilder = new RewardOptionSetBuilder();
        var factsBuilder = new RewardAssessmentFactsBuilder();

        var optionSetA = optionBuilder.Build(ToFoundationRunState(runState));
        var optionSetB = optionBuilder.Build(ToFoundationRunState(runState));
        var factsA = factsBuilder.Build(ToFoundationRunState(runState), slice, optionSetA);
        var factsB = factsBuilder.Build(ToFoundationRunState(runState), slice, optionSetB);
        var inputPack = new FoundationAdvicePromptBuilder(configuration).BuildInputPack(ToFoundationRunState(runState), ToFoundationTrigger(new AdviceTrigger("reward-screen-opened", DateTimeOffset.UtcNow, false, true, "reward-test", runState.RecentEvents.LastOrDefault())), slice);

        Assert(optionSetA is not null && optionSetB is not null, "Expected reward option set to be built for rewards scene.");
        Assert(optionSetA.Options.Select(option => option.Label).SequenceEqual(optionSetB.Options.Select(option => option.Label), StringComparer.Ordinal), "Expected reward option labels to be deterministic.");
        Assert(optionSetA.SkipAllowed, "Expected reward option set to expose skip availability.");
        Assert(factsA is not null && factsB is not null, "Expected reward assessment facts to be built for rewards scene.");
        Assert(factsA.KnowledgeFingerprint == factsB.KnowledgeFingerprint, "Expected reward assessment knowledge fingerprint to be reproducible.");
        Assert(factsA.FactLines.SequenceEqual(factsB.FactLines, StringComparer.Ordinal), "Expected reward assessment fact lines to be reproducible.");
        Assert(inputPack.RewardOptionSet is not null, "Expected reward input pack to include deterministic option set.");
        Assert(inputPack.RewardAssessmentFacts is not null, "Expected reward input pack to include deterministic assessment facts.");
        Assert(inputPack.RewardRecommendationTraceSeed is not null, "Expected reward input pack to include a trace seed.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestRewardDeterministicFallback()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateRewardTestConfiguration(root);
        SeedRewardKnowledgeCatalog(root);
        var hostSnapshot = CreateHostSnapshot("shop-fallback-run", "shop");
        var hostSession = new LiveExportSession("session-shop", "shop-fallback-run", "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 1, Path.Combine(root, "live"), "choice-list-presented", "shop");
        var runState = new CompanionRunState(
            hostSnapshot,
            hostSession,
            "shop summary",
            new[]
            {
                new LiveExportEventEnvelope(DateTimeOffset.UtcNow, 1, "shop-fallback-run", "choice-list-presented", "shop", 1, 3, new Dictionary<string, object?>()),
            },
            false)
        {
            NormalizedState = CompanionStateMapper.FromLiveExport(hostSnapshot, hostSession, new[]
            {
                new LiveExportEventEnvelope(DateTimeOffset.UtcNow, 1, "shop-fallback-run", "choice-list-presented", "shop", 1, 3, new Dictionary<string, object?>()),
            }),
        };
        var knowledgeService = new FoundationKnowledgeCatalogService(configuration, root);
        var slice = knowledgeService.BuildSlice(ToFoundationRunState(runState), 16, 8192);
        var inputPack = new FoundationAdvicePromptBuilder(configuration).BuildInputPack(ToFoundationRunState(runState), ToFoundationTrigger(new AdviceTrigger("choice-list-presented", DateTimeOffset.UtcNow, false, true, "shop-test", runState.RecentEvents.Last())), slice);

        Assert(inputPack.RewardOptionSet is null, "Expected reward deterministic option set to stay off outside reward scenes.");
        Assert(inputPack.RewardAssessmentFacts is null, "Expected reward deterministic facts to stay off outside reward scenes.");
        Assert(inputPack.RewardRecommendationTraceSeed is null, "Expected reward deterministic trace to stay off outside reward scenes.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestRewardNonCardFallback()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateRewardTestConfiguration(root);
        SeedRewardKnowledgeCatalog(root);
        var runState = CreateNonCardRewardRunState("reward-non-card-run");
        var knowledgeService = new FoundationKnowledgeCatalogService(configuration, root);
        var slice = knowledgeService.BuildSlice(ToFoundationRunState(runState), 16, 8192);
        var optionBuilder = new RewardOptionSetBuilder();
        var factsBuilder = new RewardAssessmentFactsBuilder();

        var optionSet = optionBuilder.Build(ToFoundationRunState(runState));
        var facts = factsBuilder.Build(ToFoundationRunState(runState), slice, optionSet);
        var inputPack = new FoundationAdvicePromptBuilder(configuration).BuildInputPack(ToFoundationRunState(runState), ToFoundationTrigger(new AdviceTrigger("reward-screen-opened", DateTimeOffset.UtcNow, false, true, "reward-non-card-test", runState.RecentEvents.LastOrDefault())), slice);

        Assert(optionSet is null, "Expected deterministic reward option set to stay off for non-card rewards.");
        Assert(facts is null, "Expected deterministic reward assessment facts to stay off for non-card rewards.");
        Assert(inputPack.RewardOptionSet is null, "Expected reward input pack option set to stay off for non-card rewards.");
        Assert(inputPack.RewardAssessmentFacts is null, "Expected reward input pack facts to stay off for non-card rewards.");
        Assert(inputPack.RewardRecommendationTraceSeed is null, "Expected reward input pack trace seed to stay off for non-card rewards.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestRewardLiveReplayDeterministicContext()
{
    foreach (var scenario in CreateCuratedRewardScenarios())
    {
        var result = ExecuteRewardScenario(scenario, FakeCodexRewardResponseMode.MinimalModelOutput);
        AssertRewardPromptPackParity(result, scenario.Name);
    }
}

static void TestRewardFinalizerMinimalNormalization()
{
    var scenario = CreateCuratedRewardScenarios().Single(candidate => candidate.Name == "draw-gap-with-unknown-alternative");
    var result = ExecuteRewardScenario(scenario, FakeCodexRewardResponseMode.MinimalModelOutput);

    Assert(result.LiveAdvice.RecommendedChoiceLabel == scenario.ExpectedChoiceLabel, "Expected minimal live response to keep the deterministic recommendation.");
    Assert(result.ReplayAdvice.RecommendedChoiceLabel == scenario.ExpectedChoiceLabel, "Expected minimal replay response to keep the deterministic recommendation.");
    Assert(result.LiveAdvice.ReasoningBullets.Count == 0, "Expected minimal live response to keep empty reasoning bullets.");
    Assert(result.ReplayAdvice.ReasoningBullets.Count == 0, "Expected minimal replay response to keep empty reasoning bullets.");
    Assert(result.LiveAdvice.KnowledgeRefs.Count == 0, "Expected minimal live response to keep empty knowledge refs.");
    Assert(result.ReplayAdvice.KnowledgeRefs.Count == 0, "Expected minimal replay response to keep empty knowledge refs.");
    Assert(result.LiveAdvice.MissingInformation.Count == 0, "Expected minimal live response to keep empty missing-information output.");
    Assert(result.ReplayAdvice.MissingInformation.Count == 0, "Expected minimal replay response to keep empty missing-information output.");
    Assert(result.LiveAdvice.DecisionBlockers.Count == 0, "Expected valid minimal live response to avoid synthetic blockers.");
    Assert(result.ReplayAdvice.DecisionBlockers.Count == 0, "Expected valid minimal replay response to avoid synthetic blockers.");
    Assert(result.LiveAdvice.RewardRecommendationTrace is not null, "Expected minimal live response to attach deterministic trace.");
    Assert(result.ReplayAdvice.RewardRecommendationTrace is not null, "Expected minimal replay response to attach deterministic trace.");
    Assert(
        result.LiveAdvice.RewardRecommendationTrace!.MissingInformation.Contains(scenario.ExpectedMissingInformation!, StringComparer.Ordinal),
        "Expected deterministic trace to preserve missing-information inputs without copying them into model output.");
    Assert(
        result.ReplayAdvice.RewardRecommendationTrace!.MissingInformation.Contains(scenario.ExpectedMissingInformation!, StringComparer.Ordinal),
        "Expected replay deterministic trace to preserve missing-information inputs without copying them into model output.");

    var invalidRawResponse = new Sts2AiCompanion.Foundation.Contracts.AdviceResponse(
        "ok",
        "invalid-label",
        "invalid-label",
        "invalid-label",
        "존재하지 않는 카드",
        new[] { "model-owned-reason" },
        Array.Empty<string>(),
        new[] { "model-owned-missing" },
        Array.Empty<string>(),
        0.5,
        new[] { "model-owned-ref" },
        DateTimeOffset.UtcNow,
        result.LivePromptPack.RunId,
        result.LivePromptPack.TriggerKind,
        null,
        "{}");
    var invalidFinalized = Sts2AiCompanion.Foundation.Reasoning.RewardAdviceResponseFinalizer.Apply(
        ToFoundationAdviceInputPack(result.LivePromptPack),
        invalidRawResponse);

    Assert(invalidFinalized.RecommendedChoiceLabel is null, "Expected invalid recommendation labels to be cleared.");
    Assert(invalidFinalized.DecisionBlockers.Contains("recommended-choice-not-in-option-set", StringComparer.Ordinal), "Expected invalid recommendation labels to add a blocker.");
    Assert(invalidFinalized.ReasoningBullets.SequenceEqual(invalidRawResponse.ReasoningBullets, StringComparer.Ordinal), "Expected finalizer not to rewrite model reasoning bullets.");
    Assert(invalidFinalized.MissingInformation.SequenceEqual(invalidRawResponse.MissingInformation, StringComparer.Ordinal), "Expected finalizer not to backfill missing-information output.");
    Assert(invalidFinalized.KnowledgeRefs.SequenceEqual(invalidRawResponse.KnowledgeRefs, StringComparer.Ordinal), "Expected finalizer not to backfill knowledge refs.");
    Assert(invalidFinalized.RewardRecommendationTrace is not null, "Expected invalid recommendation labels to still carry deterministic trace.");
}

static void TestRewardModelRationaleArtifacts()
{
    foreach (var scenario in CreateCuratedRewardScenarios())
    {
        var result = ExecuteRewardScenario(scenario, FakeCodexRewardResponseMode.ModelRationaleOutput);

        Assert(result.LiveAdvice.RecommendedChoiceLabel == scenario.ExpectedChoiceLabel, $"Expected live recommendation '{scenario.ExpectedChoiceLabel}' for scenario {scenario.Name}.");
        Assert(result.ReplayAdvice.RecommendedChoiceLabel == scenario.ExpectedChoiceLabel, $"Expected replay recommendation '{scenario.ExpectedChoiceLabel}' for scenario {scenario.Name}.");
        Assert(result.LivePromptPack.RewardOptionSet!.Options.Select(option => option.Label).Contains(result.LiveAdvice.RecommendedChoiceLabel, StringComparer.Ordinal), $"Expected live recommendation to stay inside visible option set for scenario {scenario.Name}.");
        Assert(result.ReplayPromptPack.RewardOptionSet!.Options.Select(option => option.Label).Contains(result.ReplayAdvice.RecommendedChoiceLabel, StringComparer.Ordinal), $"Expected replay recommendation to stay inside visible option set for scenario {scenario.Name}.");
        Assert(result.LiveAdvice.ReasoningBullets.Count > 0, $"Expected live model rationale to survive for scenario {scenario.Name}.");
        Assert(result.ReplayAdvice.ReasoningBullets.Count > 0, $"Expected replay model rationale to survive for scenario {scenario.Name}.");
        Assert(result.LiveAdvice.KnowledgeRefs.Count > 0, $"Expected live model rationale to carry knowledge refs for scenario {scenario.Name}.");
        Assert(result.ReplayAdvice.KnowledgeRefs.Count > 0, $"Expected replay model rationale to carry knowledge refs for scenario {scenario.Name}.");
        Assert(result.LiveAdvice.RewardRecommendationTrace is not null, $"Expected live deterministic trace for scenario {scenario.Name}.");
        Assert(result.ReplayAdvice.RewardRecommendationTrace is not null, $"Expected replay deterministic trace for scenario {scenario.Name}.");
        Assert(result.LiveMarkdown.Contains("## Reward Deterministic Trace", StringComparison.Ordinal), $"Expected live markdown trace section for scenario {scenario.Name}.");
        Assert(result.ReplayMarkdown.Contains("## Reward Deterministic Trace", StringComparison.Ordinal), $"Expected replay markdown trace section for scenario {scenario.Name}.");
        Assert(result.LiveMarkdown.Contains(scenario.ExpectedChoiceLabel, StringComparison.Ordinal), $"Expected live markdown to mention the recommended reward for scenario {scenario.Name}.");
        Assert(result.ReplayMarkdown.Contains(scenario.ExpectedChoiceLabel, StringComparison.Ordinal), $"Expected replay markdown to mention the recommended reward for scenario {scenario.Name}.");
        Assert(result.LiveAdvice.ReasoningBullets.SequenceEqual(result.ReplayAdvice.ReasoningBullets, StringComparer.Ordinal), $"Expected live and replay reasoning parity for scenario {scenario.Name}.");
        Assert(result.LiveAdvice.MissingInformation.SequenceEqual(result.ReplayAdvice.MissingInformation, StringComparer.Ordinal), $"Expected live and replay missing-information parity for scenario {scenario.Name}.");
        Assert(result.LiveAdvice.DecisionBlockers.SequenceEqual(result.ReplayAdvice.DecisionBlockers, StringComparer.Ordinal), $"Expected live and replay decision-blocker parity for scenario {scenario.Name}.");
        Assert(result.LiveAdvice.KnowledgeRefs.SequenceEqual(result.ReplayAdvice.KnowledgeRefs, StringComparer.Ordinal), $"Expected live and replay knowledge-ref parity for scenario {scenario.Name}.");
        AssertRewardPromptPackParity(result, scenario.Name);
        AssertReviewerReadableRewardAdvice(result.LiveAdvice, result.LivePromptPack, scenario.Name, "live");
        AssertReviewerReadableRewardAdvice(result.ReplayAdvice, result.ReplayPromptPack, scenario.Name, "replay");
        Assert(result.LiveAdvice.DecisionBlockers.Count == 0, $"Expected live decision blockers to stay empty for scenario {scenario.Name}.");
        Assert(result.ReplayAdvice.DecisionBlockers.Count == 0, $"Expected replay decision blockers to stay empty for scenario {scenario.Name}.");

        if (!string.IsNullOrWhiteSpace(scenario.ExpectedRationaleContains))
        {
            Assert(result.LiveAdvice.ReasoningBullets.Any(bullet => bullet.Contains(scenario.ExpectedRationaleContains, StringComparison.Ordinal)), $"Expected live reasoning to contain '{scenario.ExpectedRationaleContains}' for scenario {scenario.Name}.");
            Assert(result.ReplayAdvice.ReasoningBullets.Any(bullet => bullet.Contains(scenario.ExpectedRationaleContains, StringComparison.Ordinal)), $"Expected replay reasoning to contain '{scenario.ExpectedRationaleContains}' for scenario {scenario.Name}.");
        }

        if (!string.IsNullOrWhiteSpace(scenario.ExpectedMissingInformation))
        {
            Assert(result.LiveAdvice.MissingInformation.Contains(scenario.ExpectedMissingInformation, StringComparer.Ordinal), $"Expected live missing info '{scenario.ExpectedMissingInformation}' for scenario {scenario.Name}.");
            Assert(result.ReplayAdvice.MissingInformation.Contains(scenario.ExpectedMissingInformation, StringComparer.Ordinal), $"Expected replay missing info '{scenario.ExpectedMissingInformation}' for scenario {scenario.Name}.");
        }
    }
}

static void TestRewardNonFirstChoiceScenario()
{
    var scenario = CreateCuratedRewardScenarios().Single(candidate => candidate.ExpectNonFirstChoice);
    var result = ExecuteRewardScenario(scenario, FakeCodexRewardResponseMode.ModelRationaleOutput);

    Assert(!string.Equals(scenario.Choices[0].Label, scenario.ExpectedChoiceLabel, StringComparison.Ordinal), $"Expected scenario {scenario.Name} to require a non-first recommendation.");
    Assert(result.LiveAdvice.RecommendedChoiceLabel == scenario.ExpectedChoiceLabel, $"Expected live recommendation '{scenario.ExpectedChoiceLabel}' for scenario {scenario.Name}.");
    Assert(result.ReplayAdvice.RecommendedChoiceLabel == scenario.ExpectedChoiceLabel, $"Expected replay recommendation '{scenario.ExpectedChoiceLabel}' for scenario {scenario.Name}.");
}

static RewardScenarioExecutionResult ExecuteRewardScenario(RewardScenarioDefinition scenario, FakeCodexRewardResponseMode rewardResponseMode)
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateCompanionHostTestConfiguration(root, collectorModeEnabled: false);
        SeedRewardKnowledgeCatalog(root);

        var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        Directory.CreateDirectory(layout.LiveRoot);

        var snapshot = CreateRewardSnapshotForScenario(scenario, scenario.RunId);
        var session = new LiveExportSession($"reward-session-{scenario.RunId}", scenario.RunId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, layout.LiveRoot, "choice-list-presented", "rewards");
        var events = CreateRewardEventsForScenario(scenario.RunId, scenario);

        WriteJson(layout.SnapshotPath, snapshot);
        WriteJson(layout.SessionPath, session);
        File.WriteAllText(layout.SummaryPath, $"reward summary :: {scenario.Name}", Encoding.UTF8);
        Directory.CreateDirectory(Path.GetDirectoryName(layout.EventsPath)!);
        File.WriteAllText(layout.EventsPath, string.Join(Environment.NewLine, events.Select(SerializeNdjson)) + Environment.NewLine, Encoding.UTF8);
        File.WriteAllText(layout.RawObservationsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ScreenTransitionsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceCandidatesPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceDecisionsPath, string.Empty, Encoding.UTF8);
        Directory.CreateDirectory(layout.SemanticSnapshotsRoot);

        var fakeClient = new FakeCodexSessionClient(rewardResponseMode);
        var host = new CompanionHost(configuration, root, fakeClient);
        try
        {
            host.RefreshAsync().GetAwaiter().GetResult();
            Assert(fakeClient.RequestCount == 1, $"Expected reward refresh to trigger automatic advice for scenario {scenario.Name}.");
            Assert(host.CurrentSnapshot.LatestAdvice is not null, $"Expected live advice artifact for scenario {scenario.Name}.");

            var liveAdvicePath = host.CurrentSnapshot.Paths.AdviceLatestJsonPath
                ?? throw new InvalidOperationException("Expected live advice json path.");
            var liveMarkdownPath = host.CurrentSnapshot.Paths.AdviceLatestMarkdownPath
                ?? throw new InvalidOperationException("Expected live advice markdown path.");
            var liveAdvice = JsonSerializer.Deserialize<AdviceResponse>(File.ReadAllText(liveAdvicePath), ConfigurationLoader.JsonOptions)
                ?? throw new InvalidOperationException("Expected live reward advice response.");
            var livePromptPackPath = Directory.GetFiles(host.CurrentSnapshot.Paths.PromptPacksRoot!, "*.json", SearchOption.TopDirectoryOnly)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .First();
            var livePromptPack = JsonSerializer.Deserialize<AdviceInputPack>(File.ReadAllText(livePromptPackPath), ConfigurationLoader.JsonOptions)
                ?? throw new InvalidOperationException("Expected live reward prompt pack.");
            var liveMarkdown = File.ReadAllText(liveMarkdownPath, Encoding.UTF8);

            var fixtureRoot = Path.Combine(root, $"reward-fixture-{scenario.Name}");
            var liveMirrorRoot = Path.Combine(fixtureRoot, "live-mirror");
            Directory.CreateDirectory(liveMirrorRoot);
            WriteJson(Path.Combine(liveMirrorRoot, configuration.LiveExport.SnapshotFileName), snapshot);
            WriteJson(Path.Combine(liveMirrorRoot, configuration.LiveExport.SessionFileName), session);
            File.WriteAllText(Path.Combine(liveMirrorRoot, configuration.LiveExport.SummaryFileName), $"reward summary :: {scenario.Name}", Encoding.UTF8);
            File.WriteAllText(Path.Combine(liveMirrorRoot, configuration.LiveExport.EventsFileName), string.Join(Environment.NewLine, events.Select(SerializeNdjson)) + Environment.NewLine, Encoding.UTF8);

            var mockResponsePath = Path.Combine(root, $"{scenario.Name}-reward-mock-response.json");
            WriteJson(mockResponsePath, ToFoundationAdviceResponse(fakeClient.BuildRawResponseForTesting(livePromptPack)));

            var replayValidator = new FoundationReplayAdvisorValidator(configuration, root);
            var replayResult = replayValidator.ValidateAsync(fixtureRoot, mockResponsePath, CancellationToken.None).GetAwaiter().GetResult();
            var replayPromptPack = JsonSerializer.Deserialize<Sts2AiCompanion.Foundation.Contracts.AdviceInputPack>(File.ReadAllText(replayResult.PromptPackPath), ConfigurationLoader.JsonOptions)
                ?? throw new InvalidOperationException("Expected replay reward prompt pack.");
            var replayAdvice = JsonSerializer.Deserialize<Sts2AiCompanion.Foundation.Contracts.AdviceResponse>(File.ReadAllText(replayResult.AdviceJsonPath), ConfigurationLoader.JsonOptions)
                ?? throw new InvalidOperationException("Expected replay reward advice response.");
            var replayMarkdown = File.ReadAllText(replayResult.AdviceMarkdownPath, Encoding.UTF8);

            return new RewardScenarioExecutionResult(
                livePromptPack,
                liveAdvice,
                liveMarkdown,
                ToHostAdviceInputPack(replayPromptPack),
                ToHostAdviceResponse(replayAdvice),
                replayMarkdown);
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

static void AssertRewardPromptPackParity(RewardScenarioExecutionResult result, string scenarioName)
{
    Assert(result.LivePromptPack.RewardOptionSet is not null, $"Expected live reward option set for scenario {scenarioName}.");
    Assert(result.LivePromptPack.RewardAssessmentFacts is not null, $"Expected live reward assessment facts for scenario {scenarioName}.");
    Assert(result.LivePromptPack.RewardRecommendationTraceSeed is not null, $"Expected live reward trace seed for scenario {scenarioName}.");
    Assert(result.ReplayPromptPack.RewardOptionSet is not null, $"Expected replay reward option set for scenario {scenarioName}.");
    Assert(result.ReplayPromptPack.RewardAssessmentFacts is not null, $"Expected replay reward assessment facts for scenario {scenarioName}.");
    Assert(result.ReplayPromptPack.RewardRecommendationTraceSeed is not null, $"Expected replay reward trace seed for scenario {scenarioName}.");

    var liveOptionSet = result.LivePromptPack.RewardOptionSet!;
    var replayOptionSet = result.ReplayPromptPack.RewardOptionSet!;
    Assert(string.Equals(liveOptionSet.SceneType, replayOptionSet.SceneType, StringComparison.Ordinal), $"Expected reward scene types to match for scenario {scenarioName}.");
    Assert(liveOptionSet.SkipAllowed == replayOptionSet.SkipAllowed, $"Expected skip visibility to match for scenario {scenarioName}.");
    Assert(string.Equals(liveOptionSet.SummaryText, replayOptionSet.SummaryText, StringComparison.Ordinal), $"Expected reward option summaries to match for scenario {scenarioName}.");
    Assert(
        liveOptionSet.Options.Select(option => (option.Ordinal, option.Kind, option.Label, option.Value, option.Description, option.IsSkipOption))
            .SequenceEqual(replayOptionSet.Options.Select(option => (option.Ordinal, option.Kind, option.Label, option.Value, option.Description, option.IsSkipOption))),
        $"Expected reward option payloads to match for scenario {scenarioName}.");

    var liveFacts = result.LivePromptPack.RewardAssessmentFacts!;
    var replayFacts = result.ReplayPromptPack.RewardAssessmentFacts!;
    Assert(string.Equals(liveFacts.KnowledgeFingerprint, replayFacts.KnowledgeFingerprint, StringComparison.Ordinal), $"Expected knowledge fingerprints to match for scenario {scenarioName}.");
    Assert(liveFacts.DeckSize == replayFacts.DeckSize, $"Expected deck size parity for scenario {scenarioName}.");
    Assert(liveFacts.AttackCount == replayFacts.AttackCount, $"Expected attack count parity for scenario {scenarioName}.");
    Assert(liveFacts.SkillCount == replayFacts.SkillCount, $"Expected skill count parity for scenario {scenarioName}.");
    Assert(liveFacts.PowerCount == replayFacts.PowerCount, $"Expected power count parity for scenario {scenarioName}.");
    Assert(liveFacts.DrawTaggedCardCount == replayFacts.DrawTaggedCardCount, $"Expected draw support count parity for scenario {scenarioName}.");
    Assert(liveFacts.BlockTaggedCardCount == replayFacts.BlockTaggedCardCount, $"Expected block support count parity for scenario {scenarioName}.");
    Assert(liveFacts.EnergyTaggedCardCount == replayFacts.EnergyTaggedCardCount, $"Expected energy support count parity for scenario {scenarioName}.");
    Assert(string.Equals(liveFacts.AttackPressure, replayFacts.AttackPressure, StringComparison.Ordinal), $"Expected attack pressure parity for scenario {scenarioName}.");
    Assert(string.Equals(liveFacts.DefensePressure, replayFacts.DefensePressure, StringComparison.Ordinal), $"Expected defense pressure parity for scenario {scenarioName}.");
    Assert(string.Equals(liveFacts.DrawSupportLevel, replayFacts.DrawSupportLevel, StringComparison.Ordinal), $"Expected draw support parity for scenario {scenarioName}.");
    Assert(string.Equals(liveFacts.EnergySupportLevel, replayFacts.EnergySupportLevel, StringComparison.Ordinal), $"Expected energy support parity for scenario {scenarioName}.");
    Assert(liveFacts.SynergyHints.SequenceEqual(replayFacts.SynergyHints, StringComparer.Ordinal), $"Expected synergy hints parity for scenario {scenarioName}.");
    Assert(liveFacts.AntiSynergyHints.SequenceEqual(replayFacts.AntiSynergyHints, StringComparer.Ordinal), $"Expected anti-synergy hints parity for scenario {scenarioName}.");
    Assert(liveFacts.MissingInformation.SequenceEqual(replayFacts.MissingInformation, StringComparer.Ordinal), $"Expected missing-information parity for scenario {scenarioName}.");
    Assert(liveFacts.FactLines.SequenceEqual(replayFacts.FactLines, StringComparer.Ordinal), $"Expected fact-line parity for scenario {scenarioName}.");
    Assert(liveFacts.KnowledgeRefs.SequenceEqual(replayFacts.KnowledgeRefs, StringComparer.Ordinal), $"Expected knowledge-ref parity for scenario {scenarioName}.");

    var liveTrace = result.LivePromptPack.RewardRecommendationTraceSeed!;
    var replayTrace = result.ReplayPromptPack.RewardRecommendationTraceSeed!;
    Assert(string.Equals(liveTrace.KnowledgeFingerprint, replayTrace.KnowledgeFingerprint, StringComparison.Ordinal), $"Expected trace knowledge fingerprints to match for scenario {scenarioName}.");
    Assert(liveTrace.CandidateLabels.SequenceEqual(replayTrace.CandidateLabels, StringComparer.Ordinal), $"Expected trace candidate labels to match for scenario {scenarioName}.");
    Assert(liveTrace.AssessmentFactLines.SequenceEqual(replayTrace.AssessmentFactLines, StringComparer.Ordinal), $"Expected trace fact lines to match for scenario {scenarioName}.");
    Assert(liveTrace.InputKnowledgeRefs.SequenceEqual(replayTrace.InputKnowledgeRefs, StringComparer.Ordinal), $"Expected trace knowledge refs to match for scenario {scenarioName}.");
    Assert(liveTrace.MissingInformation.SequenceEqual(replayTrace.MissingInformation, StringComparer.Ordinal), $"Expected trace missing-information flags to match for scenario {scenarioName}.");
}

static void AssertReviewerReadableRewardAdvice(AdviceResponse advice, AdviceInputPack promptPack, string scenarioName, string channel)
{
    var recommendedChoiceLabel = advice.RecommendedChoiceLabel
        ?? throw new InvalidOperationException($"Expected {channel} advice to include a recommendation for scenario {scenarioName}.");
    var optionSet = promptPack.RewardOptionSet
        ?? throw new InvalidOperationException($"Expected {channel} prompt pack option set for scenario {scenarioName}.");
    var trace = advice.RewardRecommendationTrace
        ?? throw new InvalidOperationException($"Expected {channel} advice to include a deterministic trace for scenario {scenarioName}.");

    Assert(optionSet.Options.Select(option => option.Label).Contains(recommendedChoiceLabel, StringComparer.Ordinal), $"Expected {channel} recommendation to stay inside visible option set for scenario {scenarioName}.");
    Assert(advice.ReasoningBullets.Count >= 2, $"Expected {channel} advice to provide at least two reasoning bullets for scenario {scenarioName}.");
    Assert(advice.ReasoningBullets.All(bullet => !string.IsNullOrWhiteSpace(bullet)), $"Expected {channel} reasoning bullets to stay non-empty for scenario {scenarioName}.");
    Assert(advice.ReasoningBullets.Any(bullet => bullet.Contains(recommendedChoiceLabel, StringComparison.Ordinal)), $"Expected {channel} reasoning to mention the recommended card for scenario {scenarioName}.");
    Assert(
        advice.ReasoningBullets.Any(bullet =>
            bullet.Contains("deterministic 사실:", StringComparison.Ordinal)
            || bullet.Contains("attack_pressure=", StringComparison.Ordinal)
            || bullet.Contains("defense_pressure=", StringComparison.Ordinal)
            || bullet.Contains("draw_support=", StringComparison.Ordinal)
            || bullet.Contains("energy_support=", StringComparison.Ordinal)),
        $"Expected {channel} reasoning to cite deterministic facts for scenario {scenarioName}.");
    Assert(trace.CandidateLabels.Count > 0, $"Expected {channel} trace to expose candidate labels for scenario {scenarioName}.");
    Assert(trace.AssessmentFactLines.Count > 0, $"Expected {channel} trace to expose deterministic fact lines for scenario {scenarioName}.");
    Assert(trace.InputKnowledgeRefs.Count > 0, $"Expected {channel} trace to expose deterministic knowledge refs for scenario {scenarioName}.");
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

static ScaffoldConfiguration CreateRewardTestConfiguration(string root)
{
    return CreateCompanionHostTestConfiguration(root, collectorModeEnabled: false);
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

static RewardScenarioDefinition CreateDefaultRewardScenario()
{
    return new RewardScenarioDefinition(
        "attack-gap-duplicate-block",
        "reward-live-run",
        new[]
        {
            new LiveExportCardSummary("Strike", "strike", 1, "Attack", false),
            new LiveExportCardSummary("Defend", "defend", 1, "Skill", false),
            new LiveExportCardSummary("수비 강화", "shrug-it-off", 1, "Skill", false),
        },
        new[]
        {
            new LiveExportChoiceSummary("card", "몸통 박치기", "bash", "적에게 피해를 주고 취약을 겁니다."),
            new LiveExportChoiceSummary("card", "수비 강화", "shrug-it-off", "방어도를 얻고 카드를 뽑습니다."),
            new LiveExportChoiceSummary("skip", "넘기기", "skip", "보상을 건너뜁니다."),
        },
        "몸통 박치기");
}

static IReadOnlyList<RewardScenarioDefinition> CreateCuratedRewardScenarios()
{
    return new[]
    {
        CreateDefaultRewardScenario(),
        new RewardScenarioDefinition(
            "defense-gap",
            "reward-defense-gap-run",
            new[]
            {
                new LiveExportCardSummary("Strike", "strike", 1, "Attack", false),
                new LiveExportCardSummary("Strike+", "strike-plus", 1, "Attack", false),
                new LiveExportCardSummary("Defend", "defend", 1, "Skill", false),
            },
            new[]
            {
                new LiveExportChoiceSummary("card", "몸통 박치기", "bash", "적에게 피해를 주고 취약을 겁니다."),
                new LiveExportChoiceSummary("card", "수비 강화", "shrug-it-off", "방어도를 얻고 카드를 뽑습니다."),
                new LiveExportChoiceSummary("skip", "넘기기", "skip", "보상을 건너뜁니다."),
            },
            "수비 강화",
            null,
            "수비 강화: adds draw support",
            true),
        new RewardScenarioDefinition(
            "draw-gap-with-unknown-alternative",
            "reward-draw-gap-run",
            new[]
            {
                new LiveExportCardSummary("Strike", "strike", 1, "Attack", false),
                new LiveExportCardSummary("Defend", "defend", 1, "Skill", false),
                new LiveExportCardSummary("몸통 박치기", "bash", 2, "Attack", false),
            },
            new[]
            {
                new LiveExportChoiceSummary("card", "전투의 함성", "battle-trance", "카드를 여러 장 뽑습니다."),
                new LiveExportChoiceSummary("card", "미확인 카드", "mystery-reward", "설명이 비어 있습니다."),
                new LiveExportChoiceSummary("skip", "넘기기", "skip", "보상을 건너뜁니다."),
            },
            "전투의 함성",
            "reward-option-knowledge-missing:미확인 카드"),
    };
}

static LiveExportSnapshot CreateRewardSnapshot(string runId)
{
    return CreateRewardSnapshotForScenario(CreateDefaultRewardScenario(), runId);
}

static LiveExportSnapshot CreateRewardSnapshotForScenario(RewardScenarioDefinition scenario, string runId)
{
    return new LiveExportSnapshot(
        runId,
        "active",
        1,
        DateTimeOffset.UtcNow,
        "rewards",
        1,
        7,
        new LiveExportPlayerSummary("Ironclad", 65, 80, 120, 3, new Dictionary<string, string?>()),
        scenario.Deck,
        new[] { "Anchor" },
        Array.Empty<string>(),
        scenario.Choices,
        new[] { "trigger: reward-screen-opened" },
        Array.Empty<string>(),
        new LiveExportEncounterSummary("Card Reward", "Reward", false, null),
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["choice-source"] = "live-export",
            ["choice-extractor"] = "reward._options",
            ["currentSceneType"] = "MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NCardRewardSelectionScreen",
            ["rootTypeSummary"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NCardRewardSelectionScreen",
            ["flowScreen"] = "rewards",
            ["visibleScreen"] = "rewards",
            ["reward-type"] = "card",
        });
}

static IReadOnlyList<LiveExportEventEnvelope> CreateRewardEvents(string runId)
{
    return CreateRewardEventsForScenario(runId, CreateDefaultRewardScenario());
}

static IReadOnlyList<LiveExportEventEnvelope> CreateRewardEventsForScenario(string runId, RewardScenarioDefinition scenario)
{
    return new[]
    {
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow.AddSeconds(-1),
            1,
            runId,
            "reward-screen-opened",
            "rewards",
            1,
            7,
            new Dictionary<string, object?>
            {
                ["choiceCount"] = scenario.Choices.Count,
            }),
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow,
            2,
            runId,
            "choice-list-presented",
            "rewards",
            1,
            7,
            new Dictionary<string, object?>
            {
                ["choices"] = scenario.Choices.Select(choice => choice.Label).ToArray(),
            }),
    };
}

static CompanionRunState CreateRewardRunState(string runId)
{
    return CreateRewardRunStateForScenario(CreateDefaultRewardScenario(), runId);
}

static CompanionRunState CreateRewardRunStateForScenario(RewardScenarioDefinition scenario, string runId)
{
    var snapshot = CreateRewardSnapshotForScenario(scenario, runId);
    var session = new LiveExportSession("reward-session", runId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(Path.GetTempPath(), "reward-live"), "choice-list-presented", "rewards");
    var events = CreateRewardEventsForScenario(runId, scenario);
    return new CompanionRunState(snapshot, session, "reward summary", events, false)
    {
        NormalizedState = CompanionStateMapper.FromLiveExport(snapshot, session, events),
    };
}

static LiveExportSnapshot CreateNonCardRewardSnapshot(string runId)
{
    return new LiveExportSnapshot(
        runId,
        "active",
        1,
        DateTimeOffset.UtcNow,
        "rewards",
        1,
        7,
        new LiveExportPlayerSummary("Ironclad", 65, 80, 120, 3, new Dictionary<string, string?>()),
        new[]
        {
            new LiveExportCardSummary("Strike", "strike", 1, "Attack", false),
            new LiveExportCardSummary("Defend", "defend", 1, "Skill", false),
        },
        new[] { "Anchor" },
        Array.Empty<string>(),
        new[]
        {
            new LiveExportChoiceSummary("relic", "고대 찻잔", "ancient-tea-set", "휴식 후 에너지를 얻습니다."),
            new LiveExportChoiceSummary("relic", "등불", "lantern", "전투 시작 시 에너지를 얻습니다."),
            new LiveExportChoiceSummary("skip", "넘기기", "skip", "보상을 건너뜁니다."),
        },
        new[] { "trigger: reward-screen-opened" },
        Array.Empty<string>(),
        new LiveExportEncounterSummary("Relic Reward", "Reward", false, null),
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["choice-source"] = "live-export",
            ["choice-extractor"] = "reward._options",
            ["currentSceneType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
            ["rootTypeSummary"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
            ["flowScreen"] = "rewards",
            ["visibleScreen"] = "rewards",
            ["reward-type"] = "relic",
        });
}

static CompanionRunState CreateNonCardRewardRunState(string runId)
{
    var snapshot = CreateNonCardRewardSnapshot(runId);
    var session = new LiveExportSession("reward-session", runId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(Path.GetTempPath(), "reward-live"), "choice-list-presented", "rewards");
    var events = new[]
    {
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow.AddSeconds(-1),
            1,
            runId,
            "reward-screen-opened",
            "rewards",
            1,
            7,
            new Dictionary<string, object?>
            {
                ["choiceCount"] = snapshot.CurrentChoices.Count,
            }),
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow,
            2,
            runId,
            "choice-list-presented",
            "rewards",
            1,
            7,
            new Dictionary<string, object?>
            {
                ["choices"] = snapshot.CurrentChoices.Select(choice => choice.Label).ToArray(),
            }),
    };
    return new CompanionRunState(snapshot, session, "reward summary", events, false)
    {
        NormalizedState = CompanionStateMapper.FromLiveExport(snapshot, session, events),
    };
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

static void SeedRewardKnowledgeCatalog(string root)
{
    var knowledgeRoot = Path.Combine(root, "artifacts", "knowledge");
    Directory.CreateDirectory(knowledgeRoot);
    var catalog = new StaticKnowledgeCatalog(
        DateTimeOffset.UtcNow,
        new StaticKnowledgeMetadata("reward-v1", "reward-test", DateTimeOffset.UtcNow, new Dictionary<string, string?>()),
        new[]
        {
            new StaticKnowledgeEntry(
                "bash",
                "몸통 박치기",
                "observed-merge",
                true,
                "적에게 피해를 주고 취약을 겁니다.",
                new[] { "card", "attack" },
                new Dictionary<string, string?>(),
                Array.Empty<StaticKnowledgeOption>()),
            new StaticKnowledgeEntry(
                "shrug-it-off",
                "수비 강화",
                "observed-merge",
                true,
                "방어도를 얻고 카드를 뽑습니다.",
                new[] { "card", "skill" },
                new Dictionary<string, string?>(),
                Array.Empty<StaticKnowledgeOption>()),
            new StaticKnowledgeEntry(
                "battle-trance",
                "전투의 함성",
                "observed-merge",
                true,
                "카드 3장을 뽑습니다.",
                new[] { "card", "skill" },
                new Dictionary<string, string?>(),
                Array.Empty<StaticKnowledgeOption>()),
            new StaticKnowledgeEntry(
                "strike",
                "Strike",
                "observed-merge",
                true,
                "Deal damage.",
                new[] { "card", "attack" },
                new Dictionary<string, string?>(),
                Array.Empty<StaticKnowledgeOption>()),
            new StaticKnowledgeEntry(
                "defend",
                "Defend",
                "observed-merge",
                true,
                "Gain Block.",
                new[] { "card", "skill" },
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
        Array.Empty<StaticKnowledgeEntry>(),
        new[]
        {
            new StaticKnowledgeEntry(
                "card-reward-context",
                "Card Reward",
                "strict-domain-scan",
                true,
                "Reward screen context.",
                new[] { "reward" },
                new Dictionary<string, string?>(),
                Array.Empty<StaticKnowledgeOption>()),
        },
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

static Sts2AiCompanion.Foundation.Contracts.AdviceInputPack ToFoundationAdviceInputPack(AdviceInputPack inputPack)
{
    return new Sts2AiCompanion.Foundation.Contracts.AdviceInputPack(
        inputPack.RunId,
        inputPack.TriggerKind,
        inputPack.CreatedAt,
        inputPack.Manual,
        inputPack.CurrentScreen,
        inputPack.SummaryText,
        inputPack.Snapshot,
        inputPack.RecentEvents.ToArray(),
        inputPack.KnowledgeEntries.ToArray(),
        inputPack.KnowledgeReasons.ToArray(),
        inputPack.ConstraintsText,
        inputPack.NormalizedState,
        inputPack.RewardOptionSet,
        inputPack.RewardAssessmentFacts,
        inputPack.RewardRecommendationTraceSeed);
}

static AdviceInputPack ToHostAdviceInputPack(Sts2AiCompanion.Foundation.Contracts.AdviceInputPack inputPack)
{
    return new AdviceInputPack(
        inputPack.RunId,
        inputPack.TriggerKind,
        inputPack.CreatedAt,
        inputPack.Manual,
        inputPack.CurrentScreen,
        inputPack.SummaryText,
        inputPack.Snapshot,
        inputPack.RecentEvents.ToArray(),
        inputPack.KnowledgeEntries.ToArray(),
        inputPack.KnowledgeReasons.ToArray(),
        inputPack.ConstraintsText,
        inputPack.NormalizedState,
        inputPack.RewardOptionSet,
        inputPack.RewardAssessmentFacts,
        inputPack.RewardRecommendationTraceSeed);
}

static Sts2AiCompanion.Foundation.Contracts.AdviceResponse ToFoundationAdviceResponse(AdviceResponse response)
{
    return new Sts2AiCompanion.Foundation.Contracts.AdviceResponse(
        response.Status,
        response.Headline,
        response.Summary,
        response.RecommendedAction,
        response.RecommendedChoiceLabel,
        response.ReasoningBullets.ToArray(),
        response.RiskNotes.ToArray(),
        response.MissingInformation.ToArray(),
        response.DecisionBlockers.ToArray(),
        response.Confidence,
        response.KnowledgeRefs.ToArray(),
        response.GeneratedAt,
        response.RunId,
        response.TriggerKind,
        response.SessionId,
        response.RawResponse,
        response.RewardRecommendationTrace);
}

static AdviceResponse ToHostAdviceResponse(Sts2AiCompanion.Foundation.Contracts.AdviceResponse response)
{
    return new AdviceResponse(
        response.Status,
        response.Headline,
        response.Summary,
        response.RecommendedAction,
        response.RecommendedChoiceLabel,
        response.ReasoningBullets.ToArray(),
        response.RiskNotes.ToArray(),
        response.MissingInformation.ToArray(),
        response.DecisionBlockers.ToArray(),
        response.Confidence,
        response.KnowledgeRefs.ToArray(),
        response.GeneratedAt,
        response.RunId,
        response.TriggerKind,
        response.SessionId,
        response.RawResponse,
        response.RewardRecommendationTrace);
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

static object CreateRuntimeHookBinding(System.Reflection.MethodInfo method, string semanticKind, string? screenOverride)
{
    var runtimeAssembly = typeof(AiCompanionModEntryPoint).Assembly;
    var candidateType = runtimeAssembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeHookCandidate");
    var bindingType = runtimeAssembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeHookBinding");
    Assert(candidateType is not null, "Expected runtime hook candidate type to exist.");
    Assert(bindingType is not null, "Expected runtime hook binding type to exist.");

    var candidate = Activator.CreateInstance(
        candidateType!,
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
        binder: null,
        args: new object?[] { "combat", semanticKind, method.DeclaringType!.FullName!, new[] { method.Name }, screenOverride },
        culture: null);
    Assert(candidate is not null, "Expected runtime hook candidate to be constructible.");

    var binding = Activator.CreateInstance(
        bindingType!,
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
        binder: null,
        args: new object?[] { candidate, method },
        culture: null);
    Assert(binding is not null, "Expected runtime hook binding to be constructible.");
    return binding!;
}

static LiveExportObservation BuildRuntimeObservationForSelfTest(IEnumerable<object> roots, string? screenOverride)
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod("BuildObservation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(method is not null, "Expected private BuildObservation helper.");

    var observation = method!.Invoke(
        null,
        new object?[]
        {
            "runtime-poll",
            DateTimeOffset.UtcNow,
            roots.ToArray(),
            AiCompanionRuntimeConfig.Defaults,
            screenOverride,
            null,
            null,
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase),
            new string?[] { screenOverride },
        }) as LiveExportObservation;
    Assert(observation is not null, "Expected runtime BuildObservation self-test helper to return an observation.");
    return observation!;
}

static object? ReadProperty(object target, string propertyName)
{
    return target.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)?.GetValue(target);
}

static LiveExportSnapshot CreateInventoryPublisherSnapshot(IReadOnlyList<LiveExportChoiceSummary> choices)
{
    return new LiveExportSnapshot(
        "run-inventory-self-test",
        "active",
        1,
        DateTimeOffset.UtcNow,
        "map",
        null,
        null,
        LiveExportPlayerSummary.Empty,
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        choices,
        Array.Empty<string>(),
        Array.Empty<string>(),
        null,
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase));
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

file sealed class FakeRewardButton
{
    public object? Reward { get; init; }

    public object? _label { get; init; }

    public object? Position { get; init; }

    public object? Size { get; init; }
}

file sealed class FakeCardSelectorPrefs
{
    public FakeCardSelectorPrefs(FakeLocString prompt, int minSelect, int maxSelect, bool requireManualConfirmation, bool cancelable)
    {
        Prompt = prompt;
        MinSelect = minSelect;
        MaxSelect = maxSelect;
        RequireManualConfirmation = requireManualConfirmation;
        Cancelable = cancelable;
    }

    public object Prompt { get; }

    public int MinSelect { get; }

    public int MaxSelect { get; }

    public bool RequireManualConfirmation { get; }

    public bool Cancelable { get; }
}

file sealed class FakeNDeckTransformSelectScreen
{
    public bool Visible { get; init; }

    public object? _prefs { get; init; }

    public object[]? _selectedCards { get; init; }

    public object? _confirmButton { get; init; }

    public object? _previewContainer { get; init; }

    public object? _previewConfirmButton { get; init; }

    public object? _grid { get; init; }
}

file sealed class FakeNDeckCardSelectScreen
{
    public bool Visible { get; init; }

    public object? _prefs { get; init; }

    public object[]? _selectedCards { get; init; }

    public object? _confirmButton { get; init; }

    public object? _previewContainer { get; init; }

    public object? _previewConfirmButton { get; init; }

    public object? _grid { get; init; }
}

file sealed class FakeNCardRewardSelectionScreen
{
    public bool Visible { get; init; }

    public object? _banner { get; init; }

    public object? _cardRow { get; init; }
}

file sealed class FakeNTreasureRoom
{
    public bool Visible { get; init; }

    public bool _isRelicCollectionOpen { get; init; }

    public object? Chest { get; init; }

    public object? ProceedButton { get; init; }

    public object? _relicCollection { get; init; }
}

file sealed class FakeNMerchantRoom
{
    public bool Visible { get; init; }

    public object? Inventory { get; init; }

    public object? MerchantButton { get; init; }

    public object? ProceedButton { get; init; }
}

file sealed class FakeNMerchantInventory
{
    public bool Visible { get; init; }

    public bool IsOpen { get; init; }

    public object? _backButton { get; init; }

    public object[] Slots { get; init; } = Array.Empty<object>();

    public IEnumerable<object> GetAllSlots()
    {
        return Slots;
    }
}

file class FakeMerchantEntry
{
    public string? Id { get; init; }

    public string? Name { get; init; }

    public bool IsStocked { get; init; }

    public bool EnoughGold { get; init; }
}

file sealed class FakeMerchantCardEntry : FakeMerchantEntry
{
    public FakeCardCreationResult? CreationResult { get; init; }
}

file sealed class FakeMerchantRelicEntry : FakeMerchantEntry
{
    public FakeRelicModel? Model { get; init; }
}

file sealed class FakeMerchantPotionEntry : FakeMerchantEntry
{
    public FakePotionModel? Model { get; init; }
}

file sealed class FakeMerchantCardRemovalEntry : FakeMerchantEntry
{
    public bool Used { get; init; }
}

file abstract class FakeMerchantSlotBase
{
    protected FakeMerchantSlotBase(string label, string id, double x, double y, bool enabled)
    {
        Name = label;
        Hitbox = new FakeClickableControl
        {
            Visible = true,
            Enabled = enabled,
            Position = new FakeVector2(x, y),
            Size = new FakeVector2(180, 180),
        };
    }

    public string Name { get; }

    public object Hitbox { get; }
}

file sealed class FakeNMerchantRelic : FakeMerchantSlotBase
{
    public FakeNMerchantRelic(string label, string id, double x, double y, bool enabled, bool stocked, bool enoughGold)
        : base(label, id, x, y, enabled)
    {
        Entry = new FakeMerchantRelicEntry
        {
            Id = id,
            Name = label,
            IsStocked = stocked,
            EnoughGold = enoughGold,
            Model = new FakeRelicModel
            {
                Id = id,
                Name = label,
            },
        };
    }

    public FakeMerchantRelicEntry Entry { get; }
}

file sealed class FakeNMerchantCard : FakeMerchantSlotBase
{
    public FakeNMerchantCard(string label, string id, double x, double y, bool enabled, bool stocked, bool enoughGold)
        : base(label, id, x, y, enabled)
    {
        Entry = new FakeMerchantCardEntry
        {
            Id = id,
            Name = label,
            IsStocked = stocked,
            EnoughGold = enoughGold,
            CreationResult = new FakeCardCreationResult
            {
                Card = new FakeCardModel
                {
                    Id = id,
                    Name = label,
                },
            },
        };
    }

    public FakeMerchantCardEntry Entry { get; }
}

file sealed class FakeNMerchantPotion : FakeMerchantSlotBase
{
    public FakeNMerchantPotion(string label, string id, double x, double y, bool enabled, bool stocked, bool enoughGold)
        : base(label, id, x, y, enabled)
    {
        Entry = new FakeMerchantPotionEntry
        {
            Id = id,
            Name = label,
            IsStocked = stocked,
            EnoughGold = enoughGold,
            Model = new FakePotionModel
            {
                Id = id,
                Name = label,
            },
        };
    }

    public FakeMerchantPotionEntry Entry { get; }
}

file sealed class FakeNMerchantCardRemoval : FakeMerchantSlotBase
{
    public FakeNMerchantCardRemoval(string label, double x, double y, bool enabled, bool enoughGold, bool used)
        : base(label, "card-removal", x, y, enabled)
    {
        Entry = new FakeMerchantCardRemovalEntry
        {
            Id = "card-removal",
            Name = label,
            IsStocked = !used,
            EnoughGold = enoughGold,
            Used = used,
        };
    }

    public FakeMerchantCardRemovalEntry Entry { get; }
}

file sealed class FakeNTreasureRoomRelicCollection
{
    public object? _holdersInUse { get; init; }

    public object? SingleplayerRelicHolder { get; init; }
}

file sealed class FakeNMapScreen
{
    public bool IsOpen { get; init; }

    public bool Visible { get; init; }

    public object? _mapPointDictionary { get; init; }
}

file class FakeSceneNode
{
    public string? Name { get; init; }

    public bool Visible { get; init; }

    public bool Enabled { get; init; } = true;

    public object? Position { get; init; }

    public object? Size { get; init; }

    public object[] Children { get; init; } = Array.Empty<object>();

    public bool IsVisibleInTree()
    {
        return Visible;
    }

    public IEnumerable<object> GetChildren(bool includeInternal = false)
    {
        return Children;
    }

    public int GetChildCount(bool includeInternal = false)
    {
        return Children.Length;
    }

    public object GetChild(int index, bool includeInternal = false)
    {
        return Children[index];
    }
}

file sealed class FakeNEventRoom : FakeSceneNode
{
}

file sealed class FakeNAncientEventLayout : FakeSceneNode
{
    public bool IsDialogueOnLastLine { get; init; }

    public object? _fakeNextButton { get; init; }

    public object? _fakeNextButtonContainer { get; init; }

    public object? DefaultFocusedControl { get; init; }
}

file sealed class FakeDialogueHitboxNButton : FakeSceneNode
{
}

file sealed class FakeEventOptionModel
{
    public string? Id { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public bool IsLocked { get; init; }

    public bool IsProceed { get; init; }
}

file sealed class FakeNEventOptionButton : FakeSceneNode
{
    public FakeNEventOptionButton(int index, string title, string description, double x, double y, bool enabled, bool isProceed = false)
    {
        Index = index;
        Visible = true;
        Enabled = enabled;
        Position = new FakeVector2(x, y);
        Size = new FakeVector2(820, 92);
        Option = new FakeEventOptionModel
        {
            Id = $"option-{index}",
            Title = title,
            Description = description,
            IsLocked = false,
            IsProceed = isProceed,
        };
    }

    public int Index { get; }

    public FakeEventOptionModel Option { get; }

    public bool HasFocusState { get; set; }

    public bool HasFocus()
    {
        return HasFocusState;
    }
}

file sealed class FakeAncientPseudoChoice
{
    public FakeAncientPseudoChoice(string title, string description, double x, double y)
    {
        Option = new FakeEventOptionModel
        {
            Id = title,
            Title = title,
            Description = description,
            IsLocked = false,
        };
        Position = new FakeVector2(x, y);
        Size = new FakeVector2(1000, 100);
    }

    public FakeEventOptionModel Option { get; }

    public object Position { get; }

    public object Size { get; }
}

file sealed class FakeRestSiteOption
{
    public string? OptionId { get; init; }

    public string? Label { get; init; }
}

file class FakeNMapPoint
{
    protected FakeNMapPoint(string pointType, int row, int col, double x, double y, bool enabled, double width, double height)
    {
        IsEnabled = enabled;
        Position = new FakeVector2(x, y);
        Size = new FakeVector2(width, height);
        Point = new FakeMapPointModel(pointType, row, col);
    }

    public bool IsEnabled { get; init; }

    public object? Position { get; init; }

    public object? Size { get; init; }

    public FakeMapPointModel Point { get; init; }
}

file sealed class FakeNBossMapPoint : FakeNMapPoint
{
    public FakeNBossMapPoint(string pointType, int row, int col, double x, double y, bool enabled, double width, double height)
        : base(pointType, row, col, x, y, enabled, width, height)
    {
    }
}

file sealed class FakeNMapPointNode : FakeNMapPoint
{
    public FakeNMapPointNode(string pointType, int row, int col, double x, double y, bool enabled, double width, double height)
        : base(pointType, row, col, x, y, enabled, width, height)
    {
    }
}

file sealed class FakeMapPointModel
{
    public FakeMapPointModel(string pointType, int row, int col)
    {
        PointType = pointType;
        coord = new FakeMapCoord(row, col);
    }

    public string PointType { get; }

    public FakeMapCoord coord { get; }
}

file sealed class FakeMapCoord
{
    public FakeMapCoord(int row, int col)
    {
        this.row = row;
        this.col = col;
    }

    public int row { get; }

    public int col { get; }
}

file sealed class FakeNRewardsScreen
{
    public bool Visible { get; init; }

    public object? _proceedButton { get; init; }

    public object[] _rewardButtons { get; init; } = Array.Empty<object>();
}

file sealed class FakeNGameOverScreen
{
    public bool Visible { get; init; } = true;
}

file sealed class FakeActiveScreenContext
{
    public object? CurrentScreen { get; init; }

    public object? GetCurrentScreen()
    {
        return CurrentScreen;
    }
}

file sealed class FakeScreenStateTracker
{
    public bool IsInSharedRelicPickingScreen { get; init; }
}

file sealed class FakeNRun
{
    public object? ScreenStateTracker { get; init; }
}

file sealed class FakeTreasureButton
{
    public bool Opened { get; init; }

    public bool Visible { get; init; }

    public bool Enabled { get; init; }

    public object? Position { get; init; }

    public object? Size { get; init; }
}

file sealed class FakeTreasureRelicHolder
{
    public FakeTreasureRelicHolder(FakeRelicModel relic, double x, double y, bool enabled)
    {
        Relic = relic;
        Visible = true;
        Enabled = enabled;
        Position = new FakeVector2(x, y);
        Size = new FakeVector2(180, 180);
    }

    public object? Relic { get; init; }

    public bool Visible { get; init; }

    public bool Enabled { get; init; }

    public object? Position { get; init; }

    public object? Size { get; init; }
}

file sealed class FakeGrid
{
    public object[]? CurrentlyDisplayedCardHolders { get; init; }
}

file sealed class FakeGridCardHolder
{
    public FakeGridCardHolder(FakeCardModel cardModel, double x, double y)
    {
        Visible = true;
        CardModel = cardModel;
        Position = new FakeVector2(x, y);
        Size = new FakeVector2(180, 254);
    }

    public bool Visible { get; init; }

    public object CardModel { get; }

    public object Position { get; }

    public object Size { get; }
}

file sealed class FakeCardModel
{
    public string? Id { get; init; }

    public string? Name { get; init; }
}

file sealed class FakeCardCreationResult
{
    public FakeCardModel? Card { get; init; }
}

file sealed class FakeRelicModel
{
    public string? Id { get; init; }

    public string? Name { get; init; }
}

file sealed class FakePotionModel
{
    public string? Id { get; init; }

    public string? Name { get; init; }
}

file sealed class FakeClickableControl
{
    public bool Visible { get; init; }

    public bool Enabled { get; init; }

    public object? Position { get; init; }

    public object? Size { get; init; }
}

file sealed class FakeContainer
{
    public bool Visible { get; init; }
}

file sealed class FakeBanner
{
    public object? label { get; init; }
}

file class FakeReward
{
    public object? Description { get; init; }
}

file sealed class FakeCardReward : FakeReward
{
}

file sealed class FakeLabel
{
    public string? Text { get; init; }
}

file sealed class FakeVector2
{
    public FakeVector2(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double X { get; }

    public double Y { get; }
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

    public bool PlayerActionsDisabled { get; init; }

    public bool EndingPlayerTurnPhaseOne { get; init; }

    public bool EndingPlayerTurnPhaseTwo { get; init; }
}

file sealed class FakeCombatState
{
    public int? RoundNumber { get; init; }
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

file sealed class FakeRuntimeCaptureHook
{
    public void Observe(object? hand, object? history, object? combatManager, object? combatState)
    {
    }
}

file sealed class FakeRuntimePlayerHand
{
    public bool InCardPlay { get; init; }

    public string? CurrentMode { get; init; }

    public int _draggedHolderIndex { get; init; }

    public Dictionary<object, int> _holdersAwaitingQueue { get; init; } = new();

    public object? _currentCardPlay { get; init; }
}

file sealed class FakeRuntimeCardPlay
{
    public object? Holder { get; init; }

    public object? Card { get; init; }

    public object? Target { get; init; }
}

file sealed class FakeRuntimeHandHolder
{
    public object? CardModel { get; init; }
}

file sealed class FakeRuntimeCombatCard
{
    public string? Id { get; init; }

    public string? Title { get; init; }

    public string? Type { get; init; }

    public string? TargetType { get; init; }
}

file sealed class FakeRuntimeTargetManager
{
    public bool IsInSelection { get; init; }

    public object? HoveredNode { get; init; }

    public long LastTargetingFinishedFrame { get; init; }
}

file sealed class FakeRuntimeTargetNode
{
    public string? Id { get; init; }

    public string? DisplayName { get; init; }

    public object? Entity { get; init; }
}

file sealed class FakeRuntimeTargetEntity
{
    public string? Id { get; init; }

    public string? DisplayName { get; init; }

    public bool IsPlayer { get; init; }

    public bool IsFriendly { get; init; }
}

file sealed class FakeRuntimeCombatHistory
{
    public object[] CardPlaysStarted { get; init; } = Array.Empty<object>();

    public object[] CardPlaysFinished { get; init; } = Array.Empty<object>();
}

file sealed class FakeRuntimeCombatHistoryEntry
{
    public object? CardPlay { get; init; }
}

file sealed record RewardScenarioDefinition(
    string Name,
    string RunId,
    IReadOnlyList<LiveExportCardSummary> Deck,
    IReadOnlyList<LiveExportChoiceSummary> Choices,
    string ExpectedChoiceLabel,
    string? ExpectedMissingInformation = null,
    string? ExpectedRationaleContains = null,
    bool ExpectNonFirstChoice = false);

file sealed record RewardScenarioExecutionResult(
    AdviceInputPack LivePromptPack,
    AdviceResponse LiveAdvice,
    string LiveMarkdown,
    AdviceInputPack ReplayPromptPack,
    AdviceResponse ReplayAdvice,
    string ReplayMarkdown);

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
