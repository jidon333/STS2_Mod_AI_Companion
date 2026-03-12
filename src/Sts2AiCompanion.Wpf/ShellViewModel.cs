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

namespace Sts2AiCompanion.Wpf;

public sealed class ShellViewModel : INotifyPropertyChanged, IAsyncDisposable
{
    private Dispatcher? _dispatcher;
    private CompanionHost? _host;
    private string _workspaceRoot = Directory.GetCurrentDirectory();

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusLine { get; private set; } = "실시간 추출을 기다리는 중입니다.";
    public string RunLine { get; private set; } = "런: 없음";
    public string ScreenLine { get; private set; } = "화면: 알 수 없음";
    public string UpdatedLine { get; private set; } = "업데이트: -";
    public string PlayerText { get; private set; } = "플레이어 상태가 아직 없습니다.";
    public string DeckText { get; private set; } = "아직 덱 정보를 읽지 못했습니다.";
    public string RelicsPotionsText { get; private set; } = "유물과 포션 정보가 아직 없습니다.";
    public string AdviceOverviewText { get; private set; } = "아직 조언이 없습니다.";
    public string AdviceDetailsText { get; private set; } = "근거와 리스크가 아직 없습니다.";
    public string CurrentChoicesText { get; private set; } = "없음";
    public string RecentEventsText { get; private set; } = "없음";
    public string KnowledgeEntriesText { get; private set; } = "없음";
    public string CollectorNotesText { get; private set; } = "수집 런 진단 정보가 없습니다.";
    public string ConfidenceLine { get; private set; } = "신뢰도: -";
    public string AutoAdviceButtonText { get; private set; } = "자동 조언 일시중지";
    public bool AutoAdviceEnabled { get; private set; } = true;

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
        AutoAdviceButtonText = AutoAdviceEnabled ? "자동 조언 일시중지" : "자동 조언 재개";
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
        StatusLine = LocalizeStatusMessage(snapshot.Status.Message);
        RunLine = $"런: {snapshot.Status.RunId ?? "없음"}";
        ScreenLine = $"화면: {TranslateScreen(snapshot.RunState?.Snapshot.CurrentScreen)}";
        UpdatedLine = $"업데이트: {snapshot.Status.UpdatedAt:yyyy-MM-dd HH:mm:ss}";
        AutoAdviceEnabled = snapshot.Status.AutoAdviceEnabled;
        AutoAdviceButtonText = AutoAdviceEnabled ? "자동 조언 일시중지" : "자동 조언 재개";

