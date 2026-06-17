using ChildRights.BuildingBlocks.Domain.Primitives;

namespace ChildRights.Education.Domain.Entities;

/// <summary>A subject grade on the Ukrainian 1–12 scale.</summary>
public sealed class Grade : Entity
{
    private Grade()
    {
    }

    public Grade(Guid id, Guid studentId, string subject, int value, string term, DateOnly recordedOn)
        : base(id)
    {
        StudentId = studentId;
        Subject = subject;
        Value = value;
        Term = term;
        RecordedOn = recordedOn;
    }

    public Guid StudentId { get; private set; }

    public string Subject { get; private set; } = string.Empty;

    /// <summary>Grade value, 1–12.</summary>
    public int Value { get; private set; }

    public string Term { get; private set; } = string.Empty;

    public DateOnly RecordedOn { get; private set; }
}
