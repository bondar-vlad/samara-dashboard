using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Education.Application.Reference.Queries;

/// <summary>
/// Returns the static reform reference dataset (all profiles and institution types).
/// Pure taxonomy — no database access — so the frontend can populate dropdowns and
/// validate profile choices against the 2027 standard.
/// </summary>
public sealed record GetReformReferenceQuery : IQuery<ReformReferenceDto>;

internal sealed class GetReformReferenceQueryHandler : IQueryHandler<GetReformReferenceQuery, ReformReferenceDto>
{
    public Task<Result<ReformReferenceDto>> Handle(GetReformReferenceQuery query, CancellationToken cancellationToken)
    {
        var profiles = ProfileTaxonomy.All.Select(ToProfileRef).ToList();

        var clusters = ProfileTaxonomy.AllClusters
            .Select(c => new ClusterReferenceDto(
                c.ToString(),
                ProfileTaxonomy.LocalizeCluster(c),
                ProfileTaxonomy.DirectionOfCluster(c).ToString(),
                ProfileTaxonomy.ProfilesInCluster(c).Select(ToProfileRef).ToList()))
            .ToList();

        var institutionTypes = InstitutionTaxonomy.All
            .Select(t => new InstitutionTypeReferenceDto(
                t.ToString(),
                InstitutionTaxonomy.Localize(t),
                InstitutionTaxonomy.DirectionOf(t).ToString(),
                InstitutionTaxonomy.ProvidesProfileEducation(t),
                InstitutionTaxonomy.OfferedProfiles(t).Select(p => p.ToString()).ToList()))
            .ToList();

        var reference = new ReformReferenceDto(clusters, profiles, institutionTypes);
        return Task.FromResult(Result.Success(reference));
    }

    private static ProfileReferenceDto ToProfileRef(EducationProfile profile)
    {
        var cluster = ProfileTaxonomy.ClusterOf(profile);
        return new ProfileReferenceDto(
            profile.ToString(),
            ProfileTaxonomy.Localize(profile),
            cluster.ToString(),
            ProfileTaxonomy.LocalizeCluster(cluster),
            ProfileTaxonomy.DirectionOf(profile).ToString());
    }
}
