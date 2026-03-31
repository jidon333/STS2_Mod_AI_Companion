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

sealed class ScreenCaptureService
{
    internal static readonly TimeSpan CaptureTimeout = TimeSpan.FromSeconds(15);
    private readonly CaptureFaultInjectionOptions? _faultInjection;
    private bool _faultConsumed;

    public ScreenCaptureService(CaptureFaultInjectionOptions? faultInjection = null)
    {
        _faultInjection = faultInjection;
    }

    internal static int GetCaptureRetryDelayMs(int attempt)
    {
        return attempt switch
        {
            0 => 0,
            1 => 200,
            2 => 350,
            _ => 500,
        };
    }

    public bool TryCapture(WindowCaptureTarget target, string outputPath)
    {
        return TryCaptureDetailed(target, outputPath, CaptureTimeout).Succeeded;
    }

    internal CaptureBoundaryResult TryCaptureDetailed(
        WindowCaptureTarget target,
        string outputPath,
        TimeSpan timeout,
        Func<WindowCaptureTarget, Bitmap?>? captureOverride = null,
        CaptureFaultInjectionContext? faultContext = null)
    {
        if (TryInjectFault(faultContext) is { } injectedFault)
        {
            return injectedFault;
        }

        try
        {
            Bitmap? bitmap;
            try
            {
                bitmap = Task.Factory.StartNew(
                        () => (captureOverride ?? TryCaptureProcessWindow)(target),
                        CancellationToken.None,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default)
                    .WaitAsync(timeout)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (TimeoutException exception)
            {
                return new CaptureBoundaryResult(
                    false,
                    CaptureBoundaryFailureKind.TimedOut,
                    exception.Message,
                    exception);
            }
            catch (Exception exception)
            {
                return new CaptureBoundaryResult(
                    false,
                    CaptureBoundaryFailureKind.Exception,
                    exception.Message,
                    exception);
            }

            if (bitmap is null)
            {
                return new CaptureBoundaryResult(
                    false,
                    CaptureBoundaryFailureKind.UnusableFrame,
                    "capture returned no bitmap");
            }

            using (bitmap)
            {
                if (IsMostlyBlack(bitmap))
                {
                    return new CaptureBoundaryResult(
                        false,
                        CaptureBoundaryFailureKind.UnusableFrame,
                        "capture returned a mostly black bitmap");
                }

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                bitmap.Save(outputPath, ImageFormat.Png);
            }

            return new CaptureBoundaryResult(true, CaptureBoundaryFailureKind.None, null);
        }
        catch (Exception exception)
        {
            return new CaptureBoundaryResult(
                false,
                CaptureBoundaryFailureKind.Exception,
                exception.Message,
                exception);
        }
    }

    internal bool ShouldForceCapture(string scopeKind, GuiSmokePhase phase, int stepIndex, int? attemptOrdinal)
    {
        if (_faultInjection is null || _faultConsumed)
        {
            return false;
        }

        if (!string.Equals(_faultInjection.ScopeKind, scopeKind, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(_faultInjection.PhaseName)
            && !string.Equals(_faultInjection.PhaseName, phase.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (_faultInjection.AttemptOrdinal is not null && _faultInjection.AttemptOrdinal != attemptOrdinal)
        {
            return false;
        }

        return _faultInjection.StepIndex is null || _faultInjection.StepIndex == stepIndex;
    }

    private CaptureBoundaryResult? TryInjectFault(CaptureFaultInjectionContext? faultContext)
    {
        if (_faultInjection is null || _faultConsumed || faultContext is null)
        {
            return null;
        }

        if (!string.Equals(_faultInjection.ScopeKind, faultContext.ScopeKind, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(_faultInjection.PhaseName)
            && !string.Equals(_faultInjection.PhaseName, faultContext.PhaseName, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (_faultInjection.StepIndex is not null && _faultInjection.StepIndex != faultContext.StepIndex)
        {
            return null;
        }

        if (_faultInjection.AttemptOrdinal is not null && _faultInjection.AttemptOrdinal != faultContext.AttemptOrdinal)
        {
            return null;
        }

        _faultConsumed = true;
        return _faultInjection.FailureKind switch
        {
            CaptureBoundaryFailureKind.UnusableFrame => new CaptureBoundaryResult(
                false,
                CaptureBoundaryFailureKind.UnusableFrame,
                "capture-fault-injected: unusable-frame"),
            CaptureBoundaryFailureKind.TimedOut => new CaptureBoundaryResult(
                false,
                CaptureBoundaryFailureKind.TimedOut,
                "capture-fault-injected: timeout",
                new TimeoutException("capture-fault-injected-timeout")),
            CaptureBoundaryFailureKind.Exception => new CaptureBoundaryResult(
                false,
                CaptureBoundaryFailureKind.Exception,
                "capture-fault-injected: exception",
                new InvalidOperationException("capture-fault-injected-exception")),
            _ => null,
        };
    }

    private static Bitmap? TryCaptureProcessWindow(WindowCaptureTarget target)
    {
        if (target.IsFallback || target.Handle == IntPtr.Zero)
        {
            return null;
        }

        Bitmap? lastCapture = null;
        for (var attempt = 0; attempt < 3; attempt += 1)
        {
            target = WindowLocator.EnsureInteractive(target);
            lastCapture?.Dispose();
            lastCapture = TryCaptureWindowClient(target);
            if (lastCapture is not null && !IsMostlyBlack(lastCapture))
            {
                return lastCapture;
            }

            var retryDelayMs = GetCaptureRetryDelayMs(attempt + 1);
            if (retryDelayMs > 0)
            {
                Thread.Sleep(retryDelayMs);
            }
        }

        lastCapture?.Dispose();
        return null;
    }

    private static Bitmap? TryCaptureWindowClient(WindowCaptureTarget target)
    {
        if (target.IsFallback || target.Handle == IntPtr.Zero)
        {
            return null;
        }

        return NativeMethods.TryCaptureClientArea(target.Handle, target.Bounds, out var bitmap)
            ? bitmap
            : null;
    }

    private static bool IsMostlyBlack(Bitmap bitmap)
    {
        var sampleColumns = 12;
        var sampleRows = 8;
        var brightSamples = 0;
        var totalSamples = 0;

        for (var row = 0; row < sampleRows; row += 1)
        {
            var y = (int)Math.Round((bitmap.Height - 1) * (row / (double)Math.Max(1, sampleRows - 1)));
            for (var column = 0; column < sampleColumns; column += 1)
            {
                var x = (int)Math.Round((bitmap.Width - 1) * (column / (double)Math.Max(1, sampleColumns - 1)));
                var pixel = bitmap.GetPixel(x, y);
                var brightness = (pixel.R + pixel.G + pixel.B) / 3.0;
                if (brightness >= 12)
                {
                    brightSamples += 1;
                }

                totalSamples += 1;
            }
        }

        return brightSamples <= Math.Max(1, totalSamples / 20);
    }
}
