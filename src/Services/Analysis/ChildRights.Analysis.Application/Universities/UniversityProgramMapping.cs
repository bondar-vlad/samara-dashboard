using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Entities;
using ChildRights.Analysis.Domain.Universities;

namespace ChildRights.Analysis.Application.Universities;

internal static class UniversityProgramMapping
{
    public static UniversityProgramDto ToDto(UniversityProgram program) => new(
        program.Id,
        program.UniversityId,
        program.UniversityName,
        program.Name,
        program.Cluster.ToString(),
        ProfileTaxonomy.LocalizeCluster(program.Cluster),
        program.RelevantProfiles.Select(p => ProfileTaxonomy.Localize(p)).ToList(),
        program.KeySubjects,
        program.KeyTopics,
        program.MinCompetitiveAverage);

    public static ProgramFitDto ToFitDto(UniversityProgram program, ProgramFit fit)
    {
        var gaps = fit.Gaps
            .Select(g => new AdmissionGapDto(g.Area, g.Name, g.Current, g.Target, g.Gap))
            .ToList();

        return new ProgramFitDto(
            program.Id,
            program.UniversityId,
            program.UniversityName,
            program.Name,
            program.Cluster.ToString(),
            ProfileTaxonomy.LocalizeCluster(program.Cluster),
            fit.FitScore,
            fit.MeetsThreshold,
            fit.Strengths,
            gaps,
            BuildAdvice(program, fit));
    }

    private static IReadOnlyCollection<string> BuildAdvice(UniversityProgram program, ProgramFit fit)
    {
        if (fit.Gaps.Count == 0)
        {
            return [$"Ви відповідаєте вимогам для «{program.Name}». Підтримуйте рівень із ключових предметів і тем."];
        }

        var advice = fit.Gaps
            .Take(4)
            .Select(g => g.Area == "topic"
                ? $"Підтягніть тему «{g.Name}» з {g.Current:0.0} до {g.Target:0.0} (потрібно +{g.Gap:0.0})."
                : $"Підтягніть предмет «{g.Name}» з {g.Current:0.0} до {g.Target:0.0} (потрібно +{g.Gap:0.0}).")
            .ToList();

        return advice;
    }
}
