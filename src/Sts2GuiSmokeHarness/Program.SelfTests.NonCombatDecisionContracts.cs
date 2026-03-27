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
    private static void RunNonCombatDecisionContractsSelfTests()
    {
        var rewardRankingScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-reward-ranking-self-test-{Guid.NewGuid():N}.png");
        try
        {
            using (var bitmap = new Bitmap(1280, 720))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var panelBrush = new SolidBrush(Color.FromArgb(222, 205, 168)))
            using (var arrowBrush = new SolidBrush(Color.FromArgb(220, 60, 55)))
            {
                graphics.Clear(Color.Black);
                graphics.FillRectangle(panelBrush, new Rectangle(420, 180, 480, 320));
                graphics.FillEllipse(arrowBrush, new Rectangle(595, 450, 90, 70));
                bitmap.Save(rewardRankingScreenshotPath, ImageFormat.Png);
            }

            var rewardObserverState = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "rewards",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-rewards",
                    true,
                    "mixed",
                    "stable",
                    "episode-rewards",
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
                        new ObserverActionNode("proceed:1", "proceed", "넘기기", "1583,764,269,108", true),
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "11 골드", "758,374,402,86"),
                        new ObserverChoice("choice", "넘기기", "1583,764,269,108"),
                    },
                    Array.Empty<ObservedCombatHandCard>()),
                null,
                null,
                null);
            var rewardSceneSignature = ComputeSceneSignature(rewardRankingScreenshotPath, rewardObserverState, GuiSmokePhase.HandleRewards);
            Assert(rewardSceneSignature.Contains("reward:fast-path", StringComparison.OrdinalIgnoreCase), "Reward scenes with explicit reward ownership should use the reward fast path.");
            Assert(!rewardSceneSignature.Contains("shot:", StringComparison.OrdinalIgnoreCase), "Reward fast path scene signatures should not pay screenshot fingerprint overhead.");
            Assert(!rewardSceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase), "Reward fast path scene signatures should not compute screenshot map-arrow contamination while explicit reward authority is present.");
            Assert(!rewardSceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase), "Reward scenes should not advertise visible map advance while explicit reward affordances are still present.");
            Assert(!GetAllowedActions(GuiSmokePhase.HandleRewards, rewardObserverState).Contains("click visible map advance", StringComparer.OrdinalIgnoreCase), "Reward allowlist should not open visible map advance while explicit reward affordances are present.");
            var rewardDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                33,
                GuiSmokePhase.HandleRewards.ToString(),
                "Resolve the visible reward screen.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                rewardSceneSignature,
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                rewardObserverState.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                GetAllowedActions(GuiSmokePhase.HandleRewards, rewardObserverState),
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Reward panel is authoritative over any background map contamination.",
                null));
            Assert(string.Equals(rewardDecision.TargetLabel, "claim reward item", StringComparison.OrdinalIgnoreCase), "Explicit reward items should outrank screenshot-derived map fallback on NRewardsScreen.");
            Assert(TryClassifyRewardMapLoop(
                GuiSmokePhase.HandleRewards,
                new GuiSmokeStepRequest(
                    "run",
                    "boot-to-long-run",
                    37,
                    GuiSmokePhase.HandleRewards.ToString(),
                    "Guard against reward/map loops.",
                    DateTimeOffset.UtcNow,
                    rewardRankingScreenshotPath,
                    new WindowBounds(0, 0, 1280, 720),
                    rewardSceneSignature + "|layer:map-background",
                    "0001",
                    1,
                    3,
                    true,
                    "tactical",
                    null,
                    rewardObserverState.Summary,
                    Array.Empty<KnownRecipeHint>(),
                    Array.Empty<EventKnowledgeCandidate>(),
                    Array.Empty<CombatCardKnowledgeHint>(),
                    GetAllowedActions(GuiSmokePhase.HandleRewards, rewardObserverState),
                    new[]
                    {
                        new GuiSmokeHistoryEntry(GuiSmokePhase.HandleRewards.ToString(), "click", "visible map advance", DateTimeOffset.UtcNow.AddSeconds(-8)),
                        new GuiSmokeHistoryEntry(GuiSmokePhase.WaitMap.ToString(), "branch-reward", null, DateTimeOffset.UtcNow.AddSeconds(-6)),
                        new GuiSmokeHistoryEntry(GuiSmokePhase.HandleRewards.ToString(), "click", "visible map advance", DateTimeOffset.UtcNow.AddSeconds(-4)),
                        new GuiSmokeHistoryEntry(GuiSmokePhase.WaitMap.ToString(), "branch-reward", null, DateTimeOffset.UtcNow.AddSeconds(-2)),
                    },
                    "Abort repeated reward/map oscillation before another map fallback click.",
                    null),
                new GuiSmokeStepDecision("act", "click", null, 0.5, 0.5, "visible map advance", "looping on background map", 0.5, "rewards", 1200, true, null),
                out _), "Repeated visible-map clicks while explicit reward affordances remain should be classified as reward-map-loop.");

            var layeredRewardObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "map",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-layered-reward",
                    false,
                    "mixed",
                    "stabilizing",
                    "episode-layered-reward",
                    "None",
                    "reward",
                    80,
                    80,
                    null,
                    new[] { "11 골드", "넘기기", "LeftArrow" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("reward-item:0", "reward-item", "11 골드", "758,374,402,86", true),
                        new ObserverActionNode("proceed:1", "proceed", "넘기기", "1583,764,269,108", true),
                        new ObserverActionNode("overlay:left", "choice", "LeftArrow", "48,930,88,88", true),
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "11 골드", "758,374,402,86"),
                        new ObserverChoice("choice", "넘기기", "1583,764,269,108"),
                        new ObserverChoice("choice", "LeftArrow", "48,930,88,88"),
                    },
                    Array.Empty<ObservedCombatHandCard>()),
                null,
                null,
                null);
            var layeredRewardState = BuildRewardMapLayerStateForObserver(layeredRewardObserver.Summary, new WindowBounds(0, 0, 1280, 720));
            Assert(layeredRewardState.RewardPanelVisible, "Layered reward/map state should keep the reward panel as foreground while explicit reward bounds remain usable.");
            Assert(layeredRewardState.MapContextVisible, "Layered reward/map state should preserve background map context instead of forcing a single exclusive screen.");
            Assert(layeredRewardState.RewardBackNavigationAvailable, "Layered reward/map state should record reward back-navigation affordances.");

            var staleRewardObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "map",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-stale-reward",
                    false,
                    "mixed",
                    "stabilizing",
                    "episode-stale-reward",
                    "None",
                    "reward",
                    80,
                    80,
                    null,
                    new[] { "넘기기" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("proceed:0", "proceed", "넘기기", "1983,764,269,108", true),
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "넘기기", "1983,764,269,108"),
                    },
                    Array.Empty<ObservedCombatHandCard>()),
                null,
                null,
                null);
            var staleRewardState = BuildRewardMapLayerStateForObserver(staleRewardObserver.Summary, new WindowBounds(1, 32, 1280, 720));
            Assert(!staleRewardState.RewardPanelVisible, "Reward foreground should clear once only stale off-window reward bounds remain.");
            Assert(staleRewardState.StaleRewardChoicePresent, "Layered reward/map state should mark stale reward choices after panel authority disappears.");
            Assert(staleRewardState.OffWindowBoundsReused, "Off-window reward bounds should be flagged for reward/map loop diagnosis.");
            Assert(GetAllowedActions(GuiSmokePhase.HandleRewards, staleRewardObserver).SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase), "When reward authority disappears, HandleRewards should release ownership with wait instead of directly reopening map fallback.");
            Assert(!GetAllowedActions(GuiSmokePhase.HandleRewards, staleRewardObserver).Contains("click proceed", StringComparer.OrdinalIgnoreCase), "Stale reward bounds should be hard-rejected from the actionable set once only map context remains.");

            var rewardAftermathContradictionObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "map",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-reward-aftermath-contradiction",
                    true,
                    "mixed",
                    "stabilizing",
                    "episode-reward-aftermath-contradiction",
                    "None",
                    "reward",
                    80,
                    80,
                    null,
                    new[] { "진행" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("proceed:0", "proceed", "진행", "1583,764,269,108", true),
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "진행", "1583,764,269,108"),
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>
                    {
                        ["rewardScreenDetected"] = "true",
                        ["rewardScreenVisible"] = "true",
                        ["rewardForegroundOwned"] = "true",
                        ["rewardTeardownInProgress"] = "false",
                        ["rewardIsCurrentActiveScreen"] = "false",
                        ["rewardIsTopOverlay"] = "true",
                        ["rewardProceedVisible"] = "true",
                        ["rewardProceedEnabled"] = "false",
                        ["rewardVisibleButtonCount"] = "1",
                        ["rewardEnabledButtonCount"] = "0",
                        ["mapCurrentActiveScreen"] = "true",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    },
                },
                null,
                null,
                null);
            var rewardAftermathContradictionState = RewardObserverSignals.TryGetState(rewardAftermathContradictionObserver.Summary);
            Assert(rewardAftermathContradictionState is not null, "Contradictory reward aftermath observer should still parse as a reward screen state.");
            Assert(!rewardAftermathContradictionState.ForegroundOwned, "Map-current reward teardown should override stale top-overlay foreground ownership.");
            Assert(rewardAftermathContradictionState.TeardownInProgress, "Map-current reward teardown should be recognized even when old metadata still claims foreground ownership.");
            Assert(
                !TryReopenMixedStateModalBranchFromWaitMap(
                    rewardAftermathContradictionObserver,
                    new List<GuiSmokeHistoryEntry>(),
                    new ArtifactRecorder(Path.Combine(Path.GetTempPath(), $"gui-smoke-reward-aftermath-{Guid.NewGuid():N}")),
                    44,
                    out _),
                "WaitMap mixed-state reopening should not resurrect HandleRewards when reward is only stale top-overlay residue under current map authority.");

            var rewardAftermathObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "map",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-reward-aftermath",
                    true,
                    "mixed",
                    "stable",
                    "episode-reward-aftermath",
                    "Monster",
                    "reward",
                    80,
                    80,
                    null,
                    new[] { "넘기기" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("proceed:0", "proceed", "넘기기", "1583,764,269,108", true),
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "넘기기", "1583,764,269,108"),
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>
                    {
                        ["rewardScreenDetected"] = "true",
                        ["rewardScreenVisible"] = "true",
                        ["rewardForegroundOwned"] = "false",
                        ["rewardTeardownInProgress"] = "true",
                        ["rewardIsCurrentActiveScreen"] = "false",
                        ["mapCurrentActiveScreen"] = "true",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    },
                },
                null,
                null,
                null);
            var rewardAftermathState = BuildRewardMapLayerStateForObserver(rewardAftermathObserver.Summary, new WindowBounds(0, 0, 1280, 720));
            Assert(!rewardAftermathState.RewardForegroundOwned, "Reward proceed aftermath should drop reward foreground ownership once map becomes current.");
            Assert(rewardAftermathState.RewardTeardownInProgress, "Reward proceed aftermath should export teardown-in-progress while stale reward visuals linger.");
            var rewardAftermathRequest = new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                44,
                GuiSmokePhase.HandleRewards.ToString(),
                "Release reward ownership after proceed.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                ComputeSceneSignature(rewardRankingScreenshotPath, rewardAftermathObserver, GuiSmokePhase.HandleRewards),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                rewardAftermathObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                GetAllowedActions(GuiSmokePhase.HandleRewards, rewardAftermathObserver),
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Reward ownership should release to WaitMap once map is current.",
                null);
            var rewardAftermathDecision = AutoDecisionProvider.Decide(rewardAftermathRequest);
            Assert(string.Equals(rewardAftermathDecision.Status, "wait", StringComparison.OrdinalIgnoreCase), "HandleRewards should wait/release instead of opening map fallback directly during reward teardown aftermath.");
            Assert(GetPostRewardPhase(rewardAftermathDecision) == GuiSmokePhase.WaitMap, "Reward teardown aftermath should hand off through WaitMap, not keep HandleRewards ownership.");

            var rewardReleasePendingHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleRewards.ToString(), "click", "reward skip", DateTimeOffset.UtcNow.AddSeconds(-2)),
            };
            var rewardReleasePendingObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "map",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-reward-release-pending",
                    false,
                    "mixed",
                    "stabilizing",
                    "episode-reward-release-pending",
                    "Monster",
                    "reward",
                    80,
                    80,
                    null,
                    new[] { "넘기기" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("proceed:0", "proceed", "넘기기", "1180,764,269,108", true),
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "넘기기", "1180,764,269,108"),
                    },
                    Array.Empty<ObservedCombatHandCard>()),
                null,
                null,
                null);
            var rewardReleasePendingState = AutoDecisionProvider.BuildRewardSceneState(
                rewardReleasePendingObserver,
                new WindowBounds(0, 0, 1280, 720),
                rewardReleasePendingHistory,
                rewardRankingScreenshotPath);
            Assert(rewardReleasePendingState.ReleaseStage == RewardReleaseStage.ReleasePending, "A fresh reward skip on the same mixed reward/map authority band should enter reward release-pending.");
            Assert(rewardReleasePendingState.SuppressSameSkipReissue, "Reward release-pending should suppress same reward skip reissue until ownership changes.");
            Assert(((ICanonicalNonCombatSceneState)rewardReleasePendingState).HandoffTarget == NonCombatHandoffTarget.WaitMap,
                "Canonical reward contract should expose WaitMap while reward skip release is pending.");
            Assert(BuildAllowedActions(
                GuiSmokePhase.HandleRewards,
                rewardReleasePendingObserver,
                Array.Empty<CombatCardKnowledgeHint>(),
                rewardRankingScreenshotPath,
                rewardReleasePendingHistory).SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase),
                "Reward release-pending should collapse the HandleRewards allowlist to wait instead of reopening reward skip.");
            var rewardReleasePendingDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                45,
                GuiSmokePhase.HandleRewards.ToString(),
                "Do not reissue reward skip on the same mixed reward/map authority band.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                ComputeSceneSignature(rewardRankingScreenshotPath, rewardReleasePendingObserver, GuiSmokePhase.HandleRewards),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                rewardReleasePendingObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                BuildAllowedActions(
                    GuiSmokePhase.HandleRewards,
                    rewardReleasePendingObserver,
                    Array.Empty<CombatCardKnowledgeHint>(),
                    rewardRankingScreenshotPath,
                    rewardReleasePendingHistory),
                rewardReleasePendingHistory,
                "Reward release-pending should wait for ownership release instead of reissuing skip.",
                null));
            Assert(string.Equals(rewardReleasePendingDecision.Status, "wait", StringComparison.OrdinalIgnoreCase), "Reward release-pending should wait instead of clicking reward skip again.");

            var postShopRewardMixedObserver = new ObserverState(
                new ObserverSummary(
                    "shop",
                    "shop",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-post-shop-mixed",
                    true,
                    "mixed",
                    "stable",
                    "episode-post-shop-mixed",
                    "Shop",
                    "shop",
                    80,
                    80,
                    null,
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    Array.Empty<ObserverActionNode>(),
                    Array.Empty<ObserverChoice>(),
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>
                    {
                        ["shopRoomDetected"] = "true",
                        ["shopRoomVisible"] = "true",
                        ["shopForegroundOwned"] = "false",
                        ["shopTeardownInProgress"] = "true",
                        ["shopIsCurrentActiveScreen"] = "false",
                        ["rewardScreenDetected"] = "true",
                        ["rewardScreenVisible"] = "true",
                        ["rewardForegroundOwned"] = "false",
                        ["rewardTeardownInProgress"] = "true",
                        ["rewardIsCurrentActiveScreen"] = "false",
                        ["mapCurrentActiveScreen"] = "true",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    },
                },
                null,
                null,
                null);
            var postShopMixedBranchRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-post-shop-mixed-{Guid.NewGuid():N}");
            Directory.CreateDirectory(postShopMixedBranchRoot);
            try
            {
                var postShopMixedLogger = new ArtifactRecorder(postShopMixedBranchRoot);
                Assert(
                    TryAdvanceAlternateBranch(
                        GuiSmokePhase.HandleShop,
                        postShopRewardMixedObserver,
                        new List<GuiSmokeHistoryEntry>(),
                        postShopMixedLogger,
                        8,
                        true,
                        out var postShopMixedPhase)
                    && postShopMixedPhase == GuiSmokePhase.WaitMap,
                    "Post-shop stale reward leftovers with map current should release to WaitMap, not reopen HandleRewards.");
            }
            finally
            {
                try
                {
                    Directory.Delete(postShopMixedBranchRoot, true);
                }
                catch
                {
                }
            }

            var combatContradictionObserver = new ObserverState(
                new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    DateTimeOffset.UtcNow,
                    "inv-combat-contradiction",
                    true,
                    "combat",
                    "stable",
                    "episode-combat-contradiction",
                    "Monster",
                    "combat",
                    60,
                    80,
                    3,
                    Array.Empty<string>(),
                    new[] { "screen-changed: map -> combat", "NMapScreen.Open" },
                    Array.Empty<ObserverActionNode>(),
                    Array.Empty<ObserverChoice>(),
                    Array.Empty<ObservedCombatHandCard>()),
                null,
                null,
                null);
            var contradictoryMapFallbackMethod = typeof(AutoDecisionProvider).GetMethod("HasContradictoryForegroundOwnerAgainstMapFallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert(contradictoryMapFallbackMethod is not null, "Expected private contradictory map fallback helper.");
            Assert((bool)(contradictoryMapFallbackMethod!.Invoke(null, new object?[]
            {
                new GuiSmokeStepRequest(
                    "run",
                    "boot-to-long-run",
                    46,
                    GuiSmokePhase.ChooseFirstNode.ToString(),
                    "Combat should suppress stale map fallback.",
                    DateTimeOffset.UtcNow,
                    rewardRankingScreenshotPath,
                    new WindowBounds(0, 0, 1280, 720),
                    "phase:choosefirstnode|screen:combat|visible:combat|layer:map-background|visible:map-arrow",
                    "0001",
                    1,
                    3,
                    true,
                    "tactical",
                    null,
                    combatContradictionObserver.Summary,
                    Array.Empty<KnownRecipeHint>(),
                    Array.Empty<EventKnowledgeCandidate>(),
                    Array.Empty<CombatCardKnowledgeHint>(),
                    GetAllowedActions(GuiSmokePhase.HandleCombat, combatContradictionObserver),
                    Array.Empty<GuiSmokeHistoryEntry>(),
                    "Combat foreground should suppress stale map fallback.",
                    null),
            }) ?? false), "Combat foreground should explicitly suppress stale map fallback lanes.");

            var terminalBoundaryObserver = new ObserverState(
                new ObserverSummary(
                    "unknown",
                    "unknown",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-terminal-boundary",
                    true,
                    "mixed",
                    "stable",
                    "episode-terminal-boundary",
                    "None",
                    "generic",
                    0,
                    80,
                    null,
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    Array.Empty<ObserverActionNode>(),
                    Array.Empty<ObserverChoice>(),
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>
                    {
                        ["terminalRunBoundary"] = "true",
                        ["gameOverScreenDetected"] = "true",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen.NGameOverScreen",
                    },
                },
                null,
                null,
                null);
            Assert(RewardObserverSignals.IsTerminalRunBoundary(terminalBoundaryObserver.Summary), "Game-over or unlock-style active screens should be classified as terminal run boundaries.");
            Assert(!MapForegroundReconciliation.HasMapForegroundOwnership(terminalBoundaryObserver, Array.Empty<GuiSmokeHistoryEntry>()), "Terminal run boundaries must not be misclassified as gameplay map ownership.");


            var rewardPolicyObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "rewards",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-reward-policy",
                    true,
                    "mixed",
                    "stable",
                    "episode-reward-policy",
                    "None",
                    "reward",
                    80,
                    80,
                    null,
                    new[] { "11 골드", "넘기기", "CARD.BATTLE_TRANCE" },
                    Array.Empty<string>(),
                    Array.Empty<ObserverActionNode>(),
                    new[]
                    {
                        new ObserverChoice("card", "CARD.BATTLE_TRANCE", "628,248,180,254", "CARD.BATTLE_TRANCE", "카드 보상"),
                        new ObserverChoice("choice", "11 골드", "758,374,402,86", null, "11 골드"),
                        new ObserverChoice("choice", "넘기기", "1280,764,269,108", null, "넘기기"),
                    },
                    Array.Empty<ObservedCombatHandCard>()),
                null,
                null,
                null);
            Assert(BuildAllowedActions(GuiSmokePhase.HandleRewards, rewardPolicyObserver, Array.Empty<CombatCardKnowledgeHint>(), rewardRankingScreenshotPath, Array.Empty<GuiSmokeHistoryEntry>()).Contains("click reward card choice", StringComparer.OrdinalIgnoreCase), "Reward allowlist should keep claimable reward card actions open while a reward card remains visible.");
            var rewardPolicyDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                38,
                GuiSmokePhase.HandleRewards.ToString(),
                "Prefer meaningful reward progression over stale gold/skip bias.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                ComputeSceneSignature(rewardRankingScreenshotPath, rewardPolicyObserver, GuiSmokePhase.HandleRewards),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                rewardPolicyObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                GetAllowedActions(GuiSmokePhase.HandleRewards, rewardPolicyObserver),
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Prefer cards or relics over default gold/skip bias when a real reward remains visible.",
                null));
            Assert(!string.Equals(rewardPolicyDecision.TargetLabel, "reward skip", StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(rewardPolicyDecision.TargetLabel, "proceed after resolving rewards", StringComparison.OrdinalIgnoreCase),
                "Reward policy should prefer a visible card or claimable reward over default gold/skip choices.");

            var transformSubtypeObserver = new ObserverState(
                new ObserverSummary(
                    "transform",
                    "transform",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-transform-subtype",
                    true,
                    "mixed",
                    "stable",
                    "episode-transform-subtype",
                    "Event",
                    "card-selection-transform",
                    80,
                    80,
                    null,
                    new[] { "타격", "수비" },
                    Array.Empty<string>(),
                    Array.Empty<ObserverActionNode>(),
                    new[]
                    {
                        new ObserverChoice("transform-card", "타격", "520,280,180,254", "CARD.STRIKE_IRONCLAD")
                        {
                            SemanticHints = new[] { "card-selection:transform", "selected-card" },
                        },
                        new ObserverChoice("transform-card", "수비", "860,280,180,254", "CARD.DEFEND_IRONCLAD")
                        {
                            SemanticHints = new[] { "card-selection:transform" },
                        },
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["cardSelectionScreenType"] = "transform",
                        ["cardSelectionMinSelect"] = "2",
                        ["cardSelectionMaxSelect"] = "2",
                        ["cardSelectionSelectedCount"] = "1",
                        ["cardSelectionPreviewVisible"] = "false",
                        ["cardSelectionMainConfirmEnabled"] = "false",
                        ["cardSelectionPreviewConfirmEnabled"] = "false",
                        ["cardSelectionSelectedCardIds"] = "CARD.STRIKE_IRONCLAD",
                    },
                },
                null,
                null,
                null);
            var transformSubtypeActions = BuildAllowedActions(GuiSmokePhase.HandleEvent, transformSubtypeObserver, Array.Empty<CombatCardKnowledgeHint>(), string.Empty, Array.Empty<GuiSmokeHistoryEntry>());
            Assert(transformSubtypeActions.Contains("transform select card", StringComparer.OrdinalIgnoreCase), "Transform subtype should open an explicit transform select lane.");
            Assert(!transformSubtypeActions.Contains("click reward card choice", StringComparer.OrdinalIgnoreCase), "Transform subtype should suppress generic reward card actions.");
            var transformConfirmObserver = new ObserverState(
                transformSubtypeObserver.Summary with
                {
                    InventoryId = "inv-transform-confirm",
                    SceneEpisodeId = "episode-transform-confirm",
                    Choices = new[]
                    {
                        new ObserverChoice("transform-confirm", "Confirm", "1700,720,180,110", "preview-confirm")
                        {
                            BindingKind = "card-selection-confirm",
                            BindingId = "preview",
                            SemanticHints = new[] { "card-selection:transform", "confirm-mode:preview" },
                            Enabled = true,
                        },
                    },
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["cardSelectionScreenType"] = "transform",
                        ["cardSelectionMinSelect"] = "2",
                        ["cardSelectionMaxSelect"] = "2",
                        ["cardSelectionSelectedCount"] = "2",
                        ["cardSelectionPreviewVisible"] = "true",
                        ["cardSelectionPreviewConfirmEnabled"] = "true",
                        ["cardSelectionPreviewMode"] = "transform-preview",
                    },
                },
                null,
                null,
                null);
            var transformConfirmActions = BuildAllowedActions(GuiSmokePhase.HandleEvent, transformConfirmObserver, Array.Empty<CombatCardKnowledgeHint>(), string.Empty, Array.Empty<GuiSmokeHistoryEntry>());
            Assert(transformConfirmActions.Contains("transform confirm", StringComparer.OrdinalIgnoreCase), "Transform preview state should open an explicit confirm lane.");
            Assert(transformConfirmActions.Contains("transform confirm", StringComparer.OrdinalIgnoreCase), "Transform preview-visible state should drive transform confirm.");

            using var postTransformEventMetaDocument = JsonDocument.Parse("""{"meta":{"rootTypeSummary":"MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen"}}""");
            var postTransformEventObserver = new ObserverState(
                new ObserverSummary(
                    "event",
                    "event",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-post-transform-event",
                    true,
                    "mixed",
                    "stable",
                    "episode-post-transform-event",
                    "None",
                    "event",
                    76,
                    80,
                    null,
                    new[] { "계속", "휴식 (1,2)" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("event-option:0", "event-option", "계속", "922,596,800,100", true),
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "계속", "922,596,800,100", "계속", "계속"),
                        new ObserverChoice("map-node", "휴식 (1,2)", "897,581,124,124", "1,2", "type:Rest;coord:1,2"),
                    },
                    Array.Empty<ObservedCombatHandCard>()),
                postTransformEventMetaDocument,
                null,
                null);
            var postTransformHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleEvent.ToString(), "click", "transform select card", DateTimeOffset.UtcNow.AddSeconds(-8)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleEvent.ToString(), "click", "transform select card left", DateTimeOffset.UtcNow.AddSeconds(-6)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleEvent.ToString(), "click", "transform confirm", DateTimeOffset.UtcNow.AddSeconds(-4)),
            };
            var postTransformAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleEvent,
                postTransformEventObserver,
                Array.Empty<CombatCardKnowledgeHint>(),
                string.Empty,
                postTransformHistory);
            Assert(postTransformAllowedActions.Contains("click event choice", StringComparer.OrdinalIgnoreCase),
                "Post-transform explicit event continue should stay in the HandleEvent allowlist.");
            var postTransformEventDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                11,
                GuiSmokePhase.HandleEvent.ToString(),
                "Resolve the event aftermath.",
                DateTimeOffset.UtcNow,
                string.Empty,
                new WindowBounds(0, 0, 1280, 720),
                "phase:handleevent|screen:event|visible:event|encounter:none|ready:true|stability:stable|layer:map-overlay-foreground|layer:event-background|stale:event-choice|current-node-arrow-visible|layer:event-foreground|shot:POSTTRANSFORM",
                "0001",
                1,
                3,
                true,
                "semantic",
                null,
                postTransformEventObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                new[]
                {
                    new EventKnowledgeCandidate(
                        "post-transform-continue",
                        "변성체의 숲",
                        "self-test post-transform continue",
                        new[]
                        {
                            new EventOptionKnowledgeCandidate("계속", "이벤트를 마무리한다.", "continue"),
                        }),
                },
                Array.Empty<CombatCardKnowledgeHint>(),
                postTransformAllowedActions,
                postTransformHistory,
                "Post-transform explicit event continue should outrank map fallback.",
                null));
            Assert(postTransformEventDecision.TargetLabel is not null
                   && (postTransformEventDecision.TargetLabel.Contains("event", StringComparison.OrdinalIgnoreCase)
                       || postTransformEventDecision.TargetLabel.Contains("계속", StringComparison.OrdinalIgnoreCase)
                       || postTransformEventDecision.TargetLabel.Contains("continue", StringComparison.OrdinalIgnoreCase))
                   && !postTransformEventDecision.TargetLabel.Contains("reachable node", StringComparison.OrdinalIgnoreCase)
                   && !postTransformEventDecision.TargetLabel.Contains("map", StringComparison.OrdinalIgnoreCase),
                "Post-transform explicit event continue should outrank mixed-state map contamination.");

            var ancientDialogueObserver = new ObserverState(
                new ObserverSummary(
                    "event",
                    "event",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-ancient-dialogue",
                    true,
                    "mixed",
                    "stable",
                    "episode-ancient-dialogue",
                    "None",
                    "event",
                    80,
                    80,
                    null,
                    new[] { "Ancient dialogue" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("ancient-dialogue:advance", "event-option", "Ancient dialogue", "0,0,1920,1080", true)
                        {
                            TypeName = "event-dialogue",
                            SemanticHints = new[] { "scene:event", "ancient-event", "source:ancient-dialogue-hitbox" },
                        },
                    },
                    new[]
                    {
                        new ObserverChoice("event-dialogue", "Ancient dialogue", "0,0,1920,1080", "dialogue-hitbox")
                        {
                            NodeId = "ancient-dialogue:advance",
                            Enabled = true,
                            SemanticHints = new[] { "scene:event", "ancient-event", "source:ancient-dialogue-hitbox" },
                        },
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["ancientEventDetected"] = "true",
                        ["ancientDialogueActive"] = "true",
                        ["ancientDialogueHitboxVisible"] = "true",
                        ["ancientDialogueHitboxEnabled"] = "true",
                        ["ancientOptionCount"] = "0",
                        ["ancientEventExtractionPath"] = "ancient-dialogue-hitbox",
                    },
                },
                null,
                null,
                null);
            var ancientDialogueActions = BuildAllowedActions(GuiSmokePhase.HandleEvent, ancientDialogueObserver, Array.Empty<CombatCardKnowledgeHint>(), string.Empty, Array.Empty<GuiSmokeHistoryEntry>());
            Assert(ancientDialogueActions.Contains("click ancient dialogue advance", StringComparer.OrdinalIgnoreCase)
                   && !ancientDialogueActions.Contains("click event choice", StringComparer.OrdinalIgnoreCase),
                "Ancient dialogue phase should expose only the explicit dialogue-advance lane.");
            var ancientDialogueDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                12,
                GuiSmokePhase.HandleEvent.ToString(),
                "Advance the ancient event dialogue.",
                DateTimeOffset.UtcNow,
                string.Empty,
                new WindowBounds(-1877, 405, 1280, 720),
                "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:ancient-dialogue",
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                ancientDialogueObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                ancientDialogueActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Advance the explicit ancient dialogue hitbox first.",
                null));
            Assert(string.Equals(ancientDialogueDecision.TargetLabel, "ancient dialogue advance", StringComparison.OrdinalIgnoreCase),
                "Ancient dialogue phase should click the explicit dialogue hitbox instead of selecting a generic event option.");

            var ancientOptionObserver = new ObserverState(
                new ObserverSummary(
                    "event",
                    "event",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-ancient-options",
                    true,
                    "mixed",
                    "stable",
                    "episode-ancient-options",
                    "None",
                    "event",
                    80,
                    80,
                    null,
                    new[] { "니오우의 비탄", "비전 두루마리", "두루마리 상자" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("ancient-event-option:0", "event-option", "니오우의 비탄", "460,360,820,92", true)
                        {
                            TypeName = "event-option",
                            SemanticHints = new[] { "scene:event", "ancient-event", "source:ancient-option-button" },
                        },
                        new ObserverActionNode("event-option:pseudo", "event-option", "니오우의 비탄", "460,1100,1000,100", true)
                        {
                            TypeName = "choice",
                            SemanticHints = new[] { "scene:event", "kind:event-option" },
                        },
                    },
                    new[]
                    {
                        new ObserverChoice("event-option", "니오우의 비탄", "460,360,820,92", "option-0")
                        {
                            NodeId = "ancient-event-option:0",
                            Enabled = true,
                            SemanticHints = new[] { "scene:event", "ancient-event", "source:ancient-option-button" },
                        },
                        new ObserverChoice("choice", "니오우의 비탄", "460,1100,1000,100", "니오우의 비탄"),
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["ancientEventDetected"] = "true",
                        ["ancientDialogueActive"] = "false",
                        ["ancientDialogueHitboxVisible"] = "false",
                        ["ancientDialogueHitboxEnabled"] = "false",
                        ["ancientOptionCount"] = "3",
                        ["ancientEventExtractionPath"] = "ancient-option-buttons",
                    },
                },
                null,
                null,
                null);
            var ancientOptionActions = BuildAllowedActions(GuiSmokePhase.HandleEvent, ancientOptionObserver, Array.Empty<CombatCardKnowledgeHint>(), string.Empty, Array.Empty<GuiSmokeHistoryEntry>());
            Assert(ancientOptionActions.Contains("click event choice", StringComparer.OrdinalIgnoreCase)
                   && !ancientOptionActions.Contains("click proceed", StringComparer.OrdinalIgnoreCase),
                "Ancient option phase should use explicit event-option buttons instead of generic proceed.");
            var ancientOptionDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                13,
                GuiSmokePhase.HandleEvent.ToString(),
                "Select an explicit ancient event option.",
                DateTimeOffset.UtcNow,
                string.Empty,
                new WindowBounds(0, 0, 1280, 720),
                "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:ancient-options",
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                ancientOptionObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                ancientOptionActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Prefer the explicit ancient option button over pseudo-choice duplicates.",
                null));
            Assert(string.Equals(ancientOptionDecision.TargetLabel, "event progression choice", StringComparison.OrdinalIgnoreCase)
                   && ancientOptionDecision.NormalizedY is < 0.95,
                "Ancient option phase should click the explicit in-window option button instead of an off-window pseudo-choice.");

            var ancientOffWindowOnlyObserver = new ObserverState(
                ancientOptionObserver.Summary with
                {
                    InventoryId = "inv-ancient-options-offwindow",
                    SceneEpisodeId = "episode-ancient-options-offwindow",
                    ActionNodes = new[]
                    {
                        new ObserverActionNode("event-option:pseudo", "event-option", "니오우의 비탄", "460,1100,1000,100", true)
                        {
                            TypeName = "choice",
                            SemanticHints = new[] { "scene:event", "kind:event-option" },
                        },
                    },
                    Choices = new[]
                    {
                        new ObserverChoice("choice", "니오우의 비탄", "460,1100,1000,100", "니오우의 비탄"),
                    },
                },
                null,
                null,
                null);
            var ancientOffWindowDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                14,
                GuiSmokePhase.HandleEvent.ToString(),
                "Do not click off-window pseudo-choice bounds.",
                DateTimeOffset.UtcNow,
                string.Empty,
                new WindowBounds(0, 0, 1280, 720),
                "phase:handleevent|screen:event|visible:event|ready:true|stability:stable|substate:ancient-options",
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                ancientOffWindowOnlyObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                new[] { "click event choice", "wait" },
                Array.Empty<GuiSmokeHistoryEntry>(),
                "No explicit button bounds remain in-window.",
                null));
            Assert(string.Equals(ancientOffWindowDecision.Status, "wait", StringComparison.OrdinalIgnoreCase),
                "Ancient event decisioning should wait instead of throwing on off-window pseudo-choice bounds.");

            var ancientFollowUpObserver = new ObserverState(
                transformSubtypeObserver.Summary with
                {
                    InventoryId = "inv-ancient-transform",
                    SceneEpisodeId = "episode-ancient-transform",
                    Meta = new Dictionary<string, string?>(transformSubtypeObserver.Meta, StringComparer.OrdinalIgnoreCase)
                    {
                        ["ancientEventDetected"] = "true",
                        ["ancientDialogueActive"] = "false",
                        ["ancientOptionCount"] = "3",
                        ["ancientEventExtractionPath"] = "ancient-option-buttons",
                    },
                },
                null,
                null,
                null);
            var ancientFollowUpActions = BuildAllowedActions(GuiSmokePhase.HandleEvent, ancientFollowUpObserver, Array.Empty<CombatCardKnowledgeHint>(), string.Empty, Array.Empty<GuiSmokeHistoryEntry>());
            Assert(ancientFollowUpActions.Contains("transform select card", StringComparer.OrdinalIgnoreCase)
                   && !ancientFollowUpActions.Contains("click ancient dialogue advance", StringComparer.OrdinalIgnoreCase),
                "Ancient option follow-up should hand off to the existing card-selection subtype instead of reopening ancient event routing.");

            var rewardPickObserver = new ObserverState(
                new ObserverSummary(
                    "card-choice",
                    "card-choice",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-reward-pick",
                    true,
                    "mixed",
                    "stable",
                    "episode-reward-pick",
                    "Reward",
                    "card-selection-reward-pick",
                    80,
                    80,
                    null,
                    new[] { "전투 최면" },
                    Array.Empty<string>(),
                    Array.Empty<ObserverActionNode>(),
                    new[]
                    {
                        new ObserverChoice("reward-pick-card", "전투 최면", "780,260,180,254", "CARD.BATTLE_TRANCE")
                        {
                            SemanticHints = new[] { "card-selection:reward-pick", "reward-pick" },
                        },
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["cardSelectionScreenType"] = "reward-pick",
                        ["cardSelectionSelectedCount"] = "0",
                        ["cardSelectionPreviewVisible"] = "false",
                    },
                },
                null,
                null,
                null);
            var rewardPickActions = BuildAllowedActions(GuiSmokePhase.HandleRewards, rewardPickObserver, Array.Empty<CombatCardKnowledgeHint>(), string.Empty, Array.Empty<GuiSmokeHistoryEntry>());
            Assert(rewardPickActions.Contains("reward pick card", StringComparer.OrdinalIgnoreCase), "Reward-pick subtype should expose a direct reward pick lane.");
            Assert(!rewardPickActions.Contains("click reward card choice", StringComparer.OrdinalIgnoreCase), "Reward-pick subtype should not reuse the generic reward row label.");
            Assert(!rewardPickActions.Contains("transform confirm", StringComparer.OrdinalIgnoreCase)
                   && !rewardPickActions.Contains("deck remove confirm", StringComparer.OrdinalIgnoreCase)
                   && !rewardPickActions.Contains("upgrade confirm", StringComparer.OrdinalIgnoreCase),
                "Reward-pick subtype should stay outside count/confirm card-selection flows.");
            var rewardFastPathContext = CreateStepAnalysisContext(
                GuiSmokePhase.HandleRewards,
                rewardPickObserver,
                rewardRankingScreenshotPath,
                Array.Empty<GuiSmokeHistoryEntry>(),
                Array.Empty<CombatCardKnowledgeHint>());
            Assert(rewardFastPathContext.UseRewardFastPath, "Explicit reward foreground/card-selection authority should enable the reward fast path.");
            var rewardFastPathSignature = ComputeSceneSignatureCore(rewardRankingScreenshotPath, rewardPickObserver, GuiSmokePhase.HandleRewards, rewardFastPathContext);
            Assert(rewardFastPathSignature.Contains("reward:fast-path", StringComparison.OrdinalIgnoreCase), "Reward fast path scene signatures should mark the fast-path contract explicitly.");
            Assert(!rewardFastPathSignature.Contains("shot:", StringComparison.OrdinalIgnoreCase), "Reward fast path scene signatures should not pay screenshot fingerprint overhead.");

            var deckRemoveObserver = new ObserverState(
                rewardPickObserver.Summary with
                {
                    CurrentScreen = "event",
                    VisibleScreen = "event",
                    InventoryId = "inv-deck-remove",
                    SceneEpisodeId = "episode-deck-remove",
                    ChoiceExtractorPath = "card-selection-deck-remove",
                    Choices = new[]
                    {
                        new ObserverChoice("deck-remove-confirm", "Confirm", "1700,720,180,110", "main-confirm")
                        {
                            BindingKind = "card-selection-confirm",
                            BindingId = "main",
                            SemanticHints = new[] { "card-selection:deck-remove", "confirm-mode:main" },
                            Enabled = true,
                        },
                    },
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["cardSelectionScreenType"] = "deck-remove",
                        ["cardSelectionMinSelect"] = "1",
                        ["cardSelectionMaxSelect"] = "2",
                        ["cardSelectionSelectedCount"] = "1",
                        ["cardSelectionMainConfirmEnabled"] = "true",
                    },
                },
                null,
                null,
                null);
            Assert(BuildAllowedActions(GuiSmokePhase.HandleEvent, deckRemoveObserver, Array.Empty<CombatCardKnowledgeHint>(), string.Empty, Array.Empty<GuiSmokeHistoryEntry>()).Contains("deck remove confirm", StringComparer.OrdinalIgnoreCase), "Deck-remove subtype should expose explicit confirm semantics.");

            var upgradeObserver = new ObserverState(
                deckRemoveObserver.Summary with
                {
                    InventoryId = "inv-upgrade-subtype",
                    SceneEpisodeId = "episode-upgrade-subtype",
                    ChoiceExtractorPath = "card-selection-upgrade",
                    Choices = new[]
                    {
                        new ObserverChoice("upgrade-confirm", "Confirm", "1700,720,180,110", "preview-confirm")
                        {
                            BindingKind = "card-selection-confirm",
                            BindingId = "preview",
                            SemanticHints = new[] { "card-selection:upgrade", "confirm-mode:preview" },
                            Enabled = true,
                        },
                    },
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["cardSelectionScreenType"] = "upgrade",
                        ["cardSelectionSelectedCount"] = "1",
                        ["cardSelectionPreviewVisible"] = "true",
                        ["cardSelectionPreviewConfirmEnabled"] = "true",
                        ["cardSelectionPreviewMode"] = "upgrade-single-preview",
                    },
                },
                null,
                null,
                null);
            Assert(BuildAllowedActions(GuiSmokePhase.HandleEvent, upgradeObserver, Array.Empty<CombatCardKnowledgeHint>(), string.Empty, Array.Empty<GuiSmokeHistoryEntry>()).Contains("upgrade confirm", StringComparer.OrdinalIgnoreCase), "Upgrade subtype should expose explicit confirm semantics.");

            var mixedRewardAfterClaimObserver = new ObserverState(
                new ObserverSummary(
                    "map",
                    "map",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-mixed-reward-after-claim",
                    true,
                    "mixed",
                    "stable",
                    "episode-mixed-reward-after-claim",
                    "Monster",
                    "reward",
                    76,
                    80,
                    1,
                    new[] { "약화 포션", "넘기기" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("map-node:0", "map-node", "약화 포션", "758,374,402,86", true),
                        new ObserverActionNode("map-node:1", "map-node", "넘기기", "1583,764,269,108", true),
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "약화 포션", "758,374,402,86", "약화 포션", "약화 포션"),
                        new ObserverChoice("choice", "넘기기", "1583,764,269,108", null, "넘기기"),
                        new ObserverChoice("potion", "약화 포션", "782,395,48,48", "POTION.WEAK_POTION", "약화를 부여합니다."),
                    },
                    Array.Empty<ObservedCombatHandCard>()),
                null,
                null,
                null);
            var mixedRewardAfterClaimHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleRewards.ToString(), "click", "claim reward item", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            var mixedRewardAfterClaimActions = BuildAllowedActions(
                GuiSmokePhase.HandleRewards,
                mixedRewardAfterClaimObserver,
                Array.Empty<CombatCardKnowledgeHint>(),
                rewardRankingScreenshotPath,
                mixedRewardAfterClaimHistory);
            Assert(!mixedRewardAfterClaimActions.Contains("click visible map advance", StringComparer.OrdinalIgnoreCase), "Reward allowlist should keep map fallback closed while a mixed reward/map state still exposes explicit reward choices.");
            Assert(mixedRewardAfterClaimActions.Contains("click reward", StringComparer.OrdinalIgnoreCase)
                   || mixedRewardAfterClaimActions.Contains("click reward choice", StringComparer.OrdinalIgnoreCase),
                "Reward allowlist should keep explicit reward actions open after the first reward claim when reward extractor evidence remains.");
            var mixedRewardAfterClaimDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                39,
                GuiSmokePhase.HandleRewards.ToString(),
                "Claim the remaining reward before leaving the reward flow.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlerewards|screen:map|visible:map|ready:true|stability:stable|layer:reward-foreground|layer:map-background",
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                mixedRewardAfterClaimObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                mixedRewardAfterClaimActions,
                mixedRewardAfterClaimHistory,
                "A prior reward claim should not suppress the next explicit reward item.",
                null));
            Assert(string.Equals(mixedRewardAfterClaimDecision.TargetLabel, "claim reward item", StringComparison.OrdinalIgnoreCase), "A prior claim reward click should not suppress the next explicit reward item in a mixed reward/map state.");
            Assert(ShouldAllowRewardMapRecovery(
                    new GuiSmokeStepRequest(
                        "run",
                        "boot-to-long-run",
                        39,
                        GuiSmokePhase.HandleRewards.ToString(),
                        "Allow one recapture window after a map recovery click.",
                        DateTimeOffset.UtcNow,
                        rewardRankingScreenshotPath,
                        new WindowBounds(1, 32, 1280, 720),
                        "phase:handlerewards|screen:rewards|visible:map|stale:reward-choice|stale:reward-bounds|layer:map-background|visible:map-arrow",
                        "0001",
                        1,
                        3,
                        true,
                        "tactical",
                        null,
                        staleRewardObserver.Summary,
                        Array.Empty<KnownRecipeHint>(),
                        Array.Empty<EventKnowledgeCandidate>(),
                        Array.Empty<CombatCardKnowledgeHint>(),
                        new[] { "click visible map advance", "click reward back", "wait" },
                        Array.Empty<GuiSmokeHistoryEntry>(),
                        "allow a short recovery window",
                        null),
                    new GuiSmokeStepDecision("act", "click", null, 0.7, 0.5, "visible reachable node", "recovery attempt", 0.9, "map", 1200, true, null)),
                "Reward-map recovery should allow a short recapture window after a real map progression click.");
        }
        finally
        {
            if (File.Exists(rewardRankingScreenshotPath))
            {
                File.Delete(rewardRankingScreenshotPath);
            }
        }
    }
}
