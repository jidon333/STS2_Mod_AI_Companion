using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModAiCompanion.Mod;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;
using static GuiSmokeChoicePrimitiveSupport;

internal static partial class Program
{
    private static void RunManualCleanBootSelfTests()
    {
        var manualCleanBootRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-clean-boot-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(manualCleanBootRoot);
            var harnessRoot = Path.Combine(manualCleanBootRoot, "harness");
            var inboxRoot = Path.Combine(harnessRoot, "inbox");
            var outboxRoot = Path.Combine(harnessRoot, "outbox");
            Directory.CreateDirectory(inboxRoot);
            Directory.CreateDirectory(outboxRoot);
            var harnessLayout = new HarnessQueueLayout(
                manualCleanBootRoot,
                harnessRoot,
                inboxRoot,
                Path.Combine(inboxRoot, "actions.ndjson"),
                outboxRoot,
                Path.Combine(outboxRoot, "results.ndjson"),
                Path.Combine(harnessRoot, "status.json"),
                Path.Combine(outboxRoot, "trace.ndjson"),
                Path.Combine(harnessRoot, "arm.json"),
                Path.Combine(outboxRoot, "inventory.latest.json"));
            File.WriteAllText(
                harnessLayout.StatusPath,
                JsonSerializer.Serialize(
                    new HarnessBridgeStatus(true, "active", null, null, DateTimeOffset.UtcNow, "warming up", null, null),
                    GuiSmokeShared.JsonOptions));
            File.WriteAllText(
                harnessLayout.InventoryPath,
                JsonSerializer.Serialize(
                    new HarnessNodeInventory(
                        "inventory-clean-boot",
                        DateTimeOffset.UtcNow,
                        "pending",
                        "main-menu",
                        null,
                        "dormant",
                        null,
                        true,
                        "mixed",
                        "stable",
                        Array.Empty<HarnessNodeInventoryItem>()),
                    GuiSmokeShared.JsonOptions));
            File.WriteAllText(harnessLayout.ActionsPath, "{}" + Environment.NewLine);

            LongRunArtifacts.InitializeSessionArtifacts(manualCleanBootRoot, "clean-boot-session", "boot-to-long-run", "headless");
            var screenshotPath = Path.Combine(manualCleanBootRoot, "main-menu.png");
            var observerPath = Path.Combine(manualCleanBootRoot, "main-menu.observer.json");
            var manualCleanBootFreshnessFloor = DateTimeOffset.UtcNow.AddSeconds(-5);
            File.WriteAllBytes(screenshotPath, Array.Empty<byte>());
            File.WriteAllText(observerPath, "{}");
            Assert(
                LongRunArtifacts.TryMarkManualCleanBootVerified(
                    manualCleanBootRoot,
                    harnessLayout,
                    new ObserverState(
                        new ObserverSummary(
                            "main-menu",
                            "main-menu",
                            false,
                            DateTimeOffset.UtcNow,
                            "inv-main-menu",
                            true,
                            "mixed",
                            "stable",
                            "episode-main-menu",
                            null,
                            "main-menu",
                            null,
                            null,
                            null,
                            new[] { "Singleplayer" },
                            Array.Empty<string>(),
                            Array.Empty<ObserverActionNode>(),
                            Array.Empty<ObserverChoice>(),
                            Array.Empty<ObservedCombatHandCard>()),
                        null,
                        null,
                        null),
                    Array.Empty<GuiSmokeHistoryEntry>(),
                    screenshotPath,
                    observerPath,
                    manualCleanBootFreshnessFloor),
                "Manual clean boot should verify on a clean main-menu first step even when status mode is transiently non-dormant.");
            var cleanBootPrevalidation = JsonSerializer.Deserialize<GuiSmokePrevalidation>(File.ReadAllText(Path.Combine(manualCleanBootRoot, "prevalidation.json")), GuiSmokeShared.JsonOptions)
                                      ?? throw new InvalidOperationException("Failed to read manual clean boot prevalidation self-test.");
            Assert(cleanBootPrevalidation.ManualCleanBootVerified, "Manual clean boot prevalidation should be marked valid on the inert stale-actions path.");
            Assert(cleanBootPrevalidation.ManualCleanBootEvidence?.ActionsQueueClear == true, "Inert stale actions should still satisfy the manual clean boot actions gate.");
            Assert(cleanBootPrevalidation.ManualCleanBootEvidence?.EvaluationNotes?.Contains("stale-actions-observed-but-inert", StringComparer.OrdinalIgnoreCase) == true, "Manual clean boot evidence should explain why stale actions were treated as inert.");
        }
        finally
        {
            if (Directory.Exists(manualCleanBootRoot))
            {
                Directory.Delete(manualCleanBootRoot, recursive: true);
            }
        }

        var manualCleanBootBlockedRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-clean-boot-blocked-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(manualCleanBootBlockedRoot);
            var harnessRoot = Path.Combine(manualCleanBootBlockedRoot, "harness");
            var inboxRoot = Path.Combine(harnessRoot, "inbox");
            var outboxRoot = Path.Combine(harnessRoot, "outbox");
            Directory.CreateDirectory(inboxRoot);
            Directory.CreateDirectory(outboxRoot);
            var harnessLayout = new HarnessQueueLayout(
                manualCleanBootBlockedRoot,
                harnessRoot,
                inboxRoot,
                Path.Combine(inboxRoot, "actions.ndjson"),
                outboxRoot,
                Path.Combine(outboxRoot, "results.ndjson"),
                Path.Combine(harnessRoot, "status.json"),
                Path.Combine(outboxRoot, "trace.ndjson"),
                Path.Combine(harnessRoot, "arm.json"),
                Path.Combine(outboxRoot, "inventory.latest.json"));
            File.WriteAllText(
                harnessLayout.InventoryPath,
                JsonSerializer.Serialize(
                    new HarnessNodeInventory(
                        "inventory-clean-boot-blocked",
                        DateTimeOffset.UtcNow,
                        "pending",
                        "main-menu",
                        null,
                        "dormant",
                        null,
                        true,
                        "mixed",
                        "stable",
                        Array.Empty<HarnessNodeInventoryItem>()),
                    GuiSmokeShared.JsonOptions));
            File.WriteAllText(
                harnessLayout.ArmSessionPath,
                JsonSerializer.Serialize(
                    new HarnessArmSession("token", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(5), "self-test"),
                    GuiSmokeShared.JsonOptions));

