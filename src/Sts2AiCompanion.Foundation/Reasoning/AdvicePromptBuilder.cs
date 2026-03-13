using System.Text;
using System.Text.Json;
using Sts2AiCompanion.Foundation.Contracts;
using Sts2ModKit.Core.Configuration;

namespace Sts2AiCompanion.Foundation.Reasoning;

public sealed class AdvicePromptBuilder
{
    private readonly ScaffoldConfiguration _configuration;

    public AdvicePromptBuilder(ScaffoldConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AdviceInputPack BuildInputPack(CompanionRunState runState, AdviceTrigger trigger, KnowledgeSlice slice)
    {
        return new AdviceInputPack(
            runState.Snapshot.RunId,
            trigger.Kind,
            DateTimeOffset.UtcNow,
            trigger.Manual,
            runState.Snapshot.CurrentScreen,
            runState.SummaryText,
            runState.Snapshot,
            runState.RecentEvents.TakeLast(_configuration.Assistant.RecentEventsCount).ToArray(),
            slice.Entries,
            slice.Reasons,
            "You are a Slay the Spire 2 advisor. Do not play for the user. Use only the supplied state, recent events, and knowledge slice. If information is missing, say exactly what is missing instead of guessing.");
    }

    public string FormatPrompt(AdviceInputPack inputPack)
    {
        var builder = new StringBuilder();
        builder.AppendLine("You are a Slay the Spire 2 external advisor.");
        builder.AppendLine("- Do not play on behalf of the user.");
        builder.AppendLine("- Use only the supplied state, choices, recent events, and knowledge slice.");
        builder.AppendLine("- If information is missing, do not guess. Report missing information and decision blockers.");
        builder.AppendLine("- Return JSON only.");
        builder.AppendLine();
        builder.AppendLine($"trigger: {inputPack.TriggerKind}");
        builder.AppendLine($"manual: {inputPack.Manual}");
        builder.AppendLine($"run_id: {inputPack.RunId}");
        builder.AppendLine($"screen: {inputPack.CurrentScreen}");
        builder.AppendLine();
        builder.AppendLine("current_state_summary:");
        builder.AppendLine(inputPack.SummaryText);
        builder.AppendLine();
        builder.AppendLine("recent_events:");
        foreach (var envelope in inputPack.RecentEvents)
        {
            builder.AppendLine($"- {envelope.Ts:O} {envelope.Kind} screen={envelope.Screen} act={envelope.Act?.ToString() ?? "?"} floor={envelope.Floor?.ToString() ?? "?"}");
            if (envelope.Payload.Count > 0)
            {
                builder.AppendLine($"  payload: {JsonSerializer.Serialize(envelope.Payload)}");
            }
        }

        if (inputPack.RecentEvents.Count == 0)
        {
            builder.AppendLine("- none");
        }

        builder.AppendLine();
        builder.AppendLine("knowledge_slice:");
        foreach (var entry in inputPack.KnowledgeEntries)
        {
            var tags = entry.Tags.Count == 0 ? "none" : string.Join(", ", entry.Tags);
            builder.AppendLine($"- {entry.Name} [{entry.Id}] source={entry.Source} observed={entry.Observed} tags={tags}");
            if (!string.IsNullOrWhiteSpace(entry.RawText))
            {
                builder.AppendLine($"  raw: {entry.RawText}");
            }

            foreach (var option in entry.Options.Take(5))
            {
                builder.AppendLine($"  option: {option.Label} :: {option.Description ?? "no-extra-description"}");
            }
        }

        if (inputPack.KnowledgeEntries.Count == 0)
        {
            builder.AppendLine("- none");
        }

        builder.AppendLine();
        builder.AppendLine("response_instructions:");
        builder.AppendLine("- headline: short current assessment");
        builder.AppendLine("- summary: 2-4 sentences");
        builder.AppendLine("- recommendedAction: next best action");
        builder.AppendLine("- recommendedChoiceLabel: exact current choice label if available, else null");
        builder.AppendLine("- reasoningBullets: 2-5 bullets");
        builder.AppendLine("- riskNotes: 0-5 bullets");
        builder.AppendLine("- missingInformation: list of missing state/details");
        builder.AppendLine("- decisionBlockers: list of direct blockers preventing a firm recommendation");
        builder.AppendLine("- confidence: number from 0.0 to 1.0 or null");
        builder.AppendLine("- knowledgeRefs: ids or names of knowledge entries used");
        return builder.ToString().TrimEnd();
    }

    public string FormatAdviceMarkdown(AdviceResponse response)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {response.Headline}");
        builder.AppendLine();
        builder.AppendLine(response.Summary);
        builder.AppendLine();
        builder.AppendLine($"- trigger: {response.TriggerKind}");
        builder.AppendLine($"- run_id: {response.RunId}");
        builder.AppendLine($"- recommended_action: {response.RecommendedAction}");
        builder.AppendLine($"- recommended_choice: {response.RecommendedChoiceLabel ?? "none"}");
        builder.AppendLine($"- confidence: {(response.Confidence?.ToString("0.00") ?? "unknown")}");
        builder.AppendLine();
        builder.AppendLine("## Reasoning");
        foreach (var bullet in response.ReasoningBullets.DefaultIfEmpty("none"))
        {
            builder.AppendLine($"- {bullet}");
        }

        builder.AppendLine();
        builder.AppendLine("## Risks");
        foreach (var note in response.RiskNotes.DefaultIfEmpty("none"))
        {
            builder.AppendLine($"- {note}");
        }

        builder.AppendLine();
        builder.AppendLine("## Missing information");
        foreach (var item in response.MissingInformation.DefaultIfEmpty("none"))
        {
            builder.AppendLine($"- {item}");
        }

        builder.AppendLine();
        builder.AppendLine("## Decision blockers");
        foreach (var item in response.DecisionBlockers.DefaultIfEmpty("none"))
        {
            builder.AppendLine($"- {item}");
        }

        builder.AppendLine();
        builder.AppendLine("## Knowledge references");
        foreach (var reference in response.KnowledgeRefs.DefaultIfEmpty("none"))
        {
            builder.AppendLine($"- {reference}");
        }

        return builder.ToString().TrimEnd();
    }
}
