using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;

static class GuiSmokeNonCombatContractSupport
{
    public static bool HasExplicitRestSiteChoiceAuthority(GuiSmokeStepRequest request)
    {
        return HasRestSiteAuthority(request.Observer)
               && RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(request.Observer)
               && !RestSiteObserverSignals.IsRestSiteSmithUpgradeState(request.Observer)
               && !AutoRestSiteCardGridAnalyzer.Analyze(request.ScreenshotPath).HasSelectableCard;
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

        var onRestSiteScreen = string.Equals(observer.CurrentScreen, "rest-site", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(observer.VisibleScreen, "rest-site", StringComparison.OrdinalIgnoreCase)
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
            request.Observer.CurrentScreen,
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
            { } label when label.Contains("rest site:", StringComparison.OrdinalIgnoreCase) => "click rest site choice",
            { } label when label.Contains("smith card", StringComparison.OrdinalIgnoreCase) => "click smith card",
            { } label when label.Contains("smith confirm", StringComparison.OrdinalIgnoreCase) => "click smith confirm",
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

    private static bool HasRestSiteAuthority(ObserverSummary observer)
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
                                  || string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
                                  || string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
                                  || HasExplicitMapNodeAuthority(observer);
        if (!mapAuthorityVisible)
        {
            return false;
        }

        return !RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer)
               && !RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(observer);
    }

    private static bool IsProceedNode(ObserverActionNode node)
    {
        return ContainsAny(node.Label, "Proceed", "Continue", "진행", "계속")
               || node.Kind.Contains("proceed", StringComparison.OrdinalIgnoreCase);
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
