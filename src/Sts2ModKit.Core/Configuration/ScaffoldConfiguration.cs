using System.Text.Json.Serialization;

namespace Sts2ModKit.Core.Configuration;

public sealed record ScaffoldConfiguration
{
    public GamePathOptions GamePaths { get; init; } = GamePathOptions.CreateLocalDefault();

    public AiCompanionModOptions AiCompanionMod { get; init; } = AiCompanionModOptions.Defaults;

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
        };
    }
}

public sealed record PartialScaffoldConfiguration
{
    public PartialGamePathOptions? GamePaths { get; init; }

    public PartialAiCompanionModOptions? AiCompanionMod { get; init; }
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

public sealed record ConfigurationLoadResult
{
    public ScaffoldConfiguration Configuration { get; init; } = ScaffoldConfiguration.CreateLocalDefault();

    public string ConfigurationSource { get; init; } = "<defaults>";

    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}
