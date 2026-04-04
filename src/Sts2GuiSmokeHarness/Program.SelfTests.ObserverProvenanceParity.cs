using System.Text.Json;
using Sts2AiCompanion.SceneProvenance;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;

internal static partial class Program
{
    private static void RunObserverProvenanceParitySelfTests()
    {
        var root = Path.Combine(Path.GetTempPath(), $"gui-smoke-observer-provenance-parity-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            var harnessRoot = Path.Combine(root, "harness");
            var inboxRoot = Path.Combine(harnessRoot, "inbox");
            var outboxRoot = Path.Combine(harnessRoot, "outbox");
            var liveRoot = Path.Combine(root, "live");
            Directory.CreateDirectory(inboxRoot);
            Directory.CreateDirectory(outboxRoot);
            Directory.CreateDirectory(liveRoot);

            var harnessLayout = new HarnessQueueLayout(
                root,
                harnessRoot,
                inboxRoot,
                Path.Combine(inboxRoot, "actions.ndjson"),
                outboxRoot,
                Path.Combine(outboxRoot, "results.ndjson"),
                Path.Combine(harnessRoot, "status.json"),
                Path.Combine(outboxRoot, "trace.ndjson"),
                Path.Combine(harnessRoot, "arm.json"),
                Path.Combine(outboxRoot, "inventory.latest.json"));
            var liveLayout = new LiveExportLayout(
                root,
                liveRoot,
                Path.Combine(liveRoot, "events.ndjson"),
                Path.Combine(liveRoot, "state.latest.json"),
                Path.Combine(liveRoot, "state.latest.txt"),
                Path.Combine(liveRoot, "session.json"))
            {
                SemanticSnapshotsRoot = Path.Combine(liveRoot, "semantic-snapshots"),
            };

            var capturedAt = DateTimeOffset.UtcNow;
            var reader = new ObserverSnapshotReader(liveLayout, harnessLayout);

            AssertObserverReaderMatchesSharedProvenance(
                "published-character-select",
                reader,
                liveLayout,
                harnessLayout,
                """
                {
                  "version": 7,
                  "currentScreen": "main-menu",
                  "publishedCurrentScreen": "character-select",
                  "publishedVisibleScreen": "character-select",
                  "publishedSceneReady": true,
                  "publishedSceneAuthority": "hook",
                  "publishedSceneStability": "stable",
                  "capturedAt": "REPLACE_CAPTURED_AT",
                  "encounter": {
                    "inCombat": false
                  },
                  "meta": {
                    "rawCurrentScreen": "main-menu",
                    "rawObservedScreen": "main-menu"
                  },
                  "player": {},
                  "choices": []
                }
                """.Replace("REPLACE_CAPTURED_AT", capturedAt.ToString("O"), StringComparison.Ordinal),
                inventoryJson: null,
                expectedCurrentScreen: "character-select",
                expectedVisibleScreen: "character-select",
                expectedSceneReady: true,
                expectedSceneAuthority: "hook",
                expectedSceneStability: "stable");

            AssertObserverReaderMatchesSharedProvenance(
                "combat-promotion-over-stale-map",
                reader,
                liveLayout,
                harnessLayout,
                """
                {
                  "version": 7,
                  "currentScreen": "combat",
                  "publishedCurrentScreen": "map",
                  "publishedVisibleScreen": "map",
                  "capturedAt": "REPLACE_CAPTURED_AT",
                  "encounter": {
                    "inCombat": true
                  },
                  "meta": {
                    "rawObservedScreen": "map"
                  },
                  "player": {},
                  "choices": []
                }
                """.Replace("REPLACE_CAPTURED_AT", capturedAt.AddSeconds(1).ToString("O"), StringComparison.Ordinal),
                inventoryJson: null,
                expectedCurrentScreen: "combat",
                expectedVisibleScreen: "combat",
                expectedSceneReady: null,
                expectedSceneAuthority: null,
                expectedSceneStability: null);

            AssertObserverReaderMatchesSharedProvenance(
                "state-document-blocks-legacy-inventory-scene-fallback",
                reader,
                liveLayout,
                harnessLayout,
                """
                {
                  "version": 7,
                  "currentScreen": "legacy-collapsed",
                  "visibleScreen": "legacy-collapsed",
                  "capturedAt": "REPLACE_CAPTURED_AT",
                  "encounter": {
                    "inCombat": false
                  },
                  "meta": {
                    "compatibilityCurrentScreen": "map",
                    "compatibilityVisibleScreen": "map",
                    "compatSceneReady": true,
                    "compatSceneAuthority": "legacy",
                    "compatSceneStability": "stable"
                  },
                  "player": {},
                  "choices": []
                }
                """.Replace("REPLACE_CAPTURED_AT", capturedAt.AddSeconds(2).ToString("O"), StringComparison.Ordinal),
                """
                {
                  "inventoryId": "inventory-legacy-map",
                  "capturedAt": "REPLACE_CAPTURED_AT",
                  "sceneType": "map",
                  "sceneEpisodeId": "episode-map",
                  "publishedSceneReady": true,
                  "publishedSceneAuthority": "inventory",
                  "publishedSceneStability": "stable"
                }
                """.Replace("REPLACE_CAPTURED_AT", capturedAt.AddSeconds(2).ToString("O"), StringComparison.Ordinal),
                expectedCurrentScreen: null,
                expectedVisibleScreen: null,
                expectedSceneReady: true,
                expectedSceneAuthority: "inventory",
                expectedSceneStability: "stable");
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static void AssertObserverReaderMatchesSharedProvenance(
        string scenarioName,
        ObserverSnapshotReader reader,
        LiveExportLayout liveLayout,
        HarnessQueueLayout harnessLayout,
        string stateJson,
        string? inventoryJson,
        string? expectedCurrentScreen,
        string? expectedVisibleScreen,
        bool? expectedSceneReady,
        string? expectedSceneAuthority,
        string? expectedSceneStability)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(liveLayout.SnapshotPath)!);
        File.WriteAllText(liveLayout.SnapshotPath, stateJson);
        if (inventoryJson is null)
        {
            if (File.Exists(harnessLayout.InventoryPath))
            {
                File.Delete(harnessLayout.InventoryPath);
            }
        }
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(harnessLayout.InventoryPath)!);
            File.WriteAllText(harnessLayout.InventoryPath, inventoryJson);
        }

        var observer = reader.Read(includeEventTail: false);
        using var stateDocument = JsonDocument.Parse(File.ReadAllText(liveLayout.SnapshotPath));
        using var inventoryDocument = inventoryJson is null
            ? null
            : JsonDocument.Parse(File.ReadAllText(harnessLayout.InventoryPath));
        var resolved = ScreenProvenanceResolver.Resolve(ScreenProvenanceResolver.CreateFromObserverDocuments(stateDocument, inventoryDocument));

        Assert(string.Equals(observer.CurrentScreen, expectedCurrentScreen, StringComparison.OrdinalIgnoreCase),
            $"Observer reader currentScreen should match expected resolved current screen for {scenarioName}.");
        Assert(string.Equals(observer.VisibleScreen, expectedVisibleScreen, StringComparison.OrdinalIgnoreCase),
            $"Observer reader visibleScreen should match expected resolved visible screen for {scenarioName}.");
        Assert(observer.SceneReady == expectedSceneReady,
            $"Observer reader sceneReady should match expected resolved sceneReady for {scenarioName}.");
        Assert(string.Equals(observer.SceneAuthority, expectedSceneAuthority, StringComparison.OrdinalIgnoreCase),
            $"Observer reader sceneAuthority should match expected resolved sceneAuthority for {scenarioName}.");
        Assert(string.Equals(observer.SceneStability, expectedSceneStability, StringComparison.OrdinalIgnoreCase),
            $"Observer reader sceneStability should match expected resolved sceneStability for {scenarioName}.");

        Assert(string.Equals(observer.CurrentScreen, resolved.ResolvedCurrentScreen, StringComparison.OrdinalIgnoreCase),
            $"Observer reader currentScreen should stay aligned with shared resolver for {scenarioName}.");
        Assert(string.Equals(observer.VisibleScreen, resolved.ResolvedVisibleScreen, StringComparison.OrdinalIgnoreCase),
            $"Observer reader visibleScreen should stay aligned with shared resolver for {scenarioName}.");
        Assert(observer.SceneReady == resolved.ResolvedSceneReady,
            $"Observer reader sceneReady should stay aligned with shared resolver for {scenarioName}.");
        Assert(string.Equals(observer.SceneAuthority, resolved.ResolvedSceneAuthority, StringComparison.OrdinalIgnoreCase),
            $"Observer reader sceneAuthority should stay aligned with shared resolver for {scenarioName}.");
        Assert(string.Equals(observer.SceneStability, resolved.ResolvedSceneStability, StringComparison.OrdinalIgnoreCase),
            $"Observer reader sceneStability should stay aligned with shared resolver for {scenarioName}.");

        Assert(string.Equals(ObserverScreenProvenance.ControlFlowCurrentScreen(observer), resolved.ResolvedCurrentScreen, StringComparison.OrdinalIgnoreCase),
            $"Harness control-flow current screen should stay aligned with shared resolver for {scenarioName}.");
        Assert(string.Equals(ObserverScreenProvenance.ControlFlowVisibleScreen(observer), resolved.ResolvedVisibleScreen, StringComparison.OrdinalIgnoreCase),
            $"Harness control-flow visible screen should stay aligned with shared resolver for {scenarioName}.");
        Assert(ObserverScreenProvenance.ControlFlowSceneReady(observer) == resolved.ResolvedSceneReady,
            $"Harness control-flow sceneReady should stay aligned with shared resolver for {scenarioName}.");
        Assert(string.Equals(ObserverScreenProvenance.ControlFlowSceneAuthority(observer), resolved.ResolvedSceneAuthority, StringComparison.OrdinalIgnoreCase),
            $"Harness control-flow sceneAuthority should stay aligned with shared resolver for {scenarioName}.");
        Assert(string.Equals(ObserverScreenProvenance.ControlFlowSceneStability(observer), resolved.ResolvedSceneStability, StringComparison.OrdinalIgnoreCase),
            $"Harness control-flow sceneStability should stay aligned with shared resolver for {scenarioName}.");
    }
}
