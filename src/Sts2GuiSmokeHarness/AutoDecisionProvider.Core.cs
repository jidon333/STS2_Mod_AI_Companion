using System.Text.Json;

sealed partial class AutoDecisionProvider
{
    private sealed class CandidateWorkItem
    {
        public required string Label { get; init; }
        public required string Source { get; init; }
        public required double Score { get; init; }
        public string? RejectReason { get; set; }
        public string? RawBounds { get; init; }
        public GuiSmokeCandidatePoint? NormalizedPoint { get; init; }
        public string? BoundsSource { get; init; }
        public string? TargetLabel { get; init; }
        public string? ActionKind { get; init; }
        public string? Reason { get; init; }
        public bool Selected { get; set; }
        public GuiSmokeStepDecision? Decision { get; init; }
    }

    private sealed class DecisionAnalysisBuilder
    {
        private readonly GuiSmokeStepRequest _request;
        private readonly List<CandidateWorkItem> _candidates = new();
        private readonly List<string> _activeCandidateSet = new();
        private readonly List<GuiSmokeSuppressedCandidate> _suppressedCandidates = new();
        private GuiSmokeStepDecision? _predictedDecision;
        private string _winnerSelectionReason = "no viable candidate";

        public DecisionAnalysisBuilder(GuiSmokeStepRequest request, string? foregroundKind, string? backgroundKind)
        {
            _request = request;
            ForegroundKind = foregroundKind;
            BackgroundKind = backgroundKind;
        }

        public string? ForegroundKind { get; }

        public string? BackgroundKind { get; }

        public void AddSuppressed(string label, string reason)
        {
            _suppressedCandidates.Add(new GuiSmokeSuppressedCandidate(label, reason));
        }

        public void Consider(
            string label,
            string source,
            double score,
            Func<GuiSmokeStepDecision?> factory,
            string rejectReason,
            string? rawBounds = null,
            string? boundsSource = null,
            GuiSmokeCandidatePoint? normalizedPoint = null)
        {
            _activeCandidateSet.Add(label);
            GuiSmokeStepDecision? decision = null;
            string? candidateRejectReason = null;
            try
            {
                decision = factory();
            }
            catch (Exception exception)
            {
                candidateRejectReason = $"factory-exception:{exception.GetType().Name}";
            }

            if (decision is null && candidateRejectReason is null)
            {
                candidateRejectReason = rejectReason;
            }

            if (decision is not null && _predictedDecision is not null)
            {
                candidateRejectReason = $"lower-priority-than:{_predictedDecision.TargetLabel ?? _predictedDecision.ActionKind ?? _predictedDecision.Status}";
            }

            if (decision is not null && _predictedDecision is null)
            {
                _predictedDecision = decision;
                _winnerSelectionReason = $"selected first viable candidate '{label}' from {source}.";
            }

            var point = normalizedPoint;
            if (point is null && decision?.NormalizedX is { } normalizedX && decision.NormalizedY is { } normalizedY)
            {
                point = new GuiSmokeCandidatePoint(normalizedX, normalizedY);
            }

            _candidates.Add(new CandidateWorkItem
            {
                Label = label,
                Source = source,
                Score = score,
                RejectReason = candidateRejectReason,
                RawBounds = rawBounds,
                NormalizedPoint = point,
                BoundsSource = boundsSource,
                TargetLabel = decision?.TargetLabel,
                ActionKind = decision?.ActionKind,
                Reason = decision?.Reason,
                Decision = decision,
            });
        }

        public GuiSmokeDecisionAnalysis Build(GuiSmokeStepDecision fallbackDecision, GuiSmokeStepDecision? actualDecision = null)
        {
            _predictedDecision ??= fallbackDecision;
            var finalDecision = actualDecision ?? _predictedDecision;
            var matchesPredicted = AreEquivalent(_predictedDecision, finalDecision);
            var selectedCandidate = _candidates.FirstOrDefault(candidate => AreEquivalent(candidate.Decision, finalDecision));
            if (selectedCandidate is not null)
            {
                selectedCandidate.Selected = true;
            }
            else
            {
                _candidates.Add(CreateExternalDecisionCandidate(finalDecision));
                _winnerSelectionReason = matchesPredicted
                    ? _winnerSelectionReason
                    : $"provider selected '{finalDecision.TargetLabel ?? finalDecision.ActionKind ?? finalDecision.Status}' outside the predicted candidate set.";
            }

            if (!matchesPredicted)
            {
                var predictedCandidate = _candidates.FirstOrDefault(candidate => AreEquivalent(candidate.Decision, _predictedDecision));
                if (predictedCandidate is not null && !predictedCandidate.Selected)
                {
                    predictedCandidate.RejectReason ??= "not-selected-by-provider";
                }
            }

            return new GuiSmokeDecisionAnalysis(
                _request.Phase,
                _request.ScreenshotPath,
                _predictedDecision,
                finalDecision,
                new GuiSmokeDecisionDebugSummary(
                    ForegroundKind,
                    BackgroundKind,
                    _activeCandidateSet.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                    _suppressedCandidates,
                    _winnerSelectionReason),
                _candidates.Select(candidate => new GuiSmokeDecisionCandidateDump(
                    candidate.Label,
                    candidate.Source,
                    candidate.Score,
                    candidate.Selected,
                    candidate.RejectReason,
                    candidate.RawBounds,
                    candidate.NormalizedPoint,
                    candidate.BoundsSource,
                    candidate.TargetLabel,
                    candidate.ActionKind,
                    candidate.Reason)).ToArray(),
                matchesPredicted);
        }

