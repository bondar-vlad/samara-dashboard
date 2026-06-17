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
        var areaAverages = SubjectProfileMap.AreaAverages(snapshot.SubjectAverages);
        if (areaAverages.Count == 0)
        {
            return;
        }

        var best = areaAverages.OrderByDescending(kv => kv.Value).First();
        var confidence = Confidence(areaAverages);
        var rationale = BuildAreaRationale(areaAverages);

        if (snapshot.GradeLevel == 9)
        {
            recommendations.Add(new RecommendationFinding(
                RecommendationKind.ProfileChoice,
                $"Рекомендований профіль для 10 класу: {SubjectProfileMap.Localize(best.Key)}",
                $"Найкращі результати у напрямі «{SubjectProfileMap.Localize(best.Key)}» (середній бал {best.Value:0.0}).",
                rationale,
                confidence));
            return;
        }

        if (snapshot.GradeLevel < 10 || snapshot.DeclaredProfile is null)
        {
            return;
        }

        var declaredArea = SubjectProfileMap.ParseDeclared(snapshot.DeclaredProfile);
        if (declaredArea is null || !areaAverages.TryGetValue(declaredArea.Value, out var declaredAverage))
        {
            return;
        }

        if (best.Key != declaredArea.Value && best.Value - declaredAverage >= 2.0 && declaredAverage < 7)
        {
            recommendations.Add(new RecommendationFinding(
                RecommendationKind.ProfileChange,
                $"Розглянути зміну профілю на «{SubjectProfileMap.Localize(best.Key)}»",
                $"Поточний профіль «{SubjectProfileMap.Localize(declaredArea.Value)}» має середній бал {declaredAverage:0.0}, " +
                $"тоді як «{SubjectProfileMap.Localize(best.Key)}» — {best.Value:0.0}.",
                rationale,
                confidence));

            flags.Add(new FlagFinding(
                "EDU-PROFILE-MISMATCH",
                FlagSeverity.Yellow,
                "Невідповідність обраного профілю",
                "Учень демонструє суттєво кращі результати поза межами обраного профілю.",
                Agency.Education,
                [AudienceRole.ClassTeacher, AudienceRole.Student],
                ["Провести профорієнтаційну консультацію"]));
        }
    }

    private static double Confidence(IReadOnlyDictionary<ProfileArea, double> areaAverages)
    {
        var sorted = areaAverages.Values.OrderByDescending(value => value).ToList();
        if (sorted.Count < 2)
        {
            return 0.7;
        }

        var margin = sorted[0] - sorted[1];
        return Math.Clamp(0.5 + (margin / 12.0), 0.5, 0.95);
    }

    private static string BuildAreaRationale(IReadOnlyDictionary<ProfileArea, double> areaAverages) =>
        "Середні бали за напрямами: " +
        string.Join("; ", areaAverages
            .OrderByDescending(kv => kv.Value)
            .Select(kv => $"{SubjectProfileMap.Localize(kv.Key)} — {kv.Value:0.0}")) + ".";
}
