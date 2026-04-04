using System.Text;

namespace Sts2AiCompanion.AdvisorSceneModel;

public static class AdvisorSceneSummaryFormatter
{
    public static string BuildSummary(AdvisorSceneArtifact artifact)
    {
        var builder = new StringBuilder();
        switch (artifact.SceneType)
        {
            case "combat":
                AppendCombatSummary(builder, artifact);
                break;
            case "reward":
                AppendRewardSummary(builder, artifact);
                break;
            case "event":
                AppendEventSummary(builder, artifact);
                break;
            case "rest-site":
                AppendRestSiteSummary(builder, artifact);
                break;
            case "shop":
                AppendShopSummary(builder, artifact);
                break;
            case "map":
                AppendMapSummary(builder, artifact);
                break;
            default:
                builder.Append("현재 장면 핵심 정보가 아직 정규화되지 않았다.");
                break;
        }

        if (artifact.MissingFacts.Count > 0)
        {
            builder.Append(" 누락 정보: ");
            builder.Append(string.Join(", ", artifact.MissingFacts));
            builder.Append('.');
        }

        return builder.ToString().Trim();
    }

    private static void AppendCombatSummary(StringBuilder builder, AdvisorSceneArtifact artifact)
    {
        var combat = artifact.Combat!;
        builder.Append($"전투 장면({combat.LifecycleStage}). HP {FormatPair(artifact.PlayerContext.CurrentHp, artifact.PlayerContext.MaxHp)}, 에너지 {FormatScalar(artifact.PlayerContext.Energy)}, 손패 {combat.HandCount}장");
        if (!string.IsNullOrWhiteSpace(combat.HandSummary))
        {
            builder.Append($", 손패 요약 {combat.HandSummary}");
        }

        builder.Append($". 타겟 가능 적 {FormatScalar(combat.TargetableEnemyCount)}, 명시적 피격 적 {FormatScalar(combat.HittableEnemyCount)}");
        if (!string.IsNullOrWhiteSpace(combat.EnemyIntentSummary))
        {
            builder.Append($", 적 의도 {combat.EnemyIntentSummary}");
        }
        else
        {
            builder.Append(", 적 의도 요약은 아직 정규화되지 않았다");
        }

        builder.Append('.');
    }

    private static void AppendRewardSummary(StringBuilder builder, AdvisorSceneArtifact artifact)
    {
        var reward = artifact.Reward!;
        builder.Append($"보상 장면({artifact.SceneStage}). 보상 항목 {reward.RewardEntryCount}개");
        if (reward.ExplicitProceedVisible)
        {
            builder.Append(", proceed 표면이 보인다");
        }

        if (reward.CardProgressionSurfacePresent)
        {
            builder.Append(", 카드 선택 하위 표면이 있다");
        }

        builder.Append('.');
    }

    private static void AppendEventSummary(StringBuilder builder, AdvisorSceneArtifact artifact)
    {
        var scene = artifact.Event!;
        builder.Append($"이벤트 장면({artifact.SceneStage}). 표시 옵션 {scene.OptionCount}개");
        if (!string.IsNullOrWhiteSpace(scene.EventIdentity))
        {
            builder.Append($", 식별된 이벤트 {scene.EventIdentity}");
        }
        else
        {
            builder.Append(", 이벤트 식별자는 아직 고정되지 않았다");
        }

        if (scene.RewardSubstateActive)
        {
            builder.Append(", 현재는 reward 하위 상태가 열려 있다");
        }

        builder.Append('.');
    }

    private static void AppendRestSiteSummary(StringBuilder builder, AdvisorSceneArtifact artifact)
    {
        var scene = artifact.RestSite!;
        builder.Append($"휴식처 장면({artifact.SceneStage}). HP {FormatPair(artifact.PlayerContext.CurrentHp, artifact.PlayerContext.MaxHp)}, 표시 선택지 {scene.ActionCount}개");
        if (scene.SmithUpgradeActive)
        {
            builder.Append(", smith 표면이 열려 있다");
        }
        else if (scene.ExplicitChoiceVisible)
        {
            builder.Append(", rest/smith 명시적 선택지가 보인다");
        }

        builder.Append('.');
    }

    private static void AppendShopSummary(StringBuilder builder, AdvisorSceneArtifact artifact)
    {
        var scene = artifact.Shop!;
        builder.Append($"상점 장면({artifact.SceneStage}). 골드 {FormatScalar(artifact.PlayerContext.Gold)}, 상품 {scene.ItemCount}개, 서비스 {scene.ServiceCount}개");
        builder.Append($", 구매 가능 항목 {scene.AffordableOptionCount}개");
        if (scene.CardRemovalVisible)
        {
            builder.Append(", 카드 제거 서비스가 보인다");
        }

        builder.Append('.');
    }

    private static void AppendMapSummary(StringBuilder builder, AdvisorSceneArtifact artifact)
    {
        var scene = artifact.Map!;
        builder.Append($"지도 장면({artifact.SceneStage}). 도달 가능 노드 {scene.ReachableNodeCount}개");
        if (scene.ReachableNodeLabels.Count > 0)
        {
            builder.Append($", 노드 {string.Join(", ", scene.ReachableNodeLabels)}");
        }

        builder.Append('.');
    }

    private static string FormatPair(int? current, int? max)
    {
        return current is null && max is null
            ? "unknown"
            : $"{current?.ToString() ?? "?"}/{max?.ToString() ?? "?"}";
    }

    private static string FormatScalar(int? value)
    {
        return value?.ToString() ?? "unknown";
    }
}
