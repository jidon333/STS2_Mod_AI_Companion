using Sts2AiCompanion.Host;
using Sts2AiCompanion.Foundation.Reasoning;
using Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

internal sealed class FakeCodexSessionClient : ICodexSessionClient
{
    private readonly FakeCodexRewardResponseMode rewardResponseMode;
    private readonly TimeSpan responseDelay;

    public FakeCodexSessionClient(
        FakeCodexRewardResponseMode rewardResponseMode = FakeCodexRewardResponseMode.ModelRationaleOutput,
        TimeSpan? responseDelay = null)
    {
        this.rewardResponseMode = rewardResponseMode;
        this.responseDelay = responseDelay ?? TimeSpan.Zero;
    }

    public int RequestCount { get; private set; }

    internal AdviceResponse BuildRawResponseForTesting(AdviceInputPack inputPack, string? sessionId = null)
    {
        return BuildResponse(inputPack, sessionId ?? "fake-session-001");
    }

    public Task<(AdviceResponse Response, string? SessionId)> ExecuteAsync(
        AdviceInputPack inputPack,
        string prompt,
        string? sessionId,
        string? modelOverride,
        string? reasoningEffortOverride,
        CancellationToken cancellationToken)
    {
        return ExecuteAsyncCore(inputPack, sessionId, cancellationToken);
    }

    private async Task<(AdviceResponse Response, string? SessionId)> ExecuteAsyncCore(
        AdviceInputPack inputPack,
        string? sessionId,
        CancellationToken cancellationToken)
    {
        RequestCount += 1;
        if (responseDelay > TimeSpan.Zero)
        {
            await Task.Delay(responseDelay, cancellationToken).ConfigureAwait(false);
        }

        var resolvedSessionId = sessionId ?? "fake-session-001";
        var rawResponse = BuildRawResponseForTesting(inputPack, resolvedSessionId);
        return (
            ToHostResponse(AdviceResponseFinalizer.Apply(ToFoundationInputPack(inputPack), ToFoundationResponse(rawResponse))),
            resolvedSessionId);
    }

    private AdviceResponse BuildResponse(AdviceInputPack inputPack, string resolvedSessionId)
    {
        var recommendedChoiceLabel = SelectRecommendedChoiceLabel(inputPack)
                                     ?? inputPack.KnowledgeEntries.FirstOrDefault()?.Name;
        if (inputPack.CompactInput is not null)
        {
            return rewardResponseMode switch
            {
                FakeCodexRewardResponseMode.MinimalModelOutput => new AdviceResponse(
                    "ok",
                    $"headline-{inputPack.TriggerKind}",
                    $"summary-{inputPack.TriggerKind}",
                    $"action-{inputPack.TriggerKind}",
                    recommendedChoiceLabel,
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    0.8,
                    Array.Empty<string>(),
                    DateTimeOffset.UtcNow,
                    inputPack.RunId,
                    inputPack.TriggerKind,
                    resolvedSessionId,
                    "{\"status\":\"ok\"}",
                    inputPack.RewardRecommendationTraceSeed),
                _ => BuildCompactRationaleResponse(inputPack, resolvedSessionId, recommendedChoiceLabel),
            };
        }

        if (inputPack.RewardOptionSet is null)
        {
            return new AdviceResponse(
                "ok",
                $"headline-{inputPack.TriggerKind}",
                $"summary-{inputPack.TriggerKind}",
                $"action-{inputPack.TriggerKind}",
                recommendedChoiceLabel,
                new[] { "reason-1", "reason-2" },
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                0.8,
                inputPack.KnowledgeEntries.Take(3).Select(entry => entry.Id).ToArray(),
                DateTimeOffset.UtcNow,
                inputPack.RunId,
                inputPack.TriggerKind,
                resolvedSessionId,
                "{\"status\":\"ok\"}",
                inputPack.RewardRecommendationTraceSeed);
        }

        return rewardResponseMode switch
        {
            FakeCodexRewardResponseMode.MinimalModelOutput => new AdviceResponse(
                "ok",
                $"headline-{inputPack.TriggerKind}",
                $"summary-{inputPack.TriggerKind}",
                $"action-{inputPack.TriggerKind}",
                recommendedChoiceLabel,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                0.8,
                Array.Empty<string>(),
                DateTimeOffset.UtcNow,
                inputPack.RunId,
                inputPack.TriggerKind,
                resolvedSessionId,
                "{\"status\":\"ok\"}",
                inputPack.RewardRecommendationTraceSeed),
            _ => BuildRewardRationaleResponse(inputPack, resolvedSessionId, recommendedChoiceLabel),
        };
    }

