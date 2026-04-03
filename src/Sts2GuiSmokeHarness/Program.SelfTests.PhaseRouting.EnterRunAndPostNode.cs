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
using static GuiSmokePostActionPhaseSupport;
using static GuiSmokeReplayArtifactSupport;

internal static partial class Program
{
    private static void RunPhaseRoutingEnterRunAndPostNodeSelfTests()
    {
        var evaluator = new ObserverAcceptanceEvaluator();
        var postEnterRunCharacterSelectObserver = CreatePhaseRoutingCharacterSelectObserver();
        Assert(
            GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(postEnterRunCharacterSelectObserver, out var postEnterRunCharacterSelectPhase)
            && postEnterRunCharacterSelectPhase == GuiSmokePhase.ChooseCharacter,
            "Run-load reconciliation should still recognize fresh new-run character-select flows once they are actually visible.");

        var continuePreferredDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            2,
            GuiSmokePhase.EnterRun.ToString(),
            "Enter a run using Continue first, otherwise Singleplayer.",
            DateTimeOffset.UtcNow,
            "screen.png",
            new WindowBounds(0, 0, 1280, 720),
            "phase:enter-run|screen:main-menu|visible:main-menu|ready:true",
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-main-menu",
                true,
                "main-menu",
                "stable",
                null,
                null,
                null,
                null,
                null,
                null,
                new[] { "Continue" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("continue", "action", "Continue", "620,560,420,96", true),
                },
                new[]
                {
                    new ObserverChoice("choice", "Continue", "620,560,420,96"),
                },
                Array.Empty<ObservedCombatHandCard>()),
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            Array.Empty<string>(),
            Array.Empty<GuiSmokeHistoryEntry>(),
            string.Empty,
            null));
        Assert(
            string.Equals(continuePreferredDecision.TargetLabel, "continue", StringComparison.OrdinalIgnoreCase),
            "EnterRun should still prefer Continue when it is visible on the main menu.");
        Assert(
            GetPostEnterRunPhase(continuePreferredDecision) == GuiSmokePhase.WaitRunLoad,
            "Continue should hand off to neutral run-load waiting, not WaitCharacterSelect.");
        var continueReturnedMainMenuDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            2,
            GuiSmokePhase.EnterRun.ToString(),
            "Prefer Continue when a stable main-menu retry surface is visible, even if Abandon Run is also present.",
            DateTimeOffset.UtcNow,
            "screen.png",
            new WindowBounds(0, 0, 1280, 720),
            "phase:enter-run|screen:main-menu|visible:main-menu|ready:true|terminal-run-boundary",
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-main-menu-run-start-and-abandon",
                true,
                "main-menu",
                "stable",
                null,
                null,
                "main-menu",
                null,
                null,
                null,
                new[] { "Continue", "Abandon Run" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("main-menu:continue", "continue-run", "Continue", "620,560,420,96", true),
                    new ObserverActionNode("main-menu:abandon-run", "menu-action", "Abandon Run", "620,680,420,96", true),
                },
                new[]
                {
                    new ObserverChoice("continue-run", "Continue", "620,560,420,96"),
                    new ObserverChoice("menu-action", "Abandon Run", "620,680,420,96"),
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                PublishedCurrentScreen = "main-menu",
                PublishedVisibleScreen = "main-menu",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stable",
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["terminalRunBoundary"] = "true",
                    ["mainMenuReturnDetected"] = "true",
                    ["choiceExtractorPath"] = "main-menu",
                    ["rootSceneIsMainMenu"] = "true",
                },
            },
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            Array.Empty<string>(),
            Array.Empty<GuiSmokeHistoryEntry>(),
            string.Empty,
            null));
        Assert(
            string.Equals(continueReturnedMainMenuDecision.TargetLabel, "continue", StringComparison.OrdinalIgnoreCase),
            "EnterRun should prefer Continue on a stable returned main menu when an explicit continue-run surface is visible, even if Abandon Run is also published.");
        Assert(
            GetPostEnterRunPhase(continueReturnedMainMenuDecision) == GuiSmokePhase.WaitRunLoad,
            "Continue retries from the returned main menu should hand off to neutral run-load waiting.");
        var continueFailedRetryHistory = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.WaitRunLoad.ToString(), "wait", "main-menu", DateTimeOffset.UtcNow.AddSeconds(-4)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.WaitRunLoad.ToString(), "wait", "main-menu", DateTimeOffset.UtcNow.AddSeconds(-3)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.WaitRunLoad.ToString(), "wait", "main-menu", DateTimeOffset.UtcNow.AddSeconds(-2)),
            new GuiSmokeHistoryEntry(GuiSmokePhase.WaitRunLoad.ToString(), "retry-enter-run", "main-menu", DateTimeOffset.UtcNow.AddSeconds(-1))
            {
                Metadata = "FreshRetryAfterContinuePending",
            },
        };
        var continueReturnedMainMenuAfterFailedRetryObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-main-menu-run-start-and-abandon-retry",
                true,
                "main-menu",
                "stable",
                null,
                null,
                "main-menu",
                null,
                null,
                null,
                new[] { "Continue", "Abandon Run" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("main-menu:continue", "continue-run", "Continue", "620,560,420,96", true),
                    new ObserverActionNode("main-menu:abandon-run", "menu-action", "Abandon Run", "620,680,420,96", true),
                },
                new[]
                {
                    new ObserverChoice("continue-run", "Continue", "620,560,420,96"),
                    new ObserverChoice("menu-action", "Abandon Run", "620,680,420,96"),
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                PublishedCurrentScreen = "main-menu",
                PublishedVisibleScreen = "main-menu",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stable",
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["terminalRunBoundary"] = "true",
                    ["mainMenuReturnDetected"] = "true",
                    ["choiceExtractorPath"] = "main-menu",
                    ["rootSceneIsMainMenu"] = "true",
                },
            },
            null,
            null,
            null);
        Assert(
            WaitRunLoadRecoverySignals.ShouldPreferRunSaveCleanupAfterFailedContinue(continueReturnedMainMenuAfterFailedRetryObserver.Summary, continueFailedRetryHistory),
            "A returned main menu with explicit Abandon Run should retire Continue after a bounded failed-Continue retry.");
        var continueReturnedMainMenuAfterFailedRetryActions = BuildAllowedActions(
            GuiSmokePhase.EnterRun,
            continueReturnedMainMenuAfterFailedRetryObserver,
            Array.Empty<CombatCardKnowledgeHint>(),
            null,
            continueFailedRetryHistory);
        Assert(
            continueReturnedMainMenuAfterFailedRetryActions.Contains("click abandon run", StringComparer.OrdinalIgnoreCase)
            && !continueReturnedMainMenuAfterFailedRetryActions.Contains("click continue", StringComparer.OrdinalIgnoreCase),
            "EnterRun allowed actions should pivot to cleanup instead of reopening Continue once a bounded failed-Continue retry returns to the main menu.");
        var continueReturnedMainMenuAfterFailedRetryDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            14,
            GuiSmokePhase.EnterRun.ToString(),
            "Retire Continue after a bounded failed-Continue retry and clear the stale run save first.",
            DateTimeOffset.UtcNow,
            "screen.png",
            new WindowBounds(0, 0, 1280, 720),
            "phase:enter-run|screen:main-menu|visible:main-menu|ready:true|terminal-run-boundary",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            continueReturnedMainMenuAfterFailedRetryObserver.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            continueReturnedMainMenuAfterFailedRetryActions,
            continueFailedRetryHistory,
            string.Empty,
            null));
        Assert(
            string.Equals(continueReturnedMainMenuAfterFailedRetryDecision.TargetLabel, "abandon run", StringComparison.OrdinalIgnoreCase),
            "EnterRun should choose Abandon Run instead of Continue when a bounded failed-Continue retry returns to the main menu with explicit cleanup authority.");
        Assert(
            GetPostEnterRunPhase(continueReturnedMainMenuAfterFailedRetryDecision) == GuiSmokePhase.WaitRunLoad,
            "The failed-Continue cleanup lane should still hand off to neutral run-load waiting so the abandon confirmation popup can be observed.");
        var runSaveCleanupDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            3,
            GuiSmokePhase.EnterRun.ToString(),
            "Clear a stale run-save surface before entering a fresh run.",
            DateTimeOffset.UtcNow,
            "screen.png",
            new WindowBounds(0, 0, 1280, 720),
            "phase:enter-run|screen:main-menu|visible:main-menu|ready:true|terminal-run-boundary",
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-main-menu-run-save",
                true,
                "main-menu",
                "stable",
                null,
                null,
                "main-menu",
                null,
                null,
                null,
                new[] { "Abandon Run" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("main-menu:abandon-run", "menu-action", "Abandon Run", "620,680,420,96", true),
                },
                new[]
                {
                    new ObserverChoice("menu-action", "Abandon Run", "620,680,420,96"),
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                PublishedCurrentScreen = "main-menu",
                PublishedVisibleScreen = "main-menu",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stable",
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["terminalRunBoundary"] = "true",
                    ["mainMenuReturnDetected"] = "true",
                    ["choiceExtractorPath"] = "main-menu",
                    ["rootSceneIsMainMenu"] = "true",
                },
            },
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            Array.Empty<string>(),
            Array.Empty<GuiSmokeHistoryEntry>(),
            string.Empty,
            null));
        Assert(
            string.Equals(runSaveCleanupDecision.TargetLabel, "abandon run", StringComparison.OrdinalIgnoreCase),
            "EnterRun should clear a persisted run-save surface with Abandon Run when no explicit continue or singleplayer run-start surface is available.");
        Assert(
            GetPostEnterRunPhase(runSaveCleanupDecision) == GuiSmokePhase.WaitRunLoad,
            "Abandon Run should hand off to neutral run-load waiting so the main-menu cleanup popup and follow-up recovery can be observed.");
        var abandonRunConfirmDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            3,
            GuiSmokePhase.EnterRun.ToString(),
            "Confirm main-menu abandon-run cleanup.",
            DateTimeOffset.UtcNow,
            "screen.png",
            new WindowBounds(0, 0, 1280, 720),
            "phase:enter-run|screen:main-menu|visible:main-menu|ready:true|terminal-run-boundary",
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-main-menu-abandon-confirm",
                true,
                "main-menu",
                "stable",
                null,
                null,
                "generic",
                null,
                null,
                null,
                new[] { "Confirm", "Cancel" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("popup:confirm", "choice", "Confirm", "760,470,210,80", true),
                    new ObserverActionNode("popup:cancel", "choice", "Cancel", "520,470,210,80", true),
                },
                new[]
                {
                    new ObserverChoice("choice", "Confirm", "760,470,210,80"),
                    new ObserverChoice("choice", "Cancel", "520,470,210,80"),
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                PublishedCurrentScreen = "main-menu",
                PublishedVisibleScreen = "main-menu",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stable",
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["terminalRunBoundary"] = "true",
                    ["mainMenuReturnDetected"] = "true",
                    ["choiceExtractorPath"] = "generic",
                    ["rootSceneIsMainMenu"] = "true",
                },
            },
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            Array.Empty<string>(),
            new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.EnterRun.ToString(), "abandon run", "main-menu", DateTimeOffset.UtcNow),
            },
            string.Empty,
            null));
        Assert(
            string.Equals(abandonRunConfirmDecision.TargetLabel, "confirm abandon run", StringComparison.OrdinalIgnoreCase),
            "EnterRun should confirm the main-menu abandon-run popup before proceeding to the fresh new-run path.");
        Assert(
            GetPostEnterRunPhase(abandonRunConfirmDecision) == GuiSmokePhase.WaitRunLoad,
            "Confirming Abandon Run should stay on neutral run-load waiting until the fresh main-menu run-start surface is exported.");
        var localizedAbandonRunConfirmDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            4,
            GuiSmokePhase.EnterRun.ToString(),
            "Confirm localized main-menu abandon-run cleanup.",
            DateTimeOffset.UtcNow,
            "screen.png",
            new WindowBounds(0, 0, 1280, 720),
            "phase:enter-run|screen:main-menu|visible:main-menu|terminal-run-boundary",
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-main-menu-abandon-confirm-localized",
                true,
                "main-menu",
                "transient",
                "main-menu-abandon-confirm",
                null,
                null,
                12,
                12,
                null,
                new[] { "\uC544\uB2C8\uC694", "\uC608" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("main-menu:abandon-run-cancel", "menu-action", "\uC544\uB2C8\uC694", "699,688,180,72", true)
                    {
                        SemanticHints = new[] { "node-id:main-menu:abandon-run-cancel" },
                    },
                    new ObserverActionNode("main-menu:abandon-run-confirm", "menu-action", "\uC608", "1041,688,180,72", true)
                    {
                        SemanticHints = new[] { "node-id:main-menu:abandon-run-confirm" },
                    },
                },
                new[]
                {
                    new ObserverChoice("cancel", "\uC544\uB2C8\uC694", "699,688,180,72", "main-menu:abandon-run-cancel", "main-menu abandon run confirm popup")
                    {
                        NodeId = "main-menu:abandon-run-cancel",
                        Enabled = true,
                    },
                    new ObserverChoice("confirm", "\uC608", "1041,688,180,72", "main-menu:abandon-run-confirm", "main-menu abandon run confirm popup")
                    {
                        NodeId = "main-menu:abandon-run-confirm",
                        Enabled = true,
                    },
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                PublishedCurrentScreen = "main-menu",
                PublishedVisibleScreen = "main-menu",
                PublishedSceneReady = null,
                PublishedSceneAuthority = null,
                PublishedSceneStability = null,
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["terminalRunBoundary"] = "true",
                    ["mainMenuReturnDetected"] = "true",
                    ["choiceExtractorPath"] = "main-menu-abandon-confirm",
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.CommonUi.NAbandonRunConfirmPopup",
                    ["rootSceneIsMainMenu"] = "true",
                    ["compatSceneReady"] = "false",
                    ["compatSceneStability"] = "transient",
                },
            },
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            Array.Empty<string>(),
            new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.EnterRun.ToString(), "abandon run", "main-menu", DateTimeOffset.UtcNow),
            },
            string.Empty,
            null));
        Assert(
            string.Equals(localizedAbandonRunConfirmDecision.TargetLabel, "confirm abandon run", StringComparison.OrdinalIgnoreCase),
            "EnterRun should use explicit popup node identity to confirm localized abandon-run popups instead of waiting for generic main-menu actions.");
        Assert(
            GetPostEnterRunPhase(localizedAbandonRunConfirmDecision) == GuiSmokePhase.WaitRunLoad,
            "Localized abandon-run confirmation should also hand back to neutral run-load waiting.");
        var ambiguousRunStartDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            2,
            GuiSmokePhase.EnterRun.ToString(),
            "Wait until the main-menu run-start surface stabilizes.",
            DateTimeOffset.UtcNow,
            "screen.png",
            new WindowBounds(0, 0, 1280, 720),
            "phase:enter-run|screen:main-menu|visible:main-menu|ready:true",
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-main-menu-ambiguous",
                true,
                "main-menu",
                "stable",
                null,
                null,
                null,
                null,
                null,
                null,
                new[] { "Continue", "Singleplayer" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("continue", "continue-run", "Continue", "620,560,420,96", true),
                    new ObserverActionNode("singleplayer", "singleplayer", "Singleplayer", "620,680,420,96", true),
                },
                new[]
                {
                    new ObserverChoice("continue-run", "Continue", "620,560,420,96"),
                    new ObserverChoice("singleplayer", "Singleplayer", "620,680,420,96"),
                },
                Array.Empty<ObservedCombatHandCard>()),
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            Array.Empty<string>(),
            Array.Empty<GuiSmokeHistoryEntry>(),
            string.Empty,
            null));
        Assert(
            string.Equals(ambiguousRunStartDecision.Status, "wait", StringComparison.OrdinalIgnoreCase),
            "EnterRun should wait while Continue and Singleplayer are both still exported on the top-level main menu surface.");
        var collapsedRunStartDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            2,
            GuiSmokePhase.EnterRun.ToString(),
            "Wait until the main-menu button layout stops exporting collapsed placeholder bounds.",
            DateTimeOffset.UtcNow,
            "screen.png",
            new WindowBounds(0, 0, 1280, 720),
            "phase:enter-run|screen:main-menu|visible:main-menu|ready:true",
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-main-menu-collapsed",
                true,
                "main-menu",
                "stable",
                null,
                null,
                null,
                null,
                null,
                null,
                Array.Empty<string>(),
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("main-menu:continue", "continue-run", "Continue", "620,560,420,96", true),
                    new ObserverActionNode("main-menu:multiplayer", "menu-action", "Multiplayer", "620,560,420,96", true),
                    new ObserverActionNode("main-menu:settings", "menu-action", "Settings", "620,560,420,96", true),
                },
                new[]
                {
                    new ObserverChoice("continue-run", "Continue", "620,560,420,96") { NodeId = "main-menu:continue" },
                    new ObserverChoice("menu-action", "Multiplayer", "620,560,420,96") { NodeId = "main-menu:multiplayer" },
                    new ObserverChoice("menu-action", "Settings", "620,560,420,96") { NodeId = "main-menu:settings" },
                },
                Array.Empty<ObservedCombatHandCard>()),
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            Array.Empty<string>(),
            Array.Empty<GuiSmokeHistoryEntry>(),
            string.Empty,
            null));
        Assert(
            string.Equals(collapsedRunStartDecision.Status, "wait", StringComparison.OrdinalIgnoreCase),
            "EnterRun should wait while main-menu buttons still share collapsed placeholder bounds.");
        var readyMainMenuObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-main-menu",
                true,
                "main-menu",
                "stable",
                null,
                null,
                null,
                null,
                null,
                null,
                new[] { "Continue" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("continue", "action", "Continue", "620,560,420,96", true),
                },
                new[]
                {
                    new ObserverChoice("choice", "Continue", "620,560,420,96"),
                },
                Array.Empty<ObservedCombatHandCard>()),
            null,
            null,
            null);
        Assert(
            evaluator.IsPhaseSatisfied(GuiSmokePhase.WaitMainMenu, readyMainMenuObserver),
            "WaitMainMenu should still accept fresh exported Continue or Singleplayer run-start surfaces.");
        var collapsedMainMenuObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-main-menu-collapsed",
                true,
                "main-menu",
                "stable",
                null,
                null,
                null,
                null,
                null,
                null,
                Array.Empty<string>(),
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("main-menu:continue", "continue-run", "Continue", "620,560,420,96", true),
                    new ObserverActionNode("main-menu:multiplayer", "menu-action", "Multiplayer", "620,560,420,96", true),
                },
                new[]
                {
                    new ObserverChoice("continue-run", "Continue", "620,560,420,96") { NodeId = "main-menu:continue" },
                    new ObserverChoice("menu-action", "Multiplayer", "620,560,420,96") { NodeId = "main-menu:multiplayer" },
                },
                Array.Empty<ObservedCombatHandCard>()),
            null,
            null,
            null);
        Assert(
            !evaluator.IsPhaseSatisfied(GuiSmokePhase.WaitMainMenu, collapsedMainMenuObserver),
            "WaitMainMenu should reject collapsed main-menu action layouts until distinct clickable bounds are exported.");

        var publishedCharacterSelectObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-character-select-published",
                false,
                "compat",
                "stabilizing",
                null,
                null,
                null,
                null,
                null,
                null,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                Array.Empty<ObserverChoice>(),
                Array.Empty<ObservedCombatHandCard>())
            {
                PublishedCurrentScreen = "character-select",
                PublishedVisibleScreen = "character-select",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "published",
                PublishedSceneStability = "stable",
                CompatibilityCurrentScreen = "main-menu",
                CompatibilityVisibleScreen = "main-menu",
                CompatibilitySceneReady = false,
                CompatibilitySceneAuthority = "compat",
                CompatibilitySceneStability = "stabilizing",
            },
            null,
            null,
            null);
        Assert(
            evaluator.IsPhaseSatisfied(GuiSmokePhase.WaitCharacterSelect, publishedCharacterSelectObserver),
            "WaitCharacterSelect should accept published character-select readiness even when compatibility aliases still lag on main-menu.");
        Assert(
            GuiSmokeObserverPhaseHeuristics.TryGetPostCharacterSelectPhase(publishedCharacterSelectObserver, out var publishedCharacterSelectPhase)
            && publishedCharacterSelectPhase == GuiSmokePhase.ChooseCharacter,
            "Post-character-select routing should prefer published character-select provenance over stale compatibility aliases.");

        var logoAnimationMainMenuObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-logo-main-menu",
                true,
                "mixed",
                "stable",
                "episode-logo-main-menu",
                null,
                "generic",
                null,
                null,
                null,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                Array.Empty<ObserverChoice>(),
                Array.Empty<ObservedCombatHandCard>())
            {
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NLogoAnimation",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NLogoAnimation",
                    ["rootSceneIsMainMenu"] = "true",
                    ["choiceExtractorPath"] = "generic",
                },
            },
            null,
            null,
            null);
        Assert(
            MainMenuRunStartObserverSignals.IsLogoAnimationOnlyMainMenu(logoAnimationMainMenuObserver),
            "Main-menu readiness helper should recognize logo-animation-only bootstrap states.");
        Assert(
            !evaluator.IsPhaseSatisfied(GuiSmokePhase.WaitMainMenu, logoAnimationMainMenuObserver),
            "WaitMainMenu must not accept a logo-animation-only main-menu state before run-start actions exist.");

        var bootstrapVisibleRunStartObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                "inv-bootstrap-visible-run-start",
                true,
                "mixed",
                "stable",
                "episode-bootstrap-visible-run-start",
                null,
                "main-menu",
                null,
                null,
                null,
                new[] { "Continue" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("main-menu:continue", "continue-run", "Continue", "620,560,420,96", true),
                },
                new[]
                {
                    new ObserverChoice("continue-run", "Continue", "620,560,420,96", "main-menu:continue")
                    {
                        NodeId = "main-menu:continue",
                        BindingKind = "continue-run",
                    },
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                PublishedVisibleScreen = "bootstrap",
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    ["choiceExtractorPath"] = "main-menu",
                    ["publishedVisibleScreen"] = "bootstrap",
                },
            },
            null,
            null,
            null);
        Assert(
            !MainMenuRunStartObserverSignals.IsRunStartSurfaceReady(bootstrapVisibleRunStartObserver),
            "Main-menu readiness helper should reject run-start surfaces while published visibility still reports bootstrap foreground.");
        Assert(
            !evaluator.IsPhaseSatisfied(GuiSmokePhase.WaitMainMenu, bootstrapVisibleRunStartObserver),
            "WaitMainMenu must not accept run-start actions while published visibility still reports bootstrap foreground.");

        var reachableNodeDecision = new GuiSmokeStepDecision("act", "click", null, null, null, "visible reachable node", "Map node selected.", 0.9, null, null, null, null);
        Assert(
            GetPostChooseFirstNodePhase(reachableNodeDecision) == GuiSmokePhase.WaitPostMapNodeRoom,
            "Reachable map-node clicks should enter neutral post-node room reconciliation instead of combat-only waiting.");
        var exportedReachableNodeDecision = new GuiSmokeStepDecision("act", "click", null, null, null, "exported reachable map node", "Map node selected from explicit export.", 0.95, null, null, null, null);
        Assert(
            GetPostChooseFirstNodePhase(exportedReachableNodeDecision) == GuiSmokePhase.WaitPostMapNodeRoom,
            "Exported reachable map-node clicks should enter neutral post-node room reconciliation instead of falling back to WaitMap.");

        var combatAfterMapTransitionObserver = new ObserverState(
            new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                null,
                true,
                "mixed",
                "stable",
                null,
                "Boss",
                "combat",
                38,
                80,
                3,
                new[] { "1턴 종료" },
                new[]
                {
                    "{\"kind\":\"map-point-selected\",\"screen\":\"map\"}",
                    "{\"kind\":\"screen-changed\",\"screen\":\"map\"}",
                    "{\"kind\":\"combat-started\",\"screen\":\"combat\"}",
                },
                Array.Empty<ObserverActionNode>(),
                Array.Empty<ObserverChoice>(),
                Array.Empty<ObservedCombatHandCard>())
            {
                PublishedCurrentScreen = "combat",
                PublishedVisibleScreen = "combat",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stable",
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom",
                    ["combatPrimaryValue"] = "true",
                },
            },
            null,
            null,
            null);
        Assert(
            GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(combatAfterMapTransitionObserver.Summary),
            "Combat observer with NCombatRoom authority should still be recognized as combat after a map-node click.");
        Assert(
            !GuiSmokeObserverPhaseHeuristics.LooksLikeMapState(combatAfterMapTransitionObserver),
            "Stale map transition tails must not outvote stronger combat truth once the destination room is combat.");
        Assert(
            !evaluator.IsPhaseSatisfied(GuiSmokePhase.WaitMap, combatAfterMapTransitionObserver),
            "WaitMap should not accept a combat observer just because stale map-transition tails are still present.");

        var waitPostMapNodeBranchRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-wait-post-map-node-branch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(waitPostMapNodeBranchRoot);
        try
        {
            var waitPostMapNodeLogger = new ArtifactRecorder(waitPostMapNodeBranchRoot);
            Assert(
                GuiSmokeObserverPhaseHeuristics.TryGetPostMapNodePhase(
                    new ObserverState(
                        new ObserverSummary(
                            "combat",
                            "combat",
                            true,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "hook",
                            "stable",
                            null,
                            "Monster",
                            "combat",
                            80,
                            80,
                            null,
                            Array.Empty<string>(),
                            Array.Empty<string>(),
                            Array.Empty<ObserverActionNode>(),
                            Array.Empty<ObserverChoice>(),
                            Array.Empty<ObservedCombatHandCard>())
                        {
                            PublishedCurrentScreen = "combat",
                            PublishedVisibleScreen = "combat",
                            PublishedSceneReady = true,
                            PublishedSceneAuthority = "hook",
                            PublishedSceneStability = "stable",
                        },
                        null,
                        null,
                        null),
                    out var postMapNodeCombatPhase)
                && postMapNodeCombatPhase == GuiSmokePhase.HandleCombat,
                "Post-node reconciliation should still recognize combat destinations.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitPostMapNodeRoom,
                    new ObserverState(
                        new ObserverSummary(
                            "combat",
                            "combat",
                            true,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "hook",
                            "stable",
                            null,
                            "Monster",
                            "combat",
                            80,
                            80,
                            null,
                            Array.Empty<string>(),
                            Array.Empty<string>(),
                            Array.Empty<ObserverActionNode>(),
                            Array.Empty<ObserverChoice>(),
                            Array.Empty<ObservedCombatHandCard>())
                        {
                            PublishedCurrentScreen = "combat",
                            PublishedVisibleScreen = "combat",
                            PublishedSceneReady = true,
                            PublishedSceneAuthority = "hook",
                            PublishedSceneStability = "stable",
                        },
                        null,
                        null,
                        null),
                    new List<GuiSmokeHistoryEntry>(),
                    waitPostMapNodeLogger,
                    10,
                    true,
                    out var waitPostMapNodeCombatBranchPhase)
                && waitPostMapNodeCombatBranchPhase == GuiSmokePhase.HandleCombat,
                "WaitPostMapNodeRoom should reopen combat handling when the destination room is combat.");
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.ChooseFirstNode,
                    combatAfterMapTransitionObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitPostMapNodeLogger,
                    11,
                    true,
                    out var chooseFirstNodeCombatBranchPhase)
                && chooseFirstNodeCombatBranchPhase == GuiSmokePhase.HandleCombat,
                "ChooseFirstNode should recover directly to combat handling if phase bookkeeping drifts after a real map-node click.");

            var chooseFirstNodeCombatWithStaleMapObserver = new ObserverState(
                new ObserverSummary(
                    "map",
                    "map",
                    true,
                    DateTimeOffset.UtcNow,
                    null,
                    null,
                    null,
                    null,
                    null,
                    "Boss",
                    "combat",
                    65,
                    80,
                    3,
                    new[] { "DrawPile", "DiscardPile", "ExhaustPile", "1턴 종료" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("map:0:3", "map-node", "Ancient (0,3)", "880,1260,208,208", true)
                        {
                            TypeName = "map-node",
                            SemanticHints = new[] { "scene:map", "kind:map-node", "source:map-choice" },
                        },
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "DrawPile", "15,985,80,80", null, "DrawPile"),
                        new ObserverChoice("choice", "DiscardPile", "1826,985,80,80", null, "DiscardPile"),
                        new ObserverChoice("choice", "ExhaustPile", "1830,800,80,80", null, "ExhaustPile"),
                        new ObserverChoice("choice", "1턴 종료", "1604,1096,220,90", null, "1턴 종료"),
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    PublishedCurrentScreen = "map",
                    PublishedVisibleScreen = "map",
                    CompatibilityCurrentScreen = "combat",
                    CompatibilityVisibleScreen = "bootstrap",
                    CompatibilitySceneReady = false,
                    CompatibilitySceneAuthority = "hook",
                    CompatibilitySceneStability = "transient",
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom",
                        ["combatPrimaryValue"] = "true",
                    },
                },
                null,
                null,
                null);
            Assert(
                !AutoDecisionProvider.HasExplicitEventRecoveryAuthority(
                    chooseFirstNodeCombatWithStaleMapObserver,
                    null,
                    Array.Empty<GuiSmokeHistoryEntry>()),
                "Combat observers with stale map-node surfaces must not reopen the explicit event recovery lane.");
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.ChooseFirstNode,
                    chooseFirstNodeCombatWithStaleMapObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitPostMapNodeLogger,
                    12,
                    true,
                    out var chooseFirstNodeCombatWithStaleMapPhase)
                && chooseFirstNodeCombatWithStaleMapPhase == GuiSmokePhase.HandleCombat,
                "ChooseFirstNode should still recover to HandleCombat when combat truth outranks stale map-node and generic choice surfaces.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitPostMapNodeRoom,
                    new ObserverState(
                        new ObserverSummary(
                            "rest-site",
                            "rest-site",
                            false,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "rest",
                            "stable",
                            null,
                            "RestSite",
                            "rest",
                            64,
                            80,
                            null,
                            new[] { "휴식", "재련", "부화" },
                            Array.Empty<string>(),
                            Array.Empty<ObserverActionNode>(),
                            new[]
                            {
                                new ObserverChoice("rest-option", "휴식", "520,360,220,160", null, "Rest")
                                {
                                    NodeId = "rest-site:HEAL",
                                    BindingKind = "rest-site-option",
                                    Enabled = true,
                                    SemanticHints = new[] { "scene:rest-site", "option-id:HEAL", "source:button" },
                                },
                            },
                            Array.Empty<ObservedCombatHandCard>()),
                        null,
                        null,
                        null),
                    new List<GuiSmokeHistoryEntry>(),
                    waitPostMapNodeLogger,
                    11,
                    true,
                    out var waitPostMapNodeRestSitePhase)
                && waitPostMapNodeRestSitePhase == GuiSmokePhase.ChooseFirstNode,
                "WaitPostMapNodeRoom should reopen rest-site handling instead of stalling in combat wait.");

            using var waitPostMapNodeRestSiteReleaseDocument = JsonDocument.Parse("""{"meta":{"mapOverlayVisible":"true"}}""");
            var waitPostMapNodeRestSiteReleaseHistory = new List<GuiSmokeHistoryEntry>();
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitPostMapNodeRoom,
                    new ObserverState(
                        new ObserverSummary(
                            "rest-site",
                            "rest-site",
                            false,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "map",
                            "stable",
                            null,
                            "RestSite",
                            "map",
                            64,
                            80,
                            null,
                            Array.Empty<string>(),
                            Array.Empty<string>(),
                            new[]
                            {
                                new ObserverActionNode("map:6:5", "map-node", "Unknown (6,5)", "770,528,56,56", true)
                                {
                                    TypeName = "map-node",
                                },
                            },
                            new[]
                            {
                                new ObserverChoice("map-node", "Unknown (6,5)", "770,528,56,56", "6,5", "type:Unknown;state:Travelable;coord:6,5"),
                            },
                            Array.Empty<ObservedCombatHandCard>())
                        {
                            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["restSiteSelectionLastSignal"] = "after-select-success",
                                ["restSiteSelectionLastSuccess"] = "true",
                            },
                        },
                        waitPostMapNodeRestSiteReleaseDocument,
                        null,
                        null),
                    waitPostMapNodeRestSiteReleaseHistory,
                    waitPostMapNodeLogger,
                    11,
                    true,
                    out var waitPostMapNodeRestSiteReleasePhase)
                && waitPostMapNodeRestSiteReleasePhase == GuiSmokePhase.ChooseFirstNode
                && string.Equals(waitPostMapNodeRestSiteReleaseHistory[^1].Action, "branch-rest-site", StringComparison.OrdinalIgnoreCase),
                "WaitPostMapNodeRoom should keep rest-site release-pending as the owner instead of reopening branch-map from exported node residue.");

            var waitMapRestSiteReleaseHistory = new List<GuiSmokeHistoryEntry>();
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitMap,
                    new ObserverState(
                        new ObserverSummary(
                            "rest-site",
                            "rest-site",
                            false,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "map",
                            "stable",
                            null,
                            "RestSite",
                            "map",
                            64,
                            80,
                            null,
                            Array.Empty<string>(),
                            Array.Empty<string>(),
                            new[]
                            {
                                new ObserverActionNode("map:6:5", "map-node", "Unknown (6,5)", "770,528,56,56", true)
                                {
                                    TypeName = "map-node",
                                },
                            },
                            new[]
                            {
                                new ObserverChoice("map-node", "Unknown (6,5)", "770,528,56,56", "6,5", "type:Unknown;state:Travelable;coord:6,5"),
                            },
                            Array.Empty<ObservedCombatHandCard>())
                        {
                            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["restSiteSelectionLastSignal"] = "after-select-success",
                                ["restSiteSelectionLastSuccess"] = "true",
                            },
                        },
                        waitPostMapNodeRestSiteReleaseDocument,
                        null,
                        null),
                    waitMapRestSiteReleaseHistory,
                    waitPostMapNodeLogger,
                    12,
                    true,
                    out var waitMapRestSiteReleasePhase)
                && waitMapRestSiteReleasePhase == GuiSmokePhase.ChooseFirstNode
                && string.Equals(waitMapRestSiteReleaseHistory[^1].Action, "branch-rest-site", StringComparison.OrdinalIgnoreCase),
                "WaitMap should read the same rest-site release-pending handoff truth instead of reopening branch-map from exported node residue.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitPostMapNodeRoom,
                    new ObserverState(
                        new ObserverSummary(
                            "shop",
                            "shop",
                            false,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "shop",
                            "stable",
                            null,
                            "Shop",
                            "shop",
                            80,
                            80,
                            null,
                            Array.Empty<string>(),
                            Array.Empty<string>(),
                            Array.Empty<ObserverActionNode>(),
                            Array.Empty<ObserverChoice>(),
                            Array.Empty<ObservedCombatHandCard>()),
                        null,
                        null,
                        null),
                    new List<GuiSmokeHistoryEntry>(),
                    waitPostMapNodeLogger,
                    12,
                    true,
                    out var waitPostMapNodeShopPhase)
                && waitPostMapNodeShopPhase == GuiSmokePhase.HandleShop,
                "WaitPostMapNodeRoom should reopen shop handling when the destination room is shop.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitPostMapNodeRoom,
                    new ObserverState(
                        new ObserverSummary(
                            "shop",
                            "shop",
                            false,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "shop",
                            "transient",
                            null,
                            "None",
                            "shop",
                            80,
                            80,
                            null,
                            new[] { "회중시계", "카드 제거 서비스" },
                            Array.Empty<string>(),
                            Array.Empty<ObserverActionNode>(),
                            new[]
                            {
                                new ObserverChoice("relic", "회중시계", null),
                                new ObserverChoice("card", "카드 제거 서비스", null),
                            },
                            Array.Empty<ObservedCombatHandCard>())
                        {
                            PublishedCurrentScreen = "shop",
                            PublishedVisibleScreen = "shop",
                            PublishedSceneReady = false,
                            PublishedSceneAuthority = "hook",
                            PublishedSceneStability = "transient",
                            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["shopRoomDetected"] = "false",
                                ["shopForegroundOwned"] = "false",
                                ["shopRoomVisible"] = "false",
                                ["shopInventoryOpen"] = "false",
                                ["shopMerchantButtonVisible"] = "false",
                                ["shopMerchantButtonEnabled"] = "false",
                                ["shopProceedEnabled"] = "false",
                                ["shopBackVisible"] = "false",
                                ["shopBackEnabled"] = "false",
                                ["shopOptionCount"] = "0",
                                ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantRoom",
                                ["rawCurrentActiveScreenType"] = "MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantRoom",
                                ["transitionInProgress"] = "true",
                            },
                        },
                        null,
                        null,
                        null),
                    new List<GuiSmokeHistoryEntry>
                    {
                        new(GuiSmokePhase.ChooseFirstNode.ToString(), "click", "exported reachable map node", DateTimeOffset.UtcNow.AddMilliseconds(-200)),
                    },
                    waitPostMapNodeLogger,
                    13,
                    true,
                    out var waitPostMapNodeShopMetaFalsePhase)
                && waitPostMapNodeShopMetaFalsePhase == GuiSmokePhase.HandleShop,
                "WaitPostMapNodeRoom should trust explicit shop room-entry surfaces even when stale shop meta still says false.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitPostMapNodeRoom,
                    new ObserverState(
                        new ObserverSummary(
                            "rewards",
                            "rewards",
                            false,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "reward",
                            "stable",
                            null,
                            "Monster",
                            "reward",
                            80,
                            80,
                            null,
                            new[] { "11 골드", "넘기기" },
                            Array.Empty<string>(),
                            new[]
                            {
                                new ObserverActionNode("reward-item:0", "reward-item", "11 골드", "758,374,402,86", true),
                                new ObserverActionNode("reward-proceed:0", "proceed", "넘기기", "1583,764,269,108", true),
                            },
                            new[]
                            {
                                new ObserverChoice("choice", "11 골드", "758,374,402,86"),
                                new ObserverChoice("choice", "넘기기", "1583,764,269,108"),
                            },
                            Array.Empty<ObservedCombatHandCard>())
                        {
                            PublishedCurrentScreen = "rewards",
                            PublishedVisibleScreen = "rewards",
                            PublishedSceneReady = true,
                            PublishedSceneAuthority = "reward",
                            PublishedSceneStability = "stable",
                            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["rewardScreenDetected"] = "true",
                                ["rewardScreenVisible"] = "true",
                                ["rewardForegroundOwned"] = "true",
                                ["rewardTeardownInProgress"] = "false",
                                ["rewardIsCurrentActiveScreen"] = "true",
                                ["rewardIsTopOverlay"] = "true",
                                ["rewardProceedVisible"] = "true",
                                ["rewardProceedEnabled"] = "true",
                                ["rewardVisibleButtonCount"] = "1",
                                ["rewardEnabledButtonCount"] = "1",
                                ["choiceExtractorPath"] = "reward",
                            },
                        },
                        null,
                        null,
                        null),
                    new List<GuiSmokeHistoryEntry>(),
                    waitPostMapNodeLogger,
                    13,
                    true,
                    out var waitPostMapNodeRewardPhase)
                && waitPostMapNodeRewardPhase == GuiSmokePhase.HandleRewards,
                "WaitPostMapNodeRoom should reopen reward handling when the destination room is rewards.");

            ObserverState CreateShopObserver(
                bool inventoryOpen,
                bool merchantButtonVisible = false,
                bool merchantButtonEnabled = false,
                bool proceedEnabled = false,
                bool backVisible = false,
                bool backEnabled = false,
                bool roomVisible = true,
                bool foregroundOwned = true,
                bool teardownInProgress = false,
                bool shopIsCurrentActiveScreen = true,
                bool mapCurrentActiveScreen = false,
                string? activeScreenType = "MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantRoom",
                int affordableOptionCount = 0,
                bool cardRemovalVisible = false,
                bool cardRemovalEnabled = false,
                bool cardRemovalEnoughGold = false,
                bool cardRemovalUsed = false,
                ObserverChoice[]? choices = null,
                IReadOnlyDictionary<string, string?>? extraMeta = null)
            {
                var meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["shopRoomDetected"] = "true",
                    ["shopRoomVisible"] = roomVisible.ToString().ToLowerInvariant(),
                    ["shopForegroundOwned"] = foregroundOwned.ToString().ToLowerInvariant(),
                    ["shopTeardownInProgress"] = teardownInProgress.ToString().ToLowerInvariant(),
                    ["shopIsCurrentActiveScreen"] = shopIsCurrentActiveScreen.ToString().ToLowerInvariant(),
                    ["mapCurrentActiveScreen"] = mapCurrentActiveScreen.ToString().ToLowerInvariant(),
                    ["activeScreenType"] = activeScreenType,
                    ["shopRootType"] = "MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantRoom",
                    ["shopInventoryOpen"] = inventoryOpen.ToString().ToLowerInvariant(),
                    ["shopMerchantButtonVisible"] = merchantButtonVisible.ToString().ToLowerInvariant(),
                    ["shopMerchantButtonEnabled"] = merchantButtonEnabled.ToString().ToLowerInvariant(),
                    ["shopProceedEnabled"] = proceedEnabled.ToString().ToLowerInvariant(),
                    ["shopBackVisible"] = backVisible.ToString().ToLowerInvariant(),
                    ["shopBackEnabled"] = backEnabled.ToString().ToLowerInvariant(),
                    ["shopOptionCount"] = (choices?.Count(static choice => choice.Kind.StartsWith("shop-option", StringComparison.OrdinalIgnoreCase) || string.Equals(choice.Kind, "shop-card-removal", StringComparison.OrdinalIgnoreCase)) ?? 0).ToString(CultureInfo.InvariantCulture),
                    ["shopAffordableOptionCount"] = affordableOptionCount.ToString(CultureInfo.InvariantCulture),
                    ["shopCardRemovalVisible"] = cardRemovalVisible.ToString().ToLowerInvariant(),
                    ["shopCardRemovalEnabled"] = cardRemovalEnabled.ToString().ToLowerInvariant(),
                    ["shopCardRemovalEnoughGold"] = cardRemovalEnoughGold.ToString().ToLowerInvariant(),
                    ["shopCardRemovalUsed"] = cardRemovalUsed.ToString().ToLowerInvariant(),
                };
                if (extraMeta is not null)
                {
                    foreach (var entry in extraMeta)
                    {
                        meta[entry.Key] = entry.Value;
                    }
                }

                var summary = new ObserverSummary(
                    "shop",
                    "shop",
                    false,
                    DateTimeOffset.UtcNow,
                    null,
                    true,
                    "shop",
                    "stable",
                    null,
                    "Shop",
                    "shop",
                    80,
                    80,
                    null,
                    choices?.Select(static choice => choice.Label).ToArray() ?? Array.Empty<string>(),
                    Array.Empty<string>(),
                    Array.Empty<ObserverActionNode>(),
                    choices ?? Array.Empty<ObserverChoice>(),
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = meta,
                };

                return new ObserverState(summary, null, null, null);
            }

            GuiSmokeStepDecision DecideShop(ObserverState observer, IReadOnlyList<GuiSmokeHistoryEntry>? history = null)
            {
                history ??= Array.Empty<GuiSmokeHistoryEntry>();
                return AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                    "run",
                    "shop-self-test",
                    12,
                    GuiSmokePhase.HandleShop.ToString(),
                    "Resolve explicit shop room semantics.",
                    DateTimeOffset.UtcNow,
                    "screen.png",
                    new WindowBounds(0, 0, 1280, 720),
                    "phase:shop|screen:shop|visible:shop|encounter:shop|ready:true|stability:stable|room:shop",
                    "shop-self-test",
                    1,
                    1,
                    true,
                    "tactical",
                    null,
                    observer.Summary,
                    Array.Empty<KnownRecipeHint>(),
                    Array.Empty<EventKnowledgeCandidate>(),
                    Array.Empty<CombatCardKnowledgeHint>(),
                    BuildAllowedActions(GuiSmokePhase.HandleShop, observer, Array.Empty<CombatCardKnowledgeHint>(), null, history),
                    history,
                    "shop self-test",
                    null));
            }

            GuiSmokeStepDecision DecideRewardsFallback(ObserverState observer, IReadOnlyList<GuiSmokeHistoryEntry>? history = null)
            {
                history ??= Array.Empty<GuiSmokeHistoryEntry>();
                return AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                    "run",
                    "reward-fallback-self-test",
                    12,
                    GuiSmokePhase.HandleRewards.ToString(),
                    "Resolve reward aftermath.",
                    DateTimeOffset.UtcNow,
                    "screen.png",
                    new WindowBounds(0, 0, 1280, 720),
                    "phase:handlerewards|screen:shop|visible:shop|encounter:shop|ready:true|stability:stable|room:shop",
                    "reward-fallback-self-test",
                    1,
                    1,
                    true,
                    "tactical",
                    null,
                    observer.Summary,
                    Array.Empty<KnownRecipeHint>(),
                    Array.Empty<EventKnowledgeCandidate>(),
                    Array.Empty<CombatCardKnowledgeHint>(),
                    BuildAllowedActions(GuiSmokePhase.HandleRewards, observer, Array.Empty<CombatCardKnowledgeHint>(), null, history),
                    history,
                    "reward fallback self-test",
                    null));
            }

            var embarkShopObserver = CreateShopObserver(inventoryOpen: true);
            Assert(GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(embarkShopObserver, out var postEmbarkShopPhase) && postEmbarkShopPhase == GuiSmokePhase.HandleShop, "Embark should reconcile to HandleShop when observer already reports a shop room.");

            var shopOpenInventoryObserver = CreateShopObserver(
                inventoryOpen: false,
                merchantButtonVisible: true,
                merchantButtonEnabled: true,
                choices: new[]
                {
                    new ObserverChoice("shop-open-inventory", "Merchant", "420,320,240,180")
                    {
                        BindingKind = "shop-room",
                        BindingId = "merchant-button",
                        Enabled = true,
                    },
                });
            Assert(BuildAllowedActions(GuiSmokePhase.HandleShop, shopOpenInventoryObserver, Array.Empty<CombatCardKnowledgeHint>(), null, Array.Empty<GuiSmokeHistoryEntry>()).Contains("click shop open inventory", StringComparer.OrdinalIgnoreCase), "HandleShop allowlist should open merchant inventory when it is closed and the merchant button is enabled.");
            Assert(string.Equals(DecideShop(shopOpenInventoryObserver).TargetLabel, "shop open inventory", StringComparison.OrdinalIgnoreCase), "HandleShop should click the explicit merchant button when inventory is closed.");
            Assert(AutoDecisionProvider.BuildShopSceneState(shopOpenInventoryObserver, Array.Empty<GuiSmokeHistoryEntry>()) is
                {
                    CanonicalForegroundOwner: NonCombatCanonicalForegroundOwner.Shop,
                    HandoffTarget: NonCombatHandoffTarget.HandleShop,
                    AllowsFastForegroundWait: true,
                },
                "Shop scene state should preserve shop ownership, HandleShop handoff, and fast-wait eligibility.");
            Assert(BuildAllowedActions(GuiSmokePhase.HandleRewards, shopOpenInventoryObserver, Array.Empty<CombatCardKnowledgeHint>(), null, Array.Empty<GuiSmokeHistoryEntry>()).Contains("click shop open inventory", StringComparer.OrdinalIgnoreCase), "HandleRewards fallback should reopen explicit shop authority instead of collapsing to reward wait.");
            Assert(string.Equals(DecideRewardsFallback(shopOpenInventoryObserver).TargetLabel, "shop open inventory", StringComparison.OrdinalIgnoreCase), "HandleRewards fallback should delegate explicit shop authority to HandleShop decisions.");
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.HandleRewards,
                    shopOpenInventoryObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    new ArtifactRecorder(Path.Combine(Path.GetTempPath(), $"gui-smoke-handle-reward-shop-reopen-{Guid.NewGuid():N}")),
                    12,
                    true,
                    out var rewardFallbackShopPhase)
                && rewardFallbackShopPhase == GuiSmokePhase.HandleShop,
                "HandleRewards should alternate-branch back to HandleShop when explicit shop authority is foreground-active.");

            var shopRelicObserver = CreateShopObserver(
                inventoryOpen: true,
                backVisible: true,
                backEnabled: true,
                affordableOptionCount: 1,
                choices: new[]
                {
                    new ObserverChoice("shop-option:relic", "게임용 말", "420,320,180,180")
                    {
                        BindingKind = "shop-option",
                        BindingId = "RELIC.GAME_PIECE",
                        Enabled = true,
                        SemanticHints = new[] { "scene:shop", "shop-type:relic" },
                    },
                    new ObserverChoice("shop-back", "Back", "220,160,140,100")
                    {
                        BindingKind = "shop-room",
                        BindingId = "back",
                        Enabled = true,
                    },
                });
            Assert(string.Equals(DecideShop(shopRelicObserver).TargetLabel, "shop buy relic", StringComparison.OrdinalIgnoreCase), "HandleShop should buy an affordable relic before backing out.");

            var shopCardObserver = CreateShopObserver(
                inventoryOpen: true,
                backVisible: true,
                backEnabled: true,
                affordableOptionCount: 1,
                choices: new[]
                {
                    new ObserverChoice("shop-option:card", "몸풀기", "420,320,180,180")
                    {
                        BindingKind = "shop-option",
                        BindingId = "CARD.WARM_UP",
                        Enabled = true,
                        SemanticHints = new[] { "scene:shop", "shop-type:card" },
                    },
                });
            Assert(string.Equals(DecideShop(shopCardObserver).TargetLabel, "shop buy card", StringComparison.OrdinalIgnoreCase), "HandleShop should buy an affordable card when no relic is chosen.");

            var shopPotionObserver = CreateShopObserver(
                inventoryOpen: true,
                backVisible: true,
                backEnabled: true,
                affordableOptionCount: 1,
                choices: new[]
                {
                    new ObserverChoice("shop-option:potion", "힘 포션", "420,320,180,180")
                    {
                        BindingKind = "shop-option",
                        BindingId = "POTION.STRENGTH_POTION",
                        Enabled = true,
                        SemanticHints = new[] { "scene:shop", "shop-type:potion" },
                    },
                });
            Assert(string.Equals(DecideShop(shopPotionObserver).TargetLabel, "shop buy potion", StringComparison.OrdinalIgnoreCase), "HandleShop should buy an affordable potion when no relic or card is chosen.");

            var shopCardRemovalObserver = CreateShopObserver(
                inventoryOpen: true,
                backVisible: true,
                backEnabled: true,
                affordableOptionCount: 0,
                cardRemovalVisible: true,
                cardRemovalEnabled: true,
                cardRemovalEnoughGold: true,
                choices: new[]
                {
                    new ObserverChoice("shop-card-removal", "카드 제거 서비스", "420,320,180,180")
                    {
                        BindingKind = "shop-card-removal",
                        BindingId = "card-removal",
                        Enabled = true,
                        SemanticHints = new[] { "scene:shop", "shop-type:card-removal", "enough-gold:true" },
                    },
                });
            Assert(string.Equals(DecideShop(shopCardRemovalObserver).TargetLabel, "shop card removal", StringComparison.OrdinalIgnoreCase), "HandleShop should treat card removal as a distinct shop service, not a normal card purchase.");

            var shopBackObserver = CreateShopObserver(
                inventoryOpen: true,
                backVisible: true,
                backEnabled: true,
                choices: new[]
                {
                    new ObserverChoice("shop-back", "Back", "220,160,140,100")
                    {
                        BindingKind = "shop-room",
                        BindingId = "back",
                        Enabled = true,
                    },
                });
            Assert(string.Equals(DecideShop(shopBackObserver).TargetLabel, "shop back", StringComparison.OrdinalIgnoreCase), "HandleShop should close inventory explicitly when no bounded purchase remains.");

            var shopProceedObserver = CreateShopObserver(
                inventoryOpen: false,
                proceedEnabled: true,
                choices: new[]
                {
                    new ObserverChoice("shop-proceed", "Proceed", "980,820,240,120")
                    {
                        BindingKind = "shop-room",
                        BindingId = "proceed",
                        Enabled = true,
                    },
                });
            Assert(string.Equals(DecideShop(shopProceedObserver).TargetLabel, "shop proceed", StringComparison.OrdinalIgnoreCase), "HandleShop should click explicit shop proceed when inventory is closed and proceed is enabled.");

            var staleVisibleShopObserver = CreateShopObserver(
                inventoryOpen: false,
                merchantButtonVisible: true,
                merchantButtonEnabled: false,
                proceedEnabled: false,
                backVisible: false,
                backEnabled: false,
                roomVisible: true,
                foregroundOwned: false,
                teardownInProgress: true,
                shopIsCurrentActiveScreen: false,
                mapCurrentActiveScreen: true,
                activeScreenType: "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                choices: new[]
                {
                    new ObserverChoice("shop-open-inventory", "Merchant", "420,320,240,180")
                    {
                        BindingKind = "shop-room",
                        BindingId = "merchant-button",
                        Enabled = false,
                    },
                });
            Assert(!ShopObserverSignals.IsShopAuthorityActive(staleVisibleShopObserver.Summary), "Visible-but-disabled merchant remnants should not keep shop foreground authority after proceed teardown starts.");
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.HandleShop,
                    staleVisibleShopObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    new ArtifactRecorder(Path.Combine(Path.GetTempPath(), $"gui-smoke-handle-shop-release-{Guid.NewGuid():N}")),
                    12,
                    true,
                    out var postShopPhase)
                && postShopPhase == GuiSmokePhase.WaitMap,
                "HandleShop should release ownership to map aftermath reconciliation when shop teardown is active.");

            var shopWithFalseMetaObserver = CreateShopObserver(
                inventoryOpen: false,
                merchantButtonVisible: false,
                merchantButtonEnabled: false,
                proceedEnabled: false,
                backVisible: false,
                backEnabled: false,
                roomVisible: false,
                foregroundOwned: false,
                shopIsCurrentActiveScreen: false,
                choices: new[]
                {
                    new ObserverChoice("shop-option:relic", "회중시계", "420,320,180,180")
                    {
                        BindingKind = "shop-option",
                        BindingId = "RELIC.POCKETWATCH",
                        Enabled = true,
                        SemanticHints = new[] { "scene:shop", "shop-type:relic" },
                    },
                },
                extraMeta: new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["shopRoomDetected"] = "false",
                    ["shopForegroundOwned"] = "false",
                    ["shopRoomVisible"] = "false",
                    ["shopInventoryOpen"] = "false",
                    ["choiceExtractorPath"] = "shop",
                });
            Assert(AutoDecisionProvider.BuildShopSceneState(shopWithFalseMetaObserver, Array.Empty<GuiSmokeHistoryEntry>()) is ShopSceneState
                {
                    CanonicalForegroundOwner: NonCombatCanonicalForegroundOwner.Shop,
                    ReleaseStage: NonCombatReleaseStage.Active,
                },
                "Explicit shop room-entry surfaces should outrank stale false shop metadata during mixed post-map entry.");

            var laggingScreenShopObserver = shopWithFalseMetaObserver with
            {
                Summary = shopWithFalseMetaObserver.Summary with
                {
                    CurrentScreen = "map",
                    VisibleScreen = "map",
                    ChoiceExtractorPath = "map",
                },
            };
            var laggingScreenNextRoomEntryState = AutoDecisionProvider.BuildNextRoomEntryState(
                laggingScreenShopObserver,
                null,
                new[]
                {
                    new GuiSmokeHistoryEntry(
                        GuiSmokePhase.ChooseFirstNode.ToString(),
                        "click",
                        "exported reachable map node",
                        DateTimeOffset.UtcNow.AddSeconds(-1)),
                });
            Assert(
                laggingScreenNextRoomEntryState is
                {
                    Owner: NonCombatCanonicalForegroundOwner.Shop,
                    HandoffTarget: NonCombatHandoffTarget.HandleShop,
                    ExplicitSurfacePresent: true,
                    MapTransitPending: false,
                },
                $"Post-map winner selection should still promote an active shop room when the screen/extractor strings lag behind the explicit shop surface. Actual owner={laggingScreenNextRoomEntryState.Owner} target={laggingScreenNextRoomEntryState.HandoffTarget} explicit={laggingScreenNextRoomEntryState.ExplicitSurfacePresent} pending={laggingScreenNextRoomEntryState.MapTransitPending}.");

            var shopWithStaleRewardObserver = CreateShopObserver(
                inventoryOpen: true,
                backVisible: true,
                backEnabled: true,
                choices: new[]
                {
                    new ObserverChoice("shop-back", "Back", "220,160,140,100")
                    {
                        BindingKind = "shop-room",
                        BindingId = "back",
                        Enabled = true,
                    },
                    new ObserverChoice("shop-option:card", "분노", "420,320,180,180")
                    {
                        BindingKind = "shop-option",
                        BindingId = "CARD.ANGER",
                        Enabled = true,
                        SemanticHints = new[] { "scene:shop", "shop-type:card" },
                    },
                },
                extraMeta: new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["rewardScreenDetected"] = "true",
                    ["rewardScreenVisible"] = "true",
                    ["rewardForegroundOwned"] = "true",
                    ["rewardTeardownInProgress"] = "true",
                    ["rewardIsCurrentActiveScreen"] = "false",
                });
            Assert(AutoDecisionProvider.TryBuildCanonicalNonCombatSceneState(shopWithStaleRewardObserver, null, Array.Empty<GuiSmokeHistoryEntry>()) is ShopSceneState
                {
                    CanonicalForegroundOwner: NonCombatCanonicalForegroundOwner.Shop,
                    ReleaseStage: NonCombatReleaseStage.Active,
                    HandoffTarget: NonCombatHandoffTarget.HandleShop,
                },
                "Active shop authority should outrank stale reward leftovers in the canonical non-combat scene builder.");
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.HandleRewards,
                    shopWithStaleRewardObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    new ArtifactRecorder(Path.Combine(Path.GetTempPath(), $"gui-smoke-handle-reward-stale-shop-{Guid.NewGuid():N}")),
                    12,
                    true,
                    out var rewardStaleShopPhase)
                && rewardStaleShopPhase == GuiSmokePhase.HandleShop,
                "HandleRewards should recover to HandleShop even when stale reward metadata remains under an active shop foreground.");

            var shopProceedAfterPurchaseActions = BuildAllowedActions(
                GuiSmokePhase.HandleShop,
                CreateShopObserver(
                    inventoryOpen: false,
                    merchantButtonVisible: true,
                    merchantButtonEnabled: true,
                    proceedEnabled: true,
                    choices: new[]
                    {
                        new ObserverChoice("shop-open-inventory", "Merchant", "420,320,240,180")
                        {
                            BindingKind = "shop-room",
                            BindingId = "merchant-button",
                            Enabled = true,
                        },
                        new ObserverChoice("shop-proceed", "Proceed", "980,820,240,120")
                        {
                            BindingKind = "shop-room",
                            BindingId = "proceed",
                            Enabled = true,
                        },
                    }),
                Array.Empty<CombatCardKnowledgeHint>(),
                null,
                new[]
                {
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleShop.ToString(), "click", "shop buy card", DateTimeOffset.UtcNow.AddSeconds(-2))
                    {
                        Metadata = "bought one card",
                    },
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleShop.ToString(), "click", "shop back", DateTimeOffset.UtcNow.AddSeconds(-1))
                    {
                        Metadata = "closed inventory",
                    },
                });
            Assert(shopProceedAfterPurchaseActions.Contains("click shop proceed", StringComparer.OrdinalIgnoreCase), "HandleShop allowlist should prioritize explicit proceed after a bounded purchase closes the inventory.");
            Assert(string.Equals(
                    AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                        "run",
                        "boot-to-long-run",
                        12,
                        GuiSmokePhase.HandleShop.ToString(),
                        "Prefer shop proceed after a bounded purchase.",
                        DateTimeOffset.UtcNow,
                        "shop.png",
                        new WindowBounds(0, 0, 1280, 720),
                        "phase:handleshop|screen:shop",
                        "0001",
                        1,
                        3,
                        true,
                        "tactical",
                        null,
                        CreateShopObserver(
                            inventoryOpen: false,
                            merchantButtonVisible: true,
                            merchantButtonEnabled: true,
                            proceedEnabled: true,
                            choices: new[]
                            {
                                new ObserverChoice("shop-open-inventory", "Merchant", "420,320,240,180")
                                {
                                    BindingKind = "shop-room",
                                    BindingId = "merchant-button",
                                    Enabled = true,
                                },
                                new ObserverChoice("shop-proceed", "Proceed", "980,820,240,120")
                                {
                                    BindingKind = "shop-room",
                                    BindingId = "proceed",
                                    Enabled = true,
                                },
                            }).Summary,
                        Array.Empty<KnownRecipeHint>(),
                        Array.Empty<EventKnowledgeCandidate>(),
                        Array.Empty<CombatCardKnowledgeHint>(),
                        shopProceedAfterPurchaseActions,
                        new[]
                        {
                            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleShop.ToString(), "click", "shop buy card", DateTimeOffset.UtcNow.AddSeconds(-2))
                            {
                                Metadata = "bought one card",
                            },
                            new GuiSmokeHistoryEntry(GuiSmokePhase.HandleShop.ToString(), "click", "shop back", DateTimeOffset.UtcNow.AddSeconds(-1))
                            {
                                Metadata = "closed inventory",
                            },
                        },
                        "Prefer explicit proceed after a bounded shop purchase.",
                        null)).TargetLabel,
                    "shop proceed",
                    StringComparison.OrdinalIgnoreCase), "HandleShop should click proceed before reopening inventory once a bounded purchase already happened.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitPostMapNodeRoom,
                    new ObserverState(
                        new ObserverSummary(
                            "event",
                            "event",
                            false,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "event",
                            "stable",
                            null,
                            "None",
                            "event",
                            80,
                            80,
                            null,
                            new[] { "선택지" },
                            Array.Empty<string>(),
                            new[] { new ObserverActionNode("event-option:0", "event-option", "선택지", "500,700,800,100", true) },
                            new[] { new ObserverChoice("choice", "선택지", "500,700,800,100") },
                            Array.Empty<ObservedCombatHandCard>()),
                        null,
                        null,
                        null),
                    new List<GuiSmokeHistoryEntry>(),
                    waitPostMapNodeLogger,
                    13,
                    true,
                    out var waitPostMapNodeEventPhase)
                && waitPostMapNodeEventPhase == GuiSmokePhase.HandleEvent,
                "WaitPostMapNodeRoom should reopen event handling when the destination room is an event.");

            Assert(
                !GuiSmokeObserverPhaseHeuristics.TryGetPostMapNodePhase(
                    new ObserverState(
                        new ObserverSummary(
                            "event",
                            "event",
                            false,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "event",
                            "stable",
                            null,
                            "None",
                            "event",
                            80,
                            80,
                            null,
                            Array.Empty<string>(),
                            Array.Empty<string>(),
                            Array.Empty<ObserverActionNode>(),
                            Array.Empty<ObserverChoice>(),
                            Array.Empty<ObservedCombatHandCard>())
                        {
                            PublishedCurrentScreen = "event",
                            PublishedVisibleScreen = "event",
                            PublishedSceneReady = true,
                            PublishedSceneAuthority = "hook",
                            PublishedSceneStability = "stable",
                            Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom",
                                ["foregroundOwner"] = "map",
                                ["foregroundActionLane"] = "none",
                            },
                        },
                        null,
                        null,
                        null),
                    out _),
                "WaitPostMapNodeRoom should not reopen event handling from screen-only event residue without an explicit event room-entry surface.");

            var postMapEventReplayPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "artifacts",
                "gui-smoke",
                "rest-site-release-pending-20260401-live2",
                "attempts",
                "0002",
                "steps",
                "0152.request.json");
            if (File.Exists(postMapEventReplayPath))
            {
                var rebuiltReplayRequest = LoadReplayRequest(postMapEventReplayPath, fullRequestRebuild: true).Request;
                Assert(
                    rebuiltReplayRequest.AllowedActions.Contains("click event choice", StringComparer.OrdinalIgnoreCase)
                    && !rebuiltReplayRequest.AllowedActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase),
                    $"Explicit event room-entry replay rebuild should open event actions instead of a wait-only allowlist. Actual allowlist=[{string.Join(", ", rebuiltReplayRequest.AllowedActions)}].");

                var rebuiltReplayArtifact = EvaluateAutoDecisionWithDiagnostics(postMapEventReplayPath, rebuiltReplayRequest).CandidateDump;
                Assert(
                    string.Equals(rebuiltReplayArtifact.FinalDecision.Status, "act", StringComparison.OrdinalIgnoreCase)
                    && (rebuiltReplayArtifact.FinalDecision.TargetLabel?.Contains("event option", StringComparison.OrdinalIgnoreCase) ?? false),
                    $"Explicit event room-entry replay rebuild should still choose an event option. Actual={rebuiltReplayArtifact.FinalDecision.Status}/{rebuiltReplayArtifact.FinalDecision.TargetLabel ?? rebuiltReplayArtifact.FinalDecision.ActionKind ?? "null"}.");
            }

            var postMapShopReplayPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "artifacts",
                "gui-smoke",
                "combat-release-room-entry-20260401-live1",
                "attempts",
                "0002",
                "steps",
                "0106.request.json");
            if (File.Exists(postMapShopReplayPath))
            {
                var rebuiltReplayRequest = LoadReplayRequest(postMapShopReplayPath, fullRequestRebuild: true).Request;
                var rebuiltReplayObserver = new ObserverState(rebuiltReplayRequest.Observer, null, null, null);
                var nextRoomEntryState = AutoDecisionProvider.BuildNextRoomEntryState(
                    rebuiltReplayObserver,
                    rebuiltReplayRequest.WindowBounds,
                    rebuiltReplayRequest.History,
                    rebuiltReplayRequest.ScreenshotPath);
                Assert(
                    nextRoomEntryState is
                    {
                        Owner: NonCombatCanonicalForegroundOwner.Shop,
                        HandoffTarget: NonCombatHandoffTarget.HandleShop,
                        ExplicitSurfacePresent: true,
                        MapTransitPending: false,
                    },
                    $"Post-map mixed replay should promote explicit shop entry as the next-room winner. Actual owner={nextRoomEntryState.Owner} target={nextRoomEntryState.HandoffTarget} explicit={nextRoomEntryState.ExplicitSurfacePresent} pending={nextRoomEntryState.MapTransitPending}.");

                var releaseState = AutoDecisionProvider.BuildCombatReleaseState(
                    rebuiltReplayObserver,
                    rebuiltReplayRequest.WindowBounds,
                    rebuiltReplayRequest.History,
                    rebuiltReplayRequest.ScreenshotPath);
                Assert(
                    releaseState is
                    {
                        LifecycleStage: CombatLifecycleStage.ReleasedToNonCombat,
                        ForegroundOwner: NonCombatCanonicalForegroundOwner.Shop,
                        ReleaseTarget: NonCombatHandoffTarget.HandleShop,
                        HasExplicitForegroundSurface: true,
                        ReleaseMismatch: false,
                    },
                    $"Combat release should hand off to shop when explicit shop entry wins over reward/combat residue. Actual lifecycle={releaseState.LifecycleStage} owner={releaseState.ForegroundOwner} target={releaseState.ReleaseTarget} explicit={releaseState.HasExplicitForegroundSurface} mismatch={releaseState.ReleaseMismatch}.");

                var rebuiltReplayArtifact = EvaluateAutoDecisionWithDiagnostics(postMapShopReplayPath, rebuiltReplayRequest).CandidateDump;
                Assert(
                    string.Equals(rebuiltReplayArtifact.FinalDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(rebuiltReplayArtifact.FinalDecision.Reason, "waiting for combat release after EnemyClick residue into shop foreground handoff", StringComparison.OrdinalIgnoreCase),
                    $"Explicit shop room-entry replay rebuild should keep HandleCombat in canonical handoff wait instead of aborting to stale reward/combat residue. Actual={rebuiltReplayArtifact.FinalDecision.Status}/{rebuiltReplayArtifact.FinalDecision.Reason ?? rebuiltReplayArtifact.FinalDecision.TargetLabel ?? rebuiltReplayArtifact.FinalDecision.ActionKind ?? "null"}.");
            }
        }
        finally
        {
            try
            {
                Directory.Delete(waitPostMapNodeBranchRoot, true);
            }
            catch
            {
            }
        }
    }
}
