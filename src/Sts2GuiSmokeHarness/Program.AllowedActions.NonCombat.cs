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
using static GuiSmokeNonCombatAllowedActionSupport;
using static GuiSmokeStepRequestFactory;
using static ObserverScreenProvenance;

internal static partial class Program
{
    static string[] GetAllowedActions(GuiSmokePhase phase, ObserverState observer)
    {
        return BuildAllowedActions(phase, observer, Array.Empty<CombatCardKnowledgeHint>(), null, Array.Empty<GuiSmokeHistoryEntry>());
    }

    static string[] BuildAllowedActions(
        GuiSmokePhase phase,
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return BuildAllowedActionsCore(phase, observer, combatCardKnowledge, screenshotPath, history, null);
    }

    internal static string[] BuildAllowedActionsCore(
        GuiSmokePhase phase,
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        GuiSmokeStepAnalysisContext? analysisContext)
    {
        var context = analysisContext ?? CreateStepAnalysisContext(phase, observer, screenshotPath, history, combatCardKnowledge);
        if (context.UseCombatFastPath)
        {
            return GetCombatAllowedActions(observer, combatCardKnowledge, screenshotPath, history, context);
        }

        if (phase == GuiSmokePhase.HandleRewards && context.UseRewardFastPath)
        {
            return BuildRewardAllowedActionsFastPath(observer, context);
        }

        var cardSelectionState = CardSelectionObserverSignals.TryGetState(observer.Summary);
        var treasureState = TreasureRoomObserverSignals.TryGetState(observer.Summary);
        var mapOverlayState = context.MapOverlayState;
        var eventScene = context.EventScene;
        var forceEventProgressionAfterCardSelection = eventScene.ForceProgressionAfterCardSelection;
        var explicitEventProceedAuthority = eventScene.ExplicitProceedVisible;
        var explicitEventRecoveryAuthority = AutoDecisionProvider.HasExplicitEventRecoveryAuthority(observer, context.WindowBounds, history, eventScene);
        var ancientContract = eventScene.AncientContract;
        var eventOwnerActive = eventScene.EventForegroundOwned
                               && eventScene.ReleaseStage == EventReleaseStage.Active;
        var ancientMapOwner = AncientEventObserverSignals.IsMapForegroundOwner(observer.Summary);
        var ancientMapSurfacePending = AncientEventObserverSignals.IsMapSurfacePending(observer.Summary);
        var restSiteScene = AutoDecisionProvider.BuildRestSiteSceneState(observer.Summary);
        var explicitRestSiteChoiceAuthority = GuiSmokeNonCombatContractSupport.HasExplicitRestSiteChoiceAuthority(observer, screenshotPath);
        if (phase == GuiSmokePhase.WaitRunLoad && GuiSmokeObserverPhaseHeuristics.TryGetPostRunLoadPhase(observer, out var postRunLoadPhase))
        {
            return BuildAllowedActionsCore(postRunLoadPhase, observer, combatCardKnowledge, screenshotPath, history, context);
        }

        if (phase == GuiSmokePhase.WaitRunLoad && WaitRunLoadRecoverySignals.ShouldRetryEnterRunFromWaitRunLoad(observer.Summary))
        {
            return BuildAllowedActionsCore(GuiSmokePhase.EnterRun, observer, combatCardKnowledge, screenshotPath, history, context);
        }

        if (phase == GuiSmokePhase.Embark && GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(observer, out var observedPhase))
        {
            return BuildAllowedActionsCore(observedPhase, observer, combatCardKnowledge, screenshotPath, history, context);
        }

        return phase switch
        {
            GuiSmokePhase.EnterRun => BuildEnterRunAllowedActions(observer.Summary),
            GuiSmokePhase.WaitRunLoad => new[] { "wait" },
            GuiSmokePhase.ChooseCharacter => new[] { "click ironclad", "click character confirm", "wait" },
            GuiSmokePhase.Embark => new[] { "click embark", "click character confirm", "wait" },
            GuiSmokePhase.HandleRewards when ShopObserverSignals.IsShopAuthorityActive(observer.Summary)
                => BuildShopAllowedActions(observer.Summary, history),
            GuiSmokePhase.HandleRewards when restSiteScene is { SelectionSettling: true }
                => new[] { "wait" },
            GuiSmokePhase.HandleRewards when explicitRestSiteChoiceAuthority
                => GuiSmokeNonCombatContractSupport.BuildExplicitRestSiteAllowedActions(observer.Summary),
            GuiSmokePhase.HandleRewards when restSiteScene is { SmithUpgradeActive: true }
                => new[] { "click smith card", "click smith confirm", "wait" },
            GuiSmokePhase.HandleRewards when GuiSmokeNonCombatContractSupport.LooksLikeRestSiteProceedState(observer.Summary)
                => new[] { "click proceed", "wait" },
            GuiSmokePhase.HandleRewards
                => BuildRewardAllowedActions(observer, context),
            GuiSmokePhase.ChooseFirstNode
                => BuildChooseFirstNodeAllowedActions(observer, combatCardKnowledge, screenshotPath, history, context),
            GuiSmokePhase.HandleEvent when LooksLikeInspectOverlayState(observer)
                => new[] { "press escape", "click inspect overlay close", "wait" },
            GuiSmokePhase.HandleEvent when cardSelectionState is not null
                => BuildCardSelectionAllowedActions(observer.Summary, cardSelectionState),
            GuiSmokePhase.HandleEvent when eventScene.RewardSubstateActive
                => BuildRewardAllowedActionsFromState(observer, eventScene.RewardScene),
            GuiSmokePhase.HandleEvent when eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.ReleasePending
                => new[] { "wait" },
            GuiSmokePhase.HandleEvent when ancientMapOwner && ancientMapSurfacePending
                => new[] { "wait" },
            GuiSmokePhase.HandleEvent when ancientContract.HasExplicitDialogueSurface
                => new[] { "click ancient dialogue advance", "wait" },
            GuiSmokePhase.HandleEvent when ancientContract.HasExplicitCompletionSurface
                => new[] { "click ancient event completion", "wait" },
            GuiSmokePhase.HandleEvent when ancientContract.HasLaneSurfaceMismatch
                => ancientContract.HasSameFamilyReconciliationSurface
                    ? new[] { "click event choice", "wait" }
                    : new[] { "wait" },
            GuiSmokePhase.HandleEvent when ancientContract.HasExplicitOptionSurface
                => new[] { "click event choice", "wait" },
            GuiSmokePhase.HandleEvent when forceEventProgressionAfterCardSelection
                => new[] { "click event choice", "click proceed", "wait" },
            GuiSmokePhase.HandleEvent when eventOwnerActive || explicitEventRecoveryAuthority
                => explicitEventProceedAuthority
                    ? new[] { "click event choice", "click proceed", "wait" }
                    : new[] { "click event choice", "wait" },
            GuiSmokePhase.HandleEvent when treasureState is { RoomDetected: true }
                => TreasureRoomObserverSignals.BuildAllowedActions(treasureState),
            GuiSmokePhase.HandleEvent when mapOverlayState.ForegroundVisible && !eventScene.StrongForegroundChoice && !explicitEventProceedAuthority && !explicitEventRecoveryAuthority
                => BuildMapOverlayRoutingAllowedActions(mapOverlayState),
            GuiSmokePhase.HandleEvent when (MapForegroundReconciliation.HasMapForegroundOwnership(observer, history) || NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(observer))
                                            && !forceEventProgressionAfterCardSelection
                                            && !eventOwnerActive
                                            && !explicitEventProceedAuthority
                                            && !explicitEventRecoveryAuthority
                => BuildMapForegroundRoutingAllowedActions(),
            GuiSmokePhase.HandleEvent when GuiSmokeNonCombatContractSupport.HasStrongMapTransitionEvidence(observer)
                                            && !forceEventProgressionAfterCardSelection
                                            && !eventOwnerActive
                => BuildMapForegroundRoutingAllowedActions(),
            GuiSmokePhase.HandleEvent => new[] { "wait" },
            GuiSmokePhase.WaitEventRelease => new[] { "wait" },
            GuiSmokePhase.HandleShop => BuildShopAllowedActions(observer.Summary, history),
            GuiSmokePhase.HandleCombat => GetCombatAllowedActions(observer, combatCardKnowledge, screenshotPath, history),
            _ => new[] { "wait" },
        };
    }

