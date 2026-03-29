using System.Text.Json;
using Sts2ModKit.Core.Configuration;

namespace Sts2ModAiCompanion.Mod.Runtime;

public sealed record AiCompanionStartupSentinelConfig
{
    public string? SessionId { get; init; }

    public string? RunId { get; init; }

    public string? LaunchToken { get; init; }

    public DateTimeOffset? LaunchIssuedAtUtc { get; init; }

    public string? SentinelRelativePath { get; init; }
}

public sealed record AiCompanionStartupOptions
{
    public bool ForceSkipIntroLogoWhenHarnessEnabled { get; init; } = true;
}

public sealed record AiCompanionRuntimeConfig
{
    public bool Enabled { get; init; } = true;

    public GamePathOptions GamePaths { get; init; } = GamePathOptions.CreateLocalDefault();

    public LiveExportOptions LiveExport { get; init; } = LiveExportOptions.Defaults;

    public HarnessOptions Harness { get; init; } = HarnessOptions.Defaults;

    public AiCompanionStartupSentinelConfig StartupSentinel { get; init; } = new();

    public AiCompanionStartupOptions Startup { get; init; } = new();

    public static AiCompanionRuntimeConfig Defaults { get; } = new();
}

internal static class AiCompanionRuntimeConfigLoader
{
    public static AiCompanionRuntimeConfig Load(string modDirectory)
    {
        var path = Path.Combine(modDirectory, AiCompanionRuntimeState.RuntimeConfigFileName);
        if (!File.Exists(path))
        {
            return AiCompanionRuntimeConfig.Defaults;
        }

        try
        {
            return JsonSerializer.Deserialize<AiCompanionRuntimeConfig>(File.ReadAllText(path), JsonOptions)
                ?? AiCompanionRuntimeConfig.Defaults;
        }
        catch (JsonException exception)
        {
            AiCompanionRuntimeLog.WriteLine($"ignored invalid runtime config: {exception.Message}");
            return AiCompanionRuntimeConfig.Defaults;
        }
    }

    private static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
}
