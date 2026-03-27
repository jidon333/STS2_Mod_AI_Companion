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

static partial class LongRunArtifacts
{
    private static readonly string[] GoalCompletionCriteria =
    {
        "attempt N terminal event is durably recorded",
        "runner-issued restart progress is durably recorded",
        "attempt N+1 steps/0001.screen.png exists for the same session",
    };

    private static readonly string[] GoalOperationalRules =
    {
        "runner is the only execution owner",
        "supervisor is read-mostly and decision-only",
        "stall sentinel is read-only and diagnosis-only",
        "supervisor and stall sentinel do not kill, relaunch, deploy, or create attempts",
        "commentary is not evidence for health, completion, or recovery",
        "gameplay results are untrusted until the trust gate is valid",
    };

    private static readonly TimeSpan NoArtifactHeartbeatThreshold = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan WindowNoStepThreshold = TimeSpan.FromSeconds(20);

    private sealed record MilestoneEvaluation(
        string MilestoneState,
        string? LastTerminalAttemptId,
        string? LastTerminalCause,
        string? LatestRestartTargetAttemptId,
        string? LatestNextAttemptId,
        string? LatestNextAttemptFirstScreenPath,
        IReadOnlyList<string> Evidence,
        IReadOnlyList<string> Blockers);

    private sealed record AttemptChronologyProjection(
        IReadOnlyList<string> StartedAttemptIds,
        string? ActiveAttemptId,
        string? LastTerminalAttemptId,
        string? LastTerminalCause,
        string? LatestRestartTargetAttemptId,
        string? LatestNextAttemptId,
        string? LatestNextAttemptFirstScreenPath);

    private sealed record DecisionWaitPlateauAnalysis(
        string DiagnosisKind,
        bool PlateauDetected,
        int RepeatedWaitCount,
        string? Phase,
        string? ObserverScreen,
        string? SceneSignature);

    private sealed record InspectOverlayLoopAnalysis(
        string DiagnosisKind,
        bool LoopDetected,
        int OverlayCloseCount,
        string? Phase,
        string? ObserverScreen,
        string? SceneSignature,
        string? LastMisdirectedTarget);

    private sealed record MapTransitionStallAnalysis(
        string DiagnosisKind,
        bool StallDetected,
        int RepeatedLoopCount,
        string? Phase,
        string? ObserverScreen,
        string? SceneSignature,
        string? LastLoopTarget);

    private sealed record MapOverlayNoOpLoopAnalysis(
        string DiagnosisKind,
        bool LoopDetected,
        int RepeatedLoopCount,
        string? Phase,
        string? ObserverScreen,
        string? SceneSignature,
        string? LastLoopTarget,
        bool MapOverlayVisible,
        bool MapBackNavigationAvailable,
        bool StaleEventChoicePresent,
        bool CurrentNodeArrowVisible,
        bool ReachableNodeCandidatePresent,
        bool RepeatedCurrentNodeArrowClick);

    private sealed record LatestStepContext(
        string? Phase,
        string? ObserverScreen,
        string? ScreenshotPath,
        MapOverlayState? MapOverlayState);

    private sealed record RewardMapLoopAnalysis(
        string DiagnosisKind,
        bool LoopDetected,
        int RepeatedLoopCount,
        string? Phase,
        string? ObserverScreen,
        string? SceneSignature,
        string? LastLoopTarget,
        bool ExplicitRewardChoicesPresent,
        bool MapArrowContaminationPresent,
        bool StaleRewardChoicePresent,
        bool StaleRewardBoundsPresent,
        bool RewardBackNavigationAvailable,
        bool OffWindowBoundsReused,
        bool ClaimableRewardPresent,
        bool MapNodeCandidateChosen,
        bool RecoveryAttemptObserved,
        bool PostClickRecaptureObserved);

    private sealed record CombatNoOpLoopAnalysis(
        string DiagnosisKind,
        bool LoopDetected,
        int RepeatedLoopCount,
        string? Phase,
        string? ObserverScreen,
        string? SceneSignature,
        string? BlockedTargetLabel);

    private sealed record HealthEvaluation(
        string HealthState,
        bool RelevantProcessObserved,
        bool WindowDetected,
        bool RunnerOwnerAlive,
        DateTimeOffset? LastArtifactHeartbeatAt,
        string? LastArtifactHeartbeatPath,
        DateTimeOffset? LastStepAt,
        string? LastStepPath,
        string? ExpectedCurrentAttemptId,
        string? ExpectedCurrentAttemptFirstStepPath,
        IReadOnlyList<string> Classifications,
        IReadOnlyList<string> Evidence);

    private sealed record ArtifactTimestamp(string Path, DateTimeOffset RecordedAt);

    private sealed record SupervisorObservationOverride(
        DateTimeOffset? Now = null,
        bool? RelevantProcessObserved = null,
        bool? WindowDetected = null,
        bool? RunnerOwnerAlive = null);

