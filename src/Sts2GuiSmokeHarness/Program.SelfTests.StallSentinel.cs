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
    private static void RunStallSentinelSelfTests()
    {
        var restSiteNoOpSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-rest-site-noop-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(restSiteNoOpSentinelRoot);
            var runRoot = Path.Combine(restSiteNoOpSentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(restSiteNoOpSentinelRoot, "rest-site-noop-session", "boot-to-long-run", "headless");
            File.WriteAllLines(
                Path.Combine(restSiteNoOpSentinelRoot, "attempt-index.ndjson"),
                new[]
                {
                    JsonSerializer.Serialize(
                        new GuiSmokeAttemptIndexEntry(
                            "0001",
                            1,
                            "rest-site-noop-session",
                            "failed",
                            "synthetic rest-site post-click noop",
                            DateTimeOffset.UtcNow.AddSeconds(-30),
                            DateTimeOffset.UtcNow.AddSeconds(-5),
                            6,
                            "rest-site-post-click-noop",
                            false,
                            "rest-site-post-click-noop",
                            GuiSmokeContractStates.TrustValid),
                        GuiSmokeShared.NdjsonOptions),
                });
            File.WriteAllText(
                Path.Combine(runRoot, "failure-summary.json"),
                JsonSerializer.Serialize(
                    new GuiSmokeFailureSummary(
                        GuiSmokePhase.ChooseFirstNode.ToString(),
                        "rest-site-post-click-noop synthetic failure",
                        "map",
                        false,
                        "synthetic-rest-site-post-click-noop.png"),
                    GuiSmokeShared.JsonOptions));
            LongRunArtifacts.RefreshStallSentinel(restSiteNoOpSentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(restSiteNoOpSentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic rest-site post-click noop attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase)
                   && diagnosisEntries[0].StallDetected,
                "Stall sentinel should preserve rest-site-post-click-noop as a first-class diagnosis kind.");
        }
        finally
        {
            if (Directory.Exists(restSiteNoOpSentinelRoot))
            {
                Directory.Delete(restSiteNoOpSentinelRoot, recursive: true);
            }
        }

        var restSiteReleaseSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-rest-site-release-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(restSiteReleaseSentinelRoot);
            var runRoot = Path.Combine(restSiteReleaseSentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(restSiteReleaseSentinelRoot, "rest-site-release-session", "boot-to-long-run", "headless");
            File.WriteAllLines(
                Path.Combine(restSiteReleaseSentinelRoot, "attempt-index.ndjson"),
                new[]
                {
                    JsonSerializer.Serialize(
                        new GuiSmokeAttemptIndexEntry(
                            "0001",
                            1,
                            "rest-site-release-session",
                            "failed",
                            "synthetic rest-site release pending stall",
                            DateTimeOffset.UtcNow.AddSeconds(-30),
                            DateTimeOffset.UtcNow.AddSeconds(-5),
                            8,
                            "rest-site-release-map-handoff-stall",
                            false,
                            "rest-site-release-map-handoff-stall",
                            GuiSmokeContractStates.TrustValid),
                        GuiSmokeShared.NdjsonOptions),
                });
            File.WriteAllText(
                Path.Combine(runRoot, "failure-summary.json"),
                JsonSerializer.Serialize(
                    new GuiSmokeFailureSummary(
                        GuiSmokePhase.ChooseFirstNode.ToString(),
                        "rest-site-release-map-handoff-stall synthetic failure",
                        "rest-site",
                        false,
                        "synthetic-rest-site-release-map-handoff-stall.png"),
                    GuiSmokeShared.JsonOptions));
            LongRunArtifacts.RefreshStallSentinel(restSiteReleaseSentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(restSiteReleaseSentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic rest-site release-pending attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "rest-site-release-map-handoff-stall", StringComparison.OrdinalIgnoreCase)
                   && diagnosisEntries[0].StallDetected,
                "Stall sentinel should preserve rest-site-release-map-handoff-stall as a first-class diagnosis kind.");
        }
        finally
        {
            if (Directory.Exists(restSiteReleaseSentinelRoot))
            {
                Directory.Delete(restSiteReleaseSentinelRoot, recursive: true);
            }
        }

        Assert(
            string.Equals(
                ClassifyFailureForAttempt(
                    GuiSmokePhase.HandleEvent,
                    observer: null,
                    terminalCause: "ancient-event-option-contract-mismatch",
                    launchFailed: false),
                "ancient-event-option-contract-mismatch",
                StringComparison.OrdinalIgnoreCase),
            "Ancient event option contract mismatch should stay a first-class failure class instead of collapsing to generic decision-abort.");
        Assert(
            IsSceneDeadEndAttempt(
                new GuiSmokeAttemptResult(
                    "0001",
                    1,
                    "ancient-contract-session",
                    "synthetic-run-root",
                    1,
                    "failed",
                    "synthetic ancient option contract mismatch",
                    12,
                    false,
                    "ancient-event-option-contract-mismatch",
                    "ancient-event-option-contract-mismatch",
                    GuiSmokeContractStates.TrustValid)),
            "Ancient event option contract mismatch should count as a dead-end attempt for supervision.");
        Assert(
            string.Equals(
                ClassifyFailureForAttempt(
                    GuiSmokePhase.ChooseFirstNode,
                    observer: null,
                    terminalCause: "post-node-handoff-contract-mismatch",
                    launchFailed: false),
                "post-node-handoff-contract-mismatch",
                StringComparison.OrdinalIgnoreCase),
            "Post-node handoff contract mismatch should stay a first-class failure class instead of collapsing to generic decision-abort.");
        Assert(
            IsSceneDeadEndAttempt(
                new GuiSmokeAttemptResult(
                    "0001",
                    1,
                    "post-node-handoff-session",
                    "synthetic-run-root",
                    1,
                    "failed",
                    "synthetic post-node handoff contract mismatch",
                    12,
                    false,
                    "post-node-handoff-contract-mismatch",
                    "post-node-handoff-contract-mismatch",
                    GuiSmokeContractStates.TrustValid)),
            "Post-node handoff contract mismatch should count as a dead-end attempt for supervision.");
        Assert(
            string.Equals(
                ClassifyFailureForAttempt(
                    GuiSmokePhase.HandleCombat,
                    observer: null,
                    terminalCause: "combat-lifecycle-transit-step-budget-exhausted",
                    launchFailed: false),
                "combat-lifecycle-transit-step-budget-exhausted",
                StringComparison.OrdinalIgnoreCase),
            "Combat lifecycle transit step-budget exhaustion should stay a first-class failure class instead of collapsing to generic max-step failure.");
        Assert(
            string.Equals(
                ClassifyFailureForAttempt(
                    GuiSmokePhase.HandleCombat,
                    observer: null,
                    terminalCause: "combat-barrier-step-budget-exhausted",
                    launchFailed: false),
                "combat-barrier-step-budget-exhausted",
                StringComparison.OrdinalIgnoreCase),
            "Combat barrier step-budget exhaustion should stay a first-class failure class instead of collapsing to generic max-step failure.");
        Assert(
            string.Equals(
                ClassifyFailureForAttempt(
                    GuiSmokePhase.HandleCombat,
                    observer: null,
                    terminalCause: "combat-release-failure-under-noncombat-foreground",
                    launchFailed: false),
                "combat-release-failure-under-noncombat-foreground",
                StringComparison.OrdinalIgnoreCase),
            "Combat release failure under noncombat foreground should stay a first-class failure class instead of collapsing to generic decision-abort.");
        Assert(
            IsSceneDeadEndAttempt(
                new GuiSmokeAttemptResult(
                    "0001",
                    1,
                    "combat-lifecycle-session",
                    "synthetic-run-root",
                    1,
                    "failed",
                    "synthetic combat lifecycle transit step-budget exhaustion",
                    180,
                    false,
                    "combat-lifecycle-transit-step-budget-exhausted",
                    "combat-lifecycle-transit-step-budget-exhausted",
                    GuiSmokeContractStates.TrustValid)),
            "Combat lifecycle transit step-budget exhaustion should count as a dead-end attempt for supervision.");
        Assert(
            IsSceneDeadEndAttempt(
                new GuiSmokeAttemptResult(
                    "0001",
                    1,
                    "combat-barrier-session",
                    "synthetic-run-root",
                    1,
                    "failed",
                    "synthetic combat barrier step-budget exhaustion",
                    180,
                    false,
                    "combat-barrier-step-budget-exhausted",
                    "combat-barrier-step-budget-exhausted",
                    GuiSmokeContractStates.TrustValid)),
            "Combat barrier step-budget exhaustion should count as a dead-end attempt for supervision.");
        Assert(
            IsSceneDeadEndAttempt(
                new GuiSmokeAttemptResult(
                    "0001",
                    1,
                    "combat-release-session",
                    "synthetic-run-root",
                    1,
                    "failed",
                    "synthetic combat release failure under noncombat foreground",
                    18,
                    false,
                    "combat-release-failure-under-noncombat-foreground",
                    "combat-release-failure-under-noncombat-foreground",
                    GuiSmokeContractStates.TrustValid)),
            "Combat release failure under noncombat foreground should count as a dead-end attempt for supervision.");
        Assert(
            IsSceneDeadEndAttempt(
                new GuiSmokeAttemptResult(
                    "0001",
                    1,
                    "combat-lifecycle-plateau-session",
                    "synthetic-run-root",
                    1,
                    "failed",
                    "synthetic combat lifecycle transit wait plateau",
                    18,
                    false,
                    "combat-lifecycle-transit-wait-plateau",
                    "combat-lifecycle-transit-wait-plateau",
                    GuiSmokeContractStates.TrustValid)),
            "Combat lifecycle transit wait plateau should count as a dead-end attempt for supervision.");
        Assert(
            IsSceneDeadEndAttempt(
                new GuiSmokeAttemptResult(
                    "0001",
                    1,
                    "combat-barrier-wait-session",
                    "synthetic-run-root",
                    1,
                    "failed",
                    "synthetic combat barrier wait plateau",
                    18,
                    false,
                    "combat-barrier-wait-plateau",
                    "combat-barrier-wait-plateau",
                    GuiSmokeContractStates.TrustValid)),
            "Combat barrier wait plateau should count as a dead-end attempt for supervision.");

        var ancientContractMismatchSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-ancient-contract-mismatch-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(ancientContractMismatchSentinelRoot);
            var runRoot = Path.Combine(ancientContractMismatchSentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(ancientContractMismatchSentinelRoot, "ancient-contract-session", "boot-to-long-run", "headless");
            File.WriteAllLines(
                Path.Combine(ancientContractMismatchSentinelRoot, "attempt-index.ndjson"),
                new[]
                {
                    JsonSerializer.Serialize(
                        new GuiSmokeAttemptIndexEntry(
                            "0001",
                            1,
                            "ancient-contract-session",
                            "failed",
                            "synthetic ancient option contract mismatch",
                            DateTimeOffset.UtcNow.AddSeconds(-30),
                            DateTimeOffset.UtcNow.AddSeconds(-5),
                            9,
                            "ancient-event-option-contract-mismatch",
                            false,
                            "ancient-event-option-contract-mismatch",
                            GuiSmokeContractStates.TrustValid),
                        GuiSmokeShared.NdjsonOptions),
                });
            File.WriteAllText(
                Path.Combine(runRoot, "failure-summary.json"),
                JsonSerializer.Serialize(
                    new GuiSmokeFailureSummary(
                        GuiSmokePhase.HandleEvent.ToString(),
                        "ancient option contract mismatch synthetic failure",
                        "event",
                        false,
                        "synthetic-ancient-contract-mismatch.png"),
                    GuiSmokeShared.JsonOptions));
            LongRunArtifacts.RefreshStallSentinel(ancientContractMismatchSentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(ancientContractMismatchSentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic ancient contract mismatch attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "ancient-event-option-contract-mismatch", StringComparison.OrdinalIgnoreCase)
                   && diagnosisEntries[0].StallDetected,
                "Stall sentinel should preserve ancient-event-option-contract-mismatch as a first-class diagnosis kind.");
        }
        finally
        {
            if (Directory.Exists(ancientContractMismatchSentinelRoot))
            {
                Directory.Delete(ancientContractMismatchSentinelRoot, recursive: true);
            }
        }

        var postNodeHandoffMismatchSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-post-node-handoff-mismatch-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(postNodeHandoffMismatchSentinelRoot);
            var runRoot = Path.Combine(postNodeHandoffMismatchSentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(postNodeHandoffMismatchSentinelRoot, "post-node-handoff-session", "boot-to-long-run", "headless");
            File.WriteAllLines(
                Path.Combine(postNodeHandoffMismatchSentinelRoot, "attempt-index.ndjson"),
                new[]
                {
                    JsonSerializer.Serialize(
                        new GuiSmokeAttemptIndexEntry(
                            "0001",
                            1,
                            "post-node-handoff-session",
                            "failed",
                            "synthetic post-node handoff contract mismatch",
                            DateTimeOffset.UtcNow.AddSeconds(-30),
                            DateTimeOffset.UtcNow.AddSeconds(-5),
                            9,
                            "post-node-handoff-contract-mismatch",
                            false,
                            "post-node-handoff-contract-mismatch",
                            GuiSmokeContractStates.TrustValid),
                        GuiSmokeShared.NdjsonOptions),
                });
            File.WriteAllText(
                Path.Combine(runRoot, "failure-summary.json"),
                JsonSerializer.Serialize(
                    new GuiSmokeFailureSummary(
                        GuiSmokePhase.ChooseFirstNode.ToString(),
                        "post-node handoff contract mismatch synthetic failure",
                        "event",
                        false,
                        "synthetic-post-node-handoff-contract-mismatch.png"),
                    GuiSmokeShared.JsonOptions));
            LongRunArtifacts.RefreshStallSentinel(postNodeHandoffMismatchSentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(postNodeHandoffMismatchSentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic post-node handoff mismatch attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "post-node-handoff-contract-mismatch", StringComparison.OrdinalIgnoreCase)
                   && diagnosisEntries[0].StallDetected,
                "Stall sentinel should preserve post-node-handoff-contract-mismatch as a first-class diagnosis kind.");
        }
        finally
        {
            if (Directory.Exists(postNodeHandoffMismatchSentinelRoot))
            {
                Directory.Delete(postNodeHandoffMismatchSentinelRoot, recursive: true);
            }
        }

        var combatBarrierBudgetSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-combat-barrier-budget-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(combatBarrierBudgetSentinelRoot);
            var runRoot = Path.Combine(combatBarrierBudgetSentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(combatBarrierBudgetSentinelRoot, "combat-barrier-session", "boot-to-long-run", "headless");
            File.WriteAllLines(
                Path.Combine(combatBarrierBudgetSentinelRoot, "attempt-index.ndjson"),
                new[]
                {
                    JsonSerializer.Serialize(
                        new GuiSmokeAttemptIndexEntry(
                            "0001",
                            1,
                            "combat-barrier-session",
                            "completed",
                            "synthetic combat barrier step-budget exhaustion",
                            DateTimeOffset.UtcNow.AddSeconds(-30),
                            DateTimeOffset.UtcNow.AddSeconds(-5),
                            180,
                            "combat-barrier-step-budget-exhausted",
                            false,
                            "combat-barrier-step-budget-exhausted",
                            GuiSmokeContractStates.TrustValid),
                        GuiSmokeShared.NdjsonOptions),
                });
            File.WriteAllText(
                Path.Combine(runRoot, "failure-summary.json"),
                JsonSerializer.Serialize(
                    new GuiSmokeFailureSummary(
                        GuiSmokePhase.HandleCombat.ToString(),
                        "combat barrier step-budget exhaustion synthetic failure",
                        "combat",
                        false,
                        "synthetic-combat-barrier-step-budget-exhausted.png"),
                    GuiSmokeShared.JsonOptions));
            LongRunArtifacts.RefreshStallSentinel(combatBarrierBudgetSentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(combatBarrierBudgetSentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic combat barrier budget exhaustion attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "combat-barrier-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
                   && diagnosisEntries[0].StallDetected,
                "Stall sentinel should preserve combat-barrier-step-budget-exhausted as a first-class diagnosis kind.");
        }
        finally
        {
            if (Directory.Exists(combatBarrierBudgetSentinelRoot))
            {
                Directory.Delete(combatBarrierBudgetSentinelRoot, recursive: true);
            }
        }

        var combatLifecycleBudgetSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-combat-lifecycle-budget-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(combatLifecycleBudgetSentinelRoot);
            var runRoot = Path.Combine(combatLifecycleBudgetSentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(combatLifecycleBudgetSentinelRoot, "combat-lifecycle-session", "boot-to-long-run", "headless");
            File.WriteAllLines(
                Path.Combine(combatLifecycleBudgetSentinelRoot, "attempt-index.ndjson"),
                new[]
                {
                    JsonSerializer.Serialize(
                        new GuiSmokeAttemptIndexEntry(
                            "0001",
                            1,
                            "combat-lifecycle-session",
                            "completed",
                            "synthetic combat lifecycle transit step-budget exhaustion",
                            DateTimeOffset.UtcNow.AddSeconds(-30),
                            DateTimeOffset.UtcNow.AddSeconds(-5),
                            180,
                            "combat-lifecycle-transit-step-budget-exhausted",
                            false,
                            "combat-lifecycle-transit-step-budget-exhausted",
                            GuiSmokeContractStates.TrustValid),
                        GuiSmokeShared.NdjsonOptions),
                });
            File.WriteAllText(
                Path.Combine(runRoot, "failure-summary.json"),
                JsonSerializer.Serialize(
                    new GuiSmokeFailureSummary(
                        GuiSmokePhase.HandleCombat.ToString(),
                        "combat lifecycle transit step-budget exhaustion synthetic failure",
                        "combat",
                        false,
                        "synthetic-combat-lifecycle-transit-step-budget-exhausted.png"),
                    GuiSmokeShared.JsonOptions));
            LongRunArtifacts.RefreshStallSentinel(combatLifecycleBudgetSentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(combatLifecycleBudgetSentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic combat lifecycle budget exhaustion attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "combat-lifecycle-transit-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
                   && diagnosisEntries[0].StallDetected,
                "Stall sentinel should preserve combat-lifecycle-transit-step-budget-exhausted as a first-class diagnosis kind.");
        }
        finally
        {
            if (Directory.Exists(combatLifecycleBudgetSentinelRoot))
            {
                Directory.Delete(combatLifecycleBudgetSentinelRoot, recursive: true);
            }
        }

        var combatReleaseFailureSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-combat-release-failure-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(combatReleaseFailureSentinelRoot);
            var runRoot = Path.Combine(combatReleaseFailureSentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(combatReleaseFailureSentinelRoot, "combat-release-session", "boot-to-long-run", "headless");
            File.WriteAllLines(
                Path.Combine(combatReleaseFailureSentinelRoot, "attempt-index.ndjson"),
                new[]
                {
                    JsonSerializer.Serialize(
                        new GuiSmokeAttemptIndexEntry(
                            "0001",
                            1,
                            "combat-release-session",
                            "failed",
                            "synthetic combat release failure under noncombat foreground",
                            DateTimeOffset.UtcNow.AddSeconds(-30),
                            DateTimeOffset.UtcNow.AddSeconds(-5),
                            18,
                            "combat-release-failure-under-noncombat-foreground",
                            false,
                            "combat-release-failure-under-noncombat-foreground",
                            GuiSmokeContractStates.TrustValid),
                        GuiSmokeShared.NdjsonOptions),
                });
            File.WriteAllText(
                Path.Combine(runRoot, "failure-summary.json"),
                JsonSerializer.Serialize(
                    new GuiSmokeFailureSummary(
                        GuiSmokePhase.HandleCombat.ToString(),
                        "combat release failure under noncombat foreground synthetic failure",
                        "event",
                        false,
                        "synthetic-combat-release-failure.png"),
                    GuiSmokeShared.JsonOptions));
            LongRunArtifacts.RefreshStallSentinel(combatReleaseFailureSentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(combatReleaseFailureSentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic combat release failure attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "combat-release-failure-under-noncombat-foreground", StringComparison.OrdinalIgnoreCase)
                   && diagnosisEntries[0].StallDetected,
                "Stall sentinel should preserve combat-release-failure-under-noncombat-foreground as a first-class diagnosis kind.");
        }
        finally
        {
            if (Directory.Exists(combatReleaseFailureSentinelRoot))
            {
                Directory.Delete(combatReleaseFailureSentinelRoot, recursive: true);
            }
        }

        var stallSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-stall-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(stallSentinelRoot);
            var runRoot = Path.Combine(stallSentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(stallSentinelRoot, "stall-session", "boot-to-long-run", "headless");
            var progressPath = Path.Combine(runRoot, "progress.ndjson");
            var progressEntries = new[]
            {
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 7, GuiSmokePhase.Embark.ToString(), "phase:embark|screen:event|visible:event|encounter:none|ready:true|stability:stable|room:treasure|shot:A", "event", null, null, true, false, true, false, false, new[] { "scene-ready-true", "decision-wait" }, Array.Empty<string>()),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 8, GuiSmokePhase.Embark.ToString(), "phase:embark|screen:event|visible:event|encounter:none|ready:true|stability:stable|room:treasure|shot:B", "event", null, null, true, false, true, false, false, new[] { "scene-ready-true", "decision-wait" }, Array.Empty<string>()),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 9, GuiSmokePhase.Embark.ToString(), "phase:embark|screen:event|visible:event|encounter:none|ready:true|stability:stable|room:treasure|shot:C", "event", null, null, true, false, true, false, false, new[] { "scene-ready-true", "decision-wait" }, Array.Empty<string>()),
            };
            File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
            LongRunArtifacts.RefreshStallSentinel(stallSentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(stallSentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic in-progress attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase), "Stall sentinel should classify Embark/event repeated waits as phase-mismatch-stall.");
            Assert(diagnosisEntries[0].StallDetected, "Stall sentinel should flag the repeated Embark/event wait plateau as a stall.");
        }
        finally
        {
            if (Directory.Exists(stallSentinelRoot))
            {
                Directory.Delete(stallSentinelRoot, recursive: true);
            }
        }

        var overlayLoopSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-overlay-loop-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(overlayLoopSentinelRoot);
            var runRoot = Path.Combine(overlayLoopSentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(overlayLoopSentinelRoot, "overlay-loop-session", "boot-to-long-run", "headless");
            var progressPath = Path.Combine(runRoot, "progress.ndjson");
            var progressEntries = new[]
            {
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-6), 6, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:reward-choice|substate:colorless-card-choice|shot:A", "event", null, "first event option", true, false, true, false, false, new[] { "scene-ready-true", "reward-choice", "colorless-card-choice" }, new[] { "action:click", "target:first event option" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-5), 7, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:inspect-overlay|substate:reward-choice|shot:B", "event", null, "overlay backdrop close", true, false, true, false, false, new[] { "scene-ready-true", "inspect-overlay", "reward-choice" }, new[] { "action:click", "target:overlay backdrop close" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-4), 8, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:event|visible:event|ready:true|stability:stable|substate:inspect-overlay|substate:reward-choice|shot:C", "event", null, null, true, false, true, false, false, new[] { "scene-ready-true", "inspect-overlay", "reward-choice", "alternate-branch:HandleEvent" }, Array.Empty<string>()),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 9, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:inspect-overlay|substate:reward-choice|shot:D", "event", null, "overlay backdrop close", true, false, true, false, false, new[] { "scene-ready-true", "inspect-overlay", "reward-choice" }, new[] { "action:click", "target:overlay backdrop close" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 10, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:event|visible:event|ready:true|stability:stable|substate:inspect-overlay|substate:reward-choice|shot:E", "event", null, null, true, false, true, false, false, new[] { "scene-ready-true", "inspect-overlay", "reward-choice", "alternate-branch:HandleEvent" }, Array.Empty<string>()),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 11, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:inspect-overlay|substate:reward-choice|shot:F", "event", null, "overlay backdrop close", true, false, true, false, false, new[] { "scene-ready-true", "inspect-overlay", "reward-choice" }, new[] { "action:click", "target:overlay backdrop close" }),
            };
            File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
            LongRunArtifacts.RefreshStallSentinel(overlayLoopSentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(overlayLoopSentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic inspect overlay loop attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "inspect-overlay-loop", StringComparison.OrdinalIgnoreCase), "Stall sentinel should classify repeated overlay dismissals in reward/event flow as inspect-overlay-loop.");
            Assert(diagnosisEntries[0].StallDetected, "Inspect overlay loop should be flagged as a stall.");
        }
        finally
        {
            if (Directory.Exists(overlayLoopSentinelRoot))
            {
                Directory.Delete(overlayLoopSentinelRoot, recursive: true);
            }
        }

        var mapTransitionSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-map-transition-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(mapTransitionSentinelRoot);
            var runRoot = Path.Combine(mapTransitionSentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(mapTransitionSentinelRoot, "map-transition-session", "boot-to-long-run", "headless");
            var progressPath = Path.Combine(runRoot, "progress.ndjson");
            var progressEntries = new[]
            {
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-5), 9, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:map-transition|visible:map-arrow|shot:A", "event", null, "event progression choice", true, false, true, false, false, new[] { "scene-ready-true", "map-transition-evidence" }, new[] { "action:click", "target:event progression choice" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-4), 10, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:event|visible:event|ready:true|stability:stable|substate:map-transition|visible:map-arrow|shot:B", "event", null, null, true, false, false, false, false, new[] { "scene-ready-true", "map-transition-evidence", "alternate-branch:HandleEvent" }, Array.Empty<string>()),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 11, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:map-transition|visible:map-arrow|shot:C", "event", null, "visible proceed", true, false, false, false, false, new[] { "scene-ready-true", "map-transition-evidence" }, new[] { "action:click", "target:visible proceed" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 12, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:event|visible:event|ready:true|stability:stable|substate:map-transition|visible:map-arrow|shot:D", "event", null, null, true, false, false, false, false, new[] { "scene-ready-true", "map-transition-evidence", "alternate-branch:HandleEvent" }, Array.Empty<string>()),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 13, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:map-transition|visible:map-arrow|shot:E", "event", null, "event progression choice", true, false, false, false, false, new[] { "scene-ready-true", "map-transition-evidence" }, new[] { "action:click", "target:event progression choice" }),
            };
            File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
            LongRunArtifacts.RefreshStallSentinel(mapTransitionSentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(mapTransitionSentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic map-transition loop attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "map-transition-stall", StringComparison.OrdinalIgnoreCase), "Stall sentinel should classify repeated event/proceed clicks on map-transition evidence as map-transition-stall.");
            Assert(diagnosisEntries[0].StallDetected, "Map transition loop should be flagged as a stall.");
        }
        finally
        {
            if (Directory.Exists(mapTransitionSentinelRoot))
            {
                Directory.Delete(mapTransitionSentinelRoot, recursive: true);
            }
        }

        var latestEventSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-latest-event-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(latestEventSentinelRoot);
            var runRoot = Path.Combine(latestEventSentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(latestEventSentinelRoot, "latest-event-session", "boot-to-long-run", "headless");
            var progressPath = Path.Combine(runRoot, "progress.ndjson");
            var progressEntries = new[]
            {
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-6), 5, GuiSmokePhase.HandleRewards.ToString(), "phase:handlerewards|screen:rewards|visible:map|stale:reward-choice|stale:reward-bounds|layer:map-background|visible:map-arrow|shot:R1", "rewards", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "reward-map-layered" }, new[] { "action:click", "target:visible map advance" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-5), 6, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:rewards|visible:map|stale:reward-choice|stale:reward-bounds|layer:map-background|visible:map-arrow|shot:R2", "rewards", null, null, true, false, false, false, false, new[] { "scene-ready-true", "reward-map-layered" }, Array.Empty<string>()),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 43, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|layer:event-foreground|contamination:map-arrow|shot:E1", "event", null, "visible reachable node", true, false, false, false, false, new[] { "scene-ready-true", "map-transition-evidence" }, new[] { "action:click", "target:visible reachable node" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 44, GuiSmokePhase.WaitCombat.ToString(), "phase:waitcombat|screen:event|visible:event|layer:event-foreground|contamination:map-arrow|shot:E2", "event", null, null, true, false, false, false, false, new[] { "scene-ready-true", "alternate-branch:HandleEvent", "map-transition-evidence", "choice-extractor:event", "reward-explicit-progression" }, Array.Empty<string>()),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 45, GuiSmokePhase.HandleEvent.ToString(), "phase:handleevent|screen:event|visible:event|layer:event-foreground|contamination:map-arrow|shot:E3", "event", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "map-transition-evidence" }, new[] { "action:click", "target:visible map advance" }),
            };
            File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
            LongRunArtifacts.RefreshStallSentinel(latestEventSentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(latestEventSentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the latest-event sentinel attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "map-transition-stall", StringComparison.OrdinalIgnoreCase), "Latest event/map contamination loop should classify as map-transition-stall.");
            Assert(string.Equals(diagnosisEntries[0].Phase, GuiSmokePhase.HandleEvent.ToString(), StringComparison.OrdinalIgnoreCase), "Latest event authority should win over stale reward summaries in stall diagnosis.");
            Assert(string.Equals(diagnosisEntries[0].ObserverScreen, "event", StringComparison.OrdinalIgnoreCase), "Latest event observer screen should win over stale reward summaries in stall diagnosis.");
        }
        finally
        {
            if (Directory.Exists(latestEventSentinelRoot))
            {
                Directory.Delete(latestEventSentinelRoot, recursive: true);
            }
        }

        var mapOverlaySentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-map-overlay-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(mapOverlaySentinelRoot);
            var runRoot = Path.Combine(mapOverlaySentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(mapOverlaySentinelRoot, "map-overlay-session", "boot-to-long-run", "headless");
            var progressPath = Path.Combine(runRoot, "progress.ndjson");
            var progressEntries = new[]
            {
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-5), 15, GuiSmokePhase.ChooseFirstNode.ToString(), "phase:choosefirstnode|screen:event|visible:event|layer:map-overlay-foreground|layer:event-background|stale:event-choice|map-back-navigation-available|current-node-arrow-visible|reachable-node-candidate-present|shot:A", "event", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "map-overlay-visible", "stale-event-choice", "current-node-arrow-visible", "reachable-node-candidate-present" }, new[] { "action:click", "target:visible map advance" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-4), 16, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:event|visible:event|layer:map-overlay-foreground|layer:event-background|stale:event-choice|map-back-navigation-available|current-node-arrow-visible|reachable-node-candidate-present|shot:B", "event", null, null, true, false, false, false, false, new[] { "scene-ready-true", "map-overlay-visible", "stale-event-choice", "alternate-branch:ChooseFirstNode" }, Array.Empty<string>()),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 17, GuiSmokePhase.ChooseFirstNode.ToString(), "phase:choosefirstnode|screen:event|visible:event|layer:map-overlay-foreground|layer:event-background|stale:event-choice|map-back-navigation-available|current-node-arrow-visible|reachable-node-candidate-present|shot:C", "event", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "map-overlay-visible", "stale-event-choice", "current-node-arrow-visible", "reachable-node-candidate-present" }, new[] { "action:click", "target:visible map advance" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 18, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:event|visible:event|layer:map-overlay-foreground|layer:event-background|stale:event-choice|map-back-navigation-available|current-node-arrow-visible|reachable-node-candidate-present|shot:D", "event", null, null, true, false, false, false, false, new[] { "scene-ready-true", "map-overlay-visible", "stale-event-choice", "alternate-branch:ChooseFirstNode" }, Array.Empty<string>()),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 19, GuiSmokePhase.ChooseFirstNode.ToString(), "phase:choosefirstnode|screen:event|visible:event|layer:map-overlay-foreground|layer:event-background|stale:event-choice|map-back-navigation-available|current-node-arrow-visible|reachable-node-candidate-present|shot:E", "event", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "map-overlay-visible", "stale-event-choice", "current-node-arrow-visible", "reachable-node-candidate-present" }, new[] { "action:click", "target:visible map advance" }),
            };
            File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
            LongRunArtifacts.RefreshStallSentinel(mapOverlaySentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(mapOverlaySentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic map-overlay loop attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "map-overlay-noop-loop", StringComparison.OrdinalIgnoreCase), "Map overlay current-node arrow repeats should classify as map-overlay-noop-loop.");
            Assert(diagnosisEntries[0].Evidence.Contains("mapOverlayVisible:true", StringComparer.OrdinalIgnoreCase), "Map overlay diagnosis should record map-overlay visibility.");
            Assert(diagnosisEntries[0].Evidence.Contains("staleEventChoicePresent:true", StringComparer.OrdinalIgnoreCase), "Map overlay diagnosis should record stale event choice contamination.");
            Assert(diagnosisEntries[0].Evidence.Contains("repeatedCurrentNodeArrowClick:true", StringComparer.OrdinalIgnoreCase), "Map overlay diagnosis should record repeated current-node-arrow clicks.");
        }
        finally
        {
            if (Directory.Exists(mapOverlaySentinelRoot))
            {
                Directory.Delete(mapOverlaySentinelRoot, recursive: true);
            }
        }

        var rewardMapLoopSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-reward-map-loop-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(rewardMapLoopSentinelRoot);
            var runRoot = Path.Combine(rewardMapLoopSentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(rewardMapLoopSentinelRoot, "reward-map-loop-session", "boot-to-long-run", "headless");
            var progressPath = Path.Combine(runRoot, "progress.ndjson");
            var progressEntries = new[]
            {
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-5), 33, GuiSmokePhase.HandleRewards.ToString(), "phase:handlerewards|screen:rewards|visible:rewards|ready:true|stability:stable|contamination:map-arrow|shot:A", "rewards", null, "visible map advance", true, false, true, false, false, new[] { "scene-ready-true", "choice-extractor:reward", "reward-screen-authority", "reward-explicit-progression" }, new[] { "action:click", "target:visible map advance" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-4), 34, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:rewards|visible:rewards|ready:true|stability:stable|contamination:map-arrow|shot:B", "rewards", null, null, true, false, false, false, false, new[] { "scene-ready-true", "choice-extractor:reward", "reward-screen-authority", "reward-explicit-progression", "alternate-branch:HandleRewards" }, Array.Empty<string>()),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 35, GuiSmokePhase.HandleRewards.ToString(), "phase:handlerewards|screen:rewards|visible:rewards|ready:true|stability:stable|contamination:map-arrow|shot:C", "rewards", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "choice-extractor:reward", "reward-screen-authority", "reward-explicit-progression" }, new[] { "action:click", "target:visible map advance" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 36, GuiSmokePhase.WaitMap.ToString(), "phase:waitmap|screen:rewards|visible:rewards|ready:true|stability:stable|contamination:map-arrow|shot:D", "rewards", null, null, true, false, false, false, false, new[] { "scene-ready-true", "choice-extractor:reward", "reward-screen-authority", "reward-explicit-progression", "alternate-branch:HandleRewards" }, Array.Empty<string>()),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 37, GuiSmokePhase.HandleRewards.ToString(), "phase:handlerewards|screen:rewards|visible:rewards|ready:true|stability:stable|contamination:map-arrow|shot:E", "rewards", null, "visible map advance", true, false, false, false, false, new[] { "scene-ready-true", "choice-extractor:reward", "reward-screen-authority", "reward-explicit-progression" }, new[] { "action:click", "target:visible map advance" }),
            };
            File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
            LongRunArtifacts.RefreshStallSentinel(rewardMapLoopSentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(rewardMapLoopSentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic reward/map loop attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "reward-map-loop", StringComparison.OrdinalIgnoreCase), "Stall sentinel should classify repeated map-like clicks on a reward-authoritative screen as reward-map-loop.");
            Assert(diagnosisEntries[0].Evidence.Contains("rewardExplicitChoicesPresent:true", StringComparer.OrdinalIgnoreCase), "Reward/map loop diagnosis should record that explicit reward choices were still present.");
            Assert(diagnosisEntries[0].Evidence.Contains("rewardMapArrowContamination:true", StringComparer.OrdinalIgnoreCase), "Reward/map loop diagnosis should record background map-arrow contamination.");
        }
        finally
        {
            if (Directory.Exists(rewardMapLoopSentinelRoot))
            {
                Directory.Delete(rewardMapLoopSentinelRoot, recursive: true);
            }
        }

        var combatNoOpSentinelRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-combat-noop-sentinel-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(combatNoOpSentinelRoot);
            var runRoot = Path.Combine(combatNoOpSentinelRoot, "attempts", "0001");
            Directory.CreateDirectory(runRoot);
            LongRunArtifacts.InitializeSessionArtifacts(combatNoOpSentinelRoot, "combat-noop-session", "boot-to-long-run", "headless");
            var progressPath = Path.Combine(runRoot, "progress.ndjson");
            var progressEntries = new[]
            {
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-6), 53, GuiSmokePhase.HandleRewards.ToString(), "phase:handlerewards|screen:rewards|visible:rewards|layer:reward-foreground|stale:reward-choice|shot:PREV", "rewards", null, "reward skip", false, false, false, false, false, new[] { "scene-ready-true", "reward-screen-authority", "stale-reward-choice" }, new[] { "action:click", "target:reward skip" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-5), 54, GuiSmokePhase.HandleCombat.ToString(), "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable|shot:A", "combat", null, "combat select attack slot 3", true, false, true, false, false, new[] { "scene-ready-true", "combat-energy", "combat-hand", "combat-noop-observed:combat lane slot 3" }, new[] { "action:press-key", "target:combat select attack slot 3" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-4), 55, GuiSmokePhase.HandleCombat.ToString(), "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable|shot:B", "combat", null, "auto-target enemy", false, false, false, false, false, new[] { "scene-ready-true", "combat-energy", "combat-hand", "combat-noop-observed:combat lane slot 3" }, new[] { "action:click", "target:auto-target enemy" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-3), 56, GuiSmokePhase.HandleCombat.ToString(), "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable|shot:C", "combat", null, "combat select attack slot 3", false, false, false, false, false, new[] { "scene-ready-true", "combat-energy", "combat-hand", "combat-noop-observed:combat lane slot 3" }, new[] { "action:press-key", "target:combat select attack slot 3" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-2), 57, GuiSmokePhase.HandleCombat.ToString(), "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable|shot:D", "combat", null, "auto-target enemy recenter", false, false, false, false, false, new[] { "scene-ready-true", "combat-energy", "combat-hand", "combat-noop-observed:combat lane slot 3" }, new[] { "action:click", "target:auto-target enemy recenter" }),
                new GuiSmokeStepProgress(DateTimeOffset.UtcNow.AddSeconds(-1), 58, GuiSmokePhase.HandleCombat.ToString(), "phase:handlecombat|screen:combat|visible:combat|ready:true|stability:stable|shot:E", "combat", null, "combat select attack slot 3", false, false, false, false, false, new[] { "scene-ready-true", "combat-energy", "combat-hand", "combat-noop-observed:combat lane slot 3" }, new[] { "action:press-key", "target:combat select attack slot 3" }),
            };
            File.WriteAllLines(progressPath, progressEntries.Select(entry => JsonSerializer.Serialize(entry, GuiSmokeShared.NdjsonOptions)));
            LongRunArtifacts.RefreshStallSentinel(combatNoOpSentinelRoot);
            var diagnosisEntries = File.ReadLines(Path.Combine(combatNoOpSentinelRoot, "stall-diagnosis.ndjson"))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<GuiSmokeStallDiagnosisEntry>(line, GuiSmokeShared.JsonOptions))
                .Where(static entry => entry is not null)
                .Cast<GuiSmokeStallDiagnosisEntry>()
                .ToArray();
            Assert(diagnosisEntries.Length == 1, "Expected a single stall diagnosis entry for the synthetic combat no-op loop attempt.");
            Assert(string.Equals(diagnosisEntries[0].DiagnosisKind, "combat-noop-loop", StringComparison.OrdinalIgnoreCase), "Stall sentinel should classify repeated attack-select/enemy-click no-op patterns as combat-noop-loop.");
            Assert(diagnosisEntries[0].StallDetected, "Combat no-op loop should be flagged as a stall.");
            Assert(!diagnosisEntries[0].Evidence.Contains("rewardMapLoopTarget:", StringComparer.OrdinalIgnoreCase), "Latest combat authority should keep stale reward evidence from overriding combat no-op diagnosis.");
        }
        finally
        {
            if (Directory.Exists(combatNoOpSentinelRoot))
            {
                Directory.Delete(combatNoOpSentinelRoot, recursive: true);
            }
        }
    }
}
