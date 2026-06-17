namespace ChildRights.BuildingBlocks.Domain.Primitives;

/// <summary>
/// Marks the consistency boundary of a cluster of entities.
/// Only aggregate roots are retrieved and persisted by repositories.
/// </summary>
public abstract class AggregateRoot : Entity
{
    protected AggregateRoot(Guid id) : base(id)
    {
    }

    protected AggregateRoot()
    {
    }
}
