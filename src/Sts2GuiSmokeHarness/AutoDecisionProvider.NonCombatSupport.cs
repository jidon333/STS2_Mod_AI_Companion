using System.Drawing;
using System.Globalization;
using System.Text.Json;
using static GuiSmokeChoicePrimitiveSupport;

sealed partial class AutoDecisionProvider
{
    private static (string? ForegroundKind, string? BackgroundKind) DescribeForegroundBackground(GuiSmokeStepRequest request)
    {
        if (HasExplicitRestSiteChoiceAuthority(request))
        {
            return ("rest-site", "map");
        }

        if (RestSiteObserverSignals.IsRestSiteSmithUpgradeState(request.Observer))
        {
            return (RestSiteObserverSignals.HasSmithConfirmVisible(request.Observer) ? "rest-site-smith-confirm" : "rest-site-smith-grid", "rest-site");
        }

        if (CardSelectionObserverSignals.TryGetState(request.Observer) is { } cardSelectionState)
        {
            return ($"card-selection:{cardSelectionState.ScreenType}", request.Observer.CurrentScreen);
        }

        var eventScene = BuildEventSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath);
        if (eventScene.RewardSubstateActive)
        {
            return ("reward", eventScene.RewardScene.LayerState.MapContextVisible ? "map" : null);
        }

        if (eventScene.MapOverlayState.ForegroundVisible && !eventScene.StrongForegroundChoice)
        {
            return ("map-overlay", eventScene.MapOverlayState.EventBackgroundPresent ? "event" : "map");
        }

        if (eventScene.ExplicitAction == EventExplicitActionKind.AncientDialogue)
        {
            return ("ancient-event-dialogue", "event");
        }

        if (eventScene.ExplicitAction == EventExplicitActionKind.AncientCompletion)
        {
            return ("ancient-event-completion", "event");
        }

        if (eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.ReleasePending)
        {
            return ("event-release-pending", eventScene.MapContextVisible ? "map" : null);
        }

        if (eventScene.ExplicitAction == EventExplicitActionKind.AncientOption)
        {
            return ("ancient-event-options", "event");
        }

        if (eventScene.EventForegroundOwned)
        {
            return ("event", request.SceneSignature.Contains("contamination:map-arrow", StringComparison.OrdinalIgnoreCase) ? "map" : null);
        }

        if (BuildShopSceneState(request.Observer, request.History) is { ReleaseStage: NonCombatReleaseStage.Active } shopScene)
        {
            return (shopScene.ForegroundDebugKind, shopScene.BackgroundDebugKind);
        }

        if (BuildTreasureSceneState(request.Observer) is { } treasureScene)
        {
            return (treasureScene.ForegroundDebugKind, treasureScene.BackgroundDebugKind);
        }

