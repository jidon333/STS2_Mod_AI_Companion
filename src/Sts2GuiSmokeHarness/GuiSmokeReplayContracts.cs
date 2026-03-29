sealed record GuiSmokeReplayGoldenSceneFixture(
    string Name,
    string RequestPath,
    string? ExpectedTargetContains,
    string? ExpectedForegroundKind,
    string? ExpectedBackgroundKind,
    IReadOnlyList<string> RequiredCandidateLabels,
    IReadOnlyList<string> RequiredSuppressedLabels,
    IReadOnlyList<string> ForbiddenTargetLabels);

sealed record GuiSmokeReplayParityFixture(
    string Name,
    string RequestPath,
    string? ExpectedStatus,
    string? ExpectedActionKind,
    string? ExpectedTargetContains,
    string? ExpectedForegroundOwner,
    string? ExpectedForegroundActionLane,
    string? ExpectedChoiceExtractorPath);

sealed record GuiSmokeReplayTimingEntry(
    string Stage,
    long ElapsedMs,
    string? Detail = null);
