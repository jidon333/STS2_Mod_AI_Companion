using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

static class RestSiteObserverSignals
{
    public static string? TryGetMetaValue(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value) ? value : null;
    }

    public static bool HasSmithUpgradeVisible(ObserverSummary observer)
    {
        return HasSmithGridVisible(observer) || HasSmithConfirmVisible(observer) || HasSmithUpgradeScreenVisible(observer);
    }

    public static bool HasSmithUpgradeScreenVisible(ObserverSummary observer)
    {
        return MatchesDirectScreen(observer, "upgrade")
               || MatchesCompatibilityScreen(observer, "upgrade")
               || string.Equals(TryGetMetaValue(observer, "restSiteUpgradeScreenVisible"), "true", StringComparison.OrdinalIgnoreCase)
               || string.Equals(TryGetMetaValue(observer, "restSiteViewKind"), "smith-grid-observer-miss", StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasSmithGridVisible(ObserverSummary observer)
    {
        return observer.Choices.Any(static choice => string.Equals(choice.Kind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase))
               || string.Equals(TryGetMetaValue(observer, "restSiteSelectionCurrentStatus"), "grid-visible", StringComparison.OrdinalIgnoreCase)
               || (string.Equals(TryGetMetaValue(observer, "restSiteUpgradeScreenVisible"), "true", StringComparison.OrdinalIgnoreCase)
                   && TryParseMetaInt(observer, "restSiteUpgradeCardCount") > 0);
    }

    public static bool HasSmithConfirmVisible(ObserverSummary observer)
    {
        return observer.Choices.Any(static choice => string.Equals(choice.Kind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase))
               || string.Equals(TryGetMetaValue(observer, "restSiteSelectionCurrentStatus"), "confirm-visible", StringComparison.OrdinalIgnoreCase)
               || string.Equals(TryGetMetaValue(observer, "restSiteUpgradeConfirmVisible"), "true", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsRestSiteSmithUpgradeState(ObserverSummary observer)
    {
        return string.Equals(observer.ChoiceExtractorPath, "rest-smith-upgrade", StringComparison.OrdinalIgnoreCase)
               || HasSmithUpgradeScreenVisible(observer)
               || string.Equals(TryGetMetaValue(observer, "restSiteViewKind"), "smith-grid", StringComparison.OrdinalIgnoreCase)
               || string.Equals(TryGetMetaValue(observer, "restSiteViewKind"), "smith-confirm", StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasExportedSmithUpgradeChoices(ObserverSummary observer)
    {
        return observer.Choices.Any(static choice =>
            string.Equals(choice.Kind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.BindingKind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.Kind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.BindingKind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase));
    }

    public static ObserverChoice? TryGetSmithConfirmChoice(ObserverSummary observer)
    {
        return observer.Choices
            .Where(static choice =>
                string.Equals(choice.Kind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.BindingKind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static choice => choice.Enabled == false ? 1 : 0)
            .ThenBy(static choice => string.IsNullOrWhiteSpace(choice.ScreenBounds) ? 1 : 0)
            .FirstOrDefault();
    }

    public static ObserverChoice? TryGetFirstSmithCardChoice(ObserverSummary observer)
    {
        return observer.Choices
            .Where(static choice =>
                string.Equals(choice.Kind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choice.BindingKind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static choice => choice.Enabled == false ? 1 : 0)
            .ThenBy(static choice => string.IsNullOrWhiteSpace(choice.ScreenBounds) ? 1 : 0)
            .ThenBy(static choice => TryGetSortX(choice.ScreenBounds))
            .ThenBy(static choice => TryGetSortY(choice.ScreenBounds))
            .FirstOrDefault();
    }

    public static RestSitePostClickEvidence BuildPostClickEvidence(ObserverSummary observer, string? targetLabel)
    {
        var normalizedTarget = RestSiteChoiceSupport.NormalizeOptionId(RestSiteChoiceSupport.MapLabelToOptionId(targetLabel) ?? targetLabel);
        var explicitChoiceVisible = RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(observer);
        var smithGridVisible = HasSmithGridVisible(observer);
        var smithConfirmVisible = HasSmithConfirmVisible(observer);
        var upgradeScreenVisible = HasSmithUpgradeScreenVisible(observer);
        var outcome = TryGetMetaValue(observer, "restSiteSelectionOutcome");
        var outcomeEvidence = TryGetMetaValue(observer, "restSiteSelectionOutcomeEvidence");
        var currentStatus = TryGetMetaValue(observer, "restSiteSelectionCurrentStatus");
        var currentOptionId = RestSiteChoiceSupport.NormalizeOptionId(TryGetMetaValue(observer, "restSiteSelectionCurrentOptionId"));
        var lastSignal = TryGetMetaValue(observer, "restSiteSelectionLastSignal");
        var lastOptionId = RestSiteChoiceSupport.NormalizeOptionId(TryGetMetaValue(observer, "restSiteSelectionLastOptionId"));
        var upgradeChoiceObserverMiss = string.Equals(TryGetMetaValue(observer, "restSiteUpgradeObserverMiss"), "true", StringComparison.OrdinalIgnoreCase)
                                        || (string.Equals(currentStatus, "grid-visible", StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(currentStatus, "confirm-visible", StringComparison.OrdinalIgnoreCase)
                                            || upgradeScreenVisible)
                                        && !observer.Choices.Any(static choice =>
                                            string.Equals(choice.Kind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(choice.Kind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase));

        var classification = "rest-site-post-click-noop";
        if (string.Equals(normalizedTarget, "SMITH", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(outcome, "failure", StringComparison.OrdinalIgnoreCase)
                || (string.Equals(lastOptionId, "SMITH", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(lastSignal, "after-select-failure", StringComparison.OrdinalIgnoreCase))
                || string.Equals(currentStatus, "selection-failed", StringComparison.OrdinalIgnoreCase))
            {
                classification = "rest-site-selection-failed";
            }
            else if (upgradeChoiceObserverMiss
                     || (string.Equals(outcome, "success", StringComparison.OrdinalIgnoreCase)
                         && upgradeScreenVisible
                         && !smithGridVisible
                         && !smithConfirmVisible))
            {
                classification = "rest-site-grid-observer-miss";
            }
            else if (string.Equals(outcome, "in-progress", StringComparison.OrdinalIgnoreCase)
                     || (string.Equals(lastOptionId, "SMITH", StringComparison.OrdinalIgnoreCase)
                      && (string.Equals(lastSignal, "before-select", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(lastSignal, "after-select-success", StringComparison.OrdinalIgnoreCase)))
                     || (string.Equals(currentOptionId, "SMITH", StringComparison.OrdinalIgnoreCase)
                         && (string.Equals(currentStatus, "selecting", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(currentStatus, "options-disabled", StringComparison.OrdinalIgnoreCase))))
            {
                classification = "rest-site-grid-not-visible-after-selection";
            }
        }

        return new RestSitePostClickEvidence(
            classification,
            outcome,
            outcomeEvidence,
            currentStatus,
            currentOptionId,
            lastSignal,
            lastOptionId,
            upgradeScreenVisible,
            explicitChoiceVisible,
            smithGridVisible,
            smithConfirmVisible,
            upgradeChoiceObserverMiss);
    }

    private static int TryParseMetaInt(ObserverSummary observer, string key)
    {
        return int.TryParse(TryGetMetaValue(observer, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private static double TryGetSortX(string? rawBounds)
        => TryParseBoundsInternal(rawBounds, out var bounds) ? bounds.X : double.MaxValue;

    private static double TryGetSortY(string? rawBounds)
        => TryParseBoundsInternal(rawBounds, out var bounds) ? bounds.Y : double.MaxValue;

    private static bool TryParseBoundsInternal(string? rawBounds, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(rawBounds))
        {
            return false;
        }

        var parts = rawBounds.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
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
}
