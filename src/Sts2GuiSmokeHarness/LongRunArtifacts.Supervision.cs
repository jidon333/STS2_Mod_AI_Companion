using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;

static class GuiSmokeContractStates
{
    public const string TrustInvalid = "invalid";
    public const string TrustValid = "valid";

    public const string MilestoneInProgress = "in_progress";
    public const string MilestoneTerminalSeen = "terminal_seen";
    public const string MilestoneRestartSeen = "restart_seen";
    public const string MilestoneDone = "done";
    public const string MilestoneFailed = "failed";

    public const string SessionStarting = "starting";
    public const string SessionCollecting = "collecting";
    public const string SessionStalled = "stalled";
    public const string SessionAborted = "aborted";
    public const string SessionCompleted = "completed";

    public const string EventAttemptTerminal = "attempt-terminal";
    public const string EventRunnerBeginRestart = "runner-begin-restart";
    public const string EventRunnerLaunchIssued = "runner-launch-issued";
    public const string EventNextAttemptStarted = "next-attempt-started";

    public const string HealthHealthy = "healthy";
    public const string HealthWarning = "warning";
    public const string HealthCritical = "critical";

    public static int GetMilestoneRank(string state)
    {
        return state switch
        {
            MilestoneTerminalSeen => 1,
            MilestoneRestartSeen => 2,
            MilestoneDone => 3,
            MilestoneFailed => 4,
            _ => 0,
        };
    }
}

sealed record GuiSmokeRunnerOwner(
    string HostName,
    int ProcessId,
    string ProcessName,
    DateTimeOffset ClaimedAt);

sealed record GuiSmokeGoalContract(
    string SessionId,
    string ScenarioId,
    string Provider,
    string SessionRoot,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string TrustState,
    string MilestoneState,
    string SessionState,
    GuiSmokeRunnerOwner RunnerOwner,
    DateTimeOffset LastRunnerHeartbeatAt,
    DateTimeOffset? CompletedAt,
    string? CompletedBy,
    IReadOnlyList<string> CompletionCriteria,
    IReadOnlyList<string> OperationalRules);

sealed record GuiSmokeProcessStopEvidence(
    DateTimeOffset VerifiedAt,
    bool WindowDetected,
    IReadOnlyList<string> RunningProcesses);

sealed record GuiSmokeFileIdentityEvidence(
    string Path,
    long Size,
    DateTimeOffset LastWriteUtc,
    string Sha256);

sealed record GuiSmokeDeployFileEvidence(
    string RelativePath,
    GuiSmokeFileIdentityEvidence Source,
    GuiSmokeFileIdentityEvidence Deployed);

sealed record GuiSmokeDeployEvidence(
    DateTimeOffset VerifiedAt,
    string ReportPath,
    string ReportSha256,
    string? SourcePackageRoot,
    string? DeployedRoot,
    IReadOnlyList<GuiSmokeDeployFileEvidence> VerifiedFiles,
    IReadOnlyList<string> MissingFiles,
    IReadOnlyList<string> HashMismatches,
    IReadOnlyList<string> UnexpectedFamilyFiles,
    IReadOnlyList<string> Notes);

sealed record GuiSmokeManualCleanBootEvidence(
    DateTimeOffset VerifiedAt,
    string ScreenshotPath,
    string ScreenshotSha256,
    string? ObserverStatePath,
    string? ObserverStateSha256,
    string? HarnessStatusPath,
    string? HarnessStatusSha256,
    string? HarnessInventoryPath,
    string? HarnessInventorySha256,
    string ArmSessionPath,
    bool ArmSessionPresent,
    string ActionsPath,
    bool ActionsPending,
    string? HarnessStatusMode,
    string? HarnessInventoryMode,
    string? ObservedScreen,
    bool FirstStepEligible = false,
    bool MainMenuObserved = false,
    bool ArmSessionClear = false,
    bool ActionsQueueClear = false,
    bool HarnessDormant = false,
    string? LastActionId = null,
    string? LastResultStatus = null,
    IReadOnlyList<string>? BlockingReasons = null,
    IReadOnlyList<string>? EvaluationNotes = null);

sealed record GuiSmokePrevalidation(
    string SessionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    bool GameStoppedBeforeDeploy,
    bool ModsPayloadReconciled,
    bool DeployIdentityVerified,
    bool ManualCleanBootVerified,
    GuiSmokeProcessStopEvidence? GameStopEvidence,
    GuiSmokeDeployEvidence? DeployEvidence,
    GuiSmokeManualCleanBootEvidence? ManualCleanBootEvidence,
    IReadOnlyList<string> Notes);

sealed record GuiSmokeStartupTraceEntry(
    DateTimeOffset RecordedAt,
    string SessionId,
    string Stage,
    string Status,
    string? Detail,
    IReadOnlyDictionary<string, string?> Metadata);

sealed record GuiSmokeStartupSummary(
    string SessionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? LatestStage,
    string? LatestStatus,
    bool GameStoppedBeforeDeployRecorded,
    bool DeployCommandSelected,
    string? DeployMode,
    string? SelectedDeployToolPath,
    string? SelectedDeployReason,
    bool DeployCommandStarted,
    bool DeployCommandFinished,
    int? DeployCommandExitCode,
    bool DeployCommandTimedOut,
    double? DeployCommandDurationMs,
    string? DeployCommandFailureReason,
    bool DeployVerificationStarted,
    bool DeployVerificationFinished,
    bool LaunchIssued,
    bool WindowDetected,
    bool ManualCleanBootEvaluationStarted,
    bool ManualCleanBootEvaluationFinished,
    bool FirstAttemptCreated,
    bool FirstScreenshotCaptured,
    string? FailureStage,
    string? FailureReason);

sealed record GuiSmokeStartupLogBaseline(
    DateTimeOffset RecordedAt,
    string SessionId,
    string RunId,
    string LaunchToken,
    DateTimeOffset LaunchIssuedAtUtc,
    string RuntimeLogPath,
    bool RuntimeLogPresent,
    long RuntimeLogSizeBytes,
    DateTimeOffset? RuntimeLogLastWriteAt,
    string GodotLogPath,
    bool GodotLogPresent,
    long GodotLogSizeBytes,
    DateTimeOffset? GodotLogLastWriteAt);

sealed record GuiSmokeStartupRuntimeEvidence(
    DateTimeOffset CapturedAt,
    string ExpectedLoadChain,
    string FailureEdge,
    string ModsRoot,
    bool CompanionPckPresent,
    bool CompanionDllPresent,
    bool CompanionRuntimeConfigPresent,
    string RuntimeConfigPath,
    bool RuntimeConfigPresent,
    bool RuntimeConfigEnabled,
    bool RuntimeConfigHarnessEnabled,
    string SettingsPath,
    bool SettingsPresent,
    bool SettingsModsEnabled,
    bool SettingsCompanionDisabled,
    string PackagedManifestPath,
    bool PackagedManifestPresent,
    string? PackagedManifestPckName,
    string? ExpectedAssemblyFileName,
    string StartupLogBaselinePath,
    DateTimeOffset? StartupLogBaselineRecordedAt,
    string? StartupSentinelSessionId,
    string? StartupSentinelRunId,
    string? StartupSentinelLaunchToken,
    DateTimeOffset? StartupSentinelLaunchIssuedAtUtc,
    string? StartupSentinelRelativePath,
    string RuntimeLogPath,
    bool RuntimeLogPresent,
    DateTimeOffset? RuntimeLogLastWriteAt,
    string RuntimeLogTailPath,
    IReadOnlyList<string> RuntimeLogMatches,
    string RuntimeLogDeltaPath,
    IReadOnlyList<string> RuntimeLogDeltaMatches,
    bool RuntimeLogDeltaTreatedAsCurrentExecution,
    bool ModuleInitializerBootstrapLogged,
    bool RuntimeExporterInitializedLogged,
    bool HarnessBridgeInitializeLogged,
    string GodotLogPath,
    bool GodotLogPresent,
    DateTimeOffset? GodotLogLastWriteAt,
    string GodotLogTailPath,
    IReadOnlyList<string> GodotLogMatches,
    string GodotLogDeltaPath,
    IReadOnlyList<string> GodotLogDeltaMatches,
    bool GodotLogDeltaTreatedAsCurrentExecution,
    bool GodotReachedMainMenu,
    bool LoaderSawAnyModLoaderSignal,
    bool LoaderSawModsDirectoryScan,
    bool LoaderSawCompanionPckScan,
    bool LoaderSawCompanionSkippedWarning,
    bool LoaderSawCompanionDisabled,
    bool LoaderSawCompanionDuplicate,
    bool LoaderSawCompanionAssembly,
    bool LoaderSawInitializerCall,
    bool LoaderSawNoModInitializerAttribute,
    bool LoaderSawModInitialization,
    bool LoaderSawPatchAll,
    bool LoaderSawPatchAllFailure,
    bool LoaderSawModLoadFailure,
    string LiveSnapshotPath,
    bool LiveSnapshotPresent,
    string HarnessInventoryPath,
    bool HarnessInventoryPresent,
    string HarnessStatusPath,
    bool HarnessStatusPresent,
    string Diagnosis);

sealed record GuiSmokeDeployCommandSummary(
    DateTimeOffset RecordedAt,
    string SessionId,
    string Mode,
    string FileName,
    string Arguments,
    string? ToolPath,
    string SelectionReason,
    int? ExitCode,
    bool TimedOut,
    double DurationMs,
    string StdoutTail,
    string StderrTail,
    string? FailureReason,
    string? FailureKind,
    string? ExceptionType,
    string? ExceptionMessage);

sealed record GuiSmokeRestartEvent(
    DateTimeOffset RecordedAt,
    string EventType,
    string SessionId,
    string AttemptId,
    int AttemptOrdinal,
    string? RunId,
    string? PreviousAttemptId,
    int? PreviousAttemptOrdinal,
    string? TerminalCause,
    bool? LaunchFailed,
    string? FailureClass,
    string? TrustStateAtStart,
    string? StepScreenPath);

sealed record GuiSmokeRelevantLogDelta(
    IReadOnlyList<string> Matches,
    bool TreatedAsCurrentExecution);

sealed record GuiSmokeSupervisorState(
    DateTimeOffset RecordedAt,
    string SessionId,
    string TrustState,
    string MilestoneState,
    string SessionState,
    string HealthState,
    bool TrustGateSatisfied,
    bool RelevantProcessObserved,
    bool WindowDetected,
    bool RunnerOwnerAlive,
    DateTimeOffset? LastArtifactHeartbeatAt,
    string? LastArtifactHeartbeatPath,
    DateTimeOffset? LastStepAt,
    string? LastStepPath,
    string? ExpectedCurrentAttemptId,
    string? ExpectedCurrentAttemptFirstStepPath,
    string? LastAttemptId,
    string? LastTerminalAttemptId,
    string? LastTerminalCause,
    string? LatestRestartTargetAttemptId,
    string? LatestNextAttemptId,
    string? LatestNextAttemptFirstScreenPath,
    IReadOnlyList<string> HealthClassifications,
    IReadOnlyList<string> Evidence,
    IReadOnlyList<string> Blockers);

