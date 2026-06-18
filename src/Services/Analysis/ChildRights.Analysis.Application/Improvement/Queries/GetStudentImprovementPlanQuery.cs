using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Admission;
using ChildRights.Analysis.Domain.Improvement;
using ChildRights.Analysis.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Improvement.Queries;

/// <summary>
/// Builds an AI-powered improvement plan for a pupil who has <b>made a choice</b> (a profile in
/// grades ≤10 or an admission direction in grade 11) — "what to pull up, in which subjects and
/// topics" to make that choice realistic. The measured gaps are computed deterministically; the
/// concrete advice comes from an AI coach. If no AI model is connected, the result is returned
/// with <c>Available=false</c> and a "model not connected" message (no canned fallback).
/// </summary>
public sealed record GetStudentImprovementPlanQuery(Guid StudentId) : IQuery<StudentImprovementPlanDto>;

internal sealed class GetStudentImprovementPlanQueryHandler(
    IEducationDataClient educationClient,
    IAnalysisDbContext context,
    IImprovementCoach coach) : IQueryHandler<GetStudentImprovementPlanQuery, StudentImprovementPlanDto>
{
    private const string ModelNotConnectedMessage = "Відповідна модель ШІ для аналізу не підключена.";
    private const string NoChoiceMessage = "Учень ще не зробив вибір — немає цілі, до якої будувати план підтягування.";

    public async Task<Result<StudentImprovementPlanDto>> Handle(
        GetStudentImprovementPlanQuery query,
        CancellationToken cancellationToken)
    {
        var profile = await educationClient.GetStudentProfileAsync(query.StudentId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<StudentImprovementPlanDto>(
                Error.NotFound($"Student '{query.StudentId}' was not found in the education service."));
        }

        var subjectAverages = profile.SubjectAverages.ToDictionary(s => s.Subject, s => s.Average);
        var topicAverages = profile.TopicAverages
            .Select(t => new TopicScore(t.Subject, t.Topic, t.Average))
            .ToList();
        var strengths = subjectAverages
            .OrderByDescending(kv => kv.Value)
            .Take(3)
            .Select(kv => kv.Key)
            .ToList();

        var target = profile.GradeLevel >= StudentRiskRules.GraduatingGradeLevel
            ? await ResolveDirectionTargetAsync(query.StudentId, subjectAverages, topicAverages, cancellationToken)
            : ResolveProfileTarget(profile, subjectAverages, topicAverages);

        // No choice yet → there is no target to pull up toward.
        if (!target.HasChoice)
        {
            return new StudentImprovementPlanDto(
                query.StudentId, coach.IsAvailable, coach.ModelName,
                target.Kind, target.Name, false, false, string.Empty, [], [], NoChoiceMessage);
        }

        var gaps = ImprovementPlanBuilder.Build(
            subjectAverages, topicAverages, target.KeySubjects, target.KeyTopics);

        // This feature is AI-driven by design: with no model connected we say so explicitly
        // instead of falling back to canned advice.
        if (!coach.IsAvailable)
        {
            return NotConnected(query.StudentId, target);
        }

        try
        {
            var result = await coach.CoachAsync(
                new ImprovementCoachRequest(profile.GradeLevel, target.Kind, target.Name, gaps, strengths),
                cancellationToken);

            var adviceByName = result.Items
                .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Advice, StringComparer.OrdinalIgnoreCase);

            var items = gaps
                .Select(g => new ImprovementItemDto(
                    g.Area, g.Name, g.Current, g.Target, g.Gap,
                    adviceByName.GetValueOrDefault(g.Name, string.Empty)))
                .ToList();

            return new StudentImprovementPlanDto(
                query.StudentId, true, coach.ModelName, target.Kind, target.Name,
                true, target.IsMismatch, result.Summary, items, result.Steps, null);
        }
        catch
        {
            // Any problem talking to the model → surface "not connected" (per product rule).
            return NotConnected(query.StudentId, target);
        }
    }

    private StudentImprovementPlanDto NotConnected(Guid studentId, TargetContext target) =>
        new(studentId, false, coach.ModelName, target.Kind, target.Name,
            true, target.IsMismatch, string.Empty, [], [], ModelNotConnectedMessage);

    private async Task<TargetContext> ResolveDirectionTargetAsync(
        Guid studentId,
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages,
        CancellationToken cancellationToken)
    {
        var choice = await context.StudentAdmissionChoices
            .FirstOrDefaultAsync(c => c.StudentId == studentId, cancellationToken);
        var directions = await context.AdmissionDirections.ToListAsync(cancellationToken);

        var desiredCode = choice?.DesiredDirectionCode;
        var hasChoice = !string.IsNullOrWhiteSpace(desiredCode);
        var desired = directions.FirstOrDefault(d => d.Code == desiredCode);

        var recommendation = AdmissionDirectionRecommender.Recommend(
            directions,
            choice?.NmtScores ?? new Dictionary<NmtSubject, int>(),
            subjectAverages,
            topicAverages);

        var isMismatch = hasChoice
            && recommendation.RecommendedCode is { } recommended
            && recommended != desiredCode;

        return new TargetContext(
            "direction",
            desired?.Name ?? desiredCode ?? string.Empty,
            hasChoice,
            isMismatch,
            desired?.KeySubjects ?? [],
            desired?.KeyTopics ?? []);
    }

    private static TargetContext ResolveProfileTarget(
        EducationStudentProfile profile,
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages)
    {
        var desiredProfiles = profile.ProfileChoice.DesiredProfiles
            .Select(p => ProfileTaxonomy.TryParse(p.Profile))
            .Where(p => p is not null)
            .Select(p => p!.Value)
            .ToList();

        if (desiredProfiles.Count == 0)
        {
            return new TargetContext("profile", string.Empty, false, false, [], []);
        }

        var desiredCluster = ProfileTaxonomy.ClusterOf(desiredProfiles[0]);
        var recommendedCluster = ProfileScoringMap.ScoreClusters(subjectAverages, topicAverages)
            .OrderByDescending(kv => kv.Value)
            .Select(kv => (ProfileCluster?)kv.Key)
            .FirstOrDefault();

        var isMismatch = recommendedCluster is { } recommended && recommended != desiredCluster;
        var (keySubjects, keyTopics) = ProfileScoringMap.KeySignalsForCluster(desiredCluster);

        return new TargetContext(
            "profile",
            ProfileTaxonomy.LocalizeCluster(desiredCluster),
            true,
            isMismatch,
            keySubjects,
            keyTopics);
    }

    /// <summary>The pupil's chosen target resolved for either stage (profile or direction).</summary>
    private sealed record TargetContext(
        string Kind,
        string Name,
        bool HasChoice,
        bool IsMismatch,
        IReadOnlyList<string> KeySubjects,
        IReadOnlyList<string> KeyTopics);
}
