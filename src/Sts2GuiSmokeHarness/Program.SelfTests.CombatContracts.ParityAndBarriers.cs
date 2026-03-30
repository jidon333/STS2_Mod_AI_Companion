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
using static GuiSmokePromptContractSupport;
using static GuiSmokeReplayArtifactSupport;
using static GuiSmokeSceneReasoningSupport;
using static GuiSmokeStepRequestFactory;

internal static partial class Program
{
    private static void RunCombatContractsParityAndBarrierSelfTests(string combatNoOpScreenshotPath, string handleCombatParityRequestPath, string runtimeStateOnlyScreenshotPath)
    {
            var parityCombatObserver = new ObserverSummary(
                "combat",
                "combat",
                true,
                DateTimeOffset.UtcNow,
                "inv-handle-combat-parity",
                true,
                "mixed",
                "stable",
                "episode-handle-combat-parity",
                "Monster",
                "combat-targets",
                73,
                80,
                3,
                new[] { "압축벌레" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("enemy-target:2", "enemy-target", "압축벌레", "1398.24,621.6,83.52,88", true),
                },
                new[]
                {
                    new ObserverChoice("enemy-target", "압축벌레", "1398.24,621.6,83.52,88", "압축벌레", "target-source:vfx-spawn-hitbox")
                    {
                        NodeId = "enemy-target:2",
                    },
                },
                new[]
                {
                    new ObservedCombatHandCard(1, "CARD.POOR_SLEEP", "Curse", null),
                    new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", null),
                    new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", null),
                    new ObservedCombatHandCard(4, "CARD.STRIKE_IRONCLAD", "Attack", null),
                })
            {
                PublishedCurrentScreen = "combat",
                PublishedVisibleScreen = "combat",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stable",
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false;node:NCombatRoom;node:NCombatUi",
                },
            };

