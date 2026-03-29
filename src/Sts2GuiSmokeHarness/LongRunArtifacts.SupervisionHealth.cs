using System.Diagnostics;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Harness;

static partial class LongRunArtifacts
{
    private sealed record HealthEvaluation(
        string HealthState,
        bool RelevantProcessObserved,
        bool WindowDetected,
        bool RunnerOwnerAlive,
        DateTimeOffset? LastArtifactHeartbeatAt,
        string? LastArtifactHeartbeatPath,
        DateTimeOffset? LastStepAt,
        string? LastStepPath,
        string? ExpectedCurrentAttemptId,
        string? ExpectedCurrentAttemptFirstStepPath,
        IReadOnlyList<string> Classifications,
        IReadOnlyList<string> Evidence);

    private sealed record ArtifactTimestamp(string Path, DateTimeOffset RecordedAt);

    private static HealthEvaluation EvaluateHealth(
        string sessionRoot,
        GuiSmokeGoalContract goal,
        AttemptChronologyProjection chronology,
        IReadOnlyList<GuiSmokeRestartEvent> restartEvents,
        DateTimeOffset now,
        SupervisorObservationOverride? observationOverride)
    {
        var activeSession = goal.SessionState is GuiSmokeContractStates.SessionStarting or GuiSmokeContractStates.SessionCollecting or GuiSmokeContractStates.SessionStalled;
        var relevantProcessObserved = observationOverride?.RelevantProcessObserved ?? ObserveRelevantProcessHealth();
        var windowDetected = observationOverride?.WindowDetected ?? ObserveGameWindow();
        var runnerOwnerAlive = observationOverride?.RunnerOwnerAlive ?? IsRunnerOwnerAlive(goal.RunnerOwner);
        var currentAttemptId = chronology.ActiveAttemptId;
        var expectedCurrentAttemptFirstStepPath = string.IsNullOrWhiteSpace(currentAttemptId)
            ? null
            : Path.Combine(sessionRoot, "attempts", currentAttemptId, "steps", "0001.screen.png");
        var runnerArtifacts = CollectRunnerHeartbeatArtifacts(sessionRoot, currentAttemptId);
        var lastArtifact = runnerArtifacts
            .OrderByDescending(static entry => entry.RecordedAt)
            .FirstOrDefault();
        var lastStep = CollectStepArtifacts(sessionRoot)
            .OrderByDescending(static entry => entry.RecordedAt)
            .FirstOrDefault();
        var classifications = new List<string>();
        var evidence = new List<string>
        {
            $"runner-owner-pid:{goal.RunnerOwner.ProcessId}",
            $"runner-owner-alive:{runnerOwnerAlive}",
            $"relevant-process-observed:{relevantProcessObserved}",
            $"window-detected:{windowDetected}",
        };

        if (lastArtifact is not null)
        {
            evidence.Add($"artifact-heartbeat:{lastArtifact.Path}");
            evidence.Add($"artifact-heartbeat-at:{lastArtifact.RecordedAt:O}");
        }
        else
        {
            evidence.Add("artifact-heartbeat:none");
        }

        if (lastStep is not null)
        {
            evidence.Add($"last-step:{lastStep.Path}");
            evidence.Add($"last-step-at:{lastStep.RecordedAt:O}");
        }
        else
        {
            evidence.Add("last-step:none");
        }

        if (activeSession && !runnerOwnerAlive)
        {
            classifications.Add("runner-dead");
        }

        if (activeSession && (lastArtifact is null || now - lastArtifact.RecordedAt > NoArtifactHeartbeatThreshold))
        {
            classifications.Add("no-artifact-heartbeat");
        }

        var latestLaunchOrStartAt = restartEvents
            .Where(eventEntry =>
                string.Equals(eventEntry.AttemptId, currentAttemptId, StringComparison.OrdinalIgnoreCase)
                && (string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventRunnerLaunchIssued, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(eventEntry.EventType, GuiSmokeContractStates.EventNextAttemptStarted, StringComparison.OrdinalIgnoreCase)))
            .Select(static eventEntry => (DateTimeOffset?)eventEntry.RecordedAt)
            .DefaultIfEmpty(null)
            .Max();
        if (activeSession
            && windowDetected
            && !string.IsNullOrWhiteSpace(expectedCurrentAttemptFirstStepPath)
            && !File.Exists(expectedCurrentAttemptFirstStepPath)
            && latestLaunchOrStartAt is not null
            && now - latestLaunchOrStartAt.Value > WindowNoStepThreshold)
        {
            classifications.Add("window-detected-no-step");
            evidence.Add($"expected-current-attempt-first-step:{expectedCurrentAttemptFirstStepPath}");
        }

        var healthState = classifications.Count == 0
            ? GuiSmokeContractStates.HealthHealthy
            : classifications.Contains("runner-dead", StringComparer.OrdinalIgnoreCase)
                ? GuiSmokeContractStates.HealthCritical
                : GuiSmokeContractStates.HealthWarning;
        return new HealthEvaluation(
            healthState,
            relevantProcessObserved,
            windowDetected,
            runnerOwnerAlive,
            lastArtifact?.RecordedAt,
            lastArtifact?.Path,
            lastStep?.RecordedAt,
            lastStep?.Path,
            currentAttemptId,
            expectedCurrentAttemptFirstStepPath,
            classifications,
            evidence);
    }

