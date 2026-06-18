namespace ChildRights.Analysis.Application.ProfileChoice;

/// <summary>A profile with its localized name and the cluster it belongs to.</summary>
public sealed record ProfileRefDto(string Profile, string ProfileName, string Cluster, string ClusterName);

/// <summary>A cluster with its data-driven score (0–12), used to rank the recommendation.</summary>
public sealed record ClusterScoreDto(string Cluster, string ClusterName, double Score);

/// <summary>
/// A pupil row in the 10th-grade profile-choice list: the cluster the pupil desires versus the
/// cluster the data recommends, and whether they match. This is the <b>profile</b> decision
/// (entering the profile high school), distinct from the 11th-grade НМТ admission decision.
/// </summary>
public sealed record ProfileChoiceStudentDto(
    Guid StudentId,
    string FullName,
    string ClassName,
    string? DesiredCluster,
    string? DesiredClusterName,
    string? RecommendedCluster,
    string? RecommendedClusterName,
    bool HasChoice,
    bool IsMatch);

/// <summary>Distribution of desired vs recommended counts for one cluster (chart bar).</summary>
public sealed record ProfileChoiceDistributionItemDto(
    string Cluster,
    string ClusterName,
    int ChosenCount,
    int RecommendedCount);

/// <summary>The school-level result: the pupil rows plus the per-cluster distribution.</summary>
public sealed record ProfileChoiceStudentsResultDto(
    IReadOnlyCollection<ProfileChoiceStudentDto> Students,
    IReadOnlyCollection<ProfileChoiceDistributionItemDto> Distribution);

/// <summary>
/// One pupil's profile-choice analysis: their desired cluster/profiles, the recommended ones,
/// the ranked clusters with scores, and a short rationale.
/// </summary>
public sealed record ProfileChoiceRecommendationDto(
    Guid StudentId,
    string? DesiredCluster,
    string? DesiredClusterName,
    IReadOnlyCollection<ProfileRefDto> DesiredProfiles,
    string? RecommendedCluster,
    string? RecommendedClusterName,
    IReadOnlyCollection<ProfileRefDto> RecommendedProfiles,
    bool HasChoice,
    bool IsMatch,
    double? Confidence,
    IReadOnlyCollection<ClusterScoreDto> Ranked,
    string Rationale);
