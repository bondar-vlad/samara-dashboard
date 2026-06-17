namespace ChildRights.BuildingBlocks.Domain.SharedKernel;

/// <summary>
/// Canonical mapping and localisation helpers for the reform profile taxonomy:
/// the <b>direction → cluster → profile</b> hierarchy plus Ukrainian display names.
/// A pupil enrols into one cluster and may choose several profiles within it, so the
/// cluster is the unit at which selection and mismatch are evaluated.
/// </summary>
public static class ProfileTaxonomy
{
    private static readonly Dictionary<EducationProfile, ProfileCluster> ProfileToCluster = new()
    {
        [EducationProfile.NaturalMathematical] = ProfileCluster.AcademicNaturalMathematical,
        [EducationProfile.SocialHumanitarian] = ProfileCluster.AcademicSocialHumanitarian,
        [EducationProfile.Construction] = ProfileCluster.ProfessionalTechnical,
        [EducationProfile.EngineeringTechnological] = ProfileCluster.ProfessionalTechnical,
        [EducationProfile.TransportLogistics] = ProfileCluster.ProfessionalTechnical,
        [EducationProfile.InformationTechnology] = ProfileCluster.ProfessionalInformationTechnology,
        [EducationProfile.Agricultural] = ProfileCluster.ProfessionalLifeSciences,
        [EducationProfile.Medical] = ProfileCluster.ProfessionalLifeSciences,
        [EducationProfile.BusinessAdministration] = ProfileCluster.ProfessionalBusinessServices,
        [EducationProfile.HospitalityEvents] = ProfileCluster.ProfessionalBusinessServices,
        [EducationProfile.BeautyServicesDesign] = ProfileCluster.ProfessionalBusinessServices,
        [EducationProfile.EducationalHumanitarian] = ProfileCluster.ProfessionalHumanitarian
    };

    private static readonly Dictionary<ProfileCluster, EducationDirection> ClusterToDirection = new()
    {
        [ProfileCluster.AcademicNaturalMathematical] = EducationDirection.Academic,
        [ProfileCluster.AcademicSocialHumanitarian] = EducationDirection.Academic,
        [ProfileCluster.ProfessionalTechnical] = EducationDirection.Professional,
        [ProfileCluster.ProfessionalInformationTechnology] = EducationDirection.Professional,
        [ProfileCluster.ProfessionalLifeSciences] = EducationDirection.Professional,
        [ProfileCluster.ProfessionalBusinessServices] = EducationDirection.Professional,
        [ProfileCluster.ProfessionalHumanitarian] = EducationDirection.Professional
    };

    private static readonly Dictionary<EducationProfile, string> ProfileNames = new()
    {
        [EducationProfile.NaturalMathematical] = "Природничо-математичний (STEM)",
        [EducationProfile.SocialHumanitarian] = "Суспільно-гуманітарний",
        [EducationProfile.Agricultural] = "Аграрний",
        [EducationProfile.Construction] = "Будівельний",
        [EducationProfile.TransportLogistics] = "Транспортно-логістичний",
        [EducationProfile.EngineeringTechnological] = "Інженерно-технологічний",
        [EducationProfile.Medical] = "Медичний",
        [EducationProfile.InformationTechnology] = "ІТ (інформаційні технології)",
        [EducationProfile.BusinessAdministration] = "Бізнес та адміністрування",
        [EducationProfile.EducationalHumanitarian] = "Освітньо-гуманітарний",
        [EducationProfile.HospitalityEvents] = "Гостинність та організація подій",
        [EducationProfile.BeautyServicesDesign] = "Послуги краси та дизайн"
    };

    private static readonly Dictionary<ProfileCluster, string> ClusterNames = new()
    {
        [ProfileCluster.AcademicNaturalMathematical] = "Природничо-математичний (STEM)",
        [ProfileCluster.AcademicSocialHumanitarian] = "Суспільно-гуманітарний",
        [ProfileCluster.ProfessionalTechnical] = "Технічний та інженерний",
        [ProfileCluster.ProfessionalInformationTechnology] = "Інформаційні технології",
        [ProfileCluster.ProfessionalLifeSciences] = "Природничо-аграрний та медичний",
        [ProfileCluster.ProfessionalBusinessServices] = "Бізнес та сфера послуг",
        [ProfileCluster.ProfessionalHumanitarian] = "Освітньо-гуманітарний"
    };

    public static IReadOnlyList<EducationProfile> All { get; } = Enum.GetValues<EducationProfile>();

    public static IReadOnlyList<ProfileCluster> AllClusters { get; } = Enum.GetValues<ProfileCluster>();

    public static ProfileCluster ClusterOf(EducationProfile profile) => ProfileToCluster[profile];

    public static EducationDirection DirectionOfCluster(ProfileCluster cluster) => ClusterToDirection[cluster];

    public static EducationDirection DirectionOf(EducationProfile profile) =>
        ClusterToDirection[ProfileToCluster[profile]];

    public static string Localize(EducationProfile profile) => ProfileNames[profile];

    public static string LocalizeCluster(ProfileCluster cluster) => ClusterNames[cluster];

    /// <summary>The profiles that belong to a cluster (the selectable options within it).</summary>
    public static IReadOnlyList<EducationProfile> ProfilesInCluster(ProfileCluster cluster) =>
        All.Where(profile => ProfileToCluster[profile] == cluster).ToList();

    public static IReadOnlyList<ProfileCluster> ClustersForDirection(EducationDirection direction) =>
        direction == EducationDirection.Basic
            ? []
            : AllClusters.Where(cluster => ClusterToDirection[cluster] == direction).ToList();

    public static IReadOnlyList<EducationProfile> ForDirection(EducationDirection direction) =>
        direction == EducationDirection.Basic
            ? []
            : All.Where(profile => DirectionOf(profile) == direction).ToList();

    /// <summary>True when every profile in the set belongs to the same cluster (and the set is non-empty).</summary>
    public static bool AreInSameCluster(IEnumerable<EducationProfile> profiles)
    {
        var clusters = profiles.Select(ClusterOf).Distinct().ToList();
        return clusters.Count == 1;
    }

    /// <summary>Tolerant parse of a stored/declared profile name (enum name) to the enum value.</summary>
    public static EducationProfile? TryParse(string? value) =>
        Enum.TryParse<EducationProfile>(value, ignoreCase: true, out var parsed) ? parsed : null;

    /// <summary>Tolerant parse of a stored cluster name (enum name) to the enum value.</summary>
    public static ProfileCluster? TryParseCluster(string? value) =>
        Enum.TryParse<ProfileCluster>(value, ignoreCase: true, out var parsed) ? parsed : null;
}
