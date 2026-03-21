using System.Security.Cryptography;
using System.Text;
using Sts2AiCompanion.Foundation.State;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModAiCompanion.HarnessBridge;

internal sealed class InventoryPublisher
{
    private readonly string _inventoryPath;
    private readonly string _liveSnapshotPath;
    private string? _lastFingerprint;
    private string? _lastObservedSceneType;
    private int _lastObservedSceneStreak;

    public InventoryPublisher(string inventoryPath, string liveSnapshotPath)
    {
        _inventoryPath = inventoryPath;
        _liveSnapshotPath = liveSnapshotPath;
    }

    public InventoryPublishAttempt TryPublish(LiveSnapshotReader snapshotReader, string mode)
    {
        var snapshot = snapshotReader.TryRead(_liveSnapshotPath);
        if (snapshot is null)
        {
            return InventoryPublishAttempt.None;
        }

        var normalizedScene = CompanionSceneNormalizer.Normalize(snapshot);
        var inventory = BuildInventory(snapshot, mode, normalizedScene);
        if (ShouldSuppressPublish(normalizedScene.SceneType))
        {
            return new InventoryPublishAttempt(
                Published: false,
                Suppressed: true,
                Inventory: inventory,
                RawSceneType: string.IsNullOrWhiteSpace(snapshot.CurrentScreen) ? "unknown" : snapshot.CurrentScreen.Trim(),
                SuppressionReason: "scene-not-yet-stable");
        }

        var fingerprint = BuildFingerprint(inventory);
        if (string.Equals(_lastFingerprint, fingerprint, StringComparison.Ordinal))
        {
            return new InventoryPublishAttempt(
                Published: false,
                Suppressed: false,
                Inventory: inventory,
                RawSceneType: string.IsNullOrWhiteSpace(snapshot.CurrentScreen) ? "unknown" : snapshot.CurrentScreen.Trim(),
                SuppressionReason: null);
        }

        LiveExportAtomicFileWriter.WriteJsonAtomic(_inventoryPath, inventory);
        _lastFingerprint = fingerprint;
        return new InventoryPublishAttempt(
            Published: true,
            Suppressed: false,
            Inventory: inventory,
            RawSceneType: string.IsNullOrWhiteSpace(snapshot.CurrentScreen) ? "unknown" : snapshot.CurrentScreen.Trim(),
            SuppressionReason: null);
    }

    private bool ShouldSuppressPublish(string sceneType)
    {
        if (string.Equals(_lastObservedSceneType, sceneType, StringComparison.OrdinalIgnoreCase))
        {
            _lastObservedSceneStreak += 1;
        }
        else
        {
            _lastObservedSceneType = sceneType;
            _lastObservedSceneStreak = 1;
        }

        if (CompanionSceneNormalizer.IsStableSceneForImmediateInventoryPublish(sceneType))
        {
            return false;
        }

        return _lastObservedSceneStreak < 2;
    }

    private static HarnessNodeInventory BuildInventory(LiveExportSnapshot snapshot, string mode, CompanionNormalizedScene normalizedScene)
    {
        var sceneType = ResolveInventorySceneType(snapshot, normalizedScene);
        var blockingModal = ResolveBlockingModal(snapshot, normalizedScene);
        var nodes = snapshot.CurrentChoices
            .Select((choice, index) => BuildNode(sceneType, choice, index))
            .ToArray();

        return new HarnessNodeInventory(
            InventoryId: BuildInventoryId(snapshot, sceneType, nodes),
            CapturedAt: snapshot.CapturedAt,
            RunId: string.IsNullOrWhiteSpace(snapshot.RunId) ? null : snapshot.RunId,
            SceneType: sceneType,
            SceneEpisodeId: TryGetMeta(snapshot.Meta, "screen-episode"),
            Mode: mode,
            BlockingModal: blockingModal,
            SceneReady: ResolveSceneReady(snapshot, normalizedScene, blockingModal),
            SceneAuthority: ResolveSceneAuthority(snapshot, normalizedScene),
            SceneStability: ResolveSceneStability(snapshot, normalizedScene, blockingModal),
            Nodes: nodes);
    }

    private static string ResolveInventorySceneType(LiveExportSnapshot snapshot, CompanionNormalizedScene normalizedScene)
    {
        var logicalScreen = NormalizeSceneToken(snapshot.CurrentScreen)
            ?? NormalizeSceneToken(TryGetMeta(snapshot.Meta, "logicalScreen"))
            ?? NormalizeSceneToken(TryGetMeta(snapshot.Meta, "flowScreen"));

        if (logicalScreen is not null)
        {
            return logicalScreen;
        }

        return normalizedScene.SceneType;
    }

