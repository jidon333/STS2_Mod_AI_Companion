using System.Drawing;
using System.Text.Json;
using static ObserverScreenProvenance;

sealed partial class AutoDecisionProvider
{
    private static GuiSmokeDecisionAnalysis AnalyzeGenericPhase(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision? actualDecision,
        Func<GuiSmokeStepDecision> factory,
        string? foregroundKind,
        string? backgroundKind)
    {
        var builder = new DecisionAnalysisBuilder(request, foregroundKind, backgroundKind);
        var predictedDecision = factory();
        builder.Consider(
            predictedDecision.TargetLabel ?? predictedDecision.ActionKind ?? predictedDecision.Status,
            "phase-generic",
            predictedDecision.Confidence ?? 0.50d,
            () => predictedDecision,
            "generic-phase-candidate-unavailable",
            boundsSource: predictedDecision.ActionKind is "click" or "right-click" ? "decision-normalized" : "decision-nonpoint");
        return builder.Build(CreateWaitDecision("waiting for passive phase", DisplayControlFlowScreen(request.Observer)), actualDecision);
    }

    private static GuiSmokeDecisionAnalysis AnalyzeHandleCombat(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision? actualDecision,
        GuiSmokeStepAnalysisContext analysisContext)
    {
        var handoffState = analysisContext.CombatResolutionHandoffState;
        var (foregroundKind, backgroundKind) = DescribeCombatResolutionForegroundBackground(request, handoffState);
        if (!HasReleasedCombatOwnership(handoffState))
        {
            return AnalyzeGenericPhase(request, actualDecision, () => DecideHandleCombat(request, analysisContext), foregroundKind, backgroundKind);
        }

        var builder = new DecisionAnalysisBuilder(request, foregroundKind, backgroundKind);
        builder.AddSuppressed("select attack slot", "combat-ownership-released-to-noncombat-handoff");
        builder.AddSuppressed("select non-enemy slot", "combat-ownership-released-to-noncombat-handoff");
        builder.AddSuppressed("click enemy", "combat-ownership-released-to-noncombat-handoff");
        builder.AddSuppressed("click end turn", "combat-ownership-released-to-noncombat-handoff");
        var releaseDecision = DecideHandleCombat(request, analysisContext);
        builder.Consider(
            releaseDecision.TargetLabel ?? releaseDecision.ActionKind ?? releaseDecision.Status,
            string.Equals(releaseDecision.Status, "abort", StringComparison.OrdinalIgnoreCase)
                ? "combat-release-contract-mismatch"
                : "combat-release-handoff",
            releaseDecision.Confidence ?? 0.99d,
            () => releaseDecision,
            "combat-release-handoff-unavailable");
        return builder.Build(releaseDecision, actualDecision);
    }

    private static GuiSmokeDecisionAnalysis AnalyzeHandleRewards(GuiSmokeStepRequest request, GuiSmokeStepDecision? actualDecision, GuiSmokeStepAnalysisContext analysisContext)
    {
        var canonicalScene = TryBuildCanonicalNonCombatSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath);
        if (canonicalScene is ShopSceneState { ReleaseStage: NonCombatReleaseStage.Active })
        {
            var shopRequest = request with { Phase = GuiSmokePhase.HandleShop.ToString() };
            return AnalyzeGenericPhase(shopRequest, actualDecision, () => DecideHandleShop(shopRequest), "shop", null);
        }

        if (canonicalScene is RestSiteSceneState { ReleaseStage: NonCombatReleaseStage.Active })
        {
            return AnalyzeChooseFirstNode(request with { Phase = GuiSmokePhase.ChooseFirstNode.ToString() }, actualDecision, analysisContext);
        }

        var rewardScene = analysisContext.RewardScene;
        var rewardMapLayer = rewardScene.LayerState;
        var rewardState = rewardScene.ScreenState;
        var (foregroundKind, backgroundKind) = DescribeForegroundBackground(request);
        var builder = new DecisionAnalysisBuilder(request, foregroundKind, backgroundKind);

        var overlayDecision = TryCreateRoomOverlayCleanupDecision(request);
        builder.Consider("click inspect overlay close", "overlay-cleanup", 1.00d, () => overlayDecision, "no-room-overlay-cleanup", boundsSource: "overlay-cleanup");

