using System;
using System.Collections.Generic;
using System.Linq;
using Sts2AiCompanion.AdvisorSceneDisplay;
using Sts2AiCompanion.AdvisorSceneModel;

namespace Sts2AiCompanion.Wpf.Display;

internal static class AdvisorSceneDisplayFormatter
{
    public static string FormatIdentity(AdvisorSceneArtifact? scene)
    {
        return scene is null
            ? "없음"
            : $"{scene.SceneType} / {scene.SceneStage} / {scene.CanonicalOwner}";
    }

    public static string FormatContext(AdvisorSceneArtifact? scene, AdvisorKnowledgeDisplayResolver resolver)
    {
        if (scene is null)
        {
            return "scene model이 아직 없습니다.";
        }

        var lines = new List<string>
        {
            $"{TranslateScene(scene.SceneType)} 장면 맥락",
            BuildPlayerLine(scene.PlayerContext),
            BuildSceneLine(scene),
        };

        var extraLines = BuildDetailLines(scene, resolver);
        if (extraLines.Count > 0)
        {
            lines.AddRange(extraLines);
        }

        return JoinLines(lines.Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    public static string FormatSummary(AdvisorSceneArtifact? scene, AdvisorKnowledgeDisplayResolver resolver)
    {
        if (scene is null)
        {
            return "scene model이 아직 없습니다.";
        }

        var lines = new List<string>
        {
            $"{TranslateScene(scene.SceneType)} 장면입니다.",
            BuildHeadline(scene),
        };

        var displaySummary = BuildDisplaySummaryLine(scene, resolver);
        if (!string.IsNullOrWhiteSpace(displaySummary))
        {
            lines.Add(displaySummary);
        }

        return JoinLines(lines.Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    public static string FormatOptions(AdvisorSceneArtifact? scene, AdvisorKnowledgeDisplayResolver resolver)
    {
        if (scene is null || scene.Options.Count == 0)
        {
            return "없음";
        }

        var visibleOptions = scene.Options
            .Where(option => !IsUtilityOption(option))
            .ToArray();

        if (visibleOptions.Length == 0)
        {
            return "없음";
        }

        if (string.Equals(scene.SceneType, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return FormatCombatOptions(visibleOptions, resolver);
        }

        if (string.Equals(scene.SceneType, "event", StringComparison.OrdinalIgnoreCase))
        {
            return JoinLines(visibleOptions.Select(option => FormatOptionLine(option, resolver, preserveLabel: true)));
        }

        return JoinLines(visibleOptions.Select(option => FormatOptionLine(option, resolver, preserveLabel: false)));
    }

    public static string FormatGaps(AdvisorSceneArtifact? scene)
    {
        if (scene is null)
        {
            return JoinLines(new[]
            {
                "missing facts",
                "- 없음",
                string.Empty,
                "observer gaps",
                "- 없음",
            });
        }

        return JoinLines(new[]
        {
            "missing facts",
            FormatBulletSection(scene.MissingFacts),
            string.Empty,
            "observer gaps",
            FormatBulletSection(scene.ObserverGaps),
        });
    }

    public static string FormatProvenance(AdvisorSceneArtifact? scene)
    {
        if (scene is null)
        {
            return JoinLines(new[]
            {
                "confidence",
                "- 없음",
                string.Empty,
                "source refs",
                "- 없음",
            });
        }

        return JoinLines(new[]
        {
            "confidence",
            FormatConfidence(scene.Confidence),
            string.Empty,
            "source refs",
            FormatBulletSection(scene.SourceRefs),
        });
    }

    private static string BuildHeadline(AdvisorSceneArtifact scene)
    {
        return scene.SceneType switch
        {
            "combat" => BuildCombatHeadline(scene),
            "reward" => BuildRewardHeadline(scene),
            "event" => BuildEventHeadline(scene),
            "rest-site" => BuildRestSiteHeadline(scene),
            "shop" => BuildShopHeadline(scene),
            "map" => BuildMapHeadline(scene),
            _ => $"{scene.SceneType} / {scene.SceneStage}",
        };
    }

    private static string BuildSceneLine(AdvisorSceneArtifact scene)
    {
        return scene.SceneType switch
        {
            "combat" => BuildCombatSceneLine(scene),
            "reward" => $"보상 단계: {scene.Reward?.ReleaseStage ?? scene.SceneStage}",
            "event" => $"이벤트 단계: {scene.Event?.AncientContractLane ?? scene.SceneStage}",
            "rest-site" => $"휴식처 단계: {scene.RestSite?.ReleaseStage ?? scene.SceneStage}",
            "shop" => $"상점 단계: {(scene.Shop?.InventoryOpen == true ? "inventory-open" : scene.SceneStage)}",
            "map" => $"맵 단계: {(scene.Map?.CurrentNodeArrowVisible == true ? "reachable" : scene.SceneStage)}",
            _ => $"{scene.SceneStage}",
        };
    }

    private static string? BuildDisplaySummaryLine(AdvisorSceneArtifact scene, AdvisorKnowledgeDisplayResolver resolver)
    {
        return scene.SceneType switch
        {
            "combat" => BuildCombatDisplaySummary(scene, resolver),
            "reward" => BuildRewardDisplaySummary(scene),
            "event" => BuildEventDisplaySummary(scene),
            "rest-site" => BuildRestSiteDisplaySummary(scene),
            "shop" => BuildShopDisplaySummary(scene),
            "map" => BuildMapDisplaySummary(scene),
            _ => AdvisorDisplaySanitizer.SanitizeText(scene.SummaryText),
        };
    }

    private static IReadOnlyList<string> BuildDetailLines(AdvisorSceneArtifact scene, AdvisorKnowledgeDisplayResolver resolver)
    {
        return scene.SceneType switch
        {
            "combat" => BuildCombatDetailLines(scene, resolver),
            "reward" => BuildRewardDetailLines(scene),
            "event" => BuildEventDetailLines(scene),
            "rest-site" => BuildRestSiteDetailLines(scene),
            "shop" => BuildShopDetailLines(scene),
            "map" => BuildMapDetailLines(scene),
            _ => Array.Empty<string>(),
        };
    }

    private static string BuildPlayerLine(AdvisorScenePlayerContext context)
    {
        var segments = new List<string>
        {
            $"플레이어: HP {FormatInt(context.CurrentHp)}/{FormatInt(context.MaxHp)}",
            $"에너지 {FormatInt(context.Energy)}",
            $"골드 {FormatInt(context.Gold)}",
            $"덱 {context.DeckCount}장",
        };

        if (context.Relics.Count > 0)
        {
            segments.Add($"유물 {context.Relics.Count}개");
        }

        if (context.Potions.Count > 0)
        {
            segments.Add($"포션 {context.Potions.Count}개");
        }

        return string.Join(" · ", segments);
    }

    private static string BuildCombatHeadline(AdvisorSceneArtifact scene)
    {
        var combat = scene.Combat;
        if (combat is null)
        {
            return $"전투 장면({scene.SceneStage})";
        }

        return $"전투 장면({combat.LifecycleStage})";
    }

    private static string BuildCombatSceneLine(AdvisorSceneArtifact scene)
    {
        var combat = scene.Combat;
        if (combat is null)
        {
            return scene.SceneStage;
        }

        var segments = new List<string>
        {
            $"손패 {combat.HandCount}장",
            $"타겟 가능 적 {FormatOptionalInt(combat.TargetableEnemyCount)}",
        };

        if (!string.IsNullOrWhiteSpace(combat.EnemyIntentSummary))
        {
            segments.Add($"적 의도 {AdvisorDisplaySanitizer.Sanitize(combat.EnemyIntentSummary)}");
        }

        return string.Join(" · ", segments);
    }

    private static string BuildCombatDisplaySummary(AdvisorSceneArtifact scene, AdvisorKnowledgeDisplayResolver resolver)
    {
        var combat = scene.Combat;
        if (combat is null)
        {
            return AdvisorDisplaySanitizer.SanitizeText(scene.SummaryText) ?? scene.SceneStage;
        }

        var segments = new List<string>
        {
            $"손패 {combat.HandCount}장",
            $"타겟 가능 적 {FormatOptionalInt(combat.TargetableEnemyCount)}",
        };

        if (!string.IsNullOrWhiteSpace(combat.EnemyIntentSummary))
        {
            segments.Add($"적 의도 {AdvisorDisplaySanitizer.Sanitize(combat.EnemyIntentSummary)}");
        }

        var handSummary = CombatHandSummaryDisplayFormatter.Format(combat.HandSummary, resolver);
        if (!string.IsNullOrWhiteSpace(handSummary))
        {
            segments.Add($"핵심 손패 {handSummary}");
        }

        return string.Join(" · ", segments);
    }

    private static IReadOnlyList<string> BuildCombatDetailLines(AdvisorSceneArtifact scene, AdvisorKnowledgeDisplayResolver resolver)
    {
        var combat = scene.Combat;
        if (combat is null)
        {
            return Array.Empty<string>();
        }

        var lines = new List<string>
        {
            $"턴: {FormatOptionalInt(combat.Turn)}",
            $"체력: {FormatInt(scene.PlayerContext.CurrentHp)}/{FormatInt(scene.PlayerContext.MaxHp)}",
            $"에너지: {FormatInt(scene.PlayerContext.Energy)}",
            $"손패 수: {combat.HandCount}",
        };

        if (!string.IsNullOrWhiteSpace(combat.HandSummary))
        {
            lines.Add($"손패 요약: {CombatHandSummaryDisplayFormatter.Format(combat.HandSummary, resolver)}");
        }

        lines.Add($"타겟 가능 적: {FormatOptionalInt(combat.TargetableEnemyCount)}");
        lines.Add($"명시적 피격 적: {FormatOptionalInt(combat.HittableEnemyCount)}");

        if (combat.DrawPileCount is not null || combat.DiscardPileCount is not null || combat.ExhaustPileCount is not null || combat.PlayPileCount is not null)
        {
            lines.Add(
                $"더미: draw {FormatOptionalInt(combat.DrawPileCount)} / discard {FormatOptionalInt(combat.DiscardPileCount)} / exhaust {FormatOptionalInt(combat.ExhaustPileCount)} / play {FormatOptionalInt(combat.PlayPileCount)}");
        }

        if (!string.IsNullOrWhiteSpace(combat.EnemyIntentSummary))
        {
            lines.Add($"적 의도: {AdvisorDisplaySanitizer.Sanitize(combat.EnemyIntentSummary)}");
        }

        return lines;
    }

    private static string BuildRewardDisplaySummary(AdvisorSceneArtifact scene)
    {
        var reward = scene.Reward;
        if (reward is null)
        {
            return scene.SceneStage;
        }

        return $"보상 항목 {reward.RewardEntryCount}개 · claim {(reward.ClaimableRewardPresent ? "가능" : "없음")} · proceed {(reward.ExplicitProceedVisible ? "보임" : "숨김")}";
    }

    private static string BuildEventDisplaySummary(AdvisorSceneArtifact scene)
    {
        var evt = scene.Event;
        if (evt is null)
        {
            return scene.SceneStage;
        }

        return $"{(string.IsNullOrWhiteSpace(evt.EventIdentity) ? "이벤트 식별자 미상" : evt.EventIdentity)} · 선택지 {evt.OptionCount}개 · reward substate {(evt.RewardSubstateActive ? "있음" : "없음")}";
    }

    private static string BuildRestSiteDisplaySummary(AdvisorSceneArtifact scene)
    {
        var restSite = scene.RestSite;
        if (restSite is null)
        {
            return scene.SceneStage;
        }

        return $"선택지 {restSite.ActionCount}개 · smith {(restSite.SmithUpgradeActive ? "열림" : "닫힘")} · proceed {(restSite.ProceedVisible ? "보임" : "숨김")}";
    }

    private static string BuildShopDisplaySummary(AdvisorSceneArtifact scene)
    {
        var shop = scene.Shop;
        if (shop is null)
        {
            return scene.SceneStage;
        }

        var breakdown = BuildShopOptionBreakdown(scene.Options);
        return $"골드 {FormatInt(scene.PlayerContext.Gold)} · 현재 인식된 상품 {shop.ItemCount}개({breakdown.ItemsSummary}) · 서비스 {shop.ServiceCount}개 · 제거 {(shop.CardRemovalVisible ? "가능" : "없음")}";
    }

    private static string BuildMapDisplaySummary(AdvisorSceneArtifact scene)
    {
        var map = scene.Map;
        if (map is null)
        {
            return scene.SceneStage;
        }

        var segments = new List<string>
        {
            $"reachable node {map.ReachableNodeCount}개",
        };

        if (!string.IsNullOrWhiteSpace(map.CurrentNode))
        {
            segments.Add($"current node {map.CurrentNode}");
        }

        if (scene.MissingFacts.Contains("map-route-context-missing", StringComparer.OrdinalIgnoreCase))
        {
            segments.Add("route gap 있음");
        }

        return string.Join(" · ", segments);
    }

    private static string BuildRewardHeadline(AdvisorSceneArtifact scene)
    {
        var reward = scene.Reward;
        if (reward is null)
        {
            return $"보상 장면({scene.SceneStage})";
        }

        return $"보상 장면({reward.ReleaseStage})";
    }

    private static IReadOnlyList<string> BuildRewardDetailLines(AdvisorSceneArtifact scene)
    {
        var reward = scene.Reward;
        if (reward is null)
        {
            return Array.Empty<string>();
        }

        return new[]
        {
            $"보상 항목: {reward.RewardEntryCount}개",
            $"claim 가능: {(reward.ClaimableRewardPresent ? "예" : "아니오")}",
            $"proceed 표면: {(reward.ExplicitProceedVisible ? "예" : "아니오")}",
        };
    }

    private static string BuildEventHeadline(AdvisorSceneArtifact scene)
    {
        var evt = scene.Event;
        if (evt is null)
        {
            return $"이벤트 장면({scene.SceneStage})";
        }

        return $"이벤트 장면({evt.AncientContractLane})";
    }

    private static IReadOnlyList<string> BuildEventDetailLines(AdvisorSceneArtifact scene)
    {
        var evt = scene.Event;
        if (evt is null)
        {
            return Array.Empty<string>();
        }

        return new[]
        {
            $"선택지: {evt.OptionCount}개",
            $"reward substate: {(evt.RewardSubstateActive ? "예" : "아니오")}",
            $"proceed 표면: {(evt.ExplicitProceedVisible ? "예" : "아니오")}",
        };
    }

    private static string BuildRestSiteHeadline(AdvisorSceneArtifact scene)
    {
        var restSite = scene.RestSite;
        if (restSite is null)
        {
            return $"휴식처 장면({scene.SceneStage})";
        }

        return $"휴식처 장면({restSite.ReleaseStage})";
    }

    private static IReadOnlyList<string> BuildRestSiteDetailLines(AdvisorSceneArtifact scene)
    {
        var restSite = scene.RestSite;
        if (restSite is null)
        {
            return Array.Empty<string>();
        }

        return new[]
        {
            $"actions: {restSite.ActionCount}개",
            $"smith 활성: {(restSite.SmithUpgradeActive ? "예" : "아니오")}",
            $"proceed 표면: {(restSite.ProceedVisible ? "예" : "아니오")}",
        };
    }

    private static string BuildShopHeadline(AdvisorSceneArtifact scene)
    {
        var shop = scene.Shop;
        if (shop is null)
        {
            return $"상점 장면({scene.SceneStage})";
        }

        return $"상점 장면({(shop.InventoryOpen ? "inventory-open" : scene.SceneStage)})";
    }

    private static IReadOnlyList<string> BuildShopDetailLines(AdvisorSceneArtifact scene)
    {
        var shop = scene.Shop;
        if (shop is null)
        {
            return Array.Empty<string>();
        }

        var breakdown = BuildShopOptionBreakdown(scene.Options);

        return new[]
        {
            $"현재 인식된 상품: 카드 {breakdown.CardCount}개 · 유물 {breakdown.RelicCount}개 · 포션 {breakdown.PotionCount}개",
            $"서비스: {shop.ServiceCount}개",
            $"구매 가능: {shop.AffordableOptionCount}개",
            $"제거 가능: {(shop.CardRemovalVisible ? "예" : "아니오")}",
        };
    }

    private static (int CardCount, int RelicCount, int PotionCount, string ItemsSummary) BuildShopOptionBreakdown(IReadOnlyList<AdvisorSceneOption> options)
    {
        var cardCount = 0;
        var relicCount = 0;
        var potionCount = 0;

        foreach (var option in options)
        {
            if (option.Tags.Contains("shop-type:card", StringComparer.OrdinalIgnoreCase))
            {
                cardCount += 1;
                continue;
            }

            if (option.Tags.Contains("shop-type:relic", StringComparer.OrdinalIgnoreCase))
            {
                relicCount += 1;
                continue;
            }

            if (option.Tags.Contains("shop-type:potion", StringComparer.OrdinalIgnoreCase))
            {
                potionCount += 1;
            }
        }

        var parts = new List<string>();
        if (cardCount > 0)
        {
            parts.Add($"카드 {cardCount}");
        }

        if (relicCount > 0)
        {
            parts.Add($"유물 {relicCount}");
        }

        if (potionCount > 0)
        {
            parts.Add($"포션 {potionCount}");
        }

        return (cardCount, relicCount, potionCount, parts.Count == 0 ? "세부 분류 없음" : string.Join(" / ", parts));
    }

    private static string BuildMapHeadline(AdvisorSceneArtifact scene)
    {
        var map = scene.Map;
        if (map is null)
        {
            return $"맵 장면({scene.SceneStage})";
        }

        return $"맵 장면({(map.CurrentNodeArrowVisible ? "reachable" : scene.SceneStage)})";
    }

    private static IReadOnlyList<string> BuildMapDetailLines(AdvisorSceneArtifact scene)
    {
        var map = scene.Map;
        if (map is null)
        {
            return Array.Empty<string>();
        }

        var lines = new List<string>
        {
            $"reachable nodes: {map.ReachableNodeCount}개",
        };

        if (!string.IsNullOrWhiteSpace(map.CurrentNode))
        {
            lines.Add($"current node: {map.CurrentNode}");
        }

        if (map.ReachableNodeLabels.Count > 0)
        {
            lines.Add($"reachable labels: {string.Join(", ", map.ReachableNodeLabels)}");
        }

        return lines;
    }

    private static string FormatCombatOptions(IReadOnlyList<AdvisorSceneOption> options, AdvisorKnowledgeDisplayResolver resolver)
    {
        var grouped = options
            .Select(option => new
            {
                Option = option,
                Category = ClassifyCombatOption(option),
            })
            .GroupBy(item => item.Category)
            .OrderBy(group => GetCombatCategoryOrder(group.Key))
            .ToArray();

        var lines = new List<string>();
        foreach (var group in grouped)
        {
            var heading = GetCombatCategoryLabel(group.Key);
            if (!string.IsNullOrWhiteSpace(heading))
            {
                lines.Add(heading);
            }

            lines.AddRange(group.Select(item => FormatOptionLine(item.Option, resolver, preserveLabel: false)));
            lines.Add(string.Empty);
        }

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return JoinLines(lines);
    }

    private static string FormatOptionLine(AdvisorSceneOption option, AdvisorKnowledgeDisplayResolver resolver, bool preserveLabel)
    {
        var label = preserveLabel
            ? AdvisorDisplaySanitizer.SanitizeText(option.Label) ?? option.Label
            : resolver.ResolveDisplayText(option.Label, option.Value);
        var description = resolver.ResolveDescription(option.Description, option.Label, option.Value);
        var status = option.Enabled ? "활성" : "비활성";

        return string.IsNullOrWhiteSpace(description)
            ? $"- {label} [{status}]"
            : $"- {label} [{status}] :: {description}";
    }

    private static bool IsUtilityOption(AdvisorSceneOption option)
    {
        return option.Tags.Any(tag => tag.Contains("utility", StringComparison.OrdinalIgnoreCase) || tag.Contains("diagnostic", StringComparison.OrdinalIgnoreCase))
               || string.Equals(option.Kind, "utility", StringComparison.OrdinalIgnoreCase)
               || string.Equals(option.Label, "Ping", StringComparison.OrdinalIgnoreCase);
    }

    private static CombatOptionCategory ClassifyCombatOption(AdvisorSceneOption option)
    {
        if (IsUtilityOption(option))
        {
            return CombatOptionCategory.Utility;
        }

        var label = NormalizeToken(option.Label);
        var kind = NormalizeToken(option.Kind);

        if (kind.Contains("card", StringComparison.OrdinalIgnoreCase) || option.Tags.Any(tag => tag.Contains("hand-card", StringComparison.OrdinalIgnoreCase)))
        {
            return CombatOptionCategory.HandCard;
        }

        if (label.Contains("drawpile", StringComparison.OrdinalIgnoreCase)
            || label.Contains("discardpile", StringComparison.OrdinalIgnoreCase)
            || label.Contains("exhaustpile", StringComparison.OrdinalIgnoreCase)
            || label.Contains("playpile", StringComparison.OrdinalIgnoreCase)
            || kind.Contains("pile", StringComparison.OrdinalIgnoreCase)
            || option.Tags.Any(tag => tag.Contains("pile-button", StringComparison.OrdinalIgnoreCase)))
        {
            return CombatOptionCategory.PileButton;
        }

        if (label.Contains("endturn", StringComparison.OrdinalIgnoreCase)
            || label.Contains("end turn", StringComparison.OrdinalIgnoreCase)
            || kind.Contains("action", StringComparison.OrdinalIgnoreCase)
            || option.Tags.Any(tag => tag.Contains("combat-action", StringComparison.OrdinalIgnoreCase)))
        {
            return CombatOptionCategory.CombatAction;
        }

        return CombatOptionCategory.HandCard;
    }

    private static int GetCombatCategoryOrder(CombatOptionCategory category)
    {
        return category switch
        {
            CombatOptionCategory.HandCard => 0,
            CombatOptionCategory.PileButton => 1,
            CombatOptionCategory.CombatAction => 2,
            _ => 3,
        };
    }

    private static string GetCombatCategoryLabel(CombatOptionCategory category)
    {
        return category switch
        {
            CombatOptionCategory.HandCard => "손패 카드",
            CombatOptionCategory.PileButton => "더미 버튼",
            CombatOptionCategory.CombatAction => "전투 행동",
            _ => "기타",
        };
    }

    private static string JoinLines(IEnumerable<string> lines)
    {
        return string.Join(Environment.NewLine, lines.Where(line => line is not null));
    }

    private static string FormatBulletSection(IEnumerable<string> items)
    {
        return JoinLines(items.DefaultIfEmpty("없음").Select(item => $"- {item}"));
    }

    private static string FormatConfidence(IReadOnlyDictionary<string, double> confidence)
    {
        if (confidence.Count == 0)
        {
            return "- 없음";
        }

        return JoinLines(confidence
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => $"- {pair.Key}: {pair.Value:0.00}"));
    }

    private static string FormatInt(int? value)
    {
        return value?.ToString() ?? "?";
    }

    private static string FormatOptionalInt(int? value)
    {
        return value?.ToString() ?? "?";
    }

    private static string TranslateScene(string? sceneType)
    {
        return sceneType?.Trim().ToLowerInvariant() switch
        {
            "combat" => "전투",
            "reward" => "보상",
            "event" => "이벤트",
            "rest-site" => "휴식처",
            "shop" => "상점",
            "map" => "맵",
            _ => sceneType ?? "장면",
        };
    }

    private static string NormalizeToken(string value)
    {
        return new string(value
            .ToLowerInvariant()
            .Where(character => char.IsLetterOrDigit(character))
            .ToArray());
    }

    private enum CombatOptionCategory
    {
        HandCard,
        PileButton,
        CombatAction,
        Utility,
    }
}
