using System.Diagnostics;
using System.Reflection;

namespace Sts2ModAiCompanion.Mod.Runtime;

internal static class AiCompanionRuntimeIdentity
{
    public static string DescribeExecutingAssembly()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var location = assembly.Location;
        var fileInfo = File.Exists(location) ? new FileInfo(location) : null;
        var versionInfo = string.IsNullOrWhiteSpace(location) ? null : FileVersionInfo.GetVersionInfo(location);
        return string.Join(
            " | ",
            new[]
            {
                $"assembly={assembly.GetName().Name}",
                $"version={assembly.GetName().Version}",
                $"mvid={assembly.ManifestModule.ModuleVersionId}",
                $"location={location}",
                $"size={fileInfo?.Length}",
                $"last_write_utc={fileInfo?.LastWriteTimeUtc:O}",
                $"file_version={versionInfo?.FileVersion}",
            });
    }
}
