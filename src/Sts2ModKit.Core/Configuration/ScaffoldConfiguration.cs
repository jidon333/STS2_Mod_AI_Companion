using System.Text.Json.Serialization;

namespace Sts2ModKit.Core.Configuration;

public sealed record ScaffoldConfiguration
{
    public GamePathOptions GamePaths { get; init; } = GamePathOptions.CreateLocalDefault();

    public AiCompanionModOptions AiCompanionMod { get; init; } = AiCompanionModOptions.Defaults;

    public LiveExportOptions LiveExport { get; init; } = LiveExportOptions.Defaults;

    public AssistantOptions Assistant { get; init; } = AssistantOptions.Defaults;

    public static ScaffoldConfiguration CreateLocalDefault()
    {
        return new ScaffoldConfiguration();
    }

    public ScaffoldConfiguration With(PartialScaffoldConfiguration? partial)
    {
        if (partial is null)
        {
            return this;
        }

        return this with
        {
            GamePaths = GamePaths.With(partial.GamePaths),
            AiCompanionMod = AiCompanionMod.With(partial.AiCompanionMod),
            LiveExport = LiveExport.With(partial.LiveExport),
            Assistant = Assistant.With(partial.Assistant),
        };
    }
}

public sealed record PartialScaffoldConfiguration
{
    public PartialGamePathOptions? GamePaths { get; init; }

    public PartialAiCompanionModOptions? AiCompanionMod { get; init; }

    public PartialLiveExportOptions? LiveExport { get; init; }

    public PartialAssistantOptions? Assistant { get; init; }
}

public sealed record GamePathOptions
{
    public string GameDirectory { get; init; } = string.Empty;

    public string UserDataRoot { get; init; } = string.Empty;

    public string SteamAccountId { get; init; } = string.Empty;

    public int ProfileIndex { get; init; } = 1;

    public string ArtifactsRoot { get; init; } = "artifacts";

    public static GamePathOptions CreateLocalDefault()
    {
        return new GamePathOptions
        {
            GameDirectory = @"D:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2",
            UserDataRoot = @"C:\Users\jidon\AppData\Roaming\SlayTheSpire2",
            SteamAccountId = "76561198206882255",
            ProfileIndex = 1,
            ArtifactsRoot = "artifacts",
        };
    }

    public GamePathOptions With(PartialGamePathOptions? partial)
    {
        if (partial is null)
        {
            return this;
        }

        return this with
        {
            GameDirectory = partial.GameDirectory ?? GameDirectory,
            UserDataRoot = partial.UserDataRoot ?? UserDataRoot,
            SteamAccountId = partial.SteamAccountId ?? SteamAccountId,
            ProfileIndex = partial.ProfileIndex ?? ProfileIndex,
            ArtifactsRoot = partial.ArtifactsRoot ?? ArtifactsRoot,
        };
    }
}

public sealed record PartialGamePathOptions
{
    public string? GameDirectory { get; init; }

    public string? UserDataRoot { get; init; }

    public string? SteamAccountId { get; init; }

    public int? ProfileIndex { get; init; }

    public string? ArtifactsRoot { get; init; }
}

public sealed record AiCompanionModOptions
{
    public string Name { get; init; } = "STS2 Mod AI Companion";

    public string Author { get; init; } = "Your Name";

    public string Version { get; init; } = "0.1.0";

    public string Description { get; init; } = "AI companion mod scaffold for Slay the Spire 2. Phase 1 is an external coaching assistant. Long-term goal is an AI teammate for multiplayer.";

    public string PckName { get; init; } = "sts2-mod-ai-companion.pck";

    public string PackageFolderName { get; init; } = "Sts2ModAiCompanion";

    [JsonIgnore]
    public string RuntimeAssemblyFileName => "Sts2ModAiCompanion.Mod.dll";

    [JsonIgnore]
    public string RuntimeConfigFileName => "sts2-mod-ai-companion.config.json";

    [JsonIgnore]
    public string RuntimeLogFileName => "sts2-mod-ai-companion.runtime.log";

    public static AiCompanionModOptions Defaults { get; } = new();

    public AiCompanionModOptions With(PartialAiCompanionModOptions? partial)
    {
        if (partial is null)
        {
            return this;
        }

        return this with
        {
            Name = partial.Name ?? Name,
            Author = partial.Author ?? Author,
            Version = partial.Version ?? Version,
            Description = partial.Description ?? Description,
            PckName = partial.PckName ?? PckName,
            PackageFolderName = partial.PackageFolderName ?? PackageFolderName,
        };
    }
}

