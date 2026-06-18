using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Infrastructure.Web;
using ChildRights.Analysis.Application.ProfileChoice.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ChildRights.Analysis.Api.Controllers;

/// <summary>
/// The 10th-grade profile-choice API: which reform profile (cluster) a pupil chooses when entering
/// the profile high school, versus what their grades recommend. This is a separate decision from
/// the 11th-grade НМТ admission flow handled by <see cref="AdmissionController"/>.
/// </summary>
[Route("api/profile")]
public sealed class ProfileController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>Lists a school's profile-choosing pupils (grades below the graduating year)
    /// with their desired vs recommended cluster, plus the per-cluster distribution.</summary>
    [HttpGet("schools/{schoolId:guid}/students")]
    public async Task<IActionResult> ProfileStudents(Guid schoolId, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new ListProfileChoiceStudentsQuery(schoolId), cancellationToken));

    /// <summary>Analyses one pupil's profile choice (ranked clusters + recommended profiles).</summary>
    [HttpGet("students/{studentId:guid}")]
    public async Task<IActionResult> ProfileStudent(Guid studentId, CancellationToken cancellationToken)
        => ToResult(await Dispatcher.Query(new GetProfileChoiceRecommendationQuery(studentId), cancellationToken));
}