sealed record GuiSmokeStallDiagnosisEntry(
    DateTimeOffset RecordedAt,
    string SessionId,
    string AttemptId,
    int AttemptOrdinal,
    string DiagnosisKind,
    bool StallDetected,
    string? FailureClass,
    string? TerminalCause,
    string? Phase,
    string? ObserverScreen,
    string? ScreenshotPath,
    int SameActionStallCount,
    bool PlateauDetected,
    string? BacklogRoute,
    IReadOnlyList<string> Evidence);

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

    public static void InitializeSessionArtifacts(
        string sessionRoot,
        string sessionId,
        string scenarioId,
        string providerKind)
    {
        Directory.CreateDirectory(sessionRoot);
        Directory.CreateDirectory(Path.Combine(sessionRoot, "attempts"));

        var now = DateTimeOffset.UtcNow;
        if (!File.Exists(GetGoalContractPath(sessionRoot)))
        {
            WriteJsonAtomicWithRetry(
                GetGoalContractPath(sessionRoot),
                new GuiSmokeGoalContract(
                    sessionId,
                    scenarioId,
                    providerKind,
                    sessionRoot,
                    now,
                    now,
                    GuiSmokeContractStates.TrustInvalid,
                    GuiSmokeContractStates.MilestoneInProgress,
                    GuiSmokeContractStates.SessionStarting,
                    CreateRunnerOwner(now),
                    now,
                    null,
                    null,
                    GoalCompletionCriteria,
                    GoalOperationalRules),
                GuiSmokeShared.JsonOptions);
        }

        if (!File.Exists(GetPrevalidationPath(sessionRoot)))
        {
            WriteJsonAtomicWithRetry(
                GetPrevalidationPath(sessionRoot),
                new GuiSmokePrevalidation(
                    sessionId,
                    now,
                    now,
                    GameStoppedBeforeDeploy: false,
                    ModsPayloadReconciled: false,
                    DeployIdentityVerified: false,
                    ManualCleanBootVerified: false,
                    GameStopEvidence: null,
                    DeployEvidence: null,
                    ManualCleanBootEvidence: null,
                    Notes: Array.Empty<string>()),
                GuiSmokeShared.JsonOptions);
        }

        if (!File.Exists(GetStartupSummaryPath(sessionRoot)))
        {
            WriteJsonAtomicWithRetry(
                GetStartupSummaryPath(sessionRoot),
                new GuiSmokeStartupSummary(
                    sessionId,
                    now,
                    now,
                    LatestStage: null,
                    LatestStatus: null,
                    GameStoppedBeforeDeployRecorded: false,
                    DeployCommandSelected: false,
                    DeployMode: null,
                    SelectedDeployToolPath: null,
                    SelectedDeployReason: null,
                    DeployCommandStarted: false,
                    DeployCommandFinished: false,
                    DeployCommandExitCode: null,
                    DeployCommandTimedOut: false,
                    DeployCommandDurationMs: null,
                    DeployCommandFailureReason: null,
                    DeployVerificationStarted: false,
                    DeployVerificationFinished: false,
                    LaunchIssued: false,
                    WindowDetected: false,
                    ManualCleanBootEvaluationStarted: false,
                    ManualCleanBootEvaluationFinished: false,
                    FirstAttemptCreated: false,
                    FirstScreenshotCaptured: false,
                    FailureStage: null,
                    FailureReason: null),
                GuiSmokeShared.JsonOptions);
        }

        RefreshSessionSummary(sessionRoot);
        RefreshSupervisorState(sessionRoot);
    }

    public static GuiSmokeGoalContract UpdateRunnerSessionState(string sessionRoot, string sessionState, string? note = null)
    {
        var goal = LoadOrCreateGoalContract(sessionRoot);
        var now = DateTimeOffset.UtcNow;
        var completedAt = sessionState is GuiSmokeContractStates.SessionCompleted or GuiSmokeContractStates.SessionAborted
            ? now
            : goal.CompletedAt;
        var completedBy = sessionState is GuiSmokeContractStates.SessionCompleted or GuiSmokeContractStates.SessionAborted
            ? "runner"
            : goal.CompletedBy;
        var updated = goal with
        {
            UpdatedAt = now,
            SessionState = sessionState,
            LastRunnerHeartbeatAt = now,
            CompletedAt = completedAt,
            CompletedBy = completedBy,
        };

        WriteJsonAtomicWithRetry(GetGoalContractPath(sessionRoot), updated, GuiSmokeShared.JsonOptions);
        if (!string.IsNullOrWhiteSpace(note))
        {
            UpdatePrevalidation(sessionRoot, note: note);
        }
        else
        {
            RefreshSupervisorState(sessionRoot);
        }

        return LoadOrCreateGoalContract(sessionRoot);
    }

    public static GuiSmokePrevalidation UpdatePrevalidation(
        string sessionRoot,
        bool? gameStoppedBeforeDeploy = null,
        bool? modsPayloadReconciled = null,
        bool? deployIdentityVerified = null,
        bool? manualCleanBootVerified = null,
        GuiSmokeProcessStopEvidence? gameStopEvidence = null,
        GuiSmokeDeployEvidence? deployEvidence = null,
        GuiSmokeManualCleanBootEvidence? manualCleanBootEvidence = null,
        string? note = null)
    {
        var prevalidation = LoadOrCreatePrevalidation(sessionRoot);
        var notes = prevalidation.Notes.ToList();
        if (!string.IsNullOrWhiteSpace(note) && !notes.Contains(note, StringComparer.OrdinalIgnoreCase))
        {
            notes.Add(note);
        }

        var updated = prevalidation with
        {
            UpdatedAt = DateTimeOffset.UtcNow,
            GameStoppedBeforeDeploy = gameStoppedBeforeDeploy ?? prevalidation.GameStoppedBeforeDeploy,
            ModsPayloadReconciled = modsPayloadReconciled ?? prevalidation.ModsPayloadReconciled,
            DeployIdentityVerified = deployIdentityVerified ?? prevalidation.DeployIdentityVerified,
            ManualCleanBootVerified = manualCleanBootVerified ?? prevalidation.ManualCleanBootVerified,
            GameStopEvidence = gameStopEvidence ?? prevalidation.GameStopEvidence,
            DeployEvidence = deployEvidence ?? prevalidation.DeployEvidence,
            ManualCleanBootEvidence = manualCleanBootEvidence ?? prevalidation.ManualCleanBootEvidence,
            Notes = notes,
        };

        WriteJsonAtomicWithRetry(GetPrevalidationPath(sessionRoot), updated, GuiSmokeShared.JsonOptions);
        RefreshSupervisorState(sessionRoot);
        return updated;
    }

    public static void RecordStartupStage(
        string sessionRoot,
        string stage,
        string status,
        string? detail = null,
        IReadOnlyDictionary<string, string?>? metadata = null)
    {
        var safeMetadata = metadata ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        AppendNdjson(
            GetStartupTracePath(sessionRoot),
            new GuiSmokeStartupTraceEntry(
                DateTimeOffset.UtcNow,
                Path.GetFileName(sessionRoot),
                stage,
                status,
                detail,
                safeMetadata));
        var summary = ApplyStartupStageUpdate(
            LoadOrCreateStartupSummary(sessionRoot),
            stage,
            status,
            detail,
            safeMetadata);
        WriteJsonAtomicWithRetry(GetStartupSummaryPath(sessionRoot), summary, GuiSmokeShared.JsonOptions);
        TryRefreshSupervisorState(sessionRoot, $"startup-stage:{stage}:{status}");
    }

    public static void RecordStartupFailure(
        string sessionRoot,
        string stage,
        string reason,
        IReadOnlyDictionary<string, string?>? metadata = null)
    {
        RecordStartupStage(sessionRoot, stage, "failed", reason, metadata);
        AppendPrevalidationNoteWithoutRefresh(sessionRoot, $"startup-failure:{stage}:{reason}");
        TryRefreshSupervisorState(sessionRoot, $"startup-failure:{stage}");
    }

    public static void RecordDeployCommandResult(
        string sessionRoot,
        GuiSmokeDeployCommand command,
        GuiSmokeProcessExecutionResult result,
        string? failureReason)
    {
        var summary = new GuiSmokeDeployCommandSummary(
            DateTimeOffset.UtcNow,
            Path.GetFileName(sessionRoot),
            command.Mode,
            command.FileName,
            command.Arguments,
            command.ToolPath,
            command.Reason,
            result.ExitCode,
            result.TimedOut,
            result.Duration.TotalMilliseconds,
            TrimOutputTail(result.Stdout),
            TrimOutputTail(result.Stderr),
            failureReason,
            result.FailureKind,
            result.ExceptionType,
            result.ExceptionMessage);
        var persistFailureReason = TryWriteJsonWithFallback(
            GetDeployCommandSummaryPath(sessionRoot),
            summary,
            GuiSmokeShared.JsonOptions,
            "deploy-command-summary");

        var combinedFailureReason = CombineFailureReasons(failureReason, persistFailureReason);
        var existingStartupSummary = LoadOrCreateStartupSummary(sessionRoot);
        var startupSummary = existingStartupSummary with
        {
            UpdatedAt = DateTimeOffset.UtcNow,
            DeployCommandExitCode = result.ExitCode,
            DeployCommandTimedOut = result.TimedOut,
            DeployCommandDurationMs = result.Duration.TotalMilliseconds,
            DeployCommandFailureReason = combinedFailureReason,
            FailureStage = string.IsNullOrWhiteSpace(combinedFailureReason) ? existingStartupSummary.FailureStage : "deploy-command-finished",
            FailureReason = string.IsNullOrWhiteSpace(combinedFailureReason) ? existingStartupSummary.FailureReason : combinedFailureReason,
        };
        persistFailureReason = CombineFailureReasons(
            persistFailureReason,
            TryWriteJsonWithFallback(
                GetStartupSummaryPath(sessionRoot),
                startupSummary,
                GuiSmokeShared.JsonOptions,
                "startup-summary"));

        if (!string.IsNullOrWhiteSpace(failureReason))
        {
            persistFailureReason = CombineFailureReasons(
                persistFailureReason,
                TryAppendPrevalidationNoteWithoutRefresh(sessionRoot, $"deploy-command-failure:{failureReason}"));
        }

        if (!string.IsNullOrWhiteSpace(persistFailureReason))
        {
            persistFailureReason = CombineFailureReasons(
                persistFailureReason,
                TryAppendPrevalidationNoteWithoutRefresh(sessionRoot, persistFailureReason));

            var recoveredStartupSummary = LoadOrCreateStartupSummary(sessionRoot) with
            {
                UpdatedAt = DateTimeOffset.UtcNow,
                DeployCommandExitCode = result.ExitCode,
                DeployCommandTimedOut = result.TimedOut,
                DeployCommandDurationMs = result.Duration.TotalMilliseconds,
                DeployCommandFailureReason = CombineFailureReasons(failureReason, persistFailureReason),
                FailureStage = "deploy-command-finished",
                FailureReason = CombineFailureReasons(failureReason, persistFailureReason),
            };
            persistFailureReason = CombineFailureReasons(
                persistFailureReason,
                TryWritePlainJson(
                    GetStartupSummaryPath(sessionRoot),
                    recoveredStartupSummary,
                    GuiSmokeShared.JsonOptions,
                    "startup-summary-plain"));
            var durablePersistFailureReason = persistFailureReason;
            if (!string.IsNullOrWhiteSpace(durablePersistFailureReason))
            {
                persistFailureReason = CombineFailureReasons(
                    persistFailureReason,
                    TryAppendPrevalidationNoteWithoutRefresh(sessionRoot, durablePersistFailureReason));
            }
        }

        TryRefreshSupervisorState(sessionRoot, "deploy-command-result");
    }

    public static void RecordGameStoppedBeforeDeployEvidence(string sessionRoot)
    {
        var runningProcesses = GetRunningRelevantProcesses();
        var evidence = new GuiSmokeProcessStopEvidence(
            DateTimeOffset.UtcNow,
            ObserveGameWindow(),
            runningProcesses);
        UpdatePrevalidation(
            sessionRoot,
            gameStoppedBeforeDeploy: runningProcesses.Count == 0 && !evidence.WindowDetected,
            gameStopEvidence: evidence,
            note: "runner captured process-stop evidence before deploy.");
    }

    public static void RecordDeployVerificationEvidence(
        string sessionRoot,
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        bool includeHarnessBridge)
    {
        var artifactsRoot = Path.GetFullPath(configuration.GamePaths.ArtifactsRoot, workspaceRoot);
        var reportPath = Path.Combine(artifactsRoot, "native-package-layout", "flat", "native-deploy-report.json");
        if (!File.Exists(reportPath))
        {
            UpdatePrevalidation(
                sessionRoot,
                modsPayloadReconciled: false,
                deployIdentityVerified: false,
                note: $"deploy report missing: {reportPath}");
            return;
        }

        var reportDocument = TryReadJsonDocument(reportPath);
        if (reportDocument is null)
        {
            UpdatePrevalidation(
                sessionRoot,
                modsPayloadReconciled: false,
                deployIdentityVerified: false,
                note: $"deploy report unreadable: {reportPath}");
            return;
        }

        var root = reportDocument.RootElement;
        var sourcePackageRoot = TryReadString(root, "sourcePackageRoot");
        var deployedRoot = TryReadString(root, "deployedRoot");
        var reportSha256 = ComputeFullFileSha256(reportPath);
        var expectedFiles = new Dictionary<string, (string RelativePath, string SourcePath, string DestinationPath)>(StringComparer.OrdinalIgnoreCase);

        if (root.TryGetProperty("files", out var filesElement) && filesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var fileElement in filesElement.EnumerateArray())
            {
                var sourcePath = TryReadString(fileElement, "sourcePath");
                var destinationPath = TryReadString(fileElement, "destinationPath");
                if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destinationPath))
                {
                    continue;
                }

                var relativePath = Path.GetFileName(destinationPath);
                expectedFiles[relativePath] = (relativePath, sourcePath, destinationPath);
            }
        }

        if (includeHarnessBridge && !string.IsNullOrWhiteSpace(deployedRoot))
        {
            foreach (var (sourceRoot, fileName) in new[]
                     {
                         (Path.Combine(workspaceRoot, "src", "Sts2ModAiCompanion.HarnessBridge", "bin", "Debug", "net7.0"), "Sts2ModAiCompanion.HarnessBridge.dll"),
                         (Path.Combine(workspaceRoot, "src", "Sts2AiCompanion.Foundation", "bin", "Debug", "net7.0"), "Sts2AiCompanion.Foundation.dll"),
                     })
            {
                var sourcePath = Path.Combine(sourceRoot, fileName);
                var destinationPath = Path.Combine(deployedRoot, fileName);
                expectedFiles[fileName] = (fileName, sourcePath, destinationPath);
            }
        }

        var verifiedFiles = new List<GuiSmokeDeployFileEvidence>();
        var missingFiles = new List<string>();
        var hashMismatches = new List<string>();
        var rewriteNotes = new List<string>();
        foreach (var expected in expectedFiles.Values.OrderBy(static entry => entry.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(expected.SourcePath))
            {
                missingFiles.Add($"missing-source:{expected.RelativePath}");
                continue;
            }

            if (!File.Exists(expected.DestinationPath))
            {
                missingFiles.Add($"missing-deployed:{expected.RelativePath}");
                continue;
            }

            var sourceEvidence = DescribeFile(expected.SourcePath);
            var deployedEvidence = DescribeFile(expected.DestinationPath);
            if (!string.Equals(sourceEvidence.Sha256, deployedEvidence.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                if (TryMatchIntentionalRewrite(expected.RelativePath, expected.SourcePath, expected.DestinationPath, out var rewriteNote))
                {
                    if (!string.IsNullOrWhiteSpace(rewriteNote))
                    {
                        rewriteNotes.Add(rewriteNote);
                    }
                }
                else
                {
                    hashMismatches.Add(expected.RelativePath);
                    continue;
                }
            }

            verifiedFiles.Add(new GuiSmokeDeployFileEvidence(expected.RelativePath, sourceEvidence, deployedEvidence));
        }

        var unexpectedFamilyFiles = new List<string>();
        if (!string.IsNullOrWhiteSpace(deployedRoot) && Directory.Exists(deployedRoot))
        {
            foreach (var file in Directory.GetFiles(deployedRoot, "*", SearchOption.TopDirectoryOnly))
            {
                var fileName = Path.GetFileName(file);
                if (!IsCompanionFamilyFile(fileName) || expectedFiles.ContainsKey(fileName))
                {
                    continue;
                }

                unexpectedFamilyFiles.Add(fileName);
            }
        }

        var notes = new List<string>();
        if (!string.IsNullOrWhiteSpace(sourcePackageRoot))
        {
            notes.Add($"sourcePackageRoot:{sourcePackageRoot}");
        }

        if (!string.IsNullOrWhiteSpace(deployedRoot))
        {
            notes.Add($"deployedRoot:{deployedRoot}");
        }

        notes.AddRange(rewriteNotes);

        var deployEvidence = new GuiSmokeDeployEvidence(
            DateTimeOffset.UtcNow,
            reportPath,
            reportSha256,
            sourcePackageRoot,
            deployedRoot,
            verifiedFiles,
            missingFiles,
            hashMismatches,
            unexpectedFamilyFiles,
            notes);
        UpdatePrevalidation(
            sessionRoot,
            modsPayloadReconciled: missingFiles.Count == 0 && unexpectedFamilyFiles.Count == 0,
            deployIdentityVerified: missingFiles.Count == 0 && hashMismatches.Count == 0,
            deployEvidence: deployEvidence,
            note: "runner captured deploy identity evidence from the native deploy report and deployed payload.");
    }

    public static GuiSmokeStartupRuntimeEvidence RecordStartupRuntimeEvidence(
        string sessionRoot,
        ScaffoldConfiguration configuration,
        LiveExportLayout liveLayout,
        HarnessQueueLayout harnessLayout)
    {
        var evidence = BuildStartupRuntimeEvidence(sessionRoot, configuration, liveLayout, harnessLayout);
        WriteJsonAtomicWithRetry(GetStartupRuntimeEvidencePath(sessionRoot), evidence, GuiSmokeShared.JsonOptions);
        File.WriteAllText(evidence.RuntimeLogTailPath, BuildLogTailBody(evidence.RuntimeLogPath, evidence.RuntimeLogMatches), new UTF8Encoding(false));
        File.WriteAllText(evidence.GodotLogTailPath, BuildLogTailBody(evidence.GodotLogPath, evidence.GodotLogMatches), new UTF8Encoding(false));
        File.WriteAllText(evidence.RuntimeLogDeltaPath, BuildLogTailBody(evidence.RuntimeLogPath, evidence.RuntimeLogDeltaMatches), new UTF8Encoding(false));
        File.WriteAllText(evidence.GodotLogDeltaPath, BuildLogTailBody(evidence.GodotLogPath, evidence.GodotLogDeltaMatches), new UTF8Encoding(false));
        AppendPrevalidationNoteWithoutRefresh(sessionRoot, $"startup-runtime-diagnosis:{evidence.Diagnosis}");
        RecordStartupStage(
            sessionRoot,
            "manual-clean-boot-runtime-evidence",
            "finished",
            evidence.Diagnosis,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["failureEdge"] = evidence.FailureEdge,
                ["runtimeLogPresent"] = evidence.RuntimeLogPresent.ToString(),
                ["runtimeLogDeltaMatches"] = evidence.RuntimeLogDeltaMatches.Count.ToString(),
                ["runtimeLogDeltaTreatedAsCurrentExecution"] = evidence.RuntimeLogDeltaTreatedAsCurrentExecution.ToString(),
                ["moduleInitializerBootstrapLogged"] = evidence.ModuleInitializerBootstrapLogged.ToString(),
                ["runtimeExporterInitializedLogged"] = evidence.RuntimeExporterInitializedLogged.ToString(),
                ["harnessBridgeInitializeLogged"] = evidence.HarnessBridgeInitializeLogged.ToString(),
                ["godotLogPresent"] = evidence.GodotLogPresent.ToString(),
                ["godotLogDeltaMatches"] = evidence.GodotLogDeltaMatches.Count.ToString(),
                ["godotLogDeltaTreatedAsCurrentExecution"] = evidence.GodotLogDeltaTreatedAsCurrentExecution.ToString(),
                ["godotReachedMainMenu"] = evidence.GodotReachedMainMenu.ToString(),
                ["loaderSawAnyModLoaderSignal"] = evidence.LoaderSawAnyModLoaderSignal.ToString(),
                ["loaderSawModsDirectoryScan"] = evidence.LoaderSawModsDirectoryScan.ToString(),
                ["loaderSawCompanionPckScan"] = evidence.LoaderSawCompanionPckScan.ToString(),
                ["loaderSawCompanionAssembly"] = evidence.LoaderSawCompanionAssembly.ToString(),
                ["loaderSawPatchAll"] = evidence.LoaderSawPatchAll.ToString(),
                ["loaderSawPatchAllFailure"] = evidence.LoaderSawPatchAllFailure.ToString(),
                ["loaderSawModInitialization"] = evidence.LoaderSawModInitialization.ToString(),
                ["loaderSawModLoadFailure"] = evidence.LoaderSawModLoadFailure.ToString(),
                ["settingsModsEnabled"] = evidence.SettingsModsEnabled.ToString(),
                ["settingsCompanionDisabled"] = evidence.SettingsCompanionDisabled.ToString(),
                ["runtimeConfigEnabled"] = evidence.RuntimeConfigEnabled.ToString(),
                ["runtimeConfigHarnessEnabled"] = evidence.RuntimeConfigHarnessEnabled.ToString(),
                ["companionPckPresent"] = evidence.CompanionPckPresent.ToString(),
                ["companionDllPresent"] = evidence.CompanionDllPresent.ToString(),
                ["companionRuntimeConfigPresent"] = evidence.CompanionRuntimeConfigPresent.ToString(),
                ["liveSnapshotPresent"] = evidence.LiveSnapshotPresent.ToString(),
                ["harnessInventoryPresent"] = evidence.HarnessInventoryPresent.ToString(),
                ["harnessStatusPresent"] = evidence.HarnessStatusPresent.ToString(),
                ["diagnosis"] = evidence.Diagnosis,
            });
        return evidence;
    }

    public static GuiSmokeStartupLogBaseline RecordStartupLogBaseline(
        string sessionRoot,
        ScaffoldConfiguration configuration,
        string runId,
        string launchToken,
        DateTimeOffset launchIssuedAtUtc)
    {
        var modsRoot = Path.Combine(configuration.GamePaths.GameDirectory, "mods");
        var runtimeLogPath = Path.Combine(modsRoot, configuration.AiCompanionMod.RuntimeLogFileName);
        var godotLogPath = Path.Combine(configuration.GamePaths.UserDataRoot, "logs", "godot.log");
        var runtimeInfo = File.Exists(runtimeLogPath) ? new FileInfo(runtimeLogPath) : null;
        var godotInfo = File.Exists(godotLogPath) ? new FileInfo(godotLogPath) : null;
        var baseline = new GuiSmokeStartupLogBaseline(
            RecordedAt: DateTimeOffset.UtcNow,
            SessionId: Path.GetFileName(sessionRoot),
            RunId: runId,
            LaunchToken: launchToken,
            LaunchIssuedAtUtc: launchIssuedAtUtc,
            RuntimeLogPath: runtimeLogPath,
            RuntimeLogPresent: runtimeInfo is not null,
            RuntimeLogSizeBytes: runtimeInfo?.Length ?? 0,
            RuntimeLogLastWriteAt: runtimeInfo?.LastWriteTimeUtc,
            GodotLogPath: godotLogPath,
            GodotLogPresent: godotInfo is not null,
            GodotLogSizeBytes: godotInfo?.Length ?? 0,
            GodotLogLastWriteAt: godotInfo?.LastWriteTimeUtc);
        WriteJsonAtomicWithRetry(GetStartupLogBaselinePath(sessionRoot), baseline, GuiSmokeShared.JsonOptions);
        return baseline;
    }

    public static bool TryMarkManualCleanBootVerified(
        string sessionRoot,
        HarnessQueueLayout harnessLayout,
        ObserverState observer,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        string screenshotPath,
        string? observerStatePath,
        DateTimeOffset? observerFreshnessFloor = null)
    {
        var prevalidation = LoadOrCreatePrevalidation(sessionRoot);
        if (prevalidation.ManualCleanBootVerified)
        {
            return true;
        }

        var firstStepEligible = history.Count == 0;
        var observedScreen = ResolveObservedScreen(observer);
        var observerFresh = observerFreshnessFloor is null || observer.IsFreshSince(observerFreshnessFloor.Value);
        var observerReady = observerFresh && !IsUnknownObservedScreen(observedScreen);
        var mainMenuObserved = observerReady && string.Equals(observedScreen, "main-menu", StringComparison.OrdinalIgnoreCase);
        var armSessionPresent = HasActiveArmSession(harnessLayout.ArmSessionPath);
        var actionsPending = HasPendingHarnessActions(harnessLayout.ActionsPath);
        var status = TryReadJson<HarnessBridgeStatus>(harnessLayout.StatusPath);
        var inventory = TryReadJson<HarnessNodeInventory>(harnessLayout.InventoryPath);
        var inventoryDormant = inventory is null || string.Equals(inventory.Mode, "dormant", StringComparison.OrdinalIgnoreCase);
        var actionsQueueClear = !actionsPending
                                || (!armSessionPresent && firstStepEligible && mainMenuObserved && inventoryDormant);
        var blockingReasons = new List<string>();
        var evaluationNotes = new List<string>();
        if (!firstStepEligible)
        {
            blockingReasons.Add("not-first-step");
        }

        if (!observerReady)
        {
            blockingReasons.Add("observer-not-ready");
        }
        else if (!mainMenuObserved)
        {
            blockingReasons.Add($"observer-not-main-menu:{observedScreen}");
        }

        if (armSessionPresent)
        {
            blockingReasons.Add("arm-session-present");
        }

        if (!actionsQueueClear)
        {
            blockingReasons.Add("actions-pending-active");
        }
        else if (actionsPending)
        {
            evaluationNotes.Add("stale-actions-observed-but-inert");
        }

        if (!inventoryDormant)
        {
            blockingReasons.Add("harness-inventory-not-dormant");
        }

        if (!observerFresh)
        {
            evaluationNotes.Add("observer-stale");
        }

        if (!string.IsNullOrWhiteSpace(status?.Mode)
            && !string.Equals(status.Mode, "dormant", StringComparison.OrdinalIgnoreCase))
        {
            evaluationNotes.Add($"status-mode:{status.Mode}");
        }

        var verified = firstStepEligible
                       && mainMenuObserved
                       && !armSessionPresent
                       && actionsQueueClear
                       && inventoryDormant;
        var evidence = new GuiSmokeManualCleanBootEvidence(
            DateTimeOffset.UtcNow,
            screenshotPath,
            File.Exists(screenshotPath) ? ComputeFullFileSha256(screenshotPath) : "missing",
            observerStatePath,
            !string.IsNullOrWhiteSpace(observerStatePath) && File.Exists(observerStatePath)
                ? ComputeFullFileSha256(observerStatePath)
                : null,
            harnessLayout.StatusPath,
            File.Exists(harnessLayout.StatusPath) ? ComputeFullFileSha256(harnessLayout.StatusPath) : null,
            harnessLayout.InventoryPath,
            File.Exists(harnessLayout.InventoryPath) ? ComputeFullFileSha256(harnessLayout.InventoryPath) : null,
            harnessLayout.ArmSessionPath,
            armSessionPresent,
            harnessLayout.ActionsPath,
            actionsPending,
            status?.Mode,
            inventory?.Mode,
            observedScreen,
            firstStepEligible,
            mainMenuObserved,
            !armSessionPresent,
            actionsQueueClear,
            inventoryDormant,
            status?.LastActionId,
            status?.LastResultStatus,
            blockingReasons,
            evaluationNotes);
        if (prevalidation.ManualCleanBootEvidence is null || firstStepEligible || verified)
        {
            UpdatePrevalidation(
                sessionRoot,
                manualCleanBootVerified: verified,
                manualCleanBootEvidence: evidence,
                note: verified
                    ? "runner captured manual clean boot evidence before the first action."
                    : $"runner recorded manual clean boot blockers:{string.Join(",", blockingReasons)}");
        }

        return verified;
    }

    private static string? ResolveObservedScreen(ObserverState observer)
    {
        return IsUnknownObservedScreen(observer.CurrentScreen)
            ? observer.VisibleScreen
            : observer.CurrentScreen;
    }

    private static bool IsUnknownObservedScreen(string? screen)
    {
        return string.IsNullOrWhiteSpace(screen)
               || string.Equals(screen, "unknown", StringComparison.OrdinalIgnoreCase);
    }

    public static void RecordAttemptStarted(
        string sessionRoot,
        string attemptId,
        int attemptOrdinal,
        string runId,
        string trustStateAtStart,
        string firstScreenPath)
    {
        AppendNdjson(
            GetRestartEventsPath(sessionRoot),
            new GuiSmokeRestartEvent(
                DateTimeOffset.UtcNow,
                GuiSmokeContractStates.EventNextAttemptStarted,
                Path.GetFileName(sessionRoot),
                attemptId,
                attemptOrdinal,
                runId,
                null,
                null,
                null,
                null,
                null,
                trustStateAtStart,
                firstScreenPath));
        RefreshSessionSummary(sessionRoot);
        RefreshSupervisorState(sessionRoot);
    }

    public static void RecordRunnerLaunchIssued(
        string sessionRoot,
        string attemptId,
        int attemptOrdinal,
        string runId,
        string trustStateAtStart)
    {
        AppendNdjson(
            GetRestartEventsPath(sessionRoot),
            new GuiSmokeRestartEvent(
                DateTimeOffset.UtcNow,
                GuiSmokeContractStates.EventRunnerLaunchIssued,
                Path.GetFileName(sessionRoot),
                attemptId,
                attemptOrdinal,
                runId,
                null,
                null,
                null,
                null,
                null,
                trustStateAtStart,
                null));
        RefreshSessionSummary(sessionRoot);
        RefreshSupervisorState(sessionRoot);
    }

    public static void RecordRunnerBeginRestart(
        string sessionRoot,
        GuiSmokeAttemptResult previousAttempt,
        string nextAttemptId,
        int nextAttemptOrdinal)
    {
        AppendNdjson(
            GetRestartEventsPath(sessionRoot),
            new GuiSmokeRestartEvent(
                DateTimeOffset.UtcNow,
                GuiSmokeContractStates.EventRunnerBeginRestart,
                Path.GetFileName(sessionRoot),
                nextAttemptId,
                nextAttemptOrdinal,
                null,
                previousAttempt.AttemptId,
                previousAttempt.AttemptOrdinal,
                previousAttempt.TerminalCause,
                previousAttempt.LaunchFailed,
                previousAttempt.FailureClass,
                previousAttempt.TrustStateAtStart,
                null));
        RefreshSessionSummary(sessionRoot);
        RefreshSupervisorState(sessionRoot);
    }

    public static void RecordAttemptTerminal(
        string sessionRoot,
        string attemptId,
        int attemptOrdinal,
        string runId,
        string? terminalCause,
        bool launchFailed,
        string? failureClass,
        string trustStateAtStart)
    {
        AppendNdjson(
            GetRestartEventsPath(sessionRoot),
            new GuiSmokeRestartEvent(
                DateTimeOffset.UtcNow,
                GuiSmokeContractStates.EventAttemptTerminal,
                Path.GetFileName(sessionRoot),
                attemptId,
                attemptOrdinal,
                runId,
                null,
                null,
                terminalCause,
                launchFailed,
                failureClass,
                trustStateAtStart,
                null));
        RefreshSessionSummary(sessionRoot);
    }

    public static GuiSmokeSessionSummary RefreshSessionSummary(string sessionRoot)
    {
        var goal = LoadOrCreateGoalContract(sessionRoot);
        var attemptEntries = ReadNdjson<GuiSmokeAttemptIndexEntry>(Path.Combine(sessionRoot, "attempt-index.ndjson"))
            .OrderBy(static entry => entry.AttemptOrdinal)
            .ToArray();
        var restartEvents = ReadNdjson<GuiSmokeRestartEvent>(GetRestartEventsPath(sessionRoot))
            .OrderBy(static entry => entry.RecordedAt)
            .ThenBy(static entry => entry.AttemptOrdinal)
            .ToArray();
        var startedAttemptIds = restartEvents
            .Where(static entry =>
                string.Equals(entry.EventType, GuiSmokeContractStates.EventRunnerLaunchIssued, StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.EventType, GuiSmokeContractStates.EventNextAttemptStarted, StringComparison.OrdinalIgnoreCase))
            .Select(static entry => entry.AttemptId)
            .Where(static attemptId => !string.IsNullOrWhiteSpace(attemptId))
            .Concat(attemptEntries.Select(static entry => entry.AttemptId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
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
            AttemptCount: startedAttemptIds.Length,
            TerminalAttemptCount: attemptEntries.Length,
            ActiveAttemptId: DetermineActiveAttemptId(restartEvents),
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
        var attemptEntries = ReadNdjson<GuiSmokeAttemptIndexEntry>(Path.Combine(sessionRoot, "attempt-index.ndjson"))
            .OrderBy(static entry => entry.AttemptOrdinal)
            .ToArray();
        var restartEvents = ReadNdjson<GuiSmokeRestartEvent>(GetRestartEventsPath(sessionRoot))
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
        var attemptEntries = ReadNdjson<GuiSmokeAttemptIndexEntry>(Path.Combine(sessionRoot, "attempt-index.ndjson"))
            .OrderBy(static entry => entry.AttemptOrdinal)
            .ToArray();
        var restartEvents = ReadNdjson<GuiSmokeRestartEvent>(GetRestartEventsPath(sessionRoot))
            .OrderBy(static entry => entry.RecordedAt)
            .ThenBy(static entry => entry.AttemptOrdinal)
            .ToArray();
        var trustGateSatisfied = prevalidation.GameStoppedBeforeDeploy
                                 && prevalidation.ModsPayloadReconciled
                                 && prevalidation.DeployIdentityVerified
                                 && prevalidation.ManualCleanBootVerified;
        var trustState = trustGateSatisfied
            ? GuiSmokeContractStates.TrustValid
            : GuiSmokeContractStates.TrustInvalid;
        var milestoneEvaluation = EvaluateMilestone(goal, trustState, attemptEntries, restartEvents, sessionRoot);
        var healthEvaluation = EvaluateHealth(sessionRoot, goal, attemptEntries, restartEvents, now, observationOverride);

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
            evidence.Add($"startup-runtime-diagnosis:{startupRuntimeEvidence.Diagnosis}");
            evidence.Add($"startup-runtime-failure-edge:{startupRuntimeEvidence.FailureEdge}");
            evidence.Add($"startup-runtime-expected-load-chain:{startupRuntimeEvidence.ExpectedLoadChain}");
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
            evidence.Add($"startup-companion-pck-present:{startupRuntimeEvidence.CompanionPckPresent}");
            evidence.Add($"startup-companion-dll-present:{startupRuntimeEvidence.CompanionDllPresent}");
            evidence.Add($"startup-companion-runtime-config-present:{startupRuntimeEvidence.CompanionRuntimeConfigPresent}");
            evidence.Add($"startup-live-snapshot-present:{startupRuntimeEvidence.LiveSnapshotPresent}");
            evidence.Add($"startup-harness-inventory-present:{startupRuntimeEvidence.HarnessInventoryPresent}");
            evidence.Add($"startup-harness-status-present:{startupRuntimeEvidence.HarnessStatusPresent}");
            evidence.Add($"startup-runtime-evidence:{GetStartupRuntimeEvidencePath(sessionRoot)}");

            if (string.Equals(startupRuntimeEvidence.Diagnosis, "runtime-bootstrap-missing", StringComparison.OrdinalIgnoreCase))
            {
                blockers.Add("startup-runtime-bootstrap-missing");
            }
            else if (string.Equals(startupRuntimeEvidence.Diagnosis, "runtime-loader-failed", StringComparison.OrdinalIgnoreCase))
            {
                blockers.Add("startup-runtime-loader-failed");
            }
            else if (string.Equals(startupRuntimeEvidence.Diagnosis, "runtime-loader-preconditions-blocked", StringComparison.OrdinalIgnoreCase))
            {
                blockers.Add("startup-runtime-loader-preconditions-blocked");
            }
            else if (string.Equals(startupRuntimeEvidence.Diagnosis, "runtime-loader-entry-not-observed", StringComparison.OrdinalIgnoreCase))
            {
                blockers.Add("startup-runtime-loader-entry-not-observed");
            }
            else if (string.Equals(startupRuntimeEvidence.Diagnosis, "runtime-loader-scan-not-observed", StringComparison.OrdinalIgnoreCase))
            {
                blockers.Add("startup-runtime-loader-scan-not-observed");
            }
            else if (string.Equals(startupRuntimeEvidence.Diagnosis, "runtime-loader-not-observed", StringComparison.OrdinalIgnoreCase))
            {
                blockers.Add("startup-runtime-loader-not-observed");
            }

            if (!string.IsNullOrWhiteSpace(startupRuntimeEvidence.FailureEdge)
                && !string.Equals(startupRuntimeEvidence.Diagnosis, "runtime-started-snapshots-present", StringComparison.OrdinalIgnoreCase))
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
            attemptEntries.LastOrDefault()?.AttemptId,
            milestoneEvaluation.LastTerminalAttemptId,
            milestoneEvaluation.LastTerminalCause,
            milestoneEvaluation.LatestRestartTargetAttemptId,
            milestoneEvaluation.LatestNextAttemptId,
            milestoneEvaluation.LatestNextAttemptFirstScreenPath,
            healthEvaluation.Classifications,
            evidence,
            blockers);
        WriteJsonAtomicWithRetry(GetSupervisorStatePath(sessionRoot), supervisorState, GuiSmokeShared.JsonOptions);
        return supervisorState;
    }

    private static HealthEvaluation EvaluateHealth(
        string sessionRoot,
        GuiSmokeGoalContract goal,
        IReadOnlyList<GuiSmokeAttemptIndexEntry> attemptEntries,
        IReadOnlyList<GuiSmokeRestartEvent> restartEvents,
        DateTimeOffset now,
        SupervisorObservationOverride? observationOverride)
    {
        var activeSession = goal.SessionState is GuiSmokeContractStates.SessionStarting or GuiSmokeContractStates.SessionCollecting or GuiSmokeContractStates.SessionStalled;
        var relevantProcessObserved = observationOverride?.RelevantProcessObserved ?? ObserveRelevantProcessHealth();
        var windowDetected = observationOverride?.WindowDetected ?? ObserveGameWindow();
        var runnerOwnerAlive = observationOverride?.RunnerOwnerAlive ?? IsRunnerOwnerAlive(goal.RunnerOwner);
        var currentAttemptId = DetermineCurrentAttemptId(attemptEntries, restartEvents);
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

    private static string? DetermineCurrentAttemptId(
        IReadOnlyList<GuiSmokeAttemptIndexEntry> attemptEntries,
        IReadOnlyList<GuiSmokeRestartEvent> restartEvents)
    {
        var startedAttempt = restartEvents.LastOrDefault(eventEntry =>
            string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventNextAttemptStarted, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(startedAttempt?.AttemptId))
        {
            return startedAttempt.AttemptId;
        }

        return attemptEntries.LastOrDefault()?.AttemptId;
    }

    private static string? DetermineActiveAttemptId(IReadOnlyList<GuiSmokeRestartEvent> restartEvents)
    {
        var activeAttempt = restartEvents
            .Where(static entry => !string.IsNullOrWhiteSpace(entry.AttemptId))
            .GroupBy(static entry => entry.AttemptId, StringComparer.OrdinalIgnoreCase)
            .Select(static group =>
            {
                var lastStartAt = group
                    .Where(static entry =>
                        string.Equals(entry.EventType, GuiSmokeContractStates.EventRunnerLaunchIssued, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(entry.EventType, GuiSmokeContractStates.EventNextAttemptStarted, StringComparison.OrdinalIgnoreCase))
                    .Select(static entry => (DateTimeOffset?)entry.RecordedAt)
                    .DefaultIfEmpty(null)
                    .Max();
                var lastTerminalAt = group
                    .Where(static entry => string.Equals(entry.EventType, GuiSmokeContractStates.EventAttemptTerminal, StringComparison.OrdinalIgnoreCase))
                    .Select(static entry => (DateTimeOffset?)entry.RecordedAt)
                    .DefaultIfEmpty(null)
                    .Max();
                return new
                {
                    AttemptId = group.Key,
                    AttemptOrdinal = group.Max(static entry => entry.AttemptOrdinal),
                    LastStartAt = lastStartAt,
                    LastTerminalAt = lastTerminalAt,
                };
            })
            .Where(static entry => entry.LastStartAt is not null && (entry.LastTerminalAt is null || entry.LastStartAt > entry.LastTerminalAt))
            .OrderByDescending(static entry => entry.LastStartAt)
            .ThenByDescending(static entry => entry.AttemptOrdinal)
            .FirstOrDefault();
        return activeAttempt?.AttemptId;
    }

    private static void WriteStallDiagnosis(string sessionRoot, string runRoot, GuiSmokeAttemptIndexEntry attemptEntry)
    {
        var failureSummary = TryReadJson<GuiSmokeFailureSummary>(Path.Combine(runRoot, "failure-summary.json"));
        var selfMetaReview = TryReadJson<GuiSmokeSelfMetaReview>(Path.Combine(runRoot, "self-meta-review.json"));
        var progress = ReadNdjson<GuiSmokeStepProgress>(Path.Combine(runRoot, "progress.ndjson"));
        var latestStepContext = LoadLatestStepContext(runRoot);
        var latestProgress = progress.LastOrDefault();
        var sameActionStallCount = progress.Count(entry => entry.ObserverSignals.Contains("same-action-stall", StringComparer.OrdinalIgnoreCase));
        var decisionWaitPlateau = AnalyzeDecisionWaitPlateau(progress);
        var inspectOverlayLoop = AnalyzeInspectOverlayLoop(progress);
        var rewardMapLoop = AnalyzeRewardMapLoop(progress);
        var mapOverlayNoOpLoop = AnalyzeMapOverlayNoOpLoop(progress, latestStepContext);
        var mapTransitionStall = AnalyzeMapTransitionStall(progress);
        var combatNoOpLoop = AnalyzeCombatNoOpLoop(progress);
        var latestPhase = failureSummary?.Phase ?? latestStepContext?.Phase ?? latestProgress?.Phase;
        var latestObserverScreen = failureSummary?.ObserverScreen ?? latestStepContext?.ObserverScreen ?? latestProgress?.PostActionScreen ?? latestProgress?.ObserverScreen;
        var diagnosisKind = DetermineDiagnosisKind(attemptEntry, failureSummary, sameActionStallCount, decisionWaitPlateau, inspectOverlayLoop, rewardMapLoop, mapOverlayNoOpLoop, mapTransitionStall, combatNoOpLoop, latestPhase, latestObserverScreen);
        var useCombatAnalysis = string.Equals(diagnosisKind, "combat-noop-loop", StringComparison.OrdinalIgnoreCase);
        var useRewardAnalysis = string.Equals(diagnosisKind, "reward-map-loop", StringComparison.OrdinalIgnoreCase);
        var useMapOverlayAnalysis = string.Equals(diagnosisKind, "map-overlay-noop-loop", StringComparison.OrdinalIgnoreCase);
        var useMapTransitionAnalysis = string.Equals(diagnosisKind, "map-transition-stall", StringComparison.OrdinalIgnoreCase);
        var useOverlayAnalysis = string.Equals(diagnosisKind, "inspect-overlay-loop", StringComparison.OrdinalIgnoreCase);
        var useWaitAnalysis = string.Equals(diagnosisKind, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(diagnosisKind, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase);
        var phase = failureSummary?.Phase
                    ?? (useCombatAnalysis ? combatNoOpLoop.Phase : null)
                    ?? (useRewardAnalysis ? rewardMapLoop.Phase : null)
                    ?? (useMapOverlayAnalysis ? mapOverlayNoOpLoop.Phase : null)
                    ?? (useMapTransitionAnalysis ? mapTransitionStall.Phase : null)
                    ?? (useOverlayAnalysis ? inspectOverlayLoop.Phase : null)
                    ?? (useWaitAnalysis ? decisionWaitPlateau.Phase : null)
                    ?? latestProgress?.Phase;
        var observerScreen = failureSummary?.ObserverScreen
                             ?? (useCombatAnalysis ? combatNoOpLoop.ObserverScreen : null)
                             ?? (useRewardAnalysis ? rewardMapLoop.ObserverScreen : null)
                             ?? (useMapOverlayAnalysis ? mapOverlayNoOpLoop.ObserverScreen : null)
                             ?? (useMapTransitionAnalysis ? mapTransitionStall.ObserverScreen : null)
                             ?? (useOverlayAnalysis ? inspectOverlayLoop.ObserverScreen : null)
                             ?? (useWaitAnalysis ? decisionWaitPlateau.ObserverScreen : null)
                             ?? latestProgress?.PostActionScreen
                             ?? latestProgress?.ObserverScreen;
        var screenshotPath = failureSummary?.ScreenshotPath ?? latestStepContext?.ScreenshotPath ?? FindLatestScreenshotPath(runRoot);
        var backlogRoute = ShouldRouteToDecompilerBacklog(diagnosisKind, phase, observerScreen)
            ? "decompiled-source-first-observer"
            : null;
        var evidence = new List<string>
        {
            $"status:{attemptEntry.Status}",
            $"terminalCause:{attemptEntry.TerminalCause ?? "null"}",
            $"failureClass:{attemptEntry.FailureClass ?? "null"}",
            $"sameActionStalls:{sameActionStallCount}",
            $"repeatedDecisionWaits:{decisionWaitPlateau.RepeatedWaitCount}",
            $"overlayLoopCount:{inspectOverlayLoop.OverlayCloseCount}",
            $"rewardMapLoopCount:{rewardMapLoop.RepeatedLoopCount}",
            $"mapOverlayLoopCount:{mapOverlayNoOpLoop.RepeatedLoopCount}",
            $"mapTransitionLoopCount:{mapTransitionStall.RepeatedLoopCount}",
            $"combatNoOpLoopCount:{combatNoOpLoop.RepeatedLoopCount}",
            $"trustStateAtStart:{attemptEntry.TrustStateAtStart}",
        };
        if (!string.IsNullOrWhiteSpace(phase))
        {
            evidence.Add($"phase:{phase}");
        }

        if (!string.IsNullOrWhiteSpace(decisionWaitPlateau.SceneSignature))
        {
            evidence.Add($"waitSignature:{decisionWaitPlateau.SceneSignature}");
        }

        if (!string.IsNullOrWhiteSpace(inspectOverlayLoop.SceneSignature))
        {
            evidence.Add($"overlayLoopSignature:{inspectOverlayLoop.SceneSignature}");
        }

        if (!string.IsNullOrWhiteSpace(inspectOverlayLoop.LastMisdirectedTarget))
        {
            evidence.Add($"overlayLoopMisdirectedTarget:{inspectOverlayLoop.LastMisdirectedTarget}");
        }

        if (!string.IsNullOrWhiteSpace(mapOverlayNoOpLoop.LastLoopTarget))
        {
            evidence.Add($"mapOverlayLoopTarget:{mapOverlayNoOpLoop.LastLoopTarget}");
        }

        if (mapOverlayNoOpLoop.MapOverlayVisible)
        {
            evidence.Add("mapOverlayVisible:true");
        }

        if (mapOverlayNoOpLoop.MapBackNavigationAvailable)
        {
            evidence.Add("mapBackNavigationAvailable:true");
        }

        if (mapOverlayNoOpLoop.StaleEventChoicePresent)
        {
            evidence.Add("staleEventChoicePresent:true");
        }

        if (mapOverlayNoOpLoop.CurrentNodeArrowVisible)
        {
            evidence.Add("currentNodeArrowVisible:true");
        }

        if (mapOverlayNoOpLoop.ReachableNodeCandidatePresent)
        {
            evidence.Add("reachableNodeCandidatePresent:true");
        }

        if (mapOverlayNoOpLoop.RepeatedCurrentNodeArrowClick)
        {
            evidence.Add("repeatedCurrentNodeArrowClick:true");
        }

        if (!string.IsNullOrWhiteSpace(rewardMapLoop.LastLoopTarget))
        {
            evidence.Add($"rewardMapLoopTarget:{rewardMapLoop.LastLoopTarget}");
        }

        if (rewardMapLoop.ExplicitRewardChoicesPresent)
        {
            evidence.Add("rewardExplicitChoicesPresent:true");
        }

        if (rewardMapLoop.StaleRewardChoicePresent)
        {
            evidence.Add("staleRewardChoicePresent:true");
        }

        if (rewardMapLoop.StaleRewardBoundsPresent)
        {
            evidence.Add("staleRewardBoundsPresent:true");
        }

        if (rewardMapLoop.RewardBackNavigationAvailable)
        {
            evidence.Add("rewardBackNavigationAvailable:true");
        }

        if (rewardMapLoop.ClaimableRewardPresent)
        {
            evidence.Add("claimableRewardPresent:true");
        }

        if (rewardMapLoop.MapArrowContaminationPresent)
        {
            evidence.Add("mapContextVisible:true");
            evidence.Add("rewardMapArrowContamination:true");
        }

        if (rewardMapLoop.OffWindowBoundsReused)
        {
            evidence.Add("offWindowBoundsReused:true");
        }

        if (rewardMapLoop.MapNodeCandidateChosen)
        {
            evidence.Add("mapNodeCandidateChosen:true");
        }

        if (rewardMapLoop.RecoveryAttemptObserved)
        {
            evidence.Add("rewardMapRecoveryAttemptObserved:true");
        }

        if (rewardMapLoop.PostClickRecaptureObserved)
        {
            evidence.Add("postClickRecaptureObserved:true");
        }

        if (!string.IsNullOrWhiteSpace(mapTransitionStall.LastLoopTarget))
        {
            evidence.Add($"mapTransitionLoopTarget:{mapTransitionStall.LastLoopTarget}");
        }

        if (!string.IsNullOrWhiteSpace(combatNoOpLoop.BlockedTargetLabel))
        {
            evidence.Add($"combatNoOpTarget:{combatNoOpLoop.BlockedTargetLabel}");
        }

        if (!string.IsNullOrWhiteSpace(backlogRoute))
        {
            evidence.Add($"backlogRoute:{backlogRoute}");
        }

        var diagnosis = new GuiSmokeStallDiagnosisEntry(
            DateTimeOffset.UtcNow,
            Path.GetFileName(sessionRoot),
            attemptEntry.AttemptId,
            attemptEntry.AttemptOrdinal,
            diagnosisKind,
            diagnosisKind is "same-action-stall" or "scene-authority-invalid" or "phase-timeout" or "decision-abort" or "phase-mismatch-stall" or "decision-wait-plateau" or "inspect-overlay-loop" or "reward-map-loop" or "map-overlay-noop-loop" or "map-transition-stall" or "combat-noop-loop" or "rest-site-post-click-noop" or "rest-site-selection-failed" or "rest-site-grid-not-visible-after-selection" or "rest-site-grid-observer-miss",
            attemptEntry.FailureClass,
            attemptEntry.TerminalCause,
            phase,
            observerScreen,
            screenshotPath,
            sameActionStallCount,
            selfMetaReview?.PlateauDetected == true
            || decisionWaitPlateau.PlateauDetected
            || inspectOverlayLoop.LoopDetected
            || rewardMapLoop.LoopDetected
            || mapOverlayNoOpLoop.LoopDetected
            || mapTransitionStall.StallDetected
            || combatNoOpLoop.LoopDetected
            || string.Equals(diagnosisKind, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "rest-site-selection-failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "rest-site-grid-not-visible-after-selection", StringComparison.OrdinalIgnoreCase)
            || string.Equals(diagnosisKind, "rest-site-grid-observer-miss", StringComparison.OrdinalIgnoreCase),
            backlogRoute,
            evidence);
        UpsertNdjson(GetStallDiagnosisPath(sessionRoot), diagnosis, static existing => existing.AttemptId, diagnosis.AttemptId);
    }

    private static GuiSmokeGoalContract LoadOrCreateGoalContract(string sessionRoot)
    {
        var existing = TryReadJson<GuiSmokeGoalContract>(GetGoalContractPath(sessionRoot));
        if (existing is not null)
        {
            return existing;
        }

        var now = DateTimeOffset.UtcNow;
        var fallback = new GuiSmokeGoalContract(
            Path.GetFileName(sessionRoot),
            "unknown",
            "unknown",
            sessionRoot,
            now,
            now,
            GuiSmokeContractStates.TrustInvalid,
            GuiSmokeContractStates.MilestoneInProgress,
            GuiSmokeContractStates.SessionStarting,
            CreateRunnerOwner(now),
            now,
            null,
            null,
            GoalCompletionCriteria,
            GoalOperationalRules);
        WriteJsonAtomicWithRetry(GetGoalContractPath(sessionRoot), fallback, GuiSmokeShared.JsonOptions);
        return fallback;
    }

    private static GuiSmokePrevalidation LoadOrCreatePrevalidation(string sessionRoot)
    {
        var existing = TryReadJson<GuiSmokePrevalidation>(GetPrevalidationPath(sessionRoot));
        if (existing is not null)
        {
            return existing;
        }

        var now = DateTimeOffset.UtcNow;
        var fallback = new GuiSmokePrevalidation(
            Path.GetFileName(sessionRoot),
            now,
            now,
            GameStoppedBeforeDeploy: false,
            ModsPayloadReconciled: false,
            DeployIdentityVerified: false,
            ManualCleanBootVerified: false,
            GameStopEvidence: null,
            DeployEvidence: null,
            ManualCleanBootEvidence: null,
            Notes: Array.Empty<string>());
        WriteJsonAtomicWithRetry(GetPrevalidationPath(sessionRoot), fallback, GuiSmokeShared.JsonOptions);
        return fallback;
    }

    private static GuiSmokeStartupSummary LoadOrCreateStartupSummary(string sessionRoot)
    {
        var existing = TryReadJson<GuiSmokeStartupSummary>(GetStartupSummaryPath(sessionRoot));
        if (existing is not null)
        {
            return existing;
        }

        var now = DateTimeOffset.UtcNow;
        var fallback = new GuiSmokeStartupSummary(
            Path.GetFileName(sessionRoot),
            now,
            now,
            LatestStage: null,
            LatestStatus: null,
            GameStoppedBeforeDeployRecorded: false,
            DeployCommandSelected: false,
            DeployMode: null,
            SelectedDeployToolPath: null,
            SelectedDeployReason: null,
            DeployCommandStarted: false,
            DeployCommandFinished: false,
            DeployCommandExitCode: null,
            DeployCommandTimedOut: false,
            DeployCommandDurationMs: null,
            DeployCommandFailureReason: null,
            DeployVerificationStarted: false,
            DeployVerificationFinished: false,
            LaunchIssued: false,
            WindowDetected: false,
            ManualCleanBootEvaluationStarted: false,
            ManualCleanBootEvaluationFinished: false,
            FirstAttemptCreated: false,
            FirstScreenshotCaptured: false,
            FailureStage: null,
            FailureReason: null);
        WriteJsonAtomicWithRetry(GetStartupSummaryPath(sessionRoot), fallback, GuiSmokeShared.JsonOptions);
        return fallback;
    }

    private static GuiSmokeStartupRuntimeEvidence BuildStartupRuntimeEvidence(
        string sessionRoot,
        ScaffoldConfiguration configuration,
        LiveExportLayout liveLayout,
        HarnessQueueLayout harnessLayout)
    {
        var companionPckName = configuration.AiCompanionMod.PckName;
        var companionPckBaseName = Path.GetFileNameWithoutExtension(companionPckName);
        var expectedAssemblyFileName = companionPckBaseName + ".dll";
        var modsRoot = Path.Combine(configuration.GamePaths.GameDirectory, "mods");
        var runtimeLogPath = Path.Combine(modsRoot, configuration.AiCompanionMod.RuntimeLogFileName);
        var runtimeConfigPath = Path.Combine(modsRoot, configuration.AiCompanionMod.RuntimeConfigFileName);
        var companionPckPath = Path.Combine(modsRoot, companionPckName);
        var companionDllPath = Path.Combine(modsRoot, expectedAssemblyFileName);
        var packagedManifestPath = ResolvePackagedManifestPath(sessionRoot);
        var settingsPath = ResolveSettingsSavePath(configuration.GamePaths);
        var godotLogPath = Path.Combine(configuration.GamePaths.UserDataRoot, "logs", "godot.log");
        var startupLogBaseline = TryReadJson<GuiSmokeStartupLogBaseline>(GetStartupLogBaselinePath(sessionRoot));
        var runtimeLogMatches = ReadRelevantLogTail(runtimeLogPath, IsRelevantRuntimeLogLine);
        var godotLogMatches = ReadRelevantLogTail(godotLogPath, IsRelevantGodotLogLine);
        var runtimeLogDelta = startupLogBaseline is null
            ? new GuiSmokeRelevantLogDelta(Array.Empty<string>(), TreatedAsCurrentExecution: false)
            : ReadRelevantLogDelta(
                runtimeLogPath,
                startupLogBaseline.RuntimeLogSizeBytes,
                IsRelevantRuntimeLogLine,
                startupLogBaseline.RuntimeLogPresent);
        var godotLogDelta = startupLogBaseline is null
            ? new GuiSmokeRelevantLogDelta(Array.Empty<string>(), TreatedAsCurrentExecution: false)
            : ReadRelevantLogDelta(
                godotLogPath,
                startupLogBaseline.GodotLogSizeBytes,
                IsRelevantGodotLogLine,
                startupLogBaseline.GodotLogPresent);
        var runtimeDiagnosticLines = startupLogBaseline is null ? runtimeLogMatches : runtimeLogDelta.Matches;
        var godotDiagnosticLines = startupLogBaseline is null ? godotLogMatches : godotLogDelta.Matches;
        var runtimeLogInfo = File.Exists(runtimeLogPath) ? new FileInfo(runtimeLogPath) : null;
        var godotLogInfo = File.Exists(godotLogPath) ? new FileInfo(godotLogPath) : null;
        var moduleInitializerLogged = runtimeDiagnosticLines.Any(static line => line.Contains("module initializer bootstrap result", StringComparison.OrdinalIgnoreCase));
        var runtimeExporterLogged = runtimeDiagnosticLines.Any(static line => line.Contains("runtime exporter initialized", StringComparison.OrdinalIgnoreCase));
        var harnessBridgeLogged = runtimeDiagnosticLines.Any(static line => line.Contains("harness bridge initialize result", StringComparison.OrdinalIgnoreCase));
        var godotReachedMainMenu = godotDiagnosticLines.Any(static line => line.Contains("Time to main menu", StringComparison.OrdinalIgnoreCase));
        var loaderSawModsDirectoryScan = godotDiagnosticLines.Any(static line => line.Contains("Found mod pck file", StringComparison.OrdinalIgnoreCase));
        var loaderSawCompanionPckScan = godotDiagnosticLines.Any(line =>
            line.Contains("Found mod pck file", StringComparison.OrdinalIgnoreCase)
            && line.Contains(companionPckName, StringComparison.OrdinalIgnoreCase));
        var loaderSawCompanionSkippedWarning = godotDiagnosticLines.Any(line =>
            line.Contains("Skipping loading mod", StringComparison.OrdinalIgnoreCase)
            && line.Contains(companionPckName, StringComparison.OrdinalIgnoreCase)
            && line.Contains("mods warning", StringComparison.OrdinalIgnoreCase));
        var loaderSawCompanionDisabled = godotDiagnosticLines.Any(line =>
            line.Contains("Skipping loading mod", StringComparison.OrdinalIgnoreCase)
            && line.Contains(companionPckName, StringComparison.OrdinalIgnoreCase)
            && line.Contains("disabled in settings", StringComparison.OrdinalIgnoreCase));
        var loaderSawCompanionDuplicate = godotDiagnosticLines.Any(line =>
            line.Contains("Tried to load mod with PCK name", StringComparison.OrdinalIgnoreCase)
            && line.Contains(companionPckBaseName, StringComparison.OrdinalIgnoreCase));
        var loaderSawCompanionAssembly = godotDiagnosticLines.Any(line =>
            line.Contains("Loading assembly DLL", StringComparison.OrdinalIgnoreCase)
            && line.Contains(expectedAssemblyFileName, StringComparison.OrdinalIgnoreCase));
        var loaderSawInitializerCall = godotDiagnosticLines.Any(line =>
            line.Contains("Calling initializer method of type", StringComparison.OrdinalIgnoreCase)
            && (line.Contains(companionPckBaseName, StringComparison.OrdinalIgnoreCase)
                || line.Contains(configuration.AiCompanionMod.Name, StringComparison.OrdinalIgnoreCase)
                || line.Contains(expectedAssemblyFileName, StringComparison.OrdinalIgnoreCase)));
        var loaderSawNoModInitializerAttribute = godotDiagnosticLines.Any(static line =>
            line.Contains("No ModInitializerAttribute detected", StringComparison.OrdinalIgnoreCase));
        var loaderSawPatchAll = loaderSawNoModInitializerAttribute
                                || godotDiagnosticLines.Any(static line => line.Contains("Harmony.PatchAll", StringComparison.OrdinalIgnoreCase) || line.Contains("PatchAll", StringComparison.OrdinalIgnoreCase));
        var loaderSawPatchAllFailure = godotDiagnosticLines.Any(static line => line.Contains("Exception caught while trying to run PatchAll", StringComparison.OrdinalIgnoreCase));
        var loaderSawModLoadFailure = godotDiagnosticLines.Any(line =>
            line.Contains("Error loading mod", StringComparison.OrdinalIgnoreCase)
            && line.Contains(companionPckBaseName, StringComparison.OrdinalIgnoreCase));
        var loaderSawModInitialization = godotDiagnosticLines.Any(line =>
            line.Contains("Finished mod initialization", StringComparison.OrdinalIgnoreCase)
            && (line.Contains(companionPckBaseName, StringComparison.OrdinalIgnoreCase)
                || line.Contains(configuration.AiCompanionMod.Name, StringComparison.OrdinalIgnoreCase)));
        var loaderSawAnyModLoaderSignal = loaderSawModsDirectoryScan
                                          || loaderSawCompanionSkippedWarning
                                          || loaderSawCompanionDisabled
                                          || loaderSawCompanionDuplicate
                                          || loaderSawCompanionAssembly
                                          || loaderSawInitializerCall
                                          || loaderSawNoModInitializerAttribute
                                          || loaderSawPatchAll
                                          || loaderSawPatchAllFailure
                                          || loaderSawModInitialization
                                          || loaderSawModLoadFailure;
        var runtimeConfigPresent = File.Exists(runtimeConfigPath);
        var runtimeConfigRoot = TryReadJsonNode(runtimeConfigPath) as JsonObject;
        var runtimeConfigEnabled = TryReadBoolean(runtimeConfigRoot?["enabled"]) ?? false;
        var runtimeConfigHarnessEnabled = TryReadBoolean(runtimeConfigRoot?["harness"]?["enabled"]) ?? false;
        var startupSentinelRoot = runtimeConfigRoot?["startupSentinel"] as JsonObject;
        var startupSentinelSessionId = TryReadString(startupSentinelRoot?["sessionId"]);
        var startupSentinelRunId = TryReadString(startupSentinelRoot?["runId"]);
        var startupSentinelLaunchToken = TryReadString(startupSentinelRoot?["launchToken"]);
        var startupSentinelLaunchIssuedAtUtc = TryReadDateTimeOffset(startupSentinelRoot?["launchIssuedAtUtc"]);
        var startupSentinelRelativePath = TryReadString(startupSentinelRoot?["sentinelRelativePath"]);
        var settingsPresent = !string.IsNullOrWhiteSpace(settingsPath) && File.Exists(settingsPath);
        var settingsRoot = TryReadJsonNode(settingsPath) as JsonObject;
        var settingsModsEnabled = TryReadBoolean(settingsRoot?["mod_settings"]?["mods_enabled"]) ?? false;
        var settingsCompanionDisabled = IsCompanionDisabledInSettings(settingsRoot, companionPckBaseName);
        var packagedManifestPresent = File.Exists(packagedManifestPath);
        var packagedManifestRoot = TryReadJsonNode(packagedManifestPath) as JsonObject;
        var packagedManifestPckName = TryReadString(packagedManifestRoot?["pck_name"]);
        var liveSnapshotPresent = File.Exists(liveLayout.SnapshotPath);
        var harnessInventoryPresent = File.Exists(harnessLayout.InventoryPath);
        var harnessStatusPresent = File.Exists(harnessLayout.StatusPath);
        var expectedLoadChain = "NGame.GameStartup -> OneTimeInitialization.ExecuteEssential -> ModManager.Initialize -> LoadModsInDirRecursive(mods) -> TryLoadModFromPck -> ModInitializerAttribute|Harmony.PatchAll -> RuntimeExportContext.EnsureInitialized";
        var failureEdge = DetermineStartupRuntimeFailureEdge(
            runtimeConfigPresent,
            runtimeConfigEnabled,
            settingsPresent,
            settingsModsEnabled,
            settingsCompanionDisabled,
            File.Exists(companionPckPath),
            File.Exists(companionDllPath),
            loaderSawPatchAllFailure,
            loaderSawModLoadFailure,
            moduleInitializerLogged,
            runtimeExporterLogged,
            harnessBridgeLogged,
            liveSnapshotPresent,
            harnessInventoryPresent,
            harnessStatusPresent,
            godotReachedMainMenu,
            loaderSawAnyModLoaderSignal,
            loaderSawModsDirectoryScan,
            loaderSawCompanionPckScan,
            loaderSawCompanionSkippedWarning,
            loaderSawCompanionDisabled,
            loaderSawCompanionDuplicate,
            loaderSawCompanionAssembly,
            loaderSawInitializerCall,
            loaderSawNoModInitializerAttribute,
            loaderSawModInitialization,
            loaderSawPatchAll);
        return new GuiSmokeStartupRuntimeEvidence(
            DateTimeOffset.UtcNow,
            expectedLoadChain,
            failureEdge,
            modsRoot,
            File.Exists(companionPckPath),
            File.Exists(companionDllPath),
            File.Exists(runtimeConfigPath),
            runtimeConfigPath,
            runtimeConfigPresent,
            runtimeConfigEnabled,
            runtimeConfigHarnessEnabled,
            settingsPath,
            settingsPresent,
            settingsModsEnabled,
            settingsCompanionDisabled,
            packagedManifestPath,
            packagedManifestPresent,
            packagedManifestPckName,
            expectedAssemblyFileName,
            GetStartupLogBaselinePath(sessionRoot),
            startupLogBaseline?.RecordedAt,
            startupSentinelSessionId,
            startupSentinelRunId,
            startupSentinelLaunchToken,
            startupSentinelLaunchIssuedAtUtc,
            startupSentinelRelativePath,
            runtimeLogPath,
            runtimeLogInfo is not null,
            runtimeLogInfo?.LastWriteTimeUtc,
            GetStartupRuntimeLogTailPath(sessionRoot),
            runtimeLogMatches,
            GetStartupRuntimeLogDeltaPath(sessionRoot),
            runtimeLogDelta.Matches,
            runtimeLogDelta.TreatedAsCurrentExecution,
            moduleInitializerLogged,
            runtimeExporterLogged,
            harnessBridgeLogged,
            godotLogPath,
            godotLogInfo is not null,
            godotLogInfo?.LastWriteTimeUtc,
            GetStartupGodotLogTailPath(sessionRoot),
            godotLogMatches,
            GetStartupGodotLogDeltaPath(sessionRoot),
            godotLogDelta.Matches,
            godotLogDelta.TreatedAsCurrentExecution,
            godotReachedMainMenu,
            loaderSawAnyModLoaderSignal,
            loaderSawModsDirectoryScan,
            loaderSawCompanionPckScan,
            loaderSawCompanionSkippedWarning,
            loaderSawCompanionDisabled,
            loaderSawCompanionDuplicate,
            loaderSawCompanionAssembly,
            loaderSawInitializerCall,
            loaderSawNoModInitializerAttribute,
            loaderSawModInitialization,
            loaderSawPatchAll,
            loaderSawPatchAllFailure,
            loaderSawModLoadFailure,
            liveLayout.SnapshotPath,
            liveSnapshotPresent,
            harnessLayout.InventoryPath,
            harnessInventoryPresent,
            harnessLayout.StatusPath,
            harnessStatusPresent,
            DetermineStartupRuntimeDiagnosis(
                moduleInitializerLogged,
                runtimeExporterLogged,
                harnessBridgeLogged,
                loaderSawCompanionAssembly,
                loaderSawModInitialization,
                loaderSawPatchAll,
                loaderSawPatchAllFailure,
                loaderSawModLoadFailure,
                liveSnapshotPresent,
                harnessInventoryPresent,
                harnessStatusPresent,
                failureEdge));
    }

    private static string DetermineStartupRuntimeDiagnosis(
        bool moduleInitializerLogged,
        bool runtimeExporterLogged,
        bool harnessBridgeLogged,
        bool loaderSawCompanionAssembly,
        bool loaderSawModInitialization,
        bool loaderSawPatchAll,
        bool loaderSawPatchAllFailure,
        bool loaderSawModLoadFailure,
        bool liveSnapshotPresent,
        bool harnessInventoryPresent,
        bool harnessStatusPresent,
        string failureEdge)
    {
        var runtimeStarted = moduleInitializerLogged || runtimeExporterLogged || harnessBridgeLogged;
        var observerArtifactsPresent = liveSnapshotPresent || harnessInventoryPresent || harnessStatusPresent;
        if (runtimeStarted)
        {
            return observerArtifactsPresent
                ? "runtime-started-snapshots-present"
                : "runtime-started-observer-missing";
        }

        if (loaderSawPatchAllFailure || loaderSawModLoadFailure)
        {
            return "runtime-loader-failed";
        }

        if (string.Equals(failureEdge, "runtime-config-disabled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(failureEdge, "mods-disabled-in-settings", StringComparison.OrdinalIgnoreCase)
            || string.Equals(failureEdge, "companion-disabled-in-settings", StringComparison.OrdinalIgnoreCase)
            || string.Equals(failureEdge, "companion-payload-missing", StringComparison.OrdinalIgnoreCase)
            || string.Equals(failureEdge, "ModManager.TryLoadModFromPck(companion)->PlayerAgreedToModLoading", StringComparison.OrdinalIgnoreCase))
        {
            return "runtime-loader-preconditions-blocked";
        }

        if (string.Equals(failureEdge, "OneTimeInitialization.ExecuteEssential->ModManager.Initialize->LoadModsInDirRecursive(mods)", StringComparison.OrdinalIgnoreCase)
            || string.Equals(failureEdge, "ModManager.Initialize->LoadModsInDirRecursive(mods)->TryLoadModFromPck(companion)", StringComparison.OrdinalIgnoreCase))
        {
            return "runtime-loader-scan-not-observed";
        }

        if (string.Equals(failureEdge, "NGame.GameStartup->OneTimeInitialization.ExecuteEssential->ModManager.Initialize", StringComparison.OrdinalIgnoreCase))
        {
            return "runtime-loader-entry-not-observed";
        }

        if (loaderSawCompanionAssembly || loaderSawModInitialization || loaderSawPatchAll)
        {
            return "runtime-bootstrap-missing";
        }

        return "runtime-loader-not-observed";
    }

    private static string DetermineStartupRuntimeFailureEdge(
        bool runtimeConfigPresent,
        bool runtimeConfigEnabled,
        bool settingsPresent,
        bool settingsModsEnabled,
        bool settingsCompanionDisabled,
        bool companionPckPresent,
        bool companionDllPresent,
        bool loaderSawPatchAllFailure,
        bool loaderSawModLoadFailure,
        bool moduleInitializerLogged,
        bool runtimeExporterLogged,
        bool harnessBridgeLogged,
        bool liveSnapshotPresent,
        bool harnessInventoryPresent,
        bool harnessStatusPresent,
        bool godotReachedMainMenu,
        bool loaderSawAnyModLoaderSignal,
        bool loaderSawModsDirectoryScan,
        bool loaderSawCompanionPckScan,
        bool loaderSawCompanionSkippedWarning,
        bool loaderSawCompanionDisabled,
        bool loaderSawCompanionDuplicate,
        bool loaderSawCompanionAssembly,
        bool loaderSawInitializerCall,
        bool loaderSawNoModInitializerAttribute,
        bool loaderSawModInitialization,
        bool loaderSawPatchAll)
    {
        var runtimeStarted = moduleInitializerLogged || runtimeExporterLogged || harnessBridgeLogged;
        var observerArtifactsPresent = liveSnapshotPresent || harnessInventoryPresent || harnessStatusPresent;
        if (runtimeStarted)
        {
            return observerArtifactsPresent
                ? "RuntimeExportContext.EnsureInitialized->observer-snapshots"
                : "RuntimeExportContext.EnsureInitialized->observer-snapshots-missing";
        }

        if (!runtimeConfigPresent || !companionPckPresent || !companionDllPresent)
        {
            return "companion-payload-missing";
        }

        if (!runtimeConfigEnabled)
        {
            return "runtime-config-disabled";
        }

        if (!settingsPresent || !settingsModsEnabled)
        {
            return "mods-disabled-in-settings";
        }

        if (settingsCompanionDisabled || loaderSawCompanionDisabled)
        {
            return "companion-disabled-in-settings";
        }

        if (loaderSawCompanionSkippedWarning)
        {
            return "ModManager.TryLoadModFromPck(companion)->PlayerAgreedToModLoading";
        }

        if (loaderSawCompanionDuplicate)
        {
            return "ModManager.TryLoadModFromPck(companion)->duplicate-pck-name";
        }

        if (loaderSawPatchAllFailure || loaderSawModLoadFailure)
        {
            return "ModManager.TryLoadModFromPck(companion)->load-or-patch-failure";
        }

        if (loaderSawCompanionAssembly || loaderSawInitializerCall || loaderSawNoModInitializerAttribute || loaderSawModInitialization || loaderSawPatchAll)
        {
            return "TryLoadModFromPck(companion)->RuntimeExportContext.EnsureInitialized";
        }

        if (loaderSawModsDirectoryScan && !loaderSawCompanionPckScan)
        {
            return "ModManager.Initialize->LoadModsInDirRecursive(mods)->TryLoadModFromPck(companion)";
        }

        if (!loaderSawAnyModLoaderSignal)
        {
            return godotReachedMainMenu
                ? "OneTimeInitialization.ExecuteEssential->ModManager.Initialize->LoadModsInDirRecursive(mods)"
                : "NGame.GameStartup->OneTimeInitialization.ExecuteEssential->ModManager.Initialize";
        }

        return "mod-load-chain-not-observed";
    }

    private static bool IsRelevantRuntimeLogLine(string line)
    {
        return line.Contains("module initializer bootstrap result", StringComparison.OrdinalIgnoreCase)
               || line.Contains("runtime exporter initialized", StringComparison.OrdinalIgnoreCase)
               || line.Contains("harness bridge initialize result", StringComparison.OrdinalIgnoreCase)
               || line.Contains("game mod initializer", StringComparison.OrdinalIgnoreCase)
               || line.Contains("runtime config refreshed", StringComparison.OrdinalIgnoreCase)
               || line.Contains("exporter patch prepare", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRelevantGodotLogLine(string line)
    {
        return line.Contains("Time to main menu", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Found mod pck file", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Skipping loading mod", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Calling initializer method of type", StringComparison.OrdinalIgnoreCase)
               || line.Contains("No ModInitializerAttribute detected", StringComparison.OrdinalIgnoreCase)
               || line.Contains("sts2-mod-ai-companion", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Sts2ModAiCompanion", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Harmony.PatchAll", StringComparison.OrdinalIgnoreCase)
               || line.Contains("PatchAll", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Tried to load mod with PCK name", StringComparison.OrdinalIgnoreCase)
               || line.Contains("RUNNING MODDED", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Error loading mod", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Loading assembly DLL", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Finished mod initialization", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<string> ReadRelevantLogTail(string path, Func<string, bool> predicate, int maxLines = 200)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<string>();
        }

        try
        {
            return File.ReadLines(path)
                .Where(predicate)
                .TakeLast(maxLines)
                .ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static GuiSmokeRelevantLogDelta ReadRelevantLogDelta(
        string path,
        long baselineSizeBytes,
        Func<string, bool> predicate,
        bool baselineFilePresent,
        int maxLines = 200)
    {
        if (!File.Exists(path))
        {
            return new GuiSmokeRelevantLogDelta(Array.Empty<string>(), TreatedAsCurrentExecution: false);
        }

        try
        {
            var fileInfo = new FileInfo(path);
            var treatCurrentFileAsDelta = !baselineFilePresent || fileInfo.Length < baselineSizeBytes;
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            if (!treatCurrentFileAsDelta && baselineSizeBytes > 0)
            {
                stream.Seek(Math.Min(baselineSizeBytes, stream.Length), SeekOrigin.Begin);
            }

            using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
            var matches = new Queue<string>();
            while (reader.ReadLine() is { } line)
            {
                if (!predicate(line))
                {
                    continue;
                }

                if (matches.Count >= maxLines)
                {
                    matches.Dequeue();
                }

                matches.Enqueue(line);
            }

            return new GuiSmokeRelevantLogDelta(matches.ToArray(), treatCurrentFileAsDelta);
        }
        catch
        {
            return new GuiSmokeRelevantLogDelta(Array.Empty<string>(), TreatedAsCurrentExecution: false);
        }
    }

    private static string ResolvePackagedManifestPath(string sessionRoot)
    {
        var workspaceRoot = Directory.GetCurrentDirectory();
        return Path.Combine(workspaceRoot, "artifacts", "native-package-layout", "flat", "export-project", "mod_manifest.json");
    }

    private static string ResolveSettingsSavePath(GamePathOptions gamePaths)
    {
        var directPath = Path.Combine(gamePaths.UserDataRoot, "steam", gamePaths.SteamAccountId, "settings.save");
        if (File.Exists(directPath))
        {
            return directPath;
        }

        var steamRoot = Path.Combine(gamePaths.UserDataRoot, "steam");
        if (!Directory.Exists(steamRoot))
        {
            return directPath;
        }

        try
        {
            return Directory.EnumerateFiles(steamRoot, "settings.save", SearchOption.AllDirectories)
                .OrderByDescending(static path => File.GetLastWriteTimeUtc(path))
                .FirstOrDefault()
                ?? directPath;
        }
        catch
        {
            return directPath;
        }
    }

    private static JsonNode? TryReadJsonNode(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(File.ReadAllText(path));
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool? TryReadBoolean(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        try
        {
            return node.GetValue<bool>();
        }
        catch (InvalidOperationException)
        {
            try
            {
                var text = node.GetValue<string>();
                return bool.TryParse(text, out var parsed)
                    ? parsed
                    : null;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }

    private static DateTimeOffset? TryReadDateTimeOffset(JsonNode? node)
    {
        var text = TryReadString(node);
        return DateTimeOffset.TryParse(text, out var parsed)
            ? parsed
            : null;
    }

    private static string? TryReadString(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        try
        {
            return node.GetValue<string>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static bool IsCompanionDisabledInSettings(JsonObject? settingsRoot, string companionPckBaseName)
    {
        if (settingsRoot?["mod_settings"] is not JsonObject modSettings
            || modSettings["disabled_mods"] is not JsonArray disabledMods)
        {
            return false;
        }

        foreach (var entry in disabledMods)
        {
            if (entry is not JsonObject disabledMod)
            {
                continue;
            }

            var name = TryReadString(disabledMod["name"]);
            if (string.Equals(name, companionPckBaseName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPassedAttempt(GuiSmokeAttemptIndexEntry entry)
    {
        var completedLike = string.Equals(entry.Status, "passed", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(entry.Status, "completed", StringComparison.OrdinalIgnoreCase);
        if (!completedLike)
        {
            return false;
        }

        if (!string.Equals(entry.TrustStateAtStart, GuiSmokeContractStates.TrustValid, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (entry.LaunchFailed)
        {
            return false;
        }

        return !string.Equals(entry.TerminalCause, "max-steps-reached", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildLogTailBody(string sourcePath, IReadOnlyList<string> matchedLines)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"source: {sourcePath}");
        if (matchedLines.Count == 0)
        {
            builder.AppendLine("<no-matching-lines>");
            return builder.ToString();
        }

        foreach (var line in matchedLines)
        {
            builder.AppendLine(line);
        }

        return builder.ToString();
    }

    private static GuiSmokeStartupSummary ApplyStartupStageUpdate(
        GuiSmokeStartupSummary summary,
        string stage,
        string status,
        string? detail,
        IReadOnlyDictionary<string, string?> metadata)
    {
        var updated = summary with
        {
            UpdatedAt = DateTimeOffset.UtcNow,
            LatestStage = stage,
            LatestStatus = status,
            FailureStage = string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase)
                ? stage
                : summary.FailureStage,
            FailureReason = string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase)
                ? detail
                : summary.FailureReason,
        };

        return stage switch
        {
            "game-stopped-before-deploy" => updated with
            {
                GameStoppedBeforeDeployRecorded = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            "deploy-command-selected" => updated with
            {
                DeployCommandSelected = true,
                DeployMode = metadata.TryGetValue("deployMode", out var deployMode) ? deployMode : updated.DeployMode,
                SelectedDeployToolPath = metadata.TryGetValue("toolPath", out var toolPath) ? toolPath : updated.SelectedDeployToolPath,
                SelectedDeployReason = metadata.TryGetValue("reason", out var reason) ? reason : updated.SelectedDeployReason,
            },
            "deploy-command-started" => updated with
            {
                DeployCommandStarted = true,
            },
            "deploy-command-finished" => updated with
            {
                DeployCommandFinished = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            "deploy-verification-started" => updated with
            {
                DeployVerificationStarted = true,
            },
            "deploy-verification-finished" => updated with
            {
                DeployVerificationFinished = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            "manual-clean-boot-launch-issued" => updated with
            {
                LaunchIssued = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            "game-window-detected" => updated with
            {
                WindowDetected = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            "manual-clean-boot-evaluation-started" => updated with
            {
                ManualCleanBootEvaluationStarted = true,
            },
            "manual-clean-boot-evaluation-finished" => updated with
            {
                ManualCleanBootEvaluationFinished = true,
            },
            "attempt-0001-started" => updated with
            {
                FirstAttemptCreated = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            "first-screenshot-captured" => updated with
            {
                FirstScreenshotCaptured = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            _ => updated,
        };
    }

    private static void AppendPrevalidationNoteWithoutRefresh(string sessionRoot, string note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return;
        }

        var prevalidation = LoadOrCreatePrevalidation(sessionRoot);
        if (prevalidation.Notes.Contains(note, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var updated = prevalidation with
        {
            UpdatedAt = DateTimeOffset.UtcNow,
            Notes = prevalidation.Notes.Concat(new[] { note }).ToArray(),
        };
        WriteJsonAtomicWithRetry(GetPrevalidationPath(sessionRoot), updated, GuiSmokeShared.JsonOptions);
    }

    private static string TrimOutputTail(string? text, int maxLength = 4000)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text.Replace("\r", string.Empty).Trim();
        return normalized.Length <= maxLength
            ? normalized
            : normalized[^maxLength..];
    }

    private static string? CombineFailureReasons(string? primary, string? secondary)
    {
        if (string.IsNullOrWhiteSpace(primary))
        {
            return string.IsNullOrWhiteSpace(secondary) ? null : secondary;
        }

        if (string.IsNullOrWhiteSpace(secondary))
        {
            return primary;
        }

        return $"{primary} | {secondary}";
    }

    private static void WriteJsonWithFallback<T>(string path, T value, JsonSerializerOptions options)
    {
        try
        {
            WriteJsonAtomicWithRetry(path, value, options);
        }
        catch
        {
            File.WriteAllText(path, JsonSerializer.Serialize(value, options));
        }
    }

    private static string? TryWriteJsonWithFallback<T>(string path, T value, JsonSerializerOptions options, string artifactName)
    {
        try
        {
            WriteJsonWithFallback(path, value, options);
            return null;
        }
        catch (Exception exception)
        {
            return $"summary-persist-failure:{artifactName}:{exception.GetType().Name}:{exception.Message}";
        }
    }

    private static string? TryWritePlainJson<T>(string path, T value, JsonSerializerOptions options, string artifactName)
    {
        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(value, options));
            return null;
        }
        catch (Exception exception)
        {
            return $"summary-persist-failure:{artifactName}:{exception.GetType().Name}:{exception.Message}";
        }
    }

    private static string? TryAppendPrevalidationNoteWithoutRefresh(string sessionRoot, string note)
    {
        try
        {
            AppendPrevalidationNoteWithoutRefresh(sessionRoot, note);
            return null;
        }
        catch (Exception exception)
        {
            return $"prevalidation-note-failure:{exception.GetType().Name}:{exception.Message}";
        }
    }

    private static void TryRefreshSupervisorState(string sessionRoot, string context)
    {
        try
        {
            RefreshSupervisorState(sessionRoot);
        }
        catch (Exception exception)
        {
            AppendPrevalidationNoteWithoutRefresh(sessionRoot, $"startup-trace-refresh-failed:{context}:{exception.GetType().Name}:{exception.Message}");

            var summary = LoadOrCreateStartupSummary(sessionRoot);
            var updatedSummary = summary with
            {
                UpdatedAt = DateTimeOffset.UtcNow,
                FailureStage = summary.FailureStage ?? context,
                FailureReason = summary.FailureReason ?? $"{exception.GetType().Name}: {exception.Message}",
            };
            WriteJsonAtomicWithRetry(GetStartupSummaryPath(sessionRoot), updatedSummary, GuiSmokeShared.JsonOptions);
        }
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

    private static string DetermineDiagnosisKind(
        GuiSmokeAttemptIndexEntry attemptEntry,
        GuiSmokeFailureSummary? failureSummary,
        int sameActionStallCount,
        DecisionWaitPlateauAnalysis decisionWaitPlateau,
        InspectOverlayLoopAnalysis inspectOverlayLoop,
        RewardMapLoopAnalysis rewardMapLoop,
        MapOverlayNoOpLoopAnalysis mapOverlayNoOpLoop,
        MapTransitionStallAnalysis mapTransitionStall,
        CombatNoOpLoopAnalysis combatNoOpLoop,
        string? latestPhase,
        string? latestObserverScreen)
    {
        var latestStateLooksCombat = string.Equals(latestPhase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(latestObserverScreen, "combat", StringComparison.OrdinalIgnoreCase);
        var latestStateLooksEvent = string.Equals(latestPhase, GuiSmokePhase.HandleEvent.ToString(), StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(latestObserverScreen, "event", StringComparison.OrdinalIgnoreCase);
        var latestStateLooksReward = string.Equals(latestPhase, GuiSmokePhase.HandleRewards.ToString(), StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(latestObserverScreen, "rewards", StringComparison.OrdinalIgnoreCase);
        if (string.Equals(attemptEntry.FailureClass, "scene-authority-invalid", StringComparison.OrdinalIgnoreCase))
        {
            return "scene-authority-invalid";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-noop-loop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-noop-loop", StringComparison.OrdinalIgnoreCase)
            || (latestStateLooksCombat && combatNoOpLoop.LoopDetected))
        {
            return "combat-noop-loop";
        }

        if (string.Equals(attemptEntry.TerminalCause, "reward-map-loop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "reward-map-loop", StringComparison.OrdinalIgnoreCase)
            || (!latestStateLooksCombat && !latestStateLooksEvent && latestStateLooksReward && rewardMapLoop.LoopDetected))
        {
            return "reward-map-loop";
        }

        if (string.Equals(attemptEntry.TerminalCause, "map-overlay-noop-loop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "map-overlay-noop-loop", StringComparison.OrdinalIgnoreCase)
            || (latestStateLooksEvent && mapOverlayNoOpLoop.LoopDetected))
        {
            return "map-overlay-noop-loop";
        }

        if (string.Equals(attemptEntry.TerminalCause, "map-transition-stall", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "map-transition-stall", StringComparison.OrdinalIgnoreCase)
            || mapTransitionStall.StallDetected)
        {
            return "map-transition-stall";
        }

        if (string.Equals(attemptEntry.TerminalCause, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase))
        {
            return "rest-site-post-click-noop";
        }

        if (string.Equals(attemptEntry.TerminalCause, "rest-site-selection-failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "rest-site-selection-failed", StringComparison.OrdinalIgnoreCase))
        {
            return "rest-site-selection-failed";
        }

        if (string.Equals(attemptEntry.TerminalCause, "rest-site-grid-not-visible-after-selection", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "rest-site-grid-not-visible-after-selection", StringComparison.OrdinalIgnoreCase))
        {
            return "rest-site-grid-not-visible-after-selection";
        }

        if (string.Equals(attemptEntry.TerminalCause, "rest-site-grid-observer-miss", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "rest-site-grid-observer-miss", StringComparison.OrdinalIgnoreCase))
        {
            return "rest-site-grid-observer-miss";
        }

        if (string.Equals(attemptEntry.TerminalCause, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase)
            || (decisionWaitPlateau.PlateauDetected && string.Equals(decisionWaitPlateau.DiagnosisKind, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase)))
        {
            return "phase-mismatch-stall";
        }

        if (string.Equals(attemptEntry.TerminalCause, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase)
            || (decisionWaitPlateau.PlateauDetected && string.Equals(decisionWaitPlateau.DiagnosisKind, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase)))
        {
            return "decision-wait-plateau";
        }

        if (string.Equals(attemptEntry.TerminalCause, "inspect-overlay-loop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "inspect-overlay-loop", StringComparison.OrdinalIgnoreCase)
            || inspectOverlayLoop.LoopDetected)
        {
            return "inspect-overlay-loop";
        }

        if (string.Equals(attemptEntry.TerminalCause, "combat-noop-loop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(attemptEntry.FailureClass, "combat-noop-loop", StringComparison.OrdinalIgnoreCase)
            || combatNoOpLoop.LoopDetected)
        {
            return "combat-noop-loop";
        }

        if (string.Equals(attemptEntry.TerminalCause, "same-action-stall", StringComparison.OrdinalIgnoreCase)
            || sameActionStallCount > 0)
        {
            return "same-action-stall";
        }

        if (string.Equals(attemptEntry.TerminalCause, "phase-timeout", StringComparison.OrdinalIgnoreCase))
        {
            return "phase-timeout";
        }

        if (string.Equals(attemptEntry.TerminalCause, "decision-abort", StringComparison.OrdinalIgnoreCase))
        {
            return "decision-abort";
        }

        if (string.Equals(attemptEntry.FailureClass, "launch-runtime-noise", StringComparison.OrdinalIgnoreCase))
        {
            return "launch-runtime-noise";
        }

        if (string.Equals(attemptEntry.Status, "failed", StringComparison.OrdinalIgnoreCase))
        {
            return failureSummary?.Phase is not null ? $"failed:{failureSummary.Phase}" : "failed";
        }

        return "no-stall";
    }

    private static DecisionWaitPlateauAnalysis AnalyzeDecisionWaitPlateau(IReadOnlyList<GuiSmokeStepProgress> progress)
    {
        if (progress.Count == 0)
        {
            return new DecisionWaitPlateauAnalysis("no-stall", false, 0, null, null, null);
        }

        var lastWait = progress.LastOrDefault(entry => entry.ObserverSignals.Contains("decision-wait", StringComparer.OrdinalIgnoreCase));
        if (lastWait is null)
        {
            return new DecisionWaitPlateauAnalysis("no-stall", false, 0, null, null, null);
        }

        var normalizedSignature = NormalizeSceneSignatureForPlateau(lastWait.SceneSignature);
        var repeatedWaitCount = 0;
        for (var index = progress.Count - 1; index >= 0; index -= 1)
        {
            var entry = progress[index];
            if (!entry.ObserverSignals.Contains("decision-wait", StringComparer.OrdinalIgnoreCase))
            {
                break;
            }

            if (!string.Equals(entry.Phase, lastWait.Phase, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(entry.ObserverScreen, lastWait.ObserverScreen, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(NormalizeSceneSignatureForPlateau(entry.SceneSignature), normalizedSignature, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            repeatedWaitCount += 1;
        }

        var phaseMismatchObserved = string.Equals(lastWait.Phase, GuiSmokePhase.Embark.ToString(), StringComparison.OrdinalIgnoreCase)
                                    && SignatureIndicatesRoomScreen(normalizedSignature, lastWait.ObserverScreen);
        var plateauLimit = phaseMismatchObserved ? 2 : 5;
        if (repeatedWaitCount < plateauLimit)
        {
            return new DecisionWaitPlateauAnalysis("no-stall", false, repeatedWaitCount, lastWait.Phase, lastWait.ObserverScreen, normalizedSignature);
        }

        return new DecisionWaitPlateauAnalysis(
            phaseMismatchObserved ? "phase-mismatch-stall" : "decision-wait-plateau",
            true,
            repeatedWaitCount,
            lastWait.Phase,
            lastWait.ObserverScreen,
            normalizedSignature);
    }

    private static InspectOverlayLoopAnalysis AnalyzeInspectOverlayLoop(IReadOnlyList<GuiSmokeStepProgress> progress)
    {
        if (progress.Count == 0)
        {
            return new InspectOverlayLoopAnalysis("no-stall", false, 0, null, null, null, null);
        }

        var lastOverlayAction = progress.LastOrDefault(entry => IsOverlayCleanupDecisionTarget(entry.DecisionTargetLabel));
        if (lastOverlayAction is null)
        {
            return new InspectOverlayLoopAnalysis("no-stall", false, 0, null, null, null, null);
        }

        var normalizedSignature = NormalizeSceneSignatureForPlateau(lastOverlayAction.SceneSignature);
        if (!SignatureIndicatesRoomScreen(normalizedSignature, lastOverlayAction.ObserverScreen))
        {
            return new InspectOverlayLoopAnalysis("no-stall", false, 0, lastOverlayAction.Phase, lastOverlayAction.ObserverScreen, normalizedSignature, null);
        }

        var overlayCloseCount = 0;
        string? lastMisdirectedTarget = null;
        for (var index = progress.Count - 1; index >= 0; index -= 1)
        {
            var entry = progress[index];
            if (!string.Equals(entry.ObserverScreen, lastOverlayAction.ObserverScreen, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(NormalizeSceneSignatureForPlateau(entry.SceneSignature), normalizedSignature, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (IsOverlayCleanupDecisionTarget(entry.DecisionTargetLabel))
            {
                overlayCloseCount += 1;
                continue;
            }

            if (entry.ObserverSignals.Contains("alternate-branch:HandleEvent", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("alternate-branch:HandleRewards", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("inspect-overlay", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("reward-choice", StringComparer.OrdinalIgnoreCase)
                || entry.DecisionTargetLabel is null)
            {
                continue;
            }

            if (string.Equals(entry.DecisionTargetLabel, "first event option", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.DecisionTargetLabel, "event progression choice", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.DecisionTargetLabel, "reward choice", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.DecisionTargetLabel, "reward card choice", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.DecisionTargetLabel, "colorless card choice", StringComparison.OrdinalIgnoreCase))
            {
                lastMisdirectedTarget = entry.DecisionTargetLabel;
            }

            break;
        }

        if (overlayCloseCount < 3)
        {
            return new InspectOverlayLoopAnalysis("no-stall", false, overlayCloseCount, lastOverlayAction.Phase, lastOverlayAction.ObserverScreen, normalizedSignature, lastMisdirectedTarget);
        }

        return new InspectOverlayLoopAnalysis(
            "inspect-overlay-loop",
            true,
            overlayCloseCount,
            lastOverlayAction.Phase,
            lastOverlayAction.ObserverScreen,
            normalizedSignature,
            lastMisdirectedTarget);
    }

    private static RewardMapLoopAnalysis AnalyzeRewardMapLoop(IReadOnlyList<GuiSmokeStepProgress> progress)
    {
        if (progress.Count == 0)
        {
            return new RewardMapLoopAnalysis("no-stall", false, 0, null, null, null, null, false, false, false, false, false, false, false, false, false, false);
        }

        var latestProgress = progress[^1];
        if (string.Equals(latestProgress.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestProgress.ObserverScreen, "combat", StringComparison.OrdinalIgnoreCase)
            || string.Equals(latestProgress.PostActionScreen, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return new RewardMapLoopAnalysis("no-stall", false, 0, latestProgress.Phase, latestProgress.PostActionScreen ?? latestProgress.ObserverScreen, NormalizeSceneSignatureForPlateau(latestProgress.SceneSignature), null, false, false, false, false, false, false, false, false, false, false);
        }

        var lastLoopEntry = progress.LastOrDefault(IsRewardMapLoopProgressEntry);
        if (lastLoopEntry is null)
        {
            return new RewardMapLoopAnalysis("no-stall", false, 0, null, null, null, null, false, false, false, false, false, false, false, false, false, false);
        }

        var normalizedSignature = NormalizeSceneSignatureForPlateau(lastLoopEntry.SceneSignature);
        var repeatedLoopCount = 0;
        string? lastLoopTarget = null;
        var explicitRewardChoicesPresent = false;
        var staleRewardChoicePresent = normalizedSignature.Contains("stale:reward-choice", StringComparison.OrdinalIgnoreCase);
        var staleRewardBoundsPresent = normalizedSignature.Contains("stale:reward-bounds", StringComparison.OrdinalIgnoreCase);
        var rewardBackNavigationAvailable = normalizedSignature.Contains("layer:reward-back-nav", StringComparison.OrdinalIgnoreCase);
        var claimableRewardPresent = normalizedSignature.Contains("reward:claimable", StringComparison.OrdinalIgnoreCase);
        var mapArrowContaminationPresent = normalizedSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase)
                                           || normalizedSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)
                                           || normalizedSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase);
        var offWindowBoundsReused = staleRewardBoundsPresent;
        var mapNodeCandidateChosen = false;
        var recoveryAttemptObserved = false;
        var postClickRecaptureObserved = false;
        for (var index = progress.Count - 1; index >= 0; index -= 1)
        {
            var entry = progress[index];
            if (!IsRewardMapLoopProgressEntry(entry)
                || !string.Equals(entry.ObserverScreen, lastLoopEntry.ObserverScreen, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(NormalizeSceneSignatureForPlateau(entry.SceneSignature), normalizedSignature, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            explicitRewardChoicesPresent |= HasExplicitRewardProgressionEvidence(entry);
            staleRewardChoicePresent |= entry.SceneSignature.Contains("stale:reward-choice", StringComparison.OrdinalIgnoreCase);
            staleRewardBoundsPresent |= entry.SceneSignature.Contains("stale:reward-bounds", StringComparison.OrdinalIgnoreCase);
            rewardBackNavigationAvailable |= entry.SceneSignature.Contains("layer:reward-back-nav", StringComparison.OrdinalIgnoreCase);
            claimableRewardPresent |= entry.SceneSignature.Contains("reward:claimable", StringComparison.OrdinalIgnoreCase)
                                      || entry.ObserverSignals.Contains("claimable-reward-present", StringComparer.OrdinalIgnoreCase);
            mapArrowContaminationPresent |= entry.SceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase)
                                            || entry.SceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)
                                            || entry.SceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase);
            offWindowBoundsReused |= entry.SceneSignature.Contains("stale:reward-bounds", StringComparison.OrdinalIgnoreCase);
            mapNodeCandidateChosen |= entry.ActuatorSignals.Contains("map-node-candidate-chosen", StringComparer.OrdinalIgnoreCase);
            recoveryAttemptObserved |= entry.ObserverSignals.Contains("reward-map-recovery-attempt", StringComparer.OrdinalIgnoreCase);
            postClickRecaptureObserved |= entry.ActuatorSignals.Contains("post-click-recapture-observed", StringComparer.OrdinalIgnoreCase);

            if (IsRewardMapLoopTarget(entry.DecisionTargetLabel) || IsStaleRewardLoopTarget(entry.DecisionTargetLabel))
            {
                repeatedLoopCount += 1;
                lastLoopTarget ??= entry.DecisionTargetLabel;
                continue;
            }

            if (entry.DecisionTargetLabel is null
                || entry.ObserverSignals.Contains("alternate-branch:HandleRewards", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("alternate-branch:WaitMap", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("reward-screen-authority", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("reward-explicit-progression", StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            break;
        }

        var loopDetected = repeatedLoopCount >= 2
                           && mapArrowContaminationPresent
                           && (explicitRewardChoicesPresent || staleRewardChoicePresent || staleRewardBoundsPresent);
        if (!loopDetected)
        {
            return new RewardMapLoopAnalysis(
                "no-stall",
                false,
                repeatedLoopCount,
                lastLoopEntry.Phase,
                lastLoopEntry.ObserverScreen,
                normalizedSignature,
                lastLoopTarget,
                explicitRewardChoicesPresent,
                mapArrowContaminationPresent,
                staleRewardChoicePresent,
                staleRewardBoundsPresent,
                rewardBackNavigationAvailable,
                offWindowBoundsReused,
                claimableRewardPresent,
                mapNodeCandidateChosen,
                recoveryAttemptObserved,
                postClickRecaptureObserved);
        }

        return new RewardMapLoopAnalysis(
            "reward-map-loop",
            true,
            repeatedLoopCount,
            lastLoopEntry.Phase,
            lastLoopEntry.ObserverScreen,
            normalizedSignature,
            lastLoopTarget,
            explicitRewardChoicesPresent,
            mapArrowContaminationPresent,
            staleRewardChoicePresent,
            staleRewardBoundsPresent,
            rewardBackNavigationAvailable,
            offWindowBoundsReused,
            claimableRewardPresent,
            mapNodeCandidateChosen,
            recoveryAttemptObserved,
            postClickRecaptureObserved);
    }

    private static MapTransitionStallAnalysis AnalyzeMapTransitionStall(IReadOnlyList<GuiSmokeStepProgress> progress)
    {
        if (progress.Count == 0)
        {
            return new MapTransitionStallAnalysis("no-stall", false, 0, null, null, null, null);
        }

        var lastLoopEntry = progress.LastOrDefault(IsMapTransitionProgressEntry);
        if (lastLoopEntry is null)
        {
            return new MapTransitionStallAnalysis("no-stall", false, 0, null, null, null, null);
        }

        var normalizedSignature = NormalizeSceneSignatureForPlateau(lastLoopEntry.SceneSignature);
        var repeatedLoopCount = 0;
        string? lastLoopTarget = null;
        for (var index = progress.Count - 1; index >= 0; index -= 1)
        {
            var entry = progress[index];
            if (!IsMapTransitionProgressEntry(entry)
                || !string.Equals(entry.ObserverScreen, lastLoopEntry.ObserverScreen, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(NormalizeSceneSignatureForPlateau(entry.SceneSignature), normalizedSignature, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (IsMapTransitionLoopTarget(entry.DecisionTargetLabel))
            {
                repeatedLoopCount += 1;
                lastLoopTarget ??= entry.DecisionTargetLabel;
                continue;
            }

            if (entry.DecisionTargetLabel is null
                || entry.ObserverSignals.Contains("alternate-branch:HandleEvent", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("alternate-branch:ChooseFirstNode", StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            break;
        }

        if (repeatedLoopCount < 2)
        {
            return new MapTransitionStallAnalysis("no-stall", false, repeatedLoopCount, lastLoopEntry.Phase, lastLoopEntry.ObserverScreen, normalizedSignature, lastLoopTarget);
        }

        return new MapTransitionStallAnalysis(
            "map-transition-stall",
            true,
            repeatedLoopCount,
            lastLoopEntry.Phase,
            lastLoopEntry.ObserverScreen,
            normalizedSignature,
            lastLoopTarget);
    }

    private static MapOverlayNoOpLoopAnalysis AnalyzeMapOverlayNoOpLoop(IReadOnlyList<GuiSmokeStepProgress> progress, LatestStepContext? latestStepContext)
    {
        if (progress.Count == 0)
        {
            return new MapOverlayNoOpLoopAnalysis("no-stall", false, 0, null, null, null, null, false, false, false, false, false, false);
        }

        var latestOverlayState = latestStepContext?.MapOverlayState;
        var shouldUseLatestOverlayFallback = latestOverlayState?.ForegroundVisible == true;
        var lastLoopEntry = progress.LastOrDefault(entry =>
            IsMapOverlayProgressEntry(entry)
            || (shouldUseLatestOverlayFallback && IsMapTransitionProgressEntry(entry)));
        if (lastLoopEntry is null)
        {
            return new MapOverlayNoOpLoopAnalysis("no-stall", false, 0, null, null, null, null, false, false, false, false, false, false);
        }

        var normalizedSignature = NormalizeSceneSignatureForPlateau(lastLoopEntry.SceneSignature);
        var repeatedLoopCount = 0;
        var repeatedCurrentNodeArrowClick = false;
        string? lastLoopTarget = null;
        for (var index = progress.Count - 1; index >= 0; index -= 1)
        {
            var entry = progress[index];
            if (!(IsMapOverlayProgressEntry(entry)
                  || (shouldUseLatestOverlayFallback && IsMapTransitionProgressEntry(entry)))
                || !string.Equals(entry.ObserverScreen, lastLoopEntry.ObserverScreen, StringComparison.OrdinalIgnoreCase)
                || (!shouldUseLatestOverlayFallback
                    && !string.Equals(NormalizeSceneSignatureForPlateau(entry.SceneSignature), normalizedSignature, StringComparison.OrdinalIgnoreCase)))
            {
                break;
            }

            if (IsMapOverlayLoopTarget(entry.DecisionTargetLabel))
            {
                repeatedLoopCount += 1;
                lastLoopTarget ??= entry.DecisionTargetLabel;
                if (string.Equals(entry.DecisionTargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase))
                {
                    repeatedCurrentNodeArrowClick = true;
                }

                continue;
            }

            if (entry.DecisionTargetLabel is null
                || entry.ObserverSignals.Contains("alternate-branch:WaitMap", StringComparer.OrdinalIgnoreCase)
                || entry.ObserverSignals.Contains("alternate-branch:ChooseFirstNode", StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            break;
        }

        var mapOverlayVisible = normalizedSignature.Contains("layer:map-overlay-foreground", StringComparison.OrdinalIgnoreCase)
                                || lastLoopEntry.ObserverSignals.Contains("map-overlay-visible", StringComparer.OrdinalIgnoreCase)
                                || latestOverlayState?.ForegroundVisible == true;
        var mapBackNavigationAvailable = normalizedSignature.Contains("map-back-navigation-available", StringComparison.OrdinalIgnoreCase)
                                         || lastLoopEntry.ObserverSignals.Contains("map-back-navigation-available", StringComparer.OrdinalIgnoreCase)
                                         || latestOverlayState?.MapBackNavigationAvailable == true;
        var staleEventChoicePresent = normalizedSignature.Contains("stale:event-choice", StringComparison.OrdinalIgnoreCase)
                                      || lastLoopEntry.ObserverSignals.Contains("stale-event-choice", StringComparer.OrdinalIgnoreCase)
                                      || latestOverlayState?.StaleEventChoicePresent == true;
        var currentNodeArrowVisible = normalizedSignature.Contains("current-node-arrow-visible", StringComparison.OrdinalIgnoreCase)
                                      || lastLoopEntry.ObserverSignals.Contains("current-node-arrow-visible", StringComparer.OrdinalIgnoreCase)
                                      || latestOverlayState?.CurrentNodeArrowVisible == true;
        var reachableNodeCandidatePresent = normalizedSignature.Contains("reachable-node-candidate-present", StringComparison.OrdinalIgnoreCase)
                                            || lastLoopEntry.ObserverSignals.Contains("reachable-node-candidate-present", StringComparer.OrdinalIgnoreCase)
                                            || normalizedSignature.Contains("exported-reachable-node-present", StringComparison.OrdinalIgnoreCase)
                                            || lastLoopEntry.ObserverSignals.Contains("exported-reachable-node-present", StringComparer.OrdinalIgnoreCase)
                                            || latestOverlayState?.ReachableNodeCandidatePresent == true
                                            || latestOverlayState?.ExportedReachableNodeCandidatePresent == true;
        var loopDetected = repeatedLoopCount >= 3
                           && mapOverlayVisible
                           && staleEventChoicePresent
                           && currentNodeArrowVisible;
        return new MapOverlayNoOpLoopAnalysis(
            loopDetected ? "map-overlay-noop-loop" : "no-stall",
            loopDetected,
            repeatedLoopCount,
            lastLoopEntry.Phase,
            lastLoopEntry.ObserverScreen,
            normalizedSignature,
            lastLoopTarget,
            mapOverlayVisible,
            mapBackNavigationAvailable,
            staleEventChoicePresent,
            currentNodeArrowVisible,
            reachableNodeCandidatePresent,
            repeatedCurrentNodeArrowClick);
    }

    private static CombatNoOpLoopAnalysis AnalyzeCombatNoOpLoop(IReadOnlyList<GuiSmokeStepProgress> progress)
    {
        if (progress.Count == 0)
        {
            return new CombatNoOpLoopAnalysis("no-stall", false, 0, null, null, null, null);
        }

        var lastCombatEntry = progress.LastOrDefault(entry =>
            string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(entry.DecisionTargetLabel));
        if (lastCombatEntry is null)
        {
            return new CombatNoOpLoopAnalysis("no-stall", false, 0, null, null, null, null);
        }

        var normalizedSignature = NormalizeSceneSignatureForPlateau(lastCombatEntry.SceneSignature);
        var blockedTargetLabel = progress
            .Where(entry =>
                string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                && entry.ObserverSignals.Any(signal => signal.StartsWith("combat-noop-observed:combat lane slot ", StringComparison.OrdinalIgnoreCase)))
            .Select(entry => entry.ObserverSignals.Last(signal => signal.StartsWith("combat-noop-observed:combat lane slot ", StringComparison.OrdinalIgnoreCase)).Split(':', 2)[1])
            .LastOrDefault()
            ?? progress
                .Where(entry =>
                    string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                    && entry.DecisionTargetLabel is not null
                    && entry.DecisionTargetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase))
                .Select(static entry => entry.DecisionTargetLabel)
                .LastOrDefault();
        var repeatedSelectionCount = 0;
        var enemyTargetCount = 0;
        var combatLoopActionCount = 0;
        for (var index = progress.Count - 1; index >= 0; index -= 1)
        {
            var entry = progress[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                || !string.Equals(entry.ObserverScreen, lastCombatEntry.ObserverScreen, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(NormalizeSceneSignatureForPlateau(entry.SceneSignature), normalizedSignature, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (entry.ObserverSignals.Any(signal => string.Equals(signal, $"combat-noop-observed:{blockedTargetLabel}", StringComparison.OrdinalIgnoreCase)))
            {
                repeatedSelectionCount += 1;
                if (string.Equals(entry.DecisionTargetLabel, "auto-target enemy", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(entry.DecisionTargetLabel, "auto-target enemy recenter", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(entry.DecisionTargetLabel, "auto-target enemy alternate", StringComparison.OrdinalIgnoreCase)
                    || (entry.DecisionTargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    enemyTargetCount += 1;
                }

                combatLoopActionCount += 1;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(blockedTargetLabel)
                && string.Equals(entry.DecisionTargetLabel, blockedTargetLabel.Replace("combat lane ", "combat select attack ", StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase))
            {
                repeatedSelectionCount += 1;
                combatLoopActionCount += 1;
                continue;
            }

            if (string.Equals(entry.DecisionTargetLabel, "auto-target enemy", StringComparison.OrdinalIgnoreCase)
                || (entry.DecisionTargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                enemyTargetCount += 1;
                combatLoopActionCount += 1;
                continue;
            }

            if (string.Equals(entry.DecisionTargetLabel, "auto-target enemy recenter", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.DecisionTargetLabel, "auto-target enemy alternate", StringComparison.OrdinalIgnoreCase))
            {
                enemyTargetCount += 1;
                combatLoopActionCount += 1;
                continue;
            }

            if (entry.DecisionTargetLabel is null)
            {
                continue;
            }

            if (entry.DecisionTargetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase))
            {
                combatLoopActionCount += 1;
                continue;
            }

            break;
        }

        if (repeatedSelectionCount == 0 && string.IsNullOrWhiteSpace(blockedTargetLabel))
        {
            blockedTargetLabel = progress
                .Where(entry =>
                    string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                    && entry.DecisionTargetLabel is not null
                    && entry.DecisionTargetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase))
                .GroupBy(static entry => entry.DecisionTargetLabel, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(static group => group.Count())
                .Select(static group => group.Key)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(blockedTargetLabel))
            {
                repeatedSelectionCount = progress.Count(entry =>
                    string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(entry.DecisionTargetLabel, blockedTargetLabel, StringComparison.OrdinalIgnoreCase));
            }
        }

        var explicitNoOpSignalsObserved = progress.Any(entry =>
            string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
            && entry.ObserverSignals.Any(signal => signal.StartsWith("combat-noop-observed:", StringComparison.OrdinalIgnoreCase)));
        var loopDetected = repeatedSelectionCount >= 2
                           && enemyTargetCount >= (explicitNoOpSignalsObserved ? 2 : 3)
                           && combatLoopActionCount >= 5
                           && !string.IsNullOrWhiteSpace(blockedTargetLabel);
        if (!loopDetected)
        {
            return new CombatNoOpLoopAnalysis("no-stall", false, repeatedSelectionCount, lastCombatEntry.Phase, lastCombatEntry.ObserverScreen, normalizedSignature, blockedTargetLabel);
        }

        return new CombatNoOpLoopAnalysis(
            "combat-noop-loop",
            true,
            repeatedSelectionCount,
            lastCombatEntry.Phase,
            lastCombatEntry.ObserverScreen,
            normalizedSignature,
            blockedTargetLabel);
    }

    private static bool IsRewardMapLoopProgressEntry(GuiSmokeStepProgress entry)
    {
        return (string.Equals(entry.Phase, GuiSmokePhase.HandleRewards.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Phase, GuiSmokePhase.WaitMap.ToString(), StringComparison.OrdinalIgnoreCase))
               && (SignatureIndicatesRewardScreen(entry.SceneSignature, entry.ObserverScreen)
                   || HasExplicitRewardProgressionEvidence(entry));
    }

    private static bool HasExplicitRewardProgressionEvidence(GuiSmokeStepProgress entry)
    {
        return entry.ObserverSignals.Contains("reward-explicit-progression", StringComparer.OrdinalIgnoreCase)
               || entry.ObserverSignals.Contains("reward-choice", StringComparer.OrdinalIgnoreCase)
               || entry.ObserverSignals.Contains("choice-extractor:reward", StringComparer.OrdinalIgnoreCase)
               || entry.ObserverSignals.Contains("choice-extractor:rewards", StringComparer.OrdinalIgnoreCase)
               || SignatureIndicatesRewardScreen(entry.SceneSignature, entry.ObserverScreen);
    }

    private static bool SignatureIndicatesRewardScreen(string? sceneSignature, string? observerScreen)
    {
        return string.Equals(observerScreen, "rewards", StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(sceneSignature)
                   && (sceneSignature.Contains("screen:rewards", StringComparison.OrdinalIgnoreCase)
                       || sceneSignature.Contains("visible:rewards", StringComparison.OrdinalIgnoreCase)));
    }

    private static bool IsMapTransitionProgressEntry(GuiSmokeStepProgress entry)
    {
        return (string.Equals(entry.Phase, GuiSmokePhase.HandleEvent.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Phase, GuiSmokePhase.WaitMap.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Phase, GuiSmokePhase.ChooseFirstNode.ToString(), StringComparison.OrdinalIgnoreCase)
                || LooksLikeEventMapFallbackWait(entry))
               && (entry.ObserverSignals.Contains("map-transition-evidence", StringComparer.OrdinalIgnoreCase)
                   || entry.SceneSignature.Contains("substate:map-transition", StringComparison.OrdinalIgnoreCase)
                   || entry.SceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)
                   || LooksLikeEventMapFallbackWait(entry));
    }

    private static bool IsMapOverlayProgressEntry(GuiSmokeStepProgress entry)
    {
        return (string.Equals(entry.Phase, GuiSmokePhase.HandleEvent.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Phase, GuiSmokePhase.WaitMap.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Phase, GuiSmokePhase.ChooseFirstNode.ToString(), StringComparison.OrdinalIgnoreCase))
               && (entry.SceneSignature.Contains("layer:map-overlay-foreground", StringComparison.OrdinalIgnoreCase)
                   || entry.ObserverSignals.Contains("map-overlay-visible", StringComparer.OrdinalIgnoreCase)
                   || entry.ObserverSignals.Contains("stale-event-choice", StringComparer.OrdinalIgnoreCase));
    }

    private static LatestStepContext? LoadLatestStepContext(string runRoot)
    {
        var stepsRoot = Path.Combine(runRoot, "steps");
        if (!Directory.Exists(stepsRoot))
        {
            return null;
        }

        var latestRequestPath = Directory.GetFiles(stepsRoot, "*.request.json", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .LastOrDefault();
        if (latestRequestPath is null)
        {
            return null;
        }

        var request = TryReadJson<GuiSmokeStepRequest>(latestRequestPath);
        if (request is null)
        {
            return null;
        }

        var observerStatePath = latestRequestPath.Replace(".request.json", ".observer.state.json", StringComparison.OrdinalIgnoreCase);
        using var observerStateDocument = TryReadJsonDocument(observerStatePath);
        var overlayObserver = new ObserverState(request.Observer, observerStateDocument, null, null);
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(overlayObserver, request.WindowBounds, request.ScreenshotPath);
        return new LatestStepContext(
            request.Phase,
            request.Observer.CurrentScreen ?? request.Observer.VisibleScreen,
            request.ScreenshotPath,
            mapOverlayState);
    }

    private static bool IsMapTransitionLoopTarget(string? decisionTargetLabel)
    {
        return string.Equals(decisionTargetLabel, "event progression choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "proceed after resolving rewards", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMapOverlayLoopTarget(string? decisionTargetLabel)
    {
        return string.Equals(decisionTargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "map back", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRewardMapLoopTarget(string? decisionTargetLabel)
    {
        return string.Equals(decisionTargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsStaleRewardLoopTarget(string? decisionTargetLabel)
    {
        return string.Equals(decisionTargetLabel, "reward skip", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "reward choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "claim reward item", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOverlayCleanupDecisionTarget(string? decisionTargetLabel)
    {
        return string.Equals(decisionTargetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "overlay back", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "overlay backdrop close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionTargetLabel, "inspect overlay escape", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeEventMapFallbackWait(GuiSmokeStepProgress entry)
    {
        return string.Equals(entry.Phase, GuiSmokePhase.WaitCombat.ToString(), StringComparison.OrdinalIgnoreCase)
               && string.Equals(entry.ObserverScreen, "event", StringComparison.OrdinalIgnoreCase)
               && entry.ObserverSignals.Contains("alternate-branch:HandleEvent", StringComparer.OrdinalIgnoreCase)
               && (entry.ObserverSignals.Contains("reward-explicit-progression", StringComparer.OrdinalIgnoreCase)
                   || entry.ObserverSignals.Contains("choice-extractor:event", StringComparer.OrdinalIgnoreCase));
    }

    private static string NormalizeSceneSignatureForPlateau(string? sceneSignature)
    {
        if (string.IsNullOrWhiteSpace(sceneSignature))
        {
            return "scene:none";
        }

        var parts = sceneSignature
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static part =>
                !part.StartsWith("shot:", StringComparison.OrdinalIgnoreCase)
                && !part.StartsWith("phase:", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        return parts.Length == 0 ? sceneSignature : string.Join("|", parts);
    }

    private static bool SignatureIndicatesRoomScreen(string normalizedSignature, string? observerScreen)
    {
        if (string.Equals(observerScreen, "event", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observerScreen, "rewards", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observerScreen, "shop", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observerScreen, "map", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observerScreen, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return normalizedSignature.Contains("|screen:event|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|screen:rewards|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|screen:shop|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|screen:map|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|screen:combat|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|room:rest-site|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|room:shop|", StringComparison.OrdinalIgnoreCase)
               || normalizedSignature.Contains("|room:treasure|", StringComparison.OrdinalIgnoreCase);
    }

    private static string? FindLatestScreenshotPath(string runRoot)
    {
        var stepsRoot = Path.Combine(runRoot, "steps");
        if (!Directory.Exists(stepsRoot))
        {
            return null;
        }

        return Directory.GetFiles(stepsRoot, "*.screen.png", SearchOption.TopDirectoryOnly)
            .OrderByDescending(static path => path, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    private static bool ShouldRouteToDecompilerBacklog(string diagnosisKind, string? phase, string? observerScreen)
    {
        return string.Equals(diagnosisKind, "scene-authority-invalid", StringComparison.OrdinalIgnoreCase)
               && (IsEarlyMenuPhase(phase)
                   || string.Equals(observerScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(observerScreen, "character-select", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsEarlyMenuPhase(string? phase)
    {
        return string.Equals(phase, GuiSmokePhase.EnterRun.ToString(), StringComparison.OrdinalIgnoreCase)
               || string.Equals(phase, GuiSmokePhase.WaitCharacterSelect.ToString(), StringComparison.OrdinalIgnoreCase)
               || string.Equals(phase, GuiSmokePhase.ChooseCharacter.ToString(), StringComparison.OrdinalIgnoreCase)
               || string.Equals(phase, GuiSmokePhase.Embark.ToString(), StringComparison.OrdinalIgnoreCase);
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

    private static string? TryNormalizeRuntimeConfigForDeployVerification(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var node = JsonNode.Parse(File.ReadAllText(path));
            if (node is not JsonObject root)
            {
                return null;
            }

            if (root["harness"] is not JsonObject harness)
            {
                return null;
            }

            harness["enabled"] = true;
            root.Remove("startupSentinel");
            return root.ToJsonString();
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string ComputeSha256Utf8(string value)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private static GuiSmokeFileIdentityEvidence DescribeFile(string path)
    {
        var info = new FileInfo(path);
        return new GuiSmokeFileIdentityEvidence(
            path,
            info.Length,
            info.LastWriteTimeUtc,
            ComputeFullFileSha256(path));
    }

    private static int CountScreenshots(string stepsRoot)
    {
        return Directory.Exists(stepsRoot)
            ? Directory.GetFiles(stepsRoot, "*.screen.png", SearchOption.TopDirectoryOnly).Length
            : 0;
    }

    private static string ComputeFullFileSha256(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(stream));
        }
        catch
        {
            return "unavailable";
        }
    }

    private static JsonDocument? TryReadJsonDocument(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return JsonDocument.Parse(stream);
        }
        catch
        {
            return null;
        }
    }

    private static string? TryReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static T? TryReadJson<T>(string path)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path), GuiSmokeShared.JsonOptions);
        }
        catch (IOException)
        {
            return default;
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static void UpsertNdjson<T>(string path, T entry, Func<T, string> keySelector, string key)
    {
        var existingEntries = new List<T>();
        if (File.Exists(path))
        {
            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                T? existing;
                try
                {
                    existing = JsonSerializer.Deserialize<T>(line, GuiSmokeShared.JsonOptions);
                }
                catch (JsonException)
                {
                    continue;
                }

                if (existing is null)
                {
                    continue;
                }

                if (string.Equals(keySelector(existing), key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                existingEntries.Add(existing);
            }
        }

        existingEntries.Add(entry);
        var lines = existingEntries
            .Select(item => JsonSerializer.Serialize(item, GuiSmokeShared.NdjsonOptions))
            .ToArray();
        WriteJsonAtomicWithRetry(path + ".tmp.json", lines, GuiSmokeShared.JsonOptions);
        File.WriteAllLines(path, lines);
        File.Delete(path + ".tmp.json");
    }

    private static void WriteJsonAtomicWithRetry<T>(
        string path,
        T value,
        JsonSerializerOptions options,
        int maxAttempts = 6,
        int retryDelayMs = 25)
    {
        for (var attempt = 1; attempt <= Math.Max(1, maxAttempts); attempt += 1)
        {
            try
            {
                LiveExportAtomicFileWriter.WriteJsonAtomic(path, value, options);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(retryDelayMs * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(retryDelayMs * attempt);
            }
        }

        LiveExportAtomicFileWriter.WriteJsonAtomic(path, value, options);
    }

    private static string GetGoalContractPath(string sessionRoot) => Path.Combine(sessionRoot, "goal-contract.json");

    private static string GetPrevalidationPath(string sessionRoot) => Path.Combine(sessionRoot, "prevalidation.json");

    private static string GetRestartEventsPath(string sessionRoot) => Path.Combine(sessionRoot, "restart-events.ndjson");

    private static string GetSupervisorStatePath(string sessionRoot) => Path.Combine(sessionRoot, "supervisor-state.json");

    private static string GetStallDiagnosisPath(string sessionRoot) => Path.Combine(sessionRoot, "stall-diagnosis.ndjson");

    private static string GetStartupTracePath(string sessionRoot) => Path.Combine(sessionRoot, "startup-trace.ndjson");

    private static string GetStartupSummaryPath(string sessionRoot) => Path.Combine(sessionRoot, "startup-summary.json");

    private static string GetStartupLogBaselinePath(string sessionRoot) => Path.Combine(sessionRoot, "startup-log-baseline.json");

    private static string GetStartupRuntimeEvidencePath(string sessionRoot) => Path.Combine(sessionRoot, "startup-runtime-evidence.json");

    private static string GetStartupRuntimeLogTailPath(string sessionRoot) => Path.Combine(sessionRoot, "startup-runtime-log.tail.txt");

    private static string GetStartupRuntimeLogDeltaPath(string sessionRoot) => Path.Combine(sessionRoot, "startup-runtime-log.delta.txt");

    private static string GetStartupGodotLogTailPath(string sessionRoot) => Path.Combine(sessionRoot, "startup-godot-log.tail.txt");

    private static string GetStartupGodotLogDeltaPath(string sessionRoot) => Path.Combine(sessionRoot, "startup-godot-log.delta.txt");

    private static string GetDeployCommandSummaryPath(string sessionRoot) => Path.Combine(sessionRoot, "deploy-command-summary.json");
}
