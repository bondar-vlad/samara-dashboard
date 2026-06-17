namespace ChildRights.Analysis.Application.Common;

public sealed record RedFlagDto(
    Guid Id,
    string RuleCode,
    string Scope,
    Guid SubjectId,
    string SubjectName,
    string Severity,
    string Title,
    string Description,
    string SourceAgency,
    IReadOnlyCollection<string> TargetAudiences,
    IReadOnlyCollection<string> RecommendedActions,
    string AiModel,
    DateTime DetectedAtUtc,
    string Status);

public sealed record RecommendationDto(
    Guid Id,
    string Scope,
    Guid SubjectId,
    string SubjectName,
    string Kind,
    string Title,
    string Summary,
    string Rationale,
    double Confidence,
    string AiModel,
    DateTime CreatedAtUtc);

public sealed record AnalysisRunDto(
    Guid Id,
    string Trigger,
    string Scope,
    Guid SubjectId,
    string ModelName,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    int FlagsProduced,
    int RecommendationsProduced,
    string Summary);

public sealed record AnalysisRunResultDto(
    Guid RunId,
    string ModelName,
    string Status,
    int FlagsProduced,
    int RecommendationsProduced,
    string Summary,
    IReadOnlyList<RedFlagDto> Flags,
    IReadOnlyList<RecommendationDto> Recommendations);

public sealed record SchoolAnalysisResultDto(
    Guid SchoolId,
    int StudentsAnalysed,
    int FlagsProduced,
    int RecommendationsProduced,
    IReadOnlyList<RecommendationDto> SchoolRecommendations);

public sealed record SeverityCountDto(string Severity, int Count);

public sealed record DashboardSummaryDto(
    int TotalFlags,
    int OpenFlags,
    IReadOnlyList<SeverityCountDto> BySeverity,
    int TotalRecommendations,
    int TotalRuns,
    IReadOnlyList<RedFlagDto> RecentFlags);
