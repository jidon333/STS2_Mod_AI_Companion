using System.Drawing;
using System.Globalization;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

sealed partial class AutoDecisionProvider
{
    private static string ResolveObserverScreen(ObserverSummary observer, string fallback)
        => DisplayControlFlowScreen(observer) ?? fallback;

    private static string? ResolveObserverScreen(ObserverSummary observer)
        => DisplayControlFlowScreen(observer);

    private static string ResolveObserverScreen(ObserverState observer, string fallback)
        => DisplayControlFlowScreen(observer) ?? fallback;

    private static string? ResolveObserverScreen(ObserverState observer)
        => DisplayControlFlowScreen(observer);

    private static string ResolveObserverCurrentScreen(ObserverSummary observer, string fallback)
        => ControlFlowCurrentScreen(observer) ?? fallback;

    private static string? ResolveObserverCurrentScreen(ObserverSummary observer)
        => ControlFlowCurrentScreen(observer);

    private static string ResolveObserverCurrentScreen(ObserverState observer, string fallback)
        => ControlFlowCurrentScreen(observer) ?? fallback;

    private static string? ResolveObserverCurrentScreen(ObserverState observer)
        => ControlFlowCurrentScreen(observer);

    private static string ResolveObserverVisibleScreen(ObserverSummary observer, string fallback)
        => ControlFlowVisibleScreen(observer) ?? fallback;

    private static string? ResolveObserverVisibleScreen(ObserverSummary observer)
        => ControlFlowVisibleScreen(observer);

    private static string ResolveObserverVisibleScreen(ObserverState observer, string fallback)
        => ControlFlowVisibleScreen(observer) ?? fallback;

    private static string? ResolveObserverVisibleScreen(ObserverState observer)
        => ControlFlowVisibleScreen(observer);

    private static GuiSmokeStepDecision CreateClickDecisionFromNode(GuiSmokeStepRequest request, ObserverActionNode node, string targetLabel)
    {
        if (!TryParseNodeBounds(node.ScreenBounds, out var bounds))
        {
            throw new InvalidOperationException($"Action node '{node.Label}' does not include screen bounds.");
        }
        if (!TryResolveNormalizedBounds(request.WindowBounds, node.ScreenBounds, bounds, out var normalizedX, out var normalizedY, out var boundsSource))
        {
            throw new InvalidOperationException($"Action node '{node.Label}' uses stale or off-window bounds '{node.ScreenBounds}'.");
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            targetLabel,
            $"Auto provider selected node '{node.Label}' using {boundsSource} bounds.",
            0.92,
            null,
            1200,
            true,
                null);
    }

    private static GuiSmokeStepDecision CreateClickDecisionFromNode(
        GuiSmokeStepRequest request,
        ObserverActionNode node,
        string targetLabel,
        string reason,
        double confidence,
        string? expectedScreen,
        int waitMs)
    {
        var decision = CreateClickDecisionFromNode(request, node, targetLabel);
        return decision with
        {
            Reason = reason,
            Confidence = confidence,
            ExpectedScreen = expectedScreen,
            WaitMs = waitMs,
        };
    }

    private static GuiSmokeStepDecision CreateCombatEnemyTargetDecisionFromNode(
        GuiSmokeStepRequest request,
        ObserverActionNode node,
        string targetLabel,
        int retryCount)
    {
        if (!TryParseNodeBounds(node.ScreenBounds, out var bounds))
        {
            throw new InvalidOperationException($"Enemy target node '{node.Label}' does not include screen bounds.");
        }

        var anchor = retryCount switch
        {
            <= 0 => (X: 0.50f, Y: 0.52f, Suffix: "body"),
            1 => (X: 0.50f, Y: 0.40f, Suffix: "upper-body"),
            2 => (X: 0.62f, Y: 0.48f, Suffix: "right-body"),
            _ => (X: 0.38f, Y: 0.48f, Suffix: "left-body"),
        };
        if (!TryResolveNormalizedPointFromBounds(request.WindowBounds, node.ScreenBounds, bounds, anchor.X, anchor.Y, out var normalizedX, out var normalizedY, out var boundsSource))
        {
            throw new InvalidOperationException($"Enemy target node '{node.Label}' uses stale or off-window bounds '{node.ScreenBounds}'.");
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            targetLabel,
            $"Auto provider selected enemy target '{node.Label}' using {boundsSource} bounds at {anchor.Suffix}.",
            retryCount == 0 ? 0.94 : 0.90,
            "combat",
            300,
            true,
            null);
    }

