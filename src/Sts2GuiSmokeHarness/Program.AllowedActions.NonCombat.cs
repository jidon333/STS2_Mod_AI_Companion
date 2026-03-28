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
        var eventOwnerActive = eventScene.EventForegroundOwned
                               && eventScene.ReleaseStage == EventReleaseStage.Active;
        var mapForegroundOwnership = MapForegroundReconciliation.HasMapForegroundOwnership(observer, history);
        var ancientMapOwner = AncientEventObserverSignals.IsMapForegroundOwner(observer.Summary);
        var ancientMapSurfacePending = AncientEventObserverSignals.IsMapSurfacePending(observer.Summary);
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
            GuiSmokePhase.EnterRun => new[] { "click continue", "click singleplayer", "click normal mode", "wait" },
            GuiSmokePhase.WaitRunLoad => new[] { "wait" },
            GuiSmokePhase.ChooseCharacter => new[] { "click ironclad", "click character confirm", "wait" },
            GuiSmokePhase.Embark => new[] { "click embark", "click character confirm", "wait" },
            GuiSmokePhase.HandleRewards
                => BuildRewardAllowedActions(observer, context),
            GuiSmokePhase.ChooseFirstNode when ancientMapOwner && ancientMapSurfacePending
                => new[] { "wait" },
            GuiSmokePhase.ChooseFirstNode when ancientMapOwner
                => BuildMapForegroundRoutingAllowedActions(),
            GuiSmokePhase.ChooseFirstNode when explicitRestSiteChoiceAuthority
                => GuiSmokeNonCombatContractSupport.BuildExplicitRestSiteAllowedActions(observer.Summary),
            GuiSmokePhase.ChooseFirstNode when GuiSmokeNonCombatContractSupport.LooksLikeRestSiteProceedState(observer.Summary)
                => new[] { "click proceed", "wait" },
            GuiSmokePhase.ChooseFirstNode when treasureState is { RoomDetected: true }
                => TreasureRoomObserverSignals.BuildAllowedActions(treasureState),
            GuiSmokePhase.ChooseFirstNode when mapOverlayState.ForegroundVisible
                => BuildMapOverlayRoutingAllowedActions(mapOverlayState),
            GuiSmokePhase.ChooseFirstNode when mapForegroundOwnership
                => BuildMapForegroundRoutingAllowedActions(),
            GuiSmokePhase.ChooseFirstNode when GuiSmokeNonCombatContractSupport.LooksLikeRestSiteState(observer.Summary)
                => new[] { "click smith card", "click smith confirm", "wait" },
            GuiSmokePhase.ChooseFirstNode => BuildMapForegroundRoutingAllowedActions(),
            GuiSmokePhase.HandleEvent when LooksLikeInspectOverlayState(observer)
                => new[] { "press escape", "click inspect overlay close", "wait" },
            GuiSmokePhase.HandleEvent when cardSelectionState is not null
                => BuildCardSelectionAllowedActions(cardSelectionState),
            GuiSmokePhase.HandleEvent when eventScene.RewardSubstateActive
                => BuildRewardAllowedActionsFromState(observer, eventScene.RewardScene),
            GuiSmokePhase.HandleEvent when eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.ReleasePending
                => new[] { "wait" },
            GuiSmokePhase.HandleEvent when ancientMapOwner && ancientMapSurfacePending
                => new[] { "wait" },
            GuiSmokePhase.HandleEvent when AncientEventObserverSignals.IsDialogueActive(observer.Summary)
                => new[] { "click ancient dialogue advance", "wait" },
            GuiSmokePhase.HandleEvent when AncientEventObserverSignals.HasExplicitCompletionAction(observer.Summary)
                => new[] { "click ancient event completion", "wait" },
            GuiSmokePhase.HandleEvent when AncientEventObserverSignals.HasExplicitOptionSelection(observer.Summary)
                => new[] { "click event choice", "wait" },
            GuiSmokePhase.HandleEvent when forceEventProgressionAfterCardSelection
                => new[] { "click event choice", "click proceed", "wait" },
            GuiSmokePhase.HandleEvent when treasureState is { RoomDetected: true }
                => TreasureRoomObserverSignals.BuildAllowedActions(treasureState),
            GuiSmokePhase.HandleEvent when mapOverlayState.ForegroundVisible && !eventScene.StrongForegroundChoice && !explicitEventProceedAuthority
                => BuildMapOverlayRoutingAllowedActions(mapOverlayState),
            GuiSmokePhase.HandleEvent when GuiSmokeNonCombatContractSupport.HasStrongMapTransitionEvidence(observer)
                                            && !forceEventProgressionAfterCardSelection
                                            && !eventOwnerActive
                => new[] { "click first reachable node", "click visible map advance", "click proceed", "wait" },
            GuiSmokePhase.HandleEvent => new[] { "click event choice", "click proceed", "wait" },
            GuiSmokePhase.WaitEventRelease => new[] { "wait" },
            GuiSmokePhase.HandleShop => BuildShopAllowedActions(observer.Summary, history),
            GuiSmokePhase.HandleCombat => GetCombatAllowedActions(observer, combatCardKnowledge, screenshotPath, history),
            _ => new[] { "wait" },
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
            return BuildCardSelectionAllowedActions(CardSelectionObserverSignals.TryGetState(observer)!);
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
            return BuildCardSelectionAllowedActions(cardSelectionState);
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
            return BuildCardSelectionAllowedActions(cardSelectionState);
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

}
