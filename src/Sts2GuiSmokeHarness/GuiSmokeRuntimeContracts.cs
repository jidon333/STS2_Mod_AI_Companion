enum GuiSmokePhase
{
    WaitMainMenu,
    EnterRun,
    WaitRunLoad,
    WaitCharacterSelect,
    ChooseCharacter,
    Embark,
    WaitMap,
    HandleRewards,
    ChooseFirstNode,
    WaitPostMapNodeRoom,
    WaitCombat,
    HandleEvent,
    WaitEventRelease,
    HandleShop,
    HandleCombat,
    Completed,
}

sealed record GuiSmokeRunManifest(
    string RunId,
    string ScenarioId,
    string Provider,
    DateTimeOffset StartedAt,
    string WorkspaceRoot,
    string LiveRoot,
    string HarnessRoot,
    string GameDirectory)
{
    public string? Status { get; set; }
    public string? ResultMessage { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

sealed record GuiSmokeSessionSummary(
    string SessionId,
    string ScenarioId,
    string Provider,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    int AttemptCount,
    int TerminalAttemptCount,
    string? ActiveAttemptId,
    DateTimeOffset LastEventAt,
    int PassedAttempts,
    int FailedAttempts,
    int TotalSteps,
    IReadOnlyDictionary<string, int> ValidationEvents);

sealed record GuiSmokeAttemptResult(
    string AttemptId,
    int AttemptOrdinal,
    string RunId,
    string RunRoot,
    int ExitCode,
    string Status,
    string Message,
    int StepCount,
    bool LaunchFailed,
    string? TerminalCause,
    string? FailureClass,
    string TrustStateAtStart);

sealed record GuiSmokeAttemptIndexEntry(
    string AttemptId,
    int AttemptOrdinal,
    string RunId,
    string Status,
    string? ResultMessage,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    int StepCount,
    string? TerminalCause,
    bool LaunchFailed,
    string? FailureClass,
    string TrustStateAtStart);

sealed record GuiSmokeFailureSummary(
    string Phase,
    string Message,
    string? ObserverScreen,
    bool? ObserverInCombat,
    string? ScreenshotPath);

sealed record GuiSmokeDeployToolSelection(
    string Path,
    string Reason);

sealed record GuiSmokeDeployCommand(
    string Mode,
    string FileName,
    string Arguments,
    TimeSpan Timeout,
    string? ToolPath,
    string Reason);

sealed record GuiSmokeProcessExecutionResult(
    string FileName,
    string Arguments,
    int? ExitCode,
    bool TimedOut,
    TimeSpan Duration,
    string Stdout,
    string Stderr,
    string? FailureKind,
    string? ExceptionType,
    string? ExceptionMessage);

sealed record GuiSmokeValidationSummary(
    string RunId,
    int TotalTraceEntries,
    IReadOnlyDictionary<string, int> EventCounts,
    IReadOnlyDictionary<string, int> SceneCounts);

sealed record GuiSmokeTraceEntry(
    DateTimeOffset RecordedAt,
    int StepIndex,
    string Phase,
    string EventKind,
    string? ObserverScreen,
    bool? ObserverInCombat,
    string? TargetLabel);

sealed record GuiSmokeStepProgress(
    DateTimeOffset RecordedAt,
    int StepIndex,
    string Phase,
    string SceneSignature,
    string? ObserverScreen,
    string? PostActionScreen,
    string? DecisionTargetLabel,
    bool ObserverProgress,
    bool ActuatorProgress,
    bool FirstSeenScene,
    bool SemanticReasoningActive,
    bool RecipeRecorded,
    IReadOnlyList<string> ObserverSignals,
    IReadOnlyList<string> ActuatorSignals);

sealed record GuiSmokeSelfMetaReview(
    DateTimeOffset RecordedAt,
    string AttemptId,
    int AttemptOrdinal,
    string RunId,
    string Status,
    string? ResultMessage,
    bool PlateauDetected,
    string DominantFailureClass,
    string DirectionRisk,
    IReadOnlyList<string> Evidence,
    IReadOnlyList<string> NextAttemptAdjustments,
    double ObserverCoverageRatio,
    double ActuatorSuccessRatio,
    int NovelSceneCount,
    int SameActionStallCount,
    int StepCount);

sealed record GuiSmokeHistoryEntry(
    string Phase,
    string Action,
    string? TargetLabel,
    DateTimeOffset RecordedAt)
{
    public string? Metadata { get; init; }
}

sealed record WindowBounds(
    int X,
    int Y,
    int Width,
    int Height);
