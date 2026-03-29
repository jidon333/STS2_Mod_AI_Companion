using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModAiCompanion.Mod;
using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Harness;
using Sts2ModKit.Core.LiveExport;
using static GuiSmokeChoicePrimitiveSupport;
using static ObserverScreenProvenance;

sealed class GuiSmokeStepAnalysisContext
{
    private static readonly RewardMapLayerState EmptyRewardMapLayerState = new(false, false, false, false, false, false, false, false, false, false, false);
    private static readonly MapOverlayState EmptyMapOverlayState = new(false, false, false, false, false, false, false);

    private readonly GuiSmokePhase _phase;
    private readonly ObserverState _observer;
    private readonly WindowBounds? _windowBounds;
    private readonly string? _screenshotPath;
    private readonly IReadOnlyList<GuiSmokeHistoryEntry> _history;
    private readonly IReadOnlyList<CombatCardKnowledgeHint> _combatCardKnowledge;
    private readonly Func<bool> _useCombatFastPathFactory;
    private readonly Func<bool> _useRewardFastPathFactory;
    private readonly Func<RewardMapLayerState> _rewardMapLayerStateFactory;
    private readonly Func<bool> _rewardBackNavigationAvailableFactory;
    private readonly Func<bool> _claimableRewardPresentFactory;
    private readonly Func<MapOverlayState> _mapOverlayStateFactory;
    private readonly Func<RewardSceneState> _rewardSceneFactory;
    private readonly Func<EventSceneState> _eventSceneFactory;
    private readonly Func<ICanonicalNonCombatSceneState?> _canonicalNonCombatSceneFactory;
    private readonly Func<ReconstructedHandleCombatContext> _combatContextFactory;
    private readonly Func<PendingCombatSelection?> _pendingCombatSelectionFactory;
    private readonly Func<CombatRuntimeState> _runtimeCombatStateFactory;
    private readonly Func<AutoCombatAnalysis> _combatAnalysisFactory;
    private readonly Func<AutoCombatHandAnalysis> _combatHandAnalysisFactory;
    private readonly Func<bool> _combatPlayerActionWindowClosedFactory;
    private readonly Func<bool> _hasSelectedNonEnemyConfirmEvidenceFactory;
    private readonly Func<bool> _canResolveCombatEnemyTargetFactory;
    private readonly Func<CombatBarrierEvaluation> _combatBarrierEvaluationFactory;

    private bool? _useCombatFastPath;
    private bool? _useRewardFastPath;
    private RewardMapLayerState? _rewardMapLayerState;
    private bool? _rewardBackNavigationAvailable;
    private bool? _claimableRewardPresent;
    private MapOverlayState? _mapOverlayState;
    private RewardSceneState? _rewardScene;
    private EventSceneState? _eventScene;
    private ICanonicalNonCombatSceneState? _canonicalNonCombatScene;
    private bool _canonicalNonCombatSceneComputed;
    private ReconstructedHandleCombatContext? _combatContext;
    private PendingCombatSelection? _pendingCombatSelection;
    private bool _pendingCombatSelectionComputed;
    private CombatRuntimeState? _runtimeCombatState;
    private AutoCombatAnalysis? _combatAnalysis;
    private AutoCombatHandAnalysis? _combatHandAnalysis;
    private bool? _combatPlayerActionWindowClosed;
    private bool? _hasSelectedNonEnemyConfirmEvidence;
    private bool? _canResolveCombatEnemyTarget;
    private CombatBarrierEvaluation? _combatBarrierEvaluation;

