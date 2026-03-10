using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using Sts2AiCompanion.Host;
using Sts2ModKit.Core.Configuration;

namespace Sts2AiCompanion.Wpf;

public sealed class ShellViewModel : INotifyPropertyChanged, IAsyncDisposable
{
    private Dispatcher? _dispatcher;
    private CompanionHost? _host;
    private string _workspaceRoot = Directory.GetCurrentDirectory();

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusLine { get; private set; } = "Waiting for live export.";
    public string RunLine { get; private set; } = "run: none";
    public string ScreenLine { get; private set; } = "screen: unknown";
    public string UpdatedLine { get; private set; } = "updated: -";
    public string PlayerLine { get; private set; } = "No player state yet.";
    public string DeckLine { get; private set; } = "Deck information is not available yet.";
    public string RelicsPotionsLine { get; private set; } = "Relics and potions are not available yet.";
    public string AdviceHeadline { get; private set; } = "No advice yet";
    public string AdviceSummary { get; private set; } = "Launch the game or press Analyze Now after a live export appears.";
    public string RecommendedActionLine { get; private set; } = "Recommended action: -";
    public string RecommendedChoiceLine { get; private set; } = "Recommended choice: -";
    public string ConfidenceLine { get; private set; } = "confidence: -";
    public string AutoAdviceButtonText { get; private set; } = "Pause Auto Advice";
    public bool AutoAdviceEnabled { get; private set; } = true;

    public ObservableCollection<string> CurrentChoices { get; } = new();
    public ObservableCollection<string> RecentEvents { get; } = new();
    public ObservableCollection<string> KnowledgeEntries { get; } = new();
    public ObservableCollection<string> ReasoningBullets { get; } = new();
    public ObservableCollection<string> RiskNotes { get; } = new();
    public ObservableCollection<string> RecentChanges { get; } = new();

    public async Task InitializeAsync(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        _workspaceRoot = FindWorkspaceRoot(AppContext.BaseDirectory);
        var configPath = Path.Combine(_workspaceRoot, "config", "ai-companion.sample.json");
        var configuration = ConfigurationLoader.LoadFromFile(configPath).Configuration;
        _host = new CompanionHost(configuration, _workspaceRoot);
        _host.SnapshotChanged += HostOnSnapshotChanged;
        await _host.StartAsync().ConfigureAwait(false);
        Apply(_host.CurrentSnapshot);
    }

    public async Task AnalyzeNowAsync()
    {
        if (_host is not null)
        {
            await _host.RequestManualAdviceAsync().ConfigureAwait(false);
        }
    }

    public void ToggleAutoAdvice()
    {
        if (_host is null)
        {
            return;
        }

        AutoAdviceEnabled = !AutoAdviceEnabled;
        AutoAdviceButtonText = AutoAdviceEnabled ? "Pause Auto Advice" : "Resume Auto Advice";
        _host.SetAutoAdviceEnabled(AutoAdviceEnabled);
        Notify(nameof(AutoAdviceButtonText));
    }

    public void RefreshKnowledge()
    {
        if (_host is not null)
        {
            Apply(_host.CurrentSnapshot);
        }
    }

