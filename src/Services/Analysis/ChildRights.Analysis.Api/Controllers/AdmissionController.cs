using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.BuildingBlocks.Infrastructure.Web;
using ChildRights.Analysis.Application.Admission.Commands;
using ChildRights.Analysis.Application.Admission.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ChildRights.Analysis.Api.Controllers;

/// <summary>
/// The admission ("second analysis") API: the 4th-НМТ-subject widget and the
/// admission-direction widget. The original profile-analysis endpoints are untouched.
/// </summary>
[Route("api/admission")]
public sealed class AdmissionController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    // ---- Reference / catalogue ----

    /// <summary>Lists the НМТ subjects (mandatory + 4th-subject options).</summary>
    [HttpGet("nmt-subjects")]
    public async Task<IActionResult> NmtSubjects(CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new ListNmtSubjectsQuery(), cancellationToken));

    /// <summary>Lists admission directions with their specialties and НМТ coefficients.</summary>
    [HttpGet("directions")]
    public async Task<IActionResult> Directions([FromQuery] string? cluster, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new ListAdmissionDirectionsQuery(cluster), cancellationToken));

    // ---- Pupil inputs (НМТ scores, chosen 4th subject, desired direction) ----

    /// <summary>Submits/updates a pupil's admission inputs (any subset).</summary>
    [HttpPut("students/{studentId:guid}/choice")]
    public async Task<IActionResult> SubmitChoice(
        Guid studentId,
        [FromBody] SubmitAdmissionChoiceRequest request,
        CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Send(
            new SubmitAdmissionChoiceCommand(
                studentId, request.NmtScores, request.ChosenFourthSubject, request.DesiredDirectionCode),
            cancellationToken));

    // ---- Widget 1: 4th НМТ subject ----

    /// <summary>Recommends the 4th НМТ subject for a pupil and compares it with their choice.</summary>
    [HttpGet("students/{studentId:guid}/fourth-subject")]
    public async Task<IActionResult> FourthSubject(Guid studentId, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new GetFourthSubjectRecommendationQuery(studentId), cancellationToken));

    /// <summary>Lists pupils of a school with their chosen vs recommended 4th subject (match / not-match).</summary>
    [HttpGet("schools/{schoolId:guid}/fourth-subject-students")]
    public async Task<IActionResult> FourthSubjectStudents(Guid schoolId, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new ListFourthSubjectStudentsQuery(schoolId), cancellationToken));

    // ---- Widget 2: admission direction ----

    /// <summary>Recommends the admission direction for a pupil (НМТ coefficients + profile topics).</summary>
    [HttpGet("students/{studentId:guid}/direction")]
    public async Task<IActionResult> Direction(Guid studentId, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new GetDirectionRecommendationQuery(studentId), cancellationToken));

    /// <summary>Lists pupils of a school with their chosen vs recommended direction (match / not-match).</summary>
    [HttpGet("schools/{schoolId:guid}/direction-students")]
    public async Task<IActionResult> DirectionStudents(Guid schoolId, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new ListDirectionStudentsQuery(schoolId), cancellationToken));
}

/// <summary>Request body for submitting a pupil's admission inputs (all parts optional).</summary>
public sealed record SubmitAdmissionChoiceRequest(
    IReadOnlyDictionary<NmtSubject, int>? NmtScores,
    NmtSubject? ChosenFourthSubject,
    string? DesiredDirectionCode);
