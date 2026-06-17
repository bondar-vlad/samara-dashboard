using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Universities.Queries;

/// <summary>
/// Produces a <b>depersonalised</b> demand report for universities: per specialty it combines
/// explicit pupil interest with data-driven candidates (pupils whose recommended cluster matches
/// the programme's cluster). No individual pupil is ever exposed — only counts.
/// </summary>
public sealed record GetUniversityDemandQuery(Guid? UniversityId = null) : IQuery<UniversityDemandDto>;

internal sealed class GetUniversityDemandQueryHandler(IAnalysisDbContext context)
    : IQueryHandler<GetUniversityDemandQuery, UniversityDemandDto>
{
    public async Task<Result<UniversityDemandDto>> Handle(
        GetUniversityDemandQuery query,
        CancellationToken cancellationToken)
    {
        var programsQuery = context.UniversityPrograms.AsQueryable();
        if (query.UniversityId is { } universityId)
        {
            programsQuery = programsQuery.Where(p => p.UniversityId == universityId);
        }

        var programs = await programsQuery.ToListAsync(cancellationToken);

        var interestCounts = await context.ProgramInterests
            .GroupBy(i => i.ProgramId)
            .Select(g => new { ProgramId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProgramId, x => x.Count, cancellationToken);

        // Data-driven candidates per cluster (depersonalised: counts only).
        var insights = await context.StudentProfileInsights.ToListAsync(cancellationToken);
        var candidatesByCluster = insights
            .GroupBy(i => i.RecommendedCluster)
            .ToDictionary(g => g.Key, g => g.Count());

        var studentsConsidered = insights.Select(i => i.StudentId).Distinct().Count();

        var programDemand = programs
            .Select(p =>
            {
                var explicitInterest = interestCounts.GetValueOrDefault(p.Id);
                var dataDriven = candidatesByCluster.GetValueOrDefault(p.Cluster);
                return new ProgramDemandDto(
                    p.Id,
                    p.UniversityId,
                    p.UniversityName,
                    p.Name,
                    p.Cluster.ToString(),
                    ProfileTaxonomy.LocalizeCluster(p.Cluster),
                    explicitInterest,
                    dataDriven,
                    explicitInterest + dataDriven);
            })
            .OrderByDescending(d => d.TotalDemand)
            .ToList();

        var report = new UniversityDemandDto(query.UniversityId, studentsConsidered, programDemand);
        return Result.Success(report);
    }
}
