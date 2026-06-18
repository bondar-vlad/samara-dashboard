using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Rules;

namespace ChildRights.Analysis.Domain.Admission;

/// <summary>How well a pupil predicts for a single 4th-subject option.</summary>
public sealed record FourthSubjectScore(NmtSubject Subject, double Score, int EvidenceCount);

/// <summary>The result of recommending a pupil's 4th НМТ subject.</summary>
public sealed record FourthSubjectRecommendation(
    NmtSubject? Recommended,
    IReadOnlyList<FourthSubjectScore> Ranked,
    string Rationale);

/// <summary>
/// Widget 1 engine: recommends which 4th НМТ subject a pupil should take, purely from their
/// school grades and topics mapped onto the elective НМТ subjects. Deterministic and testable.
/// </summary>
public static class FourthSubjectRecommender
{
    public static FourthSubjectRecommendation Recommend(
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages,
        IReadOnlyDictionary<string, int>? gradeCounts = null)
    {
        var accumulator = new Dictionary<NmtSubject, (double Sum, double Weight, int Count)>();

        void Add(NmtSubject subject, double value, double weight, int count)
        {
            var current = accumulator.GetValueOrDefault(subject);
            accumulator[subject] = (current.Sum + (value * weight), current.Weight + weight, current.Count + count);
        }

        foreach (var (subject, average) in subjectAverages)
        {
            if (NmtSubjectCatalog.FromSchoolSubject(subject) is { } nmt)
            {
                // Evidence = the actual number of grades for that subject (defaults to 1).
                Add(nmt, average, 1.0, gradeCounts?.GetValueOrDefault(subject) ?? 1);
            }
        }

        // Topics reinforce the score (weighted higher) but do not add to the grade count, since the
        // subject average above already counts every grade, topic-tagged ones included.
        foreach (var topic in topicAverages)
        {
            if (NmtSubjectCatalog.FromSchoolSubject(topic.Subject) is { } nmt)
            {
                Add(nmt, topic.Average, 1.5, 0);
            }
        }

        var ranked = accumulator
            .Where(kv => NmtSubjectCatalog.IsFourthSubjectOption(kv.Key))
            .Select(kv => new FourthSubjectScore(
                kv.Key, Math.Round(kv.Value.Sum / kv.Value.Weight, 2), kv.Value.Count))
            .OrderByDescending(s => s.Score)
            .ToList();

        if (ranked.Count == 0)
        {
            return new FourthSubjectRecommendation(
                null, ranked, "Недостатньо даних для рекомендації 4-го предмета НМТ.");
        }

        var best = ranked[0];
        var rationale =
            $"Найкращі результати серед предметів на вибір: " +
            string.Join(", ", ranked.Take(3).Select(s => $"{NmtSubjectCatalog.Localize(s.Subject)} ({s.Score:0.0})")) + ".";

        return new FourthSubjectRecommendation(best.Subject, ranked, rationale);
    }
}
