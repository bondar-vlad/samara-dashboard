using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Entities;
using ChildRights.Analysis.Domain.Enums;
using ChildRights.Analysis.Domain.Rules;

namespace ChildRights.Analysis.Domain.Admission;

/// <summary>
/// The deterministic admission rule engine (the "second analysis"). Produces 4th-НМТ-subject
/// and admission-direction recommendations, and raises a red flag whenever the pupil's own
/// choice differs from the data-driven recommendation. Pure and testable.
/// </summary>
public static class AdmissionRules
{
    public static RuleEvaluation Evaluate(
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages,
        IReadOnlyDictionary<NmtSubject, int> nmtScores,
        NmtSubject? chosenFourthSubject,
        string? desiredDirectionCode,
        IReadOnlyCollection<AdmissionDirection> directions)
    {
        var flags = new List<FlagFinding>();
        var recommendations = new List<RecommendationFinding>();

        EvaluateFourthSubject(subjectAverages, topicAverages, chosenFourthSubject, flags, recommendations);
        EvaluateDirection(subjectAverages, topicAverages, nmtScores, desiredDirectionCode, directions, flags, recommendations);

        return new RuleEvaluation(flags, recommendations);
    }

    /// <summary>The <see cref="Enums.AnalysisGoal.NmtFourthSubject"/> case in isolation.</summary>
    public static RuleEvaluation EvaluateFourthSubjectGoal(
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages,
        NmtSubject? chosenFourthSubject)
    {
        var flags = new List<FlagFinding>();
        var recommendations = new List<RecommendationFinding>();
        EvaluateFourthSubject(subjectAverages, topicAverages, chosenFourthSubject, flags, recommendations);
        return new RuleEvaluation(flags, recommendations);
    }

    /// <summary>The <see cref="Enums.AnalysisGoal.AdmissionDirection"/> case in isolation.</summary>
    public static RuleEvaluation EvaluateDirectionGoal(
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages,
        IReadOnlyDictionary<NmtSubject, int> nmtScores,
        string? desiredDirectionCode,
        IReadOnlyCollection<AdmissionDirection> directions)
    {
        var flags = new List<FlagFinding>();
        var recommendations = new List<RecommendationFinding>();
        EvaluateDirection(subjectAverages, topicAverages, nmtScores, desiredDirectionCode, directions, flags, recommendations);
        return new RuleEvaluation(flags, recommendations);
    }

    private static void EvaluateFourthSubject(
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages,
        NmtSubject? chosen,
        List<FlagFinding> flags,
        List<RecommendationFinding> recommendations)
    {
        var recommendation = FourthSubjectRecommender.Recommend(subjectAverages, topicAverages);
        if (recommendation.Recommended is not { } recommended)
        {
            return;
        }

        recommendations.Add(new RecommendationFinding(
            RecommendationKind.FourthSubjectChoice,
            $"Рекомендований 4-й предмет НМТ: {NmtSubjectCatalog.Localize(recommended)}",
            $"За результатами навчання найкраще скласти {NmtSubjectCatalog.Localize(recommended)}.",
            recommendation.Rationale,
            0.8));

        if (chosen is { } c && c != recommended)
        {
            flags.Add(new FlagFinding(
                "ADM-NMT4-MISMATCH",
                FlagSeverity.Yellow,
                "Невідповідність вибору 4-го предмета НМТ",
                $"Учень обрав «{NmtSubjectCatalog.Localize(c)}», проте результати вказують на " +
                $"«{NmtSubjectCatalog.Localize(recommended)}».",
                Agency.Education,
                [AudienceRole.Student, AudienceRole.ClassTeacher, AudienceRole.Parent],
                ["Обговорити вибір 4-го предмета НМТ з урахуванням сильних предметів"]));
        }
    }

    private static void EvaluateDirection(
        IReadOnlyDictionary<string, double> subjectAverages,
        IReadOnlyList<TopicScore> topicAverages,
        IReadOnlyDictionary<NmtSubject, int> nmtScores,
        string? desiredDirectionCode,
        IReadOnlyCollection<AdmissionDirection> directions,
        List<FlagFinding> flags,
        List<RecommendationFinding> recommendations)
    {
        if (directions.Count == 0)
        {
            return;
        }

        var recommendation = AdmissionDirectionRecommender.Recommend(
            directions, nmtScores, subjectAverages, topicAverages);

        if (recommendation.RecommendedCode is not { } recommendedCode)
        {
            return;
        }

        var recommendedName = directions.First(d => d.Code == recommendedCode).Name;

        recommendations.Add(new RecommendationFinding(
            RecommendationKind.AdmissionDirectionChoice,
            $"Рекомендований напрям вступу: {recommendedName}",
            recommendation.Rationale,
            recommendation.Rationale,
            0.75));

        if (!string.IsNullOrWhiteSpace(desiredDirectionCode) && desiredDirectionCode != recommendedCode)
        {
            var desiredName = directions.FirstOrDefault(d => d.Code == desiredDirectionCode)?.Name ?? desiredDirectionCode;

            flags.Add(new FlagFinding(
                "ADM-DIRECTION-MISMATCH",
                FlagSeverity.Yellow,
                "Невідповідність обраного напряму вступу",
                $"Учень обрав напрям «{desiredName}», проте бали НМТ та профільні теми " +
                $"вказують на «{recommendedName}».",
                Agency.Education,
                [AudienceRole.Student, AudienceRole.ClassTeacher, AudienceRole.Parent],
                ["Провести профорієнтаційну консультацію щодо напряму вступу"]));
        }
    }
}
