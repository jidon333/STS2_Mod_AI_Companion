using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModAiCompanion.HarnessBridge.ActionIngress;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModAiCompanion.HarnessBridge;

internal sealed class HarnessBridgeHost : IHarnessActionIngress
{
    private static readonly string[] SingletonPropertyNames = { "Instance", "Current", "Singleton", "State" };
    private static readonly string[] NestedRootNames = { "Run", "RunState", "State", "Player", "Character", "Encounter", "Combat", "Manager" };
    private static readonly string[] SceneNodeKeywords = { "Screen", "Room", "Map", "Reward", "Event", "Shop", "Rest", "Button", "Character", "Menu", "Card" };
    private static readonly string[] FtuesToDisable = { "accept_tutorials_ftue", "accept_tutorial_ftue" };

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
        WriteStatus("idle", null, null, "harness bridge started");
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
        if (!File.Exists(_layout.ActionsPath))
        {
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

            if (envelope?.Action is null || !_processedActionIds.Add(envelope.Action.ActionId))
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
            WriteStatus("idle", envelope.Action.ActionId, result.Status, result.FailureKind);
        }
    }

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
        return ClickButton(action with { TargetLabel = aliases.FirstOrDefault() }, state, startedAt, aliases);
    }

    private HarnessActionResult ClickButton(
        HarnessAction action,
        CompanionState state,
        DateTimeOffset startedAt,
        IReadOnlyList<string>? aliases = null)
    {
        HarnessActionResult? semanticFailure = null;
        var semanticResult = TryExecuteButtonSemanticAction(action, state, startedAt);
        if (semanticResult is not null)
        {
            if (string.Equals(semanticResult.Status, "ok", StringComparison.OrdinalIgnoreCase))
            {
                return semanticResult;
            }

            semanticFailure = semanticResult;
        }

        var sceneRoots = TryResolveSceneRoots();
        if (sceneRoots.Count == 0)
        {
            return semanticFailure ?? BuildResult(action, startedAt, "failed", true, failureKind: "scene-root-unavailable");
        }

        var resolvedAliases = aliases ?? ResolveHarnessAliases(action.TargetLabel, state);
        var candidates = sceneRoots
            .SelectMany(root => EnumerateSceneNodes(root, 4096))
            .Select(CreateNodeCandidate)
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate.Label) || !string.IsNullOrWhiteSpace(candidate.TypeName))
            .OrderByDescending(candidate => ScoreButtonCandidate(candidate, action.TargetLabel, resolvedAliases))
            .ToArray();

        foreach (var candidate in candidates)
        {
            if (ScoreButtonCandidate(candidate, action.TargetLabel, resolvedAliases) <= 0)
            {
                break;
            }

            if (TryActivateCandidate(candidate))
            {
                var observed = candidate.Label ?? candidate.TypeName;
                if (!string.IsNullOrWhiteSpace(semanticFailure?.ObservedStateDelta))
                {
                    observed = CombineObserved(semanticFailure.ObservedStateDelta, observed);
                }

                return BuildResult(action, startedAt, "ok", false, observed: observed);
            }
        }

        if (semanticFailure is not null)
        {
            return semanticFailure with
            {
                FailureKind = $"{semanticFailure.FailureKind}|matching-node-not-found:{SummarizeCandidates(candidates)}",
            };
        }

        return BuildResult(action, startedAt, "failed", true, failureKind: $"matching-node-not-found:{SummarizeCandidates(candidates)}");
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
                if (FindFirstVisibleNodeByType(sceneRoots, "NCombatRoom") is not null
                    || FindFirstVisibleNodeByType(sceneRoots, "NCombatScreen") is not null
                    || FindFirstVisibleNodeByType(sceneRoots, "NCombatHud") is not null)
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

            TryInvokeMethod(mapRoom ?? typeof(object), "ReopenMap", out _);
            TryInvokeMethod(mapScreen, "Open", out _);
            TryInvokeMethod(mapScreen, "Open", out _, false);

            if (runState is not null)
            {
                TryInvokeMethod(mapScreen, "Initialize", out _, runState);
                var runMap = TryGetMemberValue(runState, "Map");
                var rng = TryGetMemberValue(runState, "Rng");
                var seed = TryGetMemberValue(rng ?? typeof(object), "Seed");
                if (runMap is not null && seed is not null)
                {
                    TryInvokeMethod(mapScreen, "SetMap", out _, runMap, seed, false);
                }
            }

            TryInvokeMethod(mapScreen, "SetTravelEnabled", out _, true);
            TryInvokeMethod(mapScreen, "RecalculateTravelability", out _);
            TryInvokeMethod(mapScreen, "InitMapVotes", out _);

            var visitedSignatureBefore = TryGetVisitedMapSignature();
            var pointsRoot = TryGetMemberValue(mapScreen, "_points")
                             ?? TryGetMemberValue(mapScreen, "Points")
                             ?? TryGetNodeAtPath(mapScreen, "TheMap/Points");
            var pointsRootChildCount = ExpandEnumerable(TryInvokeMethod(pointsRoot ?? typeof(object), "GetChildren")).Count();
            var mapPointDictionary = TryGetMemberValue(mapScreen, "_mapPointDictionary") ?? TryGetMemberValue(mapScreen, "MapPointDictionary");
            var mapPointDictionaryCount = ExpandEnumerable(TryGetMemberValue(mapPointDictionary ?? typeof(object), "Values")).Count();
            var mapScreenIsOpen = TryConvertToBool(TryGetMemberValue(mapScreen, "IsOpen"));
            var mapScreenVisible = IsVisibleNode(mapScreen);
            var travelEnabled = TryConvertToBool(TryGetMemberValue(mapScreen, "IsTravelEnabled"));
            var traveling = TryConvertToBool(TryGetMemberValue(mapScreen, "IsTraveling"));

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
            var mapTaskFailure = string.Empty;
            var activated = (mapPoint.Node is not null && TryInvokeMethod(mapScreen, "OnMapPointSelectedLocally", out _, mapPoint.Node))
                            || (mapPoint.Node is not null && TryInvokeMethod(mapPoint.Node, "OnRelease", out _))
                            || (mapCoordObject is not null && TryInvokeTaskMethod(mapScreen, "TravelToMapCoord", TimeSpan.FromSeconds(15), out mapTaskFailure, mapCoordObject))
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
                taskFailure = string.IsNullOrWhiteSpace(mapTaskFailure) ? null : mapTaskFailure,
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
            .Where(candidate => candidate.Point is not null)
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
            .Where(candidate => candidate.Point is not null)
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
        var pointType = TryConvertToDisplayString(TryGetMemberValue(point, "PointType"))
                        ?? TryConvertToDisplayString(TryGetMemberValue(node ?? typeof(object), "PointType"))
                        ?? point.ToString()
                        ?? string.Empty;
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
        var dismissedOverlay = false;
        var mainMenu = TryResolveMainMenu(sceneRoots);
        if (mainMenu is null || !IsVisibleNode(mainMenu))
        {
            if (TryDismissBlockingOverlays(sceneRoots, out var overlayObserved) && !string.IsNullOrWhiteSpace(overlayObserved))
            {
                observedParts.Add(overlayObserved);
                dismissedOverlay = true;
                mainMenu = TryResolveMainMenu(TryResolveSceneRoots());
            }
        }

        if (mainMenu is null || !IsVisibleNode(mainMenu))
        {
            observed = observedParts.Count == 0 ? "main-menu-unavailable" : string.Join("|", observedParts);
            failureKind = dismissedOverlay ? "main-menu-pending-after-overlay-dismiss" : "main-menu-unavailable";
            return false;
        }

        if (TryOpenCharacterSelectFromMainMenu(mainMenu, state, out var directCharacterSelectObserved))
        {
            observed = CombineObserved(string.Join("|", observedParts), directCharacterSelectObserved);
            failureKind = string.Empty;
            return true;
        }

        if (TryOpenSingleplayerSubmenuFromMainMenu(mainMenu, state, out var startObserved))
        {
            observed = CombineObserved(string.Join("|", observedParts), startObserved);
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
            if (TryInvokeMethod(game, "ReloadMainMenu", out _))
            {
                observed = "reload-main-menu";
                Thread.Sleep(500);
                return true;
            }
        }

        if (TryInvokeTaskMethod(game, "LaunchMainMenu", TimeSpan.FromSeconds(8), out var launchObserved, true))
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
                else if (TryInvokeMethod(game, "LaunchMainMenu", out _, true))
                {
                    observedParts.Add("launch-main-menu");
                    launchAttempted = true;
                }
                else if (TryInvokeMethod(game, "ReloadMainMenu", out _))
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

        if (TryInvokeMethod(mainMenu, "OpenSingleplayerSubmenu", out _))
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
            && TryInvokeMethod(mainMenu, "SingleplayerButtonPressed", out _, singleplayerButton))
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

        if (TryOpenCharacterSelectDirectly(mainMenu, out var directObserved))
        {
            observed = CombineObserved(string.Join("|", observedParts), directObserved);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(directObserved))
        {
            observedParts.Add(directObserved);
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

        if (TryInvokeMethod(mainMenu, "OpenSingleplayerSubmenu", out _))
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
                TryInvokeMethod(characterSelect, "InitializeSingleplayer", out _);
                TryInvokeMethod(characterSelect, "OnSubmenuOpened", out _);
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
            if (TryInvokeMethod(timeoutOverlay, methodName, out _, null) || TryInvokeMethod(timeoutOverlay, methodName, out _))
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
        if (standardButton is not null && TryInvokeMethod(submenu, "OpenCharacterSelect", out _, standardButton))
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
            if (TryInvokeMethod(submenu, "OpenCharacterSelect", out _, candidate) || TryActivateNode(candidate))
            {
                observed = ResolveNodeLabel(candidate) ?? ResolveNodeName(candidate) ?? "standard-run";
                return true;
            }
        }

        if (TryInvokeMethod(submenu, "OpenCharacterSelect", out _, null))
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
            if (TryInvokeMethod(feedbackScreen, methodName, out _, null) || TryInvokeMethod(feedbackScreen, methodName, out _))
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

        var candidates = EnumerateSceneNodes(feedbackScreen, 256)
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
                observed = $"feedback-screen:{candidate.Label ?? candidate.NodeName ?? candidate.TypeName}";
                Thread.Sleep(200);
                return true;
            }
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
        if ((standardButton is not null && TryInvokeMethod(submenu, "OpenCharacterSelect", out _, standardButton))
            || TryInvokeMethod(submenu, "OpenCharacterSelect", out _, null)
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

        if (TryInvokeMethod(mainMenu, "OpenSingleplayerSubmenu", out var openedSubmenu) && openedSubmenu is not null)
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
                TryInvokeMethod(resolvedSubmenu, "OnSubmenuOpened", out _);
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

            if (TryGetMemberValue(characterSelect, "_lobby") is null && TryGetMemberValue(characterSelect, "Lobby") is null)
            {
                TryInvokeMethod(characterSelect, "InitializeSingleplayer", out _);
            }

            TryInvokeMethod(characterSelect, "OnSubmenuOpened", out _);

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
            var selectedViaScreen = selected.Character is not null
                                    && TryInvokeMethod(characterSelect, "SelectCharacter", out _, selected.Node, selected.Character);
            var selectedViaButton = selectedViaScreen
                                    || TryInvokeMethod(selected.Node, "Select", out _)
                                    || TryActivateNode(selected.Node);
            if (!selectedViaButton)
            {
                return (false, CombineObserved(overlayObserved, $"character-select-activate-failed:{identity}"), "character-select-activate-failed");
            }

            var lobby = TryGetMemberValue(characterSelect, "_lobby") ?? TryGetMemberValue(characterSelect, "Lobby");
            if (lobby is not null && selected.Character is not null)
            {
                TryInvokeMethod(lobby, "SetCharacter", out _, selected.Character);
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
        TryForceFtuesDisabled();
        var roots = sceneRoots;
        var observedParts = new List<string>();
        if (TryDismissBlockingOverlays(roots, out var overlayObserved) && !string.IsNullOrWhiteSpace(overlayObserved))
        {
            observedParts.Add(overlayObserved);
            Thread.Sleep(250);
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
            TryInvokeMethod(characterSelect, "InitializeSingleplayer", out _);
            TryInvokeMethod(characterSelect, "OnSubmenuOpened", out _);
        }

        var embarkButton = TryGetMemberValue(characterSelect, "_embarkButton") ?? TryGetMemberValue(characterSelect, "EmbarkButton");
        if (embarkButton is not null && TryInvokeMethod(characterSelect, "OnEmbarkPressed", out _, embarkButton))
        {
            observedParts.Add("embark-pressed-with-button");
            if (TryWaitForRunStart(TimeSpan.FromSeconds(3), out var runObserved))
            {
                observed = CombineObserved(string.Join("|", observedParts), runObserved);
                return true;
            }
        }

        if (TryInvokeMethod(characterSelect, "OnEmbarkPressed", out _, null))
        {
            observedParts.Add("embark-pressed");
            if (TryWaitForRunStart(TimeSpan.FromSeconds(3), out var runObserved))
            {
                observed = CombineObserved(string.Join("|", observedParts), runObserved);
                return true;
            }
        }

        var lobby = TryGetMemberValue(characterSelect, "_lobby") ?? TryGetMemberValue(characterSelect, "Lobby");
        if (lobby is not null && TryInvokeMethod(lobby, "SetReady", out _, true))
        {
            observedParts.Add("lobby-ready");
            if (TryInvokeMethod(lobby, "BeginRunIfAllPlayersReady", out _))
            {
                observedParts.Add("begin-run-if-ready");
            }

            if (TryWaitForRunStart(TimeSpan.FromSeconds(4), out var runObserved))
            {
                observed = CombineObserved(string.Join("|", observedParts), runObserved);
                return true;
            }
        }

        if (confirmButton is not null && TryActivateNode(confirmButton))
        {
            observedParts.Add("confirm-button");
            if (TryWaitForRunStart(TimeSpan.FromSeconds(3), out var runObserved))
            {
                observed = CombineObserved(string.Join("|", observedParts), runObserved);
                return true;
            }
        }

        if (embarkButton is not null && TryActivateNode(embarkButton))
        {
            observedParts.Add("embark-button");
            if (TryWaitForRunStart(TimeSpan.FromSeconds(3), out var runObserved))
            {
                observed = CombineObserved(string.Join("|", observedParts), runObserved);
                return true;
            }
        }

        if (lobby is not null && TryForceSingleplayerRunStart(characterSelect, lobby, out var forceObserved))
        {
            observedParts.Add(forceObserved);
            if (TryWaitForRunStart(TimeSpan.FromSeconds(8), out var runObserved))
            {
                observed = CombineObserved(string.Join("|", observedParts), runObserved);
                return true;
            }
        }

        observed = string.Join("|", observedParts.Where(part => !string.IsNullOrWhiteSpace(part)).Distinct(StringComparer.OrdinalIgnoreCase));
        return false;
    }

    private static bool TryForceSingleplayerRunStart(object characterSelect, object lobby, out string observed)
    {
        observed = string.Empty;
        var observedParts = new List<string>();

        if (TryInvokeMethod(lobby, "BeginRunIfAllPlayersReady", out _))
        {
            observedParts.Add("begin-run-if-ready");
            if (TryWaitForRunStart(TimeSpan.FromSeconds(4), out var beginObserved))
            {
                observed = CombineObserved(string.Join("|", observedParts), beginObserved);
                return true;
            }
        }

        if (TryResolveLobbySeed(lobby, out var seed)
            && TryGetLobbyModifiers(lobby, out var modifiers)
            && TryInvokeMethod(lobby, "BeginRun", out _, seed, modifiers))
        {
            observedParts.Add("begin-run-direct");
            if (TryWaitForRunStart(TimeSpan.FromSeconds(6), out var beginObserved))
            {
                observed = CombineObserved(string.Join("|", observedParts), beginObserved);
                return true;
            }
        }

        if (TryResolveLobbySeed(lobby, out seed)
            && TryBuildActsForSeed(lobby, seed, out var acts)
            && TryGetLobbyModifiers(lobby, out modifiers)
            && TryInvokeMethod(characterSelect, "BeginRun", out _, seed, acts, modifiers))
        {
            observedParts.Add("character-select-begin-run");
            if (TryWaitForRunStart(TimeSpan.FromSeconds(6), out var beginObserved))
            {
                observed = CombineObserved(string.Join("|", observedParts), beginObserved);
                return true;
            }
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

    private static void TryForceFtuesDisabled()
    {
        try
        {
            var saveManagerType = TryFindLoadedType("MegaCrit.Sts2.Core.Saves.SaveManager");
            var saveManager = saveManagerType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (saveManager is null || saveManagerType is null)
            {
                return;
            }

            saveManagerType.GetMethod("SetFtuesEnabled", BindingFlags.Public | BindingFlags.Instance)?.Invoke(saveManager, new object[] { false });
            foreach (var ftueKey in FtuesToDisable)
            {
                saveManagerType.GetMethod("MarkFtueAsComplete", BindingFlags.Public | BindingFlags.Instance)?.Invoke(saveManager, new object[] { ftueKey });
            }
        }
        catch
        {
            // Best-effort only for harness mode.
        }
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

            if (FindFirstVisibleNodeByType(refreshedRoots, "NCombatRoom") is not null
                || FindFirstVisibleNodeByType(refreshedRoots, "NCombatScreen") is not null
                || FindFirstVisibleNodeByType(refreshedRoots, "NCombatHud") is not null)
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

        if (FindFirstVisibleNodeByType(refreshedRoots, "NCombatRoom") is not null
            || FindFirstVisibleNodeByType(refreshedRoots, "NCombatScreen") is not null
            || FindFirstVisibleNodeByType(refreshedRoots, "NCombatHud") is not null)
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

            if (!string.IsNullOrWhiteSpace(snapshot.RunStatus)
                && !string.Equals(snapshot.RunStatus, "idle", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(snapshot.RunStatus, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                observed = $"run-started:map-inferred:status:{snapshot.RunStatus}";
                return true;
            }

            if (snapshot.Act is not null || snapshot.Floor is not null || snapshot.Deck.Count > 0 || snapshot.Relics.Count > 0)
            {
                observed = "run-started:map-inferred:progress";
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
        if (FindFirstVisibleNodeByType(sceneRoots, "NCombatRoom") is not null
            || FindFirstVisibleNodeByType(sceneRoots, "NCombatScreen") is not null
            || FindFirstVisibleNodeByType(sceneRoots, "NCombatHud") is not null)
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

    private static bool InvokeOnMainThread<T>(Func<T> action, int timeoutMs, out T result)
    {
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
        var pointType = TryConvertToDisplayString(TryGetMemberValue(point, "PointType")) ?? point.ToString() ?? string.Empty;

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
        var pointType = TryConvertToDisplayString(TryGetMemberValue(point, "PointType")) ?? point.ToString() ?? string.Empty;
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
        var pointType = TryConvertToDisplayString(TryGetMemberValue(point ?? node!, "PointType")) ?? "Unknown";
        return row is not null && col is not null
            ? $"map-point:{pointType}@({row},{col})"
            : $"map-point:{pointType}";
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
