using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
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

    public static string DescribeCoreAssembly()
    {
        return DescribeAssembly(typeof(Sts2ModKit.Core.LiveExport.LiveExportAtomicFileWriter).Assembly, includeWriteJsonSignature: true);
    }

    public static string DescribeWriterCompatibility()
    {
        var writerType = typeof(Sts2ModKit.Core.LiveExport.LiveExportAtomicFileWriter);
        var methods = writerType.GetMethods(BindingFlags.Public | BindingFlags.Static);
        var hasTwoArgWriteJson = methods.Any(method =>
            string.Equals(method.Name, "WriteJsonAtomic", StringComparison.Ordinal)
            && method.IsGenericMethodDefinition
            && method.GetParameters().Length == 2);
        var hasThreeArgWriteJson = methods.Any(method =>
            string.Equals(method.Name, "WriteJsonAtomic", StringComparison.Ordinal)
            && method.IsGenericMethodDefinition
            && method.GetParameters().Length == 3);
        var hasAppendShared = methods.Any(method =>
            string.Equals(method.Name, "AppendAllTextShared", StringComparison.Ordinal));

        return string.Join(
            " | ",
            new[]
            {
                $"write_json_atomic_two_arg={hasTwoArgWriteJson}",
                $"write_json_atomic_three_arg={hasThreeArgWriteJson}",
                $"append_all_text_shared={hasAppendShared}",
            });
    }

    private static string DescribeAssembly(Assembly assembly, bool includeWriteJsonSignature)
    {
        var location = assembly.Location;
        var fileInfo = File.Exists(location) ? new FileInfo(location) : null;
        var versionInfo = string.IsNullOrWhiteSpace(location) ? null : FileVersionInfo.GetVersionInfo(location);
        var parts = new List<string>
        {
            $"assembly={assembly.GetName().Name}",
            $"version={assembly.GetName().Version}",
            $"mvid={assembly.ManifestModule.ModuleVersionId}",
            $"location={location}",
            $"size={fileInfo?.Length}",
            $"last_write_utc={fileInfo?.LastWriteTimeUtc:O}",
            $"file_version={versionInfo?.FileVersion}",
        };

        if (includeWriteJsonSignature)
        {
            var hasCompatibilityOverload = typeof(Sts2ModKit.Core.LiveExport.LiveExportAtomicFileWriter)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Any(method =>
                    string.Equals(method.Name, "WriteJsonAtomic", StringComparison.Ordinal)
                    && method.GetParameters().Length == 2);
            parts.Add($"write_json_atomic_two_arg={hasCompatibilityOverload}");
        }

        return string.Join(" | ", parts);
    }
}
