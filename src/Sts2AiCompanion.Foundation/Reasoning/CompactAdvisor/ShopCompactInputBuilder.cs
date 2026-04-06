using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

internal sealed class ShopCompactInputBuilder
{
    public CompactAdvisorBuildResult Build(CompanionRunState runState, KnowledgeSlice boundedSlice)
    {
        var options = BuildVisibleOptions(runState.Snapshot.CurrentChoices);
        var missingInformation = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var decisionBlockers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (options.Count == 0)
        {
            decisionBlockers.Add("shop-compact-input-insufficient");
            return new CompactAdvisorBuildResult(false, null, Array.Empty<string>(), decisionBlockers.ToArray(), "shop-compact-input-insufficient");
        }

        AddDuplicateLabelBlockers(options, decisionBlockers);

        var itemOptions = options.Where(IsShopItemOption).ToArray();
        var serviceOptions = options.Where(IsShopServiceOption).ToArray();
        var purchaseOptions = itemOptions.Concat(serviceOptions).ToArray();
        if (purchaseOptions.Length == 0)
        {
            decisionBlockers.Add("shop-compact-input-insufficient");
        }

        if (itemOptions.Any(static option => string.IsNullOrWhiteSpace(option.Description)
                                             || string.Equals(option.Description, "Merchant inventory slot", StringComparison.OrdinalIgnoreCase)))
        {
            missingInformation.Add("shop-item-effect-summary-missing");
        }

        var pricesKnown = purchaseOptions.Length > 0 && purchaseOptions.All(option => ContainsPrice(option.Description));
        if (!pricesKnown && purchaseOptions.Length > 0)
        {
            missingInformation.Add("shop-item-price-missing");
        }

        var inventoryOpen = TryReadBoolMeta(runState, "shopInventoryOpen") == true;
        var cardRemovalVisible = TryReadBoolMeta(runState, "shopCardRemovalVisible") == true
                                 || serviceOptions.Any();
        var cardRemovalAvailable = TryReadBoolMeta(runState, "shopCardRemovalEnabled") == true
                                   || serviceOptions.Any(static option => option.Enabled);

        var compactKnowledge = CompactAdvisorBuilderShared.FilterKnowledgeEntries(
            boundedSlice,
            options.SelectMany(option => new[] { option.Label, option.Value, option.Kind })
                .Append("shop")
                .Append(cardRemovalVisible ? "service:card-removal" : null));
        var shopFacts = new ShopCompactFacts(
            inventoryOpen,
            itemOptions.Length,
            serviceOptions.Length,
            purchaseOptions.Count(static option => option.Enabled),
            cardRemovalVisible,
            cardRemovalAvailable,
            pricesKnown,
            missingInformation.ToArray());
        var compactInput = new RewardEventCompactAdvisorInput(
            SceneType: "shop",
            SceneStage: inventoryOpen ? "inventory-open" : "merchant-entry",
            CanonicalOwner: "shop",
            RunContext: CompactAdvisorBuilderShared.BuildRunContext(runState),
            PlayerSummary: CompactAdvisorBuilderShared.BuildPlayerSummary(runState),
            VisibleOptions: options,
            KnowledgeEntries: compactKnowledge,
            RecentEvents: CompactAdvisorBuilderShared.FilterRecentEvents(runState.RecentEvents, IsRelevantShopEvent),
            MissingInformation: missingInformation.ToArray(),
            DecisionBlockers: decisionBlockers.ToArray(),
            ShopFacts: shopFacts);
        return decisionBlockers.Contains("shop-compact-input-insufficient")
            ? new CompactAdvisorBuildResult(false, compactInput, compactInput.MissingInformation, compactInput.DecisionBlockers, "shop-compact-input-insufficient")
            : new CompactAdvisorBuildResult(true, compactInput, compactInput.MissingInformation, compactInput.DecisionBlockers, "supported");
    }

    private static IReadOnlyList<CompactAdvisorOption> BuildVisibleOptions(IReadOnlyList<LiveExportChoiceSummary> snapshotChoices)
    {
        return CompactAdvisorBuilderShared.SanitizeSnapshotChoices(snapshotChoices)
            .Where(IsShopChoice)
            .GroupBy(
                choice => $"{choice.Kind}|{choice.Label}|{choice.Value ?? string.Empty}",
                StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .Select(choice => new CompactAdvisorOption(
                choice.Kind,
                choice.Label,
                choice.Value,
                choice.Description,
                choice.Enabled ?? true,
                choice.SemanticHints.Where(hint => !string.IsNullOrWhiteSpace(hint)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()))
            .ToArray();
    }

    private static void AddDuplicateLabelBlockers(
        IReadOnlyList<CompactAdvisorOption> options,
        HashSet<string> decisionBlockers)
    {
        var duplicateLabels = options
            .GroupBy(option => option.Label, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        foreach (var label in duplicateLabels)
        {
            decisionBlockers.Add($"shop-duplicate-option-label:{label}");
        }

        if (duplicateLabels.Length > 0)
        {
            decisionBlockers.Add("shop-compact-input-insufficient");
        }
    }

    private static bool IsShopChoice(LiveExportChoiceSummary choice)
    {
        return choice.Kind.StartsWith("shop", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => hint.Contains("shop", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsShopItemOption(CompactAdvisorOption option)
    {
        return option.Kind is "shop-option:card" or "shop-option:relic" or "shop-option:potion";
    }

    private static bool IsShopServiceOption(CompactAdvisorOption option)
    {
        return string.Equals(option.Kind, "shop-card-removal", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsPrice(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return false;
        }

        return description.Contains("price", StringComparison.OrdinalIgnoreCase)
               || description.Contains("cost", StringComparison.OrdinalIgnoreCase)
               || description.Contains("골드", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRelevantShopEvent(LiveExportEventEnvelope envelope)
    {
        return string.Equals(envelope.Screen, "shop", StringComparison.OrdinalIgnoreCase)
               || envelope.Kind is "shop-opened" or "choice-list-presented";
    }

    private static bool? TryReadBoolMeta(CompanionRunState runState, string key)
    {
        return bool.TryParse(CompactAdvisorBuilderShared.TryGetMeta(runState, key), out var parsed) ? parsed : null;
    }
}
