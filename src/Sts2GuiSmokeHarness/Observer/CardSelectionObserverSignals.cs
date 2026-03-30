using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

static class CardSelectionObserverSignals
{
    public static string? TryGetMetaValue(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value) ? value : null;
    }

    public static CardSelectionSubtypeState? TryGetState(ObserverSummary observer)
    {
        var screenType = NormalizeScreenType(
            TryGetMetaValue(observer, "cardSelectionScreenType")
            ?? InferScreenTypeFromObserver(observer));
        if (screenType is null)
        {
            return null;
        }

        return new CardSelectionSubtypeState(
            screenType,
            TryGetMetaValue(observer, "cardSelectionPrompt"),
            TryGetMetaInt(observer, "cardSelectionMinSelect"),
            TryGetMetaInt(observer, "cardSelectionMaxSelect"),
            TryGetMetaInt(observer, "cardSelectionSelectedCount") ?? 0,
            TryGetMetaBool(observer, "cardSelectionRequireManualConfirmation"),
            TryGetMetaBool(observer, "cardSelectionCancelable"),
            TryGetMetaBool(observer, "cardSelectionPreviewVisible") == true,
            TryGetMetaBool(observer, "cardSelectionMainConfirmEnabled") == true,
            TryGetMetaBool(observer, "cardSelectionPreviewConfirmEnabled") == true,
            TryGetMetaValue(observer, "cardSelectionPreviewMode"),
            ParseStringList(TryGetMetaValue(observer, "cardSelectionSelectedCardIds")),
            TryGetMetaValue(observer, "cardSelectionRootType") ?? TryGetMetaValue(observer, "rootTypeSummary"));
    }

    public static bool IsCardSelectionState(ObserverSummary observer)
        => TryGetState(observer) is not null;

    public static bool IsTransformState(ObserverSummary observer)
        => string.Equals(TryGetState(observer)?.ScreenType, "transform", StringComparison.OrdinalIgnoreCase);

    public static bool IsDeckRemoveState(ObserverSummary observer)
        => string.Equals(TryGetState(observer)?.ScreenType, "deck-remove", StringComparison.OrdinalIgnoreCase);

    public static bool IsUpgradeState(ObserverSummary observer)
        => string.Equals(TryGetState(observer)?.ScreenType, "upgrade", StringComparison.OrdinalIgnoreCase);

    public static bool IsRewardPickState(ObserverSummary observer)
        => string.Equals(TryGetState(observer)?.ScreenType, "reward-pick", StringComparison.OrdinalIgnoreCase);

    public static bool IsSimpleSelectState(ObserverSummary observer)
        => string.Equals(TryGetState(observer)?.ScreenType, "simple-select", StringComparison.OrdinalIgnoreCase);

    public static bool IsNonRewardCountConfirmFamily(ObserverSummary observer)
    {
        var screenType = TryGetState(observer)?.ScreenType;
        return string.Equals(screenType, "transform", StringComparison.OrdinalIgnoreCase)
               || string.Equals(screenType, "deck-remove", StringComparison.OrdinalIgnoreCase)
               || string.Equals(screenType, "upgrade", StringComparison.OrdinalIgnoreCase)
               || string.Equals(screenType, "simple-select", StringComparison.OrdinalIgnoreCase)
               || string.Equals(screenType, "bundle-select", StringComparison.OrdinalIgnoreCase)
               || string.Equals(screenType, "relic-select", StringComparison.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<ObserverChoice> GetCardChoices(ObserverSummary observer, CardSelectionSubtypeState state)
    {
        var explicitChoices = observer.Choices
            .Where(choice => IsSubtypeCardChoice(choice, state.ScreenType))
            .OrderBy(static choice => choice.Enabled == false ? 1 : 0)
            .ThenBy(static choice => GetSortX(choice.ScreenBounds))
            .ThenBy(static choice => GetSortY(choice.ScreenBounds))
            .ToArray();
        return explicitChoices;
    }

    public static ObserverChoice? TryGetConfirmChoice(ObserverSummary observer, CardSelectionSubtypeState state)
    {
        var confirmKind = state.ScreenType switch
        {
            "transform" => "transform-confirm",
            "deck-remove" => "deck-remove-confirm",
            "upgrade" => "upgrade-confirm",
            "simple-select" => "simple-select-confirm",
            "bundle-select" => "bundle-select-confirm",
            _ => null,
        };
        if (confirmKind is null)
        {
            return null;
        }

        var explicitConfirm = observer.Choices
            .Where(choice => string.Equals(choice.Kind, confirmKind, StringComparison.OrdinalIgnoreCase))
            .OrderBy(static choice => choice.BindingId == "preview" ? 0 : 1)
            .ThenBy(static choice => choice.Enabled == false ? 1 : 0)
            .FirstOrDefault();
        if (explicitConfirm is not null)
        {
            return explicitConfirm;
        }

        var synthesizedConfirm = TryCreateMetaBackedConfirmChoice(observer, state, confirmKind);
        if (synthesizedConfirm is not null)
        {
            return synthesizedConfirm;
        }

        if (state.ScreenType is "simple-select" or "bundle-select")
        {
            return null;
        }

        return observer.Choices
            .Where(static choice => IsConfirmLabel(choice.Label))
            .OrderBy(static choice => choice.Enabled == false ? 1 : 0)
            .ThenByDescending(static choice => choice.BindingId == "preview" ? 1 : 0)
            .ThenBy(static choice => GetSortY(choice.ScreenBounds))
            .ThenBy(static choice => GetSortX(choice.ScreenBounds))
            .FirstOrDefault();
    }

    public static bool HasActionableDecisionSurface(ObserverSummary observer, CardSelectionSubtypeState state)
    {
        if (IsConfirmReady(state)
            && TryGetConfirmChoice(observer, state) is { Enabled: not false, ScreenBounds.Length: > 0 })
        {
            return true;
        }

        return GetCardChoices(observer, state)
            .Any(choice => choice.Enabled != false && !string.IsNullOrWhiteSpace(choice.ScreenBounds));
    }

    public static bool IsSubtypeCardChoice(ObserverChoice choice, string screenType)
    {
        var expectedKind = screenType switch
        {
            "transform" => "transform-card",
            "deck-remove" => "deck-remove-card",
            "upgrade" => "upgrade-card",
            "reward-pick" => "reward-pick-card",
            "simple-select" => "simple-select-card",
            "bundle-select" => "bundle-select-card",
            "relic-select" => "relic-select-card",
            _ => null,
        };
        if (expectedKind is not null && string.Equals(choice.Kind, expectedKind, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return choice.SemanticHints.Any(hint => string.Equals(hint, $"card-selection:{screenType}", StringComparison.OrdinalIgnoreCase))
               && !choice.SemanticHints.Any(static hint => string.Equals(hint, "confirm-mode:main", StringComparison.OrdinalIgnoreCase)
                                                           || string.Equals(hint, "confirm-mode:preview", StringComparison.OrdinalIgnoreCase));
    }

    private static ObserverChoice? TryCreateMetaBackedConfirmChoice(
        ObserverSummary observer,
        CardSelectionSubtypeState state,
        string confirmKind)
    {
        if (!string.Equals(state.ScreenType, "upgrade", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var confirmBounds = TryGetMetaValue(observer, "restSiteUpgradeConfirmBounds");
        if (string.IsNullOrWhiteSpace(confirmBounds))
        {
            return null;
        }

        var previewMode = state.PreviewVisible
            || string.Equals(TryGetMetaValue(observer, "restSiteUpgradePreviewMode"), "single", StringComparison.OrdinalIgnoreCase)
            || string.Equals(TryGetMetaValue(observer, "restSiteUpgradePreviewMode"), "multi", StringComparison.OrdinalIgnoreCase);
        var enabled = previewMode
            ? state.PreviewConfirmEnabled || string.Equals(TryGetMetaValue(observer, "restSiteUpgradeConfirmEnabled"), "true", StringComparison.OrdinalIgnoreCase)
            : state.MainConfirmEnabled;

        return new ObserverChoice(
            confirmKind,
            "Confirm",
            confirmBounds,
            previewMode ? "preview-confirm" : "main-confirm")
        {
            BindingKind = "card-selection-confirm",
            BindingId = previewMode ? "preview" : "main",
            SemanticHints = new[] { $"card-selection:{state.ScreenType}", $"confirm-mode:{(previewMode ? "preview" : "main")}" },
            Enabled = enabled,
        };
    }

    public static bool IsSelectedCardChoice(ObserverChoice choice)
    {
        return choice.SemanticHints.Any(static hint => string.Equals(hint, "selected-card", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsConfirmReady(CardSelectionSubtypeState state)
    {
        if (state.PreviewVisible && state.PreviewConfirmEnabled)
        {
            return true;
        }

        if (state.ScreenType is "simple-select" or "bundle-select"
            && state.MainConfirmEnabled)
        {
            return true;
        }

        return state.ScreenType is "transform" or "deck-remove"
               && !state.PreviewVisible
               && state.MinSelect != state.MaxSelect
               && state.MinSelect is not null
               && state.SelectedCount >= state.MinSelect
               && state.MainConfirmEnabled;
    }

    private static string? InferScreenTypeFromObserver(ObserverSummary observer)
    {
        var rootTypeSummary = observer.Meta.TryGetValue("rootTypeSummary", out var rawRootTypeSummary)
            ? rawRootTypeSummary
            : null;
        var activeScreenType = TryGetMetaValue(observer, "activeScreenType");
        var rawCurrentActiveScreenType = TryGetMetaValue(observer, "rawCurrentActiveScreenType");
        var rawTopOverlayType = TryGetMetaValue(observer, "rawTopOverlayType");

        if (string.Equals(observer.ChoiceExtractorPath, "card-selection-transform", StringComparison.OrdinalIgnoreCase)
            || string.Equals(ControlFlowCurrentScreen(observer), "transform", StringComparison.OrdinalIgnoreCase)
            || rootTypeSummary?.Contains("NDeckTransformSelectScreen", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "transform";
        }

        if (string.Equals(observer.ChoiceExtractorPath, "card-selection-deck-remove", StringComparison.OrdinalIgnoreCase)
            || rootTypeSummary?.Contains("NDeckCardSelectScreen", StringComparison.OrdinalIgnoreCase) == true
            || activeScreenType?.Contains("NDeckCardSelectScreen", StringComparison.OrdinalIgnoreCase) == true
            || rawCurrentActiveScreenType?.Contains("NDeckCardSelectScreen", StringComparison.OrdinalIgnoreCase) == true
            || rawTopOverlayType?.Contains("NDeckCardSelectScreen", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "deck-remove";
        }

        if (string.Equals(observer.ChoiceExtractorPath, "card-selection-upgrade", StringComparison.OrdinalIgnoreCase)
            || rootTypeSummary?.Contains("NDeckUpgradeSelectScreen", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "upgrade";
        }

        if (string.Equals(observer.ChoiceExtractorPath, "card-selection-reward-pick", StringComparison.OrdinalIgnoreCase)
            || rootTypeSummary?.Contains("NCardRewardSelectionScreen", StringComparison.OrdinalIgnoreCase) == true
            || activeScreenType?.Contains("NCardRewardSelectionScreen", StringComparison.OrdinalIgnoreCase) == true
            || rawCurrentActiveScreenType?.Contains("NCardRewardSelectionScreen", StringComparison.OrdinalIgnoreCase) == true
            || rawTopOverlayType?.Contains("NCardRewardSelectionScreen", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "reward-pick";
        }

        if (rootTypeSummary?.Contains("NChooseACardSelectionScreen", StringComparison.OrdinalIgnoreCase) == true
            || activeScreenType?.Contains("NChooseACardSelectionScreen", StringComparison.OrdinalIgnoreCase) == true
            || rawCurrentActiveScreenType?.Contains("NChooseACardSelectionScreen", StringComparison.OrdinalIgnoreCase) == true
            || rawTopOverlayType?.Contains("NChooseACardSelectionScreen", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "reward-pick";
        }

        if (string.Equals(observer.ChoiceExtractorPath, "card-selection-simple-select", StringComparison.OrdinalIgnoreCase)
            || rootTypeSummary?.Contains("NSimpleCardSelectScreen", StringComparison.OrdinalIgnoreCase) == true
            || activeScreenType?.Contains("NSimpleCardSelectScreen", StringComparison.OrdinalIgnoreCase) == true
            || rawCurrentActiveScreenType?.Contains("NSimpleCardSelectScreen", StringComparison.OrdinalIgnoreCase) == true
            || rawTopOverlayType?.Contains("NSimpleCardSelectScreen", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "simple-select";
        }

        if (string.Equals(observer.ChoiceExtractorPath, "card-selection-bundle-select", StringComparison.OrdinalIgnoreCase)
            || rootTypeSummary?.Contains("NChooseABundleSelectionScreen", StringComparison.OrdinalIgnoreCase) == true
            || activeScreenType?.Contains("NChooseABundleSelectionScreen", StringComparison.OrdinalIgnoreCase) == true
            || rawCurrentActiveScreenType?.Contains("NChooseABundleSelectionScreen", StringComparison.OrdinalIgnoreCase) == true
            || rawTopOverlayType?.Contains("NChooseABundleSelectionScreen", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "bundle-select";
        }

        if (string.Equals(observer.ChoiceExtractorPath, "card-selection-relic-select", StringComparison.OrdinalIgnoreCase)
            || rootTypeSummary?.Contains("NChooseARelicSelection", StringComparison.OrdinalIgnoreCase) == true
            || activeScreenType?.Contains("NChooseARelicSelection", StringComparison.OrdinalIgnoreCase) == true
            || rawCurrentActiveScreenType?.Contains("NChooseARelicSelection", StringComparison.OrdinalIgnoreCase) == true
            || rawTopOverlayType?.Contains("NChooseARelicSelection", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "relic-select";
        }

        return null;
    }

    private static string? NormalizeScreenType(string? value)
    {
        return value switch
        {
            "transform" => "transform",
            "deck-remove" => "deck-remove",
            "upgrade" => "upgrade",
            "reward-pick" => "reward-pick",
            "simple-select" => "simple-select",
            "bundle-select" => "bundle-select",
            "relic-select" => "relic-select",
            "unknown-card-select" => "unknown-card-select",
            _ => null,
        };
    }

    private static bool? TryGetMetaBool(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    private static int? TryGetMetaInt(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static IReadOnlyList<string> ParseStringList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool IsConfirmLabel(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("Confirm", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("확인", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("선택", StringComparison.OrdinalIgnoreCase));
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
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
        {
            return false;
        }

        bounds = new RectangleF(x, y, width, height);
        return width > 0f && height > 0f;
    }
}
