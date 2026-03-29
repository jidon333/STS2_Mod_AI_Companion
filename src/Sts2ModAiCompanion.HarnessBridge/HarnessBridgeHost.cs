using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Harness;

namespace Sts2ModAiCompanion.HarnessBridge;

internal sealed class HarnessBridgeHost
{
    private readonly HarnessQueueLayout _layout;
    private readonly int _pollIntervalMs;
    private readonly ArmSessionReader _armSessionReader = new();
    private readonly ActionQueueScanner _actionQueueScanner = new();
    private readonly LiveSnapshotReader _liveSnapshotReader = new();
    private readonly StatusPublisher _statusPublisher;
    private readonly TraceWriter _traceWriter;
    private readonly InventoryPublisher _inventoryPublisher;
    private readonly CancellationTokenSource _cancellation = new();
    private Task? _loopTask;
    private string? _lastMode;
    private string? _lastActionId;
    private string? _lastResultStatus;
    private string? _lastMessage;
    private string? _lastGuardState;
    private string? _lastGuardReason;

    public HarnessBridgeHost(HarnessQueueLayout layout, string liveSnapshotPath, int pollIntervalMs)
    {
        _layout = layout;
        _pollIntervalMs = Math.Max(pollIntervalMs, 100);
        _statusPublisher = new StatusPublisher(layout.StatusPath);
        _traceWriter = new TraceWriter(layout.TracePath);
        _inventoryPublisher = new InventoryPublisher(layout.InventoryPath, liveSnapshotPath);
    }

    public void Start()
    {
        if (_loopTask is not null)
        {
            return;
        }

        _loopTask = Task.Run(RunLoopAsync);
    }

