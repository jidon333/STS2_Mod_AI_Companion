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
using static GuiSmokeFixtureIoSupport;
using static GuiSmokeReplayArtifactSupport;
using static GuiSmokePostActionPhaseSupport;

internal static partial class Program
{
    private static void RunCaptureReplaySelfTests()
    {
        var request = CreateBaseSelfTestRequest();
        var decision = CreateBaseSelfTestDecision();
        var workspaceRoot = Directory.GetCurrentDirectory();
        var cmdWslPath = "/mnt/c/Windows/System32/cmd.exe";
        var cmdWindowsPath = @"C:\Windows\System32\cmd.exe";

        var videoSkipRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-video-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(videoSkipRoot);
            var missingFfmpegPath = Path.Combine(videoSkipRoot, "missing-ffmpeg.exe");
            var cmdBindingFromHostPath = GuiSmokeVideoRecorder.ResolveExecutablePathForSelfTest(cmdWslPath, workspaceRoot)
                                       ?? throw new InvalidOperationException("Expected host ffmpeg-style path binding.");
            Assert(File.Exists(cmdBindingFromHostPath.HostPath), "Host binding should resolve to an executable path visible to the current runtime.");
            Assert(string.Equals(cmdBindingFromHostPath.ProcessPath, cmdWindowsPath, StringComparison.OrdinalIgnoreCase), "Host binding should produce Windows process path for child execution.");

            var cmdBindingFromWindowsPath = GuiSmokeVideoRecorder.ResolveExecutablePathForSelfTest(cmdWindowsPath, workspaceRoot)
                                          ?? throw new InvalidOperationException("Expected Windows ffmpeg-style path binding.");
            Assert(File.Exists(cmdBindingFromWindowsPath.HostPath), "Windows binding should resolve to an executable path visible to the current runtime.");
            Assert(string.Equals(cmdBindingFromWindowsPath.ProcessPath, cmdWindowsPath, StringComparison.OrdinalIgnoreCase), "Windows binding should preserve Windows process path.");

            var fallbackBinding = GuiSmokeVideoRecorder.ResolveFfmpegPathForSelfTest(
                                      new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                                      workspaceRoot,
                                      envPathOverride: null,
                                      pathOverride: string.Empty,
                                      fallbackProbePaths: new[] { cmdWindowsPath })
                                  ?? throw new InvalidOperationException("Expected ffmpeg fallback probe binding.");
            Assert(File.Exists(fallbackBinding.HostPath), "Fallback ffmpeg probe should resolve to a host-visible executable.");
            Assert(string.Equals(fallbackBinding.ProcessPath, cmdWindowsPath, StringComparison.OrdinalIgnoreCase), "Fallback ffmpeg probe should preserve Windows child process path.");

            using var recorder = GuiSmokeVideoRecorder.Create(
                workspaceRoot,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["--ffmpeg-path"] = missingFfmpegPath,
                },
                "self-test-session",
                "self-test-run",
                videoSkipRoot,
                videoSkipRoot,
                attemptId: "0001",
                scopeKind: "attempt");
            var started = recorder.TryStart(new WindowCaptureTarget(IntPtr.Zero, "self-test-window", new Rectangle(100, 200, 401, 301), false, false));
            Assert(!started, "Video recorder should skip when ffmpeg is unavailable.");
            recorder.Complete(keepRecording: true, completionReason: "self-test-video-skip");

