using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Education.Application.Abstractions;
using ChildRights.Education.Domain.Enums;
using ChildRights.Education.Domain.Policies;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Application.Students.Queries;

/// <summary>Returns a pupil's 360° profile: attendance summary and subject averages.</summary>
public sealed record GetStudentProfileQuery(Guid StudentId) : IQuery<StudentProfileDto>;

internal sealed class GetStudentProfileQueryHandler(IEducationDbContext context)
    : IQueryHandler<GetStudentProfileQuery, StudentProfileDto>
{
    public async Task<Result<StudentProfileDto>> Handle(
        GetStudentProfileQuery query,
        CancellationToken cancellationToken)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == query.StudentId, cancellationToken);

        if (student is null)
        {
            return Result.Failure<StudentProfileDto>(
                Error.NotFound($"Student '{query.StudentId}' was not found."));
        }

        var className = await context.Classes
            .Where(c => c.Id == student.ClassId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var attendance = await context.AttendanceRecords
            .Where(a => a.StudentId == student.Id)
            .ToListAsync(cancellationToken);

        var unexcused = attendance.Count(a => a.Status == AttendanceStatus.Unexcused);
        var attendanceSummary = new AttendanceSummaryDto(
            attendance.Count,
            attendance.Count(a => a.Status == AttendanceStatus.Present),
            attendance.Count(a => a.Status == AttendanceStatus.Excused),
            unexcused,
            AttendancePolicy.Evaluate(unexcused).ToString());

        var subjectAverages = await context.Grades
            .Where(g => g.StudentId == student.Id)
            .GroupBy(g => g.Subject)
            .Select(g => new SubjectAverageDto(g.Key, g.Average(x => x.Value), g.Count()))
            .ToListAsync(cancellationToken);

        return new StudentProfileDto(
            student.Id,
            student.FullName,
            student.GradeLevel,
            student.DeclaredProfile?.ToString(),
            student.SchoolId,
            student.ClassId,
            className,
            attendanceSummary,
            subjectAverages);
    }
}