        if (snapshot.RunState is not null)
        {
            var player = snapshot.RunState.Snapshot.Player;
            PlayerText = JoinLines(new[]
            {
                $"이름: {player.Name ?? "미확인"}",
                $"체력: {player.CurrentHp?.ToString() ?? "?"}/{player.MaxHp?.ToString() ?? "?"}",
                $"골드: {player.Gold?.ToString() ?? "?"}",
                $"에너지: {player.Energy?.ToString() ?? "?"}",
            });
            DeckText = snapshot.RunState.Snapshot.Deck.Count == 0
                ? "아직 덱 정보를 읽지 못했습니다."
                : JoinLines(snapshot.RunState.Snapshot.Deck.Take(20).Select(card => $"- {card.Name}"));
            RelicsPotionsText = JoinLines(new[]
            {
                "유물:",
                snapshot.RunState.Snapshot.Relics.Count == 0
                    ? "- 없음"
                    : JoinLines(snapshot.RunState.Snapshot.Relics.Take(12).Select(relic => $"- {relic}")),
                string.Empty,
                "포션:",
                snapshot.RunState.Snapshot.Potions.Count == 0
                    ? "- 없음"
                    : JoinLines(snapshot.RunState.Snapshot.Potions.Take(8).Select(potion => $"- {potion}")),
            });
        }
        else
        {
            PlayerText = "플레이어 상태가 아직 없습니다.";
            DeckText = "아직 덱 정보를 읽지 못했습니다.";
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
                "근거:",
                FormatBulletSection(snapshot.LatestAdvice.ReasoningBullets),
                string.Empty,
                "리스크:",
                FormatBulletSection(snapshot.LatestAdvice.RiskNotes),
                string.Empty,
                "최근 변화:",
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
                "게임을 실행하거나 live export가 잡힌 뒤 '지금 분석'을 눌러 주세요.",
            });
            AdviceDetailsText = "근거와 리스크가 아직 없습니다.";
            ConfidenceLine = "신뢰도: -";
        }

        CurrentChoicesText = JoinLines(
            (snapshot.RunState?.Snapshot.CurrentChoices ?? Array.Empty<Sts2ModKit.Core.LiveExport.LiveExportChoiceSummary>())
            .Select(choice => $"[{TranslateChoiceKind(choice.Kind)}] {choice.Label} :: {choice.Description ?? choice.Value ?? "세부 정보 없음"}")
            .DefaultIfEmpty("없음"));
        RecentEventsText = JoinLines(
            (snapshot.RunState?.RecentEvents ?? Array.Empty<Sts2ModKit.Core.LiveExport.LiveExportEventEnvelope>())
            .Select(evt => $"{TranslateEventKind(evt.Kind)} @ {TranslateScreen(evt.Screen)} ({evt.Act?.ToString() ?? "?"}-{evt.Floor?.ToString() ?? "?"})")
            .DefaultIfEmpty("없음"));
        KnowledgeEntriesText = JoinLines(
            (snapshot.LatestKnowledgeSlice?.Entries ?? Array.Empty<Sts2ModKit.Core.Knowledge.StaticKnowledgeEntry>())
            .Select(entry => $"{entry.Name} [출처: {TranslateSource(entry.Source)}]")
            .DefaultIfEmpty("없음"));
        CollectorNotesText = snapshot.CollectorStatus?.Notes ?? "수집 런 모드가 비활성화되었습니다.";

        NotifyAll();
    }

    private static string JoinLines(IEnumerable<string> lines)
    {
        return string.Join(Environment.NewLine, lines.Where(line => line is not null));
    }

    private static string FormatBulletSection(IEnumerable<string> items)
    {
        return JoinLines(items.DefaultIfEmpty("없음").Select(item => $"- {item}"));
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
            "Automatic advice is paused." => "자동 조언이 일시중지되었습니다.",
            _ when message.StartsWith("Advice generated for ", StringComparison.Ordinal) =>
                $"조언 생성 완료: {TranslateEventKind(message["Advice generated for ".Length..])}",
            _ => message,
        };
    }

    private static string TranslateScreen(string? screen)
    {
        return screen?.Trim().ToLowerInvariant() switch
        {
            null or "" => "알 수 없음",
            "unknown" => "알 수 없음",
            "main-menu" => "메인 메뉴",
            "combat" => "전투",
            "reward" or "rewards" => "보상",
            "event" => "이벤트",
            "shop" => "상점",
            "rest" or "rest-site" => "휴식",
            "map" => "맵",
            "campfire" => "휴식",
            "victory" => "승리",
            "death" => "패배",
            "character-select" => "캐릭터 선택",
            _ => screen ?? "알 수 없음",
        };
    }

    private static string TranslateEventKind(string? kind)
    {
        return kind?.Trim().ToLowerInvariant() switch
        {
            null or "" => "알 수 없음",
            "runtime-poll" => "런타임 폴링",
            "screen-changed" => "화면 전환",
            "choice-list-presented" => "선택지 표시",
            "choice-selected" => "선택지 선택",
            "combat-started" => "전투 시작",
            "combat-ended" => "전투 종료",
            "turn-started" => "턴 시작",
            "turn-ended" => "턴 종료",
            "map-node-entered" => "맵 노드 진입",
            "reward-opened" or "reward-screen-opened" => "보상 화면 열림",
            "event-opened" or "event-screen-opened" => "이벤트 화면 열림",
            "shop-opened" => "상점 화면 열림",
            "rest-opened" => "휴식 화면 열림",
            "run-started" => "런 시작",
            "run-loaded" => "런 불러오기",
            "run-ended" => "런 종료",
            "app-started" => "앱 시작",
            "app-stopped" => "앱 종료",
            "save-persisted" => "저장 완료",
            _ => kind ?? "알 수 없음",
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
            "release-scan" => "릴리스 스캔",
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
    }

    private void Notify([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
