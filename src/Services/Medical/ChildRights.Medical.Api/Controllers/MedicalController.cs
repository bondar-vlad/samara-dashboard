using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.Contracts.Medical;
using ChildRights.Medical.Api.Domain;
using ChildRights.Medical.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Medical.Api.Controllers;

[ApiController]
[Route("api/medical")]
public sealed class MedicalController(MedicalDbContext context, IEventPublisher eventPublisher) : ControllerBase
{
    /// <summary>How many visits of the same category raise a cross-agency medical concern.</summary>
    private const int ConcernThreshold = 3;

    /// <summary>Lists recorded medical visits, optionally for one pupil.</summary>
    [HttpGet("visits")]
    public async Task<IActionResult> List([FromQuery] Guid? studentId, CancellationToken cancellationToken)
    {
        var query = context.Visits.AsQueryable();
        if (studentId is { } id)
        {
            query = query.Where(v => v.StudentId == id);
        }

        var visits = await query
            .OrderByDescending(v => v.Date)
            .Select(v => new
            {
                v.Id,
                v.StudentId,
                v.StudentName,
                v.ConditionCategory,
                v.Date,
                v.Note
            })
            .ToListAsync(cancellationToken);

        return Ok(visits);
    }

    /// <summary>
    /// Records a medical visit. When the same condition recurs beyond the threshold the
    /// service raises a cross-agency medical concern for the Analysis service to pick up.
    /// </summary>
    [HttpPost("visits")]
    public async Task<IActionResult> Record([FromBody] RecordVisitRequest request, CancellationToken cancellationToken)
    {
        var visit = new MedicalVisit(
            Guid.NewGuid(), request.StudentId, request.StudentName, request.ConditionCategory, request.Date, request.Note);

        context.Visits.Add(visit);
        await context.SaveChangesAsync(cancellationToken);

        var occurrences = await context.Visits.CountAsync(
            v => v.StudentId == request.StudentId && v.ConditionCategory == request.ConditionCategory,
            cancellationToken);

        var concernRaised = occurrences >= ConcernThreshold;
        if (concernRaised)
        {
            await eventPublisher.PublishAsync(
                new MedicalConcernReportedIntegrationEvent
                {
                    StudentId = request.StudentId,
                    StudentName = request.StudentName,
                    ConditionCategory = request.ConditionCategory,
                    OccurrencesInPeriod = occurrences,
                    RecommendedReferral = $"Звернутися до профільного лікаря ({request.ConditionCategory})."
                },
                cancellationToken);
        }

        return Ok(new { visit.Id, OccurrencesInPeriod = occurrences, ConcernRaised = concernRaised });
    }
}

public sealed record RecordVisitRequest(Guid StudentId, string StudentName, string ConditionCategory, DateOnly Date, string? Note);
