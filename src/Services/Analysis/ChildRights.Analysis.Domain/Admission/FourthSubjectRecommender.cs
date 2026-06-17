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
        IReadOnlyList<TopicScore> topicAverages)
    {
        var accumulator = new Dictionary<NmtSubject, (double Sum, double Weight, int Count)>();

        void Add(NmtSubject subject, double value, double weight)
        {
            var current = accumulator.GetValueOrDefault(subject);
            accumulator[subject] = (current.Sum + (value * weight), current.Weight + weight, current.Count + 1);
        }

        foreach (var (subject, average) in subjectAverages)
        {
            if (NmtSubjectCatalog.FromSchoolSubject(subject) is { } nmt)
            {
                Add(nmt, average, 1.0);
            }
        }

        // Topics reinforce the subject they belong to (weighted higher, like profile scoring).
        foreach (var topic in topicAverages)
        {
            if (NmtSubjectCatalog.FromSchoolSubject(topic.Subject) is { } nmt)
            {
                Add(nmt, topic.Average, 1.5);
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
