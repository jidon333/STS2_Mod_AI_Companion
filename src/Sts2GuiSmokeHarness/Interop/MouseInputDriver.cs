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

sealed class MouseInputDriver
{
    private const int HoverPrimeDelayMs = 90;

    private static void PrepareWindow(WindowCaptureTarget target, int delayMs)
    {
        if (target.Handle == IntPtr.Zero)
        {
            return;
        }

        if (NativeMethods.GetForegroundWindow() == target.Handle)
        {
            return;
        }

        NativeMethods.SetForegroundWindow(target.Handle);
        Thread.Sleep(Math.Min(delayMs, 80));
    }

    public void MoveCursor(WindowCaptureTarget target, double normalizedX, double normalizedY)
    {
        PrepareWindow(target, 100);

        var point = TransformNormalizedPoint(target, normalizedX, normalizedY);
        NativeMethods.SetCursorPos(point.X, point.Y);
    }

    public void Click(WindowCaptureTarget target, double normalizedX, double normalizedY)
    {
        PrepareWindow(target, 200);

        var point = TransformNormalizedPoint(target, normalizedX, normalizedY);
        NativeMethods.SetCursorPos(point.X, point.Y);
        var inputs = new[]
        {
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTDOWN),
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTUP),
        };
        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException("Failed to send mouse input.");
        }
    }

    public void HoverPrimedClick(WindowCaptureTarget target, double normalizedX, double normalizedY)
    {
        PrepareWindow(target, 200);

        var point = TransformNormalizedPoint(target, normalizedX, normalizedY);
        NativeMethods.SetCursorPos(point.X, point.Y);
        Thread.Sleep(HoverPrimeDelayMs);
        NativeMethods.SetCursorPos(point.X, point.Y);
        var inputs = new[]
        {
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTDOWN),
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTUP),
        };
        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException("Failed to send hover-primed mouse input.");
        }
    }

    public void ClickCurrent(WindowCaptureTarget target)
    {
        PrepareWindow(target, 200);

        var inputs = new[]
        {
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTDOWN),
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTUP),
        };
        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException("Failed to send current-cursor mouse input.");
        }
    }

    public void ConfirmNonEnemy(WindowCaptureTarget target, double normalizedX, double normalizedY, int holdMs)
    {
        PrepareWindow(target, 200);
        var point = TransformNormalizedPoint(target, normalizedX, normalizedY);
        NativeMethods.SetCursorPos(point.X, point.Y);

        var down = new[]
        {
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTDOWN),
        };
        var sentDown = NativeMethods.SendInput((uint)down.Length, down, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sentDown != down.Length)
        {
            throw new InvalidOperationException("Failed to send non-enemy confirm mouse-down input.");
        }

        Thread.Sleep(Math.Max(holdMs, 16));

        var up = new[]
        {
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTUP),
        };
        var sentUp = NativeMethods.SendInput((uint)up.Length, up, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sentUp != up.Length)
        {
            throw new InvalidOperationException("Failed to send non-enemy confirm mouse-up input.");
        }
    }

    public void ConfirmAttackCard(WindowCaptureTarget target, double normalizedX, double normalizedY, int primeMs, int holdMs)
    {
        PrepareWindow(target, 200);

        var point = TransformNormalizedPoint(target, normalizedX, normalizedY);
        NativeMethods.SetCursorPos(point.X, point.Y);
        Thread.Sleep(Math.Max(primeMs, 16));

        var down = new[]
        {
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTDOWN),
        };
        var sentDown = NativeMethods.SendInput((uint)down.Length, down, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sentDown != down.Length)
        {
            throw new InvalidOperationException("Failed to send attack confirm mouse-down input.");
        }

        // Shortcut-started multi-target card play is sampled across process frames.
        Thread.Sleep(Math.Max(holdMs, 16));

        var up = new[]
        {
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_LEFTUP),
        };
        var sentUp = NativeMethods.SendInput((uint)up.Length, up, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sentUp != up.Length)
        {
            throw new InvalidOperationException("Failed to send attack confirm mouse-up input.");
        }
    }

    public void RightClick(WindowCaptureTarget target, double normalizedX, double normalizedY)
    {
        PrepareWindow(target, 200);

        var point = TransformNormalizedPoint(target, normalizedX, normalizedY);
        NativeMethods.SetCursorPos(point.X, point.Y);
        var inputs = new[]
        {
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_RIGHTDOWN),
            NativeMethods.CreateMouseInput(0, 0, NativeMethods.MOUSEEVENTF_RIGHTUP),
        };
        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException("Failed to send right-click mouse input.");
        }
    }

    public static Point TransformNormalizedPoint(WindowCaptureTarget target, double normalizedX, double normalizedY)
    {
        var clampedX = Math.Clamp(normalizedX, 0d, 1d);
        var clampedY = Math.Clamp(normalizedY, 0d, 1d);
        var pixelX = target.Bounds.X + (int)Math.Round((target.Bounds.Width - 1) * clampedX);
        var pixelY = target.Bounds.Y + (int)Math.Round((target.Bounds.Height - 1) * clampedY);
        return new Point(pixelX, pixelY);
    }

    public void PressKey(WindowCaptureTarget target, string keyText)
    {
        PrepareWindow(target, 200);

        var virtualKey = ResolveVirtualKey(keyText);
        var inputs = new[]
        {
            NativeMethods.CreateKeyboardInput(virtualKey, 0),
            NativeMethods.CreateKeyboardInput(virtualKey, NativeMethods.KEYEVENTF_KEYUP),
        };
        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException($"Failed to send keyboard input for key '{keyText}'.");
        }
    }

    private static ushort ResolveVirtualKey(string keyText)
    {
        var trimmed = keyText.Trim();
        if (string.Equals(trimmed, "Escape", StringComparison.OrdinalIgnoreCase)
            || string.Equals(trimmed, "Esc", StringComparison.OrdinalIgnoreCase))
        {
            return NativeMethods.VK_ESCAPE;
        }

        if (string.Equals(trimmed, "Enter", StringComparison.OrdinalIgnoreCase))
        {
            return NativeMethods.VK_RETURN;
        }

        if (string.Equals(trimmed, "Space", StringComparison.OrdinalIgnoreCase))
        {
            return NativeMethods.VK_SPACE;
        }

        if (trimmed.Length != 1)
        {
            throw new InvalidOperationException($"Unsupported keyText '{keyText}'.");
        }

        var virtualKey = NativeMethods.VkKeyScan(trimmed[0]);
        if (virtualKey == -1)
        {
            throw new InvalidOperationException($"Unable to resolve keyText '{keyText}' to a virtual key.");
        }

        return (ushort)(virtualKey & 0xFF);
    }
}