    static string[] BuildChooseFirstNodeAllowedActions(
        ObserverState observer,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        GuiSmokeStepAnalysisContext context)
    {
        return GuiSmokeChooseFirstNodeLaneSupport.Resolve(observer, context.WindowBounds, screenshotPath, history, context) switch
        {
            GuiSmokeChooseFirstNodeLane.AncientMapPending => new[] { "wait" },
            GuiSmokeChooseFirstNodeLane.AncientMapForeground => BuildMapForegroundRoutingAllowedActions(),
            GuiSmokeChooseFirstNodeLane.RestSiteExplicitChoice => GuiSmokeNonCombatContractSupport.BuildExplicitRestSiteAllowedActions(observer.Summary),
            GuiSmokeChooseFirstNodeLane.RestSiteSmithUpgrade => new[] { "click smith card", "click smith confirm", "wait" },
            GuiSmokeChooseFirstNodeLane.RestSiteProceed => new[] { "click proceed", "wait" },
            GuiSmokeChooseFirstNodeLane.RestSiteSelectionSettling => new[] { "wait" },
            GuiSmokeChooseFirstNodeLane.TreasureRoom => TreasureRoomObserverSignals.BuildAllowedActions(TreasureRoomObserverSignals.TryGetState(observer.Summary)!),
            GuiSmokeChooseFirstNodeLane.ShopRoom => BuildShopAllowedActions(observer.Summary, history),
            GuiSmokeChooseFirstNodeLane.EventRecovery => BuildAllowedActionsCore(GuiSmokePhase.HandleEvent, observer, combatCardKnowledge, screenshotPath, history, context),
            GuiSmokeChooseFirstNodeLane.MapOverlay => BuildMapOverlayRoutingAllowedActions(context.MapOverlayState),
            _ => BuildMapForegroundRoutingAllowedActions(),
        };
    }