    public static GuiSmokeSessionSummary RefreshSessionSummary(string sessionRoot)
    {
        var goal = LoadOrCreateGoalContract(sessionRoot);
        var attemptEntries = ReadNdjsonRecords<GuiSmokeAttemptIndexEntry>(Path.Combine(sessionRoot, "attempt-index.ndjson"))
            .OrderBy(static entry => entry.AttemptOrdinal)
            .ToArray();
        var restartEvents = ReadNdjsonRecords<GuiSmokeRestartEvent>(GetRestartEventsPath(sessionRoot))
            .OrderBy(static entry => entry.RecordedAt)
            .ThenBy(static entry => entry.AttemptOrdinal)
            .ToArray();
        var chronology = ProjectAttemptChronology(sessionRoot, attemptEntries, restartEvents);
        var sessionEventTimes = restartEvents
            .Select(static entry => (DateTimeOffset?)entry.RecordedAt)
            .Concat(attemptEntries.Select(static entry => (DateTimeOffset?)(entry.CompletedAt ?? entry.StartedAt)))
            .Where(static entry => entry is not null)
            .Select(static entry => entry!.Value)
            .ToArray();
        var startedAt = restartEvents
            .Where(static entry =>
                string.Equals(entry.EventType, GuiSmokeContractStates.EventRunnerLaunchIssued, StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.EventType, GuiSmokeContractStates.EventNextAttemptStarted, StringComparison.OrdinalIgnoreCase))
            .Select(static entry => (DateTimeOffset?)entry.RecordedAt)
            .Concat(attemptEntries.Select(static entry => (DateTimeOffset?)entry.StartedAt))
            .Where(static entry => entry is not null)
            .Select(static entry => entry!.Value)
            .DefaultIfEmpty(goal.CreatedAt)
            .Min();
        var completedAt = attemptEntries
            .Select(static entry => entry.CompletedAt ?? entry.StartedAt)
            .DefaultIfEmpty(sessionEventTimes.LastOrDefault(goal.CreatedAt))
            .Max();
        var lastEventAt = sessionEventTimes.Length == 0
            ? goal.CreatedAt
            : sessionEventTimes.Max();
        var passedAttempts = attemptEntries.Count(static entry =>
            IsPassedAttempt(entry));
        var failedAttempts = attemptEntries.Length - passedAttempts;
        var totalSteps = attemptEntries.Sum(static entry => entry.StepCount);
        var validationEvents = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in attemptEntries)
        {
            var attemptValidationPath = Path.Combine(sessionRoot, "attempts", entry.AttemptId, "validation-summary.json");
            if (!File.Exists(attemptValidationPath))
            {
                continue;
            }

            GuiSmokeValidationSummary? attemptValidation;
            try
            {
                attemptValidation = JsonSerializer.Deserialize<GuiSmokeValidationSummary>(File.ReadAllText(attemptValidationPath), GuiSmokeShared.JsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (attemptValidation is null)
            {
                continue;
            }

            foreach (var pair in attemptValidation.EventCounts)
            {
                validationEvents[pair.Key] = validationEvents.TryGetValue(pair.Key, out var current)
                    ? current + pair.Value
                    : pair.Value;
            }
        }

        var summary = new GuiSmokeSessionSummary(
            SessionId: Path.GetFileName(sessionRoot),
            ScenarioId: goal.ScenarioId,
            Provider: goal.Provider,
            StartedAt: startedAt,
            CompletedAt: completedAt,
            AttemptCount: chronology.StartedAttemptIds.Count,
            TerminalAttemptCount: attemptEntries.Length,
            ActiveAttemptId: chronology.ActiveAttemptId,
            LastEventAt: lastEventAt,
            PassedAttempts: passedAttempts,
            FailedAttempts: failedAttempts,
            TotalSteps: totalSteps,
            ValidationEvents: validationEvents);
        WriteJsonAtomicWithRetry(Path.Combine(sessionRoot, "session-summary.json"), summary, GuiSmokeShared.JsonOptions);
        return summary;
    }

    public static GuiSmokeSupervisorState RefreshSupervisorState(string sessionRoot)
    {
        return RefreshSupervisorStateCore(sessionRoot, null);
    }

    public static GuiSmokeSupervisorState RefreshSupervisorStateForTesting(
        string sessionRoot,
        DateTimeOffset? now = null,
        bool? relevantProcessObserved = null,
        bool? windowDetected = null,
        bool? runnerOwnerAlive = null)
    {
        return RefreshSupervisorStateCore(
            sessionRoot,
            new SupervisorObservationOverride(now, relevantProcessObserved, windowDetected, runnerOwnerAlive));
    }

    public static void RefreshStallSentinel(string sessionRoot)
    {
        var attemptEntries = ReadNdjsonRecords<GuiSmokeAttemptIndexEntry>(Path.Combine(sessionRoot, "attempt-index.ndjson"))
            .OrderBy(static entry => entry.AttemptOrdinal)
            .ToArray();
        var restartEvents = ReadNdjsonRecords<GuiSmokeRestartEvent>(GetRestartEventsPath(sessionRoot))
            .OrderBy(static entry => entry.RecordedAt)
            .ThenBy(static entry => entry.AttemptOrdinal)
            .ToArray();
        var attemptDirectories = Directory.Exists(Path.Combine(sessionRoot, "attempts"))
            ? Directory.GetDirectories(Path.Combine(sessionRoot, "attempts"))
                .Select(Path.GetFileName)
                .Where(static name => !string.IsNullOrWhiteSpace(name))
                .Select(static name => name!)
                .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : Array.Empty<string>();

        foreach (var attemptId in attemptDirectories)
        {
            var indexedAttempt = attemptEntries.LastOrDefault(entry => string.Equals(entry.AttemptId, attemptId, StringComparison.OrdinalIgnoreCase));
            var attemptOrdinal = indexedAttempt?.AttemptOrdinal
                                 ?? restartEvents.LastOrDefault(entry => string.Equals(entry.AttemptId, attemptId, StringComparison.OrdinalIgnoreCase))?.AttemptOrdinal
                                 ?? 0;
            var diagnosisEntry = indexedAttempt ?? new GuiSmokeAttemptIndexEntry(
                attemptId,
                attemptOrdinal,
                RunId: $"{Path.GetFileName(sessionRoot)}-attempt-{attemptId}",
                Status: "in-progress",
                ResultMessage: null,
                StartedAt: DateTimeOffset.UtcNow,
                CompletedAt: null,
                StepCount: CountScreenshots(Path.Combine(sessionRoot, "attempts", attemptId, "steps")),
                TerminalCause: null,
                LaunchFailed: false,
                FailureClass: null,
                TrustStateAtStart: LoadOrCreateGoalContract(sessionRoot).TrustState);
            WriteStallDiagnosis(sessionRoot, Path.Combine(sessionRoot, "attempts", attemptId), diagnosisEntry);
        }
    }

    private static GuiSmokeSupervisorState RefreshSupervisorStateCore(string sessionRoot, SupervisorObservationOverride? observationOverride)
    {
        var now = observationOverride?.Now ?? DateTimeOffset.UtcNow;
        var goal = LoadOrCreateGoalContract(sessionRoot);
        var prevalidation = LoadOrCreatePrevalidation(sessionRoot);
        var attemptEntries = ReadNdjsonRecords<GuiSmokeAttemptIndexEntry>(Path.Combine(sessionRoot, "attempt-index.ndjson"))
            .OrderBy(static entry => entry.AttemptOrdinal)
            .ToArray();
        var restartEvents = ReadNdjsonRecords<GuiSmokeRestartEvent>(GetRestartEventsPath(sessionRoot))
            .OrderBy(static entry => entry.RecordedAt)
            .ThenBy(static entry => entry.AttemptOrdinal)
            .ToArray();
        var chronology = ProjectAttemptChronology(sessionRoot, attemptEntries, restartEvents);
        var trustGateSatisfied = prevalidation.GameStoppedBeforeDeploy
                                 && prevalidation.ModsPayloadReconciled
                                 && prevalidation.DeployIdentityVerified
                                 && prevalidation.ManualCleanBootVerified;
        var trustState = trustGateSatisfied
            ? GuiSmokeContractStates.TrustValid
            : GuiSmokeContractStates.TrustInvalid;
        var milestoneEvaluation = EvaluateMilestone(goal, trustState, attemptEntries, restartEvents, sessionRoot);
        var healthEvaluation = EvaluateHealth(sessionRoot, goal, chronology, restartEvents, now, observationOverride);

        var blockers = new List<string>();
        if (!prevalidation.GameStoppedBeforeDeploy)
        {
            blockers.Add("missing-gate:gameStoppedBeforeDeploy");
        }

        if (!prevalidation.ModsPayloadReconciled)
        {
            blockers.Add("missing-gate:modsPayloadReconciled");
        }

        if (!prevalidation.DeployIdentityVerified)
        {
            blockers.Add("missing-gate:deployIdentityVerified");
        }

        if (!prevalidation.ManualCleanBootVerified)
        {
            blockers.Add("missing-gate:manualCleanBootVerified");
            if (prevalidation.ManualCleanBootEvidence?.BlockingReasons is { Count: > 0 })
            {
                blockers.AddRange(prevalidation.ManualCleanBootEvidence.BlockingReasons.Select(static reason => $"manual-clean-boot:{reason}"));
            }
        }

        blockers.AddRange(milestoneEvaluation.Blockers);
        var goalStateChanged = !string.Equals(goal.TrustState, trustState, StringComparison.OrdinalIgnoreCase)
                               || !string.Equals(goal.MilestoneState, milestoneEvaluation.MilestoneState, StringComparison.OrdinalIgnoreCase);
        var updatedGoal = goalStateChanged
            ? goal with
            {
                UpdatedAt = now,
                TrustState = trustState,
                MilestoneState = milestoneEvaluation.MilestoneState,
            }
            : goal;
        if (goalStateChanged)
        {
            WriteJsonAtomicWithRetry(GetGoalContractPath(sessionRoot), updatedGoal, GuiSmokeShared.JsonOptions);
        }

        var evidence = new List<string>();
        evidence.AddRange(milestoneEvaluation.Evidence);
        evidence.AddRange(healthEvaluation.Evidence);
        evidence.Add($"chronology-source:{GetRestartEventsPath(sessionRoot)}");
        evidence.Add("attempt-index-projection:terminal-summary");
        evidence.Add("session-summary-projection:reviewer-facing");
        evidence.Add("supervisor-state-projection:machine-verdict");
        evidence.Add("last-attempt-id:legacy-alias-of-lastTerminalAttemptId");
        if (prevalidation.DeployEvidence is not null)
        {
            evidence.Add($"deploy-report:{prevalidation.DeployEvidence.ReportPath}");
            evidence.Add($"deploy-report-sha256:{prevalidation.DeployEvidence.ReportSha256}");
        }

        if (prevalidation.ManualCleanBootEvidence is not null)
        {
            evidence.Add($"manual-clean-boot-screen:{prevalidation.ManualCleanBootEvidence.ScreenshotPath}");
            evidence.Add($"manual-clean-boot-first-step:{prevalidation.ManualCleanBootEvidence.FirstStepEligible}");
            evidence.Add($"manual-clean-boot-main-menu:{prevalidation.ManualCleanBootEvidence.MainMenuObserved}");
            evidence.Add($"manual-clean-boot-arm-clear:{prevalidation.ManualCleanBootEvidence.ArmSessionClear}");
            evidence.Add($"manual-clean-boot-actions-clear:{prevalidation.ManualCleanBootEvidence.ActionsQueueClear}");
            evidence.Add($"manual-clean-boot-harness-dormant:{prevalidation.ManualCleanBootEvidence.HarnessDormant}");
            if (prevalidation.ManualCleanBootEvidence.BlockingReasons is { Count: > 0 })
            {
                foreach (var reason in prevalidation.ManualCleanBootEvidence.BlockingReasons)
                {
                    evidence.Add($"manual-clean-boot-blocker:{reason}");
                }
            }
        }

        var startupSummary = LoadOrCreateStartupSummary(sessionRoot);
        if (!string.IsNullOrWhiteSpace(startupSummary.LatestStage))
        {
            evidence.Add($"startup-last-stage:{startupSummary.LatestStage}");
        }

        if (!string.IsNullOrWhiteSpace(startupSummary.LatestStatus))
        {
            evidence.Add($"startup-last-status:{startupSummary.LatestStatus}");
        }

        if (!string.IsNullOrWhiteSpace(startupSummary.DeployMode))
        {
            evidence.Add($"startup-deploy-mode:{startupSummary.DeployMode}");
        }

        if (!string.IsNullOrWhiteSpace(startupSummary.SelectedDeployToolPath))
        {
            evidence.Add($"startup-deploy-tool:{startupSummary.SelectedDeployToolPath}");
        }

        if (startupSummary.DeployCommandExitCode is not null)
        {
            evidence.Add($"deploy-command-exit-code:{startupSummary.DeployCommandExitCode}");
        }

        if (startupSummary.DeployCommandDurationMs is not null)
        {
            evidence.Add($"deploy-command-duration-ms:{startupSummary.DeployCommandDurationMs.Value:F0}");
        }

        if (startupSummary.DeployCommandTimedOut)
        {
            evidence.Add("deploy-command-timeout:true");
            blockers.Add("deploy-command-timeout");
        }

        if (!string.IsNullOrWhiteSpace(startupSummary.DeployCommandFailureReason))
        {
            evidence.Add($"deploy-command-failure:{startupSummary.DeployCommandFailureReason}");
            blockers.Add("deploy-command-failed");
        }

        if (!string.IsNullOrWhiteSpace(startupSummary.FailureStage))
        {
            evidence.Add($"startup-failure-stage:{startupSummary.FailureStage}");
            blockers.Add($"startup-failure:{startupSummary.FailureStage}");
        }

        if (!string.IsNullOrWhiteSpace(startupSummary.FailureReason))
        {
            evidence.Add($"startup-failure-reason:{startupSummary.FailureReason}");
        }

        var startupRuntimeEvidence = TryReadJson<GuiSmokeStartupRuntimeEvidence>(GetStartupRuntimeEvidencePath(sessionRoot));
        if (startupRuntimeEvidence is not null)
        {
            var normalizedStartupDiagnosis = NormalizeStartupRuntimeDiagnosis(startupRuntimeEvidence.Diagnosis);
            evidence.Add($"startup-runtime-diagnosis:{normalizedStartupDiagnosis}");
            evidence.Add($"startup-runtime-failure-edge:{startupRuntimeEvidence.FailureEdge}");
            evidence.Add($"startup-runtime-expected-load-chain:{startupRuntimeEvidence.ExpectedLoadChain}");
            evidence.Add($"startup-runtime-captures:{GetStartupRuntimeCapturesPath(sessionRoot)}");
            evidence.Add($"startup-runtime-capture-count:{startupRuntimeEvidence.CaptureCount}");
            evidence.Add($"startup-runtime-latest-capture-stage:{startupRuntimeEvidence.LatestCaptureStage ?? "null"}");
            evidence.Add($"startup-runtime-latest-capture-reason:{startupRuntimeEvidence.LatestCaptureReason ?? "null"}");
            evidence.Add($"startup-runtime-first-positive-at:{startupRuntimeEvidence.FirstPositiveCaptureAt?.ToString("O") ?? "null"}");
            evidence.Add($"startup-runtime-first-positive-reason:{startupRuntimeEvidence.FirstPositiveReason ?? "null"}");
            evidence.Add($"startup-runtime-ever-reached-main-menu:{startupRuntimeEvidence.EverReachedMainMenu}");
            evidence.Add($"startup-runtime-ever-current-sentinel:{startupRuntimeEvidence.EverSawCurrentExecutionSentinel}");
            evidence.Add($"startup-runtime-ever-runtime-exporter:{startupRuntimeEvidence.EverSawRuntimeExporter}");
            evidence.Add($"startup-runtime-ever-harness-bridge:{startupRuntimeEvidence.EverSawHarnessBridge}");
            evidence.Add($"startup-runtime-ever-fresh-snapshot:{startupRuntimeEvidence.EverSawFreshSnapshot}");
            evidence.Add($"startup-runtime-ever-stale-snapshot:{startupRuntimeEvidence.EverSawStaleSnapshot}");
            evidence.Add($"startup-runtime-ever-loader-signal:{startupRuntimeEvidence.EverSawLoaderSignal}");
            evidence.Add($"startup-runtime-log-present:{startupRuntimeEvidence.RuntimeLogPresent}");
            evidence.Add($"startup-runtime-log-delta-path:{startupRuntimeEvidence.RuntimeLogDeltaPath}");
            evidence.Add($"startup-runtime-log-delta-matches:{startupRuntimeEvidence.RuntimeLogDeltaMatches.Count}");
            evidence.Add($"startup-module-initializer-logged:{startupRuntimeEvidence.ModuleInitializerBootstrapLogged}");
            evidence.Add($"startup-runtime-exporter-logged:{startupRuntimeEvidence.RuntimeExporterInitializedLogged}");
            evidence.Add($"startup-harness-bridge-logged:{startupRuntimeEvidence.HarnessBridgeInitializeLogged}");
            evidence.Add($"startup-godot-log-present:{startupRuntimeEvidence.GodotLogPresent}");
            evidence.Add($"startup-godot-log-delta-path:{startupRuntimeEvidence.GodotLogDeltaPath}");
            evidence.Add($"startup-godot-log-delta-matches:{startupRuntimeEvidence.GodotLogDeltaMatches.Count}");
            evidence.Add($"startup-godot-reached-main-menu:{startupRuntimeEvidence.GodotReachedMainMenu}");
            evidence.Add($"startup-loader-any-signal:{startupRuntimeEvidence.LoaderSawAnyModLoaderSignal}");
            evidence.Add($"startup-loader-mod-scan-seen:{startupRuntimeEvidence.LoaderSawModsDirectoryScan}");
            evidence.Add($"startup-loader-companion-pck-scan:{startupRuntimeEvidence.LoaderSawCompanionPckScan}");
            evidence.Add($"startup-loader-companion-seen:{startupRuntimeEvidence.LoaderSawCompanionAssembly}");
            evidence.Add($"startup-loader-initializer-call-seen:{startupRuntimeEvidence.LoaderSawInitializerCall}");
            evidence.Add($"startup-loader-no-modinitializer-seen:{startupRuntimeEvidence.LoaderSawNoModInitializerAttribute}");
            evidence.Add($"startup-loader-patchall-seen:{startupRuntimeEvidence.LoaderSawPatchAll}");
            evidence.Add($"startup-loader-patchall-failure:{startupRuntimeEvidence.LoaderSawPatchAllFailure}");
            evidence.Add($"startup-loader-mod-load-failure:{startupRuntimeEvidence.LoaderSawModLoadFailure}");
            evidence.Add($"startup-settings-path:{startupRuntimeEvidence.SettingsPath}");
            evidence.Add($"startup-settings-present:{startupRuntimeEvidence.SettingsPresent}");
            evidence.Add($"startup-settings-mods-enabled:{startupRuntimeEvidence.SettingsModsEnabled}");
            evidence.Add($"startup-settings-companion-disabled:{startupRuntimeEvidence.SettingsCompanionDisabled}");
            evidence.Add($"startup-runtime-config-path:{startupRuntimeEvidence.RuntimeConfigPath}");
            evidence.Add($"startup-runtime-config-present:{startupRuntimeEvidence.RuntimeConfigPresent}");
            evidence.Add($"startup-runtime-config-enabled:{startupRuntimeEvidence.RuntimeConfigEnabled}");
            evidence.Add($"startup-runtime-config-harness-enabled:{startupRuntimeEvidence.RuntimeConfigHarnessEnabled}");
            evidence.Add($"startup-sentinel-session-id:{startupRuntimeEvidence.StartupSentinelSessionId ?? "null"}");
            evidence.Add($"startup-sentinel-run-id:{startupRuntimeEvidence.StartupSentinelRunId ?? "null"}");
            evidence.Add($"startup-sentinel-launch-token:{startupRuntimeEvidence.StartupSentinelLaunchToken ?? "null"}");
            evidence.Add($"startup-sentinel-relative-path:{startupRuntimeEvidence.StartupSentinelRelativePath ?? "null"}");
            evidence.Add($"startup-sentinel-path:{startupRuntimeEvidence.SentinelPath ?? "null"}");
            evidence.Add($"startup-sentinel-present:{startupRuntimeEvidence.SentinelPresent}");
            evidence.Add($"startup-sentinel-last-write-at:{startupRuntimeEvidence.SentinelLastWriteAt?.ToString("O") ?? "null"}");
            evidence.Add($"startup-sentinel-session-match:{startupRuntimeEvidence.SentinelSessionMatch}");
            evidence.Add($"startup-sentinel-run-match:{startupRuntimeEvidence.SentinelRunMatch}");
            evidence.Add($"startup-sentinel-launch-token-match:{startupRuntimeEvidence.SentinelLaunchTokenMatch}");
            evidence.Add($"startup-companion-pck-present:{startupRuntimeEvidence.CompanionPckPresent}");
            evidence.Add($"startup-companion-dll-present:{startupRuntimeEvidence.CompanionDllPresent}");
            evidence.Add($"startup-companion-runtime-config-present:{startupRuntimeEvidence.CompanionRuntimeConfigPresent}");
            evidence.Add($"startup-live-snapshot-present:{startupRuntimeEvidence.LiveSnapshotPresent}");
            evidence.Add($"startup-fresh-snapshot-present:{startupRuntimeEvidence.FreshSnapshotPresent}");
            evidence.Add($"startup-stale-snapshot-observed:{startupRuntimeEvidence.StaleSnapshotObserved}");
            evidence.Add($"startup-no-snapshot-evidence:{startupRuntimeEvidence.NoSnapshotEvidence}");
            evidence.Add($"startup-harness-inventory-present:{startupRuntimeEvidence.HarnessInventoryPresent}");
            evidence.Add($"startup-harness-status-present:{startupRuntimeEvidence.HarnessStatusPresent}");
            evidence.Add($"startup-runtime-evidence:{GetStartupRuntimeEvidencePath(sessionRoot)}");

            if (string.Equals(normalizedStartupDiagnosis, "runtime-bootstrap-missing", StringComparison.OrdinalIgnoreCase))
            {
                blockers.Add("startup-runtime-bootstrap-missing");
            }
            else if (string.Equals(normalizedStartupDiagnosis, "observer-bootstrap-bridge-missing", StringComparison.OrdinalIgnoreCase))
            {
                blockers.Add("startup-observer-bootstrap-bridge-missing");
            }
            else if (string.Equals(normalizedStartupDiagnosis, "loader-entry-before-initializer-not-proven", StringComparison.OrdinalIgnoreCase))
            {
                blockers.Add("startup-loader-entry-before-initializer-not-proven");
            }

            if (!string.IsNullOrWhiteSpace(startupRuntimeEvidence.FailureEdge)
                && !string.Equals(normalizedStartupDiagnosis, "runtime-started-snapshots-present", StringComparison.OrdinalIgnoreCase))
            {
                blockers.Add($"startup-load-chain-failure:{startupRuntimeEvidence.FailureEdge}");
            }
        }

        var supervisorState = new GuiSmokeSupervisorState(
            now,
            updatedGoal.SessionId,
            trustState,
            milestoneEvaluation.MilestoneState,
            updatedGoal.SessionState,
            healthEvaluation.HealthState,
            trustGateSatisfied,
            healthEvaluation.RelevantProcessObserved,
            healthEvaluation.WindowDetected,
            healthEvaluation.RunnerOwnerAlive,
            healthEvaluation.LastArtifactHeartbeatAt,
            healthEvaluation.LastArtifactHeartbeatPath,
            healthEvaluation.LastStepAt,
            healthEvaluation.LastStepPath,
            healthEvaluation.ExpectedCurrentAttemptId,
            healthEvaluation.ExpectedCurrentAttemptFirstStepPath,
            chronology.LastTerminalAttemptId,
            chronology.LastTerminalAttemptId,
            chronology.LastTerminalCause,
            chronology.LatestRestartTargetAttemptId,
            chronology.LatestNextAttemptId,
            chronology.LatestNextAttemptFirstScreenPath,
            healthEvaluation.Classifications,
            evidence,
            blockers);
        WriteJsonAtomicWithRetry(GetSupervisorStatePath(sessionRoot), supervisorState, GuiSmokeShared.JsonOptions);
        return supervisorState;
    }

    private static HealthEvaluation EvaluateHealth(
        string sessionRoot,
        GuiSmokeGoalContract goal,
        AttemptChronologyProjection chronology,
        IReadOnlyList<GuiSmokeRestartEvent> restartEvents,
        DateTimeOffset now,
        SupervisorObservationOverride? observationOverride)
    {
        var activeSession = goal.SessionState is GuiSmokeContractStates.SessionStarting or GuiSmokeContractStates.SessionCollecting or GuiSmokeContractStates.SessionStalled;
        var relevantProcessObserved = observationOverride?.RelevantProcessObserved ?? ObserveRelevantProcessHealth();
        var windowDetected = observationOverride?.WindowDetected ?? ObserveGameWindow();
        var runnerOwnerAlive = observationOverride?.RunnerOwnerAlive ?? IsRunnerOwnerAlive(goal.RunnerOwner);
        var currentAttemptId = chronology.ActiveAttemptId;
        var expectedCurrentAttemptFirstStepPath = string.IsNullOrWhiteSpace(currentAttemptId)
            ? null
            : Path.Combine(sessionRoot, "attempts", currentAttemptId, "steps", "0001.screen.png");
        var runnerArtifacts = CollectRunnerHeartbeatArtifacts(sessionRoot, currentAttemptId);
        var lastArtifact = runnerArtifacts
            .OrderByDescending(static entry => entry.RecordedAt)
            .FirstOrDefault();
        var lastStep = CollectStepArtifacts(sessionRoot)
            .OrderByDescending(static entry => entry.RecordedAt)
            .FirstOrDefault();
        var classifications = new List<string>();
        var evidence = new List<string>
        {
            $"runner-owner-pid:{goal.RunnerOwner.ProcessId}",
            $"runner-owner-alive:{runnerOwnerAlive}",
            $"relevant-process-observed:{relevantProcessObserved}",
            $"window-detected:{windowDetected}",
        };

        if (lastArtifact is not null)
        {
            evidence.Add($"artifact-heartbeat:{lastArtifact.Path}");
            evidence.Add($"artifact-heartbeat-at:{lastArtifact.RecordedAt:O}");
        }
        else
        {
            evidence.Add("artifact-heartbeat:none");
        }

        if (lastStep is not null)
        {
            evidence.Add($"last-step:{lastStep.Path}");
            evidence.Add($"last-step-at:{lastStep.RecordedAt:O}");
        }
        else
        {
            evidence.Add("last-step:none");
        }

        if (activeSession && !runnerOwnerAlive)
        {
            classifications.Add("runner-dead");
        }

        if (activeSession && (lastArtifact is null || now - lastArtifact.RecordedAt > NoArtifactHeartbeatThreshold))
        {
            classifications.Add("no-artifact-heartbeat");
        }

        var latestLaunchOrStartAt = restartEvents
            .Where(eventEntry =>
                string.Equals(eventEntry.AttemptId, currentAttemptId, StringComparison.OrdinalIgnoreCase)
                && (string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventRunnerLaunchIssued, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventNextAttemptStarted, StringComparison.OrdinalIgnoreCase)))
            .Select(static eventEntry => (DateTimeOffset?)eventEntry.RecordedAt)
            .DefaultIfEmpty(null)
            .Max();
        if (activeSession
            && windowDetected
            && !string.IsNullOrWhiteSpace(expectedCurrentAttemptFirstStepPath)
            && !File.Exists(expectedCurrentAttemptFirstStepPath)
            && latestLaunchOrStartAt is not null
            && now - latestLaunchOrStartAt.Value > WindowNoStepThreshold)
        {
            classifications.Add("window-detected-no-step");
            evidence.Add($"expected-current-attempt-first-step:{expectedCurrentAttemptFirstStepPath}");
        }

        var healthState = classifications.Count == 0
            ? GuiSmokeContractStates.HealthHealthy
            : classifications.Contains("runner-dead", StringComparer.OrdinalIgnoreCase)
                ? GuiSmokeContractStates.HealthCritical
                : GuiSmokeContractStates.HealthWarning;
        return new HealthEvaluation(
            healthState,
            relevantProcessObserved,
            windowDetected,
            runnerOwnerAlive,
            lastArtifact?.RecordedAt,
            lastArtifact?.Path,
            lastStep?.RecordedAt,
            lastStep?.Path,
            currentAttemptId,
            expectedCurrentAttemptFirstStepPath,
            classifications,
            evidence);
    }