    private static GuiSmokeStepDecision CreateCombatEndTurnDecisionFromNode(
        GuiSmokeStepRequest request,
        ObserverActionNode node,
        string targetLabel)
    {
        var decision = CreateClickDecisionFromNode(request, node, targetLabel);
        return decision with
        {
            ExpectedScreen = "combat",
            WaitMs = 200,
        };
    }

    private static GuiSmokeStepDecision CreateCombatPressKeyDecision(
        string keyText,
        string targetLabel,
        string reason,
        double confidence,
        int waitMs)
    {
        return new GuiSmokeStepDecision(
            "act",
            "press-key",
            keyText,
            null,
            null,
            targetLabel,
            reason,
            confidence,
            "combat",
            waitMs,
            true,
            null);
    }

    private static GuiSmokeStepDecision CreateNonCombatPressKeyDecision(
        string keyText,
        string targetLabel,
        string reason,
        double confidence,
        string expectedScreen,
        int waitMs)
    {
        return new GuiSmokeStepDecision(
            "act",
            "press-key",
            keyText,
            null,
            null,
            targetLabel,
            reason,
            confidence,
            expectedScreen,
            waitMs,
            true,
            null);
    }

    private static string BuildCombatEnemyTargetLabel(ObserverActionNode node, int retryCount)
    {
        var baseLabel = string.IsNullOrWhiteSpace(node.Label)
            ? "combat enemy target"
            : $"combat enemy target {node.Label.Trim()}";
        return retryCount switch
        {
            <= 0 => baseLabel,
            1 => $"{baseLabel} recenter",
            2 => $"{baseLabel} alternate",
            _ => $"{baseLabel} fallback",
        };
    }

    private static GuiSmokeStepDecision CreateClickDecisionFromChoice(
        GuiSmokeStepRequest request,
        ObserverChoice choice,
        string targetLabel,
        string reason,
        double confidence,
        string expectedScreen,
        int waitMs)
    {
        if (!TryParseNodeBounds(choice.ScreenBounds, out var bounds))
        {
            throw new InvalidOperationException($"Observer choice '{choice.Label}' does not include screen bounds.");
        }
        if (!TryResolveNormalizedBounds(request.WindowBounds, choice.ScreenBounds, bounds, out var normalizedX, out var normalizedY, out _))
        {
            throw new InvalidOperationException($"Observer choice '{choice.Label}' uses stale or off-window bounds '{choice.ScreenBounds}'.");
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            targetLabel,
            reason,
            confidence,
            expectedScreen,
            waitMs,
            true,
            null);
    }

    private static bool HasUsableLogicalBounds(string? raw)
    {
        if (!TryParseNodeBounds(raw, out var bounds))
        {
            return false;
        }

        var centerX = bounds.X + bounds.Width / 2f;
        var centerY = bounds.Y + bounds.Height / 2f;
        return centerX >= 0f
               && centerY >= 0f
               && centerX <= 1920f
               && centerY <= 1080f;
    }

    private static bool TryResolveNormalizedBounds(
        WindowBounds windowBounds,
        string? rawBounds,
        RectangleF bounds,
        out double normalizedX,
        out double normalizedY,
        out string boundsSource)
    {
        return TryResolveNormalizedPointFromBounds(windowBounds, rawBounds, bounds, 0.5f, 0.5f, out normalizedX, out normalizedY, out boundsSource);
    }

    private static bool TryResolveNormalizedPointFromBounds(
        WindowBounds windowBounds,
        string? rawBounds,
        RectangleF bounds,
        float anchorX,
        float anchorY,
        out double normalizedX,
        out double normalizedY,
        out string boundsSource)
    {
        normalizedX = default;
        normalizedY = default;
        boundsSource = "unknown";

        var pointX = bounds.X + bounds.Width * anchorX;
        var pointY = bounds.Y + bounds.Height * anchorY;
        if (HasUsableLogicalBounds(rawBounds))
        {
            normalizedX = Math.Clamp(pointX / 1920f, 0d, 1d);
            normalizedY = Math.Clamp(pointY / 1080f, 0d, 1d);
            boundsSource = "logical";
            return true;
        }

        if (!IsBoundsInsideWindow(rawBounds, windowBounds))
        {
            return false;
        }

        normalizedX = Math.Clamp((pointX - windowBounds.X) / Math.Max(1d, windowBounds.Width), 0d, 1d);
        normalizedY = Math.Clamp((pointY - windowBounds.Y) / Math.Max(1d, windowBounds.Height), 0d, 1d);
        boundsSource = "window";
        return true;
    }

