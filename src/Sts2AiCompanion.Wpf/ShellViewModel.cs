using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using Sts2AiCompanion.Host;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Knowledge;
using Sts2ModKit.Core.LiveExport;

namespace Sts2AiCompanion.Wpf;

public sealed class ShellViewModel : INotifyPropertyChanged, IAsyncDisposable
{
    private static readonly IReadOnlyDictionary<string, string?> ModelOptions = new Dictionary<string, string?>
    {
        ["기본값"] = null,
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
    private CompanionHost? _host;
    private DispatcherTimer? _analysisTimer;
    private string _workspaceRoot = Directory.GetCurrentDirectory();
    private bool _analysisInProgress;
    private DateTimeOffset? _analysisStartedAt;
    private string? _analysisTriggerKind;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusLine { get; private set; } = "실시간 추출 대기 중입니다.";
    public string RunLine { get; private set; } = "런: 없음";
    public string ScreenLine { get; private set; } = "화면: 확인 중";
    public string UpdatedLine { get; private set; } = "업데이트: -";
    public string AnalysisStatusText { get; private set; } = "분석 상태: 대기 중";
    public string PlayerText { get; private set; } = "플레이어 상태가 아직 없습니다.";
    public string DeckText { get; private set; } = "덱 정보를 아직 읽지 못했습니다.";
    public string RelicsPotionsText { get; private set; } = "유물과 포션 정보가 아직 없습니다.";
    public string AdviceOverviewText { get; private set; } = "아직 조언이 없습니다.";
    public string AdviceDetailsText { get; private set; } = "근거와 리스크 정보가 아직 없습니다.";
    public string CurrentChoicesText { get; private set; } = "없음";
    public string RecentEventsText { get; private set; } = "없음";
    public string KnowledgeEntriesText { get; private set; } = "없음";
    public string CollectorNotesText { get; private set; } = "수집 런 진단 정보가 없습니다.";
    public string ConfidenceLine { get; private set; } = "신뢰도: -";
    public string AutoAdviceButtonText { get; private set; } = "자동 조언 일시중지";
    public IReadOnlyList<string> AvailableModels => ModelOptions.Keys.ToArray();
    public IReadOnlyList<string> AvailableReasoningOptions => ReasoningOptions.Keys.ToArray();
    public string SelectedModelOption { get; private set; } = "기본값";
    public string SelectedReasoningOption { get; private set; } = "Extra high";
    public bool AutoAdviceEnabled { get; private set; } = true;

    public async Task InitializeAsync(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        _workspaceRoot = FindWorkspaceRoot(AppContext.BaseDirectory);
        var configPath = Path.Combine(_workspaceRoot, "config", "ai-companion.sample.json");
        var configuration = ConfigurationLoader.LoadFromFile(configPath).Configuration;
        _host = new CompanionHost(configuration, _workspaceRoot);
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
        AutoAdviceButtonText = AutoAdviceEnabled ? "자동 조언 일시중지" : "자동 조언 다시 켜기";
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
        SelectedModelOption = string.IsNullOrWhiteSpace(option) ? "기본값" : option!;
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
        RunLine = $"런: {snapshot.Status.RunId ?? "없음"}";
        var normalizedScreen = snapshot.RunState?.NormalizedState.Scene.SemanticSceneType;
        ScreenLine = $"화면: {TranslateScreen(string.IsNullOrWhiteSpace(normalizedScreen) ? snapshot.RunState?.Snapshot.CurrentScreen : normalizedScreen)}";
        UpdatedLine = $"업데이트: {snapshot.Status.UpdatedAt:yyyy-MM-dd HH:mm:ss}";
        AutoAdviceEnabled = snapshot.Status.AutoAdviceEnabled;
        AutoAdviceButtonText = AutoAdviceEnabled ? "자동 조언 일시중지" : "자동 조언 다시 켜기";
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
                    $"이름: {player.Name ?? "미확인"}",
                    $"체력: {player.CurrentHp?.ToString() ?? "?"}/{player.MaxHp?.ToString() ?? "?"}",
                    $"골드: {player.Gold?.ToString() ?? "?"}",
                    $"에너지: {player.Energy?.ToString() ?? "?"}",
                }.Concat(player.Resources.Select(pair => $"{pair.Key}: {pair.Value ?? "?"}")));

