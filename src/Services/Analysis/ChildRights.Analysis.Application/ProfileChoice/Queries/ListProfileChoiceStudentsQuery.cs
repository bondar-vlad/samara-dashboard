using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Rules;

namespace ChildRights.Analysis.Application.ProfileChoice.Queries;

/// <summary>
/// 10th-grade profile widget list: for a school, every pupil who is still choosing a profile
/// (grade &lt; 11) with their desired cluster, the data-recommended cluster, and whether they
/// match — plus the per-cluster distribution. The recommended cluster is computed on the fly
/// from the pupil's topic/subject grades, so the widget works without a stored analysis run.
/// </summary>
public sealed record ListProfileChoiceStudentsQuery(Guid SchoolId)
    : IQuery<ProfileChoiceStudentsResultDto>;

internal sealed class ListProfileChoiceStudentsQueryHandler(IEducationDataClient educationClient)
    : IQueryHandler<ListProfileChoiceStudentsQuery, ProfileChoiceStudentsResultDto>
{
    public async Task<Result<ProfileChoiceStudentsResultDto>> Handle(
        ListProfileChoiceStudentsQuery query,
        CancellationToken cancellationToken)
    {
        // Profile choice concerns pupils who have not yet graduated into admission (grade < 11).
        var students = (await educationClient.GetStudentsAsync(query.SchoolId, cancellationToken))
            .Where(s => s.GradeLevel < StudentRiskRules.GraduatingGradeLevel)
            .ToList();

        var rows = new List<ProfileChoiceStudentDto>();
        var chosenCounts = new Dictionary<ProfileCluster, int>();
        var recommendedCounts = new Dictionary<ProfileCluster, int>();

        foreach (var student in students)
        {
            var profile = await educationClient.GetStudentProfileAsync(student.Id, cancellationToken);
            if (profile is null)
            {
                continue;
            }

            var subjectAverages = profile.SubjectAverages.ToDictionary(s => s.Subject, s => s.Average);
            var topicAverages = profile.TopicAverages
                .Select(t => new TopicScore(t.Subject, t.Topic, t.Average))
                .ToList();

            var clusterScores = ProfileScoringMap.ScoreClusters(subjectAverages, topicAverages);
            ProfileCluster? recommended = clusterScores.Count > 0
                ? clusterScores.OrderByDescending(kv => kv.Value).First().Key
                : null;

            var desired = ProfileTaxonomy.TryParseCluster(profile.ProfileChoice.DesiredCluster);

            if (desired is { } dc)
            {
                chosenCounts[dc] = chosenCounts.GetValueOrDefault(dc) + 1;
            }

            if (recommended is { } rc)
            {
                recommendedCounts[rc] = recommendedCounts.GetValueOrDefault(rc) + 1;
            }

            rows.Add(new ProfileChoiceStudentDto(
                student.Id,
                student.FullName,
                student.ClassName,
                desired?.ToString(),
                desired is { } d ? ProfileTaxonomy.LocalizeCluster(d) : null,
                recommended?.ToString(),
                recommended is { } r ? ProfileTaxonomy.LocalizeCluster(r) : null,
                desired is not null,
                desired is not null && desired == recommended));
        }

        var distribution = ProfileTaxonomy.AllClusters
            .Select(c => new ProfileChoiceDistributionItemDto(
                c.ToString(),
                ProfileTaxonomy.LocalizeCluster(c),
                chosenCounts.GetValueOrDefault(c),
                recommendedCounts.GetValueOrDefault(c)))
            .OrderByDescending(d => d.ChosenCount)
            .ToList();

        return Result.Success(new ProfileChoiceStudentsResultDto(
            rows.OrderBy(r => r.FullName).ToList(), distribution));
    }
}
