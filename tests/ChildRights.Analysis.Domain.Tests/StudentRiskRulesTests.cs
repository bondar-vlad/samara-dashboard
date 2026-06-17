using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Enums;
using ChildRights.Analysis.Domain.Rules;
using Xunit;

namespace ChildRights.Analysis.Domain.Tests;

public sealed class StudentRiskRulesTests
{
    private static StudentSnapshot Snapshot(
        int gradeLevel = 9,
        EducationProfile? declaredProfile = null,
        IReadOnlyList<EducationProfile>? desiredProfiles = null,
        int unexcused = 0,
        Dictionary<string, double>? subjectAverages = null,
        IReadOnlyList<TopicScore>? topicAverages = null) =>
        new(
            Guid.NewGuid(),
            "Тест Учень",
            Guid.NewGuid(),
            Guid.NewGuid(),
            gradeLevel,
            declaredProfile,
            desiredProfiles ?? [],
            unexcused,
            subjectAverages ?? new Dictionary<string, double>(),
            topicAverages ?? []);

    [Theory]
    [InlineData(4, FlagSeverity.Green)]
    [InlineData(5, FlagSeverity.Yellow)]
    [InlineData(10, FlagSeverity.Orange)]
    [InlineData(20, FlagSeverity.Red)]
    public void Attendance_thresholds_map_to_expected_severity(int unexcused, FlagSeverity expected)
    {
        var evaluation = StudentRiskRules.Evaluate(Snapshot(unexcused: unexcused));

        var attendanceFlag = evaluation.Flags.SingleOrDefault(f => f.RuleCode == "EDU-ATTENDANCE");

        if (expected == FlagSeverity.Green)
        {
            Assert.Null(attendanceFlag);
        }
        else
        {
            Assert.NotNull(attendanceFlag);
            Assert.Equal(expected, attendanceFlag!.Severity);
        }
    }

    [Fact]
    public void Red_attendance_escalates_to_cross_agency_audiences()
    {
        var evaluation = StudentRiskRules.Evaluate(Snapshot(unexcused: 25));

        var flag = Assert.Single(evaluation.Flags, f => f.RuleCode == "EDU-ATTENDANCE");
        Assert.Contains(AudienceRole.SocialService, flag.TargetAudiences);
        Assert.Contains(AudienceRole.JuvenilePolice, flag.TargetAudiences);
    }

    [Fact]
    public void Low_subject_average_raises_academic_risk_flag()
    {
        var evaluation = StudentRiskRules.Evaluate(Snapshot(
            subjectAverages: new Dictionary<string, double> { ["Математика"] = 3.0 }));

        Assert.Contains(evaluation.Flags, f => f.RuleCode == "EDU-GRADE-LOW");
    }

    [Fact]
    public void Grade9_pupil_receives_a_profile_choice_recommendation_in_strongest_cluster()
    {
        var evaluation = StudentRiskRules.Evaluate(Snapshot(
            gradeLevel: 9,
            subjectAverages: new Dictionary<string, double>
            {
                ["Біологія"] = 11,
                ["Хімія"] = 12,
                ["Математика"] = 7
            },
            topicAverages:
            [
                new TopicScore("Біологія", "Анатомія людини", 12),
                new TopicScore("Біологія", "Генетика", 12)
            ]));

        var recommendation = Assert.Single(
            evaluation.Recommendations, r => r.Kind == RecommendationKind.ProfileChoice);
        Assert.Equal(ProfileCluster.ProfessionalLifeSciences, recommendation.RecommendedCluster);
        Assert.Contains(EducationProfile.Medical, recommendation.RecommendedProfiles!);
    }

    [Fact]
    public void Topic_signal_steers_recommendation_within_law_toward_business_cluster()
    {
        // Strong finance topics inside law/economics → business cluster, not the broad legal one.
        var evaluation = StudentRiskRules.Evaluate(Snapshot(
            gradeLevel: 10,
            declaredProfile: EducationProfile.SocialHumanitarian,
            subjectAverages: new Dictionary<string, double>
            {
                ["Правознавство"] = 9,
                ["Економіка"] = 11
            },
            topicAverages:
            [
                new TopicScore("Правознавство", "Фінансове право", 12),
                new TopicScore("Економіка", "Фінанси та інвестиції", 12),
                new TopicScore("Правознавство", "Кримінальне право", 6)
            ]));

        var recommendation = Assert.Single(evaluation.Recommendations, r => r.RecommendedCluster is not null);
        Assert.Equal(ProfileCluster.ProfessionalBusinessServices, recommendation.RecommendedCluster);
        Assert.Contains(EducationProfile.BusinessAdministration, recommendation.RecommendedProfiles!);
    }

    [Fact]
    public void Desired_cluster_differing_from_recommended_raises_mismatch_flag()
    {
        // Wants the academic humanitarian cluster, but data points to business services.
        var evaluation = StudentRiskRules.Evaluate(Snapshot(
            gradeLevel: 10,
            desiredProfiles: [EducationProfile.SocialHumanitarian],
            subjectAverages: new Dictionary<string, double> { ["Економіка"] = 11 },
            topicAverages:
            [
                new TopicScore("Економіка", "Фінанси та інвестиції", 12),
                new TopicScore("Економіка", "Мікроекономіка", 11)
            ]));

        Assert.Contains(evaluation.Flags, f => f.RuleCode == "EDU-PROFILE-MISMATCH");
    }

    [Fact]
    public void Desired_cluster_matching_recommended_raises_no_mismatch_flag()
    {
        var evaluation = StudentRiskRules.Evaluate(Snapshot(
            gradeLevel: 10,
            desiredProfiles: [EducationProfile.Medical],
            subjectAverages: new Dictionary<string, double> { ["Біологія"] = 12, ["Хімія"] = 11 },
            topicAverages: [new TopicScore("Біологія", "Генетика", 12)]));

        Assert.DoesNotContain(evaluation.Flags, f => f.RuleCode == "EDU-PROFILE-MISMATCH");
    }
}
