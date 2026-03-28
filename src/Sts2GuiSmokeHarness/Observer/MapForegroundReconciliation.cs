using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;

static class MapForegroundReconciliation
{
    public static bool HasMapForegroundOwnership(ObserverState observer, IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(observer.Summary))
        {
            return false;
        }

        if (GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(observer.Summary))
        {
            return false;
        }

        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, null, history);
        var mapVisible = GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(observer)
                         || string.Equals(observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase);
        return mapVisible
               && !HasStrongerForegroundModalAuthority(observer, history, eventScene);
    }

    private static bool HasStrongerForegroundModalAuthority(
        ObserverState observer,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        EventSceneState eventScene)
    {
        if (CardSelectionObserverSignals.TryGetState(observer.Summary) is not null)
        {
            return true;
        }

        if (TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary))
        {
            return true;
        }

        if (LooksLikeInspectOverlayForeground(observer))
        {
            return true;
        }

        if (LooksLikeRewardForeground(observer))
        {
            return true;
        }

        if (HasExplicitEventProgressionForeground(observer)
            || eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.Active)
        {
            return true;
        }

        return HasRecentMapOpenAftermath(history) && !IsMapOnlyForegroundState(observer, eventScene);
    }

    private static bool IsMapOnlyForegroundState(ObserverState observer, EventSceneState eventScene)
    {
        return CardSelectionObserverSignals.TryGetState(observer.Summary) is null
               && !TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary)
               && !LooksLikeInspectOverlayForeground(observer)
               && !LooksLikeRewardForeground(observer)
               && !HasExplicitEventProgressionForeground(observer)
               && !(eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.Active);
    }

    private static bool LooksLikeInspectOverlayForeground(ObserverState observer)
    {
        return observer.Choices.Any(static choice =>
                   IsOverlayLikeLabel(choice.Label)
                   || string.Equals(choice.Kind, "inspect", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(choice.BindingKind, "overlay", StringComparison.OrdinalIgnoreCase))
               || observer.ActionNodes.Any(static node =>
                   IsOverlayLikeLabel(node.Label)
                   || node.Kind.Contains("overlay", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeRewardForeground(ObserverState observer)
    {
        return RewardObserverSignals.IsRewardAuthorityActive(observer.Summary);
    }

    private static bool HasExplicitEventProgressionForeground(ObserverState observer)
    {
        if (NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer))
        {
            return false;
        }

        var eventAuthority = string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(observer.VisibleScreen, "event", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(observer.ChoiceExtractorPath, "event", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(observer.ChoiceExtractorPath, "room-event", StringComparison.OrdinalIgnoreCase);
        if (!eventAuthority)
        {
            return false;
        }

        return observer.ActionNodes.Any(static node =>
                   node.Actionable
                   && (IsProceedLikeText(node.Label)
                       || node.Kind.Contains("proceed", StringComparison.OrdinalIgnoreCase)
                       || node.Kind.Contains("continue", StringComparison.OrdinalIgnoreCase)))
               || observer.Choices.Any(static choice =>
                   IsProceedLikeText(choice.Label)
                   && !string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsOverlayLikeLabel(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("Back", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Close", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("닫기", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("취소", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Cancel", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsProceedLikeText(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("Proceed", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Continue", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("진행", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("계속", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasRecentMapOpenAftermath(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        static bool IsMapOpenAftermathTarget(string? targetLabel)
        {
            return string.Equals(targetLabel, "treasure proceed", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(targetLabel, "reward proceed", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(targetLabel, "event progression choice", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(targetLabel, "transform confirm", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(targetLabel, "deck remove confirm", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(targetLabel, "upgrade confirm", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(targetLabel, "rest site: smith confirm", StringComparison.OrdinalIgnoreCase);
        }

        for (var index = history.Count - 1; index >= 0 && index >= history.Count - 4; index -= 1)
        {
            var entry = history[index];
            if (IsMapOpenAftermathTarget(entry.TargetLabel))
            {
                return true;
            }

            if (!string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(entry.Action, "observer-accepted", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(entry.Action, "recapture-required", StringComparison.OrdinalIgnoreCase)
                && !entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return false;
    }
}
