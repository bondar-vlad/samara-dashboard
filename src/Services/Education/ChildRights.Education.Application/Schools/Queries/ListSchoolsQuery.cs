using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Education.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Application.Schools.Queries;

/// <summary>Lists institutions, optionally filtered by region, community or institution type.</summary>
public sealed record ListSchoolsQuery(string? Region = null, string? Community = null, string? InstitutionType = null)
    : IQuery<IReadOnlyCollection<SchoolSummaryDto>>;

internal sealed class ListSchoolsQueryHandler(IEducationDbContext context)
    : IQueryHandler<ListSchoolsQuery, IReadOnlyCollection<SchoolSummaryDto>>
{
    public async Task<Result<IReadOnlyCollection<SchoolSummaryDto>>> Handle(
        ListSchoolsQuery query,
        CancellationToken cancellationToken)
    {
        var schools = await context.Schools
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        IEnumerable<Domain.Entities.School> filtered = schools;
        if (!string.IsNullOrWhiteSpace(query.Region))
        {
            filtered = filtered.Where(s => s.Region.Contains(query.Region, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Community))
        {
            filtered = filtered.Where(s => s.Community.Contains(query.Community, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.InstitutionType))
        {
            filtered = filtered.Where(s =>
                string.Equals(s.InstitutionType.ToString(), query.InstitutionType, StringComparison.OrdinalIgnoreCase));
        }

        var filteredList = filtered.ToList();
        var schoolIds = filteredList.Select(s => s.Id).ToList();

        var studentCounts = await context.Students
            .Where(s => schoolIds.Contains(s.SchoolId))
            .GroupBy(s => s.SchoolId)
            .Select(g => new { SchoolId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SchoolId, x => x.Count, cancellationToken);

        var offeringCounts = await context.SchoolProfileOfferings
            .Where(o => schoolIds.Contains(o.SchoolId))
            .GroupBy(o => o.SchoolId)
            .Select(g => new { SchoolId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SchoolId, x => x.Count, cancellationToken);

        IReadOnlyCollection<SchoolSummaryDto> result = filteredList
            .Select(s => new SchoolSummaryDto(
                s.Id,
                s.Name,
                s.Community,
                s.Region,
                s.InstitutionType.ToString(),
                InstitutionTaxonomy.Localize(s.InstitutionType),
                s.Direction.ToString(),
                studentCounts.GetValueOrDefault(s.Id),
                offeringCounts.GetValueOrDefault(s.Id)))
            .ToList();

        return Result.Success(result);
    }
}