            LongRunArtifacts.InitializeSessionArtifacts(manualCleanBootBlockedRoot, "clean-boot-blocked-session", "boot-to-long-run", "headless");
            var screenshotPath = Path.Combine(manualCleanBootBlockedRoot, "main-menu.png");
            var observerPath = Path.Combine(manualCleanBootBlockedRoot, "main-menu.observer.json");
            var blockedManualCleanBootFreshnessFloor = DateTimeOffset.UtcNow.AddSeconds(-5);
            File.WriteAllBytes(screenshotPath, Array.Empty<byte>());
            File.WriteAllText(observerPath, "{}");
            Assert(
                !LongRunArtifacts.TryMarkManualCleanBootVerified(
                    manualCleanBootBlockedRoot,
                    harnessLayout,
                    new ObserverState(
                        new ObserverSummary(
                            "main-menu",
                            "main-menu",
                            false,
                            DateTimeOffset.UtcNow,
                            "inv-bootstrap-boundary",
                            true,
                            "mixed",
                            "stable",
                            "episode-bootstrap-boundary",
                            null,
                            "main-menu",
                            null,
                            null,
                            null,
                            Array.Empty<string>(),
                            Array.Empty<string>(),
                            Array.Empty<ObserverActionNode>(),
                            Array.Empty<ObserverChoice>(),
                            Array.Empty<ObservedCombatHandCard>()),
                        null,
                        null,
                        null),
                    Array.Empty<GuiSmokeHistoryEntry>(),
                    screenshotPath,
                    observerPath,
                    blockedManualCleanBootFreshnessFloor),
                "Manual clean boot should remain invalid when an external arm session is present.");
            var blockedPrevalidation = JsonSerializer.Deserialize<GuiSmokePrevalidation>(File.ReadAllText(Path.Combine(manualCleanBootBlockedRoot, "prevalidation.json")), GuiSmokeShared.JsonOptions)
                                     ?? throw new InvalidOperationException("Failed to read blocked manual clean boot prevalidation self-test.");
            Assert(blockedPrevalidation.ManualCleanBootEvidence?.BlockingReasons?.Contains("arm-session-present", StringComparer.OrdinalIgnoreCase) == true, "Manual clean boot failure evidence should record the blocking arm session.");
        }
        finally
        {
            if (Directory.Exists(manualCleanBootBlockedRoot))
            {
                Directory.Delete(manualCleanBootBlockedRoot, recursive: true);
            }
        }

        var manualCleanBootBootstrapRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-clean-boot-bootstrap-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(manualCleanBootBootstrapRoot);
            var harnessRoot = Path.Combine(manualCleanBootBootstrapRoot, "harness");
            var inboxRoot = Path.Combine(harnessRoot, "inbox");
            var outboxRoot = Path.Combine(harnessRoot, "outbox");
            var liveRoot = Path.Combine(manualCleanBootBootstrapRoot, "live");
            Directory.CreateDirectory(inboxRoot);
            Directory.CreateDirectory(outboxRoot);
            Directory.CreateDirectory(liveRoot);
            var harnessLayout = new HarnessQueueLayout(
                manualCleanBootBootstrapRoot,
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
                manualCleanBootBootstrapRoot,
                liveRoot,
                Path.Combine(liveRoot, "events.ndjson"),
                Path.Combine(liveRoot, "state.latest.json"),
                Path.Combine(liveRoot, "state.latest.txt"),
                Path.Combine(liveRoot, "session.json"))
            {
                SemanticSnapshotsRoot = Path.Combine(liveRoot, "semantic-snapshots"),
            };
            var inventoryCapturedAt = DateTimeOffset.UtcNow;
            File.WriteAllText(
                harnessLayout.InventoryPath,
                JsonSerializer.Serialize(
                    new HarnessNodeInventory(
                        "inventory-main-menu-only",
                        inventoryCapturedAt,
                        null,
                        "main-menu",
                        "episode-main-menu-only",
                        "dormant",
                        null,
                        true,
                        "mixed",
                        "stable",
                        Array.Empty<HarnessNodeInventoryItem>()),
                    GuiSmokeShared.JsonOptions));

            var inventoryOnlyObserver = new ObserverSnapshotReader(liveLayout, harnessLayout).Read();
            Assert(string.Equals(inventoryOnlyObserver.CurrentScreen, "main-menu", StringComparison.OrdinalIgnoreCase), "Observer should fall back to harness inventory sceneType when state.latest.json is not available.");
            Assert(string.Equals(inventoryOnlyObserver.VisibleScreen, "main-menu", StringComparison.OrdinalIgnoreCase), "Observer visibleScreen should fall back to harness inventory sceneType when state.latest.json is not available.");
            Assert(string.IsNullOrWhiteSpace(inventoryOnlyObserver.RawCurrentScreen), "Observer raw current-screen should stay null when inventory only provides the generic sceneType alias.");
            Assert(string.IsNullOrWhiteSpace(inventoryOnlyObserver.PublishedCurrentScreen), "Observer published current-screen should not collapse back to the generic inventory sceneType alias.");
            Assert(string.IsNullOrWhiteSpace(inventoryOnlyObserver.CompatibilityCurrentScreen), "Observer compatibility current-screen should stay null when inventory only provides the generic sceneType alias.");
            Assert(inventoryOnlyObserver.PublishedSceneReady is null
                   && inventoryOnlyObserver.CompatibilitySceneReady is null, "Observer scene-ready provenance should stay null when inventory only provides the generic sceneReady alias.");
            Assert(inventoryOnlyObserver.CapturedAt == inventoryCapturedAt, "Observer capturedAt should fall back to harness inventory when state.latest.json is not available.");

            LongRunArtifacts.InitializeSessionArtifacts(manualCleanBootBootstrapRoot, "clean-boot-bootstrap-session", "boot-to-long-run", "headless");
            var screenshotPath = Path.Combine(manualCleanBootBootstrapRoot, "main-menu.png");
            var observerPath = Path.Combine(manualCleanBootBootstrapRoot, "main-menu.observer.json");
            File.WriteAllBytes(screenshotPath, Array.Empty<byte>());
            File.WriteAllText(observerPath, "{}");
            Assert(
                LongRunArtifacts.TryMarkManualCleanBootVerified(
                    manualCleanBootBootstrapRoot,
                    harnessLayout,
                    inventoryOnlyObserver,
                    Array.Empty<GuiSmokeHistoryEntry>(),
                    screenshotPath,
                    observerPath,
                    inventoryCapturedAt.AddSeconds(-5)),
                "Manual clean boot should verify from fresh harness inventory main-menu evidence when state.latest.json is not available yet.");

            var bootstrapStates = new[]
            {
                new ObserverState(
                    new ObserverSummary(
                        null,
                        null,
                        false,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        Array.Empty<string>(),
                        Array.Empty<string>(),
                        Array.Empty<ObserverActionNode>(),
                        Array.Empty<ObserverChoice>(),
                        Array.Empty<ObservedCombatHandCard>()),
                    null,
                    null,
                    null),
                new ObserverState(
                    new ObserverSummary(
                        "main-menu",
                        "main-menu",
                        false,
                        DateTimeOffset.UtcNow,
                        null,
                        true,
                        "mixed",
                        "stable",
                        "episode-main-menu-bootstrap",
                        null,
                        "main-menu",
                        null,
                        null,
                        null,
                        Array.Empty<string>(),
                        Array.Empty<string>(),
                        Array.Empty<ObserverActionNode>(),
                        Array.Empty<ObserverChoice>(),
                        Array.Empty<ObservedCombatHandCard>())
                    {
                        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NLogoAnimation",
                            ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NLogoAnimation",
                            ["rootSceneIsMainMenu"] = "true",
                            ["choiceExtractorPath"] = "generic",
                        },
                    },
                    null,
                    null,
                    null),
                new ObserverState(
                    new ObserverSummary(
                        "main-menu",
                        "main-menu",
                        false,
                        DateTimeOffset.UtcNow,
                        null,
                        true,
                        "mixed",
                        "stable",
                        "episode-main-menu-bootstrap-surface",
                        null,
                        "main-menu",
                        null,
                        null,
                        null,
                        new[] { "Singleplayer" },
                        Array.Empty<string>(),
                        new[]
                        {
                            new ObserverActionNode("main-menu:singleplayer", "singleplayer", "Singleplayer", "620,680,420,96", true),
                        },
                        new[]
                        {
                            new ObserverChoice("singleplayer", "Singleplayer", "620,680,420,96", "main-menu:singleplayer")
                            {
                                NodeId = "main-menu:singleplayer",
                                BindingKind = "singleplayer",
                            },
                        },
                        Array.Empty<ObservedCombatHandCard>())
                    {
                        PublishedVisibleScreen = "bootstrap",
                        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                            ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                            ["choiceExtractorPath"] = "main-menu",
                            ["publishedVisibleScreen"] = "bootstrap",
                        },
                    },
                    null,
                    null,
                    null),
                new ObserverState(
                    new ObserverSummary(
                        "main-menu",
                        "main-menu",
                        false,
                        DateTimeOffset.UtcNow,
                        null,
                        true,
                        "mixed",
                        "stable",
                        "episode-main-menu-bootstrap-ready",
                        null,
                        "main-menu",
                        null,
                        null,
                        null,
                        new[] { "Singleplayer" },
                        Array.Empty<string>(),
                        new[]
                        {
                            new ObserverActionNode("main-menu:singleplayer", "singleplayer", "Singleplayer", "620,680,420,96", true),
                        },
                        new[]
                        {
                            new ObserverChoice("singleplayer", "Singleplayer", "620,680,420,96", "main-menu:singleplayer")
                            {
                                NodeId = "main-menu:singleplayer",
                                BindingKind = "singleplayer",
                            },
                        },
                        Array.Empty<ObservedCombatHandCard>()),
                    null,
                    null,
                    null),
            };
            var bootstrapIndex = 0;
            Assert(
                !IsManualCleanBootObserverReady(bootstrapStates[1], DateTimeOffset.UtcNow.AddSeconds(-5)),
                "Bootstrap polling should keep ignoring fresh logo-animation main-menu states that still lack Continue or Singleplayer authority.");
            Assert(
                !IsManualCleanBootObserverReady(bootstrapStates[2], DateTimeOffset.UtcNow.AddSeconds(-5)),
                "Bootstrap polling should keep ignoring run-start labels while published visibility still reports bootstrap foreground.");
            Assert(
                IsManualCleanBootObserverReady(bootstrapStates[3], DateTimeOffset.UtcNow.AddSeconds(-5)),
                "Bootstrap polling should treat exported Continue or Singleplayer authority as the actual run-start readiness signal.");
            var bootstrappedObserver = BootstrapManualCleanBootObserverAsync(
                    bootstrapStates[bootstrapIndex],
                    () => bootstrapStates[Math.Min(++bootstrapIndex, bootstrapStates.Length - 1)],
                    DateTimeOffset.UtcNow.AddSeconds(-5),
                    3,
                    0,
                    static _ => Task.CompletedTask)
                .GetAwaiter()
                .GetResult();
            Assert(bootstrapIndex == 3, "Manual clean boot bootstrap should keep polling past bootstrap-visible main-menu surfaces until a concrete run-start surface appears.");
            Assert(string.Equals(bootstrappedObserver.CurrentScreen, "main-menu", StringComparison.OrdinalIgnoreCase), "Manual clean boot bootstrap should keep polling until a fresh main-menu observer arrives.");
            Assert(
                bootstrappedObserver.ActionNodes.Any(node => string.Equals(node.Label, "Singleplayer", StringComparison.OrdinalIgnoreCase)),
                "Manual clean boot bootstrap should ignore bootstrap-visible main-menu states and wait for an actual run-start surface.");
        }
        finally
        {
            if (Directory.Exists(manualCleanBootBootstrapRoot))
            {
                Directory.Delete(manualCleanBootBootstrapRoot, recursive: true);
            }
        }

        var observerAliasPrecedenceRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-observer-alias-precedence-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(observerAliasPrecedenceRoot);
            var harnessRoot = Path.Combine(observerAliasPrecedenceRoot, "harness");
            var inboxRoot = Path.Combine(harnessRoot, "inbox");
            var outboxRoot = Path.Combine(harnessRoot, "outbox");
            var liveRoot = Path.Combine(observerAliasPrecedenceRoot, "live");
            Directory.CreateDirectory(inboxRoot);
            Directory.CreateDirectory(outboxRoot);
            Directory.CreateDirectory(liveRoot);
            var harnessLayout = new HarnessQueueLayout(
                observerAliasPrecedenceRoot,
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
                observerAliasPrecedenceRoot,
                liveRoot,
                Path.Combine(liveRoot, "events.ndjson"),
                Path.Combine(liveRoot, "state.latest.json"),
                Path.Combine(liveRoot, "state.latest.txt"),
                Path.Combine(liveRoot, "session.json"));

            var capturedAt = DateTimeOffset.UtcNow;
            File.WriteAllText(
                liveLayout.SnapshotPath,
                """
                {
                  "version": 7,
                  "currentScreen": "legacy-collapsed",
                  "publishedCurrentScreen": "legacy-event",
                  "publishedVisibleScreen": "legacy-direct-visible",
                  "publishedSceneReady": true,
                  "publishedSceneAuthority": "hook",
                  "publishedSceneStability": "stable",
                  "capturedAt": "REPLACE_CAPTURED_AT",
                  "meta": {
                    "visibleScreen": "legacy-generic-visible",
                    "sceneReady": "false",
                    "sceneAuthority": "legacy-generic-authority",
                    "sceneStability": "legacy-generic-stability",
                    "rawCurrentScreen": "rewards",
                    "rawObservedScreen": "legacy-raw",
                    "compatibilityCurrentScreen": "map",
                    "compatLogicalScreen": "legacy-compat",
                    "compatibilityVisibleScreen": "map",
                    "compatVisibleScreen": "legacy-visible"
                  },
                  "player": {},
                  "choices": []
                }
                """.Replace("REPLACE_CAPTURED_AT", capturedAt.ToString("O"), StringComparison.Ordinal));
            File.WriteAllText(
                harnessLayout.InventoryPath,
                JsonSerializer.Serialize(
                    new HarnessNodeInventory(
                        "inventory-alias-precedence",
                        capturedAt,
                        null,
                        "inventory-legacy",
                        "episode-alias-precedence",
                        "dormant",
                        null,
                        true,
                        "mixed",
                        "stable",
                        Array.Empty<HarnessNodeInventoryItem>())
                    {
                        RawCurrentScreen = "inventory-raw",
                        PublishedCurrentScreen = "inventory-published",
                        PublishedVisibleScreen = "inventory-published-visible",
                        PublishedSceneReady = false,
                        PublishedSceneAuthority = "inventory-published-authority",
                        PublishedSceneStability = "inventory-published-stability",
                        CompatibilityCurrentScreen = "inventory-compat",
                        CompatibilityVisibleScreen = "inventory-visible",
                    },
                    GuiSmokeShared.JsonOptions));

            var aliasReader = new ObserverSnapshotReader(liveLayout, harnessLayout);
            var aliasObserver = aliasReader.Read();
            Assert(string.Equals(aliasObserver.RawCurrentScreen, "rewards", StringComparison.OrdinalIgnoreCase), "Observer reader should preserve additive rawCurrentScreen separately from legacy rawObservedScreen.");
            Assert(string.Equals(aliasObserver.RawObservedScreen, "legacy-raw", StringComparison.OrdinalIgnoreCase), "Observer reader should preserve legacy rawObservedScreen instead of collapsing it onto rawCurrentScreen.");
            Assert(string.Equals(aliasObserver.PublishedCurrentScreen, "legacy-event", StringComparison.OrdinalIgnoreCase), "Observer reader should preserve the direct published current screen before compatibility fallback.");
            Assert(string.Equals(aliasObserver.PublishedVisibleScreen, "legacy-direct-visible", StringComparison.OrdinalIgnoreCase), "Observer reader should preserve the direct published visible screen before compatibility fallback.");
            Assert(string.Equals(aliasObserver.CompatibilityCurrentScreen, "map", StringComparison.OrdinalIgnoreCase), "Observer reader should preserve the explicit compatibility current screen instead of collapsing back to the published screen.");
            Assert(aliasObserver.PublishedSceneReady == true
                   && string.Equals(aliasObserver.PublishedSceneAuthority, "hook", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(aliasObserver.PublishedSceneStability, "stable", StringComparison.OrdinalIgnoreCase), "Observer reader should preserve direct published scene readiness and authority before compatibility fallback.");
            Assert(string.Equals(aliasObserver.CurrentScreen, "legacy-event", StringComparison.OrdinalIgnoreCase), "Observer reader primary current screen should now prefer published current screen before compatibility aliases.");
            Assert(string.Equals(aliasObserver.VisibleScreen, "legacy-direct-visible", StringComparison.OrdinalIgnoreCase), "Observer reader primary visible screen should now prefer published visible screen before compatibility aliases.");
            Assert(string.Equals(ObserverScreenProvenance.DirectCurrentScreen(aliasObserver), "rewards", StringComparison.OrdinalIgnoreCase), "Direct current-screen helper should only report raw current-screen provenance.");
            Assert(string.Equals(ObserverScreenProvenance.DirectObservedScreen(aliasObserver), "legacy-raw", StringComparison.OrdinalIgnoreCase), "Direct observed-screen helper should only report raw observed-screen provenance.");
            Assert(string.Equals(ObserverScreenProvenance.PublishedCurrentScreen(aliasObserver), "legacy-event", StringComparison.OrdinalIgnoreCase), "Published current-screen helper should only report published provenance.");
            Assert(string.Equals(ObserverScreenProvenance.PublishedVisibleScreen(aliasObserver), "legacy-direct-visible", StringComparison.OrdinalIgnoreCase), "Published visible-screen helper should only report published provenance.");
            Assert(ObserverScreenProvenance.PublishedSceneReady(aliasObserver) == true
                   && string.Equals(ObserverScreenProvenance.PublishedSceneAuthority(aliasObserver), "hook", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(ObserverScreenProvenance.PublishedSceneStability(aliasObserver), "stable", StringComparison.OrdinalIgnoreCase), "Published scene helpers should only report published readiness, authority, and stability.");
            Assert(string.Equals(ObserverScreenProvenance.StrictCompatibilityCurrentScreen(aliasObserver), "map", StringComparison.OrdinalIgnoreCase), "Strict compatibility current-screen helper should stay within compatibility provenance.");
            Assert(string.Equals(ObserverScreenProvenance.StrictCompatibilityVisibleScreen(aliasObserver), "map", StringComparison.OrdinalIgnoreCase), "Strict compatibility visible-screen helper should stay within compatibility provenance.");
            Assert(string.Equals(ObserverScreenProvenance.ControlFlowCurrentScreen(aliasObserver), "legacy-event", StringComparison.OrdinalIgnoreCase), "Control-flow screen helper should prefer published current screen before compatibility aliases.");
            Assert(string.Equals(ObserverScreenProvenance.ControlFlowVisibleScreen(aliasObserver), "legacy-direct-visible", StringComparison.OrdinalIgnoreCase), "Control-flow visible-screen helper should prefer published visible screen before compatibility aliases.");
            Assert(ObserverScreenProvenance.ControlFlowSceneReady(aliasObserver) == true
                   && string.Equals(ObserverScreenProvenance.ControlFlowSceneAuthority(aliasObserver), "hook", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(ObserverScreenProvenance.ControlFlowSceneStability(aliasObserver), "stable", StringComparison.OrdinalIgnoreCase), "Control-flow scene helpers should prefer published readiness and authority before compatibility aliases.");
            Assert(string.Equals(ObserverScreenProvenance.DisplayScreen(aliasObserver), "legacy-event", StringComparison.OrdinalIgnoreCase), "Display screen should follow control-flow provenance instead of the compatibility-collapsed current screen.");
            var aliasSceneSignature = GuiSmokeSceneReasoningSupport.ComputeSceneSignatureCore(null, aliasObserver, GuiSmokePhase.HandleEvent, null);
            Assert(aliasSceneSignature.Contains("screen:legacy-event", StringComparison.OrdinalIgnoreCase), "Scene signatures should use control-flow current screen instead of collapsing back to compatibility current screen.");
            Assert(aliasSceneSignature.Contains("visible:legacy-direct-visible", StringComparison.OrdinalIgnoreCase), "Scene signatures should use control-flow visible screen instead of compatibility visible screen.");
            var aliasObserverDescription = DescribeObserverHuman(aliasObserver);
            Assert(aliasObserverDescription.Contains("logical=legacy-event", StringComparison.OrdinalIgnoreCase), "Observer description should report control-flow current screen, not compatibility current screen.");
            Assert(aliasObserverDescription.Contains("visible=legacy-direct-visible", StringComparison.OrdinalIgnoreCase), "Observer description should report control-flow visible screen, not compatibility visible screen.");

            File.WriteAllText(
                liveLayout.SnapshotPath,
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
                """.Replace("REPLACE_CAPTURED_AT", capturedAt.ToString("O"), StringComparison.Ordinal));

            var combatTrackerObserver = aliasReader.Read();
            Assert(string.Equals(combatTrackerObserver.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase), "Observer reader primary current screen should promote tracker combat screen over stale published map provenance when encounter truth says in-combat.");
            Assert(string.Equals(combatTrackerObserver.VisibleScreen, "combat", StringComparison.OrdinalIgnoreCase), "Observer reader primary visible screen should follow the promoted tracker combat screen during in-combat observations.");
            Assert(string.Equals(ObserverScreenProvenance.ControlFlowCurrentScreen(combatTrackerObserver), "combat", StringComparison.OrdinalIgnoreCase), "Control-flow current screen should prefer promoted tracker combat truth over stale published map provenance.");
            Assert(string.Equals(ObserverScreenProvenance.ControlFlowVisibleScreen(combatTrackerObserver), "combat", StringComparison.OrdinalIgnoreCase), "Control-flow visible screen should prefer promoted tracker combat truth over stale published map provenance.");

            var staleInventoryCapturedAt = capturedAt.AddSeconds(-2);
            var freshStateCapturedAt = capturedAt.AddSeconds(2);
            File.WriteAllText(
                harnessLayout.InventoryPath,
                JsonSerializer.Serialize(
                    new HarnessNodeInventory(
                        "inventory-stale-main-menu-popup",
                        staleInventoryCapturedAt,
                        null,
                        "main-menu",
                        "main-menu",
                        "dormant",
                        null,
                        null,
                        null,
                        null,
                        new[]
                        {
                            new HarnessNodeInventoryItem(
                                "main-menu:abandon-run-cancel",
                                "menu-action",
                                "No",
                                "main-menu abandon run confirm popup",
                                "cancel",
                                null,
                                true,
                                null,
                                true,
                                "699,688,180,72",
                                new[] { "node-id:main-menu:abandon-run-cancel" }),
                            new HarnessNodeInventoryItem(
                                "main-menu:abandon-run-confirm",
                                "menu-action",
                                "Yes",
                                "main-menu abandon run confirm popup",
                                "confirm",
                                null,
                                true,
                                null,
                                true,
                                "1041,688,180,72",
                                new[] { "node-id:main-menu:abandon-run-confirm" }),
                        })
                    {
                        RawCurrentScreen = "main-menu",
                        PublishedCurrentScreen = "main-menu",
                        PublishedVisibleScreen = "main-menu",
                        CompatibilityCurrentScreen = "main-menu",
                    },
                    GuiSmokeShared.JsonOptions));
            File.WriteAllText(
                liveLayout.SnapshotPath,
                """
                {
                  "version": 9,
                  "capturedAt": "REPLACE_CAPTURED_AT",
                  "currentScreen": "main-menu",
                  "publishedCurrentScreen": "main-menu",
                  "publishedVisibleScreen": "main-menu",
                  "encounter": {
                    "inCombat": false
                  },
                  "meta": {
                    "screen": "main-menu",
                    "screen-episode": "main-menu",
                    "choiceExtractorPath": "main-menu",
                    "activeScreenType": "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    "rawObservedScreen": "main-menu",
                    "rawCurrentScreen": "0"
                  },
                  "player": {},
                  "currentChoices": [
                    {
                      "kind": "menu-action",
                      "label": "Singleplayer",
                      "value": "main-menu:singleplayer",
                      "description": "Main menu",
                      "nodeId": "main-menu:singleplayer",
                      "screenBounds": "676,684,200,50",
                      "enabled": true,
                      "semanticHints": []
                    },
                    {
                      "kind": "menu-action",
                      "label": "Quit",
                      "value": "main-menu:quit",
                      "description": "Main menu",
                      "nodeId": "main-menu:quit",
                      "screenBounds": "676,934,200,50",
                      "enabled": true,
                      "semanticHints": []
                    }
                  ]
                }
                """.Replace("REPLACE_CAPTURED_AT", freshStateCapturedAt.ToString("O"), StringComparison.Ordinal));

            var staleInventoryMainMenuObserver = aliasReader.Read();
            Assert(!staleInventoryMainMenuObserver.ActionNodes.Any(node => string.Equals(node.NodeId, "main-menu:abandon-run-confirm", StringComparison.OrdinalIgnoreCase)), "Observer reader should discard stale popup action nodes when a fresher state snapshot exposes a new main-menu choice surface.");
            Assert(staleInventoryMainMenuObserver.ActionNodes.Any(node => string.Equals(node.NodeId, "main-menu:singleplayer", StringComparison.OrdinalIgnoreCase)), "Observer reader should rebuild actionable main-menu nodes from the fresher state choice surface when the inventory surface lags behind.");
            Assert(!MainMenuRunStartObserverSignals.HasAbandonRunConfirmSurface(staleInventoryMainMenuObserver), "Main-menu popup detection should not survive on stale inventory nodes once the fresher state snapshot has returned to the main-menu action surface.");
            Assert(MainMenuRunStartObserverSignals.IsRunStartSurfaceReady(staleInventoryMainMenuObserver), "Main-menu run-start readiness should follow the fresher state-derived action surface instead of a lagging popup inventory.");

            File.WriteAllText(
                harnessLayout.InventoryPath,
                JsonSerializer.Serialize(
                    new HarnessNodeInventory(
                        "inventory-legacy-only",
                        capturedAt,
                        null,
                        "inventory-legacy",
                        "episode-legacy-only",
                        "dormant",
                        null,
                        null,
                        null,
                        null,
                        Array.Empty<HarnessNodeInventoryItem>()),
                    GuiSmokeShared.JsonOptions));
            File.WriteAllText(
                liveLayout.SnapshotPath,
                """
                {
                  "version": 7,
                  "currentScreen": "legacy-collapsed",
                  "capturedAt": "REPLACE_CAPTURED_AT",
                  "meta": {
                    "visibleScreen": "legacy-generic-visible",
                    "sceneReady": "false",
                    "sceneAuthority": "legacy-generic-authority",
                    "sceneStability": "legacy-generic-stability"
                  },
                  "player": {},
                  "choices": []
                }
                """.Replace("REPLACE_CAPTURED_AT", capturedAt.ToString("O"), StringComparison.Ordinal));

            var legacyCompatibilityLeakObserver = aliasReader.Read();
            Assert(string.IsNullOrWhiteSpace(legacyCompatibilityLeakObserver.CurrentScreen), "Observer reader top-level currentScreen should stay empty when only legacy collapsed screen aliases exist.");
            Assert(string.IsNullOrWhiteSpace(legacyCompatibilityLeakObserver.VisibleScreen), "Observer reader visibleScreen should stay empty instead of rehydrating legacy current/visible screen aliases.");
            Assert(legacyCompatibilityLeakObserver.SceneReady is null
                   && string.IsNullOrWhiteSpace(legacyCompatibilityLeakObserver.SceneAuthority)
                   && string.IsNullOrWhiteSpace(legacyCompatibilityLeakObserver.SceneStability), "Observer reader control-flow scene fields should stay empty when only legacy meta scene fields are present.");
            Assert(legacyCompatibilityLeakObserver.PublishedSceneReady is null
                   && string.IsNullOrWhiteSpace(legacyCompatibilityLeakObserver.PublishedSceneAuthority)
                   && string.IsNullOrWhiteSpace(legacyCompatibilityLeakObserver.PublishedSceneStability), "Observer reader published provenance should stay empty when only legacy meta scene fields are present.");
            Assert(string.IsNullOrWhiteSpace(legacyCompatibilityLeakObserver.CompatibilityVisibleScreen), "Observer reader should not repopulate compatibility visible-screen provenance from legacy meta.visibleScreen.");
            Assert(legacyCompatibilityLeakObserver.CompatibilitySceneReady is null
                   && string.IsNullOrWhiteSpace(legacyCompatibilityLeakObserver.CompatibilitySceneAuthority)
                   && string.IsNullOrWhiteSpace(legacyCompatibilityLeakObserver.CompatibilitySceneStability), "Observer reader compatibility provenance should stay empty when only legacy meta scene fields are present.");
            Assert(string.IsNullOrWhiteSpace(ObserverScreenProvenance.ControlFlowCurrentScreen(legacyCompatibilityLeakObserver))
                   && string.IsNullOrWhiteSpace(ObserverScreenProvenance.ControlFlowVisibleScreen(legacyCompatibilityLeakObserver)), "Control-flow screen helpers should not collapse back to legacy top-level current/visible screen aliases when additive provenance is absent.");
            Assert(ObserverScreenProvenance.ControlFlowSceneReady(legacyCompatibilityLeakObserver) is null
                   && string.IsNullOrWhiteSpace(ObserverScreenProvenance.ControlFlowSceneAuthority(legacyCompatibilityLeakObserver))
                   && string.IsNullOrWhiteSpace(ObserverScreenProvenance.ControlFlowSceneStability(legacyCompatibilityLeakObserver)), "Control-flow scene helpers should stay empty when only legacy meta diagnostics exist.");
            Assert(!ObserverScreenProvenance.MatchesControlFlowScreen(legacyCompatibilityLeakObserver, "legacy-collapsed"), "Control-flow screen matching should not treat legacy top-level currentScreen as published/direct/compat provenance.");
            Assert(string.IsNullOrWhiteSpace(ObserverScreenProvenance.DisplayScreen(legacyCompatibilityLeakObserver)), "Display screen should stay empty when only legacy collapsed screen aliases exist.");

            File.WriteAllText(
                harnessLayout.InventoryPath,
                JsonSerializer.Serialize(
                    new HarnessNodeInventory(
                        "inventory-alias-precedence",
                        capturedAt,
                        null,
                        "inventory-legacy",
                        "episode-alias-precedence",
                        "dormant",
                        null,
                        true,
                        "mixed",
                        "stable",
                        Array.Empty<HarnessNodeInventoryItem>())
                    {
                        RawCurrentScreen = "inventory-raw",
                        PublishedCurrentScreen = "inventory-published",
                        PublishedVisibleScreen = "inventory-published-visible",
                        PublishedSceneReady = false,
                        PublishedSceneAuthority = "inventory-published-authority",
                        PublishedSceneStability = "inventory-published-stability",
                        CompatibilityCurrentScreen = "inventory-compat",
                        CompatibilityVisibleScreen = "inventory-visible",
                    },
                    GuiSmokeShared.JsonOptions));
            File.WriteAllText(
                liveLayout.SnapshotPath,
                """
                {
                  "version": 8,
                  "currentScreen": "legacy-collapsed",
                  "capturedAt": "REPLACE_CAPTURED_AT",
                  "meta": {
                    "rawObservedScreen": "legacy-raw-only",
                    "compatibilityCurrentScreen": "event",
                    "compatibilityVisibleScreen": "event"
                  },
                  "player": {},
                  "choices": []
                }
                """.Replace("REPLACE_CAPTURED_AT", capturedAt.ToString("O"), StringComparison.Ordinal));
            File.WriteAllText(
                harnessLayout.InventoryPath,
                JsonSerializer.Serialize(
                    new HarnessNodeInventory(
                        "inventory-alias-precedence",
                        capturedAt,
                        null,
                        "inventory-legacy",
                        "episode-alias-precedence",
                        "dormant",
                        null,
                        true,
                        "mixed",
                        "stable",
                        Array.Empty<HarnessNodeInventoryItem>())
                    {
                        RawCurrentScreen = "inventory-raw",
                        PublishedCurrentScreen = "inventory-published",
                        PublishedVisibleScreen = "inventory-published-visible",
                        PublishedSceneReady = false,
                        PublishedSceneAuthority = "inventory-published-authority",
                        PublishedSceneStability = "inventory-published-stability",
                        CompatibilityCurrentScreen = "inventory-compat",
                        CompatibilityVisibleScreen = "inventory-visible",
                    },
                    GuiSmokeShared.JsonOptions));

            var inventoryPublishedFallbackObserver = aliasReader.Read();
            Assert(string.Equals(inventoryPublishedFallbackObserver.PublishedCurrentScreen, "inventory-published", StringComparison.OrdinalIgnoreCase), "Observer reader should use additive inventory published current screen when the state snapshot omits direct published screen fields.");
            Assert(string.Equals(inventoryPublishedFallbackObserver.PublishedVisibleScreen, "inventory-published-visible", StringComparison.OrdinalIgnoreCase), "Observer reader should use additive inventory published visible screen when the state snapshot omits direct published visible fields.");
            Assert(string.Equals(inventoryPublishedFallbackObserver.CompatibilityCurrentScreen, "event", StringComparison.OrdinalIgnoreCase), "Observer reader should fold compatibility logical-screen fallback into the strict compatibility current-screen surface.");
            Assert(string.Equals(inventoryPublishedFallbackObserver.CompatibilityVisibleScreen, "event", StringComparison.OrdinalIgnoreCase), "Observer reader should fold compatibility visible-screen fallback into the strict compatibility visible-screen surface.");
            Assert(inventoryPublishedFallbackObserver.PublishedSceneReady == false
                   && string.Equals(inventoryPublishedFallbackObserver.PublishedSceneAuthority, "inventory-published-authority", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(inventoryPublishedFallbackObserver.PublishedSceneStability, "inventory-published-stability", StringComparison.OrdinalIgnoreCase), "Observer reader should preserve additive inventory published readiness and authority without collapsing them into compatibility fields.");

            File.WriteAllText(
                harnessLayout.InventoryPath,
                JsonSerializer.Serialize(
                    new HarnessNodeInventory(
                        "inventory-legacy-screen-only",
                        capturedAt,
                        null,
                        "inventory-legacy",
                        "episode-legacy-screen-only",
                        "dormant",
                        null,
                        null,
                        null,
                        null,
                        Array.Empty<HarnessNodeInventoryItem>()),
                    GuiSmokeShared.JsonOptions));
            File.WriteAllText(
                liveLayout.SnapshotPath,
                """
                {
                  "version": 9,
                  "currentScreen": "legacy-collapsed",
                  "publishedCurrentScreen": "published-only",
                  "capturedAt": "REPLACE_CAPTURED_AT",
                  "meta": {
                    "rawObservedScreen": "raw-only",
                    "visibleScreen": "legacy-visible",
                    "sceneReady": "true",
                    "sceneAuthority": "legacy",
                    "sceneStability": "stable"
                  },
                  "player": {},
                  "choices": []
                }
                """.Replace("REPLACE_CAPTURED_AT", capturedAt.ToString("O"), StringComparison.Ordinal));

            var legacyBleedObserver = aliasReader.Read();
            Assert(string.Equals(legacyBleedObserver.PublishedCurrentScreen, "published-only", StringComparison.OrdinalIgnoreCase), "Observer reader should still preserve direct published current-screen provenance.");
            Assert(string.IsNullOrWhiteSpace(legacyBleedObserver.PublishedVisibleScreen), "Observer reader should not repopulate published visible-screen provenance from published current-screen or legacy visibleScreen.");
            Assert(legacyBleedObserver.PublishedSceneReady is null
                   && string.IsNullOrWhiteSpace(legacyBleedObserver.PublishedSceneAuthority)
                   && string.IsNullOrWhiteSpace(legacyBleedObserver.PublishedSceneStability), "Observer reader should not repopulate published scene diagnostics from legacy sceneReady/sceneAuthority/sceneStability metadata.");
            Assert(string.IsNullOrWhiteSpace(legacyBleedObserver.CompatibilityVisibleScreen), "Observer reader compatibility visible-screen should stay empty when no explicit compatibility visible-screen provenance exists.");
            Assert(legacyBleedObserver.CompatibilitySceneReady is null
                   && string.IsNullOrWhiteSpace(legacyBleedObserver.CompatibilitySceneAuthority)
                   && string.IsNullOrWhiteSpace(legacyBleedObserver.CompatibilitySceneStability), "Observer reader compatibility diagnostics should stay empty when only legacy sceneReady/sceneAuthority/sceneStability metadata exists.");
            Assert(legacyBleedObserver.SceneReady is null
                   && string.IsNullOrWhiteSpace(legacyBleedObserver.SceneAuthority)
                   && string.IsNullOrWhiteSpace(legacyBleedObserver.SceneStability), "Observer reader primary scene diagnostics should stay empty when only legacy compatibility metadata exists.");
            Assert(string.Equals(legacyBleedObserver.VisibleScreen, "raw-only", StringComparison.OrdinalIgnoreCase), "Observer reader visible-screen should prefer direct raw observed screen over legacy visibleScreen metadata.");
            Assert(string.Equals(ObserverScreenProvenance.ControlFlowCurrentScreen(legacyBleedObserver), "published-only", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(ObserverScreenProvenance.ControlFlowVisibleScreen(legacyBleedObserver), "raw-only", StringComparison.OrdinalIgnoreCase), "Control-flow screen helpers should remain limited to published/direct/compat provenance after legacy scene metadata is demoted.");

            File.WriteAllLines(
                liveLayout.EventsPath,
                Enumerable.Range(1, 18)
                    .Select(index => $"event-line-{index:00}"));
            var tailObserver = aliasReader.Read();
            Assert(tailObserver.LastEventsTail.Count == 10
                   && string.Equals(tailObserver.LastEventsTail[0], "event-line-09", StringComparison.Ordinal)
                   && string.Equals(tailObserver.LastEventsTail[^1], "event-line-18", StringComparison.Ordinal),
                "Observer reader should preserve the latest 10 event lines without rereading the entire event stream semantics.");

            File.AppendAllLines(
                liveLayout.EventsPath,
                new[]
                {
                    "event-line-19",
                    "event-line-20",
                });
            var updatedTailObserver = aliasReader.Read();
            Assert(updatedTailObserver.LastEventsTail.Count == 10
                   && string.Equals(updatedTailObserver.LastEventsTail[0], "event-line-11", StringComparison.Ordinal)
                   && string.Equals(updatedTailObserver.LastEventsTail[^1], "event-line-20", StringComparison.Ordinal),
                "Observer reader event-tail cache should invalidate when the live events stream grows.");
        }
        finally
        {
            if (Directory.Exists(observerAliasPrecedenceRoot))
            {
                Directory.Delete(observerAliasPrecedenceRoot, recursive: true);
            }
        }

        var manualCleanBootObserverNotReadyRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-clean-boot-observer-not-ready-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(manualCleanBootObserverNotReadyRoot);
            var harnessRoot = Path.Combine(manualCleanBootObserverNotReadyRoot, "harness");
            var inboxRoot = Path.Combine(harnessRoot, "inbox");
            var outboxRoot = Path.Combine(harnessRoot, "outbox");
            Directory.CreateDirectory(inboxRoot);
            Directory.CreateDirectory(outboxRoot);
            var harnessLayout = new HarnessQueueLayout(
                manualCleanBootObserverNotReadyRoot,
                harnessRoot,
                inboxRoot,
                Path.Combine(inboxRoot, "actions.ndjson"),
                outboxRoot,
                Path.Combine(outboxRoot, "results.ndjson"),
                Path.Combine(harnessRoot, "status.json"),
                Path.Combine(outboxRoot, "trace.ndjson"),
                Path.Combine(harnessRoot, "arm.json"),
                Path.Combine(outboxRoot, "inventory.latest.json"));
            LongRunArtifacts.InitializeSessionArtifacts(manualCleanBootObserverNotReadyRoot, "clean-boot-observer-not-ready-session", "boot-to-long-run", "headless");
            var screenshotPath = Path.Combine(manualCleanBootObserverNotReadyRoot, "unknown.png");
            var observerPath = Path.Combine(manualCleanBootObserverNotReadyRoot, "unknown.observer.json");
            File.WriteAllBytes(screenshotPath, Array.Empty<byte>());
            File.WriteAllText(observerPath, "{}");
            Assert(
                !LongRunArtifacts.TryMarkManualCleanBootVerified(
                    manualCleanBootObserverNotReadyRoot,
                    harnessLayout,
                    new ObserverState(
                        new ObserverSummary(
                            null,
                            null,
                            false,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            Array.Empty<string>(),
                            Array.Empty<string>(),
                            Array.Empty<ObserverActionNode>(),
                            Array.Empty<ObserverChoice>(),
                            Array.Empty<ObservedCombatHandCard>()),
                        null,
                        null,
                        null),
                    Array.Empty<GuiSmokeHistoryEntry>(),
                    screenshotPath,
                    observerPath,
                    DateTimeOffset.UtcNow.AddSeconds(-5)),
                "Manual clean boot should remain invalid while observer evidence is not ready.");
            var notReadyPrevalidation = JsonSerializer.Deserialize<GuiSmokePrevalidation>(
                                            File.ReadAllText(Path.Combine(manualCleanBootObserverNotReadyRoot, "prevalidation.json")),
                                            GuiSmokeShared.JsonOptions)
                                        ?? throw new InvalidOperationException("Failed to read observer-not-ready manual clean boot self-test.");
            Assert(notReadyPrevalidation.ManualCleanBootEvidence?.BlockingReasons?.Contains("observer-not-ready", StringComparer.OrdinalIgnoreCase) == true, "Manual clean boot should report observer-not-ready instead of observer-not-main-menu:unknown.");
        }
        finally
        {
            if (Directory.Exists(manualCleanBootObserverNotReadyRoot))
            {
                Directory.Delete(manualCleanBootObserverNotReadyRoot, recursive: true);
            }
        }
    }
}
