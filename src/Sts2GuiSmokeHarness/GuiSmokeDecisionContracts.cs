interface IGuiDecisionProvider
{
    Task<GuiSmokeStepDecision> GetDecisionAsync(string requestPath, string decisionPath, TimeSpan timeout, CancellationToken cancellationToken);
}

sealed record GuiSmokeStepRequest(
    string RunId,
    string ScenarioId,
    int StepIndex,
    string Phase,
    string Goal,
    DateTimeOffset IssuedAt,
    string ScreenshotPath,
    WindowBounds WindowBounds,
    string SceneSignature,
    string AttemptId,
    int AttemptOrdinal,
    int BoundedExplorationBudget,
    bool FirstSeenScene,
    string ReasoningMode,
    string? SemanticGoal,
    ObserverSummary Observer,
    IReadOnlyList<KnownRecipeHint> KnownRecipes,
    IReadOnlyList<EventKnowledgeCandidate> EventKnowledgeCandidates,
    IReadOnlyList<CombatCardKnowledgeHint> CombatCardKnowledge,
    string[] AllowedActions,
    IReadOnlyList<GuiSmokeHistoryEntry> History,
    string FailureModeHint,
    string? DecisionRiskHint);

sealed record GuiSmokeSceneRequestContext(
    string SceneSignature,
    bool FirstSeenScene,
    string ReasoningMode,
    IReadOnlyList<KnownRecipeHint> KnownRecipes);

sealed record KnownRecipeHint(
    string SceneSignature,
    string Phase,
    string ActionKind,
    string? TargetLabel,
    string? ExpectedScreen,
    string? Reason);

sealed record SceneRecipeEntry(
    DateTimeOffset RecordedAt,
    string SceneSignature,
    string Phase,
    string ActionKind,
    string? TargetLabel,
    string? ExpectedScreen,
    string? Reason,
    string ScreenshotPath);

sealed record UnknownSceneEntry(
    DateTimeOffset RecordedAt,
    string SceneSignature,
    string Phase,
    string ScreenshotPath,
    string? ObserverScreen,
    string? VisibleScreen,
    string? Reason);

sealed record EventKnowledgeCandidate(
    string EventId,
    string Title,
    string? MatchReason,
    IReadOnlyList<EventOptionKnowledgeCandidate> Options);

sealed record EventOptionKnowledgeCandidate(
    string Label,
    string? Description,
    string? OptionKey);

sealed record CombatCardKnowledgeHint(
    int SlotIndex,
    string Name,
    string? Type,
    string? Target,
    int? Cost,
    string MatchSource);

sealed record GuiSmokeStepDecision(
    string Status,
    string? ActionKind,
    string? KeyText,
    double? NormalizedX,
    double? NormalizedY,
    string? TargetLabel,
    string? Reason,
    double? Confidence,
    string? ExpectedScreen,
    int? WaitMs,
    bool? RequiresRecapture,
    string? AbortReason,
    string? SceneInterpretation = null,
    string? ExpectedDelta = null,
    string? DecisionRisk = null);

sealed record GuiSmokeCandidatePoint(
    double X,
    double Y);

sealed record GuiSmokeSuppressedCandidate(
    string Label,
    string SuppressionReason);

sealed record GuiSmokeDecisionCandidateDump(
    string Label,
    string Source,
    double Score,
    bool Selected,
    string? RejectReason,
    string? RawBounds,
    GuiSmokeCandidatePoint? NormalizedPoint,
    string? BoundsSource,
    string? TargetLabel,
    string? ActionKind,
    string? Reason);

sealed record GuiSmokeDecisionDebugSummary(
    string? ForegroundKind,
    string? BackgroundKind,
    IReadOnlyList<string> ActiveCandidateSet,
    IReadOnlyList<GuiSmokeSuppressedCandidate> SuppressedCandidates,
    string WinnerSelectionReason);

sealed record GuiSmokeCandidateDumpArtifact(
    string Phase,
    string ScreenshotPath,
    GuiSmokeStepDecision PredictedDecision,
    GuiSmokeStepDecision FinalDecision,
    bool MatchesPredictedDecision,
    GuiSmokeDecisionDebugSummary DebugSummary,
    IReadOnlyList<GuiSmokeDecisionCandidateDump> Candidates);

sealed record GuiSmokeDecisionAnalysis(
    string Phase,
    string ScreenshotPath,
    GuiSmokeStepDecision PredictedDecision,
    GuiSmokeStepDecision FinalDecision,
    GuiSmokeDecisionDebugSummary DebugSummary,
    IReadOnlyList<GuiSmokeDecisionCandidateDump> Candidates,
    bool MatchesPredictedDecision)
{
    public GuiSmokeCandidateDumpArtifact ToArtifact()
    {
        return new GuiSmokeCandidateDumpArtifact(
            Phase,
            ScreenshotPath,
            PredictedDecision,
            FinalDecision,
            MatchesPredictedDecision,
            DebugSummary,
            Candidates);
    }
}

sealed record GuiSmokeReplayEvaluation(
    string RequestPath,
    GuiSmokeStepRequest Request,
    GuiSmokeStepDecision Decision,
    GuiSmokeCandidateDumpArtifact CandidateDump);

sealed record GuiSmokeReplayRequestLoadResult(
    GuiSmokeStepRequest Request,
    bool FullRequestRebuild,
    bool ObserverStateLoaded,
    IReadOnlyList<GuiSmokeReplayTimingEntry> Timings);
