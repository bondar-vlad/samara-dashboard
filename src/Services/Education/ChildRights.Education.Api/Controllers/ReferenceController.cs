using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Web;
using ChildRights.Education.Application.Reference.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ChildRights.Education.Api.Controllers;

[Route("api/reference")]
public sealed class ReferenceController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>
    /// Returns the 2027 reform reference dataset: all specialisation profiles (with group
    /// and direction) and all institution types (with the profiles each may offer).
    /// </summary>
    [HttpGet("reform")]
    public async Task<IActionResult> GetReform(CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new GetReformReferenceQuery(), cancellationToken));
}
