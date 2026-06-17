using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Rules;

namespace ChildRights.Analysis.Infrastructure.Ai;

/// <summary>
/// Deterministic, always-available analysis "model". Wraps the pure domain rule
/// engine. Acts as the safe default and the fallback when no LLM is configured.
/// </summary>
internal sealed class RuleBasedAiAnalysisProvider : IAiAnalysisProvider
{
    public string ModelName => "rule-based-v1";

    public Task<AnalysisResult> AnalyzeAsync(AnalysisRequest request, CancellationToken cancellationToken = default)
    {
        var evaluation = StudentRiskRules.Evaluate(request.Snapshot);

        var summary =
            $"Детерміновані правила: {evaluation.Flags.Count} ред-флаг(ів), " +
            $"{evaluation.Recommendations.Count} рекомендаці(й).";

        return Task.FromResult(new AnalysisResult(
            ModelName,
            evaluation.Flags,
            evaluation.Recommendations,
            summary));
    }
}
