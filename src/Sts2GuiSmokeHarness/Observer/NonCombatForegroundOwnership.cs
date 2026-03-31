using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;

static class NonCombatForegroundOwnership
{
    public static NonCombatForegroundOwner Resolve(ObserverState observer)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(observer.Summary))
        {
            return NonCombatForegroundOwner.Unknown;
        }

        if (GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary))
        {
            return NonCombatForegroundOwner.Combat;
        }

        var rewardScene = AutoDecisionProvider.BuildRewardSceneState(observer, null);
        if (rewardScene.RewardForegroundOwned)
        {
            return NonCombatForegroundOwner.Reward;
        }

        if (AutoDecisionProvider.BuildShopSceneState(observer) is { ReleaseStage: NonCombatReleaseStage.Active })
        {
            return NonCombatForegroundOwner.Shop;
        }

        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, null);
        if (eventScene.EventForegroundOwned
            && eventScene.ExplicitAction is EventExplicitActionKind.AncientDialogue
                or EventExplicitActionKind.AncientCompletion
                or EventExplicitActionKind.AncientOption
                or EventExplicitActionKind.AncientOptionContractMismatch)
        {
            return NonCombatForegroundOwner.Event;
        }

        if (HasExplicitMapForegroundAuthority(observer))
        {
            return NonCombatForegroundOwner.Map;
        }

        if (AutoDecisionProvider.BuildRestSiteSceneState(observer) is { ReleaseStage: NonCombatReleaseStage.Active })
        {
            return NonCombatForegroundOwner.RestSite;
        }

        if (eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.Active)
        {
            return NonCombatForegroundOwner.Event;
        }

        return NonCombatForegroundOwner.Unknown;
    }

    public static NonCombatForegroundOwner Resolve(ObserverSummary observer)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(observer))
        {
            return NonCombatForegroundOwner.Unknown;
        }

        if (GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer))
        {
            return NonCombatForegroundOwner.Combat;
        }

        var observerState = new ObserverState(observer, null, null, null);
        var rewardScene = AutoDecisionProvider.BuildRewardSceneState(observerState, null);
        if (rewardScene.RewardForegroundOwned)
        {
            return NonCombatForegroundOwner.Reward;
        }

        if (AutoDecisionProvider.BuildShopSceneState(observerState) is { ReleaseStage: NonCombatReleaseStage.Active })
        {
            return NonCombatForegroundOwner.Shop;
        }

        var eventScene = AutoDecisionProvider.BuildEventSceneState(observerState, null);
        if (eventScene.EventForegroundOwned
            && eventScene.ExplicitAction is EventExplicitActionKind.AncientDialogue
                or EventExplicitActionKind.AncientCompletion
                or EventExplicitActionKind.AncientOption
                or EventExplicitActionKind.AncientOptionContractMismatch)
        {
            return NonCombatForegroundOwner.Event;
        }

        if (HasExplicitMapForegroundAuthority(observer))
        {
            return NonCombatForegroundOwner.Map;
        }

        if (AutoDecisionProvider.BuildRestSiteSceneState(observerState) is { ReleaseStage: NonCombatReleaseStage.Active })
        {
            return NonCombatForegroundOwner.RestSite;
        }

        if (eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.Active)
        {
            return NonCombatForegroundOwner.Event;
        }

        return NonCombatForegroundOwner.Unknown;
    }

    public static bool HasExplicitMapForegroundAuthority(ObserverState observer)
    {
        if (HasExplicitMapForegroundAuthority(observer.Summary))
        {
            return true;
        }

        return IsMapScreenTypeName(GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "declaringType"))
               || IsMapScreenTypeName(GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "instanceType"));
    }

    public static bool HasExplicitMapForegroundAuthority(ObserverSummary observer)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(observer)
            || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer))
        {
            return false;
        }

        return AncientEventObserverSignals.IsMapForegroundOwner(observer)
               || TryGetMetaBool(observer, "mapCurrentActiveScreen") == true
               || IsMapScreenTypeName(TryGetMetaValue(observer, "activeScreenType"));
    }

    public static bool HasExplicitRestSiteForegroundAuthority(ObserverSummary observer)
    {
        if (HasExplicitMapForegroundAuthority(observer))
        {
            return false;
        }

        return string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
               && (RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(observer)
                   || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer)
                   || GuiSmokeNonCombatContractSupport.LooksLikeRestSiteProceedState(observer));
    }

    public static bool HasExplicitEventForegroundAuthority(ObserverSummary observer)
    {
        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, null);
        return eventScene.EventForegroundOwned
               && eventScene.ReleaseStage == EventReleaseStage.Active
               && eventScene.HasExplicitProgression;
    }

    private static bool? TryGetMetaBool(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value)
               && bool.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    private static string? TryGetMetaValue(ObserverSummary observer, string key)
        => observer.Meta.TryGetValue(key, out var value) ? value : null;

    private static bool IsMapScreenTypeName(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase);
    }
}
