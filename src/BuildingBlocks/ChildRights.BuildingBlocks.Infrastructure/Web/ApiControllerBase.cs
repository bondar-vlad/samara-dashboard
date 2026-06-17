using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChildRights.BuildingBlocks.Infrastructure.Web;

/// <summary>
/// Base controller shared by every service. Translates the <see cref="Result"/> pattern
/// into HTTP responses with consistent status codes and ProblemDetails errors.
/// </summary>
[ApiController]
public abstract class ApiControllerBase(IDispatcher dispatcher) : ControllerBase
{
    protected IDispatcher Dispatcher => dispatcher;

    protected IActionResult ToResult(Result result) =>
        result.IsSuccess ? Ok() : ToProblem(result.Error);

    protected IActionResult ToResult<TValue>(Result<TValue> result) =>
        result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);

    private IActionResult ToProblem(Error error)
    {
        var status = error.Code switch
        {
            "not_found" => StatusCodes.Status404NotFound,
            "validation" => StatusCodes.Status400BadRequest,
            "conflict" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(statusCode: status, title: error.Code, detail: error.Description);
    }
}
