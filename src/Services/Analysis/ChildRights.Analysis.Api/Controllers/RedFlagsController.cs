using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Web;
using ChildRights.Analysis.Application.Flags.Commands;
using ChildRights.Analysis.Application.Flags.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ChildRights.Analysis.Api.Controllers;

[Route("api/red-flags")]
public sealed class RedFlagsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>Lists red flags filtered by severity, scope, subject and/or status.</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? severity,
        [FromQuery] string? scope,
        [FromQuery] Guid? subjectId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new ListRedFlagsQuery(severity, scope, subjectId, status), cancellationToken));

    /// <summary>Acknowledges a red flag.</summary>
    [HttpPost("{id:guid}/acknowledge")]
    public async Task<IActionResult> Acknowledge(Guid id, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Send(new AcknowledgeRedFlagCommand(id), cancellationToken));

    /// <summary>Resolves a red flag.</summary>
    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Send(new ResolveRedFlagCommand(id), cancellationToken));
}
