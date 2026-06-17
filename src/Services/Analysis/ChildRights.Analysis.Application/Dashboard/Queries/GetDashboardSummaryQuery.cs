using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Application.Common;
using ChildRights.Analysis.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Dashboard.Queries;

/// <summary>Top-level KPIs for the management dashboard landing page.</summary>
public sealed record GetDashboardSummaryQuery : IQuery<DashboardSummaryDto>;

internal sealed class GetDashboardSummaryQueryHandler(IAnalysisDbContext context)
    : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    public async Task<Result<DashboardSummaryDto>> Handle(
        GetDashboardSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var flags = await context.RedFlags.ToListAsync(cancellationToken);

        var bySeverity = flags
            .GroupBy(f => f.Severity)
            .OrderByDescending(g => g.Key)
            .Select(g => new SeverityCountDto(g.Key.ToString(), g.Count()))
            .ToList();

        var totalRecommendations = await context.Recommendations.CountAsync(cancellationToken);
        var totalRuns = await context.AnalysisRuns.CountAsync(cancellationToken);

        var recentFlags = flags
            .OrderByDescending(f => f.DetectedAtUtc)
            .Take(10)
            .Select(f => f.ToDto())
            .ToList();

        var summary = new DashboardSummaryDto(
            flags.Count,
            flags.Count(f => f.Status == FlagStatus.Open),
            bySeverity,
            totalRecommendations,
            totalRuns,
            recentFlags);

        return Result.Success(summary);
    }
}
