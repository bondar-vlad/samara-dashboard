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

public sealed record EducationAttendanceSummary(
    int Total,
    int Present,
    int Excused,
    int Unexcused,
    string Severity);

public sealed record EducationStudentProfile(
    Guid Id,
    string FullName,
    int GradeLevel,
    string? DeclaredProfile,
    Guid SchoolId,
    Guid ClassId,
    string ClassName,
    EducationAttendanceSummary Attendance,
    IReadOnlyList<EducationSubjectAverage> SubjectAverages);
