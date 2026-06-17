using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Contracts.Analysis;
using ChildRights.Contracts.Notifications;
using ChildRights.Notifications.Api.Domain;
using ChildRights.Notifications.Api.Persistence;
using MassTransit;

namespace ChildRights.Notifications.Api.Messaging;

/// <summary>
/// Turns a raised red flag into concrete dispatches: one notification per target
/// audience, plus — for Red severity — formal inter-agency referrals that are
/// published back onto the bus (and picked up by, e.g., Social Services).
/// </summary>
public sealed class RedFlagRaisedConsumer(NotificationsDbContext context, IEventPublisher eventPublisher)
    : IConsumer<RedFlagRaisedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<RedFlagRaisedIntegrationEvent> context_)
    {
        var message = context_.Message;

        foreach (var audience in message.TargetAudiences)
        {
            context.Notifications.Add(new Notification(
                Guid.NewGuid(),
                message.FlagId,
                message.SubjectId,
                message.SubjectName,
                audience.ToString(),
                message.Severity.ToString(),
                message.Title,
                BuildMessage(message),
                DateTime.UtcNow));
        }

        if (message.Severity == FlagSeverity.Red)
        {
            foreach (var toAgency in ResolveReferralAgencies(message.TargetAudiences))
            {
                var referral = new Referral(
                    Guid.NewGuid(),
                    message.SourceAgency,
                    toAgency,
                    message.SubjectId,
                    message.SubjectName,
                    message.Severity.ToString(),
                    $"{message.Title}: {message.Description}",
                    DateTime.UtcNow);

                context.Referrals.Add(referral);

                await eventPublisher.PublishAsync(
                    new InterAgencyReferralRequestedIntegrationEvent
                    {
                        ReferralId = referral.Id,
                        FromAgency = message.SourceAgency,
                        ToAgency = toAgency,
                        SubjectId = message.SubjectId,
                        SubjectName = message.SubjectName,
                        Severity = message.Severity,
                        Reason = referral.Reason
                    },
                    context_.CancellationToken);
            }
        }

        await context.SaveChangesAsync(context_.CancellationToken);
    }

    private static string BuildMessage(RedFlagRaisedIntegrationEvent message)
    {
        var actions = message.RecommendedActions.Count > 0
            ? " Рекомендовані дії: " + string.Join("; ", message.RecommendedActions) + "."
            : string.Empty;
        return message.Description + actions;
    }

    private static IEnumerable<Agency> ResolveReferralAgencies(IReadOnlyCollection<AudienceRole> audiences)
    {
        var agencies = new HashSet<Agency>();
        if (audiences.Contains(AudienceRole.SocialService))
        {
            agencies.Add(Agency.SocialServices);
        }

        if (audiences.Contains(AudienceRole.JuvenilePolice))
        {
            agencies.Add(Agency.JuvenilePolice);
        }

        if (audiences.Contains(AudienceRole.MedicalService))
        {
            agencies.Add(Agency.Medical);
        }

        return agencies;
    }
}