            DeckText = snapshot.RunState.Snapshot.Deck.Count == 0
                ? "덱 정보를 아직 읽지 못했습니다."
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
                        parts.Add("강화");
                    }

                    return "- " + string.Join(" / ", parts);
                }));

            RelicsPotionsText = JoinLines(new[]
            {
                "유물",
                snapshot.RunState.Snapshot.Relics.Count == 0
                    ? "- 없음"
                    : JoinLines(snapshot.RunState.Snapshot.Relics.Take(12).Select(relic => $"- {relic}")),
                string.Empty,
                "포션",
                snapshot.RunState.Snapshot.Potions.Count == 0
                    ? "- 없음"
                    : JoinLines(snapshot.RunState.Snapshot.Potions.Take(8).Select(potion => $"- {potion}")),
            });
        }
        else
        {
            PlayerText = "플레이어 상태가 아직 없습니다.";
            DeckText = "덱 정보를 아직 읽지 못했습니다.";
            RelicsPotionsText = "유물과 포션 정보가 아직 없습니다.";
        }

        if (snapshot.LatestAdvice is not null)
        {
            AdviceOverviewText = JoinLines(new[]
            {
                snapshot.LatestAdvice.Headline,
                string.Empty,
                snapshot.LatestAdvice.Summary,
                string.Empty,
                $"권장 행동: {snapshot.LatestAdvice.RecommendedAction}",
                $"권장 선택지: {snapshot.LatestAdvice.RecommendedChoiceLabel ?? "-"}",
            });

            AdviceDetailsText = JoinLines(new[]
            {
                "근거",
                FormatBulletSection(snapshot.LatestAdvice.ReasoningBullets),
                string.Empty,
                "리스크",
                FormatBulletSection(snapshot.LatestAdvice.RiskNotes),
                string.Empty,
                "부족한 정보",
                FormatBulletSection(snapshot.LatestAdvice.MissingInformation),
                string.Empty,
                "판단 차단 요인",
                FormatBulletSection(snapshot.LatestAdvice.DecisionBlockers),
                string.Empty,
                "최근 변화",
                FormatBulletSection(snapshot.RunState?.Snapshot.RecentChanges ?? Array.Empty<string>()),
            });
            ConfidenceLine = $"신뢰도: {snapshot.LatestAdvice.Confidence?.ToString("0.00") ?? "미상"}";
        }
        else
        {
            AdviceOverviewText = JoinLines(new[]
            {
                "아직 조언이 없습니다.",
                string.Empty,
                "게임을 실행했거나 live export가 붙은 뒤 '지금 분석'을 눌러 주세요.",
            });
            AdviceDetailsText = "근거와 리스크 정보가 아직 없습니다.";
            ConfidenceLine = "신뢰도: -";
        }

        CurrentChoicesText = JoinLines(
            (snapshot.RunState?.Snapshot.CurrentChoices ?? Array.Empty<LiveExportChoiceSummary>())
            .Select(choice => $"[{TranslateChoiceKind(choice.Kind)}] {choice.Label} :: {choice.Description ?? choice.Value ?? "추가 정보 없음"}")
            .DefaultIfEmpty("없음"));

        RecentEventsText = JoinLines(
            (snapshot.RunState?.RecentEvents ?? Array.Empty<LiveExportEventEnvelope>())
            .Select(evt => $"{TranslateEventKind(evt.Kind)} @ {TranslateScreen(evt.Screen)} ({evt.Act?.ToString() ?? "?"}-{evt.Floor?.ToString() ?? "?"})")
            .DefaultIfEmpty("없음"));

        KnowledgeEntriesText = JoinLines(
            (snapshot.LatestKnowledgeSlice?.Entries ?? Array.Empty<StaticKnowledgeEntry>())
            .Select(entry => $"{entry.Name} [출처: {TranslateSource(entry.Source)}]")
            .DefaultIfEmpty("없음"));

        CollectorNotesText = snapshot.CollectorStatus switch
        {
            null => "수집 런 진단 정보가 없습니다.",
            { Enabled: false } => "수집 런 모드가 비활성화되어 있습니다.",
            var collector => JoinLines(new[]
            {
                $"collector mode: {(collector.Enabled ? "on" : "off")}",
                $"최근 semantic 화면: {TranslateScreen(collector.LastSemanticScreen)}",
                $"화면 episode: {collector.ActiveScreenEpisode ?? "없음"}",
                $"선택지 추출 상태: {collector.ChoiceExtractionStatus ?? "미상"}",
                $"마지막 extractor 경로: {collector.LastAcceptedExtractorPath ?? "없음"}",
                $"마지막 degraded 이유: {collector.LastDegradedReason ?? "없음"}",
                $"session id: {collector.SessionId ?? "없음"}",
                $"지식 사용 개수: {collector.KnowledgeEntriesUsedCount}",
                $"지식 사용 이유: {(collector.KnowledgeReasons.Count > 0 ? string.Join(", ", collector.KnowledgeReasons.Take(4)) : "없음")}",
                $"지식 참조: {(collector.TopKnowledgeRefs.Count > 0 ? string.Join(", ", collector.TopKnowledgeRefs.Take(4)) : "없음")}",
                $"부족한 정보: {(collector.MissingInformation.Count > 0 ? string.Join(", ", collector.MissingInformation.Take(4)) : "없음")}",
                $"판단 차단 요인: {(collector.DecisionBlockers.Count > 0 ? string.Join(", ", collector.DecisionBlockers.Take(4)) : "없음")}",
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
                ? "재시도중"
                : "분석중";
            AnalysisStatusText = $"{prefix}: {TranslateEventKind(_analysisTriggerKind)} ({seconds}초)";
        }
        else
        {
            _analysisTimer?.Stop();
            AnalysisStatusText = state switch
            {
                "failed" => $"분석 실패: {message}",
                "canceled" => $"분석 취소: {message}",
                "degraded" => $"분석 제한: {message}",
                "retrying" => "분석 상태: 재시도 대기",
                _ => "분석 상태: 대기 중",
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
        return JoinLines(items.DefaultIfEmpty("없음").Select(item => $"- {item}"));
    }

    private static string ToModelDisplay(string? model)
    {
        return ModelOptions.FirstOrDefault(pair => string.Equals(pair.Value, model, StringComparison.OrdinalIgnoreCase)).Key ?? "기본값";
    }

    private static string ToReasoningDisplay(string? reasoning)
    {
        return ReasoningOptions.FirstOrDefault(pair => string.Equals(pair.Value, reasoning, StringComparison.OrdinalIgnoreCase)).Key ?? "Medium (default)";
    }

    private static string LocalizeStatusMessage(string? message)
    {
        return message switch
        {
            null or "" => "상태 메시지가 없습니다.",
            "Waiting for live export." => "실시간 추출을 기다리는 중입니다.",
            "state.latest.json is not available yet." => "아직 state.latest.json이 생성되지 않았습니다.",
            "Monitoring live export updates." => "실시간 추출 갱신을 감시 중입니다.",
            "Automatic advice is enabled." => "자동 조언이 켜져 있습니다.",
            "Automatic advice is paused." => "자동 조언이 일시중지되어 있습니다.",
            _ when message.StartsWith("Advice generated for ", StringComparison.Ordinal) =>
                $"조언 생성 완료: {TranslateEventKind(message["Advice generated for ".Length..])}",
            _ when message.StartsWith("AI analyzing: ", StringComparison.Ordinal) =>
                $"분석중: {TranslateEventKind(message["AI analyzing: ".Length..])}",
            _ when message.StartsWith("Retrying AI advice: ", StringComparison.Ordinal) =>
                $"재시도중: {TranslateEventKind(message["Retrying AI advice: ".Length..])}",
            _ when message.StartsWith("AI request canceled: ", StringComparison.Ordinal) =>
                $"분석 취소: {TranslateEventKind(message["AI request canceled: ".Length..])}",
            _ when message.StartsWith("AI advice degraded: ", StringComparison.Ordinal) =>
                $"조언 제한: {message["AI advice degraded: ".Length..]}",
            _ when message.StartsWith("AI request failed: ", StringComparison.Ordinal) =>
                $"분석 실패: {message["AI request failed: ".Length..]}",
            _ => message,
        };
    }

    private static string TranslateScreen(string? screen)
    {
        return screen?.Trim().ToLowerInvariant() switch
        {
            null or "" => "확인 중",
            "unknown" => "미확인",
            "main-menu" => "메인 메뉴",
            "combat" => "전투",
            "reward" or "rewards" => "보상",
            "event" => "이벤트",
            "shop" => "상점",
            "rest" or "rest-site" or "campfire" => "휴식",
            "map" => "맵",
            "victory" => "승리",
            "death" => "패배",
            "character-select" => "캐릭터 선택",
            "card-choice" => "카드 선택",
            "upgrade" => "강화",
            "transform" => "변형",
            _ => screen ?? "확인 중",
        };
    }

    private static string TranslateEventKind(string? kind)
    {
        return kind?.Trim().ToLowerInvariant() switch
        {
            null or "" => "미상",
            "runtime-poll" => "주기 상태 수집",
            "screen-changed" => "화면 전환",
            "choice-list-presented" => "??? ??",
            "choice-selected" => "??? ??",
            "main-menu" => "?? ??",
            "singleplayer-button-pressed" => "?????? ??",
            "singleplayer-submenu" => "?????? ????",
            "open-character-select" => "??? ?? ??",
            "character-select" => "??? ??",
            "character-selected" => "??? ?? ??",
            "combat-started" => "?? ??",
            "combat-ended" => "?? ??",
            "turn-started" => "? ??",
            "turn-ended" => "? ??",
            "map" => "? ??",
            "map-point-selected" => "? ?? ??",
            "map-node-entered" => "? ?? ??",
            "reward-opened" or "reward-screen-opened" => "보상 화면 열림",
            "event-opened" or "event-screen-opened" => "이벤트 화면 열림",
            "shop-opened" => "상점 화면 열림",
            "rest-opened" => "휴식 화면 열림",
            "run-started" => "런 시작",
            "run-loaded" => "런 불러오기",
            "run-ended" => "런 종료",
            "app-started" => "앱 시작",
            "app-stopped" => "앱 종료",
            "manual" => "수동 분석",
            "retry-last" => "마지막 요청 재시도",
            _ => kind ?? "미상",
        };
    }

    private static string TranslateChoiceKind(string? kind)
    {
        return kind?.Trim().ToLowerInvariant() switch
        {
            null or "" => "선택지",
            "choice" => "선택지",
            "card" => "카드",
            "reward" => "보상",
            "event" => "이벤트",
            "shop" => "상점",
            "rest" => "휴식",
            "button" => "버튼",
            "relic" => "유물",
            "potion" => "포션",
            _ => kind ?? "선택지",
        };
    }

    private static string TranslateSource(string? source)
    {
        return source?.Trim().ToLowerInvariant() switch
        {
            null or "" => "미상",
            "localization-scan" => "로컬라이제이션 스캔",
            "strict-domain-scan" => "엄격 도메인 스캔",
            "assembly-scan" => "어셈블리 스캔",
            "pck-inventory" => "PCK 인벤토리",
            "observed-merge" => "관찰 병합",
            "release-scan" => "릴리즈 스캔",
            _ => source ?? "미상",
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