    private async Task RunLoopAsync()
    {
        _traceWriter.Write("bridge-started", null, new
        {
            root = _layout.HarnessRoot,
            actionsPath = _layout.ActionsPath,
            statusPath = _layout.StatusPath,
            tracePath = _layout.TracePath,
            inventoryPath = _layout.InventoryPath,
        });

        while (!_cancellation.IsCancellationRequested)
        {
            try
            {
                Tick();
            }
            catch (Exception exception)
            {
                _lastResultStatus = "error";
                _lastMessage = exception.Message;
                _statusPublisher.Write(new HarnessBridgeStatus(
                    Enabled: true,
                    Mode: "error",
                    LastActionId: _lastActionId,
                    LastResultStatus: _lastResultStatus,
                    UpdatedAt: DateTimeOffset.UtcNow,
                    Message: _lastMessage,
                    GuardState: "error",
                    LastGuardReason: exception.Message));
                _traceWriter.Write("bridge-error", _lastActionId, new { message = exception.Message });
            }

            try
            {
                await Task.Delay(_pollIntervalMs, _cancellation.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void Tick()
    {
        var armSession = _armSessionReader.TryReadActiveSession(
            _layout.ArmSessionPath,
            out var armPresent,
            out var sessionTokenPresent,
            out var modeMessage);
        var mode = armSession is null ? "dormant" : "armed";
        _lastGuardState = armSession is null ? "dormant" : "armed-no-inventory";
        _lastGuardReason = modeMessage;

        var inventoryAttempt = _inventoryPublisher.TryPublish(_liveSnapshotReader, mode);
        var currentInventory = inventoryAttempt.Inventory;
        UpdateGuardSurface(armSession, currentInventory, modeMessage);
        if (inventoryAttempt.Suppressed && inventoryAttempt.Inventory is not null)
        {
            _traceWriter.Write("inventory-suppressed", null, new
            {
                rawSceneType = inventoryAttempt.RawSceneType,
                publishedSceneType = inventoryAttempt.Inventory.PublishedSceneType,
                sceneType = inventoryAttempt.Inventory.SceneType,
                compatibilitySceneType = inventoryAttempt.Inventory.CompatibilitySceneType,
                reason = inventoryAttempt.SuppressionReason,
                sceneReady = ResolveGuardSceneReady(inventoryAttempt.Inventory),
                sceneAuthority = ResolveGuardSceneAuthority(inventoryAttempt.Inventory),
                sceneStability = ResolveGuardSceneStability(inventoryAttempt.Inventory),
                mode,
            });
        }

        if (inventoryAttempt.Published && inventoryAttempt.Inventory is not null)
        {
            _traceWriter.Write("inventory-published", null, new
            {
                inventoryId = inventoryAttempt.Inventory.InventoryId,
                rawSceneType = inventoryAttempt.RawSceneType,
                publishedSceneType = inventoryAttempt.Inventory.PublishedSceneType,
                sceneType = inventoryAttempt.Inventory.SceneType,
                compatibilitySceneType = inventoryAttempt.Inventory.CompatibilitySceneType,
                nodeCount = inventoryAttempt.Inventory.Nodes.Count,
                sceneReady = ResolveGuardSceneReady(inventoryAttempt.Inventory),
                sceneAuthority = ResolveGuardSceneAuthority(inventoryAttempt.Inventory),
                sceneStability = ResolveGuardSceneStability(inventoryAttempt.Inventory),
                mode = inventoryAttempt.Inventory.Mode,
            });
        }

        if (!string.Equals(_lastMode, mode, StringComparison.Ordinal))
        {
            _traceWriter.Write("mode-changed", null, new
            {
                previous = _lastMode,
                current = mode,
                message = modeMessage,
                armPresent,
                sessionTokenPresent,
            });
            _lastMode = mode;
        }

        foreach (var action in _actionQueueScanner.ReadPendingActions(_layout.ActionsPath))
        {
            _traceWriter.Write("action-seen", action.ActionId, new
            {
                kind = action.Kind,
                targetLabel = action.TargetLabel,
                requestedAt = action.RequestedAt,
                sessionToken = action.SessionToken,
                inventoryId = action.InventoryId,
                nodeId = action.NodeId,
            });

            _lastActionId = action.ActionId;
            if (armSession is null)
            {
                _lastResultStatus = "ignored";
                _lastMessage = "bridge-dormant-no-arm";
                _lastGuardState = "dormant";
                _lastGuardReason = _lastMessage;
                _traceWriter.Write("action-ignored", action.ActionId, new
                {
                    reason = _lastMessage,
                    kind = action.Kind,
                });
                continue;
            }

            var guardResult = EvaluateGuard(action, armSession, currentInventory);
            _lastResultStatus = "rejected";
            _lastMessage = guardResult.Reason;
            _lastGuardState = guardResult.GuardState;
            _lastGuardReason = guardResult.Reason;
            _traceWriter.Write("action-rejected", action.ActionId, new
            {
                reason = _lastMessage,
                guardState = guardResult.GuardState,
                kind = action.Kind,
                inventoryId = action.InventoryId,
                nodeId = action.NodeId,
            });
        }

        _statusPublisher.Write(new HarnessBridgeStatus(
            Enabled: true,
            Mode: mode,
            LastActionId: _lastActionId,
            LastResultStatus: _lastResultStatus,
            UpdatedAt: DateTimeOffset.UtcNow,
            Message: _lastMessage ?? modeMessage,
            GuardState: _lastGuardState,
            LastGuardReason: _lastGuardReason));
    }

    private void UpdateGuardSurface(HarnessArmSession? armSession, HarnessNodeInventory? inventory, string modeMessage)
    {
        if (armSession is null)
        {
            _lastGuardState = "dormant";
            _lastGuardReason = modeMessage;
            return;
        }

        if (inventory is null)
        {
            _lastGuardState = "armed-no-inventory";
            _lastGuardReason = "inventory-unavailable";
            return;
        }

        if (ResolveGuardSceneReady(inventory) != true)
        {
            _lastGuardState = "armed-but-unsafe";
            _lastGuardReason = "scene-not-ready";
            return;
        }

        var sceneStability = ResolveGuardSceneStability(inventory);
        if (!string.Equals(sceneStability, "stable", StringComparison.OrdinalIgnoreCase))
        {
            _lastGuardState = "armed-but-unsafe";
            _lastGuardReason = $"scene-not-stable:{sceneStability ?? "unknown"}";
            return;
        }

        if (!string.IsNullOrWhiteSpace(inventory.BlockingModal))
        {
            _lastGuardState = "armed-but-unsafe";
            _lastGuardReason = $"blocking-modal:{inventory.BlockingModal}";
            return;
        }

        _lastGuardState = "armed-safe-for-future-dispatch";
        _lastGuardReason = "guard-checks-passed";
    }

    private static GuardEvaluation EvaluateGuard(
        ActionQueueScanner.QueuedHarnessAction action,
        HarnessArmSession armSession,
        HarnessNodeInventory? inventory)
    {
        if (!string.IsNullOrWhiteSpace(action.SessionToken)
            && !string.Equals(action.SessionToken, armSession.SessionToken, StringComparison.Ordinal))
        {
            return new GuardEvaluation("armed-but-unsafe", "session-token-mismatch");
        }

        if (inventory is null)
        {
            return new GuardEvaluation("armed-but-unsafe", "inventory-unavailable");
        }

        if (!string.IsNullOrWhiteSpace(action.InventoryId)
            && !string.Equals(action.InventoryId, inventory.InventoryId, StringComparison.Ordinal))
        {
            return new GuardEvaluation("armed-but-unsafe", "inventory-mismatch");
        }

        if (ResolveGuardSceneReady(inventory) != true)
        {
            return new GuardEvaluation("armed-but-unsafe", "scene-not-ready");
        }

        var sceneStability = ResolveGuardSceneStability(inventory);
        if (!string.Equals(sceneStability, "stable", StringComparison.OrdinalIgnoreCase))
        {
            return new GuardEvaluation("armed-but-unsafe", $"scene-not-stable:{sceneStability ?? "unknown"}");
        }

        if (!string.IsNullOrWhiteSpace(inventory.BlockingModal))
        {
            return new GuardEvaluation("armed-but-unsafe", $"blocking-modal:{inventory.BlockingModal}");
        }

        if (!string.IsNullOrWhiteSpace(action.NodeId)
            && inventory.Nodes.All(node => !string.Equals(node.NodeId, action.NodeId, StringComparison.Ordinal)))
        {
            return new GuardEvaluation("armed-but-unsafe", "node-not-found");
        }

        return new GuardEvaluation("armed-safe-for-future-dispatch", "actuator-disabled-until-post-clean-boot");
    }

    private static bool? ResolveGuardSceneReady(HarnessNodeInventory inventory)
    {
        return inventory.PublishedSceneReady;
    }

    private static string? ResolveGuardSceneAuthority(HarnessNodeInventory inventory)
    {
        return inventory.PublishedSceneAuthority;
    }

    private static string? ResolveGuardSceneStability(HarnessNodeInventory inventory)
    {
        return inventory.PublishedSceneStability;
    }

    private sealed record GuardEvaluation(string GuardState, string Reason);
}
