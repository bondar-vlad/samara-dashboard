namespace ChildRights.Analysis.Application.Abstractions;

/// <summary>
/// Port that lets the Analysis service pull pupil data from the Education microservice.
/// Implemented by a resilient typed HTTP client in the infrastructure layer.
/// </summary>
public interface IEducationDataClient
{
    Task<IReadOnlyList<EducationStudentRef>> GetStudentsAsync(Guid? schoolId, CancellationToken cancellationToken = default);

    Task<EducationStudentProfile?> GetStudentProfileAsync(Guid studentId, CancellationToken cancellationToken = default);
}

public sealed record EducationStudentRef(
    Guid Id,
    string FullName,
    string ClassName,
    int GradeLevel,
    Guid SchoolId);

public sealed record EducationSubjectAverage(string Subject, double Average, int Count);

/// <summary>Per-topic performance, the granularity that drives topic-aware profiling.</summary>
public sealed record EducationTopicAverage(string Subject, string Topic, double Average, int Count);

public sealed record EducationAttendanceSummary(
    int Total,
    int Present,
    int Excused,
    int Unexcused,
    string Severity);

/// <summary>A profile reference as returned by the Education API (enum names as strings).</summary>
public sealed record EducationProfileRef(string Profile, string ProfileName, string Cluster, string ClusterName);

/// <summary>The pupil's declared/desired/recommended profile picture.</summary>
public sealed record EducationProfileChoice(
    string? DeclaredProfile,
    string? DesiredCluster,
    IReadOnlyList<EducationProfileRef> DesiredProfiles,
    string? RecommendedCluster,
    IReadOnlyList<EducationProfileRef> RecommendedProfiles,
    double? RecommendationConfidence,
    DateTime? RecommendationUpdatedUtc,
    bool HasMismatch);

public sealed record EducationStudentProfile(
    Guid Id,
    string FullName,
    int GradeLevel,
    Guid SchoolId,
    Guid ClassId,
    string ClassName,
    EducationProfileChoice ProfileChoice,
    EducationAttendanceSummary Attendance,
    IReadOnlyList<EducationSubjectAverage> SubjectAverages,
    IReadOnlyList<EducationTopicAverage> TopicAverages);
