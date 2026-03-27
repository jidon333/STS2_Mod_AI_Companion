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

sealed class GuiSmokeReplayTracer
{
    private readonly string _scope;
    private readonly bool _verbose;
    private readonly List<GuiSmokeReplayTimingEntry> _entries = new();

    public GuiSmokeReplayTracer(string scope, bool verbose)
    {
        _scope = scope;
        _verbose = verbose;
    }

    public IReadOnlyList<GuiSmokeReplayTimingEntry> Entries => _entries;

    public void Info(string message)
    {
        Console.Error.WriteLine($"[{_scope}] {message}");
    }

    public void Skipped(string stage, string detail)
    {
        _entries.Add(new GuiSmokeReplayTimingEntry(stage, 0, $"skipped:{detail}"));
        if (_verbose)
        {
            Info($"{stage} skipped ({detail})");
        }
    }

    public T Measure<T>(string stage, Func<T> action, string? detail = null, bool alwaysLog = false)
    {
        if (_verbose || alwaysLog)
        {
            Info($"start {stage}{FormatDetail(detail)}");
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            return action();
        }
        finally
        {
            stopwatch.Stop();
            var entry = new GuiSmokeReplayTimingEntry(stage, stopwatch.ElapsedMilliseconds, detail);
            _entries.Add(entry);
            if (_verbose || alwaysLog)
            {
                Info($"done {stage} {stopwatch.ElapsedMilliseconds}ms{FormatDetail(detail)}");
            }
        }
    }

    private static string FormatDetail(string? detail)
    {
        return string.IsNullOrWhiteSpace(detail)
            ? string.Empty
            : $" ({detail})";
    }
}
