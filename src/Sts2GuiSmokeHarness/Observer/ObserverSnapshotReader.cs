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

sealed class ObserverSnapshotReader
{
    private readonly LiveExportLayout _liveLayout;
    private readonly HarnessQueueLayout _harnessLayout;
    private string? _cachedEventsPath;
    private long? _cachedEventsLength;
    private DateTime _cachedEventsLastWriteUtc;
    private string[]? _cachedEventTail;

    public ObserverSnapshotReader(LiveExportLayout liveLayout, HarnessQueueLayout harnessLayout)
    {
        _liveLayout = liveLayout;
        _harnessLayout = harnessLayout;
    }

    public ObserverState Read(bool includeEventTail = true)
    {
        JsonDocument? stateDocument = TryReadJson(_liveLayout.SnapshotPath);
        JsonDocument? inventoryDocument = TryReadJson(_harnessLayout.InventoryPath);
        var eventLines = includeEventTail ? TryReadTailCached(_liveLayout.EventsPath, 10) : null;

        var inventorySceneType = TryReadString(inventoryDocument?.RootElement, "sceneType");
        var inventoryRawCurrentScreen = TryReadString(inventoryDocument?.RootElement, "rawCurrentScreen")
                                        ?? TryReadString(inventoryDocument?.RootElement, "rawSceneType")
                                    ?? inventorySceneType;
        var inventoryPublishedCurrentScreen = TryReadString(inventoryDocument?.RootElement, "publishedCurrentScreen")
                                              ?? TryReadString(inventoryDocument?.RootElement, "publishedSceneType");
        var inventoryPublishedVisibleScreen = TryReadString(inventoryDocument?.RootElement, "publishedVisibleScreen")
                                              ?? TryReadString(inventoryDocument?.RootElement, "publishedVisibleScene");
        var inventoryCompatibilityCurrentScreen = TryReadString(inventoryDocument?.RootElement, "compatibilityCurrentScreen")
                                                  ?? TryReadString(inventoryDocument?.RootElement, "compatibilitySceneType")
                                              ?? inventorySceneType;
        var compatibilityCurrentScreen = TryReadNestedString(stateDocument?.RootElement, "meta", "compatibilityCurrentScreen")
                                         ?? TryReadNestedString(stateDocument?.RootElement, "meta", "compatLogicalScreen")
                                         ?? inventoryCompatibilityCurrentScreen;
        var compatibilityLogicalScreen = TryReadNestedString(stateDocument?.RootElement, "meta", "compatLogicalScreen")
                                         ?? TryReadString(inventoryDocument?.RootElement, "compatibilityLogicalScreen")
                                         ?? TryReadString(inventoryDocument?.RootElement, "compatibilitySceneType")
                                         ?? inventoryCompatibilityCurrentScreen;
        var rawCurrentScreen = TryReadNestedString(stateDocument?.RootElement, "meta", "rawCurrentScreen")
                               ?? inventoryRawCurrentScreen;
        var rawObservedScreen = TryReadNestedString(stateDocument?.RootElement, "meta", "rawObservedScreen")
                               ?? TryReadNestedString(stateDocument?.RootElement, "meta", "screen")
                               ?? rawCurrentScreen
                               ?? inventoryRawCurrentScreen
                               ?? compatibilityCurrentScreen;
        var publishedCurrentScreen = TryReadString(stateDocument?.RootElement, "currentScreen")
                                     ?? TryReadNestedString(stateDocument?.RootElement, "meta", "logicalScreen")
                                     ?? inventoryPublishedCurrentScreen
                                     ?? inventorySceneType;
        var currentScreen = publishedCurrentScreen
                            ?? rawCurrentScreen
                            ?? rawObservedScreen
                            ?? compatibilityCurrentScreen
                            ?? compatibilityLogicalScreen;
        var publishedVisibleScreen = TryReadNestedString(stateDocument?.RootElement, "meta", "visibleScreen")
                                     ?? inventoryPublishedVisibleScreen
                                     ?? TryReadString(inventoryDocument?.RootElement, "visibleScreen")
                                     ?? publishedCurrentScreen
                                     ?? inventoryPublishedCurrentScreen;
        var snapshotVersion = TryReadInt64(stateDocument?.RootElement, "version");
        var compatibilityVisibleScreen = TryReadNestedString(stateDocument?.RootElement, "meta", "compatibilityVisibleScreen")
                                         ?? TryReadString(inventoryDocument?.RootElement, "compatibilityVisibleScreen")
                                         ?? TryReadNestedString(stateDocument?.RootElement, "meta", "compatVisibleScreen")
                                         ?? TryReadString(inventoryDocument?.RootElement, "compatibilityVisibleScene")
                                         ?? inventoryCompatibilityCurrentScreen;
        var compatibilityVisibleObservedScreen = TryReadNestedString(stateDocument?.RootElement, "meta", "compatVisibleScreen")
                                                 ?? TryReadString(inventoryDocument?.RootElement, "compatibilityVisibleScene")
                                                 ?? compatibilityVisibleScreen;
        var visibleScreen = publishedVisibleScreen
                            ?? rawObservedScreen
                            ?? rawCurrentScreen
                            ?? compatibilityVisibleScreen
                            ?? compatibilityVisibleObservedScreen
                            ?? currentScreen;
        var inCombat = TryReadBool(stateDocument?.RootElement, "encounter", "inCombat");
        var capturedAt = TryReadDateTimeOffset(stateDocument?.RootElement, "capturedAt")
                         ?? TryReadDateTimeOffset(inventoryDocument?.RootElement, "capturedAt");
        var inventoryId = inventoryDocument is null
            ? null
            : TryReadString(inventoryDocument.RootElement, "inventoryId");
        var inventoryPublishedSceneReady = TryReadBool(inventoryDocument?.RootElement, "publishedSceneReady");
        var compatibilitySceneReady = TryReadNestedBool(stateDocument?.RootElement, "meta", "compatSceneReady")
                                      ?? TryReadBool(inventoryDocument?.RootElement, "compatibilitySceneReady")
                                      ?? TryReadBool(inventoryDocument?.RootElement, "sceneReady");
        var publishedSceneReady = TryReadNestedBool(stateDocument?.RootElement, "meta", "sceneReady")
                                  ?? inventoryPublishedSceneReady
                                  ?? TryReadBool(inventoryDocument?.RootElement, "sceneReady");
        var sceneReady = publishedSceneReady ?? compatibilitySceneReady;
        var inventoryPublishedSceneAuthority = TryReadString(inventoryDocument?.RootElement, "publishedSceneAuthority");
        var compatibilitySceneAuthority = TryReadNestedString(stateDocument?.RootElement, "meta", "compatSceneAuthority")
                                          ?? TryReadString(inventoryDocument?.RootElement, "compatibilitySceneAuthority")
                                          ?? TryReadString(inventoryDocument?.RootElement, "sceneAuthority");
        var publishedSceneAuthority = TryReadNestedString(stateDocument?.RootElement, "meta", "sceneAuthority")
                                      ?? inventoryPublishedSceneAuthority
                                      ?? TryReadString(inventoryDocument?.RootElement, "sceneAuthority");
        var sceneAuthority = publishedSceneAuthority ?? compatibilitySceneAuthority;
        var inventoryPublishedSceneStability = TryReadString(inventoryDocument?.RootElement, "publishedSceneStability");
        var compatibilitySceneStability = TryReadNestedString(stateDocument?.RootElement, "meta", "compatSceneStability")
                                          ?? TryReadString(inventoryDocument?.RootElement, "compatibilitySceneStability")
                                          ?? TryReadString(inventoryDocument?.RootElement, "sceneStability");
        var publishedSceneStability = TryReadNestedString(stateDocument?.RootElement, "meta", "sceneStability")
                                      ?? inventoryPublishedSceneStability
                                      ?? TryReadString(inventoryDocument?.RootElement, "sceneStability");
        var sceneStability = publishedSceneStability ?? compatibilitySceneStability;
        var sceneEpisodeId = TryReadString(inventoryDocument?.RootElement, "sceneEpisodeId")
                             ?? TryReadNestedString(stateDocument?.RootElement, "meta", "screen-episode");
        var encounterKind = TryReadNestedString(stateDocument?.RootElement, "encounter", "kind");
        var choiceExtractorPath = TryReadNestedString(stateDocument?.RootElement, "meta", "choiceExtractorPath");
        var playerCurrentHp = TryReadInt32(stateDocument?.RootElement, "player", "currentHp");
        var playerMaxHp = TryReadInt32(stateDocument?.RootElement, "player", "maxHp");
        var playerEnergy = TryReadInt32(stateDocument?.RootElement, "player", "energy");
        var combatHand = ParseCombatHandSummary(TryReadNestedString(stateDocument?.RootElement, "meta", "combatHandSummary"));
        var currentChoices = ReadChoiceLabels(stateDocument);
        var meta = ReadMetaDictionary(stateDocument);

        return new ObserverState(
            new ObserverSummary(currentScreen, visibleScreen, inCombat, capturedAt, inventoryId, sceneReady, sceneAuthority, sceneStability, sceneEpisodeId, encounterKind, choiceExtractorPath, playerCurrentHp, playerMaxHp, playerEnergy, currentChoices, eventLines ?? Array.Empty<string>(), ReadActionNodes(inventoryDocument), ReadChoices(stateDocument), combatHand)
            {
                SnapshotVersion = snapshotVersion,
                RawCurrentScreen = rawCurrentScreen,
                RawObservedScreen = rawObservedScreen,
                PublishedCurrentScreen = publishedCurrentScreen,
                PublishedVisibleScreen = publishedVisibleScreen,
                PublishedSceneReady = publishedSceneReady,
                PublishedSceneAuthority = publishedSceneAuthority,
                PublishedSceneStability = publishedSceneStability,
                CompatibilityCurrentScreen = compatibilityCurrentScreen,
                CompatibilityLogicalScreen = compatibilityLogicalScreen,
                CompatibilityVisibleScreen = compatibilityVisibleScreen,
                CompatibilityVisibleObservedScreen = compatibilityVisibleObservedScreen,
                CompatibilitySceneReady = compatibilitySceneReady,
                CompatibilitySceneAuthority = compatibilitySceneAuthority,
                CompatibilitySceneStability = compatibilitySceneStability,
                Meta = meta,
            },
            stateDocument,
            inventoryDocument,
            eventLines);
    }

