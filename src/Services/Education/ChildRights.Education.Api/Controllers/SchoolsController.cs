using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Web;
using ChildRights.Education.Application.Schools.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ChildRights.Education.Api.Controllers;

[Route("api/schools")]
public sealed class SchoolsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>Lists institutions, optionally filtered by region, community or institution type.</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? region,
        [FromQuery] string? community,
        [FromQuery] string? institutionType,
        CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new ListSchoolsQuery(region, community, institutionType), cancellationToken));

    /// <summary>Returns the full institution card: classes and offered reform profiles.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetails(Guid id, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new GetSchoolDetailsQuery(id), cancellationToken));

    /// <summary>Returns an aggregated, school-level overview for the institution dashboard.</summary>
    [HttpGet("{id:guid}/overview")]
    public async Task<IActionResult> GetOverview(Guid id, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new GetSchoolOverviewQuery(id), cancellationToken));
}
