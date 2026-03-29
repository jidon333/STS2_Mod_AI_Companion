using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModAiCompanion.Mod;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;
using static GuiSmokeChoicePrimitiveSupport;

internal static partial class Program
{
    static string ResolveProviderKind(IReadOnlyDictionary<string, string> options, bool isLongRun)
    {
        if (!options.TryGetValue("--provider", out var providerRaw) || string.IsNullOrWhiteSpace(providerRaw))
        {
            return isLongRun
                ? "auto"
                : "session";
        }

        return providerRaw.Trim().ToLowerInvariant() switch
        {
            "session" => "session",
            "auto" => "auto",
            "headless" => "headless",
            _ => throw new InvalidOperationException($"Unsupported provider: {providerRaw}"),
        };
    }

    static void ValidateProviderConfiguration(string providerKind, IReadOnlyDictionary<string, string> options)
    {
        if (string.Equals(providerKind, "headless", StringComparison.OrdinalIgnoreCase)
            && (!options.TryGetValue("--provider-command", out var providerCommand)
                || string.IsNullOrWhiteSpace(providerCommand)))
        {
            throw new InvalidOperationException("--provider-command is required for --provider headless.");
        }
    }

    static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length; index += 1)
        {
            var current = args[index];
            if (!current.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            if (index + 1 < args.Length && !args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                options[current] = args[index + 1];
                index += 1;
                continue;
            }

            options[current] = "true";
        }

        return options;
    }

    static CaptureFaultInjectionOptions? ResolveCaptureFaultInjectionOptions(IReadOnlyDictionary<string, string> options)
    {
        if (!options.TryGetValue("--capture-fault-mode", out var modeRaw) || string.IsNullOrWhiteSpace(modeRaw))
        {
            return null;
        }

        var failureKind = modeRaw.Trim().ToLowerInvariant() switch
        {
            "unusable-frame" => CaptureBoundaryFailureKind.UnusableFrame,
            "timeout" => CaptureBoundaryFailureKind.TimedOut,
            "exception" => CaptureBoundaryFailureKind.Exception,
            _ => throw new InvalidOperationException($"Unsupported capture fault mode: {modeRaw}"),
        };
        var scopeKind = options.TryGetValue("--capture-fault-scope", out var scopeRaw) && !string.IsNullOrWhiteSpace(scopeRaw)
            ? scopeRaw.Trim().ToLowerInvariant()
            : "attempt";
        var phaseName = options.TryGetValue("--capture-fault-phase", out var phaseRaw) && !string.IsNullOrWhiteSpace(phaseRaw)
            ? phaseRaw.Trim()
            : null;
        int? stepIndex = options.TryGetValue("--capture-fault-step", out var stepRaw)
                         && int.TryParse(stepRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedStep)
            ? parsedStep
            : null;
        int? attemptOrdinal = options.TryGetValue("--capture-fault-attempt", out var attemptRaw)
                              && int.TryParse(attemptRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedAttempt)
            ? parsedAttempt
            : null;
        return new CaptureFaultInjectionOptions(failureKind, scopeKind, phaseName, stepIndex, attemptOrdinal);
    }

    static bool IsLifecycleProofModeEnabled(IReadOnlyDictionary<string, string> options)
    {
        return options.ContainsKey("--lifecycle-proof-mode");
    }

    static string? ResolveConfigPath(IReadOnlyDictionary<string, string> options, string workspaceRoot)
    {
        if (options.TryGetValue("--config", out var explicitPath))
        {
            return ResolveCliPath(explicitPath, workspaceRoot);
        }

        var samplePath = Path.Combine(workspaceRoot, "config", "ai-companion.sample.json");
        return File.Exists(samplePath) ? samplePath : null;
    }

    static string ResolveCliPath(string explicitPath, string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(explicitPath))
        {
            throw new InvalidOperationException("A non-empty CLI path is required.");
        }

        if (TryConvertWslPathToWindowsPath(explicitPath, out var translatedPath))
        {
            return Path.GetFullPath(translatedPath);
        }

        return Path.IsPathRooted(explicitPath)
            ? Path.GetFullPath(explicitPath)
            : Path.GetFullPath(explicitPath, workspaceRoot);
    }

    static bool TryConvertWslPathToWindowsPath(string path, out string translatedPath)
    {
        translatedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        if (path.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase)
            || (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':'))
        {
            return false;
        }

        var normalized = path.Replace('\\', '/');
        if (normalized.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase)
            && normalized.Length >= 7
            && char.IsLetter(normalized[5])
            && normalized[6] == '/')
        {
            var driveLetter = char.ToUpperInvariant(normalized[5]);
            var remainder = normalized.Length == 7 ? string.Empty : normalized[7..].Replace('/', '\\');
            translatedPath = string.IsNullOrEmpty(remainder)
                ? $"{driveLetter}:\\"
                : $"{driveLetter}:\\{remainder}";
            return true;
        }

        if (!normalized.StartsWith("/", StringComparison.Ordinal))
        {
            return false;
        }

        var distroName = Environment.GetEnvironmentVariable("WSL_DISTRO_NAME");
        if (!string.IsNullOrWhiteSpace(distroName))
        {
            translatedPath = $@"\\wsl.localhost\{distroName}\{normalized.TrimStart('/').Replace('/', '\\')}";
            return true;
        }

        return TryResolveWslPathViaExe(normalized, out translatedPath);
    }

    static bool TryResolveWslPathViaExe(string wslPath, out string translatedPath)
    {
        translatedPath = string.Empty;

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"wslpath -w \"{wslPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            if (!process.WaitForExit(3000) || process.ExitCode != 0)
            {
                return false;
            }

            var stdout = process.StandardOutput.ReadToEnd();
            if (string.IsNullOrWhiteSpace(stdout))
            {
                return false;
            }

            translatedPath = stdout.Trim();
            return true;
        }
        catch
        {
            return false;
        }
    }

    static ScaffoldConfiguration ApplyPathOverrides(ScaffoldConfiguration configuration, IReadOnlyDictionary<string, string> options)
    {
        var gamePaths = configuration.GamePaths.With(new PartialGamePathOptions
        {
            GameDirectory = options.TryGetValue("--game-dir", out var gameDirectory) ? gameDirectory : null,
            UserDataRoot = options.TryGetValue("--user-data-root", out var userDataRoot) ? userDataRoot : null,
            SteamAccountId = options.TryGetValue("--steam-account-id", out var steamAccountId) ? steamAccountId : null,
            ProfileIndex = options.TryGetValue("--profile-index", out var profileIndexRaw)
                && int.TryParse(profileIndexRaw, out var profileIndex)
                ? profileIndex
                : null,
            ArtifactsRoot = options.TryGetValue("--artifacts-root", out var artifactsRoot) ? artifactsRoot : null,
        });

        return configuration with
        {
            GamePaths = gamePaths,
        };
    }

    static void WriteUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- run --scenario boot-to-combat|boot-to-long-run --provider session|auto|headless [--provider-command \"<cmd>\"] [--config path] [--run-root path] [--deploy-mode in-process|subprocess] [--runtime-assembly-root path] [--ffmpeg-path path] [--keep-video-on-success] [--disable-video-capture] [--max-attempts n] [--max-consecutive-launch-failures n] [--max-scene-dead-ends n] [--max-session-hours n] [--max-steps n] [--stop-on-first-terminal] [--stop-on-first-loop] [--capture-fault-mode unusable-frame|timeout|exception] [--capture-fault-scope bootstrap|attempt] [--capture-fault-phase <GuiSmokePhase>] [--capture-fault-step n] [--capture-fault-attempt n] [--lifecycle-proof-mode]");
        Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- inspect-run --run-root <path>");
        Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- inspect-session --session-root <path>");
        Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- replay-step --request <path> [--decision <path>] [--out <path>] [--trace] [--full-request-rebuild]");
        Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- replay-test [--suite <path>] [--trace] [--full-request-rebuild]");
        Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- replay-parity-test [--suite <path>] [--trace]");
        Console.WriteLine("  dotnet run --project src\\Sts2GuiSmokeHarness -- self-test");
    }

    static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
