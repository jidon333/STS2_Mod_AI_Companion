using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Foundation.State;

public sealed record CompanionNormalizedScene(
    string SceneType,
    string SemanticSceneType,
    double Confidence,
    string Source);

public static class CompanionSceneNormalizer
{
    public static CompanionNormalizedScene Normalize(LiveExportSnapshot snapshot)
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
            return new CompanionNormalizedScene("feedback-overlay", "feedback-overlay", 0.92, "meta:feedback-overlay");
        }

        if (timeoutOverlayVisible && overlayChoiceNoise)
        {
            return new CompanionNormalizedScene("blocking-overlay", "blocking-overlay", 0.92, "meta:timeout-overlay");
        }

        var looksLikeMainMenu = LooksLikeMainMenu(choiceLabels)
                                || string.Equals(rawScene, "main-menu", StringComparison.OrdinalIgnoreCase)
                                || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMainMenu");
        var looksLikeCharacterSelect = LooksLikeCharacterSelect(choiceLabels)
                                       || currentSceneTypeLooksLikeCharacterSelect;
        var looksLikeSingleplayerSubmenu = LooksLikeSingleplayerSubmenu(choiceLabels)
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

        if (!looksLikeSemanticGameplayScreen && feedbackOverlayVisible)
        {
            return new CompanionNormalizedScene("feedback-overlay", "feedback-overlay", 0.85, "meta:feedback-overlay");
        }

        if (!looksLikeSemanticGameplayScreen && timeoutOverlayVisible)
        {
            return new CompanionNormalizedScene("blocking-overlay", "blocking-overlay", 0.85, "meta:timeout-overlay");
        }

        if (string.Equals(rawScene, "main-menu", StringComparison.OrdinalIgnoreCase))
        {
            if (LooksLikeCharacterSelect(choiceLabels))
            {
                return new CompanionNormalizedScene("character-select", "character-select", 0.96, "choices:character-select");
            }

            if (LooksLikeSingleplayerSubmenu(choiceLabels))
            {
                return new CompanionNormalizedScene("singleplayer-submenu", "singleplayer-submenu", 0.94, "choices:singleplayer-submenu");
            }

            if (LooksLikeMainMenu(choiceLabels) || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMainMenu"))
            {
                return new CompanionNormalizedScene("main-menu", "main-menu", 0.98, "raw:main-menu");
            }
        }

        if (currentSceneTypeLooksLikeCharacterSelect)
        {
            return new CompanionNormalizedScene("character-select", "character-select", 0.97, "meta:character-select");
        }

        if (currentSceneTypeLooksLikeSingleplayerSubmenu)
        {
            return new CompanionNormalizedScene("singleplayer-submenu", "singleplayer-submenu", 0.95, "meta:singleplayer-submenu");
        }

        if (string.Equals(rawScene, "combat", StringComparison.OrdinalIgnoreCase)
            || (snapshot.Encounter?.InCombat == true && combatMarkersVisible))
        {
            return new CompanionNormalizedScene("combat", "combat", 0.96, "raw/meta:combat");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NCardRewardSelectionScreen")
            || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NRewardsScreen"))
        {
            return new CompanionNormalizedScene("rewards", "rewards", 0.96, "meta:rewards");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NEventLayout")
            || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NEventRoom"))
        {
            return new CompanionNormalizedScene("event", "event", 0.95, "meta:event");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMerchantInventory")
            || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMerchantRoom")
            || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMerchant"))
        {
            return new CompanionNormalizedScene("shop", "shop", 0.94, "meta:shop");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NRestSiteRoom")
            || ContainsSceneMarker(currentSceneType, rootTypeSummary, "NRestSite"))
        {
            return new CompanionNormalizedScene("rest-site", "rest-site", 0.94, "meta:rest-site");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMapScreen"))
        {
            return new CompanionNormalizedScene("map", "map", 0.94, "meta:map");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMainMenu")
            && (LooksLikeMainMenu(choiceLabels) || string.Equals(rawScene, "main-menu", StringComparison.OrdinalIgnoreCase)))
        {
            return new CompanionNormalizedScene("main-menu", "main-menu", 0.95, "meta:main-menu");
        }

        if (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NLogoAnimation"))
        {
            return new CompanionNormalizedScene("startup", "startup", 0.9, "meta:logo-animation");
        }

        if (string.Equals(rawScene, "main-menu", StringComparison.OrdinalIgnoreCase)
            && !ContainsSceneMarker(currentSceneType, rootTypeSummary, "NMainMenu")
            && (ContainsSceneMarker(currentSceneType, rootTypeSummary, "NGame")
                || !string.IsNullOrWhiteSpace(modalType)))
        {
            return new CompanionNormalizedScene("startup", "startup", 0.6, "meta:startup-fallback");
        }

        return new CompanionNormalizedScene(
            rawScene,
            rawScene,
            string.Equals(rawScene, "unknown", StringComparison.OrdinalIgnoreCase) ? 0.0 : 0.8,
            TryGetMeta(snapshot.Meta, "scene-source") ?? "live-export");
    }

    public static IReadOnlyList<LiveExportChoiceSummary> SanitizeChoices(IReadOnlyList<LiveExportChoiceSummary> choices)
    {
        return choices
            .Where(choice => !IsOverlayChoice(choice.Label))
            .ToArray();
    }

    public static bool IsOverlayChoice(string? label)
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

    public static bool IsStableSceneForImmediateInventoryPublish(string sceneType)
    {
        return sceneType is "main-menu"
            or "singleplayer-submenu"
            or "character-select"
            or "map"
            or "rewards"
            or "event"
            or "shop"
            or "rest-site";
    }

    private static string? TryGetMeta(IReadOnlyDictionary<string, string?> meta, string key)
    {
        return meta.TryGetValue(key, out var value) ? value : null;
    }

    private static bool ContainsSceneMarker(string? currentSceneType, string? rootTypeSummary, string marker)
    {
        return (!string.IsNullOrWhiteSpace(currentSceneType) && currentSceneType.Contains(marker, StringComparison.OrdinalIgnoreCase))
               || (!string.IsNullOrWhiteSpace(rootTypeSummary) && rootTypeSummary.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeMainMenu(IReadOnlyList<string> labels)
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

    private static bool LooksLikeCharacterSelect(IReadOnlyList<string> labels)
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

    private static bool LooksLikeSingleplayerSubmenu(IReadOnlyList<string> labels)
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
}
