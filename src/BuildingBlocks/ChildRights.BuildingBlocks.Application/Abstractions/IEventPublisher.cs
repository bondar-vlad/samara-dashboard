namespace ChildRights.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Port for publishing integration events to the message bus. The concrete
/// transport (RabbitMQ via MassTransit, or in-memory) lives in the infrastructure layer.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class;
}
