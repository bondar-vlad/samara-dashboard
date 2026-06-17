using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.Education.Domain.Enums;

namespace ChildRights.Education.Domain.Entities;

/// <summary>
/// A pupil — the aggregate root of the Education bounded context.
/// </summary>
public sealed class Student : AggregateRoot
{
    private Student()
    {
    }

    public Student(
        Guid id,
        string fullName,
        DateOnly dateOfBirth,
        Guid schoolId,
        Guid classId,
        int gradeLevel,
        EducationProfile? declaredProfile = null)
        : base(id)
    {
        FullName = fullName;
        DateOfBirth = dateOfBirth;
        SchoolId = schoolId;
        ClassId = classId;
        GradeLevel = gradeLevel;
        DeclaredProfile = declaredProfile;
    }

    public string FullName { get; private set; } = string.Empty;

    public DateOnly DateOfBirth { get; private set; }

    public Guid SchoolId { get; private set; }

    public Guid ClassId { get; private set; }

    public int GradeLevel { get; private set; }

    /// <summary>The specialisation the pupil currently follows (grade 10+), if any.</summary>
    public EducationProfile? DeclaredProfile { get; private set; }

    public void DeclareProfile(EducationProfile profile) => DeclaredProfile = profile;
}
