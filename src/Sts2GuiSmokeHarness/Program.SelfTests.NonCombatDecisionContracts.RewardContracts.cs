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
    private static void RunNonCombatRewardDecisionContractSelfTests(string rewardRankingScreenshotPath)
    {
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

            var staleRewardObserver = CreateStaleRewardObserver();
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
            var parsedRewardAftermathContradictionState = rewardAftermathContradictionState!;
            Assert(!parsedRewardAftermathContradictionState.ForegroundOwned, "Map-current reward teardown should override stale top-overlay foreground ownership.");
            Assert(parsedRewardAftermathContradictionState.TeardownInProgress, "Map-current reward teardown should be recognized even when old metadata still claims foreground ownership.");
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
                        new ObserverActionNode("map:2:4", "map-node", "Unknown (2,4)", "1060.739,503.598,56,56", true)
                        {
                            TypeName = "map-node",
                        },
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "넘기기", "1583,764,269,108"),
                        new ObserverChoice("map-node", "Unknown (2,4)", "1060.739,503.598,56,56", "2,4", "type:Unknown;state:Travelable;coord:2,4")
                        {
                            NodeId = "map:2:4",
                            Enabled = true,
                        },
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
            var rewardAftermathScene = AutoDecisionProvider.BuildRewardSceneState(
                rewardAftermathObserver,
                new WindowBounds(0, 0, 1280, 720),
                Array.Empty<GuiSmokeHistoryEntry>(),
                rewardRankingScreenshotPath);
            Assert(!rewardAftermathState.RewardForegroundOwned, "Reward proceed aftermath should drop reward foreground ownership once map becomes current.");
            Assert(rewardAftermathState.RewardTeardownInProgress, "Reward proceed aftermath should export teardown-in-progress while stale reward visuals linger.");
            Assert(rewardAftermathScene.CanonicalForegroundOwner == NonCombatForegroundOwner.Map, "Reward proceed aftermath should compute map as the current reward-scene owner once map becomes current.");
            Assert(((ICanonicalNonCombatSceneState)rewardAftermathScene).CanonicalForegroundOwner == NonCombatCanonicalForegroundOwner.Map, "Canonical reward contract should surface map ownership for released-to-map reward aftermath.");
            Assert(((ICanonicalNonCombatSceneState)rewardAftermathScene).HandoffTarget == NonCombatHandoffTarget.WaitMap, "Released-to-map reward aftermath should hand off through WaitMap under the canonical contract.");
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
            var rewardAftermathChooseFirstNodeRequest = new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                45,
                GuiSmokePhase.ChooseFirstNode.ToString(),
                "Click the first reachable map node after reward release.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                ComputeSceneSignature(rewardRankingScreenshotPath, rewardAftermathObserver, GuiSmokePhase.ChooseFirstNode),
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
                GetAllowedActions(GuiSmokePhase.ChooseFirstNode, rewardAftermathObserver),
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Released-to-map reward aftermath should route to exported map nodes, not stay blocked behind stale reward wrappers.",
                null);
            var rewardAftermathMapNodeDecision = AutoDecisionProvider.Decide(rewardAftermathChooseFirstNodeRequest);
            Assert(string.Equals(rewardAftermathMapNodeDecision.TargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase), "ChooseFirstNode should click the exported travelable map node once reward ownership has released to map.");

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
            Assert(((ICanonicalNonCombatSceneState)rewardReleasePendingState).CanonicalForegroundOwner == NonCombatCanonicalForegroundOwner.Reward, "Reward release-pending should still report reward as the canonical owner until the release finishes.");
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
                Assert(AutoDecisionProvider.BuildShopSceneState(postShopRewardMixedObserver, Array.Empty<GuiSmokeHistoryEntry>()) is
                    {
                        CanonicalForegroundOwner: NonCombatCanonicalForegroundOwner.Map,
                        ReleaseStage: NonCombatReleaseStage.Released,
                        HandoffTarget: NonCombatHandoffTarget.WaitMap,
                    },
                    "Released-to-map shop aftermath should report map as the canonical owner while preserving WaitMap handoff.");
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
            var combatContradictionDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
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
                null));
            Assert(!string.Equals(combatContradictionDecision.TargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(combatContradictionDecision.TargetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(combatContradictionDecision.TargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase),
                "Combat foreground should keep stale map fallback lanes closed.");

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
    }
}
