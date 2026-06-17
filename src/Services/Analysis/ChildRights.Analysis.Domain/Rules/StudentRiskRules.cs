using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Enums;

namespace ChildRights.Analysis.Domain.Rules;

/// <summary>
/// The deterministic child-rights rule engine. Pure and side-effect free, so it is
/// fully unit-testable and also serves as the safe fallback when no AI model is available.
/// Encodes attendance, academic-risk and profiling rules.
/// </summary>
public static class StudentRiskRules
{
    public const int YellowAbsences = 5;
    public const int OrangeAbsences = 10;
    public const int RedAbsences = 20;

    public static RuleEvaluation Evaluate(StudentSnapshot snapshot)
    {
        var flags = new List<FlagFinding>();
        var recommendations = new List<RecommendationFinding>();

        EvaluateAttendance(snapshot, flags);
        EvaluateAcademicRisk(snapshot, flags);
        EvaluateProfiling(snapshot, flags, recommendations);

        return new RuleEvaluation(flags, recommendations);
    }

    private static void EvaluateAttendance(StudentSnapshot snapshot, List<FlagFinding> flags)
    {
        var severity = snapshot.UnexcusedAbsences switch
        {
            >= RedAbsences => FlagSeverity.Red,
            >= OrangeAbsences => FlagSeverity.Orange,
            >= YellowAbsences => FlagSeverity.Yellow,
            _ => FlagSeverity.Green
        };

        if (severity < FlagSeverity.Yellow)
        {
            return;
        }

        var audiences = new List<AudienceRole> { AudienceRole.ClassTeacher };
        var actions = new List<string> { "Зʼясувати причини пропусків занять" };

        if (severity >= FlagSeverity.Orange)
        {
            audiences.AddRange([AudienceRole.Parent, AudienceRole.SchoolAdministration]);
            actions.Add("Поінформувати батьків та адміністрацію закладу");
        }

        if (severity >= FlagSeverity.Red)
        {
            audiences.AddRange([AudienceRole.SocialService, AudienceRole.JuvenilePolice, AudienceRole.EducationSafetyOfficer]);
            actions.Add("Подати звернення до служби у справах дітей та ювенальної поліції");
        }

        flags.Add(new FlagFinding(
            "EDU-ATTENDANCE",
            severity,
            "Систематичні пропуски занять",
            $"Зафіксовано {snapshot.UnexcusedAbsences} пропусків без поважної причини.",
            Agency.Education,
            audiences,
            actions));
    }

    private static void EvaluateAcademicRisk(StudentSnapshot snapshot, List<FlagFinding> flags)
    {
        var failing = snapshot.SubjectAverages
            .Where(kv => kv.Value < 4)
            .Select(kv => kv.Key)
            .ToList();

        if (failing.Count == 0)
        {
            return;
        }

        flags.Add(new FlagFinding(
            "EDU-GRADE-LOW",
            FlagSeverity.Yellow,
            "Низька успішність",
            $"Середній бал нижчий за 4 з предметів: {string.Join(", ", failing)}.",
            Agency.Education,
            [AudienceRole.ClassTeacher, AudienceRole.Parent],
            ["Запланувати додаткові заняття", "Поінформувати батьків про ризик неатестації"]));
    }

    private static void EvaluateProfiling(
        StudentSnapshot snapshot,
        List<FlagFinding> flags,
        List<RecommendationFinding> recommendations)
    {
        var profileScores = ProfileScoringMap.ScoreProfiles(snapshot.SubjectAverages, snapshot.TopicAverages);
        if (profileScores.Count == 0)
        {
            return;
        }

        // Choose the strongest cluster using evidence-aware cluster scoring (breadth + strength),
        // then pick the strongest profiles within that cluster.
        var clusterScores = ProfileScoringMap.ScoreClusters(snapshot.SubjectAverages, snapshot.TopicAverages)
            .OrderByDescending(kv => kv.Value)
            .ToList();

        var bestCluster = clusterScores[0].Key;
        var confidence = Confidence(clusterScores.Select(c => c.Value).ToList());

        // Within the best cluster, the profiles the pupil is strongest in (several allowed).
        var recommendedProfiles = profileScores
            .Where(kv => ProfileTaxonomy.ClusterOf(kv.Key) == bestCluster)
            .OrderByDescending(kv => kv.Value)
            .Where(kv => kv.Value >= 7)
            .Select(kv => kv.Key)
            .Take(3)
            .ToList();

        if (recommendedProfiles.Count == 0)
        {
            recommendedProfiles = profileScores
                .Where(kv => ProfileTaxonomy.ClusterOf(kv.Key) == bestCluster)
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .Take(1)
                .ToList();
        }

        var rationale = BuildRationale(snapshot, profileScores);
        var clusterName = ProfileTaxonomy.LocalizeCluster(bestCluster);
        var profileNames = string.Join(", ", recommendedProfiles.Select(ProfileTaxonomy.Localize));

        var declaredCluster = snapshot.DeclaredProfile is { } declared
            ? ProfileTaxonomy.ClusterOf(declared)
            : (ProfileCluster?)null;

        var kind = snapshot.GradeLevel <= 9 || declaredCluster is null
            ? RecommendationKind.ProfileChoice
            : declaredCluster == bestCluster
                ? RecommendationKind.ProfileChoice
                : RecommendationKind.ProfileChange;

        var title = kind == RecommendationKind.ProfileChange
            ? $"Розглянути зміну кластера на «{clusterName}»"
            : $"Рекомендований кластер: {clusterName}";

        recommendations.Add(new RecommendationFinding(
            kind,
            title,
            $"Рекомендовані профілі в межах кластера «{clusterName}»: {profileNames}.",
            rationale,
            confidence,
            bestCluster,
            recommendedProfiles));

        // Mismatch: the pupil wants a different cluster than the data recommends.
        if (snapshot.DesiredCluster is { } desiredCluster && desiredCluster != bestCluster)
        {
            flags.Add(new FlagFinding(
                "EDU-PROFILE-MISMATCH",
                FlagSeverity.Yellow,
                "Невідповідність бажаного профілю",
                $"Учень обрав кластер «{ProfileTaxonomy.LocalizeCluster(desiredCluster)}», " +
                $"проте результати вказують на «{clusterName}».",
                Agency.Education,
                [AudienceRole.ClassTeacher, AudienceRole.Student, AudienceRole.Parent],
                ["Провести профорієнтаційну консультацію", "Обговорити сильні теми та відповідні профілі"]));
        }
    }

    private static double Confidence(IReadOnlyList<double> clusterScores)
    {
        if (clusterScores.Count < 2)
        {
            return 0.75;
        }

        var margin = clusterScores[0] - clusterScores[1];
        return Math.Clamp(0.5 + (margin / 12.0), 0.5, 0.97);
    }

    private static string BuildRationale(
        StudentSnapshot snapshot,
        IReadOnlyDictionary<EducationProfile, double> profileScores)
    {
        var topProfiles = profileScores
            .OrderByDescending(kv => kv.Value)
            .Take(3)
            .Select(kv => $"{ProfileTaxonomy.Localize(kv.Key)} — {kv.Value:0.0}");

        var rationale = "Оцінки за профілями: " + string.Join("; ", topProfiles) + ".";

        var topTopics = snapshot.TopicAverages
            .OrderByDescending(t => t.Average)
            .Take(3)
            .Select(t => $"{t.Topic} ({t.Average:0.0})")
            .ToList();

        if (topTopics.Count > 0)
        {
            rationale += " Найсильніші теми: " + string.Join(", ", topTopics) + ".";
        }

        return rationale;
    }
}
