using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Education.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Application.Schools.Queries;

/// <summary>Full institution card: classes plus the reform profiles it offers.</summary>
public sealed record GetSchoolDetailsQuery(Guid SchoolId) : IQuery<SchoolDetailsDto>;

internal sealed class GetSchoolDetailsQueryHandler(IEducationDbContext context)
    : IQueryHandler<GetSchoolDetailsQuery, SchoolDetailsDto>
{
    public async Task<Result<SchoolDetailsDto>> Handle(
        GetSchoolDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var school = await context.Schools
            .FirstOrDefaultAsync(s => s.Id == query.SchoolId, cancellationToken);

        if (school is null)
        {
            return Result.Failure<SchoolDetailsDto>(Error.NotFound($"School '{query.SchoolId}' was not found."));
        }

        var classes = await context.Classes
            .Where(c => c.SchoolId == school.Id)
            .OrderBy(c => c.GradeLevel).ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var studentCountsByClass = await context.Students
            .Where(s => s.SchoolId == school.Id)
            .GroupBy(s => s.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassId, x => x.Count, cancellationToken);

        var offerings = await context.SchoolProfileOfferings
            .Where(o => o.SchoolId == school.Id)
            .ToListAsync(cancellationToken);

        var classDtos = classes
            .Select(c => new ClassDto(
                c.Id, c.Name, c.GradeLevel, c.ClassTeacher, studentCountsByClass.GetValueOrDefault(c.Id)))
            .ToList();

        var offeredProfiles = offerings
            .Select(o => new OfferedProfileDto(
                o.Profile.ToString(),
                ProfileTaxonomy.Localize(o.Profile),
                ProfileTaxonomy.ClusterOf(o.Profile).ToString(),
                ProfileTaxonomy.LocalizeCluster(ProfileTaxonomy.ClusterOf(o.Profile))))
            .ToList();

        var details = new SchoolDetailsDto(
            school.Id,
            school.Name,
            school.Community,
            school.Region,
            school.InstitutionType.ToString(),
            InstitutionTaxonomy.Localize(school.InstitutionType),
            school.Direction.ToString(),
            studentCountsByClass.Values.Sum(),
            classDtos,
            offeredProfiles);

        return Result.Success(details);
    }
}