    private static HarnessNodeInventoryItem BuildNode(string sceneType, LiveExportChoiceSummary choice, int index)
    {
        var label = choice.Label?.Trim() ?? string.Empty;
        var kind = ResolveKind(sceneType, choice);
        var actionable = !string.IsNullOrWhiteSpace(label) && !CompanionSceneNormalizer.IsOverlayChoice(label);
        var hints = BuildHints(sceneType, choice, kind);

        return new HarnessNodeInventoryItem(
            NodeId: string.IsNullOrWhiteSpace(choice.NodeId) ? $"{kind}:{index}" : choice.NodeId,
            Kind: kind,
            Label: label,
            Description: choice.Description,
            TypeName: string.IsNullOrWhiteSpace(choice.Kind) ? "choice" : choice.Kind,
            UiPath: null,
            Visible: true,
            Enabled: null,
            Actionable: actionable,
            ScreenBounds: choice.ScreenBounds,
            SemanticHints: hints);
    }

    private static string ResolveKind(string sceneType, LiveExportChoiceSummary choice)
    {
        var label = choice.Label?.Trim() ?? string.Empty;
        if (CompanionSceneNormalizer.IsOverlayChoice(label))
        {
            return "overlay-dismiss";
        }

        return sceneType switch
        {
            "main-menu" => ResolveMainMenuKind(label),
            "singleplayer-submenu" => "mode-option",
            "character-select" => IsEmbarkLabel(label) ? "embark" : "character",
            "map" => IsExplicitMapPointChoice(choice) ? "map-node" : NormalizeChoiceKind(choice.Kind),
            "rewards" => IsProceedLabel(label) ? "proceed" : "reward-item",
            "event" => "event-option",
            "shop" => "shop-option",
            "rest-site" => "rest-option",
            _ => NormalizeChoiceKind(choice.Kind),
        };
    }

