using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Universities.Queries;

/// <summary>Lists universities in the catalogue.</summary>
public sealed record ListUniversitiesQuery(string? Region = null) : IQuery<IReadOnlyCollection<UniversityDto>>;

internal sealed class ListUniversitiesQueryHandler(IAnalysisDbContext context)
    : IQueryHandler<ListUniversitiesQuery, IReadOnlyCollection<UniversityDto>>
{
    public async Task<Result<IReadOnlyCollection<UniversityDto>>> Handle(
        ListUniversitiesQuery query,
        CancellationToken cancellationToken)
    {
        var universities = await context.Universities
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(query.Region))
        {
            universities = universities
                .Where(u => u.Region.Contains(query.Region, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var programCounts = await context.UniversityPrograms
            .GroupBy(p => p.UniversityId)
            .Select(g => new { UniversityId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UniversityId, x => x.Count, cancellationToken);

        IReadOnlyCollection<UniversityDto> result = universities
            .Select(u => new UniversityDto(u.Id, u.Name, u.City, u.Region, programCounts.GetValueOrDefault(u.Id)))
            .ToList();

        return Result.Success(result);
    }
}

/// <summary>Lists university programmes (specialties), optionally filtered by university or cluster.</summary>
public sealed record ListUniversityProgramsQuery(Guid? UniversityId = null, string? Cluster = null)
    : IQuery<IReadOnlyCollection<UniversityProgramDto>>;

internal sealed class ListUniversityProgramsQueryHandler(IAnalysisDbContext context)
    : IQueryHandler<ListUniversityProgramsQuery, IReadOnlyCollection<UniversityProgramDto>>
{
    public async Task<Result<IReadOnlyCollection<UniversityProgramDto>>> Handle(
        ListUniversityProgramsQuery query,
        CancellationToken cancellationToken)
    {
        var programsQuery = context.UniversityPrograms.AsQueryable();
        if (query.UniversityId is { } universityId)
        {
            programsQuery = programsQuery.Where(p => p.UniversityId == universityId);
        }

        var programs = await programsQuery.OrderBy(p => p.UniversityName).ThenBy(p => p.Name).ToListAsync(cancellationToken);

        if (ProfileTaxonomy.TryParseCluster(query.Cluster) is { } cluster)
        {
            programs = programs.Where(p => p.Cluster == cluster).ToList();
        }

        IReadOnlyCollection<UniversityProgramDto> result = programs
            .Select(UniversityProgramMapping.ToDto)
            .ToList();

        return Result.Success(result);
    }
}
