using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Education.Application.Students;

/// <summary>Builds the localized profile DTOs from the shared taxonomy.</summary>
internal static class ProfileMapping
{
    public static ProfileRefDto ToRef(EducationProfile profile)
    {
        var cluster = ProfileTaxonomy.ClusterOf(profile);
        return new ProfileRefDto(
            profile.ToString(),
            ProfileTaxonomy.Localize(profile),
            cluster.ToString(),
            ProfileTaxonomy.LocalizeCluster(cluster));
    }

    public static IReadOnlyCollection<ProfileRefDto> ToRefs(IEnumerable<EducationProfile> profiles) =>
        profiles.Select(ToRef).ToList();

    public static ProfileChoiceDto ToChoice(
        EducationProfile? declared,
        ProfileCluster? desiredCluster,
        IReadOnlyList<EducationProfile> desiredProfiles,
        ProfileCluster? recommendedCluster,
        IReadOnlyList<EducationProfile> recommendedProfiles,
        double? confidence,
        DateTime? updatedUtc,
        bool hasMismatch) =>
        new(
            declared?.ToString(),
            desiredCluster?.ToString(),
            desiredCluster is { } dc ? ProfileTaxonomy.LocalizeCluster(dc) : null,
            ToRefs(desiredProfiles),
            recommendedCluster?.ToString(),
            recommendedCluster is { } rc ? ProfileTaxonomy.LocalizeCluster(rc) : null,
            ToRefs(recommendedProfiles),
            confidence,
            updatedUtc,
            hasMismatch);
}
