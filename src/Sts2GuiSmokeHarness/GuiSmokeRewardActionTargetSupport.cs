internal static class GuiSmokeRewardActionTargetSupport
{
    internal static bool IsRewardCardChoiceTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "reward card choice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "colorless card choice", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsRewardClaimTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "claim reward item", StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(targetLabel)
                   && targetLabel.StartsWith("claim reward ", StringComparison.OrdinalIgnoreCase));
    }

    internal static bool IsRewardSkipTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "reward skip", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsRewardGenericChoiceTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "reward choice", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsRewardStaleLoopTarget(string? targetLabel)
    {
        return IsRewardSkipTarget(targetLabel)
               || IsRewardGenericChoiceTarget(targetLabel)
               || IsRewardCardChoiceTarget(targetLabel)
               || IsRewardClaimTarget(targetLabel);
    }

    internal static string? TryGetRewardStaleLoopKey(string? targetLabel)
    {
        if (!IsRewardStaleLoopTarget(targetLabel))
        {
            return null;
        }

        return targetLabel?.Trim().ToLowerInvariant();
    }
}
