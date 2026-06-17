namespace ChildRights.BuildingBlocks.Domain.Primitives;

/// <summary>
/// Base class for value objects: immutable, compared by structural value
/// rather than identity.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other) =>
        other is not null && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj) => obj is ValueObject other && Equals(other);

    public override int GetHashCode() =>
        GetEqualityComponents().Aggregate(0, (hash, component) => HashCode.Combine(hash, component));
}