    public GuiSmokeStepAnalysisContext(
        GuiSmokePhase phase,
        ObserverState observer,
        WindowBounds? windowBounds,
        string? screenshotPath,
        IReadOnlyList<GuiSmokeHistoryEntry> history,
        IReadOnlyList<CombatCardKnowledgeHint> combatCardKnowledge,
        Func<bool> useCombatFastPathFactory,
        Func<bool> useRewardFastPathFactory,
        Func<RewardMapLayerState> rewardMapLayerStateFactory,
        Func<bool> rewardBackNavigationAvailableFactory,
        Func<bool> claimableRewardPresentFactory,
        Func<MapOverlayState> mapOverlayStateFactory,
        Func<RewardSceneState> rewardSceneFactory,
        Func<EventSceneState> eventSceneFactory,
        Func<ICanonicalNonCombatSceneState?> canonicalNonCombatSceneFactory,
        Func<ReconstructedHandleCombatContext> combatContextFactory,
        Func<PendingCombatSelection?> pendingCombatSelectionFactory,
        Func<CombatRuntimeState> runtimeCombatStateFactory,
        Func<AutoCombatAnalysis> combatAnalysisFactory,
        Func<AutoCombatHandAnalysis> combatHandAnalysisFactory,
        Func<bool> combatPlayerActionWindowClosedFactory,
        Func<bool> hasSelectedNonEnemyConfirmEvidenceFactory,
        Func<bool> canResolveCombatEnemyTargetFactory,
        Func<CombatBarrierEvaluation> combatBarrierEvaluationFactory)
    {
        _phase = phase;
        _observer = observer;
        _windowBounds = windowBounds;
        _screenshotPath = screenshotPath;
        _history = history;
        _combatCardKnowledge = combatCardKnowledge;
        _useCombatFastPathFactory = useCombatFastPathFactory;
        _useRewardFastPathFactory = useRewardFastPathFactory;
        _rewardMapLayerStateFactory = rewardMapLayerStateFactory;
        _rewardBackNavigationAvailableFactory = rewardBackNavigationAvailableFactory;
        _claimableRewardPresentFactory = claimableRewardPresentFactory;
        _mapOverlayStateFactory = mapOverlayStateFactory;
        _rewardSceneFactory = rewardSceneFactory;
        _eventSceneFactory = eventSceneFactory;
        _canonicalNonCombatSceneFactory = canonicalNonCombatSceneFactory;
        _combatContextFactory = combatContextFactory;
        _pendingCombatSelectionFactory = pendingCombatSelectionFactory;
        _runtimeCombatStateFactory = runtimeCombatStateFactory;
        _combatAnalysisFactory = combatAnalysisFactory;
        _combatHandAnalysisFactory = combatHandAnalysisFactory;
        _combatPlayerActionWindowClosedFactory = combatPlayerActionWindowClosedFactory;
        _hasSelectedNonEnemyConfirmEvidenceFactory = hasSelectedNonEnemyConfirmEvidenceFactory;
        _canResolveCombatEnemyTargetFactory = canResolveCombatEnemyTargetFactory;
        _combatBarrierEvaluationFactory = combatBarrierEvaluationFactory;
    }

    public GuiSmokePhase Phase => _phase;

    public ObserverState Observer => _observer;

    public WindowBounds? WindowBounds => _windowBounds;

    public string? ScreenshotPath => _screenshotPath;

    public bool HasScreenshotEvidence => !string.IsNullOrWhiteSpace(_screenshotPath);

    public IReadOnlyList<GuiSmokeHistoryEntry> History => _history;

    public IReadOnlyList<CombatCardKnowledgeHint> CombatCardKnowledge => _combatCardKnowledge;

    public bool UseCombatFastPath => _useCombatFastPath ??= _useCombatFastPathFactory();

    public bool UseRewardFastPath => _useRewardFastPath ??= _useRewardFastPathFactory();

    public bool UseAuthorityFastPath => UseCombatFastPath || UseRewardFastPath;

    public RewardMapLayerState RewardMapLayerState => _rewardMapLayerState ??= _rewardMapLayerStateFactory();

    public bool RewardBackNavigationAvailable => _rewardBackNavigationAvailable ??= _rewardBackNavigationAvailableFactory();

    public bool ClaimableRewardPresent => _claimableRewardPresent ??= _claimableRewardPresentFactory();

    public MapOverlayState MapOverlayState => _mapOverlayState ??= _mapOverlayStateFactory();

    public RewardSceneState RewardScene => _rewardScene ??= _rewardSceneFactory();

    public EventSceneState EventScene => _eventScene ??= _eventSceneFactory();

    public ICanonicalNonCombatSceneState? CanonicalNonCombatScene
    {
        get
        {
            if (!_canonicalNonCombatSceneComputed)
            {
                _canonicalNonCombatScene = _canonicalNonCombatSceneFactory();
                _canonicalNonCombatSceneComputed = true;
            }

            return _canonicalNonCombatScene;
        }
    }

    public ReconstructedHandleCombatContext CombatContext => _combatContext ??= _combatContextFactory();