    private static IReadOnlyList<ArtifactTimestamp> CollectRunnerHeartbeatArtifacts(string sessionRoot, string? currentAttemptId)
    {
        var paths = new List<string>
        {
            GetPrevalidationPath(sessionRoot),
            GetRestartEventsPath(sessionRoot),
            Path.Combine(sessionRoot, "attempt-index.ndjson"),
            Path.Combine(sessionRoot, "session-summary.json"),
        };

        if (!string.IsNullOrWhiteSpace(currentAttemptId))
        {
            var attemptRoot = Path.Combine(sessionRoot, "attempts", currentAttemptId);
            paths.Add(Path.Combine(attemptRoot, "run.json"));
            paths.Add(Path.Combine(attemptRoot, "run.log"));
            paths.Add(Path.Combine(attemptRoot, "trace.ndjson"));
            paths.Add(Path.Combine(attemptRoot, "progress.ndjson"));
        }

        var timestamps = new List<ArtifactTimestamp>();
        foreach (var path in paths.Where(File.Exists))
        {
            timestamps.Add(new ArtifactTimestamp(path, new FileInfo(path).LastWriteTimeUtc));
        }

        return timestamps;
    }

    private static IReadOnlyList<ArtifactTimestamp> CollectStepArtifacts(string sessionRoot)
    {
        var attemptsRoot = Path.Combine(sessionRoot, "attempts");
        if (!Directory.Exists(attemptsRoot))
        {
            return Array.Empty<ArtifactTimestamp>();
        }

        return Directory.GetFiles(attemptsRoot, "*.screen.png", SearchOption.AllDirectories)
            .Select(path => new ArtifactTimestamp(path, new FileInfo(path).LastWriteTimeUtc))
            .ToArray();
    }

