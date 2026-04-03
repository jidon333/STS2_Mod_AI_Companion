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

sealed record GuiSmokeVideoRecordingMetadata(
    string ScopeKind,
    string SessionId,
    string RunId,
    string RootPath,
    string SessionRoot,
    string OutputPath)
{
    public string? AttemptId { get; init; }

    public string Status { get; set; } = "pending";

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? RecordingStartedAt { get; set; }

    public DateTimeOffset? RecordingEndedAt { get; set; }

    public string? FfmpegPath { get; set; }

    public bool FfmpegAvailable { get; set; }

    public string? CommandLine { get; set; }

    public bool WindowScopedCaptureRequested { get; set; }

    public string? CaptureMode { get; set; }

    public string? CaptureInputPattern { get; set; }

    public string? CaptureModeNote { get; set; }

    public string? WindowTitle { get; set; }

    public WindowBounds? CaptureBounds { get; set; }

    public bool? Kept { get; set; }

    public string? CompletionReason { get; set; }

    public string? SkipReason { get; set; }

    public int? ExitCode { get; set; }
}

sealed record VideoPathBinding(string HostPath, string ProcessPath);

sealed record GuiSmokeFfmpegCaptureSupport(
    bool SupportsGdigrab,
    bool IsFfmpegBinary = true,
    bool SkipActualProcessLaunch = false);