    private AdviceResponse BuildCompactRationaleResponse(AdviceInputPack inputPack, string resolvedSessionId, string? recommendedChoiceLabel)
    {
        var compact = inputPack.CompactInput
            ?? throw new InvalidOperationException("CompactInput was expected.");
        var reasoning = new List<string>
        {
            $"현재 장면: {compact.SceneType} / {compact.SceneStage}",
            $"보이는 선택지: {string.Join(", ", compact.VisibleOptions.Select(option => option.Label))}",
        };

        if (!string.IsNullOrWhiteSpace(recommendedChoiceLabel))
        {
            reasoning.Add($"추천 라벨 '{recommendedChoiceLabel}'은 현재 visible options와 exact match 됩니다.");

            if (inputPack.RewardAssessmentFacts is not null)
            {
                foreach (var hint in inputPack.RewardAssessmentFacts.SynergyHints.Where(hint => hint.StartsWith(recommendedChoiceLabel + ":", StringComparison.OrdinalIgnoreCase)))
                {
                    reasoning.Add($"deterministic 사실: {hint}");
                }

                foreach (var hint in inputPack.RewardAssessmentFacts.AntiSynergyHints.Where(hint => hint.StartsWith(recommendedChoiceLabel + ":", StringComparison.OrdinalIgnoreCase)))
                {
                    reasoning.Add($"deterministic 사실: {hint}");
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(recommendedChoiceLabel) && inputPack.RewardAssessmentFacts is not null)
        {
            reasoning.AddRange(
                inputPack.RewardAssessmentFacts.SynergyHints
                    .Where(hint => hint.StartsWith(recommendedChoiceLabel + ":", StringComparison.OrdinalIgnoreCase))
                    .Take(2)
                    .Select(hint => $"deterministic 사실: {hint}"));
            reasoning.AddRange(
                inputPack.RewardAssessmentFacts.AntiSynergyHints
                    .Where(hint => hint.StartsWith(recommendedChoiceLabel + ":", StringComparison.OrdinalIgnoreCase))
                    .Take(2)
                    .Select(hint => $"deterministic 사실: {hint}"));
        }

        if (inputPack.RewardRecommendationTraceSeed is not null)
        {
            reasoning.AddRange(inputPack.RewardRecommendationTraceSeed.AssessmentFactLines.Take(2));
        }

        if (compact.RewardFacts is not null)
        {
            reasoning.AddRange(
                compact.RewardFacts.FactLines
                    .Take(2)
                    .Select(line => $"deterministic 사실: {line}"));
        }

        if (compact.EventFacts is not null)
        {
            reasoning.Add($"event_id={compact.EventFacts.EventId ?? "unknown"}");
            var firstFact = compact.EventFacts.OptionFacts.FirstOrDefault(fact => string.Equals(fact.Label, recommendedChoiceLabel, StringComparison.Ordinal))
                            ?? compact.EventFacts.OptionFacts.FirstOrDefault();
            if (firstFact is not null && firstFact.Effects.Count > 0)
            {
                reasoning.AddRange(firstFact.Effects.Select(effect => $"{firstFact.Label}: {effect.Kind} {effect.Amount?.ToString() ?? string.Empty}".Trim()).Take(2));
            }
        }

        if (inputPack.RewardAssessmentFacts is not null)
        {
            reasoning.Add($"현재 덱 facts: attack_pressure={inputPack.RewardAssessmentFacts.AttackPressure}, defense_pressure={inputPack.RewardAssessmentFacts.DefensePressure}, draw_support={inputPack.RewardAssessmentFacts.DrawSupportLevel}, energy_support={inputPack.RewardAssessmentFacts.EnergySupportLevel}");
        }

        return new AdviceResponse(
            "ok",
            $"headline-{inputPack.TriggerKind}",
            $"summary-{inputPack.TriggerKind}",
            $"action-{inputPack.TriggerKind}",
            recommendedChoiceLabel,
            reasoning
                .Where(bullet => !string.IsNullOrWhiteSpace(bullet))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(7)
                .ToArray(),
            Array.Empty<string>(),
            compact.MissingInformation.ToArray(),
            compact.DecisionBlockers.ToArray(),
            0.8,
            compact.KnowledgeEntries.Select(entry => entry.Id).Take(3).ToArray(),
            DateTimeOffset.UtcNow,
            inputPack.RunId,
            inputPack.TriggerKind,
            resolvedSessionId,
            "{\"status\":\"ok\"}",
            inputPack.RewardRecommendationTraceSeed);
    }

    private AdviceResponse BuildRewardRationaleResponse(AdviceInputPack inputPack, string resolvedSessionId, string? recommendedChoiceLabel)
    {
        var facts = inputPack.RewardAssessmentFacts;
        var reasoning = new List<string>();
        if (inputPack.RewardOptionSet is not null)
        {
            reasoning.Add($"현재 보이는 card reward 옵션: {string.Join(", ", inputPack.RewardOptionSet.Options.Select(option => option.Label))}");
        }

        if (!string.IsNullOrWhiteSpace(recommendedChoiceLabel))
        {
            reasoning.Add($"추천 라벨 '{recommendedChoiceLabel}'은 visible option set 안에 있습니다.");
            if (facts is not null)
            {
                foreach (var hint in facts.SynergyHints.Where(hint => hint.StartsWith(recommendedChoiceLabel + ":", StringComparison.OrdinalIgnoreCase)))
                {
                    reasoning.Add($"deterministic 사실: {hint}");
                }

                foreach (var hint in facts.AntiSynergyHints.Where(hint => hint.StartsWith(recommendedChoiceLabel + ":", StringComparison.OrdinalIgnoreCase)))
                {
                    reasoning.Add($"deterministic 사실: {hint}");
                }
            }
        }

        if (facts is not null)
        {
            reasoning.Add($"현재 덱 facts: attack_pressure={facts.AttackPressure}, defense_pressure={facts.DefensePressure}, draw_support={facts.DrawSupportLevel}, energy_support={facts.EnergySupportLevel}");
        }

        return new AdviceResponse(
            "ok",
            $"headline-{inputPack.TriggerKind}",
            $"summary-{inputPack.TriggerKind}",
            $"action-{inputPack.TriggerKind}",
            recommendedChoiceLabel,
            reasoning
                .Where(bullet => !string.IsNullOrWhiteSpace(bullet))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .ToArray(),
            Array.Empty<string>(),
            facts?.MissingInformation.ToArray() ?? Array.Empty<string>(),
            Array.Empty<string>(),
            0.8,
            inputPack.RewardRecommendationTraceSeed?.InputKnowledgeRefs.Take(3).ToArray() ?? Array.Empty<string>(),
            DateTimeOffset.UtcNow,
            inputPack.RunId,
            inputPack.TriggerKind,
            resolvedSessionId,
            "{\"status\":\"ok\"}",
            inputPack.RewardRecommendationTraceSeed);
    }

    private static string? SelectRecommendedChoiceLabel(AdviceInputPack inputPack)
    {
        if (inputPack.CompactInput is not null)
        {
            return SelectCompactChoiceLabel(inputPack);
        }

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

    private static string? SelectCompactChoiceLabel(AdviceInputPack inputPack)
    {
        var compact = inputPack.CompactInput!;
        if (string.Equals(compact.SceneType, "event", StringComparison.OrdinalIgnoreCase))
        {
            return compact.VisibleOptions
                .Where(option => option.Enabled)
                .OrderBy(option => option.Tags.Contains("proceed", StringComparer.OrdinalIgnoreCase))
                .ThenBy(option => option.Label, StringComparer.Ordinal)
                .Select(option => option.Label)
                .FirstOrDefault();
        }

        if (inputPack.RewardOptionSet is not null)
        {
            return SelectRecommendedChoiceLabel(inputPack with { CompactInput = null });
        }

        return compact.VisibleOptions
            .Where(option => option.Enabled)
            .Where(option => !option.Tags.Contains("skip-option", StringComparer.OrdinalIgnoreCase))
            .Select(option => option.Label)
            .FirstOrDefault()
            ?? compact.VisibleOptions.FirstOrDefault()?.Label;
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

    private static Sts2AiCompanion.Foundation.Contracts.AdviceInputPack ToFoundationInputPack(AdviceInputPack inputPack)
    {
        return new Sts2AiCompanion.Foundation.Contracts.AdviceInputPack(
            inputPack.RunId,
            inputPack.TriggerKind,
            inputPack.CreatedAt,
            inputPack.Manual,
            inputPack.CurrentScreen,
            inputPack.SummaryText,
            inputPack.Snapshot,
            inputPack.RecentEvents.ToArray(),
            inputPack.KnowledgeEntries.ToArray(),
            inputPack.KnowledgeReasons.ToArray(),
            inputPack.ConstraintsText,
            inputPack.NormalizedState,
            inputPack.RewardOptionSet,
            inputPack.RewardAssessmentFacts,
            inputPack.RewardRecommendationTraceSeed,
            inputPack.CompactInput);
    }

    private static Sts2AiCompanion.Foundation.Contracts.AdviceResponse ToFoundationResponse(AdviceResponse response)
    {
        return new Sts2AiCompanion.Foundation.Contracts.AdviceResponse(
            response.Status,
            response.Headline,
            response.Summary,
            response.RecommendedAction,
            response.RecommendedChoiceLabel,
            response.ReasoningBullets.ToArray(),
            response.RiskNotes.ToArray(),
            response.MissingInformation.ToArray(),
            response.DecisionBlockers.ToArray(),
            response.Confidence,
            response.KnowledgeRefs.ToArray(),
            response.GeneratedAt,
            response.RunId,
            response.TriggerKind,
            response.SessionId,
            response.RawResponse,
            response.RewardRecommendationTrace);
    }

    private static AdviceResponse ToHostResponse(Sts2AiCompanion.Foundation.Contracts.AdviceResponse response)
    {
        return new AdviceResponse(
            response.Status,
            response.Headline,
            response.Summary,
            response.RecommendedAction,
            response.RecommendedChoiceLabel,
            response.ReasoningBullets.ToArray(),
            response.RiskNotes.ToArray(),
            response.MissingInformation.ToArray(),
            response.DecisionBlockers.ToArray(),
            response.Confidence,
            response.KnowledgeRefs.ToArray(),
            response.GeneratedAt,
            response.RunId,
            response.TriggerKind,
            response.SessionId,
            response.RawResponse,
            response.RewardRecommendationTrace);
    }
}

internal enum FakeCodexRewardResponseMode
{
    MinimalModelOutput,
    ModelRationaleOutput,
}
