using System.Text;

namespace Sts2ModKit.Core.LiveExport;

public static class LiveExportSummaryFormatter
{
    public static string Format(LiveExportSnapshot snapshot)
    {
        var builder = new StringBuilder();
        AppendSection(builder, "run status", new[]
        {
            $"run_id: {snapshot.RunId}",
            $"status: {snapshot.RunStatus}",
            $"version: {snapshot.Version}",
            $"captured_at: {snapshot.CapturedAt:O}",
            $"act: {FormatNullable(snapshot.Act)}",
            $"floor: {FormatNullable(snapshot.Floor)}",
        });

        AppendSection(builder, "current screen / encounter", new[]
        {
            $"screen: {snapshot.CurrentScreen}",
            $"encounter: {snapshot.Encounter?.Name ?? "unknown"}",
            $"encounter_kind: {snapshot.Encounter?.Kind ?? "unknown"}",
            $"combat_active: {FormatNullable(snapshot.Encounter?.InCombat)}",
            $"turn: {FormatNullable(snapshot.Encounter?.Turn)}",
        });

        AppendSection(builder, "player summary", new[]
        {
            $"name: {snapshot.Player.Name ?? "unknown"}",
            $"hp: {FormatPair(snapshot.Player.CurrentHp, snapshot.Player.MaxHp)}",
            $"gold: {FormatNullable(snapshot.Player.Gold)}",
            $"energy: {FormatNullable(snapshot.Player.Energy)}",
        }.Concat(snapshot.Player.Resources
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .Select(entry => $"{entry.Key}: {entry.Value ?? "unknown"}")));

        AppendSection(builder, "deck summary", snapshot.Deck.Count == 0
            ? new[] { "deck: none" }
            : snapshot.Deck.Select(card =>
            {
                var upgraded = card.Upgraded == true ? " upgraded" : string.Empty;
                var cost = card.Cost is null ? "?" : card.Cost.Value.ToString();
                return $"- {card.Name} [{card.Type ?? "unknown"} cost={cost}{upgraded}]";
            }));

        AppendSection(builder, "relics / potions",
            snapshot.Relics.Select(relic => $"- relic: {relic}")
                .Concat(snapshot.Potions.Select(potion => $"- potion: {potion}"))
                .DefaultIfEmpty("none"));

        AppendSection(builder, "current choices",
            snapshot.CurrentChoices.Count == 0
                ? new[] { "choices: none" }
                : snapshot.CurrentChoices.Select(choice => $"- [{choice.Kind}] {choice.Label} :: {choice.Description ?? choice.Value ?? "no details"}"));

        AppendSection(builder, "recent changes",
            snapshot.RecentChanges.DefaultIfEmpty("none"));

        AppendSection(builder, "extraction warnings",
            snapshot.Warnings.DefaultIfEmpty("none"));

        return builder.ToString().TrimEnd();
    }

    public static LiveExportTriggerDecision EvaluateCodexTrigger(
        LiveExportObservation observation,
        LiveExportTriggerWindow window)
    {
        var normalizedKind = observation.TriggerKind.Trim().ToLowerInvariant();
        var immediateKinds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "choice-list-presented",
            "reward-screen-opened",
            "event-screen-opened",
        };

        var periodicKinds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "turn-started",
            "combat-started",
            "map",
            "map-node-entered",
        };

        if (immediateKinds.Contains(normalizedKind))
        {
            return new LiveExportTriggerDecision(true, normalizedKind, true);
        }

        if (!periodicKinds.Contains(normalizedKind))
        {
            return new LiveExportTriggerDecision(false, "not-a-trigger", false);
        }

        if (window.LastTriggerAt is null || observation.ObservedAt - window.LastTriggerAt >= window.MinInterval)
        {
            return new LiveExportTriggerDecision(true, normalizedKind, false);
        }

        return new LiveExportTriggerDecision(false, "min-interval", false);
    }

    private static void AppendSection(StringBuilder builder, string title, IEnumerable<string> lines)
    {
        builder.AppendLine(title);
        foreach (var line in lines)
        {
            builder.AppendLine(line);
        }

        builder.AppendLine();
    }

    private static string FormatNullable<T>(T? value)
        where T : struct
    {
        return value?.ToString() ?? "unknown";
    }

    private static string FormatPair(int? left, int? right)
    {
        return $"{FormatNullable(left)}/{FormatNullable(right)}";
    }
}
