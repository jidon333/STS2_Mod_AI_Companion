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
using static GuiSmokeNonCombatAllowedActionSupport;
using static ObserverScreenProvenance;

internal static partial class Program
{
    static string ComputeSceneSignature(string screenshotPath, ObserverState observer, GuiSmokePhase phase)
    {
        return ComputeSceneSignatureCore(screenshotPath, observer, phase, null);
    }

    static string ComputeSceneSignatureCore(string screenshotPath, ObserverState observer, GuiSmokePhase phase, GuiSmokeStepAnalysisContext? analysisContext)
    {
        var context = analysisContext ?? CreateStepAnalysisContext(phase, observer, screenshotPath, Array.Empty<GuiSmokeHistoryEntry>(), Array.Empty<CombatCardKnowledgeHint>());
        if (context.UseCombatFastPath)
        {
            var combatRuntimeState = context.RuntimeCombatState;
            var combatAnalysis = context.CombatAnalysis;
            var combatCompatibilityCurrentScreen = CompatibilityCurrentScreen(observer) ?? "unknown";
            var combatCompatibilityVisibleScreen = CompatibilityVisibleScreen(observer) ?? "unknown";
            var combatTags = new List<string>(capacity: 10)
            {
                $"phase:{phase.ToString().ToLowerInvariant()}",
                $"screen:{combatCompatibilityCurrentScreen.Trim().ToLowerInvariant()}",
                $"visible:{combatCompatibilityVisibleScreen.Trim().ToLowerInvariant()}",
                $"encounter:{(observer.EncounterKind ?? "none").Trim().ToLowerInvariant()}",
                $"ready:{(CompatibilitySceneReady(observer)?.ToString() ?? "unknown").ToLowerInvariant()}",
                $"stability:{(CompatibilitySceneStability(observer) ?? "unknown").Trim().ToLowerInvariant()}",
                "combat:fast-path",
                $"combat-targeting:{((combatRuntimeState.TargetingInProgress == true || combatAnalysis.HasTargetArrow) ? "active" : "inactive")}",
                $"combat-hittable:{(combatRuntimeState.HittableEnemyCount?.ToString(CultureInfo.InvariantCulture) ?? "unknown")}",
            };

            if (context.PendingCombatSelection is { } pendingSelection)
            {
                combatTags.Add($"combat-selection:{pendingSelection.Kind.ToString().ToLowerInvariant()}");
                combatTags.Add($"combat-slot:{pendingSelection.SlotIndex.ToString(CultureInfo.InvariantCulture)}");
            }
            else if (combatAnalysis.HasSelectedCard)
            {
                combatTags.Add($"combat-selection:{combatAnalysis.SelectedCardKind.ToString().ToLowerInvariant()}");
            }

            if (combatRuntimeState.KeepsCardPlayOpen)
            {
                combatTags.Add("combat-play-open");
            }
            return string.Join("|", combatTags);
        }

        if (phase == GuiSmokePhase.HandleRewards && context.UseRewardFastPath)
        {
            return ComputeRewardFastPathSceneSignature(observer, context);
        }

        var cardSelectionState = CardSelectionObserverSignals.TryGetState(observer.Summary);
        var rewardMapLayer = context.RewardMapLayerState;
        var rewardBackNavigationAvailable = context.RewardBackNavigationAvailable;
        var claimableRewardPresent = context.ClaimableRewardPresent;
        var mapOverlayState = context.MapOverlayState;
        var rewardScene = context.RewardScene;
        var eventScene = context.EventScene;
        var canonicalScene = context.CanonicalNonCombatScene;
        var compatibilityCurrentScreen = CompatibilityCurrentScreen(observer) ?? "unknown";
        var compatibilityVisibleScreen = CompatibilityVisibleScreen(observer) ?? "unknown";
        var suppressMapTransitionByForegroundAuthority = canonicalScene is
            {
                CanonicalForegroundOwner: not NonCombatCanonicalForegroundOwner.Unknown
                    and not NonCombatCanonicalForegroundOwner.Map,
            };
        var tags = new List<string>(capacity: 10)
        {
            $"phase:{phase.ToString().ToLowerInvariant()}",
            $"screen:{compatibilityCurrentScreen.Trim().ToLowerInvariant()}",
            $"visible:{compatibilityVisibleScreen.Trim().ToLowerInvariant()}",
            $"encounter:{(observer.EncounterKind ?? "none").Trim().ToLowerInvariant()}",
            $"ready:{(CompatibilitySceneReady(observer)?.ToString() ?? "unknown").ToLowerInvariant()}",
            $"stability:{(CompatibilitySceneStability(observer) ?? "unknown").Trim().ToLowerInvariant()}",
        };

        if (cardSelectionState is not null)
        {
            tags.Add($"card-selection:{cardSelectionState.ScreenType}");
            tags.Add($"card-selection-selected:{cardSelectionState.SelectedCount.ToString(CultureInfo.InvariantCulture)}");
            if (cardSelectionState.PreviewVisible)
            {
                tags.Add($"card-selection-preview:{cardSelectionState.PreviewMode ?? "visible"}");
            }
        }

        if (TreasureRoomObserverSignals.LooksLikeTreasureState(observer.Summary))
        {
            tags.Add("room:treasure");
        }

        if (GuiSmokeNonCombatContractSupport.LooksLikeRestSiteState(observer.Summary))
        {
            tags.Add("room:rest-site");
        }

        var shopState = ShopObserverSignals.TryGetState(observer.Summary);
        if (shopState is { ForegroundOwned: true })
        {
            tags.Add("room:shop");
        }
        else if (shopState is { RoomVisible: true })
        {
            tags.Add("room-visible:shop");
        }

        if (!ShouldSuppressRoomSubstateHeuristics(phase, observer))
        {
            if (GuiSmokeNonCombatContractSupport.HasExplicitRestSiteChoiceAuthority(observer, screenshotPath))
            {
                tags.Add("layer:rest-site-foreground");
            }

            if (rewardMapLayer.RewardForegroundOwned)
            {
                tags.Add("layer:reward-foreground");
            }
            else if (rewardMapLayer.RewardTeardownInProgress)
            {
                tags.Add("layer:reward-teardown");
            }

            if (rewardMapLayer.MapContextVisible)
            {
                tags.Add("layer:map-background");
            }

            if (mapOverlayState.ForegroundVisible)
            {
                tags.Add("layer:map-overlay-foreground");
            }

            if (mapOverlayState.EventBackgroundPresent)
            {
                tags.Add("layer:event-background");
            }

            if (rewardBackNavigationAvailable)
            {
                tags.Add("layer:reward-back-nav");
            }

            if (rewardMapLayer.StaleRewardChoicePresent)
            {
                tags.Add("stale:reward-choice");
            }

            if (rewardMapLayer.StaleRewardBoundsPresent)
            {
                tags.Add("stale:reward-bounds");
            }

            if (mapOverlayState.StaleEventChoicePresent)
            {
                tags.Add("stale:event-choice");
            }

            if (claimableRewardPresent)
            {
                tags.Add("reward:claimable");
            }

            if (mapOverlayState.MapBackNavigationAvailable)
            {
                tags.Add("map-back-navigation-available");
            }

            if (mapOverlayState.CurrentNodeArrowVisible)
            {
                tags.Add("current-node-arrow-visible");
            }

            if (mapOverlayState.ReachableNodeCandidatePresent)
            {
                tags.Add("reachable-node-candidate-present");
            }

            if (mapOverlayState.ExportedReachableNodeCandidatePresent)
            {
                tags.Add("exported-reachable-node-present");
            }

            if (LooksLikeInspectOverlayState(observer))
            {
                tags.Add("substate:inspect-overlay");
            }

            if (GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer))
            {
                tags.Add("substate:reward-choice");
            }

            if (GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer))
            {
                tags.Add("substate:colorless-card-choice");
            }

