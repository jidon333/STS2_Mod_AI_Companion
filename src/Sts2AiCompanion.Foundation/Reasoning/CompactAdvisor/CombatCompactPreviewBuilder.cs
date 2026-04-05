using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

internal sealed class CombatCompactPreviewBuilder
{
    public CompactAdvisorBuildResult Build(CompanionRunState runState, KnowledgeSlice boundedSlice)
    {
        var options = BuildVisibleOptions(runState.Snapshot.CurrentChoices);
        var missingInformation = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var decisionBlockers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "combat-preview-only",
        };

        var snapshot = runState.Snapshot;
        var energy = snapshot.Player.Energy;
        var handSummary = CompactAdvisorBuilderShared.FirstNonBlank(
            CompactAdvisorBuilderShared.TryGetMeta(runState, "combatHandSummary"),
            runState.NormalizedState.Combat.HandSummary);
        var fallbackHandCount = options.Count(IsHandCardOption);
        var handCount = snapshot.CombatHandCount
                        ?? TryReadIntMeta(runState, "combatHandCount")
                        ?? (fallbackHandCount > 0 ? fallbackHandCount : null);
        var targetableEnemyCount = TryReadIntMeta(runState, "combatTargetableEnemyCount");
        var hittableEnemyCount = TryReadIntMeta(runState, "combatHittableEnemyCount");
        var targetingInProgress = TryReadBoolMeta(runState, "combatTargetingInProgress") == true;
        var targetSummary = CompactAdvisorBuilderShared.TryGetMeta(runState, "combatTargetSummary");
        var enemyIntentSummary = CompactAdvisorBuilderShared.FirstNonBlank(
            snapshot.EnemyIntentSummary,
            CompactAdvisorBuilderShared.TryGetMeta(runState, "enemyIntentSummary"),
            CompactAdvisorBuilderShared.TryGetMeta(runState, "enemy-intent-summary"),
            runState.NormalizedState.Combat.EnemyIntentSummary);
        var roundNumber = TryReadIntMeta(runState, "combatRoundNumber");

        if (energy is null)
        {
            missingInformation.Add("combat-energy-missing");
        }

        if (handCount is null)
        {
            missingInformation.Add("combat-hand-count-missing");
        }

        if (snapshot.DrawPileCount is null
            || snapshot.DiscardPileCount is null
            || snapshot.ExhaustPileCount is null
            || snapshot.PlayPileCount is null)
        {
            missingInformation.Add("combat-pile-counts-partial");
        }

        if (string.IsNullOrWhiteSpace(enemyIntentSummary))
        {
            missingInformation.Add("combat-enemy-intent-summary-missing");
        }

        if (energy is null
            || handCount is null
            || (snapshot.DrawPileCount is null
                && snapshot.DiscardPileCount is null
                && snapshot.ExhaustPileCount is null
                && snapshot.PlayPileCount is null))
        {
            decisionBlockers.Add("combat-compact-input-insufficient");
        }

