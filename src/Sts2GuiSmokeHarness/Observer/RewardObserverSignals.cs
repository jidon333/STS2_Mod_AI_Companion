using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

static class RewardObserverSignals
{
    public static RewardScreenState? TryGetState(ObserverSummary observer)
    {
        var explicitDetected = TryGetMetaBool(observer, "rewardScreenDetected");
        var explicitVisible = TryGetMetaBool(observer, "rewardScreenVisible");
        var explicitForegroundOwned = TryGetMetaBool(observer, "rewardForegroundOwned");
        var explicitTeardown = TryGetMetaBool(observer, "rewardTeardownInProgress");
        var rewardIsCurrentActiveScreen = TryGetMetaBool(observer, "rewardIsCurrentActiveScreen") == true;
        var rewardIsTopOverlay = TryGetMetaBool(observer, "rewardIsTopOverlay") == true;
        var mapIsCurrentActiveScreen = TryGetMetaBool(observer, "mapCurrentActiveScreen") == true;
        var terminalRunBoundary = TryGetMetaBool(observer, "terminalRunBoundary") == true;
        var screenDetected = explicitDetected
                             ?? rewardIsCurrentActiveScreen
                             || rewardIsTopOverlay
                             || MatchesControlFlowScreen(observer, "rewards")
                             || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase)
                             || observer.Choices.Any(static choice =>
                                 choice.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                                 || choice.SemanticHints.Any(static hint => hint.Contains("reward", StringComparison.OrdinalIgnoreCase)))
                             || observer.ActionNodes.Any(static node =>
                                 node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
                                 || node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase));
        if (!screenDetected && !terminalRunBoundary)
        {
            return null;
        }

        var proceedVisible = TryGetMetaBool(observer, "rewardProceedVisible") == true;
        var proceedEnabled = TryGetMetaBool(observer, "rewardProceedEnabled") == true;
        var visibleButtonCount = TryGetMetaInt(observer, "rewardVisibleButtonCount") ?? 0;
        var enabledButtonCount = TryGetMetaInt(observer, "rewardEnabledButtonCount") ?? 0;
        var hasOpenPotionSlots = TryGetMetaBool(observer, "hasOpenPotionSlots");
        var explicitRewardProgressionPresent = observer.Choices.Any(static choice =>
                                                 choice.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
                                                 || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase))
                                             || observer.ActionNodes.Any(static node =>
                                                 node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
                                                 || node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase));
        var explicitMapProgressionPresent = observer.Choices.Any(MapNodeSourceSupport.IsExplicitMapPointChoice)
                                            || observer.ActionNodes.Any(static node =>
                                                node.Actionable
                                                && MapNodeSourceSupport.IsExplicitMapPointNode(node));
        var actionableRewardAffordance = proceedEnabled || enabledButtonCount > 0;
        var staleTopOverlayTeardown = rewardIsTopOverlay
                                      && !rewardIsCurrentActiveScreen
                                      && mapIsCurrentActiveScreen
                                      && explicitMapProgressionPresent
                                      && !proceedEnabled
                                      && !explicitRewardProgressionPresent;
        var screenVisible = explicitVisible
                            ?? rewardIsCurrentActiveScreen
                            || rewardIsTopOverlay
                            || proceedVisible
                            || visibleButtonCount > 0
                            || MatchesControlFlowScreen(observer, "rewards")
                            || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase)
                            || explicitRewardProgressionPresent;
        var foregroundOwned = explicitForegroundOwned
                              ?? (!terminalRunBoundary
                                  && !staleTopOverlayTeardown
                                  && (rewardIsCurrentActiveScreen
                                      || proceedEnabled
                                      || enabledButtonCount > 0
                                      || (rewardIsTopOverlay && actionableRewardAffordance && !mapIsCurrentActiveScreen)
                                      || (explicitRewardProgressionPresent
                                          && !mapIsCurrentActiveScreen
                                          && !staleTopOverlayTeardown)));
        if (staleTopOverlayTeardown)
        {
            foregroundOwned = false;
        }

        var teardownInProgress = explicitTeardown
                                 ?? (screenVisible
                                     && !terminalRunBoundary
                                     && (staleTopOverlayTeardown
                                         || (!foregroundOwned
                                             && (mapIsCurrentActiveScreen
                                                 || MatchesControlFlowScreen(observer, "map")))));
        if (staleTopOverlayTeardown)
        {
            teardownInProgress = true;
        }

        return new RewardScreenState(
            ScreenDetected: screenDetected == true,
            ScreenVisible: screenVisible == true,
            ForegroundOwned: foregroundOwned == true,
            TeardownInProgress: teardownInProgress == true,
            RewardIsCurrentActiveScreen: rewardIsCurrentActiveScreen,
            RewardIsTopOverlay: rewardIsTopOverlay,
            MapIsCurrentActiveScreen: mapIsCurrentActiveScreen,
            ActiveScreenType: TryGetMetaValue(observer, "activeScreenType"),
            ProceedVisible: proceedVisible,
            ProceedEnabled: proceedEnabled,
            VisibleButtonCount: visibleButtonCount,
            EnabledButtonCount: enabledButtonCount,
            HasOpenPotionSlots: hasOpenPotionSlots,
            TerminalRunBoundary: terminalRunBoundary,
            GameOverScreenDetected: TryGetMetaBool(observer, "gameOverScreenDetected") == true,
            UnlockScreenDetected: TryGetMetaBool(observer, "unlockScreenDetected") == true,
            TimelineUnlockDetected: TryGetMetaBool(observer, "timelineUnlockDetected") == true,
            MainMenuReturnDetected: TryGetMetaBool(observer, "mainMenuReturnDetected") == true,
            RootType: TryGetMetaValue(observer, "rewardScreenRootType"));
    }

    public static bool IsRewardAuthorityActive(ObserverSummary observer)
        => TryGetState(observer) is { ForegroundOwned: true };

    public static bool IsTerminalRunBoundary(ObserverSummary observer)
        => TryGetState(observer) is { TerminalRunBoundary: true };

    private static string? TryGetMetaValue(ObserverSummary observer, string key)
        => observer.Meta.TryGetValue(key, out var value) ? value : null;

    private static bool? TryGetMetaBool(ObserverSummary observer, string key)
        => observer.Meta.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed)
            ? parsed
            : null;

    private static int? TryGetMetaInt(ObserverSummary observer, string key)
        => observer.Meta.TryGetValue(key, out var value) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
}
