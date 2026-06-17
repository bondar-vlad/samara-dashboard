using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Education.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Application.Students.Queries;

/// <summary>Lists pupils, optionally filtered by school and/or class.</summary>
public sealed record ListStudentsQuery(Guid? SchoolId = null, Guid? ClassId = null)
    : IQuery<IReadOnlyCollection<StudentSummaryDto>>;

internal sealed class ListStudentsQueryHandler(IEducationDbContext context)
    : IQueryHandler<ListStudentsQuery, IReadOnlyCollection<StudentSummaryDto>>
{
    public async Task<Result<IReadOnlyCollection<StudentSummaryDto>>> Handle(
        ListStudentsQuery query,
        CancellationToken cancellationToken)
    {
        var studentsQuery = context.Students.AsQueryable();

        if (query.SchoolId is { } schoolId)
        {
            studentsQuery = studentsQuery.Where(s => s.SchoolId == schoolId);
        }

        if (query.ClassId is { } classId)
        {
            studentsQuery = studentsQuery.Where(s => s.ClassId == classId);
        }

        var students = await studentsQuery
            .OrderBy(s => s.FullName)
            .ToListAsync(cancellationToken);

        var classIds = students.Select(s => s.ClassId).Distinct().ToList();
        var classNames = await context.Classes
            .Where(c => classIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        IReadOnlyCollection<StudentSummaryDto> result = students
            .Select(s => new StudentSummaryDto(
                s.Id,
                s.FullName,
                classNames.GetValueOrDefault(s.ClassId, string.Empty),
                s.GradeLevel,
                s.SchoolId))
            .ToList();

        return Result.Success(result);
    }
}
