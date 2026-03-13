using System.Security.Cryptography;
using System.Text;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModAiCompanion.HarnessBridge;

internal sealed class InventoryPublisher
{
    private readonly string _inventoryPath;
    private readonly string _liveSnapshotPath;
    private string? _lastFingerprint;

    public InventoryPublisher(string inventoryPath, string liveSnapshotPath)
    {
        _inventoryPath = inventoryPath;
        _liveSnapshotPath = liveSnapshotPath;
    }

    public bool TryPublish(LiveSnapshotReader snapshotReader, string mode, out HarnessNodeInventory inventory)
    {
        inventory = default!;
        var snapshot = snapshotReader.TryRead(_liveSnapshotPath);
        if (snapshot is null)
        {
            return false;
        }

        inventory = BuildInventory(snapshot, mode);
        var fingerprint = BuildFingerprint(inventory);
        if (string.Equals(_lastFingerprint, fingerprint, StringComparison.Ordinal))
        {
            return false;
        }

        LiveExportAtomicFileWriter.WriteJsonAtomic(_inventoryPath, inventory);
        _lastFingerprint = fingerprint;
        return true;
    }

    private static HarnessNodeInventory BuildInventory(LiveExportSnapshot snapshot, string mode)
    {
        var sceneType = NormalizeSceneType(snapshot);
        var blockingModal = TryGetBlockingModal(snapshot.Meta);
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
            Nodes: nodes);
    }

    private static HarnessNodeInventoryItem BuildNode(string sceneType, LiveExportChoiceSummary choice, int index)
    {
        var label = choice.Label?.Trim() ?? string.Empty;
        var kind = ResolveKind(sceneType, choice);
        var actionable = !string.IsNullOrWhiteSpace(label) && !IsOverlayChoice(label);
        var hints = BuildHints(sceneType, choice);

        return new HarnessNodeInventoryItem(
            NodeId: $"{kind}:{index}",
            Kind: kind,
            Label: label,
            Description: choice.Description,
            TypeName: string.IsNullOrWhiteSpace(choice.Kind) ? "choice" : choice.Kind,
            UiPath: null,
            Visible: true,
            Enabled: null,
            Actionable: actionable,
            ScreenBounds: null,
            SemanticHints: hints);
    }

    private static string ResolveKind(string sceneType, LiveExportChoiceSummary choice)
    {
        if (!string.IsNullOrWhiteSpace(choice.Kind))
        {
            return choice.Kind.Trim().ToLowerInvariant();
        }

        return sceneType switch
        {
            "character-select" => "character",
            "singleplayer-submenu" => "mode-option",
            "map" => "map-node",
            "rewards" => "reward-item",
            "event" => "event-option",
            "shop" => "shop-option",
            "rest-site" => "rest-option",
            _ => "choice",
        };
    }

    private static IReadOnlyList<string> BuildHints(string sceneType, LiveExportChoiceSummary choice)
    {
        var hints = new List<string>(capacity: 4)
        {
            $"scene:{sceneType}",
            $"kind:{ResolveKind(sceneType, choice)}",
        };

        if (!string.IsNullOrWhiteSpace(choice.Value))
        {
            hints.Add($"value:{choice.Value.Trim()}");
        }

        if (IsOverlayChoice(choice.Label))
        {
            hints.Add("overlay");
        }

        return hints;
    }

    private static string NormalizeSceneType(LiveExportSnapshot snapshot)
    {
        if (bool.TryParse(TryGetMeta(snapshot.Meta, "modal-blocking"), out var blocking) && blocking)
        {
            return "blocking-overlay";
        }

        var currentScreen = snapshot.CurrentScreen?.Trim();
        return string.IsNullOrWhiteSpace(currentScreen) ? "unknown" : currentScreen;
    }

    private static string? TryGetBlockingModal(IReadOnlyDictionary<string, string?> meta)
    {
        if (!bool.TryParse(TryGetMeta(meta, "modal-blocking"), out var blocking) || !blocking)
        {
            return null;
        }

        return TryGetMeta(meta, "modal-type") ?? "blocking";
    }

    private static string? TryGetMeta(IReadOnlyDictionary<string, string?> meta, string key)
    {
        return meta.TryGetValue(key, out var value) ? value : null;
    }

    private static bool IsOverlayChoice(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        var normalized = label.Trim();
        return normalized.Equals("Dismisser", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("Exclaim", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("Question", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("BackButton", StringComparison.OrdinalIgnoreCase)
               || normalized.Equals("Send!", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildInventoryId(LiveExportSnapshot snapshot, string sceneType, IReadOnlyList<HarnessNodeInventoryItem> nodes)
    {
        var builder = new StringBuilder();
        builder.Append(snapshot.RunId);
        builder.Append('|');
        builder.Append(snapshot.CapturedAt.UtcTicks);
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
            builder.Append('|');
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
        return Convert.ToHexString(bytes);
    }
}
