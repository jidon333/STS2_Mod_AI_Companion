using System.Text.Json;
using System.Text;
using Sts2AiCompanion.AdvisorSceneDisplay;
using Sts2AiCompanion.AdvisorSceneModel;
using Sts2AiCompanion.SceneProvenance;
using Sts2AiCompanion.Foundation.State;
using Sts2AiCompanion.Harness.Actions;
using Sts2AiCompanion.Host;
using Sts2AiCompanion.Wpf.Display;
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
Run("runtime reflection prioritizes reward-pick child-screen choices over generic reward rows", TestRuntimeReflectionPrioritizesRewardPickChildScreenChoices, failures);
Run("runtime reflection exports deck-remove card-selection preview semantics", TestRuntimeReflectionDeckRemoveCardSelectionExport, failures);
Run("runtime reflection exports treasure room chest holder and proceed semantics", TestRuntimeReflectionTreasureRoomExport, failures);
Run("runtime reflection exports explicit shop room semantics and typed shop choices", TestRuntimeReflectionShopExport, failures);
Run("runtime reflection exports reward foreground ownership and teardown semantics", TestRuntimeReflectionRewardOwnershipExport, failures);
Run("runtime reflection exports map-node from mixed rest-site aftermath authority", TestRuntimeReflectionMixedRestAftermathMapExport, failures);
Run("runtime reflection reports filtered mixed-aftermath map-point diagnostics", TestRuntimeReflectionMixedRestAftermathMapDiagnostics, failures);
Run("runtime reflection exports rest-site click-ready and proceed semantics", TestRuntimeReflectionRestSiteClickReadyAndProceedExport, failures);
Run("runtime reflection exports explicit ancient dialogue advance before Neow options", TestRuntimeReflectionAncientEventDialogueExport, failures);
Run("runtime reflection exports explicit ancient option buttons and suppresses pseudo-choice duplicates", TestRuntimeReflectionAncientEventOptionExport, failures);
Run("runtime reflection marks ancient post-choice completion buttons explicitly", TestRuntimeReflectionAncientEventCompletionExport, failures);
Run("runtime reflection reconciles ancient await-options residue to generic event-option button lane", TestRuntimeReflectionAncientAwaitOptionsReconcilesToGenericEventButtons, failures);
Run("runtime reflection exports generic event proceed semantics from EventOption.IsProceed", TestRuntimeReflectionGenericEventProceedExport, failures);
Run("runtime reflection exports typed event option detail from hover-tip and result-model seams", TestRuntimeReflectionEventOptionTypedDetailExport, failures);
Run("runtime reflection normalizes ancient mixed post-proceed ownership to map lane", TestRuntimeReflectionAncientMixedPostProceedOwnershipNormalization, failures);
Run("runtime reflection keeps map owner when post-proceed map surface is pending", TestRuntimeReflectionAncientMixedPostProceedMapPending, failures);
Run("runtime reflection prefers direct current screen over broad observed probe", TestRuntimeReflectionPrefersDirectCurrentScreenOverBroadObservedProbe, failures);
Run("inventory publisher preserves strict map-node source contract", TestInventoryPublisherMapNodeSourceCorrection, failures);
Run("tracker and inventory preserve raw and compatibility screen provenance", TestTrackerAndInventoryPreserveScreenProvenance, failures);
Run("tracker re-emits additive screen provenance aliases", TestTrackerReEmitsAdditiveScreenProvenanceAliases, failures);
Run("inventory publisher separates primary and compatibility scene provenance", TestInventoryPublisherSeparatesPrimaryAndCompatibilitySceneProvenance, failures);
Run("inventory publisher node hints prefer primary scene provenance", TestInventoryPublisherNodeHintsPreferPrimarySceneProvenance, failures);
Run("inventory publisher does not promote compatibility winners into node semantics", TestInventoryPublisherDoesNotPromoteCompatibilitySceneWinnerIntoNodeSemantics, failures);
Run("inventory publisher keeps primary scene unknown when only compatibility winner exists", TestInventoryPublisherKeepsPrimarySceneUnknownWhenOnlyCompatibilityWinnerExists, failures);
Run("inventory publisher promotes tracker combat screen over stale published map scene", TestInventoryPublisherPromotesTrackerCombatPrimaryScene, failures);
Run("published scene provenance ignores legacy compatibility meta", TestPublishedSceneProvenanceIgnoresLegacyCompatibilityMeta, failures);
Run("inventory publisher suppresses immediate publish for unstable mixed provenance", TestInventoryPublisherSuppressesImmediatePublishForMixedProvenance, failures);
Run("inventory publisher fingerprint tracks provenance fields", TestInventoryPublisherFingerprintTracksProvenanceFields, failures);
Run("bridge guard surface requires explicit published scene provenance", TestHarnessBridgeGuardSurfaceRequiresPublishedProvenance, failures);
Run("runtime reflection rejects overlay-like player roots", TestRuntimeReflectionRejectsOverlayLikePlayerRoots, failures);
Run("runtime reflection extracts combat cards from player combat state", TestRuntimeReflectionExtractDeckFromCombatState, failures);
Run("runtime reflection prefers deck source over combat zones", TestRuntimeReflectionPrefersDeckSourceOverCombatZones, failures);
Run("runtime reflection ignores standalone deck aliases when player deck is present", TestRuntimeReflectionIgnoresStandaloneDeckAliasesWhenPlayerDeckPresent, failures);
Run("runtime reflection exports structured combat counts from combat state", TestRuntimeReflectionExportsStructuredCombatCounts, failures);
Run("runtime reflection prefers evaluated display text for card-like options", TestRuntimeReflectionPrefersEvaluatedDisplayTextForCardLikeOptions, failures);
Run("runtime reflection keeps sanitized fallback when evaluated card-like text is unavailable", TestRuntimeReflectionKeepsSanitizedFallbackWhenNoEvaluatedTextExists, failures);
Run("runtime reflection prefers player hand ui holders over combat-state hand fallback", TestRuntimeReflectionPrefersPlayerHandUiHolders, failures);
Run("runtime reflection encounter prefers CombatManager IsInProgress", TestRuntimeReflectionEncounterPrefersCombatManagerIsInProgress, failures);
Run("runtime reflection encounter does not override CombatManager IsInProgress false", TestRuntimeReflectionEncounterDoesNotOverrideCombatManagerFalse, failures);
Run("runtime reflection screen resolution prefers overlay screens", TestRuntimeReflectionScreenResolution, failures);
Run("runtime reflection prefers explicit active feedback screen over broad combat probe", TestRuntimeReflectionPrefersActiveFeedbackScreenOverBroadCombatProbe, failures);
Run("runtime reflection prefers abandon-run confirm popup over broad feedback bag", TestRuntimeReflectionPrefersAbandonRunConfirmPopupOverBroadFeedbackBag, failures);
Run("runtime reflection does not infer combat from global startup type bags", TestRuntimeReflectionDoesNotInferCombatFromGlobalStartupTypeBag, failures);
Run("runtime reflection exports combat play state metadata", TestRuntimeReflectionCombatMetadataExport, failures);
Run("runtime reflection capture clears combat slot and success inference", TestRuntimeReflectionCaptureClearsCombatMetadata, failures);
Run("companion scene normalizer prefers main-menu over hidden character-select markers", TestCompanionSceneNormalizerMainMenuPriority, failures);
Run("companion scene normalizer detects blocking overlay from placeholder choices", TestCompanionSceneNormalizerBlockingOverlay, failures);
Run("companion state mapper prefers main-menu over hidden character-select markers", TestCompanionStateMapperMainMenuPriority, failures);
Run("companion state mapper follows resolved primary scene over compatibility-only visible alias", TestCompanionStateMapperPrimarySceneWinsCompatibilityVisibleAlias, failures);
Run("screen provenance resolver promotes combat over stale published map", TestScreenProvenanceResolverCombatPromotion, failures);
Run("screen provenance resolver prefers published screen over raw fallback", TestScreenProvenanceResolverPrefersPublishedScreen, failures);
Run("screen provenance resolver preserves compatibility as diagnostic-only provenance", TestScreenProvenanceResolverCompatibilityDoesNotPromotePrimary, failures);
Run("screen provenance resolver uses inventory fallback only for harness adapter", TestScreenProvenanceResolverInventoryFallbackIsolation, failures);
Run("screen provenance harness and host adapters stay aligned on A2 fixtures", TestScreenProvenanceHarnessHostParity, failures);
Run("live export tracker preserves high-value state across partial observations", TestLiveExportTrackerPartialMerge, failures);
Run("live export tracker clears combat structured fields outside combat", TestLiveExportTrackerClearsCombatStructuredFieldsOutsideCombat, failures);
Run("live export tracker accepts explicit published scene over sticky fallback preservation", TestLiveExportTrackerPrefersExplicitPublishedSceneOverStickyFallback, failures);
Run("live export tracker accepts foreground owner over fallback published screen", TestLiveExportTrackerPrefersForegroundOwnerOverFallbackPublishedScreen, failures);
Run("live export tracker accepts authoritative combat encounter on high-value screen", TestLiveExportTrackerAcceptsAuthoritativeCombatEncounter, failures);
Run("collector mode records screen episodes and choice diagnostics", TestLiveExportTrackerCollectorMode, failures);
Run("live export tracker keeps authoritative existing-run menu-to-combat transitions", TestLiveExportTrackerMenuToCombatExistingRunAuthority, failures);
Run("live export tracker accepts direct character-select branch without submenu", TestLiveExportTrackerMenuToCombatDirectBranchAuthority, failures);
Run("harness path resolver exposes trace queue path", TestHarnessPathResolver, failures);
Run("bridge action executor round-trips action results through the queue", TestBridgeActionExecutorRoundTrip, failures);
Run("companion path resolver keeps per-run artifacts under companion root", TestCompanionPathResolver, failures);
Run("knowledge catalog service builds a bounded relevant slice", TestKnowledgeCatalogService, failures);
Run("advisor display sanitizer strips raw markers", TestAdvisorDisplaySanitizer, failures);
Run("advisor display resolver uses knowledge fallback for missing descriptions", TestAdvisorKnowledgeDisplayResolver, failures);
Run("wpf scene display formatter hides utility combat options and sanitizes reward markers", TestAdvisorSceneDisplayFormatter, failures);
Run("deck display formatter aggregates localized card names", TestDeckDisplayFormatter, failures);
Run("advice prompt builder emits the required prompt sections", TestAdvicePromptBuilder, failures);
Run("reward compact advisor input builder preserves exact labels and missing facts", TestRewardCompactAdvisorInputBuilder, failures);
Run("event compact advisor input builder extracts explicit option facts and fails closed on duplicates", TestEventCompactAdvisorInputBuilder, failures);
Run("event compact advisor input builder respects generic fallback ordering", TestEventCompactFallbackOrdering, failures);
Run("event compact advisor input builder keeps knowledge slice strict and event-local", TestEventCompactKnowledgeFiltering, failures);
Run("event compact advisor input builder can keep knowledge slice empty when exact event-local matches are absent", TestEventCompactKnowledgeCanStayEmpty, failures);
Run("event compact prompt allows explicit target-filter events to recommend with deck summary", TestCompactAdvicePromptBuilder, failures);
Run("shop compact advisor input builder extracts purchase counts and fails closed on duplicates", TestShopCompactAdvisorInputBuilder, failures);
Run("combat compact preview builder captures current facts and stays preview-only", TestCombatCompactPreviewBuilder, failures);
Run("reward deterministic builders are reproducible", TestRewardDeterministicBuilders, failures);
Run("reward deterministic layer stays off for non-card rewards", TestRewardNonCardFallback, failures);
Run("reward deterministic layer falls back outside reward scenes", TestRewardDeterministicFallback, failures);
Run("reward live and replay paths share deterministic context", TestRewardLiveReplayDeterministicContext, failures);
Run("reward finalizer only normalizes labels and attaches trace", TestRewardFinalizerMinimalNormalization, failures);
Run("compact finalizer clears labels outside current scene options", TestCompactAdviceFinalizer, failures);
Run("reward model rationale survives without heavy backfill", TestRewardModelRationaleArtifacts, failures);
Run("reward fixtures include a non-first winning choice", TestRewardNonFirstChoiceScenario, failures);
Run("shop compact advisor host flow supports model-call and fails closed on duplicates", TestCompanionHostShopCompactAdviceSafety, failures);
Run("combat preview compact safety stays no-call and retry-last safe", TestCompanionHostCombatPreviewCompactAdviceSafety, failures);
Run("host wrappers converge on foundation prompt and knowledge services", TestHostFoundationConvergence, failures);
Run("companion host keeps advice flow while diagnostics stay optional", TestCompanionHostAdviceFlow, failures);
Run("companion host manual reward advice uses compact input", TestCompanionHostManualRewardCompactAdviceFlow, failures);
Run("companion host keeps compact advisor managed scenes manual-only", TestCompanionHostAutomaticCompactAdviceRemainsManualOnly, failures);
Run("companion host keeps scene model polling responsive while auto advice runs", TestCompanionHostScenePollingStaysResponsiveDuringAutoAdvice, failures);
Run("replay validator uses compact path for reward event shop and combat fixtures", TestReplayAdvisorValidatorCompactPath, failures);
Run("companion host publishes live advisor scene model artifacts", TestCompanionHostLiveSceneModelArtifacts, failures);
Run("companion host classifies combat options and structured pile counts", TestCompanionHostCombatSceneModelClassification, failures);
Run("companion host deduplicates event binding options and keeps enriched descriptions", TestCompanionHostEventSceneModelOptionDeduping, failures);
Run("codex cli trace parser extracts thread id from json events", TestCodexCliTraceParser, failures);
Run("codex cli surfaces context overflow diagnostics from exec trace", TestCodexCliContextOverflowDiagnostic, failures);
Run("host codex client retries without resume after context overflow", TestHostCodexCliClientRetriesWithoutResumeAfterContextOverflow, failures);

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

        var holdStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var releaseTask = Task.Run(() =>
        {
            Thread.Sleep(120);
            holdStream.Dispose();
        });

        LiveExportAtomicFileWriter.WriteAllTextAtomic(path, "third");
        releaseTask.GetAwaiter().GetResult();

        Assert(File.ReadAllText(path) == "third", "Expected atomic writer to retry and replace the previous contents after a transient destination lock.");
        Assert(!File.Exists(path + ".tmp"), "Expected atomic writer retry path to clean up the temp file.");
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
            new FakeGridCardHolder(new FakeCardModel { Id = "CARD.SHRUG_IT_OFF", Name = "몸통 박치기", Description = "방어도를 {CalculatedBlock:base()} 얻습니다.", EvaluatedDescription = "방어도를 8 얻고 카드를 1장 뽑습니다." }, 520, 280),
            new FakeGridCardHolder(new FakeCardModel { Id = "CARD.BATTLE_TRANCE", Name = "전투 최면", Description = "카드를 {DrawCount:base()}장 뽑습니다.", EvaluatedDescription = "카드를 3장 뽑습니다." }, 860, 280),
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

static void TestRuntimeReflectionPrioritizesRewardPickChildScreenChoices()
{
    var rewardButtons = Enumerable.Range(0, 8)
        .Select(index => (object)new FakeRewardButton
        {
            Reward = new FakeCardReward
            {
                Description = new FakeLocString($"보상 설명 {index}"),
            },
            _label = new FakeLabel { Text = $"보상 카드 행 {index}" },
            Position = new FakeVector2(720, 280 + (index * 48)),
            Size = new FakeVector2(402, 86),
        })
        .ToArray();
    var proceedButton = new FakeClickableControl
    {
        Visible = true,
        Enabled = true,
        Position = new FakeVector2(1860, 764),
        Size = new FakeVector2(269, 108),
    };
    var rewardScreen = new FakeNRewardsScreen
    {
        Visible = true,
        _proceedButton = proceedButton,
        _rewardButtons = rewardButtons,
    };
    var rewardPickScreen = new FakeNCardRewardSelectionScreen
    {
        Visible = true,
        _banner = new FakeBanner { label = new FakeLabel { Text = "카드를 선택하세요" } },
        _cardRow = new object[]
        {
            new FakeGridCardHolder(new FakeCardModel { Id = "CARD.SHRUG_IT_OFF", Name = "흘려보내기", Description = "방어도를 {CalculatedBlock:base()} 얻습니다.", EvaluatedDescription = "방어도를 8 얻고 카드를 1장 뽑습니다." }, 520, 280),
            new FakeGridCardHolder(new FakeCardModel { Id = "CARD.RAGE", Name = "격노", Description = "이번 턴 공격을 사용할 때마다 방어도를 {CalculatedBlock:base()} 얻습니다.", EvaluatedDescription = "이번 턴 공격을 사용할 때마다 방어도를 3 얻습니다." }, 860, 280),
            new FakeGridCardHolder(new FakeCardModel { Id = "CARD.ANGER", Name = "분노", Description = "적에게 {CalculatedDamage:diff()} 피해를 줍니다.", EvaluatedDescription = "적에게 6 피해를 줍니다." }, 1200, 280),
        },
    };
    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            rewardScreen,
            rewardPickScreen,
            proceedButton,
            new FakeActiveScreenContext
            {
                CurrentScreen = rewardPickScreen,
            },
        },
        "rewards");

    Assert(observation.Meta.TryGetValue("cardSelectionVisibleCardCount", out var visibleCardCount)
           && string.Equals(visibleCardCount, "3", StringComparison.OrdinalIgnoreCase),
        "Reward-pick child-screen should still export the visible card count.");
    Assert(observation.Meta.TryGetValue("choiceExtractorPath", out var extractorPath)
           && string.Equals(extractorPath, "card-selection-reward-pick", StringComparison.OrdinalIgnoreCase),
        "Reward-pick child-screen should keep the card-selection extractor path even while the parent reward screen remains foreground-owned.");

    var rewardPickChoices = observation.Choices
        .Where(choice => string.Equals(choice.Kind, "reward-pick-card", StringComparison.OrdinalIgnoreCase))
        .ToArray();
    Assert(rewardPickChoices.Length == 3,
        $"Reward-pick child-screen should keep all explicit card-selection choices ahead of the generic reward budget. Actual reward-pick-card count={rewardPickChoices.Length}.");
    Assert(rewardPickChoices.All(choice => !string.IsNullOrWhiteSpace(choice.ScreenBounds)),
        "Reward-pick child-screen should preserve bounds on the exported card-selection choices.");
    Assert(rewardPickChoices.Any(choice => string.Equals(choice.Label, "흘려보내기", StringComparison.OrdinalIgnoreCase)
                                          && string.Equals(choice.Description, "방어도를 8 얻고 카드를 1장 뽑습니다.", StringComparison.OrdinalIgnoreCase)),
        "Reward-pick child-screen should prefer evaluated card display text over the generic prompt.");
    Assert(rewardPickChoices.All(choice => choice.Description?.Contains("{CalculatedBlock}", StringComparison.Ordinal) != true),
        "Reward-pick child-screen should not leak raw dynamic placeholder text.");
    Assert(observation.Choices.Any(choice => string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)),
        "Reward-pick child-screen export should stay additive and keep parent reward rows for diagnostics.");
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
    Assert(shopChoices.Any(choice => string.Equals((string?)ReadProperty(choice, "Kind"), "shop-option:card", StringComparison.OrdinalIgnoreCase)
                                     && string.Equals((string?)ReadProperty(choice, "Description"), "적에게 12 피해를 주고 카드를 1장 뽑습니다.", StringComparison.OrdinalIgnoreCase)), "Expected shop card choice to prefer the evaluated card display text.");
    Assert(shopChoices.Any(choice => string.Equals((string?)ReadProperty(choice, "Kind"), "shop-option:relic", StringComparison.OrdinalIgnoreCase)
                                     && string.Equals((string?)ReadProperty(choice, "Description"), "턴 종료 시 방어도를 8 얻습니다.", StringComparison.OrdinalIgnoreCase)), "Expected shop relic choice to prefer the evaluated hover-tip text.");
    Assert(shopChoices.Any(choice => string.Equals((string?)ReadProperty(choice, "Kind"), "shop-option:potion", StringComparison.OrdinalIgnoreCase)
                                     && string.Equals((string?)ReadProperty(choice, "Description"), "이번 전투에서 힘을 2 얻습니다.", StringComparison.OrdinalIgnoreCase)), "Expected shop potion choice to prefer the evaluated hover-tip text.");
    Assert(shopChoices.All(choice => !(((string?)ReadProperty(choice, "Description"))?.Contains("{CalculatedDamage:diff()}", StringComparison.Ordinal) ?? false)), "Expected shop choice descriptions to drop raw dynamic placeholders.");

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
    var activeRewardObservation = observeMethod!.Invoke(null, new object?[] { new object[] { rewardScreen, rewardButton, proceedButton, activeRewardContext, new FakePlayerEntity { HasOpenPotionSlots = true } }, "rewards" });
    Assert(activeRewardObservation is not null, "Expected reward observation.");
    Assert((bool)(ReadProperty(activeRewardObservation!, "ScreenDetected") ?? false), "Expected reward screen detection.");
    Assert((bool)(ReadProperty(activeRewardObservation!, "ForegroundOwned") ?? false), "Expected reward foreground ownership while reward screen is current.");
    Assert((bool)(ReadProperty(activeRewardObservation!, "RewardIsCurrentActiveScreen") ?? false), "Expected reward current-active-screen export.");
    Assert((bool?)(ReadProperty(activeRewardObservation!, "HasOpenPotionSlots")) == true, "Expected reward observation to export open potion slot truth from the player root.");

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
    var rewardAftermathObservation = observeMethod.Invoke(null, new object?[] { new object[] { staleRewardScreen, disabledRewardButton, disabledProceedButton, activeMapContext, new FakePlayerEntity { HasOpenPotionSlots = false } }, "rewards" });
    Assert(rewardAftermathObservation is not null, "Expected reward aftermath observation.");
    Assert((bool)(ReadProperty(rewardAftermathObservation!, "ScreenVisible") ?? false), "Expected stale reward visibility to remain exported for diagnostics.");
    Assert((bool)(ReadProperty(rewardAftermathObservation!, "ForegroundOwned") ?? true) == false, "Expected reward foreground ownership to drop once map becomes current and reward controls are disabled.");
    Assert((bool)(ReadProperty(rewardAftermathObservation!, "TeardownInProgress") ?? false), "Expected reward teardown export once map is current and reward visuals merely linger.");
    Assert((bool)(ReadProperty(rewardAftermathObservation!, "MapIsCurrentActiveScreen") ?? false), "Expected map current active screen export during reward proceed aftermath.");
    Assert((bool?)(ReadProperty(rewardAftermathObservation!, "HasOpenPotionSlots")) == false, "Expected reward observation to preserve a full potion belt truth during stale reward aftermath.");

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
            new
            {
                CurrentScreen = "map",
            },
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
    Assert(observation.Meta.TryGetValue("rawCurrentScreen", out var rawCurrentScreen)
           && string.Equals(rawCurrentScreen, "map", StringComparison.OrdinalIgnoreCase),
        "Mixed rest-site aftermath should preserve direct rawCurrentScreen separately from the semantic screen winner.");
    Assert(observation.Meta.TryGetValue("rawCurrentActiveScreenType", out var rawCurrentActiveScreenType)
           && rawCurrentActiveScreenType?.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase) == true,
        "Mixed rest-site aftermath should export rawCurrentActiveScreenType so harness consumers can read direct active-screen provenance.");

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

static void TestRuntimeReflectionRestSiteClickReadyAndProceedExport()
{
    var healOption = new FakeRestSiteOption
    {
        OptionId = "HEAL",
        Label = "Rest",
        IsEnabled = true,
    };
    var smithOption = new FakeRestSiteOption
    {
        OptionId = "SMITH",
        Label = "Smith",
        IsEnabled = true,
    };
    var healButtonNotReady = new FakeNRestSiteButton
    {
        Visible = true,
        Enabled = true,
        Option = healOption,
        MouseFilter = "Ignore",
        Position = new FakeVector2(520, 260),
        Size = new FakeVector2(200, 110),
    };
    var smithButtonReady = new FakeNRestSiteButton
    {
        Visible = true,
        Enabled = true,
        Option = smithOption,
        MouseFilter = "Stop",
        Position = new FakeVector2(820, 260),
        Size = new FakeVector2(200, 110),
    };
    var notReadyRoom = new FakeNRestSiteRoom
    {
        Visible = true,
        _lastFocused = healButtonNotReady,
    };
    var notReadyObservation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            notReadyRoom,
            healButtonNotReady,
            smithButtonReady,
        },
        "rest-site");
    Assert(notReadyObservation.Meta.TryGetValue("restSiteButtonsVisible", out var buttonsVisible)
           && string.Equals(buttonsVisible, "true", StringComparison.OrdinalIgnoreCase),
        "Rest-site observation should export visible buttons when explicit rest-site choices are on screen.");
    Assert(notReadyObservation.Meta.TryGetValue("restSiteButtonsClickReady", out var buttonsClickReady)
           && string.Equals(buttonsClickReady, "false", StringComparison.OrdinalIgnoreCase),
        "Rest-site observation should not treat visible buttons as click-ready before input opens.");
    Assert(notReadyObservation.Meta.TryGetValue("restSiteClickReadyOptionIds", out var clickReadyOptionIds)
           && string.Equals(clickReadyOptionIds, "SMITH", StringComparison.OrdinalIgnoreCase),
        $"Rest-site click-ready export should preserve only input-ready options. actual={clickReadyOptionIds ?? "null"}");
    Assert(notReadyObservation.Meta.TryGetValue("restSiteSelectionCurrentStatus", out var currentStatus)
           && string.Equals(currentStatus, "explicit-choice", StringComparison.OrdinalIgnoreCase),
        "Rest-site observation should stay in explicit-choice while no proceed or smith overlay is visible.");

    var proceedButton = new FakeNProceedButton
    {
        Visible = true,
        Enabled = true,
        Position = new FakeVector2(1576.3, 761.3),
        Size = new FakeVector2(282.45, 113.4),
    };
    var healButtonReady = new FakeNRestSiteButton
    {
        Visible = true,
        Enabled = true,
        Option = healOption,
        MouseFilter = "Stop",
        Position = new FakeVector2(520, 260),
        Size = new FakeVector2(200, 110),
    };
    var proceedRoom = new FakeNRestSiteRoom
    {
        Visible = true,
        _lastFocused = healButtonReady,
        ProceedButton = proceedButton,
    };
    var proceedObservation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            proceedRoom,
            healButtonReady,
            smithButtonReady,
            proceedButton,
        },
        "rest-site");
    Assert(proceedObservation.Meta.TryGetValue("restSiteProceedVisible", out var proceedVisible)
           && string.Equals(proceedVisible, "true", StringComparison.OrdinalIgnoreCase)
           && proceedObservation.Meta.TryGetValue("restSiteProceedEnabled", out var proceedEnabled)
           && string.Equals(proceedEnabled, "true", StringComparison.OrdinalIgnoreCase),
        "Rest-site observation should export proceed visibility and enabled state once the room publishes proceed.");
    Assert(proceedObservation.Meta.TryGetValue("restSiteSelectionCurrentStatus", out var proceedStatus)
           && string.Equals(proceedStatus, "proceed-visible", StringComparison.OrdinalIgnoreCase),
        $"Proceed-visible rest-site observation should promote room-local proceed status. actual={proceedStatus ?? "null"}");
    proceedObservation.Meta.TryGetValue("restSiteSelectionOutcome", out var selectionOutcome);
    proceedObservation.Meta.TryGetValue("restSiteSelectionOutcomeEvidence", out var outcomeEvidence);
    Assert(string.Equals(selectionOutcome, "success", StringComparison.OrdinalIgnoreCase)
           && string.Equals(outcomeEvidence, "runtime-poll:rest-site-proceed", StringComparison.OrdinalIgnoreCase),
        $"Rest-site proceed should count as heal success even without a hook signal. actual={selectionOutcome ?? "null"}/{outcomeEvidence ?? "null"}");
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

static void TestRuntimeReflectionAncientAwaitOptionsReconcilesToGenericEventButtons()
{
    var staleProceedButton = new FakeNEventOptionButton(
        0,
        "진행",
        "[gold][b]진행[/b][/gold]",
        460,
        942,
        enabled: false,
        isProceed: true);
    var ancientLayout = new FakeNAncientEventLayout
    {
        Visible = true,
        IsDialogueOnLastLine = true,
        DefaultFocusedControl = staleProceedButton,
        Children = new object[] { staleProceedButton },
    };
    var genericEventButtons = new object[]
    {
        new FakeNEventOptionButton(
            10,
            "해독한다",
            "최대 체력을 3 잃고 무작위 카드를 1장 강화합니다.",
            922,
            596,
            enabled: true),
        new FakeNEventOptionButton(
            11,
            "부순다",
            "체력을 20 회복합니다.",
            922,
            700,
            enabled: true),
    };
    var eventRoom = new FakeNEventRoom
    {
        Visible = true,
        Children = new object[] { ancientLayout }.Concat(genericEventButtons).ToArray(),
    };
    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeActiveScreenContext
            {
                CurrentScreen = eventRoom,
            },
            eventRoom,
            genericEventButtons[0],
            genericEventButtons[1],
        },
        "event");

    Assert(observation.Meta.TryGetValue("ancientEventDetected", out var ancientDetected)
           && string.Equals(ancientDetected, "true", StringComparison.OrdinalIgnoreCase),
        "Ancient residue should remain detectable even when the actionable surface is a generic event button family.");
    Assert(observation.Meta.TryGetValue("foregroundOwner", out var foregroundOwner)
           && string.Equals(foregroundOwner, "event", StringComparison.OrdinalIgnoreCase),
        "Generic event button reconciliation should keep event ownership while map release truth is absent.");
    Assert(observation.Meta.TryGetValue("foregroundActionLane", out var foregroundLane)
           && string.Equals(foregroundLane, "event-choice", StringComparison.OrdinalIgnoreCase),
        $"Generic event button reconciliation should surface the actual event-choice lane instead of promoting metadata-only ancient-option. Actual lane='{foregroundLane ?? "<missing>"}'.");
    Assert(!observation.Meta.ContainsKey("ancientPhase")
           && !observation.Meta.ContainsKey("ancientEventExtractionPath")
           && !observation.Meta.ContainsKey("ancientOptionCount")
           && !observation.Meta.ContainsKey("ancientCompletionCount"),
        "Generic event button reconciliation should clear ancient foreground-driving metadata once the final lane is generic event-choice.");
    Assert(!observation.Meta.ContainsKey("ancientOptionSummary"),
        "Ancient await-options reconciliation should not keep stale ancient option summaries when no explicit ancient buttons are exported.");

    var exportedOptions = observation.Choices
        .Where(choice => string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase))
        .OrderBy(choice => choice.NodeId, StringComparer.OrdinalIgnoreCase)
        .ToArray();
    Assert(exportedOptions.Length == 2,
        $"Await-options reconciliation should keep only the canonical actionable event-option button family once the final lane is generic event-choice. Actual options: {string.Join(" | ", exportedOptions.Select(choice => $"{choice.NodeId}:{choice.Label}:{string.Join(",", choice.SemanticHints ?? Array.Empty<string>())}"))}");
    Assert(exportedOptions.Count(choice => choice.SemanticHints.Contains("source:event-option-button", StringComparer.OrdinalIgnoreCase)) == 2,
        "Await-options reconciliation should keep the two actionable generic NEventOptionButton choices.");
    Assert(exportedOptions.All(choice => !choice.SemanticHints.Contains("source:event-option", StringComparer.OrdinalIgnoreCase)),
        "Await-options reconciliation should drop raw EventOption residue once the final exported lane is canonicalized to generic event-choice.");
    Assert(exportedOptions.All(choice => !choice.SemanticHints.Contains("source:ancient-option-button", StringComparer.OrdinalIgnoreCase)),
        "Await-options reconciliation should not relabel any live event-option surface as an ancient explicit button source.");
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

