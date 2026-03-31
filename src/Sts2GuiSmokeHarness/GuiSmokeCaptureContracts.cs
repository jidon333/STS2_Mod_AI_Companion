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

sealed record WindowCaptureTarget(
    IntPtr Handle,
    string Title,
    Rectangle Bounds,
    bool IsFallback,
    bool IsMinimized);

enum CaptureBoundaryFailureKind
{
    None,
    UnusableFrame,
    TimedOut,
    Exception,
}

sealed record CaptureFaultInjectionOptions(
    CaptureBoundaryFailureKind FailureKind,
    string ScopeKind,
    string? PhaseName,
    int? StepIndex,
    int? AttemptOrdinal);

sealed record CaptureFaultInjectionContext(
    string ScopeKind,
    string? PhaseName,
    int? StepIndex,
    int? AttemptOrdinal);

sealed record CaptureBoundaryResult(
    bool Succeeded,
    CaptureBoundaryFailureKind FailureKind,
    string? Detail,
    Exception? Exception = null);
