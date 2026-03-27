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
        Func<WindowCaptureTarget, Bitmap?>? captureOverride = null)
    {
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
