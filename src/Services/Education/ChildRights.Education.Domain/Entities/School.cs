using ChildRights.BuildingBlocks.Domain.Primitives;

namespace ChildRights.Education.Domain.Entities;

/// <summary>An educational institution (school).</summary>
public sealed class School : Entity
{
    private School()
    {
    }

    public School(Guid id, string name, string community, string region)
        : base(id)
    {
        Name = name;
        Community = community;
        Region = region;
    }

    public string Name { get; private set; } = string.Empty;

    public string Community { get; private set; } = string.Empty;

    public string Region { get; private set; } = string.Empty;
}
