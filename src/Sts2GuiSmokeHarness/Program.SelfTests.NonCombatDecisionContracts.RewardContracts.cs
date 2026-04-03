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
using static GuiSmokeSceneReasoningSupport;
using static GuiSmokeStepRequestFactory;
using static GuiSmokeRewardMapEvidenceSupport;

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
            Assert(string.Equals(rewardDecision.TargetLabel, "claim reward gold", StringComparison.OrdinalIgnoreCase), "Explicit gold rewards should preserve the reward lane over screenshot-derived map routing on NRewardsScreen.");
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
                    Array.Empty<ObservedCombatHandCard>())
                {
                    PublishedCurrentScreen = "rewards",
                    PublishedVisibleScreen = "map",
                    PublishedSceneReady = false,
                    PublishedSceneAuthority = "hook",
                    PublishedSceneStability = "stabilizing",
                },
                null,
                null,
                null);
            var layeredRewardState = BuildRewardMapLayerState(layeredRewardObserver.Summary, new WindowBounds(0, 0, 1280, 720));
            Assert(layeredRewardState.RewardPanelVisible, "Layered reward/map state should keep the reward panel as foreground while explicit reward bounds remain usable.");
            Assert(layeredRewardState.MapContextVisible, "Layered reward/map state should preserve background map context instead of forcing a single exclusive screen.");
            Assert(layeredRewardState.RewardBackNavigationAvailable, "Layered reward/map state should record reward back-navigation affordances.");

            var rewardBackObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "map",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-reward-back",
                    true,
                    "mixed",
                    "stable",
                    "episode-reward-back",
                    "None",
                    "reward",
                    80,
                    80,
                    null,
                    new[] { "Back", "Proceed" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("overlay:back", "choice", "Back", "48,620,88,88", true),
                        new ObserverActionNode("proceed:stale", "proceed", "Proceed", "1983,764,269,108", true),
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "Back", "48,620,88,88"),
                        new ObserverChoice("choice", "Proceed", "1983,764,269,108"),
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    PublishedCurrentScreen = "rewards",
                    PublishedVisibleScreen = "map",
                    PublishedSceneReady = true,
                    PublishedSceneAuthority = "mixed",
                    PublishedSceneStability = "stable",
                    Meta = new Dictionary<string, string?>
                    {
                        ["rewardScreenDetected"] = "true",
                        ["rewardScreenVisible"] = "true",
                        ["rewardForegroundOwned"] = "true",
                        ["rewardTeardownInProgress"] = "false",
                        ["rewardIsCurrentActiveScreen"] = "true",
                        ["rewardIsTopOverlay"] = "true",
                        ["rewardProceedVisible"] = "false",
                        ["rewardProceedEnabled"] = "false",
                        ["rewardVisibleButtonCount"] = "0",
                        ["rewardEnabledButtonCount"] = "0",
                        ["mapCurrentActiveScreen"] = "false",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                        ["choiceExtractorPath"] = "reward",
                    },
                },
                null,
                null,
                null);
            var rewardBackScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-reward-back-{Guid.NewGuid():N}.png");
            var rewardBackScene = AutoDecisionProvider.BuildRewardSceneState(
                rewardBackObserver,
                new WindowBounds(0, 0, 1280, 720),
                history: null,
                screenshotPath: rewardBackScreenshotPath);
            Assert(rewardBackScene.LayerState.StaleRewardChoicePresent, "Reward back states should keep stale reward progression residue visible while the reward foreground still owns the lane.");
            Assert(rewardBackScene.ExplicitAction == RewardExplicitActionKind.Back, "Reward back states should resolve to the explicit back action when stale reward residue is the only remaining progression surface.");
            Assert(
                GetAllowedActions(GuiSmokePhase.HandleRewards, rewardBackObserver).SequenceEqual(new[] { "click reward back", "wait" }, StringComparer.OrdinalIgnoreCase),
                "Reward back states should collapse the HandleRewards allowlist to reward-back navigation.");
            var rewardBackDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                34,
                GuiSmokePhase.HandleRewards.ToString(),
                "Use the visible reward back navigation to dismiss stale reward residue over the map.",
                DateTimeOffset.UtcNow,
                rewardBackScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                "phase:handlerewards|screen:rewards|visible:map|encounter:none|ready:true|stability:stable|layer:reward-foreground|layer:map-background|layer:reward-back-nav|stale:reward-choice",
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                rewardBackObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                GetAllowedActions(GuiSmokePhase.HandleRewards, rewardBackObserver),
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Reward back navigation is the only valid progression lane while stale reward residue sits over the map.",
                null));
            Assert(string.Equals(rewardBackDecision.TargetLabel, "reward back", StringComparison.OrdinalIgnoreCase), "Reward back states should click the explicit back node instead of waiting or reopening map fallback.");

            var staleRewardObserver = CreateStaleRewardObserver();
            var staleRewardState = BuildRewardMapLayerState(staleRewardObserver.Summary, new WindowBounds(1, 32, 1280, 720));
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
                        new ObserverActionNode("map:11:4", "map-node", "Monster (11,4)", "1018,514,56,56", true)
                        {
                            TypeName = "map-node",
                        },
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "진행", "1583,764,269,108"),
                        new ObserverChoice("map-node", "Monster (11,4)", "1018,514,56,56", "11,4", "type:Monster;state:Travelable;coord:11,4")
                        {
                            NodeId = "map:11:4",
                            Enabled = true,
                        },
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    PublishedCurrentScreen = "rewards",
                    PublishedVisibleScreen = "map",
                    PublishedSceneReady = true,
                    PublishedSceneAuthority = "hook",
                    PublishedSceneStability = "stabilizing",
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

            var staleEnabledRewardAftermathObserver = new ObserverState(
                new ObserverSummary(
                    "map",
                    "map",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-reward-aftermath-stale-enabled",
                    true,
                    "mixed",
                    "stable",
                    "episode-reward-aftermath-stale-enabled",
                    "None",
                    "map",
                    80,
                    80,
                    null,
                    new[] { "Monster (14,5)" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("map:14:5", "map-node", "Monster (14,5)", "1205.072,544.27,56,56", true)
                        {
                            TypeName = "map-node",
                        },
                    },
                    new[]
                    {
                        new ObserverChoice("map-node", "Monster (14,5)", "1205.072,544.27,56,56", "14,5", "type:Monster;state:Travelable;coord:14,5")
                        {
                            NodeId = "map:14:5",
                            Enabled = true,
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
                        ["rewardScreenDetected"] = "true",
                        ["rewardScreenVisible"] = "true",
                        ["rewardForegroundOwned"] = "true",
                        ["rewardTeardownInProgress"] = "false",
                        ["rewardIsCurrentActiveScreen"] = "false",
                        ["rewardIsTopOverlay"] = "true",
                        ["rewardProceedVisible"] = "true",
                        ["rewardProceedEnabled"] = "false",
                        ["rewardVisibleButtonCount"] = "1",
                        ["rewardEnabledButtonCount"] = "1",
                        ["mapCurrentActiveScreen"] = "true",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    },
                },
                null,
                null,
                null);
            var staleEnabledRewardAftermathState = RewardObserverSignals.TryGetState(staleEnabledRewardAftermathObserver.Summary);
            Assert(staleEnabledRewardAftermathState is not null, "Map-current stale reward button metadata should still parse into a reward state.");
            Assert(!staleEnabledRewardAftermathState!.ForegroundOwned, "Map-current stale reward button metadata must not keep reward foreground ownership alive once only map-node surface remains.");
            Assert(staleEnabledRewardAftermathState.TeardownInProgress, "Map-current stale reward button metadata should downgrade to reward teardown instead of active reward ownership.");
            var staleEnabledRewardAftermathScene = AutoDecisionProvider.BuildRewardSceneState(
                staleEnabledRewardAftermathObserver,
                new WindowBounds(0, 0, 1280, 720),
                Array.Empty<GuiSmokeHistoryEntry>(),
                rewardRankingScreenshotPath);
            Assert(staleEnabledRewardAftermathScene.CanonicalForegroundOwner == NonCombatForegroundOwner.Map,
                "Canonical reward contract should release to map when only stale reward button metadata remains over an explicit reachable-node surface.");
            Assert(
                new ObserverAcceptanceEvaluator().IsPhaseSatisfied(
                    GuiSmokePhase.WaitMap,
                    staleEnabledRewardAftermathObserver,
                    new[] { new GuiSmokeHistoryEntry(GuiSmokePhase.HandleRewards.ToString(), "click", "reward skip", DateTimeOffset.UtcNow) }),
                "WaitMap should accept map ownership once reachable-node surface is current and reward only survives as stale enabled-button metadata.");

            var metaOnlyRewardClaimObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "map",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-reward-meta-only-claim",
                    true,
                    "mixed",
                    "stable",
                    "episode-reward-meta-only-claim",
                    "Monster",
                    "reward",
                    80,
                    80,
                    null,
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    Array.Empty<ObserverActionNode>(),
                    Array.Empty<ObserverChoice>(),
                    Array.Empty<ObservedCombatHandCard>())
                {
                    PublishedCurrentScreen = "rewards",
                    PublishedVisibleScreen = "map",
                    PublishedSceneReady = true,
                    PublishedSceneAuthority = "hook",
                    PublishedSceneStability = "stable",
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
                        ["rewardEnabledButtonCount"] = "1",
                        ["mapCurrentActiveScreen"] = "true",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    },
                },
                null,
                null,
                null);
            var metaOnlyRewardClaimState = RewardObserverSignals.TryGetState(metaOnlyRewardClaimObserver.Summary);
            Assert(metaOnlyRewardClaimState is not null, "Meta-only reward claim observers should still parse into a reward state.");
            Assert(metaOnlyRewardClaimState!.ForegroundOwned, "Reward teardown narrowing must not release a top-overlay reward when map-current truth is present but no explicit map progression surface exists yet.");
            Assert(!metaOnlyRewardClaimState.TeardownInProgress, "Meta-only reward claim observers should stay in active reward ownership until explicit map progression evidence appears.");

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
                    PublishedCurrentScreen = "rewards",
                    PublishedVisibleScreen = "map",
                    PublishedSceneReady = true,
                    PublishedSceneAuthority = "hook",
                    PublishedSceneStability = "stable",
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
            var rewardAftermathState = BuildRewardMapLayerState(rewardAftermathObserver.Summary, new WindowBounds(0, 0, 1280, 720));
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
            var rewardAftermathAnalysisContext = CreateRequestAnalysisContext(rewardAftermathRequest);
            Assert(ReferenceEquals(rewardAftermathAnalysisContext.RewardScene, rewardAftermathAnalysisContext.RewardScene), "Request analysis context should cache reward scene truth within the same request.");
            Assert(ReferenceEquals(rewardAftermathAnalysisContext.EventScene, rewardAftermathAnalysisContext.EventScene), "Request analysis context should cache event scene truth within the same request.");
            Assert(ReferenceEquals(rewardAftermathAnalysisContext.CanonicalNonCombatScene, rewardAftermathAnalysisContext.CanonicalNonCombatScene), "Request analysis context should cache canonical noncombat scene truth within the same request.");
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
                "Released-to-map reward aftermath should route to exported map nodes, not stay blocked behind stale reward scene-state residue.",
                null);
            var rewardAftermathMapNodeDecision = AutoDecisionProvider.Decide(rewardAftermathChooseFirstNodeRequest);
            Assert(string.Equals(rewardAftermathMapNodeDecision.TargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase), "ChooseFirstNode should click the exported travelable map node once reward ownership has released to map.");
            var rewardAftermathChooseFirstNodeAnalysisContext = CreateRequestAnalysisContext(rewardAftermathChooseFirstNodeRequest);
            var rewardAftermathChooseFirstNodeAnalysis = AutoDecisionProvider.Analyze(rewardAftermathChooseFirstNodeRequest, analysisContext: rewardAftermathChooseFirstNodeAnalysisContext);
            Assert(string.Equals(rewardAftermathChooseFirstNodeAnalysis.FinalDecision.TargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase), "Request-scoped cached noncombat scene truth should preserve ChooseFirstNode exported map-node routing.");

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

            var rewardProceedMetaOnlyScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-reward-proceed-meta-{Guid.NewGuid():N}.png");
            var rewardProceedMetaOnlyObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "rewards",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-reward-proceed-meta-only",
                    true,
                    "mixed",
                    "stable",
                    "episode-reward-proceed-meta-only",
                    "None",
                    "reward",
                    80,
                    80,
                    null,
                    new[] { "진행" },
                    Array.Empty<string>(),
                    Array.Empty<ObserverActionNode>(),
                    Array.Empty<ObserverChoice>(),
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>
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
                        ["mapCurrentActiveScreen"] = "false",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                    },
                },
                null,
                null,
                null);
            var rewardProceedMetaOnlyState = AutoDecisionProvider.BuildRewardSceneState(
                rewardProceedMetaOnlyObserver,
                new WindowBounds(0, 0, 1280, 720),
                Array.Empty<GuiSmokeHistoryEntry>(),
                rewardProceedMetaOnlyScreenshotPath);
            Assert(rewardProceedMetaOnlyState.ExplicitProceedVisible, "Reward scene truth should promote runtime proceed visibility even when no explicit reward hitbox is exported.");
            Assert(rewardProceedMetaOnlyState.ExplicitAction == RewardExplicitActionKind.SkipProceed, "Proceed-only reward aftermath should normalize to the skip/proceed lane.");
            var rewardProceedMetaOnlyAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleRewards,
                rewardProceedMetaOnlyObserver,
                Array.Empty<CombatCardKnowledgeHint>(),
                rewardProceedMetaOnlyScreenshotPath,
                Array.Empty<GuiSmokeHistoryEntry>());
            Assert(rewardProceedMetaOnlyAllowedActions.Contains("click proceed", StringComparer.OrdinalIgnoreCase), "Proceed-only reward runtime metadata should open the reward proceed lane in HandleRewards.");
            var rewardProceedMetaOnlyDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                46,
                GuiSmokePhase.HandleRewards.ToString(),
                "Proceed-only reward runtime metadata should still advance the reward screen.",
                DateTimeOffset.UtcNow,
                rewardProceedMetaOnlyScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                ComputeSceneSignature(rewardProceedMetaOnlyScreenshotPath, rewardProceedMetaOnlyObserver, GuiSmokePhase.HandleRewards),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                rewardProceedMetaOnlyObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                rewardProceedMetaOnlyAllowedActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Proceed-only reward runtime metadata should click the proceed anchor instead of waiting indefinitely.",
                null));
            Assert(string.Equals(rewardProceedMetaOnlyDecision.TargetLabel, "proceed after resolving rewards", StringComparison.OrdinalIgnoreCase), "Proceed-only reward runtime metadata should still click reward proceed without requiring an exported hitbox.");

            var fullPotionSlotObserver = new ObserverState(
                new ObserverSummary(
                    "event",
                    "event",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-reward-potion-full",
                    true,
                    "mixed",
                    "stable",
                    "episode-reward-potion-full",
                    "Event",
                    "reward",
                    80,
                    80,
                    null,
                    new[] { "액상 기억", "강장제", "Proceed" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("reward-potion:0", "event-option", "액상 기억", "758,364,402,86", true)
                        {
                            TypeName = "potion",
                            SemanticHints = new[] { "reward", "reward-potion", "reward-type:PotionReward" },
                        },
                        new ObserverActionNode("reward-potion:1", "event-option", "강장제", "758,460,402,86", true)
                        {
                            TypeName = "potion",
                            SemanticHints = new[] { "reward", "reward-potion", "reward-type:PotionReward" },
                        },
                        new ObserverActionNode("reward-proceed:2", "event-option", "Proceed", "1583,764,269,108", true),
                    },
                    new[]
                    {
                        new ObserverChoice("potion", "액상 기억", "758,364,402,86", "PotionReward", "액상 기억")
                        {
                            BindingKind = "reward-type",
                            BindingId = "PotionReward",
                            SemanticHints = new[] { "reward", "reward-potion", "reward-type:PotionReward" },
                        },
                        new ObserverChoice("potion", "강장제", "758,460,402,86", "PotionReward", "강장제")
                        {
                            BindingKind = "reward-type",
                            BindingId = "PotionReward",
                            SemanticHints = new[] { "reward", "reward-potion", "reward-type:PotionReward" },
                        },
                        new ObserverChoice("choice", "Proceed", "1583,764,269,108"),
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>
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
                        ["hasOpenPotionSlots"] = "false",
                        ["mapCurrentActiveScreen"] = "false",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                        ["rewardScreenRootType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                    },
                },
                null,
                null,
                null);
            var fullPotionSlotScene = AutoDecisionProvider.BuildRewardSceneState(
                fullPotionSlotObserver,
                new WindowBounds(0, 0, 1920, 1080),
                Array.Empty<GuiSmokeHistoryEntry>(),
                rewardRankingScreenshotPath);
            Assert(!fullPotionSlotScene.ClaimableRewardPresent, "Visible potion rewards should not remain claimable when runtime truth reports no open potion slots.");
            Assert(fullPotionSlotScene.ExplicitAction == RewardExplicitActionKind.SkipProceed, "Potion-only reward residue with full slots should normalize to proceed instead of claim.");
            var fullPotionSlotAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleRewards,
                fullPotionSlotObserver,
                Array.Empty<CombatCardKnowledgeHint>(),
                rewardRankingScreenshotPath,
                Array.Empty<GuiSmokeHistoryEntry>());
            Assert(!fullPotionSlotAllowedActions.Contains("click reward", StringComparer.OrdinalIgnoreCase)
                   && !fullPotionSlotAllowedActions.Contains("click reward choice", StringComparer.OrdinalIgnoreCase),
                "Potion-only reward residue with full slots should keep reward claim actions closed.");
            Assert(fullPotionSlotAllowedActions.Contains("click proceed", StringComparer.OrdinalIgnoreCase),
                "Potion-only reward residue with full slots should keep proceed available.");
            var fullPotionSlotDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                47,
                GuiSmokePhase.HandleRewards.ToString(),
                "A full potion belt should force reward proceed instead of a stale potion claim loop.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1920, 1080),
                ComputeSceneSignature(rewardRankingScreenshotPath, fullPotionSlotObserver, GuiSmokePhase.HandleRewards),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                fullPotionSlotObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                fullPotionSlotAllowedActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Visible potion rewards remain on screen, but runtime truth says there is no open potion slot. Proceed instead of looping on claim.",
                null));
            Assert(string.Equals(fullPotionSlotDecision.TargetLabel, "proceed after resolving rewards", StringComparison.OrdinalIgnoreCase),
                "Potion-only reward residue with full slots should click proceed instead of claim reward item.");

            var unknownPotionCapacityMeta = new Dictionary<string, string?>(fullPotionSlotObserver.Summary.Meta, StringComparer.OrdinalIgnoreCase)
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
                ["mapCurrentActiveScreen"] = "false",
                ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                ["rewardScreenRootType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
            };
            unknownPotionCapacityMeta.Remove("hasOpenPotionSlots");
            var unknownPotionCapacityObserver = fullPotionSlotObserver with
            {
                Summary = fullPotionSlotObserver.Summary with
                {
                    Meta = unknownPotionCapacityMeta,
                },
            };
            var unknownPotionCapacityScene = AutoDecisionProvider.BuildRewardSceneState(
                unknownPotionCapacityObserver,
                new WindowBounds(0, 0, 1920, 1080),
                Array.Empty<GuiSmokeHistoryEntry>(),
                rewardRankingScreenshotPath);
            Assert(!unknownPotionCapacityScene.ClaimableRewardPresent,
                "Potion rewards should stay unclaimable when slot capacity is unknown; unknown runtime truth must not reopen stale claim loops.");
            Assert(unknownPotionCapacityScene.ExplicitAction == RewardExplicitActionKind.SkipProceed,
                "Potion-only reward residue with unknown slot capacity should normalize to proceed instead of claim.");
            var unknownPotionCapacityAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleRewards,
                unknownPotionCapacityObserver,
                Array.Empty<CombatCardKnowledgeHint>(),
                rewardRankingScreenshotPath,
                Array.Empty<GuiSmokeHistoryEntry>());
            Assert(!unknownPotionCapacityAllowedActions.Contains("click reward", StringComparer.OrdinalIgnoreCase)
                   && !unknownPotionCapacityAllowedActions.Contains("click reward choice", StringComparer.OrdinalIgnoreCase),
                "Unknown potion capacity should keep reward claim actions closed.");
            Assert(unknownPotionCapacityAllowedActions.Contains("click proceed", StringComparer.OrdinalIgnoreCase),
                "Unknown potion capacity should still keep proceed available.");
            var unknownPotionCapacityDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                47,
                GuiSmokePhase.HandleRewards.ToString(),
                "Unknown potion capacity should follow AutoSlay's strict contract and prefer proceed over looping on potion claim.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1920, 1080),
                ComputeSceneSignature(rewardRankingScreenshotPath, unknownPotionCapacityObserver, GuiSmokePhase.HandleRewards),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                unknownPotionCapacityObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                unknownPotionCapacityAllowedActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Visible potion rewards without confirmed slot capacity should not loop on claim.",
                null));
            Assert(string.Equals(unknownPotionCapacityDecision.TargetLabel, "proceed after resolving rewards", StringComparison.OrdinalIgnoreCase),
                "Unknown potion capacity should click proceed instead of claim reward item.");

            var openPotionSlotObserver = fullPotionSlotObserver with
            {
                Summary = fullPotionSlotObserver.Summary with
                {
                    Meta = new Dictionary<string, string?>(fullPotionSlotObserver.Summary.Meta, StringComparer.OrdinalIgnoreCase)
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
                        ["hasOpenPotionSlots"] = "true",
                        ["mapCurrentActiveScreen"] = "false",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                        ["rewardScreenRootType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                    },
                },
            };
            var openPotionSlotScene = AutoDecisionProvider.BuildRewardSceneState(
                openPotionSlotObserver,
                new WindowBounds(0, 0, 1920, 1080),
                Array.Empty<GuiSmokeHistoryEntry>(),
                rewardRankingScreenshotPath);
            Assert(openPotionSlotScene.ClaimableRewardPresent, "Potion rewards should stay claimable when runtime truth reports an open potion slot.");

            var staleMapPotionNodeObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "rewards",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-stale-map-potion-node",
                    true,
                    "rewards",
                    "stable",
                    "episode-reward-stale-map-node",
                    "Monster",
                    "reward",
                    80,
                    80,
                    2,
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("potion:0", "potion", "무쇠의 심장", "758,373.299,402,86", true)
                        {
                            TypeName = "potion",
                            SemanticHints = new[]
                            {
                                "scene:map",
                                "kind:potion",
                                "scene-raw:map",
                                "scene-published:map",
                                "value:무쇠의 심장",
                                "raw-kind:potion",
                                "reward",
                                "reward-potion",
                                "reward-type:PotionReward",
                            },
                        },
                    },
                    new[]
                    {
                        new ObserverChoice("card", "덱에 추가할 카드를 선택하세요.", "758,469,402,86", "CardReward", "카드 보상")
                        {
                            BindingKind = "reward-type",
                            BindingId = "CardReward",
                            SemanticHints = new[] { "reward", "reward-card", "reward-type:CardReward" },
                        },
                        new ObserverChoice("choice", "넘기기", "1583,764,269,108", null, "넘기기"),
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>
                    {
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
                        ["hasOpenPotionSlots"] = "true",
                        ["mapCurrentActiveScreen"] = "false",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                        ["rewardScreenRootType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                    },
                },
                null,
                null,
                null);
            var staleMapPotionNodeScene = AutoDecisionProvider.BuildRewardSceneState(
                staleMapPotionNodeObserver,
                new WindowBounds(0, 0, 1920, 1080),
                Array.Empty<GuiSmokeHistoryEntry>(),
                rewardRankingScreenshotPath);
            Assert(!staleMapPotionNodeScene.ClaimableRewardPresent,
                "Map-scoped stale potion nodes must not reopen reward claimability after the current reward surface has already collapsed to card choice plus proceed.");
            Assert(staleMapPotionNodeScene.ExplicitAction == RewardExplicitActionKind.CardChoice,
                "Current reward card choice should outrank stale map-scoped potion node residue.");
            var staleMapPotionNodeAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleRewards,
                staleMapPotionNodeObserver,
                Array.Empty<CombatCardKnowledgeHint>(),
                rewardRankingScreenshotPath,
                Array.Empty<GuiSmokeHistoryEntry>());
            Assert(!staleMapPotionNodeAllowedActions.Contains("click reward", StringComparer.OrdinalIgnoreCase),
                "Stale map-scoped potion nodes should not reopen generic reward-claim actions.");
            Assert(staleMapPotionNodeAllowedActions.Contains("click reward card choice", StringComparer.OrdinalIgnoreCase),
                "Current reward card choice should remain actionable.");
            var staleMapPotionNodeDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                48,
                GuiSmokePhase.HandleRewards.ToString(),
                "Stale map-scoped reward nodes should not outrank the current reward card choice.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1920, 1080),
                ComputeSceneSignature(rewardRankingScreenshotPath, staleMapPotionNodeObserver, GuiSmokePhase.HandleRewards),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                staleMapPotionNodeObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                staleMapPotionNodeAllowedActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Current reward choices should beat stale potion node residue from the map layer.",
                null));
            Assert(string.Equals(staleMapPotionNodeDecision.TargetLabel, "reward card choice", StringComparison.OrdinalIgnoreCase),
                "Reward decision should choose the current card reward instead of stale potion residue.");

            var explicitRelicRewardObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "rewards",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-explicit-relic-reward",
                    true,
                    "room",
                    "stable",
                    "episode-explicit-relic-reward",
                    null,
                    "event",
                    80,
                    80,
                    null,
                    new[] { "기묘하게 매끄러운 돌", "넘기기" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("reward-relic-node", "relic", "기묘하게 매끄러운 돌", "758,374,402,86", true)
                        {
                            TypeName = "reward-type",
                            SemanticHints = new[] { "reward", "reward-relic", "reward-type:RelicReward" },
                        },
                    },
                    new[]
                    {
                        new ObserverChoice("relic", "기묘하게 매끄러운 돌", "758,374,402,86", "기묘하게 매끄러운 돌")
                        {
                            BindingKind = "reward-type",
                            BindingId = "RelicReward",
                            SemanticHints = new[] { "reward", "reward-relic", "reward-type:RelicReward" },
                        },
                        new ObserverChoice("choice", "넘기기", "1983,764,269,108"),
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>
                    {
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
                        ["hasOpenPotionSlots"] = "true",
                        ["mapCurrentActiveScreen"] = "false",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                        ["rewardScreenRootType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                    },
                },
                null,
                null,
                null);
            var explicitRelicRewardScene = AutoDecisionProvider.BuildRewardSceneState(
                explicitRelicRewardObserver,
                new WindowBounds(0, 0, 1920, 1080),
                Array.Empty<GuiSmokeHistoryEntry>(),
                rewardRankingScreenshotPath);
            Assert(GuiSmokeNonCombatContractSupport.HasExplicitRewardProgressionAffordance(explicitRelicRewardObserver.Summary),
                "Observer reward contract support should expose the same explicit relic reward affordance as the reward scene-state.");
            Assert(explicitRelicRewardScene.ExplicitAction == RewardExplicitActionKind.Claim,
                "Explicit relic rewards must stay on the claim lane instead of being promoted to reward card-choice.");
            var explicitRelicRewardAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleRewards,
                explicitRelicRewardObserver,
                Array.Empty<CombatCardKnowledgeHint>(),
                rewardRankingScreenshotPath,
                Array.Empty<GuiSmokeHistoryEntry>());
            Assert(explicitRelicRewardAllowedActions.Contains("click reward", StringComparer.OrdinalIgnoreCase)
                   && !explicitRelicRewardAllowedActions.Contains("click reward card choice", StringComparer.OrdinalIgnoreCase),
                "Relic reward claim states should expose reward claim actions, not reward card-choice actions.");
            var explicitRelicRewardDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                49,
                GuiSmokePhase.HandleRewards.ToString(),
                "Explicit relic rewards should claim the relic instead of waiting for a reward card surface.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1920, 1080),
                ComputeSceneSignature(rewardRankingScreenshotPath, explicitRelicRewardObserver, GuiSmokePhase.HandleRewards),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                explicitRelicRewardObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                explicitRelicRewardAllowedActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Relic reward claim states should not stall behind the reward card-choice contract.",
                null));
            Assert(string.Equals(explicitRelicRewardDecision.TargetLabel, "claim reward relic", StringComparison.OrdinalIgnoreCase),
                "Reward decision should claim the relic reward directly instead of waiting for a reward card surface.");

            var nodeOnlyRelicRewardObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "rewards",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-node-only-relic-reward",
                    true,
                    "room",
                    "stable",
                    "episode-node-only-relic-reward",
                    null,
                    "event",
                    80,
                    80,
                    null,
                    new[] { "기묘하게 매끄러운 돌", "넘기기" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("reward-relic-node-only", "relic", "기묘하게 매끄러운 돌", "758,374,402,86", true)
                        {
                            TypeName = "reward-type",
                            SemanticHints = new[] { "reward", "reward-relic", "reward-type:RelicReward" },
                        },
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "넘기기", "1983,764,269,108"),
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>
                    {
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
                        ["hasOpenPotionSlots"] = "true",
                        ["mapCurrentActiveScreen"] = "false",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                        ["rewardScreenRootType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                    },
                },
                null,
                null,
                null);
            var nodeOnlyRelicRewardScene = AutoDecisionProvider.BuildRewardSceneState(
                nodeOnlyRelicRewardObserver,
                new WindowBounds(0, 0, 1920, 1080),
                Array.Empty<GuiSmokeHistoryEntry>(),
                rewardRankingScreenshotPath);
            Assert(GuiSmokeNonCombatContractSupport.HasExplicitRewardProgressionAffordance(nodeOnlyRelicRewardObserver.Summary),
                "Observer reward contract support should keep reward-type relic nodes claimable when the rewards screen is foreground-owned.");
            Assert(nodeOnlyRelicRewardScene.ExplicitAction == RewardExplicitActionKind.Claim,
                "Reward-type relic nodes must stay on the claim lane even when only the node is exported.");
            var nodeOnlyRelicRewardAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleRewards,
                nodeOnlyRelicRewardObserver,
                Array.Empty<CombatCardKnowledgeHint>(),
                rewardRankingScreenshotPath,
                Array.Empty<GuiSmokeHistoryEntry>());
            Assert(nodeOnlyRelicRewardAllowedActions.Contains("click reward", StringComparer.OrdinalIgnoreCase)
                   && !nodeOnlyRelicRewardAllowedActions.Contains("click reward card choice", StringComparer.OrdinalIgnoreCase),
                "Node-only relic rewards should still expose reward claim actions and keep proceed secondary.");
            var nodeOnlyRelicRewardDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                50,
                GuiSmokePhase.HandleRewards.ToString(),
                "Reward-type relic nodes should claim the reward before using proceed.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1920, 1080),
                ComputeSceneSignature(rewardRankingScreenshotPath, nodeOnlyRelicRewardObserver, GuiSmokePhase.HandleRewards),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                nodeOnlyRelicRewardObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                nodeOnlyRelicRewardAllowedActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Node-only relic reward surfaces should outrank proceed in the rewards screen.",
                null));
            Assert(string.Equals(nodeOnlyRelicRewardDecision.TargetLabel, "claim reward relic", StringComparison.OrdinalIgnoreCase),
                "Reward decision should claim the reward-type relic node before any proceed fallback.");

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

            var explicitMixedRewardObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "rewards",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-explicit-mixed-reward",
                    true,
                    "mixed",
                    "stable",
                    "episode-explicit-mixed-reward",
                    "Monster",
                    "event",
                    80,
                    80,
                    null,
                    new[] { "{gold} 골드", "덱에 추가할 카드를 선택하세요.", "넘기기" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("reward-gold-node", "gold", "{gold} 골드", "758,374,402,86", true)
                        {
                            TypeName = "reward-type",
                            SemanticHints = new[] { "reward", "reward-gold", "reward-type:GoldReward" },
                        },
                        new ObserverActionNode("reward-card-node", "card", "덱에 추가할 카드를 선택하세요.", "758,470,402,86", true)
                        {
                            TypeName = "reward-type",
                            SemanticHints = new[] { "reward", "reward-card", "reward-type:CardReward" },
                        },
                    },
                    new[]
                    {
                        new ObserverChoice("gold", "{gold} 골드", "758,374,402,86", "{gold} 골드", "16 골드")
                        {
                            BindingKind = "reward-type",
                            BindingId = "GoldReward",
                            SemanticHints = new[] { "reward", "reward-gold", "reward-type:GoldReward" },
                        },
                        new ObserverChoice("card", "덱에 추가할 카드를 선택하세요.", "758,470,402,86", "CardReward", "카드 보상")
                        {
                            BindingKind = "reward-type",
                            BindingId = "CardReward",
                            SemanticHints = new[] { "reward", "reward-card", "reward-type:CardReward" },
                        },
                        new ObserverChoice("choice", "넘기기", "1583,764,269,108"),
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    Meta = new Dictionary<string, string?>
                    {
                        ["rewardScreenDetected"] = "true",
                        ["rewardScreenVisible"] = "true",
                        ["rewardForegroundOwned"] = "true",
                        ["rewardTeardownInProgress"] = "false",
                        ["rewardIsCurrentActiveScreen"] = "true",
                        ["rewardIsTopOverlay"] = "true",
                        ["rewardProceedVisible"] = "true",
                        ["rewardProceedEnabled"] = "true",
                        ["rewardVisibleButtonCount"] = "3",
                        ["rewardEnabledButtonCount"] = "3",
                        ["mapCurrentActiveScreen"] = "false",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                        ["rewardScreenRootType"] = "MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen",
                    },
                },
                null,
                null,
                null);
            var explicitMixedRewardScene = AutoDecisionProvider.BuildRewardSceneState(
                explicitMixedRewardObserver,
                new WindowBounds(0, 0, 1920, 1080),
                Array.Empty<GuiSmokeHistoryEntry>(),
                rewardRankingScreenshotPath);
            Assert(explicitMixedRewardScene.ExplicitAction == RewardExplicitActionKind.Claim
                   && explicitMixedRewardScene.ClaimSurfacePresent
                   && explicitMixedRewardScene.CardProgressionSurfacePresent,
                "Immediate gold claims must outrank reward card rows while preserving the remaining card row as secondary progression.");
            var explicitMixedRewardDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                39,
                GuiSmokePhase.HandleRewards.ToString(),
                "Immediate reward claims should outrank reward card-row progression when both are visible.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1920, 1080),
                ComputeSceneSignature(rewardRankingScreenshotPath, explicitMixedRewardObserver, GuiSmokePhase.HandleRewards),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                explicitMixedRewardObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                GetAllowedActions(GuiSmokePhase.HandleRewards, explicitMixedRewardObserver),
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Gold claims should not be delayed behind reward card-row progression when both are explicit.",
                null));
            Assert(string.Equals(explicitMixedRewardDecision.TargetLabel, "claim reward gold", StringComparison.OrdinalIgnoreCase),
                "Immediate gold rewards should be claimed before clicking reward card rows.");

            var rewardCardProgressionObserver = explicitMixedRewardObserver with
            {
                Summary = explicitMixedRewardObserver.Summary with
                {
                    CurrentChoices = new[] { "덱에 추가할 카드를 선택하세요.", "넘기기" },
                    ActionNodes = new[]
                    {
                        new ObserverActionNode("reward-card-node", "card", "덱에 추가할 카드를 선택하세요.", "758,470,402,86", true)
                        {
                            TypeName = "reward-type",
                            SemanticHints = new[] { "reward", "reward-card", "reward-type:CardReward" },
                        },
                    },
                    Choices = new[]
                    {
                        new ObserverChoice("card", "덱에 추가할 카드를 선택하세요.", "758,470,402,86", "CardReward", "카드 보상")
                        {
                            BindingKind = "reward-type",
                            BindingId = "CardReward",
                            SemanticHints = new[] { "reward", "reward-card", "reward-type:CardReward" },
                        },
                        new ObserverChoice("choice", "넘기기", "1583,764,269,108"),
                    },
                    Meta = new Dictionary<string, string?>(explicitMixedRewardObserver.Summary.Meta ?? new Dictionary<string, string?>())
                    {
                        ["choiceExtractorPath"] = "event",
                    },
                },
            };
            var rewardCardProgressionDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                40,
                GuiSmokePhase.HandleRewards.ToString(),
                "Repeated reward card clicks without subtype publish should degrade to a reward-family wait, not a repeated click.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1920, 1080),
                ComputeSceneSignature(rewardRankingScreenshotPath, rewardCardProgressionObserver, GuiSmokePhase.HandleRewards),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                rewardCardProgressionObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                GetAllowedActions(GuiSmokePhase.HandleRewards, rewardCardProgressionObserver),
                new[]
                {
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleRewards.ToString(), "click", "reward card choice", DateTimeOffset.UtcNow.AddSeconds(-2)),
                },
                "Reward aftermath card rows should wait for subtype publish instead of repeating the same click.",
                null));
            Assert(string.Equals(rewardCardProgressionDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(rewardCardProgressionDecision.Reason, "waiting for reward card progression to publish card-selection state", StringComparison.OrdinalIgnoreCase),
                "Repeated reward card rows should transition into reward-family progression wait instead of reissuing the same click.");

            var rewardPromptObserver = new ObserverState(
                new ObserverSummary(
                    "rewards",
                    "rewards",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-reward-prompt",
                    true,
                    "mixed",
                    "stable",
                    "episode-reward-prompt",
                    "Monster",
                    "reward",
                    80,
                    80,
                    null,
                    new[] { "{gold} 골드", "{gold} 골드", "덱에 추가할 카드를 선택하세요.", "넘기기" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("reward-item:0", "reward-item", "{gold} 골드", "758,374,402,86", true) { TypeName = "gold" },
                        new ObserverActionNode("reward-item:1", "reward-item", "{gold} 골드", "758,470,402,86", true) { TypeName = "gold" },
                        new ObserverActionNode("reward-item:2", "reward-item", "덱에 추가할 카드를 선택하세요.", "758,566,402,86", true)
                        {
                            TypeName = "card",
                            SemanticHints = new[] { "reward", "reward-card", "reward-type:CardReward" },
                        },
                        new ObserverActionNode("proceed:3", "proceed", "넘기기", "1583,764,269,108", true),
                    },
                    new[]
                    {
                        new ObserverChoice("gold", "{gold} 골드", "758,374,402,86", "GoldReward", "{gold} 골드")
                        {
                            BindingKind = "reward-type",
                            BindingId = "GoldReward",
                            SemanticHints = new[] { "reward", "reward-gold", "reward-type:GoldReward" },
                        },
                        new ObserverChoice("gold", "{gold} 골드", "758,470,402,86", "GoldReward", "{gold} 골드")
                        {
                            BindingKind = "reward-type",
                            BindingId = "GoldReward",
                            SemanticHints = new[] { "reward", "reward-gold", "reward-type:GoldReward" },
                        },
                        new ObserverChoice("card", "덱에 추가할 카드를 선택하세요.", "758,566,402,86", "CardReward", "카드 보상")
                        {
                            BindingKind = "reward-type",
                            BindingId = "CardReward",
                            SemanticHints = new[] { "reward", "reward-card", "reward-type:CardReward" },
                        },
                        new ObserverChoice("choice", "넘기기", "1583,764,269,108"),
                    },
                    Array.Empty<ObservedCombatHandCard>()),
                null,
                null,
                null);
            var rewardPromptHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleRewards.ToString(), "click", "claim reward gold", DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleRewards.ToString(), "click", "claim reward gold", DateTimeOffset.UtcNow.AddSeconds(-2)),
            };
            var rewardPromptDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                165,
                GuiSmokePhase.HandleRewards.ToString(),
                "After gold rewards are claimed, the remaining card reward prompt should open the card reward child screen instead of repeating a generic claim label.",
                DateTimeOffset.UtcNow,
                rewardRankingScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                ComputeSceneSignature(rewardRankingScreenshotPath, rewardPromptObserver, GuiSmokePhase.HandleRewards),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                rewardPromptObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                GetAllowedActions(GuiSmokePhase.HandleRewards, rewardPromptObserver),
                rewardPromptHistory,
                "The remaining reward is a card reward prompt, not a generic confirm button.",
                null));
            Assert(string.Equals(rewardPromptDecision.TargetLabel, "reward card choice", StringComparison.OrdinalIgnoreCase),
                "Reward card prompt labels containing '선택' should still route through the reward-card lane instead of a generic claim label.");
            Assert(!TryClassifyRewardMapLoop(
                    GuiSmokePhase.HandleRewards,
                    new GuiSmokeStepRequest(
                        "run",
                        "boot-to-long-run",
                        165,
                        GuiSmokePhase.HandleRewards.ToString(),
                        "Do not classify gold->gold->card reward progression as a reward-map-loop.",
                        DateTimeOffset.UtcNow,
                        rewardRankingScreenshotPath,
                        new WindowBounds(0, 0, 1280, 720),
                        ComputeSceneSignature(rewardRankingScreenshotPath, rewardPromptObserver, GuiSmokePhase.HandleRewards),
                        "0001",
                        1,
                        3,
                        true,
                        "tactical",
                        null,
                        rewardPromptObserver.Summary,
                        Array.Empty<KnownRecipeHint>(),
                        Array.Empty<EventKnowledgeCandidate>(),
                        Array.Empty<CombatCardKnowledgeHint>(),
                        GetAllowedActions(GuiSmokePhase.HandleRewards, rewardPromptObserver),
                        rewardPromptHistory,
                        "Different reward subtypes should not be collapsed into the same stale reward loop key.",
                        null),
                    rewardPromptDecision,
                    out _),
                "Gold claims followed by a remaining card reward prompt should not trip the reward-map-loop sentinel.");
    }
}
