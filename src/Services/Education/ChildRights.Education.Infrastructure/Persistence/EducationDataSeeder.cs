using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Education.Domain.Entities;
using ChildRights.Education.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Infrastructure.Persistence;

/// <summary>
/// Seeds a deterministic demo dataset built around the 2027 profile-education reform.
///
/// Institutions: an academic lyceum, a professional college and a feeder gymnasium —
/// each offering the profiles its institution type permits.
///
/// Pupils (profile scenarios):
///   • Oleh   — 12 unexcused absences → attendance red flag; desires ІТ.
///   • Mariia — desires "Суспільно-гуманітарний", but her grades peak on finance/economics
///              topics inside law → analysis should recommend "Бізнес та адміністрування"
///              (desired ≠ recommended ⇒ profile-mismatch flag).
///   • Andrii — strong biology/chemistry, desires "Медичний" → recommendation matches.
///   • Sofiia — grade 9, strong languages → profile-choice recommendation for grade 10.
/// </summary>
internal sealed class EducationDataSeeder(EducationDbContext context) : IDataSeeder
{
    private static readonly Guid LyceumId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid CollegeId = Guid.Parse("11111111-1111-1111-1111-111111111112");
    private static readonly Guid GymnasiumId = Guid.Parse("11111111-1111-1111-1111-111111111113");

    private static readonly Guid Class9 = Guid.Parse("22222222-2222-2222-2222-222222220009");
    private static readonly Guid Class10 = Guid.Parse("22222222-2222-2222-2222-222222220010");

    private static readonly Guid OlehId = Guid.Parse("33333333-3333-3333-3333-333333331001");
    private static readonly Guid MariiaId = Guid.Parse("33333333-3333-3333-3333-333333331002");
    private static readonly Guid SofiiaId = Guid.Parse("33333333-3333-3333-3333-333333331003");
    private static readonly Guid AndriiId = Guid.Parse("33333333-3333-3333-3333-333333331004");

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await context.Schools.AnyAsync(cancellationToken))
        {
            return;
        }

        SeedInstitutions();
        var (oleh, mariia, sofiia, andrii) = SeedStudents();
        SeedAttendance(oleh, [mariia, sofiia, andrii]);
        SeedGrades(oleh, mariia, sofiia, andrii);

        await context.SaveChangesAsync(cancellationToken);
    }

    private void SeedInstitutions()
    {
        var lyceum = new School(LyceumId, "Академічний ліцей №5", "Самарська громада", "Дніпропетровська область", InstitutionType.AcademicLyceum);
        lyceum.OfferProfile(EducationProfile.NaturalMathematical);
        lyceum.OfferProfile(EducationProfile.SocialHumanitarian);

        var college = new School(CollegeId, "Фаховий коледж №2", "Самарська громада", "Дніпропетровська область", InstitutionType.ProfessionalCollege);
        college.OfferProfile(EducationProfile.InformationTechnology);
        college.OfferProfile(EducationProfile.BusinessAdministration);
        college.OfferProfile(EducationProfile.EngineeringTechnological);

        var gymnasium = new School(GymnasiumId, "Гімназія №1", "Самарська громада", "Дніпропетровська область", InstitutionType.Gymnasium);

        context.Schools.AddRange(lyceum, college, gymnasium);

        context.Classes.AddRange(
            new SchoolClass(Class9, LyceumId, "9-А", 9, "Коваленко Н. П."),
            new SchoolClass(Class10, LyceumId, "10-А", 10, "Шевченко О. І."));
    }

    private (Student Oleh, Student Mariia, Student Sofiia, Student Andrii) SeedStudents()
    {
        var oleh = new Student(OlehId, "Петренко Олег", new DateOnly(2010, 4, 12), LyceumId, Class9, 9,
            desiredProfiles: [EducationProfile.InformationTechnology]);

        var mariia = new Student(MariiaId, "Іваненко Марія", new DateOnly(2009, 9, 3), LyceumId, Class10, 10,
            declaredProfile: EducationProfile.SocialHumanitarian,
            desiredProfiles: [EducationProfile.SocialHumanitarian]);

        var sofiia = new Student(SofiiaId, "Коваль Софія", new DateOnly(2010, 1, 25), LyceumId, Class9, 9,
            desiredProfiles: [EducationProfile.SocialHumanitarian]);

        // Andrii wants several profiles within the professional life-sciences cluster.
        var andrii = new Student(AndriiId, "Бондар Андрій", new DateOnly(2009, 6, 18), LyceumId, Class10, 10,
            declaredProfile: EducationProfile.NaturalMathematical,
            desiredProfiles: [EducationProfile.Medical, EducationProfile.Agricultural]);

        context.Students.AddRange(oleh, mariia, sofiia, andrii);
        return (oleh, mariia, sofiia, andrii);
    }

    private void SeedAttendance(Student withRisk, IEnumerable<Student> regular)
    {
        var start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-40);

        for (var i = 0; i < 12; i++)
        {
            context.AttendanceRecords.Add(new AttendanceRecord(
                Guid.NewGuid(), withRisk.Id, start.AddDays(i * 2), AttendanceStatus.Unexcused, null));
        }

        foreach (var student in regular)
        {
            for (var i = 0; i < 6; i++)
            {
                context.AttendanceRecords.Add(new AttendanceRecord(
                    Guid.NewGuid(), student.Id, start.AddDays(i), AttendanceStatus.Present, null));
            }
        }
    }

    private void SeedGrades(Student oleh, Student mariia, Student sofiia, Student andrii)
    {
        // Mariia: declares humanities, but peaks on finance/economics topics inside law.
        AddTopic(mariia.Id, "Правознавство", "Фінансове право", 11, 12, 11);
        AddTopic(mariia.Id, "Правознавство", "Господарське право", 11, 12);
        AddTopic(mariia.Id, "Правознавство", "Конституційне право", 7, 6);
        AddTopic(mariia.Id, "Правознавство", "Кримінальне право", 6, 7);
        AddTopic(mariia.Id, "Економіка", "Фінанси та інвестиції", 12, 11, 12);
        AddTopic(mariia.Id, "Економіка", "Мікроекономіка", 11, 10);
        AddTopic(mariia.Id, "Математика", "Алгебра", 10, 11);
        AddTopic(mariia.Id, "Українська література", "Поезія XX століття", 7, 8);

        // Andrii: strong life sciences → medical fit.
        AddTopic(andrii.Id, "Біологія", "Генетика", 12, 11, 12);
        AddTopic(andrii.Id, "Біологія", "Анатомія людини", 11, 12);
        AddTopic(andrii.Id, "Хімія", "Органічна хімія", 11, 10, 11);
        AddTopic(andrii.Id, "Математика", "Геометрія", 8, 9);

        // Oleh: capable in IT topics, weaker maths; attendance is his risk.
        AddTopic(oleh.Id, "Інформатика", "Програмування", 10, 9, 10);
        AddTopic(oleh.Id, "Математика", "Алгебра", 6, 5);

        // Sofiia: languages-leaning, grade 9.
        AddTopic(sofiia.Id, "Англійська мова", "Граматика", 11, 12);
        AddTopic(sofiia.Id, "Українська література", "Проза", 11, 10);
        AddTopic(sofiia.Id, "Математика", "Алгебра", 9, 10);
    }

    private void AddTopic(Guid studentId, string subject, string topic, params int[] values)
    {
        foreach (var value in values)
        {
            context.Grades.Add(new Grade(
                Guid.NewGuid(), studentId, subject, value, "I семестр",
                DateOnly.FromDateTime(DateTime.UtcNow), topic));
        }
    }
}