        if (LooksLikeRestSiteState(request.Observer))
        {
            return ("rest-site", request.SceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase) ? "map" : null);
        }

        if (string.Equals(request.Observer.CurrentScreen, "combat", StringComparison.OrdinalIgnoreCase))
        {
            return ("combat", null);
        }

        if (string.Equals(request.Observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase))
        {
            return ("map", null);
        }

        return (request.Observer.CurrentScreen, request.Observer.VisibleScreen);
    }

    private static bool HasExplicitRestSiteChoiceAuthority(GuiSmokeStepRequest request)
    {
        return HasRestSiteAuthority(request.Observer)
               && HasExplicitRestSiteChoiceAffordance(request.Observer)
               && !RestSiteObserverSignals.IsRestSiteSmithUpgradeState(request.Observer)
               && !AutoRestSiteCardGridAnalyzer.Analyze(request.ScreenshotPath).HasSelectableCard;
    }

    private static bool HasRestSiteAuthority(ObserverSummary observer)
    {
        return string.Equals(observer.EncounterKind, "RestSite", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "rest", StringComparison.OrdinalIgnoreCase)
               || RestSiteObserverSignals.IsRestSiteSmithUpgradeState(observer);
    }

    private static string[] BuildExplicitRestSiteCandidateLabels(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.BuildCandidateLabels(observer).ToArray();
    }

    private static bool HasExplicitRestSiteChoiceAffordance(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.HasExplicitRestSiteChoiceAffordance(observer);
    }

    private static bool HasRestSiteRestChoice(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.HasRestSiteRestChoice(observer);
    }

    private static bool HasRestSiteSmithChoice(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.HasRestSiteSmithChoice(observer);
    }

    private static bool HasRestSiteHatchChoice(ObserverSummary observer)
    {
        return RestSiteChoiceSupport.HasRestSiteHatchChoice(observer);
    }

    private static bool IsExplicitRestSiteChoiceLabel(string? label)
    {
        return HasRestSiteRestLabel(label)
               || HasRestSiteSmithLabel(label)
               || HasRestSiteHatchLabel(label);
    }

    private static bool HasRestSiteRestLabel(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("휴식", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Rest", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasRestSiteSmithLabel(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("재련", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Smith", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasRestSiteHatchLabel(string? label)
    {
        return !string.IsNullOrWhiteSpace(label)
               && (label.Contains("부화", StringComparison.OrdinalIgnoreCase)
                   || label.Contains("Hatch", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasStrongForegroundEventChoice(GuiSmokeStepRequest request)
    {
        var eventScene = BuildEventSceneState(request.Observer, request.WindowBounds, request.History, request.ScreenshotPath);
        return eventScene.StrongForegroundChoice;
    }

    private static bool HasExplicitEventProceedAuthority(ObserverState observer, WindowBounds? windowBounds)
    {
        return EventProceedObserverSignals.HasExplicitEventProceedAuthority(observer, windowBounds);
    }

    private static bool HasExplicitEventProceedAuthority(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return EventProceedObserverSignals.HasExplicitEventProceedAuthority(observer, windowBounds);
    }

    private static bool HasEventChoiceAuthority(ObserverSummary observer)
    {
        return string.Equals(observer.CurrentScreen, "event", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.VisibleScreen, "event", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "event", StringComparison.OrdinalIgnoreCase)
               || string.Equals(observer.ChoiceExtractorPath, "room-event", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExplicitEventProceedNode(ObserverActionNode node)
    {
        return node.Actionable
               && ((node.NodeId?.StartsWith("event-option:", StringComparison.OrdinalIgnoreCase) ?? false)
                   || node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase))
               && HasExplicitEventProceedSemantic(node.SemanticHints);
    }

    private static bool IsExplicitEventProceedChoice(ObserverChoice choice)
    {
        return !string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
               && HasExplicitEventProceedSemantic(choice.SemanticHints);
    }

    private static bool HasExplicitEventProceedSemantic(IReadOnlyList<string> semanticHints)
    {
        return semanticHints.Any(static hint =>
            string.Equals(hint, "option-role:proceed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(hint, "event-proceed", StringComparison.OrdinalIgnoreCase));
    }

    private static bool ShouldPrioritizeExplicitEventProgressionAfterCardSelection(ObserverState observer, IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        return HasRecentCardSelectionSubtypeAftermath(history)
               && HasExplicitEventProgressionChoiceVisible(observer, null);
    }

    private static bool ShouldPrioritizeExplicitEventProgressionAfterCardSelection(GuiSmokeStepRequest request)
    {
        return HasRecentCardSelectionSubtypeAftermath(request.History)
               && HasExplicitEventProgressionChoiceVisible(request.Observer, request.WindowBounds);
    }

    private static bool HasRecentCardSelectionSubtypeAftermath(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        static bool IsSubtypeTarget(string? targetLabel)
        {
            return !string.IsNullOrWhiteSpace(targetLabel)
                   && (targetLabel.StartsWith("transform ", StringComparison.OrdinalIgnoreCase)
                       || targetLabel.StartsWith("deck remove ", StringComparison.OrdinalIgnoreCase)
                       || targetLabel.StartsWith("upgrade ", StringComparison.OrdinalIgnoreCase));
        }

        for (var index = history.Count - 1; index >= 0 && index >= history.Count - 6; index -= 1)
        {
            var entry = history[index];
            if (string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase)
                && IsSubtypeTarget(entry.TargetLabel))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasExplicitEventProgressionChoiceVisible(ObserverState observer, WindowBounds? windowBounds)
    {
        var eventScene = BuildEventSceneState(observer, windowBounds);
        return eventScene.EventForegroundOwned
               && eventScene.ReleaseStage == EventReleaseStage.Active
               && eventScene.HasExplicitProgression;
    }

    private static bool HasExplicitEventProgressionChoiceVisible(ObserverSummary observer, WindowBounds? windowBounds)
    {
        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, windowBounds);
        return eventScene.EventForegroundOwned
               && eventScene.ReleaseStage == EventReleaseStage.Active
               && eventScene.HasExplicitProgression;
    }

    internal static bool HasRawExplicitEventChoiceVisible(ObserverSummary observer, WindowBounds? windowBounds)
    {
        static bool IsGenericContinueLabel(string? label)
        {
            return !string.IsNullOrWhiteSpace(label)
                   && (label.Contains("계속", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("Continue", StringComparison.OrdinalIgnoreCase)
                       || label.Contains("Proceed", StringComparison.OrdinalIgnoreCase));
        }

        return observer.ActionNodes.Any(node =>
                   node.Actionable
                   && HasActiveNodeBounds(node.ScreenBounds, windowBounds)
                   && !MapNodeSourceSupport.IsExplicitMapPointNode(node)
                   && !IsGenericContinueLabel(node.Label)
                   && !IsBackChoiceLabel(node.Label)
                   && (node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(node.TypeName, "event-option", StringComparison.OrdinalIgnoreCase)))
               || observer.Choices.Any(choice =>
                   HasActiveNodeBounds(choice.ScreenBounds, windowBounds)
                   && !MapNodeSourceSupport.IsExplicitMapPointChoice(choice)
                   && !IsGenericContinueLabel(choice.Label)
                   && !IsBackChoiceLabel(choice.Label)
                   && (string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(choice.Kind, "event-option", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(choice.BindingKind, "event-option", StringComparison.OrdinalIgnoreCase)));
    }

    internal static bool HasRawEventProgressionSurface(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return observer.ActionNodes.Any(node =>
                   node.Actionable
                   && HasActiveNodeBounds(node.ScreenBounds, windowBounds)
                   && !MapNodeSourceSupport.IsExplicitMapPointNode(node)
                   && ScoreProgressionNode(node) > 0)
               || observer.Choices.Any(choice =>
                   HasActiveNodeBounds(choice.ScreenBounds, windowBounds)
                   && !MapNodeSourceSupport.IsExplicitMapPointChoice(choice)
                   && ScoreProgressionChoice(choice) > 0);
    }

    private static bool MatchesRestSiteTarget(string? label, string targetLabel)
    {
        return RestSiteChoiceSupport.MapOptionIdToTargetLabel(RestSiteChoiceSupport.MapLabelToOptionId(label) ?? string.Empty)
            .Equals(targetLabel, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildRestSiteChoiceReason(string targetLabel, int currentHp, int maxHp)
    {
        return targetLabel switch
        {
            "rest site: rest" => $"Rest site detected from explicit choices. HP {currentHp}/{maxHp} favors healing before routing again.",
            "rest site: hatch" => "Rest site detected from explicit choices. Hatch is visible and no stronger rest/smith lane is available.",
            _ => "Rest site detected from explicit choices. HP is healthy enough to prefer smithing over generic map routing.",
        };
    }

    private static IReadOnlyList<RestSiteDecisionCandidate> BuildRestSiteDecisionCandidates(GuiSmokeStepRequest request)
    {
        var observedChoices = RestSiteChoiceSupport.GetObservedChoices(request.Observer);
        if (observedChoices.Count == 0)
        {
            return Array.Empty<RestSiteDecisionCandidate>();
        }

        var selectedOptionId = RestSiteChoiceSupport.DetermineAutoPickOptionId(request.Observer);
        return observedChoices
            .Select(choice => BuildRestSiteDecisionCandidate(request, choice, selectedOptionId))
            .ToArray();
    }

    private static RestSiteDecisionCandidate BuildRestSiteDecisionCandidate(
        GuiSmokeStepRequest request,
        RestSiteObservedChoice choice,
        string? selectedOptionId)
    {
        var targetLabel = choice.TargetLabel;
        var rawBounds = choice.HasMetadata
            ? choice.Choice?.ScreenBounds
            : choice.Choice?.ScreenBounds ?? choice.ActionNode?.ScreenBounds;
        var boundsSource = choice.HasMetadata
            ? !string.IsNullOrWhiteSpace(choice.Choice?.ScreenBounds) ? "observer-rest-site-choice" : null
            : !string.IsNullOrWhiteSpace(choice.Choice?.ScreenBounds)
                ? "observer-rest-site-choice-legacy"
                : !string.IsNullOrWhiteSpace(choice.ActionNode?.ScreenBounds)
                    ? "observer-rest-site-node-legacy"
                    : "fixed-rest-site-fallback";
        var rejectReason = "no-rest-site-decision";
        GuiSmokeStepDecision? decision = null;

        if (!choice.IsDefaultAutoPick)
        {
            rejectReason = "not-default-auto-pick";
        }
        else if (string.IsNullOrWhiteSpace(selectedOptionId)
                 || !string.Equals(choice.OptionId, selectedOptionId, StringComparison.OrdinalIgnoreCase))
        {
            rejectReason = "not-selected-by-rest-site-policy";
        }
        else if (choice.HasMetadata && !choice.IsEnabled)
        {
            rejectReason = "disabled-explicit-choice";
        }
        else
        {
            var repeatState = GetRestSiteExplicitChoiceRepeatState(request, targetLabel);
            if (repeatState == RestSiteExplicitChoiceRepeatState.NoOpDetected)
            {
                decision = CreateRestSitePostClickNoOpWaitDecision(request, targetLabel, request.Observer.CurrentScreen);
            }
            else if (repeatState == RestSiteExplicitChoiceRepeatState.GraceNeeded)
            {
                decision = CreateRestSiteGraceWaitDecision(targetLabel, request.Observer.CurrentScreen);
            }
            else if (choice.HasMetadata)
            {
                if (string.IsNullOrWhiteSpace(choice.Choice?.ScreenBounds))
                {
                    rejectReason = "missing-hitbox-for-explicit-choice";
                }
                else if (choice.Choice is null || !HasActiveNodeBounds(choice.Choice.ScreenBounds, request.WindowBounds))
                {
                    rejectReason = "stale-hitbox-for-explicit-choice";
                }
                else
                {
                    var currentHp = request.Observer.PlayerCurrentHp ?? 0;
                    var maxHp = request.Observer.PlayerMaxHp ?? 0;
                    decision = CreateClickDecisionFromChoice(
                        request,
                        choice.Choice,
                        targetLabel,
                        BuildRestSiteChoiceReason(targetLabel, currentHp, maxHp),
                        0.92,
                        request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                        1400);
                }
            }
            else
            {
                decision = TryCreateLegacyRestSiteDecision(request, choice);
                rejectReason = decision is null
                    ? "legacy-rest-site-choice-unavailable"
                    : "legacy-rest-site-choice-selected";
            }
        }

        return new RestSiteDecisionCandidate(
            choice.CandidateLabel,
            choice.HasMetadata ? "observer-rest-site-choice" : "legacy-rest-site-choice",
            GetRestSiteCandidateScore(choice.OptionId),
            rejectReason,
            rawBounds,
            boundsSource,
            decision);
    }

    private static GuiSmokeStepDecision? TryCreateLegacyRestSiteDecision(GuiSmokeStepRequest request, RestSiteObservedChoice choice)
    {
        var currentHp = request.Observer.PlayerCurrentHp ?? 0;
        var maxHp = request.Observer.PlayerMaxHp ?? 0;
        if (choice.Choice is not null
            && HasActiveNodeBounds(choice.Choice.ScreenBounds, request.WindowBounds))
        {
            return CreateClickDecisionFromChoice(
                request,
                choice.Choice,
                choice.TargetLabel,
                BuildRestSiteChoiceReason(choice.TargetLabel, currentHp, maxHp),
                0.92,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                1400);
        }

        if (choice.ActionNode is not null
            && choice.ActionNode.Actionable
            && HasActiveNodeBounds(choice.ActionNode.ScreenBounds, request.WindowBounds))
        {
            return CreateClickDecisionFromNode(request, choice.ActionNode, choice.TargetLabel);
        }

        var normalizedX = choice.TargetLabel switch
        {
            "rest site: rest" => 0.405,
            "rest site: smith" => 0.575,
            "rest site: hatch" => 0.745,
            _ => 0.575,
        };
        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            0.305,
            choice.TargetLabel,
            BuildRestSiteChoiceReason(choice.TargetLabel, currentHp, maxHp),
            0.86,
            "map",
            1500,
            true,
            null);
    }

    private static double GetRestSiteCandidateScore(string optionId)
    {
        return RestSiteChoiceSupport.NormalizeOptionId(optionId) switch
        {
            "HEAL" => 1.00d,
            "SMITH" => 0.98d,
            "HATCH" => 0.94d,
            _ => 0.72d,
        };
    }

    private static GuiSmokeStepDecision CreateRestSiteGraceWaitDecision(string targetLabel, string? expectedScreen)
    {
        return new GuiSmokeStepDecision(
            "wait",
            null,
            null,
            null,
            null,
            targetLabel,
            $"Rest-site explicit choice '{targetLabel}' persisted after a click. Allow one recapture before escalating to a no-op diagnosis.",
            0.70d,
            expectedScreen,
            700,
            true,
            null,
            DecisionRisk: "rest-site-post-click-recapture-grace");
    }

    private static GuiSmokeStepDecision CreateRestSitePostClickNoOpWaitDecision(GuiSmokeStepRequest request, string targetLabel, string? expectedScreen)
    {
        var evidence = RestSiteObserverSignals.BuildPostClickEvidence(request.Observer, targetLabel);
        var reason = evidence.Classification switch
        {
            "rest-site-selection-failed" => $"Rest-site explicit choice '{targetLabel}' reported an after-select failure in observer metadata. Stop repeating the click and surface the failed selection directly.",
            "rest-site-grid-not-visible-after-selection" => $"Rest-site explicit choice '{targetLabel}' was accepted for selection, but no smith grid or confirm state became observer-visible after grace recapture.",
            "rest-site-grid-observer-miss" => $"Rest-site smith upgrade screen became runtime-visible, but the observer did not export card or confirm hitboxes. Stop repeating the smith click and surface the observer miss.",
            _ => $"Rest-site explicit choice '{targetLabel}' remained on the same fingerprint after grace recapture. Stop repeating the click and escalate to rest-site-post-click-noop.",
        };
        return new GuiSmokeStepDecision(
            "wait",
            null,
            null,
            null,
            null,
            targetLabel,
            reason,
            0.72d,
            expectedScreen,
            250,
            true,
            null,
            DecisionRisk: evidence.Classification);
    }

    private static RestSiteExplicitChoiceRepeatState GetRestSiteExplicitChoiceRepeatState(GuiSmokeStepRequest request, string targetLabel)
    {
        if (!HasExplicitRestSiteChoiceAuthority(request)
            || !IsExplicitRestSiteOptionTarget(targetLabel))
        {
            return RestSiteExplicitChoiceRepeatState.None;
        }

        var fingerprint = RestSiteChoiceSupport.BuildExplicitChoiceFingerprint(request.Observer);
        for (var index = request.History.Count - 1; index >= 0; index -= 1)
        {
            var entry = request.History[index];
            if (TryParseRestSiteActionMetadata(entry.Metadata, out var metadata))
            {
                if (!string.Equals(metadata.TargetLabel, targetLabel, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(metadata.Fingerprint, fingerprint, StringComparison.Ordinal))
                {
                    return RestSiteExplicitChoiceRepeatState.None;
                }

                return string.Equals(metadata.Kind, "explicit-grace-wait", StringComparison.OrdinalIgnoreCase)
                    ? RestSiteExplicitChoiceRepeatState.NoOpDetected
                    : string.Equals(metadata.Kind, "explicit-click", StringComparison.OrdinalIgnoreCase)
                        ? RestSiteExplicitChoiceRepeatState.GraceNeeded
                        : RestSiteExplicitChoiceRepeatState.None;
            }

            if (entry.Action is "click" or "click-current" or "confirm-non-enemy" or "right-click" or "press-key")
            {
                return RestSiteExplicitChoiceRepeatState.None;
            }
        }

        return RestSiteExplicitChoiceRepeatState.None;
    }

    private static bool HasRecentRestSiteExplicitClick(GuiSmokeStepRequest request, string? targetLabel)
    {
        if (!IsExplicitRestSiteOptionTarget(targetLabel))
        {
            return false;
        }

        for (var index = request.History.Count - 1; index >= 0; index -= 1)
        {
            var entry = request.History[index];
            if (!TryParseRestSiteActionMetadata(entry.Metadata, out var metadata))
            {
                continue;
            }

            return string.Equals(metadata.Kind, "explicit-click", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(metadata.TargetLabel, targetLabel, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool IsExplicitRestSiteOptionTarget(string? targetLabel)
    {
        return string.Equals(targetLabel, "rest site: rest", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: smith", StringComparison.OrdinalIgnoreCase)
               || string.Equals(targetLabel, "rest site: hatch", StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(targetLabel)
                   && targetLabel.StartsWith("rest site: option:", StringComparison.OrdinalIgnoreCase));
    }

    private static string? BuildRestSiteHistoryMetadata(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        if (!HasExplicitRestSiteChoiceAuthority(request)
            || !IsExplicitRestSiteOptionTarget(decision.TargetLabel))
        {
            return null;
        }

        var kind = string.Equals(decision.Status, "act", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(decision.ActionKind, "click", StringComparison.OrdinalIgnoreCase)
            ? "explicit-click"
            : string.Equals(decision.Status, "wait", StringComparison.OrdinalIgnoreCase)
              && string.Equals(decision.DecisionRisk, "rest-site-post-click-recapture-grace", StringComparison.OrdinalIgnoreCase)
                ? "explicit-grace-wait"
                : null;
        if (kind is null)
        {
            return null;
        }

        return JsonSerializer.Serialize(
            new RestSiteActionMetadata(
                kind,
                decision.TargetLabel!,
                RestSiteChoiceSupport.BuildExplicitChoiceFingerprint(request.Observer)),
            GuiSmokeShared.JsonOptions);
    }

    private static bool TryParseRestSiteActionMetadata(string? metadata, out RestSiteActionMetadata parsed)
    {
        parsed = default!;
        if (string.IsNullOrWhiteSpace(metadata))
        {
            return false;
        }

        try
        {
            parsed = JsonSerializer.Deserialize<RestSiteActionMetadata>(metadata, GuiSmokeShared.JsonOptions)!;
            return parsed is not null
                   && !string.IsNullOrWhiteSpace(parsed.Kind)
                   && !string.IsNullOrWhiteSpace(parsed.TargetLabel)
                   && !string.IsNullOrWhiteSpace(parsed.Fingerprint);
        }
        catch
        {
            return false;
        }
    }

    public static string? BuildRestSiteHistoryMetadataForDecision(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        return BuildRestSiteHistoryMetadata(request, decision);
    }

    public static bool HasRecentRestSiteExplicitClickForRequest(GuiSmokeStepRequest request, string? targetLabel)
    {
        return HasRecentRestSiteExplicitClick(request, targetLabel);
    }

    public static bool HasExplicitRestSiteChoiceAuthorityForRequest(GuiSmokeStepRequest request)
    {
        return HasExplicitRestSiteChoiceAuthority(request);
    }

    public static bool IsExplicitRestSiteOptionTargetLabel(string? targetLabel)
    {
        return IsExplicitRestSiteOptionTarget(targetLabel);
    }

    private static string ToCandidateLabel(GuiSmokeStepDecision? decision, string fallback)
    {
        return decision?.TargetLabel switch
        {
            { } target when target.Contains("reward", StringComparison.OrdinalIgnoreCase) => "click reward choice",
            { } target when target.Contains("ancient dialogue", StringComparison.OrdinalIgnoreCase) => "click ancient dialogue advance",
            { } target when target.Contains("ancient event completion", StringComparison.OrdinalIgnoreCase) => "click ancient event completion",
            { } target when target.Contains("event", StringComparison.OrdinalIgnoreCase) => "click event choice",
            { } target when target.Contains("rest site:", StringComparison.OrdinalIgnoreCase) => "click rest site choice",
            { } target when target.Contains("smith card", StringComparison.OrdinalIgnoreCase) => "click smith card",
            { } target when target.Contains("smith confirm", StringComparison.OrdinalIgnoreCase) => "click smith confirm",
            { } target when target.Contains("map back", StringComparison.OrdinalIgnoreCase) => "click map back",
            { } target when target.Contains("exported reachable map node", StringComparison.OrdinalIgnoreCase) => "click exported reachable node",
            { } target when target.Contains("visible map advance", StringComparison.OrdinalIgnoreCase) => "click visible map advance",
            _ => fallback,
        };
    }

    private static bool AllowsAction(GuiSmokeStepRequest request, string action)
    {
        return request.AllowedActions.Contains(action, StringComparer.OrdinalIgnoreCase);
    }

    private static string? TryFindMapNodeBounds(GuiSmokeStepRequest request)
    {
        return request.Observer.Choices.FirstOrDefault(choice =>
                   MapNodeSourceSupport.IsExplicitMapPointChoice(choice)
                   && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds))?.ScreenBounds
               ?? request.Observer.ActionNodes.FirstOrDefault(node =>
                   node.Actionable
                   && MapNodeSourceSupport.IsExplicitMapPointNode(node)
                   && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds))?.ScreenBounds;
    }

    private static string? TryFindBackBounds(GuiSmokeStepRequest request)
    {
        return request.Observer.ActionNodes.FirstOrDefault(node =>
                   node.Actionable
                   && IsBackNode(node)
                   && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds))?.ScreenBounds
               ?? request.Observer.Choices.FirstOrDefault(choice =>
                   IsBackChoiceLabel(choice.Label)
                   && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds))?.ScreenBounds;
    }

    private static string? TryFindEventChoiceBounds(GuiSmokeStepRequest request)
    {
        return AncientEventObserverSignals.GetActiveDialogueNode(request.Observer, request.WindowBounds)?.ScreenBounds
               ?? AncientEventObserverSignals.GetActiveDialogueChoice(request.Observer, request.WindowBounds)?.ScreenBounds
               ?? AncientEventObserverSignals.GetActiveCompletionNode(request.Observer, request.WindowBounds)?.ScreenBounds
               ?? AncientEventObserverSignals.GetActiveCompletionChoice(request.Observer, request.WindowBounds)?.ScreenBounds
               ?? AncientEventObserverSignals.GetActiveOptionNodes(request.Observer, request.WindowBounds).FirstOrDefault()?.ScreenBounds
               ?? AncientEventObserverSignals.GetActiveOptionChoices(request.Observer, request.WindowBounds).FirstOrDefault()?.ScreenBounds
               ?? request.Observer.ActionNodes.FirstOrDefault(node =>
                   node.Actionable
                   && node.Kind.Contains("event-option", StringComparison.OrdinalIgnoreCase)
                   && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds))?.ScreenBounds
               ?? request.Observer.Choices.FirstOrDefault(choice =>
                   string.Equals(choice.Kind, "choice", StringComparison.OrdinalIgnoreCase)
                   && !IsBackChoiceLabel(choice.Label)
                   && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds))?.ScreenBounds;
    }

    private static string? TryFindProceedBounds(GuiSmokeStepRequest request)
    {
        return request.Observer.ActionNodes.FirstOrDefault(node =>
                   node.Actionable
                   && IsProceedNode(node)
                   && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds))?.ScreenBounds
               ?? request.Observer.Choices.FirstOrDefault(choice =>
                   IsProceedChoice(choice)
                   && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds))?.ScreenBounds;
    }

    private static string? TryFindRewardBounds(GuiSmokeStepRequest request)
    {
        return request.Observer.ActionNodes.FirstOrDefault(node =>
                   node.Actionable
                   && IsExplicitRewardProgressionNode(node)
                   && HasActiveRewardBounds(node.ScreenBounds, request.WindowBounds))?.ScreenBounds
               ?? request.Observer.Choices.FirstOrDefault(choice =>
                   IsExplicitRewardProgressionChoice(choice)
                   && HasActiveRewardBounds(choice.ScreenBounds, request.WindowBounds))?.ScreenBounds;
    }

    private static GuiSmokeStepDecision? TryFindActionNodeDecision(GuiSmokeStepRequest request, string contains, string targetLabel)
    {
        var node = request.Observer.ActionNodes.FirstOrDefault(candidate =>
            candidate.Actionable &&
            candidate.Label.Contains(contains, StringComparison.OrdinalIgnoreCase));
        return node is null ? null : CreateClickDecisionFromNode(request, node, targetLabel);
    }

    private static GuiSmokeStepDecision? TryFindFirstReachableMapNodeDecision(GuiSmokeStepRequest request)
    {
        if (IsMapFallbackBlockedByForegroundAuthority(request)
            || !GuiSmokeNonCombatContractSupport.AllowsAction(request, "click first reachable node"))
        {
            return null;
        }

        var analysis = AutoMapAnalyzer.Analyze(request.ScreenshotPath);
        if (analysis.HasReachableNode)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                analysis.ReachableNodeNormalizedX,
                analysis.ReachableNodeNormalizedY,
                "visible reachable node",
                "The screenshot shows the current map arrow and a connected reachable node. Click the reachable node directly.",
                0.90,
                "map",
                1500,
                true,
                null);
        }

        return null;
    }

    private static GuiSmokeStepDecision? TryCreateExportedReachableMapPointDecision(GuiSmokeStepRequest request)
    {
        if (IsMapFallbackBlockedByForegroundAuthority(request)
            || !GuiSmokeNonCombatContractSupport.AllowsAction(request, "click exported reachable node"))
        {
            return null;
        }

        var choice = request.Observer.Choices
            .Where(choice =>
                MapNodeSourceSupport.IsExplicitMapPointChoice(choice)
                && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds))
            .OrderBy(GetChoiceSortY)
            .ThenBy(GetChoiceSortX)
            .FirstOrDefault();
        if (choice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                choice,
                "exported reachable map node",
                $"Use the exported reachable map point '{choice.Label}' instead of clicking the red current-node arrow.",
                0.95,
                "map",
                1400);
        }

        var node = request.Observer.ActionNodes
            .Where(node =>
                node.Actionable
                && MapNodeSourceSupport.IsExplicitMapPointNode(node)
                && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds))
            .OrderBy(GetNodeSortY)
            .ThenBy(GetNodeSortX)
            .FirstOrDefault();
        return node is null ? null : CreateClickDecisionFromNode(request, node, "exported reachable map node");
    }

    private static GuiSmokeStepDecision? TryCreateMapBackNavigationDecision(GuiSmokeStepRequest request)
    {
        if (IsMapFallbackBlockedByForegroundAuthority(request)
            || !GuiSmokeNonCombatContractSupport.AllowsAction(request, "click map back"))
        {
            return null;
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (!mapOverlayState.ForegroundVisible || !mapOverlayState.MapBackNavigationAvailable)
        {
            return null;
        }

        var backNode = request.Observer.ActionNodes.FirstOrDefault(node => node.Actionable && IsBackNode(node) && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds));
        if (backNode is not null)
        {
            return CreateClickDecisionFromNode(request, backNode, "map back");
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            0.045,
            0.905,
            "map back",
            "Map overlay foreground is visible and the bottom-left red back arrow can return to the underlying event context.",
            0.86,
            "map",
            1200,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateMapOverlayForegroundDecision(GuiSmokeStepRequest request)
    {
        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (!mapOverlayState.ForegroundVisible)
        {
            return null;
        }

        return TryCreateExportedReachableMapPointDecision(request)
               ?? TryCreateMapBackNavigationDecision(request)
               ?? TryFindFirstReachableMapNodeDecision(request);
    }

    private static GuiSmokeStepDecision? TryCreateSemanticEventDecision(GuiSmokeStepRequest request)
    {
        if (!string.Equals(request.ReasoningMode, "semantic", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var preferredLabels = request.EventKnowledgeCandidates
            .SelectMany(static candidate => candidate.Options)
            .OrderByDescending(static option => ScoreSemanticEventOption(option.Label, option.Description))
            .Select(static option => option.Label)
            .ToArray();
        foreach (var label in preferredLabels)
        {
            var choice = request.Observer.Choices.FirstOrDefault(candidate =>
                string.Equals(candidate.Label, label, StringComparison.OrdinalIgnoreCase)
                && TryParseNodeBounds(candidate.ScreenBounds, out _));
            if (choice is null || !TryParseNodeBounds(choice.ScreenBounds, out var bounds))
            {
                continue;
            }

            var centerX = bounds.X + bounds.Width / 2f;
            var centerY = bounds.Y + bounds.Height / 2f;
            var leadingCandidate = request.EventKnowledgeCandidates.FirstOrDefault(candidate =>
                candidate.Options.Any(option => string.Equals(option.Label, label, StringComparison.OrdinalIgnoreCase)));
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                Math.Clamp(centerX / 1920f, 0d, 1d),
                Math.Clamp(centerY / 1080f, 0d, 1d),
                $"semantic event option: {label}",
                $"Semantic event reasoning selected '{label}' from {leadingCandidate?.Title ?? "event knowledge"}.",
                0.86,
                "event",
                1400,
                true,
                null,
                leadingCandidate is null ? "semantic event match" : $"event candidate: {leadingCandidate.Title}",
                "event option changes page, grants reward, or advances room flow",
                request.DecisionRiskHint);
        }

        return null;
    }

    private static int ScoreSemanticEventOption(string? label, string? description)
    {
        var score = 0;
        if (ContainsAny(label, "떠", "leave", "continue", "확인", "proceed", "take"))
        {
            score += 5;
        }

        if (ContainsAny(description, "획득", "gain", "얻", "heal", "회복", "gold", "카드"))
        {
            score += 3;
        }

        if (ContainsAny(description, "잃", "lose", "피해", "damage", "hp", "wound", "부상"))
        {
            score -= 4;
        }

        return score;
    }

    private static GuiSmokeStepDecision? TryCreateTreasureRoomDecision(GuiSmokeStepRequest request)
    {
        var treasureState = TreasureRoomObserverSignals.TryGetState(request.Observer);
        if (treasureState is null || !treasureState.RoomDetected)
        {
            return null;
        }

        if (treasureState.InspectOverlayVisible)
        {
            var overlayBackChoice = TreasureRoomObserverSignals.TryGetOverlayBackChoice(request.Observer);
            if (overlayBackChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    overlayBackChoice,
                    "treasure overlay back",
                    "Treasure inspect overlay is foreground-visible. Close it before retrying any chest, relic-holder, or proceed action.",
                    0.98,
                    request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                    1200);
            }

            return null;
        }

        if (!treasureState.ChestOpened && treasureState.ChestClickable)
        {
            var chestChoice = TreasureRoomObserverSignals.TryGetChestChoice(request.Observer);
            if (chestChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    chestChoice,
                    "treasure chest",
                    "Treasure room authority is active. Open the chest before any relic holder or proceed action.",
                    0.98,
                    request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                    1500);
            }

            var treasureAnalysis = AutoTreasureAnalyzer.Analyze(request.ScreenshotPath);
            if (treasureAnalysis.HasClosedChestHighlight)
            {
                return CreateTreasureChestCenterDecision(request);
            }
        }

        if (treasureState.ProceedEnabled)
        {
            var proceedChoice = TreasureRoomObserverSignals.TryGetProceedChoice(request.Observer);
            if (proceedChoice is not null)
            {
                return CreateClickDecisionFromChoice(
                    request,
                    proceedChoice,
                    "treasure proceed",
                    "Treasure relic picking is finished. Use the explicit treasure proceed affordance before any map routing fallback.",
                    0.96,
                    request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                    1400);
            }
        }

        if (WasRecentTreasureProceedAction(request.History))
        {
            return null;
        }

        var relicHolderChoice = TreasureRoomObserverSignals.GetRelicHolderChoices(request.Observer).FirstOrDefault();
        if (relicHolderChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                relicHolderChoice,
                "treasure relic holder",
                "Treasure chest is open and proceed is not yet authoritative. Click the explicit treasure relic holder, not a top-bar inventory relic icon or map node.",
                0.97,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "map",
                1500);
        }

        return null;
    }

    private static bool WasRecentTreasureProceedAction(IReadOnlyList<GuiSmokeHistoryEntry> history)
    {
        for (var index = history.Count - 1; index >= 0 && index >= history.Count - 5; index -= 1)
        {
            var entry = history[index];
            if (string.Equals(entry.TargetLabel, "treasure proceed", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!string.Equals(entry.Action, "wait", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(entry.Action, "observer-accepted", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(entry.Action, "recapture-required", StringComparison.OrdinalIgnoreCase)
                && !entry.Action.StartsWith("branch-", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return false;
    }

    private static GuiSmokeStepDecision CreateTreasureChestCenterDecision(GuiSmokeStepRequest request)
    {
        var chestChoice = request.Observer.Choices.FirstOrDefault(choice =>
            choice.Label.Contains("Chest", StringComparison.OrdinalIgnoreCase)
            || choice.Label.Contains("\uC0C1\uC790", StringComparison.OrdinalIgnoreCase));
        var normalizedX = 0.500d;
        var normalizedY = 0.560d;
        if (chestChoice is not null
            && TryParseNodeBounds(chestChoice.ScreenBounds, out var chestBounds)
            && HasUsableLogicalBounds(chestChoice.ScreenBounds))
        {
            var centerX = chestBounds.X + chestBounds.Width / 2f;
            var centerY = chestBounds.Y + chestBounds.Height / 2f;
            normalizedX = Math.Clamp(centerX / 1920f, 0.35d, 0.65d);
            normalizedY = Math.Clamp(centerY / 1080f, 0.35d, 0.70d);
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            "treasure chest center",
            "Treasure chest room detected. Open the center chest before trying map routing or proceed actions.",
            0.96,
            "map",
            1500,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateRestSiteUpgradeDecision(GuiSmokeStepRequest request)
    {
        if (!LooksLikeRestSiteState(request.Observer))
        {
            return null;
        }

        var observerConfirmChoice = RestSiteObserverSignals.TryGetSmithConfirmChoice(request.Observer);
        if (observerConfirmChoice is not null
            && observerConfirmChoice.Enabled != false
            && !string.IsNullOrWhiteSpace(observerConfirmChoice.ScreenBounds)
            && HasActiveNodeBounds(observerConfirmChoice.ScreenBounds, request.WindowBounds))
        {
            return CreateClickDecisionFromChoice(
                request,
                observerConfirmChoice,
                "rest site: smith confirm",
                "Rest site smith confirm is runtime-visible. Confirm the selected upgrade instead of repeating the smith option click.",
                0.95,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "upgrade",
                1400);
        }

        var observerSmithCardChoice = RestSiteObserverSignals.TryGetFirstSmithCardChoice(request.Observer);
        if (observerSmithCardChoice is not null
            && observerSmithCardChoice.Enabled != false
            && !string.IsNullOrWhiteSpace(observerSmithCardChoice.ScreenBounds)
            && HasActiveNodeBounds(observerSmithCardChoice.ScreenBounds, request.WindowBounds))
        {
            return CreateClickDecisionFromChoice(
                request,
                observerSmithCardChoice,
                "rest site: smith card",
                "Rest site smith grid is runtime-visible. Select an exported upgrade card hitbox instead of relying on screenshot-only inference.",
                0.94,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "upgrade",
                1400);
        }

        if (RestSiteObserverSignals.HasExportedSmithUpgradeChoices(request.Observer))
        {
            return null;
        }

        var lastUpgradeAction = request.History
            .Where(entry =>
                string.Equals(entry.TargetLabel, "rest site: smith card", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.TargetLabel, "rest site: smith confirm", StringComparison.OrdinalIgnoreCase))
            .LastOrDefault();
        if (lastUpgradeAction is not null
            && string.Equals(lastUpgradeAction.TargetLabel, "rest site: smith card", StringComparison.OrdinalIgnoreCase))
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.949,
                0.716,
                "rest site: smith confirm",
                "Rest site upgrade comparison is visible. Click the right-side confirm button after selecting the card.",
                0.94,
                "map",
                1400,
                true,
                null);
        }

        var analysis = AutoRestSiteCardGridAnalyzer.Analyze(request.ScreenshotPath);
        if (!analysis.HasSelectableCard)
        {
            return null;
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            analysis.CardNormalizedX,
            analysis.CardNormalizedY,
            "rest site: smith card",
            "Rest site upgrade card grid is visible. Select a visible card before confirming with V.",
            0.91,
            "map",
            1400,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateRestSiteProceedDecision(GuiSmokeStepRequest request)
    {
        if (!GuiSmokeNonCombatContractSupport.AllowsAction(request, "click proceed"))
        {
            return null;
        }

        if (BuildRestSiteSceneState(request.Observer) is not
            {
                ProceedVisible: true,
                ExplicitChoiceVisible: false,
                SmithUpgradeActive: false,
            })
        {
            return null;
        }

        var proceedNode = request.Observer.ActionNodes.FirstOrDefault(node =>
            node.Actionable
            && IsProceedNode(node)
            && HasActiveNodeBounds(node.ScreenBounds, request.WindowBounds));
        if (proceedNode is not null)
        {
            return CreateClickDecisionFromNode(request, proceedNode, "visible proceed");
        }

        var proceedChoice = request.Observer.Choices.FirstOrDefault(choice =>
            IsProceedChoice(choice)
            && HasActiveNodeBounds(choice.ScreenBounds, request.WindowBounds));
        if (proceedChoice is not null)
        {
            return CreateClickDecisionFromChoice(
                request,
                proceedChoice,
                "visible proceed",
                "Rest-site post-selection proceed is runtime-visible. Advance the room flow before attempting any map routing.",
                0.96,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "rest-site",
                1400);
        }

        return null;
    }

    private static string? TryFindRestSiteUpgradeBounds(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        return decision.TargetLabel switch
        {
            { } label when label.Contains("smith confirm", StringComparison.OrdinalIgnoreCase)
                => RestSiteObserverSignals.TryGetSmithConfirmChoice(request.Observer)?.ScreenBounds,
            { } label when label.Contains("smith card", StringComparison.OrdinalIgnoreCase)
                => RestSiteObserverSignals.TryGetFirstSmithCardChoice(request.Observer)?.ScreenBounds,
            _ => null,
        };
    }

    private static string? TryResolveRestSiteUpgradeBoundsSource(GuiSmokeStepRequest request, GuiSmokeStepDecision decision)
    {
        return decision.TargetLabel switch
        {
            { } label when label.Contains("smith confirm", StringComparison.OrdinalIgnoreCase)
                && RestSiteObserverSignals.TryGetSmithConfirmChoice(request.Observer) is not null => "observer-rest-site-smith-confirm",
            { } label when label.Contains("smith card", StringComparison.OrdinalIgnoreCase)
                && RestSiteObserverSignals.TryGetFirstSmithCardChoice(request.Observer) is not null => "observer-rest-site-smith-card",
            _ => "screenshot-rest-site-upgrade",
        };
    }

    private static bool IsMapFallbackBlockedByForegroundAuthority(GuiSmokeStepRequest request)
    {
        if (RewardObserverSignals.IsTerminalRunBoundary(request.Observer)
            || GuiSmokeObserverPhaseHeuristics.LooksLikeCombatState(request.Observer)
            || CardSelectionObserverSignals.TryGetState(request.Observer) is not null)
        {
            return true;
        }

        return NonCombatForegroundOwnership.Resolve(request.Observer) is
            not NonCombatForegroundOwner.Unknown
            and not NonCombatForegroundOwner.Map;
    }

    private static GuiSmokeStepDecision? TryFindVisibleMapAdvanceDecision(GuiSmokeStepRequest request)
    {
        if (IsMapFallbackBlockedByForegroundAuthority(request)
            || !GuiSmokeNonCombatContractSupport.AllowsAction(request, "click visible map advance"))
        {
            return null;
        }

        if (BuildRewardMapLayerState(request.Observer, request.WindowBounds).RewardPanelVisible)
        {
            return null;
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        if (mapOverlayState.ForegroundVisible || mapOverlayState.ExportedReachableNodeCandidatePresent)
        {
            return null;
        }

        var attempt = request.History.Count(entry =>
            string.Equals(entry.Action, "click", StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.TargetLabel, "visible map advance", StringComparison.OrdinalIgnoreCase));

        var analysis = AutoMapAnalyzer.Analyze(request.ScreenshotPath);
        if (!analysis.HasCurrentArrow)
        {
            return null;
        }

        var offset = attempt switch
        {
            0 => new PointF(0f, -0.105f),
            1 => new PointF(-0.060f, -0.110f),
            2 => new PointF(0.060f, -0.110f),
            _ => new PointF(0f, -0.135f),
        };

        var normalizedX = Math.Clamp(analysis.ArrowNormalizedX + offset.X, 0.08f, 0.92f);
        var normalizedY = Math.Clamp(analysis.ArrowNormalizedY + offset.Y, 0.10f, 0.86f);
        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            normalizedX,
            normalizedY,
            "visible map advance",
            $"Visible map is present while logical flow remains '{request.Observer.CurrentScreen}'. Advance from the red current-node arrow using screenshot-derived positioning (attempt {attempt + 1}).",
            0.78,
            "map",
            1500,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateRestSiteDecision(GuiSmokeStepRequest request)
    {
        return BuildRestSiteDecisionCandidates(request)
            .Select(static candidate => candidate.Decision)
            .FirstOrDefault(static decision => decision is not null);
    }

    private static GuiSmokeStepDecision? TryCreateHiddenOverlayCleanupDecision(GuiSmokeStepRequest request)
    {
        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(request.ScreenshotPath);
        if (overlayAnalysis.HasCentralOverlayPanel)
        {
            return null;
        }

        var choices = request.Observer.CurrentChoices;
        var hasOverlayChoices = choices.Any(static label =>
            label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
            || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
            || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
        if (!hasOverlayChoices)
        {
            return null;
        }

        var cleanupAttempts = request.History.Count(entry =>
            string.Equals(entry.TargetLabel, "hidden overlay close", StringComparison.OrdinalIgnoreCase));
        if (cleanupAttempts >= 2)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.180,
                0.520,
                "hidden overlay close",
                "Overlay controls remain active in the room state even though no central panel is visible. Retry backdrop dismissal before progressing.",
                0.72,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1200,
                true,
                null);
        }

        return new GuiSmokeStepDecision(
            "act",
            "press-key",
            "Escape",
            null,
            null,
            "hidden overlay close",
            "Overlay controls remain active behind the visible room. Send escape/cancel before trying to progress.",
            0.84,
            request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
            1000,
            true,
            null);
    }

    private static GuiSmokeStepDecision? TryCreateOverlayAdvanceDecision(GuiSmokeStepRequest request)
    {
        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(request.ScreenshotPath);
        if (!overlayAnalysis.HasCentralOverlayPanel)
        {
            return null;
        }

        var choices = request.Observer.CurrentChoices;
        var hasBackstop = choices.Any(static label => label.Contains("Backstop", StringComparison.OrdinalIgnoreCase));
        var hasLeftArrow = choices.Any(static label => label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase));
        var hasRightArrow = choices.Any(static label => label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
        if (!hasBackstop && !hasLeftArrow && !hasRightArrow)
        {
            return null;
        }

        var recentOverlayBackCount = CountRecentOverlayCleanupAttempts(request.History);

        if (recentOverlayBackCount >= 3)
        {
            return new GuiSmokeStepDecision(
                "act",
                "press-key",
                "Escape",
                null,
                null,
                "overlay close",
                "Overlay remains open after repeated dismiss attempts. Send escape before trying to progress again.",
                0.72,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1200,
                true,
                null);
        }

        if (overlayAnalysis.HasBottomLeftBackArrow)
        {
            return new GuiSmokeStepDecision(
                "act",
                "click",
                null,
                0.045,
                0.905,
                "overlay back",
                "An inspect overlay is present above the room flow. Close it via the visible back arrow before trying to progress.",
                0.93,
                request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
                1200,
                true,
                null);
        }

        return new GuiSmokeStepDecision(
            "act",
            "click",
            null,
            0.180,
            0.520,
            "overlay backdrop close",
            "A centered inspect overlay is visible without a dedicated back arrow. Click the dark backdrop to dismiss it before trying to progress.",
            0.88,
            request.Observer.CurrentScreen ?? request.Observer.VisibleScreen ?? "event",
            1200,
            true,
            null);
    }

    private static bool IsProceedNode(ObserverActionNode node)
    {
        return node.Label.Contains("\uC9C4\uD589", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("\uB118\uAE30\uAE30", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Proceed", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Continue", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsProceedChoice(ObserverChoice choice)
    {
        return !string.Equals(choice.Kind, "relic", StringComparison.OrdinalIgnoreCase)
               && !string.Equals(choice.Kind, "map-node", StringComparison.OrdinalIgnoreCase)
               && IsProceedLikeLabel(choice.Label);
    }

    private static bool IsBackNode(ObserverActionNode node)
    {
        return node.Label.Contains("\uB4A4\uB85C", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Back", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMapNode(ObserverActionNode node)
    {
        return node.Kind.Contains("map", StringComparison.OrdinalIgnoreCase)
               || node.NodeId.Contains("map", StringComparison.OrdinalIgnoreCase)
               || node.Label.Contains("Map", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeRestSiteState(ObserverSummary observer)
    {
        return GuiSmokeNonCombatContractSupport.LooksLikeRestSiteState(observer);
    }

    private static bool LooksLikeMapTransitionState(GuiSmokeStepRequest request)
    {
        if (IsMapFallbackBlockedByForegroundAuthority(request))
        {
            return false;
        }

        var mapOverlayState = GuiSmokeMapOverlayHeuristics.BuildState(request.Observer, request.WindowBounds, request.ScreenshotPath);
        return mapOverlayState.ForegroundVisible
               || string.Equals(request.Observer.CurrentScreen, "map", StringComparison.OrdinalIgnoreCase)
               || string.Equals(request.Observer.VisibleScreen, "map", StringComparison.OrdinalIgnoreCase)
               || request.SceneSignature.Contains("substate:map-transition", StringComparison.OrdinalIgnoreCase)
               || request.SceneSignature.Contains("layer:map-background", StringComparison.OrdinalIgnoreCase)
               || request.SceneSignature.Contains("visible:map-arrow", StringComparison.OrdinalIgnoreCase)
               || request.Observer.LastEventsTail.Any(eventTail =>
                   eventTail.Contains("screen-changed: map", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("map-point-selected", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("\"screen\":\"map\"", StringComparison.OrdinalIgnoreCase)
                   || eventTail.Contains("\"currentScreen\":\"map\"", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsBackChoiceLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        return label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
               || label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
               || label.Contains("\uB4A4\uB85C", StringComparison.OrdinalIgnoreCase)
               || label.Contains("Back", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasOverlayChoiceState(ObserverSummary observer)
    {
        return observer.CurrentChoices.Any(static label =>
            label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
            || label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
            || label.Contains("RightArrow", StringComparison.OrdinalIgnoreCase));
    }
}