    public static IReadOnlyList<ObserverChoice> ParseChoicesForTesting(JsonDocument? document)
    {
        return ReadChoices(document);
    }

    public static IReadOnlyDictionary<string, string?> ParseMetaForTesting(JsonDocument? document)
    {
        return ReadMetaDictionary(document);
    }

    private static JsonDocument? TryReadJson(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return JsonDocument.Parse(stream);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private string[]? TryReadTailCached(string path, int lines)
    {
        if (!File.Exists(path))
        {
            _cachedEventsPath = path;
            _cachedEventsLength = null;
            _cachedEventsLastWriteUtc = default;
            _cachedEventTail = null;
            return null;
        }

        try
        {
            var fileInfo = new FileInfo(path);
            var length = fileInfo.Length;
            var lastWriteUtc = fileInfo.LastWriteTimeUtc;
            if (string.Equals(_cachedEventsPath, path, StringComparison.Ordinal)
                && _cachedEventsLength == length
                && _cachedEventsLastWriteUtc == lastWriteUtc)
            {
                return _cachedEventTail;
            }

            var tail = TryReadTail(path, lines);
            _cachedEventsPath = path;
            _cachedEventsLength = length;
            _cachedEventsLastWriteUtc = lastWriteUtc;
            _cachedEventTail = tail;
            return tail;
        }
        catch (IOException)
        {
            return _cachedEventTail;
        }
    }

    private static string[]? TryReadTail(string path, int lines)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            if (stream.Length == 0)
            {
                return Array.Empty<string>();
            }

            const int chunkSize = 4096;
            var buffer = new byte[chunkSize];
            var chunks = new List<byte[]>();
            var newlineCount = 0;
            var position = stream.Length;
            while (position > 0 && newlineCount <= lines)
            {
                var readSize = (int)Math.Min(chunkSize, position);
                position -= readSize;
                stream.Seek(position, SeekOrigin.Begin);
                var read = stream.Read(buffer, 0, readSize);
                if (read <= 0)
                {
                    break;
                }

                var chunk = new byte[read];
                Buffer.BlockCopy(buffer, 0, chunk, 0, read);
                newlineCount += chunk.Count(static value => value == (byte)'\n');
                chunks.Add(chunk);
            }

            chunks.Reverse();
            var totalLength = chunks.Sum(static chunk => chunk.Length);
            var combined = new byte[totalLength];
            var offset = 0;
            foreach (var chunk in chunks)
            {
                Buffer.BlockCopy(chunk, 0, combined, offset, chunk.Length);
                offset += chunk.Length;
            }

            var text = Encoding.UTF8.GetString(combined);
            using var reader = new StringReader(text);
            var queue = new Queue<string>(Math.Max(1, lines));
            if (position > 0)
            {
                reader.ReadLine();
            }

            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                queue.Enqueue(line);
                while (queue.Count > lines)
                {
                    queue.Dequeue();
                }
            }

