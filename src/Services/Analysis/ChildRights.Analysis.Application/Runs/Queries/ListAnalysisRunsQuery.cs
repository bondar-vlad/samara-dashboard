using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Runs.Queries;

/// <summary>Lists recent analysis runs (audit trail of triggers and produced output).</summary>
public sealed record ListAnalysisRunsQuery(int Take = 50) : IQuery<IReadOnlyCollection<AnalysisRunDto>>;

internal sealed class ListAnalysisRunsQueryHandler(IAnalysisDbContext context)
    : IQueryHandler<ListAnalysisRunsQuery, IReadOnlyCollection<AnalysisRunDto>>
{
    public async Task<Result<IReadOnlyCollection<AnalysisRunDto>>> Handle(
        ListAnalysisRunsQuery query,
        CancellationToken cancellationToken)
    {
        var runs = await context.AnalysisRuns
            .OrderByDescending(r => r.StartedAtUtc)
            .Take(query.Take)
            .ToListAsync(cancellationToken);

        IReadOnlyCollection<AnalysisRunDto> result = runs.Select(r => r.ToDto()).ToList();
        return Result.Success(result);
    }
}
