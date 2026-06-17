using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Contracts.Notifications;
using ChildRights.Notifications.Api.Domain;
using ChildRights.Notifications.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Notifications.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(NotificationsDbContext context, IEventPublisher eventPublisher)
    : ControllerBase
{
    /// <summary>Lists dispatched notifications, optionally for one child.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? subjectId, CancellationToken cancellationToken)
    {
        var query = context.Notifications.AsQueryable();
        if (subjectId is { } id)
        {
            query = query.Where(n => n.SubjectId == id);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .Select(n => new
            {
                n.Id,
                n.FlagId,
                n.SubjectId,
                n.SubjectName,
                n.Audience,
                n.Severity,
                n.Title,
                n.Message,
                n.Status,
                n.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(notifications);
    }

    /// <summary>Lists inter-agency referrals, optionally filtered by destination agency.</summary>
    [HttpGet("/api/referrals")]
    public async Task<IActionResult> ListReferrals([FromQuery] Agency? toAgency, CancellationToken cancellationToken)
    {
        var query = context.Referrals.AsQueryable();
        if (toAgency is { } agency)
        {
            query = query.Where(r => r.ToAgency == agency);
        }

        var referrals = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new
            {
                r.Id,
                FromAgency = r.FromAgency.ToString(),
                ToAgency = r.ToAgency.ToString(),
                r.SubjectId,
                r.SubjectName,
                r.Severity,
                r.Reason,
                r.Status,
                r.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(referrals);
    }

    /// <summary>Manually raises an inter-agency referral and publishes it onto the bus.</summary>
    [HttpPost("/api/referrals")]
    public async Task<IActionResult> CreateReferral(
        [FromBody] CreateReferralRequest request,
        CancellationToken cancellationToken)
    {
        var referral = new Referral(
            Guid.NewGuid(),
            request.FromAgency,
            request.ToAgency,
            request.SubjectId,
            request.SubjectName,
            request.Severity.ToString(),
            request.Reason,
            DateTime.UtcNow);

        context.Referrals.Add(referral);
        await context.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new InterAgencyReferralRequestedIntegrationEvent
            {
                ReferralId = referral.Id,
                FromAgency = request.FromAgency,
                ToAgency = request.ToAgency,
                SubjectId = request.SubjectId,
                SubjectName = request.SubjectName,
                Severity = request.Severity,
                Reason = request.Reason
            },
            cancellationToken);

        return Ok(new { referral.Id });
    }
}

public sealed record CreateReferralRequest(
    Agency FromAgency,
    Agency ToAgency,
    Guid SubjectId,
    string SubjectName,
    FlagSeverity Severity,
    string Reason);
