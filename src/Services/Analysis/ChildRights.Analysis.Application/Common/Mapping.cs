using ChildRights.Analysis.Domain.Entities;

namespace ChildRights.Analysis.Application.Common;

internal static class Mapping
{
    public static RedFlagDto ToDto(this RedFlag flag) => new(
        flag.Id,
        flag.RuleCode,
        flag.Scope.ToString(),
        flag.SubjectId,
        flag.SubjectName,
        flag.Severity.ToString(),
        flag.Title,
        flag.Description,
        flag.SourceAgency.ToString(),
        flag.TargetAudiences.Select(a => a.ToString()).ToList(),
        flag.RecommendedActions,
        flag.AiModel,
        flag.DetectedAtUtc,
        flag.Status.ToString());

    public static RecommendationDto ToDto(this Recommendation recommendation) => new(
        recommendation.Id,
        recommendation.Scope.ToString(),
        recommendation.SubjectId,
        recommendation.SubjectName,
        recommendation.Kind.ToString(),
        recommendation.Title,
        recommendation.Summary,
        recommendation.Rationale,
        recommendation.Confidence,
        recommendation.AiModel,
        recommendation.CreatedAtUtc);

    public static AnalysisRunDto ToDto(this AnalysisRun run) => new(
        run.Id,
        run.Trigger.ToString(),
        run.Scope.ToString(),
        run.SubjectId,
        run.ModelName,
        run.Status.ToString(),
        run.StartedAtUtc,
        run.CompletedAtUtc,
        run.FlagsProduced,
        run.RecommendationsProduced,
        run.Summary);
}
