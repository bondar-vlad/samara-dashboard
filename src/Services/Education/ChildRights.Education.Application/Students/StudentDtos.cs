namespace ChildRights.Education.Application.Students;

public sealed record StudentSummaryDto(
    Guid Id,
    string FullName,
    string ClassName,
    int GradeLevel,
    Guid SchoolId,
    string? DesiredCluster,
    string? RecommendedCluster,
    bool HasProfileMismatch);

public sealed record SubjectAverageDto(string Subject, double Average, int Count);

/// <summary>Per-topic performance within a subject (e.g. "Фінансове право" inside "Правознавство").</summary>
public sealed record TopicAverageDto(string Subject, string Topic, double Average, int Count);

public sealed record AttendanceSummaryDto(
    int Total,
    int Present,
    int Excused,
    int Unexcused,
    string Severity);

/// <summary>A profile with its localized name and cluster, for display in choice lists.</summary>
public sealed record ProfileRefDto(string Profile, string ProfileName, string Cluster, string ClusterName);

/// <summary>
/// A pupil's profile-choice picture under the reform: the declared specialisation plus the
/// desired and recommended <i>clusters</i>, each with the several profiles chosen within them.
/// </summary>
public sealed record ProfileChoiceDto(
    string? DeclaredProfile,
    string? DesiredCluster,
    string? DesiredClusterName,
    IReadOnlyCollection<ProfileRefDto> DesiredProfiles,
    string? RecommendedCluster,
    string? RecommendedClusterName,
    IReadOnlyCollection<ProfileRefDto> RecommendedProfiles,
    double? RecommendationConfidence,
    DateTime? RecommendationUpdatedUtc,
    bool HasMismatch);

public sealed record StudentProfileDto(
    Guid Id,
    string FullName,
    int GradeLevel,
    Guid SchoolId,
    Guid ClassId,
    string ClassName,
    ProfileChoiceDto ProfileChoice,
    AttendanceSummaryDto Attendance,
    IReadOnlyCollection<SubjectAverageDto> SubjectAverages,
    IReadOnlyCollection<TopicAverageDto> TopicAverages);