public sealed record PartialAiCompanionModOptions
{
    public string? Name { get; init; }

    public string? Author { get; init; }

    public string? Version { get; init; }

    public string? Description { get; init; }

    public string? PckName { get; init; }

    public string? PackageFolderName { get; init; }
}

public sealed record LiveExportOptions
{
    public bool Enabled { get; init; } = true;

    public bool DiscoveryMode { get; init; } = true;

    public bool CollectorModeEnabled { get; init; } = false;

    public int CollectorMaxRawEvents { get; init; } = 4096;

    public bool CollectorKeepSemanticSnapshots { get; init; } = true;

    public bool ScenePollingEnabled { get; init; } = true;

    public int ScenePollingIntervalMs { get; init; } = 250;

    public int ScenePollingMaxNodes { get; init; } = 256;

    public int DuplicateSuppressionMs { get; init; } = 250;

    public int SnapshotWriteThrottleMs { get; init; } = 150;

    public int MaxRecentChanges { get; init; } = 16;

    public int MaxDeckEntries { get; init; } = 40;

    public int MaxChoiceEntries { get; init; } = 10;

    public string RelativeLiveRoot { get; init; } = "ai_companion/live";

    public string EventsFileName { get; init; } = "events.ndjson";

    public string SnapshotFileName { get; init; } = "state.latest.json";

    public string SummaryFileName { get; init; } = "state.latest.txt";

    public string SessionFileName { get; init; } = "session.json";

    public string RawObservationsFileName { get; init; } = "raw-observations.ndjson";

    public string ScreenTransitionsFileName { get; init; } = "screen-transitions.ndjson";

    public string ChoiceCandidatesFileName { get; init; } = "choice-candidates.ndjson";

    public string ChoiceDecisionsFileName { get; init; } = "choice-decisions.ndjson";

    public string SemanticSnapshotsFolderName { get; init; } = "semantic-snapshots";

    public static LiveExportOptions Defaults { get; } = new();

    public LiveExportOptions With(PartialLiveExportOptions? partial)
    {
        if (partial is null)
        {
            return this;
        }

        return this with
        {
            Enabled = partial.Enabled ?? Enabled,
            DiscoveryMode = partial.DiscoveryMode ?? DiscoveryMode,
            CollectorModeEnabled = partial.CollectorModeEnabled ?? CollectorModeEnabled,
            CollectorMaxRawEvents = partial.CollectorMaxRawEvents ?? CollectorMaxRawEvents,
            CollectorKeepSemanticSnapshots = partial.CollectorKeepSemanticSnapshots ?? CollectorKeepSemanticSnapshots,
            ScenePollingEnabled = partial.ScenePollingEnabled ?? ScenePollingEnabled,
            ScenePollingIntervalMs = partial.ScenePollingIntervalMs ?? ScenePollingIntervalMs,
            ScenePollingMaxNodes = partial.ScenePollingMaxNodes ?? ScenePollingMaxNodes,
            DuplicateSuppressionMs = partial.DuplicateSuppressionMs ?? DuplicateSuppressionMs,
            SnapshotWriteThrottleMs = partial.SnapshotWriteThrottleMs ?? SnapshotWriteThrottleMs,
            MaxRecentChanges = partial.MaxRecentChanges ?? MaxRecentChanges,
            MaxDeckEntries = partial.MaxDeckEntries ?? MaxDeckEntries,
            MaxChoiceEntries = partial.MaxChoiceEntries ?? MaxChoiceEntries,
            RelativeLiveRoot = partial.RelativeLiveRoot ?? RelativeLiveRoot,
            EventsFileName = partial.EventsFileName ?? EventsFileName,
            SnapshotFileName = partial.SnapshotFileName ?? SnapshotFileName,
            SummaryFileName = partial.SummaryFileName ?? SummaryFileName,
            SessionFileName = partial.SessionFileName ?? SessionFileName,
            RawObservationsFileName = partial.RawObservationsFileName ?? RawObservationsFileName,
            ScreenTransitionsFileName = partial.ScreenTransitionsFileName ?? ScreenTransitionsFileName,
            ChoiceCandidatesFileName = partial.ChoiceCandidatesFileName ?? ChoiceCandidatesFileName,
            ChoiceDecisionsFileName = partial.ChoiceDecisionsFileName ?? ChoiceDecisionsFileName,
            SemanticSnapshotsFolderName = partial.SemanticSnapshotsFolderName ?? SemanticSnapshotsFolderName,
        };
    }
}

