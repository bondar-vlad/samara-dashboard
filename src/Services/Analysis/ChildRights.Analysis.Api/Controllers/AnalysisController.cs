using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.BuildingBlocks.Infrastructure.Web;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Application.Improvement.Queries;
using ChildRights.Analysis.Application.Runs.Commands;
using ChildRights.Analysis.Application.Runs.Queries;
using ChildRights.Analysis.Application.Universities.Commands;
using ChildRights.Analysis.Application.Universities.Queries;
using ChildRights.Analysis.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ChildRights.Analysis.Api.Controllers;

[Route("api/analysis")]
public sealed class AnalysisController(IDispatcher dispatcher, IAiAnalysisProviderFactory providerFactory)
    : ApiControllerBase(dispatcher)
{
    /// <summary>Lists the analysis models available to this deployment.</summary>
    [HttpGet("models")]
    public IActionResult GetModels() => Ok(providerFactory.AvailableModels);

    /// <summary>
    /// Runs an on-demand analysis for a single pupil. When <paramref name="kind"/> is omitted the
    /// pupil's grade decides: grades ≤10 get the profile analysis, grade 11+ get the admission
    /// analysis (4th НМТ subject + direction). Pass <c>kind</c> to override (Profile / Admission / All).
    /// </summary>
    [HttpPost("students/{studentId:guid}/run")]
    public async Task<IActionResult> RunStudent(
        Guid studentId,
        [FromQuery] string? model,
        [FromQuery] AnalysisKind? kind = null,
        CancellationToken cancellationToken = default)
        => ToResult(await Dispatcher.Send(
            new RunStudentAnalysisCommand(studentId, AnalysisTrigger.OnDemand, model, kind), cancellationToken));

    /// <summary>Runs a school-wide analysis and aggregates institution/community recommendations.</summary>
    [HttpPost("schools/{schoolId:guid}/run")]
    public async Task<IActionResult> RunSchool(
        Guid schoolId,
        [FromQuery] string? model,
        CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Send(
            new RunSchoolAnalysisCommand(schoolId, AnalysisTrigger.OnDemand, model), cancellationToken));

    /// <summary>Lists recent analysis runs (audit trail).</summary>
    [HttpGet("runs")]
    public async Task<IActionResult> GetRuns([FromQuery] int take = 50, CancellationToken cancellationToken = default)
        => ToResult(await Dispatcher.Query(new ListAnalysisRunsQuery(take), cancellationToken));

    /// <summary>Whether a pupil has been analysed yet (drives the "run analysis" prompt + button).</summary>
    [HttpGet("students/{studentId:guid}/status")]
    public async Task<IActionResult> StudentStatus(Guid studentId, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new GetStudentAnalysisStatusQuery(studentId), cancellationToken));

    /// <summary>
    /// Ranks university specialties by how well the pupil fits them, with concrete topic/subject
    /// improvement advice for each. The pupil-facing "which university suits me" view.
    /// </summary>
    [HttpGet("students/{studentId:guid}/university-fit")]
    public async Task<IActionResult> UniversityFit(
        Guid studentId,
        [FromQuery] int take = 5,
        [FromQuery] string? cluster = null,
        CancellationToken cancellationToken = default)
        => ToResult(await Dispatcher.Query(new GetStudentUniversityFitQuery(studentId, take, cluster), cancellationToken));

    /// <summary>Gap analysis for one chosen specialty: exactly what to improve and by how much.</summary>
    [HttpGet("students/{studentId:guid}/university-fit/{programId:guid}")]
    public async Task<IActionResult> ProgramGap(Guid studentId, Guid programId, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new GetStudentProgramGapQuery(studentId, programId), cancellationToken));

    /// <summary>
    /// AI improvement plan ("що підтягнути") toward the pupil's <b>chosen</b> profile (grades ≤10)
    /// or admission direction (grade 11) — for when the pupil keeps a choice the data flags as a
    /// mismatch. AI-only: when no model is connected the result reports it (Available=false)
    /// rather than inventing advice.
    /// </summary>
    [HttpGet("students/{studentId:guid}/improvement-plan")]
    public async Task<IActionResult> ImprovementPlan(Guid studentId, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new GetStudentImprovementPlanQuery(studentId), cancellationToken));

    /// <summary>Records the pupil's interest in a specialty (feeds depersonalised university demand).</summary>
    [HttpPost("students/{studentId:guid}/program-interest/{programId:guid}")]
    public async Task<IActionResult> ExpressInterest(Guid studentId, Guid programId, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Send(new ExpressProgramInterestCommand(studentId, programId), cancellationToken));
}
