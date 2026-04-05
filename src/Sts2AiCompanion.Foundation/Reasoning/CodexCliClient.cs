using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2AiCompanion.Foundation.Reasoning.CompactAdvisor;
using Sts2ModKit.Core.Configuration;

namespace Sts2AiCompanion.Foundation.Reasoning;

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
        string? modelOverride,
        string? reasoningEffortOverride,
        CancellationToken cancellationToken)
    {
        var schemaPath = Path.Combine(Path.GetTempPath(), $"sts2-codex-schema-{Guid.NewGuid():N}.json");
        var outputPath = Path.Combine(Path.GetTempPath(), $"sts2-codex-output-{Guid.NewGuid():N}.json");
        var sanitizedPrompt = SanitizePrompt(prompt);

        try
        {
            await File.WriteAllTextAsync(schemaPath, BuildSchema(), cancellationToken).ConfigureAwait(false);
            var before = ReadSessionIndex()
                .Select(entry => entry.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            using var process = new Process
            {
                StartInfo = CreateStartInfo(sessionId, modelOverride, reasoningEffortOverride, schemaPath, outputPath),
            };

            process.Start();
            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.StandardInput.WriteAsync(sanitizedPrompt).ConfigureAwait(false);
            process.StandardInput.Close();
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            var standardOutput = await stdoutTask.ConfigureAwait(false);
            var standardError = await stderrTask.ConfigureAwait(false);
            var execTrace = ParseExecTrace(standardOutput);
            var execError = TryExtractExecErrorMessage(standardOutput)
                ?? TryExtractExecErrorMessage(standardError);
            var resolvedSessionId = sessionId
                ?? execTrace.ThreadId
                ?? ResolveCreatedSessionId(before);

            var rawOutput = File.Exists(outputPath)
                ? await File.ReadAllTextAsync(outputPath, cancellationToken).ConfigureAwait(false)
                : string.Empty;
            if (string.IsNullOrWhiteSpace(rawOutput))
            {
                rawOutput = execTrace.LastAgentMessageJson
                    ?? (string.IsNullOrWhiteSpace(execError)
                        ? ExtractJsonObject(standardOutput)
                        : standardOutput)
                    ?? string.Empty;
            }

            if (process.ExitCode != 0)
            {
                var failureMessage = !string.IsNullOrWhiteSpace(execError)
                    ? execError
                    : $"Codex CLI exited with code {process.ExitCode}.";
                if (string.IsNullOrWhiteSpace(execError) && !string.IsNullOrWhiteSpace(standardError))
                {
                    failureMessage += $" stderr: {TrimDiagnosticText(standardError)}";
                }

                return (CreateDegradedResponse(inputPack, resolvedSessionId, failureMessage, rawOutput), resolvedSessionId);
            }

            if (string.IsNullOrWhiteSpace(rawOutput))
            {
                var emptyMessage = !string.IsNullOrWhiteSpace(execError)
                    ? execError
                    : "Codex가 빈 응답을 반환했습니다.";
                if (string.IsNullOrWhiteSpace(execError) && !string.IsNullOrWhiteSpace(standardError))
                {
                    emptyMessage += $" stderr: {TrimDiagnosticText(standardError)}";
                }

                return (CreateDegradedResponse(inputPack, resolvedSessionId, emptyMessage, rawOutput), resolvedSessionId);
            }

            if (!string.IsNullOrWhiteSpace(execError)
                && string.IsNullOrWhiteSpace(execTrace.LastAgentMessageJson)
                && LooksLikeExecTrace(rawOutput))
            {
                return (CreateDegradedResponse(inputPack, resolvedSessionId, execError, rawOutput), resolvedSessionId);
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<CodexAdviceContract>(rawOutput, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                if (parsed is null)
                {
                    return (CreateDegradedResponse(inputPack, resolvedSessionId, "Codex가 빈 응답을 반환했습니다.", rawOutput), resolvedSessionId);
                }

                var response = new AdviceResponse(
                    "ok",
                    parsed.Headline ?? "AI 조언",
                    parsed.Summary ?? "응답 요약이 비어 있습니다.",
                    parsed.RecommendedAction ?? "현재 상태를 다시 확인하세요.",
                    parsed.RecommendedChoiceLabel,
                    parsed.ReasoningBullets ?? Array.Empty<string>(),
                    parsed.RiskNotes ?? Array.Empty<string>(),
                    parsed.MissingInformation ?? Array.Empty<string>(),
                    parsed.DecisionBlockers ?? Array.Empty<string>(),
                    parsed.Confidence,
                    parsed.KnowledgeRefs ?? Array.Empty<string>(),
                    DateTimeOffset.UtcNow,
                    inputPack.RunId,
                    inputPack.TriggerKind,
                    resolvedSessionId,
                    rawOutput,
                    null,
                    parsed.ConservativeView?.ToContract(),
                    parsed.AggressiveView?.ToContract(),
                    parsed.FinalView?.ToContract());
                return (AdviceResponseFinalizer.Apply(inputPack, response), resolvedSessionId);
            }
            catch (JsonException exception)
            {
                var message = !string.IsNullOrWhiteSpace(execError)
                    ? execError
                    : $"Codex response schema parse failed: {exception.Message}";
                return (CreateDegradedResponse(inputPack, resolvedSessionId, message, rawOutput), resolvedSessionId);
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

    private ProcessStartInfo CreateStartInfo(string? sessionId, string? modelOverride, string? reasoningEffortOverride, string schemaPath, string outputPath)
    {
        var launch = ResolveCodexLaunch();
        var startInfo = new ProcessStartInfo
        {
            FileName = launch.FileName,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(false),
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
        startInfo.ArgumentList.Add("--json");
        startInfo.ArgumentList.Add("--output-schema");
        startInfo.ArgumentList.Add(schemaPath);
        startInfo.ArgumentList.Add("-o");
        startInfo.ArgumentList.Add(outputPath);

        var selectedModel = string.IsNullOrWhiteSpace(modelOverride)
            ? _configuration.Assistant.CodexModel
            : modelOverride;
        if (!string.IsNullOrWhiteSpace(selectedModel))
        {
            startInfo.ArgumentList.Add("-m");
            startInfo.ArgumentList.Add(selectedModel!);
        }

        var selectedReasoningEffort = string.IsNullOrWhiteSpace(reasoningEffortOverride)
            ? _configuration.Assistant.CodexReasoningEffort
            : reasoningEffortOverride;
        if (!string.IsNullOrWhiteSpace(selectedReasoningEffort))
        {
            startInfo.ArgumentList.Add("-c");
            startInfo.ArgumentList.Add($"model_reasoning_effort=\"{selectedReasoningEffort}\"");
        }

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
        var response = new AdviceResponse(
            "degraded",
            "Codex 조언 사용 불가",
            message,
            "현재 상태를 다시 확인하세요.",
            null,
            new[] { "Codex CLI 실행이 실패했거나 응답을 읽을 수 없었습니다." },
            new[] { message },
            Array.Empty<string>(),
            new[] { message },
            null,
            Array.Empty<string>(),
            DateTimeOffset.UtcNow,
            inputPack.RunId,
            inputPack.TriggerKind,
            sessionId,
            rawOutput);
        return AdviceResponseFinalizer.Apply(inputPack, response);
    }

    public static string? TryExtractExecErrorMessage(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        foreach (var rawLine in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(rawLine) || !rawLine.TrimStart().StartsWith('{'))
            {
                continue;
            }

            try
            {
                using var document = JsonDocument.Parse(rawLine);
                var root = document.RootElement;
                if (!root.TryGetProperty("type", out var typeElement))
                {
                    continue;
                }

                var eventType = typeElement.GetString();
                if (string.Equals(eventType, "error", StringComparison.Ordinal)
                    && root.TryGetProperty("message", out var messageElement))
                {
                    return NormalizeExecErrorMessage(messageElement.GetString());
                }

                if (string.Equals(eventType, "turn.failed", StringComparison.Ordinal)
                    && root.TryGetProperty("error", out var errorElement)
                    && errorElement.TryGetProperty("message", out var errorMessageElement))
                {
                    return NormalizeExecErrorMessage(errorMessageElement.GetString());
                }
            }
            catch (JsonException)
            {
            }
        }

        return null;
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

    private static string NormalizeExecErrorMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Codex CLI request failed.";
        }

        if (message.Contains("Your input exceeds the context window of this model", StringComparison.OrdinalIgnoreCase)
            || message.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase))
        {
            return "Codex 요청이 모델 컨텍스트 한도를 초과했습니다. 자동 조언 세션을 새로 시작하거나 입력 크기를 줄여야 합니다.";
        }

        return TrimDiagnosticText(message);
    }

    private static bool LooksLikeExecTrace(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return text.Contains("\"type\":\"thread.started\"", StringComparison.Ordinal)
            || text.Contains("\"type\":\"turn.started\"", StringComparison.Ordinal)
            || text.Contains("\"type\":\"error\"", StringComparison.Ordinal)
            || text.Contains("\"type\":\"turn.failed\"", StringComparison.Ordinal);
    }

    public static (string? ThreadId, string? LastAgentMessageJson) ParseExecTrace(string? standardOutput)
    {
        if (string.IsNullOrWhiteSpace(standardOutput))
        {
            return (null, null);
        }

        string? threadId = null;
        string? lastAgentMessageJson = null;

        foreach (var rawLine in standardOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(rawLine) || !rawLine.TrimStart().StartsWith('{'))
            {
                continue;
            }

            try
            {
                using var document = JsonDocument.Parse(rawLine);
                var root = document.RootElement;
                if (!root.TryGetProperty("type", out var typeElement))
                {
                    continue;
                }

                var eventType = typeElement.GetString();
                if (string.Equals(eventType, "thread.started", StringComparison.Ordinal)
                    && root.TryGetProperty("thread_id", out var threadIdElement))
                {
                    threadId = threadIdElement.GetString();
                    continue;
                }

                if (!string.Equals(eventType, "item.completed", StringComparison.Ordinal)
                    || !root.TryGetProperty("item", out var itemElement)
                    || !itemElement.TryGetProperty("type", out var itemTypeElement)
                    || !string.Equals(itemTypeElement.GetString(), "agent_message", StringComparison.Ordinal)
                    || !itemElement.TryGetProperty("text", out var textElement))
                {
                    continue;
                }

                var text = textElement.GetString();
                if (!string.IsNullOrWhiteSpace(text) && text.TrimStart().StartsWith('{'))
                {
                    lastAgentMessageJson = text;
                }
            }
            catch (JsonException)
            {
            }
        }

        return (threadId, lastAgentMessageJson);
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
            "reasoningBullets": { "type": "array", "items": { "type": "string" } },
            "riskNotes": { "type": "array", "items": { "type": "string" } },
            "missingInformation": { "type": "array", "items": { "type": "string" } },
            "decisionBlockers": { "type": "array", "items": { "type": "string" } },
            "confidence": { "type": ["number", "null"] },
            "knowledgeRefs": { "type": "array", "items": { "type": "string" } }
            ,
            "conservativeView": {
              "type": ["object", "null"],
              "additionalProperties": false,
              "properties": {
                "headline": { "type": "string" },
                "recommendedChoiceLabel": { "type": ["string", "null"] },
                "summary": { "type": "string" },
                "reasoningBullets": { "type": "array", "items": { "type": "string" } },
                "riskNotes": { "type": "array", "items": { "type": "string" } }
              },
              "required": ["headline", "recommendedChoiceLabel", "summary", "reasoningBullets", "riskNotes"]
            },
            "aggressiveView": {
              "type": ["object", "null"],
              "additionalProperties": false,
              "properties": {
                "headline": { "type": "string" },
                "recommendedChoiceLabel": { "type": ["string", "null"] },
                "summary": { "type": "string" },
                "reasoningBullets": { "type": "array", "items": { "type": "string" } },
                "riskNotes": { "type": "array", "items": { "type": "string" } }
              },
              "required": ["headline", "recommendedChoiceLabel", "summary", "reasoningBullets", "riskNotes"]
            },
            "finalView": {
              "type": ["object", "null"],
              "additionalProperties": false,
              "properties": {
                "headline": { "type": "string" },
                "recommendedChoiceLabel": { "type": ["string", "null"] },
                "summary": { "type": "string" },
                "reasoningBullets": { "type": "array", "items": { "type": "string" } },
                "riskNotes": { "type": "array", "items": { "type": "string" } }
              },
              "required": ["headline", "recommendedChoiceLabel", "summary", "reasoningBullets", "riskNotes"]
            }
          },
          "required": [
            "headline",
            "summary",
            "recommendedAction",
            "recommendedChoiceLabel",
            "reasoningBullets",
            "riskNotes",
            "missingInformation",
            "decisionBlockers",
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

    private sealed record SessionIndexEntry(
        string Id,
        string? ThreadName,
        DateTimeOffset UpdatedAt);

    private sealed record CodexAdviceContract(
        string? Headline,
        string? Summary,
        string? RecommendedAction,
        string? RecommendedChoiceLabel,
        IReadOnlyList<string>? ReasoningBullets,
        IReadOnlyList<string>? RiskNotes,
        IReadOnlyList<string>? MissingInformation,
        IReadOnlyList<string>? DecisionBlockers,
        double? Confidence,
        IReadOnlyList<string>? KnowledgeRefs,
        CodexAdviceViewContract? ConservativeView,
        CodexAdviceViewContract? AggressiveView,
        CodexAdviceViewContract? FinalView);

    private sealed record CodexAdviceViewContract(
        string? Headline,
        string? RecommendedChoiceLabel,
        string? Summary,
        IReadOnlyList<string>? ReasoningBullets,
        IReadOnlyList<string>? RiskNotes)
    {
        public AdvicePerspectiveView ToContract()
        {
            return new AdvicePerspectiveView(
                Headline ?? "AI 관점",
                RecommendedChoiceLabel,
                Summary ?? "요약이 비어 있습니다.",
                ReasoningBullets ?? Array.Empty<string>(),
                RiskNotes ?? Array.Empty<string>());
        }
    }
}
