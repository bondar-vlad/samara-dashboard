using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Enums;

namespace ChildRights.Analysis.Domain.Rules;

/// <summary>Immutable snapshot of a pupil used as input to the rule/AI engine.</summary>
public sealed record StudentSnapshot(
    Guid StudentId,
    string StudentName,
    Guid SchoolId,
    Guid ClassId,
    int GradeLevel,
    string? DeclaredProfile,
    int UnexcusedAbsences,
    IReadOnlyDictionary<string, double> SubjectAverages);

/// <summary>A red-flag finding produced by the engine (not yet persisted).</summary>
public sealed record FlagFinding(
    string RuleCode,
    FlagSeverity Severity,
    string Title,
    string Description,
    Agency SourceAgency,
    IReadOnlyCollection<AudienceRole> TargetAudiences,
    IReadOnlyCollection<string> RecommendedActions);

/// <summary>A recommendation finding produced by the engine (not yet persisted).</summary>
public sealed record RecommendationFinding(
    RecommendationKind Kind,
    string Title,
    string Summary,
    string Rationale,
    double Confidence);

/// <summary>The complete result of evaluating a single snapshot.</summary>
public sealed record RuleEvaluation(
    IReadOnlyList<FlagFinding> Flags,
    IReadOnlyList<RecommendationFinding> Recommendations);
