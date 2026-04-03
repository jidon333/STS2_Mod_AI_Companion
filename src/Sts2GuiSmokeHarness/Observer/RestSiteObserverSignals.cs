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

    public static bool TryGetMetaBool(ObserverSummary observer, string key)
    {
        return string.Equals(TryGetMetaValue(observer, key), "true", StringComparison.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<string> GetClickReadyOptionIds(ObserverSummary observer)
    {
        var raw = TryGetMetaValue(observer, "restSiteClickReadyOptionIds");
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        return raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(RestSiteChoiceSupport.NormalizeOptionId)
            .Where(static optionId => !string.IsNullOrWhiteSpace(optionId))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static bool IsRestSiteChoiceClickReady(ObserverSummary observer, string? optionId)
    {
        var normalizedOptionId = RestSiteChoiceSupport.NormalizeOptionId(optionId);
        if (string.IsNullOrWhiteSpace(normalizedOptionId))
        {
            return false;
        }

        var clickReadyOptionIds = GetClickReadyOptionIds(observer);
        if (clickReadyOptionIds.Count > 0)
        {
            return clickReadyOptionIds.Contains(normalizedOptionId, StringComparer.OrdinalIgnoreCase);
        }

        if (observer.Meta.ContainsKey("restSiteButtonsClickReady"))
        {
            return TryGetMetaBool(observer, "restSiteButtonsClickReady");
        }

        if (observer.Meta.ContainsKey("restSiteOptionsInteractive"))
        {
            return string.Equals(TryGetMetaValue(observer, "restSiteOptionsInteractive"), "true", StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    public static bool HasProceedVisible(ObserverSummary observer)
    {
        return TryGetMetaBool(observer, "restSiteProceedVisible");
    }

    public static bool HasProceedEnabled(ObserverSummary observer)
    {
        return TryGetMetaBool(observer, "restSiteProceedEnabled");
    }

    public static string? GetProceedBounds(ObserverSummary observer)
    {
        var rawBounds = TryGetMetaValue(observer, "restSiteProceedBounds");
        return string.IsNullOrWhiteSpace(rawBounds) ? null : rawBounds;
    }

    public static bool HasChoiceSurfaceAmbiguous(ObserverSummary observer)
    {
        return TryGetMetaBool(observer, "restSiteChoiceSurfaceAmbiguous");
    }

    public static bool HasSmithUpgradeVisible(ObserverSummary observer)
    {
        return HasSmithGridVisible(observer) || HasSmithConfirmVisible(observer) || HasSmithUpgradeScreenVisible(observer);
    }

    public static bool HasSmithUpgradeScreenVisible(ObserverSummary observer)
    {
        return MatchesDirectScreen(observer, "upgrade")
               || MatchesControlFlowScreen(observer, "upgrade")
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

    public static bool IsRestSiteSelectionSettlingState(ObserverSummary observer)
    {
        if (IsRestSiteSmithUpgradeSurfacePending(observer))
        {
            return true;
        }

        if (IsRestSiteSmithUpgradeState(observer))
        {
            return false;
        }

        var selectionSignalFresh = HasFreshSelectionSignalForCurrentRoom(observer);

        var currentStatus = TryGetMetaValue(observer, "restSiteSelectionCurrentStatus");
        var outcome = TryGetMetaValue(observer, "restSiteSelectionOutcome");
        var lastSignal = TryGetMetaValue(observer, "restSiteSelectionLastSignal");
        var currentOptionId = RestSiteChoiceSupport.NormalizeOptionId(TryGetMetaValue(observer, "restSiteSelectionCurrentOptionId"));
        var lastOptionId = RestSiteChoiceSupport.NormalizeOptionId(TryGetMetaValue(observer, "restSiteSelectionLastOptionId"));
        var observedOptionId = RestSiteChoiceSupport.NormalizeOptionId(TryGetMetaValue(observer, "restSiteSelectionObservedOptionId"));
        var activeOptionId = currentOptionId ?? observedOptionId ?? lastOptionId;
        if (string.IsNullOrWhiteSpace(activeOptionId))
        {
            return false;
        }

        if (string.Equals(currentStatus, "selection-failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(outcome, "failure", StringComparison.OrdinalIgnoreCase)
            || string.Equals(lastSignal, "after-select-failure", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Equals(currentStatus, "selecting", StringComparison.OrdinalIgnoreCase)
               || string.Equals(currentStatus, "options-disabled", StringComparison.OrdinalIgnoreCase)
               || (selectionSignalFresh
                   && (string.Equals(outcome, "in-progress", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(lastSignal, "before-select", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(outcome, "success", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(lastSignal, "after-select-success", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(TryGetMetaValue(observer, "restSiteSelectionLastSuccess"), "true", StringComparison.OrdinalIgnoreCase)));
    }

    public static bool HasSelectionAcceptedRecently(ObserverSummary observer)
    {
        if (!HasFreshSelectionSignalForCurrentRoom(observer))
        {
            return false;
        }

        return string.Equals(TryGetMetaValue(observer, "restSiteSelectionLastSuccess"), "true", StringComparison.OrdinalIgnoreCase)
               || string.Equals(TryGetMetaValue(observer, "restSiteSelectionLastSignal"), "after-select-success", StringComparison.OrdinalIgnoreCase)
               || IsRestSiteSelectionSettlingState(observer);
    }

    public static bool HasFreshSelectionSignalForCurrentRoom(ObserverSummary observer)
    {
        if (!TryGetMetaTimestamp(observer, "restSiteSelectionLastSignalAt", out var lastSignalAt))
        {
            return true;
        }

        if (!TryGetLatestRestSiteRoomEnteredAt(observer, out var roomEnteredAt))
        {
            return true;
        }

        return lastSignalAt >= roomEnteredAt;
    }

    public static bool HasExportedSmithUpgradeChoices(ObserverSummary observer)
    {
        return observer.Choices.Any(static choice =>
            string.Equals(choice.Kind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.BindingKind, "rest-site-smith-card", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.Kind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase)
            || string.Equals(choice.BindingKind, "rest-site-smith-confirm", StringComparison.OrdinalIgnoreCase))
               || HasUpgradeCardSelectionSurface(observer);
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
        var proceedVisible = HasProceedVisible(observer);
        var proceedEnabled = HasProceedEnabled(observer);
        var smithGridVisible = HasSmithGridVisible(observer);
        var smithConfirmVisible = HasSmithConfirmVisible(observer);
        var upgradeScreenVisible = HasSmithUpgradeScreenVisible(observer);
        var outcome = TryGetMetaValue(observer, "restSiteSelectionOutcome");
        var outcomeEvidence = TryGetMetaValue(observer, "restSiteSelectionOutcomeEvidence");
        var currentStatus = TryGetMetaValue(observer, "restSiteSelectionCurrentStatus");
        var currentOptionId = RestSiteChoiceSupport.NormalizeOptionId(TryGetMetaValue(observer, "restSiteSelectionCurrentOptionId"));
        var lastSignal = TryGetMetaValue(observer, "restSiteSelectionLastSignal");
        var lastOptionId = RestSiteChoiceSupport.NormalizeOptionId(TryGetMetaValue(observer, "restSiteSelectionLastOptionId"));
        var lastChosenOptionId = RestSiteChoiceSupport.GetLastChosenOptionId(observer);
        var targetStillVisible = RestSiteChoiceSupport.IsTargetStillVisible(observer, normalizedTarget);
        var choiceSurfaceAmbiguous = RestSiteChoiceSupport.HasChoiceSurfaceAmbiguity(observer);
        var visibleOptionIds = string.Join(",", RestSiteChoiceSupport.GetVisibleOptionIds(observer));
        var choiceSurfaceSummary = RestSiteChoiceSupport.GetChoiceSurfaceAmbiguitySummary(observer);
        var upgradeActionSurfacePresent = HasUpgradeCardSelectionSurface(observer);
        var smithUpgradeSurfacePending = IsRestSiteSmithUpgradeSurfacePending(observer);
        var upgradeChoiceObserverMiss = string.Equals(TryGetMetaValue(observer, "restSiteUpgradeObserverMiss"), "true", StringComparison.OrdinalIgnoreCase)
                                        || (string.Equals(currentStatus, "grid-visible", StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(currentStatus, "confirm-visible", StringComparison.OrdinalIgnoreCase)
                                            || upgradeScreenVisible)
                                        && !upgradeActionSurfacePresent
                                        && !smithUpgradeSurfacePending;

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
            else if (choiceSurfaceAmbiguous
                     && targetStillVisible
                     && !proceedEnabled
                     && !smithGridVisible
                     && !smithConfirmVisible
                     && !upgradeScreenVisible)
            {
                classification = "rest-site-choice-surface-ambiguous";
            }
            else if (upgradeChoiceObserverMiss
                     || (string.Equals(outcome, "success", StringComparison.OrdinalIgnoreCase)
                         && upgradeScreenVisible
                         && !smithGridVisible
                         && !smithConfirmVisible
                         && !smithUpgradeSurfacePending))
            {
                classification = "rest-site-grid-observer-miss";
            }
            else if (smithUpgradeSurfacePending
                     || string.Equals(outcome, "in-progress", StringComparison.OrdinalIgnoreCase)
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
            lastChosenOptionId,
            upgradeScreenVisible,
            explicitChoiceVisible,
            proceedVisible,
            proceedEnabled,
            smithGridVisible,
            smithConfirmVisible,
            upgradeChoiceObserverMiss,
            targetStillVisible,
            choiceSurfaceAmbiguous,
            string.IsNullOrWhiteSpace(visibleOptionIds) ? null : visibleOptionIds,
            choiceSurfaceSummary);
    }

    public static bool IsRestSiteSmithUpgradeSurfacePending(ObserverSummary observer)
    {
        if (!HasFreshSelectionSignalForCurrentRoom(observer)
            || !IsRestSiteSmithUpgradeState(observer)
            || HasSmithGridVisible(observer)
            || HasSmithConfirmVisible(observer)
            || HasUpgradeCardSelectionSurface(observer))
        {
            return false;
        }

        if (HasProceedEnabled(observer))
        {
            return false;
        }

        var currentStatus = TryGetMetaValue(observer, "restSiteSelectionCurrentStatus");
        var outcome = TryGetMetaValue(observer, "restSiteSelectionOutcome");
        var lastSignal = TryGetMetaValue(observer, "restSiteSelectionLastSignal");
        var currentOptionId = RestSiteChoiceSupport.NormalizeOptionId(TryGetMetaValue(observer, "restSiteSelectionCurrentOptionId"));
        var observedOptionId = RestSiteChoiceSupport.NormalizeOptionId(TryGetMetaValue(observer, "restSiteSelectionObservedOptionId"));
        var lastOptionId = RestSiteChoiceSupport.NormalizeOptionId(TryGetMetaValue(observer, "restSiteSelectionLastOptionId"));
        var activeOptionId = currentOptionId ?? observedOptionId ?? lastOptionId;
        if (!string.Equals(activeOptionId, "SMITH", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(currentStatus, "selection-failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(outcome, "failure", StringComparison.OrdinalIgnoreCase)
            || string.Equals(lastSignal, "after-select-failure", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Equals(currentStatus, "proceed-visible", StringComparison.OrdinalIgnoreCase)
               || string.Equals(currentStatus, "selecting", StringComparison.OrdinalIgnoreCase)
               || string.Equals(currentStatus, "options-disabled", StringComparison.OrdinalIgnoreCase)
               || string.Equals(outcome, "in-progress", StringComparison.OrdinalIgnoreCase)
               || string.Equals(lastSignal, "before-select", StringComparison.OrdinalIgnoreCase)
               || string.Equals(lastSignal, "after-select-success", StringComparison.OrdinalIgnoreCase)
               || string.Equals(TryGetMetaValue(observer, "restSiteSelectionLastSuccess"), "true", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasUpgradeCardSelectionSurface(ObserverSummary observer)
    {
        if (CardSelectionObserverSignals.TryGetState(observer) is not { ScreenType: "upgrade" } upgradeState)
        {
            return false;
        }

        return CardSelectionObserverSignals.GetCardChoices(observer, upgradeState).Count > 0
               || CardSelectionObserverSignals.TryGetConfirmChoice(observer, upgradeState) is not null;
    }

    private static int TryParseMetaInt(ObserverSummary observer, string key)
    {
        return int.TryParse(TryGetMetaValue(observer, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private static bool TryGetMetaTimestamp(ObserverSummary observer, string key, out DateTimeOffset timestamp)
    {
        var rawValue = TryGetMetaValue(observer, key);
        return DateTimeOffset.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out timestamp);
    }

    private static bool TryGetLatestRestSiteRoomEnteredAt(ObserverSummary observer, out DateTimeOffset roomEnteredAt)
    {
        roomEnteredAt = default;
        var found = false;
        foreach (var tail in observer.LastEventsTail)
        {
            if (string.IsNullOrWhiteSpace(tail)
                || !tail.Contains("\"kind\":\"room-entered\"", StringComparison.OrdinalIgnoreCase)
                || !tail.Contains("\"screen\":\"rest-site\"", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!TryParseEventTailTimestamp(tail, out var parsedTimestamp))
            {
                continue;
            }

            if (!found || parsedTimestamp > roomEnteredAt)
            {
                roomEnteredAt = parsedTimestamp;
                found = true;
            }
        }

        return found;
    }

    private static bool TryParseEventTailTimestamp(string eventTail, out DateTimeOffset timestamp)
    {
        timestamp = default;
        try
        {
            using var document = JsonDocument.Parse(eventTail);
            if (!document.RootElement.TryGetProperty("ts", out var timestampElement))
            {
                return false;
            }

            return timestampElement.ValueKind == JsonValueKind.String
                   && DateTimeOffset.TryParse(
                       timestampElement.GetString(),
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.RoundtripKind,
                       out timestamp);
        }
        catch (JsonException)
        {
            return false;
        }
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