static void TestRuntimeReflectionEventOptionTypedDetailExport()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod(
        "TryCreateChoiceSummary",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
        binder: null,
        new[] { typeof(object) },
        modifiers: null);
    Assert(method is not null, "Expected private TryCreateChoiceSummary helper.");

    var birdButton = new FakeNEventOptionButton(
        0,
        "새",
        "시작 카드를 1장 선택해 쪼기로 변화시킵니다.",
        460,
        942,
        enabled: true,
        optionKey: "WOOD_CARVINGS.pages.INITIAL.options.BIRD")
    {
        Option = new FakeEventOptionModel
        {
            Id = "bird-option",
            TextKey = "WOOD_CARVINGS.pages.INITIAL.options.BIRD",
            Title = "새",
            Description = "시작 카드를 1장 선택해 로 변화시킵니다.",
            HoverTips = new object[]
            {
                new FakeCardHoverTip(new FakeCardModel
                {
                    Id = "Peck",
                    Name = "쪼기",
                }),
            },
        },
        _label = new FakeLabel { Text = "[gold][b]새[/b][/gold]\n시작 카드를 1장 선택해 쪼기로 변화시킵니다." },
    };

    var birdSummary = method.Invoke(null, new object?[] { birdButton }) as LiveExportChoiceSummary;
    Assert(birdSummary is not null, "Expected generic event option summary for bird button.");
    Assert(birdSummary!.EventOptionDetail is not null, "Expected typed event option detail for bird button.");
    Assert(string.Equals(birdSummary.EventOptionDetail!.OptionKey, "WOOD_CARVINGS.pages.INITIAL.options.BIRD", StringComparison.Ordinal), "Expected bird option key.");
    Assert(string.Equals(birdSummary.EventOptionDetail!.EvaluatedDescription, "시작 카드를 1장 선택해 쪼기로 변화시킵니다.", StringComparison.Ordinal), "Expected evaluated bird description.");
    Assert(string.Equals(birdSummary.EventOptionDetail!.ResultCard?.Id, "Peck", StringComparison.Ordinal), "Expected bird result card id.");
    Assert(string.Equals(birdSummary.EventOptionDetail!.ResultCard?.Title, "쪼기", StringComparison.Ordinal), "Expected bird result card title.");

    var snakeButton = new FakeNEventOptionButton(
        1,
        "뱀",
        "카드 1장에 미끈거림을 인챈트합니다.",
        460,
        1038,
        enabled: true,
        optionKey: "WOOD_CARVINGS.pages.INITIAL.options.SNAKE")
    {
        Option = new FakeEventOptionModel
        {
            Id = "snake-option",
            TextKey = "WOOD_CARVINGS.pages.INITIAL.options.SNAKE",
            Title = "뱀",
            Description = "카드 1장에 을 인챈트합니다.",
            HoverTips = new object[]
            {
                new FakeHoverTip
                {
                    Id = "Slither",
                    Title = "미끈거림",
                    Description = "미끈거림은 해당 카드를 뽑았을 때 이번 전투 동안 비용을 무작위 0~3으로 바꿉니다.",
                },
            },
        },
        _label = new FakeLabel { Text = "[gold][b]뱀[/b][/gold]\n카드 1장에 미끈거림을 인챈트합니다." },
    };

    var snakeSummary = method.Invoke(null, new object?[] { snakeButton }) as LiveExportChoiceSummary;
    Assert(snakeSummary is not null, "Expected generic event option summary for snake button.");
    Assert(string.Equals(snakeSummary!.EventOptionDetail?.HoverTipTitle, "미끈거림", StringComparison.Ordinal), "Expected snake hover-tip title.");
    Assert(snakeSummary.EventOptionDetail?.HoverTipDescription?.Contains("무작위 0~3", StringComparison.Ordinal) == true, "Expected snake hover-tip description.");
    Assert(string.Equals(snakeSummary.EventOptionDetail?.HoverTipId, "Slither", StringComparison.Ordinal), "Expected snake hover-tip id.");

    var neowButton = new FakeNEventOptionButton(
        2,
        "니오우의 비탄",
        "덱에 니오우의 격분을 1장 추가합니다.",
        460,
        1134,
        enabled: true,
        optionKey: "NEOW.pages.INITIAL.options.NEOWS_TORMENT")
    {
        Option = new FakeEventOptionModel
        {
            Id = "neow-option",
            TextKey = "NEOW.pages.INITIAL.options.NEOWS_TORMENT",
            Title = "니오우의 비탄",
            Description = "덱에 니오우의 격분을 1장 추가합니다.",
            Relic = new FakeRelicModel
            {
                Id = "NeowsTorment",
                Name = "니오우의 비탄",
                HoverTips = new object[]
                {
                    new FakeCardHoverTip(new FakeCardModel
                    {
                        Id = "NeowsFury",
                        Name = "니오우의 격분",
                    }),
                },
            },
        },
        _label = new FakeLabel { Text = "[gold][b]니오우의 비탄[/b][/gold]\n덱에 니오우의 격분을 1장 추가합니다." },
    };

    var neowSummary = method.Invoke(null, new object?[] { neowButton }) as LiveExportChoiceSummary;
    Assert(neowSummary is not null, "Expected generic event option summary for Neow button.");
    Assert(string.Equals(neowSummary!.EventOptionDetail?.ResultRelic?.Id, "NeowsTorment", StringComparison.Ordinal), "Expected Neow result relic id.");
    Assert(string.Equals(neowSummary.EventOptionDetail?.ResultCard?.Id, "NeowsFury", StringComparison.Ordinal), "Expected Neow result card id from relic hover tips.");
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

