using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Education.Domain.Entities;
using ChildRights.Education.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Infrastructure.Persistence;

/// <summary>
/// Seeds a large, deterministic demo dataset for the 2027 profile-education reform:
/// six institutions of different types across several communities and regions, each with
/// 100+ pupils distributed over classes, with realistic topic-level grades, attendance and
/// self-reported desired profiles.
///
/// The data is generated from a fixed RNG seed (and deterministic GUIDs) so every run
/// produces exactly the same dataset. It is shaped to exercise the analysis engine:
///   • each pupil follows a subject/topic <i>archetype</i> that maps to a profile cluster;
///   • ~22% of pupils desire a different cluster than their grades suggest → profile-mismatch flags;
///   • ~12% of pupils have elevated unexcused absences → attendance red flags.
///
/// Four named pupils keep their exact scripted scenarios (used by scripts/demo.ps1):
///   • Oleh   — 12 unexcused absences (attendance red flag); desires ІТ.
///   • Mariia — desires "Суспільно-гуманітарний", but peaks on finance topics → business mismatch.
///   • Andrii — strong life sciences, desires Медичний/Аграрний (matches).
///   • Sofiia — grade 9, languages-leaning → profile-choice recommendation.
/// </summary>
internal sealed class EducationDataSeeder(EducationDbContext context) : IDataSeeder
{
    // Fixed institution GUIDs (the first three keep their original demo identifiers).
    private static readonly Guid LyceumId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid CollegeId = Guid.Parse("11111111-1111-1111-1111-111111111112");
    private static readonly Guid GymnasiumId = Guid.Parse("11111111-1111-1111-1111-111111111113");
    private static readonly Guid ScientificLyceumId = Guid.Parse("11111111-1111-1111-1111-111111111114");
    private static readonly Guid ProfLyceumId = Guid.Parse("11111111-1111-1111-1111-111111111115");
    private static readonly Guid BusinessCollegeId = Guid.Parse("11111111-1111-1111-1111-111111111116");

    // The lyceum's grade-9 / grade-10 "А" classes keep their original identifiers.
    private static readonly Guid Class9 = Guid.Parse("22222222-2222-2222-2222-222222220009");
    private static readonly Guid Class10 = Guid.Parse("22222222-2222-2222-2222-222222220010");

    private static readonly Guid OlehId = Guid.Parse("33333333-3333-3333-3333-333333331001");
    private static readonly Guid MariiaId = Guid.Parse("33333333-3333-3333-3333-333333331002");
    private static readonly Guid SofiiaId = Guid.Parse("33333333-3333-3333-3333-333333331003");
    private static readonly Guid AndriiId = Guid.Parse("33333333-3333-3333-3333-333333331004");

