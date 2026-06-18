using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Admission;
using ChildRights.Analysis.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Admission.Queries;

/// <summary>
/// Widget 1 list: for a school, every pupil with their chosen 4th НМТ subject, the recommended
/// one, and whether they match — plus the distribution (how many chose each subject).
/// </summary>
public sealed record ListFourthSubjectStudentsQuery(Guid SchoolId)
    : IQuery<FourthSubjectStudentsResultDto>;

public sealed record FourthSubjectStudentsResultDto(
    IReadOnlyCollection<FourthSubjectStudentDto> Students,
    IReadOnlyCollection<FourthSubjectDistributionItemDto> Distribution);

internal sealed class ListFourthSubjectStudentsQueryHandler(
    IAnalysisDbContext context,
    IEducationDataClient educationClient)
    : IQueryHandler<ListFourthSubjectStudentsQuery, FourthSubjectStudentsResultDto>
{
    public async Task<Result<FourthSubjectStudentsResultDto>> Handle(
        ListFourthSubjectStudentsQuery query,
        CancellationToken cancellationToken)
    {
        // Admission (НМТ) concerns only graduating pupils (11th grade); younger pupils are still
        // in the profile-choice stage and must not appear in this widget.
        var students = (await educationClient.GetStudentsAsync(query.SchoolId, cancellationToken))
            .Where(s => s.GradeLevel >= StudentRiskRules.GraduatingGradeLevel)
            .ToList();
        var choices = await context.StudentAdmissionChoices
            .Where(c => c.SchoolId == query.SchoolId)
            .ToDictionaryAsync(c => c.StudentId, c => c, cancellationToken);

        var rows = new List<FourthSubjectStudentDto>();
        var chosenCounts = new Dictionary<NmtSubject, int>();
        var recommendedCounts = new Dictionary<NmtSubject, int>();

        foreach (var student in students)
        {
            var profile = await educationClient.GetStudentProfileAsync(student.Id, cancellationToken);
            if (profile is null)
            {
                continue;
            }

            var subjectAverages = profile.SubjectAverages.ToDictionary(s => s.Subject, s => s.Average);
            var topicAverages = profile.TopicAverages.Select(t => new TopicScore(t.Subject, t.Topic, t.Average)).ToList();
            var recommended = FourthSubjectRecommender.Recommend(subjectAverages, topicAverages).Recommended;

            choices.TryGetValue(student.Id, out var choice);
            var chosen = choice?.ChosenFourthSubject;

            if (chosen is { } c)
            {
                chosenCounts[c] = chosenCounts.GetValueOrDefault(c) + 1;
            }

            if (recommended is { } r)
            {
                recommendedCounts[r] = recommendedCounts.GetValueOrDefault(r) + 1;
            }

            rows.Add(new FourthSubjectStudentDto(
                student.Id,
                student.FullName,
                student.ClassName,
                chosen?.ToString(),
                recommended?.ToString(),
                chosen is not null && chosen == recommended));
        }

        var distribution = NmtSubjectCatalog.FourthSubjectOptions
            .Select(s => new FourthSubjectDistributionItemDto(
                s.ToString(),
                NmtSubjectCatalog.Localize(s),
                chosenCounts.GetValueOrDefault(s),
                recommendedCounts.GetValueOrDefault(s)))
            .ToList();

        return Result.Success(new FourthSubjectStudentsResultDto(
            rows.OrderBy(r => r.FullName).ToList(), distribution));
    }
}