    public void OpenArtifacts()
    {
        var target = _host?.CurrentSnapshot.Paths.RunRoot
                     ?? _host?.CurrentSnapshot.Paths.CompanionRoot
                     ?? Path.Combine(_workspaceRoot, "artifacts");
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{target}\"",
            UseShellExecute = true,
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is not null)
        {
            _host.SnapshotChanged -= HostOnSnapshotChanged;
            await _host.DisposeAsync().ConfigureAwait(false);
        }
    }

    private void HostOnSnapshotChanged(object? sender, CompanionHostSnapshot snapshot)
    {
        if (_dispatcher is null)
        {
            Apply(snapshot);
            return;
        }

        _dispatcher.Invoke(() => Apply(snapshot));
    }

    private void Apply(CompanionHostSnapshot snapshot)
    {
        StatusLine = snapshot.Status.Message;
        RunLine = $"run: {snapshot.Status.RunId ?? "none"}";
        ScreenLine = $"screen: {snapshot.RunState?.Snapshot.CurrentScreen ?? "unknown"}";
        UpdatedLine = $"updated: {snapshot.Status.UpdatedAt:yyyy-MM-dd HH:mm:ss}";
        AutoAdviceEnabled = snapshot.Status.AutoAdviceEnabled;
        AutoAdviceButtonText = AutoAdviceEnabled ? "Pause Auto Advice" : "Resume Auto Advice";

        if (snapshot.RunState is not null)
        {
            var player = snapshot.RunState.Snapshot.Player;
            PlayerLine =
                $"name={player.Name ?? "unknown"} HP={player.CurrentHp?.ToString() ?? "?"}/{player.MaxHp?.ToString() ?? "?"} gold={player.Gold?.ToString() ?? "?"} energy={player.Energy?.ToString() ?? "?"}";
            DeckLine = snapshot.RunState.Snapshot.Deck.Count == 0
                ? "Deck information is not available yet."
                : string.Join(", ", snapshot.RunState.Snapshot.Deck.Take(12).Select(card => card.Name));
            RelicsPotionsLine =
                $"relics: {(snapshot.RunState.Snapshot.Relics.Count == 0 ? "none" : string.Join(", ", snapshot.RunState.Snapshot.Relics.Take(8)))} | potions: {(snapshot.RunState.Snapshot.Potions.Count == 0 ? "none" : string.Join(", ", snapshot.RunState.Snapshot.Potions.Take(5)))}";
        }
        else
        {
            PlayerLine = "No player state yet.";
            DeckLine = "Deck information is not available yet.";
            RelicsPotionsLine = "Relics and potions are not available yet.";
        }

        if (snapshot.LatestAdvice is not null)
        {
            AdviceHeadline = snapshot.LatestAdvice.Headline;
            AdviceSummary = snapshot.LatestAdvice.Summary;
            RecommendedActionLine = $"Recommended action: {snapshot.LatestAdvice.RecommendedAction}";
            RecommendedChoiceLine = $"Recommended choice: {snapshot.LatestAdvice.RecommendedChoiceLabel ?? "-"}";
            ConfidenceLine = $"confidence: {snapshot.LatestAdvice.Confidence?.ToString("0.00") ?? "unknown"}";
            ReplaceCollection(ReasoningBullets, snapshot.LatestAdvice.ReasoningBullets);
            ReplaceCollection(RiskNotes, snapshot.LatestAdvice.RiskNotes);
        }
        else
        {
            AdviceHeadline = "No advice yet";
            AdviceSummary = "Launch the game or press Analyze Now after a live export appears.";
            RecommendedActionLine = "Recommended action: -";
            RecommendedChoiceLine = "Recommended choice: -";
            ConfidenceLine = "confidence: -";
            ReplaceCollection(ReasoningBullets, Array.Empty<string>());
            ReplaceCollection(RiskNotes, Array.Empty<string>());
        }

        ReplaceCollection(
            CurrentChoices,
            snapshot.RunState?.Snapshot.CurrentChoices.Select(choice =>
                    $"[{choice.Kind}] {choice.Label} :: {choice.Description ?? choice.Value ?? "no details"}")
            ?? Array.Empty<string>());
        ReplaceCollection(
            RecentEvents,
            snapshot.RunState?.RecentEvents.Select(evt =>
                    $"{evt.Kind} @ {evt.Screen} ({evt.Act?.ToString() ?? "?"}-{evt.Floor?.ToString() ?? "?"})")
            ?? Array.Empty<string>());
        ReplaceCollection(
            KnowledgeEntries,
            snapshot.LatestKnowledgeSlice?.Entries.Select(entry => $"{entry.Name} [{entry.Source}]")
            ?? Array.Empty<string>());
        ReplaceCollection(
            RecentChanges,
            snapshot.RunState?.Snapshot.RecentChanges ?? Array.Empty<string>());

        NotifyAll();
    }

    private static void ReplaceCollection(ObservableCollection<string> target, IEnumerable<string> values)
    {
        target.Clear();
        foreach (var value in values.DefaultIfEmpty("none"))
        {
            target.Add(value);
        }
    }

    private static string FindWorkspaceRoot(string start)
    {
        var current = new DirectoryInfo(start);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "STS2_Mod_AI_Companion.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    private void NotifyAll()
    {
        Notify(nameof(StatusLine));
        Notify(nameof(RunLine));
        Notify(nameof(ScreenLine));
        Notify(nameof(UpdatedLine));
        Notify(nameof(PlayerLine));
        Notify(nameof(DeckLine));
        Notify(nameof(RelicsPotionsLine));
        Notify(nameof(AdviceHeadline));
        Notify(nameof(AdviceSummary));
        Notify(nameof(RecommendedActionLine));
        Notify(nameof(RecommendedChoiceLine));
        Notify(nameof(ConfidenceLine));
        Notify(nameof(AutoAdviceButtonText));
    }

    private void Notify([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
