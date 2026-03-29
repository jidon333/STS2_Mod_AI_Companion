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
        if (ShouldSuppressPublish(inventory))
        {
            return new InventoryPublishAttempt(
                Published: false,
                Suppressed: true,
                Inventory: inventory,
                RawSceneType: string.IsNullOrWhiteSpace(inventory.RawSceneType) ? "unknown" : inventory.RawSceneType,
                SuppressionReason: "scene-not-yet-stable");
        }

        var fingerprint = BuildFingerprint(inventory);
        if (string.Equals(_lastFingerprint, fingerprint, StringComparison.Ordinal))
        {
            return new InventoryPublishAttempt(
                Published: false,
                Suppressed: false,
                Inventory: inventory,
                RawSceneType: string.IsNullOrWhiteSpace(inventory.RawSceneType) ? "unknown" : inventory.RawSceneType,
                SuppressionReason: null);
        }

        LiveExportAtomicFileWriter.WriteJsonAtomic(_inventoryPath, inventory);
        _lastFingerprint = fingerprint;
        return new InventoryPublishAttempt(
            Published: true,
            Suppressed: false,
            Inventory: inventory,
            RawSceneType: string.IsNullOrWhiteSpace(inventory.RawSceneType) ? "unknown" : inventory.RawSceneType,
            SuppressionReason: null);
    }

    private bool ShouldSuppressPublish(HarnessNodeInventory inventory)
    {
        var sceneType = inventory.SceneType;
        if (string.Equals(_lastObservedSceneType, sceneType, StringComparison.OrdinalIgnoreCase))
        {
            _lastObservedSceneStreak += 1;
        }
        else
        {
            _lastObservedSceneType = sceneType;
            _lastObservedSceneStreak = 1;
        }

        if (CompanionSceneNormalizer.IsStableSceneForImmediateInventoryPublish(sceneType)
            && !HasExplicitCompatibilityInstability(inventory)
            && !HasPrimaryScreenDisagreement(inventory))
        {
            return false;
        }

        return _lastObservedSceneStreak < 2;
    }

    private static HarnessNodeInventory BuildInventory(LiveExportSnapshot snapshot, string mode, CompanionNormalizedScene normalizedScene)
    {
        var rawSceneType = ResolveRawSceneType(snapshot, normalizedScene);
        var publishedSceneType = ResolvePublishedSceneType(snapshot, normalizedScene);
        var compatibilitySceneType = ResolveCompatibilitySceneType(snapshot, normalizedScene, publishedSceneType, rawSceneType);
        var blockingModal = ResolveBlockingModal(snapshot, normalizedScene);
        var publishedVisibleScene = ResolvePublishedVisibleScene(snapshot, normalizedScene, publishedSceneType);
        var publishedSceneReady = ResolvePublishedSceneReady(snapshot, blockingModal);
        var publishedSceneAuthority = ResolvePublishedSceneAuthority(snapshot);
        var publishedSceneStability = ResolvePublishedSceneStability(snapshot, blockingModal);
        var compatibilityVisibleScene = ResolveCompatibilityVisibleScene(snapshot, normalizedScene);
        var compatibilitySceneReady = ResolveSceneReady(snapshot, normalizedScene, blockingModal);
        var compatibilitySceneAuthority = ResolveSceneAuthority(snapshot, normalizedScene);
        var compatibilitySceneStability = ResolveSceneStability(snapshot, normalizedScene, blockingModal);
        var sceneReady = publishedSceneReady ?? compatibilitySceneReady;
        var sceneAuthority = publishedSceneAuthority ?? compatibilitySceneAuthority;
        var sceneStability = publishedSceneStability ?? compatibilitySceneStability;
        var nodes = snapshot.CurrentChoices
            .Select((choice, index) => BuildNode(compatibilitySceneType, rawSceneType, publishedSceneType, choice, index))
            .ToArray();

        return new HarnessNodeInventory(
            InventoryId: BuildInventoryId(snapshot, compatibilitySceneType, nodes),
            CapturedAt: snapshot.CapturedAt,
            RunId: string.IsNullOrWhiteSpace(snapshot.RunId) ? null : snapshot.RunId,
            SceneType: compatibilitySceneType,
            SceneEpisodeId: TryGetMeta(snapshot.Meta, "screen-episode"),
            Mode: mode,
            BlockingModal: blockingModal,
            SceneReady: sceneReady,
            SceneAuthority: sceneAuthority,
            SceneStability: sceneStability,
            Nodes: nodes)
        {
            RawSceneType = rawSceneType,
            RawCurrentScreen = rawSceneType,
            PublishedSceneType = publishedSceneType,
            PublishedCurrentScreen = publishedSceneType,
            PublishedVisibleScene = publishedVisibleScene,
            PublishedVisibleScreen = publishedVisibleScene,
            PublishedSceneReady = publishedSceneReady,
            PublishedSceneAuthority = publishedSceneAuthority,
            PublishedSceneStability = publishedSceneStability,
            CompatibilitySceneType = compatibilitySceneType,
            CompatibilityLogicalScreen = compatibilitySceneType,
            CompatibilityCurrentScreen = compatibilitySceneType,
            CompatibilityVisibleScene = compatibilityVisibleScene,
            CompatibilityVisibleScreen = compatibilityVisibleScene,
            CompatibilitySceneReady = compatibilitySceneReady,
            CompatibilitySceneAuthority = compatibilitySceneAuthority,
            CompatibilitySceneStability = compatibilitySceneStability,
        };
    }

    private static string ResolveRawSceneType(LiveExportSnapshot snapshot, CompanionNormalizedScene normalizedScene)
    {
        return NormalizeSceneToken(snapshot.RawCurrentScreen)
               ?? NormalizeSceneToken(snapshot.RawObservedScreen)
               ?? NormalizeSceneToken(TryGetMeta(snapshot.Meta, "rawObservedScreen"))
               ?? NormalizeSceneToken(TryGetMeta(snapshot.Meta, "rawCurrentScreen"))
               ?? NormalizeSceneToken(TryGetMeta(snapshot.Meta, "screen"))
               ?? NormalizeSceneToken(snapshot.CurrentScreen)
               ?? normalizedScene.SceneType;
    }

    private static string ResolvePublishedSceneType(LiveExportSnapshot snapshot, CompanionNormalizedScene normalizedScene)
    {
        return NormalizeSceneToken(snapshot.PublishedCurrentScreen)
               ?? NormalizeSceneToken(TryGetMeta(snapshot.Meta, "publishedCurrentScreen"))
               ?? NormalizeSceneToken(snapshot.CurrentScreen)
               ?? NormalizeSceneToken(TryGetMeta(snapshot.Meta, "logicalScreen"))
               ?? normalizedScene.SceneType;
    }

    private static string ResolvePublishedVisibleScene(LiveExportSnapshot snapshot, CompanionNormalizedScene normalizedScene, string publishedSceneType)
    {
        return NormalizeSceneToken(snapshot.PublishedVisibleScreen)
               ?? NormalizeSceneToken(TryGetMeta(snapshot.Meta, "publishedVisibleScreen"))
               ?? NormalizeSceneToken(TryGetMeta(snapshot.Meta, "visibleScreen"))
               ?? publishedSceneType
               ?? normalizedScene.SceneType;
    }

    private static bool? ResolvePublishedSceneReady(
        LiveExportSnapshot snapshot,
        string? blockingModal)
    {
        if (snapshot.PublishedSceneReady is not null)
        {
            return snapshot.PublishedSceneReady;
        }

        if (bool.TryParse(TryGetMeta(snapshot.Meta, "publishedSceneReady"), out var explicitPublishedSceneReady))
        {
            return explicitPublishedSceneReady;
        }

        if (bool.TryParse(TryGetMeta(snapshot.Meta, "sceneReady"), out var publishedSceneReady))
        {
            return publishedSceneReady;
        }

        if (!string.IsNullOrWhiteSpace(blockingModal))
        {
            return false;
        }

        return null;
    }

    private static string? ResolvePublishedSceneAuthority(LiveExportSnapshot snapshot)
    {
        return snapshot.PublishedSceneAuthority
               ?? TryGetMeta(snapshot.Meta, "publishedSceneAuthority")
               ?? TryGetMeta(snapshot.Meta, "sceneAuthority");
    }

    private static string? ResolvePublishedSceneStability(
        LiveExportSnapshot snapshot,
        string? blockingModal)
    {
        var publishedSceneStability = snapshot.PublishedSceneStability
                                      ?? TryGetMeta(snapshot.Meta, "publishedSceneStability")
                                      ?? TryGetMeta(snapshot.Meta, "sceneStability");
        if (!string.IsNullOrWhiteSpace(publishedSceneStability))
        {
            return publishedSceneStability;
        }

        if (!string.IsNullOrWhiteSpace(blockingModal))
        {
            return "blocked";
        }

        return null;
    }

    private static string ResolveCompatibilitySceneType(
        LiveExportSnapshot snapshot,
        CompanionNormalizedScene normalizedScene,
        string publishedSceneType,
        string rawSceneType)
    {
        var logicalScreen = NormalizeSceneToken(snapshot.CompatibilityLogicalScreen)
            ?? NormalizeSceneToken(TryGetMeta(snapshot.Meta, "compatLogicalScreen"))
            ?? publishedSceneType
            ?? rawSceneType
            ?? normalizedScene.SceneType;

        if (logicalScreen is not null)
        {
            return logicalScreen;
        }

        return publishedSceneType
               ?? rawSceneType
               ?? normalizedScene.SceneType;
    }

    private static HarnessNodeInventoryItem BuildNode(string compatibilitySceneType, string rawSceneType, string publishedSceneType, LiveExportChoiceSummary choice, int index)
    {
        var label = choice.Label?.Trim() ?? string.Empty;
        var kind = ResolveKind(compatibilitySceneType, choice);
        var actionable = !string.IsNullOrWhiteSpace(label) && !CompanionSceneNormalizer.IsOverlayChoice(label);
        var hints = BuildHints(compatibilitySceneType, rawSceneType, publishedSceneType, choice, kind);

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
            "shop" => ResolveShopKind(choice),
            "rest-site" => "rest-option",
            _ => NormalizeChoiceKind(choice.Kind),
        };
    }

    private static IReadOnlyList<string> BuildHints(string compatibilitySceneType, string rawSceneType, string publishedSceneType, LiveExportChoiceSummary choice, string resolvedKind)
    {
        var hints = new List<string>(capacity: 8)
        {
            $"scene:{compatibilitySceneType}",
            $"scene-compat:{compatibilitySceneType}",
            $"kind:{resolvedKind}",
        };

        if (!string.IsNullOrWhiteSpace(rawSceneType))
        {
            hints.Add($"scene-raw:{rawSceneType}");
        }

        if (!string.IsNullOrWhiteSpace(publishedSceneType))
        {
            hints.Add($"scene-published:{publishedSceneType}");
        }

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

    private static string ResolveShopKind(LiveExportChoiceSummary choice)
    {
        var rawKind = NormalizeChoiceKind(choice.Kind);
        if (rawKind.StartsWith("shop-", StringComparison.OrdinalIgnoreCase)
            || rawKind.StartsWith("shop-option:", StringComparison.OrdinalIgnoreCase))
        {
            return rawKind;
        }

        return rawKind;
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
        if (snapshot.CompatibilitySceneReady is { } compatibilitySceneReady)
        {
            return compatibilitySceneReady;
        }

        if (bool.TryParse(TryGetMeta(snapshot.Meta, "compatSceneReady"), out compatibilitySceneReady))
        {
            return compatibilitySceneReady;
        }

        if (!string.IsNullOrWhiteSpace(blockingModal))
        {
            return false;
        }

        return null;
    }

    private static string? ResolveSceneAuthority(LiveExportSnapshot snapshot, CompanionNormalizedScene normalizedScene)
    {
        if (!string.IsNullOrWhiteSpace(snapshot.CompatibilitySceneAuthority))
        {
            return snapshot.CompatibilitySceneAuthority;
        }

        var compatibilityAuthority = TryGetMeta(snapshot.Meta, "compatSceneAuthority");
        if (!string.IsNullOrWhiteSpace(compatibilityAuthority))
        {
            return compatibilityAuthority;
        }

        return null;
    }

    private static string? ResolveSceneStability(
        LiveExportSnapshot snapshot,
        CompanionNormalizedScene normalizedScene,
        string? blockingModal)
    {
        if (!string.IsNullOrWhiteSpace(snapshot.CompatibilitySceneStability))
        {
            return snapshot.CompatibilitySceneStability;
        }

        var compatibilityStability = TryGetMeta(snapshot.Meta, "compatSceneStability");
        if (!string.IsNullOrWhiteSpace(compatibilityStability))
        {
            return compatibilityStability;
        }

        if (!string.IsNullOrWhiteSpace(blockingModal))
        {
            return "blocked";
        }

        return null;
    }

    private static string? ResolveCompatibilityVisibleScene(LiveExportSnapshot snapshot, CompanionNormalizedScene normalizedScene)
    {
        return NormalizeSceneToken(snapshot.CompatibilityVisibleScreen)
               ?? NormalizeSceneToken(TryGetMeta(snapshot.Meta, "compatVisibleScreen"))
               ?? NormalizeSceneToken(snapshot.CompatibilityLogicalScreen)
               ?? NormalizeSceneToken(TryGetMeta(snapshot.Meta, "compatLogicalScreen"))
               ?? NormalizeSceneToken(TryGetMeta(snapshot.Meta, "visibleScreen"))
               ?? NormalizeSceneToken(snapshot.CurrentScreen)
               ?? normalizedScene.SceneType;
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
        builder.Append(inventory.RawSceneType);
        builder.Append('|');
        builder.Append(inventory.PublishedSceneType);
        builder.Append('|');
        builder.Append(inventory.PublishedVisibleScene);
        builder.Append('|');
        builder.Append(inventory.PublishedSceneReady);
        builder.Append('|');
        builder.Append(inventory.PublishedSceneAuthority);
        builder.Append('|');
        builder.Append(inventory.PublishedSceneStability);
        builder.Append('|');
        builder.Append(inventory.CompatibilityLogicalScreen);
        builder.Append('|');
        builder.Append(inventory.CompatibilityVisibleScene);
        builder.Append('|');
        builder.Append(inventory.CompatibilitySceneReady);
        builder.Append('|');
        builder.Append(inventory.CompatibilitySceneAuthority);
        builder.Append('|');
        builder.Append(inventory.CompatibilitySceneStability);
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

    private static bool HasExplicitCompatibilityInstability(HarnessNodeInventory inventory)
    {
        return inventory.CompatibilitySceneReady == false
               || (!string.IsNullOrWhiteSpace(inventory.CompatibilitySceneStability)
                   && !string.Equals(inventory.CompatibilitySceneStability, "stable", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasPrimaryScreenDisagreement(HarnessNodeInventory inventory)
    {
        var sceneType = inventory.SceneType;
        var rawSceneType = inventory.RawSceneType;
        var publishedSceneType = inventory.PublishedSceneType;
        var compatibilityVisibleScene = inventory.CompatibilityVisibleScene;
        return !string.IsNullOrWhiteSpace(rawSceneType)
               && !string.Equals(rawSceneType, sceneType, StringComparison.OrdinalIgnoreCase)
               || !string.IsNullOrWhiteSpace(publishedSceneType)
               && !string.Equals(publishedSceneType, sceneType, StringComparison.OrdinalIgnoreCase)
               || !string.IsNullOrWhiteSpace(compatibilityVisibleScene)
               && !string.Equals(compatibilityVisibleScene, sceneType, StringComparison.OrdinalIgnoreCase);
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