    private static IReadOnlyList<string> BuildHints(string sceneType, LiveExportChoiceSummary choice, string resolvedKind)
    {
        var hints = new List<string>(capacity: 8)
        {
            $"scene:{sceneType}",
            $"kind:{resolvedKind}",
        };

        if (!string.IsNullOrWhiteSpace(choice.Value))
        {
            hints.Add($"value:{choice.Value.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(choice.Kind))
        {
            hints.Add($"raw-kind:{choice.Kind.Trim().ToLowerInvariant()}");
        }

        if (!string.IsNullOrWhiteSpace(choice.NodeId))
        {
            hints.Add($"node-id:{choice.NodeId.Trim()}");
        }

        foreach (var hint in choice.SemanticHints)
        {
            if (!string.IsNullOrWhiteSpace(hint))
            {
                hints.Add(hint.Trim());
            }
        }

        if (TryExtractCoordHint(choice.Description) is { } coordHint)
        {
            hints.Add(coordHint);
        }

        if (string.Equals(resolvedKind, "map-node", StringComparison.OrdinalIgnoreCase))
        {
            hints.Add("source:map-choice");
        }

        if (CompanionSceneNormalizer.IsOverlayChoice(choice.Label))
        {
            hints.Add("overlay");
        }

        return hints.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string NormalizeChoiceKind(string? kind)
    {
        return string.IsNullOrWhiteSpace(kind) ? "choice" : kind.Trim().ToLowerInvariant();
    }

    private static bool IsExplicitMapPointChoice(LiveExportChoiceSummary choice)
    {
        if (string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(choice.NodeId)
            && choice.NodeId.StartsWith("map:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (TryExtractCoordHint(choice.Description) is not null)
        {
            return true;
        }

        return choice.SemanticHints.Any(hint =>
            !string.IsNullOrWhiteSpace(hint)
            && (hint.StartsWith("coord:", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "raw-kind:map-node", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hint, "source:map-choice", StringComparison.OrdinalIgnoreCase)));
    }

    private static string? TryExtractCoordHint(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        foreach (var segment in description.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment.StartsWith("coord:", StringComparison.OrdinalIgnoreCase))
            {
                return segment;
            }
        }

        return null;
    }

    private static string ResolveMainMenuKind(string label)
    {
        if (ContainsAny(label, "\uD504\uB85C\uD544", "profile"))
        {
            return "profile-slot";
        }

        if (ContainsAny(label, "\uACC4\uC18D", "continue"))
        {
            return "continue-run";
        }

        return "menu-action";
    }

    private static bool IsEmbarkLabel(string label)
    {
        return ContainsAny(label, "\uCD9C\uBC1C", "embark");
    }

    private static bool IsProceedLabel(string label)
    {
        return ContainsAny(label, "\uC9C4\uD589", "\uB118\uAE30\uAE30", "proceed", "continue", "skip");
    }

    private static bool ContainsAny(string source, params string[] candidates)
    {
        return candidates.Any(candidate => source.Contains(candidate, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ResolveBlockingModal(LiveExportSnapshot snapshot, CompanionNormalizedScene normalizedScene)
    {
        if (normalizedScene.SceneType is "feedback-overlay" or "blocking-overlay")
        {
            return TryGetMeta(snapshot.Meta, "modal-type") ?? normalizedScene.SceneType;
        }

        if (!bool.TryParse(TryGetMeta(snapshot.Meta, "modal-blocking"), out var blocking) || !blocking)
        {
            return null;
        }

        return TryGetMeta(snapshot.Meta, "modal-type") ?? "blocking";
    }

    private static bool? ResolveSceneReady(
        LiveExportSnapshot snapshot,
        CompanionNormalizedScene normalizedScene,
        string? blockingModal)
    {
        if (bool.TryParse(TryGetMeta(snapshot.Meta, "sceneReady"), out var sceneReady))
        {
            return sceneReady;
        }

        if (!string.IsNullOrWhiteSpace(blockingModal))
        {
            return false;
        }

        if (normalizedScene.SceneType is "unknown" or "startup" or "feedback-overlay" or "blocking-overlay")
        {
            return false;
        }

        if (normalizedScene.SceneType == "combat")
        {
            return snapshot.Encounter?.InCombat == true;
        }

        var visibleScreen = NormalizeSceneToken(TryGetMeta(snapshot.Meta, "visibleScreen"));
        if (normalizedScene.SceneType == "rewards" && visibleScreen == "map")
        {
            return false;
        }

        return true;
    }

    private static string? ResolveSceneAuthority(LiveExportSnapshot snapshot, CompanionNormalizedScene normalizedScene)
    {
        var explicitAuthority = TryGetMeta(snapshot.Meta, "sceneAuthority");
        if (!string.IsNullOrWhiteSpace(explicitAuthority))
        {
            return explicitAuthority;
        }

        var source = TryGetMeta(snapshot.Meta, "source");
        if (!string.IsNullOrWhiteSpace(source) && !string.Equals(source, "scene-poll", StringComparison.OrdinalIgnoreCase))
        {
            return "hook";
        }

        return normalizedScene.Source.StartsWith("meta:", StringComparison.OrdinalIgnoreCase)
            ? "mixed"
            : "polling";
    }

    private static string? ResolveSceneStability(
        LiveExportSnapshot snapshot,
        CompanionNormalizedScene normalizedScene,
        string? blockingModal)
    {
        var explicitStability = TryGetMeta(snapshot.Meta, "sceneStability");
        if (!string.IsNullOrWhiteSpace(explicitStability))
        {
            return explicitStability;
        }

        if (!string.IsNullOrWhiteSpace(blockingModal))
        {
            return "blocked";
        }

        if (normalizedScene.SceneType is "unknown" or "startup" or "feedback-overlay" or "blocking-overlay")
        {
            return "transient";
        }

        if (ResolveSceneReady(snapshot, normalizedScene, blockingModal) == true)
        {
            return "stable";
        }

        return "stabilizing";
    }

    private static string? TryGetMeta(IReadOnlyDictionary<string, string?> meta, string key)
    {
        return meta.TryGetValue(key, out var value) ? value : null;
    }

    private static string? NormalizeSceneToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed switch
        {
            "main-menu" or
            "singleplayer-submenu" or
            "character-select" or
            "map" or
            "rewards" or
            "event" or
            "shop" or
            "rest-site" or
            "combat" or
            "feedback-overlay" or
            "blocking-overlay" or
            "startup" or
            "unknown" => trimmed,
            _ => null,
        };
    }

    private static string BuildInventoryId(LiveExportSnapshot snapshot, string sceneType, IReadOnlyList<HarnessNodeInventoryItem> nodes)
    {
        var builder = new StringBuilder();
        builder.Append(snapshot.RunId);
        builder.Append('|');
        builder.Append(snapshot.CapturedAt.UtcDateTime.Ticks);
        builder.Append('|');
        builder.Append(sceneType);
        builder.Append('|');

        foreach (var node in nodes)
        {
            builder.Append(node.NodeId);
            builder.Append(':');
            builder.Append(node.Label);
            builder.Append(':');
            builder.Append(node.Actionable ? '1' : '0');
            builder.Append(':');
            builder.Append(node.ScreenBounds);
            builder.Append('|');
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
        return Convert.ToHexString(bytes[..8]).ToLowerInvariant();
    }

    private static string BuildFingerprint(HarnessNodeInventory inventory)
    {
        var builder = new StringBuilder();
        builder.Append(inventory.SceneType);
        builder.Append('|');
        builder.Append(inventory.Mode);
        builder.Append('|');
        builder.Append(inventory.BlockingModal);
        builder.Append('|');

        foreach (var node in inventory.Nodes)
        {
            builder.Append(node.NodeId);
            builder.Append(':');
            builder.Append(node.Label);
            builder.Append(':');
            builder.Append(node.Kind);
            builder.Append(':');
            builder.Append(node.Actionable ? '1' : '0');
            builder.Append(':');
            builder.Append(node.ScreenBounds);
            builder.Append('|');
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
        return Convert.ToHexString(bytes);
    }
}

internal sealed record InventoryPublishAttempt(
    bool Published,
    bool Suppressed,
    HarnessNodeInventory? Inventory,
    string? RawSceneType,
    string? SuppressionReason)
{
    public static InventoryPublishAttempt None { get; } = new(false, false, null, null, null);
}
