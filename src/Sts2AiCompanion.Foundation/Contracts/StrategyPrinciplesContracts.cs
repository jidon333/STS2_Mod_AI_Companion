namespace Sts2AiCompanion.Foundation.Contracts;

public sealed record StrategyPrincipleEntry(
    string Id,
    string Title,
    string Summary,
    string TransferConfidence);

public sealed record AdvicePerspectiveView(
    string Headline,
    string? RecommendedChoiceLabel,
    string Summary,
    IReadOnlyList<string> ReasoningBullets,
    IReadOnlyList<string> RiskNotes);
