using System.IO;
using static ObserverScreenProvenance;

static class GuiSmokeRewardMapEvidenceSupport
{
    public static RewardMapLayerState BuildRewardMapLayerState(ObserverSummary observer, WindowBounds? windowBounds)
    {
        return AutoDecisionProvider.BuildRewardSceneState(observer, windowBounds).LayerState;
    }

    public static bool LooksLikeRewardBackNavigationAffordance(ObserverSummary observer, string? screenshotPath)
    {
        if (observer.CurrentChoices.Any(static label =>
                label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                || label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                || label.Contains("뒤로", StringComparison.OrdinalIgnoreCase)
                || label.Contains("Back", StringComparison.OrdinalIgnoreCase))
            || observer.ActionNodes.Any(static node =>
                node.Actionable
                && (node.Label.Contains("LeftArrow", StringComparison.OrdinalIgnoreCase)
                    || node.Label.Contains("Backstop", StringComparison.OrdinalIgnoreCase)
                    || node.Label.Contains("뒤로", StringComparison.OrdinalIgnoreCase)
                    || node.Label.Contains("Back", StringComparison.OrdinalIgnoreCase))))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(screenshotPath) || !File.Exists(screenshotPath))
        {
            return false;
        }

        var overlayAnalysis = AutoOverlayUiAnalyzer.Analyze(screenshotPath);
        return overlayAnalysis.HasBottomLeftBackArrow
               && !overlayAnalysis.HasCentralOverlayPanel
               && (string.Equals(ControlFlowVisibleScreen(observer), "map", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(ControlFlowCurrentScreen(observer), "rewards", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase));
    }

    public static bool HasScreenshotClaimableRewardEvidence(ObserverSummary observer, string? screenshotPath)
    {
        if (string.IsNullOrWhiteSpace(screenshotPath) || !File.Exists(screenshotPath))
        {
            return false;
        }

        if (!MatchesControlFlowScreen(observer, "rewards")
            && !string.Equals(observer.ChoiceExtractorPath, "reward", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(observer.ChoiceExtractorPath, "rewards", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return AutoEventCardGridAnalyzer.Analyze(screenshotPath).HasSelectableCard;
    }
}
