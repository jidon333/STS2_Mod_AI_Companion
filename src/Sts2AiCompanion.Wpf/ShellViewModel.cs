using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using Sts2AiCompanion.Advisor;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Wpf;

public sealed class ShellViewModel : INotifyPropertyChanged, IAsyncDisposable
{
    private static readonly IReadOnlyDictionary<string, string?> ModelOptions = new Dictionary<string, string?>
    {
        ["疫꿸퀡??첎?] = null,
        ["GPT-5.4"] = "gpt-5.4",
        ["GPT-5.3-Codex"] = "gpt-5.3-codex",
        ["GPT-5.3-Codex-Spark"] = "gpt-5.3-codex-spark",
        ["GPT-5.2-Codex"] = "gpt-5.2-codex",
        ["GPT-5.2"] = "gpt-5.2",
        ["GPT-5.1-Codex-Max"] = "gpt-5.1-codex-max",
        ["GPT-5.1-Codex-Mini"] = "gpt-5.1-codex-mini",
    };

    private static readonly IReadOnlyDictionary<string, string?> ReasoningOptions = new Dictionary<string, string?>
    {
        ["Low"] = "low",
        ["Medium (default)"] = "medium",
        ["High"] = "high",
        ["Extra high"] = "xhigh",
    };

    private Dispatcher? _dispatcher;
    private AdvisorCoordinator? _host;
    private DispatcherTimer? _analysisTimer;
    private string _workspaceRoot = Directory.GetCurrentDirectory();
    private bool _analysisInProgress;
    private DateTimeOffset? _analysisStartedAt;
    private string? _analysisTriggerKind;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusLine { get; private set; } = "??쇰뻻揶??곕뗄????疫?餓λ쵐???덈뼄.";
    public string RunLine { get; private set; } = "?? ??곸벉";
    public string ScreenLine { get; private set; } = "?遺얇늺: ?類ㅼ뵥 餓?;
    public string UpdatedLine { get; private set; } = "??낅쑓??꾨뱜: -";
    public string AnalysisStatusText { get; private set; } = "?브쑴苑??怨밴묶: ??疫?餓?;
    public string PlayerText { get; private set; } = "???쟿??곷선 ?怨밴묶揶쎛 ?袁⑹춦 ??곷뮸??덈뼄.";
    public string DeckText { get; private set; } = "???類ｋ궖???袁⑹춦 ??? 筌륁궢六??щ빍??";
    public string RelicsPotionsText { get; private set; } = "?醫듢ゆ??????類ｋ궖揶쎛 ?袁⑹춦 ??곷뮸??덈뼄.";
    public string AdviceOverviewText { get; private set; } = "?袁⑹춦 鈺곌퀣堉????곷뮸??덈뼄.";
    public string AdviceDetailsText { get; private set; } = "域뱀눊援?? ?귐딅뮞???類ｋ궖揶쎛 ?袁⑹춦 ??곷뮸??덈뼄.";
    public string CurrentChoicesText { get; private set; } = "??곸벉";
    public string RecentEventsText { get; private set; } = "??곸벉";
    public string KnowledgeEntriesText { get; private set; } = "??곸벉";
    public string CollectorNotesText { get; private set; } = "??륁춿 ??筌욊쑬???類ｋ궖揶쎛 ??곷뮸??덈뼄.";
    public string ConfidenceLine { get; private set; } = "?醫듚?? -";
    public string AutoAdviceButtonText { get; private set; } = "?癒?짗 鈺곌퀣堉???깅뻻餓λ쵐?";
    public IReadOnlyList<string> AvailableModels => ModelOptions.Keys.ToArray();
    public IReadOnlyList<string> AvailableReasoningOptions => ReasoningOptions.Keys.ToArray();
    public string SelectedModelOption { get; private set; } = "疫꿸퀡??첎?;
    public string SelectedReasoningOption { get; private set; } = "Extra high";
    public bool AutoAdviceEnabled { get; private set; } = true;

    public async Task InitializeAsync(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        _workspaceRoot = FindWorkspaceRoot(AppContext.BaseDirectory);
        var configPath = Path.Combine(_workspaceRoot, "config", "ai-companion.sample.json");
        var configuration = ConfigurationLoader.LoadFromFile(configPath).Configuration;
        _host = new AdvisorCoordinator(configuration, _workspaceRoot);
        _host.SnapshotChanged += HostOnSnapshotChanged;

        _analysisTimer = new DispatcherTimer(DispatcherPriority.Background, dispatcher)
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        _analysisTimer.Tick += (_, _) => UpdateAnalysisStatusText();

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

    public async Task RetryLastAsync()
    {
        if (_host is not null)
        {
            await _host.RequestRetryLastAdviceAsync().ConfigureAwait(false);
        }
    }

    public void ToggleAutoAdvice()
    {
        if (_host is null)
        {
            return;
        }

        AutoAdviceEnabled = !AutoAdviceEnabled;
        AutoAdviceButtonText = AutoAdviceEnabled ? "?癒?짗 鈺곌퀣堉???깅뻻餓λ쵐?" : "?癒?짗 鈺곌퀣堉???쇰뻻 ?녹뮄由?;
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

    public void SetSelectedModelOption(string? option)
    {
        SelectedModelOption = string.IsNullOrWhiteSpace(option) ? "疫꿸퀡??첎? : option!;
        _host?.SetSelectedModel(ModelOptions.TryGetValue(SelectedModelOption, out var model) ? model : null);
        Notify(nameof(SelectedModelOption));
    }

    public void SetSelectedReasoningOption(string? option)
    {
        SelectedReasoningOption = string.IsNullOrWhiteSpace(option) ? "Medium (default)" : option!;
        _host?.SetSelectedReasoningEffort(ReasoningOptions.TryGetValue(SelectedReasoningOption, out var reasoning) ? reasoning : "medium");
        Notify(nameof(SelectedReasoningOption));
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
        if (_analysisTimer is not null)
        {
            _analysisTimer.Stop();
            _analysisTimer = null;
        }

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
        StatusLine = LocalizeStatusMessage(snapshot.Status.Message);
        RunLine = $"?? {snapshot.Status.RunId ?? "??곸벉"}";
        ScreenLine = $"?遺얇늺: {TranslateScreen(snapshot.RunState?.Snapshot.CurrentScreen)}";
        UpdatedLine = $"??낅쑓??꾨뱜: {snapshot.Status.UpdatedAt:yyyy-MM-dd HH:mm:ss}";
        AutoAdviceEnabled = snapshot.Status.AutoAdviceEnabled;
        AutoAdviceButtonText = AutoAdviceEnabled ? "?癒?짗 鈺곌퀣堉???깅뻻餓λ쵐?" : "?癒?짗 鈺곌퀣堉???쇰뻻 ?녹뮄由?;
        SelectedModelOption = ToModelDisplay(snapshot.Status.SelectedModel);
        SelectedReasoningOption = ToReasoningDisplay(snapshot.Status.SelectedReasoningEffort);

        _analysisInProgress = snapshot.Status.AnalysisInProgress;
        _analysisStartedAt = snapshot.Status.AnalysisStartedAt;
        _analysisTriggerKind = snapshot.Status.AnalysisTriggerKind;
        UpdateAnalysisStatusText(snapshot.Status.State, snapshot.Status.Message);

        if (snapshot.RunState is not null)
        {
            var player = snapshot.RunState.Snapshot.Player;
            PlayerText = JoinLines(
                new[]
                {
                    $"??已? {player.Name ?? "沃섎챸???}",
                    $"筌ｋ??? {player.CurrentHp?.ToString() ?? "?"}/{player.MaxHp?.ToString() ?? "?"}",
                    $"?ⓥ뫀諭? {player.Gold?.ToString() ?? "?"}",
                    $"?癒?섐筌왖: {player.Energy?.ToString() ?? "?"}",
                }.Concat(player.Resources.Select(pair => $"{pair.Key}: {pair.Value ?? "?"}")));

            DeckText = snapshot.RunState.Snapshot.Deck.Count == 0
                ? "???類ｋ궖???袁⑹춦 ??? 筌륁궢六??щ빍??"
                : JoinLines(snapshot.RunState.Snapshot.Deck.Take(24).Select(card =>
                {
                    var parts = new List<string> { card.Name };
                    if (card.Cost is not null)
                    {
                        parts.Add($"cost {card.Cost}");
                    }

                    if (!string.IsNullOrWhiteSpace(card.Type))
                    {
                        parts.Add(card.Type!);
                    }

                    if (card.Upgraded == true)
                    {
                        parts.Add("揶쏅벤??);
                    }

                    return "- " + string.Join(" / ", parts);
                }));

            RelicsPotionsText = JoinLines(new[]
            {
                "?醫듢?,
                snapshot.RunState.Snapshot.Relics.Count == 0
                    ? "- ??곸벉"
                    : JoinLines(snapshot.RunState.Snapshot.Relics.Take(12).Select(relic => $"- {relic}")),
                string.Empty,
                "????,
                snapshot.RunState.Snapshot.Potions.Count == 0
                    ? "- ??곸벉"
                    : JoinLines(snapshot.RunState.Snapshot.Potions.Take(8).Select(potion => $"- {potion}")),
            });
        }
        else
        {
            PlayerText = "???쟿??곷선 ?怨밴묶揶쎛 ?袁⑹춦 ??곷뮸??덈뼄.";
            DeckText = "???類ｋ궖???袁⑹춦 ??? 筌륁궢六??щ빍??";
            RelicsPotionsText = "?醫듢ゆ??????類ｋ궖揶쎛 ?袁⑹춦 ??곷뮸??덈뼄.";
        }

        if (snapshot.LatestAdvice is not null)
        {
            AdviceOverviewText = JoinLines(new[]
            {
                snapshot.LatestAdvice.Headline,
                string.Empty,
                snapshot.LatestAdvice.Summary,
                string.Empty,
                $"亦낅슣????곕짗: {snapshot.LatestAdvice.RecommendedAction}",
                $"亦낅슣???醫뤾문筌왖: {snapshot.LatestAdvice.RecommendedChoiceLabel ?? "-"}",
            });

            AdviceDetailsText = JoinLines(new[]
            {
                "域뱀눊援?,
                FormatBulletSection(snapshot.LatestAdvice.ReasoningBullets),
                string.Empty,
                "?귐딅뮞??,
                FormatBulletSection(snapshot.LatestAdvice.RiskNotes),
                string.Empty,
                "?봔鈺곌퉲釉??類ｋ궖",
                FormatBulletSection(snapshot.LatestAdvice.MissingInformation),
                string.Empty,
                "?癒?뼊 筌△뫀???遺우뵥",
                FormatBulletSection(snapshot.LatestAdvice.DecisionBlockers),
                string.Empty,
                "筌ㅼ뮄??癰궰??,
                FormatBulletSection(snapshot.RunState?.Snapshot.RecentChanges ?? Array.Empty<string>()),
            });
            ConfidenceLine = $"?醫듚?? {snapshot.LatestAdvice.Confidence?.ToString("0.00") ?? "沃섎챷湲?}";
        }
        else
        {
            AdviceOverviewText = JoinLines(new[]
            {
                "?袁⑹춦 鈺곌퀣堉????곷뮸??덈뼄.",
                string.Empty,
                "野껊슣?????쎈뻬??뉕탢??live export揶쎛 ?븐늿? ??'筌왖疫??브쑴苑??????쑎 雅뚯눘苑??",
            });
            AdviceDetailsText = "域뱀눊援?? ?귐딅뮞???類ｋ궖揶쎛 ?袁⑹춦 ??곷뮸??덈뼄.";
            ConfidenceLine = "?醫듚?? -";
        }

        CurrentChoicesText = JoinLines(
            (snapshot.RunState?.Snapshot.CurrentChoices ?? Array.Empty<LiveExportChoiceSummary>())
            .Select(choice => $"[{TranslateChoiceKind(choice.Kind)}] {choice.Label} :: {choice.Description ?? choice.Value ?? "?곕떽? ?類ｋ궖 ??곸벉"}")
            .DefaultIfEmpty("??곸벉"));

        RecentEventsText = JoinLines(
            (snapshot.RunState?.RecentEvents ?? Array.Empty<LiveExportEventEnvelope>())
            .Select(evt => $"{TranslateEventKind(evt.Kind)} @ {TranslateScreen(evt.Screen)} ({evt.Act?.ToString() ?? "?"}-{evt.Floor?.ToString() ?? "?"})")
            .DefaultIfEmpty("??곸벉"));

        KnowledgeEntriesText = JoinLines(
            (snapshot.LatestKnowledgeSlice?.Entries ?? Array.Empty<StaticKnowledgeEntry>())
            .Select(entry => $"{entry.Name} [?곗뮇荑? {TranslateSource(entry.Source)}]")
            .DefaultIfEmpty("??곸벉"));

        CollectorNotesText = snapshot.CollectorStatus switch
        {
            null => "??륁춿 ??筌욊쑬???類ｋ궖揶쎛 ??곷뮸??덈뼄.",
            { Enabled: false } => "??륁춿 ??筌뤴뫀諭뜹첎? ??쑵??源딆넅??뤿선 ??됰뮸??덈뼄.",
            var collector => JoinLines(new[]
            {
                $"collector mode: {(collector.Enabled ? "on" : "off")}",
                $"筌ㅼ뮄??semantic ?遺얇늺: {TranslateScreen(collector.LastSemanticScreen)}",
                $"?遺얇늺 episode: {collector.ActiveScreenEpisode ?? "??곸벉"}",
                $"?醫뤾문筌왖 ?곕뗄???怨밴묶: {collector.ChoiceExtractionStatus ?? "沃섎챷湲?}",
                $"筌띾뜆?筌?extractor 野껋럥以? {collector.LastAcceptedExtractorPath ?? "??곸벉"}",
                $"筌띾뜆?筌?degraded ??곸?: {collector.LastDegradedReason ?? "??곸벉"}",
                $"session id: {collector.SessionId ?? "??곸벉"}",
                $"筌왖??????揶쏆뮇?? {collector.KnowledgeEntriesUsedCount}",
                $"筌왖????????곸?: {(collector.KnowledgeReasons.Count > 0 ? string.Join(", ", collector.KnowledgeReasons.Take(4)) : "??곸벉")}",
                $"筌왖??筌〓챷?? {(collector.TopKnowledgeRefs.Count > 0 ? string.Join(", ", collector.TopKnowledgeRefs.Take(4)) : "??곸벉")}",
                $"?봔鈺곌퉲釉??類ｋ궖: {(collector.MissingInformation.Count > 0 ? string.Join(", ", collector.MissingInformation.Take(4)) : "??곸벉")}",
                $"?癒?뼊 筌△뫀???遺우뵥: {(collector.DecisionBlockers.Count > 0 ? string.Join(", ", collector.DecisionBlockers.Take(4)) : "??곸벉")}",
                string.Empty,
                collector.Notes,
            }),
        };

        NotifyAll();
    }

    private void UpdateAnalysisStatusText(string? state = null, string? message = null)
    {
        if (_analysisInProgress && _analysisStartedAt is not null)
        {
            _analysisTimer?.Start();
            var elapsed = DateTimeOffset.UtcNow - _analysisStartedAt.Value;
            var seconds = Math.Max(0, (int)elapsed.TotalSeconds);
            var prefix = string.Equals(state, "retrying", StringComparison.OrdinalIgnoreCase)
                ? "????袁⑹㉦"
                : "?브쑴苑띴빳?;
            AnalysisStatusText = $"{prefix}: {TranslateEventKind(_analysisTriggerKind)} ({seconds}??";
        }
        else
        {
            _analysisTimer?.Stop();
            AnalysisStatusText = state switch
            {
                "failed" => $"?브쑴苑???쎈솭: {message}",
                "canceled" => $"?브쑴苑??띯뫁?? {message}",
                "degraded" => $"?브쑴苑???쀫립: {message}",
                "retrying" => "?브쑴苑??怨밴묶: ???????疫?,
                _ => "?브쑴苑??怨밴묶: ??疫?餓?,
            };
        }

        Notify(nameof(AnalysisStatusText));
    }

    private static string JoinLines(IEnumerable<string> lines)
    {
        return string.Join(Environment.NewLine, lines.Where(line => line is not null));
    }

    private static string FormatBulletSection(IEnumerable<string> items)
    {
        return JoinLines(items.DefaultIfEmpty("??곸벉").Select(item => $"- {item}"));
    }

    private static string ToModelDisplay(string? model)
    {
        return ModelOptions.FirstOrDefault(pair => string.Equals(pair.Value, model, StringComparison.OrdinalIgnoreCase)).Key ?? "疫꿸퀡??첎?;
    }

    private static string ToReasoningDisplay(string? reasoning)
    {
        return ReasoningOptions.FirstOrDefault(pair => string.Equals(pair.Value, reasoning, StringComparison.OrdinalIgnoreCase)).Key ?? "Medium (default)";
    }

    private static string LocalizeStatusMessage(string? message)
    {
        return message switch
        {
            null or "" => "?怨밴묶 筌롫뗄?놅쭪?揶쎛 ??곷뮸??덈뼄.",
            "Waiting for live export." => "??쇰뻻揶??곕뗄???疫꿸퀡?롧뵳???餓λ쵐???덈뼄.",
            "state.latest.json is not available yet." => "?袁⑹춦 state.latest.json????밴쉐??? ??녿릭??щ빍??",
            "Monitoring live export updates." => "??쇰뻻揶??곕뗄??揶쏄퉮???揶쏅Ŋ??餓λ쵐???덈뼄.",
            "Automatic advice is enabled." => "?癒?짗 鈺곌퀣堉???녹뮇議???됰뮸??덈뼄.",
            "Automatic advice is paused." => "?癒?짗 鈺곌퀣堉????깅뻻餓λ쵐???뤿선 ??됰뮸??덈뼄.",
            _ when message.StartsWith("Advice generated for ", StringComparison.Ordinal) =>
                $"鈺곌퀣堉???밴쉐 ?袁⑥┷: {TranslateEventKind(message["Advice generated for ".Length..])}",
            _ when message.StartsWith("AI analyzing: ", StringComparison.Ordinal) =>
                $"?브쑴苑띴빳? {TranslateEventKind(message["AI analyzing: ".Length..])}",
            _ when message.StartsWith("Retrying AI advice: ", StringComparison.Ordinal) =>
                $"????袁⑹㉦: {TranslateEventKind(message["Retrying AI advice: ".Length..])}",
            _ when message.StartsWith("AI request canceled: ", StringComparison.Ordinal) =>
                $"?브쑴苑??띯뫁?? {TranslateEventKind(message["AI request canceled: ".Length..])}",
            _ when message.StartsWith("AI advice degraded: ", StringComparison.Ordinal) =>
                $"鈺곌퀣堉???쀫립: {message["AI advice degraded: ".Length..]}",
            _ when message.StartsWith("AI request failed: ", StringComparison.Ordinal) =>
                $"?브쑴苑???쎈솭: {message["AI request failed: ".Length..]}",
            _ => message,
        };
    }

    private static string TranslateScreen(string? screen)
    {
        return screen?.Trim().ToLowerInvariant() switch
        {
            null or "" => "?類ㅼ뵥 餓?,
            "unknown" => "沃섎챸???,
            "main-menu" => "筌롫뗄??筌롫뗀??,
            "combat" => "?袁る떮",
            "reward" or "rewards" => "癰귣똻湲?,
            "event" => "??源??,
            "shop" => "?怨몄젎",
            "rest" or "rest-site" or "campfire" => "??곷뻼",
            "map" => "筌?,
            "victory" => "?諛멤봺",
            "death" => "??ㅺ컳",
            "character-select" => "筌?Ŧ????醫뤾문",
            "card-choice" => "燁삳?諭??醫뤾문",
            "upgrade" => "揶쏅벤??,
            "transform" => "癰궰??,
            _ => screen ?? "?類ㅼ뵥 餓?,
        };
    }

    private static string TranslateEventKind(string? kind)
    {
        return kind?.Trim().ToLowerInvariant() switch
        {
            null or "" => "沃섎챷湲?,
            "runtime-poll" => "雅뚯눊由??怨밴묶 ??륁춿",
            "screen-changed" => "?遺얇늺 ?袁れ넎",
            "choice-list-presented" => "?醫뤾문筌왖 ??뽯뻻",
            "choice-selected" => "?醫뤾문筌왖 ?醫뤾문",
            "main-menu" => "Main menu",
            "singleplayer-button-pressed" => "Singleplayer pressed",
            "singleplayer-submenu" => "Singleplayer submenu",
            "open-character-select" => "Open character select",
            "character-select" => "Character select",
            "character-selected" => "Character selected",
            "combat-started" => "?袁る떮 ??뽰삂",
            "combat-ended" => "?袁る떮 ?ル굝利?,
            "turn-started" => "????뽰삂",
            "turn-ended" => "???ル굝利?,
            "map" => "Map opened",
            "map-point-selected" => "Map point selected",
            "map-node-entered" => "筌??紐껊굡 筌욊쑴??,
            "reward-opened" or "reward-screen-opened" => "癰귣똻湲??遺얇늺 ????,
            "event-opened" or "event-screen-opened" => "??源???遺얇늺 ????,
            "shop-opened" => "?怨몄젎 ?遺얇늺 ????,
            "rest-opened" => "??곷뻼 ?遺얇늺 ????,
            "run-started" => "????뽰삂",
            "run-loaded" => "???븍뜄???븍┛",
            "run-ended" => "???ル굝利?,
            "app-started" => "????뽰삂",
            "app-stopped" => "???ル굝利?,
            "manual" => "??롫짗 ?브쑴苑?,
            "retry-last" => "筌띾뜆?筌??遺욧퍕 ?????,
            _ => kind ?? "沃섎챷湲?,
        };
    }

    private static string TranslateChoiceKind(string? kind)
    {
        return kind?.Trim().ToLowerInvariant() switch
        {
            null or "" => "?醫뤾문筌왖",
            "choice" => "?醫뤾문筌왖",
            "card" => "燁삳?諭?,
            "reward" => "癰귣똻湲?,
            "event" => "??源??,
            "shop" => "?怨몄젎",
            "rest" => "??곷뻼",
            "button" => "甕곌쑵??,
            "relic" => "?醫듢?,
            "potion" => "????,
            _ => kind ?? "?醫뤾문筌왖",
        };
    }

    private static string TranslateSource(string? source)
    {
        return source?.Trim().ToLowerInvariant() switch
        {
            null or "" => "沃섎챷湲?,
            "localization-scan" => "嚥≪뮇類??깆뵠??뽰뵠????쇳떔",
            "strict-domain-scan" => "?袁㏐봄 ?袁⑥컭????쇳떔",
            "assembly-scan" => "??곷띺뇡遺얄봺 ??쇳떔",
            "pck-inventory" => "PCK ?紐껉뭣?醫듼봺",
            "observed-merge" => "?온筌?癰귣쵑鍮",
            "release-scan" => "?깅??곻쭩???쇳떔",
            _ => source ?? "沃섎챷湲?,
        };
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
        Notify(nameof(AnalysisStatusText));
        Notify(nameof(PlayerText));
        Notify(nameof(DeckText));
        Notify(nameof(RelicsPotionsText));
        Notify(nameof(AdviceOverviewText));
        Notify(nameof(AdviceDetailsText));
        Notify(nameof(CurrentChoicesText));
        Notify(nameof(RecentEventsText));
        Notify(nameof(KnowledgeEntriesText));
        Notify(nameof(CollectorNotesText));
        Notify(nameof(ConfidenceLine));
        Notify(nameof(AutoAdviceButtonText));
        Notify(nameof(SelectedModelOption));
        Notify(nameof(SelectedReasoningOption));
    }

    private void Notify([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}