public sealed record PartialLiveExportOptions
{
    public bool? Enabled { get; init; }

    public bool? DiscoveryMode { get; init; }

    public bool? CollectorModeEnabled { get; init; }

    public int? CollectorMaxRawEvents { get; init; }

    public bool? CollectorKeepSemanticSnapshots { get; init; }

    public bool? ScenePollingEnabled { get; init; }

    public int? ScenePollingIntervalMs { get; init; }

    public int? ScenePollingMaxNodes { get; init; }

    public int? DuplicateSuppressionMs { get; init; }

    public int? SnapshotWriteThrottleMs { get; init; }

    public int? MaxRecentChanges { get; init; }

    public int? MaxDeckEntries { get; init; }

    public int? MaxChoiceEntries { get; init; }

    public string? RelativeLiveRoot { get; init; }

    public string? EventsFileName { get; init; }

    public string? SnapshotFileName { get; init; }

    public string? SummaryFileName { get; init; }

    public string? SessionFileName { get; init; }

    public string? RawObservationsFileName { get; init; }

    public string? ScreenTransitionsFileName { get; init; }

    public string? ChoiceCandidatesFileName { get; init; }

    public string? ChoiceDecisionsFileName { get; init; }

    public string? SemanticSnapshotsFolderName { get; init; }
}

public sealed record AssistantOptions
{
    public string CodexCommand { get; init; } = "codex";

    public string? OptionalGodotExe { get; init; }

    public bool AutoAdviceEnabled { get; init; } = true;

    public int LivePollIntervalMs { get; init; } = 750;

    public int StateDebounceMs { get; init; } = 300;

    public int MinAdviceIntervalMs { get; init; } = 2000;

    public int MaxKnowledgeEntries { get; init; } = 20;

    public int MaxKnowledgeBytes { get; init; } = 12288;

    public int RecentEventsCount { get; init; } = 20;

    public string CompanionArtifactsRelativeRoot { get; init; } = "companion";

    public static AssistantOptions Defaults { get; } = new();

    public AssistantOptions With(PartialAssistantOptions? partial)
    {
        if (partial is null)
        {
            return this;
        }

        return this with
        {
            CodexCommand = partial.CodexCommand ?? CodexCommand,
            OptionalGodotExe = partial.OptionalGodotExe ?? OptionalGodotExe,
            AutoAdviceEnabled = partial.AutoAdviceEnabled ?? AutoAdviceEnabled,
            LivePollIntervalMs = partial.LivePollIntervalMs ?? LivePollIntervalMs,
            StateDebounceMs = partial.StateDebounceMs ?? StateDebounceMs,
            MinAdviceIntervalMs = partial.MinAdviceIntervalMs ?? MinAdviceIntervalMs,
            MaxKnowledgeEntries = partial.MaxKnowledgeEntries ?? MaxKnowledgeEntries,
            MaxKnowledgeBytes = partial.MaxKnowledgeBytes ?? MaxKnowledgeBytes,
            RecentEventsCount = partial.RecentEventsCount ?? RecentEventsCount,
            CompanionArtifactsRelativeRoot = partial.CompanionArtifactsRelativeRoot ?? CompanionArtifactsRelativeRoot,
        };
    }
}

public sealed record PartialAssistantOptions
{
    public string? CodexCommand { get; init; }

    public string? OptionalGodotExe { get; init; }

    public bool? AutoAdviceEnabled { get; init; }

    public int? LivePollIntervalMs { get; init; }

    public int? StateDebounceMs { get; init; }

    public int? MinAdviceIntervalMs { get; init; }

    public int? MaxKnowledgeEntries { get; init; }

    public int? MaxKnowledgeBytes { get; init; }

    public int? RecentEventsCount { get; init; }

    public string? CompanionArtifactsRelativeRoot { get; init; }
}

public sealed record ConfigurationLoadResult
{
    public ScaffoldConfiguration Configuration { get; init; } = ScaffoldConfiguration.CreateLocalDefault();

    public string ConfigurationSource { get; init; } = "<defaults>";

    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}