        var cardSelectionDecision = TryCreateCardSelectionDecision(request);
        builder.Consider(
            ToCandidateLabel(cardSelectionDecision, "click card-selection subtype"),
            "card-selection-runtime",
            0.98d,
            () => cardSelectionDecision,
            "no-card-selection-subtype-decision");
        if (CardSelectionObserverSignals.IsCardSelectionState(request.Observer))
        {
            builder.AddSuppressed("click reward choice", "card-selection-subtype-foreground-suppresses-reward-row-reuse");
            builder.AddSuppressed("click reward back", "card-selection-subtype-foreground-suppresses-reward-back-nav");
            builder.AddSuppressed("click first reachable node", "card-selection-subtype-foreground-suppresses-map-fallback");
            builder.AddSuppressed("click visible map advance", "card-selection-subtype-foreground-suppresses-map-fallback");
            return builder.Build(CreateWaitDecision("waiting for card-selection subtype progression", DisplayControlFlowScreen(request.Observer)), actualDecision);
        }

        var explicitRewardDecision = TryCreateExplicitRewardResolutionDecision(request, rewardScene);
        builder.Consider(
            ToCandidateLabel(explicitRewardDecision, "click reward choice"),
            "reward-explicit",
            0.95d,
            () => explicitRewardDecision,
            "no-explicit-reward-progression",
            rawBounds: TryFindRewardBounds(request),
            boundsSource: "observer-reward");

        var rewardBackDecision = TryCreateRewardBackNavigationDecision(request, rewardScene);
        builder.Consider("click reward back", "reward-back-nav", 0.88d, () => rewardBackDecision, "reward-back-not-available", rawBounds: TryFindBackBounds(request), boundsSource: "observer-back");

        if (rewardState is { TerminalRunBoundary: true })
        {
            builder.AddSuppressed("click first reachable node", "terminal-boundary-suppresses-gameplay-map-fallback");
            builder.AddSuppressed("click visible map advance", "terminal-boundary-suppresses-gameplay-map-fallback");
        }
        else if (rewardScene.ReleaseToMapPending
                 || (!rewardScene.RewardForegroundOwned
                     && GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(request.Observer)))
        {
            builder.AddSuppressed("click first reachable node", "reward-ownership-release-defers-map-routing-to-waitmap-reconciliation");
            builder.AddSuppressed("click visible map advance", "reward-ownership-release-defers-map-routing-to-waitmap-reconciliation");
            return builder.Build(CreateRewardOwnershipReleaseWaitDecision(request), actualDecision);
        }
        else
        {
            builder.AddSuppressed("click first reachable node", "reward-owner-active-preserves-room-lane");
            builder.AddSuppressed("click visible map advance", "reward-owner-active-preserves-room-lane");
        }

