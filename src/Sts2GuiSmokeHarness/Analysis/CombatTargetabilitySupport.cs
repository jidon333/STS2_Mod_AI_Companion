using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

static class CombatTargetabilitySupport
{
    public static bool HasExplicitTargetableEnemyAuthority(ObserverSummary observer)
    {
        return observer.Meta.ContainsKey("combatTargetableEnemyCount")
               || observer.Meta.ContainsKey("combatTargetableEnemyIds")
               || observer.Meta.ContainsKey("combatHittableEnemyCount")
               || observer.Meta.ContainsKey("combatHittableEnemyIds");
    }

    public static bool HasExplicitCombatEnemyTargetNodeSource(ObserverSummary observer)
    {
        var runtime = CombatRuntimeStateSupport.Read(observer, Array.Empty<CombatCardKnowledgeHint>());
        var targetableIds = (runtime.HasExplicitHittableEnemyAuthority ? runtime.HittableEnemyIds : runtime.TargetableEnemyIds)
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return observer.ActionNodes
            .Where(static node => node.Actionable)
            .Any(node => IsExplicitCombatEnemyTargetNode(node, targetableIds, runtime.HasExplicitHittableEnemyAuthority || runtime.HasExplicitTargetableEnemyAuthority));
    }

    public static string DescribeMissingCombatEnemyTargetDecisionSource(ObserverSummary observer, WindowBounds? windowBounds)
    {
        var runtime = CombatRuntimeStateSupport.Read(observer, Array.Empty<CombatCardKnowledgeHint>());
        var targetableIds = (runtime.HasExplicitHittableEnemyAuthority ? runtime.HittableEnemyIds : runtime.TargetableEnemyIds)
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var explicitNodes = observer.ActionNodes
            .Where(static node => node.Actionable)
            .Where(node => IsExplicitCombatEnemyTargetNode(node, targetableIds, runtime.HasExplicitHittableEnemyAuthority || runtime.HasExplicitTargetableEnemyAuthority))
            .ToArray();
        if (explicitNodes.Length == 0)
        {
            return runtime.HasExplicitHittableEnemyAuthority || runtime.HasExplicitTargetableEnemyAuthority
                ? "explicit runtime enemy-target authority exists, but no usable target-manager enemy node is available yet"
                : "waiting for explicit enemy target node source";
        }

        if (windowBounds is not null)
        {
            var unusableNode = explicitNodes.FirstOrDefault(node => !HasUsableCombatTargetBounds(node.ScreenBounds, windowBounds));
            if (unusableNode is not null)
            {
                return $"explicit enemy target node '{unusableNode.Label}' exists but its bounds are unusable for body targeting";
            }
        }

        return "explicit enemy target nodes exist, but none survived actionable body-target filtering";
    }

    public static IReadOnlyList<ObserverActionNode> GetCombatEnemyTargetNodes(ObserverSummary observer, WindowBounds? windowBounds = null)
    {
        var runtime = CombatRuntimeStateSupport.Read(observer, Array.Empty<CombatCardKnowledgeHint>());
        if (runtime.HasExplicitHittableEnemyAuthority && !runtime.HasExplicitHittableEnemy)
        {
            return Array.Empty<ObserverActionNode>();
        }

        var targetableIds = (runtime.HasExplicitHittableEnemyAuthority ? runtime.HittableEnemyIds : runtime.TargetableEnemyIds)
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return observer.ActionNodes
            .Where(static node => node.Actionable)
            .Where(node => windowBounds is null
                ? TryParseBounds(node.ScreenBounds, out _)
                : HasUsableCombatTargetBounds(node.ScreenBounds, windowBounds))
            .Where(node => IsExplicitCombatEnemyTargetNode(node, targetableIds, runtime.HasExplicitHittableEnemyAuthority || runtime.HasExplicitTargetableEnemyAuthority))
            .OrderBy(node => GetSortX(node))
            .ThenBy(node => GetSortY(node))
            .ToArray();
    }

    private static bool IsExplicitCombatEnemyTargetNode(
        ObserverActionNode node,
        HashSet<string> targetableIds,
        bool requireExplicitRuntimeEvidence)
    {
        if (string.Equals(node.TypeName, "relic", StringComparison.OrdinalIgnoreCase)
            || node.SemanticHints.Any(static hint => string.Equals(hint, "raw-kind:relic", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        var explicitEnemyTargetNode = string.Equals(node.Kind, "enemy-target", StringComparison.OrdinalIgnoreCase)
                                      || node.NodeId.StartsWith("enemy-target:", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(node.TypeName, "enemy-target", StringComparison.OrdinalIgnoreCase)
                                      || node.SemanticHints.Any(static hint =>
                                          string.Equals(hint, "combat-targetable", StringComparison.OrdinalIgnoreCase)
                                          || string.Equals(hint, "combat-hittable", StringComparison.OrdinalIgnoreCase)
                                          || hint.StartsWith("target-id:", StringComparison.OrdinalIgnoreCase));
        if (!explicitEnemyTargetNode)
        {
            return false;
        }

        if (targetableIds.Count == 0)
        {
            return !requireExplicitRuntimeEvidence;
        }

        var hintedIds = node.SemanticHints
            .Where(static hint => hint.StartsWith("target-id:", StringComparison.OrdinalIgnoreCase))
            .Select(static hint => hint["target-id:".Length..])
            .Where(static hint => !string.IsNullOrWhiteSpace(hint));
        return hintedIds.Any(targetableIds.Contains)
               || targetableIds.Contains(node.NodeId)
               || targetableIds.Contains(node.Label);
    }

    private static bool HasActiveBounds(string? rawBounds, WindowBounds windowBounds)
    {
        if (!TryParseBounds(rawBounds, out var bounds))
        {
            return false;
        }

        return bounds.Width > 0f
               && bounds.Height > 0f
               && bounds.Left + bounds.Width >= windowBounds.X
               && bounds.Top + bounds.Height >= windowBounds.Y
               && bounds.Left <= windowBounds.X + windowBounds.Width
               && bounds.Top <= windowBounds.Y + windowBounds.Height;
    }

    private static bool HasUsableCombatTargetBounds(string? rawBounds, WindowBounds windowBounds)
    {
        return HasUsableLogicalBoundsForCombatTarget(rawBounds) || HasActiveBounds(rawBounds, windowBounds);
    }

    private static bool HasUsableLogicalBoundsForCombatTarget(string? rawBounds)
    {
        return TryParseBounds(rawBounds, out var bounds)
               && bounds.Left >= 0f
               && bounds.Top >= 0f
               && bounds.Right <= 1920f
               && bounds.Bottom <= 1080f;
    }

    private static bool TryParseBounds(string? rawBounds, out RectangleF bounds)
    {
        bounds = default;
        if (string.IsNullOrWhiteSpace(rawBounds))
        {
            return false;
        }

        var parts = rawBounds.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length != 4
            || !float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
        {
            return false;
        }

        bounds = new RectangleF(x, y, width, height);
        return bounds.Width > 0f && bounds.Height > 0f;
    }

    private static float GetSortX(ObserverActionNode node)
    {
        return TryParseBounds(node.ScreenBounds, out var bounds) ? bounds.Left : float.MaxValue;
    }

    private static float GetSortY(ObserverActionNode node)
    {
        return TryParseBounds(node.ScreenBounds, out var bounds) ? bounds.Top : float.MaxValue;
    }
}
