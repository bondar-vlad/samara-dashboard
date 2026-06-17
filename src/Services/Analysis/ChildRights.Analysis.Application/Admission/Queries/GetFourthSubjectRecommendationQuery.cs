using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Admission;
using ChildRights.Analysis.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Admission.Queries;

/// <summary>
/// Widget 1: recommends the pupil's 4th НМТ subject from their grades/topics and reports
/// whether it matches the subject the pupil chose.
/// </summary>
public sealed record GetFourthSubjectRecommendationQuery(Guid StudentId) : IQuery<FourthSubjectRecommendationDto>;

internal sealed class GetFourthSubjectRecommendationQueryHandler(
    IAnalysisDbContext context,
    IEducationDataClient educationClient)
    : IQueryHandler<GetFourthSubjectRecommendationQuery, FourthSubjectRecommendationDto>
{
    public async Task<Result<FourthSubjectRecommendationDto>> Handle(
        GetFourthSubjectRecommendationQuery query,
        CancellationToken cancellationToken)
    {
        var profile = await educationClient.GetStudentProfileAsync(query.StudentId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<FourthSubjectRecommendationDto>(
                Error.NotFound($"Student '{query.StudentId}' was not found in the education service."));
        }

        var subjectAverages = profile.SubjectAverages.ToDictionary(s => s.Subject, s => s.Average);
        var topicAverages = profile.TopicAverages.Select(t => new TopicScore(t.Subject, t.Topic, t.Average)).ToList();

        var recommendation = FourthSubjectRecommender.Recommend(subjectAverages, topicAverages);

        var choice = await context.StudentAdmissionChoices
            .FirstOrDefaultAsync(c => c.StudentId == query.StudentId, cancellationToken);
        var chosen = choice?.ChosenFourthSubject;

        var dto = new FourthSubjectRecommendationDto(
            query.StudentId,
            chosen?.ToString(),
            chosen is { } cs ? NmtSubjectCatalog.Localize(cs) : null,
            recommendation.Recommended?.ToString(),
            recommendation.Recommended is { } rec ? NmtSubjectCatalog.Localize(rec) : null,
            chosen is not null,
            chosen is not null && chosen == recommendation.Recommended,
            recommendation.Ranked.Select(AdmissionMapping.ToDto).ToList(),
            recommendation.Rationale);

        return Result.Success(dto);
    }
}