static void TestRuntimeReflectionPrefersDirectCurrentScreenOverBroadObservedProbe()
{
    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new
            {
                CurrentScreen = "rewards",
            },
        },
        "map");

    Assert(observation.Meta.TryGetValue("rawCurrentScreen", out var rawCurrentScreen)
           && string.Equals(rawCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Runtime reflection should export rawCurrentScreen from direct runtime state.");
    Assert(observation.Meta.TryGetValue("rawObservedScreen", out var rawObservedScreen)
           && string.Equals(rawObservedScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Runtime-poll screen resolution should not let a broad observed probe override the direct current screen.");
    Assert(observation.Meta.TryGetValue("publishedCurrentScreen", out var publishedCurrentScreen)
           && string.Equals(publishedCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Runtime reflection should publish the direct current screen when it conflicts with a broad observed probe.");
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
    Assert(string.Equals(snapshot.RawCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should expose raw current screen separately from raw observed screen.");
    Assert(string.Equals(snapshot.RawObservedScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should preserve the raw observed screen separately from compatibility logical screen.");
    Assert(string.Equals(snapshot.PublishedCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should expose published current screen separately from raw and compatibility provenance.");
    Assert(string.Equals(snapshot.PublishedVisibleScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker published visible screen should preserve the direct observed screen instead of compatibility visible-screen shaping.");
    Assert(snapshot.PublishedSceneReady is null, "Tracker should not synthesize published scene-ready when the observation did not publish it explicitly.");
    Assert(string.Equals(snapshot.CompatibilityCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should expose compatibility current screen separately from legacy current screen.");
    Assert(string.Equals(snapshot.CompatibilityLogicalScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should expose the compatibility logical screen explicitly.");
    Assert(string.Equals(snapshot.CompatibilityVisibleScreen, "map", StringComparison.OrdinalIgnoreCase), "Tracker should expose compatibility visible-screen shaping separately.");
    Assert(snapshot.CompatibilitySceneReady == false, "Reward/map mixed aftermath should demote compatibility scene-ready while leaving raw screen intact.");
    Assert(!snapshot.Meta.ContainsKey("logicalScreen"), "Tracker should no longer emit tracker-shaped logicalScreen as primary meta.");
    Assert(!snapshot.Meta.ContainsKey("visibleScreen"), "Tracker should no longer emit tracker-shaped visibleScreen as primary meta.");
    Assert(!snapshot.Meta.ContainsKey("sceneReady"), "Tracker should no longer emit tracker-shaped sceneReady as primary meta.");
    Assert(!snapshot.Meta.ContainsKey("sceneAuthority"), "Tracker should no longer emit tracker-shaped sceneAuthority as primary meta.");
    Assert(!snapshot.Meta.ContainsKey("sceneStability"), "Tracker should no longer emit tracker-shaped sceneStability as primary meta.");
    Assert(snapshot.Meta.TryGetValue("screen", out var rawScreenMeta)
           && string.Equals(rawScreenMeta, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker meta 'screen' should now preserve raw observed screen.");
    Assert(snapshot.Meta.TryGetValue("rawCurrentScreen", out var rawCurrentScreen)
           && string.Equals(rawCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should emit rawCurrentScreen alongside rawObservedScreen for additive provenance migration.");
    Assert(snapshot.Meta.TryGetValue("compatLogicalScreen", out var compatLogicalScreen)
           && string.Equals(compatLogicalScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should write compatibility logical screen into dedicated meta.");
    Assert(snapshot.Meta.TryGetValue("compatibilityCurrentScreen", out var compatibilityCurrentScreen)
           && string.Equals(compatibilityCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should emit compatibilityCurrentScreen alongside compatLogicalScreen for additive provenance migration.");
    Assert(snapshot.Meta.TryGetValue("compatVisibleScreen", out var compatVisibleScreen)
           && string.Equals(compatVisibleScreen, "map", StringComparison.OrdinalIgnoreCase), "Tracker should write compatibility visible screen into dedicated meta.");
    Assert(snapshot.Meta.TryGetValue("compatibilityVisibleScreen", out var compatibilityVisibleScreen)
           && string.Equals(compatibilityVisibleScreen, "map", StringComparison.OrdinalIgnoreCase), "Tracker should emit compatibilityVisibleScreen alongside compatVisibleScreen for additive provenance migration.");
    Assert(snapshot.Meta.TryGetValue("publishedCurrentScreen", out var publishedCurrentScreenMeta)
           && string.Equals(publishedCurrentScreenMeta, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should emit publishedCurrentScreen explicitly instead of forcing bridge consumers to reuse logicalScreen.");
    Assert(snapshot.Meta.TryGetValue("publishedVisibleScreen", out var publishedVisibleScreenMeta)
           && string.Equals(publishedVisibleScreenMeta, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should emit publishedVisibleScreen explicitly from direct observation instead of forcing bridge consumers to reuse compatibility visibleScreen.");

    var buildInventoryMethod = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.InventoryPublisher")
        ?.GetMethod("BuildInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(buildInventoryMethod is not null, "Expected private InventoryPublisher.BuildInventory helper.");
    var normalizedScene = new CompanionNormalizedScene("map", "map", 1.0, "test");
    var inventory = buildInventoryMethod!.Invoke(null, new object?[] { snapshot, "dormant", normalizedScene }) as HarnessNodeInventory;
    Assert(inventory is not null, "Expected inventory publisher to build an inventory from the tracker snapshot.");
    Assert(string.Equals(inventory!.RawSceneType, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory should preserve raw scene type separately from compatibility scene type.");
    Assert(string.Equals(inventory.RawCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory should expose raw current screen explicitly instead of forcing consumers to overload rawSceneType.");
    Assert(string.Equals(inventory.SceneType, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory primary scene type should follow published scene provenance when it agrees with compatibility scene type.");
    Assert(string.Equals(inventory.CompatibilitySceneType, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory should expose compatibility scene type explicitly.");
    Assert(string.Equals(inventory.CompatibilityLogicalScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory should expose compatibility logical screen explicitly instead of forcing consumers to overload compatibility scene type.");
    Assert(string.Equals(inventory.CompatibilityCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory should expose compatibility current screen explicitly instead of overloading sceneType.");
    Assert(string.Equals(inventory.CompatibilityVisibleScene, "map", StringComparison.OrdinalIgnoreCase), "Inventory should preserve compatibility visible scene for downstream diagnostics.");
    Assert(string.Equals(inventory.CompatibilityVisibleScreen, "map", StringComparison.OrdinalIgnoreCase), "Inventory should expose compatibility visible screen explicitly for downstream consumers.");
    Assert(inventory.CompatibilitySceneReady == false, "Inventory should preserve compatibility scene-ready instead of recomputing raw winner truth.");
}

static void TestTrackerReEmitsAdditiveScreenProvenanceAliases()
{
    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live");
    var observation = new LiveExportObservation(
        "runtime-poll",
        DateTimeOffset.UtcNow,
        "run-provenance-alias-self-test",
        "active",
        "map",
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
            ["rawCurrentScreen"] = "rewards",
            ["instanceType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
            ["currentSceneType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
            ["rootTypeSummary"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
        });

    var snapshot = tracker.Apply(observation).Snapshot;
    Assert(string.Equals(snapshot.RawCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should expose raw current screen directly when additive rawCurrentScreen input is provided.");
    Assert(string.Equals(snapshot.RawObservedScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should accept rawCurrentScreen input as the raw observed screen source.");
    Assert(string.Equals(snapshot.PublishedCurrentScreen, "map", StringComparison.OrdinalIgnoreCase), "Tracker should preserve published current screen separately from additive raw input.");
    Assert(string.Equals(snapshot.PublishedVisibleScreen, "map", StringComparison.OrdinalIgnoreCase), "Tracker should preserve published visible screen separately from additive raw input.");
    Assert(string.Equals(snapshot.CompatibilityCurrentScreen, "map", StringComparison.OrdinalIgnoreCase), "Tracker should keep compatibility current screen sourced from tracker shaping.");
    Assert(string.Equals(snapshot.CompatibilityLogicalScreen, "map", StringComparison.OrdinalIgnoreCase), "Tracker should keep compatibility logical screen sourced from tracker shaping.");
    Assert(string.Equals(snapshot.CompatibilityVisibleScreen, "map", StringComparison.OrdinalIgnoreCase), "Tracker should keep compatibility visible screen sourced from tracker shaping.");
    Assert(snapshot.Meta.TryGetValue("rawObservedScreen", out var rawObservedScreenMeta)
           && string.Equals(rawObservedScreenMeta, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should preserve rawObservedScreen meta when rawCurrentScreen input is provided.");
    Assert(snapshot.Meta.TryGetValue("rawCurrentScreen", out var rawCurrentScreenMeta)
           && string.Equals(rawCurrentScreenMeta, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker should re-emit rawCurrentScreen meta after reading additive input.");
    Assert(snapshot.Meta.TryGetValue("compatibilityCurrentScreen", out var compatibilityCurrentScreenMeta)
           && string.Equals(compatibilityCurrentScreenMeta, "map", StringComparison.OrdinalIgnoreCase), "Tracker should emit compatibilityCurrentScreen even when only additive raw input was provided.");
    Assert(snapshot.Meta.TryGetValue("compatibilityVisibleScreen", out var compatibilityVisibleScreenMeta)
           && string.Equals(compatibilityVisibleScreenMeta, "map", StringComparison.OrdinalIgnoreCase), "Tracker should emit compatibilityVisibleScreen even when only additive raw input was provided.");
    Assert(snapshot.Meta.TryGetValue("publishedCurrentScreen", out var publishedCurrentScreenMeta)
           && string.Equals(publishedCurrentScreenMeta, "map", StringComparison.OrdinalIgnoreCase), "Tracker should emit publishedCurrentScreen even when only additive raw input was provided.");
    Assert(snapshot.Meta.TryGetValue("publishedVisibleScreen", out var publishedVisibleScreenMeta)
           && string.Equals(publishedVisibleScreenMeta, "map", StringComparison.OrdinalIgnoreCase), "Tracker should emit publishedVisibleScreen even when only additive raw input was provided.");
}

static void TestInventoryPublisherSeparatesPrimaryAndCompatibilitySceneProvenance()
{
    var buildInventoryMethod = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.InventoryPublisher")
        ?.GetMethod("BuildInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(buildInventoryMethod is not null, "Expected private InventoryPublisher.BuildInventory helper.");

    var snapshot = CreateInventoryPublisherSnapshot(
        Array.Empty<LiveExportChoiceSummary>()) with
    {
        CurrentScreen = "rewards",
        RawObservedScreen = "rewards",
        PublishedCurrentScreen = "event",
        PublishedVisibleScreen = "event",
        PublishedSceneReady = true,
        PublishedSceneAuthority = "polling",
        PublishedSceneStability = "stable",
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
    Assert(string.Equals(inventory!.SceneType, "event", StringComparison.OrdinalIgnoreCase), "Inventory primary scene type should preserve published scene provenance even when compatibility logical-screen disagrees.");
    Assert(inventory.SceneReady == true, "Inventory primary scene-ready should preserve published readiness instead of compatibility readiness.");
    Assert(string.Equals(inventory.SceneAuthority, "polling", StringComparison.OrdinalIgnoreCase), "Inventory primary scene authority should preserve published authority instead of compatibility authority.");
    Assert(string.Equals(inventory.SceneStability, "stable", StringComparison.OrdinalIgnoreCase), "Inventory primary scene stability should preserve published stability instead of compatibility stability.");
    Assert(string.Equals(inventory.PublishedSceneType, "event", StringComparison.OrdinalIgnoreCase), "Inventory published scene type should preserve explicit published provenance even when compatibility scene type disagrees.");
    Assert(string.Equals(inventory.PublishedVisibleScene, "event", StringComparison.OrdinalIgnoreCase), "Inventory published visible scene should preserve explicit published provenance even when compatibility visible scene disagrees.");
    Assert(inventory.PublishedSceneReady == true, "Inventory published scene-ready should preserve explicit published provenance.");
    Assert(string.Equals(inventory.PublishedSceneAuthority, "polling", StringComparison.OrdinalIgnoreCase), "Inventory published scene authority should preserve explicit published provenance.");
    Assert(string.Equals(inventory.PublishedSceneStability, "stable", StringComparison.OrdinalIgnoreCase), "Inventory published scene stability should preserve explicit published provenance.");
    Assert(string.Equals(inventory.CompatibilityLogicalScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory should preserve explicit compatibility logical screen separately from scene type.");
    Assert(string.Equals(inventory.CompatibilityVisibleScene, "map", StringComparison.OrdinalIgnoreCase), "Inventory visible scene should follow explicit compatibility visible screen, not legacy visibleScreen meta.");
    Assert(inventory.CompatibilitySceneReady == false, "Inventory scene-ready should preserve explicit compatibility truth.");
    Assert(string.Equals(inventory.CompatibilitySceneAuthority, "mixed", StringComparison.OrdinalIgnoreCase), "Inventory scene authority should preserve explicit compatibility truth.");
    Assert(string.Equals(inventory.CompatibilitySceneStability, "stabilizing", StringComparison.OrdinalIgnoreCase), "Inventory scene stability should preserve explicit compatibility truth.");
}

static void TestPublishedSceneProvenanceIgnoresLegacyCompatibilityMeta()
{
    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live");
    var observation = new LiveExportObservation(
        "runtime-poll",
        DateTimeOffset.UtcNow,
        "run-published-scene-strictness-self-test",
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
            ["logicalScreen"] = "event",
            ["visibleScreen"] = "map",
            ["sceneReady"] = "true",
            ["sceneAuthority"] = "legacy",
            ["sceneStability"] = "stable",
        });

    var snapshot = tracker.Apply(observation).Snapshot;
    Assert(string.Equals(snapshot.PublishedCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker published current screen should come from direct observed truth, not legacy logicalScreen.");
    Assert(string.Equals(snapshot.PublishedVisibleScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Tracker published visible screen should stay on explicit published/direct truth, not legacy visibleScreen.");
    Assert(snapshot.PublishedSceneReady is null, "Tracker published scene-ready should ignore legacy sceneReady compatibility meta.");
    Assert(string.IsNullOrWhiteSpace(snapshot.PublishedSceneAuthority), "Tracker published scene authority should ignore legacy sceneAuthority compatibility meta.");
    Assert(string.IsNullOrWhiteSpace(snapshot.PublishedSceneStability), "Tracker published scene stability should ignore legacy sceneStability compatibility meta.");
    Assert(string.Equals(snapshot.CompatibilityLogicalScreen, "event", StringComparison.OrdinalIgnoreCase), "Tracker compatibility logical screen should still preserve legacy logicalScreen.");
    Assert(string.Equals(snapshot.CompatibilityVisibleScreen, "map", StringComparison.OrdinalIgnoreCase), "Tracker compatibility visible screen should still preserve legacy visibleScreen.");
    Assert(snapshot.CompatibilitySceneReady == true, "Tracker compatibility scene-ready should keep legacy sceneReady.");
    Assert(string.Equals(snapshot.CompatibilitySceneAuthority, "legacy", StringComparison.OrdinalIgnoreCase), "Tracker compatibility scene authority should keep legacy sceneAuthority.");
    Assert(string.Equals(snapshot.CompatibilitySceneStability, "stable", StringComparison.OrdinalIgnoreCase), "Tracker compatibility scene stability should keep legacy sceneStability.");

    var buildInventoryMethod = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.InventoryPublisher")
        ?.GetMethod("BuildInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(buildInventoryMethod is not null, "Expected private InventoryPublisher.BuildInventory helper.");
    var normalizedScene = new CompanionNormalizedScene("event", "event", 1.0, "test");
    var inventory = buildInventoryMethod!.Invoke(null, new object?[] { snapshot, "dormant", normalizedScene }) as HarnessNodeInventory;
    Assert(inventory is not null, "Expected inventory publisher to build an inventory from the strict published snapshot.");
    Assert(string.Equals(inventory!.SceneType, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory primary scene type should preserve published/direct screen truth instead of compatibility logical-screen truth.");
    Assert(inventory.SceneReady is null, "Inventory primary scene-ready should stay empty when only compatibility readiness exists.");
    Assert(string.IsNullOrWhiteSpace(inventory.SceneAuthority), "Inventory primary scene authority should stay empty when only compatibility authority exists.");
    Assert(string.IsNullOrWhiteSpace(inventory.SceneStability), "Inventory primary scene stability should stay empty when only compatibility stability exists.");
    Assert(string.Equals(inventory.PublishedSceneType, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory published scene type should stay on published/direct screen truth.");
    Assert(string.Equals(inventory.PublishedVisibleScene, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory published visible scene should stay on published/direct screen truth.");
    Assert(inventory.PublishedSceneReady is null, "Inventory published scene-ready should ignore legacy compatibility sceneReady meta.");
    Assert(string.IsNullOrWhiteSpace(inventory.PublishedSceneAuthority), "Inventory published scene authority should ignore legacy compatibility sceneAuthority meta.");
    Assert(string.IsNullOrWhiteSpace(inventory.PublishedSceneStability), "Inventory published scene stability should ignore legacy compatibility sceneStability meta.");
    Assert(string.Equals(inventory.CompatibilitySceneType, "event", StringComparison.OrdinalIgnoreCase), "Inventory compatibility scene type should continue to preserve legacy logicalScreen.");
    Assert(string.Equals(inventory.CompatibilityVisibleScene, "map", StringComparison.OrdinalIgnoreCase), "Inventory compatibility visible scene should continue to preserve legacy visibleScreen.");
    Assert(inventory.CompatibilitySceneReady == true, "Inventory compatibility scene-ready should continue to preserve legacy sceneReady.");
    Assert(string.Equals(inventory.CompatibilitySceneAuthority, "legacy", StringComparison.OrdinalIgnoreCase), "Inventory compatibility scene authority should continue to preserve legacy sceneAuthority.");
    Assert(string.Equals(inventory.CompatibilitySceneStability, "stable", StringComparison.OrdinalIgnoreCase), "Inventory compatibility scene stability should continue to preserve legacy sceneStability.");
}

static void TestInventoryPublisherKeepsPrimarySceneUnknownWhenOnlyCompatibilityWinnerExists()
{
    var buildInventoryMethod = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.InventoryPublisher")
        ?.GetMethod("BuildInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(buildInventoryMethod is not null, "Expected private InventoryPublisher.BuildInventory helper.");

    var snapshot = LiveExportSnapshot.CreateEmpty("run-compat-only-primary-scene-self-test") with
    {
        CurrentScreen = "event",
        CompatibilityCurrentScreen = "event",
        CompatibilityLogicalScreen = "event",
        CompatibilityVisibleScreen = "map",
        CompatibilitySceneReady = true,
        CompatibilitySceneAuthority = "legacy",
        CompatibilitySceneStability = "stable",
        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["compatibilityCurrentScreen"] = "event",
            ["compatLogicalScreen"] = "event",
            ["compatibilityVisibleScreen"] = "map",
            ["compatSceneReady"] = "true",
            ["compatSceneAuthority"] = "legacy",
            ["compatSceneStability"] = "stable",
        },
    };

    var normalizedScene = new CompanionNormalizedScene("event", "event", 1.0, "test");
    var inventory = buildInventoryMethod!.Invoke(null, new object?[] { snapshot, "dormant", normalizedScene }) as HarnessNodeInventory;
    Assert(inventory is not null, "Expected inventory publisher to build an inventory from the compatibility-only snapshot.");
    Assert(string.Equals(inventory!.SceneType, "unknown", StringComparison.OrdinalIgnoreCase), "Inventory primary scene type should stay unknown when only compatibility winner shaping is available.");
    Assert(string.IsNullOrWhiteSpace(inventory.RawSceneType), "Inventory raw scene type should stay empty when no raw provenance exists.");
    Assert(string.IsNullOrWhiteSpace(inventory.PublishedSceneType), "Inventory published scene type should stay empty when no published provenance exists.");
    Assert(string.Equals(inventory.CompatibilitySceneType, "event", StringComparison.OrdinalIgnoreCase), "Inventory compatibility scene type should keep the legacy compatibility winner separately from the primary scene.");
    Assert(string.Equals(inventory.CompatibilityVisibleScene, "map", StringComparison.OrdinalIgnoreCase), "Inventory compatibility visible scene should remain available for legacy diagnostics.");
    Assert(inventory.SceneReady is null
           && string.IsNullOrWhiteSpace(inventory.SceneAuthority)
           && string.IsNullOrWhiteSpace(inventory.SceneStability), "Inventory primary scene diagnostics should stay empty when only compatibility diagnostics exist.");
    Assert(inventory.CompatibilitySceneReady == true
           && string.Equals(inventory.CompatibilitySceneAuthority, "legacy", StringComparison.OrdinalIgnoreCase)
           && string.Equals(inventory.CompatibilitySceneStability, "stable", StringComparison.OrdinalIgnoreCase), "Inventory compatibility diagnostics should remain available for legacy readers.");
}

static void TestInventoryPublisherPromotesTrackerCombatPrimaryScene()
{
    var buildInventoryMethod = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.InventoryPublisher")
        ?.GetMethod("BuildInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(buildInventoryMethod is not null, "Expected private InventoryPublisher.BuildInventory helper.");

    var snapshot = CreateInventoryPublisherSnapshot(
        new[]
        {
            new LiveExportChoiceSummary(
                Kind: "enemy-target",
                Label: "Jaw Worm",
                Value: null,
                Description: "combat target")
            {
                NodeId = "enemy-target:jaw-worm",
                ScreenBounds = "720,180,180,260",
            },
        }) with
    {
        CurrentScreen = "combat",
        PublishedCurrentScreen = "map",
        PublishedVisibleScreen = "map",
        RawObservedScreen = "map",
        Encounter = new LiveExportEncounterSummary("Jaw Worm", "Combat", true, 1),
    };

    var normalizedScene = new CompanionNormalizedScene("map", "map", 1.0, "test");
    var inventory = buildInventoryMethod!.Invoke(null, new object?[] { snapshot, "dormant", normalizedScene }) as HarnessNodeInventory;
    Assert(inventory is not null, "Expected inventory publisher to build an inventory from the combat tracker snapshot.");
    Assert(string.Equals(inventory!.SceneType, "combat", StringComparison.OrdinalIgnoreCase), "Inventory primary scene type should promote tracker combat truth over stale published map provenance.");
    Assert(string.Equals(inventory.PublishedSceneType, "map", StringComparison.OrdinalIgnoreCase), "Inventory should preserve published scene provenance separately while promoting tracker combat truth into the primary scene type.");
    Assert(inventory.Nodes.Count == 1, "Expected one combat target node in the built inventory.");
    Assert(inventory.Nodes[0].SemanticHints.Contains("scene:combat", StringComparer.OrdinalIgnoreCase), "Inventory node hints should follow the promoted combat primary scene when tracker combat truth overrides stale published map provenance.");
    Assert(inventory.Nodes[0].SemanticHints.Contains("scene-published:map", StringComparer.OrdinalIgnoreCase), "Inventory node hints should still expose the published map provenance separately for diagnostics.");
}

static void TestInventoryPublisherNodeHintsPreferPrimarySceneProvenance()
{
    var buildInventoryMethod = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.InventoryPublisher")
        ?.GetMethod("BuildInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(buildInventoryMethod is not null, "Expected private InventoryPublisher.BuildInventory helper.");

    var snapshot = LiveExportSnapshot.CreateEmpty("run-node-primary-provenance-self-test") with
    {
        CurrentScreen = "rewards",
        RawCurrentScreen = "event",
        RawObservedScreen = "event",
        PublishedCurrentScreen = "event",
        PublishedVisibleScreen = "event",
        CompatibilityLogicalScreen = "rewards",
        CompatibilityCurrentScreen = "rewards",
        CompatibilityVisibleScreen = "rewards",
        CurrentChoices = new[]
        {
            new LiveExportChoiceSummary(
                Kind: string.Empty,
                Label: "Proceed",
                Value: "event:proceed",
                Description: "source:event-option-button")
            {
                ScreenBounds = "100,100,64,32",
                NodeId = "event:proceed",
            }
        },
    };

    var normalizedScene = new CompanionNormalizedScene("rewards", "rewards", 1.0, "test");
    var inventory = buildInventoryMethod!.Invoke(null, new object?[] { snapshot, "dormant", normalizedScene }) as HarnessNodeInventory;
    Assert(inventory is not null, "Expected inventory publisher to build a node inventory from mixed primary and compatibility scene provenance.");
    Assert(inventory!.Nodes.Count == 1, "Expected exactly one node in the mixed provenance inventory.");

    var node = inventory.Nodes[0];
    Assert(string.Equals(node.Kind, "event-option", StringComparison.OrdinalIgnoreCase), "Inventory node kind should follow published/raw primary scene provenance instead of the compatibility scene winner.");
    Assert(node.SemanticHints.Contains("scene:event", StringComparer.OrdinalIgnoreCase), "Inventory node semantic hints should expose the primary scene provenance.");
    Assert(!node.SemanticHints.Contains("scene-compat:rewards", StringComparer.OrdinalIgnoreCase), "Inventory node semantic hints should not restate compatibility-scene winner hints at the node layer.");
    Assert(!node.SemanticHints.Contains("scene:rewards", StringComparer.OrdinalIgnoreCase), "Inventory node primary scene hint should not collapse back to the compatibility scene winner.");
    Assert(string.Equals(inventory.CompatibilitySceneType, "rewards", StringComparison.OrdinalIgnoreCase), "Inventory root should still preserve compatibility scene provenance separately from node semantics.");
}

static void TestInventoryPublisherDoesNotPromoteCompatibilitySceneWinnerIntoNodeSemantics()
{
    var buildInventoryMethod = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.InventoryPublisher")
        ?.GetMethod("BuildInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(buildInventoryMethod is not null, "Expected private InventoryPublisher.BuildInventory helper.");

    var snapshot = LiveExportSnapshot.CreateEmpty("run-node-compat-winner-self-test") with
    {
        CurrentScreen = "event",
        CompatibilityLogicalScreen = "event",
        CompatibilityCurrentScreen = "event",
        CompatibilityVisibleScreen = "event",
        CurrentChoices = new[]
        {
            new LiveExportChoiceSummary(
                Kind: string.Empty,
                Label: "Proceed",
                Value: "event:proceed",
                Description: "source:event-option-button")
            {
                ScreenBounds = "100,100,64,32",
                NodeId = "event:proceed",
            }
        },
    };

    var normalizedScene = new CompanionNormalizedScene("event", "event", 1.0, "test");
    var inventory = buildInventoryMethod!.Invoke(null, new object?[] { snapshot, "dormant", normalizedScene }) as HarnessNodeInventory;
    Assert(inventory is not null, "Expected inventory publisher to build a node inventory from compatibility-only scene truth.");
    Assert(inventory!.Nodes.Count == 1, "Expected exactly one node in the compatibility-only inventory.");

    var node = inventory.Nodes[0];
    Assert(string.Equals(node.Kind, "choice", StringComparison.OrdinalIgnoreCase), "Inventory node kind should not be promoted from compatibility scene winners when published/raw scene provenance is absent.");
    Assert(!node.SemanticHints.Contains("scene:event", StringComparer.OrdinalIgnoreCase), "Inventory node primary scene hint should stay empty when only compatibility scene provenance exists.");
    Assert(!node.SemanticHints.Contains("scene-compat:event", StringComparer.OrdinalIgnoreCase), "Inventory node semantic hints should not promote compatibility scene winners at the node layer.");
    Assert(string.Equals(inventory.CompatibilitySceneType, "event", StringComparison.OrdinalIgnoreCase), "Inventory root should preserve compatibility scene provenance even when node semantics stay passthrough.");
}

static void TestInventoryPublisherSuppressesImmediatePublishForMixedProvenance()
{
    var publisherType = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.InventoryPublisher");
    Assert(publisherType is not null, "Expected InventoryPublisher type.");
    var buildInventoryMethod = publisherType!.GetMethod("BuildInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    var shouldSuppressMethod = publisherType.GetMethod("ShouldSuppressPublish", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    Assert(buildInventoryMethod is not null, "Expected private InventoryPublisher.BuildInventory helper.");
    Assert(shouldSuppressMethod is not null, "Expected private InventoryPublisher.ShouldSuppressPublish helper.");

    var publisher = Activator.CreateInstance(
        publisherType,
        Path.Combine(Path.GetTempPath(), $"inventory-publisher-self-test-{Guid.NewGuid():N}.json"),
        Path.Combine(Path.GetTempPath(), $"live-snapshot-self-test-{Guid.NewGuid():N}.json"));
    Assert(publisher is not null, "Expected InventoryPublisher instance.");

    var snapshot = CreateInventoryPublisherSnapshot(Array.Empty<LiveExportChoiceSummary>()) with
    {
        CurrentScreen = "map",
        PublishedCurrentScreen = "map",
        PublishedVisibleScreen = "map",
        RawObservedScreen = "rewards",
    };

    var normalizedScene = new CompanionNormalizedScene("map", "map", 1.0, "test");
    var inventory = buildInventoryMethod!.Invoke(null, new object?[] { snapshot, "dormant", normalizedScene }) as HarnessNodeInventory;
    Assert(inventory is not null, "Expected inventory publisher to build an inventory.");

    var shouldSuppress = shouldSuppressMethod!.Invoke(publisher, new object?[] { inventory! });
    Assert(shouldSuppress is bool suppressed && suppressed, "Inventory publisher should suppress the first publish when published/raw screen provenance still disagrees even without compatibility instability.");
}

static void TestInventoryPublisherFingerprintTracksProvenanceFields()
{
    var publisherType = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.InventoryPublisher");
    Assert(publisherType is not null, "Expected InventoryPublisher type.");
    var buildFingerprintMethod = publisherType!.GetMethod("BuildFingerprint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(buildFingerprintMethod is not null, "Expected private InventoryPublisher.BuildFingerprint helper.");

    var baseline = new HarnessNodeInventory(
        "inventory-1",
        DateTimeOffset.UtcNow,
        "run-id",
        "rewards",
        "episode-1",
        "dormant",
        null,
        false,
        "mixed",
        "stabilizing",
        Array.Empty<HarnessNodeInventoryItem>())
    {
        RawSceneType = "rewards",
        RawCurrentScreen = "rewards",
        CompatibilitySceneType = "rewards",
        CompatibilityLogicalScreen = "rewards",
        CompatibilityCurrentScreen = "rewards",
        CompatibilityVisibleScene = "map",
        CompatibilityVisibleScreen = "map",
        CompatibilitySceneReady = false,
        CompatibilitySceneAuthority = "mixed",
        CompatibilitySceneStability = "stabilizing",
    };

    var changedVisible = baseline with
    {
        CompatibilityVisibleScene = "rewards",
        CompatibilityVisibleScreen = "rewards",
    };

    var baselineFingerprint = buildFingerprintMethod!.Invoke(null, new object?[] { baseline }) as string;
    var changedVisibleFingerprint = buildFingerprintMethod.Invoke(null, new object?[] { changedVisible }) as string;
    Assert(!string.IsNullOrWhiteSpace(baselineFingerprint), "Expected inventory publisher fingerprint.");
    Assert(!string.IsNullOrWhiteSpace(changedVisibleFingerprint), "Expected changed inventory publisher fingerprint.");
    Assert(!string.Equals(baselineFingerprint, changedVisibleFingerprint, StringComparison.Ordinal), "Inventory publisher fingerprint should change when compatibility provenance changes even without node churn.");
}

static void TestHarnessBridgeGuardSurfaceRequiresPublishedProvenance()
{
    var hostType = typeof(HarnessBridgeEntryPoint).Assembly.GetType("Sts2ModAiCompanion.HarnessBridge.HarnessBridgeHost");
    Assert(hostType is not null, "Expected HarnessBridgeHost type.");

    var resolveReadyMethod = hostType!.GetMethod("ResolveGuardSceneReady", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    var resolveAuthorityMethod = hostType.GetMethod("ResolveGuardSceneAuthority", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    var resolveStabilityMethod = hostType.GetMethod("ResolveGuardSceneStability", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(resolveReadyMethod is not null && resolveAuthorityMethod is not null && resolveStabilityMethod is not null, "Expected private harness-bridge guard provenance helpers.");

    var compatibilityOnlyInventory = new HarnessNodeInventory(
        "compat-only-inventory",
        DateTimeOffset.UtcNow,
        null,
        "main-menu",
        "episode",
        "armed",
        null,
        true,
        "compat",
        "stable",
        Array.Empty<HarnessNodeInventoryItem>())
    {
        CompatibilitySceneReady = true,
        CompatibilitySceneAuthority = "compat",
        CompatibilitySceneStability = "stable",
    };

    Assert(resolveReadyMethod.Invoke(null, new object?[] { compatibilityOnlyInventory }) is null, "Guard scene-ready should ignore compatibility-only provenance.");
    Assert(resolveAuthorityMethod.Invoke(null, new object?[] { compatibilityOnlyInventory }) is null, "Guard scene-authority should ignore compatibility-only provenance.");
    Assert(resolveStabilityMethod.Invoke(null, new object?[] { compatibilityOnlyInventory }) is null, "Guard scene-stability should ignore compatibility-only provenance.");

    var publishedInventory = compatibilityOnlyInventory with
    {
        PublishedSceneReady = true,
        PublishedSceneAuthority = "published",
        PublishedSceneStability = "stable",
    };

    Assert(resolveReadyMethod.Invoke(null, new object?[] { publishedInventory }) is bool publishedReady && publishedReady, "Guard scene-ready should accept explicit published provenance.");
    Assert(string.Equals(resolveAuthorityMethod.Invoke(null, new object?[] { publishedInventory }) as string, "published", StringComparison.OrdinalIgnoreCase), "Guard scene-authority should accept explicit published provenance.");
    Assert(string.Equals(resolveStabilityMethod.Invoke(null, new object?[] { publishedInventory }) as string, "stable", StringComparison.OrdinalIgnoreCase), "Guard scene-stability should accept explicit published provenance.");
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
    Assert(cards!.Count == 4, $"Expected duplicate combat cards to be preserved, got {cards.Count}.");
    Assert(cards.Count(card => card.Name == "Strike") == 2, "Expected both Strike copies to remain present.");
    Assert(cards.Any(card => card.Name == "Bash"), "Expected Bash to be present.");
}

static void TestRuntimeReflectionPrefersDeckSourceOverCombatZones()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod("ExtractDeck", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(method is not null, "Expected private ExtractDeck helper.");

    var authoritativeDeck = new FakeDeckPile
    {
        Cards = new object[]
        {
            new FakeCombatCard { Name = "Strike", CardId = "STRIKE", Cost = 1, Type = "attack" },
            new FakeCombatCard { Name = "Strike", CardId = "STRIKE", Cost = 1, Type = "attack" },
            new FakeCombatCard { Name = "Defend", CardId = "DEFEND", Cost = 1, Type = "skill" },
        },
    };

    var roots = new object[]
    {
        new FakePlayerEntity
        {
            Name = "Ironclad",
            Deck = authoritativeDeck,
        },
        new FakeCombatPlayerRoot
        {
            PlayerCombatState = new FakePlayerCombatState
            {
                Hand = new object[]
                {
                    new FakeCombatCard { Name = "Noise Hand", CardId = "NOISE_HAND", Cost = 1, Type = "attack" },
                },
                DrawPile = new object[]
                {
                    new FakeCombatCard { Name = "Noise Draw", CardId = "NOISE_DRAW", Cost = 1, Type = "attack" },
                },
                DiscardPile = new object[]
                {
                    new FakeCombatCard { Name = "Noise Discard", CardId = "NOISE_DISCARD", Cost = 1, Type = "attack" },
                },
            },
        },
    };

    var cards = method!.Invoke(null, new object?[] { roots, 16 }) as IReadOnlyList<LiveExportCardSummary>;
    Assert(cards is not null, "Expected ExtractDeck to return a card list.");
    Assert(cards!.Count == 3, $"Expected deck extraction to prefer deck source only, got {cards.Count}.");
    Assert(cards.Count(card => card.Name == "Strike") == 2, "Expected both Strike copies from deck source to remain present.");
    Assert(cards.All(card => !card.Name!.StartsWith("Noise", StringComparison.Ordinal)), "Expected combat-zone noise cards to stay out of deck extraction.");
}

static void TestRuntimeReflectionIgnoresStandaloneDeckAliasesWhenPlayerDeckPresent()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod("ExtractDeck", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(method is not null, "Expected private ExtractDeck helper.");

    var playerDeck = new FakeDeckPile
    {
        Cards = new object[]
        {
            new FakeCombatCard { Name = "Strike", CardId = "STRIKE", Cost = 1, Type = "attack" },
            new FakeCombatCard { Name = "Defend", CardId = "DEFEND", Cost = 1, Type = "skill" },
        },
    };

    var aliasDeck = new FakeDeckPile
    {
        Cards = new object[]
        {
            new FakeCombatCard { Name = "Strike", CardId = "STRIKE", Cost = 1, Type = "attack" },
            new FakeCombatCard { Name = "Defend", CardId = "DEFEND", Cost = 1, Type = "skill" },
            new FakeCombatCard { Name = "Bash", CardId = "BASH", Cost = 2, Type = "attack" },
        },
    };

    var roots = new object[]
    {
        new FakePlayerEntity
        {
            Name = "Ironclad",
            Deck = playerDeck,
        },
        aliasDeck,
    };

    var cards = method!.Invoke(null, new object?[] { roots, 16 }) as IReadOnlyList<LiveExportCardSummary>;
    Assert(cards is not null, "Expected ExtractDeck to return a card list.");
    Assert(cards!.Count == 2, $"Expected standalone deck aliases to be ignored when player deck is present, got {cards.Count}.");
    Assert(cards.Any(card => card.Name == "Strike"), "Expected Strike to remain present.");
    Assert(cards.Any(card => card.Name == "Defend"), "Expected Defend to remain present.");
    Assert(cards.All(card => card.Name != "Bash"), "Expected standalone deck alias cards to stay out of authoritative deck extraction.");
}

static void TestRuntimeReflectionExportsStructuredCombatCounts()
{
    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeCombatPlayerRoot
            {
                PlayerCombatState = new FakePlayerCombatState
                {
                    Hand = new object[]
                    {
                        new FakeCombatCard { Name = "Strike", CardId = "STRIKE", Cost = 1, Type = "attack" },
                        new FakeCombatCard { Name = "Defend", CardId = "DEFEND", Cost = 1, Type = "skill" },
                        new FakeCombatCard { Name = "Bash", CardId = "BASH", Cost = 2, Type = "attack" },
                    },
                    DrawPile = new object[]
                    {
                        new FakeCombatCard { Name = "Pommel Strike", CardId = "POMMEL_STRIKE", Cost = 1, Type = "attack" },
                        new FakeCombatCard { Name = "Twin Strike", CardId = "TWIN_STRIKE", Cost = 1, Type = "attack" },
                    },
                    DiscardPile = new object[]
                    {
                        new FakeCombatCard { Name = "Shrug It Off", CardId = "SHRUG", Cost = 1, Type = "skill" },
                    },
                    ExhaustPile = new object[]
                    {
                        new FakeCombatCard { Name = "Offering", CardId = "OFFERING", Cost = 0, Type = "skill" },
                    },
                    PlayPile = new object[]
                    {
                        new FakeCombatCard { Name = "Strike+", CardId = "STRIKE_PLUS", Cost = 1, Type = "attack" },
                        new FakeCombatCard { Name = "Defend+", CardId = "DEFEND_PLUS", Cost = 1, Type = "skill" },
                    },
                },
            },
            new FakeCombatManagerState
            {
                IsInProgress = true,
                IsPlayPhase = true,
                PlayerActionsDisabled = false,
            },
            new FakeCombatState
            {
                RoundNumber = 2,
                Cards = Enumerable.Range(0, 12)
                    .Select(index => (object)new FakeCombatCard { Name = $"Noise {index}", CardId = $"NOISE_{index}", Cost = 1, Type = "attack" })
                    .ToArray(),
            },
        },
        "combat");

    Assert(observation.CombatHandCount == 3, $"Expected structured combat hand count 3, got {observation.CombatHandCount?.ToString() ?? "<null>"}.");
    Assert(observation.DrawPileCount == 2, $"Expected structured draw pile count 2, got {observation.DrawPileCount?.ToString() ?? "<null>"}.");
    Assert(observation.DiscardPileCount == 1, $"Expected structured discard pile count 1, got {observation.DiscardPileCount?.ToString() ?? "<null>"}.");
    Assert(observation.ExhaustPileCount == 1, $"Expected structured exhaust pile count 1, got {observation.ExhaustPileCount?.ToString() ?? "<null>"}.");
    Assert(observation.PlayPileCount == 2, $"Expected structured play pile count 2, got {observation.PlayPileCount?.ToString() ?? "<null>"}.");
    Assert(observation.Meta.TryGetValue("combatHandCount", out var combatHandCount) && combatHandCount == "3", "Expected combatHandCount meta mirror.");
    Assert(observation.Meta.TryGetValue("combatHandSummary", out var combatHandSummary)
           && combatHandSummary is not null
           && combatHandSummary.Contains("Strike", StringComparison.Ordinal)
           && !combatHandSummary.Contains("CARD.STRIKE", StringComparison.OrdinalIgnoreCase),
        $"Expected combatHandSummary to prefer observed card names over raw ids, got '{combatHandSummary ?? "<null>"}'.");
    Assert(observation.Meta.TryGetValue("drawPileCount", out var drawPileCount) && drawPileCount == "2", "Expected drawPileCount meta mirror.");
    Assert(observation.Meta.TryGetValue("discardPileCount", out var discardPileCount) && discardPileCount == "1", "Expected discardPileCount meta mirror.");
    Assert(observation.Meta.TryGetValue("exhaustPileCount", out var exhaustPileCount) && exhaustPileCount == "1", "Expected exhaustPileCount meta mirror.");
    Assert(observation.Meta.TryGetValue("playPileCount", out var playPileCount) && playPileCount == "2", "Expected playPileCount meta mirror.");
    Assert(observation.Payload.TryGetValue("combatHandCount", out var payloadCombatHandCount) && payloadCombatHandCount is 3, "Expected combatHandCount payload mirror.");
    Assert(observation.Payload.TryGetValue("drawPileCount", out var payloadDrawPileCount) && payloadDrawPileCount is 2, "Expected drawPileCount payload mirror.");
    Assert(observation.Payload.TryGetValue("discardPileCount", out var payloadDiscardPileCount) && payloadDiscardPileCount is 1, "Expected discardPileCount payload mirror.");
    Assert(observation.Payload.TryGetValue("exhaustPileCount", out var payloadExhaustPileCount) && payloadExhaustPileCount is 1, "Expected exhaustPileCount payload mirror.");
    Assert(observation.Payload.TryGetValue("playPileCount", out var payloadPlayPileCount) && payloadPlayPileCount is 2, "Expected playPileCount payload mirror.");
}

static void TestRuntimeReflectionPrefersEvaluatedDisplayTextForCardLikeOptions()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var shopCandidateMethod = extractorType!.GetMethod("TryCreateShopOptionCandidate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    var cardSelectionCandidateMethod = extractorType.GetMethod("TryCreateCardSelectionCandidate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert(shopCandidateMethod is not null, "Expected private TryCreateShopOptionCandidate helper.");
    Assert(cardSelectionCandidateMethod is not null, "Expected private TryCreateCardSelectionCandidate helper.");

    var shopCardSlot = new FakeNMerchantCard("몸풀기", "CARD.WARM_UP", x: 700, y: 260, enabled: true, stocked: true, enoughGold: true)
    {
        _cardNode = new FakeCardNode
        {
            _descriptionLabel = new FakeLabel { Text = "[b]적에게 11 피해를 주고[/b] 카드를 1장 뽑습니다." },
        },
    };
    var shopCardCandidate = shopCandidateMethod.Invoke(null, new object?[] { shopCardSlot });
    Assert(shopCardCandidate is not null, "Expected shop card candidate.");
    Assert(
        string.Equals((string?)ReadProperty(shopCardCandidate!, "Description"), "적에게 11 피해를 주고 카드를 1장 뽑습니다.", StringComparison.Ordinal),
        $"Expected shop card candidate to prefer evaluated card-node description. actual='{(string?)ReadProperty(shopCardCandidate!, "Description") ?? "<null>"}'.");

    var shopPotionSlot = new FakeNMerchantPotion("힘 포션", "POTION.STRENGTH_POTION", x: 980, y: 260, enabled: true, stocked: true, enoughGold: true)
    {
        Entry = new FakeMerchantPotionEntry
        {
            Id = "POTION.STRENGTH_POTION",
            Name = "힘 포션",
            IsStocked = true,
            EnoughGold = true,
            Model = new FakePotionModel
            {
                Id = "POTION.STRENGTH_POTION",
                Name = "힘 포션",
                HoverTips = new object[]
                {
                    new FakeHoverTip
                    {
                        Id = "POTION.STRENGTH_POTION",
                        Title = "힘 포션",
                        Description = "전투 중 힘을 2 얻습니다.",
                    },
                },
            },
        },
    };
    var shopPotionCandidate = shopCandidateMethod.Invoke(null, new object?[] { shopPotionSlot });
    Assert(shopPotionCandidate is not null, "Expected shop potion candidate.");
    Assert(
        string.Equals((string?)ReadProperty(shopPotionCandidate!, "Description"), "전투 중 힘을 2 얻습니다.", StringComparison.Ordinal),
        $"Expected shop potion candidate to prefer evaluated hover-tip description. actual='{(string?)ReadProperty(shopPotionCandidate!, "Description") ?? "<null>"}'.");

    var rewardCardHolder = new FakeCardSelectionHolder
    {
        Visible = true,
        Enabled = true,
        Position = new FakeVector2(320, 420),
        Size = new FakeVector2(180, 254),
        CardModel = new FakeCardModel
        {
            Id = "pommel-strike",
            Name = "Pommel Strike",
            Description = "{CalculatedDamage:diff()} 피해를 주고 카드를 1장 뽑습니다.",
        },
        CardNode = new FakeCardNode
        {
            _descriptionLabel = new FakeLabel { Text = "적에게 10 피해를 주고 카드를 1장 뽑습니다." },
        },
    };
    var rewardCardCandidate = cardSelectionCandidateMethod.Invoke(null, new object?[] { rewardCardHolder, Array.Empty<string>() });
    Assert(rewardCardCandidate is not null, "Expected reward card candidate.");
    Assert(
        string.Equals((string?)ReadProperty(rewardCardCandidate!, "Description"), "적에게 10 피해를 주고 카드를 1장 뽑습니다.", StringComparison.Ordinal),
        $"Expected reward card candidate to prefer evaluated card-node description. actual='{(string?)ReadProperty(rewardCardCandidate!, "Description") ?? "<null>"}'.");
}

static void TestRuntimeReflectionKeepsSanitizedFallbackWhenNoEvaluatedTextExists()
{
    var extractorType = typeof(AiCompanionModEntryPoint).Assembly.GetType("Sts2ModAiCompanion.Mod.Runtime.RuntimeSnapshotReflectionExtractor");
    Assert(extractorType is not null, "Expected runtime snapshot extractor type to exist.");

    var method = extractorType!.GetMethod(
        "TryResolveChoiceDescription",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
        binder: null,
        new[] { typeof(object) },
        modifiers: null);
    Assert(method is not null, "Expected private TryResolveChoiceDescription helper.");

    var rawOnlyCardLikeChoice = new FakeCardLikeChoice
    {
        Description = "[b]{CalculatedDamage:diff()}[/b] 피해를 주고 {IfUpgraded:강화 시 }카드를 1장 뽑습니다.",
    };

    var description = method.Invoke(null, new object?[] { rawOnlyCardLikeChoice }) as string;
    Assert(!string.IsNullOrWhiteSpace(description), "Expected placeholder fallback to leave readable text.");
    Assert(!description!.Contains("{CalculatedDamage:diff()}", StringComparison.Ordinal), $"Expected dynamic placeholder to be removed from fallback description. actual='{description}'.");
    Assert(!description.Contains("{IfUpgraded:강화 시 }", StringComparison.Ordinal), $"Expected conditional placeholder to be removed from fallback description. actual='{description}'.");
    Assert(!description.Contains("[b]", StringComparison.Ordinal), $"Expected BBCode markup to be removed from fallback description. actual='{description}'.");
    Assert(description.Contains("피해를 주고", StringComparison.Ordinal), $"Expected fallback description to keep surrounding readable text. actual='{description}'.");
}

static void TestRuntimeReflectionPrefersPlayerHandUiHolders()
{
    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeRuntimePlayerHand
            {
                ActiveHolders = new object[]
                {
                    new FakeRuntimeHandHolder
                    {
                        CardModel = new FakeRuntimeCombatCard
                        {
                            Id = "CARD.STRIKE_IRONCLAD",
                            Title = "Strike",
                            Type = "Attack",
                        },
                    },
                    new FakeRuntimeHandHolder
                    {
                        CardModel = new FakeRuntimeCombatCard
                        {
                            Id = "CARD.DEFEND_IRONCLAD",
                            Title = "Defend",
                            Type = "Skill",
                        },
                    },
                },
            },
            new FakeCombatPlayerRoot
            {
                PlayerCombatState = new FakePlayerCombatState
                {
                    Hand = Enumerable.Range(0, 10)
                        .Select(index => (object)new FakeCombatCard
                        {
                            Name = $"Noise {index}",
                            CardId = $"NOISE_{index}",
                            Cost = 1,
                            Type = "attack",
                        })
                        .ToArray(),
                },
            },
            new FakeCombatManagerState
            {
                IsInProgress = true,
                IsPlayPhase = true,
                PlayerActionsDisabled = false,
            },
        },
        "combat");

    Assert(observation.CombatHandCount == 2, $"Expected UI hand holders to win over combat-state fallback, got {observation.CombatHandCount?.ToString() ?? "<null>"}.");
    Assert(observation.Meta.TryGetValue("combatHandSummary", out var combatHandSummary)
           && combatHandSummary is not null
           && combatHandSummary.Contains("Strike", StringComparison.Ordinal)
           && combatHandSummary.Contains("Defend", StringComparison.Ordinal)
           && !combatHandSummary.Contains("Noise", StringComparison.Ordinal),
        $"Expected combatHandSummary to come from visible UI holders, got '{combatHandSummary ?? "<null>"}'.");
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

static void TestRuntimeReflectionDoesNotInferCombatFromGlobalStartupTypeBag()
{
    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeCombatManagerState
            {
                IsInProgress = false,
                IsPlayPhase = false,
                IsEnemyTurnStarted = false,
                PlayerActionsDisabled = false,
                EndingPlayerTurnPhaseOne = false,
                EndingPlayerTurnPhaseTwo = false,
            },
            new FakeNGameRoot(),
        },
        null);

    Assert(string.Equals(observation.Screen, "unknown", StringComparison.OrdinalIgnoreCase), "Runtime reflection should not infer combat from a startup root that only exposes NGame plus the global CombatManager singleton.");
    Assert(observation.Meta.TryGetValue("publishedCurrentScreen", out var publishedCurrentScreen)
           && string.Equals(publishedCurrentScreen, "unknown", StringComparison.OrdinalIgnoreCase), "Runtime reflection should keep publishedCurrentScreen unknown until a concrete foreground screen appears.");

    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\startup-live");
    var snapshot = tracker.Apply(observation).Snapshot;
    Assert(string.Equals(snapshot.CurrentScreen, "unknown", StringComparison.OrdinalIgnoreCase), "Tracker should not seed combat from a startup runtime-poll that lacks authoritative combat or foreground screen truth.");
    Assert(snapshot.Encounter?.InCombat != true, "Startup runtime-poll should not be treated as in-combat when CombatManager.IsInProgress is false.");
}

static void TestRuntimeReflectionPrefersActiveFeedbackScreenOverBroadCombatProbe()
{
    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeCombatManagerState
            {
                IsInProgress = false,
                IsPlayPhase = false,
                IsEnemyTurnStarted = false,
                PlayerActionsDisabled = false,
                EndingPlayerTurnPhaseOne = false,
                EndingPlayerTurnPhaseTwo = false,
            },
            new FakeActiveScreenContext { CurrentScreen = new FakeFeedbackScreen() },
        },
        "combat");

    Assert(string.Equals(observation.Screen, "feedback-overlay", StringComparison.OrdinalIgnoreCase), $"Runtime reflection should prefer the explicit feedback overlay over a broad combat probe, got {observation.Screen}.");
    Assert(observation.Meta.TryGetValue("publishedCurrentScreen", out var publishedCurrentScreen)
           && string.Equals(publishedCurrentScreen, "feedback-overlay", StringComparison.OrdinalIgnoreCase), "Runtime reflection should publish feedback-overlay when ActiveScreenContext resolves the feedback screen.");
    Assert(observation.Meta.TryGetValue("rawCurrentActiveScreenType", out var currentActiveScreenType)
           && currentActiveScreenType.Contains("Feedback", StringComparison.OrdinalIgnoreCase), "Runtime reflection should surface the feedback active screen type for diagnostics.");
    Assert(observation.Encounter?.InCombat != true, "Feedback overlay startup should not be treated as in-combat when CombatManager.IsInProgress is false.");
}

static void TestRuntimeReflectionPrefersAbandonRunConfirmPopupOverBroadFeedbackBag()
{
    var popup = new FakeNAbandonRunConfirmPopup
    {
        VerticalPopup = new FakeNVerticalPopup
        {
            YesButton = new FakeNPopupYesNoButton
            {
                IsYes = true,
                Visible = true,
                Enabled = true,
                Position = new FakeVector2(640, 512),
                Size = new FakeVector2(180, 72),
                _label = new FakeLabel { Text = "확인" },
            },
            NoButton = new FakeNPopupYesNoButton
            {
                IsYes = false,
                Visible = true,
                Enabled = true,
                Position = new FakeVector2(840, 512),
                Size = new FakeVector2(180, 72),
                _label = new FakeLabel { Text = "취소" },
            },
        },
    };

    var observation = BuildRuntimeObservationForSelfTest(
        new object[]
        {
            new FakeNMainMenu
            {
                _continueButton = new FakeNMainMenuContinueButton
                {
                    Visible = true,
                    Enabled = true,
                    Position = new FakeVector2(220, 260),
                    Size = new FakeVector2(220, 72),
                    _label = new FakeLabel { Text = "계속" },
                },
                _abandonRunButton = new FakeNMainMenuTextButton
                {
                    Visible = true,
                    Enabled = true,
                    Position = new FakeVector2(220, 540),
                    Size = new FakeVector2(220, 72),
                    _label = new FakeLabel { Text = "전투 포기" },
                },
            },
            new FakeFeedbackScreen(),
            popup,
            new FakeActiveScreenContext { CurrentScreen = popup },
        },
        null);

    Assert(string.Equals(observation.Screen, "main-menu", StringComparison.OrdinalIgnoreCase), $"Runtime reflection should let AbandonRunConfirmPopup beat the broad feedback bag, got {observation.Screen}.");
    Assert(observation.Meta.TryGetValue("publishedCurrentScreen", out var publishedCurrentScreen)
           && string.Equals(publishedCurrentScreen, "main-menu", StringComparison.OrdinalIgnoreCase), "Runtime reflection should publish main-menu while the abandon-run confirm popup owns the foreground.");
    Assert(observation.Meta.TryGetValue("rawCurrentActiveScreenType", out var currentActiveScreenType)
           && currentActiveScreenType.Contains("AbandonRunConfirmPopup", StringComparison.OrdinalIgnoreCase), "Runtime reflection should surface the abandon-run popup active screen type for diagnostics.");
    Assert(observation.Meta.TryGetValue("choiceExtractorPath", out var extractorPath)
           && string.Equals(extractorPath, "main-menu-abandon-confirm", StringComparison.OrdinalIgnoreCase), "Choice extraction should prefer the abandon-run confirm popup over the underlying main-menu buttons.");
    Assert(observation.Choices.Any(choice => string.Equals(choice.Label, "확인", StringComparison.OrdinalIgnoreCase)), "Popup extraction should export the confirm button.");
    Assert(observation.Choices.Any(choice => string.Equals(choice.Label, "취소", StringComparison.OrdinalIgnoreCase)), "Popup extraction should export the cancel button.");
    Assert(!observation.Choices.Any(choice => string.Equals(choice.Label, "계속", StringComparison.OrdinalIgnoreCase)), "Underlying main-menu choices should not win while the abandon-run popup owns the foreground.");
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
        new Dictionary<string, string?>())
    {
        CombatHandCount = 5,
        DrawPileCount = 10,
        DiscardPileCount = 4,
        ExhaustPileCount = 1,
        PlayPileCount = 2,
    };
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
    Assert(second.CombatHandCount == first.CombatHandCount, "Expected partial runtime poll not to clear structured combat hand count.");
    Assert(second.DrawPileCount == first.DrawPileCount, "Expected partial runtime poll not to clear draw pile count.");
    Assert(second.DiscardPileCount == first.DiscardPileCount, "Expected partial runtime poll not to clear discard pile count.");
    Assert(second.ExhaustPileCount == first.ExhaustPileCount, "Expected partial runtime poll not to clear exhaust pile count.");
    Assert(second.PlayPileCount == first.PlayPileCount, "Expected partial runtime poll not to clear play pile count.");
}

static void TestLiveExportTrackerClearsCombatStructuredFieldsOutsideCombat()
{
    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live");
    var combatSeed = new LiveExportObservation(
        "runtime-poll",
        DateTimeOffset.UtcNow.AddSeconds(-1),
        "run-001",
        "active",
        "combat",
        1,
        9,
        LiveExportPlayerSummary.Empty,
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<LiveExportChoiceSummary>(),
        Array.Empty<string>(),
        new LiveExportEncounterSummary("Jaw Worm", "Combat", true, 2),
        new Dictionary<string, object?>(),
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["combatHandSummary"] = "1:Strike|Attack|1;2:Defend|Skill|1;3:Bash|Attack|2",
            ["combatTargetCount"] = "1",
            ["enemyIntentSummary"] = "Jaw Worm attacks for 11",
        })
    {
        CombatHandCount = 3,
        DrawPileCount = 10,
        DiscardPileCount = 4,
        ExhaustPileCount = 1,
        PlayPileCount = 2,
        EnemyIntentSummary = "Jaw Worm attacks for 11",
    };
    var first = tracker.Apply(combatSeed).Snapshot;

    var rewardAftermath = new LiveExportObservation(
        "reward-screen-opened",
        DateTimeOffset.UtcNow,
        "run-001",
        "active",
        "rewards",
        1,
        9,
        LiveExportPlayerSummary.Empty,
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        new[]
        {
            new LiveExportChoiceSummary("choice", "넘기기", null, "넘기기"),
        },
        Array.Empty<string>(),
        new LiveExportEncounterSummary("RewardsScreen", "Reward", false, null),
        new Dictionary<string, object?>(),
        new Dictionary<string, string?>());
    var second = tracker.Apply(rewardAftermath).Snapshot;

    Assert(first.CombatHandCount == 3 && first.DrawPileCount == 10, "Expected seed combat snapshot to hold structured combat values.");
    Assert(second.CombatHandCount is null, "Expected combat hand count to clear outside combat.");
    Assert(second.DrawPileCount is null, "Expected draw pile count to clear outside combat.");
    Assert(second.DiscardPileCount is null, "Expected discard pile count to clear outside combat.");
    Assert(second.ExhaustPileCount is null, "Expected exhaust pile count to clear outside combat.");
    Assert(second.PlayPileCount is null, "Expected play pile count to clear outside combat.");
    Assert(second.EnemyIntentSummary is null, "Expected enemy intent summary to clear outside combat.");
    Assert(second.Meta.TryGetValue("combatHandSummary", out var combatHandSummary) && combatHandSummary is null, "Expected combatHandSummary meta to clear outside combat.");
    Assert(second.Meta.TryGetValue("combatTargetCount", out var combatTargetCount) && combatTargetCount is null, "Expected combatTargetCount meta to clear outside combat.");
    Assert(second.Meta.TryGetValue("enemyIntentSummary", out var enemyIntentMeta) && enemyIntentMeta is null, "Expected enemyIntentSummary meta to clear outside combat.");
}

static void TestLiveExportTrackerPrefersExplicitPublishedSceneOverStickyFallback()
{
    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live");
    var seed = new LiveExportObservation(
        "choice-list-presented",
        DateTimeOffset.UtcNow.AddSeconds(-1),
        "run-001",
        "active",
        "event",
        1,
        1,
        LiveExportPlayerSummary.Empty,
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        new[]
        {
            new LiveExportChoiceSummary("event-option", "황금 진주", "golden-pearl", null),
            new LiveExportChoiceSummary("event-option", "정밀한 가위", "precise-scissors", null),
            new LiveExportChoiceSummary("event-option", "나뭇잎 습포", "leaf-poultice", null),
        },
        Array.Empty<string>(),
        new LiveExportEncounterSummary("Neow", "Event", false, null),
        new Dictionary<string, object?>(),
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["publishedCurrentScreen"] = "event",
            ["publishedVisibleScreen"] = "event",
        });
    var first = tracker.Apply(seed).Snapshot;
    Assert(first.CurrentScreen == "event", "Expected seed event observation to establish event screen.");

    var mapPoll = new LiveExportObservation(
        "runtime-poll",
        DateTimeOffset.UtcNow,
        "run-001",
        "active",
        "unknown",
        1,
        1,
        LiveExportPlayerSummary.Empty,
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<LiveExportChoiceSummary>(),
        Array.Empty<string>(),
        new LiveExportEncounterSummary(null, null, false, null),
        new Dictionary<string, object?>(),
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["publishedCurrentScreen"] = "map",
            ["publishedVisibleScreen"] = "map",
        });
    var second = tracker.Apply(mapPoll).Snapshot;

    Assert(second.CurrentScreen == "map", "Expected explicit published map screen to beat sticky event fallback preservation.");
    Assert(second.PublishedCurrentScreen == "map", "Expected publishedCurrentScreen to remain map.");
    Assert(second.CurrentChoices.Count == 0, "Expected explicit published map poll not to preserve stale event choices.");
}

static void TestLiveExportTrackerPrefersForegroundOwnerOverFallbackPublishedScreen()
{
    var tracker = new LiveExportStateTracker(LiveExportStateTrackerOptions.CreateDefault(), @"C:\temp\live");
    var seed = new LiveExportObservation(
        "choice-list-presented",
        DateTimeOffset.UtcNow.AddSeconds(-1),
        "run-001",
        "active",
        "event",
        1,
        1,
        LiveExportPlayerSummary.Empty,
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        new[]
        {
            new LiveExportChoiceSummary("event-option", "황금 진주", "golden-pearl", null),
            new LiveExportChoiceSummary("event-option", "정밀한 가위", "precise-scissors", null),
            new LiveExportChoiceSummary("event-option", "나뭇잎 습포", "leaf-poultice", null),
        },
        Array.Empty<string>(),
        new LiveExportEncounterSummary("Neow", "Event", false, null),
        new Dictionary<string, object?>(),
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["publishedCurrentScreen"] = "event",
            ["publishedVisibleScreen"] = "event",
            ["foregroundOwner"] = "event",
        });
    var first = tracker.Apply(seed).Snapshot;
    Assert(first.CurrentScreen == "event", "Expected seed event observation to establish event screen.");

    var mapPoll = new LiveExportObservation(
        "runtime-poll",
        DateTimeOffset.UtcNow,
        "run-001",
        "active",
        "feedback-overlay",
        1,
        1,
        LiveExportPlayerSummary.Empty,
        Array.Empty<LiveExportCardSummary>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<LiveExportChoiceSummary>(),
        Array.Empty<string>(),
        new LiveExportEncounterSummary(null, null, false, null),
        new Dictionary<string, object?>(),
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["publishedCurrentScreen"] = "feedback-overlay",
            ["publishedVisibleScreen"] = "bootstrap",
            ["foregroundOwner"] = "map",
            ["mapCurrentActiveScreen"] = "true",
        });
    var second = tracker.Apply(mapPoll).Snapshot;

    Assert(second.CurrentScreen == "map", "Expected foreground owner map to beat sticky event preservation when published screen stays fallback.");
    Assert(second.CurrentChoices.Count == 0, "Expected fallback map poll with explicit foreground owner not to preserve stale event choices.");
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

static void TestCompanionStateMapperPrimarySceneWinsCompatibilityVisibleAlias()
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
    Assert(state.Scene.VisibleSceneType == "rewards", $"Expected visible scene to stay on resolved primary rewards scene, got {state.Scene.VisibleSceneType}.");
    Assert(state.Scene.FlowSceneType == "rewards", $"Expected flow rewards scene, got {state.Scene.FlowSceneType}.");
}

static void TestScreenProvenanceResolverCombatPromotion()
{
    var snapshot = CreateScreenProvenanceSnapshot("combat-promotion", "combat") with
    {
        Encounter = new LiveExportEncounterSummary("Jaw Worm", "Combat", true, 2),
        PublishedCurrentScreen = "map",
        PublishedVisibleScreen = "map",
        Meta = MergeMeta(
            CreateScreenProvenanceSnapshot("combat-promotion", "combat").Meta,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["foregroundOwner"] = "map",
                ["combatHandSummary"] = "Strike, Defend",
            }),
    };

    var resolved = ScreenProvenanceResolver.Resolve(ScreenProvenanceResolver.CreateFromLiveSnapshot(snapshot));

    Assert(resolved.ResolvedCurrentScreen == "combat", $"Expected combat current screen after promotion, got {resolved.ResolvedCurrentScreen ?? "<null>"}.");
    Assert(resolved.ResolvedVisibleScreen == "combat", $"Expected combat visible screen after promotion, got {resolved.ResolvedVisibleScreen ?? "<null>"}.");
    Assert(resolved.CombatPromotionApplied, "Expected combat promotion to apply when tracker current screen is combat.");
}

static void TestScreenProvenanceResolverPrefersPublishedScreen()
{
    var input = new ScreenProvenanceInput(
        RawCurrentScreen: "map",
        RawObservedScreen: "event",
        TrackerCurrentScreen: "feedback-overlay",
        PublishedCurrentScreen: "reward",
        PublishedVisibleScreen: "reward",
        CompatibilityCurrentScreen: "map",
        CompatibilityLogicalScreen: "map",
        CompatibilityVisibleScreen: "map",
        PublishedSceneReady: true,
        PublishedSceneAuthority: "polling",
        PublishedSceneStability: "stable",
        CompatibilitySceneReady: false,
        CompatibilitySceneAuthority: "legacy",
        CompatibilitySceneStability: "stable",
        EncounterInCombat: false);

    var resolved = ScreenProvenanceResolver.Resolve(input);

    Assert(resolved.ResolvedCurrentScreen == "reward", $"Expected published reward current screen, got {resolved.ResolvedCurrentScreen ?? "<null>"}.");
    Assert(resolved.ResolvedVisibleScreen == "reward", $"Expected published reward visible screen, got {resolved.ResolvedVisibleScreen ?? "<null>"}.");
    Assert(resolved.ProvenanceSource.Contains("published-current", StringComparison.OrdinalIgnoreCase), $"Expected published provenance source, got {resolved.ProvenanceSource}.");
    Assert(resolved.ResolvedSceneReady == true, "Expected published scene-ready to be preserved.");
}

static void TestScreenProvenanceResolverCompatibilityDoesNotPromotePrimary()
{
    var input = new ScreenProvenanceInput(
        RawCurrentScreen: null,
        RawObservedScreen: null,
        TrackerCurrentScreen: "unknown",
        PublishedCurrentScreen: null,
        PublishedVisibleScreen: null,
        CompatibilityCurrentScreen: null,
        CompatibilityLogicalScreen: "event",
        CompatibilityVisibleScreen: "map",
        PublishedSceneReady: null,
        PublishedSceneAuthority: null,
        PublishedSceneStability: null,
        CompatibilitySceneReady: true,
        CompatibilitySceneAuthority: "legacy",
        CompatibilitySceneStability: "stable",
        EncounterInCombat: false);

    var resolved = ScreenProvenanceResolver.Resolve(input);

    Assert(resolved.ResolvedCurrentScreen is null, $"Expected compatibility-only input not to promote primary current screen, got {resolved.ResolvedCurrentScreen ?? "<null>"}.");
    Assert(resolved.ResolvedVisibleScreen is null, $"Expected compatibility-only input not to promote primary visible screen, got {resolved.ResolvedVisibleScreen ?? "<null>"}.");
    Assert(resolved.ResolvedSceneReady is null, "Expected compatibility-only scene-ready not to promote primary scene readiness.");
    Assert(string.Equals(resolved.CompatibilityLogicalScreen, "event", StringComparison.OrdinalIgnoreCase), $"Expected compatibility logical screen to remain available, got {resolved.CompatibilityLogicalScreen ?? "<null>"}.");
    Assert(string.Equals(resolved.CompatibilityVisibleScreen, "map", StringComparison.OrdinalIgnoreCase), $"Expected compatibility visible screen to remain available, got {resolved.CompatibilityVisibleScreen ?? "<null>"}.");
    Assert(resolved.ProvenanceSource.Contains("none", StringComparison.OrdinalIgnoreCase), $"Expected no primary provenance source for compatibility-only input, got {resolved.ProvenanceSource}.");
}

static void TestScreenProvenanceResolverInventoryFallbackIsolation()
{
    var snapshot = CreateScreenProvenanceSnapshot("inventory-fallback-host", "unknown");
    var inventory = new HarnessNodeInventory(
        "inventory-only",
        DateTimeOffset.UtcNow,
        snapshot.RunId,
        "map",
        "episode-map",
        "dormant",
        null,
        true,
        "published",
        "stable",
        Array.Empty<HarnessNodeInventoryItem>())
    {
        PublishedCurrentScreen = "map",
        PublishedVisibleScreen = "map",
        PublishedSceneReady = true,
        PublishedSceneAuthority = "inventory",
        PublishedSceneStability = "stable",
    };

    var harnessResolved = ScreenProvenanceResolver.Resolve(CreateHarnessProvenanceInput(null, inventory));
    var hostResolved = ScreenProvenanceResolver.Resolve(ScreenProvenanceResolver.CreateFromLiveSnapshot(snapshot));

    Assert(harnessResolved.ResolvedCurrentScreen == "map", $"Expected harness adapter to use inventory fallback current screen, got {harnessResolved.ResolvedCurrentScreen ?? "<null>"}.");
    Assert(harnessResolved.ResolvedVisibleScreen == "map", $"Expected harness adapter to use inventory fallback visible screen, got {harnessResolved.ResolvedVisibleScreen ?? "<null>"}.");
    Assert(hostResolved.ResolvedCurrentScreen is null, $"Expected host adapter not to use inventory fallback current screen, got {hostResolved.ResolvedCurrentScreen ?? "<null>"}.");
    Assert(hostResolved.ResolvedVisibleScreen is null, $"Expected host adapter not to use inventory fallback visible screen, got {hostResolved.ResolvedVisibleScreen ?? "<null>"}.");
}

static void TestScreenProvenanceHarnessHostParity()
{
    foreach (var scenario in CreateScreenProvenanceParityScenarios())
    {
        var harnessResolved = ScreenProvenanceResolver.Resolve(CreateHarnessProvenanceInput(scenario.Snapshot, scenario.Inventory));
        var hostResolved = ScreenProvenanceResolver.Resolve(ScreenProvenanceResolver.CreateFromLiveSnapshot(scenario.Snapshot));

        Assert(string.Equals(harnessResolved.ResolvedCurrentScreen, hostResolved.ResolvedCurrentScreen, StringComparison.OrdinalIgnoreCase), $"Expected harness/host current screen parity for {scenario.Name}.");
        Assert(string.Equals(harnessResolved.ResolvedVisibleScreen, hostResolved.ResolvedVisibleScreen, StringComparison.OrdinalIgnoreCase), $"Expected harness/host visible screen parity for {scenario.Name}.");
        Assert(string.Equals(harnessResolved.ResolvedSceneAuthority, hostResolved.ResolvedSceneAuthority, StringComparison.OrdinalIgnoreCase), $"Expected harness/host scene authority parity for {scenario.Name}.");
        Assert(string.Equals(harnessResolved.ResolvedSceneStability, hostResolved.ResolvedSceneStability, StringComparison.OrdinalIgnoreCase), $"Expected harness/host scene stability parity for {scenario.Name}.");

        Assert(string.Equals(harnessResolved.ResolvedCurrentScreen, scenario.ExpectedResolvedCurrentScreen, StringComparison.OrdinalIgnoreCase), $"Expected resolved current screen {scenario.ExpectedResolvedCurrentScreen} for {scenario.Name}, got {harnessResolved.ResolvedCurrentScreen ?? "<null>"}.");
        Assert(string.Equals(harnessResolved.ResolvedVisibleScreen, scenario.ExpectedResolvedVisibleScreen, StringComparison.OrdinalIgnoreCase), $"Expected resolved visible screen {scenario.ExpectedResolvedVisibleScreen} for {scenario.Name}, got {harnessResolved.ResolvedVisibleScreen ?? "<null>"}.");

        var sceneModel = ResolveHostSceneModelForParityScenario(scenario);
        Assert(string.Equals(sceneModel.SceneType, scenario.ExpectedSceneType, StringComparison.OrdinalIgnoreCase), $"Expected host sceneType {scenario.ExpectedSceneType} for {scenario.Name}, got {sceneModel.SceneType}.");
        Assert(string.Equals(sceneModel.CanonicalOwner, scenario.ExpectedCanonicalOwner, StringComparison.OrdinalIgnoreCase), $"Expected host canonicalOwner {scenario.ExpectedCanonicalOwner} for {scenario.Name}, got {sceneModel.CanonicalOwner}.");
    }
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

static void TestAdvisorDisplaySanitizer()
{
    var sanitized = AdvisorDisplaySanitizer.SanitizeText("[color=gold]Gain[/color] {gold} 99 <color=#ff0>now</color> :: reward");
    Assert(string.Equals(sanitized, "Gain 99 now reward", StringComparison.Ordinal), $"Expected sanitizer to strip raw markers, got '{sanitized ?? "<null>"}'.");

    var namedColorSanitized = AdvisorDisplaySanitizer.SanitizeText("[gold]희귀 카드[/gold] [blue]1[/blue]장 [red]감소[/red]");
    Assert(string.Equals(namedColorSanitized, "희귀 카드 1장 감소", StringComparison.Ordinal), $"Expected sanitizer to strip named color tags, got '{namedColorSanitized ?? "<null>"}'.");

    var dynamicSanitized = AdvisorDisplaySanitizer.SanitizeText("[b]{CalculatedDamage:diff()}[/b] 피해를 주고 [img]res://x.png[/img] {IfUpgraded:강화 시 }카드를 1장 뽑습니다.");
    Assert(string.Equals(dynamicSanitized, "피해를 주고 카드를 1장 뽑습니다.", StringComparison.Ordinal), $"Expected sanitizer to strip dynamic placeholders and formatting noise, got '{dynamicSanitized ?? "<null>"}'.");

    var prettified = AdvisorDisplaySanitizer.PrettifyIdentifier("CARD.STRIKE_IRONCLAD");
    Assert(string.Equals(prettified, "Strike Ironclad", StringComparison.Ordinal), $"Expected prettified identifier, got '{prettified ?? "<null>"}'.");
}

static void TestAdvisorKnowledgeDisplayResolver()
{
    var resolver = CreateDisplayKnowledgeResolver();
    var option = new AdvisorSceneOption(
        "Pommel Strike",
        "reward-pick-card",
        "pommel-strike",
        null,
        true,
        new[] { "reward-card", "scene:reward" });

    var resolved = resolver.ResolveSceneOption(option);
    Assert(string.Equals(resolved.Title, "몸통 박치기", StringComparison.Ordinal), $"Expected localized card title, got '{resolved.Title}'.");
    Assert(string.Equals(resolved.Description, "적에게 피해를 주고 카드를 1장 뽑습니다.", StringComparison.Ordinal), $"Expected localized card description, got '{resolved.Description ?? "<null>"}'.");
    Assert(resolved.UsedKnowledge, "Expected scene option resolution to report knowledge usage.");

    var fallbackDescription = resolver.ResolveDescription(null, "Pommel Strike", "pommel-strike");
    Assert(string.Equals(fallbackDescription, "적에게 피해를 주고 카드를 1장 뽑습니다.", StringComparison.Ordinal), $"Expected description fallback from knowledge, got '{fallbackDescription ?? "<null>"}'.");
}

static void TestAdvisorSceneDisplayFormatter()
{
    var resolver = CreateDisplayKnowledgeResolver();
    var player = new AdvisorScenePlayerContext(70, 80, 3, 120, 12, Array.Empty<string>(), Array.Empty<string>());
    var combatScene = new AdvisorSceneArtifact(
        AdvisorSceneSchema.Version,
        "live",
        "display-combat-run",
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow,
        null,
        null,
        null,
        "combat",
        "player-play-open",
        "combat",
        "raw combat summary",
        player,
        Array.Empty<AdvisorSceneUiSurface>(),
        new[]
        {
            new AdvisorSceneOption("Strike", "combat-hand-card", "CARD.STRIKE_IRONCLAD", "Deal 6 damage.", true, new[] { "category:hand-card" }),
            new AdvisorSceneOption("DrawPile", "choice", "draw-pile", null, true, new[] { "category:pile-button" }),
            new AdvisorSceneOption("End Turn", "choice", "end-turn", null, true, new[] { "category:combat-action" }),
            new AdvisorSceneOption("Ping", "utility", "ping", "Diagnostic ping.", true, new[] { "category:utility" }),
        },
        Array.Empty<string>(),
        Array.Empty<string>(),
        new Dictionary<string, double>(),
        Array.Empty<string>())
    {
        Combat = new AdvisorSceneCombatDetails(
            true,
            "jaw-worm",
            "player-play-open",
            2,
            3,
            "1:CARD.STRIKE_IRONCLAD|Attack|1;2:CARD.DEFEND_IRONCLAD|Skill|1;3:pommel-strike|Attack|1",
            10,
            4,
            1,
            2,
            1,
            1,
            false,
            "Jaw Worm attacks for 11"),
    };
    var combatContext = AdvisorSceneDisplayFormatter.FormatContext(combatScene, resolver);
    Assert(!combatContext.Contains("1:CARD.STRIKE_IRONCLAD", StringComparison.Ordinal), $"Expected combat context to humanize the raw hand summary, got '{combatContext}'.");
    Assert(!combatContext.Contains("|Attack|1", StringComparison.Ordinal), $"Expected combat context to strip machine summary separators, got '{combatContext}'.");
    Assert(combatContext.Contains("비용 1", StringComparison.Ordinal), $"Expected combat context to keep human-readable card metadata, got '{combatContext}'.");
    Assert(combatContext.Contains("명시적 피격 적: 1", StringComparison.Ordinal), $"Expected combat context to label HittableEnemyCount correctly, got '{combatContext}'.");

    var combatSummary = AdvisorSceneDisplayFormatter.FormatSummary(combatScene, resolver);
    Assert(!combatSummary.Contains("1:CARD.STRIKE_IRONCLAD", StringComparison.Ordinal), $"Expected combat summary to avoid raw hand summary payloads, got '{combatSummary}'.");
    Assert(combatSummary.Contains("핵심 손패", StringComparison.Ordinal), $"Expected combat summary to keep the humanized hand summary section, got '{combatSummary}'.");

    var combatOptions = AdvisorSceneDisplayFormatter.FormatOptions(combatScene, resolver);
    Assert(combatOptions.Contains("손패 카드", StringComparison.Ordinal), "Expected combat formatter to render the hand-card section.");
    Assert(combatOptions.Contains("더미 버튼", StringComparison.Ordinal), "Expected combat formatter to render the pile-button section.");
    Assert(combatOptions.Contains("전투 행동", StringComparison.Ordinal), "Expected combat formatter to render the combat-action section.");
    Assert(!combatOptions.Contains("Ping", StringComparison.OrdinalIgnoreCase), "Expected combat formatter to hide utility Ping from the default option panel.");

    var rewardScene = new AdvisorSceneArtifact(
        AdvisorSceneSchema.Version,
        "live",
        "display-reward-run",
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow,
        null,
        null,
        null,
        "reward",
        "claim",
        "reward",
        "reward summary",
        player,
        Array.Empty<AdvisorSceneUiSurface>(),
        new[]
        {
            new AdvisorSceneOption("보상 {gold}", "gold", "reward-gold", "[color=#ff0]{gold} 100[/color]", true, new[] { "reward-gold" }),
            new AdvisorSceneOption("Pommel Strike", "reward-pick-card", "pommel-strike", null, true, new[] { "reward-card" }),
        },
        Array.Empty<string>(),
        Array.Empty<string>(),
        new Dictionary<string, double>(),
        Array.Empty<string>())
    {
        Reward = new AdvisorSceneRewardDetails("claim", "claim", true, false, true, false, 2, true),
    };
    var rewardOptions = AdvisorSceneDisplayFormatter.FormatOptions(rewardScene, resolver);
    Assert(!rewardOptions.Contains("{gold}", StringComparison.Ordinal), $"Expected reward formatter to sanitize raw gold markers, got '{rewardOptions}'.");
    Assert(rewardOptions.Contains("몸통 박치기", StringComparison.Ordinal), "Expected reward formatter to localize the card label.");
    Assert(rewardOptions.Contains("적에게 피해를 주고 카드를 1장 뽑습니다.", StringComparison.Ordinal), "Expected reward formatter to fill the missing card description from knowledge.");

    var eventScene = new AdvisorSceneArtifact(
        AdvisorSceneSchema.Version,
        "live",
        "display-event-run",
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow,
        null,
        null,
        null,
        "event",
        "event-choice",
        "event",
        "event summary",
        player,
        Array.Empty<AdvisorSceneUiSurface>(),
        new[]
        {
            new AdvisorSceneOption("새", "event-option", "peck", "시작 카드를 1장 선택해 쪼기로 변화시킵니다.", true, new[] { "scene:event" }),
            new AdvisorSceneOption("뱀", "event-option", "slither", "카드 1장에 미끈거림을 인챈트합니다.", true, new[] { "scene:event" }),
            new AdvisorSceneOption("고리", "event-option", "toric-toughness", "시작 카드를 1장 선택해 고리형 강인함으로 변화시킵니다.", true, new[] { "scene:event" }),
        },
        Array.Empty<string>(),
        Array.Empty<string>(),
        new Dictionary<string, double>(),
        Array.Empty<string>())
    {
        Event = new AdvisorSceneEventDetails(
            "event-choice",
            "event-choice",
            false,
            false,
            true,
            "event-option",
            "WoodCarvings",
            3),
    };
    var eventOptions = AdvisorSceneDisplayFormatter.FormatOptions(eventScene, resolver);
    Assert(eventOptions.Contains("- 새 [활성] :: 시작 카드를 1장 선택해 쪼기로 변화시킵니다.", StringComparison.Ordinal), $"Expected event formatter to preserve the original 새 label, got '{eventOptions}'.");
    Assert(eventOptions.Contains("- 뱀 [활성] :: 카드 1장에 미끈거림을 인챈트합니다.", StringComparison.Ordinal), $"Expected event formatter to preserve the original 뱀 label, got '{eventOptions}'.");
    Assert(eventOptions.Contains("- 고리 [활성] :: 시작 카드를 1장 선택해 고리형 강인함으로 변화시킵니다.", StringComparison.Ordinal), $"Expected event formatter to preserve the original 고리 label, got '{eventOptions}'.");
    Assert(!eventOptions.Contains("- 쪼기 [활성]", StringComparison.Ordinal), $"Expected event formatter not to relabel 새 as 쪼기, got '{eventOptions}'.");
    Assert(!eventOptions.Contains("- 고리형 강인함 [활성]", StringComparison.Ordinal), $"Expected event formatter not to relabel 고리 as 고리형 강인함, got '{eventOptions}'.");

    var compactEventOptions = EventCompactOptionDisplayFormatter.Format(
        new Sts2AiCompanion.Foundation.Contracts.RewardEventCompactAdvisorInput(
            "event",
            "event-choice",
            "event",
            new Sts2AiCompanion.Foundation.Contracts.CompactRunContext(1, 1, 70, 80, 120, 0, 0, 10),
            new Sts2AiCompanion.Foundation.Contracts.CompactPlayerSummary("start deck", Array.Empty<string>(), Array.Empty<string>()),
            new[]
            {
                new Sts2AiCompanion.Foundation.Contracts.CompactAdvisorOption(
                    "event-option",
                    "고리",
                    "toric-toughness",
                    "시작 카드를 1장 선택해 고리형 강인함으로 변화시킵니다.",
                    true,
                    Array.Empty<string>()),
            },
            Array.Empty<Sts2AiCompanion.Foundation.Contracts.CompactKnowledgeEntry>(),
            Array.Empty<Sts2AiCompanion.Foundation.Contracts.CompactRecentEvent>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            EventFacts: new Sts2AiCompanion.Foundation.Contracts.EventCompactFacts(
                "WoodCarvings",
                false,
                false,
                false,
                new[]
                {
                    new Sts2AiCompanion.Foundation.Contracts.EventCompactOptionFact(
                        "고리",
                        "toric-toughness",
                        true,
                        new[]
                        {
                            new Sts2AiCompanion.Foundation.Contracts.EventCompactEffect("transform_target_card", 1, "고리형 강인함"),
                            new Sts2AiCompanion.Foundation.Contracts.EventCompactEffect("result_card_effect", 1, "고리형 강인함은 비용 2 스킬 카드로, 방어도 5를 얻고 다음 2턴 동안 턴 시작 시 방어도 5를 얻습니다."),
                        },
                        Array.Empty<string>()),
                },
                Array.Empty<string>())));
    Assert(compactEventOptions.Contains("- 고리 [활성] ::", StringComparison.Ordinal), $"Expected compact event formatter to keep the original 고리 label, got '{compactEventOptions}'.");
    Assert(compactEventOptions.Contains("고리형 강인함", StringComparison.Ordinal), $"Expected compact event formatter to include the result card name in the description, got '{compactEventOptions}'.");
    Assert(!compactEventOptions.Contains("- 고리형 강인함 [활성]", StringComparison.Ordinal), $"Expected compact event formatter not to replace the label with the result card name, got '{compactEventOptions}'.");
}

static void TestDeckDisplayFormatter()
{
    var resolver = CreateDisplayKnowledgeResolver();
    var deckText = DeckDisplayFormatter.FormatDeck(
        new[]
        {
            new LiveExportCardSummary("Pommel Strike", "pommel-strike", 1, "Attack", false),
            new LiveExportCardSummary("Pommel Strike", "pommel-strike", 1, "Attack", true),
            new LiveExportCardSummary("Defend", "CARD.DEFEND_IRONCLAD", 1, "Skill", false),
        },
        resolver);

    Assert(deckText.Contains("총 카드 수: 3", StringComparison.Ordinal), "Expected deck formatter to include the total card count.");
    Assert(deckText.Contains("- 몸통 박치기 x 2", StringComparison.Ordinal), $"Expected deck formatter to aggregate duplicate localized card names, got '{deckText}'.");
    Assert(deckText.Contains("- 수비 x 1", StringComparison.Ordinal), $"Expected raw card ids to resolve to localized titles, got '{deckText}'.");
    Assert(!deckText.Contains("CARD.DEFEND_IRONCLAD", StringComparison.Ordinal), $"Expected deck formatter to avoid raw ids, got '{deckText}'.");
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

static void TestRewardCompactAdvisorInputBuilder()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateRewardTestConfiguration(root);
        SeedRewardKnowledgeCatalog(root);
        var scenario = CreateCuratedRewardScenarios().Single(candidate => candidate.Name == "draw-gap-with-unknown-alternative");
        var runState = CreateRewardRunStateForScenario(scenario, $"{scenario.RunId}-compact");
        var knowledgeService = new FoundationKnowledgeCatalogService(configuration, root);
        var slice = knowledgeService.BuildSlice(ToFoundationRunState(runState), 16, 8192);
        var compactBuilder = new Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.RewardEventCompactAdvisorInputBuilder();
        var compactResult = compactBuilder.Build(ToFoundationRunState(runState), slice);

        Assert(compactResult.Supported, "Expected reward compact input to stay supported when visible options are stable.");
        Assert(compactResult.CompactInput is not null, "Expected reward compact input.");
        var compact = compactResult.CompactInput!;
        Assert(string.Equals(compact.SceneType, "reward", StringComparison.Ordinal), $"Expected compact reward scene type, got {compact.SceneType}.");
        Assert(compact.VisibleOptions.Select(option => option.Label).SequenceEqual(scenario.Choices.Select(choice => choice.Label), StringComparer.Ordinal), "Expected compact reward visible options to preserve exact current labels.");
        Assert(compact.RewardFacts is not null, "Expected reward facts.");
        Assert(compact.MissingInformation.Contains("reward-option-knowledge-missing:미확인 카드", StringComparer.Ordinal), "Expected reward compact input to preserve deterministic missing-information flags.");
        Assert(compact.DecisionBlockers.Count == 0, "Expected reward compact input to stay supported without synthetic blockers.");
        Assert(compact.KnowledgeEntries.Count <= 6, "Expected compact knowledge slice to stay bounded.");

        var duplicateScenario = new RewardScenarioDefinition(
            "reward-duplicate-labels",
            "reward-duplicate-labels-run",
            scenario.Deck,
            new[]
            {
                new LiveExportChoiceSummary("card", "같은 카드", "same-card-0", "카드를 얻습니다."),
                new LiveExportChoiceSummary("card", "같은 카드", "same-card-1", "카드를 얻습니다."),
                new LiveExportChoiceSummary("skip", "넘기기", "skip", "보상을 건너뜁니다."),
            },
            "같은 카드");
        var duplicateRunState = CreateRewardRunStateForScenario(duplicateScenario, $"{duplicateScenario.RunId}-compact");
        var duplicateSlice = knowledgeService.BuildSlice(ToFoundationRunState(duplicateRunState), 16, 8192);
        var duplicateResult = compactBuilder.Build(ToFoundationRunState(duplicateRunState), duplicateSlice);
        Assert(!duplicateResult.Supported, "Expected duplicate reward option labels to fail closed.");
        Assert(duplicateResult.DecisionBlockers.Contains("reward-duplicate-option-label:같은 카드", StringComparer.Ordinal), "Expected duplicate reward label blocker.");
        Assert(duplicateResult.DecisionBlockers.Contains("reward-compact-input-insufficient", StringComparer.Ordinal), "Expected duplicate reward labels to mark compact input insufficient.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestEventCompactAdvisorInputBuilder()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateRewardTestConfiguration(root);
        SeedKnowledgeCatalog(root);
        var runState = CreateEventRunState("event-compact-run");
        var knowledgeService = new FoundationKnowledgeCatalogService(configuration, root);
        var slice = knowledgeService.BuildSlice(ToFoundationRunState(runState), 16, 8192);
        var compactBuilder = new Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.RewardEventCompactAdvisorInputBuilder();

        var compactResult = compactBuilder.Build(ToFoundationRunState(runState), slice);
        Assert(compactResult.Supported, "Expected event compact input to stay supported when explicit option facts exist.");
        Assert(compactResult.CompactInput is not null, "Expected event compact input.");
        var compact = compactResult.CompactInput!;
        Assert(compact.EventFacts is not null, "Expected event compact facts.");
        Assert(compact.EventFacts.EventIdentityMissing, "Expected current event fixture to surface identity missing.");
        Assert(compact.MissingInformation.Contains("event-identity-missing", StringComparer.Ordinal), "Expected event identity gap to surface.");
        Assert(compact.EventFacts.OptionFacts.Any(fact => fact.Label == "해독한다" && fact.Effects.Any(effect => effect.Kind == "hp_loss")), "Expected explicit hp_loss fact for the first event option.");
        Assert(compact.EventFacts.OptionFacts.Any(fact => fact.Label == "부순다" && fact.Effects.Any(effect => effect.Kind == "hp_gain")), "Expected explicit hp_gain fact for the second event option.");

        var neowRunState = CreateNeowEventRunState("event-neow-compact-run");
        var neowSlice = knowledgeService.BuildSlice(ToFoundationRunState(neowRunState), 16, 8192);
        var neowResult = compactBuilder.Build(ToFoundationRunState(neowRunState), neowSlice);
        Assert(neowResult.Supported, "Expected Neow-like ancient event compact input to stay supported once option effects are explicit.");
        Assert(neowResult.CompactInput?.EventFacts is not null, "Expected Neow-like compact event facts.");
        var neowFacts = neowResult.CompactInput!.EventFacts!;
        Assert(string.Equals(neowFacts.EventId, "Neow", StringComparison.Ordinal), $"Expected Neow identity inference, got {neowFacts.EventId ?? "<null>"}.");
        Assert(!neowFacts.EventIdentityMissing, "Expected Neow-like ancient event to resolve identity.");
        Assert(neowFacts.OptionFacts.Any(fact => fact.Label == "니오우의 비탄" && fact.Effects.Any(effect => effect.Kind == "result_relic" && effect.Text.Contains("니오우의 비탄", StringComparison.Ordinal))), "Expected Neow's Torment option to expose typed result relic detail.");
        Assert(neowFacts.OptionFacts.Any(fact => fact.Label == "납 문진" && fact.Effects.Any(effect => effect.Kind == "card_gain" && effect.Amount == 1)), "Expected Lead Paperweight option to expose card gain.");
        Assert(neowFacts.OptionFacts.Any(fact => fact.Label == "니오우의 비탄" && fact.Effects.Any(effect => effect.Kind == "card_gain" && effect.Amount == 1)), "Expected Neow's Torment option to expose card gain.");
        Assert(neowFacts.OptionFacts.Any(fact => fact.Label == "니오우의 비탄" && fact.Effects.Any(effect => effect.Kind == "added_card_effect" && effect.Text.Contains("피해 10", StringComparison.Ordinal))), "Expected Neow's Torment option to expose Neow's Fury card effect.");
        Assert(neowFacts.OptionFacts.Any(fact => fact.Label == "은 도가니" && fact.Effects.Any(effect => effect.Kind == "card_reward_upgrade" && effect.Amount == 3)), "Expected Silver Crucible option to expose first 3 upgraded rewards.");
        Assert(neowFacts.OptionFacts.Any(fact => fact.Label == "은 도가니" && fact.Effects.Any(effect => effect.Kind == "treasure_chest_empty")), "Expected Silver Crucible downside to surface.");

        var promptBuilder = new Sts2AiCompanion.Foundation.Reasoning.AdvicePromptBuilder(configuration);
        var neowPromptPack = promptBuilder.BuildInputPack(ToFoundationRunState(neowRunState), ToFoundationTrigger(new AdviceTrigger("manual", DateTimeOffset.UtcNow, true, true, "manual", null)), neowSlice, neowResult.CompactInput);
        var neowPrompt = promptBuilder.FormatPrompt(neowPromptPack);
        Assert(neowPrompt.Contains("event_id: Neow", StringComparison.Ordinal), "Expected prompt to preserve Neow event identity.");
        Assert(neowPrompt.Contains("opening_event: true", StringComparison.Ordinal), "Expected prompt to mark Neow as an opening event.");
        Assert(neowPrompt.Contains("added_card_effect", StringComparison.Ordinal), "Expected prompt to expose added-card effect details for Neow's Torment.");
        Assert(!neowPrompt.Contains("- act: ?", StringComparison.Ordinal), "Expected prompt to omit unresolved act values instead of emitting '?'.");
        Assert(!neowPrompt.Contains("- floor: ?", StringComparison.Ordinal), "Expected prompt to omit unresolved floor values instead of emitting '?'.");
        Assert(!neowPrompt.Contains("[blue]", StringComparison.OrdinalIgnoreCase), "Expected prompt text to strip BBCode color tags.");
        Assert(!neowPrompt.Contains("[gold]", StringComparison.OrdinalIgnoreCase), "Expected prompt text to strip color markers.");
        Assert(!neowPrompt.Contains("{Cards}", StringComparison.OrdinalIgnoreCase), "Expected prompt text to strip dynamic placeholders.");

        var woodCarvingsRunState = CreateWoodCarvingsEventRunState("event-wood-carvings-compact-run");
        var woodCarvingsSlice = knowledgeService.BuildSlice(ToFoundationRunState(woodCarvingsRunState), 16, 8192);
        var woodCarvingsResult = compactBuilder.Build(ToFoundationRunState(woodCarvingsRunState), woodCarvingsSlice);
        Assert(woodCarvingsResult.Supported, "Expected Wood Carvings event compact input to stay supported after canonical dedupe.");
        Assert(woodCarvingsResult.CompactInput?.EventFacts is not null, "Expected Wood Carvings compact event facts.");
        var woodCarvingsFacts = woodCarvingsResult.CompactInput!.EventFacts!;
        Assert(string.Equals(woodCarvingsFacts.EventId, "WoodCarvings", StringComparison.Ordinal), $"Expected Wood Carvings identity inference, got {woodCarvingsFacts.EventId ?? "<null>"}.");
        Assert(!woodCarvingsFacts.EventIdentityMissing, "Expected Wood Carvings identity to resolve from binding ids.");
        Assert(woodCarvingsResult.CompactInput!.VisibleOptions.Count == 3, $"Expected canonicalized Wood Carvings visible options count of 3, got {woodCarvingsResult.CompactInput.VisibleOptions.Count}.");
        Assert(!woodCarvingsResult.DecisionBlockers.Any(blocker => blocker.StartsWith("event-duplicate-option-label:", StringComparison.OrdinalIgnoreCase)), "Expected canonicalized Wood Carvings options to avoid duplicate-label blockers.");
        Assert(!woodCarvingsResult.DecisionBlockers.Any(blocker => blocker.Contains("target", StringComparison.OrdinalIgnoreCase)), "Expected target_filter facts to remain advisory rather than escalating into decision blockers.");
        Assert(woodCarvingsFacts.OptionFacts.Any(fact => fact.Label == "새" && fact.Effects.Any(effect => effect.Kind == "result_card" && effect.Text.Contains("쪼기", StringComparison.Ordinal))), "Expected bird option to expose typed result card detail.");
        Assert(woodCarvingsFacts.OptionFacts.Any(fact => fact.Label == "새" && fact.Effects.Any(effect => effect.Kind == "transform_target_card" && effect.Text.Contains("쪼기", StringComparison.Ordinal))), "Expected bird option to expose Peck transform target.");
        Assert(woodCarvingsFacts.OptionFacts.Any(fact => fact.Label == "새" && fact.Effects.Any(effect => effect.Kind == "result_card_effect" && effect.Text.Contains("피해 2", StringComparison.Ordinal))), "Expected bird option to expose Peck card effect details.");
        Assert(woodCarvingsFacts.OptionFacts.Any(fact => fact.Label == "뱀" && fact.Effects.Any(effect => effect.Kind == "card_enchant" && effect.Text.Contains("미끈거림", StringComparison.Ordinal))), "Expected snake option to expose Slither enchantment.");
        Assert(woodCarvingsFacts.OptionFacts.Any(fact => fact.Label == "뱀" && fact.Effects.Any(effect => effect.Kind == "result_enchantment" && effect.Text.Contains("미끈거림", StringComparison.Ordinal))), "Expected snake option to expose typed enchantment detail.");
        Assert(woodCarvingsFacts.OptionFacts.Any(fact => fact.Label == "뱀" && fact.Effects.Any(effect => effect.Kind == "result_enchantment_effect" && effect.Text.Contains("무작위 0~3", StringComparison.Ordinal))), "Expected snake option to expose Slither effect details.");
        Assert(woodCarvingsFacts.OptionFacts.Any(fact => fact.Label == "고리" && fact.Effects.Any(effect => effect.Kind == "transform_target_card" && effect.Text.Contains("고리형 강인함", StringComparison.Ordinal))), "Expected torus option to expose Toric Toughness transform target.");
        Assert(woodCarvingsFacts.OptionFacts.Any(fact => fact.Label == "고리" && fact.Effects.Any(effect => effect.Kind == "result_card_effect" && effect.Text.Contains("방어도 5", StringComparison.Ordinal))), "Expected torus option to expose Toric Toughness card effect details.");
        Assert(!woodCarvingsResult.MissingInformation.Contains("event-option-effects-missing:새", StringComparer.Ordinal), "Expected Wood Carvings bird option to avoid missing-effects gaps.");

        var woodPromptPack = promptBuilder.BuildInputPack(ToFoundationRunState(woodCarvingsRunState), ToFoundationTrigger(new AdviceTrigger("manual", DateTimeOffset.UtcNow, true, true, "manual", null)), woodCarvingsSlice, woodCarvingsResult.CompactInput);
        var woodPrompt = promptBuilder.FormatPrompt(woodPromptPack);
        Assert(woodPrompt.Contains("result_card_effect", StringComparison.Ordinal), "Expected Wood Carvings prompt to include result-card effect lines.");
        Assert(woodPrompt.Contains("result_enchantment_effect", StringComparison.Ordinal), "Expected Wood Carvings prompt to include enchantment effect lines.");
        Assert(!woodPrompt.Contains("성능 정보가 compact input에 없다", StringComparison.Ordinal), "Expected generic event facts to eliminate legacy missing-performance blocker text.");

        var duplicateRunState = CreateEventRunState(
            "event-duplicate-compact-run",
            new[]
            {
                new LiveExportChoiceSummary("event-option", "같은 선택지", "option-0", "최대 체력을 5 잃습니다."),
                new LiveExportChoiceSummary("event-option", "같은 선택지", "option-1", "50 골드를 얻습니다."),
            });
        var duplicateSlice = knowledgeService.BuildSlice(ToFoundationRunState(duplicateRunState), 16, 8192);
        var duplicateResult = compactBuilder.Build(ToFoundationRunState(duplicateRunState), duplicateSlice);
        Assert(!duplicateResult.Supported, "Expected duplicate event option labels to fail closed even when explicit facts exist.");
        Assert(duplicateResult.DecisionBlockers.Contains("event-duplicate-option-label:같은 선택지", StringComparer.Ordinal), "Expected duplicate event label blocker.");
        Assert(duplicateResult.DecisionBlockers.Contains("event-compact-input-insufficient", StringComparer.Ordinal), "Expected duplicate event labels to mark compact input insufficient.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestEventCompactFallbackOrdering()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateRewardTestConfiguration(root);
        SeedKnowledgeCatalog(root);
        var knowledgeService = new FoundationKnowledgeCatalogService(configuration, root);
        var compactBuilder = new Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.RewardEventCompactAdvisorInputBuilder();

        var typedDetailRunState = CreateEventRunState(
            "event-fallback-order-typed-detail",
            new[]
            {
                new LiveExportChoiceSummary("event-option", "새", "bird", "시작 카드를 1장 선택해 로 변화시킵니다.")
                {
                    BindingKind = "event-option",
                    BindingId = "WOOD_CARVINGS.pages.INITIAL.options.BIRD",
                    Enabled = true,
                    SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option-button", "option-role:choice" },
                    EventOptionDetail = new LiveExportEventOptionDetail(
                        "WOOD_CARVINGS.pages.INITIAL.options.BIRD",
                        "WOOD_CARVINGS.pages.INITIAL.options.BIRD",
                        "새",
                        null,
                        null,
                        null,
                        null,
                        new LiveExportModelSummary("card", "Peck", "쪼기", null),
                        null,
                        null,
                        null,
                        null,
                        null),
                },
            });
        var typedDetailSlice = knowledgeService.BuildSlice(ToFoundationRunState(typedDetailRunState), 16, 8192);
        var typedDetailResult = compactBuilder.Build(ToFoundationRunState(typedDetailRunState), typedDetailSlice);
        Assert(typedDetailResult.Supported, "Expected typed-detail event option to stay supported.");
        Assert(typedDetailResult.CompactInput?.EventFacts?.OptionFacts.Any(fact =>
            fact.Label == "새"
            && fact.Effects.Any(effect => effect.Kind == "result_card_effect" && effect.Text.Contains("피해 2", StringComparison.Ordinal))) == true,
            "Expected strict model fallback to fill result-card effect from typed detail.");
        Assert(!typedDetailResult.MissingInformation.Contains("event-option-effects-missing:새", StringComparer.Ordinal), "Expected typed-detail fallback ordering to avoid missing-effects gaps.");

        var hoverTipOnlyRunState = CreateEventRunState(
            "event-fallback-order-hover-tip-only",
            new[]
            {
                new LiveExportChoiceSummary("event-option", "툴팁 확인", "hover-tip", "툴팁 효과를 확인합니다.")
                {
                    BindingKind = "event-option",
                    BindingId = "UNKNOWN_EVENT.pages.INITIAL.options.HOVER",
                    Enabled = true,
                    SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option-button", "option-role:choice" },
                    EventOptionDetail = new LiveExportEventOptionDetail(
                        "UNKNOWN_EVENT.pages.INITIAL.options.HOVER",
                        "UNKNOWN_EVENT.pages.INITIAL.options.HOVER",
                        "툴팁 확인",
                        null,
                        "미끈거림",
                        "미끈거림은 해당 카드를 뽑았을 때 이번 전투 동안 비용을 무작위 0~3으로 바꿉니다.",
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null),
                },
            });
        var hoverTipOnlySlice = knowledgeService.BuildSlice(ToFoundationRunState(hoverTipOnlyRunState), 16, 8192);
        var hoverTipOnlyResult = compactBuilder.Build(ToFoundationRunState(hoverTipOnlyRunState), hoverTipOnlySlice);
        Assert(hoverTipOnlyResult.Supported, "Expected hover-tip-only event option to stay supported with generic facts.");
        var hoverTipOnlyFact = hoverTipOnlyResult.CompactInput?.EventFacts?.OptionFacts.SingleOrDefault(fact => fact.Label == "툴팁 확인");
        Assert(hoverTipOnlyFact is not null, "Expected hover-tip-only option fact.");
        Assert(hoverTipOnlyFact!.Effects.Any(effect => effect.Kind == "hover_tip" && effect.Text.Contains("미끈거림", StringComparison.Ordinal)), "Expected hover-tip-only path to preserve generic hover tip title.");
        Assert(hoverTipOnlyFact.Effects.Any(effect => effect.Kind == "hover_tip_effect" && effect.Text.Contains("무작위 0~3", StringComparison.Ordinal)), "Expected hover-tip-only path to preserve generic hover tip effect.");
        Assert(!hoverTipOnlyFact.Effects.Any(effect => effect.Kind == "result_enchantment"), "Expected hover-tip-only path to avoid heuristic enchantment promotion without a typed hover-tip id.");

        var missingRunState = CreateEventRunState(
            "event-fallback-order-missing",
            new[]
            {
                new LiveExportChoiceSummary("event-option", "알 수 없는 선택지", "unknown", "{MysteryPlaceholder}")
                {
                    BindingKind = "event-option",
                    BindingId = "UNKNOWN_EVENT.pages.INITIAL.options.MYSTERY",
                    Enabled = true,
                    SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option-button", "option-role:choice" },
                },
            });
        var missingSlice = knowledgeService.BuildSlice(ToFoundationRunState(missingRunState), 16, 8192);
        var missingResult = compactBuilder.Build(ToFoundationRunState(missingRunState), missingSlice);
        Assert(!missingResult.Supported, "Expected unknown placeholder-only event option to fail closed.");
        Assert(missingResult.MissingInformation.Contains("event-option-effects-missing:알 수 없는 선택지", StringComparer.Ordinal), "Expected missing-information fallback when typed detail and display text are unavailable.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestEventCompactKnowledgeFiltering()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateRewardTestConfiguration(root);
        SeedEventKnowledgeNoiseCatalog(root);
        var knowledgeService = new FoundationKnowledgeCatalogService(configuration, root);
        var compactBuilder = new Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.RewardEventCompactAdvisorInputBuilder();
        var promptBuilder = new FoundationAdvicePromptBuilder(configuration);
        var runState = CreateWoodCarvingsEventRunState("event-knowledge-strict-run");
        var slice = knowledgeService.BuildSlice(ToFoundationRunState(runState), 16, 8192);

        var compactResult = compactBuilder.Build(ToFoundationRunState(runState), slice);
        Assert(compactResult.Supported, "Expected Wood Carvings event compact input to stay supported when strict event-local knowledge filtering is applied.");
        Assert(compactResult.CompactInput is not null, "Expected compact event input.");
        var compact = compactResult.CompactInput!;
        Assert(compact.KnowledgeEntries.Count == 1, $"Expected only the exact Wood Carvings event entry to survive strict filtering, got {compact.KnowledgeEntries.Count} entries.");
        Assert(compact.KnowledgeEntries.Any(entry => entry.Id.Contains("woodcarvings", StringComparison.OrdinalIgnoreCase)), "Expected strict event-local knowledge to keep the Wood Carvings entry.");
        Assert(!compact.KnowledgeEntries.Any(entry => entry.Name.Contains("진리의 석판", StringComparison.Ordinal)), "Expected unrelated narrative event knowledge to be excluded.");
        Assert(!compact.KnowledgeEntries.Any(entry => entry.Name.Contains("버섯이 먹고 싶어", StringComparison.Ordinal)), "Expected unrelated event knowledge to stay out of compact input.");
        Assert(!compact.KnowledgeEntries.Any(entry => entry.Name.Contains("속삭이는 골짜기", StringComparison.Ordinal)), "Expected short label false positives not to leak Whispering Hollow into compact knowledge.");

        var trigger = ToFoundationTrigger(new AdviceTrigger("manual", DateTimeOffset.UtcNow, true, true, "event-knowledge-strict", runState.RecentEvents.LastOrDefault()));
        var prompt = promptBuilder.FormatPrompt(promptBuilder.BuildInputPack(ToFoundationRunState(runState), trigger, slice, compact));
        Assert(prompt.Contains("나무 조각 [megacrit-sts2-core-models-events-woodcarvings]", StringComparison.Ordinal), "Expected prompt knowledge slice to keep the relevant Wood Carvings entry.");
        Assert(!prompt.Contains("속삭이는 골짜기", StringComparison.Ordinal), "Expected prompt knowledge slice to exclude unrelated event narratives.");
        Assert(!prompt.Contains("버섯이 먹고 싶어", StringComparison.Ordinal), "Expected prompt knowledge slice to stay event-local.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestEventCompactKnowledgeCanStayEmpty()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateRewardTestConfiguration(root);
        SeedEventKnowledgeNoiseCatalog(root);
        var knowledgeService = new FoundationKnowledgeCatalogService(configuration, root);
        var compactBuilder = new Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.RewardEventCompactAdvisorInputBuilder();
        var promptBuilder = new FoundationAdvicePromptBuilder(configuration);
        var runState = CreateEventRunState(
            "event-knowledge-empty-run",
            new[]
            {
                new LiveExportChoiceSummary("event-option", "새", "option-0", "최대 체력을 3 잃습니다.")
                {
                    BindingKind = "event-option",
                    BindingId = "option-0",
                    Enabled = true,
                    SemanticHints = new[] { "scene:event", "source:event-option-button" },
                },
                new LiveExportChoiceSummary("event-option", "뱀", "option-1", "50 골드를 얻습니다.")
                {
                    BindingKind = "event-option",
                    BindingId = "option-1",
                    Enabled = true,
                    SemanticHints = new[] { "scene:event", "source:event-option-button" },
                },
            });
        var slice = knowledgeService.BuildSlice(ToFoundationRunState(runState), 16, 8192);

        var compactResult = compactBuilder.Build(ToFoundationRunState(runState), slice);
        Assert(compactResult.Supported, "Expected explicit event facts to keep compact input supported even when strict event knowledge is empty.");
        Assert(compactResult.CompactInput is not null, "Expected compact event input.");
        var compact = compactResult.CompactInput!;
        Assert(compact.KnowledgeEntries.Count == 0, $"Expected strict event-local filtering to allow an empty knowledge slice when exact matches are absent, got {compact.KnowledgeEntries.Count} entries.");
        Assert(compact.EventFacts is not null && compact.EventFacts.OptionFacts.Any(fact => fact.Effects.Count > 0), "Expected event facts to remain sufficient without narrative knowledge.");

        var trigger = ToFoundationTrigger(new AdviceTrigger("manual", DateTimeOffset.UtcNow, true, true, "event-knowledge-empty", runState.RecentEvents.LastOrDefault()));
        var prompt = promptBuilder.FormatPrompt(promptBuilder.BuildInputPack(ToFoundationRunState(runState), trigger, slice, compact));
        Assert(prompt.Contains("knowledge_slice:", StringComparison.Ordinal), "Expected compact prompt to keep the knowledge slice section.");
        Assert(prompt.Contains("- none", StringComparison.Ordinal), "Expected compact prompt to render an empty event knowledge slice as none.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestShopCompactAdvisorInputBuilder()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateRewardTestConfiguration(root);
        SeedKnowledgeCatalog(root);
        var runState = CreateShopRunState("shop-compact-run");
        var knowledgeService = new FoundationKnowledgeCatalogService(configuration, root);
        var slice = knowledgeService.BuildSlice(ToFoundationRunState(runState), 16, 8192);
        var compactBuilder = new Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.RewardEventCompactAdvisorInputBuilder();

        var compactResult = compactBuilder.Build(ToFoundationRunState(runState), slice);
        Assert(compactResult.Supported, "Expected shop compact input to stay supported when visible purchase options are stable.");
        Assert(compactResult.CompactInput is not null, "Expected shop compact input.");
        var compact = compactResult.CompactInput!;
        Assert(string.Equals(compact.SceneType, "shop", StringComparison.Ordinal), $"Expected compact shop scene type, got {compact.SceneType}.");
        Assert(compact.ShopFacts is not null, "Expected shop compact facts.");
        Assert(compact.ShopFacts.InventoryOpen, "Expected inventory-open shop fact.");
        Assert(compact.ShopFacts.ItemCount == 2, $"Expected item count 2, got {compact.ShopFacts.ItemCount}.");
        Assert(compact.ShopFacts.ServiceCount == 1, $"Expected service count 1, got {compact.ShopFacts.ServiceCount}.");
        Assert(compact.ShopFacts.AffordableOptionCount == 2, $"Expected affordable option count 2, got {compact.ShopFacts.AffordableOptionCount}.");
        Assert(compact.VisibleOptions.Select(option => option.Label).SequenceEqual(runState.Snapshot.CurrentChoices.Select(choice => choice.Label), StringComparer.Ordinal), "Expected shop visible options to preserve exact current labels.");

        var duplicateRunState = CreateShopRunState("shop-duplicate-compact-run", CreateShopDuplicateVisibleOptionsSceneModelSnapshot("shop-duplicate-compact-run"));
        var duplicateSlice = knowledgeService.BuildSlice(ToFoundationRunState(duplicateRunState), 16, 8192);
        var duplicateResult = compactBuilder.Build(ToFoundationRunState(duplicateRunState), duplicateSlice);
        Assert(!duplicateResult.Supported, "Expected duplicate shop option labels to fail closed.");
        Assert(duplicateResult.DecisionBlockers.Contains("shop-duplicate-option-label:몸풀기", StringComparer.Ordinal), "Expected duplicate shop label blocker.");
        Assert(duplicateResult.DecisionBlockers.Contains("shop-compact-input-insufficient", StringComparer.Ordinal), "Expected duplicate shop labels to mark compact input insufficient.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestCombatCompactPreviewBuilder()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateRewardTestConfiguration(root);
        SeedKnowledgeCatalog(root);
        var runState = CreateCombatRunState("combat-compact-preview-run");
        var knowledgeService = new FoundationKnowledgeCatalogService(configuration, root);
        var slice = knowledgeService.BuildSlice(ToFoundationRunState(runState), 16, 8192);
        var compactBuilder = new Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.RewardEventCompactAdvisorInputBuilder();

        var compactResult = compactBuilder.Build(ToFoundationRunState(runState), slice);
        Assert(!compactResult.Supported, "Expected combat compact preview to stay no-call in this wave.");
        Assert(string.Equals(compactResult.ReasonCode, "combat-preview-only", StringComparison.Ordinal), $"Expected combat preview reason code, got {compactResult.ReasonCode}.");
        Assert(compactResult.CompactInput is not null, "Expected combat compact input.");
        var compact = compactResult.CompactInput!;
        Assert(string.Equals(compact.SceneType, "combat", StringComparison.Ordinal), $"Expected compact combat scene type, got {compact.SceneType}.");
        Assert(compact.CombatFacts is not null, "Expected combat compact facts.");
        Assert(compact.CombatFacts.PreviewOnly, "Expected combat facts to stay preview-only.");
        Assert(compact.CombatFacts.Energy == 3, $"Expected combat energy 3, got {compact.CombatFacts.Energy?.ToString() ?? "<null>"}.");
        Assert(compact.CombatFacts.HandCount == 3, $"Expected combat hand count 3, got {compact.CombatFacts.HandCount?.ToString() ?? "<null>"}.");
        Assert(compact.CombatFacts.DrawPileCount == 10, $"Expected draw pile count 10, got {compact.CombatFacts.DrawPileCount?.ToString() ?? "<null>"}.");
        Assert(compact.CombatFacts.RoundNumber == 3, $"Expected combat round number 3, got {compact.CombatFacts.RoundNumber?.ToString() ?? "<null>"}.");
        Assert(compact.CombatFacts.TargetableEnemyCount == 1, $"Expected targetable enemy count 1, got {compact.CombatFacts.TargetableEnemyCount?.ToString() ?? "<null>"}.");
        Assert(compact.CombatFacts.HittableEnemyCount == 1, $"Expected hittable enemy count 1, got {compact.CombatFacts.HittableEnemyCount?.ToString() ?? "<null>"}.");
        Assert(compact.CombatFacts.TargetSummary is not null && compact.CombatFacts.TargetSummary.Contains("Jaw Worm", StringComparison.OrdinalIgnoreCase), "Expected combat target summary to survive into compact facts.");
        Assert(compact.VisibleOptions.All(option => !string.Equals(option.Label, "Ping", StringComparison.OrdinalIgnoreCase)), "Expected combat preview compact options to exclude Ping.");
        Assert(compact.DecisionBlockers.Contains("combat-preview-only", StringComparer.Ordinal), "Expected combat preview blocker.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestCompactAdvicePromptBuilder()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateRewardTestConfiguration(root);
        SeedRewardKnowledgeCatalog(root);
        var runState = CreateRewardRunState("reward-compact-prompt-run");
        var knowledgeService = new FoundationKnowledgeCatalogService(configuration, root);
        var slice = knowledgeService.BuildSlice(ToFoundationRunState(runState), 16, 8192);
        var compactBuilder = new Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.RewardEventCompactAdvisorInputBuilder();
        var compactResult = compactBuilder.Build(ToFoundationRunState(runState), slice);
        var promptBuilder = new FoundationAdvicePromptBuilder(configuration);
        var trigger = ToFoundationTrigger(new AdviceTrigger("manual", DateTimeOffset.UtcNow, true, true, "compact-test", runState.RecentEvents.LastOrDefault()));
        var inputPack = promptBuilder.BuildInputPack(ToFoundationRunState(runState), trigger, slice, compactResult.CompactInput);
        var prompt = promptBuilder.FormatPrompt(inputPack);

        Assert(inputPack.CompactInput is not null, "Expected prompt pack to carry compact input.");
        Assert(prompt.Contains("scene_identity:", StringComparison.Ordinal), "Expected compact prompt to include scene identity.");
        Assert(prompt.Contains("reward_facts:", StringComparison.Ordinal), "Expected compact prompt to include reward facts.");
        Assert(prompt.Contains("visible_options:", StringComparison.Ordinal), "Expected compact prompt to include visible options.");
        Assert(!prompt.Contains("current_state_summary:", StringComparison.Ordinal), "Compact prompt should not include the legacy large state summary section.");
        Assert(!prompt.Contains("reward_option_set:", StringComparison.Ordinal), "Compact prompt should not include the legacy deterministic dump section.");

        var shopRunState = CreateShopRunState("shop-compact-prompt-run");
        var shopSlice = knowledgeService.BuildSlice(ToFoundationRunState(shopRunState), 16, 8192);
        var shopCompactResult = compactBuilder.Build(ToFoundationRunState(shopRunState), shopSlice);
        var shopInputPack = promptBuilder.BuildInputPack(
            ToFoundationRunState(shopRunState),
            trigger,
            shopSlice,
            shopCompactResult.CompactInput);
        var shopPrompt = promptBuilder.FormatPrompt(shopInputPack);

        Assert(shopInputPack.CompactInput is not null && string.Equals(shopInputPack.CompactInput.SceneType, "shop", StringComparison.Ordinal), "Expected compact shop input in prompt pack.");
        Assert(shopPrompt.Contains("shop_facts:", StringComparison.Ordinal), "Expected compact prompt to include shop facts.");
        Assert(!shopPrompt.Contains("reward_facts:", StringComparison.Ordinal), "Shop compact prompt should not include reward facts.");

        var woodCarvingsRunState = CreateWoodCarvingsEventRunState("event-compact-prompt-run");
        var eventSlice = knowledgeService.BuildSlice(ToFoundationRunState(woodCarvingsRunState), 16, 8192);
        var eventCompactResult = compactBuilder.Build(ToFoundationRunState(woodCarvingsRunState), eventSlice);
        var eventInputPack = promptBuilder.BuildInputPack(
            ToFoundationRunState(woodCarvingsRunState),
            ToFoundationTrigger(new AdviceTrigger("manual", DateTimeOffset.UtcNow, true, true, "event-compact-test", woodCarvingsRunState.RecentEvents.LastOrDefault())),
            eventSlice,
            eventCompactResult.CompactInput);
        var eventPrompt = promptBuilder.FormatPrompt(eventInputPack);
        Assert(eventInputPack.CompactInput is not null && string.Equals(eventInputPack.CompactInput.SceneType, "event", StringComparison.Ordinal), "Expected compact event input in prompt pack.");
        Assert(eventPrompt.Contains("event_facts:", StringComparison.Ordinal), "Expected compact prompt to include event facts.");
        Assert(eventPrompt.Contains("target_filter", StringComparison.Ordinal), "Expected compact event prompt to carry target filter facts.");
        Assert(eventPrompt.Contains("그 자체만으로 추천을 보류하지 마세요.", StringComparison.Ordinal), "Expected event compact prompt to prevent target-filter-only overblocking.");
        Assert(eventPrompt.Contains("별도의 추가 우선순위 필드가 없다는 이유만으로 decisionBlockers를 만들지 마세요.", StringComparison.Ordinal), "Expected event compact prompt to allow deck-summary-based comparison.");
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

static void TestCompactAdviceFinalizer()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateRewardTestConfiguration(root);
        SeedRewardKnowledgeCatalog(root);
        var runState = CreateRewardRunState("compact-finalizer-run");
        var knowledgeService = new FoundationKnowledgeCatalogService(configuration, root);
        var slice = knowledgeService.BuildSlice(ToFoundationRunState(runState), 16, 8192);
        var compactBuilder = new Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.RewardEventCompactAdvisorInputBuilder();
        var compactResult = compactBuilder.Build(ToFoundationRunState(runState), slice);
        var promptBuilder = new FoundationAdvicePromptBuilder(configuration);
        var inputPack = promptBuilder.BuildInputPack(
            ToFoundationRunState(runState),
            ToFoundationTrigger(new AdviceTrigger("manual", DateTimeOffset.UtcNow, true, true, "compact-finalizer-test", runState.RecentEvents.LastOrDefault())),
            slice,
            compactResult.CompactInput);
        var invalidResponse = new Sts2AiCompanion.Foundation.Contracts.AdviceResponse(
            "ok",
            "invalid-compact-label",
            "invalid-compact-label",
            "invalid-compact-label",
            "현재 화면에 없는 선택지",
            new[] { "model-owned-reason" },
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            0.4,
            Array.Empty<string>(),
            DateTimeOffset.UtcNow,
            inputPack.RunId,
            inputPack.TriggerKind,
            null,
            "{}");
        var finalized = Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor.AdviceResponseFinalizer.Apply(inputPack, invalidResponse);

        Assert(string.Equals(finalized.Status, "degraded", StringComparison.OrdinalIgnoreCase), "Expected compact finalizer to degrade invalid labels.");
        Assert(finalized.RecommendedChoiceLabel is null, "Expected compact finalizer to clear invalid labels.");
        Assert(finalized.DecisionBlockers.Contains("recommended-choice-not-in-current-scene-options", StringComparer.Ordinal), "Expected compact finalizer blocker.");
        Assert(finalized.ReasoningBullets.SequenceEqual(invalidResponse.ReasoningBullets, StringComparer.Ordinal), "Expected compact finalizer not to rewrite reasoning.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
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
            Assert(fakeClient.RequestCount == 0, $"Expected reward scenes to stay manual-only during refresh for scenario {scenario.Name}.");

            var manualRequested = host.RequestManualAdviceAsync().GetAwaiter().GetResult();
            Assert(manualRequested, $"Expected manual reward advice to succeed for scenario {scenario.Name}.");
            Assert(fakeClient.RequestCount == 1, $"Expected manual reward advice to invoke codex for scenario {scenario.Name}.");
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

static void TestCompanionHostScenePollingStaysResponsiveDuringAutoAdvice()
{
    var root = CreateTempDirectory();
    try
    {
        var baseConfiguration = CreateCompanionHostTestConfiguration(root, collectorModeEnabled: false);
        var configuration = baseConfiguration with
        {
            Assistant = baseConfiguration.Assistant with
            {
                AutoAdviceEnabled = true,
                LivePollIntervalMs = 250,
                MinAdviceIntervalMs = 0,
            },
        };
        SeedKnowledgeCatalog(root);

        var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        Directory.CreateDirectory(layout.LiveRoot);
        const string runId = "scene-poll-during-advice-run";

        var initialSnapshot = CreateHostSnapshot(runId, "shop");
        var eventSnapshot = CreateEventSceneModelSnapshot(runId);
        var session = new LiveExportSession("session-scene-poll", runId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 1, layout.LiveRoot, "choice-list-presented", initialSnapshot.CurrentScreen);
        var events = new[]
        {
            new LiveExportEventEnvelope(DateTimeOffset.UtcNow, 1, runId, "choice-list-presented", initialSnapshot.CurrentScreen, initialSnapshot.Act, initialSnapshot.Floor, new Dictionary<string, object?>()),
        };

        WriteJson(layout.SnapshotPath, initialSnapshot);
        WriteJson(layout.SessionPath, session);
        File.WriteAllText(layout.SummaryPath, "shop summary", Encoding.UTF8);
        Directory.CreateDirectory(Path.GetDirectoryName(layout.EventsPath)!);
        File.WriteAllText(layout.EventsPath, string.Join(Environment.NewLine, events.Select(SerializeNdjson)) + Environment.NewLine, Encoding.UTF8);
        File.WriteAllText(layout.RawObservationsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ScreenTransitionsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceCandidatesPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceDecisionsPath, string.Empty, Encoding.UTF8);
        Directory.CreateDirectory(layout.SemanticSnapshotsRoot);

        var fakeClient = new FakeCodexSessionClient(responseDelay: TimeSpan.FromSeconds(4));
        var host = new CompanionHost(configuration, root, fakeClient);
        try
        {
            host.StartAsync().GetAwaiter().GetResult();

            WaitForCondition(
                () => host.CurrentSnapshot.LatestSceneModel is { SceneType: "shop" },
                TimeSpan.FromSeconds(2),
                "Expected initial scene model to publish shop.");
            Assert(fakeClient.RequestCount == 0, "Expected shop refresh to stay no-call while polling remains responsive.");

            WriteJson(layout.SnapshotPath, eventSnapshot);
            File.WriteAllText(layout.SummaryPath, "event summary", Encoding.UTF8);

            WaitForCondition(
                () => host.CurrentSnapshot.LatestSceneModel is { SceneType: "event", SceneStage: "ancient-option" },
                TimeSpan.FromSeconds(2),
                "Expected polling to publish the newer event scene model.");
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

static void TestCodexCliContextOverflowDiagnostic()
{
    const string trace = """
    {"type":"thread.started","thread_id":"019cdcfc-aefb-76c1-92e7-d36d9d89d3cb"}
    {"type":"turn.started"}
    {"type":"error","message":"Error running remote compact task: {\n  \"error\": {\n    \"message\": \"Your input exceeds the context window of this model. Please adjust your input and try again.\",\n    \"type\": \"invalid_request_error\",\n    \"param\": \"input\",\n    \"code\": \"context_length_exceeded\"\n  }\n}"}
    {"type":"turn.failed","error":{"message":"Error running remote compact task: {\n  \"error\": {\n    \"message\": \"Your input exceeds the context window of this model. Please adjust your input and try again.\",\n    \"type\": \"invalid_request_error\",\n    \"param\": \"input\",\n    \"code\": \"context_length_exceeded\"\n  }\n}"}}
    """;

    var diagnostic = FoundationCodexCliClient.TryExtractExecErrorMessage(trace);

    Assert(
        string.Equals(diagnostic, "Codex 요청이 모델 컨텍스트 한도를 초과했습니다. 자동 조언 세션을 새로 시작하거나 입력 크기를 줄여야 합니다.", StringComparison.Ordinal),
        $"Expected context overflow to surface a direct diagnostic. actual={diagnostic ?? "null"}");
}

static void TestHostCodexCliClientRetriesWithoutResumeAfterContextOverflow()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateCompanionHostTestConfiguration(root, collectorModeEnabled: false);
        var fakeInner = new FakeFoundationCodexSessionClient();
        fakeInner.Enqueue(sessionId => (
            new Sts2AiCompanion.Foundation.Contracts.AdviceResponse(
                "degraded",
                "Codex 조언 사용 불가",
                "Codex 요청이 모델 컨텍스트 한도를 초과했습니다. 자동 조언 세션을 새로 시작하거나 입력 크기를 줄여야 합니다.",
                "현재 상태를 다시 확인하세요.",
                null,
                new[] { "Codex CLI 실행이 실패했거나 응답을 읽을 수 없었습니다." },
                new[] { "Codex 요청이 모델 컨텍스트 한도를 초과했습니다. 자동 조언 세션을 새로 시작하거나 입력 크기를 줄여야 합니다." },
                Array.Empty<string>(),
                new[] { "Codex 요청이 모델 컨텍스트 한도를 초과했습니다. 자동 조언 세션을 새로 시작하거나 입력 크기를 줄여야 합니다." },
                null,
                Array.Empty<string>(),
                DateTimeOffset.UtcNow,
                "retry-run",
                "choice-list-presented",
                sessionId,
                "{\"type\":\"thread.started\"}\n{\"type\":\"error\",\"message\":\"Error running remote compact task: {\\n  \\\"error\\\": {\\n    \\\"message\\\": \\\"Your input exceeds the context window of this model. Please adjust your input and try again.\\\",\\n    \\\"type\\\": \\\"invalid_request_error\\\",\\n    \\\"param\\\": \\\"input\\\",\\n    \\\"code\\\": \\\"context_length_exceeded\\\"\\n  }\\n}\"}"),
            sessionId));
        fakeInner.Enqueue(sessionId => (
            new Sts2AiCompanion.Foundation.Contracts.AdviceResponse(
                "ok",
                "headline-choice-list-presented",
                "summary-choice-list-presented",
                "action-choice-list-presented",
                "Pommel Strike",
                new[] { "reason-1" },
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                0.8,
                Array.Empty<string>(),
                DateTimeOffset.UtcNow,
                "retry-run",
                "choice-list-presented",
                "new-session-001",
                "{\"status\":\"ok\"}"),
            "new-session-001"));

        var client = Activator.CreateInstance(
            typeof(Sts2AiCompanion.Host.CodexCliClient),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            binder: null,
            args: new object?[] { fakeInner },
            culture: null) as Sts2AiCompanion.Host.CodexCliClient;
        Assert(client is not null, "Expected CodexCliClient test hook constructor to be available.");
        var inputPack = new AdviceInputPack(
            "retry-run",
            "choice-list-presented",
            DateTimeOffset.UtcNow,
            false,
            "event",
            "summary",
            CreateHostSnapshot("retry-run", "event"),
            Array.Empty<LiveExportEventEnvelope>(),
            Array.Empty<StaticKnowledgeEntry>(),
            Array.Empty<string>(),
            "constraints");

        var (response, sessionId) = client!.ExecuteAsync(
            inputPack,
            "prompt",
            "old-session-001",
            configuration.Assistant.CodexModel,
            configuration.Assistant.CodexReasoningEffort,
            CancellationToken.None).GetAwaiter().GetResult();

        Assert(fakeInner.RequestCount == 2, $"Expected a single retry without resume after context overflow. actual={fakeInner.RequestCount}");
        Assert(fakeInner.SeenSessionIds.Count == 2 && fakeInner.SeenSessionIds[0] == "old-session-001" && fakeInner.SeenSessionIds[1] is null, "Expected retry path to clear the Codex session id on the second attempt.");
        Assert(string.Equals(response.Status, "ok", StringComparison.Ordinal), $"Expected retry response to succeed. actual={response.Status}");
        Assert(string.Equals(sessionId, "new-session-001", StringComparison.Ordinal), $"Expected retry path to return the new session id. actual={sessionId ?? "null"}");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
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
        var snapshot = CreateEventSceneModelSnapshot(runId);
        var session = new LiveExportSession("session-001", runId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 1, layout.LiveRoot, "choice-list-presented", "event");
        var eventPayload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["screenEpisode"] = "event",
        };
        var events = new[]
        {
            new LiveExportEventEnvelope(DateTimeOffset.UtcNow, 1, runId, "choice-list-presented", "event", 1, 3, eventPayload),
        };

        WriteJson(layout.SnapshotPath, snapshot);
        WriteJson(layout.SessionPath, session);
        File.WriteAllText(layout.SummaryPath, "event summary", Encoding.UTF8);
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
            Assert(fakeClient.RequestCount == 0, "Expected event refresh to stay no-call on the compact advisor manual path.");
            Assert(host.CurrentSnapshot.LatestSceneModel is { SceneType: "event" }, "Expected event scene model to publish after refresh.");
            Assert(host.CurrentSnapshot.RunState?.NormalizedState.Scene.SceneType == "event", "Expected host run state to carry normalized foundation scene state.");

            var manualRequested = host.RequestManualAdviceAsync().GetAwaiter().GetResult();
            Assert(manualRequested, "Expected manual advice request to succeed after refresh.");
            Assert(fakeClient.RequestCount == 1, "Expected manual compact advice to invoke codex for the supported event scene.");
            Assert(host.CurrentSnapshot.LatestAdvice is not null, "Expected manual advice request to publish a response.");

            var promptPacks = Directory.GetFiles(host.CurrentSnapshot.Paths.PromptPacksRoot!, "*.json", SearchOption.TopDirectoryOnly)
                .OrderBy(File.GetLastWriteTimeUtc)
                .ToArray();
            Assert(promptPacks.Length >= 1, $"Expected prompt pack artifacts to be preserved. actual={promptPacks.Length}");
            var noCallPromptPack = JsonSerializer.Deserialize<AdviceInputPack>(File.ReadAllText(promptPacks[^1]), ConfigurationLoader.JsonOptions)
                ?? throw new InvalidOperationException("Expected manual event prompt pack.");
            Assert(string.Equals(noCallPromptPack.CurrentScreen, "event", StringComparison.Ordinal), $"Expected event prompt pack to preserve event screen, got {noCallPromptPack.CurrentScreen}.");
            Assert(noCallPromptPack.CompactInput is not null, "Expected manual event prompt pack to carry compact input.");
            Assert(string.Equals(noCallPromptPack.CompactInput.SceneType, "event", StringComparison.Ordinal), $"Expected compact event scene type, got {noCallPromptPack.CompactInput.SceneType}.");

            var retryRequested = host.RequestRetryLastAdviceAsync().GetAwaiter().GetResult();
            Assert(retryRequested, "Expected retry-last advice request to succeed after a prior advice run.");
            Assert(fakeClient.RequestCount == 2, "Expected retry-last advice to invoke codex again for the supported event scene.");
            Assert(host.CurrentSnapshot.LatestAdvice is not null, "Expected retry-last to preserve a response for the supported event scene.");

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

static void TestCompanionHostLiveSceneModelArtifacts()
{
    foreach (var scenario in CreateHostSceneModelScenarios())
    {
        ExecuteHostSceneModelScenario(scenario);
    }
}

static void TestCompanionHostManualRewardCompactAdviceFlow()
{
    var root = CreateTempDirectory();
    try
    {
        var baseConfiguration = CreateCompanionHostTestConfiguration(root, collectorModeEnabled: false);
        var configuration = baseConfiguration with
        {
            Assistant = baseConfiguration.Assistant with
            {
                AutoAdviceEnabled = false,
            },
        };
        var scenario = CreateDefaultRewardScenario();
        SeedRewardKnowledgeCatalog(root);

        var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        Directory.CreateDirectory(layout.LiveRoot);
        var snapshot = CreateRewardSnapshotForScenario(scenario, "manual-reward-compact-run");
        var session = new LiveExportSession("manual-reward-session", snapshot.RunId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, layout.LiveRoot, "choice-list-presented", "rewards");
        var events = CreateRewardEventsForScenario(snapshot.RunId, scenario);

        WriteJson(layout.SnapshotPath, snapshot);
        WriteJson(layout.SessionPath, session);
        File.WriteAllText(layout.SummaryPath, "manual reward summary", Encoding.UTF8);
        Directory.CreateDirectory(Path.GetDirectoryName(layout.EventsPath)!);
        File.WriteAllText(layout.EventsPath, string.Join(Environment.NewLine, events.Select(SerializeNdjson)) + Environment.NewLine, Encoding.UTF8);
        File.WriteAllText(layout.RawObservationsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ScreenTransitionsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceCandidatesPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceDecisionsPath, string.Empty, Encoding.UTF8);
        Directory.CreateDirectory(layout.SemanticSnapshotsRoot);

        var fakeClient = new FakeCodexSessionClient();
        var host = new CompanionHost(configuration, root, fakeClient);
        try
        {
            host.RefreshAsync().GetAwaiter().GetResult();
            Assert(fakeClient.RequestCount == 0, "Expected auto advice off to skip codex on refresh.");

            var manualRequested = host.RequestManualAdviceAsync().GetAwaiter().GetResult();
            Assert(manualRequested, "Expected manual reward advice request to succeed.");
            Assert(fakeClient.RequestCount == 1, "Expected manual reward advice to invoke codex through the compact path.");
            Assert(host.CurrentSnapshot.LatestAdvice is not null, "Expected manual reward advice to publish a response.");

            var promptPackPath = Directory.GetFiles(host.CurrentSnapshot.Paths.PromptPacksRoot!, "*.json", SearchOption.TopDirectoryOnly)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .First();
            var promptPack = JsonSerializer.Deserialize<AdviceInputPack>(File.ReadAllText(promptPackPath), ConfigurationLoader.JsonOptions)
                ?? throw new InvalidOperationException("Expected manual reward prompt pack.");
            Assert(promptPack.CompactInput is not null, "Expected manual reward prompt pack to carry compact input.");
            Assert(string.Equals(promptPack.CompactInput.SceneType, "reward", StringComparison.Ordinal), $"Expected compact reward scene type, got {promptPack.CompactInput.SceneType}.");
            Assert(promptPack.CompactInput.VisibleOptions.Select(option => option.Label).Contains(host.CurrentSnapshot.LatestAdvice.RecommendedChoiceLabel, StringComparer.Ordinal), "Expected reward recommendation to exact-match a visible compact option label.");
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

static void TestCompanionHostAutomaticCompactAdviceRemainsManualOnly()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateCompanionHostTestConfiguration(root, collectorModeEnabled: false);
        var scenario = CreateDefaultRewardScenario();
        SeedRewardKnowledgeCatalog(root);

        void ExecuteManagedSceneScenario(
            string name,
            LiveExportSnapshot snapshot,
            LiveExportSession session,
            IReadOnlyList<LiveExportEventEnvelope> events,
            bool expectManualModelCall,
            string expectedSceneType,
            string? expectedNoCallBlocker = null)
        {
            var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
            Directory.CreateDirectory(layout.LiveRoot);
            WriteJson(layout.SnapshotPath, snapshot);
            WriteJson(layout.SessionPath, session);
            File.WriteAllText(layout.SummaryPath, $"{name} auto compact summary", Encoding.UTF8);
            Directory.CreateDirectory(Path.GetDirectoryName(layout.EventsPath)!);
            File.WriteAllText(layout.EventsPath, string.Join(Environment.NewLine, events.Select(SerializeNdjson)) + Environment.NewLine, Encoding.UTF8);
            File.WriteAllText(layout.RawObservationsPath, string.Empty, Encoding.UTF8);
            File.WriteAllText(layout.ScreenTransitionsPath, string.Empty, Encoding.UTF8);
            File.WriteAllText(layout.ChoiceCandidatesPath, string.Empty, Encoding.UTF8);
            File.WriteAllText(layout.ChoiceDecisionsPath, string.Empty, Encoding.UTF8);
            Directory.CreateDirectory(layout.SemanticSnapshotsRoot);

            var fakeClient = new FakeCodexSessionClient();
            var host = new CompanionHost(configuration, root, fakeClient);
            try
            {
                host.RefreshAsync().GetAwaiter().GetResult();
                Assert(fakeClient.RequestCount == 0, $"Expected {name} scene to stay manual-only even when auto advice is enabled.");
                Assert(host.CurrentSnapshot.LatestAdvice is null, $"Expected skipped automatic {name} compact advice not to publish a response.");

                var manualRequested = host.RequestManualAdviceAsync().GetAwaiter().GetResult();
                Assert(manualRequested, $"Expected manual {name} compact advice request to be accepted.");
                Assert(fakeClient.RequestCount == (expectManualModelCall ? 1 : 0), expectManualModelCall
                    ? $"Expected manual {name} advice to invoke codex once."
                    : $"Expected manual {name} advice to stay no-call.");

                var promptPackPath = Directory.GetFiles(host.CurrentSnapshot.Paths.PromptPacksRoot!, "*.json", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(File.GetLastWriteTimeUtc)
                    .First();
                var promptPack = JsonSerializer.Deserialize<AdviceInputPack>(File.ReadAllText(promptPackPath), ConfigurationLoader.JsonOptions)
                    ?? throw new InvalidOperationException($"Expected {name} prompt pack.");
                Assert(promptPack.CompactInput is not null, $"Expected {name} prompt pack to carry compact input.");
                Assert(string.Equals(promptPack.CompactInput.SceneType, expectedSceneType, StringComparison.Ordinal), $"Expected compact scene type {expectedSceneType}, got {promptPack.CompactInput.SceneType}.");

                if (expectManualModelCall)
                {
                    Assert(host.CurrentSnapshot.LatestAdvice is not null, $"Expected {name} manual advice response.");
                }
                else
                {
                    Assert(host.CurrentSnapshot.LatestAdvice is not null, $"Expected {name} no-call advice response.");
                    Assert(host.CurrentSnapshot.LatestAdvice.DecisionBlockers.Contains(expectedNoCallBlocker ?? string.Empty, StringComparer.Ordinal), $"Expected {name} no-call blocker '{expectedNoCallBlocker}'.");
                }
            }
            finally
            {
                host.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
        }

        ExecuteManagedSceneScenario(
            "reward",
            CreateRewardSnapshotForScenario(scenario, "auto-reward-compact-run"),
            new LiveExportSession("auto-reward-session", "auto-reward-compact-run", "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(root, "live"), "choice-list-presented", "rewards"),
            CreateRewardEventsForScenario("auto-reward-compact-run", scenario),
            expectManualModelCall: true,
            expectedSceneType: "reward");

        ExecuteManagedSceneScenario(
            "event",
            CreateEventSceneModelSnapshot("auto-event-compact-run"),
            new LiveExportSession("auto-event-session", "auto-event-compact-run", "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(root, "live"), "choice-list-presented", "event"),
            CreateEventEvents("auto-event-compact-run"),
            expectManualModelCall: true,
            expectedSceneType: "event");

        ExecuteManagedSceneScenario(
            "shop",
            CreateShopSceneModelSnapshot("auto-shop-compact-run"),
            new LiveExportSession("auto-shop-session", "auto-shop-compact-run", "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(root, "live"), "choice-list-presented", "shop"),
            CreateShopEvents("auto-shop-compact-run", CreateShopSceneModelSnapshot("auto-shop-compact-run").CurrentChoices),
            expectManualModelCall: true,
            expectedSceneType: "shop");

        ExecuteManagedSceneScenario(
            "combat",
            CreateCombatPreviewSceneModelSnapshot("auto-combat-compact-run"),
            new LiveExportSession("auto-combat-session", "auto-combat-compact-run", "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(root, "live"), "combat-started", "combat"),
            CreateCombatRunState("auto-combat-compact-run", CreateCombatPreviewSceneModelSnapshot("auto-combat-compact-run")).RecentEvents,
            expectManualModelCall: false,
            expectedSceneType: "combat",
            expectedNoCallBlocker: "combat-preview-only");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestCompanionHostShopCompactAdviceSafety()
{
    var root = CreateTempDirectory();
    try
    {
        var baseConfiguration = CreateCompanionHostTestConfiguration(root, collectorModeEnabled: false);
        var configuration = baseConfiguration with
        {
            Assistant = baseConfiguration.Assistant with
            {
                AutoAdviceEnabled = true,
            },
        };
        SeedKnowledgeCatalog(root);

        void ExecuteShopScenario(
            string runId,
            LiveExportSnapshot snapshot,
            bool expectModelCall,
            string? expectedBlocker)
        {
            var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
            Directory.CreateDirectory(layout.LiveRoot);
            var session = new LiveExportSession($"shop-session-{runId}", runId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 1, layout.LiveRoot, "choice-list-presented", "shop");
            var events = CreateShopEvents(runId, snapshot.CurrentChoices);

            WriteJson(layout.SnapshotPath, snapshot);
            WriteJson(layout.SessionPath, session);
            File.WriteAllText(layout.SummaryPath, $"shop compact safety summary :: {runId}", Encoding.UTF8);
            Directory.CreateDirectory(Path.GetDirectoryName(layout.EventsPath)!);
            File.WriteAllText(layout.EventsPath, string.Join(Environment.NewLine, events.Select(SerializeNdjson)) + Environment.NewLine, Encoding.UTF8);
            File.WriteAllText(layout.RawObservationsPath, string.Empty, Encoding.UTF8);
            File.WriteAllText(layout.ScreenTransitionsPath, string.Empty, Encoding.UTF8);
            File.WriteAllText(layout.ChoiceCandidatesPath, string.Empty, Encoding.UTF8);
            File.WriteAllText(layout.ChoiceDecisionsPath, string.Empty, Encoding.UTF8);
            Directory.CreateDirectory(layout.SemanticSnapshotsRoot);

            var fakeClient = new FakeCodexSessionClient();
            var host = new CompanionHost(configuration, root, fakeClient);
            try
            {
                host.RefreshAsync().GetAwaiter().GetResult();
                Assert(fakeClient.RequestCount == 0, "Expected shop refresh to stay manual-only even when auto advice is enabled.");

                var sceneModel = host.CurrentSnapshot.LatestSceneModel
                    ?? throw new InvalidOperationException("Expected latest scene model for shop compact safety.");
                Assert(string.Equals(sceneModel.SceneType, "shop", StringComparison.Ordinal), "Expected shop scene type.");
                Assert(sceneModel.Options.Select(option => option.Label).SequenceEqual(snapshot.CurrentChoices.Select(choice => choice.Label), StringComparer.Ordinal), "Expected shop visible options to preserve the current labels.");

                var manualRequested = host.RequestManualAdviceAsync().GetAwaiter().GetResult();
                Assert(manualRequested, "Expected manual shop compact request to be accepted.");
                Assert(fakeClient.RequestCount == (expectModelCall ? 1 : 0), expectModelCall
                    ? "Expected supported shop manual request to invoke codex."
                    : "Expected duplicate shop manual request to fail closed without invoking codex.");
                Assert(host.CurrentSnapshot.LatestAdvice is not null, "Expected manual shop compact request to publish a response.");

                var promptPackPath = Directory.GetFiles(host.CurrentSnapshot.Paths.PromptPacksRoot!, "*.json", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(File.GetLastWriteTimeUtc)
                    .FirstOrDefault()
                    ?? throw new InvalidOperationException("Expected a prompt pack artifact for shop manual flow.");
                var promptPack = JsonSerializer.Deserialize<AdviceInputPack>(File.ReadAllText(promptPackPath), ConfigurationLoader.JsonOptions)
                    ?? throw new InvalidOperationException("Expected a shop prompt pack.");
                Assert(string.Equals(promptPack.CurrentScreen, "shop", StringComparison.Ordinal), "Expected shop prompt pack to preserve current screen.");
                Assert(promptPack.CompactInput is not null, "Expected shop prompt pack to carry compact input.");
                Assert(string.Equals(promptPack.CompactInput.SceneType, "shop", StringComparison.Ordinal), $"Expected compact shop scene type, got {promptPack.CompactInput.SceneType}.");
                Assert(promptPack.CompactInput.ShopFacts is not null, "Expected shop prompt pack to include shop facts.");

                if (expectModelCall)
                {
                    Assert(promptPack.CompactInput.ShopFacts.ItemCount == 2, $"Expected shop item count 2, got {promptPack.CompactInput.ShopFacts.ItemCount}.");
                    Assert(promptPack.CompactInput.ShopFacts.ServiceCount == 1, $"Expected shop service count 1, got {promptPack.CompactInput.ShopFacts.ServiceCount}.");
                    Assert(promptPack.CompactInput.ShopFacts.AffordableOptionCount == 2, $"Expected shop affordable option count 2, got {promptPack.CompactInput.ShopFacts.AffordableOptionCount}.");
                    Assert(host.CurrentSnapshot.LatestAdvice.RecommendedChoiceLabel is not null, "Expected supported shop manual advice to produce a recommendation label.");
                    Assert(promptPack.CompactInput.VisibleOptions.Select(option => option.Label).Contains(host.CurrentSnapshot.LatestAdvice.RecommendedChoiceLabel, StringComparer.Ordinal), "Expected shop recommendation to exact-match a visible compact option label.");
                }
                else
                {
                    Assert(string.Equals(host.CurrentSnapshot.LatestAdvice.Status, "degraded", StringComparison.OrdinalIgnoreCase), "Expected duplicate-label shop advice to degrade.");
                    Assert(host.CurrentSnapshot.LatestAdvice.DecisionBlockers.Contains(expectedBlocker ?? string.Empty, StringComparer.Ordinal), $"Expected duplicate-label blocker '{expectedBlocker}'.");
                    Assert(host.CurrentSnapshot.LatestAdvice.DecisionBlockers.Contains("shop-compact-input-insufficient", StringComparer.Ordinal), "Expected duplicate-label shop path to mark compact input insufficient.");
                    Assert(host.CurrentSnapshot.LatestAdvice.RecommendedChoiceLabel is null, "Expected duplicate-label shop path to clear recommendation label.");
                }

                var retryRequested = host.RequestRetryLastAdviceAsync().GetAwaiter().GetResult();
                Assert(retryRequested, "Expected retry-last to resolve the shop prompt pack.");
                Assert(fakeClient.RequestCount == (expectModelCall ? 2 : 0), expectModelCall
                    ? "Expected retry-last on supported shop flow to invoke codex again."
                    : "Expected retry-last on duplicate-label shop flow to remain no-call.");
                Assert(host.CurrentSnapshot.LatestAdvice is not null, "Expected retry-last to publish a shop response.");
                if (!expectModelCall)
                {
                    Assert(host.CurrentSnapshot.LatestAdvice.DecisionBlockers.Contains(expectedBlocker ?? string.Empty, StringComparer.Ordinal), $"Expected retry-last to preserve blocker '{expectedBlocker}'.");
                }
            }
            finally
            {
                host.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
        }

        ExecuteShopScenario(
            runId: "shop-compact-supported-run",
            snapshot: CreateShopSceneModelSnapshot("shop-compact-supported-run"),
            expectModelCall: true,
            expectedBlocker: null);

        ExecuteShopScenario(
            runId: "shop-compact-duplicate-run",
            snapshot: CreateShopDuplicateVisibleOptionsSceneModelSnapshot("shop-compact-duplicate-run"),
            expectModelCall: false,
            expectedBlocker: "shop-duplicate-option-label:몸풀기");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestCompanionHostCombatPreviewCompactAdviceSafety()
{
    var root = CreateTempDirectory();
    try
    {
        var baseConfiguration = CreateCompanionHostTestConfiguration(root, collectorModeEnabled: false);
        var configuration = baseConfiguration with
        {
            Assistant = baseConfiguration.Assistant with
            {
                AutoAdviceEnabled = true,
            },
        };
        SeedKnowledgeCatalog(root);

        var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        Directory.CreateDirectory(layout.LiveRoot);
        var snapshot = CreateCombatPreviewSceneModelSnapshot("combat-preview-safety-run");
        var session = new LiveExportSession("combat-preview-session", snapshot.RunId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 1, layout.LiveRoot, "combat-started", "combat");
        var events = CreateCombatRunState(snapshot.RunId, snapshot).RecentEvents;

        WriteJson(layout.SnapshotPath, snapshot);
        WriteJson(layout.SessionPath, session);
        File.WriteAllText(layout.SummaryPath, "combat preview safety summary", Encoding.UTF8);
        Directory.CreateDirectory(Path.GetDirectoryName(layout.EventsPath)!);
        File.WriteAllText(layout.EventsPath, string.Join(Environment.NewLine, events.Select(SerializeNdjson)) + Environment.NewLine, Encoding.UTF8);
        File.WriteAllText(layout.RawObservationsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ScreenTransitionsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceCandidatesPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceDecisionsPath, string.Empty, Encoding.UTF8);
        Directory.CreateDirectory(layout.SemanticSnapshotsRoot);

        var fakeClient = new FakeCodexSessionClient();
        var host = new CompanionHost(configuration, root, fakeClient);
        try
        {
            host.RefreshAsync().GetAwaiter().GetResult();
            Assert(fakeClient.RequestCount == 0, "Expected combat preview refresh to stay auto-skip safe.");

            var sceneModel = host.CurrentSnapshot.LatestSceneModel
                ?? throw new InvalidOperationException("Expected latest scene model for combat preview safety.");
            Assert(string.Equals(sceneModel.SceneType, "combat", StringComparison.Ordinal), "Expected combat scene type.");
            Assert(sceneModel.Combat is not null, "Expected combat preview scene model to carry combat details.");

            var manualRequested = host.RequestManualAdviceAsync().GetAwaiter().GetResult();
            Assert(manualRequested, "Expected manual combat preview request to be accepted.");
            Assert(fakeClient.RequestCount == 0, "Expected manual combat preview request to fail closed without invoking codex.");
            Assert(host.CurrentSnapshot.LatestAdvice is not null, "Expected manual combat preview request to publish a degraded response.");
            Assert(string.Equals(host.CurrentSnapshot.LatestAdvice.Status, "degraded", StringComparison.OrdinalIgnoreCase), "Expected combat preview advice to degrade.");
            Assert(host.CurrentSnapshot.LatestAdvice.DecisionBlockers.Contains("combat-preview-only", StringComparer.Ordinal), "Expected combat preview blocker.");
            Assert(host.CurrentSnapshot.LatestAdvice.RecommendedChoiceLabel is null, "Expected combat preview to keep recommendation label empty.");

            var promptPackPath = Directory.GetFiles(host.CurrentSnapshot.Paths.PromptPacksRoot!, "*.json", SearchOption.TopDirectoryOnly)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault()
                ?? throw new InvalidOperationException("Expected a prompt pack artifact for combat preview flow.");
            var promptPack = JsonSerializer.Deserialize<AdviceInputPack>(File.ReadAllText(promptPackPath), ConfigurationLoader.JsonOptions)
                ?? throw new InvalidOperationException("Expected a combat preview prompt pack.");
            Assert(promptPack.CompactInput is not null, "Expected combat preview prompt pack to carry compact input.");
            Assert(string.Equals(promptPack.CompactInput.SceneType, "combat", StringComparison.Ordinal), $"Expected compact combat scene type, got {promptPack.CompactInput.SceneType}.");
            Assert(promptPack.CompactInput.CombatFacts is not null, "Expected combat preview prompt pack to include combat facts.");
            Assert(promptPack.CompactInput.CombatFacts.PreviewOnly, "Expected combat preview facts to stay preview-only.");

            var retryRequested = host.RequestRetryLastAdviceAsync().GetAwaiter().GetResult();
            Assert(retryRequested, "Expected retry-last to resolve the combat preview prompt pack.");
            Assert(fakeClient.RequestCount == 0, "Expected retry-last on combat preview flow to remain no-call.");
            Assert(host.CurrentSnapshot.LatestAdvice is not null && host.CurrentSnapshot.LatestAdvice.DecisionBlockers.Contains("combat-preview-only", StringComparer.Ordinal), "Expected retry-last to preserve the combat preview blocker.");
            Assert(host.CurrentSnapshot.LatestAdvice.RecommendedChoiceLabel is null, "Expected retry-last combat preview to keep recommendation label empty.");
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

static void TestReplayAdvisorValidatorCompactPath()
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateCompanionHostTestConfiguration(root, collectorModeEnabled: false);
        SeedRewardKnowledgeCatalog(root);
        SeedKnowledgeCatalog(root);

        var rewardFixtureRoot = Path.Combine(root, "reward-compact-fixture");
        WriteReplayFixture(
            rewardFixtureRoot,
            configuration,
            CreateRewardSnapshotForScenario(CreateDefaultRewardScenario(), "replay-compact-reward-run"),
            new LiveExportSession("replay-reward-session", "replay-compact-reward-run", "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(root, "live"), "choice-list-presented", "rewards"),
            CreateRewardEventsForScenario("replay-compact-reward-run", CreateDefaultRewardScenario()),
            "reward replay summary");

        var eventFixtureRoot = Path.Combine(root, "event-compact-fixture");
        WriteReplayFixture(
            eventFixtureRoot,
            configuration,
            CreateEventSceneModelSnapshot("replay-compact-event-run"),
            new LiveExportSession("replay-event-session", "replay-compact-event-run", "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(root, "live"), "choice-list-presented", "event"),
            CreateEventEvents("replay-compact-event-run"),
            "event replay summary");

        var shopFixtureRoot = Path.Combine(root, "shop-compact-fixture");
        var shopSnapshot = CreateShopSceneModelSnapshot("replay-compact-shop-run");
        WriteReplayFixture(
            shopFixtureRoot,
            configuration,
            shopSnapshot,
            new LiveExportSession("replay-shop-session", "replay-compact-shop-run", "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(root, "live"), "choice-list-presented", "shop"),
            CreateShopEvents("replay-compact-shop-run", shopSnapshot.CurrentChoices),
            "shop replay summary");

        var combatFixtureRoot = Path.Combine(root, "combat-compact-fixture");
        var combatSnapshot = CreateCombatPreviewSceneModelSnapshot("replay-compact-combat-run");
        WriteReplayFixture(
            combatFixtureRoot,
            configuration,
            combatSnapshot,
            new LiveExportSession("replay-combat-session", "replay-compact-combat-run", "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(root, "live"), "combat-started", "combat"),
            CreateCombatRunState("replay-compact-combat-run", combatSnapshot).RecentEvents,
            "combat replay summary");

        var replayValidator = new FoundationReplayAdvisorValidator(configuration, root);
        var rewardResult = replayValidator.ValidateAsync(rewardFixtureRoot, null, CancellationToken.None).GetAwaiter().GetResult();
        var rewardPromptPack = JsonSerializer.Deserialize<Sts2AiCompanion.Foundation.Contracts.AdviceInputPack>(File.ReadAllText(rewardResult.PromptPackPath), ConfigurationLoader.JsonOptions)
            ?? throw new InvalidOperationException("Expected replay reward prompt pack.");
        Assert(rewardPromptPack.CompactInput is not null, "Expected replay reward prompt pack to carry compact input.");
        Assert(string.Equals(rewardPromptPack.CompactInput.SceneType, "reward", StringComparison.Ordinal), $"Expected replay reward compact scene type, got {rewardPromptPack.CompactInput.SceneType}.");

        var eventResult = replayValidator.ValidateAsync(eventFixtureRoot, null, CancellationToken.None).GetAwaiter().GetResult();
        var eventPromptPack = JsonSerializer.Deserialize<Sts2AiCompanion.Foundation.Contracts.AdviceInputPack>(File.ReadAllText(eventResult.PromptPackPath), ConfigurationLoader.JsonOptions)
            ?? throw new InvalidOperationException("Expected replay event prompt pack.");
        Assert(eventPromptPack.CompactInput is not null, "Expected replay event prompt pack to carry compact input.");
        Assert(string.Equals(eventPromptPack.CompactInput.SceneType, "event", StringComparison.Ordinal), $"Expected replay event compact scene type, got {eventPromptPack.CompactInput.SceneType}.");
        Assert(eventPromptPack.CompactInput.EventFacts is not null, "Expected replay event prompt pack to include event facts.");

        var shopResult = replayValidator.ValidateAsync(shopFixtureRoot, null, CancellationToken.None).GetAwaiter().GetResult();
        var shopPromptPack = JsonSerializer.Deserialize<Sts2AiCompanion.Foundation.Contracts.AdviceInputPack>(File.ReadAllText(shopResult.PromptPackPath), ConfigurationLoader.JsonOptions)
            ?? throw new InvalidOperationException("Expected replay shop prompt pack.");
        var shopAdvice = JsonSerializer.Deserialize<Sts2AiCompanion.Foundation.Contracts.AdviceResponse>(File.ReadAllText(shopResult.AdviceJsonPath), ConfigurationLoader.JsonOptions)
            ?? throw new InvalidOperationException("Expected replay shop advice response.");
        Assert(shopPromptPack.CompactInput is not null, "Expected replay shop prompt pack to carry compact input.");
        Assert(string.Equals(shopPromptPack.CompactInput.SceneType, "shop", StringComparison.Ordinal), $"Expected replay shop compact scene type, got {shopPromptPack.CompactInput.SceneType}.");
        Assert(shopPromptPack.CompactInput.ShopFacts is not null, "Expected replay shop prompt pack to include shop facts.");
        Assert(!shopAdvice.DecisionBlockers.Contains("unsupported-scene-for-compact-advisor:shop", StringComparer.Ordinal), "Expected replay shop fixture to stay on the compact shop path.");

        var combatResult = replayValidator.ValidateAsync(combatFixtureRoot, null, CancellationToken.None).GetAwaiter().GetResult();
        var combatPromptPack = JsonSerializer.Deserialize<Sts2AiCompanion.Foundation.Contracts.AdviceInputPack>(File.ReadAllText(combatResult.PromptPackPath), ConfigurationLoader.JsonOptions)
            ?? throw new InvalidOperationException("Expected replay combat prompt pack.");
        var combatAdvice = JsonSerializer.Deserialize<Sts2AiCompanion.Foundation.Contracts.AdviceResponse>(File.ReadAllText(combatResult.AdviceJsonPath), ConfigurationLoader.JsonOptions)
            ?? throw new InvalidOperationException("Expected replay combat advice response.");
        Assert(combatPromptPack.CompactInput is not null, "Expected replay combat prompt pack to carry compact input.");
        Assert(string.Equals(combatPromptPack.CompactInput.SceneType, "combat", StringComparison.Ordinal), $"Expected replay combat compact scene type, got {combatPromptPack.CompactInput.SceneType}.");
        Assert(combatPromptPack.CompactInput.CombatFacts is not null, "Expected replay combat prompt pack to include combat facts.");
        Assert(string.Equals(combatAdvice.Status, "degraded", StringComparison.OrdinalIgnoreCase), "Expected replay combat compact path to stay degraded preview-only.");
        Assert(combatAdvice.DecisionBlockers.Contains("combat-preview-only", StringComparer.Ordinal), "Expected replay combat advice to carry combat preview blocker.");
        Assert(combatAdvice.RecommendedChoiceLabel is null, "Expected replay combat preview advice to keep recommendation label empty.");
    }
    finally
    {
        SafeDeleteDirectory(root);
    }
}

static void TestCompanionHostCombatSceneModelClassification()
{
    var sceneModel = ResolveHostSceneModelForSnapshot(
        "combat-classification",
        "combat-started",
        CreateCombatSceneModelSnapshot("scene-model-combat-run"));

    Assert(string.Equals(sceneModel.SceneType, "combat", StringComparison.Ordinal), $"Expected combat scene type, got {sceneModel.SceneType}.");
    Assert(string.Equals(sceneModel.CanonicalOwner, "combat", StringComparison.Ordinal), $"Expected combat canonical owner, got {sceneModel.CanonicalOwner}.");
    Assert(sceneModel.Combat is not null, "Expected combat details.");
    Assert(sceneModel.Combat.HandCount == 3, $"Expected structured combat hand count 3, got {sceneModel.Combat.HandCount}.");
    Assert(sceneModel.Combat.DrawPileCount == 10, $"Expected draw pile count 10, got {sceneModel.Combat.DrawPileCount?.ToString() ?? "<null>"}.");
    Assert(sceneModel.Combat.DiscardPileCount == 4, $"Expected discard pile count 4, got {sceneModel.Combat.DiscardPileCount?.ToString() ?? "<null>"}.");
    Assert(sceneModel.Combat.ExhaustPileCount == 1, $"Expected exhaust pile count 1, got {sceneModel.Combat.ExhaustPileCount?.ToString() ?? "<null>"}.");
    Assert(sceneModel.Combat.PlayPileCount == 2, $"Expected play pile count 2, got {sceneModel.Combat.PlayPileCount?.ToString() ?? "<null>"}.");
    Assert(string.Equals(sceneModel.Combat.EnemyIntentSummary, "Jaw Worm attacks for 11", StringComparison.Ordinal), $"Expected enemy intent summary, got '{sceneModel.Combat.EnemyIntentSummary ?? "<null>"}'.");
    Assert(!sceneModel.MissingFacts.Contains("combat-pile-counts-partial", StringComparer.Ordinal), "Structured pile counts should not surface as missing facts when populated.");
    Assert(!sceneModel.MissingFacts.Contains("combat-enemy-intent-summary-missing", StringComparer.Ordinal), "Enemy intent should not be missing when the structured field is populated.");

    var handCards = sceneModel.Options.Count(option => option.Tags.Contains("category:hand-card", StringComparer.OrdinalIgnoreCase));
    var pileButtons = sceneModel.Options.Count(option => option.Tags.Contains("category:pile-button", StringComparer.OrdinalIgnoreCase));
    var combatActions = sceneModel.Options.Count(option => option.Tags.Contains("category:combat-action", StringComparer.OrdinalIgnoreCase));
    var utilityOptions = sceneModel.Options.Count(option => option.Tags.Contains("category:utility", StringComparer.OrdinalIgnoreCase));
    Assert(handCards == 3, $"Expected three hand-card options, got {handCards}.");
    Assert(pileButtons == 4, $"Expected four pile-button options, got {pileButtons}.");
    Assert(combatActions == 1, $"Expected one combat-action option, got {combatActions}.");
    Assert(utilityOptions == 1, $"Expected one utility option, got {utilityOptions}.");
    Assert(sceneModel.Options.Any(option => string.Equals(option.Label, "Ping", StringComparison.OrdinalIgnoreCase)
                                            && option.Tags.Contains("category:utility", StringComparer.OrdinalIgnoreCase)), "Expected Ping to stay present in scene truth as a utility option.");
}

static void TestCompanionHostEventSceneModelOptionDeduping()
{
    var runState = CreateWoodCarvingsEventRunState("scene-model-event-wood-carvings");
    var sceneModel = ResolveHostSceneModelForSnapshot(
        runState.Snapshot.RunId,
        "choice-list-presented",
        runState.Snapshot);

    Assert(string.Equals(sceneModel.SceneType, "event", StringComparison.Ordinal), $"Expected event scene type, got {sceneModel.SceneType}.");
    Assert(string.Equals(sceneModel.Event?.EventIdentity, "WoodCarvings", StringComparison.Ordinal), $"Expected Wood Carvings scene identity, got {sceneModel.Event?.EventIdentity ?? "<null>"}.");
    Assert(sceneModel.Options.Count == 3, $"Expected Wood Carvings scene model options to dedupe to 3, got {sceneModel.Options.Count}.");

    var bird = sceneModel.Options.FirstOrDefault(option => string.Equals(option.Label, "새", StringComparison.Ordinal))
        ?? throw new InvalidOperationException("Expected 새 option.");
    var snake = sceneModel.Options.FirstOrDefault(option => string.Equals(option.Label, "뱀", StringComparison.Ordinal))
        ?? throw new InvalidOperationException("Expected 뱀 option.");
    var torus = sceneModel.Options.FirstOrDefault(option => string.Equals(option.Label, "고리", StringComparison.Ordinal))
        ?? throw new InvalidOperationException("Expected 고리 option.");

    Assert(bird.Description?.Contains("쪼기", StringComparison.Ordinal) == true, $"Expected 새 description to fill the transformed card title, got '{bird.Description ?? "<null>"}'.");
    Assert(snake.Description?.Contains("미끈거림", StringComparison.Ordinal) == true, $"Expected 뱀 description to fill the enchantment title, got '{snake.Description ?? "<null>"}'.");
    Assert(torus.Description?.Contains("고리형 강인함", StringComparison.Ordinal) == true, $"Expected 고리 description to fill the transformed card title, got '{torus.Description ?? "<null>"}'.");

    var strippedSnapshot = CreateWoodCarvingsEventSnapshotWithoutTypedDetails("scene-model-event-wood-carvings-live-like");
    var strippedSceneModel = ResolveHostSceneModelForSnapshot(
        strippedSnapshot.RunId,
        "choice-list-presented",
        strippedSnapshot);

    Assert(string.Equals(strippedSceneModel.Event?.EventIdentity, "WoodCarvings", StringComparison.Ordinal), $"Expected live-like Wood Carvings scene identity inference, got {strippedSceneModel.Event?.EventIdentity ?? "<null>"}.");
    Assert(!strippedSceneModel.MissingFacts.Contains("event-identity-missing", StringComparer.Ordinal), "Expected live-like Wood Carvings binding identity to prevent event-identity-missing.");
    Assert(strippedSceneModel.Options.Count == 3, $"Expected live-like Wood Carvings scene model options to dedupe to 3, got {strippedSceneModel.Options.Count}.");
    Assert(strippedSceneModel.Options.Select(option => option.Label).SequenceEqual(new[] { "새", "뱀", "고리" }, StringComparer.Ordinal), $"Expected live-like Wood Carvings labels to stay on 새/뱀/고리, got {string.Join(", ", strippedSceneModel.Options.Select(option => option.Label))}.");
    Assert(strippedSceneModel.Options.Any(option => string.Equals(option.Label, "새", StringComparison.Ordinal) && option.Description?.Contains("쪼기", StringComparison.Ordinal) == true), "Expected live-like 새 description to recover 쪼기 from binding compatibility enrichment.");
    Assert(strippedSceneModel.Options.Any(option => string.Equals(option.Label, "뱀", StringComparison.Ordinal) && option.Description?.Contains("미끈거림", StringComparison.Ordinal) == true), "Expected live-like 뱀 description to recover 미끈거림 from binding compatibility enrichment.");
    Assert(strippedSceneModel.Options.Any(option => string.Equals(option.Label, "고리", StringComparison.Ordinal) && option.Description?.Contains("고리형 강인함", StringComparison.Ordinal) == true), "Expected live-like 고리 description to recover 고리형 강인함 from binding compatibility enrichment.");
    Assert(strippedSceneModel.ObserverGaps.Any(gap => gap.Contains("eventOptionDetail missing", StringComparison.OrdinalIgnoreCase)), "Expected live-like scene model to expose missing eventOptionDetail as an observer gap.");
}

static void ExecuteHostSceneModelScenario(HostSceneModelScenario scenario)
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateCompanionHostTestConfiguration(root, collectorModeEnabled: false);
        configuration = configuration with
        {
            Assistant = configuration.Assistant with
            {
                AutoAdviceEnabled = false,
            },
        };
        SeedKnowledgeCatalog(root);

        var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        Directory.CreateDirectory(layout.LiveRoot);
        var session = new LiveExportSession($"session-{scenario.RunId}", scenario.RunId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 1, layout.LiveRoot, scenario.EventKind, scenario.Snapshot.CurrentScreen);
        var events = new[]
        {
            new LiveExportEventEnvelope(DateTimeOffset.UtcNow, 1, scenario.RunId, scenario.EventKind, scenario.Snapshot.CurrentScreen, scenario.Snapshot.Act, scenario.Snapshot.Floor, new Dictionary<string, object?>()),
        };

        WriteJson(layout.SnapshotPath, scenario.Snapshot);
        WriteJson(layout.SessionPath, session);
        File.WriteAllText(layout.SummaryPath, $"{scenario.Name} summary", Encoding.UTF8);
        Directory.CreateDirectory(Path.GetDirectoryName(layout.EventsPath)!);
        File.WriteAllText(layout.EventsPath, string.Join(Environment.NewLine, events.Select(SerializeNdjson)) + Environment.NewLine, Encoding.UTF8);
        File.WriteAllText(layout.RawObservationsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ScreenTransitionsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceCandidatesPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceDecisionsPath, string.Empty, Encoding.UTF8);
        Directory.CreateDirectory(layout.SemanticSnapshotsRoot);

        var host = new CompanionHost(configuration, root, new FakeCodexSessionClient());
        try
        {
            host.RefreshAsync().GetAwaiter().GetResult();
            var sceneModel = host.CurrentSnapshot.LatestSceneModel
                ?? throw new InvalidOperationException($"Expected latest scene model for scenario {scenario.Name}.");
            Assert(string.Equals(sceneModel.SourceKind, "live", StringComparison.Ordinal), $"Expected sourceKind=live for scenario {scenario.Name}.");
            Assert(string.Equals(sceneModel.SceneType, scenario.ExpectedSceneType, StringComparison.Ordinal), $"Expected sceneType {scenario.ExpectedSceneType} for scenario {scenario.Name} but got {sceneModel.SceneType}.");
            Assert(string.Equals(sceneModel.SceneStage, scenario.ExpectedSceneStage, StringComparison.Ordinal), $"Expected sceneStage {scenario.ExpectedSceneStage} for scenario {scenario.Name} but got {sceneModel.SceneStage}.");
            Assert(string.Equals(sceneModel.CanonicalOwner, scenario.ExpectedCanonicalOwner, StringComparison.Ordinal), $"Expected canonicalOwner {scenario.ExpectedCanonicalOwner} for scenario {scenario.Name} but got {sceneModel.CanonicalOwner}.");
            Assert(sceneModel.Options.Count == scenario.ExpectedOptionCount, $"Expected option count {scenario.ExpectedOptionCount} for scenario {scenario.Name} but got {sceneModel.Options.Count}.");
            foreach (var missingFact in scenario.ExpectedMissingFacts)
            {
                Assert(sceneModel.MissingFacts.Contains(missingFact, StringComparer.Ordinal), $"Expected missing fact '{missingFact}' for scenario {scenario.Name}.");
            }

            if (string.Equals(scenario.ExpectedSceneType, "map", StringComparison.OrdinalIgnoreCase)
                && string.Equals(scenario.ExpectedSceneStage, "map-surface-pending", StringComparison.OrdinalIgnoreCase))
            {
                var mapOverlaySurface = sceneModel.UiSurfaceInventory.FirstOrDefault(surface => string.Equals(surface.SurfaceId, "map-overlay", StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidOperationException($"Expected map-overlay UI surface for scenario {scenario.Name}.");
                Assert(!mapOverlaySurface.Present, $"Expected map-overlay UI surface to be absent for scenario {scenario.Name}.");
            }

            Assert(sceneModel.AttemptId is null && sceneModel.StepIndex is null && sceneModel.Phase is null, $"Expected live scene model replay envelope fields to stay null for scenario {scenario.Name}.");
            Assert(sceneModel.CapturedAtUtc is not null, $"Expected live scene model to carry capturedAtUtc for scenario {scenario.Name}.");
            Assert(sceneModel.PublishedAtUtc is not null, $"Expected live scene model to carry publishedAtUtc for scenario {scenario.Name}.");
            Assert(sceneModel.PublishedAtUtc >= sceneModel.CapturedAtUtc, $"Expected publishedAtUtc to be >= capturedAtUtc for scenario {scenario.Name}.");
            Assert(!string.IsNullOrWhiteSpace(host.CurrentSnapshot.Paths.AdvisorSceneRoot) && Directory.Exists(host.CurrentSnapshot.Paths.AdvisorSceneRoot!), $"Expected advisor-scene root for scenario {scenario.Name}.");
            Assert(File.Exists(host.CurrentSnapshot.Paths.AdvisorSceneLatestJsonPath!), $"Expected advisor-scene.latest.json for scenario {scenario.Name}.");
            Assert(File.Exists(host.CurrentSnapshot.Paths.AdvisorSceneLogPath!), $"Expected advisor-scene.ndjson for scenario {scenario.Name}.");

            var latestSceneArtifact = JsonSerializer.Deserialize<AdvisorSceneArtifact>(File.ReadAllText(host.CurrentSnapshot.Paths.AdvisorSceneLatestJsonPath!, Encoding.UTF8), ConfigurationLoader.JsonOptions)
                ?? throw new InvalidOperationException($"Expected advisor scene artifact json for scenario {scenario.Name}.");
            Assert(string.Equals(latestSceneArtifact.SceneType, scenario.ExpectedSceneType, StringComparison.Ordinal), $"Expected persisted sceneType {scenario.ExpectedSceneType} for scenario {scenario.Name}.");
            Assert(latestSceneArtifact.CapturedAtUtc is not null, $"Expected persisted capturedAtUtc for scenario {scenario.Name}.");
            Assert(latestSceneArtifact.PublishedAtUtc is not null, $"Expected persisted publishedAtUtc for scenario {scenario.Name}.");

            var initialLineCount = File.ReadAllLines(host.CurrentSnapshot.Paths.AdvisorSceneLogPath!, Encoding.UTF8).Length;
            Assert(initialLineCount == 1, $"Expected one advisor-scene.ndjson line after first publish for scenario {scenario.Name}.");

            host.RefreshAsync().GetAwaiter().GetResult();
            var secondLineCount = File.ReadAllLines(host.CurrentSnapshot.Paths.AdvisorSceneLogPath!, Encoding.UTF8).Length;
            Assert(secondLineCount == initialLineCount, $"Expected unchanged refresh not to append advisor-scene.ndjson for scenario {scenario.Name}.");
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

static AdvisorSceneArtifact ResolveHostSceneModelForSnapshot(
    string scenarioName,
    string eventKind,
    LiveExportSnapshot snapshot)
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateCompanionHostTestConfiguration(root, collectorModeEnabled: false);
        configuration = configuration with
        {
            Assistant = configuration.Assistant with
            {
                AutoAdviceEnabled = false,
            },
        };
        SeedKnowledgeCatalog(root);

        var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        Directory.CreateDirectory(layout.LiveRoot);
        var session = new LiveExportSession(
            $"session-{snapshot.RunId}",
            snapshot.RunId,
            "active",
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow,
            1,
            layout.LiveRoot,
            eventKind,
            snapshot.CurrentScreen);
        var events = new[]
        {
            new LiveExportEventEnvelope(DateTimeOffset.UtcNow, 1, snapshot.RunId, eventKind, snapshot.CurrentScreen, snapshot.Act, snapshot.Floor, new Dictionary<string, object?>()),
        };

        WriteJson(layout.SnapshotPath, snapshot);
        WriteJson(layout.SessionPath, session);
        File.WriteAllText(layout.SummaryPath, $"{scenarioName} summary", Encoding.UTF8);
        Directory.CreateDirectory(Path.GetDirectoryName(layout.EventsPath)!);
        File.WriteAllText(layout.EventsPath, string.Join(Environment.NewLine, events.Select(SerializeNdjson)) + Environment.NewLine, Encoding.UTF8);
        File.WriteAllText(layout.RawObservationsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ScreenTransitionsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceCandidatesPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceDecisionsPath, string.Empty, Encoding.UTF8);
        Directory.CreateDirectory(layout.SemanticSnapshotsRoot);

        var host = new CompanionHost(configuration, root, new FakeCodexSessionClient());
        try
        {
            host.RefreshAsync().GetAwaiter().GetResult();
            return host.CurrentSnapshot.LatestSceneModel
                ?? throw new InvalidOperationException($"Expected latest scene model for scenario {scenarioName}.");
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

static IReadOnlyList<HostSceneModelScenario> CreateHostSceneModelScenarios()
{
    return new[]
    {
        new HostSceneModelScenario(
            "reward-claim",
            "scene-model-reward-run",
            "reward-screen-opened",
            CreateRewardSceneModelSnapshot("scene-model-reward-run"),
            "reward",
            "claim",
            "reward",
            3,
            Array.Empty<string>()),
        new HostSceneModelScenario(
            "event-ancient-option",
            "scene-model-event-run",
            "event-opened",
            CreateEventSceneModelSnapshot("scene-model-event-run"),
            "event",
            "ancient-option",
            "event",
            2,
            Array.Empty<string>()),
        new HostSceneModelScenario(
            "shop-inventory-open",
            "scene-model-shop-run",
            "shop-opened",
            CreateShopSceneModelSnapshot("scene-model-shop-run"),
            "shop",
            "inventory-open",
            "shop",
            4,
            new[] { "shop-item-price-missing" }),
        new HostSceneModelScenario(
            "map-surface-pending",
            "scene-model-map-pending-run",
            "map-surface-pending",
            CreateMapSurfacePendingSceneModelSnapshot("scene-model-map-pending-run"),
            "map",
            "map-surface-pending",
            "map",
            0,
            new[] { "map-route-context-missing", "map-current-node-identity-missing" }),
        new HostSceneModelScenario(
            "map-provenance-only-stays-pending",
            "scene-model-map-provenance-only-run",
            "map-opened",
            CreateMapProvenanceOnlySceneModelSnapshot("scene-model-map-provenance-only-run"),
            "map",
            "map-surface-pending",
            "map",
            0,
            new[] { "map-route-context-missing", "map-current-node-identity-missing" }),
    };
}

static LiveExportSnapshot CreateCombatSceneModelSnapshot(string runId)
{
    return CreateHostSnapshot(runId, "combat") with
    {
        CurrentScreen = "combat",
        Encounter = new LiveExportEncounterSummary("Jaw Worm", "Combat", true, 2),
        CombatHandCount = 3,
        DrawPileCount = 10,
        DiscardPileCount = 4,
        ExhaustPileCount = 1,
        PlayPileCount = 2,
        EnemyIntentSummary = "Jaw Worm attacks for 11",
        CurrentChoices = new[]
        {
            new LiveExportChoiceSummary("combat-hand-card", "Strike", "CARD.STRIKE_IRONCLAD", "Deal 6 damage.")
            {
                SemanticHints = new[] { "combat-hand-card" },
            },
            new LiveExportChoiceSummary("combat-hand-card", "Defend", "CARD.DEFEND_IRONCLAD", "Gain 5 Block.")
            {
                SemanticHints = new[] { "combat-hand-card" },
            },
            new LiveExportChoiceSummary("card", "Pommel Strike", "pommel-strike", "Deal damage and draw a card.")
            {
                SemanticHints = new[] { "combat-hand-card" },
            },
            new LiveExportChoiceSummary("choice", "DrawPile", "draw-pile", null)
            {
                BindingId = "DrawPile",
                SemanticHints = new[] { "combat-pile-button" },
            },
            new LiveExportChoiceSummary("choice", "DiscardPile", "discard-pile", null)
            {
                BindingId = "DiscardPile",
                SemanticHints = new[] { "combat-pile-button" },
            },
            new LiveExportChoiceSummary("choice", "ExhaustPile", "exhaust-pile", null)
            {
                BindingId = "ExhaustPile",
                SemanticHints = new[] { "combat-pile-button" },
            },
            new LiveExportChoiceSummary("choice", "PlayPile", "play-pile", null)
            {
                BindingId = "PlayPile",
                SemanticHints = new[] { "combat-pile-button" },
            },
            new LiveExportChoiceSummary("choice", "End Turn", "end-turn", null)
            {
                SemanticHints = new[] { "combat-end-turn" },
            },
            new LiveExportChoiceSummary("utility", "Ping", "ping", "Diagnostic ping.")
            {
                SemanticHints = new[] { "utility", "ping" },
            },
        },
        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["choice-source"] = "live-export",
            ["choiceExtractorPath"] = "combat.hand",
            ["currentSceneType"] = "CombatScreen",
            ["rootTypeSummary"] = "CombatScreen",
            ["flowScreen"] = "combat",
            ["visibleScreen"] = "combat",
            ["foregroundOwner"] = "combat",
            ["foregroundActionLane"] = "player-play-open",
            ["combatHandSummary"] = "Strike, Defend, Pommel Strike",
            ["combatTargetCount"] = "1",
            ["combatTargetableEnemyCount"] = "1",
            ["combatHittableEnemyCount"] = "1",
            ["combatTargetingInProgress"] = "false",
            ["combatRoundNumber"] = "3",
            ["combatTargetSummary"] = "Jaw Worm:hittable",
        },
        PublishedCurrentScreen = "combat",
        PublishedVisibleScreen = "combat",
        PublishedSceneAuthority = "hook",
        PublishedSceneStability = "stable",
    };
}

static LiveExportSnapshot CreateCombatPreviewSceneModelSnapshot(string runId)
{
    var baseSnapshot = CreateCombatSceneModelSnapshot(runId);
    return baseSnapshot with
    {
        RecentChanges = new[] { "trigger: combat-preview" },
        Meta = MergeMeta(
            baseSnapshot.Meta,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["combatPreviewVisible"] = "true",
                ["combatPreviewReason"] = "hand-and-pile-count-preview",
            }),
    };
}

static LiveExportSnapshot CreateRewardSceneModelSnapshot(string runId)
{
    return CreateHostSnapshot(runId, "rewards") with
    {
        CurrentScreen = "rewards",
        Encounter = new LiveExportEncounterSummary("RewardsScreen", "Reward", false, null),
        CurrentChoices = new[]
        {
            new LiveExportChoiceSummary("card", "몸통 박치기", "CARD.BASH", "적에게 피해를 주고 취약을 겁니다.")
            {
                BindingKind = "reward-type",
                BindingId = "CardReward",
                SemanticHints = new[] { "reward-card" },
            },
            new LiveExportChoiceSummary("card", "수비 강화", "CARD.SHRUG_IT_OFF", "방어도를 얻고 카드를 뽑습니다.")
            {
                BindingKind = "reward-type",
                BindingId = "CardReward",
                SemanticHints = new[] { "reward-card" },
            },
            new LiveExportChoiceSummary("choice", "넘기기", null, "넘기기"),
        },
        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["choice-source"] = "live-export",
            ["choiceExtractorPath"] = "reward",
            ["currentSceneType"] = "NRewardsScreen",
            ["rootTypeSummary"] = "NRewardsScreen",
            ["flowScreen"] = "rewards",
            ["visibleScreen"] = "rewards",
            ["foregroundOwner"] = "reward",
            ["foregroundActionLane"] = "reward-claim",
            ["rewardProceedVisible"] = "false",
        },
    };
}

static LiveExportSnapshot CreateShopDuplicateVisibleOptionsSceneModelSnapshot(string runId)
{
    var baseSnapshot = CreateShopSceneModelSnapshot(runId);
    return baseSnapshot with
    {
        CurrentChoices = new[]
        {
            new LiveExportChoiceSummary("shop-option:card", "몸풀기", "CARD.WARM_UP", "Merchant inventory slot")
            {
                BindingKind = "shop-option",
                BindingId = "shop-card-0",
                Enabled = true,
                SemanticHints = new[] { "shop", "shop-option:card" },
            },
            new LiveExportChoiceSummary("shop-option:card", "몸풀기", "CARD.WARM_UP_PLUS", "Merchant inventory slot")
            {
                BindingKind = "shop-option",
                BindingId = "shop-card-1",
                Enabled = false,
                SemanticHints = new[] { "shop", "shop-option:card" },
            },
            new LiveExportChoiceSummary("shop-card-removal", "카드 제거", "service:card-removal", "Merchant service")
            {
                BindingKind = "shop-option",
                BindingId = "shop-removal",
                Enabled = true,
                SemanticHints = new[] { "shop", "service" },
            },
            new LiveExportChoiceSummary("shop-back", "뒤로", null, "상점을 닫습니다.")
            {
                BindingKind = "shop-option",
                BindingId = "shop-back",
                Enabled = true,
                SemanticHints = new[] { "shop" },
            },
        },
        RecentChanges = new[] { "trigger: shop-duplicate-visible-options" },
    };
}

static LiveExportSnapshot CreateEventSceneModelSnapshot(string runId)
{
    return CreateHostSnapshot(runId, "event") with
    {
        CurrentScreen = "event",
        Encounter = new LiveExportEncounterSummary("EventRoom", "Event", false, null),
        CurrentChoices = new[]
        {
            new LiveExportChoiceSummary("event-option", "해독한다", "option-0", "최대 체력을 3 잃고 무작위 카드를 1장 강화합니다.")
            {
                BindingKind = "event-option",
                BindingId = "option-0",
                SemanticHints = new[] { "source:event-option-button" },
            },
            new LiveExportChoiceSummary("event-option", "부순다", "option-1", "체력을 20 회복합니다.")
            {
                BindingKind = "event-option",
                BindingId = "option-1",
                SemanticHints = new[] { "source:event-option-button" },
            },
        },
        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["choice-source"] = "live-export",
            ["choiceExtractorPath"] = "event",
            ["currentSceneType"] = "NEventRoom",
            ["rootTypeSummary"] = "NEventRoom",
            ["flowScreen"] = "event",
            ["visibleScreen"] = "event",
            ["foregroundOwner"] = "event",
            ["foregroundActionLane"] = "ancient-option",
            ["ancientPhase"] = "options",
            ["ancientEventDetected"] = "true",
        },
    };
}

static IReadOnlyList<LiveExportEventEnvelope> CreateEventEvents(string runId)
{
    return new[]
    {
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow.AddSeconds(-1),
            1,
            runId,
            "event-opened",
            "event",
            1,
            3,
            new Dictionary<string, object?>()),
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow,
            2,
            runId,
            "choice-list-presented",
            "event",
            1,
            3,
            new Dictionary<string, object?>
            {
                ["choices"] = new[] { "해독한다", "부순다" },
            }),
    };
}

static IReadOnlyList<LiveExportEventEnvelope> CreateShopEvents(string runId, IReadOnlyList<LiveExportChoiceSummary> choices)
{
    return new[]
    {
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow.AddSeconds(-1),
            1,
            runId,
            "shop-opened",
            "shop",
            1,
            3,
            new Dictionary<string, object?>()),
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow,
            2,
            runId,
            "choice-list-presented",
            "shop",
            1,
            3,
            new Dictionary<string, object?>
            {
                ["choices"] = choices.Select(choice => choice.Label).ToArray(),
            }),
    };
}

static CompanionRunState CreateEventRunState(string runId, IReadOnlyList<LiveExportChoiceSummary>? choices = null)
{
    var snapshot = CreateEventSceneModelSnapshot(runId) with
    {
        CurrentChoices = choices ?? CreateEventSceneModelSnapshot(runId).CurrentChoices,
    };
    var session = new LiveExportSession("event-session", runId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(Path.GetTempPath(), "event-live"), "choice-list-presented", "event");
    var events = CreateEventEvents(runId);
    return new CompanionRunState(snapshot, session, "event summary", events, false)
    {
        NormalizedState = CompanionStateMapper.FromLiveExport(snapshot, session, events),
    };
}

static CompanionRunState CreateNeowEventRunState(string runId)
{
    var snapshot = CreateHostSnapshot(runId, "event") with
    {
        CurrentScreen = "event",
        Encounter = new LiveExportEncounterSummary("root", "Event", false, null),
        CurrentChoices = new[]
        {
            new LiveExportChoiceSummary("event-option", "납 문진", "0", "무색 카드 [blue]2[/blue]장 중 [blue]1[/blue]장을 선택해 [gold]덱[/gold]에 추가합니다.")
            {
                BindingKind = "event-option",
                BindingId = "ancient-event-option:0",
                Enabled = true,
                SemanticHints = new[] { "scene:event", "ancient-event", "source:ancient-option-button", "option-index:0" },
                EventOptionDetail = new LiveExportEventOptionDetail(
                    "NEOW.pages.INITIAL.options.LEAD_PAPERWEIGHT",
                    "ancient-event-option:0",
                    "납 문진",
                    "무색 카드 2장 중 1장을 선택해 덱에 추가합니다.",
                    null,
                    null,
                    null,
                    null,
                    new LiveExportModelSummary("relic", "LeadPaperweight", "납 문진", null),
                    null,
                    null,
                    null,
                    null),
            },
            new LiveExportChoiceSummary("event-option", "니오우의 비탄", "1", "[gold]덱[/gold]에 [gold]니오우의 격분[/gold]을 [blue]1[/blue]장 추가합니다.")
            {
                BindingKind = "event-option",
                BindingId = "ancient-event-option:1",
                Enabled = true,
                SemanticHints = new[] { "scene:event", "ancient-event", "source:ancient-option-button", "option-index:1" },
                EventOptionDetail = new LiveExportEventOptionDetail(
                    "NEOW.pages.INITIAL.options.NEOWS_TORMENT",
                    "ancient-event-option:1",
                    "니오우의 비탄",
                    "덱에 니오우의 격분을 1장 추가합니다.",
                    null,
                    null,
                    null,
                    new LiveExportModelSummary("card", "NeowsFury", "니오우의 격분", null),
                    new LiveExportModelSummary("relic", "NeowsTorment", "니오우의 비탄", null),
                    null,
                    null,
                    null,
                    null),
            },
            new LiveExportChoiceSummary("event-option", "은 도가니", "2", "처음 [blue]{Cards}[/blue]번의 카드 보상이 [gold]강화[/gold]된 상태로 등장합니다. 처음으로 여는 보물 상자가 [red]비어 있습니다[/red].")
            {
                BindingKind = "event-option",
                BindingId = "ancient-event-option:2",
                Enabled = true,
                SemanticHints = new[] { "scene:event", "ancient-event", "source:ancient-option-button", "option-index:2" },
                EventOptionDetail = new LiveExportEventOptionDetail(
                    "NEOW.pages.INITIAL.options.SILVER_CRUCIBLE",
                    "ancient-event-option:2",
                    "은 도가니",
                    "처음 3번의 카드 보상이 강화된 상태로 등장합니다. 처음으로 여는 보물 상자가 비어 있습니다.",
                    null,
                    null,
                    null,
                    null,
                    new LiveExportModelSummary("relic", "SilverCrucible", "은 도가니", null),
                    null,
                    null,
                    null,
                    null),
            },
        },
        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["choice-source"] = "live-export",
            ["choiceExtractorPath"] = "event",
            ["currentSceneType"] = "NEventRoom",
            ["rootTypeSummary"] = "NEventRoom",
            ["flowScreen"] = "event",
            ["visibleScreen"] = "event",
            ["foregroundOwner"] = "event",
            ["foregroundActionLane"] = "ancient-option",
            ["ancientPhase"] = "options",
            ["ancientEventDetected"] = "true",
        },
    };
    var session = new LiveExportSession("event-session", runId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(Path.GetTempPath(), "event-live"), "choice-list-presented", "event");
    var events = new[]
    {
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow.AddSeconds(-1),
            1,
            runId,
            "event-opened",
            "event",
            null,
            null,
            new Dictionary<string, object?>()),
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow,
            2,
            runId,
            "choice-list-presented",
            "event",
            null,
            null,
            new Dictionary<string, object?>
            {
                ["choices"] = new[] { "납 문진", "니오우의 비탄", "은 도가니" },
            }),
    };
    return new CompanionRunState(snapshot, session, "neow summary", events, false)
    {
        NormalizedState = CompanionStateMapper.FromLiveExport(snapshot, session, events),
    };
}

static CompanionRunState CreateWoodCarvingsEventRunState(string runId)
{
    var snapshot = CreateHostSnapshot(runId, "event") with
    {
        CurrentScreen = "event",
        Encounter = new LiveExportEncounterSummary("root", "Event", false, null),
        CurrentChoices = new[]
        {
            new LiveExportChoiceSummary("event-option", "새", "새", "시작 카드를 1장 선택해 로 변화시킵니다.")
            {
                BindingKind = "event-option",
                BindingId = "WOOD_CARVINGS.pages.INITIAL.options.BIRD",
                Enabled = true,
                SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option-button", "option-role:choice" },
                EventOptionDetail = new LiveExportEventOptionDetail(
                    "WOOD_CARVINGS.pages.INITIAL.options.BIRD",
                    "WOOD_CARVINGS.pages.INITIAL.options.BIRD",
                    "새",
                    "시작 카드를 1장 선택해 쪼기로 변화시킵니다.",
                    null,
                    null,
                    null,
                    new LiveExportModelSummary("card", "Peck", "쪼기", null),
                    null,
                    null,
                    null,
                    null,
                    null),
            },
            new LiveExportChoiceSummary("event-option", "새", null, "시작 카드를 1장 선택해 로 변화시킵니다.")
            {
                BindingKind = "event-option",
                BindingId = "WOOD_CARVINGS.pages.INITIAL.options.BIRD",
                Enabled = true,
                SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option", "option-role:choice" },
                EventOptionDetail = new LiveExportEventOptionDetail(
                    "WOOD_CARVINGS.pages.INITIAL.options.BIRD",
                    "WOOD_CARVINGS.pages.INITIAL.options.BIRD",
                    "새",
                    "시작 카드를 1장 선택해 쪼기로 변화시킵니다.",
                    null,
                    null,
                    null,
                    new LiveExportModelSummary("card", "Peck", "쪼기", null),
                    null,
                    null,
                    null,
                    null,
                    null),
            },
            new LiveExportChoiceSummary("event-option", "뱀", "뱀", "카드 1장에 을 인챈트합니다.")
            {
                BindingKind = "event-option",
                BindingId = "WOOD_CARVINGS.pages.INITIAL.options.SNAKE",
                Enabled = true,
                SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option-button", "option-role:choice" },
                EventOptionDetail = new LiveExportEventOptionDetail(
                    "WOOD_CARVINGS.pages.INITIAL.options.SNAKE",
                    "WOOD_CARVINGS.pages.INITIAL.options.SNAKE",
                    "뱀",
                    "카드 1장에 미끈거림을 인챈트합니다.",
                    "미끈거림",
                    "미끈거림은 해당 카드를 뽑았을 때 이번 전투 동안 비용을 무작위 0~3으로 바꿉니다.",
                    "Slither",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null),
            },
            new LiveExportChoiceSummary("event-option", "뱀", null, "카드 1장에 을 인챈트합니다.")
            {
                BindingKind = "event-option",
                BindingId = "WOOD_CARVINGS.pages.INITIAL.options.SNAKE",
                Enabled = true,
                SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option", "option-role:choice" },
                EventOptionDetail = new LiveExportEventOptionDetail(
                    "WOOD_CARVINGS.pages.INITIAL.options.SNAKE",
                    "WOOD_CARVINGS.pages.INITIAL.options.SNAKE",
                    "뱀",
                    "카드 1장에 미끈거림을 인챈트합니다.",
                    "미끈거림",
                    "미끈거림은 해당 카드를 뽑았을 때 이번 전투 동안 비용을 무작위 0~3으로 바꿉니다.",
                    "Slither",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null),
            },
            new LiveExportChoiceSummary("event-option", "고리", "고리", "시작 카드를 1장 선택해 으로 변화시킵니다.")
            {
                BindingKind = "event-option",
                BindingId = "WOOD_CARVINGS.pages.INITIAL.options.TORUS",
                Enabled = true,
                SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option-button", "option-role:choice" },
                EventOptionDetail = new LiveExportEventOptionDetail(
                    "WOOD_CARVINGS.pages.INITIAL.options.TORUS",
                    "WOOD_CARVINGS.pages.INITIAL.options.TORUS",
                    "고리",
                    "시작 카드를 1장 선택해 고리형 강인함으로 변화시킵니다.",
                    null,
                    null,
                    null,
                    new LiveExportModelSummary("card", "ToricToughness", "고리형 강인함", null),
                    null,
                    null,
                    null,
                    null,
                    null),
            },
            new LiveExportChoiceSummary("event-option", "고리", null, "시작 카드를 1장 선택해 으로 변화시킵니다.")
            {
                BindingKind = "event-option",
                BindingId = "WOOD_CARVINGS.pages.INITIAL.options.TORUS",
                Enabled = true,
                SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option", "option-role:choice" },
                EventOptionDetail = new LiveExportEventOptionDetail(
                    "WOOD_CARVINGS.pages.INITIAL.options.TORUS",
                    "WOOD_CARVINGS.pages.INITIAL.options.TORUS",
                    "고리",
                    "시작 카드를 1장 선택해 고리형 강인함으로 변화시킵니다.",
                    null,
                    null,
                    null,
                    new LiveExportModelSummary("card", "ToricToughness", "고리형 강인함", null),
                    null,
                    null,
                    null,
                    null,
                    null),
            },
        },
        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["choice-source"] = "live-export",
            ["choiceExtractorPath"] = "event",
            ["currentSceneType"] = "NEventRoom",
            ["rootTypeSummary"] = "NEventRoom",
            ["flowScreen"] = "event",
            ["visibleScreen"] = "event",
            ["foregroundOwner"] = "event",
            ["foregroundActionLane"] = "event-option",
        },
    };
    var session = new LiveExportSession("event-session", runId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(Path.GetTempPath(), "event-live"), "choice-list-presented", "event");
    var events = new[]
    {
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow.AddSeconds(-1),
            1,
            runId,
            "event-opened",
            "event",
            null,
            null,
            new Dictionary<string, object?>()),
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow,
            2,
            runId,
            "choice-list-presented",
            "event",
            null,
            null,
            new Dictionary<string, object?>
            {
                ["choices"] = new[] { "새", "뱀", "고리" },
            }),
    };
    return new CompanionRunState(snapshot, session, "wood carvings summary", events, false)
    {
        NormalizedState = CompanionStateMapper.FromLiveExport(snapshot, session, events),
    };
}

static LiveExportSnapshot CreateWoodCarvingsEventSnapshotWithoutTypedDetails(string runId)
{
    var runState = CreateWoodCarvingsEventRunState(runId);
    return runState.Snapshot with
    {
        RunId = runId,
        CurrentChoices = runState.Snapshot.CurrentChoices
            .Select(choice => choice with
            {
                EventOptionDetail = null,
            })
            .ToArray(),
    };
}

static CompanionRunState CreateShopRunState(string runId, LiveExportSnapshot? snapshot = null)
{
    var resolvedSnapshot = snapshot ?? CreateShopSceneModelSnapshot(runId);
    var session = new LiveExportSession("shop-session", runId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(Path.GetTempPath(), "shop-live"), "choice-list-presented", "shop");
    var events = CreateShopEvents(runId, resolvedSnapshot.CurrentChoices);
    return new CompanionRunState(resolvedSnapshot, session, "shop summary", events, false)
    {
        NormalizedState = CompanionStateMapper.FromLiveExport(resolvedSnapshot, session, events),
    };
}

static LiveExportSnapshot CreateShopSceneModelSnapshot(string runId)
{
    return CreateHostSnapshot(runId, "shop") with
    {
        CurrentScreen = "shop",
        Encounter = new LiveExportEncounterSummary("MerchantRoom", "Shop", false, null),
        CurrentChoices = new[]
        {
            new LiveExportChoiceSummary("shop-option:relic", "게임용 말", "RELIC.GAME_PIECE", "Merchant inventory slot")
            {
                BindingKind = "shop-option",
                BindingId = "shop-relic-0",
                Enabled = true,
                SemanticHints = new[] { "shop", "shop-option:relic" },
            },
            new LiveExportChoiceSummary("shop-option:card", "몸풀기", "CARD.WARM_UP", "Merchant inventory slot")
            {
                BindingKind = "shop-option",
                BindingId = "shop-card-0",
                Enabled = false,
                SemanticHints = new[] { "shop", "shop-option:card" },
            },
            new LiveExportChoiceSummary("shop-card-removal", "카드 제거", "service:card-removal", "Merchant service")
            {
                BindingKind = "shop-option",
                BindingId = "shop-removal",
                Enabled = true,
                SemanticHints = new[] { "shop", "service" },
            },
            new LiveExportChoiceSummary("shop-back", "뒤로", null, "상점을 닫습니다.")
            {
                BindingKind = "shop-option",
                BindingId = "shop-back",
                Enabled = true,
                SemanticHints = new[] { "shop" },
            },
        },
        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["choice-source"] = "live-export",
            ["choiceExtractorPath"] = "shop",
            ["currentSceneType"] = "NMerchantRoom",
            ["rootTypeSummary"] = "NMerchantRoom",
            ["flowScreen"] = "shop",
            ["visibleScreen"] = "shop",
            ["foregroundOwner"] = "shop",
            ["foregroundActionLane"] = "shop-inventory",
            ["shopInventoryOpen"] = "true",
            ["shopAffordableOptionCount"] = "2",
            ["shopCardRemovalVisible"] = "true",
            ["shopCardRemovalEnabled"] = "true",
            ["shopCardRemovalUsed"] = "false",
            ["shopBackEnabled"] = "true",
            ["shopProceedEnabled"] = "false",
        },
    };
}

static CompanionRunState CreateCombatRunState(string runId, LiveExportSnapshot? snapshot = null)
{
    var resolvedSnapshot = snapshot ?? CreateCombatSceneModelSnapshot(runId);
    var session = new LiveExportSession("combat-session", runId, "active", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 2, Path.Combine(Path.GetTempPath(), "combat-live"), "combat-started", "combat");
    var events = new[]
    {
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow.AddSeconds(-1),
            1,
            runId,
            "combat-started",
            "combat",
            resolvedSnapshot.Act,
            resolvedSnapshot.Floor,
            new Dictionary<string, object?>()),
        new LiveExportEventEnvelope(
            DateTimeOffset.UtcNow,
            2,
            runId,
            "choice-list-presented",
            "combat",
            resolvedSnapshot.Act,
            resolvedSnapshot.Floor,
            new Dictionary<string, object?>
            {
                ["choices"] = resolvedSnapshot.CurrentChoices.Select(choice => choice.Label).ToArray(),
            }),
    };
    return new CompanionRunState(resolvedSnapshot, session, "combat summary", events, false)
    {
        NormalizedState = CompanionStateMapper.FromLiveExport(resolvedSnapshot, session, events),
    };
}

static LiveExportSnapshot CreateMapSurfacePendingSceneModelSnapshot(string runId)
{
    return CreateHostSnapshot(runId, "map") with
    {
        CurrentScreen = "map",
        Encounter = new LiveExportEncounterSummary("MapRoom", "Map", false, null),
        CurrentChoices = Array.Empty<LiveExportChoiceSummary>(),
        RecentChanges = new[] { "trigger: map-surface-pending" },
        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["choice-source"] = "live-export",
            ["choiceExtractorPath"] = "map",
            ["currentSceneType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
            ["rootTypeSummary"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
            ["flowScreen"] = "map",
            ["visibleScreen"] = "map",
            ["foregroundOwner"] = "map",
            ["mapReleaseAuthority"] = "true",
            ["mapSurfacePending"] = "true",
        },
    };
}

static LiveExportSnapshot CreateMapProvenanceOnlySceneModelSnapshot(string runId)
{
    return CreateHostSnapshot(runId, "map") with
    {
        CurrentScreen = "map",
        PublishedCurrentScreen = "map",
        PublishedVisibleScreen = "map",
        PublishedSceneAuthority = "polling",
        PublishedSceneStability = "stable",
        Encounter = new LiveExportEncounterSummary("Map", "Transition", false, null),
        CurrentChoices = Array.Empty<LiveExportChoiceSummary>(),
        Meta = MergeMeta(
            CreateHostSnapshot(runId, "map").Meta,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["foregroundOwner"] = "map",
            }),
    };
}

static LiveExportSnapshot CreateScreenProvenanceSnapshot(string runId, string screen)
{
    return CreateHostSnapshot(runId, screen) with
    {
        CurrentScreen = screen,
        Encounter = new LiveExportEncounterSummary(screen, screen, false, null),
        CurrentChoices = Array.Empty<LiveExportChoiceSummary>(),
        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["choice-source"] = "live-export",
            ["currentSceneType"] = "MegaCrit.Sts2.Core.Nodes.NGame",
            ["rootTypeSummary"] = "MegaCrit.Sts2.Core.Nodes.NGame",
            ["flowScreen"] = screen,
            ["visibleScreen"] = screen,
        },
    };
}

static HarnessNodeInventory CreateScreenProvenanceInventory(
    string inventoryId,
    string runId,
    string sceneType,
    string? publishedCurrentScreen,
    string? publishedVisibleScreen,
    string? publishedSceneAuthority = "published",
    string? publishedSceneStability = "stable")
{
    return new HarnessNodeInventory(
        inventoryId,
        DateTimeOffset.UtcNow,
        runId,
        sceneType,
        $"{sceneType}-episode",
        "dormant",
        null,
        true,
        publishedSceneAuthority,
        publishedSceneStability,
        Array.Empty<HarnessNodeInventoryItem>())
    {
        PublishedCurrentScreen = publishedCurrentScreen,
        PublishedVisibleScreen = publishedVisibleScreen,
        PublishedSceneReady = true,
        PublishedSceneAuthority = publishedSceneAuthority,
        PublishedSceneStability = publishedSceneStability,
    };
}

static IReadOnlyList<ScreenProvenanceParityScenario> CreateScreenProvenanceParityScenarios()
{
    var combatSnapshot = CreateScreenProvenanceSnapshot("parity-combat-run", "combat") with
    {
        Encounter = new LiveExportEncounterSummary("Jaw Worm", "Combat", true, 2),
        PublishedCurrentScreen = "map",
        PublishedVisibleScreen = "map",
        PublishedSceneAuthority = "polling",
        PublishedSceneStability = "stable",
        Meta = MergeMeta(
            CreateScreenProvenanceSnapshot("parity-combat-run", "combat").Meta,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["foregroundOwner"] = "map",
                ["combatHandSummary"] = "Strike, Defend",
            }),
    };

    var eventSnapshot = CreateEventSceneModelSnapshot("parity-event-run") with
    {
        PublishedCurrentScreen = "event",
        PublishedVisibleScreen = "event",
        PublishedSceneAuthority = "hook",
        PublishedSceneStability = "stable",
        RawObservedScreen = "map",
    };

    var mapSnapshot = CreateScreenProvenanceSnapshot("parity-map-run", "map") with
    {
        PublishedCurrentScreen = "map",
        PublishedVisibleScreen = "map",
        PublishedSceneAuthority = "polling",
        PublishedSceneStability = "stable",
        CurrentChoices = new[]
        {
            new LiveExportChoiceSummary("map-node", "휴식 (1,2)", "1,2", "type:Rest;coord:1,2"),
        },
        Meta = MergeMeta(
            CreateScreenProvenanceSnapshot("parity-map-run", "map").Meta,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["mapPointCount"] = "1",
                ["mapCurrentNodeArrowVisible"] = "true",
            }),
    };

    var rewardSnapshot = CreateRewardSceneModelSnapshot("parity-reward-run") with
    {
        PublishedCurrentScreen = "rewards",
        PublishedVisibleScreen = "rewards",
        PublishedSceneAuthority = "hook",
        PublishedSceneStability = "stable",
    };

    return new[]
    {
        new ScreenProvenanceParityScenario(
            "combat with stale map foreground owner",
            "combat-started",
            combatSnapshot,
            CreateScreenProvenanceInventory("combat-stale-map", combatSnapshot.RunId, "map", "map", "map", "inventory", "stale"),
            "combat",
            "combat",
            "combat",
            "combat"),
        new ScreenProvenanceParityScenario(
            "event screen with explicit published event screen",
            "event-opened",
            eventSnapshot,
            CreateScreenProvenanceInventory("event-published", eventSnapshot.RunId, "event", "event", "event", "hook", "stable"),
            "event",
            "event",
            "event",
            "event"),
        new ScreenProvenanceParityScenario(
            "map overlay with reachable nodes",
            "map-opened",
            mapSnapshot,
            CreateScreenProvenanceInventory("map-overlay", mapSnapshot.RunId, "map", "map", "map", "polling", "stable"),
            "map",
            "map",
            "map",
            "map"),
        new ScreenProvenanceParityScenario(
            "reward aftermath with published reward proceed state",
            "reward-screen-opened",
            rewardSnapshot,
            CreateScreenProvenanceInventory("reward-aftermath", rewardSnapshot.RunId, "reward", "rewards", "rewards", "hook", "stable"),
            "rewards",
            "rewards",
            "reward",
            "reward"),
    };
}

static AdvisorSceneArtifact ResolveHostSceneModelForParityScenario(ScreenProvenanceParityScenario scenario)
{
    var root = CreateTempDirectory();
    try
    {
        var configuration = CreateCompanionHostTestConfiguration(root, collectorModeEnabled: false);
        configuration = configuration with
        {
            Assistant = configuration.Assistant with
            {
                AutoAdviceEnabled = false,
            },
        };
        SeedKnowledgeCatalog(root);

        var layout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        Directory.CreateDirectory(layout.LiveRoot);
        var session = new LiveExportSession(
            $"session-{scenario.Snapshot.RunId}",
            scenario.Snapshot.RunId,
            "active",
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow,
            1,
            layout.LiveRoot,
            scenario.EventKind,
            scenario.Snapshot.CurrentScreen);
        var events = new[]
        {
            new LiveExportEventEnvelope(DateTimeOffset.UtcNow, 1, scenario.Snapshot.RunId, scenario.EventKind, scenario.Snapshot.CurrentScreen, scenario.Snapshot.Act, scenario.Snapshot.Floor, new Dictionary<string, object?>()),
        };

        WriteJson(layout.SnapshotPath, scenario.Snapshot);
        WriteJson(layout.SessionPath, session);
        File.WriteAllText(layout.SummaryPath, $"{scenario.Name} summary", Encoding.UTF8);
        Directory.CreateDirectory(Path.GetDirectoryName(layout.EventsPath)!);
        File.WriteAllText(layout.EventsPath, string.Join(Environment.NewLine, events.Select(SerializeNdjson)) + Environment.NewLine, Encoding.UTF8);
        File.WriteAllText(layout.RawObservationsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ScreenTransitionsPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceCandidatesPath, string.Empty, Encoding.UTF8);
        File.WriteAllText(layout.ChoiceDecisionsPath, string.Empty, Encoding.UTF8);
        Directory.CreateDirectory(layout.SemanticSnapshotsRoot);

        var host = new CompanionHost(configuration, root, new FakeCodexSessionClient());
        try
        {
            host.RefreshAsync().GetAwaiter().GetResult();
            return host.CurrentSnapshot.LatestSceneModel
                ?? throw new InvalidOperationException($"Expected latest scene model for scenario {scenario.Name}.");
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

static Dictionary<string, string?> MergeMeta(
    IReadOnlyDictionary<string, string?> baseline,
    IReadOnlyDictionary<string, string?> overlay)
{
    var merged = new Dictionary<string, string?>(baseline, StringComparer.OrdinalIgnoreCase);
    foreach (var pair in overlay)
    {
        merged[pair.Key] = pair.Value;
    }

    return merged;
}

static ScreenProvenanceInput CreateHarnessProvenanceInput(
    LiveExportSnapshot? snapshot,
    HarnessNodeInventory? inventory)
{
    using var stateDocument = snapshot is null
        ? null
        : JsonDocument.Parse(JsonSerializer.Serialize(snapshot, ConfigurationLoader.JsonOptions));
    using var inventoryDocument = inventory is null
        ? null
        : JsonDocument.Parse(JsonSerializer.Serialize(inventory, ConfigurationLoader.JsonOptions));
    return ScreenProvenanceResolver.CreateFromObserverDocuments(stateDocument, inventoryDocument);
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

static void WriteReplayFixture(
    string fixtureRoot,
    ScaffoldConfiguration configuration,
    LiveExportSnapshot snapshot,
    LiveExportSession session,
    IReadOnlyList<LiveExportEventEnvelope> events,
    string summaryText)
{
    var liveMirrorRoot = Path.Combine(fixtureRoot, "live-mirror");
    Directory.CreateDirectory(liveMirrorRoot);
    WriteJson(Path.Combine(liveMirrorRoot, configuration.LiveExport.SnapshotFileName), snapshot);
    WriteJson(Path.Combine(liveMirrorRoot, configuration.LiveExport.SessionFileName), session);
    File.WriteAllText(Path.Combine(liveMirrorRoot, configuration.LiveExport.SummaryFileName), summaryText, Encoding.UTF8);
    File.WriteAllText(Path.Combine(liveMirrorRoot, configuration.LiveExport.EventsFileName), string.Join(Environment.NewLine, events.Select(SerializeNdjson)) + Environment.NewLine, Encoding.UTF8);
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

static void SeedEventKnowledgeNoiseCatalog(string root)
{
    var knowledgeRoot = Path.Combine(root, "artifacts", "knowledge");
    Directory.CreateDirectory(knowledgeRoot);
    var catalog = new StaticKnowledgeCatalog(
        DateTimeOffset.UtcNow,
        new StaticKnowledgeMetadata("event-knowledge-v1", "event-knowledge-test", DateTimeOffset.UtcNow, new Dictionary<string, string?>()),
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>(),
        new[]
        {
            CreateEventKnowledgeEntry(
                "megacrit-sts2-core-models-events-tabletoftruth",
                "진리의 석판",
                "TabletofTruth",
                "TABLETOFTRUTH",
                "당신은 오래된 석판을 발견합니다. 새겨진 새 모양 문양이 어렴풋이 보입니다."),
            CreateEventKnowledgeEntry(
                "megacrit-sts2-core-models-events-woodcarvings",
                "나무 조각",
                "WoodCarvings",
                "WOOD_CARVINGS",
                "당신은 먼지가 쌓인 상자를 열고 정교한 나무 조각 3개를 발견했습니다. 새, 뱀, 그리고... 고리?"),
            CreateEventKnowledgeEntry(
                "megacrit-sts2-core-models-events-hungryformushrooms",
                "버섯이 먹고 싶어",
                "HungryForMushrooms",
                "HUNGRY_FOR_MUSHROOMS",
                "낯선 버섯 옆에는 뱀 모양 표식이 새겨져 있습니다."),
            CreateEventKnowledgeEntry(
                "megacrit-sts2-core-models-events-doorsoflightanddark",
                "빛과 어둠의 문",
                "DoorsOfLightAndDark",
                "DOORS_OF_LIGHT_AND_DARK",
                "문 손잡이에는 고리 장식과 함께 빛나는 문양이 보입니다."),
            CreateEventKnowledgeEntry(
                "megacrit-sts2-core-models-events-fieldofmansizedholes",
                "사람 크기 구멍의 들판",
                "FieldOfManSizedHoles",
                "FIELD_OF_MAN_SIZED_HOLES",
                "구덩이 주변에는 새 깃털이 흩어져 있습니다."),
            CreateEventKnowledgeEntry(
                "megacrit-sts2-core-models-events-whisperinghollow",
                "속삭이는 골짜기",
                "WhisperingHollow",
                "WHISPERING_HOLLOW",
                "속삭이는 나무 아래에서 뱀과 고리 모양 장식이 바람에 흔들립니다."),
        },
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>());
    File.WriteAllText(Path.Combine(knowledgeRoot, "catalog.latest.json"), JsonSerializer.Serialize(catalog, ConfigurationLoader.JsonOptions), Encoding.UTF8);
}

static StaticKnowledgeEntry CreateEventKnowledgeEntry(
    string id,
    string name,
    string className,
    string classId,
    string rawText)
{
    return new StaticKnowledgeEntry(
        id,
        name,
        "localization-scan",
        false,
        rawText,
        new[] { "l10n", "localized-event", "strict-event", "strict-model" },
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["fullName"] = $"MegaCrit.Sts2.Core.Models.Events.{className}",
            ["className"] = className,
            ["strictDomain"] = "event",
            ["strictModel"] = "true",
            ["classId"] = classId,
            ["l10nKey"] = classId,
            ["title"] = name,
        },
        Array.Empty<StaticKnowledgeOption>());
}

static AdvisorKnowledgeDisplayResolver CreateDisplayKnowledgeResolver()
{
    var catalog = new StaticKnowledgeCatalog(
        DateTimeOffset.UtcNow,
        new StaticKnowledgeMetadata("display-v1", "display-test", DateTimeOffset.UtcNow, new Dictionary<string, string?>()),
        new[]
        {
            new StaticKnowledgeEntry(
                "pommel-strike",
                "몸통 박치기",
                "localization-scan",
                true,
                "적에게 피해를 주고 카드를 1장 뽑습니다.",
                new[] { "card", "attack" },
                new Dictionary<string, string?>
                {
                    ["englishTitle"] = "Pommel Strike",
                    ["classId"] = "POMMEL_STRIKE",
                    ["title"] = "몸통 박치기",
                    ["description"] = "적에게 피해를 주고 카드를 1장 뽑습니다.",
                },
                Array.Empty<StaticKnowledgeOption>()),
            new StaticKnowledgeEntry(
                "strike-ironclad",
                "타격",
                "localization-scan",
                true,
                "적에게 피해를 줍니다.",
                new[] { "card", "attack" },
                new Dictionary<string, string?>
                {
                    ["classId"] = "STRIKE_IRONCLAD",
                    ["title"] = "타격",
                    ["description"] = "적에게 피해를 줍니다.",
                },
                Array.Empty<StaticKnowledgeOption>()),
            new StaticKnowledgeEntry(
                "defend-ironclad",
                "수비",
                "localization-scan",
                true,
                "방어도를 얻습니다.",
                new[] { "card", "skill" },
                new Dictionary<string, string?>
                {
                    ["classId"] = "DEFEND_IRONCLAD",
                    ["title"] = "수비",
                    ["description"] = "방어도를 얻습니다.",
                },
                Array.Empty<StaticKnowledgeOption>()),
            new StaticKnowledgeEntry(
                "peck",
                "쪼기",
                "strict-domain-scan",
                true,
                "피해 2를 3번 줍니다.",
                new[] { "card", "attack" },
                new Dictionary<string, string?>
                {
                    ["classId"] = "Peck",
                    ["title"] = "쪼기",
                    ["description"] = "피해 2를 3번 줍니다.",
                },
                Array.Empty<StaticKnowledgeOption>()),
            new StaticKnowledgeEntry(
                "slither",
                "미끈거림",
                "strict-domain-scan",
                true,
                "해당 카드를 뽑았을 때 이번 전투 동안 비용을 무작위 0~3으로 바꿉니다.",
                new[] { "enchantment" },
                new Dictionary<string, string?>
                {
                    ["classId"] = "Slither",
                    ["title"] = "미끈거림",
                    ["description"] = "해당 카드를 뽑았을 때 이번 전투 동안 비용을 무작위 0~3으로 바꿉니다.",
                },
                Array.Empty<StaticKnowledgeOption>()),
            new StaticKnowledgeEntry(
                "toric-toughness",
                "고리형 강인함",
                "strict-domain-scan",
                true,
                "방어도 5를 얻고 다음 2턴 동안 턴 시작 시 방어도 5를 얻습니다.",
                new[] { "card", "skill" },
                new Dictionary<string, string?>
                {
                    ["classId"] = "ToricToughness",
                    ["title"] = "고리형 강인함",
                    ["description"] = "방어도 5를 얻고 다음 2턴 동안 턴 시작 시 방어도 5를 얻습니다.",
                },
                Array.Empty<StaticKnowledgeOption>()),
        },
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>(),
        Array.Empty<StaticKnowledgeEntry>());
    return new AdvisorKnowledgeDisplayResolver(catalog);
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
        inputPack.RewardRecommendationTraceSeed,
        inputPack.CompactInput);
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
        inputPack.RewardRecommendationTraceSeed,
        inputPack.CompactInput);
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

static void WaitForCondition(Func<bool> predicate, TimeSpan timeout, string message)
{
    var deadline = DateTime.UtcNow + timeout;
    while (DateTime.UtcNow < deadline)
    {
        if (predicate())
        {
            return;
        }

        Thread.Sleep(25);
    }

    throw new InvalidOperationException(message);
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
        var model = new FakeRelicModel
        {
            Id = id,
            Name = label,
            Description = "턴 종료 시 {InCombat:Block} 얻습니다.",
            HoverTips = new object[]
            {
                new FakeHoverTip
                {
                    Id = id,
                    Title = label,
                    Description = "턴 종료 시 방어도를 8 얻습니다.",
                },
            },
        };
        Entry = new FakeMerchantRelicEntry
        {
            Id = id,
            Name = label,
            IsStocked = stocked,
            EnoughGold = enoughGold,
            Model = model,
        };
    }

    public FakeMerchantRelicEntry Entry { get; }
}

file sealed class FakeNMerchantCard : FakeMerchantSlotBase
{
    public FakeNMerchantCard(string label, string id, double x, double y, bool enabled, bool stocked, bool enoughGold)
        : base(label, id, x, y, enabled)
    {
        var card = new FakeCardModel
        {
            Id = id,
            Name = label,
            Description = "적에게 {CalculatedDamage:diff()} 피해를 주고 {IfUpgraded:NL}카드를 1장 뽑습니다.",
            EvaluatedDescription = "[center]적에게 12 피해를 주고 카드를 1장 뽑습니다.[/center]",
        };
        Entry = new FakeMerchantCardEntry
        {
            Id = id,
            Name = label,
            IsStocked = stocked,
            EnoughGold = enoughGold,
            CreationResult = new FakeCardCreationResult
            {
                Card = card,
            },
        };
        _cardNode = new FakeCardNode
        {
            Model = card,
            _descriptionLabel = new FakeLabel { Text = card.EvaluatedDescription },
        };
    }

    public FakeMerchantCardEntry Entry { get; }

    public object? _cardNode { get; init; }
}

file sealed class FakeNMerchantPotion : FakeMerchantSlotBase
{
    public FakeNMerchantPotion(string label, string id, double x, double y, bool enabled, bool stocked, bool enoughGold)
        : base(label, id, x, y, enabled)
    {
        var model = new FakePotionModel
        {
            Id = id,
            Name = label,
            Description = "이번 전투에서 {CalculatedPower:strength()} 얻습니다.",
            HoverTips = new object[]
            {
                new FakeHoverTip
                {
                    Id = id,
                    Title = label,
                    Description = "이번 전투에서 힘을 2 얻습니다.",
                },
            },
        };
        Entry = new FakeMerchantPotionEntry
        {
            Id = id,
            Name = label,
            IsStocked = stocked,
            EnoughGold = enoughGold,
            Model = model,
        };
    }

    public FakeMerchantPotionEntry Entry { get; init; }
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

    public string? TextKey { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public object[] HoverTips { get; init; } = Array.Empty<object>();

    public object? Relic { get; init; }

    public string? TargetSelectorHint { get; init; }

    public string? TargetFilter { get; init; }

    public bool IsLocked { get; init; }

    public bool IsProceed { get; init; }
}

file sealed class FakeNEventOptionButton : FakeSceneNode
{
    public FakeNEventOptionButton(int index, string title, string description, double x, double y, bool enabled, bool isProceed = false, string? optionKey = null)
    {
        Index = index;
        Visible = true;
        Enabled = enabled;
        Position = new FakeVector2(x, y);
        Size = new FakeVector2(820, 92);
        Option = new FakeEventOptionModel
        {
            Id = $"option-{index}",
            TextKey = optionKey ?? $"option-{index}",
            Title = title,
            Description = description,
            IsLocked = false,
            IsProceed = isProceed,
        };
        _label = new FakeLabel { Text = $"[gold][b]{title}[/b][/gold]\n{description}" };
    }

    public int Index { get; }

    public FakeEventOptionModel Option { get; init; }

    public FakeLabel? _label { get; init; }

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

    public bool IsEnabled { get; init; }
}

file sealed class FakeNRestSiteRoom
{
    public bool Visible { get; init; }

    public object? _lastFocused { get; init; }

    public object? ProceedButton { get; init; }
}

file sealed class FakeNRestSiteButton
{
    public bool Visible { get; init; }

    public bool Enabled { get; init; }

    public object? Option { get; init; }

    public object? MouseFilter { get; init; }

    public object? Position { get; init; }

    public object? Size { get; init; }
}

file sealed class FakeNProceedButton
{
    public bool Visible { get; init; }

    public bool Enabled { get; init; }

    public object? Position { get; init; }

    public object? Size { get; init; }
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

file sealed class FakeNAbandonRunConfirmPopup
{
    public object? VerticalPopup { get; init; }
}

file sealed class FakeNVerticalPopup
{
    public object? YesButton { get; init; }

    public object? NoButton { get; init; }
}

file sealed class FakeNPopupYesNoButton
{
    public bool IsYes { get; init; }

    public bool Visible { get; init; }

    public bool Enabled { get; init; }

    public object? Position { get; init; }

    public object? Size { get; init; }

    public object? _label { get; init; }
}

file sealed class FakeNMainMenu
{
    public object? _continueButton { get; init; }

    public object? _abandonRunButton { get; init; }
}

file class FakeNMainMenuTextButton
{
    public bool Visible { get; init; }

    public bool Enabled { get; init; }

    public object? Position { get; init; }

    public object? Size { get; init; }

    public object? _label { get; init; }
}

file sealed class FakeNMainMenuContinueButton : FakeNMainMenuTextButton
{
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
        CardNode = new FakeCardNode
        {
            Model = cardModel,
            _descriptionLabel = string.IsNullOrWhiteSpace(cardModel.EvaluatedDescription)
                ? null
                : new FakeLabel { Text = cardModel.EvaluatedDescription },
        };
        Position = new FakeVector2(x, y);
        Size = new FakeVector2(180, 254);
    }

    public bool Visible { get; init; }

    public object CardModel { get; }

    public object CardNode { get; }

    public object Position { get; }

    public object Size { get; }
}

file sealed class FakeCardSelectionHolder
{
    public bool Visible { get; init; }

    public bool Enabled { get; init; }

    public object? Position { get; init; }

    public object? Size { get; init; }

    public object? CardModel { get; init; }

    public object? CardNode { get; init; }
}

file sealed class FakeCardNode
{
    public object? Model { get; init; }

    public object? _descriptionLabel { get; init; }
}

file sealed class FakeCardModel
{
    public string? Id { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }

    public string? EvaluatedDescription { get; init; }

    public object[] HoverTips { get; init; } = Array.Empty<object>();
}

file sealed class FakeFeedbackScreen
{
}

file sealed class FakeCardCreationResult
{
    public FakeCardModel? Card { get; init; }
}

file sealed class FakeRelicModel
{
    public string? Id { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }

    public object[] HoverTips { get; init; } = Array.Empty<object>();
}

file sealed class FakePotionModel
{
    public string? Id { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }

    public object[] HoverTips { get; init; } = Array.Empty<object>();
}

file sealed class FakeHoverTip
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? Id { get; init; }

    public object? CanonicalModel { get; init; }
}

file sealed class FakeCardHoverTip
{
    public FakeCardHoverTip(FakeCardModel card)
    {
        Card = card;
    }

    public FakeCardModel Card { get; }

    public string? Id => Card.Id ?? Card.Name;

    public object CanonicalModel => Card;
}

file sealed class FakeCardLikeChoice
{
    public string? Description { get; init; }
}

file sealed class FakeChoiceDescriptionCarrier
{
    public string? Description { get; init; }

    public object? _label { get; init; }
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

    public bool HasOpenPotionSlots { get; init; }

    public object? Deck { get; init; }
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
    public object[] Deck { get; init; } = Array.Empty<object>();

    public object[] Hand { get; init; } = Array.Empty<object>();

    public object[] DrawPile { get; init; } = Array.Empty<object>();

    public object[] DiscardPile { get; init; } = Array.Empty<object>();

    public object[] ExhaustPile { get; init; } = Array.Empty<object>();

    public object[] PlayPile { get; init; } = Array.Empty<object>();
}

file sealed class FakeCombatCard
{
    public string? Name { get; init; }

    public string? CardId { get; init; }

    public int Cost { get; init; }

    public string? Type { get; init; }
}

file sealed class FakeDeckPile
{
    public object[] Cards { get; init; } = Array.Empty<object>();
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

file sealed class FakeNGameRoot
{
}

file sealed class FakeCombatState
{
    public int? RoundNumber { get; init; }

    public object[] Cards { get; init; } = Array.Empty<object>();
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

    public object[] ActiveHolders { get; init; } = Array.Empty<object>();

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

file sealed record HostSceneModelScenario(
    string Name,
    string RunId,
    string EventKind,
    LiveExportSnapshot Snapshot,
    string ExpectedSceneType,
    string ExpectedSceneStage,
    string ExpectedCanonicalOwner,
    int ExpectedOptionCount,
    IReadOnlyList<string> ExpectedMissingFacts);

file sealed record ScreenProvenanceParityScenario(
    string Name,
    string EventKind,
    LiveExportSnapshot Snapshot,
    HarnessNodeInventory? Inventory,
    string ExpectedResolvedCurrentScreen,
    string ExpectedResolvedVisibleScreen,
    string ExpectedSceneType,
    string ExpectedCanonicalOwner);

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
