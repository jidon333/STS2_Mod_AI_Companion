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
                IsManualCleanBootObserverReady(bootstrapStates[2], DateTimeOffset.UtcNow.AddSeconds(-5)),
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
            Assert(bootstrapIndex == 2, "Manual clean boot bootstrap should keep polling past the fresh logo-animation main menu until a concrete run-start surface appears.");
            Assert(string.Equals(bootstrappedObserver.CurrentScreen, "main-menu", StringComparison.OrdinalIgnoreCase), "Manual clean boot bootstrap should keep polling until a fresh main-menu observer arrives.");
            Assert(
                bootstrappedObserver.ActionNodes.Any(node => string.Equals(node.Label, "Singleplayer", StringComparison.OrdinalIgnoreCase)),
                "Manual clean boot bootstrap should ignore logo-animation-only main-menu states and wait for an actual run-start surface.");
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
                  "currentScreen": "legacy-event",
                  "capturedAt": "REPLACE_CAPTURED_AT",
                  "meta": {
                    "visibleScreen": "legacy-direct-visible",
                    "sceneReady": "true",
                    "sceneAuthority": "hook",
                    "sceneStability": "stable",
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
            Assert(string.Equals(aliasObserver.CompatibilityLogicalScreen, "legacy-compat", StringComparison.OrdinalIgnoreCase), "Observer reader should preserve compatibility logical screen provenance separately from compatibility current screen.");
            Assert(aliasObserver.PublishedSceneReady == true
                   && string.Equals(aliasObserver.PublishedSceneAuthority, "hook", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(aliasObserver.PublishedSceneStability, "stable", StringComparison.OrdinalIgnoreCase), "Observer reader should preserve direct published scene readiness and authority before compatibility fallback.");
            Assert(string.Equals(aliasObserver.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase), "Observer reader should prefer additive compatibilityCurrentScreen over legacy currentScreen and compatLogicalScreen.");
            Assert(string.Equals(aliasObserver.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase), "Observer reader should prefer additive compatibilityVisibleScreen over legacy compatVisibleScreen and inventory fallback.");

            File.WriteAllText(
                liveLayout.SnapshotPath,
                """
                {
                  "version": 8,
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

            var inventoryPublishedFallbackObserver = aliasReader.Read();
            Assert(string.Equals(inventoryPublishedFallbackObserver.PublishedCurrentScreen, "inventory-published", StringComparison.OrdinalIgnoreCase), "Observer reader should use additive inventory published current screen when the state snapshot omits direct published screen fields.");
            Assert(string.Equals(inventoryPublishedFallbackObserver.PublishedVisibleScreen, "inventory-published-visible", StringComparison.OrdinalIgnoreCase), "Observer reader should use additive inventory published visible screen when the state snapshot omits direct published visible fields.");
            Assert(inventoryPublishedFallbackObserver.PublishedSceneReady == false
                   && string.Equals(inventoryPublishedFallbackObserver.PublishedSceneAuthority, "inventory-published-authority", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(inventoryPublishedFallbackObserver.PublishedSceneStability, "inventory-published-stability", StringComparison.OrdinalIgnoreCase), "Observer reader should preserve additive inventory published readiness and authority without collapsing them into compatibility fields.");

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
