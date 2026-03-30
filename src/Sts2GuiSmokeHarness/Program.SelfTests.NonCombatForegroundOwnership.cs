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
using static GuiSmokeReplayArtifactSupport;
using static GuiSmokeSceneReasoningSupport;
using static GuiSmokeStepRequestFactory;

internal static partial class Program
{
    private static void RunNonCombatForegroundOwnershipSelfTests()
    {
        var mapTransitionScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-map-transition-self-test-{Guid.NewGuid():N}.png");
        try
        {
            using (var bitmap = new Bitmap(1280, 720))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var backgroundBrush = new SolidBrush(Color.FromArgb(194, 168, 125)))
            using (var arrowBrush = new SolidBrush(Color.FromArgb(220, 60, 55)))
            {
                graphics.Clear(Color.Black);
                graphics.FillRectangle(backgroundBrush, new Rectangle(200, 40, 880, 620));
                graphics.FillEllipse(arrowBrush, new Rectangle(590, 455, 100, 80));
                bitmap.Save(mapTransitionScreenshotPath, ImageFormat.Png);
            }

            using var mapTransitionStateDocument = JsonDocument.Parse("""{"meta":{"declaringType":"MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen","instanceType":"MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen"}}""");
            var mapTransitionObserver = new ObserverState(
                new ObserverSummary(
                    "event",
                    "event",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-map-transition",
                    true,
                    "mixed",
                    "stable",
                    "episode-map-transition",
                    "None",
                    "event",
                    80,
                    80,
                    null,
                    new[] { "진행" },
                    new[] { "screen-changed: map", "map-point-selected" },
                    new[] { new ObserverActionNode("event-option:0", "event-option", "진행", "460,942,1000,100", true) },
                    new[] { new ObserverChoice("choice", "진행", "460,942,1000,100", "진행") },
                    Array.Empty<ObservedCombatHandCard>()),
                mapTransitionStateDocument,
                null,
                null);
            Assert(GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(mapTransitionObserver, out var postMapPhase) && postMapPhase == GuiSmokePhase.ChooseFirstNode, "Map-screen authority should win over stale event labels when observer meta already points at NMapScreen.");
            var mapTransitionAllowedActions = GetAllowedActions(GuiSmokePhase.HandleEvent, mapTransitionObserver);
            var mapTransitionEventScene = AutoDecisionProvider.BuildEventSceneState(mapTransitionObserver, null);
            var mapTransitionEventRecovery = AutoDecisionProvider.HasExplicitEventRecoveryAuthority(mapTransitionObserver, null, Array.Empty<GuiSmokeHistoryEntry>(), mapTransitionEventScene);
            var mapTransitionMapOwner = NonCombatForegroundOwnership.HasExplicitMapForegroundAuthority(mapTransitionObserver);
            Assert(mapTransitionAllowedActions.Contains("click visible map advance", StringComparer.OrdinalIgnoreCase),
                $"Event allowlist should open map affordances when map transition evidence is stronger than the stale event screen. Actual allowlist=[{string.Join(", ", mapTransitionAllowedActions)}] eventOwner={mapTransitionEventScene.EventForegroundOwned} release={mapTransitionEventScene.ReleaseStage} explicitProceed={mapTransitionEventScene.ExplicitProceedVisible} explicitProgress={mapTransitionEventScene.HasExplicitProgression} mapOwner={mapTransitionMapOwner} explicitEventRecovery={mapTransitionEventRecovery}.");
            var mapTransitionWaitObserver = new ObserverState(
                mapTransitionObserver.Summary with
                {
                    PublishedCurrentScreen = "event",
                    PublishedVisibleScreen = "event",
                    PublishedSceneReady = true,
                    PublishedSceneAuthority = "hook",
                    PublishedSceneStability = "stable",
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["transitionInProgress"] = "true",
                        ["rootSceneIsRun"] = "true",
                        ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.NRun",
                        ["mapCurrentActiveScreen"] = "true",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    },
                },
                mapTransitionStateDocument,
                null,
                null);
            var mapTransitionWaitDecision = InvokeForegroundAwareNonCombatWaitDecision(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                41,
                GuiSmokePhase.ChooseFirstNode.ToString(),
                "Wait during an explicit map transition boundary.",
                DateTimeOffset.UtcNow,
                mapTransitionScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                ComputeSceneSignature(mapTransitionScreenshotPath, mapTransitionWaitObserver, GuiSmokePhase.ChooseFirstNode),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                mapTransitionWaitObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                BuildAllowedActions(GuiSmokePhase.ChooseFirstNode, mapTransitionWaitObserver, Array.Empty<CombatCardKnowledgeHint>(), mapTransitionScreenshotPath, Array.Empty<GuiSmokeHistoryEntry>()),
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Explicit transition truth should keep noncombat waits conservative.",
                null),
                "waiting for explicit map transition completion");
            Assert(mapTransitionWaitDecision.WaitMs == 2000, "Explicit transition/loading truth should keep noncombat waits at the conservative duration.");
        }
        finally
        {
            if (File.Exists(mapTransitionScreenshotPath))
            {
                File.Delete(mapTransitionScreenshotPath);
            }
        }

        var eventContaminationScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-event-contamination-self-test-{Guid.NewGuid():N}.png");
        try
        {
            using (var bitmap = new Bitmap(1280, 720))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var panelBrush = new SolidBrush(Color.FromArgb(90, 64, 42)))
            using (var choiceBrush = new SolidBrush(Color.FromArgb(215, 188, 124)))
            using (var arrowBrush = new SolidBrush(Color.FromArgb(220, 60, 55)))
            {
                graphics.Clear(Color.Black);
                graphics.FillRectangle(panelBrush, new Rectangle(720, 150, 470, 420));
                graphics.FillRectangle(choiceBrush, new Rectangle(880, 430, 280, 66));
                graphics.FillRectangle(choiceBrush, new Rectangle(880, 520, 280, 66));
                graphics.FillEllipse(arrowBrush, new Rectangle(590, 452, 94, 74));
                bitmap.Save(eventContaminationScreenshotPath, ImageFormat.Png);
            }

            using var eventStateDocument = JsonDocument.Parse("""{"meta":{"declaringType":"MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom","instanceType":"MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom"}}""");
            var eventObserverState = new ObserverState(
                new ObserverSummary(
                    "event",
                    "event",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-event-foreground",
                    true,
                    "mixed",
                    "stable",
                    "episode-event-foreground",
                    "None",
                    "event",
                    80,
                    80,
                    null,
                    new[] { "그래도 휴식한다", "나무들을 베어낸다" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("event-option:0", "event-option", "그래도 휴식한다", "922,596,800,100", true),
                        new ObserverActionNode("event-option:1", "event-option", "나무들을 베어낸다", "922,700,800,100", true),
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "그래도 휴식한다", "922,596,800,100", "그래도 휴식한다"),
                        new ObserverChoice("choice", "나무들을 베어낸다", "922,700,800,100", "나무들을 베어낸다"),
                    },
                    Array.Empty<ObservedCombatHandCard>()),
                eventStateDocument,
                null,
                null);
            var eventSceneSignature = ComputeSceneSignature(eventContaminationScreenshotPath, eventObserverState, GuiSmokePhase.HandleEvent);
            Assert(eventSceneSignature.Contains("layer:event-foreground", StringComparison.OrdinalIgnoreCase), "Explicit event choices should mark the event as foreground-authoritative.");
            Assert(eventSceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase), "Background map arrows on event scenes should be tagged as contamination.");
            Assert(!eventSceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase), "Event foreground authority should suppress visible map advance tags.");
            Assert(!GetAllowedActions(GuiSmokePhase.HandleEvent, eventObserverState).Contains("click visible map advance", StringComparer.OrdinalIgnoreCase), "HandleEvent allowlist should not expose map fallback when explicit event options are visible.");
            var eventDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                43,
                GuiSmokePhase.HandleEvent.ToString(),
                "Resolve the event choice.",
                DateTimeOffset.UtcNow,
                eventContaminationScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                eventSceneSignature,
                "0001",
                1,
                3,
                true,
                "semantic",
                null,
                eventObserverState.Summary,
                Array.Empty<KnownRecipeHint>(),
                new[]
                {
                    new EventKnowledgeCandidate(
                        "uneasy-rest-site",
                        "불안한 휴식 장소",
                        "self-test event foreground",
                        new[]
                        {
                            new EventOptionKnowledgeCandidate("그래도 휴식한다", "휴식을 시도한다.", "rest"),
                            new EventOptionKnowledgeCandidate("나무들을 베어낸다", "나무를 베어 길을 연다.", "chop"),
                        }),
                },
                Array.Empty<CombatCardKnowledgeHint>(),
                GetAllowedActions(GuiSmokePhase.HandleEvent, eventObserverState),
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Explicit event owner should preserve the room lane over background map contamination.",
                null));
            Assert(eventDecision.TargetLabel is not null && eventDecision.TargetLabel.Contains("event", StringComparison.OrdinalIgnoreCase), "Event decisioning should click an explicit event option instead of a map fallback when NEventRoom authority is present.");
            Assert(string.Equals(DetermineReasoningMode(GuiSmokePhase.HandleEvent, eventObserverState, firstSeenScene: true), "semantic", StringComparison.OrdinalIgnoreCase), "Ambiguous event scenes should stay in semantic reasoning mode until ownership becomes explicit.");

            using var eventProceedStateDocument = JsonDocument.Parse("""{"meta":{"declaringType":"MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom","instanceType":"MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom","choiceExtractorPath":"event","mapOverlayVisible":"true"}}""");
            var explicitProceedObserverState = new ObserverState(
                new ObserverSummary(
                    "event",
                    "event",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-event-proceed",
                    true,
                    "mixed",
                    "stable",
                    "episode-event-proceed",
                    "None",
                    "event",
                    80,
                    80,
                    null,
                    new[] { "계속", "계속", "휴식 (1,2)" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("event-option:0", "event-option", "계속", "918,595.5,808,101", true)
                        {
                            SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option-button", "option-role:proceed", "event-proceed" },
                        },
                    },
                    new[]
                    {
                        new ObserverChoice("event-option", "계속", "918,595.5,808,101", "PROCEED", "[gold][b]계속[/b][/gold]")
                        {
                            NodeId = "event-option:0",
                            BindingKind = "event-option",
                            BindingId = "PROCEED",
                            Enabled = true,
                            SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option-button", "option-role:proceed", "event-proceed" },
                        },
                        new ObserverChoice("map-node", "휴식 (1,2)", "897,581,124,124", "1,2", "type:Rest;coord:1,2")
                        {
                            NodeId = "map:1:2",
                            Enabled = true,
                        },
                    },
                    Array.Empty<ObservedCombatHandCard>())
	                {
	                    PublishedCurrentScreen = "event",
	                    PublishedVisibleScreen = "event",
	                    PublishedSceneReady = true,
	                    PublishedSceneAuthority = "hook",
	                    PublishedSceneStability = "stable",
	                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
	                    {
                        ["choiceExtractorPath"] = "event",
                        ["eventProceedOptionVisible"] = "true",
                        ["eventProceedOptionEnabled"] = "true",
                        ["eventProceedOptionCount"] = "1",
                    },
                },
                eventProceedStateDocument,
                null,
                null);
            Assert(string.Equals(DetermineReasoningMode(GuiSmokePhase.HandleEvent, explicitProceedObserverState, firstSeenScene: true), "tactical", StringComparison.OrdinalIgnoreCase), "Explicit event proceed ownership should collapse HandleEvent reasoning to tactical mode.");
            var explicitProceedAllowedActions = GetAllowedActions(GuiSmokePhase.HandleEvent, explicitProceedObserverState);
            Assert(explicitProceedAllowedActions.Contains("click proceed", StringComparer.OrdinalIgnoreCase)
                   && !explicitProceedAllowedActions.Contains("click exported reachable node", StringComparer.OrdinalIgnoreCase)
                   && !explicitProceedAllowedActions.Contains("click first reachable node", StringComparer.OrdinalIgnoreCase),
                "Explicit event proceed should keep HandleEvent on the event progression lane instead of collapsing to map-only fallback.");
            var explicitProceedDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                44,
                GuiSmokePhase.HandleEvent.ToString(),
                "Resolve the event follow-up.",
                DateTimeOffset.UtcNow,
                eventContaminationScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                ComputeSceneSignature(eventContaminationScreenshotPath, explicitProceedObserverState, GuiSmokePhase.HandleEvent),
                "0001",
                1,
                3,
                true,
                "semantic",
                null,
                explicitProceedObserverState.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                explicitProceedAllowedActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Explicit event proceed should beat stale map overlay evidence.",
                null));
            Assert(
                string.Equals(explicitProceedDecision.Status, "act", StringComparison.OrdinalIgnoreCase)
                && string.Equals(explicitProceedDecision.ActionKind, "click", StringComparison.OrdinalIgnoreCase)
                && string.Equals(explicitProceedDecision.TargetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase),
                $"HandleEvent should click the explicit event proceed lane instead of waiting when EventOption.IsProceed is exported. Actual decision={explicitProceedDecision.Status}/{explicitProceedDecision.ActionKind}/{explicitProceedDecision.TargetLabel ?? "null"}.");
            var chooseFirstNodeEventRecoveryContext = CreateObserverOnlyAnalysisContext(
                GuiSmokePhase.ChooseFirstNode,
                explicitProceedObserverState,
                Array.Empty<GuiSmokeHistoryEntry>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                new WindowBounds(0, 0, 1280, 720));
            var chooseFirstNodeEventRecoveryPolicy = GuiSmokeStepScreenshotPolicy.Evaluate(45, false, chooseFirstNodeEventRecoveryContext);
            Assert(!chooseFirstNodeEventRecoveryPolicy.NeedsScreenshot
                   && string.Equals(chooseFirstNodeEventRecoveryPolicy.SkipReason, "event-explicit-authority", StringComparison.OrdinalIgnoreCase),
                "ChooseFirstNode should stay observer-only when explicit event proceed authority is already exported.");
            var chooseFirstNodeEventRecoveryActions = BuildAllowedActions(
                GuiSmokePhase.ChooseFirstNode,
                explicitProceedObserverState,
                Array.Empty<CombatCardKnowledgeHint>(),
                string.Empty,
                Array.Empty<GuiSmokeHistoryEntry>());
            Assert(chooseFirstNodeEventRecoveryActions.Contains("click proceed", StringComparer.OrdinalIgnoreCase)
                   && !chooseFirstNodeEventRecoveryActions.Contains("click exported reachable node", StringComparer.OrdinalIgnoreCase),
                "ChooseFirstNode should reopen the event lane instead of map routing when explicit event authority is still foreground-owned.");
            var chooseFirstNodeEventRecoveryDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                45,
                GuiSmokePhase.ChooseFirstNode.ToString(),
                "Recover explicit event authority from a lagging ChooseFirstNode phase.",
                DateTimeOffset.UtcNow,
                string.Empty,
                new WindowBounds(0, 0, 1280, 720),
                "phase:choosefirstnode|screen:event|visible:event|encounter:none|ready:true|stability:stable|layer:event-foreground",
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                explicitProceedObserverState.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                chooseFirstNodeEventRecoveryActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "ChooseFirstNode should recover the explicit event lane without screenshot enrichment.",
                null));
            Assert(string.Equals(chooseFirstNodeEventRecoveryDecision.TargetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase),
                $"ChooseFirstNode event recovery should click the explicit event proceed affordance instead of waiting for map routing. Actual status={chooseFirstNodeEventRecoveryDecision.Status}, action={chooseFirstNodeEventRecoveryDecision.ActionKind}, target={chooseFirstNodeEventRecoveryDecision.TargetLabel ?? "<null>"}.");
            var chooseFirstNodeEventRecoveryBranchRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-event-recovery-{Guid.NewGuid():N}");
            Directory.CreateDirectory(chooseFirstNodeEventRecoveryBranchRoot);
            try
            {
                var chooseFirstNodeEventRecoveryLogger = new ArtifactRecorder(chooseFirstNodeEventRecoveryBranchRoot);
                var chooseFirstNodeEventRecoveryHistory = new List<GuiSmokeHistoryEntry>();
                Assert(
                    TryAdvanceAlternateBranch(
                        GuiSmokePhase.ChooseFirstNode,
                        explicitProceedObserverState,
                        chooseFirstNodeEventRecoveryHistory,
                        chooseFirstNodeEventRecoveryLogger,
                        45,
                        true,
                        out var chooseFirstNodeEventRecoveryPhase)
                    && chooseFirstNodeEventRecoveryPhase == GuiSmokePhase.HandleEvent,
                    "ChooseFirstNode should preflight-branch back to HandleEvent when explicit event authority is still active.");
            }
            finally
            {
                try
                {
                    Directory.Delete(chooseFirstNodeEventRecoveryBranchRoot, true);
                }
                catch
                {
                }
            }
            var explicitProceedWaitDecision = InvokeForegroundAwareNonCombatWaitDecision(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                45,
                GuiSmokePhase.HandleEvent.ToString(),
                "Wait on explicit event proceed ownership.",
                DateTimeOffset.UtcNow,
                eventContaminationScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                ComputeSceneSignature(eventContaminationScreenshotPath, explicitProceedObserverState, GuiSmokePhase.HandleEvent),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                explicitProceedObserverState.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                explicitProceedAllowedActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Explicit event proceed should use the reduced noncombat wait.",
                null),
                "waiting for explicit event progression choice");
            Assert(explicitProceedWaitDecision.WaitMs == 400, "Stable explicit event proceed ownership should use the faster noncombat wait.");

            var explicitProceedReleaseHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleEvent.ToString(), "click", "visible proceed", DateTimeOffset.UtcNow.AddMilliseconds(-200)),
            };
            var explicitProceedReleaseState = AutoDecisionProvider.BuildEventSceneState(
                explicitProceedObserverState,
                new WindowBounds(0, 0, 1280, 720),
                explicitProceedReleaseHistory,
                eventContaminationScreenshotPath);
            Assert(explicitProceedReleaseState.ReleaseStage == EventReleaseStage.ReleasePending,
                "A fresh explicit event proceed on the same authority band should enter event release-pending.");
            Assert(explicitProceedReleaseState.SuppressSameProceedReissue,
                "Event release-pending should suppress same proceed reissue until ownership changes.");
            Assert(((ICanonicalNonCombatSceneState)explicitProceedReleaseState).HandoffTarget == NonCombatHandoffTarget.WaitEventRelease,
                "Canonical event contract should expose WaitEventRelease while explicit proceed is still release-pending.");
            Assert(BuildAllowedActions(
                GuiSmokePhase.HandleEvent,
                explicitProceedObserverState,
                Array.Empty<CombatCardKnowledgeHint>(),
                eventContaminationScreenshotPath,
                explicitProceedReleaseHistory).SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase),
                "Event release-pending should collapse the HandleEvent allowlist to wait instead of reopening proceed.");
            var explicitProceedReissueDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                46,
                GuiSmokePhase.HandleEvent.ToString(),
                "Do not reissue explicit event proceed on the same authority band.",
                DateTimeOffset.UtcNow,
                eventContaminationScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                ComputeSceneSignature(eventContaminationScreenshotPath, explicitProceedObserverState, GuiSmokePhase.HandleEvent),
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                explicitProceedObserverState.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                new[] { "wait" },
                explicitProceedReleaseHistory,
                "Explicit event proceed release-pending should wait for release instead of reissuing proceed.",
                null));
            Assert(string.Equals(explicitProceedReissueDecision.Status, "wait", StringComparison.OrdinalIgnoreCase),
                "HandleEvent should wait instead of reissuing the same explicit proceed on the same authority band.");
        }
        finally
        {
            if (File.Exists(eventContaminationScreenshotPath))
            {
                File.Delete(eventContaminationScreenshotPath);
            }
        }

        using (var choiceMetadataDocument = JsonDocument.Parse(
                   """
                   {
                     "currentChoices": [
                       {
                         "kind": "rest-option",
                         "label": "Smith",
                         "value": "SMITH",
                         "description": "Upgrade a card.",
                         "nodeId": "rest-site:SMITH",
                         "screenBounds": "820,260,200,110",
                         "bindingKind": "rest-site-option",
                         "bindingId": "SMITH",
                         "enabled": true,
                         "iconAssetPath": "res://smith.png",
                         "semanticHints": [
                           "scene:rest-site",
                           "option-id:SMITH",
                           "source:button"
                         ]
                       }
                     ]
                   }
                   """))
        {
            var parsedMetadataChoices = ObserverSnapshotReader.ParseChoicesForTesting(choiceMetadataDocument);
            Assert(parsedMetadataChoices.Count == 1
                   && string.Equals(parsedMetadataChoices[0].NodeId, "rest-site:SMITH", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(parsedMetadataChoices[0].BindingKind, "rest-site-option", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(parsedMetadataChoices[0].BindingId, "SMITH", StringComparison.OrdinalIgnoreCase)
                   && parsedMetadataChoices[0].Enabled == true
                   && string.Equals(parsedMetadataChoices[0].IconAssetPath, "res://smith.png", StringComparison.OrdinalIgnoreCase)
                   && parsedMetadataChoices[0].SemanticHints.Contains("option-id:SMITH", StringComparer.OrdinalIgnoreCase),
                "Observer choice reader should preserve rest-site binding metadata and semantic hints.");
        }

        using (var legacyChoiceDocument = JsonDocument.Parse(
                   """
                   {
                     "currentChoices": [
                       {
                         "kind": "choice",
                         "label": "Smith",
                         "value": "smith",
                         "screenBounds": "820,260,200,110"
                       }
                     ]
                   }
                   """))
        {
            var parsedLegacyChoices = ObserverSnapshotReader.ParseChoicesForTesting(legacyChoiceDocument);
            Assert(parsedLegacyChoices.Count == 1
                   && parsedLegacyChoices[0].BindingId is null
                   && string.Equals(parsedLegacyChoices[0].Label, "Smith", StringComparison.OrdinalIgnoreCase),
                "Observer choice reader should keep old artifacts readable when choice metadata fields are absent.");
        }

        using (var restSiteMetaDocument = JsonDocument.Parse(
                   """
                   {
                     "meta": {
                       "restSiteSelectionLastSignal": "after-select-failure",
                       "restSiteSelectionLastOptionId": "SMITH",
                       "restSiteSelectionCurrentStatus": "explicit-choice",
                       "restSiteUpgradeScreenVisible": "false"
                     }
                   }
                   """))
        {
            var parsedMeta = ObserverSnapshotReader.ParseMetaForTesting(restSiteMetaDocument);
            Assert(string.Equals(parsedMeta["restSiteSelectionLastSignal"], "after-select-failure", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(parsedMeta["restSiteSelectionLastOptionId"], "SMITH", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(parsedMeta["restSiteSelectionCurrentStatus"], "explicit-choice", StringComparison.OrdinalIgnoreCase),
                "Observer snapshot reader should preserve rest-site transition metadata from snapshot meta.");
        }

        var restSiteMetadataScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-rest-site-metadata-self-test-{Guid.NewGuid():N}.png");
        try
        {
            using (var bitmap = new Bitmap(1280, 720))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Black);
                bitmap.Save(restSiteMetadataScreenshotPath, ImageFormat.Png);
            }

            var restSiteMetadataSummary = new ObserverSummary(
                "map",
                "map",
                false,
                DateTimeOffset.UtcNow,
                "inv-rest-site",
                true,
                "mixed",
                "stable",
                "episode-rest-site",
                "RestSite",
                "rest",
                70,
                80,
                null,
                new[] { "Rest", "Smith", "Hatch", "Mend" },
                Array.Empty<string>(),
                Array.Empty<ObserverActionNode>(),
                new[]
                {
                    new ObserverChoice("rest-option", "Rest", "520,260,200,110", "HEAL", "Recover HP")
                    {
                        NodeId = "rest-site:HEAL",
                        BindingKind = "rest-site-option",
                        BindingId = "HEAL",
                        Enabled = true,
                        SemanticHints = new[] { "scene:rest-site", "option-id:HEAL", "source:button" },
                    },
                    new ObserverChoice("rest-option", "Smith", "820,260,200,110", "SMITH", "Upgrade a card")
                    {
                        NodeId = "rest-site:SMITH",
                        BindingKind = "rest-site-option",
                        BindingId = "SMITH",
                        Enabled = true,
                        IconAssetPath = "res://smith.png",
                        SemanticHints = new[] { "scene:rest-site", "option-id:SMITH", "source:button" },
                    },
                    new ObserverChoice("rest-option", "Hatch", "1120,260,200,110", "HATCH", "Hatch a card")
                    {
                        NodeId = "rest-site:HATCH",
                        BindingKind = "rest-site-option",
                        BindingId = "HATCH",
                        Enabled = true,
                        SemanticHints = new[] { "scene:rest-site", "option-id:HATCH", "source:button" },
                    },
                    new ObserverChoice("rest-option", "Mend", "1420,260,200,110", "MEND", "Special rest-site option")
                    {
                        NodeId = "rest-site:MEND",
                        BindingKind = "rest-site-option",
                        BindingId = "MEND",
                        Enabled = true,
                        SemanticHints = new[] { "scene:rest-site", "option-id:MEND", "source:button" },
                    },
                },
                Array.Empty<ObservedCombatHandCard>());
            var restSiteMetadataObserver = new ObserverState(restSiteMetadataSummary, null, null, null);
            var restSiteMetadataRequest = new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                10,
                GuiSmokePhase.ChooseFirstNode.ToString(),
                "Choose the authoritative rest-site option.",
                DateTimeOffset.UtcNow,
                restSiteMetadataScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                "phase:choosefirstnode|screen:map|visible:map|encounter:restsite|ready:true|stability:stable|room:rest-site|layer:map-background|layer:map-overlay-foreground",
                "0001",
                1,
                3,
                true,
                "semantic",
                null,
                restSiteMetadataSummary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                GetAllowedActions(GuiSmokePhase.ChooseFirstNode, restSiteMetadataObserver),
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Authoritative rest-site buttons must supply metadata and hitboxes.",
                null);
            var restSiteMetadataAnalysis = AutoDecisionProvider.Analyze(restSiteMetadataRequest);
            Assert(string.Equals(restSiteMetadataAnalysis.FinalDecision.TargetLabel, "rest site: smith", StringComparison.OrdinalIgnoreCase),
                "Metadata-first rest-site decisioning should prefer smith when HP is healthy.");
            var selectedRestSiteCandidate = restSiteMetadataAnalysis.Candidates.Single(candidate => candidate.Selected);
            Assert(!string.IsNullOrWhiteSpace(selectedRestSiteCandidate.RawBounds)
                   && !string.IsNullOrWhiteSpace(selectedRestSiteCandidate.BoundsSource)
                   && !string.Equals(selectedRestSiteCandidate.BoundsSource, "provider-external", StringComparison.OrdinalIgnoreCase),
                "Selected authoritative rest-site candidate should retain raw bounds and a non-external bounds source.");
            Assert(restSiteMetadataAnalysis.DebugSummary.SuppressedCandidates.Any(candidate => string.Equals(candidate.Label, "click visible map advance", StringComparison.OrdinalIgnoreCase))
                   && restSiteMetadataAnalysis.DebugSummary.SuppressedCandidates.Any(candidate => string.Equals(candidate.Label, "click exported reachable node", StringComparison.OrdinalIgnoreCase))
                   && restSiteMetadataAnalysis.DebugSummary.SuppressedCandidates.Any(candidate => string.Equals(candidate.Label, "click first reachable node", StringComparison.OrdinalIgnoreCase))
                   && restSiteMetadataAnalysis.DebugSummary.SuppressedCandidates.Any(candidate => string.Equals(candidate.Label, "click map back", StringComparison.OrdinalIgnoreCase)),
                "Authoritative rest-site choices should keep map-routing fallbacks suppressed.");
            Assert(restSiteMetadataAnalysis.Candidates.Any(candidate =>
                    string.Equals(candidate.Label, "click option:MEND", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(candidate.RejectReason, "not-default-auto-pick", StringComparison.OrdinalIgnoreCase)),
                "Additional rest-site options should be visible in candidates but excluded from default auto-pick.");

            var missingHitboxSummary = restSiteMetadataSummary with
            {
                Choices = new[]
                {
                    new ObserverChoice("rest-option", "Smith", null, "SMITH", "Upgrade a card")
                    {
                        NodeId = "rest-site:SMITH",
                        BindingKind = "rest-site-option",
                        BindingId = "SMITH",
                        Enabled = true,
                        SemanticHints = new[] { "scene:rest-site", "option-id:SMITH", "source:button" },
                    },
                },
                CurrentChoices = new[] { "Smith" },
            };
            var missingHitboxRequest = restSiteMetadataRequest with
            {
                Observer = missingHitboxSummary,
                AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, new ObserverState(missingHitboxSummary, null, null, null)),
            };
            var missingHitboxAnalysis = AutoDecisionProvider.Analyze(missingHitboxRequest);
            Assert(string.Equals(missingHitboxAnalysis.FinalDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                   && missingHitboxAnalysis.Candidates.Any(candidate =>
                       string.Equals(candidate.Label, "click rest site choice", StringComparison.OrdinalIgnoreCase)
                       && string.Equals(candidate.RejectReason, "missing-hitbox-for-explicit-choice", StringComparison.OrdinalIgnoreCase)),
                "Authoritative rest-site options without hitboxes should wait and record missing-hitbox-for-explicit-choice instead of using fixed coordinates.");

            var legacyNoBoundsSummary = restSiteMetadataSummary with
            {
                Choices = Array.Empty<ObserverChoice>(),
                ActionNodes = Array.Empty<ObserverActionNode>(),
                CurrentChoices = new[] { "Smith" },
            };
            var legacyNoBoundsRequest = restSiteMetadataRequest with
            {
                Observer = legacyNoBoundsSummary,
                AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, new ObserverState(legacyNoBoundsSummary, null, null, null)),
            };
            var legacyNoBoundsAnalysis = AutoDecisionProvider.Analyze(legacyNoBoundsRequest);
            Assert(string.Equals(legacyNoBoundsAnalysis.FinalDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                   && legacyNoBoundsAnalysis.Candidates.Any(candidate =>
                       string.Equals(candidate.Label, "click rest site choice", StringComparison.OrdinalIgnoreCase)
                       && string.Equals(candidate.RejectReason, "legacy-rest-site-choice-without-bounds", StringComparison.OrdinalIgnoreCase)),
                "Legacy rest-site fallback should wait when no actionable observer bounds exist instead of guessing fixed smith/rest coordinates.");

            var fingerprint = RestSiteChoiceSupport.BuildExplicitChoiceFingerprint(restSiteMetadataSummary);
            var firstSmithClick = new GuiSmokeHistoryEntry(
                GuiSmokePhase.ChooseFirstNode.ToString(),
                "click",
                "rest site: smith",
                DateTimeOffset.UtcNow.AddSeconds(-2))
            {
                Metadata = JsonSerializer.Serialize(
                    new RestSiteActionMetadata("explicit-click", "rest site: smith", fingerprint),
                    GuiSmokeShared.JsonOptions),
            };
            var graceRequest = restSiteMetadataRequest with
            {
                History = new[] { firstSmithClick },
            };
            var graceDecision = AutoDecisionProvider.Decide(graceRequest);
            Assert(string.Equals(graceDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(graceDecision.DecisionRisk, "rest-site-post-click-recapture-grace", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(graceDecision.TargetLabel, "rest site: smith", StringComparison.OrdinalIgnoreCase),
                "After a rest-site option click, the next identical fingerprint should yield a single recapture grace wait instead of another smith click.");

            var graceHistoryEntry = new GuiSmokeHistoryEntry(
                GuiSmokePhase.ChooseFirstNode.ToString(),
                "wait",
                graceDecision.TargetLabel,
                DateTimeOffset.UtcNow.AddSeconds(-1))
            {
                Metadata = AutoDecisionProvider.BuildRestSiteHistoryMetadataForDecision(graceRequest, graceDecision),
            };
            var noOpRequest = restSiteMetadataRequest with
            {
                History = new[] { firstSmithClick, graceHistoryEntry },
            };
            var noOpDecision = AutoDecisionProvider.Decide(noOpRequest);
            Assert(string.Equals(noOpDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(noOpDecision.DecisionRisk, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase)
                   && TryClassifyRestSitePostClickNoOp(GuiSmokePhase.ChooseFirstNode, noOpRequest, noOpDecision, out var noOpMessage)
                   && noOpMessage.Contains("rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase),
                "A repeated explicit rest-site fingerprint after grace should escalate to rest-site-post-click-noop and stop repeated smith clicks.");

            var selectionFailedSummary = restSiteMetadataSummary with
            {
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["restSiteSelectionLastSignal"] = "after-select-failure",
                    ["restSiteSelectionLastOptionId"] = "SMITH",
                    ["restSiteSelectionCurrentStatus"] = "explicit-choice",
                    ["restSiteUpgradeScreenVisible"] = "false",
                },
            };
            var selectionFailedRequest = restSiteMetadataRequest with
            {
                Observer = selectionFailedSummary,
                AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, new ObserverState(selectionFailedSummary, null, null, null)),
                History = new[] { firstSmithClick, graceHistoryEntry },
            };
            var selectionFailedDecision = AutoDecisionProvider.Decide(selectionFailedRequest);
            Assert(string.Equals(selectionFailedDecision.DecisionRisk, "rest-site-selection-failed", StringComparison.OrdinalIgnoreCase)
                   && TryClassifyRestSitePostClickNoOp(GuiSmokePhase.ChooseFirstNode, selectionFailedRequest, selectionFailedDecision, out var selectionFailedMessage)
                   && selectionFailedMessage.Contains("rest-site-selection-failed", StringComparison.OrdinalIgnoreCase),
                "Rest-site selection failure metadata should surface as rest-site-selection-failed instead of a generic noop.");

            var smithUpgradeSummary = restSiteMetadataSummary with
            {
                CurrentScreen = "upgrade",
                VisibleScreen = "upgrade",
                ChoiceExtractorPath = "rest-smith-upgrade",
                CurrentChoices = new[] { "Strike", "Defend", "Smith Confirm" },
                Choices = new[]
                {
                    new ObserverChoice("rest-site-smith-card", "Strike", "420,220,150,210", "CARD.STRIKE_IRONCLAD", "Upgradable card")
                    {
                        NodeId = "rest-site:smith-card:card-strike-ironclad",
                        BindingKind = "rest-site-smith-card",
                        BindingId = "CARD.STRIKE_IRONCLAD",
                        Enabled = true,
                        SemanticHints = new[] { "scene:rest-site", "substate:smith-grid", "source:grid-holder" },
                    },
                    new ObserverChoice("rest-site-smith-card", "Defend", "610,220,150,210", "CARD.DEFEND_IRONCLAD", "Upgradable card")
                    {
                        NodeId = "rest-site:smith-card:card-defend-ironclad",
                        BindingKind = "rest-site-smith-card",
                        BindingId = "CARD.DEFEND_IRONCLAD",
                        Enabled = true,
                        SemanticHints = new[] { "scene:rest-site", "substate:smith-grid", "source:grid-holder" },
                    },
                    new ObserverChoice("rest-site-smith-confirm", "Smith Confirm", "980,520,150,80", "SMITH_CONFIRM", "Confirm smith upgrade")
                    {
                        NodeId = "rest-site:smith-confirm",
                        BindingKind = "rest-site-smith-confirm",
                        BindingId = "SMITH_CONFIRM",
                        Enabled = true,
                        SemanticHints = new[] { "scene:rest-site", "substate:smith-confirm", "source:confirm-button" },
                    },
                },
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["restSiteSelectionCurrentStatus"] = "grid-visible",
                    ["restSiteSelectionCurrentOptionId"] = "SMITH",
                    ["restSiteUpgradeScreenVisible"] = "true",
                    ["restSiteUpgradeCardCount"] = "2",
                    ["restSiteViewKind"] = "smith-grid",
                },
            };
            var smithUpgradeObserver = new ObserverState(smithUpgradeSummary, null, null, null);
            var smithUpgradeRequest = restSiteMetadataRequest with
            {
                Observer = smithUpgradeSummary,
                AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, smithUpgradeObserver),
            };
            Assert(!GuiSmokeNonCombatContractSupport.HasExplicitRestSiteChoiceAuthority(smithUpgradeObserver, restSiteMetadataScreenshotPath)
                   && smithUpgradeRequest.AllowedActions.Contains("click smith card", StringComparer.OrdinalIgnoreCase),
                "Visible smith upgrade state should disable explicit rest-site choice authority and expose smith card actions.");
            var smithUpgradeObserverOnlyContext = CreateObserverOnlyAnalysisContext(
                GuiSmokePhase.ChooseFirstNode,
                smithUpgradeObserver,
                Array.Empty<GuiSmokeHistoryEntry>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                new WindowBounds(0, 0, 1280, 720));
            var smithUpgradeObserverOnlyPolicy = GuiSmokeStepScreenshotPolicy.Evaluate(10, false, smithUpgradeObserverOnlyContext);
            Assert(!smithUpgradeObserverOnlyPolicy.NeedsScreenshot
                   && string.Equals(smithUpgradeObserverOnlyPolicy.SkipReason, "rest-site-upgrade-runtime", StringComparison.OrdinalIgnoreCase),
                "Observer-exported smith grid/confirm truth should keep ChooseFirstNode on the observer-only rest-site upgrade lane.");
            var smithUpgradeDecision = AutoDecisionProvider.Decide(smithUpgradeRequest);
            Assert(string.Equals(smithUpgradeDecision.TargetLabel, "rest site: smith confirm", StringComparison.OrdinalIgnoreCase),
                "Observer-exported smith confirm should preserve the rest-site lane once the confirm button is visible.");

            var disabledConfirmSmithUpgradeSummary = smithUpgradeSummary with
            {
                Choices = new[]
                {
                    smithUpgradeSummary.Choices[0],
                    smithUpgradeSummary.Choices[1],
                    smithUpgradeSummary.Choices[2] with
                    {
                        Enabled = false,
                    },
                },
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["restSiteSelectionCurrentStatus"] = "confirm-visible",
                    ["restSiteSelectionCurrentOptionId"] = "SMITH",
                    ["restSiteUpgradeScreenVisible"] = "true",
                    ["restSiteUpgradeCardCount"] = "2",
                    ["restSiteUpgradeConfirmVisible"] = "true",
                    ["restSiteUpgradeConfirmEnabled"] = "false",
                    ["restSiteViewKind"] = "smith-confirm",
                },
            };
            var disabledConfirmSmithUpgradeRequest = restSiteMetadataRequest with
            {
                Observer = disabledConfirmSmithUpgradeSummary,
                AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, new ObserverState(disabledConfirmSmithUpgradeSummary, null, null, null)),
            };
            var disabledConfirmSmithUpgradeDecision = AutoDecisionProvider.Decide(disabledConfirmSmithUpgradeRequest);
            Assert(string.Equals(disabledConfirmSmithUpgradeDecision.TargetLabel, "rest site: smith card", StringComparison.OrdinalIgnoreCase),
                "Disabled smith confirm should not be clicked while exported smith card choices remain actionable.");
            Assert(GuiSmokeNonCombatContractSupport.TryMapNonCombatAllowedAction(disabledConfirmSmithUpgradeDecision, out var disabledConfirmSmithUpgradeAction)
                   && string.Equals(disabledConfirmSmithUpgradeAction, "click smith card", StringComparison.OrdinalIgnoreCase),
                $"Smith-grid selection should map to click smith card instead of the broader rest-site choice lane. actual={disabledConfirmSmithUpgradeAction ?? "null"}");

            var missingConfirmAfterSmithSummary = smithUpgradeSummary with
            {
                Choices = Array.Empty<ObserverChoice>(),
                CurrentChoices = new[] { "Smith Confirm" },
            };
            var missingConfirmAfterSmithRequest = restSiteMetadataRequest with
            {
                Observer = missingConfirmAfterSmithSummary,
                AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, new ObserverState(missingConfirmAfterSmithSummary, null, null, null)),
                History = new[]
                {
                    new GuiSmokeHistoryEntry(
                        GuiSmokePhase.ChooseFirstNode.ToString(),
                        "click",
                        "rest site: smith card",
                        DateTimeOffset.UtcNow.AddSeconds(-1)),
                },
            };
            var missingConfirmAfterSmithDecision = AutoDecisionProvider.Decide(missingConfirmAfterSmithRequest);
            Assert(string.Equals(missingConfirmAfterSmithDecision.Status, "wait", StringComparison.OrdinalIgnoreCase),
                "Rest-site smith confirm should wait when no exported confirm/card hitbox is present instead of clicking a fixed confirm coordinate.");

            var restSiteSelectionSettlingSummary = restSiteMetadataSummary with
            {
                CurrentScreen = "rest-site",
                VisibleScreen = "rest-site",
                ChoiceExtractorPath = "rest",
                CurrentChoices = new[] { "휴식", "재련", "부화" },
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["restSiteSelectionLastSignal"] = "after-select-success",
                    ["restSiteSelectionLastOptionId"] = "SMITH",
                    ["restSiteSelectionLastSuccess"] = "true",
                    ["restSiteSelectionCurrentStatus"] = "explicit-choice",
                    ["restSiteViewKind"] = "explicit-choice",
                },
            };
            var restSiteSelectionSettlingObserver = new ObserverState(restSiteSelectionSettlingSummary, null, null, null);
            Assert(!GuiSmokeNonCombatContractSupport.HasExplicitRestSiteChoiceAuthority(restSiteSelectionSettlingObserver, restSiteMetadataScreenshotPath),
                "Post-selection rest-site settle should not reopen the explicit rest-site choice lane before proceed or smith overlay truth appears.");
            Assert(AutoDecisionProvider.BuildRestSiteSceneState(restSiteSelectionSettlingObserver) is { SelectionSettling: true },
                "Rest-site scene state should preserve a selection-settling stage while post-smith proceed is not yet visible.");
            var restSiteSelectionSettlingActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, restSiteSelectionSettlingObserver);
            Assert(restSiteSelectionSettlingActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase),
                $"Rest-site selection-settling should wait instead of reopening explicit choices. actual=[{string.Join(", ", restSiteSelectionSettlingActions)}]");
            var restSiteSelectionSettlingDecision = AutoDecisionProvider.Decide(restSiteMetadataRequest with
            {
                Observer = restSiteSelectionSettlingSummary,
                AllowedActions = restSiteSelectionSettlingActions,
            });
            Assert(string.Equals(restSiteSelectionSettlingDecision.Status, "wait", StringComparison.OrdinalIgnoreCase),
                "ChooseFirstNode should wait while a completed rest-site selection is settling back to proceed.");

            var restSiteSelectionInProgressSummary = restSiteMetadataSummary with
            {
                CurrentScreen = "rest-site",
                VisibleScreen = "rest-site",
                ChoiceExtractorPath = "rest",
                CurrentChoices = new[] { "휴식", "재련", "부화" },
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["restSiteSelectionLastSignal"] = "before-select",
                    ["restSiteSelectionLastOptionId"] = "SMITH",
                    ["restSiteSelectionCurrentStatus"] = "selecting",
                    ["restSiteSelectionCurrentOptionId"] = "SMITH",
                    ["restSiteSelectionOutcome"] = "in-progress",
                    ["restSiteViewKind"] = "explicit-choice",
                },
            };
            var restSiteSelectionInProgressObserver = new ObserverState(restSiteSelectionInProgressSummary, null, null, null);
            Assert(AutoDecisionProvider.BuildRestSiteSceneState(restSiteSelectionInProgressObserver) is { SelectionSettling: true },
                "Rest-site scene state should preserve a selection-settling stage immediately after a smith click is accepted.");
            var restSiteSelectionInProgressActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, restSiteSelectionInProgressObserver);
            Assert(restSiteSelectionInProgressActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase),
                $"Rest-site in-progress selection should wait instead of clicking smith twice. actual=[{string.Join(", ", restSiteSelectionInProgressActions)}]");

            var restSiteProceedSummary = restSiteMetadataSummary with
            {
                CurrentScreen = "rest-site",
                VisibleScreen = "rest-site",
                ChoiceExtractorPath = "generic",
                CurrentChoices = new[] { "진행", "불타는 혈액" },
                Choices = new[]
                {
                    new ObserverChoice("choice", "진행", "1576.3,761.3,282.45,113.4", null, "진행"),
                    new ObserverChoice("relic", "불타는 혈액", "12,82,68,68", "RELIC.BURNING_BLOOD"),
                },
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["restSiteSelectionLastSignal"] = "after-select-success",
                    ["restSiteSelectionLastSuccess"] = "true",
                },
            };
            var restSiteProceedObserver = new ObserverState(restSiteProceedSummary, null, null, null);
            Assert(GuiSmokeNonCombatContractSupport.LooksLikeRestSiteProceedState(restSiteProceedSummary),
                "Rest-site proceed helper should recognize post-confirm proceed-visible observer state.");
            var restSiteProceedRequest = restSiteMetadataRequest with
            {
                Observer = restSiteProceedSummary,
                AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, restSiteProceedObserver),
            };
            Assert(restSiteProceedRequest.AllowedActions.Contains("click proceed", StringComparer.OrdinalIgnoreCase)
                   && !restSiteProceedRequest.AllowedActions.Contains("click smith card", StringComparer.OrdinalIgnoreCase),
                "ChooseFirstNode allowlist should reopen the rest-site proceed lane before stale smith/wait residue.");
            Assert(AutoDecisionProvider.BuildRestSiteSceneState(restSiteProceedObserver) is
                {
                    CanonicalForegroundOwner: NonCombatCanonicalForegroundOwner.RestSite,
                    HandoffTarget: NonCombatHandoffTarget.ChooseFirstNode,
                    AllowsFastForegroundWait: true,
                },
                "Rest-site scene state should preserve ChooseFirstNode handoff and fast foreground waits.");
            var restSiteProceedDecision = AutoDecisionProvider.Decide(restSiteProceedRequest);
            Assert(string.Equals(restSiteProceedDecision.TargetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase),
                "Observer-visible rest-site proceed choice should create a visible proceed decision without waiting for screenshot arrows.");
            var restSiteProceedRewardContext = GuiSmokeStepRequestFactory.CreateObserverOnlyAnalysisContext(
                GuiSmokePhase.HandleRewards,
                restSiteProceedObserver,
                Array.Empty<GuiSmokeHistoryEntry>(),
                Array.Empty<CombatCardKnowledgeHint>());
            Assert(!restSiteProceedRewardContext.UseRewardFastPath,
                "Rest-site proceed aftermath should not reopen the reward fast path just because proceed/relic affordances are visible.");
            var restSiteProceedHandleRewardsActions = BuildAllowedActions(
                GuiSmokePhase.HandleRewards,
                restSiteProceedObserver,
                Array.Empty<CombatCardKnowledgeHint>(),
                string.Empty,
                Array.Empty<GuiSmokeHistoryEntry>());
            Assert(restSiteProceedHandleRewardsActions.Contains("click proceed", StringComparer.OrdinalIgnoreCase)
                   && !restSiteProceedHandleRewardsActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase),
                "HandleRewards fallback should recover to the rest-site proceed lane instead of collapsing to wait on post-confirm rest-site surfaces.");
            var restSiteProceedHandleRewardsDecision = AutoDecisionProvider.Decide(restSiteProceedRequest with
            {
                Phase = GuiSmokePhase.HandleRewards.ToString(),
                AllowedActions = restSiteProceedHandleRewardsActions,
            });
            Assert(string.Equals(restSiteProceedHandleRewardsDecision.TargetLabel, "visible proceed", StringComparison.OrdinalIgnoreCase),
                "HandleRewards compatibility routing should delegate back to the rest-site proceed lane when reward authority is absent.");

            var restSiteProceedLabelOnlySummary = restSiteProceedSummary with
            {
                Choices = new[]
                {
                    new ObserverChoice("relic", "불타는 혈액", "12,82,68,68", "RELIC.BURNING_BLOOD"),
                },
            };
            Assert(GuiSmokeNonCombatContractSupport.LooksLikeRestSiteProceedState(restSiteProceedLabelOnlySummary),
                "Rest-site proceed helper should still promote post-confirm proceed when currentChoices expose the proceed affordance before structured choice export stabilizes.");
            var restSiteProceedBranchRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-rest-site-proceed-{Guid.NewGuid():N}");
            Directory.CreateDirectory(restSiteProceedBranchRoot);
            try
            {
                var restSiteProceedBranchLogger = new ArtifactRecorder(restSiteProceedBranchRoot);
                var restSiteProceedBranchHistory = new List<GuiSmokeHistoryEntry>();
                Assert(
                    TryAdvanceAlternateBranch(
                        GuiSmokePhase.WaitMap,
                        restSiteProceedObserver,
                        restSiteProceedBranchHistory,
                        restSiteProceedBranchLogger,
                        9,
                        true,
                        out var restSiteProceedNextPhase)
                    && restSiteProceedNextPhase == GuiSmokePhase.ChooseFirstNode,
                    "WaitMap should branch immediately to ChooseFirstNode when a post-confirm rest-site proceed affordance is visible.");
            }
            finally
            {
                try
                {
                    Directory.Delete(restSiteProceedBranchRoot, true);
                }
                catch
                {
                }
            }

            var legacyRestSiteRequestPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "artifacts",
                "gui-smoke",
                "verify-longrun-20260316-10",
                "attempts",
                "0001",
                "steps",
                "0010.request.json");
            if (File.Exists(legacyRestSiteRequestPath))
            {
                var legacyReplayRequest = LoadReplayRequest(legacyRestSiteRequestPath).Request;
                var legacyReplayArtifact = EvaluateAutoDecisionWithDiagnostics(legacyRestSiteRequestPath, legacyReplayRequest).CandidateDump;
                Assert(legacyReplayArtifact.FinalDecision.Status is "wait"
                       && string.Equals(legacyReplayArtifact.FinalDecision.Reason, "waiting for an explicit rest-site choice", StringComparison.OrdinalIgnoreCase),
                    "Legacy replay artifact without explicit rest-site metadata should now wait instead of reconstructing rest-site semantics from bridge compatibility winners.");
            }

            var mixedSmithGridSummary = disabledConfirmSmithUpgradeSummary with
            {
                CurrentScreen = "upgrade",
                VisibleScreen = "upgrade",
                EncounterKind = "RestSite",
                ChoiceExtractorPath = "card-selection-upgrade",
                ActionNodes = new[]
                {
                    new ObserverActionNode("map:8:3", "map-node", "RestSite (8,3)", "642,425,56,56", true)
                    {
                        TypeName = "map-node",
                    },
                },
                Meta = new Dictionary<string, string?>(disabledConfirmSmithUpgradeSummary.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    ["cardSelectionScreenType"] = "upgrade",
                    ["cardSelectionSelectedCount"] = "0",
                    ["restSiteUpgradeScreenVisible"] = "true",
                    ["restSiteUpgradeCardCount"] = "2",
                    ["restSiteViewKind"] = "smith-grid",
                },
            };
            var mixedSmithGridObserver = new ObserverState(mixedSmithGridSummary, null, null, null);
            var mixedSmithGridAllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, mixedSmithGridObserver);
            Assert(mixedSmithGridAllowedActions.Contains("click smith card", StringComparer.OrdinalIgnoreCase)
                   && !mixedSmithGridAllowedActions.Contains("click exported reachable node", StringComparer.OrdinalIgnoreCase),
                $"Rest-site smith grid should keep smith-card allowlists ahead of stale map-overlay routing. allowed=[{string.Join(", ", mixedSmithGridAllowedActions)}]");

            var mixedRestAftermathSummary = restSiteMetadataSummary with
            {
                CurrentScreen = "rest-site",
                VisibleScreen = "rest-site",
                ChoiceExtractorPath = "generic",
                CurrentChoices = Array.Empty<string>(),
                ActionNodes = Array.Empty<ObserverActionNode>(),
                Choices = Array.Empty<ObserverChoice>(),
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["mapCurrentActiveScreen"] = "true",
                    ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                },
            };
            var mixedRestAftermathObserver = new ObserverState(mixedRestAftermathSummary, null, null, null);
            var mixedRestAftermathAllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, mixedRestAftermathObserver);
            Assert(mixedRestAftermathAllowedActions.Contains("click exported reachable node", StringComparer.OrdinalIgnoreCase)
                   && !mixedRestAftermathAllowedActions.Contains("click smith card", StringComparer.OrdinalIgnoreCase),
                "Mixed rest-site aftermath should reopen ChooseFirstNode on the map lane instead of stale smith/wait allowlists once NMapScreen is current.");
            Assert(GuiSmokeObserverPhaseHeuristics.TryGetPostEmbarkPhase(mixedRestAftermathSummary, out var mixedRestAftermathPhase)
                   && mixedRestAftermathPhase == GuiSmokePhase.ChooseFirstNode,
                "Post-embark reconciliation should prefer map progression over stale rest-site residue when mapCurrentActiveScreen is true.");

            var mixedRestAftermathReplayRoot = Path.Combine(
                Directory.GetCurrentDirectory(),
                "artifacts",
                "gui-smoke",
                "verify-reward-proceed-aftermath-v2-20260322-200024",
                "attempts",
                "0001",
                "steps");
            foreach (var replayStep in new[] { "0018", "0019" })
            {
                var replayRequestPath = Path.Combine(mixedRestAftermathReplayRoot, $"{replayStep}.request.json");
                if (!File.Exists(replayRequestPath))
                {
                    continue;
                }

                var replayRequest = LoadReplayRequest(replayRequestPath).Request;
                Assert(!GuiSmokeNonCombatContractSupport.AllowsAnyMapRoutingAction(replayRequest),
                    $"Mixed rest-site aftermath replay {replayStep} should preserve the original stale non-map allowlist before rebuild.");
                var replayArtifact = EvaluateAutoDecisionWithDiagnostics(replayRequestPath, replayRequest).CandidateDump;
                Assert(string.Equals(replayArtifact.FinalDecision.Status, "wait", StringComparison.OrdinalIgnoreCase),
                    $"Mixed rest-site aftermath replay {replayStep} should wait instead of emitting illegal screenshot map routing against a non-map allowlist.");
                Assert(!string.Equals(replayArtifact.FinalDecision.TargetLabel, "visible reachable node", StringComparison.OrdinalIgnoreCase),
                    $"Mixed rest-site aftermath replay {replayStep} must not recreate the old request/decision mismatch.");

                var rebuiltReplayRequest = LoadReplayRequest(replayRequestPath, fullRequestRebuild: true).Request;
                Assert(GuiSmokeNonCombatContractSupport.AllowsAnyMapRoutingAction(rebuiltReplayRequest)
                       && !rebuiltReplayRequest.AllowedActions.Contains("click smith card", StringComparer.OrdinalIgnoreCase)
                       && !rebuiltReplayRequest.AllowedActions.Contains("click smith confirm", StringComparer.OrdinalIgnoreCase),
                    $"Mixed rest-site aftermath replay rebuild {replayStep} should reopen the legal map-routing lane instead of stale smith/wait actions. Actual allowlist=[{string.Join(", ", rebuiltReplayRequest.AllowedActions)}].");
            }
        }
        finally
        {
            if (File.Exists(restSiteMetadataScreenshotPath))
            {
                File.Delete(restSiteMetadataScreenshotPath);
            }
        }

        var mapOverlayScreenshotPath = Path.Combine(Path.GetTempPath(), $"gui-smoke-map-overlay-self-test-{Guid.NewGuid():N}.png");
        try
        {
            using (var bitmap = new Bitmap(1280, 720))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var parchmentBrush = new SolidBrush(Color.FromArgb(201, 176, 132)))
            using (var arrowBrush = new SolidBrush(Color.FromArgb(220, 55, 48)))
            using (var nodeBrush = new SolidBrush(Color.FromArgb(63, 54, 42)))
            using (var legendBrush = new SolidBrush(Color.FromArgb(184, 207, 222)))
            {
                graphics.Clear(Color.Black);
                graphics.FillRectangle(parchmentBrush, new Rectangle(210, 40, 840, 650));
                graphics.FillEllipse(arrowBrush, new Rectangle(892, 414, 54, 46));
                graphics.FillEllipse(nodeBrush, new Rectangle(884, 454, 82, 82));
                graphics.FillEllipse(nodeBrush, new Rectangle(882, 548, 86, 86));
                graphics.FillRectangle(legendBrush, new Rectangle(1038, 193, 176, 285));
                graphics.FillPolygon(arrowBrush, new[]
                {
                    new Point(18, 515),
                    new Point(66, 478),
                    new Point(66, 495),
                    new Point(102, 495),
                    new Point(102, 535),
                    new Point(66, 535),
                    new Point(66, 552),
                });
                bitmap.Save(mapOverlayScreenshotPath, ImageFormat.Png);
            }

            using var mapOverlayStateDocument = JsonDocument.Parse("""{"meta":{"declaringType":"MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen","instanceType":"MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen","choiceExtractorPath":"event","mapOverlayVisible":"true"}}""");
            var mapOverlayObserver = new ObserverState(
                new ObserverSummary(
                    "event",
                    "event",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-map-overlay",
                    true,
                    "mixed",
                    "stable",
                    "episode-map-overlay",
                    "None",
                    "event",
                    80,
                    80,
                    null,
                    new[] { "계속", "Back", "휴식 (1,2)" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("event-option:0", "event-option", "계속", "922,596,800,100", true),
                        new ObserverActionNode("back:left", "choice", "Back", "48,930,88,88", true),
                    },
                    new[]
                    {
                        new ObserverChoice("choice", "계속", "922,596,800,100", "계속"),
                        new ObserverChoice("choice", "Back", "48,930,88,88", "Back"),
                        new ObserverChoice("map-node", "휴식 (1,2)", "897,581,124,124", "1,2", "type:Rest;coord:1,2"),
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    PublishedCurrentScreen = "map",
                    PublishedVisibleScreen = "map",
                    PublishedSceneReady = true,
                    PublishedSceneAuthority = "hook",
                    PublishedSceneStability = "stable",
                },
                mapOverlayStateDocument,
                null,
                null);
            var mapOverlaySignature = ComputeSceneSignature(mapOverlayScreenshotPath, mapOverlayObserver, GuiSmokePhase.ChooseFirstNode);
            Assert(mapOverlaySignature.Contains("layer:map-overlay-foreground", StringComparison.OrdinalIgnoreCase), "Map overlay foreground should be modeled explicitly when NMapScreen is open over a stale event context.");
            Assert(mapOverlaySignature.Contains("stale:event-choice", StringComparison.OrdinalIgnoreCase), "Stale event choices should be marked as stale when the map overlay is foreground.");
            Assert(mapOverlaySignature.Contains("map-back-navigation-available", StringComparison.OrdinalIgnoreCase), "Map overlay should expose the back-navigation affordance.");
            Assert(!GetAllowedActions(GuiSmokePhase.ChooseFirstNode, mapOverlayObserver).Contains("click visible map advance", StringComparer.OrdinalIgnoreCase), "ChooseFirstNode should not expose visible map advance while map overlay foreground is active.");
            Assert(GetAllowedActions(GuiSmokePhase.ChooseFirstNode, mapOverlayObserver).Contains("click exported reachable node", StringComparer.OrdinalIgnoreCase), "ChooseFirstNode should promote exported map points to first-class candidates in mixed map-overlay state.");
            Assert(GetAllowedActions(GuiSmokePhase.ChooseFirstNode, mapOverlayObserver).Contains("click map back", StringComparer.OrdinalIgnoreCase), "ChooseFirstNode should open map back-navigation in mixed map-overlay state.");
            Assert(GetAllowedActions(GuiSmokePhase.HandleEvent, mapOverlayObserver).Contains("click exported reachable node", StringComparer.OrdinalIgnoreCase)
                   && !GetAllowedActions(GuiSmokePhase.HandleEvent, mapOverlayObserver).Contains("click proceed", StringComparer.OrdinalIgnoreCase),
                "True map-overlay foreground should still win during HandleEvent when explicit event proceed authority is absent.");
            var live5bMixedAftermathObserver = new ObserverState(
                mapOverlayObserver.Summary with
                {
                    ChoiceExtractorPath = "event+map",
                    CurrentChoices = new[] { "계속", "계속", "Monster (5,1)", "RestSite (5,2)", "Monster (5,3)" },
                    ActionNodes = new[]
                    {
                        new ObserverActionNode("event-option:0", "event-option", "계속", "922,596,800,100", true)
                        {
                            TypeName = "event-option",
                            SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option-button", "option-role:proceed", "event-proceed" },
                        },
                        new ObserverActionNode("event-option:proceed", "event-option", "계속", null, true)
                        {
                            TypeName = "event-option",
                            SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option", "option-role:proceed", "event-proceed" },
                        },
                        new ObserverActionNode("map:5:1", "event-option", "Monster (5,1)", "621.923,513.368,56,56", true)
                        {
                            TypeName = "map-node",
                            SemanticHints = new[] { "scene:event", "kind:event-option", "raw-kind:map-node", "node-id:map:5:1", "coord:5,1" },
                        },
                        new ObserverActionNode("map:5:2", "event-option", "RestSite (5,2)", "770.838,528.498,56,56", true)
                        {
                            TypeName = "map-node",
                            SemanticHints = new[] { "scene:event", "kind:event-option", "raw-kind:map-node", "node-id:map:5:2", "coord:5,2" },
                        },
                        new ObserverActionNode("map:5:3", "event-option", "Monster (5,3)", "890.219,509.932,56,56", true)
                        {
                            TypeName = "map-node",
                            SemanticHints = new[] { "scene:event", "kind:event-option", "raw-kind:map-node", "node-id:map:5:3", "coord:5,3" },
                        },
                    },
                    Choices = new[]
                    {
                        new ObserverChoice("event-option", "계속", "922,596,800,100", "PROCEED", "[gold][b]계속[/b][/gold]")
                        {
                            NodeId = "event-option:0",
                            BindingKind = "event-option",
                            BindingId = "PROCEED",
                            Enabled = true,
                            SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option-button", "option-role:proceed", "event-proceed" },
                        },
                        new ObserverChoice("event-option", "계속", null, "PROCEED", "계속")
                        {
                            NodeId = "event-option:proceed",
                            BindingKind = "event-option",
                            BindingId = "PROCEED",
                            Enabled = true,
                            SemanticHints = new[] { "scene:event", "kind:event-option", "source:event-option", "option-role:proceed", "event-proceed" },
                        },
                        new ObserverChoice("map-node", "Monster (5,1)", "621.923,513.368,56,56", "5,1", "type:Monster;state:Travelable;coord:5,1")
                        {
                            NodeId = "map:5:1",
                            Enabled = true,
                        },
                        new ObserverChoice("map-node", "RestSite (5,2)", "770.838,528.498,56,56", "5,2", "type:RestSite;state:Travelable;coord:5,2")
                        {
                            NodeId = "map:5:2",
                            Enabled = true,
                        },
                        new ObserverChoice("map-node", "Monster (5,3)", "890.219,509.932,56,56", "5,3", "type:Monster;state:Travelable;coord:5,3")
                        {
                            NodeId = "map:5:3",
                            Enabled = true,
                        },
                    },
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["choiceExtractorPath"] = "event+map",
                        ["eventProceedOptionVisible"] = "true",
                        ["eventProceedOptionEnabled"] = "true",
                        ["eventProceedOptionCount"] = "1",
                        ["mapCurrentActiveScreen"] = "true",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    },
                },
                mapOverlayStateDocument,
                null,
                null);
            Assert(NonCombatForegroundOwnership.Resolve(live5bMixedAftermathObserver) == NonCombatForegroundOwner.Map, "Map current-active-screen authority should own mixed event/map aftermath even when event proceed residue lingers.");
            Assert(!EventProceedObserverSignals.HasExplicitEventProceedAuthority(live5bMixedAftermathObserver, new WindowBounds(1, 32, 1280, 720)), "Lingering EventOption.IsProceed residue should not stay authoritative once NMapScreen is the current active screen owner.");
            Assert(string.Equals(DetermineReasoningMode(GuiSmokePhase.HandleEvent, live5bMixedAftermathObserver, firstSeenScene: true), "tactical", StringComparison.OrdinalIgnoreCase), "Mixed aftermath with explicit NMapScreen ownership should collapse HandleEvent reasoning to tactical mode.");
            var mixedAftermathAllowedActions = BuildAllowedActions(GuiSmokePhase.HandleEvent, live5bMixedAftermathObserver, Array.Empty<CombatCardKnowledgeHint>(), mapOverlayScreenshotPath, Array.Empty<GuiSmokeHistoryEntry>());
            Assert(mixedAftermathAllowedActions.Contains("click exported reachable node", StringComparer.OrdinalIgnoreCase)
                   && !mixedAftermathAllowedActions.Contains("click proceed", StringComparer.OrdinalIgnoreCase),
                $"HandleEvent should reopen the map lane instead of the stale event-proceed lane when map owns the foreground. Actual allowlist=[{string.Join(", ", mixedAftermathAllowedActions)}].");
            var staleAncientMapAftermathObserver = new ObserverState(
                new ObserverSummary(
                    "map",
                    "map",
                    false,
                    DateTimeOffset.UtcNow,
                    "inv-live59-ancient",
                    true,
                    "published",
                    "stable",
                    null,
                    "Monster",
                    "map",
                    109,
                    80,
                    null,
                    new[] { "Monster (2,4)" },
                    Array.Empty<string>(),
                    new[]
                    {
                        new ObserverActionNode("map:2:4", "map-node", "Monster (2,4)", "1072.061,542.331,56,56", true)
                        {
                            TypeName = "map-node",
                            SemanticHints = new[] { "scene:map", "kind:map-node", "scene-raw:map", "scene-published:map", "value:2,4", "raw-kind:map-node", "node-id:map:2:4", "coord:2,4", "source:map-choice" },
                        },
                    },
                    new[]
                    {
                        new ObserverChoice("map-node", "Monster (2,4)", "1072.061,542.331,56,56", "2,4", "type:Monster;state:Travelable;coord:2,4")
                        {
                            NodeId = "map:2:4",
                            Enabled = true,
                        },
                    },
                    Array.Empty<ObservedCombatHandCard>())
                {
                    PublishedCurrentScreen = "map",
                    PublishedVisibleScreen = "map",
                    Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["choiceExtractorPath"] = "map",
                        ["foregroundOwner"] = "event",
                        ["foregroundActionLane"] = "ancient-option",
                        ["ancientEventDetected"] = "true",
                        ["ancientDialogueActive"] = "false",
                        ["ancientOptionCount"] = "0",
                        ["ancientCompletionCount"] = "0",
                        ["ancientPhase"] = "await-options",
                        ["ancientOptionSummary"] = "ancient-event-option:0@460,942,1000,100",
                        ["ancientCompletionSummary"] = "ancient-event-option:0@460,942,1000,100",
                        ["rewardForegroundOwned"] = "false",
                        ["rewardTeardownInProgress"] = "true",
                        ["mapCurrentActiveScreen"] = "true",
                        ["activeScreenType"] = "MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen",
                    },
                },
                mapOverlayStateDocument,
                null,
                null);
            Assert(!AncientEventObserverSignals.HasExplicitOptionSelection(staleAncientMapAftermathObserver.Summary),
                "Stale ancient-option residue must not stay actionable once explicit map foreground ownership is present.");
            Assert(!AncientEventObserverSignals.HasForegroundAuthority(staleAncientMapAftermathObserver.Summary),
                "Explicit map foreground ownership must outrank stale ancient foreground metadata.");
            Assert(!AutoDecisionProvider.HasExplicitEventRecoveryAuthority(staleAncientMapAftermathObserver, new WindowBounds(1, 32, 1280, 720), Array.Empty<GuiSmokeHistoryEntry>()),
                "Stale ancient-event residue must not reopen explicit event recovery once map ownership is explicit.");
            var staleAncientAllowedActions = BuildAllowedActions(GuiSmokePhase.HandleEvent, staleAncientMapAftermathObserver, Array.Empty<CombatCardKnowledgeHint>(), mapOverlayScreenshotPath, Array.Empty<GuiSmokeHistoryEntry>());
            Assert(staleAncientAllowedActions.Contains("click exported reachable node", StringComparer.OrdinalIgnoreCase)
                   && !staleAncientAllowedActions.Contains("click event choice", StringComparer.OrdinalIgnoreCase),
                $"HandleEvent should reopen the map lane instead of waiting for stale ancient option buttons. Actual allowlist=[{string.Join(", ", staleAncientAllowedActions)}].");
            var staleAncientHandleEventDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                109,
                GuiSmokePhase.HandleEvent.ToString(),
                "Resolve the event screen. If nothing else is obvious, pick the first visible option.",
                DateTimeOffset.UtcNow,
                mapOverlayScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handleevent|screen:map|visible:map|encounter:monster|layer:reward-teardown|layer:map-background|layer:map-overlay-foreground|reachable-node-candidate-present|exported-reachable-node-present",
                "0001",
                1,
                4,
                true,
                "tactical",
                null,
                staleAncientMapAftermathObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                staleAncientAllowedActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Explicit map ownership should beat stale ancient event residue.",
                null));
            Assert(string.Equals(staleAncientHandleEventDecision.TargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase),
                $"HandleEvent should choose the exported reachable map node when stale ancient residue lingers. Actual decision={staleAncientHandleEventDecision.TargetLabel ?? "null"}.");
            var mixedAftermathChooseFirstNodeDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                51,
                GuiSmokePhase.ChooseFirstNode.ToString(),
                "Click the first reachable map node.",
                DateTimeOffset.UtcNow,
                mapOverlayScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:choosefirstnode|screen:event|visible:event|layer:map-overlay-foreground|layer:event-background|stale:event-choice|current-node-arrow-visible|reachable-node-candidate-present|exported-reachable-node-present",
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                live5bMixedAftermathObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                BuildAllowedActions(GuiSmokePhase.ChooseFirstNode, live5bMixedAftermathObserver, Array.Empty<CombatCardKnowledgeHint>(), mapOverlayScreenshotPath, Array.Empty<GuiSmokeHistoryEntry>()),
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Map owner should beat lingering event proceed residue.",
                null));
            Assert(string.Equals(mixedAftermathChooseFirstNodeDecision.TargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase),
                "Map-node routing should not stay blocked once mixed event aftermath resolves to map foreground ownership.");
            var mixedAftermathDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                51,
                GuiSmokePhase.ChooseFirstNode.ToString(),
                "Click the first reachable map node.",
                DateTimeOffset.UtcNow,
                mapOverlayScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:choosefirstnode|screen:event|visible:event|layer:map-overlay-foreground|layer:event-background|stale:event-choice|current-node-arrow-visible|reachable-node-candidate-present|exported-reachable-node-present",
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                live5bMixedAftermathObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                BuildAllowedActions(GuiSmokePhase.ChooseFirstNode, live5bMixedAftermathObserver, Array.Empty<CombatCardKnowledgeHint>(), mapOverlayScreenshotPath, Array.Empty<GuiSmokeHistoryEntry>()),
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Map owner should click an exported travelable node instead of waiting.",
                null));
            Assert(string.Equals(mixedAftermathDecision.TargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase), "ChooseFirstNode should act on exported travelable map nodes in mixed aftermath instead of stalling behind stale event residue.");
            var mixedAftermathWaitDecision = InvokeForegroundAwareNonCombatWaitDecision(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                52,
                GuiSmokePhase.ChooseFirstNode.ToString(),
                "Wait while stable explicit map ownership remains foreground-active.",
                DateTimeOffset.UtcNow,
                mapOverlayScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:choosefirstnode|screen:event|visible:event|layer:map-overlay-foreground|layer:event-background|stale:event-choice|current-node-arrow-visible|reachable-node-candidate-present|exported-reachable-node-present",
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                live5bMixedAftermathObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                BuildAllowedActions(GuiSmokePhase.ChooseFirstNode, live5bMixedAftermathObserver, Array.Empty<CombatCardKnowledgeHint>(), mapOverlayScreenshotPath, Array.Empty<GuiSmokeHistoryEntry>()),
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Stable explicit map ownership should use the faster noncombat wait.",
                null),
                "waiting for exported or screenshot-reachable map node");
            Assert(mixedAftermathWaitDecision.WaitMs == 400, "Stable explicit map ownership should use the faster noncombat wait.");
            var mapOverlayReplay = AutoDecisionProvider.Analyze(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                15,
                GuiSmokePhase.ChooseFirstNode.ToString(),
                "Click the first reachable map node.",
                DateTimeOffset.UtcNow,
                mapOverlayScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                mapOverlaySignature,
                "0001",
                1,
                3,
                true,
                "tactical",
                null,
                mapOverlayObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                Array.Empty<CombatCardKnowledgeHint>(),
                BuildAllowedActions(GuiSmokePhase.ChooseFirstNode, mapOverlayObserver, Array.Empty<CombatCardKnowledgeHint>(), mapOverlayScreenshotPath, Array.Empty<GuiSmokeHistoryEntry>()),
                new[]
                {
                    new GuiSmokeHistoryEntry(GuiSmokePhase.HandleEvent.ToString(), "event-resolved-map", null, DateTimeOffset.UtcNow.AddSeconds(-4)),
                    new GuiSmokeHistoryEntry(GuiSmokePhase.WaitMap.ToString(), "branch-map", null, DateTimeOffset.UtcNow.AddSeconds(-2)),
                },
                "Use exported reachable map points before any screenshot arrow fallback.",
                null));
            Assert(!string.Equals(mapOverlayReplay.FinalDecision.TargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase), "Map overlay replay analysis should not fall back to the red current-node arrow.");
        }
        finally
        {
            if (File.Exists(mapOverlayScreenshotPath))
            {
                File.Delete(mapOverlayScreenshotPath);
            }
        }
    }
}
