using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;

sealed class GuiSmokeVideoRecorder : IDisposable
{
    private const int StopTimeoutMs = 5000;
    private const int MinimumCaptureDimension = 2;
    private const int CaptureFramerate = 15;
    private const int MaxDiagnosticLines = 8;

    private readonly string _workspaceRoot;
    private readonly string _metadataPath;
    private readonly GuiSmokeVideoRecordingMetadata _metadata;
    private readonly GuiSmokeFfmpegCaptureSupport? _captureSupportOverride;
    private readonly List<string> _diagnosticLines = new();
    private readonly object _diagnosticLock = new();
    private string? _ffmpegProcessPath;
    private Process? _process;
    private bool _completed;
    private bool _disposed;

    private GuiSmokeVideoRecorder(
        string workspaceRoot,
        string rootPath,
        GuiSmokeVideoRecordingMetadata metadata,
        GuiSmokeFfmpegCaptureSupport? captureSupportOverride)
    {
        _workspaceRoot = workspaceRoot;
        _metadataPath = Path.Combine(rootPath, "video-recording.json");
        _metadata = metadata;
        _captureSupportOverride = captureSupportOverride;
        PersistMetadata();
    }

    public static GuiSmokeVideoRecorder Create(
        string workspaceRoot,
        IReadOnlyDictionary<string, string> options,
        string sessionId,
        string runId,
        string rootPath,
        string sessionRoot,
        string? attemptId,
        string scopeKind,
        GuiSmokeFfmpegCaptureSupport? captureSupportOverride = null)
    {
        Directory.CreateDirectory(rootPath);
        var outputPath = Path.Combine(rootPath, "video.review.mkv");
        var metadata = new GuiSmokeVideoRecordingMetadata(
            scopeKind,
            sessionId,
            runId,
            rootPath,
            sessionRoot,
            outputPath)
        {
            AttemptId = attemptId,
        };
        var recorder = new GuiSmokeVideoRecorder(workspaceRoot, rootPath, metadata, captureSupportOverride);
        recorder.InitializeAvailability(options);
        return recorder;
    }

    public bool TryStart(WindowCaptureTarget? target)
    {
        if (_completed || _process is not null)
        {
            return false;
        }

        if (string.Equals(_metadata.Status, "skipped", StringComparison.OrdinalIgnoreCase)
            || string.Equals(_metadata.Status, "start-failed", StringComparison.OrdinalIgnoreCase))
        {
            PersistMetadata();
            LogVideo($"video scope={_metadata.ScopeKind} start skipped status={_metadata.Status} reason={_metadata.SkipReason ?? "none"}");
            return false;
        }

        if (target is null)
        {
            MarkSkipped("window-not-found");
            return false;
        }

        var bounds = NormalizeCaptureBounds(target.Bounds);
        if (bounds.Width < MinimumCaptureDimension || bounds.Height < MinimumCaptureDimension)
        {
            MarkSkipped("window-bounds-invalid");
            return false;
        }

        var inputPattern = TryBuildWindowCaptureInputPattern(target);
        if (string.IsNullOrWhiteSpace(inputPattern))
        {
            MarkSkipped("window-input-unavailable");
            return false;
        }

        _metadata.WindowScopedCaptureRequested = !target.IsFallback;
        _metadata.CaptureMode = inputPattern.StartsWith("hwnd=", StringComparison.OrdinalIgnoreCase)
            ? "window-hwnd"
            : "window-title";
        _metadata.CaptureInputPattern = inputPattern;
        _metadata.CaptureModeNote = "Single-window ffmpeg gdigrab capture of the detected game window.";
        _metadata.WindowTitle = target.Title;
        _metadata.CaptureBounds = new WindowBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);

