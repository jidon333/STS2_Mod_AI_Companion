using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Harness;

static partial class LongRunArtifacts
{
    public static bool TryMarkManualCleanBootVerified(
        string sessionRoot,
        HarnessQueueLayout harnessLayout,
        ObserverState observer,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        string screenshotPath,
        string? observerStatePath,
        DateTimeOffset? observerFreshnessFloor = null,
        bool stillInWaitMainMenu = true)
    {
        var prevalidation = LoadOrCreatePrevalidation(sessionRoot);
        if (prevalidation.ManualCleanBootVerified)
        {
            return true;
        }

        var firstStepEligible = history.Count == 0;
        var observedScreen = ResolveObservedScreen(observer);
        var observerFresh = observerFreshnessFloor is null || observer.IsFreshSince(observerFreshnessFloor.Value);
        var observerReady = observerFresh && !IsUnknownObservedScreen(observedScreen);
        var mainMenuObserved = observerReady && string.Equals(observedScreen, "main-menu", StringComparison.OrdinalIgnoreCase);
        var armSessionPresent = HasActiveArmSession(harnessLayout.ArmSessionPath);
        var actionsPending = HasPendingHarnessActions(harnessLayout.ActionsPath);
        var status = TryReadJson<HarnessBridgeStatus>(harnessLayout.StatusPath);
        var inventory = TryReadJson<HarnessNodeInventory>(harnessLayout.InventoryPath);
        var inventoryDormant = inventory is null || string.Equals(inventory.Mode, "dormant", StringComparison.OrdinalIgnoreCase);
        var actionsQueueClear = !actionsPending
                                || (!armSessionPresent && firstStepEligible && mainMenuObserved && inventoryDormant);
        var blockingReasons = new List<string>();
        var evaluationNotes = new List<string>();
        if (!firstStepEligible)
        {
            blockingReasons.Add("not-first-step");
        }

        if (!stillInWaitMainMenu)
        {
            blockingReasons.Add("not-wait-main-menu-phase");
        }

        if (!observerReady)
        {
            blockingReasons.Add("observer-not-ready");
        }
        else if (!mainMenuObserved)
        {
            blockingReasons.Add($"observer-not-main-menu:{observedScreen}");
        }

        if (armSessionPresent)
        {
            blockingReasons.Add("arm-session-present");
        }

        if (!actionsQueueClear)
        {
            blockingReasons.Add("actions-pending-active");
        }
        else if (actionsPending)
        {
            evaluationNotes.Add("stale-actions-observed-but-inert");
        }

        if (!inventoryDormant)
        {
            blockingReasons.Add("harness-inventory-not-dormant");
        }

        if (!observerFresh)
        {
            evaluationNotes.Add("observer-stale");
        }

        if (!string.IsNullOrWhiteSpace(status?.Mode)
            && !string.Equals(status.Mode, "dormant", StringComparison.OrdinalIgnoreCase))
        {
            evaluationNotes.Add($"status-mode:{status.Mode}");
        }

        var verified = firstStepEligible
                       && stillInWaitMainMenu
                       && mainMenuObserved
                       && !armSessionPresent
                       && actionsQueueClear
                       && inventoryDormant;
        var evidence = new GuiSmokeManualCleanBootEvidence(
            DateTimeOffset.UtcNow,
            screenshotPath,
            File.Exists(screenshotPath) ? ComputeFullFileSha256(screenshotPath) : "missing",
            observerStatePath,
            !string.IsNullOrWhiteSpace(observerStatePath) && File.Exists(observerStatePath)
                ? ComputeFullFileSha256(observerStatePath)
                : null,
            harnessLayout.StatusPath,
            File.Exists(harnessLayout.StatusPath) ? ComputeFullFileSha256(harnessLayout.StatusPath) : null,
            harnessLayout.InventoryPath,
            File.Exists(harnessLayout.InventoryPath) ? ComputeFullFileSha256(harnessLayout.InventoryPath) : null,
            harnessLayout.ArmSessionPath,
            armSessionPresent,
            harnessLayout.ActionsPath,
            actionsPending,
            status?.Mode,
            inventory?.Mode,
            observedScreen,
            firstStepEligible,
            mainMenuObserved,
            !armSessionPresent,
            actionsQueueClear,
            inventoryDormant,
            status?.LastActionId,
            status?.LastResultStatus,
            blockingReasons,
            evaluationNotes);
        if (prevalidation.ManualCleanBootEvidence is null || firstStepEligible || verified)
        {
            UpdatePrevalidation(
                sessionRoot,
                manualCleanBootVerified: verified,
                manualCleanBootEvidence: evidence,
                note: verified
                    ? "runner captured manual clean boot evidence before the first action."
                    : $"runner recorded manual clean boot blockers:{string.Join(",", blockingReasons)}");
        }

        return verified;
    }

    private static string? ResolveObservedScreen(ObserverState observer)
    {
        return ObserverScreenProvenance.DisplayScreen(observer);
    }

    private static bool IsUnknownObservedScreen(string? screen)
    {
        return string.IsNullOrWhiteSpace(screen)
               || string.Equals(screen, "unknown", StringComparison.OrdinalIgnoreCase);
    }
}
