using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Admission;
using ChildRights.Analysis.Domain.Enums;
using ChildRights.Analysis.Domain.Rules;
using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Analysis.Infrastructure.Ai;

/// <summary>
/// Deterministic, always-available analysis "model". Wraps the pure domain rule engines and
/// routes each <see cref="AnalysisGoal"/> to the rule subset for that concrete case. Acts as
/// the safe default and the fallback when no LLM is configured.
/// </summary>
internal sealed class RuleBasedAiAnalysisProvider : IAiAnalysisProvider
{
    public string ModelName => "rule-based-v1";

    public Task<AnalysisResult> AnalyzeAsync(AnalysisRequest request, CancellationToken cancellationToken = default)
    {
        var evaluation = EvaluateGoal(request);

        var summary =
            $"Детерміновані правила ({request.Goal}): {evaluation.Flags.Count} ред-флаг(ів), " +
            $"{evaluation.Recommendations.Count} рекомендаці(й).";

        return Task.FromResult(new AnalysisResult(
            ModelName,
            evaluation.Flags,
            evaluation.Recommendations,
            summary));
    }

    /// <summary>
    /// Routes a request to the deterministic rule subset for its <see cref="AnalysisGoal"/>.
    /// Exposed so the LLM provider can reuse the exact same per-goal fallback.
    /// </summary>
    internal static RuleEvaluation EvaluateGoal(AnalysisRequest request)
    {
        var snapshot = request.Snapshot;
        var admission = request.Admission;

        return request.Goal switch
        {
            AnalysisGoal.StudentRisk => StudentRiskRules.EvaluateRisk(snapshot),

            AnalysisGoal.ProfileChoice => StudentRiskRules.EvaluateProfile(snapshot),

            AnalysisGoal.NmtFourthSubject => AdmissionRules.EvaluateFourthSubjectGoal(
                snapshot.SubjectAverages,
                snapshot.TopicAverages,
                admission?.ChosenFourthSubject),

            AnalysisGoal.AdmissionDirection => AdmissionRules.EvaluateDirectionGoal(
                snapshot.SubjectAverages,
                snapshot.TopicAverages,
                admission?.NmtScores ?? new Dictionary<NmtSubject, int>(),
                admission?.DesiredDirectionCode,
                admission?.Directions ?? []),

            _ => new RuleEvaluation([], [])
        };
    }
}
