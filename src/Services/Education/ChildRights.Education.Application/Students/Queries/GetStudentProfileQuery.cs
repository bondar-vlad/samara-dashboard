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

        var grades = await context.Grades
            .Where(g => g.StudentId == student.Id)
            .ToListAsync(cancellationToken);

        var subjectAverages = grades
            .GroupBy(g => g.Subject)
            .Select(g => new SubjectAverageDto(g.Key, g.Average(x => x.Value), g.Count()))
            .ToList();

        var topicAverages = grades
            .Where(g => !string.IsNullOrWhiteSpace(g.Topic))
            .GroupBy(g => new { g.Subject, g.Topic })
            .Select(g => new TopicAverageDto(g.Key.Subject, g.Key.Topic!, g.Average(x => x.Value), g.Count()))
            .OrderByDescending(t => t.Average)
            .ToList();

        var profileChoice = ProfileMapping.ToChoice(
            student.DeclaredProfile,
            student.DesiredCluster,
            student.DesiredProfiles,
            student.RecommendedCluster,
            student.RecommendedProfiles,
            student.RecommendationConfidence,
            student.RecommendationUpdatedUtc,
            student.HasProfileMismatch);

        return new StudentProfileDto(
            student.Id,
            student.FullName,
            student.GradeLevel,
            student.SchoolId,
            student.ClassId,
            className,
            profileChoice,
            attendanceSummary,
            subjectAverages,
            topicAverages);
    }
}