        var outputProcessPath = GetProcessCompatiblePath(_metadata.OutputPath);
        var workingDirectory = GetProcessCompatiblePath(_workspaceRoot);
        var executablePath = _metadata.FfmpegPath!;
        var commandPath = _ffmpegProcessPath ?? executablePath;
        var arguments = BuildFfmpegArguments(inputPattern, outputProcessPath);
        _metadata.CommandLine = QuoteCommand(commandPath, arguments);
        try
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                },
            };
            _process.OutputDataReceived += (_, eventArgs) => RecordDiagnosticLine(eventArgs.Data);
            _process.ErrorDataReceived += (_, eventArgs) => RecordDiagnosticLine(eventArgs.Data);
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            _metadata.Status = "recording";
            _metadata.RecordingStartedAt = DateTimeOffset.UtcNow;
            PersistMetadata();
            LogVideo($"video scope={_metadata.ScopeKind} recording started ffmpeg={_metadata.FfmpegPath ?? "null"} output={_metadata.OutputPath}");
            return true;
        }
        catch (Exception exception)
        {
            _metadata.Status = "start-failed";
            _metadata.SkipReason = $"ffmpeg-start-failed:{exception.GetType().Name}:{SanitizeText(exception.Message)}";
            PersistMetadata();
            LogVideo($"video scope={_metadata.ScopeKind} start failed reason={_metadata.SkipReason}");
            _process?.Dispose();
            _process = null;
            return false;
        }
    }

    public void Complete(bool keepRecording, string completionReason)
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        _metadata.CompletionReason = completionReason;
        _metadata.RecordingEndedAt = DateTimeOffset.UtcNow;

        if (_process is not null)
        {
            StopProcess();
        }

        if (_process is not null)
        {
            _metadata.ExitCode = _process.HasExited ? _process.ExitCode : null;
            _process.Dispose();
            _process = null;
        }

        var outputExists = File.Exists(_metadata.OutputPath);
        if (string.Equals(_metadata.Status, "recording", StringComparison.OrdinalIgnoreCase))
        {
            if (outputExists && keepRecording)
            {
                _metadata.Status = "kept";
                _metadata.Kept = true;
            }
            else if (outputExists)
            {
                if (TryDeleteOutput())
                {
                    _metadata.Status = "deleted";
                    _metadata.Kept = false;
                }
            }
            else
            {
                _metadata.Status = keepRecording ? "kept-missing-output" : "deleted-missing-output";
                _metadata.Kept = keepRecording;
            }
        }
        else if (string.Equals(_metadata.Status, "pending", StringComparison.OrdinalIgnoreCase))
        {
            _metadata.Status = "skipped";
            _metadata.SkipReason ??= "recording-never-started";
            _metadata.Kept = false;
        }
        else if (!string.Equals(_metadata.Status, "kept", StringComparison.OrdinalIgnoreCase)
                 && !string.Equals(_metadata.Status, "deleted", StringComparison.OrdinalIgnoreCase))
        {
            _metadata.Kept ??= false;
        }

        AppendDiagnosticSummary();
        PersistMetadata();
        LogVideo($"video scope={_metadata.ScopeKind} completed status={_metadata.Status} kept={_metadata.Kept?.ToString() ?? "null"} reason={_metadata.CompletionReason ?? "none"} skipReason={_metadata.SkipReason ?? "none"} exitCode={_metadata.ExitCode?.ToString(CultureInfo.InvariantCulture) ?? "null"} captureMode={_metadata.CaptureMode ?? "null"}");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (!_completed)
        {
            Complete(keepRecording: false, completionReason: "disposed-without-explicit-complete");
        }
    }

    private void InitializeAvailability(IReadOnlyDictionary<string, string> options)
    {
        if (options.ContainsKey("--disable-video-capture"))
        {
            _metadata.Status = "skipped";
            _metadata.SkipReason = "video-capture-disabled";
            PersistMetadata();
            LogVideo($"video scope={_metadata.ScopeKind} availability skipped reason={_metadata.SkipReason}");
            return;
        }

        var hasExplicitFfmpegOverride = options.ContainsKey("--ffmpeg-path")
            || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("STS2_GUI_SMOKE_FFMPEG_PATH"));
        var ffmpegPath = ResolveFfmpegPath(options, _workspaceRoot);
        if (ffmpegPath is null)
        {
            _metadata.Status = "skipped";
            _metadata.FfmpegAvailable = false;
            _metadata.SkipReason = "ffmpeg-not-found";
            PersistMetadata();
            LogVideo($"video scope={_metadata.ScopeKind} availability skipped reason={_metadata.SkipReason}");
            return;
        }

        if (!TryBindCaptureReadyFfmpeg(ffmpegPath))
        {
            if (!hasExplicitFfmpegOverride)
            {
                var attemptedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ffmpegPath.HostPath,
                };
                foreach (var fallbackProbePath in GetDefaultFfmpegFallbackProbePaths())
                {
                    var alternateBinding = ResolveExecutablePath(fallbackProbePath, _workspaceRoot);
                    if (alternateBinding is null || !attemptedPaths.Add(alternateBinding.HostPath))
                    {
                        continue;
                    }

                    if (TryBindCaptureReadyFfmpeg(alternateBinding))
                    {
                        PersistMetadata();
                        LogVideo($"video scope={_metadata.ScopeKind} availability ready ffmpeg={_metadata.FfmpegPath}");
                        return;
                    }
                }
            }

            _metadata.Status = "skipped";
            _metadata.FfmpegAvailable = false;
            _metadata.SkipReason = "ffmpeg-missing-gdigrab";
            PersistMetadata();
            LogVideo($"video scope={_metadata.ScopeKind} availability skipped reason={_metadata.SkipReason} ffmpeg={_metadata.FfmpegPath ?? "null"}");
            return;
        }

        PersistMetadata();
        LogVideo($"video scope={_metadata.ScopeKind} availability ready ffmpeg={_metadata.FfmpegPath}");
    }

    private static VideoPathBinding? ResolveFfmpegPath(IReadOnlyDictionary<string, string> options, string workspaceRoot)
    {
        return ResolveFfmpegPath(options, workspaceRoot, envPathOverride: null, pathOverride: null, fallbackProbePaths: null);
    }

    private static VideoPathBinding? ResolveFfmpegPath(
        IReadOnlyDictionary<string, string> options,
        string workspaceRoot,
        string? envPathOverride,
        string? pathOverride,
        IReadOnlyList<string>? fallbackProbePaths)
    {
        if (options.TryGetValue("--ffmpeg-path", out var explicitPath))
        {
            return ResolveExecutablePath(explicitPath, workspaceRoot);
        }

        var envPath = envPathOverride ?? Environment.GetEnvironmentVariable("STS2_GUI_SMOKE_FFMPEG_PATH");
        if (!string.IsNullOrWhiteSpace(envPath))
        {
            return ResolveExecutablePath(envPath, workspaceRoot);
        }

        var pathValue = pathOverride ?? Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            pathValue = string.Empty;
        }

        foreach (var pathEntry in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            foreach (var candidateName in new[] { "ffmpeg.exe", "ffmpeg" })
            {
                try
                {
                    var candidatePath = Path.Combine(pathEntry, candidateName);
                    if (File.Exists(candidatePath))
                    {
                        var fullHostPath = Path.GetFullPath(candidatePath);
                        return new VideoPathBinding(fullHostPath, GetProcessCompatiblePath(fullHostPath));
                    }
                }
                catch
                {
                }
            }
        }

        foreach (var fallbackProbePath in fallbackProbePaths ?? GetDefaultFfmpegFallbackProbePaths())
        {
            var binding = ResolveExecutablePath(fallbackProbePath, workspaceRoot);
            if (binding is not null)
            {
                return binding;
            }
        }

        return null;
    }

    internal static VideoPathBinding? ResolveExecutablePathForSelfTest(string explicitPath, string workspaceRoot)
    {
        return ResolveExecutablePath(explicitPath, workspaceRoot);
    }

    internal static VideoPathBinding? ResolveFfmpegPathForSelfTest(
        IReadOnlyDictionary<string, string> options,
        string workspaceRoot,
        string? envPathOverride,
        string? pathOverride,
        IReadOnlyList<string>? fallbackProbePaths)
    {
        return ResolveFfmpegPath(options, workspaceRoot, envPathOverride, pathOverride, fallbackProbePaths);
    }

    private static Rectangle NormalizeCaptureBounds(Rectangle bounds)
    {
        var width = Math.Max(MinimumCaptureDimension, bounds.Width);
        var height = Math.Max(MinimumCaptureDimension, bounds.Height);
        if (width % 2 != 0)
        {
            width -= 1;
        }

        if (height % 2 != 0)
        {
            height -= 1;
        }

        width = Math.Max(MinimumCaptureDimension, width);
        height = Math.Max(MinimumCaptureDimension, height);
        return new Rectangle(bounds.X, bounds.Y, width, height);
    }

    private static string? TryBuildWindowCaptureInputPattern(WindowCaptureTarget target)
    {
        if (target.Handle != IntPtr.Zero)
        {
            return $"hwnd={target.Handle.ToInt64().ToString(CultureInfo.InvariantCulture)}";
        }

        if (!string.IsNullOrWhiteSpace(target.Title))
        {
            return $"title={target.Title}";
        }

        return null;
    }

    private bool TryBindCaptureReadyFfmpeg(VideoPathBinding ffmpegPath)
    {
        _metadata.FfmpegPath = ffmpegPath.HostPath;
        _ffmpegProcessPath = ffmpegPath.ProcessPath;

        var captureSupport = _captureSupportOverride ?? ProbeCaptureSupport(ffmpegPath);
        if (!captureSupport.SupportsGdigrab)
        {
            _metadata.FfmpegAvailable = false;
            RecordDiagnosticLine($"ffmpeg path={ffmpegPath.HostPath} missing gdigrab support");
            return false;
        }

        _metadata.FfmpegAvailable = true;
        _metadata.SkipReason = null;
        return true;
    }

    private static GuiSmokeFfmpegCaptureSupport ProbeCaptureSupport(VideoPathBinding ffmpegPath)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath.HostPath,
                    Arguments = "-hide_banner -devices",
                    WorkingDirectory = Path.GetDirectoryName(ffmpegPath.HostPath) ?? Environment.CurrentDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
            };
            process.Start();
            var standardOutput = process.StandardOutput.ReadToEnd();
            var standardError = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(StopTimeoutMs))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                }

                return new GuiSmokeFfmpegCaptureSupport(false);
            }

            return new GuiSmokeFfmpegCaptureSupport(
                standardOutput.Contains("gdigrab", StringComparison.OrdinalIgnoreCase)
                || standardError.Contains("gdigrab", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return new GuiSmokeFfmpegCaptureSupport(false);
        }
    }

    private static string BuildFfmpegArguments(string inputPattern, string outputPath)
    {
        return string.Join(
            " ",
            "-hide_banner",
            "-loglevel error",
            "-nostats",
            "-y",
            "-f gdigrab",
            $"-framerate {CaptureFramerate.ToString(CultureInfo.InvariantCulture)}",
            "-draw_mouse 0",
            "-i",
            QuoteArgument(inputPattern),
            "-c:v libx264",
            "-preset ultrafast",
            "-pix_fmt yuv420p",
            QuoteArgument(outputPath));
    }

    private static string QuoteArgument(string value)
    {
        return $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
    }

    private static string QuoteCommand(string fileName, string arguments)
    {
        return $"{QuoteArgument(fileName)} {arguments}";
    }

    private static IEnumerable<string> GetDefaultFfmpegFallbackProbePaths()
    {
        foreach (var wingetCandidate in GetWingetFfmpegFallbackProbePaths())
        {
            yield return wingetCandidate;
        }

        yield return @"C:\Program Files\SteelSeries\GG\apps\moments\ffmpeg.exe";
        yield return @"C:\Program Files\ffmpeg\bin\ffmpeg.exe";
        yield return @"C:\ffmpeg\bin\ffmpeg.exe";
    }

    private static IEnumerable<string> GetWingetFfmpegFallbackProbePaths()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
        {
            yield break;
        }

        var packagesRoot = Path.Combine(localAppData, "Microsoft", "WinGet", "Packages");
        if (!Directory.Exists(packagesRoot))
        {
            yield break;
        }

        IEnumerable<string> packageDirectories;
        try
        {
            packageDirectories = Directory.EnumerateDirectories(packagesRoot, "*FFmpeg*", SearchOption.TopDirectoryOnly)
                .Concat(Directory.EnumerateDirectories(packagesRoot, "*ffmpeg*", SearchOption.TopDirectoryOnly))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path.IndexOf("Gyan.FFmpeg", StringComparison.OrdinalIgnoreCase) >= 0 ? 0 : 1)
                .ThenBy(path => path.IndexOf("BtbN.FFmpeg", StringComparison.OrdinalIgnoreCase) >= 0 ? 0 : 1)
                .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch
        {
            yield break;
        }

        var yielded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var packageDirectory in packageDirectories)
        {
            IEnumerable<string> candidates;
            try
            {
                candidates = Directory.EnumerateFiles(packageDirectory, "ffmpeg.exe", SearchOption.AllDirectories)
                    .OrderBy(path => path.IndexOf("essentials_build", StringComparison.OrdinalIgnoreCase) >= 0 ? 0 : 1)
                    .ThenBy(path => path.IndexOf($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}ffmpeg.exe", StringComparison.OrdinalIgnoreCase) >= 0 ? 0 : 1)
                    .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch
            {
                continue;
            }

            foreach (var candidate in candidates)
            {
                if (yielded.Add(candidate))
                {
                    yield return candidate;
                }
            }
        }
    }

    private void MarkSkipped(string reason)
    {
        _metadata.Status = "skipped";
        _metadata.SkipReason = reason;
        _metadata.Kept = false;
        PersistMetadata();
        LogVideo($"video scope={_metadata.ScopeKind} skipped reason={reason}");
    }

    private void StopProcess()
    {
        if (_process is null)
        {
            return;
        }

        try
        {
            if (!_process.HasExited)
            {
                _process.StandardInput.WriteLine("q");
                _process.StandardInput.Flush();
            }
        }
        catch
        {
        }

        try
        {
            if (!_process.WaitForExit(StopTimeoutMs) && !_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                _process.WaitForExit(StopTimeoutMs);
            }
        }
        catch
        {
            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
            }
        }
    }

    private bool TryDeleteOutput()
    {
        try
        {
            if (File.Exists(_metadata.OutputPath))
            {
                File.Delete(_metadata.OutputPath);
            }

            return true;
        }
        catch (Exception exception)
        {
            _metadata.Status = "kept-delete-failed";
            _metadata.Kept = true;
            _metadata.CompletionReason = $"{_metadata.CompletionReason};delete-failed:{exception.GetType().Name}:{SanitizeText(exception.Message)}";
            return false;
        }
    }

    private void ApplyOutputDisposition(bool keepRecording)
    {
        if (File.Exists(_metadata.OutputPath) && keepRecording)
        {
            _metadata.Status = "kept";
            _metadata.Kept = true;
            return;
        }

        if (File.Exists(_metadata.OutputPath))
        {
            if (TryDeleteOutput())
            {
                _metadata.Status = "deleted";
                _metadata.Kept = false;
            }

            return;
        }

        ApplyMissingOutputDisposition(keepRecording);
    }

    private void ApplyMissingOutputDisposition(bool keepRecording)
    {
        _metadata.Status = keepRecording ? "kept-missing-output" : "deleted-missing-output";
        _metadata.Kept = keepRecording;
    }

    private void RecordDiagnosticLine(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        var sanitized = SanitizeText(line);
        lock (_diagnosticLock)
        {
            if (_diagnosticLines.Count >= MaxDiagnosticLines)
            {
                return;
            }

            _diagnosticLines.Add(sanitized);
        }
    }

    private void AppendDiagnosticSummary()
    {
        string[] lines;
        lock (_diagnosticLock)
        {
            if (_diagnosticLines.Count == 0)
            {
                var exitCode = _metadata.ExitCode;
                if ((exitCode ?? 0) != 0
                    && !string.IsNullOrWhiteSpace(_metadata.CompletionReason)
                    && !_metadata.CompletionReason.Contains("ffmpeg-exit-code:", StringComparison.OrdinalIgnoreCase))
                {
                    _metadata.CompletionReason = $"{_metadata.CompletionReason};ffmpeg-exit-code:{exitCode!.Value.ToString(CultureInfo.InvariantCulture)}";
                }

                return;
            }

            lines = _diagnosticLines.ToArray();
        }

        var diagnosticSummary = string.Join(" | ", lines);
        var nonZeroExitCode = _metadata.ExitCode;
        if ((nonZeroExitCode ?? 0) != 0)
        {
            diagnosticSummary = $"exit={nonZeroExitCode!.Value.ToString(CultureInfo.InvariantCulture)} {diagnosticSummary}";
        }

        if (string.IsNullOrWhiteSpace(_metadata.CompletionReason))
        {
            _metadata.CompletionReason = $"ffmpeg:{diagnosticSummary}";
        }
        else if (!_metadata.CompletionReason.Contains(diagnosticSummary, StringComparison.Ordinal))
        {
            _metadata.CompletionReason = $"{_metadata.CompletionReason};ffmpeg:{diagnosticSummary}";
        }
    }

    private static void LogVideo(string message)
    {
        var line = $"[gui-smoke {DateTimeOffset.Now:HH:mm:ss}] {message}";
        Console.WriteLine(line);
        GuiSmokeShared.HarnessLogSink?.Invoke(line);
    }

    private void PersistMetadata()
    {
        var directory = Path.GetDirectoryName(_metadataPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_metadataPath, JsonSerializer.Serialize(_metadata, GuiSmokeShared.JsonOptions));
    }

    private static VideoPathBinding? ResolveExecutablePath(string explicitPath, string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(explicitPath))
        {
            throw new InvalidOperationException("A non-empty ffmpeg path is required.");
        }

        if (TryNormalizeWindowsPath(explicitPath, out var processPath))
        {
            return TryBindExecutable(processPath, fallbackHostPath: null);
        }

        if (TryConvertHostPathToWindows(explicitPath, out processPath))
        {
            return TryBindExecutable(processPath, fallbackHostPath: explicitPath);
        }

        var fullPath = Path.IsPathRooted(explicitPath)
            ? Path.GetFullPath(explicitPath)
            : Path.GetFullPath(explicitPath, workspaceRoot);
        if (TryConvertHostPathToWindows(fullPath, out processPath))
        {
            return TryBindExecutable(processPath, fallbackHostPath: fullPath);
        }

        return File.Exists(fullPath)
            ? new VideoPathBinding(fullPath, fullPath)
            : null;
    }

    private static VideoPathBinding? TryBindExecutable(string processPath, string? fallbackHostPath)
    {
        foreach (var candidate in EnumerateExecutableHostCandidates(processPath, fallbackHostPath))
        {
            try
            {
                var fullHostPath = Path.GetFullPath(candidate);
                if (File.Exists(fullHostPath))
                {
                    return new VideoPathBinding(fullHostPath, processPath);
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateExecutableHostCandidates(string processPath, string? fallbackHostPath)
    {
        if (OperatingSystem.IsWindows())
        {
            yield return processPath;
        }

        if (!string.IsNullOrWhiteSpace(fallbackHostPath))
        {
            yield return fallbackHostPath;
        }

        if (TryConvertWindowsPathToWslHost(processPath, out var translatedHostPath))
        {
            yield return translatedHostPath;
        }
    }

    private static string SanitizeText(string? value)
    {
        return (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Trim();
    }

    private static string GetProcessCompatiblePath(string path)
    {
        if (TryConvertHostPathToWindows(path, out var translatedPath))
        {
            return translatedPath;
        }

        return Path.GetFullPath(path);
    }

    private static string NormalizeWindowsProcessPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        return path.Replace('/', '\\');
    }

    private static bool TryNormalizeWindowsPath(string path, out string translatedPath)
    {
        translatedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        if (path.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase)
            || !(path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':'))
        {
            return false;
        }

        var driveLetter = char.ToUpperInvariant(path[0]);
        var suffix = path[2..].Replace('/', '\\');
        translatedPath = $"{driveLetter}:{suffix}";
        return true;
    }

    private static bool TryConvertWindowsPathToWslHost(string path, out string translatedPath)
    {
        translatedPath = string.Empty;
        if (!TryNormalizeWindowsPath(path, out var normalizedWindowsPath))
        {
            return false;
        }

        var driveLetter = char.ToLowerInvariant(normalizedWindowsPath[0]);
        var suffix = normalizedWindowsPath[2..].Replace('\\', '/');
        translatedPath = $"/mnt/{driveLetter}{suffix}";
        return true;
    }

    private static bool TryConvertHostPathToWindows(string path, out string translatedPath)
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
        if (!normalized.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase)
            || normalized.Length < 7
            || !char.IsLetter(normalized[5])
            || normalized[6] != '/')
        {
            return false;
        }

        var driveLetter = char.ToUpperInvariant(normalized[5]);
        translatedPath = $"{driveLetter}:{normalized[6..].Replace('/', '\\')}";
        return true;
    }
}
