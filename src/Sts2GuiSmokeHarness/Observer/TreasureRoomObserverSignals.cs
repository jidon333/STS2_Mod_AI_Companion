using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;

static class TreasureRoomObserverSignals
{
    public static TreasureRoomSubtypeState? TryGetState(ObserverSummary observer)
    {
        var sharedRelicPickingActive = TryGetMetaBool(observer, "sharedRelicPickingActive");
        var relicCollectionOpen = TryGetMetaBool(observer, "treasureRelicCollectionOpen");
        var mapScreenOpen = TryGetMetaBool(observer, "mapScreenOpen");
        var genericChestVisible = HasGenericChestChoice(observer);
        var explicitTreasureSurface = HasExplicitTreasureSurface(observer, genericChestVisible);
        var explicitChestClickable = TryGetMetaBool(observer, "treasureChestClickable");
        var explicitProceedEnabled = TryGetMetaBool(observer, "treasureProceedEnabled");
        var chestOpened = TryGetMetaBool(observer, "treasureChestOpened") == true;
        var staleAfterProceedResidue = mapScreenOpen == true
                                       && sharedRelicPickingActive == false
                                       && relicCollectionOpen == false
                                       && explicitProceedEnabled != true
                                       && chestOpened;
        if (HasConflictingForegroundRoomContext(observer, explicitTreasureSurface))
        {
            return null;
        }

        var detected = TryGetMetaBool(observer, "treasureRoomDetected") == true
                       || HasTreasureRoomContext(observer)
                       || explicitTreasureSurface;
        if (!detected || staleAfterProceedResidue)
        {
            return null;
        }

        return new TreasureRoomSubtypeState(
            RoomDetected: true,
            ChestClickable: explicitChestClickable == true || genericChestVisible,
            ChestOpened: chestOpened,
            RelicHolderCount: TryGetMetaInt(observer, "treasureRelicHolderCount") ?? 0,
            VisibleRelicHolderCount: TryGetMetaInt(observer, "treasureVisibleRelicHolderCount") ?? 0,
            EnabledRelicHolderCount: TryGetMetaInt(observer, "treasureEnabledRelicHolderCount") ?? 0,
            ProceedEnabled: explicitProceedEnabled == true,
            InspectOverlayVisible: TryGetMetaBool(observer, "treasureInspectOverlayVisible") == true
                                   || observer.CurrentChoices.Any(static label => IsOverlayLabel(label)),
            RelicHolderIds: ParseStringList(TryGetMetaValue(observer, "treasureRelicHolderIds")),
            RootType: TryGetMetaValue(observer, "treasureRoomRootType"));
    }

    public static bool IsTreasureAuthorityActive(ObserverSummary observer)
        => TryGetState(observer) is { RoomDetected: true };

    public static bool LooksLikeTreasureState(ObserverSummary observer)
    {
        var explicitTreasureSurface = HasExplicitTreasureSurface(observer);
        if (HasConflictingForegroundRoomContext(observer, explicitTreasureSurface))
        {
            return false;
        }

        if (IsTreasureAuthorityActive(observer)
            || HasTreasureRoomContext(observer)
            || explicitTreasureSurface)
        {
            return true;
        }

        return observer.CurrentChoices.Any(static label =>
            label.Contains("Chest", StringComparison.OrdinalIgnoreCase)
            || label.Contains("\uC0C1\uC790", StringComparison.OrdinalIgnoreCase));
    }

    public static string[] BuildAllowedActions(TreasureRoomSubtypeState state)
    {
        if (state.InspectOverlayVisible)
        {
            return new[] { "click treasure overlay back", "wait" };
        }

        if (!state.ChestOpened && state.ChestClickable)
        {
            return new[] { "click treasure chest", "wait" };
        }

        if (state.ProceedEnabled)
        {
            return new[] { "click treasure proceed", "wait" };
        }

        if (state.EnabledRelicHolderCount > 0)
        {
            return new[] { "click treasure relic holder", "wait" };
        }

        return new[] { "wait" };
    }

