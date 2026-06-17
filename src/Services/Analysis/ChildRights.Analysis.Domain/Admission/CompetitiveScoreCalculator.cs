using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Analysis.Domain.Admission;

/// <summary>
/// Computes the конкурсний бал (competitive score) for an admission direction from a pupil's
/// НМТ scores and the direction's normalised subject coefficients. Pure and deterministic.
/// </summary>
public static class CompetitiveScoreCalculator
{
    /// <summary>
    /// Weighted НМТ score on the 100–200 scale. Only subjects the pupil actually took count;
    /// the used coefficients are renormalised so a missing optional subject does not deflate
    /// the result unfairly. Returns null when no relevant score is available.
    /// </summary>
    public static double? Compute(
        IReadOnlyDictionary<NmtSubject, int> nmtScores,
        IReadOnlyDictionary<NmtSubject, double> coefficients)
    {
        double weightedSum = 0;
        double usedWeight = 0;

        foreach (var (subject, coefficient) in coefficients)
        {
            if (nmtScores.TryGetValue(subject, out var score))
            {
                weightedSum += score * coefficient;
                usedWeight += coefficient;
            }
        }

        return usedWeight <= 0 ? null : Math.Round(weightedSum / usedWeight, 1);
    }
}
