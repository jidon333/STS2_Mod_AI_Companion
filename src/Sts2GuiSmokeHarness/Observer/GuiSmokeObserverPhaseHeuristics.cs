using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

static class GuiSmokeObserverPhaseHeuristics
{
    public static bool TryGetPostRunLoadPhase(ObserverState observer, out GuiSmokePhase nextPhase)
    {
        return TryGetPostRunLoadPhase(
            observer.Summary,
            TryReadObserverMetaString(observer.StateDocument, "declaringType"),
            TryReadObserverMetaString(observer.StateDocument, "instanceType"),
            out nextPhase);
    }

    public static bool TryGetPostRunLoadPhase(ObserverSummary observer, out GuiSmokePhase nextPhase)
    {
        return TryGetPostRunLoadPhase(observer, null, null, out nextPhase);
    }

    public static bool TryGetPostRunLoadPhase(ObserverSummary observer, string? declaringType, out GuiSmokePhase nextPhase)
    {
        return TryGetPostRunLoadPhase(observer, declaringType, null, out nextPhase);
    }

    public static bool TryGetPostRunLoadPhase(ObserverSummary observer, string? declaringType, string? instanceType, out GuiSmokePhase nextPhase)
    {
        if (RootSceneTransitionObserverSignals.ShouldHoldRunLoadBoundary(observer))
        {
            nextPhase = default;
            return false;
        }

        if (CompatibilitySceneReady(observer) == false
            && !MatchesCompatibilityScreen(observer, "character-select"))
        {
            return TryGetPostRunLoadNonReadyRoomPhase(observer, out nextPhase);
        }

        if (MatchesCompatibilityScreen(observer, "character-select"))
        {
            nextPhase = GuiSmokePhase.ChooseCharacter;
            return true;
        }

        if (TryGetPostEmbarkPhase(observer, declaringType, instanceType, out nextPhase))
        {
            return true;
        }

        nextPhase = default;
        return false;
    }

    private static bool TryGetPostRunLoadNonReadyRoomPhase(ObserverSummary observer, out GuiSmokePhase nextPhase)
    {
        if (LooksLikeCombatState(observer))
        {
            nextPhase = GuiSmokePhase.HandleCombat;
            return true;
        }

        if (AutoDecisionProvider.TryBuildCanonicalNonCombatSceneState(observer, null) is { } canonicalScene
            && canonicalScene.CanonicalForegroundOwner is not NonCombatCanonicalForegroundOwner.Unknown and not NonCombatCanonicalForegroundOwner.Map
            && TryMapExplicitRunLoadRoomHandoffTarget(canonicalScene.HandoffTarget, out nextPhase))
        {
            return true;
        }

        nextPhase = default;
        return false;
    }

    private static bool TryMapExplicitRunLoadRoomHandoffTarget(NonCombatHandoffTarget handoffTarget, out GuiSmokePhase nextPhase)
    {
        nextPhase = handoffTarget switch
        {
            NonCombatHandoffTarget.HandleRewards => GuiSmokePhase.HandleRewards,
            NonCombatHandoffTarget.HandleEvent => GuiSmokePhase.HandleEvent,
            NonCombatHandoffTarget.HandleShop => GuiSmokePhase.HandleShop,
            NonCombatHandoffTarget.ChooseFirstNode => GuiSmokePhase.ChooseFirstNode,
            _ => default,
        };

        return handoffTarget is NonCombatHandoffTarget.HandleRewards
            or NonCombatHandoffTarget.HandleEvent
            or NonCombatHandoffTarget.HandleShop
            or NonCombatHandoffTarget.ChooseFirstNode;
    }

    public static bool TryGetPostCharacterSelectPhase(ObserverState observer, out GuiSmokePhase nextPhase)
    {
        if (MatchesCompatibilityScreen(observer, "character-select"))
        {
            nextPhase = GuiSmokePhase.ChooseCharacter;
            return true;
        }

        nextPhase = default;
        return false;
    }

    public static bool TryGetPostEmbarkPhase(ObserverState observer, out GuiSmokePhase nextPhase)
    {
        return TryGetPostEmbarkPhase(
            observer.Summary,
            TryReadObserverMetaString(observer.StateDocument, "declaringType"),
            TryReadObserverMetaString(observer.StateDocument, "instanceType"),
            out nextPhase);
    }

