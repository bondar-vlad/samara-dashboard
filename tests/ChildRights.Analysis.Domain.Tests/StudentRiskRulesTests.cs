using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Enums;
using ChildRights.Analysis.Domain.Rules;
using Xunit;

namespace ChildRights.Analysis.Domain.Tests;

public sealed class StudentRiskRulesTests
{
    private static StudentSnapshot Snapshot(
        int gradeLevel = 9,
        string? declaredProfile = null,
        int unexcused = 0,
        Dictionary<string, double>? subjectAverages = null) =>
        new(
            Guid.NewGuid(),
            "Тест Учень",
            Guid.NewGuid(),
            Guid.NewGuid(),
            gradeLevel,
            declaredProfile,
            unexcused,
            subjectAverages ?? new Dictionary<string, double>());

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
    public void Grade9_pupil_receives_a_profile_choice_recommendation()
    {
        var evaluation = StudentRiskRules.Evaluate(Snapshot(
            gradeLevel: 9,
            subjectAverages: new Dictionary<string, double>
            {
                ["Біологія"] = 11,
                ["Хімія"] = 12,
                ["Математика"] = 7
            }));

        var recommendation = Assert.Single(
            evaluation.Recommendations, r => r.Kind == RecommendationKind.ProfileChoice);
        Assert.Contains("Природничий", recommendation.Title);
    }

    [Fact]
    public void Grade10_profile_mismatch_recommends_a_profile_change()
    {
        // Declared Mathematics but much stronger in Philology.
        var evaluation = StudentRiskRules.Evaluate(Snapshot(
            gradeLevel: 10,
            declaredProfile: "Mathematics",
            subjectAverages: new Dictionary<string, double>
            {
                ["Математика"] = 5,
                ["Українська література"] = 11,
                ["Українська мова"] = 10
            }));

        Assert.Contains(evaluation.Recommendations, r => r.Kind == RecommendationKind.ProfileChange);
        Assert.Contains(evaluation.Flags, f => f.RuleCode == "EDU-PROFILE-MISMATCH");
    }

    [Fact]
    public void Well_matched_pupil_produces_no_profile_change()
    {
        var evaluation = StudentRiskRules.Evaluate(Snapshot(
            gradeLevel: 10,
            declaredProfile: "NaturalSciences",
            subjectAverages: new Dictionary<string, double>
            {
                ["Біологія"] = 11,
                ["Хімія"] = 12
            }));

        Assert.DoesNotContain(evaluation.Recommendations, r => r.Kind == RecommendationKind.ProfileChange);
    }
}
