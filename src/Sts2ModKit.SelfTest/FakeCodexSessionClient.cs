using Sts2AiCompanion.Host;

internal sealed class FakeCodexSessionClient : ICodexSessionClient
{
    public int RequestCount { get; private set; }

    public Task<(AdviceResponse Response, string? SessionId)> ExecuteAsync(
        AdviceInputPack inputPack,
        string prompt,
        string? sessionId,
        string? modelOverride,
        string? reasoningEffortOverride,
        CancellationToken cancellationToken)
    {
        RequestCount += 1;
        var resolvedSessionId = sessionId ?? "fake-session-001";
        var recommendedChoiceLabel = SelectRecommendedChoiceLabel(inputPack)
                                     ?? inputPack.KnowledgeEntries.FirstOrDefault()?.Name;
        return Task.FromResult<(AdviceResponse Response, string? SessionId)>((
            new AdviceResponse(
                "ok",
                $"headline-{inputPack.TriggerKind}",
                $"summary-{inputPack.TriggerKind}",
                $"action-{inputPack.TriggerKind}",
                recommendedChoiceLabel,
                inputPack.RewardOptionSet is null ? new[] { "reason-1", "reason-2" } : Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                0.8,
                inputPack.RewardOptionSet is null ? inputPack.KnowledgeEntries.Take(3).Select(entry => entry.Id).ToArray() : Array.Empty<string>(),
                DateTimeOffset.UtcNow,
                inputPack.RunId,
                inputPack.TriggerKind,
                resolvedSessionId,
                "{\"status\":\"ok\"}",
                inputPack.RewardRecommendationTraceSeed),
            resolvedSessionId));
    }

    private static string? SelectRecommendedChoiceLabel(AdviceInputPack inputPack)
    {
        if (inputPack.RewardOptionSet is null)
        {
            return null;
        }

        return inputPack.RewardOptionSet.Options
            .Where(option => !option.IsSkipOption)
            .Select(option => new
            {
                option.Label,
                option.Ordinal,
                Score = ScoreOption(option.Label, inputPack.RewardAssessmentFacts),
            })
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Ordinal)
            .Select(candidate => candidate.Label)
            .FirstOrDefault();
    }

    private static int ScoreOption(string label, Sts2AiCompanion.Foundation.Contracts.RewardAssessmentFacts? facts)
    {
        if (facts is null)
        {
            return 0;
        }

        var score = 0;
        score += facts.SynergyHints.Count(hint => hint.StartsWith(label + ":", StringComparison.OrdinalIgnoreCase)) * 4;
        score -= facts.AntiSynergyHints.Count(hint => hint.StartsWith(label + ":", StringComparison.OrdinalIgnoreCase)) * 3;
        score -= facts.MissingInformation.Count(hint => hint.Contains(label, StringComparison.OrdinalIgnoreCase));
        return score;
    }
}
