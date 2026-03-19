using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Sts2ModKit.Core.Configuration;
using Sts2ModAiCompanion.Mod.Runtime;

namespace Sts2ModAiCompanion.Mod;

public sealed record NativePackageFile(
    string RelativePath,
    string OutputPath,
    string SourceKind,
    string Status);

public sealed record MissingNativeArtifact(
    string RelativePath,
    string Reason);

public sealed record NativePackageResult(
    string OutputRoot,
    string LayoutKind,
    string ModsRoot,
    string PackageRoot,
    string ReportPath,
    IReadOnlyList<NativePackageFile> Files,
    IReadOnlyList<MissingNativeArtifact> MissingArtifacts,
    IReadOnlyList<string> Warnings);

public sealed record PckBuildMapping(
    string SourcePath,
    string TargetPath);

public sealed record PckBuildSpec(
    string OutputPckPath,
    IReadOnlyList<PckBuildMapping> Files);

public sealed record NativePckBuildResult(
    string OutputRoot,
    string LayoutKind,
    string PackageRoot,
    string SpecPath,
    string OutputPckPath,
    string GodotExecutablePath,
    int ExitCode,
    string StdOut,
    string StdErr);

public sealed record NativeDeploymentFile(
    string SourcePath,
    string DestinationPath,
    string Status);

public sealed record NativeDeploymentResult(
    string LayoutKind,
    string SourcePackageRoot,
    string ModsRoot,
    string DeployedRoot,
    string ReportPath,
    IReadOnlyList<NativeDeploymentFile> Files,
    IReadOnlyList<string> Warnings);

public static partial class AiCompanionModEntryPoint
{
    public static NativePackageResult MaterializeNativePackage(
        ScaffoldConfiguration configuration,
        string outputRoot,
        string runtimeAssemblyRoot,
        string layoutKind)
    {
        var normalizedLayout = NormalizeNativeLayoutKind(layoutKind);
        var descriptor = CreateDescriptor(configuration);
        var modsRoot = Path.Combine(outputRoot, "native-package-layout", normalizedLayout, "mods");
        var packageRoot = normalizedLayout == "flat"
            ? modsRoot
            : Path.Combine(modsRoot, descriptor.PackageFolderName);
        var reportPath = Path.Combine(outputRoot, "native-package-layout", normalizedLayout, "native-package-report.json");

        Directory.CreateDirectory(packageRoot);

        var files = new List<NativePackageFile>();
        var warnings = new List<string>
        {
            "This repo currently stages a minimal STS2 native mods-folder package.",
            "Run build-native-pck to create the Godot resource pack before live deployment.",
        };

        files.Add(WriteNativeTextFile(
            packageRoot,
            AiCompanionRuntimeState.RuntimeConfigFileName,
            BuildRuntimeConfigContents(configuration),
            "generated"));

        var primaryAssemblySourcePath = Path.Combine(runtimeAssemblyRoot, configuration.AiCompanionMod.RuntimeAssemblyFileName);
        var primaryAssemblyTargetName = descriptor.PckName.Replace(".pck", ".dll", StringComparison.OrdinalIgnoreCase);
        if (File.Exists(primaryAssemblySourcePath))
        {
            files.Add(CopyNativeFile(packageRoot, primaryAssemblyTargetName, primaryAssemblySourcePath, "build-artifact"));
        }
        else
        {
            warnings.Add($"Runtime build artifact not found: {primaryAssemblySourcePath}");
        }

        foreach (var assemblyName in new[]
                 {
                     "Sts2ModKit.Core.dll",
                 })
        {
            var sourcePath = Path.Combine(runtimeAssemblyRoot, assemblyName);
            if (!File.Exists(sourcePath))
            {
                warnings.Add($"Runtime build artifact not found: {sourcePath}");
                continue;
            }

            files.Add(CopyNativeFile(packageRoot, assemblyName, sourcePath, "build-artifact"));
        }

        var assemblyManifestPath = Path.Combine(packageRoot, "runtime-assembly-manifest.json");
        File.WriteAllText(
            assemblyManifestPath,
            JsonSerializer.Serialize(
                BuildRuntimeAssemblyManifest(packageRoot, primaryAssemblyTargetName, configuration),
                JsonOptions));
        files.Add(new NativePackageFile("runtime-assembly-manifest.json", assemblyManifestPath, "generated", "written"));

        var outputPckPath = Path.Combine(packageRoot, descriptor.PckName);
        var missingArtifacts = new List<MissingNativeArtifact>();
        if (File.Exists(outputPckPath))
        {
            files.Add(new NativePackageFile(
                descriptor.PckName,
                outputPckPath,
                "existing-artifact",
                "preserved"));
        }
        else
        {
            missingArtifacts.Add(new MissingNativeArtifact(
                descriptor.PckName,
                "Run build-native-pck to create the Godot resource pack required by the native mods-folder route."));
        }

        PruneUnexpectedNativeFiles(packageRoot, files.Select(file => file.RelativePath));

        var result = new NativePackageResult(
            outputRoot,
            normalizedLayout,
            modsRoot,
            packageRoot,
            reportPath,
            files,
            missingArtifacts,
            warnings);

        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        File.WriteAllText(reportPath, JsonSerializer.Serialize(result, JsonOptions));
        return result;
    }

