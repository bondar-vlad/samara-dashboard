using ChildRights.BuildingBlocks.Domain.Primitives;

namespace ChildRights.Education.Domain.Entities;

/// <summary>A subject grade on the Ukrainian 1–12 scale, optionally tied to a curriculum topic.</summary>
public sealed class Grade : Entity
{
    private Grade()
    {
    }

    public Grade(Guid id, Guid studentId, string subject, int value, string term, DateOnly recordedOn, string? topic = null)
        : base(id)
    {
        StudentId = studentId;
        Subject = subject;
        Value = value;
        Term = term;
        RecordedOn = recordedOn;
        Topic = topic;
    }

    public Guid StudentId { get; private set; }

    public string Subject { get; private set; } = string.Empty;

    /// <summary>The curriculum topic the grade was awarded for (e.g. "Фінансове право"). Enables topic-level analysis.</summary>
    public string? Topic { get; private set; }

    /// <summary>Grade value, 1–12.</summary>
    public int Value { get; private set; }

    public string Term { get; private set; } = string.Empty;

    public DateOnly RecordedOn { get; private set; }
}
