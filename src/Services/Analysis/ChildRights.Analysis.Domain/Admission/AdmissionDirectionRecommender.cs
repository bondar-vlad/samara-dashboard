using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Entities;
using ChildRights.Analysis.Domain.Rules;

namespace ChildRights.Analysis.Domain.Admission;

/// <summary>How a pupil matches one admission direction.</summary>
public sealed record DirectionMatch(
    string DirectionCode,
    string DirectionName,
    double? CompetitiveScore,
    double TopicFit,
    double CombinedScore);

/// <summary>The result of recommending an admission direction for a pupil.</summary>
public sealed record DirectionRecommendation(
    string? RecommendedCode,
    IReadOnlyList<DirectionMatch> Ranked,
    string Rationale);

/// <summary>
/// Widget 2 engine: ranks admission directions for a pupil by combining the конкурсний бал
/// (НМТ scores weighted by each direction's coefficients) with how well the pupil's school
/// subjects/topics fit the direction. This is what lets the platform say "even though your НМТ
/// competitive score for X is fine, your topic strengths suggest direction Y fits you better".
/// </summary>
public static class AdmissionDirectionRecommender
{
    // Blend of the two signals. Competitive score reflects admission feasibility; topic fit
    // reflects genuine aptitude. Both matter, so they are weighted comparably.
    private const double CompetitiveWeight = 0.55;
    private const double TopicWeight = 0.45;

    public static DirectionRecommendation Recommend(
        IReadOnlyCollection<AdmissionDirection> directions,
        IReadOnlyDictionary<NmtSubject, int> nmtScores,
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages)
    {
        var matches = new List<DirectionMatch>();

        foreach (var direction in directions)
        {
            var competitive = CompetitiveScoreCalculator.Compute(nmtScores, direction.NmtCoefficients);
            var topicFit = TopicFit(direction, subjectAverages, topicAverages);

            // Normalise both signals to 0..1 before blending (НМТ is on a 100–200 scale).
            var competitiveNorm = competitive is { } c ? Math.Clamp((c - 100) / 100.0, 0, 1) : 0;
            var topicNorm = Math.Clamp(topicFit / 12.0, 0, 1);

            var combined = competitive is null
                ? topicNorm
                : (competitiveNorm * CompetitiveWeight) + (topicNorm * TopicWeight);

            matches.Add(new DirectionMatch(
                direction.Code,
                direction.Name,
                competitive,
                Math.Round(topicFit, 2),
                Math.Round(combined, 4)));
        }

        var ranked = matches.OrderByDescending(m => m.CombinedScore).ToList();
        if (ranked.Count == 0)
        {
            return new DirectionRecommendation(null, ranked, "Немає напрямів для аналізу.");
        }

        var best = ranked[0];
        var rationale =
            $"Рекомендований напрям «{best.DirectionName}» " +
            (best.CompetitiveScore is { } cs ? $"(конкурсний бал {cs:0.0}; " : "(") +
            $"профільність за темами {best.TopicFit:0.0}). " +
            "Враховано бали НМТ із коефіцієнтами напряму та результати за профільними темами.";

        return new DirectionRecommendation(best.DirectionCode, ranked, rationale);
    }

    private static double TopicFit(
        AdmissionDirection direction,
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages)
    {
        var values = new List<double>();

        foreach (var subject in direction.KeySubjects)
        {
            if (subjectAverages.TryGetValue(subject, out var average))
            {
                values.Add(average);
            }
        }

        foreach (var topic in direction.KeyTopics)
        {
            var match = topicAverages
                .Where(t => string.Equals(t.Topic, topic, StringComparison.OrdinalIgnoreCase))
                .Select(t => t.Average)
                .ToList();

            if (match.Count > 0)
            {
                values.Add(match.Average());
            }
        }

        return values.Count == 0 ? 0 : values.Average();
    }
}
