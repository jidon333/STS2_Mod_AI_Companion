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
    IReadOnlyList<HarnessNodeInventoryItem> Nodes);

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
