using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Sts2AiCompanion.Host;

public sealed partial class CompanionHost
{
    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return property
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToArray();
    }

    private static IEnumerable<string> SplitJsonObjects(string content)
    {
        var utf8 = Encoding.UTF8.GetBytes(content);
        var offset = 0;
        var chunks = new List<string>();

        while (offset < utf8.Length)
        {
            while (offset < utf8.Length && char.IsWhiteSpace((char)utf8[offset]))
            {
                offset += 1;
            }

            if (offset >= utf8.Length)
            {
                break;
            }

            var reader = new Utf8JsonReader(utf8.AsSpan(offset), isFinalBlock: true, state: default);
            try
            {
                using var document = JsonDocument.ParseValue(ref reader);
                chunks.Add(document.RootElement.GetRawText());
                offset += (int)reader.BytesConsumed;
            }
            catch (JsonException)
            {
                break;
            }
        }

        return chunks;
    }
}
