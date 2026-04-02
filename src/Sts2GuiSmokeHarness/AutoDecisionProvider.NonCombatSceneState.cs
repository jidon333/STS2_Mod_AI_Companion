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
        var activeRewardChoices = observer.Choices.Where(choice => IsCurrentRewardProgressionChoice(choice, windowBounds, rewardState)).ToArray();
        var activeRewardNodes = observer.ActionNodes
            .Where(node => IsCurrentRewardProgressionNode(node, windowBounds, rewardState, activeRewardChoices))
            .ToArray();
        var claimableRewardChoices = activeRewardChoices
            .Where(choice => IsClaimableRewardProgressionChoice(choice, rewardState))
            .ToArray();
        var claimableRewardNodes = activeRewardNodes
            .Where(node => IsClaimableRewardProgressionNode(node, rewardState, activeRewardChoices))
            .ToArray();
        var staleRewardChoices = observer.Choices.Where(choice => IsStaleRewardProgressionChoice(choice, windowBounds, rewardState)).ToArray();
        var staleRewardNodes = observer.ActionNodes.Where(node => IsStaleRewardProgressionNode(node, windowBounds, rewardState)).ToArray();
        var explicitRewardChoicesPresent = activeRewardChoices.Length > 0 || activeRewardNodes.Length > 0;
        var rewardContextVisible = rewardState?.ScreenVisible == true
                                   || RewardObserverSignals.IsRewardAuthorityActive(observer)
                                   || GuiSmokeObserverPhaseHeuristics.LooksLikeRewardsState(observer)
                                   || MatchesControlFlowScreen(observer, "rewards")
                                   || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase);
        var intrinsicRewardCardAuthority = observer.Choices.Any(choice => IsRewardCardChoice(choice, observer))
                                           || activeRewardNodes.Any(node => IsCardRewardNode(node, activeRewardChoices));
        var rewardScreenHint = rewardContextVisible || intrinsicRewardCardAuthority;
        var rewardForegroundOwned = !strongerRoomForegroundAuthority
                                    && (rewardState?.ForegroundOwned == true
                                        || (rewardScreenHint
                                            && rewardState?.MapIsCurrentActiveScreen != true
                                            && (explicitRewardChoicesPresent || intrinsicRewardCardAuthority)));
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
                                  && intrinsicRewardCardAuthority;
        var colorlessChoiceVisible = !strongerRoomForegroundAuthority
                                     && !CardSelectionObserverSignals.IsNonRewardCountConfirmFamily(observer)
                                     && !GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer)
                                     && rewardScreenHint
                                     && intrinsicRewardCardAuthority
                                     && observer.Choices.Any(IsInspectPreviewChoice);
        var explicitProceedVisible = rewardState?.ProceedVisible == true
                                     || rewardState?.ProceedEnabled == true
                                     || (!strongerRoomForegroundAuthority
                                         && (activeRewardChoices.Any(choice => IsSkipOrProceedLabel(choice.Label))
                                             || activeRewardNodes.Any(IsProceedNode)));
        var claimableRewardPresent = !strongerRoomForegroundAuthority
                                     && (claimableRewardChoices.Length > 0
                                         || claimableRewardNodes.Length > 0);
        var aftermathResiduePresent = rewardForegroundOwned
                                      && !string.IsNullOrWhiteSpace(observer.ChoiceExtractorPath)
                                      && !string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
                                      && !string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase);
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
            : claimableRewardPresent
                ? RewardExplicitActionKind.Claim
                : colorlessChoiceVisible
                    ? RewardExplicitActionKind.ColorlessChoice
                    : rewardChoiceVisible
                        ? RewardExplicitActionKind.CardChoice
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
            suppressSameSkipReissue,
            rewardChoiceVisible || colorlessChoiceVisible,
            claimableRewardPresent,
            aftermathResiduePresent);
    }

    internal static CombatReleaseState BuildCombatReleaseState(
        ObserverState observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null,
        PostNodeHandoffState? handoffState = null,
        CombatBarrierEvaluation? combatBarrier = null)
    {
        var nextRoomEntryState = BuildNextRoomEntryState(observer, windowBounds, history, screenshotPath);
        handoffState ??= BuildCombatResolutionHandoffState(observer, windowBounds, history, screenshotPath);
        combatBarrier ??= CombatBarrierSupport.Inactive;
        var runtime = CombatRuntimeStateSupport.Read(observer.Summary, Array.Empty<CombatCardKnowledgeHint>());

        if (nextRoomEntryState.HasExplicitWinner
            && nextRoomEntryState.HandoffTarget is not NonCombatHandoffTarget.None and not NonCombatHandoffTarget.HandleCombat)
        {
            handoffState = BuildPostNodeHandoffStateForNextRoomEntry(nextRoomEntryState, observer, screenshotPath);
        }

        var releaseTarget = handoffState.HandoffTarget;
        var foregroundOwner = handoffState.Owner;
        var hasExplicitForegroundSurface = handoffState.HasExplicitSurface;
        var releaseMismatch = handoffState.ContractMismatch
                              || (foregroundOwner != NonCombatCanonicalForegroundOwner.Combat
                                  && releaseTarget is not NonCombatHandoffTarget.None and not NonCombatHandoffTarget.HandleCombat
                                  && handoffState.ReleaseStage == NonCombatReleaseStage.None
                                  && !hasExplicitForegroundSurface);
        var releaseSubtype = combatBarrier.Kind switch
        {
            CombatBarrierKind.EnemyClick => CombatReleaseSubtype.EnemyClickResidue,
            CombatBarrierKind.EndTurn => CombatReleaseSubtype.EndTurnReopenLatency,
            _ => CombatReleaseSubtype.None,
        };
        var lifecycleStage = DetermineCombatLifecycleStage(observer.Summary, runtime, nextRoomEntryState, handoffState);
        var combatAuthorityState = releaseTarget is NonCombatHandoffTarget.None or NonCombatHandoffTarget.HandleCombat
            ? CombatAuthorityState.Active
            : hasExplicitForegroundSurface
                ? CombatAuthorityState.Released
                : CombatAuthorityState.ResidueOnly;
        if (lifecycleStage == CombatLifecycleStage.Inactive
            && combatAuthorityState == CombatAuthorityState.Active)
        {
            combatAuthorityState = CombatAuthorityState.ResidueOnly;
        }

        return new CombatReleaseState(
            combatBarrier.Kind,
            lifecycleStage,
            combatAuthorityState,
            foregroundOwner,
            releaseTarget,
            handoffState.ReleaseStage,
            hasExplicitForegroundSurface,
            releaseMismatch,
            releaseSubtype,
            handoffState);
    }

    private static CombatLifecycleStage DetermineCombatLifecycleStage(
        ObserverSummary observer,
        CombatRuntimeState runtime,
        NextRoomEntryState nextRoomEntryState,
        PostNodeHandoffState handoffState)
    {
        if ((nextRoomEntryState.HasExplicitWinner
             && nextRoomEntryState.Owner is not (NonCombatCanonicalForegroundOwner.Unknown or NonCombatCanonicalForegroundOwner.Combat))
            || (handoffState.Owner is not NonCombatCanonicalForegroundOwner.Combat
                && handoffState.HandoffTarget is not NonCombatHandoffTarget.None and not NonCombatHandoffTarget.HandleCombat
                && handoffState.HasExplicitSurface))
        {
            return CombatLifecycleStage.ReleasedToNonCombat;
        }

        bool? combatInProgress = observer.InCombat;
        if (combatInProgress is null
            && bool.TryParse(CombatAuthoritySupport.TryGetMetaValue(observer, "combatPrimaryValue"), out var parsedPrimary))
        {
            combatInProgress = parsedPrimary;
        }
        var isPlayPhase = CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsPlayPhase", "CombatManager.IsPlayPhase");
        var isEnemyTurnStarted = CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsEnemyTurnStarted", "CombatManager.IsEnemyTurnStarted");
        if (combatInProgress == false)
        {
            return CombatLifecycleStage.Inactive;
        }

        var playerWindowOpen = isPlayPhase == true
                               && isEnemyTurnStarted == false
                               && runtime.PlayerActionsDisabled == false
                               && runtime.EndingPlayerTurnPhaseOne == false
                               && runtime.EndingPlayerTurnPhaseTwo == false;
        if (playerWindowOpen)
        {
            return CombatLifecycleStage.PlayerPlayOpen;
        }

        if (isEnemyTurnStarted == true)
        {
            return CombatLifecycleStage.EnemyTurn;
        }

        if (runtime.PlayerActionsDisabled == true
            || runtime.EndingPlayerTurnPhaseOne == true
            || runtime.EndingPlayerTurnPhaseTwo == true)
        {
            return CombatLifecycleStage.EndTurnTransit;
        }

        if (isPlayPhase == false)
        {
            return CombatLifecycleStage.PlayerReopenPending;
        }

        return CombatLifecycleStage.Unknown;
    }

    internal static NextRoomEntryState BuildNextRoomEntryState(
        ObserverState observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null,
        bool assumeRecentMapClickAccepted = false)
    {
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(observer, windowBounds, screenshotPath);
        var rewardScene = BuildRewardSceneState(observer, windowBounds, history, screenshotPath);
        var eventScene = BuildEventSceneState(observer, windowBounds, history, screenshotPath);
        var shopScene = BuildShopSceneState(observer, history);
        var restSiteScene = BuildRestSiteSceneState(observer);
        var treasureScene = BuildTreasureSceneState(observer);
        var mapExplicitOwner = NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer);
        var recentMapClickAccepted = assumeRecentMapClickAccepted || HasRecentMapClickAccepted(history);
        var combatResiduePresent = HasCombatResolutionAuthority(observer.Summary);
        var rewardResiduePresent = rewardScene.ReleaseStage != RewardReleaseStage.None || rewardScene.AftermathResiduePresent;

        if (HasExplicitShopRoomEntrySurface(observer.Summary, shopScene))
        {
            return new NextRoomEntryState(
                NonCombatCanonicalForegroundOwner.Shop,
                NonCombatHandoffTarget.HandleShop,
                NonCombatReleaseStage.Active,
                recentMapClickAccepted ? NextRoomTransitStage.Settled : NextRoomTransitStage.None,
                ExplicitSurfacePresent: true,
                SurfaceKind: PostNodeHandoffSurfaceKind.Shop,
                RecentMapClickAccepted: recentMapClickAccepted,
                MapTransitPending: false,
                CombatResiduePresent: combatResiduePresent,
                RewardResiduePresent: rewardResiduePresent,
                MapOverlayResiduePresent: mapOverlayState.ForegroundVisible);
        }

        if (eventScene.EventForegroundOwned
            && eventScene.ReleaseStage == EventReleaseStage.Active
            && eventScene.ExplicitRoomEntrySurfacePresent)
        {
            return new NextRoomEntryState(
                NonCombatCanonicalForegroundOwner.Event,
                NonCombatHandoffTarget.HandleEvent,
                NonCombatReleaseStage.Active,
                recentMapClickAccepted ? NextRoomTransitStage.Settled : NextRoomTransitStage.None,
                ExplicitSurfacePresent: true,
                SurfaceKind: PostNodeHandoffSurfaceKind.Event,
                RecentMapClickAccepted: recentMapClickAccepted,
                MapTransitPending: false,
                CombatResiduePresent: combatResiduePresent,
                RewardResiduePresent: rewardResiduePresent,
                MapOverlayResiduePresent: mapOverlayState.ForegroundVisible);
        }

        if (!recentMapClickAccepted
            && rewardScene.RewardForegroundOwned
            && rewardScene.ReleaseStage == RewardReleaseStage.Active
            && (rewardScene.ClaimSurfacePresent
                || rewardScene.CardProgressionSurfacePresent
                || rewardScene.ExplicitProceedVisible))
        {
            return new NextRoomEntryState(
                NonCombatCanonicalForegroundOwner.Reward,
                NonCombatHandoffTarget.HandleRewards,
                NonCombatReleaseStage.Active,
                NextRoomTransitStage.None,
                ExplicitSurfacePresent: true,
                SurfaceKind: PostNodeHandoffSurfaceKind.Reward,
                RecentMapClickAccepted: false,
                MapTransitPending: false,
                CombatResiduePresent: combatResiduePresent,
                RewardResiduePresent: rewardResiduePresent,
                MapOverlayResiduePresent: mapOverlayState.ForegroundVisible);
        }

        if (restSiteScene is { ReleaseStage: NonCombatReleaseStage.Active })
        {
            var surfaceKind = restSiteScene.SmithUpgradeActive
                ? PostNodeHandoffSurfaceKind.RestSiteSmithUpgrade
                : restSiteScene.ProceedVisible
                    ? PostNodeHandoffSurfaceKind.RestSiteProceed
                    : restSiteScene.SelectionSettling
                        ? PostNodeHandoffSurfaceKind.RestSiteSelectionSettling
                        : PostNodeHandoffSurfaceKind.RestSiteChoice;
            return new NextRoomEntryState(
                NonCombatCanonicalForegroundOwner.RestSite,
                NonCombatHandoffTarget.ChooseFirstNode,
                NonCombatReleaseStage.Active,
                recentMapClickAccepted ? NextRoomTransitStage.Settled : NextRoomTransitStage.None,
                ExplicitSurfacePresent: true,
                SurfaceKind: surfaceKind,
                RecentMapClickAccepted: recentMapClickAccepted,
                MapTransitPending: false,
                CombatResiduePresent: combatResiduePresent,
                RewardResiduePresent: rewardResiduePresent,
                MapOverlayResiduePresent: mapOverlayState.ForegroundVisible);
        }

        if (treasureScene is not null)
        {
            return new NextRoomEntryState(
                NonCombatCanonicalForegroundOwner.Treasure,
                NonCombatHandoffTarget.ChooseFirstNode,
                NonCombatReleaseStage.Active,
                recentMapClickAccepted ? NextRoomTransitStage.Settled : NextRoomTransitStage.None,
                ExplicitSurfacePresent: true,
                SurfaceKind: PostNodeHandoffSurfaceKind.Treasure,
                RecentMapClickAccepted: recentMapClickAccepted,
                MapTransitPending: false,
                CombatResiduePresent: combatResiduePresent,
                RewardResiduePresent: rewardResiduePresent,
                MapOverlayResiduePresent: mapOverlayState.ForegroundVisible);
        }

        if (!recentMapClickAccepted && mapExplicitOwner)
        {
            return new NextRoomEntryState(
                NonCombatCanonicalForegroundOwner.Map,
                NonCombatHandoffTarget.ChooseFirstNode,
                NonCombatReleaseStage.Active,
                NextRoomTransitStage.Settled,
                ExplicitSurfacePresent: true,
                SurfaceKind: mapOverlayState.ForegroundVisible ? PostNodeHandoffSurfaceKind.MapOverlay : PostNodeHandoffSurfaceKind.MapNode,
                RecentMapClickAccepted: false,
                MapTransitPending: false,
                CombatResiduePresent: combatResiduePresent,
                RewardResiduePresent: rewardResiduePresent,
                MapOverlayResiduePresent: mapOverlayState.ForegroundVisible);
        }

        return new NextRoomEntryState(
            NonCombatCanonicalForegroundOwner.Unknown,
            NonCombatHandoffTarget.None,
            NonCombatReleaseStage.None,
            recentMapClickAccepted ? NextRoomTransitStage.RoomEntryPending : NextRoomTransitStage.None,
            ExplicitSurfacePresent: false,
            SurfaceKind: PostNodeHandoffSurfaceKind.None,
            RecentMapClickAccepted: recentMapClickAccepted,
            MapTransitPending: recentMapClickAccepted,
            CombatResiduePresent: combatResiduePresent,
            RewardResiduePresent: rewardResiduePresent,
            MapOverlayResiduePresent: mapOverlayState.ForegroundVisible);
    }

    internal static CombatReleaseState BuildCombatReleaseState(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null,
        PostNodeHandoffState? handoffState = null,
        CombatBarrierEvaluation? combatBarrier = null)
    {
        return BuildCombatReleaseState(new ObserverState(observer, null, null, null), windowBounds, history, screenshotPath, handoffState, combatBarrier);
    }

    internal static EventSceneState BuildEventSceneState(
        ObserverState observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        return BuildEventSceneStateCore(
            observer.Summary,
            windowBounds,
            history,
            screenshotPath,
            NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer),
            NonCombatForegroundOwnership.HasAuthoritativeMapForegroundScreen(observer));
    }

    internal static EventSceneState BuildEventSceneState(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        return BuildEventSceneStateCore(
            observer,
            windowBounds,
            history,
            screenshotPath,
            NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer),
            NonCombatForegroundOwnership.HasAuthoritativeMapForegroundScreen(observer));
    }

    private static EventSceneState BuildEventSceneStateCore(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history,
        string? screenshotPath,
        bool mapExplicitOwner,
        bool mapForegroundSuppressesEventSurface)
    {
        var rewardScene = BuildRewardSceneState(observer, windowBounds, history, screenshotPath);
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(observer, windowBounds, screenshotPath);
        var mapReleaseSignal = HasPostNodeMapReleaseSignal(observer, mapOverlayState, mapExplicitOwner);
        var ancientContract = AncientEventObserverSignals.GetAncientEventOptionContractState(observer, windowBounds);
        var ancientDialogueActive = ancientContract.HasExplicitDialogueSurface;
        var ancientCompletionActive = ancientContract.HasExplicitCompletionSurface;
        var ancientOptionActive = ancientContract.HasExplicitOptionSurface;
        var ancientOptionContractMismatch = ancientContract.ContractLane == AncientEventObserverSignals.AncientEventContractLane.OptionContractMismatch;
        var eventChoiceAuthority = EventProceedObserverSignals.HasEventChoiceAuthority(observer);
        var foregroundGenericEventChoiceSurfaceWithoutBounds = HasForegroundGenericEventChoiceSurfaceWithoutBounds(observer, windowBounds);
        var rawEventChoiceFamilyPresent = (!mapForegroundSuppressesEventSurface || foregroundGenericEventChoiceSurfaceWithoutBounds)
                                          && !rewardScene.RewardForegroundOwned
                                          && HasRawEventChoiceFamily(observer);
        var rawExplicitProceedVisible = !mapForegroundSuppressesEventSurface
                                        && !rewardScene.RewardForegroundOwned
                                        && EventProceedObserverSignals.HasExplicitEventProceedSignal(observer, windowBounds);
        var rawActiveEventChoiceVisible = (!mapForegroundSuppressesEventSurface || foregroundGenericEventChoiceSurfaceWithoutBounds)
                                          && !rewardScene.RewardForegroundOwned
                                          && HasRawExplicitEventChoiceVisible(observer, windowBounds);
        var weakEventChoiceFamilyPresent = rawEventChoiceFamilyPresent && !rawActiveEventChoiceVisible;
        var rawExplicitRoomEntrySurfacePresent = HasExplicitEventRoomEntrySurface(observer, windowBounds)
                                                 && (ancientDialogueActive
                                                     || ancientCompletionActive
                                                     || ancientOptionActive
                                                     || ancientOptionContractMismatch
                                                     || rawActiveEventChoiceVisible);
        var eventReleaseToMapActive = HasRecentEventReleaseIntent(history)
                                      && (mapReleaseSignal || mapOverlayState.ForegroundVisible)
                                      && !rawExplicitRoomEntrySurfacePresent;
        var explicitProceedVisible = rawExplicitProceedVisible && !eventReleaseToMapActive;
        var activeEventChoiceVisible = rawActiveEventChoiceVisible && !eventReleaseToMapActive;
        var forceProgressionAfterCardSelection = HasRecentCardSelectionSubtypeAftermath(history ?? Array.Empty<GuiSmokeHistoryEntry>())
                                                && (explicitProceedVisible || activeEventChoiceVisible);
        var explicitRoomEntrySurfacePresent = rawExplicitRoomEntrySurfacePresent
                                              || (HasExplicitEventRoomEntrySurface(observer, windowBounds)
                                                  && forceProgressionAfterCardSelection);
        var rewardSubstateActive = !explicitRoomEntrySurfacePresent
                                   && (rewardScene.RewardForegroundOwned || rewardScene.ReleaseStage == RewardReleaseStage.ReleasePending);
        var mapContextVisible = mapOverlayState.ForegroundVisible
                                || MatchesControlFlowScreen(observer, "map")
                                || rewardScene.LayerState.MapContextVisible
                                || mapReleaseSignal;
        var hasExplicitProgression = ancientDialogueActive
                                     || ancientCompletionActive
                                     || ancientOptionActive
                                     || ancientOptionContractMismatch
                                     || explicitProceedVisible
                                     || activeEventChoiceVisible
                                     || forceProgressionAfterCardSelection;
        var strongForegroundChoice = explicitRoomEntrySurfacePresent
                                     || ancientDialogueActive
                                     || ancientCompletionActive
                                     || ancientOptionActive
                                     || ancientOptionContractMismatch
                                     || explicitProceedVisible
                                     || activeEventChoiceVisible
                                     || forceProgressionAfterCardSelection;
        var suppressSameProceedReissue = !rewardSubstateActive
                                         && !explicitRoomEntrySurfacePresent
                                         && (ancientCompletionActive || rawExplicitProceedVisible)
                                         && HasRecentEventReleaseIntent(history);
        var eventChoiceFamilyAuthority = eventChoiceAuthority || rawEventChoiceFamilyPresent;
        var canonicalOwner = rewardSubstateActive
            ? NonCombatForegroundOwner.Reward
            : strongForegroundChoice
              || suppressSameProceedReissue
              || (eventChoiceFamilyAuthority && (hasExplicitProgression || weakEventChoiceFamilyPresent))
                ? NonCombatForegroundOwner.Event
            : mapExplicitOwner
                ? NonCombatForegroundOwner.Map
            : (mapOverlayState.ForegroundVisible || mapReleaseSignal) && !strongForegroundChoice
                    ? NonCombatForegroundOwner.Map
                    : NonCombatForegroundOwner.Unknown;
        var releaseStage = rewardSubstateActive
            ? EventReleaseStage.Released
            : canonicalOwner == NonCombatForegroundOwner.Event
                ? suppressSameProceedReissue || (weakEventChoiceFamilyPresent && !hasExplicitProgression)
                    ? EventReleaseStage.ReleasePending
                    : EventReleaseStage.Active
                : HasRecentEventReleaseIntent(history) && (mapReleaseSignal || mapOverlayState.ForegroundVisible)
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
                        : ancientOptionContractMismatch
                            ? EventExplicitActionKind.AncientOptionContractMismatch
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
            ancientContract,
            mapContextVisible,
            rewardSubstateActive,
            hasExplicitProgression,
            explicitRoomEntrySurfacePresent,
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
        var authoritativeMapForegroundScreen = NonCombatForegroundOwnership.HasAuthoritativeMapForegroundScreen(observer);
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
        var selectionSettling = explicitScreenAuthority
                                && explicitChoiceVisible
                                && !smithUpgradeActive
                                && !proceedVisible
                                && RestSiteObserverSignals.IsRestSiteSelectionSettlingState(observer.Summary);
        var selectionAcceptedRecently = explicitScreenAuthority
                                        && RestSiteObserverSignals.HasSelectionAcceptedRecently(observer.Summary);
        var releasePending = explicitScreenAuthority
                             && selectionAcceptedRecently
                             && !explicitChoiceVisible
                             && !smithUpgradeActive
                             && !proceedVisible
                             && !selectionSettling;
        if (authoritativeMapForegroundScreen
            && !explicitChoiceVisible
            && !smithUpgradeActive
            && !proceedVisible
            && !selectionSettling)
        {
            return null;
        }

        if (!smithUpgradeActive && !explicitChoiceVisible && !proceedVisible && !selectionSettling && !releasePending)
        {
            return null;
        }

        var mapContextVisible = MatchesControlFlowScreen(observer, "map")
                                || NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer.Summary);
        var mapOverlayResiduePresent = !authoritativeMapForegroundScreen
                                       && NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer.Summary);
        var aftermathResiduePresent = releasePending
                                      || (!string.IsNullOrWhiteSpace(observer.ChoiceExtractorPath)
                                          && !string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase)
                                          && explicitScreenAuthority);
        return new RestSiteSceneState(
            explicitChoiceVisible,
            smithUpgradeActive,
            RestSiteObserverSignals.HasSmithConfirmVisible(observer.Summary),
            proceedVisible,
            selectionSettling,
            selectionAcceptedRecently,
            aftermathResiduePresent,
            mapOverlayResiduePresent,
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

    internal static PostNodeHandoffState BuildPostNodeHandoffState(
        ObserverState observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(observer.Summary))
        {
            return new PostNodeHandoffState(
                NonCombatCanonicalForegroundOwner.Unknown,
                NonCombatHandoffTarget.None,
                NonCombatReleaseStage.None,
                HasExplicitSurface: false,
                PostNodeHandoffSurfaceKind.None,
                ContractMismatch: false,
                MapOverlayVisible: false,
                StaleBackgroundPresent: false);
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(observer, windowBounds, screenshotPath);
        var mapExplicitOwner = NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer);
        var mapAuthoritativeScreen = NonCombatForegroundOwnership.HasAuthoritativeMapForegroundScreen(observer);
        var mapReleaseSignal = HasPostNodeMapReleaseSignal(observer.Summary, mapOverlayState, mapExplicitOwner);
        var combatHandoffState = TryBuildCombatPostNodeHandoffState(observer.Summary, mapOverlayState, mapExplicitOwner);

        var rewardScene = BuildRewardSceneState(observer, windowBounds, history, screenshotPath);
        if (rewardScene.RewardForegroundOwned || rewardScene.ReleaseStage != RewardReleaseStage.None)
        {
            return BuildRewardPostNodeHandoffState(observer.Summary, rewardScene, mapOverlayState, mapExplicitOwner);
        }

        if (BuildShopSceneState(observer, history) is { ReleaseStage: not NonCombatReleaseStage.None } shopScene)
        {
            return BuildShopPostNodeHandoffState(observer.Summary, shopScene, mapOverlayState, mapExplicitOwner);
        }

        if (BuildRestSiteSceneState(observer) is { } restSiteScene)
        {
            return BuildRestSitePostNodeHandoffState(observer.Summary, restSiteScene, mapOverlayState, mapExplicitOwner);
        }

        if (BuildTreasureSceneState(observer) is { } treasureScene)
        {
            return BuildTreasurePostNodeHandoffState(observer.Summary, treasureScene, mapOverlayState, mapExplicitOwner);
        }

        if (combatHandoffState is not null)
        {
            return combatHandoffState;
        }

        if (mapExplicitOwner && NonCombatForegroundOwnership.HasAuthoritativeMapForegroundScreen(observer))
        {
            return BuildMapPostNodeHandoffState(
                observer.Summary,
                mapOverlayState,
                mapExplicitOwner: true,
                staleBackgroundPresent: HasObserverStaleRoomBackground(observer.Summary));
        }

        var eventScene = BuildEventSceneState(observer, windowBounds, history, screenshotPath);
        if (eventScene.RewardSubstateActive)
        {
            return BuildRewardPostNodeHandoffState(observer.Summary, eventScene.RewardScene, mapOverlayState, mapExplicitOwner);
        }

        if (eventScene.EventForegroundOwned || eventScene.ReleaseStage != EventReleaseStage.None || eventScene.MapForegroundOwned)
        {
            return BuildEventPostNodeHandoffState(observer.Summary, eventScene, mapOverlayState, mapExplicitOwner, mapReleaseSignal);
        }

        if (combatHandoffState is not null)
        {
            return combatHandoffState;
        }

        if (mapExplicitOwner || mapReleaseSignal || mapAuthoritativeScreen)
        {
            return BuildMapPostNodeHandoffState(observer.Summary, mapOverlayState, mapExplicitOwner, staleBackgroundPresent: HasObserverStaleRoomBackground(observer.Summary));
        }

        return new PostNodeHandoffState(
            NonCombatCanonicalForegroundOwner.Unknown,
            NonCombatHandoffTarget.None,
            NonCombatReleaseStage.None,
            HasExplicitSurface: false,
            PostNodeHandoffSurfaceKind.None,
            ContractMismatch: false,
            MapOverlayVisible: mapOverlayState.ForegroundVisible,
            StaleBackgroundPresent: HasObserverStaleRoomBackground(observer.Summary));
    }

    internal static PostNodeHandoffState BuildPostNodeHandoffState(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        return BuildPostNodeHandoffState(new ObserverState(observer, null, null, null), windowBounds, history, screenshotPath);
    }

    internal static PostNodeHandoffState BuildCombatResolutionHandoffState(
        ObserverState observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(observer.Summary))
        {
            return new PostNodeHandoffState(
                NonCombatCanonicalForegroundOwner.Unknown,
                NonCombatHandoffTarget.None,
                NonCombatReleaseStage.None,
                HasExplicitSurface: false,
                PostNodeHandoffSurfaceKind.None,
                ContractMismatch: false,
                MapOverlayVisible: false,
                StaleBackgroundPresent: false);
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(observer, windowBounds, screenshotPath);
        var mapExplicitOwner = NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer);
        var mapAuthoritativeScreen = NonCombatForegroundOwnership.HasAuthoritativeMapForegroundScreen(observer);
        var mapReleaseSignal = HasPostNodeMapReleaseSignal(observer.Summary, mapOverlayState, mapExplicitOwner);
        var combatResolutionAuthority = HasCombatResolutionAuthority(observer.Summary);
        var nextRoomEntryState = BuildNextRoomEntryState(observer, windowBounds, history, screenshotPath);

        if (nextRoomEntryState.HasExplicitWinner
            && nextRoomEntryState.Owner is not (NonCombatCanonicalForegroundOwner.Unknown or NonCombatCanonicalForegroundOwner.Combat))
        {
            return BuildPostNodeHandoffStateForNextRoomEntry(nextRoomEntryState, observer.Summary, mapOverlayState);
        }

        var rewardScene = BuildRewardSceneState(observer, windowBounds, history, screenshotPath);
        if (rewardScene.RewardForegroundOwned || rewardScene.ReleaseStage != RewardReleaseStage.None)
        {
            var rewardHandoffState = BuildRewardPostNodeHandoffState(observer.Summary, rewardScene, mapOverlayState, mapExplicitOwner);
            if (!combatResolutionAuthority
                || rewardHandoffState.Owner != NonCombatCanonicalForegroundOwner.Map
                || rewardHandoffState.HasExplicitSurface)
            {
                return rewardHandoffState;
            }
        }

        if (BuildShopSceneState(observer, history) is { ReleaseStage: not NonCombatReleaseStage.None } shopScene)
        {
            var shopHandoffState = BuildShopPostNodeHandoffState(observer.Summary, shopScene, mapOverlayState, mapExplicitOwner);
            if (!combatResolutionAuthority
                || shopHandoffState.Owner != NonCombatCanonicalForegroundOwner.Map
                || shopHandoffState.HasExplicitSurface)
            {
                return shopHandoffState;
            }
        }

        if (BuildRestSiteSceneState(observer) is { } restSiteScene)
        {
            return BuildRestSitePostNodeHandoffState(observer.Summary, restSiteScene, mapOverlayState, mapExplicitOwner);
        }

        if (BuildTreasureSceneState(observer) is { } treasureScene)
        {
            return BuildTreasurePostNodeHandoffState(observer.Summary, treasureScene, mapOverlayState, mapExplicitOwner);
        }

        var eventScene = BuildEventSceneState(observer, windowBounds, history, screenshotPath);
        if (eventScene.RewardSubstateActive)
        {
            var rewardHandoffState = BuildRewardPostNodeHandoffState(observer.Summary, eventScene.RewardScene, mapOverlayState, mapExplicitOwner);
            if (!combatResolutionAuthority
                || rewardHandoffState.Owner != NonCombatCanonicalForegroundOwner.Map
                || rewardHandoffState.HasExplicitSurface)
            {
                return rewardHandoffState;
            }
        }

        if (eventScene.EventForegroundOwned || eventScene.ReleaseStage != EventReleaseStage.None || eventScene.MapForegroundOwned)
        {
            var eventHandoffState = BuildEventPostNodeHandoffState(observer.Summary, eventScene, mapOverlayState, mapExplicitOwner, mapReleaseSignal);
            if (!combatResolutionAuthority
                || eventHandoffState.Owner != NonCombatCanonicalForegroundOwner.Map
                || eventHandoffState.HasExplicitSurface)
            {
                return eventHandoffState;
            }
        }

        if (combatResolutionAuthority)
        {
            var hasExplicitSurface = HasExplicitCombatTakeoverSurface(observer.Summary);
            var pending = IsCombatTakeoverPending(observer.Summary, hasExplicitSurface);
            return new PostNodeHandoffState(
                NonCombatCanonicalForegroundOwner.Combat,
                NonCombatHandoffTarget.HandleCombat,
                pending ? NonCombatReleaseStage.ReleasePending : NonCombatReleaseStage.Active,
                HasExplicitSurface: hasExplicitSurface && !pending,
                pending ? PostNodeHandoffSurfaceKind.CombatTakeover : PostNodeHandoffSurfaceKind.CombatForeground,
                ContractMismatch: false,
                MapOverlayVisible: mapOverlayState.ForegroundVisible,
                StaleBackgroundPresent: mapOverlayState.ForegroundVisible || HasObserverStaleRoomBackground(observer.Summary));
        }

        if (mapExplicitOwner || mapReleaseSignal || mapAuthoritativeScreen)
        {
            return BuildMapPostNodeHandoffState(
                observer.Summary,
                mapOverlayState,
                mapExplicitOwner,
                staleBackgroundPresent: HasObserverStaleRoomBackground(observer.Summary));
        }

        return new PostNodeHandoffState(
            NonCombatCanonicalForegroundOwner.Unknown,
            NonCombatHandoffTarget.None,
            NonCombatReleaseStage.None,
            HasExplicitSurface: false,
            PostNodeHandoffSurfaceKind.None,
            ContractMismatch: false,
            MapOverlayVisible: mapOverlayState.ForegroundVisible,
            StaleBackgroundPresent: HasObserverStaleRoomBackground(observer.Summary));
    }

    internal static PostNodeHandoffState BuildCombatResolutionHandoffState(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        IReadOnlyList<GuiSmokeHistoryEntry>? history = null,
        string? screenshotPath = null)
    {
        return BuildCombatResolutionHandoffState(new ObserverState(observer, null, null, null), windowBounds, history, screenshotPath);
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

    private static PostNodeHandoffState BuildRewardPostNodeHandoffState(
        ObserverSummary observer,
        RewardSceneState rewardScene,
        MapOverlayState mapOverlayState,
        bool mapExplicitOwner)
    {
        var owner = ((ICanonicalNonCombatSceneState)rewardScene).CanonicalForegroundOwner;
        var releaseStage = ((ICanonicalNonCombatSceneState)rewardScene).ReleaseStage;
        var handoffTarget = ((ICanonicalNonCombatSceneState)rewardScene).HandoffTarget;
        var mapSurfaceOwned = owner == NonCombatCanonicalForegroundOwner.Map && mapExplicitOwner;
        var mapSurfacePending = owner == NonCombatCanonicalForegroundOwner.Map && !mapExplicitOwner;
        var resolvedReleaseStage = mapSurfaceOwned
            ? NonCombatReleaseStage.Active
            : mapSurfacePending
                ? NonCombatReleaseStage.ReleasePending
                : releaseStage;
        var resolvedHandoffTarget = mapSurfaceOwned
            ? NonCombatHandoffTarget.ChooseFirstNode
            : mapSurfacePending
                ? NonCombatHandoffTarget.WaitMap
                : handoffTarget;
        var hasExplicitSurface = mapSurfaceOwned
                                 || (rewardScene.ExplicitAction != RewardExplicitActionKind.None
                                     && releaseStage == NonCombatReleaseStage.Active);
        return new PostNodeHandoffState(
            owner,
            resolvedHandoffTarget,
            resolvedReleaseStage,
            hasExplicitSurface,
            mapSurfaceOwned
                ? mapOverlayState.ForegroundVisible ? PostNodeHandoffSurfaceKind.MapOverlay : PostNodeHandoffSurfaceKind.MapNode
                : owner == NonCombatCanonicalForegroundOwner.Map
                    ? PostNodeHandoffSurfaceKind.MapSurfacePending
                    : hasExplicitSurface ? PostNodeHandoffSurfaceKind.Reward : PostNodeHandoffSurfaceKind.None,
            ContractMismatch: owner != NonCombatCanonicalForegroundOwner.Map
                              && hasExplicitSurface
                              && HasExporterMapForegroundClaim(observer)
                              && !mapExplicitOwner,
            MapOverlayVisible: mapOverlayState.ForegroundVisible,
            StaleBackgroundPresent: rewardScene.LayerState.MapContextVisible);
    }

    private static PostNodeHandoffState BuildShopPostNodeHandoffState(
        ObserverSummary observer,
        ShopSceneState shopScene,
        MapOverlayState mapOverlayState,
        bool mapExplicitOwner)
    {
        var owner = shopScene.CanonicalForegroundOwner;
        var releaseStage = shopScene.ReleaseStage;
        var mapSurfaceOwned = owner == NonCombatCanonicalForegroundOwner.Map && mapExplicitOwner;
        var mapSurfacePending = owner == NonCombatCanonicalForegroundOwner.Map && !mapExplicitOwner;
        var resolvedReleaseStage = mapSurfaceOwned
            ? NonCombatReleaseStage.Active
            : mapSurfacePending
                ? NonCombatReleaseStage.ReleasePending
                : releaseStage;
        var resolvedHandoffTarget = mapSurfaceOwned
            ? NonCombatHandoffTarget.ChooseFirstNode
            : mapSurfacePending
                ? NonCombatHandoffTarget.WaitMap
                : shopScene.HandoffTarget;
        var hasExplicitSurface = mapSurfaceOwned || releaseStage == NonCombatReleaseStage.Active;
        return new PostNodeHandoffState(
            owner,
            resolvedHandoffTarget,
            resolvedReleaseStage,
            hasExplicitSurface,
            mapSurfaceOwned
                ? mapOverlayState.ForegroundVisible ? PostNodeHandoffSurfaceKind.MapOverlay : PostNodeHandoffSurfaceKind.MapNode
                : owner == NonCombatCanonicalForegroundOwner.Map
                    ? PostNodeHandoffSurfaceKind.MapSurfacePending
                    : hasExplicitSurface ? PostNodeHandoffSurfaceKind.Shop : PostNodeHandoffSurfaceKind.None,
            ContractMismatch: owner != NonCombatCanonicalForegroundOwner.Map
                              && hasExplicitSurface
                              && HasExporterMapForegroundClaim(observer)
                              && !mapExplicitOwner,
            MapOverlayVisible: mapOverlayState.ForegroundVisible,
            StaleBackgroundPresent: string.Equals(shopScene.BackgroundDebugKind, "map", StringComparison.OrdinalIgnoreCase));
    }

    private static PostNodeHandoffState BuildRestSitePostNodeHandoffState(
        ObserverSummary observer,
        RestSiteSceneState restSiteScene,
        MapOverlayState mapOverlayState,
        bool mapExplicitOwner)
    {
        var surfaceKind = restSiteScene.SmithUpgradeActive
            ? PostNodeHandoffSurfaceKind.RestSiteSmithUpgrade
            : restSiteScene.ProceedVisible
                ? PostNodeHandoffSurfaceKind.RestSiteProceed
                : restSiteScene.SelectionSettling
                    ? PostNodeHandoffSurfaceKind.RestSiteSelectionSettling
                    : restSiteScene.ReleaseStage == NonCombatReleaseStage.ReleasePending
                        ? PostNodeHandoffSurfaceKind.RestSiteReleasePending
                        : PostNodeHandoffSurfaceKind.RestSiteChoice;
        return new PostNodeHandoffState(
            restSiteScene.CanonicalForegroundOwner,
            restSiteScene.HandoffTarget,
            restSiteScene.ReleaseStage,
            HasExplicitSurface: restSiteScene.ReleaseStage == NonCombatReleaseStage.Active,
            surfaceKind,
            ContractMismatch: HasExporterMapForegroundClaim(observer) && !mapExplicitOwner,
            MapOverlayVisible: mapOverlayState.ForegroundVisible,
            StaleBackgroundPresent: restSiteScene.MapContextVisible || restSiteScene.AftermathResiduePresent);
    }

    private static PostNodeHandoffState BuildTreasurePostNodeHandoffState(
        ObserverSummary observer,
        TreasureSceneState treasureScene,
        MapOverlayState mapOverlayState,
        bool mapExplicitOwner)
    {
        return new PostNodeHandoffState(
            treasureScene.CanonicalForegroundOwner,
            treasureScene.HandoffTarget,
            treasureScene.ReleaseStage,
            HasExplicitSurface: true,
            PostNodeHandoffSurfaceKind.Treasure,
            ContractMismatch: HasExporterMapForegroundClaim(observer) && !mapExplicitOwner,
            MapOverlayVisible: mapOverlayState.ForegroundVisible,
            StaleBackgroundPresent: false);
    }

    private static PostNodeHandoffState BuildEventPostNodeHandoffState(
        ObserverSummary observer,
        EventSceneState eventScene,
        MapOverlayState mapOverlayState,
        bool mapExplicitOwner,
        bool mapReleaseSignal)
    {
        var owner = ((ICanonicalNonCombatSceneState)eventScene).CanonicalForegroundOwner;
        var releaseStage = ((ICanonicalNonCombatSceneState)eventScene).ReleaseStage;
        var handoffTarget = ((ICanonicalNonCombatSceneState)eventScene).HandoffTarget;
        var mapSurfaceOwned = owner == NonCombatCanonicalForegroundOwner.Map && mapExplicitOwner;
        var mapSurfacePending = owner == NonCombatCanonicalForegroundOwner.Map && !mapExplicitOwner;
        var hasExplicitSurface = eventScene.ExplicitAction != EventExplicitActionKind.None
                                 && owner == NonCombatCanonicalForegroundOwner.Event;
        var contractMismatch = eventScene.ExplicitAction == EventExplicitActionKind.AncientOptionContractMismatch
                               || (owner != NonCombatCanonicalForegroundOwner.Map
                                   && hasExplicitSurface
                                   && HasExporterMapForegroundClaim(observer)
                                   && !mapExplicitOwner)
                               || (owner == NonCombatCanonicalForegroundOwner.Map
                                   && hasExplicitSurface
                                   && !mapExplicitOwner
                                   && mapReleaseSignal);
        var surfaceKind = eventScene.ExplicitAction switch
        {
            EventExplicitActionKind.None when owner == NonCombatCanonicalForegroundOwner.Map && mapExplicitOwner && mapOverlayState.ForegroundVisible
                => PostNodeHandoffSurfaceKind.MapOverlay,
            EventExplicitActionKind.None when owner == NonCombatCanonicalForegroundOwner.Map && mapExplicitOwner
                => PostNodeHandoffSurfaceKind.MapNode,
            EventExplicitActionKind.None when owner == NonCombatCanonicalForegroundOwner.Map
                => PostNodeHandoffSurfaceKind.MapSurfacePending,
            EventExplicitActionKind.None => PostNodeHandoffSurfaceKind.None,
            EventExplicitActionKind.AncientOptionContractMismatch => PostNodeHandoffSurfaceKind.ContractMismatch,
            _ => PostNodeHandoffSurfaceKind.Event,
        };
        return new PostNodeHandoffState(
            owner,
            mapSurfaceOwned
                ? NonCombatHandoffTarget.ChooseFirstNode
                : mapSurfacePending
                    ? NonCombatHandoffTarget.WaitMap
                    : handoffTarget,
            mapSurfaceOwned
                ? NonCombatReleaseStage.Active
                : mapSurfacePending
                    ? NonCombatReleaseStage.ReleasePending
                    : releaseStage,
            hasExplicitSurface || mapSurfaceOwned,
            surfaceKind,
            contractMismatch,
            MapOverlayVisible: mapOverlayState.ForegroundVisible,
            StaleBackgroundPresent: eventScene.MapContextVisible
                                    || mapOverlayState.EventBackgroundPresent);
    }

    private static PostNodeHandoffState BuildMapPostNodeHandoffState(
        ObserverSummary observer,
        MapOverlayState mapOverlayState,
        bool mapExplicitOwner,
        bool staleBackgroundPresent)
    {
        return new PostNodeHandoffState(
            NonCombatCanonicalForegroundOwner.Map,
            mapExplicitOwner ? NonCombatHandoffTarget.ChooseFirstNode : NonCombatHandoffTarget.WaitMap,
            mapExplicitOwner ? NonCombatReleaseStage.Active : NonCombatReleaseStage.ReleasePending,
            mapExplicitOwner,
            mapExplicitOwner
                ? mapOverlayState.ForegroundVisible ? PostNodeHandoffSurfaceKind.MapOverlay : PostNodeHandoffSurfaceKind.MapNode
                : PostNodeHandoffSurfaceKind.MapSurfacePending,
            ContractMismatch: false,
            MapOverlayVisible: mapOverlayState.ForegroundVisible,
            StaleBackgroundPresent: staleBackgroundPresent);
    }

    private static PostNodeHandoffState? TryBuildCombatPostNodeHandoffState(
        ObserverSummary observer,
        MapOverlayState mapOverlayState,
        bool mapExplicitOwner)
    {
        if (!HasCombatTakeoverAuthority(observer))
        {
            return null;
        }

        var hasExplicitSurface = HasExplicitCombatTakeoverSurface(observer);
        var pending = IsCombatTakeoverPending(observer, hasExplicitSurface);
        return new PostNodeHandoffState(
            NonCombatCanonicalForegroundOwner.Combat,
            NonCombatHandoffTarget.HandleCombat,
            pending ? NonCombatReleaseStage.ReleasePending : NonCombatReleaseStage.Active,
            HasExplicitSurface: hasExplicitSurface && !pending,
            PostNodeHandoffSurfaceKind.CombatTakeover,
            ContractMismatch: HasExporterMapForegroundClaim(observer) && !mapExplicitOwner,
            MapOverlayVisible: mapOverlayState.ForegroundVisible,
            StaleBackgroundPresent: mapOverlayState.ForegroundVisible || HasObserverStaleRoomBackground(observer));
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

            return string.Equals(entry.Action, "event-resolved-map", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(entry.Action, "branch-map", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(entry.TargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(entry.TargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(entry.TargetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(entry.TargetLabel, "ancient event completion", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    internal static bool HasRecentMapClickAccepted(IReadOnlyList<GuiSmokeHistoryEntry>? history)
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
                || entry.Action.StartsWith("observer-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return string.Equals(entry.Action, "branch-map", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(entry.TargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(entry.TargetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(entry.TargetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(entry.TargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool HasPostNodeMapReleaseSignal(
        ObserverSummary observer,
        MapOverlayState mapOverlayState,
        bool mapExplicitOwner)
    {
        if (mapExplicitOwner)
        {
            return true;
        }

        return mapOverlayState.ForegroundVisible
               || TryGetMetaBool(observer, "mapCurrentActiveScreen") == true
               || IsMapScreenType(TryGetMetaValue(observer, "activeScreenType"))
               || MatchesControlFlowScreen(observer, "map");
    }

    private static bool HasCombatTakeoverAuthority(ObserverSummary observer)
    {
        return GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer)
               || (HasCombatForegroundSignal(observer)
                   && (HasCombatChoiceExtractor(observer)
                       || HasCombatLifecycleSignal(observer)));
    }

    private static bool HasCombatResolutionAuthority(ObserverSummary observer)
    {
        return observer.InCombat == true
               || MatchesControlFlowScreen(observer, "combat")
               || string.Equals(observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.VisibleScreen, "combat", StringComparison.OrdinalIgnoreCase)
               || (HasCombatChoiceExtractor(observer)
                   && TryGetMetaBool(observer, "combatPrimaryValue") == true);
    }

    private static bool HasExplicitCombatTakeoverSurface(ObserverSummary observer)
    {
        return observer.CombatHand.Count > 0
               || observer.CurrentChoices.Any(IsCombatTakeoverChoiceLabel)
               || observer.ActionNodes.Any(static node => node.Actionable && IsCombatTakeoverChoiceLabel(node.Label))
               || string.Equals(observer.ChoiceExtractorPath, "combat-targets", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCombatTakeoverPending(ObserverSummary observer, bool hasExplicitCombatSurface)
    {
        return !MatchesControlFlowScreen(observer, "combat")
               || TryGetMetaBool(observer, "transitionInProgress") == true
               || TryGetMetaBool(observer, "combatPlayerActionsDisabled") == true
               || TryGetMetaBool(observer, "combatEndingPlayerTurnPhaseOne") == true
               || TryGetMetaBool(observer, "combatEndingPlayerTurnPhaseTwo") == true
               || !hasExplicitCombatSurface;
    }

    private static bool HasCombatForegroundSignal(ObserverSummary observer)
    {
        return MatchesControlFlowScreen(observer, "combat")
               || string.Equals(observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.VisibleScreen, "combat", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.PublishedCurrentScreen, "combat", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.RawObservedScreen, "combat", StringComparison.OrdinalIgnoreCase)
               || IsCombatRoomType(TryGetMetaValue(observer, "activeScreenType"))
               || IsCombatRoomType(TryGetMetaValue(observer, "rawCurrentActiveScreenType"));
    }

    private static bool HasCombatChoiceExtractor(ObserverSummary observer)
    {
        return string.Equals(observer.ChoiceExtractorPath, "combat", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "combat-targets", StringComparison.OrdinalIgnoreCase)
               || string.Equals(TryGetMetaValue(observer, "choiceExtractorPath"), "combat", StringComparison.OrdinalIgnoreCase)
               || string.Equals(TryGetMetaValue(observer, "choiceExtractorPath"), "combat-targets", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasCombatLifecycleSignal(ObserverSummary observer)
    {
        return observer.LastEventsTail.Any(static tail =>
            tail.Contains("\"kind\":\"combat-started\"", StringComparison.OrdinalIgnoreCase)
            || tail.Contains("\"kind\":\"room-entered\"", StringComparison.OrdinalIgnoreCase)
            || tail.Contains("\"screen\":\"combat\"", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsCombatTakeoverChoiceLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        return CombatHistorySupport.IsCombatEndTurnLabel(label)
               || string.Equals(label, "DrawPile", StringComparison.OrdinalIgnoreCase)
               || string.Equals(label, "DiscardPile", StringComparison.OrdinalIgnoreCase)
               || string.Equals(label, "ExhaustPile", StringComparison.OrdinalIgnoreCase)
               || string.Equals(label, "핑", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasExplicitShopRoomEntrySurface(ObserverSummary observer, ShopSceneState? shopScene)
    {
        if (shopScene is not { ReleaseStage: NonCombatReleaseStage.Active })
        {
            return false;
        }

        return ShopObserverSignals.HasExplicitForegroundSurface(observer);
    }

    private static PostNodeHandoffState BuildPostNodeHandoffStateForNextRoomEntry(
        NextRoomEntryState nextRoomEntryState,
        ObserverState observer,
        string? screenshotPath)
    {
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(observer, null, screenshotPath);
        return BuildPostNodeHandoffStateForNextRoomEntry(nextRoomEntryState, observer.Summary, mapOverlayState);
    }

    private static PostNodeHandoffState BuildPostNodeHandoffStateForNextRoomEntry(
        NextRoomEntryState nextRoomEntryState,
        ObserverSummary observer,
        MapOverlayState mapOverlayState)
    {
        return new PostNodeHandoffState(
            nextRoomEntryState.Owner,
            nextRoomEntryState.HandoffTarget,
            nextRoomEntryState.ReleaseStage,
            nextRoomEntryState.ExplicitSurfacePresent,
            nextRoomEntryState.SurfaceKind,
            ContractMismatch: false,
            MapOverlayVisible: mapOverlayState.ForegroundVisible,
            StaleBackgroundPresent: nextRoomEntryState.CombatResiduePresent
                                    || nextRoomEntryState.RewardResiduePresent
                                    || nextRoomEntryState.MapOverlayResiduePresent
                                    || HasObserverStaleRoomBackground(observer));
    }

    private static bool HasExporterMapForegroundClaim(ObserverSummary observer)
    {
        return string.Equals(TryGetMetaValue(observer, "foregroundOwner"), "map", StringComparison.OrdinalIgnoreCase)
               || string.Equals(TryGetMetaValue(observer, "foregroundActionLane"), "map-node", StringComparison.OrdinalIgnoreCase)
               || TryGetMetaBool(observer, "mapReleaseAuthority") == true;
    }

    private static bool HasObserverStaleRoomBackground(ObserverSummary observer)
    {
        return EventProceedObserverSignals.HasEventChoiceAuthority(observer)
               || GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer)
               || GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer)
               || ShopObserverSignals.IsShopAuthorityActive(observer)
               || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer)
               || NonCombatForegroundOwnership.HasExplicitRestSiteForegroundAuthority(observer);
    }

    private static bool? TryGetMetaBool(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value)
               && bool.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    private static string? TryGetMetaValue(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value) ? value : null;
    }

    private static bool IsMapScreenType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NMapScreen", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCombatRoomType(string? typeName)
    {
        return !string.IsNullOrWhiteSpace(typeName)
               && typeName.Contains("NCombatRoom", StringComparison.OrdinalIgnoreCase);
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

    private static bool IsCurrentRewardProgressionChoice(
        ObserverChoice choice,
        WindowBounds? windowBounds,
        RewardScreenState? rewardState)
    {
        return IsExplicitRewardProgressionChoice(choice, rewardState)
               && HasActiveRewardBounds(choice.ScreenBounds, windowBounds);
    }

    private static IReadOnlyList<ObserverChoice> GetClaimableRewardProgressionChoices(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        RewardScreenState? rewardState)
    {
        return observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, windowBounds, rewardState))
            .Where(choice => IsClaimableRewardProgressionChoice(choice, rewardState))
            .ToArray();
    }

    private static IReadOnlyList<ObserverActionNode> GetClaimableRewardProgressionNodes(
        ObserverSummary observer,
        WindowBounds? windowBounds,
        RewardScreenState? rewardState)
    {
        var activeRewardChoices = observer.Choices
            .Where(choice => IsCurrentRewardProgressionChoice(choice, windowBounds, rewardState))
            .ToArray();
        return observer.ActionNodes
            .Where(node => IsCurrentRewardProgressionNode(node, windowBounds, rewardState, activeRewardChoices))
            .Where(node => IsClaimableRewardProgressionNode(node, rewardState, activeRewardChoices))
            .ToArray();
    }

    private static bool IsCurrentRewardProgressionNode(
        ObserverActionNode node,
        WindowBounds? windowBounds,
        RewardScreenState? rewardState,
        IReadOnlyList<ObserverChoice> activeRewardChoices)
    {
        return IsExplicitRewardProgressionNode(node, rewardState)
               && HasActiveRewardBounds(node.ScreenBounds, windowBounds)
               && HasCurrentRewardNodeAuthority(node, rewardState, activeRewardChoices);
    }

    private static bool IsClaimableRewardProgressionChoice(ObserverChoice choice, RewardScreenState? rewardState)
    {
        if (IsSkipOrProceedLabel(choice.Label))
        {
            return false;
        }

        if (IsRewardCardChoice(choice))
        {
            return false;
        }

        return !IsPotionRewardChoice(choice) || AllowsPotionRewardClaim(rewardState);
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

        if (IsCardRewardNode(node, activeRewardChoices))
        {
            return false;
        }

        return !IsPotionRewardNode(node, activeRewardChoices) || AllowsPotionRewardClaim(rewardState);
    }

    private static bool HasCurrentRewardNodeAuthority(
        ObserverActionNode node,
        RewardScreenState? rewardState,
        IReadOnlyList<ObserverChoice> activeRewardChoices)
    {
        if (IsProceedNode(node)
            || node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
            || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (activeRewardChoices.Any(choice =>
                string.Equals(choice.Label, node.Label, StringComparison.OrdinalIgnoreCase)
                && RewardBoundsLookEquivalent(choice.ScreenBounds, node.ScreenBounds)))
        {
            return true;
        }

        if (IsRewardOwnedExplicitRewardNode(node, rewardState))
        {
            return true;
        }

        if (rewardState?.RewardIsCurrentActiveScreen == true || rewardState?.ForegroundOwned == true)
        {
            return node.SemanticHints.Any(static hint =>
                string.Equals(hint, "scene:rewards", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "scene-published:rewards", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "scene-raw:rewards", StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    private static bool AllowsPotionRewardClaim(RewardScreenState? rewardState)
    {
        // AutoSlay only treats potion rewards as claimable when player state explicitly confirms
        // an open potion slot. Unknown slot capacity is not strong enough to reopen claim loops.
        return rewardState?.HasOpenPotionSlots == true;
    }

    private static bool IsStaleRewardProgressionChoice(
        ObserverChoice choice,
        WindowBounds? windowBounds,
        RewardScreenState? rewardState)
    {
        return IsExplicitRewardProgressionChoice(choice, rewardState)
               && !HasActiveRewardBounds(choice.ScreenBounds, windowBounds);
    }

    private static bool IsStaleRewardProgressionNode(
        ObserverActionNode node,
        WindowBounds? windowBounds,
        RewardScreenState? rewardState)
    {
        return IsExplicitRewardProgressionNode(node, rewardState)
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

    private static bool IsExplicitRewardProgressionChoice(ObserverChoice choice, RewardScreenState? rewardState)
    {
        var rewardOwnedRelicChoice = IsRewardOwnedExplicitRelicChoice(choice, rewardState);
        if (!TryParseNodeBounds(choice.ScreenBounds, out _)
            || IsOverlayChoiceLabel(choice.Label)
            || (IsInspectPreviewChoice(choice) && !rewardOwnedRelicChoice)
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

    private static bool IsExplicitRewardProgressionNode(ObserverActionNode node, RewardScreenState? rewardState)
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
               || IsRewardOwnedExplicitRewardNode(node, rewardState)
               || node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasRewardForegroundClaimAuthority(RewardScreenState? rewardState)
    {
        return rewardState?.RewardIsCurrentActiveScreen == true
               || rewardState?.ForegroundOwned == true;
    }

    private static bool IsRewardOwnedExplicitRelicChoice(ObserverChoice choice, RewardScreenState? rewardState)
    {
        return HasRewardForegroundClaimAuthority(rewardState)
               && !HasConflictingNonRewardSceneHint(choice.SemanticHints)
               && ((string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(choice.BindingId, "RelicReward", StringComparison.OrdinalIgnoreCase))
                   || choice.SemanticHints.Any(static hint =>
                       string.Equals(hint, "reward-relic", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(hint, "reward-type:RelicReward", StringComparison.OrdinalIgnoreCase)));
    }

    private static bool IsRewardOwnedExplicitRewardNode(ObserverActionNode node, RewardScreenState? rewardState)
    {
        return HasRewardForegroundClaimAuthority(rewardState)
               && !HasConflictingNonRewardSceneHint(node.SemanticHints)
               && (string.Equals(node.TypeName, "reward-type", StringComparison.OrdinalIgnoreCase)
                   || node.SemanticHints.Any(static hint =>
                       hint.StartsWith("reward-type:", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(hint, "reward-card", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(hint, "reward-pick", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(hint, "reward-relic", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(hint, "reward-gold", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(hint, "reward-potion", StringComparison.OrdinalIgnoreCase)));
    }

    private static bool HasConflictingNonRewardSceneHint(IReadOnlyList<string> semanticHints)
    {
        return semanticHints.Any(static hint =>
            (hint.StartsWith("scene:", StringComparison.OrdinalIgnoreCase)
             || hint.StartsWith("scene-published:", StringComparison.OrdinalIgnoreCase)
             || hint.StartsWith("scene-raw:", StringComparison.OrdinalIgnoreCase))
            && !hint.Contains("reward", StringComparison.OrdinalIgnoreCase));
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

    private static bool IsGoldRewardNode(ObserverActionNode node, IReadOnlyList<ObserverChoice> activeRewardChoices)
    {
        if (string.Equals(node.TypeName, "gold", StringComparison.OrdinalIgnoreCase)
            || ContainsAny(node.Label, "골드", "gold")
            || node.SemanticHints.Any(static hint => string.Equals(hint, "reward-gold", StringComparison.OrdinalIgnoreCase)
                                                     || string.Equals(hint, "reward-type:GoldReward", StringComparison.OrdinalIgnoreCase)
                                                     || string.Equals(hint, "raw-kind:gold", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return activeRewardChoices.Any(choice =>
            IsGoldRewardChoice(choice)
            && string.Equals(choice.Label, node.Label, StringComparison.OrdinalIgnoreCase)
            && RewardBoundsLookEquivalent(choice.ScreenBounds, node.ScreenBounds));
    }

    private static bool IsRelicRewardNode(ObserverActionNode node, IReadOnlyList<ObserverChoice> activeRewardChoices)
    {
        if (string.Equals(node.TypeName, "relic", StringComparison.OrdinalIgnoreCase)
            || node.SemanticHints.Any(static hint => string.Equals(hint, "reward-relic", StringComparison.OrdinalIgnoreCase)
                                                     || string.Equals(hint, "reward-type:RelicReward", StringComparison.OrdinalIgnoreCase)
                                                     || string.Equals(hint, "raw-kind:relic", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return activeRewardChoices.Any(choice =>
            IsRelicRewardChoice(choice)
            && string.Equals(choice.Label, node.Label, StringComparison.OrdinalIgnoreCase)
            && RewardBoundsLookEquivalent(choice.ScreenBounds, node.ScreenBounds));
    }

    private static bool IsCardRewardNode(ObserverActionNode node, IReadOnlyList<ObserverChoice> activeRewardChoices)
    {
        if (string.Equals(node.TypeName, "card", StringComparison.OrdinalIgnoreCase)
            || node.SemanticHints.Any(static hint => string.Equals(hint, "reward-card", StringComparison.OrdinalIgnoreCase)
                                                     || string.Equals(hint, "reward-pick", StringComparison.OrdinalIgnoreCase)
                                                     || string.Equals(hint, "reward-type:CardReward", StringComparison.OrdinalIgnoreCase)
                                                     || string.Equals(hint, "raw-kind:card", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return activeRewardChoices.Any(choice =>
            IsRewardCardChoice(choice)
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

    private static bool IsRewardCardChoice(ObserverChoice choice, ObserverSummary? observer = null)
    {
        var explicitRewardCardBinding = string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                                        && string.Equals(choice.BindingId, "CardReward", StringComparison.OrdinalIgnoreCase);
        var explicitRewardCardHint = choice.SemanticHints.Any(static hint => string.Equals(hint, "reward-card", StringComparison.OrdinalIgnoreCase)
                                                                             || string.Equals(hint, "reward-pick", StringComparison.OrdinalIgnoreCase)
                                                                             || string.Equals(hint, "reward-type:CardReward", StringComparison.OrdinalIgnoreCase));
        var explicitRewardCardAuthority = explicitRewardCardBinding
                                          || explicitRewardCardHint
                                          || string.Equals(choice.Value, "CardReward", StringComparison.OrdinalIgnoreCase)
                                          || ContainsAny(choice.Description, "카드 보상", "reward")
                                          || choice.SemanticHints.Any(static hint =>
                                              string.Equals(hint, "scene:rewards", StringComparison.OrdinalIgnoreCase)
                                              || string.Equals(hint, "scene-published:rewards", StringComparison.OrdinalIgnoreCase)
                                              || string.Equals(hint, "scene-raw:rewards", StringComparison.OrdinalIgnoreCase));
        var companionRewardAuthority = observer is not null && HasRewardCardCompanionAuthority(observer);
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
                || explicitRewardCardBinding
                || explicitRewardCardHint)
               && (!IsSkipOrProceedLabel(choice.Label) || explicitRewardCardBinding || explicitRewardCardHint)
               && (!IsConfirmLikeLabel(choice.Label) || explicitRewardCardBinding || explicitRewardCardHint)
               && !IsDismissLikeLabel(choice.Label)
               && !IsOverlayChoiceLabel(choice.Label)
               && (HasRewardCardLikeBounds(choice.ScreenBounds) || explicitRewardCardAuthority || companionRewardAuthority);
    }

    private static bool HasRewardCardLikeBounds(string? screenBounds)
    {
        if (!TryParseNodeBounds(screenBounds, out var bounds))
        {
            return false;
        }

        return bounds.Width >= 120f || bounds.Height >= 150f;
    }

    private static bool HasRewardCardCompanionAuthority(ObserverSummary observer)
    {
        return observer.Choices.Any(static choice =>
                   IsRelicRewardChoice(choice)
                   || IsGoldRewardChoice(choice)
                   || IsPotionRewardChoice(choice)
                   || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                   || choice.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
                   || choice.SemanticHints.Any(static hint =>
                       hint.StartsWith("reward-type:", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(hint, "reward", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(hint, "reward-card", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(hint, "reward-pick", StringComparison.OrdinalIgnoreCase)))
               || observer.ActionNodes.Any(static node =>
                   node.Actionable
                   && (string.Equals(node.TypeName, "reward-type", StringComparison.OrdinalIgnoreCase)
                       || node.Kind.Contains("reward", StringComparison.OrdinalIgnoreCase)
                       || node.NodeId.Contains("reward", StringComparison.OrdinalIgnoreCase)
                       || node.SemanticHints.Any(static hint =>
                           hint.StartsWith("reward-type:", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(hint, "reward", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(hint, "reward-card", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(hint, "reward-pick", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(hint, "reward-relic", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(hint, "reward-gold", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(hint, "reward-potion", StringComparison.OrdinalIgnoreCase))));
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

        if (GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer) && observer.Choices.Any(choice => IsRewardCardChoice(choice, observer)))
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

        if (GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer) && observer.Choices.Any(choice => IsRewardCardChoice(choice, observer)))
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