    private static IReadOnlyList<ArtifactTimestamp> CollectRunnerHeartbeatArtifacts(string sessionRoot, string? currentAttemptId)
    {
        var paths = new List<string>
        {
            GetPrevalidationPath(sessionRoot),
            GetRestartEventsPath(sessionRoot),
            Path.Combine(sessionRoot, "attempt-index.ndjson"),
            Path.Combine(sessionRoot, "session-summary.json"),
        };

        if (!string.IsNullOrWhiteSpace(currentAttemptId))
        {
            var attemptRoot = Path.Combine(sessionRoot, "attempts", currentAttemptId);
            paths.Add(Path.Combine(attemptRoot, "run.json"));
            paths.Add(Path.Combine(attemptRoot, "run.log"));
            paths.Add(Path.Combine(attemptRoot, "trace.ndjson"));
            paths.Add(Path.Combine(attemptRoot, "progress.ndjson"));
        }

        var timestamps = new List<ArtifactTimestamp>();
        foreach (var path in paths.Where(File.Exists))
        {
            timestamps.Add(new ArtifactTimestamp(path, new FileInfo(path).LastWriteTimeUtc));
        }

        return timestamps;
    }

    private static IReadOnlyList<ArtifactTimestamp> CollectStepArtifacts(string sessionRoot)
    {
        var attemptsRoot = Path.Combine(sessionRoot, "attempts");
        if (!Directory.Exists(attemptsRoot))
        {
            return Array.Empty<ArtifactTimestamp>();
        }

        return Directory.GetFiles(attemptsRoot, "*.screen.png", SearchOption.AllDirectories)
            .Select(path => new ArtifactTimestamp(path, new FileInfo(path).LastWriteTimeUtc))
            .ToArray();
    }

    private static bool ObserveRelevantProcessHealth()
    {
        try
        {
            return Process.GetProcesses().Any(process =>
                string.Equals(process.ProcessName, "SlayTheSpire2", StringComparison.OrdinalIgnoreCase)
                || string.Equals(process.ProcessName, "crashpad_handler", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    private static bool ObserveGameWindow()
    {
        try
        {
            return WindowLocator.TryFindSts2Window() is not null;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsRunnerOwnerAlive(GuiSmokeRunnerOwner owner)
    {
        try
        {
            var process = Process.GetProcessById(owner.ProcessId);
            if (process.HasExited)
            {
                return false;
            }

            return string.Equals(process.ProcessName, owner.ProcessName, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static GuiSmokeRunnerOwner CreateRunnerOwner(DateTimeOffset claimedAt)
    {
        return new GuiSmokeRunnerOwner(
            Environment.MachineName,
            Environment.ProcessId,
            GetCurrentProcessName(),
            claimedAt);
    }

    private static string GetCurrentProcessName()
    {
        try
        {
            return Process.GetCurrentProcess().ProcessName;
        }
        catch
        {
            return "Sts2GuiSmokeHarness";
        }
    }

    private static IReadOnlyList<string> GetRunningRelevantProcesses()
    {
        try
        {
            return Process.GetProcesses()
                .Where(process =>
                    string.Equals(process.ProcessName, "SlayTheSpire2", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(process.ProcessName, "crashpad_handler", StringComparison.OrdinalIgnoreCase))
                .Select(static process => process.ProcessName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static bool HasActiveArmSession(string armSessionPath)
    {
        if (!File.Exists(armSessionPath))
        {
            return false;
        }

        var armSession = TryReadJson<HarnessArmSession>(armSessionPath);
        return armSession is null || armSession.ExpiresAt > DateTimeOffset.UtcNow;
    }

    private static bool HasPendingHarnessActions(string actionsPath)
    {
        if (!File.Exists(actionsPath))
        {
            return false;
        }

        try
        {
            return File.ReadLines(actionsPath).Any(static line => !string.IsNullOrWhiteSpace(line));
        }
        catch
        {
            return true;
        }
    }

    private static bool IsHarnessDormant(HarnessQueueLayout harnessLayout)
    {
        var status = TryReadJson<HarnessBridgeStatus>(harnessLayout.StatusPath);
        if (status is not null && !string.Equals(status.Mode, "dormant", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var inventory = TryReadJson<HarnessNodeInventory>(harnessLayout.InventoryPath);
        return inventory is null || string.Equals(inventory.Mode, "dormant", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCompanionFamilyFile(string fileName)
    {
        return fileName.StartsWith("Sts2ModAiCompanion", StringComparison.OrdinalIgnoreCase)
               || fileName.StartsWith("Sts2AiCompanion", StringComparison.OrdinalIgnoreCase)
               || fileName.StartsWith("Sts2ModKit", StringComparison.OrdinalIgnoreCase)
               || string.Equals(fileName, "runtime-assembly-manifest.json", StringComparison.OrdinalIgnoreCase)
               || string.Equals(fileName, "sts2-mod-ai-companion.config.json", StringComparison.OrdinalIgnoreCase)
               || string.Equals(fileName, "sts2-mod-ai-companion.runtime-config", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryMatchIntentionalRewrite(
        string relativePath,
        string sourcePath,
        string deployedPath,
        out string? rewriteNote)
    {
        rewriteNote = null;
        if (!string.Equals(relativePath, "sts2-mod-ai-companion.runtime-config", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var normalizedSource = TryNormalizeRuntimeConfigForDeployVerification(sourcePath);
        var normalizedDeployed = TryNormalizeRuntimeConfigForDeployVerification(deployedPath);
        if (normalizedSource is null || normalizedDeployed is null)
        {
            return false;
        }

        var sourceHash = ComputeSha256Utf8(normalizedSource);
        var deployedHash = ComputeSha256Utf8(normalizedDeployed);
        if (!string.Equals(sourceHash, deployedHash, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        rewriteNote = $"rewrite-normalized-match:{relativePath}:{deployedHash}";
        return true;
    }
}