    public static NativePckBuildResult BuildNativePck(
        ScaffoldConfiguration configuration,
        string outputRoot,
        string runtimeAssemblyRoot,
        string layoutKind,
        string godotExecutablePath,
        string workspaceRoot)
    {
        var package = MaterializeNativePackage(configuration, outputRoot, runtimeAssemblyRoot, layoutKind);
        var descriptor = CreateDescriptor(configuration);
        var normalizedLayout = NormalizeNativeLayoutKind(layoutKind);
        var outputPckPath = Path.Combine(package.PackageRoot, descriptor.PckName);
        var specPath = Path.Combine(outputRoot, "native-package-layout", normalizedLayout, "pck-build-spec.json");
        var templateProjectRoot = Path.Combine(workspaceRoot, "templates", "basic-native-mod", "export-pack-template");
        var exportProjectRoot = Path.Combine(outputRoot, "native-package-layout", normalizedLayout, "export-project");

        if (!File.Exists(godotExecutablePath))
        {
            throw new FileNotFoundException("Godot executable was not found.", godotExecutablePath);
        }

        if (!Directory.Exists(templateProjectRoot))
        {
            throw new DirectoryNotFoundException($"Godot export-pack template project was not found: {templateProjectRoot}");
        }

        RecreateDirectory(exportProjectRoot);
        CopyDirectoryContents(templateProjectRoot, exportProjectRoot);
        File.WriteAllText(Path.Combine(exportProjectRoot, "mod_manifest.json"), CreateNativeManifestJson(descriptor));

        var spec = new PckBuildSpec(
            outputPckPath,
            new[]
            {
                new PckBuildMapping(
                    Path.Combine(exportProjectRoot, "mod_manifest.json"),
                    "res://mod_manifest.json"),
            });

        Directory.CreateDirectory(Path.GetDirectoryName(specPath)!);
        File.WriteAllText(specPath, JsonSerializer.Serialize(spec, JsonOptions));

        var startInfo = new ProcessStartInfo
        {
            FileName = godotExecutablePath,
            Arguments = $"--headless --path \"{exportProjectRoot}\" --export-pack \"Windows Desktop\" \"{outputPckPath}\"",
            WorkingDirectory = exportProjectRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start Godot executable: {godotExecutablePath}");
        var stdOut = process.StandardOutput.ReadToEnd();
        var stdErr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        var result = new NativePckBuildResult(
            outputRoot,
            normalizedLayout,
            package.PackageRoot,
            specPath,
            outputPckPath,
            godotExecutablePath,
            process.ExitCode,
            stdOut,
            stdErr);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Godot PCK build failed with exit code {process.ExitCode}.{Environment.NewLine}{stdOut}{Environment.NewLine}{stdErr}");
        }

        return result;
    }

