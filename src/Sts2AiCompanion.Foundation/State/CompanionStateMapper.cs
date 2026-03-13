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
        var sanitizedChoices = SanitizeChoices(snapshot.CurrentChoices);
        var normalizedScene = NormalizeScene(snapshot);
        var sceneType = normalizedScene.SceneType;
        var confidence = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["scene"] = normalizedScene.Confidence,
            ["choices"] = sanitizedChoices.Count == 0 ? 0.2 : 0.8,
            ["deck"] = snapshot.Deck.Count == 0 ? 0.2 : 0.8,
            ["player"] = snapshot.Player.CurrentHp is null ? 0.3 : 0.9,
        };

        var unknowns = new List<string>();
        if (snapshot.Deck.Count == 0) unknowns.Add("deck-empty");
        if (sanitizedChoices.Count == 0) unknowns.Add("choices-empty");
        if (string.Equals(sceneType, "unknown", StringComparison.OrdinalIgnoreCase)) unknowns.Add("scene-unknown");

        var rewardEntries = IsRewardScene(sceneType)
            ? sanitizedChoices.Select(ToChoiceItem).ToArray()
            : Array.Empty<CompanionChoiceItem>();
        var eventEntries = IsEventScene(sceneType)
            ? sanitizedChoices.Select(ToChoiceItem).ToArray()
            : Array.Empty<CompanionChoiceItem>();
        var shopEntries = IsShopScene(sceneType)
            ? sanitizedChoices.Select(ToChoiceItem).ToArray()
            : Array.Empty<CompanionChoiceItem>();
        var restEntries = IsRestScene(sceneType)
            ? sanitizedChoices.Select(ToChoiceItem).ToArray()
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
                normalizedScene.SemanticSceneType,
                confidence["scene"],
                normalizedScene.Source,
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
                sanitizedChoices.Select(ToChoiceItem).ToArray(),
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

    private static NormalizedScene NormalizeScene(LiveExportSnapshot snapshot)
    {
        var rawScene = string.IsNullOrWhiteSpace(snapshot.CurrentScreen) ? "unknown" : snapshot.CurrentScreen.Trim();
        var currentSceneType = TryGetMeta(snapshot.Meta, "currentSceneType");
        var rootTypeSummary = TryGetMeta(snapshot.Meta, "rootTypeSummary");
        var modalType = TryGetMeta(snapshot.Meta, "modal-type");
        var choiceLabels = SanitizeChoices(snapshot.CurrentChoices)
            .Select(choice => choice.Label)
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .ToArray();
        var overlayChoiceNoise = snapshot.CurrentChoices.Count > 0 && choiceLabels.Length == 0;
        var currentSceneTypeLooksLikeCharacterSelect = ContainsSceneMarker(currentSceneType, null, "NCharacterSelectScreen");
        var currentSceneTypeLooksLikeSingleplayerSubmenu = ContainsSceneMarker(currentSceneType, null, "NSingleplayerSubmenu");
        var feedbackOverlayVisible = ContainsSceneMarker(currentSceneType, rootTypeSummary, "NSendFeedbackScreen");
        var timeoutOverlayVisible = ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMultiplayerTimeoutOverlay");
        var combatMarkersVisible = ContainsSceneMarker(currentSceneType, rootTypeSummary, "NCombatRoom")
                                   || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NCombatScreen")
                                   || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NCombatHud")
                                   || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NPlayerHand")
                                   || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NTargetManager")
                                   || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NSelectionReticle")
                                   || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NTargetingArrow");

        if (feedbackOverlayVisible && overlayChoiceNoise)
        {
            return new NormalizedScene("feedback-overlay", "feedback-overlay", 0.92, "meta:feedback-overlay");
        }

        if (timeoutOverlayVisible && overlayChoiceNoise)
        {
            return new NormalizedScene("blocking-overlay", "blocking-overlay", 0.92, "meta:timeout-overlay");
        }

        var looksLikeMainMenu = LooksLikeMainMenuNormalized(choiceLabels)
                                || string.Equals(rawScene, "main-menu", StringComparison.OrdinalIgnoreCase)
                                || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMainMenu");
        var looksLikeCharacterSelect = LooksLikeCharacterSelectNormalized(choiceLabels)
                                       || currentSceneTypeLooksLikeCharacterSelect;
        var looksLikeSingleplayerSubmenu = LooksLikeSingleplayerSubmenuNormalized(choiceLabels)
                                           || currentSceneTypeLooksLikeSingleplayerSubmenu;
        var looksLikeSemanticGameplayScreen = looksLikeMainMenu
                                              || looksLikeCharacterSelect
                                              || looksLikeSingleplayerSubmenu
                                              || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMapScreen")
                                              || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NCardRewardSelectionScreen")
                                              || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NRewardsScreen")
                                              || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NEventLayout")
                                              || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NEventRoom")
                                              || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMerchantInventory")
                                              || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMerchantRoom")
                                              || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMerchant")
                                              || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NRestSiteRoom")
                                              || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NRestSite");

        if (!looksLikeSemanticGameplayScreen
            && ContainsSceneMarker(currentSceneType, rootTypeSummary, "NSendFeedbackScreen"))
        {
            return new NormalizedScene("feedback-overlay", "feedback-overlay", 0.85, "meta:feedback-overlay");
        }

        if (!looksLikeSemanticGameplayScreen
            && ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMultiplayerTimeoutOverlay"))
        {
            return new NormalizedScene("blocking-overlay", "blocking-overlay", 0.85, "meta:timeout-overlay");
        }

        if (string.Equals(rawScene, "main-menu", StringComparison.OrdinalIgnoreCase))
        {
            if (LooksLikeCharacterSelectNormalized(choiceLabels))
            {
                return new NormalizedScene("character-select", "character-select", 0.96, "choices:character-select");
            }

            if (LooksLikeSingleplayerSubmenuNormalized(choiceLabels))
            {
                return new NormalizedScene("singleplayer-submenu", "singleplayer-submenu", 0.94, "choices:singleplayer-submenu");
            }

            if (LooksLikeMainMenuNormalized(choiceLabels) || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMainMenu"))
            {
                return new NormalizedScene("main-menu", "main-menu", 0.98, "raw:main-menu");
            }
        }

        if (currentSceneTypeLooksLikeCharacterSelect)
        {
            return new NormalizedScene("character-select", "character-select", 0.97, "meta:character-select");
        }

        if (currentSceneTypeLooksLikeSingleplayerSubmenu)
        {
            return new NormalizedScene("singleplayer-submenu", "singleplayer-submenu", 0.95, "meta:singleplayer-submenu");
        }

        if (string.Equals(rawScene, "combat", StringComparison.OrdinalIgnoreCase)
            || (snapshot.Encounter?.InCombat == true && combatMarkersVisible))
        {
            return new NormalizedScene("combat", "combat", 0.96, "raw/meta:combat");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMapScreen"))
        {
            return new NormalizedScene("map", "map", 0.94, "meta:map");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NCardRewardSelectionScreen")
            || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NRewardsScreen"))
        {
            return new NormalizedScene("rewards", "rewards", 0.96, "meta:rewards");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NEventLayout")
            || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NEventRoom"))
        {
            return new NormalizedScene("event", "event", 0.95, "meta:event");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMerchantInventory")
            || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMerchantRoom")
            || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMerchant"))
        {
            return new NormalizedScene("shop", "shop", 0.94, "meta:shop");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NRestSiteRoom")
            || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NRestSite"))
        {
            return new NormalizedScene("rest-site", "rest-site", 0.94, "meta:rest-site");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMainMenu")
            && (LooksLikeMainMenuNormalized(choiceLabels) || string.Equals(rawScene, "main-menu", StringComparison.OrdinalIgnoreCase)))
        {
            return new NormalizedScene("main-menu", "main-menu", 0.95, "meta:main-menu");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NLogoAnimation"))
        {
            return new NormalizedScene("startup", "startup", 0.9, "meta:logo-animation");
        }

        if (string.Equals(rawScene, "main-menu", StringComparison.OrdinalIgnoreCase)
            && !ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMainMenu")
            && (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NGame")
                || !string.IsNullOrWhiteSpace(modalType)))
        {
            return new NormalizedScene("startup", "startup", 0.6, "meta:startup-fallback");
        }

        return new NormalizedScene(
            rawScene,
            rawScene,
            string.Equals(rawScene, "unknown", StringComparison.OrdinalIgnoreCase) ? 0.0 : 0.8,
            TryGetMeta(snapshot.Meta, "scene-source") ?? "live-export");
    }

    private static IReadOnlyList<LiveExportChoiceSummary> SanitizeChoices(IReadOnlyList<LiveExportChoiceSummary> choices)
    {
        return choices
            .Where(choice => !IsOverlayChoice(choice.Label))
            .ToArray();
    }

    private static bool IsOverlayChoice(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        var normalized = label.Trim();
        return normalized.Equals("Dismisser", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("Exclaim", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("Question", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("BackButton", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("Send!", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsSceneMarker(string? currentSceneType, string? rootTypeSummary, string marker)
    {
        return (!string.IsNullOrWhiteSpace(currentSceneType) && currentSceneType.Contains(marker, StringComparison.OrdinalIgnoreCase))
               || (!string.IsNullOrWhiteSpace(rootTypeSummary) && rootTypeSummary.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeMainMenuNormalized(IReadOnlyList<string> labels)
    {
        var candidates = new[]
        {
            "\uC2F1\uAE00\uD50C\uB808\uC774", "singleplayer",
            "\uBA40\uD2F0\uD50C\uB808\uC774", "multiplayer",
            "\uC5F0\uB300\uD45C", "leaderboard",
            "\uC124\uC815", "settings",
            "\uBC31\uACFC\uC0AC\uC804", "compendium",
            "\uC885\uB8CC", "quit",
        };

        return labels.Any(label => candidates.Any(candidate => label.Contains(candidate, StringComparison.OrdinalIgnoreCase)));
    }

    private static bool LooksLikeCharacterSelectNormalized(IReadOnlyList<string> labels)
    {
        var candidates = new[]
        {
            "\uC544\uC774\uC5B8\uD074\uB798\uB4DC", "ironclad",
            "\uC0AC\uC77C\uB7F0\uD2B8", "silent",
            "\uB514\uD399\uD2B8", "defect",
            "\uB9AC\uC820\uD2B8", "regent",
            "\uB124\uD06C\uB85C\uBC14\uC778\uB354", "necrobinder",
            "\uCD9C\uBC1C", "embark",
        };

        return labels.Any(label => candidates.Any(candidate => label.Contains(candidate, StringComparison.OrdinalIgnoreCase)));
    }

    private static bool LooksLikeSingleplayerSubmenuNormalized(IReadOnlyList<string> labels)
    {
        var candidates = new[]
        {
            "\uC77C\uBC18", "standard",
            "\uC8FC\uAC04", "weekly",
            "\uC2DC\uB4DC", "seed",
            "\uC77C\uC77C", "daily",
        };

        return labels.Any(label => candidates.Any(candidate => label.Contains(candidate, StringComparison.OrdinalIgnoreCase)));
    }

    private static bool LooksLikeMainMenu(IReadOnlyList<string> labels)
    {
        var candidates = new[]
        {
            "싱글플레이", "singleplayer",
            "멀티플레이", "multiplayer",
            "연대표", "leaderboard",
            "설정", "settings",
            "백과사전", "compendium",
            "종료", "quit",
        };
        return labels.Any(label => candidates.Any(candidate => label.Contains(candidate, StringComparison.OrdinalIgnoreCase)));
    }

    private static bool LooksLikeCharacterSelect(IReadOnlyList<string> labels)
    {
        var characterCandidates = new[]
        {
            "아이언클래드", "ironclad",
            "사일런트", "silent",
            "디펙트", "defect",
            "레전트", "regent",
            "네크로", "necrobinder",
            "출발", "embark",
        };
        return labels.Any(label => characterCandidates.Any(candidate => label.Contains(candidate, StringComparison.OrdinalIgnoreCase)));
    }

    private static bool LooksLikeSingleplayerSubmenu(IReadOnlyList<string> labels)
    {
        var submenuCandidates = new[]
        {
            "표준", "standard",
            "주간", "weekly",
            "시드", "seed",
            "일일", "daily",
        };
        return labels.Any(label => submenuCandidates.Any(candidate => label.Contains(candidate, StringComparison.OrdinalIgnoreCase)));
    }

    private sealed record NormalizedScene(string SceneType, string SemanticSceneType, double Confidence, string Source);
}