    public static bool TryGetPostEmbarkPhase(ObserverSummary observer, out GuiSmokePhase nextPhase)
    {
        return TryGetPostEmbarkPhase(observer, null, null, out nextPhase);
    }

    public static bool TryGetPostEmbarkPhase(ObserverSummary observer, string? declaringType, out GuiSmokePhase nextPhase)
    {
        return TryGetPostEmbarkPhase(observer, declaringType, null, out nextPhase);
    }

    public static bool TryGetPostEmbarkPhase(ObserverSummary observer, string? declaringType, string? instanceType, out GuiSmokePhase nextPhase)
    {
        if (TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer))
        {
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (RewardObserverSignals.IsRewardAuthorityActive(observer))
        {
            nextPhase = GuiSmokePhase.HandleRewards;
            return true;
        }

        if (LooksLikeCombatState(observer))
        {
            nextPhase = GuiSmokePhase.HandleCombat;
            return true;
        }

        if (LooksLikeShopState(observer))
        {
            nextPhase = GuiSmokePhase.HandleShop;
            return true;
        }

        if (LooksLikeMapState(observer, declaringType, instanceType)
            || LooksLikeRestSiteState(observer))
        {
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (LooksLikeEventState(observer, declaringType, instanceType))
        {
            nextPhase = GuiSmokePhase.HandleEvent;
            return true;
        }

        nextPhase = default;
        return false;
    }

    public static bool TryGetPostMapNodePhase(ObserverState observer, out GuiSmokePhase nextPhase)
    {
        return TryGetPostEmbarkPhase(observer, out nextPhase);
    }

    public static bool TryGetPostMapNodePhase(ObserverSummary observer, out GuiSmokePhase nextPhase)
    {
        return TryGetPostEmbarkPhase(observer, out nextPhase);
    }

    public static bool TryGetPostEventReleasePhase(ObserverState observer, out GuiSmokePhase nextPhase)
    {
        return TryGetPostEventReleasePhase(
            observer.Summary,
            TryReadObserverMetaString(observer.StateDocument, "declaringType"),
            TryReadObserverMetaString(observer.StateDocument, "instanceType"),
            out nextPhase);
    }

    public static bool TryGetPostEventReleasePhase(ObserverSummary observer, out GuiSmokePhase nextPhase)
    {
        return TryGetPostEventReleasePhase(observer, null, null, out nextPhase);
    }

    public static bool TryGetPostEventReleasePhase(ObserverSummary observer, string? declaringType, string? instanceType, out GuiSmokePhase nextPhase)
    {
        if (AncientEventObserverSignals.IsMapForegroundOwner(observer)
            || AncientEventObserverSignals.HasMapReleaseAuthority(observer, declaringType, instanceType))
        {
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (AncientEventObserverSignals.HasForegroundAuthority(observer))
        {
            nextPhase = default;
            return false;
        }

        if (RewardObserverSignals.IsRewardAuthorityActive(observer))
        {
            nextPhase = GuiSmokePhase.HandleRewards;
            return true;
        }

        if (LooksLikeCombatState(observer))
        {
            nextPhase = GuiSmokePhase.HandleCombat;
            return true;
        }

        if (LooksLikeShopState(observer))
        {
            nextPhase = GuiSmokePhase.HandleShop;
            return true;
        }

        if (TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer)
            || LooksLikeRestSiteState(observer)
            || LooksLikeMapState(observer, declaringType, instanceType))
        {
            nextPhase = GuiSmokePhase.ChooseFirstNode;
            return true;
        }

        if (LooksLikeEventState(observer, declaringType, instanceType))
        {
            nextPhase = GuiSmokePhase.HandleEvent;
            return true;
        }

        nextPhase = default;
        return false;
    }

    public static bool LooksLikeRewardsState(ObserverSummary observer)
    {
        var rewardState = RewardObserverSignals.TryGetState(observer);
        if (rewardState is not null)
        {
            return rewardState.ForegroundOwned;
        }

        return MatchesCompatibilityScreen(observer, "rewards")
               || string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase)
               || observer.ActionNodes.Any(static node =>
                   node.Actionable
                   && (node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
                       || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)));
    }

    public static bool LooksLikeMapState(ObserverState observer)
    {
        return LooksLikeMapState(
            observer.Summary,
            TryReadObserverMetaString(observer.StateDocument, "declaringType"),
            TryReadObserverMetaString(observer.StateDocument, "instanceType"));
    }

    public static bool LooksLikeMapState(ObserverSummary observer)
    {
        return LooksLikeMapState(observer, null, null);
    }

    public static bool LooksLikeMapState(ObserverSummary observer, string? declaringType)
    {
        return LooksLikeMapState(observer, declaringType, null);
    }

    public static bool LooksLikeMapState(ObserverSummary observer, string? declaringType, string? instanceType)
    {
        if (LooksLikeCombatState(observer))
        {
            return false;
        }

        var mapCurrentActiveScreen = observer.Meta.TryGetValue("mapCurrentActiveScreen", out var mapCurrentActiveScreenValue)
                                     ? mapCurrentActiveScreenValue
                                     : null;
        return MatchesCompatibilityScreen(observer, "map")
               || string.Equals(mapCurrentActiveScreen, "true", StringComparison.OrdinalIgnoreCase)
               || IsMapScreenType(declaringType)
               || IsMapScreenType(instanceType)
               || observer.LastEventsTail.Any(IsMapTransitionEventTail);
    }

    public static bool LooksLikeCombatState(ObserverSummary observer)
    {
        var combatScreen = MatchesCompatibilityScreen(observer, "combat");
        var combatRoomAuthority = IsCombatRoomType(TryGetMetaValue(observer, "activeScreenType"))
                                  || IsCombatRoomType(TryGetMetaValue(observer, "currentRunRoomSceneType"))
                                  || IsCombatRoomType(TryGetMetaValue(observer, "currentRunRoomType"));
        var combatPrimaryActive = string.Equals(TryGetMetaValue(observer, "combatPrimaryValue"), "true", StringComparison.OrdinalIgnoreCase);
        return (combatScreen && observer.InCombat == true)
               || (combatRoomAuthority && (observer.InCombat == true || combatPrimaryActive));
    }

    public static bool LooksLikeEventState(ObserverSummary observer, string? declaringType)
    {
        return LooksLikeEventState(observer, declaringType, null);
    }

    public static bool LooksLikeEventState(ObserverSummary observer, string? declaringType, string? instanceType)
    {
        if (AncientEventObserverSignals.IsMapForegroundOwner(observer))
        {
            return false;
        }

        if (AncientEventObserverSignals.HasForegroundAuthority(observer))
        {
            return true;
        }

        if (LooksLikeMapState(observer, declaringType, instanceType))
        {
            return false;
        }

        if (MatchesCompatibilityScreen(observer, "event")
            || string.Equals(observer.ChoiceExtractorPath, "event", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(declaringType)
            && (declaringType.Contains("EventRoom", StringComparison.OrdinalIgnoreCase)
                || declaringType.Contains(".Events.", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return observer.ActionNodes.Any(static node =>
            node.Actionable
            && (node.NodeId.StartsWith("event-option:", StringComparison.OrdinalIgnoreCase)
                || string.Equals(node.Kind, "event-option", StringComparison.OrdinalIgnoreCase)
                || node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)));
    }

    private static bool IsMapScreenType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("Map.NMapScreen", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCombatRoomType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && (typeName.Contains("NCombatRoom", StringComparison.OrdinalIgnoreCase)
                   || typeName.Contains("CombatRoom", StringComparison.OrdinalIgnoreCase));
    }

    private static string? TryGetMetaValue(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value)
            ? value
            : null;
    }

    private static bool IsMapTransitionEventTail(string? eventTail)
    {
        return !string.IsNullOrWhiteSpace(eventTail)
               && (eventTail.Contains("screen-changed: map", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("-> map", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("NMapScreen.Open", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("map-point-selected", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("\"screen\":\"map\"", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("\"currentScreen\":\"map\"", StringComparison.OrdinalIgnoreCase));
    }

    public static string? TryReadObserverMetaString(JsonDocument? stateDocument, string propertyName)
    {
        if (stateDocument is null
            || !stateDocument.RootElement.TryGetProperty("meta", out var metaElement)
            || metaElement.ValueKind != JsonValueKind.Object
            || !metaElement.TryGetProperty(propertyName, out var valueElement)
            || valueElement.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return valueElement.GetString();
    }

    private static bool LooksLikeShopState(ObserverSummary observer)
    {
        return ShopObserverSignals.IsShopAuthorityActive(observer);
    }

    private static bool LooksLikeRestSiteState(ObserverSummary observer)
    {
        return GuiSmokeNonCombatContractSupport.LooksLikeRestSiteState(observer);
    }
}
