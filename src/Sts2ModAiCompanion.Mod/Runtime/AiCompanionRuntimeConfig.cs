using System.Text.Json;

namespace Sts2ModAiCompanion.Mod.Runtime;

public sealed record AiCompanionRuntimeConfig
{
    public bool Enabled { get; init; } = true;

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