    private readonly Random _rng = new(20270901);
    private int _classSeq;
    private int _studentSeq;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await context.Schools.AnyAsync(cancellationToken))
        {
            return;
        }

        // Seeding inserts ~10k rows; disabling change detection keeps it fast.
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            foreach (var plan in BuildSchoolPlans())
            {
                SeedSchool(plan);
            }

            SeedNamedDemoPupils();

            await context.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }

    // ----------------------------------------------------------------------------------
    // Institutions
    // ----------------------------------------------------------------------------------

    private IReadOnlyList<SchoolPlan> BuildSchoolPlans() =>
    [
        new(LyceumId, "Академічний ліцей №5", "Самарська громада", "Дніпропетровська область",
            InstitutionType.AcademicLyceum,
            [EducationProfile.NaturalMathematical, EducationProfile.SocialHumanitarian],
            [9, 10, 11], ClassesPerGrade: 2,
            [("stem", 4), ("law", 3), ("medical", 2), ("it", 1)]),

        new(CollegeId, "Фаховий коледж №2", "Самарська громада", "Дніпропетровська область",
            InstitutionType.ProfessionalCollege,
            [EducationProfile.InformationTechnology, EducationProfile.BusinessAdministration, EducationProfile.EngineeringTechnological],
            [10, 11], ClassesPerGrade: 3,
            [("it", 4), ("business", 3), ("engineering", 3)]),

        new(GymnasiumId, "Гімназія №1", "Самарська громада", "Дніпропетровська область",
            InstitutionType.Gymnasium,
            [],
            [8, 9], ClassesPerGrade: 3,
            [("stem", 2), ("law", 2), ("medical", 2), ("it", 2), ("business", 1), ("languages", 1)]),

        new(ScientificLyceumId, "Науковий ліцей «Перспектива»", "Кам'янська громада", "Дніпропетровська область",
            InstitutionType.ScientificLyceum,
            [EducationProfile.NaturalMathematical, EducationProfile.SocialHumanitarian],
            [9, 10, 11], ClassesPerGrade: 2,
            [("stem", 5), ("medical", 3), ("it", 2)]),

        new(ProfLyceumId, "Професійний ліцей №7", "Нікопольська громада", "Дніпропетровська область",
            InstitutionType.ProfessionalLyceum,
            [EducationProfile.InformationTechnology, EducationProfile.Construction, EducationProfile.TransportLogistics],
            [10, 11], ClassesPerGrade: 3,
            [("engineering", 4), ("it", 3), ("business", 1)]),

        new(BusinessCollegeId, "Фаховий коледж №3 «Бізнес та сервіс»", "Львівська громада", "Львівська область",
            InstitutionType.ProfessionalCollege,
            [EducationProfile.BusinessAdministration, EducationProfile.HospitalityEvents, EducationProfile.BeautyServicesDesign],
            [10, 11], ClassesPerGrade: 3,
            [("business", 4), ("languages", 3), ("medical", 1)])
    ];

    private void SeedSchool(SchoolPlan plan)
    {
        var school = new School(plan.Id, plan.Name, plan.Community, plan.Region, plan.Type);
        foreach (var profile in plan.OfferedProfiles)
        {
            school.OfferProfile(profile);
        }

        context.Schools.Add(school);

        foreach (var grade in plan.GradeLevels)
        {
            for (var c = 0; c < plan.ClassesPerGrade; c++)
            {
                var letter = ClassLetters[c];
                var (classId, isNamed) = ResolveClassId(plan.Id, grade, c);

                context.Classes.Add(new SchoolClass(
                    classId, plan.Id, $"{grade}-{letter}", grade, PickTeacher()));

                // The two named lyceum classes are topped up after the named pupils are added.
                var count = isNamed ? StudentsPerClass - 2 : StudentsPerClass;
                for (var s = 0; s < count; s++)
                {
                    SeedPupil(plan, classId, grade);
                }
            }
        }
    }

    private (Guid ClassId, bool IsNamed) ResolveClassId(Guid schoolId, int grade, int classIndex)
    {
        if (schoolId == LyceumId && classIndex == 0 && grade == 9)
        {
            return (Class9, true);
        }

        if (schoolId == LyceumId && classIndex == 0 && grade == 10)
        {
            return (Class10, true);
        }

        return (Guid.Parse($"cccccccc-0000-0000-0000-{++_classSeq:D12}"), false);
    }

    // ----------------------------------------------------------------------------------
    // Pupils (procedural)
    // ----------------------------------------------------------------------------------

    private void SeedPupil(SchoolPlan plan, Guid classId, int grade)
    {
        var studentId = Guid.Parse($"55555555-0000-0000-0000-{++_studentSeq:D12}");
        var archetype = Archetypes[WeightedPick(plan.ArchetypeWeights)];

        // Desired profiles: usually the archetype cluster, sometimes a deliberate mismatch.
        var desired = _rng.NextDouble() < 0.22
            ? new[] { MismatchProfile(archetype.Cluster) }
            : archetype.Desired;

        // Declared profile (grade 10+) is one the institution actually offers.
        EducationProfile? declared = grade >= 10 && plan.OfferedProfiles.Count > 0
            ? PickDeclared(plan.OfferedProfiles, archetype.Cluster)
            : null;

        var pupil = new Student(
            studentId, PickName(), BirthDate(grade), plan.Id, classId, grade, declared, desired);
        context.Students.Add(pupil);

        SeedGrades(studentId, archetype);
        SeedAttendance(studentId);
    }

    private void SeedGrades(Guid studentId, Archetype archetype)
    {
        // Strong topics dominate (graded 10–12) — these steer the recommended cluster.
        foreach (var (subject, topic) in archetype.StrongTopics)
        {
            var entries = _rng.Next(2, 4);
            for (var i = 0; i < entries; i++)
            {
                AddGrade(studentId, subject, topic, _rng.Next(10, 13));
            }
        }

        foreach (var subject in archetype.StrongSubjects)
        {
            AddGrade(studentId, subject, null, _rng.Next(9, 12));
        }

        foreach (var subject in archetype.WeakSubjects)
        {
            AddGrade(studentId, subject, null, _rng.Next(5, 9));
        }

        // Everyone has a general language grade (realistic + feeds humanities signal mildly).
        AddGrade(studentId, "Українська мова", null, _rng.Next(7, 11));
    }

    private void SeedAttendance(Guid studentId)
    {
        var start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-45);
        var atRisk = _rng.NextDouble() < 0.12;
        var unexcused = atRisk ? _rng.Next(10, 23) : _rng.Next(0, 3);
        var present = _rng.Next(4, 9);
        var day = 0;

        for (var i = 0; i < unexcused; i++)
        {
            context.AttendanceRecords.Add(new AttendanceRecord(
                Guid.NewGuid(), studentId, start.AddDays(day++), AttendanceStatus.Unexcused, null));
        }

        for (var i = 0; i < present; i++)
        {
            context.AttendanceRecords.Add(new AttendanceRecord(
                Guid.NewGuid(), studentId, start.AddDays(day++), AttendanceStatus.Present, null));
        }
    }

    // ----------------------------------------------------------------------------------
    // Named demo pupils (kept exactly for scripts/demo.ps1)
    // ----------------------------------------------------------------------------------

    private void SeedNamedDemoPupils()
    {
        var oleh = new Student(OlehId, "Петренко Олег", new DateOnly(2011, 4, 12), LyceumId, Class9, 9,
            desiredProfiles: [EducationProfile.InformationTechnology]);
        var mariia = new Student(MariiaId, "Іваненко Марія", new DateOnly(2010, 9, 3), LyceumId, Class10, 10,
            declaredProfile: EducationProfile.SocialHumanitarian,
            desiredProfiles: [EducationProfile.SocialHumanitarian]);
        var sofiia = new Student(SofiiaId, "Коваль Софія", new DateOnly(2011, 1, 25), LyceumId, Class9, 9,
            desiredProfiles: [EducationProfile.SocialHumanitarian]);
        var andrii = new Student(AndriiId, "Бондар Андрій", new DateOnly(2010, 6, 18), LyceumId, Class10, 10,
            declaredProfile: EducationProfile.NaturalMathematical,
            desiredProfiles: [EducationProfile.Medical, EducationProfile.Agricultural]);

        context.Students.AddRange(oleh, mariia, sofiia, andrii);

        // Oleh crosses the attendance red-flag threshold; the others attend regularly.
        var start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-40);
        for (var i = 0; i < 12; i++)
        {
            context.AttendanceRecords.Add(new AttendanceRecord(
                Guid.NewGuid(), OlehId, start.AddDays(i * 2), AttendanceStatus.Unexcused, null));
        }

        foreach (var id in new[] { MariiaId, SofiiaId, AndriiId })
        {
            for (var i = 0; i < 6; i++)
            {
                context.AttendanceRecords.Add(new AttendanceRecord(
                    Guid.NewGuid(), id, start.AddDays(i), AttendanceStatus.Present, null));
            }
        }

        // Mariia: declares humanities, but peaks on finance/economics topics inside law.
        AddGrade(MariiaId, "Правознавство", "Фінансове право", 11);
        AddGrade(MariiaId, "Правознавство", "Фінансове право", 12);
        AddGrade(MariiaId, "Правознавство", "Господарське право", 12);
        AddGrade(MariiaId, "Правознавство", "Конституційне право", 7);
        AddGrade(MariiaId, "Економіка", "Фінанси та інвестиції", 12);
        AddGrade(MariiaId, "Економіка", "Мікроекономіка", 11);
        AddGrade(MariiaId, "Українська література", "Поезія XX століття", 7);

        // Andrii: strong life sciences → medical fit.
        AddGrade(AndriiId, "Біологія", "Генетика", 12);
        AddGrade(AndriiId, "Біологія", "Анатомія людини", 11);
        AddGrade(AndriiId, "Хімія", "Органічна хімія", 11);
        AddGrade(AndriiId, "Математика", "Геометрія", 8);

        // Oleh: capable in IT topics, weaker maths; attendance is his risk.
        AddGrade(OlehId, "Інформатика", "Програмування", 10);
        AddGrade(OlehId, "Інформатика", "Алгоритми", 9);
        AddGrade(OlehId, "Математика", "Алгебра", 6);

        // Sofiia: languages-leaning, grade 9.
        AddGrade(SofiiaId, "Англійська мова", "Комунікації", 12);
        AddGrade(SofiiaId, "Українська література", null, 11);
        AddGrade(SofiiaId, "Математика", null, 8);
    }

    // ----------------------------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------------------------

    private void AddGrade(Guid studentId, string subject, string? topic, int value) =>
        context.Grades.Add(new Grade(
            Guid.NewGuid(), studentId, subject, value, "I семестр",
            DateOnly.FromDateTime(DateTime.UtcNow), topic));

    private EducationProfile PickDeclared(IReadOnlyList<EducationProfile> offered, ProfileCluster cluster)
    {
        var matching = offered.FirstOrDefault(p => ProfileTaxonomy.ClusterOf(p) == cluster);
        return matching != default && offered.Contains(matching) ? matching : offered[_rng.Next(offered.Count)];
    }

    private EducationProfile MismatchProfile(ProfileCluster cluster)
    {
        // A profile from any cluster other than the pupil's data-driven one.
        var pool = ProfileTaxonomy.All.Where(p => ProfileTaxonomy.ClusterOf(p) != cluster).ToList();
        return pool[_rng.Next(pool.Count)];
    }

    private string PickName() =>
        $"{Surnames[_rng.Next(Surnames.Length)]} {FirstNames[_rng.Next(FirstNames.Length)]}";

    private string PickTeacher() =>
        $"{Surnames[_rng.Next(Surnames.Length)]} {Initials[_rng.Next(Initials.Length)]}";

    private DateOnly BirthDate(int grade)
    {
        var year = 2026 - 6 - grade;
        return new DateOnly(year, _rng.Next(1, 13), _rng.Next(1, 28));
    }

    private string WeightedPick(IReadOnlyList<(string Key, int Weight)> weights)
    {
        var total = weights.Sum(w => w.Weight);
        var roll = _rng.Next(total);
        var acc = 0;
        foreach (var (key, weight) in weights)
        {
            acc += weight;
            if (roll < acc)
            {
                return key;
            }
        }

        return weights[0].Key;
    }

    // ----------------------------------------------------------------------------------
    // Static data
    // ----------------------------------------------------------------------------------

    private const int StudentsPerClass = 18;

    private static readonly string[] ClassLetters = ["А", "Б", "В", "Г", "Д"];

    private static readonly string[] FirstNames =
    [
        "Олег", "Андрій", "Іван", "Дмитро", "Максим", "Назар", "Богдан", "Артем", "Денис", "Тарас",
        "Марія", "Софія", "Олена", "Анна", "Катерина", "Наталія", "Ірина", "Юлія", "Вікторія", "Дарина"
    ];

    private static readonly string[] Surnames =
    [
        "Петренко", "Іваненко", "Коваленко", "Шевченко", "Бондаренко", "Мельниченко", "Ткаченко",
        "Кравченко", "Савченко", "Марченко", "Гриценко", "Лисенко", "Назаренко", "Романенко",
        "Павленко", "Сидоренко", "Зінченко", "Левченко", "Дяченко", "Тимошенко"
    ];

    private static readonly string[] Initials =
    [
        "Н. П.", "О. І.", "С. В.", "Л. М.", "Т. О.", "В. А.", "Г. С.", "І. П.", "М. В.", "К. О."
    ];

    private sealed record SchoolPlan(
        Guid Id,
        string Name,
        string Community,
        string Region,
        InstitutionType Type,
        IReadOnlyList<EducationProfile> OfferedProfiles,
        IReadOnlyList<int> GradeLevels,
        int ClassesPerGrade,
        IReadOnlyList<(string Key, int Weight)> ArchetypeWeights);

    private sealed record Archetype(
        ProfileCluster Cluster,
        EducationProfile[] Desired,
        (string Subject, string Topic)[] StrongTopics,
        string[] StrongSubjects,
        string[] WeakSubjects);

    // Archetypes use the exact subject/topic strings the analysis engine maps to profiles.
    private static readonly Dictionary<string, Archetype> Archetypes = new()
    {
        ["it"] = new Archetype(
            ProfileCluster.ProfessionalInformationTechnology,
            [EducationProfile.InformationTechnology],
            [("Інформатика", "Програмування"), ("Інформатика", "Алгоритми"), ("Математика", "Алгебра")],
            ["Фізика"],
            ["Українська література", "Біологія"]),

        ["medical"] = new Archetype(
            ProfileCluster.ProfessionalLifeSciences,
            [EducationProfile.Medical, EducationProfile.Agricultural],
            [("Біологія", "Генетика"), ("Біологія", "Анатомія людини"), ("Хімія", "Органічна хімія")],
            [],
            ["Математика", "Фізика"]),

        ["business"] = new Archetype(
            ProfileCluster.ProfessionalBusinessServices,
            [EducationProfile.BusinessAdministration],
            [("Економіка", "Фінанси та інвестиції"), ("Економіка", "Мікроекономіка"), ("Правознавство", "Фінансове право")],
            ["Англійська мова", "Математика"],
            ["Фізика", "Біологія"]),

        ["law"] = new Archetype(
            ProfileCluster.AcademicSocialHumanitarian,
            [EducationProfile.SocialHumanitarian],
            [("Правознавство", "Конституційне право"), ("Правознавство", "Кримінальне право")],
            ["Історія", "Українська література"],
            ["Математика", "Хімія"]),

        ["engineering"] = new Archetype(
            ProfileCluster.ProfessionalTechnical,
            [EducationProfile.EngineeringTechnological, EducationProfile.Construction],
            [("Технології", "Креслення"), ("Технології", "Будівельні конструкції"), ("Математика", "Геометрія")],
            ["Фізика"],
            ["Українська література", "Біологія"]),

        ["stem"] = new Archetype(
            ProfileCluster.AcademicNaturalMathematical,
            [EducationProfile.NaturalMathematical],
            [("Математика", "Алгебра"), ("Математика", "Геометрія")],
            ["Фізика", "Хімія", "Біологія"],
            ["Українська література"]),

        ["languages"] = new Archetype(
            ProfileCluster.ProfessionalBusinessServices,
            [EducationProfile.HospitalityEvents, EducationProfile.BeautyServicesDesign],
            [("Англійська мова", "Комунікації")],
            ["Українська література", "Географія"],
            ["Фізика", "Математика"])
    };
}
