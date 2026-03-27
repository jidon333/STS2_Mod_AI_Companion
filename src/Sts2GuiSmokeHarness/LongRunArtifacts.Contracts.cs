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
    string Diagnosis,
    string? SentinelPath = null,
    bool SentinelPresent = false,
    DateTimeOffset? SentinelLastWriteAt = null,
    bool SentinelSessionMatch = false,
    bool SentinelRunMatch = false,
    bool SentinelLaunchTokenMatch = false,
    int CaptureCount = 0,
    DateTimeOffset? FirstPositiveCaptureAt = null,
    string? FirstPositiveReason = null,
    DateTimeOffset? LastCaptureAt = null,
    bool EverReachedMainMenu = false,
    bool EverSawCurrentExecutionSentinel = false,
    bool EverSawRuntimeExporter = false,
    bool EverSawHarnessBridge = false,
    bool EverSawFreshSnapshot = false,
    bool EverSawStaleSnapshot = false,
    bool EverSawLoaderSignal = false,
    string? LatestCaptureStage = null,
    string? LatestCaptureReason = null,
    bool FreshSnapshotPresent = false,
    bool StaleSnapshotObserved = false,
    bool NoSnapshotEvidence = false);

sealed record GuiSmokeStartupSentinelEvidence(
    string? Path,
    bool Present,
    DateTimeOffset? LastWriteAt,
    bool SessionMatch,
    bool RunMatch,
    bool LaunchTokenMatch);

sealed record GuiSmokeStartupRuntimeCapture(
    DateTimeOffset CapturedAt,
    string Stage,
    string CaptureReason,
    string Diagnosis,
    string FailureEdge,
    bool GodotReachedMainMenu,
    bool SentinelPresent,
    bool SentinelSessionMatch,
    bool SentinelRunMatch,
    bool SentinelLaunchTokenMatch,
    bool RuntimeExporterInitializedLogged,
    bool HarnessBridgeInitializeLogged,
    bool LiveSnapshotPresent,
    bool FreshSnapshotPresent,
    bool StaleSnapshotObserved,
    bool NoSnapshotEvidence,
    bool LoaderSawAnyModLoaderSignal,
    bool LoaderSawCompanionAssembly,
    bool LoaderSawInitializerCall,
    bool LoaderSawPatchAll,
    int DeltaRuntimeMatchCount,
    int DeltaGodotMatchCount);

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
