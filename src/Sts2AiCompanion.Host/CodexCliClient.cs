using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        var schemaPath = Path.Combine(Path.GetTempPath(), $"sts2-codex-schema-{Guid.NewGuid():N}.json");
        var outputPath = Path.Combine(Path.GetTempPath(), $"sts2-codex-output-{Guid.NewGuid():N}.json");
        var sanitizedPrompt = SanitizePrompt(prompt);
        try
        {
            await File.WriteAllTextAsync(schemaPath, BuildSchema(), cancellationToken).ConfigureAwait(false);
            var before = ReadSessionIndex().Select(entry => entry.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

            using var process = new Process
            {
                StartInfo = CreateStartInfo(sessionId, schemaPath, outputPath),
            };

            process.Start();
            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.StandardInput.WriteAsync(sanitizedPrompt).ConfigureAwait(false);
            process.StandardInput.Close();
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            var resolvedSessionId = sessionId ?? ResolveCreatedSessionId(before);
            var standardOutput = await stdoutTask.ConfigureAwait(false);
            var standardError = await stderrTask.ConfigureAwait(false);
            var rawOutput = File.Exists(outputPath)
                ? await File.ReadAllTextAsync(outputPath, cancellationToken).ConfigureAwait(false)
                : string.Empty;
            if (string.IsNullOrWhiteSpace(rawOutput))
            {
                rawOutput = ExtractJsonObject(standardOutput) ?? string.Empty;
            }

            if (process.ExitCode != 0)
            {
                var failureMessage = $"Codex CLI exited with code {process.ExitCode}.";
                if (!string.IsNullOrWhiteSpace(standardError))
                {
                    failureMessage += $" stderr: {TrimDiagnosticText(standardError)}";
                }

                return (CreateDegradedResponse(inputPack, resolvedSessionId, failureMessage, rawOutput), resolvedSessionId);
            }

            if (string.IsNullOrWhiteSpace(rawOutput))
            {
                var emptyMessage = "Codex returned an empty response.";
                if (!string.IsNullOrWhiteSpace(standardError))
                {
                    emptyMessage += $" stderr: {TrimDiagnosticText(standardError)}";
                }

                return (CreateDegradedResponse(inputPack, resolvedSessionId, emptyMessage, rawOutput), resolvedSessionId);
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
        var launch = ResolveCodexLaunch();
        var startInfo = new ProcessStartInfo
        {
            FileName = launch.FileName,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _workspaceRoot,
        };

        foreach (var prefixArgument in launch.ArgumentPrefix)
        {
            startInfo.ArgumentList.Add(prefixArgument);
        }

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

    private (string FileName, IReadOnlyList<string> ArgumentPrefix) ResolveCodexLaunch()
    {
        var configured = _configuration.Assistant.CodexCommand;
        var resolved = ResolveCommandPath(configured) ?? configured;

        if (OperatingSystem.IsWindows()
            && (resolved.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase)
                || resolved.EndsWith(".bat", StringComparison.OrdinalIgnoreCase)))
        {
            return (Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe", new[] { "/d", "/c", resolved });
        }

        return (resolved, Array.Empty<string>());
    }

    private static string? ResolveCommandPath(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return null;
        }

        if (Path.IsPathRooted(command)
            || command.Contains(Path.DirectorySeparatorChar)
            || command.Contains(Path.AltDirectorySeparatorChar))
        {
            return File.Exists(command) ? command : null;
        }

        var pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return null;
        }

        var extensions = OperatingSystem.IsWindows()
            ? ExpandPathExtensions(command)
            : new[] { string.Empty };

        foreach (var directory in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            foreach (var extension in extensions)
            {
                var candidate = Path.Combine(directory, command + extension);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static IReadOnlyList<string> ExpandPathExtensions(string command)
    {
        if (Path.HasExtension(command))
        {
            return new[] { string.Empty };
        }

        var pathExt = Environment.GetEnvironmentVariable("PATHEXT");
        var extensions = string.IsNullOrWhiteSpace(pathExt)
            ? new[] { ".exe", ".cmd", ".bat" }
            : pathExt.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new[] { string.Empty }
            .Concat(extensions
                .Select(extension => extension.StartsWith('.') ? extension : "." + extension)
                .Distinct(StringComparer.OrdinalIgnoreCase))
            .ToArray();
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

    private static string? ExtractJsonObject(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return null;
        }

        return text[start..(end + 1)];
    }

    private static string TrimDiagnosticText(string text)
    {
        var compact = text
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\n', ' ')
            .Trim();
        return compact.Length <= 400
            ? compact
            : compact[..400] + "...";
    }

    private static string SanitizePrompt(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(prompt.Length);
        for (var index = 0; index < prompt.Length; index += 1)
        {
            var character = prompt[index];
            if (char.IsSurrogate(character))
            {
                if (char.IsHighSurrogate(character)
                    && index + 1 < prompt.Length
                    && char.IsLowSurrogate(prompt[index + 1]))
                {
                    builder.Append(character);
                    builder.Append(prompt[index + 1]);
                    index += 1;
                    continue;
                }

                builder.Append('\uFFFD');
                continue;
            }

            if (char.IsControl(character) && character is not '\r' and not '\n' and not '\t')
            {
                continue;
            }

            builder.Append(character);
        }

        return builder.ToString();
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
