using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Web;
using ChildRights.Analysis.Application.Dashboard.Queries;
using ChildRights.Analysis.Application.Recommendations.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ChildRights.Analysis.Api.Controllers;

[Route("api/recommendations")]
public sealed class RecommendationsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>Lists profiling recommendations filtered by scope and/or subject.</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? scope,
        [FromQuery] Guid? subjectId,
        CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new ListRecommendationsQuery(scope, subjectId), cancellationToken));
}

[Route("api/dashboard")]
public sealed class DashboardController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>Top-level KPIs for the management dashboard landing page.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new GetDashboardSummaryQuery(), cancellationToken));
}
