using System.Text.Json;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Harness.Evaluation;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;

namespace Sts2ModKit.Tool;

internal static class HarnessCommands
{
    public static Task<HarnessScenarioCommandResult> RunScenarioAsync(
        ScaffoldConfiguration configuration,
        string workspaceRoot,
        string scenarioPath,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("run-harness-scenario is disabled until the post-clean-boot cycle.");
    }

    public static HarnessRunInspectionResult InspectRun(string workspaceRoot, string runId)
    {
        throw new InvalidOperationException("inspect-harness-run is disabled until the post-clean-boot cycle.");
    }

    public static async Task<HarnessArmSession> ArmSessionAsync(
        ScaffoldConfiguration configuration,
        string reason,
        CancellationToken cancellationToken)
    {
        var layout = HarnessPathResolver.Resolve(configuration.GamePaths, configuration.Harness);
        HarnessPathResolver.EnsureDirectories(layout);

        var issuedAt = DateTimeOffset.UtcNow;
        var ttl = TimeSpan.FromMilliseconds(Math.Max(configuration.Harness.StepTimeoutMs * 8, 600_000));
        var session = new HarnessArmSession(
            Guid.NewGuid().ToString("N"),
            issuedAt,
            issuedAt.Add(ttl),
            string.IsNullOrWhiteSpace(reason) ? "manual-arm" : reason.Trim());

        LiveExportAtomicFileWriter.WriteJsonAtomic(layout.ArmSessionPath, session, ProgramJson.SerializerOptions);
        await Task.CompletedTask.ConfigureAwait(false);
        return session;
    }

    public static HarnessControlInspectionResult DisarmSession(ScaffoldConfiguration configuration)
    {
        var layout = HarnessPathResolver.Resolve(configuration.GamePaths, configuration.Harness);
        HarnessPathResolver.EnsureDirectories(layout);
        try
        {
            if (File.Exists(layout.ArmSessionPath))
            {
                File.Delete(layout.ArmSessionPath);
            }
        }
        catch
        {
            // Best-effort disarm so manual clean boot can still be inspected.
        }

        return InspectControl(configuration);
    }

    public static HarnessControlInspectionResult InspectControl(ScaffoldConfiguration configuration)
    {
        var layout = HarnessPathResolver.Resolve(configuration.GamePaths, configuration.Harness);
        HarnessPathResolver.EnsureDirectories(layout);

        var armSession = TryReadArmSession(layout);
        var armExpired = armSession is not null && armSession.ExpiresAt <= DateTimeOffset.UtcNow;
        var inventory = TryReadInventory(layout);
        return new HarnessControlInspectionResult(
            layout.HarnessRoot,
            layout.StatusPath,
            layout.ArmSessionPath,
            layout.InventoryPath,
            TryReadBridgeStatus(layout),
            armSession,
            armExpired,
            inventory?.InventoryId,
            inventory?.SceneType,
            inventory?.Mode,
            inventory?.BlockingModal,
            inventory?.Nodes.Count ?? 0);
    }

    public static Task<HarnessActionResult> DispatchNodeAsync(
        ScaffoldConfiguration configuration,
        string inventoryId,
        string nodeId,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("dispatch-harness-node is disabled until the post-clean-boot cycle.");
    }

    private static HarnessArmSession? TryReadArmSession(HarnessQueueLayout layout)
    {
        if (!File.Exists(layout.ArmSessionPath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<HarnessArmSession>(
                File.ReadAllText(layout.ArmSessionPath),
                ProgramJson.SerializerOptions);
        }
        catch
        {
            return null;
        }
    }

    private static HarnessBridgeStatus? TryReadBridgeStatus(HarnessQueueLayout layout)
    {
        if (!File.Exists(layout.StatusPath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<HarnessBridgeStatus>(
                File.ReadAllText(layout.StatusPath),
                ProgramJson.SerializerOptions);
        }
        catch
        {
            return null;
        }
    }

    private static HarnessNodeInventory? TryReadInventory(HarnessQueueLayout layout)
    {
        if (!File.Exists(layout.InventoryPath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<HarnessNodeInventory>(
                File.ReadAllText(layout.InventoryPath),
                ProgramJson.SerializerOptions);
        }
        catch
        {
            return null;
        }
    }
}

internal sealed record HarnessScenarioCommandResult(
    string ScenarioPath,
    string RunId,
    string Status,
    string ScenarioOutputPath,
    string ActionsPath,
    string ResultsPath,
    string EvaluationPath,
    string ReplayBundleRoot,
    string FinalScene,
    IReadOnlyList<string> MissingRequiredScenes);

internal sealed record HarnessRunInspectionResult(
    string RunId,
    string RunRoot,
    int ActionCount,
    int ResultCount,
    AcceptanceReport? Evaluation);

internal sealed record HarnessControlInspectionResult(
    string HarnessRoot,
    string StatusPath,
    string ArmSessionPath,
    string InventoryPath,
    HarnessBridgeStatus? Status,
    HarnessArmSession? ArmSession,
    bool ArmSessionExpired,
    string? InventoryId,
    string? SceneType,
    string? InventoryMode,
    string? BlockingModal,
    int NodeCount);