            return queue.ToArray();
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static IReadOnlyList<string> ReadChoiceLabels(JsonDocument? document)
    {
        if (document is null
            || !document.RootElement.TryGetProperty("currentChoices", out var choicesElement)
            || choicesElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        var labels = new List<string>();
        foreach (var choice in choicesElement.EnumerateArray())
        {
            if (choice.ValueKind == JsonValueKind.String)
            {
                labels.Add(choice.GetString() ?? string.Empty);
                continue;
            }

            if (choice.ValueKind == JsonValueKind.Object && choice.TryGetProperty("label", out var labelElement))
            {
                labels.Add(labelElement.GetString() ?? string.Empty);
            }
        }

        return labels;
    }

    private static IReadOnlyList<ObserverActionNode> ReadActionNodes(JsonDocument? document)
    {
        if (document is null
            || !document.RootElement.TryGetProperty("nodes", out var nodesElement)
            || nodesElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ObserverActionNode>();
        }

        var nodes = new List<ObserverActionNode>();
        foreach (var node in nodesElement.EnumerateArray())
        {
            if (node.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var nodeId = TryReadString(node, "nodeId");
            var kind = TryReadString(node, "kind");
            var label = TryReadString(node, "label");
            var screenBounds = TryReadString(node, "screenBounds");
            var typeName = TryReadString(node, "typeName");
            var semanticHints = TryReadStringArray(node, "semanticHints");
            var actionable = node.TryGetProperty("actionable", out var actionableElement)
                             && actionableElement.ValueKind == JsonValueKind.True;
            if (string.IsNullOrWhiteSpace(nodeId)
                || string.IsNullOrWhiteSpace(kind)
                || string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            nodes.Add(new ObserverActionNode(nodeId, kind, label, screenBounds, actionable)
            {
                TypeName = typeName,
                SemanticHints = semanticHints,
            });
        }

        return nodes;
    }

    private static IReadOnlyList<ObserverChoice> ReadChoices(JsonDocument? document)
    {
        if (document is null
            || !document.RootElement.TryGetProperty("currentChoices", out var choicesElement)
            || choicesElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ObserverChoice>();
        }

        var choices = new List<ObserverChoice>();
        foreach (var choice in choicesElement.EnumerateArray())
        {
            if (choice.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var kind = TryReadString(choice, "kind") ?? "choice";
            var label = TryReadString(choice, "label");
            var screenBounds = TryReadString(choice, "screenBounds");
            var value = TryReadString(choice, "value");
            var description = TryReadString(choice, "description");
            var nodeId = TryReadString(choice, "nodeId");
            var bindingKind = TryReadString(choice, "bindingKind");
            var bindingId = TryReadString(choice, "bindingId");
            var enabled = TryReadBool(choice, "enabled");
            var iconAssetPath = TryReadString(choice, "iconAssetPath");
            var semanticHints = TryReadStringArray(choice, "semanticHints");
            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            choices.Add(new ObserverChoice(kind, label, screenBounds, value, description)
            {
                NodeId = nodeId,
                BindingKind = bindingKind,
                BindingId = bindingId,
                Enabled = enabled,
                IconAssetPath = iconAssetPath,
                SemanticHints = semanticHints,
            });
        }

        return choices;
    }

    private static IReadOnlyDictionary<string, string?> ReadMetaDictionary(JsonDocument? document)
    {
        if (document is null
            || !document.RootElement.TryGetProperty("meta", out var metaElement)
            || metaElement.ValueKind != JsonValueKind.Object)
        {
            return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }

        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in metaElement.EnumerateObject())
        {
            values[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Number => property.Value.GetRawText(),
                JsonValueKind.Null => null,
                _ => property.Value.GetRawText(),
            };
        }

        return values;
    }

    private static IReadOnlyList<ObservedCombatHandCard> ParseCombatHandSummary(string? summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return Array.Empty<ObservedCombatHandCard>();
        }

        var cards = new List<ObservedCombatHandCard>();
        foreach (var part in summary.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var colonIndex = part.IndexOf(':');
            if (colonIndex <= 0
                || !int.TryParse(part[..colonIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out var slotIndex))
            {
                continue;
            }

            var payload = part[(colonIndex + 1)..].Split('|', StringSplitOptions.None);
            if (payload.Length == 0 || string.IsNullOrWhiteSpace(payload[0]))
            {
                continue;
            }

            int? cost = payload.Length >= 3 && int.TryParse(payload[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedCost)
                ? parsedCost
                : null;
            cards.Add(new ObservedCombatHandCard(
                slotIndex,
                payload[0],
                payload.Length >= 2 ? payload[1] : null,
                cost));
        }

        return cards;
    }

    private static string? TryReadString(JsonElement? element, string propertyName)
    {
        return element.HasValue
               && element.Value.ValueKind == JsonValueKind.Object
               && element.Value.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static IReadOnlyList<string> TryReadStringArray(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return property
            .EnumerateArray()
            .Where(static item => item.ValueKind == JsonValueKind.String)
            .Select(static item => item.GetString())
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Cast<string>()
            .ToArray();
    }

    private static string? TryReadNestedString(JsonElement? element, string objectPropertyName, string stringPropertyName)
    {
        return element.HasValue
               && element.Value.ValueKind == JsonValueKind.Object
               && element.Value.TryGetProperty(objectPropertyName, out var objectProperty)
               && objectProperty.ValueKind == JsonValueKind.Object
               && objectProperty.TryGetProperty(stringPropertyName, out var stringProperty)
               && stringProperty.ValueKind == JsonValueKind.String
            ? stringProperty.GetString()
            : null;
    }

    private static bool? TryReadNestedBool(JsonElement? element, string objectPropertyName, string boolPropertyName)
    {
        if (!element.HasValue
            || element.Value.ValueKind != JsonValueKind.Object
            || !element.Value.TryGetProperty(objectPropertyName, out var objectProperty)
            || objectProperty.ValueKind != JsonValueKind.Object
            || !objectProperty.TryGetProperty(boolPropertyName, out var boolProperty))
        {
            return null;
        }

        return boolProperty.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(boolProperty.GetString(), out var parsed) => parsed,
            _ => null,
        };
    }

    private static bool? TryReadBool(JsonElement? element, string objectPropertyName, string boolPropertyName)
    {
        if (!element.HasValue
            || element.Value.ValueKind != JsonValueKind.Object
            || !element.Value.TryGetProperty(objectPropertyName, out var objectProperty)
            || objectProperty.ValueKind != JsonValueKind.Object
            || !objectProperty.TryGetProperty(boolPropertyName, out var boolProperty))
        {
            return null;
        }

        return boolProperty.ValueKind == JsonValueKind.True
            ? true
            : boolProperty.ValueKind == JsonValueKind.False
                ? false
                : null;
    }

    private static bool? TryReadBool(JsonElement? element, string propertyName)
    {
        if (!element.HasValue
            || element.Value.ValueKind != JsonValueKind.Object
            || !element.Value.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(property.GetString(), out var parsed) => parsed,
            _ => null,
        };
    }

    private static int? TryReadInt32(JsonElement? element, string objectPropertyName, string intPropertyName)
    {
        if (!element.HasValue
            || element.Value.ValueKind != JsonValueKind.Object
            || !element.Value.TryGetProperty(objectPropertyName, out var objectProperty)
            || objectProperty.ValueKind != JsonValueKind.Object
            || !objectProperty.TryGetProperty(intPropertyName, out var intProperty))
        {
            return null;
        }

        return intProperty.ValueKind switch
        {
            JsonValueKind.Number when intProperty.TryGetInt32(out var numericValue) => numericValue,
            JsonValueKind.String when int.TryParse(intProperty.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stringValue) => stringValue,
            _ => null,
        };
    }

    private static long? TryReadInt64(JsonElement? element, string propertyName)
    {
        if (!element.HasValue
            || element.Value.ValueKind != JsonValueKind.Object
            || !element.Value.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetInt64(out var numericValue) => numericValue,
            JsonValueKind.String when long.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stringValue) => stringValue,
            _ => null,
        };
    }

    private static DateTimeOffset? TryReadDateTimeOffset(JsonElement? element, string propertyName)
    {
        return element.HasValue
               && element.Value.ValueKind == JsonValueKind.Object
               && element.Value.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.String
               && DateTimeOffset.TryParse(property.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : null;
    }
}
