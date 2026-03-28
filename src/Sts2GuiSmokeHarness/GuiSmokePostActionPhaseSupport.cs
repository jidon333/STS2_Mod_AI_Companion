using System;

static class GuiSmokePostActionPhaseSupport
{
    internal static GuiSmokePhase GetPostEnterRunPhase(GuiSmokeStepDecision decision)
    {
        return string.Equals(decision.TargetLabel, "continue", StringComparison.OrdinalIgnoreCase)
            ? GuiSmokePhase.WaitRunLoad
            : GuiSmokePhase.WaitCharacterSelect;
    }

    internal static GuiSmokePhase GetPostRewardPhase(GuiSmokeStepDecision decision)
    {
        if (IsRewardReleaseTarget(decision.TargetLabel))
        {
            return GuiSmokePhase.WaitMap;
        }

        if (KeepsCurrentRoomPhase(decision.TargetLabel))
        {
            return GuiSmokePhase.HandleRewards;
        }

        if (IsReachableNodeTarget(decision.TargetLabel))
        {
            return GuiSmokePhase.WaitPostMapNodeRoom;
        }

        return GuiSmokePhase.WaitMap;
    }

    internal static GuiSmokePhase GetPostChooseFirstNodePhase(GuiSmokeStepDecision decision)
    {
        return IsReachableNodeTarget(decision.TargetLabel)
            ? GuiSmokePhase.WaitPostMapNodeRoom
            : GuiSmokePhase.WaitMap;
    }

    internal static GuiSmokePhase GetPostHandleEventPhase(GuiSmokeStepDecision decision)
    {
        if (IsAncientEventCompletionTarget(decision.TargetLabel))
        {
            return GuiSmokePhase.WaitEventRelease;
        }

        if (KeepsCurrentRoomPhase(decision.TargetLabel))
        {
            return GuiSmokePhase.HandleEvent;
        }

        return IsRoomProgressTarget(decision.TargetLabel)
            ? GetPostChooseFirstNodePhase(decision)
            : GuiSmokePhase.HandleEvent;
    }

    internal static bool IsReachableNodeTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "first reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsAncientEventCompletionTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "ancient event completion", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsRoomProgressTarget(string? targetLabel)
    {
        return IsReachableNodeTarget(targetLabel)
               || string.Equals(targetLabel, "treasure chest center", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "treasure chest", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "treasure relic holder", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "treasure proceed", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay back", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "overlay backdrop close", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "treasure overlay back", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: rest", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: smith", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: hatch", StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(targetLabel)
                   && targetLabel.StartsWith("rest site: option:", StringComparison.OrdinalIgnoreCase));
    }

    internal static bool KeepsCurrentRoomPhase(string? targetLabel)
    {
        return string.Equals(targetLabel, "claim reward item", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "reward choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "reward card choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "colorless card choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "reward skip", StringComparison.OrdinalIgnoreCase)
               || Program.IsOverlayCleanupTarget(targetLabel);
    }

    internal static bool IsRewardReleaseTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "reward skip", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "proceed after resolving rewards", StringComparison.OrdinalIgnoreCase);
    }
}
