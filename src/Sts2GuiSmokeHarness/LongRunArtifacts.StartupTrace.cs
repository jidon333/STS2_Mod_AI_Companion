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

static partial class LongRunArtifacts
{
    public static void RecordStartupStage(
        string sessionRoot,
        string stage,
        string status,
        string? detail = null,
        IReadOnlyDictionary<string, string?>? metadata = null)
    {
        var safeMetadata = metadata ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        AppendNdjson(
            GetStartupTracePath(sessionRoot),
            new GuiSmokeStartupTraceEntry(
                DateTimeOffset.UtcNow,
                Path.GetFileName(sessionRoot),
                stage,
                status,
                detail,
                safeMetadata));
        var summary = ApplyStartupStageUpdate(
            LoadOrCreateStartupSummary(sessionRoot),
            stage,
            status,
            detail,
            safeMetadata);
        WriteJsonAtomicWithRetry(GetStartupSummaryPath(sessionRoot), summary, GuiSmokeShared.JsonOptions);
        TryRefreshSupervisorState(sessionRoot, $"startup-stage:{stage}:{status}");
    }

    public static void RecordStartupFailure(
        string sessionRoot,
        string stage,
        string reason,
        IReadOnlyDictionary<string, string?>? metadata = null)
    {
        RecordStartupStage(sessionRoot, stage, "failed", reason, metadata);
        AppendPrevalidationNoteWithoutRefresh(sessionRoot, $"startup-failure:{stage}:{reason}");
        TryRefreshSupervisorState(sessionRoot, $"startup-failure:{stage}");
    }
    private static GuiSmokeStartupSummary ApplyStartupStageUpdate(
        GuiSmokeStartupSummary summary,
        string stage,
        string status,
        string? detail,
        IReadOnlyDictionary<string, string?> metadata)
    {
        var updated = summary with
        {
            UpdatedAt = DateTimeOffset.UtcNow,
            LatestStage = stage,
            LatestStatus = status,
            FailureStage = string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase)
                ? stage
                : summary.FailureStage,
            FailureReason = string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase)
                ? detail
                : summary.FailureReason,
        };

        return stage switch
        {
            "game-stopped-before-deploy" => updated with
            {
                GameStoppedBeforeDeployRecorded = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            "deploy-command-selected" => updated with
            {
                DeployCommandSelected = true,
                DeployMode = metadata.TryGetValue("deployMode", out var deployMode) ? deployMode : updated.DeployMode,
                SelectedDeployToolPath = metadata.TryGetValue("toolPath", out var toolPath) ? toolPath : updated.SelectedDeployToolPath,
                SelectedDeployReason = metadata.TryGetValue("reason", out var reason) ? reason : updated.SelectedDeployReason,
            },
            "deploy-command-started" => updated with
            {
                DeployCommandStarted = true,
            },
            "deploy-command-finished" => updated with
            {
                DeployCommandFinished = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            "deploy-verification-started" => updated with
            {
                DeployVerificationStarted = true,
            },
            "deploy-verification-finished" => updated with
            {
                DeployVerificationFinished = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            "manual-clean-boot-launch-issued" or "bootstrap-launch-issued" => updated with
            {
                LaunchIssued = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            "game-window-detected" or "bootstrap-window-detected" => updated with
            {
                WindowDetected = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            "manual-clean-boot-evaluation-started" or "bootstrap-manual-clean-boot-evaluation-started" => updated with
            {
                ManualCleanBootEvaluationStarted = true,
            },
            "manual-clean-boot-evaluation-finished" or "bootstrap-manual-clean-boot-evaluation-finished" => updated with
            {
                ManualCleanBootEvaluationFinished = true,
            },
            "attempt-0001-started" or "authoritative-attempt-started" => updated with
            {
                FirstAttemptCreated = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            "first-screenshot-captured" or "authoritative-first-screenshot-captured" => updated with
            {
                FirstScreenshotCaptured = !string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase),
            },
            _ => updated,
        };
    }

}
