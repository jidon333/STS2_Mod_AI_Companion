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
    CombatNoOpLoopAnalysis CombatNoOpLoop);

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
        return ReconstructCore(combatHistory);
    }

    public static ReconstructedHandleCombatContext Reconstruct(
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var combatHistory = BuildSerializedHistoryWindow(history);
        var runtime = CombatRuntimeStateSupport.Read(observer, combatCardKnowledge);
        if (ShouldResetHistoryForFreshCombat(combatHistory, runtime))
        {
            combatHistory = Array.Empty<GuiSmokeHistoryEntry>();
        }

        return ReconstructCore(combatHistory);
    }

    private static ReconstructedHandleCombatContext ReconstructCore(IReadOnlyList<GuiSmokeHistoryEntry> combatHistory)
    {
        var pendingSelection = CombatHistorySupport.TryGetPendingCombatSelection(combatHistory);
        var combatNoOpCountsBySlot = CombatHistorySupport.GetCombatNoOpCountsBySlot(combatHistory);
        return new ReconstructedHandleCombatContext(
            combatHistory,
            pendingSelection,
            combatNoOpCountsBySlot,
            AnalyzeCombatNoOpLoop(combatHistory, combatNoOpCountsBySlot));
    }

    private static bool ShouldResetHistoryForFreshCombat(
        IReadOnlyList<GuiSmokeHistoryEntry> combatHistory,
        CombatRuntimeState runtime)
    {
        if (!CombatRuntimeStateSupport.LooksLikeFreshCombatEncounterStart(runtime))
        {
            return false;
        }

        for (var index = combatHistory.Count - 1; index >= 0; index -= 1)
        {
            var entry = combatHistory[index];
            if (!string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                || !CombatBarrierSupport.IsMeaningfulCombatHistoryAction(entry.Action))
            {
                continue;
            }

            var metadata = CombatBarrierSupport.TryParseHistoryMetadata(entry.Metadata);
            return IsFreshCombatIncompatibleWithHistory(runtime, metadata);
        }

        return false;
    }

    private static bool IsFreshCombatIncompatibleWithHistory(
        CombatRuntimeState runtime,
        CombatBarrierHistoryMetadata? metadata)
    {
        if (metadata is null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(metadata.ScreenEpisodeId)
            && !string.IsNullOrWhiteSpace(runtime.ScreenEpisodeId)
            && !string.Equals(metadata.ScreenEpisodeId, runtime.ScreenEpisodeId, StringComparison.Ordinal))
        {
            return true;
        }

        if (metadata.RoundNumber is not null
            && runtime.RoundNumber is not null
            && runtime.RoundNumber < metadata.RoundNumber)
        {
            return true;
        }

        if ((metadata.HistoryStartedCount is not null || metadata.HistoryFinishedCount is not null)
            && runtime.HistoryStartedCount is null
            && runtime.HistoryFinishedCount is null)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(metadata.LastFinishedCardId)
            && string.IsNullOrWhiteSpace(runtime.LastCardPlayFinishedCardId))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(metadata.InteractionRevision)
               && !CombatRuntimeStateSupport.IsDefaultInteractionRevision(metadata.InteractionRevision)
               && CombatRuntimeStateSupport.IsDefaultInteractionRevision(runtime.InteractionRevision);
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
}
