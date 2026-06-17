using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Rules;
using ChildRights.Analysis.Domain.Universities;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Universities.Queries;

/// <summary>
/// Ranks university programmes (specialties) by how well a pupil currently fits them and,
/// for each, lists the concrete subjects/topics to improve. This is the pupil-facing
/// "which university suits me and what should I improve" view.
/// </summary>
public sealed record GetStudentUniversityFitQuery(Guid StudentId, int Take = 5, string? Cluster = null)
    : IQuery<StudentUniversityFitDto>;

internal sealed class GetStudentUniversityFitQueryHandler(
    IAnalysisDbContext context,
    IEducationDataClient educationClient) : IQueryHandler<GetStudentUniversityFitQuery, StudentUniversityFitDto>
{
    public async Task<Result<StudentUniversityFitDto>> Handle(
        GetStudentUniversityFitQuery query,
        CancellationToken cancellationToken)
    {
        var profile = await educationClient.GetStudentProfileAsync(query.StudentId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<StudentUniversityFitDto>(
                Error.NotFound($"Student '{query.StudentId}' was not found in the education service."));
        }

        var subjectAverages = profile.SubjectAverages.ToDictionary(s => s.Subject, s => s.Average);
        var topicAverages = profile.TopicAverages.Select(t => new TopicScore(t.Subject, t.Topic, t.Average)).ToList();

        // Determine the pupil's data-driven cluster to focus and label the result.
        var recommendedCluster = ProfileScoringMap.ScoreClusters(subjectAverages, topicAverages)
            .OrderByDescending(kv => kv.Value)
            .Select(kv => (ProfileCluster?)kv.Key)
            .FirstOrDefault();

        var programs = await context.UniversityPrograms.ToListAsync(cancellationToken);

        if (ProfileTaxonomy.TryParseCluster(query.Cluster) is { } filterCluster)
        {
            programs = programs.Where(p => p.Cluster == filterCluster).ToList();
        }

        var matches = programs
            .Select(program => new
            {
                program,
                fit = UniversityFitCalculator.Evaluate(program, subjectAverages, topicAverages)
            })
            .OrderByDescending(x => x.fit.FitScore)
            .Take(Math.Clamp(query.Take, 1, 20))
            .Select(x => UniversityProgramMapping.ToFitDto(x.program, x.fit))
            .ToList();

        var clusterName = recommendedCluster is { } rc ? ProfileTaxonomy.LocalizeCluster(rc) : string.Empty;

        return new StudentUniversityFitDto(
            query.StudentId,
            recommendedCluster?.ToString() ?? string.Empty,
            clusterName,
            matches);
    }
}
