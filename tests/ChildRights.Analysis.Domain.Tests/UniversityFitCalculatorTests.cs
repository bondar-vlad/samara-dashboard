using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Entities;
using ChildRights.Analysis.Domain.Rules;
using ChildRights.Analysis.Domain.Universities;
using Xunit;

namespace ChildRights.Analysis.Domain.Tests;

public sealed class UniversityFitCalculatorTests
{
    private static UniversityProgram MedicineProgram() => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "НМУ",
        "Медицина",
        ProfileCluster.ProfessionalLifeSciences,
        [EducationProfile.Medical],
        ["Біологія", "Хімія"],
        ["Анатомія людини", "Генетика"],
        minCompetitiveAverage: 10.0);

    [Fact]
    public void Strong_candidate_meets_threshold_with_no_gaps()
    {
        var program = MedicineProgram();

        var fit = UniversityFitCalculator.Evaluate(
            program,
            new Dictionary<string, double> { ["Біологія"] = 11, ["Хімія"] = 10 },
            [new TopicScore("Біологія", "Анатомія людини", 11), new TopicScore("Біологія", "Генетика", 10)]);

        Assert.True(fit.MeetsThreshold);
        Assert.Empty(fit.Gaps);
        Assert.True(fit.FitScore > 0.8);
    }

    [Fact]
    public void Weak_candidate_gets_specific_topic_and_subject_gaps()
    {
        var program = MedicineProgram();

        var fit = UniversityFitCalculator.Evaluate(
            program,
            new Dictionary<string, double> { ["Біологія"] = 8, ["Хімія"] = 7 },
            [new TopicScore("Біологія", "Анатомія людини", 7), new TopicScore("Біологія", "Генетика", 6)]);

        Assert.False(fit.MeetsThreshold);
        Assert.NotEmpty(fit.Gaps);

        // The biggest gap is surfaced first and names a concrete area to improve.
        var biggest = fit.Gaps.First();
        Assert.True(biggest.Gap > 0);
        Assert.Contains(fit.Gaps, g => g.Name == "Генетика");
        Assert.Contains(fit.Gaps, g => g.Name == "Хімія");
    }
}

public sealed class ProfileScoringMapTests
{
    [Fact]
    public void Topics_outweigh_subject_average_when_scoring_profiles()
    {
        var scores = ProfileScoringMap.ScoreProfiles(
            new Dictionary<string, double> { ["Правознавство"] = 8 },
            [new TopicScore("Правознавство", "Фінансове право", 12)]);

        // The finance topic lifts the business profile above the generic humanitarian one.
        Assert.True(scores[EducationProfile.BusinessAdministration] > scores[EducationProfile.SocialHumanitarian]);
    }
}
