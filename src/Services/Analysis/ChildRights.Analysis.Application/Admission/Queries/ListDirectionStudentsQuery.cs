using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Admission;
using ChildRights.Analysis.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Admission.Queries;

/// <summary>
/// Widget 2 list: for a school, every pupil with their desired admission direction, the
/// recommended one, and whether they match — plus the per-direction distribution.
/// </summary>
public sealed record ListDirectionStudentsQuery(Guid SchoolId) : IQuery<DirectionStudentsResultDto>;

public sealed record DirectionStudentsResultDto(
    IReadOnlyCollection<DirectionStudentDto> Students,
    IReadOnlyCollection<DirectionDistributionItemDto> Distribution);

internal sealed class ListDirectionStudentsQueryHandler(
    IAnalysisDbContext context,
    IEducationDataClient educationClient)
    : IQueryHandler<ListDirectionStudentsQuery, DirectionStudentsResultDto>
{
    public async Task<Result<DirectionStudentsResultDto>> Handle(
        ListDirectionStudentsQuery query,
        CancellationToken cancellationToken)
    {
        var directions = await context.AdmissionDirections.ToListAsync(cancellationToken);
        var directionNames = directions.ToDictionary(d => d.Code, d => d.Name);

        // Admission (НМТ) concerns only graduating pupils (11th grade); younger pupils are still
        // in the profile-choice stage and must not appear in this widget.
        var students = (await educationClient.GetStudentsAsync(query.SchoolId, cancellationToken))
            .Where(s => s.GradeLevel >= StudentRiskRules.GraduatingGradeLevel)
            .ToList();
        var choices = await context.StudentAdmissionChoices
            .Where(c => c.SchoolId == query.SchoolId)
            .ToDictionaryAsync(c => c.StudentId, c => c, cancellationToken);

        var rows = new List<DirectionStudentDto>();
        var chosenCounts = new Dictionary<string, int>();
        var recommendedCounts = new Dictionary<string, int>();

        foreach (var student in students)
        {
            var profile = await educationClient.GetStudentProfileAsync(student.Id, cancellationToken);
            if (profile is null)
            {
                continue;
            }

            var subjectAverages = profile.SubjectAverages.ToDictionary(s => s.Subject, s => s.Average);
            var topicAverages = profile.TopicAverages.Select(t => new TopicScore(t.Subject, t.Topic, t.Average)).ToList();

            choices.TryGetValue(student.Id, out var choice);
            var nmtScores = choice?.NmtScores ?? [];
            var recommended = AdmissionDirectionRecommender
                .Recommend(directions, nmtScores, subjectAverages, topicAverages).RecommendedCode;

            var desired = choice?.DesiredDirectionCode;

            if (desired is not null)
            {
                chosenCounts[desired] = chosenCounts.GetValueOrDefault(desired) + 1;
            }

            if (recommended is not null)
            {
                recommendedCounts[recommended] = recommendedCounts.GetValueOrDefault(recommended) + 1;
            }

            rows.Add(new DirectionStudentDto(
                student.Id,
                student.FullName,
                student.ClassName,
                desired,
                recommended,
                desired is not null && desired == recommended));
        }

        var distribution = directions
            .Select(d => new DirectionDistributionItemDto(
                d.Code,
                d.Name,
                chosenCounts.GetValueOrDefault(d.Code),
                recommendedCounts.GetValueOrDefault(d.Code)))
            .OrderByDescending(d => d.ChosenCount)
            .ToList();

        return Result.Success(new DirectionStudentsResultDto(
            rows.OrderBy(r => r.FullName).ToList(), distribution));
    }
}
