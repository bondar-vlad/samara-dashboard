using ChildRights.Analysis.Domain.Entities;
using ChildRights.Analysis.Domain.Rules;

namespace ChildRights.Analysis.Domain.Universities;

/// <summary>An area (subject or topic) the pupil should improve to be competitive for a programme.</summary>
public sealed record AdmissionGap(string Area, string Name, double Current, double Target, double Gap);

/// <summary>The result of matching a pupil against a single university programme.</summary>
public sealed record ProgramFit(
    Guid ProgramId,
    double FitScore,
    bool MeetsThreshold,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<AdmissionGap> Gaps);

/// <summary>
/// Pure, deterministic calculator that matches a pupil's subject/topic grades against a
/// university programme's key areas, producing a fit score and concrete, topic-level
/// improvement gaps ("raise Фінансове право from 7 to 9"). Fully unit-testable.
/// </summary>
public static class UniversityFitCalculator
{
    public static ProgramFit Evaluate(
        UniversityProgram program,
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages)
    {
        var strengths = new List<string>();
        var gaps = new List<AdmissionGap>();
        var scores = new List<double>();

        foreach (var subject in program.KeySubjects)
        {
            var current = subjectAverages.GetValueOrDefault(subject, 0);
            scores.Add(current);
            Classify("subject", subject, current, program.MinCompetitiveAverage, strengths, gaps);
        }

        foreach (var topic in program.KeyTopics)
        {
            var current = TopicAverage(topic, topicAverages, subjectAverages);
            scores.Add(current);
            Classify("topic", topic, current, program.MinCompetitiveAverage, strengths, gaps);
        }

        var average = scores.Count > 0 ? scores.Average() : 0;
        var fitScore = Math.Round(Math.Clamp(average / 12.0, 0, 1), 3);
        var meetsThreshold = scores.Count > 0 && average >= program.MinCompetitiveAverage;

        return new ProgramFit(program.Id, fitScore, meetsThreshold, strengths, gaps.OrderByDescending(g => g.Gap).ToList());
    }

    private static void Classify(
        string area,
        string name,
        double current,
        double target,
        List<string> strengths,
        List<AdmissionGap> gaps)
    {
        if (current >= target)
        {
            strengths.Add(name);
        }
        else
        {
            gaps.Add(new AdmissionGap(area, name, Math.Round(current, 1), target, Math.Round(target - current, 1)));
        }
    }

    private static double TopicAverage(
        string topic,
        IReadOnlyList<TopicScore> topicAverages,
        IReadOnlyDictionary<string, double> subjectAverages)
    {
        var matches = topicAverages.Where(t => string.Equals(t.Topic, topic, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count > 0)
        {
            return matches.Average(t => t.Average);
        }

        // Topic not graded individually: fall back to the subject it most likely belongs to.
        var subjectMatch = topicAverages.FirstOrDefault(t => string.Equals(t.Topic, topic, StringComparison.OrdinalIgnoreCase));
        return subjectMatch is not null ? subjectMatch.Average : subjectAverages.GetValueOrDefault(topic, 0);
    }
}
