using System.Drawing;
using System.Globalization;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

sealed partial class AutoDecisionProvider
{
    internal static RewardSceneState BuildRewardSceneState(
        ObserverState observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        return BuildRewardSceneState(observer.Summary, windowBounds, history, screenshotPath);
    }

    internal static RewardSceneState BuildRewardSceneState(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        var strongerRoomForegroundAuthority = NonCombatForegroundOwnership.HasExplicitRestSiteForegroundAuthority(observer)
                                             || ShopObserverSignals.IsShopAuthorityActive(observer)
                                             || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer);
        var rewardState = RewardObserverSignals.TryGetState(observer);
        var mapContextVisible = rewardState?.MapIsCurrentActiveScreen == true
                                || GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer)
                                || MatchesControlFlowScreen(observer, "map");
        var rewardBackNavigationAvailable = HasOverlayChoiceState(observer)
                                            || observer.ActionNodes.Any(static node => node.Actionable && IsBackNode(node));
        var activeRewardChoices = observer.Choices.Where(choice => IsCurrentRewardProgressionChoice(choice, windowBounds)).ToArray();
        var activeRewardNodes = observer.ActionNodes.Where(node => IsCurrentRewardProgressionNode(node, windowBounds)).ToArray();
        var claimableRewardChoices = activeRewardChoices
            .Where(choice => IsClaimableRewardProgressionChoice(choice, rewardState))
            .ToArray();
        var claimableRewardNodes = activeRewardNodes
            .Where(node => IsClaimableRewardProgressionNode(node, rewardState, activeRewardChoices))
            .ToArray();
        var staleRewardChoices = observer.Choices.Where(choice => IsStaleRewardProgressionChoice(choice, windowBounds)).ToArray();
        var staleRewardNodes = observer.ActionNodes.Where(node => IsStaleRewardProgressionNode(node, windowBounds)).ToArray();
        var explicitRewardChoicesPresent = activeRewardChoices.Length > 0 || activeRewardNodes.Length > 0;
        var rewardContextVisible = rewardState?.ScreenVisible == true
                                   || RewardObserverSignals.IsRewardAuthorityActive(observer)
                                   || GuiSmokeObserverPhaseHeuristics.LooksLikeRewardsState(observer)
                                   || MatchesControlFlowScreen(observer, "rewards")
                                   || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase);
        var intrinsicRewardCardAuthority = observer.Choices.Any(IsRewardCardChoice);
        var fallbackRewardChoiceAuthority = !strongerRoomForegroundAuthority
                                            && !CardSelectionObserverSignals.IsNonRewardCountConfirmFamily(observer)
                                            && !GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer)
                                            && (intrinsicRewardCardAuthority
                                                || (rewardContextVisible
                                                    && observer.Choices.Any(IsInspectPreviewChoice)
                                                    && observer.Choices.Any(static choice => IsSkipOrProceedLabel(choice.Label))));
        var rewardScreenHint = rewardContextVisible || fallbackRewardChoiceAuthority;
        var rewardForegroundOwned = !strongerRoomForegroundAuthority
                                    && (rewardState?.ForegroundOwned == true
                                        || (rewardScreenHint
                                            && rewardState?.MapIsCurrentActiveScreen != true
                                            && (explicitRewardChoicesPresent || fallbackRewardChoiceAuthority)));
        var staleRewardChoicePresent = staleRewardChoices.Length > 0 || staleRewardNodes.Length > 0;
        var offWindowBoundsReused = staleRewardChoices.Any(choice => IsOffWindowBounds(choice.ScreenBounds, windowBounds))
                                  || staleRewardNodes.Any(node => IsOffWindowBounds(node.ScreenBounds, windowBounds));
        var layerState = new RewardMapLayerState(
            RewardPanelVisible: rewardForegroundOwned,
            MapContextVisible: mapContextVisible,
            RewardBackNavigationAvailable: rewardBackNavigationAvailable,
            StaleRewardChoicePresent: staleRewardChoicePresent,
            StaleRewardBoundsPresent: staleRewardChoicePresent && (staleRewardChoices.Length > 0 || staleRewardNodes.Length > 0),
            OffWindowBoundsReused: offWindowBoundsReused,
            RewardForegroundOwned: rewardForegroundOwned,
            RewardTeardownInProgress: rewardState?.TeardownInProgress == true,
            RewardIsCurrentActiveScreen: rewardState?.RewardIsCurrentActiveScreen == true,
            MapCurrentActiveScreen: rewardState?.MapIsCurrentActiveScreen == true,
            TerminalRunBoundary: rewardState?.TerminalRunBoundary == true);

        var rewardChoiceVisible = !strongerRoomForegroundAuthority
                                  && !CardSelectionObserverSignals.IsNonRewardCountConfirmFamily(observer)
                                  && !GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer)
                                  && rewardScreenHint
                                  && (observer.Choices.Count(IsRewardCardChoice) > 0
                                      || (observer.Choices.Count(IsInspectPreviewChoice) > 0
                                          && observer.Choices.Any(static choice => IsSkipOrProceedLabel(choice.Label))));
        var colorlessChoiceVisible = !strongerRoomForegroundAuthority
                                     && !CardSelectionObserverSignals.IsNonRewardCountConfirmFamily(observer)
                                     && !GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer)
                                     && rewardScreenHint
                                     && observer.Choices.Any(IsRewardCardChoice)
                                     && observer.Choices.Any(IsInspectPreviewChoice);
        var explicitProceedVisible = rewardState?.ProceedVisible == true
                                     || rewardState?.ProceedEnabled == true
                                     || (!strongerRoomForegroundAuthority
                                         && (activeRewardChoices.Any(choice => IsSkipOrProceedLabel(choice.Label))
                                             || activeRewardNodes.Any(IsProceedNode)));
        var claimableRewardPresent = (!strongerRoomForegroundAuthority
                                      && (claimableRewardChoices.Length > 0
                                          || claimableRewardNodes.Length > 0))
                                     || (!strongerRoomForegroundAuthority
                                         && !string.IsNullOrWhiteSpace(screenshotPath)
                                         && HasScreenshotClaimableRewardEvidenceInScreenshot(observer, screenshotPath));
        var suppressSameSkipReissue = rewardForegroundOwned
                                      && explicitProceedVisible
                                      && !claimableRewardPresent
                                      && HasRecentRewardSkipReleaseIntent(history);
        var canonicalOwner = rewardForegroundOwned
            ? NonCombatForegroundOwner.Reward
            : (!strongerRoomForegroundAuthority
                && (layerState.RewardTeardownInProgress || layerState.MapCurrentActiveScreen)
                    ? NonCombatForegroundOwner.Map
                    : NonCombatForegroundOwner.Unknown);
        var releaseStage = canonicalOwner == NonCombatForegroundOwner.Reward
            ? (suppressSameSkipReissue ? RewardReleaseStage.ReleasePending : RewardReleaseStage.Active)
            : ((!strongerRoomForegroundAuthority
                && (layerState.RewardTeardownInProgress || layerState.MapCurrentActiveScreen))
                ? RewardReleaseStage.Released
                : RewardReleaseStage.None);
        var explicitAction = releaseStage != RewardReleaseStage.Active
            ? RewardExplicitActionKind.None
            : colorlessChoiceVisible
                ? RewardExplicitActionKind.ColorlessChoice
                : rewardChoiceVisible || claimableRewardPresent
                    ? (rewardChoiceVisible ? RewardExplicitActionKind.CardChoice : RewardExplicitActionKind.Claim)
                    : rewardBackNavigationAvailable && mapContextVisible && staleRewardChoicePresent && !layerState.MapCurrentActiveScreen
                        ? RewardExplicitActionKind.Back
                        : explicitProceedVisible
                            ? RewardExplicitActionKind.SkipProceed
                            : RewardExplicitActionKind.None;

        return new RewardSceneState(
            layerState,
            rewardState,
            canonicalOwner,
            releaseStage,
            explicitAction,
            rewardChoiceVisible,
            colorlessChoiceVisible,
            claimableRewardPresent,
            explicitProceedVisible,
            suppressSameSkipReissue);
    }

    internal static EventSceneState BuildEventSceneState(
        ObserverState observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        return BuildEventSceneState(
            observer.Summary,
            windowBounds,
            history,
            screenshotPath,
            NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer));
    }

    internal static EventSceneState BuildEventSceneState(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        return BuildEventSceneState(
            observer,
            windowBounds,
            history,
            screenshotPath,
            NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer));
    }

    private static EventSceneState BuildEventSceneState(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history,
        string? screenshotPath,
        bool mapExplicitOwner)
    {
        var rewardScene = BuildRewardSceneState(observer, windowBounds, history, screenshotPath);
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(observer, windowBounds, screenshotPath);
        var ancientDialogueActive = AncientEventObserverSignals.IsDialogueActive(observer);
        var ancientCompletionActive = AncientEventObserverSignals.HasExplicitCompletionAction(observer);
        var ancientOptionActive = AncientEventObserverSignals.HasExplicitOptionSelection(observer);
        var eventChoiceAuthority = EventProceedObserverSignals.HasEventChoiceAuthority(observer);
        var genericEventProgressVisible = eventChoiceAuthority
                                          && !rewardScene.RewardForegroundOwned
                                          && HasRawEventProgressionSurface(observer, windowBounds);
        var explicitProceedVisible = eventChoiceAuthority
                                     && !mapExplicitOwner
                                     && !rewardScene.RewardForegroundOwned
                                     && EventProceedObserverSignals.HasExplicitEventProceedSignal(observer, windowBounds);
        var activeEventChoiceVisible = eventChoiceAuthority
                                       && !mapExplicitOwner
                                       && !rewardScene.RewardForegroundOwned
                                       && HasRawExplicitEventChoiceVisible(observer, windowBounds);
        var forceProgressionAfterCardSelection = HasRecentCardSelectionSubtypeAftermath(history ?? Array.Empty<GuiSmokeHistoryEntry>())
                                                && (explicitProceedVisible || activeEventChoiceVisible || genericEventProgressVisible);
        var rewardSubstateActive = rewardScene.RewardForegroundOwned || rewardScene.ReleaseStage == RewardReleaseStage.ReleasePending;
        var mapContextVisible = mapOverlayState.ForegroundVisible
                                || MatchesControlFlowScreen(observer, "map")
                                || rewardScene.LayerState.MapContextVisible
                                || mapExplicitOwner;
        var hasExplicitProgression = ancientDialogueActive
                                     || ancientCompletionActive
                                     || ancientOptionActive
                                     || explicitProceedVisible
                                     || activeEventChoiceVisible
                                     || genericEventProgressVisible
                                     || forceProgressionAfterCardSelection;
        var strongForegroundChoice = ancientDialogueActive
                                     || ancientCompletionActive
                                     || ancientOptionActive
                                     || explicitProceedVisible
                                     || activeEventChoiceVisible
                                     || forceProgressionAfterCardSelection;
        var suppressSameProceedReissue = !rewardSubstateActive
                                         && (ancientCompletionActive || explicitProceedVisible)
                                         && HasRecentEventReleaseIntent(history);
        var canonicalOwner = rewardSubstateActive
            ? NonCombatForegroundOwner.Reward
            : mapExplicitOwner
                ? NonCombatForegroundOwner.Map
                : mapOverlayState.ForegroundVisible && !strongForegroundChoice
                    ? NonCombatForegroundOwner.Map
                    : ancientDialogueActive || ancientCompletionActive || ancientOptionActive || (eventChoiceAuthority && hasExplicitProgression)
                        ? NonCombatForegroundOwner.Event
                        : NonCombatForegroundOwner.Unknown;
        var releaseStage = rewardSubstateActive
            ? EventReleaseStage.Released
            : canonicalOwner == NonCombatForegroundOwner.Event
                ? suppressSameProceedReissue
                    ? EventReleaseStage.ReleasePending
                    : EventReleaseStage.Active
                : HasRecentEventReleaseIntent(history) && (mapExplicitOwner || mapOverlayState.ForegroundVisible)
                    ? EventReleaseStage.Released
                    : EventReleaseStage.None;
        var explicitAction = rewardSubstateActive
            ? EventExplicitActionKind.RewardSubstate
            : ancientDialogueActive
                ? EventExplicitActionKind.AncientDialogue
                : ancientCompletionActive
                    ? EventExplicitActionKind.AncientCompletion
                    : ancientOptionActive
                        ? EventExplicitActionKind.AncientOption
                        : explicitProceedVisible
                            ? EventExplicitActionKind.Proceed
                            : activeEventChoiceVisible || forceProgressionAfterCardSelection
                                ? EventExplicitActionKind.EventChoice
                                : EventExplicitActionKind.None;

        return new EventSceneState(
            canonicalOwner,
            releaseStage,
            explicitAction,
            rewardScene,
            mapOverlayState,
            mapContextVisible,
            rewardSubstateActive,
            hasExplicitProgression,
            strongForegroundChoice,
            forceProgressionAfterCardSelection,
            explicitProceedVisible,
            suppressSameProceedReissue);
    }

    internal static ShopSceneState? BuildShopSceneState(
        ObserverState observer,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null)
    {
        var state = ShopObserverSignals.TryGetState(observer.Summary);
        return state is null
            ? null
            : new ShopSceneState(state, ShopObserverSignals.HasRecentPurchase(history ?? Array.Empty<GuiSmokeHistoryEntry>()));
    }

    internal static ShopSceneState? BuildShopSceneState(
        ObserverSummary observer,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null)
    {
        return BuildShopSceneState(new ObserverState(observer, null, null, null), history);
    }

    internal static RestSiteSceneState? BuildRestSiteSceneState(ObserverState observer)
    {
        var explicitScreenAuthority = MatchesControlFlowScreen(observer, "rest-site")
                                      || string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase);
        var smithUpgradeActive = RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer.Summary);
        var hasAuthoritativeChoiceMetadata = RestSiteChoiceSupport.HasAuthoritativeMetadata(observer.Summary);
        var explicitChoiceVisible = hasAuthoritativeChoiceMetadata
                                    || (explicitScreenAuthority
                                        && RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(observer.Summary));
        var proceedVisible = explicitScreenAuthority
                             && GuiSmokeNonCombatContractSupport.LooksLikeRestSiteProceedState(observer.Summary);
        if (!smithUpgradeActive && !explicitChoiceVisible && !proceedVisible)
        {
            return null;
        }

        var mapContextVisible = MatchesControlFlowScreen(observer, "map")
                                || NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer.Summary);
        return new RestSiteSceneState(
            explicitChoiceVisible,
            smithUpgradeActive,
            RestSiteObserverSignals.HasSmithConfirmVisible(observer.Summary),
            proceedVisible,
            mapContextVisible);
    }

    internal static RestSiteSceneState? BuildRestSiteSceneState(ObserverSummary observer)
    {
        return BuildRestSiteSceneState(new ObserverState(observer, null, null, null));
    }

    internal static TreasureSceneState? BuildTreasureSceneState(ObserverState observer)
    {
        var state = TreasureRoomObserverSignals.TryGetState(observer.Summary);
        return state is null ? null : new TreasureSceneState(state);
    }

    internal static TreasureSceneState? BuildTreasureSceneState(ObserverSummary observer)
    {
        return BuildTreasureSceneState(new ObserverState(observer, null, null, null));
    }

    internal static ICanonicalNonCombatSceneState? TryBuildCanonicalNonCombatSceneState(
        ObserverState observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(observer.Summary)
            || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary))
        {
            return null;
        }

        var rewardScene = BuildRewardSceneState(observer, windowBounds, history, screenshotPath);
        if (rewardScene.RewardForegroundOwned || rewardScene.ReleaseStage != RewardReleaseStage.None)
        {
            return rewardScene;
        }

        if (BuildShopSceneState(observer, history) is { ReleaseStage: not NonCombatReleaseStage.None } shopScene)
        {
            return shopScene;
        }

        if (BuildRestSiteSceneState(observer) is { } restSiteScene)
        {
            return restSiteScene;
        }

        if (BuildTreasureSceneState(observer) is { } treasureScene)
        {
            return treasureScene;
        }

        var eventScene = BuildEventSceneState(observer, windowBounds, history, screenshotPath);
        if (eventScene.RewardSubstateActive)
        {
            return eventScene.RewardScene;
        }

        if (eventScene.EventForegroundOwned || eventScene.ReleaseStage != EventReleaseStage.None)
        {
            return eventScene;
        }

        return null;
    }

    internal static ICanonicalNonCombatSceneState? TryBuildCanonicalNonCombatSceneState(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        return TryBuildCanonicalNonCombatSceneState(new ObserverState(observer, null, null, null), windowBounds, history, screenshotPath);
    }

    private static bool HasRecentRewardSkipReleaseIntent(IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        if (history is null || history.Count == 0)
        {
            return false;
        }

        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Action, "observer-accepted", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Action, "recapture-required", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return string.Equals(entry.TargetLabel, "reward skip", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(entry.TargetLabel, "proceed after resolving rewards", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    internal static bool HasRecentEventReleaseIntent(IReadOnlyList<GuiSmokeHistoryEntry>? history)
    {
        if (history is null || history.Count == 0)
        {
            return false;
        }

        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Action, "observer-accepted", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Action, "recapture-required", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return string.Equals(entry.TargetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(entry.TargetLabel, "ancient event completion", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static RewardMapLayerState BuildRewardMapLayerState(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return BuildRewardSceneState(observer, windowBounds).LayerState;
    }

    private static bool LooksLikeInspectOverlayState(ObserverSummary observer)
    {
        return HasOverlayChoiceState(observer)
               || observer.ActionNodes.Any(static node => IsOverlayChoiceLabel(node.Label));
    }

    private static bool IsCurrentRewardProgressionChoice(ObserverChoice choice, WindowBounds? windowBounds)
    {
        return IsExplicitRewardProgressionChoice(choice)
               && HasActiveRewardBounds(choice.ScreenBounds, windowBounds);
    }

    private static IReadOnlyList<ObserverChoice> GetClaimableRewardProgressionChoices(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        RewardScreenState? rewardState)
    {
        return observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, windowBounds))
            .Where(choice => IsClaimableRewardProgressionChoice(choice, rewardState))
            .ToArray();
    }

    private static IReadOnlyList<ObserverActionNode> GetClaimableRewardProgressionNodes(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        RewardScreenState? rewardState)
    {
        var activeRewardChoices = observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, windowBounds))
            .ToArray();
        return observer.ActionNodes
            .Where(node => IsCurrentRewardProgressionNode(node, windowBounds))
            .Where(node => IsClaimableRewardProgressionNode(node, rewardState, activeRewardChoices))
            .ToArray();
    }

    private static bool IsCurrentRewardProgressionNode(ObserverActionNode node, WindowBounds? windowBounds)
    {
        return IsExplicitRewardProgressionNode(node)
               && HasActiveRewardBounds(node.ScreenBounds, windowBounds);
    }

    private static bool IsClaimableRewardProgressionChoice(ObserverChoice choice, RewardScreenState? rewardState)
    {
        if (IsSkipOrProceedLabel(choice.Label))
        {
            return false;
        }

        return !IsPotionRewardChoice(choice) || rewardState?.HasOpenPotionSlots != false;
    }

    private static bool IsClaimableRewardProgressionNode(
        ObserverActionNode node,
        RewardScreenState? rewardState,
        IReadOnlyList<ObserverChoice> activeRewardChoices)
    {
        if (IsProceedNode(node))
        {
            return false;
        }

        return !IsPotionRewardNode(node, activeRewardChoices) || rewardState?.HasOpenPotionSlots != false;
    }

    private static bool IsStaleRewardProgressionChoice(ObserverChoice choice, WindowBounds? windowBounds)
    {
        return IsExplicitRewardProgressionChoice(choice)
               && !HasActiveRewardBounds(choice.ScreenBounds, windowBounds);
    }

    private static bool IsStaleRewardProgressionNode(ObserverActionNode node, WindowBounds? windowBounds)
    {
        return IsExplicitRewardProgressionNode(node)
               && !HasActiveRewardBounds(node.ScreenBounds, windowBounds);
    }

    private static bool HasActiveRewardBounds(string? screenBounds, WindowBounds? windowBounds)
    {
        if (HasUsableLogicalBounds(screenBounds))
        {
            return true;
        }

        return windowBounds is not null && IsBoundsInsideWindow(screenBounds, windowBounds);
    }

    private static bool HasActiveNodeBounds(string? screenBounds, WindowBounds? windowBounds)
    {
        if (HasUsableLogicalBounds(screenBounds))
        {
            return true;
        }

        return windowBounds is not null && IsBoundsInsideWindow(screenBounds, windowBounds);
    }

    private static bool IsOffWindowBounds(string? screenBounds, WindowBounds? windowBounds)
    {
        if (string.IsNullOrWhiteSpace(screenBounds) || windowBounds is null || !TryParseNodeBounds(screenBounds, out _))
        {
            return false;
        }

        return !HasUsableLogicalBounds(screenBounds)
               && !IsBoundsInsideWindow(screenBounds, windowBounds);
    }

    private static bool IsBoundsInsideWindow(string? screenBounds, WindowBounds windowBounds)
    {
        if (!TryParseNodeBounds(screenBounds, out var bounds))
        {
            return false;
        }

        return bounds.Right > windowBounds.X
               && bounds.Bottom > windowBounds.Y
               && bounds.X < windowBounds.X + windowBounds.Width
               && bounds.Y < windowBounds.Y + windowBounds.Height;
    }

    private static bool IsExplicitRewardProgressionChoice(ObserverChoice choice)
    {
        if (!TryParseNodeBounds(choice.ScreenBounds, out _)
            || IsOverlayChoiceLabel(choice.Label)
            || IsInspectPreviewChoice(choice)
            || IsDismissLikeLabel(choice.Label))
        {
            return false;
        }

        return IsRewardCardChoice(choice)
               || IsPotionRewardChoice(choice)
               || IsGoldRewardChoice(choice)
               || IsRelicRewardChoice(choice)
               || IsSkipOrProceedLabel(choice.Label)
               || choice.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || HasLargeChoiceBounds(choice.ScreenBounds);
    }

    private static bool IsExplicitRewardProgressionNode(ObserverActionNode node)
    {
        if (!node.Actionable
            || !TryParseNodeBounds(node.ScreenBounds, out _)
            || IsOverlayChoiceLabel(node.Label)
            || IsInspectPreviewBounds(node.ScreenBounds)
            || IsDismissLikeLabel(node.Label)
            || IsMapNode(node)
            || IsBackNode(node))
        {
            return false;
        }

        return IsProceedNode(node)
               || node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || HasLargeChoiceBounds(node.ScreenBounds);
    }

    private static bool IsPotionRewardNode(ObserverActionNode node, IReadOnlyList<ObserverChoice> activeRewardChoices)
    {
        if (string.Equals(node.TypeName, "potion", StringComparison.OrdinalIgnoreCase)
            || node.Kind.Contains("potion", StringComparison.OrdinalIgnoreCase)
            || node.SemanticHints.Any(static hint =>
                string.Equals(hint, "reward-potion", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "reward-type:PotionReward", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "raw-kind:potion", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return activeRewardChoices.Any(choice =>
            IsPotionRewardChoice(choice)
            && string.Equals(choice.Label, node.Label, StringComparison.OrdinalIgnoreCase)
            && RewardBoundsLookEquivalent(choice.ScreenBounds, node.ScreenBounds));
    }

    private static bool RewardBoundsLookEquivalent(string? firstBounds, string? secondBounds)
    {
        if (!TryParseNodeBounds(firstBounds, out var first) || !TryParseNodeBounds(secondBounds, out var second))
        {
            return string.Equals(firstBounds, secondBounds, StringComparison.OrdinalIgnoreCase);
        }

        return Math.Abs(first.X - second.X) <= 36f
               && Math.Abs(first.Y - second.Y) <= 36f
               && Math.Abs(first.Width - second.Width) <= 48f
               && Math.Abs(first.Height - second.Height) <= 48f;
    }

    private static bool IsRewardCardChoice(ObserverChoice choice)
    {
        if (choice.Kind.StartsWith("transform-", StringComparison.OrdinalIgnoreCase)
            || choice.Kind.StartsWith("deck-remove-", StringComparison.OrdinalIgnoreCase)
            || choice.Kind.StartsWith("upgrade-", StringComparison.OrdinalIgnoreCase)
            || choice.SemanticHints.Any(static hint =>
                string.Equals(hint, "card-selection:transform", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "card-selection:deck-remove", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "card-selection:upgrade", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return (string.Equals(choice.Kind, "card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.Kind, "reward-card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.Kind, "reward-pick-card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(choice.BindingId, "CardReward", StringComparison.OrdinalIgnoreCase)
                || choice.SemanticHints.Any(static hint => string.Equals(hint, "reward-card", StringComparison.OrdinalIgnoreCase)
                                                           || string.Equals(hint, "reward-pick", StringComparison.OrdinalIgnoreCase)
                                                           || string.Equals(hint, "reward-type:CardReward", StringComparison.OrdinalIgnoreCase)))
               && !IsSkipOrProceedLabel(choice.Label)
               && !IsConfirmLikeLabel(choice.Label)
               && !IsDismissLikeLabel(choice.Label)
               && !IsOverlayChoiceLabel(choice.Label)
               && HasRewardCardLikeBounds(choice.ScreenBounds);
    }

    private static bool HasRewardCardLikeBounds(string? screenBounds)
    {
        if (!TryParseNodeBounds(screenBounds, out var bounds))
        {
            return true;
        }

        return bounds.Width >= 120f || bounds.Height >= 150f;
    }

    private static bool IsInspectPreviewChoice(ObserverChoice choice)
    {
        return IsOverlayChoiceLabel(choice.Label)
               || string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || IsInspectPreviewBounds(choice.ScreenBounds);
    }

    private static int ScoreExplicitRewardProgressionChoice(ObserverChoice choice)
    {
        if (IsRewardCardChoice(choice))
        {
            return 280;
        }

        if (IsPotionRewardChoice(choice))
        {
            return 220;
        }

        if (IsRelicRewardChoice(choice))
        {
            return 250;
        }

        if (IsGoldRewardChoice(choice))
        {
            return 180;
        }

        if (IsSkipLikeLabel(choice.Label))
        {
            return 70;
        }

        if (IsProceedLikeLabel(choice.Label) || IsConfirmLikeLabel(choice.Label))
        {
            return 110;
        }

        if (choice.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
            || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true)
        {
            return 220;
        }

        if (HasLargeChoiceBounds(choice.ScreenBounds))
        {
            return 200;
        }

        return ScoreProgressionChoice(choice);
    }

    private static int ScoreExplicitRewardProgressionNode(ObserverActionNode node)
    {
        if (IsProceedNode(node))
        {
            return 110;
        }

        if (node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
            || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase))
        {
            return 220;
        }

        if (HasLargeChoiceBounds(node.ScreenBounds))
        {
            return 200;
        }

        return ScoreProgressionNode(node);
    }

    private static bool IsOverlayCleanupTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay back", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay backdrop close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "inspect overlay escape", StringComparison.OrdinalIgnoreCase);
    }

    private static int CountRecentOverlayCleanupAttempts(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var count = 0;
        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (IsOverlayCleanupTarget(entry.TargetLabel))
            {
                count += 1;
                continue;
            }

            if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            break;
        }

        return count;
    }

    private static int ScoreProgressionChoice(ObserverChoice choice)
    {
        if (IsOverlayChoiceLabel(choice.Label))
        {
            return -400;
        }

        if (IsInspectPreviewChoice(choice))
        {
            return -240;
        }

        if (IsDismissLikeLabel(choice.Label))
        {
            return -120;
        }

        if (IsExplicitEventProceedChoice(choice))
        {
            return 180;
        }

        if (IsRewardCardChoice(choice))
        {
            return 240;
        }

        if (IsRelicRewardChoice(choice))
        {
            return 210;
        }

        if (IsGoldRewardChoice(choice))
        {
            return 120;
        }

        if (IsSkipLikeLabel(choice.Label))
        {
            return 20;
        }

        if (IsProceedLikeLabel(choice.Label) || IsConfirmLikeLabel(choice.Label))
        {
            return 60;
        }

        if (HasLargeChoiceBounds(choice.ScreenBounds))
        {
            return 120;
        }

        return TryParseNodeBounds(choice.ScreenBounds, out _) ? 20 : 0;
    }

    private static int ScoreProgressionNode(ObserverActionNode node)
    {
        if (IsOverlayChoiceLabel(node.Label))
        {
            return -400;
        }

        if (IsInspectPreviewBounds(node.ScreenBounds))
        {
            return -240;
        }

        if (IsDismissLikeLabel(node.Label))
        {
            return -120;
        }

        if (IsExplicitEventProceedNode(node))
        {
            return 180;
        }

        if (IsSkipLikeLabel(node.Label))
        {
            return 70;
        }

        if (IsProceedLikeLabel(node.Label) || IsConfirmLikeLabel(node.Label))
        {
            return 60;
        }

        if (HasLargeChoiceBounds(node.ScreenBounds))
        {
            return 120;
        }

        if (node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
            || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
            || node.Kind.Contains("choice", StringComparison.OrdinalIgnoreCase))
        {
            return 80;
        }

        return TryParseNodeBounds(node.ScreenBounds, out _) ? 20 : 0;
    }

    private static float GetChoiceSortY(ObserverChoice choice)
    {
        return TryParseNodeBounds(choice.ScreenBounds, out var bounds) ? bounds.Y : float.MaxValue;
    }

    private static float GetChoiceSortX(ObserverChoice choice)
    {
        return TryParseNodeBounds(choice.ScreenBounds, out var bounds) ? bounds.X : float.MaxValue;
    }

    private static float GetNodeSortY(ObserverActionNode node)
    {
        return TryParseNodeBounds(node.ScreenBounds, out var bounds) ? bounds.Y : float.MaxValue;
    }

    private static float GetNodeSortX(ObserverActionNode node)
    {
        return TryParseNodeBounds(node.ScreenBounds, out var bounds) ? bounds.X : float.MaxValue;
    }

    private static string GetProgressChoiceTargetLabel(ObserverActionNode node, ObserverSummary observer)
    {
        if (AncientEventObserverSignals.IsAncientDialogueNode(node))
        {
            return "ancient dialogue advance";
        }

        if (AncientEventObserverSignals.IsExplicitAncientCompletionNode(node))
        {
            return "ancient event completion";
        }

        if (IsExplicitEventProceedNode(node))
        {
            return "visible proceed";
        }

        return GetProgressChoiceTargetLabel(node.Label, observer);
    }

    private static string GetProgressChoiceTargetLabel(ObserverChoice choice, ObserverSummary observer)
    {
        if (AncientEventObserverSignals.IsAncientDialogueChoice(choice))
        {
            return "ancient dialogue advance";
        }

        if (AncientEventObserverSignals.IsExplicitAncientCompletionChoice(choice))
        {
            return "ancient event completion";
        }

        if (IsExplicitEventProceedChoice(choice))
        {
            return "visible proceed";
        }

        return GetProgressChoiceTargetLabel(choice.Label, observer);
    }

    private static string GetProgressChoiceTargetLabel(string? label, ObserverSummary observer)
    {
        if (IsSkipLikeLabel(label))
        {
            return "reward skip";
        }

        if (GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer) && observer.Choices.Any(IsRewardCardChoice))
        {
            return "colorless card choice";
        }

        if (GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer))
        {
            return "reward choice";
        }

        return "event progression choice";
    }

    private static bool IsGoldRewardChoice(ObserverChoice choice)
    {
        return ContainsAny(choice.Label, "골드", "gold")
               || ContainsAny(choice.Description, "골드", "gold")
               || ContainsAny(choice.Value, "GOLD.")
               || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                  && string.Equals(choice.BindingId, "GoldReward", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => string.Equals(hint, "reward-gold", StringComparison.OrdinalIgnoreCase)
                                                          || string.Equals(hint, "reward-type:GoldReward", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPotionRewardChoice(ObserverChoice choice)
    {
        return string.Equals(choice.Kind, "potion", StringComparison.OrdinalIgnoreCase)
               || ContainsAny(choice.Label, "포션", "potion")
               || ContainsAny(choice.Description, "포션", "potion")
               || choice.Value?.StartsWith("POTION.", StringComparison.OrdinalIgnoreCase) == true
               || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                  && string.Equals(choice.BindingId, "PotionReward", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => string.Equals(hint, "reward-potion", StringComparison.OrdinalIgnoreCase)
                                                          || string.Equals(hint, "reward-type:PotionReward", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsRelicRewardChoice(ObserverChoice choice)
    {
        return string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || ContainsAny(choice.Description, "relic", "유물")
               || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                  && string.Equals(choice.BindingId, "RelicReward", StringComparison.OrdinalIgnoreCase)
               || choice.SemanticHints.Any(static hint => string.Equals(hint, "reward-relic", StringComparison.OrdinalIgnoreCase)
                                                          || string.Equals(hint, "reward-type:RelicReward", StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildProgressChoiceReason(ObserverChoice choice, ObserverSummary observer)
    {
        if (AncientEventObserverSignals.IsAncientDialogueChoice(choice))
        {
            return "Ancient event dialogue is still active. Advance it through the explicit dialogue hitbox before selecting an option.";
        }

        if (AncientEventObserverSignals.IsExplicitAncientCompletionChoice(choice))
        {
            return $"Ancient event completion '{choice.Label}' is still exported from an explicit NEventOptionButton proceed lane. Finish the event before handing off to map routing.";
        }

        if (IsExplicitEventProceedChoice(choice))
        {
            return $"Explicit event proceed '{choice.Label}' is exported from EventOption.IsProceed authority. Advance the event before considering any stale map overlay candidate.";
        }

        if (IsSkipLikeLabel(choice.Label))
        {
            return $"Progression skip '{choice.Label}' is visible. Prefer it over inspect or preview affordances.";
        }

        if (GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer) && observer.Choices.Any(IsRewardCardChoice))
        {
            return $"Colorless reward choice '{choice.Label}' is visible. Click a real card option, not the relic inspect icons.";
        }

        if (GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer))
        {
            return $"Reward progression choice '{choice.Label}' is visible. Prefer it over inspect or preview affordances.";
        }

        return $"Event progression choice '{choice.Label}' is visible. Use the large room option instead of inspect affordances.";
    }

    private static bool LooksLikeShopState(ObserverSummary observer)
    {
        return ShopObserverSignals.IsShopAuthorityActive(observer);
    }
}
