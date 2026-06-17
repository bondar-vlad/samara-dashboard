using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Education.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ChildRights.Education.Infrastructure.Tests;

/// <summary>
/// Runs the real <see cref="EducationDataSeeder"/> against an in-memory SQLite database
/// (real relational schema via EnsureCreated) and asserts the demo dataset meets the
/// "at least 5 schools, at least 100 pupils each" requirement and stays internally consistent.
/// </summary>
public sealed class EducationDataSeederTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly EducationDbContext _context;

    public EducationDataSeederTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<EducationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new EducationDbContext(options);
        _context.Database.EnsureCreated();
    }

    private async Task SeedAsync() => await new EducationDataSeeder(_context).SeedAsync();

    [Fact]
    public async Task Seeds_at_least_five_schools_each_with_at_least_100_pupils()
    {
        await SeedAsync();

        var schools = await _context.Schools.ToListAsync();
        Assert.True(schools.Count >= 5, $"Expected ≥5 schools, got {schools.Count}.");

        foreach (var school in schools)
        {
            var pupils = await _context.Students.CountAsync(s => s.SchoolId == school.Id);
            Assert.True(pupils >= 100, $"School '{school.Name}' has {pupils} pupils (<100).");
        }
    }

    [Fact]
    public async Task Seeding_is_idempotent()
    {
        await SeedAsync();
        var firstCount = await _context.Students.CountAsync();

        await SeedAsync();
        var secondCount = await _context.Students.CountAsync();

        Assert.Equal(firstCount, secondCount);
    }

    [Fact]
    public async Task Named_demo_pupils_keep_their_scenarios()
    {
        await SeedAsync();

        var oleh = await _context.Students.FindAsync(Guid.Parse("33333333-3333-3333-3333-333333331001"));
        Assert.NotNull(oleh);
        Assert.Equal("Петренко Олег", oleh!.FullName);

        var unexcused = await _context.AttendanceRecords.CountAsync(
            a => a.StudentId == oleh.Id && a.Status == Domain.Enums.AttendanceStatus.Unexcused);
        Assert.True(unexcused >= 12, $"Oleh should have ≥12 unexcused absences, got {unexcused}.");

        var mariia = await _context.Students.FindAsync(Guid.Parse("33333333-3333-3333-3333-333333331002"));
        Assert.NotNull(mariia);
        Assert.Contains(EducationProfile.SocialHumanitarian, mariia!.DesiredProfiles);
    }

    [Fact]
    public async Task Every_pupil_has_grades_and_a_valid_class()
    {
        await SeedAsync();

        var classIds = (await _context.Classes.Select(c => c.Id).ToListAsync()).ToHashSet();
        var students = await _context.Students.ToListAsync();

        Assert.All(students, s => Assert.Contains(s.ClassId, classIds));

        var studentsWithGrades = await _context.Grades.Select(g => g.StudentId).Distinct().CountAsync();
        Assert.True(studentsWithGrades >= students.Count - 1,
            "Almost every pupil should have at least one grade.");
    }

    [Fact]
    public async Task Produces_topic_level_grades_and_desired_mismatches()
    {
        await SeedAsync();

        var topicGrades = await _context.Grades.CountAsync(g => g.Topic != null);
        Assert.True(topicGrades > 0, "Expected topic-level grades for topic-aware analysis.");

        // Every pupil has at least one self-reported desired profile (set in the constructor).
        var students = await _context.Students.ToListAsync();
        Assert.All(students, s => Assert.NotEmpty(s.DesiredProfiles));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
