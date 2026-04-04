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
using Sts2AiCompanion.SceneProvenance;
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
        var provenanceInput = ScreenProvenanceResolver.CreateFromObserverDocuments(stateDocument, inventoryDocument);
        var provenance = ScreenProvenanceResolver.Resolve(provenanceInput);
        var eventLines = includeEventTail ? TryReadTailCached(_liveLayout.EventsPath, 10) : null;
        var stateCapturedAt = TryReadDateTimeOffset(stateDocument?.RootElement, "capturedAt");
        var inventoryCapturedAt = TryReadDateTimeOffset(inventoryDocument?.RootElement, "capturedAt");

        var inventoryCompatibilityCurrentScreen = provenanceInput.InventoryCompatibilityCurrentScreen;
        var compatibilityCurrentScreen = TryReadNestedString(stateDocument?.RootElement, "meta", "compatibilityCurrentScreen")
                                         ?? TryReadNestedString(stateDocument?.RootElement, "meta", "compatLogicalScreen")
                                         ?? inventoryCompatibilityCurrentScreen;
        var compatibilityLogicalScreen = TryReadNestedString(stateDocument?.RootElement, "meta", "compatLogicalScreen")
                                         ?? TryReadString(inventoryDocument?.RootElement, "compatibilityLogicalScreen")
                                         ?? TryReadString(inventoryDocument?.RootElement, "compatibilitySceneType");
        var rawCurrentScreen = provenance.RawCurrentScreen;
        var rawObservedScreen = provenance.RawObservedScreen;
        var publishedCurrentScreen = provenance.PublishedCurrentScreen;
        var currentScreen = provenance.ResolvedCurrentScreen;
        var publishedVisibleScreen = provenance.PublishedVisibleScreen;
        var snapshotVersion = TryReadInt64(stateDocument?.RootElement, "version");
        var compatibilityVisibleScreen = TryReadNestedString(stateDocument?.RootElement, "meta", "compatibilityVisibleScreen")
                                         ?? TryReadString(inventoryDocument?.RootElement, "compatibilityVisibleScreen")
                                         ?? TryReadNestedString(stateDocument?.RootElement, "meta", "compatVisibleScreen")
                                         ?? TryReadString(inventoryDocument?.RootElement, "compatibilityVisibleScene");
        var visibleScreen = provenance.ResolvedVisibleScreen;
        var inCombat = TryReadBool(stateDocument?.RootElement, "encounter", "inCombat");
        var capturedAt = stateCapturedAt
                         ?? inventoryCapturedAt;
        var inventoryId = inventoryDocument is null
            ? null
            : TryReadString(inventoryDocument.RootElement, "inventoryId");
        var inventoryPublishedSceneReady = TryReadBool(inventoryDocument?.RootElement, "publishedSceneReady");
        var compatibilitySceneReady = TryReadNestedBool(stateDocument?.RootElement, "meta", "compatSceneReady")
                                      ?? TryReadBool(inventoryDocument?.RootElement, "compatibilitySceneReady");
        var publishedSceneReady = provenance.PublishedSceneReady
                                  ?? inventoryPublishedSceneReady;
        var sceneReady = provenance.ResolvedSceneReady;
        var inventoryPublishedSceneAuthority = TryReadString(inventoryDocument?.RootElement, "publishedSceneAuthority");
        var compatibilitySceneAuthority = TryReadNestedString(stateDocument?.RootElement, "meta", "compatSceneAuthority")
                                          ?? TryReadString(inventoryDocument?.RootElement, "compatibilitySceneAuthority");
        var publishedSceneAuthority = provenance.PublishedSceneAuthority
                                      ?? inventoryPublishedSceneAuthority;
        var sceneAuthority = provenance.ResolvedSceneAuthority;
        var inventoryPublishedSceneStability = TryReadString(inventoryDocument?.RootElement, "publishedSceneStability");
        var compatibilitySceneStability = TryReadNestedString(stateDocument?.RootElement, "meta", "compatSceneStability")
                                          ?? TryReadString(inventoryDocument?.RootElement, "compatibilitySceneStability");
        var publishedSceneStability = provenance.PublishedSceneStability
                                      ?? inventoryPublishedSceneStability;
        var sceneStability = provenance.ResolvedSceneStability;
        var sceneEpisodeId = TryReadString(inventoryDocument?.RootElement, "sceneEpisodeId")
                             ?? TryReadNestedString(stateDocument?.RootElement, "meta", "screen-episode");
        var encounterKind = TryReadNestedString(stateDocument?.RootElement, "encounter", "kind");
        var choiceExtractorPath = TryReadNestedString(stateDocument?.RootElement, "meta", "choiceExtractorPath");
        var playerCurrentHp = TryReadInt32(stateDocument?.RootElement, "player", "currentHp");
        var playerMaxHp = TryReadInt32(stateDocument?.RootElement, "player", "maxHp");
        var playerEnergy = TryReadInt32(stateDocument?.RootElement, "player", "energy");
        var combatHand = ParseCombatHandSummary(TryReadNestedString(stateDocument?.RootElement, "meta", "combatHandSummary"));
        var currentChoices = ReadChoiceLabels(stateDocument);
        var choices = ReadChoices(stateDocument);
        var actionNodes = ReconcileActionNodes(
            ReadActionNodes(inventoryDocument),
            choices,
            stateCapturedAt,
            inventoryCapturedAt);
        var meta = ReadMetaDictionary(stateDocument);

        return new ObserverState(
            new ObserverSummary(
                currentScreen,
                visibleScreen,
                inCombat,
                capturedAt,
                inventoryId,
                sceneReady,
                sceneAuthority,
                sceneStability,
                sceneEpisodeId,
                encounterKind,
                choiceExtractorPath,
                playerCurrentHp,
                playerMaxHp,
                playerEnergy,
                currentChoices,
                eventLines ?? Array.Empty<string>(),
                actionNodes,
                choices,
                combatHand)
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
                CompatibilityVisibleScreen = compatibilityVisibleScreen,
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

    internal static ObserverState CreateReplayObserverState(JsonDocument stateDocument, string[]? eventLines = null)
    {
        var stateCapturedAt = TryReadDateTimeOffset(stateDocument.RootElement, "capturedAt");
        var provenanceInput = ScreenProvenanceResolver.CreateFromObserverDocuments(stateDocument, null);
        var provenance = ScreenProvenanceResolver.Resolve(provenanceInput);
        var inventoryDocument = (JsonDocument?)null;
        var inventoryCompatibilityCurrentScreen = provenanceInput.InventoryCompatibilityCurrentScreen;
        var compatibilityCurrentScreen = TryReadNestedString(stateDocument.RootElement, "meta", "compatibilityCurrentScreen")
                                         ?? TryReadNestedString(stateDocument.RootElement, "meta", "compatLogicalScreen")
                                         ?? inventoryCompatibilityCurrentScreen;
        var rawCurrentScreen = provenance.RawCurrentScreen;
        var rawObservedScreen = provenance.RawObservedScreen;
        var publishedCurrentScreen = provenance.PublishedCurrentScreen;
        var currentScreen = provenance.ResolvedCurrentScreen;
        var publishedVisibleScreen = provenance.PublishedVisibleScreen;
        var snapshotVersion = TryReadInt64(stateDocument.RootElement, "version");
        var compatibilityVisibleScreen = TryReadNestedString(stateDocument.RootElement, "meta", "compatibilityVisibleScreen")
                                         ?? TryReadNestedString(stateDocument.RootElement, "meta", "compatVisibleScreen");
        var visibleScreen = provenance.ResolvedVisibleScreen;
        var inCombat = TryReadBool(stateDocument.RootElement, "encounter", "inCombat");
        var capturedAt = stateCapturedAt;
        var inventoryId = (string?)null;
        var compatibilitySceneReady = TryReadNestedBool(stateDocument.RootElement, "meta", "compatSceneReady");
        var publishedSceneReady = provenance.PublishedSceneReady;
        var sceneReady = provenance.ResolvedSceneReady;
        var compatibilitySceneAuthority = TryReadNestedString(stateDocument.RootElement, "meta", "compatSceneAuthority");
        var publishedSceneAuthority = provenance.PublishedSceneAuthority;
        var sceneAuthority = provenance.ResolvedSceneAuthority;
        var compatibilitySceneStability = TryReadNestedString(stateDocument.RootElement, "meta", "compatSceneStability");
        var publishedSceneStability = provenance.PublishedSceneStability;
        var sceneStability = provenance.ResolvedSceneStability;
        var sceneEpisodeId = TryReadNestedString(stateDocument.RootElement, "meta", "screen-episode");
        var encounterKind = TryReadNestedString(stateDocument.RootElement, "encounter", "kind");
        var choiceExtractorPath = TryReadNestedString(stateDocument.RootElement, "meta", "choiceExtractorPath");
        var playerCurrentHp = TryReadInt32(stateDocument.RootElement, "player", "currentHp");
        var playerMaxHp = TryReadInt32(stateDocument.RootElement, "player", "maxHp");
        var playerEnergy = TryReadInt32(stateDocument.RootElement, "player", "energy");
        var combatHand = ParseCombatHandSummary(TryReadNestedString(stateDocument.RootElement, "meta", "combatHandSummary"));
        var currentChoices = ReadChoiceLabels(stateDocument);
        var choices = ReadChoices(stateDocument);
        var actionNodes = BuildActionNodesFromChoices(choices);
        var meta = ReadMetaDictionary(stateDocument);

        return new ObserverState(
            new ObserverSummary(
                currentScreen,
                visibleScreen,
                inCombat,
                capturedAt,
                inventoryId,
                sceneReady,
                sceneAuthority,
                sceneStability,
                sceneEpisodeId,
                encounterKind,
                choiceExtractorPath,
                playerCurrentHp,
                playerMaxHp,
                playerEnergy,
                currentChoices,
                eventLines ?? Array.Empty<string>(),
                actionNodes,
                choices,
                combatHand)
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
                CompatibilityVisibleScreen = compatibilityVisibleScreen,
                CompatibilitySceneReady = compatibilitySceneReady,
                CompatibilitySceneAuthority = compatibilitySceneAuthority,
                CompatibilitySceneStability = compatibilitySceneStability,
                Meta = meta,
            },
            stateDocument,
            null,
            eventLines);
    }

    private static IReadOnlyList<ObserverActionNode> ReconcileActionNodes(
        IReadOnlyList<ObserverActionNode> inventoryNodes,
        IReadOnlyList<ObserverChoice> stateChoices,
        DateTimeOffset? stateCapturedAt,
        DateTimeOffset? inventoryCapturedAt)
    {
        var stateChoiceNodes = BuildActionNodesFromChoices(stateChoices);
        if (stateChoiceNodes.Count == 0)
        {
            return inventoryNodes;
        }

        if (inventoryNodes.Count == 0)
        {
            return stateChoiceNodes;
        }

        if (inventoryCapturedAt is not null
            && stateCapturedAt is not null
            && inventoryCapturedAt < stateCapturedAt)
        {
            return stateChoiceNodes;
        }

        return HasInventoryNodeStateMismatch(inventoryNodes, stateChoiceNodes)
            ? stateChoiceNodes
            : inventoryNodes;
    }

    private static IReadOnlyList<ObserverActionNode> BuildActionNodesFromChoices(IReadOnlyList<ObserverChoice> choices)
    {
        var nodes = new List<ObserverActionNode>();
        foreach (var choice in choices)
        {
            if (string.IsNullOrWhiteSpace(choice.Label))
            {
                continue;
            }

            var nodeId = choice.NodeId ?? choice.Value;
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                continue;
            }

            var kind = string.IsNullOrWhiteSpace(choice.Kind)
                ? "choice"
                : choice.Kind;
            var semanticHints = choice.SemanticHints.Count == 0
                ? new[] { $"node-id:{nodeId}" }
                : choice.SemanticHints;

            nodes.Add(new ObserverActionNode(
                nodeId,
                kind,
                choice.Label,
                choice.ScreenBounds,
                choice.Enabled != false && !string.IsNullOrWhiteSpace(choice.ScreenBounds))
            {
                TypeName = choice.BindingKind ?? choice.Kind,
                SemanticHints = semanticHints,
            });
        }

        return nodes;
    }

    private static bool HasInventoryNodeStateMismatch(
        IReadOnlyList<ObserverActionNode> inventoryNodes,
        IReadOnlyList<ObserverActionNode> stateChoiceNodes)
    {
        return inventoryNodes.Any(inventoryNode => !stateChoiceNodes.Any(stateNode => MatchesActionSurface(inventoryNode, stateNode)));
    }

    private static bool MatchesActionSurface(ObserverActionNode left, ObserverActionNode right)
    {
        if (!string.IsNullOrWhiteSpace(left.NodeId)
            && !string.IsNullOrWhiteSpace(right.NodeId))
        {
            return string.Equals(left.NodeId, right.NodeId, StringComparison.OrdinalIgnoreCase);
        }

        return string.Equals(left.Label, right.Label, StringComparison.OrdinalIgnoreCase)
               && string.Equals(left.Kind, right.Kind, StringComparison.OrdinalIgnoreCase)
               && string.Equals(left.ScreenBounds, right.ScreenBounds, StringComparison.OrdinalIgnoreCase);
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
