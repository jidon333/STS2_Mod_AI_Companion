using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Foundation.State;

public static class CompanionStateMapper
{
    public static CompanionState FromLiveExport(
        LiveExportSnapshot snapshot,
        LiveExportSession? session,
        IReadOnlyList<LiveExportEventEnvelope> recentEvents)
    {
        var sceneType = string.IsNullOrWhiteSpace(snapshot.CurrentScreen) ? "unknown" : snapshot.CurrentScreen;
        var confidence = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["scene"] = string.Equals(sceneType, "unknown", StringComparison.OrdinalIgnoreCase) ? 0.0 : 0.8,
            ["choices"] = snapshot.CurrentChoices.Count == 0 ? 0.2 : 0.8,
            ["deck"] = snapshot.Deck.Count == 0 ? 0.2 : 0.8,
            ["player"] = snapshot.Player.CurrentHp is null ? 0.3 : 0.9,
        };

        var unknowns = new List<string>();
        if (snapshot.Deck.Count == 0) unknowns.Add("deck-empty");
        if (snapshot.CurrentChoices.Count == 0) unknowns.Add("choices-empty");
        if (string.Equals(sceneType, "unknown", StringComparison.OrdinalIgnoreCase)) unknowns.Add("scene-unknown");

        var rewardEntries = IsRewardScene(sceneType)
            ? snapshot.CurrentChoices.Select(ToChoiceItem).ToArray()
            : Array.Empty<CompanionChoiceItem>();
        var eventEntries = IsEventScene(sceneType)
            ? snapshot.CurrentChoices.Select(ToChoiceItem).ToArray()
            : Array.Empty<CompanionChoiceItem>();
        var shopEntries = IsShopScene(sceneType)
            ? snapshot.CurrentChoices.Select(ToChoiceItem).ToArray()
            : Array.Empty<CompanionChoiceItem>();
        var restEntries = IsRestScene(sceneType)
            ? snapshot.CurrentChoices.Select(ToChoiceItem).ToArray()
            : Array.Empty<CompanionChoiceItem>();

        return new CompanionState(
            Guid.NewGuid().ToString("N"),
            snapshot.CapturedAt,
            new CompanionRunIdentity(
                snapshot.RunId,
                session?.SessionId,
                snapshot.RunStatus,
                snapshot.Act,
                snapshot.Floor,
                TryGetMeta(snapshot.Meta, "seed")),
            new CompanionSceneState(
                sceneType,
                sceneType,
                confidence["scene"],
                TryGetMeta(snapshot.Meta, "scene-source") ?? "live-export",
                TryGetMeta(snapshot.Meta, "screen-episode")),
            new CompanionPlayerState(
                snapshot.Player.CurrentHp,
                snapshot.Player.MaxHp,
                TryParseInt(TryGetMeta(snapshot.Meta, "block")),
                snapshot.Player.Energy,
                snapshot.Player.Gold,
                snapshot.Player.Resources,
                snapshot.Deck.Count == 0 ? "unknown" : $"{snapshot.Deck.Count} cards",
                snapshot.Relics,
                snapshot.Potions),
            new CompanionCombatState(
                snapshot.Encounter?.InCombat ?? false,
                snapshot.Encounter?.Kind,
                snapshot.Encounter?.Turn,
                TryGetMeta(snapshot.Meta, "hand-summary"),
                TryGetMeta(snapshot.Meta, "enemy-intent-summary")),
            new CompanionChoiceState(
                snapshot.CurrentChoices.Select(ToChoiceItem).ToArray(),
                TryGetMeta(snapshot.Meta, "choice-source") ?? "live-export",
                TryGetMeta(snapshot.Meta, "choice-extractor"),
                confidence["choices"]),
            new CompanionRewardState(
                IsRewardScene(sceneType) ? TryGetMeta(snapshot.Meta, "reward-type") ?? "generic" : null,
                rewardEntries,
                rewardEntries.Length > 0),
            new CompanionEventState(
                IsEventScene(sceneType) ? TryGetMeta(snapshot.Meta, "event-id") : null,
                IsEventScene(sceneType) ? TryGetMeta(snapshot.Meta, "event-page-id") : null,
                eventEntries),
            new CompanionShopState(
                Array.Empty<CompanionChoiceItem>(),
                shopEntries,
                shopEntries.All(entry => entry.Description?.Contains("price", StringComparison.OrdinalIgnoreCase) == true)),
            new CompanionRestState(restEntries),
            new CompanionMapState(
                TryGetMeta(snapshot.Meta, "map-node"),
                ParseDelimited(TryGetMeta(snapshot.Meta, "reachable-nodes"))),
            new CompanionModalState(
                TryGetMeta(snapshot.Meta, "modal-type"),
                bool.TryParse(TryGetMeta(snapshot.Meta, "modal-blocking"), out var isBlocking) && isBlocking),
            new CompanionTransitionState(
                recentEvents.LastOrDefault()?.Screen,
                sceneType,
                recentEvents.LastOrDefault()?.Kind),
            snapshot.Warnings,
            unknowns,
            confidence);
    }

    private static CompanionChoiceItem ToChoiceItem(LiveExportChoiceSummary summary)
    {
        return new CompanionChoiceItem(summary.Kind, summary.Label, summary.Value, summary.Description);
    }

    private static string? TryGetMeta(IReadOnlyDictionary<string, string?> meta, string key)
    {
        return meta.TryGetValue(key, out var value) ? value : null;
    }

    private static IReadOnlyList<string> ParseDelimited(string? raw)
    {
        return string.IsNullOrWhiteSpace(raw)
            ? Array.Empty<string>()
            : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static int? TryParseInt(string? value)
    {
        return int.TryParse(value, out var parsed) ? parsed : null;
    }

    private static bool IsRewardScene(string sceneType) => string.Equals(sceneType, "reward", StringComparison.OrdinalIgnoreCase) || string.Equals(sceneType, "rewards", StringComparison.OrdinalIgnoreCase);
    private static bool IsEventScene(string sceneType) => string.Equals(sceneType, "event", StringComparison.OrdinalIgnoreCase);
    private static bool IsShopScene(string sceneType) => string.Equals(sceneType, "shop", StringComparison.OrdinalIgnoreCase);
    private static bool IsRestScene(string sceneType) => string.Equals(sceneType, "rest", StringComparison.OrdinalIgnoreCase) || string.Equals(sceneType, "rest-site", StringComparison.OrdinalIgnoreCase);
}
