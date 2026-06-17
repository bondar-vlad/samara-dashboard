using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Contracts.Analysis;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Application.Common;
using ChildRights.Analysis.Domain.Admission;
using ChildRights.Analysis.Domain.Entities;
using ChildRights.Analysis.Domain.Enums;
using ChildRights.Analysis.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChildRights.Analysis.Application.Engine;

internal sealed class AnalysisEngine(
    IEducationDataClient educationClient,
    IAiAnalysisProviderFactory providerFactory,
    IAnalysisDbContext context,
    IEventPublisher eventPublisher,
    IClock clock,
    ILogger<AnalysisEngine> logger) : IAnalysisEngine
{
    public async Task<AnalysisRunResultDto> AnalyzeStudentAsync(
        Guid studentId,
        AnalysisTrigger trigger,
        string? modelName = null,
        AnalysisKind kind = AnalysisKind.Profile,
        CancellationToken cancellationToken = default)
    {
        var provider = providerFactory.Resolve(modelName);

        var run = new AnalysisRun(
            Guid.NewGuid(), trigger, AnalysisScope.Student, studentId, provider.ModelName, clock.UtcNow);
        context.AnalysisRuns.Add(run);
        await context.SaveChangesAsync(cancellationToken);

        try
        {
            var profile = await educationClient.GetStudentProfileAsync(studentId, cancellationToken);
            if (profile is null)
            {
                run.Fail($"Учня {studentId} не знайдено в освітньому сервісі.", clock.UtcNow);
                await context.SaveChangesAsync(cancellationToken);
                return EmptyResult(run, provider.ModelName);
            }

            var declaredProfile = ProfileTaxonomy.TryParse(profile.ProfileChoice.DeclaredProfile);
            var desiredProfiles = profile.ProfileChoice.DesiredProfiles
                .Select(p => ProfileTaxonomy.TryParse(p.Profile))
                .Where(p => p is not null)
                .Select(p => p!.Value)
                .ToList();

            var snapshot = new StudentSnapshot(
                profile.Id,
                profile.FullName,
                profile.SchoolId,
                profile.ClassId,
                profile.GradeLevel,
                declaredProfile,
                desiredProfiles,
                profile.Attendance.Unexcused,
                profile.SubjectAverages.ToDictionary(s => s.Subject, s => s.Average),
                profile.TopicAverages.Select(t => new TopicScore(t.Subject, t.Topic, t.Average)).ToList());

            var allFlags = new List<RedFlag>();
            var allRecommendations = new List<Recommendation>();
            var summaries = new List<string>();
            AnalysisResult? profileResult = null;

            RedFlag MapFlag(Domain.Rules.FlagFinding f) => new(
                Guid.NewGuid(), f.RuleCode, AnalysisScope.Student, studentId, profile.FullName,
                f.Severity, f.Title, f.Description, f.SourceAgency, f.TargetAudiences,
                f.RecommendedActions, provider.ModelName, clock.UtcNow);

            Recommendation MapRecommendation(Domain.Rules.RecommendationFinding r) => new(
                Guid.NewGuid(), AnalysisScope.Student, studentId, profile.FullName,
                r.Kind, r.Title, r.Summary, r.Rationale, r.Confidence, provider.ModelName, clock.UtcNow);

            // Pass 1 — profile-education analysis (the original behaviour, the default).
            if (kind is AnalysisKind.Profile or AnalysisKind.All)
            {
                profileResult = await provider.AnalyzeAsync(new AnalysisRequest(snapshot), cancellationToken);
                allFlags.AddRange(profileResult.Flags.Select(MapFlag));
                allRecommendations.AddRange(profileResult.Recommendations.Select(MapRecommendation));
                summaries.Add(profileResult.Summary);
            }

            // Pass 2 — admission analysis (4th НМТ subject + admission direction).
            if (kind is AnalysisKind.Admission or AnalysisKind.All)
            {
                var choice = await context.StudentAdmissionChoices
                    .FirstOrDefaultAsync(c => c.StudentId == studentId, cancellationToken);
                var directions = await context.AdmissionDirections.ToListAsync(cancellationToken);

                var admission = AdmissionRules.Evaluate(
                    snapshot.SubjectAverages,
                    snapshot.TopicAverages,
                    choice?.NmtScores ?? new Dictionary<NmtSubject, int>(),
                    choice?.ChosenFourthSubject,
                    choice?.DesiredDirectionCode,
                    directions);

                allFlags.AddRange(admission.Flags.Select(MapFlag));
                allRecommendations.AddRange(admission.Recommendations.Select(MapRecommendation));
                summaries.Add(
                    $"Вступний аналіз: {admission.Flags.Count} флаг(ів), {admission.Recommendations.Count} рекомендаці(й).");
            }

            var summary = string.Join(" ", summaries);
            context.RedFlags.AddRange(allFlags);
            context.Recommendations.AddRange(allRecommendations);
            run.Complete(allFlags.Count, allRecommendations.Count, summary, clock.UtcNow);
            await context.SaveChangesAsync(cancellationToken);

            if (profileResult is not null)
            {
                await UpsertProfileInsightAsync(snapshot, profileResult, cancellationToken);
                await PublishProfileRecommendationAsync(snapshot, profileResult, cancellationToken);
            }

            await PublishAsync(allFlags, allRecommendations, cancellationToken);

            logger.LogInformation(
                "Analysis run {RunId} ({Kind}) for student {StudentId} via {Model}: {Flags} flags, {Recs} recommendations.",
                run.Id, kind, studentId, provider.ModelName, allFlags.Count, allRecommendations.Count);

            return new AnalysisRunResultDto(
                run.Id, provider.ModelName, run.Status.ToString(),
                allFlags.Count, allRecommendations.Count, summary,
                allFlags.Select(f => f.ToDto()).ToList(),
                allRecommendations.Select(r => r.ToDto()).ToList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Analysis failed for student {StudentId}", studentId);
            run.Fail(ex.Message, clock.UtcNow);
            await context.SaveChangesAsync(cancellationToken);
            return EmptyResult(run, provider.ModelName);
        }
    }

    private async Task PublishAsync(
        IEnumerable<RedFlag> flags,
        IEnumerable<Recommendation> recommendations,
        CancellationToken cancellationToken)
    {
        foreach (var flag in flags)
        {
            await eventPublisher.PublishAsync(
                new RedFlagRaisedIntegrationEvent
                {
                    FlagId = flag.Id,
                    RuleCode = flag.RuleCode,
                    Scope = flag.Scope,
                    SubjectId = flag.SubjectId,
                    SubjectName = flag.SubjectName,
                    Severity = flag.Severity,
                    Title = flag.Title,
                    Description = flag.Description,
                    SourceAgency = flag.SourceAgency,
                    TargetAudiences = flag.TargetAudiences,
                    RecommendedActions = flag.RecommendedActions
                },
                cancellationToken);
        }

        foreach (var recommendation in recommendations)
        {
            await eventPublisher.PublishAsync(
                new RecommendationIssuedIntegrationEvent
                {
                    RecommendationId = recommendation.Id,
                    Scope = recommendation.Scope,
                    SubjectId = recommendation.SubjectId,
                    Kind = recommendation.Kind.ToString(),
                    Summary = recommendation.Summary
                },
                cancellationToken);
        }
    }

    /// <summary>
    /// Publishes the structured profile recommendation (cluster + profiles) so the Education
    /// service can write it back onto the pupil and establish the recommended-profile link.
    /// </summary>
    private async Task PublishProfileRecommendationAsync(
        StudentSnapshot snapshot,
        AnalysisResult result,
        CancellationToken cancellationToken)
    {
        var profiling = result.Recommendations.FirstOrDefault(r => r.RecommendedCluster is not null);
        if (profiling?.RecommendedCluster is not { } cluster)
        {
            return;
        }

        var profiles = (profiling.RecommendedProfiles ?? [])
            .Select(p => p.ToString())
            .ToList();

        await eventPublisher.PublishAsync(
            new StudentProfileRecommendedIntegrationEvent
            {
                StudentId = snapshot.StudentId,
                RecommendedCluster = cluster.ToString(),
                RecommendedProfiles = profiles,
                DesiredCluster = snapshot.DesiredCluster?.ToString(),
                Confidence = profiling.Confidence,
                IsMismatch = snapshot.DesiredCluster is not null && snapshot.DesiredCluster != cluster
            },
            cancellationToken);
    }

    /// <summary>
    /// Persists the pupil's latest profiling insight (recommended cluster/profiles + topic
    /// strengths) so the platform can produce depersonalised demand analytics for universities.
    /// </summary>
    private async Task UpsertProfileInsightAsync(
        StudentSnapshot snapshot,
        AnalysisResult result,
        CancellationToken cancellationToken)
    {
        var profiling = result.Recommendations.FirstOrDefault(r => r.RecommendedCluster is not null);
        if (profiling?.RecommendedCluster is not { } cluster)
        {
            return;
        }

        var profiles = profiling.RecommendedProfiles ?? [];
        var topicStrengths = snapshot.TopicAverages
            .Select(t => new TopicStrength(t.Subject, t.Topic, t.Average))
            .ToList();

        var existing = await context.StudentProfileInsights
            .FirstOrDefaultAsync(i => i.StudentId == snapshot.StudentId, cancellationToken);

        if (existing is null)
        {
            context.StudentProfileInsights.Add(new StudentProfileInsight(
                Guid.NewGuid(), snapshot.StudentId, snapshot.SchoolId, cluster, profiles,
                snapshot.DesiredCluster, profiling.Confidence, topicStrengths, clock.UtcNow));
        }
        else
        {
            existing.Update(cluster, profiles, snapshot.DesiredCluster, profiling.Confidence, topicStrengths, clock.UtcNow);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static AnalysisRunResultDto EmptyResult(AnalysisRun run, string modelName) =>
        new(run.Id, modelName, run.Status.ToString(), 0, 0, run.Summary, [], []);
}
