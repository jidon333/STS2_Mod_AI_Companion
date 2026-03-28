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
    public static void InitializeSessionArtifacts(
        string sessionRoot,
        string sessionId,
        string scenarioId,
        string providerKind)
    {
        Directory.CreateDirectory(sessionRoot);
        Directory.CreateDirectory(Path.Combine(sessionRoot, "attempts"));

        var now = DateTimeOffset.UtcNow;
        if (!File.Exists(GetGoalContractPath(sessionRoot)))
        {
            WriteJsonAtomicWithRetry(
                GetGoalContractPath(sessionRoot),
                new GuiSmokeGoalContract(
                    sessionId,
                    scenarioId,
                    providerKind,
                    sessionRoot,
                    now,
                    now,
                    GuiSmokeContractStates.TrustInvalid,
                    GuiSmokeContractStates.MilestoneInProgress,
                    GuiSmokeContractStates.SessionStarting,
                    CreateRunnerOwner(now),
                    now,
                    null,
                    null,
                    GoalCompletionCriteria,
                    GoalOperationalRules),
                GuiSmokeShared.JsonOptions);
        }

        if (!File.Exists(GetPrevalidationPath(sessionRoot)))
        {
            WriteJsonAtomicWithRetry(
                GetPrevalidationPath(sessionRoot),
                new GuiSmokePrevalidation(
                    sessionId,
                    now,
                    now,
                    GameStoppedBeforeDeploy: false,
                    ModsPayloadReconciled: false,
                    DeployIdentityVerified: false,
                    ManualCleanBootVerified: false,
                    GameStopEvidence: null,
                    DeployEvidence: null,
                    ManualCleanBootEvidence: null,
                    Notes: Array.Empty<string>()),
                GuiSmokeShared.JsonOptions);
        }

        if (!File.Exists(GetStartupSummaryPath(sessionRoot)))
        {
            WriteJsonAtomicWithRetry(
                GetStartupSummaryPath(sessionRoot),
                new GuiSmokeStartupSummary(
                    sessionId,
                    now,
                    now,
                    LatestStage: null,
                    LatestStatus: null,
                    GameStoppedBeforeDeployRecorded: false,
                    DeployCommandSelected: false,
                    DeployMode: null,
                    SelectedDeployToolPath: null,
                    SelectedDeployReason: null,
                    DeployCommandStarted: false,
                    DeployCommandFinished: false,
                    DeployCommandExitCode: null,
                    DeployCommandTimedOut: false,
                    DeployCommandDurationMs: null,
                    DeployCommandFailureReason: null,
                    DeployVerificationStarted: false,
                    DeployVerificationFinished: false,
                    LaunchIssued: false,
                    WindowDetected: false,
                    ManualCleanBootEvaluationStarted: false,
                    ManualCleanBootEvaluationFinished: false,
                    FirstAttemptCreated: false,
                    FirstScreenshotCaptured: false,
                    FailureStage: null,
                    FailureReason: null),
                GuiSmokeShared.JsonOptions);
        }

        RefreshSessionSummary(sessionRoot);
        RefreshSupervisorState(sessionRoot);
    }

    public static GuiSmokeGoalContract UpdateRunnerSessionState(string sessionRoot, string sessionState, string? note = null)
    {
        var goal = LoadOrCreateGoalContract(sessionRoot);
        var now = DateTimeOffset.UtcNow;
        var completedAt = sessionState is GuiSmokeContractStates.SessionCompleted or GuiSmokeContractStates.SessionAborted
            ? now
            : goal.CompletedAt;
        var completedBy = sessionState is GuiSmokeContractStates.SessionCompleted or GuiSmokeContractStates.SessionAborted
            ? "runner"
            : goal.CompletedBy;
        var updated = goal with
        {
            UpdatedAt = now,
            SessionState = sessionState,
            LastRunnerHeartbeatAt = now,
            CompletedAt = completedAt,
            CompletedBy = completedBy,
        };

        WriteJsonAtomicWithRetry(GetGoalContractPath(sessionRoot), updated, GuiSmokeShared.JsonOptions);
        if (!string.IsNullOrWhiteSpace(note))
        {
            UpdatePrevalidation(sessionRoot, note: note);
        }
        else
        {
            RefreshSupervisorState(sessionRoot);
        }

        return LoadOrCreateGoalContract(sessionRoot);
    }

    public static GuiSmokePrevalidation UpdatePrevalidation(
        string sessionRoot,
        bool? gameStoppedBeforeDeploy = null,
        bool? modsPayloadReconciled = null,
        bool? deployIdentityVerified = null,
        bool? manualCleanBootVerified = null,
        GuiSmokeProcessStopEvidence? gameStopEvidence = null,
        GuiSmokeDeployEvidence? deployEvidence = null,
        GuiSmokeManualCleanBootEvidence? manualCleanBootEvidence = null,
        string? note = null)
    {
        var prevalidation = LoadOrCreatePrevalidation(sessionRoot);
        var notes = prevalidation.Notes.ToList();
        if (!string.IsNullOrWhiteSpace(note) && !notes.Contains(note, StringComparer.OrdinalIgnoreCase))
        {
            notes.Add(note);
        }

        var updated = prevalidation with
        {
            UpdatedAt = DateTimeOffset.UtcNow,
            GameStoppedBeforeDeploy = gameStoppedBeforeDeploy ?? prevalidation.GameStoppedBeforeDeploy,
            ModsPayloadReconciled = modsPayloadReconciled ?? prevalidation.ModsPayloadReconciled,
            DeployIdentityVerified = deployIdentityVerified ?? prevalidation.DeployIdentityVerified,
            ManualCleanBootVerified = manualCleanBootVerified ?? prevalidation.ManualCleanBootVerified,
            GameStopEvidence = gameStopEvidence ?? prevalidation.GameStopEvidence,
            DeployEvidence = deployEvidence ?? prevalidation.DeployEvidence,
            ManualCleanBootEvidence = manualCleanBootEvidence ?? prevalidation.ManualCleanBootEvidence,
            Notes = notes,
        };

        WriteJsonAtomicWithRetry(GetPrevalidationPath(sessionRoot), updated, GuiSmokeShared.JsonOptions);
        RefreshSupervisorState(sessionRoot);
        return updated;
    }

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

    private static void AppendPrevalidationNoteWithoutRefresh(string sessionRoot, string note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return;
        }

        var prevalidation = LoadOrCreatePrevalidation(sessionRoot);
        if (prevalidation.Notes.Contains(note, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var updated = prevalidation with
        {
            UpdatedAt = DateTimeOffset.UtcNow,
            Notes = prevalidation.Notes.Concat(new[] { note }).ToArray(),
        };
        WriteJsonAtomicWithRetry(GetPrevalidationPath(sessionRoot), updated, GuiSmokeShared.JsonOptions);
    }
    private static string? TryAppendPrevalidationNoteWithoutRefresh(string sessionRoot, string note)
    {
        try
        {
            AppendPrevalidationNoteWithoutRefresh(sessionRoot, note);
            return null;
        }
        catch (Exception exception)
        {
            return $"prevalidation-note-failure:{exception.GetType().Name}:{exception.Message}";
        }
    }

    private static void TryRefreshSupervisorState(string sessionRoot, string context)
    {
        try
        {
            RefreshSupervisorState(sessionRoot);
        }
        catch (Exception exception)
        {
            AppendPrevalidationNoteWithoutRefresh(sessionRoot, $"startup-trace-refresh-failed:{context}:{exception.GetType().Name}:{exception.Message}");

            var summary = LoadOrCreateStartupSummary(sessionRoot);
            var updatedSummary = summary with
            {
                UpdatedAt = DateTimeOffset.UtcNow,
                FailureStage = summary.FailureStage ?? context,
                FailureReason = summary.FailureReason ?? $"{exception.GetType().Name}: {exception.Message}",
            };
            WriteJsonAtomicWithRetry(GetStartupSummaryPath(sessionRoot), updatedSummary, GuiSmokeShared.JsonOptions);
        }
    }
}
