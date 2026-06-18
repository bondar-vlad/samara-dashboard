using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Entities;
using ChildRights.Analysis.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChildRights.Analysis.Infrastructure.Persistence;

/// <summary>
/// Seeds deterministic demo admission inputs (НМТ scores + chosen 4th subject + desired direction)
/// for every <b>graduating</b> (11th-grade) pupil, so the admission widgets show a realistic mix of
/// "Вибір учня" and статус (збіг / розбіжність / не обрано) out of the box — without anyone having
/// to submit choices by hand.
///
/// The pupil roster is owned by the Education service, so this seeder pulls it over the resilient
/// HTTP client. If Education is not reachable yet it logs a warning and skips; because the choices
/// table is left empty, it simply retries on the next startup once Education is available.
/// </summary>
internal sealed class AdmissionChoiceSeeder(
    AnalysisDbContext context,
    IEducationDataClient educationClient,
    IClock clock,
    ILogger<AdmissionChoiceSeeder> logger) : IDataSeeder
{
    // Share of graduating pupils left without a choice, so the "Не обрано" status is demonstrable.
    private const double NotChosenShare = 0.22;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await context.StudentAdmissionChoices.AnyAsync(cancellationToken))
        {
            return;
        }

        var directionCodes = await context.AdmissionDirections
            .Select(d => d.Code)
            .ToListAsync(cancellationToken);
        if (directionCodes.Count == 0)
        {
            return; // The direction catalogue has not been seeded yet.
        }

        IReadOnlyList<EducationStudentRef> students;
        try
        {
            students = await educationClient.GetStudentsAsync(null, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Demo admission choices were not seeded because the Education service is unreachable. " +
                "They will be seeded on the next startup once Education is available.");
            return;
        }

        var graduating = students
            .Where(s => s.GradeLevel >= StudentRiskRules.GraduatingGradeLevel)
            .OrderBy(s => s.Id)
            .ToList();
        if (graduating.Count == 0)
        {
            return;
        }

        var fourthOptions = NmtSubjectCatalog.FourthSubjectOptions.ToList();
        var seeded = 0;

        foreach (var student in graduating)
        {
            // Deterministic per-pupil pseudo-randomness from a stable hash of the WHOLE student
            // GUID (XOR of its four 32-bit words). Using only the first bytes would collide,
            // because the demo GUIDs share a constant prefix and differ only in their tail.
            var idBytes = student.Id.ToByteArray();
            var seed = BitConverter.ToInt32(idBytes, 0)
                ^ BitConverter.ToInt32(idBytes, 4)
                ^ BitConverter.ToInt32(idBytes, 8)
                ^ BitConverter.ToInt32(idBytes, 12);
            var rng = new Random(seed);

            if (rng.NextDouble() < NotChosenShare)
            {
                continue;
            }

            var choice = new StudentAdmissionChoice(Guid.NewGuid(), student.Id, student.SchoolId);

            choice.SetDesiredDirection(directionCodes[rng.Next(directionCodes.Count)], clock.UtcNow);

            var fourth = fourthOptions[rng.Next(fourthOptions.Count)];
            choice.SetFourthSubject(fourth, clock.UtcNow);

            choice.SetNmtScores(new Dictionary<NmtSubject, int>
            {
                [NmtSubject.UkrainianLanguage] = rng.Next(130, 196),
                [NmtSubject.Mathematics] = rng.Next(130, 196),
                [NmtSubject.HistoryOfUkraine] = rng.Next(130, 196),
                [fourth] = rng.Next(130, 196),
            }, clock.UtcNow);

            context.StudentAdmissionChoices.Add(choice);
            seeded++;
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} demo admission choices for graduating pupils.", seeded);
    }
}
