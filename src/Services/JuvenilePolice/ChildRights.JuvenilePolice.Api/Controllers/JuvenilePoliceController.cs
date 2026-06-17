using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Contracts.JuvenilePolice;
using ChildRights.JuvenilePolice.Api.Domain;
using ChildRights.JuvenilePolice.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.JuvenilePolice.Api.Controllers;

[ApiController]
[Route("api/juvenile")]
public sealed class JuvenilePoliceController(JuvenilePoliceDbContext context, IEventPublisher eventPublisher)
    : ControllerBase
{
    /// <summary>Lists filed bullying reports, optionally for a specific class.</summary>
    [HttpGet("bullying-reports")]
    public async Task<IActionResult> List([FromQuery] Guid? classId, CancellationToken cancellationToken)
    {
        var query = context.BullyingReports.AsQueryable();
        if (classId is { } id)
        {
            query = query.Where(r => r.ClassId == id);
        }

        var reports = await query
            .OrderByDescending(r => r.FiledOn)
            .Select(r => new
            {
                r.Id,
                r.ClassId,
                r.SchoolId,
                Severity = r.Severity.ToString(),
                r.Summary,
                r.FiledOn
            })
            .ToListAsync(cancellationToken);

        return Ok(reports);
    }

    /// <summary>Files a bullying report and broadcasts it as a class-level signal for analysis.</summary>
    [HttpPost("bullying-reports")]
    public async Task<IActionResult> File([FromBody] FileBullyingReportRequest request, CancellationToken cancellationToken)
    {
        var report = new BullyingReport(
            Guid.NewGuid(),
            request.ClassId,
            request.SchoolId,
            request.Severity,
            request.Summary,
            DateOnly.FromDateTime(DateTime.UtcNow));

        context.BullyingReports.Add(report);
        await context.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new BullyingReportFiledIntegrationEvent
            {
                ReportId = report.Id,
                ClassId = report.ClassId,
                SchoolId = report.SchoolId,
                Severity = report.Severity,
                Summary = report.Summary
            },
            cancellationToken);

        return Ok(new { report.Id });
    }
}

public sealed record FileBullyingReportRequest(Guid ClassId, Guid SchoolId, FlagSeverity Severity, string Summary);
