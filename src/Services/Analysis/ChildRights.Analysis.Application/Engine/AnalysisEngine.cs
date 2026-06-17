using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Contracts.Analysis;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Application.Common;
using ChildRights.Analysis.Domain.Entities;
using ChildRights.Analysis.Domain.Rules;
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

            var snapshot = new StudentSnapshot(
                profile.Id,
                profile.FullName,
                profile.SchoolId,
                profile.ClassId,
                profile.GradeLevel,
                profile.DeclaredProfile,
                profile.Attendance.Unexcused,
                profile.SubjectAverages.ToDictionary(s => s.Subject, s => s.Average));

            var result = await provider.AnalyzeAsync(new AnalysisRequest(snapshot), cancellationToken);

            var flags = result.Flags
                .Select(f => new RedFlag(
                    Guid.NewGuid(), f.RuleCode, AnalysisScope.Student, studentId, profile.FullName,
                    f.Severity, f.Title, f.Description, f.SourceAgency, f.TargetAudiences,
                    f.RecommendedActions, provider.ModelName, clock.UtcNow))
                .ToList();

            var recommendations = result.Recommendations
                .Select(r => new Recommendation(
                    Guid.NewGuid(), AnalysisScope.Student, studentId, profile.FullName,
                    r.Kind, r.Title, r.Summary, r.Rationale, r.Confidence, provider.ModelName, clock.UtcNow))
                .ToList();

            context.RedFlags.AddRange(flags);
            context.Recommendations.AddRange(recommendations);
            run.Complete(flags.Count, recommendations.Count, result.Summary, clock.UtcNow);
            await context.SaveChangesAsync(cancellationToken);

            await PublishAsync(flags, recommendations, cancellationToken);

            logger.LogInformation(
                "Analysis run {RunId} for student {StudentId} via {Model}: {Flags} flags, {Recs} recommendations.",
                run.Id, studentId, provider.ModelName, flags.Count, recommendations.Count);

            return new AnalysisRunResultDto(
                run.Id, provider.ModelName, run.Status.ToString(),
                flags.Count, recommendations.Count, result.Summary,
                flags.Select(f => f.ToDto()).ToList(),
                recommendations.Select(r => r.ToDto()).ToList());
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

    private static AnalysisRunResultDto EmptyResult(AnalysisRun run, string modelName) =>
        new(run.Id, modelName, run.Status.ToString(), 0, 0, run.Summary, [], []);
}
