namespace Sts2ModKit.Core.Harness;

public sealed record HarnessArmSession(
    string SessionToken,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    string Reason);

public sealed record HarnessNodeInventory(
    string InventoryId,
    DateTimeOffset CapturedAt,
    string? RunId,
    string SceneType,
    string? SceneEpisodeId,
    string Mode,
    string? BlockingModal,
    bool? SceneReady,
    string? SceneAuthority,
    string? SceneStability,
    IReadOnlyList<HarnessNodeInventoryItem> Nodes)
{
    public string? RawSceneType { get; init; }

    public string? CompatibilitySceneType { get; init; }

    public string? CompatibilityVisibleScene { get; init; }

    public bool? CompatibilitySceneReady { get; init; }

    public string? CompatibilitySceneAuthority { get; init; }

    public string? CompatibilitySceneStability { get; init; }
}

public sealed record HarnessNodeInventoryItem(
    string NodeId,
    string Kind,
    string Label,
    string? Description,
    string TypeName,
    string? UiPath,
    bool Visible,
    bool? Enabled,
    bool Actionable,
    string? ScreenBounds,
    IReadOnlyList<string> SemanticHints);