    public static ObserverChoice? TryGetChestChoice(ObserverSummary observer)
    {
        return observer.Choices.FirstOrDefault(static choice =>
                   string.Equals(choice.Kind, "treasure-chest", StringComparison.OrdinalIgnoreCase)
                   && choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds))
               ?? observer.Choices.FirstOrDefault(static choice =>
                   string.Equals(choice.BindingKind, "treasure-room", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(choice.BindingId, "chest", StringComparison.OrdinalIgnoreCase)
                   && choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds))
               ?? observer.Choices.FirstOrDefault(static choice =>
                   string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
                   && choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds)
                   && (choice.Label.Contains("Chest", StringComparison.OrdinalIgnoreCase)
                       || choice.Label.Contains("상자", StringComparison.OrdinalIgnoreCase)));
    }

    public static IReadOnlyList<ObserverChoice> GetRelicHolderChoices(ObserverSummary observer)
    {
        return observer.Choices
            .Where(static choice =>
                string.Equals(choice.Kind, "treasure-relic-holder", StringComparison.OrdinalIgnoreCase)
                || (string.Equals(choice.BindingKind, "treasure-room", StringComparison.OrdinalIgnoreCase)
                    && choice.SemanticHints.Any(static hint => string.Equals(hint, "treasure-relic-holder", StringComparison.OrdinalIgnoreCase))))
            .Where(static choice => choice.Enabled != false && HasUsableBounds(choice.ScreenBounds))
            .OrderBy(static choice => GetSortY(choice.ScreenBounds))
            .ThenBy(static choice => GetSortX(choice.ScreenBounds))
            .ToArray();
    }

    public static ObserverChoice? TryGetProceedChoice(ObserverSummary observer)
    {
        return observer.Choices.FirstOrDefault(static choice =>
                   string.Equals(choice.Kind, "treasure-proceed", StringComparison.OrdinalIgnoreCase)
                   && choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds))
               ?? observer.Choices.FirstOrDefault(static choice =>
                   string.Equals(choice.BindingKind, "treasure-room", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(choice.BindingId, "proceed", StringComparison.OrdinalIgnoreCase)
                   && choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds));
    }

    public static ObserverChoice? TryGetOverlayBackChoice(ObserverSummary observer)
    {
        return observer.Choices.FirstOrDefault(static choice =>
            IsOverlayLabel(choice.Label) && HasUsableBounds(choice.ScreenBounds));
    }

    private static string? TryGetMetaValue(ObserverSummary observer, string key)
        => observer.Meta.TryGetValue(key, out var value) ? value : null;

    private static bool HasTreasureRoomContext(ObserverSummary observer)
    {
        return string.Equals(observer.EncounterKind, "Treasure", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "treasure", StringComparison.OrdinalIgnoreCase)
               || IsTreasureRoomType(TryGetMetaValue(observer, "rawRoomTypeValue"))
               || IsTreasureRoomType(TryGetMetaValue(observer, "rawCurrentRunRoomType"))
               || IsTreasureRoomType(TryGetMetaValue(observer, "rawCurrentRunRoomSceneType"))
               || IsTreasureRoomType(TryGetMetaValue(observer, "currentRunRoomType"))
               || IsTreasureRoomType(TryGetMetaValue(observer, "currentRunRoomSceneType"))
               || (observer.Meta.TryGetValue("rootTypeSummary", out var rootTypeSummary)
                   && rootTypeSummary?.Contains("NTreasureRoom", StringComparison.OrdinalIgnoreCase) == true);
    }

    private static bool HasConflictingForegroundRoomContext(ObserverSummary observer, bool explicitTreasureSurface)
    {
        if (explicitTreasureSurface
            || string.Equals(observer.EncounterKind, "Treasure", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return IsForeignRoomScreen(observer.CurrentScreen)
               || IsForeignRoomScreen(observer.VisibleScreen)
               || IsForeignRoomExtractor(observer.ChoiceExtractorPath)
               || HasForeignForegroundRoot(observer);
    }

    private static bool HasExplicitTreasureSurface(ObserverSummary observer)
    {
        return HasExplicitTreasureSurface(observer, HasGenericChestChoice(observer));
    }

    private static bool HasExplicitTreasureSurface(ObserverSummary observer, bool genericChestVisible)
    {
        return genericChestVisible
               || observer.ActionNodes.Any(static node =>
                   node.Actionable
                   && HasUsableBounds(node.ScreenBounds)
                   && (string.Equals(node.Kind, "treasure-chest", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(node.Kind, "treasure-relic-holder", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(node.Kind, "treasure-proceed", StringComparison.OrdinalIgnoreCase)
                       || node.SemanticHints.Any(static hint => string.Equals(hint, "treasure-room", StringComparison.OrdinalIgnoreCase))))
               || observer.Choices.Any(static choice =>
                   choice.Enabled != false
                   && HasUsableBounds(choice.ScreenBounds)
                   && (string.Equals(choice.Kind, "treasure-chest", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(choice.Kind, "treasure-relic-holder", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(choice.Kind, "treasure-proceed", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(choice.BindingKind, "treasure-room", StringComparison.OrdinalIgnoreCase)));
    }

    private static bool HasGenericChestChoice(ObserverSummary observer)
    {
        return observer.Choices.Any(static choice =>
            string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
            && choice.Enabled != false
            && HasUsableBounds(choice.ScreenBounds)
            && (choice.Label.Contains("Chest", StringComparison.OrdinalIgnoreCase)
                || choice.Label.Contains("상자", StringComparison.OrdinalIgnoreCase)));
    }

    private static bool? TryGetMetaBool(ObserverSummary observer, string key)
        => observer.Meta.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed)
            ? parsed
            : null;

    private static int? TryGetMetaInt(ObserverSummary observer, string key)
        => observer.Meta.TryGetValue(key, out var value) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static bool IsTreasureRoomType(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
               && value.Contains("Treasure", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsForeignRoomScreen(string? screen)
    {
        return string.Equals(screen, "event", StringComparison.OrdinalIgnoreCase)
               || string.Equals(screen, "shop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(screen, "rewards", StringComparison.OrdinalIgnoreCase)
               || string.Equals(screen, "rest-site", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsForeignRoomExtractor(string? extractor)
    {
        return string.Equals(extractor, "event", StringComparison.OrdinalIgnoreCase)
               || string.Equals(extractor, "shop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(extractor, "reward", StringComparison.OrdinalIgnoreCase)
               || string.Equals(extractor, "rest-site", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasForeignForegroundRoot(ObserverSummary observer)
    {
        return observer.Meta.TryGetValue("rootTypeSummary", out var rootTypeSummary)
               && !string.IsNullOrWhiteSpace(rootTypeSummary)
               && (rootTypeSummary.Contains("NEventRoom", StringComparison.OrdinalIgnoreCase)
                   || rootTypeSummary.Contains("NShopRoom", StringComparison.OrdinalIgnoreCase)
                   || rootTypeSummary.Contains("NShopScreen", StringComparison.OrdinalIgnoreCase)
                   || rootTypeSummary.Contains("NRewardScreen", StringComparison.OrdinalIgnoreCase)
                   || rootTypeSummary.Contains("NRestRoom", StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> ParseStringList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool IsOverlayLabel(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasUsableBounds(string? rawBounds)
    {
        return TryParseBounds(rawBounds, out _);
    }

    private static float GetSortX(string? rawBounds)
    {
        return TryParseBounds(rawBounds, out var bounds) ? bounds.X : float.MaxValue;
    }

    private static float GetSortY(string? rawBounds)
    {
        return TryParseBounds(rawBounds, out var bounds) ? bounds.Y : float.MaxValue;
    }

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
