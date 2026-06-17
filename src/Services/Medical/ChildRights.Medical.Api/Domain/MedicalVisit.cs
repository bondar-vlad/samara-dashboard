using ChildRights.BuildingBlocks.Domain.Primitives;

namespace ChildRights.Medical.Api.Domain;

/// <summary>A single recorded visit/medical event for a pupil.</summary>
public sealed class MedicalVisit : Entity
{
    private MedicalVisit()
    {
    }

    public MedicalVisit(Guid id, Guid studentId, string studentName, string conditionCategory, DateOnly date, string? note)
        : base(id)
    {
        StudentId = studentId;
        StudentName = studentName;
        ConditionCategory = conditionCategory;
        Date = date;
        Note = note;
    }

    public Guid StudentId { get; private set; }

    public string StudentName { get; private set; } = string.Empty;

    public string ConditionCategory { get; private set; } = string.Empty;

    public DateOnly Date { get; private set; }

    public string? Note { get; private set; }
}
