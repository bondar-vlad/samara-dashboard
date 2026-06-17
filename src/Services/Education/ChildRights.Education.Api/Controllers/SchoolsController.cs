using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Web;
using ChildRights.Education.Application.Schools.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ChildRights.Education.Api.Controllers;

[Route("api/schools")]
public sealed class SchoolsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>Returns an aggregated, school-level overview for the institution dashboard.</summary>
    [HttpGet("{id:guid}/overview")]
    public async Task<IActionResult> GetOverview(Guid id, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new GetSchoolOverviewQuery(id), cancellationToken));
}
