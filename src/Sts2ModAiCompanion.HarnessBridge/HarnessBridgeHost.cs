using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModAiCompanion.HarnessBridge.ActionIngress;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModAiCompanion.HarnessBridge;

internal sealed class HarnessBridgeHost : IHarnessActionIngress
{
    private const string SessionTokenMetadataKey = "sessionToken";
    private static readonly string[] SingletonPropertyNames = { "Instance", "Current", "Singleton", "State" };
    private static readonly string[] NestedRootNames = { "Run", "RunState", "State", "Player", "Character", "Encounter", "Combat", "Manager" };
    private static readonly string[] SceneNodeKeywords = { "Screen", "Room", "Map", "Reward", "Event", "Shop", "Rest", "Button", "Character", "Menu", "Card" };
    private static readonly IReadOnlyDictionary<string, string[]> SentinelAliases =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["__start__"] = new[] { "start", "new run", "play", "embark", "시작", "출발", "싱글플레이", "새 게임" },
            ["__ironclad__"] = new[] { "ironclad", "아이언클래드" },
            ["__confirm__"] = new[] { "confirm", "ok", "yes", "continue", "확인", "계속" },
            ["__cancel__"] = new[] { "cancel", "back", "skip", "no", "취소", "뒤로", "넘기기" },
            ["__end_turn__"] = new[] { "endturn", "end turn", "turn end", "턴 종료" },
            ["__standard_run__"] = new[] { "standard", "standard run", "normal", "\uC77C\uBC18" },
            ["__first__"] = Array.Empty<string>(),
            ["__first_reward__"] = new[] { "reward", "card", "relic", "potion", "보상", "카드", "유물", "포션" },
            ["__first_combat__"] = new[] { "combat", "battle", "fight", "node", "room", "전투" },
            ["__first_playable__"] = new[] { "card", "play", "hand", "attack", "skill", "power", "카드" },
        };

    private readonly HarnessQueueLayout _layout;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };
    private readonly HashSet<string> _processedActionIds = new(StringComparer.Ordinal);
    private readonly int _pollIntervalMs;
    private Thread? _thread;
    private HarnessIngressStatus _status = new(true, "idle", null, DateTimeOffset.UtcNow);
    private string? _activeSessionToken;
    private bool _testModeActivatedForSession;
    private string? _lastInventorySignature;
    private static IReadOnlyList<string> _defaultArtifactRefs = Array.Empty<string>();

    public HarnessBridgeHost(HarnessQueueLayout layout, int pollIntervalMs)
    {
        _layout = layout;
        _pollIntervalMs = Math.Max(pollIntervalMs, 100);
        _defaultArtifactRefs = new[] { layout.ResultsPath, layout.StatusPath, layout.TracePath };
    }

    public void Start()
    {
        HarnessPathResolver.EnsureDirectories(_layout);
        WriteStatus("dormant", null, null, "waiting for arm session");
        _thread = new Thread(RunLoop)
        {
            IsBackground = true,
            Name = "STS2 Harness Bridge",
        };
        _thread.Start();
    }

    public Task<HarnessActionResult> SubmitAsync(HarnessAction action, CancellationToken cancellationToken)
    {
        var envelope = new HarnessActionEnvelope(action, CompanionState.CreateUnknown());
        return Task.FromResult(ExecuteAction(envelope));
    }

    private void RunLoop()
    {
        while (true)
        {
            try
            {
                PublishNodeInventory();
                ProcessPendingActions();
            }
            catch (Exception exception)
            {
                WriteStatus("error", _status.LastActionId, "failed", exception.Message);
            }

            Thread.Sleep(_pollIntervalMs);
        }
    }

    private void ProcessPendingActions()
    {
        var armSession = TryGetActiveArmSession();
        if (armSession is null)
        {
            return;
        }

        if (!File.Exists(_layout.ActionsPath))
        {
            EnsureArmedStatus(armSession);
            return;
        }

        using var stream = new FileStream(_layout.ActionsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            HarnessActionEnvelope? envelope;
            try
            {
                envelope = JsonSerializer.Deserialize<HarnessActionEnvelope>(line, _jsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (envelope?.Action is null)
            {
                continue;
            }

            var actionSessionToken = TryGetActionSessionToken(envelope.Action);
            if (string.IsNullOrWhiteSpace(actionSessionToken)
                || !string.Equals(actionSessionToken, armSession.SessionToken, StringComparison.Ordinal))
            {
                AppendTrace("action-ignored", envelope.Action.ActionId, new
                {
                    reason = "session-token-mismatch",
                    expectedSessionToken = armSession.SessionToken,
                    observedSessionToken = actionSessionToken,
                    envelope.Action.Kind,
                    envelope.Action.TargetLabel,
                    requestedAt = envelope.Action.RequestedAt,
                });
                continue;
            }

            if (envelope.Action.RequestedAt < armSession.IssuedAt)
            {
                AppendTrace("action-ignored", envelope.Action.ActionId, new
                {
                    reason = "requested-before-arm",
                    armIssuedAt = armSession.IssuedAt,
                    requestedAt = envelope.Action.RequestedAt,
                    envelope.Action.Kind,
                    envelope.Action.TargetLabel,
                });
                continue;
            }

            if (!_processedActionIds.Add(envelope.Action.ActionId))
            {
                continue;
            }

            AppendTrace("action-received", envelope.Action.ActionId, new
            {
                envelope.Action.Kind,
                envelope.Action.TargetLabel,
                envelope.Action.ExpectedStateDelta,
                requestedAt = envelope.Action.RequestedAt,
            });
            WriteStatus("executing", envelope.Action.ActionId, "pending", envelope.Action.Kind);
            var result = ExecuteAction(envelope);
            AppendResult(JsonSerializer.Serialize(result, _jsonOptions) + Environment.NewLine);
            AppendTrace("action-result", envelope.Action.ActionId, new
            {
                result.Status,
                result.FailureKind,
                result.ObservedStateDelta,
                result.Recoverable,
                startedAt = result.StartedAt,
                completedAt = result.CompletedAt,
            });
            WriteStatus("armed", envelope.Action.ActionId, result.Status, result.FailureKind);
        }
    }

    private HarnessArmSession? TryGetActiveArmSession()
    {
        var armSession = TryReadArmSession();
        if (armSession is null)
        {
            if (!string.IsNullOrWhiteSpace(_activeSessionToken))
            {
                _activeSessionToken = null;
                _processedActionIds.Clear();
                _testModeActivatedForSession = false;
                EnsureDormantStatus("waiting for arm session");
            }

            return null;
        }

        var tokenChanged = !string.Equals(_activeSessionToken, armSession.SessionToken, StringComparison.Ordinal);
        if (tokenChanged)
        {
            _activeSessionToken = armSession.SessionToken;
            _processedActionIds.Clear();
            _testModeActivatedForSession = false;
        }

        if (!_testModeActivatedForSession)
        {
            HarnessBridgeEntryPoint.ActivateHarnessSession();
            _testModeActivatedForSession = true;
        }

        EnsureArmedStatus(armSession);
        return armSession;
    }

    private HarnessArmSession? TryReadArmSession()
    {
        if (!File.Exists(_layout.ArmSessionPath))
        {
            return null;
        }

        try
        {
            using var stream = new FileStream(_layout.ArmSessionPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var session = JsonSerializer.Deserialize<HarnessArmSession>(stream, _jsonOptions);
            if (session is null || string.IsNullOrWhiteSpace(session.SessionToken) || session.IssuedAt == default)
            {
                return null;
            }

            if (session.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return null;
            }

            return session with { SessionToken = session.SessionToken.Trim() };
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private void EnsureArmedStatus(HarnessArmSession armSession)
    {
        var message = string.IsNullOrWhiteSpace(armSession.Reason)
            ? "armed"
            : $"armed:{armSession.Reason}";
        if (!string.Equals(_status.Mode, "armed", StringComparison.OrdinalIgnoreCase))
        {
            WriteStatus("armed", _status.LastActionId, null, message);
        }
    }

    private void EnsureDormantStatus(string message)
    {
        if (!string.Equals(_status.Mode, "dormant", StringComparison.OrdinalIgnoreCase))
        {
            WriteStatus("dormant", _status.LastActionId, null, message);
        }
    }

    private static string? TryGetActionSessionToken(HarnessAction action)
    {
        return action.Metadata.TryGetValue(SessionTokenMetadataKey, out var token) && !string.IsNullOrWhiteSpace(token)
            ? token.Trim()
            : null;
    }

    private static string? TryGetInventoryId(HarnessAction action)
    {
        return action.Metadata.TryGetValue("inventoryId", out var inventoryId) && !string.IsNullOrWhiteSpace(inventoryId)
            ? inventoryId.Trim()
            : null;
    }

    private void PublishNodeInventory()
    {
        var snapshot = BuildInventorySnapshot();
        if (string.Equals(_lastInventorySignature, snapshot.Signature, StringComparison.Ordinal))
        {
            return;
        }

        _lastInventorySignature = snapshot.Signature;
        Directory.CreateDirectory(Path.GetDirectoryName(_layout.InventoryPath)!);
        File.WriteAllText(_layout.InventoryPath, JsonSerializer.Serialize(snapshot.Inventory, _jsonOptions));
    }

    private InventorySnapshot BuildInventorySnapshot()
    {
        var sceneRoots = TryResolveSceneRoots();
        var liveSnapshot = TryReadLiveExportSnapshot();
        var scene = ResolveInventoryScene(sceneRoots, liveSnapshot);
        var blockingModal = ResolveBlockingModal(sceneRoots);
        var inputLocked = IsInputLocked(sceneRoots, out _);
        var mode = TryReadArmSession() is null ? "dormant" : "armed";

        var dispatchTargets = sceneRoots.Count == 0
            ? Array.Empty<InventoryDispatchTarget>()
            : sceneRoots
                .SelectMany(root => EnumerateSceneNodes(root, 4096))
                .Select(CreateNodeCandidate)
                .Where(candidate => candidate.ActivationNode is not null)
                .GroupBy(candidate => RuntimeHelpers.GetHashCode(candidate.ActivationNode ?? candidate.Node))
                .Select(group => group.First())
                .Select(candidate => CreateInventoryDispatchTarget(candidate, scene, blockingModal, inputLocked))
                .Where(target => target is not null)
                .Cast<InventoryDispatchTarget>()
                .OrderBy(target => target.Kind, StringComparer.OrdinalIgnoreCase)
                .ThenBy(target => target.Label, StringComparer.OrdinalIgnoreCase)
                .ThenBy(target => target.UiPath, StringComparer.OrdinalIgnoreCase)
                .ToArray();

        var inventoryId = CreateInventoryId(mode, scene, blockingModal, dispatchTargets);
        var inventoryNodes = dispatchTargets
            .Select(target => new HarnessNodeInventoryItem(
                CreateNodeId(inventoryId, target),
                target.Kind,
                target.Label,
                target.Description,
                target.TypeName,
                target.UiPath,
                target.Visible,
                target.Enabled,
                target.Actionable,
                null,
                target.SemanticHints))
            .ToArray();
        var inventory = new HarnessNodeInventory(
            inventoryId,
            DateTimeOffset.UtcNow,
            liveSnapshot?.RunId,
            scene,
            TryReadLiveEpisodeId(liveSnapshot),
            mode,
            blockingModal,
            inventoryNodes);
        return new InventorySnapshot(
            inventory,
            dispatchTargets.Zip(inventoryNodes, (target, node) => new InventoryDispatchNode(node.NodeId, target)).ToArray(),
            inventoryId);
    }

    private HarnessActionResult DispatchNode(HarnessAction action, CompanionState state, DateTimeOffset startedAt)
    {
        var inventoryId = TryGetInventoryId(action);
        if (string.IsNullOrWhiteSpace(inventoryId))
        {
            return BuildResult(action, startedAt, "failed", true, failureKind: "dispatch-node-missing-inventory-id");
        }

        if (string.IsNullOrWhiteSpace(action.TargetRef))
        {
            return BuildResult(action, startedAt, "failed", true, failureKind: "dispatch-node-missing-target-ref");
        }

        var snapshot = BuildInventorySnapshot();
        if (!string.Equals(snapshot.Inventory.InventoryId, inventoryId, StringComparison.Ordinal))
        {
            return BuildResult(
                action,
                startedAt,
                "failed",
                true,
                observed: snapshot.Inventory.InventoryId,
                failureKind: "dispatch-node-inventory-mismatch");
        }

        var target = snapshot.Nodes.FirstOrDefault(candidate => string.Equals(candidate.NodeId, action.TargetRef, StringComparison.Ordinal));
        if (target is null)
        {
            return BuildResult(action, startedAt, "failed", true, failureKind: "dispatch-node-unavailable");
        }

        if (!target.Target.Actionable)
        {
            return BuildResult(action, startedAt, "failed", true, failureKind: "dispatch-node-not-actionable");
        }

        if (!TryActivateCandidate(target.Target.Candidate))
        {
            return BuildResult(
                action,
                startedAt,
                "failed",
                true,
                observed: target.Target.Label,
                failureKind: "dispatch-node-activation-failed");
        }

        return BuildResult(
            action,
            startedAt,
            "ok",
            false,
            observed: $"{snapshot.Inventory.SceneType}:{target.Target.Kind}:{target.Target.Label}");
    }

    private static InventoryDispatchTarget? CreateInventoryDispatchTarget(
        NodeCandidate candidate,
        string scene,
        string? blockingModal,
        bool inputLocked)
    {
        var activationNode = candidate.ActivationNode ?? candidate.Node;
        if (activationNode is null)
        {
            return null;
        }

        var visible = TryEvaluateNodeVisibility(activationNode);
        if (!visible)
        {
            return null;
        }

        var kind = ClassifyInventoryNode(candidate, scene, blockingModal);
        var label = candidate.Label ?? candidate.NodeName ?? candidate.TypeName;
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        if (LooksLikePlaceholder(label) && !string.Equals(kind, "overlay-dismiss", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var enabled = TryGetNodeEnabled(activationNode);
        var actionable = enabled != false;
        if (!string.IsNullOrWhiteSpace(blockingModal) || inputLocked)
        {
            actionable = string.Equals(kind, "overlay-dismiss", StringComparison.OrdinalIgnoreCase) && enabled != false;
        }

        var description = !string.IsNullOrWhiteSpace(candidate.NodeName)
                          && !string.Equals(candidate.NodeName, label, StringComparison.OrdinalIgnoreCase)
            ? candidate.NodeName
            : null;
        var uiPath = TryDescribeNodePath(activationNode);
        var hints = new[] { scene, kind, candidate.NodeName, candidate.TypeName }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new InventoryDispatchTarget(candidate, kind, label, description, candidate.TypeName, uiPath, visible, enabled, actionable, hints);
    }

    private static string ResolveInventoryScene(IReadOnlyList<object> sceneRoots, LiveExportSnapshot? liveSnapshot)
    {
        var liveScene = NormalizeHarnessScene(liveSnapshot?.CurrentScreen);
        if (!string.IsNullOrWhiteSpace(liveScene) && !string.Equals(liveScene, "unknown", StringComparison.OrdinalIgnoreCase))
        {
            return liveScene;
        }

        var observedScene = EnumerateObservedScenes(sceneRoots)
            .Select(NormalizeHarnessScene)
            .FirstOrDefault(scene => !string.IsNullOrWhiteSpace(scene));
        return string.IsNullOrWhiteSpace(observedScene) ? "unknown" : observedScene;
    }

    private static string? ResolveBlockingModal(IReadOnlyList<object> sceneRoots)
    {
        if (FindFirstVisibleNodeByType(sceneRoots, "NMultiplayerTimeoutOverlay") is not null)
        {
            return "timeout-overlay";
        }

        if (FindFirstVisibleNodeByType(sceneRoots, "NSendFeedbackScreen") is not null)
        {
            return "feedback-overlay";
        }

        return CountBlockingOverlays(sceneRoots) > 0 ? "blocking-overlay" : null;
    }

    private static string ClassifyInventoryNode(NodeCandidate candidate, string scene, string? blockingModal)
    {
        if (!string.IsNullOrWhiteSpace(blockingModal) && ScoreOverlayDismissCandidate(candidate) > 0)
        {
            return "overlay-dismiss";
        }

        var typeName = candidate.TypeName;
        if (typeName.Contains("MapPoint", StringComparison.OrdinalIgnoreCase))
        {
            return "map-node";
        }

        if (typeName.Contains("Card", StringComparison.OrdinalIgnoreCase))
        {
            return "card";
        }

        return scene switch
        {
            "character-select" => "character",
            "map" => "map-node",
            "rewards" => "reward-item",
            "event" => "event-option",
            "rest-site" => "rest-option",
            "shop" => "shop-option",
            _ => "button",
        };
    }

    private static string CreateInventoryId(
        string mode,
        string scene,
        string? blockingModal,
        IReadOnlyList<InventoryDispatchTarget> targets)
    {
        var payload = string.Join(
            "|",
            targets.Select(target =>
                string.Join(
                    "::",
                    target.Kind,
                    target.Label,
                    target.TypeName,
                    target.UiPath,
                    target.Visible,
                    target.Enabled,
                    target.Actionable)));
        return "inv-" + ComputeStableId($"{mode}|{scene}|{blockingModal}|{payload}");
    }

    private static string CreateNodeId(string inventoryId, InventoryDispatchTarget target)
    {
        return "node-" + ComputeStableId(
            string.Join(
                "|",
                inventoryId,
                target.Kind,
                target.Label,
                target.TypeName,
                target.UiPath));
    }

    private static string ComputeStableId(string payload)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash[..8]).ToLowerInvariant();
    }

    private static string? TryDescribeNodePath(object? node)
    {
        if (node is null)
        {
            return null;
        }

        var rawPath = TryInvokeMethod(node, "GetPath");
        var path = rawPath?.ToString()?.Trim();
        if (!string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        var names = EnumerateSelfAndAncestors(node, 8)
            .Select(ResolveNodeName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Reverse()
            .ToArray();
        return names.Length == 0 ? null : string.Join("/", names);
    }

    private static string? TryReadLiveEpisodeId(LiveExportSnapshot? snapshot)
    {
        if (snapshot?.Meta is null)
        {
            return null;
        }

        if (snapshot.Meta.TryGetValue("sceneEpisodeId", out var episodeId) && !string.IsNullOrWhiteSpace(episodeId))
        {
            return episodeId;
        }

        return snapshot.Meta.TryGetValue("screenEpisodeId", out episodeId) && !string.IsNullOrWhiteSpace(episodeId)
            ? episodeId
            : null;
    }

    private static LiveExportSnapshot? TryReadLiveExportSnapshot()
    {
        try
        {
            var layout = LiveExportPathResolver.Resolve(GamePathOptions.CreateLocalDefault(), LiveExportOptions.Defaults);
            if (!File.Exists(layout.SnapshotPath))
            {
                return null;
            }

            return JsonSerializer.Deserialize<LiveExportSnapshot>(
                File.ReadAllText(layout.SnapshotPath),
                LiveExportJsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static JsonSerializerOptions LiveExportJsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private void AppendResult(string payload)
    {
        var attempts = 0;
        while (true)
        {
            try
            {
                using var stream = new FileStream(_layout.ResultsPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
                using var writer = new StreamWriter(stream);
                writer.Write(payload);
                writer.Flush();
                return;
            }
            catch (IOException) when (attempts < 20)
            {
                attempts += 1;
                Thread.Sleep(100);
            }
        }
    }

    private void AppendTrace(string kind, string? actionId, object payload)
    {
        var envelope = new
        {
            ts = DateTimeOffset.UtcNow,
            kind,
            actionId,
            payload,
        };

        var attempts = 0;
        while (true)
        {
            try
            {
                using var stream = new FileStream(_layout.TracePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
                using var writer = new StreamWriter(stream);
                writer.Write(JsonSerializer.Serialize(envelope, _jsonOptions));
                writer.Write(Environment.NewLine);
                writer.Flush();
                return;
            }
            catch (IOException) when (attempts < 20)
            {
                attempts += 1;
                Thread.Sleep(100);
            }
        }
    }

    private HarnessActionResult ExecuteAction(HarnessActionEnvelope envelope)
    {
        var action = envelope.Action;
        var startedAt = DateTimeOffset.UtcNow;
        try
        {
            // Test harness actions must fail fast and always produce a result artifact.
            // The previous main-thread dispatch path could stall without ever writing an
            // outbox result, which made unattended scenarios impossible to triage.
            return ExecuteActionCore(envelope, startedAt);
        }
        catch (Exception exception)
        {
            return BuildResult(action, startedAt, "failed", true, failureKind: exception.Message);
        }
    }

    private HarnessActionResult ExecuteActionCore(HarnessActionEnvelope envelope, DateTimeOffset startedAt)
    {
        var action = envelope.Action;
        return action.Kind.ToLowerInvariant() switch
        {
            "noop" => BuildResult(action, startedAt, "ok", false, "noop"),
            "dispatch_node" => DispatchNode(action, envelope.State, startedAt),
            "click_button" => ClickButton(action, envelope.State, startedAt),
            "click_card" => ClickCard(action, envelope.State, startedAt),
            "confirm" => ClickNamedAction(action, envelope.State, startedAt, "__confirm__"),
            "cancel" => ClickNamedAction(action, envelope.State, startedAt, "__cancel__"),
            "end_turn" => ClickNamedAction(action, envelope.State, startedAt, "__end_turn__"),
            "choose_reward" => ChooseReward(action, envelope.State, startedAt),
            "choose_map_node" => ChooseMapNode(action, envelope.State, startedAt),
            _ => BuildResult(action, startedAt, "not-supported-yet", false, failureKind: $"{action.Kind}-not-supported-yet"),
        };
    }

    private static HarnessActionResult? ExecuteOnMainThread(Func<HarnessActionResult> action, int timeoutMs)
    {
        if (IsGameMainThread())
        {
            return action();
        }

        HarnessActionResult? result = null;
        Exception? exception = null;
        using var completed = new ManualResetEventSlim(false);

        if (!TryDispatchDeferred(() =>
        {
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                completed.Set();
            }
        }))
        {
            return action();
        }

        if (!completed.Wait(Math.Max(timeoutMs + 5_000, 5_000)))
        {
            return null;
        }

        if (exception is not null)
        {
            throw exception;
        }

        return result;
    }

    private static bool TryDispatchDeferred(Action callback)
    {
        try
        {
            var callableType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => string.Equals(type.FullName, "Godot.Callable", StringComparison.Ordinal));
            if (callableType is null)
            {
                return false;
            }

            var fromMethod = callableType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(method =>
                {
                    if (!string.Equals(method.Name, "From", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    var parameters = method.GetParameters();
                    return parameters.Length == 1 && typeof(Delegate).IsAssignableFrom(parameters[0].ParameterType);
                });
            if (fromMethod is null)
            {
                return false;
            }

            var callable = fromMethod.Invoke(null, new object[] { callback });
            if (callable is null)
            {
                return false;
            }

            var callDeferredMethod = callableType.GetMethod("CallDeferred", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (callDeferredMethod is null)
            {
                return false;
            }

            callDeferredMethod.Invoke(callable, Array.Empty<object>());
            return true;
        }
        catch
        {
            return false;
        }
    }

    private HarnessActionResult ClickNamedAction(HarnessAction action, CompanionState state, DateTimeOffset startedAt, string sentinel)
    {
        var aliases = ResolveHarnessAliases(action.TargetLabel ?? sentinel, state);
        return ClickButton(action with { TargetLabel = action.TargetLabel ?? sentinel }, state, startedAt, aliases);
    }

    private HarnessActionResult ClickButton(
        HarnessAction action,
        CompanionState state,
        DateTimeOffset startedAt,
        IReadOnlyList<string>? aliases = null)
    {
        var resolvedAliases = aliases ?? ResolveHarnessAliases(action.TargetLabel, state);
        HarnessActionResult? lastFailure = null;

        for (var attempt = 1; attempt <= 2; attempt += 1)
        {
            var sceneRoots = TryResolveSceneRoots();
            var scoredCandidates = sceneRoots.Count == 0
                ? Array.Empty<(NodeCandidate Candidate, int Score)>()
                : sceneRoots
                    .SelectMany(root => EnumerateSceneNodes(root, 4096))
                    .Select(CreateNodeCandidate)
                    .Where(candidate => !string.IsNullOrWhiteSpace(candidate.Label) || !string.IsNullOrWhiteSpace(candidate.TypeName))
                    .Select(candidate => (Candidate: candidate, Score: ScoreButtonCandidate(candidate, action.TargetLabel, resolvedAliases)))
                    .OrderByDescending(entry => entry.Score)
                    .ToArray();
            var candidates = scoredCandidates.Select(entry => entry.Candidate).ToArray();
            var primaryCandidate = scoredCandidates.FirstOrDefault(entry => entry.Score > 0).Candidate;
            var snapshot = CaptureClickAttemptSnapshot(action, sceneRoots, primaryCandidate, state.Scene.SceneType);
            var expectations = ResolveClickExpectations(action, snapshot);

            AppendClickFingerprint(action, attempt, "preflight", snapshot, expectations);

            var preflightFailure = EvaluateClickPreflight(action, sceneRoots, primaryCandidate, snapshot);
            if (!string.IsNullOrWhiteSpace(preflightFailure))
            {
                var failureKind = preflightFailure == "target-button-unavailable"
                    ? $"{preflightFailure}:{SummarizeCandidates(candidates)}"
                    : preflightFailure;
                lastFailure = BuildResult(
                    action,
                    startedAt,
                    "failed",
                    true,
                    observed: snapshot.ButtonLabel,
                    failureKind: $"preflight-unsatisfied:{failureKind}");
                AppendClickFingerprint(action, attempt, "preflight-failed", snapshot, expectations, failureKind: lastFailure.FailureKind);
                if (attempt >= 2)
                {
                    return lastFailure;
                }

                Thread.Sleep(300);
                continue;
            }

            var dispatch = TryDispatchClickAction(action, state, startedAt, primaryCandidate, snapshot.ButtonLabel, candidates);
            AppendClickFingerprint(
                action,
                attempt,
                "dispatch",
                snapshot,
                expectations,
                dispatchPath: dispatch.DispatchPath,
                failureKind: dispatch.Success ? null : dispatch.FailureKind);
            if (!dispatch.Success)
            {
                lastFailure = BuildResult(
                    action,
                    startedAt,
                    "failed",
                    true,
                    observed: dispatch.Observed,
                    failureKind: dispatch.FailureKind);
                if (attempt >= 2)
                {
                    return lastFailure;
                }

                Thread.Sleep(300);
                continue;
            }

            var postflight = WaitForClickPostflight(action, snapshot, dispatch.Candidate ?? primaryCandidate, expectations);
            AppendClickFingerprint(
                action,
                attempt,
                "postflight",
                postflight.FinalSnapshot,
                expectations,
                dispatchPath: dispatch.DispatchPath,
                actualScene: postflight.ActualScene,
                sceneChanged: postflight.SceneTransitionObserved,
                buttonStateChanged: postflight.ButtonStateChanged,
                auxiliaryChanged: postflight.AuxiliaryChanged,
                failureKind: postflight.Success ? null : postflight.FailureKind);

            var observed = CombineObserved(dispatch.Observed, postflight.Observed);
            if (postflight.Success)
            {
                return BuildResult(action, startedAt, "ok", false, observed: observed);
            }

            lastFailure = BuildResult(
                action,
                startedAt,
                "failed",
                true,
                observed: observed,
                failureKind: postflight.FailureKind);
            if (attempt >= 2)
            {
                return lastFailure;
            }

            Thread.Sleep(400);
        }

        return lastFailure ?? BuildResult(action, startedAt, "failed", true, failureKind: "click-attempt-exhausted");
    }

    private ClickAttemptSnapshot CaptureClickAttemptSnapshot(
        HarnessAction action,
        IReadOnlyList<object> sceneRoots,
        NodeCandidate? candidate,
        string? fallbackScene)
    {
        var liveScene = TryReadLiveExportScene(out var resolvedLiveScene)
            ? NormalizeHarnessScene(resolvedLiveScene)
            : string.Empty;
        var observedScene = EnumerateObservedScenes(sceneRoots)
            .Select(NormalizeHarnessScene)
            .FirstOrDefault(scene => !string.IsNullOrWhiteSpace(scene))
            ?? string.Empty;
        var scene = NormalizeHarnessScene(fallbackScene);
        if (string.IsNullOrWhiteSpace(scene) || string.Equals(scene, "unknown", StringComparison.OrdinalIgnoreCase))
        {
            scene = !string.IsNullOrWhiteSpace(liveScene) ? liveScene : observedScene;
        }

        if (string.IsNullOrWhiteSpace(scene))
        {
            scene = "unknown";
        }

        var buttonNode = candidate?.ActivationNode ?? candidate?.Node;
        var buttonVisible = TryEvaluateNodeVisibility(buttonNode);
        var buttonEnabled = TryGetNodeEnabled(buttonNode);
        var overlayCount = CountBlockingOverlays(sceneRoots);
        var inputLocked = IsInputLocked(sceneRoots, out var inputLockReason);
        var buttonLabel = candidate?.Label ?? candidate?.NodeName ?? candidate?.TypeName ?? string.Empty;

        return new ClickAttemptSnapshot(
            scene,
            liveScene,
            overlayCount,
            inputLocked,
            inputLockReason,
            action.TargetLabel ?? string.Empty,
            buttonLabel,
            buttonVisible,
            buttonEnabled);
    }

    private static string? EvaluateClickPreflight(
        HarnessAction action,
        IReadOnlyList<object> sceneRoots,
        NodeCandidate? primaryCandidate,
        ClickAttemptSnapshot snapshot)
    {
        if (sceneRoots.Count == 0)
        {
            return "scene-root-unavailable";
        }

        var isCancel = MatchesSentinel(action.TargetLabel, "__cancel__");
        if (!isCancel && snapshot.OverlayCount > 0)
        {
            return $"blocking-overlay:{snapshot.OverlayCount}";
        }

        if (!isCancel && snapshot.InputLocked)
        {
            return string.IsNullOrWhiteSpace(snapshot.InputLockReason)
                ? "input-locked"
                : $"input-locked:{snapshot.InputLockReason}";
        }

        if (primaryCandidate is null)
        {
            return "target-button-unavailable";
        }

        if (!snapshot.ButtonVisible)
        {
            return "target-button-hidden";
        }

        if (snapshot.ButtonEnabled == false)
        {
            return "target-button-disabled";
        }

        return null;
    }

    private ClickDispatchOutcome TryDispatchClickAction(
        HarnessAction action,
        CompanionState state,
        DateTimeOffset startedAt,
        NodeCandidate? primaryCandidate,
        string? preflightObserved,
        IReadOnlyList<NodeCandidate> candidates)
    {
        HarnessActionResult? semanticFailure = null;
        var semanticResult = TryExecuteButtonSemanticAction(action, state, startedAt);
        if (semanticResult is not null)
        {
            if (string.Equals(semanticResult.Status, "ok", StringComparison.OrdinalIgnoreCase))
            {
                return new ClickDispatchOutcome(
                    true,
                    "semantic-ui",
                    semanticResult.ObservedStateDelta ?? preflightObserved,
                    null,
                    primaryCandidate);
            }

            if (MatchesSentinel(action.TargetLabel, "__cancel__"))
            {
                return new ClickDispatchOutcome(
                    false,
                    "semantic-ui",
                    semanticResult.ObservedStateDelta ?? preflightObserved,
                    semanticResult.FailureKind ?? "dispatch-failed",
                    primaryCandidate);
            }

            semanticFailure = semanticResult;
        }

        if (primaryCandidate is null)
        {
            return new ClickDispatchOutcome(
                false,
                "none",
                semanticFailure?.ObservedStateDelta ?? preflightObserved,
                semanticFailure?.FailureKind ?? $"dispatch-target-unavailable:{SummarizeCandidates(candidates)}",
                null);
        }

        if (TryDispatchClickNode(primaryCandidate.ActivationNode ?? primaryCandidate.Node, out var dispatchPath))
        {
            var observed = primaryCandidate.Label ?? primaryCandidate.NodeName ?? primaryCandidate.TypeName;
            observed = CombineObserved(semanticFailure?.ObservedStateDelta ?? preflightObserved, observed);
            return new ClickDispatchOutcome(true, dispatchPath, observed, null, primaryCandidate);
        }

        return new ClickDispatchOutcome(
            false,
            "dispatch-failed",
            semanticFailure?.ObservedStateDelta ?? preflightObserved,
            semanticFailure?.FailureKind ?? "dispatch-failed",
            primaryCandidate);
    }

    private ClickPostflightOutcome WaitForClickPostflight(
        HarnessAction action,
        ClickAttemptSnapshot before,
        NodeCandidate? primaryCandidate,
        IReadOnlyList<string> expectations)
    {
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(Math.Max(action.TimeoutMs, 1_000));
        var expectedScene = NormalizeHarnessScene(action.ExpectedStateDelta);
        ClickAttemptSnapshot finalSnapshot = before;
        string? lastObserved = null;

        while (DateTimeOffset.UtcNow < deadline)
        {
            Thread.Sleep(200);
            var sceneRoots = TryResolveSceneRoots();
            finalSnapshot = CaptureClickAttemptSnapshot(action, sceneRoots, primaryCandidate, before.Scene);

            var sceneTransitionObserved = false;
            string? sceneObserved = null;
            if (!string.IsNullOrWhiteSpace(expectedScene))
            {
                sceneTransitionObserved = TryProbeExpectedSceneTransition(expectedScene, sceneRoots, out var observedScene);
                sceneObserved = observedScene;
            }
            else if (!string.Equals(finalSnapshot.Scene, before.Scene, StringComparison.OrdinalIgnoreCase))
            {
                sceneTransitionObserved = true;
                sceneObserved = $"scene-change:{before.Scene}->{finalSnapshot.Scene}";
            }

            var buttonStateChanged = DidButtonStateChange(before, finalSnapshot);
            var buttonObserved = buttonStateChanged
                ? $"button-state:{before.ButtonVisible}/{FormatEnabled(before.ButtonEnabled)}->{finalSnapshot.ButtonVisible}/{FormatEnabled(finalSnapshot.ButtonEnabled)}"
                : null;
            var auxiliaryChanged = TryObserveAuxiliaryClickTransition(action, before, finalSnapshot, sceneRoots, out var auxiliaryObserved);
            lastObserved = CombineObserved(sceneObserved, CombineObserved(buttonObserved, auxiliaryObserved));

            if (sceneTransitionObserved || buttonStateChanged || auxiliaryChanged)
            {
                return new ClickPostflightOutcome(
                    true,
                    null,
                    lastObserved,
                    finalSnapshot.Scene,
                    finalSnapshot,
                    sceneTransitionObserved,
                    buttonStateChanged,
                    auxiliaryChanged);
            }
        }

        var actualScene = finalSnapshot.Scene;
        var failureKind = !string.IsNullOrWhiteSpace(expectedScene) && !SceneMatchesOrProgressesPastHarness(actualScene, expectedScene)
            ? $"transition-mismatch:{actualScene}"
            : "postflight-timeout";
        return new ClickPostflightOutcome(
            false,
            failureKind,
            lastObserved,
            actualScene,
            finalSnapshot,
            false,
            false,
            false);
    }

    private void AppendClickFingerprint(
        HarnessAction action,
        int attempt,
        string phase,
        ClickAttemptSnapshot snapshot,
        IReadOnlyList<string> expectations,
        string? dispatchPath = null,
        string? actualScene = null,
        bool? sceneChanged = null,
        bool? buttonStateChanged = null,
        bool? auxiliaryChanged = null,
        string? failureKind = null)
    {
        AppendTrace("click-fingerprint", action.ActionId, new
        {
            attempt,
            phase,
            scene = snapshot.Scene,
            liveScene = snapshot.LiveScene,
            target = snapshot.TargetLabel,
            button = snapshot.ButtonLabel,
            buttonVisible = snapshot.ButtonVisible,
            buttonEnabled = snapshot.ButtonEnabled,
            overlayCount = snapshot.OverlayCount,
            inputLocked = snapshot.InputLocked,
            inputLockReason = snapshot.InputLockReason,
            expected = expectations,
            actualScene,
            sceneChanged,
            buttonStateChanged,
            auxiliaryChanged,
            dispatchPath,
            failureKind,
        });
    }

    private static IReadOnlyList<string> ResolveClickExpectations(HarnessAction action, ClickAttemptSnapshot snapshot)
    {
        var expectations = new List<string>();
        var expectedScene = NormalizeHarnessScene(action.ExpectedStateDelta);
        if (!string.IsNullOrWhiteSpace(expectedScene))
        {
            expectations.Add($"scene:{expectedScene}");
        }

        expectations.Add("button-state-change");

        if (MatchesSentinel(action.TargetLabel, "__cancel__") || snapshot.OverlayCount > 0)
        {
            expectations.Add("overlay-count-change");
        }
        else if (MatchesSentinel(action.TargetLabel, "__confirm__") || string.Equals(expectedScene, "map", StringComparison.OrdinalIgnoreCase))
        {
            expectations.Add("run-start-signal");
        }
        else
        {
            expectations.Add("follow-up-scene-change");
        }

        return expectations
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool TryDispatchClickNode(object node, out string dispatchPath)
    {
        foreach (var attempt in new (string Path, Func<bool> Try)[]
                 {
                     ("ui-signal:pressed", () => TryInvokeMethodOnMainThread(node, "EmitSignal", 2_000, out _, "pressed")),
                     ("ui-signal:pressed-node", () => TryInvokeMethodOnMainThread(node, "EmitSignal", 2_000, out _, "pressed", node)),
                     ("ui-signal:released", () => TryInvokeMethodOnMainThread(node, "EmitSignal", 2_000, out _, "released")),
                     ("ui-handler:OnPressed", () => TryInvokeMethodOnMainThread(node, "OnPressed", 2_000, out _)),
                     ("ui-handler:_Pressed", () => TryInvokeMethodOnMainThread(node, "_Pressed", 2_000, out _)),
                     ("ui-handler:Pressed", () => TryInvokeMethodOnMainThread(node, "Pressed", 2_000, out _)),
                     ("ui-handler:OnRelease", () => TryInvokeMethodOnMainThread(node, "OnRelease", 2_000, out _)),
                     ("ui-handler:OnReleased", () => TryInvokeMethodOnMainThread(node, "OnReleased", 2_000, out _)),
                     ("ui-handler:OnClick", () => TryInvokeMethodOnMainThread(node, "OnClick", 2_000, out _)),
                     ("ui-handler:OnClicked", () => TryInvokeMethodOnMainThread(node, "OnClicked", 2_000, out _)),
                     ("input-path:ForceClick", () => TryInvokeMethodOnMainThread(node, "ForceClick", 2_000, out _)),
                     ("input-path:Press", () => TryInvokeMethodOnMainThread(node, "Press", 2_000, out _)),
                     ("input-path:Click", () => TryInvokeMethodOnMainThread(node, "Click", 2_000, out _)),
                     ("input-path:Activate", () => TryInvokeMethodOnMainThread(node, "Activate", 2_000, out _)),
                 })
        {
            try
            {
                if (attempt.Try())
                {
                    dispatchPath = attempt.Path;
                    return true;
                }
            }
            catch
            {
                // Ignore handler-specific failures and continue down the allowed dispatch order.
            }
        }

        dispatchPath = "dispatch-failed";
        return false;
    }

    private static bool TryEvaluateNodeVisibility(object? node)
    {
        if (node is null)
        {
            return false;
        }

        try
        {
            return IsVisibleNode(node);
        }
        catch
        {
            return false;
        }
    }

    private static bool? TryGetNodeEnabled(object? node)
    {
        if (node is null)
        {
            return null;
        }

        try
        {
            var disabled = TryConvertToBool(TryGetMemberValue(node, "Disabled"))
                           ?? TryConvertToBool(TryGetMemberValue(node, "IsDisabled"))
                           ?? TryConvertToBool(TryInvokeMethod(node, "IsDisabled"));
            if (disabled is not null)
            {
                return !disabled.Value;
            }

            return TryConvertToBool(TryGetMemberValue(node, "Enabled"))
                   ?? TryConvertToBool(TryGetMemberValue(node, "IsEnabled"))
                   ?? TryConvertToBool(TryInvokeMethod(node, "IsEnabled"));
        }
        catch
        {
            return null;
        }
    }

    private static int CountBlockingOverlays(IReadOnlyList<object> sceneRoots)
    {
        return EnumerateBlockingOverlayNodes(sceneRoots).Count();
    }

    private static IEnumerable<object> EnumerateBlockingOverlayNodes(IReadOnlyList<object> sceneRoots)
    {
        var seen = new HashSet<int>();

        var feedbackScreen = ResolveFeedbackScreen(sceneRoots);
        if (feedbackScreen is not null && IsBlockingFeedbackScreen(feedbackScreen) && seen.Add(RuntimeHelpers.GetHashCode(feedbackScreen)))
        {
            yield return feedbackScreen;
        }

        var timeoutOverlay = FindFirstNodeByType(sceneRoots, "NMultiplayerTimeoutOverlay");
        if (timeoutOverlay is not null && IsVisibleNode(timeoutOverlay) && seen.Add(RuntimeHelpers.GetHashCode(timeoutOverlay)))
        {
            yield return timeoutOverlay;
        }

        foreach (var typeSuffix in new[] { "NAbandonRunConfirmPopup", "NVerticalPopup", "NModalPopup" })
        {
            var popup = FindFirstVisibleNodeByType(sceneRoots, typeSuffix);
            if (popup is not null && seen.Add(RuntimeHelpers.GetHashCode(popup)))
            {
                yield return popup;
            }
        }
    }

    private static bool IsInputLocked(IReadOnlyList<object> sceneRoots, out string reason)
    {
        if (CountBlockingOverlays(sceneRoots) > 0)
        {
            reason = "blocking-overlay";
            return true;
        }

        var transition = FindFirstVisibleNodeByType(sceneRoots, "NTransition");
        if (transition is not null)
        {
            var active = TryConvertToBool(TryGetMemberValue(transition, "Active"))
                         ?? TryConvertToBool(TryGetMemberValue(transition, "IsActive"))
                         ?? true;
            if (active)
            {
                reason = "transition-active";
                return true;
            }
        }

        var game = TryResolveGame(sceneRoots);
        if (game is not null)
        {
            foreach (var memberName in new[] { "InputLocked", "IsInputLocked", "InteractionLocked", "IsTransitioning", "IsLoadingScene" })
            {
                if (TryConvertToBool(TryGetMemberValue(game, memberName)) == true)
                {
                    reason = memberName;
                    return true;
                }
            }
        }

        reason = string.Empty;
        return false;
    }

    private static bool DidButtonStateChange(ClickAttemptSnapshot before, ClickAttemptSnapshot after)
    {
        return before.ButtonVisible != after.ButtonVisible
               || before.ButtonEnabled != after.ButtonEnabled;
    }

    private static bool TryObserveAuxiliaryClickTransition(
        HarnessAction action,
        ClickAttemptSnapshot before,
        ClickAttemptSnapshot after,
        IReadOnlyList<object> sceneRoots,
        out string? observed)
    {
        observed = null;

        if ((MatchesSentinel(action.TargetLabel, "__confirm__")
             || string.Equals(NormalizeHarnessScene(action.ExpectedStateDelta), "map", StringComparison.OrdinalIgnoreCase))
            && (TryProbeRunStartFromLiveExport(out var liveObserved) || TryProbeRunStart(sceneRoots, out liveObserved)))
        {
            observed = liveObserved;
            return true;
        }

        if ((MatchesSentinel(action.TargetLabel, "__cancel__") || before.OverlayCount > 0) && after.OverlayCount < before.OverlayCount)
        {
            observed = $"overlay-count:{before.OverlayCount}->{after.OverlayCount}";
            return true;
        }

        if (!string.IsNullOrWhiteSpace(before.LiveScene)
            && !string.IsNullOrWhiteSpace(after.LiveScene)
            && !string.Equals(before.LiveScene, after.LiveScene, StringComparison.OrdinalIgnoreCase))
        {
            observed = $"live-scene:{before.LiveScene}->{after.LiveScene}";
            return true;
        }

        return false;
    }

    private static string FormatEnabled(bool? enabled)
    {
        return enabled is null ? "?" : enabled.Value ? "enabled" : "disabled";
    }

    private HarnessActionResult ClickCard(HarnessAction action, CompanionState state, DateTimeOffset startedAt)
    {
        var semanticResult = ExecuteOnMainThread(() => TryExecuteCombatCardAction(action, startedAt), action.TimeoutMs);
        if (semanticResult is not null)
        {
            if (string.Equals(semanticResult.Status, "ok", StringComparison.OrdinalIgnoreCase))
            {
                return semanticResult;
            }

            if (MatchesSentinel(action.TargetLabel, "__first_playable__"))
            {
                return semanticResult;
            }
        }

        return ChooseSemanticNode(
            action,
            state,
            startedAt,
            ResolveHarnessAliases(action.TargetLabel ?? "__first_playable__", state),
            new[] { "card", "hand", "play", "attack", "skill", "power", "카드" },
            candidate => candidate.TypeName.Contains("Card", StringComparison.OrdinalIgnoreCase)
                         || candidate.Label?.Contains("card", StringComparison.OrdinalIgnoreCase) == true);
    }

    private HarnessActionResult ChooseReward(HarnessAction action, CompanionState state, DateTimeOffset startedAt)
    {
        var semanticResult = TryExecuteRewardSemanticAction(action, startedAt);
        if (semanticResult is not null)
        {
            return semanticResult;
        }

        return ChooseSemanticNode(
            action,
            state,
            startedAt,
            ResolveHarnessAliases(action.TargetLabel ?? "__first_reward__", state),
            new[] { "reward", "card", "relic", "potion", "보상", "카드", "유물", "포션" },
            candidate => candidate.TypeName.Contains("Reward", StringComparison.OrdinalIgnoreCase)
                         || candidate.TypeName.Contains("Card", StringComparison.OrdinalIgnoreCase)
                         || candidate.TypeName.Contains("Relic", StringComparison.OrdinalIgnoreCase)
                         || candidate.TypeName.Contains("Potion", StringComparison.OrdinalIgnoreCase));
    }

    private HarnessActionResult ChooseMapNode(HarnessAction action, CompanionState state, DateTimeOffset startedAt)
    {
        var semanticResult = TryExecuteMapSemanticAction(action, startedAt);
        if (semanticResult is not null)
        {
            return semanticResult;
        }

        return ChooseSemanticNode(
            action,
            state,
            startedAt,
            ResolveHarnessAliases(action.TargetLabel ?? "__first_combat__", state),
            new[] { "map", "node", "combat", "battle", "room", "전투", "맵" },
            candidate => candidate.TypeName.Contains("Map", StringComparison.OrdinalIgnoreCase)
                         || candidate.TypeName.Contains("Node", StringComparison.OrdinalIgnoreCase)
                         || candidate.TypeName.Contains("Room", StringComparison.OrdinalIgnoreCase));
    }

    private HarnessActionResult ChooseSemanticNode(
        HarnessAction action,
        CompanionState state,
        DateTimeOffset startedAt,
        IReadOnlyList<string> aliases,
        IReadOnlyList<string> preferredKeywords,
        Func<NodeCandidate, bool> domainPredicate)
    {
        var sceneRoots = TryResolveSceneRoots();
        if (sceneRoots.Count == 0)
        {
            return BuildResult(action, startedAt, "failed", true, failureKind: "scene-root-unavailable");
        }

        var candidates = sceneRoots
            .SelectMany(root => EnumerateSceneNodes(root, 4096))
            .Select(CreateNodeCandidate)
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate.Label) || !string.IsNullOrWhiteSpace(candidate.TypeName))
            .Where(candidate => !LooksLikePlaceholder(candidate.Label))
            .OrderByDescending(candidate => ScoreSemanticCandidate(candidate, action.TargetLabel, aliases, preferredKeywords, domainPredicate))
            .ToArray();

        foreach (var candidate in candidates)
        {
            if (ScoreSemanticCandidate(candidate, action.TargetLabel, aliases, preferredKeywords, domainPredicate) <= 0)
            {
                break;
            }

            if (TryActivateCandidate(candidate))
            {
                return BuildResult(action, startedAt, "ok", false, observed: candidate.Label ?? candidate.TypeName);
            }
        }

        return BuildResult(action, startedAt, "failed", true, failureKind: $"no-semantic-candidate:{SummarizeCandidates(candidates)}");
    }

    private HarnessActionResult TryExecuteCombatCardAction(HarnessAction action, DateTimeOffset startedAt)
    {
        var sceneRoots = TryResolveSceneRoots();
        if (sceneRoots.Count > 0
            && TryDismissBlockingOverlays(sceneRoots, out var overlayObserved)
            && !string.IsNullOrWhiteSpace(overlayObserved))
        {
            AppendTrace("combat-overlay-dismissed", action.ActionId, new
            {
                observed = overlayObserved,
                visibleTypes = SummarizeVisibleTypes(sceneRoots),
            });
            Thread.Sleep(400);
        }

        var hand = TryResolvePlayerHand(waitForReady: true);
        var holders = ResolveCombatHandHolders(hand);
        if (holders.Length == 0)
        {
            return BuildResult(action, startedAt, "failed", true, failureKind: hand is null ? "player-hand-unavailable" : "hand-holder-unavailable");
        }

        var holderEntries = holders
            .Select(holder => new
            {
                Holder = holder,
                CardModel = TryResolveCardModelFromHolder(holder),
            })
            .Where(entry => entry.CardModel is not null)
            .ToArray();

        if (holderEntries.Length == 0)
        {
            return BuildResult(action, startedAt, "failed", true, failureKind: "hand-holder-unavailable");
        }

        var failureDetails = new List<string>();
        foreach (var entry in holderEntries)
        {
            var cardModel = entry.CardModel!;
            var cardName = DescribeCardModel(cardModel);
            if (LooksLikePlaceholder(cardName))
            {
                failureDetails.Add($"placeholder:{cardName}");
                continue;
            }

            if (!TryResolveCombatTarget(cardModel, out var target, out var targetSummary, out var targetFailure))
            {
                failureDetails.Add($"{cardName}:target={targetFailure}");
                continue;
            }

            if (!TryCanPlayCard(cardModel, target, out var canPlayFailure))
            {
                failureDetails.Add($"{cardName}:canplay={canPlayFailure}");
                continue;
            }

            if (!TryInvokeMethod(cardModel, "TryManualPlay", out var playResult, target))
            {
                failureDetails.Add($"{cardName}:try-manual-play-missing");
                continue;
            }

            var played = TryConvertToBool(playResult);
            if (played == false)
            {
                failureDetails.Add($"{cardName}:try-manual-play=false");
                continue;
            }

            AppendTrace("combat-card-played", action.ActionId, new
            {
                card = cardName,
                target = targetSummary,
                holderType = entry.Holder.GetType().FullName,
                modelType = cardModel.GetType().FullName,
            });
            return BuildResult(action, startedAt, "ok", false, observed: $"played:{cardName}|target:{targetSummary}");
        }

        var failureSummary = string.Join("; ", failureDetails.Take(8));
        AppendTrace("combat-card-unavailable", action.ActionId, new
        {
            holderCount = holderEntries.Length,
            failures = failureDetails.Take(8).ToArray(),
        });
        return BuildResult(
            action,
            startedAt,
            "failed",
            true,
            failureKind: string.IsNullOrWhiteSpace(failureSummary) ? "no-playable-card-found" : $"no-playable-card-found:{failureSummary}");
    }

    private static object? TryResolvePlayerHand(bool waitForReady = false)
    {
        var deadline = DateTimeOffset.UtcNow.Add(waitForReady ? TimeSpan.FromSeconds(5) : TimeSpan.Zero);
        do
        {
            var handType = TryFindLoadedType("MegaCrit.Sts2.Core.Nodes.Combat.NPlayerHand");
            var hand = handType is null ? null : TryGetMemberValue(handType, "Instance");
            if (hand is not null)
            {
                return hand;
            }

            var sceneRoots = TryResolveSceneRoots();
            hand = FindFirstVisibleNodeByType(sceneRoots, "NPlayerHand") ?? FindFirstNodeByType(sceneRoots, "NPlayerHand");
            if (hand is not null)
            {
                return hand;
            }

            if (!waitForReady || DateTimeOffset.UtcNow >= deadline)
            {
                break;
            }

            Thread.Sleep(200);
        }
        while (true);

        return null;
    }

    private static IEnumerable<object> EnumerateHandHolders(object hand)
    {
        var activeHolders = ExpandEnumerable(TryGetMemberValue(hand, "ActiveHolders")).ToArray();
        if (activeHolders.Length > 0)
        {
            foreach (var holder in activeHolders)
            {
                yield return holder;
            }

            yield break;
        }

        var holders = ExpandEnumerable(TryGetMemberValue(hand, "Holders")).ToArray();
        if (holders.Length > 0)
        {
            foreach (var holder in holders)
            {
                yield return holder;
            }

            yield break;
        }

        var container = TryGetMemberValue(hand, "CardHolderContainer");
        foreach (var holder in ExpandEnumerable(TryInvokeMethod(container ?? typeof(object), "GetChildren")))
        {
            if (TypeNameContains(holder, "HandCardHolder") || TypeNameContains(holder, "CardHolder"))
            {
                yield return holder;
            }
        }
    }

    private static object[] ResolveCombatHandHolders(object? hand)
    {
        if (hand is not null)
        {
            var holders = EnumerateHandHolders(hand).ToArray();
            if (holders.Length > 0)
            {
                return holders;
            }
        }

        var sceneRoots = TryResolveSceneRoots();
        return sceneRoots
            .SelectMany(root => EnumerateSceneNodes(root, 4096))
            .Where(node =>
                TypeNameContains(node, "HandCardHolder")
                || TypeNameContains(node, "CombatCardHolder")
                || TypeNameContains(node, "HandHolder"))
            .Where(IsVisibleNode)
            .ToArray();
    }

    private static object? TryResolveCardModelFromHolder(object holder)
    {
        var directModel = TryGetMemberValue(holder, "CardModel")
                          ?? TryGetMemberValue(holder, "Model");
        if (directModel is not null)
        {
            return directModel;
        }

        var cardNode = TryGetMemberValue(holder, "CardNode")
                       ?? TryGetMemberValue(holder, "_cardNode")
                       ?? TryInvokeMethod(holder, "GetCardNode");
        return cardNode is null
            ? null
            : TryGetMemberValue(cardNode, "Model")
              ?? TryGetMemberValue(cardNode, "CardModel");
    }

    private static bool TryResolveCombatTarget(object cardModel, out object? target, out string targetSummary, out string failureKind)
    {
        target = null;
        targetSummary = "none";
        failureKind = string.Empty;

        var targetType = TryConvertToDisplayString(TryGetMemberValue(cardModel, "TargetType")) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(targetType)
            || targetType.Contains("None", StringComparison.OrdinalIgnoreCase)
            || targetType.Contains("NoTarget", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var combatState = TryResolveCombatState(cardModel);
        if (combatState is null)
        {
            failureKind = "combat-state-unavailable";
            return false;
        }

        if (targetType.Contains("Enemy", StringComparison.OrdinalIgnoreCase))
        {
            target = ExpandEnumerable(TryGetMemberValue(combatState, "HittableEnemies")).FirstOrDefault();
            if (target is null)
            {
                failureKind = "enemy-target-unavailable";
                return false;
            }

            targetSummary = DescribeCombatTarget(target);
            return true;
        }

        if (targetType.Contains("Ally", StringComparison.OrdinalIgnoreCase)
            || targetType.Contains("Player", StringComparison.OrdinalIgnoreCase)
            || targetType.Contains("Self", StringComparison.OrdinalIgnoreCase))
        {
            target = ExpandEnumerable(TryGetMemberValue(combatState, "PlayerCreatures")).FirstOrDefault();
            if (target is null)
            {
                var owner = TryGetMemberValue(cardModel, "Owner");
                target = owner is null ? null : TryGetMemberValue(owner, "Creature");
            }

            if (target is null)
            {
                failureKind = "ally-target-unavailable";
                return false;
            }

            targetSummary = DescribeCombatTarget(target);
            return true;
        }

        return true;
    }

    private static object? TryResolveCombatState(object cardModel)
    {
        var combatState = TryGetMemberValue(cardModel, "CombatState");
        if (combatState is not null)
        {
            return combatState;
        }

        var owner = TryGetMemberValue(cardModel, "Owner");
        var creature = owner is null ? null : TryGetMemberValue(owner, "Creature");
        combatState = creature is null ? null : TryGetMemberValue(creature, "CombatState");
        if (combatState is not null)
        {
            return combatState;
        }

        var combatManagerType = TryFindLoadedType("MegaCrit.Sts2.Core.Combat.CombatManager");
        var combatManager = combatManagerType is null ? null : TryGetMemberValue(combatManagerType, "Instance");
        if (combatManager is not null && TryInvokeMethod(combatManager, "DebugOnlyGetState", out var managerState))
        {
            return managerState;
        }

        return null;
    }

    private static bool TryCanPlayCard(object cardModel, object? target, out string failureKind)
    {
        if (TryInvokeMethod(cardModel, "CanPlayTargeting", out var canPlayTargeting, target))
        {
            var canPlayTargetingValue = TryConvertToBool(canPlayTargeting);
            if (canPlayTargetingValue == true)
            {
                failureKind = string.Empty;
                return true;
            }

            if (canPlayTargetingValue == false)
            {
                failureKind = "can-play-targeting-false";
                return false;
            }
        }

        if (TryInvokeMethod(cardModel, "CanPlay", out var canPlay))
        {
            var canPlayValue = TryConvertToBool(canPlay);
            if (canPlayValue == true)
            {
                failureKind = string.Empty;
                return true;
            }

            if (canPlayValue == false)
            {
                failureKind = "can-play-false";
                return false;
            }
        }

        failureKind = "can-play-unavailable";
        return false;
    }

    private static string DescribeCardModel(object cardModel)
    {
        var displayName = TryConvertToDisplayString(TryGetMemberValue(cardModel, "DisplayName"))
                          ?? TryConvertToDisplayString(TryGetMemberValue(cardModel, "Name"));
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName!;
        }

        var id = TryGetMemberValue(cardModel, "Id");
        var idEntry = TryConvertToDisplayString(TryGetMemberValue(id ?? typeof(object), "Entry"))
                      ?? TryConvertToDisplayString(TryGetMemberValue(id ?? typeof(object), "Name"));
        if (!string.IsNullOrWhiteSpace(idEntry))
        {
            return idEntry!;
        }

        return cardModel.GetType().Name;
    }

    private static string DescribeCombatTarget(object target)
    {
        var label = TryConvertToDisplayString(TryGetMemberValue(target, "DisplayName"))
                    ?? TryConvertToDisplayString(TryGetMemberValue(target, "Name"));
        if (!string.IsNullOrWhiteSpace(label))
        {
            return label!;
        }

        var monster = TryGetMemberValue(target, "Monster");
        label = TryConvertToDisplayString(TryGetMemberValue(monster ?? typeof(object), "DisplayName"))
                ?? TryConvertToDisplayString(TryGetMemberValue(monster ?? typeof(object), "Name"));
        if (!string.IsNullOrWhiteSpace(label))
        {
            return label!;
        }

        if (TryGetMemberValue(target, "Player") is not null)
        {
            return "player";
        }

        return target.GetType().Name;
    }

    private HarnessActionResult BuildResult(
        HarnessAction action,
        DateTimeOffset startedAt,
        string status,
        bool recoverable,
        string? observed = null,
        string? failureKind = null)
    {
        return new HarnessActionResult(
            action.ActionId,
            status,
            startedAt,
            DateTimeOffset.UtcNow,
            failureKind,
            recoverable,
            observed,
            _defaultArtifactRefs);
    }

    private HarnessActionResult? TryExecuteButtonSemanticAction(HarnessAction action, CompanionState state, DateTimeOffset startedAt)
    {
        var sceneRoots = TryResolveSceneRoots();
        if (sceneRoots.Count == 0)
        {
            return null;
        }

        if (MatchesSentinel(action.TargetLabel, "__start__"))
        {
            if (TryExecuteMainMenuStart(sceneRoots, state, out var observed, out var failureKind))
            {
                return BuildResult(action, startedAt, "ok", false, observed: observed);
            }

            return BuildResult(action, startedAt, "failed", true, observed: observed, failureKind: string.IsNullOrWhiteSpace(failureKind) ? "start-semantic-failed" : failureKind);
        }

        if (MatchesSentinel(action.TargetLabel, "__ironclad__"))
        {
            if (TryExecuteCharacterSelection(sceneRoots, ResolveHarnessAliases(action.TargetLabel, state), out var observed))
            {
                return BuildResult(action, startedAt, "ok", false, observed: observed);
            }

            return BuildResult(action, startedAt, "failed", true, observed: observed, failureKind: "ironclad-selection-failed");
        }

        if (MatchesSentinel(action.TargetLabel, "__standard_run__"))
        {
            if (TryExecuteStandardRunSelection(sceneRoots, out var observed))
            {
                return BuildResult(action, startedAt, "ok", false, observed: observed);
            }

            return BuildResult(action, startedAt, "failed", true, failureKind: "standard-run-selection-failed");
        }

        if (MatchesSentinel(action.TargetLabel, "__confirm__"))
        {
            if (TryExecuteCharacterConfirm(sceneRoots, null, out var confirmObserved))
            {
                return BuildResult(action, startedAt, "ok", false, observed: confirmObserved);
            }

            return BuildResult(
                action,
                startedAt,
                "failed",
                true,
                observed: string.IsNullOrWhiteSpace(confirmObserved) ? null : confirmObserved,
                failureKind: "confirm-semantic-failed");
        }

        if (MatchesSentinel(action.TargetLabel, "__cancel__"))
        {
            if (!InvokeOnMainThread(() =>
                {
                    var roots = TryResolveSceneRoots();
                    var dismissed = TryDismissBlockingOverlays(roots, out var observed);
                    return (Dismissed: dismissed, Observed: observed);
                },
                3_000,
                out (bool Dismissed, string Observed) dismissOutcome))
            {
                return BuildResult(action, startedAt, "failed", true, failureKind: "cancel-dispatch-timeout");
            }

            if (dismissOutcome.Dismissed)
            {
                return BuildResult(action, startedAt, "ok", false, observed: dismissOutcome.Observed);
            }

            return BuildResult(
                action,
                startedAt,
                "failed",
                true,
                observed: string.IsNullOrWhiteSpace(dismissOutcome.Observed) ? null : dismissOutcome.Observed,
                failureKind: "cancel-semantic-failed");
        }

        return null;
    }

    private HarnessActionResult? TryExecuteMapSemanticAction(HarnessAction action, DateTimeOffset startedAt)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(10);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var sceneRoots = TryResolveSceneRoots();
            if (sceneRoots.Count == 0)
            {
                Thread.Sleep(200);
                continue;
            }

            if (TryDismissBlockingOverlays(sceneRoots, out var overlayObserved) && !string.IsNullOrWhiteSpace(overlayObserved))
            {
                AppendTrace("map-overlay-dismissed", action.ActionId, new
                {
                    observed = overlayObserved,
                    visibleTypes = SummarizeVisibleTypes(sceneRoots),
                });
                Thread.Sleep(250);
                continue;
            }

            var mapScreen = ResolveMapScreen(sceneRoots);
            if (mapScreen is null)
            {
                if (HasCombatSceneEvidence(sceneRoots))
                {
                    return BuildResult(action, startedAt, "ok", false, observed: "already-combat");
                }

                Thread.Sleep(200);
                continue;
            }

            var run = FindFirstNodeByType(sceneRoots, "NRun");
            run ??= TryGetMemberValue(TryFindLoadedType("MegaCrit.Sts2.Core.Nodes.NRun") ?? typeof(object), "Instance");
            var mapRoom = run is not null ? TryGetMemberValue(run, "MapRoom") : FindFirstNodeByType(sceneRoots, "NMapRoom");
            var runManager = TryResolveRunManager();
            object? runState = null;
            if (runManager is not null)
            {
                TryInvokeMethod(runManager, "DebugOnlyGetState", out runState);
            }

            var mapScreenIsOpen = TryConvertToBool(TryGetMemberValue(mapScreen, "IsOpen"));
            var mapScreenVisible = IsVisibleNode(mapScreen);
            var travelEnabled = TryConvertToBool(TryGetMemberValue(mapScreen, "IsTravelEnabled"));
            var traveling = TryConvertToBool(TryGetMemberValue(mapScreen, "IsTraveling"));
            var shouldPrepareMapScreen = mapScreenIsOpen != true && !mapScreenVisible;

            if (shouldPrepareMapScreen)
            {
                mapScreenIsOpen = TryConvertToBool(TryGetMemberValue(mapScreen, "IsOpen"));
                mapScreenVisible = IsVisibleNode(mapScreen);
                travelEnabled = TryConvertToBool(TryGetMemberValue(mapScreen, "IsTravelEnabled"));
                traveling = TryConvertToBool(TryGetMemberValue(mapScreen, "IsTraveling"));
            }

            var visitedSignatureBefore = TryGetVisitedMapSignature();
            var pointsRoot = TryGetMemberValue(mapScreen, "_points")
                             ?? TryGetMemberValue(mapScreen, "Points")
                             ?? TryGetNodeAtPath(mapScreen, "TheMap/Points");
            var pointsRootChildCount = ExpandEnumerable(TryInvokeMethod(pointsRoot ?? typeof(object), "GetChildren")).Count();
            var mapPointDictionary = TryGetMemberValue(mapScreen, "_mapPointDictionary") ?? TryGetMemberValue(mapScreen, "MapPointDictionary");
            var mapPointDictionaryCount = ExpandEnumerable(TryGetMemberValue(mapPointDictionary ?? typeof(object), "Values")).Count();

            var wantsCombat = MatchesSentinel(action.TargetLabel, "__first_combat__");
            var allMapCandidates = EnumerateAllMapPointCandidates(mapScreen).ToArray();
            if (allMapCandidates.Length == 0)
            {
                AppendTrace("map-screen-waiting", action.ActionId, new
                {
                    target = action.TargetLabel,
                    visibleTypes = SummarizeVisibleTypes(sceneRoots),
                    mapScreenType = mapScreen.GetType().FullName ?? mapScreen.GetType().Name,
                    mapScreenIsOpen,
                    mapScreenVisible,
                    travelEnabled,
                    traveling,
                    mapRoomPresent = mapRoom is not null,
                    pointsRootPresent = pointsRoot is not null,
                    pointsRootChildCount,
                    mapPointDictionaryCount,
                    visitedSignature = visitedSignatureBefore,
                });
                Thread.Sleep(250);
                continue;
            }

            var roomEnteredObserved = false;
            Action? roomEnteredHandler = null;
            var subscribed = false;
            try
            {
                if (runManager is not null)
                {
                    roomEnteredHandler = () => roomEnteredObserved = true;
                    subscribed = TrySubscribeEvent(runManager, "RoomEntered", roomEnteredHandler, out _);
                }
            }
            catch
            {
                subscribed = false;
            }

            var visibleMapCandidates = allMapCandidates.Where(candidate => candidate.Node is not null && IsVisibleNode(candidate.Node)).ToArray();
            var mapPoint = TryResolveAutoSlayMapPointCandidate(mapScreen, runState, action.TargetLabel, allMapCandidates)
                ?? TryResolveVisibleMapPointCandidate(mapScreen, runState, action.TargetLabel, visibleMapCandidates)
                ?? TryResolveSemanticMapPointCandidate(mapScreen, runState, action.TargetLabel)
                ?? visibleMapCandidates
                    .Where(candidate => !wantsCombat || IsCombatMapPoint(candidate.Point, candidate.Node))
                    .OrderByDescending(candidate => ScoreSemanticMapPointCandidate(candidate.Point!, candidate.State, candidate.Enabled, wantsCombat, candidate.Node))
                    .FirstOrDefault(candidate => ScoreSemanticMapPointCandidate(candidate.Point!, candidate.State, candidate.Enabled, wantsCombat, candidate.Node) > 0)
                ?? allMapCandidates
                    .Where(candidate => !wantsCombat || IsCombatMapPoint(candidate.Point, candidate.Node))
                    .OrderByDescending(candidate => ScoreMapPointCandidate(candidate.Point!, candidate.State, candidate.Enabled))
                    .FirstOrDefault(candidate => ScoreMapPointCandidate(candidate.Point!, candidate.State, candidate.Enabled) > 0);

            if (mapPoint is null)
            {
                var currentCoord = TryGetMapCoord(TryGetMemberValue(runState ?? typeof(object), "CurrentMapCoord"));
                var rowZeroCount = visibleMapCandidates.Count(candidate => TryGetMapCoord(candidate.Point) is { Row: 0 });
                var combatVisibleCount = visibleMapCandidates.Count(candidate => IsCombatMapPoint(candidate.Point, candidate.Node));
                var candidateSummary = SummarizeMapCandidates(allMapCandidates.Length == 0 ? visibleMapCandidates : allMapCandidates);
                AppendTrace("map-point-unavailable", action.ActionId, new
                {
                    target = action.TargetLabel,
                    visibleTypes = SummarizeVisibleTypes(sceneRoots),
                    visitedSignature = visitedSignatureBefore,
                    mapScreenType = mapScreen.GetType().FullName ?? mapScreen.GetType().Name,
                    mapScreenIsOpen,
                    mapScreenVisible,
                    travelEnabled,
                    traveling,
                    mapRoomPresent = mapRoom is not null,
                    pointsRootPresent = pointsRoot is not null,
                    pointsRootChildCount,
                    mapPointDictionaryCount,
                    currentCoord = currentCoord is null ? null : $"{currentCoord.Value.Row},{currentCoord.Value.Col}",
                    mapCandidateCount = allMapCandidates.Length,
                    visibleCandidateCount = visibleMapCandidates.Length,
                    rowZeroVisibleCount = rowZeroCount,
                    combatVisibleCount,
                    candidates = candidateSummary,
                });
                Thread.Sleep(250);
                continue;
            }

            var observedTarget = DescribeMapPoint(mapPoint.Node, mapPoint.Point);
            AppendTrace("map-point-selected", action.ActionId, new
            {
                target = action.TargetLabel,
                observedTarget,
                state = mapPoint.State,
                enabled = mapPoint.Enabled,
                visibleTypes = SummarizeVisibleTypes(sceneRoots),
            });
            var mapCoordObject = TryGetMapCoordObject(mapPoint.Point) ?? TryGetMapCoordObject(mapPoint.Node);
            var activated = (mapPoint.Node is not null && TryInvokeMethodOnMainThread(mapScreen, "OnMapPointSelectedLocally", 2_000, out _, mapPoint.Node))
                            || (mapPoint.Node is not null && TryInvokeMethod(mapPoint.Node, "OnRelease", out _))
                            || (mapPoint.Node is not null && TryActivateNode(mapPoint.Node));
            var visitedSignatureAfter = TryGetVisitedMapSignature();
            if (subscribed && runManager is not null && roomEnteredHandler is not null)
            {
                TryUnsubscribeEvent(runManager, "RoomEntered", roomEnteredHandler);
            }

            if (activated)
            {
                var expectedScene = NormalizeHarnessScene(action.ExpectedStateDelta);
                var transitionDeadline = DateTimeOffset.UtcNow.AddSeconds(10);
                while (DateTimeOffset.UtcNow < transitionDeadline)
                {
                    var refreshedRoots = TryResolveSceneRoots();
                    if (TryDismissBlockingOverlays(refreshedRoots, out var transitionOverlayObserved) && !string.IsNullOrWhiteSpace(transitionOverlayObserved))
                    {
                        AppendTrace("map-transition-overlay-dismissed", action.ActionId, new
                        {
                            observed = transitionOverlayObserved,
                            visibleTypes = SummarizeVisibleTypes(refreshedRoots),
                        });
                        Thread.Sleep(200);
                        refreshedRoots = TryResolveSceneRoots();
                    }

                    if (roomEnteredObserved)
                    {
                        return BuildResult(action, startedAt, "ok", false, observed: $"{observedTarget}|room-entered");
                    }

                    if (TryProbeExpectedSceneTransition(expectedScene, refreshedRoots, out var probeObserved))
                    {
                        return BuildResult(action, startedAt, "ok", false, observed: CombineObserved(observedTarget, probeObserved));
                    }

                    if (string.IsNullOrWhiteSpace(expectedScene)
                        && !string.Equals(visitedSignatureBefore, TryGetVisitedMapSignature(), StringComparison.Ordinal))
                    {
                        return BuildResult(action, startedAt, "ok", false, observed: $"{observedTarget}|map-progressed");
                    }

                    Thread.Sleep(200);
                }

                if (roomEnteredObserved)
                {
                    return BuildResult(action, startedAt, "ok", false, observed: $"{observedTarget}|room-entered");
                }

                if (TryProbeExpectedSceneTransition(expectedScene, TryResolveSceneRoots(), out var finalProbeObserved))
                {
                    return BuildResult(action, startedAt, "ok", false, observed: CombineObserved(observedTarget, finalProbeObserved));
                }

                if (string.IsNullOrWhiteSpace(expectedScene)
                    && !string.Equals(visitedSignatureBefore, visitedSignatureAfter, StringComparison.Ordinal))
                {
                    return BuildResult(action, startedAt, "ok", false, observed: $"{observedTarget}|map-progressed");
                }

                if (!string.Equals(visitedSignatureBefore, visitedSignatureAfter, StringComparison.Ordinal))
                {
                    return BuildResult(
                        action,
                        startedAt,
                        "failed",
                        true,
                        failureKind: "map-progressed-no-transition",
                        observed: $"{observedTarget}|map-progressed-no-transition");
                }

                return BuildResult(
                    action,
                    startedAt,
                    "failed",
                    true,
                    failureKind: "map-point-clicked-no-transition",
                    observed: $"{observedTarget}|map-point-clicked-no-transition");
            }

            AppendTrace("map-point-activation-failed", action.ActionId, new
            {
                target = action.TargetLabel,
                observedTarget,
                visibleTypes = SummarizeVisibleTypes(sceneRoots),
            });
            return BuildResult(action, startedAt, "failed", true, failureKind: "map-point-activation-failed");
        }

        return BuildResult(action, startedAt, "failed", true, failureKind: "map-point-unavailable");
    }

    private static IReadOnlyList<MapPointCandidate> EnumerateAllMapPointCandidates(object mapScreen)
    {
        var results = new List<MapPointCandidate>();
        var seenCoords = new HashSet<string>(StringComparer.Ordinal);

        void AddCandidate(object? node, object? point = null)
        {
            if (node is null && point is null)
            {
                return;
            }

            var resolvedPoint = point ?? TryGetMemberValue(node ?? typeof(object), "Point") ?? node;
            if (resolvedPoint is null)
            {
                return;
            }

            var coord = TryGetMapCoord(resolvedPoint);
            if (coord is null)
            {
                return;
            }

            var key = $"{coord.Value.Row},{coord.Value.Col}";
            if (!seenCoords.Add(key))
            {
                return;
            }

            results.Add(new MapPointCandidate(
                node,
                resolvedPoint,
                TryConvertToDisplayString(TryGetMemberValue(node ?? resolvedPoint, "State")) ?? string.Empty,
                TryConvertToBool(TryGetMemberValue(node ?? resolvedPoint, "IsEnabled"))));
        }

        var pointsRoot = TryGetMemberValue(mapScreen, "_points") ?? TryGetMemberValue(mapScreen, "Points");
        foreach (var node in EnumerateSceneNodes(pointsRoot ?? mapScreen, 4096).Where(IsMapPointNode))
        {
            AddCandidate(node);
        }

        var dictionary = TryGetMemberValue(mapScreen, "_mapPointDictionary") ?? TryGetMemberValue(mapScreen, "MapPointDictionary");
        foreach (var node in ExpandEnumerable(TryGetMemberValue(dictionary ?? typeof(object), "Values")).Where(IsMapPointNode))
        {
            AddCandidate(node);
        }

        foreach (var node in new[]
                 {
                     TryGetMemberValue(mapScreen, "_startingPointNode"),
                     TryGetMemberValue(mapScreen, "_bossPointNode"),
                     TryGetMemberValue(mapScreen, "_secondBossPointNode"),
                 }.Where(node => node is not null && IsMapPointNode(node)))
        {
            AddCandidate(node);
        }

        return results;
    }

    private static MapPointCandidate? TryResolveAutoSlayMapPointCandidate(object mapScreen, object? runState, string? targetLabel, IReadOnlyList<MapPointCandidate>? allCandidates = null)
    {
        var candidates = (allCandidates ?? EnumerateAllMapPointCandidates(mapScreen))
            .Where(candidate => candidate.Point is not null && !IsInvalidMapPointCandidate(candidate.Point, candidate.Node))
            .ToArray();
        if (candidates.Length == 0)
        {
            return null;
        }

        var wantsCombat = MatchesSentinel(targetLabel, "__first_combat__");
        var visitedCoords = ExpandEnumerable(TryGetMemberValue(runState ?? typeof(object), "VisitedMapCoords"))
            .Select(TryGetMapCoord)
            .Where(coord => coord is not null)
            .Cast<(int Row, int Col)>()
            .ToArray();

        IEnumerable<MapPointCandidate> nextCandidates;
        if (visitedCoords.Length == 0)
        {
            var rowZero = candidates.Where(candidate => TryGetMapCoord(candidate.Point) is { Row: 0 }).ToArray();
            nextCandidates = rowZero.Length > 0 ? rowZero : candidates;
        }
        else
        {
            var anchorCoord = visitedCoords[^1];
            var currentNode = candidates.FirstOrDefault(candidate => MapCoordsEqual(TryGetMapCoord(candidate.Point), anchorCoord));
            var childCoords = ExpandEnumerable(TryGetMemberValue(currentNode?.Point ?? typeof(object), "Children"))
                .Select(TryGetMapCoord)
                .Where(coord => coord is not null)
                .Cast<(int Row, int Col)>()
                .ToArray();

            if (childCoords.Length > 0)
            {
                var visibleChildren = candidates
                    .Where(candidate =>
                    {
                        var coord = TryGetMapCoord(candidate.Point);
                        return coord is not null && childCoords.Any(child => MapCoordsEqual(coord, child));
                    })
                    .ToArray();
                nextCandidates = visibleChildren.Length > 0 ? visibleChildren : candidates;
            }
            else
            {
                nextCandidates = candidates;
            }
        }

        var ranked = nextCandidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Coord = TryGetMapCoord(candidate.Point),
                Score =
                    (candidate.Enabled == true ? 1000 : 0)
                    + (candidate.Node is not null && IsVisibleNode(candidate.Node) ? 250 : 0)
                    + ScoreSemanticMapPointCandidate(candidate.Point!, candidate.State, candidate.Enabled, wantsCombat, candidate.Node)
                    + ScoreMapPointCandidate(candidate.Point!, candidate.State, candidate.Enabled),
            })
            .Where(entry => entry.Score > 0)
            .OrderByDescending(entry => entry.Score)
            .ThenBy(entry => entry.Coord?.Row ?? int.MaxValue)
            .ThenBy(entry => entry.Coord?.Col ?? int.MaxValue)
            .ToArray();
        if (ranked.Length == 0)
        {
            return null;
        }

        if (wantsCombat)
        {
            var combatCandidate = ranked.FirstOrDefault(entry => IsCombatMapPoint(entry.Candidate.Point, entry.Candidate.Node));
            if (combatCandidate is not null)
            {
                return combatCandidate.Candidate;
            }

            return null;
        }

        return ranked[0].Candidate;
    }

    private static MapPointCandidate? TryResolveSemanticMapPointCandidate(object mapScreen, object? runState, string? targetLabel)
    {
        var dictionary = TryGetMemberValue(mapScreen, "_mapPointDictionary") ?? TryGetMemberValue(mapScreen, "MapPointDictionary");
        var values = ExpandEnumerable(TryGetMemberValue(dictionary ?? typeof(object), "Values"))
            .Where(IsMapPointNode)
            .Select(node =>
            {
                var point = TryGetMemberValue(node, "Point") ?? node;
                return point is null
                    ? null
                    : new MapPointCandidate(
                        node,
                        point,
                        TryConvertToDisplayString(TryGetMemberValue(node, "State")) ?? string.Empty,
                        TryConvertToBool(TryGetMemberValue(node, "IsEnabled")));
            })
            .Where(candidate => candidate is not null && TryGetMapCoord(candidate.Point) is not null)
            .Cast<MapPointCandidate>()
            .Where(candidate => !IsInvalidMapPointCandidate(candidate.Point, candidate.Node))
            .ToArray();
        var visitedCoords = ExpandEnumerable(TryGetMemberValue(runState ?? typeof(object), "VisitedMapCoords"))
            .Select(TryGetMapCoord)
            .Where(coord => coord is not null)
            .Cast<(int Row, int Col)>()
            .ToArray();
        var currentCoord = TryGetMapCoord(TryGetMemberValue(runState ?? typeof(object), "CurrentMapCoord"));

        if (values.Length == 0)
        {
            var syntheticCandidates = TryGetChildMapCoordObjects(runState, currentCoord, visitedCoords)
                .Select(child => new MapPointCandidate(
                    null,
                    child,
                    visitedCoords.Length == 0 ? "Travelable" : "Reachable",
                    true))
                .ToArray();
            if (syntheticCandidates.Length == 0)
            {
                return null;
            }

            values = syntheticCandidates;
        }

        IEnumerable<MapPointCandidate> candidates = values;
        if (visitedCoords.Length == 0)
        {
            candidates = values.Where(candidate => TryGetMapCoord(candidate.Point) is { Row: 0 });
        }
        else
        {
            var anchorCoord = currentCoord ?? visitedCoords[^1];
            var currentNode = values.FirstOrDefault(candidate => MapCoordsEqual(TryGetMapCoord(candidate.Point), anchorCoord));
            var childCoords = ExpandEnumerable(TryGetMemberValue(currentNode?.Point ?? typeof(object), "Children"))
                .Select(TryGetMapCoord)
                .Where(coord => coord is not null)
                .Cast<(int Row, int Col)>()
                .ToArray();

            if (childCoords.Length > 0)
            {
                candidates = values.Where(candidate =>
                {
                    var coord = TryGetMapCoord(candidate.Point);
                    return coord is not null && childCoords.Any(child => MapCoordsEqual(coord, child));
                });
            }
        }

        var wantsCombat = MatchesSentinel(targetLabel, "__first_combat__");
        var rankedCandidates = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Score = ScoreSemanticMapPointCandidate(candidate.Point, candidate.State, candidate.Enabled, wantsCombat, candidate.Node),
            })
            .Where(entry => entry.Score > 0)
            .OrderByDescending(entry => entry.Score)
            .ThenBy(entry => TryGetMapCoord(entry.Candidate.Point)?.Col ?? int.MaxValue)
            .ToArray();

        if (wantsCombat)
        {
            var combatCandidates = rankedCandidates
                .Where(entry => IsCombatMapPoint(entry.Candidate.Point, entry.Candidate.Node))
                .ToArray();
            if (combatCandidates.Length > 0)
            {
                return combatCandidates[0].Candidate;
            }

            return null;
        }

        if (rankedCandidates.Length > 0)
        {
            return rankedCandidates[0].Candidate;
        }

        return values
            .Select(candidate => new
            {
                Candidate = candidate,
                Score = ScoreSemanticMapPointCandidate(candidate.Point, candidate.State, candidate.Enabled, wantsCombat, candidate.Node),
            })
            .OrderByDescending(entry => entry.Score)
            .FirstOrDefault(entry => entry.Score > 0)
            ?.Candidate;
    }

    private static IEnumerable<MapPointCandidate> EnumerateVisibleMapPointCandidates(object mapScreen)
    {
        return EnumerateSceneNodes(mapScreen, 2048)
            .Where(IsMapPointNode)
            .Where(IsVisibleNode)
            .Select(node =>
            {
                var point = TryGetMemberValue(node, "Point") ?? node;
                return point is null
                    ? null
                    : new MapPointCandidate(
                        node,
                        point,
                        TryConvertToDisplayString(TryGetMemberValue(node, "State")) ?? string.Empty,
                        TryConvertToBool(TryGetMemberValue(node, "IsEnabled")));
            })
            .Where(candidate => candidate is not null && TryGetMapCoord(candidate.Point) is not null)
            .Cast<MapPointCandidate>();
    }

    private static MapPointCandidate? TryResolveVisibleMapPointCandidate(
        object mapScreen,
        object? runState,
        string? targetLabel,
        IReadOnlyList<MapPointCandidate>? visibleCandidates = null)
    {
        var candidates = (visibleCandidates ?? EnumerateVisibleMapPointCandidates(mapScreen).ToArray())
            .Where(candidate => candidate.Point is not null && !IsInvalidMapPointCandidate(candidate.Point, candidate.Node))
            .ToArray();
        if (candidates.Length == 0)
        {
            return null;
        }

        var wantsCombat = MatchesSentinel(targetLabel, "__first_combat__");
        var visitedCoords = ExpandEnumerable(TryGetMemberValue(runState ?? typeof(object), "VisitedMapCoords"))
            .Select(TryGetMapCoord)
            .Where(coord => coord is not null)
            .Cast<(int Row, int Col)>()
            .ToArray();
        var currentCoord = TryGetMapCoord(TryGetMemberValue(runState ?? typeof(object), "CurrentMapCoord"));

        IEnumerable<MapPointCandidate> nextCandidates;
        if (visitedCoords.Length == 0)
        {
            var rowZero = candidates.Where(candidate => TryGetMapCoord(candidate.Point) is { Row: 0 }).ToArray();
            nextCandidates = rowZero.Length > 0 ? rowZero : candidates;
        }
        else
        {
            var anchorCoord = currentCoord ?? visitedCoords[^1];
            var currentNode = candidates.FirstOrDefault(candidate => MapCoordsEqual(TryGetMapCoord(candidate.Point), anchorCoord));
            var childCoords = ExpandEnumerable(TryGetMemberValue(currentNode?.Point ?? typeof(object), "Children"))
                .Select(TryGetMapCoord)
                .Where(coord => coord is not null)
                .Cast<(int Row, int Col)>()
                .ToArray();

            if (childCoords.Length > 0)
            {
                var visibleChildren = candidates
                    .Where(candidate =>
                    {
                        var coord = TryGetMapCoord(candidate.Point);
                        return coord is not null && childCoords.Any(child => MapCoordsEqual(coord, child));
                    })
                    .ToArray();
                nextCandidates = visibleChildren.Length > 0 ? visibleChildren : candidates;
            }
            else
            {
                nextCandidates = candidates;
            }
        }

        var ranked = nextCandidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Coord = TryGetMapCoord(candidate.Point),
                Score = ScoreVisibleMapPointCandidate(candidate, wantsCombat),
            })
            .Where(entry => entry.Score > 0)
            .OrderByDescending(entry => entry.Score)
            .ThenBy(entry => entry.Coord?.Row ?? int.MaxValue)
            .ThenBy(entry => entry.Coord?.Col ?? int.MaxValue)
            .ToArray();
        if (ranked.Length == 0)
        {
            return null;
        }

        if (wantsCombat)
        {
            var combatCandidate = ranked.FirstOrDefault(entry => IsCombatMapPoint(entry.Candidate.Point, entry.Candidate.Node));
            if (combatCandidate is not null)
            {
                return combatCandidate.Candidate;
            }

            return null;
        }

        return ranked[0].Candidate;
    }

    private static int ScoreVisibleMapPointCandidate(MapPointCandidate candidate, bool wantsCombat)
    {
        var score = ScoreSemanticMapPointCandidate(candidate.Point, candidate.State, candidate.Enabled, wantsCombat, candidate.Node);
        var coord = TryGetMapCoord(candidate.Point);
        if (coord is { Row: 0 })
        {
            score += 200;
        }

        if (candidate.Node is not null && IsVisibleNode(candidate.Node))
        {
            score += 100;
        }

        if (candidate.Enabled == true)
        {
            score += 150;
        }
        else if (candidate.Enabled == false)
        {
            score -= 300;
        }

        return score;
    }

    private static bool IsCombatMapPoint(object point, object? node)
    {
        var pointType = ResolveMapPointType(point, node);
        return pointType.Contains("Monster", StringComparison.OrdinalIgnoreCase)
               || pointType.Contains("Combat", StringComparison.OrdinalIgnoreCase)
               || pointType.Contains("Enemy", StringComparison.OrdinalIgnoreCase);
    }

    private HarnessActionResult? TryExecuteRewardSemanticAction(HarnessAction action, DateTimeOffset startedAt)
    {
        var sceneRoots = TryResolveSceneRoots();
        if (sceneRoots.Count == 0)
        {
            return null;
        }

        var cardRewardScreen = FindFirstNodeByType(sceneRoots, "NCardRewardSelectionScreen");
        if (cardRewardScreen is not null)
        {
            var cardRow = TryGetMemberValue(cardRewardScreen, "_cardRow") ?? TryGetMemberValue(cardRewardScreen, "CardRow");
            var rewardRoot = cardRow ?? cardRewardScreen;
            if (rewardRoot is not null)
            {
                var holder = EnumerateSceneNodes(rewardRoot, 256)
                    .Where(node => TypeNameContains(node, "CardHolder"))
                    .OrderByDescending(node => IsVisibleNode(node) ? 10 : 0)
                    .FirstOrDefault(node => !LooksLikePlaceholder(ResolveNodeLabel(node)));

                if (holder is not null
                    && (TryInvokeMethod(holder, "EmitSignal", out _, "pressed", holder)
                        || TryInvokeMethod(holder, "EmitSignal", out _, "Pressed", holder)
                        || TryInvokeMethod(cardRewardScreen, "SelectCard", out _, holder)
                        || TryActivateNode(holder)))
                {
                    return BuildResult(action, startedAt, "ok", false, observed: ResolveNodeLabel(holder) ?? "card-reward-holder");
                }
            }
        }

        var rewardsScreen = FindFirstNodeByType(sceneRoots, "NRewardsScreen");
        if (rewardsScreen is null)
        {
            return null;
        }

        var rewardButtons = ExpandEnumerable(TryGetMemberValue(rewardsScreen, "_rewardButtons") ?? TryGetMemberValue(rewardsScreen, "RewardButtons"))
            .Where(button => !LooksLikePlaceholder(ResolveNodeLabel(button)))
            .ToArray();
        foreach (var rewardButton in rewardButtons)
        {
            if (TryActivateNode(rewardButton) || TryInvokeMethod(rewardButton, "OnRelease", out _))
            {
                return BuildResult(action, startedAt, "ok", false, observed: ResolveNodeLabel(rewardButton) ?? rewardButton.GetType().Name);
            }
        }

        return BuildResult(action, startedAt, "failed", true, failureKind: "reward-unavailable");
    }

    private static bool TryExecuteMainMenuStart(IReadOnlyList<object> sceneRoots, CompanionState state, out string observed, out string failureKind)
    {
        var observedParts = new List<string>();
        if (!TryEnsureMainMenuAvailable(sceneRoots, out var mainMenu, out var mainMenuObserved, out var mainMenuFailureKind))
        {
            observed = string.IsNullOrWhiteSpace(mainMenuObserved)
                ? "main-menu-unavailable"
                : mainMenuObserved;
            failureKind = string.IsNullOrWhiteSpace(mainMenuFailureKind) ? "main-menu-unavailable" : mainMenuFailureKind;
            return false;
        }

        if (!string.IsNullOrWhiteSpace(mainMenuObserved))
        {
            observedParts.Add(mainMenuObserved);
        }

        if (TryOpenSingleplayerSubmenuFromMainMenu(mainMenu, state, out var startObserved))
        {
            observed = CombineObserved(string.Join("|", observedParts), startObserved);
            failureKind = string.Empty;
            return true;
        }

        if (TryOpenCharacterSelectFromMainMenu(mainMenu, state, out var directObserved))
        {
            observed = CombineObserved(string.Join("|", observedParts), directObserved);
            failureKind = string.Empty;
            return true;
        }

        observed = CombineObserved(string.Join("|", observedParts), "main-menu-start-flow-failed");
        failureKind = "main-menu-start-flow-failed";
        return false;
    }

    private static bool TrySkipLogoAnimation(IReadOnlyList<object> sceneRoots, out string observed)
    {
        var game = TryResolveGame(sceneRoots);
        if (game is null)
        {
            observed = string.Empty;
            return false;
        }

        var logoAnimation = TryGetMemberValue(game, "LogoAnimation") ?? FindFirstNodeByType(sceneRoots, "NLogoAnimation");
        if (logoAnimation is null && TryResolveMainMenu(sceneRoots) is not null)
        {
            if (TryInvokeMethodOnMainThread(game, "ReloadMainMenu", 2_000, out _))
            {
                observed = "reload-main-menu";
                Thread.Sleep(500);
                return true;
            }
        }

        if (TryInvokeTaskMethodOnMainThread(game, "LaunchMainMenu", TimeSpan.FromSeconds(8), out var launchObserved, true))
        {
            observed = launchObserved;
            return true;
        }

        if (logoAnimation is null)
        {
            observed = string.Empty;
            return false;
        }

        observed = string.Empty;
        return false;
    }

    private static bool TryOpenCharacterSelectDirectly(object mainMenu, out string observed)
    {
        observed = string.Empty;
        if (TryOpenCharacterSelectViaSingleplayerSubmenu(mainMenu, out var submenuObserved))
        {
            observed = submenuObserved;
            return true;
        }

        var submenuStack = TryGetMemberValue(mainMenu, "SubmenuStack")
                           ?? TryGetMemberValue(mainMenu, "_submenuStack")
                           ?? TryGetMemberValue(mainMenu, "Submenus");
        var characterSelectType = TryFindLoadedType("MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen");
        if (submenuStack is null || characterSelectType is null)
        {
            return false;
        }

        if (!TryInvokeGenericMethod(submenuStack, "GetSubmenuType", new[] { characterSelectType }, out var screen))
        {
            return false;
        }

        if (screen is null)
        {
            return false;
        }

        TryInvokeMethod(screen, "InitializeSingleplayer", out _);
        if (TryInvokeMethod(submenuStack, "Push", out _, screen))
        {
            TryInvokeMethod(screen, "OnSubmenuOpened", out _);
            observed = "push-character-select";
            return true;
        }

        if (TryInvokeMethod(submenuStack, "PushSubmenuType", out _, characterSelectType))
        {
            TryInvokeMethod(screen, "OnSubmenuOpened", out _);
            observed = "push-character-select:type";
            return true;
        }

        return false;
    }

    private static bool TryAdvanceFromMainMenu(out string observed)
    {
        return TryWaitForCharacterSelect(out observed, TimeSpan.FromSeconds(15), trySubmenuActivation: true);
    }

    private static bool TryEnsureMainMenuAvailable(
        IReadOnlyList<object> sceneRoots,
        out object mainMenu,
        out string observed,
        out string failureKind)
    {
        var observedParts = new List<string>();
        var roots = sceneRoots;
        var launchAttempted = false;
        var deadline = DateTimeOffset.UtcNow.AddSeconds(30);

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (TryDismissBlockingOverlays(roots, out var overlayObserved) && !string.IsNullOrWhiteSpace(overlayObserved))
            {
                observedParts.Add(overlayObserved);
                Thread.Sleep(250);
                roots = TryResolveSceneRoots();
                continue;
            }

            var resolvedMainMenu = TryResolveMainMenu(roots);
            if (resolvedMainMenu is not null && IsVisibleNode(resolvedMainMenu))
            {
                mainMenu = resolvedMainMenu;
                observed = string.Join("|", observedParts.Where(part => !string.IsNullOrWhiteSpace(part)).Distinct(StringComparer.OrdinalIgnoreCase));
                failureKind = string.Empty;
                return true;
            }

            var game = TryResolveGame(roots);
            if (game is not null && !launchAttempted)
            {
                if (TrySkipLogoAnimation(roots, out var launchObserved) && !string.IsNullOrWhiteSpace(launchObserved))
                {
                    observedParts.Add(launchObserved);
                    launchAttempted = true;
                }
                else if (TryInvokeTaskMethodOnMainThread(game, "LaunchMainMenu", TimeSpan.FromSeconds(8), out var launchMainMenuObserved, true))
                {
                    observedParts.Add(string.IsNullOrWhiteSpace(launchMainMenuObserved) ? "launch-main-menu" : launchMainMenuObserved);
                    launchAttempted = true;
                }
                else if (TryInvokeMethodOnMainThread(game, "ReloadMainMenu", 2_000, out _))
                {
                    observedParts.Add("reload-main-menu");
                    launchAttempted = true;
                }
            }

            Thread.Sleep(250);
            roots = TryResolveSceneRoots();
        }

        mainMenu = null!;
        observed = string.Join("|", observedParts.Where(part => !string.IsNullOrWhiteSpace(part)).Distinct(StringComparer.OrdinalIgnoreCase));
        failureKind = "main-menu-timeout";
        return false;
    }

    private static bool TryOpenSingleplayerSubmenuFromMainMenu(object mainMenu, CompanionState state, out string observed)
    {
        var observedParts = new List<string>();

        if (TryWaitForSingleplayerSubmenu(mainMenu, out _, out var existingObserved, TimeSpan.FromSeconds(1)))
        {
            observed = existingObserved;
            return true;
        }

        var preferredAliases = ResolveHarnessAliases("__start__", state);
        var singleplayerButton = TryGetMemberValue(mainMenu, "_singleplayerButton")
                                 ?? TryGetMemberValue(mainMenu, "SingleplayerButton")
                                 ?? EnumerateSceneNodes(mainMenu, 256)
                                     .FirstOrDefault(node => string.Equals(ResolveNodeName(node), "SingleplayerButton", StringComparison.OrdinalIgnoreCase));

        if (singleplayerButton is not null
            && IsVisibleNode(singleplayerButton)
            && IsPreferredMainMenuStartCandidate(singleplayerButton)
            && TryActivateNode(singleplayerButton))
        {
            observedParts.Add(ResolveNodeLabel(singleplayerButton) ?? ResolveNodeName(singleplayerButton) ?? "singleplayer-button");
            if (TryWaitForSingleplayerSubmenu(mainMenu, out _, out var submenuObserved, TimeSpan.FromSeconds(6)))
            {
                observed = CombineObserved(string.Join("|", observedParts), submenuObserved);
                return true;
            }

            if (!string.IsNullOrWhiteSpace(submenuObserved))
            {
                observedParts.Add(submenuObserved);
            }
        }

        var menuButtons = ExpandEnumerable(TryGetMemberValue(mainMenu, "MainMenuButtons"))
            .Concat(singleplayerButton is null ? Array.Empty<object>() : new[] { singleplayerButton })
            .Concat(EnumerateSceneNodes(mainMenu, 512).Where(node => TypeNameEndsWith(node, "NMainMenuTextButton")))
            .Where(node => node is not null)
            .Cast<object>()
            .GroupBy(node => RuntimeHelpers.GetHashCode(node))
            .Select(group => group.First())
            .Where(IsVisibleNode)
            .Where(node => !IsForbiddenMainMenuAction(node))
            .OrderByDescending(node => ScoreMainMenuButton(node, state))
            .ThenByDescending(node => ScoreButtonCandidate(CreateNodeCandidate(node), "__start__", preferredAliases))
            .ToArray();

        foreach (var menuButton in menuButtons)
        {
            if (ScoreMainMenuButton(menuButton, state) <= 0
                && ScoreButtonCandidate(CreateNodeCandidate(menuButton), "__start__", preferredAliases) <= 0)
            {
                continue;
            }

            if (!TryActivateNode(menuButton))
            {
                continue;
            }

            observedParts.Add(ResolveNodeLabel(menuButton) ?? ResolveNodeName(menuButton) ?? "main-menu-button");
            if (TryWaitForSingleplayerSubmenu(mainMenu, out _, out var submenuObserved, TimeSpan.FromSeconds(4)))
            {
                observed = CombineObserved(string.Join("|", observedParts), submenuObserved);
                return true;
            }

            if (!string.IsNullOrWhiteSpace(submenuObserved))
            {
                observedParts.Add(submenuObserved);
            }
        }

        if (TryInvokeMethodOnMainThread(mainMenu, "OpenSingleplayerSubmenu", 2_000, out _))
        {
            observedParts.Add("open-singleplayer-submenu");
            if (TryWaitForSingleplayerSubmenu(mainMenu, out _, out var submenuObserved, TimeSpan.FromSeconds(6)))
            {
                observed = CombineObserved(string.Join("|", observedParts), submenuObserved);
                return true;
            }

            if (!string.IsNullOrWhiteSpace(submenuObserved))
            {
                observedParts.Add(submenuObserved);
            }
        }

        observed = string.Join("|", observedParts);
        return false;
    }

    private static bool TryOpenCharacterSelectFromMainMenu(object mainMenu, CompanionState state, out string observed)
    {
        var observedParts = new List<string>();

        var visibleCharacterSelect = FindFirstNodeByType(TryResolveSceneRoots(), "NCharacterSelectScreen");
        if (visibleCharacterSelect is not null && IsVisibleNode(visibleCharacterSelect))
        {
            observed = "character-select";
            return true;
        }

        var preferredAliases = ResolveHarnessAliases("__start__", state);
        var singleplayerButton = TryGetMemberValue(mainMenu, "_singleplayerButton")
                                 ?? TryGetMemberValue(mainMenu, "SingleplayerButton")
                                 ?? EnumerateSceneNodes(mainMenu, 256)
                                     .FirstOrDefault(node => string.Equals(ResolveNodeName(node), "SingleplayerButton", StringComparison.OrdinalIgnoreCase));

        if (singleplayerButton is not null
            && TryInvokeMethodOnMainThread(mainMenu, "SingleplayerButtonPressed", 2_000, out _, singleplayerButton))
        {
            observedParts.Add("singleplayer-button-pressed");
            if (TryWaitForCharacterSelect(out var afterPressedObserved, TimeSpan.FromSeconds(6), trySubmenuActivation: true))
            {
                observed = CombineObserved(string.Join("|", observedParts), afterPressedObserved);
                return true;
            }

            if (!string.IsNullOrWhiteSpace(afterPressedObserved))
            {
                observedParts.Add(afterPressedObserved);
            }
        }

        var submenu = FindFirstNodeByType(EnumerateSceneNodes(mainMenu, 512).Cast<object>().ToArray(), "NSingleplayerSubmenu");
        if (submenu is not null && TryOpenCharacterSelectFromSubmenu(submenu, out var existingSubmenuObserved))
        {
            observedParts.Add(existingSubmenuObserved);
            observed = string.Join("|", observedParts);
            return true;
        }

        if (singleplayerButton is not null
            && IsVisibleNode(singleplayerButton)
            && IsPreferredMainMenuStartCandidate(singleplayerButton))
        {
            if (TryActivateNode(singleplayerButton))
            {
                observedParts.Add(ResolveNodeLabel(singleplayerButton) ?? ResolveNodeName(singleplayerButton) ?? "singleplayer-button");
                if (TryWaitForCharacterSelect(out var afterSingleplayerObserved, TimeSpan.FromSeconds(6), trySubmenuActivation: true))
                {
                    observed = CombineObserved(string.Join("|", observedParts), afterSingleplayerObserved);
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(afterSingleplayerObserved))
                {
                    observedParts.Add(afterSingleplayerObserved);
                }
            }
        }

        var menuButtons = ExpandEnumerable(TryGetMemberValue(mainMenu, "MainMenuButtons"))
            .Concat(singleplayerButton is null ? Array.Empty<object>() : new[] { singleplayerButton })
            .Concat(EnumerateSceneNodes(mainMenu, 512).Where(node => TypeNameEndsWith(node, "NMainMenuTextButton")))
            .Where(node => node is not null)
            .Cast<object>()
            .GroupBy(node => RuntimeHelpers.GetHashCode(node))
            .Select(group => group.First())
            .Where(IsVisibleNode)
            .Where(node => !IsForbiddenMainMenuAction(node))
            .OrderByDescending(node => ScoreMainMenuButton(node, state))
            .ThenByDescending(node => ScoreButtonCandidate(CreateNodeCandidate(node), "__start__", preferredAliases))
            .ToArray();

        foreach (var menuButton in menuButtons)
        {
            if (ScoreMainMenuButton(menuButton, state) <= 0
                && ScoreButtonCandidate(CreateNodeCandidate(menuButton), "__start__", preferredAliases) <= 0)
            {
                continue;
            }

            if (!TryActivateNode(menuButton))
            {
                continue;
            }

            observedParts.Add(ResolveNodeLabel(menuButton) ?? ResolveNodeName(menuButton) ?? "main-menu-button");
            if (TryWaitForCharacterSelect(out var afterButtonObserved, TimeSpan.FromSeconds(4), trySubmenuActivation: true))
            {
                observed = CombineObserved(string.Join("|", observedParts), afterButtonObserved);
                return true;
            }

            if (!string.IsNullOrWhiteSpace(afterButtonObserved))
            {
                observedParts.Add(afterButtonObserved);
            }
        }

        if (TryInvokeMethodOnMainThread(mainMenu, "OpenSingleplayerSubmenu", 2_000, out _))
        {
            observedParts.Add("open-singleplayer-submenu");
            if (TryWaitForCharacterSelect(out var afterSubmenuObserved, TimeSpan.FromSeconds(6), trySubmenuActivation: true))
            {
                observed = CombineObserved(string.Join("|", observedParts), afterSubmenuObserved);
                return true;
            }

            if (!string.IsNullOrWhiteSpace(afterSubmenuObserved))
            {
                observedParts.Add(afterSubmenuObserved);
            }
        }

        observed = string.Join("|", observedParts);
        return false;
    }

    private static bool TryWaitForCharacterSelect(out string observed, TimeSpan timeout, bool trySubmenuActivation)
    {
        var observedParts = new List<string>();
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        while (DateTimeOffset.UtcNow < deadline)
        {
            Thread.Sleep(200);
            var refreshedRoots = TryResolveSceneRoots();

            if (TryDismissBlockingOverlays(refreshedRoots, out var overlayObserved) && !string.IsNullOrWhiteSpace(overlayObserved))
            {
                observedParts.Add(overlayObserved);
                continue;
            }

            var characterSelect = FindFirstNodeByType(refreshedRoots, "NCharacterSelectScreen");
            if (characterSelect is not null && IsVisibleNode(characterSelect))
            {
                observed = CombineObserved(string.Join("|", observedParts), "character-select");
                return true;
            }

            if (trySubmenuActivation)
            {
                var submenu = FindFirstNodeByType(refreshedRoots, "NSingleplayerSubmenu");
                if (submenu is not null && TryClickStandardRunButton(submenu, out var submenuObserved))
                {
                    observedParts.Add(submenuObserved);
                }
            }
        }

        observed = string.Join("|", observedParts.Where(part => !string.IsNullOrWhiteSpace(part)).Distinct(StringComparer.OrdinalIgnoreCase));
        return false;
    }

    private static bool TryDismissBlockingOverlays(IReadOnlyList<object> sceneRoots, out string observed)
    {
        if (TryDismissFeedbackScreen(sceneRoots, out observed))
        {
            return true;
        }

        var timeoutOverlay = FindFirstNodeByType(sceneRoots, "NMultiplayerTimeoutOverlay");
        if (timeoutOverlay is not null && IsVisibleNode(timeoutOverlay))
        {
            if (TryDismissTimeoutOverlay(timeoutOverlay, out observed))
            {
                return true;
            }

            observed = "timeout-overlay-blocking";
            return false;
        }

        observed = string.Empty;
        return false;
    }

    private static bool TryDismissTimeoutOverlay(object timeoutOverlay, out string observed)
    {
        foreach (var methodName in new[] { "Dismiss", "Close", "Hide", "OnDismissPressed", "OnBackPressed" })
        {
            if (TryInvokeMethodOnMainThread(timeoutOverlay, methodName, 2_000, out _, null)
                || TryInvokeMethodOnMainThread(timeoutOverlay, methodName, 2_000, out _))
            {
                observed = $"timeout-overlay:{methodName}";
                Thread.Sleep(200);
                return true;
            }
        }

        var dismissButton = TryGetMemberValue(timeoutOverlay, "_dismisser")
                            ?? TryGetMemberValue(timeoutOverlay, "Dismisser")
                            ?? EnumerateSceneNodes(timeoutOverlay, 128)
                                .FirstOrDefault(node =>
                                    string.Equals(ResolveNodeName(node), "Dismisser", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(ResolveNodeLabel(node), "Dismisser", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(ResolveNodeLabel(node), "\uD655\uC778", StringComparison.OrdinalIgnoreCase));
        if (dismissButton is not null && TryActivateNode(dismissButton))
        {
            observed = $"timeout-overlay:{ResolveNodeLabel(dismissButton) ?? ResolveNodeName(dismissButton) ?? "Dismisser"}";
            Thread.Sleep(200);
            return true;
        }

        var candidates = EnumerateSceneNodes(timeoutOverlay, 256)
            .Select(CreateNodeCandidate)
            .OrderByDescending(ScoreOverlayDismissCandidate)
            .ToArray();
        foreach (var candidate in candidates)
        {
            if (ScoreOverlayDismissCandidate(candidate) <= 0)
            {
                break;
            }

            if (TryActivateCandidate(candidate))
            {
                observed = $"timeout-overlay:{candidate.Label ?? candidate.NodeName ?? candidate.TypeName}";
                Thread.Sleep(200);
                return true;
            }
        }

        observed = string.Empty;
        return false;
    }

    private static bool TryClickStandardRunButton(object submenu, out string observed)
    {
        var subtreeCandidates = EnumerateSceneNodes(submenu, 256)
            .Where(IsVisibleNode)
            .Where(node =>
                string.Equals(ResolveNodeName(node), "StandardButton", StringComparison.OrdinalIgnoreCase)
                || IsLikelyStandardRunLabel(ResolveNodeLabel(node))
                || IsLikelyStandardRunLabel(ResolveNodeName(node)))
            .ToArray();

        var standardButton = TryGetMemberValue(submenu, "_standardButton")
                             ?? TryGetMemberValue(submenu, "StandardButton")
                             ?? subtreeCandidates.FirstOrDefault();
        if (standardButton is not null && TryInvokeMethodOnMainThread(submenu, "OpenCharacterSelect", 2_000, out _, standardButton))
        {
            observed = ResolveNodeLabel(standardButton) ?? ResolveNodeName(standardButton) ?? "standard-run";
            return true;
        }

        if (standardButton is not null && TryActivateNode(standardButton))
        {
            observed = ResolveNodeLabel(standardButton) ?? ResolveNodeName(standardButton) ?? "standard-run";
            return true;
        }

        foreach (var candidate in subtreeCandidates)
        {
            if (TryInvokeMethodOnMainThread(submenu, "OpenCharacterSelect", 2_000, out _, candidate) || TryActivateNode(candidate))
            {
                observed = ResolveNodeLabel(candidate) ?? ResolveNodeName(candidate) ?? "standard-run";
                return true;
            }
        }

        if (TryInvokeMethodOnMainThread(submenu, "OpenCharacterSelect", 2_000, out _, null))
        {
            observed = "standard-run";
            return true;
        }

        observed = string.Empty;
        return false;
    }

    private static bool TryDismissFeedbackScreen(IReadOnlyList<object> sceneRoots, out string observed)
    {
        var feedbackScreen = ResolveFeedbackScreen(sceneRoots);
        if (feedbackScreen is null || !IsBlockingFeedbackScreen(feedbackScreen))
        {
            observed = string.Empty;
            return false;
        }

        foreach (var methodName in new[] { "Close", "Hide", "OnClose", "OnBackPressed" })
        {
            if (TryInvokeMethodOnMainThread(feedbackScreen, methodName, 2_000, out _, null)
                || TryInvokeMethodOnMainThread(feedbackScreen, methodName, 2_000, out _))
            {
                observed = $"feedback-screen:{methodName}";
                Thread.Sleep(200);
                return true;
            }
        }

        var backButton = TryGetMemberValue(feedbackScreen, "_backButton")
                         ?? TryGetMemberValue(feedbackScreen, "BackButton")
                         ?? EnumerateSceneNodes(feedbackScreen, 128)
                             .FirstOrDefault(node => string.Equals(ResolveNodeName(node), "BackButton", StringComparison.OrdinalIgnoreCase));
        if (backButton is not null && TryActivateNode(backButton))
        {
            observed = "feedback-screen:BackButton";
            Thread.Sleep(200);
            return true;
        }

        observed = string.Empty;
        return false;
    }

    private static bool IsBlockingFeedbackScreen(object feedbackScreen)
    {
        if (IsVisibleNode(feedbackScreen))
        {
            return true;
        }

        var visible = TryConvertToBool(TryGetMemberValue(feedbackScreen, "Visible"))
                      ?? TryConvertToBool(TryGetMemberValue(feedbackScreen, "IsVisible"))
                      ?? TryConvertToBool(TryGetMemberValue(feedbackScreen, "IsOpen"));
        if (visible == true)
        {
            return true;
        }

        return EnumerateSceneNodes(feedbackScreen, 96)
            .Any(node => IsVisibleNode(node)
                         && !LooksLikePlaceholder(ResolveNodeLabel(node))
                         && !LooksLikePlaceholder(ResolveNodeName(node)));
    }

    private static object? ResolveFeedbackScreen(IReadOnlyList<object> sceneRoots)
    {
        var game = sceneRoots.FirstOrDefault(root => TypeNameEndsWith(root, "NGame"));
        var feedbackScreen = game is null
            ? null
            : TryGetMemberValue(game, "FeedbackScreen");
        if (feedbackScreen is not null)
        {
            return feedbackScreen;
        }

        return FindFirstNodeByType(sceneRoots, "NSendFeedbackScreen");
    }

    private static bool TryOpenCharacterSelectFromSubmenu(object submenu, out string observed)
    {
        var standardButton = TryGetMemberValue(submenu, "_standardButton")
                             ?? TryGetMemberValue(submenu, "StandardButton");
        if ((standardButton is not null && TryInvokeMethodOnMainThread(submenu, "OpenCharacterSelect", 2_000, out _, standardButton))
            || TryInvokeMethodOnMainThread(submenu, "OpenCharacterSelect", 2_000, out _, null)
            || TryClickStandardRunButton(submenu, out _))
        {
            observed = "open-character-select";
            return true;
        }

        if (TryClickStandardRunButton(submenu, out var submenuObserved))
        {
            observed = submenuObserved;
            return true;
        }

        observed = string.Empty;
        return false;
    }

    private static bool TryOpenCharacterSelectViaSingleplayerSubmenu(object mainMenu, out string observed)
    {
        observed = string.Empty;
        object? submenu = null;

        if (TryInvokeMethodOnMainThread(mainMenu, "OpenSingleplayerSubmenu", 2_000, out var openedSubmenu) && openedSubmenu is not null)
        {
            submenu = openedSubmenu;
        }

        if (submenu is null)
        {
            submenu = FindFirstNodeByType(EnumerateSceneNodes(mainMenu, 512).Cast<object>().ToArray(), "NSingleplayerSubmenu");
        }

        if (submenu is null)
        {
            observed = "open-singleplayer-submenu-missing";
            return false;
        }

        if (!TryOpenCharacterSelectFromSubmenu(submenu, out var submenuObserved))
        {
            return false;
        }

        observed = CombineObserved("open-singleplayer-submenu", submenuObserved);
        return true;
    }

    private static bool TryExecuteStandardRunSelection(IReadOnlyList<object> sceneRoots, out string observed)
    {
        var observedParts = new List<string>();
        var roots = sceneRoots;
        if (TryDismissBlockingOverlays(roots, out var overlayObserved) && !string.IsNullOrWhiteSpace(overlayObserved))
        {
            observedParts.Add(overlayObserved);
            roots = TryResolveSceneRoots();
        }

        var submenu = FindFirstVisibleNodeByType(roots, "NSingleplayerSubmenu");
        if (submenu is null)
        {
            var mainMenu = TryResolveMainMenu(roots);
            if (mainMenu is not null
                && IsVisibleNode(mainMenu)
                && TryOpenSingleplayerSubmenuFromMainMenu(mainMenu, CompanionState.CreateUnknown(), out var submenuObserved))
            {
                observedParts.Add(submenuObserved);
                roots = TryResolveSceneRoots();
                submenu = FindFirstVisibleNodeByType(roots, "NSingleplayerSubmenu");
            }
        }

        if (submenu is null)
        {
            observed = string.Join("|", observedParts);
            return false;
        }

        if (!TryOpenCharacterSelectFromSubmenu(submenu, out var openObserved))
        {
            observed = CombineObserved(string.Join("|", observedParts), openObserved);
            return false;
        }

        observedParts.Add(openObserved);
        if (!TryWaitForVisibleSceneNode("NCharacterSelectScreen", TimeSpan.FromSeconds(6), out _, out var characterSelectObserved))
        {
            observed = CombineObserved(string.Join("|", observedParts), characterSelectObserved);
            return false;
        }

        observed = CombineObserved(string.Join("|", observedParts), characterSelectObserved);
        return true;
    }

    private static bool TryWaitForSingleplayerSubmenu(object mainMenu, out object submenu, out string observed, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var roots = TryResolveSceneRoots();
            var resolvedSubmenu = FindFirstNodeByType(roots, "NSingleplayerSubmenu");
            if (resolvedSubmenu is null)
            {
                resolvedSubmenu = FindFirstNodeByType(EnumerateSceneNodes(mainMenu, 512).Cast<object>().ToArray(), "NSingleplayerSubmenu");
            }

            if (resolvedSubmenu is not null && IsVisibleNode(resolvedSubmenu))
            {
                submenu = resolvedSubmenu;
                observed = "singleplayer-submenu";
                return true;
            }

            Thread.Sleep(200);
        }

        submenu = null!;
        observed = string.Empty;
        return false;
    }

    private static bool TryExecuteCharacterSelection(IReadOnlyList<object> sceneRoots, IReadOnlyList<string> aliases, out string observed)
    {
        var observedParts = new List<string>();
        if (!InvokeOnMainThread(() =>
        {
            var roots = TryResolveSceneRoots();
            if (TryDismissBlockingOverlays(roots, out var overlayObserved) && !string.IsNullOrWhiteSpace(overlayObserved))
            {
                roots = TryResolveSceneRoots();
                if (string.IsNullOrWhiteSpace(overlayObserved))
                {
                    overlayObserved = "overlay-dismissed-before-selection";
                }
            }

            var mainMenu = FindFirstVisibleNodeByType(roots, "NMainMenu");
            var mainMenuRoot = mainMenu ?? FindFirstNodeByType(roots, "NMainMenu");
            var characterSelect = ResolveCharacterSelectScreen(mainMenuRoot, roots);
            if (characterSelect is null)
            {
                var unavailableObserved = string.IsNullOrWhiteSpace(overlayObserved)
                    ? "character-select-unavailable"
                    : CombineObserved(overlayObserved, "character-select-unavailable");
                return (false, unavailableObserved, "character-select-unavailable");
            }

            var characterButtonRoot = TryGetNodeAtPath(characterSelect, "CharSelectButtons/ButtonContainer") ?? characterSelect;
            var characterButtons = EnumerateSceneNodes(characterButtonRoot, 512)
                .Where(node => TypeNameEndsWith(node, "NCharacterSelectButton"))
                .Select(node =>
                {
                    TryInvokeMethod(node, "UnlockIfPossible", out _);
                    return new
                    {
                        Node = node,
                        Score = ScoreCharacterButton(node, aliases),
                        Label = ResolveNodeLabel(node),
                        NodeName = ResolveNodeName(node),
                        CharacterId = ResolveCharacterIdentity(node),
                        Character = TryGetMemberValue(node, "Character") ?? TryGetMemberValue(node, "_character"),
                    };
                })
                .OrderByDescending(candidate => candidate.Score)
                .ToArray();

            var selected = characterButtons.FirstOrDefault(candidate => candidate.Score > 0);
            if (selected is null)
            {
                var candidateSummary = string.Join(", ", characterButtons
                    .Take(6)
                    .Select(candidate =>
                    {
                        var identity = candidate.CharacterId ?? candidate.Label ?? candidate.NodeName ?? candidate.Node.GetType().Name;
                        return $"{identity}:{candidate.Score}";
                    }));
                return (false, $"no-character-match[{candidateSummary}]", "no-character-match");
            }

            var identity = selected.CharacterId ?? selected.Label ?? selected.NodeName ?? "ironclad";
            var selectedViaButton = TryDispatchClickNode(selected.Node, out _)
                                    || TryActivateNode(selected.Node);
            if (!selectedViaButton)
            {
                return (false, CombineObserved(overlayObserved, $"character-select-activate-failed:{identity}"), "character-select-activate-failed");
            }

            return (true, CombineObserved(overlayObserved, CombineObserved(identity, "character-selected")), string.Empty);
        }, 5_000, out (bool Success, string Observed, string FailureKind) selectionStep))
        {
            observed = "character-selection-dispatch-timeout";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(selectionStep.Observed))
        {
            observedParts.Add(selectionStep.Observed);
        }

        if (!selectionStep.Success)
        {
            observed = string.Join("|", observedParts.Where(part => !string.IsNullOrWhiteSpace(part)));
            return false;
        }
        observed = string.Join("|", observedParts.Where(part => !string.IsNullOrWhiteSpace(part)).Distinct(StringComparer.OrdinalIgnoreCase));
        return true;
    }

    private static object? ResolveCharacterSelectScreen(object? mainMenu, IReadOnlyList<object> sceneRoots)
    {
        if (mainMenu is not null)
        {
            var screenFromPath = TryGetNodeAtPath(mainMenu, "Submenus/CharacterSelectScreen");
            if (screenFromPath is not null)
            {
                return screenFromPath;
            }
        }

        return FindFirstVisibleNodeByType(sceneRoots, "NCharacterSelectScreen")
               ?? FindFirstNodeByType(sceneRoots, "NCharacterSelectScreen");
    }

    private static bool TryExecuteCharacterConfirm(IReadOnlyList<object> sceneRoots, object? confirmButton, out string observed)
    {
        var roots = sceneRoots;
        var observedParts = new List<string>();
        if (TryDismissBlockingOverlays(roots, out var overlayObserved) && !string.IsNullOrWhiteSpace(overlayObserved))
        {
            observedParts.Add(overlayObserved);
            roots = TryResolveSceneRoots();
        }

        var characterSelect = FindFirstVisibleNodeByType(roots, "NCharacterSelectScreen");
        if (characterSelect is null)
        {
            observed = string.Join("|", observedParts.Where(part => !string.IsNullOrWhiteSpace(part)).Distinct(StringComparer.OrdinalIgnoreCase));
            return false;
        }

        if ((TryGetMemberValue(characterSelect, "_lobby") ?? TryGetMemberValue(characterSelect, "Lobby")) is null)
        {
            observedParts.Add("character-select-lobby-unavailable");
            observed = string.Join("|", observedParts.Where(part => !string.IsNullOrWhiteSpace(part)).Distinct(StringComparer.OrdinalIgnoreCase));
            return false;
        }

        var embarkButton = confirmButton
                           ?? TryGetMemberValue(characterSelect, "_embarkButton")
                           ?? TryGetMemberValue(characterSelect, "EmbarkButton");
        string? dispatchedObserved = null;
        if (confirmButton is not null && TryActivateNode(confirmButton))
        {
            dispatchedObserved = ResolveNodeLabel(confirmButton) ?? ResolveNodeName(confirmButton) ?? "confirm-button";
        }
        else if (embarkButton is not null && TryActivateNode(embarkButton))
        {
            dispatchedObserved = ResolveNodeLabel(embarkButton) ?? ResolveNodeName(embarkButton) ?? "embark-button";
        }
        else if (embarkButton is not null && TryInvokeMethodOnMainThread(characterSelect, "OnEmbarkPressed", 2_000, out _, embarkButton))
        {
            dispatchedObserved = "embark-pressed-with-button";
        }
        else if (TryInvokeMethodOnMainThread(characterSelect, "OnEmbarkPressed", 2_000, out _, null))
        {
            dispatchedObserved = "embark-pressed";
        }

        if (!string.IsNullOrWhiteSpace(dispatchedObserved))
        {
            observedParts.Add(dispatchedObserved!);
            observed = string.Join("|", observedParts.Where(part => !string.IsNullOrWhiteSpace(part)).Distinct(StringComparer.OrdinalIgnoreCase));
            return true;
        }

        observed = string.Join("|", observedParts.Where(part => !string.IsNullOrWhiteSpace(part)).Distinct(StringComparer.OrdinalIgnoreCase));
        return false;
    }

    private static bool TryResolveLobbySeed(object lobby, out string seed)
    {
        seed = string.Empty;
        try
        {
            var game = TryResolveGame(TryResolveSceneRoots());
            var debugSeed = TryConvertToDisplayString(TryGetMemberValue(game ?? typeof(object), "DebugSeedOverride"));
            if (!string.IsNullOrWhiteSpace(debugSeed))
            {
                seed = debugSeed;
                return true;
            }

            var configuredSeed = TryConvertToDisplayString(TryGetMemberValue(lobby, "Seed"));
            var seedHelperType = TryFindLoadedType("MegaCrit.Sts2.Core.Helpers.SeedHelper");
            if (string.IsNullOrWhiteSpace(configuredSeed))
            {
                if (seedHelperType is not null
                    && TryInvokeMethod(seedHelperType, "GetRandomSeed", out var randomSeed)
                    && !string.IsNullOrWhiteSpace(TryConvertToDisplayString(randomSeed)))
                {
                    seed = TryConvertToDisplayString(randomSeed)!;
                    return true;
                }

                return false;
            }

            if (seedHelperType is not null
                && TryInvokeMethod(seedHelperType, "CanonicalizeSeed", out var canonicalSeed, configuredSeed)
                && !string.IsNullOrWhiteSpace(TryConvertToDisplayString(canonicalSeed)))
            {
                seed = TryConvertToDisplayString(canonicalSeed)!;
                return true;
            }

            seed = configuredSeed!;
            return true;
        }
        catch
        {
            seed = string.Empty;
            return false;
        }
    }

    private static bool TryBuildActsForSeed(object lobby, string seed, out List<object> acts)
    {
        acts = new List<object>();
        try
        {
            var actModelType = TryFindLoadedType("MegaCrit.Sts2.Core.Models.Acts.ActModel");
            var unlockStateType = TryFindLoadedType("MegaCrit.Sts2.Core.Unlocks.UnlockState");
            var netService = TryGetMemberValue(lobby, "NetService");
            if (actModelType is null || unlockStateType is null || netService is null)
            {
                return false;
            }

            var multiplayer = TryConvertToBool(TryInvokeMethod(netService, "IsMultiplayer")) ?? false;
            var players = ExpandEnumerable(TryGetMemberValue(lobby, "Players")).ToArray();
            var unlockStates = players
                .Select(player => TryGetMemberValue(player, "unlockState"))
                .Where(value => value is not null)
                .ToArray();
            if (unlockStates.Length == 0)
            {
                return false;
            }

            object? unlockState = null;
            if (unlockStates.Length == 1)
            {
                if (!TryInvokeMethod(unlockStateType, "FromSerializable", out unlockState, unlockStates[0]))
                {
                    return false;
                }
            }
            else
            {
                var converted = new List<object>();
                foreach (var serializableUnlockState in unlockStates)
                {
                    if (!TryInvokeMethod(unlockStateType, "FromSerializable", out var convertedUnlock, serializableUnlockState) || convertedUnlock is null)
                    {
                        return false;
                    }

                    converted.Add(convertedUnlock);
                }

                unlockState = Activator.CreateInstance(unlockStateType, new object?[] { converted });
            }

            if (unlockState is null)
            {
                return false;
            }

            if (!TryInvokeMethod(actModelType, "GetRandomList", out var actsResult, seed, unlockState, multiplayer))
            {
                return false;
            }

            acts = ExpandEnumerable(actsResult).Where(item => item is not null).Cast<object>().ToList();
            return acts.Count > 0;
        }
        catch
        {
            acts = new List<object>();
            return false;
        }
    }

    private static bool TryGetLobbyModifiers(object lobby, out List<object> modifiers)
    {
        modifiers = ExpandEnumerable(TryGetMemberValue(lobby, "Modifiers")).Where(item => item is not null).Cast<object>().ToList();
        return true;
    }

    private static bool TryWaitForRunStart(TimeSpan timeout, out string observed)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        var observedParts = new List<string>();
        while (DateTimeOffset.UtcNow < deadline)
        {
            Thread.Sleep(250);
            var refreshedRoots = TryResolveSceneRoots();
            if (TryDismissBlockingOverlays(refreshedRoots, out var overlayObserved) && !string.IsNullOrWhiteSpace(overlayObserved))
            {
                observedParts.Add(overlayObserved);
                refreshedRoots = TryResolveSceneRoots();
            }

            if (TryProbeRunStartFromLiveExport(out var liveObserved))
            {
                observed = CombineObserved(string.Join("|", observedParts), liveObserved);
                return true;
            }

            if (TryProbeRunStart(refreshedRoots, out var probeObserved))
            {
                observed = CombineObserved(string.Join("|", observedParts), probeObserved);
                return true;
            }

            var run = FindFirstNodeByType(refreshedRoots, "NRun");
            run ??= TryGetMemberValue(TryFindLoadedType("MegaCrit.Sts2.Core.Nodes.NRun") ?? typeof(object), "Instance");
            if (run is null)
            {
                continue;
            }

            var globalUi = TryGetMemberValue(run, "GlobalUi");
            var mapScreen = globalUi is not null ? TryGetMemberValue(globalUi, "MapScreen") : null;
            var mapRoom = TryGetMemberValue(run, "MapRoom");
            var mapScreenIsOpen = TryConvertToBool(TryGetMemberValue(mapScreen ?? run, "IsOpen")) == true;

            if (mapRoom is not null)
            {
                observed = CombineObserved(string.Join("|", observedParts), "run-started:map-room");
                return true;
            }

            if (mapScreen is not null && (mapScreenIsOpen || IsVisibleNode(mapScreen)))
            {
                observed = CombineObserved(
                    string.Join("|", observedParts),
                    mapScreenIsOpen ? "run-started:map-open" : "run-started:map-visible");
                return true;
            }

            if (FindFirstVisibleNodeByType(refreshedRoots, "NMapScreen") is not null)
            {
                observed = CombineObserved(string.Join("|", observedParts), "run-started:NMapScreen");
                return true;
            }

            if (HasCombatSceneEvidence(refreshedRoots))
            {
                observed = CombineObserved(string.Join("|", observedParts), "run-started:combat-visible");
                return true;
            }
        }

        observed = string.Join("|", observedParts.Where(part => !string.IsNullOrWhiteSpace(part)).Distinct(StringComparer.OrdinalIgnoreCase));
        return false;
    }

    private static bool TryProbeRunStart(IReadOnlyList<object> refreshedRoots, out string observed)
    {
        var run = FindFirstNodeByType(refreshedRoots, "NRun");
        run ??= TryGetMemberValue(TryFindLoadedType("MegaCrit.Sts2.Core.Nodes.NRun") ?? typeof(object), "Instance");
        if (run is null)
        {
            observed = string.Empty;
            return false;
        }

        var globalUi = TryGetMemberValue(run, "GlobalUi");
        var mapScreen = globalUi is not null ? TryGetMemberValue(globalUi, "MapScreen") : null;
        var mapRoom = TryGetMemberValue(run, "MapRoom");
        var mapScreenIsOpen = TryConvertToBool(TryGetMemberValue(mapScreen ?? run, "IsOpen")) == true;

        if (mapRoom is not null)
        {
            observed = "run-started:map-room";
            return true;
        }

        if (mapScreen is not null && (mapScreenIsOpen || IsVisibleNode(mapScreen)))
        {
            observed = mapScreenIsOpen ? "run-started:map-open" : "run-started:map-visible";
            return true;
        }

        if (FindFirstVisibleNodeByType(refreshedRoots, "NMapScreen") is not null)
        {
            observed = "run-started:NMapScreen";
            return true;
        }

        if (HasCombatSceneEvidence(refreshedRoots))
        {
            observed = "run-started:combat-visible";
            return true;
        }

        observed = string.Empty;
        return false;
    }

    private static bool TryProbeRunStartFromLiveExport(out string observed)
    {
        observed = string.Empty;

        try
        {
            var layout = LiveExportPathResolver.Resolve(GamePathOptions.CreateLocalDefault(), LiveExportOptions.Defaults);
            if (!File.Exists(layout.SnapshotPath))
            {
                return false;
            }

            var snapshot = JsonSerializer.Deserialize<LiveExportSnapshot>(
                File.ReadAllText(layout.SnapshotPath),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
            if (snapshot is null)
            {
                return false;
            }

            var screen = snapshot.CurrentScreen?.Trim();
            if (string.Equals(screen, "map", StringComparison.OrdinalIgnoreCase)
                || string.Equals(screen, "reward", StringComparison.OrdinalIgnoreCase)
                || string.Equals(screen, "rewards", StringComparison.OrdinalIgnoreCase)
                || string.Equals(screen, "event", StringComparison.OrdinalIgnoreCase)
                || string.Equals(screen, "shop", StringComparison.OrdinalIgnoreCase)
                || string.Equals(screen, "rest", StringComparison.OrdinalIgnoreCase)
                || string.Equals(screen, "rest-site", StringComparison.OrdinalIgnoreCase))
            {
                observed = $"run-started:live-export:{screen}";
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryProbeExpectedSceneTransition(string? expectedScene, IReadOnlyList<object> sceneRoots, out string observed)
    {
        observed = string.Empty;
        var normalizedExpected = NormalizeHarnessScene(expectedScene);
        if (string.IsNullOrWhiteSpace(normalizedExpected))
        {
            return false;
        }

        var observedScenes = EnumerateObservedScenes(sceneRoots).ToArray();

        if (TryReadLiveExportScene(out var liveScene)
            && SceneMatchesOrProgressesPastHarness(liveScene, normalizedExpected)
            && (!string.Equals(normalizedExpected, "combat", StringComparison.OrdinalIgnoreCase)
                || observedScenes.Any(candidate => SceneMatchesOrProgressesPastHarness(candidate, normalizedExpected))))
        {
            observed = $"live-export:{NormalizeHarnessScene(liveScene)}";
            return true;
        }

        foreach (var candidate in observedScenes)
        {
            if (SceneMatchesOrProgressesPastHarness(candidate, normalizedExpected))
            {
                observed = $"scene:{NormalizeHarnessScene(candidate)}";
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> EnumerateObservedScenes(IReadOnlyList<object> sceneRoots)
    {
        if (HasCombatSceneEvidence(sceneRoots))
        {
            yield return "combat";
        }

        if (FindFirstVisibleNodeByType(sceneRoots, "NCardRewardSelectionScreen") is not null
            || FindFirstVisibleNodeByType(sceneRoots, "NRewardsScreen") is not null)
        {
            yield return "rewards";
        }

        if (FindFirstVisibleNodeByType(sceneRoots, "NEventLayout") is not null
            || FindFirstVisibleNodeByType(sceneRoots, "NEventRoom") is not null)
        {
            yield return "event";
        }

        if (FindFirstVisibleNodeByType(sceneRoots, "NMerchantInventory") is not null
            || FindFirstVisibleNodeByType(sceneRoots, "NMerchantRoom") is not null
            || FindFirstVisibleNodeByType(sceneRoots, "NMerchant") is not null)
        {
            yield return "shop";
        }

        if (FindFirstVisibleNodeByType(sceneRoots, "NRestSiteRoom") is not null
            || FindFirstVisibleNodeByType(sceneRoots, "NRestSite") is not null)
        {
            yield return "rest-site";
        }

        if (FindFirstVisibleNodeByType(sceneRoots, "NMapScreen") is not null)
        {
            yield return "map";
        }
    }

    private static bool TryReadLiveExportScene(out string scene)
    {
        scene = string.Empty;
        try
        {
            var layout = LiveExportPathResolver.Resolve(GamePathOptions.CreateLocalDefault(), LiveExportOptions.Defaults);
            if (!File.Exists(layout.SnapshotPath))
            {
                return false;
            }

            var snapshot = JsonSerializer.Deserialize<LiveExportSnapshot>(
                File.ReadAllText(layout.SnapshotPath),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
            if (snapshot is null || string.IsNullOrWhiteSpace(snapshot.CurrentScreen))
            {
                return false;
            }

            scene = NormalizeHarnessScene(snapshot.CurrentScreen);
            return !string.IsNullOrWhiteSpace(scene);
        }
        catch
        {
            scene = string.Empty;
            return false;
        }
    }

    private static string NormalizeHarnessScene(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "reward" => "rewards",
            "rest" => "rest-site",
            _ => normalized ?? string.Empty,
        };
    }

    private static bool SceneMatchesOrProgressesPastHarness(string? actual, string? expected)
    {
        var normalizedActual = NormalizeHarnessScene(actual);
        var normalizedExpected = NormalizeHarnessScene(expected);
        if (string.Equals(normalizedActual, normalizedExpected, StringComparison.Ordinal))
        {
            return true;
        }

        return normalizedExpected switch
        {
            "combat" => normalizedActual is "rewards" or "event" or "shop" or "rest-site",
            _ => false,
        };
    }

    private static bool HasCombatSceneEvidence(IReadOnlyList<object> sceneRoots)
    {
        if (FindFirstVisibleNodeByType(sceneRoots, "NCombatRoom") is not null
            || FindFirstVisibleNodeByType(sceneRoots, "NCombatScreen") is not null
            || FindFirstVisibleNodeByType(sceneRoots, "NCombatHud") is not null
            || FindFirstVisibleNodeByType(sceneRoots, "NPlayerHand") is not null)
        {
            return true;
        }

        return FindFirstVisibleNodeByType(sceneRoots, "NTargetManager") is not null
               && FindFirstVisibleNodeByType(sceneRoots, "NSelectionReticle") is not null;
    }

    private static bool InvokeOnMainThread<T>(Func<T> action, int timeoutMs, out T result)
    {
        if (IsGameMainThread())
        {
            result = action();
            return true;
        }

        T localResult = default!;
        Exception? exception = null;
        using var completed = new ManualResetEventSlim(false);

        if (!TryDispatchDeferred(() =>
            {
                try
                {
                    localResult = action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    completed.Set();
                }
            }))
        {
            result = action();
            return true;
        }

        if (!completed.Wait(Math.Max(timeoutMs, 1_000)))
        {
            result = default!;
            return false;
        }

        if (exception is not null)
        {
            throw exception;
        }

        result = localResult;
        return true;
    }

    private static bool TryInvokeMethodOnMainThread(object source, string methodName, int timeoutMs, out object? result, params object?[]? args)
    {
        result = null;
        if (IsGameMainThread())
        {
            return TryInvokeMethod(source, methodName, out result, args);
        }

        if (!InvokeOnMainThread(
                () =>
                {
                    var success = TryInvokeMethod(source, methodName, out var localResult, args);
                    return (Success: success, Result: localResult);
                },
                timeoutMs,
                out (bool Success, object? Result) invocation))
        {
            return false;
        }

        result = invocation.Result;
        return invocation.Success;
    }

    private static bool TryInvokeTaskMethodOnMainThread(object source, string methodName, TimeSpan timeout, out string observed, params object?[]? args)
    {
        observed = string.Empty;
        if (!TryInvokeMethodOnMainThread(source, methodName, 2_000, out var result, args))
        {
            return false;
        }

        if (result is Task task)
        {
            try
            {
                if (!task.Wait(timeout))
                {
                    observed = "task-timeout";
                    return false;
                }
            }
            catch (Exception exception)
            {
                observed = exception.GetBaseException().Message;
                return false;
            }

            observed = methodName;
            return true;
        }

        observed = result?.ToString() ?? methodName;
        return true;
    }

    private static bool IsGameMainThread()
    {
        try
        {
            var gameType = TryFindLoadedType("MegaCrit.Sts2.Core.Nodes.NGame");
            return gameType is not null
                   && TryInvokeMethod(gameType, "IsMainThread", out var result)
                   && TryConvertToBool(result) == true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryWaitForVisibleSceneNode(string typeSuffix, TimeSpan timeout, out object? node, out string observed)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        while (DateTimeOffset.UtcNow < deadline)
        {
            Thread.Sleep(200);
            var refreshedRoots = TryResolveSceneRoots();
            var visibleNode = FindFirstVisibleNodeByType(refreshedRoots, typeSuffix);
            if (visibleNode is not null)
            {
                node = visibleNode;
                observed = typeSuffix;
                return true;
            }
        }

        node = null;
        observed = string.Empty;
        return false;
    }

    private static object? ResolveMapScreen(IReadOnlyList<object> sceneRoots)
    {
        var run = FindFirstNodeByType(sceneRoots, "NRun");
        run ??= TryGetMemberValue(TryFindLoadedType("MegaCrit.Sts2.Core.Nodes.NRun") ?? typeof(object), "Instance");
        var globalUi = run is not null ? TryGetMemberValue(run, "GlobalUi") : null;
        var mapScreen = globalUi is not null ? TryGetMemberValue(globalUi, "MapScreen") : null;
        return mapScreen ?? FindFirstNodeByType(sceneRoots, "NMapScreen");
    }

    private static bool IsMapPointNode(object node)
    {
        var typeName = node.GetType().FullName ?? node.GetType().Name;
        return typeName.Contains("MapPoint", StringComparison.OrdinalIgnoreCase)
               && !typeName.Contains("History", StringComparison.OrdinalIgnoreCase)
               && !typeName.Contains("Vote", StringComparison.OrdinalIgnoreCase);
    }

    private static object? TryResolveGame(IReadOnlyList<object> sceneRoots)
    {
        var game = sceneRoots.FirstOrDefault(root => TypeNameEndsWith(root, "NGame"));
        if (game is not null)
        {
            return game;
        }

        var gameType = TryFindLoadedType("MegaCrit.Sts2.Core.Nodes.NGame");
        return gameType is null ? null : TryGetMemberValue(gameType, "Instance");
    }

    private static object? TryResolveMainMenu(IReadOnlyList<object> sceneRoots)
    {
        var game = TryResolveGame(sceneRoots);
        var mainMenu = game is null ? null : TryGetMemberValue(game, "MainMenu");
        if (mainMenu is not null)
        {
            return mainMenu;
        }

        return FindFirstNodeByType(sceneRoots, "NMainMenu");
    }

    private static string CombineObserved(string? left, string? right)
    {
        var values = new[] { left, right }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .SelectMany(value => value!.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return string.Join("|", values);
    }

    private void WriteStatus(string mode, string? lastActionId, string? lastResultStatus, string? message)
    {
        _status = new HarnessIngressStatus(true, mode, lastActionId, DateTimeOffset.UtcNow);
        var payload = new HarnessBridgeStatus(
            true,
            mode,
            lastActionId,
            lastResultStatus,
            DateTimeOffset.UtcNow,
            message);
        File.WriteAllText(_layout.StatusPath, JsonSerializer.Serialize(payload, _jsonOptions));
    }

    private static IReadOnlyList<object> TryResolveSceneRoots()
    {
        var roots = new List<object>();
        try
        {
            foreach (var typeName in new[]
                     {
                         "MegaCrit.Sts2.Core.Nodes.NGame",
                         "MegaCrit.Sts2.Core.Runs.RunManager",
                         "MegaCrit.Sts2.Core.Combat.CombatManager",
                         "MegaCrit.Sts2.Core.Saves.SaveManager",
                     })
            {
                var type = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetType(typeName, throwOnError: false, ignoreCase: false))
                    .FirstOrDefault(candidate => candidate is not null);
                if (type is null)
                {
                    continue;
                }

                AddIfUseful(roots, type);
                foreach (var singletonName in SingletonPropertyNames)
                {
                    AddIfUseful(roots, TryGetMemberValue(type, singletonName));
                }
            }

            var engineType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("Godot.Engine", throwOnError: false, ignoreCase: false))
                .FirstOrDefault(type => type is not null);
            if (engineType is null)
            {
                return Array.Empty<object>();
            }

            var mainLoop = TryInvokeMethod(engineType, "GetMainLoop")
                           ?? TryGetMemberValue(engineType, "MainLoop");
            if (mainLoop is null)
            {
                return Array.Empty<object>();
            }

            AddIfUseful(roots, mainLoop);
            foreach (var candidate in new[]
                     {
                         TryGetMemberValue(mainLoop, "CurrentScene"),
                         TryInvokeMethod(mainLoop, "GetCurrentScene"),
                         TryGetMemberValue(mainLoop, "Root"),
                     })
            {
                if (candidate is null)
                {
                    continue;
                }

                AddIfUseful(roots, candidate);
            }

            for (var index = 0; index < roots.Count; index += 1)
            {
                foreach (var nestedRootName in NestedRootNames)
                {
                    AddIfUseful(roots, TryGetMemberValue(roots[index], nestedRootName));
                }
            }

            var traversalRoot = roots.FirstOrDefault(candidate => candidate is not Type);
            foreach (var node in EnumerateSceneNodes(traversalRoot!, 4096))
            {
                if (!IsInterestingSceneNode(node))
                {
                    continue;
                }

                AddIfUseful(roots, node);
                foreach (var nestedRootName in NestedRootNames.Concat(new[] { "Screen", "Room", "Layout", "Model", "Controller" }))
                {
                    AddIfUseful(roots, TryGetMemberValue(node, nestedRootName));
                }
            }
        }
        catch
        {
            return Array.Empty<object>();
        }

        return roots;
    }

    private static bool IsInterestingSceneNode(object node)
    {
        var typeName = node.GetType().FullName ?? node.GetType().Name;
        return IsVisibleNode(node)
               || SceneNodeKeywords.Any(keyword => typeName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsVisibleNode(object node)
    {
        var isInsideTree = TryConvertToBool(TryGetMemberValue(node, "IsInsideTree"))
                           ?? TryConvertToBool(TryInvokeMethod(node, "IsInsideTree"))
                           ?? true;
        var visibleInTree = TryConvertToBool(TryGetMemberValue(node, "VisibleInTree"))
                            ?? TryConvertToBool(TryInvokeMethod(node, "IsVisibleInTree"));
        var visible = TryConvertToBool(TryGetMemberValue(node, "Visible"))
                      ?? TryConvertToBool(TryGetMemberValue(node, "IsVisible"));

        return isInsideTree && (visibleInTree ?? visible ?? false);
    }

    private static object? FindFirstNodeByType(IEnumerable<object> sceneRoots, string typeSuffix)
    {
        return sceneRoots
            .SelectMany(root => EnumerateSceneNodes(root, 4096))
            .FirstOrDefault(node => TypeNameEndsWith(node, typeSuffix));
    }

    private static object? FindFirstVisibleNodeByType(IEnumerable<object> sceneRoots, string typeSuffix)
    {
        return sceneRoots
            .SelectMany(root => EnumerateSceneNodes(root, 4096))
            .FirstOrDefault(node => TypeNameEndsWith(node, typeSuffix) && IsVisibleNode(node));
    }

    private static IEnumerable<object> EnumerateSceneNodes(object root, int maxNodes)
    {
        var queue = new Queue<object>();
        var seen = new HashSet<int>();
        queue.Enqueue(root);
        seen.Add(RuntimeHelpers.GetHashCode(root));

        while (queue.Count > 0 && seen.Count <= maxNodes)
        {
            var current = queue.Dequeue();
            yield return current;

            foreach (var child in TryEnumerateChildren(current))
            {
                var key = RuntimeHelpers.GetHashCode(child);
                if (!seen.Add(key))
                {
                    continue;
                }

                queue.Enqueue(child);
                if (seen.Count >= maxNodes)
                {
                    break;
                }
            }
        }
    }

    private static IEnumerable<object> TryEnumerateChildren(object root)
    {
        var seen = new HashSet<int>();
        foreach (var candidate in new[]
                 {
                     TryInvokeMethod(root, "GetChildren", true),
                     TryInvokeMethod(root, "GetChildren"),
                 })
        {
            foreach (var item in ExpandEnumerable(candidate))
            {
                if (seen.Add(RuntimeHelpers.GetHashCode(item)))
                {
                    yield return item;
                }
            }
        }

        var count = TryConvertToInt(TryInvokeMethod(root, "GetChildCount"));
        if (count is null or <= 0)
        {
            yield break;
        }

        for (var index = 0; index < count.Value; index += 1)
        {
            var child = TryInvokeMethod(root, "GetChild", index);
            if (child is not null && seen.Add(RuntimeHelpers.GetHashCode(child)))
            {
                yield return child;
            }
        }
    }

    private static IEnumerable<object> ExpandEnumerable(object? candidate)
    {
        if (candidate is null || candidate is string)
        {
            yield break;
        }

        if (candidate is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is not null)
                {
                    yield return item;
                }
            }
        }
    }

    private static bool TryActivateNode(object node)
    {
        if (IsGameMainThread())
        {
            return TryActivateNodeCore(node);
        }

        return InvokeOnMainThread(() => TryActivateNodeCore(node), 2_000, out var activated)
            ? activated
            : TryActivateNodeCore(node);
    }

    private static bool TryActivateNodeCore(object node)
    {
        foreach (var attempt in new Func<bool>[]
                 {
                     () => TryInvokeMethod(node, "ForceClick", out _),
                     () => TryInvokeMethod(node, "EmitSignal", out _, "pressed"),
                     () => TryInvokeMethod(node, "EmitSignal", out _, "pressed", node),
                     () => TryInvokeMethod(node, "EmitSignal", out _, "released"),
                     () => TryInvokeMethod(node, "EmitSignal", out _, "Released"),
                     () => TryInvokeMethod(node, "Press", out _),
                     () => TryInvokeMethod(node, "OnRelease", out _),
                     () => TryInvokeMethod(node, "_Pressed", out _),
                     () => TryInvokeMethod(node, "OnPressed", out _),
                     () => TryInvokeMethod(node, "Activate", out _),
                     () => TryInvokeMethod(node, "Select", out _),
                     () => TryInvokeMethod(node, "Click", out _),
                     () => TryInvokeMethod(node, "Pressed", out _),
                 })
        {
            try
            {
                if (attempt())
                {
                    return true;
                }
            }
            catch
            {
                // Ignore node activation failures and keep trying.
            }
        }

        return false;
    }

    private static bool TryActivateCandidate(NodeCandidate candidate)
    {
        return TryActivateNode(candidate.ActivationNode ?? candidate.Node);
    }

    private static string? ResolveNodeLabel(object node)
    {
        var directLabel = ResolveDirectNodeLabel(node);
        if (!string.IsNullOrWhiteSpace(directLabel))
        {
            return directLabel;
        }

        return ResolveDescendantNodeLabel(node);
    }

    private static string? ResolveDirectNodeLabel(object node)
    {
        foreach (var memberName in new[] { "Text", "BbcodeText", "DisplayedText", "Label", "Title", "Description" })
        {
            var value = TryConvertToDisplayString(TryGetMemberValue(node, memberName));
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        foreach (var methodName in new[] { "GetRawText", "GetText", "GetParsedText", "GetFormattedText" })
        {
            var value = TryConvertToDisplayString(TryInvokeMethod(node, methodName));
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string? ResolveDescendantNodeLabel(object node)
    {
        var queue = new Queue<(object Node, int Depth)>();
        var seen = new HashSet<int>();
        foreach (var child in TryEnumerateChildren(node))
        {
            queue.Enqueue((child, 1));
            seen.Add(RuntimeHelpers.GetHashCode(child));
        }

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            var label = ResolveDirectNodeLabel(current);
            if (!string.IsNullOrWhiteSpace(label) && !LooksLikePlaceholder(label))
            {
                return label;
            }

            if (depth >= 3)
            {
                continue;
            }

            foreach (var child in TryEnumerateChildren(current))
            {
                var key = RuntimeHelpers.GetHashCode(child);
                if (seen.Add(key))
                {
                    queue.Enqueue((child, depth + 1));
                }
            }
        }

        return null;
    }

    private static string? ResolveNodeName(object node)
    {
        return TryConvertToDisplayString(TryGetMemberValue(node, "Name"));
    }

    private static NodeCandidate CreateNodeCandidate(object node)
    {
        var label = ResolveNodeLabel(node);
        var activationNode = ResolveActionableNode(node);
        var typeNode = activationNode ?? node;
        var nodeName = ResolveNodeName(typeNode) ?? ResolveNodeName(node);
        var typeName = typeNode.GetType().FullName ?? typeNode.GetType().Name;
        return new NodeCandidate(node, activationNode, label, nodeName, typeName);
    }

    private static object? ResolveActionableNode(object node)
    {
        foreach (var candidate in EnumerateSelfAndAncestors(node, 6))
        {
            if (LooksActionableNode(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static IEnumerable<object> EnumerateSelfAndAncestors(object node, int maxDepth)
    {
        object? current = node;
        for (var depth = 0; current is not null && depth <= maxDepth; depth += 1)
        {
            yield return current;
            current = TryGetParentNode(current);
        }
    }

    private static object? TryGetParentNode(object node)
    {
        return TryGetMemberValue(node, "Parent")
               ?? TryGetMemberValue(node, "parent")
               ?? TryInvokeMethod(node, "GetParent");
    }

    private static bool LooksActionableNode(object node)
    {
        var typeName = node.GetType().FullName ?? node.GetType().Name;
        return typeName.Contains("Button", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Option", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Clickable", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Control", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("MapPoint", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("Reward", StringComparison.OrdinalIgnoreCase)
               || typeName.Contains("CardHolder", StringComparison.OrdinalIgnoreCase);
    }

    private static int ScoreButtonCandidate(NodeCandidate candidate, string? targetLabel, IReadOnlyList<string> aliases)
    {
        var score = 0;
        var label = candidate.Label ?? string.Empty;
        var nodeName = candidate.NodeName ?? string.Empty;
        var typeName = candidate.TypeName;
        var aliasMatched = false;
        if (typeName.Contains("Button", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Option", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Reward", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Map", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Card", StringComparison.OrdinalIgnoreCase))
        {
            score += 25;
        }

        if (LooksLikePlaceholder(label))
        {
            score -= 100;
        }

        foreach (var alias in aliases)
        {
            if (string.Equals(label, alias, StringComparison.OrdinalIgnoreCase)
                || string.Equals(nodeName, alias, StringComparison.OrdinalIgnoreCase)
                || string.Equals(typeName, alias, StringComparison.OrdinalIgnoreCase))
            {
                aliasMatched = true;
                score += 200;
            }
            else if (label.Contains(alias, StringComparison.OrdinalIgnoreCase)
                     || nodeName.Contains(alias, StringComparison.OrdinalIgnoreCase)
                     || typeName.Contains(alias, StringComparison.OrdinalIgnoreCase))
            {
                aliasMatched = true;
                score += 100;
            }
        }

        if (aliases.Count > 0 && !aliasMatched)
        {
            return LooksLikePlaceholder(label) ? -200 : -25;
        }

        if (aliases.Count == 0 && !string.IsNullOrWhiteSpace(targetLabel))
        {
            if (string.Equals(label, targetLabel, StringComparison.OrdinalIgnoreCase))
            {
                score += 200;
            }
            else if (string.Equals(nodeName, targetLabel, StringComparison.OrdinalIgnoreCase))
            {
                score += 180;
            }
            else if (label.Contains(targetLabel, StringComparison.OrdinalIgnoreCase))
            {
                score += 100;
            }
            else if (nodeName.Contains(targetLabel, StringComparison.OrdinalIgnoreCase))
            {
                score += 80;
            }
        }
        else if (aliases.Count == 0 && !string.IsNullOrWhiteSpace(label))
        {
            score += 25;
        }

        return score;
    }

    private static int ScoreSemanticCandidate(
        NodeCandidate candidate,
        string? targetLabel,
        IReadOnlyList<string> aliases,
        IReadOnlyList<string> preferredKeywords,
        Func<NodeCandidate, bool> domainPredicate)
    {
        if (LooksLikePlaceholder(candidate.Label))
        {
            return -100;
        }

        var score = domainPredicate(candidate) ? 80 : 0;
        foreach (var keyword in preferredKeywords)
        {
            if ((candidate.Label?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                || (candidate.NodeName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                || candidate.TypeName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                score += 40;
            }
        }

        foreach (var alias in aliases)
        {
            if (string.Equals(candidate.Label, alias, StringComparison.OrdinalIgnoreCase))
            {
                score += 120;
            }
            else if (string.Equals(candidate.NodeName, alias, StringComparison.OrdinalIgnoreCase))
            {
                score += 110;
            }
            else if (!string.IsNullOrWhiteSpace(alias)
                     && ((candidate.Label?.Contains(alias, StringComparison.OrdinalIgnoreCase) ?? false)
                         || (candidate.NodeName?.Contains(alias, StringComparison.OrdinalIgnoreCase) ?? false)))
            {
                score += 80;
            }
        }

        if (aliases.Count == 0 && !string.IsNullOrWhiteSpace(targetLabel))
        {
            if (string.Equals(candidate.Label, targetLabel, StringComparison.OrdinalIgnoreCase))
            {
                score += 120;
            }
            else if (string.Equals(candidate.NodeName, targetLabel, StringComparison.OrdinalIgnoreCase))
            {
                score += 100;
            }
        }

        if (!string.IsNullOrWhiteSpace(candidate.Label))
        {
            score += 20;
        }
        else if (!string.IsNullOrWhiteSpace(candidate.NodeName))
        {
            score += 10;
        }

        return score;
    }

    private static int ScoreOverlayDismissCandidate(NodeCandidate candidate)
    {
        var label = candidate.Label ?? string.Empty;
        var nodeName = candidate.NodeName ?? string.Empty;
        var typeName = candidate.TypeName;
        var score = 0;

        if (typeName.Contains("Button", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Back", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Dismiss", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Close", StringComparison.OrdinalIgnoreCase))
        {
            score += 50;
        }

        foreach (var token in new[] { "dismiss", "dismisser", "close", "back", "ok", "confirm", "cancel" })
        {
            if (label.Contains(token, StringComparison.OrdinalIgnoreCase)
                || nodeName.Contains(token, StringComparison.OrdinalIgnoreCase)
                || typeName.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                score += 120;
            }
        }

        if (LooksLikePlaceholder(label) && !nodeName.Contains("Dismiss", StringComparison.OrdinalIgnoreCase))
        {
            score -= 50;
        }

        return score;
    }

    private static IReadOnlyList<string> ResolveHarnessAliases(string? targetLabel, CompanionState? state = null)
    {
        var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return state?.Choices.List
                .Select(choice => choice.Label)
                .Where(label => !string.IsNullOrWhiteSpace(label))
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
                ?? Array.Empty<string>();
        }

        if (SentinelAliases.TryGetValue(targetLabel, out var sentinelAliases))
        {
            foreach (var alias in sentinelAliases)
            {
                if (!string.IsNullOrWhiteSpace(alias))
                {
                    aliases.Add(alias);
                }
            }
        }
        else
        {
            aliases.Add(targetLabel);
        }

        if (state is not null)
        {
            foreach (var choice in state.Choices.List)
            {
                if (string.IsNullOrWhiteSpace(choice.Label))
                {
                    continue;
                }

                if (ShouldAddChoiceAlias(targetLabel, aliases, choice.Label))
                {
                    aliases.Add(choice.Label);
                }
            }
        }

        return aliases.ToArray();
    }

    private static bool ShouldAddChoiceAlias(string? targetLabel, IReadOnlyCollection<string> aliases, string choiceLabel)
    {
        if (aliases.Count == 0)
        {
            return true;
        }

        if (aliases.Any(alias => !string.IsNullOrWhiteSpace(alias) && choiceLabel.Contains(alias, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (MatchesSentinel(targetLabel, "__ironclad__"))
        {
            return choiceLabel.Contains("ironclad", StringComparison.OrdinalIgnoreCase)
                   || choiceLabel.Contains("아이언", StringComparison.OrdinalIgnoreCase);
        }

        if (MatchesSentinel(targetLabel, "__start__"))
        {
            return IsLikelyMainMenuStartLabel(choiceLabel);
        }

        if (MatchesSentinel(targetLabel, "__standard_run__"))
        {
            return IsLikelyStandardRunLabel(choiceLabel);
        }

        return false;
    }

    private static bool IsLikelyStandardRunLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        return label.Contains("standard", StringComparison.OrdinalIgnoreCase)
               || label.Contains("normal", StringComparison.OrdinalIgnoreCase)
               || label.Contains("\uC77C\uBC18", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLikelyMainMenuStartLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        return label.Contains("singleplayer", StringComparison.OrdinalIgnoreCase)
               || label.Contains("play", StringComparison.OrdinalIgnoreCase)
               || label.Contains("new run", StringComparison.OrdinalIgnoreCase)
               || label.Contains("새 게임", StringComparison.OrdinalIgnoreCase)
               || label.Contains("시작", StringComparison.OrdinalIgnoreCase)
               || label.Contains("출발", StringComparison.OrdinalIgnoreCase)
               || label.Contains("싱글플레이", StringComparison.OrdinalIgnoreCase);
    }

    private static int ScoreMainMenuButton(object node, CompanionState state)
    {
        var score = 0;
        var label = ResolveNodeLabel(node) ?? string.Empty;
        var name = ResolveNodeName(node) ?? string.Empty;
        var typeName = node.GetType().FullName ?? node.GetType().Name;

        if (LooksLikePlaceholder(label))
        {
            score -= 200;
        }

        if (!string.IsNullOrWhiteSpace(label))
        {
            score += 50;
        }

        if (name.Contains("singleplayer", StringComparison.OrdinalIgnoreCase))
        {
            score += 500;
        }

        if (typeName.Contains("Continue", StringComparison.OrdinalIgnoreCase)
            || name.Contains("continue", StringComparison.OrdinalIgnoreCase)
            || name.Contains("profile", StringComparison.OrdinalIgnoreCase)
            || name.Contains("patch", StringComparison.OrdinalIgnoreCase))
        {
            score -= 250;
        }

        if (state.Choices.List.Any(choice => string.Equals(choice.Label, label, StringComparison.OrdinalIgnoreCase)))
        {
            score += 75;
        }

        if (IsLikelyMainMenuStartLabel(label))
        {
            score += 300;
        }

        if (IsVisibleNode(node))
        {
            score += 25;
        }

        return score;
    }

    private static bool IsPreferredMainMenuStartCandidate(object node)
    {
        var label = ResolveNodeLabel(node) ?? string.Empty;
        var name = ResolveNodeName(node) ?? string.Empty;
        return IsLikelyMainMenuStartLabel(label)
               || ContainsAny(name, "singleplayer", "singleplayerbutton", "start", "newgame");
    }

    private static bool IsForbiddenMainMenuAction(object node)
    {
        var label = ResolveNodeLabel(node) ?? string.Empty;
        var name = ResolveNodeName(node) ?? string.Empty;
        var typeName = node.GetType().FullName ?? node.GetType().Name;
        return ContainsAny(
                   label,
                   "abandon",
                   "forfeit",
                   "quit run",
                   "continue",
                   "multiplayer",
                   "settings",
                   "patch",
                   "profile",
                   "codex",
                   "\uC804\uD22C \uD3EC\uAE30",
                   "\uACC4\uC18D",
                   "\uBA40\uD2F0\uD50C\uB808\uC774",
                   "\uC124\uC815",
                   "\uBC31\uACFC\uC0AC\uC804",
                   "\uC885\uB8CC",
                   "\uD504\uB85C\uD544")
               || ContainsAny(name, "continue", "profile", "patch", "multiplayer", "abandon", "quit")
               || ContainsAny(typeName, "Continue", "Profile", "Patch");
    }

    private static bool ContainsAny(string value, params string[] needles)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (var needle in needles)
        {
            if (!string.IsNullOrWhiteSpace(needle)
                && value.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyList<string> ResolveAliases(string? targetLabel, CompanionState? state = null)
    {
        var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return state?.Choices.List
                .Select(choice => choice.Label)
                .Where(label => !string.IsNullOrWhiteSpace(label))
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
                ?? Array.Empty<string>();
        }

        if (SentinelAliases.TryGetValue(targetLabel, out var sentinelAliases))
        {
            foreach (var alias in sentinelAliases)
            {
                if (!string.IsNullOrWhiteSpace(alias))
                {
                    aliases.Add(alias);
                }
            }
        }
        else
        {
            aliases.Add(targetLabel);
        }

        if (state is not null)
        {
            foreach (var choice in state.Choices.List)
            {
                if (string.IsNullOrWhiteSpace(choice.Label))
                {
                    continue;
                }

                if (aliases.Count == 0
                    || aliases.Any(alias => choice.Label.Contains(alias, StringComparison.OrdinalIgnoreCase))
                    || choice.Label.Contains("싱글", StringComparison.OrdinalIgnoreCase)
                    || choice.Label.Contains("아이언", StringComparison.OrdinalIgnoreCase))
                {
                    aliases.Add(choice.Label);
                }
            }
        }

        return aliases.ToArray();
    }

    private static bool LooksLikePlaceholder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return value.StartsWith("@Control@", StringComparison.Ordinal)
               || value.StartsWith("@Node", StringComparison.Ordinal)
               || value.Contains("BackButton", StringComparison.OrdinalIgnoreCase)
               || value.Contains("Preview", StringComparison.OrdinalIgnoreCase)
               || value.Contains("Holder", StringComparison.OrdinalIgnoreCase)
               || value.Contains("Hitbox", StringComparison.OrdinalIgnoreCase)
               || value.Contains("PlayQueue", StringComparison.OrdinalIgnoreCase)
               || value.Contains("CardPreviewContainer", StringComparison.OrdinalIgnoreCase)
               || value.Contains("MerchantCardHolder", StringComparison.OrdinalIgnoreCase);
    }

    private static string SummarizeCandidates(IEnumerable<NodeCandidate> candidates)
    {
        return string.Join(
            " | ",
            candidates
                .Take(8)
                .Select(candidate => $"{candidate.Label ?? candidate.NodeName ?? "<null>"}<{candidate.TypeName}>"));
    }

    private static void AddIfUseful(ICollection<object> target, object? candidate)
    {
        if (candidate is null or string)
        {
            return;
        }

        if (candidate is IEnumerable and not Type)
        {
            return;
        }

        var key = RuntimeHelpers.GetHashCode(candidate);
        if (target.Any(existing => RuntimeHelpers.GetHashCode(existing) == key))
        {
            return;
        }

        target.Add(candidate);
    }

    private static bool MatchesSentinel(string? value, string sentinel)
    {
        return string.Equals(value?.Trim(), sentinel, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TypeNameEndsWith(object node, string typeSuffix)
    {
        var typeName = node.GetType().FullName ?? node.GetType().Name;
        return typeName.EndsWith(typeSuffix, StringComparison.OrdinalIgnoreCase)
               || typeName.Contains($".{typeSuffix}", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TypeNameContains(object node, string fragment)
    {
        var typeName = node.GetType().FullName ?? node.GetType().Name;
        return typeName.Contains(fragment, StringComparison.OrdinalIgnoreCase);
    }

    private static int ScoreCharacterButton(object node, IReadOnlyList<string> aliases)
    {
        var score = 0;
        var label = ResolveNodeLabel(node) ?? string.Empty;
        var nodeName = ResolveNodeName(node) ?? string.Empty;
        var characterId = ResolveCharacterIdentity(node) ?? string.Empty;
        var typeName = node.GetType().FullName ?? node.GetType().Name;
        if (!string.IsNullOrWhiteSpace(characterId))
        {
            score += 40;
        }

        if (TryConvertToBool(TryGetMemberValue(node, "IsLocked")) == true)
        {
            score -= 200;
        }

        foreach (var alias in aliases)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                continue;
            }

            if (label.Contains(alias, StringComparison.OrdinalIgnoreCase))
            {
                score += 100;
            }

            if (nodeName.Contains(alias, StringComparison.OrdinalIgnoreCase))
            {
                score += 180;
            }

            if (characterId.Contains(alias, StringComparison.OrdinalIgnoreCase))
            {
                score += 220;
            }

            if (typeName.Contains(alias, StringComparison.OrdinalIgnoreCase))
            {
                score += 80;
            }

            var character = TryGetMemberValue(node, "_character") ?? TryGetMemberValue(node, "Character");
            if (character is not null)
            {
                var id = TryConvertToDisplayString(TryGetMemberValue(character, "Id"))
                         ?? TryConvertToDisplayString(TryGetMemberValue(character, "Entry"))
                         ?? TryConvertToDisplayString(TryGetMemberValue(TryGetMemberValue(character, "Id") ?? character, "Entry"));
                var name = TryConvertToDisplayString(TryGetMemberValue(character, "Name"));
                if ((id?.Contains(alias, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (name?.Contains(alias, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (character.ToString()?.Contains(alias, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    score += 200;
                }
            }
        }

        if (characterId.Contains("ironclad", StringComparison.OrdinalIgnoreCase))
        {
            score += 120;
        }

        return score;
    }

    private static string? ResolveCharacterIdentity(object node)
    {
        var character = TryGetMemberValue(node, "_character") ?? TryGetMemberValue(node, "Character");
        if (character is null)
        {
            return null;
        }

        var values = new[]
        {
            TryConvertToDisplayString(TryGetMemberValue(character, "Id")),
            TryConvertToDisplayString(TryGetMemberValue(character, "Name")),
            TryConvertToDisplayString(TryGetMemberValue(character, "CharacterSelectTitle")),
            TryConvertToDisplayString(TryGetMemberValue(TryGetMemberValue(character, "Id") ?? character, "Entry")),
            character.ToString(),
        };

        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static object? TryGetNodeAtPath(object root, string path)
    {
        return TryInvokeMethod(root, "GetNodeOrNull", path)
               ?? TryInvokeMethod(root, "GetNode", path);
    }

    private static object? TryGetMapCoordObject(object? source)
    {
        if (source is null)
        {
            return null;
        }

        var coord = TryGetMemberValue(source, "coord") ?? TryGetMemberValue(source, "Coord");
        if (coord is not null)
        {
            return coord;
        }

        var point = TryGetMemberValue(source, "Point");
        if (point is not null && !ReferenceEquals(point, source))
        {
            return TryGetMapCoordObject(point);
        }

        var row = TryConvertToInt(TryGetMemberValue(source, "row") ?? TryGetMemberValue(source, "Row"));
        var col = TryConvertToInt(TryGetMemberValue(source, "col") ?? TryGetMemberValue(source, "Col"));
        return row is not null && col is not null ? source : null;
    }

    private static (int Row, int Col)? TryGetMapCoord(object? source)
    {
        var coordObject = TryGetMapCoordObject(source);
        if (coordObject is null)
        {
            return null;
        }

        var row = TryConvertToInt(TryGetMemberValue(coordObject, "row") ?? TryGetMemberValue(coordObject, "Row"));
        var col = TryConvertToInt(TryGetMemberValue(coordObject, "col") ?? TryGetMemberValue(coordObject, "Col"));
        return row is not null && col is not null ? (row.Value, col.Value) : null;
    }

    private static bool MapCoordsEqual((int Row, int Col)? left, (int Row, int Col)? right)
    {
        return left is not null
               && right is not null
               && left.Value.Row == right.Value.Row
               && left.Value.Col == right.Value.Col;
    }

    private static int ScoreSemanticMapPointCandidate(object point, string state, bool? isEnabled, bool wantsCombat, object? node)
    {
        var score = ScoreMapPointCandidate(point, state, isEnabled);
        var pointType = ResolveMapPointType(point, node);

        if (IsInvalidMapPointType(pointType))
        {
            return int.MinValue / 4;
        }

        if (wantsCombat)
        {
            if (pointType.Contains("Monster", StringComparison.OrdinalIgnoreCase))
            {
                score += 300;
            }
            else
            {
                score -= 75;
            }
        }

        if (state.Contains("Travelable", StringComparison.OrdinalIgnoreCase)
            || state.Contains("Reachable", StringComparison.OrdinalIgnoreCase))
        {
            score += 120;
        }

        if (isEnabled == false)
        {
            score -= 500;
        }

        if (node is not null && IsVisibleNode(node))
        {
            score += 30;
        }

        return score;
    }

    private static int ScoreMapPointCandidate(object point, string state, bool? isEnabled)
    {
        var score = 0;
        var pointType = ResolveMapPointType(point, null);
        if (IsInvalidMapPointType(pointType))
        {
            score -= 1_000;
        }

        if (pointType.Contains("Monster", StringComparison.OrdinalIgnoreCase))
        {
            score += 200;
        }

        var coord = TryGetMemberValue(point, "coord") ?? TryGetMemberValue(point, "Coord");
        var row = TryConvertToInt(TryGetMemberValue(coord ?? point, "row") ?? TryGetMemberValue(coord ?? point, "Row"));
        if (row == 0)
        {
            score += 100;
        }

        if (state.Contains("Travelable", StringComparison.OrdinalIgnoreCase)
            || state.Contains("Reachable", StringComparison.OrdinalIgnoreCase)
            || state.Contains("Idle", StringComparison.OrdinalIgnoreCase))
        {
            score += 50;
        }

        if (isEnabled == true)
        {
            score += 20;
        }

        return score;
    }

    private static string DescribeMapPoint(object? node, object? point)
    {
        var coord = TryGetMemberValue(point ?? node!, "coord") ?? TryGetMemberValue(point ?? node!, "Coord");
        var row = TryConvertToInt(TryGetMemberValue(coord ?? point ?? node!, "row") ?? TryGetMemberValue(coord ?? point ?? node!, "Row"));
        var col = TryConvertToInt(TryGetMemberValue(coord ?? point ?? node!, "col") ?? TryGetMemberValue(coord ?? point ?? node!, "Col"));
        var pointType = ResolveMapPointType(point, node);
        return row is not null && col is not null
            ? $"map-point:{pointType}@({row},{col})"
            : $"map-point:{pointType}";
    }

    private static bool IsInvalidMapPointCandidate(object? point, object? node)
    {
        return IsInvalidMapPointType(ResolveMapPointType(point, node));
    }

    private static bool IsInvalidMapPointType(string? pointType)
    {
        if (string.IsNullOrWhiteSpace(pointType))
        {
            return true;
        }

        return pointType.Contains("Unassigned", StringComparison.OrdinalIgnoreCase)
               || pointType.Contains("Unknown", StringComparison.OrdinalIgnoreCase)
               || pointType.Contains("None", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveMapPointType(object? point, object? node)
    {
        return TryConvertToDisplayString(TryGetMemberValue(point ?? typeof(object), "PointType"))
               ?? TryConvertToDisplayString(TryGetMemberValue(node ?? typeof(object), "PointType"))
               ?? TryConvertToDisplayString(TryGetMemberValue(point ?? typeof(object), "RoomType"))
               ?? TryConvertToDisplayString(TryGetMemberValue(node ?? typeof(object), "RoomType"))
               ?? TryConvertToDisplayString(TryGetMemberValue(TryGetMemberValue(point ?? typeof(object), "Model") ?? typeof(object), "PointType"))
               ?? TryConvertToDisplayString(TryGetMemberValue(TryGetMemberValue(node ?? typeof(object), "Model") ?? typeof(object), "PointType"))
               ?? TryConvertToDisplayString(TryGetMemberValue(TryGetMemberValue(point ?? typeof(object), "Model") ?? typeof(object), "RoomType"))
               ?? TryConvertToDisplayString(TryGetMemberValue(TryGetMemberValue(node ?? typeof(object), "Model") ?? typeof(object), "RoomType"))
               ?? TryConvertToDisplayString(point)
               ?? TryConvertToDisplayString(node)
               ?? "Unknown";
    }

    private static string SummarizeMapCandidates(IEnumerable<MapPointCandidate> candidates)
    {
        return string.Join(
            "; ",
            candidates.Take(8).Select(candidate =>
            {
                var summary = DescribeMapPoint(candidate.Node, candidate.Point);
                var state = string.IsNullOrWhiteSpace(candidate.State) ? "state=?" : $"state={candidate.State}";
                var enabled = candidate.Enabled is null ? "enabled=?" : $"enabled={candidate.Enabled.Value}";
                return $"{summary}|{state}|{enabled}";
            }));
    }

    private static string SummarizeVisibleTypes(IEnumerable<object> sceneRoots)
    {
        return string.Join(
            ", ",
            sceneRoots
                .SelectMany(root => EnumerateSceneNodes(root, 128))
                .Where(IsVisibleNode)
                .Select(node => node.GetType().Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(12));
    }

    private static string? TryGetVisitedMapSignature()
    {
        try
        {
            var runManager = TryResolveRunManager();
            if (runManager is null || !TryInvokeMethod(runManager, "DebugOnlyGetState", out var runState) || runState is null)
            {
                return null;
            }

            var visited = ExpandEnumerable(TryGetMemberValue(runState, "VisitedMapCoords")).ToArray();
            if (visited.Length == 0)
            {
                return "none";
            }

            var parts = visited
                .Select(coord =>
                {
                    var row = TryConvertToInt(TryGetMemberValue(coord, "row") ?? TryGetMemberValue(coord, "Row"));
                    var col = TryConvertToInt(TryGetMemberValue(coord, "col") ?? TryGetMemberValue(coord, "Col"));
                    return row is not null && col is not null ? $"{row},{col}" : coord.ToString();
                })
                .Where(value => !string.IsNullOrWhiteSpace(value));
            return string.Join(";", parts);
        }
        catch
        {
            return null;
        }
    }

    private static object? TryResolveRunManager()
    {
        var type = TryFindLoadedType("MegaCrit.Sts2.Core.Runs.RunManager");
        return type is null ? null : TryGetMemberValue(type, "Instance");
    }

    private static bool TrySubscribeEvent(object source, string eventName, Delegate handler, out EventInfo? eventInfo)
    {
        eventInfo = null;
        try
        {
            var type = source.GetType();
            eventInfo = type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .FirstOrDefault(candidate => string.Equals(candidate.Name, eventName, StringComparison.OrdinalIgnoreCase));
            if (eventInfo?.EventHandlerType is null)
            {
                eventInfo = null;
                return false;
            }

            var typedHandler = Delegate.CreateDelegate(eventInfo.EventHandlerType, handler.Target, handler.Method, throwOnBindFailure: false);
            if (typedHandler is null)
            {
                eventInfo = null;
                return false;
            }

            eventInfo.AddEventHandler(source, typedHandler);
            return true;
        }
        catch
        {
            eventInfo = null;
            return false;
        }
    }

    private static void TryUnsubscribeEvent(object source, string eventName, Delegate handler)
    {
        try
        {
            var type = source.GetType();
            var eventInfo = type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .FirstOrDefault(candidate => string.Equals(candidate.Name, eventName, StringComparison.OrdinalIgnoreCase));
            if (eventInfo?.EventHandlerType is null)
            {
                return;
            }

            var typedHandler = Delegate.CreateDelegate(eventInfo.EventHandlerType, handler.Target, handler.Method, throwOnBindFailure: false);
            if (typedHandler is null)
            {
                return;
            }

            eventInfo.RemoveEventHandler(source, typedHandler);
        }
        catch
        {
            // Ignore unsubscribe failures during harness probing.
        }
    }

    private static object? TryGetMemberValue(object source, string memberName)
    {
        try
        {
            var type = source as Type ?? source.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var property = type.GetProperties(flags).FirstOrDefault(candidate => string.Equals(candidate.Name.TrimStart('_'), memberName.TrimStart('_'), StringComparison.OrdinalIgnoreCase));
            if (property is not null)
            {
                return property.GetValue(source is Type ? null : source);
            }

            var field = type.GetFields(flags).FirstOrDefault(candidate => string.Equals(candidate.Name.TrimStart('_'), memberName.TrimStart('_'), StringComparison.OrdinalIgnoreCase));
            if (field is not null)
            {
                return field.GetValue(source is Type ? null : source);
            }
        }
        catch
        {
            // Ignore reflection failures during bridge probing.
        }

        return null;
    }

    private static Type? TryFindLoadedType(string fullName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(fullName, throwOnError: false, ignoreCase: false))
            .FirstOrDefault(type => type is not null);
    }

    private static bool TryInvokeGenericMethod(object source, string methodName, IReadOnlyList<Type> genericTypeArguments, out object? result, params object?[]? args)
    {
        result = null;
        try
        {
            var type = source as Type ?? source.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var providedArguments = args ?? Array.Empty<object?>();
            var methods = type.GetMethods(flags)
                .Where(candidate =>
                    string.Equals(candidate.Name, methodName, StringComparison.OrdinalIgnoreCase)
                    && candidate.IsGenericMethodDefinition
                    && candidate.GetGenericArguments().Length == genericTypeArguments.Count)
                .OrderBy(candidate => candidate.GetParameters().Length)
                .ToArray();

            foreach (var genericDefinition in methods)
            {
                var method = genericDefinition.MakeGenericMethod(genericTypeArguments.ToArray());
                var parameters = method.GetParameters();
                if (parameters.Length < providedArguments.Length)
                {
                    continue;
                }

                if (parameters.Skip(providedArguments.Length).Any(parameter => !parameter.IsOptional))
                {
                    continue;
                }

                var invocationArguments = new object?[parameters.Length];
                var compatible = true;
                for (var index = 0; index < providedArguments.Length; index += 1)
                {
                    if (!CanAssignArgument(parameters[index].ParameterType, providedArguments[index]))
                    {
                        compatible = false;
                        break;
                    }

                    invocationArguments[index] = providedArguments[index];
                }

                if (!compatible)
                {
                    continue;
                }

                for (var index = providedArguments.Length; index < parameters.Length; index += 1)
                {
                    invocationArguments[index] = parameters[index].HasDefaultValue ? parameters[index].DefaultValue : null;
                }

                result = method.Invoke(source is Type ? null : source, invocationArguments);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool TryInvokeMethod(object source, string methodName, out object? result, params object?[]? args)
    {
        result = null;
        try
        {
            var type = source as Type ?? source.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var providedArguments = args ?? Array.Empty<object?>();
            var methods = type.GetMethods(flags)
                .Where(candidate => string.Equals(candidate.Name, methodName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(candidate => candidate.GetParameters().Length)
                .ToArray();

            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length < providedArguments.Length)
                {
                    continue;
                }

                if (parameters.Skip(providedArguments.Length).Any(parameter => !parameter.IsOptional))
                {
                    continue;
                }

                var invocationArguments = new object?[parameters.Length];
                var compatible = true;
                for (var index = 0; index < providedArguments.Length; index += 1)
                {
                    if (!CanAssignArgument(parameters[index].ParameterType, providedArguments[index]))
                    {
                        compatible = false;
                        break;
                    }

                    invocationArguments[index] = providedArguments[index];
                }

                if (!compatible)
                {
                    continue;
                }

                for (var index = providedArguments.Length; index < parameters.Length; index += 1)
                {
                    invocationArguments[index] = parameters[index].HasDefaultValue ? parameters[index].DefaultValue : null;
                }

                result = method.Invoke(source is Type ? null : source, invocationArguments);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool TryInvokeTaskMethod(object source, string methodName, TimeSpan timeout, out string observed, params object?[]? args)
    {
        observed = string.Empty;
        if (!TryInvokeMethod(source, methodName, out var result, args))
        {
            return false;
        }

        if (result is Task task)
        {
            try
            {
                if (!task.Wait(timeout))
                {
                    observed = "task-timeout";
                    return false;
                }
            }
            catch (AggregateException exception)
            {
                observed = exception.Flatten().InnerExceptions.FirstOrDefault()?.Message ?? exception.Message;
                return false;
            }
            catch (Exception exception)
            {
                observed = exception.Message;
                return false;
            }

            if (task.IsFaulted)
            {
                observed = task.Exception?.GetBaseException().Message ?? "task-faulted";
                return false;
            }
        }

        var deadline = DateTimeOffset.UtcNow.AddSeconds(8);
        while (DateTimeOffset.UtcNow < deadline)
        {
            Thread.Sleep(200);
            var refreshedRoots = TryResolveSceneRoots();
            if (FindFirstNodeByType(refreshedRoots, "NCharacterSelectScreen") is not null)
            {
                observed = "launch-main-menu:character-select";
                return true;
            }

            if (FindFirstNodeByType(refreshedRoots, "NSingleplayerSubmenu") is not null)
            {
                observed = "launch-main-menu:singleplayer-submenu";
                return true;
            }

            if (FindFirstNodeByType(refreshedRoots, "NMainMenu") is not null)
            {
                observed = "launch-main-menu:main-menu";
                return true;
            }
        }

        return false;
    }

    private static object? TryInvokeMethod(object source, string methodName, params object?[]? args)
    {
        return TryInvokeMethod(source, methodName, out var result, args) ? result : null;
    }

    private static bool CanAssignArgument(Type parameterType, object? argument)
    {
        if (argument is null)
        {
            return !parameterType.IsValueType || Nullable.GetUnderlyingType(parameterType) is not null;
        }

        return parameterType.IsInstanceOfType(argument);
    }

    private static int? TryConvertToInt(object? value)
    {
        return value switch
        {
            int intValue => intValue,
            long longValue when longValue is <= int.MaxValue and >= int.MinValue => (int)longValue,
            _ => null,
        };
    }

    private static bool? TryConvertToBool(object? value)
    {
        return value switch
        {
            bool boolValue => boolValue,
            _ => null,
        };
    }

    private static string? TryConvertToDisplayString(object? value)
    {
        if (value is null)
        {
            return null;
        }

        return value switch
        {
            string stringValue => stringValue.Trim(),
            char character => character.ToString(),
            bool boolValue => boolValue ? "true" : "false",
            byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture),
            Enum enumValue => enumValue.ToString(),
            _ => null,
        };
    }

    private sealed record HarnessActionEnvelope(HarnessAction Action, CompanionState State);
    private sealed record MapPointCandidate(object? Node, object Point, string State, bool? Enabled);
    private sealed record NodeCandidate(object Node, object? ActivationNode, string? Label, string? NodeName, string TypeName);
    private sealed record InventoryDispatchTarget(
        NodeCandidate Candidate,
        string Kind,
        string Label,
        string? Description,
        string TypeName,
        string? UiPath,
        bool Visible,
        bool? Enabled,
        bool Actionable,
        IReadOnlyList<string> SemanticHints);
    private sealed record InventoryDispatchNode(string NodeId, InventoryDispatchTarget Target);
    private sealed record InventorySnapshot(
        HarnessNodeInventory Inventory,
        IReadOnlyList<InventoryDispatchNode> Nodes,
        string Signature);
    private sealed record ClickAttemptSnapshot(
        string Scene,
        string LiveScene,
        int OverlayCount,
        bool InputLocked,
        string InputLockReason,
        string TargetLabel,
        string ButtonLabel,
        bool ButtonVisible,
        bool? ButtonEnabled);
    private sealed record ClickDispatchOutcome(
        bool Success,
        string DispatchPath,
        string? Observed,
        string? FailureKind,
        NodeCandidate? Candidate);
    private sealed record ClickPostflightOutcome(
        bool Success,
        string? FailureKind,
        string? Observed,
        string ActualScene,
        ClickAttemptSnapshot FinalSnapshot,
        bool SceneTransitionObserved,
        bool ButtonStateChanged,
        bool AuxiliaryChanged);

    private static IEnumerable<object> TryGetChildMapCoordObjects(object? runState, (int Row, int Col)? currentCoord, IReadOnlyList<(int Row, int Col)> visitedCoords)
    {
        if (runState is null)
        {
            yield break;
        }

        var currentPoint = ExpandEnumerable(TryGetMemberValue(runState, "CurrentMapPoint"))
            .Concat(new[] { TryGetMemberValue(runState, "CurrentMapPoint") })
            .FirstOrDefault(candidate => candidate is not null);
        var coordRoot = currentPoint ?? TryGetMemberValue(runState, "CurrentMapCoord");
        var children = ExpandEnumerable(TryGetMemberValue(coordRoot ?? typeof(object), "Children"))
            .Concat(ExpandEnumerable(TryGetMemberValue(currentPoint ?? typeof(object), "Children")))
            .ToArray();

        foreach (var child in children)
        {
            var coord = TryGetMapCoord(child);
            if (coord is null)
            {
                continue;
            }

            if (currentCoord is not null && MapCoordsEqual(coord, currentCoord))
            {
                continue;
            }

            if (visitedCoords.Any(visited => MapCoordsEqual(coord, visited)))
            {
                continue;
            }

            yield return child;
        }
    }
}
