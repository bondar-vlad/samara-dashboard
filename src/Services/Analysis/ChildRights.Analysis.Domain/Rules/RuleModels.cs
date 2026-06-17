using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Enums;

namespace ChildRights.Analysis.Domain.Rules;

/// <summary>A pupil's average for a single curriculum topic within a subject.</summary>
public sealed record TopicScore(string Subject, string Topic, double Average);

/// <summary>Immutable snapshot of a pupil used as input to the rule/AI engine.</summary>
public sealed record StudentSnapshot(
    Guid StudentId,
    string StudentName,
    Guid SchoolId,
    Guid ClassId,
    int GradeLevel,
    EducationProfile? DeclaredProfile,
    IReadOnlyList<EducationProfile> DesiredProfiles,
    int UnexcusedAbsences,
    IReadOnlyDictionary<string, double> SubjectAverages,
    IReadOnlyList<TopicScore> TopicAverages)
{
    /// <summary>The cluster implied by the pupil's desired profiles, if any.</summary>
    public ProfileCluster? DesiredCluster =>
        DesiredProfiles.Count == 0 ? null : ProfileTaxonomy.ClusterOf(DesiredProfiles[0]);
}

/// <summary>A red-flag finding produced by the engine (not yet persisted).</summary>
public sealed record FlagFinding(
    string RuleCode,
    FlagSeverity Severity,
    string Title,
    string Description,
    Agency SourceAgency,
    IReadOnlyCollection<AudienceRole> TargetAudiences,
    IReadOnlyCollection<string> RecommendedActions);

/// <summary>
/// A recommendation finding produced by the engine (not yet persisted). For profiling
/// recommendations it carries the structured cluster and profiles so the result can be
/// written back to the pupil record and published as an integration event.
/// </summary>
public sealed record RecommendationFinding(
    RecommendationKind Kind,
    string Title,
    string Summary,
    string Rationale,
    double Confidence,
    ProfileCluster? RecommendedCluster = null,
    IReadOnlyList<EducationProfile>? RecommendedProfiles = null);

/// <summary>The complete result of evaluating a single snapshot.</summary>
public sealed record RuleEvaluation(
    IReadOnlyList<FlagFinding> Flags,
    IReadOnlyList<RecommendationFinding> Recommendations);
