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

static class CombatHistorySupport
{
    public static PendingCombatSelection? TryGetPendingCombatSelection(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return TryGetPendingCombatSelection(history, history.Count);
    }

    public static PendingCombatSelection? TryGetPendingCombatSelection(IReadOnlyList<GuiSmokeHistoryEntry> history, int endExclusive)
    {
        for (var index = Math.Min(history.Count, endExclusive) - 1; index >= 0; index -= 1)
        {
            var entry = history[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryParsePendingCombatSelection(entry.TargetLabel, out var selection))
            {
                return selection;
            }

            if (IsSelectionClearingEntry(entry))
            {
                return null;
            }

            if (IsNeutralCombatLabel(entry.TargetLabel))
            {
                continue;
            }

            if (IsCombatDecisionAction(entry.Action))
            {
                return null;
            }
        }

        return null;
    }

    public static IReadOnlyDictionary<int, int> GetCombatNoOpCountsBySlot(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var counts = new Dictionary<int, int>();
        for (var index = 0; index < history.Count; index += 1)
        {
            var entry = history[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                || !string.Equals(entry.Action, "combat-noop", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!TryResolveCombatLaneSlotIndex(entry.TargetLabel, history, index + 1, out var slotIndex))
            {
                continue;
            }

            counts[slotIndex] = counts.TryGetValue(slotIndex, out var count)
                ? count + 1
                : 1;
        }

        return counts;
    }

    public static string? ResolveCombatLaneLabel(string? targetLabel, IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return TryResolveCombatLaneSlotIndex(targetLabel, history, history.Count, out var slotIndex)
            ? $"combat lane slot {slotIndex}"
            : targetLabel;
    }

    public static bool HasRecentNonEnemySelection(IReadOnlyList<GuiSmokeHistoryEntry> history, int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            return false;
        }

        return history
            .Where(entry => string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
            .TakeLast(8)
            .Any(entry =>
                TryParsePendingCombatSelection(entry.TargetLabel, out var selection)
                && selection is { Kind: AutoCombatCardKind.DefendLike }
                && selection.SlotIndex == slotIndex);
    }

    public static bool TryParsePendingCombatSelection(string? targetLabel, out PendingCombatSelection? selection)
    {
        selection = null;
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return false;
        }

        if (targetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase)
            && ExtractFirstDigit(targetLabel) is { } attackSlot
            && IsValidSlotIndex(attackSlot))
        {
            selection = new PendingCombatSelection(attackSlot, AutoCombatCardKind.AttackLike);
            return true;
        }

        if ((targetLabel.StartsWith("combat select non-enemy slot ", StringComparison.OrdinalIgnoreCase)
             || targetLabel.StartsWith("combat select defend slot ", StringComparison.OrdinalIgnoreCase))
            && ExtractFirstDigit(targetLabel) is { } nonEnemySlot
            && IsValidSlotIndex(nonEnemySlot))
        {
            selection = new PendingCombatSelection(nonEnemySlot, AutoCombatCardKind.DefendLike);
            return true;
        }

        if (targetLabel.StartsWith("auto-select slot ", StringComparison.OrdinalIgnoreCase)
            && ExtractFirstDigit(targetLabel) is { } legacySlot
            && IsValidSlotIndex(legacySlot))
        {
            selection = new PendingCombatSelection(legacySlot, AutoCombatCardKind.AttackLike);
            return true;
        }

        return false;
    }

    public static int? ExtractCombatLaneSlotIndex(string? targetLabel)
    {
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return null;
        }

        if (targetLabel.StartsWith("combat lane slot ", StringComparison.OrdinalIgnoreCase)
            || targetLabel.StartsWith("combat select attack slot ", StringComparison.OrdinalIgnoreCase))
        {
            return ExtractFirstDigit(targetLabel);
        }

        return null;
    }

    public static bool IsCombatEnemyTargetLabel(string? targetLabel)
    {
        return targetLabel is not null
               && (targetLabel.StartsWith("auto-target enemy", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.StartsWith("combat enemy target", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsCombatEndTurnLabel(string? targetLabel)
    {
        return !string.IsNullOrWhiteSpace(targetLabel)
               && (targetLabel.Contains("end turn", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.Contains("턴 종료", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsCombatCancelSelectionLabel(string? targetLabel)
    {
        return !string.IsNullOrWhiteSpace(targetLabel)
               && (targetLabel.Contains("cancel", StringComparison.OrdinalIgnoreCase)
                   || targetLabel.Contains("취소", StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryResolveCombatLaneSlotIndex(
        string? targetLabel,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        int endExclusive,
        out int slotIndex)
    {
        slotIndex = default;
        if (ExtractCombatLaneSlotIndex(targetLabel) is { } directSlot
            && IsValidSlotIndex(directSlot))
        {
            slotIndex = directSlot;
            return true;
        }

        if (!IsCombatEnemyTargetLabel(targetLabel))
        {
            return false;
        }

        if (TryGetPendingCombatSelection(history, endExclusive) is { Kind: AutoCombatCardKind.AttackLike } scopedSelection
            && IsValidSlotIndex(scopedSelection.SlotIndex))
        {
            slotIndex = scopedSelection.SlotIndex;
            return true;
        }

        if (TryGetPendingCombatSelection(history) is { Kind: AutoCombatCardKind.AttackLike } recoveredSelection
            && IsValidSlotIndex(recoveredSelection.SlotIndex))
        {
            slotIndex = recoveredSelection.SlotIndex;
            return true;
        }

        return false;
    }

    private static bool IsSelectionClearingEntry(GuiSmokeHistoryEntry entry)
    {
        return IsCombatEndTurnLabel(entry.TargetLabel)
               || IsCombatCancelSelectionLabel(entry.TargetLabel)
               || string.Equals(entry.TargetLabel, "confirm selected non-enemy card", StringComparison.OrdinalIgnoreCase)
               || string.Equals(entry.TargetLabel, "confirm selected attack card", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNeutralCombatLabel(string? targetLabel)
    {
        return string.IsNullOrWhiteSpace(targetLabel)
               || targetLabel.StartsWith("combat lane slot ", StringComparison.OrdinalIgnoreCase)
               || IsCombatEnemyTargetLabel(targetLabel)
               || string.Equals(targetLabel, "observer-accepted", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCombatDecisionAction(string action)
    {
        return string.Equals(action, "click", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "click-current", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "confirm-attack-card", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "right-click", StringComparison.OrdinalIgnoreCase)
               || string.Equals(action, "press-key", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex is >= 1 and <= 5;
    }

    private static int? ExtractFirstDigit(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        foreach (var character in value)
        {
            if (char.IsDigit(character))
            {
                return character - '0';
            }
        }

        return null;
    }
}
