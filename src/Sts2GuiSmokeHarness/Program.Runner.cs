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

internal static partial class Program
{
    static async Task<int> RunScenarioAsync(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        IReadOnlyDictionary<string, string> options)
    {
        var scenarioId = options.TryGetValue("--scenario", out var scenarioRaw)
            ? scenarioRaw
            : "boot-to-combat";
        var isLongRun = string.Equals(scenarioId, "boot-to-long-run", StringComparison.OrdinalIgnoreCase);
        if (!isLongRun && !string.Equals(scenarioId, "boot-to-combat", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported scenario: {scenarioId}");
        }

        var providerKind = ResolveProviderKind(options, isLongRun);
        ValidateProviderConfiguration(providerKind, options);
        var liveLayout = LiveExportPathResolver.Resolve(configuration.GamePaths, configuration.LiveExport);
        var harnessLayout = HarnessPathResolver.Resolve(configuration.GamePaths, configuration.Harness);
        var sessionId = $"{DateTimeOffset.Now:yyyyMMdd-HHmmss}-{scenarioId}";
        var sessionRoot = options.TryGetValue("--run-root", out var explicitRunRoot)
            ? ResolveCliPath(explicitRunRoot, workspaceRoot)
            : Path.Combine(workspaceRoot, "artifacts", "gui-smoke", sessionId);
        var sessionDeadline = isLongRun
            ? TryGetPositiveIntOption(options, "--max-session-hours") is { } maxSessionHours
                ? DateTimeOffset.UtcNow.AddHours(maxSessionHours)
                : DateTimeOffset.MaxValue
            : DateTimeOffset.UtcNow.AddMinutes(10);
        var maxAttempts = TryGetPositiveIntOption(options, "--max-attempts")
            ?? (isLongRun ? int.MaxValue : 1);
        var maxConsecutiveLaunchFailures = TryGetPositiveIntOption(options, "--max-consecutive-launch-failures") ?? 3;
        var maxSceneDeadEnds = TryGetPositiveIntOption(options, "--max-scene-dead-ends") ?? 5;
        var maxSteps = TryGetPositiveIntOption(options, "--max-steps");
        var stopOnFirstTerminal = options.ContainsKey("--stop-on-first-terminal");
        var stopOnFirstLoop = options.ContainsKey("--stop-on-first-loop");

        if (isLongRun)
        {
            Directory.CreateDirectory(sessionRoot);
            Directory.CreateDirectory(Path.Combine(sessionRoot, "attempts"));
            LongRunArtifacts.InitializeSessionArtifacts(sessionRoot, sessionId, scenarioId, providerKind);
        }

        void RecordStartupStage(
            string stage,
            string status,
            string? detail = null,
            IReadOnlyDictionary<string, string?>? metadata = null)
        {
            if (!isLongRun)
            {
                return;
            }

            try
            {
                LongRunArtifacts.RecordStartupStage(sessionRoot, stage, status, detail, metadata);
            }
            catch (Exception exception)
            {
                LogHarness($"startup stage record failed stage={stage} status={status} error={exception.GetType().Name}: {exception.Message}");
            }
        }

        void RecordStartupFailure(
            string stage,
            string reason,
            IReadOnlyDictionary<string, string?>? metadata = null)
        {
            if (!isLongRun)
            {
                return;
            }

            try
            {
                LongRunArtifacts.RecordStartupFailure(sessionRoot, stage, reason, metadata);
            }
            catch (Exception exception)
            {
                LogHarness($"startup failure record failed stage={stage} reason={reason} error={exception.GetType().Name}: {exception.Message}");
            }
        }

        if (!options.ContainsKey("--skip-deploy"))
        {
            var startupStage = "game-stopped-before-deploy";
            try
            {
                EnsureGameNotRunning();
                if (isLongRun)
                {
                    LongRunArtifacts.RecordGameStoppedBeforeDeployEvidence(sessionRoot);
                    RecordStartupStage(startupStage, "finished");
                }

                var deployCommand = BuildDeployNativePackageCommand(configuration, workspaceRoot, options);
                startupStage = "deploy-command-selected";
                RecordStartupStage(
                    startupStage,
                    "finished",
                    $"{deployCommand.Mode}:{deployCommand.Reason}",
                    new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["deployMode"] = deployCommand.Mode,
                        ["toolPath"] = deployCommand.ToolPath,
                        ["reason"] = deployCommand.Reason,
                    });

                startupStage = "deploy-command-started";
                RecordStartupStage(startupStage, "started");
                var deployResult = await RunDeployNativePackageAsync(configuration, workspaceRoot, options, deployCommand).ConfigureAwait(false);
                var deployFailureReason = BuildDeployCommandFailureReason(deployResult);
                if (isLongRun)
                {
                    LongRunArtifacts.RecordDeployCommandResult(sessionRoot, deployCommand, deployResult, deployFailureReason);
                }

                startupStage = "deploy-command-finished";
                if (!string.IsNullOrWhiteSpace(deployFailureReason))
                {
                    RecordStartupFailure(
                        startupStage,
                        deployFailureReason,
                        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["deployMode"] = deployCommand.Mode,
                            ["toolPath"] = deployCommand.ToolPath,
                            ["exitCode"] = deployResult.ExitCode?.ToString(CultureInfo.InvariantCulture),
                            ["timedOut"] = deployResult.TimedOut.ToString(),
                        });
                    throw new InvalidOperationException($"Deploy command failed: {deployFailureReason}");
                }

                RecordStartupStage(
                    startupStage,
                    "finished",
                    $"exitCode={deployResult.ExitCode ?? 0};durationMs={deployResult.Duration.TotalMilliseconds:F0}",
                    new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["deployMode"] = deployCommand.Mode,
                        ["toolPath"] = deployCommand.ToolPath,
                        ["exitCode"] = deployResult.ExitCode?.ToString(CultureInfo.InvariantCulture) ?? "0",
                        ["timedOut"] = "False",
                        ["durationMs"] = deployResult.Duration.TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture),
                    });

