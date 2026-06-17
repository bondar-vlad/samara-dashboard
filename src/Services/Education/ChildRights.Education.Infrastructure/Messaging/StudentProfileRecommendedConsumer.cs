using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Contracts.Analysis;
using ChildRights.Education.Application.Students.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChildRights.Education.Infrastructure.Messaging;

/// <summary>
/// Consumes profile recommendations from the Analysis service and writes them back onto
/// the pupil record, establishing the pupil↔recommended-profile link inside Education.
/// </summary>
public sealed class StudentProfileRecommendedConsumer(IDispatcher dispatcher, ILogger<StudentProfileRecommendedConsumer> logger)
    : IConsumer<StudentProfileRecommendedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<StudentProfileRecommendedIntegrationEvent> context)
    {
        var message = context.Message;

        var profiles = message.RecommendedProfiles
            .Select(ProfileTaxonomy.TryParse)
            .Where(p => p is not null)
            .Select(p => p!.Value)
            .ToList();

        if (profiles.Count == 0)
        {
            logger.LogWarning("Received no recognisable recommended profiles for student {StudentId}.",
                message.StudentId);
            return;
        }

        await dispatcher.Send(
            new ApplyProfileRecommendationCommand(message.StudentId, profiles, message.Confidence),
            context.CancellationToken);
    }
}