            var metadata = TryReadJson<GuiSmokeVideoRecordingMetadata>(Path.Combine(videoSkipRoot, "video-recording.json"))
                           ?? throw new InvalidOperationException("Expected video metadata after ffmpeg skip self-test.");
            Assert(string.Equals(metadata.Status, "skipped", StringComparison.OrdinalIgnoreCase), "Missing ffmpeg should produce skipped metadata.");
            Assert(metadata.SkipReason is not null && metadata.SkipReason.Contains("ffmpeg-not-found", StringComparison.OrdinalIgnoreCase), "Video skip metadata should explain missing ffmpeg.");
            Assert(!File.Exists(metadata.OutputPath), "Skipped video capture should not leave a recording file behind.");
        }
        finally
        {
            if (Directory.Exists(videoSkipRoot))
            {
                Directory.Delete(videoSkipRoot, recursive: true);
            }
        }

        var unsupportedWindowVideoRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-video-unsupported-window-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(unsupportedWindowVideoRoot);

            using var recorder = GuiSmokeVideoRecorder.Create(
                workspaceRoot,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["--ffmpeg-path"] = cmdWindowsPath,
                },
                "self-test-session",
                "self-test-run",
                unsupportedWindowVideoRoot,
                unsupportedWindowVideoRoot,
                attemptId: "0001",
                scopeKind: "attempt");
            var started = recorder.TryStart(new WindowCaptureTarget(IntPtr.Zero, "self-test-window", new Rectangle(100, 200, 401, 301), false, false));
            Assert(!started, "Video recorder should skip when the selected executable lacks gdigrab window capture support.");
            recorder.Complete(keepRecording: true, completionReason: "self-test-unsupported-window-video");

            var metadata = TryReadJson<GuiSmokeVideoRecordingMetadata>(Path.Combine(unsupportedWindowVideoRoot, "video-recording.json"))
                           ?? throw new InvalidOperationException("Expected unsupported-window video metadata.");
            Assert(string.Equals(metadata.Status, "skipped", StringComparison.OrdinalIgnoreCase), "Unsupported ffmpeg should produce skipped video metadata.");
            Assert(metadata.SkipReason is not null && metadata.SkipReason.Contains("ffmpeg-missing-gdigrab", StringComparison.OrdinalIgnoreCase), "Unsupported ffmpeg metadata should explain missing gdigrab support.");
        }
        finally
        {
            if (Directory.Exists(unsupportedWindowVideoRoot))
            {
                Directory.Delete(unsupportedWindowVideoRoot, recursive: true);
            }
        }

        var attemptWindowVideoRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-video-attempt-window-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(attemptWindowVideoRoot);
            using var recorder = GuiSmokeVideoRecorder.Create(
                workspaceRoot,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["--ffmpeg-path"] = cmdWindowsPath,
                },
                "self-test-session",
                "self-test-run",
                attemptWindowVideoRoot,
                attemptWindowVideoRoot,
                attemptId: "0001",
                scopeKind: "attempt",
                captureSupportOverride: new GuiSmokeFfmpegCaptureSupport(true));
            var started = recorder.TryStart(new WindowCaptureTarget(new IntPtr(12345), "self-test-window", new Rectangle(100, 200, 401, 301), false, false));
            Assert(started, "Attempt review video should use single-window capture when gdigrab is available.");
            recorder.Complete(keepRecording: true, completionReason: "self-test-attempt-window-video");

            var metadata = TryReadJson<GuiSmokeVideoRecordingMetadata>(Path.Combine(attemptWindowVideoRoot, "video-recording.json"))
                           ?? throw new InvalidOperationException("Expected attempt window video metadata.");
            Assert(!string.Equals(metadata.Status, "recording", StringComparison.OrdinalIgnoreCase), "Attempt video metadata must finalize out of transient recording state.");
            Assert(string.Equals(metadata.CaptureMode, "window-hwnd", StringComparison.OrdinalIgnoreCase), "Attempt metadata should honestly report window-handle capture mode.");
            Assert(metadata.WindowScopedCaptureRequested, "Attempt window capture should preserve the requested window-scoped intent.");
            Assert(metadata.CaptureInputPattern is not null && metadata.CaptureInputPattern.Contains("hwnd=12345", StringComparison.OrdinalIgnoreCase), "Attempt window capture should record the handle-based input pattern.");
            Assert(metadata.CommandLine is not null && metadata.CommandLine.Contains("-f gdigrab", StringComparison.OrdinalIgnoreCase) && metadata.CommandLine.Contains("hwnd=12345", StringComparison.OrdinalIgnoreCase), "Attempt window capture should record the live ffmpeg gdigrab command.");
        }
        finally
        {
            if (Directory.Exists(attemptWindowVideoRoot))
            {
                Directory.Delete(attemptWindowVideoRoot, recursive: true);
            }
        }

        var bootstrapWindowVideoRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-video-bootstrap-window-self-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(bootstrapWindowVideoRoot);
            using var recorder = GuiSmokeVideoRecorder.Create(
                workspaceRoot,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["--ffmpeg-path"] = cmdWindowsPath,
                },
                "self-test-session",
                "self-test-run",
                bootstrapWindowVideoRoot,
                bootstrapWindowVideoRoot,
                attemptId: null,
                scopeKind: "bootstrap",
                captureSupportOverride: new GuiSmokeFfmpegCaptureSupport(true));
            var started = recorder.TryStart(new WindowCaptureTarget(IntPtr.Zero, "self-test-window", new Rectangle(100, 200, 401, 301), false, false));
            Assert(started, "Bootstrap recorder should use single-window capture when a window title is available.");
            recorder.Complete(keepRecording: false, completionReason: "self-test-bootstrap-window-video");

            var metadata = TryReadJson<GuiSmokeVideoRecordingMetadata>(Path.Combine(bootstrapWindowVideoRoot, "video-recording.json"))
                           ?? throw new InvalidOperationException("Expected bootstrap window video metadata.");
            Assert(!string.Equals(metadata.Status, "recording", StringComparison.OrdinalIgnoreCase), "Bootstrap video metadata must finalize out of transient recording state.");
            Assert(string.Equals(metadata.CaptureMode, "window-title", StringComparison.OrdinalIgnoreCase), "Bootstrap metadata should honestly report window-title capture mode.");
            Assert(metadata.WindowScopedCaptureRequested, "Bootstrap window capture should preserve the requested window-scoped intent.");
            Assert(metadata.CaptureModeNote is not null && metadata.CaptureModeNote.Contains("Single-window", StringComparison.OrdinalIgnoreCase), "Bootstrap metadata should explain single-window capture semantics.");
        }
        finally
        {
            if (Directory.Exists(bootstrapWindowVideoRoot))
            {
                Directory.Delete(bootstrapWindowVideoRoot, recursive: true);
            }
        }

        ValidateDecision(
            GuiSmokePhase.EnterRun,
            request,
            decision);

        var specialKeyDecision = new GuiSmokeStepDecision("act", "right-click", null, null, null, "cancel selection", "cancel with right click", 0.8, "combat", 500, false, null);
        ValidateDecision(
            GuiSmokePhase.HandleCombat,
            request with
            {
                Phase = GuiSmokePhase.HandleCombat.ToString(),
                AllowedActions = new[] { "click card", "click enemy", "click end turn", "right-click cancel selected card", "wait" },
            },
            specialKeyDecision);

        var autoRewardRequest = request with
        {
            Phase = GuiSmokePhase.HandleRewards.ToString(),
            Observer = new ObserverSummary(
                "rewards",
                "rewards",
                false,
                DateTimeOffset.UtcNow,
                "inv",
                true,
                "mixed",
                "stable",
                "episode-1",
                null,
                null,
                30,
                80,
                null,
                new[] { "skip" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("reward:0", "reward-item", "Reward Card", "100,200,120,120", true),
                    new ObserverActionNode("reward:1", "button", "Proceed", "900,700,240,90", true),
                },
                Array.Empty<ObserverChoice>(),
                Array.Empty<ObservedCombatHandCard>()),
            AllowedActions = new[] { "click proceed", "click reward", "wait" },
        };
        var autoDecision = AutoDecisionProvider.Decide(autoRewardRequest);
        Assert(autoDecision.ActionKind == "click", "Auto provider should choose a click for reward handling.");
        Assert(autoDecision.TargetLabel == "claim reward item", "Auto provider should prefer the reward item before proceed.");

        var treasureRequest = request with
        {
            Phase = GuiSmokePhase.ChooseFirstNode.ToString(),
            Observer = new ObserverSummary(
                "map",
                "map",
                false,
                DateTimeOffset.UtcNow,
                "inv",
                true,
                "mixed",
                "stable",
                "episode-2",
                "Treasure",
                "treasure",
                57,
                80,
                null,
                new[] { "Chest", "Proceed" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("treasure:chest", "treasure-chest", "Chest", "602,367,800,500", true),
                },
                new[]
                {
                    new ObserverChoice("treasure-chest", "Chest", "602,367,800,500", null, "Chest")
                    {
                        BindingKind = "treasure-room",
                        BindingId = "chest",
                        Enabled = true,
                        SemanticHints = new[] { "treasure-room", "treasure-chest" },
                    },
                },
                Array.Empty<ObservedCombatHandCard>())
            {
                Meta = new Dictionary<string, string?>
                {
                    ["transitionInProgress"] = "false",
                    ["rootSceneIsMainMenu"] = "false",
                    ["rootSceneIsRun"] = "true",
                    ["currentRunNodePresent"] = "true",
                    ["rootSceneCurrentType"] = "MegaCrit.Sts2.Core.Nodes.NRun",
                    ["treasureRoomDetected"] = "true",
                    ["treasureChestClickable"] = "true",
                    ["treasureChestOpened"] = "false",
                    ["treasureRelicHolderCount"] = "0",
                    ["treasureVisibleRelicHolderCount"] = "0",
                    ["treasureEnabledRelicHolderCount"] = "0",
                    ["treasureProceedEnabled"] = "false",
                    ["treasureInspectOverlayVisible"] = "false",
                    ["treasureRoomRootType"] = "MegaCrit.Sts2.Core.Nodes.Rooms.NTreasureRoom",
                },
            },
            AllowedActions = new[] { "click treasure chest", "wait" },
        };
        Assert(
            GetAllowedActions(GuiSmokePhase.ChooseFirstNode, new ObserverState(treasureRequest.Observer, null, null, null)).Contains("click treasure chest", StringComparer.OrdinalIgnoreCase),
            "Treasure room with a closed chest should expose the explicit treasure chest lane.");
        Assert(AutoDecisionProvider.BuildTreasureSceneState(new ObserverState(treasureRequest.Observer, null, null, null)) is
            {
                CanonicalForegroundOwner: NonCombatCanonicalForegroundOwner.Treasure,
                HandoffTarget: NonCombatHandoffTarget.ChooseFirstNode,
                AllowsFastForegroundWait: false,
            },
            "Treasure scene state should preserve ChooseFirstNode handoff without enabling fast foreground waits.");
        Assert(
            !GetAllowedActions(GuiSmokePhase.ChooseFirstNode, new ObserverState(treasureRequest.Observer, null, null, null)).Contains("click exported reachable node", StringComparer.OrdinalIgnoreCase),
            "Treasure room authority should suppress generic exported reachable-node routing.");
        var treasureDecision = AutoDecisionProvider.Decide(treasureRequest);
        Assert(treasureDecision.ActionKind == "click", "Treasure handling should click.");
        Assert(treasureDecision.TargetLabel == "treasure chest", "Treasure handling should use the explicit treasure chest lane first.");
        Assert(
            IsRoomProgressTarget("treasure proceed")
            && GetSameActionStallLimit(
                GuiSmokePhase.ChooseFirstNode,
                new GuiSmokeStepDecision("act", "click", null, 0.5, 0.4, "treasure proceed", "proceed", 0.9, "map", 1200, true, null)) == 4,
            "Treasure proceed should be treated as room progress with the same stall budget as the chest.");

        var treasureRelicRequest = treasureRequest with
        {
            Observer = treasureRequest.Observer with
            {
                Meta = new Dictionary<string, string?>(treasureRequest.Observer.Meta)
                {
                    ["treasureChestClickable"] = "false",
                    ["treasureChestOpened"] = "true",
                    ["treasureRelicHolderCount"] = "2",
                    ["treasureVisibleRelicHolderCount"] = "2",
                    ["treasureEnabledRelicHolderCount"] = "1",
                    ["treasureRelicHolderIds"] = "RELIC.ANCHOR,RELIC.BAG_OF_PREPARATION",
                },
                CurrentChoices = new[] { "닻", "가방", "진행", "불타는 혈액" },
                Choices = new[]
                {
                    new ObserverChoice("treasure-relic-holder", "닻", "680,280,180,180", "RELIC.ANCHOR", "Treasure holder")
                    {
                        BindingKind = "treasure-room",
                        BindingId = "RELIC.ANCHOR",
                        Enabled = true,
                        SemanticHints = new[] { "treasure-room", "treasure-relic-holder" },
                    },
                    new ObserverChoice("relic", "불타는 혈액", "12,82,68,68", "RELIC.BURNING_BLOOD", "Inventory relic")
                    {
                        Enabled = true,
                    },
                },
                ActionNodes = Array.Empty<ObserverActionNode>(),
            },
            AllowedActions = new[] { "click treasure relic holder", "wait" },
        };
        Assert(
            GetAllowedActions(GuiSmokePhase.ChooseFirstNode, new ObserverState(treasureRelicRequest.Observer, null, null, null)).Contains("click treasure relic holder", StringComparer.OrdinalIgnoreCase),
            "Open treasure rooms with visible holders should expose the explicit relic-holder lane.");
        var treasureRelicDecision = AutoDecisionProvider.Decide(treasureRelicRequest);
        Assert(treasureRelicDecision.TargetLabel == "treasure relic holder", "Treasure relic-holder lane should preserve the treasure room action lane over top-left inventory relic icons.");

        var treasureProceedRequest = treasureRequest with
        {
            Observer = treasureRequest.Observer with
            {
                Meta = new Dictionary<string, string?>(treasureRequest.Observer.Meta)
                {
                    ["treasureChestClickable"] = "false",
                    ["treasureChestOpened"] = "true",
                    ["treasureRelicHolderCount"] = "1",
                    ["treasureVisibleRelicHolderCount"] = "0",
                    ["treasureEnabledRelicHolderCount"] = "0",
                    ["treasureProceedEnabled"] = "true",
                },
                Choices = new[]
                {
                    new ObserverChoice("treasure-proceed", "진행", "980,540,220,90", null, "Treasure proceed")
                    {
                        BindingKind = "treasure-room",
                        BindingId = "proceed",
                        Enabled = true,
                        SemanticHints = new[] { "treasure-room", "treasure-proceed" },
                    },
                },
                ActionNodes = Array.Empty<ObserverActionNode>(),
            },
            AllowedActions = new[] { "click treasure proceed", "wait" },
        };
        Assert(
            GetAllowedActions(GuiSmokePhase.ChooseFirstNode, new ObserverState(treasureProceedRequest.Observer, null, null, null)).Contains("click treasure proceed", StringComparer.OrdinalIgnoreCase),
            "Treasure room should expose explicit proceed once relic picking is finished.");
        var treasureProceedDecision = AutoDecisionProvider.Decide(treasureProceedRequest);
        Assert(treasureProceedDecision.TargetLabel == "treasure proceed", "Treasure proceed lane should open once no enabled relic holder remains.");

        var treasureProceedWithStaleGenericRequest = treasureProceedRequest with
        {
            Observer = treasureProceedRequest.Observer with
            {
                Choices = new[]
                {
                    new ObserverChoice("treasure-proceed", "진행", "980,540,220,90", null, "Treasure proceed")
                    {
                        BindingKind = "treasure-room",
                        BindingId = "proceed",
                        Enabled = true,
                        SemanticHints = new[] { "treasure-room", "treasure-proceed" },
                    },
                    new ObserverChoice("choice", "진행", "1983,764,269,108", null, "Stale generic proceed")
                    {
                        Enabled = true,
                    },
                },
            },
        };
        var treasureProceedWithStaleGenericDecision = AutoDecisionProvider.Decide(treasureProceedWithStaleGenericRequest);
        Assert(
            treasureProceedWithStaleGenericDecision.TargetLabel == "treasure proceed",
            "Treasure proceed should prefer the explicit treasure affordance over stale generic proceed bounds.");

        var treasureAfterProceedStaleGenericRequest = treasureProceedRequest with
        {
            Observer = treasureProceedRequest.Observer with
            {
                Meta = new Dictionary<string, string?>(treasureProceedRequest.Observer.Meta)
                {
                    ["treasureProceedEnabled"] = "false",
                },
                Choices = new[]
                {
                    new ObserverChoice("choice", "진행", "1983,764,269,108", null, "Stale generic proceed")
                    {
                        Enabled = true,
                    },
                },
            },
            AllowedActions = new[] { "wait" },
        };
        Assert(
            !GetAllowedActions(GuiSmokePhase.ChooseFirstNode, new ObserverState(treasureAfterProceedStaleGenericRequest.Observer, null, null, null)).Contains("click treasure proceed", StringComparer.OrdinalIgnoreCase),
            "Treasure aftermath should not reopen proceed from stale generic bounds once explicit treasure proceed is gone.");
        var treasureAfterProceedStaleGenericDecision = AutoDecisionProvider.Decide(treasureAfterProceedStaleGenericRequest);
        Assert(
            string.Equals(treasureAfterProceedStaleGenericDecision.Status, "wait", StringComparison.OrdinalIgnoreCase),
            "Treasure aftermath should wait for the next authoritative state instead of reusing stale generic proceed bounds.");

        var treasureAfterProceedStaleHolderRequest = treasureProceedRequest with
        {
            History = new[]
            {
                new GuiSmokeHistoryEntry(GuiSmokePhase.ChooseFirstNode.ToString(), "click", "treasure proceed", DateTimeOffset.UtcNow),
                new GuiSmokeHistoryEntry(GuiSmokePhase.WaitMap.ToString(), "branch-treasure", null, DateTimeOffset.UtcNow.AddMilliseconds(100)),
            },
            Observer = treasureProceedRequest.Observer with
            {
                Meta = new Dictionary<string, string?>(treasureProceedRequest.Observer.Meta)
                {
                    ["treasureProceedEnabled"] = "false",
                    ["mapScreenOpen"] = "true",
                    ["sharedRelicPickingActive"] = "false",
                    ["treasureRelicCollectionOpen"] = "false",
                },
                Choices = new[]
                {
                    new ObserverChoice("treasure-relic-holder", "타격용 인형", "857.792,357.208,204,204", "Relic", "Treasure room relic holder")
                    {
                        BindingKind = "treasure-room",
                        BindingId = "Relic",
                        Enabled = true,
                        SemanticHints = new[] { "treasure-room", "treasure-relic-holder" },
                    },
                },
            },
            AllowedActions = new[] { "wait" },
        };
        var treasureAfterProceedStaleHolderDecision = AutoDecisionProvider.Decide(treasureAfterProceedStaleHolderRequest);
        Assert(
            string.Equals(treasureAfterProceedStaleHolderDecision.Status, "wait", StringComparison.OrdinalIgnoreCase),
            "Treasure aftermath should not fall back to a stale relic-holder click immediately after treasure proceed, even if branch-treasure reconciliation entries appear in between.");

        var fakeMapNodeRelicObserver = new ObserverState(
            new ObserverSummary(
                "map",
                "map",
                false,
                DateTimeOffset.UtcNow,
                "inv-fake-map-node",
                true,
                "mixed",
                "stable",
                "episode-fake-map-node",
                "Treasure",
                "generic",
                76,
                80,
                null,
                new[] { "불타는 혈액" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("map-node:2", "map-node", "불타는 혈액", "12,82,68,68", true)
                    {
                        TypeName = "relic",
                        SemanticHints = new[] { "scene:map", "kind:map-node", "raw-kind:relic", "value:RELIC.BURNING_BLOOD" },
                    },
                },
                new[]
                {
                    new ObserverChoice("relic", "불타는 혈액", "12,82,68,68", "RELIC.BURNING_BLOOD", "Inventory relic"),
                },
                Array.Empty<ObservedCombatHandCard>()),
            null,
            null,
            null);
        var fakeMapNodeRelicRequest = request with
        {
            Phase = GuiSmokePhase.ChooseFirstNode.ToString(),
            Observer = fakeMapNodeRelicObserver.Summary,
            AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, fakeMapNodeRelicObserver),
        };
        var exportedMapPointMethod = typeof(AutoDecisionProvider).GetMethod("TryCreateExportedReachableMapPointDecision", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert(exportedMapPointMethod is not null, "Expected private exported map-point decision helper.");
        var fakeExportedMapPointDecision = exportedMapPointMethod!.Invoke(null, new object?[] { fakeMapNodeRelicRequest }) as GuiSmokeStepDecision;
        Assert(fakeExportedMapPointDecision is null, "Fake map-node action nodes carrying raw-kind relic metadata should be rejected before exported map routing is even considered.");
        Assert(
            !fakeMapNodeRelicRequest.AllowedActions.Contains("click exported reachable node", StringComparer.OrdinalIgnoreCase),
            "Top-left relic inventory affordances must not open the exported reachable-map-node lane.");
        var fakeMapNodeRelicDecision = AutoDecisionProvider.Decide(fakeMapNodeRelicRequest);
        Assert(
            !string.Equals(fakeMapNodeRelicDecision.TargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase),
            "Fake map-node action nodes carrying raw-kind relic metadata must not be clicked as reachable map nodes.");

        var explicitMapPointObserver = new ObserverState(
            new ObserverSummary(
                "map",
                "map",
                false,
                DateTimeOffset.UtcNow,
                "inv-real-map-node",
                true,
                "map",
                "stable",
                "episode-real-map-node",
                "None",
                "map",
                76,
                80,
                null,
                new[] { "휴식 (1,2)" },
                Array.Empty<string>(),
                new[]
                {
                    new ObserverActionNode("map:1:2", "map-node", "휴식 (1,2)", "897,581,124,124", true)
                    {
                        TypeName = "map-node",
                        SemanticHints = new[] { "scene:map", "kind:map-node", "raw-kind:map-node", "coord:1,2", "source:map-choice" },
                    },
                },
                new[]
                {
                    new ObserverChoice("map-node", "휴식 (1,2)", "897,581,124,124", "1,2", "type:Rest;coord:1,2")
                    {
                        NodeId = "map:1:2",
                        SemanticHints = new[] { "coord:1,2" },
                    },
                },
                Array.Empty<ObservedCombatHandCard>()),
            null,
            null,
            null);
        var explicitMapPointRequest = request with
        {
            Phase = GuiSmokePhase.ChooseFirstNode.ToString(),
            Observer = explicitMapPointObserver.Summary,
            AllowedActions = GetAllowedActions(GuiSmokePhase.ChooseFirstNode, explicitMapPointObserver),
        };
        var explicitExportedMapPointDecision = exportedMapPointMethod.Invoke(null, new object?[] { explicitMapPointRequest }) as GuiSmokeStepDecision;
        Assert(
            explicitExportedMapPointDecision is not null
            && string.Equals(explicitExportedMapPointDecision.TargetLabel, "exported reachable map node", StringComparison.OrdinalIgnoreCase),
            "Explicit map point metadata should still produce an exported reachable-map-node decision.");
        var explicitMapPointDecision = AutoDecisionProvider.Decide(explicitMapPointRequest);
        Assert(
            explicitMapPointDecision is not null,
            "Explicit map point fixture should remain decisionable after source correction.");

            Assert(GetDecisionWaitMinimumMs(GuiSmokePhase.HandleCombat) == 140, "Combat decision wait minimum should use the reduced fast-path baseline.");
            Assert(GetDecisionWaitMinimumMs(GuiSmokePhase.WaitRunLoad) == 260, "Run-load waits should keep a slightly higher minimum than stable foreground phases.");
            Assert(GetDecisionWaitMinimumMs(GuiSmokePhase.HandleEvent) == 200, "Stable non-combat foreground phases should use the reduced baseline wait.");
            Assert(GetLaunchPollingIntervalMs() == 250, "Launch/focus polling should use the faster cadence.");
            Assert(ScreenCaptureService.GetCaptureRetryDelayMs(0) == 0, "Initial capture attempt should not pay retry backoff.");
            Assert(ScreenCaptureService.GetCaptureRetryDelayMs(1) == 200, "Second capture attempt should use the short backoff.");
            Assert(ScreenCaptureService.GetCaptureRetryDelayMs(2) == 350, "Third capture attempt should use the medium backoff.");
            Assert(ScreenCaptureService.GetCaptureRetryDelayMs(3) == 500, "Later capture attempts should clamp to the bounded max backoff.");
            var detailedCaptureService = new ScreenCaptureService();
            var detailedCaptureRoot = Path.Combine(Path.GetTempPath(), $"gui-smoke-capture-boundary-{Guid.NewGuid():N}");
            Directory.CreateDirectory(detailedCaptureRoot);
            try
            {
                var captureTarget = new WindowCaptureTarget(IntPtr.Zero, "test", new Rectangle(0, 0, 32, 32), true, false);

                var successfulCapturePath = Path.Combine(detailedCaptureRoot, "success.png");
                var successfulCaptureResult = detailedCaptureService.TryCaptureDetailed(
                    captureTarget,
                    successfulCapturePath,
                    TimeSpan.FromSeconds(1),
                    static _ =>
                    {
                        var bitmap = new Bitmap(8, 8);
                        using var graphics = Graphics.FromImage(bitmap);
                        graphics.Clear(Color.White);
                        return bitmap;
                    });
                Assert(successfulCaptureResult.Succeeded && File.Exists(successfulCapturePath), "Detailed capture should persist a valid bitmap on success.");

                var blackFrameCaptureResult = detailedCaptureService.TryCaptureDetailed(
                    captureTarget,
                    Path.Combine(detailedCaptureRoot, "black-frame.png"),
                    TimeSpan.FromSeconds(1),
                    static _ => new Bitmap(8, 8));
                Assert(
                    !blackFrameCaptureResult.Succeeded && blackFrameCaptureResult.FailureKind == CaptureBoundaryFailureKind.UnusableFrame,
                    "Detailed capture should classify a mostly black bitmap as an unusable frame.");

                var timeoutCaptureResult = detailedCaptureService.TryCaptureDetailed(
                    captureTarget,
                    Path.Combine(detailedCaptureRoot, "timeout.png"),
                    TimeSpan.FromMilliseconds(25),
                    static _ =>
                    {
                        Thread.Sleep(250);
                        var bitmap = new Bitmap(8, 8);
                        using var graphics = Graphics.FromImage(bitmap);
                        graphics.Clear(Color.White);
                        return bitmap;
                    });
                Assert(
                    !timeoutCaptureResult.Succeeded && timeoutCaptureResult.FailureKind == CaptureBoundaryFailureKind.TimedOut,
                    "Detailed capture should classify an over-budget capture as capture-timeout.");

                var exceptionCaptureResult = detailedCaptureService.TryCaptureDetailed(
                    captureTarget,
                    Path.Combine(detailedCaptureRoot, "exception.png"),
                    TimeSpan.FromSeconds(1),
                    static _ => throw new InvalidOperationException("boom"));
                Assert(
                    !exceptionCaptureResult.Succeeded
                    && exceptionCaptureResult.FailureKind == CaptureBoundaryFailureKind.Exception
                    && exceptionCaptureResult.Exception is InvalidOperationException,
                    "Detailed capture should classify thrown capture failures as capture-exception.");
            }
            finally
            {
                if (Directory.Exists(detailedCaptureRoot))
                {
                    Directory.Delete(detailedCaptureRoot, recursive: true);
                }
            }
            var replayParitySummary = BuildReplayParityDecisionSummary(
                new GuiSmokeStepRequest(
                    "run",
                    "boot-to-long-run",
                    99,
                    GuiSmokePhase.ChooseFirstNode.ToString(),
                    "Parity summary should capture generic non-combat decision semantics.",
                    DateTimeOffset.UtcNow,
                    string.Empty,
                    new WindowBounds(0, 0, 1280, 720),
                    "phase:choosefirstnode|screen:event",
                    "0001",
                    1,
                    3,
                    true,
                    "tactical",
                    null,
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
                        null,
                        "map",
                        80,
                        80,
                        null,
                        new[] { "Monster (1,3)" },
                        Array.Empty<string>(),
                        Array.Empty<ObserverActionNode>(),
                        Array.Empty<ObserverChoice>(),
                        Array.Empty<ObservedCombatHandCard>())
                    {
                        Meta = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["foregroundOwner"] = "map",
                            ["foregroundActionLane"] = "map-node",
                        },
                    },
                    Array.Empty<KnownRecipeHint>(),
                    Array.Empty<EventKnowledgeCandidate>(),
                    Array.Empty<CombatCardKnowledgeHint>(),
                    new[] { "click exported reachable node", "wait" },
                    Array.Empty<GuiSmokeHistoryEntry>(),
                    "Parity summary should preserve the exported map-node contract.",
                    "Choose the exported reachable map node."),
                new GuiSmokeStepDecision("act", "click", null, 0.401, 0.522, "exported reachable map node", "test", 0.95, "map", null, false, null));
            Assert(replayParitySummary.Contains("target:exported reachable map node", StringComparison.OrdinalIgnoreCase), "Replay parity summary should preserve the generic decision target label.");
            Assert(replayParitySummary.Contains("x:0.401", StringComparison.OrdinalIgnoreCase) && replayParitySummary.Contains("y:0.522", StringComparison.OrdinalIgnoreCase), "Replay parity summary should preserve normalized click coordinates.");
            Assert(string.Equals(BuildReplayParityAllowedActionKey(new[] { "wait", "Click Exported Reachable Node" }), "click exported reachable node|wait", StringComparison.OrdinalIgnoreCase), "Replay parity allowlist key should sort and normalize action labels.");
    }
}