    static string[] BuildShopAllowedActions(ObserverSummary observer, IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        var state = ShopObserverSignals.TryGetState(observer);
        if (state is null)
        {
            return new[] { "wait" };
        }

        if (LooksLikeInspectOverlayState(new ObserverState(observer, null, null, null)))
        {
            return new[] { "press escape", "click inspect overlay close", "wait" };
        }

        if (CardSelectionObserverSignals.TryGetState(observer) is not null)
        {
            return BuildCardSelectionAllowedActions(observer, CardSelectionObserverSignals.TryGetState(observer)!);
        }

        return ShopObserverSignals.BuildAllowedActions(observer, state, alreadyPurchased: ShopObserverSignals.HasRecentPurchase(history));
    }

    static string[] BuildRewardAllowedActionsFastPath(ObserverState observer, GuiSmokeStepAnalysisContext context)
    {
        return BuildRewardAllowedActions(observer, context);
    }

    static string[] BuildRewardAllowedActions(ObserverState observer, GuiSmokeStepAnalysisContext context)
    {
        if (LooksLikeInspectOverlayState(observer))
        {
            return new[] { "press escape", "click inspect overlay close", "wait" };
        }

        var cardSelectionState = CardSelectionObserverSignals.TryGetState(observer.Summary);
        if (cardSelectionState is not null)
        {
            return BuildCardSelectionAllowedActions(observer.Summary, cardSelectionState);
        }

        var rewardScene = context.RewardScene;
        return BuildRewardAllowedActionsFromState(observer, rewardScene);
    }

    static string[] BuildRewardAllowedActionsFromState(ObserverState observer, RewardSceneState rewardScene)
    {
        if (LooksLikeInspectOverlayState(observer))
        {
            return new[] { "press escape", "click inspect overlay close", "wait" };
        }

        var cardSelectionState = CardSelectionObserverSignals.TryGetState(observer.Summary);
        if (cardSelectionState is not null)
        {
            return BuildCardSelectionAllowedActions(observer.Summary, cardSelectionState);
        }

        if (rewardScene.LayerState.TerminalRunBoundary || rewardScene.ReleaseToMapPending)
        {
            return new[] { "wait" };
        }

        return rewardScene.ExplicitAction switch
        {
            RewardExplicitActionKind.ColorlessChoice
                => new[] { "click colorless card choice", "click reward skip", "click proceed", "press escape", "wait" },
            RewardExplicitActionKind.CardChoice
                => new[] { "click reward card choice", "click reward choice", "click reward skip", "click proceed", rewardScene.LayerState.RewardBackNavigationAvailable ? "click reward back" : "press escape", "wait" },
            RewardExplicitActionKind.Claim
                => rewardScene.RewardChoiceVisible
                    ? new[] { "click reward card choice", "click reward choice", "click reward skip", "click proceed", rewardScene.LayerState.RewardBackNavigationAvailable ? "click reward back" : "press escape", "wait" }
                    : new[] { "click reward", "click reward skip", "click proceed", "wait" },
            RewardExplicitActionKind.SkipProceed
                => new[] { "click reward skip", "click proceed", "wait" },
            RewardExplicitActionKind.Back
                => new[] { "click reward back", "wait" },
            _ => new[] { "wait" },
        };
    }

    static string[] BuildEnterRunAllowedActions(ObserverSummary observer)
    {
        if (MainMenuRunStartObserverSignals.HasAbandonRunConfirmSurface(observer))
        {
            return new[] { "click confirm abandon run", "click cancel abandon run", "wait" };
        }

        if (MainMenuRunStartObserverSignals.HasRunSaveCleanupSurface(observer))
        {
            return new[] { "click abandon run", "wait" };
        }

        if (MatchesControlFlowScreen(observer, "singleplayer-submenu"))
        {
            return new[] { "click normal mode", "wait" };
        }

        return new[] { "click continue", "click singleplayer", "click normal mode", "wait" };
    }

}
