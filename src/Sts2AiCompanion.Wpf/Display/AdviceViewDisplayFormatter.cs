using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoundationAdvicePerspectiveView = Sts2AiCompanion.Foundation.Contracts.AdvicePerspectiveView;
using HostAdviceResponse = Sts2AiCompanion.Host.AdviceResponse;

namespace Sts2AiCompanion.Wpf.Display;

public static class AdviceViewDisplayFormatter
{
    public static string FormatFinalOverview(HostAdviceResponse advice, string? analysisLatencyText)
    {
        return JoinLines(new[]
        {
            advice.Headline,
            string.Empty,
            advice.Summary,
            string.Empty,
            $"권장 행동: {advice.RecommendedAction}",
            $"권장 선택지: {advice.RecommendedChoiceLabel ?? "-"}",
            analysisLatencyText is null ? null : $"분석 시간: {analysisLatencyText}",
        });
    }

    public static string FormatFinalDetails(HostAdviceResponse advice, IReadOnlyList<string> recentChanges)
    {
        return JoinLines(new[]
        {
            "근거",
            FormatBulletSection(advice.ReasoningBullets),
            string.Empty,
            "리스크",
            FormatBulletSection(advice.RiskNotes),
            string.Empty,
            "부족한 정보",
            FormatBulletSection(advice.MissingInformation),
            string.Empty,
            "판단 차단 요인",
            FormatBulletSection(advice.DecisionBlockers),
            string.Empty,
            "최근 변화",
            FormatBulletSection(recentChanges),
        });
    }

    public static string FormatAuxiliaryView(string title, FoundationAdvicePerspectiveView? view)
    {
        if (view is null)
        {
            return $"{title}: 별도 관점 없음";
        }

        return JoinLines(new[]
        {
            view.Headline,
            string.Empty,
            view.Summary,
            string.Empty,
            $"권장 선택지: {view.RecommendedChoiceLabel ?? "-"}",
            string.Empty,
            "근거",
            FormatBulletSection(view.ReasoningBullets),
            string.Empty,
            "리스크",
            FormatBulletSection(view.RiskNotes),
        });
    }

    private static string FormatBulletSection(IReadOnlyList<string> items)
    {
        return JoinLines(items.DefaultIfEmpty("없음").Select(item => $"- {item}"));
    }

    private static string JoinLines(IEnumerable<string?> lines)
    {
        var builder = new StringBuilder();
        foreach (var line in lines.Where(static line => line is not null))
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(line);
        }

        return builder.ToString();
    }
}
