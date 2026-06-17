using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Application.Common;
using ChildRights.Analysis.Domain.Entities;
using ChildRights.Analysis.Domain.Enums;

namespace ChildRights.Analysis.Application.Runs.Commands;

/// <summary>
/// School-level analysis: analyses every pupil, then aggregates the individual
/// profiling results into institution and community recommendations
/// (which profiles to open, which academy courses to create).
/// </summary>
public sealed record RunSchoolAnalysisCommand(
    Guid SchoolId,
    AnalysisTrigger Trigger = AnalysisTrigger.OnDemand,
    string? ModelName = null) : ICommand<SchoolAnalysisResultDto>;

internal sealed class RunSchoolAnalysisCommandHandler(
    IEducationDataClient educationClient,
    IAnalysisEngine engine,
    IAnalysisDbContext context,
    IClock clock) : ICommandHandler<RunSchoolAnalysisCommand, SchoolAnalysisResultDto>
{
    public async Task<Result<SchoolAnalysisResultDto>> Handle(
        RunSchoolAnalysisCommand command,
        CancellationToken cancellationToken)
    {
        var students = await educationClient.GetStudentsAsync(command.SchoolId, cancellationToken);
        if (students.Count == 0)
        {
            return Result.Failure<SchoolAnalysisResultDto>(
                Error.NotFound($"No pupils found for school '{command.SchoolId}'."));
        }

        var totalFlags = 0;
        var totalRecommendations = 0;
        var profileDemand = new Dictionary<string, int>();

        foreach (var student in students)
        {
            var result = await engine.AnalyzeStudentAsync(
                student.Id, command.Trigger, command.ModelName, cancellationToken: cancellationToken);

            totalFlags += result.FlagsProduced;
            totalRecommendations += result.RecommendationsProduced;

            foreach (var recommendation in result.Recommendations
                .Where(r => r.Kind is nameof(RecommendationKind.ProfileChoice) or nameof(RecommendationKind.ProfileChange)))
            {
                profileDemand[recommendation.Title] = profileDemand.GetValueOrDefault(recommendation.Title) + 1;
            }
        }

        var schoolRecommendations = new List<Recommendation>();
        if (profileDemand.Count > 0)
        {
            var topProfile = profileDemand.OrderByDescending(kv => kv.Value).First();

            schoolRecommendations.Add(new Recommendation(
                Guid.NewGuid(), AnalysisScope.School, command.SchoolId, "Заклад освіти",
                RecommendationKind.OpenProfile, "Відкрити профільний клас за найбільшим попитом",
                $"За результатами аналізу найбільший попит формує напрям «{topProfile.Key}» ({topProfile.Value} рекомендацій).",
                "Сформовано на основі індивідуальних профорієнтаційних висновків учнів закладу.",
                0.8, "aggregate-v1", clock.UtcNow));

            schoolRecommendations.Add(new Recommendation(
                Guid.NewGuid(), AnalysisScope.Community, command.SchoolId, "Громада / область",
                RecommendationKind.AcademyCourse, "Сформувати курс для академії неперервної освіти",
                "Запланувати курс підвищення кваліфікації вчителів за затребуваними профілями громади.",
                "Аналіз агрегованого попиту на профілі в межах громади та області.",
                0.7, "aggregate-v1", clock.UtcNow));

            context.Recommendations.AddRange(schoolRecommendations);
            await context.SaveChangesAsync(cancellationToken);
        }

        return new SchoolAnalysisResultDto(
            command.SchoolId,
            students.Count,
            totalFlags,
            totalRecommendations,
            schoolRecommendations.Select(r => r.ToDto()).ToList());
    }
}
