namespace ChildRights.BuildingBlocks.Domain.SharedKernel;

/// <summary>
/// Canonical facts about institution types: their education direction, Ukrainian
/// display name, and which specialisation profiles each type may offer. Centralising
/// this keeps "which profiles can a фаховий коледж open?" answered the same way everywhere.
/// </summary>
public static class InstitutionTaxonomy
{
    private static readonly Dictionary<InstitutionType, EducationDirection> Directions = new()
    {
        [InstitutionType.Gymnasium] = EducationDirection.Basic,
        [InstitutionType.AcademicLyceum] = EducationDirection.Academic,
        [InstitutionType.ScientificLyceum] = EducationDirection.Academic,
        [InstitutionType.ArtLyceum] = EducationDirection.Academic,
        [InstitutionType.SportsLyceum] = EducationDirection.Academic,
        [InstitutionType.MilitaryLyceum] = EducationDirection.Academic,
        [InstitutionType.ProfessionalLyceum] = EducationDirection.Professional,
        [InstitutionType.ProfessionalCollege] = EducationDirection.Professional
    };

    private static readonly Dictionary<InstitutionType, string> Names = new()
    {
        [InstitutionType.Gymnasium] = "Гімназія",
        [InstitutionType.AcademicLyceum] = "Академічний ліцей",
        [InstitutionType.ProfessionalLyceum] = "Професійний ліцей",
        [InstitutionType.ProfessionalCollege] = "Фаховий коледж",
        [InstitutionType.ScientificLyceum] = "Науковий ліцей",
        [InstitutionType.ArtLyceum] = "Мистецький ліцей",
        [InstitutionType.SportsLyceum] = "Спортивний ліцей",
        [InstitutionType.MilitaryLyceum] = "Військовий ліцей"
    };

    public static IReadOnlyList<InstitutionType> All { get; } = Enum.GetValues<InstitutionType>();

    public static EducationDirection DirectionOf(InstitutionType type) => Directions[type];

    public static string Localize(InstitutionType type) => Names[type];

    /// <summary>True for lyceums/colleges that deliver grade 10–12 profile education.</summary>
    public static bool ProvidesProfileEducation(InstitutionType type) =>
        DirectionOf(type) != EducationDirection.Basic;

    /// <summary>The profiles an institution of this type may offer, derived from its direction.</summary>
    public static IReadOnlyList<EducationProfile> OfferedProfiles(InstitutionType type) =>
        ProfileTaxonomy.ForDirection(DirectionOf(type));
}
