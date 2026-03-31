using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

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

        return AutoDecisionProvider.BuildPostNodeHandoffState(observer, null).Owner switch
        {
            NonCombatCanonicalForegroundOwner.Reward => NonCombatForegroundOwner.Reward,
            NonCombatCanonicalForegroundOwner.Shop => NonCombatForegroundOwner.Shop,
            NonCombatCanonicalForegroundOwner.RestSite => NonCombatForegroundOwner.RestSite,
            NonCombatCanonicalForegroundOwner.Map => NonCombatForegroundOwner.Map,
            NonCombatCanonicalForegroundOwner.Event => NonCombatForegroundOwner.Event,
            _ => NonCombatForegroundOwner.Unknown,
        };
    }

    public static NonCombatForegroundOwner Resolve(ObserverSummary observer)
    {
        return Resolve(new ObserverState(observer, null, null, null));
    }

    public static bool HasExplicitMapForegroundAuthority(ObserverState observer)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(observer.Summary)
            || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary))
        {
            return false;
        }

        if (TryGetMetaBool(observer.Summary, "mapCurrentActiveScreen") == true
            || IsMapScreenTypeName(TryGetMetaValue(observer.Summary, "activeScreenType")))
        {
            return true;
        }

        if (HasExplicitMapPointSurface(observer)
            && MatchesControlFlowScreen(observer, "map"))
        {
            return true;
        }

        if (IsMapScreenTypeName(GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "declaringType"))
            || IsMapScreenTypeName(GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "instanceType")))
        {
            return true;
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(observer, null, null);
        return mapOverlayState.ForegroundVisible
               && (mapOverlayState.ExportedReachableNodeCandidatePresent
                   || mapOverlayState.MapBackNavigationAvailable);
    }

    public static bool HasExplicitMapForegroundAuthority(ObserverSummary observer)
    {
        return HasExplicitMapForegroundAuthority(new ObserverState(observer, null, null, null));
    }

    public static bool HasAuthoritativeMapForegroundScreen(ObserverState observer)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(observer.Summary)
            || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary))
        {
            return false;
        }

        return TryGetMetaBool(observer.Summary, "mapCurrentActiveScreen") == true
               || IsMapScreenTypeName(TryGetMetaValue(observer.Summary, "activeScreenType"))
               || IsMapScreenTypeName(GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "declaringType"))
               || IsMapScreenTypeName(GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "instanceType"));
    }

    public static bool HasAuthoritativeMapForegroundScreen(ObserverSummary observer)
    {
        return HasAuthoritativeMapForegroundScreen(new ObserverState(observer, null, null, null));
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
        var handoffState = AutoDecisionProvider.BuildPostNodeHandoffState(observer, null);
        return handoffState.IsEventOwner
               && handoffState.ReleaseStage == NonCombatReleaseStage.Active
               && handoffState.HasExplicitSurface;
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

    private static bool HasExplicitMapPointSurface(ObserverState observer)
    {
        return observer.Choices.Any(MapNodeSourceSupport.IsExplicitMapPointChoice)
               || observer.ActionNodes.Any(static node => node.Actionable && MapNodeSourceSupport.IsExplicitMapPointNode(node));
    }
}