                if (isLongRun)
                {
                    startupStage = "deploy-verification-started";
                    RecordStartupStage(startupStage, "started");
                    LongRunArtifacts.RecordDeployVerificationEvidence(
                        sessionRoot,
                        configuration,
                        workspaceRoot,
                        includeHarnessBridge: true);
                    var deployPrevalidation = JsonSerializer.Deserialize<GuiSmokePrevalidation>(
                        File.ReadAllText(Path.Combine(sessionRoot, "prevalidation.json")),
                        GuiSmokeShared.JsonOptions);
                    startupStage = "deploy-verification-finished";
                    RecordStartupStage(
                        startupStage,
                        "finished",
                        deployPrevalidation is null
                            ? "prevalidation-unreadable"
                            : $"modsPayloadReconciled={deployPrevalidation.ModsPayloadReconciled};deployIdentityVerified={deployPrevalidation.DeployIdentityVerified}");
                }
            }
            catch (Exception exception)
            {
                if (isLongRun)
                {
                    RecordStartupFailure(startupStage, $"{exception.GetType().Name}: {exception.Message}");
                    LongRunArtifacts.UpdatePrevalidation(
                        sessionRoot,
                        note: BuildDeployFailureNote(exception));
                    LongRunArtifacts.UpdateRunnerSessionState(
                        sessionRoot,
                        GuiSmokeContractStates.SessionAborted,
                        $"runner aborted during deploy: {exception.GetType().Name}: {exception.Message}");
                }

                throw;
            }
        }
        else if (Directory.Exists(sessionRoot))
        {
            Directory.Delete(sessionRoot, recursive: true);
        }

        if (isLongRun && options.ContainsKey("--skip-deploy"))
        {
            LongRunArtifacts.UpdatePrevalidation(
                sessionRoot,
                note: "skip-deploy was enabled; trust gate remains invalid until required deploy proof is recorded.");
        }

        if (isLongRun && options.ContainsKey("--skip-launch"))
        {
            maxAttempts = Math.Min(maxAttempts, 1);
            LongRunArtifacts.UpdatePrevalidation(
                sessionRoot,
                note: "skip-launch was enabled; manual clean boot proof was not recorded by the runner.");
        }

        if (isLongRun && !options.ContainsKey("--skip-launch"))
        {
            LongRunArtifacts.UpdateRunnerSessionState(
                sessionRoot,
                GuiSmokeContractStates.SessionCollecting,
                "runner performing bootstrap before authoritative attempt 0001.");
            var bootstrapSucceeded = await RunBootstrapPhaseAsync(
                configuration,
                workspaceRoot,
                options,
                liveLayout,
                harnessLayout,
                sessionId,
                sessionRoot,
                sessionDeadline).ConfigureAwait(false);
            if (!bootstrapSucceeded)
            {
                LongRunArtifacts.UpdateRunnerSessionState(
                    sessionRoot,
                    GuiSmokeContractStates.SessionAborted,
                    "runner aborted before authoritative attempt 0001 because bootstrap did not verify manual clean boot.");
                return 1;
            }
        }

        GuiSmokeAttemptResult? lastAttempt = null;
        var consecutiveLaunchFailures = 0;
        var consecutiveSceneDeadEnds = 0;
        for (var attemptOrdinal = 1; attemptOrdinal <= maxAttempts && DateTimeOffset.UtcNow < sessionDeadline; attemptOrdinal += 1)
        {
            var attemptId = attemptOrdinal.ToString("0000", CultureInfo.InvariantCulture);
            var trustStateAtStart = GuiSmokeContractStates.TrustInvalid;
            if (isLongRun)
            {
                if (lastAttempt is not null)
                {
                    if (ShouldMarkSessionStalled(lastAttempt))
                    {
                        LongRunArtifacts.UpdateRunnerSessionState(
                            sessionRoot,
                            GuiSmokeContractStates.SessionStalled,
                            $"runner observed terminal attempt {lastAttempt.AttemptId} before deciding restart.");
                    }

                    LongRunArtifacts.RecordRunnerBeginRestart(sessionRoot, lastAttempt, attemptId, attemptOrdinal);
                }

                LongRunArtifacts.UpdateRunnerSessionState(
                    sessionRoot,
                    GuiSmokeContractStates.SessionCollecting,
                    $"runner starting attempt {attemptId}.");
                trustStateAtStart = LongRunArtifacts.RefreshSupervisorState(sessionRoot).TrustState;
            }

            lastAttempt = await RunAttemptAsync(
                configuration,
                workspaceRoot,
                options,
                scenarioId,
                providerKind,
                liveLayout,
                harnessLayout,
                sessionId,
                sessionRoot,
                isLongRun,
                attemptId,
                attemptOrdinal,
                sessionDeadline,
                trustStateAtStart,
                maxSteps).ConfigureAwait(false);

            if (!isLongRun)
            {
                return lastAttempt.ExitCode;
            }

            if (stopOnFirstTerminal)
            {
                LongRunArtifacts.UpdateRunnerSessionState(
                    sessionRoot,
                    GuiSmokeContractStates.SessionAborted,
                    $"runner stopped after first terminal attempt {attemptId}.");
                return lastAttempt.ExitCode;
            }

            if (stopOnFirstLoop && IsLoopLikeAttempt(lastAttempt))
            {
                LongRunArtifacts.UpdateRunnerSessionState(
                    sessionRoot,
                    GuiSmokeContractStates.SessionAborted,
                    $"runner stopped after first loop-classified attempt {attemptId}.");
                return lastAttempt.ExitCode;
            }

            consecutiveLaunchFailures = lastAttempt.LaunchFailed
                ? consecutiveLaunchFailures + 1
                : 0;
            consecutiveSceneDeadEnds = IsSceneDeadEndAttempt(lastAttempt!)
                ? consecutiveSceneDeadEnds + 1
                : 0;

            if (consecutiveLaunchFailures >= maxConsecutiveLaunchFailures)
            {
                LogHarness($"session abort consecutive launch failures={consecutiveLaunchFailures}");
                LongRunArtifacts.UpdateRunnerSessionState(
                    sessionRoot,
                    GuiSmokeContractStates.SessionAborted,
                    $"runner aborted after {consecutiveLaunchFailures} consecutive launch failures.");
                return 1;
            }

            if (consecutiveSceneDeadEnds >= maxSceneDeadEnds)
            {
                LogHarness($"session abort consecutive scene dead-ends={consecutiveSceneDeadEnds}");
                LongRunArtifacts.UpdateRunnerSessionState(
                    sessionRoot,
                    GuiSmokeContractStates.SessionAborted,
                    $"runner aborted after {consecutiveSceneDeadEnds} consecutive scene dead-ends.");
                return 1;
            }
        }

        if (isLongRun)
        {
            var supervisorState = LongRunArtifacts.RefreshSupervisorState(sessionRoot);
            var finalSessionState = string.Equals(supervisorState.MilestoneState, GuiSmokeContractStates.MilestoneDone, StringComparison.OrdinalIgnoreCase)
                ? GuiSmokeContractStates.SessionCompleted
                : GuiSmokeContractStates.SessionAborted;
            LongRunArtifacts.UpdateRunnerSessionState(
                sessionRoot,
                finalSessionState,
                finalSessionState == GuiSmokeContractStates.SessionCompleted
                    ? "runner ended after milestone proof was completed."
                    : "runner ended before milestone proof completed.");
        }

        return lastAttempt?.ExitCode ?? 1;
    }


    static int GetDecisionWaitMinimumMs(GuiSmokePhase phase)
    {
        return phase switch
        {
            GuiSmokePhase.HandleCombat => 140,
            GuiSmokePhase.WaitRunLoad or GuiSmokePhase.WaitMainMenu or GuiSmokePhase.WaitCharacterSelect => 260,
            GuiSmokePhase.WaitMap or GuiSmokePhase.WaitEventRelease or GuiSmokePhase.WaitPostMapNodeRoom => 220,
            _ => 200,
        };
    }

    static int GetLaunchPollingIntervalMs()
    {
        return 250;
    }

    static int GetActionSettleDelayMs(
        GuiSmokePhase phase,
        GuiSmokeStepDecision decision,
        int defaultMinimumMs,
        int combatMinimumMs)
    {
        var minimumMs = phase == GuiSmokePhase.HandleCombat
            ? combatMinimumMs
            : defaultMinimumMs;
        return Math.Max(minimumMs, decision.WaitMs ?? minimumMs);
    }

    static bool IsNonEnemyCombatSelectionLabel(string? targetLabel)
    {
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return false;
        }

        return targetLabel.StartsWith("combat select non-enemy slot ", StringComparison.OrdinalIgnoreCase)
               || targetLabel.StartsWith("combat select defend slot ", StringComparison.OrdinalIgnoreCase);
    }

    static bool ShouldMarkSessionStalled(GuiSmokeAttemptResult result)
    {
        return string.Equals(result.Status, "failed", StringComparison.OrdinalIgnoreCase)
               && !result.LaunchFailed
               && !string.Equals(result.FailureClass, "launch-runtime-noise", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsLoopLikeAttempt(GuiSmokeAttemptResult result)
    {
        return (result.TerminalCause?.Contains("loop", StringComparison.OrdinalIgnoreCase) ?? false)
               || (result.TerminalCause?.Contains("stall", StringComparison.OrdinalIgnoreCase) ?? false)
               || string.Equals(result.TerminalCause, "combat-lifecycle-transit-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "combat-release-failure-under-noncombat-foreground", StringComparison.OrdinalIgnoreCase)
               || IsRestSitePostClickFailureKind(result.TerminalCause)
               || (result.FailureClass?.Contains("loop", StringComparison.OrdinalIgnoreCase) ?? false)
               || (result.FailureClass?.Contains("stall", StringComparison.OrdinalIgnoreCase) ?? false)
               || string.Equals(result.FailureClass, "combat-lifecycle-transit-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "combat-release-failure-under-noncombat-foreground", StringComparison.OrdinalIgnoreCase)
               || IsRestSitePostClickFailureKind(result.FailureClass);
    }

    static bool IsExplicitDecisionAbortRisk(string? decisionRisk)
    {
        return string.Equals(decisionRisk, "ancient-event-option-contract-mismatch", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionRisk, "post-node-handoff-contract-mismatch", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionRisk, "combat-barrier-handoff-mismatch", StringComparison.OrdinalIgnoreCase)
               || string.Equals(decisionRisk, "combat-release-failure-under-noncombat-foreground", StringComparison.OrdinalIgnoreCase);
    }

    static bool TryClassifyMaxStepBudgetExhaustion(
        string stepsRoot,
        out string terminalCause,
        out string failureClass,
        out string message)
    {
        terminalCause = string.Empty;
        failureClass = string.Empty;
        message = string.Empty;

        var runRoot = Path.GetDirectoryName(stepsRoot);
        if (string.IsNullOrWhiteSpace(runRoot))
        {
            return false;
        }

        var progressPath = Path.Combine(runRoot, "progress.ndjson");
        if (!File.Exists(progressPath))
        {
            return false;
        }

        var totalProgressCount = 0;
        var handleCombatCount = 0;
        foreach (var line in File.ReadLines(progressPath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            totalProgressCount += 1;
            if (line.Contains("\"phase\":\"HandleCombat\"", StringComparison.OrdinalIgnoreCase))
            {
                handleCombatCount += 1;
            }
        }

        if (totalProgressCount == 0 || handleCombatCount < 20 || handleCombatCount * 2 < totalProgressCount)
        {
            return false;
        }

        var lifecycleWaitCount = 0;
        var maxConsecutiveLifecycleFingerprint = 0;
        string? lastLifecycleFingerprint = null;
        var consecutiveLifecycleFingerprint = 0;
        var lifecycleStageCounts = new Dictionary<CombatLifecycleStage, int>();
        foreach (var requestPath in Directory.EnumerateFiles(stepsRoot, "*.request.json", SearchOption.TopDirectoryOnly)
                     .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            if (!TryReadCombatLifecycleWaitEvidence(requestPath, out var lifecycleStage, out var waitFingerprint))
            {
                continue;
            }

            lifecycleWaitCount += 1;
            lifecycleStageCounts[lifecycleStage] = lifecycleStageCounts.TryGetValue(lifecycleStage, out var existingCount)
                ? existingCount + 1
                : 1;
            consecutiveLifecycleFingerprint = string.Equals(lastLifecycleFingerprint, waitFingerprint, StringComparison.Ordinal)
                ? consecutiveLifecycleFingerprint + 1
                : 1;
            lastLifecycleFingerprint = waitFingerprint;
            maxConsecutiveLifecycleFingerprint = Math.Max(maxConsecutiveLifecycleFingerprint, consecutiveLifecycleFingerprint);
        }

        if (lifecycleWaitCount < 8 || maxConsecutiveLifecycleFingerprint < 4)
        {
            return false;
        }

        terminalCause = "combat-lifecycle-transit-step-budget-exhausted";
        failureClass = "combat-lifecycle-transit-step-budget-exhausted";
        message = $"combat-lifecycle-transit-step-budget-exhausted handleCombatSteps={handleCombatCount}/{totalProgressCount} lifecycleWaits={lifecycleWaitCount} maxRepeatedLifecycleFingerprint={maxConsecutiveLifecycleFingerprint} lifecycleStages={FormatLifecycleStageCounts(lifecycleStageCounts)}";
        return true;
    }

    static bool TryReadCombatLifecycleWaitEvidence(
        string requestPath,
        out CombatLifecycleStage lifecycleStage,
        out string waitFingerprint)
    {
        lifecycleStage = CombatLifecycleStage.Unknown;
        waitFingerprint = string.Empty;

        var decisionPath = requestPath.EndsWith(".request.json", StringComparison.OrdinalIgnoreCase)
            ? requestPath[..^".request.json".Length] + ".decision.json"
            : Path.ChangeExtension(requestPath, ".decision.json");
        if (!File.Exists(decisionPath))
        {
            return false;
        }

        using var requestStream = new FileStream(requestPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var request = JsonSerializer.Deserialize<GuiSmokeStepRequest>(requestStream, GuiSmokeShared.JsonOptions);
        if (request is null
            || !Enum.TryParse<GuiSmokePhase>(request.Phase, ignoreCase: true, out var phase)
            || phase != GuiSmokePhase.HandleCombat)
        {
            return false;
        }

        using var decisionStream = new FileStream(decisionPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var decisionDocument = JsonDocument.Parse(decisionStream);
        var decisionRoot = decisionDocument.RootElement;
        var status = decisionRoot.TryGetProperty("status", out var statusElement) ? statusElement.GetString() : null;
        if (!string.Equals(status, "wait", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var observer = new ObserverState(
            request.Observer with
            {
                LastEventsTail = request.Observer.LastEventsTail ?? Array.Empty<string>(),
                ActionNodes = request.Observer.ActionNodes ?? Array.Empty<ObserverActionNode>(),
                Choices = request.Observer.Choices ?? Array.Empty<ObserverChoice>(),
                CombatHand = request.Observer.CombatHand ?? Array.Empty<ObservedCombatHandCard>(),
            },
            null,
            null,
            request.Observer.LastEventsTail?.ToArray() ?? Array.Empty<string>());
        var analysisContext = GuiSmokeStepRequestFactory.CreateStepAnalysisContext(
            phase,
            observer,
            request.ScreenshotPath,
            request.History ?? Array.Empty<GuiSmokeHistoryEntry>(),
            request.CombatCardKnowledge ?? Array.Empty<CombatCardKnowledgeHint>(),
            request.WindowBounds);
        lifecycleStage = analysisContext.CombatReleaseState.LifecycleStage;
        if (lifecycleStage is not (CombatLifecycleStage.EndTurnTransit
            or CombatLifecycleStage.EnemyTurn
            or CombatLifecycleStage.PlayerReopenPending))
        {
            return false;
        }

        waitFingerprint = BuildDecisionWaitFingerprint(phase, request.SceneSignature, observer, analysisContext);
        return true;
    }

    static string FormatLifecycleStageCounts(IReadOnlyDictionary<CombatLifecycleStage, int> lifecycleStageCounts)
    {
        return string.Join(
            ",",
            lifecycleStageCounts
                .OrderBy(static entry => entry.Key)
                .Select(static entry => $"{entry.Key}:{entry.Value.ToString(CultureInfo.InvariantCulture)}"));
    }

    static string ClassifyFailureForAttempt(
        GuiSmokePhase phase,
        ObserverState? observer,
        string terminalCause,
        bool launchFailed)
    {
        if (launchFailed
            || string.Equals(terminalCause, "launch-failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(terminalCause, "process-lost", StringComparison.OrdinalIgnoreCase))
        {
            return "launch-runtime-noise";
        }

        if (IsSceneAuthorityInvalidFailure(phase, observer))
        {
            return "scene-authority-invalid";
        }

        return terminalCause switch
        {
            "combat-lifecycle-transit-step-budget-exhausted" => "combat-lifecycle-transit-step-budget-exhausted",
            "combat-barrier-step-budget-exhausted" => "combat-barrier-step-budget-exhausted",
            "combat-barrier-handoff-mismatch" => "combat-barrier-handoff-mismatch",
            "combat-release-failure-under-noncombat-foreground" => "combat-release-failure-under-noncombat-foreground",
            "reward-aftermath-card-progression-stall" => "reward-aftermath-card-progression-stall",
            "rest-site-release-map-handoff-stall" => "rest-site-release-map-handoff-stall",
            "reward-map-loop" => "reward-map-loop",
            "map-overlay-noop-loop" => "map-overlay-noop-loop",
            "map-transition-stall" => "map-transition-stall",
            "phase-mismatch-stall" => "phase-mismatch-stall",
            "decision-wait-plateau" => "decision-wait-plateau",
            "combat-lifecycle-transit-wait-plateau" => "combat-lifecycle-transit-wait-plateau",
            "combat-barrier-wait-plateau" => "combat-barrier-wait-plateau",
            "inspect-overlay-loop" => "inspect-overlay-loop",
            "combat-noop-loop" => "combat-noop-loop",
            "rest-site-post-click-noop" => "rest-site-post-click-noop",
            "rest-site-selection-failed" => "rest-site-selection-failed",
            "rest-site-grid-not-visible-after-selection" => "rest-site-grid-not-visible-after-selection",
            "rest-site-grid-observer-miss" => "rest-site-grid-observer-miss",
            "ancient-event-option-contract-mismatch" => "ancient-event-option-contract-mismatch",
            "post-node-handoff-contract-mismatch" => "post-node-handoff-contract-mismatch",
            "same-action-stall" => "screenshot-heuristic-drift",
            "decision-abort" => "semantic-scene-ambiguity",
            "phase-timeout" => "observer-blindspot",
            "global-timeout" => "observer-blindspot",
            _ => "generic-recovery-failure",
        };
    }

    static bool IsSceneAuthorityInvalidFailure(GuiSmokePhase phase, ObserverState? observer)
    {
        var currentScreen = observer is null ? null : ObserverScreenProvenance.ControlFlowCurrentScreen(observer);
        var visibleScreen = observer is null ? null : ObserverScreenProvenance.ControlFlowVisibleScreen(observer);
        var sceneAuthority = observer is null ? null : ObserverScreenProvenance.ControlFlowSceneAuthority(observer);
        return string.Equals(currentScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase)
               || string.Equals(visibleScreen, "singleplayer-submenu", StringComparison.OrdinalIgnoreCase)
               || string.Equals(currentScreen, "character-select", StringComparison.OrdinalIgnoreCase)
               || string.Equals(visibleScreen, "character-select", StringComparison.OrdinalIgnoreCase)
               || (string.Equals(currentScreen, "main-menu", StringComparison.OrdinalIgnoreCase)
                   && (phase is GuiSmokePhase.EnterRun or GuiSmokePhase.WaitCharacterSelect or GuiSmokePhase.ChooseCharacter)
                   && !string.Equals(sceneAuthority, "observer", StringComparison.OrdinalIgnoreCase));
    }

    static bool IsSceneDeadEndAttempt(GuiSmokeAttemptResult result)
    {
        if (result.LaunchFailed)
        {
            return false;
        }

        return string.Equals(result.TerminalCause, "same-action-stall", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "combat-lifecycle-transit-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "combat-barrier-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "combat-barrier-handoff-mismatch", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "combat-release-failure-under-noncombat-foreground", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "reward-aftermath-card-progression-stall", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "rest-site-release-map-handoff-stall", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "reward-map-loop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "map-overlay-noop-loop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "map-transition-stall", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "combat-lifecycle-transit-wait-plateau", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "combat-barrier-wait-plateau", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "inspect-overlay-loop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "combat-noop-loop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "ancient-event-option-contract-mismatch", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "post-node-handoff-contract-mismatch", StringComparison.OrdinalIgnoreCase)
               || IsRestSitePostClickFailureKind(result.TerminalCause)
               || string.Equals(result.TerminalCause, "decision-abort", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.TerminalCause, "phase-timeout", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "combat-lifecycle-transit-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "combat-barrier-step-budget-exhausted", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "combat-release-failure-under-noncombat-foreground", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "reward-aftermath-card-progression-stall", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "rest-site-release-map-handoff-stall", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "reward-map-loop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "map-overlay-noop-loop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "map-transition-stall", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "phase-mismatch-stall", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "decision-wait-plateau", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "combat-lifecycle-transit-wait-plateau", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "combat-barrier-wait-plateau", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "inspect-overlay-loop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "combat-noop-loop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "combat-barrier-handoff-mismatch", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "ancient-event-option-contract-mismatch", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "post-node-handoff-contract-mismatch", StringComparison.OrdinalIgnoreCase)
               || IsRestSitePostClickFailureKind(result.FailureClass)
               || string.Equals(result.FailureClass, "scene-authority-invalid", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "observer-blindspot", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "semantic-scene-ambiguity", StringComparison.OrdinalIgnoreCase)
               || string.Equals(result.FailureClass, "screenshot-heuristic-drift", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsRestSitePostClickFailureKind(string? value)
    {
        return string.Equals(value, "rest-site-post-click-noop", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "rest-site-selection-failed", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "rest-site-grid-not-visible-after-selection", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "rest-site-grid-observer-miss", StringComparison.OrdinalIgnoreCase);
    }

    static bool ShouldUseHoverPrimedClick(GuiSmokeStepDecision decision)
    {
        return string.Equals(decision.ActionKind, "click", StringComparison.OrdinalIgnoreCase)
               && string.Equals(decision.TargetLabel, "ancient event completion", StringComparison.OrdinalIgnoreCase);
    }

    static void SetHarnessLogSink(Action<string>? sink)
    {
        GuiSmokeShared.HarnessLogSink = sink;
    }

    static bool ShouldRecaptureForObserverDrift(ObserverSummary requestObserver, ObserverState latestObserver, GuiSmokeStepDecision? decision = null)
    {
        if (latestObserver.CapturedAt is null || requestObserver.CapturedAt is null)
        {
            return false;
        }

        if (latestObserver.CapturedAt <= requestObserver.CapturedAt)
        {
            return false;
        }

        if (!string.Equals(
                ObserverScreenProvenance.ControlFlowCurrentScreen(requestObserver),
                ObserverScreenProvenance.ControlFlowCurrentScreen(latestObserver),
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.Equals(
                ObserverScreenProvenance.ControlFlowVisibleScreen(requestObserver),
                ObserverScreenProvenance.ControlFlowVisibleScreen(latestObserver),
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (requestObserver.InCombat != latestObserver.InCombat)
        {
            return true;
        }

        if (!string.Equals(requestObserver.InventoryId, latestObserver.InventoryId, StringComparison.Ordinal))
        {
            if (string.Equals(decision?.ActionKind, "press-key", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        return false;
    }
}
