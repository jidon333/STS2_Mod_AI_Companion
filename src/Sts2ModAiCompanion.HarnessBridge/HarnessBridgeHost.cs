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
                    Message: _lastMessage));
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

        var inventoryAttempt = _inventoryPublisher.TryPublish(_liveSnapshotReader, mode);
        if (inventoryAttempt.Suppressed && inventoryAttempt.Inventory is not null)
        {
            _traceWriter.Write("inventory-suppressed", null, new
            {
                rawSceneType = inventoryAttempt.RawSceneType,
                sceneType = inventoryAttempt.Inventory.SceneType,
                reason = inventoryAttempt.SuppressionReason,
                mode,
            });
        }

        if (inventoryAttempt.Published && inventoryAttempt.Inventory is not null)
        {
            _traceWriter.Write("inventory-published", null, new
            {
                inventoryId = inventoryAttempt.Inventory.InventoryId,
                rawSceneType = inventoryAttempt.RawSceneType,
                sceneType = inventoryAttempt.Inventory.SceneType,
                nodeCount = inventoryAttempt.Inventory.Nodes.Count,
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
            });

            _lastActionId = action.ActionId;
            if (armSession is null)
            {
                _lastResultStatus = "ignored";
                _lastMessage = "bridge-dormant-no-arm";
                _traceWriter.Write("action-ignored", action.ActionId, new
                {
                    reason = _lastMessage,
                    kind = action.Kind,
                });
                continue;
            }

            _lastResultStatus = "rejected";
            _lastMessage = "actuator-disabled-until-post-clean-boot";
            _traceWriter.Write("action-rejected", action.ActionId, new
            {
                reason = _lastMessage,
                kind = action.Kind,
            });
        }

        _statusPublisher.Write(new HarnessBridgeStatus(
            Enabled: true,
            Mode: mode,
            LastActionId: _lastActionId,
            LastResultStatus: _lastResultStatus,
            UpdatedAt: DateTimeOffset.UtcNow,
            Message: _lastMessage ?? modeMessage));
    }
}
