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

static class CombatEligibilitySupport
{
    public static bool IsCombatPlayerActionWindowClosed(ObserverSummary observer)
    {
        if (CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatPlayerActionsDisabled", "CombatManager.PlayerActionsDisabled") == true)
        {
            return true;
        }

        if (CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatEndingPlayerTurnPhaseOne", "CombatManager.EndingPlayerTurnPhaseOne") == true)
        {
            return true;
        }

        if (CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "combatEndingPlayerTurnPhaseTwo", "CombatManager.EndingPlayerTurnPhaseTwo") == true)
        {
            return true;
        }

        if (CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsEnemyTurnStarted", "CombatManager.IsEnemyTurnStarted") == true)
        {
            return true;
        }

        if (CombatAuthoritySupport.TryGetBoolOrCrossCheck(observer, "CombatManager.IsPlayPhase", "CombatManager.IsPlayPhase") == false)
        {
            return true;
        }

        return false;
    }

    public static bool IsPlayableAutoNonEnemyCombatCard(CombatCardKnowledgeHint card, int? energy)
    {
        return IsNonEnemyCombatCard(card)
               && IsAutoNonEnemyPromotionEligible(card)
               && IsPlayableAtCurrentEnergy(card, energy);
    }

    public static bool IsPlayableAutoNonEnemyCombatHandCard(
        ObservedCombatHandCard card,
        int? energy,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        if (!IsNonEnemyCombatHandCard(card) || IsInertCombatHandCard(card))
        {
            return false;
        }

        if (ResolveObservedCombatCardKnowledge(card, combatCardKnowledge) is { } knowledgeCard
            && !IsAutoNonEnemyPromotionEligible(knowledgeCard))
        {
            return false;
        }

        var resolvedCost = ResolveObservedCombatCardCost(card, combatCardKnowledge);
        if (resolvedCost is < 0)
        {
            return false;
        }

        if (energy is null || resolvedCost is null)
        {
            return true;
        }

        return resolvedCost <= energy;
    }

    public static bool HasSelectedNonEnemyConfirmEvidence(
        ObserverSummary observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        AutoCombatAnalysis analysis,
        PendingCombatSelection? pendingSelection)
    {
        if (CombatRuntimeStateSupport.HasRuntimeSelectedNonEnemyConfirmEvidence(observer, combatCardKnowledge, pendingSelection))
        {
            return true;
        }

        return CombatRuntimeStateSupport.HasNonEnemyConfirmEvidence(observer, combatCardKnowledge, pendingSelection, analysis);
    }

    public static bool HasSelectedNonEnemyConfirmEvidence(GuiSmokeStepRequest request)
    {
        var combatContext = HandleCombatContextSupport.Reconstruct(request.History, request.Observer, request.CombatCardKnowledge);
        var pendingSelection = CombatRuntimeStateSupport.ResolvePendingSelection(request.Observer, request.CombatCardKnowledge, combatContext.PendingSelection);
        if (CombatRuntimeStateSupport.HasRuntimeSelectedNonEnemyConfirmEvidence(request.Observer, request.CombatCardKnowledge, pendingSelection))
        {
            return true;
        }

        var analysis = string.IsNullOrWhiteSpace(request.ScreenshotPath)
            ? new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown)
            : AutoCombatAnalyzer.Analyze(request.ScreenshotPath);
        return HasSelectedNonEnemyConfirmEvidence(request.Observer, request.CombatCardKnowledge, analysis, pendingSelection);
    }

    public static bool HasSelectedAttackConfirmEvidence(GuiSmokeStepRequest request)
    {
        return CombatRuntimeStateSupport.HasPositiveAttackConfirmEvidence(
            request.Observer,
            request.CombatCardKnowledge);
    }

    private static bool IsAutoNonEnemyPromotionEligible(CombatCardKnowledgeHint card)
    {
        return !string.Equals(card.Type, "Status", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(card.Type, "Curse", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(card.Target, "None", StringComparison.OrdinalIgnoreCase)
               && card.Cost is not < 0;
    }

    private static bool IsNonEnemyCombatCard(CombatCardKnowledgeHint card)
    {
        if (string.Equals(card.Target, "AnyEnemy", StringComparison.OrdinalIgnoreCase)
            || string.Equals(card.Target, "RandomEnemy", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Equals(card.Type, "Power", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "Self", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "None", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AllAllies", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Target, "AnyAlly", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonEnemyCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Skill", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Type, "Power", StringComparison.OrdinalIgnoreCase)
               || card.Name.Contains("DEFEND", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInertCombatHandCard(ObservedCombatHandCard card)
    {
        return string.Equals(card.Type, "Status", StringComparison.OrdinalIgnoreCase)
               || string.Equals(card.Type, "Curse", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlayableAtCurrentEnergy(CombatCardKnowledgeHint card, int? energy)
    {
        if (energy is null || card.Cost is null)
        {
            return true;
        }

        if (card.Cost < 0)
        {
            return energy > 0;
        }

        return card.Cost <= energy;
    }

    private static int? ResolveObservedCombatCardCost(
        ObservedCombatHandCard card,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        if (card.Cost is not null)
        {
            return card.Cost;
        }

        var slotMatch = combatCardKnowledge.FirstOrDefault(candidate => candidate.SlotIndex == card.SlotIndex);
        if (slotMatch?.Cost is not null)
        {
            return slotMatch.Cost;
        }

        var cardKeys = BuildLookupKeys(card.Name);
        if (cardKeys.Count == 0)
        {
            return null;
        }

        return combatCardKnowledge
            .Where(candidate => candidate.Cost is not null)
            .Where(candidate => BuildLookupKeys(candidate.Name).Any(cardKeys.Contains))
            .Select(static candidate => candidate.Cost)
            .FirstOrDefault();
    }

    private static CombatCardKnowledgeHint? ResolveObservedCombatCardKnowledge(
        ObservedCombatHandCard card,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge)
    {
        var slotMatch = combatCardKnowledge.FirstOrDefault(candidate => candidate.SlotIndex == card.SlotIndex);
        if (slotMatch is not null)
        {
            return slotMatch;
        }

        var cardKeys = BuildLookupKeys(card.Name);
        if (cardKeys.Count == 0)
        {
            return null;
        }

        return combatCardKnowledge.FirstOrDefault(candidate =>
            BuildLookupKeys(candidate.Name).Any(cardKeys.Contains));
    }

    private static IReadOnlyList<string> BuildLookupKeys(string? cardName)
    {
        if (string.IsNullOrWhiteSpace(cardName))
        {
            return Array.Empty<string>();
        }

        var keys = new List<string>();
        var normalizedName = NormalizeLookupKey(cardName);
        AddLookupKey(keys, normalizedName);

        var parts = cardName
            .Split(new[] { '.', '_', '-', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeLookupKey)
            .Where(static part => part.Length > 0)
            .ToArray();
        if (parts.Length == 0)
        {
            return keys;
        }

        var trimmedParts = parts[0] == "card"
            ? parts[1..]
            : parts;
        if (trimmedParts.Length == 0)
        {
            return keys;
        }

        AddLookupKey(keys, string.Concat(trimmedParts));
        AddLookupKey(keys, trimmedParts[0]);
        if (trimmedParts.Length > 1 && IsCombatClassSuffix(trimmedParts[^1]))
        {
            AddLookupKey(keys, string.Concat(trimmedParts[..^1]));
        }

        foreach (var part in trimmedParts)
        {
            AddLookupKey(keys, part);
        }

        return keys;
    }

    private static void AddLookupKey(List<string> keys, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate)
            || keys.Contains(candidate, StringComparer.Ordinal))
        {
            return;
        }

        keys.Add(candidate);
    }

    private static bool IsCombatClassSuffix(string value)
    {
        return value is "ironclad" or "silent" or "defect" or "watcher" or "colorless" or "status" or "curse";
    }

    private static string NormalizeLookupKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        Span<char> buffer = stackalloc char[value.Length];
        var length = 0;
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                buffer[length] = char.ToLowerInvariant(character);
                length += 1;
            }
        }

        return length == 0
            ? string.Empty
            : new string(buffer[..length]);
    }
}
