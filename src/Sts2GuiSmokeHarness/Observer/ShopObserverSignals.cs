using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;

static class ShopObserverSignals
{
    public static ShopRoomState? TryGetState(ObserverSummary observer)
    {
        var roomDetected = TryGetMetaBool(observer, "shopRoomDetected")
                           ?? (string.Equals(observer.CurrentScreen, "shop", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(observer.VisibleScreen, "shop", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(observer.EncounterKind, "Shop", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(observer.ChoiceExtractorPath, "shop", StringComparison.OrdinalIgnoreCase));
        if (!roomDetected)
        {
            return null;
        }

        var inventoryOpen = TryGetMetaBool(observer, "shopInventoryOpen") == true;
        var merchantButtonVisible = TryGetMetaBool(observer, "shopMerchantButtonVisible") == true;
        var merchantButtonEnabled = TryGetMetaBool(observer, "shopMerchantButtonEnabled") == true;
        var proceedEnabled = TryGetMetaBool(observer, "shopProceedEnabled") == true;
        var backVisible = TryGetMetaBool(observer, "shopBackVisible") == true;
        var backEnabled = TryGetMetaBool(observer, "shopBackEnabled") == true;
        var roomVisible = TryGetMetaBool(observer, "shopRoomVisible")
                          ?? (inventoryOpen
                              || merchantButtonVisible
                              || backVisible
                              || proceedEnabled
                              || string.Equals(observer.CurrentScreen, "shop", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(observer.VisibleScreen, "shop", StringComparison.OrdinalIgnoreCase));
        var foregroundOwned = TryGetMetaBool(observer, "shopForegroundOwned")
                              ?? (inventoryOpen
                                  || merchantButtonEnabled
                                  || proceedEnabled
                                  || (backVisible && backEnabled)
                                  || string.Equals(observer.ChoiceExtractorPath, "shop", StringComparison.OrdinalIgnoreCase));
        var teardownInProgress = TryGetMetaBool(observer, "shopTeardownInProgress") == true;
        var shopIsCurrentActiveScreen = TryGetMetaBool(observer, "shopIsCurrentActiveScreen") == true;
        var mapIsCurrentActiveScreen = TryGetMetaBool(observer, "mapCurrentActiveScreen") == true;

        return new ShopRoomState(
            RoomDetected: true,
            RoomVisible: roomVisible == true,
            ForegroundOwned: foregroundOwned == true,
            TeardownInProgress: teardownInProgress,
            ShopIsCurrentActiveScreen: shopIsCurrentActiveScreen,
            MapIsCurrentActiveScreen: mapIsCurrentActiveScreen,
            ActiveScreenType: TryGetMetaValue(observer, "activeScreenType"),
            InventoryOpen: inventoryOpen,
            MerchantButtonVisible: merchantButtonVisible,
            MerchantButtonEnabled: merchantButtonEnabled,
            ProceedEnabled: proceedEnabled,
            BackVisible: backVisible,
            BackEnabled: backEnabled,
            OptionCount: TryGetMetaInt(observer, "shopOptionCount") ?? 0,
            AffordableOptionCount: TryGetMetaInt(observer, "shopAffordableOptionCount") ?? 0,
            AffordableOptionIds: ParseStringList(TryGetMetaValue(observer, "shopAffordableOptionIds")),
            AffordableRelicIds: ParseStringList(TryGetMetaValue(observer, "shopAffordableRelicIds")),
            AffordableCardIds: ParseStringList(TryGetMetaValue(observer, "shopAffordableCardIds")),
            AffordablePotionIds: ParseStringList(TryGetMetaValue(observer, "shopAffordablePotionIds")),
            CardRemovalVisible: TryGetMetaBool(observer, "shopCardRemovalVisible") == true,
            CardRemovalEnabled: TryGetMetaBool(observer, "shopCardRemovalEnabled") == true,
            CardRemovalEnoughGold: TryGetMetaBool(observer, "shopCardRemovalEnoughGold") == true,
            CardRemovalUsed: TryGetMetaBool(observer, "shopCardRemovalUsed") == true,
            RootType: TryGetMetaValue(observer, "shopRootType"));
    }

    public static bool IsShopAuthorityActive(ObserverSummary observer)
        => TryGetState(observer) is { RoomDetected: true, ForegroundOwned: true };

    public static bool HasRecentPurchase(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        for (var index = history.Count - 1; index >= 0 && index >= history.Count - 8; index -= 1)
        {
            var entry = history[index];
            if (string.Equals(entry.TargetLabel, "shop buy relic", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.TargetLabel, "shop buy card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.TargetLabel, "shop buy potion", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.TargetLabel, "shop card removal", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Action, "observer-accepted", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Action, "recapture-required", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.TargetLabel, "shop back", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return false;
        }

        return false;
    }

    public static string[] BuildAllowedActions(ObserverSummary observer, ShopRoomState state, bool alreadyPurchased)
    {
        if (!state.InventoryOpen && alreadyPurchased && state.ProceedEnabled)
        {
            return new[] { "click shop proceed", state.MerchantButtonEnabled ? "click shop open inventory" : "wait" };
        }

        if (!state.InventoryOpen && state.MerchantButtonEnabled)
        {
            return new[] { "click shop open inventory", state.ProceedEnabled ? "click shop proceed" : "wait" };
        }

        if (state.InventoryOpen && !alreadyPurchased)
        {
            if (GetAffordableRelicChoices(observer).Count > 0)
            {
                return new[] { "click shop buy relic", "click shop back", "wait" };
            }

            if (GetAffordableCardChoices(observer).Count > 0)
            {
                return new[] { "click shop buy card", "click shop back", "wait" };
            }

            if (GetAffordablePotionChoices(observer).Count > 0)
            {
                return new[] { "click shop buy potion", "click shop back", "wait" };
            }

            if (state.CardRemovalEnabled)
            {
                return new[] { "click shop card removal", "click shop back", "wait" };
            }
        }

        if (state.InventoryOpen && state.BackVisible && state.BackEnabled)
        {
            return new[] { "click shop back", "wait" };
        }

        if (!state.InventoryOpen && state.ProceedEnabled)
        {
            return new[] { "click shop proceed", "wait" };
        }

        return new[] { "wait" };
    }

    public static IReadOnlyList<ObserverChoice> GetAffordableRelicChoices(ObserverSummary observer)
        => GetTypedShopChoices(observer, "relic", requireEnabled: true);

    public static IReadOnlyList<ObserverChoice> GetAffordableCardChoices(ObserverSummary observer)
        => GetTypedShopChoices(observer, "card", requireEnabled: true);

    public static IReadOnlyList<ObserverChoice> GetAffordablePotionChoices(ObserverSummary observer)
        => GetTypedShopChoices(observer, "potion", requireEnabled: true);

    public static ObserverChoice? TryGetCardRemovalChoice(ObserverSummary observer)
    {
        return observer.Choices.FirstOrDefault(static choice =>
                   string.Equals(choice.Kind, "shop-card-removal", StringComparison.OrdinalIgnoreCase)
                   && choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds))
               ?? observer.Choices.FirstOrDefault(static choice =>
                   string.Equals(choice.BindingKind, "shop-card-removal", StringComparison.OrdinalIgnoreCase)
                   && choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds));
    }

    public static ObserverChoice? TryGetMerchantButtonChoice(ObserverSummary observer)
    {
        return observer.Choices.FirstOrDefault(static choice =>
                   string.Equals(choice.Kind, "shop-open-inventory", StringComparison.OrdinalIgnoreCase)
                   && choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds))
               ?? observer.Choices.FirstOrDefault(static choice =>
                   string.Equals(choice.BindingKind, "shop-room", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(choice.BindingId, "merchant-button", StringComparison.OrdinalIgnoreCase)
                   && choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds));
    }

    public static ObserverChoice? TryGetBackChoice(ObserverSummary observer)
    {
        return observer.Choices.FirstOrDefault(static choice =>
                   string.Equals(choice.Kind, "shop-back", StringComparison.OrdinalIgnoreCase)
                   && choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds))
               ?? observer.Choices.FirstOrDefault(static choice =>
                   string.Equals(choice.BindingKind, "shop-room", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(choice.BindingId, "back", StringComparison.OrdinalIgnoreCase)
                   && choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds));
    }

    public static ObserverChoice? TryGetProceedChoice(ObserverSummary observer)
    {
        return observer.Choices.FirstOrDefault(static choice =>
                   string.Equals(choice.Kind, "shop-proceed", StringComparison.OrdinalIgnoreCase)
                   && choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds))
               ?? observer.Choices.FirstOrDefault(static choice =>
                   string.Equals(choice.BindingKind, "shop-room", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(choice.BindingId, "proceed", StringComparison.OrdinalIgnoreCase)
                   && choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds));
    }

    private static IReadOnlyList<ObserverChoice> GetTypedShopChoices(ObserverSummary observer, string optionType, bool requireEnabled)
    {
        return observer.Choices
            .Where(choice =>
                (string.Equals(choice.Kind, $"shop-option:{optionType}", StringComparison.OrdinalIgnoreCase)
                 || (string.Equals(choice.BindingKind, "shop-option", StringComparison.OrdinalIgnoreCase)
                     && choice.SemanticHints.Any(hint => string.Equals(hint, $"shop-type:{optionType}", StringComparison.OrdinalIgnoreCase))))
                && (!requireEnabled || choice.Enabled != false)
                && HasUsableBounds(choice.ScreenBounds))
            .OrderBy(static choice => GetSortY(choice.ScreenBounds))
            .ThenBy(static choice => GetSortX(choice.ScreenBounds))
            .ToArray();
    }

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

    private static IReadOnlyList<string> ParseStringList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool HasUsableBounds(string? rawBounds)
        => TryParseBounds(rawBounds, out _);

    private static float GetSortX(string? rawBounds)
        => TryParseBounds(rawBounds, out var bounds) ? bounds.X : float.MaxValue;

    private static float GetSortY(string? rawBounds)
        => TryParseBounds(rawBounds, out var bounds) ? bounds.Y : float.MaxValue;

    private static bool TryParseBounds(string? rawBounds, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(rawBounds))
        {
            return false;
        }

        var parts = rawBounds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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
