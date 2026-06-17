using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Recommendations.Queries;

/// <summary>Lists profiling recommendations filtered by scope and/or subject.</summary>
public sealed record ListRecommendationsQuery(string? Scope = null, Guid? SubjectId = null)
    : IQuery<IReadOnlyCollection<RecommendationDto>>;

internal sealed class ListRecommendationsQueryHandler(IAnalysisDbContext context)
    : IQueryHandler<ListRecommendationsQuery, IReadOnlyCollection<RecommendationDto>>
{
    public async Task<Result<IReadOnlyCollection<RecommendationDto>>> Handle(
        ListRecommendationsQuery query,
        CancellationToken cancellationToken)
    {
        var recommendationsQuery = context.Recommendations.AsQueryable();

        if (query.SubjectId is { } subjectId)
        {
            recommendationsQuery = recommendationsQuery.Where(r => r.SubjectId == subjectId);
        }

        var recommendations = await recommendationsQuery
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        IEnumerable<Domain.Entities.Recommendation> filtered = recommendations;
        if (!string.IsNullOrWhiteSpace(query.Scope))
        {
            filtered = filtered.Where(r => string.Equals(r.Scope.ToString(), query.Scope, StringComparison.OrdinalIgnoreCase));
        }

        IReadOnlyCollection<RecommendationDto> result = filtered.Select(r => r.ToDto()).ToList();
        return Result.Success(result);
    }
}