    public PendingCombatSelection? PendingCombatSelection
    {
        get
        {
            if (!_pendingCombatSelectionComputed)
            {
                _pendingCombatSelection = _pendingCombatSelectionFactory();
                _pendingCombatSelectionComputed = true;
            }

            return _pendingCombatSelection;
        }
    }

    public CombatRuntimeState RuntimeCombatState => _runtimeCombatState ??= _runtimeCombatStateFactory();

    public AutoCombatAnalysis CombatAnalysis => _combatAnalysis ??= _combatAnalysisFactory();

    public AutoCombatHandAnalysis CombatHandAnalysis => _combatHandAnalysis ??= _combatHandAnalysisFactory();

    public bool CombatPlayerActionWindowClosed => _combatPlayerActionWindowClosed ??= _combatPlayerActionWindowClosedFactory();

    public bool HasSelectedNonEnemyConfirmEvidence => _hasSelectedNonEnemyConfirmEvidence ??= _hasSelectedNonEnemyConfirmEvidenceFactory();

    public bool CanResolveCombatEnemyTarget => _canResolveCombatEnemyTarget ??= _canResolveCombatEnemyTargetFactory();

    public CombatBarrierEvaluation CombatBarrierEvaluation => _combatBarrierEvaluation ??= _combatBarrierEvaluationFactory();

    public static GuiSmokeStepAnalysisContext CreateForHandleCombatRequest(GuiSmokeStepRequest request)
    {
        var observer = new ObserverState(request.Observer, null, null, request.Observer.LastEventsTail?.ToArray() ?? Array.Empty<string>());
        var history = request.History;
        var combatCardKnowledge = request.CombatCardKnowledge;
        var screenshotPath = request.ScreenshotPath;
        ReconstructedHandleCombatContext? combatContext = null;
        PendingCombatSelection? pendingSelection = null;
        var pendingSelectionComputed = false;
        CombatRuntimeState? runtimeCombatState = null;
        AutoCombatAnalysis? combatAnalysis = null;
        AutoCombatHandAnalysis? combatHandAnalysis = null;
        CombatBarrierEvaluation? combatBarrierEvaluation = null;
        RewardSceneState? rewardScene = null;
        EventSceneState? eventScene = null;
        ICanonicalNonCombatSceneState? canonicalNonCombatScene = null;
        var canonicalNonCombatSceneComputed = false;

        ReconstructedHandleCombatContext GetCombatContext()
            => combatContext ??= HandleCombatContextSupport.Reconstruct(history);

        PendingCombatSelection? GetPendingSelection()
        {
            if (!pendingSelectionComputed)
            {
                pendingSelection = CombatRuntimeStateSupport.ResolvePendingSelection(observer.Summary, combatCardKnowledge, GetCombatContext().PendingSelection);
                pendingSelectionComputed = true;
            }

            return pendingSelection;
        }

        CombatRuntimeState GetRuntimeCombatState()
            => runtimeCombatState ??= CombatRuntimeStateSupport.Read(observer.Summary, combatCardKnowledge);

        AutoCombatAnalysis GetCombatAnalysis()
            => combatAnalysis ??= string.IsNullOrWhiteSpace(screenshotPath)
                ? new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown)
                : AutoCombatAnalyzer.Analyze(screenshotPath);

        AutoCombatHandAnalysis GetCombatHandAnalysis()
            => combatHandAnalysis ??= string.IsNullOrWhiteSpace(screenshotPath)
                ? new AutoCombatHandAnalysis(Array.Empty<AutoCombatHandSlotAnalysis>())
                : AutoCombatHandAnalyzer.Analyze(screenshotPath);

        bool HasSelectedNonEnemyConfirmEvidence()
        {
            var pendingSelection = GetPendingSelection();
            if (CombatRuntimeStateSupport.HasRuntimeSelectedNonEnemyConfirmEvidence(observer.Summary, combatCardKnowledge, pendingSelection))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(screenshotPath)
                   && CombatEligibilitySupport.HasSelectedNonEnemyConfirmEvidence(observer.Summary, combatCardKnowledge, GetCombatAnalysis(), pendingSelection);
        }