        var compactKnowledge = CompactAdvisorBuilderShared.FilterKnowledgeEntries(
            boundedSlice,
            options.SelectMany(option => new[] { option.Label, option.Value, option.Kind })
                .Append("combat"));
        var combatFacts = new CombatCompactFacts(
            PreviewOnly: true,
            CurrentHp: snapshot.Player.CurrentHp,
            MaxHp: snapshot.Player.MaxHp,
            Energy: energy,
            TurnNumber: runState.NormalizedState.Combat.Turn,
            RoundNumber: roundNumber,
            HandCount: handCount,
            HandSummary: handSummary,
            DrawPileCount: snapshot.DrawPileCount,
            DiscardPileCount: snapshot.DiscardPileCount,
            ExhaustPileCount: snapshot.ExhaustPileCount,
            PlayPileCount: snapshot.PlayPileCount,
            TargetableEnemyCount: targetableEnemyCount,
            HittableEnemyCount: hittableEnemyCount,
            TargetingInProgress: targetingInProgress,
            TargetSummary: targetSummary,
            EnemyIntentSummary: enemyIntentSummary,
            MissingInformation: missingInformation.ToArray());
        var compactInput = new RewardEventCompactAdvisorInput(
            SceneType: "combat",
            SceneStage: ResolveCombatStage(runState, handCount, targetingInProgress),
            CanonicalOwner: "combat",
            RunContext: CompactAdvisorBuilderShared.BuildRunContext(runState),
            PlayerSummary: CompactAdvisorBuilderShared.BuildPlayerSummary(runState),
            VisibleOptions: options,
            KnowledgeEntries: compactKnowledge,
            RecentEvents: CompactAdvisorBuilderShared.FilterRecentEvents(runState.RecentEvents, IsRelevantCombatEvent),
            MissingInformation: missingInformation.ToArray(),
            DecisionBlockers: decisionBlockers.ToArray(),
            CombatFacts: combatFacts);
        var reasonCode = decisionBlockers.Contains("combat-compact-input-insufficient")
            ? "combat-compact-input-insufficient"
            : "combat-preview-only";
        return new CompactAdvisorBuildResult(false, compactInput, compactInput.MissingInformation, compactInput.DecisionBlockers, reasonCode);
    }

    private static IReadOnlyList<CompactAdvisorOption> BuildVisibleOptions(IReadOnlyList<LiveExportChoiceSummary> snapshotChoices)
    {
        return snapshotChoices
            .Where(IsPreviewVisibleChoice)
            .Select(ToCombatOption)
            .ToArray();
    }

    private static CompactAdvisorOption ToCombatOption(LiveExportChoiceSummary choice)
    {
        var tags = choice.SemanticHints
            .Where(hint => !string.IsNullOrWhiteSpace(hint))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var category = ResolveCombatOptionCategory(choice);
        if (!string.IsNullOrWhiteSpace(category))
        {
            tags.Add($"category:{category}");
        }

        return new CompactAdvisorOption(
            choice.Kind,
            choice.Label,
            choice.Value,
            choice.Description,
            choice.Enabled ?? true,
            tags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static bool IsPreviewVisibleChoice(LiveExportChoiceSummary choice)
    {
        return IsCombatHandChoice(choice)
               || IsCombatPileButtonChoice(choice)
               || IsEndTurnChoice(choice);
    }

    private static string? ResolveCombatOptionCategory(LiveExportChoiceSummary choice)
    {
        if (IsEndTurnChoice(choice))
        {
            return "combat-action";
        }

        if (IsCombatPileButtonChoice(choice))
        {
            return "pile-button";
        }

        if (IsCombatHandChoice(choice))
        {
            return "hand-card";
        }

        return null;
    }

    private static bool IsHandCardOption(CompactAdvisorOption option)
    {
        return option.Tags.Contains("category:hand-card", StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsCombatHandChoice(LiveExportChoiceSummary choice)
    {
        return (string.Equals(choice.Kind, "card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.Kind, "combat-hand-card", StringComparison.OrdinalIgnoreCase))
               && !IsCombatPileButtonChoice(choice)
               && !IsEndTurnChoice(choice)
               && !IsCombatUtilityChoice(choice);
    }

    private static bool IsEndTurnChoice(LiveExportChoiceSummary choice)
    {
        return choice.Label.Contains("턴 종료", StringComparison.OrdinalIgnoreCase)
               || choice.Label.Contains("End Turn", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Contains("combat-end-turn", StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsCombatPileButtonChoice(LiveExportChoiceSummary choice)
    {
        return TryGetCombatPileId(choice) is not null;
    }

    private static bool IsCombatUtilityChoice(LiveExportChoiceSummary choice)
    {
        return choice.Label.Contains("Ping", StringComparison.OrdinalIgnoreCase)
               || choice.Kind.Contains("ping", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => hint.Contains("ping", StringComparison.OrdinalIgnoreCase));
    }

    private static string? TryGetCombatPileId(LiveExportChoiceSummary choice)
    {
        var seeds = new[]
        {
            choice.Label,
            choice.Value,
            choice.BindingId,
            choice.NodeId,
            string.Join(' ', choice.SemanticHints),
        };
        foreach (var seed in seeds)
        {
            if (string.IsNullOrWhiteSpace(seed))
            {
                continue;
            }

            if (seed.Contains("DrawPile", StringComparison.OrdinalIgnoreCase))
            {
                return "draw";
            }

            if (seed.Contains("DiscardPile", StringComparison.OrdinalIgnoreCase))
            {
                return "discard";
            }

            if (seed.Contains("ExhaustPile", StringComparison.OrdinalIgnoreCase))
            {
                return "exhaust";
            }

            if (seed.Contains("PlayPile", StringComparison.OrdinalIgnoreCase))
            {
                return "play";
            }
        }

        return null;
    }

    private static string ResolveCombatStage(CompanionRunState runState, int? handCount, bool targetingInProgress)
    {
        return CompactAdvisorBuilderShared.FirstNonBlank(
                   CompactAdvisorBuilderShared.TryGetMeta(runState, "foregroundActionLane"),
                   targetingInProgress ? "targeting" : null,
                   handCount.GetValueOrDefault() > 0 ? "player-play-open" : null,
                   "combat-runtime")!;
    }

    private static bool IsRelevantCombatEvent(LiveExportEventEnvelope envelope)
    {
        return string.Equals(envelope.Screen, "combat", StringComparison.OrdinalIgnoreCase)
               || envelope.Kind is "combat-started" or "choice-list-presented";
    }

    private static bool? TryReadBoolMeta(CompanionRunState runState, string key)
    {
        return bool.TryParse(CompactAdvisorBuilderShared.TryGetMeta(runState, key), out var parsed) ? parsed : null;
    }

    private static int? TryReadIntMeta(CompanionRunState runState, string key)
    {
        return int.TryParse(CompactAdvisorBuilderShared.TryGetMeta(runState, key), out var parsed) ? parsed : null;
    }
}