    public static NativeDeploymentResult DeployNativePackage(
        ScaffoldConfiguration configuration,
        string outputRoot,
        string runtimeAssemblyRoot,
        string layoutKind)
    {
        var package = MaterializeNativePackage(configuration, outputRoot, runtimeAssemblyRoot, layoutKind);
        var normalizedLayout = NormalizeNativeLayoutKind(layoutKind);
        var descriptor = CreateDescriptor(configuration);
        var modsRoot = Path.Combine(configuration.GamePaths.GameDirectory, "mods");
        var deployedRoot = normalizedLayout == "flat"
            ? modsRoot
            : Path.Combine(modsRoot, descriptor.PackageFolderName);
        var reportPath = Path.Combine(outputRoot, "native-package-layout", normalizedLayout, "native-deploy-report.json");
        var warnings = new List<string>();
        var files = new List<NativeDeploymentFile>();

        Directory.CreateDirectory(modsRoot);
        Directory.CreateDirectory(deployedRoot);

        if (Process.GetProcessesByName("SlayTheSpire2").Length > 0)
        {
            warnings.Add("SlayTheSpire2.exe is running during deploy. A locked stale DLL can survive until the next restart.");
        }

        var deployableFiles = Directory.GetFiles(package.PackageRoot, "*", SearchOption.TopDirectoryOnly)
            .Where(path => !string.Equals(Path.GetFileName(path), "runtime-assembly-manifest.json", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        foreach (var file in deployableFiles)
        {
            var fileName = Path.GetFileName(file);
            var destinationPath = Path.Combine(deployedRoot, fileName);
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            File.Copy(file, destinationPath, overwrite: true);
            files.Add(new NativeDeploymentFile(file, destinationPath, "copied"));
            var sourceHash = ComputeFileHash(file);
            var deployedHash = ComputeFileHash(destinationPath);
            if (!string.Equals(sourceHash, deployedHash, StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add($"Deploy verification mismatch: {fileName} source={sourceHash} deployed={deployedHash}");
            }

            if (fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add($"Deploy verification: {fileName} hash={deployedHash}");
            }
        }

        foreach (var staleFileName in new[]
                 {
                     "runtime-assembly-manifest.json",
                     "sts2-mod-ai-companion.config.json",
                 })
        {
            var stalePath = Path.Combine(deployedRoot, staleFileName);
            if (!File.Exists(stalePath))
            {
                continue;
            }

            File.Delete(stalePath);
            warnings.Add($"Removed stale deploy artifact: {staleFileName}");
        }

        if (!files.Any(file => file.DestinationPath.EndsWith(".pck", StringComparison.OrdinalIgnoreCase)))
        {
            warnings.Add("No .pck file was deployed. Run build-native-pck before launching the game.");
        }

        var result = new NativeDeploymentResult(
            normalizedLayout,
            package.PackageRoot,
            modsRoot,
            deployedRoot,
            reportPath,
            files,
            warnings);

        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        File.WriteAllText(reportPath, JsonSerializer.Serialize(result, JsonOptions));
        return result;
    }

    private static string NormalizeNativeLayoutKind(string? layoutKind)
    {
        if (string.Equals(layoutKind, "subdir", StringComparison.OrdinalIgnoreCase))
        {
            return "subdir";
        }

        return "flat";
    }

    private static NativePackageFile WriteNativeTextFile(
        string packageRoot,
        string relativePath,
        string contents,
        string sourceKind)
    {
        var outputPath = Path.Combine(packageRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllText(outputPath, contents);
        return new NativePackageFile(relativePath, outputPath, sourceKind, "written");
    }

    private static NativePackageFile CopyNativeFile(
        string packageRoot,
        string relativePath,
        string sourcePath,
        string sourceKind)
    {
        var outputPath = Path.Combine(packageRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.Copy(sourcePath, outputPath, overwrite: true);
        return new NativePackageFile(relativePath, outputPath, sourceKind, "copied");
    }

    private static string CreateNativeManifestJson(AiCompanionModDescriptor descriptor)
    {
        var manifest = new
        {
            pck_name = Path.GetFileNameWithoutExtension(descriptor.PckName),
            name = descriptor.Name,
            author = descriptor.Author,
            description = descriptor.Description,
            version = descriptor.Version,
        };

        return JsonSerializer.Serialize(manifest, JsonOptions);
    }

    private static string BuildRuntimeConfigContents(ScaffoldConfiguration configuration)
    {
        var runtimeConfig = new AiCompanionRuntimeConfig
        {
            Enabled = configuration.LiveExport.Enabled,
            GamePaths = configuration.GamePaths,
            LiveExport = configuration.LiveExport,
            Harness = configuration.Harness,
        };

        return JsonSerializer.Serialize(runtimeConfig, JsonOptions);
    }

    private static string ComputeFileHash(string path)
    {
        using var stream = File.OpenRead(path);
        using var sha256 = SHA256.Create();
        return Convert.ToHexString(sha256.ComputeHash(stream));
    }

    private static object BuildRuntimeAssemblyManifest(
        string packageRoot,
        string primaryAssemblyTargetName,
        ScaffoldConfiguration configuration)
    {
        return new
        {
            generatedAt = DateTimeOffset.UtcNow,
            collectorModeEnabled = configuration.LiveExport.CollectorModeEnabled,
            assemblies = new[]
            {
                DescribeAssemblyArtifact(packageRoot, primaryAssemblyTargetName),
                DescribeAssemblyArtifact(packageRoot, "Sts2ModKit.Core.dll"),
            }.Where(entry => entry is not null).ToArray(),
        };
    }

    private static object? DescribeAssemblyArtifact(string packageRoot, string fileName)
    {
        var path = Path.Combine(packageRoot, fileName);
        if (!File.Exists(path))
        {
            return null;
        }

        var fileInfo = new FileInfo(path);
        return new
        {
            fileName,
            path,
            size = fileInfo.Length,
            lastWriteUtc = fileInfo.LastWriteTimeUtc,
            sha256 = ComputeFileHash(path),
        };
    }

    private static void RecreateDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static void CopyDirectoryContents(string sourceDirectory, string destinationDirectory)
    {
        foreach (var directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(destinationDirectory, relative));
        }

        foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, file);
            var destinationPath = Path.Combine(destinationDirectory, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(file, destinationPath, overwrite: true);
        }
    }

    private static void PruneUnexpectedNativeFiles(string packageRoot, IEnumerable<string> expectedRelativePaths)
    {
        var expectedFileNames = new HashSet<string>(
            expectedRelativePaths.Select(relativePath => Path.GetFileName(relativePath) ?? string.Empty),
            StringComparer.OrdinalIgnoreCase);

        foreach (var file in Directory.GetFiles(packageRoot, "*", SearchOption.TopDirectoryOnly))
        {
            var fileName = Path.GetFileName(file);
            if (expectedFileNames.Contains(fileName))
            {
                continue;
            }

            File.Delete(file);
        }
    }
}