    private static AttemptChronologyProjection ProjectAttemptChronology(
        string sessionRoot,
        IReadOnlyList<GuiSmokeAttemptIndexEntry> attemptEntries,
        IReadOnlyList<GuiSmokeRestartEvent> restartEvents)
    {
        var startedAttemptIds = new List<string>();
        var startedAttemptSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string? activeAttemptId = null;
        string? lastTerminalAttemptId = null;
        string? latestRestartTargetAttemptId = null;
        string? latestNextAttemptId = null;
        string? latestNextAttemptFirstScreenPath = null;

        foreach (var eventEntry in restartEvents)
        {
            if (string.IsNullOrWhiteSpace(eventEntry.AttemptId))
            {
                continue;
            }

            if (string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventRunnerLaunchIssued, StringComparison.OrdinalIgnoreCase))
            {
                AddStartedAttemptId(startedAttemptIds, startedAttemptSet, eventEntry.AttemptId);
                activeAttemptId = eventEntry.AttemptId;
                continue;
            }

            if (string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventNextAttemptStarted, StringComparison.OrdinalIgnoreCase))
            {
                AddStartedAttemptId(startedAttemptIds, startedAttemptSet, eventEntry.AttemptId);
                activeAttemptId = eventEntry.AttemptId;
                latestNextAttemptId = eventEntry.AttemptId;
                latestNextAttemptFirstScreenPath = string.IsNullOrWhiteSpace(eventEntry.StepScreenPath)
                    ? Path.Combine(sessionRoot, "attempts", eventEntry.AttemptId, "steps", "0001.screen.png")
                    : eventEntry.StepScreenPath;
                continue;
            }

