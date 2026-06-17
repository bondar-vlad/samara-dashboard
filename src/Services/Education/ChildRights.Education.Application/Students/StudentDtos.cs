namespace ChildRights.Education.Application.Students;

public sealed record StudentSummaryDto(
    Guid Id,
    string FullName,
    string ClassName,
    int GradeLevel,
    Guid SchoolId);

public sealed record SubjectAverageDto(string Subject, double Average, int Count);

public sealed record AttendanceSummaryDto(
    int Total,
    int Present,
    int Excused,
    int Unexcused,
    string Severity);

public sealed record StudentProfileDto(
    Guid Id,
    string FullName,
    int GradeLevel,
    string? DeclaredProfile,
    Guid SchoolId,
    Guid ClassId,
    string ClassName,
    AttendanceSummaryDto Attendance,
    IReadOnlyCollection<SubjectAverageDto> SubjectAverages);
