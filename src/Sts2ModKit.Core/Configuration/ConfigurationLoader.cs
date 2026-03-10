using System.Text.Json;

namespace Sts2ModKit.Core.Configuration;

public static class ConfigurationLoader
{
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static ConfigurationLoadResult LoadFromFile(string? configPath)
    {
        var source = string.IsNullOrWhiteSpace(configPath) ? "<defaults>" : configPath!;
        if (string.IsNullOrWhiteSpace(configPath) || !File.Exists(configPath))
        {
            return LoadFromJson(null, source);
        }

        var json = File.ReadAllText(configPath);
        return LoadFromJson(json, source);
    }

    public static ConfigurationLoadResult LoadFromJson(string? json, string source)
    {
        var warnings = new List<string>();
        var configuration = ScaffoldConfiguration.CreateLocalDefault();

        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var partial = JsonSerializer.Deserialize<PartialScaffoldConfiguration>(json, JsonOptions);
                configuration = configuration.With(partial);
            }
            catch (JsonException exception)
            {
                warnings.Add($"Ignored configuration document from {source}: {exception.Message}");
            }
        }

        return new ConfigurationLoadResult
        {
            Configuration = configuration,
            ConfigurationSource = source,
            Warnings = warnings,
        };
    }

    public static string Serialize(ScaffoldConfiguration configuration)
    {
        return JsonSerializer.Serialize(configuration, JsonOptions);
    }
}
