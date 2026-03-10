using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Sts2ModKit.Core.Configuration;

namespace Sts2AiCompanion.Host;

public sealed class CodexCliClient : ICodexSessionClient
{
    private readonly ScaffoldConfiguration _configuration;
    private readonly string _workspaceRoot;
    private readonly string _sessionIndexPath;

    public CodexCliClient(ScaffoldConfiguration configuration, string workspaceRoot)
    {
        _configuration = configuration;
        _workspaceRoot = workspaceRoot;
        _sessionIndexPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".codex",
            "session_index.jsonl");
    }

    public async Task<(AdviceResponse Response, string? SessionId)> ExecuteAsync(
        AdviceInputPack inputPack,
        string prompt,
        string? sessionId,
        CancellationToken cancellationToken)
    {
        var schemaPath = Path.GetTempFileName();
        var outputPath = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(schemaPath, BuildSchema(), cancellationToken).ConfigureAwait(false);
            var before = ReadSessionIndex().Select(entry => entry.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

            using var process = new Process
            {
                StartInfo = CreateStartInfo(sessionId, schemaPath, outputPath),
            };

            process.Start();
            await process.StandardInput.WriteAsync(prompt).ConfigureAwait(false);
            process.StandardInput.Close();
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            var resolvedSessionId = sessionId ?? ResolveCreatedSessionId(before);
            var rawOutput = File.Exists(outputPath)
                ? await File.ReadAllTextAsync(outputPath, cancellationToken).ConfigureAwait(false)
                : string.Empty;

            if (process.ExitCode != 0)
            {
                return (CreateDegradedResponse(inputPack, resolvedSessionId, $"Codex CLI exited with code {process.ExitCode}.", rawOutput), resolvedSessionId);
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<CodexAdviceContract>(rawOutput, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                if (parsed is null)
                {
                    return (CreateDegradedResponse(inputPack, resolvedSessionId, "Codex returned an empty response.", rawOutput), resolvedSessionId);
                }

                return (new AdviceResponse(
                    "ok",
                    parsed.Headline ?? "AI advice",
                    parsed.Summary ?? "The response did not include a summary.",
                    parsed.RecommendedAction ?? "Re-check the current state before deciding.",
                    parsed.RecommendedChoiceLabel,
                    parsed.ReasoningBullets ?? Array.Empty<string>(),
                    parsed.RiskNotes ?? Array.Empty<string>(),
                    parsed.Confidence,
                    parsed.KnowledgeRefs ?? Array.Empty<string>(),
                    DateTimeOffset.UtcNow,
                    inputPack.RunId,
                    inputPack.TriggerKind,
                    resolvedSessionId,
                    rawOutput), resolvedSessionId);
            }
            catch (JsonException exception)
            {
                return (CreateDegradedResponse(inputPack, resolvedSessionId, $"Codex response schema parse failed: {exception.Message}", rawOutput), resolvedSessionId);
            }
        }
        catch (Exception exception)
        {
            return (CreateDegradedResponse(inputPack, sessionId, exception.Message, null), sessionId);
        }
        finally
        {
            SafeDelete(schemaPath);
            SafeDelete(outputPath);
        }
    }

    private ProcessStartInfo CreateStartInfo(string? sessionId, string schemaPath, string outputPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _configuration.Assistant.CodexCommand,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _workspaceRoot,
        };

        startInfo.ArgumentList.Add("exec");
        startInfo.ArgumentList.Add("-C");
        startInfo.ArgumentList.Add(_workspaceRoot);
        startInfo.ArgumentList.Add("--output-schema");
        startInfo.ArgumentList.Add(schemaPath);
        startInfo.ArgumentList.Add("-o");
        startInfo.ArgumentList.Add(outputPath);
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            startInfo.ArgumentList.Add("resume");
            startInfo.ArgumentList.Add(sessionId!);
        }

        startInfo.ArgumentList.Add("-");
        return startInfo;
    }

    private string? ResolveCreatedSessionId(IReadOnlySet<string> before)
    {
        return ReadSessionIndex()
            .Where(entry => !before.Contains(entry.Id))
            .OrderByDescending(entry => entry.UpdatedAt)
            .Select(entry => entry.Id)
            .FirstOrDefault()
            ?? ReadSessionIndex()
                .OrderByDescending(entry => entry.UpdatedAt)
                .Select(entry => entry.Id)
                .FirstOrDefault();
    }

    private IReadOnlyList<SessionIndexEntry> ReadSessionIndex()
    {
        if (!File.Exists(_sessionIndexPath))
        {
            return Array.Empty<SessionIndexEntry>();
        }

        var entries = new List<SessionIndexEntry>();
        foreach (var line in File.ReadLines(_sessionIndexPath))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<SessionIndexJson>(line);
                if (parsed is null || string.IsNullOrWhiteSpace(parsed.Id))
                {
                    continue;
                }

                entries.Add(new SessionIndexEntry(parsed.Id, parsed.ThreadName, parsed.UpdatedAt ?? DateTimeOffset.MinValue));
            }
            catch (JsonException)
            {
            }
        }

        return entries;
    }

    private static AdviceResponse CreateDegradedResponse(AdviceInputPack inputPack, string? sessionId, string message, string? rawOutput)
    {
        return new AdviceResponse(
            "degraded",
            "Codex advice is unavailable",
            message,
            "Review the current state and choices manually.",
            null,
            new[] { "The Codex CLI failed or returned an invalid response." },
            new[] { message },
            null,
            Array.Empty<string>(),
            DateTimeOffset.UtcNow,
            inputPack.RunId,
            inputPack.TriggerKind,
            sessionId,
            rawOutput);
    }

    private static void SafeDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
        }
    }

    private static string BuildSchema()
    {
        return """
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "headline": { "type": "string" },
            "summary": { "type": "string" },
            "recommendedAction": { "type": "string" },
            "recommendedChoiceLabel": { "type": ["string", "null"] },
            "reasoningBullets": {
              "type": "array",
              "items": { "type": "string" }
            },
            "riskNotes": {
              "type": "array",
              "items": { "type": "string" }
            },
            "confidence": { "type": ["number", "null"] },
            "knowledgeRefs": {
              "type": "array",
              "items": { "type": "string" }
            }
          },
          "required": [
            "headline",
            "summary",
            "recommendedAction",
            "recommendedChoiceLabel",
            "reasoningBullets",
            "riskNotes",
            "confidence",
            "knowledgeRefs"
          ]
        }
        """;
    }

    private sealed record SessionIndexJson(
        string Id,
        string? ThreadName,
        DateTimeOffset? UpdatedAt);

    private sealed record CodexAdviceContract(
        string? Headline,
        string? Summary,
        string? RecommendedAction,
        string? RecommendedChoiceLabel,
        IReadOnlyList<string>? ReasoningBullets,
        IReadOnlyList<string>? RiskNotes,
        double? Confidence,
        IReadOnlyList<string>? KnowledgeRefs);
}
