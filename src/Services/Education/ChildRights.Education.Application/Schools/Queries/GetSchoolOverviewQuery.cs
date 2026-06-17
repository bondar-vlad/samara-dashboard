using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Education.Application.Abstractions;
using ChildRights.Education.Application.Students;
using ChildRights.Education.Domain.Enums;
using ChildRights.Education.Domain.Policies;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Application.Schools.Queries;

public sealed record SchoolOverviewDto(
    Guid SchoolId,
    string SchoolName,
    string Community,
    string Region,
    int StudentCount,
    int ClassCount,
    int StudentsWithAttendanceRisk,
    IReadOnlyCollection<SubjectAverageDto> SubjectAverages);

/// <summary>Aggregated, school-level view used by the institution dashboard.</summary>
public sealed record GetSchoolOverviewQuery(Guid SchoolId) : IQuery<SchoolOverviewDto>;

internal sealed class GetSchoolOverviewQueryHandler(IEducationDbContext context)
    : IQueryHandler<GetSchoolOverviewQuery, SchoolOverviewDto>
{
    public async Task<Result<SchoolOverviewDto>> Handle(
        GetSchoolOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var school = await context.Schools
            .FirstOrDefaultAsync(s => s.Id == query.SchoolId, cancellationToken);

        if (school is null)
        {
            return Result.Failure<SchoolOverviewDto>(
                Error.NotFound($"School '{query.SchoolId}' was not found."));
        }

        var studentIds = await context.Students
            .Where(s => s.SchoolId == school.Id)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var classCount = await context.Classes
            .CountAsync(c => c.SchoolId == school.Id, cancellationToken);

        var unexcusedPerStudent = await context.AttendanceRecords
            .Where(a => studentIds.Contains(a.StudentId) && a.Status == AttendanceStatus.Unexcused)
            .GroupBy(a => a.StudentId)
            .Select(g => g.Count())
            .ToListAsync(cancellationToken);

        var atRisk = unexcusedPerStudent.Count(count => count >= AttendancePolicy.OrangeThreshold);

        var subjectAverages = await context.Grades
            .Where(g => studentIds.Contains(g.StudentId))
            .GroupBy(g => g.Subject)
            .Select(g => new SubjectAverageDto(g.Key, g.Average(x => x.Value), g.Count()))
            .ToListAsync(cancellationToken);

        return new SchoolOverviewDto(
            school.Id,
            school.Name,
            school.Community,
            school.Region,
            studentIds.Count,
            classCount,
            atRisk,
            subjectAverages);
    }
}
