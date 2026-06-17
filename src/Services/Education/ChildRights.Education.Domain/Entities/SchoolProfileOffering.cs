using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Education.Domain.Entities;

/// <summary>A specialisation profile that a particular institution offers (grade 10–12).</summary>
public sealed class SchoolProfileOffering : Entity
{
    private SchoolProfileOffering()
    {
    }

    public SchoolProfileOffering(Guid id, Guid schoolId, EducationProfile profile)
        : base(id)
    {
        SchoolId = schoolId;
        Profile = profile;
    }

    public Guid SchoolId { get; private set; }

    public EducationProfile Profile { get; private set; }
}
