using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.Education.Domain.Entities;
using ChildRights.Education.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Infrastructure.Persistence;

/// <summary>
/// Seeds a small, deterministic demo dataset so the dashboard shows meaningful
/// red flags and recommendations out of the box.
/// Scenario:
///   • Oleh — 12 unexcused absences  → attendance red flag (escalation to parents/admin).
///   • Mariia — declared "Mathematics" profile but weak in maths, strong in philology
///     → profiling "change profile" recommendation.
///   • Andrii — strong sciences, confirms his Natural Sciences profile.
/// </summary>
internal sealed class EducationDataSeeder(EducationDbContext context) : IDataSeeder
{
    private static readonly Guid SchoolId = Guid.Parse("11111111-1111-1111-1111-111111111111");
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

        context.Schools.Add(new School(SchoolId, "Ліцей №5", "Самарська громада", "Дніпропетровська область"));

        context.Classes.AddRange(
            new SchoolClass(Class9, SchoolId, "9-А", 9, "Коваленко Н. П."),
            new SchoolClass(Class10, SchoolId, "10-А", 10, "Шевченко О. І."));

        var oleh = new Student(OlehId, "Петренко Олег", new DateOnly(2010, 4, 12), SchoolId, Class9, 9);
        var mariia = new Student(MariiaId, "Іваненко Марія", new DateOnly(2009, 9, 3), SchoolId, Class10, 10, EducationProfile.Mathematics);
        var sofiia = new Student(SofiiaId, "Коваль Софія", new DateOnly(2010, 1, 25), SchoolId, Class9, 9);
        var andrii = new Student(AndriiId, "Бондар Андрій", new DateOnly(2009, 6, 18), SchoolId, Class10, 10, EducationProfile.NaturalSciences);

        context.Students.AddRange(oleh, mariia, sofiia, andrii);

        var start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-40);

        // Oleh crosses the attendance red-flag threshold.
        for (var i = 0; i < 12; i++)
        {
            context.AttendanceRecords.Add(new AttendanceRecord(
                Guid.NewGuid(), oleh.Id, start.AddDays(i * 2), AttendanceStatus.Unexcused, null));
        }

        foreach (var student in new[] { mariia, sofiia, andrii })
        {
            for (var i = 0; i < 6; i++)
            {
                context.AttendanceRecords.Add(new AttendanceRecord(
                    Guid.NewGuid(), student.Id, start.AddDays(i), AttendanceStatus.Present, null));
            }
        }

        AddGrades(mariia.Id, "Математика", 4, 5, 4);
        AddGrades(mariia.Id, "Українська література", 11, 12, 11);
        AddGrades(mariia.Id, "Українська мова", 10, 11, 10);
        AddGrades(mariia.Id, "Історія", 10, 9, 11);

        AddGrades(andrii.Id, "Біологія", 11, 12, 11);
        AddGrades(andrii.Id, "Хімія", 10, 11, 12);
        AddGrades(andrii.Id, "Математика", 9, 8, 9);

        AddGrades(oleh.Id, "Математика", 6, 5, 6);
        AddGrades(oleh.Id, "Українська мова", 7, 6, 7);

        AddGrades(sofiia.Id, "Математика", 10, 9, 10);
        AddGrades(sofiia.Id, "Англійська мова", 11, 12, 11);

        await context.SaveChangesAsync(cancellationToken);
        return;

        void AddGrades(Guid studentId, string subject, params int[] values)
        {
            foreach (var value in values)
            {
                context.Grades.Add(new Grade(
                    Guid.NewGuid(), studentId, subject, value, "I семестр",
                    DateOnly.FromDateTime(DateTime.UtcNow)));
            }
        }
    }
}