        return builder.Build(CreateWaitDecision("waiting for reward actions", DisplayControlFlowScreen(request.Observer)), actualDecision);
    }

    private static GuiSmokeDecisionAnalysis AnalyzeChooseFirstNode(GuiSmokeStepRequest request, GuiSmokeStepDecision? actualDecision, GuiSmokeStepAnalysisContext analysisContext)
    {
        var handoffState = analysisContext.PostNodeHandoffState;
        var chooseFirstNodeLane = GuiSmokeChooseFirstNodeLaneSupport.Resolve(
            new ObserverState(request.Observer, null, null, request.Observer.LastEventsTail?.ToArray() ?? Array.Empty<string>()),
            analysisContext.WindowBounds,
            request.ScreenshotPath,
            request.History,
            analysisContext);
        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.RewardForeground)
        {
            return AnalyzeHandleRewards(request with { Phase = GuiSmokePhase.HandleRewards.ToString() }, actualDecision, analysisContext);
        }

        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.EventForeground)
        {
            return AnalyzeHandleEvent(request with { Phase = GuiSmokePhase.HandleEvent.ToString() }, actualDecision, analysisContext);
        }

        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.CombatTakeover)
        {
            if (handoffState.HasExplicitSurface)
            {
                var combatRequest = request with { Phase = GuiSmokePhase.HandleCombat.ToString() };
                return AnalyzeGenericPhase(combatRequest, actualDecision, () => DecideHandleCombat(combatRequest, analysisContext), "combat", null);
            }

            var (combatForegroundKind, combatBackgroundKind) = DescribeForegroundBackground(request);
            var combatBuilder = new DecisionAnalysisBuilder(request, combatForegroundKind, combatBackgroundKind);
            combatBuilder.AddSuppressed("click exported reachable node", "combat-takeover-family-suppresses-map-routing");
            combatBuilder.AddSuppressed("click first reachable node", "combat-takeover-family-suppresses-map-routing");
            combatBuilder.AddSuppressed("click visible map advance", "combat-takeover-family-suppresses-map-routing");
            combatBuilder.AddSuppressed("click map back", "combat-takeover-family-suppresses-map-routing");
            return combatBuilder.Build(
                CreateWaitDecision("waiting for combat takeover handoff to publish the first combat surface", DisplayControlFlowScreen(request.Observer)),
                actualDecision);
        }

        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.ShopRoom)
        {
            var shopRequest = request with { Phase = GuiSmokePhase.HandleShop.ToString() };
            return AnalyzeGenericPhase(shopRequest, actualDecision, () => DecideHandleShop(shopRequest), "shop", null);
        }

        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.ContractMismatch)
        {
            var (mismatchForegroundKind, mismatchBackgroundKind) = DescribeForegroundBackground(request);
            var mismatchBuilder = new DecisionAnalysisBuilder(request, mismatchForegroundKind, mismatchBackgroundKind);
            var mismatchDecision = CreatePostNodeHandoffContractMismatchAbortDecision(
                request,
                $"post-node handoff contract mismatch: owner={handoffState.Owner} target={handoffState.HandoffTarget} surface={handoffState.SurfaceKind}");
            mismatchBuilder.Consider(
                "abort handoff contract mismatch",
                "post-node-handoff-contract",
                1.00d,
                () => mismatchDecision,
                "no-post-node-handoff-contract-mismatch");
            return mismatchBuilder.Build(mismatchDecision, actualDecision);
        }

        var mapOverlayState = analysisContext.MapOverlayState;
        var (foregroundKind, backgroundKind) = DescribeForegroundBackground(request);
        var builder = new DecisionAnalysisBuilder(request, foregroundKind, backgroundKind);

        if (GuiSmokeNonCombatContractSupport.HasExplicitRestSiteChoiceAuthority(request))
        {
            builder.AddSuppressed("click exported reachable node", "rest-site-owner-active-preserves-room-lane");
            builder.AddSuppressed("click first reachable node", "rest-site-owner-active-preserves-room-lane");
            builder.AddSuppressed("click visible map advance", "rest-site-explicit-choices-suppress-current-node-arrow");
            builder.AddSuppressed("click map back", "rest-site-explicit-choices-are-the-progression-lane");
            foreach (var restSiteCandidate in BuildRestSiteDecisionCandidates(request))
            {
                var capturedCandidate = restSiteCandidate;
                builder.Consider(
                    capturedCandidate.Label,
                    capturedCandidate.Source,
                    capturedCandidate.Score,
                    () => capturedCandidate.Decision,
                    capturedCandidate.RejectReason ?? "no-rest-site-explicit-choice",
                    rawBounds: capturedCandidate.RawBounds,
                    boundsSource: capturedCandidate.BoundsSource);
            }

            builder.Consider(
                ToCandidateLabel(TryCreateRestSiteUpgradeDecision(request), "click smith card"),
                "rest-site-upgrade",
                0.92d,
                () => TryCreateRestSiteUpgradeDecision(request),
                "rest-site-upgrade-grid-not-visible");
            return builder.Build(CreateWaitDecision("waiting for an explicit rest-site choice", DisplayControlFlowScreen(request.Observer)), actualDecision);
        }

        if (GuiSmokeNonCombatContractSupport.LooksLikeRestSiteState(request.Observer)
            && TryCreateRestSiteUpgradeDecision(request) is { } observerUpgradeDecision)
        {
            builder.AddSuppressed("click exported reachable node", "rest-site-owner-active-preserves-room-lane");
            builder.AddSuppressed("click first reachable node", "rest-site-owner-active-preserves-room-lane");
            builder.AddSuppressed("click visible map advance", "rest-site-upgrade-suppresses-current-node-arrow");
            builder.AddSuppressed("click map back", "rest-site-upgrade-remains-foreground-authority");
            builder.Consider(
                ToCandidateLabel(observerUpgradeDecision, "click smith card"),
                "rest-site-upgrade",
                observerUpgradeDecision.TargetLabel?.Contains("confirm", StringComparison.OrdinalIgnoreCase) == true ? 0.95d : 0.92d,
                () => observerUpgradeDecision,
                "rest-site-upgrade-grid-not-visible",
                rawBounds: TryFindRestSiteUpgradeBounds(request, observerUpgradeDecision),
                boundsSource: TryResolveRestSiteUpgradeBoundsSource(request, observerUpgradeDecision));
            return builder.Build(CreateWaitDecision("waiting for smith grid or confirm state", DisplayControlFlowScreen(request.Observer)), actualDecision);
        }

        if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(request.Observer)
            && GuiSmokeNonCombatContractSupport.HasRecentRestSiteExplicitClick(request.History, "rest site: smith"))
        {
            builder.AddSuppressed("click exported reachable node", "rest-site-upgrade-observer-state-suppresses-map-routing");
            builder.AddSuppressed("click first reachable node", "rest-site-upgrade-observer-state-suppresses-map-routing");
            builder.AddSuppressed("click visible map advance", "rest-site-upgrade-observer-state-suppresses-current-node-arrow");
            builder.AddSuppressed("click map back", "rest-site-upgrade-observer-state-remains-foreground-authority");
            var observerMissDecision = CreateRestSitePostClickNoOpWaitDecision(request, "rest site: smith", DisplayControlFlowScreen(request.Observer));
            builder.Consider(
                "click smith card",
                "rest-site-upgrade",
                0.92d,
                () => TryCreateRestSiteUpgradeDecision(request),
                "rest-site-upgrade-grid-not-visible");
            builder.Consider(
                "wait",
                "rest-site-upgrade-observer",
                0.97d,
                () => observerMissDecision,
                "rest-site-upgrade-post-click-state-unavailable");
            return builder.Build(observerMissDecision, actualDecision);
        }

        if (BuildRestSiteSceneState(request.Observer) is
            {
                ProceedVisible: true,
                ExplicitChoiceVisible: false,
                SmithUpgradeActive: false,
            })
        {
            builder.AddSuppressed("click exported reachable node", "rest-site-owner-active-preserves-room-lane");
            builder.AddSuppressed("click first reachable node", "rest-site-owner-active-preserves-room-lane");
            builder.AddSuppressed("click visible map advance", "rest-site-owner-active-suppresses-map-arrow-contamination");
            builder.AddSuppressed("click map back", "rest-site-owner-active-preserves-room-lane");
            builder.Consider(
                "click proceed",
                "rest-site-proceed",
                0.96d,
                () => TryCreateRestSiteProceedDecision(request),
                "rest-site-proceed-not-visible",
                rawBounds: TryFindProceedBounds(request),
                boundsSource: "observer-proceed");
            return builder.Build(CreateWaitDecision("waiting for explicit rest-site proceed", DisplayControlFlowScreen(request.Observer)), actualDecision);
        }

        if (BuildRestSiteSceneState(request.Observer) is
            {
                SelectionSettling: true,
                ProceedVisible: false,
                SmithUpgradeActive: false,
            })
        {
            builder.AddSuppressed("click exported reachable node", "rest-site-selection-settling-preserves-room-lane");
            builder.AddSuppressed("click first reachable node", "rest-site-selection-settling-preserves-room-lane");
            builder.AddSuppressed("click visible map advance", "rest-site-selection-settling-suppresses-map-arrow-contamination");
            builder.AddSuppressed("click map back", "rest-site-selection-settling-preserves-room-lane");
            return builder.Build(
                CreateWaitDecision("waiting for rest-site selection to settle into smith upgrade or proceed", DisplayControlFlowScreen(request.Observer)),
                actualDecision);
        }

        if (TreasureRoomObserverSignals.IsTreasureAuthorityActive(request.Observer))
        {
            builder.AddSuppressed("click exported reachable node", "treasure-room-owner-active-preserves-room-lane");
            builder.AddSuppressed("click first reachable node", "treasure-room-owner-active-preserves-room-lane");
            builder.AddSuppressed("click visible map advance", "treasure-room-explicit-affordances-suppress-current-node-arrow");
            builder.AddSuppressed("click map back", "treasure-room-progression-is-chest-holder-proceed");
            var treasureDecision = TryCreateTreasureRoomDecision(request);
            builder.Consider(
                ToCandidateLabel(treasureDecision, "click treasure room action"),
                "treasure-room-runtime",
                0.98d,
                () => treasureDecision,
                "no-treasure-room-actionable-affordance");
            return builder.Build(CreateWaitDecision("waiting for treasure room progression", DisplayControlFlowScreen(request.Observer)), actualDecision);
        }

        if (chooseFirstNodeLane == GuiSmokeChooseFirstNodeLane.MapSurfacePending)
        {
            builder.AddSuppressed("click exported reachable node", "map-surface-pending-without-exported-node");
            builder.AddSuppressed("click first reachable node", "map-surface-pending-without-exported-node");
            builder.AddSuppressed("click visible map advance", "map-surface-pending-suppresses-arrow-fallback");
            builder.AddSuppressed("click map back", "map-surface-pending-without-overlay-back-nav");
            return builder.Build(
                CreateWaitDecision("waiting for post-node map surface to republish after room release", DisplayControlFlowScreen(request.Observer)),
                actualDecision);
        }

        if (mapOverlayState.ForegroundVisible)
        {
            if (mapOverlayState.StaleEventChoicePresent)
            {
                builder.AddSuppressed("click event choice", "map-overlay-foreground-removes-stale-event-choice");
            }

            if (!GuiSmokeNonCombatContractSupport.AllowsAnyMapRoutingAction(request))
            {
                builder.AddSuppressed("click exported reachable node", "request-allowlist-excludes-map-routing");
                builder.AddSuppressed("click map back", "request-allowlist-excludes-map-routing");
                return builder.Build(
                    GuiSmokeNonCombatContractSupport.CreateMapRoutingContractWaitDecision(
                        request,
                        "map overlay is visible but request allowlist does not permit map routing; waiting for exporter/phase reconciliation"),
                    actualDecision);
            }

            builder.Consider(
                "click exported reachable node",
                "observer-export:map-node",
                1.00d,
                () => TryCreateExportedReachableMapPointDecision(request),
                "no-exported-reachable-node",
                rawBounds: TryFindMapNodeBounds(request),
                boundsSource: "observer-map-node");
            builder.Consider(
                "click map back",
                "overlay-back-navigation",
                0.90d,
                () => TryCreateMapBackNavigationDecision(request),
                "map-back-navigation-unavailable",
                rawBounds: TryFindBackBounds(request),
                boundsSource: "overlay-back");
            return builder.Build(CreateWaitDecision("waiting for exported reachable map node", DisplayControlFlowScreen(request.Observer)), actualDecision);
        }

        builder.Consider(
            "click exported reachable node",
            "observer-export:map-node",
            0.96d,
            () => TryCreateExportedReachableMapPointDecision(request),
            "no-exported-reachable-node",
            rawBounds: TryFindMapNodeBounds(request),
            boundsSource: "observer-map-node");
        if (handoffState.ContractMismatch)
        {
            var mismatchDecision = CreatePostNodeHandoffContractMismatchAbortDecision(
                request,
                $"post-node handoff contract mismatch: owner={handoffState.Owner} target={handoffState.HandoffTarget} surface={handoffState.SurfaceKind}");
            builder.Consider(
                "abort handoff contract mismatch",
                "post-node-handoff-contract",
                0.99d,
                () => mismatchDecision,
                "no-post-node-handoff-contract-mismatch");
            return builder.Build(mismatchDecision, actualDecision);
        }

        return builder.Build(CreateWaitDecision("waiting for reachable map node", DisplayControlFlowScreen(request.Observer)), actualDecision);
    }

    private static (string? ForegroundKind, string? BackgroundKind) DescribeCombatResolutionForegroundBackground(
        GuiSmokeStepRequest request,
        PostNodeHandoffState handoffState)
    {
        var foregroundKind = handoffState.Owner switch
        {
            NonCombatCanonicalForegroundOwner.Event when handoffState.ReleaseStage == NonCombatReleaseStage.ReleasePending
                => "event-release-pending",
            NonCombatCanonicalForegroundOwner.Event => "event",
            NonCombatCanonicalForegroundOwner.Reward => "reward",
            NonCombatCanonicalForegroundOwner.Shop => "shop",
            NonCombatCanonicalForegroundOwner.RestSite => "rest-site",
            NonCombatCanonicalForegroundOwner.Treasure => "treasure",
            NonCombatCanonicalForegroundOwner.Map when handoffState.SurfaceKind == PostNodeHandoffSurfaceKind.MapSurfacePending
                => "map-surface-pending",
            NonCombatCanonicalForegroundOwner.Map => "map",
            NonCombatCanonicalForegroundOwner.Combat when handoffState.SurfaceKind == PostNodeHandoffSurfaceKind.CombatTakeover
                => "combat-takeover",
            NonCombatCanonicalForegroundOwner.Combat => "combat",
            _ => DisplayControlFlowScreen(request.Observer),
        };
        var backgroundKind = handoffState.MapOverlayVisible && handoffState.Owner != NonCombatCanonicalForegroundOwner.Map
            ? "map"
            : null;
        return (foregroundKind, backgroundKind);
    }

    private static GuiSmokeDecisionAnalysis AnalyzeHandleEvent(GuiSmokeStepRequest request, GuiSmokeStepDecision? actualDecision, GuiSmokeStepAnalysisContext analysisContext)
    {
        var eventScene = analysisContext.EventScene;
        var ancientContract = eventScene.AncientContract;
        if (eventScene.RewardSubstateActive)
        {
            return AnalyzeHandleRewards(request with { Phase = GuiSmokePhase.HandleRewards.ToString() }, actualDecision, analysisContext);
        }

        var forceEventProgressionAfterCardSelection = eventScene.ForceProgressionAfterCardSelection;
        var eventForegroundActive = eventScene.EventForegroundOwned
                                    && eventScene.ReleaseStage == EventReleaseStage.Active;
        var mapOverlayState = eventScene.MapOverlayState;
        var strongEventForegroundChoice = eventScene.StrongForegroundChoice || forceEventProgressionAfterCardSelection;
        var (foregroundKind, backgroundKind) = DescribeForegroundBackground(request);
        var builder = new DecisionAnalysisBuilder(request, foregroundKind, backgroundKind);

        var overlayDecision = TryCreateRoomOverlayCleanupDecision(request);
        builder.Consider("click inspect overlay close", "overlay-cleanup", 1.00d, () => overlayDecision, "no-room-overlay-cleanup", boundsSource: "overlay-cleanup");

        var cardSelectionDecision = TryCreateCardSelectionDecision(request);
        builder.Consider(
            ToCandidateLabel(cardSelectionDecision, "click card-selection subtype"),
            "card-selection-runtime",
            0.99d,
            () => cardSelectionDecision,
            "no-card-selection-subtype-decision");
        if (CardSelectionObserverSignals.IsCardSelectionState(request.Observer))
        {
            builder.AddSuppressed("click reward choice", "card-selection-subtype-foreground-suppresses-reward-row-reuse");
            builder.AddSuppressed("click reward back", "card-selection-subtype-foreground-suppresses-reward-back-nav");
            builder.AddSuppressed("click event choice", "card-selection-subtype-foreground-suppresses-generic-event-choice");
            builder.AddSuppressed("click first reachable node", "card-selection-subtype-foreground-suppresses-map-fallback");
            return builder.Build(CreateWaitDecision("waiting for card-selection subtype progression", DisplayControlFlowScreen(request.Observer)), actualDecision);
        }

        var treasureDecision = TryCreateTreasureRoomDecision(request);
        builder.Consider(
            ToCandidateLabel(treasureDecision, "click treasure room action"),
            "treasure-room-runtime",
            0.97d,
            () => treasureDecision,
            "no-treasure-room-actionable-affordance");
        if (TreasureRoomObserverSignals.IsTreasureAuthorityActive(request.Observer))
        {
            builder.AddSuppressed("click exported reachable node", "treasure-room-owner-active-preserves-room-lane");
            builder.AddSuppressed("click first reachable node", "treasure-room-owner-active-preserves-room-lane");
            builder.AddSuppressed("click visible map advance", "treasure-room-owner-active-suppresses-map-arrow-contamination");
            builder.AddSuppressed("click event choice", "treasure-room-owner-active-preserves-room-lane");
            builder.AddSuppressed("click proceed", "treasure-room-explicit-proceed-is-handled-by-treasure-state-machine");
            return builder.Build(CreateWaitDecision("waiting for treasure room progression", DisplayControlFlowScreen(request.Observer)), actualDecision);
        }

        var explicitRewardDecision = TryCreateExplicitRewardResolutionDecision(request);
        builder.Consider(
            ToCandidateLabel(explicitRewardDecision, "click reward choice"),
            "reward-explicit",
            0.96d,
            () => explicitRewardDecision,
            "no-reward-resolution-needed",
            rawBounds: TryFindRewardBounds(request),
            boundsSource: "observer-reward");

        var rewardBackDecision = TryCreateRewardBackNavigationDecision(request);
        builder.Consider("click reward back", "reward-back-nav", 0.90d, () => rewardBackDecision, "reward-back-not-available", rawBounds: TryFindBackBounds(request), boundsSource: "observer-back");

        var ancientDialogueDecision = TryCreateAncientDialogueAdvanceDecision(request);
        builder.Consider(
            "click ancient dialogue advance",
            "ancient-dialogue",
            0.98d,
            () => ancientDialogueDecision,
            "no-ancient-dialogue-hitbox",
            rawBounds: TryFindEventChoiceBounds(request),
            boundsSource: "observer-ancient-dialogue");
        if (ancientContract.HasExplicitDialogueSurface)
        {
            builder.AddSuppressed("click event choice", "ancient-dialogue-must-finish-before-option-selection");
            builder.AddSuppressed("click proceed", "ancient-dialogue-does-not-use-generic-proceed");
            builder.AddSuppressed("click exported reachable node", "event-owner-active-preserves-room-lane");
            builder.AddSuppressed("click first reachable node", "event-owner-active-preserves-room-lane");
            builder.AddSuppressed("click visible map advance", "event-owner-active-suppresses-map-arrow-contamination");
            return builder.Build(CreateWaitDecision("waiting for explicit ancient dialogue hitbox", DisplayControlFlowScreen(request.Observer)), actualDecision);
        }

        var ancientCompletionDecision = TryCreateAncientEventCompletionDecision(request);
        builder.Consider(
            "click ancient event completion",
            "ancient-completion-button",
            0.97d,
            () => ancientCompletionDecision,
            "no-explicit-ancient-completion-button",
            rawBounds: TryFindEventChoiceBounds(request),
            boundsSource: "observer-ancient-completion");
        if (ancientContract.HasExplicitCompletionSurface)
        {
            builder.AddSuppressed("click proceed", "ancient-completion-remains-event-owned-through-explicit-proceed-button");
            builder.AddSuppressed("click exported reachable node", "event-owner-active-preserves-room-lane");
            builder.AddSuppressed("click first reachable node", "event-owner-active-preserves-room-lane");
            builder.AddSuppressed("click visible map advance", "event-owner-active-suppresses-map-arrow-contamination");
            return builder.Build(CreateWaitDecision("waiting for explicit ancient event completion", DisplayControlFlowScreen(request.Observer)), actualDecision);
        }

        if (eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.ReleasePending)
        {
            builder.AddSuppressed("click proceed", "event-release-pending-suppresses-same-proceed-reissue");
            builder.AddSuppressed("click event choice", "event-release-pending-suppresses-stale-event-choice-reissue");
            builder.AddSuppressed("click exported reachable node", "event-release-pending-waits-for-explicit-release-before-map-routing");
            builder.AddSuppressed("click first reachable node", "event-release-pending-waits-for-explicit-release-before-map-routing");
            builder.AddSuppressed("click visible map advance", "event-release-pending-suppresses-map-contamination");
            return builder.Build(CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit event release"), actualDecision);
        }

        var ancientOptionDecision = TryCreateAncientEventOptionDecision(request);
        builder.Consider(
            "click event choice",
            "ancient-option-buttons",
            0.96d,
            () => ancientOptionDecision,
            "no-explicit-ancient-option-button",
            rawBounds: TryFindEventChoiceBounds(request),
            boundsSource: "observer-ancient-option");
        if (ancientContract.HasExplicitOptionSurface)
        {
            builder.AddSuppressed("click proceed", "ancient-event-options-should-use-explicit-option-buttons");
            builder.AddSuppressed("click exported reachable node", "event-owner-active-preserves-room-lane");
            builder.AddSuppressed("click first reachable node", "event-owner-active-preserves-room-lane");
            builder.AddSuppressed("click visible map advance", "event-owner-active-suppresses-map-arrow-contamination");
            return builder.Build(CreateWaitDecision("waiting for explicit ancient event option buttons", DisplayControlFlowScreen(request.Observer)), actualDecision);
        }

        var ancientOptionContractReconciliationDecision = TryCreateAncientOptionContractReconciliationDecision(request, eventScene);
        builder.Consider(
            "click event choice",
            "ancient-option-contract-reconciliation",
            0.955d,
            () => ancientOptionContractReconciliationDecision,
            "no-same-family-event-option-button-for-bounded-reconciliation",
            rawBounds: TryFindEventChoiceBounds(request),
            boundsSource: "observer-event-option-button");
        if (ancientContract.HasLaneSurfaceMismatch)
        {
            builder.AddSuppressed("click proceed", "ancient-option-contract-mismatch-suppresses-stale-proceed-inference");
            builder.AddSuppressed("click exported reachable node", "event-owner-active-preserves-room-lane");
            builder.AddSuppressed("click first reachable node", "event-owner-active-preserves-room-lane");
            builder.AddSuppressed("click visible map advance", "event-owner-active-suppresses-map-arrow-contamination");
            return builder.Build(
                CreateAncientOptionContractMismatchAbortDecision(
                    request,
                    "ancient option contract mismatch: foreground lane claims explicit ancient buttons, but the actionable event-option surface is generic",
                    "ancient-event-option-contract-mismatch"),
                actualDecision);
        }

        if (mapOverlayState.ForegroundVisible && !strongEventForegroundChoice)
        {
            if (mapOverlayState.StaleEventChoicePresent)
            {
                builder.AddSuppressed("click event choice", "map-overlay-foreground-removes-stale-event-choice");
            }

            builder.Consider("click exported reachable node", "observer-export:map-node", 0.88d, () => TryCreateExportedReachableMapPointDecision(request), "no-exported-reachable-node", rawBounds: TryFindMapNodeBounds(request), boundsSource: "observer-map-node");
            builder.Consider("click map back", "overlay-back-navigation", 0.84d, () => TryCreateMapBackNavigationDecision(request), "map-back-navigation-unavailable", rawBounds: TryFindBackBounds(request), boundsSource: "overlay-back");
            return builder.Build(CreateWaitDecision("waiting for an explicit event progression choice", DisplayControlFlowScreen(request.Observer)), actualDecision);
        }

        if (forceEventProgressionAfterCardSelection)
        {
            builder.AddSuppressed("click exported reachable node", "event-owner-active-preserves-room-lane");
            builder.AddSuppressed("click first reachable node", "event-owner-active-preserves-room-lane");
            builder.AddSuppressed("click visible map advance", "event-owner-active-suppresses-map-arrow-contamination");
        }

        if (eventForegroundActive
            && (request.SceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase)
                || request.SceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase)
                || request.SceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)))
        {
            builder.AddSuppressed("click visible map advance", "event-owner-active-suppresses-map-arrow-contamination");
        }

        var explicitProceedDecision = TryCreateExplicitEventProceedDecision(request);
        builder.Consider(
            "click proceed",
            "event-proceed-explicit",
            eventForegroundActive || eventScene.ExplicitProceedVisible ? 0.97d : 0.78d,
            () => explicitProceedDecision,
            "no-explicit-event-proceed",
            rawBounds: TryFindEventChoiceBounds(request),
            boundsSource: "observer-event");

        var semanticDecision = TryCreateSemanticEventDecision(request);
        builder.Consider("click event choice", "semantic-event", eventForegroundActive ? 0.98d : 0.72d, () => semanticDecision, "no-semantic-event-choice", rawBounds: TryFindEventChoiceBounds(request), boundsSource: "observer-event");

        var explicitEventChoice = TryCreateEventProgressChoiceDecision(request);
        builder.Consider("click event choice", "observer:event-choice", eventForegroundActive ? 0.94d : 0.68d, () => explicitEventChoice, "no-explicit-event-choice", rawBounds: TryFindEventChoiceBounds(request), boundsSource: "observer-event");

        if (LooksLikeMapTransitionState(request))
        {
            builder.Consider("click exported reachable node", "branch:choose-first-node", 0.66d, () => DecideChooseFirstNode(request with { Phase = GuiSmokePhase.ChooseFirstNode.ToString() }, analysisContext), "no-map-transition-candidate", boundsSource: "branch-choose-first-node");
        }
        else
        {
            builder.AddSuppressed("click exported reachable node", "event-owner-active-suppresses-map-transition-branch");
        }

        return builder.Build(CreateWaitDecision("waiting for an explicit event progression choice", DisplayControlFlowScreen(request.Observer)), actualDecision);
    }
}
