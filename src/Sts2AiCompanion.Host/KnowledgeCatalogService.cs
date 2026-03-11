using System.Text.Json;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Knowledge;

namespace Sts2AiCompanion.Host;

public sealed class KnowledgeCatalogService
{
    private readonly string _knowledgeRoot;
    private StaticKnowledgeCatalog _catalog = StaticKnowledgeCatalog.CreateEmpty();
    private DateTimeOffset? _lastLoadedAt;

    public KnowledgeCatalogService(ScaffoldConfiguration configuration, string workspaceRoot)
    {
        _knowledgeRoot = CompanionPathResolver.ResolveKnowledgeRoot(configuration, workspaceRoot);
    }

    public StaticKnowledgeCatalog CurrentCatalog => _catalog;

    public StaticKnowledgeCatalog ReloadIfChanged()
    {
        var path = ResolveCatalogPath();
        if (!File.Exists(path))
        {
            _catalog = StaticKnowledgeCatalog.CreateEmpty();
            _lastLoadedAt = null;
            return _catalog;
        }

        var lastWriteTime = File.GetLastWriteTimeUtc(path);
        if (_lastLoadedAt is not null && lastWriteTime <= _lastLoadedAt.Value.UtcDateTime)
        {
            return _catalog;
        }

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        _catalog = JsonSerializer.Deserialize<StaticKnowledgeCatalog>(stream, ConfigurationLoader.JsonOptions)
            ?? StaticKnowledgeCatalog.CreateEmpty();
        _lastLoadedAt = new DateTimeOffset(lastWriteTime, TimeSpan.Zero);
        return _catalog;
    }

    public KnowledgeSlice BuildSlice(CompanionRunState runState, int maxEntries, int maxBytes)
    {
        ReloadIfChanged();

        var selected = new Dictionary<string, StaticKnowledgeEntry>(StringComparer.OrdinalIgnoreCase);
        var reasons = new List<string>();

        AddScreenEntries(runState.Snapshot.CurrentScreen, selected, reasons);
        foreach (var choice in runState.Snapshot.CurrentChoices)
        {
            AddMatches(choice.Label, selected, reasons, "choice-label");
            AddMatches(choice.Value, selected, reasons, "choice-id");
        }

        foreach (var card in runState.Snapshot.Deck)
        {
            AddMatches(card.Id, selected, reasons, "deck-card-id");
            AddMatches(card.Name, selected, reasons, "deck-card-name");
        }

        foreach (var relic in runState.Snapshot.Relics)
        {
            AddMatches(relic, selected, reasons, "relic");
        }

        foreach (var potion in runState.Snapshot.Potions)
        {
            AddMatches(potion, selected, reasons, "potion");
        }

        foreach (var recentEvent in runState.RecentEvents.TakeLast(6))
        {
            AddMatches(recentEvent.Kind, selected, reasons, "recent-event-kind");
            foreach (var payloadValue in recentEvent.Payload.Values.Select(value => value?.ToString()).Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                AddMatches(payloadValue, selected, reasons, "recent-event-payload");
            }
        }

        var ordered = selected.Values
            .OrderByDescending(entry => entry.Observed)
            .ThenByDescending(entry => entry.Options.Count)
            .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var bounded = new List<StaticKnowledgeEntry>();
        var byteCount = 0;
        foreach (var entry in ordered)
        {
            var delta = entry.Name.Length + (entry.RawText?.Length ?? 0) + entry.Options.Sum(option => option.Label.Length + (option.Description?.Length ?? 0));
            if (bounded.Count >= maxEntries || byteCount + delta > maxBytes)
            {
                break;
            }

            bounded.Add(entry);
            byteCount += delta;
        }

        return new KnowledgeSlice(
            bounded,
            byteCount,
            reasons.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(reason => reason, StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private void AddScreenEntries(string screen, IDictionary<string, StaticKnowledgeEntry> selected, ICollection<string> reasons)
    {
        var sectionEntries = screen switch
        {
            "event" => _catalog.Events,
            "shop" => _catalog.Shops,
            "rewards" => _catalog.Rewards,
            _ => Array.Empty<StaticKnowledgeEntry>(),
        };

        foreach (var entry in sectionEntries.Take(6))
        {
            selected[entry.Id] = entry;
        }

        if (sectionEntries.Count > 0)
        {
            reasons.Add($"screen:{screen}");
        }
    }

    private void AddMatches(string? seed, IDictionary<string, StaticKnowledgeEntry> selected, ICollection<string> reasons, string reason)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            return;
        }

        foreach (var entry in EnumerateAllEntries())
        {
            if (IsMatch(seed!, entry))
            {
                selected[entry.Id] = entry;
                reasons.Add(reason);
            }
        }
    }

    private IEnumerable<StaticKnowledgeEntry> EnumerateAllEntries()
    {
        return _catalog.Cards
            .Concat(_catalog.Relics)
            .Concat(_catalog.Potions)
            .Concat(_catalog.Events)
            .Concat(_catalog.Shops)
            .Concat(_catalog.Rewards)
            .Concat(_catalog.Keywords);
    }

    private static bool IsMatch(string seed, StaticKnowledgeEntry entry)
    {
        if (string.Equals(entry.Id, Normalize(seed), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(entry.Name, seed, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return entry.Name.Contains(seed, StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(entry.RawText) && entry.RawText.Contains(seed, StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string seed)
    {
        return new string(seed
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray())
            .Trim('-');
    }

    private string ResolveCatalogPath()
    {
        var assistantPath = Path.Combine(_knowledgeRoot, "catalog.assistant.json");
        if (File.Exists(assistantPath))
        {
            return assistantPath;
        }

        return Path.Combine(_knowledgeRoot, "catalog.latest.json");
    }
}
