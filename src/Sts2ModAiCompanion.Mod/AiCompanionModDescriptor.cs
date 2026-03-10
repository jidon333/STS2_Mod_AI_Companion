using System.Text.Json;
using Sts2ModKit.Core.Configuration;

namespace Sts2ModAiCompanion.Mod;

public sealed record AiCompanionModDescriptor(
    string Name,
    string Author,
    string Version,
    string Description,
    string PckName,
    string PackageFolderName);

public static partial class AiCompanionModEntryPoint
{
    public static AiCompanionModDescriptor CreateDescriptor(ScaffoldConfiguration configuration)
    {
        var options = configuration.AiCompanionMod;
        return new AiCompanionModDescriptor(
            options.Name,
            options.Author,
            options.Version,
            options.Description,
            options.PckName,
            options.PackageFolderName);
    }

    internal static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
}
