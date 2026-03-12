using System.Text.Json;
using Sts2AiCompanion.Foundation.Contracts;

namespace Sts2AiCompanion.Harness.Replay;

public sealed class ReplayController
{
    public async Task<CompanionState?> LoadStateAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<CompanionState>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
