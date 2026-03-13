using System.Text.Json;
using Sts2ModKit.Core.Harness;

namespace Sts2ModAiCompanion.HarnessBridge;

internal sealed class ArmSessionReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public HarnessArmSession? TryReadActiveSession(
        string armSessionPath,
        out bool armPresent,
        out bool sessionTokenPresent,
        out string message)
    {
        armPresent = File.Exists(armSessionPath);
        sessionTokenPresent = false;
        if (!armPresent)
        {
            message = "no-arm";
            return null;
        }

        try
        {
            var session = JsonSerializer.Deserialize<HarnessArmSession>(File.ReadAllText(armSessionPath), JsonOptions);
            if (session is null)
            {
                message = "invalid-arm";
                return null;
            }

            sessionTokenPresent = !string.IsNullOrWhiteSpace(session.SessionToken);
            if (!sessionTokenPresent)
            {
                message = "missing-session-token";
                return null;
            }

            if (session.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                message = "expired-arm";
                return null;
            }

            message = "armed";
            return session;
        }
        catch
        {
            message = "invalid-arm";
            return null;
        }
    }
}
