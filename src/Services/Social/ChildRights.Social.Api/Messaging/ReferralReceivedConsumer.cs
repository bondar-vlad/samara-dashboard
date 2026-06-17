using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Contracts.Notifications;
using ChildRights.Social.Api.Domain;
using ChildRights.Social.Api.Persistence;
using MassTransit;

namespace ChildRights.Social.Api.Messaging;

/// <summary>
/// Closes the inter-agency loop: when a referral is addressed to social services,
/// automatically open a social case. (Education → Analysis → Notifications → Social.)
/// </summary>
public sealed class ReferralReceivedConsumer(SocialDbContext context)
    : IConsumer<InterAgencyReferralRequestedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<InterAgencyReferralRequestedIntegrationEvent> context_)
    {
        var message = context_.Message;
        if (message.ToAgency != Agency.SocialServices)
        {
            return;
        }

        var socialCase = new SocialCase(
            Guid.NewGuid(),
            message.SubjectId,
            message.SubjectName,
            message.FromAgency.ToString(),
            message.Severity.ToString(),
            message.Reason,
            DateOnly.FromDateTime(DateTime.UtcNow));

        context.Cases.Add(socialCase);
        await context.SaveChangesAsync(context_.CancellationToken);
    }
}