            if (string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventAttemptTerminal, StringComparison.OrdinalIgnoreCase))
            {
                lastTerminalAttemptId = eventEntry.AttemptId;
                if (string.Equals(activeAttemptId, eventEntry.AttemptId, StringComparison.OrdinalIgnoreCase))
                {
                    activeAttemptId = null;
                }

                continue;
            }

            if (string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventRunnerBeginRestart, StringComparison.OrdinalIgnoreCase))
            {
                latestRestartTargetAttemptId = eventEntry.AttemptId;
            }
        }

        foreach (var attemptEntry in attemptEntries)
        {
            AddStartedAttemptId(startedAttemptIds, startedAttemptSet, attemptEntry.AttemptId);
        }

        lastTerminalAttemptId ??= attemptEntries.LastOrDefault()?.AttemptId;
        var lastTerminalCause = ResolveLastTerminalCause(attemptEntries, restartEvents, lastTerminalAttemptId);
        return new AttemptChronologyProjection(
            startedAttemptIds,
            activeAttemptId,
            lastTerminalAttemptId,
            lastTerminalCause,
            latestRestartTargetAttemptId,
            latestNextAttemptId,
            latestNextAttemptFirstScreenPath);
    }

    private static void AddStartedAttemptId(ICollection<string> startedAttemptIds, ISet<string> startedAttemptSet, string? attemptId)
    {
        if (string.IsNullOrWhiteSpace(attemptId) || !startedAttemptSet.Add(attemptId))
        {
            return;
        }

        startedAttemptIds.Add(attemptId);
    }

    private static string? ResolveLastTerminalCause(
        IReadOnlyList<GuiSmokeAttemptIndexEntry> attemptEntries,
        IReadOnlyList<GuiSmokeRestartEvent> restartEvents,
        string? lastTerminalAttemptId)
    {
        if (string.IsNullOrWhiteSpace(lastTerminalAttemptId))
        {
            return null;
        }

        var attemptIndexCause = attemptEntries.LastOrDefault(entry =>
            string.Equals(entry.AttemptId, lastTerminalAttemptId, StringComparison.OrdinalIgnoreCase))?.TerminalCause;
        if (!string.IsNullOrWhiteSpace(attemptIndexCause))
        {
            return attemptIndexCause;
        }

        return restartEvents.LastOrDefault(eventEntry =>
            string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventAttemptTerminal, StringComparison.OrdinalIgnoreCase)
            && string.Equals(eventEntry.AttemptId, lastTerminalAttemptId, StringComparison.OrdinalIgnoreCase))?.TerminalCause;
    }
    private static MilestoneEvaluation EvaluateMilestone(
        GuiSmokeGoalContract goal,
        string trustState,
        IReadOnlyList<GuiSmokeAttemptIndexEntry> attemptEntries,
        IReadOnlyList<GuiSmokeRestartEvent> restartEvents,
        string sessionRoot)
    {
        var priorState = goal.MilestoneState;
        if (string.Equals(priorState, GuiSmokeContractStates.MilestoneDone, StringComparison.OrdinalIgnoreCase))
        {
            return new MilestoneEvaluation(
                GuiSmokeContractStates.MilestoneDone,
                null,
                null,
                null,
                null,
                null,
                new[] { "milestone:done-persisted" },
                Array.Empty<string>());
        }

        if (string.Equals(trustState, GuiSmokeContractStates.TrustInvalid, StringComparison.OrdinalIgnoreCase))
        {
            return new MilestoneEvaluation(
                GuiSmokeContractStates.GetMilestoneRank(priorState) > 0 ? priorState : GuiSmokeContractStates.MilestoneInProgress,
                null,
                null,
                null,
                null,
                null,
                Array.Empty<string>(),
                new[] { "trust-gate-invalid" });
        }

        var bestState = GuiSmokeContractStates.MilestoneInProgress;
        string? lastTerminalAttemptId = null;
        string? lastTerminalCause = null;
        string? latestRestartTargetAttemptId = null;
        string? latestNextAttemptId = null;
        string? latestNextAttemptFirstScreenPath = null;
        var evidence = new List<string>();
        var blockers = new List<string>();

        foreach (var attemptEntry in attemptEntries)
        {
            if (!string.Equals(attemptEntry.TrustStateAtStart, GuiSmokeContractStates.TrustValid, StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(attemptEntry.TerminalCause))
            {
                continue;
            }

            var attemptTerminal = restartEvents.LastOrDefault(eventEntry =>
                string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventAttemptTerminal, StringComparison.OrdinalIgnoreCase)
                && string.Equals(eventEntry.AttemptId, attemptEntry.AttemptId, StringComparison.OrdinalIgnoreCase));
            if (attemptTerminal is null)
            {
                continue;
            }

            bestState = PromoteMilestoneState(bestState, GuiSmokeContractStates.MilestoneTerminalSeen);
            lastTerminalAttemptId = attemptEntry.AttemptId;
            lastTerminalCause = attemptEntry.TerminalCause;
            evidence.Add($"attempt-terminal:{attemptEntry.AttemptId}:{attemptEntry.TerminalCause}");

            var restartProgress = restartEvents.LastOrDefault(eventEntry =>
                string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventRunnerBeginRestart, StringComparison.OrdinalIgnoreCase)
                && string.Equals(eventEntry.PreviousAttemptId, attemptEntry.AttemptId, StringComparison.OrdinalIgnoreCase));
            if (restartProgress is null)
            {
                blockers.Add($"restart-missing-after:{attemptEntry.AttemptId}");
                continue;
            }

            bestState = PromoteMilestoneState(bestState, GuiSmokeContractStates.MilestoneRestartSeen);
            latestRestartTargetAttemptId = restartProgress.AttemptId;
            evidence.Add($"runner-begin-restart:{restartProgress.AttemptId}");

            var launchIssued = restartEvents.LastOrDefault(eventEntry =>
                string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventRunnerLaunchIssued, StringComparison.OrdinalIgnoreCase)
                && string.Equals(eventEntry.AttemptId, restartProgress.AttemptId, StringComparison.OrdinalIgnoreCase));
            if (launchIssued is not null)
            {
                evidence.Add($"runner-launch-issued:{launchIssued.AttemptId}");
            }

            var nextAttemptStarted = restartEvents.LastOrDefault(eventEntry =>
                string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventNextAttemptStarted, StringComparison.OrdinalIgnoreCase)
                && string.Equals(eventEntry.AttemptId, restartProgress.AttemptId, StringComparison.OrdinalIgnoreCase));
            if (nextAttemptStarted is null)
            {
                blockers.Add($"next-attempt-start-missing:{restartProgress.AttemptId}");
                continue;
            }

            latestNextAttemptId = nextAttemptStarted.AttemptId;
            latestNextAttemptFirstScreenPath = string.IsNullOrWhiteSpace(nextAttemptStarted.StepScreenPath)
                ? Path.Combine(sessionRoot, "attempts", nextAttemptStarted.AttemptId, "steps", "0001.screen.png")
                : nextAttemptStarted.StepScreenPath;
            if (!File.Exists(latestNextAttemptFirstScreenPath))
            {
                blockers.Add($"next-attempt-first-screen-missing:{latestNextAttemptId}");
                continue;
            }

            evidence.Add($"next-attempt-started:{latestNextAttemptId}");
            evidence.Add($"next-attempt-first-screen:{latestNextAttemptFirstScreenPath}");
            bestState = PromoteMilestoneState(bestState, GuiSmokeContractStates.MilestoneDone);
        }

        if (!string.Equals(bestState, GuiSmokeContractStates.MilestoneDone, StringComparison.OrdinalIgnoreCase)
            && string.Equals(goal.SessionState, GuiSmokeContractStates.SessionAborted, StringComparison.OrdinalIgnoreCase))
        {
            bestState = GuiSmokeContractStates.MilestoneFailed;
            blockers.Add("session-aborted-before-milestone");
        }

        if (GuiSmokeContractStates.GetMilestoneRank(priorState) > GuiSmokeContractStates.GetMilestoneRank(bestState))
        {
            bestState = priorState;
        }

        return new MilestoneEvaluation(
            bestState,
            lastTerminalAttemptId,
            lastTerminalCause,
            latestRestartTargetAttemptId,
            latestNextAttemptId,
            latestNextAttemptFirstScreenPath,
            evidence,
            blockers);
    }

    private static string PromoteMilestoneState(string current, string candidate)
    {
        return GuiSmokeContractStates.GetMilestoneRank(candidate) > GuiSmokeContractStates.GetMilestoneRank(current)
            ? candidate
            : current;
    }
    private static bool ObserveRelevantProcessHealth()
    {
        try
        {
            return Process.GetProcesses().Any(process =>
                string.Equals(process.ProcessName, "SlayTheSpire2", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "crashpad_handler", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    private static bool ObserveGameWindow()
    {
        try
        {
            return WindowLocator.TryFindSts2Window() is not null;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsRunnerOwnerAlive(GuiSmokeRunnerOwner owner)
    {
        try
        {
            var process = Process.GetProcessById(owner.ProcessId);
            if (process.HasExited)
            {
                return false;
            }

            return string.Equals(process.ProcessName, owner.ProcessName, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static GuiSmokeRunnerOwner CreateRunnerOwner(DateTimeOffset claimedAt)
    {
        return new GuiSmokeRunnerOwner(
            Environment.MachineName,
            Environment.ProcessId,
            GetCurrentProcessName(),
            claimedAt);
    }

    private static string GetCurrentProcessName()
    {
        try
        {
            return Process.GetCurrentProcess().ProcessName;
        }
        catch
        {
            return "Sts2GuiSmokeHarness";
        }
    }

    private static IReadOnlyList<string> GetRunningRelevantProcesses()
    {
        try
        {
            return Process.GetProcesses()
                .Where(process =>
                    string.Equals(process.ProcessName, "SlayTheSpire2", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(process.ProcessName, "crashpad_handler", StringComparison.OrdinalIgnoreCase))
                .Select(static process => process.ProcessName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static bool HasActiveArmSession(string armSessionPath)
    {
        if (!File.Exists(armSessionPath))
        {
            return false;
        }

        var armSession = TryReadJson<HarnessArmSession>(armSessionPath);
        return armSession is null || armSession.ExpiresAt > DateTimeOffset.UtcNow;
    }

    private static bool HasPendingHarnessActions(string actionsPath)
    {
        if (!File.Exists(actionsPath))
        {
            return false;
        }

        try
        {
            return File.ReadLines(actionsPath).Any(static line => !string.IsNullOrWhiteSpace(line));
        }
        catch
        {
            return true;
        }
    }

    private static bool IsHarnessDormant(HarnessQueueLayout harnessLayout)
    {
        var status = TryReadJson<HarnessBridgeStatus>(harnessLayout.StatusPath);
        if (status is not null && !string.Equals(status.Mode, "dormant", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var inventory = TryReadJson<HarnessNodeInventory>(harnessLayout.InventoryPath);
        return inventory is null || string.Equals(inventory.Mode, "dormant", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCompanionFamilyFile(string fileName)
    {
        return fileName.StartsWith("Sts2ModAiCompanion", StringComparison.OrdinalIgnoreCase)
               || fileName.StartsWith("Sts2AiCompanion", StringComparison.OrdinalIgnoreCase)
               || fileName.StartsWith("Sts2ModKit", StringComparison.OrdinalIgnoreCase)
               || string.Equals(fileName, "runtime-assembly-manifest.json", StringComparison.OrdinalIgnoreCase)
               || string.Equals(fileName, "sts2-mod-ai-companion.config.json", StringComparison.OrdinalIgnoreCase)
               || string.Equals(fileName, "sts2-mod-ai-companion.runtime-config", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryMatchIntentionalRewrite(
        string relativePath,
        string sourcePath,
        string deployedPath,
        out string? rewriteNote)
    {
        rewriteNote = null;
        if (!string.Equals(relativePath, "sts2-mod-ai-companion.runtime-config", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var normalizedSource = TryNormalizeRuntimeConfigForDeployVerification(sourcePath);
        var normalizedDeployed = TryNormalizeRuntimeConfigForDeployVerification(deployedPath);
        if (normalizedSource is null || normalizedDeployed is null)
        {
            return false;
        }

        var sourceHash = ComputeSha256Utf8(normalizedSource);
        var deployedHash = ComputeSha256Utf8(normalizedDeployed);
        if (!string.Equals(sourceHash, deployedHash, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        rewriteNote = $"rewrite-normalized-match:{relativePath}:{deployedHash}";
        return true;
    }
}
