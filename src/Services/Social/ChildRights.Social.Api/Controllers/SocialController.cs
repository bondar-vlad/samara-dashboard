using ChildRights.Social.Api.Domain;
using ChildRights.Social.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Social.Api.Controllers;

[ApiController]
[Route("api/social")]
public sealed class SocialController(SocialDbContext context) : ControllerBase
{
    /// <summary>Lists social-services cases, optionally for one child.</summary>
    [HttpGet("cases")]
    public async Task<IActionResult> List([FromQuery] Guid? subjectId, CancellationToken cancellationToken)
    {
        var query = context.Cases.AsQueryable();
        if (subjectId is { } id)
        {
            query = query.Where(c => c.SubjectId == id);
        }

        var cases = await query
            .OrderByDescending(c => c.OpenedOn)
            .Select(c => new
            {
                c.Id,
                c.SubjectId,
                c.SubjectName,
                c.SourceAgency,
                c.Severity,
                c.Reason,
                c.Status,
                c.OpenedOn
            })
            .ToListAsync(cancellationToken);

        return Ok(cases);
    }

    /// <summary>Manually opens a social-services case.</summary>
    [HttpPost("cases")]
    public async Task<IActionResult> Open([FromBody] OpenSocialCaseRequest request, CancellationToken cancellationToken)
    {
        var socialCase = new SocialCase(
            Guid.NewGuid(),
            request.SubjectId,
            request.SubjectName,
            "Manual",
            "Yellow",
            request.Reason,
            DateOnly.FromDateTime(DateTime.UtcNow));

        context.Cases.Add(socialCase);
        await context.SaveChangesAsync(cancellationToken);

        return Ok(new { socialCase.Id });
    }
}

public sealed record OpenSocialCaseRequest(Guid SubjectId, string SubjectName, string Reason);
