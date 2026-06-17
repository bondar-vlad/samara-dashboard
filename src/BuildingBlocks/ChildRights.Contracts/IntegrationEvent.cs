namespace ChildRights.Contracts;

/// <summary>
/// Base type for every integration event published to the message bus.
/// Integration events are the public, versioned contract between microservices —
/// keep them additive and backwards compatible.
/// </summary>
public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