    private static GuiSmokeStepDecision CreateWaitDecision(string reason, string? expectedScreen)
    {
        return CreateWaitDecision(reason, expectedScreen, 2000);
    }

    private static GuiSmokeStepDecision CreateWaitDecision(string reason, string? expectedScreen, int waitMs)
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
            expectedScreen,
            waitMs,
            true,
            null);
    }

    private static GuiSmokeStepDecision CreateForegroundAwareNonCombatWaitDecision(
        GuiSmokeStepRequest request,
        string reason,
        bool allowFastForegroundWait = true)
    {
        var waitMs = allowFastForegroundWait && ShouldUseFastNonCombatForegroundWait(request)
            ? 400
            : 2000;
        return CreateWaitDecision(reason, ResolveObserverScreen(request.Observer), waitMs);
    }

    private static GuiSmokeStepDecision CreateForegroundAwareContractWaitDecision(
        GuiSmokeStepRequest request,
        string reason,
        string decisionRisk,
        bool allowFastForegroundWait = true)
    {
        return CreateForegroundAwareNonCombatWaitDecision(request, reason, allowFastForegroundWait) with
        {
            DecisionRisk = decisionRisk,
        };
    }

    private static bool ShouldUseFastNonCombatForegroundWait(GuiSmokeStepRequest request)
    {
        if (ControlFlowSceneReady(request.Observer) == false
            || !string.Equals(ControlFlowSceneStability(request.Observer), "stable", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (RootSceneTransitionObserverSignals.TryGetState(request.Observer)?.TransitionInProgress == true)
        {
            return false;
        }

        if (request.Phase is not nameof(GuiSmokePhase.HandleEvent)
            and not nameof(GuiSmokePhase.ChooseFirstNode)
            and not nameof(GuiSmokePhase.HandleRewards)
            and not nameof(GuiSmokePhase.HandleShop))
        {
            return false;
        }

        if (TryBuildCanonicalNonCombatSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath) is { } canonicalScene)
        {
            return canonicalScene.AllowsFastForegroundWait;
        }

        return NonCombatForegroundOwnership.Resolve(request.Observer) == NonCombatForegroundOwner.Map;
    }

    public static string? BuildHistoryMetadataForDecision(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        return AutoDecisionProvider.BuildRestSiteHistoryMetadataForDecision(request, decision)
               ?? CombatBarrierSupport.BuildHistoryMetadataForDecision(request, decision);
    }

    private static GuiSmokeStepDecision CreatePhaseWaitDecision(GuiSmokePhase phase, string reason, string? expectedScreen)
    {
        var waitMs = phase == GuiSmokePhase.HandleCombat ? 500 : 2000;
        return new GuiSmokeStepDecision(
            "wait",
            null,
            null,
            null,
            null,
            null,
            reason,
            0.60,
            expectedScreen,
            waitMs,
            true,
            null);
    }

    public static GuiSmokeStepDecision CreateCombatBarrierWaitDecision(CombatBarrierEvaluation barrier, CombatReleaseState releaseState, string? expectedScreen)
    {
        var waitMs = releaseState.LifecycleStage switch
        {
            CombatLifecycleStage.EnemyTurn => 1600,
            CombatLifecycleStage.EndTurnTransit => 1200,
            CombatLifecycleStage.PlayerReopenPending => 1200,
            _ => CombatBarrierPolicy.HandleCombatWaitMinimumMs,
        };
        return new GuiSmokeStepDecision(
            "wait",
            null,
            null,
            null,
            null,
            null,
            $"combat barrier wait barrier={barrier.Kind} source={barrier.SourceAction ?? "null"} reason={barrier.Reason}",
            0.66,
            expectedScreen,
            waitMs,
            true,
            null);
    }

    private static bool TryParseNodeBounds(string? raw, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
        {
            return false;
        }

        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
        {
            return false;
        }

        if (width <= 0 || height <= 0)
        {
            return false;
        }

        bounds = new RectangleF(x, y, width, height);
        return true;
    }
}
