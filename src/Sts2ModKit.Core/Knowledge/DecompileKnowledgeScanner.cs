using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace Sts2ModKit.Core.Knowledge;

public static class DecompileKnowledgeScanner
{
    public static StaticKnowledgeDecompileScan EnsureDecompiled(
        string assemblyPath,
        string decompiledRoot,
        out IReadOnlyList<string> warnings)
    {
        var localWarnings = new List<string>();
        if (!File.Exists(assemblyPath))
        {
            localWarnings.Add($"Missing assembly decompile target: {assemblyPath}");
            warnings = localWarnings;
            return CreateEmpty(assemblyPath, decompiledRoot, localWarnings);
        }

        if (!TryResolveToolPath(out var toolPath, out var toolWarnings))
        {
            localWarnings.AddRange(toolWarnings);
            warnings = localWarnings;
            return CreateEmpty(assemblyPath, decompiledRoot, localWarnings);
        }

        localWarnings.AddRange(toolWarnings);
        Directory.CreateDirectory(decompiledRoot);

        var fingerprint = BuildFingerprint(assemblyPath);
        var markerPath = Path.Combine(decompiledRoot, ".decompile-scan.json");
        var marker = TryReadMarker(markerPath);
        var toolVersion = ReadToolVersion(toolPath!, localWarnings);
        var canReuse = marker is not null
            && string.Equals(marker.Fingerprint, fingerprint, StringComparison.Ordinal)
            && string.Equals(marker.ToolPath, toolPath, StringComparison.OrdinalIgnoreCase)
            && Directory.Exists(Path.Combine(decompiledRoot, "MegaCrit"));

        if (!canReuse)
        {
            RecreateDirectoryContents(decompiledRoot);
            var arguments = new[] { "-p", "--nested-directories", "-o", decompiledRoot, assemblyPath };
            var (exitCode, stdout, stderr) = RunProcess(toolPath!, arguments, Path.GetDirectoryName(assemblyPath)!);
            if (exitCode != 0)
            {
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    localWarnings.Add($"ilspycmd stderr: {TrimLine(stderr)}");
                }

                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    localWarnings.Add($"ilspycmd stdout: {TrimLine(stdout)}");
                }

                warnings = localWarnings;
                return CreateEmpty(assemblyPath, decompiledRoot, localWarnings) with
                {
                    ToolPath = toolPath!,
                    ToolVersion = toolVersion,
                    Fingerprint = fingerprint,
                };
            }

            var nextMarker = new DecompileMarker(fingerprint, toolPath!, toolVersion);
            File.WriteAllText(
                markerPath,
                JsonSerializer.Serialize(
                    nextMarker,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                    }));
        }

        var stats = BuildStats(decompiledRoot);
        warnings = localWarnings;
        return new StaticKnowledgeDecompileScan(
            DateTimeOffset.UtcNow,
            assemblyPath,
            decompiledRoot,
            toolPath!,
            toolVersion,
            fingerprint,
            canReuse,
            stats,
            localWarnings);
    }

    private static IReadOnlyDictionary<string, string?> BuildStats(string decompiledRoot)
    {
        var csFiles = Directory.Exists(decompiledRoot)
            ? Directory.GetFiles(decompiledRoot, "*.cs", SearchOption.AllDirectories)
            : Array.Empty<string>();

        return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["decompiledFileCount"] = csFiles.Length.ToString(),
            ["cardsDirectoryExists"] = Directory.Exists(Path.Combine(decompiledRoot, "MegaCrit", "Sts2", "Core", "Models", "Cards")).ToString(),
            ["relicsDirectoryExists"] = Directory.Exists(Path.Combine(decompiledRoot, "MegaCrit", "Sts2", "Core", "Models", "Relics")).ToString(),
            ["potionsDirectoryExists"] = Directory.Exists(Path.Combine(decompiledRoot, "MegaCrit", "Sts2", "Core", "Models", "Potions")).ToString(),
            ["eventsDirectoryExists"] = Directory.Exists(Path.Combine(decompiledRoot, "MegaCrit", "Sts2", "Core", "Models", "Events")).ToString(),
        };
    }

    private static StaticKnowledgeDecompileScan CreateEmpty(
        string assemblyPath,
        string decompiledRoot,
        IReadOnlyList<string> warnings)
    {
        return new StaticKnowledgeDecompileScan(
            DateTimeOffset.UtcNow,
            assemblyPath,
            decompiledRoot,
            string.Empty,
            string.Empty,
            string.Empty,
            false,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase),
            warnings);
    }

    private static string BuildFingerprint(string assemblyPath)
    {
        var info = new FileInfo(assemblyPath);
        using var stream = File.OpenRead(assemblyPath);
        using var sha = SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(stream)).ToLowerInvariant();
        return $"{info.Length}:{info.LastWriteTimeUtc.Ticks}:{hash}";
    }

    private static bool TryResolveToolPath(out string? toolPath, out IReadOnlyList<string> warnings)
    {
        var localWarnings = new List<string>();
        foreach (var candidate in EnumerateToolPathCandidates())
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            if (File.Exists(candidate))
            {
                toolPath = candidate;
                warnings = localWarnings;
                return true;
            }

            var whereResult = RunProcess("where", new[] { candidate }, Environment.CurrentDirectory);
            if (whereResult.ExitCode == 0)
            {
                var resolved = whereResult.StdOut
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault(line => File.Exists(line.Trim()));
                if (!string.IsNullOrWhiteSpace(resolved))
                {
                    toolPath = resolved.Trim();
                    warnings = localWarnings;
                    return true;
                }
            }
        }

        localWarnings.Add("ilspycmd executable was not found. Install a compatible global .NET tool, for example: dotnet tool install --global ilspycmd --version 8.2.0.7535");
        toolPath = null;
        warnings = localWarnings;
        return false;
    }

    private static IEnumerable<string> EnumerateToolPathCandidates()
    {
        var explicitPath = Environment.GetEnvironmentVariable("ILSPYCMD_EXE");
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            yield return explicitPath;
        }

        yield return "ilspycmd";

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(userProfile))
        {
            yield return Path.Combine(userProfile, ".dotnet", "tools", "ilspycmd.exe");
            yield return Path.Combine(userProfile, ".dotnet", "tools", "ilspycmd");
        }
    }

    private static string ReadToolVersion(string toolPath, ICollection<string> warnings)
    {
        var (exitCode, stdout, stderr) = RunProcess(toolPath, new[] { "--version" }, Environment.CurrentDirectory);
        if (exitCode == 0 && !string.IsNullOrWhiteSpace(stdout))
        {
            return TrimLine(stdout);
        }

        if (!string.IsNullOrWhiteSpace(stderr))
        {
            warnings.Add($"Failed to read ilspycmd version: {TrimLine(stderr)}");
        }

        return string.Empty;
    }

    private static (int ExitCode, string StdOut, string StdErr) RunProcess(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
        };

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, stdout, stderr);
    }

    private static string TrimLine(string text)
    {
        return text
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();
    }

    private static void RecreateDirectoryContents(string directory)
    {
        if (Directory.Exists(directory))
        {
            foreach (var child in Directory.GetFileSystemEntries(directory))
            {
                if (Directory.Exists(child))
                {
                    Directory.Delete(child, recursive: true);
                }
                else
                {
                    File.Delete(child);
                }
            }
        }
        else
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static DecompileMarker? TryReadMarker(string markerPath)
    {
        if (!File.Exists(markerPath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<DecompileMarker>(File.ReadAllText(markerPath));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record DecompileMarker(string Fingerprint, string ToolPath, string ToolVersion);
}
