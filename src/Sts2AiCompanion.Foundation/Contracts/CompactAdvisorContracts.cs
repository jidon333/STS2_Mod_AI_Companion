namespace Sts2AiCompanion.Foundation.Contracts;

public sealed record CompactRunContext(
    int? Act,
    int? Floor,
    int? CurrentHp,
    int? MaxHp,
    int? Gold,
    int RelicCount,
    int PotionCount,
    int DeckCount);

public sealed record CompactPlayerSummary(
    string DeckSummary,
    IReadOnlyList<string> KeyRelics,
    IReadOnlyList<string> KeyPotions);

public sealed record CompactAdvisorOption(
    string Kind,
    string Label,
    string? Value,
    string? Description,
    bool Enabled,
    IReadOnlyList<string> Tags);

public sealed record CompactKnowledgeEntry(
    string Id,
    string Name,
    string? Summary);

public sealed record CompactRecentEvent(
    string Kind,
    string Screen,
    int? Act,
    int? Floor,
    string? Summary);

public sealed record RewardCompactFacts(
    string? RewardType,
    bool SkipAllowed,
    IReadOnlyList<string> FactLines,
    IReadOnlyList<string> MissingInformation,
    IReadOnlyList<string> KnowledgeRefs);

public sealed record EventCompactEffect(
    string Kind,
    int? Amount,
    string Text);

public sealed record EventCompactOptionFact(
    string Label,
    string? Value,
    bool Enabled,
    IReadOnlyList<EventCompactEffect> Effects,
    IReadOnlyList<string> MissingInformation);

public sealed record EventCompactFacts(
    string? EventId,
    bool EventIdentityMissing,
    bool RewardChildActive,
    bool ProceedVisible,
    IReadOnlyList<EventCompactOptionFact> OptionFacts,
    IReadOnlyList<string> MissingInformation);

public sealed record RewardEventCompactAdvisorInput(
    string SceneType,
    string SceneStage,
    string CanonicalOwner,
    CompactRunContext RunContext,
    CompactPlayerSummary PlayerSummary,
    IReadOnlyList<CompactAdvisorOption> VisibleOptions,
    IReadOnlyList<CompactKnowledgeEntry> KnowledgeEntries,
    IReadOnlyList<CompactRecentEvent> RecentEvents,
    IReadOnlyList<string> MissingInformation,
    IReadOnlyList<string> DecisionBlockers,
    RewardCompactFacts? RewardFacts = null,
    EventCompactFacts? EventFacts = null);

public sealed record CompactAdvisorBuildResult(
    bool Supported,
    RewardEventCompactAdvisorInput? CompactInput,
    IReadOnlyList<string> MissingInformation,
    IReadOnlyList<string> DecisionBlockers,
    string ReasonCode);
