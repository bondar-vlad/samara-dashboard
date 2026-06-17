using ChildRights.BuildingBlocks.Domain.Primitives;

namespace ChildRights.Analysis.Domain.Entities;

/// <summary>
/// A pupil's explicit interest in a specific university programme (спеціальність).
/// Aggregating these — <b>depersonalised</b> — gives universities and communities a demand
/// signal for each specialty. Drives the "send desired specialties to the university" flow.
/// </summary>
public sealed class ProgramInterest : Entity
{
    private ProgramInterest()
    {
    }

    public ProgramInterest(
        Guid id,
        Guid studentId,
        Guid schoolId,
        Guid universityId,
        string universityName,
        Guid programId,
        string programName,
        DateTime createdUtc)
        : base(id)
    {
        StudentId = studentId;
        SchoolId = schoolId;
        UniversityId = universityId;
        UniversityName = universityName;
        ProgramId = programId;
        ProgramName = programName;
        CreatedUtc = createdUtc;
    }

    public Guid StudentId { get; private set; }

    public Guid SchoolId { get; private set; }

    public Guid UniversityId { get; private set; }

    public string UniversityName { get; private set; } = string.Empty;

    public Guid ProgramId { get; private set; }

    public string ProgramName { get; private set; } = string.Empty;

    public DateTime CreatedUtc { get; private set; }
}
