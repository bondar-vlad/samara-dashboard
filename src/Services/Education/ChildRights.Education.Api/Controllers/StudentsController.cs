using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Web;
using ChildRights.Education.Api.Models;
using ChildRights.Education.Application.Students.Commands;
using ChildRights.Education.Application.Students.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ChildRights.Education.Api.Controllers;

[Route("api/students")]
public sealed class StudentsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>Lists pupils, optionally filtered by school and/or class.</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? schoolId,
        [FromQuery] Guid? classId,
        CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new ListStudentsQuery(schoolId, classId), cancellationToken));

    /// <summary>Returns a pupil's profile: attendance summary and subject averages.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProfile(Guid id, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new GetStudentProfileQuery(id), cancellationToken));

    /// <summary>Records an attendance entry and evaluates the attendance red-flag policy.</summary>
    [HttpPost("{id:guid}/attendance")]
    public async Task<IActionResult> RecordAttendance(
        Guid id,
        [FromBody] RecordAttendanceRequest request,
        CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Send(
            new RecordAttendanceCommand(id, request.Date, request.Status, request.Subject),
            cancellationToken));

    /// <summary>Records a subject grade for a pupil.</summary>
    [HttpPost("{id:guid}/grades")]
    public async Task<IActionResult> RecordGrade(
        Guid id,
        [FromBody] RecordGradeRequest request,
        CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Send(
            new RecordGradeCommand(id, request.Subject, request.Value, request.Term),
            cancellationToken));
}
