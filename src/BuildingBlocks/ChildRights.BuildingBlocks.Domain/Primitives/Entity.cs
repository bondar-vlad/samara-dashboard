namespace ChildRights.BuildingBlocks.Domain.Primitives;

/// <summary>
/// Base class for all entities. Provides identity equality and a
/// transient collection of domain events for the infrastructure to dispatch.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity(Guid id) => Id = id;

    // Required by EF Core materialization.
    protected Entity()
    {
    }

    public Guid Id { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public bool Equals(Entity? other) => other is not null && other.Id == Id && other.GetType() == GetType();

    public override bool Equals(object? obj) => obj is Entity entity && Equals(entity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
