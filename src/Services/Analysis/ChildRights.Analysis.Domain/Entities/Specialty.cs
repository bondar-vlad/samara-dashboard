using ChildRights.BuildingBlocks.Domain.Primitives;

namespace ChildRights.Analysis.Domain.Entities;

/// <summary>
/// A specialty (спеціальність) belonging to an <see cref="AdmissionDirection"/>. One direction
/// has many specialties — universities realise these specialties, applicants pick the direction.
/// </summary>
public sealed class Specialty : Entity
{
    private Specialty()
    {
    }

    public Specialty(Guid id, Guid directionId, string code, string name)
        : base(id)
    {
        DirectionId = directionId;
        Code = code;
        Name = name;
    }

    public Guid DirectionId { get; private set; }

    /// <summary>National specialty code (e.g. "121" Інженерія програмного забезпечення).</summary>
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;
}
