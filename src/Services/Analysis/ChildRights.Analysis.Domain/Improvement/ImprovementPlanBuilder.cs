using ChildRights.Analysis.Domain.Rules;

namespace ChildRights.Analysis.Domain.Improvement;

/// <summary>
/// One area to pull up on the way to a chosen profile/direction: the pupil's current level,
/// the target level and the gap between them.
/// </summary>
public sealed record ImprovementGap(string Area, string Name, double Current, double Target, double Gap);

/// <summary>
/// Builds a concrete "what to pull up" gap analysis toward a pupil's <b>chosen</b> target
/// (profile cluster or admission direction). Pure and deterministic: it computes the measurable
/// gaps; an AI coach then turns each gap into concrete study advice.
/// </summary>
public static class ImprovementPlanBuilder
{
    /// <summary>The solid, competitive level to aim for (out of 12).</summary>
    public const double TargetLevel = 10.0;

    private const double MinGap = 0.5;
    private const int MaxItems = 6;

    public static IReadOnlyList<ImprovementGap> Build(
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages,
        IReadOnlyList<string> keySubjects,
        IReadOnlyList<string> keyTopics,
        double targetLevel = TargetLevel)
    {
        var gaps = new List<ImprovementGap>();

        foreach (var subject in keySubjects.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (subjectAverages.TryGetValue(subject, out var current))
            {
                AddGap(gaps, "subject", subject, current, targetLevel);
            }
        }

        var topicLevels = topicAverages
            .GroupBy(t => t.Topic, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Average(t => t.Average), StringComparer.OrdinalIgnoreCase);

        foreach (var topic in keyTopics.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (topicLevels.TryGetValue(topic, out var current))
            {
                AddGap(gaps, "topic", topic, current, targetLevel);
            }
        }

        return gaps
            .OrderByDescending(g => g.Gap)
            .Take(MaxItems)
            .ToList();
    }

    private static void AddGap(List<ImprovementGap> gaps, string area, string name, double current, double target)
    {
        var gap = target - current;
        if (gap >= MinGap)
        {
            gaps.Add(new ImprovementGap(area, name, Math.Round(current, 1), target, Math.Round(gap, 1)));
        }
    }
}