        private static CandidateWorkItem CreateExternalDecisionCandidate(GuiSmokeStepDecision decision)
        {
            return new CandidateWorkItem
            {
                Label = decision.TargetLabel ?? decision.ActionKind ?? decision.Status,
                Source = "provider:external",
                Score = 1.10d,
                Selected = true,
                RejectReason = null,
                RawBounds = null,
                NormalizedPoint = decision.NormalizedX is { } x && decision.NormalizedY is { } y
                    ? new GuiSmokeCandidatePoint(x, y)
                    : null,
                BoundsSource = decision.ActionKind is "click" or "right-click" ? "provider-external" : "provider-external-nonclick",
                TargetLabel = decision.TargetLabel,
                ActionKind = decision.ActionKind,
                Reason = decision.Reason,
                Decision = decision,
            };
        }

        private static bool AreEquivalent(GuiSmokeStepDecision? left, GuiSmokeStepDecision? right)
        {
            if (left is null || right is null)
            {
                return false;
            }

            return string.Equals(left.Status, right.Status, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(left.ActionKind, right.ActionKind, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(left.TargetLabel, right.TargetLabel, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(left.KeyText, right.KeyText, StringComparison.OrdinalIgnoreCase);
        }
    }

    public async Task<GuiSmokeStepDecision> GetDecisionAsync(string requestPath, string decisionPath, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var stream = new FileStream(requestPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var request = await JsonSerializer.DeserializeAsync<GuiSmokeStepRequest>(stream, GuiSmokeShared.JsonOptions, cancellationToken).ConfigureAwait(false)
                     ?? throw new InvalidOperationException("Failed to parse step request.");
        return Decide(request);
    }

    public static GuiSmokeDecisionAnalysis Analyze(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision? actualDecision = null,
        GuiSmokeStepAnalysisContext? analysisContext = null)
    {
        var context = analysisContext ?? GuiSmokeStepAnalysisContext.CreateForHandleCombatRequest(request);
        var phase = Enum.Parse<GuiSmokePhase>(request.Phase, ignoreCase: true);
        return phase switch
        {
            GuiSmokePhase.HandleEvent => AnalyzeHandleEvent(request, actualDecision),
            GuiSmokePhase.HandleRewards => AnalyzeHandleRewards(request, actualDecision),
            GuiSmokePhase.ChooseFirstNode => AnalyzeChooseFirstNode(request, actualDecision),
            GuiSmokePhase.HandleShop => AnalyzeGenericPhase(request, actualDecision, () => DecideHandleShop(request), "shop", null),
            GuiSmokePhase.HandleCombat => AnalyzeGenericPhase(request, actualDecision, () => DecideHandleCombat(request, context), "combat", null),
            GuiSmokePhase.EnterRun => AnalyzeGenericPhase(request, actualDecision, () => DecideEnterRun(request), "main-menu", null),
            GuiSmokePhase.WaitRunLoad => AnalyzeGenericPhase(request, actualDecision, () => DecideWaitRunLoad(request), request.Observer.CurrentScreen, null),
            GuiSmokePhase.ChooseCharacter => AnalyzeGenericPhase(request, actualDecision, () => DecideChooseCharacter(request), "character-select", null),
            GuiSmokePhase.Embark => AnalyzeGenericPhase(request, actualDecision, () => DecideEmbark(request), "embark", null),
            _ => AnalyzeGenericPhase(request, actualDecision, () => CreateWaitDecision("waiting for passive phase", request.Observer.CurrentScreen), request.Observer.CurrentScreen, null),
        };
    }

    public static GuiSmokeStepDecision Decide(GuiSmokeStepRequest request)
    {
        return Analyze(request).FinalDecision;
    }
}
