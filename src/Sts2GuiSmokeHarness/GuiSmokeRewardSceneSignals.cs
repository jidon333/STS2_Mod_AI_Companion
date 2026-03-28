internal static class GuiSmokeRewardSceneSignals
{
    internal static bool LooksLikeRewardChoiceState(ObserverState observer)
    {
        return LooksLikeRewardChoiceState(observer.Summary);
    }

    internal static bool LooksLikeRewardChoiceState(ObserverSummary observer)
    {
        return AutoDecisionProvider.BuildRewardSceneState(observer, null).RewardChoiceVisible;
    }

    internal static bool LooksLikeColorlessCardChoiceState(ObserverState observer)
    {
        return LooksLikeColorlessCardChoiceState(observer.Summary);
    }

    internal static bool LooksLikeColorlessCardChoiceState(ObserverSummary observer)
    {
        return AutoDecisionProvider.BuildRewardSceneState(observer, null).ColorlessChoiceVisible;
    }

    internal static bool HasRewardChoiceAuthority(ObserverSummary observer)
    {
        var rewardScene = AutoDecisionProvider.BuildRewardSceneState(observer, null);
        return rewardScene.RewardForegroundOwned
               || rewardScene.LayerState.StaleRewardChoicePresent
               || rewardScene.ScreenState?.ScreenVisible == true;
    }

    internal static bool HasStrongRewardScreenAuthority(ObserverState observer)
    {
        var rewardScene = AutoDecisionProvider.BuildRewardSceneState(observer, null);
        return rewardScene.ScreenState?.ForegroundOwned == true
               || rewardScene.ScreenState?.ScreenVisible == true
               || IsRewardsScreenType(GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "declaringType"))
               || IsRewardsScreenType(GuiSmokeObserverPhaseHeuristics.TryReadObserverMetaString(observer.StateDocument, "instanceType"));
    }

    internal static bool HasStrongRewardScreenAuthority(ObserverSummary observer)
    {
        var rewardScene = AutoDecisionProvider.BuildRewardSceneState(observer, null);
        return rewardScene.ScreenState?.ForegroundOwned == true
               || rewardScene.ScreenState?.ScreenVisible == true
               || GuiSmokeObserverPhaseHeuristics.LooksLikeRewardsState(observer);
    }

    private static bool IsRewardsScreenType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("RewardsScreen", StringComparison.OrdinalIgnoreCase);
    }
}
