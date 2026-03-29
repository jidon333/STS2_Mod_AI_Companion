using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModAiCompanion.Mod;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;
using static GuiSmokeChoicePrimitiveSupport;

sealed record PendingCombatSelection(
    int SlotIndex,
    AutoCombatCardKind Kind);

sealed record CombatNoOpLoopAnalysis(
    bool LoopDetected,
    int? BlockedSlotIndex,
    int RepeatedSelectionCount);

sealed record ReconstructedHandleCombatContext(
    IReadOnlyList<GuiSmokeHistoryEntry> CombatHistory,
    PendingCombatSelection? PendingSelection,
    IReadOnlyDictionary<int, int> CombatNoOpCountsBySlot,
    CombatNoOpLoopAnalysis CombatNoOpLoop,
    bool RepeatedNonEnemyLoop,
    bool RepeatedAttackSelectionLoop);

static class HandleCombatContextSupport
{
    public const int SerializedHistoryWindow = 12;

    public static IReadOnlyList<GuiSmokeHistoryEntry> BuildSerializedHistoryWindow(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var combatHistory = history
            .Where(entry => string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (combatHistory.Length <= SerializedHistoryWindow)
        {
            return combatHistory;
        }

        var serializedWindow = combatHistory
            .TakeLast(SerializedHistoryWindow)
            .ToArray();
        if (serializedWindow.Any(static entry => CombatBarrierSupport.IsMeaningfulCombatHistoryAction(entry.Action)))
        {
            return serializedWindow;
        }

        var latestMeaningfulIndex = Array.FindLastIndex(
            combatHistory,
            static entry => CombatBarrierSupport.IsMeaningfulCombatHistoryAction(entry.Action));
        if (latestMeaningfulIndex < 0)
        {
            return serializedWindow;
        }

        // Preserve the last meaningful combat actuation so trailing waits cannot erase the barrier seed.
        var trailingWaitTail = combatHistory
            .Skip(latestMeaningfulIndex + 1)
            .TakeLast(SerializedHistoryWindow - 1)
            .ToArray();
        return new[] { combatHistory[latestMeaningfulIndex] }
            .Concat(trailingWaitTail)
            .ToArray();
    }

    public static ReconstructedHandleCombatContext Reconstruct(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var combatHistory = BuildSerializedHistoryWindow(history);
        var pendingSelection = CombatHistorySupport.TryGetPendingCombatSelection(combatHistory);
        var combatNoOpCountsBySlot = CombatHistorySupport.GetCombatNoOpCountsBySlot(combatHistory);
        return new ReconstructedHandleCombatContext(
            combatHistory,
            pendingSelection,
            combatNoOpCountsBySlot,
            AnalyzeCombatNoOpLoop(combatHistory, combatNoOpCountsBySlot),
            HasRecentRepeatedNonEnemyLoop(combatHistory),
            HasRecentRepeatedAttackSelectionLoop(combatHistory));
    }

    public static bool HasRecentNonEnemySelection(ReconstructedHandleCombatContext context, int slotIndex)
    {
        return CombatHistorySupport.HasRecentNonEnemySelection(context.CombatHistory, slotIndex);
    }

    public static int GetCombatNoOpCountForSlot(ReconstructedHandleCombatContext context, int slotIndex)
    {
        return context.CombatNoOpCountsBySlot.TryGetValue(slotIndex, out var count)
            ? count
            : 0;
    }

    private static bool HasRecentRepeatedNonEnemyLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var labels = history
            .Select(static entry => entry.TargetLabel)
            .Where(static label =>
                !string.IsNullOrWhiteSpace(label)
                && (IsNonEnemySelectionLabel(label)
                    || string.Equals(label, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase)))
            .TakeLast(6)
            .ToArray();
        if (labels.Length < 4)
        {
            return false;
        }

        if (labels.Length >= 4
            && IsNonEnemySelectionLabel(labels[0])
            && string.Equals(labels[1], "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase)
            && IsNonEnemySelectionLabel(labels[2])
            && string.Equals(labels[3], "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var recentSelectCount = labels.Count(IsNonEnemySelectionLabel);
        if (recentSelectCount >= 3)
        {
            var distinctSelectionLabels = labels
                .Where(IsNonEnemySelectionLabel)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
            if (distinctSelectionLabels == 1)
            {
                return true;
            }
        }

        if (labels.Length >= 5)
        {
            var trailingWindow = labels.TakeLast(5).ToArray();
            var allowedMixedLoop = trailingWindow.All(label =>
                IsNonEnemySelectionLabel(label)
                || string.Equals(label, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase));
            if (allowedMixedLoop && trailingWindow.Count(IsNonEnemySelectionLabel) >= 3)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasRecentRepeatedAttackSelectionLoop(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var labels = history
            .Select(static entry => entry.TargetLabel)
            .Where(static label => !string.IsNullOrWhiteSpace(label))
            .TakeLast(6)
            .ToArray();
        if (labels.Length < 3)
        {
            return false;
        }

        var attackSelections = labels
            .Where(static label => label is not null && label.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase))
            .Select(static label => label!)
            .TakeLast(4)
            .ToArray();
        if (attackSelections.Length < 3)
        {
            return false;
        }

        var distinctAttackSelections = attackSelections
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (distinctAttackSelections > 1)
        {
            return true;
        }

        return attackSelections.Length >= 3;
    }

    private static CombatNoOpLoopAnalysis AnalyzeCombatNoOpLoop(
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        IReadOnlyDictionary<int, int> recentNoOpCounts)
    {
        if (history.Count < 4 || recentNoOpCounts.Count == 0)
        {
            return new CombatNoOpLoopAnalysis(false, null, 0);
        }

        var mostRecentBlockedSlot = history
            .Reverse()
            .Select(entry => CombatHistorySupport.ExtractCombatLaneSlotIndex(entry.TargetLabel))
            .FirstOrDefault(static slotIndex => slotIndex.HasValue);
        if (!mostRecentBlockedSlot.HasValue)
        {
            mostRecentBlockedSlot = recentNoOpCounts
                .OrderByDescending(static pair => pair.Value)
                .ThenBy(static pair => pair.Key)
                .Select(static pair => (int?)pair.Key)
                .FirstOrDefault();
        }

        if (!mostRecentBlockedSlot.HasValue)
        {
            return new CombatNoOpLoopAnalysis(false, null, 0);
        }

        var recentEnemyTargetCount = history.Count(entry =>
            string.Equals(entry.TargetLabel, "auto-target enemy", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entry.TargetLabel, "auto-target enemy recenter", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entry.TargetLabel, "auto-target enemy alternate", StringComparison.OrdinalIgnoreCase)
            || (entry.TargetLabel?.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase) ?? false));
        var repeatedSameSlotCount = recentNoOpCounts.TryGetValue(mostRecentBlockedSlot.Value, out var blockedCount)
            ? blockedCount
            : 0;
        var loopDetected = repeatedSameSlotCount >= 2 && recentEnemyTargetCount >= 2;
        return new CombatNoOpLoopAnalysis(loopDetected, mostRecentBlockedSlot, repeatedSameSlotCount);
    }

    private static bool IsNonEnemySelectionLabel(string? targetLabel)
    {
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return false;
        }

        return targetLabel.StartsWith("combat select non-enemy slot ", StringComparison.OrdinalIgnoreCase)
               || targetLabel.StartsWith("combat select defend slot ", StringComparison.OrdinalIgnoreCase);
    }
}
