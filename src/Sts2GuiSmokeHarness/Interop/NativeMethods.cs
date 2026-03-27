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

static class NativeMethods
{
    public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    public const uint MOUSEEVENTF_LEFTUP = 0x0004;
    public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    public const uint KEYEVENTF_KEYUP = 0x0002;
    public const ushort VK_ESCAPE = 0x1B;
    public const ushort VK_RETURN = 0x0D;
    public const ushort VK_SPACE = 0x20;
    private const uint INPUT_MOUSE = 0;
    private const uint INPUT_KEYBOARD = 1;
    private const uint PW_RENDERFULLCONTENT = 0x00000002;
    public const int SW_RESTORE = 9;

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern short VkKeyScan(char ch);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsHungAppWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    public static bool TryGetClientBounds(IntPtr handle, out Rectangle bounds)
    {
        bounds = Rectangle.Empty;
        if (!GetClientRect(handle, out var rect))
        {
            return false;
        }

        var topLeft = new POINT { X = rect.Left, Y = rect.Top };
        var bottomRight = new POINT { X = rect.Right, Y = rect.Bottom };
        if (!ClientToScreen(handle, ref topLeft) || !ClientToScreen(handle, ref bottomRight))
        {
            return false;
        }

        bounds = Rectangle.FromLTRB(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
        return bounds.Width > 0 && bounds.Height > 0;
    }

    public static bool TryCaptureClientArea(IntPtr handle, Rectangle clientBounds, out Bitmap bitmap)
    {
        bitmap = null!;
        if (IsHungAppWindow(handle))
        {
            return false;
        }

        if (!GetWindowRect(handle, out var windowRect))
        {
            return false;
        }

        var fullWindowBounds = Rectangle.FromLTRB(windowRect.Left, windowRect.Top, windowRect.Right, windowRect.Bottom);
        if (fullWindowBounds.Width <= 0 || fullWindowBounds.Height <= 0)
        {
            return false;
        }

        using var fullWindowBitmap = new Bitmap(fullWindowBounds.Width, fullWindowBounds.Height);
        using (var graphics = Graphics.FromImage(fullWindowBitmap))
        {
            var hdc = graphics.GetHdc();
            try
            {
                if (!PrintWindow(handle, hdc, PW_RENDERFULLCONTENT))
                {
                    return false;
                }
            }
            finally
            {
                graphics.ReleaseHdc(hdc);
            }
        }

        var cropArea = new Rectangle(
            clientBounds.X - fullWindowBounds.X,
            clientBounds.Y - fullWindowBounds.Y,
            clientBounds.Width,
            clientBounds.Height);
        if (cropArea.X < 0
            || cropArea.Y < 0
            || cropArea.Right > fullWindowBounds.Width
            || cropArea.Bottom > fullWindowBounds.Height
            || cropArea.Width <= 0
            || cropArea.Height <= 0)
        {
            return false;
        }

        bitmap = fullWindowBitmap.Clone(cropArea, fullWindowBitmap.PixelFormat);
        return true;
    }

    public static INPUT CreateMouseInput(int dx, int dy, uint flags)
    {
        return new INPUT
        {
            type = INPUT_MOUSE,
            U = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = dx,
                    dy = dy,
                    dwFlags = flags,
                }
            }
        };
    }

    public static INPUT CreateKeyboardInput(ushort virtualKey, uint flags)
    {
        return new INPUT
        {
            type = INPUT_KEYBOARD,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = virtualKey,
                    wScan = 0,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero,
                }
            }
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;

        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}
