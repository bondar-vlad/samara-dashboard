using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Web;
using ChildRights.Analysis.Application.Universities.Commands;
using ChildRights.Analysis.Application.Universities.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ChildRights.Analysis.Api.Controllers;

[Route("api/universities")]
public sealed class UniversitiesController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>Lists universities in the catalogue.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? region, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new ListUniversitiesQuery(region), cancellationToken));

    /// <summary>Lists university programmes (specialties), filtered by university or cluster.</summary>
    [HttpGet("programs")]
    public async Task<IActionResult> ListPrograms(
        [FromQuery] Guid? universityId,
        [FromQuery] string? cluster,
        CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new ListUniversityProgramsQuery(universityId, cluster), cancellationToken));

    /// <summary>
    /// Depersonalised demand report: for each specialty, explicit pupil interest plus
    /// data-driven candidates. Intended to be shared with universities and communities.
    /// </summary>
    [HttpGet("demand")]
    public async Task<IActionResult> Demand([FromQuery] Guid? universityId, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new GetUniversityDemandQuery(universityId), cancellationToken));
}