            if (!mapOverlayState.ForegroundVisible
                && GuiSmokeNonCombatContractSupport.HasStrongMapTransitionEvidence(observer)
                && !MatchesCompatibilityScreen(observer, "map")
                && !suppressMapTransitionByForegroundAuthority)
            {
                tags.Add("substate:map-transition");
            }

            var mapAnalysis = AutoMapAnalyzer.Analyze(screenshotPath);
            if (mapAnalysis.HasCurrentArrow)
            {
                if (!mapOverlayState.ForegroundVisible)
                {
                    tags.Add(suppressMapTransitionByForegroundAuthority
                        ? "contamination:map-arrow"
                        : "visible:map-arrow");
                }
            }

            if (eventScene.EventForegroundOwned && eventScene.ReleaseStage == EventReleaseStage.Active)
            {
                tags.Add("layer:event-foreground");
            }
        }

        if (rewardMapLayer.TerminalRunBoundary)
        {
            tags.Add("terminal-run-boundary");
        }

        var screenshotFingerprint = ComputeFileFingerprint(screenshotPath);
        tags.Add($"shot:{screenshotFingerprint[..Math.Min(12, screenshotFingerprint.Length)]}");
        return string.Join("|", tags);
    }

    static string ComputeRewardFastPathSceneSignature(ObserverState observer, GuiSmokeStepAnalysisContext context)
    {
        var rewardState = RewardObserverSignals.TryGetState(observer.Summary);
        var cardSelectionState = CardSelectionObserverSignals.TryGetState(observer.Summary);
        var compatibilityCurrentScreen = CompatibilityCurrentScreen(observer) ?? "unknown";
        var compatibilityVisibleScreen = CompatibilityVisibleScreen(observer) ?? "unknown";
        var rewardTags = new List<string>(capacity: 12)
        {
            "phase:handlerewards",
            $"screen:{compatibilityCurrentScreen.Trim().ToLowerInvariant()}",
            $"visible:{compatibilityVisibleScreen.Trim().ToLowerInvariant()}",
            $"encounter:{(observer.EncounterKind ?? "none").Trim().ToLowerInvariant()}",
            $"ready:{(CompatibilitySceneReady(observer)?.ToString() ?? "unknown").ToLowerInvariant()}",
            $"stability:{(CompatibilitySceneStability(observer) ?? "unknown").Trim().ToLowerInvariant()}",
            "reward:fast-path",
        };

        if (cardSelectionState is not null)
        {
            rewardTags.Add($"card-selection:{cardSelectionState.ScreenType}");
            rewardTags.Add($"card-selection-selected:{cardSelectionState.SelectedCount.ToString(CultureInfo.InvariantCulture)}");
            if (cardSelectionState.PreviewVisible)
            {
                rewardTags.Add($"card-selection-preview:{cardSelectionState.PreviewMode ?? "visible"}");
            }
        }

        if (rewardState is not null)
        {
            rewardTags.Add(rewardState.ForegroundOwned ? "reward-owner:foreground" : rewardState.TeardownInProgress ? "reward-owner:teardown" : "reward-owner:visible");
            if (rewardState.RewardIsCurrentActiveScreen)
            {
                rewardTags.Add("reward-current-active");
            }

            if (rewardState.MapIsCurrentActiveScreen)
            {
                rewardTags.Add("map-current-active");
            }

            if (rewardState.TerminalRunBoundary)
            {
                rewardTags.Add("terminal-run-boundary");
            }

            if (rewardState.ProceedVisible)
            {
                rewardTags.Add(rewardState.ProceedEnabled ? "reward-proceed:enabled" : "reward-proceed:visible");
            }
        }

        if (GuiSmokeNonCombatContractSupport.HasExplicitRewardProgressionAffordance(observer.Summary))
        {
            rewardTags.Add("reward-explicit-progression");
        }

        if (GuiSmokeRewardSceneSignals.LooksLikeColorlessCardChoiceState(observer))
        {
            rewardTags.Add("substate:colorless-card-choice");
        }
        else if (GuiSmokeRewardSceneSignals.LooksLikeRewardChoiceState(observer))
        {
            rewardTags.Add("substate:reward-choice");
        }

        return string.Join("|", rewardTags);
    }

    static IReadOnlyList<KnownRecipeHint> LoadKnownRecipes(string sessionRoot, string sceneSignature, string phase)
    {
        var recipesPath = Path.Combine(sessionRoot, "scene-recipes.ndjson");
        if (!File.Exists(recipesPath))
        {
            return Array.Empty<KnownRecipeHint>();
        }

        var hints = new List<KnownRecipeHint>();
        foreach (var line in File.ReadLines(recipesPath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            SceneRecipeEntry? entry;
            try
            {
                entry = JsonSerializer.Deserialize<SceneRecipeEntry>(line, GuiSmokeShared.JsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (entry is null
                || !string.Equals(entry.SceneSignature, sceneSignature, StringComparison.Ordinal)
                || !string.Equals(entry.Phase, phase, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            hints.Add(new KnownRecipeHint(
                entry.SceneSignature,
                entry.Phase,
                entry.ActionKind,
                entry.TargetLabel,
                entry.ExpectedScreen,
                entry.Reason));
        }

        return hints
            .TakeLast(3)
            .ToArray();
    }

    static bool HasSceneSignatureHistory(string sessionRoot, string sceneSignature)
    {
        foreach (var fileName in new[] { "scene-recipes.ndjson", "unknown-scenes.ndjson" })
        {
            var path = Path.Combine(sessionRoot, fileName);
            if (!File.Exists(path))
            {
                continue;
            }

            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains(sceneSignature, StringComparison.Ordinal))
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }

    static string DetermineReasoningMode(GuiSmokePhase phase, ObserverState observer, bool firstSeenScene)
    {
        if (phase != GuiSmokePhase.HandleEvent)
        {
            return "tactical";
        }

        var foregroundOwner = NonCombatForegroundOwnership.Resolve(observer);
        if (foregroundOwner is NonCombatForegroundOwner.Map
            or NonCombatForegroundOwner.Reward
            or NonCombatForegroundOwner.Shop
            or NonCombatForegroundOwner.RestSite)
        {
            return "tactical";
        }

        var eventScene = AutoDecisionProvider.BuildEventSceneState(observer, null);
        var compatibilityCurrentScreen = CompatibilityCurrentScreen(observer);
        var compatibilityVisibleScreen = CompatibilityVisibleScreen(observer);
        if (eventScene.EventForegroundOwned && eventScene.ExplicitAction == EventExplicitActionKind.EventChoice)
        {
            return (firstSeenScene
                    || string.Equals(compatibilityCurrentScreen, "unknown", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(compatibilityVisibleScreen, "unknown", StringComparison.OrdinalIgnoreCase)
                    || observer.Summary.CurrentChoices.Count > 0)
                ? "semantic"
                : "tactical";
        }

        if (AncientEventObserverSignals.HasForegroundAuthority(observer.Summary)
            || EventProceedObserverSignals.HasExplicitEventProceedAuthority(observer, null))
        {
            return "tactical";
        }

        if (firstSeenScene
            || string.Equals(compatibilityCurrentScreen, "unknown", StringComparison.OrdinalIgnoreCase)
            || string.Equals(compatibilityVisibleScreen, "unknown", StringComparison.OrdinalIgnoreCase)
            || observer.Summary.CurrentChoices.Count > 0)
        {
            return "semantic";
        }

        return "tactical";
    }

    static string? BuildSemanticGoal(GuiSmokePhase phase, ObserverState observer, string reasoningMode)
    {
        if (!string.Equals(reasoningMode, "semantic", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return phase switch
        {
            GuiSmokePhase.HandleEvent => TreasureRoomObserverSignals.LooksLikeTreasureState(observer.Summary)
                ? "Interpret the room state, identify whether the chest is unopened, opened, or in reward follow-up, and choose the safest progress action."
                : "Interpret the event screen, infer what each visible option means, and choose a low-risk action that keeps the run progressing.",
            GuiSmokePhase.HandleCombat => "Interpret the combat board, hand, and targets before committing to the next action.",
            _ => "Interpret the current screen semantically before acting.",
        };
    }

    static string? BuildDecisionRiskHint(GuiSmokePhase phase, ObserverState observer, bool firstSeenScene, string reasoningMode)
    {
        var hints = new List<string>();
        if (CompatibilitySceneReady(observer) == false)
        {
            hints.Add("scene-not-ready");
        }

        if (!string.Equals(CompatibilitySceneStability(observer), "stable", StringComparison.OrdinalIgnoreCase))
        {
            hints.Add($"scene-stability:{CompatibilitySceneStability(observer) ?? "unknown"}");
        }

        if (firstSeenScene)
        {
            hints.Add("first-seen-scene");
        }

        if (string.Equals(reasoningMode, "semantic", StringComparison.OrdinalIgnoreCase))
        {
            hints.Add("semantic-scene-ambiguity");
        }

        if (phase == GuiSmokePhase.HandleCombat && observer.PlayerEnergy is null)
        {
            hints.Add("observer-missing-energy");
        }

        return hints.Count == 0
            ? null
            : string.Join(", ", hints);
    }

    static IReadOnlyList<EventKnowledgeCandidate> LoadEventKnowledgeCandidates(string workspaceRoot, ObserverState observer, string reasoningMode)
    {
        if (!string.Equals(reasoningMode, "semantic", StringComparison.OrdinalIgnoreCase))
        {
            return Array.Empty<EventKnowledgeCandidate>();
        }

        var choiceKeys = observer.Summary.CurrentChoices
            .Concat(observer.Summary.Choices.Select(static choice => choice.Label))
            .Where(static label => !string.IsNullOrWhiteSpace(label))
            .Select(NormalizeKnowledgeKey)
            .Where(static label => label.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (choiceKeys.Length == 0)
        {
            return Array.Empty<EventKnowledgeCandidate>();
        }

        var matches = new List<(AssistantEventKnowledge Event, int Score, string Reason)>();
        foreach (var entry in AssistantKnowledgeCatalog.LoadEvents(workspaceRoot))
        {
            var optionKeys = entry.Options
                .Select(static option => NormalizeKnowledgeKey(option.Label))
                .Where(static label => label.Length > 0)
                .ToArray();
            if (optionKeys.Length == 0)
            {
                continue;
            }

            var score = choiceKeys.Count(choiceKey => optionKeys.Contains(choiceKey, StringComparer.Ordinal));
            if (score <= 0)
            {
                continue;
            }

            matches.Add((entry, score, $"matched-options:{score}"));
        }

        return matches
            .OrderByDescending(static match => match.Score)
            .ThenBy(static match => match.Event.Title, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select(match => new EventKnowledgeCandidate(
                match.Event.Id,
                match.Event.Title,
                match.Reason,
                match.Event.Options
                    .Take(5)
                    .Select(static option => new EventOptionKnowledgeCandidate(
                        option.Label,
                        option.Description,
                        option.OptionKey))
                    .ToArray()))
            .ToArray();
    }

    static IReadOnlyList<CombatCardKnowledgeHint> LoadCombatCardKnowledge(string workspaceRoot, ObserverState observer)
    {
        if (observer.CombatHand.Count == 0)
        {
            return Array.Empty<CombatCardKnowledgeHint>();
        }

        var cards = AssistantKnowledgeCatalog.LoadCards(workspaceRoot);
        var hints = new List<CombatCardKnowledgeHint>(observer.CombatHand.Count);
        foreach (var handCard in observer.CombatHand)
        {
            var matchKeys = BuildCombatKnowledgeLookupKeys(handCard.Name);
            if (matchKeys.Count == 0)
            {
                continue;
            }

            var match = cards
                .Select(card => new
                {
                    Card = card,
                    BestMatchLength = matchKeys
                        .Where(key => card.MatchKeys.Contains(key, StringComparer.Ordinal))
                        .DefaultIfEmpty(string.Empty)
                        .Max(static key => key.Length),
                })
                .Where(static entry => entry.BestMatchLength > 0)
                .OrderByDescending(static entry => entry.BestMatchLength)
                .ThenBy(static entry => entry.Card.Id, StringComparer.OrdinalIgnoreCase)
                .Select(static entry => entry.Card)
                .FirstOrDefault();
            if (match is null)
            {
                continue;
            }

            hints.Add(new CombatCardKnowledgeHint(
                handCard.SlotIndex,
                handCard.Name,
                handCard.Type ?? match.Type,
                match.Target,
                handCard.Cost ?? match.Cost,
                "assistant/cards.json"));
        }

        return hints;
    }

    static IReadOnlyList<string> BuildCombatKnowledgeLookupKeys(string? cardName)
    {
        if (string.IsNullOrWhiteSpace(cardName))
        {
            return Array.Empty<string>();
        }

        var keys = new List<string>();
        var normalizedName = NormalizeKnowledgeKey(cardName);
        AddCombatKnowledgeLookupKey(keys, normalizedName);

        var parts = cardName
            .Split(new[] { '.', '_', '-', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeKnowledgeKey)
            .Where(static part => part.Length > 0)
            .ToArray();
        if (parts.Length == 0)
        {
            return keys;
        }

        var trimmedParts = parts[0] == "card"
            ? parts[1..]
            : parts;
        if (trimmedParts.Length == 0)
        {
            return keys;
        }

        AddCombatKnowledgeLookupKey(keys, string.Concat(trimmedParts));
        AddCombatKnowledgeLookupKey(keys, trimmedParts[0]);
        if (trimmedParts.Length > 1 && IsCombatClassSuffix(trimmedParts[^1]))
        {
            AddCombatKnowledgeLookupKey(keys, string.Concat(trimmedParts[..^1]));
        }

        foreach (var part in trimmedParts)
        {
            AddCombatKnowledgeLookupKey(keys, part);
        }

        return keys;
    }

    static void AddCombatKnowledgeLookupKey(List<string> keys, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate)
            || keys.Contains(candidate, StringComparer.Ordinal))
        {
            return;
        }

        keys.Add(candidate);
    }

    static bool IsCombatClassSuffix(string value)
    {
        return value is "ironclad" or "silent" or "defect" or "watcher" or "colorless" or "status" or "curse";
    }

    static string NormalizeKnowledgeKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        Span<char> buffer = stackalloc char[value.Length];
        var length = 0;
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                buffer[length] = char.ToLowerInvariant(character);
                length += 1;
            }
        }

        return length == 0
            ? string.Empty
            : new string(buffer[..length]);
    }
}
