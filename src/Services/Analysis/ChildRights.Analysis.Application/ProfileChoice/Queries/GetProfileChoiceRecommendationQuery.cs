using System.Globalization;
using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Rules;

namespace ChildRights.Analysis.Application.ProfileChoice.Queries;

/// <summary>
/// One pupil's 10th-grade profile analysis: ranks reform clusters from the pupil's topic/subject
/// grades, lists the strongest profiles within the recommended cluster, and reports whether the
/// recommendation matches the cluster the pupil desires. Self-contained (no stored run needed).
/// </summary>
public sealed record GetProfileChoiceRecommendationQuery(Guid StudentId)
    : IQuery<ProfileChoiceRecommendationDto>;

internal sealed class GetProfileChoiceRecommendationQueryHandler(IEducationDataClient educationClient)
    : IQueryHandler<GetProfileChoiceRecommendationQuery, ProfileChoiceRecommendationDto>
{
    public async Task<Result<ProfileChoiceRecommendationDto>> Handle(
        GetProfileChoiceRecommendationQuery query,
        CancellationToken cancellationToken)
    {
        var profile = await educationClient.GetStudentProfileAsync(query.StudentId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<ProfileChoiceRecommendationDto>(
                Error.NotFound($"Student '{query.StudentId}' was not found in the education service."));
        }

        var subjectAverages = profile.SubjectAverages.ToDictionary(s => s.Subject, s => s.Average);
        var topicAverages = profile.TopicAverages
            .Select(t => new TopicScore(t.Subject, t.Topic, t.Average))
            .ToList();

        var clusterScores = ProfileScoringMap.ScoreClusters(subjectAverages, topicAverages)
            .OrderByDescending(kv => kv.Value)
            .ToList();
        var profileScores = ProfileScoringMap.ScoreProfiles(subjectAverages, topicAverages);

        ProfileCluster? recommended = clusterScores.Count > 0 ? clusterScores[0].Key : null;
        var desired = ProfileTaxonomy.TryParseCluster(profile.ProfileChoice.DesiredCluster);

        // Strongest profiles within the recommended cluster (several allowed within a cluster).
        var recommendedProfiles = recommended is { } rc
            ? profileScores
                .Where(kv => ProfileTaxonomy.ClusterOf(kv.Key) == rc)
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .Take(3)
                .ToList()
            : [];

        if (recommendedProfiles.Count == 0 && recommended is { } rc2)
        {
            recommendedProfiles = ProfileTaxonomy.ProfilesInCluster(rc2).Take(2).ToList();
        }

        // The profiles the pupil chose, as reported by the Education service.
        var desiredProfiles = profile.ProfileChoice.DesiredProfiles
            .Select(p => ProfileTaxonomy.TryParse(p.Profile))
            .Where(p => p is not null)
            .Select(p => p!.Value)
            .ToList();

        var dto = new ProfileChoiceRecommendationDto(
            query.StudentId,
            desired?.ToString(),
            desired is { } d ? ProfileTaxonomy.LocalizeCluster(d) : null,
            desiredProfiles.Select(ToProfileRef).ToList(),
            recommended?.ToString(),
            recommended is { } r ? ProfileTaxonomy.LocalizeCluster(r) : null,
            recommendedProfiles.Select(ToProfileRef).ToList(),
            desired is not null,
            desired is not null && desired == recommended,
            recommended is not null ? Confidence(clusterScores.Select(c => c.Value).ToList()) : null,
            clusterScores
                .Select(c => new ClusterScoreDto(c.Key.ToString(), ProfileTaxonomy.LocalizeCluster(c.Key), c.Value))
                .ToList(),
            BuildRationale(profile, recommended));

        return Result.Success(dto);
    }

    private static ProfileRefDto ToProfileRef(EducationProfile profile)
    {
        var cluster = ProfileTaxonomy.ClusterOf(profile);
        return new ProfileRefDto(
            profile.ToString(),
            ProfileTaxonomy.Localize(profile),
            cluster.ToString(),
            ProfileTaxonomy.LocalizeCluster(cluster));
    }

    /// <summary>Confidence from the separation between the top two cluster scores.</summary>
    private static double Confidence(IReadOnlyList<double> scores)
    {
        if (scores.Count == 0)
        {
            return 0;
        }

        if (scores.Count < 2 || scores[0] + scores[1] <= 0)
        {
            return 0.75;
        }

        return Math.Clamp(scores[0] / (scores[0] + scores[1]), 0.5, 0.95);
    }

    private static string BuildRationale(EducationStudentProfile profile, ProfileCluster? recommended)
    {
        var topTopics = profile.TopicAverages
            .OrderByDescending(t => t.Average)
            .Take(3)
            .Select(t => $"{t.Topic} ({t.Average.ToString("0.#", CultureInfo.InvariantCulture)})")
            .ToList();

        var basis = topTopics.Count > 0
            ? string.Join(", ", topTopics)
            : string.Join(", ", profile.SubjectAverages
                .OrderByDescending(s => s.Average)
                .Take(3)
                .Select(s => $"{s.Subject} ({s.Average.ToString("0.#", CultureInfo.InvariantCulture)})"));

        if (recommended is { } rc && basis.Length > 0)
        {
            return $"Найсильніші напрями за оцінками: {basis}. За цими сигналами система рекомендує " +
                   $"кластер «{ProfileTaxonomy.LocalizeCluster(rc)}».";
        }

        return "Недостатньо оцінок для впевненої рекомендації профілю.";
    }
}
