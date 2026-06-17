namespace ChildRights.Education.Application.Reference;

/// <summary>Reference item for a reform profile (for dropdowns and validation on the frontend).</summary>
public sealed record ProfileReferenceDto(
    string Profile,
    string ProfileName,
    string Cluster,
    string ClusterName,
    string Direction);

/// <summary>
/// Reference item for a reform cluster and the profiles it groups. A pupil enrols into
/// a cluster and may select several of its profiles.
/// </summary>
public sealed record ClusterReferenceDto(
    string Cluster,
    string ClusterName,
    string Direction,
    IReadOnlyCollection<ProfileReferenceDto> Profiles);

/// <summary>Reference item for an institution type and the profiles it may offer.</summary>
public sealed record InstitutionTypeReferenceDto(
    string InstitutionType,
    string InstitutionTypeName,
    string Direction,
    bool ProvidesProfileEducation,
    IReadOnlyCollection<string> EligibleProfiles);

/// <summary>The complete reform reference dataset returned to clients in one call.</summary>
public sealed record ReformReferenceDto(
    IReadOnlyCollection<ClusterReferenceDto> Clusters,
    IReadOnlyCollection<ProfileReferenceDto> Profiles,
    IReadOnlyCollection<InstitutionTypeReferenceDto> InstitutionTypes);
