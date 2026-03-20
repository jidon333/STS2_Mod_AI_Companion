namespace Sts2AiCompanion.Foundation.Contracts;

public sealed record RewardOptionSet(
    string SceneType,
    bool SkipAllowed,
    string SummaryText,
    IReadOnlyList<RewardOption> Options);

public sealed record RewardOption(
    int Ordinal,
    string Kind,
    string Label,
    string? Value,
    string? Description,
    bool IsSkipOption);

public sealed record RewardAssessmentFacts(
    string KnowledgeFingerprint,
    int DeckSize,
    int AttackCount,
    int SkillCount,
    int PowerCount,
    int DrawTaggedCardCount,
    int BlockTaggedCardCount,
    int EnergyTaggedCardCount,
    string AttackPressure,
    string DefensePressure,
    string DrawSupportLevel,
    string EnergySupportLevel,
    IReadOnlyList<string> SynergyHints,
    IReadOnlyList<string> AntiSynergyHints,
    IReadOnlyList<string> MissingInformation,
    IReadOnlyList<string> FactLines,
    IReadOnlyList<string> KnowledgeRefs);

public sealed record RewardRecommendationTrace(
    string KnowledgeFingerprint,
    IReadOnlyList<string> CandidateLabels,
    IReadOnlyList<string> AssessmentFactLines,
    IReadOnlyList<string> InputKnowledgeRefs,
    IReadOnlyList<string> MissingInformation);
