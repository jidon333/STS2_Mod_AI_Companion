using System.Text.Json;

namespace Sts2ModKit.Tool;

internal static class ProgramJson
{
    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
}
