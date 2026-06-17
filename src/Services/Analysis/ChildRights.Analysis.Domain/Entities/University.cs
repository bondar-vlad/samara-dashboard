using ChildRights.BuildingBlocks.Domain.Primitives;

namespace ChildRights.Analysis.Domain.Entities;

/// <summary>A higher-education institution in the university catalogue.</summary>
public sealed class University : Entity
{
    private University()
    {
    }

    public University(Guid id, string name, string city, string region)
        : base(id)
    {
        Name = name;
        City = city;
        Region = region;
    }

    public string Name { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string Region { get; private set; } = string.Empty;
}
