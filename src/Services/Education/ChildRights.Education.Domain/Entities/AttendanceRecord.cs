using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.Education.Domain.Enums;

namespace ChildRights.Education.Domain.Entities;

/// <summary>A single attendance entry for a pupil on a given date.</summary>
public sealed class AttendanceRecord : Entity
{
    private AttendanceRecord()
    {
    }

    public AttendanceRecord(Guid id, Guid studentId, DateOnly date, AttendanceStatus status, string? subject)
        : base(id)
    {
        StudentId = studentId;
        Date = date;
        Status = status;
        Subject = subject;
    }

    public Guid StudentId { get; private set; }

    public DateOnly Date { get; private set; }

    public AttendanceStatus Status { get; private set; }

    public string? Subject { get; private set; }
}
