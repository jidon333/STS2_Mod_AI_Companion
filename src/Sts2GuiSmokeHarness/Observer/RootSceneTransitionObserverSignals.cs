using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

static class RootSceneTransitionObserverSignals
{
    public static RootSceneTransitionState? TryGetState(ObserverSummary observer)
    {
        var rootSceneCurrentType = TryGetMetaValue(observer, "rootSceneCurrentType")
                                   ?? TryGetMetaValue(observer, "currentSceneType");
        var transitionInProgress = TryGetMetaBool(observer, "transitionInProgress") == true;
        var rootSceneIsMainMenuHint = TryGetMetaBool(observer, "rootSceneIsMainMenu");
        var rootSceneIsRunHint = TryGetMetaBool(observer, "rootSceneIsRun");
        var rootSceneIsMainMenu = rootSceneIsMainMenuHint
                                  ?? (rootSceneCurrentType?.Contains("MainMenu", StringComparison.OrdinalIgnoreCase) == true);
        var rootSceneIsRun = rootSceneIsRunHint
                             ?? (rootSceneCurrentType?.Contains(".NRun", StringComparison.OrdinalIgnoreCase) == true
                                 || rootSceneCurrentType?.EndsWith(".NRun", StringComparison.OrdinalIgnoreCase) == true);
        var currentRunNodePresent = TryGetMetaBool(observer, "currentRunNodePresent") == true;
        var currentRunRoomType = TryGetMetaValue(observer, "currentRunRoomType");
        var currentRunRoomSceneType = TryGetMetaValue(observer, "currentRunRoomSceneType");

        if (!transitionInProgress
            && string.IsNullOrWhiteSpace(rootSceneCurrentType)
            && rootSceneIsMainMenuHint is null
            && rootSceneIsRunHint is null
            && !currentRunNodePresent
            && string.IsNullOrWhiteSpace(currentRunRoomType)
            && string.IsNullOrWhiteSpace(currentRunRoomSceneType))
        {
            return null;
        }

        return new RootSceneTransitionState(
            transitionInProgress,
            rootSceneCurrentType,
            rootSceneIsMainMenu == true,
            rootSceneIsRun == true,
            currentRunNodePresent,
            currentRunRoomType,
            currentRunRoomSceneType);
    }

    public static bool ShouldHoldRunLoadBoundary(ObserverSummary observer)
    {
        var state = TryGetState(observer);
        if (state is null)
        {
            return false;
        }

        if (state.TransitionInProgress)
        {
            return true;
        }

        if (state.RootSceneIsRun || state.CurrentRunNodePresent)
        {
            return false;
        }

        if (WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(observer))
        {
            return false;
        }

        if (state.RootSceneIsMainMenu)
        {
            return true;
        }

        return MatchesControlFlowScreen(observer, "main-menu")
               || MatchesControlFlowScreen(observer, "character-select")
               || MatchesControlFlowScreen(observer, "singleplayer-submenu");
    }

    public static bool ShouldTreatCaptureAsTransitionWait(GuiSmokePhase phase, ObserverSummary observer)
    {
        var state = TryGetState(observer);
        if (state?.TransitionInProgress == true)
        {
            return true;
        }

        return phase == GuiSmokePhase.WaitRunLoad && ShouldHoldRunLoadBoundary(observer);
    }

    private static string? TryGetMetaValue(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value) ? value : null;
    }

    private static bool? TryGetMetaBool(ObserverSummary observer, string key)
    {
        return observer.Meta.TryGetValue(key, out var value)
               && bool.TryParse(value, out var parsed)
            ? parsed
            : null;
    }
}
