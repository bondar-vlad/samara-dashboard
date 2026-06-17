using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Admission;
using ChildRights.Analysis.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Admission.Queries;

/// <summary>
/// Widget 2: ranks admission directions for a pupil (конкурсний бал from НМТ × coefficients,
/// blended with topic fit) and reports whether the top one matches the desired direction.
/// </summary>
public sealed record GetDirectionRecommendationQuery(Guid StudentId) : IQuery<DirectionRecommendationDto>;

internal sealed class GetDirectionRecommendationQueryHandler(
    IAnalysisDbContext context,
    IEducationDataClient educationClient)
    : IQueryHandler<GetDirectionRecommendationQuery, DirectionRecommendationDto>
{
    public async Task<Result<DirectionRecommendationDto>> Handle(
        GetDirectionRecommendationQuery query,
        CancellationToken cancellationToken)
    {
        var profile = await educationClient.GetStudentProfileAsync(query.StudentId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<DirectionRecommendationDto>(
                Error.NotFound($"Student '{query.StudentId}' was not found in the education service."));
        }

        var choice = await context.StudentAdmissionChoices
            .FirstOrDefaultAsync(c => c.StudentId == query.StudentId, cancellationToken);

        var nmtScores = choice?.NmtScores ?? [];
        var subjectAverages = profile.SubjectAverages.ToDictionary(s => s.Subject, s => s.Average);
        var topicAverages = profile.TopicAverages.Select(t => new TopicScore(t.Subject, t.Topic, t.Average)).ToList();

        var directions = await context.AdmissionDirections.ToListAsync(cancellationToken);
        var recommendation = AdmissionDirectionRecommender.Recommend(
            directions, nmtScores, subjectAverages, topicAverages);

        var desiredCode = choice?.DesiredDirectionCode;
        var directionNames = directions.ToDictionary(d => d.Code, d => d.Name);

        var dto = new DirectionRecommendationDto(
            query.StudentId,
            desiredCode,
            desiredCode is not null ? directionNames.GetValueOrDefault(desiredCode) : null,
            recommendation.RecommendedCode,
            recommendation.RecommendedCode is { } rc ? directionNames.GetValueOrDefault(rc) : null,
            desiredCode is not null,
            desiredCode is not null && desiredCode == recommendation.RecommendedCode,
            nmtScores.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
            recommendation.Ranked.Select(AdmissionMapping.ToDto).ToList(),
            recommendation.Rationale);

        return Result.Success(dto);
    }
}
