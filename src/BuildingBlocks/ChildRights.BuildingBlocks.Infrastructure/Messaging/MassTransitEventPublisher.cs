using ChildRights.BuildingBlocks.Application.Abstractions;
using MassTransit;

namespace ChildRights.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Publishes integration events through MassTransit's transport (RabbitMQ or in-memory).</summary>
internal sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class
        => publishEndpoint.Publish(integrationEvent, cancellationToken);
}
