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

internal static partial class Program
{
    private static void RunPhaseRoutingRunLoadRecoverySelfTests()
    {
        var postEnterRunTreasureObserver = new ObserverState(
            new ObserverSummary(
                "map",
                "map",
                false,
                DateTimeOffset.UtcNow,
                null,
                true,
                "mixed",
                "stable",
                null,
                "Treasure",
                "generic",
                76,
                80,
                null,
                new[] { "Chest", "진행", "Chest" },
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                new[]
                {
                    new ObserverChoice("treasure-chest", "Chest", "820,360,280,240", "Chest", "Treasure chest")
                    {
                        BindingKind = "treasure-room",
                        BindingId = "chest",
                        Enabled = true,
                        SemanticHints = new[] { "treasure-room", "treasure-chest" },
                    },
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                PublishedCurrentScreen = "map",
                PublishedVisibleScreen = "map",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stable",
                Meta = new Dictionary<string, string?>
                {
                    ["treasureRoomDetected"] = "true",
                    ["treasureChestClickable"] = "true",
                    ["treasureChestOpened"] = "false",
                    ["treasureEnabledRelicHolderCount"] = "0",
                    ["treasureProceedEnabled"] = "false",
                    ["mapScreenOpen"] = "false",
                },
            },
            null,
            null,
            null);
        Assert(
            GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(postEnterRunTreasureObserver, out var postEnterRunTreasurePhase)
            && postEnterRunTreasurePhase == GuiSmokePhase.ChooseFirstNode,
            "Run-load reconciliation should resolve to ChooseFirstNode when Continue resumes directly into a treasure room.");

        var waitRunLoadTransitionObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                null,
                false,
                "transition",
                "transition",
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
                PublishedCurrentScreen = "main-menu",
                PublishedVisibleScreen = "main-menu",
                PublishedSceneReady = false,
                PublishedSceneAuthority = "transition",
                PublishedSceneStability = "transition",
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["transitionInProgress"] = "true",
                    ["rootSceneIsMainMenu"] = "true",
                    ["rootSceneIsRun"] = "false",
                    ["currentRunNodePresent"] = "false",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                },
            },
            null,
            null,
            null);
        Assert(
            !GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(waitRunLoadTransitionObserver, out _),
            "Run-load reconciliation should not branch while explicit transition truth is still active.");
        Assert(
            RootSceneTransitionObserverSignals.ShouldTreatCaptureAsTransitionWait(GuiSmokePhase.WaitRunLoad, waitRunLoadTransitionObserver.Summary),
            "Explicit run-load transition truth should suppress generic black-frame recovery nudges.");
        Assert(
            BuildAllowedActions(GuiSmokePhase.WaitRunLoad, waitRunLoadTransitionObserver, Array.Empty<CombatCardKnowledgeHint>(), null, Array.Empty<GuiSmokeHistoryEntry>()).SequenceEqual(new[] { "wait" }),
            "WaitRunLoad should remain wait-only while explicit transition truth is still active.");

        var waitRunLoadNotReadyRewardObserver = new ObserverState(
            new ObserverSummary(
                "rewards",
                "map",
                false,
                DateTimeOffset.UtcNow,
                null,
                false,
                "mixed",
                "stabilizing",
                null,
                null,
                "reward",
                59,
                80,
                0,
                new[] { "{gold} 골드", "덱에 추가할 카드를 선택하세요.", "넘기기", "{gold} 골드" },
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                new[]
                {
                    new ObserverChoice("gold", "GoldReward", "460,250,240,340"),
                    new ObserverChoice("card", "CardReward", "740,250,240,340"),
                    new ObserverChoice("choice", "넘기기", "860,900,220,80")
                    {
                        Enabled = true,
                    },
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                PublishedCurrentScreen = "rewards",
                PublishedVisibleScreen = "map",
                PublishedSceneReady = false,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stabilizing",
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["transitionInProgress"] = "false",
                    ["rootSceneIsMainMenu"] = "false",
                    ["rootSceneIsRun"] = "true",
                    ["currentRunNodePresent"] = "true",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.NRun",
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                    ["rewardScreenDetected"] = "true",
                    ["rewardScreenVisible"] = "true",
                    ["rewardForegroundOwned"] = "true",
                    ["rewardTeardownInProgress"] = "false",
                    ["rewardIsCurrentActiveScreen"] = "true",
                    ["rewardIsTopOverlay"] = "true",
                    ["rewardProceedVisible"] = "true",
                    ["rewardProceedEnabled"] = "true",
                    ["rewardVisibleButtonCount"] = "2",
                    ["rewardEnabledButtonCount"] = "2",
                    ["mapCurrentActiveScreen"] = "false",
                    ["choiceExtractorPath"] = "reward",
                },
            },
            null,
            null,
            null);
        Assert(
            GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(waitRunLoadNotReadyRewardObserver, out var waitRunLoadNotReadyRewardPhase)
            && waitRunLoadNotReadyRewardPhase == GuiSmokePhase.HandleRewards,
            "WaitRunLoad should hand off to rewards when non-ready resumed run authority is already explicitly reward-owned.");

        var waitRunLoadNotReadyMapObserver = new ObserverState(
            new ObserverSummary(
                "map",
                "map",
                false,
                DateTimeOffset.UtcNow,
                null,
                false,
                "mixed",
                "stabilizing",
                null,
                null,
                "generic",
                59,
                80,
                0,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                Array.Empty<ObserverChoice>(),
                Array.Empty<ObservedCombatHandCard>())
            {
                PublishedCurrentScreen = "map",
                PublishedVisibleScreen = "map",
                PublishedSceneReady = false,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stabilizing",
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["transitionInProgress"] = "false",
                    ["rootSceneIsMainMenu"] = "false",
                    ["rootSceneIsRun"] = "true",
                    ["currentRunNodePresent"] = "true",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.NRun",
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    ["mapCurrentActiveScreen"] = "true",
                },
            },
            null,
            null,
            null);
        Assert(
            !GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(waitRunLoadNotReadyMapObserver, out _),
            "WaitRunLoad should stay blocked while scene is not ready when only map authority is present.");

        var waitRunLoadStuckContinueObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                null,
                true,
                "mixed",
                "stable",
                "main-menu",
                null,
                null,
                24,
                24,
                null,
                new[] { "\uACC4\uC18D", "\uBA40\uD2F0\uD50C\uB808\uC774", "\uC885\uB8CC" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("main-menu:continue", "continue-run", "\uACC4\uC18D", "676,659,200,50", true)
                    {
                        TypeName = "continue-run",
                        SemanticHints = new[] { "scene:main-menu", "kind:continue-run", "value:main-menu:continue" },
                    },
                },
                new[]
                {
                    new ObserverChoice("continue-run", "\uACC4\uC18D", "676,659,200,50", "main-menu:continue", "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenuContinueButton")
                    {
                        NodeId = "main-menu:continue",
                        BindingKind = "continue-run",
                        Enabled = true,
                    },
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
                    ["transitionInProgress"] = "false",
                    ["rootSceneIsMainMenu"] = "true",
                    ["rootSceneIsRun"] = "false",
                    ["currentRunNodePresent"] = "false",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    ["terminalRunBoundary"] = "true",
                    ["mainMenuReturnDetected"] = "true",
                    ["choiceExtractorPath"] = "main-menu",
                },
            },
            null,
            null,
            null);
        Assert(
            WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(waitRunLoadStuckContinueObserver.Summary),
            "Stable main-menu continue authority without transition evidence should reopen EnterRun from WaitRunLoad.");
        Assert(
            !RootSceneTransitionObserverSignals.ShouldTreatCaptureAsTransitionWait(GuiSmokePhase.WaitRunLoad, waitRunLoadStuckContinueObserver.Summary),
            "Stable main-menu continue authority should not be treated as an in-flight run-load transition.");
        var waitRunLoadRetryActions = BuildAllowedActions(
            GuiSmokePhase.WaitRunLoad,
            waitRunLoadStuckContinueObserver,
            Array.Empty<CombatCardKnowledgeHint>(),
            null,
            Array.Empty<GuiSmokeHistoryEntry>());
        Assert(
            waitRunLoadRetryActions.Contains("click continue", StringComparer.OrdinalIgnoreCase)
            && waitRunLoadRetryActions.Contains("wait", StringComparer.OrdinalIgnoreCase),
            "WaitRunLoad should reopen EnterRun actions when Continue remains visible on a stable main menu.");
        var waitRunLoadRetryDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            6,
            GuiSmokePhase.WaitRunLoad.ToString(),
            "Wait for run-load readiness.",
            DateTimeOffset.UtcNow,
            "screen.png",
            new WindowBounds(0, 0, 1280, 720),
            "phase:wait-run-load|screen:main-menu|visible:main-menu|ready:true|stability:stable",
            "0001",
            1,
            3,
            true,
            "tactical",
            null,
            waitRunLoadStuckContinueObserver.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            waitRunLoadRetryActions,
            Array.Empty<GuiSmokeHistoryEntry>(),
            string.Empty,
            null));
        Assert(
            string.Equals(waitRunLoadRetryDecision.ActionKind, "click", StringComparison.OrdinalIgnoreCase)
            && string.Equals(waitRunLoadRetryDecision.TargetLabel, "continue", StringComparison.OrdinalIgnoreCase),
            "WaitRunLoad should retry Continue instead of returning a passive wait decision when the stable main-menu continue surface persists.");

        var waitRunLoadContinuePendingResidueObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                null,
                false,
                "mixed",
                "transition",
                null,
                null,
                null,
                24,
                24,
                null,
                new[] { "멀티플레이", "종료", "설정", "백과사전", "연대표", "전투 포기" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("main-menu:multiplayer", "menu-action", "멀티플레이", "676,734,200,50", true),
                    new ObserverActionNode("main-menu:전투-포기", "menu-action", "전투 포기", "676,984,200,50", true),
                },
                new[]
                {
                    new ObserverChoice("menu-action", "멀티플레이", "676,734,200,50", "main-menu:multiplayer", "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenuTextButton")
                    {
                        NodeId = "main-menu:multiplayer",
                        Enabled = true,
                    },
                    new ObserverChoice("menu-action", "전투 포기", "676,984,200,50", "main-menu:전투-포기", "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenuTextButton")
                    {
                        NodeId = "main-menu:전투-포기",
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
                    ["transitionInProgress"] = "false",
                    ["rootSceneIsMainMenu"] = "true",
                    ["rootSceneIsRun"] = "false",
                    ["currentRunNodePresent"] = "false",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    ["terminalRunBoundary"] = "true",
                    ["mainMenuReturnDetected"] = "true",
                    ["choiceExtractorPath"] = "main-menu",
                },
            },
            null,
            null,
            null);
        var continuePendingHistory = new[]
        {
            new GuiSmokeHistoryEntry(GuiSmokePhase.EnterRun.ToString(), "click", "continue", DateTimeOffset.UtcNow.AddSeconds(-1)),
        };
        Assert(
            !WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(waitRunLoadContinuePendingResidueObserver.Summary, continuePendingHistory),
            "WaitRunLoad should not reopen EnterRun while a recently submitted Continue action is still pending and only main-menu residue remains.");
        var waitRunLoadContinuePendingActions = BuildAllowedActions(
            GuiSmokePhase.WaitRunLoad,
            waitRunLoadContinuePendingResidueObserver,
            Array.Empty<CombatCardKnowledgeHint>(),
            null,
            continuePendingHistory);
        Assert(
            waitRunLoadContinuePendingActions.SequenceEqual(new[] { "wait" }),
            "WaitRunLoad should stay wait-only while the submitted Continue action is still pending.");
        var waitRunLoadContinuePendingDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
            "run",
            "boot-to-long-run",
            7,
            GuiSmokePhase.WaitRunLoad.ToString(),
            "Wait for run-load readiness.",
            DateTimeOffset.UtcNow,
            "screen.png",
            new WindowBounds(0, 0, 1280, 720),
            "phase:wait-run-load|screen:main-menu|visible:main-menu|ready:unknown|stability:unknown|terminal-run-boundary",
            "0001",
            1,
            3,
            false,
            "tactical",
            null,
            waitRunLoadContinuePendingResidueObserver.Summary,
            Array.Empty<KnownRecipeHint>(),
            Array.Empty<EventKnowledgeCandidate>(),
            Array.Empty<CombatCardKnowledgeHint>(),
            waitRunLoadContinuePendingActions,
            continuePendingHistory,
            string.Empty,
            null));
        Assert(
            string.Equals(waitRunLoadContinuePendingDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
            && waitRunLoadContinuePendingDecision.Reason?.Contains("submitted Continue action", StringComparison.OrdinalIgnoreCase) == true,
            "WaitRunLoad should explain that the submitted Continue action is still resolving instead of reopening EnterRun from residue.");

        var continueFailedRetryHistory = new List<GuiSmokeHistoryEntry>
        {
            new(GuiSmokePhase.EnterRun.ToString(), "click", "continue", DateTimeOffset.UtcNow.AddSeconds(-3)),
        };
        continueFailedRetryHistory.AddRange(Enumerable.Range(0, 10)
            .Select(waitIndex => new GuiSmokeHistoryEntry(
                GuiSmokePhase.WaitRunLoad.ToString(),
                "wait",
                null,
                DateTimeOffset.UtcNow.AddMilliseconds(-2000 + (waitIndex * 100)))));
        var continueFailedRetryState = WaitRunLoadRecoverySignals.GetState(waitRunLoadStuckContinueObserver.Summary, continueFailedRetryHistory);
        Assert(
            continueFailedRetryState.ShouldRetryEnterRun
            && string.Equals(continueFailedRetryState.RetryReason, "FreshRetryAfterContinuePending", StringComparison.OrdinalIgnoreCase),
            "WaitRunLoad should escalate a submitted Continue action that fails back to the main menu into the bounded retry-after-pending contract, not a generic fresh retry.");
        var waitRunLoadContinueRetryActions = BuildAllowedActions(
            GuiSmokePhase.WaitRunLoad,
            waitRunLoadStuckContinueObserver,
            Array.Empty<CombatCardKnowledgeHint>(),
            null,
            continueFailedRetryHistory);
        Assert(
            waitRunLoadContinueRetryActions.Contains("click continue", StringComparer.OrdinalIgnoreCase),
            "WaitRunLoad should expose Continue again once the submitted Continue action has clearly failed back to a fresh retry surface.");

        var waitRunLoadTerminalMainMenuObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                null,
                true,
                "mixed",
                "stable",
                "main-menu",
                null,
                null,
                24,
                24,
                null,
                new[] { "\uBA40\uD2F0\uD50C\uB808\uC774", "\uC885\uB8CC" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("main-menu:multiplayer", "menu-action", "\uBA40\uD2F0\uD50C\uB808\uC774", "676,759,200,50", true),
                },
                new[]
                {
                    new ObserverChoice("menu-action", "\uBA40\uD2F0\uD50C\uB808\uC774", "676,759,200,50", "main-menu:multiplayer", "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenuTextButton")
                    {
                        NodeId = "main-menu:multiplayer",
                        Enabled = true,
                    },
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
                    ["transitionInProgress"] = "false",
                    ["rootSceneIsMainMenu"] = "true",
                    ["rootSceneIsRun"] = "false",
                    ["currentRunNodePresent"] = "false",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    ["terminalRunBoundary"] = "true",
                    ["mainMenuReturnDetected"] = "true",
                    ["choiceExtractorPath"] = "main-menu",
                },
            },
            null,
            null,
            null);
        Assert(
            !WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(waitRunLoadTerminalMainMenuObserver.Summary),
            "Terminal main-menu returns without Continue should not be retried as run-load recovery.");

        var waitRunLoadFreshMainMenuObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                null,
                true,
                "mixed",
                "stable",
                "main-menu",
                null,
                null,
                24,
                24,
                null,
                new[] { "\uC2F1\uAE00\uD50C\uB808\uC774", "\uBA40\uD2F0\uD50C\uB808\uC774", "\uC124\uC815", "\uC885\uB8CC" },
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                new[]
                {
                    new ObserverChoice("menu-action", "\uC2F1\uAE00\uD50C\uB808\uC774", "676,684,200,50", "main-menu:singleplayer", "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenuTextButton")
                    {
                        NodeId = "main-menu:singleplayer",
                        Enabled = true,
                    },
                    new ObserverChoice("menu-action", "\uBA40\uD2F0\uD50C\uB808\uC774", "676,734,200,50", "main-menu:multiplayer", "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenuTextButton")
                    {
                        NodeId = "main-menu:multiplayer",
                        Enabled = true,
                    },
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                PublishedCurrentScreen = "main-menu",
                PublishedVisibleScreen = "bootstrap",
                PublishedSceneReady = null,
                PublishedSceneAuthority = null,
                PublishedSceneStability = null,
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["transitionInProgress"] = "false",
                    ["rootSceneIsMainMenu"] = "true",
                    ["rootSceneIsRun"] = "false",
                    ["currentRunNodePresent"] = "false",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    ["terminalRunBoundary"] = "true",
                    ["mainMenuReturnDetected"] = "true",
                    ["choiceExtractorPath"] = "main-menu",
                    ["publishedVisibleScreen"] = "bootstrap",
                    ["compatLogicalScreen"] = "bootstrap",
                    ["compatVisibleScreen"] = "bootstrap",
                },
            },
            null,
            null,
            null);
        Assert(
            WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(waitRunLoadFreshMainMenuObserver.Summary),
            "Fresh main-menu singleplayer authority should reopen EnterRun from WaitRunLoad even when stale bootstrap visibility lingers in published meta.");
        var waitRunLoadFreshActions = BuildAllowedActions(
            GuiSmokePhase.WaitRunLoad,
            waitRunLoadFreshMainMenuObserver,
            Array.Empty<CombatCardKnowledgeHint>(),
            null,
            Array.Empty<GuiSmokeHistoryEntry>());
        Assert(
            waitRunLoadFreshActions.Contains("click singleplayer", StringComparer.OrdinalIgnoreCase)
            && waitRunLoadFreshActions.Contains("wait", StringComparer.OrdinalIgnoreCase),
            "WaitRunLoad should reopen the singleplayer run-start lane when explicit NMainMenu choices outrank stale bootstrap visibility.");

        var waitRunLoadRunSaveCleanupObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                null,
                true,
                "mixed",
                "stable",
                "main-menu",
                null,
                null,
                24,
                24,
                null,
                new[] { "\uC804\uD22C \uD3EC\uAE30" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("main-menu:\uC804\uD22C-\uD3EC\uAE30", "menu-action", "\uC804\uD22C \uD3EC\uAE30", "676,709,200,50", true),
                },
                new[]
                {
                    new ObserverChoice("menu-action", "\uC804\uD22C \uD3EC\uAE30", "676,709,200,50", "main-menu:\uC804\uD22C-\uD3EC\uAE30", "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenuTextButton")
                    {
                        NodeId = "main-menu:\uC804\uD22C-\uD3EC\uAE30",
                        Enabled = true,
                    },
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
                    ["transitionInProgress"] = "false",
                    ["rootSceneIsMainMenu"] = "true",
                    ["rootSceneIsRun"] = "false",
                    ["currentRunNodePresent"] = "false",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    ["terminalRunBoundary"] = "true",
                    ["mainMenuReturnDetected"] = "true",
                    ["choiceExtractorPath"] = "main-menu",
                },
            },
            null,
            null,
            null);
        Assert(
            WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(waitRunLoadRunSaveCleanupObserver.Summary),
            "Stable main-menu run-save cleanup surfaces should reopen EnterRun from WaitRunLoad instead of timing out.");
        var waitRunLoadRunSaveActions = BuildAllowedActions(
            GuiSmokePhase.WaitRunLoad,
            waitRunLoadRunSaveCleanupObserver,
            Array.Empty<CombatCardKnowledgeHint>(),
            null,
            Array.Empty<GuiSmokeHistoryEntry>());
        Assert(
            waitRunLoadRunSaveActions.Contains("click abandon run", StringComparer.OrdinalIgnoreCase)
            && waitRunLoadRunSaveActions.Contains("wait", StringComparer.OrdinalIgnoreCase),
            "WaitRunLoad recovery should expose Abandon Run when a stale run-save surface remains on the main menu.");

        var waitRunLoadAbandonConfirmObserver = new ObserverState(
            new ObserverSummary(
                "main-menu",
                "main-menu",
                false,
                DateTimeOffset.UtcNow,
                null,
                true,
                "mixed",
                "transient",
                "main-menu-abandon-confirm",
                null,
                null,
                36,
                36,
                null,
                new[] { "\uC544\uB2C8\uC694", "\uC608" },
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
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
                    ["transitionInProgress"] = "false",
                    ["rootSceneIsMainMenu"] = "true",
                    ["rootSceneIsRun"] = "false",
                    ["currentRunNodePresent"] = "false",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu",
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.CommonUi.NAbandonRunConfirmPopup",
                    ["terminalRunBoundary"] = "true",
                    ["mainMenuReturnDetected"] = "true",
                    ["choiceExtractorPath"] = "main-menu-abandon-confirm",
                    ["compatSceneReady"] = "false",
                    ["compatSceneStability"] = "transient",
                },
            },
            null,
            null,
            null);
        Assert(
            WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(waitRunLoadAbandonConfirmObserver.Summary),
            "Explicit abandon-run confirm popup surfaces should reopen EnterRun from WaitRunLoad even before generic ready/stable heuristics settle.");
        var waitRunLoadAbandonConfirmActions = BuildAllowedActions(
            GuiSmokePhase.WaitRunLoad,
            waitRunLoadAbandonConfirmObserver,
            Array.Empty<CombatCardKnowledgeHint>(),
            null,
            Array.Empty<GuiSmokeHistoryEntry>());
        Assert(
            waitRunLoadAbandonConfirmActions.Contains("click confirm abandon run", StringComparer.OrdinalIgnoreCase)
            && waitRunLoadAbandonConfirmActions.Contains("click cancel abandon run", StringComparer.OrdinalIgnoreCase)
            && waitRunLoadAbandonConfirmActions.Contains("wait", StringComparer.OrdinalIgnoreCase),
            "WaitRunLoad recovery should expose confirm/cancel while the abandon-run popup owns the foreground.");

        var waitCharacterSelectBranchRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-wait-character-select-branch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(waitCharacterSelectBranchRoot);
        try
        {
            var waitCharacterSelectLogger = new ArtifactRecorder(waitCharacterSelectBranchRoot);
            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitRunLoad,
                    waitRunLoadStuckContinueObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    4,
                    true,
                    out var waitRunLoadRetryPhase)
                && waitRunLoadRetryPhase == GuiSmokePhase.EnterRun,
                "WaitRunLoad should bounce back to EnterRun when Continue remains visible on a stable main-menu surface without transition evidence.");

            Assert(
                !TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitRunLoad,
                    waitRunLoadTerminalMainMenuObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    5,
                    true,
                    out _),
                "WaitRunLoad should not misclassify terminal main-menu returns without Continue as run-load retries.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitRunLoad,
                    waitRunLoadAbandonConfirmObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    6,
                    true,
                    out var waitRunLoadAbandonConfirmPhase)
                && waitRunLoadAbandonConfirmPhase == GuiSmokePhase.EnterRun,
                "WaitRunLoad should bounce back to EnterRun when the abandon-run confirm popup is explicitly visible on the main menu.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitRunLoad,
                    postEnterRunTreasureObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    6,
                    true,
                    out var waitRunLoadTreasurePhase)
                && waitRunLoadTreasurePhase == GuiSmokePhase.ChooseFirstNode,
                "WaitRunLoad should reopen treasure handling when Continue resumes directly into a treasure room.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitRunLoad,
                    CreatePhaseRoutingRewardMixedStateObserver(),
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    7,
                    true,
                    out var waitRunLoadRewardPhase)
                && waitRunLoadRewardPhase == GuiSmokePhase.HandleRewards,
                "WaitRunLoad should branch to rewards when Continue resumes into a reward foreground.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitRunLoad,
                    new ObserverState(
                        new ObserverSummary(
                            "event",
                            "event",
                            false,
                            DateTimeOffset.UtcNow,
                            null,
                            true,
                            "mixed",
                            "stable",
                            null,
                            "None",
                            "event",
                            80,
                            80,
                            null,
                            new[] { "Option A" },
                            Array.Empty<string>(),
                            new[] { new ObserverActionNode("event-option:0", "event-option", "Option A", "460,750,1000,100", true) },
                            new[] { new ObserverChoice("choice", "Option A", "460,750,1000,100") },
                            Array.Empty<ObservedCombatHandCard>())
                        {
                            PublishedCurrentScreen = "event",
                            PublishedVisibleScreen = "event",
                            PublishedSceneReady = true,
                            PublishedSceneAuthority = "hook",
                            PublishedSceneStability = "stable",
                        },
                        null,
                        null,
                        null),
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    8,
                    true,
                    out var waitRunLoadEventPhase)
                && waitRunLoadEventPhase == GuiSmokePhase.HandleEvent,
                "WaitRunLoad should branch to event handling when Continue resumes into an event room.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitRunLoad,
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
                    waitCharacterSelectLogger,
                    9,
                    true,
                    out var waitRunLoadCombatPhase)
                && waitRunLoadCombatPhase == GuiSmokePhase.HandleCombat,
                "WaitRunLoad should branch to combat handling when Continue resumes into combat.");

            Assert(
                TryAdvanceAlternateBranch(
                    GuiSmokePhase.WaitCharacterSelect,
                    CreatePhaseRoutingCharacterSelectObserver(),
                    new List<GuiSmokeHistoryEntry>(),
                    waitCharacterSelectLogger,
                    10,
                    true,
                    out var waitCharacterSelectPhase)
                && waitCharacterSelectPhase == GuiSmokePhase.ChooseCharacter,
                "WaitCharacterSelect should only reopen the actual character-select flow.");
        }
        finally
        {
            try
            {
                Directory.Delete(waitCharacterSelectBranchRoot, true);
            }
            catch
            {
            }
        }

        Assert(
            WindowLocator.HasMeaningfulDrift(
                new WindowCaptureTarget(IntPtr.Zero, "before", new Rectangle(100, 200, 1000, 800), false, false),
                new WindowCaptureTarget(IntPtr.Zero, "after", new Rectangle(140, 200, 1000, 800), false, false)),
            "Bounds drift should be detected when the window moved.");

        Assert(
            TryParseScreenBounds("100,200,40,20", out var parsedBounds)
            && Math.Abs(parsedBounds.X - 100f) < 0.01f
            && Math.Abs(parsedBounds.Width - 40f) < 0.01f,
            "Screen bounds should parse from observer inventory strings.");
    }
}
