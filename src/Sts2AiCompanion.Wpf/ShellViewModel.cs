using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Threading;
using Sts2AiCompanion.AdvisorSceneDisplay;
using Sts2AiCompanion.AdvisorSceneModel;
using Sts2AiCompanion.Host;
using Sts2AiCompanion.Wpf.Display;
using WpfKnowledgeCatalogService = Sts2AiCompanion.Foundation.Knowledge.KnowledgeCatalogService;
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
    private WpfKnowledgeCatalogService? _knowledgeCatalogService;
    private ScaffoldConfiguration? _configuration;
    private AdvicePromptBuilder? _advicePromptBuilder;
    private DispatcherTimer? _analysisTimer;
    private string _workspaceRoot = Directory.GetCurrentDirectory();
    private bool _analysisInProgress;
    private DateTimeOffset? _analysisStartedAt;
    private string? _analysisTriggerKind;
    private string? _lastPersistedUiSnapshotJson;

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
    public string ConservativeAdviceText { get; private set; } = "보수적 관점이 아직 없습니다.";
    public string AggressiveAdviceText { get; private set; } = "공격적 관점이 아직 없습니다.";
    public string CurrentChoicesText { get; private set; } = "없음";
    public string SceneIdentityText { get; private set; } = "없음";
    public string SceneContextText { get; private set; } = "scene context가 아직 없습니다.";
    public string SceneSummaryText { get; private set; } = "scene model이 아직 없습니다.";
    public string SceneOptionsText { get; private set; } = "없음";
    public string SceneGapsText { get; private set; } = "없음";
    public string SceneProvenanceText { get; private set; } = "없음";
    public string RecentEventsText { get; private set; } = "없음";
    public string KnowledgeEntriesText { get; private set; } = "없음";
    public string AiPromptSummaryText { get; private set; } = "아직 생성된 AI 입력 요약이 없습니다.";
    public string AiInputText { get; private set; } = "아직 생성된 prompt-pack이 없습니다.";
    public string AiPromptPreviewText { get; private set; } = "아직 생성된 프롬프트가 없습니다.";
    public string CurrentSceneDiagnosticsText { get; private set; } = "현재 장면 진단 정보가 없습니다.";
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
        _configuration = configuration;
        _advicePromptBuilder = new AdvicePromptBuilder(configuration);
        _knowledgeCatalogService = new WpfKnowledgeCatalogService(configuration, _workspaceRoot);
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
        _knowledgeCatalogService?.ReloadIfChanged();
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
        var resolver = BuildKnowledgeResolver();
        StatusLine = LocalizeStatusMessage(snapshot.Status.Message);
        RunLine = $"런: {snapshot.Status.RunId ?? "없음"}";
        var normalizedScreen = snapshot.RunState?.NormalizedState.Scene.SemanticSceneType;
        var sceneScreen = snapshot.LatestSceneModel?.SceneType;
        ScreenLine = $"화면: {TranslateScreen(string.IsNullOrWhiteSpace(sceneScreen) ? (string.IsNullOrWhiteSpace(normalizedScreen) ? snapshot.RunState?.Snapshot.CurrentScreen : normalizedScreen) : sceneScreen)}";
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

            DeckText = DeckDisplayFormatter.FormatDeck(snapshot.RunState.Snapshot.Deck, resolver);
            RelicsPotionsText = JoinLines(new[]
            {
                DeckDisplayFormatter.FormatNamedListSection("유물", snapshot.RunState.Snapshot.Relics.Take(12).ToArray(), resolver),
                string.Empty,
                DeckDisplayFormatter.FormatNamedListSection("포션", snapshot.RunState.Snapshot.Potions.Take(8).ToArray(), resolver),
            });
        }
        else
        {
            PlayerText = "플레이어 상태가 아직 없습니다.";
            DeckText = "덱 정보를 아직 읽지 못했습니다.";
            RelicsPotionsText = "유물과 포션 정보가 아직 없습니다.";
        }

        var latestPromptPack = TryLoadLatestPromptPack(snapshot.Paths.PromptPacksRoot);

        if (snapshot.LatestAdvice is not null)
        {
            var analysisLatencyText = FormatAdviceLatency(snapshot, latestPromptPack.pack);
            AdviceOverviewText = AdviceViewDisplayFormatter.FormatFinalOverview(snapshot.LatestAdvice, analysisLatencyText);
            AdviceDetailsText = AdviceViewDisplayFormatter.FormatFinalDetails(snapshot.LatestAdvice, snapshot.RunState?.Snapshot.RecentChanges ?? Array.Empty<string>());
            ConservativeAdviceText = AdviceViewDisplayFormatter.FormatAuxiliaryView("보수적 관점", snapshot.LatestAdvice.ConservativeView);
            AggressiveAdviceText = AdviceViewDisplayFormatter.FormatAuxiliaryView("공격적 관점", snapshot.LatestAdvice.AggressiveView);
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
            ConservativeAdviceText = "보수적 관점이 아직 없습니다.";
            AggressiveAdviceText = "공격적 관점이 아직 없습니다.";
            ConfidenceLine = "신뢰도: -";
        }

        if (snapshot.LatestSceneModel is { } sceneModel)
        {
            SceneIdentityText = $"{sceneModel.SceneType} / {sceneModel.SceneStage} / {sceneModel.CanonicalOwner}";
            SceneContextText = AdvisorSceneDisplayFormatter.FormatContext(sceneModel, resolver);
            SceneSummaryText = AdvisorSceneDisplayFormatter.FormatSummary(sceneModel, resolver);
            SceneOptionsText = BuildSceneOptionsText(sceneModel, resolver, latestPromptPack.pack);
            SceneGapsText = AdvisorSceneDisplayFormatter.FormatGaps(sceneModel);
            SceneProvenanceText = AdvisorSceneDisplayFormatter.FormatProvenance(sceneModel);
        }
        else
        {
            SceneIdentityText = "없음";
            SceneContextText = "scene context가 아직 없습니다.";
            SceneSummaryText = "scene model이 아직 없습니다.";
            SceneOptionsText = "없음";
            SceneGapsText = AdvisorSceneDisplayFormatter.FormatGaps(null);
            SceneProvenanceText = AdvisorSceneDisplayFormatter.FormatProvenance(null);
        }

        CurrentChoicesText = SceneOptionsText;

        RecentEventsText = JoinLines(
            (snapshot.RunState?.RecentEvents ?? Array.Empty<LiveExportEventEnvelope>())
            .Select(evt => $"{TranslateEventKind(evt.Kind)} @ {TranslateScreen(evt.Screen)} ({evt.Act?.ToString() ?? "?"}-{evt.Floor?.ToString() ?? "?"})")
            .DefaultIfEmpty("없음"));

        KnowledgeEntriesText = JoinLines(
            (snapshot.LatestKnowledgeSlice?.Entries ?? Array.Empty<StaticKnowledgeEntry>())
            .Select(entry => $"{resolver.ResolveDisplayText(entry.Name, entry.Id)} [출처: {TranslateSource(entry.Source)}]")
            .DefaultIfEmpty("없음"));

        AiPromptSummaryText = BuildAiPromptSummaryText(snapshot, latestPromptPack.pack);
        AiInputText = BuildAiInputText(snapshot, latestPromptPack.path, latestPromptPack.pack);
        AiPromptPreviewText = BuildAiPromptPreviewText(latestPromptPack.path, latestPromptPack.pack);
        CurrentSceneDiagnosticsText = BuildCurrentSceneDiagnosticsText(snapshot, latestPromptPack.pack);
        CollectorNotesText = BuildCollectorHistoryText(snapshot);
        PersistRenderedTextArtifacts(snapshot);

        NotifyAll();
    }

    private string BuildSceneOptionsText(AdvisorSceneArtifact sceneModel, AdvisorKnowledgeDisplayResolver resolver, AdviceInputPack? promptPack)
    {
        var compact = promptPack?.CompactInput;
        if (string.Equals(sceneModel.SceneType, "event", StringComparison.OrdinalIgnoreCase)
            && string.Equals(compact?.SceneType, "event", StringComparison.OrdinalIgnoreCase)
            && compact.EventFacts is not null
            && compact.VisibleOptions.Count > 0)
        {
            var enriched = EventCompactOptionDisplayFormatter.Format(compact);
            if (!string.IsNullOrWhiteSpace(enriched))
            {
                return enriched;
            }
        }

        return AdvisorSceneDisplayFormatter.FormatOptions(sceneModel, resolver);
    }

    private (string? path, AdviceInputPack? pack) TryLoadLatestPromptPack(string? promptPacksRoot)
    {
        if (string.IsNullOrWhiteSpace(promptPacksRoot) || !Directory.Exists(promptPacksRoot))
        {
            return default;
        }

        var path = Directory.GetFiles(promptPacksRoot, "*.json", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(path))
        {
            return default;
        }

        try
        {
            var json = File.ReadAllText(path);
            var pack = JsonSerializer.Deserialize<AdviceInputPack>(json, ConfigurationLoader.JsonOptions);
            return (path, pack);
        }
        catch
        {
            return (path, null);
        }
    }

    private string BuildAiInputText(CompanionHostSnapshot snapshot, string? promptPackPath, AdviceInputPack? promptPack)
    {
        if (string.IsNullOrWhiteSpace(promptPackPath))
        {
            return "아직 생성된 prompt-pack이 없습니다.";
        }

        if (promptPack is null)
        {
            return JoinLines(new[]
            {
                $"prompt-pack: {Path.GetFileName(promptPackPath)}",
                $"path: {promptPackPath}",
                string.Empty,
                "최신 prompt-pack을 읽지 못했습니다.",
            });
        }

        var compact = promptPack.CompactInput;
        var compactMode = compact switch
        {
            null => "legacy",
            { CombatFacts.PreviewOnly: true } => "compact / combat preview-only",
            _ => $"compact / {compact.SceneType}",
        };
        var excerpt = new
        {
            trigger = promptPack.TriggerKind,
            manual = promptPack.Manual,
            current_screen = promptPack.CurrentScreen,
            summary_text = Truncate(promptPack.SummaryText, 400),
            knowledge_reasons = promptPack.KnowledgeReasons.Take(6).ToArray(),
            recent_events = promptPack.RecentEvents.TakeLast(3).Select(evt => new
            {
                evt.Kind,
                evt.Screen,
                evt.Act,
                evt.Floor,
                payload_keys = evt.Payload.Keys.Take(8).ToArray(),
            }).ToArray(),
            knowledge_entries = promptPack.KnowledgeEntries.Take(6).Select(entry => new
            {
                entry.Id,
                entry.Name,
                entry.Source,
            }).ToArray(),
            strategy_principles = (promptPack.StrategyPrinciples ?? Array.Empty<Sts2AiCompanion.Foundation.Contracts.StrategyPrincipleEntry>())
                .Take(3)
                .Select(principle => new
                {
                    principle.Id,
                    principle.Title,
                    principle.TransferConfidence,
                })
                .ToArray(),
            compact_input = compact is null ? null : new
            {
                compact.SceneType,
                compact.SceneStage,
                compact.CanonicalOwner,
                run_context = compact.RunContext,
                player_summary = compact.PlayerSummary,
                visible_options = compact.VisibleOptions.Take(12).ToArray(),
                reward_facts = compact.RewardFacts,
                event_facts = compact.EventFacts,
                shop_facts = compact.ShopFacts,
                combat_facts = compact.CombatFacts,
                missing_information = compact.MissingInformation,
                decision_blockers = compact.DecisionBlockers,
            },
        };

        return JoinLines(new[]
        {
            $"prompt-pack: {Path.GetFileName(promptPackPath)}",
            $"path: {promptPackPath}",
            $"created: {promptPack.CreatedAt:yyyy-MM-dd HH:mm:ss}",
            $"trigger: {TranslateEventKind(promptPack.TriggerKind)}",
            $"manual: {(promptPack.Manual ? "yes" : "no")}",
            $"current screen: {TranslateScreen(promptPack.CurrentScreen)}",
            $"advisor path: {compactMode}",
            $"knowledge entries: {promptPack.KnowledgeEntries.Count}",
            $"knowledge reasons: {(promptPack.KnowledgeReasons.Count > 0 ? string.Join(", ", promptPack.KnowledgeReasons.Take(6)) : "none")}",
            $"latest advice status: {snapshot.LatestAdvice?.Status ?? "none"}",
            FormatAdviceLatency(snapshot, promptPack) is { } latency ? $"analysis duration: {latency}" : null,
            string.Empty,
            "입력 발췌",
            SerializePreview(excerpt),
        });
    }

    private string BuildAiPromptSummaryText(CompanionHostSnapshot snapshot, AdviceInputPack? promptPack)
    {
        if (promptPack is null)
        {
            return "아직 생성된 AI 입력 요약이 없습니다.";
        }

        var compact = promptPack.CompactInput;
        var visibleLabels = compact?.VisibleOptions.Select(option => option.Label).Take(6).ToArray() ?? Array.Empty<string>();
        var currentMissing = snapshot.LatestAdvice?.MissingInformation ?? compact?.MissingInformation ?? Array.Empty<string>();
        var currentBlockers = snapshot.LatestAdvice?.DecisionBlockers ?? compact?.DecisionBlockers ?? Array.Empty<string>();
        var keyKnowledge = (snapshot.LatestAdvice?.KnowledgeRefs.Count > 0
                ? snapshot.LatestAdvice.KnowledgeRefs
                : compact?.KnowledgeEntries.Select(entry => entry.Name).ToArray())
            ?? Array.Empty<string>();
        var strategyPrinciples = promptPack.StrategyPrinciples ?? Array.Empty<Sts2AiCompanion.Foundation.Contracts.StrategyPrincipleEntry>();

        return JoinLines(new[]
        {
            $"AI는 지금 `trigger={promptPack.TriggerKind}`, `scene={compact?.SceneType ?? promptPack.CurrentScreen}` 기준으로 판단합니다.",
            $"보이는 선택지는 {(visibleLabels.Length > 0 ? string.Join(", ", visibleLabels) : "없음")} 입니다.",
            $"런 맥락은 HP {compact?.RunContext.CurrentHp?.ToString() ?? "?"}/{compact?.RunContext.MaxHp?.ToString() ?? "?"}, 골드 {compact?.RunContext.Gold?.ToString() ?? "?"}, 덱 {compact?.RunContext.DeckCount.ToString() ?? "?"}장 수준으로 압축돼 들어갑니다.",
            $"AI가 확신 못 하는 이유는 {(currentMissing.Count > 0 ? string.Join(", ", currentMissing.Take(4)) : "없음")} 입니다.",
            $"실제 차단 요인은 {(currentBlockers.Count > 0 ? string.Join(", ", currentBlockers.Take(4)) : "없음")} 입니다.",
            $"참고 지식은 {(keyKnowledge.Count > 0 ? string.Join(", ", keyKnowledge.Take(4)) : "거의 없음")} 정도만 씁니다.",
            $"전략 원칙은 {(strategyPrinciples.Count > 0 ? string.Join(", ", strategyPrinciples.Select(principle => principle.Title).Take(3)) : "없음")} 수준으로만 보조 렌즈로 붙습니다.",
            compact?.EventFacts is not null && compact.EventFacts.EventIdentityMissing
                ? "이 event는 정체가 비어 있어, 선택지 효과 구조화가 약하면 추천을 멈추는 쪽으로 동작합니다."
                : null,
            compact?.CombatFacts?.PreviewOnly == true
                ? "combat는 현재 preview-only라 facts만 읽고 추천은 하지 않습니다."
                : null,
        });
    }

    private string BuildAiPromptPreviewText(string? promptPackPath, AdviceInputPack? promptPack)
    {
        if (string.IsNullOrWhiteSpace(promptPackPath) || promptPack is null)
        {
            return "아직 생성된 프롬프트가 없습니다.";
        }

        if (_advicePromptBuilder is null)
        {
            return "프롬프트 빌더가 아직 초기화되지 않았습니다.";
        }

        try
        {
            var prompt = _advicePromptBuilder.FormatPrompt(promptPack);
            var preview = Truncate(prompt, 24000);
            return JoinLines(new[]
            {
                $"prompt-pack: {Path.GetFileName(promptPackPath)}",
                $"prompt length: {prompt.Length} chars",
                string.Empty,
                preview,
                preview.Length < prompt.Length ? string.Empty : string.Empty,
                preview.Length < prompt.Length ? "(프롬프트가 길어 일부만 표시했습니다.)" : string.Empty,
            });
        }
        catch (Exception exception)
        {
            return JoinLines(new[]
            {
                $"prompt-pack: {Path.GetFileName(promptPackPath)}",
                string.Empty,
                $"프롬프트를 재구성하지 못했습니다: {exception.Message}",
            });
        }
    }

    private string BuildCurrentSceneDiagnosticsText(CompanionHostSnapshot snapshot, AdviceInputPack? promptPack)
    {
        var collector = snapshot.CollectorStatus;
        var normalizedScene = snapshot.RunState?.NormalizedState.Scene.SemanticSceneType;
        var latestScene = snapshot.LatestSceneModel?.SceneType;
        var currentScene = string.IsNullOrWhiteSpace(latestScene)
            ? (string.IsNullOrWhiteSpace(normalizedScene) ? snapshot.RunState?.Snapshot.CurrentScreen : normalizedScene)
            : latestScene;
        var historicalReason = collector?.LastDegradedReason;
        var currentCompact = promptPack?.CompactInput;
        var currentBlockers = snapshot.LatestAdvice?.DecisionBlockers ?? currentCompact?.DecisionBlockers ?? Array.Empty<string>();
        var currentMissing = snapshot.LatestAdvice?.MissingInformation ?? currentCompact?.MissingInformation ?? Array.Empty<string>();
        var analysisLatency = FormatAdviceLatency(snapshot, promptPack);

        var lines = new List<string>
        {
            $"현재 scene: {(snapshot.LatestSceneModel is null ? TranslateScreen(currentScene) : snapshot.LatestSceneModel.SceneType + " / " + snapshot.LatestSceneModel.SceneStage + " / " + snapshot.LatestSceneModel.CanonicalOwner)}",
            $"normalized scene: {TranslateScreen(normalizedScene)}",
            $"화면 episode: {collector?.ActiveScreenEpisode ?? "없음"}",
            $"선택지 추출: {collector?.ChoiceExtractionStatus ?? "미상"}",
            $"현재 선택지 수: {snapshot.LatestSceneModel?.Options.Count ?? snapshot.RunState?.Snapshot.CurrentChoices.Count ?? 0}",
            $"현재 advice 상태: {snapshot.LatestAdvice?.Status ?? "없음"}",
            analysisLatency is null ? "최근 분석 시간: 없음" : $"최근 분석 시간: {analysisLatency}",
            $"현재 compact path: {DescribeCompactPath(currentCompact)}",
            $"지식 slice: {(snapshot.LatestKnowledgeSlice?.Entries.Count ?? 0)}개 / {(snapshot.LatestKnowledgeSlice?.Reasons.Count > 0 ? string.Join(", ", snapshot.LatestKnowledgeSlice!.Reasons.Take(4)) : "이유 없음")}",
            $"부족한 정보: {(currentMissing.Count > 0 ? string.Join(", ", currentMissing.Take(6)) : "없음")}",
            $"판단 차단 요인: {(currentBlockers.Count > 0 ? string.Join(", ", currentBlockers.Take(6)) : "없음")}",
        };

        if (string.IsNullOrWhiteSpace(collector?.LastSemanticScreen) && !string.IsNullOrWhiteSpace(normalizedScene))
        {
            lines.Add("참고: latest semantic screen이 비어 있어도 normalized scene/episode가 현재 장면을 유지하고 있습니다.");
        }

        if (LooksLikeHistoricalDegradedReason(currentScene, historicalReason))
        {
            lines.Add("주의: 마지막 degraded 이유는 현재 scene 실패가 아니라 이전 combat/map 수집 실패 이력이 섞인 collector 히스토리일 가능성이 큽니다.");
        }

        return JoinLines(lines);
    }

    private static string BuildCollectorHistoryText(CompanionHostSnapshot snapshot)
    {
        return snapshot.CollectorStatus switch
        {
            null => "수집 런 진단 정보가 없습니다.",
            { Enabled: false } => "수집 런 모드가 비활성화되어 있습니다.",
            var collector => JoinLines(new[]
            {
                $"collector mode: {(collector.Enabled ? "on" : "off")}",
                $"최근 semantic 화면(collector): {TranslateScreen(collector.LastSemanticScreen)}",
                $"active screen episode: {collector.ActiveScreenEpisode ?? "없음"}",
                $"choice extraction: {collector.ChoiceExtractionStatus ?? "미상"}",
                $"extractor path: {collector.LastAcceptedExtractorPath ?? "없음"}",
                $"마지막 degraded 이유(collector 히스토리): {collector.LastDegradedReason ?? "없음"}",
                $"session id: {collector.SessionId ?? "없음"}",
                $"지식 사용 개수: {collector.KnowledgeEntriesUsedCount}",
                $"지식 사용 이유: {(collector.KnowledgeReasons.Count > 0 ? string.Join(", ", collector.KnowledgeReasons.Take(4)) : "없음")}",
                $"지식 참조(top): {(collector.TopKnowledgeRefs.Count > 0 ? string.Join(", ", collector.TopKnowledgeRefs.Take(4)) : "없음")}",
            }),
        };
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

    private static string FormatConfidence(IReadOnlyDictionary<string, double> confidence)
    {
        if (confidence.Count == 0)
        {
            return "- 없음";
        }

        return JoinLines(confidence
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => $"- {pair.Key}: {pair.Value:0.00}"));
    }

    private static string SerializePreview(object value)
    {
        return JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
    }

    private void PersistRenderedTextArtifacts(CompanionHostSnapshot snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot.Paths.RunRoot))
        {
            return;
        }

        try
        {
            var directory = Path.Combine(snapshot.Paths.RunRoot!, "wpf-sidecar");
            Directory.CreateDirectory(directory);

            var rendered = new
            {
                generatedAt = DateTimeOffset.UtcNow,
                runId = snapshot.Status.RunId,
                screen = snapshot.RunState?.Snapshot.CurrentScreen,
                advisorPath = DescribeCompactPath(TryLoadLatestPromptPack(snapshot.Paths.PromptPacksRoot).pack?.CompactInput),
                panels = new Dictionary<string, string>
                {
                    ["statusLine"] = StatusLine,
                    ["runLine"] = RunLine,
                    ["screenLine"] = ScreenLine,
                    ["updatedLine"] = UpdatedLine,
                    ["analysisStatusText"] = AnalysisStatusText,
                    ["playerText"] = PlayerText,
                    ["deckText"] = DeckText,
                    ["relicsPotionsText"] = RelicsPotionsText,
                    ["adviceOverviewText"] = AdviceOverviewText,
                    ["adviceDetailsText"] = AdviceDetailsText,
                    ["conservativeAdviceText"] = ConservativeAdviceText,
                    ["aggressiveAdviceText"] = AggressiveAdviceText,
                    ["sceneIdentityText"] = SceneIdentityText,
                    ["sceneContextText"] = SceneContextText,
                    ["sceneSummaryText"] = SceneSummaryText,
                    ["sceneOptionsText"] = SceneOptionsText,
                    ["sceneGapsText"] = SceneGapsText,
                    ["sceneProvenanceText"] = SceneProvenanceText,
                    ["recentEventsText"] = RecentEventsText,
                    ["knowledgeEntriesText"] = KnowledgeEntriesText,
                    ["aiPromptSummaryText"] = AiPromptSummaryText,
                    ["aiInputText"] = AiInputText,
                    ["aiPromptPreviewText"] = AiPromptPreviewText,
                    ["currentSceneDiagnosticsText"] = CurrentSceneDiagnosticsText,
                    ["collectorNotesText"] = CollectorNotesText,
                    ["confidenceLine"] = ConfidenceLine,
                    ["autoAdviceButtonText"] = AutoAdviceButtonText,
                    ["selectedModelOption"] = SelectedModelOption,
                    ["selectedReasoningOption"] = SelectedReasoningOption,
                },
                renderedText = BuildRenderedUiLogText(),
            };

            var json = JsonSerializer.Serialize(rendered, new JsonSerializerOptions { WriteIndented = true });
            if (string.Equals(json, _lastPersistedUiSnapshotJson, StringComparison.Ordinal))
            {
                return;
            }

            _lastPersistedUiSnapshotJson = json;
            File.WriteAllText(Path.Combine(directory, "wpf-rendered.latest.json"), json, Encoding.UTF8);
            File.WriteAllText(Path.Combine(directory, "wpf-rendered.latest.md"), BuildRenderedUiLogText(), Encoding.UTF8);
            File.AppendAllText(Path.Combine(directory, "wpf-rendered.ndjson"), json + Environment.NewLine, Encoding.UTF8);
        }
        catch
        {
            // WPF artifact logging must not disrupt the live sidecar.
        }
    }

    private string BuildRenderedUiLogText()
    {
        return JoinLines(new[]
        {
            "# WPF Rendered Snapshot",
            string.Empty,
            "## 상태",
            StatusLine,
            RunLine,
            ScreenLine,
            UpdatedLine,
            AnalysisStatusText,
            ConfidenceLine,
            $"모델: {SelectedModelOption}",
            $"추론 강도: {SelectedReasoningOption}",
            $"자동 조언 버튼: {AutoAdviceButtonText}",
            string.Empty,
            "## 플레이어",
            PlayerText,
            string.Empty,
            "## 덱",
            DeckText,
            string.Empty,
            "## 유물 / 포션",
            RelicsPotionsText,
            string.Empty,
            "## 조언 개요",
            AdviceOverviewText,
            string.Empty,
            "## 조언 상세",
            AdviceDetailsText,
            string.Empty,
            "## 보수적 조언",
            ConservativeAdviceText,
            string.Empty,
            "## 공격적 조언",
            AggressiveAdviceText,
            string.Empty,
            "## 장면 정체",
            SceneIdentityText,
            string.Empty,
            "## 장면 맥락",
            SceneContextText,
            string.Empty,
            "## 장면 요약",
            SceneSummaryText,
            string.Empty,
            "## 보이는 선택지",
            SceneOptionsText,
            string.Empty,
            "## 누락 정보 / gaps",
            SceneGapsText,
            string.Empty,
            "## provenance",
            SceneProvenanceText,
            string.Empty,
            "## 최근 이벤트",
            RecentEventsText,
            string.Empty,
            "## 관련 지식",
            KnowledgeEntriesText,
            string.Empty,
            "## AI 입력 해석",
            AiPromptSummaryText,
            string.Empty,
            "## AI 입력",
            AiInputText,
            string.Empty,
            "## AI 프롬프트 미리보기",
            AiPromptPreviewText,
            string.Empty,
            "## 현재 장면 진단",
            CurrentSceneDiagnosticsText,
            string.Empty,
            "## 수집 런 진단",
            CollectorNotesText,
        });
    }

    private static string Truncate(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
        {
            return text ?? string.Empty;
        }

        return text[..maxLength] + Environment.NewLine + "...(truncated)";
    }

    private static string? FormatAdviceLatency(CompanionHostSnapshot snapshot, AdviceInputPack? promptPack)
    {
        if (snapshot.LatestAdvice is null || promptPack is null)
        {
            return null;
        }

        var startedAt = promptPack.CreatedAt;
        var finishedAt = snapshot.LatestAdvice.GeneratedAt;
        if (finishedAt < startedAt)
        {
            return null;
        }

        var elapsed = finishedAt - startedAt;
        if (elapsed.TotalSeconds >= 10)
        {
            return $"{elapsed.TotalSeconds:0.0}초";
        }

        if (elapsed.TotalSeconds >= 1)
        {
            return $"{elapsed.TotalSeconds:0.00}초";
        }

        return $"{Math.Max(0, (int)elapsed.TotalMilliseconds)}ms";
    }

    private static string DescribeCompactPath(Sts2AiCompanion.Foundation.Contracts.RewardEventCompactAdvisorInput? compactInput)
    {
        return compactInput switch
        {
            null => "legacy / no compact input",
            { CombatFacts.PreviewOnly: true } => "combat preview-only / no-call",
            _ => $"compact / {compactInput.SceneType}",
        };
    }

    private static bool LooksLikeHistoricalDegradedReason(string? currentScene, string? degradedReason)
    {
        if (string.IsNullOrWhiteSpace(currentScene) || string.IsNullOrWhiteSpace(degradedReason))
        {
            return false;
        }

        var normalizedScene = currentScene.Trim().ToLowerInvariant();
        var reason = degradedReason.Trim().ToLowerInvariant();
        if (!normalizedScene.Contains("combat", StringComparison.Ordinal) && reason.Contains("combat", StringComparison.Ordinal))
        {
            return true;
        }

        if (!normalizedScene.Contains("map", StringComparison.Ordinal) && reason.Contains("map", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
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

    private AdvisorKnowledgeDisplayResolver BuildKnowledgeResolver()
    {
        var catalog = _knowledgeCatalogService?.ReloadIfChanged() ?? _knowledgeCatalogService?.CurrentCatalog;
        return new AdvisorKnowledgeDisplayResolver(catalog ?? Sts2ModKit.Core.Knowledge.StaticKnowledgeCatalog.CreateEmpty());
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
        Notify(nameof(ConservativeAdviceText));
        Notify(nameof(AggressiveAdviceText));
        Notify(nameof(CurrentChoicesText));
        Notify(nameof(SceneIdentityText));
        Notify(nameof(SceneContextText));
        Notify(nameof(SceneSummaryText));
        Notify(nameof(SceneOptionsText));
        Notify(nameof(SceneGapsText));
        Notify(nameof(SceneProvenanceText));
        Notify(nameof(RecentEventsText));
        Notify(nameof(KnowledgeEntriesText));
        Notify(nameof(AiPromptSummaryText));
        Notify(nameof(AiInputText));
        Notify(nameof(AiPromptPreviewText));
        Notify(nameof(CurrentSceneDiagnosticsText));
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
