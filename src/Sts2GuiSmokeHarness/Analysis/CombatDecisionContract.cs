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

static class CombatDecisionContract
{
    public static bool TryMapSemanticAction(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string semanticAction)
    {
        return TryMapSemanticAction(request, decision, out semanticAction, out _);
    }

    public static bool IsAllowed(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string? semanticAction)
    {
        semanticAction = null;
        if (!TryMapSemanticAction(request, decision, out var mappedAction, out var allowLegacyCardAliases))
        {
            return false;
        }

        semanticAction = mappedAction;
        if (request.AllowedActions.Contains(mappedAction, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        return allowLegacyCardAliases
               && (request.AllowedActions.Contains("click card", StringComparer.OrdinalIgnoreCase)
                   || request.AllowedActions.Contains("select card from hand", StringComparer.OrdinalIgnoreCase));
    }

    private static bool TryMapSemanticAction(
        GuiSmokeStepRequest request,
        GuiSmokeStepDecision decision,
        out string semanticAction,
        out bool allowLegacyCardAliases)
    {
        semanticAction = string.Empty;
        allowLegacyCardAliases = false;

        if (string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase))
        {
            semanticAction = "wait";
            return true;
        }

        if (!string.Equals(request.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(decision.ActionKind, "confirm-non-enemy", StringComparison.OrdinalIgnoreCase))
        {
            if (CombatEligibilitySupport.HasSelectedNonEnemyConfirmEvidence(request))
            {
                semanticAction = "confirm selected non-enemy card";
                return true;
            }

            return false;
        }

        if (string.Equals(decision.ActionKind, "click-current", StringComparison.OrdinalIgnoreCase))
        {
            if (CombatEligibilitySupport.HasSelectedNonEnemyConfirmEvidence(request))
            {
                semanticAction = "confirm selected non-enemy card";
                return true;
            }

            return false;
        }

        if (string.Equals(decision.ActionKind, "confirm-attack-card", StringComparison.OrdinalIgnoreCase))
        {
            if (CombatEligibilitySupport.HasSelectedAttackConfirmEvidence(request))
            {
                semanticAction = "confirm selected attack card";
                return true;
            }

            return false;
        }

        if (CombatHistorySupport.TryParsePendingCombatSelection(decision.TargetLabel, out var selection)
            && selection is not null)
        {
            semanticAction = selection.Kind == AutoCombatCardKind.AttackLike
                ? $"select attack slot {selection.SlotIndex}"
                : $"select non-enemy slot {selection.SlotIndex}";
            allowLegacyCardAliases = true;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(decision.TargetLabel)
            && decision.TargetLabel.StartsWith("combat select hand slot ", StringComparison.OrdinalIgnoreCase))
        {
            semanticAction = "select card from hand";
            return true;
        }

        if (string.Equals(decision.TargetLabel, "confirm selected hand card", StringComparison.OrdinalIgnoreCase))
        {
            semanticAction = "confirm selected hand card";
            return true;
        }

        if (CombatHistorySupport.IsCombatEnemyTargetLabel(decision.TargetLabel))
        {
            semanticAction = "click enemy";
            return true;
        }

        if (CombatHistorySupport.IsCombatEndTurnLabel(decision.TargetLabel)
            || (string.Equals(decision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase)
                && string.Equals(decision.KeyText, "E", StringComparison.OrdinalIgnoreCase)))
        {
            semanticAction = "click end turn";
            return true;
        }

        if (CombatHistorySupport.IsCombatCancelSelectionLabel(decision.TargetLabel)
            || string.Equals(decision.ActionKind, "right-click", StringComparison.OrdinalIgnoreCase))
        {
            semanticAction = "right-click cancel selected card";
            return true;
        }

        if (string.Equals(decision.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase)
            && decision.KeyText?.Length == 1
            && char.IsDigit(decision.KeyText[0])
            && decision.KeyText[0] is >= '1' and <= '5')
        {
            semanticAction = $"select attack slot {decision.KeyText[0]}";
            allowLegacyCardAliases = true;
            return true;
        }

        return false;
    }
}
