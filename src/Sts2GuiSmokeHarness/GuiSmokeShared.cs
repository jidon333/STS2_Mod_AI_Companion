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

static class GuiSmokeShared
{
    public static Action<string>? HarnessLogSink { get; set; }

    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static JsonSerializerOptions NdjsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static async Task RunProcessAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        TimeSpan timeout,
        bool waitForExit = true)
    {
        var result = await RunProcessDetailedAsync(fileName, arguments, workingDirectory, timeout, waitForExit).ConfigureAwait(false);
        if (!waitForExit)
        {
            return;
        }

        if (result.TimedOut)
        {
            throw new TimeoutException(
                $"Process timed out after {timeout}: {fileName} {arguments}{Environment.NewLine}stdout:{Environment.NewLine}{result.Stdout}{Environment.NewLine}stderr:{Environment.NewLine}{result.Stderr}");
        }

        if (!string.IsNullOrWhiteSpace(result.FailureKind))
        {
            throw new InvalidOperationException(
                $"Process failed before exit: {result.FailureKind} ({result.ExceptionType}: {result.ExceptionMessage}) {fileName} {arguments}{Environment.NewLine}stdout:{Environment.NewLine}{result.Stdout}{Environment.NewLine}stderr:{Environment.NewLine}{result.Stderr}");
        }

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Process failed with exit code {result.ExitCode}: {fileName} {arguments}{Environment.NewLine}stdout:{Environment.NewLine}{result.Stdout}{Environment.NewLine}stderr:{Environment.NewLine}{result.Stderr}");
        }
    }

    public static async Task<GuiSmokeProcessExecutionResult> RunProcessDetailedAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        TimeSpan timeout,
        bool waitForExit = true)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = waitForExit,
                RedirectStandardError = waitForExit,
                CreateNoWindow = true,
            },
        };

        var stopwatch = Stopwatch.StartNew();
        try
        {
            process.Start();
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(
                fileName,
                arguments,
                null,
                false,
                stopwatch.Elapsed,
                string.Empty,
                string.Empty,
                "process-start-failure",
                exception.GetType().Name,
                exception.Message);
        }

        if (!waitForExit)
        {
            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(fileName, arguments, null, false, stopwatch.Elapsed, string.Empty, string.Empty, null, null, null);
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        using var timeoutCts = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
            }

            try
            {
                await process.WaitForExitAsync().ConfigureAwait(false);
            }
            catch
            {
            }

            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(
                fileName,
                arguments,
                process.HasExited ? process.ExitCode : null,
                true,
                stopwatch.Elapsed,
                await stdoutTask.ConfigureAwait(false),
                await stderrTask.ConfigureAwait(false),
                null,
                null,
                null);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(
                fileName,
                arguments,
                process.HasExited ? process.ExitCode : null,
                false,
                stopwatch.Elapsed,
                await TryReadProcessOutputAsync(stdoutTask).ConfigureAwait(false),
                await TryReadProcessOutputAsync(stderrTask).ConfigureAwait(false),
                "process-wait-failure",
                exception.GetType().Name,
                exception.Message);
        }

        string stdout;
        string stderr;
        try
        {
            stdout = await stdoutTask.ConfigureAwait(false);
            stderr = await stderrTask.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return new GuiSmokeProcessExecutionResult(
                fileName,
                arguments,
                process.ExitCode,
                false,
                stopwatch.Elapsed,
                await TryReadProcessOutputAsync(stdoutTask).ConfigureAwait(false),
                await TryReadProcessOutputAsync(stderrTask).ConfigureAwait(false),
                "output-drain-failure",
                exception.GetType().Name,
                exception.Message);
        }

        stopwatch.Stop();
        return new GuiSmokeProcessExecutionResult(
            fileName,
            arguments,
            process.ExitCode,
            false,
            stopwatch.Elapsed,
            stdout,
            stderr,
            null,
            null,
            null);
    }

    private static async Task<string> TryReadProcessOutputAsync(Task<string>? task)
    {
        if (task is null)
        {
            return string.Empty;
        }

        try
        {
            return await task.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            return $"<output-read-failed:{exception.GetType().Name}:{exception.Message}>";
        }
    }
}
