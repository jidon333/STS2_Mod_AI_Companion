using System.Text.Json;
using System.Text.Json.Serialization;
using Sts2AiCompanion.Foundation.Artifacts;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Configuration;

namespace Sts2AiCompanion.Foundation.Knowledge;

public sealed class StrategyPrinciplesService
{
    private static readonly IReadOnlyDictionary<string, string[]> ScenePrincipleIds = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        ["reward"] = new[]
        {
            "sts.general.jobs_over_archetypes",
            "sts.general.skip_and_removal_reduce_variance",
            "sts.general.consistency_and_variance_control",
        },
        ["event"] = new[]
        {
            "sts.general.jobs_over_archetypes",
            "sts.general.risk_management_greed_vs_fear",
        },
        ["shop"] = new[]
        {
            "sts.general.skip_and_removal_reduce_variance",
            "sts.general.jobs_over_archetypes",
            "sts.general.hp_as_resource",
        },
        ["combat"] = new[]
        {
            "sts.general.frontload_vs_scaling",
            "sts.general.draw_selection_as_consistency",
            "sts.general.consistency_and_variance_control",
        },
    };

    private const string EnergyDrawPairResourceId = "sts.general.energy_draw_pair_resource";
    private const string FrontloadVsScalingId = "sts.general.frontload_vs_scaling";
    private const int MaxPrinciples = 3;

    private readonly string _artifactPath;
    private IReadOnlyDictionary<string, StrategyPrincipleArtifactEntry> _entries = new Dictionary<string, StrategyPrincipleArtifactEntry>(StringComparer.OrdinalIgnoreCase);
    private DateTimeOffset? _lastLoadedAt;

    public StrategyPrinciplesService(ScaffoldConfiguration configuration, string workspaceRoot)
    {
        _artifactPath = Path.Combine(
            CompanionPathResolver.ResolveKnowledgeRoot(configuration, workspaceRoot),
            "strategy",
            "strategy-principles.sts.json");
    }

    public IReadOnlyList<StrategyPrincipleEntry> GetRelevantPrinciples(RewardEventCompactAdvisorInput? compactInput)
    {
        ReloadIfChanged();
        if (compactInput is null)
        {
            return Array.Empty<StrategyPrincipleEntry>();
        }

        var sceneType = compactInput.SceneType?.Trim().ToLowerInvariant() switch
        {
            "rewards" => "reward",
            var normalized => normalized ?? "unknown",
        };
        if (!ScenePrincipleIds.TryGetValue(sceneType, out var defaults))
        {
            return Array.Empty<StrategyPrincipleEntry>();
        }

        var selected = new List<StrategyPrincipleEntry>();
        foreach (var principleId in defaults)
        {
            TryAddPrinciple(selected, principleId, allowMedium: false);
        }

        if (string.Equals(sceneType, "event", StringComparison.OrdinalIgnoreCase)
            && selected.Count < MaxPrinciples
            && ShouldIncludeEventFrontloadPrinciple(compactInput))
        {
            TryAddPrinciple(selected, FrontloadVsScalingId, allowMedium: false);
        }

        if (string.Equals(sceneType, "combat", StringComparison.OrdinalIgnoreCase)
            && selected.Count < MaxPrinciples
            && compactInput.CombatFacts?.Energy is not null)
        {
            TryAddPrinciple(selected, EnergyDrawPairResourceId, allowMedium: true);
        }

        return selected;
    }

    private void ReloadIfChanged()
    {
        if (!File.Exists(_artifactPath))
        {
            _entries = new Dictionary<string, StrategyPrincipleArtifactEntry>(StringComparer.OrdinalIgnoreCase);
            _lastLoadedAt = null;
            return;
        }

        var lastWriteTime = File.GetLastWriteTimeUtc(_artifactPath);
        if (_lastLoadedAt is not null && lastWriteTime <= _lastLoadedAt.Value.UtcDateTime)
        {
            return;
        }

        try
        {
            using var stream = new FileStream(_artifactPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var parsed = JsonSerializer.Deserialize<StrategyPrinciplesArtifact>(stream, ConfigurationLoader.JsonOptions);
            _entries = (parsed?.Entries ?? Array.Empty<StrategyPrincipleArtifactEntry>())
                .Where(static entry =>
                    !string.IsNullOrWhiteSpace(entry.Id)
                    && !string.IsNullOrWhiteSpace(entry.Title)
                    && !string.IsNullOrWhiteSpace(entry.Summary))
                .GroupBy(static entry => entry.Id, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(static group => group.Key, static group => group.First(), StringComparer.OrdinalIgnoreCase);
            _lastLoadedAt = new DateTimeOffset(lastWriteTime, TimeSpan.Zero);
        }
        catch
        {
            _entries = new Dictionary<string, StrategyPrincipleArtifactEntry>(StringComparer.OrdinalIgnoreCase);
            _lastLoadedAt = null;
        }
    }

    private void TryAddPrinciple(ICollection<StrategyPrincipleEntry> selected, string principleId, bool allowMedium)
    {
        if (selected.Count >= MaxPrinciples
            || !_entries.TryGetValue(principleId, out var entry)
            || !IsAllowedConfidence(entry.TransferConfidence, allowMedium)
            || selected.Any(existing => string.Equals(existing.Id, principleId, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        selected.Add(new StrategyPrincipleEntry(
            entry.Id,
            entry.Title,
            entry.Summary,
            entry.TransferConfidence));
    }

    private static bool IsAllowedConfidence(string? transferConfidence, bool allowMedium)
    {
        return transferConfidence?.Trim().ToLowerInvariant() switch
        {
            "high" => true,
            "medium" => allowMedium,
            _ => false,
        };
    }

    private static bool ShouldIncludeEventFrontloadPrinciple(RewardEventCompactAdvisorInput compactInput)
    {
        var effects = compactInput.EventFacts?.OptionFacts
            .SelectMany(static fact => fact.Effects)
            .Select(static effect => effect.Kind)
            .Where(static kind => !string.IsNullOrWhiteSpace(kind))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (effects is null || effects.Length == 0)
        {
            return false;
        }

        return effects.Any(static kind =>
            kind is "hp_loss" or "hp_gain" or "gold_loss" or "gold_gain"
                or "result_card" or "result_card_effect"
                or "result_relic" or "result_relic_effect"
                or "result_power" or "result_power_effect");
    }

    private sealed record StrategyPrinciplesArtifact(
        [property: JsonPropertyName("entries")]
        IReadOnlyList<StrategyPrincipleArtifactEntry>? Entries);

    private sealed record StrategyPrincipleArtifactEntry(
        [property: JsonPropertyName("id")]
        string Id,
        [property: JsonPropertyName("title")]
        string Title,
        [property: JsonPropertyName("summary")]
        string Summary,
        [property: JsonPropertyName("transfer_confidence")]
        string TransferConfidence);
}