            var parityCombatKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.POOR_SLEEP", "Curse", "None", -1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(4, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var parityFullCombatHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target 압축벌레", DateTimeOffset.UtcNow.AddSeconds(-12)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "auto-end turn", DateTimeOffset.UtcNow.AddSeconds(-11)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "wait", null, DateTimeOffset.UtcNow.AddSeconds(-10)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-9)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 4", DateTimeOffset.UtcNow.AddSeconds(-8)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 4", DateTimeOffset.UtcNow.AddSeconds(-7)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 4", DateTimeOffset.UtcNow.AddSeconds(-6)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-5)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 2", DateTimeOffset.UtcNow.AddSeconds(-4)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow.AddSeconds(-3)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "combat-noop", "combat lane slot 2", DateTimeOffset.UtcNow.AddSeconds(-2)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 2", DateTimeOffset.UtcNow.AddSeconds(-1)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 3", DateTimeOffset.UtcNow),
            };
            var paritySerializedHistory = BuildSerializedStepHistory(GuiSmokePhase.HandleCombat, parityFullCombatHistory);
            Assert(paritySerializedHistory.Count == HandleCombatContextSupport.SerializedHistoryWindow, "HandleCombat request serialization should retain the last 12 combat entries for parity rebuilds.");
            Assert(paritySerializedHistory.All(entry => string.Equals(entry.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)), "HandleCombat request serialization should only persist combat-phase history.");
            var parityObserverState = new ObserverState(parityCombatObserver, null, null, null);
            var parityAllowedActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                parityObserverState,
                parityCombatKnowledge,
                combatNoOpScreenshotPath,
                paritySerializedHistory);
            Assert(!parityAllowedActions.Contains("select attack slot 2", StringComparer.OrdinalIgnoreCase), "Step19-like parity regression should keep blocked attack slot 2 closed.");
            Assert(!parityAllowedActions.Contains("select attack slot 4", StringComparer.OrdinalIgnoreCase), "Step19-like parity regression should keep blocked attack slot 4 closed.");
            var parityRequest = new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                19,
                GuiSmokePhase.HandleCombat.ToString(),
                "Preserve enough combat history for live/rebuild parity.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0002",
                2,
                3,
                false,
                "tactical",
                null,
                parityCombatObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                parityCombatKnowledge,
                parityAllowedActions,
                paritySerializedHistory,
                BuildFailureModeHintCore(
                    GuiSmokePhase.HandleCombat,
                    parityObserverState,
                    parityCombatKnowledge,
                    combatNoOpScreenshotPath,
                    paritySerializedHistory),
                null);
            File.WriteAllText(handleCombatParityRequestPath, JsonSerializer.Serialize(parityRequest, GuiSmokeShared.JsonOptions), Encoding.UTF8);
            var parityNonRebuild = LoadReplayRequest(handleCombatParityRequestPath, fullRequestRebuild: false).Request;
            var parityRebuilt = LoadReplayRequest(handleCombatParityRequestPath, fullRequestRebuild: true).Request;
            Assert(parityNonRebuild.AllowedActions.OrderBy(static action => action, StringComparer.OrdinalIgnoreCase)
                   .SequenceEqual(parityRebuilt.AllowedActions.OrderBy(static action => action, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase),
                "Saved-vs-rebuilt HandleCombat parity should preserve the same allowlist on the synthetic step19-like request.");
            var parityNonRebuildDecision = EvaluateAutoDecisionWithDiagnostics(handleCombatParityRequestPath, parityNonRebuild).Decision;
            var parityRebuiltDecision = EvaluateAutoDecisionWithDiagnostics(handleCombatParityRequestPath, parityRebuilt).Decision;
            Assert(CombatDecisionContract.TryMapSemanticAction(parityNonRebuild, parityNonRebuildDecision, out var parityNonRebuildSemantic), "Saved synthetic HandleCombat parity request should map to a combat semantic action.");
            Assert(CombatDecisionContract.TryMapSemanticAction(parityRebuilt, parityRebuiltDecision, out var parityRebuiltSemantic), "Rebuilt synthetic HandleCombat parity request should map to a combat semantic action.");
            Assert(string.Equals(parityNonRebuildSemantic, parityRebuiltSemantic, StringComparison.OrdinalIgnoreCase), "Step19-like synthetic parity regression should keep saved and rebuilt final semantics aligned.");
            Assert(!string.Equals(parityNonRebuildSemantic, "select attack slot 2", StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(parityNonRebuildSemantic, "select attack slot 4", StringComparison.OrdinalIgnoreCase),
                "Step19-like synthetic parity regression should not regress to a blocked attack-slot semantic.");
            Assert(!string.Equals(parityRebuiltDecision.TargetLabel, "combat select attack slot 2", StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(parityRebuiltDecision.TargetLabel, "combat select attack slot 4", StringComparison.OrdinalIgnoreCase),
                "Step19-like synthetic parity regression should not rebuild to illegal blocked attack slots.");

            var combatFastPathContext = CreateStepAnalysisContext(
                GuiSmokePhase.HandleCombat,
                parityObserverState,
                combatNoOpScreenshotPath,
                paritySerializedHistory,
                parityCombatKnowledge);
            Assert(combatFastPathContext.UseCombatFastPath, "Explicit combat authority without contradictory room ownership should enable the combat fast path.");
            var combatFastPathSignature = ComputeSceneSignatureCore(combatNoOpScreenshotPath, parityObserverState, GuiSmokePhase.HandleCombat, combatFastPathContext);
            Assert(combatFastPathSignature.Contains("combat:fast-path", StringComparison.OrdinalIgnoreCase), "Combat fast path scene signatures should mark the fast-path contract explicitly.");
            Assert(!combatFastPathSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase), "Combat fast path scene signatures should not add reward/map contamination layers.");
            Assert(!combatFastPathSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase), "Combat fast path scene signatures should not add screenshot map-arrow contamination.");
            var parityContextAnalysis = AutoDecisionProvider.Analyze(parityRequest, analysisContext: CreateRequestAnalysisContext(parityRequest));
            Assert(CombatDecisionContract.TryMapSemanticAction(parityRequest, parityContextAnalysis.FinalDecision, out var parityContextSemantic), "Context-backed combat analysis should still map to a legal combat semantic action.");
            Assert(string.Equals(parityContextSemantic, parityNonRebuildSemantic, StringComparison.OrdinalIgnoreCase), "Context-backed combat analysis should preserve the same HandleCombat parity semantic as saved/rebuilt replay analysis.");

            WindowCaptureTarget BuildObserverOnlyCombatWindow()
            {
                return new WindowCaptureTarget(IntPtr.Zero, "self-test-combat", new Rectangle(1, 32, 1280, 720), false, false);
            }

            GuiSmokeStepRequest BuildObserverOnlyCombatRequest(
                string attemptId,
                int stepNumber,
                ObserverSummary observerSummary,
                IReadOnlyList<CombatCardKnowledgeHint> combatKnowledge,
                IReadOnlyList<GuiSmokeHistoryEntry> combatHistory)
            {
                var observerState = new ObserverState(observerSummary, null, null, null);
                var analysisContext = CreateObserverOnlyAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    observerState,
                    combatHistory,
                    combatKnowledge,
                    new WindowBounds(1, 32, 1280, 720));
                return CreateStepRequest(
                    "run",
                    "boot-to-long-run",
                    stepNumber,
                    GuiSmokePhase.HandleCombat,
                    Path.Combine(Path.GetTempPath(), $"{attemptId}.screen.png"),
                    BuildObserverOnlyCombatWindow(),
                    observerState,
                    combatHistory,
                    Directory.GetCurrentDirectory(),
                    Path.GetTempPath(),
                    attemptId,
                    1,
                    analysisContext,
                    null);
            }

            var observerOnlyAttackObserver = parityCombatObserver with
            {
                InventoryId = "inv-handle-combat-observer-attack",
                CurrentChoices = new[] { "2턴 종료" },
                ActionNodes = new[]
                {
                    new ObserverActionNode("end-turn", "button", "2턴 종료", "1604,846,220,90", true),
                },
                Choices = Array.Empty<ObserverChoice>(),
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(1, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    new ObservedCombatHandCard(2, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                },
            };
            var observerOnlyAttackKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            };
            var observerOnlyAttackContext = CreateObserverOnlyAnalysisContext(
                GuiSmokePhase.HandleCombat,
                new ObserverState(observerOnlyAttackObserver, null, null, null),
                Array.Empty<GuiSmokeHistoryEntry>(),
                observerOnlyAttackKnowledge,
                new WindowBounds(1, 32, 1280, 720));
            var observerOnlyAttackPolicy = GuiSmokeStepScreenshotPolicy.Evaluate(2, false, observerOnlyAttackContext);
            Assert(!observerOnlyAttackPolicy.NeedsScreenshot
                   && string.Equals(observerOnlyAttackPolicy.SkipReason, "combat-observer-attack-slot", StringComparison.OrdinalIgnoreCase),
                "Combat screenshot policy should skip capture when observer/knowledge already expose a playable attack slot.");
            var observerOnlyAttackRequest = BuildObserverOnlyCombatRequest(
                "observer-only-attack",
                2,
                observerOnlyAttackObserver,
                observerOnlyAttackKnowledge,
                Array.Empty<GuiSmokeHistoryEntry>());
            Assert(string.IsNullOrWhiteSpace(observerOnlyAttackRequest.ScreenshotPath), "Observer-only combat requests should leave screenshot path empty.");
            var observerOnlyAttackDecision = AutoDecisionProvider.Analyze(
                observerOnlyAttackRequest,
                analysisContext: CreateRequestAnalysisContext(observerOnlyAttackRequest)).FinalDecision;
            Assert(CombatDecisionContract.TryMapSemanticAction(observerOnlyAttackRequest, observerOnlyAttackDecision, out var observerOnlyAttackSemantic)
                   && string.Equals(observerOnlyAttackSemantic, "select attack slot 1", StringComparison.OrdinalIgnoreCase),
                "Observer-only combat fast path should still select a playable attack slot without screenshot analysis.");

            var observerOnlyTargetHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 1", DateTimeOffset.UtcNow.AddSeconds(-1)),
            };
            var observerOnlyTargetObserver = observerOnlyAttackObserver with
            {
                InventoryId = "inv-handle-combat-observer-target",
                ChoiceExtractorPath = "combat-targets",
                CurrentChoices = new[] { "Jaw Worm" },
                ActionNodes = new[]
                {
                    new ObserverActionNode("enemy-target:1", "enemy-target", "Jaw Worm", "967,433,84,88", true),
                    new ObserverActionNode("end-turn", "button", "2턴 종료", "1604,846,220,90", true),
                },
                Choices = new[]
                {
                    new ObserverChoice("enemy-target", "Jaw Worm", "967,433,84,88", "Jaw Worm", "target-source:vfx-spawn-hitbox")
                    {
                        NodeId = "enemy-target:1",
                    },
                },
                Meta = new Dictionary<string, string?>(observerOnlyAttackObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "true",
                    ["combatSelectedCardSlot"] = "1",
                    ["combatSelectedCardType"] = "Attack",
                    ["combatSelectedCardTargetType"] = "AnyEnemy",
                    ["combatTargetingInProgress"] = "true",
                    ["combatTargetableEnemyCount"] = "1",
                    ["combatTargetableEnemyIds"] = "enemy-target:1",
                    ["combatHittableEnemyCount"] = "1",
                    ["combatHittableEnemyIds"] = "enemy-target:1",
                },
            };
            var observerOnlyTargetContext = CreateObserverOnlyAnalysisContext(
                GuiSmokePhase.HandleCombat,
                new ObserverState(observerOnlyTargetObserver, null, null, null),
                observerOnlyTargetHistory,
                observerOnlyAttackKnowledge,
                new WindowBounds(1, 32, 1280, 720));
            var observerOnlyTargetPolicy = GuiSmokeStepScreenshotPolicy.Evaluate(3, false, observerOnlyTargetContext);
            Assert(!observerOnlyTargetPolicy.NeedsScreenshot
                   && string.Equals(observerOnlyTargetPolicy.SkipReason, "combat-explicit-target-runtime", StringComparison.OrdinalIgnoreCase),
                "Combat screenshot policy should skip capture when runtime/observer already expose an explicit enemy target.");
            var observerOnlyTargetRequest = BuildObserverOnlyCombatRequest(
                "observer-only-target",
                3,
                observerOnlyTargetObserver,
                observerOnlyAttackKnowledge,
                observerOnlyTargetHistory);
            Assert(observerOnlyTargetRequest.AllowedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase),
                $"Observer-only combat request should still open click enemy in the allowlist. allowed=[{string.Join(", ", observerOnlyTargetRequest.AllowedActions)}]");
            var observerOnlyTargetReplayContext = CreateRequestAnalysisContext(observerOnlyTargetRequest);
            Assert(observerOnlyTargetReplayContext.PendingCombatSelection?.Kind == AutoCombatCardKind.AttackLike,
                $"Observer-only combat replay context should preserve the pending attack selection. pending={observerOnlyTargetReplayContext.PendingCombatSelection?.Kind.ToString() ?? "null"}");
            var observerOnlyTargetDecision = AutoDecisionProvider.Analyze(
                observerOnlyTargetRequest,
                analysisContext: observerOnlyTargetReplayContext).FinalDecision;
            var observerOnlyTargetMapped = CombatDecisionContract.TryMapSemanticAction(observerOnlyTargetRequest, observerOnlyTargetDecision, out var observerOnlyTargetSemantic);
            Assert(observerOnlyTargetMapped
                   && string.Equals(observerOnlyTargetSemantic, "click enemy", StringComparison.OrdinalIgnoreCase),
                $"Observer-only combat fast path should still click the explicit enemy target without screenshot analysis. actual={observerOnlyTargetDecision.TargetLabel ?? observerOnlyTargetDecision.ActionKind ?? observerOnlyTargetDecision.Status} semantic={observerOnlyTargetSemantic ?? "null"}");

            GuiSmokeStepRequest BuildBarrierRequest(
                string stepId,
                int stepNumber,
                ObserverSummary barrierObserver,
                IReadOnlyList<CombatCardKnowledgeHint> barrierKnowledge,
                IReadOnlyList<string> barrierAllowedActions,
                IReadOnlyList<GuiSmokeHistoryEntry> barrierHistory,
                string hint)
            {
                return new GuiSmokeStepRequest(
                    "run",
                    "boot-to-long-run",
                    stepNumber,
                    GuiSmokePhase.HandleCombat.ToString(),
                    "Combat barrier self-test request.",
                    DateTimeOffset.UtcNow,
                    runtimeStateOnlyScreenshotPath,
                    new WindowBounds(1, 32, 1280, 720),
                    "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                    stepId,
                    1,
                    3,
                    false,
                    "tactical",
                    null,
                    barrierObserver,
                    Array.Empty<KnownRecipeHint>(),
                    Array.Empty<EventKnowledgeCandidate>(),
                    barrierKnowledge,
                    barrierAllowedActions.ToArray(),
                    barrierHistory,
                    hint,
                    null);
            }

            var combatBarrierCapturedAt = DateTimeOffset.UtcNow;
            var enemyClickBarrierObserver = new ObserverSummary(
                "combat",
                "combat",
                true,
                combatBarrierCapturedAt,
                "inv-enemy-click-barrier",
                true,
                "hook",
                "stable",
                "episode-enemy-click-barrier",
                "Monster",
                "generic",
                70,
                80,
                1,
                new[] { "3턴 종료" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true),
                },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                })
            {
                PublishedCurrentScreen = "combat",
                PublishedVisibleScreen = "combat",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stable",
                SnapshotVersion = 10,
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "true",
                    ["combatSelectedCardSlot"] = "2",
                    ["combatSelectedCardType"] = "Attack",
                    ["combatSelectedCardTargetType"] = "AnyEnemy",
                    ["combatTargetingInProgress"] = "true",
                    ["combatHistoryStartedCount"] = "1",
                    ["combatHistoryFinishedCount"] = "0",
                    ["combatInteractionRevision"] = "1:0:true:true:2",
                },
            };
            var enemyBarrierKnowledge = new[]
            {
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
            };
            var enemyBarrierSeedRequest = BuildBarrierRequest(
                "0003",
                30,
                enemyClickBarrierObserver,
                enemyBarrierKnowledge,
                new[] { "wait" },
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Seed enemy click barrier metadata.");
            var enemyBarrierMetadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(
                enemyBarrierSeedRequest,
                new GuiSmokeStepDecision("act", "click", null, 0.5, 0.5, "combat enemy target Cultist", "seed", 0.9, "combat", 300, true, null));
            var enemyBarrierHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "click", "combat enemy target Cultist", DateTimeOffset.UtcNow)
                {
                    Metadata = enemyBarrierMetadata,
                },
            };
            var enemyBarrierActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(enemyClickBarrierObserver, null, null, null),
                enemyBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                enemyBarrierHistory);
            Assert(enemyBarrierActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase), "EnemyClick hard barrier should collapse the allowlist to wait until a fresh authoritative snapshot arrives.");
            var enemyBarrierRequest = BuildBarrierRequest(
                "0004",
                31,
                enemyClickBarrierObserver,
                enemyBarrierKnowledge,
                enemyBarrierActions,
                enemyBarrierHistory,
                "EnemyClick barrier should wait locally before another provider decision.");
            var enemyBarrierDecision = AutoDecisionProvider.Decide(enemyBarrierRequest);
            Assert(string.Equals(enemyBarrierDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                   && enemyBarrierDecision.Reason?.Contains("combat barrier wait", StringComparison.OrdinalIgnoreCase) == true,
                "EnemyClick hard barrier should synthesize a combat barrier wait decision.");
            Assert(CombatBarrierSupport.TryClassifyWaitPlateau(
                    enemyBarrierRequest,
                    CreateRequestAnalysisContext(enemyBarrierRequest),
                    4,
                    out var barrierWaitCause,
                    out _)
                   && string.Equals(barrierWaitCause, "combat-barrier-wait-plateau", StringComparison.OrdinalIgnoreCase),
                "Repeated unresolved hard-barrier waits should classify as combat-barrier-wait-plateau.");

            var freshEnemyBarrierObserver = enemyClickBarrierObserver with
            {
                CapturedAt = combatBarrierCapturedAt.AddMilliseconds(250),
                InventoryId = "inv-enemy-click-barrier-fresh",
                SnapshotVersion = 11,
                Meta = new Dictionary<string, string?>(enemyClickBarrierObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "false",
                    ["combatTargetingInProgress"] = "false",
                    ["combatHistoryFinishedCount"] = "1",
                    ["combatInteractionRevision"] = "1:1:false:false:none",
                    ["combatLastCardPlayFinishedCardId"] = "CARD.STRIKE_IRONCLAD",
                },
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                },
            };
            Assert(!CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    new ObserverState(freshEnemyBarrierObserver, null, null, null),
                    runtimeStateOnlyScreenshotPath,
                    enemyBarrierHistory,
                    new[] { new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test") })
                    .CombatBarrierEvaluation.IsActive,
                "EnemyClick barrier should release once a fresh finished-card snapshot arrives.");

            var attackBarrierCapturedAt = DateTimeOffset.UtcNow;
            var attackBarrierObserver = new ObserverSummary(
                "combat",
                "combat",
                true,
                attackBarrierCapturedAt,
                "inv-attack-barrier",
                true,
                "hook",
                "stable",
                "episode-attack-barrier",
                "Monster",
                "combat",
                74,
                80,
                2,
                new[] { "3턴 종료" },
                Array.Empty<string>(),
                new[] { new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true) },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    new ObservedCombatHandCard(4, "CARD.BASH", "Attack", 2),
                })
            {
                PublishedCurrentScreen = "combat",
                PublishedVisibleScreen = "combat",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stable",
                SnapshotVersion = 20,
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "true",
                    ["combatSelectedCardSlot"] = "2",
                    ["combatSelectedCardType"] = "Attack",
                    ["combatSelectedCardTargetType"] = "AnyEnemy",
                    ["combatTargetingInProgress"] = "false",
                    ["combatHistoryStartedCount"] = "3",
                    ["combatHistoryFinishedCount"] = "2",
                    ["combatInteractionRevision"] = "3:2:true:false:2",
                },
            };
            var attackBarrierKnowledge = new[]
            {
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(4, "CARD.BASH", "Attack", "AnyEnemy", 2, "self-test"),
            };
            var attackBarrierSeedRequest = BuildBarrierRequest("0005", 32, attackBarrierObserver, attackBarrierKnowledge, new[] { "wait" }, Array.Empty<GuiSmokeHistoryEntry>(), "Seed attack barrier metadata.");
            var attackBarrierHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select attack slot 2", DateTimeOffset.UtcNow)
                {
                    Metadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(
                        attackBarrierSeedRequest,
                        new GuiSmokeStepDecision("act", "press-key", "2", null, null, "combat select attack slot 2", "seed", 0.8, "combat", 120, true, null)),
                },
            };
            var attackBarrierActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(attackBarrierObserver, null, null, null),
                attackBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                attackBarrierHistory);
            Assert(!attackBarrierActions.Contains("select attack slot 2", StringComparer.OrdinalIgnoreCase), "AttackSelect barrier should suppress same-slot attack reissue while unresolved.");
            Assert(!attackBarrierActions.Contains("select attack slot 4", StringComparer.OrdinalIgnoreCase), "AttackSelect barrier should keep alternate legal attack lanes closed until the selected lane resolves.");
            Assert(!attackBarrierActions.Contains("click end turn", StringComparer.OrdinalIgnoreCase), "AttackSelect barrier should keep end-turn closed while the selected attack lane is unresolved.");
            Assert(attackBarrierActions.Contains("right-click cancel selected card", StringComparer.OrdinalIgnoreCase), "AttackSelect barrier should expose an explicit cancel lane while the selected attack remains unresolved.");

            var attackBarrierReleasedObserver = attackBarrierObserver with
            {
                CapturedAt = attackBarrierCapturedAt.AddMilliseconds(250),
                InventoryId = "inv-attack-barrier-released",
                SnapshotVersion = 21,
                ChoiceExtractorPath = "combat-targets",
                CurrentChoices = new[] { "Cultist" },
                ActionNodes = new[]
                {
                    new ObserverActionNode("enemy-target:cultist:1", "enemy-target", "Cultist", "930,210,180,250", true),
                },
                Choices = new[]
                {
                    new ObserverChoice("enemy-target", "Cultist", "930,210,180,250", "MONSTER.CULTIST", "Enemy target")
                    {
                        NodeId = "enemy-target:cultist:1",
                        Enabled = true,
                    },
                },
                Meta = new Dictionary<string, string?>(attackBarrierObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "true",
                    ["combatSelectedCardSlot"] = "2",
                    ["combatSelectedCardType"] = "Attack",
                    ["combatSelectedCardTargetType"] = "AnyEnemy",
                    ["combatTargetingInProgress"] = "true",
                    ["combatTargetableEnemyCount"] = "1",
                    ["combatHittableEnemyCount"] = "1",
                    ["combatInteractionRevision"] = "3:2:true:true:2",
                },
            };
            var attackBarrierReleasedActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(attackBarrierReleasedObserver, null, null, null),
                attackBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                attackBarrierHistory);
            Assert(attackBarrierReleasedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "AttackSelect barrier should release as soon as explicit target authority appears.");

            var nonEnemyBarrierCapturedAt = DateTimeOffset.UtcNow;
            var nonEnemyBarrierObserver = new ObserverSummary(
                "combat",
                "combat",
                true,
                nonEnemyBarrierCapturedAt,
                "inv-non-enemy-barrier",
                true,
                "hook",
                "stable",
                "episode-non-enemy-barrier",
                "Monster",
                "combat",
                78,
                80,
                1,
                new[] { "3턴 종료" },
                Array.Empty<string>(),
                new[] { new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true) },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                })
            {
                PublishedCurrentScreen = "combat",
                PublishedVisibleScreen = "combat",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stable",
                SnapshotVersion = 30,
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "true",
                    ["combatTargetingInProgress"] = "false",
                    ["combatHistoryStartedCount"] = "4",
                    ["combatHistoryFinishedCount"] = "2",
                    ["combatInteractionRevision"] = "4:2:true:false:none",
                },
            };
            var nonEnemyBarrierKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            };
            var nonEnemyBarrierSeedRequest = BuildBarrierRequest("0006", 33, nonEnemyBarrierObserver, nonEnemyBarrierKnowledge, new[] { "wait" }, Array.Empty<GuiSmokeHistoryEntry>(), "Seed non-enemy barrier metadata.");
            var nonEnemyBarrierHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "combat select non-enemy slot 1", DateTimeOffset.UtcNow)
                {
                    Metadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(
                        nonEnemyBarrierSeedRequest,
                        new GuiSmokeStepDecision("act", "press-key", "1", null, null, "combat select non-enemy slot 1", "seed", 0.8, "combat", 120, true, null)),
                },
            };
            var nonEnemyBarrierActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(nonEnemyBarrierObserver, null, null, null),
                nonEnemyBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                nonEnemyBarrierHistory);
            Assert(!nonEnemyBarrierActions.Contains("select non-enemy slot 1", StringComparer.OrdinalIgnoreCase), "NonEnemySelect barrier should suppress same-slot non-enemy reissue while unresolved.");
            Assert(!nonEnemyBarrierActions.Contains("select non-enemy slot 3", StringComparer.OrdinalIgnoreCase), "NonEnemySelect barrier should keep alternate non-enemy slots closed until the selected lane resolves.");
            Assert(!nonEnemyBarrierActions.Contains("click end turn", StringComparer.OrdinalIgnoreCase), "NonEnemySelect barrier should keep end-turn closed while the selected lane is unresolved.");
            Assert(
                nonEnemyBarrierActions.Contains("right-click cancel selected card", StringComparer.OrdinalIgnoreCase),
                $"NonEnemySelect barrier should expose an explicit cancel lane while the selected non-enemy card remains unresolved. actual=[{string.Join(", ", nonEnemyBarrierActions)}]");
            var nonEnemyBarrierDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                33,
                GuiSmokePhase.HandleCombat.ToString(),
                "Do not end turn while a non-enemy lane remains unresolved.",
                DateTimeOffset.UtcNow,
                runtimeStateOnlyScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0006",
                1,
                3,
                false,
                "tactical",
                null,
                nonEnemyBarrierObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                nonEnemyBarrierKnowledge,
                nonEnemyBarrierActions,
                nonEnemyBarrierHistory,
                "Do not end turn while a non-enemy lane remains unresolved.",
                null));
            Assert(string.Equals(nonEnemyBarrierDecision.Status, "wait", StringComparison.OrdinalIgnoreCase), "Unresolved non-enemy barriers should defer with a wait decision instead of forcing end turn.");

            var nonEnemyBarrierSupersededObserver = nonEnemyBarrierObserver with
            {
                InventoryId = "inv-non-enemy-barrier-superseded",
                SnapshotVersion = 51,
                ChoiceExtractorPath = "combat-targets",
                CurrentChoices = new[] { "Cultist" },
                ActionNodes = new[]
                {
                    new ObserverActionNode("enemy-target:1", "enemy-target", "Cultist", "967,433,84,88", true),
                    new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true),
                },
                Choices = new[]
                {
                    new ObserverChoice("enemy-target", "Cultist", "967,433,84,88", "Cultist", "target-source:vfx-spawn-hitbox")
                    {
                        NodeId = "enemy-target:1",
                    },
                },
                CombatHand = new[]
                {
                    new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                    new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                },
                Meta = new Dictionary<string, string?>(nonEnemyBarrierObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatCardPlayPending"] = "true",
                    ["combatSelectedCardId"] = "CARD.STRIKE_IRONCLAD",
                    ["combatSelectedCardName"] = "타격",
                    ["combatSelectedCardType"] = "Attack",
                    ["combatSelectedCardTargetType"] = "AnyEnemy",
                    ["combatSelectedCardSlot"] = "2",
                    ["combatAwaitingPlayCount"] = "1",
                    ["combatAwaitingPlaySlots"] = "2",
                    ["combatTargetingInProgress"] = "true",
                    ["combatValidTargetsType"] = "AnyEnemy",
                    ["combatTargetableEnemyCount"] = "1",
                    ["combatTargetableEnemyIds"] = "Cultist",
                    ["combatHittableEnemyCount"] = "1",
                    ["combatHittableEnemyIds"] = "Cultist",
                    ["combatInteractionRevision"] = "6:5:true:true:2",
                },
            };
            var nonEnemyBarrierSupersededKnowledge = new[]
            {
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            };
            var nonEnemyBarrierSupersededContext = CreateStepAnalysisContext(
                GuiSmokePhase.HandleCombat,
                new ObserverState(nonEnemyBarrierSupersededObserver, null, null, null),
                runtimeStateOnlyScreenshotPath,
                nonEnemyBarrierHistory,
                nonEnemyBarrierSupersededKnowledge);
            Assert(!nonEnemyBarrierSupersededContext.CombatBarrierEvaluation.IsActive, "NonEnemySelect barrier should release once explicit attack-target runtime authority supersedes the same-slot lane.");
            Assert(nonEnemyBarrierSupersededContext.CombatMicroStage.Kind == CombatMicroStageKind.ResolvingAttackTarget, "Superseded non-enemy lane should rebuild as an attack-target stage when runtime targeting takes ownership.");
            var nonEnemyBarrierSupersededActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(nonEnemyBarrierSupersededObserver, null, null, null),
                nonEnemyBarrierSupersededKnowledge,
                runtimeStateOnlyScreenshotPath,
                nonEnemyBarrierHistory);
            Assert(nonEnemyBarrierSupersededActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Superseded non-enemy lane should reopen the explicit enemy target lane.");
            Assert(!nonEnemyBarrierSupersededActions.Contains("click end turn", StringComparer.OrdinalIgnoreCase), "Superseded non-enemy lane should not reopen end turn while attack targeting is active.");
            var nonEnemyBarrierSupersededRequest = BuildBarrierRequest(
                "0006b",
                33,
                nonEnemyBarrierSupersededObserver,
                nonEnemyBarrierSupersededKnowledge,
                nonEnemyBarrierSupersededActions,
                nonEnemyBarrierHistory,
                "Superseded non-enemy lane should hand off to explicit attack targeting.");
            var nonEnemyBarrierSupersededDecision = AutoDecisionProvider.Decide(nonEnemyBarrierSupersededRequest);
            Assert(CombatDecisionContract.TryMapSemanticAction(nonEnemyBarrierSupersededRequest, nonEnemyBarrierSupersededDecision, out var nonEnemyBarrierSupersededSemantic)
                   && string.Equals(nonEnemyBarrierSupersededSemantic, "click enemy", StringComparison.OrdinalIgnoreCase),
                "Superseded non-enemy lane should drive explicit enemy targeting instead of waiting for non-enemy confirm.");

            var nonEnemyConfirmResolvingObserver = nonEnemyBarrierObserver with
            {
                InventoryId = "inv-non-enemy-confirm-resolving",
                SnapshotVersion = 52,
                PlayerEnergy = 0,
                Meta = new Dictionary<string, string?>(nonEnemyBarrierObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatRoundNumber"] = "2",
                    ["combatPlayerActionsDisabled"] = "false",
                    ["combatEndingPlayerTurnPhaseOne"] = "false",
                    ["combatEndingPlayerTurnPhaseTwo"] = "false",
                    ["combatCardPlayPending"] = "false",
                    ["combatSelectedCardSlot"] = null,
                    ["combatAwaitingPlaySlots"] = null,
                    ["combatTargetingInProgress"] = "false",
                    ["combatTargetableEnemyCount"] = "0",
                    ["combatHistoryStartedCount"] = "5",
                    ["combatHistoryFinishedCount"] = "4",
                    ["combatInteractionRevision"] = "5:4:false:false:none",
                    ["combatLastCardPlayStartedCardId"] = "CARD.ARMAMENTS",
                    ["combatLastCardPlayFinishedCardId"] = "CARD.STRIKE_IRONCLAD",
                    ["combatLastCardPlayFinishedTargetId"] = "enemy-1",
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false",
                },
            };
            var nonEnemyConfirmResolvingHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "confirm-non-enemy", "confirm selected non-enemy card", DateTimeOffset.UtcNow.AddMilliseconds(-300)),
            };
            var nonEnemyConfirmResolvingActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(nonEnemyConfirmResolvingObserver, null, null, null),
                nonEnemyBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                nonEnemyConfirmResolvingHistory);
            Assert(
                nonEnemyConfirmResolvingActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase),
                $"A resolving combat card play should keep combat wait-only until the started play finishes. actual=[{string.Join(", ", nonEnemyConfirmResolvingActions)}]");
            var nonEnemyConfirmResolvingDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                34,
                GuiSmokePhase.HandleCombat.ToString(),
                "Do not end turn while the confirmed non-enemy play is still resolving.",
                DateTimeOffset.UtcNow,
                runtimeStateOnlyScreenshotPath,
                new WindowBounds(1, 32, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable|combat-action-in-flight",
                "0006",
                1,
                3,
                false,
                "tactical",
                null,
                nonEnemyConfirmResolvingObserver,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                nonEnemyBarrierKnowledge,
                nonEnemyConfirmResolvingActions,
                nonEnemyConfirmResolvingHistory,
                "Confirmed non-enemy plays must drain before end turn is attempted.",
                null));
            Assert(string.Equals(nonEnemyConfirmResolvingDecision.Status, "wait", StringComparison.OrdinalIgnoreCase), "A resolving combat card play should wait instead of falling through to auto-end turn.");

            var endTurnBarrierCapturedAt = DateTimeOffset.UtcNow;
            var endTurnBarrierKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            };
            var endTurnBarrierSeedObserver = new ObserverSummary(
                "combat",
                "combat",
                true,
                endTurnBarrierCapturedAt,
                "inv-end-turn-barrier-seed",
                true,
                "hook",
                "stable",
                "episode-end-turn-barrier",
                "Monster",
                "combat",
                82,
                80,
                2,
                new[] { "2턴 종료" },
                Array.Empty<string>(),
                new[] { new ObserverActionNode("end-turn", "button", "2턴 종료", "1604,846,220,90", true) },
                Array.Empty<ObserverChoice>(),
                new[]
                {
                    new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                })
            {
                PublishedCurrentScreen = "combat",
                PublishedVisibleScreen = "combat",
                PublishedSceneReady = true,
                PublishedSceneAuthority = "hook",
                PublishedSceneStability = "stable",
                SnapshotVersion = 40,
                Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["combatRoundNumber"] = "2",
                    ["combatPlayerActionsDisabled"] = "false",
                    ["combatEndingPlayerTurnPhaseOne"] = "false",
                    ["combatEndingPlayerTurnPhaseTwo"] = "false",
                    ["combatHistoryStartedCount"] = "5",
                    ["combatHistoryFinishedCount"] = "5",
                    ["combatInteractionRevision"] = "5:5:false:false:none",
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false",
                },
            };
            var endTurnBarrierSeedRequest = BuildBarrierRequest(
                "0007",
                34,
                endTurnBarrierSeedObserver,
                endTurnBarrierKnowledge,
                new[] { "wait" },
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Seed end-turn barrier metadata.");
            var endTurnBarrierMetadata = AutoDecisionProvider.BuildHistoryMetadataForDecision(
                endTurnBarrierSeedRequest,
                new GuiSmokeStepDecision("act", "press-key", "E", null, null, "auto-end turn", "seed", 0.8, "combat", 120, true, null));
            Assert(endTurnBarrierMetadata is not null, "EndTurn barrier seed metadata should serialize the armed round.");
            var endTurnBarrierHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "press-key", "auto-end turn", DateTimeOffset.UtcNow)
                {
                    Metadata = endTurnBarrierMetadata,
                },
            };
            var endTurnBarrierAckObserver = endTurnBarrierSeedObserver with
            {
                CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(250),
                InventoryId = "inv-end-turn-barrier-ack",
                SnapshotVersion = 41,
                Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatRoundNumber"] = "2",
                    ["combatPlayerActionsDisabled"] = "true",
                    ["combatEndingPlayerTurnPhaseOne"] = "true",
                    ["combatEndingPlayerTurnPhaseTwo"] = "false",
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false",
                },
            };
            var endTurnBarrierAckActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(endTurnBarrierAckObserver, null, null, null),
                endTurnBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                endTurnBarrierHistory);
            Assert(endTurnBarrierAckActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase), "EndTurn barrier should stay wait-only after acknowledgement but before the next round reopens.");
            Assert(CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    new ObserverState(endTurnBarrierAckObserver, null, null, null),
                    runtimeStateOnlyScreenshotPath,
                    endTurnBarrierHistory,
                    endTurnBarrierKnowledge)
                    .CombatBarrierEvaluation.IsActive,
                "EndTurn barrier should remain active during the acknowledged closed-window band.");
            Assert(!CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    new ObserverState(endTurnBarrierAckObserver, null, null, null),
                    runtimeStateOnlyScreenshotPath,
                    endTurnBarrierHistory,
                    endTurnBarrierKnowledge)
                    .CombatBarrierEvaluation.OverWaitRisk,
                "Acknowledged EndTurn transit should not be flagged as an over-wait plateau risk by itself.");

            var endTurnTransitProgressObservers = new[]
            {
                endTurnBarrierSeedObserver with
                {
                    CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(250),
                    InventoryId = "inv-end-turn-transit-1",
                    SnapshotVersion = 53,
                    Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatRoundNumber"] = "1",
                        ["combatPlayerActionsDisabled"] = "true",
                        ["combatEndingPlayerTurnPhaseOne"] = "false",
                        ["combatEndingPlayerTurnPhaseTwo"] = "true",
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false",
                    },
                },
                endTurnBarrierSeedObserver with
                {
                    CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(500),
                    InventoryId = "inv-end-turn-transit-2",
                    SnapshotVersion = 54,
                    Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatRoundNumber"] = "1",
                        ["combatPlayerActionsDisabled"] = "true",
                        ["combatEndingPlayerTurnPhaseOne"] = "false",
                        ["combatEndingPlayerTurnPhaseTwo"] = "true",
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=true;CombatManager.IsEnding=false",
                    },
                },
                endTurnBarrierSeedObserver with
                {
                    CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(750),
                    InventoryId = "inv-end-turn-transit-3",
                    SnapshotVersion = 55,
                    Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatRoundNumber"] = "2",
                        ["combatPlayerActionsDisabled"] = "false",
                        ["combatEndingPlayerTurnPhaseOne"] = "false",
                        ["combatEndingPlayerTurnPhaseTwo"] = "true",
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=true;CombatManager.IsEnding=false",
                    },
                },
                endTurnBarrierSeedObserver with
                {
                    CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(1000),
                    InventoryId = "inv-end-turn-transit-4",
                    SnapshotVersion = 56,
                    Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                    {
                        ["combatRoundNumber"] = "2",
                        ["combatPlayerActionsDisabled"] = "false",
                        ["combatEndingPlayerTurnPhaseOne"] = "false",
                        ["combatEndingPlayerTurnPhaseTwo"] = "true",
                        ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=true;CombatManager.IsEnding=false",
                    },
                },
            };
            string? lastEndTurnTransitFingerprint = null;
            var endTurnTransitWaitCount = 0;
            for (var index = 0; index < endTurnTransitProgressObservers.Length; index += 1)
            {
                var transitObserverState = new ObserverState(endTurnTransitProgressObservers[index], null, null, null);
                var transitActions = BuildAllowedActions(
                    GuiSmokePhase.HandleCombat,
                    transitObserverState,
                    endTurnBarrierKnowledge,
                    runtimeStateOnlyScreenshotPath,
                    endTurnBarrierHistory);
                Assert(transitActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase), "Acknowledged EndTurn transit should remain wait-only before the player window reopens.");
                var transitRequest = BuildBarrierRequest(
                    "0007-transit",
                    40 + index,
                    endTurnTransitProgressObservers[index],
                    endTurnBarrierKnowledge,
                    transitActions,
                    endTurnBarrierHistory,
                    "Acknowledged EndTurn transit progress should remain a safe wait.");
                var transitContext = CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    transitObserverState,
                    runtimeStateOnlyScreenshotPath,
                    endTurnBarrierHistory,
                    endTurnBarrierKnowledge);
                Assert(transitContext.CombatBarrierEvaluation.IsActive
                       && !transitContext.CombatBarrierEvaluation.OverWaitRisk,
                    "Acknowledged EndTurn transit should stay active without being classified as an over-wait risk.");
                var transitFingerprint = BuildDecisionWaitFingerprint(
                    GuiSmokePhase.HandleCombat,
                    transitRequest.SceneSignature,
                    transitObserverState,
                    transitContext);
                endTurnTransitWaitCount = string.Equals(lastEndTurnTransitFingerprint, transitFingerprint, StringComparison.Ordinal)
                    ? endTurnTransitWaitCount + 1
                    : 1;
                lastEndTurnTransitFingerprint = transitFingerprint;
                Assert(
                    !CombatBarrierSupport.TryClassifyWaitPlateau(transitRequest, transitContext, endTurnTransitWaitCount, out _, out _),
                    "Acknowledged EndTurn transit with authority progress should not classify as combat-barrier-wait-plateau.");
                Assert(
                    !TryClassifyDecisionWaitPlateau(GuiSmokePhase.HandleCombat, transitObserverState, endTurnTransitWaitCount, out _, out _),
                    "Acknowledged EndTurn transit with authority progress should not classify as generic decision-wait-plateau.");
            }

            var frozenEndTurnTransitObserver = endTurnBarrierSeedObserver with
            {
                CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(1200),
                InventoryId = "inv-end-turn-transit-frozen",
                SnapshotVersion = 57,
                Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatRoundNumber"] = "2",
                    ["combatPlayerActionsDisabled"] = "false",
                    ["combatEndingPlayerTurnPhaseOne"] = "false",
                    ["combatEndingPlayerTurnPhaseTwo"] = "true",
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=false;CombatManager.IsEnemyTurnStarted=true;CombatManager.IsEnding=false",
                },
            };
            string? lastFrozenEndTurnTransitFingerprint = null;
            var frozenEndTurnTransitWaitCount = 0;
            GuiSmokeStepRequest? frozenTransitRequest = null;
            GuiSmokeStepAnalysisContext? frozenTransitContext = null;
            for (var index = 0; index < 4; index += 1)
            {
                var frozenObserverState = new ObserverState(frozenEndTurnTransitObserver, null, null, null);
                var frozenActions = BuildAllowedActions(
                    GuiSmokePhase.HandleCombat,
                    frozenObserverState,
                    endTurnBarrierKnowledge,
                    runtimeStateOnlyScreenshotPath,
                    endTurnBarrierHistory);
                frozenTransitRequest = BuildBarrierRequest(
                    "0007-frozen",
                    50 + index,
                    frozenEndTurnTransitObserver,
                    endTurnBarrierKnowledge,
                    frozenActions,
                    endTurnBarrierHistory,
                    "Frozen acknowledged EndTurn transit may still stall.");
                frozenTransitContext = CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    frozenObserverState,
                    runtimeStateOnlyScreenshotPath,
                    endTurnBarrierHistory,
                    endTurnBarrierKnowledge);
                var frozenFingerprint = BuildDecisionWaitFingerprint(
                    GuiSmokePhase.HandleCombat,
                    frozenTransitRequest.SceneSignature,
                    frozenObserverState,
                    frozenTransitContext);
                frozenEndTurnTransitWaitCount = string.Equals(lastFrozenEndTurnTransitFingerprint, frozenFingerprint, StringComparison.Ordinal)
                    ? frozenEndTurnTransitWaitCount + 1
                    : 1;
                lastFrozenEndTurnTransitFingerprint = frozenFingerprint;
            }
            Assert(frozenTransitRequest is not null && frozenTransitContext is not null, "Frozen transit fixtures should be created for stall classification.");
            Assert(
                !CombatBarrierSupport.TryClassifyWaitPlateau(frozenTransitRequest!, frozenTransitContext!, frozenEndTurnTransitWaitCount, out _, out _),
                "Acknowledged EndTurn transit should not use combat-barrier-wait-plateau even when frozen.");
            Assert(
                TryClassifyDecisionWaitPlateau(GuiSmokePhase.HandleCombat, new ObserverState(frozenEndTurnTransitObserver, null, null, null), frozenEndTurnTransitWaitCount, out var frozenTransitCause, out _)
                && string.Equals(frozenTransitCause, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase),
                "Frozen acknowledged EndTurn transit should still be stallable via the generic wait plateau path.");

            var endTurnBarrierReopenedObserver = endTurnBarrierSeedObserver with
            {
                CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(650),
                InventoryId = "inv-end-turn-barrier-reopened",
                SnapshotVersion = 42,
                PlayerEnergy = 3,
                CurrentChoices = new[] { "3턴 종료" },
                ActionNodes = new[] { new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true) },
                Meta = new Dictionary<string, string?>(endTurnBarrierSeedObserver.Meta, StringComparer.OrdinalIgnoreCase)
                {
                    ["combatRoundNumber"] = "3",
                    ["combatPlayerActionsDisabled"] = "false",
                    ["combatEndingPlayerTurnPhaseOne"] = "false",
                    ["combatEndingPlayerTurnPhaseTwo"] = "false",
                    ["combatCrossCheck"] = "CombatManager.IsPlayPhase=true;CombatManager.IsEnemyTurnStarted=false;CombatManager.IsEnding=false",
                },
            };
            var endTurnBarrierReopenedState = new ObserverState(endTurnBarrierReopenedObserver, null, null, null);
            var endTurnBarrierReopenedContext = CreateStepAnalysisContext(
                GuiSmokePhase.HandleCombat,
                endTurnBarrierReopenedState,
                runtimeStateOnlyScreenshotPath,
                endTurnBarrierHistory,
                endTurnBarrierKnowledge);
            var endTurnBarrierReopenedActions = BuildAllowedActionsCore(
                GuiSmokePhase.HandleCombat,
                endTurnBarrierReopenedState,
                endTurnBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                endTurnBarrierHistory,
                endTurnBarrierReopenedContext);
            Assert(!endTurnBarrierReopenedActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase), "EndTurn barrier should release once the next player turn reopens on a higher round.");
            Assert(endTurnBarrierReopenedContext.CombatBarrierEvaluation.IsActive == false,
                "EndTurn barrier should be inactive after round-advanced player-turn reopen.");
            Assert(endTurnBarrierReopenedContext.CombatMicroStage.Kind == CombatMicroStageKind.PlayerActionOpen,
                "Released EndTurn barrier should rebuild as an open player-action stage instead of staying in turn-closing.");
            var endTurnBarrierReopenedRequest = BuildBarrierRequest(
                "0008",
                35,
                endTurnBarrierReopenedObserver,
                endTurnBarrierKnowledge,
                endTurnBarrierReopenedActions,
                endTurnBarrierHistory,
                "Release end-turn barrier after the next player turn reopens.");
            var endTurnBarrierReopenedDecision = AutoDecisionProvider.Decide(endTurnBarrierReopenedRequest);
            Assert(!string.Equals(endTurnBarrierReopenedDecision.Status, "wait", StringComparison.OrdinalIgnoreCase)
                   || endTurnBarrierReopenedDecision.Reason?.Contains("combat barrier wait", StringComparison.OrdinalIgnoreCase) != true,
                "EndTurn barrier release should prevent combat barrier wait reentry on the reopened player turn.");

            var endTurnObserverDriftOnlyHistory = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "observer-drift", "auto-end turn", DateTimeOffset.UtcNow),
            };
            var endTurnObserverDriftActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(endTurnBarrierSeedObserver, null, null, null),
                endTurnBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                endTurnObserverDriftOnlyHistory);
            Assert(!endTurnObserverDriftActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase), "Observer-drift auto-end-turn should not arm a hard EndTurn barrier before any input is sent.");
            Assert(!CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    new ObserverState(endTurnBarrierSeedObserver, null, null, null),
                    runtimeStateOnlyScreenshotPath,
                    endTurnObserverDriftOnlyHistory,
                    endTurnBarrierKnowledge)
                    .CombatBarrierEvaluation.IsActive,
                "Observer-drift auto-end-turn should not produce an active EndTurn barrier.");

            var inventoryOnlyDriftRequestObserver = endTurnBarrierSeedObserver with
            {
                InventoryId = "inv-end-turn-drift-request",
                CapturedAt = endTurnBarrierCapturedAt.AddMilliseconds(1100),
            };
            var inventoryOnlyDriftLatestObserver = new ObserverState(
                inventoryOnlyDriftRequestObserver with
                {
                    InventoryId = "inv-end-turn-drift-latest",
                    CapturedAt = inventoryOnlyDriftRequestObserver.CapturedAt!.Value.AddMilliseconds(250),
                    ActionNodes = new[] { new ObserverActionNode("end-turn", "button", "2턴 종료", "1605,846,220,90", true) },
                },
                null,
                null,
                null);
            Assert(
                !ShouldRecaptureForObserverDrift(
                    inventoryOnlyDriftRequestObserver,
                    inventoryOnlyDriftLatestObserver,
                    new GuiSmokeStepDecision("act", "press-key", "E", null, null, "auto-end turn", "self-test", 0.5, "combat", 120, true, null)),
                "Inventory-only observer drift should not cancel combat hotkey actuation.");
            Assert(
                ShouldRecaptureForObserverDrift(
                    inventoryOnlyDriftRequestObserver,
                    inventoryOnlyDriftLatestObserver,
                    new GuiSmokeStepDecision("act", "click", null, 0.5, 0.5, "click end turn", "self-test", 0.5, "combat", 120, true, null)),
                "Inventory-only observer drift should still recapture pointer-based combat clicks.");

            var stickyEndTurnBarrierHistory = new[]
            {
                endTurnBarrierHistory[0],
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "wait", null, DateTimeOffset.UtcNow.AddMilliseconds(100)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "wait", null, DateTimeOffset.UtcNow.AddMilliseconds(200)),
                new GuiSmokeHistoryEntry(GuiSmokePhase.HandleCombat.ToString(), "wait", null, DateTimeOffset.UtcNow.AddMilliseconds(300)),
            };
            var stickyEndTurnBarrierActions = BuildAllowedActions(
                GuiSmokePhase.HandleCombat,
                new ObserverState(endTurnBarrierReopenedObserver, null, null, null),
                endTurnBarrierKnowledge,
                runtimeStateOnlyScreenshotPath,
                stickyEndTurnBarrierHistory);
            Assert(!stickyEndTurnBarrierActions.SequenceEqual(new[] { "wait" }, StringComparer.OrdinalIgnoreCase), "EndTurn sticky release should ignore trailing wait-only history once the round has advanced and player control reopened.");
            Assert(CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    new ObserverState(endTurnBarrierReopenedObserver, null, null, null),
                    runtimeStateOnlyScreenshotPath,
                    stickyEndTurnBarrierHistory,
                    endTurnBarrierKnowledge)
                    .CombatBarrierEvaluation.IsActive == false,
                "EndTurn sticky release should keep the same auto-end-turn source from re-arming after a reopened player turn.");

            var longEndTurnWaitTailHistory = new[] { endTurnBarrierHistory[0] }
                .Concat(Enumerable.Range(1, HandleCombatContextSupport.SerializedHistoryWindow + 1)
                    .Select(waitIndex => new GuiSmokeHistoryEntry(
                        GuiSmokePhase.HandleCombat.ToString(),
                        "wait",
                        null,
                        DateTimeOffset.UtcNow.AddMilliseconds(waitIndex * 100))))
                .ToArray();
            var serializedEndTurnWaitTailHistory = BuildSerializedStepHistory(GuiSmokePhase.HandleCombat, longEndTurnWaitTailHistory);
            Assert(serializedEndTurnWaitTailHistory.Count == HandleCombatContextSupport.SerializedHistoryWindow, "HandleCombat serialization should still respect the fixed combat history window size.");
            Assert(serializedEndTurnWaitTailHistory.Count(entry => string.Equals(entry.TargetLabel, "auto-end turn", StringComparison.OrdinalIgnoreCase)) == 1, "Trailing wait-only combat history should keep the most recent end-turn barrier seed in the serialized request.");
            Assert(serializedEndTurnWaitTailHistory[0].Metadata is not null, "Preserved end-turn barrier seed should retain its barrier metadata.");
            Assert(serializedEndTurnWaitTailHistory.Skip(1).All(entry => string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)), "Trailing wait-only combat history should otherwise preserve the latest wait tail.");
            Assert(CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    new ObserverState(frozenEndTurnTransitObserver, null, null, null),
                    runtimeStateOnlyScreenshotPath,
                    serializedEndTurnWaitTailHistory,
                    endTurnBarrierKnowledge)
                    .CombatBarrierEvaluation.Kind == CombatBarrierKind.EndTurn,
                "Long acknowledged end-turn transit should rebuild the same hard EndTurn barrier after request serialization.");
            Assert(CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    new ObserverState(frozenEndTurnTransitObserver, null, null, null),
                    runtimeStateOnlyScreenshotPath,
                    serializedEndTurnWaitTailHistory,
                    endTurnBarrierKnowledge)
                    .CombatBarrierEvaluation.IsActive,
                "Long acknowledged end-turn transit should stay active instead of collapsing to generic closed-phase wait after serialization.");
            Assert(CreateStepAnalysisContext(
                    GuiSmokePhase.HandleCombat,
                    new ObserverState(endTurnBarrierReopenedObserver, null, null, null),
                    runtimeStateOnlyScreenshotPath,
                    serializedEndTurnWaitTailHistory,
                    endTurnBarrierKnowledge)
                    .CombatBarrierEvaluation.IsActive == false,
                "Preserving the end-turn seed across long wait tails must still release once the reopened player turn is observed.");

            var slotAlignmentObserver = new ObserverState(
                new ObserverSummary(
                    "combat",
                    "combat",
                    true,
                    DateTimeOffset.UtcNow,
                    "inv-slot-alignment",
                    true,
                    "mixed",
                    "stable",
                    "episode-slot-alignment",
                    "Combat",
                    "combat",
                    80,
                    80,
                    1,
                    new[] { "3턴 종료" },
                    Array.Empty<string>(),
                    new[] { new ObserverActionNode("end-turn", "button", "3턴 종료", "1604,846,220,90", true) },
                    Array.Empty<ObserverChoice>(),
                    new[]
                    {
                        new ObservedCombatHandCard(1, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                        new ObservedCombatHandCard(2, "CARD.STRIKE_IRONCLAD", "Attack", 1),
                        new ObservedCombatHandCard(3, "CARD.DEFEND_IRONCLAD", "Skill", 1),
                    })
                {
                    PublishedCurrentScreen = "combat",
                    PublishedVisibleScreen = "combat",
                    PublishedSceneReady = true,
                    PublishedSceneAuthority = "hook",
                    PublishedSceneStability = "stable",
                },
                null,
                null,
                null);
            var slotAlignmentKnowledge = new[]
            {
                new CombatCardKnowledgeHint(1, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
                new CombatCardKnowledgeHint(2, "CARD.STRIKE_IRONCLAD", "Attack", "AnyEnemy", 1, "self-test"),
                new CombatCardKnowledgeHint(3, "CARD.DEFEND_IRONCLAD", "Skill", "Self", 1, "self-test"),
            };
            var slotAllowedActions = BuildAllowedActions(GuiSmokePhase.HandleCombat, slotAlignmentObserver, slotAlignmentKnowledge, combatNoOpScreenshotPath, Array.Empty<GuiSmokeHistoryEntry>());
            Assert(slotAllowedActions.Contains("select attack slot 2", StringComparer.OrdinalIgnoreCase), "Combat allowlist should expose the actual playable attack slot from observer/knowledge alignment.");
            Assert(!slotAllowedActions.Contains("click enemy", StringComparer.OrdinalIgnoreCase), "Combat allowlist should not expose enemy targeting before an actual attack selection is confirmed.");
            var slotAlignmentDecision = AutoDecisionProvider.Decide(new GuiSmokeStepRequest(
                "run",
                "boot-to-long-run",
                8,
                GuiSmokePhase.HandleCombat.ToString(),
                "Choose a combat action from the aligned slot map.",
                DateTimeOffset.UtcNow,
                combatNoOpScreenshotPath,
                new WindowBounds(0, 0, 1280, 720),
                "phase:handlecombat|screen:combat|visible:combat|encounter:monster|ready:true|stability:stable",
                "0001",
                1,
                3,
                false,
                "tactical",
                null,
                slotAlignmentObserver.Summary,
                Array.Empty<KnownRecipeHint>(),
                Array.Empty<EventKnowledgeCandidate>(),
                slotAlignmentKnowledge,
                slotAllowedActions,
                Array.Empty<GuiSmokeHistoryEntry>(),
                "Only slot 2 is a playable attack.",
                null));
            Assert(string.Equals(slotAlignmentDecision.TargetLabel, "combat select attack slot 2", StringComparison.OrdinalIgnoreCase), "Combat decisioning should align with observer/knowledge slot 2 instead of drifting to a screenshot-only slot.");
    }
}