        CombatBarrierEvaluation GetCombatBarrierEvaluation()
            => combatBarrierEvaluation ??= CombatBarrierSupport.Evaluate(
                history,
                observer,
                GetCombatContext(),
                GetRuntimeCombatState(),
                string.IsNullOrWhiteSpace(screenshotPath)
                    ? new AutoCombatAnalysis(false, AutoCombatOverlayBand.None, false, false, AutoCombatCardKind.Unknown)
                    : GetCombatAnalysis(),
                HasSelectedNonEnemyConfirmEvidence(),
                CanResolveCombatEnemyTarget(),
                CombatEligibilitySupport.IsCombatPlayerActionWindowClosed(observer.Summary));

        RewardSceneState GetRewardScene()
            => rewardScene ??= AutoDecisionProvider.BuildRewardSceneState(observer, request.WindowBounds, history, screenshotPath);

        EventSceneState GetEventScene()
            => eventScene ??= AutoDecisionProvider.BuildEventSceneState(observer, request.WindowBounds, history, screenshotPath);

        ICanonicalNonCombatSceneState? GetCanonicalNonCombatScene()
        {
            if (!canonicalNonCombatSceneComputed)
            {
                canonicalNonCombatScene = AutoDecisionProvider.TryBuildCanonicalNonCombatSceneState(observer, request.WindowBounds, history, screenshotPath);
                canonicalNonCombatSceneComputed = true;
            }

            return canonicalNonCombatScene;
        }

        bool CanResolveCombatEnemyTarget()
        {
            var pending = GetPendingSelection();
            if (CombatRuntimeStateSupport.CanResolveEnemyTargetWithoutScreenshot(observer.Summary, combatCardKnowledge, pending))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(screenshotPath)
                   && CombatRuntimeStateSupport.CanResolveEnemyTarget(observer.Summary, combatCardKnowledge, pending, GetCombatAnalysis());
        }

        static bool LooksLikeInspectOverlayForeground(ObserverSummary summary)
        {
            return summary.CurrentChoices.Any(static label =>
                       label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase))
                   || summary.ActionNodes.Any(static node =>
                       node.Label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                       || node.Label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                       || node.Label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
        }

        static bool HasRestSiteAuthorityForCombat(ObserverSummary summary)
        {
            return string.Equals(summary.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(summary.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase)
                   || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(summary);
        }

        bool ComputeUseCombatFastPath()
        {
            if (!string.Equals(request.Phase, GuiSmokePhase.HandleCombat.ToString(), StringComparison.OrdinalIgnoreCase)
                || observer.InCombat != true)
            {
                return false;
            }

            var combatScreenVisible = MatchesControlFlowScreen(observer, "combat");
            if (!combatScreenVisible)
            {
                return false;
            }

            var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, null);
            if (RewardObserverSignals.IsTerminalRunBoundary(observer.Summary)
                || CardSelectionObserverSignals.TryGetState(observer.Summary) is not null
                || TreasureRoomObserverSignals.IsTreasureAuthorityActive(observer.Summary)
                || ShopObserverSignals.IsShopAuthorityActive(observer.Summary)
                || RewardObserverSignals.IsRewardAuthorityActive(observer.Summary)
                || HasRestSiteAuthorityForCombat(observer.Summary)
                || eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.Active
                || eventScene.EventForegroundOwned && eventScene.HasExplicitProgression
                || LooksLikeInspectOverlayForeground(observer.Summary))
            {
                return false;
            }

            return true;
        }

        return new GuiSmokeStepAnalysisContext(
            GuiSmokePhase.HandleCombat,
            observer,
            request.WindowBounds,
            screenshotPath,
            history,
            combatCardKnowledge,
            ComputeUseCombatFastPath,
            () => false,
            () => EmptyRewardMapLayerState with { TerminalRunBoundary = RewardObserverSignals.IsTerminalRunBoundary(observer.Summary) },
            () => false,
            () => false,
            () => EmptyMapOverlayState,
            GetRewardScene,
            GetEventScene,
            GetCanonicalNonCombatScene,
            GetCombatContext,
            GetPendingSelection,
            GetRuntimeCombatState,
            GetCombatAnalysis,
            GetCombatHandAnalysis,
            () => CombatEligibilitySupport.IsCombatPlayerActionWindowClosed(observer.Summary),
            HasSelectedNonEnemyConfirmEvidence,
            CanResolveCombatEnemyTarget,
            GetCombatBarrierEvaluation);
    }
}
