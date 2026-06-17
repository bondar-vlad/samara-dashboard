using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Admission;
using ChildRights.Analysis.Domain.Entities;
using ChildRights.Analysis.Domain.Rules;
using Xunit;

namespace ChildRights.Analysis.Domain.Tests;

public sealed class AdmissionRulesTests
{
    private static AdmissionDirection ItDirection() => new(
        Guid.NewGuid(), "12", "Інформаційні технології", "12 Інформаційні технології",
        ProfileCluster.ProfessionalInformationTechnology,
        new Dictionary<NmtSubject, double>
        {
            [NmtSubject.Mathematics] = 0.5,
            [NmtSubject.UkrainianLanguage] = 0.2,
            [NmtSubject.HistoryOfUkraine] = 0.1,
            [NmtSubject.Physics] = 0.2
        },
        ["Математика", "Інформатика"],
        ["Програмування", "Алгоритми"]);

    private static AdmissionDirection MedicineDirection() => new(
        Guid.NewGuid(), "22", "Охорона здоров'я", "22 Охорона здоров'я",
        ProfileCluster.ProfessionalLifeSciences,
        new Dictionary<NmtSubject, double>
        {
            [NmtSubject.Biology] = 0.4,
            [NmtSubject.Chemistry] = 0.3,
            [NmtSubject.UkrainianLanguage] = 0.2,
            [NmtSubject.Mathematics] = 0.1
        },
        ["Біологія", "Хімія"],
        ["Анатомія людини", "Генетика"]);

    [Fact]
    public void FourthSubject_recommends_the_strongest_elective()
    {
        var recommendation = FourthSubjectRecommender.Recommend(
            new Dictionary<string, double> { ["Біологія"] = 11, ["Хімія"] = 10, ["Фізика"] = 6 },
            [new TopicScore("Біологія", "Генетика", 12)]);

        Assert.Equal(NmtSubject.Biology, recommendation.Recommended);
    }

    [Fact]
    public void CompetitiveScore_is_the_weighted_average_of_taken_subjects()
    {
        var score = CompetitiveScoreCalculator.Compute(
            new Dictionary<NmtSubject, int>
            {
                [NmtSubject.Mathematics] = 190,
                [NmtSubject.UkrainianLanguage] = 170,
                [NmtSubject.HistoryOfUkraine] = 160,
                [NmtSubject.Physics] = 180
            },
            ItDirection().NmtCoefficients);

        // 0.5*190 + 0.2*170 + 0.1*160 + 0.2*180 = 95 + 34 + 16 + 36 = 181
        Assert.NotNull(score);
        Assert.Equal(181, score!.Value, 1);
    }

    [Fact]
    public void Direction_recommender_prefers_the_better_competitive_and_topic_fit()
    {
        var directions = new[] { ItDirection(), MedicineDirection() };

        var recommendation = AdmissionDirectionRecommender.Recommend(
            directions,
            new Dictionary<NmtSubject, int>
            {
                [NmtSubject.Mathematics] = 195,
                [NmtSubject.UkrainianLanguage] = 175,
                [NmtSubject.HistoryOfUkraine] = 160,
                [NmtSubject.Physics] = 185
            },
            new Dictionary<string, double> { ["Математика"] = 11, ["Інформатика"] = 12 },
            [new TopicScore("Інформатика", "Програмування", 12)]);

        Assert.Equal("12", recommendation.RecommendedCode);
    }

    [Fact]
    public void Chosen_fourth_subject_differing_from_recommended_raises_flag()
    {
        var evaluation = AdmissionRules.Evaluate(
            new Dictionary<string, double> { ["Біологія"] = 12, ["Хімія"] = 11 },
            [new TopicScore("Біологія", "Генетика", 12)],
            new Dictionary<NmtSubject, int>(),
            chosenFourthSubject: NmtSubject.Physics, // wants physics, data says biology
            desiredDirectionCode: null,
            directions: []);

        Assert.Contains(evaluation.Flags, f => f.RuleCode == "ADM-NMT4-MISMATCH");
    }

    [Fact]
    public void Desired_direction_differing_from_recommended_raises_flag()
    {
        var directions = new[] { ItDirection(), MedicineDirection() };

        var evaluation = AdmissionRules.Evaluate(
            new Dictionary<string, double> { ["Біологія"] = 12, ["Хімія"] = 12 },
            [new TopicScore("Біологія", "Анатомія людини", 12)],
            new Dictionary<NmtSubject, int>
            {
                [NmtSubject.Biology] = 195,
                [NmtSubject.Chemistry] = 190,
                [NmtSubject.UkrainianLanguage] = 180,
                [NmtSubject.Mathematics] = 150
            },
            chosenFourthSubject: null,
            desiredDirectionCode: "12", // wants IT, data says medicine
            directions: directions);

        Assert.Contains(evaluation.Flags, f => f.RuleCode == "ADM-DIRECTION-MISMATCH");
    }
}
