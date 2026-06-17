using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Education.Domain.Entities;

/// <summary>
/// An educational institution. Its <see cref="InstitutionType"/> determines the reform
/// <see cref="EducationDirection"/> (academic vs professional) and therefore which
/// specialisation profiles it may offer.
/// </summary>
public sealed class School : Entity
{
    private readonly List<SchoolProfileOffering> _offeredProfiles = [];

    private School()
    {
    }

    public School(Guid id, string name, string community, string region, InstitutionType institutionType)
        : base(id)
    {
        Name = name;
        Community = community;
        Region = region;
        InstitutionType = institutionType;
    }

    public string Name { get; private set; } = string.Empty;

    public string Community { get; private set; } = string.Empty;

    public string Region { get; private set; } = string.Empty;

    public InstitutionType InstitutionType { get; private set; }

    /// <summary>The reform direction implied by the institution type.</summary>
    public EducationDirection Direction => InstitutionTaxonomy.DirectionOf(InstitutionType);

    public IReadOnlyCollection<SchoolProfileOffering> OfferedProfiles => _offeredProfiles.AsReadOnly();

    /// <summary>Adds a profile to the institution's offering if the type permits it and it is not already present.</summary>
    public void OfferProfile(EducationProfile profile)
    {
        if (!InstitutionTaxonomy.OfferedProfiles(InstitutionType).Contains(profile))
        {
            throw new InvalidOperationException(
                $"{InstitutionTaxonomy.Localize(InstitutionType)} cannot offer the '{profile}' profile.");
        }

        if (_offeredProfiles.All(o => o.Profile != profile))
        {
            _offeredProfiles.Add(new SchoolProfileOffering(Guid.NewGuid(), Id, profile));
        }
    }
}
