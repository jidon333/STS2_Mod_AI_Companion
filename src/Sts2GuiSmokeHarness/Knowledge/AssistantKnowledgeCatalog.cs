using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;

static class AssistantKnowledgeCatalog
{
    private static readonly object Sync = new();
    private static string? _workspaceRoot;
    private static IReadOnlyList<AssistantCardKnowledge>? _cards;
    private static IReadOnlyList<AssistantEventKnowledge>? _events;

    public static IReadOnlyList<AssistantCardKnowledge> LoadCards(string workspaceRoot)
    {
        lock (Sync)
        {
            EnsureLoaded(workspaceRoot);
            return _cards ?? Array.Empty<AssistantCardKnowledge>();
        }
    }

    public static IReadOnlyList<AssistantEventKnowledge> LoadEvents(string workspaceRoot)
    {
        lock (Sync)
        {
            EnsureLoaded(workspaceRoot);
            return _events ?? Array.Empty<AssistantEventKnowledge>();
        }
    }

    private static void EnsureLoaded(string workspaceRoot)
    {
        if (string.Equals(_workspaceRoot, workspaceRoot, StringComparison.Ordinal) && _cards is not null && _events is not null)
        {
            return;
        }

        _workspaceRoot = workspaceRoot;
        _cards = LoadCardsCore(Path.Combine(workspaceRoot, "artifacts", "knowledge", "assistant", "cards.json"));
        _events = LoadEventsCore(Path.Combine(workspaceRoot, "artifacts", "knowledge", "assistant", "events.json"));
    }

    private static IReadOnlyList<AssistantCardKnowledge> LoadCardsCore(string path)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<AssistantCardKnowledge>();
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<AssistantCardKnowledge>();
            }

            var cards = new List<AssistantCardKnowledge>();
            foreach (var element in document.RootElement.EnumerateArray())
            {
                var id = TryReadString(element, "id");
                var name = TryReadString(element, "name") ?? TryReadString(element, "title");
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                cards.Add(new AssistantCardKnowledge(
                    id,
                    name,
                    TryReadString(element, "type"),
                    TryReadString(element, "target"),
                    TryReadInt32(element, "cost"),
                    BuildCardMatchKeys(element, name)));
            }

            return cards;
        }
        catch
        {
            return Array.Empty<AssistantCardKnowledge>();
        }
    }

    private static IReadOnlyList<AssistantEventKnowledge> LoadEventsCore(string path)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<AssistantEventKnowledge>();
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<AssistantEventKnowledge>();
            }

            var events = new List<AssistantEventKnowledge>();
            foreach (var element in document.RootElement.EnumerateArray())
            {
                var id = TryReadString(element, "id");
                var title = TryReadString(element, "title") ?? TryReadString(element, "name");
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                var options = new List<AssistantEventOptionKnowledge>();
                if (element.TryGetProperty("options", out var optionsElement) && optionsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var option in optionsElement.EnumerateArray())
                    {
                        var label = TryReadString(option, "label");
                        if (string.IsNullOrWhiteSpace(label))
                        {
                            continue;
                        }

                        options.Add(new AssistantEventOptionKnowledge(
                            label,
                            TryReadString(option, "description"),
                            TryReadNestedString(option, "attributes", "optionKey")));
                    }
                }

                events.Add(new AssistantEventKnowledge(id, title, options));
            }

            return events;
        }
        catch
        {
            return Array.Empty<AssistantEventKnowledge>();
        }
    }

    private static IReadOnlyList<string> BuildCardMatchKeys(JsonElement element, string name)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal)
        {
            NormalizeKey(name),
        };

        var title = TryReadString(element, "title");
        if (!string.IsNullOrWhiteSpace(title))
        {
            keys.Add(NormalizeKey(title));
        }

        var englishTitle = TryReadString(element, "englishTitle");
        if (!string.IsNullOrWhiteSpace(englishTitle))
        {
            keys.Add(NormalizeKey(englishTitle));
        }

        var classId = TryReadString(element, "classId");
        if (!string.IsNullOrWhiteSpace(classId))
        {
            keys.Add(NormalizeKey(classId));
        }

        return keys.Where(static key => key.Length > 0).ToArray();
    }

    private static string NormalizeKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var buffer = new char[value.Length];
        var length = 0;
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                buffer[length] = char.ToLowerInvariant(character);
                length += 1;
            }
        }

        return length == 0
            ? string.Empty
            : new string(buffer, 0, length);
    }

    private static string? TryReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static string? TryReadNestedString(JsonElement element, string objectPropertyName, string propertyName)
    {
        return element.TryGetProperty(objectPropertyName, out var objectProperty)
               && objectProperty.ValueKind == JsonValueKind.Object
               && objectProperty.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static int? TryReadInt32(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetInt32(out var numeric) => numeric,
            JsonValueKind.String when int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null,
        };
    }
}
