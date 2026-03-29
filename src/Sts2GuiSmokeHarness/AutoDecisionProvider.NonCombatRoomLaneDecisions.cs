sealed partial class AutoDecisionProvider
{
    private static GuiSmokeStepDecision DecideChooseFirstNodeRestSiteExplicitChoice(GuiSmokeStepRequest request)
    {
        ConfigureRestSiteChooseFirstNodeDebug(BuildExplicitRestSiteCandidateLabels(request.Observer), "rest-site-owner-active-suppresses-map-arrow-contamination");
        return GuiSmokeDecisionDebug.TraceCandidate(
                   "rest site explicit choice",
                   "rest-site-choice",
                   0.98,
                   TryCreateRestSiteDecision(request),
                   "rest-site explicit choices are not visible")
               ?? GuiSmokeDecisionDebug.TraceCandidate(
                   "rest site upgrade",
                   "rest-site-upgrade",
                   0.92,
                   TryCreateRestSiteUpgradeDecision(request),
                   "rest-site upgrade grid is not currently actionable")
               ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for an explicit rest-site choice");
    }

    private static GuiSmokeStepDecision DecideChooseFirstNodeRestSiteSmithUpgrade(GuiSmokeStepRequest request)
    {
        ConfigureRestSiteChooseFirstNodeDebug(new[] { "click smith card", "click smith confirm", "wait" }, "rest-site-owner-active-suppresses-map-arrow-contamination");
        return GuiSmokeDecisionDebug.TraceCandidate(
                   "rest site upgrade",
                   "rest-site-upgrade",
                   0.92,
                   TryCreateRestSiteUpgradeDecision(request),
                   "rest-site upgrade grid is not currently actionable")
               ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for rest-site smith upgrade controls");
    }

    private static GuiSmokeStepDecision DecideChooseFirstNodeRestSiteProceed(GuiSmokeStepRequest request)
    {
        ConfigureRestSiteChooseFirstNodeDebug(new[] { "click proceed", "wait" }, "rest-site-owner-active-suppresses-map-arrow-contamination");
        return GuiSmokeDecisionDebug.TraceCandidate(
                   "visible proceed",
                   "rest-site-proceed",
                   0.96,
                   TryCreateRestSiteProceedDecision(request),
                   "rest-site proceed affordance is not currently actionable")
               ?? CreateForegroundAwareNonCombatWaitDecision(request, "waiting for explicit rest-site proceed");
    }

    private static GuiSmokeStepDecision? TryCreateChooseFirstNodeTreasureRoomDecision(GuiSmokeStepRequest request)
    {
        return TryCreateTreasureRoomLaneDecision(
            request,
            "waiting for treasure room progression",
            suppressEventChoice: false);
    }

    private static GuiSmokeStepDecision? TryCreateTreasureRoomLaneDecision(
        GuiSmokeStepRequest request,
        string waitReason,
        bool suppressEventChoice)
    {
        var treasureDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "treasure room",
            "treasure-room-runtime",
            0.95,
            TryCreateTreasureRoomDecision(request),
            "no explicit treasure room affordance is currently actionable");
        if (!TreasureRoomObserverSignals.IsTreasureAuthorityActive(request.Observer))
        {
            return treasureDecision;
        }

        var treasureState = TreasureRoomObserverSignals.TryGetState(request.Observer);
        if (treasureState is not null)
        {
            GuiSmokeDecisionDebug.ReplaceActiveCandidates(
                TreasureRoomObserverSignals.BuildAllowedActions(treasureState)
                    .Append("wait")
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray());
        }

        ConfigureTreasureRoomLaneDebug(suppressEventChoice);
        return treasureDecision ?? CreateForegroundAwareNonCombatWaitDecision(request, waitReason);
    }

    private static GuiSmokeStepDecision? TryCreateEventProgressionDecision(GuiSmokeStepRequest request)
    {
        var semanticDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "semantic event option",
            "event-semantic",
            0.92,
            TryCreateSemanticEventDecision(request),
            "no semantic event option matched current screenshot and observer evidence");
        if (semanticDecision is not null)
        {
            return semanticDecision;
        }

        return GuiSmokeDecisionDebug.TraceCandidate(
            "explicit event choice",
            "event-choice",
            0.90,
            TryCreateEventProgressChoiceDecision(request),
            "no explicit event choice has usable bounds");
    }

    private static GuiSmokeStepDecision? TryCreateRewardForegroundDecision(GuiSmokeStepRequest request)
    {
        var explicitRewardDecision = GuiSmokeDecisionDebug.TraceCandidate(
            "explicit reward resolution",
            "reward-foreground",
            0.94,
            TryCreateExplicitRewardResolutionDecision(request),
            "no explicit reward foreground affordance is available");
        if (explicitRewardDecision is not null)
        {
            return explicitRewardDecision;
        }

        return GuiSmokeDecisionDebug.TraceCandidate(
            "reward back",
            "reward-back-nav",
            0.84,
            TryCreateRewardBackNavigationDecision(request),
            "reward back navigation is not available");
    }

    private static void ConfigureRestSiteChooseFirstNodeDebug(string[] candidates, string mapAdvanceSuppressionReason)
    {
        GuiSmokeDecisionDebug.SetSceneModel("rest-site", "map");
        GuiSmokeDecisionDebug.ReplaceActiveCandidates(candidates);
        GuiSmokeDecisionDebug.Suppress("click exported reachable node", "rest-site-owner-active-preserves-room-lane");
        GuiSmokeDecisionDebug.Suppress("click first reachable node", "rest-site-owner-active-preserves-room-lane");
        GuiSmokeDecisionDebug.Suppress("click visible map advance", mapAdvanceSuppressionReason);
        GuiSmokeDecisionDebug.Suppress("click map back", "rest-site-owner-active-preserves-room-lane");
    }

    private static void ConfigureTreasureRoomLaneDebug(bool suppressEventChoice)
    {
        GuiSmokeDecisionDebug.SetSceneModel("treasure-room", "room-context");
        GuiSmokeDecisionDebug.Suppress("click exported reachable node", "treasure-room-owner-active-preserves-room-lane");
        GuiSmokeDecisionDebug.Suppress("click first reachable node", "treasure-room-owner-active-preserves-room-lane");
        GuiSmokeDecisionDebug.Suppress("click visible map advance", "treasure-room-owner-active-suppresses-map-arrow-contamination");
        if (suppressEventChoice)
        {
            GuiSmokeDecisionDebug.Suppress("click event choice", "treasure-room-owner-active-preserves-room-lane");
        }
    }
}
