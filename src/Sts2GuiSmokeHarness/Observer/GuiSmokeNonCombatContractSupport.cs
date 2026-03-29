using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

static class GuiSmokeNonCombatContractSupport
{
    public static bool HasExplicitRewardProgressionAffordance(ObserverSummary observer)
    {
        if (NonCombatForegroundOwnership.HasExplicitRestSiteForegroundAuthority(observer)
            || ShopObserverSignals.IsShopAuthorityActive(observer)
            || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer))
        {
            return false;
        }

        return observer.Choices.Any(IsExplicitRewardProgressionChoice)
               || observer.ActionNodes.Any(IsExplicitRewardProgressionNode);
    }

    public static bool HasExplicitRestSiteChoiceAuthority(GuiSmokeStepRequest request)
    {
        return HasExplicitRestSiteChoiceAuthority(request.Observer, request.ScreenshotPath);
    }

    public static bool HasExplicitRestSiteChoiceAuthority(ObserverSummary observer, string? screenshotPath)
    {
        if (MapAuthorityOutranksStaleRestSiteResidue(observer)
            || !HasRestSiteAuthority(observer)
            || !RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(observer)
            || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer)
            || RestSiteObserverSignals.IsRestSiteSelectionSettlingState(observer))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(screenshotPath))
        {
            return true;
        }

        return !AutoRestSiteCardGridAnalyzer.Analyze(screenshotPath).HasSelectableCard;
    }

    public static bool HasExplicitRestSiteChoiceAuthority(ObserverState observer, string? screenshotPath)
    {
        return HasExplicitRestSiteChoiceAuthority(observer.Summary, screenshotPath);
    }

    public static string[] BuildExplicitRestSiteAllowedActions(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.BuildAllowedActions(observer).ToArray();
    }

    public static bool HasRecentRestSiteExplicitClick(IReadOnlyList<GuiSmokeHistoryEntry> history, string? targetLabel)
    {
        if (!IsExplicitRestSiteOptionTarget(targetLabel))
        {
            return false;
        }

        for (var index = history.Count - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (!TryParseRestSiteActionMetadata(entry.Metadata, out var metadata))
            {
                continue;
            }

            return string.Equals(metadata.Kind, "explicit-click", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(metadata.TargetLabel, targetLabel, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public static bool IsExplicitRestSiteOptionTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "rest site: rest", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: smith", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: hatch", StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(targetLabel)
                   && targetLabel.StartsWith("rest site: option:", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsMapFallbackBlockedByForegroundAuthority(GuiSmokeStepRequest request)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(request.Observer)
            || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(request.Observer)
            || CardSelectionObserverSignals.TryGetState(request.Observer) is not null)
        {
            return true;
        }

        return NonCombatForegroundOwnership.Resolve(request.Observer) is
            not NonCombatForegroundOwner.Unknown
            and not NonCombatForegroundOwner.Map;
    }

    public static bool LooksLikeRestSiteState(ObserverSummary observer)
    {
        if (MapAuthorityOutranksStaleRestSiteResidue(observer))
        {
            return false;
        }

        if (string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
            || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase)
            || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer))
        {
            return true;
        }

        if (observer.Choices.Any(static choice =>
                !string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
                && ContainsAny(choice.Label, "휴식", "Rest", "재련", "Smith")))
        {
            return true;
        }

        return observer.Choices.Count == 0
               && observer.CurrentChoices.Any(static label =>
                   ContainsAny(label, "휴식", "Rest", "재련", "Smith"));
    }

    public static bool LooksLikeRestSiteProceedState(ObserverSummary observer)
    {
        if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer)
            || MapAuthorityOutranksStaleRestSiteResidue(observer))
        {
            return false;
        }

        var onRestSiteScreen = MatchesControlFlowScreen(observer, "rest-site")
                               || string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase);
        if (!onRestSiteScreen)
        {
            return false;
        }

        var hasProceedActionNode = observer.ActionNodes.Any(static node =>
            node.Actionable
            && IsProceedNode(node)
            && TryParseBounds(node.ScreenBounds, out _));
        var hasProceedChoice = observer.Choices.Any(static choice =>
            IsProceedChoice(choice)
            && HasLargeChoiceBounds(choice.ScreenBounds));
        var hasProceedLabel = observer.CurrentChoices.Any(IsProceedLikeLabel);
        if (!hasProceedActionNode && !hasProceedChoice && !hasProceedLabel)
        {
            return false;
        }

        var hasExplicitRestChoice = observer.Choices.Any(static choice =>
            string.Equals(choice.Kind, "rest-option", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.BindingKind, "rest-site-option", StringComparison.OrdinalIgnoreCase)
            || (HasLargeChoiceBounds(choice.ScreenBounds)
                && ContainsAny(choice.Label, "휴식", "Rest", "재련", "Smith", "부화", "Hatch")));
        if (!hasExplicitRestChoice)
        {
            return true;
        }

        return string.Equals(RestSiteObserverSignals.TryGetMetaValue(observer, "restSiteSelectionLastSuccess"), "true", StringComparison.OrdinalIgnoreCase)
               || string.Equals(RestSiteObserverSignals.TryGetMetaValue(observer, "restSiteSelectionLastSignal"), "after-select-success", StringComparison.OrdinalIgnoreCase)
               || (hasProceedLabel
                   && !observer.CurrentChoices.Any(static label =>
                       ContainsAny(label, "휴식", "Rest", "재련", "Smith", "부화", "Hatch")));
    }

    public static bool AllowsAction(GuiSmokeStepRequest request, string action)
    {
        return request.AllowedActions.Contains(action, StringComparer.OrdinalIgnoreCase);
    }

    public static bool AllowsAnyMapRoutingAction(GuiSmokeStepRequest request)
    {
        return AllowsAction(request, "click exported reachable node")
               || AllowsAction(request, "click first reachable node")
               || AllowsAction(request, "click visible map advance")
               || AllowsAction(request, "click map back");
    }

    public static GuiSmokeStepDecision CreateMapRoutingContractWaitDecision(GuiSmokeStepRequest request, string reason)
    {
        return new GuiSmokeStepDecision(
            "wait",
            null,
            null,
            null,
            null,
            null,
            reason,
            0.60,
            DisplayScreen(request.Observer),
            2000,
            true,
            null,
            DecisionRisk: "map-routing-disallowed-by-allowlist");
    }

    public static bool TryMapNonCombatAllowedAction(GuiSmokeStepDecision decision, out string allowedAction)
    {
        allowedAction = string.Empty;
        if (string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase))
        {
            allowedAction = "wait";
            return true;
        }

        if (string.IsNullOrWhiteSpace(decision.TargetLabel))
        {
            return false;
        }

        allowedAction = decision.TargetLabel switch
        {
            { } label when label.Contains("exported reachable map node", StringComparison.OrdinalIgnoreCase) => "click exported reachable node",
            { } label when label.Contains("visible reachable node", StringComparison.OrdinalIgnoreCase)
                           || label.Contains("first reachable node", StringComparison.OrdinalIgnoreCase) => "click first reachable node",
            { } label when label.Contains("visible map advance", StringComparison.OrdinalIgnoreCase) => "click visible map advance",
            { } label when label.Contains("map back", StringComparison.OrdinalIgnoreCase) => "click map back",
            { } label when label.Contains("smith card", StringComparison.OrdinalIgnoreCase) => "click smith card",
            { } label when label.Contains("smith confirm", StringComparison.OrdinalIgnoreCase) => "click smith confirm",
            { } label when label.Contains("rest site:", StringComparison.OrdinalIgnoreCase) => "click rest site choice",
            { } label when label.Contains("treasure chest", StringComparison.OrdinalIgnoreCase) => "click treasure chest",
            { } label when label.Contains("treasure relic holder", StringComparison.OrdinalIgnoreCase) => "click treasure relic holder",
            { } label when label.Contains("treasure proceed", StringComparison.OrdinalIgnoreCase) => "click treasure proceed",
            { } label when label.Contains("ancient dialogue advance", StringComparison.OrdinalIgnoreCase) => "click ancient dialogue advance",
            { } label when label.Contains("ancient event completion", StringComparison.OrdinalIgnoreCase) => "click ancient event completion",
            { } label when label.Contains("event progression choice", StringComparison.OrdinalIgnoreCase) => "click event choice",
            { } label when label.Contains("visible proceed", StringComparison.OrdinalIgnoreCase) => "click proceed",
            _ => string.Empty,
        };
        return allowedAction.Length > 0;
    }

    public static bool HasStrongMapTransitionEvidence(ObserverState observer)
    {
        var canonicalScene = AutoDecisionProvider.TryBuildCanonicalNonCombatSceneState(observer, null);
        if (RewardObserverSignals.IsTerminalRunBoundary(observer.Summary)
            || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary)
            || CardSelectionObserverSignals.TryGetState(observer.Summary) is not null
            || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary)
            || canonicalScene is { CanonicalForegroundOwner: not NonCombatCanonicalForegroundOwner.Unknown and not NonCombatCanonicalForegroundOwner.Map })
        {
            return false;
        }

        return GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer);
    }

    public static bool HasStrongMapTransitionEvidenceFromScene(ObserverSummary observer, string? sceneSignature)
    {
        var canonicalScene = AutoDecisionProvider.TryBuildCanonicalNonCombatSceneState(observer, null);
        if (RewardObserverSignals.IsTerminalRunBoundary(observer)
            || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer)
            || CardSelectionObserverSignals.TryGetState(observer) is not null
            || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer)
            || canonicalScene is { CanonicalForegroundOwner: not NonCombatCanonicalForegroundOwner.Unknown and not NonCombatCanonicalForegroundOwner.Map })
        {
            return false;
        }

        return GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer)
               || (!string.IsNullOrWhiteSpace(sceneSignature)
                   && (sceneSignature.Contains("substate:map-transition", StringComparison.OrdinalIgnoreCase)
                       || sceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)));
    }

    public static bool IsRewardMapRecoveryTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "reward back", StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasRestSiteAuthority(ObserverSummary observer)
    {
        return string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase)
               || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer);
    }

    private static bool HasMapCurrentActiveScreen(ObserverSummary observer)
    {
        return observer.Meta.TryGetValue("mapCurrentActiveScreen", out var value)
               && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasExplicitMapNodeAuthority(ObserverSummary observer)
    {
        return observer.Choices.Any(MapNodeSourceSupport.IsExplicitMapPointChoice)
               || observer.ActionNodes.Any(static node =>
                   node.Actionable
                   && MapNodeSourceSupport.IsExplicitMapPointNode(node));
    }

    private static bool MapAuthorityOutranksStaleRestSiteResidue(ObserverSummary observer)
    {
        var mapAuthorityVisible = HasMapCurrentActiveScreen(observer)
                                  || MatchesControlFlowScreen(observer, "map")
                                  || HasExplicitMapNodeAuthority(observer);
        if (!mapAuthorityVisible)
        {
            return false;
        }

        var explicitRestSiteChoiceAffordance = RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(observer);
        var buttonsExplicitlyHidden = string.Equals(RestSiteObserverSignals.TryGetMetaValue(observer, "restSiteButtonsVisible"), "false", StringComparison.OrdinalIgnoreCase);
        var optionsExplicitlyDisabled = string.Equals(RestSiteObserverSignals.TryGetMetaValue(observer, "restSiteOptionsInteractive"), "false", StringComparison.OrdinalIgnoreCase);
        var staleExplicitRestSiteResidue = explicitRestSiteChoiceAffordance && buttonsExplicitlyHidden && optionsExplicitlyDisabled;

        return !RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer)
               && (!explicitRestSiteChoiceAffordance || staleExplicitRestSiteResidue);
    }

    private static bool IsProceedNode(ObserverActionNode node)
    {
        return ContainsAny(node.Label, "Proceed", "Continue", "진행", "계속")
               || node.Kind.Contains("proceed", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExplicitRewardProgressionChoice(ObserverChoice choice)
    {
        if (!TryParseBounds(choice.ScreenBounds, out _)
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
            || !TryParseBounds(node.ScreenBounds, out _)
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

    private static bool IsRewardCardChoice(ObserverChoice choice)
    {
        return (string.Equals(choice.Kind, "card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.Kind, "reward-card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.BindingKind, "reward-type", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(choice.BindingId, "CardReward", StringComparison.OrdinalIgnoreCase)
                || choice.SemanticHints.Any(static hint => string.Equals(hint, "reward-card", StringComparison.OrdinalIgnoreCase)
                                                           || string.Equals(hint, "reward-type:CardReward", StringComparison.OrdinalIgnoreCase)))
               && !IsSkipOrProceedLabel(choice.Label)
               && !IsConfirmLikeLabel(choice.Label)
               && !IsDismissLikeLabel(choice.Label)
               && !IsOverlayChoiceLabel(choice.Label)
               && HasRewardCardLikeBounds(choice.ScreenBounds);
    }

    private static bool HasRewardCardLikeBounds(string? screenBounds)
    {
        if (!TryParseBounds(screenBounds, out var bounds))
        {
            return true;
        }

        return bounds.Width >= 120f || bounds.Height >= 150f;
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

    private static bool IsInspectPreviewChoice(ObserverChoice choice)
    {
        return IsOverlayChoiceLabel(choice.Label)
               || string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               || choice.Value?.StartsWith("RELIC.", StringComparison.OrdinalIgnoreCase) == true
               || IsInspectPreviewBounds(choice.ScreenBounds);
    }

    private static bool IsBackNode(ObserverActionNode node)
    {
        return node.Label.Contains("Back", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("뒤", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("back", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMapNode(ObserverActionNode node)
    {
        return node.NodeId.Contains("map", StringComparison.OrdinalIgnoreCase)
               || node.Kind.Contains("map", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Map", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("지도", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsProceedChoice(ObserverChoice choice)
    {
        return !string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
               && IsProceedLikeLabel(choice.Label);
    }

    private static bool TryParseBounds(string? raw, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4
            || !float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height)
            || width <= 0f
            || height <= 0f)
        {
            return false;
        }

        bounds = new RectangleF(x, y, width, height);
        return true;
    }

    private static bool TryParseRestSiteActionMetadata(string? metadata, out RestSiteActionMetadata parsed)
    {
        parsed = default!;
        if (string.IsNullOrWhiteSpace(metadata))
        {
            return false;
        }

        try
        {
            parsed = JsonSerializer.Deserialize<RestSiteActionMetadata>(metadata, GuiSmokeShared.JsonOptions)!;
            return parsed is not null
                   && !string.IsNullOrWhiteSpace(parsed.Kind)
                   && !string.IsNullOrWhiteSpace(parsed.TargetLabel)
                   && !string.IsNullOrWhiteSpace(parsed.Fingerprint);
        }
        catch
        {
            return false;
        }
    }
}
