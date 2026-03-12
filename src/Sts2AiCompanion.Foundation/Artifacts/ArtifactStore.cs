using System.Text.Json;
using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Foundation.Artifacts;

public sealed class ArtifactStore
{
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    public ArtifactStore(CompanionArtifactPaths paths)
    {
        Paths = paths;
    }

    public CompanionArtifactPaths Paths { get; }

    public void EnsureRunDirectories()
    {
        Directory.CreateDirectory(Paths.CompanionRoot);
        if (Paths.RunRoot is not null) Directory.CreateDirectory(Paths.RunRoot);
        if (Paths.LiveMirrorRoot is not null) Directory.CreateDirectory(Paths.LiveMirrorRoot);
        if (Paths.PromptPacksRoot is not null) Directory.CreateDirectory(Paths.PromptPacksRoot);
        if (Paths.AdviceRoot is not null) Directory.CreateDirectory(Paths.AdviceRoot);
    }

    public async Task WriteJsonAsync<T>(string path, T payload, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, payload, _jsonOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task AppendNdjsonAsync<T>(string path, T payload, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var line = JsonSerializer.Serialize(payload, _jsonOptions);
        await File.AppendAllTextAsync(path, line + Environment.NewLine, cancellationToken).ConfigureAwait(false);
    }
}
