using ChildRights.BuildingBlocks.Domain.Primitives;

namespace ChildRights.Education.Domain.Entities;

/// <summary>A class group within a school (e.g. "9-A").</summary>
public sealed class SchoolClass : Entity
{
    private SchoolClass()
    {
    }

    public SchoolClass(Guid id, Guid schoolId, string name, int gradeLevel, string classTeacher)
        : base(id)
    {
        SchoolId = schoolId;
        Name = name;
        GradeLevel = gradeLevel;
        ClassTeacher = classTeacher;
    }

    public Guid SchoolId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int GradeLevel { get; private set; }

    public string ClassTeacher { get; private set; } = string.Empty;
}
