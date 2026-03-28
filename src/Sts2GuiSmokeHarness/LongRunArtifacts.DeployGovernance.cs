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
    public static void RecordDeployCommandResult(
        string sessionRoot,
        GuiSmokeDeployCommand command,
        GuiSmokeProcessExecutionResult result,
        string? failureReason)
    {
        var summary = new GuiSmokeDeployCommandSummary(
            DateTimeOffset.UtcNow,
            Path.GetFileName(sessionRoot),
            command.Mode,
            command.FileName,
            command.Arguments,
            command.ToolPath,
            command.Reason,
            result.ExitCode,
            result.TimedOut,
            result.Duration.TotalMilliseconds,
            TrimOutputTail(result.Stdout),
            TrimOutputTail(result.Stderr),
            failureReason,
            result.FailureKind,
            result.ExceptionType,
            result.ExceptionMessage);
        var persistFailureReason = TryWriteJsonWithFallback(
            GetDeployCommandSummaryPath(sessionRoot),
            summary,
            GuiSmokeShared.JsonOptions,
            "deploy-command-summary");

        var combinedFailureReason = CombineFailureReasons(failureReason, persistFailureReason);
        var existingStartupSummary = LoadOrCreateStartupSummary(sessionRoot);
        var startupSummary = existingStartupSummary with
        {
            UpdatedAt = DateTimeOffset.UtcNow,
            DeployCommandExitCode = result.ExitCode,
            DeployCommandTimedOut = result.TimedOut,
            DeployCommandDurationMs = result.Duration.TotalMilliseconds,
            DeployCommandFailureReason = combinedFailureReason,
            FailureStage = string.IsNullOrWhiteSpace(combinedFailureReason) ? existingStartupSummary.FailureStage : "deploy-command-finished",
            FailureReason = string.IsNullOrWhiteSpace(combinedFailureReason) ? existingStartupSummary.FailureReason : combinedFailureReason,
        };
        persistFailureReason = CombineFailureReasons(
            persistFailureReason,
            TryWriteJsonWithFallback(
                GetStartupSummaryPath(sessionRoot),
                startupSummary,
                GuiSmokeShared.JsonOptions,
                "startup-summary"));

        if (!string.IsNullOrWhiteSpace(failureReason))
        {
            persistFailureReason = CombineFailureReasons(
                persistFailureReason,
                TryAppendPrevalidationNoteWithoutRefresh(sessionRoot, $"deploy-command-failure:{failureReason}"));
        }

        if (!string.IsNullOrWhiteSpace(persistFailureReason))
        {
            persistFailureReason = CombineFailureReasons(
                persistFailureReason,
                TryAppendPrevalidationNoteWithoutRefresh(sessionRoot, persistFailureReason));

            var recoveredStartupSummary = LoadOrCreateStartupSummary(sessionRoot) with
            {
                UpdatedAt = DateTimeOffset.UtcNow,
                DeployCommandExitCode = result.ExitCode,
                DeployCommandTimedOut = result.TimedOut,
                DeployCommandDurationMs = result.Duration.TotalMilliseconds,
                DeployCommandFailureReason = CombineFailureReasons(failureReason, persistFailureReason),
                FailureStage = "deploy-command-finished",
                FailureReason = CombineFailureReasons(failureReason, persistFailureReason),
            };
            persistFailureReason = CombineFailureReasons(
                persistFailureReason,
                TryWritePlainJson(
                    GetStartupSummaryPath(sessionRoot),
                    recoveredStartupSummary,
                    GuiSmokeShared.JsonOptions,
                    "startup-summary-plain"));
            var durablePersistFailureReason = persistFailureReason;
            if (!string.IsNullOrWhiteSpace(durablePersistFailureReason))
            {
                persistFailureReason = CombineFailureReasons(
                    persistFailureReason,
                    TryAppendPrevalidationNoteWithoutRefresh(sessionRoot, durablePersistFailureReason));
            }
        }

        TryRefreshSupervisorState(sessionRoot, "deploy-command-result");
    }

    public static void RecordGameStoppedBeforeDeployEvidence(string sessionRoot)
    {
        var runningProcesses = GetRunningRelevantProcesses();
        var evidence = new GuiSmokeProcessStopEvidence(
            DateTimeOffset.UtcNow,
            ObserveGameWindow(),
            runningProcesses);
        UpdatePrevalidation(
            sessionRoot,
            gameStoppedBeforeDeploy: runningProcesses.Count == 0 && !evidence.WindowDetected,
            gameStopEvidence: evidence,
            note: "runner captured process-stop evidence before deploy.");
    }

    public static void RecordDeployVerificationEvidence(
        string sessionRoot,
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        bool includeHarnessBridge)
    {
        var artifactsRoot = Path.GetFullPath(configuration.GamePaths.ArtifactsRoot, workspaceRoot);
        var reportPath = Path.Combine(artifactsRoot, "native-package-layout", "flat", "native-deploy-report.json");
        if (!File.Exists(reportPath))
        {
            UpdatePrevalidation(
                sessionRoot,
                modsPayloadReconciled: false,
                deployIdentityVerified: false,
                note: $"deploy report missing: {reportPath}");
            return;
        }

        var reportDocument = TryReadJsonDocument(reportPath);
        if (reportDocument is null)
        {
            UpdatePrevalidation(
                sessionRoot,
                modsPayloadReconciled: false,
                deployIdentityVerified: false,
                note: $"deploy report unreadable: {reportPath}");
            return;
        }

        var root = reportDocument.RootElement;
        var sourcePackageRoot = TryReadString(root, "sourcePackageRoot");
        var deployedRoot = TryReadString(root, "deployedRoot");
        var reportSha256 = ComputeFullFileSha256(reportPath);
        var expectedFiles = new Dictionary<string, (string RelativePath, string SourcePath, string DestinationPath)>(StringComparer.OrdinalIgnoreCase);

        if (root.TryGetProperty("files", out var filesElement) && filesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var fileElement in filesElement.EnumerateArray())
            {
                var sourcePath = TryReadString(fileElement, "sourcePath");
                var destinationPath = TryReadString(fileElement, "destinationPath");
                if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destinationPath))
                {
                    continue;
                }

                var relativePath = Path.GetFileName(destinationPath);
                expectedFiles[relativePath] = (relativePath, sourcePath, destinationPath);
            }
        }

        if (includeHarnessBridge && !string.IsNullOrWhiteSpace(deployedRoot))
        {
            foreach (var (sourceRoot, fileName) in new[]
                     {
                         (Path.Combine(workspaceRoot, "src", "Sts2ModAiCompanion.HarnessBridge", "bin", "Debug", "net7.0"), "Sts2ModAiCompanion.HarnessBridge.dll"),
                         (Path.Combine(workspaceRoot, "src", "Sts2AiCompanion.Foundation", "bin", "Debug", "net7.0"), "Sts2AiCompanion.Foundation.dll"),
                     })
            {
                var sourcePath = Path.Combine(sourceRoot, fileName);
                var destinationPath = Path.Combine(deployedRoot, fileName);
                expectedFiles[fileName] = (fileName, sourcePath, destinationPath);
            }
        }

        var verifiedFiles = new List<GuiSmokeDeployFileEvidence>();
        var missingFiles = new List<string>();
        var hashMismatches = new List<string>();
        var rewriteNotes = new List<string>();
        foreach (var expected in expectedFiles.Values.OrderBy(static entry => entry.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(expected.SourcePath))
            {
                missingFiles.Add($"missing-source:{expected.RelativePath}");
                continue;
            }

            if (!File.Exists(expected.DestinationPath))
            {
                missingFiles.Add($"missing-deployed:{expected.RelativePath}");
                continue;
            }

            var sourceEvidence = DescribeFile(expected.SourcePath);
            var deployedEvidence = DescribeFile(expected.DestinationPath);
            if (!string.Equals(sourceEvidence.Sha256, deployedEvidence.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                if (TryMatchIntentionalRewrite(expected.RelativePath, expected.SourcePath, expected.DestinationPath, out var rewriteNote))
                {
                    if (!string.IsNullOrWhiteSpace(rewriteNote))
                    {
                        rewriteNotes.Add(rewriteNote);
                    }
                }
                else
                {
                    hashMismatches.Add(expected.RelativePath);
                    continue;
                }
            }

            verifiedFiles.Add(new GuiSmokeDeployFileEvidence(expected.RelativePath, sourceEvidence, deployedEvidence));
        }

        var unexpectedFamilyFiles = new List<string>();
        if (!string.IsNullOrWhiteSpace(deployedRoot) && Directory.Exists(deployedRoot))
        {
            foreach (var file in Directory.GetFiles(deployedRoot, "*", SearchOption.TopDirectoryOnly))
            {
                var fileName = Path.GetFileName(file);
                if (!IsCompanionFamilyFile(fileName) || expectedFiles.ContainsKey(fileName))
                {
                    continue;
                }

                unexpectedFamilyFiles.Add(fileName);
            }
        }

        var notes = new List<string>();
        if (!string.IsNullOrWhiteSpace(sourcePackageRoot))
        {
            notes.Add($"sourcePackageRoot:{sourcePackageRoot}");
        }

        if (!string.IsNullOrWhiteSpace(deployedRoot))
        {
            notes.Add($"deployedRoot:{deployedRoot}");
        }

        notes.AddRange(rewriteNotes);

        var deployEvidence = new GuiSmokeDeployEvidence(
            DateTimeOffset.UtcNow,
            reportPath,
            reportSha256,
            sourcePackageRoot,
            deployedRoot,
            verifiedFiles,
            missingFiles,
            hashMismatches,
            unexpectedFamilyFiles,
            notes);
        UpdatePrevalidation(
            sessionRoot,
            modsPayloadReconciled: missingFiles.Count == 0 && unexpectedFamilyFiles.Count == 0,
            deployIdentityVerified: missingFiles.Count == 0 && hashMismatches.Count == 0,
            deployEvidence: deployEvidence,
            note: "runner captured deploy identity evidence from the native deploy report and deployed payload.");
    }
}
