namespace Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;

public static class CompactAdvisorScenePolicy
{
    public static string NormalizeSceneType(string? sceneType)
    {
        if (string.IsNullOrWhiteSpace(sceneType))
        {
            return "unknown";
        }

        return string.Equals(sceneType, "rewards", StringComparison.OrdinalIgnoreCase)
            ? "reward"
            : sceneType.Trim().ToLowerInvariant();
    }

    public static bool IsCompactAdvisorScene(string? sceneType)
    {
        return NormalizeSceneType(sceneType) is "reward" or "event" or "shop" or "combat";
    }

    public static bool AllowsModelCall(string? sceneType)
    {
        return NormalizeSceneType(sceneType) is "reward" or "event" or "shop";
    }

    public static bool IsPreviewOnlyScene(string? sceneType)
    {
        return string.Equals(NormalizeSceneType(sceneType), "combat", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsNoCallReason(string? blocker)
    {
        if (string.IsNullOrWhiteSpace(blocker))
        {
            return false;
        }

        return string.Equals(blocker, "reward-compact-input-insufficient", StringComparison.OrdinalIgnoreCase)
               || string.Equals(blocker, "event-compact-input-insufficient", StringComparison.OrdinalIgnoreCase)
               || string.Equals(blocker, "shop-compact-input-insufficient", StringComparison.OrdinalIgnoreCase)
               || string.Equals(blocker, "combat-compact-input-insufficient", StringComparison.OrdinalIgnoreCase)
               || string.Equals(blocker, "combat-preview-only", StringComparison.OrdinalIgnoreCase)
               || blocker.StartsWith("reward-duplicate-option-label:", StringComparison.OrdinalIgnoreCase)
               || blocker.StartsWith("event-duplicate-option-label:", StringComparison.OrdinalIgnoreCase)
               || blocker.StartsWith("shop-duplicate-option-label:", StringComparison.OrdinalIgnoreCase);
    }
}
